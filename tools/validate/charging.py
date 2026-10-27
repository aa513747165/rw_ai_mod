#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""充能链静态验证：充能台 + 充能护盾腰带（新链，由 tools/validate_new.py 自动发现）。"""
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

ITEM_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ChargeShield.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ChargingStation.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ChargeShield.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Charging.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
CATEGORY_XML = "1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml"
PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"

CS_FILES = [
    "Source/MyModeTest/Rechargeable.cs",
    "Source/MyModeTest/ChargingStation.cs",
    "Source/MyModeTest/ShieldPack.cs",
]

TEX_STATION = "1.7/Textures/Things/Building/Misc/MMT_ChargingStation.png"
TEX_BEAM = "1.7/Textures/Things/Building/Misc/MMT_ChargeBeam.png"
TEX_BELT = "1.7/Textures/Things/Item/MMT/Shield/MMT_ShieldBeltTech.png"
TEX_BUBBLE = "1.7/Textures/Things/Item/MMT/Shield/MMT_ShieldBubbleTech.png"


def _comp_classes(node) -> list[str]:
    comps = node.find("comps")
    if comps is None:
        return []
    return [li.get("Class") or (li.findtext("compClass") or "") for li in comps.findall("li")]


def validate_charging() -> None:
    require_files(
        [ITEM_XML, BUILDING_XML, RECIPE_XML, RESEARCH_XML, JOBS_XML, CATEGORY_XML]
        + CS_FILES
        + [TEX_STATION, TEX_BEAM]
    )
    assert_png(TEX_STATION)
    assert_png(TEX_BEAM)

    # ---- 物品：充能护盾腰带 ----
    items = def_map(parse_xml(ITEM_XML))
    if "MMT_ChargeShieldPack" not in items:
        fail("物品缺少 MMT_ChargeShieldPack")
    pack = items["MMT_ChargeShieldPack"]
    assert_equal(child_text(pack, "label"), "充能护盾腰带", "护盾腰带 label")
    assert_equal(
        child_text(pack, "graphicData/texPath"),
        "Things/Item/MMT/Shield/MMT_ShieldBeltTech",
        "护盾腰带贴图",
    )
    require_files([TEX_BELT, TEX_BUBBLE])
    assert_png(TEX_BELT)
    assert_png(TEX_BUBBLE)
    assert_equal(child_text(pack, "tickerType"), "Normal", "护盾腰带 tickerType")
    assert_equal(child_text(pack, "thingClass"), "MyModeTest.Apparel_ChargeShieldPack", "护盾腰带 thingClass")
    pack_cats = li_values(pack, "thingCategories")
    if "MMT_RechargeableDevices" not in pack_cats:
        fail("护盾腰带需归入 MMT_RechargeableDevices 分类")
    pack_comps = _comp_classes(pack)
    for comp in ("MyModeTest.CompProperties_Rechargeable", "MyModeTest.CompProperties_ShieldPack"):
        if comp not in pack_comps:
            fail(f"护盾腰带缺少 comp: {comp}")
    layers = li_values(pack, "apparel/layers")
    if "Belt" not in layers:
        fail("护盾腰带必须是腰带层（Belt）")

    # ---- 分类 ----
    cats = def_map(parse_xml(CATEGORY_XML))
    if "MMT_RechargeableDevices" not in cats:
        fail("ThingCategories 缺少 MMT_RechargeableDevices")
    assert_equal(
        child_text(cats["MMT_RechargeableDevices"], "parent"), "ItemsMisc", "可充能设备分类 parent"
    )

    # ---- 建筑：充能台 ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_ChargingStation" not in buildings:
        fail("建筑缺少 MMT_ChargingStation")
    station = buildings["MMT_ChargingStation"]
    assert_equal(
        child_text(station, "thingClass"), "MyModeTest.Building_ChargingStation", "充能台 thingClass"
    )
    assert_equal(child_text(station, "drawerType"), "MapMeshAndRealTime", "充能台 drawerType")
    assert_equal(child_text(station, "size"), "(1,1)", "充能台尺寸")
    # 可打包(minifiedDef)即可搬运，必须有显式 Mass，否则游戏报 "is haulable, but does not have an authored mass value"
    assert_equal(child_text(station, "statBases/Mass"), "25", "充能台质量")
    assert_equal(
        child_text(station, "designationCategory"), "MMT_Production", "充能台建造分类"
    )
    if li_values(station, "researchPrerequisites") != ["MMT_RechargeableTech"]:
        fail("充能台研究前置必须是 MMT_RechargeableTech")
    if station.find("building") is None:
        fail("充能台缺少显式 <building/> 标签（1.7 BuildingBase 不自带 BuildingProperties）")
    station_comps = _comp_classes(station)
    for comp in ("CompProperties_Flickable", "CompProperties_Power", "MyModeTest.CompProperties_ChargingStation"):
        if comp not in station_comps:
            fail(f"充能台缺少 comp: {comp}")

    # ---- 配方 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_MakeChargeShieldPack" not in recipes:
        fail("配方缺少 MMT_MakeChargeShieldPack")
    recipe = recipes["MMT_MakeChargeShieldPack"]
    assert_equal(child_text(recipe, "researchPrerequisite"), "MMT_RechargeableTech", "配方研究前置")
    products = recipe.find("products")
    if products is None or products.findtext("MMT_ChargeShieldPack") != "1":
        fail("配方 products 必须产出 MMT_ChargeShieldPack x1")
    filter_defs = li_values(recipe, "fixedIngredientFilter/thingDefs")
    for need in ("MMT_IntegratedCircuit", "ComponentSpacer", "Plasteel", "MMT_Wire"):
        if need not in filter_defs:
            fail(f"配方缺少原料: {need}")
    # 挂电动装配台
    parts_buildings = def_map(parse_xml(PARTS_BUILDING_XML))
    bench_recipes = li_values(parts_buildings["MMT_ElectricAssemblyBench"], "recipes")
    if "MMT_MakeChargeShieldPack" not in bench_recipes:
        fail("电动装配台 recipes 缺少 MMT_MakeChargeShieldPack")

    # ---- 研究 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_RechargeableTech" not in research:
        fail("研究缺少 MMT_RechargeableTech")
    node = research["MMT_RechargeableTech"]
    if "MMT_IntegratedCircuitTech" not in li_values(node, "prerequisites"):
        fail("充能技术前置必须是 MMT_IntegratedCircuitTech")
    assert_equal(child_text(node, "researchViewX"), research_coord("MMT_RechargeableTech", "x"), "研究坐标X")
    assert_equal(child_text(node, "researchViewY"), research_coord("MMT_RechargeableTech", "y"), "研究坐标Y")

    # ---- JobDef ----
    jobs = def_map(parse_xml(JOBS_XML))
    for job_def, driver in (
        ("MMT_LoadChargingStation", "MyModeTest.JobDriver_LoadChargingStation"),
        ("MMT_UnloadChargingStation", "MyModeTest.JobDriver_UnloadChargingStation"),
    ):
        if job_def not in jobs:
            fail(f"JobDefs 缺少 {job_def}")
        assert_equal(child_text(jobs[job_def], "driverClass"), driver, f"{job_def} driverClass")

    # ---- C# 关键实现 ----
    rechargeable_cs = read_text(CS_FILES[0])
    for token in ("CompProperties_Rechargeable", "CompRechargeable", "AddCharge", "Drain"):
        if token not in rechargeable_cs:
            fail(f"Rechargeable.cs 缺少: {token}")
    station_cs = read_text(CS_FILES[1])
    for token in (
        "Building_ChargingStation",
        "CompProperties_ChargingStation",
        "CompChargingStation",
        "JobDriver_LoadChargingStation",
        "JobDriver_UnloadChargingStation",
        "CompFloatMenuOptions",
        "MMT_ChargeBeam",
        "MMT_ChargingDefOf",
    ):
        if token not in station_cs:
            fail(f"ChargingStation.cs 缺少: {token}")
    shield_cs = read_text(CS_FILES[2])
    for token in (
        "CompProperties_ShieldPack",
        "CompShieldPack",
        "Apparel_ChargeShieldPack",
        "DrawWornExtras",
        '"GetGizmos"',
        '"PreApplyDamage"',
        '"set_Drafted"',
    ):
        if token not in shield_cs:
            fail(f"ShieldPack.cs 缺少: {token}")


SUITES = {"charging": validate_charging}
