# myModeTest 手动木材资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在现有 `myModeTest` 玻璃链和混凝土链基础上，实现“原木 → 木板 / 柴火 / 木炭”的手动木材生产链。

**Architecture:** 使用原版 `WoodLog` 作为唯一输入，新增手动锯木机生产木板和柴火，新增简易木炭窑生产木炭。先扩展静态验证器，再按资源、建筑、配方逐步闭合接口，最后将木材套件纳入总体验证。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-16-myModeTest-wood-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`。
- `packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀。
- 所有自定义 label、description、jobString 使用中文；`defName` 使用英文。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 首版只实现手动木材链，不实现电动锯木、电动木炭、燃料系统、竹子或红木扩展。
- 锯木与劈柴配方必须使用 `GeneralLaborSpeed`，烧炭配方必须使用 `SmeltingSpeed`。
- 配方输入必须直接使用原版 `WoodLog`，不得使用 HSK 的 `Wooden` 分类。
- HSK 贴图来源必须固定为提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 `WoodLog`、`Woody`、`Stony`、`GeneralLaborSpeed`、`SmeltingSpeed`、`MakeWoodPlanks_Hand`、`Recipe_MakeWoodPlanks_Hand` 若在 1.7 改名，会导致资源、配方、研究或建筑失效；Task 4 的 `wood-recipes` 套件固定检查这些精确引用。
2. `Graphic_StackCount` 木板贴图目录和 `Graphic_Multi` 建筑材料遮罩路径若不一致，会导致游戏内丢图；Task 2 和 Task 3 的 PNG 检查固定全部 17 张图。
3. `MMT_CharcoalBurning` 依赖 `MMT_Woodworking`，研究链顺序错误会阻塞木炭窑；Task 3 的 `wood-building` 套件固定检查前置关系和坐标。
4. 木板必须具有 `stuffProps/categories/Woody` 才能成为建造材料；Task 2 的 `wood-resources` 套件固定检查该分类和材料因子。
5. HSK 的 `SmithingSpeed` 和 `Wooden` 分类在 1.7 不存在，误用会造成加载解析失败；Task 4 的验证器和 `git grep` 检查不得保留这两个引用。

---

### Task 1: 扩展木材链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有 `validate_mod.py` 的通用 XML、PNG、Def 和列表断言函数。
- Produces: `python tools/validate_mod.py {wood-resources|wood-building|wood-recipes|wood}`；Task 2-4 分别使用这些套件。

- [ ] **Step 1: 运行 wood-resources 验证，确认套件不存在**

Run:

```bash
python tools/validate_mod.py wood-resources
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'wood-resources'`。

- [ ] **Step 2: 添加木材验证常量**

在 `CONCRETE_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml"` 后插入：

```python
WOOD_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml"
WOOD_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml"
WOOD_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Wood.xml"
WOOD_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wood.xml"

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
```

将 `VANILLA_THINGS = {"ComponentIndustrial"}` 修改为：

```python
VANILLA_THINGS = {"ComponentIndustrial", "WoodLog"}
```

- [ ] **Step 3: 添加木材验证期望**

在 `CONCRETE_RECIPE_EXPECTATIONS` 定义结束后插入：

```python
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
```

- [ ] **Step 4: 添加木材验证函数**

在 `def validate_all_xml() -> None:` 前插入：

```python
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
```

- [ ] **Step 5: 注册木材验证套件**

将 `SUITES` 修改为：

```python
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
    "all": validate_all,
}
```

- [ ] **Step 6: 验证新套件按预期缺少产品文件**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] wood-resources: 缺少文件:`，并列出 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml`。

- [ ] **Step 7: 验证现有总套件仍然通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出仍以混凝土套件和 `all-xml: OK (10 files)` 结束。

- [ ] **Step 8: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加木材链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 木材资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml`
- Create: `1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_a.png`
- Create: `1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_b.png`
- Create: `1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_c.png`
- Create: `1.7/Textures/Things/Item/MMT/Kindling.png`
- Create: `1.7/Textures/Things/Item/MMT/Charcoal.png`

**Interfaces:**
- Consumes: Task 1 的 `wood-resources` 套件。
- Produces: `MMT_WoodPlank`、`MMT_Kindling`、`MMT_Charcoal`；Task 4 的配方引用这些 defName。

- [ ] **Step 1: 运行 wood-resources 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] wood-resources: 缺少文件:`。

