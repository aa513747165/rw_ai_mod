#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""医疗注射炮台静态验证：建筑 + 投射物 + 作业 + WorkGiver + 研究 + 贴图（由 tools/validate_new.py 自动发现）。"""
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

BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_MedicalTurret.xml"
PROJECTILE_XML = "1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
WORKGIVERS_XML = "1.7/Defs/WorkGiverDefs/MMT_WorkGivers.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Medicine.xml"

TEX_BASE = "1.7/Textures/Things/Building/Security/MMT_MedicalTurret_Base.png"
TEX_ICO = "1.7/Textures/Things/Building/Security/MMT_MedicalTurret_ico.png"
TEX_SYRINGE = "1.7/Textures/Things/Projectile/MMT_Syringe.png"


def validate_medicalturret() -> None:
    require_files([BUILDING_XML, PROJECTILE_XML, JOBS_XML, WORKGIVERS_XML, RESEARCH_XML])

    # ---- 建筑：自定义 Comp + 电力 + 研究前置 ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_MedicalTurret" not in buildings:
        fail("建筑缺少 MMT_MedicalTurret")
    b = buildings["MMT_MedicalTurret"]
    assert_equal(child_text(b, "thingClass"), "Building", "注射炮台 thingClass（非炮塔 AI）")
    comp = next((li for li in b.findall("comps/li")
                 if li.attrib.get("Class") == "MyModeTest.CompProperties_MedicalTurret"), None)
    if comp is None:
        fail("注射炮台缺少 MyModeTest.CompProperties_MedicalTurret")
    assert_equal(child_text(comp, "range"), "25", "注射炮台射程")
    assert_equal(child_text(comp, "cooldownTicks"), "180", "注射炮台冷却")
    assert_equal(child_text(comp, "maxPerKind"), "5", "注射炮台每种药品上限")
    if b.find("comps/li[@Class='CompProperties_Power']") is None:
        fail("注射炮台缺少电力 Comp")
    assert_equal(li_values(b, "researchPrerequisites"), ["MMT_Medicine_C1"], "注射炮台研究前置")

    # ---- 投射物：自定义必中注射弹 ----
    projectiles = def_map(parse_xml(PROJECTILE_XML))
    if "MMT_Bullet_MedicalSyringe" not in projectiles:
        fail("投射物缺少 MMT_Bullet_MedicalSyringe")
    p = projectiles["MMT_Bullet_MedicalSyringe"]
    assert_equal(child_text(p, "thingClass"), "MyModeTest.Projectile_MedicalSyringe", "注射弹 thingClass")
    assert_equal(child_text(p, "projectile/speed"), "60", "注射弹速度")

    # ---- 作业：装填 + 取出 ----
    jobs = def_map(parse_xml(JOBS_XML))
    if "MMT_LoadMedicalTurret" not in jobs or "MMT_UnloadMedicalTurret" not in jobs:
        fail("JobDef 缺少 MMT_LoadMedicalTurret 或 MMT_UnloadMedicalTurret")
    assert_equal(child_text(jobs["MMT_LoadMedicalTurret"], "driverClass"),
                 "MyModeTest.JobDriver_LoadMedicalTurret", "装填作业 driverClass")
    assert_equal(child_text(jobs["MMT_UnloadMedicalTurret"], "driverClass"),
                 "MyModeTest.JobDriver_UnloadMedicalTurret", "取出作业 driverClass")

    # ---- WorkGiver：自动装填（搬运工作） ----
    wgs = def_map(parse_xml(WORKGIVERS_XML))
    if "MMT_LoadMedicalTurret" not in wgs:
        fail("WorkGiver 缺少 MMT_LoadMedicalTurret")
    wg = wgs["MMT_LoadMedicalTurret"]
    assert_equal(child_text(wg, "giverClass"), "MyModeTest.WorkGiver_LoadMedicalTurret", "装填 WorkGiver giverClass")
    assert_equal(child_text(wg, "workType"), "Hauling", "装填 WorkGiver 工作类型")

    # ---- 研究：医疗物资 V，前置 B2，坐标不与其他研究重叠 ----
    research = def_map(parse_xml(RESEARCH_XML))
    if "MMT_Medicine_C1" not in research:
        fail("研究缺少 MMT_Medicine_C1")
    if "MMT_Medicine_B2" not in research:
        fail("研究前置 MMT_Medicine_B2 不存在")
    r = research["MMT_Medicine_C1"]
    assert_equal(li_values(r, "prerequisites"), ["MMT_Medicine_B2"], "医疗物资 V 前置")
    coords = {}
    for name, node in research.items():
        key = (child_text(node, "researchViewX"), child_text(node, "researchViewY"))
        if key in coords:
            fail("研究坐标重叠：%s 与 %s 同为 %s" % (coords[key], name, key))
        coords[key] = name

    # ---- 贴图 ----
    assert_png(TEX_BASE)
    assert_png(TEX_ICO)
    assert_png(TEX_SYRINGE)

    print("medicalturret: OK")


SUITES = {"medicalturret": validate_medicalturret}
