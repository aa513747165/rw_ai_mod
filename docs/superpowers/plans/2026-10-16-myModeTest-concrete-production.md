# myModeTest 手动混凝土资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在现有 `myModeTest` 玻璃链基础上，实现“砂 + 碎石 → 混凝土 → 钢筋混凝土”的手动混凝土生产链。

**Architecture:** 复用已有 `MMT_Sand`、`MMT_CrushedStone` 和原版切石台，新增一个手动混凝土搅拌机、一个研究、两个资源和两个配方。先扩展静态验证器，再按资源、建筑、配方逐步闭合接口，最后将混凝土套件纳入总体验证。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-16-myModeTest-concrete-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`。
- `packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀。
- 所有自定义 label、description、jobString 使用中文；`defName` 使用英文。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 首版只实现手动混凝土链，不实现电动搅拌机、电动钢筋混凝土、石化或电子内容。
- `MMT_MakeReinforcedConcrete_Hand` 的 `workSpeedStat` 必须为 `GeneralLaborSpeed`。
- HSK 贴图来源必须固定为提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 `TableStonecutter`、`Stonecutting`、`Metallic`、`Stony`、`GeneralLaborSpeed`、`SmeltingSpeed` 若在 1.7 改名，会导致建筑、配方、研究或材料失效；Task 4 的 `python tools/validate_mod.py concrete-recipes` 固定检查这些精确引用。
2. 双原料配方的 `ingredients` 与 `fixedIngredientFilter` 顺序或内容不一致，会导致配方无法正确识别原料；Task 4 的验证函数逐项比较两类输入。
3. 手动搅拌机使用 `Graphic_Multi` 和材料遮罩，缺少 `_eastm/_northm/_southm` 会导致贴图或材料着色异常；Task 3 的 PNG 检查固定全部 6 张建筑图。
4. 钢筋混凝土需要 `stuffProps/categories/Stony` 才能成为建造材料；Task 2 的 `concrete-resources` 套件固定检查该分类和材料因子。
5. HSK 电动阶段依赖橡胶、电子元件、Mechanism 和 `SK.` 组件，误入首版会扩大范围并产生悬空引用；Task 3 和 Task 4 的验证器扫描 `SK.`，且不加入这些成本或组件。

---

### Task 1: 扩展混凝土链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有 `validate_mod.py` 的 `parse_xml`、`def_map`、`child_text`、`li_values`、`assert_equal`、`assert_png`、`read_text`。
- Produces: `python tools/validate_mod.py {concrete-resources|concrete-building|concrete-recipes|concrete}`；Task 2-4 分别使用这些套件。

- [ ] **Step 1: 运行 concrete-resources 验证，确认套件不存在**

Run:

```bash
python tools/validate_mod.py concrete-resources
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'concrete-resources'`。

- [ ] **Step 2: 添加混凝土验证常量和期望**

在 `BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml"` 后插入：

```python
CONCRETE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml"
CONCRETE_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml"
CONCRETE_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml"
CONCRETE_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml"

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
```

在 `VANILLA_THINGS = {"ComponentIndustrial"}` 后插入：

```python
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
```

- [ ] **Step 3: 添加混凝土验证函数**

在 `def validate_all_xml() -> None:` 前插入：

```python
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
```

- [ ] **Step 4: 注册混凝土验证套件**

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
    "all": validate_all,
}
```

- [ ] **Step 5: 验证新套件按预期缺少产品文件**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] concrete-resources: 缺少文件:`，并列出 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml`。

- [ ] **Step 6: 验证现有总套件仍然通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出仍以玻璃链套件和 `all-xml: OK (6 files)` 结束。

- [ ] **Step 7: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加混凝土链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 混凝土资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml`
- Create: `1.7/Textures/Things/Item/MMT/Concrete.png`
- Create: `1.7/Textures/Things/Item/MMT/ReinforcedConcrete.png`

**Interfaces:**
- Consumes: Task 1 的 `concrete-resources` 套件。
- Produces: `MMT_Concrete`、`MMT_ReinforcedConcrete`；Task 4 的配方引用这些 defName。

