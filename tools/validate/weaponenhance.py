#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""武器强化静态验证：强化台/46 词条模块/材料阶梯/comp 挂载补丁/淬火油/贴图。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

BENCH_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_WeaponEnhance.xml"
MODULES_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_WeaponModules.xml"
RECIPES_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_WeaponEnhance.xml"
OIL_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_WeaponEnhance.xml"
PATCH_XML = "1.7/Patches/MMT_Patch_WeaponEnhancement.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_WeaponEnhance.xml"
SHIELD1_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ChargeShield.xml"
SHIELD2_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ShieldTiers.xml"
CS_WEAPON = "Source/MyModeTest/WeaponEnhance.cs"
CS_COMMON = "Source/MyModeTest/EnhanceCommon.cs"

MODULE_DEFS = [
    "MMT_WeaponModSharp", "MMT_WeaponModAccurate", "MMT_WeaponModSwift", "MMT_WeaponModPierce",
    "MMT_WeaponModDeadly", "MMT_WeaponModGale", "MMT_WeaponModHeavy", "MMT_WeaponModRapid",
    "MMT_WeaponModEmp", "MMT_WeaponModIncendiary", "MMT_WeaponModLifeLeech",
    "MMT_WeaponModShieldBreaker", "MMT_WeaponModFrost",
    "MMT_WeaponModExecute", "MMT_WeaponModChainLightning", "MMT_WeaponModTimeFold", "MMT_WeaponModOverload",
    # 附魔2（Infusion 2）借鉴扩展（14 个）
    "MMT_WeaponModBlitz", "MMT_WeaponModGambler", "MMT_WeaponModRiot",
    "MMT_WeaponModExplode", "MMT_WeaponModStormCall", "MMT_WeaponModGas",
    "MMT_WeaponModSuppress", "MMT_WeaponModDisarm", "MMT_WeaponModToxic",
    "MMT_WeaponModInsectBane", "MMT_WeaponModMending", "MMT_WeaponModGuardian",
    "MMT_WeaponModInsight", "MMT_WeaponModSureHit",
    # 附魔3 扩展批次（15 个）
    "MMT_WeaponModLongRange", "MMT_WeaponModDeadeye", "MMT_WeaponModMagazine",
    "MMT_WeaponModBlinding", "MMT_WeaponModCorrode", "MMT_WeaponModBleed",
    "MMT_WeaponModSmoke", "MMT_WeaponModSlaughter", "MMT_WeaponModDemolition",
    "MMT_WeaponModVampire", "MMT_WeaponModDisruptor", "MMT_WeaponModBrutal",
    "MMT_WeaponModTerror", "MMT_WeaponModRangedLeech", "MMT_WeaponModCorpseBurst",
]

# 词条 → 效果键（与 ModuleEffectKeys 常量一一对应）
MODULE_KEYS = {
    "MMT_WeaponModSharp": ["WeaponDamageFactor"],
    "MMT_WeaponModAccurate": ["WeaponHitChance"],
    "MMT_WeaponModSwift": ["WeaponCooldownFactor"],
    "MMT_WeaponModPierce": ["WeaponArmorPen"],
    "MMT_WeaponModDeadly": ["WeaponCritChance"],
    "MMT_WeaponModGale": ["WeaponCooldownFactor", "WeaponWarmupFactor"],
    "MMT_WeaponModHeavy": ["WeaponDamageFactor", "WeaponKnockback"],
    "MMT_WeaponModRapid": ["WeaponCooldownFactor", "WeaponHitChance"],
    "MMT_WeaponModEmp": ["WeaponEmp"],
    "MMT_WeaponModIncendiary": ["WeaponIncendiary"],
    "MMT_WeaponModLifeLeech": ["WeaponLifeLeech"],
    "MMT_WeaponModShieldBreaker": ["WeaponShieldBreaker"],
    "MMT_WeaponModFrost": ["WeaponFrost"],
    "MMT_WeaponModExecute": ["WeaponExecute"],
    "MMT_WeaponModChainLightning": ["WeaponChainLightning"],
    "MMT_WeaponModTimeFold": ["WeaponTimeFold"],
    "MMT_WeaponModOverload": ["WeaponDamageFactor", "WeaponCooldownFactor"],
    "MMT_WeaponModBlitz": ["WeaponCooldownFactor", "WeaponDamageFactor"],
    "MMT_WeaponModGambler": ["WeaponDamageFactor", "WeaponHitChance"],
    "MMT_WeaponModRiot": ["WeaponDamageFactor", "WeaponSuppression"],
    "MMT_WeaponModExplode": ["WeaponExplode"],
    "MMT_WeaponModStormCall": ["WeaponStormCall"],
    "MMT_WeaponModGas": ["WeaponGas"],
    "MMT_WeaponModSuppress": ["WeaponSuppression"],
    "MMT_WeaponModDisarm": ["WeaponDisarm"],
    "MMT_WeaponModToxic": ["WeaponToxic"],
    "MMT_WeaponModInsectBane": ["WeaponInsectBane"],
    "MMT_WeaponModMending": ["WeaponMending"],
    "MMT_WeaponModGuardian": ["WeaponGuardian"],
    "MMT_WeaponModInsight": ["WeaponSkillExp"],
    "MMT_WeaponModSureHit": ["WeaponGuaranteedHit", "WeaponWarmupFactor"],
    # 附魔3 扩展批次
    "MMT_WeaponModLongRange": ["WeaponRangeBonus"],
    "MMT_WeaponModDeadeye": ["WeaponHitChance"],
    "MMT_WeaponModMagazine": ["WeaponBurstShots"],
    "MMT_WeaponModBlinding": ["WeaponBlinding"],
    "MMT_WeaponModCorrode": ["WeaponArmorShred"],
    "MMT_WeaponModBleed": ["WeaponBleed"],
    "MMT_WeaponModSmoke": ["WeaponSmoke"],
    "MMT_WeaponModSlaughter": ["WeaponFleshBane"],
    "MMT_WeaponModDemolition": ["WeaponDemolition"],
    "MMT_WeaponModVampire": ["WeaponLifeLeech"],
    "MMT_WeaponModDisruptor": ["WeaponEmp"],
    "MMT_WeaponModBrutal": ["WeaponCritDamage"],
    "MMT_WeaponModTerror": ["WeaponTerror"],
    "MMT_WeaponModRangedLeech": ["WeaponRangedLeech"],
    "MMT_WeaponModCorpseBurst": ["WeaponCorpseBurst"],
}


