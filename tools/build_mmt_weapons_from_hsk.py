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
        interact = resolve_from_element(index, element, "soundInteract")
        if interact:
            sounds.add(interact)
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
    """按 texPath 从 HSK/CE 复制 png 到 1.7/Textures 同路径；文件夹图形（_a/_b/_c）整目录复制；原版打包资源跳过。"""
    for root in texture_roots(mapping):
        candidate = root / f"{tex_path}.png"
        if candidate.is_file():
            dest = TEXTURES_DIR / f"{tex_path}.png"
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(candidate, dest)
            return dest
        folder = root / tex_path
        if folder.is_dir():
            dest_dir = TEXTURES_DIR / tex_path
            dest_dir.mkdir(parents=True, exist_ok=True)
            last: Path | None = None
            for file in sorted(folder.glob("*.png")):
                shutil.copyfile(file, dest_dir / file.name)
                last = dest_dir / file.name
            return last
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


# ===== 武器与配方转换 =====

# 22 个补充克隆的批次归属（计划固定）
PATCH_GUN_CLONES = [
    "Gun_BoltActionRifle", "Gun_LMG", "Gun_Minigun", "Gun_IncendiaryLauncher",
    "Gun_Revolver", "Gun_Autopistol", "Gun_AssaultRifle", "Gun_PumpShotgun",
    "Gun_ChainShotgun", "Gun_MachinePistol", "Gun_HeavySMG", "Gun_SniperRifle",
]
PATCH_GRENADE_CLONES = ["Weapon_GrenadeMolotov", "Weapon_GrenadeFrag", "Weapon_GrenadeTox", "Weapon_GrenadeEMP"]
CE_GRENADE_CLONES = ["CE_Weapon_GrenadeStickBomb", "CE_Weapon_GrenadeFirefoam", "CE_Weapon_GrenadeFlashbang", "CE_Weapon_GrenadeSmoke"]
CLONE_BATCH = {
    **{name: "conventional" for name in PATCH_GUN_CLONES},
    **{name: "conventional" for name in ["CE_FlareGun"]},
    **{name: "archaic" for name in PATCH_GRENADE_CLONES},
    **{name: "archaic" for name in CE_GRENADE_CLONES},
    "Gun_ChargeRifle": "futuristic",
}

# 原版可识别的武器 statBases 白名单；CE 精度系字段（SightsEfficiency 等）按 spec 丢弃
WEAPON_STAT_WHITELIST = {
    "MarketValue", "Mass", "MaxHitPoints", "Flammability", "DeteriorationRate",
    "RangedWeapon_Cooldown", "Beauty", "SellPriceFactor",
}
CE_REMOVE_STATS = {"SightsEfficiency", "ShotSpread", "SwayFactor", "Bulk"}

# 原版 VerbProperties 白名单字段
VERB_FIELD_WHITELIST = {
    "hasStandardCommand", "defaultProjectile", "warmupTime", "range", "minRange",
    "burstShotCount", "ticksBetweenBurstShots", "soundCast", "soundCastTail",
    "muzzleFlashScale", "forcedMissRadius", "requireLineOfSight", "consumeFuelPerShot",
    "ai_IsBuildingDestroyer", "ai_AvoidFriendlyFireRadius", "onlyManualCast",
    "noiseRadius", "forceNormalTimeSpeed", "label",
}
VERB_CLASS_MAP = {
    "CombatExtended.Verb_ShootCE": "Verb_Shoot",
    "CombatExtended.Verb_ShootCEOneUse": "Verb_LaunchProjectile",
    "CombatExtended.Verb_LaunchProjectileCE": "Verb_LaunchProjectile",
    "CombatExtended.Verb_ShootFlareCE": "Verb_Shoot",
    "CombatExtended.Verb_ShootMortarCE": "Verb_ShootMortar",
    "Verb_Shoot": "Verb_Shoot",
    "Verb_ShootOneUse": "Verb_ShootOneUse",
    "Verb_LaunchProjectile": "Verb_LaunchProjectile",
    "Verb_ShootMortar": "Verb_ShootMortar",
}
# 已确认丢弃的 CE verb 字段（后座/瞄准模式/弹匣/CE 弹道散布等，原版无法表达）
VERB_KNOWN_DROPS = {
    "verbClass", "recoilAmount", "recoilPattern", "ai_UseBurstMode", "ai_AimMode",
    "ammoSet", "ejectCasings", "weaponClass", "circularError", "indirectFirePenalty",
    "ignorePartialLoSBlocker", "rangedFireRulepack",
}

# 原版 Tool 白名单字段
TOOL_FIELD_WHITELIST = {"label", "capacities", "power", "cooldownTime", "chanceFactor", "linkedBodyPartsGroup"}

# 武器级杂项字段直拷白名单
WEAPON_MISC_WHITELIST = {"tradeability", "generateCommonality", "soundInteract", "relicChance", "techLevel", "uiIconScale"}

UNFINISHED_MAP = {"UnfinishedWeapon": "UnfinishedGun", "UnfinishedGun": "UnfinishedGun", "UnfinishedMeleeWeapon": "UnfinishedMeleeWeapon", "UnfinishedTurretGun": "UnfinishedGun", "UnfinishedAmmo": "UnfinishedGun"}

# 原版 graphicClass 白名单；SK/CE 自定义图形类一律回退 Graphic_Single
VANILLA_GRAPHIC_CLASSES = {
    "Graphic_Single", "Graphic_Multi", "Graphic_StackCount", "Graphic_Random",
    "Graphic_RandomRotated", "Graphic_Cluster", "Graphic_Terrain",
}


