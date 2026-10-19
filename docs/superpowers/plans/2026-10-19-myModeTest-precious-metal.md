# myModeTest 金银链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增金/银矿石、金/银矿脉，以及手动熔炉冶炼原版金银的 2 个配方。

**Architecture:** 新增 2 个资源、2 个矿脉（继承原版 `RockBase`）和 2 个配方（挂载既有 `MMT_HandFurnace`，产物为原版 `Silver`/`Gold`）。验证器新增 `precious` 套件，`VANILLA_THINGS` 白名单与 `METAL_FURNACE_RECIPES` 同步更新。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-precious-metal-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`；`packageId` 必须为 `YourName.myModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀；中文 label/description，英文 defName。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- 不引入 HSK 的 `PRS`/`Matty` 自定义物品分类；矿石使用原版 `ResourcesRaw`。
- 配方产物为原版 `Silver`/`Gold`，不实现 HSK 合金锭与助熔剂。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，用文件复制。
- 所有文本文件使用 UTF-8。
- 最终必须执行 `python tools/validate_mod.py all` 和 `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 新配方挂载 `MMT_HandFurnace`，`METAL_FURNACE_RECIPES` 必须先追加 2 个配方名，否则金属建筑验证器在 Task 2 后失败。
2. 产物为原版 `Silver`/`Gold`，`VANILLA_THINGS` 白名单必须先追加，否则配方闭包检查报未知输出。
3. `validate_all()` 中 `validate_precious()` 放在 `validate_metal()` 之后（依赖熔炉建筑定义）。

---

### Task 1: 扩展金银链静态验证器

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数。
- Produces: `python tools/validate_mod.py precious`。

- [ ] **Step 1: 确认套件不存在**

Run:

```bash
python tools/validate_mod.py precious
```

Expected: 退出码为 `2`，输出包含 `invalid choice: 'precious'`。

- [ ] **Step 2: 添加金银验证常量**

在硫磺常量组后插入：

```python
PRECIOUS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Precious.xml"
PRECIOUS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Precious.xml"
PRECIOUS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Precious.xml"

PRECIOUS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}_{suffix}.png"
    for ore in ("SilverOre", "GoldOre")
    for suffix in ("a", "b", "c")
]

PRECIOUS_RESOURCE_EXPECTATIONS = {
    "MMT_SilverOre": {"label": "银矿石", "value": "1.0", "stack": "1800", "tex": "Things/Item/MMT/MMT_Ore/MMT_SilverOre"},
    "MMT_GoldOre": {"label": "金矿石", "value": "25.0", "stack": "300", "tex": "Things/Item/MMT/MMT_Ore/MMT_GoldOre"},
}

PRECIOUS_MINEABLE_EXPECTATIONS = {
    "MMT_MineableSilverOre": {"label": "银矿脉", "yield_thing": "MMT_SilverOre", "hp": "1500"},
    "MMT_MineableGoldOre": {"label": "金矿脉", "yield_thing": "MMT_GoldOre", "hp": "1500"},
}

PRECIOUS_RECIPE_EXPECTATIONS = {
    "MMT_SmeltSilver_Hand": {"work": "750", "ore": "MMT_SilverOre", "ore_count": "14", "output": "Silver", "output_count": "10", "skill": "2"},
    "MMT_SmeltGold_Hand": {"work": "750", "ore": "MMT_GoldOre", "ore_count": "12", "output": "Gold", "output_count": "15", "skill": "3"},
}
```

- [ ] **Step 3: 更新白名单与熔炉配方列表**

- `VANILLA_THINGS` 改为 `{"ComponentIndustrial", "WoodLog", "Steel", "Silver", "Gold"}`。
- `METAL_FURNACE_RECIPES` 追加 `"MMT_SmeltSilver_Hand"`、`"MMT_SmeltGold_Hand"`。

- [ ] **Step 4: 添加金银验证函数并注册**

在 `def validate_all_xml() -> None:` 前插入 `validate_precious()`：

- 资源：按 `PRECIOUS_RESOURCE_EXPECTATIONS` 检查 label/价值/堆叠/texPath；`graphicClass` 为 `Graphic_StackCount`、可燃性 0、`techLevel` Medieval、`ResourcesRaw`、无 stuffProps。
- 矿脉：按 `PRECIOUS_MINEABLE_EXPECTATIONS` 检查（`RockBase`、图集贴图、`mineableThing`/`mineableYield` 75、`isResourceRock`、`veinMineable`）。
- 配方：按 `PRECIOUS_RECIPE_EXPECTATIONS` 检查（输入为矿石单引用、`fixedIngredientFilter` 一致、产物为原版金银、`SmeltingSpeed`/`Smelt`/`Recipe_Smelt`、技能成长 0.8、挂载 `MMT_HandFurnace`、研究前置 `MMT_BasicMetalworking`）。
- PNG 与 `SK.` 扫描。

`SUITES` 中 `"all"` 之前注册 `"precious": validate_precious`。`validate_all()` 在 `validate_metal()` 后插入 `validate_precious()`。

- [ ] **Step 5: RED 验证**

Run:

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py precious
```

