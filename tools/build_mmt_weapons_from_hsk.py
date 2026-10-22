#!/usr/bin/env python3
"""从本地 HSK/CE XML 生成 MMT 武器 Def；所有人工覆盖写入 mapping JSON。

流程：读取映射表 -> 建立 HSK/CE/中文翻译索引 -> 按批次转换 -> --write 落盘或 --check 比对。
"""
from __future__ import annotations

import argparse
import copy
import json
import shutil
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MAPPING_PATH = ROOT / "tools/data/mmt_weapon_mapping.json"
DEFS_DIR = ROOT / "1.7/Defs"
TEXTURES_DIR = ROOT / "1.7/Textures"
SOUNDS_DIR = ROOT / "1.7/Sounds"

AUDIO_EXTS = (".ogg", ".wav", ".mp3")

# 原版投射物可直接表达的字段；其余 CE 字段列入人工审查输出
PROJECTILE_COPY_FIELDS = (
    "explosionRadius",
    "explosionChance",
    "explosionDelay",
    "preExplosionSpawnThingDef",
    "preExplosionSpawnChance",
    "postExplosionSpawnThingDef",
    "postExplosionSpawnChance",
    "applyDamageToExplosionCellsNeighbors",
    "flyOverhead",
    "ai_IsIncendiary",
)
# 已明确处理的 CE 字段，不进入人工审查
PROJECTILE_KNOWN_FIELDS = frozenset(
    {
        "damageDef",
        "damageAmountBase",
        "speed",
        "armorPenetrationSharp",
        "armorPenetrationBlunt",
        "dropsCasings",
        *PROJECTILE_COPY_FIELDS,
    }
)

WEAPON_BASE_XML = """<?xml version="1.0" encoding="utf-8"?>
<Defs>
\t<!-- MMT 武器共享基类：以原版 BaseWeapon/BaseGun/BaseMeleeWeapon 为模型，剔除 CE/HSK 组件 -->
\t<ThingDef Name="MMT_BaseWeapon" Abstract="True">
\t\t<thingClass>ThingWithComps</thingClass>
\t\t<category>Item</category>
\t\t<drawerType>MapMeshOnly</drawerType>
\t\t<drawGUIOverlay>true</drawGUIOverlay>
\t\t<altitudeLayer>Item</altitudeLayer>
\t\t<alwaysHaulable>true</alwaysHaulable>
\t\t<tickerType>Never</tickerType>
\t\t<equipmentType>Primary</equipmentType>
\t\t<useHitPoints>true</useHitPoints>
\t\t<pathCost>14</pathCost>
\t\t<selectable>true</selectable>
\t\t<rotatable>false</rotatable>
\t\t<statBases>
\t\t\t<MaxHitPoints>100</MaxHitPoints>
\t\t\t<Flammability>1.0</Flammability>
\t\t\t<DeteriorationRate>2</DeteriorationRate>
\t\t\t<Beauty>-3</Beauty>
\t\t\t<SellPriceFactor>0.20</SellPriceFactor>
\t\t</statBases>
\t\t<comps>
\t\t\t<li Class="CompProperties_Forbiddable"/>
\t\t\t<li>
\t\t\t\t<compClass>CompEquippable</compClass>
\t\t\t</li>
\t\t</comps>
\t\t<graphicData>
\t\t\t<onGroundRandomRotateAngle>35</onGroundRandomRotateAngle>
\t\t</graphicData>
\t</ThingDef>

\t<ThingDef Name="MMT_BaseGun" Abstract="True" ParentName="MMT_BaseWeapon">
\t\t<techLevel>Industrial</techLevel>
\t\t<smeltable>true</smeltable>
\t\t<thingCategories>
\t\t\t<li>WeaponsRanged</li>
\t\t</thingCategories>
\t\t<weaponClasses>
\t\t\t<li>Ranged</li>
\t\t</weaponClasses>
\t\t<statBases>
\t\t\t<Flammability>0.5</Flammability>
\t\t</statBases>
\t\t<comps>
\t\t\t<li>
\t\t\t\t<compClass>CompQuality</compClass>
\t\t\t</li>
\t\t</comps>
\t</ThingDef>

\t<ThingDef Name="MMT_BaseMeleeWeapon" Abstract="True" ParentName="MMT_BaseWeapon">
\t\t<techLevel>Industrial</techLevel>
\t\t<smeltable>true</smeltable>
\t\t<burnableByRecipe>true</burnableByRecipe>
\t\t<thingCategories>
\t\t\t<li>WeaponsMelee</li>
\t\t</thingCategories>
\t\t<weaponClasses>
\t\t\t<li>Melee</li>
\t\t</weaponClasses>
\t\t<comps>
\t\t\t<li>
\t\t\t\t<compClass>CompQuality</compClass>
\t\t\t</li>
\t\t</comps>
\t</ThingDef>

\t<!-- 炮塔枪基类：不作为装备流通，仅被炮塔建筑引用 -->
\t<ThingDef Name="MMT_BaseTurretGun" Abstract="True" ParentName="MMT_BaseGun">
\t\t<tradeability>None</tradeability>
\t\t<generateCommonality>0</generateCommonality>
\t</ThingDef>
</Defs>
"""


