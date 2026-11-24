#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""精英爆破手静态验证：PawnKindDef + 袭击注入补丁 + AI/消耗接线（由 tools/validate_new.py 自动发现）。

设计来源：docs/mmt-endgame-design.md §10.5（财富 50 万+ 袭击中编入专职爆破单位，
多枚爆墙炸弹 + 直奔墙体，点数按 combatPower 预扣，走原版袭击生成管线）。
"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    read_text,
    require_files,
)

KIND_XML = "1.7/Defs/PawnKindDefs/MMT_PawnKinds_EliteBreacher.xml"
ELITE_CS = "Source/MyModeTest/EliteBreacher.cs"
BREACH_CS = "Source/MyModeTest/BreachCharge.cs"
SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"


def validate_elitebreacher() -> None:
    require_files([KIND_XML, ELITE_CS, BREACH_CS, SETTINGS_CS])

    # ---- PawnKindDef：合格破墙者、高战力、配枪、默认海盗派系兜底 ----
    kinds = def_map(parse_xml(KIND_XML))
    if "MMT_EliteBreacher" not in kinds:
        fail("PawnKindDef 缺少 MMT_EliteBreacher")
    kind = kinds["MMT_EliteBreacher"]
    assert_equal(child_text(kind, "race"), "Human", "精英爆破手种族")
    assert_equal(child_text(kind, "defaultFactionDef"), "Pirate", "精英爆破手默认派系兜底")
    assert_equal(child_text(kind, "isGoodBreacher"), "true", "精英爆破手必须是合格破墙者")
    assert_equal(child_text(kind, "combatPower"), "180", "精英爆破手战力（=袭击点数预扣单价）")
    if "Gun" not in li_values(kind, "weaponTags"):
        fail("精英爆破手应配枪（weaponTags 含 Gun）")
    if kind.find("weaponMoney") is None or kind.find("apparelMoney") is None:
        fail("精英爆破手应有高装备预算（weaponMoney/apparelMoney）")

    # ---- 注入补丁：原版袭击生成管线、财富门槛 50 万、点数预扣、设置开关 ----
    elite = read_text(ELITE_CS)
    for needle in (
        "PawnGroupMakerUtility", "GeneratePawns", "PawnGroupMakerParms",
        "WealthThreshold = 500000f", "EliteBreacherEnabled", "parms.points -=",
        "parms.raidStrategy == null", "humanlikeFaction", "BombCount = 5",
        "Patch_EliteBreacherRaidInjection",
    ):
        if needle not in elite:
            fail(f"EliteBreacher.cs 缺少关键实现: {needle}")

    # ---- AI/消耗接线：精英分支直奔墙体 + 炸弹消耗摘 Hediff ----
    breach = read_text(BREACH_CS)
    for needle in (
        "EliteBreacherUtil.IsElite", "FindWallTarget",
        "FillCategory.Full", "Traversability.Impassable",
        "Destroy(DestroyMode.Vanish)", "RemoveHediff",
    ):
        if needle not in breach:
            fail(f"BreachCharge.cs 缺少精英/消耗接线: {needle}")

    # ---- 设置：静态访问器（字段/持久化由 settings 套件负责） ----
    if "EliteBreacherEnabled" not in read_text(SETTINGS_CS):
        fail("MMTSettings 缺少 EliteBreacherEnabled 静态访问器")

    print("elitebreacher: OK")


SUITES = {"elitebreacher": validate_elitebreacher}
