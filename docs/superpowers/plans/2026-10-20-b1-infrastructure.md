# B1 基础设施实施计划：独立标签页 + 物品类别树

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 MMT 建立独立研究标签页、独立建筑分类、物品类别树骨架与原版锭类别 patch，作为 3.1/3.2 全部后续批次（B2–B8）的前置。

**Architecture:** 纯 XML Def 改动 + 一个 XPath patch 文件 + validate_mod.py 新验证套件。无 C# 改动，不引用任何 SK 类型或 HSK 程序集。

**Tech Stack:** RimWorld 1.7 XML Def（ResearchTabDef / DesignationCategoryDef / ThingCategoryDef / PatchOperationAdd）、Python 3（tools/validate_mod.py）。

**Spec:** `docs/superpowers/specs/2026-10-20-b1-infrastructure-design.md`

## Global Constraints

- 所有文件 UTF-8 编码；defName 一律 `MMT_` 前缀；label/description 用中文（沿用 HSK 1.6 中文翻译包措辞）。
- 不引用 `SK.` 类型、不依赖 HSK 程序集或 Def；原版类别（`Manufactured`/`Textiles`/`ResourcesRaw` 等）直接引用。
- patch 只允许 `PatchOperationAdd`（追加语义），禁止 `PatchOperationReplace`/`PatchOperationRemove`（spec §3.4）。
- commit message 用中文 conventional commits，结尾附 `Co-Authored-By: Claude Code <noreply@anthropic.com>`；只提交本地，不 push。
- 新增/修改的 `.xml` 自动 `git add`；`tools/validate_mod.py` 是 `.py`，按项目规则 add 前需先询问用户确认。
- 工作区存在用户预先暂存的 35 个文件，**不得**将其混入本计划的任何 commit：commit 一律用 `git commit -m "..." -- <本任务文件清单>` 按路径提交。
- 矿脉 Def（`ParentName="RockBase"`，共 11 个）不是建筑菜单项，**不改动**其 designationCategory。

## Review Focus

1. 后续批次新增研究忘加 `<tab>MMT_ResearchTab</tab>` → 会静默回落到原版 Main 页 → Task 4 套件按目录全量扫描所有研究 def 的 tab 字段（非写死清单，新增 def 自动被覆盖）。
2. 后续批次新建筑忘改分类 → 静默落回原版"生产"页 → Task 4 套件按目录全量扫描所有非矿脉建筑 def。
3. patch xpath 中原版 defName 拼错 → 游戏启动时 patch 报红但静态易漏 → Task 3 步骤 3 对照白名单逐一核对，Task 4 套件断言 8 个目标映射精确相等。
4. 矿脉（RockBase）被误加 designationCategory → 建筑菜单出现异常项 → Task 2 步骤 3 验证 11 个矿脉无该字段，Task 4 套件固化此断言。
5. 类别 `parent` 拼错（尤其 MMT 内部子类引用）→ 游戏加载报红 → Task 4 套件断言每个 parent 在 MMT 类别集合或原版白名单 `{Manufactured, Textiles, ResourcesRaw}` 内。

---

### Task 1: 研究标签页 MMT_ResearchTab

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml`
- Modify: `1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml`（12 个文件，18 个 ResearchProjectDef）

**Interfaces:**
- Consumes: 无
- Produces: `MMT_ResearchTab`（ResearchTabDef defName，Task 4 验证其存在）；全部 18 个研究 def 带 `<tab>MMT_ResearchTab</tab>`（Task 4 全量断言）

- [ ] **Step 1: 创建研究标签页 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ResearchTabDef>
    <defName>MMT_ResearchTab</defName>
    <label>MMT 资源生产</label>
  </ResearchTabDef>
</Defs>
```

- [ ] **Step 2: 给全部研究 def 插入 tab 字段**

在仓库根目录执行（幂等：已含 tab 的文件会跳过；插在 `<techLevel>` 行后，保持同级缩进）：

```bash
python -c "
from pathlib import Path
for f in Path('1.7/Defs/ResearchProjectDefs').glob('MMT_Research_*.xml'):
    text = f.read_text(encoding='utf-8')
    if '<tab>MMT_ResearchTab</tab>' in text:
        continue
    out = []
    for line in text.splitlines():
        out.append(line)
        if '<techLevel>' in line and '</techLevel>' in line:
            indent = line[:len(line) - len(line.lstrip())]
            out.append(f'{indent}<tab>MMT_ResearchTab</tab>')
    f.write_text('\n'.join(out) + '\n', encoding='utf-8')
"
```

