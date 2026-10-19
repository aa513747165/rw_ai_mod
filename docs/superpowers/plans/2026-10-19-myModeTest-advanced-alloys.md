# myModeTest 高级合金链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增 3 种稀有矿石及自设计矿脉、3 种中间金属、5 种高级合金（+铁钛产原版 Plasteel）、9 个电弧炉配方、2 个研究；全部挂现有电弧炉，零新建筑。

**Architecture:** 新增 5 个 XML（矿石/合金/矿脉/配方/研究）+ 9 张矿石贴图；修改 `MMT_Buildings_Electric.xml`（电弧炉 recipes 追加 9 项）与 `tools/validate_mod.py`（新套件 `advanced-alloys` + 电弧炉配方断言扩展 + Plasteel 白名单）。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-advanced-alloys-design.md`

## Global Constraints

- Mod 显示名 `myModeTest`；`packageId` `YourName.myModeTest`。
- 自定义 Def 全部 `MMT_` 前缀；中文 label/description，英文 defName。
- 不引用 `SK.` 类型与 HSK 专属 Def/分类；stuff 分类仅可用 `Metallic`（禁 SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic）。
- 配方 workSpeedStat 用 `SmeltingSpeed`（禁 `SmithingSpeed`）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，文件复制。
- 所有文本文件 UTF-8；git 提交信息结尾带 `Co-Authored-By: Claude Code <noreply@anthropic.com>`。
- 最终必须执行 `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all` 和 `"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不启动 RimWorld，不声称游戏内加载通过；只提交本地 Git，不 push。

## Review Focus

1. **电弧炉 recipes 变更破坏 electric 套件**：`validate_electric` 对 recipes 做全等断言（现 7 项），追加 9 项后必须在 Task 1 同步改为 `ELECTRIC_FURNACE_RECIPES + ADVANCED_ALLOYS_FURNACE_RECIPES`，否则 Task 3 后 electric 套件失败。
2. **Plasteel 是原版 Def**：`MMT_MakeFerrotitanium` 的输出不在任何 MMT 资源闭包内，输出闭包必须含 `VANILLA_THINGS` 且其中必须新增 `Plasteel`（Task 1）。
3. **HSK 禁用的 stuff 分类混入**：复制参考数值时可能带入 SolidMetallic 等 HSK 分类，Task 1 套件对资源 XML 做禁用字符串扫描。
4. **矿脉数值不统一**：3 矿脉 HP（10000/13000/20000）、产量（50/75/50）、常见度（0.4/0.25/0.15）、矿块范围（5~10/3~9/3~9）各不相同，Task 1 逐项断言，不得套用现有矿脉的固定 75 产量断言。
5. **配方研究前置分两层**：精炼 3 配方前置 `MMT_AdvancedMetalRefining`、合炼 6 配方前置 `MMT_AdvancedAlloys`，Task 1 按配方逐项断言，不统一断言。

---

### Task 1: 新增 advanced-alloys 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数（`require_files`/`parse_xml`/`def_map`/`child_text`/`li_values`/`assert_equal`/`assert_png`/`read_text`/`fail`）与常量 `ELECTRIC_BUILDING_XML`/`ELECTRIC_FURNACE_RECIPES`/`METAL_RESOURCE_XML`/`PETROCHEM_RESOURCE_XML`/`VANILLA_THINGS`。
- Produces: 常量 `ADVANCED_ORE_RESOURCE_XML`/`ADVANCED_ALLOY_RESOURCE_XML`/`ADVANCED_ROCKS_BUILDING_XML`/`ADVANCED_ALLOYS_RECIPE_XML`/`ADVANCED_ALLOYS_RESEARCH_XML`/`ADVANCED_ORE_ITEM_TEXTURES`/`ADVANCED_ORE_EXPECTATIONS`/`ADVANCED_ALLOY_EXPECTATIONS`/`ADVANCED_MINEABLE_EXPECTATIONS`/`ADVANCED_ALLOYS_FURNACE_RECIPES`/`ADVANCED_ALLOYS_RECIPE_EXPECTATIONS`；函数 `validate_advanced_alloys()`；套件名 `advanced-alloys`。

