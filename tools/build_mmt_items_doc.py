#!/usr/bin/env python3
"""生成 MMT 其它物品手册 docs/mmt-items.md（全部非武器、非建筑 ThingDef）。

第 1 部分：MMT 物品总表（价值/堆叠/材料类别/产出配方/被引用次数），按目录+链分组。
第 2 部分：HSK Core_SK 中 MMT 未实现的资源/材料类物品清单（不含防具服装——MMT 无防具链）。
改 Def 后重跑：python tools/build_mmt_items_doc.py
"""
from __future__ import annotations

import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from docgen_common import (  # noqa: E402
    FAMILY_ZH, HSK_CORE_SK, MMT_DEFS, ROOT, DefIndex, Namer, chain_list,
    chain_text, load_alias, load_hsk_zh, load_mmt_labels,
    load_vanilla_thing_names, merged_kv, mmt_to_hsk, recipe_ingredients,
    recipe_products, stuff_cat_zh,
)

OUT = ROOT / "docs/mmt-items.md"

# MMT 物品目录 -> 章节名
DIR_ZH = {
    "ThingDefs_Resources": "资源与材料",
    "ThingDefs_Items": "制品（仿生/药品/护盾/炮塔弹药等）",
    "ThingDefs_Plants": "作物",
}


def family_of(rel_path: str) -> str:
    name = Path(rel_path).stem
    for pre in ("MMT_Items_Resource_", "MMT_Items_", "MMT_"):
        if name.startswith(pre):
            return name[len(pre):]
    return name


def stuff_note(chain) -> str:
    """可作为建筑材料（stuffProps）的类别说明。"""
    for node in chain:
        sp = node.find("stuffProps")
        if sp is not None:
            cats = [c.text.strip() for c in sp.findall("./categories/li") if c.text]
            return "/".join(stuff_cat_zh(c) for c in cats) if cats else "是"
    return ""


