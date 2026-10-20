# B2 基础冶金 · 设计 Spec

- 日期：2026-10-20
- 状态：待用户审阅
- 引用：`docs/superpowers/specs/2026-10-20-b2-b8-batch-roadmap.md` §2（全局约定见该文 §0，此处不重复）

## 1. 目标与范围

补齐电熔炼/熔炉/窑炉体系的基础金属与烧制品，数值全部对齐 HSK。新增 10 个资源、3 条矿脉、1 座窑炉、9 条新配方（含篝火火化）、1 组灰烬 patch、类别补齐，不新增研究。

**已确认的 5 项决策**（brainstorming 结论）：

1. 软粘土来源 = 新建粘土矿脉 `MMT_MineableSoftClay`（HSK 的 SK 程序集 Designator 路线不可用）。
2. 新建窑炉 `MMT_Kiln`（燃料型，原版 `CompProperties_Refuelable` 替代 SK 燃料 comp）。
3. 灰烬 = patch 原版火化配方追加产出 + 新建 `MMT_BurnThings`、`MMT_CremateCorpseCampfire`。
4. 研究并入现有：手工配方→`MMT_BasicMetalworking`，电配方→`MMT_ElectricSmelting`。
5. 工作区现有 WIP 保留，B2 在其基础上追加编辑，提交只含 B2 内容。

**勘察修正点**（与路线图原文的差异）：

- 铝锭类别：HSK 实际归 `HCMBar`（高传导率类），路线图表格误写 SLDBar → **已确认归 `MMT_HCMBar`**。
- 原版 1.7 无 `CremateCorpseCampfire` 配方（系 HSK 自建）→ MMT 新建同名功能配方挂原版篝火。
- 原版火化配方（BurnApparel/CremateCorpse）无 `<products>` 节点 → patch 用条件序列先建节点再插入。
- MMT 无电熔炼器，HSK 挂 ElectricSmelter 的配方一律挂 `MMT_ElectricArcFurnace`（现有模式）。
- 灰烬 patch 只追加产出，**不改**原版配方的 workAmount 等其他字段（HSK patch 改了工作量，MMT 保持原版手感）。

## 2. 新资源 ThingDef（10 个）

### 2.1 追加到 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml`

| defName | label | MarketValue | Mass | stackLimit | thingCategories | stuff |
|---|---|---|---|---|---|---|
| `MMT_Anglesite` | 铅矾矿 | 4 | 0.3 | 300 | ResourcesRaw | 否 |
| `MMT_AluminiumOre` | 铝土矿 | 2 | 0.1 | 600 | ResourcesRaw | 否 |
| `MMT_LeadBar` | 铅合金 | 6 | 0.4 | 300 | **MMT_HeavyBar** | 是 |
| `MMT_AluminiumBar` | 铝合金 | 3 | 0.1 | 300 | **MMT_HCMBar** | 是 |
| `MMT_FerrosiliconAlloy` | 斯特合金 | 5 | 0.3 | 300 | MMT_SLDBar | 是 |
| `MMT_MetalCan` | 金属罐 | 1.7 | 0.1 | 200 | Manufactured | 否 |

- 锭类 stuffProps：statFactors/statOffsets 全数值照抄 HSK（LeadBar/AluminiumBar/FerrosiliconAlloy 对应条目），但 stuff categories 只用原版 `Metallic`（HSK 的 HeavyMetallic/StrongMetallic/Sterile/RuggedMetallic 为 SK 自定义类别，MMT 不复制；与现有铜/锡/青铜同模式）。`stuffAdjective` 用中文（铅制/铝制/斯特制）。`terrainAffordanceNeeded` = Medium。
- 矿石不带 deep* 深钻井属性（采集侧非目标，见路线图 §9）。
- 金属罐：HSK 还有 `useHitPoints` 与 SK 排除 modExtension；MMT 保留 useHitPoints，跳过 modExtension。

### 2.2 新建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml`

| defName | label | MarketValue | Mass | stackLimit | thingCategories | 备注 |
|---|---|---|---|---|---|---|
| `MMT_SoftClay` | 软粘土 | 1 | 0.15 | 600 | ResourcesRaw | 非 stuff |
| `MMT_ClayBrick` | 粘土砖 | 3 | 0.5 | 300 | Manufactured | stuff，category=MMT_Bricks |
| `MMT_Ceramics` | 陶瓷 | 12 | 0.4 | 300 | Manufactured | stuff，category=Stony |
| `MMT_Ash` | 灰烬 | 0.3 | 0.05 | 600 | ResourcesRaw | Beauty -5，tradeability None |

- 粘土砖/陶瓷 stuffProps 数值照抄 HSK（ClayBrick/Ceramics 条目）；粘土砖 stuff category 用新建的 `MMT_Bricks`（见 §6），陶瓷用原版 `Stony`。
- 灰烬 tradeability=None、Beauty=-5 对齐 HSK。

