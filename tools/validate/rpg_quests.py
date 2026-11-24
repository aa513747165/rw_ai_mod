#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""副本/据点静态验证（mmt-dungeon-design.md §5.2）。

校验点：
- 扫描仪三档成本表字段完整（模块数 > 0、天数/威胁系数递增、品质地板合法）；
- 据点池条目引用有效、Boss 巢穴不进随机池；
- 奖励池条目 defName 有效、minTier/maxTier ∈ {1,2,3} 且 minTier ≤ maxTier；
- DungeonLayoutExtension 引用的建筑 defName 在原版 + MMT 白名单内（Boss 白名单允许预留缺失）；
- Boss 巢穴池化字段（bossBudgetFraction > 0、bossCountRange、bossPawnKindDefNames）齐全；
- 历练书/凭证碎片/合成配方/设置项/C# 关键实现接线齐全。
"""
from __future__ import annotations

import re
from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Exploration.xml"
DUNGEON_XML = "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_RpgDungeons.xml"
SITES_XML = "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_Exploration.xml"
ITEMS_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_DungeonLoot.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Dungeon.xml"
BENCH_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_WeaponEnhance.xml"
DUNGEON_CS = "Source/MyModeTest/Dungeon.cs"
EXPLORATION_CS = "Source/MyModeTest/Exploration.cs"
PROGRESS_CS = "Source/MyModeTest/PawnProgress.cs"
SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"
MOD_CS = "Source/MyModeTest/MyModeTestMod.cs"
QUEST_XML = "1.7/Defs/QuestScriptDefs/MMT_Quests_RpgChain.xml"
QUESTCHAIN_CS = "Source/MyModeTest/RpgQuestChain.cs"

DUNGEON_PARTS = ("MMT_SitePart_TrainingGround", "MMT_SitePart_ClassShrine", "MMT_SitePart_BossLair")

# 布局扩展默认清单只允许原版 + MMT def（§4.2：不预置第三方 mod 引用）
VANILLA_LAYOUT_WHITELIST = {"Wall", "Door", "Sandbags", "TorchLamp"}

QUALITY_ORDER = ["Awful", "Poor", "Normal", "Good", "Excellent", "Masterwork", "Legendary"]


def _read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def _all_mmt_defnames() -> set[str]:
    """扫全部 Def XML 收集 defName（用于奖励池条目有效性检查）。"""
    names: set[str] = set()
    for path in Path("1.7/Defs").rglob("*.xml"):
        for m in re.finditer(r"<defName>(\w+)</defName>", _read(str(path))):
            names.add(m.group(1))
    return names


def _layout_ok(name: str, mmt_defs: set[str]) -> bool:
    return name in VANILLA_LAYOUT_WHITELIST or (name in mmt_defs and name.startswith("MMT_"))


def validate_rpg_quests() -> None:
    require_files([BUILDING_XML, DUNGEON_XML, SITES_XML, ITEMS_XML, RECIPE_XML,
                   DUNGEON_CS, EXPLORATION_CS, PROGRESS_CS, SETTINGS_CS, MOD_CS,
                   QUEST_XML, QUESTCHAIN_CS])
    mmt_defs = _all_mmt_defnames()

    # ---- 三档成本表（§1.1/§5.2） ----
    buildings = def_map(parse_xml(BUILDING_XML))
    scanner = buildings.get("MMT_DeepSpaceScanner")
    if scanner is None:
        fail("缺少深空扫描仪 MMT_DeepSpaceScanner")
    comp = None
    for li in scanner.findall("comps/li"):
        if li.attrib.get("Class", "").endswith("CompProperties_DeepSpaceScanner"):
            comp = li
    if comp is None:
        fail("扫描仪缺少 CompProperties_DeepSpaceScanner")
    tiers = comp.findall("scanTiers/li")
    if len(tiers) != 3:
        fail(f"扫描档位必须恰好 3 档，实际 {len(tiers)}")
    prev_days, prev_threat = 0.0, 0.0
    for i, t in enumerate(tiers, start=1):
        assert_equal(child_text(t, "tier"), str(i), f"档位 {i} 编号")
        modules = int(child_text(t, "sonarModules") or "0")
        if modules <= 0:
            fail(f"档位 {i} 声纳模块数必须 > 0")
        days = float(child_text(t, "scanDaysFactor") or "0")
        threat = float(child_text(t, "threatFactor") or "0")
        if days <= prev_days or threat <= prev_threat:
            fail(f"档位 {i} 天数/威胁系数必须随档递增（days={days}, threat={threat}）")
        prev_days, prev_threat = days, threat
        assert_equal(child_text(t, "lootTier"), str(i), f"档位 {i} lootTier")
        qmin, qmax = child_text(t, "gearQualityMin"), child_text(t, "gearQualityMax")
        if qmin not in QUALITY_ORDER or qmax not in QUALITY_ORDER:
            fail(f"档位 {i} 品质档位非法: {qmin}~{qmax}")
        if QUALITY_ORDER.index(qmin) > QUALITY_ORDER.index(qmax):
            fail(f"档位 {i} 品质地板高于天花板: {qmin} > {qmax}")

    # ---- 据点池（§0：Boss 巢穴低概率在池，受设置门控） ----
    pool = comp.findall("sitePool/li")
    if len(pool) < 5:
        fail(f"据点池条目不足（应 ≥5：旧三类 + 试炼场/职业圣殿），实际 {len(pool)}")
    pool_parts = {}
    total_weight = 0.0
    for li in pool:
        part = child_text(li, "sitePart")
        if not part:
            fail("据点池条目缺少 sitePart")
        if part not in mmt_defs:
            fail(f"据点池引用了不存在的 SitePartDef: {part}")
        weight = float(child_text(li, "weight") or "0")
        if weight <= 0:
            fail(f"据点池 {part} 权重必须 > 0")
        total_weight += weight
        if not child_text(li, "label"):
            fail(f"据点池 {part} 缺少 label")
        pool_parts[part] = li
    boss = pool_parts.get("MMT_SitePart_BossLair")
    if boss is None:
        fail("据点池缺少 Boss 巢穴（MMT_SitePart_BossLair 应低概率在池）")
    else:
        if child_text(boss, "requiresBossEncounters") != "true":
            fail("Boss 巢穴池条目必须 requiresBossEncounters=true（受设置门控）")
        if child_text(boss, "factionGuards") != "true":
            fail("Boss 巢穴必须 factionGuards=true（宿主派系守卫）")
        bw = float(child_text(boss, "weight") or "0")
        if total_weight > 0 and bw / total_weight > 0.08:
            fail(f"Boss 巢穴应为低概率结果（权重占比 ≤8%），实际 {bw / total_weight:.1%}")
    for part in ("MMT_SitePart_TrainingGround", "MMT_SitePart_ClassShrine"):
        if part not in pool_parts:
            fail(f"据点池缺少 {part}")
        if child_text(pool_parts[part], "factionGuards") != "true":
            fail(f"{part} 必须 factionGuards=true（宿主派系守卫）")

    # ---- 副本据点 Def ----
    dungeons = def_map(parse_xml(DUNGEON_XML))
    for name in DUNGEON_PARTS:
        if name not in dungeons:
            fail(f"缺少副本据点 {name}")
            continue
        node = dungeons[name]
        assert_equal(child_text(node, "workerClass"), "MyModeTest.SitePartWorker_MMT_Dungeon",
                     f"{name} workerClass")
        assert_equal(child_text(node, "wantsThreatPoints"), "true", f"{name} wantsThreatPoints")
        for tex_key in ("siteTexture", "expandingIconTexture"):
            tex = child_text(node, tex_key)
            if tex and not Path("1.7/Textures/" + tex + ".png").exists():
                fail(f"{name} 缺少世界图标 {tex}")

        # ---- 布局扩展（§4.2 白名单） ----
        layout = None
        reward = None
        for li in node.findall("modExtensions/li"):
            cls = li.attrib.get("Class", "")
            if cls.endswith("DungeonLayoutExtension"):
                layout = li
            elif cls.endswith("StockpileRewardExtension"):
                reward = li
        if layout is None:
            fail(f"{name} 缺少 DungeonLayoutExtension")
        else:
            for field in ("wallDefName", "doorDefName"):
                v = layout.findtext(field)
                if v and not _layout_ok(v.strip(), mmt_defs):
                    fail(f"{name} 布局 {field} 不在原版/MMT 白名单: {v}")
            for li in layout.findall("coverDefs/li") + layout.findall("lightDefs/li"):
                v = child_text(li, "defName")
                if not _layout_ok(v, mmt_defs):
                    fail(f"{name} 掩体/照明不在白名单: {v}")
                if float(child_text(li, "weight") or "0") <= 0:
                    fail(f"{name} 掩体/照明 {v} 权重必须 > 0")
            for li in layout.findall("turretDefs/*"):
                if not _layout_ok((li.text or "").strip(), mmt_defs):
                    fail(f"{name} 炮塔白名单不在原版/MMT 白名单: {li.text}")
            # Boss 白名单允许预留缺失（Boss 后续自建，C# 惰性解析静默跳过；池化选取见 §4.7）
            for li in layout.findall("bossPawnKindDefNames/li"):
                if not (li.text or "").strip():
                    fail(f"{name} Boss 白名单存在空条目")
            if name == "MMT_SitePart_BossLair":
                fraction = float(layout.findtext("bossBudgetFraction") or "0")
                if fraction <= 0:
                    fail("Boss 巢穴必须 bossBudgetFraction > 0（Boss 预算占威胁点比例）")
                count_range = layout.findtext("bossCountRange") or ""
                m = re.match(r"^(\d+)~(\d+)$", count_range.strip())
                if not m or int(m.group(1)) < 1 or int(m.group(2)) < int(m.group(1)):
                    fail(f"Boss 巢穴 bossCountRange 非法: {count_range}")
                if not layout.findall("bossPawnKindDefNames/li"):
                    fail("Boss 巢穴应保留 bossPawnKindDefNames 白名单（MMT 后续 Boss 登记口）")

        # ---- 奖励池（§3.1/§5.2） ----
        if reward is None:
            fail(f"{name} 缺少 StockpileRewardExtension")
            continue
        entries = reward.findall("tieredRewards/li")
        if not entries:
            fail(f"{name} 缺少分档奖励条目 tieredRewards")
        for li in entries:
            ref = child_text(li, "thingDefName")
            if ref not in mmt_defs:
                fail(f"{name} 奖励条目引用无效 defName: {ref}")
            min_tier = int(li.findtext("minTier") or "1")
            max_tier = int(li.findtext("maxTier") or "3")
            if min_tier not in (1, 2, 3) or max_tier not in (1, 2, 3) or min_tier > max_tier:
                fail(f"{name} 奖励条目 {ref} 档位区间非法: {min_tier}~{max_tier}")
        for li in reward.findall("gearRewards/li"):
            min_tier = int(li.findtext("minTier") or "1")
            max_tier = int(li.findtext("maxTier") or "3")
            if min_tier not in (1, 2, 3) or max_tier not in (1, 2, 3) or min_tier > max_tier:
                fail(f"{name} 装备箱条目档位区间非法: {min_tier}~{max_tier}")
    # 职业圣殿必出凭证碎片（§3.2）
    shrine_reward_refs = [child_text(li, "thingDefName")
                          for li in dungeons["MMT_SitePart_ClassShrine"]
                          .findall("modExtensions/li/tieredRewards/li")]
    if "MMT_ClassTokenFragment" not in shrine_reward_refs:
        fail("职业圣殿奖励池必须包含职业凭证碎片")

    # ---- 旧三类据点的分档加成（§3.1 静态堆升级） ----
    sites = def_map(parse_xml(SITES_XML))
    for name in ("MMT_SitePart_RareDeposit", "MMT_SitePart_OrbitalRuinCache", "MMT_SitePart_StrongholdCache"):
        node = sites.get(name)
        if node is None:
            fail(f"缺少据点 {name}")
            continue
        for li in node.findall("modExtensions/li/tieredRewards/li"):
            ref = child_text(li, "thingDefName")
            if ref not in mmt_defs:
                fail(f"{name} 分档加成引用无效 defName: {ref}")

    # ---- 新物品（历练书/凭证碎片） ----
    items = def_map(parse_xml(ITEMS_XML))
    for name, tex in (("MMT_XpBook", "Things/Item/MMT/MMT_XpBook"),
                      ("MMT_ClassTokenFragment", "Things/Item/MMT/MMT_ClassTokenFragment")):
        if name not in items:
            fail(f"缺少物品 {name}")
            continue
        if not Path(f"1.7/Textures/{tex}.png").exists():
            fail(f"{name} 缺少贴图 {tex}")
    xp = items["MMT_XpBook"]
    doer = xp.find("ingestible/outcomeDoers/li")
    if doer is None or doer.attrib.get("Class") != "MyModeTest.IngestionOutcomeDoer_GiveProgressXp":
        fail("历练书缺少 IngestionOutcomeDoer_GiveProgressXp")
    else:
        assert_equal(child_text(doer, "xpAmount"), "500", "历练书 XP")

    # ---- 碎片合成配方（3 → 1，挂装备强化台） ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_CraftClassToken" not in recipes:
        fail("缺少配方 MMT_CraftClassToken")
    else:
        recipe = recipes["MMT_CraftClassToken"]
        count = child_text(recipe, "ingredients/li/count")
        assert_equal(count, "3", "凭证配方碎片数量")
        assert_equal(child_text(recipe, "products/MMT_ClassToken"), "1", "凭证配方产出")
    bench = _read(BENCH_XML)
    if "<li>MMT_CraftClassToken</li>" not in bench:
        fail("MMT_WeaponEnhanceBench 未登记 MMT_CraftClassToken 配方")

    # ---- Boss 召唤器（dev 测试工具） ----
    SUMMONER_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Dungeon.xml"
    SUMMONER_CS = "Source/MyModeTest/BossSummoner.cs"
    require_files([SUMMONER_XML, SUMMONER_CS])
    buildings2 = def_map(parse_xml(SUMMONER_XML))
    if "MMT_BossSummoner" not in buildings2:
        fail("缺少 Boss 召唤器 MMT_BossSummoner")
    else:
        summoner = buildings2["MMT_BossSummoner"]
        comp2 = [li.attrib.get("Class", "") for li in summoner.findall("comps/li")]
        if "MyModeTest.CompProperties_BossSummoner" not in comp2:
            fail("Boss 召唤器缺少 CompProperties_BossSummoner")
        names = [li.text for li in summoner.findall("comps/li/bossPawnKindDefNames/li")]
        if not names or "MMT_BossAncientGuardian" not in names:
            fail("Boss 召唤器候选列表应预留 MMT_BossAncientGuardian")
        threshold = None
        for li in summoner.findall("comps/li"):
            if li.attrib.get("Class", "").endswith("CompProperties_BossSummoner"):
                threshold = li.findtext("autoBossMinCombatPower")
        if threshold is None or float(threshold) <= 0:
            fail("Boss 召唤器缺少 autoBossMinCombatPower（自动扫描战力阈值）")
        if not Path("1.7/Textures/Things/Building/Production/MMT_BossSummoner.png").exists():
            fail("Boss 召唤器缺少贴图")
    summoner_cs = _read(SUMMONER_CS)
    for token in ("CompBossSummoner", "Patch_BossSummonerDevOnly", "Prefs.DevMode",
                  "LordJob_AssaultColony", "BeginTargeting", "Designator_Build",
                  "BossPool", "bossPawnKindDefNames", "autoBossMinCombatPower",
                  "combatPower", "ManhunterPermanent", "modContentPack"):
        if token not in summoner_cs:
            fail(f"BossSummoner.cs 缺少: {token}")

    # ---- Boss 池（§4.7：原版/Mod 全库扫描 + 白名单，按财富预算选取、可多只） ----
    BOSSPOOL_CS = "Source/MyModeTest/BossPool.cs"
    require_files([BOSSPOOL_CS])
    bosspool_cs = _read(BOSSPOOL_CS)
    for token in ("BossPool", "AutoScanMinCombatPower", "Candidates", "PickBosses",
                  "ResolveBossFaction", "isFighter", "combatPower", "GetNamedSilentFail",
                  "RandomElementByWeight", "IsAnomalyEntity", "OfMechanoids"):
        if token not in bosspool_cs:
            fail(f"BossPool.cs 缺少: {token}")

    # ---- 设置项接线（§5.1） ----
    settings = _read(SETTINGS_CS)
    for token in ("dungeonFactionBlacklist", "dungeonTurretBlacklist", "dungeonTierCostFactor",
                  "rpgQuestsEnabled", "bossEncountersEnabled",
                  "DungeonFactionBlacklisted", "DungeonTurretBlacklisted",
                  "DungeonTierCostFactor", "BossEncountersEnabled", "RpgQuestsEnabled"):
        if token not in settings:
            fail(f"MMTSettings.cs 缺少: {token}")
    for token in ('Scribe_Values.Look(ref dungeonFactionBlacklist',
                  'Scribe_Values.Look(ref dungeonTurretBlacklist',
                  'Scribe_Values.Look(ref dungeonTierCostFactor'):
        if token not in settings:
            fail(f"MMTSettings.ExposeData 缺少: {token}")
    mod = _read(MOD_CS)
    for token in ("dungeonFactionBlacklist", "dungeonTurretBlacklist", "dungeonTierCostFactor",
                  "rpgQuestsEnabled", "bossEncountersEnabled"):
        if token not in mod:
            fail(f"设置面板缺少: {token}")

    # ---- C# 关键实现 ----
    dungeon = _read(DUNGEON_CS)
    for token in ("SitePartWorker_MMT_Dungeon", "DungeonLayoutExtension", "DungeonSiteGen",
                  "GenerateDungeonSite", "GenerateStockpileSite", "PickHostFaction",
                  "TryGetRandomFactionForCombatPawnGroup", "TryPickTurret", "GenStep_Power",
                  "LordJob_SitePawns", "TrySpawnBoss", "bossPawnKindDefNames",
                  "bossBudgetFraction", "bossCountRange", "BossPool.PickBosses",
                  "SampleStuffFromGuards", "DungeonGearGen", "DungeonRewardGen",
                  "randomValue", "modContentPack"):
        if token not in dungeon:
            fail(f"Dungeon.cs 缺少: {token}")
    exploration = _read(EXPLORATION_CS)
    for token in ("ScanTier", "ScanSiteEntry", "scanTiers", "sitePool", "OpenTierMenu",
                  "GetTierConfig", "FloatMenu", "gearQualityMin", "gearQualityMax"):
        if token not in exploration:
            fail(f"Exploration.cs 缺少: {token}")
    progress = _read(PROGRESS_CS)
    if "IngestionOutcomeDoer_GiveProgressXp" not in progress:
        fail("PawnProgress.cs 缺少 IngestionOutcomeDoer_GiveProgressXp")

    # ---- 任务链「远古军械库」（rpg 文档 §3.3） ----
    quest_defs = def_map(parse_xml(QUEST_XML))
    qnode = quest_defs.get("MMT_Quest_AncientArmory")
    if qnode is None:
        fail("缺少任务链 QuestScriptDef MMT_Quest_AncientArmory")
    else:
        assert_equal(child_text(qnode, "autoAccept"), "true", "任务链 autoAccept")
        assert_equal(child_text(qnode, "rootSelectionWeight"), "0", "任务链 rootSelectionWeight（不走随机任务池）")
        root = qnode.find("root")
        if root is None or root.get("Class") != "MyModeTest.QuestNode_Root_MMTAncientArmory":
            fail("任务链 root 应为 MyModeTest.QuestNode_Root_MMTAncientArmory")
    chain = _read(QUESTCHAIN_CS)
    for token in ("QuestPart_MMTAncientArmory", "GameComponent_RpgQuestChain", "QuestNode_Root_MMTAncientArmory",
                  "NotifyChainEnded", "NotifyScanCompleted", "CountFragments", "GenerateDungeonSite",
                  "DropPodUtility", "ExposeData", "FragmentNeed", "CooldownTicks"):
        if token not in chain:
            fail(f"RpgQuestChain.cs 缺少: {token}")
    if "GameComponent_RpgQuestChain.Current?.NotifyScanCompleted" not in exploration:
        fail("Exploration.cs 缺少任务链扫描触发钩子 NotifyScanCompleted")

    print("rpg_quests: OK")


SUITES = {"rpg_quests": validate_rpg_quests}
