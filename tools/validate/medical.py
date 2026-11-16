#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""战地医疗装备链静态验证（移植自超级医疗科技，新链，由 tools/validate_new.py 自动发现）。

链路构成：
- MMT_MedicalBackpack（战地医疗背包，Belt 层充能装备，配方挂精密装配台）
  → MMT_MedAerosolShell（气雾胶囊弹体，MyModeTest.Projectile_MMTMedBurst）
  → MMT_NaniteHealing（纳米医疗 hediff，伤口愈合 ×5，1 小时）
- MMT_HealTurret（小型治疗炮塔，MyModeTest.Building_MMTHealTurret）
  → MMT_HealTurret_Gun（塔顶武器数据）→ MMT_HealBolt（零伤害弹体）→ MMT_HealBoltImpact（零伤害 DamageDef）
- 研究：MMT_FieldMedicalPack（精密装配台配方前置）→ MMT_HealTurretResearch
- 运行时：Source/MyModeTest/Medical.cs（verb/弹体/hediff/建筑 + 两条 CompRefuelable 补丁）
"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    parse_xml,
    require_files,
)

ITEMS_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_FieldMedical.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_FieldMedical.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_FieldMedical.xml"
SOUND_XML = "1.7/Defs/SoundDefs/MMT_Sounds_FieldMedical.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_FieldMedical.xml"
KEYED_XML = "1.7/Languages/ChineseSimplified/Keyed/MMT_FieldMedical.xml"
CS_FILE = "Source/MyModeTest/Medical.cs"

TEXTURES = [
    "1.7/Textures/Things/Pawn/Humanlike/Apparel/MMT_Medpack/MMT_Medpack_south.png",
    "1.7/Textures/Things/Pawn/Humanlike/Apparel/MMT_Medpack/MMT_Medpack_east.png",
    "1.7/Textures/Things/Pawn/Humanlike/Apparel/MMT_Medpack/MMT_Medpack_north.png",
    "1.7/Textures/Things/Building/Security/MMT_MedTurret_Top.png",
    "1.7/Textures/Things/Building/Security/MMT_MedTurret_Building.png",
    "1.7/Textures/Things/Projectile/MMT_HealBolt.png",
    "1.7/Textures/Things/Projectile/MMT_MedAerosolShell.png",
    "1.7/Textures/Things/MMT_MistCloud.png",
]
SOUND_WAV = "1.7/Sounds/MMT_HealTurret/click_soft.wav"

# 原作固定数值（移植不得漂移）
BACKPACK_COST = {"MedicineUltratech": "2", "Plasteel": "30", "ComponentSpacer": "4"}
TURRET_COST = {"Steel": "20", "ComponentSpacer": "1", "ComponentIndustrial": "2"}


def _cost_map(elem):
    return {c.tag: (c.text or "").strip() for c in elem.findall("costList/*")}