### 2.3 贴图

- 锭：沿用 `Things/Item/MMT/MMT_Ingot` 调色（颜色取 HSK 对应 graphicData.color）。
- 矿石：`Things/Item/MMT/MMT_Ore/MMT_<名>/` 多状态目录模式（_a/_b/_c），复制 HSK 对应矿石贴图（Anglesite/Bauxite）。
- 软粘土/粘土砖/陶瓷/灰烬/金属罐：复制 HSK 授权贴图（Things/Item/Resource/ 下 SoftClay/ClayBricks/Ceramics/Ash/MetalCan）到 `1.7/Textures/Things/Item/MMT/` 相应目录。
- HSK 无可复制贴图的，用现有 MMT 贴图调色兜底。

## 3. 矿脉（3 条）

| defName | label | mineableThing | yield | commonality | lump | 文件 |
|---|---|---|---|---|---|---|
| `MMT_MineableAnglesite` | 铅矾矿脉 | MMT_Anglesite | 75 | 0.8 | 6~13 | MMT_Buildings_Metal.xml |
| `MMT_MineableAluminium` | 铝土矿脉 | MMT_AluminiumOre | 75 | 0.65 | 11~19 | MMT_Buildings_Metal.xml |
| `MMT_MineableSoftClay` | 粘土矿脉 | MMT_SoftClay | 75 | 0.7 | 8~15 | MMT_Buildings_Ceramics.xml（新建） |

- commonality/lump 对齐 HSK（软粘土无 HSK 矿脉参照，取铅铝中间值）。
- MaxHitPoints 沿用 MMT 现有矿脉规模（1300–1800），不抄 HSK 的 5500+。
- 贴图沿用 `RockFlecked_Atlas` 双色模式。

## 4. 窑炉 `MMT_Kiln`（新建，入 `MMT_Buildings_Ceramics.xml`）

对标 HSK Kiln，SK 专属组件用原版替代：

- thingClass `Building_WorkTable`，3×3，stuffCategories = Stony + MMT_Bricks，costStuffCount 180。
- WorkToBuild 3000 / MaxHitPoints 270 / Flammability 0 / Cleanliness -8。
- comps：`CompProperties_Refuelable`（fuelCapacity 30，fuelFilter = MMT_Coal/MMT_Charcoal/MMT_Kindling/WoodLog）+ `CompProperties_Glower`（半径 3，暖色）+ `CompProperties_Flickable`。
- `designationCategory=MMT_Production`；researchPrerequisites = `MMT_BasicMetalworking`；constructionSkillPrerequisite 4。
- 初始 recipes：`MMT_MakeCeramics`、`MMT_MakeClayBricks`、`MMT_BurnThings`。
- 贴图：复制 HSK Kiln 贴图（Things/Building/Production/Kiln/，含朝向）到 MMT 目录。

## 5. 配方（8 条新增，另有 §6.4 篝火火化 1 条，共 9 条）

| defName | 输入 → 产出 | workAmount | 技能 | 挂载建筑 | 研究 |
|---|---|---|---|---|---|
| `MMT_SmeltLeadBar_Hand` | MMT_Anglesite×10 → MMT_LeadBar×10 | 1100 | 制作≥2 | MMT_HandFurnace | MMT_BasicMetalworking |
| `MMT_SmeltLeadBar_Electric` | MMT_Anglesite×20 → MMT_LeadBar×20 | 1000 | 制作≥6 | MMT_ElectricArcFurnace | MMT_ElectricSmelting |
| `MMT_SmeltAluminiumBar_Electric` | MMT_AluminiumOre×20 → MMT_AluminiumBar×20 | 1200 | 制作≥7 | MMT_ElectricArcFurnace | MMT_ElectricSmelting |
| `MMT_MakeFerrosiliconAlloy` | Steel×15 + MMT_Silicon×10 + MMT_AluminiumBar×5 → ×30 | 1700 | 制作≥8 | MMT_ElectricArcFurnace | MMT_ElectricSmelting |
| `MMT_MakeMetalCan` | [MMT_MetallicBar]×8 → MMT_MetalCan×15 | 800 | 制作≥2 | MMT_ElectricArcFurnace | MMT_ElectricSmelting |
| `MMT_MakeCeramics` | MMT_SoftClay×20 + MMT_GlassBatch×5 → MMT_Ceramics×10 | 1200 | 制作≥8 | MMT_Kiln + MMT_ElectricArcFurnace | MMT_BasicMetalworking |
| `MMT_MakeClayBricks` | MMT_SoftClay×15 → MMT_ClayBrick×15 | 800 | 制作≥6 | MMT_Kiln + MMT_ElectricArcFurnace | MMT_BasicMetalworking |
| `MMT_BurnThings` | [Textiles/Leathers/WoodLog]×20 → MMT_Ash×3 | 400 | 无 | 原版 Campfire + ElectricCrematorium + MMT_Kiln | 无 |

