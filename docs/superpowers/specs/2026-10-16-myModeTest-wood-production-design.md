# myModeTest RimWorld 1.7 手动木材资源生产链设计

日期：2026-10-16  
状态：设计已在对话中确认，待实施  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在已完成的玻璃资源链和手动混凝土资源链基础上，新增第三条资源生产链：手动木材加工链。

本链参考 Hardcore SK（HSK）的木材加工流程，使用 RimWorld 原版 `WoodLog` 独立实现：

```text
原木
    ↓ 手动锯木机
木板

原木
    ↓ 手动锯木机
柴火

原木
    ↓ 简易木炭窑
木炭
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 手动锯木、手动柴火、手动木炭 |
| 电动阶段 | 只写入后续路线，本次不实施 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 复制已确认获得授权的 HSK 静态贴图 |
| 木板用途 | 定义为原版 `Woody` 建造材料 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 3 个资源：`MMT_WoodPlank`、`MMT_Kindling`、`MMT_Charcoal`
- 2 个研究：`MMT_Woodworking`、`MMT_CharcoalBurning`
- 2 个建筑：`MMT_HandSawmill`、`MMT_CharcoalKiln`
- 3 个配方：锯木板、劈柴火、烧木炭
- 17 张静态贴图
- `tools/validate_mod.py` 的木材链静态验证套件
- 后续电动与扩展路线记录

### 3.2 不包含

- 电动锯木机。
- 电动木炭配方。
- HSK 自定义燃料组件、IdlePower、设施联动或 `SK.` 类型。
- 竹子、红木及其板材扩展。
- 石化材料、电子元件、高级加工建筑。
- 木板家具、武器或其他消费配方。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准与 1.7 适配

HSK 参考来源固定为：

- 仓库：`https://github.com/HardcoreSK/Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`

首版采用的手动流程基准：

| 流程 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---:|---:|
| 手动锯木板 | WoodLog ×10 | WoodPlank ×15 | 1200 | Crafting 3 |
| 手动劈柴火 | Wooden ×10 | Kindling ×20 | 500 | 无 |
| 手动烧木炭 | Wooden ×25 | Charcoal ×20 | 1600 | 无 |

RimWorld 1.7 适配：

- HSK 使用的 `SmithingSpeed` 在 RimWorld 1.7 中不存在，锯木和劈柴配方使用 `GeneralLaborSpeed`。
- HSK 使用的 `Wooden` 分类在 RimWorld 1.7 原版中不存在，首版直接使用原版 `WoodLog`。
- `SmeltingSpeed`、`MakeWoodPlanks_Hand` 效果和 `Recipe_MakeWoodPlanks_Hand` 音效在 RimWorld 1.7 中存在，可以继续使用。
- 不复制 HSK 的 `SK.` 燃料组件；简易木炭窑是普通手工工作台。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 图形 | 用途 |
|---|---|---:|---:|---|---|
| `MMT_WoodPlank` | 木板 | 1.5 | 300 | `Graphic_StackCount` | `Woody` 建造材料 |
| `MMT_Kindling` | 柴火 | 2.3 | 300 | `Graphic_Single` | 燃料类中间资源 |
| `MMT_Charcoal` | 木炭 | 2.0 | 600 | `Graphic_Single` | 燃料类中间资源 |

### 5.1 木板

`MMT_WoodPlank` 继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，并通过 `stuffProps` 加入原版 `Woody` 材料分类。

核心材料参数：

- `stuffAdjective`：木制
- `MaxHitPoints` 因子：0.9
- `Flammability` 因子：1
- `Beauty` 因子：1.65
- `WorkToMake` 因子：0.7
- `WorkToBuild` 因子：0.7
- 材料颜色参考 HSK：`(133,97,67)`
- 地形承载：`Light`

### 5.2 柴火

`MMT_Kindling` 继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，作为后续燃料系统的中间资源。

### 5.3 木炭

`MMT_Charcoal` 继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，作为后续金属和燃料系统的中间资源。

## 6. 配方设计

### 6.1 锯木板

- defName：`MMT_MakeWoodPlanks_Hand`
- 中文名：锯木板
- 输入：原版 `WoodLog ×10`
- 输出：`MMT_WoodPlank ×15`
- 工作量：1200
- 工作台：`MMT_HandSawmill`
- 研究：`MMT_Woodworking`
- 技能要求：Crafting 3
- `workSpeedStat`：`GeneralLaborSpeed`
- `effectWorking`：`MakeWoodPlanks_Hand`
- `soundWorking`：`Recipe_MakeWoodPlanks_Hand`

### 6.2 劈柴火

- defName：`MMT_MakeKindling_Hand`
- 中文名：劈柴火
- 输入：原版 `WoodLog ×10`
- 输出：`MMT_Kindling ×20`
- 工作量：500
- 工作台：`MMT_HandSawmill`
- 研究：`MMT_Woodworking`
- 技能要求：无
- `workSpeedStat`：`GeneralLaborSpeed`
- `effectWorking`：`MakeWoodPlanks_Hand`
- `soundWorking`：`Recipe_MakeWoodPlanks_Hand`

### 6.3 烧木炭

- defName：`MMT_MakeCharcoal_Hand`
- 中文名：烧制木炭
- 输入：原版 `WoodLog ×25`
- 输出：`MMT_Charcoal ×20`
- 工作量：1600
- 工作台：`MMT_CharcoalKiln`
- 研究：`MMT_CharcoalBurning`
- 技能要求：无
- `workSpeedStat`：`SmeltingSpeed`
- `effectWorking`：`Smelt`
- `soundWorking`：`Recipe_Smelt`

