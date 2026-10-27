#!/usr/bin/env python3
"""生成 MMT 武器手册 docs/mmt-weapons.md。

第 1 部分：MMT 全部武器数值表（伤害/穿透/射程/射速/冷却/DPS/制造）。
第 2 部分：HSK Core_SK 中 MMT 未实现的武器清单（CE 数值原样列出，口径体系不同不直接可比）。
改 Def 后重跑：python tools/build_mmt_weapons_doc.py
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from docgen_common import (  # noqa: E402
    FAMILY_ZH, HSK_CE, HSK_CORE_SK, MMT_DEFS, ROOT, DefIndex, Namer,
    chain_list, chain_text, fmt_ingredients, load_alias, load_hsk_zh,
    load_mmt_labels, merged_kv, mmt_to_hsk, recipe_ingredients, recipe_products,
    recipe_skills, recipe_users,
)

OUT = ROOT / "docs/mmt-weapons.md"
MAPPING_JSON = ROOT / "tools/data/mmt_weapon_mapping.json"
VANILLA_WEAPONS_PATCH = ROOT / "1.7/Patches/MMT_Patch_VanillaWeapons.xml"

GROUPS = {  # MMT 武器文件名 -> 章节名
    "MMT_Weapons_Conventional.xml": "常规枪械",
    "MMT_Weapons_Archaic.xml": "冷兵器弓弩与手雷",
    "MMT_Weapons_Futuristic.xml": "未来与远古武器",
    "MMT_Weapons_TurretGuns.xml": "炮塔武器",
    "MMT_Weapons_NPC.xml": "NPC/特殊武器",
}


def load_synth_weapons() -> tuple[dict, dict]:
    """解析 MMT_Patch_VanillaWeapons.xml：同名替换的原版武器内容经 patch 注入。

    返回 (虚拟 ThingDef 表 vanilla名->Element, 注释映射 vanilla名->MMT_源def名)。
    与 build_mmt_weapon_chains.py 的 synth 机制一致。
    """
    synths: dict[str, object] = {}
    src: dict[str, str] = {}
    if not VANILLA_WEAPONS_PATCH.is_file():
        return synths, src
    import re
    text = VANILLA_WEAPONS_PATCH.read_text(encoding="utf-8")
    for m in re.finditer(r"<!--\s*(\w+)\s*←\s*(MMT_\w+)", text):
        src[m.group(1)] = m.group(2)
    import xml.etree.ElementTree as ET
    for op in ET.parse(VANILLA_WEAPONS_PATCH).getroot().findall("Operation"):
        operations = op.find("operations")
        if operations is None:
            continue
        synth = ET.Element("ThingDef")
        name = None
        for li in operations:
            xpath = li.findtext("xpath") or ""
            if 'defName="' in xpath:
                name = xpath.split('defName="')[1].split('"')[0]
            value = li.find("value")
            if value is not None and len(value):
                synth.append(value[0])
        if name:
            synths[name] = synth
    return synths, src


def synth_group(name: str) -> str:
    """同名替换武器归组（同 build_mmt_weapon_chains.py）：电荷系->未来，枪械->常规，其余->冷兵器。"""
    if name.startswith("Gun_Charge"):
        return "MMT_Weapons_Futuristic.xml"
    if name.startswith("Gun_"):
        return "MMT_Weapons_Conventional.xml"
    return "MMT_Weapons_Archaic.xml"


def first_verb(chain) -> dict:
    """继承链上第一个 verbs/li -> 关键字段。"""
    for node in chain:
        verb = node.find("./verbs/li")
        if verb is not None:
            g = lambda t: (verb.findtext(t) or "").strip() or None
            return {
                "range": g("range"), "warmup": g("warmupTime"),
                "burst": g("burstShotCount"), "ticks": g("ticksBetweenBurstShots"),
                "proj": g("defaultProjectile"), "verbClass": g("verbClass"),
            }
    return {}


def proj_stats(index: DefIndex, proj_name: str | None) -> dict:
    if not proj_name:
        return {}
    pnode = index.get(proj_name)
    if pnode is None:
        return {}
    pchain = index.chain(proj_name)
    for node in pchain:
        p = node.find("projectile")
        if p is not None:
            g = lambda t: (p.findtext(t) or "").strip() or None
            return {
                "dmg": g("damageAmountBase"), "ap": g("armorPenetrationBase"),
                "ap_sharp": g("armorPenetrationSharp"), "speed": g("speed"),
                "radius": g("explosionRadius"), "dmgDef": g("damageDef"),
            }
    return {}


def fnum(v: str | None) -> float | None:
    try:
        return float(v)
    except (TypeError, ValueError):
        return None


def dps_pair(dmg: float | None, burst: float, ticks: float, warmup: float, cooldown: float) -> tuple[str, str]:
    """(爆发DPS, 持续DPS)。tick=1/60s；爆发窗口=(连发数-1)×间隔；持续周期=预热+冷却+窗口。"""
    if dmg is None:
        return "—", "—"
    window = max(burst - 1, 0) * ticks / 60.0
    burst_dps = f"{dmg * burst / window:.1f}" if window > 0 else "—"
    cycle = warmup + cooldown + window
    sustained = f"{dmg * burst / cycle:.1f}" if cycle > 0 else "—"
    return burst_dps, sustained


def melee_tools(chain) -> list[dict]:
    for node in chain:
        tools = node.find("tools")
        if tools is None or not tools.findall("li"):
            continue
        out = []
        for li in tools.findall("li"):
            caps = [c.text.strip() for c in li.findall("./capacities/li") if c.text]
            out.append({
                "label": (li.findtext("label") or "").strip() or "/".join(caps),
                "power": (li.findtext("power") or "").strip(),
                "cd": (li.findtext("cooldownTime") or "").strip(),
                "ap": (li.findtext("armorPenetration") or "").strip(),
                "cf": (li.findtext("chanceFactor") or "1").strip(),
            })
        return out
    return []


def melee_dps(tools: list[dict]) -> str:
    """期望 DPS = Σ(power/cooldown×权重)/Σ权重（权重=chanceFactor）。"""
    num = den = 0.0
    for t in tools:
        p, cd, cf = fnum(t["power"]), fnum(t["cd"]), fnum(t["cf"])
        if p and cd and cf:
            num += p / cd * cf
            den += cf
    return f"{num / den:.1f}" if den else "—"


def ce_ammo_comp(chain) -> tuple[str | None, str | None]:
    """CE AmmoUser 的 (magazineSize, reloadTime)。"""
    for node in chain:
        comps = node.find("comps")
        if comps is None:
            continue
        for li in comps.findall("li"):
            if "AmmoUser" in (li.get("Class") or ""):
                return (
                    (li.findtext("magazineSize") or "").strip() or None,
                    (li.findtext("reloadTime") or "").strip() or None,
                )
    return None, None


def main() -> int:
    alias = load_alias()
    namer = Namer(load_mmt_labels(), load_hsk_zh())
    mapping = json.loads(MAPPING_JSON.read_text(encoding="utf-8"))

    mmt = DefIndex()
    mmt.add_dir(MMT_DEFS, def_kinds=("ThingDef", "RecipeDef"))
    mmt_recipes = {k: n for k, n in mmt.nodes.items() if n.tag == "RecipeDef" and n.findtext("defName")}
    # 产物 -> 配方反查；配方 -> 建筑反查（建筑 recipes 列表 + 配方侧 recipeUsers 两种挂载）
    prod_recipe: dict[str, list[str]] = {}
    bld_recipes: dict[str, list[str]] = {}
    for k, n in mmt.nodes.items():
        if n.tag != "ThingDef" or k in mmt.abstract:
            continue
        for r in chain_list(mmt.chain(k), "recipes"):
            bld_recipes.setdefault(r, []).append(k)
    for rname, rnode in mmt_recipes.items():
        for d in recipe_products(rnode):
            prod_recipe.setdefault(d, []).append(rname)
        for u in recipe_users(rnode):
            bld_recipes.setdefault(rname, []).append(u)

    # ---------- 第 1 部分：MMT 武器 ----------
    out: list[str] = []
    out.append("# MMT 武器手册（战斗数值）")
    out.append("")
    out.append("- 由 `tools/build_mmt_weapons_doc.py` 自动生成（改 Def 后重跑）；MMT 数据源 `1.7/Defs`，HSK 数据源 `Hardcore-SK/Mods/Core_SK`（只读参考）。")
    out.append("- DPS 口径：tick=1/60 秒；爆发 DPS=伤害×连发数÷((连发数-1)×连发间隔)，单发武器不适用（—）；持续 DPS=伤害×连发数÷(预热+冷却+连发窗口)。近战 DPS=Σ(伤害/冷却×概率)÷Σ概率。")
    out.append("- 制造配料链见 [mmt-weapon-chains.md](mmt-weapon-chains.md)；本手册重数值。")
    out.append("- 第 2 部分 HSK 武器为 CombatExtended 口径（伤害在弹药上、有弹匣/换弹），与 MMT 的原版式数值不直接可比，仅作移植参考清单。")
    out.append("")

    total_ranged = total_melee = 0
    synths, synth_src = load_synth_weapons()
    for fname, gname in GROUPS.items():
        # (展示名, 继承链) 列表：文件内 def + 归组的 patch 注入武器
        entries: list[tuple[str, list]] = [
            (k, mmt.chain(k))
            for k in mmt.concrete_def_names()
            if mmt.nodes[k].tag == "ThingDef" and mmt.file_of(k) == f"1.7/Defs/ThingDefs_Weapons/{fname}"
        ]
        entries += [(n, [el]) for n, el in synths.items() if synth_group(n) == fname]
        if not entries:
            continue
        out.append(f"## {gname}（{fname}）")
        out.append("")
        ranged_rows, melee_rows = [], []
        for k, chain in entries:
            label = chain_text(chain, "label") or namer.mmt(k)
            verb = first_verb(chain)
            tools = melee_tools(chain)
            stats = merged_kv(chain, "statBases")
            cooldown = stats.get("RangedWeapon_Cooldown")
            # 制造信息
            makers = []
            research = skills = ""
            for rname in prod_recipe.get(k, []):
                rnode = mmt_recipes[rname]
                tables = [namer.mmt(b) for b in bld_recipes.get(rname, [])]
                makers.append((rname, tables, rnode))
            if makers:
                rname, tables, rnode = makers[0]
                res = (rnode.findtext("researchPrerequisite") or "").strip()
                research = namer.mmt(res) if res else ""
                skills = recipe_skills(rnode)
            maker_txt = "、".join(t for _, ts, _ in makers for t in ts) or "—"
            if verb and (verb.get("range") or verb.get("proj")):
                ps = proj_stats(mmt, verb.get("proj"))
                burst = fnum(verb.get("burst")) or 1.0
                ticks = fnum(verb.get("ticks")) or 0.0
                warmup = fnum(verb.get("warmup")) or 0.0
                cd = fnum(cooldown) or 0.0
                bdps, sdps = dps_pair(fnum(ps.get("dmg")), burst, ticks, warmup, cd)
                note = []
                if ps.get("radius"):
                    note.append(f"爆炸半径{ps['radius']}")
                if ps.get("dmgDef") and ps["dmgDef"] not in ("Bullet",):
                    note.append(ps["dmgDef"])
                if not ps:
                    note.append(f"弹药 {verb.get('proj')} 非本 mod 定义")
                ranged_rows.append([
                    f"{label}（`{k}`）",
                    ps.get("dmg") or "—", ps.get("ap") or ps.get("ap_sharp") or "—",
                    ps.get("speed") or "—", verb.get("range") or "—",
                    f"{verb.get('burst') or 1}×{verb.get('ticks') or '—'}",
                    verb.get("warmup") or "0", cooldown or "0", bdps, sdps,
                    "；".join(note) or "—", research or "—", skills or "—", maker_txt,
                ])
                total_ranged += 1
            elif tools:
                tool_txt = "；".join(
                    f"{t['label']} {t['power']}伤/{t['cd']}s/{t['cf']}概率" + (f"/穿{t['ap']}" if t["ap"] else "")
                    for t in tools
                )
                melee_rows.append([
                    f"{label}（`{k}`）", tool_txt, melee_dps(tools),
                    research or "—", skills or "—", maker_txt,
                ])
                total_melee += 1
        if ranged_rows:
            out.append("| 武器 | 伤害 | 穿透 | 弹速 | 射程 | 连发×间隔tick | 预热s | 冷却s | 爆发DPS | 持续DPS | 备注 | 研究 | 技能 | 制造台 |")
            out.append("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
            for r in ranged_rows:
                out.append("| " + " | ".join(r) + " |")
            out.append("")
        if melee_rows:
            out.append("| 武器 | 近战方式（伤害/冷却s/概率） | 近战DPS | 研究 | 技能 | 制造台 |")
            out.append("| --- | --- | --- | --- | --- | --- |")
            for r in melee_rows:
                out.append("| " + " | ".join(r) + " |")
            out.append("")

    # ---------- 第 2 部分：HSK 未实现武器 ----------
    hsk = DefIndex()
    hsk.add_dir(HSK_CORE_SK / "Defs/ThingDefs_Weapons")
    proj_index = DefIndex()
    proj_index.add_dir(HSK_CORE_SK / "Defs/CombatExtended")
    proj_index.add_dir(HSK_CE / "Defs")

    batch_files = {f for files in mapping["batches"].values() for f in files}
    vanilla_repl = set(mapping.get("vanilla_replacements", {}))
    mmt_names = set(mmt.concrete_def_names())
    hsk_names = set(hsk.concrete_def_names())

    def implemented(def_name: str, file_name: str) -> bool:
        # 逐 def 判定（不用批次文件整批豁免：批次文件内也可能有个别武器未生成）
        if def_name in vanilla_repl:
            return True
        if def_name in mmt_names or f"MMT_{def_name}" in mmt_names:
            return True
        if f"MMT_{def_name}" in synth_src.values():  # patch 注入原版 def 的同名替换
            return True
        return any(mmt_to_hsk(m, alias, hsk_names) == def_name for m in mmt_names)

    hsk_rows = []
    for k in hsk.concrete_def_names():
        chain = hsk.chain(k)
        verb = first_verb(chain)
        tools = melee_tools(chain)
        if not verb and not tools:
            continue
        fname = Path(hsk.file_of(k)).name
        if implemented(k, fname):
            continue
        stats = merged_kv(chain, "statBases")
        kind = "混合" if (verb and tools) else ("远程" if verb else "近战")
        ps = proj_stats(proj_index, verb.get("proj")) if verb else {}
        mag, reload = ce_ammo_comp(chain)
        hsk_rows.append([
            namer.hsk(k), f"`{k}`", kind,
            ps.get("dmg") or "—", verb.get("range") or "—",
            verb.get("warmup") or "—", stats.get("RangedWeapon_Cooldown") or "—",
            (f"{verb.get('burst')}×{verb.get('ticks')}" if verb.get("burst") else "—"),
            (f"{mag}/{reload}s" if mag else "—"), fname,
        ])

    out.append("## HSK 有而 MMT 未实现的武器")
    out.append("")
    out.append(f"共 {len(hsk_rows)} 件（逐 def 判定：vanilla_replacements / MMT_ 同名 / patch 同名替换 / 别名表）。弹匣列为 弹匣容量/换弹秒。")
    out.append("")
    if not hsk_rows:
        out.append("- 无。Core_SK 全部武器均已移植（含经 patch 同名替换原版武器的部分）。")
        out.append("")
    else:
        out.append("| 武器 | defName | 类型 | 弹药伤害 | 射程 | 预热s | 冷却s | 连发×间隔tick | 弹匣/换弹 | 源文件 |")
        out.append("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
        for r in hsk_rows:
            out.append("| " + " | ".join(r) + " |")
        out.append("")

    OUT.write_text("\n".join(out) + "\n", encoding="utf-8")
    print(OUT)
    print(f"MMT 武器：远程 {total_ranged} / 近战 {total_melee}；HSK 未实现武器 {len(hsk_rows)} 件")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
