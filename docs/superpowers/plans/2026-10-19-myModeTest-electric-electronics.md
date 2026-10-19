# myModeTest 电子电动工作台 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增电子电动工作台（600W）、1 个共用研究 `MMT_ElectricElectronics`，以及 3 个电子电动配方；手动配方保持不变。

**Architecture:** 全部为新文件（研究/建筑/配方各 1 个 XML + 6 张贴图），不改动既有链文件。验证器新增独立 `electric-electronics` 套件并纳入 `all`（位于 `electric-petrochem` 之后）。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-electric-electronics-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 建筑使用原版电力组件，不得使用 `SK.CompProperties_IdlePower`。
- 配方不得使用 RimWorld 1.7 不存在的 `ElectronicCraftingSpeed`。
- 金原料使用原版 `Gold`（已在 `VANILLA_THINGS` 白名单）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 建筑成本含跨链资源（`MMT_CopperBar` 40），成本断言需逐项核对。
2. 配方输入闭包跨四条链（电子/金属/基础/石化）加 `VANILLA_THINGS` 的 `Gold`，闭包必须完整。
3. 研究为单前置 `MMT_IntegratedCircuitTech`（与本系列其它双前置研究不同），断言按设计核对。

---

### Task 1: 新增 electric-electronics 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数与电子/金属/基础/石化资源 XML 常量。
- Produces: `electric-electronics` 套件，纳入 `all`。

- [ ] **Step 1: 新增常量**

```python
ELECTRIC_ELECTRONICS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricElectronics.xml"
ELECTRIC_ELECTRONICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml"
ELECTRIC_ELECTRONICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_ElectricElectronics.xml"

ELECTRIC_ELECTRONICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]

ELECTRIC_ELECTRONICS_TABLE_RECIPES = [
    "MMT_AssembleElectronicParts_Electric",
    "MMT_AssembleIC_Electric",
    "MMT_AssembleMicrochip_Electric",
]

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
```

- [ ] **Step 2: 实现 `validate_electric_electronics()`**

1. 研究检查：仅含 `MMT_ElectricElectronics`；label 电动电子工艺、baseCost 2200、Industrial、前置 `["MMT_IntegratedCircuitTech"]`、坐标 18.00/2.00。
2. 建筑检查：仅含 `MMT_ElectricElectronicsTable`；label 电子电动工作台、thingClass、texPath、Graphic_Multi、stuffCategories `["Metallic"]`、costStuffCount 140、costList（`MMT_CopperBar` 40、`ComponentIndustrial` 10）、WorkToBuild 2750、MaxHitPoints 220、size (3,1)、recipes 等于期望列表、研究前置、建造技能 8。comps 必须包含 Power（CompPowerTrader、600W）+ Flickable + Breakdownable。
3. 配方检查：复用多输入元组模式；统一断言 `GeneralLaborSpeed`/`Smith`/`Recipe_Machining`、workSkill Crafting、learn 0.8、研究前置 `MMT_ElectricElectronics`；不得写 recipeUsers，必须挂载到电子电动工作台；输入闭包 `ELECTRONICS_RESOURCE_XML | METAL_RESOURCE_XML | RESOURCE_XML | PETROCHEM_RESOURCE_XML | VANILLA_THINGS`，输出必须在电子资源闭包。
4. 6 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `ElectronicCraftingSpeed`。

- [ ] **Step 3: 注册套件**

`SUITES` 追加 `"electric-electronics": validate_electric_electronics`；`validate_all()` 在 `validate_electric_petrochem()` 之后追加 `validate_electric_electronics()`。

- [ ] **Step 4: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-electronics`
预期：失败，提示缺少文件。

- [ ] **Step 5: 提交**

```
git add tools/validate_mod.py
git commit -m "test: 添加电子电动工作台静态验证"
```

---

### Task 2: 建筑、研究与贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricElectronics.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_ElectricElectronicsTable/`（6 张）

- [ ] **Step 1: 复制贴图**

HSK 源 `Mods/Core_SK/Textures/Things/Building/Production/TableElectronics/TableElectronics_{east,north,south}{,m}.png` → `MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable_{east,north,south}{,m}.png`。

- [ ] **Step 2: 编写研究 XML**

`MMT_Research_ElectricElectronics.xml` 含唯一研究 `MMT_ElectricElectronics`（电动电子工艺，2200，Industrial，前置 `MMT_IntegratedCircuitTech`，坐标 18.00/2.00）。

- [ ] **Step 3: 编写建筑 XML**

`MMT_Buildings_ElectricElectronics.xml` 含唯一建筑 `MMT_ElectricElectronicsTable`，结构参照 `MMT_Buildings_ElectricWood.xml` 的电动锯木机（同为 3×1 电动台）：

- 3×1、surfaceType Item、interactionCellOffset (0,0,-1)、Metallic×140 + 铜锭 40 + 零部件 10
- WorkToBuild 2750、HP 220、Flammability 1.0、Cleanliness -1、Mass 500
- comps：Power（CompPowerTrader、600W、shortCircuitInRain true）+ Flickable + Breakdownable
- recipes 3 项、研究前置 `MMT_ElectricElectronics`、建造技能 8、uninstallWork 1400

- [ ] **Step 4: 提交**

```
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_ElectricElectronics.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ElectricElectronics.xml 1.7/Textures/Things/Building/Production/MMT_ElectricElectronicsTable/
git commit -m "feat: 添加电子电动工作台与电动电子工艺研究"
```

---

### Task 3: 电动配方与最终验证（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_ElectricElectronics.xml`

- [ ] **Step 1: 编写配方 XML**

3 个配方，结构参照 `MMT_Recipes_Electronics.xml`，研究前置 `MMT_ElectricElectronics`，不写 recipeUsers。数值以 Task 1 期望常量为准。

- [ ] **Step 2: 运行 electric-electronics 套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric-electronics`
预期：通过。

- [ ] **Step 3: 提交**

```
git add 1.7/Defs/RecipeDefs/MMT_Recipes_ElectricElectronics.xml
git commit -m "feat: 添加电子电动配方"
```

- [ ] **Step 4: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 42 增至 45）。

- [ ] **Step 5: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 警告 0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 6: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
