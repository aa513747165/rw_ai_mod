using System.Collections.Generic;
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
        /// <summary>仅限 MMT/原版防具强化：勾上后防具强化 comp 对其它 mod 防具不生效。</summary>
        public bool apparelEnhanceVanillaOnly = false;

        // —— 随机掉落强化/附魔（RPG 支柱一，mmt-rpg-design.md §1） ——
        /// <summary>随机强化/附魔总开关：袭击者装备生成时掷初始强化等级与附魔词条（关闭不掷点，既有数据保留）。</summary>
        public bool lootRollEnabled = true;
        /// <summary>掷点概率倍率（0-3）：放大非 +0 强化档权重与附魔概率。</summary>
        public float lootRollChanceFactor = 1f;

        // —— 历练/职业（RPG 支柱二，mmt-rpg-design.md §2 + mmt-class-design.md §5.5） ——
        /// <summary>历练/职业总开关：关闭不再获得 XP、gizmo/页签隐藏，既有 Hediff 保留。</summary>
        public bool pawnProgressEnabled = true;
        /// <summary>XP 飘字（性能项，默认关）。</summary>
        public bool xpGainMotes = false;
        /// <summary>维持型光环脉冲间隔 tick（性能项，调大降载）。</summary>
        public int classAuraInterval = 300;
        /// <summary>品质暴击 perk（MMT_Perk_Masterwork）生效开关。</summary>
        public bool classQualityCritEnabled = true;
        /// <summary>历练 XP 全局倍率（0.1-5）。</summary>
        public float xpMultiplier = 1f;
        /// <summary>公会狩猎委托总开关（天赋树任务系统）。</summary>
        public bool talentQuestsEnabled = true;
        /// <summary>公会委托间隔天数（0.5-5）。</summary>
        public float guildQuestIntervalDays = 1f;
        /// <summary>世界委托直建新副本概率（0-1；无既有据点可挂载时按此概率生成扫描仪同池据点）。</summary>
        public float guildQuestDungeonChance = 0.35f;

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

        // —— 武器附魔特效（附魔3 扩展批次） ——
        /// <summary>威慑词条（命中概率恐慌逃跑）。</summary>
        public bool enableWeaponTerror = true;
        /// <summary>烟幕词条（命中落点放烟雾；危险特效）。</summary>
        public bool enableWeaponSmoke = true;
        /// <summary>屠夫词条（对血肉追加伤害）。</summary>
        public bool enableWeaponFleshBane = true;
        /// <summary>破城词条（对建筑追加伤害）。</summary>
        public bool enableWeaponDemolition = true;
        /// <summary>汲取词条（远程吸血）。</summary>
        public bool enableWeaponRangedLeech = true;
        /// <summary>尸爆词条（击杀时尸体爆炸；危险特效）。</summary>
        public bool enableWeaponCorpseBurst = true;

        // —— 防具附魔特效（危险类另有单件防具 gizmo 开关） ——
        /// <summary>危险受击特效总开关（荆棘/放电/毒雾；与防具 gizmo 两层门控）。</summary>
        public bool apparelHazardEffects = true;
        /// <summary>荆棘词条（受击反弹伤害）。</summary>
        public bool enableApparelThorns = true;
        /// <summary>放电词条（受击概率眩晕攻击者）。</summary>
        public bool enableApparelDischarge = true;
        /// <summary>毒雾词条（受击概率放毒气）。</summary>
        public bool enableApparelToxicCloud = true;

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
        /// <summary>贸易商队订单（到访商队随机成员高价收购订单）。</summary>
        public bool tradeOrdersEnabled = true;
        /// <summary>据点生成总开关（扫描仪生成据点及后续据点类型）。</summary>
        public bool explorationSitesEnabled = true;
        /// <summary>奇观超载事故（硬核向，默认关）。</summary>
        public bool wonderOverloadEnabled = false;
        /// <summary>奇观周年庆典。</summary>
        public bool wonderAnniversaryEnabled = true;

        // —— 副本/据点（mmt-dungeon-design.md §5.1；rpg 文档 §4 同表） ——
        /// <summary>任务链开关（rpg 文档 §3.3 入口 Incident 第一行短路；本包实装后生效）。</summary>
        public bool rpgQuestsEnabled = true;
        /// <summary>Boss 巢穴开关（玩家主动触发型，默认开）。</summary>
        public bool bossEncountersEnabled = true;
        /// <summary>副本派系黑名单（defName 逗号分隔，默认空）。</summary>
        public string dungeonFactionBlacklist = "";
        /// <summary>副本炮塔黑名单（defName 逗号分隔，排除个别不平衡的 mod 炮塔）。</summary>
        public string dungeonTurretBlacklist = "";
        /// <summary>档位成本倍率（0.5-3）：整体缩放三档声纳模块/扫描天数成本。</summary>
        public float dungeonTierCostFactor = 1f;
        /// <summary>玩家自建副本蓝图（跨存档全局；恢复默认设置不清除——蓝图是内容不是调参）。</summary>
        public List<DungeonBlueprint> dungeonBlueprints = new List<DungeonBlueprint>();

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

        public static bool EliteBreacherEnabled
        {
            get { return Instance == null || Instance.eliteBreacherEnabled; }
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

        public static bool EnableWeaponTerror
        {
            get { return Instance == null || Instance.enableWeaponTerror; }
        }

        public static bool EnableWeaponSmoke
        {
            get { return Instance == null || Instance.enableWeaponSmoke; }
        }

        public static bool EnableWeaponFleshBane
        {
            get { return Instance == null || Instance.enableWeaponFleshBane; }
        }

        public static bool EnableWeaponDemolition
        {
            get { return Instance == null || Instance.enableWeaponDemolition; }
        }

        public static bool EnableWeaponRangedLeech
        {
            get { return Instance == null || Instance.enableWeaponRangedLeech; }
        }

        public static bool EnableWeaponCorpseBurst
        {
            get { return Instance == null || Instance.enableWeaponCorpseBurst; }
        }

        public static bool ApparelEnhanceVanillaOnly
        {
            get { return Instance != null && Instance.apparelEnhanceVanillaOnly; }
        }

        public static bool LootRollEnabled
        {
            get { return Instance == null || Instance.lootRollEnabled; }
        }

        public static float LootRollChanceFactor
        {
            get { return Instance != null ? Instance.lootRollChanceFactor : 1f; }
        }

        public static bool PawnProgressEnabled
        {
            get { return Instance == null || Instance.pawnProgressEnabled; }
        }

        public static int ClassAuraInterval
        {
            get { return Instance != null ? Mathf.Max(60, Instance.classAuraInterval) : 300; }
        }

        public static bool ClassQualityCritEnabled
        {
            get { return Instance == null || Instance.classQualityCritEnabled; }
        }

        /// <summary>历练 XP 全局倍率（AddXp 统一乘算口）。</summary>
        public static float XpMultiplier
        {
            get { return Instance != null ? Mathf.Clamp(Instance.xpMultiplier, 0.1f, 5f) : 1f; }
        }

        public static bool TalentQuestsEnabled
        {
            get { return Instance == null || Instance.talentQuestsEnabled; }
        }

        public static float GuildQuestIntervalDays
        {
            get { return Instance != null ? Mathf.Clamp(Instance.guildQuestIntervalDays, 0.5f, 5f) : 1f; }
        }

        public static float GuildQuestDungeonChance
        {
            get { return Instance != null ? Mathf.Clamp01(Instance.guildQuestDungeonChance) : 0.35f; }
        }

        public static bool ApparelHazardEffects
        {
            get { return Instance == null || Instance.apparelHazardEffects; }
        }

        public static bool EnableApparelThorns
        {
            get { return Instance == null || Instance.enableApparelThorns; }
        }

        public static bool EnableApparelDischarge
        {
            get { return Instance == null || Instance.enableApparelDischarge; }
        }

        public static bool EnableApparelToxicCloud
        {
            get { return Instance == null || Instance.enableApparelToxicCloud; }
        }

        public static bool TradeOrdersEnabled
        {
            get { return Instance == null || Instance.tradeOrdersEnabled; }
        }

        // —— 副本/据点便捷读取 ——

        public static bool RpgQuestsEnabled
        {
            get { return Instance == null || Instance.rpgQuestsEnabled; }
        }

        public static bool BossEncountersEnabled
        {
            get { return Instance == null || Instance.bossEncountersEnabled; }
        }

        public static float DungeonTierCostFactor
        {
            get { return Instance != null ? Mathf.Clamp(Instance.dungeonTierCostFactor, 0.5f, 3f) : 1f; }
        }

        // 黑名单解析缓存：按 raw 字符串变化自动失效重解析（避免每次查询都 Split+建集合）
        private static string factionBlacklistRaw;
        private static HashSet<string> factionBlacklistSet;
        private static string turretBlacklistRaw;
        private static HashSet<string> turretBlacklistSet;

        /// <summary>派系是否在副本黑名单内（defName 逗号分隔，大小写不敏感）。</summary>
        public static bool DungeonFactionBlacklisted(string defName)
        {
            if (defName == null)
                return false;
            string raw = Instance != null ? Instance.dungeonFactionBlacklist : null;
            if (raw != factionBlacklistRaw)
            {
                factionBlacklistRaw = raw;
                factionBlacklistSet = ParseDefNameList(raw);
            }
            return factionBlacklistSet.Contains(defName);
        }

        /// <summary>炮塔是否在黑名单内（defName 逗号分隔，大小写不敏感）。</summary>
        public static bool DungeonTurretBlacklisted(string defName)
        {
            if (defName == null)
                return false;
            string raw = Instance != null ? Instance.dungeonTurretBlacklist : null;
            if (raw != turretBlacklistRaw)
            {
                turretBlacklistRaw = raw;
                turretBlacklistSet = ParseDefNameList(raw);
            }
            return turretBlacklistSet.Contains(defName);
        }

        /// <summary>逗号分隔 defName 列表 → 集合（空安全）。</summary>
        private static HashSet<string> ParseDefNameList(string raw)
        {
            HashSet<string> set = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (raw.NullOrEmpty())
                return set;
            string[] parts = raw.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string s = parts[i].Trim();
                if (s.Length > 0)
                    set.Add(s);
            }
            return set;
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
            tradeOrdersEnabled = true;
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
            enableWeaponTerror = true;
            enableWeaponSmoke = true;
            enableWeaponFleshBane = true;
            enableWeaponDemolition = true;
            enableWeaponRangedLeech = true;
            enableWeaponCorpseBurst = true;
            apparelEnhanceVanillaOnly = false;
            lootRollEnabled = true;
            lootRollChanceFactor = 1f;
            pawnProgressEnabled = true;
            xpGainMotes = false;
            classAuraInterval = 300;
            classQualityCritEnabled = true;
            xpMultiplier = 1f;
            talentQuestsEnabled = true;
            guildQuestIntervalDays = 1f;
            guildQuestDungeonChance = 0.35f;
            apparelHazardEffects = true;
            enableApparelThorns = true;
            enableApparelDischarge = true;
            enableApparelToxicCloud = true;
            rpgQuestsEnabled = true;
            bossEncountersEnabled = true;
            dungeonFactionBlacklist = "";
            dungeonTurretBlacklist = "";
            dungeonTierCostFactor = 1f;
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
            Scribe_Values.Look(ref tradeOrdersEnabled, "tradeOrdersEnabled", true);
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
            Scribe_Values.Look(ref enableWeaponTerror, "enableWeaponTerror", true);
            Scribe_Values.Look(ref enableWeaponSmoke, "enableWeaponSmoke", true);
            Scribe_Values.Look(ref enableWeaponFleshBane, "enableWeaponFleshBane", true);
            Scribe_Values.Look(ref enableWeaponDemolition, "enableWeaponDemolition", true);
            Scribe_Values.Look(ref enableWeaponRangedLeech, "enableWeaponRangedLeech", true);
            Scribe_Values.Look(ref enableWeaponCorpseBurst, "enableWeaponCorpseBurst", true);
            Scribe_Values.Look(ref apparelEnhanceVanillaOnly, "apparelEnhanceVanillaOnly", false);
            Scribe_Values.Look(ref lootRollEnabled, "lootRollEnabled", true);
            Scribe_Values.Look(ref lootRollChanceFactor, "lootRollChanceFactor", 1f);
            Scribe_Values.Look(ref pawnProgressEnabled, "pawnProgressEnabled", true);
            Scribe_Values.Look(ref xpGainMotes, "xpGainMotes", false);
            Scribe_Values.Look(ref classAuraInterval, "classAuraInterval", 300);
            Scribe_Values.Look(ref classQualityCritEnabled, "classQualityCritEnabled", true);
            Scribe_Values.Look(ref xpMultiplier, "xpMultiplier", 1f);
            Scribe_Values.Look(ref talentQuestsEnabled, "talentQuestsEnabled", true);
            Scribe_Values.Look(ref guildQuestIntervalDays, "guildQuestIntervalDays", 1f);
            Scribe_Values.Look(ref guildQuestDungeonChance, "guildQuestDungeonChance", 0.35f);
            Scribe_Values.Look(ref apparelHazardEffects, "apparelHazardEffects", true);
            Scribe_Values.Look(ref enableApparelThorns, "enableApparelThorns", true);
            Scribe_Values.Look(ref enableApparelDischarge, "enableApparelDischarge", true);
            Scribe_Values.Look(ref enableApparelToxicCloud, "enableApparelToxicCloud", true);
            Scribe_Values.Look(ref rpgQuestsEnabled, "rpgQuestsEnabled", true);
            Scribe_Values.Look(ref bossEncountersEnabled, "bossEncountersEnabled", true);
            Scribe_Values.Look(ref dungeonFactionBlacklist, "dungeonFactionBlacklist", "");
            Scribe_Values.Look(ref dungeonTurretBlacklist, "dungeonTurretBlacklist", "");
            Scribe_Values.Look(ref dungeonTierCostFactor, "dungeonTierCostFactor", 1f);
            Scribe_Collections.Look(ref dungeonBlueprints, "dungeonBlueprints", LookMode.Deep);
            if (dungeonBlueprints == null)
                dungeonBlueprints = new List<DungeonBlueprint>();
        }
    }
}
