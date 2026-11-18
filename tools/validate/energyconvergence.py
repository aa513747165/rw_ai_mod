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
# 发电站三朝向贴图：全程用 _base 本体，点火态仅靠能量球/光环特效，无静态点火图
TEX_PLANT_BASE = [
    f"1.7/Textures/Things/Building/Power/MMT_EnergyConvergencePlant_base_{d}.png"
    for d in ("east", "north", "south")
]
TEX_CHARGER = [
    f"1.7/Textures/Things/Building/Power/MMT_IgnitionCharger_{d}.png"
    for d in ("east", "north", "south")
]
TEX_ORB = "1.7/Textures/Things/Building/Misc/MMT_EnergyOrb.png"
TEX_HALO = "1.7/Textures/Things/Building/Misc/MMT_Halo.png"


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
    all_tex = [TEX_CRYSTAL, TEX_ORB, TEX_HALO] + TEX_PLANT_BASE + TEX_CHARGER
    require_files([ITEM_XML, RECIPE_XML, RESEARCH_XML, BUILDING_XML, CS_FILE] + all_tex)
    for tex in all_tex:
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
        "Things/Building/Power/MMT_EnergyConvergencePlant_base",
        "发电站贴图",
    )
    assert_equal(child_text(plant, "graphicData/graphicClass"), "Graphic_Multi", "发电站 graphicClass")
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
    assert_equal(child_text(charger, "graphicData/graphicClass"), "Graphic_Multi", "充能装置 graphicClass")
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
        "MMT_EnergyOrb",
        "MMT_Halo",
        "MMT_ChargeBeam",
        "DrawIgnitedEffect",
        "FleckDefOf.FireGlow",
        "Command_Action",
        "开始点火",
    ):
        if token not in cs:
            fail(f"EnergyConvergence.cs 缺少: {token}")


SUITES = {"energyconvergence": validate_energyconvergence}


# ==================== 能量结晶培育路线（种植→提纯→结晶） ====================

PLANT_XML = "1.7/Defs/ThingDefs_Plants/MMT_Plants_EnergyCrystal.xml"
CHEMLAB_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricPetrochem.xml"

TEX_LUMENBULB_PLANT = [
    f"1.7/Textures/Things/Plant/MMT/MMT_PlantLumenbulb/Lumenbulb{v}.png"
    for v in "AB"
] + ["1.7/Textures/Things/Plant/MMT/MMT_PlantLumenbulb_Immature/LumenbulbImmatureA.png"]
TEX_LUMEN_BULB = "1.7/Textures/Things/Item/Resource/MMT_LumenBulb.png"
TEX_ESSENCE = "1.7/Textures/Things/Item/Resource/MMT_CrystalEssence.png"
TEX_INCUBATOR = [
    f"1.7/Textures/Things/Building/Production/MMT_CrystalIncubator/MMT_CrystalIncubator_{d}.png"
    for d in ("east", "north", "south")
]


