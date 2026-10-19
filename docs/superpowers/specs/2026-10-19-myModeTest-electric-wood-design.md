# myModeTest RimWorld 1.7 电动木材包设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

木材链目前只有手动配方（手动锯木机、简易木炭窑）。本包是电动设备系列的第二个包，参考 Hardcore SK（HSK）的电动锯木机与电动烧炭配方，独立实现：

```text
电动锯木机（电力 600W，研究解锁）
  ├─ 电锯木板（双倍批量、同工作量 = 效率翻倍）
  └─ 电劈柴火（双倍批量、同工作量）
电动木炭窑（电力 1500W，研究解锁）
  └─ 电烧木炭（同批量、工作量减半）
```

电动配方与手动配方并存：手动配方不变，电动配方批量翻倍或工作量更低、技能要求更高。

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 电动锯木机 + 电动木炭窑 + 3 个电动配方 + 1 个研究 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的贴图（含遮罩） |
| 建筑实现 | 原版 `Building_WorkTable` + `CompProperties_Power`，不使用 HSK 的 `SK.` 组件 |
| 研究门控 | 新增 1 个研究 `MMT_ElectricWoodworking`，两建筑与全部电动配方共用 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 2 个建筑：`MMT_ElectricSawmill`（电动锯木机，600W）、`MMT_ElectricCharcoalKiln`（电动木炭窑，1500W）
- 1 个研究：`MMT_ElectricWoodworking`
- 3 个电动配方：电锯木板、电劈柴火（挂电动锯木机）、电烧木炭（挂电动木炭窑）
- 12 张已授权贴图（两建筑各东/北/南三向 + 遮罩）
- `tools/validate_mod.py` 的 `electric-wood` 静态验证套件
- 后续路线记录

### 3.2 不包含

- 石化电动实验室、电子电动工作台（后续逐包实施）。
- HSK 木炭坑（`CharcoalPit`，依赖 `UniversalFermenterSK` 自定义 C# 类）及其灰烬/黏土砖配方。
- HSK 的红木/竹木板配方（本 Mod 无红木/竹材料）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 电动锯木机（`TableSawmillElectric`） | 3×1、StrongMetallic×150 + 零部件 10 + 电子元件 5 + 机械 5、WorkToBuild 5000、HP 170、电力 600W（待机 60W）、建造技能 10 |
| 电锯木板（`MakeWoodPlanks_Electric`） | 原木 20 → 木板 30，1200，Crafting 6 |
| 电劈柴火（`MakeKindling_Electric`） | 原木 20 → 柴火 40，500，Crafting 4，技能成长 0.25 |
| 电烧木炭（`MakeCharcoal_Electric`） | 原木 25 → 木炭 20，800（手动 1600 减半），Crafting 4 |

关键调研事实：

1. HSK 的 `MakeCharcoal_Electric` 是**孤儿配方**——全库搜索确认没有任何建筑挂载它（木炭实际靠手动炉/窑或木炭坑生产）。因此本 Mod 新建"电动木炭窑"建筑挂载该配方，建筑数值为自设计。
2. HSK 木炭坑使用 `UniversalFermenterSK.RecipeDef_UF` 自定义 C# 类，本 Mod 不可用、不实现。
3. HSK 电动锯木机用 `SK.CompProperties_IdlePower`（600W/待机 60W），本 Mod 改用原版 `CompProperties_Power`（恒定 600W）。

与 HSK 的差异：

1. 建筑不使用 `SK.` 组件，改用原版电力组件（与电弧炉包一致）。
2. 建造成本替换：`StrongMetallic` → 原版 `Metallic`，`ElectronicComponents` → `MMT_ElectronicParts`，去掉 `Mechanism`。
3. 电锯/电劈配方的 `SmithingSpeed`（RimWorld 1.7 不存在）→ `GeneralLaborSpeed`（与手动配方一致）。
4. 输入的 `Wooden` 分类（RimWorld 1.7 不存在）→ 原版 `WoodLog`（与手动配方一致）。
5. 电动锯木/柴火配方的音效与特效沿用手动配方已验证的 `MakeWoodPlanks_Hand`/`Recipe_MakeWoodPlanks_Hand`，不引入 HSK 的 `*_Electric` 音效（无法确认 1.7 原版存在对应 SoundDef）。

## 5. 研究设计

新增文件 `MMT_Research_ElectricWood.xml`：

| defName | 中文名 | 工作量 | 时代 | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_ElectricWoodworking` | 电动木材加工 | 1500 | Industrial | `MMT_CharcoalBurning` + `MicroelectronicsBasics` | (2.00, 1.00) |

两建筑与全部 3 个电动配方共用此前置。坐标沿木材研究行（木工 0.00、烧炭 1.00）延伸至 2.00。

## 6. 建筑设计

新增文件 `MMT_Buildings_ElectricWood.xml`，两个建筑均为 `Building_WorkTable` + 原版电力组件。

### 6.1 `MMT_ElectricSawmill`（电动锯木机）

- 3×1、`Graphic_Multi`、texPath `Things/Building/Production/MMT_ElectricSawmill/MMT_ElectricSawmill`（含遮罩）
- 成本：`Metallic` 材料 ×150 + `ComponentIndustrial` ×10 + `MMT_ElectronicParts` ×5
- WorkToBuild 5000、HP 170、建造技能 10（与 HSK 一致）
- comps：`CompProperties_Power`（600W）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`
- surfaceType Item、interactionCellOffset (0,0,-1)（与手动锯木机一致）
- recipes：电锯木板、电劈柴火；研究前置 `MMT_ElectricWoodworking`

