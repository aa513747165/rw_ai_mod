#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""导出 MMT 全部研究为 markmap 兼容的思维导图文档。

解析 1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml，按前置关系构树，
并反向扫描 RecipeDef/ThingDef/植物的 researchPrerequisites 得出每项研究解锁的内容。
产物：docs/mmt-research-mindmap.md（生成物，改 Def 后需重跑本脚本）。
"""
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RESEARCH_GLOB = os.path.join(ROOT, "1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml")
RECIPE_GLOB = os.path.join(ROOT, "1.7/Defs/RecipeDefs/*.xml")
THING_GLOBS = [
    os.path.join(ROOT, "1.7/Defs/ThingDefs_Buildings/*.xml"),
    os.path.join(ROOT, "1.7/Defs/ThingDefs_Plants/*.xml"),
]
OUT_PATH = os.path.join(ROOT, "docs/mmt-research-mindmap.md")


def parse(path):
    return ET.parse(path).getroot()


def text_of(node, tag, default=""):
    child = node.find(tag)
    return child.text.strip() if child is not None and child.text else default


def li_values(node, tag):
    el = node.find(tag)
    if el is None:
        return []
    return [li.text.strip() for li in el.findall("li") if li.text]


def collect_research():
    """读取全部 MMT 研究项目。"""
    projects = {}
    for path in sorted(glob.glob(RESEARCH_GLOB)):
        for node in parse(path).findall("ResearchProjectDef"):
            name = text_of(node, "defName")
            if not name:
                continue
            projects[name] = {
                "label": text_of(node, "label"),
                "desc": text_of(node, "description"),
                "cost": text_of(node, "baseCost"),
                "tech": text_of(node, "techLevel"),
                "prereqs": li_values(node, "prerequisites"),
                "x": text_of(node, "researchViewX"),
                "y": text_of(node, "researchViewY"),
                "building": text_of(node, "requiredResearchBuilding"),
            }
    return projects


def collect_unlocks(projects):
    """反向扫描配方/建筑/植物，得出每项研究解锁的内容。"""
    names = set(projects)
    unlocks = defaultdict(lambda: {"recipes": [], "buildings": [], "plants": []})
    for path in sorted(glob.glob(RECIPE_GLOB)):
        for node in parse(path).findall("RecipeDef"):
            req = text_of(node, "researchPrerequisite")
            if req in names:
                label = text_of(node, "label") or text_of(node, "defName")
                unlocks[req]["recipes"].append(label)
    for pattern in THING_GLOBS:
        for path in sorted(glob.glob(pattern)):
            for node in parse(path).findall("ThingDef"):
                reqs = li_values(node, "researchPrerequisites")
                sow = li_values(node.find("plant") if node.find("plant") is not None else ET.Element("x"),
                                "sowResearchPrerequisites")
                name = text_of(node, "defName")
                label = text_of(node, "label") or name
                for req in reqs:
                    if req in names:
                        key = "plants" if node.find("plant") is not None else "buildings"
                        unlocks[req][key].append(label)
                for req in sow:
                    if req in names:
                        unlocks[req]["plants"].append(label + "（播种）")
    return unlocks


def build_tree(projects):
    """按前置关系构树：根=无 MMT 前置的研究；多前置挂到首个 MMT 前置，其余记为交叉链接。"""
    names = set(projects)
    children = defaultdict(list)
    cross_links = defaultdict(list)
    roots = []
    for name, p in projects.items():
        mmt_prereqs = [q for q in p["prereqs"] if q in names]
        if not mmt_prereqs:
            roots.append(name)
        else:
            children[mmt_prereqs[0]].append(name)
            for extra in mmt_prereqs[1:]:
                cross_links[name].append(extra)

    def sort_key(n):
        try:
            return (float(projects[n]["x"]), float(projects[n]["y"]))
        except ValueError:
            return (0.0, 0.0)

    roots.sort(key=sort_key)
    for kids in children.values():
        kids.sort(key=sort_key)
    return roots, children, cross_links


def node_line(name, projects, cross_links):
    p = projects[name]
    parts = ["**{}** `{}`".format(p["label"], name),
             "坐标 ({}, {})".format(p["x"], p["y"]),
             "{}点".format(p["cost"]),
             p["tech"]]
    vanilla = [q for q in p["prereqs"] if q not in projects]
    if vanilla:
        parts.append("前置(原版): " + ", ".join(vanilla))
    if cross_links.get(name):
        extra = ", ".join(projects[q]["label"] for q in cross_links[name])
        parts.append("交叉前置: " + extra)
    if p["building"]:
        parts.append("需要研究台: " + p["building"])
    return "- " + " ｜ ".join(parts)


def render(out, name, projects, children, cross_links, unlocks, depth, visited):
    indent = "  " * depth
    out.append(indent + node_line(name, projects, cross_links))
    u = unlocks.get(name)
    if u:
        for kind, title in (("buildings", "解锁建筑"), ("recipes", "解锁配方"), ("plants", "解锁植物")):
            if u[kind]:
                out.append(indent + "  - {}: {}".format(title, "、".join(u[kind])))
    if name in visited:  # 防御环
        return
    visited.add(name)
    for kid in children.get(name, []):
        render(out, kid, projects, children, cross_links, unlocks, depth + 1, visited)


def main():
    projects = collect_research()
    if not projects:
        print("未找到任何 MMT 研究定义", file=sys.stderr)
        return 1
    unlocks = collect_unlocks(projects)
    roots, children, cross_links = build_tree(projects)

    out = [
        "# MMT 研究思维导图",
        "",
        "<!-- 生成物：由 tools/build_research_mindmap.py 生成，改研究 Def 后重跑，请勿手改 -->",
        "<!-- 可用 markmap（如 VS Code Markmap 插件 / markmap.js）渲染为思维导图 -->",
        "",
        "- MMT 研究总览（{} 项）".format(len(projects)),
    ]
    visited = set()
    for root in roots:
        render(out, root, projects, children, cross_links, unlocks, 1, visited)
    # 防御：若有节点因环未被访问，附加列出
    leftovers = [n for n in projects if n not in visited]
    for name in sorted(leftovers):
        out.append("  " + node_line(name, projects, cross_links) + "（前置成环，未挂树）")

    # 带 BOM 写出：Markmap 部分查看器在 Windows 上按 ANSI(GBK) 解析无 BOM 文件导致中文乱码
    with open(OUT_PATH, "w", encoding="utf-8-sig") as f:
        f.write("\n".join(out) + "\n")
    print("已生成 {}（{} 项研究，{} 棵根）".format(OUT_PATH, len(projects), len(roots)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