def sanitize_graphic_data(graphic: ET.Element | None) -> ET.Element | None:
    """剔除 SK/CE 自定义 graphicClass，回退为 Graphic_Single。"""
    if graphic is None:
        return None
    out = copy.deepcopy(graphic)
    cls = out.find("graphicClass")
    if cls is not None and cls.text and cls.text not in VANILLA_GRAPHIC_CLASSES:
        print(f"[manual_review] graphicClass {cls.text} 回退为 Graphic_Single")
        cls.text = "Graphic_Single"
    return out

# 配方批次覆盖：BuildFireExtinguisher 产物在 npc 文件，但属玩家可制造配方，归入常规批次
RECIPE_BATCH_OVERRIDES = {"BuildFireExtinguisher": "conventional"}


def def_chain(index: SourceIndex, element: ET.Element | None) -> list[ET.Element]:
    """返回 [自身, 父, ..., 根] 的继承链。"""
    chain: list[ET.Element] = []
    seen: set[str] = set()
    current = element
    while current is not None:
        chain.append(current)
        parent = current.get("ParentName")
        if not parent or parent in seen:
            break
        seen.add(parent)
        current = index.thing_defs.get(parent)
    return chain


def nearest_node(chain: list[ET.Element], tag: str) -> ET.Element | None:
    """取链上最近的包含指定子节点的节点副本。"""
    for node in chain:
        found = node.find(tag)
        if found is not None:
            return found
    return None


def nearest_text(chain: list[ET.Element], path: str) -> str | None:
    for node in chain:
        found = node.find(path)
        if found is not None and found.text:
            return found.text
    return None


def merge_stat_bases(chain: list[ET.Element], whitelist: set[str]) -> dict[str, str]:
    """合并继承链 statBases（子覆盖父），并按白名单过滤。"""
    merged: dict[str, str] = {}
    for node in reversed(chain):
        stats = node.find("statBases")
        if stats is None:
            continue
        for stat in stats:
            if stat.tag in whitelist and stat.text:
                merged[stat.tag] = stat.text
    return merged


def convert_tools(chain: list[ET.Element]) -> ET.Element | None:
    """ToolCE -> 原版 Tool：保留威力/冷却/部位，穿甲 sharp/blunt 除以 100。"""
    tools = nearest_node(chain, "tools")
    if tools is None:
        return None
    out = ET.Element("tools")
    for li in tools.findall("li"):
        tool = ET.SubElement(out, "li")
        for field in ("label", "power", "cooldownTime", "chanceFactor", "linkedBodyPartsGroup"):
            value = li.findtext(field)
            if value:
                ET.SubElement(tool, field).text = value
        capacities = li.find("capacities")
        if capacities is not None:
            tool.append(copy.deepcopy(capacities))
        sharp = li.findtext("armorPenetrationSharp")
        blunt = li.findtext("armorPenetrationBlunt")
        if sharp or blunt:
            raw = float(sharp or blunt)
            ET.SubElement(tool, "armorPenetration").text = f"{raw / 100:.3f}".rstrip("0").rstrip(".")
        for child in li:
            if isinstance(child.tag, str) and child.tag not in TOOL_FIELD_WHITELIST and child.tag not in ("armorPenetrationSharp", "armorPenetrationBlunt", "id"):
                print(f"[manual_review] Tool 未转换字段 {child.tag}")
    return out


def convert_verbs(chain: list[ET.Element], index: SourceIndex, mapping: dict) -> ET.Element | None:
    """VerbPropertiesCE -> 原版 VerbProperties：白名单字段 + 投射物/音效映射。"""
    verbs = nearest_node(chain, "verbs")
    if verbs is None:
        return None
    fallbacks = mapping["sound_fallbacks"]
    out = ET.Element("verbs")
    for li in verbs.findall("li"):
        verb = ET.SubElement(out, "li")
        raw_class = li.findtext("verbClass") or "CombatExtended.Verb_ShootCE"
        verb_class = VERB_CLASS_MAP.get(raw_class)
        if verb_class is None:
            print(f"[manual_review] 未知 verbClass {raw_class}，按 Verb_Shoot 处理")
            verb_class = "Verb_Shoot"
        ET.SubElement(verb, "verbClass").text = verb_class
        for field in sorted(VERB_FIELD_WHITELIST):
            value = li.findtext(field)
            if not value:
                continue
            if field == "defaultProjectile" and value in index.thing_defs:
                value = mmt_name(value)
            elif field in ("soundCast", "soundCastTail"):
                if value in fallbacks:
                    value = fallbacks[value]
                elif value in index.sound_defs:
                    value = mmt_name(value)
            ET.SubElement(verb, field).text = value
        target_params = li.find("targetParams")
        if target_params is not None:
            verb.append(copy.deepcopy(target_params))
        elif verb_class == "Verb_LaunchProjectile":
            # 手雷类补可瞄准地面参数（原版手雷行为），CE 源通常省略
            tp = ET.SubElement(verb, "targetParams")
            ET.SubElement(tp, "canTargetLocations").text = "true"
        for child in li:
            if isinstance(child.tag, str) and child.tag not in VERB_FIELD_WHITELIST and child.tag not in VERB_KNOWN_DROPS and child.tag != "targetParams":
                print(f"[manual_review] verb 未转换字段 {child.tag}")
    return out


