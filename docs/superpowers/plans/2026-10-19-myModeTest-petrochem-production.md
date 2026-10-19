# myModeTest 石化材料资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在现有 `myModeTest` 四条链基础上，实现“油苗 → 原油 → 聚合物 → 塑料/橡胶/合成纤维/碳材料”的手动石化生产链。

**Architecture:** 新增 1 个油苗矿脉（继承原版 `RockBase`，原版图集贴图）、6 个资源、1 个手动化学实验台（普通 `Building_WorkTable`，无电力无 SK 组件）、1 个研究和 5 个手动配方。碳材料复用木材链 `MMT_Charcoal`。先扩展静态验证器，再按资源、研究/建筑、配方逐步闭合接口，最后将 petrochem 套件纳入总体验证。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-petrochem-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description/jobString，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 首版只实现手动石化链，不实现耗电实验室、深层采油、硫磺支线或下游消费配方。
- 石化配方不得使用 RimWorld 1.7 不存在的 `SmithingSpeed`。
- 所有石化资源不定义 `stuffProps`。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK` 的 HEAD `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`，用文件复制而非网络下载。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 `RockBase`、`BiofuelRefining`、`ResourcesRaw`、`SmeltingSpeed`、`GeneralLaborSpeed`、`Smith`、`Recipe_Smith` 若在 1.7 改名，会导致矿脉、研究或配方失效；Task 4 的 `petrochem-recipes` 套件固定检查精确引用。
2. `MMT_MakeCarbon_Hand` 跨链引用木材链的 `MMT_Charcoal`，引用闭包必须覆盖木材资源 XML；Task 4 固定检查。
3. HSK 石化配方使用 1.7 已删除的 `SmithingSpeed`；Task 4 对配方 XML 全文禁用该字符串。
4. 油苗矿脉的 `mineableThing` 必须与 `MMT_CrudeOil` 配对且带 `veinMineable`；Task 3 固定检查。
5. HSK 实验室依赖 `SK.CompProperties_IdlePower` 等组件，误入首版会扩大范围；Task 3 验证化学台无电力组件并扫描 `SK.`。

---

### Task 1: 扩展石化链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有 `validate_mod.py` 的公共函数。
- Produces: `python tools/validate_mod.py {petrochem-resources|petrochem-building|petrochem-recipes|petrochem}`。

- [ ] **Step 1: 运行 petrochem-resources 验证，确认套件不存在**

Run:

```bash
python tools/validate_mod.py petrochem-resources
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'petrochem-resources'`。

- [ ] **Step 2: 添加石化验证常量和期望**

在 `METAL_BUILDING_XML = ...` 行后插入：

```python
PETROCHEM_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml"
PETROCHEM_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml"
PETROCHEM_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Petrochem.xml"
PETROCHEM_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Petrochem.xml"
```

在 `METAL_BUILDING_TEXTURES` 后插入：

```python
PETROCHEM_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Petro/{name}.png"
    for name in ("CrudeOil", "Polymers", "Plastic", "Rubber", "SyntheticFibers", "Carbon")
]

PETROCHEM_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_{side}.png"
    for side in ("east", "north", "south")
]
```

在常量区末尾（`METAL_RECIPE_EXPECTATIONS` 之后）插入：

```python
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
```

同时扩展原版研究白名单：

```python
VANILLA_RESEARCH = {"Stonecutting", "Smithing", "BiofuelRefining"}
```

- [ ] **Step 3: 添加石化验证函数**

在 `def validate_all_xml() -> None:` 前插入四个函数：

```python
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
```

- [ ] **Step 4: 注册石化验证套件**

在 `SUITES` 中 `"all"` 之前插入：

```python
    "petrochem-resources": validate_petrochem_resources,
    "petrochem-building": validate_petrochem_building,
    "petrochem-recipes": validate_petrochem_recipes,
    "petrochem": validate_petrochem,
```

- [ ] **Step 5: 验证新套件按预期缺少产品文件**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] petrochem-resources: 缺少文件:`，并列出 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml`。

- [ ] **Step 6: 验证现有总套件仍然通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出以 `all-xml: OK (18 files)` 结束。

- [ ] **Step 7: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加石化链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 石化资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Petro/`（6 张资源贴图）

**Interfaces:**
- Consumes: Task 1 的 `petrochem-resources` 套件。
- Produces: `MMT_CrudeOil`、`MMT_Polymers`、`MMT_Plastic`、`MMT_Rubber`、`MMT_SyntheticFibers`、`MMT_Carbon`；Task 4 的配方引用这些 defName。

- [ ] **Step 1: 运行 petrochem-resources 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] petrochem-resources: 缺少文件:`。

- [ ] **Step 2: 创建资源 Def**

创建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml`，包含 6 个资源（`ResourceBase`、`ResourcesRaw`、`Graphic_Single`、无 stuffProps），数值以设计文档第 5 节为准。

- [ ] **Step 3: 从本地 HSK 克隆复制已授权贴图**

Run（在仓库根目录执行）：

