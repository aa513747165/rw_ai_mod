#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""百合花模因链静态验证：MMT_Lily MemeDef + 同性之爱戒律 + LilyMeme.cs 三补丁（新链，由 tools/validate_new.py 自动发现）。

设计要点（对应 1.7/Defs/MemeDefs/MMT_Memes_Lily.xml 头注释）：
- 普通模因（groupDef Misc，不占结构槽），requireOne 自动带上戒律 MMT_SameSexLove_Blessed；
- 女女恋爱去惩罚 / 亲热怀孕 / 后代必女三条机制全在 Source/MyModeTest/LilyMeme.cs 按 HasMeme 生效；
- 图标为自绘白色剪影 1.7/Textures/UI/Memes/MMT_Lily.png。
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

MEME_XML = "1.7/Defs/MemeDefs/MMT_Memes_Lily.xml"
PRECEPT_XML = "1.7/Defs/PreceptDefs/MMT_Precepts_Lily.xml"
PATCH_CS = "Source/MyModeTest/LilyMeme.cs"
ICON_PNG = "1.7/Textures/UI/Memes/MMT_Lily.png"


def validate_lily() -> None:
    require_files([MEME_XML, PRECEPT_XML, PATCH_CS, ICON_PNG])

    # ---- 模因 Def ----
    defs = def_map(parse_xml(MEME_XML))
    if "MMT_Lily" not in defs:
        fail("缺少模因: MMT_Lily")
    meme = defs["MMT_Lily"]
    if meme.tag != "MemeDef":
        fail("MMT_Lily 必须是 MemeDef")
    assert_equal(child_text(meme, "label"), "百合花", "MMT_Lily label")
    assert_equal(child_text(meme, "iconPath"), "UI/Memes/MMT_Lily", "MMT_Lily 图标")
    assert_equal(child_text(meme, "groupDef"), "Misc", "MMT_Lily 组")
    # 必须是普通模因：不得带 category/structure 专属字段
    for tag in ("category", "worshipRoomLabel", "deityCount"):
        if meme.find(tag) is not None:
            fail(f"MMT_Lily 不得含 structure 专属字段: {tag}")
    # requireOne 必须指向专属戒律（选模因自动带上）
    req = [li.text for li in meme.findall("requireOne/li/li")]
    if "MMT_SameSexLove_Blessed" not in req:
        fail("MMT_Lily requireOne 缺少 MMT_SameSexLove_Blessed")

    # ---- 戒律 / 议题 Def ----
    pdefs = def_map(parse_xml(PRECEPT_XML))
    if "MMT_SameSexLove" not in pdefs:
        fail("缺少议题: MMT_SameSexLove")
    if pdefs["MMT_SameSexLove"].tag != "IssueDef":
        fail("MMT_SameSexLove 必须是 IssueDef")
    if "MMT_SameSexLove_Blessed" not in pdefs:
        fail("缺少戒律: MMT_SameSexLove_Blessed")
    precept = pdefs["MMT_SameSexLove_Blessed"]
    if precept.tag != "PreceptDef":
        fail("MMT_SameSexLove_Blessed 必须是 PreceptDef")
    assert_equal(child_text(precept, "issue"), "MMT_SameSexLove", "戒律 issue")
    memes = [li.text for li in precept.findall("associatedMemes/li")]
    if "MMT_Lily" not in memes:
        fail("MMT_SameSexLove_Blessed associatedMemes 缺少 MMT_Lily")

    # ---- C# 补丁目标与 def 引用 ----
    with open(PATCH_CS, encoding="utf-8") as f:
        cs = f.read()
    for token in (
        "MMT_Lily",
        "<MakeNewToils>b__12_4",
        "PregnancyChanceForPartners",
        "GetInheritedGeneSet",
        "SetParents",
        "PregnantHuman",
        "ApplyBirthOutcome",
        "FixedGender",
        "Gender.Female",
        "RandomSelectionWeight",
        "AttractedToGender",
        "SecondaryLovinChanceFactor",
        "BiotechActive",
        "GetNamedSilentFail",
    ):
        if token not in cs:
            fail(f"LilyMeme.cs 缺少关键内容: {token}")

    print("lily: OK")


SUITES = {"lily": validate_lily}
