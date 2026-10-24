# 当前 MOD（MMT）资源生产链路 与 HSK 对比

- MMT 数据源：`1.7/Defs`（本 MOD）；HSK 数据源：[hsk-resource-production-paths.md](hsk-resource-production-paths.md)（`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`）。
- 由 `tools/build_mmt_chains_compare.js` 自动生成；MMT 侧使用 MOD 实际中文 label，HSK 侧沿用 MMT 对应物的中文名并附 defName。
- 匹配规则：产出物的 HSK 对应 def 相同，且首选投入/建筑别名一致者优先；`MMT 新增` 表示 HSK 无该资源或对应配方。
- 矿脉说明：原版钢/银/金/玻璃钢矿脉已由 `1.7/Patches/MMT_Patch_VanillaVeins.xml` 改造为 MMT 铁矿/银矿/金矿/钛铁矿（HSK 做法），不再作为 MMT def 出现在下表；零件矿脉照 HSK 保留原版。

## 1. 汇总

| 项 | 数量 |
| --- | --- |
| MMT 矿脉 | 12 |
| MMT 配方 | 299 |
| 与 HSK 数值一致 | 134 |
| 与 HSK 有数值差异 | 56 |
| HSK 有对应资源但无对应配方 | 5 |
| MMT 新增（HSK 无对应资源） | 104 |

## 2. 矿脉对比

| MMT 矿脉 | 产出 | MMT 单格产量 | MMT 矿团/散布/耐久 | HSK 对应矿脉 | HSK 单格产量 | HSK 矿团/散布/耐久 | 对比 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 镍矿矿藏(MMT_MineableNickelOre) | 镍矿 | 50 | 5~10 / 0.4 / 10000 | Nickel Ore Deposits(`MineableNickel`) | 50 | 5~10 / 0.4 / 10000 | 产量一致 |
| 钛铁矿(MMT_MineableIlmenite) | 钛铁矿 | 75 | 3~9 / 0.25 / 13000 | Ilmenite Ore Deposits(`MineableTitanium`) | 75 | 3~9 / 0.25 / 13000 | 产量一致 |
| 黑钨矿(MMT_MineableWolframite) | 黑钨矿 | 50 | 3~9 / 0.15 / 20000 | Wolframite Ore Deposits(`MineableTungsten`) | 50 | 3~9 / 0.15 / 20000 | 产量一致 |
| 压实的盐(MMT_MineableSalt) | 盐 | 150 | 5~10 / 1.2 / 900 | Compacted salt(`MineableSalt`) | 150 | 5~10 / 1.2 / 900 | 产量一致 |
| 粘土矿脉(MMT_MineableSoftClay) | 软粘土 | 75 | 8~15 / 0.7 / 1400 | — | — | — | MMT 新增资源 |
| 煤矿(MMT_MineableCoal) | 原煤 | 75 | 11~18 / 1.2 / 1500 | Coal Ore Deposits(`MineableCoal`) | 75 | 11~18 / 1.2 / 1500 | 产量一致 |
| 铜矿(MMT_MineableCopperOre) | 铜矿 | 75 | 15~25 / 0.95 / 1800 | Copper Ore Deposits(`MineableCopper`) | 75 | 15~25 / 0.95 / 1800 | 产量一致 |
| 锡矿(MMT_MineableTinOre) | 锡矿 | 75 | 9~17 / 1.1 / 1300 | Tin Ore Deposits(`MineableTin`) | 75 | 9~17 / 1.1 / 1300 | 产量一致 |
| 铅矾矿脉(MMT_MineableAnglesite) | 铅矾矿 | 75 | 6~13 / 0.8 / 1600 | Anglesite Deposits(`MineableLead`) | 75 | 6~13 / 0.8 / 6000 | 产量一致 |
| 铝土矿脉(MMT_MineableAluminium) | 铝土矿 | 75 | 11~19 / 0.65 / 1600 | Bauxite Ore Deposits(`MineableAluminium`) | 75 | 11~19 / 0.65 / 5500 | 产量一致 |
| 硝石矿脉(MMT_MineableNitre) | 硝石 | 75 | 8~16 / 1.05 / 1800 | Nitre Deposits(`MineableNitre`) | 75 | 8~16 / 1.05 / 1800 | 产量一致 |
| 硫磺矿脉(MMT_MineableSulfur) | 硫磺 | 75 | 10~16 / 0.8 / 1500 | — | — | — | HSK 无矿脉（仅深钻等） |

## 3. 配方对比（按 MMT 家族分组）

### 3.1 高级合金（AdvancedAlloys）

#### 提炼镍合金（15个）（`MMT_SmeltNickelBar`）

- **MMT**：镍矿×15 -> 镍合金×15 @ 电弧炉 ｜ 工作量=1500 ｜ 研究:高级金属精炼(MMT_AdvancedMetalRefining)，技能:Crafting≥13
- **HSK** `ProcessNickelOre`：镍矿(Nickel)×15 -> 镍合金(NickelBar)×15 @ 电弧炉(EAF) ｜ 工作量=1500 ｜ 研究:Metals_C3，技能:Crafting≥13
- **对比**：数值一致

#### 提炼钴合金（15个）（`MMT_ExtractCobalt`）

- **MMT**：镍矿×30 -> 钴合金×15 @ 电弧炉 ｜ 工作量=1300 ｜ 研究:高级金属精炼(MMT_AdvancedMetalRefining)，技能:Crafting≥14
- **HSK** `ProcessCobalt`：镍矿(Nickel)×30 -> 钴合金(Cobalt)×15 @ 电弧炉(EAF) ｜ 工作量=1300 ｜ 研究:Metals_C3，技能:Crafting≥14
- **对比**：数值一致

#### 提炼铬合金（10个）（`MMT_ExtractChromium`）

- **MMT**：铁矿石×25 -> 铬合金×10 @ 电弧炉 ｜ 工作量=1200 ｜ 研究:高级金属精炼(MMT_AdvancedMetalRefining)，技能:Crafting≥11
- **HSK** `ProcessForChromium`：铁矿石(Iron)×25 -> 铬合金(Chromium)×10 @ 电弧炉(EAF) ｜ 工作量=1200 ｜ 研究:Metals_C3，技能:Crafting≥11
- **对比**：数值一致

#### 提炼钨合金（15个）（`MMT_SmeltTungsten`）

- **MMT**：黑钨矿×15 -> 钨合金×15 @ 电弧炉 ｜ 工作量=3000 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥17
- **HSK** `ProcessTungsten`：黑钨矿(Wolframite)×15 -> 钨合金(Tungsten)×15 @ 电弧炉(EAF) ｜ 工作量=3000 ｜ 研究:Metals_D1，技能:Crafting≥17
- **对比**：数值一致

#### 提炼钛合金（15个）（`MMT_SmeltTitanium`）

- **MMT**：钛铁矿×15 -> 钛合金×15 @ 电弧炉 ｜ 工作量=3300 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥16
- **HSK** `ProcessTitanium`：钛铁矿(Ilmenite)×15 -> 钛合金(Titanium)×15 @ 电弧炉(EAF) ｜ 工作量=3300 ｜ 研究:Metals_D1，技能:Crafting≥16
- **对比**：数值一致

#### 冶炼司太立合金（20个）（`MMT_MakeStellite`）

- **MMT**：钴合金×5 + 铬合金×13 + 钨合金×2 -> 司太立合金（钨铬钴合金）×20 @ 电弧炉 ｜ 工作量=2100 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥17
- **HSK** `MakeStelliteAlloy`：钴合金(Cobalt)×5 + 铬合金(Chromium)×13 + 钨合金(Tungsten)×2 -> 司太立合金（钨铬钴合金）(StelliteAlloy)×20 @ 电弧炉(EAF) ｜ 工作量=2100 ｜ 研究:Metals_D1，技能:Crafting≥17
- **对比**：数值一致

#### 冶炼镍钛合金（15个）（`MMT_MakeNitinol`）

- **MMT**：钛合金×4 + 镍合金×11 -> 镍钛合金×15 @ 电弧炉 ｜ 工作量=2600 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥16
- **HSK** `MakeNitinolAlloy`：钛合金(Titanium)×4 + 镍合金(NickelBar)×11 -> 镍钛合金(NitinolAlloy)×15 @ 电弧炉(EAF) ｜ 工作量=2600 ｜ 研究:Metals_D1，技能:Crafting≥16
- **对比**：数值一致

#### 冶炼钨钴合金（10个）（`MMT_MakePobedite`）

- **MMT**：钨合金×3 + 碳纤维锭×3 + 钴合金×3 -> 钨钴合金×10 @ 电弧炉 ｜ 工作量=2700 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥18
- **HSK** `MakePobediteAlloy`：钨合金(Tungsten)×3 + 碳纤维锭(CarbonAlloy)×3 + 钴合金(Cobalt)×3 -> 钨钴合金(PobediteAlloy)×10 @ 电弧炉(EAF) ｜ 工作量=2700 ｜ 研究:Metals_D1，技能:Crafting≥18
- **对比**：数值一致

#### 冶炼钛铁合金（15个）（`MMT_MakeFerrotitanium`）

- **MMT**：钛合金×5 + 钢(Steel)×12 + 碳纤维锭×3 -> 铁钛合金(Plasteel)×15 @ 电弧炉 ｜ 工作量=2400 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥15
- **HSK** `MakeFerrotitaniumAlloy`：钛合金(Titanium)×5 + 钢(Steel)×12 + 碳纤维锭(CarbonAlloy)×3 -> 铁钛合金(Plasteel)×15 @ 电弧炉(EAF) ｜ 工作量=2400 ｜ 研究:Metals_D1，技能:Crafting≥15
- **对比**：数值一致

#### 冶炼梅尔基奥合金（30个）（`MMT_MakeCupronickelAlloy`）

- **MMT**：铜×24 + 镍合金×6 -> 梅尔基奥合金×30 @ 电弧炉 ｜ 工作量=1600 ｜ 研究:高级金属精炼(MMT_AdvancedMetalRefining)，技能:Crafting≥12
- **HSK** `MakeCupronickelAlloy`：铜(CopperBar)×24 + 镍合金(NickelBar)×6 -> 梅尔基奥合金(CupronickelAlloy)×30 @ 电弧炉(EAF) ｜ 工作量=1600 ｜ 研究:Metals_C2，技能:Crafting≥12
- **对比**：数值一致

#### 冶炼贫铀合金（10个）（`MMT_MakeDepletedUranium`）

- **MMT**：Uranium×10 -> 贫铀合金×10 @ 电弧炉 ｜ 工作量=1700 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥11
- **HSK** `MakeDepletedUraniumAlloy`：Uranium×10 -> 贫铀合金(DepletedUranium)×10 @ 电弧炉(EAF) ｜ 工作量=1700 ｜ 研究:Metals_C3，技能:Crafting≥11
- **对比**：数值一致

#### 冶炼斯坦合金（20个）（`MMT_MakeSteelBar`）

- **MMT**：钢(Steel)×15 + 镍合金×5 -> 斯坦合金×20 @ 电弧炉 ｜ 工作量=1800 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥11
- **HSK** `MakeFerronickelAlloy`：钢(Steel)×15 + 镍合金(NickelBar)×5 -> 斯坦合金(SteelBar)×20 @ 电弧炉(EAF) ｜ 工作量=1800 ｜ 研究:Metals_C3，技能:Crafting≥11
- **对比**：数值一致

#### 冶炼亚力可合金（20个）（`MMT_MakeAlnicoAlloy`）

- **MMT**：钢(Steel)×8 + 铝合金×3 + 钴合金×3 + 镍合金×6 -> 亚力可合金×20 @ 电弧炉 ｜ 工作量=1650 ｜ 研究:高级合金(MMT_AdvancedAlloys)，技能:Crafting≥13
- **HSK** `MakeAlnicoAlloy`：钢(Steel)×8 + 铝合金(AluminiumBar)×3 + 钴合金(Cobalt)×3 + 镍合金(NickelBar)×6 -> 亚力可合金(AlnicoAlloy)×20 @ 电弧炉(EAF) ｜ 工作量=1650 ｜ 研究:Metals_C3，技能:Crafting≥13
- **对比**：数值一致

### 3.2 Agriculture（Agriculture）

#### 制作堆肥（`MMT_MakeCompost`）

- **MMT**：[MeatRaw] 或 [AnimalProductRaw]×25 + Hay 或 灰烬 或 [PlantFoodRaw]×5 -> 堆肥料×5 @ — ｜ 工作量=625 ｜ 研究:农业 I(MMT_AgricultureBasic)
- **HSK** `MakeCompost`：RottedMeat 或 [MeatRaw] 或 [AnimalProductRaw] 或 [Waste]×25 + Hay 或 RottedMush 或 灰烬(Ash) 或 [PlantFoodRaw]×5 -> 堆肥料(Compost)×5 @ ButcherSpot, TableButcher, TableKitchen ｜ 工作量=625 ｜ 研究:SewageSludgeComposting
- **对比**：数值一致

#### 制作肥料（`MMT_MakeFertilizer`）

- **MMT**：堆肥料×10 -> 肥料×10 @ — ｜ 工作量=10000 ｜ 研究:农业 I(MMT_AgricultureBasic)
- **HSK** `MakeFertilizer`：堆肥料(Compost)×10 -> 肥料(Fertilizer)×10 ｜ 工作量=10000
- **对比**：数值一致

#### 拌制土堆（`MMT_MakeDirt`）

- **MMT**：沙×2 + 肥料×1 -> 土堆×3 @ — ｜ 工作量=300 ｜ 研究:农业 I(MMT_AgricultureBasic)
- **HSK** `make_dirt`：沙(SandResource)×2 + 肥料(Fertilizer)×1 -> 土堆(Dirt)×3 @ ButcherSpot ｜ 工作量=300 ｜ 研究:Craft_0
- **对比**：数值一致

#### 拌制土堆（`MMT_MakeDirt_Hand`）

- **MMT**：沙×20 + 肥料×10 -> 土堆×30 @ 手动混凝土搅拌器 ｜ 工作量=3000 ｜ 研究:混凝土技术(MMT_ConcreteBasic)
- **HSK** `make_dirt_hand`：沙(SandResource)×20 + 肥料(Fertilizer)×10 -> 土堆(Dirt)×30 @ 手动混凝土搅拌器(ConcreteMixer) ｜ 工作量=3000 ｜ 研究:Concrete_C1
- **对比**：数值一致

#### 挖掘盐（`MMT_MineSalt`）

- **MMT**：— -> 盐×5 @ 挖盐点 ｜ 工作量=1800
- **HSK** `VG_Minesalt`：— -> 盐(Salt)×5 @ VG_SaltMine ｜ 工作量=1800
- **对比**：数值一致

### 3.3 Ceramics（Ceramics）

#### 烧制陶瓷（10个）（`MMT_MakeCeramics`）

- **MMT**：软粘土×20 + 玻璃生料×5 -> 陶瓷×10 @ 电弧炉, 窑炉 ｜ 工作量=1200 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥8
- **HSK** `MakeCeramics`：SoftClay×20 + 玻璃生料(GlassBatch)×5 -> 陶瓷(Ceramics)×10 @ ElectricSmelter, Kiln ｜ 工作量=1200 ｜ 研究:Craft_B1，技能:Crafting≥8
- **对比**：数值一致

#### 烧制粘土砖（15个）（`MMT_MakeClayBricks`）

- **MMT**：软粘土×15 -> 粘土砖×15 @ 电弧炉, 窑炉 ｜ 工作量=800 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥6
- **HSK** `MakeClayBricks`：SoftClay×15 -> 粘土砖(ClayBrick)×15 @ ElectricSmelter, Kiln ｜ 工作量=800 ｜ 研究:Craft_B1，技能:Crafting≥6
- **对比**：数值一致

#### 焚烧材料（`MMT_BurnThings`）

- **MMT**：原木(WoodLog) 或 [Textiles] 或 [Leathers]×20 -> 灰烬×3 @ 窑炉 ｜ 工作量=400
- **HSK** `BurnApparel`：— -> 灰烬(Ash)×2 @ Campfire, ElectricCrematorium, Kiln ｜ 工作量=200 ｜ 研究:Craft_0
- **对比**：产出 3 vs HSK 2；工作量 400 vs HSK 200

#### 火化尸体（`MMT_CremateCorpseCampfire`）

- **MMT**：[Corpses]×1 -> 灰烬×4 @ — ｜ 工作量=800
- **HSK** `BurnApparel`：— -> 灰烬(Ash)×2 @ Campfire, ElectricCrematorium, Kiln ｜ 工作量=200 ｜ 研究:Craft_0
- **对比**：产出 4 vs HSK 2；工作量 800 vs HSK 200

### 3.4 混凝土（Concrete）

#### 搅拌混凝土（`MMT_MixConcrete_Hand`）

- **MMT**：沙×45 + 碎石×30 -> 混凝土×15 @ 手动混凝土搅拌器 ｜ 工作量=1200 ｜ 研究:混凝土技术(MMT_ConcreteBasic)，技能:Crafting≥3
- **HSK** `MixConcrete`：沙(SandResource)×45 + 碎石(CrushedStone)×30 -> 混凝土(ConcreteResource)×15 @ 手动混凝土搅拌器(ConcreteMixer) ｜ 工作量=1200 ｜ 研究:Concrete_C1，技能:Crafting≥3
- **对比**：数值一致

#### 制作钢筋混凝土（`MMT_MakeReinforcedConcrete_Hand`）

- **MMT**：[MMT_MetallicBar]×10 + 混凝土×10 -> 钢筋混凝土×8 @ — ｜ 工作量=1600 ｜ 研究:混凝土技术(MMT_ConcreteBasic)，技能:Crafting≥4
- **HSK** `MakeReinforcedConcrete_Electric`：[Metallic]×20 + 混凝土(ConcreteResource)×20 -> 钢筋混凝土(ReinforcedConcrete)×16 @ AdvToolBench ｜ 工作量=1200 ｜ 技能:Crafting≥7
- **对比**：首投入 10 vs HSK 20；产出 8 vs HSK 16；工作量 1600 vs HSK 1200；技能 Crafting≥4 vs HSK Crafting≥7

#### 搅拌混凝土（`MMT_MixConcrete_Electric`）

- **MMT**：沙×45 + 碎石×30 -> 混凝土×15 @ 电动混凝土搅拌机 ｜ 工作量=600 ｜ 研究:混凝土 II(MMT_ConcreteAdvanced)，技能:Crafting≥6
- **HSK** `MixConcreteElectric`：沙(SandResource)×45 + 碎石(CrushedStone)×30 -> 混凝土(ConcreteResource)×15 @ 电动混凝土搅拌机(EConcreteMixer) ｜ 工作量=600 ｜ 研究:Concrete_C2，技能:Crafting≥6
- **对比**：数值一致

#### 制作钢筋混凝土（`MMT_MakeReinforcedConcrete_Electric`）

- **MMT**：[MMT_MetallicBar]×20 + 混凝土×20 -> 钢筋混凝土×16 @ 电动装配台 ｜ 工作量=1200 ｜ 技能:Crafting≥7
- **HSK** `MakeReinforcedConcrete_Electric`：[Metallic]×20 + 混凝土(ConcreteResource)×20 -> 钢筋混凝土(ReinforcedConcrete)×16 @ AdvToolBench ｜ 工作量=1200 ｜ 技能:Crafting≥7
- **对比**：数值一致

#### 拌制土堆（`MMT_MakeDirt_Electric`）

- **MMT**：沙×20 + 肥料×10 -> 土堆×30 @ 电动混凝土搅拌机 ｜ 工作量=1500 ｜ 研究:混凝土 II(MMT_ConcreteAdvanced)
- **HSK** `make_dirt_Electric`：沙(SandResource)×20 + 肥料(Fertilizer)×10 -> 土堆(Dirt)×30 @ 电动混凝土搅拌机(EConcreteMixer) ｜ 工作量=1500 ｜ 研究:Concrete_C2
- **对比**：数值一致

### 3.5 Drugs（Drugs）

#### 合成中性胺（天然）（`MMT_MakeNeutroamine`）

