using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 可充能设备参数：电池容量（XML 可配置）。
    /// 挂在任何需要内置电量的物品上（护盾腰带等），配合充能台回充。
    /// </summary>
    public class CompProperties_Rechargeable : CompProperties
    {
        /// <summary>电池容量上限（Wd，1 Wd = 1 瓦特·游戏天）。</summary>
        public float maxChargeWd = 4000f;

        /// <summary>新造物品出厂时的初始电量（0~1，默认满电出厂方便测试与首次使用）。</summary>
        public float initialChargeRatio = 1f;

        public CompProperties_Rechargeable()
        {
            compClass = typeof(CompRechargeable);
        }
    }

    /// <summary>
    /// 通用可充能 Comp：只负责存电量与充/放电接口，具体耗电逻辑由设备自己的 Comp 实现。
    /// 充能台通过本 Comp 识别"可充能设备"并回充电量。
    /// </summary>
    public class CompRechargeable : ThingComp
    {
        /// <summary>当前电量（Wd，存档持久化）。</summary>
        private float chargeWd;

        // 强化 comp 惰性缓存（服装在背包/充能台内不 spawn，不能靠 PostSpawnSetup 缓存）
        private CompShieldEnhancement enhComp;

        private CompProperties_Rechargeable Props
        {
            get { return (CompProperties_Rechargeable)props; }
        }

        /// <summary>强化 comp；物品上没有 CompShieldEnhancement 时为 null（电量上限即基础值）。</summary>
        private CompShieldEnhancement Enhancement
        {
            get
            {
                if (enhComp == null && parent is ThingWithComps twc)
                    enhComp = twc.TryGetComp<CompShieldEnhancement>();
                return enhComp;
            }
        }

        /// <summary>当前电量（Wd）。</summary>
        public float ChargeWd
        {
            get { return chargeWd; }
        }

        /// <summary>电量上限（Wd）：基础容量 × 强化倍率（无强化 comp 时倍率为 1）。
        /// 下游 ChargeRatio/IsFull/AddCharge 与充能台推进、电量条 gizmo 全部经此读取，强化生效零改动跟随。</summary>
        public float MaxChargeWd
        {
            get
            {
                CompShieldEnhancement enh = Enhancement;
                return Props.maxChargeWd * (enh != null ? enh.MaxChargeFactor : 1f);
            }
        }

        /// <summary>电量百分比 0~1。</summary>
        public float ChargeRatio
        {
            get { return MaxChargeWd <= 0f ? 0f : chargeWd / MaxChargeWd; }
        }

        /// <summary>是否已充满。</summary>
        public bool IsFull
        {
            get { return chargeWd >= MaxChargeWd; }
        }

        /// <summary>是否需要充电。</summary>
        public bool NeedsCharge
        {
            get { return !IsFull; }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            chargeWd = MaxChargeWd * Props.initialChargeRatio;
        }

        /// <summary>充入电量（不超过上限），返回实际充入量。</summary>
        public float AddCharge(float amount)
        {
            if (amount <= 0f)
                return 0f;
            float before = chargeWd;
            chargeWd = UnityEngine.Mathf.Min(MaxChargeWd, chargeWd + amount);
            return chargeWd - before;
        }

        /// <summary>消耗电量（不低于 0），返回实际消耗量。</summary>
        public float Drain(float amount)
        {
            if (amount <= 0f)
                return 0f;
            float before = chargeWd;
            chargeWd = UnityEngine.Mathf.Max(0f, chargeWd - amount);
            return before - chargeWd;
        }

        /// <summary>直接清空电量（EMP 等场景）。</summary>
        public void ClearCharge()
        {
            chargeWd = 0f;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref chargeWd, "chargeWd", 0f);
        }

        public override string CompInspectStringExtra()
        {
            return "电量: " + chargeWd.ToString("F0") + "/" + MaxChargeWd.ToString("F0")
                + " Wd（" + ChargeRatio.ToStringPercent() + "）";
        }
    }
}
