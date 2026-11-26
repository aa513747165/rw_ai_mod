using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 装备随机强化/附魔掷点（设计方案 mmt-rpg-design.md §1：RPG 支柱一，只做随机强化等级 + 随机附魔）。
    /// 装备生成瞬间按难度一次掷好，直接写入既有强化 comp 的 level/enchants 字段：
    /// 不新增 comp/存档字段，数值结算/label/inspect 全部复用强化系统。
    /// 难度分档：袭击者 = 生成时威胁点四档（&lt;500 / 500-2000 / 2000-5000 / &gt;5000），
    /// 据点/任务走 Roll(gear, tier) 入口（tier 0-3 对应同一四档）；难度越高强化越高、附魔越多越高级。
    /// 词条池照抄模块 XML 档位注释分组（tools/validate/loot_roll.py 校验防漏），新增零个效果键。
    /// </summary>
    public static class LootRoll
    {
        // ==================== 词条池（照抄 XML 档位注释分组，校验套件同步核对） ====================

        /// <summary>武器池·低级（基础属性）。</summary>
        private static readonly string[] WeaponLow =
        {
            "MMT_WeaponModSharp", "MMT_WeaponModAccurate", "MMT_WeaponModSwift", "MMT_WeaponModPierce",
        };

        /// <summary>武器池·中级（复合属性）。</summary>
        private static readonly string[] WeaponMid =
        {
            "MMT_WeaponModDeadly", "MMT_WeaponModGale", "MMT_WeaponModHeavy", "MMT_WeaponModRapid",
            "MMT_WeaponModBlitz",
        };

        /// <summary>武器池·高级（命中特效）。</summary>
        private static readonly string[] WeaponHigh =
        {
            "MMT_WeaponModEmp", "MMT_WeaponModIncendiary", "MMT_WeaponModLifeLeech",
            "MMT_WeaponModShieldBreaker", "MMT_WeaponModFrost", "MMT_WeaponModExplode",
            "MMT_WeaponModGas", "MMT_WeaponModSuppress", "MMT_WeaponModDisarm",
            "MMT_WeaponModToxic", "MMT_WeaponModInsectBane", "MMT_WeaponModMending",
            "MMT_WeaponModGuardian", "MMT_WeaponModInsight", "MMT_WeaponModLongRange",
            "MMT_WeaponModDeadeye", "MMT_WeaponModMagazine", "MMT_WeaponModBlinding",
            "MMT_WeaponModCorrode", "MMT_WeaponModBleed", "MMT_WeaponModSmoke",
            "MMT_WeaponModSlaughter", "MMT_WeaponModDemolition",
        };

        /// <summary>武器池·极品（独特机制；仅最高档开放，权重减半）。</summary>
        private static readonly string[] WeaponExtreme =
        {
            "MMT_WeaponModExecute", "MMT_WeaponModChainLightning", "MMT_WeaponModTimeFold",
            "MMT_WeaponModOverload", "MMT_WeaponModGambler", "MMT_WeaponModRiot",
            "MMT_WeaponModStormCall", "MMT_WeaponModSureHit", "MMT_WeaponModVampire",
            "MMT_WeaponModDisruptor", "MMT_WeaponModBrutal", "MMT_WeaponModTerror",
            "MMT_WeaponModRangedLeech", "MMT_WeaponModCorpseBurst",
        };

        /// <summary>防具池·低级（防御强化/功能辅助）。</summary>
        private static readonly string[] ApparelLow =
        {
            "MMT_ApparelModFortify", "MMT_ApparelModBladeWard", "MMT_ApparelModBulwark",
            "MMT_ApparelModWarmth", "MMT_ApparelModCooling",
        };

        /// <summary>防具池·中级（专项防御/功能辅助）。</summary>
        private static readonly string[] ApparelMid =
        {
            "MMT_ApparelModHeatWard", "MMT_ApparelModAbsorb", "MMT_ApparelModEvasion",
            "MMT_ApparelModSwift", "MMT_ApparelModPorter", "MMT_ApparelModNumbing",
            "MMT_ApparelModSteady", "MMT_ApparelModGrit", "MMT_ApparelModImmunity",
        };

        /// <summary>防具池·高级（生存机制/反击特效）。</summary>
        private static readonly string[] ApparelHigh =
        {
            "MMT_ApparelModRecovery", "MMT_ApparelModEmpShield", "MMT_ApparelModDiligent",
            "MMT_ApparelModThorns", "MMT_ApparelModDischarge", "MMT_ApparelModMindWard",
            "MMT_ApparelModRegen",
        };

        /// <summary>防具池·极品（复合/危险特效；仅最高档开放，权重减半）。</summary>
        private static readonly string[] ApparelExtreme =
        {
            "MMT_ApparelModAegis", "MMT_ApparelModFeather", "MMT_ApparelModToxicCloud",
        };

        // ==================== 限定与危险清单 ====================

        /// <summary>近战限定词条（不对远程武器入池）：吸血/嗜血的治疗效果只在近战命中结算。</summary>
        private static readonly HashSet<string> MeleeOnlyWeaponMods = new HashSet<string>
        {
            "MMT_WeaponModLifeLeech", "MMT_WeaponModVampire",
        };

        /// <summary>远程限定词条（不对近战武器入池）：汲取/扩容/长程只对远程 verb 有意义。</summary>
        private static readonly HashSet<string> RangedOnlyWeaponMods = new HashSet<string>
        {
            "MMT_WeaponModRangedLeech", "MMT_WeaponModMagazine", "MMT_WeaponModLongRange",
        };

        /// <summary>危险特效词条（池内权重减半；仍受既有 gizmo/设置门控）。</summary>
        private static readonly HashSet<string> HazardMods = new HashSet<string>
        {
            "MMT_WeaponModExplode", "MMT_WeaponModStormCall", "MMT_WeaponModGas",
            "MMT_WeaponModSmoke", "MMT_WeaponModCorpseBurst",
            "MMT_ApparelModThorns", "MMT_ApparelModDischarge", "MMT_ApparelModToxicCloud",
        };

        // ==================== 档位表 ====================

        /// <summary>强化等级掷骰段位：区间内均匀取值，weight 为相对权重。</summary>
        private struct LevelBand
        {
            public int min;
            public int max;
            public float weight;

            public LevelBand(int min, int max, float weight)
            {
                this.min = min;
                this.max = max;
                this.weight = weight;
            }
        }

        /// <summary>四档强化等级分布（设计方案 §1.2；lootRollChanceFactor 放大非 +0 档权重）。</summary>
        private static readonly LevelBand[][] LevelBands =
        {
            // 威胁点 < 500：+0 70%，+1~3 25%，+4~6 5%
            new[] { new LevelBand(0, 0, 70f), new LevelBand(1, 3, 25f), new LevelBand(4, 6, 5f) },
            // 500–2000：+0 50%，+1~3 33%，+4~6 14%，+7~9 3%
            new[] { new LevelBand(0, 0, 50f), new LevelBand(1, 3, 33f), new LevelBand(4, 6, 14f), new LevelBand(7, 9, 3f) },
            // 2000–5000：+0 35%，+1~3 33%，+4~6 20%，+7~9 9%，+10~12 3%
            new[] { new LevelBand(0, 0, 35f), new LevelBand(1, 3, 33f), new LevelBand(4, 6, 20f), new LevelBand(7, 9, 9f), new LevelBand(10, 12, 3f) },
            // > 5000：+0 25%，+1~3 28%，+4~6 25%，+7~9 13%，+10~12 7%，+13~15 2%
            new[] { new LevelBand(0, 0, 25f), new LevelBand(1, 3, 28f), new LevelBand(4, 6, 25f), new LevelBand(7, 9, 13f), new LevelBand(10, 12, 7f), new LevelBand(13, 15, 2f) },
        };

        /// <summary>四档附魔概率（设计方案 §1.3；× lootRollChanceFactor，封顶 1）。</summary>
        private static readonly float[] EnchantChances = { 0.15f, 0.30f, 0.45f, 0.60f };

        // ---- def 缓存（惰性解析；校验套件保证 defName 存在） ----
        private static bool poolsResolved;
        private static readonly Dictionary<string, ThingDef> moduleDefs = new Dictionary<string, ThingDef>();

        /// <summary>威胁点 → 难度档（0-3）。</summary>
        public static int TierForPoints(float points)
        {
            if (points < 500f) return 0;
            if (points < 2000f) return 1;
            if (points < 5000f) return 2;
            return 3;
        }

        /// <summary>袭击者入口：按威胁点分档掷点。</summary>
        public static void RollForPoints(ThingWithComps gear, float points)
        {
            Roll(gear, TierForPoints(points));
        }

        /// <summary>
        /// 据点/任务/袭击者共用入口：对一件装备掷初始强化等级与随机附魔。
        /// 直接写既有强化 comp（武器或防具），无 comp（如护盾腰带）静默跳过。
        /// </summary>
        public static void Roll(ThingWithComps gear, int tier)
        {
            if (gear == null || !MMTSettings.LootRollEnabled)
                return;
            IEnchantable enh = gear.TryGetComp<CompWeaponEnhancement>();
            bool isWeapon = enh != null;
            if (!isWeapon)
                enh = gear.TryGetComp<CompApparelEnhancement>();
            if (enh == null)
                return;
            tier = Mathf.Clamp(tier, 0, 3);
            float factor = MMTSettings.LootRollChanceFactor;

            // ---- 强化等级：掷段位（非 +0 档权重 × 倍率），直接写入（不耗材料、不走成功率） ----
            int level = RollLevel(tier, factor);
            if (isWeapon)
                ((CompWeaponEnhancement)enh).SetLevelForLoot(level);
            else
                ((CompApparelEnhancement)enh).SetLevelForLoot(level);

            // ---- 附魔：概率 × 倍率；条数受当前槽位限制（强化等级天然限低档位词条数） ----
            if (!Rand.Chance(Mathf.Clamp01(EnchantChances[tier] * factor)))
                return;
            int count = EnchantCount(tier, enh.EnchantSlots);
            if (count <= 0)
                return;
            List<ThingDef> pool = BuildPool(tier, isWeapon, gear.def);
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                ThingDef pick = WeightedPick(pool, isWeapon);
                if (pick == null)
                    break;
                enh.AddEnchant(pick);
                pool.Remove(pick);
            }
        }

        /// <summary>袭击者威胁点获取（武器/防具两条生成钩子共用）：仅敌对派系，无图跳过。</summary>
        public static bool TryRaiderThreatPoints(Pawn pawn, out float points)
        {
            points = 0f;
            Faction faction = pawn?.Faction;
            // 世界生成阶段（派系领袖生成）玩家派系尚不存在，必须用 SilentFail 版避免红字
            Faction player = Faction.OfPlayerSilentFail;
            if (faction == null || player == null || !faction.HostileTo(player))
                return false;
            Map map = pawn.Map ?? Find.AnyPlayerHomeMap;
            if (map == null)
                return false;
            points = StorytellerUtility.DefaultThreatPointsNow(map);
            return true;
        }

        /// <summary>掷强化等级：按段位权重抽取（非 +0 段 × 倍率），段内均匀取级。</summary>
        private static int RollLevel(int tier, float factor)
        {
            LevelBand[] bands = LevelBands[tier];
            float total = 0f;
            for (int i = 0; i < bands.Length; i++)
                total += BandWeight(bands[i], factor);
            float roll = Rand.Value * total;
            for (int i = 0; i < bands.Length; i++)
            {
                roll -= BandWeight(bands[i], factor);
                if (roll <= 0f)
                    return Rand.RangeInclusive(bands[i].min, bands[i].max);
            }
            return 0;
        }

        /// <summary>段位权重：非 +0 段 × 设置倍率（放大高难度掉落概率）。</summary>
        private static float BandWeight(LevelBand band, float factor)
        {
            return band.min == 0 ? band.weight : band.weight * factor;
        }

        /// <summary>附魔条数：低中档固定 1 条，高档 1~2 条，最高档 1 条~槽满。</summary>
        private static int EnchantCount(int tier, int slots)
        {
            if (slots <= 0)
                return 0;
            switch (tier)
            {
                case 0:
                case 1:
                    return 1;
                case 2:
                    return Rand.Chance(0.5f) ? 1 : Mathf.Min(2, slots);
                default:
                    return Rand.RangeInclusive(1, slots);
            }
        }

        /// <summary>
        /// 组建抽取池：按档位开放池子（tier0 低级 / tier1 +中级 / tier2 +高级 / tier3 +极品），
        /// 武器按近战/远程过滤限定词条。每次掷点新建小列表（生成瞬间一次，非热点）。
        /// </summary>
        private static List<ThingDef> BuildPool(int tier, bool isWeapon, ThingDef gearDef)
        {
            ResolvePools();
            List<ThingDef> pool = new List<ThingDef>();
            bool ranged = isWeapon && gearDef != null && gearDef.IsRangedWeapon;
            AddPool(pool, isWeapon ? WeaponLow : ApparelLow, isWeapon, ranged);
            if (tier >= 1)
                AddPool(pool, isWeapon ? WeaponMid : ApparelMid, isWeapon, ranged);
            if (tier >= 2)
                AddPool(pool, isWeapon ? WeaponHigh : ApparelHigh, isWeapon, ranged);
            if (tier >= 3)
                AddPool(pool, isWeapon ? WeaponExtreme : ApparelExtreme, isWeapon, ranged);
            return pool;
        }

        /// <summary>把一组模块 defName 解析入池（按近战/远程限定过滤）。</summary>
        private static void AddPool(List<ThingDef> pool, string[] names, bool isWeapon, bool ranged)
        {
            for (int i = 0; i < names.Length; i++)
            {
                ThingDef def;
                if (!moduleDefs.TryGetValue(names[i], out def) || def == null)
                    continue;
                if (isWeapon)
                {
                    if (ranged && MeleeOnlyWeaponMods.Contains(names[i]))
                        continue;
                    if (!ranged && RangedOnlyWeaponMods.Contains(names[i]))
                        continue;
                }
                pool.Add(def);
            }
        }

        /// <summary>加权抽取：极品与危险特效权重减半（可叠加到 1/4）。</summary>
        private static ThingDef WeightedPick(List<ThingDef> pool, bool isWeapon)
        {
            float total = 0f;
            for (int i = 0; i < pool.Count; i++)
                total += PickWeight(pool[i], isWeapon);
            if (total <= 0f)
                return null;
            float roll = Rand.Value * total;
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= PickWeight(pool[i], isWeapon);
                if (roll <= 0f)
                    return pool[i];
            }
            return pool[pool.Count - 1];
        }

        /// <summary>单词条权重：极品 ×0.5，危险特效 ×0.5。</summary>
        private static float PickWeight(ThingDef def, bool isWeapon)
        {
            float w = 1f;
            string[] extreme = isWeapon ? WeaponExtreme : ApparelExtreme;
            for (int i = 0; i < extreme.Length; i++)
            {
                if (extreme[i] == def.defName)
                {
                    w *= 0.5f;
                    break;
                }
            }
            if (HazardMods.Contains(def.defName))
                w *= 0.5f;
            return w;
        }

        /// <summary>惰性解析全部模块 defName → ThingDef（缺失静默跳过，校验套件防漏）。</summary>
        private static void ResolvePools()
        {
            if (poolsResolved)
                return;
            poolsResolved = true;
            ResolveNames(WeaponLow);
            ResolveNames(WeaponMid);
            ResolveNames(WeaponHigh);
            ResolveNames(WeaponExtreme);
            ResolveNames(ApparelLow);
            ResolveNames(ApparelMid);
            ResolveNames(ApparelHigh);
            ResolveNames(ApparelExtreme);
        }

        private static void ResolveNames(string[] names)
        {
            for (int i = 0; i < names.Length; i++)
                moduleDefs[names[i]] = DefDatabase<ThingDef>.GetNamedSilentFail(names[i]);
        }
    }

    /// <summary>袭击者武器掷点：武器生成后按当前威胁点掷强化等级与附魔（紧跟 BudgetPatch，互不影响）。</summary>
    [HarmonyPatch(typeof(PawnWeaponGenerator), nameof(PawnWeaponGenerator.TryGenerateWeaponFor))]
    public static class Patch_LootRollRaiderWeapon
    {
        public static void Postfix(Pawn pawn)
        {
            if (!MMTSettings.LootRollEnabled)
                return;
            ThingWithComps weapon = pawn?.equipment?.Primary;
            if (weapon == null)
                return;
            float points;
            if (!LootRoll.TryRaiderThreatPoints(pawn, out points))
                return;
            LootRoll.RollForPoints(weapon, points);
        }
    }

    /// <summary>
    /// 袭击者防具掷点：着装生成出口（1.7 签名经反编译核实 GenerateStartingApparelFor(Pawn, PawnGenerationRequest)）
    /// 对每件已穿防具掷强化等级与附魔；护盾腰带无附魔 comp 自动跳过。
    /// </summary>
    [HarmonyPatch(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor))]
    public static class Patch_LootRollRaiderApparel
    {
        public static void Postfix(Pawn pawn)
        {
            if (!MMTSettings.LootRollEnabled)
                return;
            if (pawn?.apparel == null)
                return;
            float points;
            if (!LootRoll.TryRaiderThreatPoints(pawn, out points))
                return;
            List<Apparel> worn = pawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
                LootRoll.RollForPoints(worn[i], points);
        }
    }
}
