#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""天赋树系统静态验证（mmt-class-design.md v2）。

校验点：
- 生成物齐全：通用树/家族树 XML、节点被动 Hediff XML、SitePartDef、Isekai 授权贴图；
- 图结构：每树唯一 Start(cost 0)、连线引用有效、全图从 Start 连通、cost/阶位规则；
- 通用树：10 个 ClassUnlock 与家族树一一对应、19 个旧 perk 全部节点化；
- 家族树：memberClasses 与 rom 映射 family 分组一致、37 职业技能全覆盖且 requireTier=stage；
- 引用闭环：节点 hediff/abilities/icon 全部存在；被动 stat 对照游戏 StatDef 白名单；
- C# 接线：TalentTreeDef/TalentTreeWindow/GuildQuest 关键符号与升阶门槛 5/20/35。
"""
from __future__ import annotations

import os
import re
import xml.etree.ElementTree as ET

from validate_mod import ROOT, fail, require_files

GAME_STATS = r"D:/RimWorld v1.7.4979/Data/Core/Defs/Stats"
GAME_ROYALTY = r"D:/RimWorld v1.7.4979/Data/Royalty/Defs"
GENERAL_XML = os.path.join(ROOT, "1.7/Defs/TalentDefs/MMT_Talents_General.xml")
FAMILIES_XML = os.path.join(ROOT, "1.7/Defs/TalentDefs/MMT_Talents_Families.xml")
HEDIFFS_XML = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_Talents.xml")
PERKS_XML = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_PawnProgress.xml")
SITE_XML = os.path.join(ROOT, "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_GuildQuest.xml")
ABILITY_DIR = os.path.join(ROOT, "1.7/Defs/AbilityDefs")
TEX_DIR = os.path.join(ROOT, "1.7/Textures")
ROM_JSON = os.path.join(ROOT, "tools/data/mmt_rom_class_mapping.json")
PROGRESS_CS = os.path.join(ROOT, "Source/MyModeTest/PawnProgress.cs")
CLASS_CS = os.path.join(ROOT, "Source/MyModeTest/PawnClass.cs")
TREE_CS = os.path.join(ROOT, "Source/MyModeTest/TalentTreeDef.cs")
WINDOW_CS = os.path.join(ROOT, "Source/MyModeTest/TalentTreeWindow.cs")
QUEST_CS = os.path.join(ROOT, "Source/MyModeTest/GuildQuest.cs")

TREE_TAG = "MyModeTest.MMT_TalentTreeDef"

# 与生成器 STAT_WHITELIST 同步
STAT_WHITELIST = {
    "MeleeHitChance", "MeleeDodgeChance", "ShootingAccuracyPawn", "AimingDelayFactor",
    "MoveSpeed", "ArmorRating_Sharp", "ArmorRating_Blunt", "ArmorRating_Heat",
    "PainShockThreshold", "MentalBreakThreshold", "ToxicResistance",
    "PsychicSensitivity", "PsychicEntropyMax", "PsychicEntropyRecoveryRate", "MeditationFocusGain",
    "WorkSpeedGlobal", "GeneralLaborSpeed", "PlantWorkSpeed", "ResearchSpeed", "GlobalLearningFactor",
    "ImmunityGainSpeed", "InjuryHealingFactor", "MedicalTendQuality",
    "SocialImpact", "NegotiationAbility", "TradePriceImprovement",
    "TameAnimalChance", "TrainAnimalChance", "ArrestSuccessChance",
}

PERK_DEFS = [
    "MMT_Perk_Veteran", "MMT_Perk_Brawler", "MMT_Perk_Tough", "MMT_Perk_Fleet",
    "MMT_Perk_FastHeal", "MMT_Perk_BattleSense", "MMT_Perk_Deft", "MMT_Perk_Miner",
    "MMT_Perk_Farmer", "MMT_Perk_Builder", "MMT_Perk_Scholar", "MMT_Perk_Chef",
    "MMT_Perk_IronBody", "MMT_Perk_EfficientGut", "MMT_Perk_DeepSleep", "MMT_Perk_MindWall",
    "MMT_Perk_GritMind", "MMT_Perk_PackMule", "MMT_Perk_Masterwork",
]


def _read(path: str) -> str:
    with open(path, encoding="utf-8") as f:
        return f.read()


def _game_stats() -> set[str]:
    stats: set[str] = set()
    for d in (GAME_STATS, os.path.join(GAME_ROYALTY, "Stats")):
        if not os.path.isdir(d):
            continue
        for fn in os.listdir(d):
            if fn.endswith(".xml"):
                for m in re.findall(r"<defName>([^<]+)</defName>", _read(os.path.join(d, fn))):
                    stats.add(m)
    return stats


def _parse_tree(path: str):
    """返回 [(defName, kind, familyKey, memberClasses, nodes, connections)]；nodes 为 dict。"""
    root = ET.parse(path).getroot()
    trees = []
    for el in root.findall(TREE_TAG):
        nodes = {}
        for li in el.find("nodes") or []:
            nid = li.findtext("nodeId")
            nodes[nid] = {
                "nodeType": li.findtext("nodeType") or "Minor",
                "cost": int(li.findtext("cost") or 1),
                "hediff": li.findtext("hediff"),
                "icon": li.findtext("icon"),
                "classBaseName": li.findtext("classBaseName"),
                "familyKey": li.findtext("familyKey"),
                "requireTier": int(li.findtext("requireTier") or 0),
                "abilities": [a.text for a in li.find("abilities") or []],
            }
        conns = [c.text for c in el.find("connections") or []]
        members = [c.text for c in el.find("memberClasses") or []]
        trees.append({
            "defName": el.findtext("defName"),
            "treeKind": el.findtext("treeKind"),
            "familyKey": el.findtext("familyKey"),
            "memberClasses": members,
            "nodes": nodes,
            "connections": conns,
        })
    return trees


def _check_tree_graph(tree):
    name = tree["defName"]
    nodes = tree["nodes"]
    starts = [n for n, nd in nodes.items() if nd["nodeType"] == "Start"]
    if len(starts) != 1:
        fail(f"{name}: Start 节点数 {len(starts)} != 1")
    for nid in starts:
        if nodes[nid]["cost"] != 0:
            fail(f"{name}: Start 节点 {nid} cost 应为 0")
    adj = {n: set() for n in nodes}
    for conn in tree["connections"]:
        a, _, b = (conn or "").partition("~")
        if a not in nodes or b not in nodes:
            fail(f"{name}: 连线引用未知节点 {conn}")
            continue
        adj[a].add(b)
        adj[b].add(a)
    # 连通性（从 Start BFS）
    if starts:
        seen = set()
        stack = list(starts)
        while stack:
            cur = stack.pop()
            if cur in seen:
                continue
            seen.add(cur)
            stack.extend(adj[cur])
        orphan = set(nodes) - seen
        if orphan:
            fail(f"{name}: {len(orphan)} 个节点与 Start 不连通（如 {sorted(orphan)[:3]}）")
    for nid, nd in nodes.items():
        if nd["cost"] < 0:
            fail(f"{name}.{nid}: cost 为负")
        if nd["nodeType"] in ("Keystone", "ClassUnlock") and nd["cost"] < 2:
            fail(f"{name}.{nid}: {nd['nodeType']} cost 应 >= 2")
        if not 0 <= nd["requireTier"] <= 3:
            fail(f"{name}.{nid}: requireTier 非法 {nd['requireTier']}")


def validate_talent_tree():
    require_files([GENERAL_XML, FAMILIES_XML, HEDIFFS_XML, SITE_XML, ROM_JSON,
                   TREE_CS, WINDOW_CS, QUEST_CS, PROGRESS_CS, CLASS_CS])
    # 贴图
    for name in ("NodeMinor", "NodeNotable", "NodeKeystone", "NodeStart",
                 "NodeMinorActive", "NodeNotableActive", "NodeKeystoneActive", "NodeStartActive"):
        if not os.path.isfile(os.path.join(TEX_DIR, "UI/TalentTree", name + ".png")):
            fail(f"天赋树节点形状贴图缺失: UI/TalentTree/{name}.png")
    icons_dir = os.path.join(TEX_DIR, "UI/TalentTree/Icons")
    if not os.path.isdir(icons_dir) or len([f for f in os.listdir(icons_dir) if f.endswith(".png")]) < 20:
        fail("天赋树通用图标目录缺失或数量不足（UI/TalentTree/Icons）")
    for name in ("Isekai_Expedition", "Isekai_Hunt", "Isekai_Raid"):
        if not os.path.isfile(os.path.join(TEX_DIR, "UI/TalentTree/World", name + ".png")):
            fail(f"公会站点世界图标缺失: UI/TalentTree/World/{name}.png")

    general = _parse_tree(GENERAL_XML)
    families = _parse_tree(FAMILIES_XML)
    if len(general) != 1 or general[0]["treeKind"] != "General":
        fail("通用树文件应含且仅含一棵 General 树")
        return
    gtree = general[0]
    _check_tree_graph(gtree)
    fam_by_key = {}
    for t in families:
        _check_tree_graph(t)
        if t["treeKind"] != "Family" or not t["familyKey"]:
            fail(f"{t['defName']}: 家族树 treeKind/familyKey 缺失")
        fam_by_key[t["familyKey"]] = t

    import json
    rom = json.load(open(ROM_JSON, encoding="utf-8"))
    rom_classes = rom["classes"]
    rom_abilities = rom["abilities"]

    # 通用树 ClassUnlock ↔ 家族树
    unlocks = {nd["familyKey"] for nd in gtree["nodes"].values() if nd["nodeType"] == "ClassUnlock"}
    if None in unlocks:
        fail("通用树存在缺 familyKey 的 ClassUnlock 节点")
        unlocks.discard(None)
    if unlocks != set(fam_by_key):
        fail(f"ClassUnlock 家族 {sorted(unlocks)} 与家族树 {sorted(fam_by_key)} 不一致")
    # 19 perk 节点化
    perk_nodes = {nd["hediff"] for nd in gtree["nodes"].values() if nd["hediff"]}
    for perk in PERK_DEFS:
        if perk not in perk_nodes:
            fail(f"旧 perk {perk} 未在通用树节点化")

    # 家族树 ↔ rom 映射
    fam_members = {}
    for cls, info in rom_classes.items():
        fam_members.setdefault(info.get("family"), []).append(cls)
    if set(fam_members) != set(fam_by_key):
        fail(f"rom 映射家族分组 {sorted(fam_members)} 与家族树 {sorted(fam_by_key)} 不一致")
    for key, t in fam_by_key.items():
        if sorted(t["memberClasses"]) != sorted(fam_members.get(key, [])):
            fail(f"{t['defName']}: memberClasses 与 rom 映射 family={key} 分组不一致")
    # 技能全覆盖 + requireTier=stage + classBaseName 合法
    covered = {}
    for t in families:
        for nid, nd in t["nodes"].items():
            if nd["nodeType"] in ("Skill", "SkillUpgrade"):
                cb = nd["classBaseName"]
                if cb not in t["memberClasses"]:
                    fail(f"{t['defName']}.{nid}: classBaseName {cb} 不在本族成员中")
            for ab in nd["abilities"]:
                if ab in covered:
                    fail(f"技能 {ab} 被多个节点引用（{covered[ab]} 与 {t['defName']}.{nid}）")
                covered[ab] = f"{t['defName']}.{nid}"
                stage = rom_abilities.get(ab, {}).get("stage")
                if stage is None:
                    fail(f"技能 {ab} 不在 rom 映射 abilities 中")
                elif nd["requireTier"] != stage:
                    fail(f"{t['defName']}.{nid}: requireTier {nd['requireTier']} != 技能 stage {stage}")
    for cls, info in rom_classes.items():
        for ab in info.get("abilities", []):
            if ab not in covered:
                fail(f"职业 {cls} 技能 {ab} 未被家族树覆盖")
    # 技能 def 存在
    ability_defs = set()
    for fn in os.listdir(ABILITY_DIR):
        if fn.startswith("MMT_Abilities_RoM_") and fn.endswith(".xml"):
            ability_defs.update(re.findall(r"<defName>([^<]+)</defName>", _read(os.path.join(ABILITY_DIR, fn))))
    for ab in covered:
        if ab not in ability_defs:
            fail(f"技能 {ab} 无 AbilityDef（{covered[ab]}）")

    # 节点 hediff 引用闭环
    gen_hediffs = set(re.findall(r"<defName>(MMT_Talent_[^<]+)</defName>", _read(HEDIFFS_XML)))
    perk_hediffs = set(re.findall(r"<defName>(MMT_Perk_[^<]+)</defName>", _read(PERKS_XML)))
    for t in general + families:
        for nid, nd in t["nodes"].items():
            h = nd["hediff"]
            if h and h not in gen_hediffs and h not in perk_hediffs:
                fail(f"{t['defName']}.{nid}: hediff {h} 未定义")
            icon = nd["icon"]
            if icon and not os.path.isfile(os.path.join(TEX_DIR, icon + ".png")):
                fail(f"{t['defName']}.{nid}: 图标缺失 {icon}.png")

    # 被动 stat 白名单 + 游戏 StatDef
    game_stats = _game_stats()
    hediffs_text = _read(HEDIFFS_XML)
    for stat in STAT_WHITELIST:
        if stat not in game_stats:
            fail(f"白名单 stat {stat} 在游戏 StatDef 中不存在（拼写？）")
    for m in re.finditer(r"<(statOffsets|statFactors)>((?:\s*<[A-Za-z]+>[^<]*</[A-Za-z]+>)+)\s*</\1>", hediffs_text):
        for stat in re.findall(r"<([A-Za-z]+)>", m.group(2)):
            if stat not in game_stats:
                fail(f"天赋被动 Hediff 引用未知 stat: {stat}")

    # C# 接线
    tree_cs = _read(TREE_CS)
    for token in ("class MMT_TalentTreeDef", "class TalentNodeRecord", "CanUnlock", "FindUnlockChain",
                  "TryUnlockWithChain", "OnClassAttuned", "public static void Migrate"):
        if token not in tree_cs:
            fail(f"TalentTreeDef.cs 缺少: {token}")
    window_cs = _read(WINDOW_CS)
    for token in ("class Window_TalentTree", "ClipSegment", "HandlePanZoom"):
        if token not in window_cs:
            fail(f"TalentTreeWindow.cs 缺少: {token}")
    quest_cs = _read(QUEST_CS)
    for token in ("class QuestPart_MMTHunt", "class GameComponent_GuildQuests",
                  "class SitePartWorker_MMT_GuildHunt", "CheckCompletion"):
        if token not in quest_cs:
            fail(f"GuildQuest.cs 缺少: {token}")
    class_cs = _read(CLASS_CS)
    if "tier == 1 ? 5 : tier == 2 ? 20 : 35" not in class_cs:
        fail("PawnClass.LevelRequirement 未按 v2 重排为 5/20/35")
    if "TalentTreeUtil.OnClassAttuned" not in class_cs:
        fail("导师台 CompleteAttune 未挂 OnClassAttuned 联动")
    progress_cs = _read(PROGRESS_CS)
    if "public const int MaxLevel = 60;" not in progress_cs:
        fail("CompPawnProgress.MaxLevel 未提升到 60")
    for field in ("talentPoints", "unlockedTalents", "unlockedFamilies", "perkMigrated"):
        if field not in progress_cs:
            fail(f"CompPawnProgress 缺少字段 {field}")

    total = sum(len(t["nodes"]) for t in general + families)
    print(f"talent_tree: OK（通用 {len(gtree['nodes'])} + 家族 {total - len(gtree['nodes'])} = {total} 节点，"
          f"{len(gen_hediffs)} 被动 Hediff，{len(covered)} 技能节点）")


SUITES = {"talent_tree": validate_talent_tree}
