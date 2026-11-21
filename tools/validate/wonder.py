#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""纪元奇点静态验证：四阶段链/终态三件套 comp/三纪元条件/启迪 hediff/贴图。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

WONDER_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wonder.xml"
COND_XML = "1.7/Defs/Misc/MMT_GameConditions_Endgame.xml"
CS = "Source/MyModeTest/Wonder.cs"

STAGE_CHAIN = [
    ("MMT_WonderFoundation", "MMT_WonderFrame"),
    ("MMT_WonderFrame", "MMT_WonderCore"),
    ("MMT_WonderCore", "MMT_Singularity"),
]


def validate_wonder() -> None:
    require_files([WONDER_XML, COND_XML, CS])

    defs = def_map(parse_xml(WONDER_XML))
    for stage, next_stage in STAGE_CHAIN:
        if stage not in defs:
            fail(f"缺少奇观阶段 {stage}")
            continue
        node = defs[stage]
        assert_equal(child_text(node, "size"), "(5,5)", f"{stage} 尺寸")
        assert_equal(child_text(node, "comps/li[@Class='MyModeTest.CompProperties_WonderStage']/nextStage"),
                     next_stage, f"{stage} 下一阶段")
        pw = child_text(node, "placeWorkers/li")
        assert_equal(pw, "PlaceWorker_NotUnderRoof", f"{stage} 必须露天")
        tex = f"1.7/Textures/Things/Building/Production/{stage}/{stage}.png"
        if not Path(tex).exists():
            fail(f"{stage} 缺少贴图 {tex}")
        mask = f"1.7/Textures/Things/Building/Production/{stage}/{stage}_m.png"
        if not Path(mask).exists():
            fail(f"{stage} 缺少遮罩 {mask}")

    # ---- 终态 ----
    if "MMT_Singularity" not in defs:
        fail("缺少终态 MMT_Singularity")
    final = defs["MMT_Singularity"]
    assert_equal(child_text(final, "thingClass"), "MyModeTest.Building_Singularity", "终态 thingClass")
    comp_classes = [li.attrib.get("Class", "") for li in final.findall("comps/li")]
    if comp_classes.count("MyModeTest.CompProperties_BuildingBattery") != 1:
        fail("终态蓄电 comp 必须恰好一个（同建筑不得重复挂载）")
    for need in ("CompProperties_Refuelable", "MyModeTest.CompProperties_WonderActivation"):
        if need not in comp_classes:
            fail(f"终态缺少 comp {need}")
    eras = final.findall("comps/li[@Class='MyModeTest.CompProperties_WonderActivation']/eras/li")
    if len(eras) != 3:
        fail(f"终态应有 3 个纪元选项，实际 {len(eras)}")
    else:
        era_conds = [child_text(li, "condition") for li in eras]
        for cond in ("MMT_Condition_SingularityPower", "MMT_Condition_SingularityMind", "MMT_Condition_SingularityHarvest"):
            if cond not in era_conds:
                fail(f"纪元选项缺少 {cond}")
    if not Path("1.7/Textures/Things/Building/Production/MMT_Singularity/MMT_SingularityGlow.png").exists():
        fail("缺少奇点辉光叠加层贴图")

    # ---- 条件与 hediff ----
    conds = def_map(parse_xml(COND_XML))
    for cond in ("MMT_Condition_SingularityPower", "MMT_Condition_SingularityMind", "MMT_Condition_SingularityHarvest"):
        if cond not in conds:
            fail(f"缺少 GameCondition {cond}")
    assert_equal(child_text(conds["MMT_Condition_SingularityMind"], "conditionClass"),
                 "MyModeTest.GameCondition_SingularityMind", "启迪 conditionClass")
    if "MMT_Hediff_SingularityInsight" not in conds:
        fail("缺少启迪 Hediff MMT_Hediff_SingularityInsight")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("CompWonderStage", "CompWonderActivation", "PlaceBlueprintForBuild",
                  "GameCondition_SingularityMind", "get_DesiredPowerOutput", "get_GrowthRate",
                  "buttonCText", "Building_Singularity"):
        if token not in cs:
            fail(f"Wonder.cs 缺少: {token}")

    print("wonder: OK")


SUITES = {"wonder": validate_wonder}
