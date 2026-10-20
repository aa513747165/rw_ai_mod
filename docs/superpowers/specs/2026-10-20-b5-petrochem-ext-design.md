# B5 石化聚合物设计 Spec

- 日期：2026-10-20
- 状态：已审阅（随实施计划一并批准）
- 依据：`docs/superpowers/specs/2026-10-20-b2-b8-batch-roadmap.md` 第 5 节；全局约定见该文件第 0 节
- HSK 数值来源：`Hardcore-SK/Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml`、`Recipes_Ammo.xml`、`Recipes_Materials_Alloys.xml`、`Recipes_RocketFuel.xml`（Odyssey 模块）及对应 ThingDef 文件，均已实地核对
- 前置：B1–B4 已完成

## 1. 范围与决策（路线图"spec 定"项落点）

| 项 | 决策 |
|---|---|
| 碳纤维锭 CarbonAlloy | 新增资源+配方（人造纤维×10+碳聚物×10→×10 @ 电石化台）；归 `MMT_USLDHBar`；**并把 `MMT_MakePobedite`/`MMT_MakeFerrotitanium` 的碳输入 `MMT_Carbon×3` 对齐为 `MMT_CarbonAlloy×3`**（已核实当前仍是 MMT_Carbon，用户 WIP 未改） |
| 复合粘剂 Compaste | 新增；**纺织机本批提前建** `MMT_Loom`，B8 直接用；Loom 研究前置用原版 `ComplexClothing`，不占新研究名额 |
| 表面活性剂 Surfactant | HSK 是 CE 弹药 Def（FirefoamFuelBase）——MMT 简化为普通资源；两条配方：酒精路线（盐10+硫磺15+[Alcohol]15→50，recipeUsers 挂原版 DrugLab+Brewery）与复合粘剂路线（粘剂15+聚合物25→50，仅 DrugLab）；椰子替代配方等 B8 |
| 石蜡 Paraffins | HSK 仅转化器（B6）产出，等不及——**MMT 自建**：原油×20→石蜡×10，挂手/电石化台（无 HSK 参照，自平衡） |
| 硫酸盐 Sulphates | 同上——**MMT 自建**：硫磺×10→硫酸盐×10，挂手/电石化台；MMT 现有塑料/橡胶配方用的是 `MMT_Sulfur`（硫磺），不改动 |
| 化合燃料 Chemfuel | **直接用原版 `Chemfuel` + 原版生物燃料精炼厂**，MMT 零改动；HSK 食物直炼配方在当前分支是孤儿配方（无建筑引用），不复制 |
| 火药/凝固汽油/硫磺配方 | 新增 `MMT_AmmoBench` 弹药工作台（对照 HSK TableHeavyAmmunition），研究前置用原版 `Machining`；新增硝石资源+硝石矿脉 |
| 氧化化合燃料 OxidizedChemfuel | 去 Prometheum/FSX → 替代方案 `Chemfuel×100 + 硝石×15 + 硫酸盐×15 + 聚合物×5 → ×75` @ 电石化台 |
| 研究 | 新增 2 个：`MMT_AdvancedPetrochemistry`（1500，Industrial，前置 MMT_ElectricPetrochemistry，坐标 16.00/1.00）、`MMT_OxidizedFuel`（2500，Spacer，前置前者，坐标 17.00/2.00）——坐标已核实不与既有 20 个研究重叠 |

## 2. SK/CE 内容剔除

- 表面活性剂不做 CE 弹药（AmmoDef/FirefoamFuelBase/ammoClass 全部去掉），普通 ResourceBase
- 爆炸 comp（CompProperties_Explosive）一律不加（MMT 既有资源均无）
- `SmithingSpeed` 在 1.7 不存在 → HSK MakeSulfur 改用 `SmeltingSpeed`
- SK.CompProperties_IdlePower 不用 → 用原版 CompProperties_Power（无 idle 功耗）
- stuff categories 仅 `Metallic`；碳纤维锭 statFactors 只保留原版 stat（MaxHitPoints 3.6/Beauty 1/Flammability 0/WorkToMake 2.5/WorkToBuild 2.5/Mass 0.8/MarketValue 1/DoorOpenSpeed 0.8）