- [ ] **Step 1: 新增常量（放在 `ELECTRIC_ELECTRONICS_RECIPE_EXPECTATIONS` 之后）**

```python
ADVANCED_ORE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedOre.xml"
ADVANCED_ALLOY_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml"
ADVANCED_ROCKS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AdvancedRocks.xml"
ADVANCED_ALLOYS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml"
ADVANCED_ALLOYS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_AdvancedAlloys.xml"

ADVANCED_ORE_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}_{suffix}.png"
    for ore in ("NickelOre", "IlmeniteOre", "WolframiteOre")
    for suffix in ("a", "b", "c")
]

ADVANCED_ORE_EXPECTATIONS = {
    "MMT_NickelOre": {"label": "镍矿石", "value": "4.0", "color": "(42,208,206)", "tex": "Things/Item/MMT/MMT_Ore/MMT_NickelOre"},
    "MMT_IlmeniteOre": {"label": "钛铁矿石", "value": "8.0", "color": "(160,178,195)", "tex": "Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre"},
    "MMT_WolframiteOre": {"label": "钨锰铁矿石", "value": "9.0", "color": "(230,230,255)", "tex": "Things/Item/MMT/MMT_Ore/MMT_WolframiteOre"},
}

ADVANCED_ALLOY_EXPECTATIONS = {
    "MMT_NickelBar": {"label": "镍锭", "value": "6.0", "stack": "150", "color": "(42,208,206)", "stuff": True, "adjective": "镍制", "hp": "2.8", "beauty": "2.4", "work_make": "2.0", "work_build": "2.0", "mass": "1.35"},
    "MMT_Cobalt": {"label": "钴锭", "value": "10.0", "stack": "150", "color": "(60,90,120)", "stuff": False},
    "MMT_Chromium": {"label": "铬锭", "value": "5.0", "stack": "150", "color": "(235,235,235)", "stuff": False},
    "MMT_Tungsten": {"label": "钨合金", "value": "40.0", "stack": "150", "color": "(240,230,230)", "stuff": False},
    "MMT_Titanium": {"label": "钛合金", "value": "45.0", "stack": "75", "color": "(160,178,181)", "stuff": True, "adjective": "钛制", "hp": "3.3", "beauty": "1.8", "work_make": "3.0", "work_build": "3.0", "mass": "0.8"},
    "MMT_StelliteAlloy": {"label": "司太立合金", "value": "30.0", "stack": "150", "color": "(125,146,178)", "stuff": True, "adjective": "司太立制", "hp": "3.55", "beauty": "1.5", "work_make": "2.0", "work_build": "2.2", "mass": "1.24"},
    "MMT_NitinolAlloy": {"label": "镍钛诺合金", "value": "27.0", "stack": "150", "color": "(175,175,175)", "stuff": True, "adjective": "镍钛诺制", "hp": "3.6", "beauty": "1.6", "work_make": "2.6", "work_build": "2.4", "mass": "0.95"},
    "MMT_PobediteAlloy": {"label": "胜利合金", "value": "33.0", "stack": "150", "color": "(85,85,85)", "stuff": True, "adjective": "胜利制", "hp": "4.0", "beauty": "1.1", "work_make": "2.8", "work_build": "2.8", "mass": "1.4"},
}

ADVANCED_MINEABLE_EXPECTATIONS = {
    "MMT_MineableNickelOre": {"label": "镍矿脉", "yield_thing": "MMT_NickelOre", "hp": "10000", "yield": "50", "commonality": "0.4", "lump": "5~10", "color": "(88,115,175)", "color_two": "(42,208,206)"},
    "MMT_MineableIlmenite": {"label": "钛铁矿脉", "yield_thing": "MMT_IlmeniteOre", "hp": "13000", "yield": "75", "commonality": "0.25", "lump": "3~9", "color": "(160,178,181)", "color_two": "(160,178,195)"},
    "MMT_MineableWolframite": {"label": "钨锰铁矿脉", "yield_thing": "MMT_WolframiteOre", "hp": "20000", "yield": "50", "commonality": "0.15", "lump": "3~9", "color": "(88,115,175)", "color_two": "(230,230,255)"},
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
```

