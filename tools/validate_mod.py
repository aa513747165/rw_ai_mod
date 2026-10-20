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
ELECTRONICS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml"
ELECTRONICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml"
ELECTRONICS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml"
ELECTRONICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml"
COAL_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Coal.xml"
COAL_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Coal.xml"
SULFUR_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Sulfur.xml"
SULFUR_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Sulfur.xml"
PRECIOUS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Precious.xml"
PRECIOUS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Precious.xml"
PRECIOUS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Precious.xml"
ELECTRIC_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Electric.xml"
ELECTRIC_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml"
ELECTRIC_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml"
ELECTRIC_WOOD_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricWood.xml"
ELECTRIC_WOOD_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricWood.xml"
ELECTRIC_WOOD_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricWood.xml"
ELECTRIC_PETROCHEM_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricPetrochem.xml"
ELECTRIC_PETROCHEM_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricPetrochem.xml"
ELECTRIC_PETROCHEM_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricPetrochem.xml"
ELECTRIC_ELECTRONICS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricElectronics.xml"
ELECTRIC_ELECTRONICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml"
ELECTRIC_ELECTRONICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricElectronics.xml"

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
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}/MMT_{ore}_{suffix}.png" for ore in ("IronOre", "CopperOre", "TinOre") for suffix in ("a", "b", "c")],
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

ELECTRONICS_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/Silicon.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/ElectronicParts.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/Microchip.png",
]

ELECTRONICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

COAL_ITEM_TEXTURES = ["1.7/Textures/Things/Item/MMT/MMT_Coal.png"]

COAL_RESOURCE_EXPECTATIONS = {
    "MMT_Coal": {"label": "原煤", "value": "2.0", "stack": "600", "flammable": "3", "tex": "Things/Item/MMT/MMT_Coal"},
}

SULFUR_ITEM_TEXTURES = ["1.7/Textures/Things/Item/MMT/MMT_Sulfur.png"]

SULFUR_RESOURCE_EXPECTATIONS = {
    "MMT_Sulfur": {"label": "硫磺", "value": "7", "stack": "300", "flammable": "2", "tex": "Things/Item/MMT/MMT_Sulfur"},
}

PRECIOUS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}/MMT_{ore}_{suffix}.png"
    for ore in ("SilverOre", "GoldOre")
    for suffix in ("a", "b", "c")
]

PRECIOUS_RESOURCE_EXPECTATIONS = {
    "MMT_SilverOre": {"label": "白银", "value": "1.0", "stack": "1800", "tex": "Things/Item/MMT/MMT_Ore/MMT_SilverOre/MMT_SilverOre"},
    "MMT_GoldOre": {"label": "金矿", "value": "25.0", "stack": "300", "tex": "Things/Item/MMT/MMT_Ore/MMT_GoldOre/MMT_GoldOre"},
}

PRECIOUS_MINEABLE_EXPECTATIONS = {
    "MMT_MineableSilverOre": {"label": "银矿", "yield_thing": "MMT_SilverOre", "hp": "1500"},
    "MMT_MineableGoldOre": {"label": "金矿", "yield_thing": "MMT_GoldOre", "hp": "1500"},
}

PRECIOUS_RECIPE_EXPECTATIONS = {
    "MMT_SmeltSilver_Hand": {"work": "750", "ore": "MMT_SilverOre", "ore_count": "14", "output": "Silver", "output_count": "10", "skill": "2"},
    "MMT_SmeltGold_Hand": {"work": "750", "ore": "MMT_GoldOre", "ore_count": "12", "output": "Gold", "output_count": "15", "skill": "3"},
}

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
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_a.png",
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_b.png",
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_c.png",
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
        "label": "沙",
        "value": "0.3",
        "stack": "900",
        "tex": "Things/Item/MMT/Sand",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
    },
    "MMT_GlassBatch": {
        "label": "玻璃生料",
        "value": "1.2",
        "stack": "300",
        "tex": "Things/Item/MMT/GlassBatch",
        "graphic": "Graphic_Single",
        "tech": "Industrial",
    },
    "MMT_Glass": {
        "label": "玻璃块",
        "value": "5.0",
        "stack": "150",
        "tex": "Things/Item/MMT/Glass",
        "graphic": "Graphic_StackCount",
        "tech": "Industrial",
    },
    "MMT_ReinforcedGlass": {
        "label": "强化玻璃块",
        "value": "17.0",
        "stack": "150",
        "tex": "Things/Item/MMT/MMT_ReinforcedGlass",
        "graphic": "Graphic_StackCount",
        "tech": "Industrial",
    },
}

BUILDING_RECIPES = ["MMT_MakeGlassBatch", "MMT_MakeGlass", "MMT_MakeReinforcedGlass"]

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
    "MMT_MakeReinforcedGlass": {
        "work": "1400",
        "inputs": [
            ("thingDefs", "MMT_GlassBatch", "40"),
            ("thingDefs", "MMT_Carbon", "5"),
            ("thingDefs", "MMT_SyntheticFibers", "5"),
        ],
        "output": "MMT_ReinforcedGlass",
        "output_count": "5",
        "users": [],
        "research": "MMT_ReinforcedGlassTech",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
}

VANILLA_CATEGORIES = {"StoneBlocks", "StoneChunks", "Metallic"}
VANILLA_WORKTABLES = {"TableStonecutter", "TableMachining"}
VANILLA_RESEARCH = {"Stonecutting", "Smithing", "BiofuelRefining", "MicroelectronicsBasics"}
VANILLA_THINGS = {"ComponentIndustrial", "ComponentSpacer", "WoodLog", "Steel", "Silver", "Gold", "Plasteel"}

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
    "MMT_IronOre": {"label": "铁矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_IronOre/MMT_IronOre", "value": "2.0", "stack": "600"},
    "MMT_CopperOre": {"label": "铜矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_CopperOre/MMT_CopperOre", "value": "2.0", "stack": "600"},
    "MMT_TinOre": {"label": "锡矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_TinOre/MMT_TinOre", "value": "2.0", "stack": "600"},
    "MMT_Anglesite": {"label": "铅矾矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_Anglesite/MMT_Anglesite", "value": "4", "stack": "300"},
    "MMT_AluminiumOre": {"label": "铝土矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_AluminiumOre/MMT_AluminiumOre", "value": "2", "stack": "600"},
}

METAL_BAR_EXPECTATIONS = {
    "MMT_CopperBar": {"label": "铜", "value": "3.0", "stack": "300", "adjective": "铜制", "hp": "1.4", "flammable": "0.1", "work": "1.5", "category": "MMT_SLDBar"},
    "MMT_TinBar": {"label": "锡", "value": "3.0", "stack": "300", "adjective": "锡制", "hp": "1.3", "flammable": "0", "work": "1.2", "category": "MMT_SLDBar"},
    "MMT_Bronze": {"label": "青铜", "value": "4.0", "stack": "600", "adjective": "青铜制", "hp": "1.6", "flammable": "0", "work": "0.8", "category": "MMT_SLDBar"},
    "MMT_LeadBar": {"label": "铅合金", "value": "6", "stack": "300", "adjective": "铅制", "hp": "1.35", "flammable": "0.1", "work": "1.7", "category": "MMT_HeavyBar"},
    "MMT_AluminiumBar": {"label": "铝合金", "value": "3", "stack": "300", "adjective": "铝制", "hp": "1.4", "flammable": "0", "work": "1.45", "category": "MMT_HCMBar"},
    "MMT_FerrosiliconAlloy": {"label": "斯特合金", "value": "5", "stack": "300", "adjective": "斯特制", "hp": "2.1", "flammable": "0", "work": "1.3", "category": "MMT_SLDBar"},
}

METAL_FURNACE_RECIPES = [
    "MMT_SmeltCopperBar_Hand",
    "MMT_SmeltTinBar_Hand",
    "MMT_MakeBronze_Hand",
    "MMT_SmeltSteel_Hand",
    "MMT_ExtractSilicon_Hand",
    "MMT_SmeltSilver_Hand",
    "MMT_SmeltGold_Hand",
    "MMT_SmeltLeadBar_Hand",
]

METAL_MINEABLE_EXPECTATIONS = {
    "MMT_MineableIronOre": {"label": "铁矿", "yield_thing": "MMT_IronOre", "hp": "1500"},
    "MMT_MineableCopperOre": {"label": "铜矿", "yield_thing": "MMT_CopperOre", "hp": "1800"},
    "MMT_MineableTinOre": {"label": "锡矿", "yield_thing": "MMT_TinOre", "hp": "1300"},
    "MMT_MineableAnglesite": {"label": "铅矾矿脉", "yield_thing": "MMT_Anglesite", "hp": "1600"},
    "MMT_MineableAluminium": {"label": "铝土矿脉", "yield_thing": "MMT_AluminiumOre", "hp": "1600"},
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
        "inputs": [("thingDefs", "MMT_IronOre", "15"), ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "13")],
        "output": "Steel",
        "output_count": "20",
        "skill": "3",
    },
    "MMT_SmeltLeadBar_Hand": {
        "work": "1100",
        "inputs": [("thingDefs", "MMT_Anglesite", "10")],
        "output": "MMT_LeadBar",
        "output_count": "10",
        "skill": "2",
    },
}

PETROCHEM_RESOURCE_EXPECTATIONS = {
    "MMT_CrudeOil": {"label": "原油", "value": "3.0", "stack": "300", "flammable": "5"},
    "MMT_Polymers": {"label": "聚合物", "value": "1.5", "stack": "600", "flammable": "5"},
    "MMT_Plastic": {"label": "塑料", "value": "9.0", "stack": "300", "flammable": "2"},
    "MMT_Rubber": {"label": "合成橡胶", "value": "11.0", "stack": "300", "flammable": "2"},
    "MMT_SyntheticFibers": {"label": "人造纤维", "value": "12.0", "stack": "150", "flammable": "2"},
    "MMT_Carbon": {"label": "碳聚物", "value": "40.0", "stack": "75", "flammable": "0"},
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
        "inputs": [("thingDefs", "MMT_Polymers", "30"), ("thingDefs", "MMT_Sulfur", "3")],
        "output": "MMT_Rubber",
        "output_count": "10",
        "skill": "6",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeSyntheticFibers_Hand": {
        "work": "1200",
        "inputs": [("thingDefs", "MMT_Polymers", "25"), ("thingDefs", "MMT_Sulfur", "3")],
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
            ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "25"),
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

