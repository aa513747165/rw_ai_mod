# -*- coding: utf-8 -*-
"""研究布局校验：LAYOUT 表与 XML 坐标一致、无重叠、MMT 内部前置边全部向右（依赖 X > 前置 X）。"""
import glob
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from validate_mod import fail, parse_xml  # noqa: E402
from apply_research_layout import LAYOUT, VANILLA_LAYOUT  # noqa: E402

RESEARCH_GLOB = "1.7/Defs/ResearchProjectDefs/MMT_Research*.xml"
PATCH_GLOB = "1.7/Patches/MMT_Patch_*.xml"

# 原版研究经补丁改写后的前置（依赖 -> 前置列表，含 MMT 与原版 defName）
VANILLA_EDGES = {
    "PsychoidBrewing": ["MMT_Drugs_A1"],
    "DrugProduction": ["MMT_Drugs_B1", "MMT_AdvancedPetrochemistry"],
    "PsychiteRefining": ["MMT_Drugs_C3"],
    "WakeUpProduction": ["MMT_Drugs_C2"],
    "PenoxycylineProduction": ["MMT_Drugs_C2"],
    "GoJuiceProduction": ["WakeUpProduction", "PsychiteRefining"],
    "MedicineProduction": ["MMT_Medicine_B2"],
}


def validate_research():
    defs = {}  # defName -> (x, y, prereqs)
    for path in sorted(glob.glob(RESEARCH_GLOB)):
        root = parse_xml(path)
        for rp in root.iter("ResearchProjectDef"):
            name = rp.findtext("defName")
            if not name:
                continue
            pres = [li.text.strip() for li in rp.findall("./prerequisites/li") if li.text]
            defs[name] = (
                float(rp.findtext("researchViewX")),
                float(rp.findtext("researchViewY")),
                pres,
            )

    # 1. 覆盖：LAYOUT 与 XML 中的 MMT 研究一一对应
    for name in defs:
        if name not in LAYOUT:
            fail(f"研究 {name} 缺少 LAYOUT 坐标（tools/apply_research_layout.py）")
    for name in LAYOUT:
        if name not in defs:
            fail(f"LAYOUT 中的 {name} 未在研究 Def 中找到")

    # 2. 坐标一致 + 无重叠
    pos = {}
    for name, (x, y, _) in defs.items():
        if name in LAYOUT:
            lx, ly = LAYOUT[name]
            if (x, y) != (float(lx), float(ly)):
                fail(f"研究 {name} 坐标 ({x},{y}) 与 LAYOUT ({lx},{ly}) 不一致")
        if (x, y) in pos:
            fail(f"研究坐标重叠: {name} 与 {pos[(x, y)]} 同在 ({x},{y})")
        pos[(x, y)] = name

    # 3. MMT 内部前置边必须向右：依赖 X 严格大于前置 X（箭头不回退）
    for name, (x, y, pres) in defs.items():
        for pre in pres:
            if pre in defs and defs[pre][0] >= x:
                fail(f"研究前置回退: {pre}{defs[pre][:2]} -> {name}({x},{y})")

    # 4. 补丁注入的原版研究：坐标与 VANILLA_LAYOUT 一致、不与任何节点重叠、前置边向右
    import re as _re

    patch_text = ""
    for path in sorted(glob.glob(PATCH_GLOB)):
        with open(path, encoding="utf-8-sig") as f:
            patch_text += f.read()
    vpos = {}
    for name, (lx, ly) in VANILLA_LAYOUT.items():
        mx = _re.search(r'defName="%s"\]/researchViewX</xpath>\s*<value><researchViewX>([^<]+)' % name, patch_text)
        my = _re.search(r'defName="%s"\]/researchViewY</xpath>\s*<value><researchViewY>([^<]+)' % name, patch_text)
        if not mx or not my:
            fail(f"补丁中未找到原版研究 {name} 的坐标注入")
            continue
        x, y = float(mx.group(1)), float(my.group(1))
        if (x, y) != (float(lx), float(ly)):
            fail(f"原版研究 {name} 补丁坐标 ({x},{y}) 与 VANILLA_LAYOUT ({lx},{ly}) 不一致")
        vpos[name] = (x, y)
        if (x, y) in pos:
            fail(f"研究坐标重叠: {name}(原版) 与 {pos[(x, y)]} 同在 ({x},{y})")
        pos[(x, y)] = name + "(原版)"
    # 原版<->MMT 交叉前置边向右
    allpos = {n: (d[0], d[1]) for n, d in defs.items()}
    allpos.update(vpos)
    for name, pres in VANILLA_EDGES.items():
        if name not in vpos:
            continue
        for pre in pres:
            if pre in allpos and allpos[pre][0] >= vpos[name][0]:
                fail(f"研究前置回退(原版链): {pre}{allpos[pre]} -> {name}{vpos[name]}")
    # MMT 研究以前版注入研究为前置的边也向右
    for name, (x, y, pres) in defs.items():
        for pre in pres:
            if pre in vpos and vpos[pre][0] >= x:
                fail(f"研究前置回退: {pre}(原版){vpos[pre]} -> {name}({x},{y})")


SUITES = {"research": validate_research}
