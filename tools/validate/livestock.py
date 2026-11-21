#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""畜牧机器链静态验证：畜牧机器建筑 + 研究 + 作业（新链，由 tools/validate_new.py 自动发现）。"""
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

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Livestock.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Livestock.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
MODULES_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_ProcessorModules.xml"

CS_FILE = "Source/MyModeTest/LivestockMachine.cs"

TEX_DIR = "1.7/Textures/Things/Building/Production/MMT_LivestockMachine"
TEX_INNER = [f"{TEX_DIR}/MMT_LivestockMachine_Inner_{d}.png" for d in ("south", "north", "east")]
TEX_SHELL = [f"{TEX_DIR}/MMT_LivestockMachine_Shell_{d}.png" for d in ("south", "north", "east")]


def _comp_classes(node) -> list[str]:
    comps = node.find("comps")
    if comps is None:
        return []
    return [li.get("Class") or (li.findtext("compClass") or "") for li in comps.findall("li")]


def validate_livestock() -> None:
    require_files([BUILDING_XML, RESEARCH_XML, JOBS_XML, MODULES_XML, CS_FILE] + TEX_INNER + TEX_SHELL)
    for tex in TEX_INNER + TEX_SHELL:
        assert_png(tex)

    # ---- 建筑：畜牧机器 ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_LivestockMachine" not in buildings:
        fail("建筑缺少 MMT_LivestockMachine")
    machine = buildings["MMT_LivestockMachine"]
    assert_equal(child_text(machine, "thingClass"), "MyModeTest.Building_LivestockMachine", "畜牧机器 thingClass")
    assert_equal(child_text(machine, "drawerType"), "MapMeshAndRealTime", "畜牧机器 drawerType")
    assert_equal(child_text(machine, "size"), "(2,2)", "畜牧机器尺寸")
    assert_equal(child_text(machine, "graphicData/texPath"),
                 "Things/Building/Production/MMT_LivestockMachine/MMT_LivestockMachine_Inner", "舱内层贴图")
    assert_equal(child_text(machine, "rotatable"), "true", "畜牧机器可旋转")
    assert_equal(child_text(machine, "tickerType"), "Normal", "畜牧机器 tickerType")
    # 可打包(minifiedDef)即可搬运，必须有显式 Mass
    if child_text(machine, "statBases/Mass") is None:
        fail("畜牧机器缺少显式 Mass（可打包建筑必须提供）")
    assert_equal(child_text(machine, "designationCategory"), "MMT_Production", "畜牧机器建造分类")
    if li_values(machine, "researchPrerequisites") != ["MMT_LivestockTech"]:
        fail("畜牧机器研究前置必须是 MMT_LivestockTech")
    if machine.find("building") is None:
        fail("畜牧机器缺少显式 <building/> 标签（1.7 BuildingBase 不自带 BuildingProperties）")
    comps = _comp_classes(machine)
    for comp in ("CompProperties_Flickable", "CompProperties_Power", "CompProperties_Refuelable",
                 "MyModeTest.CompProperties_LivestockMachine", "MyModeTest.CompProperties_ModuleSlots"):
        if comp not in comps:
            fail(f"畜牧机器缺少 comp: {comp}")
    # 模块槽上限：超频/增值各 5
    slots_node = None
    for li in machine.find("comps").findall("li"):
        if li.get("Class") == "MyModeTest.CompProperties_ModuleSlots":
            slots_node = li
    slot_limits = {
        item.findtext("key"): item.findtext("max")
        for item in slots_node.find("slotLimits").findall("li")
    }
    if slot_limits.get("Overclock") != "5" or slot_limits.get("Yield") != "5":
        fail(f"畜牧机器模块槽上限应为 Overclock=5、Yield=5，实际 {slot_limits}")
    # 饲料仓：不自动烧（无 fuelConsumptionRate 或显式 0），含饲料过滤器
    fuel_node = None
    for li in machine.find("comps").findall("li"):
        if li.get("Class") == "CompProperties_Refuelable":
            fuel_node = li
    rate = fuel_node.findtext("fuelConsumptionRate")
    if rate is not None and float(rate) != 0.0:
        fail(f"畜牧机器饲料仓不应自动消耗（fuelConsumptionRate={rate}）")
    fuel_defs = li_values(fuel_node, "fuelFilter/thingDefs")
    for need in ("Hay", "Kibble"):
        if need not in fuel_defs:
            fail(f"饲料仓过滤器缺少: {need}")
    fuel_cats = li_values(fuel_node, "fuelFilter/categories")
    if "PlantFoodRaw" not in fuel_cats:
        fail("饲料仓过滤器缺少 PlantFoodRaw 类别")
    tabs = li_values(machine, "inspectorTabs")
    if "MyModeTest.ITab_ModuleSlots" not in tabs:
        fail("畜牧机器缺少 ITab_ModuleSlots 页签")

    # ---- 研究 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_LivestockTech" not in research:
        fail("研究缺少 MMT_LivestockTech")
    node = research["MMT_LivestockTech"]
    if "MMT_AutoBoost" not in li_values(node, "prerequisites"):
        fail("畜牧自动化前置必须是 MMT_AutoBoost")
    assert_equal(child_text(node, "researchViewX"), research_coord("MMT_LivestockTech", "x"), "研究坐标X")
    assert_equal(child_text(node, "researchViewY"), research_coord("MMT_LivestockTech", "y"), "研究坐标Y")

    # ---- JobDef ----
    jobs = def_map(parse_xml(JOBS_XML))
    # 放入为右键直选直接传送（无 JobDef）；放出为作业
    if "MMT_LoadLivestockMachine" in jobs:
        fail("放入动物已改直接传送，JobDefs 不应再有 MMT_LoadLivestockMachine")
    unload = jobs.get("MMT_UnloadLivestockMachine")
    if unload is None:
        fail("JobDefs 缺少 MMT_UnloadLivestockMachine")
    else:
        assert_equal(child_text(unload, "driverClass"), "MyModeTest.JobDriver_UnloadLivestockMachine",
                     "MMT_UnloadLivestockMachine driverClass")

    # ---- C# 关键实现 ----
    cs = read_text(CS_FILE)
    for token in (
        "Building_LivestockMachine",
        "CompProperties_LivestockMachine",
        "CompLivestockMachine",
        "LivestockProductSource",
        "TeleportIntoMachine",
        "JobDriver_UnloadLivestockMachine",
        "MMT_LivestockDefOf",
        "MMT_LivestockMachine_Shell",
        "CompHasGatherableBodyResource",
        "CompMilkable",
        "CompEggLayer",
        "meatDef",
        "FoodFallPerTickAssumingCategory",
        "ModuleEffectKeys.SpeedFactor",
        "ModuleEffectKeys.YieldBonus",
        "maxBodySizeForFullScale",
        "Faction.OfPlayer",
    ):
        if token not in cs:
            fail(f"LivestockMachine.cs 缺少: {token}")


SUITES = {"livestock": validate_livestock}