ELECTRONICS_RESOURCE_EXPECTATIONS = {
    "MMT_Silicon": {"label": "硅", "value": "4.0", "stack": "300", "flammable": "0", "tex": "Things/Item/MMT/MMT_Electronics/Silicon", "tech": "Industrial"},
    "MMT_ElectronicParts": {"label": "电子元件", "value": "25.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/ElectronicParts", "tech": "Industrial"},
    "MMT_IntegratedCircuit": {"label": "集成电路", "value": "55.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/IntegratedCircuit", "tech": "Industrial"},
    "MMT_Microchip": {"label": "微芯片集成电路", "value": "100.0", "stack": "25", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/Microchip", "tech": "Spacer"},
}

ELECTRONICS_TABLE_RECIPES = ["MMT_AssembleElectronicParts_Hand", "MMT_AssembleIC_Hand", "MMT_AssembleMicrochip_Hand"]

ELECTRONICS_RECIPE_EXPECTATIONS = {
    "MMT_ExtractSilicon_Hand": {
        "work": "1600",
        "inputs": [("thingDefs", "MMT_Sand", "50")],
        "output": "MMT_Silicon",
        "output_count": "5",
        "skill": "5",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "building": "MMT_HandFurnace",
        "research": "MMT_BasicElectronics",
    },
    "MMT_AssembleElectronicParts_Hand": {
        "work": "2500",
        "inputs": [
            ("thingDefs", "MMT_CopperBar", "15"),
            ("thingDefs", "MMT_Glass", "10"),
            ("thingDefs", "MMT_Silicon", "10"),
        ],
        "output": "MMT_ElectronicParts",
        "output_count": "5",
        "skill": "8",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
        "research": "MMT_BasicElectronics",
    },
    "MMT_AssembleIC_Hand": {
        "work": "3500",
        "inputs": [
            ("thingDefs", "MMT_Silicon", "25"),
            ("thingDefs", "MMT_Plastic", "20"),
            ("thingDefs", "MMT_ElectronicParts", "3"),
            ("thingDefs", "Gold", "2"),
        ],
        "output": "MMT_IntegratedCircuit",
        "output_count": "5",
        "skill": "10",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
        "research": "MMT_IntegratedCircuitTech",
    },
    "MMT_AssembleMicrochip_Hand": {
        "work": "3500",
        "inputs": [
            ("thingDefs", "MMT_IntegratedCircuit", "2"),
            ("thingDefs", "MMT_Carbon", "5"),
            ("thingDefs", "Gold", "5"),
        ],
        "output": "MMT_Microchip",
        "output_count": "1",
        "skill": "12",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
        "research": "MMT_IntegratedCircuitTech",
    },
}


ELECTRIC_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_{side}.png"
    for side in ("east", "north", "south")
]

ELECTRIC_FURNACE_RECIPES = [
    "MMT_SmeltCopperBar_Electric",
    "MMT_SmeltTinBar_Electric",
    "MMT_MakeBronze_Electric",
    "MMT_SmeltSteel_Electric",
    "MMT_SmeltSilver_Electric",
    "MMT_SmeltGold_Electric",
    "MMT_ExtractSilicon_Electric",
    "MMT_SmeltLeadBar_Electric",
    "MMT_SmeltAluminiumBar_Electric",
    "MMT_MakeFerrosiliconAlloy",
    "MMT_MakeMetalCan",
]

ELECTRIC_RECIPE_EXPECTATIONS = {
    "MMT_SmeltCopperBar_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_CopperOre", "20")],
        "output": "MMT_CopperBar",
        "output_count": "20",
        "skill": "4",
    },
    "MMT_SmeltTinBar_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_TinOre", "20")],
        "output": "MMT_TinBar",
        "output_count": "20",
        "skill": "4",
    },
    "MMT_MakeBronze_Electric": {
        "work": "700",
        "inputs": [("thingDefs", "MMT_CopperBar", "20"), ("thingDefs", "MMT_TinBar", "10")],
        "output": "MMT_Bronze",
        "output_count": "30",
        "skill": "4",
    },
    "MMT_SmeltSteel_Electric": {
        "work": "900",
        "inputs": [("thingDefs", "MMT_IronOre", "15"), ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "7")],
        "output": "Steel",
        "output_count": "20",
        "skill": "6",
    },
    "MMT_SmeltSilver_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_SilverOre", "14")],
        "output": "Silver",
        "output_count": "10",
        "skill": "5",
    },
    "MMT_SmeltGold_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_GoldOre", "12")],
        "output": "Gold",
        "output_count": "15",
        "skill": "6",
    },
    "MMT_ExtractSilicon_Electric": {
        "work": "800",
        "inputs": [("thingDefs", "MMT_Sand", "50")],
        "output": "MMT_Silicon",
        "output_count": "5",
        "skill": "5",
    },
    "MMT_SmeltLeadBar_Electric": {
        "work": "1000",
        "inputs": [("thingDefs", "MMT_Anglesite", "20")],
        "output": "MMT_LeadBar",
        "output_count": "20",
        "skill": "6",
    },
    "MMT_SmeltAluminiumBar_Electric": {
        "work": "1200",
        "inputs": [("thingDefs", "MMT_AluminiumOre", "20")],
        "output": "MMT_AluminiumBar",
        "output_count": "20",
        "skill": "7",
    },
    "MMT_MakeFerrosiliconAlloy": {
        "work": "1700",
        "inputs": [("thingDefs", "Steel", "15"), ("thingDefs", "MMT_Silicon", "10"), ("thingDefs", "MMT_AluminiumBar", "5")],
        "output": "MMT_FerrosiliconAlloy",
        "output_count": "30",
        "skill": "8",
    },
}


ELECTRIC_WOOD_BUILDING_TEXTURES = [
    *[f"1.7/Textures/Things/Building/Production/MMT_ElectricSawmill/MMT_ElectricSawmill_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
    *[f"1.7/Textures/Things/Building/Production/MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
]

ELECTRIC_WOOD_BUILDING_EXPECTATIONS = {
    "MMT_ElectricSawmill": {
        "label": "电动锯木台",
        "tex": "Things/Building/Production/MMT_ElectricSawmill/MMT_ElectricSawmill",
        "stuff": "Metallic",
        "stuff_count": "150",
        "components": "10",
        "parts": "5",
        "work": "5000",
        "hp": "170",
        "size": "(3,1)",
        "power": "600",
        "skill": "10",
        "recipes": ["MMT_MakeWoodPlanks_Electric", "MMT_MakeKindling_Electric"],
    },
    "MMT_ElectricCharcoalKiln": {
        "label": "电动木炭窑",
        "tex": "Things/Building/Production/MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln",
        "stuff": "Stony",
        "stuff_count": "180",
        "components": "5",
        "parts": "3",
        "work": "3000",
        "hp": "270",
        "size": "(3,3)",
        "power": "1500",
        "skill": "8",
        "recipes": ["MMT_MakeCharcoal_Electric"],
    },
}

ELECTRIC_WOOD_RECIPE_EXPECTATIONS = {
    "MMT_MakeWoodPlanks_Electric": {
        "work": "1200",
        "input_count": "20",
        "output": "MMT_WoodPlank",
        "output_count": "30",
        "skill": "6",
        "learn": "0.8",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "building": "MMT_ElectricSawmill",
    },
    "MMT_MakeKindling_Electric": {
        "work": "500",
        "input_count": "20",
        "output": "MMT_Kindling",
        "output_count": "40",
        "skill": "4",
        "learn": "0.25",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "building": "MMT_ElectricSawmill",
    },
    "MMT_MakeCharcoal_Electric": {
        "work": "800",
        "input_count": "25",
        "output": "MMT_Charcoal",
        "output_count": "20",
        "skill": "4",
        "learn": "0.8",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "building": "MMT_ElectricCharcoalKiln",
    },
}


ELECTRIC_PETROCHEM_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab_{side}.png"
    for side in ("east", "north", "south")
]

ELECTRIC_PETROCHEM_LAB_RECIPES = [
    "MMT_DistillPolymers_Electric",
    "MMT_MakePlastic_Electric",
    "MMT_MakeRubber_Electric",
    "MMT_MakeSyntheticFibers_Electric",
    "MMT_MakeCarbon_Electric",
]

ELECTRIC_PETROCHEM_RECIPE_EXPECTATIONS = {
    "MMT_DistillPolymers_Electric": {
        "work": "1000",
        "inputs": [("thingDefs", "MMT_CrudeOil", "20")],
        "output": "MMT_Polymers",
        "output_count": "15",
        "skill": "6",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
    "MMT_MakePlastic_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_Polymers", "30")],
        "output": "MMT_Plastic",
        "output_count": "10",
        "skill": "8",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeRubber_Electric": {
        "work": "500",
        "inputs": [("thingDefs", "MMT_Polymers", "30"), ("thingDefs", "MMT_Sulfur", "3")],
        "output": "MMT_Rubber",
        "output_count": "10",
        "skill": "8",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeSyntheticFibers_Electric": {
        "work": "800",
        "inputs": [("thingDefs", "MMT_Polymers", "25"), ("thingDefs", "MMT_Sulfur", "3")],
        "output": "MMT_SyntheticFibers",
        "output_count": "5",
        "skill": "10",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
    "MMT_MakeCarbon_Electric": {
        "work": "2800",
        "inputs": [
            ("thingDefs", "MMT_Polymers", "25"),
            ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "25"),
            ("thingDefs", "MMT_Rubber", "15"),
        ],
        "output": "MMT_Carbon",
        "output_count": "12",
        "skill": "12",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Smith",
    },
}


ELECTRIC_ELECTRONICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

ELECTRIC_ELECTRONICS_TABLE_RECIPES = [
    "MMT_AssembleElectronicParts_Electric",
    "MMT_AssembleIC_Electric",
    "MMT_AssembleMicrochip_Electric",
]

ELECTRIC_ELECTRONICS_RECIPE_EXPECTATIONS = {
    "MMT_AssembleElectronicParts_Electric": {
        "work": "1600",
        "inputs": [
            ("thingDefs", "MMT_CopperBar", "15"),
            ("thingDefs", "MMT_Glass", "10"),
            ("thingDefs", "MMT_Silicon", "10"),
        ],
        "output": "MMT_ElectronicParts",
        "output_count": "5",
        "skill": "10",
    },
    "MMT_AssembleIC_Electric": {
        "work": "2400",
        "inputs": [
            ("thingDefs", "MMT_Silicon", "25"),
            ("thingDefs", "MMT_Plastic", "20"),
            ("thingDefs", "MMT_ElectronicParts", "3"),
            ("thingDefs", "Gold", "2"),
        ],
        "output": "MMT_IntegratedCircuit",
        "output_count": "5",
        "skill": "12",
    },
    "MMT_AssembleMicrochip_Electric": {
        "work": "2400",
        "inputs": [
            ("thingDefs", "MMT_IntegratedCircuit", "2"),
            ("thingDefs", "MMT_Carbon", "5"),
            ("thingDefs", "Gold", "5"),
        ],
        "output": "MMT_Microchip",
        "output_count": "1",
        "skill": "14",
    },
}


ADVANCED_ORE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedOre.xml"
ADVANCED_ALLOY_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml"
ADVANCED_ROCKS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AdvancedRocks.xml"
ADVANCED_ALLOYS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml"
ADVANCED_ALLOYS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_AdvancedAlloys.xml"

ADVANCED_ORE_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}/MMT_{ore}_{suffix}.png"
    for ore in ("NickelOre", "IlmeniteOre", "WolframiteOre")
    for suffix in ("a", "b", "c")
]

ADVANCED_ORE_EXPECTATIONS = {
    "MMT_NickelOre": {"label": "镍矿", "value": "4.0", "color": "(42,208,206)", "tex": "Things/Item/MMT/MMT_Ore/MMT_NickelOre/MMT_NickelOre"},
    "MMT_IlmeniteOre": {"label": "钛铁矿", "value": "8.0", "color": "(160,178,195)", "tex": "Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre/MMT_IlmeniteOre"},
    "MMT_WolframiteOre": {"label": "黑钨矿", "value": "9.0", "color": "(230,230,255)", "tex": "Things/Item/MMT/MMT_Ore/MMT_WolframiteOre/MMT_WolframiteOre"},
}

