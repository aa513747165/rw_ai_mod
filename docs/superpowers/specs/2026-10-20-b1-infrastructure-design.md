# B1 基础设施设计：独立标签页 + 物品类别树

- 日期：2026-10-20
- 状态：待用户审阅
- 所属方案：3.1/3.2 按方案 A（按资源链分批）的第 1 批，所有后续批次的前置

## 1. 背景与已确认决策

目标：把 `docs/hsk-resource-production-chains.md` 的 3.1 原材料（46 条）与 3.2 加工品（135 条）按 HSK 方式在 MMT 中实现。可行性分析结论：约 85% 可纯 XML 等价实现，其余经用户决策处理。已确认决策：

1. **DLC 直接依赖**：游戏 DLC 齐全，Anomaly（活铁/活铁塑造台）、Odyssey（逆重科技）内容直接引用，不做 `MayRequire` 条件加载。
2. **去 CE**：炮塔组件箱链（25 条 `*_Crate` 配方）整体砍掉；`AmmoBench` 挂载点改为 MMT 自建弹药台；`Prometheum`/`FSX` 等 CE 物品不使用。
3. **独立标签页**：MMT 研究项目放独立研究标签页；MMT 建筑放独立建筑分类。
4. **植物系**：Patch 原版存在的植物（棉花→原棉、恶魔菇→生恶魔菇）；原版不存在的（亚麻/竹/红木/椰子/橡胶树/野玫瑰/金丝桃/薄荷）由 MMT 自建作物（B8 批次范围）。
5. **反物质**：MMT 自建粒子对撞台 + 反物质生产配方（B6 批次范围）。
6. **实施方式**：方案 A，按资源链分 8 批（B1–B8），每批 设计→spec→计划→实施→验证→commit。

## 2. 本批范围（B1）

只做结构件，不做任何资源/配方/生产建筑：

- MMT 研究标签页（ResearchTabDef）
- MMT 建筑独立分类（DesignationCategoryDef）
- MMT 物品类别树（ThingCategoryDef 骨架）+ 原版锭类别归属 patch
- validate_mod.py 新增 `infrastructure` 套件

## 3. 详细设计

### 3.1 研究标签页

新文件 `1.7/Defs/ResearchProjectDefs/MMT_ResearchTab.xml`：

```xml
<ResearchTabDef>
  <defName>MMT_ResearchTab</defName>
  <label>MMT 资源生产</label>
</ResearchTabDef>
```

- 现有 12 个研究文件（`1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml`）中全部 `MMT_` 研究项目补 `<tab>MMT_ResearchTab</tab>`。
- `researchViewX/Y` 坐标保留不动（页内坐标，换页不影响布局）。
- 后续批次新增研究一律进此页。

### 3.2 建筑独立分类

新文件 `1.7/Defs/ThingDefs_Buildings/MMT_DesignationCategory.xml`：

```xml
<DesignationCategoryDef>
  <defName>MMT_Production</defName>
  <label>MMT 生产</label>
  <order>350</order>
</DesignationCategoryDef>
```

- 现有 15 个建筑文件（`1.7/Defs/ThingDefs_Buildings/MMT_Buildings_*.xml`）中 `<designationCategory>Production</designationCategory>` 全部改为 `MMT_Production`。
- 后续新建筑一律进此分类。

### 3.3 物品类别树

新文件 `1.7/Defs/ThingCategoryDefs/MMT_ThingCategories.xml`。

**原则**：原版已有的类别直接引用原版（`ResourcesRaw`、`Foods`、`Corpses`、`MeatRaw`、`AnimalProductRaw`、`PlantFoodRaw`、`Textiles`、`Leathers`、`StoneChunks`、`Woody` 等），不重复造；HSK 有而原版没有的，建 `MMT_` 前缀类别，中文 label 沿用 HSK 1.6 中文翻译包。

B1 只建类别骨架（不含成员），各批次实施时把 MMT 资源挂入对应类别：

