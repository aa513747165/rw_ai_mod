#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""贸易商队订单静态验证（早期贸易功能）。

校验点：
- MMT_TradeDepot 世界物件 XML 结构齐全（worldObjectClass/图标路径）；
- TradeOrder.cs 关键实现接线齐全（状态机/抉择信/抵达结算/商队钩子/Debug 入口）；
- 设置项 tradeOrdersEnabled 在字段/便捷读取/重置/存档/设置面板五处接线齐全。
"""
from __future__ import annotations

import re

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

DEPOT_XML = "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_TradeOrder.xml"
TRADE_CS = "Source/MyModeTest/TradeOrder.cs"
SETTINGS_CS = "Source/MyModeTest/MMTSettings.cs"
MOD_CS = "Source/MyModeTest/MyModeTestMod.cs"

# C# 侧必须存在的关键类型/成员（订单生命周期全链路）
CS_REQUIRED = (
    "class GameComponent_MMTTradeOrders",
    "class TradeOrderData",
    "class TradeOrderGen",
    "class ChoiceLetter_TradeOrder",
    "class WorldObject_TradeDepot",
    "class CaravanArrivalAction_TradeDepot",
    "Patch_LordMaker_MakeNewLord",
    "LordJob_TradeWithColony",
    "TryOfferOrder",
    "AcceptOrder",
    "DeclineOrder",
    "SpawnDepot",
    "NotifyDepotArrived",
    "DeliverFromColony",
    "CountColonyGoods",
    "TakeColonyGoods",
    "Patch_Pawn_GetGizmos_TradeOrder",
    "enum TradeOrderState",
    "RewardFactorMin = 1.5f",
    "RewardFactorMax = 2.5f",
    "WealthValueFactor",
    "WealthTotal",
    "MMT_TradeOrderDefOf",
)


def _read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def validate_trade_order() -> None:
    require_files([DEPOT_XML, TRADE_CS, SETTINGS_CS, MOD_CS])

    # ---- 世界物件 XML ----
    defs = def_map(parse_xml(DEPOT_XML))
    depot = defs.get("MMT_TradeDepot")
    if depot is None:
        fail("缺少贸易据点 WorldObjectDef MMT_TradeDepot")
    assert_equal(child_text(depot, "worldObjectClass"), "MyModeTest.WorldObject_TradeDepot",
                 "MMT_TradeDepot worldObjectClass")
    texture = child_text(depot, "texture")
    if not texture:
        fail("MMT_TradeDepot 缺少 texture（世界地图图标）")
    # 非原版路径（MMT_ 开头文件名）必须是本包贴图
    if texture.split("/")[-1].startswith("MMT_"):
        from validate_mod import assert_png  # noqa: PLC0415

        assert_png("1.7/Textures/" + texture + ".png")
    if not (child_text(depot, "label") or "").strip():
        fail("MMT_TradeDepot 缺少中文 label")

    # ---- C# 接线 ----
    cs = _read(TRADE_CS)
    for needle in CS_REQUIRED:
        if needle not in cs:
            fail(f"TradeOrder.cs 缺少关键实现: {needle}")
    # 报酬倍率区间与设计一致（150%-250%）
    m = re.search(r"RewardFactorMin = ([\d.]+)f", cs)
    n = re.search(r"RewardFactorMax = ([\d.]+)f", cs)
    if not m or not n or not (1.5 <= float(m.group(1)) < float(n.group(1)) <= 2.5):
        fail("报酬倍率区间必须落在 150%-250% 内")

    # ---- 设置项五处接线 ----
    settings = _read(SETTINGS_CS)
    for needle in ("bool tradeOrdersEnabled = true", "TradeOrdersEnabled",
                   "tradeOrdersEnabled = true;",
                   'Scribe_Values.Look(ref tradeOrdersEnabled, "tradeOrdersEnabled", true)'):
        if needle not in settings:
            fail(f"MMTSettings.cs 缺少 tradeOrdersEnabled 接线: {needle}")
    mod = _read(MOD_CS)
    if "ref s.tradeOrdersEnabled" not in mod:
        fail("MyModeTestMod.cs 设置面板缺少 tradeOrdersEnabled 开关")


SUITES = {"trade_order": validate_trade_order}
