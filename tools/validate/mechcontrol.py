#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""机械集控核心链静态验证：MMT_MechControlHub 建筑 + 带宽/控制组模块 + 配方 + 研究（新链，由 tools/validate_new.py 自动发现）。

设计约束（详见 Source/MyModeTest/MechControlHub.cs 头注释与 Def 注释）：
- 基础带宽 20，每个带宽模块 +20（最多 4 个 → 上限 100）；基础控制组 1，每个控制组模块 +1（最多 3 个 → 上限 4）；
- 耗电 500W + 250W × 已插模块数；模块复用通用槽位系统（CompProperties_ModuleSlots + ModuleExtension）；
- 贴图复制自已授权的 Gestalt Engine（Workshop 3392334612），主机叠层按模块数 Tier0~T4。
"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    assert_png,
    child_text,
    def_map,
    fail,
    parse_xml,
    read_text,
    require_files,
)

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_MechControl.xml"
ITEMS_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_MechControlModules.xml"
RECIPES_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_MechControl.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_MechControl.xml"
ELECTRONICS_BENCH_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml"
STATPART_PATCH_XML = "1.7/Patches/MMT_Patch_MechControlStats.xml"
CS_FILE = "Source/MyModeTest/MechControlHub.cs"

TEX_DIR = "1.7/Textures/Things/Building/Production/MMT_MechControlHub"
TEX_BUILDING = TEX_DIR + "/MMT_MechControlHub.png"
TEX_MASK = TEX_DIR + "/MMT_MechControlHub_m.png"
TEX_MENU_ICON = "1.7/Textures/Things/Building/Production/MMT_MechControlHub_MenuIcon.png"
TEX_TIERS = [TEX_DIR + f"/MMT_MechControlHub_Tier{i}.png" for i in range(5)]
TEX_UI = [
    "1.7/Textures/UI/Icons/MMT_ConnectMechanoid.png",
    "1.7/Textures/UI/Icons/MMT_HackMechanoid.png",
    "1.7/Textures/UI/Icons/MMT_MechControlOverseer.png",
]
TEX_MODULES = [
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/MechBandwidthModule.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/MechControlGroupModule.png",
]

MODULE_DEFS = {
    "MMT_MechBandwidthModule": ("MechBandwidth", "20"),
    "MMT_MechControlGroupModule": ("MechControlGroup", "1"),
}

SLOT_LIMITS = {"MechBandwidth": "4", "MechControlGroup": "3"}


def _mod_extension(thing_def, class_name):
    for li in thing_def.findall("modExtensions/li"):
        if li.attrib.get("Class") == class_name:
            return li
    return None