- [ ] **Step 2: 修改既有常量与 electric 套件**

1. `VANILLA_THINGS` 改为：
```python
VANILLA_THINGS = {"ComponentIndustrial", "WoodLog", "Steel", "Silver", "Gold", "Plasteel"}
```
2. `validate_electric()` 中电弧炉配方断言改为：
```python
    assert_equal(li_values(furnace, "recipes"), ELECTRIC_FURNACE_RECIPES + ADVANCED_ALLOYS_FURNACE_RECIPES, "电弧炉配方")
```

- [ ] **Step 3: 实现 `validate_advanced_alloys()`（放在 `validate_electric_electronics()` 之后）**

```python
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
```

- [ ] **Step 4: 注册套件**

`validate_all()` 在 `validate_electric_electronics()` 之后追加 `validate_advanced_alloys()`；`SUITES` 在 `"electric-electronics"` 之后追加 `"advanced-alloys": validate_advanced_alloys`。

- [ ] **Step 5: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py advanced-alloys`
预期：失败，提示缺少 5 个 XML 文件。

- [ ] **Step 6: 提交**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加高级合金链静态验证"
```

---

### Task 2: 资源、矿脉、研究与贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedOre.xml`
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AdvancedRocks.xml`
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_AdvancedAlloys.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ore/` 新增 9 张

**Interfaces:**
- Produces: Task 3 配方引用的资源 defName（`MMT_NickelOre`/`MMT_IlmeniteOre`/`MMT_WolframiteOre`/`MMT_NickelBar`/`MMT_Cobalt`/`MMT_Chromium`/`MMT_Tungsten`/`MMT_Titanium`/`MMT_StelliteAlloy`/`MMT_NitinolAlloy`/`MMT_PobediteAlloy`）与研究 defName（`MMT_AdvancedMetalRefining`/`MMT_AdvancedAlloys`）。

- [ ] **Step 1: 复制 9 张矿石贴图**

```bash
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Nickel/Nickel_a.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_NickelOre_a.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Nickel/Nickel_b.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_NickelOre_b.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Nickel/Nickel_c.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_NickelOre_c.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Ilmenite/Ilmenite_a.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre_a.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Ilmenite/Ilmenite_b.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre_b.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Ilmenite/Ilmenite_c.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_IlmeniteOre_c.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Wolframite/Wolframite_a.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_WolframiteOre_a.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Wolframite/Wolframite_b.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_WolframiteOre_b.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Wolframite/Wolframite_c.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_WolframiteOre_c.png"
```

- [ ] **Step 2: 编写矿石 XML**

`MMT_Items_Resource_AdvancedOre.xml` 含 3 个 ThingDef，结构参照 `MMT_Items_Resource_Metal.xml` 的 `MMT_IronOre`（`ResourceBase` + `Graphic_StackCount` + `Metal_Drop` 音效 + `tickerType Rare` + DeteriorationRate 0 + Flammability 0 + `ResourcesRaw`）。差异点：

- defName/label/description/color/MarketValue/stackLimit 300 以 Task 1 `ADVANCED_ORE_EXPECTATIONS` 为准；
- Mass：镍矿 0.4、钛铁矿 0.15、钨锰铁矿 0.2；
- description 示例（镍矿石）：`含镍的天然矿石，可冶炼镍锭，并能从中伴生提取钴。`

- [ ] **Step 3: 编写合金资源 XML**

`MMT_Items_Resource_AdvancedAlloys.xml` 含 8 个 ThingDef。带 stuff 的 5 种（镍锭/钛/司太立/镍钛诺/胜利）结构参照 `MMT_Items_Resource_Metal.xml` 的 `MMT_Bronze`；不带 stuff 的 3 种（钴/铬/钨）结构参照同文件矿石段但用 Ingot 贴图。统一要点：

