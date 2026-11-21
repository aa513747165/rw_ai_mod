#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""能量风暴静态验证：GameCondition/Incident/阻尼器/贴图/C# 实现。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

COND_XML = "1.7/Defs/Misc/MMT_GameConditions_Endgame.xml"
DAMP_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Storm.xml"
CS = "Source/MyModeTest/Storm.cs"


def validate_storm() -> None:
    require_files([COND_XML, DAMP_XML, CS])

    defs = def_map(parse_xml(COND_XML))
    if "MMT_EnergyStorm" not in defs:
        fail("缺少能量风暴 GameCondition MMT_EnergyStorm")
    assert_equal(child_text(defs["MMT_EnergyStorm"], "conditionClass"),
                 "MyModeTest.GameCondition_MMT_EnergyStorm", "能量风暴 conditionClass")
    if "MMT_Incident_EnergyStorm" not in defs:
        fail("缺少能量风暴事件 MMT_Incident_EnergyStorm")
    incident = defs["MMT_Incident_EnergyStorm"]
    assert_equal(child_text(incident, "workerClass"), "MyModeTest.IncidentWorker_EnergyStorm", "事件 workerClass")
    assert_equal(child_text(incident, "category"), "ThreatBig", "事件类别")

    damp = def_map(parse_xml(DAMP_XML))
    if "MMT_StormDampener" not in damp:
        fail("缺少风暴阻尼器 MMT_StormDampener")
    comp_classes = [li.attrib.get("Class", "") for li in damp["MMT_StormDampener"].findall("comps/li")]
    if "MyModeTest.CompProperties_StormDampener" not in comp_classes:
        fail("阻尼器缺少 CompProperties_StormDampener")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("GameCondition_MMT_EnergyStorm", "IncidentWorker_EnergyStorm", "CanFireNowSub",
                  "ChanceFactorNow", "CompStormDampener", "IsProtected", "DoBreakdown",
                  "EnergyStormEnabled", "EnergyStormChanceFactor", "GameConditionTick"):
        if token not in cs:
            fail(f"Storm.cs 缺少: {token}")
    if "2500" not in cs:
        fail("风暴点名间隔必须 ≥2500 tick（性能原则）")

    for suffix in ("_south", "_north", "_east", "_southm", "_northm", "_eastm"):
        tex = f"1.7/Textures/Things/Building/Production/MMT_StormDampener/MMT_StormDampener{suffix}.png"
        if not Path(tex).exists():
            fail(f"阻尼器缺少贴图 {tex}")

    print("storm: OK")


SUITES = {"storm": validate_storm}
