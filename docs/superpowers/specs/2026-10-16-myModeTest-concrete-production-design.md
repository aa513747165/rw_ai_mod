# myModeTest RimWorld 1.7 手动混凝土资源生产链设计

日期：2026-10-16  
状态：设计已在对话中确认，待实施  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在已完成的 `myModeTest` 玻璃资源生产链基础上，新增第二条资源生产链：手动混凝土链。

本链复用已有资源 `MMT_Sand` 和 `MMT_CrushedStone`，参考 Hardcore SK（HSK）的手动混凝土生产流程，独立实现：

```text
砂 + 碎石
    ↓ 手动混凝土搅拌机
混凝土
    ↓ 原版切石台 + 金属材料
钢筋混凝土
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 手动混凝土 + 手动钢筋混凝土 |
| 电动阶段 | 只写入后续路线，本次不实施 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 复制已确认获得授权的 HSK 静态贴图 |
| 钢筋混凝土用途 | 定义为 `Stony` 建造材料 |
| 普通混凝土用途 | 仅作为中间资源 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个研究：`MMT_ConcreteBasic`
- 1 个建筑：`MMT_ConcreteMixer`
- 2 个资源：`MMT_Concrete`、`MMT_ReinforcedConcrete`
- 2 个配方：搅拌混凝土、制作钢筋混凝土
- 2 张资源贴图
- 手动混凝土搅拌机的 6 张建筑贴图和材料遮罩图
- `tools/validate_mod.py` 的 `concrete` 静态验证套件
- 后续电动阶段路线记录

### 3.2 不包含

- 电动混凝土搅拌机。
- 电动钢筋混凝土配方。
- 石化材料、橡胶、塑料、合成纤维、碳材料。
- 电子元件、高级装配台或 HSK 高级生产建筑。
- HSK 自定义 `SK.` 类型、程序集、设施联动和 IdlePower 组件。
- 混凝土墙体、地板或其他建筑消费配方。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为：

- 仓库：`https://github.com/HardcoreSK/Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`

首版采用的手动流程基准：

| 流程 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---:|---:|
| 手动搅拌混凝土 | SandResource ×45 + CrushedStone ×30 | ConcreteResource ×15 | 1200 | Crafting 3 |
| 手动钢筋混凝土 | Metallic ×10 + ConcreteResource ×10 | ReinforcedConcrete ×8 | 1600 | Crafting 4 |

RimWorld 1.7 已删除 HSK 使用的 `StonecuttingSpeed`。本链的钢筋混凝土配方使用原版 1.7 切石配方采用的 `GeneralLaborSpeed`。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 图形 | 用途 |
|---|---|---:|---:|---|---|
| `MMT_Concrete` | 混凝土 | 6.0 | 150 | `Graphic_Single` | 中间资源 |
| `MMT_ReinforcedConcrete` | 钢筋混凝土 | 16.0 | 75 | `Graphic_Single` | `Stony` 建造材料 |

### 5.1 混凝土

`MMT_Concrete` 继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，仅作为钢筋混凝土的原料。

### 5.2 钢筋混凝土

`MMT_ReinforcedConcrete` 继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，并通过 `stuffProps` 加入原版 `Stony` 材料分类。

核心材料参数：

- `stuffAdjective`：钢筋混凝土
- `MaxHitPoints` 因子：2.8
- `Flammability` 因子：0
- `WorkToMake` 因子：4.5
- `WorkToBuild` 因子：4.5
- 材料颜色参考 HSK：`(122,122,120)`

## 6. 配方设计

### 6.1 搅拌混凝土

- defName：`MMT_MixConcrete_Hand`
- 中文名：搅拌混凝土
- 输入：`MMT_Sand ×45`、`MMT_CrushedStone ×30`
- 输出：`MMT_Concrete ×15`
- 工作量：1200
- 工作台：`MMT_ConcreteMixer`
- 研究：`MMT_ConcreteBasic`
- 技能要求：Crafting 3
- `workSpeedStat`：`SmeltingSpeed`
- `effectWorking`：`Smelt`
- `soundWorking`：`Recipe_Smelt`

### 6.2 制作钢筋混凝土