@dataclass(frozen=True)
class SourceIndex:
    thing_defs: dict[str, ET.Element]
    recipe_defs: dict[str, ET.Element]
    research_defs: dict[str, ET.Element]
    sound_defs: dict[str, ET.Element]
    strings_zh: dict[str, str]
    vanilla_thing_names: frozenset[str]
    vanilla_asset_paths: frozenset[str]


def load_mapping(path: Path = MAPPING_PATH) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def mmt_name(source_name: str) -> str:
    return source_name if source_name.startswith("MMT_") else f"MMT_{source_name}"


def collect_defs(root: Path, pattern: str, tag: str) -> dict[str, ET.Element]:
    """按 glob 收集指定 tag 的 Def，以 defName/Name 为键。"""
    result: dict[str, ET.Element] = {}
    for path in sorted(root.glob(pattern)):
        for node in ET.parse(path).getroot().iter(tag):
            name = node.findtext("defName") or node.get("Name")
            if name:
                result[name] = node
    return result


def load_chinese_strings(root: Path) -> dict[str, str]:
    """加载 HSK 1.6 中文 DefInjected 文本，键为节点标签路径末段。"""
    strings: dict[str, str] = {}
    for path in sorted(root.rglob("*.xml")):
        for node in ET.parse(path).getroot():
            if isinstance(node.tag, str) and node.text and node.text.strip():
                strings[node.tag] = node.text.strip()
    return strings


def load_vanilla_asset_paths(data_dir: Path) -> frozenset[str]:
    """收集原版+DLC Def 引用的全部 texPath/clipPath/uiIconPath，作为打包资源白名单。"""
    paths: set[str] = set()
    for path in sorted(data_dir.glob("*/Defs/**/*.xml")):
        for node in ET.parse(path).getroot().iter():
            if node.tag in ("texPath", "clipPath", "uiIconPath") and node.text:
                paths.add(node.text.strip())
    return frozenset(paths)