- [ ] **Step 2: 创建资源 Def**

创建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_WoodPlank</defName>
    <label>木板</label>
    <description>由原木锯成的板材，可用于建造木质结构。</description>
    <graphicData>
      <texPath>Things/Item/MMT/WoodPlank</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Neolithic</techLevel>
    <statBases>
      <MarketValue>1.5</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>5.0</Flammability>
      <Mass>0.3</Mass>
    </statBases>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Woody</li>
      </categories>
      <stuffAdjective>木制</stuffAdjective>
      <appearance>Planks</appearance>
      <color>(133,97,67)</color>
      <constructEffect>ConstructWood</constructEffect>
      <statFactors>
        <MaxHitPoints>0.9</MaxHitPoints>
        <Flammability>1</Flammability>
        <Beauty>1.65</Beauty>
        <WorkToMake>0.7</WorkToMake>
        <WorkToBuild>0.7</WorkToBuild>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Light</terrainAffordanceNeeded>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Kindling</defName>
    <label>柴火</label>
    <description>由原木劈成的柴火，可作为后续燃料系统的中间资源。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Kindling</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Neolithic</techLevel>
    <statBases>
      <MarketValue>2.3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>5.0</Flammability>
      <Mass>0.15</Mass>
    </statBases>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Charcoal</defName>
    <label>木炭</label>
    <description>由原木烧制而成的木炭，可作为后续金属和燃料系统的中间资源。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Charcoal</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <color>(95,95,95)</color>
    </graphicData>
    <tickerType>Normal</tickerType>
    <techLevel>Medieval</techLevel>
    <statBases>
      <MarketValue>2.0</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>3.0</Flammability>
      <Mass>0.16</Mass>
    </statBases>
    <stackLimit>600</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: 下载已授权的资源贴图**

Run:

```bash
mkdir -p "1.7/Textures/Things/Item/MMT/WoodPlank"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/WoodPlank/WoodPlank_a.png" -o "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_a.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/WoodPlank/WoodPlank_b.png" -o "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_b.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/WoodPlank/WoodPlank_c.png" -o "1.7/Textures/Things/Item/MMT/WoodPlank/WoodPlank_c.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/Kindling.png" -o "1.7/Textures/Things/Item/MMT/Kindling.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/Charcoal.png" -o "1.7/Textures/Things/Item/MMT/Charcoal.png"
```

- [ ] **Step 4: 运行 wood-resources 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-resources
```

Expected: 退出码为 `0`，输出 `wood-resources: OK`。

- [ ] **Step 5: 提交资源定义和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Wood.xml 1.7/Textures/Things/Item/MMT/WoodPlank 1.7/Textures/Things/Item/MMT/Kindling.png 1.7/Textures/Things/Item/MMT/Charcoal.png
git commit -m "feat: 添加木材资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 木材研究与生产建筑

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Wood.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wood.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_HandSawmill/` 下 6 张 PNG
- Create: `1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/` 下 6 张 PNG

**Interfaces:**
- Consumes: Task 1 的 `wood-building` 套件。
- Produces: `MMT_Woodworking`、`MMT_CharcoalBurning`、`MMT_HandSawmill`、`MMT_CharcoalKiln`；Task 4 依赖这些名称。

- [ ] **Step 1: 运行 wood-building 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-building
```

Expected: 退出码为 `1`，输出包含 `[FAIL] wood-building: 缺少文件:`。

- [ ] **Step 2: 创建研究 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_Research_Wood.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ResearchProjectDef>
    <defName>MMT_Woodworking</defName>
    <label>木工加工</label>
    <description>学习如何将原木加工成木板和柴火。</description>
    <baseCost>300</baseCost>
    <techLevel>Neolithic</techLevel>
    <researchViewX>0.00</researchViewX>
    <researchViewY>1.00</researchViewY>
  </ResearchProjectDef>

  <ResearchProjectDef>
    <defName>MMT_CharcoalBurning</defName>
    <label>木炭烧制</label>
    <description>学习如何使用简易木炭窑将原木烧制成木炭。</description>
    <baseCost>500</baseCost>
    <techLevel>Medieval</techLevel>
    <prerequisites>
      <li>MMT_Woodworking</li>
    </prerequisites>
    <researchViewX>1.00</researchViewX>
    <researchViewY>1.00</researchViewY>
  </ResearchProjectDef>
</Defs>
```

