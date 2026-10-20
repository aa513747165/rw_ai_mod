# 当前 MOD（MMT）资源生产链路 与 HSK 对比

- MMT 数据源：`1.7/Defs`（本 MOD）；HSK 数据源：[hsk-resource-production-paths.md](hsk-resource-production-paths.md)（`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`）。
- 由 `tools/build_mmt_chains_compare.js` 自动生成；MMT 侧使用 MOD 实际中文 label，HSK 侧沿用 MMT 对应物的中文名并附 defName。
- 匹配规则：产出物的 HSK 对应 def 相同，且首选投入/建筑别名一致者优先；`MMT 新增` 表示 HSK 无该资源或对应配方。

## 1. 汇总

| 项 | 数量 |
| --- | --- |
| MMT 矿脉 | 16 |
| MMT 配方 | 104 |
| 与 HSK 数值一致 | 61 |
| 与 HSK 有数值差异 | 43 |
| HSK 有对应资源但无对应配方 | 0 |
| MMT 新增（HSK 无对应资源） | 0 |

## 2. 矿脉对比

| MMT 矿脉 | 产出 | MMT 单格产量 | MMT 矿团/散布/耐久 | HSK 对应矿脉 | HSK 单格产量 | HSK 矿团/散布/耐久 | 对比 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 镍矿矿藏(MMT_MineableNickelOre) | 镍矿 | 50 | 5~10 / 0.4 / 10000 | Nickel Ore Deposits(`MineableNickel`) | 50 | 5~10 / 0.4 / 10000 | 产量一致 |
| 钛铁矿(MMT_MineableIlmenite) | 钛铁矿 | 75 | 3~9 / 0.25 / 13000 | Ilmenite Ore Deposits(`MineableTitanium`) | 75 | 3~9 / 0.25 / 13000 | 产量一致 |
| 黑钨矿(MMT_MineableWolframite) | 黑钨矿 | 50 | 3~9 / 0.15 / 20000 | Wolframite Ore Deposits(`MineableTungsten`) | 50 | 3~9 / 0.15 / 20000 | 产量一致 |
| 压实的盐(MMT_MineableSalt) | 盐 | 150 | 5~10 / 1.2 / 900 | Compacted salt(`MineableSalt`) | 150 | 5~10 / 1.2 / 900 | 产量一致 |
| 粘土矿脉(MMT_MineableSoftClay) | 软粘土 | 75 | 8~15 / 0.7 / 1400 | — | — | — | MMT 新增资源 |
| 煤矿(MMT_MineableCoal) | 原煤 | 75 | 11~18 / 1.2 / 1500 | Coal Ore Deposits(`MineableCoal`) | 75 | 11~18 / 1.2 / 1500 | 产量一致 |
| 铁矿(MMT_MineableIronOre) | 铁矿石 | 75 | 15~25 / 1.0 / 1500 | Iron Ore Deposits(`MineableSteel`) | 35 | 20~40 / 1.1 / 2800 | 产量 75 vs HSK 35 |
| 铜矿(MMT_MineableCopperOre) | 铜矿 | 75 | 15~25 / 0.95 / 1800 | Copper Ore Deposits(`MineableCopper`) | 75 | 15~25 / 0.95 / 1800 | 产量一致 |
| 锡矿(MMT_MineableTinOre) | 锡矿 | 75 | 9~17 / 1.1 / 1300 | Tin Ore Deposits(`MineableTin`) | 75 | 9~17 / 1.1 / 1300 | 产量一致 |
| 铅矾矿脉(MMT_MineableAnglesite) | 铅矾矿 | 75 | 6~13 / 0.8 / 1600 | Anglesite Deposits(`MineableLead`) | 75 | 6~13 / 0.8 / 6000 | 产量一致 |
| 铝土矿脉(MMT_MineableAluminium) | 铝土矿 | 75 | 11~19 / 0.65 / 1600 | Bauxite Ore Deposits(`MineableAluminium`) | 75 | 11~19 / 0.65 / 5500 | 产量一致 |
| 油苗(MMT_MineableOilSeep) | 原油 | 75 | 10~18 / 0.8 / 1500 | — | — | — | MMT 新增资源 |
| 硝石矿脉(MMT_MineableNitre) | 硝石 | 75 | 8~16 / 1.05 / 1800 | Nitre Deposits(`MineableNitre`) | 75 | 8~16 / 1.05 / 1800 | 产量一致 |
| 银矿(MMT_MineableSilverOre) | 白银 | 75 | 10~16 / 1.0 / 1500 | Silver Ore Deposits(`MineableSilver`) | 250 | 7~15 / 0.25 / 3500 | 产量 75 vs HSK 250 |
| 金矿(MMT_MineableGoldOre) | 金矿 | 75 | 8~14 / 0.5 / 1500 | Gold Ore Deposits(`MineableGold`) | 50 | 3~9 / 0.15 / 4500 | 产量 75 vs HSK 50 |
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