### 6.2 `MMT_ElectricCharcoalKiln`（电动木炭窑，自设计建筑）

- 3×3、`Graphic_Multi`、texPath `Things/Building/Production/MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln`（含遮罩）
- 成本：`Stony` 材料 ×180 + `ComponentIndustrial` ×5 + `MMT_ElectronicParts` ×3
- WorkToBuild 3000、HP 270、建造技能 8（介于手动窑 4 与电弧炉 10 之间）
- comps：`CompProperties_Power`（1500W，干馏加热耗电，介于玻璃工坊 400W 与电弧炉 2500W 之间）+ `CompProperties_Flickable` + `CompProperties_Breakdownable` + `CompProperties_Glower`（半径 4，颜色 (217,112,33,0)）
- interactionCellOffset (0,0,-2)（与简易木炭窑一致）
- recipes：电烧木炭；研究前置 `MMT_ElectricWoodworking`

## 7. 配方设计

新增文件 `MMT_Recipes_ElectricWood.xml`，研究前置均为 `MMT_ElectricWoodworking`，workSkill Crafting：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | 成长 | 对应手动配方 |
|---|---|---|---|---:|---|---|---|
| `MMT_MakeWoodPlanks_Electric` | 电锯木板 | 原木 20 | 木板 30 | 1200 | 6 | 0.8 | 10→15 / 1200 / 3 |
| `MMT_MakeKindling_Electric` | 电劈柴火 | 原木 20 | 柴火 40 | 500 | 4 | 0.25 | 10→20 / 500 / 无 |
| `MMT_MakeCharcoal_Electric` | 电烧木炭 | 原木 25 | 木炭 20 | 800 | 4 | 0.8 | 25→20 / 1600 / 无 |

电锯木板/电劈柴火：`GeneralLaborSpeed` + `MakeWoodPlanks_Hand` + `Recipe_MakeWoodPlanks_Hand`（沿用手动配方）；电烧木炭：`SmeltingSpeed` + `Smelt` + `Recipe_Smelt`。

木板/柴火采用 HSK 的双倍批量同工作量（效率翻倍）；木炭采用 HSK 的同批量工作量减半。

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径，均含遮罩（`_m` 后缀，与手动建筑一致）。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Building/Production/TableSawmill/TableSawmill_{east,north,south}{,m}.png`（6 张） | `Textures/Things/Building/Production/MMT_ElectricSawmill/MMT_ElectricSawmill_{east,north,south}{,m}.png` |
| `Things/Building/Production/Kiln/Kiln_{east,north,south}{,m}.png`（6 张） | `Textures/Things/Building/Production/MMT_ElectricCharcoalKiln/MMT_ElectricCharcoalKiln_{east,north,south}{,m}.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`，新增 `electric-wood` 套件并纳入 `all`（位于 `electric` 之后）：

1. 研究字段检查（1500、Industrial、双前置 `MMT_CharcoalBurning`+`MicroelectronicsBasics`、坐标 2.00/1.00）。
2. 两建筑检查：与手动台相反，必须包含 `CompProperties_Power`（锯木机 600W、木炭窑 1500W）；逐项核对成本、尺寸、配方列表、研究前置、建造技能；木炭窑检查 Glower（半径 4）。
3. 配方检查：单输入（`WoodLog`）模式；输入走 `VANILLA_THINGS`，输出走木材资源闭包；统一断言 workSpeedStat/effect/sound/learn/研究前置；不得写 recipeUsers，必须挂载到对应建筑。
4. 12 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `SmithingSpeed` 或 `Wooden`（1.7 不存在，与木材套件一致）。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **石化电动实验室**：蒸馏与合成的电动配方。
2. **电子电动工作台**：电子元件/集成电路/微芯片的电动配方。
3. **熔炼返还阶段**：武器/工具熔炼回收配方（HSK `SmeltWeapon` 等值）。
4. **平衡调整阶段**：游戏内测试电力消耗与产能提升的性价比后再调整数值；电动木炭窑功耗 1500W 为自设计值，优先评估。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electric-wood` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_ElectricSawmill`（600W）、`MMT_ElectricCharcoalKiln`（1500W）建筑定义完整，均含电力组件。
- `MMT_ElectricWoodworking` 研究定义完整，两建筑与 3 个配方共用此前置。
- 3 个电动配方挂载到对应建筑，输入/工作量/技能符合设计表。
- 12 张已授权贴图（含遮罩）按映射复制完成。
- `electric-wood` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