def map_thing_name(name: str, mapping: dict) -> str:
    """配料/产物名称映射：配料表 -> 类别表 -> 原样。"""
    return mapping["ingredient_defs"].get(name) or mapping["ingredient_categories"].get(name) or name


def convert_weapon(source_name: str, batch: str, index: SourceIndex, mapping: dict, base_override: str | None = None) -> ET.Element:
    """单把 HSK/CE 武器转 MMT 原版机制武器。"""
    element = index.thing_defs.get(source_name)
    if element is None:
        element = load_recipe_product_clone(source_name, mapping)
    if element is None:
        raise KeyError(f"找不到武器源定义: {source_name}")
    chain = def_chain(index, element)
    weapon_classes = nearest_text(chain, "weaponClasses/li") or "Ranged"
    if base_override is not None:
        base = base_override
    else:
        base = "MMT_BaseMeleeWeapon" if weapon_classes in ("Melee", "MeleeBlunt", "MeleePiercer") else "MMT_BaseGun"
    out = ET.Element("ThingDef", {"ParentName": base})
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    label = index.strings_zh.get(f"{source_name}.label") or nearest_text(chain, "label") or source_name
    description = index.strings_zh.get(f"{source_name}.description") or nearest_text(chain, "description") or ""
    ET.SubElement(out, "label").text = label
    ET.SubElement(out, "description").text = description
    graphic = sanitize_graphic_data(nearest_node(chain, "graphicData"))
    if graphic is not None:
        out.append(graphic)
    sound_interact = nearest_text(chain, "soundInteract")
    if sound_interact:
        if sound_interact in mapping["sound_fallbacks"]:
            sound_interact = mapping["sound_fallbacks"][sound_interact]
        elif sound_interact in index.sound_defs:
            sound_interact = mmt_name(sound_interact)
        ET.SubElement(out, "soundInteract").text = sound_interact
    tech = nearest_text(chain, "techLevel")
    if tech:
        ET.SubElement(out, "techLevel").text = tech
    stats = merge_stat_bases(chain, WEAPON_STAT_WHITELIST)
    if stats:
        stat_node = ET.SubElement(out, "statBases")
        for key in sorted(stats):
            ET.SubElement(stat_node, key).text = stats[key]
    tags: list[str] = []
    for node in reversed(chain):
        for li in node.findall("./weaponTags/li"):
            if li.text and not li.text.startswith("CE_") and li.text not in tags:
                tags.append(li.text)
    if tags:
        tag_node = ET.SubElement(out, "weaponTags")
        for tag in tags:
            ET.SubElement(tag_node, "li").text = tag
    classes_node = nearest_node(chain, "weaponClasses")
    if classes_node is not None:
        out.append(copy.deepcopy(classes_node))
    verbs = convert_verbs(chain, index, mapping)
    if verbs is not None:
        out.append(verbs)
    tools = convert_tools(chain)
    if tools is not None:
        out.append(tools)
    smelt = nearest_node(chain, "smeltProducts")
    if smelt is not None:
        smelt_out = ET.SubElement(out, "smeltProducts")
        for item in smelt:
            ET.SubElement(smelt_out, map_thing_name(item.tag, mapping)).text = item.text
    for field in ("tradeability", "generateCommonality"):
        value = nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value
    return out


def recipe_chain(index: SourceIndex, element: ET.Element) -> list[ET.Element]:
    chain: list[ET.Element] = []
    seen: set[str] = set()
    current: ET.Element | None = element
    while current is not None:
        chain.append(current)
        parent = current.get("ParentName")
        if not parent or parent in seen:
            break
        seen.add(parent)
        current = index.recipe_defs.get(parent)
    return chain


def convert_filter(filter_node: ET.Element, mapping: dict) -> ET.Element:
    """映射 ingredients/fixedIngredientFilter 的 thingDefs 与 categories。"""
    out = ET.Element("filter") if filter_node.tag == "filter" else ET.Element(filter_node.tag)
    for section in filter_node:
        if section.tag in ("thingDefs", "categories"):
            mapped = ET.SubElement(out, section.tag)
            lookup = mapping["ingredient_defs"] if section.tag == "thingDefs" else mapping["ingredient_categories"]
            for li in section.findall("li"):
                if li.text:
                    ET.SubElement(mapped, "li").text = lookup.get(li.text, li.text)
        else:
            out.append(copy.deepcopy(section))
    return out


