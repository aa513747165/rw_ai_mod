#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""防具强化静态验证：18 词条模块/配方/comp 挂载补丁/研究/贴图（由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

MODULES_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ApparelModules.xml"
RECIPES_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ApparelEnhance.xml"
PATCH_XML = "1.7/Patches/MMT_Patch_ApparelEnhancement.xml"
BENCH_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_WeaponEnhance.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Endgame.xml"
CS_APPAREL = "Source/MyModeTest/ApparelEnhance.cs"
CS_COMMON = "Source/MyModeTest/EnhanceCommon.cs"
CS_MODULE = "Source/MyModeTest/ModuleSystem.cs"
CS_IFACE = "Source/MyModeTest/EquipmentInterfaces.cs"

# 词条 → 效果键（与 ModuleEffectKeys.Apparel* 一一对应）
MODULE_KEYS = {
    # 低级
    "MMT_ApparelModFortify": ["ApparelArmorFactor"],
    "MMT_ApparelModBladeWard": ["ApparelArmorSharpFactor"],
    "MMT_ApparelModBulwark": ["ApparelArmorBluntFactor"],
    "MMT_ApparelModWarmth": ["ApparelInsulationColdFactor"],
    "MMT_ApparelModCooling": ["ApparelInsulationHeatFactor"],
    # 中级
    "MMT_ApparelModHeatWard": ["ApparelArmorHeatFactor"],
    "MMT_ApparelModAbsorb": ["ApparelDamageTakenFactor"],
    "MMT_ApparelModEvasion": ["ApparelDodgeChance"],
    "MMT_ApparelModSwift": ["ApparelMoveSpeed"],
    "MMT_ApparelModPorter": ["ApparelCarryCapacity"],
    # 高级
    "MMT_ApparelModRecovery": ["ApparelHealOnHit"],
    "MMT_ApparelModEmpShield": ["ApparelEmpProtection"],
    "MMT_ApparelModDiligent": ["ApparelWorkSpeedFactor"],
    "MMT_ApparelModThorns": ["ApparelReflectDamage"],
    "MMT_ApparelModDischarge": ["ApparelShockBurst"],
    # 极品
    "MMT_ApparelModAegis": ["ApparelArmorFactor", "ApparelDamageTakenFactor"],
    "MMT_ApparelModFeather": ["ApparelMoveSpeed", "ApparelDodgeChance"],
    "MMT_ApparelModToxicCloud": ["ApparelToxicCloud"],
}

# 档位材料抽查基准（低/极品两档）
TIER_LOW = {"MMT_QuenchOil": "2", "MMT_EnergyCrystal": "3"}
TIER_BEST = {"MMT_QuenchOil": "5", "MMT_ArchotechShard": "5", "MMT_AncientDatacore": "1"}


