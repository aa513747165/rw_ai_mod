"""三份手册生成脚本（build_mmt_buildings_doc / build_mmt_weapons_doc / build_mmt_items_doc）的共用解析工具。

- DefIndex：扫描 XML 建 defName -> Element 索引，支持 ParentName 继承链合并。
- MMT 侧中文 label 直接读 1.7/Defs；HSK 侧中文名读 H:\\rw\\汉化 下 Core_SK 的 DefInjected。
- MMT -> HSK defName 别名表与 build_mmt_chains_compare.js 共用 tools/data/mmt_hsk_alias.json。
"""
from __future__ import annotations

import json
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MMT_DEFS = ROOT / "1.7/Defs"
HSK_CORE_SK = ROOT / "Hardcore-SK/Mods/Core_SK"
HSK_CE = ROOT / "Hardcore-SK/Mods/CombatExtended"
HSK_ZH = ROOT / "汉化/Languages/ChineseSimplified (简体中文)/DefInjected"
ALIAS_JSON = ROOT / "tools/data/mmt_hsk_alias.json"
VANILLA_DATA = Path("D:/SteamLibrary/steamapps/common/RimWorld/Data")

# 链（家族）中文名，与 build_mmt_chains_compare.js 的 FAMILY_ZH 保持一致
FAMILY_ZH = {
    "Metal": "金属", "Electric": "电动金属", "Precious": "贵金属", "AdvancedAlloys": "高级合金",
    "Wood": "木材", "ElectricWood": "电动木材", "Concrete": "混凝土", "Glass": "玻璃",
    "Electronics": "电子", "ElectricElectronics": "电动电子", "Petrochem": "石化",
    "ElectricPetrochem": "电动石化", "Parts": "零件",
    "Agriculture": "农业", "Bionics": "仿生", "Ceramics": "陶瓷", "Drugs": "药品", "Matter": "物质",
    "Medicine": "医药", "Nuclear": "核能", "OrePowder": "矿粉", "PetrochemExt": "石化扩展",
    "ProcessorModules": "处理机模块", "Robotics": "机器人", "Shield": "护盾", "Textiles": "纺织",
    "TopMaterials": "顶级材料", "TurretAmmo": "炮塔弹药", "TurretCrates": "炮塔箱",
    "WeaponParts": "武器部件", "Weapons_Archaic": "古代武器", "Weapons_Conventional": "常规武器",
    "Weapons_Futuristic": "未来武器", "ChargeShield": "充能护盾", "PsychicShield": "灵能护盾",
    "SieveMeshes": "筛网", "StateBackup": "状态备份", "BionicsRemove": "仿生移除",
    "Coal": "煤", "Sulfur": "硫", "Power": "电力", "Grinder": "研磨", "Sieve": "筛选",
    "ChunkDrill": "采石", "AutoProcessor": "自动处理", "AdvancedRocks": "深钻井",
    "Research": "研究", "ChargingStation": "充能", "Teleport": "传送", "Turrets": "炮塔",
    "Crops": "作物", "NaturalOrgans": "天然器官", "Neuromod": "神经调制",
    "BionicsAdv": "高级仿生", "BionicsCONN": "CONN 仿生",
    "AdvancedOre": "高级矿石", "SuperAlloys": "超级合金",
    "Plants_Cultivated": "栽培作物", "Hediffs_BionicsCONN": "CONN 仿生附件",
    "Cultivated": "栽培", "Items": "杂项", "Resources": "资源",
}

# stuff 材料类别中文名（与 build_mmt_weapon_chains.py 一致并扩充）
STUFF_CAT_ZH = {
    "Metallic": "金属", "Woody": "木质", "Stony": "石质", "Leathery": "皮革",
    "Fabric": "织物", "RuggedMetallic": "粗金属", "Bricks": "砖",
    "StrongMetallic": "坚固金属", "Plastic": "塑料", "Glassy": "玻璃质",
    "MetallicShiny": "贵金属",
}
# 常见原版/HSK 技能、研究与资源的中文名（MMT 无 label 时回落）
EXTRA_ZH = {
    "Crafting": "制作", "Construction": "建造", "Intellectual": "智识",
    "Artistic": "艺术", "Shooting": "射击", "Melee": "近战", "Mining": "采矿",
    "Steel": "钢", "WoodLog": "原木", "Plasteel": "玻璃钢", "Uranium": "铀",
    "ComponentIndustrial": "工业零件", "ComponentSpacer": "太空零件",
    "ComponentAdvanced": "高级零部件", "Gold": "黄金", "Silver": "白银",
    "Chemfuel": "化合燃料", "Cloth": "布",
    "Gunsmithing": "枪械制造", "BlowbackOperation": "反冲作用", "GasOperation": "导气式",
    "PrecisionRifling": "精密膛线", "MultibarrelWeapons": "多管武器", "HeavyTurrets": "重型炮塔",
    "Mortars": "迫击炮", "SniperTurret": "狙击炮塔", "Machining": "机械加工",
    "LongBlades": "长刃", "Greatbow": "巨弓", "Smithing": "锻造", "ChargedShot": "充能射击",
    "Fabrication": "制造", "RecurveBow": "反曲弓", "GunTurrets": "机枪塔",
    "Electricity": "电力", "Microelectronics": "微电子", "FabricationBench": "制造台",
}


