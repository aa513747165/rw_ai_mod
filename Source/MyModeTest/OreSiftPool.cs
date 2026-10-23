using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 无中生有链：筛选配方的随机矿石池。
    /// 扫描全部矿脉 Def（isResourceRock 且 mineableThing 非空），按 mineableScatterCommonality 加权，
    /// 原版/MMT/其它 mod 的矿脉自动入池，无需维护列表。
    /// </summary>
    [StaticConstructorOnStartup]
    public static class OreSiftPool
    {
        // 非矿物类矿脉黑名单：压缩机械（产零部件）等不符合"筛矿"设定，需要时在此追加
        private static readonly HashSet<string> ExcludedVeins = new HashSet<string> { "MineableComponentsIndustrial" };

        private static readonly List<KeyValuePair<ThingDef, float>> Pool = new List<KeyValuePair<ThingDef, float>>();

        static OreSiftPool()
        {
            // 流程开始：Def 全部解析完成后构建矿石池
            foreach (ThingDef vein in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (vein.building == null || !vein.building.isResourceRock)
                    continue;
                ThingDef drop = vein.building.mineableThing;
                if (drop == null || ExcludedVeins.Contains(vein.defName))
                    continue;
                float weight = vein.building.mineableScatterCommonality;
                // 未配置出现率的矿脉给一个低保底权重，避免永远筛不到
                if (weight <= 0f)
                    weight = 0.05f;
                Pool.Add(new KeyValuePair<ThingDef, float>(drop, weight));
            }
            Log.Message("[myModeTest] 矿粉筛选矿石池已构建: " + Pool.Count + " 种矿脉");
            // 流程结束
        }

        /// <summary>按矿脉权重随机抽取一种矿石。</summary>
        public static ThingDef RollOre()
        {
            if (Pool.Count == 0)
                return null;
            float total = 0f;
            foreach (KeyValuePair<ThingDef, float> kv in Pool)
                total += kv.Value;
            float roll = Rand.Value * total;
            foreach (KeyValuePair<ThingDef, float> kv in Pool)
            {
                roll -= kv.Value;
                if (roll <= 0f)
                    return kv.Key;
            }
            return Pool[Pool.Count - 1].Key;
        }
    }

    /// <summary>
    /// Harmony 补丁：拦截配方产物生成，将 MMT 筛选配方的产出替换为按矿脉权重随机的矿石。
    /// </summary>
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class Patch_SiftOrePowder
    {
        private const string HandRecipe = "MMT_SiftOrePowder_Hand";
        private const string ElectricRecipe = "MMT_SiftOrePowder_Electric";

        public static void Postfix(RecipeDef recipeDef, ref IEnumerable<Thing> __result)
        {
            bool electric = recipeDef.defName == ElectricRecipe;
            if (!electric && recipeDef.defName != HandRecipe)
                return;
            // 节点流转：固定产出 -> 矿石池随机产出
            ThingDef ore = OreSiftPool.RollOre();
            if (ore == null)
                return;
            // 电动筛批量更大：6~10，手动 4~7
            int count = electric ? Rand.RangeInclusive(6, 10) : Rand.RangeInclusive(4, 7);
            Thing thing = ThingMaker.MakeThing(ore);
            thing.stackCount = count;
            __result = new List<Thing> { thing };
        }
    }
}
