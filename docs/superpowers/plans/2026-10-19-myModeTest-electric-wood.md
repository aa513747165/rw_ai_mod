# myModeTest 电动木材包 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增电动锯木机（600W）与电动木炭窑（1500W）两个建筑、1 个共用研究 `MMT_ElectricWoodworking`，以及 3 个木材电动配方；手动配方保持不变。

**Architecture:** 全部为新文件（研究/建筑/配方各 1 个 XML + 12 张贴图），不改动既有链文件。验证器新增独立 `electric-wood` 套件并纳入 `all`（位于 `electric` 之后）。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-electric-wood-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 建筑使用原版电力组件，不得使用 `SK.CompProperties_IdlePower`。
- 配方不得使用 RimWorld 1.7 不存在的 `SmithingSpeed` 或 `Wooden` 分类。
- 输入使用原版 `WoodLog`（已在 `VANILLA_THINGS` 白名单）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 两建筑功耗不同（锯木机 600W、木炭窑 1500W），电力断言必须按建筑分别核对，不能统一值。
2. 电锯木板/电劈柴火挂电动锯木机、电烧木炭挂电动木炭窑，挂载断言必须按配方对应建筑分别核对。
3. 电劈柴火的技能成长为 0.25（其余 0.8），learn 断言按期望键取值。
4. 贴图含遮罩共 12 张，复制时 `{side}.png` 与 `{side}m.png` 一一对应。

---

### Task 1: 新增 electric-wood 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数与木材资源 XML 常量。
- Produces: `electric-wood` 套件，纳入 `all`。

- [ ] **Step 1: 新增常量**

```python
ELECTRIC_WOOD_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricWood.xml"
ELECTRIC_WOOD_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricWood.xml"
ELECTRIC_WOOD_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricWood.xml"

ELECTRIC_WOOD_BUILDING_TEXTURES = [
    *[f"1.7/Textures/Things/Building/Production/MMT_ElectricSawmill/MMT_ElectricSawmill_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
    *[f"1.7/Textures/Things/Building/Production/MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
]

ELECTRIC_WOOD_BUILDING_EXPECTATIONS = {
    "MMT_ElectricSawmill": {
        "label": "电动锯木机",
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
```

- [ ] **Step 2: 实现 `validate_electric_wood()`**

1. 研究检查：`MMT_Research_ElectricWood.xml` 仅含 `MMT_ElectricWoodworking`；label 电动木材加工、baseCost 1500、techLevel Industrial、前置 `["MMT_CharcoalBurning", "MicroelectronicsBasics"]`、坐标 2.00/1.00。
2. 建筑检查：defName 集合等于期望；按期望键逐项断言 label/thingClass/texPath/Graphic_Multi/stuffCategories/costStuffCount/costList（`ComponentIndustrial`+`MMT_ElectronicParts`）/WorkToBuild/MaxHitPoints/size/recipes/研究前置 `["MMT_ElectricWoodworking"]`/建造技能。comps 必须包含 `CompProperties_Power`（compClass `CompPowerTrader`、basePowerConsumption 按期望）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`；木炭窑额外检查 `CompProperties_Glower`（glowRadius 4、glowColor `(217,112,33,0)`）。
3. 配方检查：单输入 `WoodLog`（参照 `validate_wood_recipes` 的单输入断言）；按期望键断言 work/speed/effect/sound/learn/skill/输出；输入必须在 `VANILLA_THINGS`，输出必须在木材资源闭包（`WOOD_RESOURCE_XML`）；不得写 recipeUsers，必须挂载到期望建筑；研究前置 `MMT_ElectricWoodworking`。
4. 12 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `SmithingSpeed` 或 `Wooden`。

- [ ] **Step 3: 注册套件**

`SUITES` 追加 `"electric-wood": validate_electric_wood`；`validate_all()` 在 `validate_electric()` 之后追加 `validate_electric_wood()`。

- [ ] **Step 4: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-wood`
预期：失败，提示缺少 `MMT_Research_ElectricWood.xml` 等文件。

- [ ] **Step 5: 提交**

```
git add tools/validate_mod.py
git commit -m "test: 添加电动木材包静态验证"
```

---

### Task 2: 建筑、研究与贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricWood.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricWood.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_ElectricSawmill/`（6 张）
- Create: `1.7/Textures/Things/Building/Production/MMT_ElectricCharcoalKiln/`（6 张）

- [ ] **Step 1: 复制贴图**

HSK 源 `Mods/Core_SK/Textures/Things/Building/Production/`：
- `TableSawmill/TableSawmill_{east,north,south}{,m}.png` → `MMT_ElectricSawmill/MMT_ElectricSawmill_{east,north,south}{,m}.png`
- `Kiln/Kiln_{east,north,south}{,m}.png` → `MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln_{east,north,south}{,m}.png`

- [ ] **Step 2: 编写研究 XML**

`MMT_Research_ElectricWood.xml` 含唯一研究 `MMT_ElectricWoodworking`（电动木材加工，1500，Industrial，前置 `MMT_CharcoalBurning`+`MicroelectronicsBasics`，坐标 2.00/1.00）。

- [ ] **Step 3: 编写建筑 XML**

`MMT_Buildings_ElectricWood.xml` 含两建筑，结构参照 `MMT_Buildings_Electric.xml` 与 `MMT_Buildings_Wood.xml`：

- `MMT_ElectricSawmill`：3×1、surfaceType Item、interactionCellOffset (0,0,-1)、Metallic×150 + 零部件 10 + 电子元件 5、WorkToBuild 5000、HP 170、Flammability 1.0、Cleanliness -7、Mass 500、comps（Power 600W + Flickable + Breakdownable）、recipes 2 项、建造技能 10、uninstallWork 2500
- `MMT_ElectricCharcoalKiln`：3×3、interactionCellOffset (0,0,-2)、Stony×180 + 零部件 5 + 电子元件 3、WorkToBuild 3000、HP 270、Flammability 0、Cleanliness -8、Mass 500、comps（Power 1500W + Flickable + Breakdownable + Glower 半径 4 颜色 (217,112,33,0)）、recipes 1 项、建造技能 8、uninstallWork 1850

- [ ] **Step 4: 提交**

```
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricWood.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricWood.xml 1.7/Textures/Things/Building/Production/MMT_ElectricSawmill/ 1.7/Textures/Things/Building/Production/MMT_ElectricCharcoalKiln/
git commit -m "feat: 添加电动锯木机、电动木炭窑与电动木材加工研究"
```

---

### Task 3: 电动配方与最终验证（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_ElectricWood.xml`

- [ ] **Step 1: 编写配方 XML**

3 个配方，结构参照 `MMT_Recipes_Wood.xml`，研究前置 `MMT_ElectricWoodworking`，不写 recipeUsers。数值以 Task 1 期望常量为准。

- [ ] **Step 2: 运行 electric-wood 套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-wood`
预期：通过。

- [ ] **Step 3: 提交**

```
git add 1.7/Defs/RecipeDefs/MMT_Recipes_ElectricWood.xml
git commit -m "feat: 添加电动木材加工配方"
```

- [ ] **Step 4: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 36 增至 39）。

- [ ] **Step 5: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 警告 0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 6: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