ADVANCED_ALLOY_EXPECTATIONS = {
    "MMT_NickelBar": {"label": "镍合金", "value": "6.0", "stack": "150", "color": "(42,208,206)", "stuff": True, "adjective": "镍制", "hp": "2.8", "beauty": "2.4", "work_make": "2.0", "work_build": "2.0", "mass": "1.35", "market_value": "0.7"},
    "MMT_Cobalt": {"label": "钴合金", "value": "10.0", "stack": "150", "color": "(60,90,120)", "stuff": False},
    "MMT_Chromium": {"label": "铬合金", "value": "5.0", "stack": "150", "color": "(235,235,235)", "stuff": False},
    "MMT_Tungsten": {"label": "钨合金", "value": "40.0", "stack": "150", "color": "(240,230,230)", "stuff": False},
    "MMT_Titanium": {"label": "钛合金", "value": "45.0", "stack": "75", "color": "(160,178,181)", "stuff": True, "adjective": "钛制", "hp": "3.3", "beauty": "1.8", "work_make": "3.0", "work_build": "3.0", "mass": "0.8", "door": "1.3"},
    "MMT_StelliteAlloy": {"label": "司太立合金（钨铬钴合金）", "value": "30.0", "stack": "150", "color": "(125,146,178)", "stuff": True, "adjective": "司太立制", "hp": "3.55", "beauty": "1.5", "work_make": "2.0", "work_build": "2.2", "mass": "1.24", "door": "0.95"},
    "MMT_NitinolAlloy": {"label": "镍钛合金", "value": "27.0", "stack": "150", "color": "(175,175,175)", "stuff": True, "adjective": "镍钛诺制", "hp": "3.6", "beauty": "1.6", "work_make": "2.6", "work_build": "2.4", "mass": "0.95", "door": "1.2"},
    "MMT_PobediteAlloy": {"label": "钨钴合金", "value": "33.0", "stack": "150", "color": "(85,85,85)", "stuff": True, "adjective": "钴硬质制", "hp": "4.0", "beauty": "1.1", "work_make": "2.8", "work_build": "2.8", "mass": "1.4", "door": "0.7"},
}

ADVANCED_MINEABLE_EXPECTATIONS = {
    "MMT_MineableNickelOre": {"label": "镍矿矿藏", "yield_thing": "MMT_NickelOre", "hp": "10000", "yield": "50", "commonality": "0.4", "lump": "5~10", "color": "(88,115,175)", "color_two": "(42,208,206)"},
    "MMT_MineableIlmenite": {"label": "钛铁矿", "yield_thing": "MMT_IlmeniteOre", "hp": "13000", "yield": "75", "commonality": "0.25", "lump": "3~9", "color": "(160,178,181)", "color_two": "(160,178,195)"},
    "MMT_MineableWolframite": {"label": "黑钨矿", "yield_thing": "MMT_WolframiteOre", "hp": "20000", "yield": "50", "commonality": "0.15", "lump": "3~9", "color": "(88,115,175)", "color_two": "(230,230,255)"},
}

ADVANCED_ALLOYS_FURNACE_RECIPES = [
    "MMT_SmeltNickelBar",
    "MMT_ExtractCobalt",
    "MMT_ExtractChromium",
    "MMT_SmeltTungsten",
    "MMT_SmeltTitanium",
    "MMT_MakeStellite",
    "MMT_MakeNitinol",
    "MMT_MakePobedite",
    "MMT_MakeFerrotitanium",
]

