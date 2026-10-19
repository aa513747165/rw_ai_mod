# myModeTest 基础零件家族（Parts）链设计

日期：2026-10-19
状态：已获用户确认（原版映射方案 + 三座新建筑 + 三级研究 + 类别输入具体物品映射方案 A）

## 1. 目标

在 myModeTest 中实现完整的 HSK 基础零件家族（`Manufactured/Parts`）资源链，覆盖：

- 6 种自造零件资源（原始零件、导线、机构件、高级机构件、太空零件、超级零件）；
- 2 种原版零件的生产配方（工业零件 `ComponentIndustrial`、高级零件 `ComponentAdvanced`）；
- 3 座新建筑（手动装配台、电动装配台、机器人装配台）；
- 3 级研究（零件加工 → 高级零件 → 超级零件）；
- 14 个工作台配方；
- 静态验证扩展。

Mod 独立实现，不依赖 HSK。

## 2. 参考基准

- HSK 本地浅克隆 `D:\project\rw\Hardcore-SK`，HEAD `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 1.7-super-unstable），禁止访问远程。
- 主要来源文件：
  - `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Parts.xml`（ComponentMedieval / Wire / Mechanism / AdvMechanism / ComponentUltra）
  - `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs/Items_Resource_Manufactured.xml`（原版 ComponentIndustrial / ComponentSpacer 数值补丁）
  - `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Production_Components.xml`（AdvToolBench / RobAssem）
  - `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Production.xml`（原版 FabricationBench 补丁）
  - `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml`（零件配方）
  - `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml`（工业/太空零件配方补丁）
  - `docs/hsk-resource-production-paths.md` 第 625–657、882–895、1125–1137、1343–1352、1435–1470 行（已提取的生产路径）

## 3. 关键决策

| 决策点 | 结论 |
|---|---|
| 原版映射 | 工业零件产出原版 `ComponentIndustrial`（白名单已有）；高级零件产出原版 `ComponentAdvanced`（需加入验证器 `VANILLA_THINGS`，与 Plasteel 先例一致）；其余 6 种自造 `MMT_`（用户选定） |
| 工作台 | 新建 3 座建筑完全对标 HSK：手动装配台（对标补丁后原版 FabricationBench）、电动装配台（对标 AdvToolBench）、机器人装配台（对标 RobAssem）（用户选定） |
| 研究分层 | 三级完全对标 HSK：零件加工（对标 Craft_0）→ 高级零件（对标 Components_D1）→ 超级零件（对标 Components_E1）（用户选定） |
| 类别输入映射（方案 A） | `[SLDBar]`→原版 `Steel`；`[USLDBar]`→原版 `Plasteel`；`[HCMBar]`→`MMT_CopperBar`；`[PRSBar]`→原版 `Silver`/`Gold` 二选一 filter；`[RARBar]`→`MMT_Tungsten`/`MMT_Titanium` 二选一；`[USLDHBar]`→`MMT_StelliteAlloy`/`MMT_PobediteAlloy` 二选一（用户选定） |
| 配方数值 | 照抄 HSK（投入/产出/工作量/技能要求不变），与混凝土等前链惯例一致 |
| HSK 自定义 ThingCategory | `Parts` 等一律替换为 `ResourcesRaw`（沿用本 Mod 惯例） |
| HSK 自定义 stuff 分类 | `RuggedMetallic` 等 → 仅 `Metallic`（1.7 原版安全集） |
| RobAssem 动画 | 弃用 HSK `SK.Building_AnimatedWorktable` 自定义类与 26 帧动画，使用普通 `Building_WorkTable` + `Graphic_Single` 静帧 |
| 非配方途径 | 不做：HSK 摧毁残留、Ideology 矿点产出（属事件/ DLC 机制，非本链范畴） |
| Pipe（钻杆） | 不做：属石油链范畴 |
| 贴图 | 全部从 HSK 本地复制（沿用已授权惯例），含蒙版 `*m.png` |

## 4. 资源定义（新文件 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Parts.xml`）

沿用现有资源模式（`ResourceBase` + `Graphic_Single` + `ResourcesRaw` + `tickerType Rare`）。数值照抄 HSK：