def main() -> int:
    alias = load_alias()
    namer = Namer(load_mmt_labels(), load_hsk_zh())

    mmt = DefIndex()
    mmt.add_dir(MMT_DEFS, def_kinds=("ThingDef", "RecipeDef"))
    mmt_recipes = {k: n for k, n in mmt.nodes.items() if n.tag == "RecipeDef" and n.findtext("defName")}

    # 产物反查 + 引用统计（配方配料 + 建筑 costList）
    prod_recipe: dict[str, list[str]] = {}
    ref_count: Counter = Counter()
    for rname, rnode in mmt_recipes.items():
        for d in recipe_products(rnode):
            prod_recipe.setdefault(d, []).append(rname)
        for names, _count in recipe_ingredients(rnode):
            for n in names:
                if not n.startswith("["):
                    ref_count[n] += 1
    for k, n in mmt.nodes.items():
        if n.tag == "ThingDef" and "/ThingDefs_Buildings/" in mmt.file_of(k):
            for d in merged_kv(mmt.chain(k), "costList"):
                ref_count[d] += 1

    # ---------- 第 1 部分：MMT 物品 ----------
    item_defs = [
        k for k in mmt.concrete_def_names()
        if mmt.nodes[k].tag == "ThingDef"
        and "/ThingDefs_Weapons/" not in mmt.file_of(k)
        and "/ThingDefs_Buildings/" not in mmt.file_of(k)
        and "Mote" not in k  # mote/特效类不是物品
    ]

    def dir_of(k: str) -> str:
        for part in mmt.file_of(k).split("/"):
            if part in DIR_ZH:
                return part
        return "其它"

    groups: dict[tuple[str, str], list[str]] = {}
    for k in item_defs:
        groups.setdefault((dir_of(k), family_of(mmt.file_of(k))), []).append(k)

    out: list[str] = []
    out.append("# MMT 其它物品手册（非武器/非建筑）")
    out.append("")
    out.append("- 由 `tools/build_mmt_items_doc.py` 自动生成（改 Def 后重跑）；MMT 数据源 `1.7/Defs`，HSK 数据源 `Hardcore-SK/Mods/Core_SK`（只读参考）。")
    out.append("- 被引用 = 作为配方配料或建筑造价出现的次数（类别过滤不计）；产出 = 有配方能制造它；材料 = stuffProps 类别（可作建造/制作用材料）。")
    out.append("- 第 2 部分为 HSK Core_SK 资源/材料类物品中 MMT 未实现的清单（不含防具服装，MMT 无防具链）；武器见 [mmt-weapons.md](mmt-weapons.md)，建筑见 [mmt-buildings.md](mmt-buildings.md)。")
    out.append("")
    out.append("## 1. MMT 物品总表")
    out.append("")
    out.append(f"共 {len(item_defs)} 件（不含抽象基类）。")
    out.append("")

    dir_order = ["ThingDefs_Resources", "ThingDefs_Items", "ThingDefs_Plants", "其它"]
    sec = 0
    for d in dir_order:
        fams = sorted(f for (dd, f) in groups if dd == d)
        if not fams:
            continue
        sec += 1
        out.append(f"### 1.{sec} {DIR_ZH.get(d, d)}")
        out.append("")
        for fam in fams:
            keys = groups[(d, fam)]
            zh = FAMILY_ZH.get(fam, fam)
            out.append(f"#### {zh}（{fam}）")
            out.append("")
            out.append("| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |")
            out.append("| --- | --- | --- | --- | --- | --- |")
            for k in keys:
                chain = mmt.chain(k)
                stats = merged_kv(chain, "statBases")
                mv = stats.get("MarketValue", "—")
                stack = chain_text(chain, "stackLimit") or "—"
                stuff = stuff_note(chain) or "—"
                prods = "、".join(prod_recipe.get(k, [])) or "—"
                out.append(f"| {namer.mmt(k)}（`{k}`） | {mv} | {stack} | {stuff} | {prods} | {ref_count.get(k, 0)} |")
            out.append("")

    # ---------- 第 2 部分：HSK 未实现资源/材料 ----------
    hsk = DefIndex()
    hsk.add_dir(HSK_CORE_SK / "Defs/ThingDefs_Resources")
    hsk.add_dir(HSK_CORE_SK / "Defs/ThingDefs_Items")

    mmt_names = set(mmt.concrete_def_names())
    mmt_referenced = set(ref_count) | set(prod_recipe)
    hsk_names = set(hsk.concrete_def_names())
    vanilla_names = load_vanilla_thing_names()

    def implemented(def_name: str) -> bool:
        if def_name in vanilla_names:  # HSK 对原版 def 的改写，不算 HSK 新增内容
            return True
        if def_name in mmt_names or f"MMT_{def_name}" in mmt_names:
            return True
        if def_name in mmt_referenced:  # MMT 直接沿用该 def（如原版/HSK 资源）
            return True
        return any(mmt_to_hsk(m, alias, hsk_names) == def_name for m in mmt_names)

    hsk_missing: dict[str, list[list[str]]] = {}
    for k in hsk.concrete_def_names():
        if implemented(k):
            continue
        chain = hsk.chain(k)
        cats = chain_list(chain, "thingCategories")
        stats = merged_kv(chain, "statBases")
        row = [namer.hsk(k), f"`{k}`", "/".join(cats) or "—", stats.get("MarketValue", "—")]
        hsk_missing.setdefault(Path(hsk.file_of(k)).name, []).append(row)

    total_missing = sum(len(v) for v in hsk_missing.values())
    out.append("## 2. HSK 有而 MMT 未实现的资源/材料")
    out.append("")
    out.append(f"共 {total_missing} 件，按 HSK 源文件分组。中文名取自 HSK 汉化（Core_SK DefInjected），无翻译时保留英文 label。与原版同名的 def（HSK 对原版的改写）不计入。")
    out.append("")
    sub = 0
    for fname in sorted(hsk_missing):
        sub += 1
        out.append(f"### 2.{sub} {fname}")
        out.append("")
        out.append("| 物品 | defName | 类别 | 价值 |")
        out.append("| --- | --- | --- | --- |")
        for row in hsk_missing[fname]:
            out.append("| " + " | ".join(row) + " |")
        out.append("")

    OUT.write_text("\n".join(out) + "\n", encoding="utf-8")
    print(OUT)
    print(f"MMT 物品 {len(item_defs)} 件；HSK 未实现资源/材料 {total_missing} 件")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
