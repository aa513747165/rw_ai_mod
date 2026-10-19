# myModeTest RimWorld 1.7 玻璃扩展链（强化玻璃）设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

玻璃、石化两条链已完成后，强化玻璃的全部原料均已齐备。本条链是玻璃链的扩展，参考 Hardcore SK（HSK）的强化玻璃流程，独立实现：

```text
玻璃配合料（玻璃链） + 碳材料（石化链） + 合成纤维（石化链）
    ↓ 玻璃工坊（已有建筑）
强化玻璃
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 强化玻璃资源 + 1 个配方 + 1 个研究 |
| 工作台 | 复用已有 `MMT_GlassworksTable`，不新建建筑 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 强化玻璃用途 | 仅作为资源产物，不定义 stuffProps（HSK 的 Glass 材料分类非原版，不引入） |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个研究：`MMT_ReinforcedGlassTech`
- 1 个资源：`MMT_ReinforcedGlass`
- 1 个配方：`MMT_MakeReinforcedGlass`
- 3 张已授权贴图（堆叠三态）
- 已有玻璃工坊的配方列表和研究文件扩展
- `tools/validate_mod.py` 的玻璃链套件扩展
- 后续路线记录

### 3.2 不包含

- 强化玻璃建材（窗、墙、家具等消费配方）。
- HSK 的 `Glass` 材料分类和 `GlassGL` 外观（非原版内容）。
- 新生产建筑。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

首版采用的手动流程基准（HSK `MakeReinforcedGlass` 配方）：

| 流程 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---:|---:|
| 强化玻璃 | GlassBatch ×40 + Carbon ×5 + SyntheticFibers ×5 | ReinforcedGlass ×5 | 1400 | Crafting 8 |

HSK 该配方挂载在玻璃工坊（`GlassworkTable`）上，与本 Mod 复用 `MMT_GlassworksTable` 的决策一致。`workSpeedStat` 为 `SmeltingSpeed`（1.7 原版存在，无需适配）。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 用途 |
|---|---|---:|---:|---:|---|---|
| `MMT_ReinforcedGlass` | 强化玻璃 | 17.0 | 150 | 0 | `Graphic_StackCount` | 中间资源 |

继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`techLevel` 为 Industrial，`DeteriorationRate` 为 0。不设置 `stuffProps`（HSK 的强化玻璃建材依赖其自定义 `Glass` 材料分类，不引入；建材用途写入后续路线）。

## 6. 配方设计

- defName：`MMT_MakeReinforcedGlass`
- 中文名：制作强化玻璃
- 输入：`MMT_GlassBatch ×40`、`MMT_Carbon ×5`、`MMT_SyntheticFibers ×5`
- 输出：`MMT_ReinforcedGlass ×5`
- 工作量：1400；技能要求：Crafting 8
- `workSpeedStat`：`SmeltingSpeed`；`effectWorking`：`Smelt`；`soundWorking`：`Recipe_Smelt`
- `workSkill`：Crafting；`workSkillLearnFactor`：0.8
- 工作台：`MMT_GlassworksTable`（加入其 recipes 列表，不写 recipeUsers）
- 研究前置：`MMT_ReinforcedGlassTech`

## 7. 研究与建筑设计

### 7.1 研究

新增 `MMT_ReinforcedGlassTech`，中文名“强化玻璃工艺”，写入既有文件 `MMT_Research_Glass.xml`。

- 基准成本：1500
- 科技等级：Industrial
- 前置研究：`MMT_Glassworking`、`MMT_BasicPetrochemistry`（跨链前置，体现原料依赖）
- 解锁：`MMT_MakeReinforcedGlass`
- 研究界面坐标：`(5.00, 1.00)`，在玻璃研究 `(4,0)` 附近，避开已有研究的 `(0,1)`、`(1,1)`、`(7,0)`、`(10,0)`、`(13,0)`

### 7.2 建筑

不新建建筑。`MMT_GlassworksTable` 的 recipes 列表追加 `MMT_MakeReinforcedGlass`；建筑的研究前置保持 `MMT_Glassworking` 不变（配方自身由 `MMT_ReinforcedGlassTech` 门控）。

## 8. 贴图映射

贴图从本地 HSK 克隆（`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`）复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/ReinforcedGlass/ReinforcedGlass_{a,b,c}.png` | `Textures/Things/Item/MMT/MMT_ReinforcedGlass/MMT_ReinforcedGlass_{a,b,c}.png` |

## 9. 静态验证设计

本条链改写在既有玻璃链文件上，验证器扩展既有玻璃套件而非新增套件：

1. `RESOURCE_EXPECTATIONS` 新增 `MMT_ReinforcedGlass`；`ITEM_TEXTURES` 追加 3 张堆叠贴图。
2. `BUILDING_RECIPES` 追加 `MMT_MakeReinforcedGlass`；`validate_building` 的研究集合扩为 `{"MMT_Glassworking", "MMT_ReinforcedGlassTech"}`，并校验新研究的 baseCost、techLevel、双前置和坐标。
3. `RECIPE_EXPECTATIONS` 新增 `MMT_MakeReinforcedGlass`；`validate_recipes` 支持多输入配方（inputs 列表形式，逻辑同混凝土验证器），引用闭包并入石化资源 XML。
4. 沿用既有检查：`MMT_` 前缀、禁止 `SK.`、PNG 魔数/IHDR/宽高。
5. `all` 套件结构不变，玻璃套件即覆盖扩展内容。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **建材阶段**：评估引入自定义 `Glass` 材料分类后，实现强化玻璃窗户、墙体或家具消费配方。
2. **碳纤维阶段**：碳材料 + 合成纤维 → 碳纤维合金（石化链后续路线）。
3. **电动扩容阶段**：评估强化玻璃的电动配方或批量配方。
4. **平衡调整阶段**：游戏内测试强化玻璃成本与价值（17.0）是否合理，再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py all` 通过（玻璃套件覆盖扩展内容）。
2. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
3. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- `MMT_ReinforcedGlass` 资源、配方、研究定义完整。
- 配方挂载在已有玻璃工坊上，输入覆盖玻璃链和石化链资源。
- 3 张已授权贴图按映射复制完成。
- 研究前置为 `MMT_Glassworking` + `MMT_BasicPetrochemistry`。
- `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