def load_source_index(mapping: dict) -> SourceIndex:
    source = ROOT / mapping["source_root"]
    ce = ROOT / mapping["combat_extended_root"]
    translations = ROOT / mapping["translation_root"]
    data_dir = Path(mapping["vanilla_root"]).parent
    vanilla_names: set[str] = set()
    for module in sorted(data_dir.iterdir()):
        if module.is_dir() and (module / "Defs").is_dir():
            vanilla_names.update(collect_defs(module, "Defs/**/*.xml", "ThingDef"))
    return SourceIndex(
        thing_defs=collect_defs(source, "Defs/**/*.xml", "ThingDef") | collect_defs(ce, "Defs/**/*.xml", "ThingDef"),
        recipe_defs=collect_defs(source, "Defs/**/*.xml", "RecipeDef") | collect_defs(ce, "Defs/**/*.xml", "RecipeDef"),
        research_defs=collect_defs(source, "Defs/ResearchProjectDefs/*.xml", "ResearchProjectDef"),
        sound_defs=collect_defs(source, "Defs/**/*.xml", "SoundDef") | collect_defs(ce, "Defs/**/*.xml", "SoundDef"),
        strings_zh=load_chinese_strings(translations),
        vanilla_thing_names=frozenset(vanilla_names),
        vanilla_asset_paths=load_vanilla_asset_paths(data_dir),
    )


def sound_roots(mapping: dict) -> list[Path]:
    return [ROOT / mapping["source_root"] / "Sounds", ROOT / mapping["combat_extended_root"] / "Sounds"]


def texture_roots(mapping: dict) -> list[Path]:
    return [ROOT / mapping["source_root"] / "Textures", ROOT / mapping["combat_extended_root"] / "Textures"]


def resolve_from_element(index: SourceIndex, element: ET.Element | None, path: str) -> str | None:
    """沿 ParentName 链查找文本字段；element 可不在索引中（如 patch 克隆）。"""
    seen: set[str] = set()
    current = element
    while current is not None:
        found = current.find(path)
        if found is not None and found.text:
            return found.text
        parent = current.get("ParentName")
        if not parent or parent in seen:
            return None
        seen.add(parent)
        current = index.thing_defs.get(parent)
    return None


def resolve_inherited_text(index: SourceIndex, def_name: str, path: str) -> str | None:
    return resolve_from_element(index, index.thing_defs.get(def_name), path)


def is_concrete(node: ET.Element) -> bool:
    return (node.get("Abstract") or "").lower() != "true" and node.findtext("defName") is not None


def batch_weapon_elements(mapping: dict, batch: str) -> list[tuple[str, ET.Element]]:
    """读取批次内所有非抽象武器 ThingDef。"""
    result: list[tuple[str, ET.Element]] = []
    for fname in mapping["batches"][batch]:
        path = ROOT / mapping["source_root"] / "Defs/ThingDefs_Weapons" / fname
        for node in ET.parse(path).getroot().iter("ThingDef"):
            if is_concrete(node):
                result.append((node.findtext("defName"), node))
    return result


def load_recipe_product_clone(def_name: str, mapping: dict | None = None) -> ET.Element | None:
    """从 HSK patch 根的 SK.PatchOperationReplaceExtended 提取完整替换内容为合成 ThingDef。"""
    mapping = mapping or load_mapping()
    xpath = f'Defs/ThingDef[defName="{def_name}"]'
    for patch_root in mapping["patch_roots"]:
        for path in sorted((ROOT / patch_root).rglob("*.xml")):
            root = ET.parse(path).getroot()
            parent_name: str | None = None
            for op in root.iter("Operation"):
                if op.get("Class") == "PatchOperationAttributeSet" and op.findtext("xpath") == xpath and op.findtext("attribute") == "ParentName":
                    parent_name = op.findtext("value")
            for op in root.iter("Operation"):
                if op.get("Class") != "SK.PatchOperationReplaceExtended" or op.findtext("xpath") != xpath:
                    continue
                value = op.find("value")
                if value is None:
                    continue
                clone = ET.Element("ThingDef")
                if parent_name:
                    clone.set("ParentName", parent_name)
                ET.SubElement(clone, "defName").text = def_name
                for child in value:
                    clone.append(copy.deepcopy(child))
                return clone
    return None


