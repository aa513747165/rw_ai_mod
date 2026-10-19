# myModeTest RimWorld 1.7 集成电路与微芯片设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

电子链目前止步于电子元件。本条小包作为电子链的延伸（参照强化玻璃扩展玻璃链的先例），参考 Hardcore SK（HSK）的高阶电子材料流程，独立实现：

```text
硅 + 塑料 + 电子元件 + 金 →（手动电子工作台）→ 集成电路
集成电路 + 碳材料 + 金   →（手动电子工作台）→ 微芯片
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 集成电路 + 微芯片资源与配方，扩展既有电子链文件 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 物品分类 | 使用原版 `ResourcesRaw`，不引入 HSK 的 `ElectronicsCat` 自定义分类 |
| 金原料 | 使用原版 `Gold`（金银链产物），不实现 HSK 的 `GoldBar` 合金锭 |
| 研究门控 | 新增 1 个研究 `MMT_IntegratedCircuitTech`，两个配方共用 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 2 个资源（追加到既有电子资源文件）：`MMT_IntegratedCircuit`、`MMT_Microchip`
- 1 个研究（追加到既有电子研究文件）：`MMT_IntegratedCircuitTech`
- 2 个配方（追加到既有电子配方文件，挂载 `MMT_HandElectronicsTable`）：`MMT_AssembleIC_Hand`、`MMT_AssembleMicrochip_Hand`
- 2 张已授权贴图
- `tools/validate_mod.py` 的电子套件期望扩展
- 后续路线记录

### 3.2 不包含

- HSK 的生物微芯片（`BioMicrochips`）、AI 芯片等 Spacer 级后续材料。
- 集成电路/微芯片的下游用途（高阶设备）。
- 电动电子工作台（随电动设备包实施）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 集成电路（`Electronics`） | 价值 550、堆叠 75、可燃 1、Industrial |
| 微芯片（`Microchips`） | 价值 1000、堆叠 25、可燃 1、Spacer、Exotic 贸易标签 |
| 集成电路配方（`MakeIntegratedCircuits`） | 硅 25 + 塑料 20 + 电子元件 3 + 金锭 2 → 5，工作量 3500，Crafting 12 |
| 微芯片配方（`MakeMicrochipIntegratedCircuits`） | 集成电路 2 + 碳纤合金 5 + 金锭 5 → 1，工作量 3500，Crafting 16 |

与 HSK 的差异：

1. **价值缩放**：本 Mod 电子元件价值 25（HSK 240），约为 HSK 的 1/10；集成电路/微芯片价值按同比例缩放为 55/100，保持 Mod 内部经济一致。
2. **金原料替换**：HSK 用 `GoldBar` 合金锭，本 Mod 用原版 `Gold`（金银链产物，用户已确认金银作用与原版一致）。
3. **碳纤合金替换**：HSK 用 `CarbonAlloy`，本 Mod 用最接近的自有材料 `MMT_Carbon`（碳材料）。
4. **统计替换**：HSK 用 `ElectronicCraftingSpeed`（1.7 原版不存在），沿用电子链先例使用 `GeneralLaborSpeed`。
5. **技能门槛降低**：集成电路 Crafting 10（HSK 12）、微芯片 Crafting 12（HSK 16），维持手动流程的低门槛定位。
6. 不引入 `ElectronicsCat` 分类和 Exotic 贸易标签，使用原版 `ResourcesRaw`。

RimWorld 1.7 已核对的原版引用：`GeneralLaborSpeed`、`Smith`、`Recipe_Machining`、`ResourcesRaw`（均在前几条链中验证过）。

## 5. 资源设计

追加到 `MMT_Items_Resource_Electronics.xml`：

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | techLevel | 图形 |
|---|---|---:|---:|---:|---|---|
| `MMT_IntegratedCircuit` | 集成电路 | 55.0 | 75 | 1.0 | Industrial | `Graphic_Single` |
| `MMT_Microchip` | 微芯片 | 100.0 | 25 | 1.0 | Spacer | `Graphic_Single` |

均继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`DeteriorationRate` 为 0，不设置 `stuffProps`。

## 6. 研究设计

追加到 `MMT_Research_Electronics.xml`：

| defName | 中文名 | 工作量 | 时代 | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_IntegratedCircuitTech` | 集成电路技术 | 2000 | Industrial | `MMT_BasicElectronics` | (17.00, 1.00) |

两个配方共用该研究前置。

## 7. 配方设计

追加到 `MMT_Recipes_Electronics.xml`，挂载 `MMT_HandElectronicsTable`（recipes 列表追加），`GeneralLaborSpeed` + `Smith` + `Recipe_Machining`，技能成长 0.8：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---|---:|---|
| `MMT_AssembleIC_Hand` | 组装集成电路 | 硅 25 + 塑料 20 + 电子元件 3 + 金 2 | 集成电路 ×5 | 3500 | Crafting 10 |
| `MMT_AssembleMicrochip_Hand` | 组装微芯片 | 集成电路 2 + 碳材料 5 + 金 5 | 微芯片 ×1 | 3500 | Crafting 12 |

输入种类与数量沿用 HSK（金锭→金、碳纤合金→碳材料）。

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径（与电子链贴图同目录）。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Resource/Electronics.png` | `Textures/Things/Item/MMT/MMT_Electronics/IntegratedCircuit.png` |
| `Things/Item/Resource/Microchips.png` | `Textures/Things/Item/MMT/MMT_Electronics/Microchip.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py` 的电子套件（不新增套件，与强化玻璃扩展玻璃套件的做法一致）：

1. `ELECTRONICS_RESOURCE_EXPECTATIONS` 追加 `MMT_IntegratedCircuit`、`MMT_Microchip`（含各自 texPath）。
2. `ELECTRONICS_ITEM_TEXTURES` 追加 2 张 PNG。
3. 电子建筑验证器的工作台 recipes 期望追加 2 个配方名。
4. `ELECTRONICS_RECIPE_EXPECTATIONS` 追加 2 个配方（多输入分支已存在）；研究验证追加 `MMT_IntegratedCircuitTech` 字段检查（工作量 2000、Industrial、前置 `MMT_BasicElectronics`）。
5. 配方资源闭包并入石化资源 XML（塑料、碳材料）；金走 `VANILLA_THINGS` 白名单。
6. `validate_all()` 顺序不变（电子套件已在石化之后）。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **高阶设备阶段**：以集成电路/微芯片为零部件的高阶设备（太阳能、通讯台等）。
2. **生物微芯片阶段**：HSK `BioMicrochips` 等值内容与装配设备。
3. **电动电子工作台阶段**：随电动设备包实施电动配方（HSK 电动配方工作量 3500→更低，技能更高）。
4. **平衡调整阶段**：游戏内测试集成电路/微芯片的成本与价值关系后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electronics` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_IntegratedCircuit`/`MMT_Microchip` 资源定义完整。
- `MMT_IntegratedCircuitTech` 研究定义完整，两个配方共用此前置。
- 2 个配方挂载手动电子工作台，输入沿用 HSK 等值。
- 2 张已授权贴图按映射复制完成。
- `electronics` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