- [ ] **Step 3: 创建建筑 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wood.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_HandSawmill</defName>
    <label>手动锯木机</label>
    <description>依靠人力切割原木的工作台，用于制作木板和柴火。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(3,3)</drawSize>
    </graphicData>
    <passability>PassThroughOnly</passability>
    <pathCost>80</pathCost>
    <fillPercent>0.6</fillPercent>
    <surfaceType>Item</surfaceType>
    <canOverlapZones>false</canOverlapZones>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-1)</interactionCellOffset>
    <useStuffTerrainAffordance>true</useStuffTerrainAffordance>
    <stuffCategories>
      <li>Woody</li>
    </stuffCategories>
    <costStuffCount>120</costStuffCount>
    <minifiedDef>MinifiedThing</minifiedDef>
    <statBases>
      <WorkToBuild>700</WorkToBuild>
      <MaxHitPoints>170</MaxHitPoints>
      <Flammability>1.0</Flammability>
      <Cleanliness>-7</Cleanliness>
      <Mass>500</Mass>
    </statBases>
    <size>(3,1)</size>
    <designationCategory>Production</designationCategory>
    <recipes>
      <li>MMT_MakeWoodPlanks_Hand</li>
      <li>MMT_MakeKindling_Hand</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <building>
      <uninstallWork>1250</uninstallWork>
    </building>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_Woodworking</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>3</constructionSkillPrerequisite>
  </ThingDef>

  <ThingDef ParentName="BuildingBase">
    <defName>MMT_CharcoalKiln</defName>
    <label>简易木炭窑</label>
    <description>用于将原木烧制成木炭的简易窑炉。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(3,3)</drawSize>
    </graphicData>
    <passability>PassThroughOnly</passability>
    <pathCost>90</pathCost>
    <fillPercent>0.9</fillPercent>
    <canOverlapZones>false</canOverlapZones>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-2)</interactionCellOffset>
    <useStuffTerrainAffordance>true</useStuffTerrainAffordance>
    <stuffCategories>
      <li>Stony</li>
    </stuffCategories>
    <costStuffCount>180</costStuffCount>
    <statBases>
      <WorkToBuild>3000</WorkToBuild>
      <MaxHitPoints>270</MaxHitPoints>
      <Flammability>0</Flammability>
      <Cleanliness>-8</Cleanliness>
      <Mass>500</Mass>
    </statBases>
    <size>(3,3)</size>
    <designationCategory>Production</designationCategory>
    <recipes>
      <li>MMT_MakeCharcoal_Hand</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <building>
      <uninstallWork>1850</uninstallWork>
    </building>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_CharcoalBurning</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>4</constructionSkillPrerequisite>
  </ThingDef>
</Defs>
```

- [ ] **Step 4: 下载已授权的建筑贴图**

Run:

```bash
mkdir -p "1.7/Textures/Things/Building/Production/MMT_HandSawmill" "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_east.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_east.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_eastm.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_eastm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_north.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_north.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_northm.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_northm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_south.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_south.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_southm.png" -o "1.7/Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_southm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_east.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_east.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_eastm.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_eastm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_north.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_north.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_northm.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_northm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_south.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_south.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_southm.png" -o "1.7/Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_southm.png"
```

- [ ] **Step 5: 运行 wood-building 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-building
```

Expected: 退出码为 `0`，输出 `wood-building: OK`。

- [ ] **Step 6: 提交研究和建筑**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Wood.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wood.xml 1.7/Textures/Things/Building/Production/MMT_HandSawmill 1.7/Textures/Things/Building/Production/MMT_CharcoalKiln
git commit -m "feat: 添加木材研究与建筑" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 木材配方与完整验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 的 `MMT_WoodPlank`、`MMT_Kindling`、`MMT_Charcoal`；Task 3 的 `MMT_Woodworking`、`MMT_CharcoalBurning`、`MMT_HandSawmill`、`MMT_CharcoalKiln`。
- Produces: `MMT_MakeWoodPlanks_Hand`、`MMT_MakeKindling_Hand`、`MMT_MakeCharcoal_Hand`，并将 `validate_wood()` 纳入 `validate_all()`。

