# MMT 其它物品手册（非武器/非建筑）

- 由 `tools/build_mmt_items_doc.py` 自动生成（改 Def 后重跑）；MMT 数据源 `1.7/Defs`，HSK 数据源 `Hardcore-SK/Mods/Core_SK`（只读参考）。
- 被引用 = 作为配方配料或建筑造价出现的次数（类别过滤不计）；产出 = 有配方能制造它；材料 = stuffProps 类别（可作建造/制作用材料）。
- 第 2 部分为 HSK Core_SK 资源/材料类物品中 MMT 未实现的清单（不含防具服装，MMT 无防具链）；武器见 [mmt-weapons.md](mmt-weapons.md)，建筑见 [mmt-buildings.md](mmt-buildings.md)。

## 1. MMT 物品总表

共 286 件（不含抽象基类）。

### 1.1 资源与材料

#### 高级合金（AdvancedAlloys）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 镍合金（`MMT_NickelBar`） | 6.0 | 150 | 金属 | MMT_SmeltNickelBar | 4 |
| 钴合金（`MMT_Cobalt`） | 10.0 | 150 | — | MMT_ExtractCobalt | 3 |
| 铬合金（`MMT_Chromium`） | 5.0 | 150 | — | MMT_ExtractChromium | 1 |
| 钨合金（`MMT_Tungsten`） | 40.0 | 150 | — | MMT_SmeltTungsten | 3 |
| 钛合金（`MMT_Titanium`） | 45.0 | 75 | 金属 | MMT_SmeltTitanium | 8 |
| 司太立合金（钨铬钴合金）（`MMT_StelliteAlloy`） | 30.0 | 150 | 金属 | MMT_MakeStellite | 1 |
| 镍钛合金（`MMT_NitinolAlloy`） | 27.0 | 150 | 金属 | MMT_MakeNitinol | 5 |
| 钨钴合金（`MMT_PobediteAlloy`） | 33.0 | 150 | 金属 | MMT_MakePobedite | 1 |
| 贫铀合金（`MMT_DepletedUranium`） | 18.0 | 75 | 金属 | MMT_MakeDepletedUranium | 1 |
| 斯坦合金（`MMT_SteelBar`） | 8.0 | 150 | 金属 | MMT_MakeSteelBar | 7 |
| 梅尔基奥合金（`MMT_CupronickelAlloy`） | 8.0 | 150 | 金属 | MMT_MakeCupronickelAlloy | 0 |
| 亚力可合金（`MMT_AlnicoAlloy`） | 11.0 | 150 | 金属 | MMT_MakeAlnicoAlloy | 5 |

#### 高级矿石（AdvancedOre）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 镍矿（`MMT_NickelOre`） | 1 | 300 | — | MMT_ConvertToNickel | 2 |
| 钛铁矿（`MMT_IlmeniteOre`） | 1 | 300 | — | MMT_ConvertToTitanium | 1 |
| 黑钨矿（`MMT_WolframiteOre`） | 1 | 300 | — | — | 1 |

#### 农业（Agriculture）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 堆肥料（`MMT_Compost`） | 2.4 | 300 | — | MMT_MakeCompost | 1 |
| 肥料（`MMT_Fertilizer`） | 3.8 | 300 | — | MMT_MakeFertilizer | 3 |
| 土堆（`MMT_Dirt`） | 0.2 | 900 | — | MMT_MakeDirt、MMT_MakeDirt_Hand、MMT_MakeDirt_Electric | 0 |
| 盐（`MMT_Salt`） | 1 | 600 | — | MMT_MineSalt | 2 |

#### 仿生（Bionics）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 人造骨头（`MMT_ArtificialBone`） | 60 | 25 | — | MMT_MakeArtificialBone、MMT_MakeArtificialBoneBulk | 0 |
| 医疗助理芯片（`MMT_MedicalAssistantChip`） | 500 | 50 | — | — | 0 |

#### 陶瓷（Ceramics）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 软粘土（`MMT_SoftClay`） | 1 | 600 | — | — | 2 |
| 粘土砖（`MMT_ClayBrick`） | 3 | 300 | MMT_Bricks | MMT_MakeClayBricks | 0 |
| 陶瓷（`MMT_Ceramics`） | 12 | 300 | 石质 | MMT_MakeCeramics | 0 |
| 灰烬（`MMT_Ash`） | 0.3 | 600 | — | MMT_BurnThings、MMT_CremateCorpseCampfire | 2 |

#### 煤（Coal）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 原煤（`MMT_Coal`） | 1 | 600 | — | — | 5 |

#### 混凝土（Concrete）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 混凝土（`MMT_Concrete`） | 6.0 | 150 | — | MMT_MixConcrete_Hand、MMT_MixConcrete_Electric | 2 |
| 钢筋混凝土（`MMT_ReinforcedConcrete`） | 16.0 | 75 | 石质 | MMT_MakeReinforcedConcrete_Hand、MMT_MakeReinforcedConcrete_Electric | 3 |

#### 作物（Crops）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 原棉（`MMT_RawCotton`） | 1.5 | 300 | — | — | 2 |
| 生亚麻（`MMT_RawFlax`） | 3.5 | 300 | — | — | 1 |
| 生恶魔菇（`MMT_RawDevilstrand`） | 6.4 | 150 | — | — | 1 |
| 天然橡胶（`MMT_RawCaoutchouc`） | 2 | — | — | — | 0 |
| 竹节（`MMT_Bamboo`） | 2.0 | 300 | 木质 | — | 4 |
| 红木（`MMT_RedWoodLog`） | 2.0 | 300 | 木质 | — | 4 |
| 椰子（`MMT_RawCoconut`） | 6 | — | — | — | 1 |
| 野玫瑰（`MMT_WildRose`） | 2 | 75 | — | — | 4 |
| 金丝桃（`MMT_Hypericum`） | 2 | 75 | — | — | 5 |
| 薄荷叶（`MMT_MintLeaves`） | 2 | 75 | — | — | 4 |

#### 电子（Electronics）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 硅（`MMT_Silicon`） | 4.0 | 300 | — | MMT_ExtractSilicon_Electric、MMT_MakeSilicon_Bulk、MMT_ExtractSilicon_Hand | 6 |
| 电子元件（`MMT_ElectronicParts`） | 25.0 | 75 | — | MMT_AssembleElectronicParts_Electric、MMT_AssembleElectronicParts_Hand | 20 |
| 集成电路（`MMT_IntegratedCircuit`） | 55.0 | 75 | — | MMT_AssembleIC_Electric、MMT_AssembleIC_Hand、MMT_DeconstructPersonalShield | 12 |
| 微芯片集成电路（`MMT_Microchip`） | 100.0 | 25 | — | MMT_AssembleMicrochip_Electric、MMT_AssembleMicrochip_Hand | 11 |

