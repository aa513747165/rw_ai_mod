#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""自动种植机链静态验证：5x5 自动种植机 + 农业 II 研究（新链，由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

import glob
import re

from validate_mod import (
    assert_equal,
    assert_png,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    require_files,
)

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AutoFarm.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Agriculture.xml"
CS_FILE = "Source/MyModeTest/AutoFarm.cs"

TEX_EAST = "1.7/Textures/Things/Building/Production/MMT_AutoFarm/MMT_AutoFarm_east.png"
TEX_NORTH = "1.7/Textures/Things/Building/Production/MMT_AutoFarm/MMT_AutoFarm_north.png"
TEX_SOUTH = "1.7/Textures/Things/Building/Production/MMT_AutoFarm/MMT_AutoFarm_south.png"


def _comp_nodes(building):
    comps = building.find("comps")
    return [] if comps is None else comps.findall("li")


def _find_comp(building, class_name: str):
    for li in _comp_nodes(building):
        if li.get("Class") == class_name:
            return li
    return None


def validate_autofarm() -> None:
    require_files([BUILDING_XML, RESEARCH_XML, CS_FILE, TEX_EAST, TEX_NORTH, TEX_SOUTH])
    assert_png(TEX_EAST)
    assert_png(TEX_NORTH)
    assert_png(TEX_SOUTH)

    # ---- 建筑：MMT_AutoFarm ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_AutoFarm" not in buildings:
        fail("建筑缺少 MMT_AutoFarm")
    farm = buildings["MMT_AutoFarm"]

    assert_equal(child_text(farm, "thingClass"), "MyModeTest.Building_AutoFarm", "thingClass")
    assert_equal(child_text(farm, "size"), "(5,5)", "size")
    assert_equal(child_text(farm, "graphicData/texPath"),
                 "Things/Building/Production/MMT_AutoFarm/MMT_AutoFarm", "texPath")
    assert_equal(child_text(farm, "graphicData/drawSize"), "(5,5)", "drawSize")
    assert_equal(child_text(farm, "tickerType"), "Normal", "tickerType")
    assert_equal(child_text(farm, "designationCategory"), "MMT_Production", "designationCategory")
    assert_equal(child_text(farm, "hasInteractionCell"), "true", "hasInteractionCell")
    # 出料口必须在 5x5 占地外（中心格外扩 3 格），否则产物会被 Near 散落到机身上
    assert_equal(child_text(farm, "interactionCellOffset"), "(0,0,-3)", "interactionCellOffset")
    if "MMT_AutoFarming" not in li_values(farm, "researchPrerequisites"):
        fail("研究前置缺少 MMT_AutoFarming")

    # comps：电力 / 开关 / 故障 / 燃料舱（肥料）
    if _find_comp(farm, "CompProperties_Power") is None:
        fail("缺少 CompProperties_Power")
    if _find_comp(farm, "CompProperties_Flickable") is None:
        fail("缺少 CompProperties_Flickable")
    fuel = _find_comp(farm, "CompProperties_Refuelable")
    if fuel is None:
        fail("缺少燃料舱 CompProperties_Refuelable")
    if "MMT_Fertilizer" not in li_values(fuel, "fuelFilter/thingDefs"):
        fail("燃料舱 fuelFilter 必须包含 MMT_Fertilizer")
    capacity = float(child_text(fuel, "fuelCapacity"))
    rate = float(child_text(fuel, "fuelConsumptionRate"))
    if capacity <= 0 or rate <= 0:
        fail("燃料舱容量与消耗速率必须为正")

    # ---- 研究：MMT_AutoFarming ----
    researches = def_map(parse_xml(RESEARCH_XML))
    if "MMT_AutoFarming" not in researches:
        fail("研究缺少 MMT_AutoFarming")
    proj = researches["MMT_AutoFarming"]
    prereqs = li_values(proj, "prerequisites")
    for need in ("MMT_AgricultureBasic", "MicroelectronicsBasics"):
        if need not in prereqs:
            fail(f"MMT_AutoFarming 前置缺少 {need}")
    assert_equal(child_text(proj, "tab"), "MMT_ResearchTab", "研究 tab")

    # 研究坐标全局唯一（与全部研究 Def 比对）
    x = child_text(proj, "researchViewX")
    y = child_text(proj, "researchViewY")
    for path in glob.glob("1.7/Defs/ResearchProjectDefs/*.xml"):
        text = open(path, encoding="utf-8").read()
        for m in re.finditer(
            r"<defName>(\w+)</defName>.*?researchViewX>([\d.]+)</researchViewX>\s*"
            r"<researchViewY>([\d.]+)</researchViewY>", text, re.S):
            name, ox, oy = m.group(1), m.group(2), m.group(3)
            if name != "MMT_AutoFarming" and float(ox) == float(x) and float(oy) == float(y):
                fail(f"研究坐标 ({x},{y}) 与 {path} 的 {name} 冲突")


SUITES = {"autofarm": validate_autofarm}