def validate_mechcontrol() -> None:
    require_files([BUILDING_XML, ITEMS_XML, RECIPES_XML, RESEARCH_XML,
                   ELECTRONICS_BENCH_XML, CS_FILE,
                   TEX_BUILDING, TEX_MASK, TEX_MENU_ICON] + TEX_TIERS + TEX_UI + TEX_MODULES)
    for png in [TEX_BUILDING, TEX_MASK, TEX_MENU_ICON] + TEX_TIERS + TEX_UI + TEX_MODULES:
        assert_png(png)

    # ---- 建筑 def ----
    buildings = def_map(parse_xml(BUILDING_XML))
    hub = buildings.get("MMT_MechControlHub")
    if hub is None:
        fail("缺少建筑 def MMT_MechControlHub")
    dn = hub.find("defName")
    if dn is None or dn.get("MayRequire") != "Ludeon.Rimworld.Biotech":
        fail("MMT_MechControlHub 的 defName 必须带 MayRequire=Biotech")
    assert_equal(child_text(hub, "size"), "(5,5)", "size")
    graphic = hub.find("graphicData")
    assert_equal(child_text(graphic, "texPath"),
                 "Things/Building/Production/MMT_MechControlHub/MMT_MechControlHub", "texPath")
    assert_equal(child_text(graphic, "graphicClass"), "Graphic_Single", "graphicClass")
    assert_equal(child_text(graphic, "shaderType"), "CutoutComplex", "shaderType")
    assert_equal(child_text(hub, "uiIconPath"),
                 "Things/Building/Production/MMT_MechControlHub_MenuIcon", "uiIconPath")
    cost = {li.tag: (li.text or "").strip() for li in hub.findall("costList/*")}
    if cost.get("AIPersonaCore") != "1":
        fail("造价必须含 1 个 AI 人格核心（终局门控）")
    tabs = [li.text for li in hub.findall("inspectorTabs/li")]
    if "MyModeTest.ITab_ModuleSlots" not in tabs:
        fail("inspectorTabs 必须含 MyModeTest.ITab_ModuleSlots（模块管理页签）")
    assert_equal(child_text(hub, "interactionCellOffset"), "(0,0,-3)", "interactionCellOffset")
    assert_equal(child_text(hub, "researchPrerequisites/li"), "MMT_MechControlHub", "研究前置")

    comps = {li.attrib.get("Class"): li for li in hub.findall("comps/li")}
    if "CompProperties_Power" not in comps:
        fail("缺少电力 Comp")
    assert_equal(child_text(comps["CompProperties_Power"], "basePowerConsumption"), "500", "基础耗电")
    if "CompProperties_Flickable" not in comps:
        fail("缺少开关 Comp（关停时带宽归零需要）")
    slots = comps.get("MyModeTest.CompProperties_ModuleSlots")
    if slots is None:
        fail("缺少通用模块槽 Comp MyModeTest.CompProperties_ModuleSlots")
    limits = {child_text(e, "key"): child_text(e, "max") for e in slots.findall("slotLimits/li")}
    assert_equal(limits, SLOT_LIMITS, "模块槽位上限")
    hub_props = comps.get("MyModeTest.CompProperties_MechControlHub")
    if hub_props is None:
        fail("缺少 MyModeTest.CompProperties_MechControlHub")
    assert_equal(child_text(hub_props, "baseBandwidth"), "20", "基础带宽")
    assert_equal(child_text(hub_props, "baseControlGroups"), "1", "基础控制组")
    assert_equal(child_text(hub_props, "powerPerModule"), "250", "每模块耗电")

    # ---- 模块物品 def ----
    items = def_map(parse_xml(ITEMS_XML))
    assert_equal(set(items), set(MODULE_DEFS), "模块物品 defName 集合")
    for name, (slot_key, value) in MODULE_DEFS.items():
        item = items[name]
        ext = _mod_extension(item, "MyModeTest.ModuleExtension")
        if ext is None:
            fail(f"{name} 缺 ModuleExtension")
        assert_equal(child_text(ext, "slotKey"), slot_key, f"{name} slotKey")
        effects = [(child_text(e, "key"), child_text(e, "valuePerItem"))
                   for e in ext.findall("effects/li")]
        assert_equal(effects, [(slot_key, value)], f"{name} 效果列表")
        if not (ext.findtext("effectReport") or "").strip():
            fail(f"{name} 缺 effectReport")
        if not child_text(item, "graphicData/texPath"):
            fail(f"{name} 缺 texPath")

    # ---- 配方 def ----
    recipes = def_map(parse_xml(RECIPES_XML))
    for name, product in (("MMT_MakeMechBandwidthModule", "MMT_MechBandwidthModule"),
                          ("MMT_MakeMechControlGroupModule", "MMT_MechControlGroupModule")):
        recipe = recipes.get(name)
        if recipe is None:
            fail(f"缺少配方 {name}")
        if recipe.find(f"products/{product}") is None:
            fail(f"{name} 产物必须是 {product}")
        assert_equal(child_text(recipe, "researchPrerequisite"), "MMT_MechControlHub", f"{name} 研究前置")
        ingredient_defs = [li.text for li in recipe.findall("ingredients/li/filter/thingDefs/li")]
        for need in ("MMT_Microchip", "MMT_IntegratedCircuit"):
            if need not in ingredient_defs:
                fail(f"{name} 材料必须含 {need}")

    # ---- 配方挂载：电子器件工作台必须列出两个模块配方 ----
    bench = def_map(parse_xml(ELECTRONICS_BENCH_XML))["MMT_ElectricElectronicsTable"]
    bench_recipes = [li.text for li in bench.findall("recipes/li")]
    for name in ("MMT_MakeMechBandwidthModule", "MMT_MakeMechControlGroupModule"):
        if name not in bench_recipes:
            fail(f"电子器件工作台未挂载配方 {name}")

    # ---- 研究 def ----
    research = def_map(parse_xml(RESEARCH_XML))["MMT_MechControlHub"]
    assert_equal(child_text(research, "tab"), "MMT_ResearchTab", "研究页")
    assert_equal(child_text(research, "baseCost"), "4000", "研究点")
    assert_equal(child_text(research, "techLevel"), "Ultra", "科技等级")
    prereqs = [li.text for li in research.findall("prerequisites/li")]
    for need in ("HighMechtech", "MMT_Robotics1"):
        if need not in prereqs:
            fail(f"研究前置必须含 {need}")
    assert_equal(child_text(research, "requiredResearchBuilding"), "HiTechResearchBench", "研究台")
    assert_equal(child_text(research, "requiredResearchFacilities/li"), "MultiAnalyzer", "研究设施")
    assert_equal(child_text(research, "researchViewX"), "6.00", "researchViewX")
    assert_equal(child_text(research, "researchViewY"), "6.50", "researchViewY")

    # ---- C#：核心类与补丁 ----
    cs = read_text(CS_FILE)
    for token in ("class CompProperties_MechControlHub", "class CompMechControlHub",
                  "class StatPart_MechControlHub",
                  "PawnKindDefOf.Mechanitor_Basic", "PawnRelationDefOf.Overseer",
                  "StatDefOf.MechBandwidth", "StatDefOf.MechControlGroups",
                  "HediffDefOf.MechlinkImplant",
                  "nameof(Pawn_MechanitorTracker.CanControlMechs)",
                  "nameof(MechanitorUtility.InMechanitorCommandRange)",
                  "nameof(MechanitorUtility.GetMechGizmos)",
                  "nameof(MechanitorBandwidthGizmo.Visible)",
                  "typeof(CompBandNode)", "nameof(GameEnder.CheckOrUpdateGameOver)",
                  "EffecterDefOf.ControlMech", "GetEffectSum(\"MechBandwidth\")",
                  "GetEffectSum(\"MechControlGroup\")"):
        if token not in cs:
            fail(f"MechControlHub.cs 缺少 {token}")

    # ---- StatPart 注入补丁（单行 getter 会被 JIT 内联，带宽/控制组改走属性计算） ----
    stat_patch = read_text(STATPART_PATCH_XML)
    for token in ('defName="MechBandwidth"', 'defName="MechControlGroups"',
                  "MyModeTest.StatPart_MechControlHub"):
        if token not in stat_patch:
            fail(f"{STATPART_PATCH_XML} 缺少 {token}")


SUITES = {"mechcontrol": validate_mechcontrol}
