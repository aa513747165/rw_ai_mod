#!/usr/bin/env python3
"""myModeTest Mod 静态验证工具。"""
from __future__ import annotations

import argparse
import json
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
PRECIOUS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Precious.xml"
VANILLA_VEIN_PATCH_XML = "1.7/Patches/MMT_Patch_VanillaVeins.xml"
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
CERAMICS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml"
CERAMICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml"
CERAMICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml"
STUFFCATEGORY_XML = "1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml"
ASH_PATCH_XML = "1.7/Patches/MMT_Patch_AshRecipes.xml"
AGRICULTURE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Agriculture.xml"
AGRICULTURE_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml"
AGRICULTURE_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Agriculture.xml"
AGRICULTURE_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Agriculture.xml"

AGRICULTURE_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/Dirt.png",
    "1.7/Textures/Things/Item/MMT/Compost.png",
    "1.7/Textures/Things/Item/MMT/Fertilizer.png",
    "1.7/Textures/Things/Item/MMT/Salt.png",
]

AGRICULTURE_BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_SaltMine.png",
    *[f"1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
]

CERAMICS_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/SoftClay.png",
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/ClayBrick.png",
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ceramics.png",
    "1.7/Textures/Things/Item/MMT/MetalCan.png",
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash/MMT_Ash_{s}.png" for s in ("a", "b", "c")],
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}/MMT_{ore}_{s}.png" for ore in ("Anglesite", "AluminiumOre") for s in ("a", "b", "c")],
]

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
    "MMT_SilverOre": {"label": "白银", "value": "1.0", "stack": "1800", "tex": "Things/Item/MMT/MMT_Ore/MMT_SilverOre"},
    "MMT_GoldOre": {"label": "金矿", "value": "25.0", "stack": "300", "tex": "Things/Item/MMT/MMT_Ore/MMT_GoldOre"},
}

# 原版矿脉改造（HSK 做法：不删 def，patch 替换产出物）。MMT 不再自带铁/银/金矿脉 def，
# 玻璃钢矿脉改造为稀有钛铁来源；零件矿脉照 HSK 保留原版不动。
VANILLA_VEIN_EXPECTATIONS = {
    "MineableSteel": {"label": "铁矿", "yield_thing": "MMT_IronOre", "hp": "1500", "yield": "75", "commonality": "1.0", "lump": "15~25"},
    "MineableSilver": {"label": "银矿", "yield_thing": "MMT_SilverOre", "hp": "1500", "yield": "75", "commonality": "1.0", "lump": "10~16"},
    "MineableGold": {"label": "金矿", "yield_thing": "MMT_GoldOre", "hp": "1500", "yield": "75", "commonality": "0.5", "lump": "8~14"},
    "MineablePlasteel": {"label": "钛铁矿", "yield_thing": "MMT_IlmeniteOre", "hp": "10000", "yield": "75", "commonality": "0.05", "lump": "2~8"},
}
VANILLA_VEIN_KEEP = ["MineableComponentsIndustrial"]  # 照 HSK 保留原版，不得出现在 patch 中

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

VANILLA_CATEGORIES = {"StoneBlocks", "StoneChunks", "Metallic", "MeatRaw", "AnimalProductRaw", "PlantFoodRaw", "FoodRaw"}
MMT_CATEGORIES = {"MMT_MetallicBar"}
VANILLA_WORKTABLES = {"TableStonecutter", "TableMachining", "ButcherSpot", "TableButcher", "FueledStove", "ElectricStove"}
VANILLA_RESEARCH = {"Stonecutting", "Smithing", "BiofuelRefining", "MicroelectronicsBasics", "Machining", "Gunsmithing", "BlowbackOperation", "GasOperation", "PrecisionRifling", "HeavyTurrets", "Mortars", "Fabrication", "ChargedShot", "ComplexClothing", "AdvancedFabrication", "Firefoam", "Pemmican", "TreeSowing"}
VANILLA_THINGS = {"ComponentIndustrial", "ComponentSpacer", "WoodLog", "Steel", "Silver", "Gold", "Plasteel", "Hay", "Uranium", "Chemfuel", "AIPersonaCore", "Hyperweave", "Bioferrite", "Apparel_ShieldBelt", "Cloth", "DevilstrandCloth", "Synthread", "SmokeleafLeaves", "Campfire", "FueledStove", "ElectricStove", "Brewery", "DrugLab"}

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

CONCRETE_BUILDING_RECIPES = ["MMT_MixConcrete_Hand", "MMT_MakeDirt_Hand"]

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
    "MMT_MixConcrete_Electric": {
        "work": "600",
        "inputs": [
            ("thingDefs", "MMT_Sand", "45"),
            ("thingDefs", "MMT_CrushedStone", "30"),
        ],
        "output": "MMT_Concrete",
        "output_count": "15",
        "users": [],
        "research": "MMT_ConcreteAdvanced",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "skill": "6",
        "building_mounted": False,
    },
    "MMT_MakeReinforcedConcrete_Electric": {
        "work": "1200",
        "inputs": [
            ("categories", "MMT_MetallicBar", "20"),
            ("thingDefs", "MMT_Concrete", "20"),
        ],
        "output": "MMT_ReinforcedConcrete",
        "output_count": "16",
        "users": [],
        "research": None,
        "speed": "GeneralLaborSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
        "skill": "7",
        "building_mounted": False,
    },
    "MMT_MakeDirt_Electric": {
        "work": "1500",
        "inputs": [
            ("thingDefs", "MMT_Sand", "20"),
            ("thingDefs", "MMT_Fertilizer", "10"),
        ],
        "output": "MMT_Dirt",
        "output_count": "30",
        "users": [],
        "research": "MMT_ConcreteAdvanced",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "skill": None,
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
    "MMT_BambooPlank": {
        "label": "竹板",
        "value": "1.5",
        "stack": "300",
        "tex": "Things/Item/MMT/MMT_BambooPlank",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
        "stuff": False,
        "stuff_lite": True,
    },
    "MMT_RedWoodPlank": {
        "label": "红木木板",
        "value": "3.5",
        "stack": "300",
        "tex": "Things/Item/MMT/MMT_RedWoodPlank",
        "graphic": "Graphic_StackCount",
        "tech": "Neolithic",
        "stuff": False,
        "stuff_lite": True,
    },
}

