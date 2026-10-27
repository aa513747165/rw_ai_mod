#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""模块系统静态验证：增效模块配置驱动体系（ModuleExtension + slotLimits）。

校验点：
- 三个增效模块物品挂 ModuleExtension，slotKey/effects/effectReport 齐全且数值与设计一致；
- 三个滤网物品挂 MeshFilterExtension 且带 slotKey=Mesh；
- 自动研磨机/自动筛子/自动采石机的 CompProperties_ModuleSlots 用 slotLimits 声明上限；
- 建筑 slotLimits 引用的 slotKey 均有对应模块物品（防 XML 写错 key）；
- C# 侧关键 token（配置驱动 API 与旧枚举清理）。
"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    parse_xml,
    read_text,
    require_files,
)

MODULES_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_ProcessorModules.xml"
MESHES_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_SieveMeshes.xml"
AUTOPROC_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AutoProcessor.xml"
DRILL_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ChunkDrill.xml"

MODULE_DEFS = {
    "MMT_OverclockModule": ("Overclock", [("SpeedFactor", "0.1")]),
    "MMT_ParallelModule": ("Parallel", [("ExtraLanes", "1")]),
    "MMT_YieldModule": ("Yield", [("YieldBonus", "10")]),
}


def _mod_extension(thing_def, class_name):
    for li in thing_def.findall("modExtensions/li"):
        if li.attrib.get("Class") == class_name:
            return li
    return None


def _slot_limits(building_def):
    """建筑 def → {slotKey: max}（CompProperties_ModuleSlots 的 slotLimits）。"""
    for li in building_def.findall("comps/li"):
        if li.attrib.get("Class") == "MyModeTest.CompProperties_ModuleSlots":
            return {child_text(e, "key"): child_text(e, "max")
                    for e in li.findall("slotLimits/li")}
    return None


def validate_modules() -> None:
    require_files([MODULES_XML, MESHES_XML, AUTOPROC_XML, DRILL_XML,
                   "Source/MyModeTest/ModuleSystem.cs",
                   "Source/MyModeTest/AutoProcessor.cs",
                   "Source/MyModeTest/ChunkDrill.cs"])

    # ---- 模块物品：ModuleExtension 配置齐全 ----
    items = def_map(parse_xml(MODULES_XML))
    assert_equal(set(items), set(MODULE_DEFS), "增效模块物品 defName 集合")
    for name, (slot_key, effects) in MODULE_DEFS.items():
        ext = _mod_extension(items[name], "MyModeTest.ModuleExtension")
        if ext is None:
            fail(f"{name} 缺 ModuleExtension")
        assert_equal(child_text(ext, "slotKey"), slot_key, f"{name} slotKey")
        got = [(child_text(e, "key"), child_text(e, "valuePerItem"))
               for e in ext.findall("effects/li")]
        assert_equal(got, effects, f"{name} 效果列表")
        if not (ext.findtext("effectReport") or "").strip():
            fail(f"{name} 缺 effectReport 效果说明")

    # ---- 滤网物品：MeshFilterExtension 带 slotKey=Mesh ----
    meshes = def_map(parse_xml(MESHES_XML))
    for name in ("MMT_MeshWood", "MMT_MeshIron", "MMT_MeshFine"):
        ext = _mod_extension(meshes[name], "MyModeTest.MeshFilterExtension")
        if ext is None:
            fail(f"{name} 缺 MeshFilterExtension")
        assert_equal(child_text(ext, "slotKey"), "Mesh", f"{name} slotKey")

    # ---- 建筑槽位上限：slotLimits 写法，引用的 key 均有对应模块物品 ----
    known_keys = {slot for slot, _ in MODULE_DEFS.values()}
    proc = def_map(parse_xml(AUTOPROC_XML))
    all_limits = []
    for name in ("MMT_AutoGrinder", "MMT_AutoSieve"):
        if name not in proc:
            fail(f"{AUTOPROC_XML} 缺建筑 {name}")
        limits = _slot_limits(proc[name])
        if limits is None:
            fail(f"{name} 缺 CompProperties_ModuleSlots")
        assert_equal(limits, {"Overclock": "5", "Parallel": "3"}, f"{name} 槽位上限")
        all_limits.append(limits)
    drill = def_map(parse_xml(DRILL_XML))
    drill_limits = _slot_limits(drill["MMT_AutoChunkDrill"])
    assert_equal(drill_limits, {"Overclock": "5", "Yield": "5"}, "自动采石机槽位上限")
    all_limits.append(drill_limits)
    for limits in all_limits:
        for key in limits:
            if key not in known_keys:
                fail(f"建筑 slotLimits 引用了未知 slotKey: {key}")

    # ---- C# 关键实现 ----
    module_cs = read_text("Source/MyModeTest/ModuleSystem.cs")
    for token in ("InstallableSlotItemExtension", "ModuleExtension", "ModuleEffect",
                  "ModuleEffectKeys", "ModuleSlotLimit", "InstalledSlotItem",
                  "SpeedFactor", "ExtraLanes", "YieldBonus"):
        if token not in module_cs:
            fail(f"ModuleSystem.cs 缺少: {token}")
    auto_cs = read_text("Source/MyModeTest/AutoProcessor.cs")
    for token in ("GetEffectSum", "AllModuleDefs", "IsModule", "MigrateLegacyCounts",
                  '"overclockInstalled"', '"parallelInstalled"', '"yieldInstalled"'):
        if token not in auto_cs:
            fail(f"AutoProcessor.cs 缺少: {token}")
    for token in ("enum ModuleType", "TypeOf", "OverclockCount", "ParallelCount", "YieldCount"):
        if token in auto_cs:
            fail(f"AutoProcessor.cs 仍残留旧模块体系: {token}")
    drill_cs = read_text("Source/MyModeTest/ChunkDrill.cs")
    for token in ("GetEffectSum", "ModuleEffectKeys.SpeedFactor", "ModuleEffectKeys.YieldBonus"):
        if token not in drill_cs:
            fail(f"ChunkDrill.cs 缺少: {token}")


SUITES = {"modules": validate_modules}
