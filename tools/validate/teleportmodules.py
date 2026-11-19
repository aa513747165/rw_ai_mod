#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""传送/充能增效模块静态验证：充能效率模块 + 增值模块兼任传送次数模块（新链，由 tools/validate_new.py 自动发现）。

次元传送模块已移除：折跃传送台的传送次数槽位改用增值模块（MMT_YieldModule，
slotKey=Yield，效果列表含 ExtraTeleportCharges），功能不变（每个 +1 次，最多 10 个）。

校验点：
- 充能效率模块挂 ModuleExtension，slotKey/effects/effectReport 齐全且数值与设计一致；
- 增值模块（在加工机模块文件中）效果列表含 ExtraTeleportCharges +1；
- 配方挂电动装配台、研究前置正确；次元传送模块配方已从装配台移除；
- 折跃传送台/充能台的 slotLimits 声明对应槽位上限；
- 研究前置与布局坐标（与 apply_research_layout.LAYOUT 一致）；"次元传送"研究已删除；
- C# 侧关键 token（效果键与次数制实现）。
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

ITEMS_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_TeleportModules.xml"
PROCESSOR_ITEMS_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_ProcessorModules.xml"
RECIPES_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_TeleportModules.xml"
TELEPORT_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Teleport.xml"
STATION_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ChargingStation.xml"
BENCH_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"
SHIELD_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ShieldTiers.xml"
RESEARCH_TP_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Teleport.xml"
RESEARCH_CHG_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Charging.xml"

TEX_CHARGE = "1.7/Textures/Things/Item/MMT/MMT_ChargeEfficiencyModule.png"

# defName -> (slotKey, [(effectKey, valuePerItem)], texPath)
MODULE_DEFS = {
    "MMT_ChargeEfficiencyModule": ("ChargeEfficiency", [("ChargeRateFactor", "0.1")],
                                   "Things/Item/MMT/MMT_ChargeEfficiencyModule"),
}

RECIPE_DEFS = {
    "MMT_MakeChargeEfficiencyModule": ("MMT_ChargeEfficiencyModule", "MMT_ChargeBoost"),
}


def _mod_extension(thing_def):
    for li in thing_def.findall("modExtensions/li"):
        if li.attrib.get("Class") == "MyModeTest.ModuleExtension":
            return li
    return None


def _slot_limits(building_def):
    for li in building_def.findall("comps/li"):
        if li.attrib.get("Class") == "MyModeTest.CompProperties_ModuleSlots":
            return {child_text(e, "key"): child_text(e, "max")
                    for e in li.findall("slotLimits/li")}
    return None


