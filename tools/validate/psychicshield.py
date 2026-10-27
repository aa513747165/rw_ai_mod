#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""灵能护盾链静态验证：灵能护盾腰带 + 配方 + 研究 + 环境保护 Hediff（新链，由 tools/validate_new.py 自动发现）。"""
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

ITEM_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_PsychicShield.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_PsychicShield.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_PsychicShield.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_PsychicShield.xml"
PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"

CS_FILE = "Source/MyModeTest/ShieldBeltPsychic.cs"

TEX_BELT = "1.7/Textures/Things/Item/MMT/Shield/MMT_ShieldBeltJade.png"
TEX_BUBBLE = "1.7/Textures/Things/Item/MMT/Shield/MMT_ShieldBubblePsychic.png"


def _comp_classes(node) -> list[str]:
    comps = node.find("comps")
    if comps is None:
        return []
    return [li.get("Class") or (li.findtext("compClass") or "") for li in comps.findall("li")]


def validate_psychicshield() -> None:
    require_files([ITEM_XML, RECIPE_XML, RESEARCH_XML, HEDIFF_XML, CS_FILE, TEX_BELT, TEX_BUBBLE])
    assert_png(TEX_BELT)
    assert_png(TEX_BUBBLE)

    # ---- 物品：灵能护盾腰带 ----
    items = def_map(parse_xml(ITEM_XML))
    if "MMT_PsychicShieldBelt" not in items:
        fail("物品缺少 MMT_PsychicShieldBelt")
    belt = items["MMT_PsychicShieldBelt"]
    assert_equal(child_text(belt, "label"), "灵能护盾腰带", "灵能护盾腰带 label")
    assert_equal(
        child_text(belt, "graphicData/texPath"),
        "Things/Item/MMT/Shield/MMT_ShieldBeltJade",
        "灵能护盾腰带贴图",
    )
    assert_equal(child_text(belt, "tickerType"), "Normal", "灵能护盾腰带 tickerType")
    if "MyModeTest.CompProperties_ShieldPsychic" not in _comp_classes(belt):
        fail("灵能护盾腰带缺少 comp: MyModeTest.CompProperties_ShieldPsychic")
    # 能量上限=1 是伤害转熵 1:1 的换算基准，改动需同步评估机制
    assert_equal(child_text(belt, "statBases/EnergyShieldEnergyMax"), "1", "灵能护盾能量上限")
    offsets = belt.find("equippedStatOffsets")
    if offsets is None or offsets.find("PsychicSensitivityOffset") is None:
        fail("灵能护盾腰带缺少 equippedStatOffsets.PsychicSensitivityOffset")
    layers = li_values(belt, "apparel/layers")
    if "Belt" not in layers:
        fail("灵能护盾腰带必须是腰带层（Belt）")

    # ---- 配方 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_MakePsychicShieldBelt" not in recipes:
        fail("配方缺少 MMT_MakePsychicShieldBelt")
    recipe = recipes["MMT_MakePsychicShieldBelt"]
    assert_equal(child_text(recipe, "researchPrerequisite"), "MMT_PsychicShieldTech", "配方研究前置")
    products = recipe.find("products")
    if products is None or products.findtext("MMT_PsychicShieldBelt") != "1":
        fail("配方 products 必须产出 MMT_PsychicShieldBelt x1")
    filter_defs = li_values(recipe, "fixedIngredientFilter/thingDefs")
    for need in ("PsychicAmplifier", "ComponentSpacer", "Plasteel", "Gold"):
        if need not in filter_defs:
            fail(f"配方缺少原料: {need}")
    # 挂电动装配台
    parts_buildings = def_map(parse_xml(PARTS_BUILDING_XML))
    bench_recipes = li_values(parts_buildings["MMT_ElectricAssemblyBench"], "recipes")
    if "MMT_MakePsychicShieldBelt" not in bench_recipes:
        fail("电动装配台 recipes 缺少 MMT_MakePsychicShieldBelt")

    # ---- 研究 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_PsychicShieldTech" not in research:
        fail("研究缺少 MMT_PsychicShieldTech")
    node = research["MMT_PsychicShieldTech"]
    if "MMT_RechargeableTech" not in li_values(node, "prerequisites"):
        fail("灵能护盾技术前置必须是 MMT_RechargeableTech")
    assert_equal(child_text(node, "researchViewX"), research_coord("MMT_PsychicShieldTech", "x"), "研究坐标X")
    assert_equal(child_text(node, "researchViewY"), research_coord("MMT_PsychicShieldTech", "y"), "研究坐标Y")

    # ---- 环境保护 Hediff ----
    hediffs = def_map(parse_xml(HEDIFF_XML))
    if "MMT_ShieldEnvironmentProtect" not in hediffs:
        fail("HediffDefs 缺少 MMT_ShieldEnvironmentProtect")
    env = hediffs["MMT_ShieldEnvironmentProtect"]
    stage = env.find("stages/li")
    if stage is None or stage.find("statOffsets/ToxicEnvironmentResistance") is None:
        fail("环境保护 Hediff 缺少 ToxicEnvironmentResistance")

    # ---- C# 关键实现 ----
    cs = read_text(CS_FILE)
    for token in (
        "CompProperties_ShieldExBase",
        "CompShieldExBase",
        "CompProperties_ShieldPsychic",
        "CompShieldPsychic",
        "TryAddEntropy",
        "OffsetPsyfocusDirectly",
        "GetPsylinkLevel",
        "MMT_ShieldEnvironmentProtect",
    ):
        if token not in cs:
            fail(f"ShieldBeltPsychic.cs 缺少: {token}")


SUITES = {"psychicshield": validate_psychicshield}
