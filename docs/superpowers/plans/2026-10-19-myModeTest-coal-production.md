# myModeTest 煤炭资源链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增煤炭资源与煤矿脉，并将炼钢、碳材料配方的碳源改为煤炭/木炭双输入。

**Architecture:** 新增 1 个资源和 1 个矿脉（继承原版 `RockBase`），修改 2 个既有配方的碳源 filter。验证器新增 `coal` 套件，并让金属/石化配方验证支持单 filter 多 thingDefs 的输入形式。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-coal-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不引入 HSK 的 `Coal` 自定义物品分类；碳源双输入直接在 filter 列出 `MMT_Charcoal` 和 `MMT_Coal`。
- 炼钢碳源数量保持 13，碳材料碳源数量保持 25。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 钢配方碳源扩为双输入后，金属配方验证器的"单引用 filter"检查会失败；Task 1 必须先扩展多引用支持并同步期望。
2. 金属/石化配方验证的资源闭包必须并入煤炭资源 XML，否则 `MMT_Coal` 报未知资源。
3. `fixedIngredientFilter` 必须与 ingredients 的引用集合一致；Task 1 的验证逻辑按扁平化集合比较。

---

### Task 1: 扩展煤炭链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数。
- Produces: `python tools/validate_mod.py coal`；金属/石化配方验证支持多引用输入。

- [ ] **Step 1: 确认套件不存在**

Run:

```bash
python tools/validate_mod.py coal
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'coal'`。

- [ ] **Step 2: 添加煤炭验证常量**

在 `ELECTRONICS_BUILDING_XML = ...` 行后插入：

```python
COAL_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Coal.xml"
COAL_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Coal.xml"

COAL_ITEM_TEXTURES = ["1.7/Textures/Things/Item/MMT/MMT_Coal.png"]

COAL_RESOURCE_EXPECTATIONS = {
    "MMT_Coal": {"label": "煤炭", "value": "2.0", "stack": "600", "flammable": "3", "tex": "Things/Item/MMT/MMT_Coal"},
}
```

- [ ] **Step 3: 改造多引用输入支持**

在 `validate_metal_recipes` 和 `validate_petrochem_recipes` 中：

- `require_files` 追加 `COAL_RESOURCE_XML`；`resource_names` 并入 `set(def_map(parse_xml(COAL_RESOURCE_XML)))`。
- 输入解析改为收集完整 `thing_defs` 列表：`actual_inputs.append(("thingDefs", tuple(thing_defs), count))`，仅在 filter 同时含 categories 或无引用时报错。
- 期望比较前将期望的 value 归一化为元组（单字符串转单元素元组）。
- `fixedIngredientFilter` 期望按扁平化集合比较：所有输入引用去重后排序对比实际排序列表。

`METAL_RECIPE_EXPECTATIONS["MMT_SmeltSteel_Hand"]` 的 inputs 改为：

```python
        "inputs": [("thingDefs", "MMT_IronOre", "15"), ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "13")],
```

`PETROCHEM_RECIPE_EXPECTATIONS["MMT_MakeCarbon_Hand"]` 的 inputs 改为：

```python
        "inputs": [
            ("thingDefs", "MMT_Polymers", "25"),
            ("thingDefs", ("MMT_Charcoal", "MMT_Coal"), "25"),
            ("thingDefs", "MMT_Rubber", "15"),
        ],
```

- [ ] **Step 4: 添加煤炭验证函数并注册**

在 `def validate_all_xml() -> None:` 前插入 `validate_coal()`：检查资源字段（按 `COAL_RESOURCE_EXPECTATIONS`）、PNG、矿脉（`RockBase`、label 煤矿脉、图集贴图、HP 1500、`mineableThing` 为 `MMT_Coal`、yield 75、`isResourceRock`、`veinMineable`）、`SK.` 扫描。`SUITES` 中 `"all"` 之前注册 `"coal": validate_coal`。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py coal
```

Expected: 退出码为 `1`，输出包含 `[FAIL] coal: 缺少文件:`。

同时确认金属/石化配方套件因双输入未实施而失败：

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-recipes
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 均退出码 `1`，输入列表断言失败。

- [ ] **Step 6: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加煤炭链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 煤炭资源、矿脉与贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Coal.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Coal.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Coal.png`

**Interfaces:**
- Consumes: Task 1 的 `coal` 套件。
- Produces: `MMT_Coal`、`MMT_MineableCoal`；Task 3 的配方引用 `MMT_Coal`。

- [ ] **Step 1: 创建资源与矿脉 Def**

- `MMT_Coal`：`ResourceBase`、`ResourcesRaw`、`Graphic_Single`、颜色 (85,85,85)、价值 2.0、堆叠 600、可燃性 3、Medieval、无 stuffProps。
- `MMT_MineableCoal`：`RockBase`、原版图集贴图、color (65,34,34)、colorTwo (147,144,134)、HP 1500、产出 `MMT_Coal` ×75、常见度 1.2、矿块 11~18、`isResourceRock`、`veinMineable`。

- [ ] **Step 2: 复制贴图**

```bash
cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/CoalOre.png" "1.7/Textures/Things/Item/MMT/MMT_Coal.png"
```

- [ ] **Step 3: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py coal
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Coal.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Coal.xml 1.7/Textures/Things/Item/MMT/MMT_Coal.png
git commit -m "feat: 添加煤炭资源与煤矿脉" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: 验证退出码为 `0`，输出 `coal: OK`。

---

### Task 3: 配方碳源双输入与完整验证

**Files:**
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml`
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 的 `MMT_Coal`。
- Produces: 钢/碳材料配方碳源双输入；`validate_coal()` 纳入 `validate_all()`。

- [ ] **Step 1: 修改钢配方**

`MMT_SmeltSteel_Hand` 碳源 filter 的 thingDefs 改为 `<li>MMT_Charcoal</li><li>MMT_Coal</li>`；`fixedIngredientFilter` 的 thingDefs 追加 `<li>MMT_Coal</li>`。

- [ ] **Step 2: 修改碳材料配方**

`MMT_MakeCarbon_Hand` 碳源 filter 同上；`fixedIngredientFilter` 追加 `<li>MMT_Coal</li>`。

- [ ] **Step 3: GREEN 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-recipes
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 均退出码 `0`。

- [ ] **Step 4: 纳入总体验证**

`validate_all()` 在 `validate_metal()` 后插入 `validate_coal()`（煤炭先于引用它的配方所在套件无强制顺序，但保持资源链声明顺序）。

- [ ] **Step 5: 完整静态验证与编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `all` 退出码 `0`，输出含 `coal: OK`，`all-xml: OK (28 files)`；编译 0 警告 0 错误；DLL 存在。

- [ ] **Step 6: 提交并验证最终状态**

```bash
git status --short
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml tools/validate_mod.py
git commit -m "feat: 钢与碳材料配方碳源支持煤炭" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
git status --short
git log --oneline --max-count=6
```

Expected: 提交前只显示上述三个文件（另有与本链无关的未跟踪 docs，不提交）；提交后日志最新为 `feat: 钢与碳材料配方碳源支持煤炭`。
