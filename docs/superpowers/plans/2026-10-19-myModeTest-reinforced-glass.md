# myModeTest 玻璃扩展链（强化玻璃）Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在既有玻璃链文件上扩展强化玻璃：`MMT_GlassBatch + MMT_Carbon + MMT_SyntheticFibers → MMT_ReinforcedGlass`。

**Architecture:** 复用已有 `MMT_GlassworksTable`，新增 1 个资源、1 个配方、1 个研究（双前置跨链门控）。先扩展玻璃链验证套件（多输入配方支持），再按资源、研究/建筑、配方逐步闭合接口。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-reinforced-glass-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description/jobString，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不新建生产建筑；配方挂载在已有 `MMT_GlassworksTable`。
- `MMT_ReinforcedGlass` 不定义 `stuffProps`。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`（HEAD `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`），用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 配方跨链引用石化链的 `MMT_Carbon` 和 `MMT_SyntheticFibers`，玻璃验证器的资源闭包必须并入石化资源 XML；Task 1 固定检查。
2. 既有玻璃配方验证只支持单输入，强化玻璃是三输入配方；Task 1 新增多输入分支，且不得破坏既有 10 个玻璃配方的验证行为。
3. 新研究前置 `MMT_BasicPetrochemistry` 是跨链引用；Task 3 固定检查双前置顺序。
4. 玻璃工坊 recipes 列表追加后，既有 `BUILDING_RECIPES` 期望必须同步更新，否则 Task 3 验证失败。

---

### Task 1: 扩展玻璃链验证器（多输入支持）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有玻璃套件与公共函数。
- Produces: 玻璃套件覆盖强化玻璃内容；`all` 套件不变。

- [ ] **Step 1: 确认当前基线通过**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，输出以 `all-xml: OK (22 files)` 结束。

- [ ] **Step 2: 扩展玻璃验证常量**

修改：

```python
ITEM_TEXTURES = [
    # ... 既有 6 张 ...
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_a.png",
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_b.png",
    "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_c.png",
]
```

`RESOURCE_EXPECTATIONS` 追加：

```python
    "MMT_ReinforcedGlass": {
        "label": "强化玻璃",
        "value": "17.0",
        "stack": "150",
        "tex": "Things/Item/MMT/MMT_ReinforcedGlass",
        "graphic": "Graphic_StackCount",
        "tech": "Industrial",
    },
```

`BUILDING_RECIPES` 追加 `"MMT_MakeReinforcedGlass"`。

`RECIPE_EXPECTATIONS` 追加（多输入形式）：

```python
    "MMT_MakeReinforcedGlass": {
        "work": "1400",
        "inputs": [
            ("thingDefs", "MMT_GlassBatch", "40"),
            ("thingDefs", "MMT_Carbon", "5"),
            ("thingDefs", "MMT_SyntheticFibers", "5"),
        ],
        "output": "MMT_ReinforcedGlass",
        "output_count": "5",
        "users": [],
        "research": "MMT_ReinforcedGlassTech",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
```

- [ ] **Step 3: 扩展 validate_building 研究检查**

将研究集合期望改为 `{"MMT_Glassworking", "MMT_ReinforcedGlassTech"}`，并追加：

```python
    reinforced = research_defs["MMT_ReinforcedGlassTech"]
    assert_equal(child_text(reinforced, "label"), "强化玻璃工艺", "强化玻璃研究 label")
    assert_equal(child_text(reinforced, "baseCost"), "1500", "强化玻璃研究 baseCost")
    assert_equal(child_text(reinforced, "techLevel"), "Industrial", "强化玻璃研究 techLevel")
    assert_equal(li_values(reinforced, "prerequisites"), ["MMT_Glassworking", "MMT_BasicPetrochemistry"], "强化玻璃研究前置")
    assert_equal(child_text(reinforced, "researchViewX"), "5.00", "强化玻璃研究 X 坐标")
    assert_equal(child_text(reinforced, "researchViewY"), "1.00", "强化玻璃研究 Y 坐标")
```

- [ ] **Step 4: 扩展 validate_recipes 多输入支持**

- `require_files` 追加 `PETROCHEM_RESOURCE_XML`。
- `resource_names` 并入 `set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))`。
- 在逐配方循环中，若期望含 `inputs` 键则走多输入分支（逻辑同 `validate_concrete_recipes` 的 inputs/fixedIngredientFilter 检查），否则保持既有单输入逻辑。
- 多输入分支的原料白名单检查使用并入后的 `resource_names | VANILLA_THINGS`。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] resources: 缺少文件:`，并列出 `MMT_ReinforcedGlass_a.png`。