#### EnergyCrystal（EnergyCrystal）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 能量结晶（`MMT_EnergyCrystal`） | 60 | 25 | — | MMT_MakeEnergyCrystal | 0 |

#### 玻璃（Glass）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 碎石（`MMT_CrushedStone`） | 0.3 | 900 | — | MMT_MakeCrushedStone_Hand、MMT_MakeCrushedStone_Electric、MMT_MakeCrushedStone_Hand_Chunk、MMT_MakeCrushedStone_Electric_Chunk | 4 |
| 沙（`MMT_Sand`） | 0.3 | 900 | — | MMT_MakeSand_Hand、MMT_MakeSand_Electric、MMT_MakeSand_Hand_Chunk、MMT_MakeSand_Electric_Chunk | 9 |
| 玻璃生料（`MMT_GlassBatch`） | 1.2 | 300 | — | MMT_MakeGlassBatch | 3 |
| 玻璃块（`MMT_Glass`） | 5.0 | 150 | — | MMT_MakeGlass | 12 |
| 强化玻璃块（`MMT_ReinforcedGlass`） | 17.0 | 150 | — | MMT_MakeReinforcedGlass | 1 |

#### 物质（Matter）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 无机物（`MMT_Matter`） | 10 | 150 | — | MMT_ConvertResourcesRaw | 15 |
| 反物质（`MMT_Antimatter`） | 120 | 75 | — | MMT_MakeAntimatter | 1 |
| 生物质（`MMT_Biomatter`） | 10 | 150 | — | MMT_ExtractBiomatterCorpse、MMT_ExtractBiomatterMeat | 4 |
| 磁性材料（`MMT_MagneticMaterial`） | 110 | 75 | — | MMT_BuildMagneticMaterial | 6 |
| 生物合成材料（`MMT_BiosyntheticMaterial`） | 90 | 75 | — | MMT_BuildBiosyntheticMaterial | 4 |
| 生物电子元件（`MMT_BioMicrochips`） | 2100 | 25 | — | MMT_MakeBioelectronicDevices、MMT_DisassembleAIPersonaCore | 2 |

#### 医药（Medicine）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 中性花瓣（`MMT_NeutroPetals`） | 1.0 | 300 | — | — | 1 |
| 芦荟叶（`MMT_Aloe`） | 5 | 150 | — | — | 2 |
| 干烟叶（`MMT_DriedLeavesSmokeleaf`） | 2.5 | 25 | — | MMT_DrySmokeleaf、MMT_DrySmokeleafBulk | 0 |
| 粗制绷带（`MMT_Bandagekitcrude`） | 5 | — | — | MMT_MakeBandagekitcrude、MMT_MakeBandagekitcrudeBulk | 0 |
| 绷带包（`MMT_Bandagekit`） | 5 | — | — | MMT_MakeBandagekit、MMT_MakeBandagekitBulk | 2 |
| 草药包（`MMT_HerbMedicine`） | 10 | 25 | — | MMT_MakeHerbMedicine | 0 |
| 基础医药包（`MMT_BasicMedicineKit`） | 10 | 25 | — | MMT_MakeBasicMedicineKit、MMT_MakeBasicMedicineKitMix | 0 |
| 抗生素（`MMT_MedicalDrink`） | 15 | 25 | — | MMT_BrewMedicalDrink | 1 |

#### 金属（Metal）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 铁矿石（`MMT_IronOre`） | 1 | 600 | — | MMT_ConvertToIron | 5 |
| 铜矿（`MMT_CopperOre`） | 1 | 600 | — | MMT_ConvertToCopper | 2 |
| 锡矿（`MMT_TinOre`） | 1 | 600 | — | MMT_ConvertToTin | 2 |
| 铜（`MMT_CopperBar`） | 3.0 | 300 | 金属 | MMT_SmeltCopperBar_Electric、MMT_SmeltCopperBar_Hand | 9 |
| 锡（`MMT_TinBar`） | 3.0 | 300 | 金属 | MMT_SmeltTinBar_Electric、MMT_SmeltTinBar_Hand | 3 |
| 青铜（`MMT_Bronze`） | 4.0 | 600 | 金属 | MMT_MakeBronze_Electric、MMT_MakeBronze_Hand | 5 |
| 铅矾矿（`MMT_Anglesite`） | 1 | 300 | — | MMT_ConvertToLead | 2 |
| 铝土矿（`MMT_AluminiumOre`） | 1 | 600 | — | MMT_ConvertToAluminium | 1 |
| 铅合金（`MMT_LeadBar`） | 6 | 300 | 金属 | MMT_SmeltLeadBar_Electric、MMT_SmeltLeadBar_Hand | 0 |
| 铝合金（`MMT_AluminiumBar`） | 3 | 300 | 金属 | MMT_SmeltAluminiumBar_Electric | 3 |
| 斯特合金（`MMT_FerrosiliconAlloy`） | 5 | 300 | 金属 | MMT_MakeFerrosiliconAlloy | 5 |
| 金属罐（`MMT_MetalCan`） | 1.7 | 200 | — | MMT_MakeMetalCan | 0 |

#### 核能（Nuclear）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 核燃料棒（`MMT_FuelRod`） | 100 | 25 | — | MMT_MakeFuelRod | 0 |

#### 矿粉（OrePowder）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 粗矿石（`MMT_CoarseOre`） | 1 | 900 | — | MMT_CrushCoarseOre | 2 |
| 矿粉（`MMT_OrePowder`） | 1 | 900 | — | MMT_GrindOrePowder_Hand、MMT_GrindOrePowder_Electric | 2 |

#### 零件（Parts）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 原始零部件（`MMT_ComponentMedieval`） | 6 | 300 | — | MMT_MakeComponentMedieval_Wood、MMT_MakeComponentMedieval_Metal | 2 |
| 电线（`MMT_Wire`） | 1.5 | 600 | — | MMT_MakeWire_Hand、MMT_MakeWire_Electric | 9 |
| 机械组件（`MMT_Mechanism`） | 200 | 75 | — | MMT_MakeMechanism_Hand、MMT_MakeMechanism_Electric | 25 |
| 先进机械组件（`MMT_AdvMechanism`） | 400 | 75 | — | MMT_MakeAdvMechanism_Hand、MMT_MakeAdvMechanism_Electric | 14 |
| 高级零部件（`MMT_SpacerComponent`） | 150 | 150 | — | MMT_MakeSpacerComponent | 18 |
| 超级零部件（`MMT_UltraComponent`） | 200 | 150 | — | MMT_MakeUltraComponent | 7 |

