#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""哨戒待机链静态验证：MMT_Sentinel 机械体工作模式 + ThinkTree 注入 + 唤醒挂钩（新链，由 tools/validate_new.py 自动发现）。"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    assert_png,
    child_text,
    def_map,
    fail,
    parse_xml,
    require_files,
)

WORKMODE_XML = "1.7/Defs/Misc/MMT_MechWorkMode_Sentinel.xml"
PATCH_XML = "1.7/Patches/MMT_Patch_MechStandby.xml"
CS_FILE = "Source/MyModeTest/MechStandby.cs"
TRACKER_CS = "Source/MyModeTest/EnemyDetectorSwitch.cs"
ICON = "1.7/Textures/UI/Icons/MMT_WorkModeSentinel.png"


def validate_mechstandby() -> None:
    require_files([WORKMODE_XML, PATCH_XML, CS_FILE, TRACKER_CS, ICON])
    assert_png(ICON)

    # ---- 工作模式 def：MMT_Sentinel ----
    modes = def_map(parse_xml(WORKMODE_XML))
    if "MMT_Sentinel" not in modes:
        fail("缺少 MechWorkModeDef MMT_Sentinel")
    mode = modes["MMT_Sentinel"]
    assert_equal(child_text(mode, "label"), "哨戒待机", "label")
    assert_equal(child_text(mode, "iconPath"), "UI/Icons/MMT_WorkModeSentinel", "iconPath")
    if not child_text(mode, "description"):
        fail("MMT_Sentinel 缺少 description")

    # ---- ThinkTree patch：注入 Mechanoid 树的 MMT_Sentinel 分支 ----
    patch_root = parse_xml(PATCH_XML)
    op = patch_root.find("Operation")
    if op is None or op.get("Class") != "PatchOperationAdd":
        fail("patch 必须是单个 PatchOperationAdd")
    xpath = child_text(op, "xpath")
    if 'ThinkTreeDef[defName="Mechanoid"]' not in xpath or "ConditionalNotFormingCaravan" not in xpath:
        fail("patch xpath 必须指向 Mechanoid 树的 ConditionalNotFormingCaravan 节点")
    value = op.find("value")
    if value is None:
        fail("patch 缺少 value 节点")
    branch = value.find("li")
    if branch is None or branch.get("Class") != "ThinkNode_ConditionalWorkMode":
        fail("注入分支必须是 ThinkNode_ConditionalWorkMode")
    if branch.get("MayRequire") != "Ludeon.RimWorld.Biotech":
        fail("注入分支必须带 MayRequire=Biotech（无 DLC 时不生效）")
    assert_equal(child_text(branch, "workMode"), "MMT_Sentinel", "workMode")
    sub_classes = [li.get("Class") for li in branch.find("subNodes").findall("li")]
    for need in ("JobGiver_SeekAllowedArea", "JobGiver_GetEnergy_Charger",
                 "JobGiver_Work", "MyModeTest.JobGiver_MechStandby"):
        if need not in sub_classes:
            fail(f"分支缺少节点 {need}")
    # 战斗机械体接敌节点必须在待机节点之前（ThinkNode_Priority 按顺序取第一个出作业的节点）
    fight_idx = next((i for i, li in enumerate(branch.find("subNodes").findall("li"))
                      if li.get("Class") == "ThinkNode_ConditionalWorkMech"), -1)
    if fight_idx == -1:
        fail("分支缺少战斗机械体接敌节点 ThinkNode_ConditionalWorkMech")
    if sub_classes.index("MyModeTest.JobGiver_MechStandby") < fight_idx:
        fail("JobGiver_MechStandby 必须排在接敌节点之后（睡眠是最后兜底）")
    # 不允许出现游走节点（否则违背"闲置即睡眠省性能"的设计）
    for cls in sub_classes:
        if cls and "Wander" in cls:
            fail(f"分支不允许包含游走节点 {cls}")

    # ---- C#：核心类与原版 API 复用 ----
    cs = open(CS_FILE, encoding="utf-8").read()
    for token in ("class JobGiver_MechStandby", "class MechStandbyManager",
                  "class MapComponent_MechStandby", "HarmonyPatch(typeof(Pawn), \"GetGizmos\")",
                  "JobDefOf.SelfShutdown", "AnyHostileNearby", "CompCanBeDormant",
                  "EnemyRaidTracker.HasActiveAssault"):
        if token not in cs:
            fail(f"MechStandby.cs 缺少 {token}")
    # 睡眠门必须是"实际进攻 + 近距离活动敌人"两级判定，不能退回全图敌对 Lord 判定
    # （地图生成的潜伏型敌对 Lord，如血肉兽突袭，会把全图判定永久堵死导致哨戒永不睡眠）
    if "EnemyRaidTracker.HasHostileLord" in cs:
        fail("JobGiver_MechStandby 不得用全图 HasHostileLord 当睡眠门（潜伏型敌 Lord 会永久堵死睡眠）")

    # ---- 唤醒挂钩：敌意跟踪器 Notify 必须回调唤醒管理器（且在感应开关早退之前）----
    tracker = open(TRACKER_CS, encoding="utf-8").read()
    hook = "MechStandbyManager.OnHostileStateChanged"
    if hook not in tracker:
        fail("EnemyDetectorSwitch.cs 缺少唤醒挂钩 MechStandbyManager.OnHostileStateChanged")
    notify_idx = tracker.find("private static void Notify")
    hook_idx = tracker.find(hook, notify_idx)
    early_return_idx = tracker.find("switches.TryGetValue", notify_idx)
    if not (notify_idx != -1 and early_return_idx != -1 and notify_idx < hook_idx < early_return_idx):
        fail("唤醒挂钩必须位于 Notify 内、感应开关 TryGetValue 早退判断之前（无开关的图也要能唤醒）")
    # "实际进攻"跟踪：排除潜伏型 Lord，且新进攻出现（IncrementAssault）即唤醒
    for token in ("IsLurkerLord", "assaultCounts", "HasActiveAssault"):
        if token not in tracker:
            fail(f"EnemyDetectorSwitch.cs 缺少 {token}（实际进攻跟踪：潜伏型敌 Lord 不得堵死哨戒待机）")
    inc_idx = tracker.find("private static void IncrementAssault")
    if inc_idx == -1 or hook not in tracker[inc_idx:tracker.find("private static void DecrementAssault")]:
        fail("IncrementAssault 必须回调唤醒管理器（新进攻出现即唤醒，不能只在计数 0->1 翻转时唤醒）")
    # 防计数泄漏：Lord 全灭销毁后 Notify_PawnLost 后缀仍会带着已销毁 Lord 进 SyncLord，
    # 此时成员已清空、派系兜底判定会把它当敌对重新计入且再无事件修正（感应开关恒通电、哨戒永不睡眠）。
    # SyncLord 必须用 lordManager.lords.Contains 过滤已移除的 Lord（LordMaker 先 AddLord 再 AddPawns，正常路径不受影响）。
    sync_idx = tracker.find("public static void SyncLord")
    if sync_idx == -1 or "lordManager.lords.Contains(lord)" not in tracker[sync_idx:tracker.find("public static void OnLordRemoved")]:
        fail("SyncLord 必须用 lordManager.lords.Contains(lord) 过滤已从管理器移除的 Lord（否则全灭销毁后会被重新计入，计数永久泄漏）")


SUITES = {"mechstandby": validate_mechstandby}