- [ ] **Step 3: 验证插入结果与 XML 良构**

```bash
grep -c "<tab>MMT_ResearchTab</tab>" 1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml | awk -F: '{s+=$2} END {print s}'
```

预期输出：`18`（与研究 def 总数一致）。

```bash
python tools/validate_mod.py all-xml
```

预期：`all-xml: OK`（全部 XML 可解析）。

- [ ] **Step 4: Commit**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml 1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml
git commit -m "feat: 添加 MMT 独立研究标签页

Co-Authored-By: Claude Code <noreply@anthropic.com>" -- 1.7/Defs/ResearchProjectDefs/
```

---

### Task 2: 建筑迁入 MMT 独立生产分类

**Files:**
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_*.xml`（11 个文件、15 处 `designationCategory>Production`；AdvancedRocks/Coal/Precious/Sulfur 4 个文件只含矿脉，无匹配、不会被改动）

**Interfaces:**
- Consumes: 无
- Produces: `MMT_Production`（DesignationCategoryDef defName，Task 4 验证）；15 处分类替换（Task 4 全量断言非矿脉建筑均为 MMT_Production、矿脉无此字段）

- [ ] **Step 1: 创建建筑分类 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <DesignationCategoryDef>
    <defName>MMT_Production</defName>
    <label>MMT 生产</label>
    <order>350</order>
  </DesignationCategoryDef>
</Defs>
```

（`order=350` 排在原版"生产"(300) 之后。）

- [ ] **Step 2: 批量替换分类**

```bash
python -c "
from pathlib import Path
old = '<designationCategory>Production</designationCategory>'
new = '<designationCategory>MMT_Production</designationCategory>'
for f in Path('1.7/Defs/ThingDefs_Buildings').glob('MMT_Buildings_*.xml'):
    t = f.read_text(encoding='utf-8')
    if old in t:
        f.write_text(t.replace(old, new), encoding='utf-8')
