#!/usr/bin/env python3
"""myModeTest Mod 静态验证工具。"""
from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"

BASE_FILES = [
    "About/About.xml",
    "LoadFolders.xml",
    ".gitignore",
    "README.md",
    "MyModeTest.sln",
    "Source/MyModeTest/MyModeTest.csproj",
    "Source/MyModeTest/MyModeTestMod.cs",
]

RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml"
CONCRETE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml"
CONCRETE_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml"
CONCRETE_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml"
CONCRETE_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml"
WOOD_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml"
WOOD_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml"
WOOD_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Wood.xml"
WOOD_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wood.xml"
METAL_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml"
METAL_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml"
METAL_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Metal.xml"
METAL_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml"
PETROCHEM_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml"
PETROCHEM_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml"
PETROCHEM_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Petrochem.xml"
PETROCHEM_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Petrochem.xml"

WOOD_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_a.png",
    "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_b.png",
    "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_c.png",
    "1.7/Textures/Things/Item/MMT/Kindling.png",
    "1.7/Textures/Things/Item/MMT/Charcoal.png",
]

WOOD_BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_east.png",
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_eastm.png",
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_north.png",
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_northm.png",
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_south.png",
    "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_southm.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_east.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_eastm.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_north.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_northm.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_south.png",
    "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_southm.png",
]

METAL_ITEM_TEXTURES = [
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}_{suffix}.png" for ore in ("IronOre", "CopperOre", "TinOre") for suffix in ("a", "b", "c")],
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_{suffix}.png" for suffix in ("a", "b", "c")],
]

METAL_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

PETROCHEM_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Petro/{name}.png"
    for name in ("CrudeOil", "Polymers", "Plastic", "Rubber", "SyntheticFibers", "Carbon")
]

PETROCHEM_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_{side}.png"
    for side in ("east", "north", "south")
]

CONCRETE_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/Concrete.png",
    "1.7/Textures/Things/Item/MMT/ReinforcedConcrete.png",
]

CONCRETE_BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_east.png",
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_eastm.png",
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_north.png",
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_northm.png",
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_south.png",
    "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_southm.png",
]

ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/CrushedStone.png",
    "1.7/Textures/Things/Item/MMT/Sand.png",
    "1.7/Textures/Things/Item/MMT/GlassBatch.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_a.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_b.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_c.png",
]

BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_east.png",
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_north.png",
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_south.png",
]

RESOURCE_EXPECTATIONS = {
    "MMT_CrushedStone": {
        "label": "碎石",
        "value": "0.3",
        "stack": "900",
        "tex": "Things/Item/MMT/CrushedStone",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
    },
    "MMT_Sand": {
        "label": "砂",
        "value": "0.3",
        "stack": "900",
        "tex": "Things/Item/MMT/Sand",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
    },
    "MMT_GlassBatch": {
        "label": "玻璃配合料",
        "value": "1.2",
        "stack": "300",
        "tex": "Things/Item/MMT/GlassBatch",
        "graphic": "Graphic_Single",
        "tech": "Industrial",
    },
    "MMT_Glass": {
        "label": "玻璃",
        "value": "5.0",
        "stack": "150",
        "tex": "Things/Item/MMT/Glass",
        "graphic": "Graphic_StackCount",
        "tech": "Industrial",
    },
}

BUILDING_RECIPES = ["MMT_MakeGlassBatch", "MMT_MakeGlass"]

