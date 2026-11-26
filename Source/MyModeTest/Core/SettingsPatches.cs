using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 资源产率倍率：MMT 配方（MMT_ 前缀）产出数量 × 设置倍率。
    /// 挂在配方结算口（非热点路径，每次做完成品一次），OreSift 随机产物同路径自动覆盖。
    /// </summary>
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_ResourceYieldFactor
    {
        public static void Postfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef)
        {
            float factor = MMTSettings.ResourceYieldFactor;
            if (factor == 1f || __result == null || recipeDef == null) return;
            if (recipeDef.defName == null || !recipeDef.defName.StartsWith("MMT_")) return;
            __result = ScaleProducts(__result, factor);
        }

        // 惰性包装：保持原可枚举语义，逐个放大 stackCount（至少保留 1）
        private static IEnumerable<Thing> ScaleProducts(IEnumerable<Thing> products, float factor)
        {
            foreach (Thing thing in products)
            {
                if (thing != null && thing.def.stackLimit > 1)
                    thing.stackCount = Mathf.Max(1, Mathf.RoundToInt(thing.stackCount * factor));
                yield return thing;
            }
        }
    }

    /// <summary>
    /// 研究成本倍率：MMT 研究（MMT_ 前缀）的 CostApparent × 设置倍率。
    /// </summary>
    [HarmonyPatch(typeof(ResearchProjectDef), "get_CostApparent")]
    public static class Patch_ResearchCostFactor
    {
        public static void Postfix(ResearchProjectDef __instance, ref float __result)
        {
            float factor = MMTSettings.ResearchCostFactor;
            if (factor == 1f || __instance == null) return;
            if (__instance.defName == null || !__instance.defName.StartsWith("MMT_")) return;
            __result *= factor;
        }
    }
}
