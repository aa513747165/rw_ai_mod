#!/usr/bin/env python3
"""从本地 HSK/CE XML 生成 MMT 武器 Def；所有人工覆盖写入 mapping JSON。

流程：读取映射表 -> 建立 HSK/CE/中文翻译索引 -> 按批次转换 -> --write 落盘或 --check 比对。
"""
from __future__ import annotations

import argparse
import json
import shutil
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MAPPING_PATH = ROOT / "tools/data/mmt_weapon_mapping.json"


@dataclass(frozen=True)
class SourceIndex:
    thing_defs: dict[str, ET.Element]
    recipe_defs: dict[str, ET.Element]
    research_defs: dict[str, ET.Element]
    sound_defs: dict[str, ET.Element]
    strings_zh: dict[str, str]


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


def load_source_index(mapping: dict) -> SourceIndex:
    source = ROOT / mapping["source_root"]
    ce = ROOT / mapping["combat_extended_root"]
    translations = ROOT / mapping["translation_root"]
    return SourceIndex(
        thing_defs=collect_defs(source, "Defs/**/*.xml", "ThingDef") | collect_defs(ce, "Defs/**/*.xml", "ThingDef"),
        recipe_defs=collect_defs(source, "Defs/**/*.xml", "RecipeDef") | collect_defs(ce, "Defs/**/*.xml", "RecipeDef"),
        research_defs=collect_defs(source, "Defs/ResearchProjectDefs/*.xml", "ResearchProjectDef"),
        sound_defs=collect_defs(source, "Defs/SoundDefs/*.xml", "SoundDef"),
        strings_zh=load_chinese_strings(translations),
    )


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
    load_source_index(mapping)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
