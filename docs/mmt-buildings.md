# MMT 建筑手册（造价与配方）

- 由 `tools/build_mmt_buildings_doc.py` 自动生成（改 Def 后重跑）；MMT 数据源 `1.7/Defs`，HSK 数据源 `Hardcore-SK/Mods/Core_SK`（只读参考）。
- 字段沿 ParentName 继承链合并；`[类别]×n` 为 stuff 材料（costStuffCount）；配方数值对比见 [mmt-vs-hsk-resource-chains.md](mmt-vs-hsk-resource-chains.md)，武器类建筑数值见 [mmt-weapons.md](mmt-weapons.md)。
- 第 2 部分为 HSK Core_SK 中 MMT 未实现的生产建筑（有配方列表或属生产分类），可作后续补全清单；家具/装饰/地面等无关类别不列。

## 1. MMT 建筑总表

共 86 座建筑（不含抽象基类），按链分组；矿脉类建筑的产量对比见对比文档第 2 节。

### 深钻井（AdvancedRocks）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 镍矿矿藏（`MMT_MineableNickelOre`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 镍矿） |
| 钛铁矿（`MMT_MineableIlmenite`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 钛铁矿） |
| 黑钨矿（`MMT_MineableWolframite`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 黑钨矿） |

### 农业（Agriculture）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 压实的盐（`MMT_MineableSalt`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 盐） |
| 挖盐点（`MMT_SaltMine`） | (1,1) | — | — | 2500 | — | 1 |  |

**挖盐点（`MMT_SaltMine`） 的配方：**

- `MMT_MineSalt`：— -> 盐×5 ｜ 工作量=1800

### 自动处理（AutoProcessor）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 矿料进料口（`MMT_Hopper`） | — | 钢×15 + 机械组件×2 | — | 300 | 自动化矿料处理 | 0 |  |
| 自动研磨机（`MMT_AutoGrinder`） | (2,2) | 钢×80 + 机械组件×6 + 工业零件×4 | 500W | 4000 | 自动化矿料处理 | 0 | CompProperties_ModuleSlots、CompProperties_AutoProcessor |
| 自动筛子（`MMT_AutoSieve`） | (2,2) | 机械组件×4 + 工业零件×4 + [金属]×80 | 400W | 3500 | 自动化矿料处理 | 0 | CompProperties_ModuleSlots、CompProperties_SieveMesh、CompProperties_AutoProcessor |

### 仿生（Bionics）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 原始假肢工作台（`MMT_PrimitiveProstheticsTable`） | (3,1) | [石质/木质]×80 | — | 3700 | 义肢制造 I | 0 |  |
| 假肢工作台（`MMT_BasicProstheticsTable`） | (2,1) | 工业零件×12 + 机械组件×8 + [金属/木质]×120 | 350W | 3700 | 义肢制造 III | 0 |  |
| 人造器官组装台（`MMT_SyntheticsTable`） | (3,1) | 塑料×65 + 人造纤维×20 + 集成电路×6 + 高级零部件×5 + [金属/木质]×135 | 850W | 5500 | 义肢制造 VI | 2 |  |
| 仿生体工作台（`MMT_BionicsTable`） | (3,1) | 人造纤维×25 + 玻璃块×25 + 集成电路×15 + 高级零部件×5 + [金属/木质]×160 | 950W | 8500 | Bionics | 0 |  |
| 神经机械体工作台（`MMT_UpgradingStation`） | (3,1) | 人造纤维×45 + 微芯片集成电路×5 + 高级零部件×7 + 先进机械组件×10 + [金属/木质]×250 | 1350W | 9000 | 义肢制造 IX | 0 |  |
| 器官培养舱（`MMT_OrganVat`） | (2,2) | 合成橡胶×35 + 人造纤维×25 + 玻璃块×25 + 电子元件×5 | 700W | 6000 | 义肢制造 IV | 0 | CompProperties_OrganVat |

**人造器官组装台（`MMT_SyntheticsTable`） 的配方：**

- `MMT_MakeArtificialBone`：生物质(5) + 高级零部件(4) + 石蜡(2) + 塑料(10) -> 人造骨头×1 ｜ 工作量=200 ｜ 研究:义肢制造 VI ｜ 技能:制作≥8
- `MMT_MakeArtificialBoneBulk`：生物质(20) + 高级零部件(16) + 石蜡(8) + 塑料(40) -> 人造骨头×4 ｜ 工作量=720 ｜ 研究:义肢制造 VI ｜ 技能:制作≥8

### 陶瓷（Ceramics）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 粘土矿脉（`MMT_MineableSoftClay`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 软粘土） |

### 充能（ChargingStation）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 充能台（`MMT_ChargingStation`） | (1,1) | 钢×60 + 工业零件×4 + 电线×10 | 1500W | 3000 | 充能技术 | 0 | CompProperties_ChargingStation、CompProperties_ModuleSlots |

### 采石（ChunkDrill）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手动采石机（`MMT_ChunkDrill`） | (2,2) | 原木×40 + 钢×60 | — | 2500 | Stonecutting | 0 | CompProperties_ChunkDrill |
| 电动采石机（`MMT_ElectricChunkDrill`） | (2,2) | 钢×100 + 机械组件×2 + 工业零件×2 | 400W | 4000 | 矿粉筛选 | 0 | CompProperties_ChunkDrill |
| 自动采石机（`MMT_AutoChunkDrill`） | (2,2) | 钢×150 + 机械组件×4 + 工业零件×4 + 电子元件×2 | 800W | 6000 | 自动化矿料处理 | 0 | CompProperties_ModuleSlots、CompProperties_ChunkDrill |

### 煤（Coal）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 煤矿（`MMT_MineableCoal`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 原煤） |

### 混凝土（Concrete）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手动混凝土搅拌器（`MMT_ConcreteMixer`） | (2,1) | 工业零件×5 + [金属]×135 | — | 1600 | 混凝土技术 | 2 |  |
| 电动混凝土搅拌机（`MMT_EConcreteMixer`） | (2,1) | 合成橡胶×45 + 工业零件×10 + 电子元件×5 + 机械组件×5 + [金属]×115 | 500W | 5000 | 混凝土 II | 2 |  |

**手动混凝土搅拌器（`MMT_ConcreteMixer`） 的配方：**

- `MMT_MixConcrete_Hand`：沙(45) + 碎石(30) -> 混凝土×15 ｜ 工作量=1200 ｜ 研究:混凝土技术 ｜ 技能:制作≥3
- `MMT_MakeDirt_Hand`：沙(20) + 肥料(10) -> 土堆×30 ｜ 工作量=3000 ｜ 研究:混凝土技术

**电动混凝土搅拌机（`MMT_EConcreteMixer`） 的配方：**

- `MMT_MixConcrete_Electric`：沙(45) + 碎石(30) -> 混凝土×15 ｜ 工作量=600 ｜ 研究:混凝土 II ｜ 技能:制作≥6
- `MMT_MakeDirt_Electric`：沙(20) + 肥料(10) -> 土堆×30 ｜ 工作量=1500 ｜ 研究:混凝土 II

### 电动金属（Electric）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 电弧炉（`MMT_ElectricArcFurnace`） | (3,3) | 钢筋混凝土×50 + 工业零件×15 + 电子元件×6 + [金属]×180 | 2500W | 4500 | 电弧冶炼 | 27 |  |

**电弧炉（`MMT_ElectricArcFurnace`） 的配方：**

- `MMT_SmeltCopperBar_Electric`：铜矿(20) -> 铜×20 ｜ 工作量=500 ｜ 研究:电弧冶炼 ｜ 技能:制作≥4
- `MMT_SmeltTinBar_Electric`：锡矿(20) -> 锡×20 ｜ 工作量=500 ｜ 研究:电弧冶炼 ｜ 技能:制作≥4
- `MMT_MakeBronze_Electric`：铜(20) + 锡(10) -> 青铜×30 ｜ 工作量=700 ｜ 研究:电弧冶炼 ｜ 技能:制作≥4
- `MMT_SmeltSteel_Electric`：铁矿石(15) + 木炭/原煤(7) -> 钢×20 ｜ 工作量=900 ｜ 研究:电弧冶炼 ｜ 技能:制作≥6
- `MMT_SmeltSilver_Electric`：白银(14) -> 白银×10 ｜ 工作量=500 ｜ 研究:电弧冶炼 ｜ 技能:制作≥5
- `MMT_SmeltGold_Electric`：金矿(12) -> 黄金×15 ｜ 工作量=500 ｜ 研究:电弧冶炼 ｜ 技能:制作≥6
- `MMT_ExtractSilicon_Electric`：沙(50) -> 硅×5 ｜ 工作量=800 ｜ 研究:电弧冶炼 ｜ 技能:制作≥5
- `MMT_MakeSilicon_Bulk`：沙(200) -> 硅×20 ｜ 工作量=1800 ｜ 研究:电弧冶炼 ｜ 技能:制作≥5
- `MMT_SmeltLeadBar_Electric`：铅矾矿(20) -> 铅合金×20 ｜ 工作量=1000 ｜ 研究:电弧冶炼 ｜ 技能:制作≥6
- `MMT_SmeltAluminiumBar_Electric`：铝土矿(20) -> 铝合金×20 ｜ 工作量=1200 ｜ 研究:电弧冶炼 ｜ 技能:制作≥7
- `MMT_MakeFerrosiliconAlloy`：钢(15) + 硅(10) + 铝合金(5) -> 斯特合金×30 ｜ 工作量=1700 ｜ 研究:电弧冶炼 ｜ 技能:制作≥8
- `MMT_MakeMetalCan`：[MMT_MetallicBar](8) -> 金属罐×15 ｜ 工作量=800 ｜ 研究:电弧冶炼 ｜ 技能:制作≥2
- `MMT_MakeCeramics`：软粘土(20) + 玻璃生料(5) -> 陶瓷×10 ｜ 工作量=1200 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥8
- `MMT_MakeClayBricks`：软粘土(15) -> 粘土砖×15 ｜ 工作量=800 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥6
- `MMT_SmeltNickelBar`：镍矿(15) -> 镍合金×15 ｜ 工作量=1500 ｜ 研究:高级金属精炼 ｜ 技能:制作≥13
- `MMT_ExtractCobalt`：镍矿(30) -> 钴合金×15 ｜ 工作量=1300 ｜ 研究:高级金属精炼 ｜ 技能:制作≥14
- `MMT_ExtractChromium`：铁矿石(25) -> 铬合金×10 ｜ 工作量=1200 ｜ 研究:高级金属精炼 ｜ 技能:制作≥11
- `MMT_SmeltTungsten`：黑钨矿(15) -> 钨合金×15 ｜ 工作量=3000 ｜ 研究:高级合金 ｜ 技能:制作≥17
- `MMT_SmeltTitanium`：钛铁矿(15) -> 钛合金×15 ｜ 工作量=3300 ｜ 研究:高级合金 ｜ 技能:制作≥16
- `MMT_MakeStellite`：钴合金(5) + 铬合金(13) + 钨合金(2) -> 司太立合金（钨铬钴合金）×20 ｜ 工作量=2100 ｜ 研究:高级合金 ｜ 技能:制作≥17
- `MMT_MakeNitinol`：钛合金(4) + 镍合金(11) -> 镍钛合金×15 ｜ 工作量=2600 ｜ 研究:高级合金 ｜ 技能:制作≥16
- `MMT_MakePobedite`：钨合金(3) + 碳纤维锭(3) + 钴合金(3) -> 钨钴合金×10 ｜ 工作量=2700 ｜ 研究:高级合金 ｜ 技能:制作≥18
- `MMT_MakeFerrotitanium`：钛合金(5) + 钢(12) + 碳纤维锭(3) -> 玻璃钢×15 ｜ 工作量=2400 ｜ 研究:高级合金 ｜ 技能:制作≥15
- `MMT_MakeCupronickelAlloy`：铜(24) + 镍合金(6) -> 梅尔基奥合金×30 ｜ 工作量=1600 ｜ 研究:高级金属精炼 ｜ 技能:制作≥12
- `MMT_MakeDepletedUranium`：铀(10) -> 贫铀合金×10 ｜ 工作量=1700 ｜ 研究:高级合金 ｜ 技能:制作≥11
- `MMT_MakeSteelBar`：钢(15) + 镍合金(5) -> 斯坦合金×20 ｜ 工作量=1800 ｜ 研究:高级合金 ｜ 技能:制作≥11
- `MMT_MakeAlnicoAlloy`：钢(8) + 铝合金(3) + 钴合金(3) + 镍合金(6) -> 亚力可合金×20 ｜ 工作量=1650 ｜ 研究:高级合金 ｜ 技能:制作≥13