- 文件归属：`MMT_SmeltLeadBar_Hand` 入 `MMT_Recipes_Metal.xml`；4 条电配方（铅电/铝电/斯特/金属罐）入 `MMT_Recipes_Electric.xml`；陶瓷 2 条 + `MMT_BurnThings` + `MMT_CremateCorpseCampfire`（见 §6.4）入新建 `MMT_Recipes_Ceramics.xml`。
- `MMT_BurnThings` 的 filter：HSK 用 SK 类别 Wooden/Woody + 原版 Textiles/Leathers → MMT 用 filter.categories = Textiles + Leathers，另加 filter.thingDefs = WoodLog（固定为此组合）。
- 挂原版建筑用 `recipeUsers`（Campfire/ElectricCrematorium），遵守全局约定 §0.4。
- 金属/陶瓷配方沿用现有 workSpeedStat=SmeltingSpeed + effectWorking=Smelt 模式；BurnThings 用 GeneralLaborSpeed + Cremate（对齐 HSK）。
- `MMT_MakeMetalCan` 的 [金属锭类] 输入用 filter.categories 引用 `MMT_MetallicBar`。

## 6. 类别与 Patch

### 6.1 新建 StuffCategoryDef

`1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml`（新建目录）：`MMT_Bricks`，label 砖块。

### 6.2 既有锭类别补齐（直接改 Def，非 patch）

`MMT_CopperBar`/`MMT_TinBar`/`MMT_Bronze` 的 thingCategories：ResourcesRaw → `MMT_SLDBar`。

### 6.3 新建 `1.7/Patches/MMT_Patch_AshRecipes.xml`

| 目标配方 | 追加产出 |
|---|---|
| 原版 `BurnApparel` | MMT_Ash×2 |
| 原版 `CremateCorpse` | MMT_Ash×4 |

- 原版这两条无 `<products>` 节点：用 `PatchOperationSequence` = 条件检测 products 不存在时添加 `<products>` 节点 → 再插入 `<MMT_Ash>n</MMT_Ash>`。
- 不改原版配方的任何其他字段。

### 6.4 新建 `MMT_CremateCorpseCampfire` 配方（入 `MMT_Recipes_Ceramics.xml`）

[尸体]×1 → MMT_Ash×4，workAmount 800，GeneralLaborSpeed + Cremate，recipeUsers = 原版 Campfire。对标 HSK 同名配方（HSK 为自建非 patch）。

## 7. 交付清单与验证

1. Def/贴图/patch 如上。
2. `tools/validate_mod.py` 新增 `basic-metallurgy` 套件：校验 10 个资源存在且数值/类别符合本 spec、3 条矿脉参数、窑炉 stuffCategories 含 MMT_Bricks、9 条配方的输入/产出/工作量/研究/挂载、灰烬 patch 的 xpath 目标存在、铜锡青铜类别已改 MMT_SLDBar。注册进 SUITES 并更新文件顶部路径常量。
3. `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 增加：Anglesite/Aluminium/LeadBar/AluminiumBar/FerrosiliconAlloy/MetalCan/SoftClay/ClayBrick/Ceramics/Ash 的 MMT↔HSK 映射；重跑对比文档。
4. `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all` 全绿。
5. `dotnet build Source/MyModeTest/MyModeTest.csproj`（未改 pom/csproj，离线）0 错误。
6. 中文 label/description：label 用 HSK 1.6 中文翻译包对应译名，description 参考 1.6 翻译改写。

## 8. 提交计划（本地，中文 conventional commits）

1. `feat: 添加铅铝斯特金属罐资源与冶炼配方`（§2.1 + §3 铅铝矿脉 + §5 金属配方 + 类别补齐 6.2）
2. `feat: 添加窑炉与陶瓷粘土砖软粘土链`（§2.2 软粘土/陶瓷/粘土砖 + §3 粘土矿脉 + §4 + §5 陶瓷配方 + §6.1）
3. `feat: 添加灰烬资源与燃烧配方 patch`（§2.2 灰烬 + §6.3 + §6.4 + BurnThings）
4. `test: 添加 basic-metallurgy 验证套件与对比映射`（§7.2/7.3）

每个单元提交前跑对应 validate 套件；全部完成后统一 `validate all` + `dotnet build`。

## 9. 非目标（本批不做）

- 深钻井产出、野外矿点、矿脉散布参数对齐（路线图 §9 采集侧）。
- 灰烬下游消费（堆肥/硝石等属 B3/B5）。
- 金属罐下游（罐头食品链属 3.3 食物链，非目标）。
- HSK 木炭窑的烧砖/烧灰批量配方（CharcoalPit 系，属 UF 建筑体系）。