def convert_recipe(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """HSK 武器配方转 MMT 配方：配料/工作台/研究/产物全部映射。"""
    element = index.recipe_defs.get(source_name)
    if element is None:
        raise KeyError(f"找不到配方源定义: {source_name}")
    chain = recipe_chain(index, element)
    out = ET.Element("RecipeDef")
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    for field in ("label", "description", "jobString"):
        value = index.strings_zh.get(f"{source_name}.{field}") or nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value.replace("\\n", "\n")
    for field in ("workAmount", "workSpeedStat", "workSkill", "workSkillLearnFactor", "effectWorking", "soundWorking"):
        value = nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value
    skill = nearest_node(chain, "skillRequirements")
    if skill is not None:
        out.append(copy.deepcopy(skill))
    unfinished = nearest_text(chain, "unfinishedThingDef")
    if unfinished:
        ET.SubElement(out, "unfinishedThingDef").text = UNFINISHED_MAP.get(unfinished, "UnfinishedGun")
    ingredients = element.find("ingredients")
    if ingredients is not None:
        ing_out = ET.SubElement(out, "ingredients")
        for li in ingredients.findall("li"):
            item = ET.SubElement(ing_out, "li")
            filter_node = li.find("filter")
            if filter_node is not None:
                item.append(convert_filter(filter_node, mapping))
            count = li.findtext("count")
            if count:
                ET.SubElement(item, "count").text = count
    fixed = element.find("fixedIngredientFilter")
    if fixed is not None:
        converted = convert_filter(fixed, mapping)
        converted.tag = "fixedIngredientFilter"
        out.append(converted)
    products = element.find("products")
    if products is not None:
        prod_out = ET.SubElement(out, "products")
        for item in products:
            ET.SubElement(prod_out, mmt_name(item.tag)).text = item.text
    users = element.find("recipeUsers")
    if users is not None:
        users_out = ET.SubElement(out, "recipeUsers")
        for li in users.findall("li"):
            if not li.text:
                continue
            target = mapping["workbenches"].get(li.text)
            if target is None:
                print(f"[manual_review] 配方 {source_name} 的工作台 {li.text} 无映射，已跳过")
                continue
            if users_out.find(f"li[.='{target}']") is None:
                ET.SubElement(users_out, "li").text = target
        if not list(users_out):
            out.remove(users_out)
            print(f"[manual_review] 配方 {source_name} 无可用工作台")
    research = element.findtext("researchPrerequisite")
    if research:
        ET.SubElement(out, "researchPrerequisite").text = mapping["research"].get(research, research)
    return out


def is_projectile_def(index: SourceIndex, element: ET.Element) -> bool:
    """判断批次内 Def 是否为投射物（归入核心投射物文件，不生成武器）。"""
    return resolve_from_element(index, element, "category") == "Projectile"


def weapon_names_for_batch(mapping: dict, batch: str) -> list[str]:
    """批次武器名清单：源文件非投射物 Def + 本批次克隆。"""
    names = [
        name
        for name, element in batch_weapon_elements(mapping, batch)
    ]
    names.extend(name for name, b in CLONE_BATCH.items() if b == batch)
    return names


def build_weapon_batch(batch: str, index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    """生成一个武器批次的 ThingDef 与 RecipeDef 文件。"""
    weapons: list[ET.Element] = []
    included_products: set[str] = set()
    for name, element in batch_weapon_elements(mapping, batch):
        if only and name not in only:
            continue
        if is_projectile_def(index, element):
            continue
        weapons.append(convert_weapon(name, batch, index, mapping))
        included_products.add(name)
    for clone_name, clone_batch in CLONE_BATCH.items():
        if clone_batch != batch or (only and clone_name not in only):
            continue
        weapons.append(convert_weapon(clone_name, batch, index, mapping))
        included_products.add(clone_name)
    weapons.sort(key=lambda node: node.findtext("defName") or "")
    recipes: list[ET.Element] = []
    for recipe_name, element in sorted(index.recipe_defs.items()):
        if not is_concrete(element) or recipe_name in mapping["excluded_recipes"]:
            continue
        override = RECIPE_BATCH_OVERRIDES.get(recipe_name)
        if override is not None:
            if override != batch:
                continue
        else:
            products = element.find("products")
            if products is None or not list(products):
                continue
            product = list(products)[0].tag
            if product not in included_products:
                continue
            if only and product not in only:
                continue
        recipes.append(convert_recipe(recipe_name, index, mapping))
    recipes.sort(key=lambda node: node.findtext("defName") or "")
    # 复制武器贴图
    for node in weapons:
        tex = node.findtext("graphicData/texPath")
        if tex:
            copy_texture_file(mapping, index, tex)
    suffix = batch.capitalize()
    return {
        f"ThingDefs_Weapons/MMT_Weapons_{suffix}.xml": serialize_defs(weapons, f"MMT {batch} 批次武器"),
        f"RecipeDefs/MMT_Recipes_Weapons_{suffix}.xml": serialize_defs(recipes, f"MMT {batch} 批次武器配方"),
    }


def build_conventional(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    return build_weapon_batch("conventional", index, mapping, only)


def build_archaic(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    return build_weapon_batch("archaic", index, mapping, only)


def build_futuristic(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    return build_weapon_batch("futuristic", index, mapping, only)


# ===== 炮塔体系 =====

# ammoSet -> (MMT 弹药物品, MMT 投射物, CE 源弹药 Def, CE 源配方)
TURRET_AMMO_TABLE = {
    "AmmoSet_Balista": ("MMT_Ammo_BalistaBolt", "MMT_Projectile_BalistaBolt", "Ammo_BalistaBolt", "MakeAmmo_BalistaBolt"),
    "AmmoSet_Catapult": ("MMT_Ammo_StoneBall", "MMT_Stone_Ball", "Ammo_StoneBall", "MakeAmmo_StoneBall"),
    "AmmoSet_30x29mmGrenade": ("MMT_Ammo_30x29mmGrenade_HE", "MMT_Bullet_30x29mmGrenade_HE", "Ammo_30x29mmGrenade_HE", "MakeAmmo_30x29mmGrenade_HE"),
    "AmmoSet_120mmCannonShell": ("MMT_Ammo_120mmCannonShell_HEAT", "MMT_Bullet_120mmCannonShell_HEAT", "Ammo_120mmCannonShell_HEAT", "MakeAmmo_120mmCannonShell_HEAT"),
    "AmmoSet_155mmHowitzerShell": ("MMT_Ammo_155mmHowitzerShell_HE", "MMT_Bullet_155mmHowitzerShell_HE", "Ammo_155mmHowitzerShell_HE", "MakeAmmo_155mmHowitzerShell_HE"),
    "AmmoSet_SPG9Rocket": ("MMT_Ammo_SPG9Rocket_Frag", "MMT_Bullet_SPG9Rocket_Frag", "Ammo_SPG9Rocket_Frag", "MakeAmmo_SPG9Rocket_Frag"),
    "AmmoSet_130mmRocketMissile": ("MMT_Ammo_130mmRocketMissile", "MMT_Bullet_130mmRocketMissile", "Ammo_130mmRocketMissile", "MakeAmmo_130mmRocketMissile"),
    "AmmoSet_50BMG": ("MMT_Ammo_50BMG_FMJ", "MMT_Bullet_50BMG_FMJ", "Ammo_50BMG_FMJ", "MakeAmmo_50BMG_FMJ"),
    "AmmoSet_762x51mmNATO": ("MMT_Ammo_762x51mmNATO_FMJ", "MMT_Bullet_762x51mmNATO_FMJ", "Ammo_762x51mmNATO_FMJ", "MakeAmmo_762x51mmNATO_FMJ"),
    "AmmoSet_30x173mm": ("MMT_Ammo_30x173mm_FMJ", "MMT_Bullet_30x173mm_FMJ", "Ammo_30x173mm_FMJ", "MakeAmmo_30x173mm_FMJ"),
    "AmmoSet_20x102mm": ("MMT_Ammo_20x102mm_FMJ", "MMT_Bullet_20x102mm_FMJ", "Ammo_20x102mm_FMJ", "MakeAmmo_20x102mm_FMJ"),
}

# HSK 材料类别 -> 原版/MMT 类别（炮塔建筑用）
TURRET_STUFF_MAP = {"RuggedMetallic": "Metallic", "Woody": "Woody", "Metallic": "Metallic"}

TURRET_BUILDING_STATS = {"MaxHitPoints", "Flammability", "WorkToBuild", "Mass", "Beauty", "ShootingAccuracyTurret"}

# MMT 炮塔建筑基类：以原版 Building_TurretGun + CE TurretBase 为模型
TURRET_BUILDING_BASE_XML = """<ThingDef Name="MMT_TurretBuildingBase" ParentName="BuildingBase" Abstract="True">
\t\t<thingClass>Building_TurretGun</thingClass>
\t\t<drawerType>MapMeshAndRealTime</drawerType>
\t\t<altitudeLayer>Building</altitudeLayer>
\t\t<tickerType>Normal</tickerType>
\t\t<passability>PassThroughOnly</passability>
\t\t<pathCost>50</pathCost>
\t\t<fillPercent>0.85</fillPercent>
\t\t<hasTooltip>true</hasTooltip>
\t\t<terrainAffordanceNeeded>Light</terrainAffordanceNeeded>
\t\t<stealable>false</stealable>
\t\t<rotatable>false</rotatable>
\t\t<minifiedDef>MinifiedThing</minifiedDef>
\t\t<tradeability>None</tradeability>
\t\t<thingCategories>
\t\t\t<li>BuildingsSecurity</li>
\t\t</thingCategories>
\t\t<designationCategory>Security</designationCategory>
\t\t<statBases>
\t\t\t<Flammability>0.7</Flammability>
\t\t\t<Beauty>-60</Beauty>
\t\t</statBases>
\t\t<comps>
\t\t\t<li Class="CompProperties_Forbiddable"/>
\t\t</comps>
\t\t<building>
\t\t\t<ai_combatDangerous>true</ai_combatDangerous>
\t\t</building>
\t\t<placeWorkers>
\t\t\t<li>PlaceWorker_TurretTop</li>
\t\t\t<li>PlaceWorker_ShowTurretRadius</li>
\t\t</placeWorkers>
\t</ThingDef>"""


def turret_gun_ammo(index: SourceIndex, element: ET.Element) -> tuple[str | None, str | None]:
    """取炮塔枪 CE AmmoUser 的 ammoSet 与 magazineSize。"""
    for node in def_chain(index, element):
        for comp in node.findall("./comps/li"):
            if comp.get("Class") == "CombatExtended.CompProperties_AmmoUser":
                return comp.findtext("ammoSet"), comp.findtext("magazineSize")
    return None, None


def convert_turret_gun(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """炮塔枪转换：MMT_BaseTurretGun 基类 + 每发消耗 1 弹药燃料。"""
    out = convert_weapon(source_name, "turrets", index, mapping, base_override="MMT_BaseTurretGun")
    for verb in out.findall("./verbs/li"):
        ET.SubElement(verb, "consumeFuelPerShot").text = "1"
    return out


def convert_turret_building(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """HSK 炮塔建筑转原版机制：refuelable 按发耗弹、人工/自动组件分派。"""
    element = index.thing_defs.get(source_name)
    if element is None:
        raise KeyError(f"找不到炮塔源定义: {source_name}")
    chain = def_chain(index, element)
    out = ET.Element("ThingDef", {"ParentName": "MMT_TurretBuildingBase"})
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    label = index.strings_zh.get(f"{source_name}.label") or nearest_text(chain, "label") or source_name
    description = index.strings_zh.get(f"{source_name}.description") or nearest_text(chain, "description") or ""
    ET.SubElement(out, "label").text = label
    ET.SubElement(out, "description").text = description
    graphic = sanitize_graphic_data(nearest_node(chain, "graphicData"))
    if graphic is not None:
        out.append(graphic)
    icon = nearest_text(chain, "uiIconPath")
    if icon:
        ET.SubElement(out, "uiIconPath").text = icon
    tech = nearest_text(chain, "techLevel")
    if tech:
        ET.SubElement(out, "techLevel").text = tech
    stats = merge_stat_bases(chain, TURRET_BUILDING_STATS)
    if stats:
        stat_node = ET.SubElement(out, "statBases")
        for key in sorted(stats):
            ET.SubElement(stat_node, key).text = stats[key]
    size = nearest_text(chain, "size")
    if size:
        ET.SubElement(out, "size").text = size
    stuff = nearest_node(chain, "stuffCategories")
    if stuff is not None:
        stuff_out = ET.SubElement(out, "stuffCategories")
        for li in stuff.findall("li"):
            if li.text:
                ET.SubElement(stuff_out, "li").text = TURRET_STUFF_MAP.get(li.text, li.text)
    cost_stuff = nearest_text(chain, "costStuffCount")
    if cost_stuff:
        ET.SubElement(out, "costStuffCount").text = cost_stuff
    cost_list = nearest_node(chain, "costList")
    if cost_list is not None:
        cost_out = ET.SubElement(out, "costList")
        for item in cost_list:
            name = item.tag
            if name.endswith("_Crate"):
                name = mmt_name(name)
            else:
                name = map_thing_name(name, mapping)
            ET.SubElement(cost_out, name).text = item.text
    # 节点流转：人工炮塔（TurretMannedBase 系）加 Mannable + 交互格；自动炮塔加电力/闪断/瘫痪
    parent_names = {node.get("ParentName") for node in chain} | {node.get("Name") for node in chain}
    manned = "TurretMannedBase" in parent_names or "TurretMortarBase" in parent_names
    comps = ET.SubElement(out, "comps")
    if manned:
        ET.SubElement(out, "hasInteractionCell").text = "True"
        ET.SubElement(out, "interactionCellOffset").text = "(0,0,-1)"
        mannable = ET.SubElement(comps, "li", {"Class": "CompProperties_Mannable"})
        ET.SubElement(mannable, "manWorkType").text = "Violent"
    else:
        for cls in ("CompProperties_Flickable", "CompProperties_Breakdownable"):
            ET.SubElement(comps, "li", {"Class": cls})
        stunnable = ET.SubElement(comps, "li", {"Class": "CompProperties_Stunnable"})
        affected = ET.SubElement(stunnable, "affectedDamageDefs")
        ET.SubElement(affected, "li").text = "EMP"
        for node in chain:
            for comp in node.findall("./comps/li"):
                if comp.get("Class") == "SK.CompProperties_IdlePower":
                    power = ET.SubElement(comps, "li", {"Class": "CompProperties_Power"})
                    ET.SubElement(power, "compClass").text = "CompPowerTrader"
                    watts = comp.findtext("basePowerConsumption")
                    if watts:
                        ET.SubElement(power, "basePowerConsumption").text = watts
    # 装填耗弹：fuelFilter 指向 MMT 弹药，容量取 ammoSet 内最大弹匣
    gun_name = nearest_text(chain, "building/turretGunDef")
    ammo_set = None
    if gun_name and gun_name in index.thing_defs:
        ammo_set, _ = turret_gun_ammo(index, index.thing_defs[gun_name])
    if ammo_set and ammo_set in TURRET_AMMO_TABLE:
        ammo_name = TURRET_AMMO_TABLE[ammo_set][0]
        capacity = turret_ammo_capacity(index, mapping, ammo_set)
        refuelable = ET.SubElement(comps, "li", {"Class": "CompProperties_Refuelable"})
        ET.SubElement(refuelable, "fuelCapacity").text = str(capacity)
        ET.SubElement(refuelable, "fuelMultiplier").text = "1"
        ET.SubElement(refuelable, "initialFuelPercent").text = "0"
        ET.SubElement(refuelable, "consumeFuelOnlyWhenUsed").text = "true"
        ET.SubElement(refuelable, "showFuelGizmo").text = "true"
        fuel_filter = ET.SubElement(refuelable, "fuelFilter")
        thing_defs = ET.SubElement(fuel_filter, "thingDefs")
        ET.SubElement(thing_defs, "li").text = ammo_name
    elif ammo_set:
        print(f"[manual_review] 炮塔 {source_name} 的 ammoSet {ammo_set} 未映射")
    building = ET.SubElement(out, "building")
    if gun_name:
        ET.SubElement(building, "turretGunDef").text = mmt_name(gun_name)
    top_size = nearest_text(chain, "building/turretTopDrawSize")
    if top_size:
        ET.SubElement(building, "turretTopDrawSize").text = top_size
    research = element.find("researchPrerequisites")
    if research is not None:
        research_out = ET.SubElement(out, "researchPrerequisites")
        for li in research.findall("li"):
            if li.text:
                ET.SubElement(research_out, "li").text = mapping["research"].get(li.text, li.text)
    for field in ("constructionSkillPrerequisite", "specialDisplayRadius"):
        value = nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value
    return out


def turret_ammo_capacity(index: SourceIndex, mapping: dict, ammo_set: str) -> int:
    """同一弹药被多个炮塔使用时取最大弹匣容量。"""
    capacity = 1
    for name, element in batch_weapon_elements(mapping, "turrets"):
        if is_projectile_def(index, element):
            continue
        gun_set, magazine = turret_gun_ammo(index, element)
        if gun_set == ammo_set and magazine:
            capacity = max(capacity, int(float(magazine)))
    return capacity


def build_turret_ammo_item(ammo_set: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """由 CE 弹药物品生成 MMT 炮塔子弹物品：保留价值/质量/贴图，记录对应投射物。"""
    ammo_name, projectile_name, ce_ammo, _ = TURRET_AMMO_TABLE[ammo_set]
    source = index.thing_defs.get(ce_ammo)
    if source is None:
        raise KeyError(f"找不到 CE 弹药源定义: {ce_ammo}")
    chain = def_chain(index, source)
    out = ET.Element("ThingDef")
    ET.SubElement(out, "thingClass").text = "ThingWithComps"
    ET.SubElement(out, "defName").text = ammo_name
    label = index.strings_zh.get(f"{ce_ammo}.label") or nearest_text(chain, "label") or ammo_name
    ET.SubElement(out, "label").text = label
    ET.SubElement(out, "description").text = index.strings_zh.get(f"{ce_ammo}.description") or nearest_text(chain, "description") or ""
    ET.SubElement(out, "category").text = "Item"
    ET.SubElement(out, "altitudeLayer").text = "Item"
    ET.SubElement(out, "useHitPoints").text = "true"
    ET.SubElement(out, "alwaysHaulable").text = "true"
    ET.SubElement(out, "selectable").text = "true"
    ET.SubElement(out, "rotatable").text = "false"
    ET.SubElement(out, "stackLimit").text = "75"
    ET.SubElement(out, "drawGUIOverlay").text = "true"
    graphic = sanitize_graphic_data(nearest_node(chain, "graphicData"))
    if graphic is not None:
        out.append(graphic)
    stats = merge_stat_bases(chain, {"MarketValue", "Mass"})
    if stats:
        stat_node = ET.SubElement(out, "statBases")
        for key in sorted(stats):
            ET.SubElement(stat_node, key).text = stats[key]
    comps = ET.SubElement(out, "comps")
    ET.SubElement(comps, "li", {"Class": "CompProperties_Forbiddable"})
    # 记录弹药对应的投射物（原版字段，供查阅与机制一致性）
    ET.SubElement(out, "projectileWhenLoaded").text = projectile_name
    return out


def convert_crate(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """HSK 炮塔箱转 MMT 箱物品：剔除 HSK 类别与 stuffProps。"""
    element = index.thing_defs.get(source_name)
    if element is None:
        raise KeyError(f"找不到炮塔箱源定义: {source_name}")
    chain = def_chain(index, element)
    out = ET.Element("ThingDef")
    ET.SubElement(out, "thingClass").text = "ThingWithComps"
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    label = index.strings_zh.get(f"{source_name}.label") or nearest_text(chain, "label") or source_name
    ET.SubElement(out, "label").text = label
    ET.SubElement(out, "description").text = index.strings_zh.get(f"{source_name}.description") or nearest_text(chain, "description") or ""
    ET.SubElement(out, "category").text = "Item"
    ET.SubElement(out, "resourceReadoutPriority").text = "Middle"
    ET.SubElement(out, "useHitPoints").text = "true"
    ET.SubElement(out, "selectable").text = "true"
    ET.SubElement(out, "altitudeLayer").text = "Item"
    ET.SubElement(out, "alwaysHaulable").text = "true"
    ET.SubElement(out, "drawGUIOverlay").text = "true"
    ET.SubElement(out, "rotatable").text = "false"
    ET.SubElement(out, "pathCost").text = "15"
    stack = nearest_text(chain, "stackLimit") or "1"
    ET.SubElement(out, "stackLimit").text = stack
    tech = nearest_text(chain, "techLevel")
    if tech:
        ET.SubElement(out, "techLevel").text = tech
    graphic = sanitize_graphic_data(nearest_node(chain, "graphicData"))
    if graphic is not None:
        out.append(graphic)
    for field in ("soundInteract", "soundDrop"):
        value = nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value
    ET.SubElement(out, "tickerType").text = "Normal"
    stats = merge_stat_bases(chain, {"MarketValue", "MaxHitPoints", "Flammability", "DeteriorationRate", "Mass", "SellPriceFactor"})
    if stats:
        stat_node = ET.SubElement(out, "statBases")
        for key in sorted(stats):
            ET.SubElement(stat_node, key).text = stats[key]
    comps = ET.SubElement(out, "comps")
    ET.SubElement(comps, "li", {"Class": "CompProperties_Forbiddable"})
    trade_tags = nearest_node(chain, "tradeTags")
    if trade_tags is not None:
        out.append(copy.deepcopy(trade_tags))
    return out


def turret_related_names(index: SourceIndex, mapping: dict, building_name: str) -> dict[str, str | None]:
    """由炮塔建筑名推导关联的枪/弹药/箱名，供 --only 最小样本过滤。"""
    element = index.thing_defs.get(building_name)
    chain = def_chain(index, element)
    gun = nearest_text(chain, "building/turretGunDef") if chain else None
    ammo_set = None
    if gun and gun in index.thing_defs:
        ammo_set, _ = turret_gun_ammo(index, index.thing_defs[gun])
    crate = None
    for node in chain:
        for item in node.findall("./costList/*"):
            if item.tag.endswith("_Crate"):
                crate = item.tag
    return {"gun": gun, "ammo_set": ammo_set, "crate": crate}


def build_turrets(index: SourceIndex, mapping: dict, only: set[str]) -> dict[str, str]:
    """炮塔批次：炮塔枪、建筑、弹药物品与配方、炮塔箱与箱配方。"""
    building_names = [
        name
        for name, element in sorted(index.thing_defs.items())
        if is_concrete(element)
        and name in _turret_building_names(index, mapping)
    ]
    if only:
        building_names = [name for name in building_names if name in only]
    related = {name: turret_related_names(index, mapping, name) for name in building_names}
    gun_names = sorted({rel["gun"] for rel in related.values() if rel["gun"]})
    ammo_sets = sorted({rel["ammo_set"] for rel in related.values() if rel["ammo_set"]})
    crate_names = sorted({rel["crate"] for rel in related.values() if rel["crate"]})

    guns = [convert_turret_gun(name, index, mapping) for name in gun_names]
    base = ET.fromstring(TURRET_BUILDING_BASE_XML)
    buildings = [base] + [convert_turret_building(name, index, mapping) for name in building_names]
    ammo_items = [build_turret_ammo_item(ammo_set, index, mapping) for ammo_set in ammo_sets]
    crates = [convert_crate(name, index, mapping) for name in crate_names]
    ammo_recipes = []
    for ammo_set in ammo_sets:
        _, _, _, recipe_name = TURRET_AMMO_TABLE[ammo_set]
        ammo_recipes.append(convert_ammo_recipe(recipe_name, ammo_set, index, mapping))
    crate_recipes = []
    for recipe_name, element in sorted(index.recipe_defs.items()):
        if not is_concrete(element):
            continue
        products = element.find("products")
        if products is None or not list(products):
            continue
        if list(products)[0].tag in crate_names:
            crate_recipes.append(convert_recipe(recipe_name, index, mapping))
    crate_recipes.sort(key=lambda node: node.findtext("defName") or "")
    # 复制炮塔相关贴图与图标
    for node in guns + buildings[1:] + ammo_items + crates:
        for tag in ("graphicData/texPath", "uiIconPath"):
            tex = node.findtext(tag)
            if tex:
                copy_texture_file(mapping, index, tex)
    return {
        "ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml": serialize_defs(guns, "MMT 炮塔枪：按发消耗对应 MMT 弹药"),
        "ThingDefs_Buildings/MMT_Buildings_Turrets.xml": serialize_defs(buildings, "MMT 可建造炮塔：refuelable 装填耗弹"),
        "ThingDefs_Items/MMT_Items_TurretAmmo.xml": serialize_defs(ammo_items, "MMT 炮塔弹药"),
        "ThingDefs_Items/MMT_Items_TurretCrates.xml": serialize_defs(crates, "MMT 炮塔箱"),
        "RecipeDefs/MMT_Recipes_TurretAmmo.xml": serialize_defs(ammo_recipes, "MMT 炮塔弹药配方"),
        "RecipeDefs/MMT_Recipes_TurretCrates.xml": serialize_defs(crate_recipes, "MMT 炮塔箱配方"),
    }


def _turret_building_names(index: SourceIndex, mapping: dict) -> set[str]:
    """Buildings_Turrets.xml 中的 16 个可建造炮塔（排除 MinifiedTurret_*）。"""
    path = ROOT / mapping["source_root"] / "Defs/ThingDefs_Buildings/Buildings_Turrets.xml"
    names: set[str] = set()
    for node in ET.parse(path).getroot().iter("ThingDef"):
        name = node.findtext("defName")
        if name and is_concrete(node) and not name.startswith("MinifiedTurret_"):
            names.add(name)
    return names


def convert_ammo_recipe(source_name: str, ammo_set: str, index: SourceIndex, mapping: dict) -> ET.Element:
    """CE 弹药配方转 MMT 炮塔弹药配方：挂 MMT_AmmoBench，产物换 MMT 弹药。"""
    element = index.recipe_defs.get(source_name)
    if element is None:
        raise KeyError(f"找不到弹药配方源定义: {source_name}")
    chain = recipe_chain(index, element)
    ammo_name = TURRET_AMMO_TABLE[ammo_set][0]
    out = ET.Element("RecipeDef")
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    for field in ("label", "description", "jobString"):
        value = index.strings_zh.get(f"{source_name}.{field}") or nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value.replace("\\n", "\n")
    for field in ("workAmount", "workSpeedStat", "workSkill", "workSkillLearnFactor", "effectWorking", "soundWorking"):
        value = nearest_text(chain, field)
        if value:
            ET.SubElement(out, field).text = value
    skill = nearest_node(chain, "skillRequirements")
    if skill is not None:
        out.append(copy.deepcopy(skill))
    ingredients = element.find("ingredients")
    if ingredients is not None:
        ing_out = ET.SubElement(out, "ingredients")
        for li in ingredients.findall("li"):
            item = ET.SubElement(ing_out, "li")
            filter_node = li.find("filter")
            if filter_node is not None:
                item.append(convert_filter(filter_node, mapping))
            count = li.findtext("count")
            if count:
                ET.SubElement(item, "count").text = count
    fixed = element.find("fixedIngredientFilter")
    if fixed is not None:
        converted = convert_filter(fixed, mapping)
        converted.tag = "fixedIngredientFilter"
        out.append(converted)
    products = element.find("products")
    if products is not None:
        prod_out = ET.SubElement(out, "products")
        for item in products:
            ET.SubElement(prod_out, ammo_name).text = item.text
    users = ET.SubElement(out, "recipeUsers")
    ET.SubElement(users, "li").text = "MMT_AmmoBench"
    return out


BATCH_BUILDERS = {
    "core": build_core,
    "research": build_research,
    "conventional": build_conventional,
    "archaic": build_archaic,
    "futuristic": build_futuristic,
    "turrets": build_turrets,
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