- [ ] **Step 6: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 扩展玻璃链静态验证覆盖强化玻璃" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 强化玻璃资源与贴图

**Files:**
- Modify: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/`（3 张贴图）

**Interfaces:**
- Consumes: Task 1 的 `resources` 套件。
- Produces: `MMT_ReinforcedGlass`；Task 4 的配方引用该 defName。

- [ ] **Step 1: 在玻璃资源 XML 末尾追加 `MMT_ReinforcedGlass` Def**

`ResourceBase`、`ResourcesRaw`、`Graphic_StackCount`、texPath `Things/Item/MMT/MMT_ReinforcedGlass`、Industrial、价值 17.0、堆叠 150、Flammability 0、Mass 0.22、无 stuffProps。

- [ ] **Step 2: 从本地 HSK 克隆复制贴图**

```bash
HSK="D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/ReinforcedGlass"
mkdir -p "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass"
for s in a b c; do cp "$HSK/ReinforcedGlass_$s.png" "1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_$s.png"; done
```

- [ ] **Step 3: 运行 resources 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py resources
```

Expected: 退出码为 `0`，输出 `resources: OK`。

- [ ] **Step 4: 提交资源和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml 1.7/Textures/Things/Item/MMT/MMT_ReinforcedGlass
git commit -m "feat: 添加强化玻璃资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 强化玻璃研究与玻璃工坊扩展

**Files:**
- Modify: `1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml`

**Interfaces:**
- Consumes: Task 1 的 `building` 套件。
- Produces: `MMT_ReinforcedGlassTech`；玻璃工坊挂载 `MMT_MakeReinforcedGlass`。

- [ ] **Step 1: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py building
```

Expected: 退出码为 `1`，研究集合断言失败。

- [ ] **Step 2: 追加研究 Def**

在 `MMT_Research_Glass.xml` 追加 `MMT_ReinforcedGlassTech`：baseCost 1500、Industrial、前置 `MMT_Glassworking` + `MMT_BasicPetrochemistry`、坐标 `(5.00, 1.00)`。

- [ ] **Step 3: 扩展玻璃工坊 recipes**

在 `MMT_Buildings_Glassworks.xml` 的 recipes 列表追加 `<li>MMT_MakeReinforcedGlass</li>`；研究前置保持 `MMT_Glassworking` 不变。

- [ ] **Step 4: 运行 building 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py building
```

Expected: 退出码为 `0`，输出 `building: OK`。

- [ ] **Step 5: 提交研究和建筑扩展**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml
git commit -m "feat: 添加强化玻璃研究并扩展玻璃工坊" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 强化玻璃配方与完整验证

**Files:**
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml`

**Interfaces:**
- Consumes: Task 2 的 `MMT_ReinforcedGlass`、石化链的 `MMT_Carbon`/`MMT_SyntheticFibers`、Task 3 的研究与建筑。
- Produces: `MMT_MakeReinforcedGlass`。

- [ ] **Step 1: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py recipes
```

Expected: 退出码为 `1`，配方集合断言失败（缺 `MMT_MakeReinforcedGlass`）。

- [ ] **Step 2: 追加配方 Def**

在 `MMT_Recipes_Glass.xml` 追加 `MMT_MakeReinforcedGlass`，数值以设计文档第 6 节为准，不写 `recipeUsers`。

- [ ] **Step 3: 运行 recipes 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py recipes
```

Expected: 退出码为 `0`，输出 `recipes: OK`。

- [ ] **Step 4: 运行完整静态验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
```

Expected: 退出码为 `0`，全部套件 OK，`all-xml: OK (22 files)`（无新增 XML 文件）。

- [ ] **Step 5: 执行 C# 编译验证**

Run:

```bash
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `dotnet build` 退出码为 `0`，0 警告 0 错误；`test -f` 退出码为 `0`。

- [ ] **Step 6: 检查 Git 工作区并提交**

Run:

```bash
git status --short
```

Expected: 只显示 `M 1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml`（另有此前未跟踪的 `docs/hsk-resource-production-paths.md`，不属于本链，不提交）。

然后执行：

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml
git commit -m "feat: 添加强化玻璃配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 7: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=6
```

Expected: `git status --short` 只剩未跟踪的 `docs/hsk-resource-production-paths.md`；日志最新提交为：

```text
feat: 添加强化玻璃配方
feat: 添加强化玻璃研究并扩展玻璃工坊
feat: 添加强化玻璃资源定义
test: 扩展玻璃链静态验证覆盖强化玻璃
docs: 添加强化玻璃链实施计划
docs: 添加强化玻璃链设计
```
