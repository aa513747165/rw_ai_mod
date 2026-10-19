# myModeTest RimWorld 1.7 石化电动实验室设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

石化链目前只有手动配方（手动化学实验台：蒸馏聚合物、塑料、橡胶、合成纤维、碳材料）。本包是电动设备系列的第三个包，参考 Hardcore SK（HSK）的石化实验室（`TableChemlab`），独立实现：

```text
石化电动实验室（电力 450W，研究解锁）
  ├─ 电蒸馏聚合物（更快）
  ├─ 电制塑料 / 电制橡胶（更快）
  ├─ 电制合成纤维（更快）
  └─ 电制碳材料（更快）
```

电动配方与手动配方并存：手动配方不变，电动配方输入/产出相同、工作量更低、技能要求更高。

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 石化电动实验室 + 5 个电动配方 + 1 个研究 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的贴图 |
| 建筑实现 | 原版 `Building_WorkTable` + `CompProperties_Power`，不使用 HSK 的 `SK.` 组件 |
| 研究门控 | 新增 1 个研究 `MMT_ElectricPetrochemistry`，建筑与全部电动配方共用 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个建筑：`MMT_ElectricChemLab`（石化电动实验室，450W）
- 1 个研究：`MMT_ElectricPetrochemistry`
- 5 个电动配方：电蒸馏聚合物、电制塑料、电制橡胶、电制合成纤维、电制碳材料
- 3 张已授权贴图（东/北/南三向，HSK 原图无遮罩）
- `tools/validate_mod.py` 的 `electric-petrochem` 静态验证套件
- 后续路线记录

### 3.2 不包含

- 电子电动工作台（后续逐包实施）。
- HSK 的硫酸盐塑料（`MakePlasticSulphate`/`Bulk`）、橡胶硫化（`VulcanizeRubber`，原料为未实现的天然橡胶）、碳纤合金（`MakeCarbonAlloy`）配方。
- HSK 的石油抽油机（`Extractor`）与物质转化（`ConvertToPolymers`，终局内容）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 石化实验室（`TableChemlab`） | 4×5、RuggedMetallic×130 + 钢筋混凝土 40 + 玻璃 50 + 零部件 20 + 电子元件 5 + 机械 15、WorkToBuild 5500、HP 340、电力 450W（待机 45W）、建造技能 12、研究 `Oil_Industry_C5` |
| 电制塑料（`MakePlastic`） | 聚合物 30 → 塑料 10，800，Crafting 10 |
| 电制橡胶（`MakeRubber`） | 聚合物 30 + 硫酸盐 3 → 橡胶 10，800，Crafting 10 |
| 电制合成纤维（`MakeSyntheticFibers`） | 聚合物 25 + 硫酸盐 3 → 纤维 5，1200，Crafting 14 |
| 电制碳材料（`MakeCarbon`） | 聚合物 25 + 煤 25 + 橡胶 15 → 碳材料 12，4000，Crafting 15 |

关键调研事实：

1. HSK 电动石化配方的输入/产出/工作量与手动配方**完全相同**，仅技能要求更高——电动优势只来自建筑本身。本 Mod 沿用电动系列的既定模式（工作量更低、技能更高），不完全照抄 HSK 的零提升数值。
2. HSK **没有原油蒸馏配方**：聚合物靠终局"物质转化"（`ConvertToPolymers`，Super_matter 研究）获得。电蒸馏聚合物为本 Mod 自设计（基于手动蒸馏配方的电动版）。
3. HSK 石化实验室用 `SK.CompProperties_IdlePower`（450W/待机 45W），本 Mod 改用原版 `CompProperties_Power`（恒定 450W）。

与 HSK 的差异：

1. 建筑不使用 `SK.` 组件与设施连接（`CompProperties_AffectedByFacilities`/工具柜为 HSK 设施，不实现）。
2. 建造成本替换：`RuggedMetallic` → 原版 `Metallic`，`ReinforcedConcrete` → `MMT_ReinforcedConcrete`，`Glass` → `MMT_Glass`，`ElectronicComponents` → `MMT_ElectronicParts`，去掉 `Mechanism`。
3. 建造技能 10（HSK 12），与电弧炉包一致维持低门槛。
4. 配方的 `SmithingSpeed`（RimWorld 1.7 不存在）→ `GeneralLaborSpeed`（与手动配方一致）。
5. 硫磺输入沿用本 Mod 的 `MMT_Sulfur`（HSK 用独立的 `Sulphates`）；碳材料的煤输入沿用本 Mod 的（木炭|煤炭）元组。

## 5. 研究设计

新增文件 `MMT_Research_ElectricPetrochem.xml`：