- [ ] **Step 1: 运行 wood-recipes 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-recipes
```

Expected: 退出码为 `1`，输出包含 `[FAIL] wood-recipes: 缺少文件:`。

- [ ] **Step 2: 创建配方 Def**

创建 `1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <RecipeDef>
    <defName>MMT_MakeWoodPlanks_Hand</defName>
    <label>锯木板</label>
    <description>将原木锯成木板。产出 15。</description>
    <jobString>正在锯木板。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>GeneralLaborSpeed</workSpeedStat>
    <effectWorking>MakeWoodPlanks_Hand</effectWorking>
    <soundWorking>Recipe_MakeWoodPlanks_Hand</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>WoodLog</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>WoodLog</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_WoodPlank>15</MMT_WoodPlank>
    </products>
    <skillRequirements>
      <Crafting>3</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_Woodworking</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeKindling_Hand</defName>
    <label>劈柴火</label>
    <description>将原木劈成柴火。产出 20。</description>
    <jobString>正在劈柴火。</jobString>
    <workAmount>500</workAmount>
    <workSpeedStat>GeneralLaborSpeed</workSpeedStat>
    <effectWorking>MakeWoodPlanks_Hand</effectWorking>
    <soundWorking>Recipe_MakeWoodPlanks_Hand</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>WoodLog</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>WoodLog</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Kindling>20</MMT_Kindling>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.25</workSkillLearnFactor>
    <researchPrerequisite>MMT_Woodworking</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeCharcoal_Hand</defName>
    <label>烧制木炭</label>
    <description>将原木烧制成木炭。产出 20。</description>
    <jobString>正在烧制木炭。</jobString>
    <workAmount>1600</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>WoodLog</li>
          </thingDefs>
        </filter>
        <count>25</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>WoodLog</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Charcoal>20</MMT_Charcoal>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_CharcoalBurning</researchPrerequisite>
  </RecipeDef>
</Defs>
```

- [ ] **Step 3: 运行 wood-recipes 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py wood-recipes
```

Expected: 退出码为 `0`，输出 `wood-recipes: OK`。

- [ ] **Step 4: 将木材套件纳入总体验证**

将 `validate_all()` 修改为：

```python
def validate_all() -> None:
    validate_base()
    validate_resources()
    validate_building()
    validate_recipes()
    validate_concrete()
    validate_wood()
    validate_all_xml()
```

- [ ] **Step 5: 运行完整静态验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，依次输出：

```text
base: OK
resources: OK
building: OK
recipes: OK
concrete-resources: OK
concrete-building: OK
concrete-recipes: OK
concrete: OK
wood-resources: OK
wood-building: OK
wood-recipes: OK
wood: OK
all-xml: OK (14 files)
```

- [ ] **Step 6: 检查 1.7 不兼容引用没有残留**

Run:

```bash
git grep -n "SmithingSpeed\|Wooden" -- 1.7 tools docs/superpowers/plans/2026-10-16-myModeTest-wood-production.md
```

Expected: 只在木材实施计划的说明性检查代码或预期文本中出现；`1.7/` 和 `tools/validate_mod.py` 的产品/期望代码不得把 `SmithingSpeed` 或 `Wooden` 当作实际引用。若 `1.7/` 下有命中，必须修正。

- [ ] **Step 7: 执行 C# 编译验证**

Run:

```bash
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `dotnet build` 退出码为 `0`，输出 `0 个警告`、`0 个错误`；`test -f` 退出码为 `0`。

- [ ] **Step 8: 检查 Git 工作区并提交**

Run:

```bash
git status --short
```

Expected: 只显示以下两个文件：

```text
?? 1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml
 M tools/validate_mod.py
```

然后执行：

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Wood.xml tools/validate_mod.py
git commit -m "feat: 添加木材生产配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 9: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=6
```

Expected: `git status --short` 无输出；日志最新提交为：

```text
feat: 添加木材生产配方
feat: 添加木材研究与建筑
feat: 添加木材资源定义
test: 添加木材链静态验证
docs: 添加木材资源链设计
```