#### 石化（Petrochem）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 原油（`MMT_CrudeOil`） | 1 | 300 | — | MMT_PumpCrudeOil | 3 |
| 聚合物（`MMT_Polymers`） | 1.5 | 600 | — | MMT_DistillPolymers_Electric、MMT_ConvertToPolymers、MMT_DistillPolymers_Hand | 11 |
| 塑料（`MMT_Plastic`） | 9.0 | 300 | — | MMT_MakePlastic_Electric、MMT_MakePlastic_Hand | 68 |
| 合成橡胶（`MMT_Rubber`） | 11.0 | 300 | — | MMT_MakeRubber_Electric、MMT_MakeRubber_Hand | 17 |
| 人造纤维（`MMT_SyntheticFibers`） | 12.0 | 150 | — | MMT_MakeSyntheticFibers_Electric、MMT_MakeSyntheticFibers_Hand | 20 |
| 碳聚物（`MMT_Carbon`） | 40.0 | 75 | — | MMT_MakeCarbon_Electric、MMT_MakeCarbon_Hand | 4 |

#### 石化扩展（PetrochemExt）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 石蜡（`MMT_Paraffins`） | 14.0 | 150 | — | MMT_ConvertToParaffins、MMT_MakeParaffins | 5 |
| 硫酸盐（`MMT_Sulphates`） | 7.0 | 300 | — | MMT_ConvertToSulphates、MMT_MakeSulphates | 2 |
| 表面活性剂（`MMT_Surfactant`） | 0.4 | 150 | — | MMT_MakeSurfactant、MMT_MakeSurfactant_Compaste、MMT_MakeSurfactant_CoconutSubstitute | 2 |
| 复合粘剂（`MMT_Compaste`） | 5.0 | 300 | — | MMT_MakeCompaste | 3 |
| 火药（`MMT_Powder`） | 25.0 | 150 | — | MMT_MakePowder | 10 |
| 凝固汽油（`MMT_Napalm`） | 5.0 | 300 | — | MMT_MakeNapalm | 2 |
| 硝石（`MMT_Nitre`） | 3.0 | 300 | — | — | 2 |
| 氧化化合燃料（`MMT_OxidizedChemfuel`） | 9.0 | 375 | — | MMT_MakeOxidizedChemfuel | 0 |
| 碳纤维锭（`MMT_CarbonAlloy`） | 40.0 | 75 | 金属 | MMT_MakeCarbonAlloy、MMT_DisassembleAIPersonaCore | 6 |

#### 贵金属（Precious）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 白银（`MMT_SilverOre`） | 1 | 1800 | — | MMT_ConvertToSilver | 2 |
| 金矿（`MMT_GoldOre`） | 1 | 300 | — | MMT_ConvertToGold | 2 |

#### 处理机模块（ProcessorModules）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 超频模块（`MMT_OverclockModule`） | 180 | 10 | — | MMT_MakeOverclockModule | 0 |
| 并行模块（`MMT_ParallelModule`） | 220 | 10 | — | MMT_MakeParallelModule | 0 |
| 矿产增值模块（`MMT_YieldModule`） | 260 | 10 | — | MMT_MakeYieldModule | 0 |

#### 机器人（Robotics）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 动力电池（`MMT_Hexcell`） | 150 | 50 | — | MMT_MakeHexcell | 1 |
| 机器人部件（`MMT_RobotParts`） | 70 | 75 | — | MMT_MakeRobotParts | 0 |
| 矿物声纳模块（`MMT_MineralSonarModule`） | 300 | 25 | — | MMT_MakeMineralSonarModule | 0 |

#### 筛网（SieveMeshes）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 木滤网（`MMT_MeshWood`） | 25 | 10 | — | MMT_MakeMeshWood | 0 |
| 铁滤网（`MMT_MeshIron`） | 60 | 10 | — | MMT_MakeMeshIron | 0 |
| 精密滤网（`MMT_MeshFine`） | 220 | 10 | — | MMT_MakeMeshFine | 0 |

#### 硫（Sulfur）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 硫磺（`MMT_Sulfur`） | 1 | 300 | — | MMT_MakeSulfur | 8 |

#### 超级合金（SuperAlloys）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| α阿尔法聚合物（`MMT_AlphaPoly`） | 45 | 75 | 金属 | MMT_DisassembleAIPersonaCore、MMT_ProduceAPoly | 2 |
| β贝塔聚合物（`MMT_BetaPoly`） | 65 | 75 | 金属 | MMT_ProduceBPoly | 0 |
| 生物铁合金（`MMT_BioferriteAlloy`） | 9 | 600 | Bioferrite/金属 | MMT_MakeBioferriteAlloy | 0 |

#### TeleportModules（TeleportModules）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 充能效率模块（`MMT_ChargeEfficiencyModule`） | 220 | 10 | — | MMT_MakeChargeEfficiencyModule | 0 |
| 次元传送模块（`MMT_DimensionalModule`） | 420 | 10 | — | MMT_MakeDimensionalModule | 0 |

#### 纺织（Textiles）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 亚麻布（`MMT_Flaxcloth`） | 6 | 150 | 织物 | MMT_MakeFlaxcloth | 0 |
| 大麻布（`MMT_HempCloth`） | 3 | 300 | 织物 | MMT_MakeHempCloth | 0 |
| 丝绒（`MMT_Velour`） | 12.0 | 300 | 织物 | MMT_MakeVelour | 0 |
| 凯夫拉（`MMT_Kevlar`） | 25 | 150 | 织物 | MMT_MakeKevlar | 0 |
| 甲壳板（`MMT_ChitinPlating`） | 35 | 75 | 织物 | MMT_MakeChitinPlating | 0 |
| 甲壳（`MMT_Chitin`） | 7 | 150 | 是 | — | 1 |
| 兽脂（`MMT_Tallow`） | 2 | 75 | — | MMT_MakeTallow | 0 |
| 绘画用品（`MMT_PaintingSupplies`） | 50 | 10 | — | MMT_CookPaint、MMT_CookPaintDye | 0 |
| 染料（`MMT_Dye`） | 1.5 | 150 | — | MMT_MakeDye | 1 |