## 3. HSK 参考数值（实地核对）

配方：MakeCarbonAlloy（纤维10+碳10→10，2800，Crafting15）、MakeCompaste（化合燃料10+橡胶5+石蜡2→20，3200，DrugCookingSpeed，Crafting5）、MakeSurfactant（盐10+硫磺15+酒15→50，4000，Crafting8）、MakeSurfactant_Polymers（粘剂15+聚合物25→50，4000，Crafting12+Intellectual4，仅 DrugLab）、MakeSulfur（铁35→硫磺10，1200，无技能无研究）、MakePowder（硝石75+[煤]20+硫磺20→20，3000，Crafting3，targetCountAdjustment 5）、MakeNapalm（化合燃料30+聚合物15→10，1200，Crafting5）、MakeOxidizedChemfuel（化合燃料100+硝石10+硫酸盐10+Prometheum/FSX5→75，10000，DrugSynthesisSpeed，Crafting12）。

资源 MarketValue/stackLimit：石蜡 14/150、硫酸盐 7/300、活性剂 0.4/150、复合粘剂 5/300、火药 25/150、凝固汽油 5/300、硝石 3/300、氧化化合燃料 9/375、碳纤维锭 40/75。

矿脉 MineableNitre：yield 75，HP 1800，commonality 1.05，lump 8~16，color (124,124,110)/(175,175,165)。

建筑：TableHeavyAmmunition size (3,1) 500W、stuff 210+零部件10+电子元件5+机械组件10、WorkToBuild 6500、HP 300、建造技能 10；TableLoom size (2,2) 300W、stuff 125+原始零部件10+电子元件3+机械组件6、WorkToBuild 3850、HP 170、建造技能 7。

## 4. 类别归属

硝石/石蜡/硫酸盐/表面活性剂/复合粘剂/火药 → `ResourcesRaw`（与既有石化资源一致）；凝固汽油/氧化化合燃料 → `MMT_FuelCat`；碳纤维锭 → `MMT_USLDHBar`（路线图指定，HSK 归 ConstructionParts 不采用）。

## 5. 贴图

复制 HSK 已授权静态贴图：物品 8 张（Paraffinwax/Sulphate/SurfactantFuel/CompoundPaste/Powder/NapalmCannister/Nitre/OxFuel，后者在 Core_SK_DLCModule/Odyssey）→ `1.7/Textures/Things/Item/MMT/MMT_PetroExt/`（Graphic_Single 模式）；建筑 12 张（Loom、TableHeavyAmmunition 各 6 张含 m 掩码）→ `Textures/Things/Building/Production/MMT_Loom/`、`MMT_AmmoBench/`。碳纤维锭复用 `MMT_Ingot` + color (130,130,130)。

## 6. 验证

新增 `petrochem-ext` 套件（资源 9 + 矿脉 1 + 建筑 2 + 配方 10 + 研究 2）；扩展 `PETROCHEM_TABLE_RECIPES`（+2）与 `ELECTRIC_PETROCHEM_LAB_RECIPES`（+4）；`ADVANCED_ALLOYS_RECIPE_EXPECTATIONS` 两条碳输入改 `MMT_CarbonAlloy`；`VANILLA_THINGS` 加 `Chemfuel`。收尾 `validate all` 全绿 + 重跑对比文档。

## 7. 不做

不改既有 5+5 石化配方；不做 HSK 食物直炼化合燃料（孤儿配方）；不做椰子版活性剂（B8）；不做 VulcanizeRubber（MMT 橡胶配方已存在）；不碰深钻井产出表；纯 XML 无 C# 改动。