def validate_medical() -> None:
    require_files([ITEMS_XML, HEDIFF_XML, BUILDING_XML, SOUND_XML, RESEARCH_XML, KEYED_XML, CS_FILE, SOUND_WAV])
    require_files(TEXTURES)

    # ---- 背包与气雾胶囊 ----
    defs = def_map(parse_xml(ITEMS_XML))
    for name in ("MMT_MedicalBackpack", "MMT_MedAerosolShell", "MMT_MedMist", "MMT_MedMistMid", "MMT_MedMistShort"):
        if name not in defs:
            fail(f"缺少 Def: {name}")

    pack = defs["MMT_MedicalBackpack"]
    if pack.find("apparel/layers/li") is None or pack.findtext("apparel/layers/li") != "Belt":
        fail("MMT_MedicalBackpack 必须在 Belt 层")
    if pack.findtext("verbs/li/verbClass") != "MyModeTest.Verb_MMTMedPackLaunch":
        fail("MMT_MedicalBackpack verbClass 必须是 MyModeTest.Verb_MMTMedPackLaunch")
    if pack.findtext("verbs/li/defaultProjectile") != "MMT_MedAerosolShell":
        fail("MMT_MedicalBackpack 弹体必须是 MMT_MedAerosolShell")
    if _cost_map(pack) != BACKPACK_COST:
        fail(f"MMT_MedicalBackpack 造价漂移: {_cost_map(pack)}")
    reloadable = pack.find("comps/li[@Class='CompProperties_ApparelReloadable']")
    if reloadable is None:
        fail("MMT_MedicalBackpack 缺少 CompProperties_ApparelReloadable")
    else:
        assert_equal(reloadable.findtext("ammoDef"), "MedicineUltratech", "背包装填弹药")
        assert_equal(reloadable.findtext("ammoCountPerCharge"), "2", "背包装填数量")
    if pack.findtext("recipeMaker/researchPrerequisite") != "MMT_FieldMedicalPack":
        fail("MMT_MedicalBackpack 配方研究前置必须是 MMT_FieldMedicalPack")

    shell = defs["MMT_MedAerosolShell"]
    if shell.findtext("thingClass") != "MyModeTest.Projectile_MMTMedBurst":
        fail("MMT_MedAerosolShell thingClass 必须是 MyModeTest.Projectile_MMTMedBurst")

    # ---- 纳米医疗 hediff ----
    hdefs = def_map(parse_xml(HEDIFF_XML))
    if "MMT_NaniteHealing" not in hdefs:
        fail("缺少 HediffDef: MMT_NaniteHealing")
    nanite = hdefs["MMT_NaniteHealing"]
    if nanite.findtext("hediffClass") != "MyModeTest.Hediff_MMTNaniteHealing":
        fail("MMT_NaniteHealing hediffClass 必须是 MyModeTest.Hediff_MMTNaniteHealing")
    assert_equal(nanite.findtext("comps/li[@Class='HediffCompProperties_Disappears']/disappearsAfterTicks"),
                 "2500", "纳米 hediff 持续时间（1 小时）")
    assert_equal(nanite.findtext("stages/li/statFactors/InjuryHealingFactor"), "5", "纳米 hediff 愈合倍率")

    # ---- 治疗炮塔 ----
    bdefs = def_map(parse_xml(BUILDING_XML))
    for name in ("MMT_HealBoltImpact", "MMT_HealBolt", "MMT_HealTurret_Gun", "MMT_HealTurret"):
        if name not in bdefs:
            fail(f"缺少 Def: {name}")
    if bdefs["MMT_HealBoltImpact"].tag != "DamageDef":
        fail("MMT_HealBoltImpact 必须是 DamageDef")
    assert_equal(bdefs["MMT_HealBoltImpact"].findtext("defaultDamage"), "0", "医疗软弹命中伤害必须为 0")
    bolt = bdefs["MMT_HealBolt"]
    if bolt.findtext("thingClass") != "MyModeTest.Projectile_MMTHealBolt":
        fail("MMT_HealBolt thingClass 必须是 MyModeTest.Projectile_MMTHealBolt")
    assert_equal(bolt.findtext("projectile/damageDef"), "MMT_HealBoltImpact", "医疗软弹 damageDef")

    turret = bdefs["MMT_HealTurret"]
    if turret.findtext("thingClass") != "MyModeTest.Building_MMTHealTurret":
        fail("MMT_HealTurret thingClass 必须是 MyModeTest.Building_MMTHealTurret")
    if _cost_map(turret) != TURRET_COST:
        fail(f"MMT_HealTurret 造价漂移: {_cost_map(turret)}")
    assert_equal(turret.findtext("costStuffCount"), "30", "炮塔金属材质数量")
    assert_equal(turret.findtext("statBases/ShootingAccuracyTurret"), "0.96", "炮塔精度")
    fuel = turret.find("comps/li[@Class='CompProperties_Refuelable']")
    if fuel is None:
        fail("MMT_HealTurret 缺少 CompProperties_Refuelable（弹药计数）")
    else:
        assert_equal(fuel.findtext("fuelCapacity"), "8", "炮塔弹容量")
        assert_equal(fuel.findtext("fuelLabel"), "MMT.HealTurretAmmo", "炮塔 fuelLabel")
        assert_equal(fuel.findtext("fuelGizmoLabel"), "MMT.HealTurretAmmoGizmo", "炮塔 fuelGizmoLabel")
    # 故意不带爆炸 comp（被打爆不该炸伤病人）
    if turret.find("comps/li[@Class='CompProperties_Explosive']") is not None:
        fail("MMT_HealTurret 不得带 CompProperties_Explosive")
    if turret.findtext("placeWorkers/li") != "MyModeTest.PlaceWorker_MMTHealTurretRange":
        fail("MMT_HealTurret placeWorker 必须是 MyModeTest.PlaceWorker_MMTHealTurretRange")
    if turret.findtext("researchPrerequisites/li") != "MMT_HealTurretResearch":
        fail("MMT_HealTurret 建造研究前置必须是 MMT_HealTurretResearch")
    gun = bdefs["MMT_HealTurret_Gun"]
    if gun.findtext("verbs/li/defaultProjectile") != "MMT_HealBolt":
        fail("MMT_HealTurret_Gun 弹体必须是 MMT_HealBolt")
    if gun.findtext("verbs/li/soundCast") != "MMT_HealTurret_ClickSoft":
        fail("MMT_HealTurret_Gun 开火音效必须是 MMT_HealTurret_ClickSoft")

    # ---- 音效 ----
    sdefs = def_map(parse_xml(SOUND_XML))
    if "MMT_HealTurret_ClickSoft" not in sdefs:
        fail("缺少 SoundDef: MMT_HealTurret_ClickSoft")
    if sdefs["MMT_HealTurret_ClickSoft"].findtext("subSounds/li/grains/li/clipPath") != "MMT_HealTurret/click_soft":
        fail("MMT_HealTurret_ClickSoft clipPath 必须是 MMT_HealTurret/click_soft")

    # ---- 研究 ----
    rdefs = def_map(parse_xml(RESEARCH_XML))
    for name in ("MMT_FieldMedicalPack", "MMT_HealTurretResearch"):
        if name not in rdefs:
            fail(f"缺少研究: {name}")
    pack_r = rdefs["MMT_FieldMedicalPack"]
    pack_pre = [li.text for li in pack_r.findall("prerequisites/li")]
    if "MMT_Medicine_D1" not in pack_pre or "SmokepopBelt" not in pack_pre:
        fail(f"MMT_FieldMedicalPack 前置须含 MMT_Medicine_D1 + SmokepopBelt: {pack_pre}")
    turret_pre = [li.text for li in rdefs["MMT_HealTurretResearch"].findall("prerequisites/li")]
    if "MMT_FieldMedicalPack" not in turret_pre or "GunTurrets" not in turret_pre:
        fail(f"MMT_HealTurretResearch 前置须含 MMT_FieldMedicalPack + GunTurrets: {turret_pre}")

    # ---- C# 关键符号 ----
    with open(CS_FILE, encoding="utf-8") as f:
        cs = f.read()
    for sym in ("class MedUtil", "class Verb_MMTMedPackLaunch", "class Projectile_MMTMedBurst",
                "class Projectile_MMTHealBolt", "class Hediff_MMTNaniteHealing",
                "class Building_MMTHealTurret", "class PlaceWorker_MMTHealTurretRange",
                "CompRefuelable_PostDraw_Patch", "CompRefuelable_InspectString_Patch"):
        if sym not in cs:
            fail(f"Medical.cs 缺少: {sym}")


SUITES = {"medical": validate_medical}
