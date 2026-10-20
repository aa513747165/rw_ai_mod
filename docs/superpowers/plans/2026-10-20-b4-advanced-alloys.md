# B4 高级合金（EAF）实施计划 / Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:subagent-driven-development (recommended) or superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax.

**Goal:** 补齐 EAF 剩余合金（贫铀/斯坦/梅尔基奥/亚力可）+ 硅批量配方 + 既有 8 合金类别补齐，数值全对齐 HSK。
**Architecture:** 纯 XML Def，无 C# 改动；配方挂 EAF 建筑内联 `<recipes>` 列表（不用 recipeUsers）；贴图复用 `Things/Item/MMT/MMT_Ingot/MMT_Ingot` + color 着色。
**Tech Stack:** RimWorld 1.7 XML Def、Python 3（tools/validate_mod.py）、Node（tools/build_mmt_chains_compare.js）。
**Spec:** `docs/superpowers/specs/2026-10-20-b4-advanced-alloys-design.md`（数值表与决策以 spec 为准）

---

## Global Constraints

- 所有 defName 加 `MMT_` 前缀；label/description 中文写死；文件 UTF-8
- 禁止出现 `SK.` 引用；stuff categories 仅 `Metallic`（禁 SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic）
- 禁止引入 SK 专有 StatDef（MeleePenetrationFactor/WorkTableWorkSpeedFactor 等）
- 配方不写 recipeUsers，挂 `MMT_ElectricArcFurnace` 的 `<recipes>` 列表
- 中文 conventional commits + `Co-Authored-By: Claude Code <noreply@anthropic.com>`；只提交本地；每 commit 前 `git status --short` 核对 WIP（CLAUDE.md 与两个 tools 脚本是用户 WIP，不纳入本批提交）
- 验证命令乱码时前缀 `PYTHONIOENCODING=utf-8`
- `Hardcore-SK/` 只读

## Review Focus

1. 类别断言与 Def 必须同 commit 更新（validate_mod.py 旧断言硬编码 `["ResourcesRaw"]`）→ Task 1
2. SK 专有 stat/stuff 类别禁入 → Task 1
3. EAF 配方列表断言是有序精确匹配（`ELECTRIC_FURNACE_RECIPES + ADVANCED_ALLOYS_FURNACE_RECIPES`），XML 追加顺序必须与常量一致 → Task 2
4. `VANILLA_THINGS` 需补 `Uranium`，否则贫铀配方输入校验失败 → Task 2
5. 硅批量配方放 `MMT_Recipes_Electric.xml`（electric 套件），不放高级合金文件——其资源名集合不含沙/硅 → Task 2

---

### Task 1: 新增 4 合金资源 + 既有 8 合金类别补齐

**Files:**
- Modify: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml`
- Modify: `tools/validate_mod.py`（ADVANCED_ALLOY_EXPECTATIONS + 类别断言行 3041）

**Interfaces:**
- Produces: `MMT_DepletedUranium` / `MMT_SteelBar` / `MMT_CupronickelAlloy` / `MMT_AlnicoAlloy`

- [ ] **Step 1: 追加 4 个 ThingDef**（字段值见 spec §4 表；统一模式同既有 8 个：ResourceBase、MMT_Ingot 贴图、Graphic_StackCount、Metal_Drop、tickerType Rare、useHitPoints true、terrainAffordanceNeeded Medium、stuffProps 仅 Metallic + statFactors）

- [ ] **Step 2: 既有 8 合金改 thingCategories**（spec 决策 1 表）：镍/钴/铬→`MMT_RARBar`；钨→`MMT_RARBar`+`MMT_USLDHBar`；钛→`MMT_RARBar`+`MMT_USLDBar`；司太立/镍钛诺→`MMT_USLDBar`；钴硬质→`MMT_USLDBar`+`MMT_USLDHBar`

- [ ] **Step 3: 同步 validate**：`ADVANCED_ALLOY_EXPECTATIONS` 12 条各加 `"categories": [...]`（含 4 条新资源的完整期望）；`validate_advanced_alloys` 中 `assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], ...)`（约 3041 行）改为断言 `expected["categories"]`

- [ ] **Step 4: 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py advanced-alloys
```

