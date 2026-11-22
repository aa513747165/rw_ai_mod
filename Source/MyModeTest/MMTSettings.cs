using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// MMT 全局设置（设计方案 §1）：产率/研究成本/传送/强化/爆墙调参与事件机制总开关。
    /// 硬性规则：任何 Incident / GameCondition / 任务 / 据点生成类机制落地时必须同步在此挂开关，
    /// 消费端在入口第一行做廉价 bool 短路；正面/中性默认开，恶意事故类默认关。
    /// 新字段一律带默认值，ExposeData 缺省回退，旧存档安全。
    /// </summary>
    public class MMTSettings : ModSettings
    {
        // —— 资源与研究 ——
        /// <summary>资源产率倍率（仅 MMT_ 前缀配方，postfix GenRecipe.MakeRecipeProducts）。</summary>
        public float resourceYieldFactor = 1f;
        /// <summary>研究成本倍率（仅 MMT_ 前缀研究）。</summary>
        public float researchCostFactor = 1f;

        // —— 强化与装备 ——
        /// <summary>强化成功率倍率（护盾强化与新武器强化共用）。</summary>
        public float enhanceSuccessFactor = 1f;
        /// <summary>仅限 MMT/原版装备强化：勾上后武器/护盾强化 comp 不挂到其它 mod 装备。</summary>
        public bool weaponEnhanceVanillaOnly = false;

        // —— 武器附魔特效（附魔2 借鉴词条；危险类另有单件武器 gizmo 开关） ——
        /// <summary>危险命中特效总开关（爆炸/引雷/毒气/燃烧；与武器 gizmo 两层门控）。</summary>
        public bool weaponHazardEffects = true;
        /// <summary>爆裂词条（命中小爆炸）。</summary>
        public bool enableWeaponExplode = true;
        /// <summary>引雷词条（命中召唤落雷）。</summary>
        public bool enableWeaponStormCall = true;
        /// <summary>毒气词条（命中放毒气/毒性积累）。</summary>
        public bool enableWeaponGas = true;
        /// <summary>压制词条（命中眩晕）。</summary>
        public bool enableWeaponSuppression = true;
        /// <summary>缴械词条（命中打掉目标武器）。</summary>
        public bool enableWeaponDisarm = true;
        /// <summary>剧毒词条（命中毒性积累）。</summary>
        public bool enableWeaponToxic = true;
        /// <summary>必中词条（每发概率必中）。</summary>
        public bool enableWeaponGuaranteedHit = true;
        /// <summary>虫敌词条（对昆虫追加伤害）。</summary>
        public bool enableWeaponInsectBane = true;
        /// <summary>急救词条（命中友军改治疗）。</summary>
        public bool enableWeaponMending = true;
        /// <summary>悟战词条（命中获取武器技能经验）。</summary>
        public bool enableWeaponSkillExp = true;

        // —— 传送 ——
        /// <summary>传送台次数上限加成（0-9）。</summary>
        public int teleportBonusCharges = 0;

        // —— 爆墙炸弹 ——
        /// <summary>爆墙炸弹总开关（关闭=完全不配发不使用）。</summary>
        public bool breachChargeEnabled = true;
        /// <summary>爆墙出现财富门槛（默认 10 万，概率封顶 = 门槛 × 10）。</summary>
        public float breachWealthThreshold = 100000f;
        /// <summary>爆墙概率倍率（0-3，0 = 关闭携带与主动进攻）。</summary>
        public float breachChanceFactor = 1f;

        // —— 事件机制开关 ——
        /// <summary>能量风暴开关。</summary>
        public bool energyStormEnabled = true;
        /// <summary>能量风暴频率倍率。</summary>
        public float energyStormChanceFactor = 1f;
        /// <summary>电磁脉冲日（护盾/充能设备离线事件）。</summary>
        public bool empDayEnabled = true;
        /// <summary>陨石雨（远古合金第二来源）。</summary>
        public bool meteorShowerEnabled = true;
        /// <summary>精英爆破手（财富 50 万+ 专职爆破单位）。</summary>
        public bool eliteBreacherEnabled = true;
        /// <summary>黑市中间人（远古合金收购事件）。</summary>
        public bool blackMarketEnabled = true;
        /// <summary>强化材料赏金任务（定居点委托）。</summary>
        public bool materialQuestsEnabled = true;
        /// <summary>据点生成总开关（扫描仪生成据点及后续据点类型）。</summary>
        public bool explorationSitesEnabled = true;
        /// <summary>奇观超载事故（硬核向，默认关）。</summary>
        public bool wonderOverloadEnabled = false;
        /// <summary>奇观周年庆典。</summary>
        public bool wonderAnniversaryEnabled = true;

        /// <summary>当前生效的设置实例（Mod 构造时注入）。</summary>
        public static MMTSettings Instance
        {
            get { return MyModeTestMod.Settings; }
        }

        // 便捷读取（空安全：补丁可能在设置注入前被触发，回退默认值）
        public static float ResourceYieldFactor
        {
            get { return Instance != null ? Instance.resourceYieldFactor : 1f; }
        }

        public static float ResearchCostFactor
        {
            get { return Instance != null ? Instance.researchCostFactor : 1f; }
        }

        public static int TeleportBonusCharges
        {
            get { return Instance != null ? Instance.teleportBonusCharges : 0; }
        }

        public static bool BreachChargeEnabled
        {
            get { return Instance == null || Instance.breachChargeEnabled; }
        }

        public static bool EnergyStormEnabled
        {
            get { return Instance == null || Instance.energyStormEnabled; }
        }

        public static float EnergyStormChanceFactor
        {
            get { return Instance != null ? Instance.energyStormChanceFactor : 1f; }
        }

        public static bool ExplorationSitesEnabled
        {
            get { return Instance == null || Instance.explorationSitesEnabled; }
        }

        public static bool WeaponEnhanceVanillaOnly
        {
            get { return Instance != null && Instance.weaponEnhanceVanillaOnly; }
        }

        public static bool WeaponHazardEffects
        {
            get { return Instance == null || Instance.weaponHazardEffects; }
        }

        public static bool EnableWeaponExplode
        {
            get { return Instance == null || Instance.enableWeaponExplode; }
        }

        public static bool EnableWeaponStormCall
        {
            get { return Instance == null || Instance.enableWeaponStormCall; }
        }

        public static bool EnableWeaponGas
        {
            get { return Instance == null || Instance.enableWeaponGas; }
        }

        public static bool EnableWeaponSuppression
        {
            get { return Instance == null || Instance.enableWeaponSuppression; }
        }

        public static bool EnableWeaponDisarm
        {
            get { return Instance == null || Instance.enableWeaponDisarm; }
        }

        public static bool EnableWeaponToxic
        {
            get { return Instance == null || Instance.enableWeaponToxic; }
        }

        public static bool EnableWeaponGuaranteedHit
        {
            get { return Instance == null || Instance.enableWeaponGuaranteedHit; }
        }

        public static bool EnableWeaponInsectBane
        {
            get { return Instance == null || Instance.enableWeaponInsectBane; }
        }

        public static bool EnableWeaponMending
        {
            get { return Instance == null || Instance.enableWeaponMending; }
        }

        public static bool EnableWeaponSkillExp
        {
            get { return Instance == null || Instance.enableWeaponSkillExp; }
        }

        /// <summary>强化成功率 × 设置倍率（0-1 截断）；护盾与武器强化共用入口。</summary>
        public static float AdjustEnhanceRate(float baseRate)
        {
            float f = Instance != null ? Instance.enhanceSuccessFactor : 1f;
            return Mathf.Clamp01(baseRate * f);
        }

        /// <summary>一键恢复默认。</summary>
        public void ResetToDefaults()
        {
            resourceYieldFactor = 1f;
            researchCostFactor = 1f;
            enhanceSuccessFactor = 1f;
            weaponEnhanceVanillaOnly = false;
            teleportBonusCharges = 0;
            breachChargeEnabled = true;
            breachWealthThreshold = 100000f;
            breachChanceFactor = 1f;
            energyStormEnabled = true;
            energyStormChanceFactor = 1f;
            empDayEnabled = true;
            meteorShowerEnabled = true;
            eliteBreacherEnabled = true;
            blackMarketEnabled = true;
            materialQuestsEnabled = true;
            explorationSitesEnabled = true;
            wonderOverloadEnabled = false;
            wonderAnniversaryEnabled = true;
            weaponHazardEffects = true;
            enableWeaponExplode = true;
            enableWeaponStormCall = true;
            enableWeaponGas = true;
            enableWeaponSuppression = true;
            enableWeaponDisarm = true;
            enableWeaponToxic = true;
            enableWeaponGuaranteedHit = true;
            enableWeaponInsectBane = true;
            enableWeaponMending = true;
            enableWeaponSkillExp = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref resourceYieldFactor, "resourceYieldFactor", 1f);
            Scribe_Values.Look(ref researchCostFactor, "researchCostFactor", 1f);
            Scribe_Values.Look(ref enhanceSuccessFactor, "enhanceSuccessFactor", 1f);
            Scribe_Values.Look(ref weaponEnhanceVanillaOnly, "weaponEnhanceVanillaOnly", false);
            Scribe_Values.Look(ref teleportBonusCharges, "teleportBonusCharges", 0);
            Scribe_Values.Look(ref breachChargeEnabled, "breachChargeEnabled", true);
            Scribe_Values.Look(ref breachWealthThreshold, "breachWealthThreshold", 100000f);
            Scribe_Values.Look(ref breachChanceFactor, "breachChanceFactor", 1f);
            Scribe_Values.Look(ref energyStormEnabled, "energyStormEnabled", true);
            Scribe_Values.Look(ref energyStormChanceFactor, "energyStormChanceFactor", 1f);
            Scribe_Values.Look(ref empDayEnabled, "empDayEnabled", true);
            Scribe_Values.Look(ref meteorShowerEnabled, "meteorShowerEnabled", true);
            Scribe_Values.Look(ref eliteBreacherEnabled, "eliteBreacherEnabled", true);
            Scribe_Values.Look(ref blackMarketEnabled, "blackMarketEnabled", true);
            Scribe_Values.Look(ref materialQuestsEnabled, "materialQuestsEnabled", true);
            Scribe_Values.Look(ref explorationSitesEnabled, "explorationSitesEnabled", true);
            Scribe_Values.Look(ref wonderOverloadEnabled, "wonderOverloadEnabled", false);
            Scribe_Values.Look(ref wonderAnniversaryEnabled, "wonderAnniversaryEnabled", true);
            Scribe_Values.Look(ref weaponHazardEffects, "weaponHazardEffects", true);
            Scribe_Values.Look(ref enableWeaponExplode, "enableWeaponExplode", true);
            Scribe_Values.Look(ref enableWeaponStormCall, "enableWeaponStormCall", true);
            Scribe_Values.Look(ref enableWeaponGas, "enableWeaponGas", true);
            Scribe_Values.Look(ref enableWeaponSuppression, "enableWeaponSuppression", true);
            Scribe_Values.Look(ref enableWeaponDisarm, "enableWeaponDisarm", true);
            Scribe_Values.Look(ref enableWeaponToxic, "enableWeaponToxic", true);
            Scribe_Values.Look(ref enableWeaponGuaranteedHit, "enableWeaponGuaranteedHit", true);
            Scribe_Values.Look(ref enableWeaponInsectBane, "enableWeaponInsectBane", true);
            Scribe_Values.Look(ref enableWeaponMending, "enableWeaponMending", true);
            Scribe_Values.Look(ref enableWeaponSkillExp, "enableWeaponSkillExp", true);
        }
    }
}
