#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""能量聚能链静态验证：能量结晶 + 能量聚能发电站 + 点火充能装置（多方块点火，新链，由 tools/validate_new.py 自动发现）。"""
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
)

ITEM_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_EnergyCrystal.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_EnergyCrystal.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_EnergyConvergence.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_EnergyConvergence.xml"
ELECTRONICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml"

CS_FILE = "Source/MyModeTest/EnergyConvergence.cs"

TEX_CRYSTAL = "1.7/Textures/Things/Item/Resource/MMT_EnergyCrystal.png"
TEX_PLANT = "1.7/Textures/Things/Building/Power/MMT_EnergyConvergencePlant.png"
TEX_PLANT_M = "1.7/Textures/Things/Building/Power/MMT_EnergyConvergencePlant_m.png"
TEX_PLANT_LIT = "1.7/Textures/Things/Building/Power/MMT_EnergyConvergencePlant_Lit.png"
TEX_PLANT_LIT_M = "1.7/Textures/Things/Building/Power/MMT_EnergyConvergencePlant_Lit_m.png"
TEX_CHARGER = "1.7/Textures/Things/Building/Power/MMT_IgnitionCharger.png"
TEX_CHARGER_M = "1.7/Textures/Things/Building/Power/MMT_IgnitionCharger_m.png"
TEX_BALL = "1.7/Textures/Things/Building/Misc/MMT_EnergyBall.png"


def _comp_classes(node) -> list[str]:
    comps = node.find("comps")
    if comps is None:
        return []
    return [li.get("Class") or (li.findtext("compClass") or "") for li in comps.findall("li")]


def _comp_classes_full(node) -> list[str]:
    """comp 的 Class 与内部 compClass 都收集（Ignitable 等子类挂在 compClass 上）。"""
    comps = node.find("comps")
    if comps is None:
        return []
    out = []
    for li in comps.findall("li"):
        if li.get("Class"):
            out.append(li.get("Class"))
        inner = li.findtext("compClass")
        if inner:
            out.append(inner)
    return out


