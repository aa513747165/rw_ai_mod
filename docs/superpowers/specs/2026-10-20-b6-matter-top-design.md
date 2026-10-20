# B6 无机物转化 + 顶级材料 设计

- 日期：2026-10-20
- 状态：已审阅（2026-10-20 用户确认：生物质双配方；反物质无机物50+铀20→2、对撞台15000W/量子台8000W；对撞台 7×2）
- 引用：`2026-10-20-b2-b8-batch-roadmap.md` §6（范围、依赖 B4+B5）、§0（全局约定）
- 依赖确认：B4（贫铀/碳纤维锭/合金类别）、B5（石蜡/硫酸盐/人造纤维/塑料）均已落地

## 1. 范围

对照 HSK 3.1/3.2 的物质转换体系与顶级材料：

| 组 | 内容 | HSK 出处 |
|---|---|---|
| 转化器 | 无机物 Matter + ConvertResourcesRaw + 13 条 ConvertTo* | Recipes_Matter.xml / MatterConverter |
| 对撞台 | 反物质 Antimatter（HSK 靠 HadronCollider 自定义类被动产，MMT 改配方制） | Buildings_Production_Futuristic.xml |
| 量子加工台 | α/β 聚合物 | Matterfab + Recipes_HitechCraft.xml |
| 装配器顶级件 | 磁性材料/生物合成材料/生物电子元件/拆 AI 核心 | Recipes_HitechCraft.xml / Recipes_Electronics.xml |
| 生物铁合金 | 挂原版活铁塑造台（决策 1，Anomaly 直接引用） | Recipes_Alloys_Anomaly.xml |

## 2. 新资源（9 个 ThingDef）