- **MMT**：中性花瓣×8 + 金丝桃×2 -> Neutroamine×1 @ — ｜ 工作量=700 ｜ 研究:毒品 V(MMT_Drugs_C2)，技能:Medicine≥6,Crafting≥3
- **HSK** `MakeNaturalNeutroamine`：中性花瓣(NeutroPetals)×8 + 金丝桃(Hypericum)×2 -> Neutroamine×1 @ DrugLab ｜ 工作量=700 ｜ 研究:Drugs_C2，技能:Medicine≥6, Crafting≥3
- **对比**：技能 Medicine≥6,Crafting≥3 vs HSK Medicine≥6, Crafting≥3

#### 精炼中性胺（石化）（`MMT_RefineNeutroamine`）

- **MMT**：Chemfuel×15 -> Neutroamine×1 @ — ｜ 工作量=1200 ｜ 研究:毒品 V(MMT_Drugs_C2)，技能:Medicine≥6
- **HSK** `MakeNaturalNeutroamine`：中性花瓣(NeutroPetals)×8 + 金丝桃(Hypericum)×2 -> Neutroamine×1 @ DrugLab ｜ 工作量=700 ｜ 研究:Drugs_C2，技能:Medicine≥6, Crafting≥3
- **对比**：首投入 15 vs HSK 8；工作量 1200 vs HSK 700；技能 Medicine≥6 vs HSK Medicine≥6, Crafting≥3

#### 晾晒烟叶（`MMT_DrySmokeleaf`）

- **MMT**：SmokeleafLeaves×5 -> 干烟叶×5 @ — ｜ 工作量=2400 ｜ 研究:毒品 I(MMT_Drugs_A1)
- **HSK**：有资源 DriedLeavesSmokeleaf，但无投入匹配的配方（HSK 侧共有 0 条产出配方）

#### 晾晒烟叶（×5）（`MMT_DrySmokeleafBulk`）

- **MMT**：SmokeleafLeaves×25 -> 干烟叶×25 @ — ｜ 工作量=9600 ｜ 研究:毒品 I(MMT_Drugs_A1)
- **HSK**：有资源 DriedLeavesSmokeleaf，但无投入匹配的配方（HSK 侧共有 0 条产出配方）

#### 泡制草药茶（`MMT_MakeHerbalTea`）

- **MMT**：野玫瑰×2 + 金丝桃×1 + 薄荷叶×2 -> 草药茶×6 @ — ｜ 工作量=780 ｜ 研究:DrugProduction(DrugProduction)，技能:Medicine≥3,Cooking≥5
- **HSK** `MakeHerbalTea`：野玫瑰(WildRose)×2 + 金丝桃(Hypericum)×1 + 薄荷叶(MintLeaves)×2 -> 草药茶(HerbalTea)×6 @ Brewery, DrugLab, ElectricBrewery, 手工制药台(meditable) ｜ 工作量=780 ｜ 研究:DrugProduction，技能:Medicine≥3, Cooking≥5
- **对比**：技能 Medicine≥3,Cooking≥5 vs HSK Medicine≥3, Cooking≥5

#### 烧制活性炭丸（`MMT_MakeActivatedCarbon`）

- **MMT**：木炭 或 原煤×5 -> 活性炭丸×9 @ — ｜ 工作量=780 ｜ 研究:DrugProduction(DrugProduction)，技能:Medicine≥3,Crafting≥5
- **HSK** `MakeActivatedCarbon`：木炭(Charcoal) 或 原煤(Coal)×5 -> 活性炭丸(ActivatedCarbon)×9 @ DrugLab, Kiln, 手工制药台(meditable) ｜ 工作量=780 ｜ 研究:DrugProduction，技能:Medicine≥3, Crafting≥5
- **对比**：技能 Medicine≥3,Crafting≥5 vs HSK Medicine≥3, Crafting≥5

#### 配制狂暴草（`MMT_MakeBerserkHerb`）

- **MMT**：MedicineHerbal×3 + PsychoidLeaves×3 -> 狂暴草×1 @ — ｜ 工作量=200 ｜ 研究:PsychoidBrewing(PsychoidBrewing)
- **HSK**：无对应资源（MMT 新增）

#### 合成美沙酮（`MMT_MakeMethadone`）

- **MMT**：Neutroamine×1 -> 美沙酮×1 @ — ｜ 工作量=500 ｜ 研究:PenoxycylineProduction(PenoxycylineProduction)
- **HSK**：无对应资源（MMT 新增）

#### 合成止痛药（`MMT_MakeIbuprofen`）

- **MMT**：Neutroamine×1 -> 止痛药×1 @ — ｜ 工作量=200 ｜ 研究:毒品 V(MMT_Drugs_C2)
- **HSK** `VG_BulkIbuprofen`：Neutroamine×4 -> 止痛药(VG_Ibuprofen)×4 @ DrugLab ｜ 工作量=720 ｜ 研究:PenoxycylineProduction，技能:Medicine≥10
- **对比**：首投入 1 vs HSK 4；产出 1 vs HSK 4；工作量 200 vs HSK 720

#### 合成止痛药（×4）（`MMT_MakeIbuprofenBulk`）

- **MMT**：Neutroamine×4 -> 止痛药×4 @ — ｜ 工作量=720 ｜ 研究:PenoxycylineProduction(PenoxycylineProduction)，技能:Medicine≥10
- **HSK** `VG_BulkIbuprofen`：Neutroamine×4 -> 止痛药(VG_Ibuprofen)×4 @ DrugLab ｜ 工作量=720 ｜ 研究:PenoxycylineProduction，技能:Medicine≥10
- **对比**：数值一致

### 3.6 电动金属（Electric）

#### 提炼铜（20个）（`MMT_SmeltCopperBar_Electric`）

- **MMT**：铜矿×20 -> 铜×20 @ 电弧炉 ｜ 工作量=500 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥4
- **HSK** `MakeCopperBars_Electric`：铜矿(Copper)×20 -> 铜(CopperBar)×20 @ ElectricSmelter ｜ 工作量=500 ｜ 研究:Metals_C1，技能:Crafting≥4
- **对比**：数值一致

#### 提炼锡（20个）（`MMT_SmeltTinBar_Electric`）

- **MMT**：锡矿×20 -> 锡×20 @ 电弧炉 ｜ 工作量=500 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥4
- **HSK** `MakeTinBars_Electric`：锡矿(Tin)×20 -> 锡(TinBar)×20 @ ElectricSmelter ｜ 工作量=500 ｜ 研究:Metals_C1，技能:Crafting≥3
- **对比**：技能 Crafting≥4 vs HSK Crafting≥3

#### 提炼青铜（30个）（`MMT_MakeBronze_Electric`）

- **MMT**：铜×20 + 锡×10 -> 青铜×30 @ 电弧炉 ｜ 工作量=700 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥4
- **HSK** `MakeBronze_Electric`：铜矿(Copper) 或 铜(CopperBar)×20 + 锡(TinBar) 或 铝合金(AluminiumBar)×10 -> 青铜(Bronze)×30 @ ElectricSmelter ｜ 工作量=700 ｜ 研究:Metals_C1，技能:Crafting≥5
- **对比**：技能 Crafting≥4 vs HSK Crafting≥5

#### 提炼钢铁（20个）（`MMT_SmeltSteel_Electric`）

- **MMT**：铁矿石×15 + 木炭 或 原煤×7 -> 钢(Steel)×20 @ 电弧炉 ｜ 工作量=900 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥6
- **HSK** `MakeSteelBars_Electric`：铁矿石(Iron)×15 + [Coal]×7 -> 钢(Steel)×20 @ ElectricSmelter ｜ 工作量=900 ｜ 研究:Metals_C1，技能:Crafting≥6
- **对比**：数值一致

#### 提炼纯银合金（15个）（`MMT_SmeltSilver_Electric`）

- **MMT**：白银×14 -> 银(Silver)×10 @ 电弧炉 ｜ 工作量=500 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥5
- **HSK** `ConvertToSilver`：无机物(Matter)×100 -> 白银(Silver)×300 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 14 vs HSK 100；产出 10 vs HSK 300；工作量 500 vs HSK 12000；技能 Crafting≥5 vs HSK Crafting≥15

#### 提炼纯金合金（15个）（`MMT_SmeltGold_Electric`）

- **MMT**：金矿×12 -> 金(Gold)×15 @ 电弧炉 ｜ 工作量=500 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥6
- **HSK** `ConvertToGold`：无机物(Matter)×60 -> 金矿(Gold)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 12 vs HSK 60；产出 15 vs HSK 20；工作量 500 vs HSK 12000；技能 Crafting≥6 vs HSK Crafting≥15

#### 提取硅（x4）（`MMT_ExtractSilicon_Electric`）

- **MMT**：沙×50 -> 硅×5 @ 电弧炉 ｜ 工作量=800 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥5
- **HSK** `MakeSilicon_Set`：沙(SandResource)×200 -> 硅(Silicon)×20 @ 电弧炉(EAF) ｜ 工作量=1800 ｜ 研究:Metals_C1，技能:Crafting≥5
- **对比**：首投入 50 vs HSK 200；产出 5 vs HSK 20；工作量 800 vs HSK 1800

#### 批量提取硅（x4）（`MMT_MakeSilicon_Bulk`）

- **MMT**：沙×200 -> 硅×20 @ 电弧炉 ｜ 工作量=1800 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥5
- **HSK** `MakeSilicon_Set`：沙(SandResource)×200 -> 硅(Silicon)×20 @ 电弧炉(EAF) ｜ 工作量=1800 ｜ 研究:Metals_C1，技能:Crafting≥5
- **对比**：数值一致

#### 提炼铅（20个）（`MMT_SmeltLeadBar_Electric`）

- **MMT**：铅矾矿×20 -> 铅合金×20 @ 电弧炉 ｜ 工作量=1000 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥6
- **HSK** `MakeLeadBars_Electric`：铅矾矿(Anglesite)×20 -> 铅合金(LeadBar)×20 @ ElectricSmelter ｜ 工作量=1000 ｜ 研究:Metals_C1，技能:Crafting≥6
- **对比**：数值一致

#### 提炼铝（20个）（`MMT_SmeltAluminiumBar_Electric`）

- **MMT**：铝土矿×20 -> 铝合金×20 @ 电弧炉 ｜ 工作量=1200 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥7
- **HSK** `MakeAluminiumBars_Electric`：铝土矿(Aluminium)×20 -> 铝合金(AluminiumBar)×20 @ ElectricSmelter ｜ 工作量=1200 ｜ 研究:Metals_C2，技能:Crafting≥7
- **对比**：数值一致

#### 冶炼斯特合金（30个）（`MMT_MakeFerrosiliconAlloy`）

- **MMT**：钢(Steel)×15 + 硅×10 + 铝合金×5 -> 斯特合金×30 @ 电弧炉 ｜ 工作量=1700 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥8
- **HSK** `MakeFerrosiliconAlloy`：钢(Steel)×15 + 硅(Silicon)×10 + 铝合金(AluminiumBar)×5 -> 斯特合金(FerrosiliconAlloy)×30 @ ElectricSmelter ｜ 工作量=1700 ｜ 研究:Metals_C2，技能:Crafting≥8
- **对比**：数值一致

#### 制作金属罐（15个）（`MMT_MakeMetalCan`）

- **MMT**：[MMT_MetallicBar]×8 -> 金属罐×15 @ 电弧炉 ｜ 工作量=800 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥2
- **HSK** `MakeMetalCan`：[Metallic]×8 -> 金属罐(MetalCan)×15 @ ElectricSmelter ｜ 工作量=800 ｜ 研究:Metals_C1，技能:Crafting≥2
- **对比**：数值一致

### 3.7 电动电子（ElectricElectronics）

#### 组装电子元件（`MMT_AssembleElectronicParts_Electric`）

- **MMT**：铜×15 + 玻璃块×10 + 硅×10 -> 电子元件×5 @ 电子器件工作台 ｜ 工作量=1600 ｜ 研究:电动电子工艺(MMT_ElectricElectronics)，技能:Crafting≥10
- **HSK** `MakeElectronicComponents`：[HCMBar]×35 + 玻璃块(Glass)×10 + 工业零件(ComponentIndustrial)×10 + SilverBar×10 -> 电子元件(ElectronicComponents)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=2500 ｜ 技能:Crafting≥8
- **对比**：首投入 15 vs HSK 35；工作量 1600 vs HSK 2500；技能 Crafting≥10 vs HSK Crafting≥8

#### 组装集成电路（`MMT_AssembleIC_Electric`）

- **MMT**：硅×25 + 塑料×20 + 电子元件×3 + 金(Gold)×2 -> 集成电路×5 @ 电子器件工作台 ｜ 工作量=2400 ｜ 研究:电动电子工艺(MMT_ElectricElectronics)，技能:Crafting≥12
- **HSK** `MakeIntegratedCircuits`：硅(Silicon)×25 + 塑料(Plastic)×20 + 电子元件(ElectronicComponents)×3 + GoldBar×2 -> 集成电路(Electronics)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:Electronics_C2，技能:Crafting≥12
- **对比**：工作量 2400 vs HSK 3500

#### 组装微芯片集成电路（`MMT_AssembleMicrochip_Electric`）

- **MMT**：集成电路×2 + 碳聚物×5 + 金(Gold)×5 -> 微芯片集成电路×1 @ 电子器件工作台 ｜ 工作量=2400 ｜ 研究:电动电子工艺(MMT_ElectricElectronics)，技能:Crafting≥14
- **HSK** `MakeMicrochipIntegratedCircuits`：集成电路(Electronics)×2 + 碳纤维锭(CarbonAlloy)×5 + GoldBar×5 -> 微芯片集成电路(Microchips)×1 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:Electronics_D1，技能:Crafting≥16
- **对比**：工作量 2400 vs HSK 3500；技能 Crafting≥14 vs HSK Crafting≥16

### 3.8 电动石化（ElectricPetrochem）

#### 电蒸馏聚合物（`MMT_DistillPolymers_Electric`）

