# myModeTest RimWorld 1.7 电子电动工作台设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

电子链目前只有手动配方（手动电子工作台：电子元件、集成电路、微芯片）。本包是电动设备系列的第四个（最后一个）包，参考 Hardcore SK（HSK）的电子工作台（`TableElectronics`），独立实现：

```text
电子电动工作台（电力 600W，研究解锁）
  ├─ 电装电子元件（更快、技能更高）
  ├─ 电装集成电路（更快、技能更高）
  └─ 电装微芯片（更快、技能更高）
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
| 首版范围 | 电子电动工作台 + 3 个电动配方 + 1 个研究 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的贴图（含遮罩） |
| 建筑实现 | 原版 `Building_WorkTable` + `CompProperties_Power`，不使用 HSK 的 `SK.` 组件 |
| 研究门控 | 新增 1 个研究 `MMT_ElectricElectronics`，建筑与全部电动配方共用 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个建筑：`MMT_ElectricElectronicsTable`（电子电动工作台，600W）
- 1 个研究：`MMT_ElectricElectronics`
- 3 个电动配方：电装电子元件、电装集成电路、电装微芯片
- 6 张已授权贴图（东/北/南三向 + 遮罩）
- `tools/validate_mod.py` 的 `electric-electronics` 静态验证套件
- 后续路线记录

### 3.2 不包含

- HSK 的生物电子设备（`MakeBioelectronicDevices`）、AI 核心拆解（`DisassembleAIPersonaCore`）、矿物声呐模块（`MakeMineralSonarModule`）配方。
- HSK 的电动装配台（`AdvToolBench`）与机器人装配台（`RobAssem`）。
- 熔炼返还阶段（后续路线）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 电子工作台（`TableElectronics`） | 3×1、RuggedMetallic×140 + 铜锭 40 + 零部件 10 + 机械 10、WorkToBuild 2750、HP 220、电力 600W（待机 60W）、建造技能 8、研究 `MicroelectronicsBasics`、配方经 recipeUsers 挂载 |
| 电子元件（`MakeElectronicComponents`） | HCM 合金 35 + 玻璃 10 + 零部件 10 + 银锭 10 → 5，2500，Crafting 8 |
| 集成电路（`MakeIntegratedCircuits`） | 硅 25 + 塑料 20 + 电子元件 3 + 金锭 2 → 5，3500，Crafting 12 |
| 微芯片（`MakeMicrochipIntegratedCircuits`） | 集成电路 2 + 碳纤合金 5 + 金锭 5 → 1，3500，Crafting 16 |

关键调研事实：

1. 本 Mod 的手动电子配方当初就是按 HSK 电动配方等值缩放设计的（集成电路/微芯片的输入产出完全一致），因此 HSK 电动配方与本 Mod 手动配方数值相同、仅技能更高。本包沿用电动系列既定模式（工作量更低、技能更高），不完全照抄 HSK 的零提升数值。
2. HSK 电子元件配方原料为 HCM 合金/零部件/银锭（本 Mod 无 HCM 分类与合金锭），本 Mod 手动配方已用铜锭/玻璃/硅替代并通过验收，电动版沿用手动配方的输入。
3. HSK 电子工作台用 `SK.CompProperties_IdlePower`（600W/待机 60W），本 Mod 改用原版 `CompProperties_Power`（恒定 600W）。

与 HSK 的差异：

1. 建筑不使用 `SK.` 组件与设施连接；配方由建筑 recipes 列表挂载（HSK 用 recipeUsers，本 Mod 惯例为建筑挂载）。
2. 建造成本替换：`RuggedMetallic` → 原版 `Metallic`，`CopperBar` → `MMT_CopperBar`，去掉 `Mechanism`。
3. 配方的 `ElectronicCraftingSpeed`（RimWorld 1.7 不存在）→ `GeneralLaborSpeed`（与手动配方一致）。
4. 金原料沿用原版 `Gold`（HSK 用 `GoldBar` 合金锭）；碳材料沿用 `MMT_Carbon`（HSK 用 `CarbonAlloy`）。
5. 建造技能 8（与 HSK 一致）。

## 5. 研究设计

新增文件 `MMT_Research_ElectricElectronics.xml`：

| defName | 中文名 | 工作量 | 时代 | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_ElectricElectronics` | 电动电子工艺 | 2200 | Industrial | `MMT_IntegratedCircuitTech` | (18.00, 2.00) |