SAWMILL_RECIPES = ["MMT_MakeWoodPlanks_Hand", "MMT_MakeKindling_Hand", "MMT_MakeBambooPlanks_Hand", "MMT_MakeRedWoodPlanks_Hand"]
CHARCOAL_KILN_RECIPES = ["MMT_MakeCharcoal_Hand", "MMT_MakeCeramics", "MMT_MakeClayBricks", "MMT_BurnThings"]

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
    "MMT_MakeBambooPlanks_Hand": {
        "work": "1200",
        "input": "MMT_Bamboo",
        "input_count": "10",
        "output": "MMT_BambooPlank",
        "output_count": "15",
        "research": "MMT_Woodworking",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "skill": "6",
        "learn": "0.8",
        "building": "MMT_HandSawmill",
    },
    "MMT_MakeRedWoodPlanks_Hand": {
        "work": "1200",
        "input": "MMT_RedWoodLog",
        "input_count": "10",
        "output": "MMT_RedWoodPlank",
        "output_count": "15",
        "research": "MMT_Woodworking",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "skill": "3",
        "learn": "0.8",
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
    "MMT_IronOre": {"label": "铁矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_IronOre", "value": "2.0", "stack": "600"},
    "MMT_CopperOre": {"label": "铜矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_CopperOre", "value": "2.0", "stack": "600"},
    "MMT_TinOre": {"label": "锡矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_TinOre", "value": "2.0", "stack": "600"},
    "MMT_Anglesite": {"label": "铅矾矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_Anglesite", "value": "4", "stack": "300"},
    "MMT_AluminiumOre": {"label": "铝土矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_AluminiumOre", "value": "2", "stack": "600"},
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
    "MMT_MakeParaffins",
    "MMT_MakeSulphates",
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
    "MMT_Silicon": {"label": "硅", "value": "4.0", "stack": "300", "flammable": "0", "tex": "Things/Item/MMT/MMT_Electronics/Silicon", "tech": "Industrial", "category": "ResourcesRaw"},
    "MMT_ElectronicParts": {"label": "电子元件", "value": "25.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/ElectronicParts", "tech": "Industrial", "category": "MMT_ElectronicsCat"},
    "MMT_IntegratedCircuit": {"label": "集成电路", "value": "55.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/IntegratedCircuit", "tech": "Industrial", "category": "MMT_ElectronicsCat"},
    "MMT_Microchip": {"label": "微芯片集成电路", "value": "100.0", "stack": "25", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/Microchip", "tech": "Spacer", "category": "MMT_ElectronicsCat"},
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
    "MMT_MakeSilicon_Bulk",
    "MMT_SmeltLeadBar_Electric",
    "MMT_SmeltAluminiumBar_Electric",
    "MMT_MakeFerrosiliconAlloy",
    "MMT_MakeMetalCan",
    "MMT_MakeCeramics",
    "MMT_MakeClayBricks",
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
    "MMT_MakeSilicon_Bulk": {
        "work": "1800",
        "inputs": [("thingDefs", "MMT_Sand", "200")],
        "output": "MMT_Silicon",
        "output_count": "20",
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
        "recipes": ["MMT_MakeWoodPlanks_Electric", "MMT_MakeKindling_Electric", "MMT_MakeBambooPlanks_Electric", "MMT_MakeRedWoodPlanks_Electric"],
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
    "MMT_MakeBambooPlanks_Electric": {
        "work": "1200",
        "input": "MMT_Bamboo",
        "input_count": "20",
        "output": "MMT_BambooPlank",
        "output_count": "30",
        "skill": "9",
        "learn": "0.8",
        "speed": "GeneralLaborSpeed",
        "effect": "MakeWoodPlanks_Hand",
        "sound": "Recipe_MakeWoodPlanks_Hand",
        "building": "MMT_ElectricSawmill",
    },
    "MMT_MakeRedWoodPlanks_Electric": {
        "work": "1200",
        "input": "MMT_RedWoodLog",
        "input_count": "20",
        "output": "MMT_RedWoodPlank",
        "output_count": "30",
        "skill": "6",
        "learn": "0.8",
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
        "building": "MMT_ElectricCharcoalKiln",    },
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
    "MMT_MakeParaffins",
    "MMT_MakeSulphates",
    "MMT_MakeCarbonAlloy",
    "MMT_MakeOxidizedChemfuel",
    "MMT_ExtractBiomatterCorpse",
    "MMT_ExtractBiomatterMeat",
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


PETROCHEM_EXT_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_PetrochemExt.xml"
PETROCHEM_EXT_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_PetrochemExt.xml"
PETROCHEM_EXT_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_PetrochemExt.xml"
PETROCHEM_EXT_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_PetrochemExt.xml"

PETROCHEM_EXT_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_PetroExt/{name}.png"
    for name in ("Paraffins", "Sulphates", "Surfactant", "Compaste", "Powder", "Napalm", "Nitre", "OxidizedChemfuel")
]

PETROCHEM_EXT_BUILDING_TEXTURES = [
    *[
        f"1.7/Textures/Things/Building/Production/MMT_Loom/MMT_Loom_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
    *[
        f"1.7/Textures/Things/Building/Production/MMT_AmmoBench/MMT_AmmoBench_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
]

PETROCHEM_EXT_RESOURCE_EXPECTATIONS = {
    "MMT_Paraffins": {"label": "石蜡", "value": "14.0", "stack": "150", "flammable": "3", "tech": "Industrial", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Paraffins"},
    "MMT_Sulphates": {"label": "硫酸盐", "value": "7.0", "stack": "300", "flammable": "2", "tech": "Industrial", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Sulphates"},
    "MMT_Surfactant": {"label": "表面活性剂", "value": "0.4", "stack": "150", "flammable": "3", "tech": "Industrial", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Surfactant"},
    "MMT_Compaste": {"label": "复合粘剂", "value": "5.0", "stack": "300", "flammable": "1.5", "tech": "Industrial", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Compaste"},
    "MMT_Powder": {"label": "火药", "value": "25.0", "stack": "150", "flammable": "14", "tech": "Industrial", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Powder"},
    "MMT_Napalm": {"label": "凝固汽油", "value": "5.0", "stack": "300", "flammable": "8", "tech": "Industrial", "categories": ["MMT_FuelCat"], "tex": "Things/Item/MMT/MMT_PetroExt/Napalm"},
    "MMT_Nitre": {"label": "硝石", "value": "3.0", "stack": "300", "flammable": "5", "tech": "Medieval", "categories": ["ResourcesRaw"], "tex": "Things/Item/MMT/MMT_PetroExt/Nitre"},
    "MMT_OxidizedChemfuel": {"label": "氧化化合燃料", "value": "9.0", "stack": "375", "flammable": "12", "tech": "Spacer", "categories": ["MMT_FuelCat"], "tex": "Things/Item/MMT/MMT_PetroExt/OxidizedChemfuel"},
    "MMT_CarbonAlloy": {"label": "碳纤维锭", "value": "40.0", "stack": "75", "flammable": "0", "tech": "Industrial", "categories": ["MMT_USLDHBar"], "tex": "Things/Item/MMT/MMT_Ingot"},
}

PETROCHEM_EXT_RESEARCH_EXPECTATIONS = {
    "MMT_AdvancedPetrochemistry": {"label": "高级石化工艺", "cost": "1500", "tech": "Industrial", "prereqs": ["MMT_ElectricPetrochemistry"], "x": "16.00", "y": "1.00"},
    "MMT_OxidizedFuel": {"label": "氧化燃料", "cost": "2500", "tech": "Spacer", "prereqs": ["MMT_AdvancedPetrochemistry"], "x": "17.00", "y": "2.00"},
}

PETROCHEM_EXT_LOOM_RECIPES = ["MMT_MakeCompaste", "MMT_MakeCloth", "MMT_MakeFlaxcloth", "MMT_MakeHempCloth", "MMT_MakeDevilstrand", "MMT_MakeSynthread", "MMT_MakeVelour", "MMT_MakeKevlar", "MMT_MakeHyperweave", "MMT_MakeChitinPlating"]
PETROCHEM_EXT_AMMO_BENCH_RECIPES = ["MMT_MakeSulfur", "MMT_MakePowder", "MMT_MakeNapalm"]

PETROCHEM_EXT_RECIPE_EXPECTATIONS = {
    "MMT_MakeParaffins": {"work": "1000", "inputs": [("thingDefs", "MMT_CrudeOil", "20")], "output": "MMT_Paraffins", "output_count": "10", "skill": "6", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "MMT_AdvancedPetrochemistry"},
    "MMT_MakeSulphates": {"work": "800", "inputs": [("thingDefs", "MMT_Sulfur", "10")], "output": "MMT_Sulphates", "output_count": "10", "skill": "6", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "MMT_AdvancedPetrochemistry"},
    "MMT_MakeCarbonAlloy": {"work": "2800", "inputs": [("thingDefs", "MMT_SyntheticFibers", "10"), ("thingDefs", "MMT_Carbon", "10")], "output": "MMT_CarbonAlloy", "output_count": "10", "skill": "15", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "MMT_AdvancedPetrochemistry"},
    "MMT_MakeCompaste": {"work": "3200", "inputs": [("thingDefs", "Chemfuel", "10"), ("thingDefs", "MMT_Rubber", "5"), ("thingDefs", "MMT_Paraffins", "2")], "output": "MMT_Compaste", "output_count": "20", "skill": "5", "speed": "DrugCookingSpeed", "effect": None, "sound": "Recipe_Drug", "research": "MMT_AdvancedPetrochemistry"},
    "MMT_MakeSurfactant": {"work": "4000", "inputs": [("thingDefs", "MMT_Salt", "10"), ("thingDefs", "MMT_Sulfur", "15"), ("categories", "Alcohol", "15")], "output": "MMT_Surfactant", "output_count": "50", "skill": "8", "speed": "DrugCookingSpeed", "effect": None, "sound": "Recipe_Drug", "research": "MMT_AdvancedPetrochemistry", "users": ["Brewery", "DrugLab"]},
    "MMT_MakeSurfactant_Compaste": {"work": "4000", "inputs": [("thingDefs", "MMT_Compaste", "15"), ("thingDefs", "MMT_Polymers", "25")], "output": "MMT_Surfactant", "output_count": "50", "skill": "12", "skill2": ("Intellectual", "4"), "speed": "DrugCookingSpeed", "effect": None, "sound": "Recipe_Drug", "research": "MMT_AdvancedPetrochemistry", "users": ["DrugLab"]},
    "MMT_MakeSurfactant_CoconutSubstitute": {"work": "4000", "inputs": [("thingDefs", "MMT_Salt", "10"), ("thingDefs", "MMT_Sulfur", "15"), ("thingDefs", "MMT_RawCoconut", "25")], "output": "MMT_Surfactant", "output_count": "50", "skill": "12", "skill2": ("Intellectual", "4"), "speed": "DrugCookingSpeed", "effect": None, "sound": "Recipe_Drug", "research": "Firefoam", "users": ["Brewery", "DrugLab"]},
    "MMT_MakeSulfur": {"work": "1200", "inputs": [("thingDefs", "MMT_IronOre", "35")], "output": "MMT_Sulfur", "output_count": "10", "skill": None, "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": None},
    "MMT_MakePowder": {"work": "3000", "inputs": [("thingDefs", "MMT_Nitre", "75"), ("categories", "MMT_Coal", "20"), ("thingDefs", "MMT_Sulfur", "20")], "output": "MMT_Powder", "output_count": "20", "skill": "3", "speed": "GeneralLaborSpeed", "effect": "Smith", "sound": "Recipe_Smith", "research": None, "target_count_adjustment": "5"},
    "MMT_MakeNapalm": {"work": "1200", "inputs": [("thingDefs", "Chemfuel", "30"), ("thingDefs", "MMT_Polymers", "15")], "output": "MMT_Napalm", "output_count": "10", "skill": "5", "speed": "GeneralLaborSpeed", "effect": "Smith", "sound": "Recipe_Smith", "research": None},
    "MMT_MakeOxidizedChemfuel": {"work": "10000", "inputs": [("thingDefs", "Chemfuel", "100"), ("thingDefs", "MMT_Nitre", "15"), ("thingDefs", "MMT_Sulphates", "15"), ("thingDefs", "MMT_Polymers", "5")], "output": "MMT_OxidizedChemfuel", "output_count": "75", "skill": "12", "speed": "DrugSynthesisSpeed", "effect": None, "sound": "Recipe_Drug", "research": "MMT_OxidizedFuel"},
}

ELECTRIC_ELECTRONICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

# === B8 织物 + 作物 + 武器部件 ===
CROPS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Crops.xml"
CROPS_PLANT_XML = "1.7/Defs/ThingDefs_Plants/MMT_Plants_Cultivated.xml"
CROPS_PATCH_XML = "1.7/Patches/MMT_Patch_CropHarvest.xml"
STATS_XML = "1.7/Defs/StatDefs/MMT_Stats_WorkRecipes.xml"
INSECT_CHITIN_PATCH_XML = "1.7/Patches/MMT_Patch_InsectChitin.xml"
TEXTILES_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Textiles.xml"
TEXTILES_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Textiles.xml"
WEAPON_PARTS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_WeaponParts.xml"
WEAPON_PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_WeaponParts.xml"
WEAPON_PARTS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_WeaponParts.xml"
TEXTILES_WEAPONS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_TextilesWeapons.xml"

CROPS_RESOURCE_EXPECTATIONS = {
    "MMT_RawCotton": {"label": "原棉", "value": "1.5", "stack": "300", "categories": ["PlantMatter"]},
    "MMT_RawFlax": {"label": "生亚麻", "value": "3.5", "stack": "300", "categories": ["PlantMatter"]},
    "MMT_RawDevilstrand": {"label": "生恶魔菇", "value": "6.4", "stack": "150", "categories": ["PlantMatter"]},
    "MMT_RawCaoutchouc": {"label": "天然橡胶", "value": "2", "categories": ["PlantMatter"], "mass": "0.1"},
    "MMT_Bamboo": {"label": "竹节", "value": "2.0", "stack": "300", "categories": ["ResourcesRaw"]},
    "MMT_RedWoodLog": {"label": "红木", "value": "2.0", "stack": "300", "categories": ["ResourcesRaw"]},
    "MMT_RawCoconut": {"label": "椰子", "value": "6", "categories": ["PlantFoodRaw"], "mass": "0.5", "food": True},
    "MMT_WildRose": {"label": "野玫瑰", "value": "2", "stack": "75", "categories": ["PlantFoodRaw"], "mass": "0.05", "food": True},
    "MMT_Hypericum": {"label": "金丝桃", "value": "2", "stack": "75", "categories": ["PlantFoodRaw"], "mass": "0.05", "food": True},
    "MMT_MintLeaves": {"label": "薄荷叶", "value": "2", "stack": "75", "categories": ["PlantFoodRaw"], "mass": "0.05", "food": True},
}

CROPS_PLANT_EXPECTATIONS = {
    "MMT_PlantFlax": {"harvest": "MMT_RawFlax", "yield": "9", "grow": "8.1", "sow": ["Ground", "Hydroponic"], "skill": "5"},
    "MMT_PlantWildRose": {"harvest": "MMT_WildRose", "yield": "6", "grow": "8", "sow": ["Hydroponic"], "skill": "12"},
    "MMT_PlantHypericum": {"harvest": "MMT_Hypericum", "yield": "4", "grow": "6", "sow": ["Hydroponic"], "skill": "12"},
    "MMT_PlantMint": {"harvest": "MMT_MintLeaves", "yield": "5", "grow": "6.5", "sow": ["Hydroponic"], "skill": "12"},
    "MMT_PlantHevea": {"harvest": "MMT_RawCaoutchouc", "yield": "20", "grow": "6.5", "sow": ["Ground"], "skill": "8"},
    "MMT_PlantRedwood": {"harvest": "MMT_RedWoodLog", "yield": "225", "grow": "85", "sow": [], "skill": None},
}

CROPS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Crops/{name}.png"
    for name in ("MMT_RawCotton", "MMT_RawFlax", "MMT_RawDevilstrand", "MMT_RawCaoutchouc", "MMT_RawCoconut", "MMT_WildRose", "MMT_Hypericum", "MMT_MintLeaves", "MMT_Bamboo")
] + [f"1.7/Textures/Things/Item/MMT/MMT_Crops/MMT_RedWoodLog/MMT_RedWoodLog_{s}.png" for s in "abc"]
CROPS_PLANT_TEXTURE_DIRS = [
    f"1.7/Textures/Things/Plant/MMT/{name}"
    for name in ("MMT_PlantFlax", "MMT_PlantFlax_Immature", "MMT_PlantWildRose", "MMT_PlantHypericum", "MMT_PlantMint", "MMT_PlantHevea", "MMT_PlantHevea_Leafless", "MMT_PlantRedwood")
]

TEXTILES_RESOURCE_EXPECTATIONS = {
    "MMT_Flaxcloth": {"label": "亚麻布", "value": "6", "stack": "150", "categories": ["MMT_BTextiles"], "class": "Graphic_StackCount"},
    "MMT_HempCloth": {"label": "大麻布", "value": "3", "stack": "300", "categories": ["MMT_BTextiles"], "class": "Graphic_StackCount"},
    "MMT_Velour": {"label": "丝绒", "value": "12.0", "stack": "300", "categories": ["Textiles"], "class": "Graphic_Single"},
    "MMT_Kevlar": {"label": "凯夫拉", "value": "25", "stack": "150", "categories": ["Textiles"], "class": "Graphic_StackCount"},
    "MMT_ChitinPlating": {"label": "甲壳板", "value": "35", "stack": "75", "categories": ["Textiles"], "class": "Graphic_Single"},
    "MMT_Chitin": {"label": "甲壳", "value": "7", "stack": "150", "parent": "LeatherBase"},
    "MMT_Tallow": {"label": "兽脂", "value": "2", "stack": "75", "categories": ["MMT_FuelCat"], "class": "Graphic_Single"},
    "MMT_PaintingSupplies": {"label": "绘画用品", "value": "50", "stack": "10", "categories": ["Manufactured"], "class": "Graphic_Single"},
    "MMT_Dye": {"label": "染料", "value": "1.5", "stack": "150", "categories": ["Manufactured"], "class": "Graphic_Single"},
}

TEXTILES_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/MMT_Textiles/MMT_Velour.png",
    "1.7/Textures/Things/Item/MMT/MMT_Textiles/MMT_ChitinPlating.png",
    "1.7/Textures/Things/Item/MMT/MMT_Textiles/MMT_Tallow.png",
    "1.7/Textures/Things/Item/MMT/MMT_Textiles/MMT_PaintingSupplies.png",
    "1.7/Textures/Things/Item/MMT/MMT_Textiles/MMT_Dye.png",
    "1.7/Textures/Things/Item/MMT/MMT_BambooPlank.png",
] + [f"1.7/Textures/Things/Item/MMT/MMT_Textiles/{name}/{name}_{s}.png" for name in ("MMT_Flaxcloth", "MMT_HempCloth", "MMT_Kevlar", "MMT_Chitin") for s in "abc"] \
  + [f"1.7/Textures/Things/Item/MMT/MMT_RedWoodPlank/MMT_RedWoodPlank_{s}.png" for s in "abc"]

TEXTILES_RECIPE_EXPECTATIONS = {
    "MMT_MakeCloth": {"work": "600", "inputs": [("thingDefs", "MMT_RawCotton", "20")], "output": "Cloth", "output_count": "20", "skill": "2", "speed": "GeneralLaborSpeed", "research": "ComplexClothing", "bench": "loom"},
    "MMT_MakeFlaxcloth": {"work": "600", "inputs": [("thingDefs", "MMT_RawFlax", "20")], "output": "MMT_Flaxcloth", "output_count": "10", "skill": "4", "speed": "GeneralLaborSpeed", "research": None, "bench": "loom"},
    "MMT_MakeHempCloth": {"work": "1200", "inputs": [("thingDefs", "SmokeleafLeaves", "30")], "output": "MMT_HempCloth", "output_count": "15", "skill": "6", "speed": "GeneralLaborSpeed", "research": "ComplexClothing", "bench": "loom"},
    "MMT_MakeDevilstrand": {"work": "600", "inputs": [("thingDefs", "MMT_RawDevilstrand", "15"), ("thingDefs", "MMT_SyntheticFibers", "2")], "output": "DevilstrandCloth", "output_count": "15", "skill": "13", "speed": "GeneralLaborSpeed", "research": "AdvancedFabrication", "bench": "loom"},
    "MMT_MakeSynthread": {"work": "600", "inputs": [("thingDefs", "MMT_RawCotton", "10"), ("thingDefs", "MMT_SyntheticFibers", "3")], "output": "Synthread", "output_count": "10", "skill": "12", "speed": "GeneralLaborSpeed", "research": "AdvancedFabrication", "bench": "loom"},
    "MMT_MakeVelour": {"work": "1800", "inputs": [("thingDefs", "MMT_Compaste", "5"), ("categories", "MMT_BTextiles", "40")], "output": "MMT_Velour", "output_count": "25", "skill": "12", "speed": "GeneralLaborSpeed", "research": "AdvancedFabrication", "bench": "loom"},
    "MMT_MakeKevlar": {"work": "1800", "inputs": [("thingDefs", "MMT_Plastic", "30"), ("thingDefs", "MMT_SyntheticFibers", "15")], "output": "MMT_Kevlar", "output_count": "10", "skill": "13", "speed": "GeneralLaborSpeed", "research": "AdvancedFabrication", "bench": "loom"},
    "MMT_MakeHyperweave": {"work": "600", "inputs": [("thingDefs", "Synthread", "10"), ("thingDefs", "DevilstrandCloth", "10")], "output": "Hyperweave", "output_count": "10", "skill": "17", "speed": "GeneralLaborSpeed", "research": "MMT_SuperFabric", "bench": "loom"},
    "MMT_MakeChitinPlating": {"work": "1080", "inputs": [("thingDefs", "MMT_Chitin", "50"), ("thingDefs", "MMT_SyntheticFibers", "4"), ("thingDefs", "MMT_Compaste", "1")], "output": "MMT_ChitinPlating", "output_count": "15", "skill": "12", "speed": "GeneralLaborSpeed", "research": "AdvancedFabrication", "bench": "loom"},
    "MMT_MakeTallow": {"work": "1200", "inputs": [("categories", "MeatRaw", "30")], "output": "MMT_Tallow", "output_count": "15", "skill": ("Cooking", "3"), "speed": "CookSpeed", "research": "Pemmican", "users": ["Campfire", "FueledStove"]},
    "MMT_CookPaint": {"work": "550", "inputs": [("thingDefs", ("MMT_WildRose", "MMT_Hypericum", "MMT_MintLeaves"), "10"), ("thingDefs", ("WoodLog", "MMT_WoodPlank", "MMT_Bamboo", "MMT_BambooPlank", "MMT_RedWoodLog", "MMT_RedWoodPlank"), "2")], "output": "MMT_PaintingSupplies", "output_count": "1", "skill": "5", "speed": "CookSpeed", "research": "MMT_ArtSupplies", "users": ["ElectricStove", "FueledStove"]},
    "MMT_CookPaintDye": {"work": "550", "inputs": [("thingDefs", "MMT_Dye", "10"), ("thingDefs", ("WoodLog", "MMT_WoodPlank", "MMT_Bamboo", "MMT_BambooPlank", "MMT_RedWoodLog", "MMT_RedWoodPlank"), "2")], "output": "MMT_PaintingSupplies", "output_count": "1", "skill": "5", "speed": "CookSpeed", "research": "MMT_ArtSupplies", "users": ["ElectricStove", "FueledStove"]},
    "MMT_MakeDye": {"work": "400", "inputs": [("thingDefs", ("MMT_WildRose", "MMT_Hypericum", "MMT_MintLeaves"), "4")], "output": "MMT_Dye", "output_count": "8", "skill": "3", "speed": "CookSpeed", "research": "MMT_ArtSupplies", "users": ["ElectricStove", "FueledStove"]},
}

WEAPON_PARTS_RESOURCE_EXPECTATIONS = {
    "MMT_WeaponParts": {"label": "武器部件", "value": "65", "stack": "75", "tech": "Industrial"},
    "MMT_PistolComponent": {"label": "手枪部件", "value": "50", "stack": "75", "tech": "Industrial"},
    "MMT_SMGComponent": {"label": "冲锋枪部件", "value": "150", "stack": "25", "tech": "Industrial"},
    "MMT_ShotgunComponent": {"label": "霰弹枪部件", "value": "150", "stack": "25", "tech": "Industrial"},
    "MMT_RifleComponent": {"label": "步枪部件", "value": "200", "stack": "25", "tech": "Industrial"},
    "MMT_AdvRifleComponent": {"label": "先进步枪部件", "value": "400", "stack": "25", "tech": "Industrial"},
    "MMT_SniperComponent": {"label": "狙击枪部件", "value": "250", "stack": "25", "tech": "Industrial"},
    "MMT_AdvSniperComponent": {"label": "先进狙击枪部件", "value": "1250", "stack": "25", "tech": "Industrial"},
    "MMT_HeavyComponent": {"label": "重型武器部件", "value": "500", "stack": "25", "tech": "Industrial"},
    "MMT_CannonComponent": {"label": "火炮武器部件", "value": "600", "stack": "25", "tech": "Industrial"},
    "MMT_LauncherComponent": {"label": "发射器部件", "value": "750", "stack": "25", "tech": "Industrial"},
    "MMT_ChargedComponent": {"label": "电荷武器部件", "value": "687", "stack": "25", "tech": "Spacer"},
    "MMT_LaserComponent": {"label": "光束武器部件", "value": "1000", "stack": "25", "tech": "Spacer"},
    "MMT_PlasmaComponent": {"label": "离子武器部件", "value": "1000", "stack": "25", "tech": "Spacer"},
}

WEAPON_PARTS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_WeaponParts/{name}.png" for name in WEAPON_PARTS_RESOURCE_EXPECTATIONS
]
WEAPON_PARTS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/{b}/{b}_{side}{mask}.png"
    for b in ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable", "MMT_MechWeaponTable")
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

WEAPON_PARTS_BUILDING_EXPECTATIONS = {
    "MMT_WeaponCraftingTable": {"label": "武器加工台", "size": "(3,1)", "power": "500", "work_build": "4000", "hp": "210", "stuff_count": "140", "research": ["Machining"], "skill": "6"},
    "MMT_AdvWeaponCraftingTable": {"label": "先进武器加工台", "size": "(3,1)", "power": "500", "work_build": "6500", "hp": "250", "stuff_count": "160", "research": ["Fabrication"], "skill": "12"},
    "MMT_MechWeaponTable": {"label": "未来武器工作台", "size": "(3,1)", "power": "1800", "work_build": "11000", "hp": "260", "stuff_count": "190", "research": ["ChargedShot"], "skill": "18"},
}

SLDB_USLDB = ("MMT_SLDBar", "MMT_USLDBar")
WEAPON_PARTS_RECIPE_EXPECTATIONS = {
    "MMT_MakeWeaponParts": {"work": "1500", "inputs": [("categories", "MMT_MetallicBar", "25"), ("thingDefs", "ComponentIndustrial", "10"), ("thingDefs", "MMT_Mechanism", "5")], "output": "MMT_WeaponParts", "output_count": "5", "skill": "6", "research": "Machining", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable", "MMT_MechWeaponTable")},
    "MMT_MakePistolComponent": {"work": "1200", "inputs": [("categories", SLDB_USLDB, "6"), ("categories", "MMT_HCMBar", "3"), ("thingDefs", "ComponentIndustrial", "2"), ("thingDefs", "MMT_Wire", "1"), ("thingDefs", "MMT_Mechanism", "1")], "output": "MMT_PistolComponent", "output_count": "1", "skill": "6", "research": "Gunsmithing", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable")},
    "MMT_MakeSMGComponent": {"work": "1800", "inputs": [("categories", SLDB_USLDB, "10"), ("categories", "MMT_HCMBar", "5"), ("thingDefs", "ComponentIndustrial", "4"), ("thingDefs", "MMT_Wire", "6"), ("thingDefs", "MMT_Mechanism", "2")], "output": "MMT_SMGComponent", "output_count": "1", "skill": "9", "research": "BlowbackOperation", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable")},
    "MMT_MakeShotgunComponent": {"work": "1800", "inputs": [("categories", SLDB_USLDB, "15"), ("categories", "MMT_HCMBar", "8"), ("thingDefs", "ComponentIndustrial", "4"), ("thingDefs", "MMT_Wire", "5"), ("thingDefs", "MMT_Mechanism", "2")], "output": "MMT_ShotgunComponent", "output_count": "1", "skill": "7", "research": "GasOperation", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable")},
    "MMT_MakeRifleComponent": {"work": "2100", "inputs": [("categories", SLDB_USLDB, "20"), ("categories", "MMT_HCMBar", "10"), ("thingDefs", "ComponentIndustrial", "6"), ("thingDefs", "MMT_Wire", "8"), ("thingDefs", "MMT_Mechanism", "3")], "output": "MMT_RifleComponent", "output_count": "1", "skill": "8", "research": "GasOperation", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable")},
    "MMT_MakeSniperComponent": {"work": "2100", "inputs": [("thingDefs", "MMT_RifleComponent", "1"), ("thingDefs", "ComponentIndustrial", "6"), ("thingDefs", "MMT_Glass", "12")], "output": "MMT_SniperComponent", "output_count": "1", "skill": "10", "research": "PrecisionRifling", "benches": ("MMT_WeaponCraftingTable", "MMT_AdvWeaponCraftingTable")},
    "MMT_MakeAdvRifleComponent": {"work": "2100", "inputs": [("thingDefs", "MMT_SyntheticFibers", "5"), ("thingDefs", "MMT_Plastic", "15"), ("thingDefs", "MMT_Rubber", "15"), ("thingDefs", "MMT_RifleComponent", "1"), ("thingDefs", "MMT_AdvMechanism", "2")], "output": "MMT_AdvRifleComponent", "output_count": "1", "skill": "13", "research": "Fabrication", "benches": ("MMT_AdvWeaponCraftingTable",)},
    "MMT_MakeAdvSniperComponent": {"work": "2100", "inputs": [("thingDefs", "MMT_SyntheticFibers", "10"), ("thingDefs", "MMT_Plastic", "20"), ("thingDefs", "MMT_Rubber", "15"), ("thingDefs", "MMT_IntegratedCircuit", "4"), ("thingDefs", "MMT_SniperComponent", "2"), ("thingDefs", "MMT_AdvMechanism", "3")], "output": "MMT_AdvSniperComponent", "output_count": "1", "skill": "15", "research": "Fabrication", "benches": ("MMT_AdvWeaponCraftingTable",)},
    "MMT_MakeHeavyComponent": {"work": "2800", "inputs": [("categories", SLDB_USLDB, "25"), ("categories", "MMT_HCMBar", "15"), ("thingDefs", "ComponentIndustrial", "12"), ("thingDefs", "MMT_Wire", "15"), ("thingDefs", "MMT_Mechanism", "6")], "output": "MMT_HeavyComponent", "output_count": "1", "skill": "12", "research": "HeavyTurrets", "benches": ("MMT_AdvWeaponCraftingTable",)},
    "MMT_MakeCannonComponent": {"work": "3100", "inputs": [("categories", SLDB_USLDB, "20"), ("thingDefs", "MMT_HeavyComponent", "1"), ("thingDefs", "MMT_AdvMechanism", "5")], "output": "MMT_CannonComponent", "output_count": "1", "skill": "14", "research": "Mortars", "benches": ("MMT_AdvWeaponCraftingTable",)},
    "MMT_MakeLauncherComponent": {"work": "3100", "inputs": [("thingDefs", "MMT_IntegratedCircuit", "2"), ("thingDefs", "MMT_HeavyComponent", "1"), ("thingDefs", "MMT_AdvMechanism", "3"), ("thingDefs", "MMT_Glass", "12")], "output": "MMT_LauncherComponent", "output_count": "1", "skill": "14", "research": "MMT_Rocketry1", "benches": ("MMT_AdvWeaponCraftingTable",)},
    "MMT_MakeChargedComponent": {"work": "2100", "inputs": [("categories", "MMT_USLDBar", "10"), ("categories", "MMT_HCMBar", "20"), ("thingDefs", "MMT_MagneticMaterial", "2"), ("thingDefs", "MMT_AdvMechanism", "2")], "output": "MMT_ChargedComponent", "output_count": "1", "skill": "14", "research": "ChargedShot", "benches": ("MMT_MechWeaponTable",)},
    "MMT_MakeLaserComponent": {"work": "2100", "inputs": [("categories", "MMT_USLDBar", "20"), ("categories", "MMT_HCMBar", "10"), ("thingDefs", "MMT_MagneticMaterial", "4"), ("thingDefs", "MMT_Microchip", "1")], "output": "MMT_LaserComponent", "output_count": "1", "skill": "15", "research": "ChargedShot", "benches": ("MMT_MechWeaponTable",)},
    "MMT_MakePlasmaComponent": {"work": "2100", "inputs": [("categories", "MMT_USLDBar", "20"), ("categories", "MMT_HCMBar", "10"), ("thingDefs", "MMT_BiosyntheticMaterial", "4"), ("thingDefs", "MMT_Microchip", "1")], "output": "MMT_PlasmaComponent", "output_count": "1", "skill": "15", "research": "ChargedShot", "benches": ("MMT_MechWeaponTable",)},
}

TEXTILES_WEAPONS_RESEARCH_EXPECTATIONS = {
    "MMT_ArtSupplies": {"label": "艺术用品", "cost": "400", "tech": "Industrial", "prereqs": [], "x": "2.00", "y": "0.00"},
    "MMT_SuperFabric": {"label": "超织物工艺", "cost": "2000", "tech": "Spacer", "prereqs": ["AdvancedFabrication"], "x": "23.00", "y": "1.00"},
    "MMT_Rocketry1": {"label": "火箭弹 I", "cost": "1400", "tech": "Industrial", "prereqs": ["HeavyTurrets"], "x": "23.00", "y": "2.00"},
}

MATTER_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Matter.xml"
SUPER_ALLOYS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_SuperAlloys.xml"
MATTER_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Matter.xml"
MATTER_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Matter.xml"
TOP_MATERIALS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_TopMaterials.xml"
MATTER_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Matter.xml"

MATTER_ITEM_TEXTURES = [
    *[
        f"1.7/Textures/Things/Item/MMT/MMT_{name}.png"
        for name in ("Matter", "Antimatter", "Biomatter", "MagneticMaterial", "BioMicrochips", "AlphaPoly", "BetaPoly")
    ],
    *[
        f"1.7/Textures/Things/Item/MMT/MMT_BiosyntheticMaterial/MMT_BiosyntheticMaterial_{suffix}.png"
        for suffix in ("a", "b", "c")
    ],
    *[
        f"1.7/Textures/Things/Item/MMT/MMT_BioferriteAlloy/MMT_BioferriteAlloy_{suffix}.png"
        for suffix in ("a", "b", "c")
    ],
]

MATTER_BUILDING_TEXTURES = [
    *[
        f"1.7/Textures/Things/Building/Production/MMT_MatterConverter/MMT_MatterConverter_{side}.png"
        for side in ("east", "north", "south")
    ],
    *[
        f"1.7/Textures/Things/Building/Production/MMT_Matterfab/MMT_Matterfab_{side}.png"
        for side in ("east", "north", "south")
    ],
    "1.7/Textures/Things/Building/Production/MMT_ParticleCollider/MMT_ParticleCollider.png",
]

MATTER_RESOURCE_EXPECTATIONS = {
    "MMT_Matter": {"label": "无机物", "value": "10", "stack": "150", "flammable": "0", "tech": "Spacer", "categories": ["MMT_SuperMaterialCat"], "tex": "Things/Item/MMT/MMT_Matter", "class": "Graphic_Single"},
    "MMT_Antimatter": {"label": "反物质", "value": "120", "stack": "75", "flammable": "5", "tech": "Ultra", "categories": ["MMT_SuperMaterialCat"], "tex": "Things/Item/MMT/MMT_Antimatter", "class": "Graphic_Single"},
    "MMT_Biomatter": {"label": "生物质", "value": "10", "stack": "150", "flammable": "2", "tech": "Industrial", "categories": ["MMT_SuperMaterialCat"], "tex": "Things/Item/MMT/MMT_Biomatter", "class": "Graphic_Single"},
    "MMT_MagneticMaterial": {"label": "磁性材料", "value": "110", "stack": "75", "flammable": "0.1", "tech": "Spacer", "categories": ["MMT_SuperMaterialCat"], "tex": "Things/Item/MMT/MMT_MagneticMaterial", "class": "Graphic_Single"},
    "MMT_BiosyntheticMaterial": {"label": "生物合成材料", "value": "90", "stack": "75", "flammable": "0.3", "tech": "Spacer", "categories": ["MMT_SuperMaterialCat"], "tex": "Things/Item/MMT/MMT_BiosyntheticMaterial", "class": "Graphic_StackCount"},
    "MMT_BioMicrochips": {"label": "生物电子元件", "value": "2100", "stack": "25", "flammable": "1", "tech": "Spacer", "categories": ["MMT_ElectronicsCat"], "tex": "Things/Item/MMT/MMT_BioMicrochips", "class": "Graphic_Single"},
}

SUPER_ALLOYS_RESOURCE_EXPECTATIONS = {
    "MMT_AlphaPoly": {"label": "α阿尔法聚合物", "value": "45", "stack": "75", "categories": ["MMT_USLDBar"], "tex": "Things/Item/MMT/MMT_AlphaPoly", "adjective": "α聚合物制", "hp": "3.7", "work_make": "3.0", "work_build": "3.0", "mass": "0.6"},
    "MMT_BetaPoly": {"label": "β贝塔聚合物", "value": "65", "stack": "75", "categories": ["MMT_USLDBar", "MMT_USLDHBar"], "tex": "Things/Item/MMT/MMT_BetaPoly", "adjective": "β聚合物制", "hp": "4.3", "work_make": "4.5", "work_build": "4.5", "mass": "1.3"},
    "MMT_BioferriteAlloy": {"label": "生物铁合金", "value": "9", "stack": "600", "categories": ["MMT_SLDBar"], "tex": "Things/Item/MMT/MMT_BioferriteAlloy", "adjective": "生物铁合金制", "hp": "2.2", "work_make": "2", "work_build": "2", "mass": "0.95"},
}

MATTER_RESEARCH_EXPECTATIONS = {
    "MMT_MatterConversion1": {"label": "物质转换 I", "cost": "2500", "tech": "Spacer", "prereqs": ["MMT_BasicElectronics", "MMT_ComponentCrafting"], "x": "19.00", "y": "1.00"},
    "MMT_MatterConversion2": {"label": "物质转换 II", "cost": "2400", "tech": "Ultra", "prereqs": ["MMT_UltraComponents", "MMT_MatterConversion1"], "x": "20.00", "y": "2.00"},
    "MMT_MatterConversion3": {"label": "物质转换 III", "cost": "2500", "tech": "Ultra", "prereqs": ["MMT_MatterConversion2"], "x": "21.00", "y": "2.00"},
}

MATTER_CONVERTER_RECIPES = [
    "MMT_ConvertResourcesRaw",
    "MMT_ConvertToIron",
    "MMT_ConvertToCopper",
    "MMT_ConvertToTin",
    "MMT_ConvertToLead",
    "MMT_ConvertToAluminium",
    "MMT_ConvertToNickel",
    "MMT_ConvertToTitanium",
    "MMT_ConvertToGold",
    "MMT_ConvertToSilver",
    "MMT_ConvertToUranium",
    "MMT_ConvertToPolymers",
    "MMT_ConvertToParaffins",
    "MMT_ConvertToSulphates",
]

MATTER_BUILDING_EXPECTATIONS = {
    "MMT_MatterConverter": {"label": "无机物转化器", "size": "(3,3)", "draw": "(3,3)", "class": "Graphic_Multi", "tex": "Things/Building/Production/MMT_MatterConverter/MMT_MatterConverter", "stuff_count": "280", "work_build": "9000", "hp": "280", "power": "2500", "research": ["MMT_MatterConversion1"], "skill": "18", "recipes": MATTER_CONVERTER_RECIPES},
    "MMT_Matterfab": {"label": "量子加工台", "size": "(3,2)", "draw": "(3,2)", "class": "Graphic_Multi", "tex": "Things/Building/Production/MMT_Matterfab/MMT_Matterfab", "stuff_count": "240", "work_build": "10000", "hp": "350", "power": "8000", "research": ["MMT_MatterConversion3"], "skill": "16", "recipes": ["MMT_ProduceAPoly", "MMT_ProduceBPoly"]},
    "MMT_ParticleCollider": {"label": "粒子对撞台", "size": "(7,2)", "draw": "(7,2)", "class": "Graphic_Single", "tex": "Things/Building/Production/MMT_ParticleCollider/MMT_ParticleCollider", "stuff_count": "120", "work_build": "12000", "hp": "400", "power": "15000", "research": ["MMT_MatterConversion2"], "skill": "18", "recipes": ["MMT_MakeAntimatter"]},
}

MATTER_RECIPE_EXPECTATIONS = {
    "MMT_ConvertResourcesRaw": {"work": "10000", "inputs": [("categories", "ResourcesRaw", "300")], "output": "MMT_Matter", "output_count": "150", "skill": "15", "research": "MMT_MatterConversion1"},
    "MMT_ConvertToIron": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_IronOre", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToCopper": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_CopperOre", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToTin": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_TinOre", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToLead": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_Anglesite", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToAluminium": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_AluminiumOre", "output_count": "80", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToNickel": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "150")], "output": "MMT_NickelOre", "output_count": "20", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToTitanium": {"work": "15000", "inputs": [("thingDefs", "MMT_Matter", "200")], "output": "MMT_IlmeniteOre", "output_count": "20", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToGold": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "60")], "output": "MMT_GoldOre", "output_count": "20", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToSilver": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "100")], "output": "MMT_SilverOre", "output_count": "300", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToUranium": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "200")], "output": "Uranium", "output_count": "50", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToPolymers": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_Polymers", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToParaffins": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_Paraffins", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
    "MMT_ConvertToSulphates": {"work": "12000", "inputs": [("thingDefs", "MMT_Matter", "50")], "output": "MMT_Sulphates", "output_count": "100", "skill": "15", "research": "MMT_MatterConversion2"},
}

TOP_MATERIALS_RECIPE_EXPECTATIONS = {
    "MMT_BuildMagneticMaterial": {"work": "2400", "inputs": [("thingDefs", "MMT_Matter", "30"), ("thingDefs", "MMT_Sulphates", "15"), ("categories", "MMT_USLDBar", "10"), ("categories", "MMT_SLDBar", "5")], "output": "MMT_MagneticMaterial", "output_count": "5", "skill": "18", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "MMT_UltraComponents", "users": ["MMT_RoboticAssembler"]},
    "MMT_BuildBiosyntheticMaterial": {"work": "2400", "inputs": [("thingDefs", "MMT_Biomatter", "15"), ("thingDefs", "MMT_SyntheticFibers", "9"), ("thingDefs", "MMT_Plastic", "30")], "output": "MMT_BiosyntheticMaterial", "output_count": "5", "skill": "18", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "MMT_UltraComponents", "users": ["MMT_RoboticAssembler"]},
    "MMT_MakeBioelectronicDevices": {"work": "7000", "inputs": [("thingDefs", "MMT_Biomatter", "60"), ("thingDefs", "Hyperweave", "15"), ("thingDefs", "MMT_Microchip", "2")], "output": "MMT_BioMicrochips", "output_count": "1", "skill": "18", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smith", "sound": "Recipe_Machining", "research": "MMT_UltraComponents", "users": ["MMT_RoboticAssembler"]},
    "MMT_DisassembleAIPersonaCore": {"work": "4000", "inputs": [("thingDefs", "AIPersonaCore", "1")], "output": "MMT_BioMicrochips", "output_count": "1", "skill": "16", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smith", "sound": "Recipe_Machining", "research": "MMT_AdvancedComponents", "users": ["MMT_RoboticAssembler"], "extra_products": {"MMT_AlphaPoly": "30", "MMT_CarbonAlloy": "40"}},
    "MMT_ProduceAPoly": {"work": "8000", "inputs": [("thingDefs", "MMT_DepletedUranium", "5"), ("thingDefs", "MMT_CarbonAlloy", "5"), ("thingDefs", "MMT_Antimatter", "3")], "output": "MMT_AlphaPoly", "output_count": "15", "skill": "19", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": None},
    "MMT_ProduceBPoly": {"work": "12000", "inputs": [("thingDefs", "MMT_AlphaPoly", "3"), ("thingDefs", "MMT_BiosyntheticMaterial", "5")], "output": "MMT_BetaPoly", "output_count": "5", "skill": "20", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": None},
    "MMT_MakeAntimatter": {"work": "10000", "inputs": [("thingDefs", "MMT_Matter", "50"), ("thingDefs", "Uranium", "20")], "output": "MMT_Antimatter", "output_count": "2", "skill": "15", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": None},
    "MMT_ExtractBiomatterCorpse": {"work": "6000", "inputs": [("categories", "Corpses", "1")], "output": "MMT_Biomatter", "output_count": "20", "skill": "8", "speed": "DrugSynthesisSpeed", "effect": "Cook", "sound": "Recipe_Smelt", "research": "MMT_MatterConversion1"},
    "MMT_ExtractBiomatterMeat": {"work": "4000", "inputs": [("categories", "MeatRaw", "40")], "output": "MMT_Biomatter", "output_count": "20", "skill": "8", "speed": "DrugSynthesisSpeed", "effect": "Cook", "sound": "Recipe_Smelt", "research": "MMT_MatterConversion1"},
    "MMT_MakeBioferriteAlloy": {"work": "2100", "inputs": [("thingDefs", "Bioferrite", "10"), ("thingDefs", "MMT_IronOre", "10"), ("categories", "MMT_Coal", "5")], "output": "MMT_BioferriteAlloy", "output_count": "10", "skill": "9", "speed": "SmeltingSpeed", "effect": "Smelt", "sound": "Recipe_Smelt", "research": "BioferriteShaping", "users": ["BioferriteShaper"]},
}


ELECTRIC_ELECTRONICS_TABLE_RECIPES = [
    "MMT_AssembleElectronicParts_Electric",
    "MMT_AssembleIC_Electric",
    "MMT_AssembleMicrochip_Electric",
    "MMT_MakeHexcell",
    "MMT_MakeRobotParts",
    "MMT_MakeMineralSonarModule",
]

ROBOTICS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Robotics.xml"
ROBOTICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Robotics.xml"

ROBOTICS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_{name}.png"
    for name in ("Hexcell", "RobotParts", "MineralSonarModule")
]

ROBOTICS_RESOURCE_EXPECTATIONS = {
    "MMT_Hexcell": {"label": "动力电池", "value": "150", "stack": "50", "flammable": "5", "tech": "Spacer", "tex": "Things/Item/MMT/MMT_Hexcell"},
    "MMT_RobotParts": {"label": "机器人部件", "value": "70", "stack": "75", "flammable": "1", "tech": "Spacer", "tex": "Things/Item/MMT/MMT_RobotParts"},
    "MMT_MineralSonarModule": {"label": "矿物声纳模块", "value": "300", "stack": "25", "flammable": "0.1", "tech": "Spacer", "tex": "Things/Item/MMT/MMT_MineralSonarModule"},
}

ROBOTICS_RECIPE_EXPECTATIONS = {
    "MMT_MakeHexcell": {"work": "6000", "inputs": [("thingDefs", "MMT_MagneticMaterial", "2"), ("categories", "MMT_HCMBar", "4"), ("thingDefs", "MMT_Microchip", "1")], "products": {"MMT_Hexcell": "10"}, "skill": "14", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smith", "sound": "Recipe_Machining", "research": "MMT_Robotics1", "benches": ["MMT_ElectricElectronicsTable"]},
    "MMT_MakeRobotParts": {"work": "8000", "inputs": [("categories", "MMT_USLDBar", "60"), ("thingDefs", "MMT_AdvMechanism", "15")], "products": {"MMT_RobotParts": "25"}, "skill": "15", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smith", "sound": "Recipe_Machining", "research": "MMT_Robotics1", "benches": ["MMT_ElectricElectronicsTable"]},
    "MMT_MakeMineralSonarModule": {"work": "4800", "inputs": [("categories", "MMT_MetallicBar", "25"), ("thingDefs", "ComponentSpacer", "5"), ("thingDefs", "MMT_ElectronicParts", "10"), ("thingDefs", "MMT_Mechanism", "1")], "products": {"MMT_MineralSonarModule": "1"}, "skill": "10", "speed": "MMT_ElectronicCraftingSpeed", "effect": "ButcherMechanoid", "sound": "Recipe_ButcherCorpseMechanoid", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench", "MMT_ElectricElectronicsTable"]},
    "MMT_DeconstructPersonalShield": {"work": "1000", "inputs": [("thingDefs", "Apparel_ShieldBelt", "1")], "products": {"ComponentSpacer": "8", "MMT_IntegratedCircuit": "1"}, "skill": "10", "speed": "MMT_ElectronicCraftingSpeed", "effect": "Smith", "sound": "Recipe_Machining", "research": "MMT_AdvancedComponents", "users": ["MMT_RoboticAssembler"]},
}

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
    "MMT_NickelOre": {"label": "镍矿", "value": "4.0", "color": "(42,208,206)", "tex": "Things/Item/MMT/MMT_Ore/MMT_NickelOre"},
    "MMT_IlmeniteOre": {"label": "钛铁矿", "value": "8.0", "color": "(160,178,195)", "tex": "Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre"},
    "MMT_WolframiteOre": {"label": "黑钨矿", "value": "9.0", "color": "(230,230,255)", "tex": "Things/Item/MMT/MMT_Ore/MMT_WolframiteOre"},
}

ADVANCED_ALLOY_EXPECTATIONS = {
    "MMT_NickelBar": {"label": "镍合金", "value": "6.0", "stack": "150", "color": "(42,208,206)", "categories": ["MMT_RARBar"], "stuff": True, "adjective": "镍制", "hp": "2.8", "beauty": "2.4", "work_make": "2.0", "work_build": "2.0", "mass": "1.35", "market_value": "0.7"},
    "MMT_Cobalt": {"label": "钴合金", "value": "10.0", "stack": "150", "color": "(60,90,120)", "categories": ["MMT_RARBar"], "stuff": False},
    "MMT_Chromium": {"label": "铬合金", "value": "5.0", "stack": "150", "color": "(235,235,235)", "categories": ["MMT_RARBar"], "stuff": False},
    "MMT_Tungsten": {"label": "钨合金", "value": "40.0", "stack": "150", "color": "(240,230,230)", "categories": ["MMT_RARBar", "MMT_USLDHBar"], "stuff": False},
    "MMT_Titanium": {"label": "钛合金", "value": "45.0", "stack": "75", "color": "(160,178,181)", "categories": ["MMT_RARBar", "MMT_USLDBar"], "stuff": True, "adjective": "钛制", "hp": "3.3", "beauty": "1.8", "work_make": "3.0", "work_build": "3.0", "mass": "0.8", "door": "1.3"},
    "MMT_StelliteAlloy": {"label": "司太立合金（钨铬钴合金）", "value": "30.0", "stack": "150", "color": "(125,146,178)", "categories": ["MMT_USLDBar"], "stuff": True, "adjective": "司太立制", "hp": "3.55", "beauty": "1.5", "work_make": "2.0", "work_build": "2.2", "mass": "1.24", "door": "0.95"},
    "MMT_NitinolAlloy": {"label": "镍钛合金", "value": "27.0", "stack": "150", "color": "(175,175,175)", "categories": ["MMT_USLDBar"], "stuff": True, "adjective": "镍钛诺制", "hp": "3.6", "beauty": "1.6", "work_make": "2.6", "work_build": "2.4", "mass": "0.95", "door": "1.2"},
    "MMT_PobediteAlloy": {"label": "钨钴合金", "value": "33.0", "stack": "150", "color": "(85,85,85)", "categories": ["MMT_USLDBar", "MMT_USLDHBar"], "stuff": True, "adjective": "钴硬质制", "hp": "4.0", "beauty": "1.1", "work_make": "2.8", "work_build": "2.8", "mass": "1.4", "door": "0.7"},
    "MMT_DepletedUranium": {"label": "贫铀合金", "value": "18.0", "stack": "75", "color": "(20,94,30)", "categories": ["MMT_RADBar", "MMT_USLDHBar"], "stuff": True, "adjective": "贫铀制", "hp": "3.05", "beauty": "0", "work_make": "2.3", "work_build": "2.3", "mass": "1.6", "market_value": "0.7", "door": "0.75"},
    "MMT_SteelBar": {"label": "斯坦合金", "value": "8.0", "stack": "150", "color": "(120,147,131)", "categories": ["MMT_USLDBar"], "stuff": True, "adjective": "斯坦制", "hp": "2.2", "beauty": "1.4", "work_make": "1.5", "work_build": "1.5", "mass": "1.3", "market_value": "0.7", "door": "0.8"},
    "MMT_CupronickelAlloy": {"label": "梅尔基奥合金", "value": "8.0", "stack": "150", "color": "(168,157,125)", "categories": ["MMT_HCMBar", "MMT_SLDBar"], "stuff": True, "adjective": "梅尔基奥制", "hp": "2.1", "beauty": "1.6", "work_make": "1.4", "work_build": "1.4", "mass": "1.15", "market_value": "0.7", "door": "0.75"},
    "MMT_AlnicoAlloy": {"label": "亚力可合金", "value": "11.0", "stack": "150", "color": "(210,210,210)", "categories": ["MMT_HCMBar", "MMT_USLDBar"], "stuff": True, "adjective": "亚力可制", "hp": "2.6", "beauty": "1.35", "work_make": "1.7", "work_build": "1.7", "mass": "1.15", "market_value": "0.7", "door": "1.0"},
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
    "MMT_MakeCupronickelAlloy",
    "MMT_MakeDepletedUranium",
    "MMT_MakeSteelBar",
    "MMT_MakeAlnicoAlloy",
]

ADVANCED_ALLOYS_RECIPE_EXPECTATIONS = {
    "MMT_SmeltNickelBar": {"work": "1500", "inputs": [("thingDefs", "MMT_NickelOre", "15")], "output": "MMT_NickelBar", "output_count": "15", "skill": "13", "research": "MMT_AdvancedMetalRefining"},
    "MMT_ExtractCobalt": {"work": "1300", "inputs": [("thingDefs", "MMT_NickelOre", "30")], "output": "MMT_Cobalt", "output_count": "15", "skill": "14", "research": "MMT_AdvancedMetalRefining"},
    "MMT_ExtractChromium": {"work": "1200", "inputs": [("thingDefs", "MMT_IronOre", "25")], "output": "MMT_Chromium", "output_count": "10", "skill": "11", "research": "MMT_AdvancedMetalRefining"},
    "MMT_SmeltTungsten": {"work": "3000", "inputs": [("thingDefs", "MMT_WolframiteOre", "15")], "output": "MMT_Tungsten", "output_count": "15", "skill": "17", "research": "MMT_AdvancedAlloys"},
    "MMT_SmeltTitanium": {"work": "3300", "inputs": [("thingDefs", "MMT_IlmeniteOre", "15")], "output": "MMT_Titanium", "output_count": "15", "skill": "16", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeStellite": {"work": "2100", "inputs": [("thingDefs", "MMT_Cobalt", "5"), ("thingDefs", "MMT_Chromium", "13"), ("thingDefs", "MMT_Tungsten", "2")], "output": "MMT_StelliteAlloy", "output_count": "20", "skill": "17", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeNitinol": {"work": "2600", "inputs": [("thingDefs", "MMT_Titanium", "4"), ("thingDefs", "MMT_NickelBar", "11")], "output": "MMT_NitinolAlloy", "output_count": "15", "skill": "16", "research": "MMT_AdvancedAlloys"},
    "MMT_MakePobedite": {"work": "2700", "inputs": [("thingDefs", "MMT_Tungsten", "3"), ("thingDefs", "MMT_CarbonAlloy", "3"), ("thingDefs", "MMT_Cobalt", "3")], "output": "MMT_PobediteAlloy", "output_count": "10", "skill": "18", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeFerrotitanium": {"work": "2400", "inputs": [("thingDefs", "MMT_Titanium", "5"), ("thingDefs", "Steel", "12"), ("thingDefs", "MMT_CarbonAlloy", "3")], "output": "Plasteel", "output_count": "15", "skill": "15", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeCupronickelAlloy": {"work": "1600", "inputs": [("thingDefs", "MMT_CopperBar", "24"), ("thingDefs", "MMT_NickelBar", "6")], "output": "MMT_CupronickelAlloy", "output_count": "30", "skill": "12", "research": "MMT_AdvancedMetalRefining"},
    "MMT_MakeDepletedUranium": {"work": "1700", "inputs": [("thingDefs", "Uranium", "10")], "output": "MMT_DepletedUranium", "output_count": "10", "skill": "11", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeSteelBar": {"work": "1800", "inputs": [("thingDefs", "Steel", "15"), ("thingDefs", "MMT_NickelBar", "5")], "output": "MMT_SteelBar", "output_count": "20", "skill": "11", "research": "MMT_AdvancedAlloys"},
    "MMT_MakeAlnicoAlloy": {"work": "1650", "inputs": [("thingDefs", "Steel", "8"), ("thingDefs", "MMT_AluminiumBar", "3"), ("thingDefs", "MMT_Cobalt", "3"), ("thingDefs", "MMT_NickelBar", "6")], "output": "MMT_AlnicoAlloy", "output_count": "20", "skill": "13", "research": "MMT_AdvancedAlloys"},
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
    "MMT_MakeReinforcedConcrete_Electric",
    "MMT_MakeMineralSonarModule",
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
    "MMT_MakeComponentMedieval_Metal": {"work": "900", "inputs": [("thingDefs", ("Steel", "MMT_Bronze", "MMT_CopperBar", "MMT_TinBar", "MMT_AluminiumBar", "Silver", "Gold"), "10")], "output": "MMT_ComponentMedieval", "output_count": "10", "skill": None, "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench", "MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Hand": {"work": "2000", "inputs": [("thingDefs", ("Steel", "MMT_Bronze", "MMT_SteelBar", "MMT_FerrosiliconAlloy"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Electric": {"work": "2000", "inputs": [("thingDefs", ("Steel", "MMT_Bronze", "MMT_SteelBar", "MMT_FerrosiliconAlloy"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeWire_Hand": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "10")], "output": "MMT_Wire", "output_count": "20", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeWire_Electric": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "20")], "output": "MMT_Wire", "output_count": "40", "skill": "6", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeMechanism_Hand": {"work": "1800", "inputs": [("thingDefs", ("Steel", "MMT_Bronze", "MMT_FerrosiliconAlloy"), "30"), ("thingDefs", "ComponentIndustrial", "10")], "output": "MMT_Mechanism", "output_count": "5", "skill": "4", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeMechanism_Electric": {"work": "1800", "inputs": [("thingDefs", ("Steel", "MMT_Bronze", "MMT_FerrosiliconAlloy"), "60"), ("thingDefs", "ComponentIndustrial", "20")], "output": "MMT_Mechanism", "output_count": "10", "skill": "7", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentSpacer_Hand": {"work": "2500", "inputs": [("thingDefs", ("Plasteel", "MMT_SteelBar", "MMT_Titanium", "MMT_NitinolAlloy", "MMT_AlnicoAlloy"), "30")], "output": "ComponentSpacer", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentSpacer_Electric": {"work": "2500", "inputs": [("thingDefs", ("Plasteel", "MMT_SteelBar", "MMT_Titanium", "MMT_NitinolAlloy", "MMT_AlnicoAlloy"), "30")], "output": "ComponentSpacer", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeAdvMechanism_Hand": {"work": "3200", "inputs": [("thingDefs", ("Plasteel", "MMT_SteelBar", "MMT_Titanium", "MMT_NitinolAlloy", "MMT_AlnicoAlloy"), "30"), ("thingDefs", "ComponentSpacer", "10"), ("thingDefs", "MMT_SyntheticFibers", "5")], "output": "MMT_AdvMechanism", "output_count": "5", "skill": "13", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeAdvMechanism_Electric": {"work": "3200", "inputs": [("thingDefs", ("Plasteel", "MMT_SteelBar", "MMT_Titanium", "MMT_NitinolAlloy", "MMT_AlnicoAlloy"), "60"), ("thingDefs", "ComponentSpacer", "20"), ("thingDefs", "MMT_SyntheticFibers", "10")], "output": "MMT_AdvMechanism", "output_count": "10", "skill": "16", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeSpacerComponent": {"work": "3500", "inputs": [("thingDefs", ("Plasteel", "MMT_SteelBar", "MMT_Titanium", "MMT_NitinolAlloy", "MMT_AlnicoAlloy"), "25"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_SpacerComponent", "output_count": "10", "skill": "9", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeUltraComponent": {"work": "4500", "inputs": [("thingDefs", ("MMT_Tungsten", "MMT_Titanium"), "15"), ("thingDefs", ("MMT_StelliteAlloy", "MMT_PobediteAlloy"), "10"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_UltraComponent", "output_count": "10", "skill": "13", "research": "MMT_UltraComponents", "benches": ["MMT_RoboticAssembler"]},
}

INFRA_RESEARCH_TAB_XML = "1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml"
INFRA_DESIGNATION_XML = "1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml"
INFRA_CATEGORIES_XML = "1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml"
INFRA_PATCH_XML = "1.7/Patches/MMT_Patch_VanillaCategories.xml"
INFRA_RESEARCH_DIR = "1.7/Defs/ResearchProjectDefs"
INFRA_BUILDING_DIR = "1.7/Defs/ThingDefs_Buildings"
INFRA_RESEARCH_TOTAL = 52
INFRA_WORKTABLE_TOTAL = 25
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
    "Cloth": "MMT_BTextiles",
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
    assert_equal(set(research_defs), {"MMT_ConcreteBasic", "MMT_ConcreteAdvanced"}, "混凝土研究 defName 集合")
    research = research_defs["MMT_ConcreteBasic"]
    assert_equal(child_text(research, "label"), "混凝土技术", "混凝土研究 label")
    assert_equal(child_text(research, "baseCost"), "1000", "混凝土研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Industrial", "混凝土研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Stonecutting"], "混凝土研究前置")
    assert_equal(child_text(research, "researchViewX"), "7.00", "混凝土研究 X 坐标")
    assert_equal(child_text(research, "researchViewY"), "0.00", "混凝土研究 Y 坐标")

    building_defs = def_map(parse_xml(CONCRETE_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_ConcreteMixer", "MMT_EConcreteMixer"}, "混凝土建筑 defName 集合")
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

    resource_names = set(def_map(parse_xml(RESOURCE_XML))) | set(def_map(parse_xml(CONCRETE_RESOURCE_XML))) | set(def_map(parse_xml(AGRICULTURE_RESOURCE_XML)))
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
        if expected["skill"] is None:
            if recipe.find("./skillRequirements/Crafting") is not None:
                fail(f"{name} 不应设 Crafting 技能要求（对齐 HSK 无技能要求）")
        else:
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
                if value not in VANILLA_CATEGORIES | MMT_CATEGORIES:
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
        elif expected.get("stuff_lite"):
            assert_equal(li_values(node, "./stuffProps/categories"), ["Woody"], f"{name} stuff categories")
            assert_equal(child_text(node, "./stuffProps/appearance"), "Planks", f"{name} stuff appearance")
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
    assert_equal(li_values(kiln, "stuffCategories"), ["Stony", "MMT_Bricks"], "木炭窑材料分类")
    assert_equal(child_text(kiln, "costStuffCount"), "180", "木炭窑材料数量")
    assert_equal(child_text(kiln, "./statBases/WorkToBuild"), "3000", "木炭窑建造工作量")
    assert_equal(child_text(kiln, "./statBases/MaxHitPoints"), "270", "木炭窑耐久")
    assert_equal(child_text(kiln, "size"), "(3,3)", "木炭窑尺寸")
    assert_equal(li_values(kiln, "recipes"), CHARCOAL_KILN_RECIPES, "木炭窑配方")
    assert_equal(li_values(kiln, "researchPrerequisites"), ["MMT_CharcoalBurning"], "木炭窑研究前置")
    assert_equal(child_text(kiln, "constructionSkillPrerequisite"), "4", "木炭窑建造技能")
    # 窑炉为燃料型（对标 HSK Kiln）：Refuelable + Glower + Flickable
    kiln_comps = [li.attrib.get("Class") for li in kiln.findall("./comps/li")]
    for expected in ("CompProperties_Refuelable", "CompProperties_Glower", "CompProperties_Flickable"):
        if expected not in kiln_comps:
            fail(f"木炭窑缺少组件: {expected}")
    fuel = kiln.find("./comps/li[@Class='CompProperties_Refuelable']")
    assert_equal(sorted(li_values(fuel, "./fuelFilter/thingDefs")), ["MMT_Charcoal", "MMT_Coal", "MMT_Kindling", "WoodLog"], "木炭窑燃料列表")

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

        if expected["input"] not in VANILLA_THINGS | {"MMT_Bamboo", "MMT_RedWoodLog"}:
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
    if ">SmithingSpeed<" in read_text(WOOD_RECIPE_XML) or "Wooden" in read_text(WOOD_RECIPE_XML):
        fail("木材配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed 或 Wooden 分类")
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
        assert_equal(child_text(node, "./graphicData/texPath"), "Things/Item/MMT/MMT_Ingot", f"{name} texPath")
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
    if ">SmithingSpeed<" in recipe_text:
        fail("石化配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed")
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
        assert_equal(li_values(node, "thingCategories"), [expected["category"]], f"{name} thingCategories")
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
    assert_equal(set(research_defs), {"MMT_BasicElectronics", "MMT_IntegratedCircuitTech", "MMT_Robotics1"}, "电子研究 defName 集合")
    robotics = research_defs["MMT_Robotics1"]
    assert_equal(child_text(robotics, "label"), "机器人工程 I", "机器人工程研究 label")
    assert_equal(child_text(robotics, "baseCost"), "2000", "机器人工程研究 baseCost")
    assert_equal(child_text(robotics, "techLevel"), "Spacer", "机器人工程研究 techLevel")
    assert_equal(child_text(robotics, "tab"), "MMT_ResearchTab", "机器人工程研究 tab")
    assert_equal(li_values(robotics, "prerequisites"), ["MMT_BasicElectronics"], "机器人工程研究前置")
    assert_equal(child_text(robotics, "researchViewX"), "22.00", "机器人工程研究 X 坐标")
    assert_equal(child_text(robotics, "researchViewY"), "1.00", "机器人工程研究 Y 坐标")
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
    if ">ElectronicCraftingSpeed<" in recipe_text:
        fail("电子配方不得使用未加 MMT_ 前缀的 HSK 属性 ElectronicCraftingSpeed")
    print("electronics-recipes: OK")


def validate_robotics() -> None:
    require_files([
        ROBOTICS_RESOURCE_XML,
        ROBOTICS_RECIPE_XML,
        MATTER_RESOURCE_XML,
        PARTS_RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        ADVANCED_ALLOY_RESOURCE_XML,
        PARTS_BUILDING_XML,
        ELECTRIC_ELECTRONICS_BUILDING_XML,
        *ROBOTICS_ITEM_TEXTURES,
    ])

    resource_defs = def_map(parse_xml(ROBOTICS_RESOURCE_XML))
    assert_equal(set(resource_defs), set(ROBOTICS_RESOURCE_EXPECTATIONS), "机器人资源 defName 集合")
    for name, expected in ROBOTICS_RESOURCE_EXPECTATIONS.items():
        node = resource_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["MMT_ElectronicsCat"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")
    if resource_defs["MMT_Hexcell"].find("./comps/li[@Class='CompProperties_Explosive']") is None:
        fail("MMT_Hexcell 缺少爆炸组件")
    assert_equal(child_text(resource_defs["MMT_Hexcell"], "useHitPoints"), "true", "动力电池 useHitPoints")

    resource_names = (
        set(resource_defs)
        | set(def_map(parse_xml(MATTER_RESOURCE_XML)))
        | set(def_map(parse_xml(PARTS_RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(ADVANCED_ALLOY_RESOURCE_XML)))
    )
    allowed_categories = {"MMT_HCMBar", "MMT_USLDBar", "MMT_MetallicBar"}

    bench_lists = {
        "MMT_ElectricAssemblyBench": li_values(def_map(parse_xml(PARTS_BUILDING_XML))["MMT_ElectricAssemblyBench"], "recipes"),
        "MMT_ElectricElectronicsTable": li_values(def_map(parse_xml(ELECTRIC_ELECTRONICS_BUILDING_XML))["MMT_ElectricElectronicsTable"], "recipes"),
    }

    recipe_defs = def_map(parse_xml(ROBOTICS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(ROBOTICS_RECIPE_EXPECTATIONS), "机器人配方 defName 集合")
    for name, expected in ROBOTICS_RECIPE_EXPECTATIONS.items():
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
            categories = li_values(filter_node, "categories")
            if bool(thing_defs) == bool(categories):
                fail(f"{name} 每个输入 filter 必须且只能包含 thingDefs 或 categories 之一")
            if thing_defs:
                actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
            else:
                actual_inputs.append(("categories", tuple(categories), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed_things = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_things = sorted({item for kind, value, _ in normalized if kind == "thingDefs" for item in value})
        assert_equal(fixed_things, expected_things, f"{name} 固定输入列表")
        fixed_categories = sorted(li_values(recipe, "./fixedIngredientFilter/categories"))
        expected_categories = sorted({item for kind, value, _ in normalized if kind == "categories" for item in value})
        assert_equal(fixed_categories, expected_categories, f"{name} 固定输入类别")

        products = {node.tag: (node.text or "").strip() for node in recipe.findall("./products/*") if isinstance(node.tag, str)}
        assert_equal(products, expected["products"], f"{name} 输出")

        research = recipe.findtext("researchPrerequisite")
        assert_equal(research.strip() if research else None, expected["research"], f"{name} researchPrerequisite")

        users = li_values(recipe, "recipeUsers")
        if "users" in expected:
            assert_equal(users, expected["users"], f"{name} recipeUsers")
        elif users:
            fail(f"{name} 不应写 recipeUsers")

        for bench in expected.get("benches", []):
            if name not in bench_lists[bench]:
                fail(f"{name} 未挂载到 {bench}")
        if not expected.get("benches") and "users" not in expected:
            fail(f"{name} 未声明挂载方式")

        for kind, value, _ in normalized:
            for item in value:
                if kind == "thingDefs":
                    if item not in resource_names | VANILLA_THINGS:
                        fail(f"{name} 使用未知资源: {item}")
                elif item not in allowed_categories:
                    fail(f"{name} 使用未知类别: {item}")
        for output_name in expected["products"]:
            if output_name not in resource_names | VANILLA_THINGS:
                fail(f"{name} 输出不是已知资源: {output_name}")

    for path in ROBOTICS_ITEM_TEXTURES:
        assert_png(path)

    for path in (ROBOTICS_RESOURCE_XML, ROBOTICS_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("robotics: OK")


def validate_electronics() -> None:
    validate_electronics_resources()
    validate_electronics_building()
    validate_electronics_recipes()
    validate_robotics()
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
        assert_equal(li_values(ingredient, "./filter/thingDefs"), [expected.get("input", "WoodLog")], f"{name} 输入引用")
        assert_equal(li_values(recipe, "./fixedIngredientFilter/thingDefs"), [expected.get("input", "WoodLog")], f"{name} 固定输入引用")

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
    if ">SmithingSpeed<" in recipe_text or "Wooden" in recipe_text:
        fail("电动木材配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed 或 Wooden 分类")
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
    if ">SmithingSpeed<" in read_text(ELECTRIC_PETROCHEM_RECIPE_XML):
        fail("电动石化配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed")
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
    if ">ElectronicCraftingSpeed<" in read_text(ELECTRIC_ELECTRONICS_RECIPE_XML):
        fail("电动电子配方不得使用未加 MMT_ 前缀的 HSK 属性 ElectronicCraftingSpeed")
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
        PETROCHEM_EXT_RESOURCE_XML,
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
        assert_equal(child_text(node, "./graphicData/texPath"), "Things/Item/MMT/MMT_Ingot", f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
        assert_equal(child_text(node, "./graphicData/color"), expected["color"], f"{name} color")
        assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
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
        | set(def_map(parse_xml(PETROCHEM_EXT_RESOURCE_XML)))
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
    if ">SmithingSpeed<" in read_text(ADVANCED_ALLOYS_RECIPE_XML):
        fail("高级合金配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed")
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
        assert_equal(li_values(node, "thingCategories"), ["MMT_Parts"], f"{name} thingCategories")
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
    if ">SmithingSpeed<" in read_text(PARTS_RECIPE_XML):
        fail("零件配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed")
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


def validate_petrochem_ext() -> None:
    require_files([
        PETROCHEM_EXT_RESOURCE_XML,
        PETROCHEM_EXT_BUILDING_XML,
        PETROCHEM_EXT_RECIPE_XML,
        PETROCHEM_EXT_RESEARCH_XML,
        PETROCHEM_RESOURCE_XML,
        PETROCHEM_BUILDING_XML,
        ELECTRIC_PETROCHEM_BUILDING_XML,
        SULFUR_RESOURCE_XML,
        AGRICULTURE_RESOURCE_XML,
        METAL_RESOURCE_XML,
        *PETROCHEM_EXT_ITEM_TEXTURES,
        *PETROCHEM_EXT_BUILDING_TEXTURES,
    ])

    resource_defs = def_map(parse_xml(PETROCHEM_EXT_RESOURCE_XML))
    assert_equal(set(resource_defs), set(PETROCHEM_EXT_RESOURCE_EXPECTATIONS), "石化扩展资源 defName 集合")
    for name, expected in PETROCHEM_EXT_RESOURCE_EXPECTATIONS.items():
        node = resource_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./statBases/DeteriorationRate"), "0", f"{name} DeteriorationRate")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
        if name == "MMT_CarbonAlloy":
            assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_StackCount", f"{name} graphicClass")
            assert_equal(child_text(node, "./graphicData/color"), "(130,130,130)", f"{name} color")
            assert_equal(child_text(node, "useHitPoints"), "true", f"{name} useHitPoints")
            assert_equal(li_values(node, "./stuffProps/categories"), ["Metallic"], f"{name} stuff categories")
            assert_equal(child_text(node, "./stuffProps/stuffAdjective"), "碳纤维制", f"{name} stuffAdjective")
            assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), "3.6", f"{name} MaxHitPoints factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), "2.5", f"{name} WorkToMake factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), "2.5", f"{name} WorkToBuild factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/Mass"), "0.8", f"{name} Mass factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/MarketValue"), "1", f"{name} MarketValue factor")
            assert_equal(child_text(node, "./stuffProps/statFactors/DoorOpenSpeed"), "0.8", f"{name} DoorOpenSpeed factor")
            assert_equal(child_text(node, "terrainAffordanceNeeded"), "Medium", f"{name} terrain affordance")
        else:
            assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
            if node.find("stuffProps") is not None:
                fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(PETROCHEM_EXT_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableNitre", "MMT_Loom", "MMT_AmmoBench"}, "石化扩展建筑 defName 集合")

    vein = building_defs["MMT_MineableNitre"]
    assert_equal(vein.attrib.get("ParentName"), "RockBase", "硝石矿脉父类")
    assert_equal(child_text(vein, "label"), "硝石矿脉", "硝石矿脉 label")
    assert_equal(child_text(vein, "./graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", "硝石矿脉 texPath")
    assert_equal(child_text(vein, "./graphicData/color"), "(124,124,110)", "硝石矿脉 color")
    assert_equal(child_text(vein, "./graphicData/colorTwo"), "(175,175,165)", "硝石矿脉 colorTwo")
    assert_equal(child_text(vein, "./statBases/MaxHitPoints"), "1800", "硝石矿脉耐久")
    assert_equal(child_text(vein, "./building/isResourceRock"), "true", "硝石矿脉 isResourceRock")
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_Nitre", "硝石矿脉 mineableThing")
    assert_equal(child_text(vein, "./building/mineableYield"), "75", "硝石矿脉 mineableYield")
    assert_equal(child_text(vein, "./building/mineableScatterCommonality"), "1.05", "硝石矿脉常见度")
    assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), "8~16", "硝石矿脉矿块范围")
    assert_equal(child_text(vein, "./building/veinMineable"), "true", "硝石矿脉 veinMineable")

    loom = building_defs["MMT_Loom"]
    assert_equal(child_text(loom, "label"), "纺织机", "纺织机 label")
    assert_equal(child_text(loom, "thingClass"), "Building_WorkTable", "纺织机 thingClass")
    assert_equal(child_text(loom, "./graphicData/texPath"), "Things/Building/Production/MMT_Loom/MMT_Loom", "纺织机 texPath")
    assert_equal(child_text(loom, "./graphicData/graphicClass"), "Graphic_Multi", "纺织机 graphicClass")
    assert_equal(li_values(loom, "stuffCategories"), ["Metallic", "Woody"], "纺织机材料分类")
    assert_equal(child_text(loom, "costStuffCount"), "125", "纺织机材料数量")
    assert_equal(child_text(loom, "./costList/MMT_ComponentMedieval"), "10", "纺织机原始零部件成本")
    assert_equal(child_text(loom, "./costList/MMT_ElectronicParts"), "3", "纺织机电子元件成本")
    assert_equal(child_text(loom, "./costList/MMT_Mechanism"), "6", "纺织机机械组件成本")
    assert_equal(child_text(loom, "./statBases/WorkToBuild"), "3850", "纺织机建造工作量")
    assert_equal(child_text(loom, "./statBases/MaxHitPoints"), "170", "纺织机耐久")
    assert_equal(child_text(loom, "size"), "(2,2)", "纺织机尺寸")
    assert_equal(child_text(loom, "designationCategory"), "MMT_Production", "纺织机建筑分类")
    assert_equal(li_values(loom, "recipes"), PETROCHEM_EXT_LOOM_RECIPES, "纺织机配方")
    assert_equal(li_values(loom, "researchPrerequisites"), ["ComplexClothing"], "纺织机研究前置")
    assert_equal(child_text(loom, "constructionSkillPrerequisite"), "7", "纺织机建造技能")
    loom_comps = [li.attrib.get("Class") for li in loom.findall("./comps/li")]
    for expected in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
        if expected not in loom_comps:
            fail(f"纺织机缺少组件: {expected}")
    assert_equal(child_text(loom, "./comps/li/basePowerConsumption"), "300", "纺织机功耗")

    bench = building_defs["MMT_AmmoBench"]
    assert_equal(child_text(bench, "label"), "弹药工作台", "弹药台 label")
    assert_equal(child_text(bench, "thingClass"), "Building_WorkTable", "弹药台 thingClass")
    assert_equal(child_text(bench, "./graphicData/texPath"), "Things/Building/Production/MMT_AmmoBench/MMT_AmmoBench", "弹药台 texPath")
    assert_equal(child_text(bench, "./graphicData/graphicClass"), "Graphic_Multi", "弹药台 graphicClass")
    assert_equal(li_values(bench, "stuffCategories"), ["Metallic"], "弹药台材料分类")
    assert_equal(child_text(bench, "costStuffCount"), "210", "弹药台材料数量")
    assert_equal(child_text(bench, "./costList/ComponentIndustrial"), "10", "弹药台零部件成本")
    assert_equal(child_text(bench, "./costList/MMT_ElectronicParts"), "5", "弹药台电子元件成本")
    assert_equal(child_text(bench, "./costList/MMT_Mechanism"), "10", "弹药台机械组件成本")
    assert_equal(child_text(bench, "./statBases/WorkToBuild"), "6500", "弹药台建造工作量")
    assert_equal(child_text(bench, "./statBases/MaxHitPoints"), "300", "弹药台耐久")
    assert_equal(child_text(bench, "size"), "(3,1)", "弹药台尺寸")
    assert_equal(child_text(bench, "designationCategory"), "MMT_Production", "弹药台建筑分类")
    assert_equal(li_values(bench, "recipes"), PETROCHEM_EXT_AMMO_BENCH_RECIPES, "弹药台配方")
    assert_equal(li_values(bench, "researchPrerequisites"), ["Machining"], "弹药台研究前置")
    assert_equal(child_text(bench, "constructionSkillPrerequisite"), "10", "弹药台建造技能")
    bench_comps = [li.attrib.get("Class") for li in bench.findall("./comps/li")]
    for expected in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
        if expected not in bench_comps:
            fail(f"弹药台缺少组件: {expected}")
    assert_equal(child_text(bench, "./comps/li/basePowerConsumption"), "500", "弹药台功耗")

    research_defs = def_map(parse_xml(PETROCHEM_EXT_RESEARCH_XML))
    assert_equal(set(research_defs), set(PETROCHEM_EXT_RESEARCH_EXPECTATIONS), "石化扩展研究 defName 集合")
    for name, expected in PETROCHEM_EXT_RESEARCH_EXPECTATIONS.items():
        node = research_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "baseCost"), expected["cost"], f"{name} baseCost")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "tab"), "MMT_ResearchTab", f"{name} tab")
        assert_equal(li_values(node, "prerequisites"), expected["prereqs"], f"{name} 前置")
        assert_equal(child_text(node, "researchViewX"), expected["x"], f"{name} X 坐标")
        assert_equal(child_text(node, "researchViewY"), expected["y"], f"{name} Y 坐标")

    recipe_defs = def_map(parse_xml(PETROCHEM_EXT_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PETROCHEM_EXT_RECIPE_EXPECTATIONS), "石化扩展配方 defName 集合")

    hand_table_recipes = li_values(def_map(parse_xml(PETROCHEM_BUILDING_XML))["MMT_HandChemTable"], "recipes")
    electric_lab_recipes = li_values(def_map(parse_xml(ELECTRIC_PETROCHEM_BUILDING_XML))["MMT_ElectricChemLab"], "recipes")
    loom_recipes = li_values(building_defs["MMT_Loom"], "recipes")
    ammo_bench_recipes = li_values(building_defs["MMT_AmmoBench"], "recipes")
    bench_membership = {
        "MMT_MakeParaffins": [hand_table_recipes, electric_lab_recipes],
        "MMT_MakeSulphates": [hand_table_recipes, electric_lab_recipes],
        "MMT_MakeCarbonAlloy": [electric_lab_recipes],
        "MMT_MakeCompaste": [loom_recipes],
        "MMT_MakeSulfur": [ammo_bench_recipes],
        "MMT_MakePowder": [ammo_bench_recipes],
        "MMT_MakeNapalm": [ammo_bench_recipes],
        "MMT_MakeOxidizedChemfuel": [electric_lab_recipes],
    }

    resource_names = (
        set(resource_defs)
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(SULFUR_RESOURCE_XML)))
        | set(def_map(parse_xml(AGRICULTURE_RESOURCE_XML)))
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(CROPS_RESOURCE_XML)))
    )
    allowed_categories = {"Alcohol", "MMT_Coal"}

    for name, expected in PETROCHEM_EXT_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        effect = recipe.findtext("effectWorking")
        assert_equal(effect.strip() if effect else None, expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")

        if expected["skill"] is None:
            if recipe.find("skillRequirements") is not None:
                fail(f"{name} 不应有技能要求")
        else:
            assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")
            if "skill2" in expected:
                skill2_name, skill2_value = expected["skill2"]
                assert_equal(child_text(recipe, f"./skillRequirements/{skill2_name}"), skill2_value, f"{name} {skill2_name} requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            categories = li_values(filter_node, "categories")
            if bool(thing_defs) == bool(categories):
                fail(f"{name} 每个输入 filter 必须且只能包含 thingDefs 或 categories 之一")
            if thing_defs:
                actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
            else:
                actual_inputs.append(("categories", tuple(categories), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed_things = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_things = sorted({item for kind, value, _ in normalized if kind == "thingDefs" for item in value})
        assert_equal(fixed_things, expected_things, f"{name} 固定输入列表")
        fixed_categories = sorted(li_values(recipe, "./fixedIngredientFilter/categories"))
        expected_categories = sorted({item for kind, value, _ in normalized if kind == "categories" for item in value})
        assert_equal(fixed_categories, expected_categories, f"{name} 固定输入类别")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        research = recipe.findtext("researchPrerequisite")
        assert_equal(research.strip() if research else None, expected["research"], f"{name} researchPrerequisite")

        users = li_values(recipe, "recipeUsers")
        if "users" in expected:
            assert_equal(users, expected["users"], f"{name} recipeUsers")
        elif users:
            fail(f"{name} 不应写 recipeUsers")

        if "target_count_adjustment" in expected:
            assert_equal(child_text(recipe, "targetCountAdjustment"), expected["target_count_adjustment"], f"{name} targetCountAdjustment")

        for bench_list in bench_membership.get(name, []):
            if name not in bench_list:
                fail(f"{name} 未挂载到目标建筑")
        if name not in bench_membership and not expected.get("users"):
            fail(f"{name} 未声明挂载方式")

        for kind, value, _ in normalized:
            for item in value:
                if kind == "thingDefs":
                    if item not in resource_names | VANILLA_THINGS:
                        fail(f"{name} 使用未知资源: {item}")
                elif item not in allowed_categories:
                    fail(f"{name} 使用未知类别: {item}")
        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已知资源: {expected['output']}")

    if ">SmithingSpeed<" in read_text(PETROCHEM_EXT_RECIPE_XML):
        fail("石化扩展配方不得使用未加 MMT_ 前缀的 HSK 属性 SmithingSpeed")

    for path in PETROCHEM_EXT_ITEM_TEXTURES:
        assert_png(path)

    for path in PETROCHEM_EXT_BUILDING_TEXTURES:
        assert_png(path)

    for path in (PETROCHEM_EXT_RESOURCE_XML, PETROCHEM_EXT_BUILDING_XML, PETROCHEM_EXT_RECIPE_XML, PETROCHEM_EXT_RESEARCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("petrochem-ext: OK")


def validate_matter_top() -> None:
    require_files([
        MATTER_RESOURCE_XML,
        SUPER_ALLOYS_RESOURCE_XML,
        MATTER_BUILDING_XML,
        MATTER_RECIPE_XML,
        TOP_MATERIALS_RECIPE_XML,
        MATTER_RESEARCH_XML,
        METAL_RESOURCE_XML,
        ADVANCED_ORE_RESOURCE_XML,
        ADVANCED_ALLOY_RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        PETROCHEM_EXT_RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        ELECTRIC_PETROCHEM_BUILDING_XML,
        PRECIOUS_RESOURCE_XML,
        *MATTER_ITEM_TEXTURES,
        *MATTER_BUILDING_TEXTURES,
    ])

    matter_defs = def_map(parse_xml(MATTER_RESOURCE_XML))
    assert_equal(set(matter_defs), set(MATTER_RESOURCE_EXPECTATIONS), "无机物系资源 defName 集合")
    for name, expected in MATTER_RESOURCE_EXPECTATIONS.items():
        node = matter_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flammable"], f"{name} Flammability")
        assert_equal(child_text(node, "./statBases/DeteriorationRate"), "0" if name != "MMT_Antimatter" else "0.01", f"{name} DeteriorationRate")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["class"], f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")
    if matter_defs["MMT_Antimatter"].find("./comps/li[@Class='CompProperties_Explosive']") is None:
        fail("MMT_Antimatter 缺少爆炸组件")
    if matter_defs["MMT_Matter"].find("./comps/li[@Class='CompProperties_Glower']") is None:
        fail("MMT_Matter 缺少发光组件")

    alloy_defs = def_map(parse_xml(SUPER_ALLOYS_RESOURCE_XML))
    assert_equal(set(alloy_defs), set(SUPER_ALLOYS_RESOURCE_EXPECTATIONS), "顶级合金资源 defName 集合")
    for name, expected in SUPER_ALLOYS_RESOURCE_EXPECTATIONS.items():
        node = alloy_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
        assert_equal(child_text(node, "useHitPoints"), "true", f"{name} useHitPoints")
        stuff_categories = [li.text for li in node.findall("./stuffProps/categories/li")]
        assert_equal([c for c in stuff_categories if c == "Metallic"], ["Metallic"], f"{name} stuff 含 Metallic")
        assert_equal(child_text(node, "./stuffProps/stuffAdjective"), expected["adjective"], f"{name} stuffAdjective")
        assert_equal(child_text(node, "./stuffProps/statFactors/MaxHitPoints"), expected["hp"], f"{name} MaxHitPoints factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/WorkToMake"), expected["work_make"], f"{name} WorkToMake factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/WorkToBuild"), expected["work_build"], f"{name} WorkToBuild factor")
        assert_equal(child_text(node, "./stuffProps/statFactors/Mass"), expected["mass"], f"{name} Mass factor")
        assert_equal(child_text(node, "terrainAffordanceNeeded"), "Medium", f"{name} terrain affordance")
    bioferrite_categories = alloy_defs["MMT_BioferriteAlloy"].findall("./stuffProps/categories/li")
    anomaly_li = [li for li in bioferrite_categories if (li.text or "").strip() == "Bioferrite"]
    if len(anomaly_li) != 1 or anomaly_li[0].attrib.get("MayRequire") != "Ludeon.RimWorld.Anomaly":
        fail("MMT_BioferriteAlloy 的 Bioferrite 材质类别须带 MayRequire Anomaly")
    for name in ("MMT_AlphaPoly", "MMT_BetaPoly"):
        if "MeleePenetrationFactor" in read_text(SUPER_ALLOYS_RESOURCE_XML):
            fail("顶级合金不得使用 HSK 专属的 MeleePenetrationFactor")

    building_defs = def_map(parse_xml(MATTER_BUILDING_XML))
    assert_equal(set(building_defs), set(MATTER_BUILDING_EXPECTATIONS), "物质系建筑 defName 集合")
    for name, expected in MATTER_BUILDING_EXPECTATIONS.items():
        node = building_defs[name]
        assert_equal(node.attrib.get("ParentName"), "BuildingBase", f"{name} 父类")
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "thingClass"), "Building_WorkTable", f"{name} thingClass")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["class"], f"{name} graphicClass")
        assert_equal(child_text(node, "./graphicData/drawSize"), expected["draw"], f"{name} drawSize")
        assert_equal(li_values(node, "stuffCategories"), ["Metallic"], f"{name} 材料分类")
        assert_equal(child_text(node, "costStuffCount"), expected["stuff_count"], f"{name} 材料数量")
        assert_equal(child_text(node, "./statBases/WorkToBuild"), expected["work_build"], f"{name} 建造工作量")
        assert_equal(child_text(node, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(node, "size"), expected["size"], f"{name} 尺寸")
        assert_equal(child_text(node, "designationCategory"), "MMT_Production", f"{name} 建筑分类")
        assert_equal(li_values(node, "recipes"), expected["recipes"], f"{name} 配方列表")
        assert_equal(li_values(node, "researchPrerequisites"), expected["research"], f"{name} 研究前置")
        assert_equal(child_text(node, "constructionSkillPrerequisite"), expected["skill"], f"{name} 建造技能")
        comps = [li.attrib.get("Class") for li in node.findall("./comps/li")]
        for comp in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable", "CompProperties_HeatPusher"):
            if comp not in comps:
                fail(f"{name} 缺少组件: {comp}")
        assert_equal(child_text(node, "./comps/li/basePowerConsumption"), expected["power"], f"{name} 功耗")

    research_defs = def_map(parse_xml(MATTER_RESEARCH_XML))
    assert_equal(set(research_defs), set(MATTER_RESEARCH_EXPECTATIONS), "物质转换研究 defName 集合")
    for name, expected in MATTER_RESEARCH_EXPECTATIONS.items():
        node = research_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "baseCost"), expected["cost"], f"{name} baseCost")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "tab"), "MMT_ResearchTab", f"{name} tab")
        assert_equal(li_values(node, "prerequisites"), expected["prereqs"], f"{name} 前置")
        assert_equal(child_text(node, "researchViewX"), expected["x"], f"{name} X 坐标")
        assert_equal(child_text(node, "researchViewY"), expected["y"], f"{name} Y 坐标")

    resource_names = (
        set(matter_defs)
        | set(alloy_defs)
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(ADVANCED_ORE_RESOURCE_XML)))
        | set(def_map(parse_xml(ADVANCED_ALLOY_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_EXT_RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(PRECIOUS_RESOURCE_XML)))
    )
    allowed_categories = {"ResourcesRaw", "MMT_USLDBar", "MMT_SLDBar", "MMT_Coal", "Corpses", "MeatRaw"}

    converter_recipes = li_values(building_defs["MMT_MatterConverter"], "recipes")
    matterfab_recipes = li_values(building_defs["MMT_Matterfab"], "recipes")
    collider_recipes = li_values(building_defs["MMT_ParticleCollider"], "recipes")
    electric_lab_recipes = li_values(def_map(parse_xml(ELECTRIC_PETROCHEM_BUILDING_XML))["MMT_ElectricChemLab"], "recipes")
    bench_membership = {
        **{name: [converter_recipes] for name in MATTER_RECIPE_EXPECTATIONS},
        "MMT_ProduceAPoly": [matterfab_recipes],
        "MMT_ProduceBPoly": [matterfab_recipes],
        "MMT_MakeAntimatter": [collider_recipes],
        "MMT_ExtractBiomatterCorpse": [electric_lab_recipes],
        "MMT_ExtractBiomatterMeat": [electric_lab_recipes],
    }

    all_recipe_expectations = {**MATTER_RECIPE_EXPECTATIONS, **TOP_MATERIALS_RECIPE_EXPECTATIONS}
    matter_recipe_defs = def_map(parse_xml(MATTER_RECIPE_XML))
    top_recipe_defs = def_map(parse_xml(TOP_MATERIALS_RECIPE_XML))
    assert_equal(set(matter_recipe_defs), set(MATTER_RECIPE_EXPECTATIONS), "物质转换配方 defName 集合")
    assert_equal(set(top_recipe_defs), set(TOP_MATERIALS_RECIPE_EXPECTATIONS), "顶级材料配方 defName 集合")

    for name, expected in all_recipe_expectations.items():
        recipe = matter_recipe_defs.get(name, top_recipe_defs.get(name))
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")
        if name in MATTER_RECIPE_EXPECTATIONS:
            assert_equal(child_text(recipe, "workSpeedStat"), "MMT_ElectronicCraftingSpeed", f"{name} workSpeedStat")
        else:
            assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            categories = li_values(filter_node, "categories")
            if bool(thing_defs) == bool(categories):
                fail(f"{name} 每个输入 filter 必须且只能包含 thingDefs 或 categories 之一")
            if thing_defs:
                actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
            else:
                actual_inputs.append(("categories", tuple(categories), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed_things = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_things = sorted({item for kind, value, _ in normalized if kind == "thingDefs" for item in value})
        assert_equal(fixed_things, expected_things, f"{name} 固定输入列表")
        fixed_categories = sorted(li_values(recipe, "./fixedIngredientFilter/categories"))
        expected_categories = sorted({item for kind, value, _ in normalized if kind == "categories" for item in value})
        assert_equal(fixed_categories, expected_categories, f"{name} 固定输入类别")

        products = {node.tag: (node.text or "").strip() for node in recipe.findall("./products/*") if isinstance(node.tag, str)}
        expected_products = {expected["output"]: expected["output_count"], **expected.get("extra_products", {})}
        assert_equal(products, expected_products, f"{name} 输出")

        research = recipe.findtext("researchPrerequisite")
        assert_equal(research.strip() if research else None, expected["research"], f"{name} researchPrerequisite")

        users = li_values(recipe, "recipeUsers")
        if "users" in expected:
            assert_equal(users, expected["users"], f"{name} recipeUsers")
        elif users:
            fail(f"{name} 不应写 recipeUsers")

        for bench_list in bench_membership.get(name, []):
            if name not in bench_list:
                fail(f"{name} 未挂载到目标建筑")
        if name not in bench_membership and "users" not in expected:
            fail(f"{name} 未声明挂载方式")

        for kind, value, _ in normalized:
            for item in value:
                if kind == "thingDefs":
                    if item not in resource_names | VANILLA_THINGS:
                        fail(f"{name} 使用未知资源: {item}")
                elif item not in allowed_categories:
                    fail(f"{name} 使用未知类别: {item}")
        for output_name in expected_products:
            if output_name not in resource_names | VANILLA_THINGS:
                fail(f"{name} 输出不是已知资源: {output_name}")

    if top_recipe_defs["MMT_MakeBioferriteAlloy"].attrib.get("MayRequire") != "Ludeon.RimWorld.Anomaly":
        fail("MMT_MakeBioferriteAlloy 须整条带 MayRequire Anomaly")

    for path in MATTER_ITEM_TEXTURES:
        assert_png(path)
    for path in MATTER_BUILDING_TEXTURES:
        assert_png(path)

    for path in (MATTER_RESOURCE_XML, SUPER_ALLOYS_RESOURCE_XML, MATTER_BUILDING_XML, MATTER_RECIPE_XML, TOP_MATERIALS_RECIPE_XML, MATTER_RESEARCH_XML):
        text = read_text(path)
        if "SK." in text:
            fail(f"{path} 不得引用 SK 类型")
        if "Bulk" in text:
            fail(f"{path} 不得使用 CE 专属的 Bulk")
    print("matter-top: OK")


def validate_precious() -> None:
    require_files([PRECIOUS_RESOURCE_XML, PRECIOUS_RECIPE_XML, METAL_BUILDING_XML, VANILLA_VEIN_PATCH_XML, *PRECIOUS_ITEM_TEXTURES])

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

    for path in (PRECIOUS_RESOURCE_XML, PRECIOUS_RECIPE_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("precious: OK")


def validate_vanilla_veins() -> None:
    """原版矿脉改造 patch：校验 4 条矿脉被 Replace 为 MMT 矿石，且零件矿脉不被触碰。"""
    require_files([VANILLA_VEIN_PATCH_XML])
    patch = parse_xml(VANILLA_VEIN_PATCH_XML)

    # 按目标 def 归集 Replace 操作
    replaced = {}
    for op in patch.iter():
        if not op.attrib.get("Class", "").endswith("PatchOperationReplace"):
            continue
        xpath = child_text(op, "xpath")
        target = xpath.split('defName="')[1].split('"')[0]
        node_name = xpath.rstrip("/").split("/")[-1]
        replaced.setdefault(target, {})[node_name] = op.find("value")

    assert_equal(set(replaced), set(VANILLA_VEIN_EXPECTATIONS), "patch 改造的矿脉 def 集合")
    for name, expected in VANILLA_VEIN_EXPECTATIONS.items():
        nodes = replaced[name]
        for node_name in ("label", "description", "graphicData", "statBases", "building"):
            if node_name not in nodes:
                fail(f"{name} 缺少 {node_name} 的 Replace 操作")
        assert_equal(child_text(nodes["label"], "label"), expected["label"], f"{name} label")
        assert_equal(child_text(nodes["graphicData"], "graphicData/texPath"), "Things/Building/Linked/RockFlecked_Atlas", f"{name} texPath")
        assert_equal(child_text(nodes["statBases"], "statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        building = nodes["building"].find("building")
        assert_equal(child_text(building, "isResourceRock"), "true", f"{name} isResourceRock")
        assert_equal(child_text(building, "mineableThing"), expected["yield_thing"], f"{name} mineableThing")
        assert_equal(child_text(building, "mineableYield"), expected["yield"], f"{name} mineableYield")
        assert_equal(child_text(building, "mineableScatterCommonality"), expected["commonality"], f"{name} commonality")
        assert_equal(child_text(building, "mineableScatterLumpSizeRange"), expected["lump"], f"{name} lump")
        assert_equal(child_text(building, "veinMineable"), "true", f"{name} veinMineable")

    patch_text = read_text(VANILLA_VEIN_PATCH_XML)
    for kept in VANILLA_VEIN_KEEP:
        if f'defName="{kept}"' in patch_text:
            fail(f"patch 不得触碰照 HSK 保留原版的矿脉: {kept}")
    if "SK." in patch_text:
        fail(f"{VANILLA_VEIN_PATCH_XML} 不得引用 SK 类型")
    print("vanilla-veins: OK")


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
            if node.get("ParentName") == "MMT_TurretBuildingBase":
                # 炮塔使用原版 Security 分类（继承自基类），由 weapons-turrets 套件单独校验
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


def validate_textiles_weapons() -> None:
    require_files([
        CROPS_RESOURCE_XML,
        CROPS_PLANT_XML,
        CROPS_PATCH_XML,
        STATS_XML,
        INSECT_CHITIN_PATCH_XML,
        TEXTILES_RESOURCE_XML,
        TEXTILES_RECIPE_XML,
        WEAPON_PARTS_RESOURCE_XML,
        WEAPON_PARTS_BUILDING_XML,
        WEAPON_PARTS_RECIPE_XML,
        TEXTILES_WEAPONS_RESEARCH_XML,
        PETROCHEM_EXT_BUILDING_XML,
        WOOD_RESOURCE_XML,
        PARTS_RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        MATTER_RESOURCE_XML,
        RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        PETROCHEM_EXT_RESOURCE_XML,
        *CROPS_ITEM_TEXTURES,
        *TEXTILES_ITEM_TEXTURES,
        *WEAPON_PARTS_ITEM_TEXTURES,
        *WEAPON_PARTS_BUILDING_TEXTURES,
    ])
    require_dirs(CROPS_PLANT_TEXTURE_DIRS)

    # --- 8.1 作物资源 ---
    crop_defs = def_map(parse_xml(CROPS_RESOURCE_XML))
    assert_equal(set(crop_defs), set(CROPS_RESOURCE_EXPECTATIONS), "作物资源 defName 集合")
    for name, expected in CROPS_RESOURCE_EXPECTATIONS.items():
        node = crop_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        if "stack" in expected:
            assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
        if "mass" in expected:
            assert_equal(child_text(node, "./statBases/Mass"), expected["mass"], f"{name} Mass（可搬运物品必须显式声明质量）")
        if expected.get("food"):
            assert_equal(child_text(node, "socialPropernessMatters"), "true", f"{name} socialPropernessMatters（preferability 高于 DesperateOnlyForHumanlikes 时必须为 true）")
            assert_equal(child_text(node, "./ingestible/foodType") is not None, True, f"{name} ingestible foodType")
            assert_equal(child_text(node, "./ingestible/preferability") is not None, True, f"{name} ingestible preferability")
            if node.find("./ingestible/joy") is not None:
                if node.find("./ingestible/joyKind") is None:
                    fail(f"{name} joy > 0 必须声明 joyKind")
        tex = child_text(node, "./graphicData/texPath")
        if not tex.startswith(f"Things/Item/MMT/MMT_Crops/{name}"):
            fail(f"{name} texPath 不在 MMT_Crops 目录: {tex}")

    # --- 8.1 植物 ---
    plant_defs = def_map(parse_xml(CROPS_PLANT_XML))
    assert_equal(set(plant_defs), set(CROPS_PLANT_EXPECTATIONS), "作物植物 defName 集合")
    for name, expected in CROPS_PLANT_EXPECTATIONS.items():
        node = plant_defs[name]
        assert_equal(child_text(node, "./plant/harvestedThingDef"), expected["harvest"], f"{name} 收获物")
        assert_equal(child_text(node, "./plant/harvestYield"), expected["yield"], f"{name} 产量")
        assert_equal(child_text(node, "./plant/growDays"), expected["grow"], f"{name} 生长天数")
        sow_node = node.find("./plant/sowTags")
        sow = [li.text for li in sow_node.findall("li")] if sow_node is not None else []
        assert_equal(sow, expected["sow"], f"{name} 播种标签")
        skill_node = node.find("./plant/sowMinSkill")
        if expected["skill"] is None:
            if skill_node is not None:
                fail(f"{name} 不应定义播种技能")
        else:
            assert_equal((skill_node.text or "").strip() if skill_node is not None else None, expected["skill"], f"{name} 播种技能")
        if node.find("./plant/sowResearchPrerequisites") is not None:
            fail(f"{name} 不得带 HSK 播种研究前置")
        tex = child_text(node, "./graphicData/texPath")
        if not tex.startswith(f"Things/Plant/MMT/{name}"):
            fail(f"{name} texPath 不在 MMT 植物目录: {tex}")

    # --- 8.1 自定义工作速度 StatDef（配方的 workSpeedStat 必须引用 MMT_ 前缀自研属性） ---
    stat_defs = def_map(parse_xml(STATS_XML))
    for stat_name, label in (("MMT_SmithingSpeed", "锻造速度"), ("MMT_ElectronicCraftingSpeed", "电子装配速度")):
        if stat_name not in stat_defs:
            fail(f"缺少 StatDef {stat_name}")
        node = stat_defs[stat_name]
        assert_equal(child_text(node, "label"), label, f"{stat_name} label")
        assert_equal(child_text(node, "category"), "PawnWork", f"{stat_name} category")
        assert_equal(child_text(node, "defaultBaseValue"), "1", f"{stat_name} defaultBaseValue")

    # --- 8.1/8.2 patch 文本断言 ---
    crop_patch = read_text(CROPS_PATCH_XML)
    for target, product in (("Plant_Cotton", "MMT_RawCotton"), ("Plant_Devilstrand", "MMT_RawDevilstrand"), ("Plant_TreeBamboo", "MMT_Bamboo"), ("Plant_TreePalm", "MMT_RawCoconut")):
        if f'defName="{target}"' not in crop_patch or product not in crop_patch:
            fail(f"作物 patch 缺少 {target} → {product}")
    insect_patch = read_text(INSECT_CHITIN_PATCH_XML)
    if insect_patch.count("PatchOperationConditional") != 3:
        fail("甲壳 patch 应为 3 个条件操作")
    for race, count in (("Megascarab", "30"), ("Spelopede", "45"), ("Megaspider", "60")):
        if race not in insect_patch:
            fail(f"甲壳 patch 缺少 {race}")
    if insect_patch.count("MMT_Chitin") < 3:
        fail("甲壳 patch 缺少 MMT_Chitin 掉落")
    infra_patch = read_text(INFRA_PATCH_XML)
    if 'defName="Cloth"' not in infra_patch or "MMT_BTextiles" not in infra_patch:
        fail("原版类别 patch 缺少 Cloth → MMT_BTextiles")

    # --- 8.2 织物资源 ---
    textile_defs = def_map(parse_xml(TEXTILES_RESOURCE_XML))
    assert_equal(set(textile_defs), set(TEXTILES_RESOURCE_EXPECTATIONS), "织物资源 defName 集合")
    for name, expected in TEXTILES_RESOURCE_EXPECTATIONS.items():
        node = textile_defs[name]
        if "parent" in expected:
            assert_equal(node.attrib.get("ParentName"), expected["parent"], f"{name} 父类")
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        if "categories" in expected:
            assert_equal(li_values(node, "thingCategories"), expected["categories"], f"{name} thingCategories")
        if "class" in expected:
            assert_equal(child_text(node, "./graphicData/graphicClass"), expected["class"], f"{name} graphicClass")
            tex = child_text(node, "./graphicData/texPath")
            if not tex.startswith(f"Things/Item/MMT/MMT_Textiles/{name}"):
                fail(f"{name} texPath 不在 MMT_Textiles 目录: {tex}")

    # --- 8.2 织物配方 ---
    textile_recipe_defs = def_map(parse_xml(TEXTILES_RECIPE_XML))
    assert_equal(set(textile_recipe_defs), set(TEXTILES_RECIPE_EXPECTATIONS), "织物配方 defName 集合")
    loom_recipes = li_values(def_map(parse_xml(PETROCHEM_EXT_BUILDING_XML))["MMT_Loom"], "recipes")

    resource_names = (
        set(crop_defs)
        | set(textile_defs)
        | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_EXT_RESOURCE_XML)))
        | set(def_map(parse_xml(PARTS_RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(MATTER_RESOURCE_XML)))
        | set(def_map(parse_xml(RESOURCE_XML)))
        | set(def_map(parse_xml(WEAPON_PARTS_RESOURCE_XML)))
    )
    allowed_categories = {"PlantMatter", "PlantFoodRaw", "MeatRaw", "MMT_BTextiles", "MMT_MetallicBar", "MMT_SLDBar", "MMT_USLDBar", "MMT_HCMBar", "MMT_FuelCat"}

    for name, expected in TEXTILES_RECIPE_EXPECTATIONS.items():
        recipe = textile_recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        skill = expected["skill"]
        if isinstance(skill, tuple):
            assert_equal(child_text(recipe, f"./skillRequirements/{skill[0]}"), skill[1], f"{name} {skill[0]} requirement")
        else:
            assert_equal(child_text(recipe, "./skillRequirements/Crafting"), skill, f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            categories = li_values(filter_node, "categories")
            if bool(thing_defs) == bool(categories):
                fail(f"{name} 每个输入 filter 必须且只能包含 thingDefs 或 categories 之一")
            if thing_defs:
                actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
            else:
                actual_inputs.append(("categories", tuple(categories), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        products = {node.tag: (node.text or "").strip() for node in recipe.findall("./products/*") if isinstance(node.tag, str)}
        assert_equal(products, {expected["output"]: expected["output_count"]}, f"{name} 输出")

        research = recipe.findtext("researchPrerequisite")
        assert_equal(research.strip() if research else None, expected["research"], f"{name} researchPrerequisite")

        if expected.get("bench") == "loom":
            if name not in loom_recipes:
                fail(f"{name} 未挂载到纺织机")
            if li_values(recipe, "recipeUsers"):
                fail(f"{name} 不应写 recipeUsers")
        else:
            assert_equal(li_values(recipe, "recipeUsers"), expected["users"], f"{name} recipeUsers")

        for kind, value, _ in normalized:
            for item in value:
                if kind == "thingDefs":
                    if item not in resource_names | VANILLA_THINGS:
                        fail(f"{name} 使用未知资源: {item}")
                elif item not in allowed_categories:
                    fail(f"{name} 使用未知类别: {item}")
        if expected["output"] not in resource_names | VANILLA_THINGS:
            fail(f"{name} 输出不是已知资源: {expected['output']}")

    # --- 8.3 武器部件资源 ---
    weapon_defs = def_map(parse_xml(WEAPON_PARTS_RESOURCE_XML))
    assert_equal(set(weapon_defs), set(WEAPON_PARTS_RESOURCE_EXPECTATIONS), "武器部件资源 defName 集合")
    for name, expected in WEAPON_PARTS_RESOURCE_EXPECTATIONS.items():
        node = weapon_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["MMT_WeaponParts"], f"{name} thingCategories")
        assert_equal(child_text(node, "./graphicData/texPath"), f"Things/Item/MMT/MMT_WeaponParts/{name}", f"{name} texPath")

    # --- 8.3 武器加工建筑 ---
    weapon_building_defs = def_map(parse_xml(WEAPON_PARTS_BUILDING_XML))
    assert_equal(set(weapon_building_defs), set(WEAPON_PARTS_BUILDING_EXPECTATIONS), "武器加工建筑 defName 集合")
    for name, expected in WEAPON_PARTS_BUILDING_EXPECTATIONS.items():
        node = weapon_building_defs[name]
        assert_equal(node.attrib.get("ParentName"), "BuildingBase", f"{name} 父类")
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "thingClass"), "Building_WorkTable", f"{name} thingClass")
        assert_equal(child_text(node, "size"), expected["size"], f"{name} 尺寸")
        assert_equal(child_text(node, "./statBases/WorkToBuild"), expected["work_build"], f"{name} 建造工作量")
        assert_equal(child_text(node, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(node, "costStuffCount"), expected["stuff_count"], f"{name} 材料数量")
        assert_equal(child_text(node, "designationCategory"), "MMT_Production", f"{name} 建筑分类")
        assert_equal(li_values(node, "researchPrerequisites"), expected["research"], f"{name} 研究前置")
        assert_equal(child_text(node, "constructionSkillPrerequisite"), expected["skill"], f"{name} 建造技能")
        comps = [li.attrib.get("Class") for li in node.findall("./comps/li")]
        for comp in ("CompProperties_Power", "CompProperties_Flickable", "CompProperties_Breakdownable"):
            if comp not in comps:
                fail(f"{name} 缺少组件: {comp}")
        assert_equal(child_text(node, "./comps/li/basePowerConsumption"), expected["power"], f"{name} 功耗")
    # 建筑 recipes 列表与配方挂载一一对应
    for building_name in WEAPON_PARTS_BUILDING_EXPECTATIONS:
        expected_list = [r for r, e in WEAPON_PARTS_RECIPE_EXPECTATIONS.items() if building_name in e["benches"]]
        assert_equal(li_values(weapon_building_defs[building_name], "recipes"), expected_list, f"{building_name} 配方列表")

    # --- 8.3 武器部件配方 ---
    weapon_recipe_defs = def_map(parse_xml(WEAPON_PARTS_RECIPE_XML))
    assert_equal(set(weapon_recipe_defs), set(WEAPON_PARTS_RECIPE_EXPECTATIONS), "武器部件配方 defName 集合")
    for name, expected in WEAPON_PARTS_RECIPE_EXPECTATIONS.items():
        recipe = weapon_recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "MMT_SmithingSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            thing_defs = li_values(filter_node, "thingDefs")
            categories = li_values(filter_node, "categories")
            if bool(thing_defs) == bool(categories):
                fail(f"{name} 每个输入 filter 必须且只能包含 thingDefs 或 categories 之一")
            if thing_defs:
                actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
            else:
                actual_inputs.append(("categories", tuple(categories), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        products = {node.tag: (node.text or "").strip() for node in recipe.findall("./products/*") if isinstance(node.tag, str)}
        assert_equal(products, {expected["output"]: expected["output_count"]}, f"{name} 输出")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")
        if research and research not in VANILLA_RESEARCH and not research.startswith("MMT_"):
            fail(f"{name} 使用未知研究: {research}")
        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载")

        for kind, value, _ in normalized:
            for item in value:
                if kind == "thingDefs":
                    if item not in resource_names | VANILLA_THINGS:
                        fail(f"{name} 使用未知资源: {item}")
                elif item not in allowed_categories:
                    fail(f"{name} 使用未知类别: {item}")

    # --- 研究 ---
    research_defs = def_map(parse_xml(TEXTILES_WEAPONS_RESEARCH_XML))
    assert_equal(set(research_defs), set(TEXTILES_WEAPONS_RESEARCH_EXPECTATIONS), "织物武器研究 defName 集合")
    for name, expected in TEXTILES_WEAPONS_RESEARCH_EXPECTATIONS.items():
        node = research_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "baseCost"), expected["cost"], f"{name} baseCost")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "tab"), "MMT_ResearchTab", f"{name} tab")
        assert_equal(li_values(node, "prerequisites"), expected["prereqs"], f"{name} 前置")
        assert_equal(child_text(node, "researchViewX"), expected["x"], f"{name} X 坐标")
        assert_equal(child_text(node, "researchViewY"), expected["y"], f"{name} Y 坐标")

    # --- 贴图 ---
    for path in (*CROPS_ITEM_TEXTURES, *TEXTILES_ITEM_TEXTURES, *WEAPON_PARTS_ITEM_TEXTURES, *WEAPON_PARTS_BUILDING_TEXTURES):
        assert_png(path)
    for directory in CROPS_PLANT_TEXTURE_DIRS:
        if not list((ROOT / directory).glob("*.png")):
            fail(f"植物贴图目录为空: {directory}")

    for path in (CROPS_RESOURCE_XML, CROPS_PLANT_XML, TEXTILES_RESOURCE_XML, TEXTILES_RECIPE_XML, WEAPON_PARTS_RESOURCE_XML, WEAPON_PARTS_BUILDING_XML, WEAPON_PARTS_RECIPE_XML, TEXTILES_WEAPONS_RESEARCH_XML, CROPS_PATCH_XML, INSECT_CHITIN_PATCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("textiles-weapons: OK")


def validate_all_xml() -> None:
    paths = [Path("About/About.xml"), Path("LoadFolders.xml")]
    defs_dir = ROOT / "1.7"
    if defs_dir.exists():
        paths.extend(path.relative_to(ROOT) for path in defs_dir.rglob("*.xml"))
    for path in paths:
        parse_xml(path.as_posix())
    print(f"all-xml: OK ({len(paths)} files)")


def validate_basic_metallurgy() -> None:
    """B2 基础冶金：陶瓷链资源/窑炉/燃烧配方/灰烬 patch 的专项校验。

    金属侧资源（铅/铝/斯特）由 metal/electric 套件覆盖（B2 已同步期望），
    本套件聚焦新建文件与例外项（MetalCan、MakeMetalCan 的 categories 输入、灰烬 patch）。
    """
    require_files([
        CERAMICS_RESOURCE_XML,
        CERAMICS_RECIPE_XML,
        CERAMICS_BUILDING_XML,
        STUFFCATEGORY_XML,
        ASH_PATCH_XML,
        METAL_RESOURCE_XML,
        ELECTRIC_BUILDING_XML,
        ELECTRIC_RECIPE_XML,
        WOOD_BUILDING_XML,
        *CERAMICS_ITEM_TEXTURES,
    ])

    # StuffCategoryDef
    stuffcat_defs = def_map(parse_xml(STUFFCATEGORY_XML))
    assert_equal(set(stuffcat_defs), {"MMT_Bricks"}, "stuff 类别 defName 集合")
    assert_equal(child_text(stuffcat_defs["MMT_Bricks"], "label"), "砖块", "MMT_Bricks label")

    # 陶瓷链资源
    resource_defs = def_map(parse_xml(CERAMICS_RESOURCE_XML))
    assert_equal(set(resource_defs), {"MMT_SoftClay", "MMT_ClayBrick", "MMT_Ceramics", "MMT_Ash"}, "陶瓷资源 defName 集合")

    clay = resource_defs["MMT_SoftClay"]
    assert_equal(child_text(clay, "label"), "软粘土", "软粘土 label")
    assert_equal(child_text(clay, "./statBases/MarketValue"), "1", "软粘土 MarketValue")
    assert_equal(child_text(clay, "stackLimit"), "600", "软粘土 stackLimit")
    assert_equal(li_values(clay, "thingCategories"), ["ResourcesRaw"], "软粘土 thingCategories")

    brick = resource_defs["MMT_ClayBrick"]
    assert_equal(child_text(brick, "label"), "粘土砖", "粘土砖 label")
    assert_equal(child_text(brick, "./statBases/MarketValue"), "3", "粘土砖 MarketValue")
    assert_equal(li_values(brick, "thingCategories"), ["Manufactured"], "粘土砖 thingCategories")
    assert_equal(li_values(brick, "./stuffProps/categories"), ["MMT_Bricks"], "粘土砖 stuff categories")

    ceramics = resource_defs["MMT_Ceramics"]
    assert_equal(child_text(ceramics, "label"), "陶瓷", "陶瓷 label")
    assert_equal(child_text(ceramics, "./statBases/MarketValue"), "12", "陶瓷 MarketValue")
    assert_equal(li_values(ceramics, "thingCategories"), ["Manufactured"], "陶瓷 thingCategories")
    assert_equal(li_values(ceramics, "./stuffProps/categories"), ["Stony"], "陶瓷 stuff categories")

    ash = resource_defs["MMT_Ash"]
    assert_equal(child_text(ash, "label"), "灰烬", "灰烬 label")
    assert_equal(child_text(ash, "./statBases/MarketValue"), "0.3", "灰烬 MarketValue")
    assert_equal(child_text(ash, "./statBases/Beauty"), "-5", "灰烬 Beauty")
    assert_equal(child_text(ash, "tradeability"), "None", "灰烬 tradeability")
    assert_equal(li_values(ash, "thingCategories"), ["ResourcesRaw"], "灰烬 thingCategories")

    # 金属罐（例外项：非 stuff、Manufactured，不在 METAL_BAR_EXPECTATIONS 结构内）
    metal_defs = def_map(parse_xml(METAL_RESOURCE_XML))
    can = metal_defs["MMT_MetalCan"]
    assert_equal(child_text(can, "label"), "金属罐", "金属罐 label")
    assert_equal(child_text(can, "./statBases/MarketValue"), "1.7", "金属罐 MarketValue")
    assert_equal(child_text(can, "stackLimit"), "200", "金属罐 stackLimit")
    assert_equal(li_values(can, "thingCategories"), ["Manufactured"], "金属罐 thingCategories")
    if can.find("stuffProps") is not None:
        fail("金属罐不应定义 stuffProps")

    # 粘土矿脉（窑炉已并入 MMT_CharcoalKiln，由 wood 套件校验其建筑本体）
    building_defs = def_map(parse_xml(CERAMICS_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableSoftClay"}, "陶瓷建筑 defName 集合")

    vein = building_defs["MMT_MineableSoftClay"]
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_SoftClay", "粘土矿脉产出")
    assert_equal(child_text(vein, "./building/mineableYield"), "75", "粘土矿脉产量")
    assert_equal(child_text(vein, "./building/mineableScatterCommonality"), "0.7", "粘土矿脉散布")
    assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), "8~15", "粘土矿脉矿团")

    # 窑炉（= MMT_CharcoalKiln，木材链建筑兼任）的陶瓷链职责：材料分类/配方/研究
    kiln = def_map(parse_xml(WOOD_BUILDING_XML))["MMT_CharcoalKiln"]
    assert_equal(child_text(kiln, "label"), "窑炉", "窑炉 label")
    assert_equal(li_values(kiln, "stuffCategories"), ["Stony", "MMT_Bricks"], "窑炉材料分类")
    assert_equal(child_text(kiln, "designationCategory"), "MMT_Production", "窑炉建造分类")
    kiln_recipes = li_values(kiln, "recipes")
    for name in ("MMT_MakeCeramics", "MMT_MakeClayBricks", "MMT_BurnThings"):
        if name not in kiln_recipes:
            fail(f"{name} 未挂载到窑炉")

    # 陶瓷系配方 + 燃烧配方
    recipe_defs = def_map(parse_xml(CERAMICS_RECIPE_XML))
    assert_equal(set(recipe_defs), {"MMT_MakeCeramics", "MMT_MakeClayBricks", "MMT_BurnThings", "MMT_CremateCorpseCampfire"}, "陶瓷配方 defName 集合")

    make_ceramics = recipe_defs["MMT_MakeCeramics"]
    assert_equal(child_text(make_ceramics, "workAmount"), "1200", "陶瓷 workAmount")
    assert_equal(child_text(make_ceramics, "./skillRequirements/Crafting"), "8", "陶瓷技能")
    assert_equal(child_text(make_ceramics, "researchPrerequisite"), "MMT_BasicMetalworking", "陶瓷研究")

    make_bricks = recipe_defs["MMT_MakeClayBricks"]
    assert_equal(child_text(make_bricks, "workAmount"), "800", "粘土砖 workAmount")
    assert_equal(child_text(make_bricks, "./skillRequirements/Crafting"), "6", "粘土砖技能")
    assert_equal(child_text(make_bricks, "researchPrerequisite"), "MMT_BasicMetalworking", "粘土砖研究")

    burn = recipe_defs["MMT_BurnThings"]
    assert_equal(child_text(burn, "workAmount"), "400", "焚烧 workAmount")
    assert_equal(sorted(li_values(burn, "recipeUsers")), ["Campfire", "ElectricCrematorium"], "焚烧挂载建筑")
    assert_equal(sorted(li_values(burn, "./ingredients/li/filter/categories")), ["Leathers", "Textiles"], "焚烧输入类别")
    assert_equal(li_values(burn, "./ingredients/li/filter/thingDefs"), ["WoodLog"], "焚烧输入物品")
    assert_equal(child_text(burn, "./ingredients/li/count"), "20", "焚烧输入数量")
    ash_products = [(node.tag, (node.text or "").strip()) for node in burn.findall("./products/*")]
    assert_equal(ash_products, [("MMT_Ash", "3")], "焚烧产出")

    cremate = recipe_defs["MMT_CremateCorpseCampfire"]
    assert_equal(child_text(cremate, "workAmount"), "800", "篝火火化 workAmount")
    assert_equal(li_values(cremate, "recipeUsers"), ["Campfire"], "篝火火化挂载")
    assert_equal(child_text(cremate, "autoStripCorpses"), "true", "篝火火化 autoStripCorpses（对齐 HSK 同名配方）")
    cremate_products = [(node.tag, (node.text or "").strip()) for node in cremate.findall("./products/*")]
    assert_equal(cremate_products, [("MMT_Ash", "4")], "篝火火化产出")

    # 陶瓷两条配方同时挂 EAF（HSK 挂 ElectricSmelter+Kiln，MMT 电配方统一挂 EAF）
    eaf = def_map(parse_xml(ELECTRIC_BUILDING_XML))["MMT_ElectricArcFurnace"]
    eaf_recipes = li_values(eaf, "recipes")
    for name in ("MMT_MakeCeramics", "MMT_MakeClayBricks"):
        if name not in eaf_recipes:
            fail(f"{name} 未挂载到电弧炉")

    # MakeMetalCan：唯一的 categories 输入配方（不塞 ELECTRIC_RECIPE_EXPECTATIONS 的原因）
    can_recipe = def_map(parse_xml(ELECTRIC_RECIPE_XML))["MMT_MakeMetalCan"]
    assert_equal(child_text(can_recipe, "workAmount"), "800", "金属罐 workAmount")
    assert_equal(li_values(can_recipe, "./ingredients/li/filter/categories"), ["MMT_MetallicBar"], "金属罐输入类别")
    assert_equal(child_text(can_recipe, "./ingredients/li/count"), "8", "金属罐输入数量")
    can_products = [(node.tag, (node.text or "").strip()) for node in can_recipe.findall("./products/*")]
    assert_equal(can_products, [("MMT_MetalCan", "15")], "金属罐产出")
    assert_equal(child_text(can_recipe, "researchPrerequisite"), "MMT_ElectricSmelting", "金属罐研究")

    # 灰烬 patch：xpath 目标与产出数值
    patch_text = read_text(ASH_PATCH_XML)
    for needle in (
        'Defs/RecipeDef[defName="BurnApparel"]',
        'Defs/RecipeDef[defName="CremateCorpse"]',
        "<MMT_Ash>2</MMT_Ash>",
        "<MMT_Ash>4</MMT_Ash>",
        "PatchOperationConditional",
    ):
        if needle not in patch_text:
            fail(f"灰烬 patch 缺少: {needle}")

    for path in CERAMICS_ITEM_TEXTURES:
        assert_png(path)

    for path in (CERAMICS_RESOURCE_XML, CERAMICS_RECIPE_XML, CERAMICS_BUILDING_XML, STUFFCATEGORY_XML, ASH_PATCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("basic-metallurgy: OK")


def validate_stone_construction() -> None:
    """B3 石料建材：农业辅料资源/盐链/电动混凝土线的专项校验。"""
    require_files([
        AGRICULTURE_RESOURCE_XML,
        AGRICULTURE_RECIPE_XML,
        AGRICULTURE_RESEARCH_XML,
        AGRICULTURE_BUILDING_XML,
        *AGRICULTURE_ITEM_TEXTURES,
        *AGRICULTURE_BUILDING_TEXTURES,
    ])

    # 农业辅料资源
    resource_defs = def_map(parse_xml(AGRICULTURE_RESOURCE_XML))
    assert_equal(set(resource_defs), {"MMT_Compost", "MMT_Fertilizer", "MMT_Dirt", "MMT_Salt"}, "农业资源 defName 集合")

    compost = resource_defs["MMT_Compost"]
    assert_equal(child_text(compost, "label"), "堆肥料", "堆肥 label")
    assert_equal(child_text(compost, "./statBases/MarketValue"), "2.4", "堆肥 MarketValue")
    assert_equal(child_text(compost, "stackLimit"), "300", "堆肥 stackLimit")
    assert_equal(child_text(compost, "./statBases/DeteriorationRate"), "5", "堆肥 DeteriorationRate")
    assert_equal(child_text(compost, "tradeability"), "None", "堆肥 tradeability")
    assert_equal(li_values(compost, "thingCategories"), ["ResourcesRaw"], "堆肥 thingCategories")

    fertilizer = resource_defs["MMT_Fertilizer"]
    assert_equal(child_text(fertilizer, "label"), "肥料", "肥料 label")
    assert_equal(child_text(fertilizer, "./statBases/MarketValue"), "3.8", "肥料 MarketValue")
    assert_equal(child_text(fertilizer, "stackLimit"), "300", "肥料 stackLimit")
    assert_equal(child_text(fertilizer, "./statBases/DeteriorationRate"), "5", "肥料 DeteriorationRate")
    assert_equal(li_values(fertilizer, "thingCategories"), ["ResourcesRaw"], "肥料 thingCategories")

    dirt = resource_defs["MMT_Dirt"]
    assert_equal(child_text(dirt, "label"), "土堆", "土堆 label")
    assert_equal(child_text(dirt, "./statBases/MarketValue"), "0.2", "土堆 MarketValue")
    assert_equal(child_text(dirt, "stackLimit"), "900", "土堆 stackLimit")
    assert_equal(child_text(dirt, "./statBases/DeteriorationRate"), "0", "土堆 DeteriorationRate")
    assert_equal(li_values(dirt, "thingCategories"), ["ResourcesRaw"], "土堆 thingCategories")

    salt = resource_defs["MMT_Salt"]
    assert_equal(child_text(salt, "label"), "盐", "盐 label")
    assert_equal(child_text(salt, "./statBases/MarketValue"), "1", "盐 MarketValue")
    assert_equal(child_text(salt, "stackLimit"), "600", "盐 stackLimit")
    assert_equal(li_values(salt, "thingCategories"), ["ResourcesRaw"], "盐 thingCategories")
    if salt.find("./statBases/Nutrition") is not None or salt.find("ingestible") is not None:
        fail("盐不应带食物属性（MMT 不做食物链）")
    if salt.find("deepCountPerPortion") is not None:
        fail("盐不应带深钻井参数（属非目标）")

    # 盐矿脉 + 挖盐点
    building_defs = def_map(parse_xml(AGRICULTURE_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableSalt", "MMT_SaltMine"}, "农业建筑 defName 集合")

    vein = building_defs["MMT_MineableSalt"]
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_Salt", "盐矿脉产出")
    assert_equal(child_text(vein, "./building/mineableYield"), "150", "盐矿脉产量")
    assert_equal(child_text(vein, "./building/mineableScatterCommonality"), "1.2", "盐矿脉散布")
    assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), "5~10", "盐矿脉矿团")

    mine = building_defs["MMT_SaltMine"]
    assert_equal(child_text(mine, "thingClass"), "Building_WorkTable", "挖盐点 thingClass")
    assert_equal(child_text(mine, "size"), "(1,1)", "挖盐点尺寸")
    assert_equal(child_text(mine, "terrainAffordanceNeeded"), "SmoothableStone", "挖盐点地形需求")
    assert_equal(child_text(mine, "designationCategory"), "MMT_Production", "挖盐点建造分类")
    assert_equal(li_values(mine, "recipes"), ["MMT_MineSalt"], "挖盐点配方列表")

    # 农业配方（堆肥/肥料/土堆×2/挖盐）
    recipe_defs = def_map(parse_xml(AGRICULTURE_RECIPE_XML))
    assert_equal(set(recipe_defs), {"MMT_MakeCompost", "MMT_MakeFertilizer", "MMT_MakeDirt", "MMT_MakeDirt_Hand", "MMT_MineSalt"}, "农业配方 defName 集合")

    make_compost = recipe_defs["MMT_MakeCompost"]
    assert_equal(child_text(make_compost, "workAmount"), "625", "堆肥 workAmount")
    assert_equal(sorted(li_values(make_compost, "recipeUsers")), ["ButcherSpot", "ElectricStove", "FueledStove", "TableButcher"], "堆肥挂载建筑")
    assert_equal(sorted(li_values(make_compost, "./ingredients/li[1]/filter/categories")), ["AnimalProductRaw", "MeatRaw"], "堆肥输入1类别")
    assert_equal(child_text(make_compost, "./ingredients/li[1]/count"), "25", "堆肥输入1数量")
    assert_equal(li_values(make_compost, "./ingredients/li[2]/filter/categories"), ["PlantFoodRaw"], "堆肥输入2类别")
    assert_equal(sorted(li_values(make_compost, "./ingredients/li[2]/filter/thingDefs")), ["Hay", "MMT_Ash"], "堆肥输入2物品")
    assert_equal(child_text(make_compost, "./ingredients/li[2]/count"), "5", "堆肥输入2数量")
    assert_equal(li_values(make_compost, "./fixedIngredientFilter/categories"), ["FoodRaw"], "堆肥固定过滤类别")
    assert_equal(sorted(li_values(make_compost, "./fixedIngredientFilter/thingDefs")), ["Hay", "MMT_Ash"], "堆肥固定过滤物品")
    compost_products = [(node.tag, (node.text or "").strip()) for node in make_compost.findall("./products/*")]
    assert_equal(compost_products, [("MMT_Compost", "5")], "堆肥产出")
    assert_equal(child_text(make_compost, "researchPrerequisite"), "MMT_AgricultureBasic", "堆肥研究")

    make_fertilizer = recipe_defs["MMT_MakeFertilizer"]
    assert_equal(child_text(make_fertilizer, "workAmount"), "10000", "肥料 workAmount")
    assert_equal(sorted(li_values(make_fertilizer, "recipeUsers")), ["ButcherSpot", "ElectricStove", "FueledStove", "TableButcher"], "肥料挂载建筑")
    assert_equal(li_values(make_fertilizer, "./ingredients/li/filter/thingDefs"), ["MMT_Compost"], "肥料输入")
    assert_equal(child_text(make_fertilizer, "./ingredients/li/count"), "10", "肥料输入数量")
    fertilizer_products = [(node.tag, (node.text or "").strip()) for node in make_fertilizer.findall("./products/*")]
    assert_equal(fertilizer_products, [("MMT_Fertilizer", "10")], "肥料产出")
    assert_equal(child_text(make_fertilizer, "researchPrerequisite"), "MMT_AgricultureBasic", "肥料研究")

    make_dirt = recipe_defs["MMT_MakeDirt"]
    assert_equal(child_text(make_dirt, "workAmount"), "300", "土堆 workAmount")
    assert_equal(li_values(make_dirt, "recipeUsers"), ["ButcherSpot"], "土堆挂载建筑")
    dirt_products = [(node.tag, (node.text or "").strip()) for node in make_dirt.findall("./products/*")]
    assert_equal(dirt_products, [("MMT_Dirt", "3")], "土堆产出")
    assert_equal(child_text(make_dirt, "researchPrerequisite"), "MMT_AgricultureBasic", "土堆研究")

    make_dirt_hand = recipe_defs["MMT_MakeDirt_Hand"]
    assert_equal(child_text(make_dirt_hand, "workAmount"), "3000", "手动土堆 workAmount")
    assert_equal(li_values(make_dirt_hand, "recipeUsers"), [], "手动土堆不写 recipeUsers（建筑 recipes 列表挂载）")
    hand_dirt_products = [(node.tag, (node.text or "").strip()) for node in make_dirt_hand.findall("./products/*")]
    assert_equal(hand_dirt_products, [("MMT_Dirt", "30")], "手动土堆产出")
    assert_equal(child_text(make_dirt_hand, "researchPrerequisite"), "MMT_ConcreteBasic", "手动土堆研究")

    mine_salt = recipe_defs["MMT_MineSalt"]
    assert_equal(child_text(mine_salt, "workAmount"), "1800", "挖盐 workAmount")
    assert_equal(child_text(mine_salt, "workSkill"), "Mining", "挖盐 workSkill")
    salt_products = [(node.tag, (node.text or "").strip()) for node in mine_salt.findall("./products/*")]
    assert_equal(salt_products, [("MMT_Salt", "5")], "挖盐产出")

    # 手动搅拌器挂载手动土堆
    concrete_building_defs = def_map(parse_xml(CONCRETE_BUILDING_XML))
    mixer_recipes = li_values(concrete_building_defs["MMT_ConcreteMixer"], "recipes")
    if "MMT_MakeDirt_Hand" not in mixer_recipes:
        fail("MMT_MakeDirt_Hand 未挂载到手动混凝土搅拌机")

    # 电动搅拌机
    e_mixer = concrete_building_defs["MMT_EConcreteMixer"]
    assert_equal(child_text(e_mixer, "label"), "电动混凝土搅拌机", "电动搅拌机 label")
    assert_equal(child_text(e_mixer, "thingClass"), "Building_WorkTable", "电动搅拌机 thingClass")
    assert_equal(child_text(e_mixer, "size"), "(2,1)", "电动搅拌机尺寸")
    assert_equal(child_text(e_mixer, "costStuffCount"), "115", "电动搅拌机材料数")
    assert_equal(child_text(e_mixer, "./statBases/WorkToBuild"), "5000", "电动搅拌机 WorkToBuild")
    assert_equal(child_text(e_mixer, "designationCategory"), "MMT_Production", "电动搅拌机建造分类")
    assert_equal(child_text(e_mixer, "./comps/li[@Class='CompProperties_Power']/basePowerConsumption"), "500", "电动搅拌机耗电")
    assert_equal(li_values(e_mixer, "researchPrerequisites"), ["MMT_ConcreteAdvanced"], "电动搅拌机研究")
    assert_equal(child_text(e_mixer, "constructionSkillPrerequisite"), "11", "电动搅拌机建造技能")
    e_mixer_costs = [(node.tag, (node.text or "").strip()) for node in e_mixer.findall("./costList/*")]
    assert_equal(e_mixer_costs, [("MMT_Rubber", "45"), ("ComponentIndustrial", "10"), ("MMT_ElectronicParts", "5"), ("MMT_Mechanism", "5")], "电动搅拌机造价")
    assert_equal(sorted(li_values(e_mixer, "recipes")), ["MMT_MakeDirt_Electric", "MMT_MixConcrete_Electric"], "电动搅拌机配方列表")

    # 混凝土电动配方
    concrete_recipe_defs = def_map(parse_xml(CONCRETE_RECIPE_XML))
    for name in ("MMT_MixConcrete_Electric", "MMT_MakeReinforcedConcrete_Electric", "MMT_MakeDirt_Electric"):
        if name not in concrete_recipe_defs:
            fail(f"{name} 不存在于混凝土配方文件")

    mix_electric = concrete_recipe_defs["MMT_MixConcrete_Electric"]
    assert_equal(child_text(mix_electric, "workAmount"), "600", "电动混凝土 workAmount")
    assert_equal(child_text(mix_electric, "./skillRequirements/Crafting"), "6", "电动混凝土技能")
    assert_equal(child_text(mix_electric, "researchPrerequisite"), "MMT_ConcreteAdvanced", "电动混凝土研究")
    mix_products = [(node.tag, (node.text or "").strip()) for node in mix_electric.findall("./products/*")]
    assert_equal(mix_products, [("MMT_Concrete", "15")], "电动混凝土产出")

    reinforced_electric = concrete_recipe_defs["MMT_MakeReinforcedConcrete_Electric"]
    assert_equal(child_text(reinforced_electric, "workAmount"), "1200", "电动钢筋混凝土 workAmount")
    assert_equal(child_text(reinforced_electric, "./skillRequirements/Crafting"), "7", "电动钢筋混凝土技能")
    assert_equal(li_values(reinforced_electric, "./ingredients/li[1]/filter/categories"), ["MMT_MetallicBar"], "电动钢筋混凝土输入类别")
    assert_equal(child_text(reinforced_electric, "./ingredients/li[1]/count"), "20", "电动钢筋混凝土输入数量")
    reinforced_products = [(node.tag, (node.text or "").strip()) for node in reinforced_electric.findall("./products/*")]
    assert_equal(reinforced_products, [("MMT_ReinforcedConcrete", "16")], "电动钢筋混凝土产出")
    if reinforced_electric.find("researchPrerequisite") is not None:
        fail("电动钢筋混凝土不应设研究前置（对齐 HSK，靠装配台建筑前置）")

    dirt_electric = concrete_recipe_defs["MMT_MakeDirt_Electric"]
    assert_equal(child_text(dirt_electric, "workAmount"), "1500", "电动土堆 workAmount")
    e_dirt_products = [(node.tag, (node.text or "").strip()) for node in dirt_electric.findall("./products/*")]
    assert_equal(e_dirt_products, [("MMT_Dirt", "30")], "电动土堆产出")
    assert_equal(child_text(dirt_electric, "researchPrerequisite"), "MMT_ConcreteAdvanced", "电动土堆研究")

    # 电动装配台挂载电动钢筋混凝土
    parts_building_defs = def_map(parse_xml(PARTS_BUILDING_XML))
    bench_recipes = li_values(parts_building_defs["MMT_ElectricAssemblyBench"], "recipes")
    if "MMT_MakeReinforcedConcrete_Electric" not in bench_recipes:
        fail("MMT_MakeReinforcedConcrete_Electric 未挂载到电动装配台")

    # 研究
    agriculture_research = def_map(parse_xml(AGRICULTURE_RESEARCH_XML))["MMT_AgricultureBasic"]
    assert_equal(child_text(agriculture_research, "baseCost"), "150", "农业 I baseCost")
    assert_equal(child_text(agriculture_research, "techLevel"), "Neolithic", "农业 I techLevel")
    assert_equal(child_text(agriculture_research, "tab"), "MMT_ResearchTab", "农业 I tab")
    assert_equal(child_text(agriculture_research, "researchViewX"), "0.00", "农业 I 坐标X")
    assert_equal(child_text(agriculture_research, "researchViewY"), "2.00", "农业 I 坐标Y")

    concrete_research = def_map(parse_xml(CONCRETE_RESEARCH_XML))["MMT_ConcreteAdvanced"]
    assert_equal(child_text(concrete_research, "baseCost"), "1000", "混凝土 II baseCost")
    assert_equal(child_text(concrete_research, "techLevel"), "Industrial", "混凝土 II techLevel")
    assert_equal(child_text(concrete_research, "tab"), "MMT_ResearchTab", "混凝土 II tab")
    assert_equal(li_values(concrete_research, "prerequisites"), ["MMT_ConcreteBasic"], "混凝土 II 前置")
    assert_equal(child_text(concrete_research, "researchViewX"), "8.00", "混凝土 II 坐标X")
    assert_equal(child_text(concrete_research, "researchViewY"), "0.00", "混凝土 II 坐标Y")

    for path in (*AGRICULTURE_ITEM_TEXTURES, *AGRICULTURE_BUILDING_TEXTURES):
        assert_png(path)

    for path in (AGRICULTURE_RESOURCE_XML, AGRICULTURE_RECIPE_XML, AGRICULTURE_RESEARCH_XML, AGRICULTURE_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("stone-construction: OK")


WEAPON_MAPPING_JSON = "tools/data/mmt_weapon_mapping.json"
WEAPON_GENERATOR_PY = "tools/build_mmt_weapons_from_hsk.py"
WEAPON_BATCH_FILES = {
    "conventional": [
        "Weapons_Pistols.xml", "Weapons_SMGs.xml", "Weapons_Carabines.xml",
        "Weapons_Rifles.xml", "Weapons_Shotguns.xml", "Weapons_SniperRifles.xml",
        "Weapons_SniperRiflesAdvanced.xml", "Weapons_Heavy.xml",
        "Weapons_Launchers.xml", "Weapons_Flamethrower.xml",
    ],
    "archaic": [
        "Weapons_Bows.xml", "Weapons_Crossbows.xml",
        "Weapons_Melee_Neolithic.xml", "Weapons_Melee_Medieval.xml",
        "Weapons_Melee_Midworld.xml", "Weapons_Melee_Hightech.xml",
        "Weapons_Grenades.xml",
    ],
    "futuristic": [
        "Weapons_Archotech.xml", "Weapons_Blasters.xml", "Weapons_Charged.xml",
        "Weapons_Lasers.xml", "Weapons_Plasma.xml",
    ],
    "turrets": [
        "Weapons_Turrets_Heavy.xml", "Weapons_Turrets_Launchers.xml",
        "Weapons_Turrets_Machineguns.xml", "Weapons_Turrets_Sentry.xml",
    ],
    "npc": [
        "Weapons_Mechanoid.xml", "Weapons_Ogre.xml",
        "Weapons_RangedSpecial.xml", "Weapons_Special.xml",
    ],
}


TURRET_AMMO_PROJECTILES = {
    "MMT_Ammo_BalistaBolt": "MMT_Projectile_BalistaBolt",
    "MMT_Ammo_StoneBall": "MMT_Stone_Ball",
    "MMT_Ammo_30x29mmGrenade_HE": "MMT_Bullet_30x29mmGrenade_HE",
    "MMT_Ammo_120mmCannonShell_HEAT": "MMT_Bullet_120mmCannonShell_HEAT",
    "MMT_Ammo_155mmHowitzerShell_HE": "MMT_Bullet_155mmHowitzerShell_HE",
    "MMT_Ammo_SPG9Rocket_Frag": "MMT_Bullet_SPG9Rocket_Frag",
    "MMT_Ammo_130mmRocketMissile": "MMT_Bullet_130mmRocketMissile",
    "MMT_Ammo_50BMG_FMJ": "MMT_Bullet_50BMG_FMJ",
    "MMT_Ammo_762x51mmNATO_FMJ": "MMT_Bullet_762x51mmNATO_FMJ",
    "MMT_Ammo_30x173mm_FMJ": "MMT_Bullet_30x173mm_FMJ",
    "MMT_Ammo_20x102mm_FMJ": "MMT_Bullet_20x102mm_FMJ",
}


GAME_DATA_DIR = ROOT.parent / "RimWorld v1.7.4979" / "Data"
if not GAME_DATA_DIR.is_dir():
    GAME_DATA_DIR = Path("D:/RimWorld v1.7.4979/Data")

WEAPON_REFERENCE_FIELDS = {
    "defaultProjectile", "projectileWhenLoaded", "turretGunDef", "minifiedDef",
    "unfinishedThingDef", "soundCast", "soundCastTail", "soundInteract", "soundDrop",
    "soundWorking", "effectWorking", "damageDef", "researchPrerequisite",
    "workSpeedStat", "workSkill",
}
WEAPON_REFERENCE_LISTS = {
    "thingDefs", "categories", "recipeUsers", "prerequisites", "researchPrerequisites",
    "stuffCategories", "thingCategories", "affectedDamageDefs", "capacities", "weaponClasses",
}
WEAPON_REFERENCE_DICTS = {"products", "costList", "smeltProducts"}
WEAPON_GENERATED_FILES = [
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Base.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml",
    "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml",
    "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml",
    "1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml",
    "1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml",
    "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml",
    "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml",
    "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml",
    "1.7/Defs/RecipeDefs/MMT_Recipes_TurretAmmo.xml",
    "1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml",
    "1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml",
    "1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml",
]


def collect_known_def_names() -> set[str]:
    """可解析名集合：MMT 全部 Def + 原版与全 DLC Def。"""
    known: set[str] = set()
    for pattern_root in (ROOT / "1.7/Defs", GAME_DATA_DIR):
        for path in sorted(pattern_root.glob("**/*.xml") if pattern_root == ROOT / "1.7/Defs" else pattern_root.glob("*/Defs/**/*.xml")):
            try:
                root = ET.parse(path).getroot()
            except ET.ParseError:
                continue
            for node in root.iter():
                if not isinstance(node.tag, str):
                    continue
                name = node.findtext("defName") or node.get("Name")
                if name:
                    known.add(name)
    return known


def validate_weapon_references() -> None:
    """武器相关生成文件的引用闭合：所有被引用 def 必须在 MMT 或原版/DLC 中存在。"""
    known = collect_known_def_names()
    dangling: list[str] = []
    for rel in WEAPON_GENERATED_FILES:
        root = parse_xml(rel)
        for node in root.iter():
            if not isinstance(node.tag, str):
                continue
            if node.tag in WEAPON_REFERENCE_FIELDS and node.text and node.text.strip() not in known:
                dangling.append(f"{rel}: {node.tag}={node.text.strip()}")
        for node in root.iter():
            if node.tag in WEAPON_REFERENCE_LISTS:
                for li in node.findall("li"):
                    if li.text and li.text.strip() not in known:
                        dangling.append(f"{rel}: {node.tag}/li={li.text.strip()}")
        for node in root.iter():
            if node.tag in WEAPON_REFERENCE_DICTS:
                for child in node:
                    if child.tag not in known:
                        dangling.append(f"{rel}: {node.tag}/{child.tag}")
    if dangling:
        for entry in dangling[:20]:
            print(f"  [dangling] {entry}")
        fail(f"武器引用闭合检查失败：{len(dangling)} 个悬空引用")
    print("weapons-references: OK")


def validate_weapons() -> None:
    validate_weapons_core()
    validate_weapons_projectiles()
    validate_weapons_research()
    validate_weapons_conventional()
    validate_weapons_archaic()
    validate_weapons_futuristic()
    validate_weapons_turrets()
    validate_weapons_npc()
    validate_weapon_references()
    weapon_files = [
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml",
    ]
    total_weapons = sum(len(def_map(parse_xml(p))) for p in weapon_files)
    assert_equal(total_weapons, 155, "武器总目标数（57+41+28+16+13，内联投射物在 MMT_Projectiles.xml）")
    generated_text = "\n".join(read_text(p) for p in [
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Base.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml",
        *weapon_files,
        "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml",
        "1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml",
        "1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml",
        "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml",
        "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml",
        "1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml",
        "1.7/Defs/RecipeDefs/MMT_Recipes_TurretAmmo.xml",
        "1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml",
        "1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml",
        "1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml",
    ])
    for forbidden in ("CombatExtended.", "SK.", "RightToolForJob."):
        if forbidden in generated_text:
            fail(f"武器 Def 残留禁用引用: {forbidden}")
    print("weapons: OK")


def validate_weapons_npc() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml"))
    assert_equal(len(weapons), 13, "NPC/特殊武器数（Bullet_FireExtinguisher 在 MMT_Projectiles.xml）")
    assert "MMT_Gun_mechlaser" in weapons
    assert "MMT_Gun_SawGun" in weapons
    assert "CombatExtended." not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml")
    print("weapons-npc: OK")


def validate_weapons_turrets() -> None:
    guns = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml"))
    buildings = def_map(parse_xml("1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml"))
    ammo = def_map(parse_xml("1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml"))
    crates = def_map(parse_xml("1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml"))
    crate_recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml"))
    assert_equal(len(guns), 16, "炮塔枪数")
    concrete_buildings = [b for b in buildings.values() if (b.get("Abstract") or "").lower() != "true"]
    building_total = len(parse_xml("1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml").findall("ThingDef"))
    assert_equal(building_total, 17, "炮塔建筑文件 Def 数（含 MMT_TurretBuildingBase）")
    assert_equal(len(concrete_buildings), 16, "可建造炮塔数")
    assert_equal(len(ammo), 11, "炮塔弹药数")
    assert_equal(len(crates), 16, "炮塔箱数（9 个无对应建筑的孤儿箱不移植）")
    assert_equal(len(crate_recipes), 16, "炮塔箱配方数")
    for ammo_name, projectile_name in TURRET_AMMO_PROJECTILES.items():
        assert_equal(child_text(ammo[ammo_name], "projectileWhenLoaded"), projectile_name, f"{ammo_name} 投射物")

    m2 = buildings["MMT_Turret_M2Browning"]
    assert_equal(child_text(m2, "building/turretGunDef"), "MMT_Gun_M2Browning", "M2 炮塔枪")
    refuelable = m2.find("./comps/li[@Class='CompProperties_Refuelable']")
    if refuelable is None:
        fail("M2 炮塔缺少 refuelable")
    assert_equal(child_text(refuelable, "fuelFilter/thingDefs/li"), "MMT_Ammo_50BMG_FMJ", "M2 弹药")
    assert_equal(child_text(refuelable, "fuelCapacity"), "150", "M2 弹容量")
    assert_equal(child_text(guns["MMT_Gun_M2Browning"], "verbs/li/consumeFuelPerShot"), "1", "M2 单发耗弹")
    print("weapons-turrets: OK")


def validate_weapons_futuristic() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml"))
    assert_equal(len(weapons), 28, "未来武器数")
    assert_equal(len(recipes), 12, "未来武器配方数（4 条 ReverseEngineer 已排除）")
    laser = weapons["MMT_LaserRifle"]
    assert_equal(child_text(laser, "verbs/li/defaultProjectile"), "MMT_Bullet_Laserbeam", "激光步枪投射物")
    assert "MMT_Gun_ChargeRifle" in weapons
    assert "SK.Graphic_SingleNoColor" not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml")
    print("weapons-futuristic: OK")


def validate_weapons_archaic() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml"))
    assert_equal(len(weapons), 41, "冷兵器与手雷 Def 数（6 个手雷投射物在 MMT_Projectiles.xml）")
    assert_equal(len(recipes), 13, "手雷配方数")
    greatsword = weapons["MMT_MeleeWeapon_GreatSword"]
    assert_equal(child_text(greatsword, "tools/li/power"), "7", "巨剑首个工具威力")
    assert "MMT_Weapon_GrenadeFrag" in weapons
    assert "MMT_CE_Weapon_GrenadeSmoke" in weapons
    assert "CombatExtended.ToolCE" not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml")
    print("weapons-archaic: OK")


def validate_weapons_conventional() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml"))
    assert_equal(len(weapons), 57, "常规武器数")
    assert_equal(len(recipes), 58, "常规武器配方数")

    deringer = weapons["MMT_RF_Gun_Deringer"]
    assert_equal(child_text(deringer, "label"), "M1917史密斯威森左轮", "Deringer 中文名")
    assert_equal(child_text(deringer, "verbs/li/defaultProjectile"), "MMT_Bullet_45ACP_FMJ", "Deringer 投射物")
    assert_equal(child_text(deringer, "verbs/li/range"), "31", "Deringer 射程")
    assert_equal(child_text(deringer, "statBases/RangedWeapon_Cooldown"), "0.31", "Deringer 冷却")

    build = recipes["MMT_BuildHandGun"]
    assert "MMT_Gun_Revolver" in weapons
    assert_equal(child_text(build, "products/MMT_Gun_Revolver"), "1", "左轮产品")
    if "<Gun_Revolver>" in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml"):
        fail("常规武器配方残留未加 MMT_ 前缀的 Gun_Revolver")
    assert "MMT_PistolComponent" in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml")
    assert "Weapon_Parts" not in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml")
    print("weapons-conventional: OK")


WEAPON_RESEARCH_TOTAL = 23


def validate_weapons_research() -> None:
    research = def_map(parse_xml("1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml"))
    assert_equal(len(research), WEAPON_RESEARCH_TOTAL, "武器研究节点数")
    coords = [(child_text(r, "researchViewX"), child_text(r, "researchViewY")) for r in research.values()]
    assert_equal(len(coords), len(set(coords)), "武器研究坐标重复")
    assert_equal(child_text(research["MMT_Rocketry2"], "baseCost"), "1300", "火箭 II 成本")
    assert_equal(li_values(research["MMT_Rocketry2"], "prerequisites"), ["MMT_Rocketry1", "SniperTurret", "MMT_GrenadeLaunchers", "Mortars"], "火箭 II 前置")
    print("weapons-research: OK")


VANILLA_DAMAGE_DEFS = {
    "AcidBurn", "Arrow", "ArrowHighVelocity", "Beam", "BeamBypassShields", "Bite", "Blunt",
    "Bomb", "BombSuper", "Bullet", "BulletToxic", "Bullet_TraitIncendiary", "Bullet_TraitTox",
    "Burn", "Crush", "Cut", "DeadlifeDust", "Decayed", "Demolish", "Deterioration", "Digested",
    "EMP", "ElectricalBurn", "EnergyBolt", "ExecutionCut", "Extinguish", "Flame", "Frostbite",
    "MechBandShockwave", "Mining", "MiningBomb", "Nerve", "NerveStun", "NociosphereVaporize",
    "Poke", "PorcupineBite", "PorcupineScratch", "Psychic", "RangedStab", "Rotting", "Scratch",
    "ScratchToxic", "Smoke", "Stab", "Stun", "SurgicalCut", "Thump", "TornadoScratch", "ToxGas",
    "ToxicBite", "VacuumBurn", "Vaporize",
}


def validate_weapons_projectiles() -> None:
    projectiles = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml"))
    bullet = projectiles["MMT_Bullet_762x51mmNATO_FMJ"]
    assert_equal(child_text(bullet, "projectile/damageDef"), "Bullet", "7.62 伤害类型")
    assert_equal(child_text(bullet, "projectile/damageAmountBase"), "20", "7.62 伤害")
    assert_equal(child_text(bullet, "projectile/armorPenetrationBase"), "0.07", "7.62 穿甲")
    assert_equal(child_text(bullet, "projectile/speed"), "156", "7.62 速度")

    fifty = projectiles["MMT_Bullet_50BMG_FMJ"]
    assert_equal(child_text(fifty, "projectile/damageAmountBase"), "42", ".50 伤害")
    assert_equal(child_text(fifty, "projectile/armorPenetrationBase"), "0.14", ".50 穿甲")

    for name, node in projectiles.items():
        damage_def = child_text(node, "projectile/damageDef")
        if damage_def not in VANILLA_DAMAGE_DEFS:
            fail(f"{name} 引用非原版 damageDef: {damage_def}")

    sounds = def_map(parse_xml("1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml"))
    if "MMT_Shot_Tavor" not in sounds:
        fail("Tavor 音频实际存在（Sounds/Tavor/IWI Tavor.ogg），必须生成 MMT_Shot_Tavor")
    if "MMT_FlameThrower" in sounds:
        fail("BurningLoop 音频缺失，FlameThrower 必须走原版 fallback，不得生成 MMT SoundDef")
    print("weapons-projectiles: OK")


def validate_weapons_core() -> None:
    mapping = json.loads(read_text(WEAPON_MAPPING_JSON))
    assert_equal(mapping["thing_prefix"], "MMT_", "武器映射前缀")
    assert_equal(mapping["batches"]["conventional"], WEAPON_BATCH_FILES["conventional"], "常规枪械批次")
    assert_equal(mapping["sound_fallbacks"]["FlameThrower"], "Shot_MiniFlameblaster", "火焰音效 fallback")
    assert_equal(len(mapping["sound_fallbacks"]), 1, "仅真正缺失的音频才允许 fallback")
    assert_equal(mapping["workbenches"]["HeavyArmsBench"], "MMT_AdvWeaponCraftingTable", "重型武器台映射")
    assert_equal(mapping["workbenches"]["TableHeavyAmmunition"], "MMT_AmmoBench", "重弹药台映射")
    assert_equal(len(mapping["recipe_product_clones"]["patch_replace"]), 17, "patch 克隆产物数")
    assert_equal(len(mapping["recipe_product_clones"]["combat_extended_defs"]), 5, "CE 克隆产物数")
    assert_equal(len(mapping["patch_roots"]), 2, "patch 根目录数")
    print("weapons-core: OK")


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
    validate_vanilla_veins()
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
    validate_basic_metallurgy()
    validate_stone_construction()
    validate_petrochem_ext()
    validate_matter_top()
    validate_textiles_weapons()
    validate_weapons()
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
    "robotics": validate_robotics,
    "electric": validate_electric,
    "electric-wood": validate_electric_wood,
    "electric-petrochem": validate_electric_petrochem,
    "electric-electronics": validate_electric_electronics,
    "advanced-alloys": validate_advanced_alloys,
    "parts": validate_parts,
    "coal": validate_coal,
    "sulfur": validate_sulfur,
    "precious": validate_precious,
    "vanilla-veins": validate_vanilla_veins,
    "basic-metallurgy": validate_basic_metallurgy,
    "stone-construction": validate_stone_construction,
    "petrochem-ext": validate_petrochem_ext,
    "matter-top": validate_matter_top,
    "textiles-weapons": validate_textiles_weapons,
    "weapons-core": validate_weapons_core,
    "weapons-projectiles": validate_weapons_projectiles,
    "weapons-research": validate_weapons_research,
    "weapons-conventional": validate_weapons_conventional,
    "weapons-archaic": validate_weapons_archaic,
    "weapons-futuristic": validate_weapons_futuristic,
    "weapons-turrets": validate_weapons_turrets,
    "weapons-npc": validate_weapons_npc,
    "weapons": validate_weapons,
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