- defName：`MMT_MakeReinforcedConcrete_Hand`
- 中文名：制作钢筋混凝土
- 输入：原版 `Metallic` 分类 ×10、`MMT_Concrete ×10`
- 输出：`MMT_ReinforcedConcrete ×8`
- 工作量：1600
- 工作台：原版 `TableStonecutter`
- 研究：`MMT_ConcreteBasic`
- 技能要求：Crafting 4
- `workSpeedStat`：`GeneralLaborSpeed`
- `effectWorking`：`CutStone`
- `soundWorking`：`Recipe_MakeStoneBlocks`

## 7. 研究与建筑设计

### 7.1 研究

新增 `MMT_ConcreteBasic`，中文名“混凝土技术”。

- 基准成本：1000
- 科技等级：Industrial
- 前置研究：原版 `Stonecutting`
- 解锁：`MMT_ConcreteMixer`、`MMT_MixConcrete_Hand`、`MMT_MakeReinforcedConcrete_Hand`
- 研究界面坐标：`(7.00, 0.00)`，避免与玻璃研究的 `(4.00, 0.00)` 重叠

### 7.2 手动混凝土搅拌机

新增 `MMT_ConcreteMixer`，中文名“手动混凝土搅拌机”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`2×1`
- 建造成本：金属材料 ×135、工业零部件 ×5
- 建造工作量：1600
- 最大耐久：180
- 建造技能要求：8
- 研究前置：`MMT_ConcreteBasic`
- 配方：`MMT_MixConcrete_Hand`
- 不使用电力
- 不复制 HSK 的自定义父类、设施联动或 `SK.` 组件
- 使用 `Graphic_Multi`，并复制材料遮罩贴图

## 8. 贴图映射

所有贴图来自固定提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`，并复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径 | myModeTest 目标路径 |
|---|---|
| `Textures/Things/Item/Resource/Concrete.png` | `Textures/Things/Item/MMT/Concrete.png` |
| `Textures/Things/Item/Resource/ReinforcedConcrete.png` | `Textures/Things/Item/MMT/ReinforcedConcrete.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_east.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_east.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_eastm.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_eastm.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_north.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_north.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_northm.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_northm.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_south.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_south.png` |
| `Textures/Things/Building/Production/ConcreteMixer/ConcreteMixer_southm.png` | `Textures/Things/Building/Production/MMT_ConcreteMixer/MMT_ConcreteMixer_southm.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `concrete` 验证套件。
2. 将 `concrete` 套件纳入 `all`。
3. 检查 4 个混凝土链 XML 文件。
4. 检查 2 个资源、2 个配方、1 个研究、1 个建筑。
5. 检查 8 张 PNG 的魔数、IHDR 块和非零宽高。
6. 检查配方输入、输出、研究、工作台和材料分类。
7. 检查钢筋混凝土的 `Stony` stuff 分类。
8. 检查所有新 Def 使用 `MMT_` 前缀。
9. 检查新 XML 不包含 `SK.`。
10. 检查 `MMT_MakeReinforcedConcrete_Hand` 使用 `GeneralLaborSpeed`。

## 10. 后续电动路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **石化材料链完成后**：补充橡胶、塑料、合成纤维等电动搅拌机材料来源。
2. **电子材料链完成后**：补充电子元件来源，并设计电动混凝土搅拌机。
3. **电动搅拌阶段**：新增二级混凝土研究、500W 电动搅拌机和 HSK 等值的快速搅拌配方。
4. **高级加工阶段**：新增高级装配或加工建筑后，再实现电动钢筋混凝土配方。
5. **建筑消费阶段**：完成游戏内加载验证后，再评估混凝土墙体、地板和其他建筑消费配方。
6. **平衡调整阶段**：游戏内测试产出效率、材料成本、研究成本和建造技能要求，再决定是否调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py concrete` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- 混凝土链 4 个 XML 文件已创建。
- 2 个资源、2 个配方、1 个研究、1 个建筑定义完整。
- 8 张已授权贴图按映射复制完成。
- 钢筋混凝土属于 `Stony` 建造材料。
- `concrete` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续电动路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
