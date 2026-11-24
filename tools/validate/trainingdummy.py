#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""沙袋人偶静态验证：高耐久/自修复/娱乐 joyKind/贴图/C# 统计。"""
from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_TrainingDummy.xml"
CS = "Source/MyModeTest/TrainingDummy.cs"
TEX_DIR = "1.7/Textures/Things/Building/Production/MMT_TrainingDummy"


def validate_trainingdummy() -> None:
    require_files([XML, CS])

    defs = def_map(parse_xml(XML))
    if "MMT_TrainingDummy" not in defs:
        fail("缺少 MMT_TrainingDummy")
        return
    node = defs["MMT_TrainingDummy"]

    assert_equal(child_text(node, "thingClass"), "MyModeTest.Building_TrainingDummy", "假人 thingClass")
    assert_equal(child_text(node, "designationCategory"), "Joy", "假人建造分类")

    hp = int(child_text(node, "statBases/MaxHitPoints") or "0")
    if hp < 10000:
        fail(f"假人耐久过低（{hp}），测试用途要求 ≥10000")
    flam = float(child_text(node, "statBases/Flammability") or "1")
    if flam > 0:
        fail("假人必须不可燃（Flammability=0），否则测燃烧武器会烧掉")
    if float(child_text(node, "statBases/JoyGainFactor") or "0") <= 0:
        fail("假人缺 JoyGainFactor（娱乐建筑必需）")
    assert_equal(child_text(node, "building/joyKind"), "Gaming_Dexterity", "假人 joyKind")
    assert_equal(child_text(node, "passability"), "PassThroughOnly", "假人通行性（避免 Impassable+fillPercent<1 的 config error）")

    for f in ("MMT_TrainingDummy_south.png", "MMT_TrainingDummy_east.png", "MMT_TrainingDummy_north.png"):
        if not Path(f"{TEX_DIR}/{f}").exists():
            fail(f"假人缺少贴图 {TEX_DIR}/{f}")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("Building_TrainingDummy", "TakeDamage", "Tick", "清零承伤统计", "近10秒秒伤"):
        if token not in cs:
            fail(f"TrainingDummy.cs 缺少: {token}")

    print("trainingdummy: OK")


SUITES = {"trainingdummy": validate_trainingdummy}
