#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""仿生/植入体移除手术链静态验证（手写 MMT_Recipes_BionicsRemove.xml，对照 HSK 30 个 Remove 配方）。

生成器 tools/build_mmt_bionics_from_hsk.py 不产出移除手术，本文件为手写维护。
"""
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

REMOVE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_BionicsRemove.xml"
HEDIFF_XMLS = [
    "1.7/Defs/HediffDefs/MMT_Hediffs_Bionics.xml",
    "1.7/Defs/HediffDefs/MMT_Hediffs_BionicsCONN.xml",
]
ITEM_XMLS = [
    "1.7/Defs/ThingDefs_Items/MMT_Items_Bionics.xml",
    "1.7/Defs/ThingDefs_Items/MMT_Items_BionicsAdv.xml",
    "1.7/Defs/ThingDefs_Items/MMT_Items_BionicsCONN.xml",
    "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Bionics.xml",
]
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Bionics.xml"

# 移除配方 defName -> (移除的 hediff, 超链接物品, 研究前置或 None)
REMOVALS = {
    # Core_SK:脑部植入体(HSK 有 workAmount 3700 / 医疗 6 覆写)
    "MMT_RemoveBrainStimulator": ("MMT_BrainStimulator", "MMT_BrainStimulator", "MMT_ProsthesisInstall_D3"),
    "MMT_RemoveAIChip": ("MMT_AIChip", "MMT_AIChip", "MMT_ProsthesisInstall_D3"),
    # Core_SK:肋骨替换件
    "MMT_RemoveMedicalRib": ("MMT_MedicalRib", "MMT_MedicalRib", None),
    "MMT_RemoveDruggedRib": ("MMT_DruggedRib", "MMT_DruggedRib", None),
    "MMT_RemoveAdrenalineRib": ("MMT_AdrenalineRib", "MMT_AdrenalineRib", None),
    "MMT_RemovePainkillerRib": ("MMT_PainkillerRib", "MMT_PainkillerRib", None),
    "MMT_RemoveHeaterRib": ("MMT_HeaterRib", "MMT_HeaterRib", None),
    "MMT_RemoveCoolerRib": ("MMT_CoolerRib", "MMT_CoolerRib", None),
    "MMT_RemoveCoagulatorRib": ("MMT_CoagulatorRib", "MMT_CoagulatorRib", None),
    # Core_SK:外骨骼
    "MMT_RemoveExoskeletonSuit": ("MMT_ExoskeletonSuit", "MMT_ExoskeletonSuit", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveProtectiveExoskeleton": ("MMT_ProtectiveExoskeleton", "MMT_ProtectiveExoskeleton", "MMT_ProsthesisInstall_D4"),
    # Core_SK:战术角膜
    "MMT_RemoveTacticalCorneaImplant": ("MMT_TacticalCorneaImplant", "MMT_TacticalCorneaImplant", None),
    # Core_SK:辅助AI核心
    "MMT_RemoveConstructorCore": ("MMT_ConstructorCore", "MMT_ConstructorCore", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveFarmerCore": ("MMT_FarmerCore", "MMT_FarmerCore", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveMinerCore": ("MMT_MinerCore", "MMT_MinerCore", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveDoctorCore": ("MMT_DoctorCore", "MMT_DoctorCore", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveDiplomatCore": ("MMT_DiplomatCore", "MMT_DiplomatCore", "MMT_ProsthesisInstall_E1"),
    # Core_SK:战斗辅助AI
    "MMT_RemoveAuxiliaryAISharpshooter": ("MMT_AuxiliaryAI_Sharpshooter", "MMT_AuxiliaryAI_Sharpshooter", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveAuxiliaryAICommando": ("MMT_AuxiliaryAI_Commando", "MMT_AuxiliaryAI_Commando", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveAuxiliaryAIBrawler": ("MMT_AuxiliaryAI_Brawler", "MMT_AuxiliaryAI_Brawler", "MMT_ProsthesisInstall_E1"),
    # CONN
    "MMT_RemoveCONN_ExoskeletonArmor": ("MMT_CONN_hediff_ExoskeletonArmor", "MMT_CONN_Exoskeleton", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_Dermalplating": ("MMT_CONN_hediff_Dermalplating", "MMT_CONN_Dermalplating", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_LaserDetection": ("MMT_CONN_hediff_LaserDetection", "MMT_CONN_LaserDetection", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_BerserkerChip": ("MMT_CONN_hediff_BerserkerChip", "MMT_CONN_BerserkerChip", None),
    "MMT_RemoveCONN_Mindtwist": ("MMT_CONN_hediff_Mindtwist", "MMT_CONN_BodyPartsArtificialtwist", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_MuseChip": ("MMT_CONN_hediff_MuseChip", "MMT_CONN_MuseChip", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_NeuroEnhancer": ("MMT_CONN_hediff_NeuroEnhancer", "MMT_CONN_NeuroEnhancer", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_CombatAI": ("MMT_CONN_hediff_CombatAI", "MMT_CONN_CombatAI", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_FlashLight": ("MMT_CONN_hediff_FlashLight", "MMT_CONN_FlashLight", "MMT_ProsthesisInstall_E1"),
    "MMT_RemoveCONN_Musclewire": ("MMT_CONN_hediff_Musclewire", "MMT_CONN_Musclewire", "MMT_ProsthesisInstall_E1"),
}


def validate_bionicsremove() -> None:
    require_files([REMOVE_XML] + HEDIFF_XMLS + ITEM_XMLS + [RESEARCH_XML])

    recipes = def_map(parse_xml(REMOVE_XML))
    hediffs = {}
    for path in HEDIFF_XMLS:
        hediffs.update(def_map(parse_xml(path)))
    items = {}
    for path in ITEM_XMLS:
        items.update(def_map(parse_xml(path)))
    research = def_map(parse_xml(RESEARCH_XML))

    # 数量与清单一致
    extra = [d for d in recipes if d.startswith("MMT_Remove") and d not in REMOVALS]
    if extra:
        fail(f"移除手术清单外多出: {sorted(extra)}")

    for def_name, (hediff, item, rp) in REMOVALS.items():
        if def_name not in recipes:
            fail(f"缺移除手术: {def_name}")
            continue
        rec = recipes[def_name]
        if rec.tag != "RecipeDef":
            fail(f"{def_name} 必须是 RecipeDef")
        assert_equal(rec.get("ParentName"), "SurgeryRemoveImplantBase", f"{def_name} 父类")
        assert_equal(child_text(rec, "removesHediff"), hediff, f"{def_name} removesHediff")
        if hediff not in hediffs:
            fail(f"{def_name} 的 removesHediff={hediff} 找不到 HediffDef")
        if item not in items:
            fail(f"{def_name} 超链接物品 {item} 找不到 ThingDef")
        assert_equal(
            child_text(rec, "descriptionHyperlinks/ThingDef"), item, f"{def_name} 物品超链接"
        )
        rp_actual = rec.findtext("researchPrerequisite")
        rp_actual = rp_actual.strip() if rp_actual else None
        assert_equal(rp_actual, rp, f"{def_name} 研究前置")
        if rp and rp not in research:
            fail(f"{def_name} 研究前置 {rp} 找不到 ResearchProjectDef")
        if not child_text(rec, "label"):
            fail(f"{def_name} 缺 label")

    # hediff 侧 spawnThingOnRemoved(若设置)必须指向存在的物品
    for def_name, (hediff, _, _) in REMOVALS.items():
        hd = hediffs.get(hediff)
        if hd is None:
            continue
        spawn = hd.findtext("spawnThingOnRemoved")
        if spawn and spawn not in items:
            fail(f"{hediff} 的 spawnThingOnRemoved={spawn} 找不到 ThingDef")

    # 原版部件补丁:注入的 recipeMaker 必须带 recipeUsers,否则配方无工作台挂载点(死内容)
    patch = parse_xml("1.7/Patches/MMT_Patch_VanillaBionics.xml")
    for op in patch:
        if op.tag != "Operation":
            continue
        for rm in op.findall(".//recipeMaker"):
            if not li_values(rm, "recipeUsers"):
                xp = op.findtext("xpath") or "?"
                fail(f"补丁注入的 recipeMaker 缺 recipeUsers: {xp}")

    print("bionicsremove: OK")


SUITES = {"bionicsremove": validate_bionicsremove}