- **MMT**：原油×20 -> 聚合物×15 @ 石化产品实验室 ｜ 工作量=1000 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToPolymers`：无机物(Matter)×50 -> 聚合物(Polymers)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 20 vs HSK 50；产出 15 vs HSK 100；工作量 1000 vs HSK 12000；技能 Crafting≥6 vs HSK Crafting≥15

#### 制作塑料（`MMT_MakePlastic_Electric`）

- **MMT**：聚合物×30 -> 塑料×10 @ 石化产品实验室 ｜ 工作量=500 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥8
- **HSK** `MakePlastic`：聚合物(Polymers)×30 -> 塑料(Plastic)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:Crafting≥10
- **对比**：工作量 500 vs HSK 800；技能 Crafting≥8 vs HSK Crafting≥10

#### 制作合成橡胶（`MMT_MakeRubber_Electric`）

- **MMT**：聚合物×30 + 硫磺×3 -> 合成橡胶×10 @ 石化产品实验室 ｜ 工作量=500 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥8
- **HSK** `MakeRubber`：聚合物(Polymers)×30 + 硫酸盐(Sulphates)×3 -> 合成橡胶(Rubber)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:Crafting≥10
- **对比**：工作量 500 vs HSK 800；技能 Crafting≥8 vs HSK Crafting≥10

#### 制作人造纤维（`MMT_MakeSyntheticFibers_Electric`）

- **MMT**：聚合物×25 + 硫磺×3 -> 人造纤维×5 @ 石化产品实验室 ｜ 工作量=800 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥10
- **HSK** `MakeSyntheticFibers`：聚合物(Polymers)×25 + 硫酸盐(Sulphates)×3 -> 人造纤维(SyntheticFibers)×5 @ 石化产品实验室(TableChemlab) ｜ 工作量=1200 ｜ 研究:Oil_Industry_C6，技能:Crafting≥14
- **对比**：工作量 800 vs HSK 1200；技能 Crafting≥10 vs HSK Crafting≥14

#### 制作碳聚物（`MMT_MakeCarbon_Electric`）

- **MMT**：聚合物×25 + 木炭 或 原煤×25 + 合成橡胶×15 -> 碳聚物×12 @ 石化产品实验室 ｜ 工作量=2800 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥12
- **HSK** `MakeCarbon`：聚合物(Polymers)×25 + [Coal]×25 + 合成橡胶(Rubber)×15 -> 碳聚物(Carbon)×12 @ 石化产品实验室(TableChemlab) ｜ 工作量=4000 ｜ 研究:Oil_Industry_C6，技能:Crafting≥15
- **对比**：工作量 2800 vs HSK 4000；技能 Crafting≥12 vs HSK Crafting≥15

### 3.9 电动木材（ElectricWood）

#### 加工木板（`MMT_MakeWoodPlanks_Electric`）

- **MMT**：原木(WoodLog)×20 -> 木板×30 @ 电动锯木台 ｜ 工作量=1200 ｜ 研究:电动木材加工(MMT_ElectricWoodworking)，技能:Crafting≥6
- **HSK** `MakeWoodPlanks_Electric`：原木(WoodLog)×20 -> 木板(WoodPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:Metals_C1，技能:Crafting≥6
- **对比**：数值一致

#### 加工柴火（`MMT_MakeKindling_Electric`）

- **MMT**：原木(WoodLog)×20 -> 柴火×40 @ 电动锯木台 ｜ 工作量=500 ｜ 研究:电动木材加工(MMT_ElectricWoodworking)，技能:Crafting≥4
- **HSK** `MakeKindling_Electric`：[Wooden]×20 -> 柴火(Kindling)×40 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=500 ｜ 研究:Metals_C1，技能:Crafting≥4
- **对比**：数值一致

#### 制作木炭（`MMT_MakeCharcoal_Electric`）

- **MMT**：原木(WoodLog)×25 -> 木炭×20 @ 电动木炭窑 ｜ 工作量=800 ｜ 研究:电动木材加工(MMT_ElectricWoodworking)，技能:Crafting≥4
- **HSK** `MakeCharcoal_Electric`：[Wooden]×25 -> 木炭(Charcoal)×20 @ ElectricSmelter ｜ 工作量=800 ｜ 研究:Metals_C1，技能:Crafting≥4
- **对比**：数值一致

#### 加工竹板（`MMT_MakeBambooPlanks_Electric`）

- **MMT**：竹节×20 -> 竹板×30 @ 电动锯木台 ｜ 工作量=1200 ｜ 研究:电动木材加工(MMT_ElectricWoodworking)，技能:Crafting≥9
- **HSK** `MakeBambooPlanks_Electric`：竹节(Bamboo)×20 -> 竹板(BambooPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:Metals_C1，技能:Crafting≥9
- **对比**：数值一致

#### 加工红木木板（`MMT_MakeRedWoodPlanks_Electric`）

- **MMT**：红木×20 -> 红木木板×30 @ 电动锯木台 ｜ 工作量=1200 ｜ 研究:电动木材加工(MMT_ElectricWoodworking)，技能:Crafting≥6
- **HSK** `MakeRedWoodPlanks_Electric`：红木(RedWoodLog)×20 -> 红木木板(RedWoodPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:Metals_C1，技能:Crafting≥6
- **对比**：数值一致

### 3.10 电子（Electronics）

#### 提炼硅（`MMT_ExtractSilicon_Hand`）

- **MMT**：沙×50 -> 硅×5 @ 熔炉 ｜ 工作量=1600 ｜ 研究:基础电子工艺(MMT_BasicElectronics)，技能:Crafting≥5
- **HSK** `MakeSilicon_Electric`：沙(SandResource)×50 -> 硅(Silicon)×5 @ ElectricSmelter ｜ 工作量=800 ｜ 研究:Metals_C1，技能:Crafting≥5
- **对比**：工作量 1600 vs HSK 800

#### 组装电子元件（`MMT_AssembleElectronicParts_Hand`）

- **MMT**：铜×15 + 玻璃块×10 + 硅×10 -> 电子元件×5 @ 电子器件工作台 ｜ 工作量=2500 ｜ 研究:基础电子工艺(MMT_BasicElectronics)，技能:Crafting≥8
- **HSK** `MakeElectronicComponents`：[HCMBar]×35 + 玻璃块(Glass)×10 + 工业零件(ComponentIndustrial)×10 + SilverBar×10 -> 电子元件(ElectronicComponents)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=2500 ｜ 技能:Crafting≥8
- **对比**：首投入 15 vs HSK 35

#### 组装集成电路（`MMT_AssembleIC_Hand`）

- **MMT**：硅×25 + 塑料×20 + 电子元件×3 + 金(Gold)×2 -> 集成电路×5 @ 电子器件工作台 ｜ 工作量=3500 ｜ 研究:集成电路技术(MMT_IntegratedCircuitTech)，技能:Crafting≥10
- **HSK** `MakeIntegratedCircuits`：硅(Silicon)×25 + 塑料(Plastic)×20 + 电子元件(ElectronicComponents)×3 + GoldBar×2 -> 集成电路(Electronics)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:Electronics_C2，技能:Crafting≥12
- **对比**：技能 Crafting≥10 vs HSK Crafting≥12

#### 组装微芯片集成电路（`MMT_AssembleMicrochip_Hand`）

- **MMT**：集成电路×2 + 碳聚物×5 + 金(Gold)×5 -> 微芯片集成电路×1 @ 电子器件工作台 ｜ 工作量=3500 ｜ 研究:集成电路技术(MMT_IntegratedCircuitTech)，技能:Crafting≥12
- **HSK** `MakeMicrochipIntegratedCircuits`：集成电路(Electronics)×2 + 碳纤维锭(CarbonAlloy)×5 + GoldBar×5 -> 微芯片集成电路(Microchips)×1 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:Electronics_D1，技能:Crafting≥16
- **对比**：技能 Crafting≥12 vs HSK Crafting≥16

### 3.11 玻璃（Glass）

#### 将石砖碾磨成碎石（`MMT_MakeCrushedStone_Hand`）

- **MMT**：[StoneBlocks]×20 -> 碎石×20 @ — ｜ 工作量=1200
- **HSK** `MakeCrushedStone_Electric`：[StoneBlocks]×40 -> 碎石(CrushedStone)×40 @ TableMachining ｜ 工作量=1200
- **对比**：首投入 20 vs HSK 40；产出 20 vs HSK 40

#### 将石砖碾磨成碎石（`MMT_MakeCrushedStone_Electric`）

- **MMT**：[StoneBlocks]×40 -> 碎石×40 @ — ｜ 工作量=1200
- **HSK** `MakeCrushedStone_Electric`：[StoneBlocks]×40 -> 碎石(CrushedStone)×40 @ TableMachining ｜ 工作量=1200
- **对比**：数值一致

#### 将石块碾磨成碎石（`MMT_MakeCrushedStone_Hand_Chunk`）

- **MMT**：[StoneChunks]×1 -> 碎石×32 @ — ｜ 工作量=2400
- **HSK** `MakeCrushedStone_Electric`：[StoneBlocks]×40 -> 碎石(CrushedStone)×40 @ TableMachining ｜ 工作量=1200
- **对比**：首投入 1 vs HSK 40；产出 32 vs HSK 40；工作量 2400 vs HSK 1200

#### 将石块碾磨成碎石（`MMT_MakeCrushedStone_Electric_Chunk`）

- **MMT**：[StoneChunks]×1 -> 碎石×32 @ — ｜ 工作量=1200
- **HSK** `MakeCrushedStone_Electric`：[StoneBlocks]×40 -> 碎石(CrushedStone)×40 @ TableMachining ｜ 工作量=1200
- **对比**：首投入 1 vs HSK 40；产出 32 vs HSK 40

#### 将碎石碾磨成沙子（`MMT_MakeSand_Hand`）

- **MMT**：碎石×10 -> 沙×25 @ — ｜ 工作量=600 ｜ 研究:Stonecutting(Stonecutting)
- **HSK** `MakeSand_Electric`：碎石(CrushedStone)×20 -> 沙(SandResource)×50 @ TableMachining ｜ 工作量=600
- **对比**：首投入 10 vs HSK 20；产出 25 vs HSK 50

#### 将碎石碾磨成沙子（`MMT_MakeSand_Electric`）

- **MMT**：碎石×20 -> 沙×50 @ — ｜ 工作量=600
- **HSK** `MakeSand_Electric`：碎石(CrushedStone)×20 -> 沙(SandResource)×50 @ TableMachining ｜ 工作量=600
- **对比**：数值一致

#### 将石块碾磨成沙子（`MMT_MakeSand_Hand_Chunk`）

- **MMT**：[StoneChunks]×1 -> 沙×80 @ — ｜ 工作量=3000 ｜ 研究:Stonecutting(Stonecutting)
- **HSK** `MakeSand_Electric`：碎石(CrushedStone)×20 -> 沙(SandResource)×50 @ TableMachining ｜ 工作量=600
- **对比**：首投入 1 vs HSK 20；产出 80 vs HSK 50；工作量 3000 vs HSK 600

#### 将石块碾磨成沙子（`MMT_MakeSand_Electric_Chunk`）

- **MMT**：[StoneChunks]×1 -> 沙×80 @ — ｜ 工作量=1500
- **HSK** `MakeSand_Electric`：碎石(CrushedStone)×20 -> 沙(SandResource)×50 @ TableMachining ｜ 工作量=600
- **对比**：首投入 1 vs HSK 20；产出 80 vs HSK 50；工作量 1500 vs HSK 600

#### 烧制玻璃生料（`MMT_MakeGlassBatch`）

- **MMT**：沙×75 -> 玻璃生料×25 @ 玻璃制作台 ｜ 工作量=900 ｜ 研究:玻璃制造(MMT_Glassworking)
- **HSK** `MakeGlassBatch`：沙(SandResource)×75 -> 玻璃生料(GlassBatch)×25 @ 玻璃制作台(GlassworkTable), 熔炉(TableFurnace) ｜ 工作量=900 ｜ 研究:Glass_B1，技能:Crafting≥3
- **对比**：数值一致

#### 烧制玻璃块（`MMT_MakeGlass`）

- **MMT**：玻璃生料×20 -> 玻璃块×10 @ 玻璃制作台 ｜ 工作量=600 ｜ 研究:玻璃制造(MMT_Glassworking)
- **HSK** `MakeGlass`：玻璃生料(GlassBatch)×20 -> 玻璃块(Glass)×10 @ 玻璃制作台(GlassworkTable), 熔炉(TableFurnace) ｜ 工作量=600 ｜ 研究:Glass_B1，技能:Crafting≥5
- **对比**：数值一致

#### 制作强化玻璃（`MMT_MakeReinforcedGlass`）

- **MMT**：玻璃生料×40 + 碳聚物×5 + 人造纤维×5 -> 强化玻璃块×5 @ 玻璃制作台 ｜ 工作量=1400 ｜ 研究:强化玻璃工艺(MMT_ReinforcedGlassTech)，技能:Crafting≥8
- **HSK** `MakeReinforcedGlass`：玻璃生料(GlassBatch)×40 + 碳聚物(Carbon)×5 + 人造纤维(SyntheticFibers)×5 -> 强化玻璃块(ReinforcedGlass)×5 @ 玻璃制作台(GlassworkTable) ｜ 工作量=1400 ｜ 研究:Glass_C1，技能:Crafting≥8
- **对比**：数值一致

### 3.12 Matter（Matter）

#### 转化为无机物（`MMT_ConvertResourcesRaw`）

- **MMT**：[ResourcesRaw]×300 -> 无机物×150 @ 无机物转化器 ｜ 工作量=10000 ｜ 研究:物质转换 I(MMT_MatterConversion1)，技能:Crafting≥15
- **HSK** `ConvertResourcesRaw`：[ResourcesRaw]×300 -> 无机物(Matter)×150 @ 无机物转化器(MatterConverter) ｜ 工作量=10000 ｜ 研究:Super_matter_D1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为铁矿石（`MMT_ConvertToIron`）

- **MMT**：无机物×50 -> 铁矿石×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToSteel`：无机物(Matter)×50 -> 铁矿石(Iron)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为铜矿（`MMT_ConvertToCopper`）

- **MMT**：无机物×50 -> 铜矿×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToCopper`：无机物(Matter)×50 -> 铜矿(Copper)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为锡矿（`MMT_ConvertToTin`）

- **MMT**：无机物×50 -> 锡矿×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToTin`：无机物(Matter)×50 -> 锡矿(Tin)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为铅矾矿（`MMT_ConvertToLead`）

- **MMT**：无机物×50 -> 铅矾矿×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToLead`：无机物(Matter)×50 -> 铅矾矿(Anglesite)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为铝土矿（`MMT_ConvertToAluminium`）

- **MMT**：无机物×50 -> 铝土矿×80 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToAluminium`：无机物(Matter)×50 -> 铝土矿(Aluminium)×80 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为镍矿（`MMT_ConvertToNickel`）

- **MMT**：无机物×150 -> 镍矿×20 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToNickel`：无机物(Matter)×150 -> 镍矿(Nickel)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为钛铁矿（`MMT_ConvertToTitanium`）

- **MMT**：无机物×200 -> 钛铁矿×20 @ 无机物转化器 ｜ 工作量=15000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToTitanium`：无机物(Matter)×200 -> 钛铁矿(Ilmenite)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=15000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为金矿（`MMT_ConvertToGold`）

- **MMT**：无机物×60 -> 金矿×20 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToGold`：无机物(Matter)×60 -> 金矿(Gold)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为银矿（`MMT_ConvertToSilver`）

- **MMT**：无机物×100 -> 白银×300 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToSilver`：无机物(Matter)×100 -> 白银(Silver)×300 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为铀矿（`MMT_ConvertToUranium`）

- **MMT**：无机物×200 -> Uranium×50 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToUranium`：无机物(Matter)×200 -> Uranium×50 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为聚合物（`MMT_ConvertToPolymers`）

- **MMT**：无机物×50 -> 聚合物×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToPolymers`：无机物(Matter)×50 -> 聚合物(Polymers)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为石蜡（`MMT_ConvertToParaffins`）

