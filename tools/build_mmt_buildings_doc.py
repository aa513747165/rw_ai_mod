#!/usr/bin/env python3
"""生成 MMT 建筑手册 docs/mmt-buildings.md。

第 1 部分：MMT 全部建筑（造价/功耗/研究前置 + 每个建筑的配方清单）。
第 2 部分：HSK Core_SK 中 MMT 未实现的生产建筑清单。
改 Def 后重跑：python tools/build_mmt_buildings_doc.py
"""
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from docgen_common import (  # noqa: E402
    FAMILY_ZH, HSK_CORE_SK, MMT_DEFS, ROOT, DefIndex, Namer, chain_list,
    chain_power, chain_text, cost_text, fmt_recipe, li_texts, load_alias,
    load_hsk_zh, load_mmt_labels, merged_kv, mmt_to_hsk, recipe_users,
)

OUT = ROOT / "docs/mmt-buildings.md"


def family_of(rel_path: str) -> str:
    name = Path(rel_path).stem
    for pre in ("MMT_Buildings_", "MMT_"):
        if name.startswith(pre):
            return name[len(pre):]
    return name


def building_row(def_name: str, mmt: DefIndex, namer: Namer, recipes: dict,
                 users_map: dict[str, list[str]]) -> tuple[dict, list[str]]:
    """返回 (主表字段 dict, 配方摘要行列表)。"""
    chain = mmt.chain(def_name)
    cost_list = merged_kv(chain, "costList")
    stuff_count = chain_text(chain, "costStuffCount")
    stuff_cats = chain_list(chain, "stuffCategories")
    research = chain_list(chain, "researchPrerequisites")
    # 建筑 recipes 列表 + 配方侧 recipeUsers 两种挂载方式都要收
    recipe_names = chain_list(chain, "recipes")
    for r in users_map.get(def_name, []):
        if r not in recipe_names:
            recipe_names.append(r)
    # MMT 自定义 comp（特殊机制建筑标注）
    comps = []
    for node in chain:
        comps_el = node.find("comps")
        if comps_el is None:
            continue
        for li in comps_el.findall("li"):
            cls = li.get("Class", "")
            if cls.startswith("MyModeTest."):
                comps.append(cls.split(".")[-1])
    mineable = chain_text(chain, "building/mineableThing")
    if mineable:
        comps.insert(0, f"矿脉（自然生成，产 {namer.mmt(mineable)}）")
    turret = chain_text(chain, "building/turretGunDef")
    if turret:
        comps.insert(0, f"炮塔（{namer.mmt(turret)}）")
    info = {
        "def": def_name,
        "label": namer.mmt(def_name),
        "size": chain_text(chain, "size") or "—",
        "cost": cost_text(cost_list, stuff_count, stuff_cats, namer.mmt),
        "power": (chain_power(chain) or "—") + ("W" if chain_power(chain) else ""),
        "work": merged_kv(chain, "statBases").get("WorkToBuild", "—"),
        "research": ", ".join(namer.mmt(r) for r in research) or "—",
        "recipes": recipe_names,
        "comps": comps,
    }
    lines = []
    for rname in recipe_names:
        rnode = recipes.get(rname)
        if rnode is None:
            lines.append(f"- `{rname}`：（未找到配方 def）")
        else:
            lines.append(f"- `{rname}`：{fmt_recipe(rnode, namer.mmt, research_fn=namer.mmt)}")
    return info, lines


