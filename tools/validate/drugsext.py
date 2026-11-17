#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""毒品扩展链（现代层+未来层）静态验证：资源/物品/hediff/配方/植株/建筑/研究/贴图引用闭合。"""
from __future__ import annotations

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    require_files,
)

RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_DrugsExt.xml"
ITEM_XML = "1.7/Defs/ThingDefs_Items/MMT_Items_DrugsExt.xml"
HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_DrugsExt.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_DrugsExt.xml"
PLANT_XML = "1.7/Defs/ThingDefs_Plants/MMT_Plants_Drugs.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_DrugsExt.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Medicine.xml"
KEYED_XML = "1.7/Languages/ChineseSimplified/Keyed/MMT_DrugsExt.xml"

TEX = "1.7/Textures"
# 目录型贴图（Graphic_StackCount）：texPath 子目录 + 期望文件名
STACK_TEXTURES = {
    "Things/Item/MMT/MMT_Medicine/MMT_SynchroCrystal": ["SynchroCrystal_a.png", "SynchroCrystal_b.png", "SynchroCrystal_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_Synchronisity": ["Synchronisity_a.png", "Synchronisity_b.png", "Synchronisity_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_PsyfocusTea": ["PsyfocusTea_a.png", "PsyfocusTea_b.png", "PsyfocusTea_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_AdvPsyfocusTea": ["AdvPsyfocusTea_a.png", "AdvPsyfocusTea_b.png", "AdvPsyfocusTea_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_SuperPsyfocusTea": ["SuperPsyfocusTea_a.png", "SuperPsyfocusTea_b.png", "SuperPsyfocusTea_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_DriedLeavesTobacco": ["DriedLeavesTobacco_a.png", "DriedLeavesTobacco_b.png", "DriedLeavesTobacco_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_Cigarette": ["Cigarette_a.png", "Cigarette_b.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_Cigar": ["Cigar_a.png", "Cigar_b.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_Coffee": ["Coffee_a.png", "Coffee_b.png", "Coffee_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_BattleCocktail": ["BattleCocktail_a.png", "BattleCocktail_b.png", "BattleCocktail_c.png"],
    "Things/Item/MMT/MMT_Medicine/MMT_GeneBooster": ["GeneBooster_a.png", "GeneBooster_b.png", "GeneBooster_c.png"],
}
# 单图贴图（Graphic_Single，texPath 直指文件名）
SINGLE_TEXTURES = [
    "Things/Item/MMT/MMT_Medicine/MMT_TobaccoLeaves.png",
    "Things/Item/MMT/MMT_Medicine/MMT_CoffeeBeans.png",
    "Things/Item/MMT/MMT_Medicine/MMT_RoastedCoffeeBeans.png",
    "Things/Item/MMT/MMT_Medicine/MMT_NanoPurgeSerum.png",
]
PLANT_TEXTURE_DIRS = [
    "Things/Plant/MMT/MMT_PlantTobacco",
    "Things/Plant/MMT/MMT_PlantTobacco_Immature",
    "Things/Plant/MMT/MMT_PlantCoffee",
    "Things/Plant/MMT/MMT_PlantCoffee_Immature",
]
BUILDING_TEXTURES = [
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_east.png",
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_eastm.png",
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_north.png",
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_northm.png",
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_south.png",
    "Things/Building/Production/MMT_AdvDrugLab/MMT_AdvDrugLab_southm.png",
]

RECIPE_EXPECT = {
    # defName: (产物 {thingDef: count}, 原料 {thingDef: count}, 研究前置)
    "MMT_MakeSynchroCrystal": ({"MMT_SynchroCrystal": "2"}, {"MMT_Sulphates": "2", "MMT_Surfactant": "1", "PsychoidLeaves": "4"}, "MMT_Drugs_D1"),
    "MMT_MakeSynchronisity": ({"MMT_Synchronisity": "2"}, {"MMT_SynchroCrystal": "4", "Neutroamine": "2"}, "MMT_Drugs_D1"),
    "MMT_BrewPsyfocusTea": ({"MMT_PsyfocusTea": "2"}, {"MMT_SynchroCrystal": "1", "MMT_Hypericum": "2"}, "MMT_Drugs_D1"),
    "MMT_BrewAdvPsyfocusTea": ({"MMT_AdvPsyfocusTea": "2"}, {"MMT_PsyfocusTea": "2", "MMT_SynchroCrystal": "1", "MMT_Hypericum": "2"}, "MMT_Drugs_D2"),
    "MMT_BrewSuperPsyfocusTea": ({"MMT_SuperPsyfocusTea": "2"}, {"MMT_AdvPsyfocusTea": "2", "MMT_SynchroCrystal": "2", "MMT_Hypericum": "4"}, "MMT_Drugs_E2"),
    "MMT_DryTobacco": ({"MMT_DriedLeavesTobacco": "5"}, {"MMT_TobaccoLeaves": "5"}, "MMT_Drugs_A1"),
    "MMT_DryTobaccoBulk": ({"MMT_DriedLeavesTobacco": "25"}, {"MMT_TobaccoLeaves": "25"}, "MMT_Drugs_A1"),
    "MMT_MakeCigarette": ({"MMT_Cigarette": "2"}, {"MMT_DriedLeavesTobacco": "2"}, "MMT_DrugPlants_A3"),
    "MMT_MakeCigar": ({"MMT_Cigar": "1"}, {"MMT_DriedLeavesTobacco": "4"}, "MMT_DrugPlants_A3"),
    "MMT_RoastCoffeeBeans": ({"MMT_RoastedCoffeeBeans": "5"}, {"MMT_CoffeeBeans": "5"}, "MMT_DrugPlants_A3"),
    "MMT_BrewCoffee": ({"MMT_Coffee": "2"}, {"MMT_RoastedCoffeeBeans": "2"}, "MMT_DrugPlants_A3"),
    "MMT_MakeBattleCocktail": ({"MMT_BattleCocktail": "1"}, {"MMT_BerserkHerb": "2", "Neutroamine": "1", "Chemfuel": "2"}, "MMT_Drugs_D1"),
    "MMT_MakeNanoRepairSerum": ({"MMT_NanoRepairSerum": "1"}, {"MedicineUltratech": "1", "MMT_Microchip": "1", "Neutroamine": "2", "MMT_BiosyntheticMaterial": "1"}, "MMT_Drugs_E1"),
    "MMT_MakeGeneBooster": ({"MMT_GeneBooster": "1"}, {"MMT_BiosyntheticMaterial": "2", "Neutroamine": "2", "MMT_UltraComponent": "1"}, "MMT_Drugs_E2"),
    "MMT_MakeNanoPurgeSerum": ({"MMT_NanoPurgeSerum": "1"}, {"MedicineUltratech": "1", "MMT_Microchip": "1", "MMT_UltraComponent": "1", "Neutroamine": "3"}, "MMT_Drugs_E2"),
    "MMT_MakePsychicAmplifier": ({"PsychicAmplifier": "1"}, {"MMT_SynchroCrystal": "4", "MMT_Microchip": "1", "MMT_UltraComponent": "1", "Neutroamine": "2"}, "MMT_Drugs_E2"),
    "MMT_MakeMechSerumResurrector": ({"MechSerumResurrector": "1"}, {"MedicineUltratech": "2", "MMT_Microchip": "2", "MMT_UltraComponent": "2", "Neutroamine": "4"}, "MMT_Drugs_E2"),
}

RESEARCH_EXPECT = {
    # defName: (label, techLevel, 前置列表, X, Y)
    "MMT_DrugPlants_A3": ("毒株种植 III", "Neolithic", ["MMT_DrugPlants_A2"], "2.00", "1.50"),
    "MMT_Drugs_D1": ("毒品 VII", "Spacer", ["MMT_Drugs_C3", "PsychiteRefining"], "12.00", "1.00"),
    "MMT_Drugs_D2": ("毒品 VIII", "Spacer", ["MMT_Drugs_D1", "MMT_AdvancedComponents"], "13.00", "1.00"),
    "MMT_Drugs_E1": ("毒品 IX", "Spacer", ["MMT_Drugs_D2", "MMT_Medicine_D1"], "14.00", "1.50"),
    "MMT_Drugs_E2": ("毒品 X", "Ultra", ["MMT_Drugs_E1", "MMT_UltraComponents"], "15.00", "1.50"),
}


def _check_textures() -> None:
    files = []
    for tex_dir, names in STACK_TEXTURES.items():
        files.extend(f"{TEX}/{tex_dir}/{n}" for n in names)
    files.extend(f"{TEX}/{p}" for p in SINGLE_TEXTURES)
    files.extend(f"{TEX}/{p}" for p in BUILDING_TEXTURES)
    for plant_dir in PLANT_TEXTURE_DIRS:
        stem = "PlantTobacco" if "Tobacco" in plant_dir else "PlantCoffee"
        files.extend(f"{TEX}/{plant_dir}/{stem}{variant}_Harvest.png" for variant in "ABCD")
    require_files(files)


def validate_drugsext() -> None:
    require_files([RESOURCE_XML, ITEM_XML, HEDIFF_XML, RECIPE_XML, PLANT_XML, BUILDING_XML, RESEARCH_XML, KEYED_XML])
    _check_textures()

    # ---- 资源：5 种原料/中间体 ----
    resources = def_map(parse_xml(RESOURCE_XML))
    for name, label in [("MMT_SynchroCrystal", "灵能结晶"), ("MMT_TobaccoLeaves", "烟叶"),
                        ("MMT_DriedLeavesTobacco", "干烟草叶"), ("MMT_CoffeeBeans", "咖啡豆"),
                        ("MMT_RoastedCoffeeBeans", "烘焙咖啡豆")]:
        if name not in resources:
            fail(f"资源缺少 {name}")
        assert_equal(child_text(resources[name], "label"), label, f"{name} label")
    assert_equal(child_text(resources["MMT_SynchroCrystal"], "thingCategories/li"), "Medicine", "灵能结晶类别")

    # ---- 物品：12 种成品 ----
    items = def_map(parse_xml(ITEM_XML))
    for name, label, category in [
        ("MMT_Synchronisity", "灵能粉", "Hard"), ("MMT_PsyfocusTea", "灵能茶", "Medical"),
        ("MMT_AdvPsyfocusTea", "高级灵能茶", "Medical"), ("MMT_SuperPsyfocusTea", "超级灵能茶", "Medical"),
        ("MMT_Cigarette", "卷烟", "Social"), ("MMT_Cigar", "雪茄", "Social"),
        ("MMT_Coffee", "咖啡", "Social"), ("MMT_BattleCocktail", "战斗鸡尾酒", "Hard"),
        ("MMT_NanoRepairSerum", "纳米修复针", "Medical"), ("MMT_GeneBooster", "基因强化剂", "Medical"),
        ("MMT_NanoPurgeSerum", "纳米净化血清", "Medical"),
    ]:
        if name not in items:
            fail(f"物品缺少 {name}")
        item = items[name]
        assert_equal(child_text(item, "label"), label, f"{name} label")
        assert_equal(child_text(item, "ingestible/drugCategory"), category, f"{name} 药物类别")
    # 灵能粉：自成瘾体系 + 恢复灵能焦点（原版 OffsetPsyfocus）
    synchro = items["MMT_Synchronisity"]
    drug_comp = next(li for li in synchro.findall("comps/li") if li.attrib.get("Class") == "CompProperties_Drug")
    assert_equal(child_text(drug_comp, "chemical"), "MMT_Synchronisity", "灵能粉 chemical")
    assert_equal(child_text(drug_comp, "isCombatEnhancingDrug"), "true", "灵能粉战斗药标记")
    doer_classes = [li.attrib.get("Class") for li in synchro.findall("ingestible/outcomeDoers/li")]
    if "IngestionOutcomeDoer_OffsetPsyfocus" not in doer_classes:
        fail("灵能粉缺少 OffsetPsyfocus 恢复灵能效果")
    # 净化血清：挂 C# outcome doer
    purge = items["MMT_NanoPurgeSerum"]
    purge_doers = [li.attrib.get("Class") for li in purge.findall("ingestible/outcomeDoers/li")]
    if "MyModeTest.IngestionOutcomeDoer_RemoveAddictions" not in purge_doers:
        fail("纳米净化血清缺少 C# 成瘾移除 doer")
    # 高级/超级灵能茶：恢复灵能焦点与睡眠的档位核对（各 50% / 各 100%）
    for name, psy, rest in [("MMT_AdvPsyfocusTea", "0.5", "0.5"), ("MMT_SuperPsyfocusTea", "1.0", "1.0")]:
        doers = items[name].findall("ingestible/outcomeDoers/li")
        psy_doer = next(li for li in doers if li.attrib.get("Class") == "IngestionOutcomeDoer_OffsetPsyfocus")
        assert_equal(child_text(psy_doer, "offset"), psy, f"{name} 灵能焦点恢复")
        rest_doer = next(li for li in doers if li.attrib.get("Class") == "IngestionOutcomeDoer_OffsetNeed")
        assert_equal(child_text(rest_doer, "need"), "Rest", f"{name} 恢复目标 need")
        assert_equal(child_text(rest_doer, "offset"), rest, f"{name} 睡眠恢复")

    # ---- hediff：灵能粉成瘾五件套 + 双阶段战斗鸡尾酒 + 纳米修复 HealPermanentWounds ----
    # 注：hediff 与同名 ThoughtDef 共存是原版惯例，def_map 会报重复，需按标签类型分别建图
    hediff_root = parse_xml(HEDIFF_XML)
    hediffs = {d.findtext("defName"): d for d in hediff_root if d.tag == "HediffDef"}
    chemicals = {d.findtext("defName"): d for d in hediff_root if d.tag == "ChemicalDef"}
    needs = {d.findtext("defName"): d for d in hediff_root if d.tag == "NeedDef"}
    thoughts = {d.findtext("defName"): d for d in hediff_root if d.tag == "ThoughtDef"}
    for name in ["MMT_SynchronisityHigh", "MMT_SynchronisityTolerance", "MMT_SynchronisityAddiction",
                 "MMT_CigaretteHigh", "MMT_CigarHigh", "MMT_CoffeeHigh",
                 "MMT_BattleCocktailHigh", "MMT_NanoRepairHigh", "MMT_GeneBoosterHigh"]:
        if name not in hediffs:
            fail(f"Hediff 缺少 {name}")
    high = hediffs["MMT_SynchronisityHigh"]
    stage = high.find("stages/li")
    assert_equal(child_text(stage, "restFallFactor"), "0.33", "灵能粉休息效率系数")
    assert_equal(child_text(stage.find("statOffsets"), "PsychicSensitivity"), "0.2", "灵能粉灵能敏感加成")
    addiction = hediffs["MMT_SynchronisityAddiction"]
    assert_equal(child_text(addiction, "chemicalNeed"), "Chemical_MMT_Synchronisity", "灵能粉成瘾化学需求")
    assert_equal(child_text(addiction, "hediffClass"), "Hediff_Addiction", "灵能粉成瘾 hediff 类")
    cocktail_stages = hediffs["MMT_BattleCocktailHigh"].findall("stages/li")
    assert_equal(len(cocktail_stages), 2, "战斗鸡尾酒应为双阶段（虚脱+强效）")
    assert_equal(child_text(cocktail_stages[1], "minSeverity"), "0.4", "战斗鸡尾酒强效期阈值")
    nano_comps = [li.attrib.get("Class") for li in hediffs["MMT_NanoRepairHigh"].findall("comps/li")]
    if "HediffCompProperties_HealPermanentWounds" not in nano_comps:
        fail("纳米修复 hediff 缺少 HealPermanentWounds 旧伤修复组件")
    # ChemicalDef + NeedDef + ThoughtDef
    if "MMT_Synchronisity" not in chemicals:
        fail("缺少 ChemicalDef MMT_Synchronisity")
    chem = chemicals["MMT_Synchronisity"]
    assert_equal(child_text(chem, "addictionHediff"), "MMT_SynchronisityAddiction", "灵能 chemical 成瘾指向")
    assert_equal(child_text(chem, "toleranceHediff"), "MMT_SynchronisityTolerance", "灵能 chemical 耐受指向")
    if "Chemical_MMT_Synchronisity" not in needs:
        fail("缺少 NeedDef Chemical_MMT_Synchronisity")
    if "MMT_SynchronisityHigh" not in thoughts:
        fail("缺少 ThoughtDef MMT_SynchronisityHigh（灵能粉心情）")

    # ---- 配方：产物/原料/研究前置全量核对 ----
    recipes = def_map(parse_xml(RECIPE_XML))
    for def_name, (products, inputs, research) in RECIPE_EXPECT.items():
        if def_name not in recipes:
            fail(f"配方缺少 {def_name}")
        recipe = recipes[def_name]
        assert_equal(child_text(recipe, "researchPrerequisite"), research, f"{def_name} 研究前置")
        actual_products = {li.tag: li.text for li in recipe.find("products")}
        assert_equal(actual_products, products, f"{def_name} 产物")
        actual_inputs = {td.text: ing.findtext("count") for ing in recipe.findall("ingredients/li")
                         for td in ing.findall("filter/thingDefs/li")}
        assert_equal(actual_inputs, inputs, f"{def_name} 原料")
    # 未来层配方仅限高级药物合成台
    for def_name in ["MMT_MakeNanoRepairSerum", "MMT_MakeGeneBooster", "MMT_MakeNanoPurgeSerum",
                     "MMT_MakePsychicAmplifier", "MMT_MakeMechSerumResurrector"]:
        assert_equal(li_values(recipes[def_name], "recipeUsers"), ["MMT_AdvDrugLab"], f"{def_name} 工作台")

    # ---- 植株：播种研究挂毒株种植 III，收获物指向本链资源 ----
    plants = def_map(parse_xml(PLANT_XML))
    for name, harvest in [("MMT_PlantTobacco", "MMT_TobaccoLeaves"), ("MMT_PlantCoffee", "MMT_CoffeeBeans")]:
        if name not in plants:
            fail(f"植株缺少 {name}")
        plant = plants[name]
        assert_equal(li_values(plant.find("plant"), "sowResearchPrerequisites"), ["MMT_DrugPlants_A3"], f"{name} 播种研究")
        assert_equal(child_text(plant, "plant/harvestedThingDef"), harvest, f"{name} 收获物")

    # ---- 建筑：高级药物合成台（Spacer、1500W、研究 VIII） ----
    buildings = def_map(parse_xml(BUILDING_XML))
    if "MMT_AdvDrugLab" not in buildings:
        fail("建筑缺少 MMT_AdvDrugLab")
    lab = buildings["MMT_AdvDrugLab"]
    assert_equal(child_text(lab, "label"), "高级药物合成台", "合成台 label")
    assert_equal(li_values(lab, "researchPrerequisites"), ["MMT_Drugs_D2"], "合成台研究前置")
    power = next(li for li in lab.findall("comps/li") if li.attrib.get("Class") == "CompProperties_Power")
    assert_equal(child_text(power, "basePowerConsumption"), "1500", "合成台耗电")
    lab_recipes = li_values(lab, "recipes")
    for def_name in ["MMT_MakeSynchronisity", "MMT_MakeNanoRepairSerum", "MMT_MakeMechSerumResurrector"]:
        if def_name not in lab_recipes:
            fail(f"合成台配方列表缺少 {def_name}")

    # ---- 研究：5 个新节点字段核对 ----
    research_defs = def_map(parse_xml(RESEARCH_XML))
    for def_name, (label, tech, prereqs, x, y) in RESEARCH_EXPECT.items():
        if def_name not in research_defs:
            fail(f"研究缺少 {def_name}")
        node = research_defs[def_name]
        assert_equal(child_text(node, "label"), label, f"{def_name} label")
        assert_equal(child_text(node, "techLevel"), tech, f"{def_name} 科技等级")
        assert_equal(li_values(node, "prerequisites"), prereqs, f"{def_name} 前置")
        assert_equal(child_text(node, "researchViewX"), x, f"{def_name} 坐标X")
        assert_equal(child_text(node, "researchViewY"), y, f"{def_name} 坐标Y")

    print("drugsext: OK")


SUITES = {"drugsext": validate_drugsext}