| MMT defName | 中文 label | 对应 HSK | 服务的配方输入 |
|---|---|---|---|
| `MMT_MetallicBar` | 金属锭类 | Metallic（锭类聚合） | [金属锭类]（金属罐、钢筋混凝土、武器部件） |
| `MMT_HCMBar` | 高传导率类 | HCMBar | [高碳金属锭类]（电线、电子元件、纯金/纯银合金） |
| `MMT_PRSBar` | 贵重类 | PRSBar | 金/银锭归类 |
| `MMT_SLDBar` | 硬质类 | SLDBar | [轻合金锭类]（零部件、机械组件） |
| `MMT_HeavyBar` | 重质类 | HeavyBar | 铅/钨等重锭归类 |
| `MMT_USLDBar` | 高硬质类 | USLDBar | [超轻合金锭类]（先进零部件、机器人部件） |
| `MMT_USLDHBar` | 高硬质超重类 | USLDHBar | [超硬/耐压合金锭类]（超级/高级零部件） |
| `MMT_RARBar` | 稀有类 | RARBar | [稀有合金锭类]（超级零部件） |
| `MMT_RADBar` | 放射性类 | RADBar | 贫铀合金归类 |
| `MMT_Coal` | 煤类 | Coal | [煤类]（炼钢、碳聚物、火药） |
| `MMT_FuelCat` | 燃料 | FuelCat | 燃料归类（石化批次使用） |
| `MMT_Parts` | 部件和机械结构 | Parts | 零部件/机械组件归类 |
| `MMT_WeaponParts` | 武器部件 | WeaponParts | 各武器部件归类（B8） |
| `MMT_ElectronicsCat` | 电子 | ElectronicsCat | 集成电路/微芯片归类（B7） |
| `MMT_GlassCat` | 玻璃制品 | GlassCat | 玻璃块/强化玻璃归类 |
| `MMT_SuperMaterialCat` | 超材料 | SuperMaterialCat | α/β聚合物归类（B6） |
| `MMT_BTextiles` | 基础织物 | BTextiles | [基础织物]（丝绒，B8） |

层级：`MMT_HCMBar` 等 8 个锭子类的 parent 为 `MMT_MetallicBar`，`MMT_MetallicBar` 的 parent 为原版 `Manufactured`；其余类别 parent 为原版 `Resources` 或 `Manufactured`（spec 实施时逐一对照 HSK parent 确定）。

**注意**：文档中方括号译名（如 [高碳金属锭类]）与 HSK 实际中文 label（高传导率类）存在出入，实施配方时以 HSK RecipeDef 中实际引用的类别 defName 为准，本表 MMT 类别一一对应 HSK defName。

### 3.4 原版物品类别归属 patch

新文件 `1.7/Patches/MMT_Patch_VanillaCategories.xml`。用 `PatchOperationAdd` **追加**类别（不替换，保留原版 `Manufactured` 归属，避免影响原版配方/筛选）：

| 原版物品 | 追加类别 | 依据（HSK 同类 patch） |
|---|---|---|
| `Steel` | `MMT_SLDBar` | HSK 将 Steel 归入 SLDBar |
| `Plasteel` | `MMT_USLDBar` | HSK 高硬质体系 |
| `Gold` | `MMT_PRSBar` | 贵重类 |
| `Silver` | `MMT_PRSBar` | 贵重类 |
| `Uranium` | `MMT_RADBar` | 放射性类 |
| `ComponentIndustrial` | `MMT_Parts` | 部件类 |
| `ComponentSpacer` | `MMT_Parts` | 部件类 |
| `ComponentAdvanced` | `MMT_Parts` | 部件类 |

说明：HSK 用的是自定义 `SK.PatchOperationReplaceExtended`（DeepReplace 整体替换），MMT 不可用该类，且追加语义更安全，故改用原版 `PatchOperationAdd`。

### 3.5 验证

`tools/validate_mod.py` 新增 `validate_infrastructure()`，注册套件 `infrastructure`：

1. 所有 `MMT_` 前缀 ResearchProjectDef 含 `<tab>MMT_ResearchTab</tab>`；
2. 所有 `MMT_` 前缀建筑 ThingDef 的 `designationCategory` 为 `MMT_Production`；
3. `MMT_ThingCategories.xml` 中所有 parent 引用可解析（原版或 MMT 类别）；
4. patch 文件中每个 xpath 目标 defName 在原版 Def 中存在（静态检查 patch 目标列表非空且为已知原版 defName）。

## 4. commit 切分

1. `feat: 添加 MMT 独立研究标签页`（3.1）
2. `feat: 建筑迁入 MMT 独立生产分类`（3.2）
3. `feat: 添加物品类别树与原版锭类别 patch`（3.3 + 3.4）
4. `test: 添加基础设施静态验证套件`（3.5）

每步完成后可编译/验证通过再进入下一步；统一编译验证按项目规则执行。

## 5. 非目标（Out of Scope）

- 任何资源 ThingDef、配方 RecipeDef、生产建筑的实质新增（后续批次）；
- 植物 patch、作物、炮塔、CE 相关内容；
- 研究树坐标重排优化（保留现状）。
