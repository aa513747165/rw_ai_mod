# -*- coding: utf-8 -*-
"""敌人武器池静态校验：原版派系刷装池内不得混入超过该派系科技等级的 MMT 武器。

背景：PawnWeaponGenerator 只按 weaponTags + weaponMoney 选武器，不做科技检查。
当前 tag 结构使 MMT 武器天然分层（部落池无工业/太空 tag），本套件防回归：
- 解析原版+DLC 派系（techLevel）与其 pawnGroupMakers 引用的 PawnKindDef（weaponTags）；
- 解析全部武器（原版+DLC+MMT，含 MMT 补丁对原版武器 techLevel 的覆盖）；
- MMT 武器（或被 MMT 改造的原版武器）落入更低科技派系武器池则 fail；原版自身问题仅警告。
"""
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from validate_mod import fail, parse_xml  # noqa: E402

GAME_DIR = os.environ.get("RIMWORLD_PATH", r"D:\RimWorld v1.7.4979")
DLC_DIRS = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]
MMT_WEAPONS_GLOB = "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_*.xml"
MMT_VANILLA_PATCH = "1.7/Patches/MMT_Patch_VanillaWeapons.xml"

TECH_RANK = {"Undefined": 0, "Animal": 0, "Neolithic": 1, "Medieval": 2,
             "Industrial": 3, "Spacer": 4, "Ultra": 5, "Archotech": 6}


def game_def_files():
    """原版+DLC 全部 Def xml 文件（Core 在前）。"""
    files = []
    for dlc in DLC_DIRS:
        files += glob.glob(os.path.join(GAME_DIR, "Data", dlc, "Defs", "**", "*.xml"), recursive=True)
    return files


def gather_defs(files, tag):
    """收集指定 def 类型：返回 (按 Name 的抽象 def, 按 defName 的具体 def)。"""
    abstracts, concretes = {}, {}
    for f in files:
        try:
            root = ET.parse(f).getroot()
        except ET.ParseError:
            continue
        for el in root.findall(tag):
            if el.get("Name"):
                abstracts[el.get("Name")] = el
            dn = el.findtext("defName")
            if dn:
                concretes[dn] = el
    return abstracts, concretes


def resolve_fields(el, abstracts, fields, _memo=None):
    """按 XML 继承链解析字段：子 def 定义则覆盖父 def（简化规则，标量与列表均为整体替换）。"""
    merged = {}
    parent_name = el.get("ParentName")
    if parent_name and parent_name in abstracts:
        merged = resolve_fields(abstracts[parent_name], abstracts, fields)
    for field in fields:
        node = el.find(field)
        if node is None:
            continue
        if field == "weaponTags":
            merged[field] = [li.text for li in node.findall("li") if li.text]
        else:
            merged[field] = node  # pawnGroupMakers/techLevel 等保留原始节点或文本
    return merged


def load_weapons():
    """全部可用武器：返回 (合并表, 原版基线表)。

    合并表 defName -> dict(tech, tags, mmt)：含原版+DLC、MMT 及 MMT 补丁 techLevel 覆盖；
    原版基线表不含 MMT 武器与补丁覆盖，用于判断"该 pawnkind 池子原版本就有的最高科技"。
    """
    weapons = {}
    baseline = {}
    for files, is_mmt in ((game_def_files(), False), (glob.glob(MMT_WEAPONS_GLOB), True)):
        abstracts, concretes = gather_defs(files, "ThingDef")
        for dn, el in concretes.items():
            r = resolve_fields(el, abstracts, ("techLevel", "weaponTags"))
            tags = r.get("weaponTags") or []
            if not tags:
                continue
            tech = r.get("techLevel")
            entry = {
                "tech": tech.text if tech is not None else "Undefined",
                "tags": tags,
                "mmt": is_mmt,
            }
            weapons[dn] = entry
            if not is_mmt:
                baseline[dn] = entry
    # MMT 补丁对原版武器 techLevel 的覆盖（这些原版 defName 视为 MMT 改造武器）
    patch = open(MMT_VANILLA_PATCH, encoding="utf-8").read()
    for m in re.finditer(
        r'<xpath>Defs/ThingDef\[defName="(\w+)"\]/techLevel</xpath>\s*<value>\s*<techLevel>(\w+)</techLevel>',
        patch,
    ):
        dn, tech = m.group(1), m.group(2)
        if dn in weapons:
            weapons[dn]["tech"] = tech
            weapons[dn]["mmt"] = True
    return weapons, baseline