def validate_apparelenhance() -> None:
    require_files([MODULES_XML, RECIPES_XML, PATCH_XML, BENCH_XML, RESEARCH_XML,
                   CS_APPAREL, CS_COMMON, CS_MODULE, CS_IFACE])

    # ---- 18 词条模块：slotKey + labelPrefix + 效果键一一对应 + 贴图 ----
    modules = def_map(parse_xml(MODULES_XML))
    if len(modules) != 18:
        fail(f"防具附魔模块应为 18 个，实际 {len(modules)}")
    for name, want_keys in MODULE_KEYS.items():
        if name not in modules:
            fail(f"缺少防具附魔模块 {name}")
            continue
        node = modules[name]
        ext = node.find("modExtensions/li[@Class='MyModeTest.ModuleExtension']")
        if ext is None:
            fail(f"{name} 缺少 ModuleExtension")
            continue
        assert_equal(child_text(ext, "slotKey"), "ApparelEnchant", f"{name} slotKey")
        if not (child_text(ext, "labelPrefix") or "").strip():
            fail(f"{name} 缺少 labelPrefix（防具名前缀）")
        keys = [child_text(li, "key") for li in ext.findall("effects/li")]
        if keys != want_keys:
            fail(f"{name} 效果键应为 {want_keys}，实际 {keys}")
        tex = "1.7/Textures/Things/Item/MMT/ApparelMod/" + name + ".png"
        if not Path(tex).exists():
            fail(f"{name} 缺少贴图 {tex}")

    # ---- 18 条配方：命名规则 + 前置研究 + 档位材料抽查 ----
    recipes = def_map(parse_xml(RECIPES_XML))
    for name in MODULE_KEYS:
        recipe = "MMT_Make" + name[len("MMT_"):]
        if recipe not in recipes:
            fail(f"缺少防具模块配方 {recipe}")
            continue
        prereq = [li.text for li in recipes[recipe].findall("researchPrerequisites/li")]
        if prereq != ["MMT_ApparelEnchantment"]:
            fail(f"{recipe} 前置研究应为 MMT_ApparelEnchantment，实际 {prereq}")
    for recipe, tier in (("MMT_MakeApparelModFortify", TIER_LOW), ("MMT_MakeApparelModToxicCloud", TIER_BEST)):
        node = recipes[recipe]
        got = {}
        for li in node.findall("ingredients/li"):
            defs = [d.text for d in li.findall("filter/thingDefs/li")]
            got[defs[0]] = child_text(li, "count")
        if got != tier:
            fail(f"{recipe} 材料应为 {tier}，实际 {got}")

    # ---- comp 挂载补丁：全体防具 + 排除护盾 + Conditional 兜底 ----
    patch_text = Path(PATCH_XML).read_text(encoding="utf-8")
    if "ThingDef[apparel" not in patch_text:
        fail("防具强化补丁必须打全体 ThingDef[apparel]（跨 Mod 兼容）")
    if 'contains(@Class,"CompProperties_Shield")' not in patch_text:
        fail("防具强化补丁必须排除护盾腰带（contains CompProperties_Shield）")
    if "CompProperties_ApparelEnhancement" not in patch_text:
        fail("防具强化补丁缺少 CompProperties_ApparelEnhancement 注入")
    if "PatchOperationConditional" not in patch_text:
        fail("防具补丁缺少无 comps 节点的兜底（PatchOperationConditional）")
    if patch_text.count("<li>0.05</li>") < 1:
        fail("防具强化补丁缺少 15 档成功率表（末档 0.05）")
    if patch_text.count("<maxLevel>") < 5:
        fail("防具强化补丁缺少 5 段材料阶梯")

    # ---- 强化台 recipes 含全部防具配方 ----
    bench = def_map(parse_xml(BENCH_XML))["MMT_WeaponEnhanceBench"]
    bench_recipes = [li.text for li in bench.findall("recipes/li")]
    for name in MODULE_KEYS:
        recipe = "MMT_Make" + name[len("MMT_"):]
        if recipe not in bench_recipes:
            fail(f"强化台未挂配方 {recipe}")

    # ---- 研究两项 ----
    research = def_map(parse_xml(RESEARCH_XML))
    for name, prereq in (("MMT_ApparelEnhancement", "MMT_WeaponEnhancement"),
                         ("MMT_ApparelEnchantment", "MMT_ApparelEnhancement")):
        if name not in research:
            fail(f"缺少研究 {name}")
            continue
        prereqs = [li.text for li in research[name].findall("prerequisites/li")]
        if prereq not in prereqs:
            fail(f"{name} 前置应含 {prereq}，实际 {prereqs}")

    # ---- C# 关键 token ----
    apparel_cs = Path(CS_APPAREL).read_text(encoding="utf-8")
    for token in ("CompApparelEnhancement", "IWornDamageAbsorber", "IWornPostDamageHandler",
                  "GetValueUnfinalized", "ApparelEnchant"):
        if token not in apparel_cs:
            fail(f"ApparelEnhance.cs 缺少 {token}")
    common_cs = Path(CS_COMMON).read_text(encoding="utf-8")
    for token in ("IEnchantable", "ApparelEquipEligible"):
        if token not in common_cs:
            fail(f"EnhanceCommon.cs 缺少 {token}")
    module_cs = Path(CS_MODULE).read_text(encoding="utf-8")
    for token in ("ApparelArmorFactor", "ApparelEmpProtection", "ApparelToxicCloud"):
        if token not in module_cs:
            fail(f"ModuleSystem.cs 缺少效果键 {token}")
    iface_cs = Path(CS_IFACE).read_text(encoding="utf-8")
    if "PostApplyDamage" not in iface_cs:
        fail("EquipmentInterfaces.cs 缺少 PostApplyDamage 统一补丁")

    print("apparelenhance: OK")


SUITES = {"apparelenhance": validate_apparelenhance}
