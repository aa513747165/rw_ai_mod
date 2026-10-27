#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""天然器官链静态验证：眼/耳/鼻/胃物品 + 移植手术（新链，由 tools/validate_new.py 自动发现）。

器官培养舱（MMT_OrganVat，旧链 bionics 套件）的产物 Eye/Ear/Nose/Stomach 在本文件定义，
defName 刻意无前缀，与原版天然器官物品（Heart/Lung/Kidney/Liver）及 HSK 同名件一致。
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
    require_files,
)

ITEM_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_NaturalOrgans.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Bionics.xml"

# defName -> (label, 贴图名, 市场价值, 质量, 手术 defName, 手术 label)
ORGANS = {
    "Eye": ("眼球", "Eye", "420", "0.3", "MMT_InstallNaturalEye", "安装眼球"),
    "Ear": ("耳朵", "Ear", "410", "0.3", "MMT_InstallNaturalEar", "安装耳朵"),
    "Nose": ("鼻子", "Nose", "504", "0.3", "MMT_InstallNaturalNose", "安装鼻子"),
    "Stomach": ("胃", "Stomach", "500", "1", "MMT_InstallNaturalStomach", "安装胃"),
}


def validate_naturalorgans() -> None:
    require_files([ITEM_XML, BUILDING_XML])
    for _, (_, tex, _, _, _, _) in ORGANS.items():
        png = f"1.7/Textures/Things/Item/MMT/MMT_Bionics/{tex}.png"
        require_files([png])
        assert_png(png)

    defs = def_map(parse_xml(ITEM_XML))

    # ---- 物品 ----
    for def_name, (label, tex, value, mass, recipe, _) in ORGANS.items():
        if def_name not in defs:
            fail(f"天然器官缺少物品: {def_name}")
        item = defs[def_name]
        if item.tag != "ThingDef":
            fail(f"{def_name} 必须是 ThingDef")
        assert_equal(item.get("ParentName"), "BodyPartNaturalBase", f"{def_name} 父类")
        assert_equal(child_text(item, "label"), label, f"{def_name} label")
        assert_equal(
            child_text(item, "graphicData/texPath"),
            f"Things/Item/MMT/MMT_Bionics/{tex}",
            f"{def_name} 贴图",
        )
        assert_equal(child_text(item, "statBases/MarketValue"), value, f"{def_name} 市场价值")
        assert_equal(child_text(item, "statBases/Mass"), mass, f"{def_name} 质量")
        link = item.findtext("descriptionHyperlinks/RecipeDef")
        assert_equal(link, recipe, f"{def_name} 描述超链接")

    # ---- 移植手术（Recipe_InstallNaturalBodyPart 直接恢复部件，无需 HediffDef）----
    for def_name, (_, _, _, _, recipe, recipe_label) in ORGANS.items():
        if recipe not in defs:
            fail(f"天然器官缺少移植手术: {recipe}")
        rec = defs[recipe]
        if rec.tag != "RecipeDef":
            fail(f"{recipe} 必须是 RecipeDef")
        assert_equal(
            rec.get("ParentName"), "SurgeryInstallBodyPartNaturalBase", f"{recipe} 父类"
        )
        assert_equal(child_text(rec, "label"), recipe_label, f"{recipe} label")
        if li_values(rec, "ingredients/li/filter/thingDefs") != [def_name]:
            fail(f"{recipe} 原料必须是 {def_name} x1")
        if li_values(rec, "appliedOnFixedBodyParts") != [def_name]:
            fail(f"{recipe} appliedOnFixedBodyParts 必须是 {def_name}")

    # ---- 器官培养舱产物引用可解析（本文件提供 Eye/Ear/Nose/Stomach）----
    benches = def_map(parse_xml(BUILDING_XML))
    vat = benches["MMT_OrganVat"]
    vat_comp = next(
        li
        for li in vat.findall("comps/li")
        if li.attrib.get("Class") == "MyModeTest.CompProperties_OrganVat"
    )
    products = {li.findtext("productDef") for li in vat_comp.findall("products/li")}
    vanilla_organs = {"Heart", "Lung", "Liver", "Kidney"}  # 原版已定义
    for organ in ORGANS:
        if organ not in products:
            fail(f"器官培养舱产物缺少 {organ}")
    missing = products - set(ORGANS) - vanilla_organs - {"MMT_Biomatter"}
    if missing:
        fail(f"器官培养舱产物引用了未知 def: {sorted(missing)}")

    print("naturalorgans: OK")


SUITES = {"naturalorgans": validate_naturalorgans}