| defName | label | MarketValue | Mass | stackLimit | techLevel | Flammability | 贴图（HSK 源） |
|---|---|---:|---:|---:|---|---:|---|
| `MMT_ComponentMedieval` | 原始零件 | 6 | 1.5 | 300 | Neolithic | 0.2 | `Components/ComponentMedieval.png` |
| `MMT_Wire` | 导线 | 1.5 | 0.2 | 600 | Industrial | 0.1 | `Components/Wires.png` |
| `MMT_Mechanism` | 机构件 | 200 | 0.65 | 75 | Industrial | 1.0 | `Components/Mechanism.png` |
| `MMT_AdvMechanism` | 高级机构件 | 400 | 0.7 | 75 | Spacer | 1.0 | `Components/AdvMechanism.png` |
| `MMT_SpacerComponent` | 太空零件 | 150 | 0.5 | 150 | Spacer | 0.3 | `Components/AdvancedComponent.png` |
| `MMT_UltraComponent` | 超级零件 | 200 | 0.5 | 150 | Ultra | 0.3 | `Components/ComponentUltra.png` |

说明：

- 太空零件数值对标 HSK 补丁后的原版 `ComponentSpacer`（价值 150、Mass 0.5、堆叠 150、Spacer）；贴图用 HSK `AdvancedComponent.png`（HSK 太空零件即用它）。
- 机构件/高级机构件的 HSK `SellPriceFactor 0.70` 不抄（Mod 现有资源均未设该项，YAGNI）。
- 原版产出 `ComponentIndustrial` / `ComponentAdvanced` 无需任何 ThingDef，仅配方引用。

贴图复制目标目录：`1.7/Textures/Things/Item/MMT/MMT_Parts/`，文件名保持 HSK 原名，texPath 形如 `Things/Item/MMT/MMT_Parts/ComponentMedieval`。

## 5. 建筑定义（新文件 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml`）

沿用现有建筑模式（`BuildingBase` 父类、`Building_WorkTable`、`Graphic_Multi`/`CutoutComplex`、stuff `Metallic`、`designationCategory Production`、电力建筑带 Power/Flickable/Breakdownable comps）。数值对标 HSK：

| defName | label | 对标 | size | costStuffCount | costList | WorkToBuild | 其它关键数值 |
|---|---|---|---|---|---|---|---|
| `MMT_HandAssemblyBench` | 装配台 | HSK 补丁后原版 FabricationBench | (3,1) | 125 | `MMT_ComponentMedieval`×10 + `MMT_Mechanism`×5 | 1200 | `WorkTableWorkSpeedFactor` 0.9；MaxHitPoints 180；Cleanliness -8；无电力 comps |
| `MMT_ElectricAssemblyBench` | 电动装配台 | HSK `AdvToolBench` | (3,1) | 220 | `MMT_SyntheticFibers`×45 + `MMT_IntegratedCircuit`×10 + `ComponentIndustrial`×15 + `MMT_Mechanism`×10 | 5500 | `WorkTableWorkSpeedFactor` 1.5；MaxHitPoints 210；Cleanliness -2；耗电 750W |
| `MMT_RoboticAssembler` | 机器人装配台 | HSK `RobAssem` | (1,1) | 120 | `MMT_SyntheticFibers`×45 + `MMT_SpacerComponent`×10 + `MMT_AdvMechanism`×10 + `MMT_Microchip`×5 | 9000 | MaxHitPoints 200；Cleanliness -4；耗电 5000W；`Graphic_Single` |

映射说明：HSK AdvToolBench 成本中的 `Electronics`（集成电路）→ `MMT_IntegratedCircuit`（本 Mod 对应物）；RobAssem 成本中的 `Microchips` → `MMT_Microchip`、`ComponentSpacer` → `MMT_SpacerComponent`、`AdvMechanism` → `MMT_AdvMechanism`。

贴图复制（含蒙版，重命名为 MMT_ 前缀）：