- [ ] **Step 1: 运行 concrete-resources 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] concrete-resources: 缺少文件:`。

- [ ] **Step 2: 创建资源 Def**

创建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Concrete</defName>
    <label>混凝土</label>
    <description>由砂和碎石搅拌而成的复合材料，可加入金属材料制成钢筋混凝土。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Concrete</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Industrial</techLevel>
    <statBases>
      <MarketValue>6.0</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>1</Flammability>
      <Mass>0.5</Mass>
    </statBases>
    <stackLimit>150</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_ReinforcedConcrete</defName>
    <label>钢筋混凝土</label>
    <description>由混凝土和金属材料制成的高强度建筑材料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/ReinforcedConcrete</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Industrial</techLevel>
    <statBases>
      <MarketValue>16.0</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.6</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>75</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Stony</li>
      </categories>
      <stuffAdjective>钢筋混凝土</stuffAdjective>
      <color>(122,122,120)</color>
      <constructEffect>ConstructMetal</constructEffect>
      <statFactors>
        <MaxHitPoints>2.8</MaxHitPoints>
        <Flammability>0</Flammability>
        <WorkToMake>4.5</WorkToMake>
        <WorkToBuild>4.5</WorkToBuild>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Heavy</terrainAffordanceNeeded>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: 下载已授权的资源贴图**

Run:

```bash
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/Concrete.png" -o "1.7/Textures/Things/Item/MMT/Concrete.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/ReinforcedConcrete.png" -o "1.7/Textures/Things/Item/MMT/ReinforcedConcrete.png"
```

- [ ] **Step 4: 运行 concrete-resources 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-resources
```

Expected: 退出码为 `0`，输出 `concrete-resources: OK`。

- [ ] **Step 5: 提交资源定义和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Concrete.xml 1.7/Textures/Things/Item/MMT/Concrete.png 1.7/Textures/Things/Item/MMT/ReinforcedConcrete.png
git commit -m "feat: 添加混凝土资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 混凝土研究与手动搅拌机

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_east.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_eastm.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_north.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_northm.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_south.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_southm.png`

**Interfaces:**
- Consumes: Task 1 的 `concrete-building` 套件。
- Produces: `MMT_ConcreteBasic`、`MMT_ConcreteMixer`、`CONCRETE_BUILDING_RECIPES = [MMT_MixConcrete_Hand]`；Task 4 依赖这些名称。

- [ ] **Step 1: 运行 concrete-building 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-building
```

Expected: 退出码为 `1`，输出包含 `[FAIL] concrete-building: 缺少文件:`。

- [ ] **Step 2: 创建研究 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ResearchProjectDef>
    <defName>MMT_ConcreteBasic</defName>
    <label>混凝土技术</label>
    <description>学习如何将砂和碎石搅拌成混凝土，并进一步制作钢筋混凝土。</description>
    <baseCost>1000</baseCost>
    <techLevel>Industrial</techLevel>
    <prerequisites>
      <li>Stonecutting</li>
    </prerequisites>
    <researchViewX>7.00</researchViewX>
    <researchViewY>0.00</researchViewY>
  </ResearchProjectDef>
</Defs>
```

- [ ] **Step 3: 创建手动混凝土搅拌机 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_ConcreteMixer</defName>
    <label>手动混凝土搅拌机</label>
    <description>依靠人力搅拌砂和碎石的工作台，用于制作混凝土。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(2,1)</drawSize>
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
      <li>Metallic</li>
    </stuffCategories>
    <costStuffCount>135</costStuffCount>
    <costList>
      <ComponentIndustrial>5</ComponentIndustrial>
    </costList>
    <minifiedDef>MinifiedThing</minifiedDef>
    <statBases>
      <WorkToBuild>1600</WorkToBuild>
      <MaxHitPoints>180</MaxHitPoints>
      <Flammability>0.5</Flammability>
      <Beauty>-40</Beauty>
      <Cleanliness>-8</Cleanliness>
      <Mass>200</Mass>
    </statBases>
    <size>(2,1)</size>
    <designationCategory>Production</designationCategory>
    <recipes>
      <li>MMT_MixConcrete_Hand</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <building>
      <uninstallWork>950</uninstallWork>
    </building>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_ConcreteBasic</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>8</constructionSkillPrerequisite>
  </ThingDef>
</Defs>
```