预期：`advanced-alloys: OK`

- [ ] **Step 5: Commit**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml tools/validate_mod.py
git status --short   # 确认未带入 CLAUDE.md / tools/build_*.js
git commit -m "feat: 添加贫铀斯坦梅尔基奥亚力可合金资源并补齐合金类别"
```

### Task 2: 新增 5 条配方 + EAF 挂载

**Files:**
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml`（4 条合金配方）
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml`（`MMT_MakeSilicon_Bulk`）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml`（EAF `<recipes>` 追加 5 条）
- Modify: `tools/validate_mod.py`（VANILLA_THINGS、ADVANCED_ALLOYS_FURNACE_RECIPES、ADVANCED_ALLOYS_RECIPE_EXPECTATIONS、ELECTRIC_FURNACE_RECIPES、ELECTRIC_RECIPE_EXPECTATIONS）

**Interfaces:**
- Consumes: `Uranium`、`Steel`、`MMT_CopperBar`、`MMT_AluminiumBar`、`MMT_NickelBar`、`MMT_Cobalt`、`MMT_Sand`
- Produces: 4 新合金 + `MMT_Silicon`

- [ ] **Step 1: 追加 4 条合金配方**（数值见 spec §2，defName/研究映射见 spec 决策 2；模式同既有 9 条：SmeltingSpeed/Smelt/Recipe_Smelt/Crafting/workSkillLearnFactor 0.8、filter+fixedIngredientFilter 双写）

- [ ] **Step 2: 追加硅批量配方**到 `MMT_Recipes_Electric.xml`：`MMT_MakeSilicon_Bulk`，MMT_Sand×200→MMT_Silicon×20，workAmount 1800，Crafting 5，研究 `MMT_ElectricSmelting`

- [ ] **Step 3: EAF `<recipes>` 追加**：在 `MMT_ExtractSilicon_Electric` 后插 `MMT_MakeSilicon_Bulk`（与 ELECTRIC_FURNACE_RECIPES 顺序一致）；在合金段末尾（`MMT_MakeFerrotitanium` 后）追加 `MMT_MakeCupronickelAlloy`、`MMT_MakeDepletedUranium`、`MMT_MakeSteelBar`、`MMT_MakeAlnicoAlloy`（顺序与 ADVANCED_ALLOYS_FURNACE_RECIPES 一致）

- [ ] **Step 4: 同步 validate**：`VANILLA_THINGS` 加 `"Uranium"`；两个 FURNACE_RECIPES 列表与 RECIPE_EXPECTATIONS 按同序补 5 条（合金配方断言含 research 键，硅批量走 electric 套件现有断言结构）

- [ ] **Step 5: 验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py advanced-alloys
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric
```

预期：两个 `OK`

- [ ] **Step 6: Commit**

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml tools/validate_mod.py
git status --short
git commit -m "feat: 添加 EAF 贫铀斯坦梅尔基奥亚力可与硅批量配方"
```

### Task 3: 全量验证 + 对比文档

**Files:**
- Modify: `docs/mmt-vs-hsk-resource-chains.md`（生成物）

- [ ] **Step 1: 全量验证 + 编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：全部套件 OK；0 错误

- [ ] **Step 2: 重跑对比文档**

```bash
node tools/build_mmt_chains_compare.js
```

预期：`docs/mmt-vs-hsk-resource-chains.md` 更新；4 种新合金显示已实现（MMT_ALIAS 无需改，名称兜底匹配）

- [ ] **Step 3: Commit**

```bash
git add docs/mmt-vs-hsk-resource-chains.md
git status --short
git commit -m "docs: 重跑 MMT 与 HSK 资源链对比文档"
```

## Self-Review 记录

- Spec 覆盖：路线图 B4 表全部条目（贫铀/斯坦/梅尔基奥/亚力可/硅批量/类别补齐）→ Task 1/2
- 决策落点：类别以 HSK 实际归属为准（路线图授权 spec 核对）；硅批量采纳并入 electric 套件；不新增研究；MMT_ALIAS 不改
- Placeholder 扫描：所有数值来自 HSK 实地核对，无 TBD
- 类型一致：无 C# 改动；stuff/stat 仅用原版存在项
