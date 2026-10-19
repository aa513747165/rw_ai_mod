# myModeTest RimWorld 1.7 石化材料资源生产链设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在已完成的 `myModeTest` 玻璃、混凝土、木材、金属四条资源生产链基础上，新增第五条资源生产链：石化材料链。

本链参考 Hardcore SK（HSK）的石化生产流程，独立实现：

```text
油苗矿脉（手工开采）
    ↓
原油
    ↓ 手动化学实验台（蒸馏）
聚合物
    ↓ 手动化学实验台
塑料 / 橡胶 / 合成纤维
聚合物 + 木炭 + 橡胶 ──→ 碳材料
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

**HSK 参考基准变更说明**：本地 HSK 克隆（`D:\project\rw\Hardcore-SK`）为浅克隆，不包含此前固定的提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`；用户要求不再访问远程仓库。本条链及以后的参考基准改为本地 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`，2026-10-18）。

**HSK 石化链现状**：在本地 HEAD 中，`CrudeOil`（原油）的 ThingDef 已被注释，游戏中没有原油 → 聚合物的生产配方，聚合物主要来自高科技物质转换器；石化配方全部挂在耗电的石化实验室（`TableChemlab`）上，且使用 1.7 已删除的 `SmithingSpeed`。因此本链的上游（原油开采、蒸馏）为本 Mod 自设计，下游产品数值参考 HSK。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 手动采油苗 + 手动蒸馏聚合物 + 手动石化产品 |
| 电动阶段 | 只写入后续路线，本次不实施 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 原油来源 | 新增油苗矿脉；深层钻井写入后续路线 |
| 硫酸盐/硫磺支线 | 不实现，橡胶/合成纤维配方去掉硫酸盐输入 |
| 碳源 | 使用本 Mod 已有资源 `MMT_Charcoal`（替代 HSK 的 Coal 分类） |
| 石化产品用途 | 全部仅作为中间资源，不定义 stuffProps |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个研究：`MMT_BasicPetrochemistry`
- 1 个生产建筑：`MMT_HandChemTable`
- 1 个矿脉建筑：`MMT_MineableOilSeep`
- 6 个资源：`MMT_CrudeOil`、`MMT_Polymers`、`MMT_Plastic`、`MMT_Rubber`、`MMT_SyntheticFibers`、`MMT_Carbon`
- 5 个配方：蒸馏聚合物、合成塑料、合成橡胶、合成纤维、制备碳材料
- 9 张已授权贴图（6 张资源、3 张建筑）
- `tools/validate_mod.py` 的 `petrochem` 静态验证套件
- 后续电动阶段路线记录

### 3.2 不包含

- 耗电石化实验室（HSK `TableChemlab` 等值内容）。
- 深层钻井采油、油井建筑。
- 硫磺、硫酸盐、石蜡、合成氨等其他石化副产物。
- 碳纤维合金、合成丝线织物及下游消费配方。
- 塑料建材（HSK 的 `Plastic`/`Sterile` 材料分类非原版，不引入）。
- HSK 自定义 `SK.` 类型、IdlePower 组件、设施联动。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆：

- 路径：`D:\project\rw\Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`

首版采用的手动流程基准（HSK 配方数值，技能要求按手动版下调）：

| 流程 | 输入 | 输出 | 工作量 | HSK 技能 | 首版技能 |
|---|---|---|---:|---:|---:|
| 蒸馏聚合物 | 原油 ×20（自设计） | 聚合物 ×15 | 1500 | - | Crafting 4 |
| 合成塑料 | 聚合物 ×30 | 塑料 ×10 | 800 | Crafting 10 | Crafting 6 |
| 合成橡胶 | 聚合物 ×30（去掉硫酸盐 ×3） | 橡胶 ×10 | 800 | Crafting 10 | Crafting 6 |
| 合成纤维 | 聚合物 ×25（去掉硫酸盐 ×3） | 合成纤维 ×5 | 1200 | Crafting 14 | Crafting 8 |
| 制备碳材料 | 聚合物 ×25 + 木炭 ×25 + 橡胶 ×15 | 碳材料 ×12 | 4000 | Crafting 15 | Crafting 10 |

与 HSK 的差异：

1. HSK 使用 `SmithingSpeed`（1.7 已删除）。蒸馏配方使用 `SmeltingSpeed`，其余化学合成配方使用 `GeneralLaborSpeed`。
2. 去掉硫酸盐输入（硫磺链未实现，写入后续路线）。
3. 碳材料的煤分类输入替换为本 Mod 的 `MMT_Charcoal`。
4. 技能要求下调：HSK 的 10~15 对应其完整科技树后期，手动首版下调到 4~10。

RimWorld 1.7 已核对的原版引用：`RockBase`、`BiofuelRefining`（研究）、`ResourcesRaw`（物品分类）、`SmeltingSpeed`、`GeneralLaborSpeed`、`Smith`（工作特效）、`Recipe_Smith`、`Recipe_Smelt`（音效）、`Things/Building/Linked/RockFlecked_Atlas`。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 用途 |
|---|---|---:|---:|---:|---|---|
| `MMT_CrudeOil` | 原油 | 3.0 | 300 | 5.0 | `Graphic_Single` | 蒸馏原料 |
| `MMT_Polymers` | 聚合物 | 1.5 | 600 | 5.0 | `Graphic_Single` | 石化中间原料 |
| `MMT_Plastic` | 塑料 | 9.0 | 300 | 2.0 | `Graphic_Single` | 中间资源 |
| `MMT_Rubber` | 橡胶 | 11.0 | 300 | 2.0 | `Graphic_Single` | 中间资源 |
| `MMT_SyntheticFibers` | 合成纤维 | 12.0 | 150 | 2.0 | `Graphic_Single` | 中间资源 |
| `MMT_Carbon` | 碳材料 | 40.0 | 75 | 0 | `Graphic_Single` | 中间资源 |

六个资源均继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`DeteriorationRate` 为 0，不设置 `stuffProps`（石化产品不作为建造材料，塑料建材留待后续路线）。数值取自 HSK 资源定义。

## 6. 配方设计

五个配方统一：工作台为 `MMT_HandChemTable`，研究前置为 `MMT_BasicPetrochemistry`，`workSkill` 为 Crafting，`workSkillLearnFactor` 为 0.8，不写 `recipeUsers`。

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 | workSpeedStat | effect/sound |
|---|---|---|---|---:|---:|---|---|
| `MMT_DistillPolymers_Hand` | 蒸馏聚合物 | `MMT_CrudeOil ×20` | `MMT_Polymers ×15` | 1500 | 4 | SmeltingSpeed | Smelt / Recipe_Smelt |
| `MMT_MakePlastic_Hand` | 合成塑料 | `MMT_Polymers ×30` | `MMT_Plastic ×10` | 800 | 6 | GeneralLaborSpeed | Smith / Recipe_Smith |
| `MMT_MakeRubber_Hand` | 合成橡胶 | `MMT_Polymers ×30` | `MMT_Rubber ×10` | 800 | 6 | GeneralLaborSpeed | Smith / Recipe_Smith |
| `MMT_MakeSyntheticFibers_Hand` | 合成纤维 | `MMT_Polymers ×25` | `MMT_SyntheticFibers ×5` | 1200 | 8 | GeneralLaborSpeed | Smith / Recipe_Smith |
| `MMT_MakeCarbon_Hand` | 制备碳材料 | `MMT_Polymers ×25` + `MMT_Charcoal ×25` + `MMT_Rubber ×15` | `MMT_Carbon ×12` | 4000 | 10 | GeneralLaborSpeed | Smith / Recipe_Smith |

## 7. 研究与建筑设计

### 7.1 研究

新增 `MMT_BasicPetrochemistry`，中文名“基础石化工艺”。

- 基准成本：1200
- 科技等级：Industrial
- 前置研究：原版 `BiofuelRefining`
- 解锁：`MMT_HandChemTable` 和 5 个石化配方
- 研究界面坐标：`(13.00, 0.00)`，避开已有研究的 `(0,1)`、`(1,1)`、`(4,0)`、`(7,0)`、`(10,0)`

### 7.2 手动化学实验台

新增 `MMT_HandChemTable`，中文名“手动化学实验台”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`3×2`，`drawSize (3,2)`（HSK 原图为 4:5 竖向比例，压缩显示效果待实机评估）
- 材料分类：`Metallic`，材料消耗 130；零部件 ×5
- 建造工作量：2000；最大耐久：200；可燃性：1.0；清洁度：-3
- 建造技能要求：6
- 研究前置：`MMT_BasicPetrochemistry`
- 配方：5 个手动石化配方
- 不使用电力；不复制 HSK 的 IdlePower、热推组件和设施联动
- 使用 `Graphic_Multi`（HSK 该建筑无材料遮罩贴图）
- `interactionCellOffset`：`(0,0,-1)`

### 7.3 油苗矿脉

新增 `MMT_MineableOilSeep`，中文名“油苗”，继承原版抽象父类 `RockBase`，使用原版图集贴图着色（无需复制贴图）：

- 产出：`MMT_CrudeOil` ×75
- 耐久：1500
- color `(45,42,40)`，colorTwo `(65,34,34)`
- 散布常见度：0.8；矿块大小：10~18
- `isResourceRock`、`veinMineable`

## 8. 贴图映射

所有贴图从本地 HSK 克隆（`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`）复制到 `1.7/Textures` 下的 `MMT` 路径。油苗使用原版图集贴图，不在此列。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Resource/CrudeOil.png` | `Textures/Things/Item/MMT/MMT_Petro/CrudeOil.png` |
| `Things/Item/Resource/Polymers.png` | `Textures/Things/Item/MMT/MMT_Petro/Polymers.png` |
| `Things/Item/Resource/Plastic.png` | `Textures/Things/Item/MMT/MMT_Petro/Plastic.png` |
| `Things/Item/Resource/Rubber.png` | `Textures/Things/Item/MMT/MMT_Petro/Rubber.png` |
| `Things/Item/Resource/Synteticfibres.png` | `Textures/Things/Item/MMT/MMT_Petro/SyntheticFibers.png` |
| `Things/Item/Resource/Carbon.png` | `Textures/Things/Item/MMT/MMT_Petro/Carbon.png` |
| `Things/Building/Production/OilRefinery/OilRefinery_east.png` | `Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_east.png` |
| `Things/Building/Production/OilRefinery/OilRefinery_north.png` | `Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_north.png` |
| `Things/Building/Production/OilRefinery/OilRefinery_south.png` | `Textures/Things/Building/Production/MMT_HandChemTable/MMT_HandChemTable_south.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `petrochem-resources`、`petrochem-building`、`petrochem-recipes`、`petrochem` 套件，并纳入 `all`。
2. 检查 4 个石化链 XML 文件。
3. 检查 6 个资源、5 个配方、1 个研究、1 个生产建筑、1 个矿脉。
4. 检查 9 张 PNG 的魔数、IHDR 块和非零宽高。
5. 检查配方输入、输出、研究、工作台引用闭包（含金属链 `MMT_Charcoal` 跨链引用）。
6. 检查所有资源无 stuffProps、原油/聚合物 Flammability 为 5。
7. 检查矿脉 `mineableThing` 与原油 defName 配对。
8. 检查原版引用白名单新增：`BiofuelRefining`。
9. 检查所有新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。
10. 检查石化配方不得使用 1.7 不存在的 `SmithingSpeed`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **硫磺支线阶段**：新增硫矿石/硫磺/硫酸盐后，恢复 HSK 等值的橡胶、塑料批量和合成纤维配方。
2. **深层采油阶段**：为原油补充 `deepCommonality` 字段，接入原版深层钻井；评估油井建筑。
3. **电动石化阶段**：电子链完成后，新增耗电石化实验室（HSK `TableChemlab` 等值：450W、更高产出）。
4. **下游消费阶段**：碳纤维合金（碳 + 合成纤维）、合成丝线织物、强化玻璃（玻璃配合料 + 碳 + 合成纤维）。
5. **燃料联动阶段**：原油/聚合物与燃料系统（燃油、化合物）联动。
6. **平衡调整阶段**：游戏内测试油苗常见度、蒸馏效率、技能门槛后再调整数值；评估化学台贴图比例。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py petrochem` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- 石化链 4 个 XML 文件已创建。
- 6 个资源、5 个配方、1 个研究、1 个生产建筑、1 个矿脉定义完整。
- 9 张已授权贴图按映射复制完成。
- 蒸馏配方使用 `SmeltingSpeed`，其余配方使用 `GeneralLaborSpeed`，无 `SmithingSpeed`。
- 碳材料配方的碳源为 `MMT_Charcoal`。
- `petrochem` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
