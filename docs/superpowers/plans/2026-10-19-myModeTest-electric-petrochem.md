# myModeTest 石化电动实验室 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增石化电动实验室（450W）、1 个共用研究 `MMT_ElectricPetrochemistry`，以及 5 个石化电动配方；手动配方保持不变。

**Architecture:** 全部为新文件（研究/建筑/配方各 1 个 XML + 3 张贴图），不改动既有链文件。验证器新增独立 `electric-petrochem` 套件并纳入 `all`（位于 `electric-wood` 之后）。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-electric-petrochem-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 建筑使用原版电力组件，不得使用 `SK.CompProperties_IdlePower`。
- 配方不得使用 RimWorld 1.7 不存在的 `SmithingSpeed`。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 建筑成本含跨链资源（`MMT_ReinforcedConcrete` 40、`MMT_Glass` 50），成本断言需逐项核对。
2. 电制碳材料为多物品输入含（木炭|煤炭）元组，复用金属配方的元组比较模式。
3. 电蒸馏用 `SmeltingSpeed`，其余 4 个用 `GeneralLaborSpeed`，speed/effect/sound 按期望键分别断言。

---

### Task 1: 新增 electric-petrochem 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数与石化/硫磺/木材/煤炭资源 XML 常量。
- Produces: `electric-petrochem` 套件，纳入 `all`。

- [ ] **Step 1: 新增常量**

```python
ELECTRIC_PETROCHEM_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricPetrochem.xml"
ELECTRIC_PETROCHEM_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricPetrochem.xml"
ELECTRIC_PETROCHEM_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricPetrochem.xml"

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
```

- [ ] **Step 2: 实现 `validate_electric_petrochem()`**

1. 研究检查：仅含 `MMT_ElectricPetrochemistry`；label 电动石化工艺、baseCost 1800、Industrial、前置 `["MMT_BasicPetrochemistry", "MicroelectronicsBasics"]`、坐标 14.00/1.00。
2. 建筑检查：仅含 `MMT_ElectricChemLab`；label 石化电动实验室、thingClass、texPath、Graphic_Multi、stuffCategories `["Metallic"]`、costStuffCount 130、costList（`MMT_ReinforcedConcrete` 40、`MMT_Glass` 50、`ComponentIndustrial` 20、`MMT_ElectronicParts` 5）、WorkToBuild 5500、MaxHitPoints 340、size (4,5)、interactionCellOffset (0,0,3)、recipes 等于期望列表、研究前置、建造技能 10。comps 必须包含 Power（CompPowerTrader、450W）+ Flickable + Breakdownable。
3. 配方检查：复用金属配方的元组输入比较与 fixedIngredientFilter 扁平化集合比较；按期望键断言 work/speed/effect/sound/skill；统一断言 workSkill Crafting、learn 0.8、研究前置 `MMT_ElectricPetrochemistry`；不得写 recipeUsers，必须挂载到石化电动实验室；输入闭包 `PETROCHEM_RESOURCE_XML | SULFUR_RESOURCE_XML | WOOD_RESOURCE_XML | COAL_RESOURCE_XML`，输出必须在石化资源闭包。
4. 3 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `SmithingSpeed`。

- [ ] **Step 3: 注册套件**

`SUITES` 追加 `"electric-petrochem": validate_electric_petrochem`；`validate_all()` 在 `validate_electric_wood()` 之后追加 `validate_electric_petrochem()`。

- [ ] **Step 4: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-petrochem`
预期：失败，提示缺少文件。

- [ ] **Step 5: 提交**

```
git add tools/validate_mod.py
git commit -m "test: 添加石化电动实验室静态验证"
```

---

### Task 2: 建筑、研究与贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricPetrochem.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricPetrochem.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_ElectricChemLab/`（3 张）

- [ ] **Step 1: 复制贴图**

HSK 源 `Mods/Core_SK/Textures/Things/Building/Production/OilRefinery/OilRefinery_{east,north,south}.png` → `MMT_ElectricChemLab/MMT_ElectricChemLab_{east,north,south}.png`。

- [ ] **Step 2: 编写研究 XML**

`MMT_Research_ElectricPetrochem.xml` 含唯一研究 `MMT_ElectricPetrochemistry`（电动石化工艺，1800，Industrial，前置 `MMT_BasicPetrochemistry`+`MicroelectronicsBasics`，坐标 14.00/1.00）。

- [ ] **Step 3: 编写建筑 XML**

`MMT_Buildings_ElectricPetrochem.xml` 含唯一建筑 `MMT_ElectricChemLab`，结构参照 `MMT_Buildings_Electric.xml`：

- 4×5、drawSize (4,5)、interactionCellOffset (0,0,3)、Metallic×130 + 强化混凝土 40 + 玻璃 50 + 零部件 20 + 电子元件 5
- WorkToBuild 5500、HP 340、Flammability 1.0、Cleanliness -3、Mass 800
- comps：Power（CompPowerTrader、450W、shortCircuitInRain true）+ Flickable + Breakdownable
- recipes 5 项、研究前置 `MMT_ElectricPetrochemistry`、建造技能 10、uninstallWork 2800

- [ ] **Step 4: 提交**

```
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricPetrochem.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricPetrochem.xml 1.7/Textures/Things/Building/Production/MMT_ElectricChemLab/
git commit -m "feat: 添加石化电动实验室与电动石化工艺研究"
```

---

### Task 3: 电动配方与最终验证（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_ElectricPetrochem.xml`

- [ ] **Step 1: 编写配方 XML**

5 个配方，结构参照 `MMT_Recipes_Petrochem.xml`，研究前置 `MMT_ElectricPetrochemistry`，不写 recipeUsers。数值以 Task 1 期望常量为准。

- [ ] **Step 2: 运行 electric-petrochem 套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-petrochem`
预期：通过。

- [ ] **Step 3: 提交**

```
git add 1.7/Defs/RecipeDefs/MMT_Recipes_ElectricPetrochem.xml
git commit -m "feat: 添加石化电动配方"
```

- [ ] **Step 4: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 39 增至 42）。

- [ ] **Step 5: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 警告 0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 6: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
