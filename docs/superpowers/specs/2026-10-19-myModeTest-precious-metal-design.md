# myModeTest RimWorld 1.7 金银链设计

日期：2026-10-19  
状态：设计已确认（用户指示：按 HSK 实现金/银矿石 + 矿脉 + 手动熔炉冶炼 → 原版金银）  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

金属链已覆盖铁/铜/锡/钢，本条小包补充贵金属，参考 Hardcore SK（HSK）的金银流程，独立实现：

```text
银矿脉（手工开采）→ 银矿石 →（手动熔炉冶炼）→ 原版 Silver
金矿脉（手工开采）→ 金矿石 →（手动熔炉冶炼）→ 原版 Gold
```

产物为原版 `Silver`/`Gold`，作用与原版完全一致（货币、交易、材料），不实现 HSK 的合金锭（`GoldBar`/`SilverBar`）。

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 金/银矿石 + 金/银矿脉 + 手动熔炉冶炼 → 原版金银 |
| 产物 | 原版 `Silver`/`Gold`，作用与原版一致（用户明确指示） |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 物品分类 | 使用原版 `ResourcesRaw`，不引入 HSK 的 `PRS`/`Matty` 自定义分类 |
| 研究门控 | 复用 `MMT_BasicMetalworking`，无新研究 |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 2 个资源：`MMT_SilverOre`、`MMT_GoldOre`
- 2 个矿脉建筑：`MMT_MineableSilverOre`、`MMT_MineableGoldOre`
- 2 个配方：`MMT_SmeltSilver_Hand`、`MMT_SmeltGold_Hand`（挂载手动熔炉）
- 6 张已授权贴图（金/银矿石各 a/b/c）
- `tools/validate_mod.py` 的 `precious` 静态验证套件
- 后续路线记录

### 3.2 不包含

- HSK 的合金锭（`GoldBar`/`SilverBar`）及助熔剂（`HCM` 分类）体系。
- 原版金银矿脉的修改（原版 `MineableGold`/`MineableSilver` 保持不变，与本链并存）。
- 金银的深层钻井字段。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 银矿石（注释中的 `Silver`） | 价值 1、堆叠 1800、可燃 0、颜色 (180,173,150)、`Graphic_StackCount` |
| 金矿石（注释中的 `Gold`） | 价值 25、堆叠 300、可燃 0、颜色 (255,215,0)、`Graphic_StackCount` |
| 银锭配方（`MakeSilverBars_Hand`） | 银 14 + 助熔剂 3 → 银锭 10，工作量 750，Crafting 2，SmeltingSpeed |
| 金锭配方（`MakeGoldBars_Hand`） | 金 12 + 助熔剂 3 → 金锭 15，工作量 750，Crafting 3，SmeltingSpeed |

与 HSK 的差异：

1. HSK 的金/银矿石 ThingDef 已被注释（改用原版金银直接作原料）；本链恢复矿石形态，冶炼产物为原版金银。
2. 不实现助熔剂（`HCM` 分类）输入，配方只消耗矿石，比例沿用 HSK（银 14→10、金 12→15）。
3. HSK 无金/银矿脉（用原版矿脉）；本链的矿脉为自设计内容（与油苗、煤矿脉、硫磺矿脉一致的自设计先例），数值待平衡。
4. 不引入 HSK 的 `PRS`/`Matty` 物品分类，使用原版 `ResourcesRaw`；不复制深钻字段。

RimWorld 1.7 已核对的原版引用：`RockBase`、`ResourcesRaw`、`Things/Building/Linked/RockFlecked_Atlas`、`SmeltingSpeed`、`Smelt`、`Recipe_Smelt`（均在前几条链中验证过）。原版 `Silver`/`Gold` 作为产物加入验证器 `VANILLA_THINGS` 白名单。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 颜色 |
|---|---|---:|---:|---:|---|---|
| `MMT_SilverOre` | 银矿石 | 1.0 | 1800 | 0 | `Graphic_StackCount` | (180,173,150) |
| `MMT_GoldOre` | 金矿石 | 25.0 | 300 | 0 | `Graphic_StackCount` | (255,215,0) |

