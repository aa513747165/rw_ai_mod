using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    // 支柱二·公会狩猎委托（mmt-class-design.md v2，架构参照 Isekai 公会任务）：
    // GameComponent 定时器按间隔生成委托（不走 QuestScriptDef 节点脚本，Quest.MakeRaw 手工拼）；
    // F-D 级本地图边缘刷怪狩猎，B-A 级世界站点群猎（3-8 只），S-SSS 级站点单 Boss；
    // 据点联动：世界委托优先挂载扫描仪已发现未清剿的据点（清剿即完成，战利品+报酬双份收益）；
    // 无既有据点时按 guildQuestDungeonChance 概率直建新副本（DungeonSiteGen.GenerateRandomSite，与扫描仪同池同公式，段位映射档位），否则回退自建狩猎场；
    // 奖励：参与者均分历练 XP + 白银空投；B 级以上附能量结晶，A 级以上附职业凭证（凭证的正式游戏内来源）；
    // 完成检测走 QuestPart 节流 tick + 目标列表，发奖布尔防重复；总开关 MMTSettings.TalentQuestsEnabled。

    /// <summary>委托段位（0=F … 8=SSS）与数值表。</summary>
    public static class GuildQuestRank
    {
        public const int Count = 9;
        public static readonly string[] Names = { "F", "E", "D", "C", "B", "A", "S", "SS", "SSS" };

        /// <summary>段位对应的目标 combatPower 区间（选怪用）。</summary>
        public static readonly (float min, float max)[] PowerBand =
        {
            (0f, 40f), (35f, 60f), (55f, 85f), (80f, 115f), (110f, 155f),
            (150f, 230f), (220f, 320f), (300f, 620f), (600f, 100000f),
        };

        /// <summary>按殖民地历练水平 + 财富定委托段位。</summary>
        public static int ForColony(Map homeMap)
        {
            float maxLevel = 1f;
            float sum = 0f;
            int n = 0;
            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
            {
                CompPawnProgress comp = CompPawnProgress.Get(pawn);
                float lv = comp?.level ?? 1f;
                maxLevel = Mathf.Max(maxLevel, lv);
                sum += lv;
                n++;
            }
            float effective = Mathf.Max(n > 0 ? sum / n : 1f, maxLevel * 0.75f);
            int maxRank = effective >= 45f ? 8 : effective >= 38f ? 7 : effective >= 30f ? 6
                : effective >= 24f ? 5 : effective >= 18f ? 4 : effective >= 12f ? 3
                : effective >= 7f ? 2 : effective >= 4f ? 1 : 0;
            // 财富加权：每 25 万财富 +1 上限（至多 +2）
            float wealth = homeMap?.wealthWatcher?.WealthTotal ?? 0f;
            maxRank = Mathf.Min(Count - 1, maxRank + Mathf.Clamp(Mathf.FloorToInt(wealth / 250000f), 0, 2));
            // 加权随机：偏向中段位
            List<int> pool = new List<int>();
            for (int r = 0; r <= maxRank; r++)
            {
                int weight = Mathf.Max(1, 6 - Mathf.Abs(r - maxRank / 2));
                for (int i = 0; i < weight; i++)
                {
                    pool.Add(r);
                }
            }
            return pool.RandomElement();
        }

        public static bool IsWorld(int rank)
        {
            return rank >= 4;
        }

        /// <summary>目标数量：F-E 1 只，D-C 2 只，B 4 只，A 6 只，S 以上单 Boss。</summary>
        public static int TargetCount(int rank)
        {
            return rank <= 1 ? 1 : rank <= 3 ? 2 : rank == 4 ? 4 : rank == 5 ? 6 : 1;
        }

        public static float XpReward(int rank)
        {
            return 60f * (1 << rank);
        }

        public static int SilverReward(int rank)
        {
            return Mathf.RoundToInt(50f * (float)Math.Pow(1.8, rank));
        }

        /// <summary>B 级以上附能量结晶。</summary>
        public static int CrystalCount(int rank)
        {
            return rank >= 4 ? 2 + (rank - 4) : 0;
        }

        /// <summary>A 级以上附职业凭证。</summary>
        public static int TokenCount(int rank)
        {
            return rank >= 5 ? 1 : 0;
        }
    }

    /// <summary>世界狩猎站点状态（存档在 GameComponent，避免给 Site 运行时挂 comp）。</summary>
    public class GuildHuntSiteRecord : IExposable
    {
        public Site site;
        public List<PawnKindDef> kinds = new List<PawnKindDef>();
        public List<Pawn> targets = new List<Pawn>();
        public bool spawned;

        public void ExposeData()
        {
            Scribe_References.Look(ref site, "site");
            Scribe_Collections.Look(ref kinds, "kinds", LookMode.Def);
            Scribe_Collections.Look(ref targets, "targets", LookMode.Reference);
            Scribe_Values.Look(ref spawned, "spawned");
            if (kinds == null)
            {
                kinds = new List<PawnKindDef>();
            }
            if (targets == null)
            {
                targets = new List<Pawn>();
            }
        }
    }

    /// <summary>选怪与敌对化工具。</summary>
    public static class GuildQuestUtil
    {
        private static List<PawnKindDef> pool;

        private static void BuildPool()
        {
            pool = new List<PawnKindDef>();
            foreach (PawnKindDef kind in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (kind?.RaceProps == null || kind.race == null || !kind.isFighter)
                {
                    continue;
                }
                if (!kind.RaceProps.Animal && !kind.RaceProps.Insect)
                {
                    continue;
                }
                if (kind.defName.IndexOf("Dryad", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (kind.combatPower <= 0f)
                {
                    continue;
                }
                pool.Add(kind);
            }
        }

        /// <summary>按段位选怪（区间内随机；区间空时取全池 combatPower 最接近者）。</summary>
        public static PawnKindDef PickCreature(int rank)
        {
            if (pool == null)
            {
                BuildPool();
            }
            (float min, float max) = GuildQuestRank.PowerBand[Mathf.Clamp(rank, 0, GuildQuestRank.Count - 1)];
            List<PawnKindDef> inBand = pool.Where(k => k.combatPower >= min && k.combatPower < max).ToList();
            if (inBand.Count > 0)
            {
                return inBand.RandomElement();
            }
            float mid = (min + max) / 2f;
            return pool.OrderBy(k => Mathf.Abs(k.combatPower - mid)).FirstOrDefault();
        }

        /// <summary>
        /// 寻找一处可挂载委托的既有 MMT 据点（委托×据点联动，mmt-class-design.md §9）：
        /// 扫描仪据点 / Boss 巢穴等已发现未清剿站点（排除公会自建的狩猎场），
        /// 要求确有敌对威胁（敌对派系宿主或机械集群守卫）且未被其它委托占用；
        /// 多个候选取距家最近者。无合格据点返回 null（调用方回退自建狩猎场）。
        /// </summary>
        public static Site TryFindLinkedSite(Map home)
        {
            Site best = null;
            float bestDist = float.MaxValue;
            PlanetTile homeTile = home?.Tile ?? PlanetTile.Invalid;
            foreach (WorldObject wo in Find.WorldObjects.AllWorldObjects)
            {
                Site s = wo as Site;
                if (s == null || s.Destroyed || !s.Spawned)
                {
                    continue;
                }
                if (!IsMmtDungeonSite(s) || !HasHostileGuards(s))
                {
                    continue;
                }
                if (GameComponent_GuildQuests.IsSiteTargeted(s))
                {
                    continue;
                }
                float dist = homeTile.Valid && s.Tile.Valid
                    ? Find.WorldGrid.ApproxDistanceInTiles(homeTile, s.Tile)
                    : 0f;
                if (best == null || dist < bestDist)
                {
                    best = s;
                    bestDist = dist;
                }
            }
            return best;
        }

        /// <summary>是否 MMT 据点站点（含扫描仪据点、副本、Boss 巢穴；排除公会狩猎场自身）。</summary>
        private static bool IsMmtDungeonSite(Site site)
        {
            foreach (SitePart part in site.parts)
            {
                string name = part?.def?.defName;
                if (name != null && name.StartsWith("MMT_SitePart_") && name != "MMT_SitePart_GuildHunt")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>站点是否确有可讨伐的敌对力量（敌对派系宿主或机械集群守卫）。</summary>
        public static bool HasHostileGuards(Site site)
        {
            if (site.Faction != null && site.Faction.HostileTo(Faction.OfPlayer))
            {
                return true;
            }
            foreach (SitePart part in site.parts)
            {
                if (part?.def != null && part.def == DungeonSiteGen.MechClusterPart)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>刷出并敌对化（动物上永久猎杀状态；虫/机械挂对应派系）。</summary>
        public static Pawn SpawnHostile(PawnKindDef kind, IntVec3 cell, Map map)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(kind, null);
            if (kind.RaceProps.Insect)
            {
                Faction insects = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Insect);
                if (insects != null)
                {
                    pawn.SetFaction(insects);
                }
            }
            GenSpawn.Spawn(pawn, cell, map);
            if (kind.RaceProps.Animal && pawn.mindState?.mentalStateHandler != null)
            {
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            }
            return pawn;
        }
    }

    /// <summary>
    /// 狩猎委托 QuestPart：本地任务生成时立即在来源图边缘刷怪；世界任务创建站点，
    /// 站点地图生成时由 SitePartWorker 刷怪（记录在 GameComponent）。
    /// 节流 tick 检测全灭 → 发奖（done 布尔防重复）；超时信号 → 失败。
    /// </summary>
    public class QuestPart_MMTHunt : QuestPart
    {
        public int rank;
        public bool isWorld;
        public Map homeMap;
        public Site site;
        public float xpReward;
        public int silverReward;
        public int crystalCount;
        public int tokenCount;
        public bool done;
        /// <summary>据点联动模式：委托挂载在扫描仪已发现的既有据点上，清剿该据点即完成（奖励与据点战利品叠加）。</summary>
        public bool linkedSite;
        /// <summary>据点为本次委托直建（与扫描仪同池同公式的新副本）；false=挂载玩家此前扫描发现的既有据点。</summary>
        public bool generatedDungeon;
        /// <summary>联动模式：曾在据点图上观察到敌对威胁（存档；用于站点消失时区分"已清剿"与"超时消失"）。</summary>
        public bool sawHostiles;
        /// <summary>本地任务的目标（世界任务目标在站点记录里）。</summary>
        public List<Pawn> localTargets = new List<Pawn>();

        private string TimeoutSignal
        {
            get { return quest.InitiateSignal + ".Timeout"; }
        }

        /// <summary>任务开始：本地刷怪 / 世界建站。</summary>
        public void BeginHunt()
        {
            if (isWorld)
            {
                if (!linkedSite)
                {
                    CreateSite();
                }
            }
            else
            {
                SpawnLocal();
            }
            string rankName = GuildQuestRank.Names[Mathf.Clamp(rank, 0, GuildQuestRank.Count - 1)];
            string objective = !isWorld
                ? "目标生物已出现在殖民地附近，尽快猎杀。"
                : !linkedSite
                    ? "目标在世界地图的狩猎场出没，组织远征队前往讨伐。"
                    : generatedDungeon
                        ? "公会侦测到一处新据点「" + (site?.Label ?? "未知据点") + "」，悬赏清剿。清剿该据点即完成委托——据点内的战利品归你们所有，公会另付委托报酬。"
                        : "公会核实了你们扫描发现的据点「" + (site?.Label ?? "未知据点") + "」，将其列为讨伐目标。清剿该据点即完成委托——据点内的战利品归你们所有，公会另付委托报酬。";
            string text = "冒险者公会发布了 " + rankName + " 级狩猎委托。\n\n"
                + objective
                + "\n\n奖励：历练 XP " + xpReward.ToString("F0") + "（参与者均分）、白银 " + silverReward
                + (crystalCount > 0 ? "、能量结晶×" + crystalCount : "")
                + (tokenCount > 0 ? "、职业凭证×" + tokenCount : "")
                + "\n期限：" + (isWorld ? 14 : 7) + " 天。";
            Find.LetterStack.ReceiveLetter("公会狩猎委托（" + rankName + " 级）", text, LetterDefOf.NeutralEvent,
                isWorld ? (GlobalTargetInfo)site : new GlobalTargetInfo(homeMap.Center, homeMap));
        }

        private void SpawnLocal()
        {
            if (homeMap == null)
            {
                return;
            }
            int count = GuildQuestRank.TargetCount(rank);
            for (int i = 0; i < count; i++)
            {
                PawnKindDef kind = GuildQuestUtil.PickCreature(rank);
                if (kind == null)
                {
                    continue;
                }
                IntVec3 cell = CellFinder.RandomEdgeCell(homeMap);
                Pawn pawn = GuildQuestUtil.SpawnHostile(kind, cell, homeMap);
                localTargets.Add(pawn);
            }
        }

        private void CreateSite()
        {
            PlanetTile homeTile = homeMap?.Tile ?? PlanetTile.Invalid;
            if (!homeTile.Valid
                || !TileFinder.TryFindNewSiteTile(out PlanetTile tile, homeTile, 2, 8, true,
                    null, 0f, false, TileFinderMode.Random, false, false, null, null))
            {
                // 找不到站点格则退化为本地狩猎
                isWorld = false;
                SpawnLocal();
                return;
            }
            List<PawnKindDef> kinds = new List<PawnKindDef>();
            int count = GuildQuestRank.TargetCount(rank);
            for (int i = 0; i < count; i++)
            {
                PawnKindDef kind = GuildQuestUtil.PickCreature(rank);
                if (kind != null)
                {
                    kinds.Add(kind);
                }
            }
            SitePartDef partDef = DefDatabase<SitePartDef>.GetNamedSilentFail("MMT_SitePart_GuildHunt");
            if (partDef == null || kinds.Count == 0)
            {
                isWorld = false;
                SpawnLocal();
                return;
            }
            List<SitePartDefWithParams> parts = new List<SitePartDefWithParams>
            {
                new SitePartDefWithParams(partDef, new SitePartParams { threatPoints = 0f }),
            };
            site = SiteMaker.MakeSite(parts, tile, null, false, null);
            if (site == null)
            {
                isWorld = false;
                SpawnLocal();
                return;
            }
            site.customLabel = "狩猎场（" + GuildQuestRank.Names[rank] + " 级委托）";
            Find.WorldObjects.Add(site);
            site.GetComponent<TimeoutComp>()?.StartTimeout(15 * 60000);
            GameComponent_GuildQuests.Current?.RegisterSite(site, kinds);
        }

        /// <summary>完成检测（由 GameComponent 节流驱动；1.7 QuestPart 无 tick 虚方法）。</summary>
        public void CheckCompletion()
        {
            if (done)
            {
                return;
            }
            if (isWorld)
            {
                if (linkedSite)
                {
                    CheckLinkedCompletion();
                    return;
                }
                if (site == null || site.Destroyed || !site.Spawned)
                {
                    EndQuest(QuestEndOutcome.Fail, "狩猎场已消失，委托失败。");
                    return;
                }
                GuildHuntSiteRecord record = GameComponent_GuildQuests.Current?.RecordFor(site);
                if (record != null && record.spawned && record.targets.All(t => t == null || t.Dead))
                {
                    Complete(record.targets.Where(t => t != null).Select(t => t.Corpse?.PositionHeld ?? t.PositionHeld).FirstOrDefault());
                }
            }
            else if (localTargets.Count > 0 && localTargets.All(t => t == null || t.Dead))
            {
                Complete(localTargets.Where(t => t != null).Select(t => t.Corpse?.PositionHeld ?? t.PositionHeld).FirstOrDefault());
            }
        }

        /// <summary>
        /// 据点联动完成检测：据点图上有敌对威胁时标记 sawHostiles；威胁全清 → 完成；
        /// 站点消失时凭 sawHostiles 区分"玩家清剿后站点被移除"（成功）与"超时/未清剿消失"（失败）。
        /// </summary>
        private void CheckLinkedCompletion()
        {
            if (site == null || site.Destroyed || !site.Spawned)
            {
                if (sawHostiles)
                {
                    Complete(site != null && site.HasMap ? site.Map.Center : IntVec3.Invalid);
                }
                else
                {
                    EndQuest(QuestEndOutcome.Fail, "据点信号消失（超时或被他人清剿），委托失败。");
                }
                return;
            }
            if (!site.HasMap)
            {
                return;
            }
            Map map = site.Map;
            if (GenHostility.AnyHostileActiveThreatToPlayer(map))
            {
                sawHostiles = true;
            }
            else if (sawHostiles)
            {
                Complete(map.Center);
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (!done && signal.tag == TimeoutSignal)
            {
                EndQuest(QuestEndOutcome.Fail, "委托期限已过，狩猎委托失败。");
            }
        }

        private void Complete(IntVec3 dropCellHint)
        {
            // 据点联动清剿后站点可能已移除：空投与参与判定回退到殖民地
            Map awardMap = isWorld ? (site != null && site.HasMap ? site.Map : homeMap) : homeMap;
            if (!dropCellHint.IsValid)
            {
                dropCellHint = awardMap?.Center ?? IntVec3.Invalid;
            }
            // XP：战斗地图上的殖民者均分；图空则全体殖民者均分
            List<Pawn> participants = awardMap != null
                ? awardMap.mapPawns.FreeColonistsSpawned.Where(p => CompPawnProgress.Get(p) != null).ToList()
                : new List<Pawn>();
            if (participants.Count == 0)
            {
                participants = PawnsFinder.AllMaps_FreeColonists
                    .Where(p => CompPawnProgress.Get(p) != null).ToList();
            }
            if (participants.Count > 0)
            {
                float share = xpReward / participants.Count;
                foreach (Pawn pawn in participants)
                {
                    CompPawnProgress.Get(pawn)?.AddXp(share, showMote: true);
                }
            }
            // 白银 + 战利品空投
            if (awardMap != null && dropCellHint.IsValid)
            {
                List<Thing> drops = new List<Thing>();
                if (silverReward > 0)
                {
                    Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silver.stackCount = silverReward;
                    drops.Add(silver);
                }
                if (crystalCount > 0)
                {
                    Thing crystal = ThingMaker.MakeThing(MMT_ClassMentorDefOf.MMT_EnergyCrystal);
                    crystal.stackCount = crystalCount;
                    drops.Add(crystal);
                }
                for (int i = 0; i < tokenCount; i++)
                {
                    drops.Add(ThingMaker.MakeThing(MMT_ClassMentorDefOf.MMT_ClassToken));
                }
                if (drops.Count > 0)
                {
                    DropPodUtility.DropThingsNear(dropCellHint, awardMap, drops, 5, false, false, false);
                }
            }
            EndQuest(QuestEndOutcome.Success, "狩猎委托完成！奖励已空投，参与狩猎的殖民者分享了 " + xpReward.ToString("F0") + " 历练 XP。");
        }

        private void EndQuest(QuestEndOutcome result, string letterText)
        {
            if (done)
            {
                return;
            }
            done = true;
            Find.LetterStack.ReceiveLetter("公会狩猎委托", letterText,
                result == QuestEndOutcome.Success ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent);
            quest.End(result, false);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rank, "rank");
            Scribe_Values.Look(ref isWorld, "isWorld");
            Scribe_References.Look(ref homeMap, "homeMap");
            Scribe_References.Look(ref site, "site");
            Scribe_Values.Look(ref xpReward, "xpReward");
            Scribe_Values.Look(ref silverReward, "silverReward");
            Scribe_Values.Look(ref crystalCount, "crystalCount");
            Scribe_Values.Look(ref tokenCount, "tokenCount");
            Scribe_Values.Look(ref done, "done");
            Scribe_Values.Look(ref linkedSite, "linkedSite");
            Scribe_Values.Look(ref generatedDungeon, "generatedDungeon");
            Scribe_Values.Look(ref sawHostiles, "sawHostiles");
            Scribe_Collections.Look(ref localTargets, "localTargets", LookMode.Reference);
            if (localTargets == null)
            {
                localTargets = new List<Pawn>();
            }
        }
    }

    /// <summary>公会委托定时器与站点记录（间隔走设置；站点状态存档在此）。</summary>
    public class GameComponent_GuildQuests : GameComponent
    {
        public static GameComponent_GuildQuests Current
        {
            get { return Verse.Current.Game?.GetComponent<GameComponent_GuildQuests>(); }
        }

        private int lastQuestTick = -1;
        private List<GuildHuntSiteRecord> sites = new List<GuildHuntSiteRecord>();

        public GameComponent_GuildQuests(Game game)
        {
        }

        public void RegisterSite(Site site, List<PawnKindDef> kinds)
        {
            sites.Add(new GuildHuntSiteRecord { site = site, kinds = kinds });
        }

        public GuildHuntSiteRecord RecordFor(Site site)
        {
            return site == null ? null : sites.FirstOrDefault(r => r.site == site);
        }

        /// <summary>该据点是否已被进行中的委托占用（联动模式去重）。</summary>
        public static bool IsSiteTargeted(Site site)
        {
            if (site == null)
            {
                return false;
            }
            foreach (Quest quest in Find.QuestManager.QuestsListForReading)
            {
                foreach (QuestPart qp in quest.PartsListForReading)
                {
                    if (qp is QuestPart_MMTHunt hunt && !hunt.done && hunt.site == site)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 250 != 0 || !MMTSettings.TalentQuestsEnabled)
            {
                return;
            }
            int now = Find.TickManager.TicksGame;
            int interval = Mathf.RoundToInt(MMTSettings.GuildQuestIntervalDays * 60000f);
            if (lastQuestTick < 0)
            {
                // 读档/新局：半个间隔后出第一张委托
                lastQuestTick = now - interval / 2;
            }
            // 清理已销毁站点记录
            sites.RemoveAll(r => r.site == null || r.site.Destroyed);
            // 驱动进行中的委托做完成检测（QuestPart 无 tick 虚方法）
            foreach (Quest quest in Find.QuestManager.QuestsListForReading)
            {
                foreach (QuestPart qp in quest.PartsListForReading)
                {
                    if (qp is QuestPart_MMTHunt hunt)
                    {
                        hunt.CheckCompletion();
                    }
                }
            }
            if (now - lastQuestTick >= interval)
            {
                TryOfferQuest();
            }
        }

        /// <summary>生成一张狩猎委托（force 供 Debug 用）。</summary>
        public void TryOfferQuest(bool force = false)
        {
            Map home = Find.AnyPlayerHomeMap;
            if (home == null)
            {
                return;
            }
            lastQuestTick = Find.TickManager.TicksGame;
            if (!force && !MMTSettings.TalentQuestsEnabled)
            {
                return;
            }
            int rank = GuildQuestRank.ForColony(home);
            bool isWorld = GuildQuestRank.IsWorld(rank);
            // 委托×据点联动：世界委托优先挂到扫描仪已发现未清剿的据点上，清剿即双份收益
            Site linked = isWorld ? GuildQuestUtil.TryFindLinkedSite(home) : null;
            bool generatedDungeon = false;
            if (isWorld && linked == null && Rand.Chance(MMTSettings.GuildQuestDungeonChance))
            {
                // 无既有据点可挂时按概率直建新副本（与深空扫描同池同公式，段位映射档位 B→I/A→II/S+→III）
                Site dungeon = DungeonSiteGen.GenerateRandomSite(home, Mathf.Clamp(rank - 3, 1, 3));
                if (dungeon != null && GuildQuestUtil.HasHostileGuards(dungeon))
                {
                    linked = dungeon;
                    generatedDungeon = true;
                }
                else if (dungeon != null)
                {
                    // 极端情况（无 Royalty 且派系枚举为空）出了无守卫据点：无法判定清剿，销毁回退自建狩猎场
                    dungeon.Destroy();
                }
            }
            Quest quest = Quest.MakeRaw();
            string rankName = GuildQuestRank.Names[rank];
            quest.name = "公会狩猎委托（" + rankName + " 级）";
            QuestPart_MMTHunt part = new QuestPart_MMTHunt
            {
                rank = rank,
                isWorld = isWorld,
                linkedSite = linked != null,
                generatedDungeon = generatedDungeon,
                site = linked,
                homeMap = home,
                xpReward = GuildQuestRank.XpReward(rank),
                silverReward = GuildQuestRank.SilverReward(rank),
                crystalCount = GuildQuestRank.CrystalCount(rank),
                tokenCount = GuildQuestRank.TokenCount(rank),
            };
            quest.description = "冒险者公会的" + rankName + " 级狩猎委托："
                + (!isWorld ? "猎杀出现在殖民地附近的目标生物。"
                    : linked == null ? "前往世界地图的狩猎场讨伐目标。"
                    : generatedDungeon ? "清剿公会新侦测到的据点「" + linked.Label + "」。"
                    : "清剿已发现的据点「" + linked.Label + "」。");
            quest.AddPart(part);
            Find.QuestManager.Add(quest);
            // 完成期限（本地 7 天 / 世界 14 天）
            QuestPart_Delay delay = new QuestPart_Delay();
            delay.delayTicks = (isWorld ? 14 : 7) * 60000;
            delay.outSignalsCompleted.Add(quest.InitiateSignal + ".Timeout");
            quest.AddPart(delay);
            part.BeginHunt();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastQuestTick, "lastQuestTick", -1);
            Scribe_Collections.Look(ref sites, "guildHuntSites", LookMode.Deep);
            if (sites == null)
            {
                sites = new List<GuildHuntSiteRecord>();
            }
        }
    }

    /// <summary>狩猎场站点内容生成：地图生成时从 GameComponent 记录取目标清单刷怪。</summary>
    public class SitePartWorker_MMT_GuildHunt : SitePartWorker
    {
        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            Site site = map?.Parent as Site;
            GuildHuntSiteRecord record = GameComponent_GuildQuests.Current?.RecordFor(site);
            if (record == null || record.spawned || map == null)
            {
                return;
            }
            record.spawned = true;
            foreach (PawnKindDef kind in record.kinds)
            {
                IntVec3 cell = CellFinderLoose.RandomCellWith(
                    c => c.Standable(map) && !c.Fogged(map) && (c - map.Center).LengthHorizontal <= 30f, map, 100);
                if (!cell.IsValid)
                {
                    cell = map.Center;
                }
                Pawn pawn = GuildQuestUtil.SpawnHostile(kind, cell, map);
                record.targets.Add(pawn);
            }
        }
    }

    /// <summary>公会委托 Debug 工具。</summary>
    public static class GuildQuestDebugActions
    {
        [DebugAction("MMT 职业", "生成公会狩猎委托", allowedGameStates = AllowedGameStates.Playing)]
        private static void ForceGuildQuest()
        {
            GameComponent_GuildQuests.Current?.TryOfferQuest(force: true);
        }
    }
}