def all_source_weapons(mapping: dict, index: SourceIndex) -> list[tuple[str, ET.Element]]:
    """全部批次武器 + 22 个补充克隆，用于核心批次扫描投射物/音效引用。"""
    result: list[tuple[str, ET.Element]] = []
    for batch in mapping["batches"]:
        result.extend(batch_weapon_elements(mapping, batch))
    for name in mapping["recipe_product_clones"]["patch_replace"]:
        clone = load_recipe_product_clone(name, mapping)
        if clone is not None:
            result.append((name, clone))
    for name in mapping["recipe_product_clones"]["combat_extended_defs"]:
        node = index.thing_defs.get(name)
        if node is not None:
            result.append((name, node))
    return result


def collect_referenced(index: SourceIndex, mapping: dict) -> tuple[set[str], set[str]]:
    """扫描全部源武器，收集被引用的投射物与音效名。"""
    projectiles: set[str] = set()
    sounds: set[str] = set()
    for name, element in all_source_weapons(mapping, index):
        projectile = resolve_from_element(index, element, "verbs/li/defaultProjectile")
        if projectile:
            if projectile in index.thing_defs:
                projectiles.add(projectile)
            elif projectile not in index.vanilla_thing_names:
                print(f"[manual_review] {name} 引用的投射物 {projectile} 不在 HSK/CE/原版索引中")
        for tag in ("soundCast", "soundCastTail"):
            sound = resolve_from_element(index, element, f"verbs/li/{tag}")
            if sound:
                sounds.add(sound)
    return projectiles, sounds