### 电动电子（ElectricElectronics）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 电子器件工作台（`MMT_ElectricElectronicsTable`） | (3,1) | 铜×40 + 工业零件×10 + [金属]×140 | 600W | 2750 | 电动电子工艺 | 8 |  |

**电子器件工作台（`MMT_ElectricElectronicsTable`） 的配方：**

- `MMT_AssembleElectronicParts_Electric`：铜(15) + 玻璃块(10) + 硅(10) -> 电子元件×5 ｜ 工作量=1600 ｜ 研究:电动电子工艺 ｜ 技能:制作≥10
- `MMT_AssembleIC_Electric`：硅(25) + 塑料(20) + 电子元件(3) + 黄金(2) -> 集成电路×5 ｜ 工作量=2400 ｜ 研究:电动电子工艺 ｜ 技能:制作≥12
- `MMT_AssembleMicrochip_Electric`：集成电路(2) + 碳聚物(5) + 黄金(5) -> 微芯片集成电路×1 ｜ 工作量=2400 ｜ 研究:电动电子工艺 ｜ 技能:制作≥14
- `MMT_MakeHexcell`：磁性材料(2) + [MMT_HCMBar](4) + 微芯片集成电路(1) -> 动力电池×10 ｜ 工作量=6000 ｜ 研究:机器人工程 I ｜ 技能:制作≥14
- `MMT_MakeRobotParts`：[MMT_USLDBar](60) + 先进机械组件(15) -> 机器人部件×25 ｜ 工作量=8000 ｜ 研究:机器人工程 I ｜ 技能:制作≥15
- `MMT_MakeMineralSonarModule`：[MMT_MetallicBar](25) + 太空零件(5) + 电子元件(10) + 机械组件(1) -> 矿物声纳模块×1 ｜ 工作量=4800 ｜ 研究:高级零件 ｜ 技能:制作≥10
- `MMT_MakeEnergyCrystal`：微芯片集成电路(1) + 硅(10) + 黄金(10) -> 能量结晶×2 ｜ 工作量=6000 ｜ 研究:能量聚能 ｜ 技能:制作≥10
- `MMT_MakeFuelRod`：铀(100) + [MMT_SLDBar](20) -> 核燃料棒×1 ｜ 工作量=8000 ｜ 研究:核能发电 ｜ 技能:制作≥12

### 电动石化（ElectricPetrochem）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 石化产品实验室（`MMT_ElectricChemLab`） | (4,5) | 钢筋混凝土×40 + 玻璃块×50 + 工业零件×20 + 电子元件×5 + [金属]×130 | 450W | 5500 | 电动石化工艺 | 11 |  |

**石化产品实验室（`MMT_ElectricChemLab`） 的配方：**

- `MMT_DistillPolymers_Electric`：原油(20) -> 聚合物×15 ｜ 工作量=1000 ｜ 研究:电动石化工艺 ｜ 技能:制作≥6
- `MMT_MakePlastic_Electric`：聚合物(30) -> 塑料×10 ｜ 工作量=500 ｜ 研究:电动石化工艺 ｜ 技能:制作≥8
- `MMT_MakeRubber_Electric`：聚合物(30) + 硫磺(3) -> 合成橡胶×10 ｜ 工作量=500 ｜ 研究:电动石化工艺 ｜ 技能:制作≥8
- `MMT_MakeSyntheticFibers_Electric`：聚合物(25) + 硫磺(3) -> 人造纤维×5 ｜ 工作量=800 ｜ 研究:电动石化工艺 ｜ 技能:制作≥10
- `MMT_MakeCarbon_Electric`：聚合物(25) + 木炭/原煤(25) + 合成橡胶(15) -> 碳聚物×12 ｜ 工作量=2800 ｜ 研究:电动石化工艺 ｜ 技能:制作≥12
- `MMT_MakeParaffins`：原油(20) -> 石蜡×10 ｜ 工作量=1000 ｜ 研究:高级石化工艺 ｜ 技能:制作≥6
- `MMT_MakeSulphates`：硫磺(10) -> 硫酸盐×10 ｜ 工作量=800 ｜ 研究:高级石化工艺 ｜ 技能:制作≥6
- `MMT_MakeCarbonAlloy`：人造纤维(10) + 碳聚物(10) -> 碳纤维锭×10 ｜ 工作量=2800 ｜ 研究:高级石化工艺 ｜ 技能:制作≥15
- `MMT_MakeOxidizedChemfuel`：化合燃料(100) + 硝石(15) + 硫酸盐(15) + 聚合物(5) -> 氧化化合燃料×75 ｜ 工作量=10000 ｜ 研究:氧化燃料 ｜ 技能:制作≥12
- `MMT_ExtractBiomatterCorpse`：[Corpses](1) -> 生物质×20 ｜ 工作量=6000 ｜ 研究:物质转换 I ｜ 技能:制作≥8
- `MMT_ExtractBiomatterMeat`：[MeatRaw](40) -> 生物质×20 ｜ 工作量=4000 ｜ 研究:物质转换 I ｜ 技能:制作≥8

### 电动木材（ElectricWood）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 电动锯木台（`MMT_ElectricSawmill`） | (3,1) | 工业零件×10 + 电子元件×5 + [金属]×150 | 600W | 5000 | 电动木材加工 | 4 |  |
| 电动木炭窑（`MMT_ElectricCharcoalKiln`） | (3,3) | 工业零件×5 + 电子元件×3 + [石质]×180 | 1500W | 3000 | 电动木材加工 | 1 |  |

**电动锯木台（`MMT_ElectricSawmill`） 的配方：**

- `MMT_MakeWoodPlanks_Electric`：原木(20) -> 木板×30 ｜ 工作量=1200 ｜ 研究:电动木材加工 ｜ 技能:制作≥6
- `MMT_MakeKindling_Electric`：原木(20) -> 柴火×40 ｜ 工作量=500 ｜ 研究:电动木材加工 ｜ 技能:制作≥4
- `MMT_MakeBambooPlanks_Electric`：竹节(20) -> 竹板×30 ｜ 工作量=1200 ｜ 研究:电动木材加工 ｜ 技能:制作≥9
- `MMT_MakeRedWoodPlanks_Electric`：红木(20) -> 红木木板×30 ｜ 工作量=1200 ｜ 研究:电动木材加工 ｜ 技能:制作≥6

**电动木炭窑（`MMT_ElectricCharcoalKiln`） 的配方：**

- `MMT_MakeCharcoal_Electric`：原木(25) -> 木炭×20 ｜ 工作量=800 ｜ 研究:电动木材加工 ｜ 技能:制作≥4

### 电子（Electronics）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 电子器件工作台（`MMT_HandElectronicsTable`） | (3,3) | 工业零件×5 + [金属]×140 | — | 2200 | 基础电子工艺, 集成电路技术 | 3 |  |

**电子器件工作台（`MMT_HandElectronicsTable`） 的配方：**

- `MMT_AssembleElectronicParts_Hand`：铜(15) + 玻璃块(10) + 硅(10) -> 电子元件×5 ｜ 工作量=2500 ｜ 研究:基础电子工艺 ｜ 技能:制作≥8
- `MMT_AssembleIC_Hand`：硅(25) + 塑料(20) + 电子元件(3) + 黄金(2) -> 集成电路×5 ｜ 工作量=3500 ｜ 研究:集成电路技术 ｜ 技能:制作≥10
- `MMT_AssembleMicrochip_Hand`：集成电路(2) + 碳聚物(5) + 黄金(5) -> 微芯片集成电路×1 ｜ 工作量=3500 ｜ 研究:集成电路技术 ｜ 技能:制作≥12

### EnergyConvergence（EnergyConvergence）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 能量聚能发电站（`MMT_EnergyConvergencePlant`） | (3,3) | 钢×200 + 碳纤维锭×40 + 黄金×30 + 先进机械组件×4 | -10000W | 10000 | 能量聚能 | 0 | CompProperties_EnergyIgnition |
| 点火充能装置（`MMT_IgnitionCharger`） | (1,1) | 钢×50 + 黄金×10 + 工业零件×2 | 50W | 2000 | 能量聚能 | 0 | CompProperties_IgnitionCharger |

### Glassworks（Glassworks）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 玻璃制作台（`MMT_GlassworksTable`） | (3,1) | 工业零件×10 + [金属]×130 | 400W | 1600 | 玻璃制造 | 3 |  |

**玻璃制作台（`MMT_GlassworksTable`） 的配方：**

- `MMT_MakeGlassBatch`：沙(75) -> 玻璃生料×25 ｜ 工作量=900 ｜ 研究:玻璃制造
- `MMT_MakeGlass`：玻璃生料(20) -> 玻璃块×10 ｜ 工作量=600 ｜ 研究:玻璃制造
- `MMT_MakeReinforcedGlass`：玻璃生料(40) + 碳聚物(5) + 人造纤维(5) -> 强化玻璃块×5 ｜ 工作量=1400 ｜ 研究:强化玻璃工艺 ｜ 技能:制作≥8

### 研磨（Grinder）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手动研磨机（`MMT_HandGrinder`） | (1,1) | [石质]×50 | — | 800 | — | 1 |  |
| 电动研磨机（`MMT_ElectricGrinder`） | (1,1) | 机械组件×4 + 工业零件×2 + [金属]×50 | 300W | 1500 | 矿粉筛选 | 1 |  |

**手动研磨机（`MMT_HandGrinder`） 的配方：**

- `MMT_GrindOrePowder_Hand`：粗矿石(25) -> 矿粉×25 ｜ 工作量=2400 ｜ 研究:Stonecutting

**电动研磨机（`MMT_ElectricGrinder`） 的配方：**

- `MMT_GrindOrePowder_Electric`：粗矿石(25) -> 矿粉×25 ｜ 工作量=1200

### 物质（Matter）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 无机物转化器（`MMT_MatterConverter`） | (3,3) | 人造纤维×50 + 高级零部件×12 + 先进机械组件×6 + 微芯片集成电路×4 + [金属]×280 | 2500W | 9000 | 物质转换 I | 14 |  |
| 量子加工台（`MMT_Matterfab`） | (3,2) | 磁性材料×25 + 高级零部件×10 + 先进机械组件×10 + 微芯片集成电路×2 + [金属]×240 | 8000W | 10000 | 物质转换 III | 2 |  |
| 粒子对撞台（`MMT_ParticleCollider`） | (7,2) | 钢筋混凝土×150 + 磁性材料×20 + 生物合成材料×15 + 先进机械组件×12 + 微芯片集成电路×3 + [金属]×120 | 15000W | 12000 | 物质转换 II | 1 |  |

**无机物转化器（`MMT_MatterConverter`） 的配方：**

