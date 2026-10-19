# myModeTest RimWorld 1.7 电子材料资源生产链设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在已完成的 `myModeTest` 玻璃、混凝土、木材、金属、石化、强化玻璃六条链基础上，新增第七条资源生产链：电子材料链。本链首次整合多条已有链的产物：

```text
砂（玻璃链）
    ↓ 手动熔炉（已有建筑）
硅
硅 + 铜锭（金属链） + 玻璃（玻璃链）
    ↓ 手动电子工作台
电子元件
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

**HSK 电子链现状**：HSK 的硅只有电动配方（电弧炉 `EAF` 与 `MakeSilicon_Electric`），电子元件配方挂在耗电的 `TableElectronics` 上，且使用非原版的 `ElectronicCraftingSpeed` 统计。因此本链的手动配方为本 Mod 自设计，数值以 HSK 电动配方为基准按手动规则调整。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 手动提取硅 + 手动组装电子元件 |
| 电动阶段 | 只写入后续路线，本次不实施 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 硅提取建筑 | 复用已有 `MMT_HandFurnace`，不新建熔炉 |
| 电子元件用途 | 仅作为中间资源，不替代原版 `ComponentIndustrial` |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个研究：`MMT_BasicElectronics`
- 1 个生产建筑：`MMT_HandElectronicsTable`
- 2 个资源：`MMT_Silicon`、`MMT_ElectronicParts`
- 2 个配方：提取硅（挂 `MMT_HandFurnace`）、组装电子元件（挂 `MMT_HandElectronicsTable`）
- 8 张已授权贴图（2 张资源、6 张建筑含遮罩）
- `tools/validate_mod.py` 的 `electronics` 静态验证套件；金属套件熔炉配方列表同步更新
- 后续路线记录

### 3.2 不包含

- 耗电电子工作台（HSK `TableElectronics` 等值内容）与电弧炉硅提取。
- 集成电路、微芯片、生物芯片等高级电子资源。
- 电子元件的消费配方（高级建筑、设备成本改造）。
- HSK 自定义 `SK.` 类型、IdlePower 组件、设施联动、`ElectronicCraftingSpeed` 统计。
- 银锭、金锭输入（HSK 电子元件配方含 SilverBar，本 Mod 尚未实现金银链）。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆：

- 路径：`D:\project\rw\Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`

首版采用的手动流程基准：

| 流程 | HSK 基准 | 首版手动版 |
|---|---|---|
| 提取硅 | 砂 ×50 → 硅 ×5，800 工作量（电动） | 砂 ×50 → 硅 ×5，1600 工作量（手动加倍），Crafting 5 |
| 组装电子元件 | 铜类锭 ×35 + 玻璃 ×10 + 零部件 ×10 + 银锭 ×10 → 5（电动） | 铜锭 ×15 + 玻璃 ×10 + 硅 ×10 → 电子元件 ×5，2500 工作量，Crafting 8 |

与 HSK 的差异：

1. 硅提取为手动配方，工作量是 HSK 电动版的 2 倍（与既有链的手动/电动关系一致）。
2. 电子元件输入以 `MMT_Silicon` 替换 HSK 的零部件和银锭（这两者依赖未实现的链），铜类锭分类输入简化为 `MMT_CopperBar`。
3. HSK 的 `ElectronicCraftingSpeed` 不是原版统计，组装配方使用 `GeneralLaborSpeed`。

RimWorld 1.7 已核对的原版引用：`MicroelectronicsBasics`（研究）、`ResourcesRaw`、`SmeltingSpeed`、`GeneralLaborSpeed`、`Smith`（特效）、`Recipe_Smelt`、`Recipe_Machining`（音效）。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 用途 |
|---|---|---:|---:|---:|---|---|
| `MMT_Silicon` | 硅 | 4.0 | 300 | 0 | `Graphic_Single` | 电子元件原料 |
| `MMT_ElectronicParts` | 电子元件 | 25.0 | 75 | 1.0 | `Graphic_Single` | 中间资源 |

两个资源均继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`DeteriorationRate` 为 0，不设置 `stuffProps`。硅的数值取自 HSK；电子元件价值按首版原料成本估算（HSK 的 ElectronicComponents 价值 240 对应其完整后期科技树，不适用手动首版）。

## 6. 配方设计

两个配方统一：研究前置 `MMT_BasicElectronics`，`workSkill` 为 Crafting，`workSkillLearnFactor` 为 0.8，不写 `recipeUsers`。

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | 工作台 | workSpeedStat | effect/sound |
|---|---|---|---|---:|---:|---|---|---|
| `MMT_ExtractSilicon_Hand` | 提取硅 | `MMT_Sand ×50` | `MMT_Silicon ×5` | 1600 | 5 | `MMT_HandFurnace` | SmeltingSpeed | Smelt / Recipe_Smelt |
| `MMT_AssembleElectronicParts_Hand` | 组装电子元件 | `MMT_CopperBar ×15` + `MMT_Glass ×10` + `MMT_Silicon ×10` | `MMT_ElectronicParts ×5` | 2500 | 8 | `MMT_HandElectronicsTable` | GeneralLaborSpeed | Smith / Recipe_Machining |

## 7. 研究与建筑设计

### 7.1 研究

新增 `MMT_BasicElectronics`，中文名“基础电子工艺”。

- 基准成本：1600
- 科技等级：Industrial
- 前置研究：原版 `MicroelectronicsBasics`
- 解锁：`MMT_HandElectronicsTable` 和 2 个电子配方
- 研究界面坐标：`(16.00, 0.00)`，避开已有研究的 `(0,1)`、`(1,1)`、`(4,0)`、`(5,1)`、`(7,0)`、`(10,0)`、`(13,0)`

### 7.2 手动电子工作台

新增 `MMT_HandElectronicsTable`，中文名“手动电子工作台”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`3×3`，`drawSize (3,3)`（与 HSK 贴图比例一致）
- 材料分类：`Metallic`，材料消耗 140；零部件 ×5
- 建造工作量：2200；最大耐久：220；可燃性：1.0；清洁度：-3
- 建造技能要求：8
- 研究前置：`MMT_BasicElectronics`
- 配方：`MMT_AssembleElectronicParts_Hand`
- 不使用电力；不复制 HSK 的电力组件和设施联动
- 使用 `Graphic_Multi`，并复制材料遮罩贴图
- `interactionCellOffset`：`(0,0,-2)`

### 7.3 手动熔炉扩展

`MMT_HandFurnace` 的 recipes 列表追加 `MMT_ExtractSilicon_Hand`；建筑其余定义不变。金属链验证器的熔炉配方期望列表同步追加。

## 8. 贴图映射

所有贴图从本地 HSK 克隆（`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`）复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Resource/Silicon.png` | `Textures/Things/Item/MMT/MMT_Electronics/Silicon.png` |
| `Things/Item/Resource/EC.png` | `Textures/Things/Item/MMT/MMT_Electronics/ElectronicParts.png` |
| `Things/Building/Production/TableElectronics/TableElectronics_{east,north,south}.png` | `Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_{east,north,south}.png` |
| `Things/Building/Production/TableElectronics/TableElectronics_{east,north,south}m.png` | `Textures/Things/Building/Production/MMT_HandElectronicsTable/MMT_HandElectronicsTable_{east,north,south}m.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `electronics-resources`、`electronics-building`、`electronics-recipes`、`electronics` 套件，并纳入 `all`。
2. 检查 3 个电子链 XML 文件（资源/配方/研究）和 1 个建筑 XML。
3. 检查 2 个资源、2 个配方、1 个研究、1 个建筑。
4. 检查 8 张 PNG 的魔数、IHDR 块和非零宽高。
5. 配方引用闭包并入玻璃链（`MMT_Sand`、`MMT_Glass`）和金属链（`MMT_CopperBar`）资源。
6. `METAL_FURNACE_RECIPES` 追加 `MMT_ExtractSilicon_Hand`，金属建筑套件同步通过。
7. 检查原版引用白名单新增：`MicroelectronicsBasics`。
8. 检查所有新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.` 和 `ElectronicCraftingSpeed`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **金银链阶段**：新增金矿/银矿冶炼后，电子元件配方恢复 HSK 等值的银锭输入。
2. **电动电子阶段**：新增耗电电子工作台与电弧炉硅提取（HSK 等值：硅 800 工作量、电动组装配方）。
3. **高级电子阶段**：集成电路（电子元件 + 硅 + 塑料）、微芯片等 HSK 高级资源。
4. **消费配方阶段**：电子元件进入高级建筑和设备成本，评估与原版零部件的关系。
5. **平衡调整阶段**：游戏内测试硅提取效率、电子元件成本与价值后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py electronics` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- 电子链 4 个 XML 文件已创建/更新。
- 2 个资源、2 个配方、1 个研究、1 个建筑定义完整。
- 8 张已授权贴图按映射复制完成。
- 硅提取挂载在已有手动熔炉上；组装配方挂载在手动电子工作台。
- 配方引用覆盖玻璃链和金属链资源。
- `electronics` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
