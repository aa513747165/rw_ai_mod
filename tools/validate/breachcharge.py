#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""爆墙炸弹静态验证：DamageDef + 武器 + 投射物 + 贴图（由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    parse_xml,
    require_files,
)

DAMAGE_XML = "1.7/Defs/DamageDefs/MMT_Damage_BreachCharge.xml"
WEAPON_XML = "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_BreachCharge.xml"
PROJ_XML = "1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_BreachCharge.xml"
JOB_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
TEX_ITEM = "1.7/Textures/Things/Weapons/Grenades/WallBreach.png"
TEX_PROJ = "1.7/Textures/Things/Projectile/WallBreach.png"


def validate_breachcharge() -> None:
    require_files([DAMAGE_XML, WEAPON_XML, PROJ_XML, HEDIFF_XML, JOB_XML, TEX_ITEM, TEX_PROJ])

    # ---- DamageDef：对人约10、对建筑放大100倍（约1000） ----
    damages = def_map(parse_xml(DAMAGE_XML))
    if "MMT_BreachBlast" not in damages:
        fail("DamageDef 缺少 MMT_BreachBlast")
    dmg = damages["MMT_BreachBlast"]
    assert_equal(child_text(dmg, "defaultDamage"), "10", "破墙爆破基础伤害（对人）")
    assert_equal(child_text(dmg, "buildingDamageFactorImpassable"), "100", "破墙爆破对不可通行建筑系数")
    assert_equal(child_text(dmg, "buildingDamageFactorPassable"), "100", "破墙爆破对可通行建筑系数")
    assert_equal(child_text(dmg, "isExplosive"), "true", "破墙爆破为爆炸伤害")

    # ---- 武器：敌方专用、AI 建筑破坏标记、无 onlyManualCast、无 weaponTags ----
    weapons = def_map(parse_xml(WEAPON_XML))
    if "MMT_Weapon_WallBreachCharge" not in weapons:
        fail("武器缺少 MMT_Weapon_WallBreachCharge")
    weapon = weapons["MMT_Weapon_WallBreachCharge"]
    assert_equal(child_text(weapon, "tradeability"), "None", "爆墙炸弹不可交易")
    assert_equal(child_text(weapon, "destroyOnDrop"), "true", "爆墙炸弹不可缴获")
    assert_equal(child_text(weapon, "generateCommonality"), "0", "爆墙炸弹不自然生成")
    assert_equal(child_text(weapon, "graphicData/texPath"), "Things/Weapons/Grenades/WallBreach", "爆墙炸弹贴图路径")
    if weapon.find("weaponTags") is not None:
        fail("爆墙炸弹不应有 weaponTags（配发由 C# 补丁控制）")
    verb = weapon.find("verbs/li")
    assert_equal(child_text(verb, "ai_IsBuildingDestroyer"), "true", "爆墙炸弹 AI 建筑破坏标记")
    assert_equal(child_text(verb, "defaultProjectile"), "MMT_Proj_WallBreachCharge", "爆墙炸弹投射物指向")
    if verb.find("onlyManualCast") is not None:
        fail("爆墙炸弹不应设 onlyManualCast（否则 AI 不会自动使用）")

    # ---- 投射物：命中即爆、伤害10、引用自定义 DamageDef ----
    projs = def_map(parse_xml(PROJ_XML))
    if "MMT_Proj_WallBreachCharge" not in projs:
        fail("投射物缺少 MMT_Proj_WallBreachCharge")
    proj = projs["MMT_Proj_WallBreachCharge"]
    assert_equal(child_text(proj, "projectile/damageDef"), "MMT_BreachBlast", "投射物伤害类型")
    assert_equal(child_text(proj, "projectile/damageAmountBase"), "10", "投射物基础伤害")
    assert_equal(child_text(proj, "projectile/explosionRadius"), "1.9", "投射物爆炸半径")
    assert_equal(child_text(proj, "graphicData/texPath"), "Things/Projectile/WallBreach", "投射物贴图路径")
    if proj.find("projectile/explosionDelay") is not None:
        fail("爆墙炸弹投射物应为命中即爆（不设 explosionDelay）")

    # ---- 携带者 Hediff：VerbGiver 授予投掷动词（额外携带、不占武器位） ----
    hediffs = def_map(parse_xml(HEDIFF_XML))
    if "MMT_Hediff_BreachChargeCarrier" not in hediffs:
        fail("Hediff 缺少 MMT_Hediff_BreachChargeCarrier")
    hediff = hediffs["MMT_Hediff_BreachChargeCarrier"]
    vg = next(li for li in hediff.findall("comps/li")
              if li.attrib.get("Class") == "HediffCompProperties_VerbGiver")
    verb = vg.find("verbs/li")
    if verb is None:
        fail("携带者 Hediff 缺少 VerbGiver 动词")
    assert_equal(child_text(verb, "ai_IsBuildingDestroyer"), "true", "携带者动词 AI 建筑破坏标记")
    assert_equal(child_text(verb, "defaultProjectile"), "MMT_Proj_WallBreachCharge", "携带者动词投射物指向")
    if verb.find("onlyManualCast") is not None:
        fail("携带者动词不应设 onlyManualCast（否则 AI 不会自动使用）")

    # ---- 投掷作业：驱动类指向自定义 JobDriver ----
    jobs = def_map(parse_xml(JOB_XML))
    if "MMT_ThrowBreachCharge" not in jobs:
        fail("作业缺少 MMT_ThrowBreachCharge")
    job = jobs["MMT_ThrowBreachCharge"]
    assert_equal(child_text(job, "driverClass"), "MyModeTest.JobDriver_ThrowBreachCharge", "投掷作业驱动类")

    print("breachcharge: OK")


SUITES = {"breachcharge": validate_breachcharge}
