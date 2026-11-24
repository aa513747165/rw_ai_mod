#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""副本蓝图设计器静态验证：建筑师分类挂载/设计器图标/C# 关键类与 Stamp 分支。"""
from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

XML = "1.7/Defs/DesignationCategoryDefs/MMT_DesignationCategories.xml"
CS = "Source/MyModeTest/DungeonBlueprint.cs"
DUNGEON_CS = "Source/MyModeTest/Dungeon.cs"
SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"
ICON = "1.7/Textures/UI/Designators/MMT_BlueprintSave.png"


def validate_dungeon_blueprint() -> None:
    require_files([XML, CS, DUNGEON_CS, SETTINGS_CS])

    defs = def_map(parse_xml(XML))
    node = defs.get("MMT_Cat_DungeonBlueprint")
    if node is None:
        fail("缺少建筑师分类 MMT_Cat_DungeonBlueprint")
        return
    assert_equal(child_text(node, "label"), "副本蓝图", "蓝图分类 label")
    classes = [li.text.strip() for li in node.findall("./specialDesignatorClasses/li") if li.text]
    if classes != ["MyModeTest.Designator_SaveDungeonBlueprint"]:
        fail(f"specialDesignatorClasses 应为 [MyModeTest.Designator_SaveDungeonBlueprint]，实际 {classes}")

    if not Path(ICON).exists():
        fail(f"设计器缺少图标 {ICON}")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("class DungeonBlueprint : IExposable", "class Designator_SaveDungeonBlueprint : Designator",
                  "class Dialog_NameDungeonBlueprint", "class DungeonBlueprintStore",
                  "enabledSiteParts", "enabledTiers", "EnabledFor", "EnabledAtTier", "GetThumbnail", "PickFor"):
        if token not in cs:
            fail(f"DungeonBlueprint.cs 缺少关键成员：{token}")

    dcs = Path(DUNGEON_CS).read_text(encoding="utf-8")
    for token in ("DungeonBlueprintStore.PickFor(def, tier)", "StampBlueprint(map, blueprint, center, faction"):
        if token not in dcs:
            fail(f"Dungeon.cs worker 未接蓝图分支：{token}")

    scs = Path(SETTINGS_CS).read_text(encoding="utf-8")
    for token in ("dungeonBlueprints", 'Scribe_Collections.Look(ref dungeonBlueprints'):
        if token not in scs:
            fail(f"MMTSettings.cs 缺少蓝图存档：{token}")

    print("dungeon_blueprint: OK")


SUITES = {"dungeon_blueprint": validate_dungeon_blueprint}