#### 武器部件（WeaponParts）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 武器部件（`MMT_WeaponParts`） | 65 | 75 | — | MMT_MakeWeaponParts | 88 |
| 手枪部件（`MMT_PistolComponent`） | 50 | 75 | — | MMT_MakePistolComponent | 7 |
| 冲锋枪部件（`MMT_SMGComponent`） | 150 | 25 | — | MMT_MakeSMGComponent | 8 |
| 霰弹枪部件（`MMT_ShotgunComponent`） | 150 | 25 | — | MMT_MakeShotgunComponent | 6 |
| 步枪部件（`MMT_RifleComponent`） | 200 | 25 | — | MMT_MakeRifleComponent | 16 |
| 先进步枪部件（`MMT_AdvRifleComponent`） | 400 | 25 | — | MMT_MakeAdvRifleComponent | 0 |
| 狙击枪部件（`MMT_SniperComponent`） | 250 | 25 | — | MMT_MakeSniperComponent | 5 |
| 先进狙击枪部件（`MMT_AdvSniperComponent`） | 1250 | 25 | — | MMT_MakeAdvSniperComponent | 4 |
| 重型武器部件（`MMT_HeavyComponent`） | 500 | 25 | — | MMT_MakeHeavyComponent | 15 |
| 火炮武器部件（`MMT_CannonComponent`） | 600 | 25 | — | MMT_MakeCannonComponent | 7 |
| 发射器部件（`MMT_LauncherComponent`） | 750 | 25 | — | MMT_MakeLauncherComponent | 8 |
| 电荷武器部件（`MMT_ChargedComponent`） | 687 | 25 | — | MMT_MakeChargedComponent | 3 |
| 离子武器部件（`MMT_PlasmaComponent`） | 1000 | 25 | — | MMT_MakePlasmaComponent | 6 |
| 光束武器部件（`MMT_LaserComponent`） | 1000 | 25 | — | MMT_MakeLaserComponent | 6 |

#### 木材（Wood）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 木板（`MMT_WoodPlank`） | 1.5 | 300 | 木质 | MMT_MakeWoodPlanks_Electric、MMT_MakeWoodPlanks_Hand | 2 |
| 柴火（`MMT_Kindling`） | 2.3 | 300 | — | MMT_MakeKindling_Electric、MMT_MakeKindling_Hand | 0 |
| 木炭（`MMT_Charcoal`） | 2.0 | 600 | — | MMT_MakeCharcoal_Electric、MMT_MakeCharcoal_Hand | 5 |
| 竹板（`MMT_BambooPlank`） | 1.5 | 300 | 木质 | MMT_MakeBambooPlanks_Electric、MMT_MakeBambooPlanks_Hand | 2 |
| 红木木板（`MMT_RedWoodPlank`） | 3.5 | 300 | 木质 | MMT_MakeRedWoodPlanks_Electric、MMT_MakeRedWoodPlanks_Hand | 2 |

### 1.2 制品（仿生/药品/护盾/炮塔弹药等）

#### 仿生（Bionics）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 眼罩（`MMT_EyePatch`） | 50 | — | — | — | 1 |
| 钩手（`MMT_HookHand`） | — | — | — | — | 1 |
| 玻璃眼（`MMT_GlassEye`） | — | — | — | — | 1 |
| 人造鼻（`MMT_ArtificialNose`） | 300 | — | — | — | 1 |
| 简易的假手（`MMT_SimpleProstheticHand`） | 800 | — | — | — | 1 |
| 简易的假足（`MMT_SimpleProstheticFoot`） | 880 | — | — | — | 1 |
| 人造骨盆（`MMT_ArtificialPelvis`） | 1100 | — | — | — | 1 |
| 仿生手（`MMT_BionicHand`） | — | — | — | — | 1 |
| 仿生足（`MMT_BionicFoot`） | — | — | — | — | 1 |
| 仿生脖颈（`MMT_BionicNeck`） | — | — | — | — | 1 |
| 脑激励器（`MMT_BrainStimulator`） | — | — | — | — | 1 |
| AI芯片（`MMT_AIChip`） | — | — | — | — | 1 |
| 合成肺（`MMT_BionicLung`） | — | — | — | — | 1 |
| 合成肝（`MMT_BionicLiver`） | 1300 | — | — | — | 1 |
| 合成肾（`MMT_BionicKidney`） | 1300 | — | — | — | 1 |
| 肌肉刺激器（`MMT_MuscleStimulator`） | — | — | — | — | 2 |
| 生命维持系统（`MMT_lifesupport`） | 180 | — | — | — | 5 |

#### 高级仿生（BionicsAdv）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 医疗肋骨（`MMT_MedicalRib`） | — | — | — | — | 1 |
| 迷幻肋骨（`MMT_DruggedRib`） | — | — | — | — | 1 |
| 肾上腺素肋骨（`MMT_AdrenalineRib`） | — | — | — | — | 1 |
| 止痛肋骨（`MMT_PainkillerRib`） | — | — | — | — | 1 |
| 加热肋骨（`MMT_HeaterRib`） | — | — | — | — | 1 |
| 冷却肋骨（`MMT_CoolerRib`） | — | — | — | — | 1 |
| 止血肋骨（`MMT_CoagulatorRib`） | — | — | — | — | 1 |
| 神经机械仿生耳（`MMT_GTEar`） | 3700 | — | — | — | 1 |
| 神经机械心（`MMT_GTHeart`） | 4250 | — | — | — | 1 |
| 神经机械肺（`MMT_GTLung`） | 3200 | — | — | — | 1 |
| 神经机械肾（`MMT_GTKidney`） | 3000 | — | — | — | 1 |
| 神经机械肝（`MMT_GTLiver`） | 3000 | — | — | — | 1 |
| 神经机械胃（`MMT_GTstomach`） | 2900 | — | — | — | 1 |
| 神经机械仿生手（`MMT_GTHand`） | 3700 | — | — | — | 1 |
| 神经机械仿生足（`MMT_GTFoot`） | 3700 | — | — | — | 1 |
| 神经机械颌（`MMT_GTDentures`） | 2750 | — | — | — | 1 |
| 神经机械仿生脊柱（`MMT_GTSpine`） | 6400 | — | — | — | 1 |
| 神经机械仿生脖颈（`MMT_GTNeck`） | 3800 | — | — | — | 1 |
| 外骨骼（`MMT_ExoskeletonSuit`） | 9000 | — | — | — | 1 |
| 防护性外骨骼（`MMT_ProtectiveExoskeleton`） | 10000 | — | — | — | 1 |
| 战术角膜植入体（`MMT_TacticalCorneaImplant`） | — | — | — | — | 1 |
| 辅助AI（建造）（`MMT_ConstructorCore`） | — | — | — | — | 1 |
| 辅助AI（农业）（`MMT_FarmerCore`） | — | — | — | — | 1 |
| 辅助AI（采矿）（`MMT_MinerCore`） | — | — | — | — | 1 |
| 辅助AI（医疗）（`MMT_DoctorCore`） | — | — | — | — | 1 |
| 辅助AI（外交）（`MMT_DiplomatCore`） | — | — | — | — | 1 |
| 辅助AI（射击）（`MMT_AuxiliaryAI_Sharpshooter`） | — | — | — | — | 1 |
| 辅助AI（作战）（`MMT_AuxiliaryAI_Commando`） | — | — | — | — | 1 |
| 辅助AI（格斗）（`MMT_AuxiliaryAI_Brawler`） | — | — | — | — | 1 |
| 合成皮肤（`MMT_SyntheticSkin`） | 150 | — | — | — | 0 |
| 神经肌肉结构（`MMT_NeuromuscularFramework`） | — | — | — | — | 0 |
| 神经治疗结构（`MMT_NeurocureFramework`） | — | — | — | — | 0 |
| 飞螳镰刃（`MMT_ScytherBlade`） | — | — | — | — | 0 |
| 年龄加速器（`MMT_SenexiumImplant`） | 400 | — | — | — | 1 |
| 机体优化纳米机器人（`MMT_OrganicOptimizingNanobots`） | — | — | — | — | 1 |
| 超感优化纳米机器人（`MMT_InstinctOptimizingNanobots`） | — | — | — | — | 0 |