## 7. 研究与建筑设计

### 7.1 木工研究

新增 `MMT_Woodworking`，中文名“木工加工”。

- 基准成本：300
- 科技等级：Neolithic
- 前置研究：无
- 解锁：`MMT_HandSawmill`、`MMT_MakeWoodPlanks_Hand`、`MMT_MakeKindling_Hand`
- 研究界面坐标：`(0.00, 1.00)`

### 7.2 烧炭研究

新增 `MMT_CharcoalBurning`，中文名“木炭烧制”。

- 基准成本：500
- 科技等级：Medieval
- 前置研究：`MMT_Woodworking`
- 解锁：`MMT_CharcoalKiln`、`MMT_MakeCharcoal_Hand`
- 研究界面坐标：`(1.00, 1.00)`

### 7.3 手动锯木机

新增 `MMT_HandSawmill`，中文名“手动锯木机”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`3×1`
- 建造成本：木质材料 ×120
- 建造工作量：700
- 最大耐久：170
- 建造技能要求：3
- 研究前置：`MMT_Woodworking`
- 配方：`MMT_MakeWoodPlanks_Hand`、`MMT_MakeKindling_Hand`
- 不使用电力
- 使用 `Graphic_Multi`，并复制材料遮罩贴图

### 7.4 简易木炭窑

新增 `MMT_CharcoalKiln`，中文名“简易木炭窑”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`3×3`
- 建造成本：石质材料 ×180
- 建造工作量：3000
- 最大耐久：270
- 建造技能要求：4
- 研究前置：`MMT_CharcoalBurning`
- 配方：`MMT_MakeCharcoal_Hand`
- 不使用电力
- 不实现 HSK 燃料库存组件
- 使用 `Graphic_Multi`，并复制材料遮罩贴图

## 8. 贴图映射

所有贴图来自固定提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`，并复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径 | myModeTest 目标路径 |
|---|---|
| `Textures/Things/Item/Resource/WoodPlank/WoodPlank_a.png` | `Textures/Things/Item/MMT/WoodPlank/WoodPlank_a.png` |
| `Textures/Things/Item/Resource/WoodPlank/WoodPlank_b.png` | `Textures/Things/Item/MMT/WoodPlank/WoodPlank_b.png` |
| `Textures/Things/Item/Resource/WoodPlank/WoodPlank_c.png` | `Textures/Things/Item/MMT/WoodPlank/WoodPlank_c.png` |
| `Textures/Things/Item/Resource/Kindling.png` | `Textures/Things/Item/MMT/Kindling.png` |
| `Textures/Things/Item/Resource/Charcoal.png` | `Textures/Things/Item/MMT/Charcoal.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_east.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_east.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_eastm.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_eastm.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_north.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_north.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_northm.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_northm.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_south.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_south.png` |
| `Textures/Things/Building/Production/TableSawmillHand/TableSawmillHand_southm.png` | `Textures/Things/Building/Production/MMT_HandSawmill/MMT_HandSawmill_southm.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_east.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_east.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_eastm.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_eastm.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_north.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_north.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_northm.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_northm.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_south.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_south.png` |
| `Textures/Things/Building/Production/Kiln/Kiln_southm.png` | `Textures/Things/Building/Production/MMT_CharcoalKiln/MMT_CharcoalKiln_southm.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `wood-resources` 验证套件。
2. 新增 `wood-building` 验证套件。
3. 新增 `wood-recipes` 验证套件。
4. 新增 `wood` 聚合套件。
5. 将 `wood` 纳入 `all`。
6. 检查 4 个木材链 XML 文件。
7. 检查 3 个资源、3 个配方、2 个研究、2 个建筑。
8. 检查 17 张 PNG 的魔数、IHDR 块和非零宽高。
9. 检查配方输入、输出、研究、工作台和材料分类。
10. 检查木板的 `Woody` stuff 分类。
11. 检查所有新 Def 使用 `MMT_` 前缀。
12. 检查新 XML 不包含 `SK.`。
13. 检查锯木与劈柴配方使用 `GeneralLaborSpeed`，烧炭配方使用 `SmeltingSpeed`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **电动锯木阶段**：新增电力锯木机、电动木板和电动柴火配方，并评估 600W 电力消耗。
2. **电动木炭阶段**：新增更高级烧炭建筑或电力配方，缩短木炭生产时间。
3. **燃料系统阶段**：设计独立的燃料库存组件后，再让木炭窑实际消耗柴火、木炭或煤炭。
4. **木材扩展阶段**：加入竹子、红木及其板材，扩展手动和电动锯木配方。
5. **石化/电子联动阶段**：石化与电子材料链完成后，再实现 HSK 风格的高级电动生产建筑。
6. **消费配方阶段**：游戏内验证资源链后，再评估木板家具、简单武器、假肢或其他消费配方。
7. **平衡调整阶段**：游戏内测试产出效率、研究成本、建造成本和燃料价值，再决定是否调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py wood` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- 木材链 4 个 XML 文件已创建。
- 3 个资源、3 个配方、2 个研究、2 个建筑定义完整。
- 17 张已授权贴图按映射复制完成。
- 木板属于 `Woody` 建造材料。
- `wood` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续电动与扩展路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