- [ ] **Step 4: 下载已授权的搅拌机贴图**

Run:

```bash
mkdir -p "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_east.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_east.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_eastm.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_eastm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_north.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_north.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_northm.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_northm.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_south.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_south.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_southm.png" -o "1.7/Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_southm.png"
```

- [ ] **Step 5: 运行 concrete-building 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-building
```

Expected: 退出码为 `0`，输出 `concrete-building: OK`。

- [ ] **Step 6: 提交研究和建筑**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml 1.7/Textures/Things/Building/Production/MMT_ConcreteMixer
git commit -m "feat: 添加混凝土研究与搅拌机" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 混凝土配方与完整验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 的 `MMT_Concrete`、`MMT_ReinforcedConcrete`；Task 3 的 `MMT_ConcreteBasic`、`MMT_ConcreteMixer`。
- Produces: `MMT_MixConcrete_Hand`、`MMT_MakeReinforcedConcrete_Hand`，并将 `validate_concrete()` 纳入 `validate_all()`。

- [ ] **Step 1: 运行 concrete-recipes 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-recipes
```

Expected: 退出码为 `1`，输出包含 `[FAIL] concrete-recipes: 缺少文件:`。

- [ ] **Step 2: 创建配方 Def**

创建 `1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <RecipeDef>
    <defName>MMT_MixConcrete_Hand</defName>
    <label>搅拌混凝土</label>
    <description>将砂和碎石搅拌成混凝土。产出 15。</description>
    <jobString>正在搅拌混凝土。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>45</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_CrushedStone</li>
          </thingDefs>
        </filter>
        <count>30</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
        <li>MMT_CrushedStone</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Concrete>15</MMT_Concrete>
    </products>
    <skillRequirements>
      <Crafting>3</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ConcreteBasic</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeReinforcedConcrete_Hand</defName>
    <label>制作钢筋混凝土</label>
    <description>在混凝土中加入金属材料，制作钢筋混凝土。产出 8。</description>
    <jobString>正在制作钢筋混凝土。</jobString>
    <workAmount>1600</workAmount>
    <workSpeedStat>GeneralLaborSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableStonecutter</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>Metallic</li>
          </categories>
        </filter>
        <count>10</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Concrete</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>Metallic</li>
      </categories>
      <thingDefs>
        <li>MMT_Concrete</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_ReinforcedConcrete>8</MMT_ReinforcedConcrete>
    </products>
    <skillRequirements>
      <Crafting>4</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ConcreteBasic</researchPrerequisite>
  </RecipeDef>
</Defs>
```

- [ ] **Step 3: 运行 concrete-recipes 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py concrete-recipes
```

Expected: 退出码为 `0`，输出 `concrete-recipes: OK`。

- [ ] **Step 4: 将混凝土套件纳入总体验证**

将 `validate_all()` 修改为：

```python
def validate_all() -> None:
    validate_base()
    validate_resources()
    validate_building()
    validate_recipes()
    validate_concrete()
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
all-xml: OK (10 files)
```

- [ ] **Step 6: 执行 C# 编译验证**

Run:

```bash
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `dotnet build` 退出码为 `0`，输出 `0 个警告`、`0 个错误`；`test -f` 退出码为 `0`。

- [ ] **Step 7: 检查 Git 工作区并提交**

Run:

```bash
git status --short
```

Expected: 只显示以下两个文件：

```text
?? 1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml
 M tools/validate_mod.py
```

然后执行：

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml tools/validate_mod.py
git commit -m "feat: 添加混凝土生产配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 8: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=6
```

Expected: `git status --short` 无输出；日志最新提交为：

```text
feat: 添加混凝土生产配方
feat: 添加混凝土研究与搅拌机
feat: 添加混凝土资源定义
test: 添加混凝土链静态验证
docs: 添加混凝土资源链设计
```