#### CONN 仿生（BionicsCONN）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 机械眼（`MMT_CONN_IndependentEye`） | 6000 | — | — | — | 1 |
| 能量连接（`MMT_CONN_EnergyLink`） | 6700 | — | — | — | 1 |
| 外刃（`MMT_CONN_OuterBlade`） | 6000 | — | — | — | 1 |
| 动力臂（`MMT_CONN_PowerArms`） | 5350 | — | — | — | 1 |
| 暗影行者（`MMT_CONN_Shadowrunner`） | 5600 | — | — | — | 1 |
| 外骨骼装甲（`MMT_CONN_Exoskeleton`） | 11200 | — | — | — | 1 |
| 皮肤装甲板（`MMT_CONN_Dermalplating`） | 7500 | — | — | — | 1 |
| 狮心（`MMT_CONN_Lionheart`） | 9000 | — | — | — | 1 |
| 变焦眼（`MMT_CONN_Zoom`） | 7000 | — | — | — | 1 |
| 能量拳（`MMT_CONN_EnergyFist`） | 5500 | — | — | — | 1 |
| 弹性跟腱（`MMT_CONN_ElasticAchilles`） | 7600 | — | — | — | 1 |
| 独眼巨人目镜（`MMT_CONN_CyclopsVisor`） | 7600 | — | — | — | 1 |
| 激光侦测器（`MMT_CONN_LaserDetection`） | 6250 | — | — | — | 1 |
| 传感耳侧（`MMT_CONN_SensoricEarside`） | 5500 | — | — | — | 1 |
| 狂战士芯片（`MMT_CONN_BerserkerChip`） | 5500 | — | — | — | 1 |
| 心灵扭曲（`MMT_CONN_BodyPartsArtificialtwist`） | 600 | — | — | — | 1 |
| 缪斯芯片（`MMT_CONN_MuseChip`） | 6600 | — | — | — | 1 |
| 神经增强器（`MMT_CONN_NeuroEnhancer`） | 7000 | — | — | — | 1 |
| 战斗AI（`MMT_CONN_CombatAI`） | 8000 | — | — | — | 1 |
| 高能肺（`MMT_CONN_EnergeticLung`） | 6000 | — | — | — | 1 |
| 高能肾（`MMT_CONN_EnergeticKidney`） | 6700 | — | — | — | 1 |
| 能量核心（`MMT_CONN_EnergyCore`） | 5500 | — | — | — | 1 |
| 自主病毒杀手（`MMT_CONN_Autonomicviruskiller`） | 6400 | — | — | — | 1 |
| 奥鲁姆（`MMT_CONN_Aurum`） | 8000 | — | — | — | 1 |
| 黑珍珠（`MMT_CONN_BlackPearl`） | 6500 | — | — | — | 1 |
| 悬浮脚（`MMT_CONN_HoverFeet`） | 7400 | — | — | — | 1 |
| 绿拇指（`MMT_CONN_Greenthumb`） | 3500 | — | — | — | 1 |
| 压缩颚（`MMT_CONN_CompressionJaw`） | 3200 | — | — | — | 1 |
| 压缩胃（`MMT_CONN_CompressionStomach`） | 3500 | — | — | — | 1 |
| 手电（`MMT_CONN_FlashLight`） | 3250 | — | — | — | 1 |
| 肌肉线（`MMT_CONN_Musclewire`） | 6750 | — | — | — | 1 |

#### 充能护盾（ChargeShield）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 充能护盾腰带（`MMT_ChargeShieldPack`） | 1500 | — | — | MMT_MakeChargeShieldPack | 0 |

#### 药品（Drugs）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 草药茶（`MMT_HerbalTea`） | 36 | — | — | MMT_MakeHerbalTea | 0 |
| 活性炭丸（`MMT_ActivatedCarbon`） | 16 | — | — | MMT_MakeActivatedCarbon | 0 |
| 狂暴草（`MMT_BerserkHerb`） | 13 | — | — | MMT_MakeBerserkHerb | 0 |
| 美沙酮（`MMT_Methadone`） | 16 | — | — | MMT_MakeMethadone | 0 |
| 止痛药（`MMT_Ibuprofen`） | 11 | — | — | MMT_MakeIbuprofen、MMT_MakeIbuprofenBulk | 0 |

#### 天然器官（NaturalOrgans）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 眼球（`Eye`） | 420 | — | — | — | 1 |
| 耳朵（`Ear`） | 410 | — | — | — | 1 |
| 鼻子（`Nose`） | 504 | — | — | — | 1 |
| 胃（`Stomach`） | 500 | — | — | — | 1 |

