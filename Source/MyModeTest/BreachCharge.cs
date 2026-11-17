using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 破墙炸弹：袭击者财富门槛配发 + 主动炸建筑。
    /// 殖民地财富达 10 万后，敌对人类单位按财富概率把主武器换成爆墙炸弹（MMT_Weapon_WallBreachCharge），
    /// 破墙者单位（isGoodBreacher）全概率、其他战斗单位半概率；
    /// 持弹敌人另有独立保守概率在每次战斗决策时主动攻击最近的玩家建筑（不要求路径被堵）。
    /// 破门本身由原版破墙 AI 完成（武器 verb 带 ai_IsBuildingDestroyer）。
    /// </summary>
    public static class BreachChargeUtil
    {
        // 财富门槛与封顶（两个补丁共用）
        private const float WealthThreshold = 100000f;  // 出现门槛：10万
        private const float WealthCap = 1000000f;       // 概率封顶：100万

        // 换装概率曲线：10万起步5% -> 100万封顶50%
        private const float EquipMin = 0.05f;
        private const float EquipMax = 0.50f;

        // 主动炸建筑概率曲线（每次决策掷点，故取保守值）：10万起步2% -> 100万封顶20%
        private const float AggroMin = 0.02f;
        private const float AggroMax = 0.20f;

        private static ThingDef bombDef;
        public static ThingDef BombDef => bombDef ?? (bombDef = DefDatabase<ThingDef>.GetNamed("MMT_Weapon_WallBreachCharge"));

        // 换装概率：破墙者全概率由调用方决定，这里只给基础曲线
        public static float EquipChance() => WealthCurve(EquipMin, EquipMax);

        // 主动炸建筑概率
        public static float AggroChance() => WealthCurve(AggroMin, AggroMax);

        // 财富线性曲线：<10万为0，10万~100万线性增长，100万封顶
        private static float WealthCurve(float min, float max)
        {
            float wealth = Find.AnyPlayerHomeMap?.wealthWatcher?.WealthTotal ?? 0f;
            if (wealth < WealthThreshold) return 0f;
            return Mathf.Clamp(min + (wealth - WealthThreshold) / (WealthCap - WealthThreshold) * (max - min), min, max);
        }
    }

    /// <summary>
    /// 补丁一：袭击者生成时按财富概率换装爆墙炸弹。
    /// 破墙者单位（isGoodBreacher）全概率，其他敌对战斗人类单位半概率。
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_BreachChargeEquip
    {
        public static void Postfix(Pawn __result)
        {
            Pawn pawn = __result;
            // 过滤：人类 + 敌对玩家 + 当前持有主武器
            if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike) return;
            if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayerSilentFail)) return;
            if (pawn.equipment?.Primary == null) return;
            // 财富概率：破墙者全概率，其他单位半概率
            float chance = BreachChargeUtil.EquipChance();
            if (chance <= 0f) return;
            if (!pawn.kindDef.isGoodBreacher) chance *= 0.5f;
            if (!Rand.Chance(chance)) return;
            // 换装：销毁原主武器 -> 装备爆墙炸弹
            pawn.equipment.Primary.Destroy();
            pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(BreachChargeUtil.BombDef));
        }
    }

    /// <summary>
    /// 补丁二：持爆墙炸弹的敌人按保守财富概率主动攻击最近玩家建筑（不要求路径被堵）。
    /// 命中时返回 false 跳过原战斗逻辑，下发 AttackStatic 工作；
    /// 工作设过期重评估，避免敌人永久锁死在炸建筑上。
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class Patch_BreachChargeAggro
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            // 仅处理：手持爆墙炸弹 + 敌对玩家
            if (pawn?.equipment?.Primary?.def != BreachChargeUtil.BombDef) return true;
            if (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayerSilentFail)) return true;
            if (pawn.Map == null) return true;
            // 按保守曲线掷点（每次决策掷一次）
            if (!Rand.Chance(BreachChargeUtil.AggroChance())) return true;
            // 节点流转：找最近的可到达的玩家建筑（不考虑是否挡路，只要求能走到，避免墙外卡死）
            Thing target = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn), 9999f,
                t => t is Building b && b.Faction == Faction.OfPlayerSilentFail);
            if (target == null) return true;
            // 下发攻击建筑工作；定期过期重评估以便脱战回打 pawn
            Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
            job.expiryInterval = 500;
            job.checkOverrideOnExpire = true;
            __result = job;
            return false;
        }
    }
}