def validate_energyconvergence() -> None:
    require_files(
        [ITEM_XML, RECIPE_XML, RESEARCH_XML, BUILDING_XML, CS_FILE]
        + [TEX_CRYSTAL, TEX_PLANT, TEX_PLANT_M, TEX_PLANT_LIT, TEX_PLANT_LIT_M, TEX_CHARGER, TEX_CHARGER_M, TEX_BALL]
    )
    for tex in (TEX_CRYSTAL, TEX_PLANT, TEX_PLANT_M, TEX_PLANT_LIT, TEX_PLANT_LIT_M, TEX_CHARGER, TEX_CHARGER_M, TEX_BALL):
        assert_png(tex)

    # ---- 物品：能量结晶 ----
    items = def_map(parse_xml(ITEM_XML))
    if "MMT_EnergyCrystal" not in items:
        fail("物品缺少 MMT_EnergyCrystal")
    crystal = items["MMT_EnergyCrystal"]
    assert_equal(child_text(crystal, "label"), "能量结晶", "能量结晶 label")
    assert_equal(
        child_text(crystal, "graphicData/texPath"),
        "Things/Item/Resource/MMT_EnergyCrystal",
        "能量结晶贴图",
    )
    if "MMT_FuelCat" not in li_values(crystal, "thingCategories"):
        fail("能量结晶需归入 MMT_FuelCat 燃料分类")

    # ---- 配方：凝聚能量结晶 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_MakeEnergyCrystal" not in recipes:
        fail("配方缺少 MMT_MakeEnergyCrystal")
    recipe = recipes["MMT_MakeEnergyCrystal"]
    assert_equal(child_text(recipe, "researchPrerequisite"), "MMT_EnergyConvergence", "配方研究前置")
    products = recipe.find("products")
    if products is None or products.findtext("MMT_EnergyCrystal") is None:
        fail("配方 products 必须产出 MMT_EnergyCrystal")
    filter_defs = li_values(recipe, "fixedIngredientFilter/thingDefs")
    for need in ("MMT_Microchip", "MMT_Silicon", "Gold"):
        if need not in filter_defs:
            fail(f"配方缺少原料: {need}")
    # 挂电子器件工作台
    elec = def_map(parse_xml(ELECTRONICS_BUILDING_XML))
    if "MMT_MakeEnergyCrystal" not in li_values(elec["MMT_ElectricElectronicsTable"], "recipes"):
        fail("电子器件工作台 recipes 缺少 MMT_MakeEnergyCrystal")

    # ---- 研究 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_EnergyConvergence" not in research:
        fail("研究缺少 MMT_EnergyConvergence")
    node = research["MMT_EnergyConvergence"]
    if "MMT_NuclearPower" not in li_values(node, "prerequisites"):
        fail("能量聚能研究前置必须是 MMT_NuclearPower")
    assert_equal(child_text(node, "researchViewX"), "9.00", "研究坐标X")
    assert_equal(child_text(node, "researchViewY"), "8.00", "研究坐标Y")

    # ---- 建筑：能量聚能发电站 ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_EnergyConvergencePlant" not in buildings:
        fail("建筑缺少 MMT_EnergyConvergencePlant")
    plant = buildings["MMT_EnergyConvergencePlant"]
    assert_equal(
        child_text(plant, "thingClass"),
        "MyModeTest.Building_EnergyConvergencePlant",
        "发电站 thingClass",
    )
    assert_equal(child_text(plant, "size"), "(3,3)", "发电站尺寸")
    assert_equal(
        child_text(plant, "graphicData/texPath"),
        "Things/Building/Power/MMT_EnergyConvergencePlant",
        "发电站贴图",
    )
    if "MMT_EnergyConvergence" not in li_values(plant, "researchPrerequisites"):
        fail("发电站研究前置必须是 MMT_EnergyConvergence")
    if plant.find("building") is None:
        fail("发电站缺少显式 <building/> 标签（1.7 BuildingBase 不自带 BuildingProperties）")
    plant_comps = _comp_classes_full(plant)
    for comp in (
        "MyModeTest.CompPowerPlantIgnitable",
        "MyModeTest.CompRefuelableIgnitable",
        "CompProperties_Flickable",
        "CompProperties_Breakdownable",
        "MyModeTest.CompProperties_EnergyIgnition",
    ):
        if comp not in plant_comps:
            fail(f"发电站缺少 comp: {comp}")
    # 燃料必须是能量结晶
    fuel_defs = li_values(plant, "comps/li/fuelFilter/thingDefs")
    if "MMT_EnergyCrystal" not in fuel_defs:
        fail("发电站燃料必须是 MMT_EnergyCrystal")
    # 四角偏移必须是 4 个
    offsets = li_values(plant, "comps/li/chargerOffsets")
    if len(offsets) != 4:
        fail(f"发电站 chargerOffsets 必须是 4 个（当前 {len(offsets)}）")

    # ---- 建筑：点火充能装置 ----
    if "MMT_IgnitionCharger" not in buildings:
        fail("建筑缺少 MMT_IgnitionCharger")
    charger = buildings["MMT_IgnitionCharger"]
    assert_equal(child_text(charger, "size"), "(1,1)", "充能装置尺寸")
    assert_equal(
        child_text(charger, "graphicData/texPath"),
        "Things/Building/Power/MMT_IgnitionCharger",
        "充能装置贴图",
    )
    if "MMT_EnergyConvergence" not in li_values(charger, "researchPrerequisites"):
        fail("充能装置研究前置必须是 MMT_EnergyConvergence")
    if charger.find("building") is None:
        fail("充能装置缺少显式 <building/> 标签")
    charger_comps = _comp_classes_full(charger)
    for comp in ("CompProperties_Power", "CompProperties_Flickable", "MyModeTest.CompProperties_IgnitionCharger"):
        if comp not in charger_comps:
            fail(f"充能装置缺少 comp: {comp}")

    # ---- C# 关键实现 ----
    cs = read_text(CS_FILE)
    for token in (
        "CompProperties_EnergyIgnition",
        "CompEnergyIgnition",
        "EnergyIgnitionState",
        "CompPowerPlantIgnitable",
        "CompRefuelableIgnitable",
        "CompProperties_IgnitionCharger",
        "CompIgnitionCharger",
        "Building_EnergyConvergencePlant",
        "MMT_EnergyBall",
        "MMT_ChargeBeam",
        "MMT_EnergyConvergencePlant_Lit",
        "Graphic_Single",
        "FleckDefOf.FireGlow",
        "Command_Action",
        "开始点火",
    ):
        if token not in cs:
            fail(f"EnergyConvergence.cs 缺少: {token}")


SUITES = {"energyconvergence": validate_energyconvergence}