#### 神经调制（Neuromod）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 神经调节增强器（`MMT_CONN_NeuromodBooster`） | 1500 | 1 | — | — | 0 |
| 洗脑（`MMT_CONN_Brainwash`） | 1600 | 1 | — | — | 0 |
| 激情赋予器（`MMT_CONN_PassionGiver`） | 1100 | 1 | — | — | 0 |
| 特性赋予器（`MMT_CONN_TraitGiver`） | 1400 | 1 | — | — | 0 |
| 想法清除器（`MMT_NeurotrainersClearer`） | 1100 | 1 | — | — | 0 |
| 重塑器（`MMT_CONN_Reconditioner`） | 1100 | 1 | — | — | 0 |

#### 灵能护盾（PsychicShield）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 灵能护盾腰带（`MMT_PsychicShieldBelt`） | 2000 | — | — | MMT_MakePsychicShieldBelt | 0 |

#### 护盾（Shield）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 部署式护盾发生器（`MMT_DeployableShield`） | 600 | 1 | — | MMT_MakeDeployableShield | 0 |

#### 状态备份（StateBackup）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 生命备份核心（`MMT_StateBackupCore`） | 8000 | — | — | — | 1 |

#### 炮塔弹药（TurretAmmo）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 120mm 炮弹（破甲）（`MMT_Ammo_120mmCannonShell_HEAT`） | — | 75 | — | MMT_MakeAmmo_120mmCannonShell_HEAT | 0 |
| 130mm 制导火箭弹（高爆弹）（`MMT_Ammo_130mmRocketMissile`） | 180 | 75 | — | MMT_MakeAmmo_130mmRocketMissile | 0 |
| 155mm 炮弹（高爆弹）（`MMT_Ammo_155mmHowitzerShell_HE`） | — | 75 | — | MMT_MakeAmmo_155mmHowitzerShell_HE | 0 |
| 20x102mm 弹药（全金属被甲弹）（`MMT_Ammo_20x102mm_FMJ`） | 5 | 75 | — | MMT_MakeAmmo_20x102mm_FMJ | 0 |
| 30x173mm 弹药（全金属被甲弹）（`MMT_Ammo_30x173mm_FMJ`） | 6.5 | 75 | — | MMT_MakeAmmo_30x173mm_FMJ | 0 |
| 30x29mm 榴弹（高爆弹）（`MMT_Ammo_30x29mmGrenade_HE`） | 2.75 | 75 | — | MMT_MakeAmmo_30x29mmGrenade_HE | 0 |
| .50 机枪弹（全金属被甲弹）（`MMT_Ammo_50BMG_FMJ`） | 0.5 | 75 | — | MMT_MakeAmmo_50BMG_FMJ | 0 |
| 7.62x51mm 北约标准弹（全金属被甲弹）（`MMT_Ammo_762x51mmNATO_FMJ`） | 0.11 | 75 | — | MMT_MakeAmmo_762x51mmNATO_FMJ | 0 |
| 弩炮箭（`MMT_Ammo_BalistaBolt`） | 1 | 75 | — | MMT_MakeAmmo_BalistaBolt | 0 |
| 石球（`MMT_Ammo_StoneBall`） | 2 | 75 | — | MMT_MakeAmmo_StoneBall | 0 |
| SPG-9 Rocket (HE)（`MMT_Ammo_SPG9Rocket_Frag`） | 61.58 | 75 | — | MMT_MakeAmmo_SPG9Rocket_Frag | 0 |
| 14.5x114mm 机枪弹（全金属被甲弹）（`MMT_Ammo_145x114mm_FMJ`） | 0.4 | 75 | — | MMT_MakeAmmo_145x114mm_FMJ | 0 |

#### 炮塔箱（TurretCrates）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| AGS-30组件箱（`MMT_AGCturret_Crate`） | 2700 | 1 | — | MMT_BuildAGCturret_Crate | 1 |
| GAU-8复仇者火炮组件箱（`MMT_Avenger_Crate`） | 4400 | 1 | — | MMT_BuildAvenger_Crate | 1 |
| 弩炮组件箱（`MMT_Balista_Crate`） | 400 | 1 | — | MMT_BuildBalista_Crate | 1 |
| 120mm坦克炮组件箱（`MMT_CannonTurret_Crate`） | 4100 | 1 | — | MMT_BuildCannonTurret_Crate | 1 |
| 投石机组件箱（`MMT_Catapult_Crate`） | 800 | 1 | — | MMT_BuildCatapult_Crate | 1 |
| 155mm榴弹炮组件箱（`MMT_HowitzerTurret_Crate`） | 4800 | 1 | — | MMT_BuildHowitzerTurret_Crate | 1 |
| FGM-148“标枪”反坦克导弹组件箱（`MMT_LightMissileSystem_Crate`） | 3400 | 1 | — | MMT_BuildLightMissileSystem_Crate | 1 |
| M2 .50勃朗宁机枪组件箱（`MMT_MGTurret_Crate`） | 2900 | 1 | — | MMT_BuildMGTurret_Crate | 1 |
| 14.5mm KPV重机枪组件箱（`MMT_KPVTurret_Crate`） | 3100 | 1 | — | MMT_BuildKPVTurret_Crate | 1 |
| 130mm中型导弹系统组件箱（`MMT_MediumMissileSystem_Crate`） | 4400 | 1 | — | MMT_BuildMediumMissileSystem_Crate | 1 |
| M134速射机枪组件箱（`MMT_MinigunTurret_Crate`） | 3000 | 1 | — | MMT_BuildMinigunTurret_Crate | 1 |
| 35mm哨戒炮塔组件箱（`MMT_NavalGunTurret_Crate`） | 4000 | 1 | — | MMT_BuildNavalGunTurret_Crate | 1 |
| 20x102mm厄利孔机炮组件箱（`MMT_Oerlikonautocannon_Crate`） | 3400 | 1 | — | MMT_BuildOerlikonautocannon_Crate | 1 |
| 130mm重型导弹系统组件箱（`MMT_RocketLauncher_Crate`） | 5600 | 1 | — | MMT_BuildRocketLauncher_Crate | 1 |
| 自动反坦克火箭发射系统组件箱（`MMT_SentryRocketLauncher_Crate`） | 3700 | 1 | — | MMT_BuildSentryRocketLauncher_Crate | 1 |
| BGM-71“陶式”反坦克导弹组件箱（`MMT_TOW_Crate`） | 3200 | 1 | — | MMT_BuildTOW_Crate | 1 |
| 20mm火神炮组件箱（`MMT_VulcanCannon_Crate`） | 3900 | 1 | — | MMT_BuildVulcanCannon_Crate | 1 |

