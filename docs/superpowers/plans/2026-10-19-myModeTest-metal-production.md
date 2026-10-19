# myModeTest 基础金属合金资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在现有 `myModeTest` 三条链基础上，实现“矿脉 → 矿石 → 手动熔炉 → 铜锭/锡锭/青铜/钢”的基础金属合金生产链。

**Architecture:** 新增 3 个地表矿脉（继承原版 `RockBase`，使用原版图集贴图）、6 个资源、1 个手动熔炉（普通 `Building_WorkTable`，无电力无 SK 组件）、1 个研究和 4 个手动冶炼配方。钢产出原版 `Steel`，碳源复用木材链的 `MMT_Charcoal`。先扩展静态验证器，再按资源、研究/建筑、配方逐步闭合接口，最后将 metal 套件纳入总体验证。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-metal-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`。
- `packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀。
- 所有自定义 label、description、jobString 使用中文；`defName` 使用英文。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 首版只实现手动金属链，不实现电动熔炉、深层钻井、煤或其他金属。
- 钢配方产出必须是原版 `Steel`，不得新建钢锭 Def 或覆盖原版 Steel。
- 所有配方的 `workSpeedStat` 必须为 `SmeltingSpeed`。
- HSK 贴图来源必须固定为提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 `RockBase`、`Smithing`、`Metallic`、`ResourcesRaw`、`SmeltingSpeed`、`Steel` 若在 1.7 改名，会导致矿脉、研究、材料或配方失效；Task 4 的 `metal-recipes` 套件固定检查这些精确引用。
2. 矿脉的 `mineableThing` 必须与矿石 defName 配对，且需要 `veinMineable` 和 `isResourceRock` 才会在地图生成中出现；Task 3 的验证函数逐项检查。
3. 矿石和锭使用 `Graphic_StackCount`，缺少 `_a/_b/_c` 任一贴图会导致堆叠显示异常；Task 2 的 PNG 检查固定全部 12 张物品图。
4. 钢配方的碳源 `MMT_Charcoal` 来自木材链，必须出现在 `fixedIngredientFilter` 且引用闭包覆盖木材资源 XML；Task 4 固定检查。
5. HSK 熔炉依赖 `SK_WorkTableFueled` 父类和燃料组件，误入首版会扩大范围并产生悬空引用；Task 3 验证熔炉无 comps 电力组件，并扫描 `SK.`。

---

### Task 1: 扩展金属链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有 `validate_mod.py` 的 `parse_xml`、`def_map`、`child_text`、`li_values`、`assert_equal`、`assert_png`、`read_text`。
- Produces: `python tools/validate_mod.py {metal-resources|metal-building|metal-recipes|metal}`；Task 2-4 分别使用这些套件。

- [ ] **Step 1: 运行 metal-resources 验证，确认套件不存在**

Run:

```bash
python tools/validate_mod.py metal-resources
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'metal-resources'`。

- [ ] **Step 2: 添加金属验证常量和期望**

在 `WOOD_BUILDING_XML = ...` 行后插入：

```python
METAL_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml"
METAL_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml"
METAL_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Metal.xml"
METAL_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml"
```

在 `WOOD_BUILDING_TEXTURES` 列表后插入：

```python
METAL_ITEM_TEXTURES = [
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}_{suffix}.png" for ore in ("IronOre", "CopperOre", "TinOre") for suffix in ("a", "b", "c")],
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_{suffix}.png" for suffix in ("a", "b", "c")],
]

METAL_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]
```

在常量区末尾（`WOOD_RECIPE_EXPECTATIONS` 之后）插入：

```python
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
```

同时扩展原版白名单常量：

```python
VANILLA_RESEARCH = {"Stonecutting", "Smithing"}
VANILLA_THINGS = {"ComponentIndustrial", "WoodLog", "Steel"}
```

- [ ] **Step 3: 添加金属验证函数**

在 `def validate_all_xml() -> None:` 前插入四个函数：

```python
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
```

- [ ] **Step 4: 注册金属验证套件**

在 `SUITES` 中 `"all"` 之前插入：

```python
    "metal-resources": validate_metal_resources,
    "metal-building": validate_metal_building,
    "metal-recipes": validate_metal_recipes,
    "metal": validate_metal,
```

- [ ] **Step 5: 验证新套件按预期缺少产品文件**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] metal-resources: 缺少文件:`，并列出 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml`。

- [ ] **Step 6: 验证现有总套件仍然通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出以 `all-xml: OK (14 files)` 结束。

- [ ] **Step 7: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加金属链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 金属资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ore/`（9 张矿石贴图）
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ingot/`（3 张锭贴图）

**Interfaces:**
- Consumes: Task 1 的 `metal-resources` 套件。
- Produces: `MMT_IronOre`、`MMT_CopperOre`、`MMT_TinOre`、`MMT_CopperBar`、`MMT_TinBar`、`MMT_Bronze`；Task 4 的配方引用这些 defName。

- [ ] **Step 1: 运行 metal-resources 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] metal-resources: 缺少文件:`。

- [ ] **Step 2: 创建资源 Def**

创建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml`，包含 3 个矿石（`ResourceBase`、`ResourcesRaw`、`Graphic_StackCount`、无 stuffProps）和 3 个锭（`ResourceBase`、`ResourcesRaw`、`Graphic_StackCount`、共享锭贴图、stuffProps 含 `Metallic` 分类和设计文档 5.2 节的核心因子），数值以设计文档第 5 节为准。

