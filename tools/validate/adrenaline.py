#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""肾上腺素急救针剂静态验证：物品 + Hediff + 配方（挂医药链 B2，由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    require_files,
)

ITEM_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_Drugs.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_Drugs.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Medicine.xml"


def validate_adrenaline() -> None:
    require_files([ITEM_XML, HEDIFF_XML, RECIPE_XML])

    # ---- 物品：肾上腺素注射剂（借用原版 GoJuice 贴图，Medical 类无成瘾） ----
    items = def_map(parse_xml(ITEM_XML))
    if "MMT_Adrenaline" not in items:
        fail("物品缺少 MMT_Adrenaline")
    item = items["MMT_Adrenaline"]
    assert_equal(child_text(item, "label"), "肾上腺素", "肾上腺素 label")
    assert_equal(child_text(item, "graphicData/texPath"), "Things/Item/Drug/GoJuice", "肾上腺素贴图")
    assert_equal(child_text(item, "techLevel"), "Medieval", "肾上腺素科技等级")
    assert_equal(child_text(item, "ingestible/drugCategory"), "Medical", "肾上腺素药物类别")
    doer = item.find("ingestible/outcomeDoers/li")
    if doer is None or doer.attrib.get("Class") != "IngestionOutcomeDoer_GiveHediff":
        fail("肾上腺素缺少 GiveHediff 服用效果")
    assert_equal(child_text(doer, "hediffDef"), "MMT_AdrenalineHigh", "肾上腺素 hediff 指向")
    assert_equal(child_text(doer, "severity"), "0.5", "肾上腺素 hediff 严重度（约 12 小时）")
    drug_comp = next(li for li in item.findall("comps/li")
                     if li.attrib.get("Class") == "CompProperties_Drug")
    assert_equal(child_text(drug_comp, "addictiveness"), "0", "肾上腺素无成瘾")

    # ---- Hediff：意识 +0.20、疼痛 ×0.5、每天 -1.0 ----
    hediffs = def_map(parse_xml(HEDIFF_XML))
    if "MMT_AdrenalineHigh" not in hediffs:
        fail("Hediff 缺少 MMT_AdrenalineHigh")
    high = hediffs["MMT_AdrenalineHigh"]
    sev_comp = next(li for li in high.findall("comps/li")
                    if li.attrib.get("Class") == "HediffCompProperties_SeverityPerDay")
    assert_equal(child_text(sev_comp, "severityPerDay"), "-1.0", "肾上腺素 hediff 消退速率")
    stage = high.find("stages/li")
    assert_equal(child_text(stage, "painFactor"), "0.5", "肾上腺素疼痛系数")
    cap = stage.find("capMods/li")
    assert_equal(child_text(cap, "capacity"), "Consciousness", "肾上腺素影响能力")
    assert_equal(child_text(cap, "offset"), "0.20", "肾上腺素意识加成")

    # ---- 配方：挂 B2，两台医疗台，基础医药包 + 中性花瓣 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    if "MMT_MakeAdrenaline" not in recipes:
        fail("配方缺少 MMT_MakeAdrenaline")
    recipe = recipes["MMT_MakeAdrenaline"]
    assert_equal(child_text(recipe, "researchPrerequisite"), "MMT_Medicine_B2", "肾上腺素研究前置")
    assert_equal([li.tag for li in recipe.find("products")], ["MMT_Adrenaline"], "肾上腺素产物")
    inputs = {td.text: ing.findtext("count") for ing in recipe.findall("ingredients/li")
              for td in ing.findall("filter/thingDefs/li")}
    assert_equal(inputs, {"MMT_BasicMedicineKit": "1", "MMT_NeutroPetals": "3"}, "肾上腺素原料")
    assert_equal(li_values(recipe, "recipeUsers"),
                 ["MMT_MedicalTable", "MMT_ElectricMedicalTable"], "肾上腺素工作台")

    print("adrenaline: OK")


SUITES = {"adrenaline": validate_adrenaline}
