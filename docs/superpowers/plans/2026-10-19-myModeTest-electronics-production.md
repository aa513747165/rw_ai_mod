# myModeTest 电子材料资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现“砂 → 硅 →（+ 铜锭 + 玻璃）→ 电子元件”的手动电子材料生产链。

**Architecture:** 新增 2 个资源、1 个手动电子工作台、1 个研究和 2 个手动配方；硅提取复用已有 `MMT_HandFurnace`（金属链建筑 XML 与验证器同步追加）。先扩展静态验证器，再按资源、研究/建筑、配方逐步闭合接口。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-electronics-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description/jobString，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不得使用 1.7 不存在的 `ElectronicCraftingSpeed`。
- 硅提取挂载已有 `MMT_HandFurnace`；电子元件挂载新建 `MMT_HandElectronicsTable`。
- 两个新资源不定义 `stuffProps`。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 `MicroelectronicsBasics` 若改名会导致研究失效；Task 1 白名单固定登记。
2. 硅提取追加到金属链熔炉，`METAL_FURNACE_RECIPES` 期望必须同步，否则 `metal-building` 套件失败。
3. 电子配方跨链引用玻璃链（`MMT_Sand`、`MMT_Glass`）和金属链（`MMT_CopperBar`），引用闭包必须并入对应资源 XML；Task 1 固定检查。
4. 电子工作台贴图含材料遮罩，缺少 `_m` 后缀图会导致着色异常；Task 3 的 PNG 检查固定全部 6 张。

---

### Task 1: 扩展电子链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数。
- Produces: `python tools/validate_mod.py {electronics-resources|electronics-building|electronics-recipes|electronics}`；`METAL_FURNACE_RECIPES` 同步追加。

- [ ] **Step 1: 确认套件不存在**

Run:

```bash
python tools/validate_mod.py electronics-resources
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'electronics-resources'`。

- [ ] **Step 2: 添加电子验证常量和期望**

在 `PETROCHEM_BUILDING_XML = ...` 行后插入：

```python
ELECTRONICS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml"
ELECTRONICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml"
ELECTRONICS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml"
ELECTRONICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml"
```

在 `PETROCHEM_BUILDING_TEXTURES` 后插入：

```python
ELECTRONICS_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/Silicon.png",
    "1.7/Textures/Things/Item/MMT/MMT_Electronics/ElectronicParts.png",
]

ELECTRONICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]
```

`METAL_FURNACE_RECIPES` 末尾追加 `"MMT_ExtractSilicon_Hand"`。

`VANILLA_RESEARCH` 追加 `"MicroelectronicsBasics"`。

常量区末尾插入：

```python
ELECTRONICS_RESOURCE_EXPECTATIONS = {
    "MMT_Silicon": {"label": "硅", "value": "4.0", "stack": "300", "flammable": "0", "tex": "Things/Item/MMT/MMT_Electronics/Silicon"},
    "MMT_ElectronicParts": {"label": "电子元件", "value": "25.0", "stack": "75", "flammable": "1.0", "tex": "Things/Item/MMT/MMT_Electronics/ElectronicParts"},
}

ELECTRONICS_TABLE_RECIPES = ["MMT_AssembleElectronicParts_Hand"]

ELECTRONICS_RECIPE_EXPECTATIONS = {
    "MMT_ExtractSilicon_Hand": {
        "work": "1600",
        "inputs": [("thingDefs", "MMT_Sand", "50")],
        "output": "MMT_Silicon",
        "output_count": "5",
        "skill": "5",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
        "building": "MMT_HandFurnace",
    },
    "MMT_AssembleElectronicParts_Hand": {
        "work": "2500",
        "inputs": [
            ("thingDefs", "MMT_CopperBar", "15"),
            ("thingDefs", "MMT_Glass", "10"),
            ("thingDefs", "MMT_Silicon", "10"),
        ],
        "output": "MMT_ElectronicParts",
        "output_count": "5",
        "skill": "8",
        "speed": "GeneralLaborSpeed",
        "effect": "Smith",
        "sound": "Recipe_Machining",
        "building": "MMT_HandElectronicsTable",
    },
}
```

- [ ] **Step 3: 添加电子验证函数**

在 `def validate_all_xml() -> None:` 前插入四个函数（结构同 `validate_petrochem_*`）：

- `validate_electronics_resources()`：检查 2 个资源的 label/value/stack/Flammability/texPath/graphicClass/thingCategories/无 stuffProps，检查 2 张 PNG。
- `validate_electronics_building()`：检查研究（label 基础电子工艺、baseCost 1600、Industrial、前置 `MicroelectronicsBasics`、坐标 16.00/0.00）；检查建筑集合恰为 `{MMT_HandElectronicsTable}`；建筑字段按设计 7.2 节（3×3、Metallic ×140 + 零部件 ×5、WorkToBuild 2200、HP 220、技能 8、无电力组件）；检查 6 张 PNG。
- `validate_electronics_recipes()`：闭包并入 `RESOURCE_XML`（玻璃）、`METAL_RESOURCE_XML`（金属）与 `ELECTRONICS_RESOURCE_XML`；逐配方检查输入/输出/技能/速度/研究前置 `MMT_BasicElectronics`；按期望的 `building` 键检查挂载建筑（熔炉或电子台）；配方 XML 禁用 `SK.` 和 `ElectronicCraftingSpeed`。
- `validate_electronics()`：顺序调用三者并打印 `electronics: OK`。