RECIPE_EXPECTATIONS = {
    "MMT_MakeCrushedStone_Hand": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneBlocks",
        "input_count": "20",
        "output": "MMT_CrushedStone",
        "output_count": "20",
        "users": ["TableStonecutter"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Electric": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneBlocks",
        "input_count": "40",
        "output": "MMT_CrushedStone",
        "output_count": "40",
        "users": ["TableMachining"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Hand_Chunk": {
        "work": "2400",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_CrushedStone",
        "output_count": "32",
        "users": ["TableStonecutter"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Electric_Chunk": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_CrushedStone",
        "output_count": "32",
        "users": ["TableMachining"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Hand": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_CrushedStone",
        "input_count": "10",
        "output": "MMT_Sand",
        "output_count": "25",
        "users": ["TableStonecutter"],
        "research": "Stonecutting",
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Electric": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_CrushedStone",
        "input_count": "20",
        "output": "MMT_Sand",
        "output_count": "50",
        "users": ["TableMachining"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Hand_Chunk": {
        "work": "3000",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_Sand",
        "output_count": "80",
        "users": ["TableStonecutter"],
        "research": "Stonecutting",
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Electric_Chunk": {
        "work": "1500",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_Sand",
        "output_count": "80",
        "users": ["TableMachining"],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeGlassBatch": {
        "work": "900",
        "input_kind": "thingDefs",
        "input": "MMT_Sand",
        "input_count": "75",
        "output": "MMT_GlassBatch",
        "output_count": "25",
        "users": [],
        "research": "MMT_Glassworking",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
    "MMT_MakeGlass": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_GlassBatch",
        "input_count": "20",
        "output": "MMT_Glass",
        "output_count": "10",
        "users": [],
        "research": "MMT_Glassworking",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
}

VANILLA_CATEGORIES = {"StoneBlocks", "StoneChunks", "Metallic"}
VANILLA_WORKTABLES = {"TableStonecutter", "TableMachining"}
VANILLA_RESEARCH = {"Stonecutting", "Smithing", "BiofuelRefining"}
VANILLA_THINGS = {"ComponentIndustrial", "WoodLog", "Steel"}

CONCRETE_RESOURCE_EXPECTATIONS = {
    "MMT_Concrete": {
        "label": "混凝土",
        "value": "6.0",
        "stack": "150",
        "tex": "Things/Item/MMT/Concrete",
        "graphic": "Graphic_Single",
        "tech": "Industrial",
        "stuff": False,
    },
    "MMT_ReinforcedConcrete": {
        "label": "钢筋混凝土",
        "value": "16.0",
        "stack": "75",
        "tex": "Things/Item/MMT/ReinforcedConcrete",
        "graphic": "Graphic_Single",
        "tech": "Industrial",
        "stuff": True,
    },
}

CONCRETE_BUILDING_RECIPES = ["MMT_MixConcrete_Hand"]

CONCRETE_RECIPE_EXPECTATIONS = {
    "MMT_MixConcrete_Hand": {
        "work": "1200",
        "inputs": [
            ("thingDefs", "MMT_Sand", "45"),
            ("thingDefs", "MMT_CrushedStone", "30"),
        ],
        "output": "MMT_Concrete",
        "output_count": "15",
        "users": [],
        "research": "MMT_ConcreteBasic",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "skill": "3",
        "building_mounted": True,
    },
    "MMT_MakeReinforcedConcrete_Hand": {
        "work": "1600",
        "inputs": [
            ("categories", "Metallic", "10"),
            ("thingDefs", "MMT_Concrete", "10"),
        ],
        "output": "MMT_ReinforcedConcrete",
        "output_count": "8",
        "users": ["TableStonecutter"],
        "research": "MMT_ConcreteBasic",
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
        "skill": "4",
        "building_mounted": False,
    },
}

WOOD_RESOURCE_EXPECTATIONS = {
    "MMT_WoodPlank": {
        "label": "木板",
        "value": "1.5",
        "stack": "300",
        "tex": "Things/Item/MMT/WoodPlank",
        "graphic": "Graphic_StackCount",
        "tech": "Neolithic",
        "stuff": True,
    },
    "MMT_Kindling": {
        "label": "柴火",
        "value": "2.3",
        "stack": "300",
        "tex": "Things/Item/MMT/Kindling",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
        "stuff": False,
    },
    "MMT_Charcoal": {
        "label": "木炭",
        "value": "2.0",
        "stack": "600",
        "tex": "Things/Item/MMT/Charcoal",
        "graphic": "Graphic_Single",
        "tech": "Medieval",
        "stuff": False,
    },
}

SAWMILL_RECIPES = ["MMT_MakeWoodPlanks_Hand", "MMT_MakeKindling_Hand"]
CHARCOAL_KILN_RECIPES = ["MMT_MakeCharcoal_Hand"]

WOOD_RECIPE_EXPECTATIONS = {
    "MMT_MakeWoodPlanks_Hand": {
        "work": "1200",
        "input": "WoodLog",
        "input_count": "10",
        "output": "MMT_WoodPlank",
        "output_count": "15",
        "research": "MMT_Woodworking",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "skill": "3",
        "learn": "0.8",
        "building": "MMT_HandSawmill",
    },
    "MMT_MakeKindling_Hand": {
        "work": "500",
        "input": "WoodLog",
        "input_count": "10",
        "output": "MMT_Kindling",
        "output_count": "20",
        "research": "MMT_Woodworking",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "skill": None,
        "learn": "0.25",
        "building": "MMT_HandSawmill",
    },
    "MMT_MakeCharcoal_Hand": {
        "work": "1600",
        "input": "WoodLog",
        "input_count": "25",
        "output": "MMT_Charcoal",
        "output_count": "20",
        "research": "MMT_CharcoalBurning",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "skill": None,
        "learn": "0.8",
        "building": "MMT_CharcoalKiln",
    },
}

METAL_ORE_EXPECTATIONS = {
    "MMT_IronOre": {"label": "铁矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_IronOre"},
    "MMT_CopperOre": {"label": "铜矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_CopperOre"},
    "MMT_TinOre": {"label": "锡矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_TinOre"},
}

METAL_BAR_EXPECTATIONS = {
    "MMT_CopperBar": {"label": "铜锭", "value": "3.0", "stack": "300", "adjective": "铜制", "hp": "1.4", "flammable": "0.1", "work": "1.5"},
    "MMT_TinBar": {"label": "锡锭", "value": "3.0", "stack": "300", "adjective": "锡制", "hp": "1.3", "flammable": "0", "work": "1.2"},
    "MMT_Bronze": {"label": "青铜", "value": "4.0", "stack": "600", "adjective": "青铜制", "hp": "1.6", "flammable": "0", "work": "0.8"},
}

METAL_FURNACE_RECIPES = [
    "MMT_SmeltCopperBar_Hand",
    "MMT_SmeltTinBar_Hand",
    "MMT_MakeBronze_Hand",
    "MMT_SmeltSteel_Hand",
]

METAL_MINEABLE_EXPECTATIONS = {
    "MMT_MineableIronOre": {"label": "铁矿脉", "yield_thing": "MMT_IronOre", "hp": "1500"},
    "MMT_MineableCopperOre": {"label": "铜矿脉", "yield_thing": "MMT_CopperOre", "hp": "1800"},
    "MMT_MineableTinOre": {"label": "锡矿脉", "yield_thing": "MMT_TinOre", "hp": "1300"},
}

METAL_RECIPE_EXPECTATIONS = {
    "MMT_SmeltCopperBar_Hand": {
        "work": "700",
        "inputs": [("thingDefs", "MMT_CopperOre", "10")],
        "output": "MMT_CopperBar",
        "output_count": "10",
        "skill": "1",
    },
    "MMT_SmeltTinBar_Hand": {
        "work": "600",
        "inputs": [("thingDefs", "MMT_TinOre", "10")],
        "output": "MMT_TinBar",
        "output_count": "10",
        "skill": "1",
    },
    "MMT_MakeBronze_Hand": {
        "work": "1000",
        "inputs": [("thingDefs", "MMT_CopperBar", "20"), ("thingDefs", "MMT_TinBar", "10")],
        "output": "MMT_Bronze",
        "output_count": "30",
        "skill": "2",
    },
    "MMT_SmeltSteel_Hand": {
        "work": "1300",
        "inputs": [("thingDefs", "MMT_IronOre", "15"), ("thingDefs", "MMT_Charcoal", "13")],
        "output": "Steel",
        "output_count": "20",
        "skill": "3",
    },
}

PETROCHEM_RESOURCE_EXPECTATIONS = {
    "MMT_CrudeOil": {"label": "原油", "value": "3.0", "stack": "300", "flammable": "5"},
    "MMT_Polymers": {"label": "聚合物", "value": "1.5", "stack": "600", "flammable": "5"},
    "MMT_Plastic": {"label": "塑料", "value": "9.0", "stack": "300", "flammable": "2"},
    "MMT_Rubber": {"label": "橡胶", "value": "11.0", "stack": "300", "flammable": "2"},
    "MMT_SyntheticFibers": {"label": "合成纤维", "value": "12.0", "stack": "150", "flammable": "2"},
    "MMT_Carbon": {"label": "碳材料", "value": "40.0", "stack": "75", "flammable": "0"},
}

PETROCHEM_TABLE_RECIPES = [
    "MMT_DistillPolymers_Hand",
    "MMT_MakePlastic_Hand",
    "MMT_MakeRubber_Hand",
    "MMT_MakeSyntheticFibers_Hand",
    "MMT_MakeCarbon_Hand",
]

PETROCHEM_RECIPE_EXPECTATIONS = {
    "MMT_DistillPolymers_Hand": {
        "work": "1500",
        "inputs": [("thingDefs", "MMT_CrudeOil", "20")],
        "output": "MMT_Polymers",
        "output_count": "15",
        "skill": "4",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
    "MMT_MakePlastic_Hand": {
        "work": "800",
        "inputs": [("thingDefs", "MMT_Polymers", "30")],
        "output": "MMT_Plastic",
        "output_count": "10",
        "skill": "6",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeRubber_Hand": {
        "work": "800",
        "inputs": [("thingDefs", "MMT_Polymers", "30")],
        "output": "MMT_Rubber",
        "output_count": "10",
        "skill": "6",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeSyntheticFibers_Hand": {
        "work": "1200",
        "inputs": [("thingDefs", "MMT_Polymers", "25")],
        "output": "MMT_SyntheticFibers",
        "output_count": "5",
        "skill": "8",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeCarbon_Hand": {
        "work": "4000",
        "inputs": [
            ("thingDefs", "MMT_Polymers", "25"),
            ("thingDefs", "MMT_Charcoal", "25"),
            ("thingDefs", "MMT_Rubber", "15"),
        ],
        "output": "MMT_Carbon",
        "output_count": "12",
        "skill": "10",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
}


def fail(message: str) -> None:
    raise AssertionError(message)


def require_files(paths: list[str]) -> None:
    missing = [path for path in paths if not (ROOT / path).is_file()]
    if missing:
        fail("缺少文件: " + ", ".join(missing))


def require_dirs(paths: list[str]) -> None:
    missing = [path for path in paths if not (ROOT / path).is_dir()]
    if missing:
        fail("缺少目录: " + ", ".join(missing))


def read_text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def parse_xml(path: str) -> ET.Element:
    data = (ROOT / path).read_bytes()
    declaration = data[:200].decode("ascii", "ignore").lower()
    if 'encoding="utf-8"' not in declaration and "encoding='utf-8'" not in declaration:
        fail(f"{path}: XML 声明必须指定 UTF-8")
    try:
        return ET.fromstring(data)
    except ET.ParseError as exc:
        fail(f"{path}: XML 解析失败: {exc}")


def child_text(node: ET.Element, path: str) -> str:
    value = node.findtext(path)
    if value is None or not value.strip():
        fail(f"节点缺少必填值: {path}")
    return value.strip()


def li_values(node: ET.Element, path: str) -> list[str]:
    container = node.find(path)
    if container is None:
        return []
    return [li.text.strip() for li in container.findall("li") if li.text and li.text.strip()]


def def_map(root: ET.Element) -> dict[str, ET.Element]:
    result: dict[str, ET.Element] = {}
    for node in root:
        if not isinstance(node.tag, str):
            continue
        name = node.findtext("defName")
        if not name:
            continue
        if name in result:
            fail(f"重复 defName: {name}")
        result[name] = node
    return result


def assert_equal(actual: object, expected: object, label: str) -> None:
    if actual != expected:
        fail(f"{label}: 期望 {expected!r}，实际 {actual!r}")


def assert_png(path: str) -> None:
    # 校验 PNG 魔数、IHDR 块类型以及宽高均大于 0
    data = (ROOT / path).read_bytes()
    if len(data) <= len(PNG_SIGNATURE) or not data.startswith(PNG_SIGNATURE):
        fail(f"{path}: 不是有效的 PNG 文件")
    # IHDR 块紧随 8 字节魔数：4 字节长度 + 4 字节类型 + 4 字节宽 + 4 字节高
    ihdr_end = len(PNG_SIGNATURE) + 8 + 8
    if len(data) < ihdr_end:
        fail(f"{path}: PNG 缺少完整的 IHDR 块")
    if data[len(PNG_SIGNATURE) + 4 : len(PNG_SIGNATURE) + 8] != b"IHDR":
        fail(f"{path}: PNG 首个块不是 IHDR")
    width = int.from_bytes(data[len(PNG_SIGNATURE) + 8 : len(PNG_SIGNATURE) + 12], "big")
    height = int.from_bytes(data[len(PNG_SIGNATURE) + 12 : len(PNG_SIGNATURE) + 16], "big")
    if width <= 0 or height <= 0:
        fail(f"{path}: PNG 宽高必须大于 0，实际 {width}x{height}")


def validate_base() -> None:
    require_dirs(["1.7/Assemblies"])
    require_files(BASE_FILES)

    about = parse_xml("About/About.xml")
    assert_equal(child_text(about, "name"), "myModeTest", "About name")
    assert_equal(child_text(about, "author"), "YourName", "About author")
    assert_equal(child_text(about, "packageId"), "YourName.myModeTest", "About packageId")
    assert_equal(li_values(about, "supportedVersions"), ["1.7"], "支持版本")

    load_folders = parse_xml("LoadFolders.xml")
    assert_equal(li_values(load_folders, "v1.7"), ["1.7"], "LoadFolders v1.7")

    csproj = parse_xml("Source/MyModeTest/MyModeTest.csproj")
    assert_equal(child_text(csproj, "./PropertyGroup/TargetFramework"), "net10.0", "TargetFramework")
    assert_equal(child_text(csproj, "./PropertyGroup/AssemblyName"), "MyModeTest", "AssemblyName")
    assert_equal(child_text(csproj, "./PropertyGroup/RootNamespace"), "MyModeTest", "RootNamespace")
    output_path = child_text(csproj, "./PropertyGroup/OutputPath")
    assert_equal(output_path, "$(MSBuildThisFileDirectory)..\\..\\1.7\\Assemblies\\", "OutputPath")

    for include, assembly_file in (("Assembly-CSharp", "Assembly-CSharp.dll"), ("UnityStubs", "UnityStubs.dll")):
        reference = csproj.find(f"./ItemGroup/Reference[@Include='{include}']")
        if reference is None:
            fail(f"csproj 缺少 {include} 引用")
        assert_equal(child_text(reference, "HintPath"), f"$(RimWorldPath)\\{assembly_file}", f"{include} HintPath")
        assert_equal(child_text(reference, "Private"), "false", f"{include} Private")

    target = csproj.find("./Target[@Name='ValidateRimWorldPath']")
    if target is None:
        fail("csproj 缺少 ValidateRimWorldPath 目标")
    errors = target.findall("Error")
    conditions = [error.attrib.get("Condition", "") for error in errors]
    texts = [error.attrib.get("Text", "") for error in errors]
    if not any("Assembly-CSharp.dll" in condition for condition in conditions):
        fail("ValidateRimWorldPath 未检查 Assembly-CSharp.dll")
    if not any("UnityStubs.dll" in condition for condition in conditions):
        fail("ValidateRimWorldPath 未检查 UnityStubs.dll")
    if not any("RimWorldPath" in text for text in texts):
        fail("ValidateRimWorldPath 错误文本未提示 RimWorldPath")

    mod_source = read_text("Source/MyModeTest/MyModeTestMod.cs")
    for expected in ("namespace MyModeTest", "public sealed class MyModeTestMod : Mod", "MyModeTestMod(ModContentPack content)", "Log.Message"):
        if expected not in mod_source:
            fail(f"MyModeTestMod.cs 缺少: {expected}")

    solution = read_text("MyModeTest.sln")
    if "Source\\MyModeTest\\MyModeTest.csproj" not in solution:
        fail("解决方案未包含 MyModeTest.csproj")

    readme = read_text("README.md")
    if "RimWorldPath" not in readme or "dotnet build" not in readme:
        fail("README 缺少 RimWorldPath 编译说明")

    print("base: OK")


def validate_resources() -> None:
    require_files([RESOURCE_XML, *ITEM_TEXTURES])
    root = parse_xml(RESOURCE_XML)
    defs = def_map(root)
    assert_equal(set(defs), set(RESOURCE_EXPECTATIONS), "资源 defName 集合")

    for name, expected in RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")

    for path in ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(RESOURCE_XML):
        fail("资源 XML 不得引用 SK 类型")
    print("resources: OK")


def validate_building() -> None:
    require_files([RESEARCH_XML, BUILDING_XML, *BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_Glassworking"}, "研究 defName 集合")
    research = research_defs["MMT_Glassworking"]
    assert_equal(child_text(research, "label"), "玻璃制造", "研究 label")
    assert_equal(child_text(research, "baseCost"), "300", "研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Medieval", "研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Stonecutting"], "研究前置")

    building_defs = def_map(parse_xml(BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_GlassworksTable"}, "建筑 defName 集合")
    building = building_defs["MMT_GlassworksTable"]
    assert_equal(child_text(building, "label"), "玻璃工坊", "建筑 label")
    assert_equal(child_text(building, "thingClass"), "Building_WorkTable", "建筑 thingClass")
    assert_equal(child_text(building, "./graphicData/texPath"), "Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable", "建筑 texPath")
    assert_equal(child_text(building, "./graphicData/graphicClass"), "Graphic_Multi", "建筑 graphicClass")
    assert_equal(child_text(building, "costStuffCount"), "130", "建筑材料数量")
    assert_equal(li_values(building, "stuffCategories"), ["Metallic"], "建筑材料分类")
    assert_equal(child_text(building, "./costList/ComponentIndustrial"), "10", "工业零部件成本")
    assert_equal(child_text(building, "./statBases/WorkToBuild"), "1600", "建造工作量")
    assert_equal(child_text(building, "./statBases/MaxHitPoints"), "180", "最大耐久")
    assert_equal(child_text(building, "size"), "(3,1)", "建筑尺寸")
    assert_equal(li_values(building, "recipes"), BUILDING_RECIPES, "建筑配方")
    assert_equal(li_values(building, "researchPrerequisites"), ["MMT_Glassworking"], "建筑研究前置")
    assert_equal(child_text(building, "constructionSkillPrerequisite"), "9", "建造技能")

    comp_classes = [li.attrib.get("Class") for li in building.findall("./comps/li")]
    for expected in (
        "CompProperties_Power",
        "CompProperties_Flickable",
        "CompProperties_Breakdownable",
        "CompProperties_Glower",
        "CompProperties_HeatPusher",
    ):
        if expected not in comp_classes:
            fail(f"建筑缺少组件: {expected}")

    power = building.find("./comps/li[@Class='CompProperties_Power']")
    if power is None:
        fail("建筑缺少电力组件")
    assert_equal(child_text(power, "compClass"), "CompPowerTrader", "电力 compClass")
    assert_equal(child_text(power, "basePowerConsumption"), "400", "电力消耗")

    for path in BUILDING_TEXTURES:
        assert_png(path)

    for path in (RESEARCH_XML, BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("building: OK")


def validate_recipes() -> None:
    require_files([RECIPE_XML, RESOURCE_XML, RESEARCH_XML, BUILDING_XML])
    recipe_defs = def_map(parse_xml(RECIPE_XML))
    assert_equal(set(recipe_defs), set(RECIPE_EXPECTATIONS), "配方 defName 集合")

    resource_names = set(def_map(parse_xml(RESOURCE_XML)))
    building_defs = def_map(parse_xml(BUILDING_XML))
    building_names = set(building_defs)
    research_names = set(def_map(parse_xml(RESEARCH_XML)))
    glassworks_recipes = li_values(building_defs["MMT_GlassworksTable"], "recipes")

    for name, expected in RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        # label 必填，child_text 会处理缺失/空值失败
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")

        ingredient = recipe.find("./ingredients/li")
        if ingredient is None:
            fail(f"{name} 缺少输入")
        assert_equal(child_text(ingredient, "count"), expected["input_count"], f"{name} 输入数量")
        filter_node = ingredient.find("filter")
        if filter_node is None:
            fail(f"{name} 缺少输入 filter")
        input_values = li_values(filter_node, expected["input_kind"])
        assert_equal(input_values, [expected["input"]], f"{name} 输入引用")

        fixed_filter = recipe.find("fixedIngredientFilter")
        if fixed_filter is None:
            fail(f"{name} 缺少 fixedIngredientFilter")
        fixed_values = li_values(fixed_filter, expected["input_kind"])
        assert_equal(fixed_values, [expected["input"]], f"{name} 固定输入引用")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        users = li_values(recipe, "recipeUsers")
        assert_equal(users, expected["users"], f"{name} recipeUsers")
        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        if expected["input_kind"] == "categories":
            if expected["input"] not in VANILLA_CATEGORIES:
                fail(f"{name} 使用未登记分类: {expected['input']}")
        elif expected["input"] not in resource_names | VANILLA_THINGS:
            fail(f"{name} 使用未知资源: {expected['input']}")

        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

        for user in users:
            if user not in VANILLA_WORKTABLES | building_names:
                fail(f"{name} 使用未知工作台: {user}")

        if research and research not in VANILLA_RESEARCH | research_names:
            fail(f"{name} 使用未知研究: {research}")

        if name in BUILDING_RECIPES and users:
            fail(f"{name} 应由 MMT_GlassworksTable 的 recipes 列表挂载，不应写 recipeUsers")
        if name in BUILDING_RECIPES and name not in glassworks_recipes:
            fail(f"{name} 未挂载到玻璃工坊")

    if "SK." in read_text(RECIPE_XML):
        fail("配方 XML 不得引用 SK 类型")
    print("recipes: OK")


def validate_concrete_resources() -> None:
    require_files([CONCRETE_RESOURCE_XML, *CONCRETE_ITEM_TEXTURES])
    defs = def_map(parse_xml(CONCRETE_RESOURCE_XML))
    assert_equal(set(defs), set(CONCRETE_RESOURCE_EXPECTATIONS), "混凝土资源 defName 集合")

    for name, expected in CONCRETE_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")

        if expected["stuff"]:
            assert_equal(li_values(node, "./stuffProps/categories"), ["Stony"], f"{name} stuff categories")
            assert_equal(child_text(node, "./stuffProps/stuffAdjective"), "钢筋混凝土", f"{name} stuffAdjective")
            assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), "2.8", f"{name} MaxHitPoints factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Flammability"), "0", f"{name} Flammability factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), "4.5", f"{name} WorkToMake factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), "4.5", f"{name} WorkToBuild factor")
            assert_equal(child_text(node, "terrainAffordanceNeeded"), "Heavy", f"{name} terrain affordance")
        elif node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    for path in CONCRETE_ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(CONCRETE_RESOURCE_XML):
        fail("混凝土资源 XML 不得引用 SK 类型")
    print("concrete-resources: OK")


def validate_concrete_building() -> None:
    require_files([CONCRETE_RESEARCH_XML, CONCRETE_BUILDING_XML, *CONCRETE_BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(CONCRETE_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_ConcreteBasic"}, "混凝土研究 defName 集合")
    research = research_defs["MMT_ConcreteBasic"]
    assert_equal(child_text(research, "label"), "混凝土技术", "混凝土研究 label")
    assert_equal(child_text(research, "baseCost"), "1000", "混凝土研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "混凝土研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Stonecutting"], "混凝土研究前置")
    assert_equal(child_text(research, "researchViewX"), "7.00", "混凝土研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "0.00", "混凝土研究 Y 坐标")

    building_defs = def_map(parse_xml(CONCRETE_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_ConcreteMixer"}, "混凝土建筑 defName 集合")
    building = building_defs["MMT_ConcreteMixer"]
    assert_equal(child_text(building, "label"), "手动混凝土搅拌机", "混凝土建筑 label")
    assert_equal(child_text(building, "thingClass"), "Building_WorkTable", "混凝土建筑 thingClass")
    assert_equal(child_text(building, "./graphicData/texPath"), "Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer", "混凝土建筑 texPath")
    assert_equal(child_text(building, "./graphicData/graphicClass"), "Graphic_Multi", "混凝土建筑 graphicClass")
    assert_equal(li_values(building, "stuffCategories"), ["Metallic"], "混凝土建筑材料分类")
    assert_equal(child_text(building, "costStuffCount"), "135", "混凝土建筑材料数量")
    assert_equal(child_text(building, "./costList/ComponentIndustrial"), "5", "混凝土建筑零部件成本")
    assert_equal(child_text(building, "./statBases/WorkToBuild"), "1600", "混凝土建筑建造工作量")
    assert_equal(child_text(building, "./statBases/MaxHitPoints"), "180", "混凝土建筑耐久")
    assert_equal(child_text(building, "size"), "(2,1)", "混凝土建筑尺寸")
    assert_equal(li_values(building, "recipes"), CONCRETE_BUILDING_RECIPES, "混凝土建筑配方")
    assert_equal(li_values(building, "researchPrerequisites"), ["MMT_ConcreteBasic"], "混凝土建筑研究前置")
    assert_equal(child_text(building, "constructionSkillPrerequisite"), "8", "混凝土建筑建造技能")

    comp_classes = [li.attrib.get("Class") for li in building.findall("./comps/li")]
    if "CompProperties_Power" in comp_classes:
        fail("手动混凝土搅拌机不应包含电力组件")

    for path in CONCRETE_BUILDING_TEXTURES:
        assert_png(path)

    for path in (CONCRETE_RESEARCH_XML, CONCRETE_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("concrete-building: OK")


def validate_concrete_recipes() -> None:
    require_files([
        CONCRETE_RECIPE_XML,
        RESOURCE_XML,
        CONCRETE_RESOURCE_XML,
        CONCRETE_RESEARCH_XML,
        CONCRETE_BUILDING_XML,
    ])
    recipe_defs = def_map(parse_xml(CONCRETE_RECIPE_XML))
    assert_equal(set(recipe_defs), set(CONCRETE_RECIPE_EXPECTATIONS), "混凝土配方 defName 集合")

    resource_names = set(def_map(parse_xml(RESOURCE_XML))) | set(def_map(parse_xml(CONCRETE_RESOURCE_XML)))
    building_defs = def_map(parse_xml(CONCRETE_BUILDING_XML))
    building_names = set(building_defs)
    research_names = set(def_map(parse_xml(CONCRETE_RESEARCH_XML)))
    mixer_recipes = li_values(building_defs["MMT_ConcreteMixer"], "recipes")

    for name, expected in CONCRETE_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        ingredients = recipe.findall("./ingredients/li")
        for ingredient in ingredients:
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            categories = li_values(filter_node, "categories")
            thing_defs = li_values(filter_node, "thingDefs")
            if len(categories) + len(thing_defs) != 1:
                fail(f"{name} 每个输入 filter 必须且只能包含一个引用")
            kind = "categories" if categories else "thingDefs"
            value = categories[0] if categories else thing_defs[0]
            actual_inputs.append((kind, value, child_text(ingredient, "count")))
        assert_equal(actual_inputs, expected["inputs"], f"{name} 输入列表")

        fixed_filter = recipe.find("fixedIngredientFilter")
        if fixed_filter is None:
            fail(f"{name} 缺少 fixedIngredientFilter")
        actual_fixed = []
        for kind in ("categories", "thingDefs"):
            for value in li_values(fixed_filter, kind):
                actual_fixed.append((kind, value))
        expected_fixed = [(kind, value) for kind, value, _ in expected["inputs"]]
        assert_equal(actual_fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        users = li_values(recipe, "recipeUsers")
        assert_equal(users, expected["users"], f"{name} recipeUsers")
        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        for kind, value, _ in expected["inputs"]:
            if kind == "categories":
                if value not in VANILLA_CATEGORIES:
                    fail(f"{name} 使用未登记分类: {value}")
            elif value not in resource_names | VANILLA_THINGS:
                fail(f"{name} 使用未知资源: {value}")

        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")
        for user in users:
            if user not in VANILLA_WORKTABLES | building_names:
                fail(f"{name} 使用未知工作台: {user}")
        if research and research not in VANILLA_RESEARCH | research_names:
            fail(f"{name} 使用未知研究: {research}")

        if expected["building_mounted"]:
            if users:
                fail(f"{name} 应由 MMT_ConcreteMixer 的 recipes 列表挂载，不应写 recipeUsers")
            if name not in mixer_recipes:
                fail(f"{name} 未挂载到手动混凝土搅拌机")

    if "SK." in read_text(CONCRETE_RECIPE_XML):
        fail("混凝土配方 XML 不得引用 SK 类型")
    print("concrete-recipes: OK")


def validate_concrete() -> None:
    validate_concrete_resources()
    validate_concrete_building()
    validate_concrete_recipes()
    print("concrete: OK")


def validate_wood_resources() -> None:
    require_files([WOOD_RESOURCE_XML, *WOOD_ITEM_TEXTURES])
    defs = def_map(parse_xml(WOOD_RESOURCE_XML))
    assert_equal(set(defs), set(WOOD_RESOURCE_EXPECTATIONS), "木材资源 defName 集合")

    for name, expected in WOOD_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")

        if expected["stuff"]:
            assert_equal(li_values(node, "./stuffProps/categories"), ["Woody"], f"{name} stuff categories")
            assert_equal(child_text(node, "./stuffProps/stuffAdjective"), "木制", f"{name} stuffAdjective")
            assert_equal(child_text(node, "./stuffProps/appearance"), "Planks", f"{name} stuff appearance")
            assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), "0.9", f"{name} MaxHitPoints factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Flammability"), "1", f"{name} Flammability factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Beauty"), "1.65", f"{name} Beauty factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), "0.7", f"{name} WorkToMake factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), "0.7", f"{name} WorkToBuild factor")
            assert_equal(child_text(node, "terrainAffordanceNeeded"), "Light", f"{name} terrain affordance")
        elif node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    for path in WOOD_ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(WOOD_RESOURCE_XML):
        fail("木材资源 XML 不得引用 SK 类型")
    print("wood-resources: OK")


def validate_wood_building() -> None:
    require_files([WOOD_RESEARCH_XML, WOOD_BUILDING_XML, *WOOD_BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(WOOD_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_Woodworking", "MMT_CharcoalBurning"}, "木材研究 defName 集合")

    woodworking = research_defs["MMT_Woodworking"]
    assert_equal(child_text(woodworking, "label"), "木工加工", "木工研究 label")
    assert_equal(child_text(woodworking, "baseCost"), "300", "木工研究 baseCost")
    assert_equal(child_text(woodworking, "techLevel"), "Neolithic", "木工研究 techLevel")
    assert_equal(li_values(woodworking, "prerequisites"), [], "木工研究前置")
    assert_equal(child_text(woodworking, "researchViewX"), "0.00", "木工研究 X 坐标")
    assert_equal(child_text(woodworking, "researchViewY"), "1.00", "木工研究 Y 坐标")

    charcoal = research_defs["MMT_CharcoalBurning"]
    assert_equal(child_text(charcoal, "label"), "木炭烧制", "烧炭研究 label")
    assert_equal(child_text(charcoal, "baseCost"), "500", "烧炭研究 baseCost")
    assert_equal(child_text(charcoal, "techLevel"), "Medieval", "烧炭研究 techLevel")
    assert_equal(li_values(charcoal, "prerequisites"), ["MMT_Woodworking"], "烧炭研究前置")
    assert_equal(child_text(charcoal, "researchViewX"), "1.00", "烧炭研究 X 坐标")
    assert_equal(child_text(charcoal, "researchViewY"), "1.00", "烧炭研究 Y 坐标")

    building_defs = def_map(parse_xml(WOOD_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_HandSawmill", "MMT_CharcoalKiln"}, "木材建筑 defName 集合")

    sawmill = building_defs["MMT_HandSawmill"]
    assert_equal(child_text(sawmill, "label"), "手动锯木机", "锯木机 label")
    assert_equal(child_text(sawmill, "thingClass"), "Building_WorkTable", "锯木机 thingClass")
    assert_equal(child_text(sawmill, "./graphicData/texPath"), "Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill", "锯木机 texPath")
    assert_equal(child_text(sawmill, "./graphicData/graphicClass"), "Graphic_Multi", "锯木机 graphicClass")
    assert_equal(li_values(sawmill, "stuffCategories"), ["Woody"], "锯木机材料分类")
    assert_equal(child_text(sawmill, "costStuffCount"), "120", "锯木机材料数量")
    assert_equal(child_text(sawmill, "./statBases/WorkToBuild"), "700", "锯木机建造工作量")
    assert_equal(child_text(sawmill, "./statBases/MaxHitPoints"), "170", "锯木机耐久")
    assert_equal(child_text(sawmill, "size"), "(3,1)", "锯木机尺寸")
    assert_equal(li_values(sawmill, "recipes"), SAWMILL_RECIPES, "锯木机配方")
    assert_equal(li_values(sawmill, "researchPrerequisites"), ["MMT_Woodworking"], "锯木机研究前置")
    assert_equal(child_text(sawmill, "constructionSkillPrerequisite"), "3", "锯木机建造技能")

    kiln = building_defs["MMT_CharcoalKiln"]
    assert_equal(child_text(kiln, "label"), "简易木炭窑", "木炭窑 label")
    assert_equal(child_text(kiln, "thingClass"), "Building_WorkTable", "木炭窑 thingClass")
    assert_equal(child_text(kiln, "./graphicData/texPath"), "Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln", "木炭窑 texPath")
    assert_equal(child_text(kiln, "./graphicData/graphicClass"), "Graphic_Multi", "木炭窑 graphicClass")
    assert_equal(li_values(kiln, "stuffCategories"), ["Stony"], "木炭窑材料分类")
    assert_equal(child_text(kiln, "costStuffCount"), "180", "木炭窑材料数量")
    assert_equal(child_text(kiln, "./statBases/WorkToBuild"), "3000", "木炭窑建造工作量")
    assert_equal(child_text(kiln, "./statBases/MaxHitPoints"), "270", "木炭窑耐久")
    assert_equal(child_text(kiln, "size"), "(3,3)", "木炭窑尺寸")
    assert_equal(li_values(kiln, "recipes"), CHARCOAL_KILN_RECIPES, "木炭窑配方")
    assert_equal(li_values(kiln, "researchPrerequisites"), ["MMT_CharcoalBurning"], "木炭窑研究前置")
    assert_equal(child_text(kiln, "constructionSkillPrerequisite"), "4", "木炭窑建造技能")

    for building_name, building in building_defs.items():
        comp_classes = [li.attrib.get("Class") for li in building.findall("./comps/li")]
        if "CompProperties_Power" in comp_classes:
            fail(f"{building_name} 不应包含电力组件")

    for path in WOOD_BUILDING_TEXTURES:
        assert_png(path)

    for path in (WOOD_RESEARCH_XML, WOOD_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("wood-building: OK")


def validate_wood_recipes() -> None:
    require_files([WOOD_RECIPE_XML, WOOD_RESOURCE_XML, WOOD_RESEARCH_XML, WOOD_BUILDING_XML])
    recipe_defs = def_map(parse_xml(WOOD_RECIPE_XML))
    assert_equal(set(recipe_defs), set(WOOD_RECIPE_EXPECTATIONS), "木材配方 defName 集合")

    resource_names = set(def_map(parse_xml(WOOD_RESOURCE_XML)))
    building_defs = def_map(parse_xml(WOOD_BUILDING_XML))
    research_names = set(def_map(parse_xml(WOOD_RESEARCH_XML)))

    for name, expected in WOOD_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), expected["learn"], f"{name} workSkillLearnFactor")

        skill = recipe.find("./skillRequirements/Crafting")
        if expected["skill"] is None:
            if skill is not None:
                fail(f"{name} 不应要求 Crafting 技能")
        else:
            if skill is None or not (skill.text or "").strip():
                fail(f"{name} 缺少 Crafting 技能要求")
            assert_equal((skill.text or "").strip(), expected["skill"], f"{name} Crafting requirement")

        ingredient = recipe.find("./ingredients/li")
        if ingredient is None:
            fail(f"{name} 缺少输入")
        assert_equal(child_text(ingredient, "count"), expected["input_count"], f"{name} 输入数量")
        input_values = li_values(ingredient, "./filter/thingDefs")
        assert_equal(input_values, [expected["input"]], f"{name} 输入引用")
        assert_equal(li_values(recipe, "./fixedIngredientFilter/thingDefs"), [expected["input"]], f"{name} 固定输入引用")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if expected["input"] not in VANILLA_THINGS:
            fail(f"{name} 使用未知原版输入: {expected['input']}")
        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")
        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载，不应写 recipeUsers")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")
        if research not in research_names:
            fail(f"{name} 使用未知研究: {research}")

        building = building_defs[expected["building"]]
        if name not in li_values(building, "recipes"):
            fail(f"{name} 未挂载到 {expected['building']}")

    if "SK." in read_text(WOOD_RECIPE_XML):
        fail("木材配方 XML 不得引用 SK 类型")
    if "SmithingSpeed" in read_text(WOOD_RECIPE_XML) or "Wooden" in read_text(WOOD_RECIPE_XML):
        fail("木材配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed 或 Wooden 分类")
    print("wood-recipes: OK")


def validate_wood() -> None:
    validate_wood_resources()
    validate_wood_building()
    validate_wood_recipes()
    print("wood: OK")


def validate_metal_resources() -> None:
    require_files([METAL_RESOURCE_XML, *METAL_ITEM_TEXTURES])
    defs = def_map(parse_xml(METAL_RESOURCE_XML))
    assert_equal(set(defs), set(METAL_ORE_EXPECTATIONS) | set(METAL_BAR_EXPECTATIONS), "金属资源 defName 集合")

    for name, expected in METAL_ORE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), "2.0", f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), "600", f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    for name, expected in METAL_BAR_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), "Things/Item/MMT/MMT_Ingot/MMT_Ingot", f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        assert_equal(li_values(node, "./stuffProps/categories"), ["Metallic"], f"{name} stuff categories")
        assert_equal(child_text(node, "./stuffProps/stuffAdjective"), expected["adjective"], f"{name} stuffAdjective")
        assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), expected["hp"], f"{name} MaxHitPoints factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/Flammability"), expected["flammable"], f"{name} Flammability factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), expected["work"], f"{name} WorkToMake factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), expected["work"], f"{name} WorkToBuild factor")

    for path in METAL_ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(METAL_RESOURCE_XML):
        fail("金属资源 XML 不得引用 SK 类型")
    print("metal-resources: OK")


def validate_metal_building() -> None:
    require_files([METAL_RESEARCH_XML, METAL_BUILDING_XML, *METAL_BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(METAL_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_BasicMetalworking"}, "金属研究 defName 集合")
    research = research_defs["MMT_BasicMetalworking"]
    assert_equal(child_text(research, "label"), "基础金属冶炼", "金属研究 label")
    assert_equal(child_text(research, "baseCost"), "800", "金属研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Medieval", "金属研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Smithing"], "金属研究前置")
    assert_equal(child_text(research, "researchViewX"), "10.00", "金属研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "0.00", "金属研究 Y 坐标")

    building_defs = def_map(parse_xml(METAL_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_HandFurnace", *METAL_MINEABLE_EXPECTATIONS}, "金属建筑 defName 集合")

    furnace = building_defs["MMT_HandFurnace"]
    assert_equal(child_text(furnace, "label"), "手动熔炉", "熔炉 label")
    assert_equal(child_text(furnace, "thingClass"), "Building_WorkTable", "熔炉 thingClass")
    assert_equal(child_text(furnace, "./graphicData/texPath"), "Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace", "熔炉 texPath")
    assert_equal(child_text(furnace, "./graphicData/graphicClass"), "Graphic_Multi", "熔炉 graphicClass")
    assert_equal(li_values(furnace, "stuffCategories"), ["Stony"], "熔炉材料分类")
    assert_equal(child_text(furnace, "costStuffCount"), "240", "熔炉材料数量")
    assert_equal(child_text(furnace, "./statBases/WorkToBuild"), "3000", "熔炉建造工作量")
    assert_equal(child_text(furnace, "./statBases/MaxHitPoints"), "280", "熔炉耐久")
    assert_equal(child_text(furnace, "size"), "(3,3)", "熔炉尺寸")
    assert_equal(li_values(furnace, "recipes"), METAL_FURNACE_RECIPES, "熔炉配方")
    assert_equal(li_values(furnace, "researchPrerequisites"), ["MMT_BasicMetalworking"], "熔炉研究前置")
    assert_equal(child_text(furnace, "constructionSkillPrerequisite"), "5", "熔炉建造技能")

    comp_classes = [li.attrib.get("Class") for li in furnace.findall("./comps/li")]
    if "CompProperties_Power" in comp_classes:
        fail("手动熔炉不应包含电力组件")

    for name, expected in METAL_MINEABLE_EXPECTATIONS.items():
        vein = building_defs[name]
        assert_equal(vein.attrib.get("ParentName"), "RockBase", f"{name} 父类")
        assert_equal(child_text(vein, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", f"{name} texPath")
        assert_equal(child_text(vein, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(vein, "./building/isResourceRock"), "true", f"{name} isResourceRock")
        assert_equal(child_text(vein, "./building/mineableThing"), expected["yield_thing"], f"{name} mineableThing")
        assert_equal(child_text(vein, "./building/mineableYield"), "75", f"{name} mineableYield")
        assert_equal(child_text(vein, "./building/veinMineable"), "true", f"{name} veinMineable")

    for path in METAL_BUILDING_TEXTURES:
        assert_png(path)

    for path in (METAL_RESEARCH_XML, METAL_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("metal-building: OK")


def validate_metal_recipes() -> None:
    require_files([METAL_RECIPE_XML, METAL_RESOURCE_XML, WOOD_RESOURCE_XML, METAL_RESEARCH_XML, METAL_BUILDING_XML])
    recipe_defs = def_map(parse_xml(METAL_RECIPE_XML))
    assert_equal(set(recipe_defs), set(METAL_RECIPE_EXPECTATIONS), "金属配方 defName 集合")

    resource_names = set(def_map(parse_xml(METAL_RESOURCE_XML))) | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
    building_defs = def_map(parse_xml(METAL_BUILDING_XML))
    furnace_recipes = li_values(building_defs["MMT_HandFurnace"], "recipes")

    for name, expected in METAL_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "SmeltingSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), "Smelt", f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), "Recipe_Smelt", f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            if len(thing_defs) != 1 or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须且只能包含一个 thingDefs 引用")
            actual_inputs.append(("thingDefs", thing_defs[0], child_text(ingredient, "count")))
        assert_equal(actual_inputs, expected["inputs"], f"{name} 输入列表")

        fixed = li_values(recipe, "./fixedIngredientFilter/thingDefs")
        assert_equal(fixed, [value for _, value, _ in expected["inputs"]], f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_HandFurnace 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in furnace_recipes:
            fail(f"{name} 未挂载到手动熔炉")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, "MMT_BasicMetalworking", f"{name} researchPrerequisite")

        for _, value, _ in expected["inputs"]:
            if value not in resource_names:
                fail(f"{name} 使用未知资源: {value}")
        if expected["output"] not in resource_names | VANILLA_THINGS:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

    if "SK." in read_text(METAL_RECIPE_XML):
        fail("金属配方 XML 不得引用 SK 类型")
    print("metal-recipes: OK")


def validate_metal() -> None:
    validate_metal_resources()
    validate_metal_building()
    validate_metal_recipes()
    print("metal: OK")


def validate_petrochem_resources() -> None:
    require_files([PETROCHEM_RESOURCE_XML, *PETROCHEM_ITEM_TEXTURES])
    defs = def_map(parse_xml(PETROCHEM_RESOURCE_XML))
    assert_equal(set(defs), set(PETROCHEM_RESOURCE_EXPECTATIONS), "石化资源 defName 集合")

    for name, expected in PETROCHEM_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./graphicData/texPath"), f"Things/Item/MMT/MMT_Petro/{name.removeprefix('MMT_')}", f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    for path in PETROCHEM_ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(PETROCHEM_RESOURCE_XML):
        fail("石化资源 XML 不得引用 SK 类型")
    print("petrochem-resources: OK")


def validate_petrochem_building() -> None:
    require_files([PETROCHEM_RESEARCH_XML, PETROCHEM_BUILDING_XML, *PETROCHEM_BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(PETROCHEM_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_BasicPetrochemistry"}, "石化研究 defName 集合")
    research = research_defs["MMT_BasicPetrochemistry"]
    assert_equal(child_text(research, "label"), "基础石化工艺", "石化研究 label")
    assert_equal(child_text(research, "baseCost"), "1200", "石化研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "石化研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["BiofuelRefining"], "石化研究前置")
    assert_equal(child_text(research, "researchViewX"), "13.00", "石化研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "0.00", "石化研究 Y 坐标")

    building_defs = def_map(parse_xml(PETROCHEM_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_HandChemTable", "MMT_MineableOilSeep"}, "石化建筑 defName 集合")

    table = building_defs["MMT_HandChemTable"]
    assert_equal(child_text(table, "label"), "手动化学实验台", "化学台 label")
    assert_equal(child_text(table, "thingClass"), "Building_WorkTable", "化学台 thingClass")
    assert_equal(child_text(table, "./graphicData/texPath"), "Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable", "化学台 texPath")
    assert_equal(child_text(table, "./graphicData/graphicClass"), "Graphic_Multi", "化学台 graphicClass")
    assert_equal(li_values(table, "stuffCategories"), ["Metallic"], "化学台材料分类")
    assert_equal(child_text(table, "costStuffCount"), "130", "化学台材料数量")
    assert_equal(child_text(table, "./costList/ComponentIndustrial"), "5", "化学台零部件成本")
    assert_equal(child_text(table, "./statBases/WorkToBuild"), "2000", "化学台建造工作量")
    assert_equal(child_text(table, "./statBases/MaxHitPoints"), "200", "化学台耐久")
    assert_equal(child_text(table, "size"), "(3,2)", "化学台尺寸")
    assert_equal(li_values(table, "recipes"), PETROCHEM_TABLE_RECIPES, "化学台配方")
    assert_equal(li_values(table, "researchPrerequisites"), ["MMT_BasicPetrochemistry"], "化学台研究前置")
    assert_equal(child_text(table, "constructionSkillPrerequisite"), "6", "化学台建造技能")

    comp_classes = [li.attrib.get("Class") for li in table.findall("./comps/li")]
    if "CompProperties_Power" in comp_classes:
        fail("手动化学实验台不应包含电力组件")

    seep = building_defs["MMT_MineableOilSeep"]
    assert_equal(seep.attrib.get("ParentName"), "RockBase", "油苗父类")
    assert_equal(child_text(seep, "label"), "油苗", "油苗 label")
    assert_equal(child_text(seep, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", "油苗 texPath")
    assert_equal(child_text(seep, "./statBases/MaxHitPoints"), "1500", "油苗耐久")
    assert_equal(child_text(seep, "./building/isResourceRock"), "true", "油苗 isResourceRock")
    assert_equal(child_text(seep, "./building/mineableThing"), "MMT_CrudeOil", "油苗 mineableThing")
    assert_equal(child_text(seep, "./building/mineableYield"), "75", "油苗 mineableYield")
    assert_equal(child_text(seep, "./building/veinMineable"), "true", "油苗 veinMineable")

    for path in PETROCHEM_BUILDING_TEXTURES:
        assert_png(path)

    for path in (PETROCHEM_RESEARCH_XML, PETROCHEM_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("petrochem-building: OK")


def validate_petrochem_recipes() -> None:
    require_files([PETROCHEM_RECIPE_XML, PETROCHEM_RESOURCE_XML, WOOD_RESOURCE_XML, PETROCHEM_RESEARCH_XML, PETROCHEM_BUILDING_XML])
    recipe_defs = def_map(parse_xml(PETROCHEM_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PETROCHEM_RECIPE_EXPECTATIONS), "石化配方 defName 集合")

    resource_names = set(def_map(parse_xml(PETROCHEM_RESOURCE_XML))) | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
    building_defs = def_map(parse_xml(PETROCHEM_BUILDING_XML))
    table_recipes = li_values(building_defs["MMT_HandChemTable"], "recipes")

    for name, expected in PETROCHEM_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            if len(thing_defs) != 1 or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须且只能包含一个 thingDefs 引用")
            actual_inputs.append(("thingDefs", thing_defs[0], child_text(ingredient, "count")))
        assert_equal(actual_inputs, expected["inputs"], f"{name} 输入列表")

        fixed = li_values(recipe, "./fixedIngredientFilter/thingDefs")
        assert_equal(fixed, [value for _, value, _ in expected["inputs"]], f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_HandChemTable 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in table_recipes:
            fail(f"{name} 未挂载到手动化学实验台")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, "MMT_BasicPetrochemistry", f"{name} researchPrerequisite")

        for _, value, _ in expected["inputs"]:
            if value not in resource_names:
                fail(f"{name} 使用未知资源: {value}")
        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

    recipe_text = read_text(PETROCHEM_RECIPE_XML)
    if "SK." in recipe_text:
        fail("石化配方 XML 不得引用 SK 类型")
    if "SmithingSpeed" in recipe_text:
        fail("石化配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed")
    print("petrochem-recipes: OK")


def validate_petrochem() -> None:
    validate_petrochem_resources()
    validate_petrochem_building()
    validate_petrochem_recipes()
    print("petrochem: OK")


def validate_all_xml() -> None:
    paths = [Path("About/About.xml"), Path("LoadFolders.xml")]
    defs_dir = ROOT / "1.7"
    if defs_dir.exists():
        paths.extend(path.relative_to(ROOT) for path in defs_dir.rglob("*.xml"))
    for path in paths:
        parse_xml(path.as_posix())
    print(f"all-xml: OK ({len(paths)} files)")


def validate_all() -> None:
    validate_base()
    validate_resources()
    validate_building()
    validate_recipes()
    validate_concrete()
    validate_wood()
    validate_metal()
    validate_petrochem()
    validate_all_xml()


SUITES = {
    "base": validate_base,
    "resources": validate_resources,
    "building": validate_building,
    "recipes": validate_recipes,
    "concrete-resources": validate_concrete_resources,
    "concrete-building": validate_concrete_building,
    "concrete-recipes": validate_concrete_recipes,
    "concrete": validate_concrete,
    "wood-resources": validate_wood_resources,
    "wood-building": validate_wood_building,
    "wood-recipes": validate_wood_recipes,
    "wood": validate_wood,
    "metal-resources": validate_metal_resources,
    "metal-building": validate_metal_building,
    "metal-recipes": validate_metal_recipes,
    "metal": validate_metal,
    "petrochem-resources": validate_petrochem_resources,
    "petrochem-building": validate_petrochem_building,
    "petrochem-recipes": validate_petrochem_recipes,
    "petrochem": validate_petrochem,
    "all": validate_all,
}


def main() -> int:
    parser = argparse.ArgumentParser(description="验证 myModeTest Mod 的静态文件。")
    parser.add_argument("suite", choices=sorted(SUITES))
    args = parser.parse_args()
    try:
        SUITES[args.suite]()
    except (AssertionError, OSError) as exc:
        print(f"[FAIL] {args.suite}: {exc}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
