using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 建筑蓄电参数：容量与自充速率（XML 可配置）。
    /// 与物品电池 CompRechargeable 互补：本 comp 面向不可拆卸的电网建筑（蓄电随建筑存档）。
    /// </summary>
    public class CompProperties_BuildingBattery : CompProperties
    {
        /// <summary>蓄电容量（Wd）。</summary>
        public float maxChargeWd = 15000f;

        /// <summary>开启并通电时的蓄电速度（Wd/天）。</summary>
        public float chargeRateWdPerDay = 3000f;

        public CompProperties_BuildingBattery()
        {
            compClass = typeof(CompBuildingBattery);
        }
    }

    /// <summary>
    /// 建筑蓄电 Comp：开启并通电时按速率自蓄电，业务 comp 通过 TryDrain 消费。
    /// 新蓄电建筑 = comps 加一行 + 业务调 TryDrain，蓄电/闸门/InspectString 零新 C#。
    /// 注意：scribe 字段名沿用历史传送台的 storedChargeWd（旧存档蓄电量无缝接管），
    /// 同一建筑不得再挂其他 scribe 同名字段的 comp。
    /// </summary>
    public class CompBuildingBattery : ThingComp
    {
        /// <summary>当前蓄电（Wd，存档持久化；字段名与旧 CompTeleportPad 一致以接管旧存档）。</summary>
        private float storedChargeWd;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;
        // 增效模块槽（可空）：充能效率模块提升蓄电速度
        private CompModuleSlots slotsComp;

        public CompProperties_BuildingBattery Props
        {
            get { return (CompProperties_BuildingBattery)props; }
        }

        /// <summary>当前蓄电（Wd）。</summary>
        public float StoredChargeWd
        {
            get { return storedChargeWd; }
        }

        /// <summary>蓄电容量（Wd）。</summary>
        public float MaxChargeWd
        {
            get { return Props.maxChargeWd; }
        }

        /// <summary>是否已满。</summary>
        public bool IsFull
        {
            get { return storedChargeWd >= Props.maxChargeWd; }
        }

        /// <summary>当前是否正在蓄电：开关打开（无开关视为开）且电网通电（无电力 comp 视为不通）。</summary>
        public bool IsCharging
        {
            get
            {
                if (flickComp != null && !flickComp.SwitchIsOn)
                    return false;
                return powerComp != null && powerComp.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
            slotsComp = parent.GetComp<CompModuleSlots>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedChargeWd, "storedChargeWd");
        }

        /// <summary>实际蓄电速度（Wd/天）：基础速率 × (1 + 充能效率模块加成合计)。</summary>
        public float EffectiveChargeRateWdPerDay
        {
            get
            {
                float bonus = slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.ChargeRateFactor) : 0f;
                return Props.chargeRateWdPerDay * (1f + bonus);
            }
        }

        /// <summary>开启并通电时按 chargeRateWdPerDay 自动蓄电，充满即止。
        /// 1.7 坑：Normal tickerType 建筑收不到 CompTickRare，周期逻辑必须放 CompTickInterval(delta)。</summary>
        public override void CompTickInterval(int delta)
        {
            if (storedChargeWd >= Props.maxChargeWd)
                return;
            if (!IsCharging)
                return;
            storedChargeWd = Mathf.Min(Props.maxChargeWd, storedChargeWd + EffectiveChargeRateWdPerDay / 60000f * delta);
        }

        /// <summary>消耗蓄电；不足时不扣任何电并返回 false。</summary>
        public bool TryDrain(float amount)
        {
            if (storedChargeWd < amount)
                return false;
            storedChargeWd -= amount;
            return true;
        }

        /// <summary>立即充满（开发者 gizmo 用）。</summary>
        public void FillFull()
        {
            storedChargeWd = Props.maxChargeWd;
        }

        public override string CompInspectStringExtra()
        {
            string text = "蓄电: " + storedChargeWd.ToString("F0") + "/" + Props.maxChargeWd.ToString("F0") + " Wd";
            if (IsFull)
                text += "（已满）";
            else if (IsCharging)
                text += "（充电中，" + EffectiveChargeRateWdPerDay.ToString("F0") + " Wd/天）";
            else
                text += "（未充电：需要开启开关并通电）";
            return text;
        }
    }
}
