#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""护盾数值梯队 + 强化系统静态验证（新链，由 tools/validate_new.py 自动发现）。

覆盖：玉质护符（中世纪）/ 充能护盾腰带 Mk.I（工业）/ 远古护盾腰带 Mk.III（未来）
三件新物品与配方、三个新研究节点、强化系统 C# 关键实现与贴图。
现有充能护盾腰带（Mk.II）的强化 comp 挂载也在此校验。"""
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

ITEMS_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ShieldTiers.xml"
MK2_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ChargeShield.xml"
RECIPES_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ShieldTiers.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Shield.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"

CS_TALISMAN = "Source/MyModeTest/ShieldTalisman.cs"
CS_ENHANCE = "Source/MyModeTest/ShieldEnhance.cs"
CS_PACK = "Source/MyModeTest/ShieldPack.cs"
CS_RECHARGE = "Source/MyModeTest/Rechargeable.cs"
CS_STATION = "Source/MyModeTest/ChargingStation.cs"

TEX_DIR = "1.7/Textures/Things/Item/MMT/Shield/"
TEX_TALISMAN = TEX_DIR + "MMT_ShieldTalisman.png"
TEX_BUBBLE_JADE = TEX_DIR + "MMT_ShieldBubbleJade.png"
TEX_BELT_EX = TEX_DIR + "MMT_ShieldBeltEx.png"
TEX_BUBBLE_VIOLET = TEX_DIR + "MMT_ShieldBubbleViolet.png"
TEX_BELT_ARCHO = TEX_DIR + "MMT_ShieldBeltArcho.png"
TEX_BUBBLE_ARCHO = TEX_DIR + "MMT_ShieldBubbleArcho.png"

RATES = ["1"] * 6 + ["0.8"] * 2 + ["0.6"] * 2 + ["0.4"] * 2 + ["0.2", "0.1", "0.01"]


def _comp_classes(node) -> list[str]:
    comps = node.find("comps")
    if comps is None:
        return []
    return [li.get("Class") or (li.findtext("compClass") or "") for li in comps.findall("li")]


def _check_enhancement_comp(item, name: str) -> None:
    """强化 comp：材料为能量结晶，成功率表恰为 15 项且数值符合设计。"""
    comps = item.find("comps")
    enh = None
    for li in comps.findall("li"):
        if (li.get("Class") or "") == "MyModeTest.CompProperties_ShieldEnhancement":
            enh = li
    if enh is None:
        fail(f"{name} 缺少 CompProperties_ShieldEnhancement")
    if enh.findtext("materialDef") != "MMT_EnergyCrystal":
        fail(f"{name} 强化材料必须是 MMT_EnergyCrystal")
    rates = [li.text.strip() for li in enh.findall("successRates/li")]
    if rates != RATES:
        fail(f"{name} 成功率表必须为 {RATES}，实际 {rates}")


def validate_shieldtiers() -> None:
    require_files(
        [ITEMS_XML, MK2_XML, RECIPES_XML, RESEARCH_XML, JOBS_XML, PARTS_BUILDING_XML]
        + [CS_TALISMAN, CS_ENHANCE, CS_PACK, CS_RECHARGE, CS_STATION]
        + [TEX_TALISMAN, TEX_BUBBLE_JADE, TEX_BELT_EX, TEX_BUBBLE_VIOLET, TEX_BELT_ARCHO, TEX_BUBBLE_ARCHO]
    )
    for tex in (TEX_TALISMAN, TEX_BUBBLE_JADE, TEX_BELT_EX, TEX_BUBBLE_VIOLET, TEX_BELT_ARCHO, TEX_BUBBLE_ARCHO):
        assert_png(tex)

    items = def_map(parse_xml(ITEMS_XML))

    # ---- 玉质护符（中世纪一次性，耐久驱动） ----
    if "MMT_WardTalisman" not in items:
        fail("物品缺少 MMT_WardTalisman")
    talisman = items["MMT_WardTalisman"]
    assert_equal(child_text(talisman, "techLevel"), "Medieval", "护符 techLevel")
    assert_equal(child_text(talisman, "statBases/MaxHitPoints"), "150", "护符耐久（即护盾量）")
    assert_equal(child_text(talisman, "smeltable"), "false", "护符 smeltable")
    if "MyModeTest.CompProperties_ShieldTalisman" not in _comp_classes(talisman):
        fail("护符缺少 CompProperties_ShieldTalisman")
    # 耐久驱动：不得带电池/强化 comp
    for forbidden in ("MyModeTest.CompProperties_Rechargeable", "MyModeTest.CompProperties_ShieldEnhancement"):
        if forbidden in _comp_classes(talisman):
            fail(f"护符不应携带 {forbidden}（无电量概念）")

    # ---- 充能护盾腰带 Mk.I（工业，2000 Wd / 50 Wd 每伤害） ----
    if "MMT_ChargeShieldPackMkI" not in items:
        fail("物品缺少 MMT_ChargeShieldPackMkI")
    mk1 = items["MMT_ChargeShieldPackMkI"]
    assert_equal(child_text(mk1, "techLevel"), "Industrial", "Mk.I techLevel")
    mk1_comps = mk1.find("comps")
    found_recharge = found_pack = False
    for li in mk1_comps.findall("li"):
        cls = li.get("Class") or ""
        if cls == "MyModeTest.CompProperties_Rechargeable":
            found_recharge = True
            assert_equal(li.findtext("maxChargeWd"), "2000", "Mk.I 电池容量")
        if cls == "MyModeTest.CompProperties_ShieldPack":
            found_pack = True
            assert_equal(li.findtext("wdPerDamage"), "50", "Mk.I 伤害换电比")
            if li.findtext("selfRegenWdPerDay") not in (None, "", "0"):
                fail("Mk.I 不应有自动回复")
    if not found_recharge or not found_pack:
        fail("Mk.I 缺少 Rechargeable/ShieldPack comp")
    _check_enhancement_comp(mk1, "Mk.I")

    # ---- 远古护盾腰带 Mk.III（未来，8000 Wd / 30 Wd 每伤害 / 自回 1200） ----
    if "MMT_ArchoShieldPack" not in items:
        fail("物品缺少 MMT_ArchoShieldPack")
    mk3 = items["MMT_ArchoShieldPack"]
    assert_equal(child_text(mk3, "techLevel"), "Ultra", "Mk.III techLevel")
    for li in mk3.find("comps").findall("li"):
        cls = li.get("Class") or ""
        if cls == "MyModeTest.CompProperties_Rechargeable":
            assert_equal(li.findtext("maxChargeWd"), "8000", "Mk.III 电池容量")
        if cls == "MyModeTest.CompProperties_ShieldPack":
            assert_equal(li.findtext("wdPerDamage"), "30", "Mk.III 伤害换电比")
            assert_equal(li.findtext("selfRegenWdPerDay"), "1200", "Mk.III 自动回复")
    _check_enhancement_comp(mk3, "Mk.III")

    # ---- 现有充能护盾腰带（Mk.II）：label 更名 + 挂载强化 comp ----
    mk2 = def_map(parse_xml(MK2_XML))["MMT_ChargeShieldPack"]
    assert_equal(child_text(mk2, "label"), "充能护盾腰带 Mk.II", "Mk.II label")
    _check_enhancement_comp(mk2, "Mk.II")

    # ---- 配方 ----
    recipes = def_map(parse_xml(RECIPES_XML))
    for def_name, product, research, bench in (
        ("MMT_MakeWardTalisman", "MMT_WardTalisman", "MMT_WardTalismanCraft", "CraftingSpot"),
        ("MMT_MakeChargeShieldMk1", "MMT_ChargeShieldPackMkI", "MMT_BatteryShieldTech", "TableMachining"),
        ("MMT_MakeArchoShieldPack", "MMT_ArchoShieldPack", "MMT_ArchoShieldTech", None),
    ):
        if def_name not in recipes:
            fail(f"配方缺少 {def_name}")
        recipe = recipes[def_name]
        assert_equal(child_text(recipe, "researchPrerequisite"), research, f"{def_name} 研究前置")
        products = recipe.find("products")
        if products is None or products.findtext(product) != "1":
            fail(f"{def_name} products 必须产出 {product} x1")
        if bench is not None and bench not in li_values(recipe, "recipeUsers"):
            fail(f"{def_name} 必须挂 {bench}")
    # Mk.III 挂电动装配台（登记在台子 recipes 里，同充能护盾腰带模式）
    bench_recipes = li_values(def_map(parse_xml(PARTS_BUILDING_XML))["MMT_ElectricAssemblyBench"], "recipes")
    if "MMT_MakeArchoShieldPack" not in bench_recipes:
        fail("电动装配台 recipes 缺少 MMT_MakeArchoShieldPack")

    # ---- 研究（坐标以 apply_research_layout.LAYOUT 为唯一事实来源） ----
    research = def_map(parse_xml(RESEARCH_XML))
    expected = {
        "MMT_WardTalismanCraft": ("Medieval", ["Smithing"]),
        "MMT_BatteryShieldTech": ("Industrial", ["Electricity", "Machining"]),
        "MMT_ArchoShieldTech": ("Ultra", ["MMT_RechargeableTech", "MMT_EnergyConvergence"]),
    }
    for def_name, (tech, prereqs) in expected.items():
        if def_name not in research:
            fail(f"研究缺少 {def_name}")
        node = research[def_name]
        assert_equal(child_text(node, "techLevel"), tech, f"{def_name} techLevel")
        if li_values(node, "prerequisites") != prereqs:
            fail(f"{def_name} 前置必须为 {prereqs}")
        assert_equal(child_text(node, "researchViewX"), research_coord(def_name, "x"), f"{def_name} 坐标X")
        assert_equal(child_text(node, "researchViewY"), research_coord(def_name, "y"), f"{def_name} 坐标Y")

    # ---- JobDef ----
    jobs = def_map(parse_xml(JOBS_XML))
    if "MMT_EnhanceChargingStation" not in jobs:
        fail("JobDefs 缺少 MMT_EnhanceChargingStation")
    assert_equal(
        child_text(jobs["MMT_EnhanceChargingStation"], "driverClass"),
        "MyModeTest.JobDriver_EnhanceGeneric",
        "强化作业 driverClass（通用驱动，护盾/武器强化台共用）",
    )

    # ---- C# 关键实现 ----
    talisman_cs = read_text(CS_TALISMAN)
    for token in ("CompProperties_ShieldTalisman", "CompShieldTalisman", "ConsumeResource", "TakeDamage"):
        if token not in talisman_cs:
            fail(f"ShieldTalisman.cs 缺少: {token}")
    enhance_cs = read_text(CS_ENHANCE)
    for token in (
        "CompProperties_ShieldEnhancement",
        "CompShieldEnhancement",
        "SuccessRateFor",
        "DefaultRates",
        "DoEnhance",
        "TransformLabel",
        "MaxChargeFactor",
        "WdCostFactor",
        "0.01f",
    ):
        if token not in enhance_cs:
            fail(f"ShieldEnhance.cs 缺少: {token}")
    pack_cs = read_text(CS_PACK)
    for token in ("EffectiveWdPerDamage", "selfRegenWdPerDay", "bubbleTexPath"):
        if token not in pack_cs:
            fail(f"ShieldPack.cs 缺少: {token}")
    recharge_cs = read_text(CS_RECHARGE)
    if "MaxChargeFactor" not in recharge_cs:
        fail("Rechargeable.cs 电量上限未接入强化倍率（MaxChargeFactor）")
    station_cs = read_text(CS_STATION)
    for token in ("MMT_EnhanceChargingStation", "强化设备"):
        if token not in station_cs:
            fail(f"ChargingStation.cs 缺少: {token}")


SUITES = {"shieldtiers": validate_shieldtiers}
