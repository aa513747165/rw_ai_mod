# myModeTest RimWorld 1.7 电弧炉电动包设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

各资源链目前只有手动配方。本包是电动设备系列的第一个包，参考 Hardcore SK（HSK）的电弧炉（EAF）与电动冶炼配方，独立实现：

```text
电弧炉（电力 2500W，研究解锁）
  ├─ 电炼铜锭 / 电炼锡锭（双倍批量、更快）
  ├─ 电炼青铜 / 电炼钢（碳源减半）
  ├─ 电炼银 / 电炼金
  └─ 电提取硅（更快）
```

电动配方与手动配方并存：手动配方不变，电动配方批量相同或翻倍、工作量更低、技能要求更高。

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 电弧炉建筑 + 金属/硅 7 个电动配方 + 1 个研究 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 建筑实现 | 原版 `Building_WorkTable` + `CompProperties_Power`，不使用 HSK 的 `SK.` 动画/电力组件 |
| 研究门控 | 新增 1 个研究 `MMT_ElectricSmelting`，建筑与全部电动配方共用 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个建筑：`MMT_ElectricArcFurnace`（电弧炉，电力 2500W）
- 1 个研究：`MMT_ElectricSmelting`
- 7 个电动配方（挂载电弧炉）：电炼铜锭、电炼锡锭、电炼青铜、电炼钢、电炼银、电炼金、电提取硅
- 3 张已授权贴图（东/北/南三向）
- `tools/validate_mod.py` 的 `electric` 静态验证套件
- 后续路线记录

### 3.2 不包含

- 电动木材包（电动锯木机/木炭窑）、石化电动实验室、电子电动工作台（后续逐包实施）。
- HSK 的合金电动配方（镍/钴/钛等未实现材料）。
- 熔炼返还（SmeltWeapon/ExtractMetalFromSlag 等 HSK 回收配方）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 电弧炉（`EAF`） | 3×3、材料 180 + 零部件 15 等、WorkToBuild 4500、HP 280、电力 2500W（待机 250W）、建造技能 12、发光/发热 |
| 电炼钢（`MakeSteelBars_Electric`） | 铁 15 + 煤/木炭 7 → 钢 20，900，Crafting 6 |
| 电炼铜（`MakeCopperBars_Electric`） | 铜矿 20 → 铜锭 20，500，Crafting 4 |
| 电提取硅（`MakeSilicon_Electric`） | 砂 50 → 硅 5，800，Crafting 5 |
| 电炼金/银（`MakeGoldBars_Electric` 等） | 同批量，500/650，Crafting 6 |

与 HSK 的差异：

1. 建筑不使用 `SK.ThingDef_AnimatedWorktable`/`SK.CompProperties_IdlePower`，改用原版 `Building_WorkTable` + `CompProperties_Power`（2500W）+ `CompProperties_Flickable` + `CompProperties_Breakdownable` + `CompProperties_Glower`；不复制动画帧，只用 `EAF_off` 静态贴图。
2. 建造成本替换：HSK 的 `ReinforcedConcrete` → 本 Mod `MMT_ReinforcedConcrete`，`ElectronicComponents` → `MMT_ElectronicParts`，去掉 `Mechanism`；材料分类用原版 `Metallic`（HSK 用 `RuggedMetallic`）。
3. 建造技能 10（HSK 12），维持本 Mod 的低门槛定位。
4. 金/银电动配方的原料沿用本 Mod 金银链的矿石（HSK 用金银 + 助熔剂炼合金锭）。

RimWorld 1.7 已核对的原版引用：`Building_WorkTable`、`CompProperties_Power`、`CompProperties_Flickable`、`CompProperties_Breakdownable`、`CompProperties_Glower`、`Metallic`、`SmeltingSpeed`、`Smelt`、`Recipe_Smelt`（均在前几条链中验证过或为原版基础定义）。

## 5. 研究设计

新增文件 `MMT_Research_Electric.xml`：