- texPath 一律 `Things/Item/MMT/MMT_Ingot/MMT_Ingot`，`Graphic_StackCount`，颜色以 Task 1 常量为准；
- 统一 statBases：DeteriorationRate 0、Flammability 0；Mass：钴/铬 0.45、钨 0.35、镍锭 0.45、钛 0.2、司太立 0.3、镍钛诺 0.3、胜利 0.5；
- 带 stuff 的 5 种：`useHitPoints true` + stuffProps（categories 仅 `Metallic`、constructEffect `ConstructMetal`、stuffAdjective/color/statFactors 以 Task 1 常量为准，statFactors 另含 Flammability 0）+ `terrainAffordanceNeeded Medium`；
- 钴/铬/钨：不写 stuffProps、不写 useHitPoints；
- label/价值/堆叠以 Task 1 `ADVANCED_ALLOY_EXPECTATIONS` 为准。

- [ ] **Step 4: 编写矿脉 XML**

`MMT_Buildings_AdvancedRocks.xml` 含 3 个 ThingDef，结构参照 `MMT_Buildings_Metal.xml` 的 `MMT_MineableCopperOre`（`RockBase` + `RockFlecked_Atlas` + color/colorTwo + building 节）。全部字段（label/HP/mineableThing/yield/commonality/lump/color/colorTwo）以 Task 1 `ADVANCED_MINEABLE_EXPECTATIONS` 为准；`mineableScatterLumpSizeRange` 写法与现有一致（如 `5~10`）。

- [ ] **Step 5: 编写研究 XML**

`MMT_Research_AdvancedAlloys.xml` 含 2 个 ResearchProjectDef，结构参照 `MMT_Research_ElectricElectronics.xml`。字段以 Task 1 Step 3 研究断言为准（精炼 1600/Industrial/前置 MMT_ElectricSmelting/坐标 12.00,2.00；合金 2300/Spacer/前置 MMT_AdvancedMetalRefining/坐标 13.00,3.00）。description 示例（高级合金）：`掌握钨、钛等高级合金的电弧熔炼工艺，可合炼司太立、镍钛诺、胜利合金与铁钛合金（玻璃钢）。`

- [ ] **Step 6: 提交**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedOre.xml 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AdvancedRocks.xml 1.7/Defs/ResearchProjectDefs/MMT_Research_AdvancedAlloys.xml 1.7/Textures/Things/Item/MMT/MMT_Ore/
git commit -m "feat: 添加高级合金资源、矿脉与研究"
```

---

### Task 3: 配方与电弧炉挂载（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml`（电弧炉 `<recipes>` 列表末尾追加 9 项）

**Interfaces:**
- Consumes: Task 2 的资源/研究 defName；既有 `MMT_IronOre`（金属链）、`MMT_Carbon`（石化链）、`Steel`（原版）、`Plasteel`（原版）。

- [ ] **Step 1: 编写配方 XML**

`MMT_Recipes_AdvancedAlloys.xml` 含 9 个 RecipeDef，结构参照 `MMT_Recipes_Electric.xml`（统一 `SmeltingSpeed`/`Smelt`/`Recipe_Smelt`、workSkill Crafting、learn 0.8、不写 recipeUsers、写 researchPrerequisite）。全部数值以 Task 1 `ADVANCED_ALLOYS_RECIPE_EXPECTATIONS` 为准。label 依次为：冶炼镍锭、提取钴、提取铬、冶炼钨合金、冶炼钛合金、合炼司太立合金、合炼镍钛诺合金、合炼胜利合金、合炼铁钛合金。description 需注明产出数量（如 `将镍矿石冶炼成镍锭。产出 15。`）。

- [ ] **Step 2: 修改电弧炉挂载**

`MMT_Buildings_Electric.xml` 中 `MMT_ElectricArcFurnace` 的 `<recipes>` 列表在 `MMT_ExtractSilicon_Electric` 之后按 `ADVANCED_ALLOYS_FURNACE_RECIPES` 顺序追加 9 个 `<li>`。

- [ ] **Step 3: 运行套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py advanced-alloys`
预期：`advanced-alloys: OK`。

- [ ] **Step 4: 提交**

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml
git commit -m "feat: 添加高级合金配方"
```

---

### Task 4: 全量验证与收尾

- [ ] **Step 1: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 45 增至 50）。

- [ ] **Step 2: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 警告 0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 3: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