- **MMT**：[Metallic]×10 + 混凝土×10 -> 钢筋混凝土×8 @ — ｜ 工作量=1600 ｜ 研究:混凝土技术(MMT_ConcreteBasic)，技能:Crafting≥4
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

### 3.5 电动金属（Electric）

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
- **HSK** `ConvertToSilver`：Matter×100 -> 白银(Silver)×300 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 14 vs HSK 100；产出 10 vs HSK 300；工作量 500 vs HSK 12000；技能 Crafting≥5 vs HSK Crafting≥15

#### 提炼纯金合金（15个）（`MMT_SmeltGold_Electric`）

- **MMT**：金矿×12 -> 金(Gold)×15 @ 电弧炉 ｜ 工作量=500 ｜ 研究:电弧冶炼(MMT_ElectricSmelting)，技能:Crafting≥6
- **HSK** `ConvertToGold`：Matter×60 -> 金矿(Gold)×20 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
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

### 3.6 电动电子（ElectricElectronics）

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

### 3.7 电动石化（ElectricPetrochem）

#### 电蒸馏聚合物（`MMT_DistillPolymers_Electric`）

- **MMT**：原油×20 -> 聚合物×15 @ 石化产品实验室 ｜ 工作量=1000 ｜ 研究:电动石化工艺(MMT_ElectricPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToPolymers`：Matter×50 -> 聚合物(Polymers)×100 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
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

### 3.8 电动木材（ElectricWood）

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

### 3.9 电子（Electronics）

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

### 3.10 玻璃（Glass）

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

### 3.11 金属（Metal）

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

### 3.12 零件（Parts）

#### 使用木板组装原始零部件（`MMT_MakeComponentMedieval_Wood`）

- **MMT**：原木(WoodLog)×20 -> 原始零部件×10 @ 手工装配台, 电动装配台 ｜ 工作量=900 ｜ 研究:零件加工(MMT_ComponentCrafting)
- **HSK** `Make_ComponentMedieval`：[Woody]×20 -> 原始零部件(ComponentMedieval)×10 @ AdvToolBench, CraftingSpot, FabricationBench, 电动锯木台(TableSawmillElectric), 手工锯台(TableSawmillHand) ｜ 工作量=900 ｜ 研究:Craft_0
- **对比**：数值一致

#### 使用金属组装原始零部件（`MMT_MakeComponentMedieval_Metal`）

- **MMT**：钢(Steel) 或 铜×10 -> 原始零部件×10 @ 手工装配台, 电动装配台 ｜ 工作量=900 ｜ 研究:零件加工(MMT_ComponentCrafting)
- **HSK** `Make_ComponentMedieval`：[Woody]×20 -> 原始零部件(ComponentMedieval)×10 @ AdvToolBench, CraftingSpot, FabricationBench, 电动锯木台(TableSawmillElectric), 手工锯台(TableSawmillHand) ｜ 工作量=900 ｜ 研究:Craft_0
- **对比**：首投入 10 vs HSK 20

#### 组装普通零部件（`MMT_MakeComponentIndustrial_Hand`）

- **MMT**：钢(Steel) 或 铁钛合金(Plasteel)×30 -> 工业零件(ComponentIndustrial)×10 @ 手工装配台 ｜ 工作量=2000 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥3
- **HSK** `Make_ComponentIndustrial`：[SLDBar] 或 [USLDBar]×30 -> 工业零件(ComponentIndustrial)×10 @ AdvToolBench, FabricationBench ｜ 工作量=2000 ｜ 研究:Craft_0，技能:Crafting≥3
- **对比**：数值一致

#### 组装普通零部件（`MMT_MakeComponentIndustrial_Electric`）

- **MMT**：钢(Steel) 或 铁钛合金(Plasteel)×30 -> 工业零件(ComponentIndustrial)×10 @ 电动装配台 ｜ 工作量=2000 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥3
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

- **MMT**：钢(Steel)×30 + 工业零件(ComponentIndustrial)×10 -> 机械组件×5 @ 手工装配台 ｜ 工作量=1800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥4
- **HSK** `MakeSimpleMechanism_Electric`：[SLDBar]×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件(Mechanism)×10 @ AdvToolBench ｜ 工作量=1800 ｜ 技能:Crafting≥7
- **对比**：首投入 30 vs HSK 60；产出 5 vs HSK 10；技能 Crafting≥4 vs HSK Crafting≥7

#### 组装机械组件（`MMT_MakeMechanism_Electric`）

- **MMT**：钢(Steel)×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件×10 @ 电动装配台 ｜ 工作量=1800 ｜ 研究:零件加工(MMT_ComponentCrafting)，技能:Crafting≥7
- **HSK** `MakeSimpleMechanism_Electric`：[SLDBar]×60 + 工业零件(ComponentIndustrial)×20 -> 机械组件(Mechanism)×10 @ AdvToolBench ｜ 工作量=1800 ｜ 技能:Crafting≥7
- **对比**：数值一致

#### 组装高级零部件（`MMT_MakeComponentSpacer_Hand`）

- **MMT**：铁钛合金(Plasteel)×30 -> 太空零件(ComponentSpacer)×10 @ 手工装配台 ｜ 工作量=2500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥6
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：首投入 30 vs HSK 25；工作量 2500 vs HSK 3500；技能 Crafting≥6 vs HSK Crafting≥9

#### 组装高级零部件（`MMT_MakeComponentSpacer_Electric`）

- **MMT**：铁钛合金(Plasteel)×30 -> 太空零件(ComponentSpacer)×10 @ 电动装配台 ｜ 工作量=2500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥6
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：首投入 30 vs HSK 25；工作量 2500 vs HSK 3500；技能 Crafting≥6 vs HSK Crafting≥9

#### 组装先进机械组件（`MMT_MakeAdvMechanism_Hand`）

- **MMT**：铁钛合金(Plasteel)×30 + 太空零件(ComponentSpacer)×10 + 人造纤维×5 -> 先进机械组件×5 @ 手工装配台 ｜ 工作量=3200 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥13
- **HSK** `MakeAdvMechanism_Electric`：[USLDBar]×60 + 高级零件(ComponentAdvanced)×20 + 人造纤维(SyntheticFibers)×10 -> 先进机械组件(AdvMechanism)×10 @ AdvToolBench ｜ 工作量=3200 ｜ 研究:Components_D1，技能:Crafting≥16
- **对比**：首投入 30 vs HSK 60；产出 5 vs HSK 10；技能 Crafting≥13 vs HSK Crafting≥16

#### 组装先进机械组件（`MMT_MakeAdvMechanism_Electric`）

- **MMT**：铁钛合金(Plasteel)×60 + 太空零件(ComponentSpacer)×20 + 人造纤维×10 -> 先进机械组件×10 @ 电动装配台 ｜ 工作量=3200 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥16
- **HSK** `MakeAdvMechanism_Electric`：[USLDBar]×60 + 高级零件(ComponentAdvanced)×20 + 人造纤维(SyntheticFibers)×10 -> 先进机械组件(AdvMechanism)×10 @ AdvToolBench ｜ 工作量=3200 ｜ 研究:Components_D1，技能:Crafting≥16
- **对比**：数值一致

#### 组装高级零部件（`MMT_MakeSpacerComponent`）

- **MMT**：铁钛合金(Plasteel)×25 + 银(Silver) 或 金(Gold)×5 -> 高级零部件×10 @ 电动装配台 ｜ 工作量=3500 ｜ 研究:高级零件(MMT_AdvancedComponents)，技能:Crafting≥9
- **HSK** `Make_ComponentSpacer`：[USLDBar]×25 + [PRSBar]×5 -> 高级零部件(ComponentSpacer)×10 @ AdvToolBench ｜ 工作量=3500 ｜ 研究:Components_D1，技能:Crafting≥9
- **对比**：数值一致

#### 组装超级零部件（`MMT_MakeUltraComponent`）

- **MMT**：钨合金 或 钛合金×15 + 司太立合金（钨铬钴合金） 或 钨钴合金×10 + 银(Silver) 或 金(Gold)×5 -> 超级零部件×10 @ 自动化装配器 ｜ 工作量=4500 ｜ 研究:超级零件(MMT_UltraComponents)，技能:Crafting≥13
- **HSK** `Make_ComponentUltra`：[RARBar]×15 + [USLDHBar]×10 + [PRSBar]×5 -> 超级零部件(ComponentUltra)×10 @ 自动化装配器(RobAssem) ｜ 工作量=4500 ｜ 研究:Components_E1，技能:Crafting≥13
- **对比**：数值一致

### 3.13 石化（Petrochem）

#### 蒸馏聚合物（`MMT_DistillPolymers_Hand`）

- **MMT**：原油×20 -> 聚合物×15 @ 石化产品实验室 ｜ 工作量=1500 ｜ 研究:基础石化工艺(MMT_BasicPetrochemistry)，技能:Crafting≥4
- **HSK** `ConvertToPolymers`：Matter×50 -> 聚合物(Polymers)×100 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
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

### 3.14 PetrochemExt（PetrochemExt）

#### 精炼石蜡（10个）（`MMT_MakeParaffins`）

- **MMT**：原油×20 -> 石蜡×10 @ 石化产品实验室, 石化产品实验室 ｜ 工作量=1000 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToParaffins`：Matter×50 -> 石蜡(Paraffins)×100 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 20 vs HSK 50；产出 10 vs HSK 100；工作量 1000 vs HSK 12000；技能 Crafting≥6 vs HSK Crafting≥15

#### 制备硫酸盐（10个）（`MMT_MakeSulphates`）

- **MMT**：硫磺×10 -> 硫酸盐×10 @ 石化产品实验室, 石化产品实验室 ｜ 工作量=800 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥6
- **HSK** `ConvertToSulphates`：Matter×50 -> 硫酸盐(Sulphates)×100 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
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

- **MMT**：盐×10 + 硫磺×15 + [Alcohol]×15 -> 表面活性剂×50 @ — ｜ 工作量=4000 ｜ 研究:高级石化工艺(MMT_AdvancedPetrochemistry)，技能:Crafting≥8
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

### 3.15 贵金属（Precious）

#### 提炼纯银合金（10个）（`MMT_SmeltSilver_Hand`）

- **MMT**：白银×14 -> 银(Silver)×10 @ 熔炉 ｜ 工作量=750 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥2
- **HSK** `ConvertToSilver`：Matter×100 -> 白银(Silver)×300 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 14 vs HSK 100；产出 10 vs HSK 300；工作量 750 vs HSK 12000；技能 Crafting≥2 vs HSK Crafting≥15

#### 提炼纯金合金（15个）（`MMT_SmeltGold_Hand`）

- **MMT**：金矿×12 -> 金(Gold)×15 @ 熔炉 ｜ 工作量=750 ｜ 研究:基础金属冶炼(MMT_BasicMetalworking)，技能:Crafting≥3
- **HSK** `ConvertToGold`：Matter×60 -> 金矿(Gold)×20 @ MatterConverter ｜ 工作量=12000 ｜ 研究:Super_matter_E1，技能:Crafting≥15
- **对比**：首投入 12 vs HSK 60；产出 15 vs HSK 20；工作量 750 vs HSK 12000；技能 Crafting≥3 vs HSK Crafting≥15

### 3.16 木材（Wood）

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

## 4. HSK 有而 MMT 未覆盖的配方（限于 MMT 涉及的资源）

- **灰烬(Ash)**：`BurnThings`（Campfire, ElectricCrematorium, Kiln，工作量=400）、`CremateCorpse`（Campfire, ElectricCrematorium, Kiln，工作量=600）、`CremateCorpseCampfire`（，工作量=800）
- **木炭(Charcoal)**：`MakeCharcoal_Hand`（Campfire, Kiln, TableFurnace，工作量=1600）
- **塑料(Plastic)**：`MakePlasticSulphate`（TableChemlab，工作量=2400）、`MakePlasticSulphateBulk`（TableChemlab，工作量=2400）
- **合成橡胶(Rubber)**：`VulcanizeRubber`（Kiln, TableChemlab，工作量=800）
- **碎石(CrushedStone)**：`MakeCrushedStone_Electric_Chunk`（TableMachining，工作量=1200）、`MakeCrushedStone_Hand`（TableStonecutter，工作量=1200）、`MakeCrushedStone_Hand_Chunk`（TableStonecutter，工作量=2400）
- **沙(SandResource)**：`MakeSand_Electric_Chunk`（TableMachining，工作量=1500）、`MakeSand_Hand`（TableStonecutter，工作量=600）、`MakeSand_Hand_Chunk`（TableStonecutter，工作量=3000）
- **表面活性剂(Surfactant)**：`MakeSurfactant_CoconutSubstitute`（Brewery, DrugLab, ElectricBrewery，工作量=4000）
- **先进机械组件(AdvMechanism)**：`MakeAdvMechanism_Hand`（FabricationBench，工作量=3200）
- **碳纤维锭(CarbonAlloy)**：`DisassembleAIPersonaCore`（RobAssem，工作量=4000）
- **集成电路(Electronics)**：`DeconstructPersonalShield`（TableRecycling，工作量=1000）
- **机械组件(Mechanism)**：`MakeSimpleMechanism_Hand`（FabricationBench，工作量=1800）
- **原始零部件(ComponentMedieval)**：`Make_ComponentMedievalBronze`（AdvToolBench, CraftingSpot, FabricationBench，工作量=900）
- **钢筋混凝土(ReinforcedConcrete)**：`MakeReinforcedConcrete_Hand`（TableStonecutter，工作量=1600）
- **电线(Wire)**：`MakeWires_Hand`（FabricationBench，工作量=800）