- **MMT**：无机物×50 -> 石蜡×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToParaffins`：无机物(Matter)×50 -> 石蜡(Paraffins)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

#### 转化为硫酸盐（`MMT_ConvertToSulphates`）

- **MMT**：无机物×50 -> 硫酸盐×100 @ 无机物转化器 ｜ 工作量=12000 ｜ 研究:物质转换 II(MMT_MatterConversion2)，技能:Crafting≥15
- **HSK** `ConvertToSulphates`：无机物(Matter)×50 -> 硫酸盐(Sulphates)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：数值一致

### 3.13 Medicine（Medicine）

#### 制作粗制绷带（`MMT_MakeBandagekitcrude`）

- **MMT**：[MMT_BTextiles]×7 -> 粗制绷带×1 @ — ｜ 工作量=150 ｜ 研究:医疗物资 I(MMT_Medicine_A1)，技能:Medicine≥1,Crafting≥1
- **HSK** `MakeBandagekitcrude`：[BTextiles]×7 -> 粗制绷带(Bandagekitcrude)×1 @ CraftingSpot, ElectricTailoringBench, HandTailoringBench, LeatherworkerTable, 手工制药台(meditable) ｜ 工作量=150 ｜ 研究:Medicine_A1，技能:Medicine≥1, Crafting≥1
- **对比**：技能 Medicine≥1,Crafting≥1 vs HSK Medicine≥1, Crafting≥1

#### 制作粗制绷带（×3）（`MMT_MakeBandagekitcrudeBulk`）

- **MMT**：[MMT_BTextiles]×21 -> 粗制绷带×3 @ — ｜ 工作量=450 ｜ 研究:医疗物资 I(MMT_Medicine_A1)，技能:Medicine≥1,Crafting≥1
- **HSK** `MakeBandagekitcrude`：[BTextiles]×7 -> 粗制绷带(Bandagekitcrude)×1 @ CraftingSpot, ElectricTailoringBench, HandTailoringBench, LeatherworkerTable, 手工制药台(meditable) ｜ 工作量=150 ｜ 研究:Medicine_A1，技能:Medicine≥1, Crafting≥1
- **对比**：首投入 21 vs HSK 7；产出 3 vs HSK 1；工作量 450 vs HSK 150；技能 Medicine≥1,Crafting≥1 vs HSK Medicine≥1, Crafting≥1

#### 制作绷带包（`MMT_MakeBandagekit`）

- **MMT**：[MMT_BTextiles]×10 -> 绷带包×1 @ — ｜ 工作量=250 ｜ 研究:医疗物资 II(MMT_Medicine_A2)，技能:Medicine≥2,Crafting≥2
- **HSK** `MakeBandageKit`：[BTextiles]×10 -> 绷带包(Bandagekit)×1 @ ElectricTailoringBench, HandTailoringBench, 手工制药台(meditable) ｜ 工作量=250 ｜ 研究:Medicine_A2，技能:Medicine≥2, Crafting≥2
- **对比**：技能 Medicine≥2,Crafting≥2 vs HSK Medicine≥2, Crafting≥2

#### 制作绷带包（×3）（`MMT_MakeBandagekitBulk`）

- **MMT**：[MMT_BTextiles]×30 -> 绷带包×3 @ — ｜ 工作量=750 ｜ 研究:医疗物资 II(MMT_Medicine_A2)，技能:Medicine≥2,Crafting≥2
- **HSK** `MakeBandageKit`：[BTextiles]×10 -> 绷带包(Bandagekit)×1 @ ElectricTailoringBench, HandTailoringBench, 手工制药台(meditable) ｜ 工作量=250 ｜ 研究:Medicine_A2，技能:Medicine≥2, Crafting≥2
- **对比**：首投入 30 vs HSK 10；产出 3 vs HSK 1；工作量 750 vs HSK 250；技能 Medicine≥2,Crafting≥2 vs HSK Medicine≥2, Crafting≥2

#### 配制草药包（`MMT_MakeHerbMedicine`）

- **MMT**：MedicineHerbal×8 + 灰烬×2 -> 草药包×1 @ — ｜ 工作量=500 ｜ 研究:医疗物资 III(MMT_Medicine_B1)，技能:Medicine≥5
- **HSK** `MakeHerbMedicine`：MedicineHerbal×8 + 灰烬(Ash)×2 -> 草药包(HerbMedicine)×1 @ ButcherSpot, TableButcher, TableKitchen, 手工制药台(meditable) ｜ 工作量=500 ｜ 研究:Medicine_B1，技能:Medicine≥5
- **对比**：数值一致

#### 制作基础医药包（芦荟）（`MMT_MakeBasicMedicineKit`）

- **MMT**：芦荟叶×5 + 绷带包×1 -> 基础医药包×1 @ — ｜ 工作量=500 ｜ 研究:医疗物资 IV(MMT_Medicine_B2)，技能:Medicine≥4,Crafting≥2
- **HSK** `MakeHerbmedicine`：芦荟叶(aloe)×5 + 绷带包(Bandagekit)×1 -> 基础医药包(Herbmedicine)×1 @ DrugLab, TablePrimitiveProsthetic ｜ 工作量=500 ｜ 研究:Medicine_B2，技能:Medicine≥4, Crafting≥2
- **对比**：技能 Medicine≥4,Crafting≥2 vs HSK Medicine≥4, Crafting≥2

#### 制作基础医药包（混合草药）（`MMT_MakeBasicMedicineKitMix`）

- **MMT**：野玫瑰×1 + 金丝桃×1 + 薄荷叶 或 芦荟叶×1 + 绷带包×1 -> 基础医药包×1 @ — ｜ 工作量=500 ｜ 研究:医疗物资 IV(MMT_Medicine_B2)，技能:Medicine≥4,Crafting≥2
- **HSK** `MakeHerbmedicineMix`：野玫瑰(WildRose)×1 + 金丝桃(Hypericum)×1 + 薄荷叶(MintLeaves) 或 芦荟叶(aloe)×1 + 绷带包(Bandagekit)×1 -> 基础医药包(Herbmedicine)×1 @ DrugLab, TablePrimitiveProsthetic ｜ 工作量=500 ｜ 研究:Medicine_B2，技能:Medicine≥4, Crafting≥2
- **对比**：技能 Medicine≥4,Crafting≥2 vs HSK Medicine≥4, Crafting≥2

#### 提炼抗生素（`MMT_BrewMedicalDrink`）

- **MMT**：MedicineHerbal×1 + RawHops×10 -> 抗生素×1 @ — ｜ 工作量=1000 ｜ 研究:PenoxycylineProduction(PenoxycylineProduction)，技能:Medicine≥10
- **HSK** `BrewMedicalDrink`：MedicineHerbal×1 + RawHops 或 Yeast×10 -> 抗生素(MedicalDrink)×1 @ 手工制药台(meditable) ｜ 工作量=1000 ｜ 研究:PenoxycylineProduction，技能:Medicine≥10
- **对比**：数值一致

#### 制作医药（`MMT_MakeMedicineIndustrial`）

- **MMT**：Neutroamine×2 + MedicineHerbal×6 + 石蜡×10 + 人造纤维×2 + 塑料×10 -> MedicineIndustrial×1 @ — ｜ 工作量=800 ｜ 研究:MedicineProduction(MedicineProduction)，技能:Medicine≥10
- **HSK**：无对应资源（MMT 新增）

#### 制作闪耀世界医药（`MMT_MakeMedicineUltratech`）

- **MMT**：微芯片集成电路×1 + 抗生素×2 + 生物合成材料×2 + 石蜡×3 + Neutroamine×2 + Synthread×5 + 塑料×20 -> MedicineUltratech×1 @ — ｜ 工作量=1400 ｜ 研究:医疗物资 VI(MMT_Medicine_D1)，技能:Medicine≥15
- **HSK**：无对应资源（MMT 新增）

### 3.14 金属（Metal）

#### 提炼铜（10个）（`MMT_SmeltCopperBar_Hand`）

- **MMT**：铜矿×10 -> 铜×10 @ 熔炉 ｜ 工作量=700 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥1
- **HSK** `MakeCopperBars_Hand`：铜矿(Copper)×10 -> 铜(CopperBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=700 ｜ 研究:Craft_B1，技能:Crafting≥1
- **对比**：数值一致

#### 提炼锡（10个）（`MMT_SmeltTinBar_Hand`）

- **MMT**：锡矿×10 -> 锡×10 @ 熔炉 ｜ 工作量=600 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥1
- **HSK** `MakeTinBars_Hand`：锡矿(Tin)×10 -> 锡(TinBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=600 ｜ 研究:Craft_B1，技能:Crafting≥1
- **对比**：数值一致

#### 提炼青铜（30个）（`MMT_MakeBronze_Hand`）

- **MMT**：铜×20 + 锡×10 -> 青铜×30 @ 熔炉 ｜ 工作量=1000 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥2
- **HSK** `MakeBronze_Hand`：铜矿(Copper) 或 铜(CopperBar)×20 + 锡(TinBar) 或 铝合金(AluminiumBar)×10 -> 青铜(Bronze)×30 @ 熔炉(TableFurnace) ｜ 工作量=1000 ｜ 研究:Craft_B1，技能:Crafting≥2
- **对比**：数值一致

#### 提炼钢铁（20个）（`MMT_SmeltSteel_Hand`）

- **MMT**：铁矿石×15 + 木炭 或 原煤×13 -> 钢(Steel)×20 @ 熔炉 ｜ 工作量=1300 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥3
- **HSK** `MakeSteelBars_Hand`：铁矿石(Iron)×15 + [Coal]×13 -> 钢(Steel)×20 @ 熔炉(TableFurnace) ｜ 工作量=1300 ｜ 研究:Craft_B1，技能:Crafting≥3
- **对比**：数值一致

#### 提炼铅（10个）（`MMT_SmeltLeadBar_Hand`）

- **MMT**：铅矾矿×10 -> 铅合金×10 @ 熔炉 ｜ 工作量=1100 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥2
- **HSK** `MakeLeadBars_Hand`：铅矾矿(Anglesite)×10 -> 铅合金(LeadBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=1100 ｜ 研究:Craft_B1，技能:Crafting≥2
- **对比**：数值一致

### 3.15 OrePowder（OrePowder）

#### 研磨矿粉（`MMT_GrindOrePowder_Hand`）

- **MMT**：[StoneChunks]×1 -> 矿粉×10 @ 手动研磨机 ｜ 工作量=2400 ｜ 研究:Stonecutting(Stonecutting)
- **HSK**：无对应资源（MMT 新增）

#### 研磨矿粉（`MMT_GrindOrePowder_Electric`）

- **MMT**：[StoneChunks]×1 -> 矿粉×10 @ 电动研磨机 ｜ 工作量=1200
- **HSK**：无对应资源（MMT 新增）

#### 筛选矿粉（`MMT_SiftOrePowder_Hand`）

- **MMT**：矿粉×15 -> — @ 手动筛子 ｜ 工作量=1800 ｜ 研究:Stonecutting(Stonecutting)
- **HSK**：无对应资源（MMT 新增）

#### 筛选矿粉（`MMT_SiftOrePowder_Electric`）

- **MMT**：矿粉×20 -> — @ 电动筛子 ｜ 工作量=900
- **HSK**：无对应资源（MMT 新增）

### 3.16 零件（Parts）

#### 使用木板组装原始零部件（`MMT_MakeComponentMedieval_Wood`）

- **MMT**：原木(WoodLog)×20 -> 原始零部件×10 @ 手工装配台, 电动装配台 ｜ 工作量=900 ｜ 研究:零件加工(MMT_ComponentCrafting)
- **HSK** `Make_ComponentMedieval`：[Woody]×20 -> 原始零部件(ComponentMedieval)×10 @ AdvToolBench, CraftingSpot, FabricationBench, 电动锯木台(TableSawmillElectric), 手工锯台(TableSawmillHand) ｜ 工作量=900 ｜ 研究:Craft_0
- **对比**：数值一致

#### 使用金属组装原始零部件（`MMT_MakeComponentMedieval_Metal`）

- **MMT**：钢(Steel) 或 青铜 或 铜 或 锡 或 铝合金 或 银(Silver) 或 金(Gold)×10 -> 原始零部件×10 @ 手工装配台, 电动装配台 ｜ 工作量=900 ｜ 研究:零件加工(MMT_ComponentCrafting)
- **HSK** `Make_ComponentMedieval`：[Woody]×20 -> 原始零部件(ComponentMedieval)×10 @ AdvToolBench, CraftingSpot, FabricationBench, 电动锯木台(TableSawmillElectric), 手工锯台(TableSawmillHand) ｜ 工作量=900 ｜ 研究:Craft_0
- **对比**：首投入 10 vs HSK 20

#### 组装普通零部件（`MMT_MakeComponentIndustrial_Hand`）

- **MMT**：钢(Steel) 或 青铜 或 斯坦合金 或 斯特合金×30 -> 工业零件(ComponentIndustrial)×10 @ 手工装配台 ｜ 工作量=2000 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥3
- **HSK** `Make_ComponentIndustrial`：[SLDBar] 或 [USLDBar]×30 -> 工业零件(ComponentIndustrial)×10 @ AdvToolBench, FabricationBench ｜ 工作量=2000 ｜ 研究:Craft_0，技能:Crafting≥3
- **对比**：数值一致

#### 组装普通零部件（`MMT_MakeComponentIndustrial_Electric`）

- **MMT**：钢(Steel) 或 青铜 或 斯坦合金 或 斯特合金×30 -> 工业零件(ComponentIndustrial)×10 @ 电动装配台 ｜ 工作量=2000 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥3
- **HSK** `Make_ComponentIndustrial`：[SLDBar] 或 [USLDBar]×30 -> 工业零件(ComponentIndustrial)×10 @ AdvToolBench, FabricationBench ｜ 工作量=2000 ｜ 研究:Craft_0，技能:Crafting≥3
- **对比**：数值一致

#### 制作电线（`MMT_MakeWire_Hand`）

- **MMT**：铜×10 -> 电线×20 @ 手工装配台 ｜ 工作量=800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥3
- **HSK** `MakeWires_Electric`：[HCMBar]×20 -> 电线(Wire)×40 @ AdvToolBench ｜ 工作量=800 ｜ 技能:Crafting≥6
- **对比**：首投入 10 vs HSK 20；产出 20 vs HSK 40；技能 Crafting≥3 vs HSK Crafting≥6

#### 制作电线（`MMT_MakeWire_Electric`）

- **MMT**：铜×20 -> 电线×40 @ 电动装配台 ｜ 工作量=800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥6
- **HSK** `MakeWires_Electric`：[HCMBar]×20 -> 电线(Wire)×40 @ AdvToolBench ｜ 工作量=800 ｜ 技能:Crafting≥6
- **对比**：数值一致

#### 组装机械组件（`MMT_MakeMechanism_Hand`）

- **MMT**：钢(Steel) 或 青铜 或 斯特合金×30 + 工业零件(ComponentIndustrial)×10 -> 机械组件×5 @ 手工装配台 ｜ 工作量=1800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥4
- **HSK** `MakeSimpleMechanism_Electric`：[SLDBar]×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件(Mechanism)×10 @ AdvToolBench ｜ 工作量=1800 ｜ 技能:Crafting≥7
- **对比**：首投入 30 vs HSK 60；产出 5 vs HSK 10；技能 Crafting≥4 vs HSK Crafting≥7

#### 组装机械组件（`MMT_MakeMechanism_Electric`）

- **MMT**：钢(Steel) 或 青铜 或 斯特合金×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件×10 @ 电动装配台 ｜ 工作量=1800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥7
- **HSK** `MakeSimpleMechanism_Electric`：[SLDBar]×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件(Mechanism)×10 @ AdvToolBench ｜ 工作量=1800 ｜ 技能:Crafting≥7
- **对比**：数值一致

#### 组装高级零部件（`MMT_MakeComponentSpacer_Hand`）

- **MMT**：铁钛合金(Plasteel) 或 斯坦合金 或 钛合金 或 镍钛合金 或 亚力可合金×30 -> 太空零件(ComponentSpacer)×10 @ 手工装配台 ｜ 工作量=2500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥6
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：首投入 30 vs HSK 25；工作量 2500 vs HSK 3500；技能 Crafting≥6 vs HSK Crafting≥9

#### 组装高级零部件（`MMT_MakeComponentSpacer_Electric`）

- **MMT**：铁钛合金(Plasteel) 或 斯坦合金 或 钛合金 或 镍钛合金 或 亚力可合金×30 -> 太空零件(ComponentSpacer)×10 @ 电动装配台 ｜ 工作量=2500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥6
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：首投入 30 vs HSK 25；工作量 2500 vs HSK 3500；技能 Crafting≥6 vs HSK Crafting≥9

#### 组装先进机械组件（`MMT_MakeAdvMechanism_Hand`）

- **MMT**：铁钛合金(Plasteel) 或 斯坦合金 或 钛合金 或 镍钛合金 或 亚力可合金×30 + 太空零件(ComponentSpacer)×10 + 人造纤维×5 -> 先进机械组件×5 @ 手工装配台 ｜ 工作量=3200 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥13
- **HSK** `MakeAdvMechanism_Electric`：[USLDBar]×60 + 高级零件(ComponentAdvanced)×20 + 人造纤维(SyntheticFibers)×10 -> 先进机械组件(AdvMechanism)×10 @ AdvToolBench ｜ 工作量=3200 ｜ 研究:Components_D1，技能:Crafting≥16
- **对比**：首投入 30 vs HSK 60；产出 5 vs HSK 10；技能 Crafting≥13 vs HSK Crafting≥16

#### 组装先进机械组件（`MMT_MakeAdvMechanism_Electric`）

- **MMT**：铁钛合金(Plasteel) 或 斯坦合金 或 钛合金 或 镍钛合金 或 亚力可合金×60 + 太空零件(ComponentSpacer)×20 + 人造纤维×10 -> 先进机械组件×10 @ 电动装配台 ｜ 工作量=3200 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥16
- **HSK** `MakeAdvMechanism_Electric`：[USLDBar]×60 + 高级零件(ComponentAdvanced)×20 + 人造纤维(SyntheticFibers)×10 -> 先进机械组件(AdvMechanism)×10 @ AdvToolBench ｜ 工作量=3200 ｜ 研究:Components_D1，技能:Crafting≥16
- **对比**：数值一致

#### 组装高级零部件（`MMT_MakeSpacerComponent`）

- **MMT**：铁钛合金(Plasteel) 或 斯坦合金 或 钛合金 或 镍钛合金 或 亚力可合金×25 + 银(Silver) 或 金(Gold)×5 -> 高级零部件×10 @ 电动装配台 ｜ 工作量=3500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥9
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：数值一致

#### 组装超级零部件（`MMT_MakeUltraComponent`）

- **MMT**：钨合金 或 钛合金×15 + 司太立合金（钨铬钴合金） 或 钨钴合金×10 + 银(Silver) 或 金(Gold)×5 -> 超级零部件×10 @ 自动化装配器 ｜ 工作量=4500 ｜ 研究:超级零件(MMT_UltraComponents)，技能:Crafting≥13
- **HSK** `Make_ComponentUltra`：[RARBar]×15 + [USLDHBar]×10 + [PRSBar]×5 -> 超级零部件(ComponentUltra)×10 @ 自动化装配器(RobAssem) ｜ 工作量=4500 ｜ 研究:Components_E1，技能:Crafting≥13
- **对比**：数值一致

### 3.17 石化（Petrochem）

#### 压取原油（`MMT_PumpCrudeOil`）

- **MMT**：— -> 原油×10 @ 手压油泵 ｜ 工作量=4000 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Mining≥3
- **HSK**：无对应资源（MMT 新增）

#### 蒸馏聚合物（`MMT_DistillPolymers_Hand`）

- **MMT**：原油×20 -> 聚合物×15 @ 石化产品实验室 ｜ 工作量=1500 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥4
- **HSK** `ConvertToPolymers`：无机物(Matter)×50 -> 聚合物(Polymers)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 20 vs HSK 50；产出 15 vs HSK 100；工作量 1500 vs HSK 12000；技能 Crafting≥4 vs HSK Crafting≥15

#### 制作塑料（`MMT_MakePlastic_Hand`）

- **MMT**：聚合物×30 -> 塑料×10 @ 石化产品实验室 ｜ 工作量=800 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥6
- **HSK** `MakePlastic`：聚合物(Polymers)×30 -> 塑料(Plastic)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:Crafting≥10
- **对比**：技能 Crafting≥6 vs HSK Crafting≥10

#### 制作合成橡胶（`MMT_MakeRubber_Hand`）

- **MMT**：聚合物×30 + 硫磺×3 -> 合成橡胶×10 @ 石化产品实验室 ｜ 工作量=800 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥6
- **HSK** `MakeRubber`：聚合物(Polymers)×30 + 硫酸盐(Sulphates)×3 -> 合成橡胶(Rubber)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:Crafting≥10
- **对比**：技能 Crafting≥6 vs HSK Crafting≥10

#### 制作人造纤维（`MMT_MakeSyntheticFibers_Hand`）

- **MMT**：聚合物×25 + 硫磺×3 -> 人造纤维×5 @ 石化产品实验室 ｜ 工作量=1200 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥8
- **HSK** `MakeSyntheticFibers`：聚合物(Polymers)×25 + 硫酸盐(Sulphates)×3 -> 人造纤维(SyntheticFibers)×5 @ 石化产品实验室(TableChemlab) ｜ 工作量=1200 ｜ 研究:Oil_Industry_C6，技能:Crafting≥14
- **对比**：技能 Crafting≥8 vs HSK Crafting≥14

#### 制作碳聚物（`MMT_MakeCarbon_Hand`）

- **MMT**：聚合物×25 + 木炭 或 原煤×25 + 合成橡胶×15 -> 碳聚物×12 @ 石化产品实验室 ｜ 工作量=4000 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥10
- **HSK** `MakeCarbon`：聚合物(Polymers)×25 + [Coal]×25 + 合成橡胶(Rubber)×15 -> 碳聚物(Carbon)×12 @ 石化产品实验室(TableChemlab) ｜ 工作量=4000 ｜ 研究:Oil_Industry_C6，技能:Crafting≥15
- **对比**：技能 Crafting≥10 vs HSK Crafting≥15

### 3.18 PetrochemExt（PetrochemExt）

#### 精炼石蜡（10个）（`MMT_MakeParaffins`）

- **MMT**：原油×20 -> 石蜡×10 @ 石化产品实验室, 石化产品实验室 ｜ 工作量=1000 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToParaffins`：无机物(Matter)×50 -> 石蜡(Paraffins)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 20 vs HSK 50；产出 10 vs HSK 100；工作量 1000 vs HSK 12000；技能 Crafting≥6 vs HSK Crafting≥15

#### 制备硫酸盐（10个）（`MMT_MakeSulphates`）

