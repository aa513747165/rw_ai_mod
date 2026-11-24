#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""世界探索静态验证：扫描仪/三类据点/独占资源/贴图。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Exploration.xml"
SITES_XML = "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_Exploration.xml"
RES_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Exploration.xml"
CS = "Source/MyModeTest/Exploration.cs"
DUNGEON_CS = "Source/MyModeTest/Dungeon.cs"

SITES = {
    "MMT_SitePart_RareDeposit": ["MMT_AncientAlloy"],
    "MMT_SitePart_OrbitalRuinCache": ["MMT_ArchotechShard", "MMT_AncientAlloy"],
    "MMT_SitePart_StrongholdCache": ["MMT_ArchotechShard", "MMT_AncientDatacore"],
}


def validate_exploration() -> None:
    require_files([BUILDING_XML, SITES_XML, RES_XML, CS])

    # ---- 扫描仪 ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_DeepSpaceScanner" not in buildings:
        fail("缺少深空扫描仪 MMT_DeepSpaceScanner")
    scanner = buildings["MMT_DeepSpaceScanner"]
    assert_equal(child_text(scanner, "size"), "(3,3)", "扫描仪尺寸")
    comp_classes = [li.attrib.get("Class", "") for li in scanner.findall("comps/li")]
    for need in ("MyModeTest.CompProperties_BuildingBattery", "MyModeTest.CompProperties_ModuleSlots",
                 "MyModeTest.CompProperties_DeepSpaceScanner"):
        if need not in comp_classes:
            fail(f"扫描仪缺少 comp {need}")
    assert_equal(child_text(scanner, "costList/MMT_MineralSonarModule"), "10", "扫描仪声纳模块造价")
    # 同建筑不得重复挂载蓄电 comp（CLAUDE.md 硬约束）
    if comp_classes.count("MyModeTest.CompProperties_BuildingBattery") != 1:
        fail("扫描仪蓄电 comp 必须恰好一个")

    # ---- 三类据点 ----
    sites = def_map(parse_xml(SITES_XML))
    for name, reward_defs in SITES.items():
        if name not in sites:
            fail(f"缺少据点 SitePart {name}")
            continue
        node = sites[name]
        assert_equal(child_text(node, "workerClass"), "MyModeTest.SitePartWorker_MMT_Stockpile", f"{name} workerClass")
        # ThingDefCountRangeClass 自定义加载格式：节点名=thingDef、文本=countRange
        rewards = [li.tag for li in node.findall("modExtensions/li/rewards/*")]
        for rd in reward_defs:
            if rd not in rewards:
                fail(f"{name} 奖励缺少 {rd}")
        site_tex = child_text(node, "siteTexture")
        if site_tex and not Path("1.7/Textures/" + site_tex + ".png").exists():
            fail(f"{name} 缺少世界图标 {site_tex}")

    # ---- 独占资源 ----
    res = def_map(parse_xml(RES_XML))
    for name, value in (("MMT_ArchotechShard", "90"), ("MMT_AncientDatacore", "150"), ("MMT_AncientAlloy", "35")):
        if name not in res:
            fail(f"缺少独占资源 {name}")
            continue
        assert_equal(child_text(res[name], "statBases/MarketValue"), value, f"{name} 价值")
        if not Path(f"1.7/Textures/Things/Item/MMT/{name}.png").exists():
            fail(f"{name} 缺少贴图")
    assert_equal(child_text(res["MMT_AncientDatacore"], "tradeability"), "None", "数据核心必须不可交易（强制闭环）")

    # ---- C# 关键实现（据点生成部分已迁入 Dungeon.cs，跨两文件检查） ----
    cs = Path(CS).read_text(encoding="utf-8") + Path(DUNGEON_CS).read_text(encoding="utf-8")
    for token in ("CompDeepSpaceScanner", "SitePartWorker_MMT_Stockpile", "PostMapGenerate",
                  "MakeSite", "TryFindNewSiteTile", "StartTimeout", "CompTickInterval",
                  "ExplorationSitesEnabled", "StockpileRewardExtension"):
        if token not in cs:
            fail(f"Exploration.cs/Dungeon.cs 缺少: {token}")

    # ---- 扫描仪贴图（单方向，不可旋转） ----
    tex = "1.7/Textures/Things/Building/Production/MMT_DeepSpaceScanner.png"
    if not Path(tex).exists():
        fail(f"扫描仪缺少贴图 {tex}")
    scanner = buildings["MMT_DeepSpaceScanner"]
    assert_equal(child_text(scanner, "graphicData/graphicClass"), "Graphic_Single", "扫描仪 graphicClass")
    assert_equal(child_text(scanner, "rotatable"), "false", "扫描仪必须不可旋转")

    print("exploration: OK")


SUITES = {"exploration": validate_exploration}