```bash
HSK="D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource"
mkdir -p "1.7/Textures/Things/Item/MMT/MMT_Petro"
cp "$HSK/CrudeOil.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/CrudeOil.png"
cp "$HSK/Polymers.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/Polymers.png"
cp "$HSK/Plastic.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/Plastic.png"
cp "$HSK/Rubber.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/Rubber.png"
cp "$HSK/Synteticfibres.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/SyntheticFibers.png"
cp "$HSK/Carbon.png" "1.7/Textures/Things/Item/MMT/MMT_Petro/Carbon.png"
```

- [ ] **Step 4: 运行 petrochem-resources 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-resources
```

Expected: 退出码为 `0`，输出 `petrochem-resources: OK`。

- [ ] **Step 5: 提交资源定义和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Petrochem.xml 1.7/Textures/Things/Item/MMT/MMT_Petro
git commit -m "feat: 添加石化资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 石化研究、手动化学实验台与油苗矿脉

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Petrochem.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Petrochem.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_HandChemTable/`（3 张建筑贴图）

**Interfaces:**
- Consumes: Task 1 的 `petrochem-building` 套件。
- Produces: `MMT_BasicPetrochemistry`、`MMT_HandChemTable`（挂载 5 个配方）、`MMT_MineableOilSeep`。

- [ ] **Step 1: 运行 petrochem-building 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-building
```

Expected: 退出码为 `1`，输出包含 `[FAIL] petrochem-building: 缺少文件:`。

- [ ] **Step 2: 创建研究 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_Research_Petrochem.xml`：`MMT_BasicPetrochemistry`，baseCost 1200，Industrial，前置 `BiofuelRefining`，坐标 `(13.00, 0.00)`。

- [ ] **Step 3: 创建化学台与油苗 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Petrochem.xml`：

- `MMT_HandChemTable`：`BuildingBase` 父类、`Building_WorkTable`、3×2、`Metallic` ×130 + 零部件 ×5、WorkToBuild 2000、HP 200、Flammability 1.0、Cleanliness -3、无电力组件、建造技能 6、`interactionCellOffset (0,0,-1)`、recipes 挂载 5 个配方，结构与既有手动建筑保持一致。
- `MMT_MineableOilSeep`：`RockBase` 父类、原版图集贴图、color `(45,42,40)`、colorTwo `(65,34,34)`、HP 1500、mineableThing `MMT_CrudeOil`、yield 75、常见度 0.8、矿块 10~18、`isResourceRock`、`veinMineable`。

- [ ] **Step 4: 从本地 HSK 克隆复制已授权建筑贴图**

```bash
HSK="D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/OilRefinery"
mkdir -p "1.7/Textures/Things/Building/Production/MMT_HandChemTable"
for side in east north south; do
  cp "$HSK/OilRefinery_$side.png" "1.7/Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_$side.png"
done
```

- [ ] **Step 5: 运行 petrochem-building 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-building
```

Expected: 退出码为 `0`，输出 `petrochem-building: OK`。

- [ ] **Step 6: 提交研究和建筑**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Petrochem.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Petrochem.xml 1.7/Textures/Things/Building/Production/MMT_HandChemTable
git commit -m "feat: 添加石化研究与化学实验台" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 石化配方与完整验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 的 6 个石化资源、木材链的 `MMT_Charcoal`、Task 3 的 `MMT_BasicPetrochemistry` 和 `MMT_HandChemTable`。
- Produces: 5 个手动石化配方，并将 `validate_petrochem()` 纳入 `validate_all()`。

- [ ] **Step 1: 运行 petrochem-recipes 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 退出码为 `1`，输出包含 `[FAIL] petrochem-recipes: 缺少文件:`。

- [ ] **Step 2: 创建配方 Def**

创建 `1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml`，包含 5 个配方，数值以设计文档第 6 节为准，研究前置 `MMT_BasicPetrochemistry`，不写 `recipeUsers`。

- [ ] **Step 3: 运行 petrochem-recipes 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 退出码为 `0`，输出 `petrochem-recipes: OK`。

- [ ] **Step 4: 将石化套件纳入总体验证**

将 `validate_all()` 修改为：

```python
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
```

- [ ] **Step 5: 运行完整静态验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出在金属套件之后追加：

```text
petrochem-resources: OK
petrochem-building: OK
petrochem-recipes: OK
petrochem: OK
all-xml: OK (22 files)
```

- [ ] **Step 6: 执行 C# 编译验证**

Run:

```bash
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `dotnet build` 退出码为 `0`，输出 0 警告 0 错误；`test -f` 退出码为 `0`。

- [ ] **Step 7: 检查 Git 工作区并提交**

Run:

```bash
git status --short
```

Expected: 只显示以下两个文件：

```text
?? 1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml
 M tools/validate_mod.py
```

然后执行：

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml tools/validate_mod.py
git commit -m "feat: 添加石化生产配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 8: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=7
```

Expected: `git status --short` 无输出；日志最新提交为：

```text
feat: 添加石化生产配方
feat: 添加石化研究与化学实验台
feat: 添加石化资源定义
test: 添加石化链静态验证
docs: 添加石化资源链实施计划
docs: 添加石化资源链设计
```