建筑与全部 3 个电动配方共用此前置。坐标沿电子研究斜线（基础电子 16.00/0.00 → 集成电路 17.00/1.00）延伸至 18.00/2.00；前置 `MMT_IntegratedCircuitTech` 已间接要求 `MicroelectronicsBasics`。

## 6. 建筑设计

新增文件 `MMT_Buildings_ElectricElectronics.xml`，`MMT_ElectricElectronicsTable` 中文名"电子电动工作台"：

- `Building_WorkTable`，3×1（HSK 尺寸），`Graphic_Multi`，texPath `Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable`，surfaceType Item，interactionCellOffset (0,0,-1)
- 成本：`Metallic` 材料 ×140 + `MMT_CopperBar` ×40 + `ComponentIndustrial` ×10
- WorkToBuild 2750、HP 220、Cleanliness -1、建造技能 8
- comps：`CompProperties_Power`（600W）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`
- recipes 挂载 3 个电动配方；研究前置 `MMT_ElectricElectronics`

## 7. 配方设计

新增文件 `MMT_Recipes_ElectricElectronics.xml`，均挂载电子电动工作台，`GeneralLaborSpeed` + `Smith` + `Recipe_Machining`，workSkill Crafting、技能成长 0.8，研究前置 `MMT_ElectricElectronics`：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | 对应手动配方 |
|---|---|---|---|---:|---|---|
| `MMT_AssembleElectronicParts_Electric` | 电装电子元件 | 铜锭 15 + 玻璃 10 + 硅 10 | 电子元件 5 | 1600 | 10 | 同输入 / 2500 / 8 |
| `MMT_AssembleIC_Electric` | 电装集成电路 | 硅 25 + 塑料 20 + 电子元件 3 + 金 2 | 集成电路 5 | 2400 | 12 | 同输入 / 3500 / 10 |
| `MMT_AssembleMicrochip_Electric` | 电装微芯片 | 集成电路 2 + 碳材料 5 + 金 5 | 微芯片 1 | 2400 | 14 | 同输入 / 3500 / 12 |

工作量统一降为手动的约 2/3（2500→1600、3500→2400），技能比手动高 2（集成电路电动技能 12 与 HSK 一致；微芯片 14 低于 HSK 16，维持本 Mod 的低门槛定位）。

## 8. 贴图映射

贴图含遮罩（`_m` 后缀，与手动电子工作台一致）。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Building/Production/TableElectronics/TableElectronics_{east,north,south}{,m}.png`（6 张） | `Textures/Things/Building/Production/MMT_ElectricElectronicsTable/MMT_ElectricElectronicsTable_{east,north,south}{,m}.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`，新增 `electric-electronics` 套件并纳入 `all`（位于 `electric-petrochem` 之后）：

1. 研究字段检查（2200、Industrial、前置 `["MMT_IntegratedCircuitTech"]`、坐标 18.00/2.00）。
2. 建筑检查：必须包含 `CompProperties_Power`（600W）；逐项核对成本（含跨链成本项 `MMT_CopperBar` 40）、尺寸 (3,1)、配方列表、研究前置、建造技能 8。
3. 配方检查：复用多输入元组模式；输入闭包并入电子/金属/基础/石化资源 XML 与 `VANILLA_THINGS`（`Gold`）；输出必须在电子资源闭包；统一断言 speed/effect/sound/learn 0.8/研究前置；不得写 recipeUsers，必须挂载到电子电动工作台。
4. 6 张 PNG 校验；3 个新 XML 的 `SK.` 扫描；配方文本不得包含 `ElectronicCraftingSpeed`（1.7 不存在，与电子套件一致）。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **熔炼返还阶段**：武器/工具熔炼回收配方（HSK `SmeltWeapon` 等值）。
2. **平衡调整阶段**：游戏内测试电力消耗与产能提升的性价比后再调整数值。
3. 电动设备系列至此覆盖金属/木材/石化/电子四条链，后续如新链加入可参照本系列模式扩展。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electric-electronics` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_ElectricElectronicsTable` 建筑定义完整，含 600W 电力组件。
- `MMT_ElectricElectronics` 研究定义完整，建筑与 3 个配方共用此前置。
- 3 个电动配方挂载电子电动工作台，输入/工作量/技能符合设计表。
- 6 张已授权贴图（含遮罩）按映射复制完成。
- `electric-electronics` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