class DefIndex:
    """key(defName，抽象基类用 Name) -> Element；带 ParentName 继承链。"""

    def __init__(self) -> None:
        self.nodes: dict[str, ET.Element] = {}
        self.parents: dict[str, str | None] = {}
        self.files: dict[str, str] = {}
        self.abstract: set[str] = set()

    def add_dir(self, base: Path, glob_pat: str = "**/*.xml", def_kinds: tuple[str, ...] = ("ThingDef",)) -> None:
        for path in sorted(base.glob(glob_pat)):
            try:
                root = ET.parse(path).getroot()
            except ET.ParseError:
                continue
            for node in root:
                if not isinstance(node.tag, str) or node.tag not in def_kinds:
                    continue
                def_name = node.findtext("defName")
                key = (def_name or node.get("Name") or "").strip()
                if not key:
                    continue
                self.nodes[key] = node
                self.parents[key] = node.get("ParentName")
                try:
                    self.files[key] = path.relative_to(ROOT).as_posix()
                except ValueError:
                    self.files[key] = path.name
                if node.get("Abstract") == "True":
                    self.abstract.add(key)

    def has(self, def_name: str) -> bool:
        return def_name in self.nodes and def_name not in self.abstract

    def get(self, def_name: str) -> ET.Element | None:
        return self.nodes.get(def_name)

    def chain(self, def_name: str) -> list[ET.Element]:
        """子类在前，沿 ParentName 到链断（父类不在索引内时停止）。"""
        out: list[ET.Element] = []
        key: str | None = def_name
        guard = 0
        while key and key in self.nodes and guard < 20:
            out.append(self.nodes[key])
            key = self.parents.get(key)
            guard += 1
        return out

    def file_of(self, def_name: str) -> str:
        return self.files.get(def_name, "")

    def concrete_def_names(self) -> list[str]:
        """有 defName 且非抽象的 def 名列表（按文件排序稳定）。"""
        return [k for k, n in self.nodes.items()
                if n.findtext("defName") and k not in self.abstract]


def chain_text(chain: list[ET.Element], path: str) -> str | None:
    """沿继承链取第一个非空文本，如 'statBases/WorkToBuild'。"""
    for node in chain:
        v = node.findtext(path)
        if v is not None and v.strip():
            return v.strip()
    return None


def chain_el(chain: list[ET.Element], path: str) -> ET.Element | None:
    """沿继承链取第一个存在的子元素（列表类字段整体替换语义）。"""
    for node in chain:
        el = node.find(path)
        if el is not None:
            return el
    return None


def kv_pairs(el: ET.Element | None) -> dict[str, str]:
    """<costList><Steel>50</Steel></costList> -> {'Steel': '50'}"""
    out: dict[str, str] = {}
    if el is None:
        return out
    for child in el:
        if isinstance(child.tag, str) and child.text:
            out[child.tag] = child.text.strip()
    return out


def merged_kv(chain: list[ET.Element], path: str) -> dict[str, str]:
    """按键合并继承链上的字典块（父类在前，子类覆盖），用于 costList/statBases。"""
    out: dict[str, str] = {}
    for node in reversed(chain):
        out.update(kv_pairs(node.find(path)))
    return out


def li_texts(el: ET.Element | None) -> list[str]:
    if el is None:
        return []
    return [li.text.strip() for li in el.findall("li") if li.text and li.text.strip()]


def chain_list(chain: list[ET.Element], path: str) -> list[str]:
    """沿继承链取第一个非空 <li> 列表（子类整体替换语义）。"""
    for node in chain:
        vals = li_texts(node.find(path))
        if vals:
            return vals
    return []


def load_alias() -> dict[str, str]:
    data = json.loads(ALIAS_JSON.read_text(encoding="utf-8"))
    data.pop("_comment", None)
    return data


def mmt_to_hsk(mmt_def: str | None, alias: dict[str, str], valid: set[str] | None = None) -> str | None:
    """MMT defName -> HSK defName；valid 提供时要求结果在其中。"""
    if not mmt_def:
        return None
    if not mmt_def.startswith("MMT_"):
        return mmt_def if (valid is None or mmt_def in valid) else None
    x = mmt_def[4:]
    cands = [alias.get(x), x, x[:-3] if x.endswith("Ore") else None]
    for c in cands:
        if c and (valid is None or c in valid):
            return c
    return None


def load_mmt_labels() -> dict[str, str]:
    """MMT 全部 Def 的 defName -> 中文 label。"""
    labels: dict[str, str] = {}
    for path in sorted(MMT_DEFS.glob("**/*.xml")):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError:
            continue
        for node in root.iter():
            name = node.findtext("defName")
            label = node.findtext("label")
            if name and label:
                labels[name.strip()] = label.strip()
    return labels