def validate_crystalcultivation() -> None:
    all_tex = TEX_LUMENBULB_PLANT + [TEX_LUMEN_BULB, TEX_ESSENCE] + TEX_INCUBATOR
    require_files([PLANT_XML, ITEM_XML, RECIPE_XML, RESEARCH_XML, BUILDING_XML, CHEMLAB_BUILDING_XML] + all_tex)
    for tex in all_tex:
        assert_png(tex)

    # ---- 植物：辉晶花 ----
    plants = def_map(parse_xml(PLANT_XML))
    if "MMT_PlantLumenbulb" not in plants:
        fail("植物缺少 MMT_PlantLumenbulb")
    plant = plants["MMT_PlantLumenbulb"]
    assert_equal(child_text(plant, "label"), "辉晶花", "辉晶花 label")
    assert_equal(
        child_text(plant, "graphicData/texPath"),
        "Things/Plant/MMT/MMT_PlantLumenbulb",
        "辉晶花贴图",
    )
    if "Hydroponic" not in li_values(plant, "plant/sowTags"):
        fail("辉晶花必须水培限定（sowTags 仅 Hydroponic）")
    tags = li_values(plant, "plant/sowTags")
    if tags != ["Hydroponic"]:
        fail(f"辉晶花 sowTags 应只有 Hydroponic（当前 {tags}）")
    if "MMT_EnergyCrystalCultivation" not in li_values(plant, "plant/sowResearchPrerequisites"):
        fail("辉晶花播种研究前置必须是 MMT_EnergyCrystalCultivation")
    assert_equal(child_text(plant, "plant/harvestedThingDef"), "MMT_LumenBulb", "辉晶花收获物")
    assert_equal(child_text(plant, "plant/growDays"), "12", "辉晶花生长天数")
    assert_equal(
        child_text(plant, "plant/immatureGraphicPath"),
        "Things/Plant/MMT/MMT_PlantLumenbulb_Immature",
        "辉晶花幼株贴图",
    )

    # ---- 中间产物 ----
    items = def_map(parse_xml(ITEM_XML))
    for def_name, label in (("MMT_LumenBulb", "辉晶球茎"), ("MMT_CrystalEssence", "晶能精粹")):
        if def_name not in items:
            fail(f"物品缺少 {def_name}")
        assert_equal(child_text(items[def_name], "label"), label, f"{def_name} label")

    # ---- 配方：提纯 + 结晶 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_RefineCrystalEssence" not in recipes:
        fail("配方缺少 MMT_RefineCrystalEssence")
    refine = recipes["MMT_RefineCrystalEssence"]
    assert_equal(child_text(refine, "researchPrerequisite"), "MMT_EnergyCrystalCultivation", "提纯配方研究前置")
    if refine.find("products") is None or refine.find("products").findtext("MMT_CrystalEssence") is None:
        fail("提纯配方 products 必须产出 MMT_CrystalEssence")
    for need in ("MMT_LumenBulb", "Neutroamine"):
        if need not in li_values(refine, "fixedIngredientFilter/thingDefs"):
            fail(f"提纯配方缺少原料: {need}")
    # 挂石化产品实验室
    chemlab = def_map(parse_xml(CHEMLAB_BUILDING_XML))
    if "MMT_RefineCrystalEssence" not in li_values(chemlab["MMT_ElectricChemLab"], "recipes"):
        fail("石化产品实验室 recipes 缺少 MMT_RefineCrystalEssence")

    if "MMT_GrowEnergyCrystal" in recipes:
        fail("手动培养配方 MMT_GrowEnergyCrystal 应已移除（培养舱为无人值守自动培养）")

    # ---- 建筑：晶体培养舱（无人值守自动培养，支持超频模块） ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_CrystalIncubator" not in buildings:
        fail("建筑缺少 MMT_CrystalIncubator")
    inc = buildings["MMT_CrystalIncubator"]
    assert_equal(child_text(inc, "label"), "晶体培养舱", "培养舱 label")
    assert_equal(child_text(inc, "thingClass"), "Building", "培养舱 thingClass")
    assert_equal(child_text(inc, "size"), "(2,2)", "培养舱尺寸")
    assert_equal(
        child_text(inc, "graphicData/texPath"),
        "Things/Building/Production/MMT_CrystalIncubator/MMT_CrystalIncubator",
        "培养舱贴图",
    )
    assert_equal(child_text(inc, "graphicData/graphicClass"), "Graphic_Multi", "培养舱 graphicClass")
    if "MMT_EnergyCrystalCultivation" not in li_values(inc, "researchPrerequisites"):
        fail("培养舱研究前置必须是 MMT_EnergyCrystalCultivation")
    if inc.find("building") is None:
        fail("培养舱缺少显式 <building/> 标签（1.7 BuildingBase 不自带 BuildingProperties）")
    inc_comps = _comp_classes_full(inc)
    for comp in (
        "CompProperties_Power",
        "CompProperties_Flickable",
        "MyModeTest.CompProperties_ModuleSlots",
        "CompProperties_Refuelable",
        "MyModeTest.CompProperties_AutoProcessor",
    ):
        if comp not in inc_comps:
            fail(f"培养舱缺少 comp: {comp}")
    # 页签：处理进度 + 增效模块
    tabs = li_values(inc, "inspectorTabs")
    for tab in ("MyModeTest.ITab_AutoProcessor", "MyModeTest.ITab_ModuleSlots"):
        if tab not in tabs:
            fail(f"培养舱 inspectorTabs 缺少: {tab}")
    # 精粹仓（燃料式）只接受晶能精粹
    fuel_defs = li_values(inc, "comps/li/fuelFilter/thingDefs")
    if fuel_defs != ["MMT_CrystalEssence"]:
        fail(f"培养舱燃料仓必须只接受 MMT_CrystalEssence（当前 {fuel_defs}）")
    # 自动处理参数：每 12000 tick 消耗 3 精粹产 1 能量结晶
    auto_props = None
    slot_limits = None
    for li in inc.findall("comps/li"):
        cls = li.attrib.get("Class")
        if cls == "MyModeTest.CompProperties_AutoProcessor":
            auto_props = li
        elif cls == "MyModeTest.CompProperties_ModuleSlots":
            slot_limits = {child_text(e, "key"): child_text(e, "max") for e in li.findall("slotLimits/li")}
    if auto_props is None:
        fail("培养舱缺少 CompProperties_AutoProcessor 配置")
    else:
        assert_equal(child_text(auto_props, "intervalTicks"), "12000", "培养周期")
        assert_equal(child_text(auto_props, "inputCount"), "3", "每轮精粹消耗")
        assert_equal(child_text(auto_props, "outputDef"), "MMT_EnergyCrystal", "培养舱产物")
    if slot_limits != {"Overclock": "5"}:
        fail(f"培养舱 slotLimits 应为 Overclock×5（当前 {slot_limits}）")

    # ---- 研究：能量结晶培育 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_EnergyCrystalCultivation" not in research:
        fail("研究缺少 MMT_EnergyCrystalCultivation")
    node = research["MMT_EnergyCrystalCultivation"]
    prereqs = li_values(node, "prerequisites")
    for need in ("MMT_EnergyConvergence", "Hydroponics"):
        if need not in prereqs:
            fail(f"培育研究前置缺少: {need}")
    assert_equal(child_text(node, "researchViewX"), "10.00", "培育研究坐标X")
    assert_equal(child_text(node, "researchViewY"), "9.00", "培育研究坐标Y")


SUITES["crystalcultivation"] = validate_crystalcultivation
