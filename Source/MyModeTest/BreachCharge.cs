using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 破墙炸弹：袭击者财富门槛配发（额外携带，不替换原武器）。
    /// 殖民地财富达 10 万后，敌对人类单位按财富概率获得爆墙炸弹：
    /// 背包放入物品本体 + 注入携带者 Hediff（MMT_Hediff_BreachChargeCarrier，经 HediffComp_VerbGiver 授予投掷动词），
    /// 原有武器与常规战斗逻辑完全不受影响。
    /// 破墙者单位（isGoodBreacher）全概率、其他战斗单位半概率。
    /// 炸弹为消耗品：自定义投掷作业每出手一次消耗背包中一枚，耗尽即摘除携带者 Hediff 回归普通战斗
    /// （原版破墙 AI 路径直接驱动动词、不经消耗；精英爆破手标配多枚，见 EliteBreacher.cs）。
    /// 炸弹的使用由三条路径触发：
    /// 1. 原版破墙 AI：FindVerbToUseForBreaching 补丁让携带者破墙时优先用炸弹动词；
    /// 2. 主动炸建筑：TryGiveJob 补丁按保守概率让携带者主动攻击最近玩家建筑
    ///    （精英爆破手 100% 主动且直奔墙体，无墙可炸时退化为最近建筑）；
    /// 3. 投掷动作本身由自定义 JobDriver_ThrowBreachCharge 驱动（AttackStatic 不支持非装备动词）。
    /// </summary>
    public static class BreachChargeUtil
    {
        // 财富门槛/封顶读设置（门槛 5万-50万，封顶 = 门槛 × 10）；曲线端点仍为代码常量
        private static float WealthThreshold
        {
            get { return MMTSettings.Instance != null ? MMTSettings.Instance.breachWealthThreshold : 100000f; }
        }

        private static float WealthCap
        {
            get { return WealthThreshold * 10f; }
        }

        // 配发概率曲线：门槛起步5% -> 封顶50%
        private const float EquipMin = 0.05f;
        private const float EquipMax = 0.50f;

        // 主动炸建筑概率曲线（每次决策掷点，故取保守值）：门槛起步2% -> 封顶20%
        private const float AggroMin = 0.02f;
        private const float AggroMax = 0.20f;

        // 概率倍率（0-3，0 = 不出现）
        private static float ChanceFactor
        {
            get { return MMTSettings.Instance != null ? MMTSettings.Instance.breachChanceFactor : 1f; }
        }

        private static ThingDef bombDef;
        private static HediffDef carrierHediff;
        private static ThingDef projDef;
        private static JobDef throwJobDef;
        public static ThingDef BombDef => bombDef ?? (bombDef = DefDatabase<ThingDef>.GetNamed("MMT_Weapon_WallBreachCharge"));
        public static HediffDef CarrierHediff => carrierHediff ?? (carrierHediff = DefDatabase<HediffDef>.GetNamed("MMT_Hediff_BreachChargeCarrier"));
        public static ThingDef ProjDef => projDef ?? (projDef = DefDatabase<ThingDef>.GetNamed("MMT_Proj_WallBreachCharge"));
        public static JobDef ThrowJobDef => throwJobDef ?? (throwJobDef = DefDatabase<JobDef>.GetNamed("MMT_ThrowBreachCharge"));

        // 配发概率：破墙者全概率由调用方决定，这里只给基础曲线（× 设置倍率）
        public static float EquipChance() => WealthCurve(EquipMin, EquipMax) * ChanceFactor;

        // 主动炸建筑概率（× 设置倍率）
        public static float AggroChance() => WealthCurve(AggroMin, AggroMax) * ChanceFactor;

        // 财富线性曲线：<10万为0，10万~100万线性增长，100万封顶
        private static float WealthCurve(float min, float max)
        {
            float wealth = Find.AnyPlayerHomeMap?.wealthWatcher?.WealthTotal ?? 0f;
            if (wealth < WealthThreshold) return 0f;
            return Mathf.Clamp(min + (wealth - WealthThreshold) / (WealthCap - WealthThreshold) * (max - min), min, max);
        }

        // 是否为爆墙炸弹携带者
        public static bool IsCarrier(Pawn pawn) =>
            pawn?.health?.hediffSet != null && pawn.health.hediffSet.HasHediff(CarrierHediff, false);

        // 从携带者 Hediff 的 VerbGiver comp 中取出爆墙炸弹投掷动词；未携带返回 null
        public static Verb GetBombVerb(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return null;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def != CarrierHediff) continue;
                if (!(hediff is HediffWithComps withComps)) continue;
                foreach (HediffComp comp in withComps.comps)
                {
                    if (!(comp is HediffComp_VerbGiver verbGiver)) continue;
                    foreach (Verb verb in verbGiver.VerbTracker.AllVerbs)
                        if (verb.verbProps.defaultProjectile == ProjDef) return verb;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 补丁一：袭击者生成时按财富概率配发爆墙炸弹（额外携带）。
    /// 不触碰原有武器：背包放物品本体 + 注入携带者 Hediff（授予投掷动词）。
    /// 破墙者单位（isGoodBreacher）全概率，其他敌对战斗人类单位半概率。
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_BreachChargeCarry
    {
        public static void Postfix(Pawn __result)
        {
            // 总开关（入口第一行廉价短路）
            if (!MMTSettings.BreachChargeEnabled) return;
            Pawn pawn = __result;
            // 过滤：人类 + 敌对玩家
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike) return;
            if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayerSilentFail)) return;
            // 财富概率：破墙者全概率，其他单位半概率
            float chance = BreachChargeUtil.EquipChance();
            if (chance <= 0f) return;
            if (!pawn.kindDef.isGoodBreacher) chance *= 0.5f;
            if (!Rand.Chance(chance)) return;
            // 配发：注入携带者 Hediff（授予投掷动词）+ 背包放入炸弹物品本体
            pawn.health.AddHediff(BreachChargeUtil.CarrierHediff);
            pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(BreachChargeUtil.BombDef));
        }
    }

    /// <summary>
    /// 补丁二：携带者按保守财富概率主动攻击最近玩家建筑（不要求路径被堵）。
    /// 精英爆破手（MMT_EliteBreacher）为专职爆破单位：100% 主动出击且直奔墙体
    /// （满格且不可通行的玩家建筑；无墙可炸时退化为最近建筑，仍履行爆破职责）。
    /// 命中时下发自定义投掷作业（MMT_ThrowBreachCharge）并跳过原战斗逻辑；
    /// 作业设过期重评估，避免敌人永久锁死在炸建筑上。
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class Patch_BreachChargeAggro
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            // 总开关（入口第一行廉价短路）
            if (!MMTSettings.BreachChargeEnabled) return true;
            // 仅处理：爆墙炸弹携带者 + 敌对玩家
            if (!BreachChargeUtil.IsCarrier(pawn)) return true;
            if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayerSilentFail)) return true;
            if (pawn.Map == null) return true;
            bool elite = EliteBreacherUtil.IsElite(pawn);
            // 精英 100% 主动；普通携带者按保守曲线掷点（每次决策掷一次）
            if (!elite && !Rand.Chance(BreachChargeUtil.AggroChance())) return true;
            // 目标选取：精英直奔墙体，无墙可炸时退化为最近玩家建筑；普通携带者直接找最近建筑
            // （统一要求能走到贴脸，避免墙外卡死）
            Thing target = elite ? FindWallTarget(pawn) : null;
            if (target == null) target = FindBuildingTarget(pawn);
            if (target == null) return true;
            // 下发投掷爆墙炸弹作业；定期过期重评估以便脱战回打 pawn
            Job job = JobMaker.MakeJob(BreachChargeUtil.ThrowJobDef, target);
            job.expiryInterval = 500;
            job.checkOverrideOnExpire = true;
            __result = job;
            return false;
        }

        // 最近可到达的玩家建筑（墙/门/炮塔/家具均可）
        private static Thing FindBuildingTarget(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn), 9999f,
                t => t is Building b && b.Faction == Faction.OfPlayerSilentFail);
        }

        // 最近可到达的玩家墙体：满格且不可通行的建筑（排除门等可通过结构）
        private static Thing FindWallTarget(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn), 9999f,
                t => t is Building b && b.Faction == Faction.OfPlayerSilentFail
                    && b.def.Fillage == FillCategory.Full && b.def.passability == Traversability.Impassable);
        }
    }

    /// <summary>
    /// 补丁三：原版破墙逻辑选动词时，携带者优先使用爆墙炸弹动词。
    /// 原版 FindVerbToUseForBreaching 只看主武器（额外携带的炸弹不在其中），
    /// 这里在返回前用携带者 Hediff 的投掷动词覆盖结果；
    /// IsGoodBreacher 内部也走此方法，携带者自动成为合格破墙者。
    /// </summary>
    [HarmonyPatch(typeof(BreachingUtility), "FindVerbToUseForBreaching")]
    public static class Patch_BreachChargeBreaching
    {
        public static void Postfix(Pawn pawn, ref Verb __result)
        {
            // 总开关（入口第一行廉价短路）
            if (!MMTSettings.BreachChargeEnabled) return;
            if (__result?.verbProps?.defaultProjectile == BreachChargeUtil.ProjDef) return;
            Verb bombVerb = BreachChargeUtil.GetBombVerb(pawn);
            if (bombVerb != null && bombVerb.Available()) __result = bombVerb;
        }
    }

    /// <summary>
    /// 1.7 引擎兜底：无装备动词（HediffComp_VerbGiver 等授予，EquipmentSource 为 null）若带
    /// forcedMissRadius，TryCastShot 强制 miss 分支会对 null equipment 解引用 NRE（每 tick 刷屏）。
    /// 这里在 GetForceMissFactorFor 入口拦截：equipment 为 null 时按原语义返回 1f（无迫击炮加成）。
    /// </summary>
    [HarmonyPatch(typeof(VerbProperties), "GetForceMissFactorFor")]
    public static class Patch_ForceMissNullEquipment
    {
        public static bool Prefix(Thing equipment, ref float __result)
        {
            if (equipment != null) return true;
            __result = 1f;
            return false;
        }
    }

    /// <summary>
    /// 投掷爆墙炸弹作业驱动：走到目标建筑贴脸后用携带者 Hediff 的投掷动词打一发。
    /// 原版 JobDriver_AttackStatic 只用装备/近战动词（忽略 job.verbToUse），故需自定义驱动。
    /// 炸弹为消耗品：出手即从背包消耗一枚（投掷被打断也算消耗，炸药已离手）；
    /// 投掷完成后背包无弹即摘除携带者 Hediff，单位回归普通战斗（精英爆破手多枚标配见 EliteBreacher.cs）。
    /// </summary>
    public class JobDriver_ThrowBreachCharge : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            // 流程开始：目标建筑消失即失败
            this.FailOnDespawnedOrNull(TargetIndex.A);
            // 走到目标贴脸（炸弹射程12.9，贴脸必在射程内且视线通畅）
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            // 投掷一发：热身/出手由站姿系统驱动，动作结束后作业成功
            Toil cast = ToilMaker.MakeToil("ThrowBreachCharge");
            Verb verb = null;
            bool castStarted = false;
            cast.initAction = delegate
            {
                verb = BreachChargeUtil.GetBombVerb(pawn);
                if (verb == null || !verb.TryStartCastOn(TargetA, false, true, true, false))
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                castStarted = true;
                // 出手即耗弹：消耗背包中一枚爆墙炸弹
                Thing bomb = pawn.inventory.innerContainer.FirstOrDefault(t => t.def == BreachChargeUtil.BombDef);
                bomb?.Destroy(DestroyMode.Vanish);
            };
            cast.defaultCompleteMode = ToilCompleteMode.Never;
            cast.tickIntervalAction = delegate
            {
                if (castStarted && !pawn.stances.FullBodyBusy)
                {
                    // 投掷完成：炸弹耗尽即摘除携带者 Hediff（此时站姿已释放，摘除安全）
                    if (!pawn.inventory.innerContainer.Any(t => t.def == BreachChargeUtil.BombDef))
                    {
                        Hediff carrier = pawn.health.hediffSet.GetFirstHediffOfDef(BreachChargeUtil.CarrierHediff, false);
                        if (carrier != null) pawn.health.RemoveHediff(carrier);
                    }
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            yield return cast;
            // 流程结束
        }
    }
}
