#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""历练/Perk 框架静态验证（mmt-rpg-design.md §2.1/§2.2 + mmt-class-design.md §5.6）。

校验点：
- MMT_PawnProgress 20 个 stage minSeverity 1..20 连续；
- 19 个 perk Hediff 与 PawnProgress.cs PerkTable 一致（defName/花费）；
- perk/历练 stat 引用有效（对照游戏 StatDef）；
- 导师台造价物品与凭证 defName 有效；
- Human comp/ITab 补丁与 Keyed 齐全。
"""
from __future__ import annotations

import os
import re

from validate_mod import ROOT, fail, require_files

GAME_STATS = r"D:/RimWorld v1.7.4979/Data/Core/Defs/Stats"
GAME_STATS_ROYALTY = r"D:/RimWorld v1.7.4979/Data/Royalty/Defs"
HEDIFFS_XML = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_PawnProgress.xml")
PATCH_XML = os.path.join(ROOT, "1.7/Patches/MMT_Patch_PawnProgress.xml")
PROGRESS_CS = os.path.join(ROOT, "Source/MyModeTest/PawnProgress.cs")
BENCH_XML = os.path.join(ROOT, "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ClassMentor.xml")
TOKEN_XML = os.path.join(ROOT, "1.7/Defs/ThingDefs_Items/MMT_Items_ClassToken.xml")
KEYED_XML = os.path.join(ROOT, "1.7/Languages/ChineseSimplified/Keyed/MMT_InspectorTabs.xml")


def _read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def _game_stats() -> set[str]:
    stats: set[str] = set()
    dirs = [GAME_STATS]
    for sub in ("Stats",):
        d = os.path.join(GAME_STATS_ROYALTY, sub)
        if os.path.isdir(d):
            dirs.append(d)
    for d in dirs:
        for fn in os.listdir(d):
            if fn.endswith(".xml"):
                stats |= set(re.findall(r"<defName>([^<]+)</defName>", _read(os.path.join(d, fn))))
    return stats


def validate_pawn_progress() -> None:
    require_files([os.path.relpath(p, ROOT) for p in [HEDIFFS_XML, PATCH_XML, PROGRESS_CS, BENCH_XML, TOKEN_XML, KEYED_XML]])
    xml = _read(HEDIFFS_XML)
    cs = _read(PROGRESS_CS)
    stats = _game_stats()

    # 20 个 stage 连续
    block = re.search(r"<defName>MMT_PawnProgress</defName>.*?</HediffDef>", xml, re.S)
    if not block:
        fail("缺少 MMT_PawnProgress HediffDef")
        return
    severities = [int(s) for s in re.findall(r"<minSeverity>(\d+)</minSeverity>", block.group(0))]
    if severities != list(range(1, 21)):
        fail("MMT_PawnProgress stage minSeverity 应为 1..20 连续，实际 %s" % severities)

    # stat 引用有效（历练 + 全部 perk）
    for stat in re.findall(r"<(statOffsets|statFactors)>(.*?)</\1>", xml, re.S):
        for name in re.findall(r"<([A-Za-z0-9_]+)>[^<]+</\1>", stat[1]):
            if name not in stats:
                fail("stat %s 不是有效的 1.7 StatDef" % name)

    # perk 表一致：XML defName 集合 = C# PerkTable 键集合，且数量 19
    xml_perks = set(re.findall(r"<defName>(MMT_Perk_[A-Za-z]+)</defName>", xml))
    cs_perks = set(re.findall(r'\{ "(MMT_Perk_[A-Za-z]+)",', cs))
    if xml_perks != cs_perks:
        fail("perk XML 与 C# PerkTable 不一致：仅XML %s / 仅C# %s"
             % (sorted(xml_perks - cs_perks), sorted(cs_perks - xml_perks)))
    if len(cs_perks) != 19:
        fail("perk 总数应为 19（18 + 品质暴击），实际 %d" % len(cs_perks))
    if 'MMT_Perk_Masterwork", ("生产", 2)' not in cs:
        fail("MMT_Perk_Masterwork 应为生产支、消耗 2 点")

    # 补丁与 Keyed
    patch = _read(PATCH_XML)
    if "CompProperties_PawnProgress" not in patch or "ITab_Pawn_Progress" not in patch:
        fail("Human comp/ITab 补丁不完整")
    if "MMT_TabPawnProgress" not in _read(KEYED_XML):
        fail("缺少 Keyed MMT_TabPawnProgress")

    # 导师台与凭证
    bench = _read(BENCH_XML)
    for mat in ("MMT_Microchip", "MMT_PobediteAlloy", "MMT_ReinforcedGlass"):
        if mat not in bench:
            fail("导师台造价缺少 %s" % mat)
    for mat in ("MMT_Microchip", "MMT_PobediteAlloy", "MMT_ReinforcedGlass"):
        found = False
        for dirpath, _dirs, files in os.walk(os.path.join(ROOT, "1.7/Defs")):
            for fn in files:
                if fn.endswith(".xml") and ("<defName>%s</defName>" % mat) in _read(os.path.join(dirpath, fn)):
                    found = True
                    break
            if found:
                break
        if not found:
            fail("导师台造价物品 %s 不存在" % mat)
    token = _read(TOKEN_XML)
    if "<defName>MMT_ClassToken</defName>" not in token:
        fail("缺少 MMT_ClassToken 凭证")
    token_tex = re.search(r"<texPath>([^<]+)</texPath>", token)
    if token_tex and not os.path.isfile(os.path.join(ROOT, "1.7/Textures", token_tex.group(1) + ".png")):
        fail("凭证贴图缺失 %s" % token_tex.group(1))
    bench_tex = re.search(r"<texPath>([^<]+)</texPath>", bench)
    if bench_tex and not os.path.isfile(os.path.join(ROOT, "1.7/Textures", bench_tex.group(1) + ".png")):
        fail("导师台贴图缺失 %s" % bench_tex.group(1))


SUITES = {"pawn_progress": validate_pawn_progress}