### 1.3 作物

#### 栽培作物（Plants_Cultivated）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 亚麻花（`MMT_PlantFlax`） | — | — | — | — | 0 |
| 野玫瑰（`MMT_PlantWildRose`） | — | — | — | — | 0 |
| 金丝桃（`MMT_PlantHypericum`） | — | — | — | — | 0 |
| 薄荷（`MMT_PlantMint`） | — | — | — | — | 0 |
| 橡胶树（`MMT_PlantHevea`） | — | — | — | — | 0 |
| 芦荟（`MMT_PlantAloe`） | — | — | — | — | 0 |
| 中性花（`MMT_PlantNeutroFlower`） | — | — | — | — | 0 |
| 红木树（`MMT_PlantRedwood`） | — | — | — | — | 0 |

### 1.4 其它

#### Skyfallers_Teleport（Skyfallers_Teleport）

| 物品 | 价值 | 堆叠 | 材料 | 产出配方 | 被引用 |
| --- | --- | --- | --- | --- | --- |
| 折跃载体（`MMT_TeleportShuttle`） | — | — | — | — | 0 |
| 折跃光柱（降临）（`MMT_TeleportShuttleIncoming`） | — | — | — | — | 0 |
| 折跃光柱（离去）（`MMT_TeleportShuttleLeaving`） | — | — | — | — | 0 |

## 2. HSK 有而 MMT 未实现的资源/材料

共 114 件，按 HSK 源文件分组。中文名取自 HSK 汉化（Core_SK DefInjected），无翻译时保留英文 label。与原版同名的 def（HSK 对原版的改写）不计入。

### 2.1 Items_CampingStuff.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| UnfinishedTent | `UnfinishedTent` | Unfinished | — |
| DeployableTent | `DeployableTent` | Tents | 300 |
| DeployableTentMedium | `DeployableTentMedium` | Tents | 550 |
| DeployableTentBig | `DeployableTentBig` | Tents | 750 |

### 2.2 Items_Crate.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| M240B_Crate | `M240B_Crate` | WeaponCrate | 2400 |
| AGSturret_Crate | `AGSturret_Crate` | WeaponCrate | 2500 |
| Mortar_Crate | `Mortar_Crate` | WeaponCrate | 2200 |
| FlakTurret_Crate | `FlakTurret_Crate` | WeaponCrate | 3800 |
| SentryTurret_Crate | `SentryTurret_Crate` | WeaponCrate | 2000 |
| HeavyTurret_Crate | `HeavyTurret_Crate` | WeaponCrate | 3500 |
| SentryBlasterTurret_Crate | `SentryBlasterTurret_Crate` | WeaponCrate | 3600 |
| LaserTurret_Crate | `LaserTurret_Crate` | WeaponCrate | 3600 |

### 2.3 Items_Exotic.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| LivingFrost | `LivingFrost` | BodyParts | 1500 |
| LivingFire | `LivingFire` | BodyParts | 1500 |

### 2.4 Items_Minified.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| MinifiedTurret_Sniper | `MinifiedTurret_Sniper` | — | — |
| MinifiedTurret_NavalGunTurretGun | `MinifiedTurret_NavalGunTurretGun` | — | — |
| MinifiedTurret_FFautocannon | `MinifiedTurret_FFautocannon` | — | — |
| MinifiedTurret_M2Browning | `MinifiedTurret_M2Browning` | — | — |
| MinifiedTurret_ChainGun | `MinifiedTurret_ChainGun` | — | — |
| MinifiedTurret_Avenger | `MinifiedTurret_Avenger` | — | — |
| MinifiedTurret_VulcanCannon | `MinifiedTurret_VulcanCannon` | — | — |
| MinifiedTurret_Balista | `MinifiedTurret_Balista` | — | — |
| MinifiedTurret_Catapult | `MinifiedTurret_Catapult` | — | — |
| MinifiedTurret_SentryRocketLauncher | `MinifiedTurret_SentryRocketLauncher` | — | — |
| MinifiedTurret_Cannon | `MinifiedTurret_Cannon` | — | — |
| MinifiedHowitzer_Cannon | `MinifiedHowitzer_Cannon` | — | — |
| MinifiedLightMissileSystem | `MinifiedLightMissileSystem` | — | — |
| MinifiedTurret_Minigun | `MinifiedTurret_Minigun` | — | — |
| MinifiedTurret_TOWLauncher | `MinifiedTurret_TOWLauncher` | — | — |
| MinifiedMediumMissileSystem | `MinifiedMediumMissileSystem` | — | — |
| Minified_RLGun | `Minified_RLGun` | — | — |
| MinifiedTurret_AGLGrenadeLauncher | `MinifiedTurret_AGLGrenadeLauncher` | — | — |

### 2.5 Items_Resource_Alloys_Medieval.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| GoldBar | `GoldBar` | HCMBar/PRSBar/HeavyBar | 35 |
| SilverBar | `SilverBar` | PRSBar/HeavyBar | 5 |

### 2.6 Items_Resource_Chemical.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| SyntheticAmmonia | `SyntheticAmmonia` | Chemical | 15 |

### 2.7 Items_Resource_Cloth.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| WoolDromedary | `WoolDromedary` | BTextiles | 3.5 |

### 2.8 Items_Resource_Crops.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Silage | `Silage` | Animalfood | 5 |

### 2.9 Items_Resource_Crops_CookingSupplies.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Rawwheat | `Rawwheat` | CookingSupplies | 2.0 |
| Rawcocoa | `Rawcocoa` | CookingSupplies | 2.0 |
| Rawsugarcane | `Rawsugarcane` | CookingSupplies | 2.0 |
| RawTobacco | `RawTobacco` | CookingSupplies | 1.8 |
| RawCoffee | `RawCoffee` | CookingSupplies | 1.2 |
| RawTea | `RawTea` | CookingSupplies | 2.0 |

