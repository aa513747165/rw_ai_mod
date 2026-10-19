# myModeTest 电弧炉电动包 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增电弧炉建筑（电力 2500W）、1 个共用研究 `MMT_ElectricSmelting`，以及 7 个挂载电弧炉的金属/硅电动配方；手动配方保持不变。

**Architecture:** 全部为新文件（研究/建筑/配方各 1 个 XML + 3 张贴图），不改动既有链文件。验证器新增独立 `electric` 套件并纳入 `all`（位于 `electronics` 之后）。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-electric-arc-furnace-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 电弧炉使用原版电力组件（`CompProperties_Power` 等），不得使用 HSK 的 `SK.ThingDef_AnimatedWorktable`/`SK.CompProperties_IdlePower`。
- 产物钢/银/金使用原版 `Steel`/`Silver`/`Gold`（已在 `VANILLA_THINGS` 白名单）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制，只复制 `EAF_off` 静态贴图，不复制动画帧。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. `electric` 套件与手动台相反：电弧炉必须包含 `CompProperties_Power` 且 `basePowerConsumption` 为 2500，缺少即失败。
2. 配方输入闭包跨多条链：金属（矿石/锭）、木材（木炭）、煤炭、金银（矿石）、基础资源（砂）、电子（硅产物）；产物钢/银/金走 `VANILLA_THINGS`。
3. 电炼钢碳源为多物品输入 `(MMT_Charcoal, MMT_Coal)` ×7，复用金属配方的元组输入比较模式。
4. 建筑成本含 `MMT_ReinforcedConcrete`（混凝土链资源），成本断言需逐项核对。

---

### Task 1: 新增 electric 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数与资源/建筑 XML 常量。
- Produces: `electric` 套件，纳入 `all`。

- [ ] **Step 1: 新增常量**

文件常量：

```python
ELECTRIC_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Electric.xml"
ELECTRIC_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml"
ELECTRIC_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml"
```

贴图常量：

```python
ELECTRIC_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_{side}.png"
    for side in ("east", "north", "south")
]
```

配方列表与期望：

```python
ELECTRIC_FURNACE_RECIPES = [
    "MMT_SmeltCopperBar_Electric",
    "MMT_SmeltTinBar_Electric",
    "MMT_MakeBronze_Electric",
    "MMT_SmeltSteel_Electric",
    "MMT_SmeltSilver_Electric",
    "MMT_SmeltGold_Electric",
    "MMT_ExtractSilicon_Electric",
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
}
```

- [ ] **Step 2: 实现 `validate_electric()`**

结构参照 `validate_metal_building` + `validate_metal_recipes`，合并为单函数三段：

1. 研究检查：`MMT_Research_Electric.xml` 仅含 `MMT_ElectricSmelting`；label 电弧冶炼、baseCost 2000、techLevel Industrial、前置 `["MMT_BasicMetalworking", "MicroelectronicsBasics"]`、坐标 11.00/1.00。
2. 建筑检查：`MMT_Buildings_Electric.xml` 仅含 `MMT_ElectricArcFurnace`；label 电弧炉、thingClass `Building_WorkTable`、texPath `Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace`、Graphic_Multi、stuffCategories `["Metallic"]`、costStuffCount 180、costList `MMT_ReinforcedConcrete` 50 / `ComponentIndustrial` 15 / `MMT_ElectronicParts` 6、WorkToBuild 4500、MaxHitPoints 280、size (3,3)、recipes 等于 `ELECTRIC_FURNACE_RECIPES`、研究前置 `["MMT_ElectricSmelting"]`、建造技能 10。comps 必须包含 `CompProperties_Power`（compClass `CompPowerTrader`、basePowerConsumption 2500）、`CompProperties_Flickable`、`CompProperties_Breakdownable`、`CompProperties_Glower`（glowRadius 4、glowColor `(217,112,33,0)`）；缺少电力组件即失败（与手动台检查相反）。
3. 配方检查：复用金属配方的元组输入比较与 fixedIngredientFilter 扁平化集合比较；统一断言 `SmeltingSpeed`/`Smelt`/`Recipe_Smelt`/workSkill Crafting/learn 0.8/研究前置 `MMT_ElectricSmelting`；不得写 recipeUsers，必须挂载到电弧炉 recipes 列表。输入闭包：`METAL_RESOURCE_XML | WOOD_RESOURCE_XML | COAL_RESOURCE_XML | PRECIOUS_RESOURCE_XML | RESOURCE_XML`；输出闭包额外并入 `ELECTRONICS_RESOURCE_XML` 与 `VANILLA_THINGS`。
4. 3 张 PNG 校验；3 个新 XML 的 `SK.` 扫描。