def validate_teleportmodules() -> None:
    require_files([ITEMS_XML, PROCESSOR_ITEMS_XML, RECIPES_XML, TELEPORT_XML, STATION_XML,
                   BENCH_XML, SHIELD_RECIPE_XML,
                   RESEARCH_TP_XML, RESEARCH_CHG_XML,
                   TEX_CHARGE,
                   "Source/MyModeTest/ModuleSystem.cs",
                   "Source/MyModeTest/BuildingBattery.cs",
                   "Source/MyModeTest/ChargingStation.cs",
                   "Source/MyModeTest/TeleportPad.cs"])
    assert_png(TEX_CHARGE)

    # ---- 充能效率模块物品：ModuleExtension 配置齐全 ----
    items = def_map(parse_xml(ITEMS_XML))
    assert_equal(set(items), set(MODULE_DEFS), "传送/充能模块物品 defName 集合")
    for name, (slot_key, effects, tex) in MODULE_DEFS.items():
        ext = _mod_extension(items[name])
        if ext is None:
            fail(f"{name} 缺 ModuleExtension")
        assert_equal(child_text(ext, "slotKey"), slot_key, f"{name} slotKey")
        got = [(child_text(e, "key"), child_text(e, "valuePerItem"))
               for e in ext.findall("effects/li")]
        assert_equal(got, effects, f"{name} 效果列表")
        if not (ext.findtext("effectReport") or "").strip():
            fail(f"{name} 缺 effectReport 效果说明")
        assert_equal(child_text(items[name], "graphicData/texPath"), tex, f"{name} 贴图路径")

    # ---- 增值模块：兼任折跃传送台次数模块（YieldBonus + ExtraTeleportCharges）----
    proc_items = def_map(parse_xml(PROCESSOR_ITEMS_XML))
    yield_mod = proc_items.get("MMT_YieldModule")
    if yield_mod is None:
        fail("加工机模块文件缺 MMT_YieldModule")
    ext = _mod_extension(yield_mod)
    if ext is None:
        fail("MMT_YieldModule 缺 ModuleExtension")
    assert_equal(child_text(ext, "slotKey"), "Yield", "MMT_YieldModule slotKey")
    got = [(child_text(e, "key"), child_text(e, "valuePerItem"))
           for e in ext.findall("effects/li")]
    assert_equal(got, [("YieldBonus", "1"), ("ExtraTeleportCharges", "1")],
                 "MMT_YieldModule 效果列表")

    # ---- 配方：产出 + 研究前置 + 挂电动装配台；次元传送模块配方已移除 ----
    recipes = def_map(parse_xml(RECIPES_XML))
    assert_equal(set(recipes), set(RECIPE_DEFS), "模块配方 defName 集合")
    bench = def_map(parse_xml(BENCH_XML))["MMT_ElectricAssemblyBench"]
    bench_recipes = li_values(bench, "recipes")
    for name, (product, research) in RECIPE_DEFS.items():
        recipe = recipes[name]
        assert_equal(child_text(recipe, "researchPrerequisite"), research, f"{name} 研究前置")
        products = recipe.find("products")
        if products is None or products.findtext(product) != "1":
            fail(f"{name} products 必须产出 {product} x1")
        if name not in bench_recipes:
            fail(f"电动装配台 recipes 缺少 {name}")
    if "MMT_MakeDimensionalModule" in bench_recipes:
        fail("电动装配台 recipes 仍残留 MMT_MakeDimensionalModule")
    if "MMT_MakeYieldModule" not in bench_recipes:
        fail("电动装配台 recipes 缺少 MMT_MakeYieldModule")

    # ---- 远古护盾腰带 Mk.III：次元传送模块已替换为增值模块 ----
    shield_recipes = def_map(parse_xml(SHIELD_RECIPE_XML))
    archo = shield_recipes["MMT_MakeArchoShieldPack"]
    archo_filter = li_values(archo.find("fixedIngredientFilter"), "thingDefs")
    if "MMT_YieldModule" not in archo_filter:
        fail("远古护盾腰带 Mk.III fixedIngredientFilter 缺 MMT_YieldModule")
    if "MMT_DimensionalModule" in archo_filter:
        fail("远古护盾腰带 Mk.III 仍残留 MMT_DimensionalModule")

    # ---- 建筑槽位上限 ----
    pad = def_map(parse_xml(TELEPORT_XML))["MMT_TeleportPad"]
    assert_equal(_slot_limits(pad), {"ChargeEfficiency": "5", "Yield": "10"},
                 "折跃传送台槽位上限")
    station = def_map(parse_xml(STATION_XML))["MMT_ChargingStation"]
    assert_equal(_slot_limits(station), {"ChargeEfficiency": "5"}, "充能台槽位上限")

    # ---- 研究：前置与布局坐标；"次元传送"研究已删除 ----
    tp_research = def_map(parse_xml(RESEARCH_TP_XML))
    if "MMT_DimensionalTeleport" in tp_research:
        fail("次元传送研究 MMT_DimensionalTeleport 应已删除")
    tp = tp_research["MMT_Teleportation"]
    assert_equal(li_values(tp, "prerequisites"), ["MMT_MatterConversion3"], "折跃传送研究前置")
    assert_equal(child_text(tp, "researchViewX"), research_coord("MMT_Teleportation", "x"), "折跃传送坐标X")
    assert_equal(child_text(tp, "researchViewY"), research_coord("MMT_Teleportation", "y"), "折跃传送坐标Y")
    chg_research = def_map(parse_xml(RESEARCH_CHG_XML))
    boost = chg_research["MMT_ChargeBoost"]
    assert_equal(li_values(boost, "prerequisites"), ["MMT_RechargeableTech"], "充能增效研究前置")
    assert_equal(child_text(boost, "researchViewX"), research_coord("MMT_ChargeBoost", "x"), "充能增效坐标X")
    assert_equal(child_text(boost, "researchViewY"), research_coord("MMT_ChargeBoost", "y"), "充能增效坐标Y")

    # ---- C# 关键实现 ----
    module_cs = read_text("Source/MyModeTest/ModuleSystem.cs")
    for token in ("ChargeRateFactor", "ExtraTeleportCharges"):
        if token not in module_cs:
            fail(f"ModuleSystem.cs 缺少: {token}")
    battery_cs = read_text("Source/MyModeTest/BuildingBattery.cs")
    for token in ("EffectiveChargeRateWdPerDay", "ModuleEffectKeys.ChargeRateFactor", "CompModuleSlots"):
        if token not in battery_cs:
            fail(f"BuildingBattery.cs 缺少: {token}")
    station_cs = read_text("Source/MyModeTest/ChargingStation.cs")
    for token in ("EffectiveChargeRateWdPerDay", "ModuleEffectKeys.ChargeRateFactor", "CompModuleSlots"):
        if token not in station_cs:
            fail(f"ChargingStation.cs 缺少: {token}")
    pad_cs = read_text("Source/MyModeTest/TeleportPad.cs")
    for token in ("teleportCharges", "MaxTeleportCharges", "TryConsumeCharge",
                  "ModuleEffectKeys.ExtraTeleportCharges", "CompModuleSlots"):
        if token not in pad_cs:
            fail(f"TeleportPad.cs 缺少: {token}")
    for token in ("cooldownUntilTick", "cooldownTicks", "energyCostWd", "次元传送模块"):
        if token in pad_cs:
            fail(f"TeleportPad.cs 仍残留冷却/耗电/次元传送模块旧机制: {token}")


SUITES = {"teleportmodules": validate_teleportmodules}
