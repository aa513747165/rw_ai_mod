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
    /// 装滤网时按滤网白名单过滤（MeshFilterExtension，XML 可配置）；
    /// 不在任何白名单的"未分类矿"按权重归入粗/中/细三档窗口，落进对应档位的滤网。
    /// </summary>
    [StaticConstructorOnStartup]
    public static class OreSiftPool
    {
        // 非矿物类矿脉黑名单：压缩机械（产零部件）等不符合"筛矿"设定，需要时在此追加
        private static readonly HashSet<string> ExcludedVeins = new HashSet<string> { "MineableComponentsIndustrial" };

        // 未分类矿归窗阈值（矿脉权重 = mineableScatterCommonality）：>= 粗档下限归木滤网，>= 中档下限归铁滤网，其余归精密滤网
        private const float CoarseTierMin = 0.6f;
        private const float MidTierMin = 0.25f;

        private static readonly List<KeyValuePair<ThingDef, float>> Pool = new List<KeyValuePair<ThingDef, float>>();

        // 全部滤网白名单的并集：用于识别"未分类矿"（含第三方 mod 矿脉），未分类矿按档位窗口归位
        private static readonly HashSet<ThingDef> Whitelisted = new HashSet<ThingDef>();

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
            // 汇总全部滤网白名单（新增滤网物品自动生效）
            foreach (ThingDef meshDef in CompSieveMesh.AllMeshDefs())
            {
                MeshFilterExtension ext = meshDef.GetModExtension<MeshFilterExtension>();
                if (ext.ores == null)
                    continue;
                foreach (ThingDef ore in ext.ores)
                    Whitelisted.Add(ore);
            }
            Log.Message("[myModeTest] 矿粉筛选矿石池已构建: " + Pool.Count + " 种矿脉");
            // 流程结束
        }

        /// <summary>按矿脉权重随机抽取一种矿石（无滤网时的现状行为）。</summary>
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

        /// <summary>按滤网过滤抽取：先判定掺沙（命中则产沙子），否则在滤网白名单窗口内按权重抽取。</summary>
        public static ThingDef RollOre(ThingDef meshDef)
        {
            MeshFilterExtension ext = meshDef != null ? meshDef.GetModExtension<MeshFilterExtension>() : null;
            if (ext == null)
                return RollOre();
            // 掺沙判定：本轮只出沙子（玻璃链原料）
            if (ext.sandChance > 0f && Rand.Chance(ext.sandChance))
                return MMT_DefOf.MMT_Sand;
            float total = 0f;
            foreach (KeyValuePair<ThingDef, float> kv in Pool)
                if (Allowed(ext, kv))
                    total += kv.Value;
            // 白名单与池无交集时兜底全池，避免空窗不出货
            if (total <= 0f)
                return RollOre();
            float roll = Rand.Value * total;
            foreach (KeyValuePair<ThingDef, float> kv in Pool)
            {
                if (!Allowed(ext, kv))
                    continue;
                roll -= kv.Value;
                if (roll <= 0f)
                    return kv.Key;
            }
            return RollOre();
        }

        /// <summary>该池条目是否被滤网放行：白名单内直接放行；其它滤网的白名单矿不放行；未分类矿按权重归窗对档放行。</summary>
        private static bool Allowed(MeshFilterExtension ext, KeyValuePair<ThingDef, float> entry)
        {
            if (ext.ores != null && ext.ores.Contains(entry.Key))
                return true;
            if (Whitelisted.Contains(entry.Key))
                return false;
            return TierOfWeight(entry.Value) == ext.tier;
        }

        /// <summary>矿脉权重 → 档位（0 粗 / 1 中 / 2 细），用于未分类矿归窗。</summary>
        private static int TierOfWeight(float weight)
        {
            if (weight >= CoarseTierMin)
                return 0;
            if (weight >= MidTierMin)
                return 1;
            return 2;
        }
    }

    /// <summary>
    /// Harmony 补丁：拦截配方产物生成，将 MMT 筛选配方的产出替换为按矿脉权重随机的矿石；
    /// 筛子装有滤网时改走滤网白名单窗口（可能掺沙）。
    /// </summary>
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class Patch_SiftOrePowder
    {
        private const string HandRecipe = "MMT_SiftOrePowder_Hand";
        private const string ElectricRecipe = "MMT_SiftOrePowder_Electric";

        public static void Postfix(RecipeDef recipeDef, Pawn worker, ref IEnumerable<Thing> __result)
        {
            bool electric = recipeDef.defName == ElectricRecipe;
            if (!electric && recipeDef.defName != HandRecipe)
                return;
            // 节点流转：固定产出 -> 矿石池随机产出（装滤网则走滤网窗口）
            ThingDef ore = null;
            Bill bill = worker != null && worker.CurJob != null ? worker.CurJob.bill : null;
            ThingWithComps building = bill != null ? bill.billStack.billGiver as ThingWithComps : null;
            CompSieveMesh meshSlot = building != null ? building.GetComp<CompSieveMesh>() : null;
            if (meshSlot != null && meshSlot.HasMesh)
                ore = OreSiftPool.RollOre(meshSlot.MeshDef);
            else
                ore = OreSiftPool.RollOre();
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
