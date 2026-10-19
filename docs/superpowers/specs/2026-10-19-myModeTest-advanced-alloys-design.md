# myModeTest 高级合金链设计

日期：2026-10-19
状态：已获用户确认（方案 A：两级研究 + 全挂电弧炉）

## 1. 目标

在 myModeTest 中实现完整的高级合金资源链（对标 HSK Hightech Alloys），覆盖：

- 3 种新矿石（镍矿、钛铁矿、钨锰铁矿）及自设计矿脉；
- 3 种中间金属（镍锭、钴、铬）；
- 5 种高级合金（钨、钛、司太立、镍钛诺、胜利合金）+ 铁钛合金（产出原版玻璃钢 Plasteel）；
- 9 个电弧炉配方 + 2 个新研究。

Mod 独立实现，不依赖 HSK。

## 2. 参考基准

- HSK 本地浅克隆 `D:\project\rw\Hardcore-SK`，HEAD `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 1.7-super-unstable），禁止访问远程。
- 主要来源文件：
  - `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml`（Nickel / Ilmenite / Wolframite）
  - `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Alloys_Industrial.xml`（Chromium / NickelBar / Cobalt / CarbonAlloy）
  - `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Alloys_Hightech.xml`（Tungsten / Titanium / StelliteAlloy / NitinolAlloy / PobediteAlloy）
  - `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml`（MineableNickel / MineableTitanium / MineableTungsten）
  - `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml`（全部配方）
  - `Mods/Core_SK/Defs/ResearchProjectDefs/Research_Craft.xml`（Metals_C3 / Metals_D1）

## 3. 关键决策

| 决策点 | 结论 |
|---|---|
| 实施范围 | 一次性全做（用户选定） |
| 铁钛合金产出 | 原版 `Plasteel`（用户选定，需加入验证器 `VANILLA_THINGS` 白名单） |
| 矿脉 | 自设计，沿用现有 `RockBase` + 原版 `RockFlecked_Atlas` 模式（用户选定） |
| 研究分层 | 两级：精炼（Industrial）→ 高级合金（Spacer），对标 HSK Metals_C3 → Metals_D1 |
| 配方挂点 | 全部挂现有 `MMT_ElectricArcFurnace`，零新建筑（与 HSK 一致） |
| 手动版本 | 不做（HSK 中这些配方本就只在电弧炉） |
| 碳纤维合金 | 不新造，胜利/铁钛配方直接使用现有 `MMT_Carbon`（碳材料，YAGNI） |
| HSK 自定义 ThingCategory | HCM/RAR/RARBar/USLDBar 等一律替换为 `ResourcesRaw`（沿用本 Mod 惯例） |
| HSK 自定义 stuff 分类 | SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic → 仅 `Metallic`（1.7 原版安全集） |
| 贴图 | 矿石贴图复制 HSK（已授权）；锭/合金复用现有 `MMT_Ingot` 贴图改色；矿脉用原版贴图零复制 |

## 4. 资源定义

### 4.1 矿石（新文件 `MMT_Items_Resource_AdvancedOre.xml`）

沿用现有矿石模式（`ResourceBase` + `Graphic_StackCount` + `ResourcesRaw`，堆叠 600/300）。稀有矿石对标 HSK 价值与堆叠：

| defName | label | 颜色 | MarketValue | Mass | stackLimit |
|---|---|---|---|---:|---:|
| `MMT_NickelOre` | 镍矿石 | (42,208,206) | 4.0 | 0.4 | 300 |
| `MMT_IlmeniteOre` | 钛铁矿石 | (160,178,195) | 8.0 | 0.15 | 300 |
| `MMT_WolframiteOre` | 钨锰铁矿石 | (230,230,255) | 9.0 | 0.2 | 300 |

注：HSK 价值为 8/16/18，本 Mod 现有普通矿石统一 2.0，为保持一致性按一半缩放为 4/8/9（仍显著高于普通矿石，体现稀有度）。

贴图（复制 HSK，9 张）：

- `Things/Item/Ores/Nickel/Nickel_{a,b,c}.png` → `Things/Item/MMT/MMT_Ore/MMT_NickelOre_{a,b,c}.png`
- `Things/Item/Ores/Ilmenite/Ilmenite_{a,b,c}.png` → `MMT_IlmeniteOre_{a,b,c}.png`
- `Things/Item/Ores/Wolframite/Wolframite_{a,b,c}.png` → `MMT_WolframiteOre_{a,b,c}.png`

texPath 分别为 `Things/Item/MMT/MMT_Ore/MMT_NickelOre` 等。

### 4.2 中间金属与高级合金（新文件 `MMT_Items_Resource_AdvancedAlloys.xml`）

统一模式：`ResourceBase` + `Things/Item/MMT/MMT_Ingot/MMT_Ingot` 贴图（复用现有）+ `Graphic_StackCount` + `useHitPoints` + `ResourcesRaw` + stuffProps（仅 `Metallic` 分类）+ `terrainAffordanceNeeded Medium`。

| defName | label | 颜色 | MarketValue | stackLimit | stuff statFactors（裁剪为 1.7 安全集） |
|---|---|---|---|---:|---|
| `MMT_NickelBar` | 镍锭 | (42,208,206) | 6.0 | 150 | MaxHP 2.8 / Beauty 2.4 / WorkToMake 2.0 / WorkToBuild 2.0 / Mass 1.35 / MarketValue 0.7 |
| `MMT_Cobalt` | 钴锭 | (60,90,120) | 10.0 | 150 | 不带 stuffProps（HSK 亦然，仅作合金原料） |
| `MMT_Chromium` | 铬锭 | (235,235,235) | 5.0 | 150 | 不带 stuffProps（HSK 亦然） |
| `MMT_Tungsten` | 钨合金 | (240,230,230) | 40.0 | 150 | 不带 stuffProps（HSK 的 Tungsten 无 stuffProps） |
| `MMT_Titanium` | 钛合金 | (160,178,181) | 45.0 | 75 | MaxHP 3.3 / Beauty 1.8 / Flammability 0 / WorkToMake 3.0 / WorkToBuild 3.0 / DoorOpenSpeed 1.3 / Mass 0.8，statOffsets Beauty 3 |
| `MMT_StelliteAlloy` | 司太立合金 | (125,146,178) | 30.0 | 150 | MaxHP 3.55 / Beauty 1.5 / WorkToMake 2.0 / WorkToBuild 2.2 / DoorOpenSpeed 0.95 / Mass 1.24 |
| `MMT_NitinolAlloy` | 镍钛诺合金 | (175,175,175) | 27.0 | 150 | MaxHP 3.6 / Beauty 1.6 / WorkToMake 2.6 / WorkToBuild 2.4 / DoorOpenSpeed 1.2 / Mass 0.95，statOffsets Beauty 3 |
| `MMT_PobediteAlloy` | 胜利合金 | (85,85,85) | 33.0 | 150 | MaxHP 4.0 / Beauty 1.1 / WorkToMake 2.8 / WorkToBuild 2.8 / DoorOpenSpeed 0.7 / Mass 1.4 |

- 统一基础属性：DeteriorationRate 0、Flammability 0、soundInteract/soundDrop `Metal_Drop`。
- stuffProps 内统一：categories 仅 `Metallic`、constructEffect `ConstructMetal`、stuffAdjective 中文（镍制/钛制/司太立制/镍钛诺制/胜利制）、color 同上、Flammability 0。
- 不带 stuffProps 的 3 种（钴/铬/钨）不设 useHitPoints（与 HSK 一致），其余设 `useHitPoints true`。
- HSK 的 Armor/Insulation/伤害倍率类 StuffPower 属性本包不移植（本 Mod 尚无武器/护甲内容，YAGNI；后续做装备包时再补）。

### 4.3 原版产出物

- `Plasteel`（玻璃钢）：原版 Def，无需新建；需加入验证器 `VANILLA_THINGS` 白名单。

## 5. 矿脉（新文件 `MMT_Buildings_AdvancedRocks.xml`）

沿用 `MMT_Buildings_Metal.xml` 矿脉模式：`RockBase` + 原版 `Things/Building/Linked/RockFlecked_Atlas` + `Graphic_Single` + `CutoutComplex` + isResourceRock/veinMineable。数值取 HSK 原值：

| defName | label | 颜色 color / colorTwo | MaxHitPoints | mineableThing | mineableYield | ScatterCommonality | LumpSizeRange |
|---|---|---|---:|---|---:|---:|---|
| `MMT_MineableNickelOre` | 镍矿脉 | (88,115,175) / (42,208,206) | 10000 | MMT_NickelOre | 50 | 0.4 | 5~10 |
| `MMT_MineableIlmenite` | 钛铁矿脉 | (160,178,181) / (160,178,195) | 13000 | MMT_IlmeniteOre | 75 | 0.25 | 3~9 |
| `MMT_MineableWolframite` | 钨锰铁矿脉 | (88,115,175) / (230,230,255) | 20000 | MMT_WolframiteOre | 50 | 0.15 | 3~9 |

注：HP 沿用 HSK（镍 10000 / 钛铁 13000 / 钨锰 20000），高于本 Mod 现有普通矿脉（1300-1800）是 HSK 刻意设计（稀有矿难挖），保持不动；列入延期平衡观察项。

## 6. 配方（新文件 `MMT_Recipes_AdvancedAlloys.xml`）

统一：workSpeedStat `SmeltingSpeed`、effectWorking `Smelt`、soundWorking `Recipe_Smelt`、workSkill Crafting、workSkillLearnFactor 0.8；不写 recipeUsers，挂载到 `MMT_ElectricArcFurnace`（修改 `MMT_Buildings_Electric.xml` 的 recipes 列表追加 9 项）。

### 6.1 精炼层（研究前置 `MMT_AdvancedMetalRefining`）

| defName | label | 输入 → 产出 | workAmount | 技能 |
|---|---|---|---:|---:|
| `MMT_SmeltNickelBar` | 冶炼镍锭 | MMT_NickelOre 15 → MMT_NickelBar 15 | 1500 | 13 |
| `MMT_ExtractCobalt` | 提取钴 | MMT_NickelOre 30 → MMT_Cobalt 15 | 1300 | 14 |
| `MMT_ExtractChromium` | 提取铬 | MMT_IronOre 25 → MMT_Chromium 10 | 1200 | 11 |

### 6.2 高级合金层（研究前置 `MMT_AdvancedAlloys`）

| defName | label | 输入 → 产出 | workAmount | 技能 |
|---|---|---|---:|---:|
| `MMT_SmeltTungsten` | 冶炼钨合金 | MMT_WolframiteOre 15 → MMT_Tungsten 15 | 3000 | 17 |
| `MMT_SmeltTitanium` | 冶炼钛合金 | MMT_IlmeniteOre 15 → MMT_Titanium 15 | 3300 | 16 |
| `MMT_MakeStellite` | 合炼司太立合金 | MMT_Cobalt 5 + MMT_Chromium 13 + MMT_Tungsten 2 → MMT_StelliteAlloy 20 | 2100 | 17 |
| `MMT_MakeNitinol` | 合炼镍钛诺合金 | MMT_Titanium 4 + MMT_NickelBar 11 → MMT_NitinolAlloy 15 | 2600 | 16 |
| `MMT_MakePobedite` | 合炼胜利合金 | MMT_Tungsten 3 + MMT_Carbon 3 + MMT_Cobalt 3 → MMT_PobediteAlloy 10 | 2700 | 18 |
| `MMT_MakeFerrotitanium` | 合炼铁钛合金 | MMT_Titanium 5 + Steel 12 + MMT_Carbon 3 → Plasteel 15 | 2400 | 15 |

数值全部取 HSK 原值（工作量/技能/输入产出比）。

## 7. 研究（新文件 `MMT_Research_AdvancedAlloys.xml`）

| defName | label | baseCost | techLevel | 前置 | 坐标 |
|---|---|---:|---|---|---|
| `MMT_AdvancedMetalRefining` | 高级金属精炼 | 1600 | Industrial | MMT_ElectricSmelting | 12.00, 2.00 |
| `MMT_AdvancedAlloys` | 高级合金 | 2300 | Spacer | MMT_AdvancedMetalRefining | 13.00, 3.00 |

- 坐标已核对与全部现有研究无冲突（(12,x) 与 (13,3) 均空闲）。
- Spacer 级研究游戏内需多元分析器（原版本体建筑，无新增依赖），对标 HSK Metals_D1 的 Spacer 定位。

## 8. 文件清单

新增（自动 add git）：

1. `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedOre.xml`（3 矿石）
2. `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_AdvancedAlloys.xml`（8 金属/合金）
3. `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_AdvancedRocks.xml`（3 矿脉）
4. `1.7/Defs/RecipeDefs/MMT_Recipes_AdvancedAlloys.xml`（9 配方）
5. `1.7/Defs/ResearchProjectDefs/MMT_Research_AdvancedAlloys.xml`（2 研究）
6. `1.7/Textures/Things/Item/MMT/MMT_Ore/` 新增 9 张矿石贴图

修改：

7. `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml`（电弧炉 recipes 追加 9 项）
8. `tools/validate_mod.py`（新增 `advanced-alloys` 套件并纳入 `all`；`VANILLA_THINGS` 增加 `Plasteel`）

## 9. 验证设计

新套件 `advanced-alloys`：

- 研究检查：2 研究的 label/baseCost/techLevel/前置/坐标逐项断言。
- 资源检查：11 个 ThingDef 的 defName/label/颜色/价值/堆叠/stuff 分类断言（仅 Metallic）；钴/铬/钨断言无 stuffProps。
- 矿脉检查：3 矿脉 mineableThing/Yield/HP/常见度逐项断言。
- 配方检查：多输入元组比较 + fixedIngredientFilter 扁平化集合比较；统一断言 SmeltingSpeed/Smelt/Recipe_Smelt；研究前置按层断言；挂载断言（电弧炉 recipes 含全部 9 项）；输入闭包 = 高级合金资源 ∪ 金属资源 ∪ 石化资源（MMT_Carbon）∪ VANILLA_THINGS（Steel/Plasteel）。
- PNG 检查：9 张新矿石贴图。
- `SK.` 扫描 + 1.7 不存在字符串扫描（ElectronicCraftingSpeed/SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic 等）。
- 最终：`python tools/validate_mod.py all`（XML 计数 45 → 50）+ `dotnet build --no-restore`（0 警告 0 错误）。

## 10. 约束

- 自定义 Def 全部 `MMT_` 前缀；中文 label/description，英文 defName。
- 不引用 `SK.` 类型与 HSK 专属 Def/分类。
- 所有文本文件 UTF-8。
- 只提交本地 Git，不 push。
- 本次不启动 RimWorld，不声称游戏内加载通过。

## 11. 延期项（记录，不在本包处理）

- 稀有矿脉 HP（10000-20000）远高于本 Mod 普通矿脉，待实机评估挖掘时长平衡。
- 高级合金 StuffPower（护甲/伤害倍率）未移植，待装备包。
- 矿石/合金 MarketValue 缩放系数（HSK 一半）待实机评估。
- AlphaPoly/BetaPoly（Ultra 级人造物质）不移植（依赖 HSK 物质装配机体系）。