- [ ] **Step 3: 注册套件**

`SUITES` 追加 `"electric": validate_electric`；`validate_all()` 在 `validate_electronics()` 之后追加 `validate_electric()`。

- [ ] **Step 4: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric`
预期：失败，提示缺少 `MMT_Research_Electric.xml` 等文件。

- [ ] **Step 5: 提交**

```
git add tools/validate_mod.py
git commit -m "test: 添加电弧炉电动包静态验证"
```

---

### Task 2: 电弧炉建筑、研究与贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Electric.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_{east,north,south}.png`

- [ ] **Step 1: 复制贴图**

| HSK 源（`Mods/Core_SK/Textures/`） | 目标（`1.7/Textures/`） |
|---|---|
| `Things/Building/Production/EAF/EAF_off_east.png` | `Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_east.png` |
| `Things/Building/Production/EAF/EAF_off_north.png` | `Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_north.png` |
| `Things/Building/Production/EAF/EAF_off_south.png` | `Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_south.png` |

- [ ] **Step 2: 编写研究 XML**

`MMT_Research_Electric.xml` 含唯一研究 `MMT_ElectricSmelting`（电弧冶炼，2000，Industrial，前置 `MMT_BasicMetalworking`+`MicroelectronicsBasics`，坐标 11.00/1.00），结构参照 `MMT_Research_Electronics.xml`。

- [ ] **Step 3: 编写建筑 XML**

`MMT_Buildings_Electric.xml` 含唯一建筑 `MMT_ElectricArcFurnace`，结构参照 `MMT_Buildings_Glassworks.xml`（同为电力工作台）：

- 3×3、Graphic_Multi、drawSize (3,3)、CutoutComplex；interactionCellOffset `(0,0,-2)`（与手动熔炉一致）
- stuffCategories `Metallic`、costStuffCount 180；costList：`MMT_ReinforcedConcrete` 50、`ComponentIndustrial` 15、`MMT_ElectronicParts` 6
- statBases：WorkToBuild 4500、MaxHitPoints 280、Flammability 1.0、Cleanliness -8、Mass 1000
- comps：Power（CompPowerTrader、2500、shortCircuitInRain true）、Flickable、Breakdownable、Glower（半径 4、颜色 (217,112,33,0)）
- recipes 列表按 `ELECTRIC_FURNACE_RECIPES` 顺序；研究前置 `MMT_ElectricSmelting`；建造技能 10
- building.uninstallWork 2000；inspectorTabs ITab_Bills；placeWorkers PlaceWorker_PreventInteractionSpotOverlap

- [ ] **Step 4: 提交**

```
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Electric.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml 1.7/Textures/Things/Building/Production/MMT_ElectricArcFurnace/
git commit -m "feat: 添加电弧炉建筑与电弧冶炼研究"
```

---

### Task 3: 电动配方与最终验证（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml`

- [ ] **Step 1: 编写配方 XML**

7 个配方，结构参照 `MMT_Recipes_Metal.xml`，统一 `SmeltingSpeed`/`Smelt`/`Recipe_Smelt`、workSkill Crafting、learn 0.8、研究前置 `MMT_ElectricSmelting`，不写 recipeUsers（由电弧炉 recipes 列表挂载）。数值以 Task 1 期望常量为准。

- [ ] **Step 2: 运行 electric 套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric`
预期：通过。

- [ ] **Step 3: 提交**

```
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml
git commit -m "feat: 添加电弧炉电动冶炼配方"
```

- [ ] **Step 4: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 33 增至 36）。

- [ ] **Step 5: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 警告 0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 6: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