def main() -> int:
    alias = load_alias()
    namer = Namer(load_mmt_labels(), load_hsk_zh())

    mmt = DefIndex()
    mmt.add_dir(MMT_DEFS, def_kinds=("ThingDef", "RecipeDef"))
    recipes = {k: n for k, n in mmt.nodes.items() if n.tag == "RecipeDef" and n.findtext("defName")}
    # 配方侧 recipeUsers 反查：工作台 defName -> [配方 defName]
    users_map: dict[str, list[str]] = {}
    for rname, rnode in recipes.items():
        for u in recipe_users(rnode):
            users_map.setdefault(u, []).append(rname)

    hsk = DefIndex()
    hsk.add_dir(HSK_CORE_SK / "Defs/ThingDefs_Buildings")

    # ---------- 第 1 部分：MMT 建筑 ----------
    bld_defs = [
        k for k in mmt.concrete_def_names()
        if mmt.nodes[k].tag == "ThingDef" and "/ThingDefs_Buildings/" in mmt.file_of(k)
    ]
    by_family: dict[str, list[str]] = {}
    for k in bld_defs:
        by_family.setdefault(family_of(mmt.file_of(k)), []).append(k)

    out: list[str] = []
    out.append("# MMT 建筑手册（造价与配方）")
    out.append("")
    out.append("- 由 `tools/build_mmt_buildings_doc.py` 自动生成（改 Def 后重跑）；MMT 数据源 `1.7/Defs`，HSK 数据源 `Hardcore-SK/Mods/Core_SK`（只读参考）。")
    out.append("- 字段沿 ParentName 继承链合并；`[类别]×n` 为 stuff 材料（costStuffCount）；配方数值对比见 [mmt-vs-hsk-resource-chains.md](mmt-vs-hsk-resource-chains.md)，武器类建筑数值见 [mmt-weapons.md](mmt-weapons.md)。")
    out.append("- 第 2 部分为 HSK Core_SK 中 MMT 未实现的生产建筑（有配方列表或属生产分类），可作后续补全清单；家具/装饰/地面等无关类别不列。")
    out.append("")
    out.append("## 1. MMT 建筑总表")
    out.append("")
    out.append(f"共 {len(bld_defs)} 座建筑（不含抽象基类），按链分组；矿脉类建筑的产量对比见对比文档第 2 节。")
    out.append("")

    for fam in sorted(by_family):
        keys = by_family[fam]
        zh = FAMILY_ZH.get(fam, fam)
        out.append(f"### {zh}（{fam}）")
        out.append("")
        out.append("| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |")
        out.append("| --- | --- | --- | --- | --- | --- | --- | --- |")
        recipe_blocks: list[tuple[str, list[str]]] = []
        for k in keys:
            info, lines = building_row(k, mmt, namer, recipes, users_map)
            note = "、".join(info["comps"])
            out.append(
                f"| {info['label']}（`{info['def']}`） | {info['size']} | {info['cost']} | {info['power']} "
                f"| {info['work']} | {info['research']} | {len(info['recipes'])} | {note} |"
            )
            if lines:
                recipe_blocks.append((f"{info['label']}（`{info['def']}`）", lines))
        out.append("")
        for title, lines in recipe_blocks:
            out.append(f"**{title} 的配方：**")
            out.append("")
            out.extend(lines)
            out.append("")

    # ---------- 第 2 部分：HSK 未实现生产建筑 ----------
    mmt_bld_set = set(bld_defs)
    hsk_names = set(hsk.concrete_def_names())
    implemented: set[str] = set()
    for k in mmt_bld_set:
        hk = mmt_to_hsk(k, alias, hsk_names)
        if hk:
            implemented.add(hk)

    hsk_missing = []
    for k in hsk.concrete_def_names():
        chain = hsk.chain(k)
        hsk_recipes = chain_list(chain, "recipes")
        cats = chain_list(chain, "thingCategories")
        if not hsk_recipes and "BuildingsProduction" not in cats:
            continue
        if k in implemented:
            continue
        cost_list = merged_kv(chain, "costList")
        hsk_missing.append({
            "def": k,
            "label": namer.hsk(k),
            "size": chain_text(chain, "size") or "—",
            "cost": cost_text(cost_list, chain_text(chain, "costStuffCount"), chain_list(chain, "stuffCategories"), namer.hsk),
            "power": (chain_power(chain) or "—") + ("W" if chain_power(chain) else ""),
            "recipes": len(hsk_recipes),
            "file": Path(hsk.file_of(k)).name,
        })

    out.append("## 2. HSK 有而 MMT 未实现的生产建筑")
    out.append("")
    out.append(f"共 {len(hsk_missing)} 座。中文名取自 HSK 汉化（Core_SK DefInjected），无翻译时保留英文 label。")
    out.append("")
    out.append("| 建筑 | defName | 尺寸 | 造价 | 功耗 | 配方数 | 源文件 |")
    out.append("| --- | --- | --- | --- | --- | --- | --- |")
    for m in hsk_missing:
        out.append(f"| {m['label']} | `{m['def']}` | {m['size']} | {m['cost']} | {m['power']} | {m['recipes']} | {m['file']} |")
    out.append("")

    OUT.write_text("\n".join(out) + "\n", encoding="utf-8")
    print(OUT)
    print(f"MMT 建筑 {len(bld_defs)} 座（{len(by_family)} 链）；HSK 未实现生产建筑 {len(hsk_missing)} 座")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