- [ ] **Step 3: 下载已授权的资源贴图**

Run（在仓库根目录执行，单行逐个下载，共 12 个文件）：

```bash
SHA=0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac
BASE="https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/$SHA/Mods/Core_SK/Textures/Things/Item"
mkdir -p "1.7/Textures/Things/Item/MMT/MMT_Ore" "1.7/Textures/Things/Item/MMT/MMT_Ingot"
for s in a b c; do
  curl -fsSL "$BASE/Ores/Iron/Iron_$s.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_IronOre_$s.png"
  curl -fsSL "$BASE/Ores/Copper/Copper_$s.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_CopperOre_$s.png"
  curl -fsSL "$BASE/Ores/Tin/Tin_$s.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_TinOre_$s.png"
done
curl -fsSL "$BASE/Ingot/ingot_A.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_a.png"
curl -fsSL "$BASE/Ingot/ingot_B.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_b.png"
curl -fsSL "$BASE/Ingot/ingot_C.png" -o "1.7/Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_c.png"
```

- [ ] **Step 4: 运行 metal-resources 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-resources
```

Expected: 退出码为 `0`，输出 `metal-resources: OK`。

- [ ] **Step 5: 提交资源定义和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml 1.7/Textures/Things/Item/MMT/MMT_Ore 1.7/Textures/Things/Item/MMT/MMT_Ingot
git commit -m "feat: 添加金属资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 金属研究、手动熔炉与矿脉

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Metal.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_HandFurnace/`（6 张熔炉贴图含遮罩）

**Interfaces:**
- Consumes: Task 1 的 `metal-building` 套件。
- Produces: `MMT_BasicMetalworking`、`MMT_HandFurnace`（挂载 4 个配方）、3 个矿脉；Task 4 依赖这些名称。

- [ ] **Step 1: 运行 metal-building 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-building
```

Expected: 退出码为 `1`，输出包含 `[FAIL] metal-building: 缺少文件:`。

- [ ] **Step 2: 创建研究 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_Research_Metal.xml`：`MMT_BasicMetalworking`，baseCost 800，Medieval，前置 `Smithing`，坐标 `(10.00, 0.00)`。

- [ ] **Step 3: 创建熔炉与矿脉 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml`：

- `MMT_HandFurnace`：`BuildingBase` 父类、`Building_WorkTable`、3×3、`Stony` ×240、WorkToBuild 3000、HP 280、Flammability 0、Cleanliness -8、无电力组件、建造技能 5、`interactionCellOffset (0,0,-2)`、recipes 挂载 4 个配方，结构与既有手动建筑保持一致。
- 3 个矿脉：`RockBase` 父类、原版图集贴图、颜色按设计文档 7.3 节、`isResourceRock`、`veinMineable`、mineableYield 75。

- [ ] **Step 4: 下载已授权的熔炉贴图**

```bash
SHA=0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac
BASE="https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/$SHA/Mods/Core_SK/Textures/Things/Building/Production/Furnace"
mkdir -p "1.7/Textures/Things/Building/Production/MMT_HandFurnace"
for side in east north south; do
  curl -fsSL "$BASE/Furnace_$side.png" -o "1.7/Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_$side.png"
  curl -fsSL "$BASE/Furnace_${side}m.png" -o "1.7/Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_${side}m.png"
done
```

- [ ] **Step 5: 运行 metal-building 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-building
```

Expected: 退出码为 `0`，输出 `metal-building: OK`。

- [ ] **Step 6: 提交研究和建筑**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Metal.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml 1.7/Textures/Things/Building/Production/MMT_HandFurnace
git commit -m "feat: 添加金属研究与手动熔炉" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 金属配方与完整验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 的 6 个金属资源、木材链的 `MMT_Charcoal`、Task 3 的 `MMT_BasicMetalworking` 和 `MMT_HandFurnace`。
- Produces: 4 个手动冶炼配方，并将 `validate_metal()` 纳入 `validate_all()`。

- [ ] **Step 1: 运行 metal-recipes 验证，确认失败**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-recipes
```

Expected: 退出码为 `1`，输出包含 `[FAIL] metal-recipes: 缺少文件:`。

- [ ] **Step 2: 创建配方 Def**

创建 `1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml`，包含 4 个配方，数值以设计文档第 6 节为准：统一 `SmeltingSpeed`/`Smelt`/`Recipe_Smelt`/Crafting/0.8，研究前置 `MMT_BasicMetalworking`，不写 `recipeUsers`。

- [ ] **Step 3: 运行 metal-recipes 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-recipes
```

Expected: 退出码为 `0`，输出 `metal-recipes: OK`。

- [ ] **Step 4: 将金属套件纳入总体验证**

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
metal-resources: OK
metal-building: OK
metal-recipes: OK
metal: OK
all-xml: OK (18 files)
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
?? 1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml
 M tools/validate_mod.py
```

然后执行：

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml tools/validate_mod.py
git commit -m "feat: 添加金属冶炼配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 8: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=7
```

Expected: `git status --short` 无输出；日志最新提交为：

```text
feat: 添加金属冶炼配方
feat: 添加金属研究与手动熔炉
feat: 添加金属资源定义
test: 添加金属链静态验证
docs: 添加金属资源链实施计划
docs: 添加金属资源链设计
```
