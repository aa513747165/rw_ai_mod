#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Mod 设置面板静态验证：设置字段齐全 + 各消费端接线（由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

from pathlib import Path

from validate_mod import fail, require_files

SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"
MOD_CS = "Source/MyModeTest/MyModeTestMod.cs"
PATCHES_CS = "Source/MyModeTest/SettingsPatches.cs"
BREACH_CS = "Source/MyModeTest/BreachCharge.cs"
SHIELD_CS = "Source/MyModeTest/ShieldEnhance.cs"
TELEPORT_CS = "Source/MyModeTest/TeleportPad.cs"

# 设计方案 §1 的完整字段清单（调参 + 事件开关）
REQUIRED_FIELDS = [
    "resourceYieldFactor", "researchCostFactor", "teleportBonusCharges",
    "enhanceSuccessFactor", "weaponEnhanceVanillaOnly",
    "breachWealthThreshold", "breachChanceFactor",
    "breachChargeEnabled", "energyStormEnabled", "energyStormChanceFactor",
    "empDayEnabled", "meteorShowerEnabled", "eliteBreacherEnabled",
    "blackMarketEnabled", "materialQuestsEnabled", "explorationSitesEnabled",
    "wonderOverloadEnabled", "wonderAnniversaryEnabled",
]


def _read(path: str) -> str:
    return Path(path).read_text(encoding="utf-8")


def validate_settings() -> None:
    require_files([SETTINGS_CS, MOD_CS, PATCHES_CS, BREACH_CS, SHIELD_CS, TELEPORT_CS])

    settings = _read(SETTINGS_CS)
    for field in REQUIRED_FIELDS:
        if field not in settings:
            fail(f"MMTSettings 缺少字段 {field}")
        if f'"{field}"' not in settings:
            fail(f"MMTSettings.ExposeData 未持久化字段 {field}")
    if "wonderOverloadEnabled = false" not in settings:
        fail("奇观超载事故必须默认关闭（硬核向）")
    if "ResetToDefaults" not in settings:
        fail("MMTSettings 缺少恢复默认")

    mod = _read(MOD_CS)
    for needle in ("GetSettings<MMTSettings>", "DoSettingsWindowContents", "SettingsCategory"):
        if needle not in mod:
            fail(f"Mod 入口缺少 {needle}")

    patches = _read(PATCHES_CS)
    if "MakeRecipeProducts" not in patches:
        fail("缺少产率倍率补丁（GenRecipe.MakeRecipeProducts）")
    if "get_CostApparent" not in patches:
        fail("缺少研究成本补丁（ResearchProjectDef.CostApparent）")
    # 两个补丁都只允许作用 MMT_ 前缀
    if patches.count('StartsWith("MMT_")') < 2:
        fail("产率/研究补丁必须各自过滤 MMT_ 前缀")

    breach = _read(BREACH_CS)
    if "private const float WealthThreshold" in breach:
        fail("爆墙财富门槛仍为硬编码常量，未接设置")
    if breach.count("BreachChargeEnabled") < 3:
        fail("爆墙三条补丁入口都必须有总开关短路")

    if "MMTSettings.AdjustEnhanceRate" not in _read(SHIELD_CS):
        fail("护盾强化成功率未接 enhanceSuccessFactor")
    if "MMTSettings.TeleportBonusCharges" not in _read(TELEPORT_CS):
        fail("传送次数上限未接 teleportBonusCharges")

    print("settings: OK")


SUITES = {"settings": validate_settings}