def load_pawnkind_tags():
    """原版 PawnKindDef -> 解析继承后的 weaponTags。"""
    abstracts, concretes = gather_defs(game_def_files(), "PawnKindDef")
    tags_by_pk = {}
    for dn, el in concretes.items():
        r = resolve_fields(el, abstracts, ("weaponTags",))
        if r.get("weaponTags"):
            tags_by_pk[dn] = r["weaponTags"]
    return tags_by_pk


def load_factions():
    """原版派系 -> (techLevel, 引用的 pawnkind 集合)。"""
    abstracts, concretes = gather_defs(game_def_files(), "FactionDef")
    factions = {}
    for dn, el in concretes.items():
        if "Player" in dn:
            continue
        r = resolve_fields(el, abstracts, ("techLevel", "pawnGroupMakers"))
        tech_node = r.get("techLevel")
        tech = tech_node.text if isinstance(tech_node, ET.Element) else (tech_node or "Undefined")
        pgm = r.get("pawnGroupMakers")
        pawnkinds = set()
        if isinstance(pgm, ET.Element):
            for opt in pgm.findall("./li/options/*"):
                pawnkinds.add(opt.tag)
        if pawnkinds:
            factions[dn] = (tech, pawnkinds)
    return factions


def validate_enemypools():
    weapons, baseline = load_weapons()
    pk_tags = load_pawnkind_tags()
    factions = load_factions()
    if len(factions) < 8:
        fail(f"派系解析数量异常（{len(factions)}），请检查游戏目录: {GAME_DIR}")
    if len(weapons) < 100:
        fail(f"武器解析数量异常（{len(weapons)}），请检查游戏目录: {GAME_DIR}")

    # 每个 pawnkind 的原版基线池最高科技：MMT 武器不超出基线即视为与原版同级，不视为泄漏
    baseline_max = {}
    for pk, tags in pk_tags.items():
        ranks = [TECH_RANK.get(w["tech"], 0) for w in baseline.values() if set(w["tags"]) & set(tags)]
        baseline_max[pk] = max(ranks, default=0)

    mmt_violations, vanilla_violations, notes = [], [], []
    for fac_name, (tech, pawnkinds) in sorted(factions.items()):
        cap = TECH_RANK.get(tech, 0)
        for pk in pawnkinds:
            tags = pk_tags.get(pk)
            if not tags:
                continue
            for w_dn, w in weapons.items():
                w_rank = TECH_RANK.get(w["tech"], 0)
                if w_rank <= cap or not set(w["tags"]) & set(tags):
                    continue
                msg = f"{fac_name}({tech}) <- {pk} 池内含 {w_dn}({w['tech']})"
                if not w["mmt"]:
                    vanilla_violations.append(msg)
                elif w_rank <= baseline_max.get(pk, 0):
                    # 与原版基线同级（原版池里本就有同科技武器），仅记录
                    notes.append(msg)
                else:
                    mmt_violations.append(msg)
    if vanilla_violations:
        print(f"  [警告] 原版自身科技越级 {len(vanilla_violations)} 条（不阻断）")
    if notes:
        print(f"  [信息] MMT 武器与原版基线同级 {len(notes)} 条（可接受）:")
        for msg in notes[:5]:
            print(f"    {msg}")
    if mmt_violations:
        for msg in mmt_violations:
            print(f"  [FAIL] {msg}")
        fail(f"MMT 武器泄漏进低科技原版派系武器池 {len(mmt_violations)} 条")
    print("enemypools: OK")


SUITES = {"enemypools": validate_enemypools}
