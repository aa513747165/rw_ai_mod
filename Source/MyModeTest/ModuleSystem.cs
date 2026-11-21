using System.Collections.Generic;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 可装入建筑槽位的物品识别扩展（挂物品 ThingDef 上）：
    /// slotKey 与建筑侧 CompProperties_ModuleSlots.slotLimits 的 key 匹配才可装，同 key 共享安装上限。
    /// 仿滤网 MeshFilterExtension 模式：新增可安装物品挂本扩展（或子类）即被槽 Comp 自动识别，零 C# 改动。
    /// </summary>
    public class InstallableSlotItemExtension : DefModExtension
    {
        /// <summary>槽位类别键（如 Overclock / Mesh），决定共享上限与互斥。</summary>
        public string slotKey;

        /// <summary>页签/菜单中的效果说明文案（完整中文文案，如"处理速度 +10%/个"）。</summary>
        public string effectReport;
    }

    /// <summary>单条模块效果：key 语义由消费端定义（见 ModuleEffectKeys），valuePerItem 每件累加。</summary>
    public class ModuleEffect
    {
        public string key;
        public float valuePerItem;
    }

    /// <summary>
    /// 增效模块配置（继承槽位识别扩展）：模块的效果列表全部 XML 化。
    /// 新增同组模块变体（如"高级超频"）= 纯 XML；新增效果语义 = 常量 1 行 + 消费端读取 1 行。
    /// </summary>
    public class ModuleExtension : InstallableSlotItemExtension
    {
        public List<ModuleEffect> effects;
    }

    /// <summary>内置效果键常量（消费端与 XML 约定用；新键无需登记，约定即可）。</summary>
    public static class ModuleEffectKeys
    {
        /// <summary>处理速度加成：每件 +0.1 → 间隔 / (1 + 合计)。</summary>
        public const string SpeedFactor = "SpeedFactor";
        /// <summary>生产线加成：每件 +1 条。</summary>
        public const string ExtraLanes = "ExtraLanes";
        /// <summary>产量加成：每件 +1 单位，消费端按设备各自系数（yieldPerModule）换算为产量增量。</summary>
        public const string YieldBonus = "YieldBonus";
        /// <summary>充能速度加成：每件 +0.1 → 充能速率 × (1 + 合计)。</summary>
        public const string ChargeRateFactor = "ChargeRateFactor";
        /// <summary>传送次数上限加成：每件 +1 次。</summary>
        public const string ExtraTeleportCharges = "ExtraTeleportCharges";

        // ---- 武器附魔词条键（武器强化台确定线；值>0 即启用，见 WeaponEnhance.cs） ----
        /// <summary>伤害加成（加法，0.08 = +8%）。锋利/重击/过载。</summary>
        public const string WeaponDamageFactor = "WeaponDamageFactor";
        /// <summary>命中加成（加法，0.08 = +8%；负值为代价）。精准/速射。</summary>
        public const string WeaponHitChance = "WeaponHitChance";
        /// <summary>冷却系数（加法，-0.08 = 冷却 -8%）。轻巧/疾风/过载/速射。</summary>
        public const string WeaponCooldownFactor = "WeaponCooldownFactor";
        /// <summary>瞄准时间系数（加法，-0.20 = 瞄准 -20%）。疾风。</summary>
        public const string WeaponWarmupFactor = "WeaponWarmupFactor";
        /// <summary>暴击率（加法，0.05 = +5%；暴击造成双倍伤害）。致命。</summary>
        public const string WeaponCritChance = "WeaponCritChance";
        /// <summary>护甲穿透（加法，0.15 = +15%）。穿透。</summary>
        public const string WeaponArmorPen = "WeaponArmorPen";
        /// <summary>击退力度（0=无，1=标准击退）。重击。</summary>
        public const string WeaponKnockback = "WeaponKnockback";
        // 特殊机制词条（命中结算，逻辑在伤害补丁）：
        /// <summary>EMP 弹：对机械伤害 ×1.5 + 短路眩晕。</summary>
        public const string WeaponEmp = "WeaponEmp";
        /// <summary>燃烧弹：命中点燃目标。</summary>
        public const string WeaponIncendiary = "WeaponIncendiary";
        /// <summary>吸血：近战伤害的 5% 转为治疗（值=转化比例）。</summary>
        public const string WeaponLifeLeech = "WeaponLifeLeech";
        /// <summary>破盾：对被护盾拦截的攻击额外消耗护盾能量（值=额外倍率）。</summary>
        public const string WeaponShieldBreaker = "WeaponShieldBreaker";
        /// <summary>冰霜：命中附加减速（-20% 移速，3 秒）。</summary>
        public const string WeaponFrost = "WeaponFrost";
        /// <summary>处决：目标血量低于 20% 时伤害 ×2。</summary>
        public const string WeaponExecute = "WeaponExecute";
        /// <summary>连锁闪电：命中弹射 2 格内另一敌对目标，造成 50% 伤害。</summary>
        public const string WeaponChainLightning = "WeaponChainLightning";
        /// <summary>时空折叠：击杀后本次攻击冷却归零。</summary>
        public const string WeaponTimeFold = "WeaponTimeFold";
    }

    /// <summary>设备侧槽位上限条目：key 对应物品扩展的 slotKey。</summary>
    public class ModuleSlotLimit
    {
        public string key;
        public int max;
    }

    /// <summary>一条已安装槽位物品记录（物品实体在安装时销毁，只留 def 与槽位键）。</summary>
    public class InstalledSlotItem : IExposable
    {
        public string slotKey;
        public ThingDef itemDef;

        public void ExposeData()
        {
            Scribe_Values.Look(ref slotKey, "slotKey");
            Scribe_Defs.Look(ref itemDef, "itemDef");
        }
    }

    /// <summary>增效模块页签的一行展示数据（槽位键 + 代表物品 + 已装/上限 + 效果文案）。</summary>
    public class ModuleSlotRow
    {
        public string slotKey;
        public ThingDef itemDef;
        public int installed;
        public int max;
        public string effectText;
    }
}
