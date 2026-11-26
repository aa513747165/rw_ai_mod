using System.Collections.Generic;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MyModeTest
{
    /// <summary>
    /// Boss 召唤器参数（dev 测试工具）：候选 = XML 白名单 + 全库自动扫描
    /// （isFighter 且 combatPower ≥ 阈值的 PawnKindDef 全收，原版/Mod Boss 通吃，零 mod 感知代码）。
    /// </summary>
    public class CompProperties_BossSummoner : CompProperties
    {
        /// <summary>XML 白名单（无条件入选；默认预留副本链 Boss：rpg 文档 §3.2 远古守卫者）。</summary>
        public List<string> bossPawnKindDefNames = new List<string> { "MMT_BossAncientGuardian" };

        /// <summary>自动扫描战力阈值：combatPower ≥ 该值的战斗单位视为 Boss 级入选（原版参考：巫妖/战争女皇 300+、炼狱魔王/战争女皇级 Boss 更高）。</summary>
        public float autoBossMinCombatPower = 150f;

        public CompProperties_BossSummoner()
        {
            compClass = typeof(CompBossSummoner);
        }
    }

    /// <summary>
    /// Boss 召唤器（仅 Dev 模式可建，测试用）：gizmo 选 Boss 种类 → 目标选格 → 以敌对身份原地生成
    /// 并挂 LordJob_AssaultColony（立即主动进攻，便于实测 Boss 战力）。
    /// 派系：机械族 Boss 挂机械族派系；其余走副本宿主派系选取（DungeonSiteGen.PickHostFaction）。
    /// </summary>
    public class CompBossSummoner : ThingComp
    {
        private CompProperties_BossSummoner Props
        {
            get { return (CompProperties_BossSummoner)props; }
        }

        // 候选缓存：Def 库加载后不变，只扫一次（gizmo 高频重绘，避免每次全库扫描+排序的 GC 压力）
        private static List<PawnKindDef> cachedBossKinds;

        /// <summary>
        /// 解析候选 Boss：复用 BossPool 池（XML 白名单 ∪ 全库自动扫描，原版/Mod Boss 自动在列）。
        /// 结果按战力降序缓存，供 FloatMenu 展示（gizmo 高频重绘，避免每次全库扫描+排序的 GC 压力）。
        /// </summary>
        private List<PawnKindDef> ResolveBossKinds()
        {
            if (cachedBossKinds != null)
                return cachedBossKinds;
            cachedBossKinds = BossPool.Candidates(Props.bossPawnKindDefNames, Props.autoBossMinCombatPower);
            return cachedBossKinds;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            Command_Action summon = new Command_Action();
            summon.defaultLabel = "召唤 Boss";
            summon.defaultDesc = "DEV 测试工具：列出全部 Boss 级单位（XML 白名单 + 全库按战力自动扫描，含原版与 Mod 单位），在地图上指定落点以敌对身份生成。仅在开发者模式可用。";
            summon.icon = parent.def.uiIcon;
            if (!Prefs.DevMode)
            {
                summon.Disable("仅开发者模式可用");
            }
            else
            {
                List<PawnKindDef> kinds = ResolveBossKinds();
                if (kinds.Count == 0)
                {
                    summon.Disable("没有可解析的 Boss 定义（候选 PawnKindDef 均未实装；自建 Boss 后将 defName 加入召唤器 XML 列表即可）");
                }
                else
                {
                    summon.action = delegate { OpenBossMenu(kinds); };
                }
            }
            yield return summon;
        }

        /// <summary>选择 Boss 种类 → 进入目标选格（菜单含战力与来源包标注）。</summary>
        private void OpenBossMenu(List<PawnKindDef> kinds)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (PawnKindDef kind in kinds)
            {
                PawnKindDef k = kind;
                string source = k.modContentPack != null && !k.modContentPack.IsCoreMod
                    ? " · " + k.modContentPack.Name
                    : "";
                options.Add(new FloatMenuOption(
                    k.LabelCap + "（" + k.defName + "，战力 " + k.combatPower.ToString("F0") + source + "）",
                    delegate { BeginTargetingFor(k); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void BeginTargetingFor(PawnKindDef kind)
        {
            Find.Targeter.BeginTargeting(TargetingParameters.ForCell(),
                delegate (LocalTargetInfo t) { SpawnBoss(kind, t.Cell); });
        }

        /// <summary>
        /// 生成 Boss：派系走 BossPool.ResolveBossFaction（类人→副本宿主派系、机械/虫/异常实体→各自原版派系），
        /// 有派系挂 LordJob_AssaultColony 立即进攻；动物等无派系靠种族原生敌意并强制永久狂暴兜底。全程静默回退。
        /// </summary>
        private void SpawnBoss(PawnKindDef kind, IntVec3 cell)
        {
            Map map = parent.Map;
            if (!cell.IsValid || !cell.InBounds(map))
                return;
            Faction faction = BossPool.ResolveBossFaction(kind,
                kind.RaceProps != null && kind.RaceProps.Humanlike ? DungeonSiteGen.PickHostFaction(1000f) : null);
            if (kind.RaceProps != null && kind.RaceProps.Humanlike && faction == null)
            {
                Messages.Message("召唤失败：没有可用的敌对派系", new LookTargets(parent),
                    MessageTypeDefOf.RejectInput, false);
                return;
            }
            Pawn boss = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction,
                PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true));
            if (boss == null)
                return;
            if (!cell.Walkable(map)
                && !CellFinder.TryFindRandomSpawnCellForPawnNear(cell, map, out cell, 5))
                cell = parent.Position;
            GenSpawn.Spawn(boss, cell, map);
            if (faction != null)
            {
                LordMaker.MakeNewLord(faction,
                    new LordJob_AssaultColony(faction, canKidnap: false, canTimeoutOrFlee: false),
                    map, Gen.YieldSingle(boss));
            }
            else
            {
                // 无派系（动物/实体）：强制永久狂暴保证敌意
                boss.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            }
            Messages.Message("DEV：已召唤 " + boss.LabelCap
                + (faction != null ? "（" + faction.Name + "，主动进攻）" : "（无派系，永久狂暴）"),
                new LookTargets(boss), MessageTypeDefOf.ThreatBig, false);
        }
    }

    /// <summary>
    /// Boss 召唤器仅 Dev 模式可见可建：建筑设计图可见性 postfix，
    /// 非 Dev 模式对 MMT_BossSummoner 强制隐藏（godMode 属开发工具，不在此限）。
    /// </summary>
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.Visible), MethodType.Getter)]
    public static class Patch_BossSummonerDevOnly
    {
        public static void Postfix(Designator_Build __instance, ref bool __result)
        {
            if (__result && !Prefs.DevMode
                && __instance.PlacingDef is ThingDef td && td.defName == "MMT_BossSummoner")
                __result = false;
        }
    }

    /// <summary>
    /// DEV 操控 Boss/召唤物放技能（测试工具）：Debug 菜单「MMT Boss → 操控放技能」对地图上任一
    /// 带技能的单位使用（Boss、召唤物、玩家小人均通用，覆盖后续"Boss 当召唤物"的手动施放测试）。
    /// 列出全部技能（含剩余冷却），选中后按技能 verb.targetParams 进原版瞄准，确认后排入施法 job；
    /// 无目标参数的技能直接激活。不打断派系/lord，仅插一个施法 job，施放完由原 ThinkTree 接管。
    /// DebugAction 本身仅 Dev 模式可见，无需额外门控。
    /// </summary>
    public static class DevBossControl
    {
        [DebugAction("MMT Boss", "操控放技能（选中有技能的单位）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ControlCast(Pawn pawn)
        {
            if (pawn?.abilities == null || pawn.abilities.abilities.Count == 0)
            {
                Messages.Message("【MMT】" + (pawn?.LabelShortCap ?? "目标") + " 没有任何技能",
                    pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Ability ability in pawn.abilities.abilities)
            {
                Ability ab = ability;
                string cd = ab.OnCooldown
                    ? "（冷却中 " + (ab.CooldownTicksRemaining / 60f).ToString("F1") + " 秒）"
                    : "";
                options.Add(new FloatMenuOption(ab.def.LabelCap + cd,
                    delegate { BeginAbilityTargeting(ab); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>有目标参数进原版瞄准，确认后 QueueCastingJob；无目标参数原地直接激活。</summary>
        private static void BeginAbilityTargeting(Ability ability)
        {
            TargetingParameters tp = ability.verb?.targetParams;
            if (tp == null)
            {
                ability.Activate(new GlobalTargetInfo(ability.pawn));
                return;
            }
            Find.Targeter.BeginTargeting(tp,
                delegate (LocalTargetInfo t) { ability.QueueCastingJob(t, t); });
        }
    }
}