| defName | 中文名 | 工作量 | 时代 | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_ElectricSmelting` | 电弧冶炼 | 2000 | Industrial | `MMT_BasicMetalworking` + `MicroelectronicsBasics` | (11.00, 1.00) |

电弧炉建筑与全部 7 个电动配方共用此前置。

## 6. 建筑设计

新增文件 `MMT_Buildings_Electric.xml`，`MMT_ElectricArcFurnace` 中文名"电弧炉"：

- `Building_WorkTable`，3×3，`Graphic_Multi`，texPath `Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace`
- 成本：`Metallic` 材料 ×180 + `MMT_ReinforcedConcrete` ×50 + `ComponentIndustrial` ×15 + `MMT_ElectronicParts` ×6
- WorkToBuild 4500、HP 280、建造技能 10
- comps：`CompProperties_Power`（basePowerConsumption 2500）、`CompProperties_Flickable`、`CompProperties_Breakdownable`、`CompProperties_Glower`（半径 4，颜色 (217,112,33,0)）
- recipes 挂载 7 个电动配方；研究前置 `MMT_ElectricSmelting`

## 7. 配方设计

新增文件 `MMT_Recipes_Electric.xml`，均挂载电弧炉，`SmeltingSpeed` + `Smelt` + `Recipe_Smelt`，技能成长 0.8，研究前置 `MMT_ElectricSmelting`：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | 对应手动配方 |
|---|---|---|---|---:|---|---|
| `MMT_SmeltCopperBar_Electric` | 电炼铜锭 | 铜矿石 20 | 铜锭 20 | 500 | 4 | 10→10 / 700 / 1 |
| `MMT_SmeltTinBar_Electric` | 电炼锡锭 | 锡矿石 20 | 锡锭 20 | 500 | 4 | 10→10 / 600 / 1 |
| `MMT_MakeBronze_Electric` | 电炼青铜 | 铜锭 20 + 锡锭 10 | 青铜 30 | 700 | 4 | 同输入 / 1000 / 2 |
| `MMT_SmeltSteel_Electric` | 电炼钢 | 铁矿石 15 + （木炭\|煤炭） 7 | 钢 20 | 900 | 6 | 碳源 13 / 1300 / 3 |
| `MMT_SmeltSilver_Electric` | 电炼银 | 银矿石 14 | 银 10 | 500 | 5 | 同输入 / 750 / 2 |
| `MMT_SmeltGold_Electric` | 电炼金 | 金矿石 12 | 金 15 | 500 | 6 | 同输入 / 750 / 3 |
| `MMT_ExtractSilicon_Electric` | 电提取硅 | 砂 50 | 硅 5 | 800 | 5 | 同输入 / 1600 / 5 |

电炼钢的碳源数量沿用 HSK 电动配方的 7（手动为 13），体现电弧炉效率；铜/锡采用 HSK 的双倍批量。

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径（只用静态关态贴图，不复制动画帧）。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Building/Production/EAF/EAF_off_east.png` | `Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_east.png` |
| `Things/Building/Production/EAF/EAF_off_north.png` | `Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_north.png` |
| `Things/Building/Production/EAF/EAF_off_south.png` | `Textures/Things/Building/Production/MMT_ElectricArcFurnace/MMT_ElectricArcFurnace_south.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `electric` 套件并纳入 `all`（位于 `electronics` 之后）：检查电弧炉建筑、研究、7 个配方和 3 张 PNG。
2. 电弧炉检查与手动台相反：必须包含 `CompProperties_Power` 且 basePowerConsumption 为 2500；检查成本清单（含 `MMT_ReinforcedConcrete` 50、`ComponentIndustrial` 15、`MMT_ElectronicParts` 6）。
3. 配方验证复用多输入模式：电炼钢碳源为 `(MMT_Charcoal, MMT_Coal)` ×7；产物钢/银/金走 `VANILLA_THINGS` 白名单；闭包并入金属/木材/煤炭/金银/基础资源 XML。
4. 研究字段检查（2000、Industrial、双前置、坐标 11.00/1.00）。
5. 检查新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **电动木材包**：电动锯木机/木炭窑与配套配方。
2. **石化电动实验室**：蒸馏与合成的电动配方。
3. **电子电动工作台**：电子元件/集成电路/微芯片的电动配方。
4. **熔炼返还阶段**：武器/工具熔炼回收配方（HSK `SmeltWeapon` 等值）。
5. **平衡调整阶段**：游戏内测试电力消耗与产能提升的性价比后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electric` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_ElectricArcFurnace` 建筑定义完整，含 2500W 电力组件。
- `MMT_ElectricSmelting` 研究定义完整，建筑与 7 个配方共用此前置。
- 7 个电动配方挂载电弧炉，输入/工作量/技能符合设计表。
- 3 张已授权贴图按映射复制完成。
- `electric` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