- **MMT**：硫磺×10 -> 硫酸盐×10 @ 石化产品实验室, 石化产品实验室 ｜ 工作量=800 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToSulphates`：无机物(Matter)×50 -> 硫酸盐(Sulphates)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 10 vs HSK 50；产出 10 vs HSK 100；工作量 800 vs HSK 12000；技能 Crafting≥6 vs HSK Crafting≥15

#### 合成碳纤维锭（10个）（`MMT_MakeCarbonAlloy`）

- **MMT**：人造纤维×10 + 碳聚物×10 -> 碳纤维锭×10 @ 石化产品实验室 ｜ 工作量=2800 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥15
- **HSK** `MakeCarbonAlloy`：人造纤维(SyntheticFibers)×10 + 碳聚物(Carbon)×10 -> 碳纤维锭(CarbonAlloy)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=2800 ｜ 研究:Oil_Industry_C6，技能:Crafting≥15
- **对比**：数值一致

#### 调配复合粘剂（20个）（`MMT_MakeCompaste`）

- **MMT**：Chemfuel×10 + 合成橡胶×5 + 石蜡×2 -> 复合粘剂×20 @ 纺织机 ｜ 工作量=3200 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥5
- **HSK** `MakeCompaste`：Chemfuel×10 + 合成橡胶(Rubber)×5 + 石蜡(Paraffins)×2 -> 复合粘剂(Compaste)×20 @ TableLoom ｜ 工作量=3200 ｜ 研究:AdvancedFabrication，技能:Crafting≥5
- **对比**：数值一致

#### 制备表面活性剂（50个）（`MMT_MakeSurfactant`）

- **MMT**：盐×10 + 硫磺×15 + [MMT_Alcohol]×15 -> 表面活性剂×50 @ — ｜ 工作量=4000 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥8
- **HSK** `MakeSurfactant`：盐(Salt)×10 + 硫磺(Sulfur)×15 + [Alcohol_Bottles]×15 -> 表面活性剂(Surfactant)×50 @ Brewery, DrugLab, ElectricBrewery ｜ 工作量=4000 ｜ 研究:Firefoam，技能:Crafting≥8
- **对比**：数值一致

#### 合成表面活性剂（50个）（`MMT_MakeSurfactant_Compaste`）

- **MMT**：复合粘剂×15 + 聚合物×25 -> 表面活性剂×50 @ — ｜ 工作量=4000 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥12,Intellectual≥4
- **HSK** `MakeSurfactant_Polymers`：复合粘剂(Compaste)×15 + 聚合物(Polymers)×25 -> 表面活性剂(Surfactant)×50 @ DrugLab ｜ 工作量=4000 ｜ 研究:Oil_Industry_C5，技能:Crafting≥12, Intellectual≥4
- **对比**：技能 Crafting≥12,Intellectual≥4 vs HSK Crafting≥12, Intellectual≥4

#### 提炼硫磺（10个）（`MMT_MakeSulfur`）

- **MMT**：铁矿石×35 -> 硫磺×10 @ 弹药工作台 ｜ 工作量=1200
- **HSK** `MakeSulfur`：铁矿石(Iron)×35 -> 硫磺(Sulfur)×10 @ 弹药工作台(AmmoBench), TableHeavyAmmunition ｜ 工作量=1200
- **对比**：数值一致

#### 配制火药（20个）（`MMT_MakePowder`）

- **MMT**：硝石×75 + [MMT_Coal]×20 + 硫磺×20 -> 火药×20 @ 弹药工作台 ｜ 工作量=3000 ｜ 技能:Crafting≥3
- **HSK** `MakePowder`：硝石(Nitre)×75 + [Coal]×20 + 硫磺(Sulfur)×20 -> 火药(Powder)×20 @ 弹药工作台(AmmoBench), TableHeavyAmmunition ｜ 工作量=3000 ｜ 技能:Crafting≥3
- **对比**：数值一致

#### 配制凝固汽油（10个）（`MMT_MakeNapalm`）

- **MMT**：Chemfuel×30 + 聚合物×15 -> 凝固汽油×10 @ 弹药工作台 ｜ 工作量=1200 ｜ 技能:Crafting≥5
- **HSK** `MakeNapalm`：Chemfuel×30 + 聚合物(Polymers)×15 -> 凝固汽油(Napalm)×10 @ 弹药工作台(AmmoBench), TableHeavyAmmunition ｜ 工作量=1200 ｜ 技能:Crafting≥5
- **对比**：数值一致

#### 合成氧化化合燃料（75个）（`MMT_MakeOxidizedChemfuel`）

- **MMT**：Chemfuel×100 + 硝石×15 + 硫酸盐×15 + 聚合物×5 -> 氧化化合燃料×75 @ 石化产品实验室 ｜ 工作量=10000 ｜ 研究:氧化燃料(MMT_OxidizedFuel)，技能:Crafting≥12
- **HSK** `MakeOxidizedChemfuel`：Chemfuel×100 + 硝石(Nitre)×10 + 硫酸盐(Sulphates)×10 + Prometheum 或 FSX×5 -> 氧化化合燃料(OxidizedChemfuel)×75 @ 石化产品实验室(TableChemlab) ｜ 工作量=10000 ｜ 研究:BasicGravtech，技能:Crafting≥12
- **对比**：数值一致

#### 制备表面活性剂（椰子替代，50个）（`MMT_MakeSurfactant_CoconutSubstitute`）

- **MMT**：盐×10 + 硫磺×15 + 椰子×25 -> 表面活性剂×50 @ — ｜ 工作量=4000 ｜ 研究:Firefoam(Firefoam)，技能:Crafting≥12,Intellectual≥4
- **HSK** `MakeSurfactant`：盐(Salt)×10 + 硫磺(Sulfur)×15 + [Alcohol_Bottles]×15 -> 表面活性剂(Surfactant)×50 @ Brewery, DrugLab, ElectricBrewery ｜ 工作量=4000 ｜ 研究:Firefoam，技能:Crafting≥8
- **对比**：技能 Crafting≥12,Intellectual≥4 vs HSK Crafting≥8

### 3.19 贵金属（Precious）

#### 提炼纯银合金（10个）（`MMT_SmeltSilver_Hand`）

- **MMT**：白银×14 -> 银(Silver)×10 @ 熔炉 ｜ 工作量=750 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥2
- **HSK** `ConvertToSilver`：无机物(Matter)×100 -> 白银(Silver)×300 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 14 vs HSK 100；产出 10 vs HSK 300；工作量 750 vs HSK 12000；技能 Crafting≥2 vs HSK Crafting≥15

#### 提炼纯金合金（15个）（`MMT_SmeltGold_Hand`）

- **MMT**：金矿×12 -> 金(Gold)×15 @ 熔炉 ｜ 工作量=750 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥3
- **HSK** `ConvertToGold`：无机物(Matter)×60 -> 金矿(Gold)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 12 vs HSK 60；产出 15 vs HSK 20；工作量 750 vs HSK 12000；技能 Crafting≥3 vs HSK Crafting≥15

### 3.20 Robotics（Robotics）

#### 制造动力电池（`MMT_MakeHexcell`）

- **MMT**：磁性材料×2 + [MMT_HCMBar]×4 + 微芯片集成电路×1 -> 动力电池×10 @ 电子器件工作台 ｜ 工作量=6000 ｜ 研究:机器人工程 I(MMT_Robotics1)，技能:Crafting≥14
- **HSK** `MakeHexcell`：磁性材料(MagneticMaterial)×2 + [HCMBar]×4 + 微芯片集成电路(Microchips)×1 -> 动力电池(Hexcell)×10 @ 电子器件工作台(TableElectronics) ｜ 工作量=6000 ｜ 研究:Droids_D1，技能:Crafting≥14
- **对比**：数值一致

#### 制造机器人部件（`MMT_MakeRobotParts`）

- **MMT**：[MMT_USLDBar]×60 + 先进机械组件×15 -> 机器人部件×25 @ 电子器件工作台 ｜ 工作量=8000 ｜ 研究:机器人工程 I(MMT_Robotics1)，技能:Crafting≥15
- **HSK** `MakeRobotParts`：[USLDBar]×60 + 先进机械组件(AdvMechanism)×15 -> 机器人部件(RobotParts)×25 @ 电子器件工作台(TableElectronics) ｜ 工作量=8000 ｜ 研究:Droids_D1，技能:Crafting≥15
- **对比**：数值一致

#### 组装矿物声纳模块（`MMT_MakeMineralSonarModule`）

- **MMT**：[MMT_MetallicBar]×25 + 太空零件(ComponentSpacer)×5 + 电子元件×10 + 机械组件×1 -> 矿物声纳模块×1 @ 电子器件工作台, 电动装配台 ｜ 工作量=4800 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥10
- **HSK** `MakeMineralSonarModule`：[Metallic]×25 + 高级零件(ComponentAdvanced)×5 + 电子元件(ElectronicComponents)×10 + 机械组件(Mechanism)×1 -> 矿物声纳模块(MineralSonarModule)×1 @ AdvToolBench, 电子器件工作台(TableElectronics) ｜ 工作量=4800 ｜ 研究:Mining_C1，技能:Crafting≥10
- **对比**：数值一致

#### 拆解护盾腰带（`MMT_DeconstructPersonalShield`）

- **MMT**：Apparel_ShieldBelt×1 -> 太空零件(ComponentSpacer)×8 + 集成电路×1 @ — ｜ 工作量=1000 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥10
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：首投入 1 vs HSK 25；产出 8 vs HSK 10；工作量 1000 vs HSK 3500；技能 Crafting≥10 vs HSK Crafting≥9

### 3.21 Textiles（Textiles）

#### 纺织布（`MMT_MakeCloth`）

- **MMT**：原棉×20 -> Cloth×20 @ 纺织机 ｜ 工作量=600 ｜ 研究:ComplexClothing(ComplexClothing)，技能:Crafting≥2
- **HSK** `MakeCloth`：原棉(RawCotton)×20 -> Cloth×20 @ TableLoom ｜ 工作量=600 ｜ 研究:Apparel_B1，技能:Crafting≥2
- **对比**：数值一致

#### 纺织亚麻布（`MMT_MakeFlaxcloth`）

- **MMT**：生亚麻×20 -> 亚麻布×10 @ 纺织机 ｜ 工作量=600 ｜ 技能:Crafting≥4
- **HSK** `MakeLinen`：生亚麻(RawFlax)×20 -> 亚麻布(Flaxcloth)×10 @ LeatherworkerTable, TableLoom ｜ 工作量=600 ｜ 技能:Crafting≥4
- **对比**：数值一致

#### 纺织大麻布（`MMT_MakeHempCloth`）

- **MMT**：SmokeleafLeaves×30 -> 大麻布×15 @ 纺织机 ｜ 工作量=1200 ｜ 研究:ComplexClothing(ComplexClothing)，技能:Crafting≥6
- **HSK** `MakeHempCloth`：SmokeleafLeaves×30 -> 大麻布(HempCloth)×15 @ TableLoom ｜ 工作量=1200 ｜ 研究:Apparel_B1，技能:Crafting≥6
- **对比**：数值一致

#### 纺织魔菇布（`MMT_MakeDevilstrand`）

- **MMT**：生恶魔菇×15 + 人造纤维×2 -> DevilstrandCloth×15 @ 纺织机 ｜ 工作量=600 ｜ 研究:AdvancedFabrication(AdvancedFabrication)，技能:Crafting≥13
- **HSK** `MakeDevilstrand`：生恶魔菇(RawDevilstrand)×15 + 人造纤维(SyntheticFibers)×2 -> DevilstrandCloth×15 @ TableLoom ｜ 工作量=600 ｜ 研究:AdvancedFabrication，技能:Crafting≥13
- **对比**：数值一致

#### 纺织合成纤维（`MMT_MakeSynthread`）

- **MMT**：原棉×10 + 人造纤维×3 -> Synthread×10 @ 纺织机 ｜ 工作量=600 ｜ 研究:AdvancedFabrication(AdvancedFabrication)，技能:Crafting≥12
- **HSK** `MakeSynthreadCloth`：原棉(RawCotton)×10 + 人造纤维(SyntheticFibers)×3 -> Synthread×10 @ TableLoom ｜ 工作量=600 ｜ 研究:AdvancedFabrication，技能:Crafting≥12
- **对比**：数值一致

#### 纺织丝绒（`MMT_MakeVelour`）

- **MMT**：复合粘剂×5 + [MMT_BTextiles]×40 -> 丝绒×25 @ 纺织机 ｜ 工作量=1800 ｜ 研究:AdvancedFabrication(AdvancedFabrication)，技能:Crafting≥12
- **HSK** `MakeVelour`：复合粘剂(Compaste)×5 + [BTextiles]×40 -> 丝绒(Velour)×25 @ TableLoom ｜ 工作量=1800 ｜ 研究:AdvancedFabrication，技能:Crafting≥12
- **对比**：数值一致

#### 纺织凯夫拉（`MMT_MakeKevlar`）

- **MMT**：塑料×30 + 人造纤维×15 -> 凯夫拉×10 @ 纺织机 ｜ 工作量=1800 ｜ 研究:AdvancedFabrication(AdvancedFabrication)，技能:Crafting≥13
- **HSK** `MakeKevlar`：塑料(Plastic)×30 + 人造纤维(SyntheticFibers)×15 -> 凯夫拉(Kevlar)×10 @ TableLoom ｜ 工作量=1800 ｜ 研究:AdvancedFabrication，技能:Crafting≥13
- **对比**：数值一致

#### 纺织超织物（`MMT_MakeHyperweave`）

- **MMT**：Synthread×10 + DevilstrandCloth×10 -> Hyperweave×10 @ 纺织机 ｜ 工作量=600 ｜ 研究:超织物工艺(MMT_SuperFabric)，技能:Crafting≥17
- **HSK** `MakeHyperweaveCloth`：Synthread×10 + DevilstrandCloth×10 -> Hyperweave×10 @ TableLoom ｜ 工作量=600 ｜ 研究:Apparel_D1，技能:Crafting≥17
- **对比**：数值一致

#### 压制甲壳板（`MMT_MakeChitinPlating`）

- **MMT**：甲壳×50 + 人造纤维×4 + 复合粘剂×1 -> 甲壳板×15 @ 纺织机 ｜ 工作量=1080 ｜ 研究:AdvancedFabrication(AdvancedFabrication)，技能:Crafting≥12
- **HSK** `MakeChitinPlating`：甲壳(Chitin)×50 + 人造纤维(SyntheticFibers)×4 + 复合粘剂(Compaste)×1 -> 甲壳板(ChitinPlating)×15 @ TableLoom ｜ 工作量=1080 ｜ 研究:AdvancedFabrication，技能:Crafting≥12
- **对比**：数值一致

#### 提炼兽脂（`MMT_MakeTallow`）

- **MMT**：[MeatRaw]×30 -> 兽脂×15 @ — ｜ 工作量=1200 ｜ 研究:Pemmican(Pemmican)，技能:Cooking≥3
- **HSK** `MakeTallow`：[MeatRaw]×30 -> 兽脂(Tallow)×15 @ Campfire, FueledStove ｜ 工作量=1200 ｜ 研究:Food_00，技能:Cooking≥3
- **对比**：数值一致

#### 调制绘画用品（花卉）（`MMT_CookPaint`）

- **MMT**：野玫瑰 或 金丝桃 或 薄荷叶×10 + 原木(WoodLog) 或 木板 或 竹节 或 竹板 或 红木 或 红木木板×2 -> 绘画用品×1 @ — ｜ 工作量=550 ｜ 研究:艺术用品(MMT_ArtSupplies)，技能:Crafting≥5
- **HSK** `CookPaint`：野玫瑰(WildRose) 或 金丝桃(Hypericum) 或 薄荷叶(MintLeaves)×10 + [Woody]×2 -> 绘画用品(PaintingSupplies)×1 @ ElectricStove, FueledStove ｜ 工作量=550 ｜ 研究:Art_A1，技能:Crafting≥5
- **对比**：数值一致

#### 调制绘画用品（染料）（`MMT_CookPaintDye`）

- **MMT**：染料×10 + 原木(WoodLog) 或 木板 或 竹节 或 竹板 或 红木 或 红木木板×2 -> 绘画用品×1 @ — ｜ 工作量=550 ｜ 研究:艺术用品(MMT_ArtSupplies)，技能:Crafting≥5
- **HSK** `CookPaint`：野玫瑰(WildRose) 或 金丝桃(Hypericum) 或 薄荷叶(MintLeaves)×10 + [Woody]×2 -> 绘画用品(PaintingSupplies)×1 @ ElectricStove, FueledStove ｜ 工作量=550 ｜ 研究:Art_A1，技能:Crafting≥5
- **对比**：数值一致

#### 提炼染料（`MMT_MakeDye`）

- **MMT**：野玫瑰 或 金丝桃 或 薄荷叶×4 -> 染料×8 @ — ｜ 工作量=400 ｜ 研究:艺术用品(MMT_ArtSupplies)，技能:Crafting≥3
- **HSK**：无对应资源（MMT 新增）

### 3.22 TopMaterials（TopMaterials）

#### 合成磁性材料（`MMT_BuildMagneticMaterial`）

- **MMT**：无机物×30 + 硫酸盐×15 + [MMT_USLDBar]×10 + [MMT_SLDBar]×5 -> 磁性材料×5 @ — ｜ 工作量=2400 ｜ 研究:超级零件(MMT_UltraComponents)，技能:Crafting≥18
- **HSK** `BuildMagneticMaterial`：无机物(Matter)×30 + 硫酸盐(Sulphates)×15 + [USLDBar]×10 + [SLDBar]×5 -> 磁性材料(MagneticMaterial)×5 @ 自动化装配器(RobAssem) ｜ 工作量=2400 ｜ 研究:Components_D3，技能:Crafting≥18
- **对比**：数值一致

#### 合成生物合成材料（`MMT_BuildBiosyntheticMaterial`）

- **MMT**：生物质×15 + 人造纤维×9 + 塑料×30 -> 生物合成材料×5 @ — ｜ 工作量=2400 ｜ 研究:超级零件(MMT_UltraComponents)，技能:Crafting≥18
- **HSK** `BuildBiosyntheticMaterial`：生物质(Biomatter)×15 + 人造纤维(SyntheticFibers)×9 + 塑料(Plastic)×30 -> 生物合成材料(BiosyntheticMaterial)×5 @ 自动化装配器(RobAssem) ｜ 工作量=2400 ｜ 技能:Crafting≥18
- **对比**：数值一致

#### 组装生物电子元件（`MMT_MakeBioelectronicDevices`）

- **MMT**：生物质×60 + Hyperweave×15 + 微芯片集成电路×2 -> 生物电子元件×1 @ — ｜ 工作量=7000 ｜ 研究:超级零件(MMT_UltraComponents)，技能:Crafting≥18
- **HSK** `MakeBioelectronicDevices`：生物质(Biomatter)×60 + Hyperweave×15 + 微芯片集成电路(Microchips)×2 -> 生物电子元件(BioMicrochips)×1 @ 自动化装配器(RobAssem) ｜ 工作量=7000 ｜ 研究:Components_D3，技能:Crafting≥18
- **对比**：数值一致

#### 拆解伪人格核心（`MMT_DisassembleAIPersonaCore`）

- **MMT**：AIPersonaCore×1 -> 生物电子元件×1 + α阿尔法聚合物×30 + 碳纤维锭×40 @ — ｜ 工作量=4000 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥16
- **HSK** `DisassembleAIPersonaCore`：AIPersonaCore×1 -> 生物电子元件(BioMicrochips)×1 + α阿尔法聚合物(AlphaPoly)×30 + 碳纤维锭(CarbonAlloy)×40 @ 自动化装配器(RobAssem) ｜ 工作量=4000 ｜ 研究:Components_D2，技能:Crafting≥16
- **对比**：数值一致

#### 制造α阿尔法聚合物（`MMT_ProduceAPoly`）

- **MMT**：贫铀合金×5 + 碳纤维锭×5 + 反物质×3 -> α阿尔法聚合物×15 @ 量子加工台 ｜ 工作量=8000 ｜ 技能:Crafting≥19
- **HSK** `ProduceAPoly`：贫铀合金(DepletedUranium)×5 + 碳纤维锭(CarbonAlloy)×5 + 反物质(Antimatter)×3 -> α阿尔法聚合物(AlphaPoly)×15 @ 量子加工台(Matterfab) ｜ 工作量=8000 ｜ 技能:Crafting≥19
- **对比**：数值一致

#### 制造β贝塔聚合物（`MMT_ProduceBPoly`）

- **MMT**：α阿尔法聚合物×3 + 生物合成材料×5 -> β贝塔聚合物×5 @ 量子加工台 ｜ 工作量=12000 ｜ 技能:Crafting≥20
- **HSK** `ProduceBPoly`：α阿尔法聚合物(AlphaPoly)×3 + 生物合成材料(BiosyntheticMaterial)×5 -> β贝塔聚合物(BetaPoly)×5 @ 量子加工台(Matterfab) ｜ 工作量=12000 ｜ 技能:Crafting≥20
- **对比**：数值一致

#### 制备反物质（`MMT_MakeAntimatter`）

- **MMT**：无机物×50 + Uranium×20 -> 反物质×2 @ 粒子对撞台 ｜ 工作量=10000 ｜ 技能:Crafting≥15
- **HSK**：有资源 Antimatter，但无投入匹配的配方（HSK 侧共有 0 条产出配方）

#### 从尸体提取生物质（`MMT_ExtractBiomatterCorpse`）

- **MMT**：[Corpses]×1 -> 生物质×20 @ 石化产品实验室 ｜ 工作量=6000 ｜ 研究:物质转换 I(MMT_MatterConversion1)，技能:Crafting≥8
- **HSK**：有资源 Biomatter，但无投入匹配的配方（HSK 侧共有 0 条产出配方）

#### 从生肉提取生物质（`MMT_ExtractBiomatterMeat`）

- **MMT**：[MeatRaw]×40 -> 生物质×20 @ 石化产品实验室 ｜ 工作量=4000 ｜ 研究:物质转换 I(MMT_MatterConversion1)，技能:Crafting≥8
- **HSK**：有资源 Biomatter，但无投入匹配的配方（HSK 侧共有 0 条产出配方）

#### 熔炼生物铁合金（`MMT_MakeBioferriteAlloy`）

- **MMT**：Bioferrite×10 + 铁矿石×10 + [MMT_Coal]×5 -> 生物铁合金×10 @ — ｜ 工作量=2100 ｜ 研究:BioferriteShaping(BioferriteShaping)，技能:Crafting≥9
- **HSK** `MakeBioferriteAlloy`：Bioferrite×10 + 铁矿石(Iron)×10 + [Coal]×5 -> 生物铁合金(BioferriteAlloy)×10 @ BioferriteShaper ｜ 工作量=2100 ｜ 研究:BioferriteShaping，技能:Crafting≥9
- **对比**：数值一致

### 3.23 TurretAmmo（TurretAmmo）

#### 制作120mm 炮弹（破甲）x1（`MMT_MakeAmmo_120mmCannonShell_HEAT`）

- **MMT**：钢(Steel)×90 + 工业零件(ComponentIndustrial)×2 + Chemfuel×7 -> 120mm 炮弹（破甲）×2 @ — ｜ 工作量=13000 ｜ 技能:Crafting≥4
- **HSK**：无对应资源（MMT 新增）

#### 制作130mm 制导火箭弹x1（`MMT_MakeAmmo_130mmRocketMissile`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×42 + [MMT_HCMBar]×8 + Chemfuel×32 + 凝固汽油×5 + 集成电路×1 -> 130mm 制导火箭弹（高爆弹）×1 @ — ｜ 工作量=2000
- **HSK**：无对应资源（MMT 新增）

#### 制作155mm 炮弹（高爆）x1（`MMT_MakeAmmo_155mmHowitzerShell_HE`）

- **MMT**：钢(Steel)×94 + 工业零件(ComponentIndustrial)×2 + Chemfuel×17 -> 155mm 炮弹（高爆弹）×1 @ — ｜ 工作量=17400 ｜ 技能:Crafting≥4
- **HSK**：无对应资源（MMT 新增）

#### 制作20x102mm  弹药（全金属被甲弹）x40（`MMT_MakeAmmo_20x102mm_FMJ`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×50 + [MMT_HCMBar]×15 + 火药×9 -> 20x102mm 弹药（全金属被甲弹）×40 @ — ｜ 工作量=3000 ｜ 技能:Crafting≥10
- **HSK**：无对应资源（MMT 新增）

#### 制作30x173mm 弹药（全金属被甲弹）x30（`MMT_MakeAmmo_30x173mm_FMJ`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×60 + [MMT_HCMBar]×16 + 火药×8 -> 30x173mm 弹药（全金属被甲弹）×30 @ — ｜ 工作量=3000 ｜ 技能:Crafting≥11
- **HSK**：无对应资源（MMT 新增）

#### 制作30x29mm 榴弹（高爆）x20（`MMT_MakeAmmo_30x29mmGrenade_HE`）

- **MMT**：钢(Steel)×74 + Chemfuel×8 + 工业零件(ComponentIndustrial)×2 -> 30x29mm 榴弹（高爆弹）×100 @ — ｜ 工作量=11800 ｜ 技能:Crafting≥4
- **HSK**：无对应资源（MMT 新增）

#### 制作.50  机枪弹（全金属被甲弹）x60（`MMT_MakeAmmo_50BMG_FMJ`）

- **MMT**：钢(Steel)×50 + 火药×25 -> .50 机枪弹（全金属被甲弹）×200 @ — ｜ 工作量=5000
- **HSK**：无对应资源（MMT 新增）

#### 制作7.62x51mm 北约标准弹（全金属被甲弹）x60（`MMT_MakeAmmo_762x51mmNATO_FMJ`）

- **MMT**：钢(Steel)×26 + 火药×13 -> 7.62x51mm 北约标准弹（全金属被甲弹）×500 @ — ｜ 工作量=2600
- **HSK**：无对应资源（MMT 新增）

#### 制作弩炮箭x10（`MMT_MakeAmmo_BalistaBolt`）

- **MMT**：[MMT_Woody]×2 + [MMT_MetallicBar]×5 -> 弩炮箭×10 @ — ｜ 工作量=1500
- **HSK**：无对应资源（MMT 新增）

#### 制作石球x5（`MMT_MakeAmmo_StoneBall`）

- **MMT**：[StoneChunks]×1 -> 石球×5 @ — ｜ 工作量=1500
- **HSK**：无对应资源（MMT 新增）

#### make SPG-9 (FRAG) rockets x5（`MMT_MakeAmmo_SPG9Rocket_Frag`）

- **MMT**：钢(Steel)×66 + Chemfuel×6 + 工业零件(ComponentIndustrial)×4 -> SPG-9 Rocket (HE)×5 @ — ｜ 工作量=11400 ｜ 技能:Crafting≥4
- **HSK**：无对应资源（MMT 新增）

### 3.24 TurretCrates（TurretCrates）

#### 制作AGS-30组件箱（`MMT_BuildAGCturret_Crate`）

- **MMT**：武器部件×12 + 火炮武器部件×1 + [MMT_MetallicBar]×20 + 合成橡胶×10 -> AGS-30组件箱×1 @ — ｜ 工作量=15500 ｜ 研究:发射器 I(MMT_GrenadeLaunchers)，技能:Crafting≥8
- **HSK** `BuildAGCturret_Crate`：武器部件(Weapon_Parts)×12 + 火炮武器部件(Cannon_Component)×1 + [Metallic]×20 + 合成橡胶(Rubber)×10 -> AGS-30组件箱(AGCturret_Crate)×1 @ HeavyArmsBench ｜ 工作量=15500 ｜ 研究:Grenade_launchers_C2，技能:Crafting≥8
- **对比**：数值一致

#### 制作GAU-8复仇者火炮组件箱（`MMT_BuildAvenger_Crate`）

- **MMT**：武器部件×22 + 火炮武器部件×1 + [MMT_MetallicBar]×60 + 合成橡胶×35 -> GAU-8复仇者火炮组件箱×1 @ — ｜ 工作量=18500 ｜ 研究:重型炮塔 III(MMT_HeavyTurrets3)，技能:Crafting≥16
- **HSK** `BuildAvenger_Crate`：武器部件(Weapon_Parts)×22 + 火炮武器部件(Cannon_Component)×1 + [Metallic]×60 + 合成橡胶(Rubber)×35 -> GAU-8复仇者火炮组件箱(Avenger_Crate)×1 @ HeavyArmsBench ｜ 工作量=18500 ｜ 研究:Heavy_turrets_C3，技能:Crafting≥16
- **对比**：数值一致

#### 制作弩炮组件箱（`MMT_BuildBalista_Crate`）

- **MMT**：[MMT_Woody]×90 + [MMT_MetallicBar]×30 -> 弩炮组件箱×1 @ — ｜ 工作量=4500 ｜ 研究:攻城武器(MMT_SiegeWeapons)，技能:Crafting≥3
- **HSK** `BuildBalista_Crate`：[Wooden]×90 + [Metallic]×30 -> 弩炮组件箱(Balista_Crate)×1 @ CraftingSpot, HeavyArmsBench ｜ 工作量=4500 ｜ 研究:Ballista_B1，技能:Crafting≥3
- **对比**：数值一致

#### 制作120mm坦克炮组件箱（`MMT_BuildCannonTurret_Crate`）

- **MMT**：武器部件×14 + 火炮武器部件×2 + [MMT_MetallicBar]×55 + 合成橡胶×25 -> 120mm坦克炮组件箱×1 @ — ｜ 工作量=17500 ｜ 研究:火炮 II(MMT_Artillery2)，技能:Crafting≥14
- **HSK** `BuildCannonTurret_Crate`：武器部件(Weapon_Parts)×14 + 火炮武器部件(Cannon_Component)×2 + [Metallic]×55 + 合成橡胶(Rubber)×25 -> 120mm坦克炮组件箱(CannonTurret_Crate)×1 @ HeavyArmsBench ｜ 工作量=17500 ｜ 研究:Artillery_C2，技能:Crafting≥14
- **对比**：数值一致

#### 制作投石机组件箱（`MMT_BuildCatapult_Crate`）

- **MMT**：[MMT_Woody]×160 + [MMT_MetallicBar]×60 -> 投石机组件箱×1 @ — ｜ 工作量=10500 ｜ 研究:攻城武器(MMT_SiegeWeapons)，技能:Crafting≥5
- **HSK** `BuildCatapult_Crate`：[Wooden]×160 + [Metallic]×60 -> 投石机组件箱(Catapult_Crate)×1 @ CraftingSpot, HeavyArmsBench ｜ 工作量=10500 ｜ 研究:Ballista_B1，技能:Crafting≥5
- **对比**：数值一致

#### 制作155mm 榴弹炮组件箱（`MMT_BuildHowitzerTurret_Crate`）

- **MMT**：武器部件×18 + 火炮武器部件×2 + [MMT_MetallicBar]×70 + 合成橡胶×35 -> 155mm榴弹炮组件箱×1 @ — ｜ 工作量=20500 ｜ 研究:火炮 II(MMT_Artillery2)，技能:Crafting≥15
- **HSK** `BuildHowitzerTurret_Crate`：武器部件(Weapon_Parts)×18 + 火炮武器部件(Cannon_Component)×2 + [Metallic]×70 + 合成橡胶(Rubber)×35 -> 155mm榴弹炮组件箱(HowitzerTurret_Crate)×1 @ HeavyArmsBench ｜ 工作量=20500 ｜ 研究:Artillery_C2，技能:Crafting≥15
- **对比**：数值一致

#### 制作FGM-148“标枪”反坦克导弹组件箱（`MMT_BuildLightMissileSystem_Crate`）

- **MMT**：武器部件×16 + 发射器部件×1 + [MMT_MetallicBar]×35 + 合成橡胶×15 -> FGM-148“标枪”反坦克导弹组件箱×1 @ — ｜ 工作量=16500 ｜ 研究:火箭弹 II(MMT_Rocketry2)，技能:Crafting≥13
- **HSK** `BuildLightMissileSystem_Crate`：武器部件(Weapon_Parts)×16 + 发射器部件(Launcher_Component)×1 + [Metallic]×35 + 合成橡胶(Rubber)×15 -> FGM-148“标枪”反坦克导弹组件箱(LightMissileSystem_Crate)×1 @ HeavyArmsBench ｜ 工作量=16500 ｜ 研究:Rockets_C2，技能:Crafting≥13
- **对比**：数值一致

#### 制作M2 .50勃朗宁机枪组件箱（`MMT_BuildMGTurret_Crate`）

- **MMT**：武器部件×14 + 重型武器部件×1 + [MMT_MetallicBar]×40 -> M2 .50勃朗宁机枪组件箱×1 @ — ｜ 工作量=11500 ｜ 研究:SniperTurret(SniperTurret)，技能:Crafting≥7
- **HSK** `BuildMGTurret_Crate`：武器部件(Weapon_Parts)×14 + 重型武器部件(Heavy_Component)×1 + [Metallic]×40 -> M2 .50勃朗宁机枪组件箱(MGTurret_Crate)×1 @ HeavyArmsBench ｜ 工作量=11500 ｜ 研究:SniperTurret，技能:Crafting≥7
- **对比**：数值一致

#### 制作130mm中型导弹系统组件箱（`MMT_BuildMediumMissileSystem_Crate`）

- **MMT**：武器部件×10 + 发射器部件×2 + [MMT_MetallicBar]×35 -> 130mm中型导弹系统组件箱×1 @ — ｜ 工作量=18500 ｜ 研究:火箭弹 III(MMT_Rocketry3)，技能:Crafting≥14
- **HSK** `BuildMediumMissileSystem_Crate`：武器部件(Weapon_Parts)×10 + 发射器部件(Launcher_Component)×2 + [Metallic]×35 -> 130mm中型导弹系统组件箱(MediumMissileSystem_Crate)×1 @ HeavyArmsBench ｜ 工作量=18500 ｜ 研究:Rockets_D1，技能:Crafting≥14
- **对比**：数值一致

#### 制作7.62x51mm M134速射机枪组件箱（`MMT_BuildMinigunTurret_Crate`）

- **MMT**：武器部件×15 + 重型武器部件×1 + [MMT_MetallicBar]×45 -> M134速射机枪组件箱×1 @ — ｜ 工作量=10500 ｜ 研究:重型炮塔 II(MMT_HeavyTurrets2)，技能:Crafting≥8
- **HSK** `BuildMinigunTurret_Crate`：武器部件(Weapon_Parts)×15 + 重型武器部件(Heavy_Component)×1 + [Metallic]×45 -> M134速射机枪组件箱(MinigunTurret_Crate)×1 @ HeavyArmsBench ｜ 工作量=10500 ｜ 研究:Heavy_turrets_C2，技能:Crafting≥8
- **对比**：数值一致

#### 制作30mm哨戒炮塔组件箱（`MMT_BuildNavalGunTurret_Crate`）

- **MMT**：武器部件×18 + 火炮武器部件×1 + [MMT_MetallicBar]×35 + 集成电路×5 -> 35mm哨戒炮塔组件箱×1 @ — ｜ 工作量=14500 ｜ 研究:自动炮塔 III(MMT_Autoturrets3)，技能:Crafting≥13
- **HSK** `BuildNavalGunTurret_Crate`：武器部件(Weapon_Parts)×18 + 火炮武器部件(Cannon_Component)×1 + [Metallic]×35 + 集成电路(Electronics)×5 -> 35mm哨戒炮塔组件箱(NavalGunTurret_Crate)×1 @ HeavyArmsBench ｜ 工作量=14500 ｜ 研究:Autoturrets_D3，技能:Crafting≥13
- **对比**：数值一致

#### 制作20mm厄利孔机炮组件箱（`MMT_BuildOerlikonautocannon_Crate`）

- **MMT**：武器部件×15 + 火炮武器部件×1 + [MMT_MetallicBar]×45 + 合成橡胶×15 -> 20x102mm厄利孔机炮组件箱×1 @ — ｜ 工作量=13500 ｜ 研究:重型炮塔 III(MMT_HeavyTurrets3)，技能:Crafting≥12
- **HSK** `BuildOerlikonautocannon_Crate`：武器部件(Weapon_Parts)×15 + 火炮武器部件(Cannon_Component)×1 + [Metallic]×45 + 合成橡胶(Rubber)×15 -> 20x102mm厄利孔机炮组件箱(Oerlikonautocannon_Crate)×1 @ HeavyArmsBench ｜ 工作量=13500 ｜ 研究:Heavy_turrets_C3，技能:Crafting≥12
- **对比**：数值一致

#### 制作130mm重型导弹系统组件箱（`MMT_BuildRocketLauncher_Crate`）

- **MMT**：武器部件×10 + 发射器部件×3 + [MMT_MetallicBar]×35 -> 130mm重型导弹系统组件箱×1 @ — ｜ 工作量=19500 ｜ 研究:火箭弹 III(MMT_Rocketry3)，技能:Crafting≥15
- **HSK** `BuildRocketLauncher_Crate`：武器部件(Weapon_Parts)×10 + 发射器部件(Launcher_Component)×3 + [Metallic]×35 -> 130mm重型导弹系统组件箱(RocketLauncher_Crate)×1 @ HeavyArmsBench ｜ 工作量=19500 ｜ 研究:Rockets_D2，技能:Crafting≥15
- **对比**：数值一致

#### 制作反坦克哨戒火箭发射系统组件箱（`MMT_BuildSentryRocketLauncher_Crate`）

- **MMT**：武器部件×15 + 发射器部件×1 + [MMT_MetallicBar]×35 + 集成电路×5 -> 自动反坦克火箭发射系统组件箱×1 @ — ｜ 工作量=16500 ｜ 研究:自动炮塔 III(MMT_Autoturrets3)，技能:Crafting≥15
- **HSK** `BuildSentryRocketLauncher_Crate`：武器部件(Weapon_Parts)×15 + 发射器部件(Launcher_Component)×1 + [Metallic]×35 + 集成电路(Electronics)×5 -> 自动反坦克火箭发射系统组件箱(SentryRocketLauncher_Crate)×1 @ HeavyArmsBench ｜ 工作量=16500 ｜ 研究:Autoturrets_D3，技能:Crafting≥15
- **对比**：数值一致

#### 制作BGM-71“陶式”反坦克导弹组件箱（`MMT_BuildTOW_Crate`）

- **MMT**：武器部件×15 + 发射器部件×1 + [MMT_MetallicBar]×30 + 合成橡胶×15 -> BGM-71“陶式”反坦克导弹组件箱×1 @ — ｜ 工作量=16500 ｜ 研究:火箭弹 III(MMT_Rocketry3)，技能:Crafting≥13
- **HSK** `BuildTOW_Crate`：武器部件(Weapon_Parts)×15 + 发射器部件(Launcher_Component)×1 + [Metallic]×30 + 合成橡胶(Rubber)×15 -> BGM-71“陶式”反坦克导弹组件箱(TOW_Crate)×1 @ HeavyArmsBench ｜ 工作量=16500 ｜ 研究:Rockets_D1，技能:Crafting≥13
- **对比**：数值一致

#### 制作20mm火神炮组件箱（`MMT_BuildVulcanCannon_Crate`）

- **MMT**：武器部件×20 + 火炮武器部件×1 + [MMT_MetallicBar]×60 + 合成橡胶×25 -> 20mm火神炮组件箱×1 @ — ｜ 工作量=17500 ｜ 研究:重型炮塔 III(MMT_HeavyTurrets3)，技能:Crafting≥16
- **HSK** `BuildVulcanCannon_Crate`：武器部件(Weapon_Parts)×20 + 火炮武器部件(Cannon_Component)×1 + [Metallic]×60 + 合成橡胶(Rubber)×25 -> 20mm火神炮组件箱(VulcanCannon_Crate)×1 @ HeavyArmsBench ｜ 工作量=17500 ｜ 研究:Heavy_turrets_C3，技能:Crafting≥16
- **对比**：数值一致

### 3.25 WeaponParts（WeaponParts）

#### 制造武器部件（`MMT_MakeWeaponParts`）

- **MMT**：[MMT_MetallicBar]×25 + 工业零件(ComponentIndustrial)×10 + 机械组件×5 -> 武器部件×5 @ 武器加工台, 先进武器加工台, 未来武器工作台 ｜ 工作量=1500 ｜ 研究:Machining(Machining)，技能:Crafting≥6
- **HSK** `MakeWeapon_Parts`：[Metallic]×25 + 工业零件(ComponentIndustrial)×10 + 机械组件(Mechanism)×5 -> 武器部件(Weapon_Parts)×5 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 未来武器工作台(MechWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1500 ｜ 研究:Machining，技能:Crafting≥6
- **对比**：数值一致

#### 制造手枪部件（`MMT_MakePistolComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×6 + [MMT_HCMBar]×3 + 工业零件(ComponentIndustrial)×2 + 电线×1 + 机械组件×1 -> 手枪部件×1 @ 武器加工台, 先进武器加工台 ｜ 工作量=1200 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥6
- **HSK** `MakePistol_Component`：[SLDBar] 或 [USLDBar]×6 + [HCMBar]×3 + 工业零件(ComponentIndustrial)×2 + 电线(Wire)×1 + 机械组件(Mechanism)×1 -> 手枪部件(Pistol_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1200 ｜ 研究:Gunsmithing，技能:Crafting≥6
- **对比**：数值一致

#### 制造冲锋枪部件（`MMT_MakeSMGComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×10 + [MMT_HCMBar]×5 + 工业零件(ComponentIndustrial)×4 + 电线×6 + 机械组件×2 -> 冲锋枪部件×1 @ 武器加工台, 先进武器加工台 ｜ 工作量=1800 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥9
- **HSK** `MakeSMG_Component`：[SLDBar] 或 [USLDBar]×10 + [HCMBar]×5 + 工业零件(ComponentIndustrial)×4 + 电线(Wire)×6 + 机械组件(Mechanism)×2 -> 冲锋枪部件(SMG_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1800 ｜ 研究:BlowbackOperation，技能:Crafting≥9
- **对比**：数值一致

#### 制造霰弹枪部件（`MMT_MakeShotgunComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×15 + [MMT_HCMBar]×8 + 工业零件(ComponentIndustrial)×4 + 电线×5 + 机械组件×2 -> 霰弹枪部件×1 @ 武器加工台, 先进武器加工台 ｜ 工作量=1800 ｜ 研究:GasOperation(GasOperation)，技能:Crafting≥7
- **HSK** `MakeShotgun_Component`：[SLDBar] 或 [USLDBar]×15 + [HCMBar]×8 + 工业零件(ComponentIndustrial)×4 + 电线(Wire)×5 + 机械组件(Mechanism)×2 -> 霰弹枪部件(Shotgun_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1800 ｜ 研究:GasOperation，技能:Crafting≥7
- **对比**：数值一致

#### 制造步枪部件（`MMT_MakeRifleComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×20 + [MMT_HCMBar]×10 + 工业零件(ComponentIndustrial)×6 + 电线×8 + 机械组件×3 -> 步枪部件×1 @ 武器加工台, 先进武器加工台 ｜ 工作量=2100 ｜ 研究:GasOperation(GasOperation)，技能:Crafting≥8
- **HSK** `MakeRifle_Component`：[SLDBar] 或 [USLDBar]×20 + [HCMBar]×10 + 工业零件(ComponentIndustrial)×6 + 电线(Wire)×8 + 机械组件(Mechanism)×3 -> 步枪部件(Rifle_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:Rifles_C1，技能:Crafting≥8
- **对比**：数值一致

#### 制造狙击枪部件（`MMT_MakeSniperComponent`）

- **MMT**：步枪部件×1 + 工业零件(ComponentIndustrial)×6 + 玻璃块×12 -> 狙击枪部件×1 @ 武器加工台, 先进武器加工台 ｜ 工作量=2100 ｜ 研究:PrecisionRifling(PrecisionRifling)，技能:Crafting≥10
- **HSK** `MakeSniper_Component`：步枪部件(Rifle_Component)×1 + 工业零件(ComponentIndustrial)×6 + 玻璃块(Glass)×12 -> 狙击枪部件(Sniper_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:Sniper_rifles_C1，技能:Crafting≥10
- **对比**：数值一致

#### 制造先进步枪部件（`MMT_MakeAdvRifleComponent`）

- **MMT**：人造纤维×5 + 塑料×15 + 合成橡胶×15 + 步枪部件×1 + 先进机械组件×2 -> 先进步枪部件×1 @ 先进武器加工台 ｜ 工作量=2100 ｜ 研究:Fabrication(Fabrication)，技能:Crafting≥13
- **HSK** `MakeAdvRifle_Component`：人造纤维(SyntheticFibers)×5 + 塑料(Plastic)×15 + 合成橡胶(Rubber)×15 + 步枪部件(Rifle_Component)×1 + 先进机械组件(AdvMechanism)×2 -> 先进步枪部件(AdvRifle_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:Rifles_C2，技能:Crafting≥13
- **对比**：数值一致

#### 制造先进狙击枪部件（`MMT_MakeAdvSniperComponent`）

- **MMT**：人造纤维×10 + 塑料×20 + 合成橡胶×15 + 集成电路×4 + 狙击枪部件×2 + 先进机械组件×3 -> 先进狙击枪部件×1 @ 先进武器加工台 ｜ 工作量=2100 ｜ 研究:Fabrication(Fabrication)，技能:Crafting≥15
- **HSK** `MakeAdvSniper_Component`：人造纤维(SyntheticFibers)×10 + 塑料(Plastic)×20 + 合成橡胶(Rubber)×15 + 集成电路(Electronics)×4 + 狙击枪部件(Sniper_Component)×2 + 先进机械组件(AdvMechanism)×3 -> 先进狙击枪部件(AdvSniper_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:Sniper_rifles_D1，技能:Crafting≥15
- **对比**：数值一致

#### 制造重型武器部件（`MMT_MakeHeavyComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×25 + [MMT_HCMBar]×15 + 工业零件(ComponentIndustrial)×12 + 电线×15 + 机械组件×6 -> 重型武器部件×1 @ 先进武器加工台 ｜ 工作量=2800 ｜ 研究:HeavyTurrets(HeavyTurrets)，技能:Crafting≥12
- **HSK** `MakeHeavy_Component`：[SLDBar] 或 [USLDBar]×25 + [HCMBar]×15 + 工业零件(ComponentIndustrial)×12 + 电线(Wire)×15 + 机械组件(Mechanism)×6 -> 重型武器部件(Heavy_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), HeavyArmsBench ｜ 工作量=2800 ｜ 研究:HeavyTurrets，技能:Crafting≥12
- **对比**：数值一致

#### 制造火炮武器部件（`MMT_MakeCannonComponent`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×20 + 重型武器部件×1 + 先进机械组件×5 -> 火炮武器部件×1 @ 先进武器加工台 ｜ 工作量=3100 ｜ 研究:Mortars(Mortars)，技能:Crafting≥14
- **HSK** `MakeCannon_Component`：[SLDBar] 或 [USLDBar]×20 + 重型武器部件(Heavy_Component)×1 + 先进机械组件(AdvMechanism)×5 -> 火炮武器部件(Cannon_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), HeavyArmsBench ｜ 工作量=3100 ｜ 研究:Turrets_31，技能:Crafting≥14
- **对比**：数值一致

#### 制造发射器部件（`MMT_MakeLauncherComponent`）

- **MMT**：集成电路×2 + 重型武器部件×1 + 先进机械组件×3 + 玻璃块×12 -> 发射器部件×1 @ 先进武器加工台 ｜ 工作量=3100 ｜ 研究:火箭弹 I(MMT_Rocketry1)，技能:Crafting≥14
- **HSK** `MakeLauncher_Component`：集成电路(Electronics)×2 + 重型武器部件(Heavy_Component)×1 + 先进机械组件(AdvMechanism)×3 + 玻璃块(Glass)×12 -> 发射器部件(Launcher_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), HeavyArmsBench ｜ 工作量=3100 ｜ 研究:Rockets_C1，技能:Crafting≥14
- **对比**：数值一致

#### 制造电荷武器部件（`MMT_MakeChargedComponent`）

- **MMT**：[MMT_USLDBar]×10 + [MMT_HCMBar]×20 + 磁性材料×2 + 先进机械组件×2 -> 电荷武器部件×1 @ 未来武器工作台 ｜ 工作量=2100 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥14
- **HSK** `MakeCharged_Component`：[USLDBar]×10 + [HCMBar]×20 + 磁性材料(MagneticMaterial)×2 + 先进机械组件(AdvMechanism)×2 -> 电荷武器部件(Charged_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:ChargedShot，技能:Crafting≥14
- **对比**：数值一致

#### 制造光束武器部件（`MMT_MakeLaserComponent`）

- **MMT**：[MMT_USLDBar]×20 + [MMT_HCMBar]×10 + 磁性材料×4 + 微芯片集成电路×1 -> 光束武器部件×1 @ 未来武器工作台 ｜ 工作量=2100 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥15
- **HSK** `MakeLaser_Component`：[USLDBar]×20 + [HCMBar]×10 + 磁性材料(MagneticMaterial)×4 + 微芯片集成电路(Microchips)×1 -> 光束武器部件(Laser_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:ChargedShot，技能:Crafting≥15
- **对比**：数值一致

#### 制造离子武器部件（`MMT_MakePlasmaComponent`）

- **MMT**：[MMT_USLDBar]×20 + [MMT_HCMBar]×10 + 生物合成材料×4 + 微芯片集成电路×1 -> 离子武器部件×1 @ 未来武器工作台 ｜ 工作量=2100 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥15
- **HSK** `MakePlasma_Component`：[USLDBar]×20 + [HCMBar]×10 + 生物合成材料(BiosyntheticMaterial)×4 + 微芯片集成电路(Microchips)×1 -> 离子武器部件(Plasma_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:ChargedShot，技能:Crafting≥15
- **对比**：数值一致

### 3.26 Weapons_Archaic（Weapons_Archaic）

#### 制作EMP手榴弹（`MMT_MakeEMPGrenades`）

- **MMT**：[MMT_MetallicBar]×36 + 工业零件(ComponentIndustrial)×7 + 电子元件×4 -> Weapon_GrenadeEMP×5 @ — ｜ 工作量=6000 ｜ 研究:手榴弹 II(MMT_GrenadesAdvanced)，技能:Crafting≥11
- **HSK**：无对应资源（MMT 新增）

#### 制作泡沫手榴弹（`MMT_MakeFirefoamGrenade`）

- **MMT**：[MMT_MetallicBar]×30 + 表面活性剂×15 + 火药 或 Chemfuel×3 + 工业零件(ComponentIndustrial)×4 -> firefoam grenade×5 @ — ｜ 工作量=4000 ｜ 研究:手榴弹 I(MMT_GrenadesBasic)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制作震撼手榴弹（`MMT_MakeFlashbangGrenades`）

- **MMT**：[MMT_MetallicBar]×25 + 火药×6 + 工业零件(ComponentIndustrial)×4 -> flashbang grenade×5 @ — ｜ 工作量=6000 ｜ 研究:手榴弹 I(MMT_GrenadesBasic)，技能:Crafting≥9
- **HSK**：无对应资源（MMT 新增）

#### 制作破片手榴弹（`MMT_MakeFragGrenades`）

- **MMT**：[MMT_MetallicBar]×30 + 火药 或 Chemfuel×7 + 工业零件(ComponentIndustrial)×4 -> Weapon_GrenadeFrag×5 @ — ｜ 工作量=4000 ｜ 研究:手榴弹 II(MMT_GrenadesAdvanced)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制作温压手榴弹（`MMT_MakeFusionGrenades`）

- **MMT**：[MMT_MetallicBar]×15 + 武器部件×3 + 高级零部件×3 + 光束武器部件×1 -> 温压手榴弹×5 @ — ｜ 工作量=7000 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作电荷手榴弹（`MMT_MakeGrenadeChargedFrag`）

- **MMT**：[MMT_MetallicBar]×15 + 武器部件×3 + 高级零部件×3 + 电荷武器部件×1 -> 电荷手榴弹×5 @ — ｜ 工作量=7000 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作高爆手榴弹（`MMT_MakeHighExplosiveGrenades`）

- **MMT**：[MMT_MetallicBar]×24 + 火药 或 Chemfuel×5 + 工业零件(ComponentIndustrial)×3 -> 高爆手榴弹×5 @ — ｜ 工作量=3600 ｜ 研究:手榴弹 I(MMT_GrenadesBasic)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作燃烧手榴弹（`MMT_MakeIncendiaryGrenade`）

- **MMT**：工业零件(ComponentIndustrial)×4 + 凝固汽油×5 + [MMT_MetallicBar]×30 -> 燃烧手榴弹×5 @ — ｜ 工作量=3500 ｜ 研究:手榴弹 II(MMT_GrenadesAdvanced)，技能:Crafting≥9
- **HSK**：无对应资源（MMT 新增）

#### 制作莫洛托夫燃烧瓶（`MMT_MakeMolotov`）

- **MMT**：玻璃块×30 + Chemfuel×30 + [MMT_BTextiles]×30 -> Weapon_GrenadeMolotov×5 @ — ｜ 工作量=3500 ｜ 研究:Greatbow(Greatbow)，技能:Crafting≥5
- **HSK**：无对应资源（MMT 新增）

#### 制作等离子手榴弹（`MMT_MakePlasmaGrenades`）

- **MMT**：[MMT_MetallicBar]×15 + 武器部件×3 + 高级零部件×3 + 离子武器部件×1 -> 等离子手榴弹×5 @ — ｜ 工作量=7000 ｜ 研究:ChargedShot(ChargedShot)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作烟雾手榴弹（`MMT_MakeSmokeGrenade`）

- **MMT**：[MMT_MetallicBar]×36 + 塑料×10 + 工业零件(ComponentIndustrial)×5 -> smoke grenade×5 @ — ｜ 工作量=6000 ｜ 研究:手榴弹 II(MMT_GrenadesAdvanced)，技能:Crafting≥9
- **HSK**：无对应资源（MMT 新增）

#### 制作木柄炸弹（`MMT_MakeStickBomb`）

- **MMT**：原始零部件×15 + Chemfuel×3 + [MMT_BTextiles]×30 -> stick bomb×5 @ — ｜ 工作量=3000 ｜ 研究:RecurveBow(RecurveBow)，技能:Crafting≥3
- **HSK**：无对应资源（MMT 新增）

#### 制作毒气手榴弹 x10（`MMT_MakeToxGrenade`）

- **MMT**：[MMT_SLDBar] 或 [MMT_USLDBar]×12 + [MMT_HCMBar]×4 + Chemfuel×15 + 火药 或 Chemfuel×3 + 工业零件(ComponentIndustrial)×2 -> 毒气手雷×5 @ — ｜ 工作量=3200 ｜ 研究:ToxGas(ToxGas)
- **HSK**：无对应资源（MMT 新增）

### 3.27 Weapons_Conventional（Weapons_Conventional）

#### 制作伯莱塔93R冲锋手枪（`MMT_Build93RBurstPistol`）

- **MMT**：手枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×12 + 塑料×12 + [Leathers]×5 -> 伯莱塔93R冲锋手枪×1 @ — ｜ 工作量=9300 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制造 AA-12自动霰弹枪（`MMT_BuildAA12AutoShotgun`）

- **MMT**：霰弹枪部件×1 + 武器部件×8 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×22 + 塑料×20 + [Leathers]×9 -> AA-12自动霰弹枪×1 @ — ｜ 工作量=19000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作AK-47突击步枪（`MMT_BuildAK47Gun`）

- **MMT**：步枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×8 + [MMT_MetallicBar]×26 + 塑料×15 + [Leathers]×11 -> AK-47突击步枪×1 @ — ｜ 工作量=19000 ｜ 研究:步枪 II(MMT_RiflesAdvanced)，技能:Crafting≥12
- **HSK**：无对应资源（MMT 新增）

#### 制造 AR-50狙击步枪（`MMT_BuildARFifty`）

- **MMT**：先进狙击枪部件×1 + 武器部件×8 + 工业零件(ComponentIndustrial)×12 + [MMT_MetallicBar]×36 + 塑料×35 + [Leathers]×12 -> AR-50狙击步枪×1 @ — ｜ 工作量=25000 ｜ 研究:狙击步枪 III(MMT_SnipersAdvanced)，技能:Crafting≥18
- **HSK**：无对应资源（MMT 新增）

#### 制造 PTRD反坦克步枪（`MMT_BuildATR`）

- **MMT**：狙击枪部件×1 + 武器部件×9 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×25 + 塑料×20 -> PTRD反坦克步枪×1 @ — ｜ 工作量=21000 ｜ 研究:狙击步枪 III(MMT_SnipersAdvanced)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作斯太尔AUG突击步枪（`MMT_BuildAUG`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×16 + 塑料×20 + [Leathers]×10 -> 斯太尔AUG突击步枪×1 @ — ｜ 工作量=19500 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作M16突击步枪（`MMT_BuildAssaultRifle`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×16 + 塑料×15 + [Leathers]×10 -> Gun_AssaultRifle×1 @ — ｜ 工作量=18000 ｜ 研究:步枪 II(MMT_RiflesAdvanced)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作伯莱塔Cx4卡宾枪（`MMT_BuildCXfour`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + 塑料×10 + [Leathers]×6 -> 伯莱塔Cx4卡宾枪×1 @ — ｜ 工作量=14000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥8
- **HSK**：无对应资源（MMT 新增）

#### 制作M1917史密斯威森左轮（`MMT_BuildDeringerPistol`）

- **MMT**：手枪部件×1 + 武器部件×2 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + [Leathers]×5 -> M1917史密斯威森左轮×1 @ — ｜ 工作量=8500 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥3
- **HSK**：无对应资源（MMT 新增）

#### 制作法玛斯G2突击步枪（`MMT_BuildFAMAS`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×16 + 塑料×15 + [Leathers]×10 -> 法玛斯G2突击步枪×1 @ — ｜ 工作量=17500 ｜ 研究:步枪 II(MMT_RiflesAdvanced)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作灭火器（`MMT_BuildFireExtinguisher`）

- **MMT**：武器部件×3 + 工业零件(ComponentIndustrial)×4 + 表面活性剂×30 + [MMT_MetallicBar]×20 -> 灭火器×1 @ — ｜ 工作量=8800 ｜ 技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作RM2喷火器（`MMT_BuildFlamethrowerRM`）

- **MMT**：重型武器部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×20 + 塑料×16 -> RM2喷火器×1 @ — ｜ 工作量=17000 ｜ 研究:HeavyTurrets(HeavyTurrets)，技能:Crafting≥12
- **HSK**：无对应资源（MMT 新增）

#### 制作信号枪（`MMT_BuildFlareGun`）

- **MMT**：武器部件×3 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×20 -> 信号枪×1 @ — ｜ 工作量=8800 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作加利尔ACE轻机枪（`MMT_BuildGalilBurstRifle`）

- **MMT**：重型武器部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×22 + 塑料×20 + [Leathers]×8 -> 加利尔ACE轻机枪×1 @ — ｜ 工作量=20500 ｜ 研究:MultibarrelWeapons(MultibarrelWeapons)，技能:Crafting≥8
- **HSK**：无对应资源（MMT 新增）

#### 制作李-恩菲尔德步枪（`MMT_BuildGun_BoltActionRifle`）

- **MMT**：步枪部件×1 + 武器部件×1 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×10 -> Gun_BoltActionRifle×1 @ — ｜ 工作量=12200 ｜ 研究:GasOperation(GasOperation)，技能:Crafting≥3
- **HSK**：无对应资源（MMT 新增）

#### 制作柯尔特M1911自动手枪（`MMT_BuildHandGun`）

- **MMT**：手枪部件×1 + 武器部件×1 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×10 + [Leathers]×5 -> Gun_Revolver×1 @ — ｜ 工作量=8500 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥3
- **HSK**：无对应资源（MMT 新增）

#### 制作MA39X突击步枪（`MMT_BuildHeavyCombatRifle`）

- **MMT**：重型武器部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×8 + [MMT_MetallicBar]×26 + 塑料×20 + [Leathers]×12 -> MA39X突击步枪×1 @ — ｜ 工作量=20500 ｜ 研究:MultibarrelWeapons(MultibarrelWeapons)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制造 M4幽灵冲锋枪（`MMT_BuildHeavySMG`）

- **MMT**：冲锋枪部件×1 + 武器部件×2 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + 塑料×15 + [Leathers]×12 -> Gun_HeavySMG×1 @ — ｜ 工作量=15000 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制造 黑卡蒂II反器材狙击步枪（`MMT_BuildHecateII`）

- **MMT**：先进狙击枪部件×1 + 武器部件×12 + 工业零件(ComponentIndustrial)×16 + [MMT_MetallicBar]×55 + 塑料×35 + [Leathers]×12 -> 黑卡蒂II反器材狙击步枪×1 @ — ｜ 工作量=25000 ｜ 研究:狙击步枪 III(MMT_SnipersAdvanced)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制作T9“炎阳”燃烧弹发射器（`MMT_BuildIncendiaryLauncher`）

- **MMT**：重型武器部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×20 + 塑料×16 + [Leathers]×12 -> Gun_IncendiaryLauncher×1 @ — ｜ 工作量=19000 ｜ 研究:发射器 I(MMT_GrenadeLaunchers)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作M72反坦克火箭筒（`MMT_BuildLAW`）

- **MMT**：发射器部件×2 + 武器部件×5 + 工业零件(ComponentIndustrial)×8 + [MMT_MetallicBar]×30 + 塑料×30 -> M72R反坦克火箭筒×1 @ — ｜ 工作量=22000 ｜ 研究:火箭弹 I(MMT_Rocketry1)，技能:Crafting≥16
- **HSK**：无对应资源（MMT 新增）

#### 制作L-15轻机枪（`MMT_BuildLMG`）

- **MMT**：重型武器部件×1 + 武器部件×5 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×15 -> Gun_LMG×1 @ — ｜ 工作量=19500 ｜ 研究:HeavyTurrets(HeavyTurrets)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作M249班用机枪（`MMT_BuildM249`）

- **MMT**：重型武器部件×1 + 武器部件×8 + 工业零件(ComponentIndustrial)×9 + [MMT_MetallicBar]×30 + 塑料×25 -> M249班用机枪×1 @ — ｜ 工作量=23000 ｜ 研究:HeavyTurrets(HeavyTurrets)，技能:Crafting≥12
- **HSK**：无对应资源（MMT 新增）

#### 制造 M37A2泵动霰弹枪（`MMT_BuildM37A2_USCM`）

- **MMT**：霰弹枪部件×1 + 武器部件×2 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×7 -> M37A2泵动霰弹枪×1 @ — ｜ 工作量=11800 ｜ 研究:GasOperation(GasOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制造 M3“注油枪”冲锋枪（`MMT_BuildM3SMG`）

- **MMT**：冲锋枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×16 + [Leathers]×6 -> M3“注油枪”冲锋枪×1 @ — ｜ 工作量=13600 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥5
- **HSK**：无对应资源（MMT 新增）

#### 制造 M42A狙击步枪（`MMT_BuildM42A_USCM`）

- **MMT**：狙击枪部件×1 + 武器部件×9 + 工业零件(ComponentIndustrial)×11 + [MMT_MetallicBar]×20 + 塑料×26 + [Leathers]×7 -> M42A狙击步枪×1 @ — ｜ 工作量=16900 ｜ 研究:PrecisionRifling(PrecisionRifling)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作M4A1卡宾枪（`MMT_BuildM4A1Gun`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×15 + 塑料×10 + [Leathers]×11 -> M4A1卡宾枪×1 @ — ｜ 工作量=15000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制作M56智能机枪（`MMT_BuildM56_USCM`）

- **MMT**：重型武器部件×1 + 武器部件×7 + 高级零部件×4 + [MMT_MetallicBar]×20 + 塑料×20 + [Leathers]×12 -> M56智能机枪×1 @ — ｜ 工作量=19900 ｜ 研究:MultibarrelWeapons(MultibarrelWeapons)，技能:Crafting≥16
- **HSK**：无对应资源（MMT 新增）

#### 制作M5火箭筒（`MMT_BuildM5_USCM`）

- **MMT**：发射器部件×2 + 武器部件×6 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×26 + 塑料×25 -> M5火箭筒×1 @ — ｜ 工作量=23000 ｜ 研究:火箭弹 I(MMT_Rocketry1)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作M79榴弹发射器（`MMT_BuildM79GL`）

- **MMT**：步枪部件×2 + 武器部件×3 + 工业零件(ComponentIndustrial)×3 + [MMT_MetallicBar]×20 + [MMT_Woody]×20 + [Leathers]×10 -> M79榴弹发射器×1 @ — ｜ 工作量=15000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥9
- **HSK**：无对应资源（MMT 新增）

#### 制造 M82A1反器材步枪（`MMT_BuildM82Gun`）

- **MMT**：先进狙击枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×15 + [MMT_MetallicBar]×26 + 塑料×25 + [Leathers]×12 -> M82A1反器材步枪×1 @ — ｜ 工作量=22500 ｜ 研究:狙击步枪 III(MMT_SnipersAdvanced)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制造 MP5A3冲锋枪（`MMT_BuildMP5Gun`）

- **MMT**：冲锋枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + 塑料×15 + [Leathers]×6 -> MP5A3冲锋枪×1 @ — ｜ 工作量=14600 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制作TAR-21突击步枪（`MMT_BuildMarineSturmRifle`）

- **MMT**：步枪部件×1 + 武器部件×5 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×22 + 塑料×20 -> TAR-21突击步枪×1 @ — ｜ 工作量=21000 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥12
- **HSK**：无对应资源（MMT 新增）

#### 制作米尔科姆榴弹发射器（`MMT_BuildMilkorGrenadeLauncher`）

- **MMT**：重型武器部件×1 + 武器部件×7 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×26 + 塑料×25 + [Leathers]×10 -> 米尔科姆榴弹发射器×1 @ — ｜ 工作量=21000 ｜ 研究:发射器 I(MMT_GrenadeLaunchers)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作XM214速射机枪（`MMT_BuildMinigun`）

- **MMT**：重型武器部件×1 + 武器部件×13 + 高级零部件×10 + [MMT_MetallicBar]×36 + 塑料×25 + [Leathers]×10 -> Gun_Minigun×1 @ — ｜ 工作量=28000 ｜ 研究:MultibarrelWeapons(MultibarrelWeapons)，技能:Crafting≥18
- **HSK**：无对应资源（MMT 新增）

#### 制造 乌兹冲锋枪（`MMT_BuildPDW`）

- **MMT**：冲锋枪部件×1 + 武器部件×1 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×10 -> Gun_MachinePistol×1 @ — ｜ 工作量=13600 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作“帕拉军工”P14 手枪（`MMT_BuildParaOrdnancePFourteenFortyfive`）

- **MMT**：手枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×3 + [MMT_MetallicBar]×12 + 塑料×12 + [Leathers]×10 -> Gun_Autopistol×1 @ — ｜ 工作量=8800 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制造 雷明顿870霰弹枪（`MMT_BuildPumpShotgun`）

- **MMT**：霰弹枪部件×1 + 武器部件×1 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×7 -> Gun_PumpShotgun×1 @ — ｜ 工作量=12000 ｜ 研究:GasOperation(GasOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作RPG-7火箭筒（`MMT_BuildRPG`）

- **MMT**：发射器部件×1 + 武器部件×7 + 工业零件(ComponentIndustrial)×7 + [MMT_MetallicBar]×26 + 塑料×25 -> RPG-7火箭筒×1 @ — ｜ 工作量=23000 ｜ 研究:火箭弹 I(MMT_Rocketry1)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作FN SCAR-H突击步枪（`MMT_BuildSCAR_H`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×16 + 塑料×20 + [Leathers]×10 -> FN SCAR-H突击步枪×1 @ — ｜ 工作量=19200 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作西格SG553突击步枪（`MMT_BuildSG553Gun`）

- **MMT**：步枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×16 + 塑料×20 + [Leathers]×10 -> 西格SG553突击步枪×1 @ — ｜ 工作量=19500 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制造 MP5K冲锋枪（`MMT_BuildSkorpionSMG`）

- **MMT**：冲锋枪部件×1 + 武器部件×3 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×6 -> MP5K冲锋枪×1 @ — ｜ 工作量=15600 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作M24狙击步枪（`MMT_BuildSniperRifle`）

- **MMT**：狙击枪部件×1 + 武器部件×9 + 工业零件(ComponentIndustrial)×9 + [MMT_MetallicBar]×15 + 塑料×22 + [Leathers]×7 -> Gun_SniperRifle×1 @ — ｜ 工作量=16600 ｜ 研究:PrecisionRifling(PrecisionRifling)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制作司芬克斯AT2000手枪（`MMT_BuildSphinxPistol`）

- **MMT**：手枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×3 + [MMT_MetallicBar]×12 + 塑料×10 + [Leathers]×5 -> 司芬克斯2000手枪×1 @ — ｜ 工作量=8800 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥9
- **HSK**：无对应资源（MMT 新增）

#### 制造 “打击者”霰弹枪（`MMT_BuildStrikerShotgun`）

- **MMT**：霰弹枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + 塑料×15 + [Leathers]×8 -> Gun_ChainShotgun×1 @ — ｜ 工作量=16200 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥10
- **HSK**：无对应资源（MMT 新增）

#### 制造 SRS99反器材狙击步枪（`MMT_BuildTAMSR`）

- **MMT**：先进狙击枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×15 + [MMT_MetallicBar]×26 + 塑料×25 + [Leathers]×12 -> SRS99反器材狙击步枪×1 @ — ｜ 工作量=23000 ｜ 研究:狙击步枪 III(MMT_SnipersAdvanced)，技能:Crafting≥18
- **HSK**：无对应资源（MMT 新增）

#### 制造 TMP消声冲锋枪（`MMT_BuildTMPMachinePistol`）

- **MMT**：冲锋枪部件×1 + 武器部件×2 + 工业零件(ComponentIndustrial)×6 + [MMT_MetallicBar]×20 + [Leathers]×7 -> TMP消声冲锋枪×1 @ — ｜ 工作量=15000 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制造 M90近程突击霰弹枪（`MMT_BuildTacticalAutoshotgun`）

- **MMT**：霰弹枪部件×1 + 武器部件×9 + 工业零件(ComponentIndustrial)×8 + [MMT_MetallicBar]×28 + 塑料×20 + [Leathers]×9 -> M90近程突击霰弹枪×1 @ — ｜ 工作量=19000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作MA5 ICWS突击步枪（`MMT_BuildTacticalCombatrifle`）

- **MMT**：步枪部件×1 + 武器部件×7 + 工业零件(ComponentIndustrial)×9 + [MMT_MetallicBar]×26 + 塑料×20 + [Leathers]×10 -> MA5 ICWS突击步枪×1 @ — ｜ 工作量=20000 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

#### 制造 M45战术霰弹枪（`MMT_BuildTacticalShotgun`）

- **MMT**：霰弹枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×20 + 塑料×15 + [Leathers]×8 -> M45战术霰弹枪×1 @ — ｜ 工作量=17500 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥12
- **HSK**：无对应资源（MMT 新增）

#### 制作TEC-9冲锋手枪（`MMT_BuildTecnine`）

- **MMT**：手枪部件×1 + 武器部件×5 + 工业零件(ComponentIndustrial)×3 + [MMT_MetallicBar]×12 + 塑料×12 + [Leathers]×12 -> TEC-9冲锋手枪×1 @ — ｜ 工作量=9200 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制造 UMP45冲锋枪（`MMT_BuildUMP45Gun`）

- **MMT**：冲锋枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×16 + 塑料×15 + [Leathers]×6 -> UMP45冲锋枪×1 @ — ｜ 工作量=14900 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥6
- **HSK**：无对应资源（MMT 新增）

#### 制作USC半自动卡宾枪（`MMT_BuildUSC`）

- **MMT**：步枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×5 + [MMT_MetallicBar]×15 + 塑料×15 + [Leathers]×7 -> HK USC半自动卡宾枪×1 @ — ｜ 工作量=14000 ｜ 研究:枪械 II(MMT_GunsAdvanced)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制造 K10“短剑”冲锋枪（`MMT_BuildVector`）

- **MMT**：冲锋枪部件×1 + 武器部件×4 + 工业零件(ComponentIndustrial)×4 + [MMT_MetallicBar]×15 + 塑料×15 + [Leathers]×6 -> K10“短剑”冲锋枪×1 @ — ｜ 工作量=15600 ｜ 研究:BlowbackOperation(BlowbackOperation)，技能:Crafting≥7
- **HSK**：无对应资源（MMT 新增）

#### 制作维克多CP1手枪（`MMT_BuildVektorCP`）

- **MMT**：手枪部件×1 + 武器部件×2 + 工业零件(ComponentIndustrial)×4 + [MMT_SLDBar] 或 [MMT_USLDBar]×15 + [Leathers]×6 -> 维克多CP1手枪×1 @ — ｜ 工作量=8800 ｜ 研究:Gunsmithing(Gunsmithing)，技能:Crafting≥8
- **HSK**：无对应资源（MMT 新增）

#### 制造 瓦尔特WA2000狙击步枪（`MMT_BuildWA2000SpecialistRifle`）

- **MMT**：狙击枪部件×1 + 武器部件×12 + 工业零件(ComponentIndustrial)×12 + [MMT_MetallicBar]×25 + 塑料×20 + [Leathers]×11 -> 瓦尔特WA2000狙击步枪×1 @ — ｜ 工作量=21000 ｜ 研究:PrecisionRifling(PrecisionRifling)，技能:Crafting≥16
- **HSK**：无对应资源（MMT 新增）

#### 制作XM8军用步枪（`MMT_BuildXM8MilitaryRifle`）

- **MMT**：步枪部件×1 + 武器部件×6 + 工业零件(ComponentIndustrial)×8 + [MMT_MetallicBar]×22 + 塑料×20 + [Leathers]×11 -> XM8军用步枪×1 @ — ｜ 工作量=20000 ｜ 研究:步枪 III(MMT_RiflesMaster)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作M60E4通用机枪（`MMT_Build_M60GPMG`）

- **MMT**：重型武器部件×1 + 武器部件×8 + 工业零件(ComponentIndustrial)×10 + [MMT_MetallicBar]×35 + 塑料×30 -> M60E4通用机枪×1 @ — ｜ 工作量=24000 ｜ 研究:MultibarrelWeapons(MultibarrelWeapons)，技能:Crafting≥13
- **HSK**：无对应资源（MMT 新增）

### 3.28 Weapons_Futuristic（Weapons_Futuristic）

#### 制造A-1BW黑寡妇（`MMT_BuildBlackWidow`）

- **MMT**：光束武器部件×1 + 动力电池×1 + 武器部件×7 + 超级零部件×8 + [MMT_USLDBar]×20 + 塑料×35 -> A-1BW“黑寡妇”×1 @ — ｜ 工作量=27000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制作M12“仲裁者”光束枪（`MMT_BuildBurstLaserRifle`）

- **MMT**：光束武器部件×1 + 武器部件×9 + 超级零部件×9 + [MMT_USLDBar]×25 + 塑料×40 -> M12“仲裁者”光束枪×1 @ — ｜ 工作量=25000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制作R3“溶媒”电荷步枪（`MMT_BuildChargeRifle`）

- **MMT**：电荷武器部件×1 + 武器部件×5 + 高级零部件×10 + [MMT_USLDBar]×20 + 塑料×27 -> Gun_ChargeRifle×1 @ — ｜ 工作量=27000 ｜ 研究:激光武器 I(MMT_DirectedEnergyWeapons1)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作M10“铆钉”光束枪（`MMT_BuildHeavyLaserRifle`）

- **MMT**：光束武器部件×1 + 武器部件×8 + 超级零部件×9 + [MMT_USLDBar]×22 + 塑料×35 -> M10“铆钉”光束枪×1 @ — ｜ 工作量=24000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥16
- **HSK**：无对应资源（MMT 新增）

#### 制作M8“亡魂”光束枪（`MMT_BuildLaserRifle`）

- **MMT**：光束武器部件×1 + 武器部件×6 + 高级零部件×9 + [MMT_USLDBar]×20 + 塑料×35 -> M8“亡魂”光束枪×1 @ — ｜ 工作量=23000 ｜ 研究:激光武器 I(MMT_DirectedEnergyWeapons1)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作M6“螳螂”光束枪（`MMT_BuildMantis`）

- **MMT**：光束武器部件×1 + 武器部件×7 + 高级零部件×9 + [MMT_USLDBar]×20 + 塑料×35 -> Gun_ChargeLance×1 @ — ｜ 工作量=24000 ｜ 研究:激光武器 I(MMT_DirectedEnergyWeapons1)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作C3“巫师”（`MMT_BuildPlasmaCarbine`）

- **MMT**：离子武器部件×1 + 武器部件×6 + 高级零部件×10 + [MMT_USLDBar]×25 + 塑料×35 -> C3“巫师”×1 @ — ｜ 工作量=26000 ｜ 研究:激光武器 I(MMT_DirectedEnergyWeapons1)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

#### 制作C1“晋升”（`MMT_BuildPlasmaPistol`）

- **MMT**：离子武器部件×1 + 武器部件×4 + 高级零部件×8 + [MMT_USLDBar]×12 + 塑料×20 -> C1“晋升”×1 @ — ｜ 工作量=18000 ｜ 研究:激光武器 I(MMT_DirectedEnergyWeapons1)，技能:Crafting≥14
- **HSK**：无对应资源（MMT 新增）

#### 制作C2“月咒”（`MMT_BuildPlasmaPrecision`）

- **MMT**：离子武器部件×1 + 武器部件×6 + 超级零部件×10 + [MMT_USLDBar]×22 + 塑料×35 -> C2“月咒”×1 @ — ｜ 工作量=25000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥16
- **HSK**：无对应资源（MMT 新增）

#### 制作C7“灾引”（`MMT_BuildPlasmaRifle`）

- **MMT**：离子武器部件×1 + 武器部件×6 + 超级零部件×10 + [MMT_USLDBar]×22 + 塑料×35 -> C7“灾引”×1 @ — ｜ 工作量=27000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制作C5“魔咒”（`MMT_BuildPlasmaShotgun`）

- **MMT**：离子武器部件×1 + 武器部件×7 + 超级零部件×11 + [MMT_USLDBar]×22 + 塑料×35 -> C5“魔咒”×1 @ — ｜ 工作量=26000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥17
- **HSK**：无对应资源（MMT 新增）

#### 制作R6“撕裂者”电荷步枪（`MMT_BuildSplitter`）

- **MMT**：电荷武器部件×1 + 武器部件×6 + 超级零部件×12 + 斯特合金×7 + [MMT_USLDBar]×22 + 塑料×35 -> R6“撕裂者”电荷步枪×1 @ — ｜ 工作量=30000 ｜ 研究:激光武器 II(MMT_DirectedEnergyWeapons2)，技能:Crafting≥15
- **HSK**：无对应资源（MMT 新增）

### 3.29 木材（Wood）

#### 加工木板（`MMT_MakeWoodPlanks_Hand`）

- **MMT**：原木(WoodLog)×10 -> 木板×15 @ 手工锯台 ｜ 工作量=1200 ｜ 研究:木工加工(MMT_Woodworking)，技能:Crafting≥3
- **HSK** `MakeWoodPlanks_Hand`：原木(WoodLog)×10 -> 木板(WoodPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:Craft_0，技能:Crafting≥3
- **对比**：数值一致

#### 加工柴火（`MMT_MakeKindling_Hand`）

- **MMT**：原木(WoodLog)×10 -> 柴火×20 @ 手工锯台 ｜ 工作量=500 ｜ 研究:木工加工(MMT_Woodworking)
- **HSK** `MakeKindling_Hand`：[Wooden]×10 -> 柴火(Kindling)×20 @ 手工锯台(TableSawmillHand) ｜ 工作量=500 ｜ 研究:Craft_0
- **对比**：数值一致

#### 制作木炭（`MMT_MakeCharcoal_Hand`）

- **MMT**：原木(WoodLog)×25 -> 木炭×20 @ 窑炉 ｜ 工作量=1600 ｜ 研究:木炭烧制(MMT_CharcoalBurning)
- **HSK** `MakeCharcoal_Electric`：[Wooden]×25 -> 木炭(Charcoal)×20 @ ElectricSmelter ｜ 工作量=800 ｜ 研究:Metals_C1，技能:Crafting≥4
- **对比**：工作量 1600 vs HSK 800

#### 加工竹板（`MMT_MakeBambooPlanks_Hand`）

- **MMT**：竹节×10 -> 竹板×15 @ 手工锯台 ｜ 工作量=1200 ｜ 研究:木工加工(MMT_Woodworking)，技能:Crafting≥6
- **HSK** `MakeBambooPlanks_Hand`：竹节(Bamboo)×10 -> 竹板(BambooPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:Craft_0，技能:Crafting≥6
- **对比**：数值一致

#### 加工红木木板（`MMT_MakeRedWoodPlanks_Hand`）

- **MMT**：红木×10 -> 红木木板×15 @ 手工锯台 ｜ 工作量=1200 ｜ 研究:木工加工(MMT_Woodworking)，技能:Crafting≥3
- **HSK** `MakeRedWoodPlanks_Hand`：红木(RedWoodLog)×10 -> 红木木板(RedWoodPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:Craft_0，技能:Crafting≥3
- **对比**：数值一致

## 4. HSK 有而 MMT 未实现的配方（限于 MMT 涉及的资源）

以下 HSK 配方产出 MMT 已涉及的资源，但 MMT 尚未实现对应配方，可作为后续补全清单。

### 4.1 灰烬(Ash)

- `BurnThings`：[Wooden] 或 [Woody] 或 [Textiles] 或 [Leathers]×20 -> 灰烬(Ash)×3 @ Campfire, ElectricCrematorium, Kiln ｜ 工作量=400 ｜ 研究:Craft_0 ｜ **MMT 未实现**
- `CremateCorpse`：— -> 灰烬(Ash)×4 @ Campfire, ElectricCrematorium, Kiln ｜ 工作量=600 ｜ 研究:Craft_0 ｜ **MMT 未实现**
- `CremateCorpseCampfire`：[Corpses]×1 -> 灰烬(Ash)×4 ｜ 工作量=800 ｜ 研究:Craft_0 ｜ **MMT 未实现**

### 4.2 木炭(Charcoal)

- `MakeCharcoal_Hand`：[Wooden]×25 -> 木炭(Charcoal)×20 @ Campfire, Kiln, 熔炉(TableFurnace) ｜ 工作量=1600 ｜ 研究:Craft_B1 ｜ **MMT 未实现**

### 4.3 塑料(Plastic)

- `MakePlasticSulphate`：聚合物(Polymers)×20 + 硫酸盐(Sulphates)×5 -> 塑料(Plastic)×20 @ 石化产品实验室(TableChemlab) ｜ 工作量=2400 ｜ 技能:Crafting≥10 ｜ **MMT 未实现**
- `MakePlasticSulphateBulk`：聚合物(Polymers)×60 + 硫酸盐(Sulphates)×15 -> 塑料(Plastic)×60 @ 石化产品实验室(TableChemlab) ｜ 工作量=2400 ｜ 技能:Crafting≥10 ｜ **MMT 未实现**

### 4.4 合成橡胶(Rubber)

- `VulcanizeRubber`：天然橡胶(RawCaoutchouc)×30 + 灰烬(Ash) 或 硫磺(Sulfur) 或 硫酸盐(Sulphates)×3 -> 合成橡胶(Rubber)×10 @ Kiln, 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 研究:BiofuelRefining，技能:Crafting≥10 ｜ **MMT 未实现**

### 4.5 碎石(CrushedStone)

- `MakeCrushedStone_Electric_Chunk`：[StoneChunks]×1 -> 碎石(CrushedStone)×32 @ TableMachining ｜ 工作量=1200 ｜ **MMT 未实现**
- `MakeCrushedStone_Hand`：[StoneBlocks]×20 -> 碎石(CrushedStone)×20 @ TableStonecutter ｜ 工作量=1200 ｜ **MMT 未实现**
- `MakeCrushedStone_Hand_Chunk`：[StoneChunks]×1 -> 碎石(CrushedStone)×32 @ TableStonecutter ｜ 工作量=2400 ｜ **MMT 未实现**

### 4.6 沙(SandResource)

- `MakeSand_Electric_Chunk`：[StoneChunks]×1 -> 沙(SandResource)×80 @ TableMachining ｜ 工作量=1500 ｜ **MMT 未实现**
- `MakeSand_Hand`：碎石(CrushedStone)×10 -> 沙(SandResource)×25 @ TableStonecutter ｜ 工作量=600 ｜ 研究:Stonecutting ｜ **MMT 未实现**
- `MakeSand_Hand_Chunk`：[StoneChunks]×1 -> 沙(SandResource)×80 @ TableStonecutter ｜ 工作量=3000 ｜ 研究:Stonecutting ｜ **MMT 未实现**

### 4.7 表面活性剂(Surfactant)

- `MakeSurfactant_CoconutSubstitute`：盐(Salt)×10 + 硫磺(Sulfur)×15 + 椰子(RawCoconut)×25 -> 表面活性剂(Surfactant)×50 @ Brewery, DrugLab, ElectricBrewery ｜ 工作量=4000 ｜ 研究:Firefoam，技能:Crafting≥12, Intellectual≥4 ｜ **MMT 未实现**

### 4.8 先进机械组件(AdvMechanism)

- `MakeAdvMechanism_Hand`：[USLDBar]×30 + 高级零件(ComponentAdvanced)×10 + 人造纤维(SyntheticFibers)×5 -> 先进机械组件(AdvMechanism)×5 @ FabricationBench ｜ 工作量=3200 ｜ 研究:Components_D1，技能:Crafting≥13 ｜ **MMT 未实现**

### 4.9 集成电路(Electronics)

- `DeconstructPersonalShield`：Apparel_ShieldBelt 或 Apparel_ShieldGuardian×1 -> 高级零件(ComponentAdvanced)×8 + 集成电路(Electronics)×1 @ TableRecycling ｜ 工作量=1000 ｜ 技能:Crafting≥10 ｜ **MMT 未实现**

### 4.10 机械组件(Mechanism)

- `MakeSimpleMechanism_Hand`：[SLDBar]×30 + 工业零件(ComponentIndustrial)×10 -> 机械组件(Mechanism)×5 @ FabricationBench ｜ 工作量=1800 ｜ 技能:Crafting≥4 ｜ **MMT 未实现**

### 4.11 绘画用品(PaintingSupplies)

- `CookPaintDye`：Dye×10 + [Woody]×2 -> 绘画用品(PaintingSupplies)×1 @ ElectricStove, FueledStove ｜ 工作量=550 ｜ 研究:Art_A1，技能:Crafting≥5 ｜ **MMT 未实现**

### 4.12 原始零部件(ComponentMedieval)

- `Make_ComponentMedievalBronze`：[SLDBar] 或 [HCMBar] 或 [PRSBar]×10 -> 原始零部件(ComponentMedieval)×10 @ AdvToolBench, CraftingSpot, FabricationBench ｜ 工作量=900 ｜ 研究:Craft_0 ｜ **MMT 未实现**

### 4.13 钢筋混凝土(ReinforcedConcrete)

- `MakeReinforcedConcrete_Hand`：[Metallic]×10 + 混凝土(ConcreteResource)×10 -> 钢筋混凝土(ReinforcedConcrete)×8 @ TableStonecutter ｜ 工作量=1600 ｜ 研究:Concrete_C1，技能:Crafting≥4 ｜ **MMT 未实现**

### 4.14 电线(Wire)

- `MakeWires_Hand`：[HCMBar]×10 -> 电线(Wire)×20 @ FabricationBench ｜ 工作量=800 ｜ 研究:Craft_0，技能:Crafting≥3 ｜ **MMT 未实现**

### 4.15 绷带包(Bandagekit)

- `MakeBandageKitBulk`：[BTextiles]×30 -> 绷带包(Bandagekit)×3 @ ElectricTailoringBench, HandTailoringBench, 手工制药台(meditable) ｜ 工作量=750 ｜ 研究:Medicine_A2，技能:Medicine≥2, Crafting≥2 ｜ **MMT 未实现**

### 4.16 粗制绷带(Bandagekitcrude)

- `MakeBandagekitcrudeBulk`：[BTextiles]×21 -> 粗制绷带(Bandagekitcrude)×3 @ CraftingSpot, ElectricTailoringBench, HandTailoringBench, LeatherworkerTable, 手工制药台(meditable) ｜ 工作量=450 ｜ 研究:Medicine_A1，技能:Medicine≥1, Crafting≥1 ｜ **MMT 未实现**


