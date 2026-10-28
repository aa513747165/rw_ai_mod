#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""自由意志模因链静态验证：MMT_FreeWill MemeDef + 职位心情补丁 + About 依赖（新链，由 tools/validate_new.py 自动发现）。

设计要点（对应 1.7/Defs/MemeDefs/MMT_Memes_FreeWill.xml 头注释）：
- 普通模因（不写 category），归 vanilla Misc 组，可与任意结构模因组合；
- 职位空缺/职位服装两种心情惩罚由 Source/MyModeTest/FreeWillMemePatch.cs 按 HasMeme 屏蔽；
- About.xml 必须硬依赖 Ideology DLC（MemeDef 所在 DLC）。
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

MEME_XML = "1.7/Defs/MemeDefs/MMT_Memes_FreeWill.xml"
PATCH_CS = "Source/MyModeTest/FreeWillMemePatch.cs"
ABOUT_XML = "About/About.xml"
IDEO_PACKAGE = "Ludeon.RimWorld.Ideology"


def validate_freewill() -> None:
    require_files([MEME_XML, PATCH_CS, ABOUT_XML])

    # ---- 模因 Def ----
    defs = def_map(parse_xml(MEME_XML))
    if "MMT_FreeWill" not in defs:
        fail("缺少模因: MMT_FreeWill")
    meme = defs["MMT_FreeWill"]
    if meme.tag != "MemeDef":
        fail("MMT_FreeWill 必须是 MemeDef")
    assert_equal(child_text(meme, "label"), "自由意志", "MMT_FreeWill label")
    assert_equal(
        child_text(meme, "iconPath"), "UI/Memes/Individualist", "MMT_FreeWill 图标"
    )
    assert_equal(child_text(meme, "groupDef"), "Misc", "MMT_FreeWill 组")
    assert_equal(child_text(meme, "impact"), "1", "MMT_FreeWill impact")
    # 必须是普通模因：不得带 category/structure 专属字段
    for tag in ("category", "worshipRoomLabel", "deityCount"):
        if meme.find(tag) is not None:
            fail(f"MMT_FreeWill 不得含 structure 专属字段: {tag}")

    # ---- About.xml 硬依赖 Ideology ----
    about = parse_xml(ABOUT_XML)
    deps = [li.findtext("packageId") for li in about.findall("modDependencies/li")]
    if IDEO_PACKAGE not in deps:
        fail(f"About.xml modDependencies 缺少 {IDEO_PACKAGE}")
    load_after = [li.text for li in about.findall("loadAfter/li")]
    if IDEO_PACKAGE not in load_after:
        fail(f"About.xml loadAfter 缺少 {IDEO_PACKAGE}")

    # ---- C# 补丁目标与 def 引用 ----
    with open(PATCH_CS, encoding="utf-8") as f:
        cs = f.read()
    for token in (
        "Thought_IdeoRoleEmpty",
        "Thought_IdeoRoleApparelRequirementNotMet",
        "Alert_RolesEmpty",
        "CurrentStateInternal",
        "GetTargets",
        "MMT_FreeWill",
        "ThoughtState.Inactive",
    ):
        if token not in cs:
            fail(f"FreeWillMemePatch.cs 缺少关键内容: {token}")

    print("freewill: OK")


SUITES = {"freewill": validate_freewill}
