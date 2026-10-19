# myModeTest 硫磺支线 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增硫磺资源与硫磺矿脉，并将合成橡胶、合成纤维配方恢复 HSK 等值（各增加硫磺 ×3 输入）。

**Architecture:** 新增 1 个资源和 1 个矿脉（继承原版 `RockBase`），修改 2 个既有石化配方增加第二输入。验证器新增 `sulfur` 套件，并更新石化配方期望常量。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-sulfur-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不引入 HSK 的 `Chemical` 自定义物品分类；硫磺使用原版 `ResourcesRaw`。
- 橡胶/纤维配方的聚合物数量、产出、工作量、技能要求均不变，只增加硫磺 ×3 输入。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 石化配方验证器的资源闭包必须并入硫磺资源 XML，否则 `MMT_Sulfur` 报未知资源。
2. 橡胶/纤维期望的 inputs 改为两条目后，`fixedIngredientFilter` 扁平化集合比较会自动覆盖，无需额外改动比较逻辑。
3. `validate_all()` 中 `validate_sulfur()` 需放在 `validate_petrochem()` 之前（石化配方引用硫磺资源）。

---

### Task 1: 扩展硫磺支线静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数。
- Produces: `python tools/validate_mod.py sulfur`；石化配方期望更新。

- [ ] **Step 1: 确认套件不存在**

Run:

```bash
python tools/validate_mod.py sulfur
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'sulfur'`。

- [ ] **Step 2: 添加硫磺验证常量**

在 `COAL_BUILDING_XML = ...` 行后插入：

```python
SULFUR_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Sulfur.xml"
SULFUR_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Sulfur.xml"

SULFUR_ITEM_TEXTURES = ["1.7/Textures/Things/Item/MMT/MMT_Sulfur.png"]

SULFUR_RESOURCE_EXPECTATIONS = {
    "MMT_Sulfur": {"label": "硫磺", "value": "7", "stack": "300", "flammable": "2", "tex": "Things/Item/MMT/MMT_Sulfur"},
}
```

（常量放在 `COAL_ITEM_TEXTURES`/`COAL_RESOURCE_EXPECTATIONS` 附近，与其它链的分组风格一致。）

- [ ] **Step 3: 更新石化配方期望**

`PETROCHEM_RECIPE_EXPECTATIONS["MMT_MakeRubber_Hand"]` 的 inputs 改为：

```python
        "inputs": [("thingDefs", "MMT_Polymers", "30"), ("thingDefs", "MMT_Sulfur", "3")],
```

`PETROCHEM_RECIPE_EXPECTATIONS["MMT_MakeSyntheticFibers_Hand"]` 的 inputs 改为：

```python
        "inputs": [("thingDefs", "MMT_Polymers", "25"), ("thingDefs", "MMT_Sulfur", "3")],
```

`validate_petrochem_recipes` 的 `require_files` 追加 `SULFUR_RESOURCE_XML`；`resource_names` 并入 `set(def_map(parse_xml(SULFUR_RESOURCE_XML)))`。

- [ ] **Step 4: 添加硫磺验证函数并注册**

在 `def validate_all_xml() -> None:` 前插入 `validate_sulfur()`：检查资源字段（按 `SULFUR_RESOURCE_EXPECTATIONS`，`techLevel` 为 Industrial）、PNG、矿脉（`RockBase`、label 硫磺矿脉、图集贴图、HP 1500、`mineableThing` 为 `MMT_Sulfur`、yield 75、`isResourceRock`、`veinMineable`）、`SK.` 扫描。`SUITES` 中 `"all"` 之前注册 `"sulfur": validate_sulfur`。`validate_all()` 在 `validate_petrochem()` 前插入 `validate_sulfur()`。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py sulfur
```

Expected: 退出码为 `1`，输出包含 `[FAIL] sulfur: 缺少文件:`。

同时确认石化配方套件因硫磺输入未实施而失败：

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 退出码 `1`，输入列表断言失败。

- [ ] **Step 6: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加硫磺支线静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 硫磺资源、矿脉与贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Sulfur.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Sulfur.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Sulfur.png`

**Interfaces:**
- Consumes: Task 1 的 `sulfur` 套件。
- Produces: `MMT_Sulfur`、`MMT_MineableSulfur`；Task 3 的配方引用 `MMT_Sulfur`。

- [ ] **Step 1: 创建资源与矿脉 Def**

- `MMT_Sulfur`：`ResourceBase`、`ResourcesRaw`、`Graphic_Single`、颜色 (255,255,255)、价值 7、堆叠 300、可燃性 2、Industrial、无 stuffProps、无爆炸 comp。
- `MMT_MineableSulfur`：`RockBase`、原版图集贴图、color (65,34,34)、colorTwo (216,190,80)、HP 1500、产出 `MMT_Sulfur` ×75、常见度 0.8、矿块 10~16、`isResourceRock`、`veinMineable`。

- [ ] **Step 2: 复制贴图**

```bash
cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Sulfur.png" "1.7/Textures/Things/Item/MMT/MMT_Sulfur.png"
```

- [ ] **Step 3: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py sulfur
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Sulfur.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Sulfur.xml 1.7/Textures/Things/Item/MMT/MMT_Sulfur.png
git commit -m "feat: 添加硫磺资源与硫磺矿脉" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: 验证退出码为 `0`，输出 `sulfur: OK`。

---

### Task 3: 配方恢复 HSK 等值与完整验证

**Files:**
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml`

**Interfaces:**
- Consumes: Task 2 的 `MMT_Sulfur`。
- Produces: 橡胶/纤维配方含硫磺输入；`all` 套件通过。

- [ ] **Step 1: 修改合成橡胶配方**

`MMT_MakeRubber_Hand` 的 ingredients 追加第二输入（filter 含 `MMT_Sulfur`，count 3）；`fixedIngredientFilter` 的 thingDefs 追加 `<li>MMT_Sulfur</li>`。

- [ ] **Step 2: 修改合成纤维配方**

`MMT_MakeSyntheticFibers_Hand` 同上。

- [ ] **Step 3: GREEN 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py petrochem-recipes
```

Expected: 退出码 `0`。

- [ ] **Step 4: 完整静态验证与编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `all` 退出码 `0`，输出含 `sulfur: OK`、`petrochem: OK`，`all-xml: OK (30 files)`；编译 0 警告 0 错误；DLL 存在。

- [ ] **Step 5: 提交并验证最终状态**

```bash
git status --short
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Petrochem.xml
git commit -m "feat: 橡胶与合成纤维配方恢复硫磺输入" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
git status --short
git log --oneline --max-count=6
```

Expected: 提交前只显示上述配方文件；提交后工作区干净，日志最新为 `feat: 橡胶与合成纤维配方恢复硫磺输入`。