新建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Matter.xml`（无机物/反物质/生物质/磁性材料/生物合成材料/生物电子元件）与 `MMT_Items_Resource_SuperAlloys.xml`（α/β 聚合物/生物铁合金）。

通用剔除规则（沿用 B1–B5 教训）：删 `Bulk`（CE）、`MaxBurningTempCelsius`/`BurnDurationHours`（HSK）、`MeleePenetrationFactor`（HSK）；stuffProps.categories 只保留原版 `Metallic`（HSK 的 SolidMetallic/RareMetallic/StrongMetallic/RuggedMetallic/HF 均不可用）。`ElectronicCraftingSpeed`、`PsychicSensitivityFactor`/`PsychicEntropyMaxOffset` 经核实 1.7 原版存在，可保留。

| defName | 中文 label | 关键数值（照 HSK） | 图形 | 类别 |
|---|---|---|---|---|
| MMT_Matter | 无机物 | MV10 Mass0.1 stack150 不燃；发光 comp 蓝光(0,0,217) r1.3 | Single RMatter.png | MMT_SuperMaterialCat |
| MMT_Antimatter | 反物质 | MV120 HP5 stack75 Mass0.1→0.01 易燃5 腐败0.01；CompProperties_Explosive Bomb r1.5 +0.5/stack（原版 comp 照搬）；tradeability None | Single Antimatter.png（HSK HadronCollider 目录） | MMT_SuperMaterialCat |
| MMT_Biomatter | 生物质 | MV10 Mass0.1 stack150 易燃2 SellPriceFactor0.2 tradeTags Exotic | Single Biomatter.png | MMT_SuperMaterialCat |
| MMT_MagneticMaterial | 磁性材料 | MV110 Mass0.05 stack75 易燃0.1 | Single MagneticMaterial.png | MMT_SuperMaterialCat |
| MMT_BiosyntheticMaterial | 生物合成材料 | MV90 Mass0.07 stack75 易燃0.3 | StackCount BiosyntheticMaterial/ | MMT_SuperMaterialCat |
| MMT_BioMicrochips | 生物电子元件 | MV2100 Mass0.4 stack25 易燃1 tradeTags Exotic | Single BioMicrochips.png | MMT_ElectronicsCat |
| MMT_AlphaPoly | α阿尔法聚合物 | MV45 stack75；StuffPower 护甲/隔温/伤害系数照搬；stuffProps（删 HF 等类别、删 MeleePenetrationFactor） | Single APoly.png | MMT_USLDBar |
| MMT_BetaPoly | β贝塔聚合物 | MV65 stack75；同上 | Single BPoly.png | MMT_USLDBar + MMT_USLDHBar |
| MMT_BioferriteAlloy | 生物铁合金 | MV9 Mass0.35 stack600 useHitPoints smeltable；stuffProps 灵能品质因子保留（原版 Anomaly 活铁同款）、HSK 类别剔除仅留 Metallic + Bioferrite（Anomaly 原版 stuff category，该 `<li>` 加 `MayRequire="Ludeon.RimWorld.Anomaly"`） | StackCount Bioferrite/（_A/_B/_C 复制后改小写 _a/_b/_c） | MMT_SLDBar |

## 3. 新建筑（3 个，`MMT_Buildings_Matter.xml`）

均 `ParentName="BuildingBase"` + `thingClass Building_WorkTable` + `designationCategory=MMT_Production`（沿用 MMT_RoboticAssembler 模式；SK 动画类/IdlePower/hopper 均不可用，用原版 PowerTrader 恒定耗电）。

| defName | 中文 | 尺寸 | 造价 | 耗电 W | 研究前置 | 建造技能 |
|---|---|---|---|---|---|---|
| MMT_MatterConverter | 无机物转化器 | 3×3 | 材料280(Metallic) + 人造纤维50 + 太空零件12 + 先进机械组件6 + 微芯片4 | 2500（=HSK 工作功耗） | MMT_MatterConversion1 | 18 |
| MMT_Matterfab | 量子加工台 | 3×2 | 材料240(Metallic) + 磁性材料25 + 太空零件10 + 先进机械组件10 + 微芯片2 | 8000（HSK 工作20000/待机2000，取折中，可审） | MMT_MatterConversion3 | 16 |
| MMT_ParticleCollider | 粒子对撞台 | 7×2 | 材料120(Metallic) + 钢筋混凝土150 + 磁性材料20 + 生物合成材料15 + 先进机械组件12 + 微芯片3 | 15000（HSK 自定义类无参照，自平衡，可审） | MMT_MatterConversion2 | 18 |

其他照 HSK：WorkToBuild 9000/10000/12000，MaxHitPoints 280/350/400，glower（蓝/青/无），HeatPusher 22/85/90，Flickable+Breakdownable。贴图：MC_off 三朝向（动画帧 MC_anim 不复制——无 SK 动画类）、MatterFab 三朝向、HadronCollider.png（Single 7×2）。对撞台不做 hopper 进料器（SK 自定义类）。

## 4. 新配方（21 条）

### 4.1 转化器 14 条（`MMT_Recipes_Matter.xml`，挂 MMT_MatterConverter，workSpeedStat=ElectronicCraftingSpeed，effectWorking/sound=Smelt 系）

| defName | 输入 | 产出 | 工作量 | 研究 |
|---|---|---|---|---|
| MMT_ConvertResourcesRaw | [ResourcesRaw]×300 | MMT_Matter×150 | 10000 | MMT_MatterConversion1 |
| MMT_ConvertToIron | MMT_Matter×50 | MMT_IronOre×100 | 12000 | MMT_MatterConversion2 |
| MMT_ConvertToCopper | ×50 | MMT_CopperOre×100 | 12000 | 同上 |
| MMT_ConvertToTin | ×50 | MMT_TinOre×100 | 12000 | 同上 |
| MMT_ConvertToLead | ×50 | MMT_Anglesite×100 | 12000 | 同上 |
| MMT_ConvertToAluminium | ×50 | MMT_AluminiumOre×80 | 12000 | 同上 |
| MMT_ConvertToNickel | ×150 | MMT_NickelOre×20 | 12000 | 同上 |
| MMT_ConvertToTitanium | ×200 | MMT_IlmeniteOre×20 | 15000 | 同上 |
| MMT_ConvertToGold | ×60 | MMT_GoldOre×20 | 12000 | 同上 |
| MMT_ConvertToSilver | ×100 | MMT_SilverOre×300 | 12000 | 同上 |
| MMT_ConvertToUranium | ×200 | Uranium×50（原版） | 12000 | 同上 |
| MMT_ConvertToPolymers | ×50 | MMT_Polymers×100 | 12000 | 同上 |
| MMT_ConvertToParaffins | ×50 | MMT_Paraffins×100 | 12000 | 同上 |
| MMT_ConvertToSulphates | ×50 | MMT_Sulphates×100 | 12000 | 同上 |

全部技能 制作≥15、workSkill Crafting。金银映射 MMT 自有矿（MMT_GoldOre/MMT_SilverOre），铀用原版 Uranium（B4 已确立）。

### 4.2 装配器 4 条（挂 MMT_RoboticAssembler，`MMT_Recipes_TopMaterials.xml`）

| defName | 输入 | 产出 | 工作量 | 技能 | 研究 |
|---|---|---|---|---|---|
| MMT_BuildMagneticMaterial | MMT_Matter×30 + MMT_Sulphates×15 + [MMT_USLDBar]×10 + [MMT_SLDBar]×5 | MMT_MagneticMaterial×5 | 2400 | 18 | MMT_UltraComponents（对照 Components_D3） |
| MMT_BuildBiosyntheticMaterial | MMT_Biomatter×15 + MMT_SyntheticFibers×9 + MMT_Plastic×30 | MMT_BiosyntheticMaterial×5 | 2400 | 18 | MMT_UltraComponents（HSK 无前置，补挂同上） |
| MMT_MakeBioelectronicDevices | MMT_Biomatter×60 + Hyperweave×15（原版） + MMT_Microchip×2 | MMT_BioMicrochips×1 | 7000 | 18 | MMT_UltraComponents |
| MMT_DisassembleAIPersonaCore | AIPersonaCore×1（原版） | MMT_BioMicrochips×1 + MMT_AlphaPoly×30 + MMT_CarbonAlloy×40 | 4000 | 16 | MMT_AdvancedComponents（对照 Components_D2） |

前两条 workSpeedStat=SmeltingSpeed，后两条=ElectronicCraftingSpeed（照 HSK）。

### 4.3 量子加工台 2 条（挂建筑 recipes 列表，HSK 配方本身无研究前置，靠建筑解锁）

| defName | 输入 | 产出 | 工作量 | 技能 |
|---|---|---|---|---|
| MMT_ProduceAPoly | MMT_DepletedUranium×5 + MMT_CarbonAlloy×5 + MMT_Antimatter×3 | MMT_AlphaPoly×15 | 8000 | 19 |
| MMT_ProduceBPoly | MMT_AlphaPoly×3 + MMT_BiosyntheticMaterial×5 | MMT_BetaPoly×5 | 12000 | 20 |

### 4.4 对撞台 1 条（自建，无 HSK 配方参照）

| defName | 输入 | 产出 | 工作量 | 技能 |
|---|---|---|---|---|
| MMT_MakeAntimatter | MMT_Matter×50 + Uranium×20 | MMT_Antimatter×2 | 10000 | 15 |

依据：HSK 对撞台 ticksToProduce=10000 产 2 个、吃无机物（hopper）。配方化后工作量 10000 对齐；铀输入为自平衡增量（可审）。

### 4.5 生物铁合金 1 条（挂原版 BioferriteShaper，决策 1）

| defName | 输入 | 产出 | 工作量 | 技能 | 研究 |
|---|---|---|---|---|---|
| MMT_MakeBioferriteAlloy | Bioferrite×10 + MMT_IronOre×10 + [MMT_Coal]×5 | MMT_BioferriteAlloy×10 | 2100 | 9 | BioferriteShaping（原版 Anomaly） |

整条 `<RecipeDef MayRequire="Ludeon.RimWorld.Anomaly">`。HSK 输入用 Iron（其铁矿 defName），MMT 用 MMT_IronOre。

### 4.6 生物质来源（HSK 靠器官实验室 TableOrganvat 自定义处理类，不可用——**待用户决策**）

推荐方案 A（双配方，挂 MMT_ElectricChemLab，研究 MMT_MatterConversion1）：
- `MMT_ExtractBiomatterCorpse`：[Corpses]×1 → MMT_Biomatter×20，工作量 6000（贴近 HSK 尸体×1→×20）
- `MMT_ExtractBiomatterMeat`：[MeatRaw]×40 → MMT_Biomatter×20，工作量 4000（无尸体时的量产替代，HSK 无此条，自平衡）

## 5. 研究（3 个，`MMT_Research_Matter.xml`，tab=MMT_ResearchTab）

| defName | label | baseCost | techLevel | 前置 | 对照 |
|---|---|---|---|---|---|
| MMT_MatterConversion1 | 物质转换 I | 2500 | Spacer | MMT_BasicElectronics + MMT_ComponentCrafting | Super_matter_D1（Electronics_D1+Components_D1） |
| MMT_MatterConversion2 | 物质转换 II | 2400 | Ultra | MMT_UltraComponents + MMT_MatterConversion1 | Super_matter_E1（Components_D3） |
| MMT_MatterConversion3 | 物质转换 III | 2500 | Ultra | MMT_MatterConversion2 | Super_matter_E2 |

## 6. 贴图复制（HSK 已授权静态贴图）

| 源（Hardcore-SK/Mods/Core_SK/Textures/） | 目标（1.7/Textures/） |
|---|---|
| Things/Item/RMatter.png | Things/Item/MMT/MMT_Matter.png |
| Things/Item/Resource/MagneticMaterial.png | Things/Item/MMT/MMT_MagneticMaterial.png |
| Things/Item/Resource/Biomatter.png | Things/Item/MMT/MMT_Biomatter.png |
| Things/Item/Resource/BioMicrochips.png | Things/Item/MMT/MMT_BioMicrochips.png |
| Things/Item/Resource/APoly.png / BPoly.png | Things/Item/MMT/MMT_AlphaPoly.png / MMT_BetaPoly.png |
| Things/Item/Resource/BiosyntheticMaterial/（目录） | Things/Item/MMT/MMT_BiosyntheticMaterial/（重命名 MMT_BiosyntheticMaterial_a/_b/_c.png） |
| Things/Building/Production/HadronCollider/Antimatter.png | Things/Item/MMT/MMT_Antimatter.png |
| Things/Item/IngotsSpecial/Bioferrite/（_A/_B/_C） | Things/Item/MMT/MMT_BioferriteAlloy/（改小写 _a/_b/_c） |
| Things/Building/Production/MatterConverter/MC_off_{east,north,south}.png | Things/Building/Production/MMT_MatterConverter/MMT_MatterConverter_{east,north,south}.png |
| Things/Building/Production/MatterFab/MatterFab_{east,north,south}.png | Things/Building/Production/MMT_Matterfab/MMT_Matterfab_{east,north,south}.png |
| Things/Building/Production/HadronCollider/HadronCollider.png | Things/Building/Production/MMT_ParticleCollider/MMT_ParticleCollider.png |

## 7. 工具链同步

- `tools/validate_mod.py`：新增 `matter-top` 套件（9 资源字段/贴图存在性 + 21 配方输入产出/研究引用 + 3 建筑造价/尺寸/耗电 + 3 研究前置），注册 SUITES，更新顶部路径常量。
- `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS`：Matter/Antimatter/Biomatter/MagneticMaterial/BiosyntheticMaterial/BioMicrochips/AlphaPoly/BetaPoly/BioferriteAlloy/MatterConverter/Matterfab/HadronCollider → MMT_* 映射。
- 重跑 `node tools/build_mmt_chains_compare.js` 更新对比文档。
- 验收：`validate_mod.py all` 全绿 + `dotnet build` 0 错误 + 同步部署目录。

## 8. 非目标与风险

- 不做：SK 动画/IdlePower/hopper 类、器官实验室（自定义处理类）、反物质反应堆（发电侧不在本批范围）。
- 风险 1：反物质/对撞台数值无 HSK 配方参照，属自平衡（§4.4、§3 耗电可审）。
- 风险 2：生物质来源偏离 HSK（无器官实验室），§4.6 待决策。
- 风险 3：Anomaly 引用（Bioferrite/BioferriteShaper/BioferriteShaping/Bioferrite 材质类别）需全部带 MayRequire；用户 DLC 全，本地验证无碍。

## 9. 待用户确认事项

1. §4.6 生物质来源：推荐方案 A 双配方（尸体+生肉）；备选只做尸体配方 / 只做生肉配方。
2. §4.4 反物质配方投入（无机物50+铀20→2）与 §3 对撞台/量子台耗电（15000/8000）是否接受。
3. 对撞台尺寸维持 HSK 7×2，还是缩小（如 5×2）。