均继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，`techLevel` 为 Medieval，`DeteriorationRate` 为 0，不设置 `stuffProps`。数值沿用 HSK 注释中的矿石定义。

## 6. 矿脉设计

新增 2 个矿脉，继承原版抽象父类 `RockBase`，使用原版图集贴图着色（无需复制贴图）：

| defName | 中文名 | 产出 | 耐久 | 常见度 | 矿块 | colorTwo（矿斑） |
|---|---|---|---:|---:|---|---|
| `MMT_MineableSilverOre` | 银矿脉 | `MMT_SilverOre` ×75 | 1500 | 1.0 | 10~16 | (180,173,150) |
| `MMT_MineableGoldOre` | 金矿脉 | `MMT_GoldOre` ×75 | 1500 | 0.5 | 8~14 | (255,215,0) |

均含 color `(65,34,34)`、`isResourceRock`、`veinMineable`。金矿脉常见度 0.5，定位稀有资源，与原版金矿地位一致。

## 7. 配方设计

两个配方均挂载 `MMT_HandFurnace`（recipes 列表追加），研究前置 `MMT_BasicMetalworking`，`SmeltingSpeed` + `Smelt` + `Recipe_Smelt`，技能成长 0.8：

| defName | 中文名 | 输入 | 输出 | 工作量 | 技能 |
|---|---|---|---:|---:|---|
| `MMT_SmeltSilver_Hand` | 冶炼银 | 银矿石 ×14 | 原版 `Silver` ×10 | 750 | Crafting 2 |
| `MMT_SmeltGold_Hand` | 冶炼金 | 金矿石 ×12 | 原版 `Gold` ×15 | 750 | Crafting 3 |

投入产出比沿用 HSK 的银锭/金锭配方（去掉助熔剂）。

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径（与金属矿石同目录风格）。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Ores/Silver/Silver_a.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_SilverOre_a.png` |
| `Things/Item/Ores/Silver/Silver_b.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_SilverOre_b.png` |
| `Things/Item/Ores/Silver/Silver_c.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_SilverOre_c.png` |
| `Things/Item/Ores/Gold/Gold_a.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_GoldOre_a.png` |
| `Things/Item/Ores/Gold/Gold_b.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_GoldOre_b.png` |
| `Things/Item/Ores/Gold/Gold_c.png` | `Textures/Things/Item/MMT/MMT_Ore/MMT_GoldOre_c.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `precious` 套件并纳入 `all`（位于 `metal` 之后）：检查 2 个矿石资源字段、2 个矿脉、2 个配方和 6 张 PNG。
2. `VANILLA_THINGS` 白名单追加 `Silver`、`Gold`（原版产物）。
3. `METAL_FURNACE_RECIPES` 追加 `MMT_SmeltSilver_Hand`、`MMT_SmeltGold_Hand`（金属建筑验证器同步覆盖熔炉配方列表变化）。
4. 配方验证复用金属链模式：输入仅 thingDefs 引用、`fixedIngredientFilter` 扁平化集合比较、必须挂载手动熔炉、研究前置为 `MMT_BasicMetalworking`。
5. 检查新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **深层钻井阶段**：为金/银矿石补充 `deepCommonality` 等字段（HSK 注释值：银 commonality 1.0、金 0.6）。
2. **电动冶炼阶段**：电弧炉配套的金/银电动配方（随电动设备包实施）。
3. **平衡调整阶段**：游戏内测试金矿脉常见度、冶炼比例与原版直接采矿的关系后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py precious` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_SilverOre`/`MMT_GoldOre` 资源和 2 个矿脉定义完整。
- 2 个冶炼配方挂载手动熔炉，产物为原版 `Silver`/`Gold`。
- 6 张已授权贴图按映射复制完成。
- `precious` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
