# myModeTest RimWorld 1.7 基础金属合金资源生产链设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在已完成的 `myModeTest` 玻璃、混凝土、木材三条资源生产链基础上，新增第四条资源生产链：基础金属合金链。

本链参考 Hardcore SK（HSK）的手动金属冶炼流程，独立实现：

```text
铁矿脉 / 铜矿脉 / 锡矿脉（地图矿脉，手工开采）
    ↓
铁矿石 / 铜矿石 / 锡矿石
    ↓ 手动熔炉
铜锭 / 锡锭
铁矿石 + 木炭 ──→ 钢（原版 Steel）
铜锭 + 锡锭 ──→ 青铜
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 手动采矿脉 + 手动熔炼（铜锭、锡锭、青铜、钢） |
| 电动阶段 | 只写入后续路线，本次不实施 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description、jobString；英文 defName |
| 贴图 | 复制已确认获得授权的 HSK 静态贴图 |
| 钢产物 | 直接产出原版 `Steel`，不新建钢锭 Def |
| 碳源 | 使用本 Mod 已有资源 `MMT_Charcoal`，与木材链衔接 |
| 矿石来源 | 新增 3 个地表矿脉建筑；深层钻井写入后续路线 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个研究：`MMT_BasicMetalworking`
- 1 个生产建筑：`MMT_HandFurnace`
- 3 个矿脉建筑：`MMT_MineableIronOre`、`MMT_MineableCopperOre`、`MMT_MineableTinOre`
- 6 个资源：`MMT_IronOre`、`MMT_CopperOre`、`MMT_TinOre`、`MMT_CopperBar`、`MMT_TinBar`、`MMT_Bronze`
- 4 个配方：冶炼铜锭、冶炼锡锭、冶炼青铜、冶炼钢
- 18 张已授权贴图（9 张矿石、3 张锭、6 张熔炉含遮罩）
- `tools/validate_mod.py` 的 `metal` 静态验证套件
- 后续电动阶段路线记录

### 3.2 不包含

- 电动熔炉（电弧炉）与电动配方。
- 深层钻井矿石来源（`deepCommonality` 等字段）。
- 煤矿脉、镍、铝、铅、金银锭等其他 HSK 金属。
- 高科技合金（镍铜合金、硅铁合金等）。
- HSK 自定义 `SK.` 类型、燃料组件（`SK_WorkTableFueled`）、设施联动和热推组件。
- 铜/锡/青铜的武器装备消费配方。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为：

- 仓库：`https://github.com/HardcoreSK/Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`

首版采用的手动流程基准（对应 HSK `Recipes_Materials_Alloys.xml` 的 `*_Hand` 配方）：

| 流程 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---:|---:|
| 冶炼铜锭 | Copper ×10 | CopperBar ×10 | 700 | Crafting 1 |
| 冶炼锡锭 | Tin ×10 | TinBar ×10 | 600 | Crafting 1 |
| 冶炼青铜 | Copper/CopperBar ×20 + TinBar ×10 | Bronze ×30 | 1000 | Crafting 2 |
| 冶炼钢 | Iron ×15 + Coal 分类 ×13 | Steel ×20 | 1300 | Crafting 3 |

首版与 HSK 的差异：

1. 青铜配方只用 `MMT_CopperBar + MMT_TinBar`，不混入铜矿石和铝锭（HSK 的宽输入依赖更多资源链）。
2. 钢的碳源用 `MMT_Charcoal`（HSK 用 Coal 分类，本 Mod 尚未实现煤）。
3. 钢直接产出原版 `Steel`（HSK 重定义了 Steel 并另有 SteelBar；本 Mod 不覆盖原版 Def）。

RimWorld 1.7 已核对的原版引用：`RockBase`（抽象父类）、`Smithing`（研究）、`Metallic`（材料分类）、`ResourcesRaw`（物品分类）、`SmeltingSpeed`（工作速度统计）、`Steel`（资源）、`Things/Building/Linked/RockFlecked_Atlas`（矿脉图集贴图，原版资源无需复制）。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 图形 | 用途 |
|---|---|---:|---:|---|---|
| `MMT_IronOre` | 铁矿石 | 2.0 | 600 | `Graphic_StackCount` | 炼钢原料 |
| `MMT_CopperOre` | 铜矿石 | 2.0 | 600 | `Graphic_StackCount` | 炼铜原料 |
| `MMT_TinOre` | 锡矿石 | 2.0 | 600 | `Graphic_StackCount` | 炼锡原料 |
| `MMT_CopperBar` | 铜锭 | 3.0 | 300 | `Graphic_StackCount` | `Metallic` 建造材料 |
| `MMT_TinBar` | 锡锭 | 3.0 | 300 | `Graphic_StackCount` | `Metallic` 建造材料 |
| `MMT_Bronze` | 青铜 | 4.0 | 600 | `Graphic_StackCount` | `Metallic` 建造材料 |