- [ ] **Step 4: 注册套件**

`SUITES` 中 `"all"` 之前插入 `electronics-resources/building/recipes/electronics` 四项。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] electronics-resources: 缺少文件:`。

同时确认金属建筑套件因熔炉配方列表未同步而失败：

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-building
```

Expected: 退出码为 `1`，输出包含 `熔炉配方` 断言失败（期望含 `MMT_ExtractSilicon_Hand`）。

- [ ] **Step 6: 验证其余总套件仍通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `1`，失败点仅为 `metal-building` 的熔炉配方列表（Task 3 修复）。

- [ ] **Step 7: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加电子链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 电子资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Electronics/`（2 张贴图）

**Interfaces:**
- Consumes: Task 1 的 `electronics-resources` 套件。
- Produces: `MMT_Silicon`、`MMT_ElectronicParts`。

- [ ] **Step 1: RED 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-resources
```

Expected: 退出码为 `1`，缺少文件。

- [ ] **Step 2: 创建资源 Def**

创建 `MMT_Items_Resource_Electronics.xml`：`MMT_Silicon` 与 `MMT_ElectronicParts`（`ResourceBase`、`ResourcesRaw`、`Graphic_Single`、无 stuffProps），数值以设计文档第 5 节为准。

- [ ] **Step 3: 复制贴图**

```bash
HSK="D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource"
mkdir -p "1.7/Textures/Things/Item/MMT/MMT_Electronics"
cp "$HSK/Silicon.png" "1.7/Textures/Things/Item/MMT/MMT_Electronics/Silicon.png"
cp "$HSK/EC.png" "1.7/Textures/Things/Item/MMT/MMT_Electronics/ElectronicParts.png"
```

- [ ] **Step 4: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-resources
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Electronics.xml 1.7/Textures/Things/Item/MMT/MMT_Electronics
git commit -m "feat: 添加电子资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: 验证退出码为 `0`，输出 `electronics-resources: OK`。

---

### Task 3: 电子研究、手动电子工作台与熔炉扩展

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml`（熔炉 recipes 追加）
- Create: `1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable/`（6 张贴图）

**Interfaces:**
- Consumes: Task 1 的 `electronics-building`、`metal-building` 套件。
- Produces: `MMT_BasicElectronics`、`MMT_HandElectronicsTable`；熔炉挂载 `MMT_ExtractSilicon_Hand`。

- [ ] **Step 1: RED 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-building
```

Expected: 退出码为 `1`，缺少文件。

- [ ] **Step 2: 创建研究 Def**

`MMT_BasicElectronics`：baseCost 1600、Industrial、前置 `MicroelectronicsBasics`、坐标 `(16.00, 0.00)`。

- [ ] **Step 3: 创建电子工作台 Def 并扩展熔炉**

- `MMT_Buildings_Electronics.xml`：`MMT_HandElectronicsTable` 按设计 7.2 节。
- `MMT_Buildings_Metal.xml`：熔炉 recipes 列表追加 `<li>MMT_ExtractSilicon_Hand</li>`。

- [ ] **Step 4: 复制建筑贴图**

```bash
HSK="D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/TableElectronics"
mkdir -p "1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable"
for side in east north south; do
  cp "$HSK/TableElectronics_$side.png" "1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_$side.png"
  cp "$HSK/TableElectronics_${side}m.png" "1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_${side}m.png"
done
```

- [ ] **Step 5: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-building
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-building
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Electronics.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electronics.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml 1.7/Textures/Things/Building/Production/MMT_HandElectronicsTable
git commit -m "feat: 添加电子研究与手动电子工作台" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: 两个套件均退出码 `0`。

---

### Task 4: 电子配方与完整验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 玻璃链 `MMT_Sand`/`MMT_Glass`、金属链 `MMT_CopperBar`、Task 2-3 的资源与建筑。
- Produces: 2 个配方；`validate_electronics()` 纳入 `validate_all()`。

- [ ] **Step 1: RED 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-recipes
```

Expected: 退出码为 `1`，缺少 `MMT_Recipes_Electronics.xml`。

- [ ] **Step 2: 创建配方 Def**

按设计文档第 6 节创建 2 个配方。

- [ ] **Step 3: GREEN 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electronics-recipes
```

Expected: 退出码为 `0`。

- [ ] **Step 4: 纳入总体验证**

`validate_all()` 在 `validate_petrochem()` 后追加 `validate_electronics()`。

- [ ] **Step 5: 完整静态验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，末尾依次输出 `electronics-resources: OK`、`electronics-building: OK`、`electronics-recipes: OK`、`electronics: OK`、`all-xml: OK (26 files)`。

- [ ] **Step 6: C# 编译验证**

```bash
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: 0 警告 0 错误；DLL 存在。

- [ ] **Step 7: 提交并验证最终状态**

```bash
git status --short
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Electronics.xml tools/validate_mod.py
git commit -m "feat: 添加电子材料配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
git status --short
git log --oneline --max-count=7
```

Expected: 提交前 `git status --short` 只显示上述两个文件（另有与本链无关的未跟踪 docs，不提交）；提交后工作区恢复干净（除既有未跟踪 docs）；日志最新为 `feat: 添加电子材料配方`。
