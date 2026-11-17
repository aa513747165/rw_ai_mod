using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 按袭击威胁点动态缩放原版敌对派系的武器预算（PawnWeaponGenerator 前缀/后缀补丁）。
    /// 威胁点（含财富/时间/难度因子）越高，敌人可买到的武器越贵；
    /// 只对官方内容包（Core/DLC）的敌对派系生效，其它 mod 派系与非敌对 pawn 完全走原版逻辑，
    /// 原版选枪方法体不做任何改动，仅在生成前后临时替换并恢复共享 kindDef 的 weaponMoney。
    /// </summary>
    [HarmonyPatch(typeof(PawnWeaponGenerator), nameof(PawnWeaponGenerator.TryGenerateWeaponFor))]
    public static class PawnWeaponBudgetPatch
    {
        // 威胁点参考值：达到该点数时预算倍率为 1.0（约中盘袭击规模）
        private const float RefPoints = 500f;
        // 预算倍率下限/上限：下限 1.0 保证只做增量（不削弱原版早期强度，避免低价池买不起最便宜的枪而空手），
        // 上限 2.0 防止后期预算无限膨胀
        private const float MinFactor = 1.0f;
        private const float MaxFactor = 2.0f;

        // 流程开始：生成武器前，判断该 pawn 是否需要动态缩放预算
        private static void Prefix(Pawn pawn, out FloatRange? __state)
        {
            __state = null;
            Faction faction = pawn?.Faction;
            // 仅干预原版官方派系（Core/DLC），mod 派系直接放行
            if (faction?.def?.modContentPack == null || !faction.def.modContentPack.IsOfficialMod)
            {
                return;
            }
            // 仅干预对玩家敌对的派系，商人/访客/盟友不受影响
            if (!faction.HostileTo(Faction.OfPlayer))
            {
                return;
            }
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
            {
                return;
            }
            // 节点流转：威胁点 -> 平方根曲线（早期平缓、后期有界）-> 预算倍率
            float points = StorytellerUtility.DefaultThreatPointsNow(map);
            float factor = (float)Math.Sqrt(points / RefPoints);
            if (factor < MinFactor) factor = MinFactor;
            if (factor > MaxFactor) factor = MaxFactor;
            FloatRange original = pawn.kindDef.weaponMoney;
            __state = original;
            pawn.kindDef.weaponMoney = new FloatRange(original.min * factor, original.max * factor);
        }

        // 流程结束：恢复共享 kindDef 的原始预算（kindDef 被所有同类 pawn 共享，必须还原）
        private static void Postfix(Pawn pawn, FloatRange? __state)
        {
            if (__state.HasValue && pawn?.kindDef != null)
            {
                pawn.kindDef.weaponMoney = __state.Value;
            }
        }
    }
}