| defName | 中文名 | 工作量 | 时代 | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_ElectricPetrochemistry` | 电动石化工艺 | 1800 | Industrial | `MMT_BasicPetrochemistry` + `MicroelectronicsBasics` | (14.00, 1.00) |

建筑与全部 5 个电动配方共用此前置。坐标规律与电弧冶炼一致（基础石化 13.00/0.00 → 电动 14.00/1.00）。

## 6. 建筑设计

新增文件 `MMT_Buildings_ElectricPetrochem.xml`，`MMT_ElectricChemLab` 中文名"石化电动实验室"：

- `Building_WorkTable`，4×5（HSK 尺寸），`Graphic_Multi`，texPath `Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab`，interactionCellOffset (0,0,3)
- 成本：`Metallic` 材料 ×130 + `MMT_ReinforcedConcrete` ×40 + `MMT_Glass` ×50 + `ComponentIndustrial` ×20 + `MMT_ElectronicParts` ×5
- WorkToBuild 5500、HP 340、Cleanliness -3、建造技能 10
- comps：`CompProperties_Power`（450W）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`
- recipes 挂载 5 个电动配方；研究前置 `MMT_ElectricPetrochemistry`

## 7. 配方设计

新增文件 `MMT_Recipes_ElectricPetrochem.xml`，均挂载石化电动实验室，研究前置 `MMT_ElectricPetrochemistry`，workSkill Crafting、技能成长 0.8：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | 对应手动配方 |
|---|---|---|---|---:|---|---|
| `MMT_DistillPolymers_Electric` | 电蒸馏聚合物 | 原油 20 | 聚合物 15 | 1000 | 6 | 同输入 / 1500 / 4 |
| `MMT_MakePlastic_Electric` | 电制塑料 | 聚合物 30 | 塑料 10 | 500 | 8 | 同输入 / 800 / 6 |
| `MMT_MakeRubber_Electric` | 电制橡胶 | 聚合物 30 + 硫磺 3 | 橡胶 10 | 500 | 8 | 同输入 / 800 / 6 |
| `MMT_MakeSyntheticFibers_Electric` | 电制合成纤维 | 聚合物 25 + 硫磺 3 | 合成纤维 5 | 800 | 10 | 同输入 / 1200 / 8 |
| `MMT_MakeCarbon_Electric` | 电制碳材料 | 聚合物 25 + （木炭\|煤炭） 25 + 橡胶 15 | 碳材料 12 | 2800 | 12 | 同输入 / 4000 / 10 |

- 电蒸馏聚合物沿用蒸馏的 `SmeltingSpeed` + `Smelt` + `Recipe_Smelt`；其余 4 个沿用合成的 `GeneralLaborSpeed` + `Smith` + `Recipe_Smith`（与手动配方一致）。
- 工作量统一降为手动的约 2/3（蒸馏 1500→1000、塑料/橡胶 800→500、纤维 1200→800、碳材料 4000→2800），技能比手动高 2（与电弧炉包的技能阶梯一致）。

## 8. 贴图映射

HSK 原图无遮罩，与手动化学实验台一致。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Building/Production/OilRefinery/OilRefinery_east.png` | `Textures/Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab_east.png` |
| `Things/Building/Production/OilRefinery/OilRefinery_north.png` | `Textures/Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab_north.png` |
| `Things/Building/Production/OilRefinery/OilRefinery_south.png` | `Textures/Things/Building/Production/MMT_ElectricChemLab/MMT_ElectricChemLab_south.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`，新增 `electric-petrochem` 套件并纳入 `all`（位于 `electric-wood` 之后）：

1. 研究字段检查（1800、Industrial、双前置、坐标 14.00/1.00）。
2. 建筑检查：必须包含 `CompProperties_Power`（450W）；逐项核对成本（含跨链成本项 `MMT_ReinforcedConcrete` 40、`MMT_Glass` 50）、尺寸 (4,5)、interactionCellOffset (0,0,3)、配方列表、研究前置、建造技能 10。
3. 配方检查：复用多输入元组模式；输入闭包并入石化/硫磺/木材/煤炭资源 XML；统一断言 speed/effect/sound/learn 0.8/研究前置；不得写 recipeUsers，必须挂载到石化电动实验室。
4. 3 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `SmithingSpeed`（1.7 不存在，与石化套件一致）。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **电子电动工作台**：电子元件/集成电路/微芯片的电动配方。
2. **熔炼返还阶段**：武器/工具熔炼回收配方（HSK `SmeltWeapon` 等值）。
3. **平衡调整阶段**：游戏内测试电力消耗与产能提升的性价比后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electric-petrochem` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_ElectricChemLab` 建筑定义完整，含 450W 电力组件。
- `MMT_ElectricPetrochemistry` 研究定义完整，建筑与 5 个配方共用此前置。
- 5 个电动配方挂载石化电动实验室，输入/工作量/技能符合设计表。
- 3 张已授权贴图按映射复制完成。
- `electric-petrochem` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
