using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 精英爆破手（mmt-endgame-design.md §10.5）：殖民地财富 ≥50 万后，
    /// 敌对玩家的人类派系袭击会额外编入专职爆破单位（多枚爆墙炸弹 + 直奔墙体）。
    /// 数量随财富爬升：50 万起 1 名，每 +50 万 +1 名，封顶 3 名；
    /// 每名按 PawnKindDef.combatPower 从袭击点数中预扣，总难度不因注入而膨胀。
    /// 走原版袭击生成管线（不加自定义 ticker，符合 §12.5 性能约束）；开关 eliteBreacherEnabled。
    /// 爆破行为（100% 主动、墙体优先）在 BreachCharge.cs 的 Patch_BreachChargeAggro 精英分支。
    /// </summary>
    public static class EliteBreacherUtil
    {
        // 财富门槛与数量曲线（代码常量，对标设计文档 §10.5 的"财富 50 万+"）
        public const float WealthThreshold = 500000f;
        private const float WealthPerExtra = 500000f;
        private const int MaxCount = 3;
        // 精英标配爆墙炸弹枚数（每投掷一次消耗一枚，耗尽后摘除携带者 Hediff 回归普通战斗）
        public const int BombCount = 5;
        // 预扣后给原袭击保留的点数下限（低预算袭击少注入甚至不注入）
        public const float MinRemainingPoints = 200f;

        private static PawnKindDef eliteKind;
        public static PawnKindDef EliteKind => eliteKind ?? (eliteKind = DefDatabase<PawnKindDef>.GetNamed("MMT_EliteBreacher"));

        public static bool IsElite(Pawn pawn) => pawn?.kindDef == EliteKind;

        // 单次袭击注入名额：<50 万为 0；50 万 1 名，每 +50 万 +1，封顶 3
        public static int EliteCount()
        {
            if (!MMTSettings.EliteBreacherEnabled) return 0;
            float wealth = Find.AnyPlayerHomeMap?.wealthWatcher?.WealthTotal ?? 0f;
            if (wealth < WealthThreshold) return 0;
            return Mathf.Min(1 + (int)((wealth - WealthThreshold) / WealthPerExtra), MaxCount);
        }
    }

    /// <summary>
    /// 袭击注入补丁：挂在原版小兵生成总出口（1.7 签名 GeneratePawns(PawnGroupMakerParms, bool)；
    /// 敌袭经 IncidentParmsUtility.GetDefaultPawnGroupMakerParms 带入 raidStrategy，商队/据点守卫/副本生成为空）。
    /// 前缀按财富算名额并预扣点数（PawnGroupMakerParms 为 class，前缀改 points 直接影响本次组生成）；
    /// 后缀生成精英追加进袭击名单——装备与强化掷点走既有生成钩子（LootRoll 等），
    /// 炸弹与携带者 Hediff 在此配发（Patch_BreachChargeCarry 可能已先发 1 枚，此处补足到标配枚数）。
    /// </summary>
    [HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns),
        new[] { typeof(PawnGroupMakerParms), typeof(bool) })]
    public static class Patch_EliteBreacherRaidInjection
    {
        private static int pendingCount;

        public static void Prefix(PawnGroupMakerParms parms)
        {
            pendingCount = 0;
            // 总开关 + 财富门槛（廉价短路在最前）
            int count = EliteBreacherUtil.EliteCount();
            if (count <= 0) return;
            if (parms == null) return;
            // 仅袭击：raidStrategy 非空（定居点/商队/副本守卫生成为 null）
            if (parms.raidStrategy == null) return;
            // 仅敌对玩家的人类派系（机械/虫族/友军增援排除）
            if (parms.faction == null || !parms.faction.def.humanlikeFaction) return;
            if (!parms.faction.HostileTo(Faction.OfPlayerSilentFail)) return;
            // 点数预算兜底：预扣后至少保留下限，不够就少注入
            float cost = EliteBreacherUtil.EliteKind.combatPower;
            while (count > 0 && parms.points - count * cost < EliteBreacherUtil.MinRemainingPoints) count--;
            if (count <= 0) return;
            parms.points -= count * cost;
            pendingCount = count;
        }

        public static void Postfix(PawnGroupMakerParms parms, ref IEnumerable<Pawn> __result)
        {
            if (pendingCount <= 0 || parms == null) return;
            int count = pendingCount;
            pendingCount = 0;
            List<Pawn> elites = new List<Pawn>(count);
            for (int i = 0; i < count; i++)
            {
                Pawn elite = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    EliteBreacherUtil.EliteKind, parms.faction, PawnGenerationContext.NonPlayer,
                    parms.tile, mustBeCapableOfViolence: true));
                if (elite == null) continue;
                // 配发：携带者 Hediff（授予投掷动词）+ 补足标配爆墙炸弹（消耗品，耗尽摘 Hediff）
                if (!BreachChargeUtil.IsCarrier(elite))
                    elite.health.AddHediff(BreachChargeUtil.CarrierHediff);
                int existing = elite.inventory.innerContainer.Count(t => t.def == BreachChargeUtil.BombDef);
                for (int b = existing; b < EliteBreacherUtil.BombCount; b++)
                    elite.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(BreachChargeUtil.BombDef));
                elites.Add(elite);
            }
            if (elites.Count == 0) return;
            Messages.Message("侦察报告：敌方袭击队伍中混入了精英爆破手，携带着大量爆墙炸弹直扑你的墙体！",
                MessageTypeDefOf.NegativeEvent, false);
            __result = __result.Concat(elites);
        }
    }
}
