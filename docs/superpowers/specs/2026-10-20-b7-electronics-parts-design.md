# B7 电子零部件 设计

- 日期：2026-10-20
- 状态：已审阅（2026-10-20 用户确认：拆解护盾做、修理包跳过）
- 引用：`2026-10-20-b2-b8-batch-roadmap.md` §7；依赖 B4（超轻/高碳合金类别）、B5、B6（磁性材料，已完成）
- HSK 数值已实地核对（Core_SK 1.7-super-unstable）

## 1. 已确认决策（无需再问）

1. **先进零部件 ComponentAdvanced 不自建**：用户 WIP 已落定——既有 `MMT_MakeAdvMechanism_*` 用原版 `ComponentSpacer`（太空零件）替代，仓库已无 `ComponentAdvanced` 引用。B7 新配方中 HSK 的 ComponentAdvanced 一律映射原版 ComponentSpacer；拆解护盾产出同步映射。
2. **护盾输入只保留原版**：HSK 的 `Apparel_ShieldGuardian` 是 SK 自定义，原版仅有 `Apparel_ShieldBelt`。
3. **新贴图复制**（HSK 已授权）：`Things/Item/HexCell.png`、`Things/Item/Resource/RobotParts.png`、`Things/Item/Resource/MineralSonarModule.png`。

## 2. 类别补齐（改动既有资源，无新文件）

| 资源 | 现状 | 改为 |
|---|---|---|
| MMT_ComponentMedieval / MMT_Wire / MMT_Mechanism / MMT_AdvMechanism / MMT_SpacerComponent / MMT_UltraComponent | ResourcesRaw | MMT_Parts |
| MMT_ElectronicParts / MMT_IntegratedCircuit / MMT_Microchip | ResourcesRaw | MMT_ElectronicsCat |
| MMT_Silicon | ResourcesRaw | 不变（原料） |

影响面：
- `MMT_ConvertResourcesRaw`（[ResourcesRaw]×300）不再吃零部件/电子件——与 HSK 一致（HSK 零部件不在 ResourcesRaw）。
- 已 grep 确认无配方以 `categories=ResourcesRaw` 引用这些物品作为特定输入。
- `validate_parts`/`validate_electronics` 的类别断言同步参数化更新。

## 3. 新资源（3 个，新建 `MMT_Items_Resource_Robotics.xml`）

| defName | label | 关键数值（照 HSK） | 类别 | 贴图 |
|---|---|---|---|---|
| MMT_Hexcell | 动力电池 | MV150 stack75→50 HP10 Det0.02 Flam5 Mass0.1 useHitPoints；爆炸 comp Flame r1.4 +0.2/stack（原版 comp）；Spacer | MMT_ElectronicsCat（HSK 误归 CTextiles，修正） | HexCell.png（Single） |
| MMT_RobotParts | 机器人部件 | MV70 stack75 Flam1 Mass0.2 tradeTags Exotic | MMT_ElectronicsCat（HSK 归 ElectronicsCat） | RobotParts.png |
| MMT_MineralSonarModule | 矿物声纳模块 | MV300 stack25 Flam0.1 Mass6 | MMT_ElectronicsCat | MineralSonarModule.png |

剔除规则同 B6（无 Bulk 等 CE/HSK 字段；RobotParts 的 tradeTags `Robots` 是 SK 语义，只留 Exotic）。

## 4. 新配方（4 条，新建 `MMT_Recipes_Robotics.xml`）

| defName | 输入 | 产出 | 工作量 | 技能 | 挂载 | 研究 |
|---|---|---|---|---|---|---|
| MMT_MakeHexcell | MMT_MagneticMaterial×2 + [MMT_HCMBar]×4 + MMT_Microchip×1 | MMT_Hexcell×10 | 6000 | 14 | MMT_ElectricElectronicsTable recipes | MMT_Robotics1 |
| MMT_MakeRobotParts | [MMT_USLDBar]×60 + MMT_AdvMechanism×15 | MMT_RobotParts×25 | 8000 | 15 | MMT_ElectricElectronicsTable recipes | MMT_Robotics1 |
| MMT_MakeMineralSonarModule | [MMT_MetallicBar]×25 + ComponentSpacer×5 + MMT_ElectronicParts×10 + MMT_Mechanism×1 | MMT_MineralSonarModule×1 | 4800 | 10 | MMT_ElectricAssemblyBench + MMT_ElectricElectronicsTable recipes | MMT_AdvancedComponents |
| MMT_DeconstructPersonalShield（待定） | Apparel_ShieldBelt×1（原版） | ComponentSpacer×8 + MMT_IntegratedCircuit×1 | 1000 | 10 | recipeUsers=MMT_RoboticAssembler | MMT_AdvancedComponents |

- 对照：HSK `MakeHexcell`（Droids_D1/技能14）、`MakeRobotParts`（Droids_D1/技能15）、`MakeMineralSonarModule`（Mining_C1/技能10，挂 AdvToolBench+TableElectronics）、`DeconstructPersonalShield`（回收站，无研究）。
- workSpeedStat 均用 ElectronicCraftingSpeed（1.7 原版有，HSK 同款）；effectWorking=Smith、soundWorking=Recipe_Machining。
- 声纳模块的 HSK 前置"采矿学 II"在 MMT 无对应线，映射 MMT_AdvancedComponents。
- 挂载方式：HSK 挂建筑 recipes 列表的，MMT 同样扩展建筑 recipes 列表（电动电子台/电动装配台），不写 recipeUsers；拆解护盾对照 HSK 挂单台建筑，用 recipeUsers 写 MMT_RoboticAssembler。

## 5. 新研究（1 个，追加 `MMT_Research_Electronics.xml`）

- `MMT_Robotics1` 机器人工程 I：baseCost 2000，Spacer，前置 MMT_BasicElectronics，坐标 (22.00, 1.00)（对照 HSK Droids_D1：2000/Spacer/前置 Electronics_D1；现有坐标最大 21,2，无冲突）。
- INFRA_RESEARCH_TOTAL 25→26。

## 6. 验证（扩展现有套件，路线图指定）

- `validate_parts`：PARTS_RESOURCE_EXPECTATIONS 各项加 `category` 断言（MMT_Parts）。
- `validate_electronics`：电子件类别断言改 MMT_ElectronicsCat；新增 3 资源（贴图/字段）、4 配方（输入产出/挂载/研究）、1 研究断言；resource_names/VANILLA_THINGS 补 Apparel_ShieldBelt。
- 建筑 recipes 列表断言更新：电动电子台 +3（Hexcell/RobotParts/声纳），电动装配台 +1（声纳）。

## 7. 不做（范围外）

- 先进零部件 MMT_ComponentAdvanced（决策 1，用原版太空零件替代）。
- 修理包 MendingKit（依赖 HSK 修理台体系 Repair_table_B1，MMT 无修理链——**待确认**，推荐跳过）。
- 武器部件/织物（B8）、CE 物品。

## 8. 待用户确认事项

1. §4 拆解护盾配方：做（挂自动化装配器，护盾腰带×1→太空零件×8+集成电路×1）还是跳过？
2. §7 修理包：跳过（推荐）还是做（需自设挂载与研究，无 HSK 体系支撑）？
