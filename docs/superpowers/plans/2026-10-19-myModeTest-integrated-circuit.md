# myModeTest 集成电路与微芯片 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在既有电子链上延伸：新增集成电路、微芯片资源，1 个共用研究，以及 2 个挂载手动电子工作台的配方。

**Architecture:** 追加到电子链既有文件（资源/研究/配方各 1 个 XML + 电子台 recipes 列表），与强化玻璃扩展玻璃链的做法一致。验证器扩展电子套件期望常量。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-integrated-circuit-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不引入 HSK 的 `ElectronicsCat` 自定义物品分类；使用原版 `ResourcesRaw`。
- 金原料使用原版 `Gold`；不得使用 `ElectronicCraftingSpeed`（1.7 不存在）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 电子配方验证器的研究前置断言当前硬编码 `MMT_BasicElectronics`，必须改为按期望键取值，否则新配方（前置 `MMT_IntegratedCircuitTech`）无法通过。
2. 新配方输入含石化资源（塑料、碳材料）和原版 `Gold`：闭包必须并入 `PETROCHEM_RESOURCE_XML`，输入检查必须并入 `VANILLA_THINGS`。
3. `MMT_Microchip` 的 techLevel 为 Spacer，资源检查需按期望键逐个断言 techLevel。

---

### Task 1: 扩展电子链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数。
- Produces: 电子套件覆盖集成电路/微芯片。

- [ ] **Step 1: 扩展资源期望与贴图列表**

`ELECTRONICS_RESOURCE_EXPECTATIONS` 追加（每项含 `tech` 键，既有 2 项补 `tech: "Industrial"`）：

```python
    "MMT_IntegratedCircuit": {"label": "集成电路", "value": "55.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/IntegratedCircuit", "tech": "Industrial"},
    "MMT_Microchip": {"label": "微芯片", "value": "100.0", "stack": "25", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/Microchip", "tech": "Spacer"},
```

`ELECTRONICS_ITEM_TEXTURES` 追加：

```python
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/Microchip.png",
```

- [ ] **Step 2: 资源验证增加 techLevel 断言**

`validate_electronics_resources` 循环中追加：`assert_equal(child_text(node, "techLevel"), expected["tech"], ...)`。

- [ ] **Step 3: 扩展研究验证**

`validate_electronics_building`：研究集合改为 `{"MMT_BasicElectronics", "MMT_IntegratedCircuitTech"}`；追加新研究字段检查（label 集成电路技术、baseCost 2000、Industrial、前置 `["MMT_BasicElectronics"]`、坐标 17.00/1.00）。电子台研究前置 `researchPrerequisites` 期望改为 `["MMT_BasicElectronics", "MMT_IntegratedCircuitTech"]`。

- [ ] **Step 4: 扩展配方期望与闭包**

`ELECTRONICS_TABLE_RECIPES` 追加 `"MMT_AssembleIC_Hand"`、`"MMT_AssembleMicrochip_Hand"`。

`ELECTRONICS_RECIPE_EXPECTATIONS` 追加（每项含 `research: "MMT_IntegratedCircuitTech"`，既有 2 项补 `research: "MMT_BasicElectronics"`）：

```python
    "MMT_AssembleIC_Hand": {
        "work": "3500",
        "inputs": [
            ("thingDefs", "MMT_Silicon", "25"),
            ("thingDefs", "MMT_Plastic", "20"),
            ("thingDefs", "MMT_ElectronicParts", "3"),
            ("thingDefs", "Gold", "2"),
        ],
        "output": "MMT_IntegratedCircuit",
        "output_count": "5",
        "skill": "10",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
        "research": "MMT_IntegratedCircuitTech",
    },
    "MMT_AssembleMicrochip_Hand": {
        "work": "3500",
        "inputs": [
            ("thingDefs", "MMT_IntegratedCircuit", "2"),
            ("thingDefs", "MMT_Carbon", "5"),
            ("thingDefs", "Gold", "5"),
        ],
        "output": "MMT_Microchip",
        "output_count": "1",
        "skill": "12",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
        "research": "MMT_IntegratedCircuitTech",
    },
```

`validate_electronics_recipes`：

- `require_files` 追加 `PETROCHEM_RESOURCE_XML`；`resource_names` 并入 `set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))`。
- 研究前置断言改为 `assert_equal(research, expected["research"], ...)`。
- 输入闭包检查改为 `if item not in resource_names | VANILLA_THINGS`（覆盖原版 `Gold`）。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics
```

Expected: 退出码为 `1`（资源 defName 集合断言失败）。

- [ ] **Step 6: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加集成电路与微芯片静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 集成电路/微芯片资源、研究、配方与贴图

**Files:**
- Modify: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml`
- Modify: `1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml`
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png`、`Microchip.png`

**Interfaces:**
- Consumes: Task 1 的电子套件期望。
- Produces: 集成电路链全部 Def；`all` 套件通过。

- [ ] **Step 1: 追加资源 Def**

- `MMT_IntegratedCircuit`：`ResourceBase`、`ResourcesRaw`、`Graphic_Single`、价值 55.0、堆叠 75、可燃 1.0、Industrial、无 stuffProps，texPath `Things/Item/MMT/MMT_Electronics/IntegratedCircuit`。
- `MMT_Microchip`：同上，价值 100.0、堆叠 25、Spacer，texPath `.../Microchip`。

- [ ] **Step 2: 追加研究 Def**

`MMT_IntegratedCircuitTech`：label 集成电路技术、baseCost 2000、Industrial、前置 `MMT_BasicElectronics`、坐标 (17.00, 1.00)。

- [ ] **Step 3: 追加配方并挂载电子台**

- `MMT_AssembleIC_Hand`：硅 25 + 塑料 20 + 电子元件 3 + 金 2 → 集成电路 5，3500，Crafting 10，`GeneralLaborSpeed`/`Smith`/`Recipe_Machining`，前置 `MMT_IntegratedCircuitTech`。
- `MMT_AssembleMicrochip_Hand`：集成电路 2 + 碳材料 5 + 金 5 → 微芯片 1，3500，Crafting 12，其余同上。
- `MMT_Buildings_Electronics.xml` 的电子台 recipes 与 researchPrerequisites 各追加对应项。

- [ ] **Step 4: 复制贴图**

```bash
cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Electronics.png" "1.7/Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png"
cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Microchips.png" "1.7/Textures/Things/Item/MMT/MMT_Electronics/Microchip.png"
```

- [ ] **Step 5: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml 1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml 1.7/Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png 1.7/Textures/Things/Item/MMT/MMT_Electronics/Microchip.png
git commit -m "feat: 添加集成电路、微芯片与配套研究配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: 验证退出码为 `0`，输出 `electronics: OK`。

---

### Task 3: 完整验证与最终确认

**Files:**
- 无新增（确认性任务）。

- [ ] **Step 1: 完整静态验证与编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `all` 退出码 `0`，输出含 `electronics: OK`，`all-xml: OK (33 files)`；编译 0 警告 0 错误；DLL 存在。

- [ ] **Step 2: 验证最终状态**

```bash
git status --short
git log --oneline --max-count=6
```

Expected: 工作区干净，日志含集成电路链全部提交。
