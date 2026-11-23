#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""随机强化/附魔掉落静态验证（RPG 支柱一，mmt-rpg-design.md §1）。

校验点：
- LootRoll.cs 词条池分组 = 模块 XML 档位注释实际分组（防新增模块漏入池）；
- 近战/远程限定与危险特效清单完整，且都是池内有效 defName；
- 掷点入口/钩子/comp 写入方法/设置项接线齐全。
"""
from __future__ import annotations

import re

from validate_mod import fail, require_files

LOOTROLL_CS = "Source/MyModeTest/LootRoll.cs"
WEAPON_ENH_CS = "Source/MyModeTest/WeaponEnhance.cs"
APPAREL_ENH_CS = "Source/MyModeTest/ApparelEnhance.cs"
SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"
MOD_CS = "Source/MyModeTest/MyModeTestMod.cs"
WEAPON_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_WeaponModules.xml"
APPAREL_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_ApparelModules.xml"

TIERS = ("低级", "中级", "高级", "极品")
TIER_ALIAS = {"极级": "极品"}  # XML 注释笔误统一归并

# 设计定死的限定/危险清单（mmt-rpg-design.md §1.3）
EXPECTED_MELEE_ONLY = {"MMT_WeaponModLifeLeech", "MMT_WeaponModVampire"}
EXPECTED_RANGED_ONLY = {"MMT_WeaponModRangedLeech", "MMT_WeaponModMagazine", "MMT_WeaponModLongRange"}
EXPECTED_HAZARD = {
    "MMT_WeaponModExplode", "MMT_WeaponModStormCall", "MMT_WeaponModGas",
    "MMT_WeaponModSmoke", "MMT_WeaponModCorpseBurst",
    "MMT_ApparelModThorns", "MMT_ApparelModDischarge", "MMT_ApparelModToxicCloud",
}


def _read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def _xml_tier_groups(path: str, prefix: str) -> dict[str, set[str]]:
    """按 XML 注释档位归组：每条 defName 归属其前方最近的档位注释。

    档位注释只含一个档位关键词（文件头"档位材料"行同时含多个关键词，自动跳过）。
    """
    groups: dict[str, set[str]] = {t: set() for t in TIERS}
    current = None
    for line in _read(path).splitlines():
        comment = re.search(r"<!--(.*?)-->", line)
        if comment:
            text = comment.group(1)
            hits = [t for t in TIERS + ("极级",) if t in text]
            if len(hits) == 1:
                current = TIER_ALIAS.get(hits[0], hits[0])
        defname = re.search(r"<defName>(MMT_\w+)</defName>", line)
        if defname:
            name = defname.group(1)
            if not name.startswith(prefix):
                fail(f"{path} 出现非本文件前缀 defName: {name}")
            if current is None:
                fail(f"{name} 之前没有档位注释，无法归组")
            groups[current].add(name)
    return groups


def _cs_pool(cs: str, array_name: str) -> set[str]:
    m = re.search(array_name + r"\s*=\s*\{(.*?)\}", cs, re.S)
    if not m:
        fail(f"LootRoll.cs 缺少池数组 {array_name}")
    return set(re.findall(r"MMT_\w+", m.group(1)))


def _cs_string_set(cs: str, set_name: str) -> set[str]:
    m = re.search(set_name + r"\s*=\s*new HashSet<string>\s*\{(.*?)\}", cs, re.S)
    if not m:
        fail(f"LootRoll.cs 缺少清单 {set_name}")
    return set(re.findall(r"MMT_\w+", m.group(1)))


def _check_pool(cs: str, xml_path: str, prefix: str, kind: str) -> set[str]:
    xml_groups = _xml_tier_groups(xml_path, prefix)
    all_names: set[str] = set()
    for tier, xml_set in xml_groups.items():
        cs_set = _cs_pool(cs, f"{kind}{ {'低级':'Low','中级':'Mid','高级':'High','极品':'Extreme'}[tier] }")
        if cs_set != xml_set:
            fail(f"{kind}池·{tier} 与 XML 档位注释不一致："
                 f"C# 多出 {sorted(cs_set - xml_set)}，缺少 {sorted(xml_set - cs_set)}")
        all_names |= cs_set
    return all_names


def validate_loot_roll() -> None:
    require_files([LOOTROLL_CS, WEAPON_ENH_CS, APPAREL_ENH_CS, SETTINGS_CS, MOD_CS,
                   WEAPON_XML, APPAREL_XML])

    cs = _read(LOOTROLL_CS)

    # ---- 词条池 = XML 档位注释分组 ----
    weapon_pool = _check_pool(cs, WEAPON_XML, "MMT_WeaponMod", "Weapon")
    apparel_pool = _check_pool(cs, APPAREL_XML, "MMT_ApparelMod", "Apparel")

    # ---- 限定/危险清单 ----
    if _cs_string_set(cs, "MeleeOnlyWeaponMods") != EXPECTED_MELEE_ONLY:
        fail("MeleeOnlyWeaponMods 与设计清单不一致: " + str(sorted(_cs_string_set(cs, "MeleeOnlyWeaponMods"))))
    if _cs_string_set(cs, "RangedOnlyWeaponMods") != EXPECTED_RANGED_ONLY:
        fail("RangedOnlyWeaponMods 与设计清单不一致: " + str(sorted(_cs_string_set(cs, "RangedOnlyWeaponMods"))))
    if _cs_string_set(cs, "HazardMods") != EXPECTED_HAZARD:
        fail("HazardMods 与设计清单不一致: " + str(sorted(_cs_string_set(cs, "HazardMods"))))
    for name in EXPECTED_MELEE_ONLY | EXPECTED_RANGED_ONLY:
        if name not in weapon_pool:
            fail(f"限定词条 {name} 不在武器池内")
    for name in EXPECTED_HAZARD:
        if name not in weapon_pool and name not in apparel_pool:
            fail(f"危险特效词条 {name} 不在任何池内")

    # ---- 掷点入口与钩子 ----
    for token in ("TierForPoints", "RollForPoints", "public static void Roll(ThingWithComps gear, int tier)",
                  "TryRaiderThreatPoints", "StorytellerUtility.DefaultThreatPointsNow",
                  "nameof(PawnWeaponGenerator.TryGenerateWeaponFor)",
                  "nameof(PawnApparelGenerator.GenerateStartingApparelFor)",
                  "MMTSettings.LootRollEnabled", "MMTSettings.LootRollChanceFactor"):
        if token not in cs:
            fail(f"LootRoll.cs 缺少: {token}")

    # ---- comp 写入方法 ----
    if "public void SetLevelForLoot(int newLevel)" not in _read(WEAPON_ENH_CS):
        fail("CompWeaponEnhancement 缺少 SetLevelForLoot")
    if "public void SetLevelForLoot(int newLevel)" not in _read(APPAREL_ENH_CS):
        fail("CompApparelEnhancement 缺少 SetLevelForLoot")

    # ---- 设置项接线（字段 + 持久化 + 面板） ----
    settings = _read(SETTINGS_CS)
    for field in ("lootRollEnabled", "lootRollChanceFactor"):
        if field not in settings:
            fail(f"MMTSettings 缺少字段 {field}")
        if f'"{field}"' not in settings:
            fail(f"MMTSettings.ExposeData 未持久化字段 {field}")
    mod = _read(MOD_CS)
    if "s.lootRollEnabled" not in mod or "s.lootRollChanceFactor" not in mod:
        fail("设置面板缺少随机掉落强化/附魔条目")

    print(f"loot_roll: OK（武器池 {len(weapon_pool)} 条，防具池 {len(apparel_pool)} 条）")


SUITES = {"loot_roll": validate_loot_roll}