def validate_weaponenhance() -> None:
    require_files([BENCH_XML, MODULES_XML, RECIPES_XML, OIL_XML, PATCH_XML, JOBS_XML, HEDIFF_XML,
                   SHIELD1_XML, SHIELD2_XML, CS_WEAPON, CS_COMMON])

    # ---- 强化台建筑 ----
    benches = def_map(parse_xml(BENCH_XML))
    if "MMT_WeaponEnhanceBench" not in benches:
        fail("缺少武器强化台 MMT_WeaponEnhanceBench")
    bench = benches["MMT_WeaponEnhanceBench"]
    assert_equal(child_text(bench, "thingClass"), "MyModeTest.Building_WeaponEnhanceBench", "强化台 thingClass")
    assert_equal(child_text(bench, "size"), "(2,2)", "强化台尺寸")
    recipes = [li.text for li in bench.findall("recipes/li")]
    if len(recipes) != 70:
        fail(f"强化台应挂 70 个附魔模块配方（46 武器 + 24 防具），实际 {len(recipes)}")
    comp_classes = [li.attrib.get("Class", "") for li in bench.findall("comps/li")]
    if "MyModeTest.CompProperties_WeaponEnhanceBench" not in comp_classes:
        fail("强化台缺少 CompProperties_WeaponEnhanceBench")

    # ---- 46 词条模块：slotKey + 效果键一一对应 ----
    modules = def_map(parse_xml(MODULES_XML))
    for name in MODULE_DEFS:
        if name not in modules:
            fail(f"缺少附魔模块 {name}")
            continue
        node = modules[name]
        ext = node.find("modExtensions/li[@Class='MyModeTest.ModuleExtension']")
        if ext is None:
            fail(f"{name} 缺少 ModuleExtension")
            continue
        assert_equal(child_text(ext, "slotKey"), "WeaponEnchant", f"{name} slotKey")
        # 全部模块都要有武器名前缀（TransformLabel 取首个非空词条）
        if not (child_text(ext, "labelPrefix") or "").strip():
            fail(f"{name} 缺少 labelPrefix（武器名前缀）")
        keys = [child_text(li, "key") for li in ext.findall("effects/li")]
        if keys != MODULE_KEYS[name]:
            fail(f"{name} 效果键应为 {MODULE_KEYS[name]}，实际 {keys}")
        tex = "1.7/Textures/Things/Item/MMT/WeaponMod/" + name + ".png"
        if not Path(tex).exists():
            fail(f"{name} 缺少贴图 {tex}")

    # ---- 配方与淬火油 ----
    recipes = def_map(parse_xml(RECIPES_XML))
    if "MMT_MakeQuenchOil" not in recipes:
        fail("缺少淬火油配方 MMT_MakeQuenchOil")
    for name in MODULE_DEFS:
        recipe = "MMT_Make" + name[len("MMT_"):]
        if recipe not in recipes:
            fail(f"缺少模块配方 {recipe}")
    oils = def_map(parse_xml(OIL_XML))
    if "MMT_QuenchOil" not in oils:
        fail("缺少淬火油资源 MMT_QuenchOil")
    if not Path("1.7/Textures/Things/Item/MMT/MMT_QuenchOil.png").exists():
        fail("淬火油缺少贴图")

    # ---- comp 挂载补丁：全体武器 + 非 MMT 护盾 ----
    patch_text = Path(PATCH_XML).read_text(encoding="utf-8")
    if "ThingDef[weaponTags" not in patch_text:
        fail("武器强化补丁必须打全体 ThingDef[weaponTags]（跨 Mod 兼容）")
    if patch_text.count("<li>0.05</li>") < 1 or patch_text.count("<li>") < 60:
        fail("武器强化补丁缺少 15 档成功率表/材料阶梯")
    if "RimWorld.CompProperties_Shield" not in patch_text:
        fail("缺少非 MMT 护盾的强化 comp 挂载补丁")
    if "PatchOperationConditional" not in patch_text:
        fail("武器补丁缺少无 comps 节点的兜底（PatchOperationConditional）")

    # ---- MMT 护盾材料阶梯（同步改挂） ----
    for path in (SHIELD1_XML, SHIELD2_XML):
        text = Path(path).read_text(encoding="utf-8")
        if "materialTiers" not in text:
            fail(f"{path} 的护盾强化 comp 缺少 materialTiers 阶梯")
        if "MMT_AncientAlloy" not in text:
            fail(f"{path} 的材料阶梯缺少远古合金段")

    # ---- 作业 driverClass ----
    jobs = def_map(parse_xml(JOBS_XML))
    for job, driver in (("MMT_LoadEnhanceBench", "MyModeTest.JobDriver_LoadEnhanceBench"),
                        ("MMT_UnloadEnhanceBench", "MyModeTest.JobDriver_UnloadEnhanceBench"),
                        ("MMT_EnhanceWeapon", "MyModeTest.JobDriver_EnhanceGeneric"),
                        ("MMT_EnchantWeapon", "MyModeTest.JobDriver_EnchantWeapon")):
        if job not in jobs:
            fail(f"JobDefs 缺少 {job}")
        else:
            assert_equal(child_text(jobs[job], "driverClass"), driver, f"{job} driverClass")

    # ---- 冰霜/致盲/蚀甲 hediff ----
    hediffs = def_map(parse_xml(HEDIFF_XML))
    for hdef in ("MMT_Hediff_FrostSlow", "MMT_Hediff_Blinded", "MMT_Hediff_ArmorShred"):
        if hdef not in hediffs:
            fail(f"缺少命中特效 Hediff {hdef}")

    # ---- C# 关键实现 ----
    cs = Path(CS_WEAPON).read_text(encoding="utf-8")
    for token in ("CompWeaponEnhancement", "CompWeaponEnhanceBench", "EnchantSlots",
                  "get_DamageAmount", "get_ArmorPenetration", "HitReportFor",
                  "AdjustedCooldownTicks", "get_WarmupTime", "AdjustedBaseMeleeDamageAmount",
                  "CombatExtendedActive", "WeaponOnHitMechanics",
                  # 附魔2 借鉴扩展：新机制键消费端 / 覆盖安装 / 必中缓存 / 危险 gizmo / 守护补丁
                  "WeaponExplode", "WeaponStormCall", "WeaponGas", "WeaponSuppression",
                  "WeaponDisarm", "WeaponToxic", "WeaponInsectBane", "WeaponMending",
                  "WeaponSkillExp", "WeaponGuaranteedHit", "WeaponGuardian",
                  "RollGuaranteedHit", "triggerEffectsEnabled", "CompGetGizmosExtra",
                  "PreApplyDamage", "overwritten",
                  # 附魔3 扩展批次：射程/连发补丁与新机制键消费端
                  "AdjustedRange", "get_BurstShotCount", "WeaponRangeBonus", "WeaponBurstShots",
                  "WeaponCritDamage", "WeaponBlinding", "WeaponArmorShred", "WeaponBleed",
                  "WeaponTerror", "WeaponSmoke", "WeaponFleshBane", "WeaponDemolition",
                  "WeaponRangedLeech", "WeaponCorpseBurst", "MMT_Hediff_Blinded", "MMT_Hediff_ArmorShred"):
        if token not in cs:
            fail(f"WeaponEnhance.cs 缺少: {token}")
    settings = Path("Source/MyModeTest/MMTSettings.cs").read_text(encoding="utf-8")
    for token in ("weaponHazardEffects", "enableWeaponExplode", "enableWeaponGuaranteedHit",
                  "enableWeaponMending", "enableWeaponSkillExp",
                  # 附魔3 扩展批次开关
                  "enableWeaponTerror", "enableWeaponSmoke", "enableWeaponFleshBane",
                  "enableWeaponDemolition", "enableWeaponRangedLeech", "enableWeaponCorpseBurst"):
        if token not in settings:
            fail(f"MMTSettings.cs 缺少: {token}")
    common = Path(CS_COMMON).read_text(encoding="utf-8")
    for token in ("EnhanceMaterialOption", "EnhanceTierMaterials", "JobDriver_EnhanceGeneric", "IEnhanceStation"):
        if token not in common:
            fail(f"EnhanceCommon.cs 缺少: {token}")

    # ---- 强化台贴图（多朝向 3 + 遮罩 3） ----
    for suffix in ("_south", "_north", "_east", "_southm", "_northm", "_eastm"):
        tex = f"1.7/Textures/Things/Building/Production/MMT_WeaponEnhanceBench/MMT_WeaponEnhanceBench{suffix}.png"
        if not Path(tex).exists():
            fail(f"强化台缺少贴图 {tex}")

    print("weaponenhance: OK")


SUITES = {"weaponenhance": validate_weaponenhance}