def load_hsk_zh() -> dict[str, str]:
    """Core_SK 中文 DefInjected：标签 `XXX.label` -> defName XXX -> 中文。"""
    out: dict[str, str] = {}
    if not HSK_ZH.is_dir():
        return out
    for path in sorted(HSK_ZH.rglob("*.xml")):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError:
            continue
        for node in root:
            if isinstance(node.tag, str) and node.text and node.tag.endswith(".label"):
                out[node.tag[: -len(".label")]] = node.text.strip()
    return out


class Namer:
    """MMT 优先 label、HSK 优先中文翻译、回落英文 label/defName 的命名器。"""

    def __init__(self, mmt_labels: dict[str, str], hsk_zh: dict[str, str], hsk_labels: dict[str, str] | None = None):
        self.mmt_labels = mmt_labels
        self.hsk_zh = hsk_zh
        self.hsk_labels = hsk_labels or {}

    def mmt(self, def_name: str) -> str:
        return self.mmt_labels.get(def_name) or EXTRA_ZH.get(def_name) or def_name

    def hsk(self, def_name: str) -> str:
        return self.hsk_zh.get(def_name) or EXTRA_ZH.get(def_name) or self.hsk_labels.get(def_name) or def_name


def load_vanilla_thing_names() -> set[str]:
    """原版+全 DLC 的 ThingDef defName 集合（用于剔除 HSK 对原版 def 的改写/重定义）。"""
    names: set[str] = set()
    if not VANILLA_DATA.is_dir():
        return names
    for module in sorted(VANILLA_DATA.iterdir()):
        defs = module / "Defs"
        if not defs.is_dir():
            continue
        idx = DefIndex()
        idx.add_dir(defs)
        names.update(idx.concrete_def_names())
    return names


def chain_power(chain: list[ET.Element]) -> str | None:
    """沿继承链找 comps 里的 basePowerConsumption（CompProperties_Power / SK IdlePower）。"""
    for node in chain:
        comps = node.find("comps")
        if comps is None:
            continue
        for li in comps.findall("li"):
            bp = li.findtext("basePowerConsumption")
            if bp and bp.strip():
                return bp.strip()
    return None


def stuff_cat_zh(cat: str) -> str:
    return STUFF_CAT_ZH.get(cat, cat)


def cost_text(cost_list: dict[str, str], cost_stuff_count: str | None, stuff_cats: list[str], name_fn) -> str:
    """造价 -> '钢×80 + 机械组件×6 + [金属/木质]×120'。"""
    parts = [f"{name_fn(d)}×{n}" for d, n in cost_list.items()]
    if cost_stuff_count:
        cats = "/".join(stuff_cat_zh(c) for c in stuff_cats) or "材料"
        parts.append(f"[{cats}]×{cost_stuff_count}")
    return " + ".join(parts) if parts else "—"


# ---------- 配方解析（RecipeDef，继承链合并后调用） ----------

def recipe_ingredients(rnode: ET.Element) -> list[tuple[list[str], str]]:
    """配料 -> [(候选 defName/类别 列表, 数量)]。类别原样带 [] 前缀由调用方决定。"""
    out: list[tuple[list[str], str]] = []
    for li in rnode.findall("./ingredients/li"):
        count = (li.findtext("count") or "?").strip()
        names = [td.text.strip() for td in li.findall("./filter/thingDefs/li") if td.text]
        names += [f"[{c.text.strip()}]" for c in li.findall("./filter/categories/li") if c.text]
        out.append((names, count))
    return out


def recipe_products(rnode: ET.Element) -> dict[str, str]:
    return kv_pairs(rnode.find("products"))


def recipe_users(rnode: ET.Element) -> list[str]:
    """配方指定的可用工作台（recipeUsers/li）。"""
    return li_texts(rnode.find("recipeUsers"))


def recipe_skills(rnode: ET.Element) -> str:
    sk = kv_pairs(rnode.find("skillRequirements"))
    return ",".join(f"{EXTRA_ZH.get(k, k)}≥{v}" for k, v in sk.items())


def fmt_ingredients(ings: list[tuple[list[str], str]], name_fn) -> str:
    parts = []
    for names, count in ings:
        shown = "/".join(
            f"[{stuff_cat_zh(n[1:-1])}]" if n.startswith("[") else name_fn(n) for n in names
        )
        parts.append(f"{shown or '?'}({count})")
    return " + ".join(parts) if parts else "—"


def fmt_recipe(rnode: ET.Element, name_fn, research_fn=None) -> str:
    """'配料(数量) -> 产物×数量 ｜ 工作量=w ｜ 研究:… ｜ 技能:…' 单行摘要。"""
    ings = recipe_ingredients(rnode)
    prods = recipe_products(rnode)
    out_txt = " + ".join(f"{name_fn(d)}×{n}" for d, n in prods.items()) or "—"
    parts = [f"{fmt_ingredients(ings, name_fn)} -> {out_txt}"]
    work = rnode.findtext("workAmount")
    if work:
        parts.append(f"工作量={work.strip()}")
    res = rnode.findtext("researchPrerequisite")
    if res:
        res = res.strip()
        parts.append(f"研究:{research_fn(res) if research_fn else res}")
    skills = recipe_skills(rnode)
    if skills:
        parts.append(f"技能:{skills}")
    return " ｜ ".join(parts)