- `MMT_ConvertResourcesRaw`：[ResourcesRaw](300) -> 无机物×150 ｜ 工作量=10000 ｜ 研究:物质转换 I ｜ 技能:制作≥15
- `MMT_ConvertToIron`：无机物(50) -> 铁矿石×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToCopper`：无机物(50) -> 铜矿×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToTin`：无机物(50) -> 锡矿×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToLead`：无机物(50) -> 铅矾矿×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToAluminium`：无机物(50) -> 铝土矿×80 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToNickel`：无机物(150) -> 镍矿×20 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToTitanium`：无机物(200) -> 钛铁矿×20 ｜ 工作量=15000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToGold`：无机物(60) -> 金矿×20 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToSilver`：无机物(100) -> 白银×300 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToUranium`：无机物(200) -> 铀×50 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToPolymers`：无机物(50) -> 聚合物×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToParaffins`：无机物(50) -> 石蜡×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15
- `MMT_ConvertToSulphates`：无机物(50) -> 硫酸盐×100 ｜ 工作量=12000 ｜ 研究:物质转换 II ｜ 技能:制作≥15

**量子加工台（`MMT_Matterfab`） 的配方：**

- `MMT_ProduceAPoly`：贫铀合金(5) + 碳纤维锭(5) + 反物质(3) -> α阿尔法聚合物×15 ｜ 工作量=8000 ｜ 技能:制作≥19
- `MMT_ProduceBPoly`：α阿尔法聚合物(3) + 生物合成材料(5) -> β贝塔聚合物×5 ｜ 工作量=12000 ｜ 技能:制作≥20

**粒子对撞台（`MMT_ParticleCollider`） 的配方：**

- `MMT_MakeAntimatter`：无机物(50) + 铀(20) -> 反物质×2 ｜ 工作量=10000 ｜ 技能:制作≥15

### 医药（Medicine）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手工制药台（`MMT_MedicalTable`） | (3,1) | 玻璃块×20 + [金属/木质]×75 | — | 2000 | 医疗物资 II | 9 |  |
| 电动制药台（`MMT_ElectricMedicalTable`） | (3,1) | 玻璃块×60 + 合成橡胶×45 + 工业零件×10 + 电子元件×5 + [金属/木质]×125 | 500W | 4000 | MedicineProduction | 10 |  |
| 晾晒架（`MMT_DryingRack`） | (2,1) | [木质/金属]×60 | — | 800 | 毒品 I | 2 |  |
| 电动晾晒架（`MMT_ElectricDryingRack`） | (2,2) | 工业零件×4 + 电子元件×2 + [金属]×80 | 400W | 2500 | 毒品 III | 2 |  |
| 中性胺精炼厂（`MMT_NeutroamineRefiner`） | (2,2) | 人造纤维×25 + 玻璃块×25 + 电子元件×15 + 太空零件×5 + [金属]×160 | 2500W | 8000 | 毒品 V | 1 |  |

**手工制药台（`MMT_MedicalTable`） 的配方：**

- `MMT_MakeHerbalTea`：野玫瑰(2) + 金丝桃(1) + 薄荷叶(2) -> 草药茶×6 ｜ 工作量=780 ｜ 研究:DrugProduction ｜ 技能:Medicine≥3,Cooking≥5
- `MMT_MakeActivatedCarbon`：木炭/原煤(5) -> 活性炭丸×9 ｜ 工作量=780 ｜ 研究:DrugProduction ｜ 技能:Medicine≥3,制作≥5
- `MMT_MakeBandagekitcrude`：[MMT_BTextiles](7) -> 粗制绷带×1 ｜ 工作量=150 ｜ 研究:医疗物资 I ｜ 技能:Medicine≥1,制作≥1
- `MMT_MakeBandagekitcrudeBulk`：[MMT_BTextiles](21) -> 粗制绷带×3 ｜ 工作量=450 ｜ 研究:医疗物资 I ｜ 技能:Medicine≥1,制作≥1
- `MMT_MakeBandagekit`：[MMT_BTextiles](10) -> 绷带包×1 ｜ 工作量=250 ｜ 研究:医疗物资 II ｜ 技能:Medicine≥2,制作≥2
- `MMT_MakeBandagekitBulk`：[MMT_BTextiles](30) -> 绷带包×3 ｜ 工作量=750 ｜ 研究:医疗物资 II ｜ 技能:Medicine≥2,制作≥2
- `MMT_MakeHerbMedicine`：MedicineHerbal(8) + 灰烬(2) -> 草药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 III ｜ 技能:Medicine≥5
- `MMT_MakeBasicMedicineKit`：芦荟叶(5) + 绷带包(1) -> 基础医药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 IV ｜ 技能:Medicine≥4,制作≥2
- `MMT_MakeBasicMedicineKitMix`：野玫瑰(1) + 金丝桃(1) + 薄荷叶/芦荟叶(1) + 绷带包(1) -> 基础医药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 IV ｜ 技能:Medicine≥4,制作≥2

**电动制药台（`MMT_ElectricMedicalTable`） 的配方：**

- `MMT_MakeHerbalTea`：野玫瑰(2) + 金丝桃(1) + 薄荷叶(2) -> 草药茶×6 ｜ 工作量=780 ｜ 研究:DrugProduction ｜ 技能:Medicine≥3,Cooking≥5
- `MMT_MakeActivatedCarbon`：木炭/原煤(5) -> 活性炭丸×9 ｜ 工作量=780 ｜ 研究:DrugProduction ｜ 技能:Medicine≥3,制作≥5
- `MMT_MakeBandagekit`：[MMT_BTextiles](10) -> 绷带包×1 ｜ 工作量=250 ｜ 研究:医疗物资 II ｜ 技能:Medicine≥2,制作≥2
- `MMT_MakeBandagekitBulk`：[MMT_BTextiles](30) -> 绷带包×3 ｜ 工作量=750 ｜ 研究:医疗物资 II ｜ 技能:Medicine≥2,制作≥2
- `MMT_MakeHerbMedicine`：MedicineHerbal(8) + 灰烬(2) -> 草药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 III ｜ 技能:Medicine≥5
- `MMT_MakeBasicMedicineKit`：芦荟叶(5) + 绷带包(1) -> 基础医药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 IV ｜ 技能:Medicine≥4,制作≥2
- `MMT_MakeBasicMedicineKitMix`：野玫瑰(1) + 金丝桃(1) + 薄荷叶/芦荟叶(1) + 绷带包(1) -> 基础医药包×1 ｜ 工作量=500 ｜ 研究:医疗物资 IV ｜ 技能:Medicine≥4,制作≥2
- `MMT_BrewMedicalDrink`：MedicineHerbal(1) + RawHops(10) -> 抗生素×1 ｜ 工作量=1000 ｜ 研究:PenoxycylineProduction ｜ 技能:Medicine≥10
- `MMT_MakeMedicineIndustrial`：Neutroamine(2) + MedicineHerbal(6) + 石蜡(10) + 人造纤维(2) + 塑料(10) -> MedicineIndustrial×1 ｜ 工作量=800 ｜ 研究:MedicineProduction ｜ 技能:Medicine≥10
- `MMT_MakeMedicineUltratech`：微芯片集成电路(1) + 抗生素(2) + 生物合成材料(2) + 石蜡(3) + Neutroamine(2) + Synthread(5) + 塑料(20) -> MedicineUltratech×1 ｜ 工作量=1400 ｜ 研究:医疗物资 VI ｜ 技能:Medicine≥15

**晾晒架（`MMT_DryingRack`） 的配方：**

- `MMT_DrySmokeleaf`：SmokeleafLeaves(5) -> 干烟叶×5 ｜ 工作量=2400 ｜ 研究:毒品 I
- `MMT_DrySmokeleafBulk`：SmokeleafLeaves(25) -> 干烟叶×25 ｜ 工作量=9600 ｜ 研究:毒品 I

**电动晾晒架（`MMT_ElectricDryingRack`） 的配方：**

- `MMT_DrySmokeleaf`：SmokeleafLeaves(5) -> 干烟叶×5 ｜ 工作量=2400 ｜ 研究:毒品 I
- `MMT_DrySmokeleafBulk`：SmokeleafLeaves(25) -> 干烟叶×25 ｜ 工作量=9600 ｜ 研究:毒品 I

**中性胺精炼厂（`MMT_NeutroamineRefiner`） 的配方：**

- `MMT_RefineNeutroamine`：化合燃料(15) -> Neutroamine×1 ｜ 工作量=1200 ｜ 研究:毒品 V ｜ 技能:Medicine≥6

### 金属（Metal）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 熔炉（`MMT_HandFurnace`） | (3,3) | [石质]×240 | — | 3000 | 基础金属冶炼 | 8 |  |
| 铜矿（`MMT_MineableCopperOre`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 铜矿） |
| 锡矿（`MMT_MineableTinOre`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 锡矿） |
| 铅矾矿脉（`MMT_MineableAnglesite`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 铅矾矿） |
| 铝土矿脉（`MMT_MineableAluminium`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 铝土矿） |

**熔炉（`MMT_HandFurnace`） 的配方：**

- `MMT_SmeltCopperBar_Hand`：铜矿(10) -> 铜×10 ｜ 工作量=700 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥1
- `MMT_SmeltTinBar_Hand`：锡矿(10) -> 锡×10 ｜ 工作量=600 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥1
- `MMT_MakeBronze_Hand`：铜(20) + 锡(10) -> 青铜×30 ｜ 工作量=1000 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥2
- `MMT_SmeltSteel_Hand`：铁矿石(15) + 木炭/原煤(13) -> 钢×20 ｜ 工作量=1300 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥3
- `MMT_ExtractSilicon_Hand`：沙(50) -> 硅×5 ｜ 工作量=1600 ｜ 研究:基础电子工艺 ｜ 技能:制作≥5
- `MMT_SmeltSilver_Hand`：白银(14) -> 白银×10 ｜ 工作量=750 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥2
- `MMT_SmeltGold_Hand`：金矿(12) -> 黄金×15 ｜ 工作量=750 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥3
- `MMT_SmeltLeadBar_Hand`：铅矾矿(10) -> 铅合金×10 ｜ 工作量=1100 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥2

### 零件（Parts）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手工装配台（`MMT_HandAssemblyBench`） | (3,1) | 原木×40 + 钢×25 + [金属]×125 | — | 1200 | 零件加工 | 7 |  |
| 电动装配台（`MMT_ElectricAssemblyBench`） | (3,1) | 人造纤维×45 + 集成电路×10 + 工业零件×15 + 机械组件×10 + [金属]×220 | 750W | 5500 | 零件加工, 高级零件 | 18 |  |
| 自动化装配器（`MMT_RoboticAssembler`） | (1,1) | 人造纤维×45 + 高级零部件×10 + 先进机械组件×10 + 微芯片集成电路×5 + [金属]×120 | 5000W | 9000 | 超级零件 | 7 |  |

**手工装配台（`MMT_HandAssemblyBench`） 的配方：**

- `MMT_MakeComponentMedieval_Wood`：原木(20) -> 原始零部件×10 ｜ 工作量=900 ｜ 研究:零件加工
- `MMT_MakeComponentMedieval_Metal`：钢/青铜/铜/锡/铝合金/白银/黄金(10) -> 原始零部件×10 ｜ 工作量=900 ｜ 研究:零件加工
- `MMT_MakeComponentIndustrial_Hand`：钢/青铜/斯坦合金/斯特合金(30) -> 工业零件×10 ｜ 工作量=2000 ｜ 研究:零件加工 ｜ 技能:制作≥3
- `MMT_MakeWire_Hand`：铜(10) -> 电线×20 ｜ 工作量=800 ｜ 研究:零件加工 ｜ 技能:制作≥3
- `MMT_MakeMechanism_Hand`：钢/青铜/斯特合金(30) + 工业零件(10) -> 机械组件×5 ｜ 工作量=1800 ｜ 研究:零件加工 ｜ 技能:制作≥4
- `MMT_MakeComponentSpacer_Hand`：玻璃钢/斯坦合金/钛合金/镍钛合金/亚力可合金(30) -> 太空零件×10 ｜ 工作量=2500 ｜ 研究:高级零件 ｜ 技能:制作≥6
- `MMT_MakeAdvMechanism_Hand`：玻璃钢/斯坦合金/钛合金/镍钛合金/亚力可合金(30) + 太空零件(10) + 人造纤维(5) -> 先进机械组件×5 ｜ 工作量=3200 ｜ 研究:高级零件 ｜ 技能:制作≥13

**电动装配台（`MMT_ElectricAssemblyBench`） 的配方：**

- `MMT_MakeComponentMedieval_Wood`：原木(20) -> 原始零部件×10 ｜ 工作量=900 ｜ 研究:零件加工
- `MMT_MakeComponentMedieval_Metal`：钢/青铜/铜/锡/铝合金/白银/黄金(10) -> 原始零部件×10 ｜ 工作量=900 ｜ 研究:零件加工
- `MMT_MakeComponentIndustrial_Electric`：钢/青铜/斯坦合金/斯特合金(30) -> 工业零件×10 ｜ 工作量=2000 ｜ 研究:零件加工 ｜ 技能:制作≥3
- `MMT_MakeWire_Electric`：铜(20) -> 电线×40 ｜ 工作量=800 ｜ 研究:零件加工 ｜ 技能:制作≥6
- `MMT_MakeMechanism_Electric`：钢/青铜/斯特合金(60) + 工业零件(20) -> 机械组件×10 ｜ 工作量=1800 ｜ 研究:零件加工 ｜ 技能:制作≥7
- `MMT_MakeComponentSpacer_Electric`：玻璃钢/斯坦合金/钛合金/镍钛合金/亚力可合金(30) -> 太空零件×10 ｜ 工作量=2500 ｜ 研究:高级零件 ｜ 技能:制作≥6
- `MMT_MakeAdvMechanism_Electric`：玻璃钢/斯坦合金/钛合金/镍钛合金/亚力可合金(60) + 太空零件(20) + 人造纤维(10) -> 先进机械组件×10 ｜ 工作量=3200 ｜ 研究:高级零件 ｜ 技能:制作≥16
- `MMT_MakeSpacerComponent`：玻璃钢/斯坦合金/钛合金/镍钛合金/亚力可合金(25) + 白银/黄金(5) -> 高级零部件×10 ｜ 工作量=3500 ｜ 研究:高级零件 ｜ 技能:制作≥9
- `MMT_MakeReinforcedConcrete_Electric`：[MMT_MetallicBar](20) + 混凝土(20) -> 钢筋混凝土×16 ｜ 工作量=1200 ｜ 技能:制作≥7
- `MMT_MakeMineralSonarModule`：[MMT_MetallicBar](25) + 太空零件(5) + 电子元件(10) + 机械组件(1) -> 矿物声纳模块×1 ｜ 工作量=4800 ｜ 研究:高级零件 ｜ 技能:制作≥10
- `MMT_MakeOverclockModule`：钢(25) + 机械组件(2) + 工业零件(2) -> 超频模块×1 ｜ 工作量=1500 ｜ 研究:自动机增效 ｜ 技能:制作≥6
- `MMT_MakeParallelModule`：钢(30) + 机械组件(3) + 工业零件(3) -> 并行模块×1 ｜ 工作量=1800 ｜ 研究:自动机增效 ｜ 技能:制作≥6
- `MMT_MakeYieldModule`：钢(35) + 机械组件(3) + 工业零件(3) + 电子元件(1) -> 矿产增值模块×1 ｜ 工作量=2000 ｜ 研究:自动机增效 ｜ 技能:制作≥6
- `MMT_MakeChargeEfficiencyModule`：钢(25) + 电线(15) + 电子元件(2) + 工业零件(2) -> 充能效率模块×1 ｜ 工作量=2000 ｜ 研究:充能增效 ｜ 技能:制作≥6
- `MMT_MakeDimensionalModule`：玻璃钢(20) + 太空零件(2) + 电子元件(3) + 黄金(10) -> 次元传送模块×1 ｜ 工作量=3000 ｜ 研究:次元传送 ｜ 技能:制作≥8
- `MMT_MakeDeployableShield`：钢(25) + 机械组件(2) + 工业零件(2) + 电线(5) -> 部署式护盾发生器×1 ｜ 工作量=4000 ｜ 研究:部署式护盾技术 ｜ 技能:制作≥6
- `MMT_MakeChargeShieldPack`：集成电路(2) + 太空零件(4) + 玻璃钢(30) + 电线(10) -> 充能护盾腰带×1 ｜ 工作量=8000 ｜ 研究:充能技术 ｜ 技能:制作≥10
- `MMT_MakePsychicShieldBelt`：PsychicAmplifier(1) + 太空零件(4) + 玻璃钢(15) + 黄金(10) -> 灵能护盾腰带×1 ｜ 工作量=8000 ｜ 研究:灵能护盾技术 ｜ 技能:制作≥10

**自动化装配器（`MMT_RoboticAssembler`） 的配方：**

- `MMT_MakeUltraComponent`：钨合金/钛合金(15) + 司太立合金（钨铬钴合金）/钨钴合金(10) + 白银/黄金(5) -> 超级零部件×10 ｜ 工作量=4500 ｜ 研究:超级零件 ｜ 技能:制作≥13
- `MMT_DeconstructPersonalShield`：Apparel_ShieldBelt(1) -> 太空零件×8 + 集成电路×1 ｜ 工作量=1000 ｜ 研究:高级零件 ｜ 技能:制作≥10
- `MMT_BuildMagneticMaterial`：无机物(30) + 硫酸盐(15) + [MMT_USLDBar](10) + [MMT_SLDBar](5) -> 磁性材料×5 ｜ 工作量=2400 ｜ 研究:超级零件 ｜ 技能:制作≥18
- `MMT_BuildBiosyntheticMaterial`：生物质(15) + 人造纤维(9) + 塑料(30) -> 生物合成材料×5 ｜ 工作量=2400 ｜ 研究:超级零件 ｜ 技能:制作≥18
- `MMT_MakeBioelectronicDevices`：生物质(60) + Hyperweave(15) + 微芯片集成电路(2) -> 生物电子元件×1 ｜ 工作量=7000 ｜ 研究:超级零件 ｜ 技能:制作≥18
- `MMT_DisassembleAIPersonaCore`：AIPersonaCore(1) -> 生物电子元件×1 + α阿尔法聚合物×30 + 碳纤维锭×40 ｜ 工作量=4000 ｜ 研究:高级零件 ｜ 技能:制作≥16
- `MMT_MakeAIPersonaCore`：生物电子元件(2) + α阿尔法聚合物(60) + 碳纤维锭(80) -> AIPersonaCore×1 ｜ 工作量=8000 ｜ 研究:人格核心合成 ｜ 技能:制作≥16

### 石化（Petrochem）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 石化产品实验室（`MMT_HandChemTable`） | (3,2) | 工业零件×5 + [金属]×130 | — | 2000 | 基础石化工艺 | 7 |  |
| 手压油泵（`MMT_HandOilPump`） | (2,2) | [金属]×60 | — | 1500 | 基础石化工艺 | 1 |  |

**石化产品实验室（`MMT_HandChemTable`） 的配方：**

- `MMT_DistillPolymers_Hand`：原油(20) -> 聚合物×15 ｜ 工作量=1500 ｜ 研究:基础石化工艺 ｜ 技能:制作≥4
- `MMT_MakePlastic_Hand`：聚合物(30) -> 塑料×10 ｜ 工作量=800 ｜ 研究:基础石化工艺 ｜ 技能:制作≥6
- `MMT_MakeRubber_Hand`：聚合物(30) + 硫磺(3) -> 合成橡胶×10 ｜ 工作量=800 ｜ 研究:基础石化工艺 ｜ 技能:制作≥6
- `MMT_MakeSyntheticFibers_Hand`：聚合物(25) + 硫磺(3) -> 人造纤维×5 ｜ 工作量=1200 ｜ 研究:基础石化工艺 ｜ 技能:制作≥8
- `MMT_MakeCarbon_Hand`：聚合物(25) + 木炭/原煤(25) + 合成橡胶(15) -> 碳聚物×12 ｜ 工作量=4000 ｜ 研究:基础石化工艺 ｜ 技能:制作≥10
- `MMT_MakeParaffins`：原油(20) -> 石蜡×10 ｜ 工作量=1000 ｜ 研究:高级石化工艺 ｜ 技能:制作≥6
- `MMT_MakeSulphates`：硫磺(10) -> 硫酸盐×10 ｜ 工作量=800 ｜ 研究:高级石化工艺 ｜ 技能:制作≥6

**手压油泵（`MMT_HandOilPump`） 的配方：**

- `MMT_PumpCrudeOil`：— -> 原油×10 ｜ 工作量=4000 ｜ 研究:基础石化工艺 ｜ 技能:采矿≥3

### 石化扩展（PetrochemExt）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 硝石矿脉（`MMT_MineableNitre`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 硝石） |
| 纺织机（`MMT_Loom`） | (2,2) | 原始零部件×10 + 电子元件×3 + 机械组件×6 + [金属/木质]×125 | 300W | 3850 | ComplexClothing | 10 |  |
| 弹药工作台（`MMT_AmmoBench`） | (3,1) | 工业零件×10 + 电子元件×5 + 机械组件×10 + [金属]×210 | 500W | 6500 | 机械加工 | 24 |  |

**纺织机（`MMT_Loom`） 的配方：**

- `MMT_MakeCompaste`：化合燃料(10) + 合成橡胶(5) + 石蜡(2) -> 复合粘剂×20 ｜ 工作量=3200 ｜ 研究:高级石化工艺 ｜ 技能:制作≥5
- `MMT_MakeCloth`：原棉(20) -> 布×20 ｜ 工作量=600 ｜ 研究:ComplexClothing ｜ 技能:制作≥2
- `MMT_MakeFlaxcloth`：生亚麻(20) -> 亚麻布×10 ｜ 工作量=600 ｜ 技能:制作≥4
- `MMT_MakeHempCloth`：SmokeleafLeaves(30) -> 大麻布×15 ｜ 工作量=1200 ｜ 研究:ComplexClothing ｜ 技能:制作≥6
- `MMT_MakeDevilstrand`：生恶魔菇(15) + 人造纤维(2) -> DevilstrandCloth×15 ｜ 工作量=600 ｜ 研究:AdvancedFabrication ｜ 技能:制作≥13
- `MMT_MakeSynthread`：原棉(10) + 人造纤维(3) -> Synthread×10 ｜ 工作量=600 ｜ 研究:AdvancedFabrication ｜ 技能:制作≥12
- `MMT_MakeVelour`：复合粘剂(5) + [MMT_BTextiles](40) -> 丝绒×25 ｜ 工作量=1800 ｜ 研究:AdvancedFabrication ｜ 技能:制作≥12
- `MMT_MakeKevlar`：塑料(30) + 人造纤维(15) -> 凯夫拉×10 ｜ 工作量=1800 ｜ 研究:AdvancedFabrication ｜ 技能:制作≥13
- `MMT_MakeHyperweave`：Synthread(10) + DevilstrandCloth(10) -> Hyperweave×10 ｜ 工作量=600 ｜ 研究:超织物工艺 ｜ 技能:制作≥17
- `MMT_MakeChitinPlating`：甲壳(50) + 人造纤维(4) + 复合粘剂(1) -> 甲壳板×15 ｜ 工作量=1080 ｜ 研究:AdvancedFabrication ｜ 技能:制作≥12

**弹药工作台（`MMT_AmmoBench`） 的配方：**

- `MMT_MakeSulfur`：铁矿石(35) -> 硫磺×10 ｜ 工作量=1200
- `MMT_MakePowder`：硝石(75) + [MMT_Coal](20) + 硫磺(20) -> 火药×20 ｜ 工作量=3000 ｜ 技能:制作≥3
- `MMT_MakeNapalm`：化合燃料(30) + 聚合物(15) -> 凝固汽油×10 ｜ 工作量=1200 ｜ 技能:制作≥5
- `MMT_MakeAmmo_120mmCannonShell_HEAT`：钢(90) + 工业零件(2) + 化合燃料(7) -> 120mm 炮弹（破甲）×2 ｜ 工作量=13000 ｜ 技能:制作≥4
- `MMT_MakeAmmo_130mmRocketMissile`：[MMT_SLDBar]/[MMT_USLDBar](42) + [MMT_HCMBar](8) + 化合燃料(32) + 凝固汽油(5) + 集成电路(1) -> 130mm 制导火箭弹（高爆弹）×1 ｜ 工作量=2000
- `MMT_MakeAmmo_155mmHowitzerShell_HE`：钢(94) + 工业零件(2) + 化合燃料(17) -> 155mm 炮弹（高爆弹）×1 ｜ 工作量=17400 ｜ 技能:制作≥4
- `MMT_MakeAmmo_20x102mm_FMJ`：[MMT_SLDBar]/[MMT_USLDBar](50) + [MMT_HCMBar](15) + 火药(9) -> 20x102mm 弹药（全金属被甲弹）×40 ｜ 工作量=3000 ｜ 技能:制作≥10
- `MMT_MakeAmmo_30x173mm_FMJ`：[MMT_SLDBar]/[MMT_USLDBar](60) + [MMT_HCMBar](16) + 火药(8) -> 30x173mm 弹药（全金属被甲弹）×30 ｜ 工作量=3000 ｜ 技能:制作≥11
- `MMT_MakeAmmo_30x29mmGrenade_HE`：钢(74) + 化合燃料(8) + 工业零件(2) -> 30x29mm 榴弹（高爆弹）×100 ｜ 工作量=11800 ｜ 技能:制作≥4
- `MMT_MakeAmmo_50BMG_FMJ`：钢(50) + 火药(25) -> .50 机枪弹（全金属被甲弹）×200 ｜ 工作量=5000
- `MMT_MakeAmmo_762x51mmNATO_FMJ`：钢(26) + 火药(13) -> 7.62x51mm 北约标准弹（全金属被甲弹）×500 ｜ 工作量=2600
- `MMT_MakeAmmo_BalistaBolt`：[MMT_Woody](2) + [MMT_MetallicBar](5) -> 弩炮箭×10 ｜ 工作量=1500
- `MMT_MakeAmmo_StoneBall`：[StoneChunks](1) -> 石球×5 ｜ 工作量=1500
- `MMT_MakeAmmo_SPG9Rocket_Frag`：钢(66) + 化合燃料(6) + 工业零件(4) -> SPG-9 Rocket (HE)×5 ｜ 工作量=11400 ｜ 技能:制作≥4
- `MMT_MakeAmmo_145x114mm_FMJ`：钢(40) + 火药(6) -> 14.5x114mm 机枪弹（全金属被甲弹）×100 ｜ 工作量=4000 ｜ 技能:制作≥5
- `MMT_MakeEMPGrenades`：[MMT_MetallicBar](36) + 工业零件(7) + 电子元件(4) -> Weapon_GrenadeEMP×5 ｜ 工作量=6000 ｜ 研究:手榴弹 II ｜ 技能:制作≥11
- `MMT_MakeFirefoamGrenade`：[MMT_MetallicBar](30) + 表面活性剂(15) + 火药/化合燃料(3) + 工业零件(4) -> firefoam grenade×5 ｜ 工作量=4000 ｜ 研究:手榴弹 I ｜ 技能:制作≥7
- `MMT_MakeFlashbangGrenades`：[MMT_MetallicBar](25) + 火药(6) + 工业零件(4) -> flashbang grenade×5 ｜ 工作量=6000 ｜ 研究:手榴弹 I ｜ 技能:制作≥9
- `MMT_MakeFragGrenades`：[MMT_MetallicBar](30) + 火药/化合燃料(7) + 工业零件(4) -> Weapon_GrenadeFrag×5 ｜ 工作量=4000 ｜ 研究:手榴弹 II ｜ 技能:制作≥7
- `MMT_MakeHighExplosiveGrenades`：[MMT_MetallicBar](24) + 火药/化合燃料(5) + 工业零件(3) -> 高爆手榴弹×5 ｜ 工作量=3600 ｜ 研究:手榴弹 I ｜ 技能:制作≥6
- `MMT_MakeIncendiaryGrenade`：工业零件(4) + 凝固汽油(5) + [MMT_MetallicBar](30) -> 燃烧手榴弹×5 ｜ 工作量=3500 ｜ 研究:手榴弹 II ｜ 技能:制作≥9
- `MMT_MakeMolotov`：玻璃块(30) + 化合燃料(30) + [MMT_BTextiles](30) -> Weapon_GrenadeMolotov×5 ｜ 工作量=3500 ｜ 研究:巨弓 ｜ 技能:制作≥5
- `MMT_MakeSmokeGrenade`：[MMT_MetallicBar](36) + 塑料(10) + 工业零件(5) -> smoke grenade×5 ｜ 工作量=6000 ｜ 研究:手榴弹 II ｜ 技能:制作≥9
- `MMT_MakeStickBomb`：原始零部件(15) + 化合燃料(3) + [MMT_BTextiles](30) -> stick bomb×5 ｜ 工作量=3000 ｜ 研究:反曲弓 ｜ 技能:制作≥3

### 电力（Power）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 原煤发电机（`MMT_CoalGenerator`） | (2,2) | 钢×100 + 工业零件×3 | -2500W | 3000 | 电力 | 0 |  |
| 核能反应堆（`MMT_NuclearReactor`） | (3,3) | 钢×300 + 碳纤维锭×60 + 工业零件×10 + 先进机械组件×4 | -20000W | 12000 | 核能发电 | 0 |  |

### 研究（Research）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 多元分析仪（`MMT_MultiAnalyzer`） | (2,2) | 钢×40 + 玻璃钢×50 + 黄金×20 + 工业零件×8 | 200W | 10000 | 多元分析仪 | 0 |  |
| 实验站（`MMT_LabStation`） | (3,1) | 钢×60 + 玻璃块×20 + 工业零件×6 | 400W | 12000 | 实验站 | 0 |  |
| 研究终端（`MMT_LabTerminal`） | (1,1) | 钢×25 + 工业零件×3 | 250W | 3000 | 研究终端 | 0 |  |

### 护盾（Shield）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 临时护盾投影器（`MMT_ShieldProjector`） | — | — | — | — | — | 0 |  |

### 筛选（Sieve）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手动筛子（`MMT_HandSieve`） | (2,2) | [木质]×75 | — | 600 | — | 1 | CompProperties_SieveMesh |
| 电动筛子（`MMT_ElectricSieve`） | (2,2) | 机械组件×2 + 工业零件×2 + [金属]×60 | 250W | 1200 | 矿粉筛选 | 1 | CompProperties_SieveMesh |

**手动筛子（`MMT_HandSieve`） 的配方：**

- `MMT_SiftOrePowder_Hand`：矿粉(15) -> — ｜ 工作量=1800 ｜ 研究:Stonecutting

**电动筛子（`MMT_ElectricSieve`） 的配方：**

- `MMT_SiftOrePowder_Electric`：矿粉(20) -> — ｜ 工作量=900

### 硫（Sulfur）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 硫磺矿脉（`MMT_MineableSulfur`） | — | — | — | — | — | 0 | 矿脉（自然生成，产 硫磺） |

### 传送（Teleport）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 折跃传送台（`MMT_TeleportPad`） | (3,3) | 玻璃钢×120 + 铀×40 + 太空零件×6 | 3000W | 12000 | 折跃传送 | 0 | CompProperties_TeleportPad、CompProperties_ModuleSlots、CompProperties_BuildingBattery |

### 炮塔（Turrets）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GAU-8复仇者火炮（`MMT_AvengerTurretGun`） | (2,2) | GAU-8复仇者火炮组件箱×1 + [金属]×100 | — | 2000 | 重型炮塔 III | 0 | 炮塔（复仇者机炮） |
| 155mm榴弹炮（`MMT_HowitzerTurretGun`） | (2,2) | 155mm榴弹炮组件箱×1 + [金属]×210 | — | 2100 | 火炮 II | 0 | 炮塔（155mm榴弹炮） |
| FGM-148“标枪”反坦克导弹（`MMT_LightMissileSystem`） | (1,1) | FGM-148“标枪”反坦克导弹组件箱×1 + [金属]×100 | — | 1600 | 火箭弹 II | 0 | 炮塔（FGM-148“标枪”反坦克导弹） |
| 弩炮（`MMT_MedievalTimes_Ballista`） | (1,1) | 弩炮组件箱×1 + [木质]×90 | — | 4000 | 攻城武器 | 0 | 炮塔（弩炮） |
| 投石机（`MMT_Medieval_Catapult`） | (2,2) | 投石机组件箱×1 + [木质]×150 | — | 5000 | 攻城武器 | 0 | 炮塔（投石机） |
| 中型导弹系统（`MMT_MediumMissileSystem`） | (2,2) | 130mm中型导弹系统组件箱×1 + [金属]×100 | — | 2400 | 火箭弹 III | 0 | 炮塔（中型导弹系统） |
| 舰载哨戒炮塔（`MMT_NavalGunTurretGun`） | (2,2) | 35mm哨戒炮塔组件箱×1 + [金属]×350 | 2200W | 1400 | 自动炮塔 III | 0 | 炮塔（哨戒机炮） |
| 重型导弹系统（`MMT_RLGun`） | (2,2) | 130mm重型导弹系统组件箱×1 + [金属]×100 | — | 2800 | 火箭弹 III | 0 | 炮塔（重型导弹系统） |
| AGS-30 榴弹发射器（`MMT_Turret_AGLGrenadeLauncher`） | (1,1) | AGS-30组件箱×1 + [金属]×90 | — | 1500 | 发射器 I | 0 | 炮塔（AGL榴弹发射器） |
| 120mm坦克炮（`MMT_Turret_Cannon`） | (2,2) | 120mm坦克炮组件箱×1 + [金属]×180 | — | 2150 | 火炮 II | 0 | 炮塔（120mm坦克炮） |
| M2 .50勃朗宁机枪（`MMT_Turret_M2Browning`） | (1,1) | M2 .50勃朗宁机枪组件箱×1 + [金属]×70 | — | 1200 | 狙击炮塔 | 0 | 炮塔（M2勃朗宁重机枪） |
| M134速射机枪（`MMT_Turret_Minigun`） | (1,1) | M134速射机枪组件箱×1 + [金属]×90 | — | 1900 | 重型炮塔 II | 0 | 炮塔（M134型速射机枪） |
| 20mm厄利孔机炮（`MMT_Turret_Oerlikonautocannon`） | (2,2) | 20x102mm厄利孔机炮组件箱×1 + [金属]×100 | — | 1700 | 重型炮塔 III | 0 | 炮塔（20x102mm厄利孔机炮） |
| 哨戒火箭发射器（`MMT_Turret_SentryRocketLauncher`） | (2,2) | 自动反坦克火箭发射系统组件箱×1 + [金属]×160 | 1600W | 16000 | 自动炮塔 III | 0 | 炮塔（哨戒火箭发射器） |
| BGM-71“陶式”反坦克导弹（`MMT_Turret_TOWLauncher`） | (1,1) | BGM-71“陶式”反坦克导弹组件箱×1 + [金属]×100 | — | 1500 | 火箭弹 III | 0 | 炮塔（BGM-71“陶式”反坦克导弹） |
| 20mm火神炮（`MMT_Turret_VulcanCannon`） | (2,2) | 20mm火神炮组件箱×1 + [金属]×100 | — | 2200 | 重型炮塔 III | 0 | 炮塔（火神炮） |
| 14.5mm KPV重机枪（`MMT_TurretKPV`） | (2,2) | 14.5mm KPV重机枪组件箱×1 + [金属]×80 | — | 1500 | 重机枪塔 | 0 | 炮塔（14.5mm KPV重机枪） |

### 武器部件（WeaponParts）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 武器加工台（`MMT_WeaponCraftingTable`） | (3,1) | 工业零件×15 + 电子元件×6 + 机械组件×6 + [金属/木质]×140 | 500W | 4000 | 机械加工 | 34 |  |
| 先进武器加工台（`MMT_AdvWeaponCraftingTable`） | (3,1) | 合成橡胶×55 + 工业零件×15 + 机械组件×8 + 集成电路×5 + [金属]×160 | 500W | 6500 | 制造 | 69 |  |
| 未来武器工作台（`MMT_MechWeaponTable`） | (3,1) | 磁性材料×20 + 太空零件×12 + 先进机械组件×5 + 生物电子元件×2 + [金属]×190 | 1800W | 11000 | 充能射击 | 19 |  |

**武器加工台（`MMT_WeaponCraftingTable`） 的配方：**

- `MMT_MakeWeaponParts`：[MMT_MetallicBar](25) + 工业零件(10) + 机械组件(5) -> 武器部件×5 ｜ 工作量=1500 ｜ 研究:机械加工 ｜ 技能:制作≥6
- `MMT_MakePistolComponent`：[MMT_SLDBar]/[MMT_USLDBar](6) + [MMT_HCMBar](3) + 工业零件(2) + 电线(1) + 机械组件(1) -> 手枪部件×1 ｜ 工作量=1200 ｜ 研究:枪械制造 ｜ 技能:制作≥6
- `MMT_MakeSMGComponent`：[MMT_SLDBar]/[MMT_USLDBar](10) + [MMT_HCMBar](5) + 工业零件(4) + 电线(6) + 机械组件(2) -> 冲锋枪部件×1 ｜ 工作量=1800 ｜ 研究:反冲作用 ｜ 技能:制作≥9
- `MMT_MakeShotgunComponent`：[MMT_SLDBar]/[MMT_USLDBar](15) + [MMT_HCMBar](8) + 工业零件(4) + 电线(5) + 机械组件(2) -> 霰弹枪部件×1 ｜ 工作量=1800 ｜ 研究:导气式 ｜ 技能:制作≥7
- `MMT_MakeRifleComponent`：[MMT_SLDBar]/[MMT_USLDBar](20) + [MMT_HCMBar](10) + 工业零件(6) + 电线(8) + 机械组件(3) -> 步枪部件×1 ｜ 工作量=2100 ｜ 研究:导气式 ｜ 技能:制作≥8
- `MMT_MakeSniperComponent`：步枪部件(1) + 工业零件(6) + 玻璃块(12) -> 狙击枪部件×1 ｜ 工作量=2100 ｜ 研究:精密膛线 ｜ 技能:制作≥10
- `MMT_BuildKPVTurret_Crate`：武器部件(17) + 重型武器部件(1) + [MMT_MetallicBar](50) -> 14.5mm KPV重机枪组件箱×1 ｜ 工作量=11500 ｜ 研究:重机枪塔 ｜ 技能:制作≥7
- `MMT_Build93RBurstPistol`：手枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](5) -> 伯莱塔93R冲锋手枪×1 ｜ 工作量=9300 ｜ 研究:枪械制造 ｜ 技能:制作≥7
- `MMT_BuildCXfour`：步枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(10) + [Leathers](6) -> 伯莱塔Cx4卡宾枪×1 ｜ 工作量=14000 ｜ 研究:枪械 II ｜ 技能:制作≥8
- `MMT_BuildDeringerPistol`：手枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_MetallicBar](15) + [Leathers](5) -> M1917史密斯威森左轮×1 ｜ 工作量=8500 ｜ 研究:枪械制造 ｜ 技能:制作≥3
- `MMT_BuildFireExtinguisher`：武器部件(3) + 工业零件(4) + 表面活性剂(30) + [MMT_MetallicBar](20) -> 灭火器×1 ｜ 工作量=8800 ｜ 技能:制作≥6
- `MMT_BuildFlamethrowerRM`：重型武器部件(1) + 武器部件(6) + 工业零件(7) + [MMT_MetallicBar](20) + 塑料(16) -> RM2喷火器×1 ｜ 工作量=17000 ｜ 研究:重型炮塔 ｜ 技能:制作≥12
- `MMT_BuildFlareGun`：武器部件(3) + 工业零件(4) + [MMT_MetallicBar](20) -> 信号枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥6
- `MMT_BuildGun_BoltActionRifle`：步枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](10) -> Gun_BoltActionRifle×1 ｜ 工作量=12200 ｜ 研究:导气式 ｜ 技能:制作≥3
- `MMT_BuildHandGun`：手枪部件(1) + 武器部件(1) + 工业零件(4) + [MMT_MetallicBar](10) + [Leathers](5) -> Gun_Revolver×1 ｜ 工作量=8500 ｜ 研究:枪械制造 ｜ 技能:制作≥3
- `MMT_BuildHeavySMG`：冲锋枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](12) -> Gun_HeavySMG×1 ｜ 工作量=15000 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildM37A2_USCM`：霰弹枪部件(1) + 武器部件(2) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> M37A2泵动霰弹枪×1 ｜ 工作量=11800 ｜ 研究:导气式 ｜ 技能:制作≥6
- `MMT_BuildM3SMG`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](16) + [Leathers](6) -> M3“注油枪”冲锋枪×1 ｜ 工作量=13600 ｜ 研究:反冲作用 ｜ 技能:制作≥5
- `MMT_BuildM42A_USCM`：狙击枪部件(1) + 武器部件(9) + 工业零件(11) + [MMT_MetallicBar](20) + 塑料(26) + [Leathers](7) -> M42A狙击步枪×1 ｜ 工作量=16900 ｜ 研究:精密膛线 ｜ 技能:制作≥13
- `MMT_BuildM4A1Gun`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](15) + 塑料(10) + [Leathers](11) -> M4A1卡宾枪×1 ｜ 工作量=15000 ｜ 研究:枪械 II ｜ 技能:制作≥7
- `MMT_BuildM79GL`：步枪部件(2) + 武器部件(3) + 工业零件(3) + [MMT_MetallicBar](20) + [MMT_Woody](20) + [Leathers](10) -> M79榴弹发射器×1 ｜ 工作量=15000 ｜ 研究:枪械 II ｜ 技能:制作≥9
- `MMT_BuildMP5Gun`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](6) -> MP5A3冲锋枪×1 ｜ 工作量=14600 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildPDW`：冲锋枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](10) -> Gun_MachinePistol×1 ｜ 工作量=13600 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildParaOrdnancePFourteenFortyfive`：手枪部件(1) + 武器部件(4) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](10) -> Gun_Autopistol×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥7
- `MMT_BuildPumpShotgun`：霰弹枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> Gun_PumpShotgun×1 ｜ 工作量=12000 ｜ 研究:导气式 ｜ 技能:制作≥6
- `MMT_BuildSkorpionSMG`：冲锋枪部件(1) + 武器部件(3) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](6) -> MP5K冲锋枪×1 ｜ 工作量=15600 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildSniperRifle`：狙击枪部件(1) + 武器部件(9) + 工业零件(9) + [MMT_MetallicBar](15) + 塑料(22) + [Leathers](7) -> Gun_SniperRifle×1 ｜ 工作量=16600 ｜ 研究:精密膛线 ｜ 技能:制作≥13
- `MMT_BuildSphinxPistol`：手枪部件(1) + 武器部件(4) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(10) + [Leathers](5) -> 司芬克斯2000手枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥9
- `MMT_BuildTMPMachinePistol`：冲锋枪部件(1) + 武器部件(2) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> TMP消声冲锋枪×1 ｜ 工作量=15000 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildTecnine`：手枪部件(1) + 武器部件(5) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](12) -> TEC-9冲锋手枪×1 ｜ 工作量=9200 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildUMP45Gun`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](16) + 塑料(15) + [Leathers](6) -> UMP45冲锋枪×1 ｜ 工作量=14900 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildUSC`：步枪部件(1) + 武器部件(6) + 工业零件(5) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](7) -> HK USC半自动卡宾枪×1 ｜ 工作量=14000 ｜ 研究:枪械 II ｜ 技能:制作≥7
- `MMT_BuildVector`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](6) -> K10“短剑”冲锋枪×1 ｜ 工作量=15600 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildVektorCP`：手枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_SLDBar]/[MMT_USLDBar](15) + [Leathers](6) -> 维克多CP1手枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥8

**先进武器加工台（`MMT_AdvWeaponCraftingTable`） 的配方：**

- `MMT_MakeWeaponParts`：[MMT_MetallicBar](25) + 工业零件(10) + 机械组件(5) -> 武器部件×5 ｜ 工作量=1500 ｜ 研究:机械加工 ｜ 技能:制作≥6
- `MMT_MakePistolComponent`：[MMT_SLDBar]/[MMT_USLDBar](6) + [MMT_HCMBar](3) + 工业零件(2) + 电线(1) + 机械组件(1) -> 手枪部件×1 ｜ 工作量=1200 ｜ 研究:枪械制造 ｜ 技能:制作≥6
- `MMT_MakeSMGComponent`：[MMT_SLDBar]/[MMT_USLDBar](10) + [MMT_HCMBar](5) + 工业零件(4) + 电线(6) + 机械组件(2) -> 冲锋枪部件×1 ｜ 工作量=1800 ｜ 研究:反冲作用 ｜ 技能:制作≥9
- `MMT_MakeShotgunComponent`：[MMT_SLDBar]/[MMT_USLDBar](15) + [MMT_HCMBar](8) + 工业零件(4) + 电线(5) + 机械组件(2) -> 霰弹枪部件×1 ｜ 工作量=1800 ｜ 研究:导气式 ｜ 技能:制作≥7
- `MMT_MakeRifleComponent`：[MMT_SLDBar]/[MMT_USLDBar](20) + [MMT_HCMBar](10) + 工业零件(6) + 电线(8) + 机械组件(3) -> 步枪部件×1 ｜ 工作量=2100 ｜ 研究:导气式 ｜ 技能:制作≥8
- `MMT_MakeSniperComponent`：步枪部件(1) + 工业零件(6) + 玻璃块(12) -> 狙击枪部件×1 ｜ 工作量=2100 ｜ 研究:精密膛线 ｜ 技能:制作≥10
- `MMT_MakeAdvRifleComponent`：人造纤维(5) + 塑料(15) + 合成橡胶(15) + 步枪部件(1) + 先进机械组件(2) -> 先进步枪部件×1 ｜ 工作量=2100 ｜ 研究:制造 ｜ 技能:制作≥13
- `MMT_MakeAdvSniperComponent`：人造纤维(10) + 塑料(20) + 合成橡胶(15) + 集成电路(4) + 狙击枪部件(2) + 先进机械组件(3) -> 先进狙击枪部件×1 ｜ 工作量=2100 ｜ 研究:制造 ｜ 技能:制作≥15
- `MMT_MakeHeavyComponent`：[MMT_SLDBar]/[MMT_USLDBar](25) + [MMT_HCMBar](15) + 工业零件(12) + 电线(15) + 机械组件(6) -> 重型武器部件×1 ｜ 工作量=2800 ｜ 研究:重型炮塔 ｜ 技能:制作≥12
- `MMT_MakeCannonComponent`：[MMT_SLDBar]/[MMT_USLDBar](20) + 重型武器部件(1) + 先进机械组件(5) -> 火炮武器部件×1 ｜ 工作量=3100 ｜ 研究:迫击炮 ｜ 技能:制作≥14
- `MMT_MakeLauncherComponent`：集成电路(2) + 重型武器部件(1) + 先进机械组件(3) + 玻璃块(12) -> 发射器部件×1 ｜ 工作量=3100 ｜ 研究:火箭弹 I ｜ 技能:制作≥14
- `MMT_Build93RBurstPistol`：手枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](5) -> 伯莱塔93R冲锋手枪×1 ｜ 工作量=9300 ｜ 研究:枪械制造 ｜ 技能:制作≥7
- `MMT_BuildAA12AutoShotgun`：霰弹枪部件(1) + 武器部件(8) + 工业零件(6) + [MMT_MetallicBar](22) + 塑料(20) + [Leathers](9) -> AA-12自动霰弹枪×1 ｜ 工作量=19000 ｜ 研究:枪械 II ｜ 技能:制作≥14
- `MMT_BuildAK47Gun`：步枪部件(1) + 武器部件(6) + 工业零件(8) + [MMT_MetallicBar](26) + 塑料(15) + [Leathers](11) -> AK-47突击步枪×1 ｜ 工作量=19000 ｜ 研究:步枪 II ｜ 技能:制作≥12
- `MMT_BuildARFifty`：先进狙击枪部件(1) + 武器部件(8) + 工业零件(12) + [MMT_MetallicBar](36) + 塑料(35) + [Leathers](12) -> AR-50狙击步枪×1 ｜ 工作量=25000 ｜ 研究:狙击步枪 III ｜ 技能:制作≥18
- `MMT_BuildATR`：狙击枪部件(1) + 武器部件(9) + 工业零件(7) + [MMT_MetallicBar](25) + 塑料(20) -> PTRD反坦克步枪×1 ｜ 工作量=21000 ｜ 研究:狙击步枪 III ｜ 技能:制作≥15
- `MMT_BuildAUG`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](16) + 塑料(20) + [Leathers](10) -> 斯太尔AUG突击步枪×1 ｜ 工作量=19500 ｜ 研究:步枪 III ｜ 技能:制作≥15
- `MMT_BuildAssaultRifle`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](16) + 塑料(15) + [Leathers](10) -> Gun_AssaultRifle×1 ｜ 工作量=18000 ｜ 研究:步枪 II ｜ 技能:制作≥13
- `MMT_BuildCXfour`：步枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(10) + [Leathers](6) -> 伯莱塔Cx4卡宾枪×1 ｜ 工作量=14000 ｜ 研究:枪械 II ｜ 技能:制作≥8
- `MMT_BuildDeringerPistol`：手枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_MetallicBar](15) + [Leathers](5) -> M1917史密斯威森左轮×1 ｜ 工作量=8500 ｜ 研究:枪械制造 ｜ 技能:制作≥3
- `MMT_BuildFAMAS`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](16) + 塑料(15) + [Leathers](10) -> 法玛斯G2突击步枪×1 ｜ 工作量=17500 ｜ 研究:步枪 II ｜ 技能:制作≥13
- `MMT_BuildFireExtinguisher`：武器部件(3) + 工业零件(4) + 表面活性剂(30) + [MMT_MetallicBar](20) -> 灭火器×1 ｜ 工作量=8800 ｜ 技能:制作≥6
- `MMT_BuildFlamethrowerRM`：重型武器部件(1) + 武器部件(6) + 工业零件(7) + [MMT_MetallicBar](20) + 塑料(16) -> RM2喷火器×1 ｜ 工作量=17000 ｜ 研究:重型炮塔 ｜ 技能:制作≥12
- `MMT_BuildFlareGun`：武器部件(3) + 工业零件(4) + [MMT_MetallicBar](20) -> 信号枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥6
- `MMT_BuildGalilBurstRifle`：重型武器部件(1) + 武器部件(4) + 工业零件(7) + [MMT_MetallicBar](22) + 塑料(20) + [Leathers](8) -> 加利尔ACE轻机枪×1 ｜ 工作量=20500 ｜ 研究:多管武器 ｜ 技能:制作≥8
- `MMT_BuildGun_BoltActionRifle`：步枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](10) -> Gun_BoltActionRifle×1 ｜ 工作量=12200 ｜ 研究:导气式 ｜ 技能:制作≥3
- `MMT_BuildHandGun`：手枪部件(1) + 武器部件(1) + 工业零件(4) + [MMT_MetallicBar](10) + [Leathers](5) -> Gun_Revolver×1 ｜ 工作量=8500 ｜ 研究:枪械制造 ｜ 技能:制作≥3
- `MMT_BuildHeavyCombatRifle`：重型武器部件(1) + 武器部件(6) + 工业零件(8) + [MMT_MetallicBar](26) + 塑料(20) + [Leathers](12) -> MA39X突击步枪×1 ｜ 工作量=20500 ｜ 研究:多管武器 ｜ 技能:制作≥14
- `MMT_BuildHeavySMG`：冲锋枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](12) -> Gun_HeavySMG×1 ｜ 工作量=15000 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildHecateII`：先进狙击枪部件(1) + 武器部件(12) + 工业零件(16) + [MMT_MetallicBar](55) + 塑料(35) + [Leathers](12) -> 黑卡蒂II反器材狙击步枪×1 ｜ 工作量=25000 ｜ 研究:狙击步枪 III ｜ 技能:制作≥17
- `MMT_BuildIncendiaryLauncher`：重型武器部件(1) + 武器部件(6) + 工业零件(7) + [MMT_MetallicBar](20) + 塑料(16) + [Leathers](12) -> Gun_IncendiaryLauncher×1 ｜ 工作量=19000 ｜ 研究:发射器 I ｜ 技能:制作≥14
- `MMT_BuildLAW`：发射器部件(2) + 武器部件(5) + 工业零件(8) + [MMT_MetallicBar](30) + 塑料(30) -> M72R反坦克火箭筒×1 ｜ 工作量=22000 ｜ 研究:火箭弹 I ｜ 技能:制作≥16
- `MMT_BuildLMG`：重型武器部件(1) + 武器部件(5) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](15) -> Gun_LMG×1 ｜ 工作量=19500 ｜ 研究:重型炮塔 ｜ 技能:制作≥14
- `MMT_BuildM249`：重型武器部件(1) + 武器部件(8) + 工业零件(9) + [MMT_MetallicBar](30) + 塑料(25) -> M249班用机枪×1 ｜ 工作量=23000 ｜ 研究:重型炮塔 ｜ 技能:制作≥12
- `MMT_BuildM37A2_USCM`：霰弹枪部件(1) + 武器部件(2) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> M37A2泵动霰弹枪×1 ｜ 工作量=11800 ｜ 研究:导气式 ｜ 技能:制作≥6
- `MMT_BuildM3SMG`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](16) + [Leathers](6) -> M3“注油枪”冲锋枪×1 ｜ 工作量=13600 ｜ 研究:反冲作用 ｜ 技能:制作≥5
- `MMT_BuildM42A_USCM`：狙击枪部件(1) + 武器部件(9) + 工业零件(11) + [MMT_MetallicBar](20) + 塑料(26) + [Leathers](7) -> M42A狙击步枪×1 ｜ 工作量=16900 ｜ 研究:精密膛线 ｜ 技能:制作≥13
- `MMT_BuildM4A1Gun`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](15) + 塑料(10) + [Leathers](11) -> M4A1卡宾枪×1 ｜ 工作量=15000 ｜ 研究:枪械 II ｜ 技能:制作≥7
- `MMT_BuildM56_USCM`：重型武器部件(1) + 武器部件(7) + 高级零部件(4) + [MMT_MetallicBar](20) + 塑料(20) + [Leathers](12) -> M56智能机枪×1 ｜ 工作量=19900 ｜ 研究:多管武器 ｜ 技能:制作≥16
- `MMT_BuildM5_USCM`：发射器部件(2) + 武器部件(6) + 工业零件(7) + [MMT_MetallicBar](26) + 塑料(25) -> M5火箭筒×1 ｜ 工作量=23000 ｜ 研究:火箭弹 I ｜ 技能:制作≥15
- `MMT_BuildM79GL`：步枪部件(2) + 武器部件(3) + 工业零件(3) + [MMT_MetallicBar](20) + [MMT_Woody](20) + [Leathers](10) -> M79榴弹发射器×1 ｜ 工作量=15000 ｜ 研究:枪械 II ｜ 技能:制作≥9
- `MMT_BuildM82Gun`：先进狙击枪部件(1) + 武器部件(6) + 工业零件(15) + [MMT_MetallicBar](26) + 塑料(25) + [Leathers](12) -> M82A1反器材步枪×1 ｜ 工作量=22500 ｜ 研究:狙击步枪 III ｜ 技能:制作≥17
- `MMT_BuildMP5Gun`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](6) -> MP5A3冲锋枪×1 ｜ 工作量=14600 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildMarineSturmRifle`：步枪部件(1) + 武器部件(5) + 工业零件(6) + [MMT_MetallicBar](22) + 塑料(20) -> TAR-21突击步枪×1 ｜ 工作量=21000 ｜ 研究:步枪 III ｜ 技能:制作≥12
- `MMT_BuildMilkorGrenadeLauncher`：重型武器部件(1) + 武器部件(7) + 工业零件(7) + [MMT_MetallicBar](26) + 塑料(25) + [Leathers](10) -> 米尔科姆榴弹发射器×1 ｜ 工作量=21000 ｜ 研究:发射器 I ｜ 技能:制作≥13
- `MMT_BuildMinigun`：重型武器部件(1) + 武器部件(13) + 高级零部件(10) + [MMT_MetallicBar](36) + 塑料(25) + [Leathers](10) -> Gun_Minigun×1 ｜ 工作量=28000 ｜ 研究:多管武器 ｜ 技能:制作≥18
- `MMT_BuildPDW`：冲锋枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](10) -> Gun_MachinePistol×1 ｜ 工作量=13600 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildParaOrdnancePFourteenFortyfive`：手枪部件(1) + 武器部件(4) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](10) -> Gun_Autopistol×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥7
- `MMT_BuildPumpShotgun`：霰弹枪部件(1) + 武器部件(1) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> Gun_PumpShotgun×1 ｜ 工作量=12000 ｜ 研究:导气式 ｜ 技能:制作≥6
- `MMT_BuildRPG`：发射器部件(1) + 武器部件(7) + 工业零件(7) + [MMT_MetallicBar](26) + 塑料(25) -> RPG-7火箭筒×1 ｜ 工作量=23000 ｜ 研究:火箭弹 I ｜ 技能:制作≥13
- `MMT_BuildSCAR_H`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](16) + 塑料(20) + [Leathers](10) -> FN SCAR-H突击步枪×1 ｜ 工作量=19200 ｜ 研究:步枪 III ｜ 技能:制作≥15
- `MMT_BuildSG553Gun`：步枪部件(1) + 武器部件(4) + 工业零件(5) + [MMT_MetallicBar](16) + 塑料(20) + [Leathers](10) -> 西格SG553突击步枪×1 ｜ 工作量=19500 ｜ 研究:步枪 III ｜ 技能:制作≥15
- `MMT_BuildSkorpionSMG`：冲锋枪部件(1) + 武器部件(3) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](6) -> MP5K冲锋枪×1 ｜ 工作量=15600 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildSniperRifle`：狙击枪部件(1) + 武器部件(9) + 工业零件(9) + [MMT_MetallicBar](15) + 塑料(22) + [Leathers](7) -> Gun_SniperRifle×1 ｜ 工作量=16600 ｜ 研究:精密膛线 ｜ 技能:制作≥13
- `MMT_BuildSphinxPistol`：手枪部件(1) + 武器部件(4) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(10) + [Leathers](5) -> 司芬克斯2000手枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥9
- `MMT_BuildStrikerShotgun`：霰弹枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](8) -> Gun_ChainShotgun×1 ｜ 工作量=16200 ｜ 研究:枪械 II ｜ 技能:制作≥10
- `MMT_BuildTAMSR`：先进狙击枪部件(1) + 武器部件(6) + 工业零件(15) + [MMT_MetallicBar](26) + 塑料(25) + [Leathers](12) -> SRS99反器材狙击步枪×1 ｜ 工作量=23000 ｜ 研究:狙击步枪 III ｜ 技能:制作≥18
- `MMT_BuildTMPMachinePistol`：冲锋枪部件(1) + 武器部件(2) + 工业零件(6) + [MMT_MetallicBar](20) + [Leathers](7) -> TMP消声冲锋枪×1 ｜ 工作量=15000 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildTacticalAutoshotgun`：霰弹枪部件(1) + 武器部件(9) + 工业零件(8) + [MMT_MetallicBar](28) + 塑料(20) + [Leathers](9) -> M90近程突击霰弹枪×1 ｜ 工作量=19000 ｜ 研究:枪械 II ｜ 技能:制作≥14
- `MMT_BuildTacticalCombatrifle`：步枪部件(1) + 武器部件(7) + 工业零件(9) + [MMT_MetallicBar](26) + 塑料(20) + [Leathers](10) -> MA5 ICWS突击步枪×1 ｜ 工作量=20000 ｜ 研究:步枪 III ｜ 技能:制作≥13
- `MMT_BuildTacticalShotgun`：霰弹枪部件(1) + 武器部件(6) + 工业零件(5) + [MMT_MetallicBar](20) + 塑料(15) + [Leathers](8) -> M45战术霰弹枪×1 ｜ 工作量=17500 ｜ 研究:枪械 II ｜ 技能:制作≥12
- `MMT_BuildTecnine`：手枪部件(1) + 武器部件(5) + 工业零件(3) + [MMT_MetallicBar](12) + 塑料(12) + [Leathers](12) -> TEC-9冲锋手枪×1 ｜ 工作量=9200 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildUMP45Gun`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](16) + 塑料(15) + [Leathers](6) -> UMP45冲锋枪×1 ｜ 工作量=14900 ｜ 研究:反冲作用 ｜ 技能:制作≥6
- `MMT_BuildUSC`：步枪部件(1) + 武器部件(6) + 工业零件(5) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](7) -> HK USC半自动卡宾枪×1 ｜ 工作量=14000 ｜ 研究:枪械 II ｜ 技能:制作≥7
- `MMT_BuildVector`：冲锋枪部件(1) + 武器部件(4) + 工业零件(4) + [MMT_MetallicBar](15) + 塑料(15) + [Leathers](6) -> K10“短剑”冲锋枪×1 ｜ 工作量=15600 ｜ 研究:反冲作用 ｜ 技能:制作≥7
- `MMT_BuildVektorCP`：手枪部件(1) + 武器部件(2) + 工业零件(4) + [MMT_SLDBar]/[MMT_USLDBar](15) + [Leathers](6) -> 维克多CP1手枪×1 ｜ 工作量=8800 ｜ 研究:枪械制造 ｜ 技能:制作≥8
- `MMT_BuildWA2000SpecialistRifle`：狙击枪部件(1) + 武器部件(12) + 工业零件(12) + [MMT_MetallicBar](25) + 塑料(20) + [Leathers](11) -> 瓦尔特WA2000狙击步枪×1 ｜ 工作量=21000 ｜ 研究:精密膛线 ｜ 技能:制作≥16
- `MMT_BuildXM8MilitaryRifle`：步枪部件(1) + 武器部件(6) + 工业零件(8) + [MMT_MetallicBar](22) + 塑料(20) + [Leathers](11) -> XM8军用步枪×1 ｜ 工作量=20000 ｜ 研究:步枪 III ｜ 技能:制作≥14
- `MMT_Build_M60GPMG`：重型武器部件(1) + 武器部件(8) + 工业零件(10) + [MMT_MetallicBar](35) + 塑料(30) -> M60E4通用机枪×1 ｜ 工作量=24000 ｜ 研究:多管武器 ｜ 技能:制作≥13

**未来武器工作台（`MMT_MechWeaponTable`） 的配方：**

- `MMT_MakeWeaponParts`：[MMT_MetallicBar](25) + 工业零件(10) + 机械组件(5) -> 武器部件×5 ｜ 工作量=1500 ｜ 研究:机械加工 ｜ 技能:制作≥6
- `MMT_MakeChargedComponent`：[MMT_USLDBar](10) + [MMT_HCMBar](20) + 磁性材料(2) + 先进机械组件(2) -> 电荷武器部件×1 ｜ 工作量=2100 ｜ 研究:充能射击 ｜ 技能:制作≥14
- `MMT_MakeLaserComponent`：[MMT_USLDBar](20) + [MMT_HCMBar](10) + 磁性材料(4) + 微芯片集成电路(1) -> 光束武器部件×1 ｜ 工作量=2100 ｜ 研究:充能射击 ｜ 技能:制作≥15
- `MMT_MakePlasmaComponent`：[MMT_USLDBar](20) + [MMT_HCMBar](10) + 生物合成材料(4) + 微芯片集成电路(1) -> 离子武器部件×1 ｜ 工作量=2100 ｜ 研究:充能射击 ｜ 技能:制作≥15
- `MMT_MakeFusionGrenades`：[MMT_MetallicBar](15) + 武器部件(3) + 高级零部件(3) + 光束武器部件(1) -> 温压手榴弹×5 ｜ 工作量=7000 ｜ 研究:充能射击 ｜ 技能:制作≥14
- `MMT_MakeGrenadeChargedFrag`：[MMT_MetallicBar](15) + 武器部件(3) + 高级零部件(3) + 电荷武器部件(1) -> 电荷手榴弹×5 ｜ 工作量=7000 ｜ 研究:充能射击 ｜ 技能:制作≥14
- `MMT_MakePlasmaGrenades`：[MMT_MetallicBar](15) + 武器部件(3) + 高级零部件(3) + 离子武器部件(1) -> 等离子手榴弹×5 ｜ 工作量=7000 ｜ 研究:充能射击 ｜ 技能:制作≥14
- `MMT_BuildBlackWidow`：光束武器部件(1) + 动力电池(1) + 武器部件(7) + 超级零部件(8) + [MMT_USLDBar](20) + 塑料(35) -> A-1BW“黑寡妇”×1 ｜ 工作量=27000 ｜ 研究:激光武器 II ｜ 技能:制作≥17
- `MMT_BuildBurstLaserRifle`：光束武器部件(1) + 武器部件(9) + 超级零部件(9) + [MMT_USLDBar](25) + 塑料(40) -> M12“仲裁者”光束枪×1 ｜ 工作量=25000 ｜ 研究:激光武器 II ｜ 技能:制作≥17
- `MMT_BuildChargeRifle`：电荷武器部件(1) + 武器部件(5) + 高级零部件(10) + [MMT_USLDBar](20) + 塑料(27) -> Gun_ChargeRifle×1 ｜ 工作量=27000 ｜ 研究:激光武器 I ｜ 技能:制作≥14
- `MMT_BuildHeavyLaserRifle`：光束武器部件(1) + 武器部件(8) + 超级零部件(9) + [MMT_USLDBar](22) + 塑料(35) -> M10“铆钉”光束枪×1 ｜ 工作量=24000 ｜ 研究:激光武器 II ｜ 技能:制作≥16
- `MMT_BuildLaserRifle`：光束武器部件(1) + 武器部件(6) + 高级零部件(9) + [MMT_USLDBar](20) + 塑料(35) -> M8“亡魂”光束枪×1 ｜ 工作量=23000 ｜ 研究:激光武器 I ｜ 技能:制作≥15
- `MMT_BuildMantis`：光束武器部件(1) + 武器部件(7) + 高级零部件(9) + [MMT_USLDBar](20) + 塑料(35) -> Gun_ChargeLance×1 ｜ 工作量=24000 ｜ 研究:激光武器 I ｜ 技能:制作≥14
- `MMT_BuildPlasmaCarbine`：离子武器部件(1) + 武器部件(6) + 高级零部件(10) + [MMT_USLDBar](25) + 塑料(35) -> C3“巫师”×1 ｜ 工作量=26000 ｜ 研究:激光武器 I ｜ 技能:制作≥15
- `MMT_BuildPlasmaPistol`：离子武器部件(1) + 武器部件(4) + 高级零部件(8) + [MMT_USLDBar](12) + 塑料(20) -> C1“晋升”×1 ｜ 工作量=18000 ｜ 研究:激光武器 I ｜ 技能:制作≥14
- `MMT_BuildPlasmaPrecision`：离子武器部件(1) + 武器部件(6) + 超级零部件(10) + [MMT_USLDBar](22) + 塑料(35) -> C2“月咒”×1 ｜ 工作量=25000 ｜ 研究:激光武器 II ｜ 技能:制作≥16
- `MMT_BuildPlasmaRifle`：离子武器部件(1) + 武器部件(6) + 超级零部件(10) + [MMT_USLDBar](22) + 塑料(35) -> C7“灾引”×1 ｜ 工作量=27000 ｜ 研究:激光武器 II ｜ 技能:制作≥17
- `MMT_BuildPlasmaShotgun`：离子武器部件(1) + 武器部件(7) + 超级零部件(11) + [MMT_USLDBar](22) + 塑料(35) -> C5“魔咒”×1 ｜ 工作量=26000 ｜ 研究:激光武器 II ｜ 技能:制作≥17
- `MMT_BuildSplitter`：电荷武器部件(1) + 武器部件(6) + 超级零部件(12) + 斯特合金(7) + [MMT_USLDBar](22) + 塑料(35) -> R6“撕裂者”电荷步枪×1 ｜ 工作量=30000 ｜ 研究:激光武器 II ｜ 技能:制作≥15

### 木材（Wood）

| 建筑 | 尺寸 | 造价 | 功耗 | 建造工作量 | 研究前置 | 配方数 | 备注 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 手工锯台（`MMT_HandSawmill`） | (3,1) | [木质]×120 | — | 700 | 木工加工 | 4 |  |
| 窑炉（`MMT_CharcoalKiln`） | (3,3) | [石质/MMT_Bricks]×180 | — | 3000 | 木炭烧制 | 4 |  |

**手工锯台（`MMT_HandSawmill`） 的配方：**

- `MMT_MakeWoodPlanks_Hand`：原木(10) -> 木板×15 ｜ 工作量=1200 ｜ 研究:木工加工 ｜ 技能:制作≥3
- `MMT_MakeKindling_Hand`：原木(10) -> 柴火×20 ｜ 工作量=500 ｜ 研究:木工加工
- `MMT_MakeBambooPlanks_Hand`：竹节(10) -> 竹板×15 ｜ 工作量=1200 ｜ 研究:木工加工 ｜ 技能:制作≥6
- `MMT_MakeRedWoodPlanks_Hand`：红木(10) -> 红木木板×15 ｜ 工作量=1200 ｜ 研究:木工加工 ｜ 技能:制作≥3

**窑炉（`MMT_CharcoalKiln`） 的配方：**

- `MMT_MakeCharcoal_Hand`：原木(25) -> 木炭×20 ｜ 工作量=1600 ｜ 研究:木炭烧制
- `MMT_MakeCeramics`：软粘土(20) + 玻璃生料(5) -> 陶瓷×10 ｜ 工作量=1200 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥8
- `MMT_MakeClayBricks`：软粘土(15) -> 粘土砖×15 ｜ 工作量=800 ｜ 研究:基础金属冶炼 ｜ 技能:制作≥6
- `MMT_BurnThings`：原木/[Textiles]/[Leathers](20) -> 灰烬×3 ｜ 工作量=400

## 2. HSK 有而 MMT 未实现的生产建筑

共 19 座。中文名取自 HSK 汉化（Core_SK DefInjected），无翻译时保留英文 label。

| 建筑 | defName | 尺寸 | 造价 | 功耗 | 配方数 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- |
| LeatherworkerTable | `LeatherworkerTable` | (3,1) | [石质/砖]×80 | — | 1 | Buildings_Production_Apparel.xml |
| TableRecycling | `TableRecycling` | (3,1) | 工业零件×12 + ElectronicComponents×3 + Mechanism×5 + [粗金属]×120 | 500W | 3 | Buildings_Production_Apparel.xml |
| HolyBasin | `HolyBasin` | — | ComponentMedieval×3 + [金属]×70 | — | 1 | Buildings_Production_Apparel.xml |
| TableKitchen | `TableKitchen` | (3,1) | ComponentMedieval×8 + Mechanism×2 + [坚固金属/木质]×115 | — | 12 | Buildings_Production_Food.xml |
| OpenHearth | `OpenHearth` | (1,1) | ComponentMedieval×6 + [石质/砖]×90 | — | 5 | Buildings_Production_Food.xml |
| ElectricStove_Pro | `ElectricStove_Pro` | (4,1) | 工业零件×20 + Rubber×30 + Electronics×5 + AdvMechanism×3 + [坚固金属]×155 | 500W | 2 | Buildings_Production_Food.xml |
| Canningstove | `Canningstove` | (1,1) | 工业零件×12 + ElectronicComponents×3 + [坚固金属]×115 | 250W | 4 | Buildings_Production_Food.xml |
| GrindStone | `GrindStone` | (1,1) | [石质/砖]×70 | — | 3 | Buildings_Production_Food.xml |
| MillElectric | `MillElectric` | (1,1) | Rubber×45 + Glass×25 + 工业零件×8 + Mechanism×5 + [坚固金属/塑料]×50 | 300W | 3 | Buildings_Production_Food.xml |
| TableOven | `TableOven` | (2,2) | ComponentMedieval×5 + [石质/砖]×120 | — | 5 | Buildings_Production_Food.xml |
| ElectricOven | `ElectricOven` | (1,1) | 工业零件×10 + ElectronicComponents×3 + Mechanism×3 + [坚固金属]×100 | 400W | 8 | Buildings_Production_Food.xml |
| soylenttable | `soylenttable` | (2,1) | Rubber×50 + Plastic×40 + 工业零件×10 + Electronics×5 + [坚固金属]×130 | 500W | 1 | Buildings_Production_Food.xml |
| UniversalFermenter | `UniversalFermenter` | (1,1) | ComponentMedieval×2 + [木质]×50 | — | 0 | Buildings_Production_Food.xml |
| JerkyRack | `JerkyRack` | (2,1) | ComponentMedieval×2 + [木质]×60 | — | 0 | Buildings_Production_Food.xml |
| CheeseBasin | `CheeseBasin` | (1,1) | ComponentMedieval×2 + [木质]×40 | — | 0 | Buildings_Production_Food.xml |
| Kiln | `Kiln` | (3,3) | [石质/砖]×180 | — | 8 | Buildings_Production_Furnaces.xml |
| HadronColliderFeeder | `HadronColliderFeeder` | (1,1) | 太空零件×2 + [坚固金属]×20 | — | 0 | Buildings_Production_Futuristic.xml |
| CompostBarrel | `CompostBarrel` | — | ComponentMedieval×2 + [坚固金属/木质]×75 | — | 0 | Buildings_Production_UF_Processors.xml |
| TableHeavyAmmunition | `TableHeavyAmmunition` | (3,1) | 工业零件×10 + ElectronicComponents×5 + Mechanism×10 + [粗金属]×210 | 500W | 3 | Buildings_Production_Weapons.xml |