- HSK `Textures/Things/Building/Production/FabricationBenchHand/FabricationBenchHand_{north,east,south}{,m}.png`（6 张）→ `1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench_{north,east,south}{,m}.png`
- HSK `.../FabricationBench/FabricationBench_{north,east,south}{,m}.png`（6 张）→ `.../MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench_{north,east,south}{,m}.png`（与 HSK AdvToolBench 一致，电动台复用该贴图）
- HSK `.../RoboticAssembler/AssemblerBench.png`（1 张静帧）→ `.../MMT_RoboticAssembler/MMT_RoboticAssembler.png`（弃用 26 帧动画与 ico）

`interactionCellOffset`：3×1 建筑沿用 `(0,0,-1)`（与 HSK 及本 Mod 现有 3×1 建筑一致）；1×1 机器人装配台用 `(0,0,-1)`（同 HSK RobAssem）。

## 6. 研究定义（新文件 `1.7/Defs/ResearchProjectDefs/MMT_Research_Parts.xml`）

| defName | label | techLevel | baseCost | prerequisites | researchView(X,Y) | 解锁内容 |
|---|---|---|---:|---|---|---|
| `MMT_ComponentCrafting` | 零件加工 | Industrial | 600 | 无（对标 HSK Craft_0 起点） | (15.00, 0.00) | 两座基础装配台 + 原始零件/工业零件/导线/机构件配方 |
| `MMT_AdvancedComponents` | 高级零件 | Spacer | 1600 | `MMT_ComponentCrafting` | (15.00, 1.00) | 高级零件/高级机构件/太空零件配方 |
| `MMT_UltraComponents` | 超级零件 | Spacer | 2500 | `MMT_AdvancedComponents` + `MMT_AdvancedAlloys` | (15.00, 2.00) | 机器人装配台 + 超级零件配方 |

坐标说明：现有占用列为 X=0..18 中 (15,0)/(15,1)/(15,2) 经核实未被占用（已统计全部现有研究坐标）。

## 7. 配方定义（新文件 `1.7/Defs/RecipeDefs/MMT_Recipes_Parts.xml`，共 14 个）

沿用现有配方写法（`ingredients`+`fixedIngredientFilter`、filter 多选一时列多个 thingDefs）。数值照抄 HSK：

| 配方 defName | 输入 | 产出 | workAmount | 技能要求 | 研究前置 | 工作台 |
|---|---|---|---:|---|---|---|
| `MMT_MakeComponentMedieval_Wood` | `WoodLog`×20 | `MMT_ComponentMedieval`×10 | 900 | — | ① | 两座装配台 |
| `MMT_MakeComponentMedieval_Metal` | `Steel` 或 `MMT_CopperBar`×10 | `MMT_ComponentMedieval`×10 | 900 | — | ① | 两座装配台 |
| `MMT_MakeComponentIndustrial_Hand` | `Steel` 或 `Plasteel`×30 | `ComponentIndustrial`×10 | 2000 | Crafting≥3 | ① | 手动 |
| `MMT_MakeComponentIndustrial_Electric` | 同上 | `ComponentIndustrial`×10 | 2000 | Crafting≥3 | ① | 电动 |
| `MMT_MakeWire_Hand` | `MMT_CopperBar`×10 | `MMT_Wire`×20 | 800 | Crafting≥3 | ① | 手动 |
| `MMT_MakeWire_Electric` | `MMT_CopperBar`×20 | `MMT_Wire`×40 | 800 | Crafting≥6 | ① | 电动 |
| `MMT_MakeMechanism_Hand` | `Steel`×30 + `ComponentIndustrial`×10 | `MMT_Mechanism`×5 | 1800 | Crafting≥4 | ① | 手动 |
| `MMT_MakeMechanism_Electric` | `Steel`×60 + `ComponentIndustrial`×20 | `MMT_Mechanism`×10 | 1800 | Crafting≥7 | ① | 电动 |
| `MMT_MakeComponentAdvanced_Hand` | `Plasteel`×30 | `ComponentAdvanced`×10 | 2500 | Crafting≥6 | ② | 手动 |
| `MMT_MakeComponentAdvanced_Electric` | 同上 | `ComponentAdvanced`×10 | 2500 | Crafting≥6 | ② | 电动 |
| `MMT_MakeAdvMechanism_Hand` | `Plasteel`×30 + `ComponentAdvanced`×10 + `MMT_SyntheticFibers`×5 | `MMT_AdvMechanism`×5 | 3200 | Crafting≥13 | ② | 手动 |
| `MMT_MakeAdvMechanism_Electric` | `Plasteel`×60 + `ComponentAdvanced`×20 + `MMT_SyntheticFibers`×10 | `MMT_AdvMechanism`×10 | 3200 | Crafting≥16 | ② | 电动 |
| `MMT_MakeSpacerComponent` | `Plasteel`×25 + `Silver` 或 `Gold`×5 | `MMT_SpacerComponent`×10 | 3500 | Crafting≥9 | ② | 电动 |
| `MMT_MakeUltraComponent` | `MMT_Tungsten` 或 `MMT_Titanium`×15 + `MMT_StelliteAlloy` 或 `MMT_PobediteAlloy`×10 + `Silver` 或 `Gold`×5 | `MMT_UltraComponent`×10 | 4500 | Crafting≥13 | ③ | 机器人装配台 |