ADVANCED_ALLOYS_RECIPE_EXPECTATIONS = {
    "MMT_SmeltNickelBar": {"work": "1500", "inputs": [("thingDefs", "MMT_NickelOre", "15")], "output": "MMT_NickelBar", "output_count": "15", "skill": "13", "research": "MMT_AdvancedMetalRefining"},
    "MMT_ExtractCobalt": {"work": "1300", "inputs": [("thingDefs", "MMT_NickelOre", "30")], "output": "MMT_Cobalt", "output_count": "15", "skill": "14", "research": "MMT_AdvancedMetalRefining"},
    "MMT_ExtractChromium": {"work": "1200", "inputs": [("thingDefs", "MMT_IronOre", "25")], "output": "MMT_Chromium", "output_count": "10", "skill": "11", "research": "MMT_AdvancedMetalRefining"},
    "MMT_SmeltTungsten": {"work": "3000", "inputs": [("thingDefs", "MMT_WolframiteOre", "15")], "output": "MMT_Tungsten", "output_count": "15", "skill": "17", "research": "MMT_AdvancedAlloys"},
    "MMT_SmeltTitanium": {"work": "3300", "inputs": [("thingDefs", "MMT_IlmeniteOre", "15")], "output": "MMT_Titanium", "output_count": "15", "skill": "16", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeStellite": {"work": "2100", "inputs": [("thingDefs", "MMT_Cobalt", "5"), ("thingDefs", "MMT_Chromium", "13"), ("thingDefs", "MMT_Tungsten", "2")], "output": "MMT_StelliteAlloy", "output_count": "20", "skill": "17", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeNitinol": {"work": "2600", "inputs": [("thingDefs", "MMT_Titanium", "4"), ("thingDefs", "MMT_NickelBar", "11")], "output": "MMT_NitinolAlloy", "output_count": "15", "skill": "16", "research": "MMT_AdvancedAlloys"},
    "MMT_MakePobedite": {"work": "2700", "inputs": [("thingDefs", "MMT_Tungsten", "3"), ("thingDefs", "MMT_Carbon", "3"), ("thingDefs", "MMT_Cobalt", "3")], "output": "MMT_PobediteAlloy", "output_count": "10", "skill": "18", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeFerrotitanium": {"work": "2400", "inputs": [("thingDefs", "MMT_Titanium", "5"), ("thingDefs", "Steel", "12"), ("thingDefs", "MMT_Carbon", "3")], "output": "Plasteel", "output_count": "15", "skill": "15", "research": "MMT_AdvancedAlloys"},
}

PARTS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Parts.xml"
PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"
PARTS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Parts.xml"
PARTS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Parts.xml"

PARTS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Parts/{name}.png"
    for name in ("ComponentMedieval", "Wires", "Mechanism", "AdvMechanism", "AdvancedComponent", "ComponentUltra")
]

PARTS_BUILDING_TEXTURES = [
    *[
        f"1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
    *[
        f"1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
    "1.7/Textures/Things/Building/Production/MMT_RoboticAssembler/MMT_RoboticAssembler.png",
]

PARTS_RESOURCE_EXPECTATIONS = {
    "MMT_ComponentMedieval": {"label": "原始零部件", "value": "6", "mass": "1.5", "stack": "300", "tech": "Neolithic", "flam": "0.2", "tex": "Things/Item/MMT/MMT_Parts/ComponentMedieval"},
    "MMT_Wire": {"label": "电线", "value": "1.5", "mass": "0.2", "stack": "600", "tech": "Industrial", "flam": "0.1", "tex": "Things/Item/MMT/MMT_Parts/Wires"},
    "MMT_Mechanism": {"label": "机械组件", "value": "200", "mass": "0.65", "stack": "75", "tech": "Industrial", "flam": "1", "tex": "Things/Item/MMT/MMT_Parts/Mechanism"},
    "MMT_AdvMechanism": {"label": "先进机械组件", "value": "400", "mass": "0.7", "stack": "75", "tech": "Spacer", "flam": "1", "tex": "Things/Item/MMT/MMT_Parts/AdvMechanism"},
    "MMT_SpacerComponent": {"label": "高级零部件", "value": "150", "mass": "0.5", "stack": "150", "tech": "Spacer", "flam": "0.3", "tex": "Things/Item/MMT/MMT_Parts/AdvancedComponent"},
    "MMT_UltraComponent": {"label": "超级零部件", "value": "200", "mass": "0.5", "stack": "150", "tech": "Ultra", "flam": "0.3", "tex": "Things/Item/MMT/MMT_Parts/ComponentUltra"},
}

PARTS_RESEARCH_EXPECTATIONS = {
    "MMT_ComponentCrafting": {"label": "零件加工", "cost": "600", "tech": "Industrial", "prereqs": [], "x": "15.00", "y": "0.00"},
    "MMT_AdvancedComponents": {"label": "高级零件", "cost": "1600", "tech": "Spacer", "prereqs": ["MMT_ComponentCrafting"], "x": "15.00", "y": "1.00"},
    "MMT_UltraComponents": {"label": "超级零件", "cost": "2500", "tech": "Spacer", "prereqs": ["MMT_AdvancedComponents", "MMT_AdvancedAlloys"], "x": "15.00", "y": "2.00"},
}

PARTS_HAND_BENCH_RECIPES = [
    "MMT_MakeComponentMedieval_Wood",
    "MMT_MakeComponentMedieval_Metal",
    "MMT_MakeComponentIndustrial_Hand",
    "MMT_MakeWire_Hand",
    "MMT_MakeMechanism_Hand",
    "MMT_MakeComponentSpacer_Hand",
    "MMT_MakeAdvMechanism_Hand",
]

PARTS_ELECTRIC_BENCH_RECIPES = [
    "MMT_MakeComponentMedieval_Wood",
    "MMT_MakeComponentMedieval_Metal",
    "MMT_MakeComponentIndustrial_Electric",
    "MMT_MakeWire_Electric",
    "MMT_MakeMechanism_Electric",
    "MMT_MakeComponentSpacer_Electric",
    "MMT_MakeAdvMechanism_Electric",
    "MMT_MakeSpacerComponent",
]

PARTS_BUILDING_EXPECTATIONS = {
    "MMT_HandAssemblyBench": {
        "label": "手工装配台", "tex": "Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench", "graphic": "Graphic_Multi",
        "stuff_count": "125", "costs": {"WoodLog": "40", "Steel": "25"},
        "work_build": "1200", "hp": "180", "size": "(3,1)", "speed": "0.9", "power": None,
        "research": ["MMT_ComponentCrafting"], "recipes": PARTS_HAND_BENCH_RECIPES, "construction_skill": "4",
    },
    "MMT_ElectricAssemblyBench": {
        "label": "电动装配台", "tex": "Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench", "graphic": "Graphic_Multi",
        "stuff_count": "220", "costs": {"MMT_SyntheticFibers": "45", "MMT_IntegratedCircuit": "10", "ComponentIndustrial": "15", "MMT_Mechanism": "10"},
        "work_build": "5500", "hp": "210", "size": "(3,1)", "speed": "1.5", "power": "750",
        "research": ["MMT_ComponentCrafting", "MMT_AdvancedComponents"], "recipes": PARTS_ELECTRIC_BENCH_RECIPES, "construction_skill": "8",
    },
    "MMT_RoboticAssembler": {
        "label": "自动化装配器", "tex": "Things/Building/Production/MMT_RoboticAssembler/MMT_RoboticAssembler", "graphic": "Graphic_Single",
        "stuff_count": "120", "costs": {"MMT_SyntheticFibers": "45", "MMT_SpacerComponent": "10", "MMT_AdvMechanism": "10", "MMT_Microchip": "5"},
        "work_build": "9000", "hp": "200", "size": "(1,1)", "speed": None, "power": "5000",
        "research": ["MMT_UltraComponents"], "recipes": ["MMT_MakeUltraComponent"], "construction_skill": "10",
    },
}

PARTS_RECIPE_EXPECTATIONS = {
    "MMT_MakeComponentMedieval_Wood": {"work": "900", "inputs": [("thingDefs", "WoodLog", "20")], "output": "MMT_ComponentMedieval", "output_count": "10", "skill": None, "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench", "MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentMedieval_Metal": {"work": "900", "inputs": [("thingDefs", ("Steel", "MMT_CopperBar"), "10")], "output": "MMT_ComponentMedieval", "output_count": "10", "skill": None, "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench", "MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Hand": {"work": "2000", "inputs": [("thingDefs", ("Steel", "Plasteel"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Electric": {"work": "2000", "inputs": [("thingDefs", ("Steel", "Plasteel"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeWire_Hand": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "10")], "output": "MMT_Wire", "output_count": "20", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeWire_Electric": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "20")], "output": "MMT_Wire", "output_count": "40", "skill": "6", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeMechanism_Hand": {"work": "1800", "inputs": [("thingDefs", "Steel", "30"), ("thingDefs", "ComponentIndustrial", "10")], "output": "MMT_Mechanism", "output_count": "5", "skill": "4", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeMechanism_Electric": {"work": "1800", "inputs": [("thingDefs", "Steel", "60"), ("thingDefs", "ComponentIndustrial", "20")], "output": "MMT_Mechanism", "output_count": "10", "skill": "7", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentSpacer_Hand": {"work": "2500", "inputs": [("thingDefs", "Plasteel", "30")], "output": "ComponentSpacer", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentSpacer_Electric": {"work": "2500", "inputs": [("thingDefs", "Plasteel", "30")], "output": "ComponentSpacer", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeAdvMechanism_Hand": {"work": "3200", "inputs": [("thingDefs", "Plasteel", "30"), ("thingDefs", "ComponentSpacer", "10"), ("thingDefs", "MMT_SyntheticFibers", "5")], "output": "MMT_AdvMechanism", "output_count": "5", "skill": "13", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeAdvMechanism_Electric": {"work": "3200", "inputs": [("thingDefs", "Plasteel", "60"), ("thingDefs", "ComponentSpacer", "20"), ("thingDefs", "MMT_SyntheticFibers", "10")], "output": "MMT_AdvMechanism", "output_count": "10", "skill": "16", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeSpacerComponent": {"work": "3500", "inputs": [("thingDefs", "Plasteel", "25"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_SpacerComponent", "output_count": "10", "skill": "9", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeUltraComponent": {"work": "4500", "inputs": [("thingDefs", ("MMT_Tungsten", "MMT_Titanium"), "15"), ("thingDefs", ("MMT_StelliteAlloy", "MMT_PobediteAlloy"), "10"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_UltraComponent", "output_count": "10", "skill": "13", "research": "MMT_UltraComponents", "benches": ["MMT_RoboticAssembler"]},
}

INFRA_RESEARCH_TAB_XML = "1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml"
INFRA_DESIGNATION_XML = "1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml"
INFRA_CATEGORIES_XML = "1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml"
INFRA_PATCH_XML = "1.7/Patches/MMT_Patch_VanillaCategories.xml"
INFRA_RESEARCH_DIR = "1.7/Defs/ResearchProjectDefs"
INFRA_BUILDING_DIR = "1.7/Defs/ThingDefs_Buildings"
INFRA_RESEARCH_TOTAL = 18
INFRA_WORKTABLE_TOTAL = 15
INFRA_VANILLA_PARENTS = {"Manufactured", "Textiles", "ResourcesRaw"}
INFRA_EXPECTED_CATEGORIES = {
    "MMT_MetallicBar": "Manufactured",
    "MMT_HCMBar": "MMT_MetallicBar",
    "MMT_PRSBar": "MMT_MetallicBar",
    "MMT_SLDBar": "MMT_MetallicBar",
    "MMT_HeavyBar": "MMT_MetallicBar",
    "MMT_USLDBar": "MMT_MetallicBar",
    "MMT_USLDHBar": "MMT_MetallicBar",
    "MMT_RARBar": "MMT_MetallicBar",
    "MMT_RADBar": "MMT_MetallicBar",
    "MMT_Coal": "ResourcesRaw",
    "MMT_FuelCat": "Manufactured",
    "MMT_Parts": "Manufactured",
    "MMT_WeaponParts": "Manufactured",
    "MMT_ElectronicsCat": "Manufactured",
    "MMT_GlassCat": "Manufactured",
    "MMT_SuperMaterialCat": "Manufactured",
    "MMT_BTextiles": "Textiles",
}
INFRA_PATCH_TARGETS = {
    "Steel": "MMT_SLDBar",
    "Plasteel": "MMT_USLDBar",
    "Gold": "MMT_PRSBar",
    "Silver": "MMT_PRSBar",
    "Uranium": "MMT_RADBar",
    "ComponentIndustrial": "MMT_Parts",
    "ComponentSpacer": "MMT_Parts",
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
    assert_equal(set(research_defs), {"MMT_Glassworking", "MMT_ReinforcedGlassTech"}, "研究 defName 集合")
    research = research_defs["MMT_Glassworking"]
    assert_equal(child_text(research, "label"), "玻璃制造", "研究 label")
    assert_equal(child_text(research, "baseCost"), "300", "研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Medieval", "研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Stonecutting"], "研究前置")

    reinforced = research_defs["MMT_ReinforcedGlassTech"]
    assert_equal(child_text(reinforced, "label"), "强化玻璃工艺", "强化玻璃研究 label")
    assert_equal(child_text(reinforced, "baseCost"), "1500", "强化玻璃研究 baseCost")
    assert_equal(child_text(reinforced, "techLevel"), "Industrial", "强化玻璃研究 techLevel")
    assert_equal(li_values(reinforced, "prerequisites"), ["MMT_Glassworking", "MMT_BasicPetrochemistry"], "强化玻璃研究前置")
    assert_equal(child_text(reinforced, "researchViewX"), "5.00", "强化玻璃研究 X 坐标")
    assert_equal(child_text(reinforced, "researchViewY"), "1.00", "强化玻璃研究 Y 坐标")

    building_defs = def_map(parse_xml(BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_GlassworksTable"}, "建筑 defName 集合")
    building = building_defs["MMT_GlassworksTable"]
    assert_equal(child_text(building, "label"), "玻璃制作台", "建筑 label")
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
    require_files([RECIPE_XML, RESOURCE_XML, RESEARCH_XML, BUILDING_XML, PETROCHEM_RESOURCE_XML])
    recipe_defs = def_map(parse_xml(RECIPE_XML))
    assert_equal(set(recipe_defs), set(RECIPE_EXPECTATIONS), "配方 defName 集合")

    resource_names = set(def_map(parse_xml(RESOURCE_XML))) | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
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

        if "inputs" in expected:
            # 多输入配方：逐项比较 ingredients 与 fixedIngredientFilter
            actual_inputs = []
            for ingredient in recipe.findall("./ingredients/li"):
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
        else:
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

        if "inputs" in expected:
            for kind, value, _ in expected["inputs"]:
                if kind == "categories":
                    if value not in VANILLA_CATEGORIES:
                        fail(f"{name} 使用未登记分类: {value}")
                elif value not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {value}")
        elif expected["input_kind"] == "categories":
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
    assert_equal(child_text(building, "label"), "手动混凝土搅拌器", "混凝土建筑 label")
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
    assert_equal(child_text(sawmill, "label"), "手工锯台", "锯木机 label")
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
    assert_equal(child_text(kiln, "label"), "窑炉", "木炭窑 label")
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
    assert_equal(set(defs), set(METAL_ORE_EXPECTATIONS) | set(METAL_BAR_EXPECTATIONS) | {"MMT_MetalCan"}, "金属资源 defName 集合")

    for name, expected in METAL_ORE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
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
        assert_equal(li_values(node, "thingCategories"), [expected["category"]], f"{name} thingCategories")
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
    assert_equal(child_text(furnace, "label"), "熔炉", "熔炉 label")
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
    require_files([METAL_RECIPE_XML, METAL_RESOURCE_XML, WOOD_RESOURCE_XML, COAL_RESOURCE_XML, METAL_RESEARCH_XML, METAL_BUILDING_XML])
    recipe_defs = def_map(parse_xml(METAL_RECIPE_XML))
    assert_equal(set(recipe_defs), set(METAL_RECIPE_EXPECTATIONS), "金属配方 defName 集合")

    resource_names = set(def_map(parse_xml(METAL_RESOURCE_XML))) | set(def_map(parse_xml(WOOD_RESOURCE_XML))) | set(def_map(parse_xml(COAL_RESOURCE_XML)))
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

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
            for item in ((value,) if isinstance(value, str) else value):
                if item not in resource_names:
                    fail(f"{name} 使用未知资源: {item}")
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
    assert_equal(child_text(table, "label"), "石化产品实验室", "化学台 label")
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
    require_files([PETROCHEM_RECIPE_XML, PETROCHEM_RESOURCE_XML, WOOD_RESOURCE_XML, COAL_RESOURCE_XML, SULFUR_RESOURCE_XML, PETROCHEM_RESEARCH_XML, PETROCHEM_BUILDING_XML])
    recipe_defs = def_map(parse_xml(PETROCHEM_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PETROCHEM_RECIPE_EXPECTATIONS), "石化配方 defName 集合")

    resource_names = set(def_map(parse_xml(PETROCHEM_RESOURCE_XML))) | set(def_map(parse_xml(WOOD_RESOURCE_XML))) | set(def_map(parse_xml(COAL_RESOURCE_XML))) | set(def_map(parse_xml(SULFUR_RESOURCE_XML)))
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

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
            for item in ((value,) if isinstance(value, str) else value):
                if item not in resource_names:
                    fail(f"{name} 使用未知资源: {item}")
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


def validate_electronics_resources() -> None:
    require_files([ELECTRONICS_RESOURCE_XML, *ELECTRONICS_ITEM_TEXTURES])
    defs = def_map(parse_xml(ELECTRONICS_RESOURCE_XML))
    assert_equal(set(defs), set(ELECTRONICS_RESOURCE_EXPECTATIONS), "电子资源 defName 集合")

    for name, expected in ELECTRONICS_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    for path in ELECTRONICS_ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(ELECTRONICS_RESOURCE_XML):
        fail("电子资源 XML 不得引用 SK 类型")
    print("electronics-resources: OK")


def validate_electronics_building() -> None:
    require_files([ELECTRONICS_RESEARCH_XML, ELECTRONICS_BUILDING_XML, *ELECTRONICS_BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(ELECTRONICS_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_BasicElectronics", "MMT_IntegratedCircuitTech"}, "电子研究 defName 集合")
    research = research_defs["MMT_BasicElectronics"]
    assert_equal(child_text(research, "label"), "基础电子工艺", "电子研究 label")
    assert_equal(child_text(research, "baseCost"), "1600", "电子研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "电子研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["MicroelectronicsBasics"], "电子研究前置")
    assert_equal(child_text(research, "researchViewX"), "16.00", "电子研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "0.00", "电子研究 Y 坐标")

    ic_research = research_defs["MMT_IntegratedCircuitTech"]
    assert_equal(child_text(ic_research, "label"), "集成电路技术", "集成电路研究 label")
    assert_equal(child_text(ic_research, "baseCost"), "2000", "集成电路研究 baseCost")
    assert_equal(child_text(ic_research, "techLevel"), "Industrial", "集成电路研究 techLevel")
    assert_equal(li_values(ic_research, "prerequisites"), ["MMT_BasicElectronics"], "集成电路研究前置")
    assert_equal(child_text(ic_research, "researchViewX"), "17.00", "集成电路研究 X 坐标")
    assert_equal(child_text(ic_research, "researchViewY"), "1.00", "集成电路研究 Y 坐标")

    building_defs = def_map(parse_xml(ELECTRONICS_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_HandElectronicsTable"}, "电子建筑 defName 集合")
    table = building_defs["MMT_HandElectronicsTable"]
    assert_equal(child_text(table, "label"), "电子器件工作台", "电子台 label")
    assert_equal(child_text(table, "thingClass"), "Building_WorkTable", "电子台 thingClass")
    assert_equal(child_text(table, "./graphicData/texPath"), "Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable", "电子台 texPath")
    assert_equal(child_text(table, "./graphicData/graphicClass"), "Graphic_Multi", "电子台 graphicClass")
    assert_equal(li_values(table, "stuffCategories"), ["Metallic"], "电子台材料分类")
    assert_equal(child_text(table, "costStuffCount"), "140", "电子台材料数量")
    assert_equal(child_text(table, "./costList/ComponentIndustrial"), "5", "电子台零部件成本")
    assert_equal(child_text(table, "./statBases/WorkToBuild"), "2200", "电子台建造工作量")
    assert_equal(child_text(table, "./statBases/MaxHitPoints"), "220", "电子台耐久")
    assert_equal(child_text(table, "size"), "(3,3)", "电子台尺寸")
    assert_equal(li_values(table, "recipes"), ELECTRONICS_TABLE_RECIPES, "电子台配方")
    assert_equal(li_values(table, "researchPrerequisites"), ["MMT_BasicElectronics", "MMT_IntegratedCircuitTech"], "电子台研究前置")
    assert_equal(child_text(table, "constructionSkillPrerequisite"), "8", "电子台建造技能")

    comp_classes = [li.attrib.get("Class") for li in table.findall("./comps/li")]
    if "CompProperties_Power" in comp_classes:
        fail("手动电子工作台不应包含电力组件")

    for path in ELECTRONICS_BUILDING_TEXTURES:
        assert_png(path)

    for path in (ELECTRONICS_RESEARCH_XML, ELECTRONICS_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("electronics-building: OK")


def validate_electronics_recipes() -> None:
    require_files([
        ELECTRONICS_RECIPE_XML,
        ELECTRONICS_RESOURCE_XML,
        RESOURCE_XML,
        METAL_RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        ELECTRONICS_RESEARCH_XML,
        ELECTRONICS_BUILDING_XML,
        METAL_BUILDING_XML,
    ])
    recipe_defs = def_map(parse_xml(ELECTRONICS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ELECTRONICS_RECIPE_EXPECTATIONS), "电子配方 defName 集合")

    resource_names = (
        set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(RESOURCE_XML)))
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
    )
    building_recipes = {
        "MMT_HandFurnace": li_values(def_map(parse_xml(METAL_BUILDING_XML))["MMT_HandFurnace"], "recipes"),
        "MMT_HandElectronicsTable": li_values(def_map(parse_xml(ELECTRONICS_BUILDING_XML))["MMT_HandElectronicsTable"], "recipes"),
    }

    for name, expected in ELECTRONICS_RECIPE_EXPECTATIONS.items():
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载，不应写 recipeUsers")
        building = expected["building"]
        if name not in building_recipes[building]:
            fail(f"{name} 未挂载到 {building}")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        for _, value, _ in expected["inputs"]:
            for item in ((value,) if isinstance(value, str) else value):
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

    recipe_text = read_text(ELECTRONICS_RECIPE_XML)
    if "SK." in recipe_text:
        fail("电子配方 XML 不得引用 SK 类型")
    if "ElectronicCraftingSpeed" in recipe_text:
        fail("电子配方不得使用 RimWorld 1.7 不存在的 ElectronicCraftingSpeed")
    print("electronics-recipes: OK")


def validate_electronics() -> None:
    validate_electronics_resources()
    validate_electronics_building()
    validate_electronics_recipes()
    print("electronics: OK")


def validate_electric() -> None:
    require_files([
        ELECTRIC_RESEARCH_XML,
        ELECTRIC_BUILDING_XML,
        ELECTRIC_RECIPE_XML,
        METAL_RESOURCE_XML,
        WOOD_RESOURCE_XML,
        COAL_RESOURCE_XML,
        PRECIOUS_RESOURCE_XML,
        RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        *ELECTRIC_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(ELECTRIC_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_ElectricSmelting"}, "电动研究 defName 集合")
    research = research_defs["MMT_ElectricSmelting"]
    assert_equal(child_text(research, "label"), "电弧冶炼", "电动研究 label")
    assert_equal(child_text(research, "baseCost"), "2000", "电动研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "电动研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["MMT_BasicMetalworking", "MicroelectronicsBasics"], "电动研究前置")
    assert_equal(child_text(research, "researchViewX"), "11.00", "电动研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "1.00", "电动研究 Y 坐标")

    building_defs = def_map(parse_xml(ELECTRIC_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_ElectricArcFurnace"}, "电动建筑 defName 集合")
    furnace = building_defs["MMT_ElectricArcFurnace"]
    assert_equal(child_text(furnace, "label"), "电弧炉", "电弧炉 label")
    assert_equal(child_text(furnace, "thingClass"), "Building_WorkTable", "电弧炉 thingClass")
    assert_equal(child_text(furnace, "./graphicData/texPath"), "Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace", "电弧炉 texPath")
    assert_equal(child_text(furnace, "./graphicData/graphicClass"), "Graphic_Multi", "电弧炉 graphicClass")
    assert_equal(li_values(furnace, "stuffCategories"), ["Metallic"], "电弧炉材料分类")
    assert_equal(child_text(furnace, "costStuffCount"), "180", "电弧炉材料数量")
    assert_equal(child_text(furnace, "./costList/MMT_ReinforcedConcrete"), "50", "电弧炉钢筋混凝土成本")
    assert_equal(child_text(furnace, "./costList/ComponentIndustrial"), "15", "电弧炉零部件成本")
    assert_equal(child_text(furnace, "./costList/MMT_ElectronicParts"), "6", "电弧炉电子元件成本")
    assert_equal(child_text(furnace, "./statBases/WorkToBuild"), "4500", "电弧炉建造工作量")
    assert_equal(child_text(furnace, "./statBases/MaxHitPoints"), "280", "电弧炉耐久")
    assert_equal(child_text(furnace, "size"), "(3,3)", "电弧炉尺寸")
    assert_equal(li_values(furnace, "recipes"), ELECTRIC_FURNACE_RECIPES + ADVANCED_ALLOYS_FURNACE_RECIPES, "电弧炉配方")
    assert_equal(li_values(furnace, "researchPrerequisites"), ["MMT_ElectricSmelting"], "电弧炉研究前置")
    assert_equal(child_text(furnace, "constructionSkillPrerequisite"), "10", "电弧炉建造技能")

    # 电动建筑与手动台相反：必须包含电力组件且功耗为 2500W
    comp_classes = [li.attrib.get("Class") for li in furnace.findall("./comps/li")]
    for expected in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable", "CompProperties_Glower"):
        if expected not in comp_classes:
            fail(f"电弧炉缺少组件: {expected}")
    power = furnace.find("./comps/li[@Class='CompProperties_Power']")
    assert_equal(child_text(power, "compClass"), "CompPowerTrader", "电弧炉电力 compClass")
    assert_equal(child_text(power, "basePowerConsumption"), "2500", "电弧炉电力消耗")
    glower = furnace.find("./comps/li[@Class='CompProperties_Glower']")
    assert_equal(child_text(glower, "glowRadius"), "4", "电弧炉发光半径")
    assert_equal(child_text(glower, "glowColor"), "(217,112,33,0)", "电弧炉发光颜色")

    recipe_defs = def_map(parse_xml(ELECTRIC_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ELECTRIC_RECIPE_EXPECTATIONS) | {"MMT_MakeMetalCan"}, "电动配方 defName 集合")

    resource_names = (
        set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
        | set(def_map(parse_xml(COAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PRECIOUS_RESOURCE_XML)))
        | set(def_map(parse_xml(RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
    )
    output_names = resource_names | VANILLA_THINGS
    furnace_recipes = li_values(furnace, "recipes")

    for name, expected in ELECTRIC_RECIPE_EXPECTATIONS.items():
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_ElectricArcFurnace 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in furnace_recipes:
            fail(f"{name} 未挂载到电弧炉")

        research_prerequisite = recipe.findtext("researchPrerequisite")
        research_prerequisite = research_prerequisite.strip() if research_prerequisite else None
        assert_equal(research_prerequisite, "MMT_ElectricSmelting", f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in output_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

    for path in ELECTRIC_BUILDING_TEXTURES:
        assert_png(path)

    for path in (ELECTRIC_RESEARCH_XML, ELECTRIC_BUILDING_XML, ELECTRIC_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("electric: OK")


def validate_electric_wood() -> None:
    require_files([
        ELECTRIC_WOOD_RESEARCH_XML,
        ELECTRIC_WOOD_BUILDING_XML,
        ELECTRIC_WOOD_RECIPE_XML,
        WOOD_RESOURCE_XML,
        *ELECTRIC_WOOD_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(ELECTRIC_WOOD_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_ElectricWoodworking"}, "电动木材研究 defName 集合")
    research = research_defs["MMT_ElectricWoodworking"]
    assert_equal(child_text(research, "label"), "电动木材加工", "电动木材研究 label")
    assert_equal(child_text(research, "baseCost"), "1500", "电动木材研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "电动木材研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["MMT_CharcoalBurning", "MicroelectronicsBasics"], "电动木材研究前置")
    assert_equal(child_text(research, "researchViewX"), "2.00", "电动木材研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "1.00", "电动木材研究 Y 坐标")

    building_defs = def_map(parse_xml(ELECTRIC_WOOD_BUILDING_XML))
    assert_equal(set(building_defs), set(ELECTRIC_WOOD_BUILDING_EXPECTATIONS), "电动木材建筑 defName 集合")
    building_recipes: dict[str, list[str]] = {}
    for name, expected in ELECTRIC_WOOD_BUILDING_EXPECTATIONS.items():
        building = building_defs[name]
        assert_equal(child_text(building, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(building, "thingClass"), "Building_WorkTable", f"{name} thingClass")
        assert_equal(child_text(building, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(building, "./graphicData/graphicClass"), "Graphic_Multi", f"{name} graphicClass")
        assert_equal(li_values(building, "stuffCategories"), [expected["stuff"]], f"{name} 材料分类")
        assert_equal(child_text(building, "costStuffCount"), expected["stuff_count"], f"{name} 材料数量")
        assert_equal(child_text(building, "./costList/ComponentIndustrial"), expected["components"], f"{name} 零部件成本")
        assert_equal(child_text(building, "./costList/MMT_ElectronicParts"), expected["parts"], f"{name} 电子元件成本")
        assert_equal(child_text(building, "./statBases/WorkToBuild"), expected["work"], f"{name} 建造工作量")
        assert_equal(child_text(building, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(building, "size"), expected["size"], f"{name} 尺寸")
        assert_equal(li_values(building, "recipes"), expected["recipes"], f"{name} 配方")
        assert_equal(li_values(building, "researchPrerequisites"), ["MMT_ElectricWoodworking"], f"{name} 研究前置")
        assert_equal(child_text(building, "constructionSkillPrerequisite"), expected["skill"], f"{name} 建造技能")

        # 电动建筑与手动台相反：必须包含电力组件且功耗符合设计
        comp_classes = [li.attrib.get("Class") for li in building.findall("./comps/li")]
        for comp in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
            if comp not in comp_classes:
                fail(f"{name} 缺少组件: {comp}")
        power = building.find("./comps/li[@Class='CompProperties_Power']")
        assert_equal(child_text(power, "compClass"), "CompPowerTrader", f"{name} 电力 compClass")
        assert_equal(child_text(power, "basePowerConsumption"), expected["power"], f"{name} 电力消耗")
        building_recipes[name] = li_values(building, "recipes")

    kiln = building_defs["MMT_ElectricCharcoalKiln"]
    glower = kiln.find("./comps/li[@Class='CompProperties_Glower']")
    if glower is None:
        fail("电动木炭窑缺少发光组件")
    assert_equal(child_text(glower, "glowRadius"), "4", "电动木炭窑发光半径")
    assert_equal(child_text(glower, "glowColor"), "(217,112,33,0)", "电动木炭窑发光颜色")

    recipe_defs = def_map(parse_xml(ELECTRIC_WOOD_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ELECTRIC_WOOD_RECIPE_EXPECTATIONS), "电动木材配方 defName 集合")

    resource_names = set(def_map(parse_xml(WOOD_RESOURCE_XML)))

    for name, expected in ELECTRIC_WOOD_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), expected["learn"], f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        ingredient = recipe.find("./ingredients/li")
        if ingredient is None:
            fail(f"{name} 缺少输入")
        assert_equal(child_text(ingredient, "count"), expected["input_count"], f"{name} 输入数量")
        assert_equal(li_values(ingredient, "./filter/thingDefs"), ["WoodLog"], f"{name} 输入引用")
        assert_equal(li_values(recipe, "./fixedIngredientFilter/thingDefs"), ["WoodLog"], f"{name} 固定输入引用")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载，不应写 recipeUsers")
        if name not in building_recipes[expected["building"]]:
            fail(f"{name} 未挂载到 {expected['building']}")

        research_prerequisite = recipe.findtext("researchPrerequisite")
        research_prerequisite = research_prerequisite.strip() if research_prerequisite else None
        assert_equal(research_prerequisite, "MMT_ElectricWoodworking", f"{name} researchPrerequisite")

        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

    for path in ELECTRIC_WOOD_BUILDING_TEXTURES:
        assert_png(path)

    for path in (ELECTRIC_WOOD_RESEARCH_XML, ELECTRIC_WOOD_BUILDING_XML, ELECTRIC_WOOD_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    recipe_text = read_text(ELECTRIC_WOOD_RECIPE_XML)
    if "SmithingSpeed" in recipe_text or "Wooden" in recipe_text:
        fail("电动木材配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed 或 Wooden 分类")
    print("electric-wood: OK")


def validate_electric_petrochem() -> None:
    require_files([
        ELECTRIC_PETROCHEM_RESEARCH_XML,
        ELECTRIC_PETROCHEM_BUILDING_XML,
        ELECTRIC_PETROCHEM_RECIPE_XML,
        PETROCHEM_RESOURCE_XML,
        SULFUR_RESOURCE_XML,
        WOOD_RESOURCE_XML,
        COAL_RESOURCE_XML,
        *ELECTRIC_PETROCHEM_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(ELECTRIC_PETROCHEM_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_ElectricPetrochemistry"}, "电动石化研究 defName 集合")
    research = research_defs["MMT_ElectricPetrochemistry"]
    assert_equal(child_text(research, "label"), "电动石化工艺", "电动石化研究 label")
    assert_equal(child_text(research, "baseCost"), "1800", "电动石化研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "电动石化研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["MMT_BasicPetrochemistry", "MicroelectronicsBasics"], "电动石化研究前置")
    assert_equal(child_text(research, "researchViewX"), "14.00", "电动石化研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "1.00", "电动石化研究 Y 坐标")

    building_defs = def_map(parse_xml(ELECTRIC_PETROCHEM_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_ElectricChemLab"}, "电动石化建筑 defName 集合")
    lab = building_defs["MMT_ElectricChemLab"]
    assert_equal(child_text(lab, "label"), "石化产品实验室", "石化实验室 label")
    assert_equal(child_text(lab, "thingClass"), "Building_WorkTable", "石化实验室 thingClass")
    assert_equal(child_text(lab, "./graphicData/texPath"), "Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab", "石化实验室 texPath")
    assert_equal(child_text(lab, "./graphicData/graphicClass"), "Graphic_Multi", "石化实验室 graphicClass")
    assert_equal(li_values(lab, "stuffCategories"), ["Metallic"], "石化实验室材料分类")
    assert_equal(child_text(lab, "costStuffCount"), "130", "石化实验室材料数量")
    assert_equal(child_text(lab, "./costList/MMT_ReinforcedConcrete"), "40", "石化实验室钢筋混凝土成本")
    assert_equal(child_text(lab, "./costList/MMT_Glass"), "50", "石化实验室玻璃成本")
    assert_equal(child_text(lab, "./costList/ComponentIndustrial"), "20", "石化实验室零部件成本")
    assert_equal(child_text(lab, "./costList/MMT_ElectronicParts"), "5", "石化实验室电子元件成本")
    assert_equal(child_text(lab, "./statBases/WorkToBuild"), "5500", "石化实验室建造工作量")
    assert_equal(child_text(lab, "./statBases/MaxHitPoints"), "340", "石化实验室耐久")
    assert_equal(child_text(lab, "size"), "(4,5)", "石化实验室尺寸")
    assert_equal(child_text(lab, "interactionCellOffset"), "(0,0,3)", "石化实验室交互格偏移")
    assert_equal(li_values(lab, "recipes"), ELECTRIC_PETROCHEM_LAB_RECIPES, "石化实验室配方")
    assert_equal(li_values(lab, "researchPrerequisites"), ["MMT_ElectricPetrochemistry"], "石化实验室研究前置")
    assert_equal(child_text(lab, "constructionSkillPrerequisite"), "10", "石化实验室建造技能")

    # 电动建筑与手动台相反：必须包含电力组件且功耗为 450W
    comp_classes = [li.attrib.get("Class") for li in lab.findall("./comps/li")]
    for comp in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
        if comp not in comp_classes:
            fail(f"石化实验室缺少组件: {comp}")
    power = lab.find("./comps/li[@Class='CompProperties_Power']")
    assert_equal(child_text(power, "compClass"), "CompPowerTrader", "石化实验室电力 compClass")
    assert_equal(child_text(power, "basePowerConsumption"), "450", "石化实验室电力消耗")

    recipe_defs = def_map(parse_xml(ELECTRIC_PETROCHEM_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ELECTRIC_PETROCHEM_RECIPE_EXPECTATIONS), "电动石化配方 defName 集合")

    resource_names = (
        set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(SULFUR_RESOURCE_XML)))
        | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
        | set(def_map(parse_xml(COAL_RESOURCE_XML)))
    )
    lab_recipes = li_values(lab, "recipes")

    for name, expected in ELECTRIC_PETROCHEM_RECIPE_EXPECTATIONS.items():
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_ElectricChemLab 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in lab_recipes:
            fail(f"{name} 未挂载到石化电动实验室")

        research_prerequisite = recipe.findtext("researchPrerequisite")
        research_prerequisite = research_prerequisite.strip() if research_prerequisite else None
        assert_equal(research_prerequisite, "MMT_ElectricPetrochemistry", f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in set(def_map(parse_xml(PETROCHEM_RESOURCE_XML))):
            fail(f"{name} 输出不是石化资源: {expected['output']}")

    for path in ELECTRIC_PETROCHEM_BUILDING_TEXTURES:
        assert_png(path)

    for path in (ELECTRIC_PETROCHEM_RESEARCH_XML, ELECTRIC_PETROCHEM_BUILDING_XML, ELECTRIC_PETROCHEM_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    if "SmithingSpeed" in read_text(ELECTRIC_PETROCHEM_RECIPE_XML):
        fail("电动石化配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed")
    print("electric-petrochem: OK")


def validate_electric_electronics() -> None:
    require_files([
        ELECTRIC_ELECTRONICS_RESEARCH_XML,
        ELECTRIC_ELECTRONICS_BUILDING_XML,
        ELECTRIC_ELECTRONICS_RECIPE_XML,
        ELECTRONICS_RESOURCE_XML,
        METAL_RESOURCE_XML,
        RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        *ELECTRIC_ELECTRONICS_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(ELECTRIC_ELECTRONICS_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_ElectricElectronics"}, "电动电子研究 defName 集合")
    research = research_defs["MMT_ElectricElectronics"]
    assert_equal(child_text(research, "label"), "电动电子工艺", "电动电子研究 label")
    assert_equal(child_text(research, "baseCost"), "2200", "电动电子研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "电动电子研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["MMT_IntegratedCircuitTech"], "电动电子研究前置")
    assert_equal(child_text(research, "researchViewX"), "18.00", "电动电子研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "2.00", "电动电子研究 Y 坐标")

    building_defs = def_map(parse_xml(ELECTRIC_ELECTRONICS_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_ElectricElectronicsTable"}, "电动电子建筑 defName 集合")
    table = building_defs["MMT_ElectricElectronicsTable"]
    assert_equal(child_text(table, "label"), "电子器件工作台", "电子电动台 label")
    assert_equal(child_text(table, "thingClass"), "Building_WorkTable", "电子电动台 thingClass")
    assert_equal(child_text(table, "./graphicData/texPath"), "Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable", "电子电动台 texPath")
    assert_equal(child_text(table, "./graphicData/graphicClass"), "Graphic_Multi", "电子电动台 graphicClass")
    assert_equal(li_values(table, "stuffCategories"), ["Metallic"], "电子电动台材料分类")
    assert_equal(child_text(table, "costStuffCount"), "140", "电子电动台材料数量")
    assert_equal(child_text(table, "./costList/MMT_CopperBar"), "40", "电子电动台铜锭成本")
    assert_equal(child_text(table, "./costList/ComponentIndustrial"), "10", "电子电动台零部件成本")
    assert_equal(child_text(table, "./statBases/WorkToBuild"), "2750", "电子电动台建造工作量")
    assert_equal(child_text(table, "./statBases/MaxHitPoints"), "220", "电子电动台耐久")
    assert_equal(child_text(table, "size"), "(3,1)", "电子电动台尺寸")
    assert_equal(li_values(table, "recipes"), ELECTRIC_ELECTRONICS_TABLE_RECIPES, "电子电动台配方")
    assert_equal(li_values(table, "researchPrerequisites"), ["MMT_ElectricElectronics"], "电子电动台研究前置")
    assert_equal(child_text(table, "constructionSkillPrerequisite"), "8", "电子电动台建造技能")

    # 电动建筑与手动台相反：必须包含电力组件且功耗为 600W
    comp_classes = [li.attrib.get("Class") for li in table.findall("./comps/li")]
    for comp in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
        if comp not in comp_classes:
            fail(f"电子电动台缺少组件: {comp}")
    power = table.find("./comps/li[@Class='CompProperties_Power']")
    assert_equal(child_text(power, "compClass"), "CompPowerTrader", "电子电动台电力 compClass")
    assert_equal(child_text(power, "basePowerConsumption"), "600", "电子电动台电力消耗")

    recipe_defs = def_map(parse_xml(ELECTRIC_ELECTRONICS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ELECTRIC_ELECTRONICS_RECIPE_EXPECTATIONS), "电动电子配方 defName 集合")

    resource_names = (
        set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
    )
    electronics_names = set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
    table_recipes = li_values(table, "recipes")

    for name, expected in ELECTRIC_ELECTRONICS_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "GeneralLaborSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), "Smith", f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), "Recipe_Machining", f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_ElectricElectronicsTable 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in table_recipes:
            fail(f"{name} 未挂载到电子电动工作台")

        research_prerequisite = recipe.findtext("researchPrerequisite")
        research_prerequisite = research_prerequisite.strip() if research_prerequisite else None
        assert_equal(research_prerequisite, "MMT_ElectricElectronics", f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in electronics_names:
            fail(f"{name} 输出不是电子资源: {expected['output']}")

    for path in ELECTRIC_ELECTRONICS_BUILDING_TEXTURES:
        assert_png(path)

    for path in (ELECTRIC_ELECTRONICS_RESEARCH_XML, ELECTRIC_ELECTRONICS_BUILDING_XML, ELECTRIC_ELECTRONICS_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    if "ElectronicCraftingSpeed" in read_text(ELECTRIC_ELECTRONICS_RECIPE_XML):
        fail("电动电子配方不得使用 RimWorld 1.7 不存在的 ElectronicCraftingSpeed")
    print("electric-electronics: OK")


def validate_advanced_alloys() -> None:
    require_files([
        ADVANCED_ORE_RESOURCE_XML,
        ADVANCED_ALLOY_RESOURCE_XML,
        ADVANCED_ROCKS_BUILDING_XML,
        ADVANCED_ALLOYS_RECIPE_XML,
        ADVANCED_ALLOYS_RESEARCH_XML,
        ELECTRIC_BUILDING_XML,
        METAL_RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        *ADVANCED_ORE_ITEM_TEXTURES,
    ])

    research_defs = def_map(parse_xml(ADVANCED_ALLOYS_RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_AdvancedMetalRefining", "MMT_AdvancedAlloys"}, "高级合金研究 defName 集合")
    refining = research_defs["MMT_AdvancedMetalRefining"]
    assert_equal(child_text(refining, "label"), "高级金属精炼", "精炼研究 label")
    assert_equal(child_text(refining, "baseCost"), "1600", "精炼研究 baseCost")
    assert_equal(child_text(refining, "techLevel"), "Industrial", "精炼研究 techLevel")
    assert_equal(li_values(refining, "prerequisites"), ["MMT_ElectricSmelting"], "精炼研究前置")
    assert_equal(child_text(refining, "researchViewX"), "12.00", "精炼研究 X 坐标")
    assert_equal(child_text(refining, "researchViewY"), "2.00", "精炼研究 Y 坐标")
    alloys = research_defs["MMT_AdvancedAlloys"]
    assert_equal(child_text(alloys, "label"), "高级合金", "合金研究 label")
    assert_equal(child_text(alloys, "baseCost"), "2300", "合金研究 baseCost")
    assert_equal(child_text(alloys, "techLevel"), "Spacer", "合金研究 techLevel")
    assert_equal(li_values(alloys, "prerequisites"), ["MMT_AdvancedMetalRefining"], "合金研究前置")
    assert_equal(child_text(alloys, "researchViewX"), "13.00", "合金研究 X 坐标")
    assert_equal(child_text(alloys, "researchViewY"), "3.00", "合金研究 Y 坐标")

    ore_defs = def_map(parse_xml(ADVANCED_ORE_RESOURCE_XML))
    assert_equal(set(ore_defs), set(ADVANCED_ORE_EXPECTATIONS), "高级矿石 defName 集合")
    for name, expected in ADVANCED_ORE_EXPECTATIONS.items():
        node = ore_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), "300", f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(child_text(node, "./graphicData/color"), expected["color"], f"{name} color")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    alloy_defs = def_map(parse_xml(ADVANCED_ALLOY_RESOURCE_XML))
    assert_equal(set(alloy_defs), set(ADVANCED_ALLOY_EXPECTATIONS), "高级合金资源 defName 集合")
    for name, expected in ADVANCED_ALLOY_EXPECTATIONS.items():
        node = alloy_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), "Things/Item/MMT/MMT_Ingot/MMT_Ingot", f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(child_text(node, "./graphicData/color"), expected["color"], f"{name} color")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if expected["stuff"]:
            assert_equal(child_text(node, "useHitPoints"), "true", f"{name} useHitPoints")
            assert_equal(li_values(node, "./stuffProps/categories"), ["Metallic"], f"{name} stuff categories")
            assert_equal(child_text(node, "./stuffProps/stuffAdjective"), expected["adjective"], f"{name} stuffAdjective")
            assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), expected["hp"], f"{name} MaxHitPoints factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Beauty"), expected["beauty"], f"{name} Beauty factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Flammability"), "0", f"{name} Flammability factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), expected["work_make"], f"{name} WorkToMake factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), expected["work_build"], f"{name} WorkToBuild factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Mass"), expected["mass"], f"{name} Mass factor")
            if "market_value" in expected:
                assert_equal(child_text(node, "./stuffProps/statFactors/MarketValue"), expected["market_value"], f"{name} MarketValue factor")
            if "door" in expected:
                assert_equal(child_text(node, "./stuffProps/statFactors/DoorOpenSpeed"), expected["door"], f"{name} DoorOpenSpeed factor")
            assert_equal(child_text(node, "terrainAffordanceNeeded"), "Medium", f"{name} terrain affordance")
        elif node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    rock_defs = def_map(parse_xml(ADVANCED_ROCKS_BUILDING_XML))
    assert_equal(set(rock_defs), set(ADVANCED_MINEABLE_EXPECTATIONS), "高级矿脉 defName 集合")
    for name, expected in ADVANCED_MINEABLE_EXPECTATIONS.items():
        vein = rock_defs[name]
        assert_equal(vein.attrib.get("ParentName"), "RockBase", f"{name} 父类")
        assert_equal(child_text(vein, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", f"{name} texPath")
        assert_equal(child_text(vein, "./graphicData/color"), expected["color"], f"{name} color")
        assert_equal(child_text(vein, "./graphicData/colorTwo"), expected["color_two"], f"{name} colorTwo")
        assert_equal(child_text(vein, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(vein, "./building/isResourceRock"), "true", f"{name} isResourceRock")
        assert_equal(child_text(vein, "./building/mineableThing"), expected["yield_thing"], f"{name} mineableThing")
        assert_equal(child_text(vein, "./building/mineableYield"), expected["yield"], f"{name} mineableYield")
        assert_equal(child_text(vein, "./building/mineableScatterCommonality"), expected["commonality"], f"{name} 常见度")
        assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), expected["lump"], f"{name} 矿块范围")
        assert_equal(child_text(vein, "./building/veinMineable"), "true", f"{name} veinMineable")

    recipe_defs = def_map(parse_xml(ADVANCED_ALLOYS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ADVANCED_ALLOYS_RECIPE_EXPECTATIONS), "高级合金配方 defName 集合")

    resource_names = (
        set(ore_defs)
        | set(alloy_defs)
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
    )
    output_names = set(alloy_defs) | VANILLA_THINGS
    furnace_recipes = li_values(def_map(parse_xml(ELECTRIC_BUILDING_XML))["MMT_ElectricArcFurnace"], "recipes")

    for name, expected in ADVANCED_ALLOYS_RECIPE_EXPECTATIONS.items():
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
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由 MMT_ElectricArcFurnace 的 recipes 列表挂载，不应写 recipeUsers")
        if name not in furnace_recipes:
            fail(f"{name} 未挂载到电弧炉")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in output_names:
            fail(f"{name} 输出不是高级合金资源: {expected['output']}")

    for path in ADVANCED_ORE_ITEM_TEXTURES:
        assert_png(path)

    for path in (ADVANCED_ORE_RESOURCE_XML, ADVANCED_ALLOY_RESOURCE_XML, ADVANCED_ROCKS_BUILDING_XML, ADVANCED_ALLOYS_RECIPE_XML, ADVANCED_ALLOYS_RESEARCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    alloy_text = read_text(ADVANCED_ALLOY_RESOURCE_XML)
    for forbidden in ("SolidMetallic", "StrongMetallic", "RuggedMetallic", "HeavyMetallic"):
        if forbidden in alloy_text:
            fail(f"高级合金资源不得使用 RimWorld 1.7 不存在的 {forbidden} 分类")
    if "SmithingSpeed" in read_text(ADVANCED_ALLOYS_RECIPE_XML):
        fail("高级合金配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed")
    print("advanced-alloys: OK")


def validate_parts() -> None:
    require_files([
        PARTS_RESOURCE_XML,
        PARTS_BUILDING_XML,
        PARTS_RECIPE_XML,
        PARTS_RESEARCH_XML,
        METAL_RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        ADVANCED_ALLOY_RESOURCE_XML,
        *PARTS_ITEM_TEXTURES,
        *PARTS_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(PARTS_RESEARCH_XML))
    assert_equal(set(research_defs), set(PARTS_RESEARCH_EXPECTATIONS), "零件研究 defName 集合")
    for name, expected in PARTS_RESEARCH_EXPECTATIONS.items():
        node = research_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "baseCost"), expected["cost"], f"{name} baseCost")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "prerequisites"), expected["prereqs"], f"{name} 前置")
        assert_equal(child_text(node, "researchViewX"), expected["x"], f"{name} X 坐标")
        assert_equal(child_text(node, "researchViewY"), expected["y"], f"{name} Y 坐标")

    resource_defs = def_map(parse_xml(PARTS_RESOURCE_XML))
    assert_equal(set(resource_defs), set(PARTS_RESOURCE_EXPECTATIONS), "零件资源 defName 集合")
    for name, expected in PARTS_RESOURCE_EXPECTATIONS.items():
        node = resource_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "./statBases/Mass"), expected["mass"], f"{name} Mass")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flam"], f"{name} Flammability")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(PARTS_BUILDING_XML))
    assert_equal(set(building_defs), set(PARTS_BUILDING_EXPECTATIONS), "零件建筑 defName 集合")
    for name, expected in PARTS_BUILDING_EXPECTATIONS.items():
        node = building_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "thingClass"), "Building_WorkTable", f"{name} thingClass")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(li_values(node, "stuffCategories"), ["Metallic"], f"{name} 材料分类")
        assert_equal(child_text(node, "costStuffCount"), expected["stuff_count"], f"{name} 材料数量")
        cost_list = {child.tag: (child.text or "").strip() for child in node.findall("./costList/*")}
        assert_equal(cost_list, expected["costs"], f"{name} 成本列表")
        assert_equal(child_text(node, "./statBases/WorkToBuild"), expected["work_build"], f"{name} 建造工作量")
        assert_equal(child_text(node, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(node, "size"), expected["size"], f"{name} 尺寸")
        speed = node.findtext("./statBases/WorkTableWorkSpeedFactor")
        speed = speed.strip() if speed else None
        assert_equal(speed, expected["speed"], f"{name} 工作台速度因子")
        assert_equal(li_values(node, "recipes"), expected["recipes"], f"{name} 配方列表")
        assert_equal(li_values(node, "researchPrerequisites"), expected["research"], f"{name} 研究前置")
        assert_equal(child_text(node, "constructionSkillPrerequisite"), expected["construction_skill"], f"{name} 建造技能")
        comp_classes = [li.attrib.get("Class") for li in node.findall("./comps/li")]
        if expected["power"] is None:
            if "CompProperties_Power" in comp_classes:
                fail(f"{name} 不应包含电力组件")
        else:
            if "CompProperties_Power" not in comp_classes:
                fail(f"{name} 缺少电力组件")
            power_comp = next(li for li in node.findall("./comps/li") if li.attrib.get("Class") == "CompProperties_Power")
            assert_equal(child_text(power_comp, "basePowerConsumption"), expected["power"], f"{name} 功耗")

    recipe_defs = def_map(parse_xml(PARTS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PARTS_RECIPE_EXPECTATIONS), "零件配方 defName 集合")

    resource_names = (
        set(resource_defs)
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(ADVANCED_ALLOY_RESOURCE_XML)))
    )
    output_names = set(resource_defs) | VANILLA_THINGS
    building_recipes = {name: li_values(node, "recipes") for name, node in building_defs.items()}

    for name, expected in PARTS_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "GeneralLaborSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), "Smith", f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), "Recipe_Machining", f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        if expected["skill"] is None:
            if recipe.find("skillRequirements") is not None:
                fail(f"{name} 不应有技能要求")
        else:
            assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载，不应写 recipeUsers")
        actual_benches = sorted(bench for bench, recipes in building_recipes.items() if name in recipes)
        assert_equal(actual_benches, sorted(expected["benches"]), f"{name} 挂载工作台")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in output_names:
            fail(f"{name} 输出不是零件资源: {expected['output']}")

    for path in PARTS_ITEM_TEXTURES + PARTS_BUILDING_TEXTURES:
        assert_png(path)

    for path in (PARTS_RESOURCE_XML, PARTS_BUILDING_XML, PARTS_RECIPE_XML, PARTS_RESEARCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    for path in (PARTS_RESOURCE_XML, PARTS_BUILDING_XML):
        text = read_text(path)
        for forbidden in ("SolidMetallic", "StrongMetallic", "RuggedMetallic", "HeavyMetallic"):
            if forbidden in text:
                fail(f"{path} 不得使用 RimWorld 1.7 不存在的 {forbidden} 分类")
    if "SmithingSpeed" in read_text(PARTS_RECIPE_XML):
        fail("零件配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed")
    print("parts: OK")


def validate_coal() -> None:
    require_files([COAL_RESOURCE_XML, COAL_BUILDING_XML, *COAL_ITEM_TEXTURES])

    defs = def_map(parse_xml(COAL_RESOURCE_XML))
    assert_equal(set(defs), set(COAL_RESOURCE_EXPECTATIONS), "煤炭资源 defName 集合")
    for name, expected in COAL_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), "Medieval", f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(COAL_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableCoal"}, "煤炭建筑 defName 集合")
    vein = building_defs["MMT_MineableCoal"]
    assert_equal(vein.attrib.get("ParentName"), "RockBase", "煤矿脉父类")
    assert_equal(child_text(vein, "label"), "煤矿", "煤矿脉 label")
    assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", "煤矿脉 texPath")
    assert_equal(child_text(vein, "./statBases/MaxHitPoints"), "1500", "煤矿脉耐久")
    assert_equal(child_text(vein, "./building/isResourceRock"), "true", "煤矿脉 isResourceRock")
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_Coal", "煤矿脉 mineableThing")
    assert_equal(child_text(vein, "./building/mineableYield"), "75", "煤矿脉 mineableYield")
    assert_equal(child_text(vein, "./building/veinMineable"), "true", "煤矿脉 veinMineable")

    for path in COAL_ITEM_TEXTURES:
        assert_png(path)

    for path in (COAL_RESOURCE_XML, COAL_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("coal: OK")


def validate_sulfur() -> None:
    require_files([SULFUR_RESOURCE_XML, SULFUR_BUILDING_XML, *SULFUR_ITEM_TEXTURES])

    defs = def_map(parse_xml(SULFUR_RESOURCE_XML))
    assert_equal(set(defs), set(SULFUR_RESOURCE_EXPECTATIONS), "硫磺资源 defName 集合")
    for name, expected in SULFUR_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), "Industrial", f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(SULFUR_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableSulfur"}, "硫磺建筑 defName 集合")
    vein = building_defs["MMT_MineableSulfur"]
    assert_equal(vein.attrib.get("ParentName"), "RockBase", "硫磺矿脉父类")
    assert_equal(child_text(vein, "label"), "硫磺矿脉", "硫磺矿脉 label")
    assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", "硫磺矿脉 texPath")
    assert_equal(child_text(vein, "./statBases/MaxHitPoints"), "1500", "硫磺矿脉耐久")
    assert_equal(child_text(vein, "./building/isResourceRock"), "true", "硫磺矿脉 isResourceRock")
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_Sulfur", "硫磺矿脉 mineableThing")
    assert_equal(child_text(vein, "./building/mineableYield"), "75", "硫磺矿脉 mineableYield")
    assert_equal(child_text(vein, "./building/veinMineable"), "true", "硫磺矿脉 veinMineable")

    for path in SULFUR_ITEM_TEXTURES:
        assert_png(path)

    for path in (SULFUR_RESOURCE_XML, SULFUR_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("sulfur: OK")


def validate_precious() -> None:
    require_files([PRECIOUS_RESOURCE_XML, PRECIOUS_BUILDING_XML, PRECIOUS_RECIPE_XML, METAL_BUILDING_XML, *PRECIOUS_ITEM_TEXTURES])

    defs = def_map(parse_xml(PRECIOUS_RESOURCE_XML))
    assert_equal(set(defs), set(PRECIOUS_RESOURCE_EXPECTATIONS), "金银资源 defName 集合")
    for name, expected in PRECIOUS_RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), "0", f"{name} Flammability")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), "Medieval", f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(PRECIOUS_BUILDING_XML))
    assert_equal(set(building_defs), set(PRECIOUS_MINEABLE_EXPECTATIONS), "金银建筑 defName 集合")
    for name, expected in PRECIOUS_MINEABLE_EXPECTATIONS.items():
        vein = building_defs[name]
        assert_equal(vein.attrib.get("ParentName"), "RockBase", f"{name} 父类")
        assert_equal(child_text(vein, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", f"{name} texPath")
        assert_equal(child_text(vein, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(vein, "./building/isResourceRock"), "true", f"{name} isResourceRock")
        assert_equal(child_text(vein, "./building/mineableThing"), expected["yield_thing"], f"{name} mineableThing")
        assert_equal(child_text(vein, "./building/mineableYield"), "75", f"{name} mineableYield")
        assert_equal(child_text(vein, "./building/veinMineable"), "true", f"{name} veinMineable")

    recipe_defs = def_map(parse_xml(PRECIOUS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PRECIOUS_RECIPE_EXPECTATIONS), "金银配方 defName 集合")
    furnace_recipes = li_values(def_map(parse_xml(METAL_BUILDING_XML))["MMT_HandFurnace"], "recipes")
    resource_names = set(defs)
    for name, expected in PRECIOUS_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "SmeltingSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), "Smelt", f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), "Recipe_Smelt", f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        ingredients = recipe.findall("./ingredients/li")
        if len(ingredients) != 1:
            fail(f"{name} 必须且只能有一个输入")
        assert_equal(li_values(ingredients[0], "./filter/thingDefs"), [expected["ore"]], f"{name} 输入引用")
        assert_equal(child_text(ingredients[0], "count"), expected["ore_count"], f"{name} 输入数量")
        assert_equal(li_values(recipe, "./fixedIngredientFilter/thingDefs"), [expected["ore"]], f"{name} 固定输入列表")

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

        if expected["ore"] not in resource_names:
            fail(f"{name} 使用未知资源: {expected['ore']}")
        if expected["output"] not in VANILLA_THINGS:
            fail(f"{name} 输出不是原版资源: {expected['output']}")

    for path in PRECIOUS_ITEM_TEXTURES:
        assert_png(path)

    for path in (PRECIOUS_RESOURCE_XML, PRECIOUS_BUILDING_XML, PRECIOUS_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("precious: OK")


def validate_infrastructure() -> None:
    require_files([INFRA_RESEARCH_TAB_XML, INFRA_DESIGNATION_XML, INFRA_CATEGORIES_XML, INFRA_PATCH_XML])

    tab = parse_xml(INFRA_RESEARCH_TAB_XML).find("ResearchTabDef")
    assert_equal(child_text(tab, "defName"), "MMT_ResearchTab", "研究标签 defName")
    assert_equal(child_text(tab, "label"), "MMT 资源生产", "研究标签 label")

    research_count = 0
    for path in sorted((ROOT / INFRA_RESEARCH_DIR).glob("MMT_Research_*.xml")):
        rel = path.relative_to(ROOT).as_posix()
        for node in parse_xml(rel).findall("ResearchProjectDef"):
            research_count += 1
            assert_equal(child_text(node, "tab"), "MMT_ResearchTab", f"{path.name} 研究 {child_text(node, 'defName')} tab")
    assert_equal(research_count, INFRA_RESEARCH_TOTAL, "研究项目总数")

    designation = parse_xml(INFRA_DESIGNATION_XML).find("DesignationCategoryDef")
    assert_equal(child_text(designation, "defName"), "MMT_Production", "建筑分类 defName")
    assert_equal(child_text(designation, "label"), "MMT 生产", "建筑分类 label")

    worktable_count = 0
    for path in sorted((ROOT / INFRA_BUILDING_DIR).glob("MMT_Buildings_*.xml")):
        rel = path.relative_to(ROOT).as_posix()
        for node in parse_xml(rel).findall("ThingDef"):
            if node.findtext("defName") is None:
                continue
            if node.get("ParentName") == "RockBase":
                if node.findtext("designationCategory") is not None:
                    fail(f"{rel} 矿脉 {node.findtext('defName')} 不应有 designationCategory")
                continue
            worktable_count += 1
            assert_equal(child_text(node, "designationCategory"), "MMT_Production", f"{rel} {node.findtext('defName')} designationCategory")
    assert_equal(worktable_count, INFRA_WORKTABLE_TOTAL, "工作台建筑总数")

    categories = def_map(parse_xml(INFRA_CATEGORIES_XML))
    assert_equal(set(categories), set(INFRA_EXPECTED_CATEGORIES), "类别 defName 集合")
    for name, expected_parent in INFRA_EXPECTED_CATEGORIES.items():
        assert_equal(child_text(categories[name], "parent"), expected_parent, f"{name} parent")

    patch_text = read_text(INFRA_PATCH_XML)
    if "PatchOperationReplace" in patch_text or "PatchOperationRemove" in patch_text:
        fail("类别 patch 只允许 PatchOperationAdd（追加语义）")
    patch = parse_xml(INFRA_PATCH_XML)
    ops = patch.findall("Operation")
    seen = {}
    for op in ops:
        assert_equal(op.attrib.get("Class"), "PatchOperationAdd", "patch 操作类型")
        xpath = child_text(op, "xpath")
        target = xpath.split('defName="')[1].split('"')[0]
        value = child_text(op, "value/li")
        if value not in INFRA_EXPECTED_CATEGORIES:
            fail(f"patch 引用了不存在的类别: {value}")
        seen[target] = value
    assert_equal(seen, INFRA_PATCH_TARGETS, "patch 目标与类别映射")
    print("infrastructure: OK")


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
    validate_infrastructure()
    validate_resources()
    validate_building()
    validate_recipes()
    validate_concrete()
    validate_wood()
    validate_metal()
    validate_precious()
    validate_coal()
    validate_sulfur()
    validate_petrochem()
    validate_electronics()
    validate_electric()
    validate_electric_wood()
    validate_electric_petrochem()
    validate_electric_electronics()
    validate_advanced_alloys()
    validate_parts()
    validate_all_xml()


SUITES = {
    "base": validate_base,
    "infrastructure": validate_infrastructure,
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
    "electronics-resources": validate_electronics_resources,
    "electronics-building": validate_electronics_building,
    "electronics-recipes": validate_electronics_recipes,
    "electronics": validate_electronics,
    "electric": validate_electric,
    "electric-wood": validate_electric_wood,
    "electric-petrochem": validate_electric_petrochem,
    "electric-electronics": validate_electric_electronics,
    "advanced-alloys": validate_advanced_alloys,
    "parts": validate_parts,
    "coal": validate_coal,
    "sulfur": validate_sulfur,
    "precious": validate_precious,
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
