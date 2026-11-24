using HarmonyLib;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// myModeTest 的 Mod 入口，确认程序集被 RimWorld 加载并注册 Harmony 补丁。
    /// 附带全局设置面板（设计方案 §1，单页滚动）。
    /// </summary>
    public sealed class MyModeTestMod : Mod
    {
        /// <summary>全局设置实例（构造时注入，MMTSettings.Instance 走这里）。</summary>
        public static MMTSettings Settings { get; private set; }

        // 设置窗口滚动位置（不进存档，会话内保持即可）
        private Vector2 settingsScrollPos = Vector2.zero;

        public MyModeTestMod(ModContentPack content) : base(content)
        {
            // 流程开始：Mod 构造函数由游戏调用，先取设置（各消费端经 MMTSettings.Instance 读取），再应用全部 Harmony 补丁。
            Settings = GetSettings<MMTSettings>();
            Log.Message("[myModeTest] Mod loaded.");
            new Harmony("MyModeTest.Harmony").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "MMT 资源生产";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            MMTSettings s = Settings;
            if (s == null) return;

            // 单页滚动：内容高度按行数估算（附魔特效分组后约 40 行 × 34 + 分组间距）
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, 2200f);
            Widgets.BeginScrollView(inRect, ref settingsScrollPos, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.maxOneColumn = true;
            list.Begin(viewRect);

            // —— 资源与研究 ——
            list.Label("【资源与研究】");
            s.resourceYieldFactor = list.SliderLabeled(
                "资源产率倍率 ×" + s.resourceYieldFactor.ToString("F2") + "（仅 MMT 配方）",
                s.resourceYieldFactor, 0.5f, 3f, 0.6f, "MMT 全部生产配方的产出数量倍率。");
            s.researchCostFactor = list.SliderLabeled(
                "研究成本倍率 ×" + s.researchCostFactor.ToString("F2") + "（仅 MMT 研究）",
                s.researchCostFactor, 0.25f, 2f, 0.6f, "MMT 全部研究项目的点数成本倍率。");
            list.Gap();

            // —— 强化与装备 ——
            list.Label("【强化与装备】");
            s.enhanceSuccessFactor = list.SliderLabeled(
                "强化成功率倍率 ×" + s.enhanceSuccessFactor.ToString("F2"),
                s.enhanceSuccessFactor, 0.5f, 2f, 0.6f, "护盾/武器强化的成功率倍率（0.5-2.0）。");
            list.CheckboxLabeled("仅限 MMT/原版装备强化（其它 mod 武器/护盾不开放强化）", ref s.weaponEnhanceVanillaOnly,
                "勾上后强化 comp 不挂到其它 mod 的武器与护盾装备。");
            list.CheckboxLabeled("仅限 MMT/原版防具强化（其它 mod 防具不开放强化）", ref s.apparelEnhanceVanillaOnly,
                "勾上后防具强化 comp 对其它 mod 的防具不生效。");
            list.Gap();

            // —— 随机掉落强化/附魔（RPG 支柱一） ——
            list.Label("【随机掉落强化/附魔】（袭击者装备生成时掷强化等级与附魔词条，威胁点越高掷得越好）");
            list.CheckboxLabeled("启用随机强化/附魔掉落", ref s.lootRollEnabled,
                "关闭后袭击者装备不再掷点（纯原版白板）；已掷出的装备数据保留。");
            s.lootRollChanceFactor = list.SliderLabeled(
                "掷点概率倍率 ×" + s.lootRollChanceFactor.ToString("F2"),
                s.lootRollChanceFactor, 0f, 3f, 0.6f, "放大非 +0 强化档权重与附魔概率（0 = 全部白板）。");
            list.Gap();

            // —— 历练/职业（RPG 支柱二） ——
            list.Label("【历练/职业】（小人历练等级、强化点与 RoM 职业体系）");
            list.CheckboxLabeled("启用历练/职业系统", ref s.pawnProgressEnabled,
                "关闭后不再获得历练 XP、强化 gizmo/页签隐藏；既有 Hediff 与已选强化保留。");
            list.CheckboxLabeled("XP 飘字（性能项，默认关）", ref s.xpGainMotes,
                "获得历练 XP 时在小人头顶飘字；命中密集时有视觉/分配开销，低配建议保持关闭。");
            s.classAuraInterval = Mathf.RoundToInt(list.SliderLabeled(
                "职业光环脉冲间隔 " + s.classAuraInterval + " tick",
                s.classAuraInterval, 60f, 1200f, 0.6f, "维持型光环（抚慰之风/指挥官光环等）刷新间隔，调大降载（默认 300）。"));
            list.CheckboxLabeled("品质暴击强化（制作成品 10% 概率品质 +1 档）", ref s.classQualityCritEnabled);
            s.xpMultiplier = list.SliderLabeled(
                "历练 XP 倍率 ×" + s.xpMultiplier.ToString("F2"),
                s.xpMultiplier, 0.1f, 5f, 0.6f, "全源历练经验统一倍率（0.1-5）。");
            list.CheckboxLabeled("公会狩猎委托（每日狩猎任务，奖励含职业凭证/能量结晶）", ref s.talentQuestsEnabled,
                "关闭后不再生成新的狩猎委托（进行中的不受影响）。");
            s.guildQuestIntervalDays = list.SliderLabeled(
                "公会委托间隔 " + s.guildQuestIntervalDays.ToString("F1") + " 天",
                s.guildQuestIntervalDays, 0.5f, 5f, 0.6f, "两次狩猎委托之间的间隔天数（0.5-5）。");
            s.guildQuestDungeonChance = list.SliderLabeled(
                "委托直建新副本概率 " + s.guildQuestDungeonChance.ToString("P0"),
                s.guildQuestDungeonChance, 0f, 1f, 0.6f, "世界委托无既有据点可挂载时，按此概率直接生成一处新据点（与深空扫描同池同规则），清剿即完成委托并拿据点战利品。");
            list.Gap();

            // —— 副本/据点（mmt-dungeon-design.md §5.1） ——
            list.Label("【副本/据点】（深空扫描三档选档、宿主派系守卫与程序化据点布局）");
            list.CheckboxLabeled("任务链开关（远古军械库三幕）", ref s.rpgQuestsEnabled,
                "关闭后任务链入口不再触发（进行中的任务不受影响）。");
            list.CheckboxLabeled("Boss 巢穴（远古守卫巢穴）", ref s.bossEncountersEnabled,
                "玩家主动触发型终局挑战，默认开。");
            s.dungeonTierCostFactor = list.SliderLabeled(
                "档位成本倍率 ×" + s.dungeonTierCostFactor.ToString("F2"),
                s.dungeonTierCostFactor, 0.5f, 3f, 0.6f, "整体缩放三档扫描的声纳模块消耗与扫描天数（0.5-3）。");
            list.Label("副本派系黑名单（defName，逗号分隔；列出的派系不会成为据点宿主）：");
            s.dungeonFactionBlacklist = Widgets.TextField(list.GetRect(24f), s.dungeonFactionBlacklist ?? "");
            list.Label("副本炮塔黑名单（defName，逗号分隔；排除个别不平衡的 mod 炮塔）：");
            s.dungeonTurretBlacklist = Widgets.TextField(list.GetRect(24f), s.dungeonTurretBlacklist ?? "");
            list.Gap();

            // —— 武器附魔特效（附魔2 借鉴词条；危险类另有单件武器 gizmo 开关） ——
            list.Label("【武器附魔特效】（关闭后对应词条不生效，模块仍可制作安装）");
            list.CheckboxLabeled("危险命中特效总开关（爆炸/引雷/毒气/燃烧/烟幕/尸爆）", ref s.weaponHazardEffects,
                "总保险丝：关闭后全部危险命中特效不触发（防误伤/引燃基地）；单件武器还可用 gizmo 单独关闭。");
            list.CheckboxLabeled("爆裂（命中小爆炸）", ref s.enableWeaponExplode);
            list.CheckboxLabeled("引雷（命中召唤落雷）", ref s.enableWeaponStormCall);
            list.CheckboxLabeled("毒气（命中放毒气/毒性积累）", ref s.enableWeaponGas);
            list.CheckboxLabeled("压制（命中眩晕）", ref s.enableWeaponSuppression);
            list.CheckboxLabeled("缴械（命中打掉目标武器）", ref s.enableWeaponDisarm);
            list.CheckboxLabeled("剧毒（命中毒性积累）", ref s.enableWeaponToxic);
            list.CheckboxLabeled("必中（每发概率必中）", ref s.enableWeaponGuaranteedHit);
            list.CheckboxLabeled("虫敌（对昆虫追加伤害）", ref s.enableWeaponInsectBane);
            list.CheckboxLabeled("急救（命中友军改治疗）", ref s.enableWeaponMending);
            list.CheckboxLabeled("悟战（命中获取武器技能经验）", ref s.enableWeaponSkillExp);
            // 附魔3 扩展批次
            list.CheckboxLabeled("威慑（命中概率恐慌逃跑）", ref s.enableWeaponTerror);
            list.CheckboxLabeled("烟幕（命中落点放烟雾）", ref s.enableWeaponSmoke);
            list.CheckboxLabeled("屠夫（对血肉追加伤害）", ref s.enableWeaponFleshBane);
            list.CheckboxLabeled("破城（对建筑追加伤害）", ref s.enableWeaponDemolition);
            list.CheckboxLabeled("汲取（远程吸血）", ref s.enableWeaponRangedLeech);
            list.CheckboxLabeled("尸爆（击杀时尸体爆炸）", ref s.enableWeaponCorpseBurst);
            list.Gap();

            // —— 防具附魔特效（危险类另有单件防具 gizmo 开关） ——
            list.Label("【防具附魔特效】（关闭后对应词条不生效，模块仍可制作安装）");
            list.CheckboxLabeled("危险受击特效总开关（荆棘/放电/毒雾）", ref s.apparelHazardEffects,
                "总保险丝：关闭后全部危险受击特效不触发（防误伤友军）；单件防具还可用 gizmo 单独关闭。");
            list.CheckboxLabeled("荆棘（受击反弹伤害）", ref s.enableApparelThorns);
            list.CheckboxLabeled("放电（受击概率眩晕攻击者）", ref s.enableApparelDischarge);
            list.CheckboxLabeled("毒雾（受击概率放毒气）", ref s.enableApparelToxicCloud);
            list.Gap();

            // —— 传送 ——
            list.Label("【传送】");
            s.teleportBonusCharges = Mathf.RoundToInt(list.SliderLabeled(
                "传送次数上限加成 +" + s.teleportBonusCharges,
                s.teleportBonusCharges, 0f, 9f, 0.6f, "传送台可用次数上限额外增加（0-9）。"));
            list.Gap();

            // —— 爆墙炸弹 ——
            list.Label("【爆墙炸弹】");
            list.CheckboxLabeled("启用爆墙炸弹（袭击者财富门槛配发）", ref s.breachChargeEnabled,
                "关闭后袭击者不再携带/使用爆墙炸弹。");
            s.breachWealthThreshold = Mathf.Round(list.SliderLabeled(
                "爆墙出现财富门槛 " + (s.breachWealthThreshold / 10000f).ToString("F0") + " 万",
                s.breachWealthThreshold, 50000f, 500000f, 0.6f, "殖民地财富达到该值后袭击者开始携带爆墙炸弹（5万-50万）。") / 10000f) * 10000f;
            s.breachChanceFactor = list.SliderLabeled(
                "爆墙概率倍率 ×" + s.breachChanceFactor.ToString("F2") + "（0=不出现）",
                s.breachChanceFactor, 0f, 3f, 0.6f, "携带/主动使用概率的倍率（0-3）。");
            list.Gap();

            // —— 事件机制开关 ——
            list.Label("【事件机制开关】（关闭后不触发新事件，进行中的不受影响）");
            list.CheckboxLabeled("能量风暴（财富 20 万+）", ref s.energyStormEnabled,
                "太阳/风能归零 + 未防护电子设备故障点名的天气级威胁。");
            s.energyStormChanceFactor = list.SliderLabeled(
                "能量风暴频率倍率 ×" + s.energyStormChanceFactor.ToString("F2"),
                s.energyStormChanceFactor, 0.5f, 3f, 0.6f, "能量风暴触发频率倍率（0.5-3）。");
            list.CheckboxLabeled("电磁脉冲日（护盾/充能设备离线 12-24h）", ref s.empDayEnabled);
            list.CheckboxLabeled("陨石雨（坑内可挖远古合金）", ref s.meteorShowerEnabled);
            list.CheckboxLabeled("精英爆破手（财富 50 万+ 专职爆破单位）", ref s.eliteBreacherEnabled);
            list.CheckboxLabeled("黑市中间人（高价收购远古合金）", ref s.blackMarketEnabled);
            list.CheckboxLabeled("强化材料赏金任务（定居点委托）", ref s.materialQuestsEnabled);
            list.CheckboxLabeled("深空扫描据点生成", ref s.explorationSitesEnabled,
                "关闭后扫描仪不生成新据点（含后续据点类型）。");
            list.CheckboxLabeled("奇观超载事故（硬核向，默认关）", ref s.wonderOverloadEnabled,
                "蓄电超载时小概率爆炸事故。");
            list.CheckboxLabeled("奇观周年庆典（激活纪念日节庆）", ref s.wonderAnniversaryEnabled);
            list.Gap();

            if (list.ButtonText("恢复默认设置"))
                s.ResetToDefaults();

            list.End();
            Widgets.EndScrollView();
        }
    }
}
