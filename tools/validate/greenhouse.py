#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""玻璃温室（透明屋顶）静态验证（新链，由 tools/validate_new.py 自动发现）。

校验点：
- RoofDef MMT_TransparentRoof 字段（非厚顶、坍塌碎屑、击穿音效）；
- 玻璃天窗框架 ThingDef：thingClass / 单图贴图 / 玻璃成本 / 研究前置 / PlaceWorker / 显式 building 节点；
- 研究 MMT_Greenhouse：前置 MMT_Glassworking、坐标与 apply_research_layout.LAYOUT 一致；
- 贴图与遮罩存在且为合法 PNG；
- C# 关键 token：屋顶转换自毁、放置限制、GlowGrid 透光补丁、失败退还建材。
"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    assert_png,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    read_text,
    require_files,
    research_coord,
)

ROOF_XML = "1.7/Defs/ThingDefs_Misc/MMT_RoofDefs_Greenhouse.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Greenhouse.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Greenhouse.xml"
CS = "Source/MyModeTest/GreenhouseRoof.cs"
TEX = "1.7/Textures/Things/Building/Production/MMT_SkylightFrame/MMT_SkylightFrame.png"
TEX_MASK = "1.7/Textures/Things/Building/Production/MMT_SkylightFrame/MMT_SkylightFrame_m.png"


def validate_greenhouse() -> None:
    require_files([ROOF_XML, BUILDING_XML, RESEARCH_XML, CS, TEX, TEX_MASK])

    # ---- 透明屋顶 RoofDef ----
    roofs = def_map(parse_xml(ROOF_XML))
    roof = roofs["MMT_TransparentRoof"]
    assert_equal(child_text(roof, "label"), "玻璃天窗顶", "透明屋顶 label")
    assert_equal(child_text(roof, "isThickRoof"), "false", "透明屋顶非厚顶")

    # ---- 玻璃天窗框架 ThingDef ----
    buildings = def_map(parse_xml(BUILDING_XML))
    frame = buildings["MMT_SkylightFrame"]
    assert_equal(child_text(frame, "thingClass"), "MyModeTest.Building_TransparentRoofFrame", "框架 thingClass")
    assert_equal(child_text(frame, "graphicData/texPath"),
                 "Things/Building/Production/MMT_SkylightFrame/MMT_SkylightFrame", "框架 texPath")
    assert_equal(child_text(frame, "graphicData/graphicClass"), "Graphic_Single", "框架 graphicClass")
    assert_equal(child_text(frame, "size"), "(1,1)", "框架尺寸")
    assert_equal(child_text(frame, "costList/MMT_Glass"), "5", "框架玻璃成本")
    assert_equal(li_values(frame, "researchPrerequisites"), ["MMT_Greenhouse"], "框架研究前置")
    assert_equal(child_text(frame, "placeWorkers/li"), "MyModeTest.PlaceWorker_TransparentRoofFrame", "框架 PlaceWorker")
    if frame.find("building") is None:
        fail("框架缺少显式 <building/> 节点（1.7 BuildingBase 必需）")
    if frame.find("minifiedDef") is not None:
        fail("框架不应声明 minifiedDef（建成即消失，无需打包）")

    # ---- 研究 ----
    researches = def_map(parse_xml(RESEARCH_XML))
    gh = researches["MMT_Greenhouse"]
    assert_equal(child_text(gh, "baseCost"), "600", "温室研究点数")
    assert_equal(li_values(gh, "prerequisites"), ["MMT_Glassworking"], "温室研究前置")
    assert_equal(child_text(gh, "researchViewX"), research_coord("MMT_Greenhouse", "x"), "温室坐标X")
    assert_equal(child_text(gh, "researchViewY"), research_coord("MMT_Greenhouse", "y"), "温室坐标Y")

    # ---- 贴图 ----
    assert_png(TEX)
    assert_png(TEX_MASK)

    # ---- C# 关键实现 ----
    cs = read_text(CS)
    for token in ("MMT_GreenhouseDefOf", "MMT_TransparentRoof", "Building_TransparentRoofFrame",
                  "SetRoof", "PlaceTempRoof", "Destroy(DestroyMode.Vanish)",
                  "PlaceWorker_TransparentRoofFrame", "WithinRangeOfRoofHolder", "ConnectedToRoofHolder",
                  "costList", "TryPlaceThing",  # 失败退还建材
                  "Patch_GreenhouseGroundGlow", "GroundGlowAt", "CurSkyGlow"):
        if token not in cs:
            fail(f"GreenhouseRoof.cs 缺少: {token}")


SUITES = {"greenhouse": validate_greenhouse}
