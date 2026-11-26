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

        /// <summary>安装后武器名前缀（如"爆裂"，TransformLabel 取首个非空词条，2~4 字中文；可空）。</summary>
        public string labelPrefix;
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

        // ---- 武器附魔词条键（装备强化台确定线；值>0 即启用，见 WeaponEnhance.cs） ----
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
        /// <summary>EMP 弹：对机械追加 EMP 伤害（值=追加倍率，0.5 = +50%）并短路眩晕。EMP弹/磁暴。</summary>
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

        // ---- 附魔2（Infusion 2）借鉴词条（命中结算，逻辑在 WeaponOnHitMechanics.Apply） ----
        /// <summary>爆裂：命中 25% 触发半径 1.9 小爆炸（伤害 = 本次 ×0.35；危险特效，受 gizmo/设置门控）。</summary>
        public const string WeaponExplode = "WeaponExplode";
        /// <summary>引雷：命中 2.5% 在命中点召唤落雷（危险特效）。</summary>
        public const string WeaponStormCall = "WeaponStormCall";
        /// <summary>毒气：命中 25% 落点放毒气（Biotech ToxGas，无则回退毒性积累；危险特效）。</summary>
        public const string WeaponGas = "WeaponGas";
        /// <summary>压制：命中概率眩晕 90 tick（值=概率）。</summary>
        public const string WeaponSuppression = "WeaponSuppression";
        /// <summary>缴械：命中 5% 打掉目标主手武器（值=概率）。</summary>
        public const string WeaponDisarm = "WeaponDisarm";
        /// <summary>剧毒：每次命中附加毒性积累（值=severity 增量，仅血肉）。</summary>
        public const string WeaponToxic = "WeaponToxic";
        /// <summary>虫敌：对昆虫追加伤害（值=追加倍率，镜像 EMP 对机械）。</summary>
        public const string WeaponInsectBane = "WeaponInsectBane";
        /// <summary>急救：命中同派系友军时按伤害 × 值 治疗其伤口（净治疗为正）。</summary>
        public const string WeaponMending = "WeaponMending";
        /// <summary>守护：持械者承伤减免（值=减免比例，上限 0.5；PreApplyDamage 补丁消费）。</summary>
        public const string WeaponGuardian = "WeaponGuardian";
        /// <summary>悟战：每次有效命中攻击者获得武器技能经验（值>0 启用）。</summary>
        public const string WeaponSkillExp = "WeaponSkillExp";
        /// <summary>必中：每发概率必中（值=概率；按 tick 缓存掷骰，命中补丁消费）。必中。</summary>
        public const string WeaponGuaranteedHit = "WeaponGuaranteedHit";

        // ---- 武器附魔扩展词条（附魔3 扩展批次；消费端见 WeaponEnhance.cs） ----
        /// <summary>射程加成（加法，2 = +2 格；AdjustedRange 补丁消费，仅远程有意义）。长程。</summary>
        public const string WeaponRangeBonus = "WeaponRangeBonus";
        /// <summary>连发弹数加成（加法，2 = +2 发；BurstShotCount 补丁消费，保底 1）。扩容。</summary>
        public const string WeaponBurstShots = "WeaponBurstShots";
        /// <summary>暴击伤害加成（加法，1 = 暴击多追加 1 次等额伤害，即暴击 ×3）。暴虐。</summary>
        public const string WeaponCritDamage = "WeaponCritDamage";
        /// <summary>致盲：命中附加致盲 hediff（视力 -30%，5 秒；值>0 启用）。致盲。</summary>
        public const string WeaponBlinding = "WeaponBlinding";
        /// <summary>蚀甲：命中附加蚀甲 hediff（利刃/钝器护甲 -25%，5 秒；值>0 启用）。蚀甲。</summary>
        public const string WeaponArmorShred = "WeaponArmorShred";
        /// <summary>放血：命中血肉目标附加流血积累（值×0.03 severity/次，仅血肉）。放血。</summary>
        public const string WeaponBleed = "WeaponBleed";
        /// <summary>威慑：命中概率使目标恐慌逃跑（值=概率；跳过倒地/死亡/机械）。威慑。</summary>
        public const string WeaponTerror = "WeaponTerror";
        /// <summary>烟幕：命中概率在落点放烟雾（值=概率；危险特效，受 gizmo/设置门控）。烟幕。</summary>
        public const string WeaponSmoke = "WeaponSmoke";
        /// <summary>屠夫：对血肉目标追加伤害（值=追加倍率，镜像虫敌/EMP）。屠夫。</summary>
        public const string WeaponFleshBane = "WeaponFleshBane";
        /// <summary>破城：对建筑/墙体追加伤害（值=追加倍率；命中目标非 Pawn 时生效）。破城。</summary>
        public const string WeaponDemolition = "WeaponDemolition";
        /// <summary>汲取：远程伤害的百分比转为治疗（值=转化比例；远程吸血，镜像近战吸血）。汲取。</summary>
        public const string WeaponRangedLeech = "WeaponRangedLeech";
        /// <summary>尸爆：击杀时尸体处触发小爆炸（值>0 启用；危险特效，受 gizmo/设置门控）。尸爆。</summary>
        public const string WeaponCorpseBurst = "WeaponCorpseBurst";

        // ---- 防具附魔词条键（防具强化线；值>0 即启用，见 ApparelEnhance.cs） ----
        /// <summary>全护甲加成（乘法，0.10 = 三防再 ×1.10）。强固/神佑。</summary>
        public const string ApparelArmorFactor = "ApparelArmorFactor";
        /// <summary>利刃护甲加成（乘法）。御刃。</summary>
        public const string ApparelArmorSharpFactor = "ApparelArmorSharpFactor";
        /// <summary>钝器护甲加成（乘法）。壁垒。</summary>
        public const string ApparelArmorBluntFactor = "ApparelArmorBluntFactor";
        /// <summary>热能护甲加成（乘法）。御热。</summary>
        public const string ApparelArmorHeatFactor = "ApparelArmorHeatFactor";
        /// <summary>承伤减免（负值，-0.08 = 承伤 -8%，合计上限 -50%）。吸收/神佑。</summary>
        public const string ApparelDamageTakenFactor = "ApparelDamageTakenFactor";
        /// <summary>闪避加成（加法，0.05 = +5%）。灵巧/轻羽。</summary>
        public const string ApparelDodgeChance = "ApparelDodgeChance";
        /// <summary>受击回血：按承伤 × 值 治疗自身伤口（每次上限 2 点）。复苏。</summary>
        public const string ApparelHealOnHit = "ApparelHealOnHit";
        /// <summary>EMP 防护：全额吸收 EMP（含眩晕）。法拉第。</summary>
        public const string ApparelEmpProtection = "ApparelEmpProtection";
        /// <summary>保暖加成（乘法）。保暖。</summary>
        public const string ApparelInsulationColdFactor = "ApparelInsulationColdFactor";
        /// <summary>隔温加成（乘法）。隔温。</summary>
        public const string ApparelInsulationHeatFactor = "ApparelInsulationHeatFactor";
        /// <summary>移速加成（乘法，0.15 = ×1.15）。疾行/轻羽。</summary>
        public const string ApparelMoveSpeed = "ApparelMoveSpeed";
        /// <summary>携带重量加成（加法，15 = +15kg）。负重。</summary>
        public const string ApparelCarryCapacity = "ApparelCarryCapacity";
        /// <summary>全局工作速度加成（乘法，0.10 = ×1.10）。勤勉。</summary>
        public const string ApparelWorkSpeedFactor = "ApparelWorkSpeedFactor";
        /// <summary>反伤：受击反弹承伤 × 值 给攻击者（危险特效，受 gizmo/设置门控）。荆棘。</summary>
        public const string ApparelReflectDamage = "ApparelReflectDamage";
        /// <summary>放电：受击概率 EMP 眩晕近战攻击者 90tick（值=概率；危险特效）。放电。</summary>
        public const string ApparelShockBurst = "ApparelShockBurst";
        /// <summary>毒雾：受击概率落点放毒气（值=概率；危险特效）。毒雾。</summary>
        public const string ApparelToxicCloud = "ApparelToxicCloud";

        // ---- 防具附魔扩展词条（附魔3 扩展批次；StatWorker 补丁 pawn 维度消费，见 ApparelEnhance.cs） ----
        /// <summary>疼痛休克阈值加成（加法，0.10 = +10%）。镇痛。</summary>
        public const string ApparelPainShockThreshold = "ApparelPainShockThreshold";
        /// <summary>射击精度加成（乘法，0.05 = ×1.05）。稳手。</summary>
        public const string ApparelShootingAccuracy = "ApparelShootingAccuracy";
        /// <summary>精神崩溃阈值加成（加法，负值降低阈值更抗压，-0.04 = -4%）。硬撑。</summary>
        public const string ApparelMentalThreshold = "ApparelMentalThreshold";
        /// <summary>心灵敏感度加成（乘法，-0.20 = ×0.80）。灵壁。</summary>
        public const string ApparelPsychicSensitivity = "ApparelPsychicSensitivity";
        /// <summary>伤口愈合速度加成（乘法，0.25 = ×1.25）。再生。</summary>
        public const string ApparelHealingFactor = "ApparelHealingFactor";
        /// <summary>免疫速度加成（乘法，0.15 = ×1.15）。免疫。</summary>
        public const string ApparelImmunitySpeed = "ApparelImmunitySpeed";
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