### 2.10 Items_Resource_Crops_Eggs.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| EggFRaptorUnfertilized | `EggFRaptorUnfertilized` | EggsUnfertilized | 300.0 |
| EggFRaptorFertilized | `EggFRaptorFertilized` | EggsFertilized | 600.0 |
| EggFenixUnfertilized | `EggFenixUnfertilized` | EggsUnfertilized | 1000.0 |
| EggFenixFertilized | `EggFenixFertilized` | EggsFertilized | 4000.0 |
| EggGreenWyvernUnfertilized | `EggGreenWyvernUnfertilized` | EggsUnfertilized | 700.0 |
| EggGreenWyvernFertilized | `EggGreenWyvernFertilized` | EggsFertilized | 3000.0 |
| EggBehirUnfertilized | `EggBehirUnfertilized` | EggsUnfertilized | 1500.0 |
| EggBehirFertilized | `EggBehirFertilized` | EggsFertilized | 4500.0 |
| EggLacosdileUnfertilized | `EggLacosdileUnfertilized` | EggsUnfertilized | 12.0 |
| EggLacosdileFertilized | `EggLacosdileFertilized` | EggsFertilized | 15.0 |
| EggNightlingUnfertilized | `EggNightlingUnfertilized` | EggsUnfertilized | 60.0 |
| EggNightlingFertilized | `EggNightlingFertilized` | EggsFertilized | 100.0 |
| EggPtarmiganUnfertilized | `EggPtarmiganUnfertilized` | EggsUnfertilized | 7.0 |
| EggPtarmiganFertilized | `EggPtarmiganFertilized` | EggsFertilized | 12.0 |

### 2.11 Items_Resource_Crops_Fruits.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| RawWatermelon | `RawWatermelon` | FruitFoodRaw | 6 |
| Rawapple | `Rawapple` | FruitFoodRaw | 5 |
| Rawbanana | `Rawbanana` | FruitFoodRaw | 6 |
| Rawgrape | `Rawgrape` | FruitFoodRaw | 7 |
| Raworange | `Raworange` | FruitFoodRaw | 7 |
| Rawpeach | `Rawpeach` | FruitFoodRaw | 6 |

### 2.12 Items_Resource_Crops_Fungi.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| RawShimmershroom | `RawShimmershroom` | FungusPlantRaw | 3.0 |
| RawGlowbulb | `RawGlowbulb` | FungusPlantRaw | 2.0 |
| GleamcapStem | `GleamcapStem` | Medicine | 5.5 |
| Rawmushroom | `Rawmushroom` | FungusPlantRaw | 5 |

### 2.13 Items_Resource_Crops_Ingridients.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Sugar | `Sugar` | CookingSupplies | 3 |
| Flour | `Flour` | CookingSupplies | 3 |
| SoyMilk | `SoyMilk` | CookingSupplies | 2.1 |
| coconutmilk | `coconutmilk` | CookingSupplies | 2.3 |
| Tofu | `Tofu` | MeatSubRaw | 14 |
| RoastedCoffeeBeans | `RoastedCoffeeBeans` | CookingSupplies | 3.3 |
| GroundCoffee | `GroundCoffee` | CookingSupplies | 3.8 |

### 2.14 Items_Resource_Crops_Luxury.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Rawonion | `Rawonion` | LuxuryStuffs | 2 |

### 2.15 Items_Resource_Crops_Vegetables.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Rawbean | `Rawbean` | BasicPlantFoodRaw | 1.5 |
| RawTomatoes | `RawTomatoes` | ExtraPlantFoodRaw | 1.9 |
| RawCarrots | `RawCarrots` | ExtraPlantFoodRaw | 2.1 |
| RawGiantLeaf | `RawGiantLeaf` | BasicPlantFoodRaw | 2.0 |
| RawAlgae | `RawAlgae` | BasicPlantFoodRaw | 4.5 |

### 2.16 Items_Resource_Extracted.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Glowstone | `Glowstone` | Extracted | 25 |
| Coldstone | `Coldstone` | Extracted | 40 |
| Peat | `Peat` | Extracted | 1.5 |

### 2.17 Items_Resource_Honey.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Honey | `Honey` | AnimalProductRaw | 13 |

### 2.18 Items_Resource_Materials.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| MendingKit | `MendingKit` | MiscManufactured | 30 |

### 2.19 Items_Resource_Medicine.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Synchronite | `Synchronite` | MedicalMaterials | 3 |

### 2.20 Items_Resource_Parts.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| 高级零部件 | `ComponentAdvanced` | Parts | 100 |
| Pipe | `Pipe` | Parts | 25 |

### 2.21 Items_Resource_Stuff_Leather.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| Leather_Nightling | `Leather_Nightling` | — | 2.5 |
| Leather_Boomanimal | `Leather_Boomanimal` | — | 3.5 |
| Leather_FireDragonskin | `Leather_FireDragonskin` | — | 10.5 |
| Leather_KirinHide | `Leather_KirinHide` | — | 10 |
| Leather_FeatheredRaptorLeather | `Leather_FeatheredRaptorLeather` | — | 5 |
| Leather_BarghestFur | `Leather_BarghestFur` | — | 14 |

### 2.22 Items_Resource_Terraforming.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| RottedMeat | `RottedMeat` | Spoilage | — |
| RottedMush | `RottedMush` | Spoilage | — |

### 2.23 Items_SatteliteControllers.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| IonCannonControllerCore | `IonCannonControllerCore` | HighTechItems | 10000 |
| WeatherControllerCore | `WeatherControllerCore` | HighTechItems | 5000 |
| Bullet_FakeSkywrathControllerBullet | `Bullet_FakeSkywrathControllerBullet` | — | — |
| Artillery_SkywrathController | `Artillery_SkywrathController` | — | — |

### 2.24 Items_Tools.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| TFJ_Tool_Woodcutting_Handaxe | `TFJ_Tool_Woodcutting_Handaxe` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Mining_Pickaxe | `TFJ_Tool_Mining_Pickaxe` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Building_Hammer | `TFJ_Tool_Building_Hammer` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Sickle | `TFJ_Tool_Sickle` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Broom | `TFJ_Tool_Broom` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Hoe | `TFJ_Tool_Hoe` | SurvivalToolsNeolithic | — |
| TFJ_Tool_Multitool | `TFJ_Tool_Multitool` | SurvivalToolsSpacer | 3000 |
| TFJ_Tool_Paxe | `TFJ_Tool_Paxe` | SurvivalToolsNeolithic | — |

### 2.25 Items_Unfinished.xml

| 物品 | defName | 类别 | 价值 |
| --- | --- | --- | --- |
| UnfinishedTurretMedieval | `UnfinishedTurretMedieval` | — | — |
| UnfinishedTurretHiTech | `UnfinishedTurretHiTech` | — | — |
| UnfinishedBalls | `UnfinishedBalls` | — | — |
| UnfinishedResearch | `UnfinishedResearch` | — | — |
| UnfinishedMetallicApparel | `UnfinishedMetallicApparel` | — | — |
| UnfinishedHealthItemCybernetic | `UnfinishedHealthItemCybernetic` | — | — |

