using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 护盾强化参数：等级上限、每级加成、成功率表与材料（XML 可配置）。
    /// 挂在可充能护盾腰带上，强化操作在充能台进行（见 ChargingStation.cs）。
    /// </summary>
    public class CompProperties_ShieldEnhancement : CompProperties
    {
        /// <summary>强化等级上限（默认 15）。</summary>
        public int maxLevel = 15;

        /// <summary>每级电量上限加成（基于基础值线性，0.06 = 每级 +6%）。</summary>
        public float chargeBonusPerLevel = 0.06f;

        /// <summary>每级伤害换电比削减（基于基础值线性，0.03 = 每级 -3%）。</summary>
        public float wdCostReductionPerLevel = 0.03f;

        /// <summary>满级额外电量上限加成（基于基础值，0.5 = 满级再 +50%）。</summary>
        public float finalLevelChargeBonus = 0.5f;

        /// <summary>满级换电比额外乘区（0.8 = 满级时再 ×0.8）。</summary>
        public float finalLevelWdFactor = 0.8f;

        /// <summary>每次强化尝试的材料消耗 = 目标等级 × 本系数。</summary>
        public int materialPerLevel = 2;

        /// <summary>强化材料（能量结晶）。</summary>
        public ThingDef materialDef;

        /// <summary>各级成功率表（第 i 项 = 强化到 i+1 级的成功率），缺省时用代码内置默认表。</summary>
        public List<float> successRates;

        public CompProperties_ShieldEnhancement()
        {
            compClass = typeof(CompShieldEnhancement);
        }
    }

    /// <summary>
    /// 护盾强化 Comp：只存强化等级（0-15），电量上限/伤害换电比的折算由
    /// CompRechargeable / CompShieldPack 读取本 comp 的系数属性完成（两处读取点，全链路自动跟随）。
    /// 成功率随目标等级递减：1-6 级 100%，7-8 级 80%，9-10 级 60%，11-12 级 40%，
    /// 13 级 20%，14 级 10%，15 级 1%（满级收益最大：电量再 +50%、换电比再 ×0.8）。
    /// </summary>
    public class CompShieldEnhancement : ThingComp
    {
        /// <summary>内置默认成功率表（强化到 1~15 级），XML 未配置时使用。</summary>
        private static readonly float[] DefaultRates =
        {
            1f, 1f, 1f, 1f, 1f, 1f,
            0.8f, 0.8f,
            0.6f, 0.6f,
            0.4f, 0.4f,
            0.2f, 0.1f, 0.01f,
        };

        /// <summary>当前强化等级（0-15，存档持久化）。</summary>
        private int level;

        private CompProperties_ShieldEnhancement Props
        {
            get { return (CompProperties_ShieldEnhancement)props; }
        }

        /// <summary>当前强化等级。</summary>
        public int Level
        {
            get { return level; }
        }

        /// <summary>下一强化等级（满级时为 maxLevel+1，仅展示用）。</summary>
        public int NextLevel
        {
            get { return level + 1; }
        }

        /// <summary>是否已达最高强化等级。</summary>
        public bool IsMaxLevel
        {
            get { return level >= Props.maxLevel; }
        }

        /// <summary>强化材料定义（能量结晶）。</summary>
        public ThingDef MaterialDef
        {
            get { return Props.materialDef; }
        }

        /// <summary>电量上限倍率：1-14 级每级 +6%（基于基础值），15 级再 +50%。</summary>
        public float MaxChargeFactor
        {
            get
            {
                float factor = 1f + Props.chargeBonusPerLevel * level;
                if (level >= Props.maxLevel)
                    factor += Props.finalLevelChargeBonus;
                return factor;
            }
        }

        /// <summary>伤害换电比倍率：1-14 级每级 -3%（基于基础值），15 级再 ×0.8（保底 5% 防零）。</summary>
        public float WdCostFactor
        {
            get
            {
                float factor = 1f - Props.wdCostReductionPerLevel * level;
                if (level >= Props.maxLevel)
                    factor *= Props.finalLevelWdFactor;
                return Mathf.Max(0.05f, factor);
            }
        }

        /// <summary>强化到下一级的成功率（满级为 0）。</summary>
        public float NextLevelSuccessRate
        {
            get { return IsMaxLevel ? 0f : SuccessRateFor(NextLevel); }
        }

        /// <summary>强化到下一级所需材料数量。</summary>
        public int NextLevelMaterialCost
        {
            get { return NextLevel * Props.materialPerLevel; }
        }

        /// <summary>强化到 targetLevel 级的成功率（XML 表缺项/越界时回退内置默认表；结果 × 设置倍率）。</summary>
        public float SuccessRateFor(int targetLevel)
        {
            if (targetLevel < 1 || targetLevel > Props.maxLevel)
                return 0f;
            float baseRate;
            if (Props.successRates != null && Props.successRates.Count >= targetLevel)
                baseRate = Props.successRates[targetLevel - 1];
            else if (targetLevel <= DefaultRates.Length)
                baseRate = DefaultRates[targetLevel - 1];
            else
                return 0f;
            return MMTSettings.AdjustEnhanceRate(baseRate);
        }

        /// <summary>掷一次强化：按下一级成功率判定，成功则等级 +1。材料由作业侧先行消耗，本方法只负责判定。</summary>
        public bool DoEnhance()
        {
            if (IsMaxLevel)
                return false;
            if (Rand.Chance(NextLevelSuccessRate))
            {
                // 节点流转：+N -> +(N+1)
                level++;
                return true;
            }
            return false;
        }

        /// <summary>物品标签追加强化等级后缀（如“充能护盾腰带 Mk.II +7”）。</summary>
        public override string TransformLabel(string label)
        {
            return level > 0 ? label + " +" + level : label;
        }

        public override string CompInspectStringExtra()
        {
            if (level <= 0)
                return null;
            return "强化等级: +" + level + "（电量上限 ×" + MaxChargeFactor.ToString("F2")
                + "，伤害耗电 ×" + WdCostFactor.ToString("F2") + "）";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref level, "shieldEnhanceLevel", 0);
        }
    }
}