"
```

- [ ] **Step 3: 验证替换结果（含矿脉不受影响）**

```bash
grep -rc "designationCategory>Production<" 1.7/Defs/ThingDefs_Buildings/ | grep -v ":0" ; echo "剩余 Production: $?"
grep -c "designationCategory>MMT_Production<" 1.7/Defs/ThingDefs_Buildings/*.xml | awk -F: '{s+=$2} END {print s}'
python -c "
import xml.etree.ElementTree as ET, glob
n = 0
for f in glob.glob('1.7/Defs/ThingDefs_Buildings/MMT_Buildings_*.xml'):
    for d in ET.parse(f).getroot().findall('ThingDef'):
        if d.get('ParentName') == 'RockBase':
            assert d.findtext('designationCategory') is None, f'矿脉被误加分类: {d.findtext(\"defName\")}'
            n += 1
print(f'矿脉检查通过: {n}')
"
```

预期：第一条无输出（无残留 `Production`）；第二条输出 `15`；第三条输出 `矿脉检查通过: 11`。

```bash
python tools/validate_mod.py all-xml
```

预期：`all-xml: OK`。

- [ ] **Step 4: Commit**

```bash
git add 1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_*.xml
git commit -m "feat: 建筑迁入 MMT 独立生产分类

Co-Authored-By: Claude Code <noreply@anthropic.com>" -- 1.7/Defs/ThingDefs_Buildings/
```

---

### Task 3: 物品类别树 + 原版锭类别 patch

**Files:**
- Create: `1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml`
- Create: `1.7/Patches/MMT_Patch_VanillaCategories.xml`

**Interfaces:**
- Consumes: 原版类别 `Manufactured`、`Textiles`、`ResourcesRaw`；原版物品 defName（Steel/Plasteel/Gold/Silver/Uranium/ComponentIndustrial/ComponentSpacer/ComponentAdvanced）
- Produces: 17 个类别 defName（`MMT_MetallicBar`、`MMT_HCMBar`、`MMT_PRSBar`、`MMT_SLDBar`、`MMT_HeavyBar`、`MMT_USLDBar`、`MMT_USLDHBar`、`MMT_RARBar`、`MMT_RADBar`、`MMT_Coal`、`MMT_FuelCat`、`MMT_Parts`、`MMT_WeaponParts`、`MMT_ElectronicsCat`、`MMT_GlassCat`、`MMT_SuperMaterialCat`、`MMT_BTextiles`）——后续批次配方 filter 与资源归属引用；8 条 patch（原版物品 → MMT 类别映射，Task 4 断言精确相等）

- [ ] **Step 1: 创建类别树骨架**

创建 `1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml`（17 个类别；锭类 8 子类 parent 为 `MMT_MetallicBar`，层级对照 HSK ThingCategories_SK.xml，中文 label 沿用 HSK 1.6 翻译包）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 锭类聚合根：对应 HSK Metallic(锭) -->
  <ThingCategoryDef>
    <defName>MMT_MetallicBar</defName>
    <label>金属锭类</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_HCMBar</defName>
    <label>高传导率类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_PRSBar</defName>
    <label>贵重类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_SLDBar</defName>
    <label>硬质类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_HeavyBar</defName>
    <label>重质类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_USLDBar</defName>
    <label>高硬质类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_USLDHBar</defName>
    <label>高硬质超重类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_RARBar</defName>
    <label>稀有类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_RADBar</defName>
    <label>放射性类</label>
    <parent>MMT_MetallicBar</parent>
  </ThingCategoryDef>
  <!-- 煤类：对应 HSK Coal（HSK parent 为 HSK 自有类别 Extracted，MMT 改用原版 ResourcesRaw） -->
  <ThingCategoryDef>
    <defName>MMT_Coal</defName>
    <label>煤类</label>
    <parent>ResourcesRaw</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_FuelCat</defName>
    <label>燃料</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_Parts</defName>
    <label>部件和机械结构</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_WeaponParts</defName>
    <label>武器部件</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_ElectronicsCat</defName>
    <label>电子</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_GlassCat</defName>
    <label>玻璃制品</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_SuperMaterialCat</defName>
    <label>超材料</label>
    <parent>Manufactured</parent>
  </ThingCategoryDef>
  <ThingCategoryDef>
    <defName>MMT_BTextiles</defName>
    <label>基础织物</label>
    <parent>Textiles</parent>
  </ThingCategoryDef>
</Defs>
```

- [ ] **Step 2: 创建原版物品类别 patch**

创建 `1.7/Patches/MMT_Patch_VanillaCategories.xml`（RimWorld 自动加载已挂载版本目录下的 `Patches/`；追加语义，保留原版 `Manufactured` 归属）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 原版锭/零部件追加 MMT 类别，使类别过滤配方能选中原版物品；对照 HSK 对 Steel→SLDBar 的同类归属 -->
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="Steel"]/thingCategories</xpath>
    <value><li>MMT_SLDBar</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="Plasteel"]/thingCategories</xpath>
    <value><li>MMT_USLDBar</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="Gold"]/thingCategories</xpath>
    <value><li>MMT_PRSBar</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="Silver"]/thingCategories</xpath>
    <value><li>MMT_PRSBar</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="Uranium"]/thingCategories</xpath>
    <value><li>MMT_RADBar</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="ComponentIndustrial"]/thingCategories</xpath>
    <value><li>MMT_Parts</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="ComponentSpacer"]/thingCategories</xpath>
    <value><li>MMT_Parts</li></value>
  </Operation>
  <Operation Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName="ComponentAdvanced"]/thingCategories</xpath>
    <value><li>MMT_Parts</li></value>
  </Operation>
</Patch>
```

- [ ] **Step 3: 验证**

```bash
python -c "
import xml.etree.ElementTree as ET
cats = ET.parse('1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml').getroot()
names = [d.findtext('defName') for d in cats.findall('ThingCategoryDef')]
assert len(names) == 17, f'类别数 {len(names)} != 17'
valid = set(names) | {'Manufactured', 'Textiles', 'ResourcesRaw'}
for d in cats.findall('ThingCategoryDef'):
    assert d.findtext('parent') in valid, f'{d.findtext(\"defName\")} parent 无法解析: {d.findtext(\"parent\")}'
patch = ET.parse('1.7/Patches/MMT_Patch_VanillaCategories.xml').getroot()
ops = patch.findall('Operation')
assert len(ops) == 8, f'patch 操作数 {len(ops)} != 8'
for op in ops:
    assert op.get('Class') == 'PatchOperationAdd', f'非法操作类型: {op.get(\"Class\")}'
    assert op.findtext('value/li') in set(names), f'patch 引用未知类别: {op.findtext(\"value/li\")}'
print('类别树与 patch: OK')
"
python tools/validate_mod.py all-xml
```

预期：输出 `类别树与 patch: OK` 与 `all-xml: OK`。

- [ ] **Step 4: Commit**

```bash
git add 1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml 1.7/Patches/MMT_Patch_VanillaCategories.xml
git commit -m "feat: 添加物品类别树与原版锭类别 patch

Co-Authored-By: Claude Code <noreply@anthropic.com>" -- 1.7/Defs/ThingCategoryDefs/ 1.7/Patches/
```

---

### Task 4: 验证套件 infrastructure + 统一编译验证

**Files:**
- Modify: `tools/validate_mod.py`（头部常量区、新增 `validate_infrastructure()`、`validate_all()`、`SUITES` 四处）

**Interfaces:**
- Consumes: Task 1 的 `MMT_ResearchTab` 与 18 个研究 tab 字段；Task 2 的 `MMT_Production` 与 15 处分类；Task 3 的 17 个类别与 8 条 patch；validate_mod.py 既有辅助函数 `require_files`、`parse_xml`、`def_map`、`child_text`、`assert_equal`、`fail`、`read_text`、`ROOT`
- Produces: 套件名 `infrastructure`（`python tools/validate_mod.py infrastructure`）；`validate_all()` 内含 `validate_infrastructure()` 调用

- [ ] **Step 1: 头部常量区追加**

在 `tools/validate_mod.py` 路径常量区末尾（最后一个 `_XML` 常量之后）追加：

```python
INFRA_RESEARCH_TAB_XML = "1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml"
INFRA_DESIGNATION_XML = "1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml"
INFRA_CATEGORIES_XML = "1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml"
INFRA_PATCH_XML = "1.7/Patches/MMT_Patch_VanillaCategories.xml"
INFRA_RESEARCH_DIR = "1.7/Defs/ResearchProjectDefs"
INFRA_BUILDING_DIR = "1.7/Defs/ThingDefs_Buildings"
INFRA_RESEARCH_TOTAL = 18
INFRA_WORKTABLE_TOTAL = 15
INFRA_VANILLA_PARENTS = {"Manufactured", "Textiles", "ResourcesRaw"}
INFRA_EXPECTED_CATEGORIES = {
    "MMT_MetallicBar": "Manufactured",
    "MMT_HCMBar": "MMT_MetallicBar",
    "MMT_PRSBar": "MMT_MetallicBar",
    "MMT_SLDBar": "MMT_MetallicBar",
    "MMT_HeavyBar": "MMT_MetallicBar",
    "MMT_USLDBar": "MMT_MetallicBar",
    "MMT_USLDHBar": "MMT_MetallicBar",
    "MMT_RARBar": "MMT_MetallicBar",
    "MMT_RADBar": "MMT_MetallicBar",
    "MMT_Coal": "ResourcesRaw",
    "MMT_FuelCat": "Manufactured",
    "MMT_Parts": "Manufactured",
    "MMT_WeaponParts": "Manufactured",
    "MMT_ElectronicsCat": "Manufactured",
    "MMT_GlassCat": "Manufactured",
    "MMT_SuperMaterialCat": "Manufactured",
    "MMT_BTextiles": "Textiles",
}
INFRA_PATCH_TARGETS = {
    "Steel": "MMT_SLDBar",
    "Plasteel": "MMT_USLDBar",
    "Gold": "MMT_PRSBar",
    "Silver": "MMT_PRSBar",
    "Uranium": "MMT_RADBar",
    "ComponentIndustrial": "MMT_Parts",
    "ComponentSpacer": "MMT_Parts",
    "ComponentAdvanced": "MMT_Parts",
}
```

- [ ] **Step 2: 新增 validate_infrastructure()**

在 `validate_all_xml()` 之前插入：

```python
def validate_infrastructure() -> None:
    require_files([INFRA_RESEARCH_TAB_XML, INFRA_DESIGNATION_XML, INFRA_CATEGORIES_XML, INFRA_PATCH_XML])

    tab = parse_xml(INFRA_RESEARCH_TAB_XML)
    assert_equal(child_text(tab, "defName"), "MMT_ResearchTab", "研究标签 defName")
    assert_equal(child_text(tab, "label"), "MMT 资源生产", "研究标签 label")

    research_count = 0
    for path in sorted((ROOT / INFRA_RESEARCH_DIR).glob("MMT_Research_*.xml")):
        rel = path.relative_to(ROOT).as_posix()
        for node in parse_xml(rel).findall("ResearchProjectDef"):
            research_count += 1
            assert_equal(child_text(node, "tab"), "MMT_ResearchTab", f"{path.name} 研究 {child_text(node, 'defName')} tab")
    assert_equal(research_count, INFRA_RESEARCH_TOTAL, "研究项目总数")

    designation = parse_xml(INFRA_DESIGNATION_XML)
    assert_equal(child_text(designation, "defName"), "MMT_Production", "建筑分类 defName")
    assert_equal(child_text(designation, "label"), "MMT 生产", "建筑分类 label")

    worktable_count = 0
    for path in sorted((ROOT / INFRA_BUILDING_DIR).glob("MMT_Buildings_*.xml")):
        rel = path.relative_to(ROOT).as_posix()
        for node in parse_xml(rel).findall("ThingDef"):
            if node.findtext("defName") is None:
                continue
            if node.get("ParentName") == "RockBase":
                if node.findtext("designationCategory") is not None:
                    fail(f"{rel} 矿脉 {node.findtext('defName')} 不应有 designationCategory")
                continue
            worktable_count += 1
            assert_equal(child_text(node, "designationCategory"), "MMT_Production", f"{rel} {node.findtext('defName')} designationCategory")
    assert_equal(worktable_count, INFRA_WORKTABLE_TOTAL, "工作台建筑总数")

    categories = def_map(parse_xml(INFRA_CATEGORIES_XML))
    assert_equal(set(categories), set(INFRA_EXPECTED_CATEGORIES), "类别 defName 集合")
    for name, expected_parent in INFRA_EXPECTED_CATEGORIES.items():
        assert_equal(child_text(categories[name], "parent"), expected_parent, f"{name} parent")

    patch_text = read_text(INFRA_PATCH_XML)
    if "PatchOperationReplace" in patch_text or "PatchOperationRemove" in patch_text:
        fail("类别 patch 只允许 PatchOperationAdd（追加语义）")
    patch = parse_xml(INFRA_PATCH_XML)
    ops = patch.findall("Operation")
    seen = {}
    for op in ops:
        assert_equal(op.attrib.get("Class"), "PatchOperationAdd", "patch 操作类型")
        xpath = child_text(op, "xpath")
        target = xpath.split('defName="')[1].split('"')[0]
        value = child_text(op, "value/li")
        if value not in INFRA_EXPECTED_CATEGORIES:
            fail(f"patch 引用了不存在的类别: {value}")
        seen[target] = value
    assert_equal(seen, INFRA_PATCH_TARGETS, "patch 目标与类别映射")
    print("infrastructure: OK")
```

- [ ] **Step 3: 注册套件并接入 validate_all()**

`validate_all()` 中在 `validate_base()` 之后插入一行：

```python
    validate_infrastructure()
```

`SUITES` 字典中插入一行（保持按字典序排列习惯，放在 `"coal"` 之前）：

```python
    "infrastructure": validate_infrastructure,
```

- [ ] **Step 4: 运行验证**

```bash
python tools/validate_mod.py infrastructure
python tools/validate_mod.py all
```

预期：`infrastructure: OK`，随后全量各套件均 OK。

- [ ] **Step 5: 统一编译验证（项目规则：所有任务完成后统一编译）**

本次无 C# 改动，仍按项目规则执行一次离线编译确认：

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：`已成功生成`（0 错误）。

- [ ] **Step 6: Commit**

`tools/validate_mod.py` 为 `.py` 文件，按项目规则**先询问用户确认后再 add**：

```bash
git add tools/validate_mod.py
git commit -m "test: 添加基础设施静态验证套件

Co-Authored-By: Claude Code <noreply@anthropic.com>" -- tools/validate_mod.py
```

---

## Self-Review 记录

- **Spec 覆盖**：spec §3.1→Task 1；§3.2→Task 2；§3.3→Task 3 Step 1；§3.4→Task 3 Step 2；§3.5→Task 4；§4 commit 切分→Task 1–4 各自末步。无缺口。
- **Placeholder 扫描**：无 TBD/TODO；所有 XML/Python 内容完整给出。
- **类型一致性**：17 个类别 defName、8 个 patch 目标、`MMT_ResearchTab`/`MMT_Production` 在 Task 1–4 与常量区完全一致；类别数修正为 17（与 spec 表格行数一致）。
- **Review Focus**：5 条均已落到对应 Task 的验证步骤（见各条尾部说明）。