①=`MMT_ComponentCrafting`，②=`MMT_AdvancedComponents`，③=`MMT_UltraComponents`。

说明：

- HSK 原始零件两个版本还挂在 CraftingSpot/锯木台上，本 Mod 无对应建筑，统一只挂两座装配台（差异已在本文档声明）。
- 电动/手动同名的 HSK 配方工作量相同，差异由工作台速度因子（0.9/1.5）体现，与 HSK 机制一致。
- `effectWorking`、`soundWorking` 等沿用现有零件类配方惯例（对标 `MMT_Recipes_Electronics.xml` 中同类配方）。
- 配方产品的 `MMT_` 与原版混用：`ComponentIndustrial`/`ComponentAdvanced`/`Plasteel`/`Steel`/`Silver`/`Gold`/`WoodLog` 均为原版物品。

## 8. 贴图复制清单（HSK → Mod，共 19 张）

物品（6 张，HSK `Mods/Core_SK/Textures/Things/Item/Components/` → `1.7/Textures/Things/Item/MMT/MMT_Parts/`）：

- `ComponentMedieval.png`、`Wires.png`、`Mechanism.png`、`AdvMechanism.png`、`AdvancedComponent.png`、`ComponentUltra.png`（文件名不变）

建筑（13 张，见第 5 节）：手动装配台 6 张（含蒙版）、电动装配台 6 张（含蒙版）、机器人装配台 1 张静帧。

## 9. 静态验证扩展（`tools/validate_mod.py`）

- `VANILLA_THINGS` 增加 `ComponentAdvanced`（`ComponentIndustrial`/`Plasteel`/`Steel`/`Silver`/`Gold`/`WoodLog` 已在列）。
- 新增 `PARTS_RESOURCE_EXPECTATIONS`：6 种资源的 label/价值/堆叠/贴图/graphic 类/时代（结构参照 `CONCRETE_RESOURCE_EXPECTATIONS`）。
- 新增零件链建筑/研究/配方期望表（结构参照现有各链验证段），覆盖：3 建筑 defName 与成本、3 研究 defName/费用/前置/坐标、14 配方的输入/产出/工作量/技能/研究/工作台归属。
- 验证方式：运行 `python tools/validate_mod.py` 全部通过。

## 10. 提交计划（git 分步，message 沿用现有风格）

1. `feat: 添加基础零件资源与贴图`（资源 xml + 6 张物品贴图）
2. `feat: 添加零件装配台建筑`（建筑 xml + 13 张建筑贴图）
3. `feat: 添加零件链研究与配方`（研究 xml + 配方 xml）
4. `test: 添加零件链静态验证`（validate_mod.py）

只提交本地仓库，不 push。

## 11. 明确不做

- HSK 摧毁残留（敌舰/反应堆巢/穿梭机）与 Ideology 矿点的零件产出途径；
- RobAssem 动画（SK 自定义 thingClass 与帧动画）；
- Pipe（钻杆，石油链范畴）；
- 原始零件在 CraftingSpot/锯木台的挂点（本 Mod 无对应建筑）；
- HSK 的 `SellPriceFactor`、`Bulk`（Combat Extended 属性）等本 Mod 未使用的字段。