### 5.1 矿石

三种矿石继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`DeteriorationRate` 为 0、`Flammability` 为 0。矿石颜色沿用 HSK：铁 `(125,140,131)`、铜 `(160,115,45)`、锡 `(210,210,255)`。不设置 `smeltable`（避免进入原版电熔炉的熔炼清单，该回收行为留待后续评估）。不设置 `stuffProps`（矿石不作为建造材料）。

### 5.2 锭与合金

三种锭继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，共享同一套锭贴图，用颜色区分：铜锭 `(160,115,45)`、锡锭 `(175,175,175)`、青铜 `(210,140,20)`。

`stuffProps` 只保留核心参数，加入原版 `Metallic` 分类：

| defName | stuffAdjective | MaxHitPoints | Flammability | WorkToMake | WorkToBuild |
|---|---|---:|---:|---:|---:|
| `MMT_CopperBar` | 铜制 | 1.4 | 0.1 | 1.5 | 1.5 |
| `MMT_TinBar` | 锡制 | 1.3 | 0 | 1.2 | 1.2 |
| `MMT_Bronze` | 青铜制 | 1.6 | 0 | 0.8 | 0.8 |

不使用 HSK 的非原版统计（如 `MeleePenetrationFactor`、`Bulk`）；护甲/隔热/武器伤害类数值留待游戏内平衡阶段评估。

## 6. 配方设计

四个配方统一：`workSpeedStat` 为 `SmeltingSpeed`（1.7 原版存在），`effectWorking` 为 `Smelt`，`soundWorking` 为 `Recipe_Smelt`，`workSkill` 为 Crafting，`workSkillLearnFactor` 为 0.8，工作台为 `MMT_HandFurnace`，研究前置为 `MMT_BasicMetalworking`。

### 6.1 冶炼铜锭

- defName：`MMT_SmeltCopperBar_Hand`
- 中文名：冶炼铜锭
- 输入：`MMT_CopperOre ×10`
- 输出：`MMT_CopperBar ×10`
- 工作量：700；技能要求：Crafting 1

### 6.2 冶炼锡锭

- defName：`MMT_SmeltTinBar_Hand`
- 中文名：冶炼锡锭
- 输入：`MMT_TinOre ×10`
- 输出：`MMT_TinBar ×10`
- 工作量：600；技能要求：Crafting 1

### 6.3 冶炼青铜

- defName：`MMT_MakeBronze_Hand`
- 中文名：冶炼青铜
- 输入：`MMT_CopperBar ×20`、`MMT_TinBar ×10`
- 输出：`MMT_Bronze ×30`
- 工作量：1000；技能要求：Crafting 2

### 6.4 冶炼钢

- defName：`MMT_SmeltSteel_Hand`
- 中文名：冶炼钢
- 输入：`MMT_IronOre ×15`、`MMT_Charcoal ×13`
- 输出：原版 `Steel ×20`
- 工作量：1300；技能要求：Crafting 3

## 7. 研究与建筑设计

### 7.1 研究

新增 `MMT_BasicMetalworking`，中文名“基础金属冶炼”。

- 基准成本：800
- 科技等级：Medieval
- 前置研究：原版 `Smithing`
- 解锁：`MMT_HandFurnace` 和 4 个冶炼配方
- 研究界面坐标：`(10.00, 0.00)`，避开木材 `(0,1)/(1,1)`、玻璃 `(4,0)`、混凝土 `(7,0)`

### 7.2 手动熔炉

新增 `MMT_HandFurnace`，中文名“手动熔炉”。

- `thingClass`：`Building_WorkTable`
- 尺寸：`3×3`
- 材料分类：`Stony`，材料消耗 240
- 建造工作量：3000；最大耐久：280；可燃性：0；清洁度：-8
- 建造技能要求：5
- 研究前置：`MMT_BasicMetalworking`
- 配方：4 个手动冶炼配方
- 不使用电力；不复制 HSK 的燃料组件、热推组件和设施联动
- 使用 `Graphic_Multi`，并复制材料遮罩贴图
- `interactionCellOffset`：`(0,0,-2)`

