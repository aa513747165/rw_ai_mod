using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 任务链「远古军械库」（mmt-rpg-design.md §3.3）：
    /// 第一幕 信号截获（触发即标注一处试炼场）→ 第二幕 试炼收集（职业凭证碎片×3）→
    /// 第三幕 守卫巢穴（生成 Boss 巢穴，清剿完成）→ 空投奖励 + 结局信，进入 30 天冷却可重复。
    /// </summary>
    public static class RpgQuestChainUtil
    {
        /// <summary>触发财富门槛（15 万）。</summary>
        public const float TriggerWealth = 150000f;

        /// <summary>扫描完成触发概率。</summary>
        public const float ScanTriggerChance = 0.20f;

        /// <summary>财富达标后的每日触发概率。</summary>
        public const float DailyWealthTriggerChance = 0.10f;

        /// <summary>完成后冷却（30 天）。</summary>
        public const int CooldownTicks = 30 * 60000;

        /// <summary>第二幕所需职业凭证碎片数。</summary>
        public const int FragmentNeed = 3;

        // 碎片计数缓存（DescriptionPart 在任务页签打开时按帧调用，必须与驱动节奏共用节流）
        private static int cachedFragCount = -1;
        private static int cachedFragTick = -1;

        /// <summary>统计玩家持有的职业凭证碎片（家园地图未禁用堆 + 殖民者背包 + 远行队货物）。250 tick 内复用缓存。</summary>
        public static int CountFragments(bool forceRefresh = false)
        {
            int now = Find.TickManager.TicksGame;
            if (!forceRefresh && cachedFragCount >= 0 && now - cachedFragTick < 250)
                return cachedFragCount;
            int total = CountFragmentsUncached();
            cachedFragCount = total;
            cachedFragTick = now;
            return total;
        }

        private static int CountFragmentsUncached()
        {
            ThingDef frag = MMT_RpgQuestDefOf.MMT_ClassTokenFragment;
            if (frag == null)
                return 0;
            int total = 0;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (!map.IsPlayerHome)
                    continue;
                List<Thing> stacks = map.listerThings.ThingsOfDef(frag);
                for (int j = 0; j < stacks.Count; j++)
                {
                    if (!stacks[j].IsForbidden(Faction.OfPlayer))
                        total += stacks[j].stackCount;
                }
                List<Pawn> colonists = map.mapPawns.FreeColonists;
                for (int j = 0; j < colonists.Count; j++)
                    total += colonists[j].inventory.innerContainer.TotalStackCountOfDef(frag);
            }
            List<Caravan> caravans = Find.WorldObjects.Caravans;
            for (int i = 0; i < caravans.Count; i++)
            {
                if (!caravans[i].IsPlayerControlled)
                    continue;
                List<Thing> items = CaravanInventoryUtility.AllInventoryItems(caravans[i]);
                for (int j = 0; j < items.Count; j++)
                {
                    if (items[j].def == frag)
                        total += items[j].stackCount;
                }
            }
            return total;
        }
    }

    /// <summary>任务链根节点：只挂自定义 QuestPart（阶段机全部在 part 内）。</summary>
    public class QuestNode_Root_MMTAncientArmory : QuestNode
    {
        protected override void RunInt()
        {
            QuestGen.quest.AddPart(new QuestPart_MMTAncientArmory());
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }

    /// <summary>
    /// 任务链阶段机 QuestPart（1.7 QuestPart 无 tick 虚方法，由 GameComponent_RpgQuestChain 节流驱动）：
    /// stage 0 = 收集碎片；stage 1 = 讨伐巢穴（sawHostiles 区分"清剿后被移除"与"超时消失"）。
    /// </summary>
    public class QuestPart_MMTAncientArmory : QuestPart
    {
        public int stage;
        public Map homeMap;
        public Site trialSite;
        public Site bossSite;
        public bool sawHostiles;

        /// <summary>第一幕：标注试炼场并发信（任务生成时由 GameComponent 调用）。</summary>
        public void Begin(Map home)
        {
            homeMap = home;
            stage = 0;
            SitePartDef trial = DefDatabase<SitePartDef>.GetNamedSilentFail("MMT_SitePart_TrainingGround");
            if (home != null && trial != null)
            {
                ScanTier tier2 = CompDeepSpaceScanner.GetTierConfig(2);
                trialSite = DungeonSiteGen.GenerateDungeonSite(home, trial, 2, 1.2f, tier2.threatFactor, "试炼场（军械库线索）");
            }
            Find.LetterStack.ReceiveLetter("远古军械库：信号截获",
                "深空扫描的数据流中隐藏着一段加密信号——一座远古军械库的坐标残片。\n\n"
                + "信号指向一处试炼场"
                + (trialSite != null ? "（已标注在世界地图）" : "")
                + "：清理副本并收集 <color=#ffd75e>职业凭证碎片 ×" + RpgQuestChainUtil.FragmentNeed + "</color>（试炼场/职业圣殿均有产出，可重复刷取），即可拼出军械库——远古守卫巢穴的完整坐标。",
                LetterDefOf.PositiveEvent,
                trialSite != null ? new LookTargets(trialSite) : LookTargets.Invalid);
        }

        /// <summary>进度推进（GameComponent 节流调用）。</summary>
        public void CheckChain()
        {
            if (quest == null || quest.State != QuestState.Ongoing)
                return;
            if (stage == 0)
            {
                if (RpgQuestChainUtil.CountFragments() >= RpgQuestChainUtil.FragmentNeed)
                    AdvanceToBossStage();
                return;
            }
            // stage 1：巢穴讨伐（站点消失时凭 sawHostiles 区分成败，同公会委托联动逻辑）
            if (bossSite == null || bossSite.Destroyed || !bossSite.Spawned)
            {
                if (sawHostiles)
                    Complete();
                else
                    Fail("巢穴信号消失（超时），坐标已失效。");
                return;
            }
            if (!bossSite.HasMap)
                return;
            if (GenHostility.AnyHostileActiveThreatToPlayer(bossSite.Map))
                sawHostiles = true;
            else if (sawHostiles)
                Complete();
        }

        /// <summary>第三幕：生成 Boss 巢穴并发信。</summary>
        private void AdvanceToBossStage()
        {
            stage = 1;
            SitePartDef lair = DefDatabase<SitePartDef>.GetNamedSilentFail("MMT_SitePart_BossLair");
            if (homeMap != null && lair != null)
            {
                ScanTier tier3 = CompDeepSpaceScanner.GetTierConfig(3);
                bossSite = DungeonSiteGen.GenerateDungeonSite(homeMap, lair, 3, 2.0f, tier3.threatFactor, "远古守卫巢穴（军械库）");
            }
            if (bossSite == null)
            {
                Fail("无法定位巢穴坐标（无可用世界格）。");
                return;
            }
            Find.LetterStack.ReceiveLetter("远古军械库：坐标拼合完成",
                "职业凭证碎片拼出了完整的坐标——远古守卫巢穴已标注在世界地图（30 天后信号消失）。\n\n巢穴深处的远古守卫者看守着军械库。清剿巢穴，取回传说装备。",
                LetterDefOf.PositiveEvent, new LookTargets(bossSite));
        }

        /// <summary>完成：空投奖励（传说装备×2 + 职业凭证 + 远古数据核心×2）+ 结局信。</summary>
        private void Complete()
        {
            Map dropMap = homeMap ?? Find.AnyPlayerHomeMap;
            if (dropMap != null)
            {
                List<Thing> rewards = new List<Thing>();
                for (int i = 0; i < 2; i++)
                {
                    ThingWithComps gear = DungeonGearGen.Generate(DungeonGearKind.Any, 3,
                        QualityCategory.Masterwork, QualityCategory.Legendary);
                    if (gear != null)
                        rewards.Add(gear);
                }
                AddReward(rewards, MMT_RpgQuestDefOf.MMT_ClassToken, 1);
                AddReward(rewards, MMT_RpgQuestDefOf.MMT_AncientDatacore, 2);
                if (rewards.Count > 0)
                {
                    IntVec3 spot = DropCellFinder.TradeDropSpot(dropMap);
                    DropPodUtility.DropThingsNear(spot, dropMap, rewards, 110, false, true, true);
                }
            }
            Find.LetterStack.ReceiveLetter("远古军械库：军械到手",
                "巢穴已清剿完毕。军械库的传说装备、职业凭证与远古数据核心已通过空投送达殖民地。\n\n（任务链进入 30 天冷却，之后可再次触发。）",
                LetterDefOf.PositiveEvent,
                dropMap != null ? new LookTargets(DropCellFinder.TradeDropSpot(dropMap), dropMap) : LookTargets.Invalid);
            EndChain(QuestEndOutcome.Success);
        }

        private static void AddReward(List<Thing> rewards, ThingDef def, int count)
        {
            if (def == null)
                return;
            Thing t = ThingMaker.MakeThing(def);
            t.stackCount = count;
            rewards.Add(t);
        }

        private void Fail(string reason)
        {
            Find.LetterStack.ReceiveLetter("远古军械库：任务失败", reason + "\n\n（任务链进入 30 天冷却，之后可再次触发。）",
                LetterDefOf.NeutralEvent, LookTargets.Invalid);
            EndChain(QuestEndOutcome.Fail);
        }

        private void EndChain(QuestEndOutcome outcome)
        {
            GameComponent_RpgQuestChain.Current?.NotifyChainEnded();
            quest?.End(outcome, false, true);
        }

        public override string DescriptionPart
        {
            get
            {
                if (stage == 0)
                    return "收集职业凭证碎片：" + Mathf.Min(RpgQuestChainUtil.CountFragments(), RpgQuestChainUtil.FragmentNeed)
                        + "/" + RpgQuestChainUtil.FragmentNeed + "（试炼场/职业圣殿产出）";
                return "清剿远古守卫巢穴" + (bossSite != null && bossSite.Spawned ? "（已在世界地图标注）" : "");
            }
        }

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                Site s = stage == 0 ? trialSite : bossSite;
                if (s != null && s.Spawned)
                    yield return s;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stage, "stage", 0);
            Scribe_Values.Look(ref sawHostiles, "sawHostiles", false);
            Scribe_References.Look(ref homeMap, "homeMap");
            Scribe_References.Look(ref trialSite, "trialSite");
            Scribe_References.Look(ref bossSite, "bossSite");
        }
    }

    /// <summary>
    /// 任务链「远古军械库」触发与节流（GameComponent 自动实例化）：
    /// 入口双门控（rpgQuestsEnabled + bossEncountersEnabled）、财富 15 万、30 天冷却；
    /// 触发途径：深空扫描完成（20%）/ 财富达标每日判定（10%）。
    /// </summary>
    public class GameComponent_RpgQuestChain : GameComponent
    {
        /// <summary>检查/驱动间隔（tick）。</summary>
        private const int CheckInterval = 250;

        private int nextAllowedTick;
        private int lastDailyCheckTick = -1;
        private QuestPart_MMTAncientArmory activeChain;

        public static GameComponent_RpgQuestChain Current
        {
            get { return Verse.Current.Game?.GetComponent<GameComponent_RpgQuestChain>(); }
        }

        public GameComponent_RpgQuestChain(Game game)
        {
        }

        public override void GameComponentTick()
        {
            int ticks = Find.TickManager.TicksGame;
            if (ticks % CheckInterval != 0)
                return;
            if (activeChain != null)
            {
                if (activeChain.quest == null || activeChain.quest.State != QuestState.Ongoing)
                {
                    activeChain = null;
                }
                else
                {
                    activeChain.CheckChain();
                    return;
                }
            }
            // 财富达标每日判定（10%/天）
            if (ticks - lastDailyCheckTick >= 60000)
            {
                lastDailyCheckTick = ticks;
                Map home = Find.AnyPlayerHomeMap;
                if (home != null && Rand.Chance(RpgQuestChainUtil.DailyWealthTriggerChance))
                    TryStart(home);
            }
        }

        /// <summary>扫描完成钩子（20% 概率触发任务链；与据点结果无关）。</summary>
        public void NotifyScanCompleted(Map homeMap)
        {
            if (Rand.Chance(RpgQuestChainUtil.ScanTriggerChance))
                TryStart(homeMap);
        }

        /// <summary>任务链入口（双开关 + 财富门槛 + 冷却 + 无进行中链）。</summary>
        public bool TryStart(Map homeMap)
        {
            if (!MMTSettings.RpgQuestsEnabled || !MMTSettings.BossEncountersEnabled)
                return false;
            if (homeMap == null || activeChain != null)
                return false;
            if (Find.TickManager.TicksGame < nextAllowedTick)
                return false;
            if (homeMap.wealthWatcher.WealthTotal < RpgQuestChainUtil.TriggerWealth)
                return false;
            if (Find.QuestManager.QuestsListForReading.Any(q => q.root == MMT_RpgQuestDefOf.MMT_Quest_AncientArmory
                && q.State == QuestState.Ongoing))
                return false;
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(MMT_RpgQuestDefOf.MMT_Quest_AncientArmory, new Slate());
            if (quest == null)
                return false;
            if (quest.State == QuestState.NotYetAccepted)
                quest.Accept(null);
            QuestPart_MMTAncientArmory part = quest.PartsListForReading.OfType<QuestPart_MMTAncientArmory>().FirstOrDefault();
            if (part == null)
                return false;
            activeChain = part;
            part.Begin(homeMap);
            return true;
        }

        /// <summary>任务链结束（成功/失败均冷却 30 天）。</summary>
        public void NotifyChainEnded()
        {
            activeChain = null;
            nextAllowedTick = Find.TickManager.TicksGame + RpgQuestChainUtil.CooldownTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextAllowedTick, "rpgChainNextAllowedTick", 0);
            Scribe_Values.Look(ref lastDailyCheckTick, "rpgChainLastDailyCheckTick", -1);
            Scribe_References.Look(ref activeChain, "rpgChainActive");
        }
    }

    [DefOf]
    public static class MMT_RpgQuestDefOf
    {
        /// <summary>任务链 QuestScriptDef。</summary>
        public static QuestScriptDef MMT_Quest_AncientArmory;
        /// <summary>职业凭证碎片（第二幕收集物）。</summary>
        public static ThingDef MMT_ClassTokenFragment;
        /// <summary>职业凭证（奖励）。</summary>
        public static ThingDef MMT_ClassToken;
        /// <summary>远古数据核心（奖励）。</summary>
        public static ThingDef MMT_AncientDatacore;

        static MMT_RpgQuestDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_RpgQuestDefOf));
        }
    }
}