Expected: 退出码为 `1`，输出包含 `[FAIL] precious: 缺少文件:`。

- [ ] **Step 6: 提交验证器扩展**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加金银链静态验证" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 金银资源、矿脉、配方与贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Precious.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Precious.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Precious.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_SilverOre_{a,b,c}.png`、`MMT_GoldOre_{a,b,c}.png`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml`（熔炉 recipes 追加 2 项）

**Interfaces:**
- Consumes: Task 1 的 `precious` 套件。
- Produces: 金银链全部 Def；`all` 套件通过。

- [ ] **Step 1: 创建资源 Def**

- `MMT_SilverOre`：`ResourceBase`、`ResourcesRaw`、`Graphic_StackCount`、颜色 (180,173,150)、价值 1.0、堆叠 1800、可燃 0、Medieval、无 stuffProps。
- `MMT_GoldOre`：同上，颜色 (255,215,0)、价值 25.0、堆叠 300。

- [ ] **Step 2: 创建矿脉 Def**

- `MMT_MineableSilverOre`：`RockBase`、图集贴图、color (65,34,34)、colorTwo (180,173,150)、HP 1500、产 `MMT_SilverOre` ×75、常见度 1.0、矿块 10~16、`isResourceRock`、`veinMineable`。
- `MMT_MineableGoldOre`：同上，colorTwo (255,215,0)、产 `MMT_GoldOre` ×75、常见度 0.5、矿块 8~14。

- [ ] **Step 3: 创建配方并挂载熔炉**

- `MMT_SmeltSilver_Hand`：银矿石 14 → `Silver` 10，750，Crafting 2，`SmeltingSpeed`/`Smelt`/`Recipe_Smelt`，研究前置 `MMT_BasicMetalworking`。
- `MMT_SmeltGold_Hand`：金矿石 12 → `Gold` 15，750，Crafting 3，其余同上。
- `MMT_Buildings_Metal.xml` 的 `MMT_HandFurnace` recipes 列表追加两项。

- [ ] **Step 4: 复制贴图**

```bash
for s in a b c; do
  cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Silver/Silver_$s.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_SilverOre_$s.png"
  cp "D:/project/rw/Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Gold/Gold_$s.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_GoldOre_$s.png"
done
```

- [ ] **Step 5: GREEN 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py precious
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal-building
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Precious.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Precious.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Precious.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml 1.7/Textures/Things/Item/MMT/MMT_Ore/
git commit -m "feat: 添加金银矿石、矿脉与冶炼配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

Expected: `precious` 与 `metal-building` 均退出码 `0`。

---

### Task 3: 完整验证与最终提交确认

**Files:**
- 无新增（确认性任务）。

- [ ] **Step 1: 完整静态验证与编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `all` 退出码 `0`，输出含 `precious: OK`，`all-xml: OK (33 files)`；编译 0 警告 0 错误；DLL 存在。

- [ ] **Step 2: 验证最终状态**

```bash
git status --short
git log --oneline --max-count=6
```

Expected: 工作区干净，日志含金银链全部提交。