### 7.3 矿脉

三个矿脉继承原版抽象父类 `RockBase`，使用原版图集贴图 `Things/Building/Linked/RockFlecked_Atlas`（无需复制贴图），以颜色区分：

| defName | 中文名 | 产出 | 耐久 | color | colorTwo | 散布常见度 | 矿块大小 |
|---|---|---|---:|---|---|---:|---|
| `MMT_MineableIronOre` | 铁矿脉 | `MMT_IronOre` ×75 | 1500 | (125,140,131) | (147,144,134) | 1.0 | 15~25 |
| `MMT_MineableCopperOre` | 铜矿脉 | `MMT_CopperOre` ×75 | 1800 | (95,85,68) | (160,115,45) | 0.95 | 15~25 |
| `MMT_MineableTinOre` | 锡矿脉 | `MMT_TinOre` ×75 | 1300 | (160,178,181) | 不设 | 1.1 | 9~17 |

均设置 `isResourceRock`、`veinMineable`。铁矿脉为 HSK 中没有的新增内容（HSK 铁主要来自深钻），参数取铜矿脉同级的保守值。

## 8. 贴图映射

所有贴图来自固定提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`，复制到 `1.7/Textures` 下的 `MMT` 路径。矿脉使用原版图集贴图，不在此列。

| HSK 源路径 | myModeTest 目标路径 |
|---|---|
| `Textures/Things/Item/Ores/Iron/Iron_{a,b,c}.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_IronOre_{a,b,c}.png` |
| `Textures/Things/Item/Ores/Copper/Copper_{a,b,c}.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_CopperOre_{a,b,c}.png` |
| `Textures/Things/Item/Ores/Tin/Tin_{a,b,c}.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_TinOre_{a,b,c}.png` |
| `Textures/Things/Item/Ingot/ingot_{A,B,C}.png` | `Textures/Things/Item/MMT/MMT_Ingot/MMT_Ingot_{a,b,c}.png` |
| `Textures/Things/Building/Production/Furnace/Furnace_{east,north,south}.png` | `Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_{east,north,south}.png` |
| `Textures/Things/Building/Production/Furnace/Furnace_{east,north,south}m.png` | `Textures/Things/Building/Production/MMT_HandFurnace/MMT_HandFurnace_{east,north,south}m.png` |

锭的 texPath 统一为 `Things/Item/MMT/MMT_Ingot/MMT_Ingot`，三种锭通过 `color` 区分。

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `metal-resources`、`metal-building`、`metal-recipes`、`metal` 套件，并纳入 `all`。
2. 检查 4 个金属链 XML 文件。
3. 检查 6 个资源、4 个配方、1 个研究、1 个生产建筑、3 个矿脉。
4. 检查 18 张 PNG 的魔数、IHDR 块和非零宽高。
5. 检查配方输入、输出、研究、工作台引用闭包。
6. 检查钢配方产出引用原版 `Steel`、碳源引用 `MMT_Charcoal`。
7. 检查锭的 `Metallic` stuff 分类。
8. 检查矿脉的 `mineableThing` 与矿石 defName 配对、`veinMineable` 存在。
9. 检查原版引用白名单：`RockBase`、`Smithing`、`Metallic`、`ResourcesRaw`、`SmeltingSpeed`、`Steel`。
10. 检查所有新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **煤与燃料阶段**：新增煤矿脉和煤炭资源后，钢配方碳源改为煤/木炭双输入，并向 HSK 等值靠拢。
2. **深层钻井阶段**：为矿石补充 `deepCommonality` 等字段，接入原版深层钻井。
3. **电动熔炼阶段**：石化/电子链完成后，新增电弧炉与电动配方（HSK `*_Electric` 等值：铜 20→20/500、锡 20→20/500、钢 15+7→20/900）。
4. **扩展金属阶段**：镍、铝、铅、钨、钛及高科技合金（镍铜、硅铁、铝镍钴等）。
5. **消费配方阶段**：铜/锡/青铜的武器、工具、零部件消费配方；青铜时代装备路线。
6. **平衡调整阶段**：游戏内测试矿脉常见度、产出效率、研究成本后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py metal` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- 金属链 4 个 XML 文件已创建。
- 6 个资源、4 个配方、1 个研究、1 个生产建筑、3 个矿脉定义完整。
- 18 张已授权贴图按映射复制完成。
- 三种锭属于 `Metallic` 建造材料。
- 钢配方产出原版 `Steel`，碳源为 `MMT_Charcoal`。
- `metal` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