def convert_projectile(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """CE 投射物转原版投射物：穿甲 sharp/100 -> armorPenetrationBase，保留爆炸等原版字段。"""
    overrides = mapping.get("projectile_overrides", {}).get(source_name, {})
    out = ET.Element("ThingDef", {"ParentName": "BaseBullet"})
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    tex = resolve_inherited_text(index, source_name, "graphicData/texPath")
    graphic_class = resolve_inherited_text(index, source_name, "graphicData/graphicClass") or "Graphic_Single"
    if tex:
        graphic = ET.SubElement(out, "graphicData")
        ET.SubElement(graphic, "texPath").text = tex
        ET.SubElement(graphic, "graphicClass").text = graphic_class
    projectile = ET.SubElement(out, "projectile")

    def field(name: str, default: str | None = None) -> str | None:
        if name in overrides:
            return str(overrides[name])
        value = resolve_inherited_text(index, source_name, f"projectile/{name}")
        return value if value is not None else default

    ET.SubElement(projectile, "damageDef").text = field("damageDef", "Bullet")
    ET.SubElement(projectile, "damageAmountBase").text = field("damageAmountBase", "1")
    sharp = resolve_inherited_text(index, source_name, "projectile/armorPenetrationSharp") or "0"
    ap = f"{float(sharp) / 100:.2f}".rstrip("0").rstrip(".") or "0"
    ET.SubElement(projectile, "armorPenetrationBase").text = str(overrides.get("armorPenetrationBase", ap))
    ET.SubElement(projectile, "speed").text = field("speed", "70")
    for extra in PROJECTILE_COPY_FIELDS:
        value = field(extra)
        if value is not None:
            ET.SubElement(projectile, extra).text = value
    # 人工审查：输出未识别的 CE 投射物字段
    node = index.thing_defs.get(source_name)
    unknown: set[str] = set()
    accepted_drops = set(mapping.get("manual_review", {}).get("projectile_fields_dropped", []))
    current = node
    seen: set[str] = set()
    while current is not None:
        props = current.find("projectile")
        if props is not None:
            for child in props:
                if isinstance(child.tag, str) and child.tag not in PROJECTILE_KNOWN_FIELDS:
                    unknown.add(child.tag)
        parent = current.get("ParentName")
        if not parent or parent in seen:
            break
        seen.add(parent)
        current = index.thing_defs.get(parent)
    for tag in sorted(unknown - accepted_drops):
        print(f"[manual_review] 投射物 {source_name} 含未转换字段 projectile/{tag}")
    return out


def convert_sound(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element | None:
    """HSK/CE SoundDef 转 MMT SoundDef；fallback 或非 HSK/CE 音效返回 None。"""
    if source_name in mapping["sound_fallbacks"]:
        return None
    source = index.sound_defs.get(source_name)
    if source is None:
        return None
    out = copy.deepcopy(source)
    out.find("defName").text = mmt_name(source_name)
    return out


def copy_texture_file(mapping: dict, index: SourceIndex, tex_path: str) -> Path | None:
    """按 texPath 从 HSK/CE 复制 png 到 1.7/Textures 同路径；原版打包资源则跳过。"""
    for root in texture_roots(mapping):
        candidate = root / f"{tex_path}.png"
        if candidate.is_file():
            dest = TEXTURES_DIR / f"{tex_path}.png"
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(candidate, dest)
            return dest
    if tex_path in index.vanilla_asset_paths:
        return None
    print(f"[manual_review] 贴图缺失: {tex_path}.png")
    return None


def copy_sound_assets(mapping: dict, index: SourceIndex, sound_def: ET.Element) -> list[Path]:
    """复制 SoundDef 引用的音频文件或文件夹到 1.7/Sounds 同路径；原版打包音频跳过。"""
    copied: list[Path] = []
    for grain in sound_def.iter("li"):
        clip = grain.find("clipPath")
        if clip is None or not clip.text:
            continue
        clip_path = clip.text
        if grain.get("Class") == "AudioGrain_Folder":
            for root in sound_roots(mapping):
                folder = root / clip_path
                if folder.is_dir():
                    dest_dir = SOUNDS_DIR / clip_path
                    dest_dir.mkdir(parents=True, exist_ok=True)
                    for file in sorted(folder.iterdir()):
                        if file.suffix.lower() in AUDIO_EXTS:
                            shutil.copyfile(file, dest_dir / file.name)
                            copied.append(dest_dir / file.name)
                    break
            else:
                print(f"[manual_review] 音频文件夹缺失: {clip_path}")
        else:
            for root in sound_roots(mapping):
                for ext in AUDIO_EXTS:
                    candidate = root / f"{clip_path}{ext}"
                    if candidate.is_file():
                        dest = SOUNDS_DIR / f"{clip_path}{ext}"
                        dest.parent.mkdir(parents=True, exist_ok=True)
                        shutil.copyfile(candidate, dest)
                        copied.append(dest)
                        break
                else:
                    continue
                break
            else:
                if clip_path not in index.vanilla_asset_paths:
                    print(f"[manual_review] 音频缺失: {clip_path}")
    return copied


def copy_referenced_assets(index: SourceIndex, mapping: dict) -> list[Path]:
    """复制核心批次生成 Def 引用的投射物贴图与音效音频。"""
    projectiles, sounds = collect_referenced(index, mapping)
    copied: list[Path] = []
    for name in sorted(projectiles):
        tex = resolve_inherited_text(index, name, "graphicData/texPath")
        if tex:
            dest = copy_texture_file(mapping, index, tex)
            if dest:
                copied.append(dest)
    for name in sorted(sounds):
        converted = convert_sound(name, index, mapping)
        if converted is not None:
            copied.extend(copy_sound_assets(mapping, index, converted))
    return copied


def serialize_defs(defs: list[ET.Element], comment: str | None = None) -> str:
    """确定性序列化 Defs 根节点：按 defName 排序、tab 缩进。"""
    root = ET.Element("Defs")
    if comment:
        root.append(ET.Comment(comment))
    root.extend(defs)
    ET.indent(root, space="\t")
    return '<?xml version="1.0" encoding="utf-8"?>\n' + ET.tostring(root, encoding="unicode")


def build_core(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    """核心批次：共享基类 + 全部引用投射物 + 引用音效。"""
    projectiles, sounds = collect_referenced(index, mapping)
    projectile_defs = [convert_projectile(name, index, mapping) for name in sorted(projectiles)]
    sound_defs = []
    for name in sorted(sounds):
        if name in mapping["sound_fallbacks"]:
            continue
        converted = convert_sound(name, index, mapping)
        if converted is not None:
            sound_defs.append(converted)
    return {
        "ThingDefs_Weapons/MMT_Weapons_Base.xml": WEAPON_BASE_XML,
        "ThingDefs_Weapons/MMT_Projectiles.xml": serialize_defs(projectile_defs, "MMT 投射物：由 HSK/CE 弹药数值转换"),
        "SoundDefs/MMT_Sounds_Weapons.xml": serialize_defs(sound_defs, "MMT 武器音效：缺失音频走映射表原版 fallback"),
    }


TECH_LEVEL_ORDER = ["Neolithic", "Medieval", "Industrial", "Spacer", "Ultra", "Archotech"]

# 压缩研究树：23 个新节点（第 24 个 MMT_Rocketry1 已存在于 MMT_Research_TextilesWeapons.xml）
NEW_RESEARCH_NODES = [
    "MMT_MeleeAdvanced", "MMT_MeleeUltra",
    "MMT_BowsBasic",
    "MMT_GrenadesBasic", "MMT_GrenadesAdvanced",
    "MMT_GunsAdvanced", "MMT_RiflesAdvanced", "MMT_RiflesMaster",
    "MMT_GrenadeLaunchers", "MMT_SnipersAdvanced",
    "MMT_DirectedEnergyWeapons1", "MMT_DirectedEnergyWeapons2",
    "MMT_ArchotechWeapons",
    "MMT_SiegeWeapons", "MMT_Turrets1",
    "MMT_HeavyTurrets2", "MMT_HeavyTurrets3",
    "MMT_Artillery2", "MMT_Autoturrets2", "MMT_Autoturrets3", "MMT_Autoturrets4",
    "MMT_Rocketry2", "MMT_Rocketry3",
]

# HSK 非武器前置研究 -> 现有 MMT 基础设施研究
NON_WEAPON_RESEARCH = {
    "Metals_C1": "MMT_BasicMetalworking",
    "Electronics_C2": "MMT_IntegratedCircuitTech",
    "Components_D1": "MMT_AdvancedComponents",
    "Components_D3": "MMT_UltraComponents",
    "Components_E1": "MMT_UltraComponents",
    "Stones_C1": "MMT_ConcreteBasic",
    "Craft_B1": "MMT_Woodworking",
    "Mechanoids": "MMT_Robotics1",
}


def resolve_research_field(index: SourceIndex, name: str, field: str) -> str | None:
    """沿 ResearchProjectDef 的 ParentName 链解析字段。"""
    seen: set[str] = set()
    current = index.research_defs.get(name)
    while current is not None:
        found = current.find(field)
        if found is not None and found.text:
            return found.text
        parent = current.get("ParentName")
        if not parent or parent in seen:
            return None
        seen.add(parent)
        current = index.research_defs.get(parent)
    return None


def build_research_defs(index: SourceIndex, mapping: dict) -> list[ET.Element]:
    """按映射表合并 HSK 武器研究为压缩 MMT 节点：成本取最大、科技取最高、前置取映射后并集。"""
    research_map = mapping["research"]
    known = set(NEW_RESEARCH_NODES) | set(research_map.values()) | set(NON_WEAPON_RESEARCH.values())
    defs: list[ET.Element] = []
    for position, node_name in enumerate(NEW_RESEARCH_NODES):
        sources = [src for src, target in research_map.items() if target == node_name]
        if not sources:
            print(f"[manual_review] 研究节点 {node_name} 没有 HSK 来源")
            continue
        costs = [int(resolve_research_field(index, src, "baseCost") or "0") for src in sources]
        levels = [resolve_research_field(index, src, "techLevel") or "Industrial" for src in sources]
        tech = max(levels, key=lambda lv: TECH_LEVEL_ORDER.index(lv) if lv in TECH_LEVEL_ORDER else 0)
        prerequisites: list[str] = []
        for src in sources:
            node = index.research_defs.get(src)
            if node is None:
                print(f"[manual_review] HSK 研究 {src} 不在索引中")
                continue
            for li in node.findall("./prerequisites/li"):
                if not li.text:
                    continue
                target = research_map.get(li.text, NON_WEAPON_RESEARCH.get(li.text, li.text))
                if target != node_name and target not in prerequisites:
                    prerequisites.append(target)
        for pre in prerequisites:
            if pre not in known:
                print(f"[manual_review] {node_name} 的前置 {pre} 非武器映射外名称，按原版/既有研究直引")
        out = ET.Element("ResearchProjectDef")
        ET.SubElement(out, "defName").text = node_name
        zh_label = index.strings_zh.get(f"{sources[0]}.label")
        zh_desc = index.strings_zh.get(f"{sources[0]}.description")
        ET.SubElement(out, "label").text = zh_label or resolve_research_field(index, sources[0], "label") or node_name
        ET.SubElement(out, "description").text = zh_desc or resolve_research_field(index, sources[0], "description") or ""
        ET.SubElement(out, "baseCost").text = str(max(costs))
        ET.SubElement(out, "techLevel").text = tech
        ET.SubElement(out, "tab").text = "MMT_ResearchTab"
        if prerequisites:
            pre_node = ET.SubElement(out, "prerequisites")
            for pre in prerequisites:
                ET.SubElement(pre_node, "li").text = pre
        # 坐标：武器研究统一放在 Y=6..8 横带，X 每行 8 列间隔 3，避免与既有 MMT 研究重叠
        ET.SubElement(out, "researchViewX").text = f"{(position % 8) * 3:.2f}"
        ET.SubElement(out, "researchViewY").text = f"{6 + position // 8:.2f}"
        defs.append(out)
    return defs


def build_research(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    return {
        "ResearchProjectDefs/MMT_Research_Weapons.xml": serialize_defs(
            build_research_defs(index, mapping), "MMT 压缩武器研究树：由 HSK Research_Weapon.xml 合并"
        ),
    }


BATCH_BUILDERS = {
    "core": build_core,
    "research": build_research,
}


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--batch", required=True, choices=["core", "research", "conventional", "archaic", "futuristic", "turrets", "npc", "all"])
    parser.add_argument("--only", action="append", default=[], help="只处理指定 defName（可重复），用于最小样本验证")
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args(argv)
    if args.check == args.write:
        parser.error("必须且只能指定 --check 或 --write")
    mapping = load_mapping()
    index = load_source_index(mapping)
    batches = list(BATCH_BUILDERS) if args.batch == "all" else [args.batch]
    outputs: dict[str, str] = {}
    for batch in batches:
        builder = BATCH_BUILDERS.get(batch)
        if builder is None:
            parser.error(f"批次 {batch} 尚未实现")
        outputs.update(builder(index, mapping, set(args.only)))
    if args.write:
        for rel, text in sorted(outputs.items()):
            dest = DEFS_DIR / rel
            dest.parent.mkdir(parents=True, exist_ok=True)
            dest.write_text(text, encoding="utf-8", newline="\n")
        if "core" in batches:
            copied = copy_referenced_assets(index, mapping)
            print(f"copied {len(copied)} asset files")
        print(f"wrote {len(outputs)} files")
        return 0
    diffs = []
    for rel, text in sorted(outputs.items()):
        dest = DEFS_DIR / rel
        if not dest.is_file() or dest.read_text(encoding="utf-8") != text:
            diffs.append(rel)
    if diffs:
        for rel in diffs:
            print(f"stale: {rel}")
        return 1
    print("generated files are up to date")
    return 0


if __name__ == "__main__":
    sys.exit(main())
