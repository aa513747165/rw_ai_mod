#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""MMT 天赋树生成器（mmt-class-design.md v2）。

输入：
  - tools/data/mmt_talent_tree_mapping.json（唯一人工维护点：通用树布局 + 家族主题配置）
  - tools/data/mmt_rom_class_mapping.json（职业 family 分组 + 技能清单/stage）
  - 1.7/Defs/AbilityDefs/MMT_Abilities_RoM_*.xml（技能图标 iconPath，rom 生成器产出）
  - 1.7/Defs/HediffDefs/MMT_Hediffs_PawnProgress.xml（19 个旧 perk 的 label/description）

产出（全部生成物，勿手改）：
  - 1.7/Defs/TalentDefs/MMT_Talents_General.xml（通用树）
  - 1.7/Defs/TalentDefs/MMT_Talents_Families.xml（10 棵职业家族树）
  - 1.7/Defs/HediffDefs/MMT_Hediffs_Talents.xml（全部节点被动 Hediff）
  - 1.7/Textures/UI/TalentTree/（Isekai 授权贴图：节点形状/图标/UI 底图/世界图标）

用法：
  python tools/build_mmt_talent_trees.py          # 落盘
  python tools/build_mmt_talent_trees.py --check  # 只校验不落盘
"""
import json
import math
import os
import shutil
import sys
import xml.etree.ElementTree as ET
from collections import Counter, OrderedDict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TALENT_JSON = os.path.join(ROOT, "tools/data/mmt_talent_tree_mapping.json")
ROM_JSON = os.path.join(ROOT, "tools/data/mmt_rom_class_mapping.json")
PERK_XML = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_PawnProgress.xml")
ABILITY_DIR = os.path.join(ROOT, "1.7/Defs/AbilityDefs")
OUT_GENERAL = os.path.join(ROOT, "1.7/Defs/TalentDefs/MMT_Talents_General.xml")
OUT_FAMILIES = os.path.join(ROOT, "1.7/Defs/TalentDefs/MMT_Talents_Families.xml")
OUT_HEDIFFS = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_Talents.xml")
OUT_TEX_DIR = os.path.join(ROOT, "1.7/Textures/UI/TalentTree")
ISEKAI = r"D:\steamcmd\steamapps\workshop\content\294100\3657580708"

HEADER = "<!-- 生成物：tools/build_mmt_talent_trees.py 由 tools/data/mmt_talent_tree_mapping.json 生成，请勿手改 -->"

# 允许使用的原版 StatDef（防手误；新 stat 先确认 defName 再加）
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

errors = []


def err(msg):
    errors.append(msg)
    print("错误: " + msg, file=sys.stderr)


def esc(s):
    return (str(s).replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;"))


def x(tag, value, indent):
    return " " * indent + "<%s>%s</%s>" % (tag, esc(value), tag)


def load_json(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f, object_pairs_hook=OrderedDict)


def parse_perk_hediffs():
    """旧 perk defName → (label, description)。"""
    result = {}
    if not os.path.isfile(PERK_XML):
        return result
    tree = ET.parse(PERK_XML)
    for el in tree.getroot().findall("HediffDef"):
        name = el.findtext("defName")
        if name and name.startswith("MMT_Perk_"):
            result[name] = (el.findtext("label") or name, el.findtext("description") or "")
    return result


def parse_ability_icons():
    """技能 defName → iconPath（读 rom 生成器产出的技能 XML）。"""
    icons = {}
    if not os.path.isdir(ABILITY_DIR):
        return icons
    for fn in os.listdir(ABILITY_DIR):
        if not (fn.startswith("MMT_Abilities_RoM_") and fn.endswith(".xml")):
            continue
        tree = ET.parse(os.path.join(ABILITY_DIR, fn))
        for el in tree.getroot().findall("AbilityDef"):
            name = el.findtext("defName")
            icon = el.findtext("iconPath")
            if name and icon:
                icons[name] = icon
    return icons


def check_stats(effects, ctx):
    for section in ("statOffsets", "statFactors"):
        for stat in (effects.get(section) or {}):
            if stat not in STAT_WHITELIST:
                err("%s: 未知 stat '%s'（不在白名单）" % (ctx, stat))


def fmt_num(v):
    if isinstance(v, float) and v == int(v):
        return str(int(v))
    return ("%.4f" % v).rstrip("0").rstrip(".")


def emit_hediff(def_name, label, description, effects, lines):
    lines.append("  <HediffDef>")
    lines.append(x("defName", def_name, 4))
    lines.append(x("label", label, 4))
    if description:
        lines.append(x("description", description, 4))
    lines.append("    <isBad>false</isBad>")
    lines.append("    <everCurableByItem>false</everCurableByItem>")
    lines.append("    <stages>")
    lines.append("      <li>")
    lines.append("        <becomeVisible>false</becomeVisible>")
    offsets = effects.get("statOffsets") or {}
    factors = effects.get("statFactors") or {}
    if offsets:
        lines.append("        <statOffsets>")
        for stat, val in offsets.items():
            lines.append("          <%s>%s</%s>" % (stat, fmt_num(val), stat))
        lines.append("        </statOffsets>")
    if factors:
        lines.append("        <statFactors>")
        for stat, val in factors.items():
            lines.append("          <%s>%s</%s>" % (stat, fmt_num(val), stat))
        lines.append("        </statFactors>")
    lines.append("      </li>")
    lines.append("    </stages>")
    lines.append("  </HediffDef>")


def emit_node(node, lines):
    """node: dict with keys nodeId/label/description/nodeType/icon/x/y/cost/hediff/abilities/classBaseName/familyKey/requireTier"""
    lines.append("      <li>")
    lines.append(x("nodeId", node["nodeId"], 8))
    lines.append(x("label", node["label"], 8))
    if node.get("description"):
        lines.append(x("description", node["description"], 8))
    lines.append(x("nodeType", node["nodeType"], 8))
    if node.get("icon"):
        lines.append(x("icon", node["icon"], 8))
    lines.append(x("x", fmt_num(node["x"]), 8))
    lines.append(x("y", fmt_num(node["y"]), 8))
    lines.append(x("cost", node.get("cost", 1), 8))
    if node.get("hediff"):
        lines.append(x("hediff", node["hediff"], 8))
    if node.get("abilities"):
        lines.append("        <abilities>")
        for ab in node["abilities"]:
            lines.append(x("li", ab, 10))
        lines.append("        </abilities>")
    if node.get("classBaseName"):
        lines.append(x("classBaseName", node["classBaseName"], 8))
    if node.get("familyKey"):
        lines.append(x("familyKey", node["familyKey"], 8))
    if node.get("requireTier"):
        lines.append(x("requireTier", node["requireTier"], 8))
    lines.append("      </li>")


def emit_tree(tree_def, lines):
    lines.append("  <MyModeTest.MMT_TalentTreeDef>")
    lines.append(x("defName", tree_def["defName"], 4))
    lines.append(x("label", tree_def["label"], 4))
    if tree_def.get("description"):
        lines.append(x("description", tree_def["description"], 4))
    lines.append(x("treeKind", tree_def["treeKind"], 4))
    if tree_def.get("familyKey"):
        lines.append(x("familyKey", tree_def["familyKey"], 4))
    if tree_def.get("memberClasses"):
        lines.append("    <memberClasses>")
        for c in tree_def["memberClasses"]:
            lines.append(x("li", c, 6))
        lines.append("    </memberClasses>")
    lines.append("    <nodes>")
    for node in tree_def["nodes"]:
        emit_node(node, lines)
    lines.append("    </nodes>")
    lines.append("    <connections>")
    for conn in tree_def["connections"]:
        lines.append(x("li", conn, 6))
    lines.append("    </connections>")
    lines.append("  </MyModeTest.MMT_TalentTreeDef>")


def build_general(mapping, perks):
    """通用树：节点 hediff 字段解析（perk 引用 / effects 生成），返回 (tree_def, hediffs)。"""
    g = mapping["generalTree"]
    nodes = []
    hediffs = []  # (defName, label, description, effects)
    ids = set()
    for n in g["nodes"]:
        node_id = n["nodeId"]
        if node_id in ids:
            err("通用树节点 id 重复: " + node_id)
        ids.add(node_id)
        node = {
            "nodeId": node_id,
            "nodeType": n.get("nodeType", "Minor"),
            "icon": n.get("icon"),
            "x": float(n["x"]),
            "y": float(n["y"]),
            "cost": int(n.get("cost", 1)),
        }
        if n.get("familyKey"):
            node["familyKey"] = n["familyKey"]
        if n.get("perk"):
            if n["perk"] not in perks:
                err("%s: 引用的 perk Hediff %s 不存在" % (node_id, n["perk"]))
                node["label"] = node_id
                node["description"] = ""
            else:
                node["label"], node["description"] = perks[n["perk"]]
            node["hediff"] = n["perk"]
        elif n.get("effects"):
            check_stats(n["effects"], node_id)
            node["label"] = n.get("label", node_id)
            node["description"] = n.get("description", "")
            node["hediff"] = "MMT_Talent_" + node_id
            hediffs.append((node["hediff"], node["label"], node["description"], n["effects"]))
        else:
            # 无效果节点（ClassUnlock 等）
            node["label"] = n.get("label", node_id)
            node["description"] = n.get("description", "")
        nodes.append(node)
    # 连接校验
    for conn in g["connections"]:
        a, _, b = conn.partition("~")
        if a not in ids or b not in ids:
            err("通用树连线引用未知节点: " + conn)
    tree_def = {
        "defName": g["defName"],
        "label": g["label"],
        "description": g.get("description", ""),
        "treeKind": "General",
        "nodes": nodes,
        "connections": list(g["connections"]),
    }
    return tree_def, hediffs


def build_family_trees(mapping, rom, ability_icons):
    """家族树：生成器自动铺布局，返回 (tree_defs, hediffs)。"""
    fam_cfg = mapping["families"]
    classes = rom["classes"]
    abilities = rom["abilities"]
    fam_classes = {key: [c for c, info in classes.items() if info.get("family") == key] for key in fam_cfg}
    tree_defs = []
    hediffs = []
    for key, cfg in fam_cfg.items():
        members = fam_classes.get(key, [])
        if not members:
            err("家族 %s 没有成员职业" % key)
            continue
        nodes = []
        connections = []
        hediff_counter = Counter()

        def add_hediff_node(node_id, label, description, effects, node_type, px, py, cost, icon, extra=None):
            check_stats(effects, node_id)
            hediff_counter[node_id] += 1
            node = {
                "nodeId": node_id, "label": label, "description": description,
                "nodeType": node_type, "icon": icon, "x": round(px, 3), "y": round(py, 3),
                "cost": cost, "hediff": "MMT_Talent_" + node_id,
            }
            if extra:
                node.update(extra)
            nodes.append(node)
            hediffs.append(("MMT_Talent_" + node_id, label, description, effects))
            return node

        prefix = "F_%s" % key
        # Start
        nodes.append({
            "nodeId": prefix + "_Start", "label": cfg["label"] + "·入门",
            "description": "踏入职阶之道。", "nodeType": "Start",
            "icon": cfg["trunk"][0]["icon"], "x": 0, "y": 0, "cost": 0,
        })
        # 主干
        prev = prefix + "_Start"
        for i, t in enumerate(cfg["trunk"]):
            tid = "%s_T%d" % (prefix, i + 1)
            add_hediff_node(tid, t["label"], t["description"], t["effects"], "Minor", 0, 1.6 * (i + 1), 1, t["icon"])
            connections.append("%s~%s" % (prev, tid))
            prev = tid
        ks = cfg["keystone"]
        add_hediff_node(prefix + "_Key", ks["label"], ks["description"], ks["effects"], "Keystone", 0, 9.6, 3, ks["icon"])
        connections.append("%s~%s_Key" % (prev, prefix))

        # 职业簇：下半圆扇形排布
        k = len(members)
        for ci, cls in enumerate(members):
            angle_deg = 270.0 if k == 1 else 190.0 + 160.0 * ci / (k - 1)
            theta = math.radians(angle_deg)
            ux, uy = math.cos(theta), math.sin(theta)
            cls_info = classes[cls]
            cls_label = cls_info.get("label", cls)
            c0 = "%s_%s_0" % (prefix, cls)
            # 簇起点：职业印记（小被动）
            add_hediff_node(c0, cls_label + "印记", "就职「%s」的职阶印记。" % cls_label,
                            {"statFactors": {"PsychicSensitivity": 1.02}}, "Notable",
                            2.6 * ux, 2.6 * uy, 1, cfg["trunk"][0]["icon"],
                            extra={"classBaseName": cls})
            connections.append("%s~%s" % (prefix + "_Start", c0))
            # 技能链（按映射表 abilities 顺序，requireTier=stage）
            prev = c0
            aj = 0
            for ab_def in cls_info.get("abilities", []):
                ab = abilities.get(ab_def)
                if ab is None:
                    err("%s: 技能 %s 不在 rom 映射表 abilities 中" % (cls, ab_def))
                    continue
                stage = int(ab.get("stage", 1))
                nid = "%s_%s_A%d" % (prefix, cls, aj)
                nodes.append({
                    "nodeId": nid, "label": ab.get("label", ab_def),
                    "description": ab.get("description", ""),
                    "nodeType": "Skill", "icon": ability_icons.get(ab_def),
                    "x": round((2.6 + 1.35 * (aj + 1)) * ux, 3), "y": round((2.6 + 1.35 * (aj + 1)) * uy, 3),
                    "cost": stage, "abilities": [ab_def], "classBaseName": cls, "requireTier": stage,
                })
                connections.append("%s~%s" % (prev, nid))
                prev = nid
                aj += 1
            # 强化节点
            for ui, ukey in enumerate(("upgrade1", "upgrade2")):
                u = cfg[ukey]
                nid = "%s_%s_U%d" % (prefix, cls, ui + 1)
                r = 2.6 + 1.35 * (aj + 1 + ui)
                add_hediff_node(nid, cls_label + "·" + u["labelSuffix"], u["description"], u["effects"],
                                "SkillUpgrade", r * ux, r * uy, 2 + ui, cfg["trunk"][0]["icon"],
                                extra={"classBaseName": cls, "requireTier": ui + 2})
                connections.append("%s~%s" % (prev, nid))
                prev = nid

        tree_defs.append({
            "defName": "MMT_TalentTree_F_%s" % key,
            "label": cfg["label"],
            "description": cfg.get("description", ""),
            "treeKind": "Family",
            "familyKey": key,
            "memberClasses": members,
            "nodes": nodes,
            "connections": connections,
        })
    return tree_defs, hediffs


def copy_isekai_textures(check_only=False):
    """复制 Isekai 授权贴图（幂等）。"""
    copied = []
    sk = os.path.join(ISEKAI, "Textures", "SkillTree")
    if not os.path.isdir(sk):
        err("Isekai 贴图目录不存在: " + sk)
        return copied
    targets = []
    for name in ("NodeMinor", "NodeNotable", "NodeKeystone", "NodeStart",
                 "NodeMinorActive", "NodeNotableActive", "NodeKeystoneActive", "NodeStartActive",
                 "ClassPanel", "DetailPanel", "SkillTreeContainerBG", "ConstellationBg", "ClassTab", "MagicCircle"):
        targets.append((os.path.join(sk, name + ".png"), OUT_TEX_DIR))
    icons_dir = os.path.join(sk, "Icons")
    if os.path.isdir(icons_dir):
        for fn in os.listdir(icons_dir):
            if fn.endswith(".png"):
                targets.append((os.path.join(icons_dir, fn), os.path.join(OUT_TEX_DIR, "Icons")))
    world_dir = os.path.join(ISEKAI, "Textures", "World")
    for sub in ("", "Sites"):
        d = os.path.join(world_dir, sub) if sub else world_dir
        if os.path.isdir(d):
            for fn in os.listdir(d):
                if fn.endswith(".png"):
                    targets.append((os.path.join(d, fn), os.path.join(OUT_TEX_DIR, "World")))
    for src, dst_dir in targets:
        if not os.path.isfile(src):
            err("Isekai 贴图缺失: " + src)
            continue
        dst = os.path.join(dst_dir, os.path.basename(src))
        if check_only:
            if not os.path.isfile(dst):
                err("贴图未复制: " + os.path.relpath(dst, ROOT))
            continue
        os.makedirs(dst_dir, exist_ok=True)
        if not os.path.isfile(dst) or os.path.getsize(dst) != os.path.getsize(src):
            shutil.copyfile(src, dst)
            copied.append(dst)
    return copied


def write_file(path, lines, check_only, changed):
    content = "\n".join(lines) + "\n"
    if os.path.isfile(path):
        with open(path, encoding="utf-8") as f:
            if f.read() == content:
                return False
    if check_only:
        changed.append(os.path.relpath(path, ROOT))
        return True
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    return True


def main():
    check_only = "--check" in sys.argv
    mapping = load_json(TALENT_JSON)
    rom = load_json(ROM_JSON)
    perks = parse_perk_hediffs()
    ability_icons = parse_ability_icons()

    general_tree, general_hediffs = build_general(mapping, perks)
    family_trees, family_hediffs = build_family_trees(mapping, rom, ability_icons)

    # 家族解锁节点 ↔ 家族树覆盖校验
    unlock_families = {n["familyKey"] for n in general_tree["nodes"] if n.get("familyKey")}
    tree_families = {t["familyKey"] for t in family_trees}
    for fk in sorted(tree_families - unlock_families):
        err("家族树 %s 在通用树中没有解锁节点" % fk)
    for fk in sorted(unlock_families - tree_families):
        err("通用树解锁节点 %s 没有对应家族树" % fk)
    # 37 职业技能全覆盖校验
    covered = set()
    for t in family_trees:
        for n in t["nodes"]:
            for ab in n.get("abilities", []):
                if ab in covered:
                    err("技能 %s 被多个节点引用" % ab)
                covered.add(ab)
    for cls, info in rom["classes"].items():
        for ab in info.get("abilities", []):
            if ab not in covered:
                err("职业 %s 技能 %s 未被任何家族树节点覆盖" % (cls, ab))

    changed = []
    # 通用树 XML
    lines = ['<?xml version="1.0" encoding="utf-8"?>', HEADER, "<Defs>"]
    emit_tree(general_tree, lines)
    lines.append("</Defs>")
    write_file(OUT_GENERAL, lines, check_only, changed)
    # 家族树 XML
    lines = ['<?xml version="1.0" encoding="utf-8"?>', HEADER, "<Defs>"]
    for t in family_trees:
        emit_tree(t, lines)
    lines.append("</Defs>")
    write_file(OUT_FAMILIES, lines, check_only, changed)
    # 被动 Hediff XML
    lines = ['<?xml version="1.0" encoding="utf-8"?>', HEADER, "<Defs>"]
    seen = set()
    for def_name, label, desc, effects in general_hediffs + family_hediffs:
        if def_name in seen:
            err("Hediff defName 重复: " + def_name)
            continue
        seen.add(def_name)
        emit_hediff(def_name, label, desc, effects, lines)
    lines.append("</Defs>")
    write_file(OUT_HEDIFFS, lines, check_only, changed)
    # 贴图
    copied = copy_isekai_textures(check_only)

    total_nodes = len(general_tree["nodes"]) + sum(len(t["nodes"]) for t in family_trees)
    print("天赋树：通用 %d 节点 + %d 棵家族树 %d 节点 = 共 %d 节点；被动 Hediff %d 个；贴图复制 %d 张"
          % (len(general_tree["nodes"]), len(family_trees),
             sum(len(t["nodes"]) for t in family_trees), total_nodes, len(seen), len(copied)))
    if changed:
        print("待更新文件: " + ", ".join(changed))
    if errors:
        print("共 %d 个错误" % len(errors), file=sys.stderr)
        sys.exit(1)
    if check_only and changed:
        sys.exit(1)


if __name__ == "__main__":
    main()
