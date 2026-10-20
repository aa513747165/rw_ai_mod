# B8 织物 + 作物 + 武器部件 设计

- 日期：2026-10-20
- 状态：已审阅（2026-10-20 用户确认：甲壳=patch 原版虫类屠宰掉落；武器部件研究=引用原版为主补 1-2 个；染料=自建染料链；椰子表面活性剂=本批补上）
- 引用：`2026-10-20-b2-b8-batch-roadmap.md` §8；依赖 B5（纺织机/复合粘剂/表面活性剂已建成）、B6（磁性/生物合成材料）、B7（零部件类别）
- HSK 数值已实地核对（Core_SK 1.7-super-unstable，b99b17d2）
- 批内三小组，按 8.1 → 8.2 → 8.3 顺序执行、分步提交

## 1. 已确认决策（无需再问）

1. **纺织机已建成**：`MMT_Loom`（2×2/500W）已在 B5 落地并挂复合粘剂，8.2 只做配方挂载，不重建。
2. **甲壳来源**：patch 原版虫类 `butcherProducts` 掉落甲壳（原版虫类 Megascarab/Spelopede/Megaspider 的 race 无 butcherProducts 节点，需 `PatchOperationConditional` 双分支）；掉落量对照 HSK 剪毛量：巨甲虫 30、穴居阔步虫 45、巨蜘蛛 60。
3. **武器部件研究引用原版为主**：Machining/Gunsmithing/BlowbackOperation/GasOperation/PrecisionRifling/HeavyTurrets/Mortars/Fabrication/ChargedShot 全部直接引用；仅发射器部件无对应，自建 `MMT_Rocketry1`。
4. **染料自建链**：新增 `MMT_Dye` 资源 + 花卉→染料配方（自平衡，HSK 无参照）+ 染料→绘画用品配方（对照 HSK CookPaintDye）。
5. **椰子表面活性剂替代配方本批补**（B5 遗留）。
6. **重型武器机床（HeavyArmsBench）不建**：HSK 该台的非炮塔箱配方（重型/火炮/发射器部件）均共挂先进武器加工台，MMT 挂先进台即可；炮塔箱链决策 2 已砍。
7. **HSK 作物播种研究**（Fabric_Plants_A1/A2、Food_Plants_A2 等）不映射：MMT 8 种新作物**不带 sowResearchPrerequisites**（保留 HSK 的 sowMinSkill 与 sowTags）；恶魔菇是原版植物，其原版 Devilstrand 播种研究天然保留。
8. **HSK 建筑 SK 类/设施联动剔除**：武器加工台的 AffectedByFacilities（ToolCabinet/PodChair 均为 SK 建筑）剔除，沿用 MMT 既有建筑模板（Building_WorkTable + 恒定耗电 + Flickable + Breakdownable）。

## 2. 8.1 作物（新建 `1.7/Defs/ThingDefs_Plants/MMT_Plants_Cultivated.xml` + patch）

### 2.1 新收获资源（10 个，新建 `MMT_Items_Resource_Crops.xml`）

| defName | label | HSK 对照 | 类别 | 贴图（HSK texPath） |
|---|---|---|---|---|
| MMT_RawCotton | 原棉 | RawCotton | ResourcesRaw（执行时按 HSK 核对） | Things/Item/Resource/RawCotton |
| MMT_RawDevilstrand | 生恶魔菇 | RawDevilstrand | 同上 | 同上目录 |
| MMT_RawFlax | 生亚麻 | RawFlax | 同上 | 同上 |
| MMT_Bamboo | 竹节 | Bamboo | ResourcesRaw（HSK 归 Woody 系，执行核对） | Things/Item/Resource/Bamboo |
| MMT_RedWoodLog | 红木 | RedWoodLog | Woody 系（对照 HSK，可做燃料/建材 stuffProps 照抄） | 同上 |
| MMT_RawCoconut | 椰子 | RawCoconut | PlantFoodRaw（食物原料，营养值照 HSK） | 同上 |
| MMT_RawCaoutchouc | 天然橡胶 | RawCaoutchouc | ResourcesRaw | 同上 |
| MMT_WildRose | 野玫瑰 | WildRose | PlantFoodRaw/ResourcesRaw 按 HSK | 同上 |
| MMT_Hypericum | 金丝桃 | Hypericum | 同上 | 同上 |
| MMT_MintLeaves | 薄荷叶 | MintLeaves | 同上 | 同上 |

- 数值（MV/stack/营养/易燃）逐项照抄 HSK 对应 def，剔除 CE/SK 专属字段。
- 椰子/野玫瑰等若 HSK 带 ingestible（直接食用），照抄；不新增食物链配方（全局非目标 3.3）。

### 2.2 新作物品（8 个）

| defName | label | 产物 | 生长天数 | 产量 | 播种标签 | sowMinSkill | 贴图 |
|---|---|---|---|---|---|---|---|
| MMT_PlantFlax | 亚麻花 | MMT_RawFlax×9 | 8.1 | 9 | Ground, Hydroponic | 4 | Things/Plant/PlantLinen_Harvest |
| MMT_PlantBamboo | 竹 | MMT_Bamboo×10 | 4 | 10 | Ground, Hydroponic | 按 HSK | Things/Plant/PlantBambooTree |
| MMT_PlantRedwood | 红木 | MMT_RedWoodLog×225 | 85 | 225 | Ground | 按 HSK | Things/Trees/TreeRedwoods |
| MMT_PlantPalm | 椰子树 | MMT_RawCoconut×22 | 12 | 22 | Ground | 按 HSK | Things/Plant/PlantCoconutPalmTree |
| MMT_PlantHevea | 橡胶树 | MMT_RawCaoutchouc×20 | 6.5 | 20 | Ground | 按 HSK | Things/Plant/PlantHeveaTree(+_Leafless) |
| MMT_PlantWildRose | 野玫瑰 | MMT_WildRose×6 | 8 | 6 | Hydroponic | 按 HSK | Things/Plant/PlantWildRose |
| MMT_PlantHypericum | 金丝桃 | MMT_Hypericum×4 | 6 | 4 | Hydroponic | 按 HSK | Things/Plant/PlantHypericum |
| MMT_PlantMint | 薄荷 | MMT_MintLeaves×5 | 6.5 | 5 | Hydroponic | 按 HSK | Things/Plant/PlantMint |

- plant 节点其余字段（fertilityMin/fertilitySensitivity/growMinGlow/harvestWork/lifespanDays/wildOrder 等）照抄 HSK，剔除 SK 专属（如 neverBlightable 保留——原版字段）。
- 贴图整目录复制（植物多阶段 png），texPath 指目录。

### 2.3 原版 patch（新建 `1.7/Patches/MMT_Patch_CropHarvest.xml`）

- `Plant_Cotton`/plant/harvestedThingDef → `MMT_RawCotton`（PatchOperationReplace）。
- `Plant_Devilstrand`/plant/harvestedThingDef → `MMT_RawDevilstrand`。
- **平衡影响（决策 4 已确认）**：布/魔菇布不再直接收获，需经纺织机工序；原版衣物链不断裂（配方按 stuff 类别选料）。已有存档中的棉花/恶魔菇植株收割产物同步变更。

## 3. 8.2 织物与杂项

### 3.1 新资源（9 个，新建 `MMT_Items_Resource_Textiles.xml`）

| defName | label | HSK 对照 | 类别 |
|---|---|---|---|
| MMT_Flaxcloth | 亚麻布 | Flaxcloth | MMT_BTextiles |
| MMT_HempCloth | 大麻布 | HempCloth | MMT_BTextiles |
| MMT_Velour | 丝绒 | Velour | 按 HSK（CTextiles→原版 Textiles） |
| MMT_Kevlar | 凯夫拉（芳纶） | Kevlar | 按 HSK |
| MMT_ChitinPlating | 甲壳板 | ChitinPlating | 按 HSK |
| MMT_Chitin | 甲壳 | Chitin | 按 HSK（AnimalProduct 系核对） |
| MMT_Tallow | 兽脂 | Tallow | 按 HSK |
| MMT_PaintingSupplies | 绘画用品 | PaintingSupplies | 按 HSK |
| MMT_Dye | 染料 | Dye（HSK 有 def 无生产链） | 按 HSK |

- 织物类 stuffProps（衣物属性系数）照抄 HSK——亚麻布/大麻布/丝绒/凯夫拉均为可制衣 stuff；甲壳板按 HSK（若 HSK 非 stuff 则不加 stuffProps）。
- 原版成品沿用原版 def（只加配方不改资源）：Cloth、DevilstrandCloth、Synthread、Hyperweave。
- 原版 `Cloth` patch 加入 `MMT_BTextiles`（丝绒配方 [基础织物] 输入需要），追加到 `MMT_Patch_VanillaCategories.xml`。

### 3.2 织物配方（9 条挂 MMT_Loom + 3 条杂项，新建 `MMT_Recipes_Textiles.xml`）

| defName | 输入 | 产出 | 工作量 | 技能 | 研究映射（HSK→MMT） |
|---|---|---|---|---|---|
| MMT_MakeCloth | MMT_RawCotton×20 | Cloth×20 | 600 | 制作2 | 服装生产 II→原版 ComplexClothing |
| MMT_MakeHempCloth | SmokeleafLeaves×30（原版） | MMT_HempCloth×15 | 1200 | 制作6 | ComplexClothing |
| MMT_MakeFlaxcloth | MMT_RawFlax×20 | MMT_Flaxcloth×10 | 600 | 制作4 | 无（HSK MakeLinen 无研究） |
| MMT_MakeDevilstrand | MMT_RawDevilstrand×15+MMT_SyntheticFibers×2 | DevilstrandCloth×15 | 600 | 制作13 | 原版 AdvancedFabrication |
| MMT_MakeSynthread | MMT_RawCotton×10+MMT_SyntheticFibers×3 | Synthread×10 | 600 | 制作12 | AdvancedFabrication |
| MMT_MakeVelour | MMT_Compaste×5+[MMT_BTextiles]×40 | MMT_Velour×25 | 1800 | 制作12 | AdvancedFabrication |
| MMT_MakeKevlar | MMT_Plastic×30+MMT_SyntheticFibers×15 | MMT_Kevlar×10 | 1800 | 制作13 | AdvancedFabrication |
| MMT_MakeHyperweave | Synthread×10+DevilstrandCloth×10 | Hyperweave×10 | 600 | 制作17 | 服装生产 V→自建 MMT_SuperFabric（§5） |
| MMT_MakeChitinPlating | MMT_Chitin×50+MMT_SyntheticFibers×4+MMT_Compaste×1 | MMT_ChitinPlating×15 | 1080 | 制作12 | AdvancedFabrication |
| MMT_MakeTallow | [MeatRaw]×30 | MMT_Tallow×15 | 1200 | 烹饪3 | 食品工程原理 I→原版 Pemmican；recipeUsers=Campfire+FueledStove（原版） |
| MMT_CookPaint | (MMT_WildRose 或 MMT_Hypericum 或 MMT_MintLeaves)×10+[Woody]×2 | MMT_PaintingSupplies×1 | 550 | 制作5 | 艺术 I→自建 MMT_ArtSupplies（§5）；recipeUsers=ElectricStove+FueledStove |
| MMT_CookPaintDye | MMT_Dye×10+[Woody]×2 | MMT_PaintingSupplies×1 | 550 | 制作5 | MMT_ArtSupplies；同上挂载 |
| MMT_MakeDye（自建链） | (MMT_WildRose 或 MMT_Hypericum 或 MMT_MintLeaves)×4 | MMT_Dye×8 | 400 | 制作3 | MMT_ArtSupplies；同上挂载 |

- 染料链自平衡说明（HSK 无参照）：直用花卉 10→1 绘画用品；染料路径 4 花→8 染料，即 5 花+工序→1 绘画用品，以额外工序换花卉减半，两路线并存合理。
- HSK MakeLinen 共挂 LeatherworkerTable（SK 台），MMT 只挂 MMT_Loom。
- 织物配方 workSpeedStat：HSK 用 GeneralLaborSpeed/DrugCookingSpeed 等，执行时逐条照抄 HSK 字段（炉灶系用 CookSpeed）。
- MMT_Loom 建筑 recipes 列表 += 9 条织物配方。

### 3.3 竹板/红木板（4 条，追加 `MMT_Recipes_Wood.xml` / `MMT_Recipes_ElectricWood.xml`）

| defName | 输入→产出 | 工作量 | 技能 | 挂载 | 研究 |
|---|---|---|---|---|---|
| MMT_MakeBambooPlanks_Hand | MMT_Bamboo×10→MMT_BambooPlank×15 | 1200 | 6 | MMT_HandSawmill | MMT_Woodworking（对照 Craft_0） |
| MMT_MakeBambooPlanks_Electric | MMT_Bamboo×20→MMT_BambooPlank×30 | 1200 | 9 | MMT_ElectricSawmill | 电动锯木台既有研究（对照 Metals_C1，执行核对） |
| MMT_MakeRedWoodPlanks_Hand | MMT_RedWoodLog×10→MMT_RedWoodPlank×15 | 1200 | 3 | MMT_HandSawmill | MMT_Woodworking |
| MMT_MakeRedWoodPlanks_Electric | MMT_RedWoodLog×20→MMT_RedWoodPlank×30 | 1200 | 6 | MMT_ElectricSawmill | 同上 |

- 新资源 MMT_BambooPlank/MMT_RedWoodPlank（2 个，stuffProps 照抄 HSK BambooPlank/RedWoodPlank，归 Woody stuff 类别，追加进 `MMT_Items_Resource_Wood.xml` 或随作物资源文件，执行定）。

### 3.4 甲壳 patch（新建 `1.7/Patches/MMT_Patch_InsectChitin.xml`）

- Megascarab/Spelopede/Megaspider 各 `PatchOperationConditional`：有 butcherProducts 则 Add li，无则 Add 整节点；掉落 `MMT_Chitin`×30/45/60。
- 屠宰产量受屠宰效率系数缩放（原版机制），与 HSK 周期性剪毛不同——一次性获取、量已折半以上，视为平衡。

### 3.5 椰子表面活性剂（追加 `MMT_Recipes_PetrochemExt.xml`）

- `MMT_MakeSurfactant_CoconutSubstitute`：MMT_Salt×10+MMT_Sulfur×15+MMT_RawCoconut×25→MMT_Surfactant×50，4000，制作≥12+智识≥4，研究原版 Firefoam（HSK 建造 IV 即 vanilla Firefoam），挂载与既有 `MMT_MakeSurfactant` 相同（recipeUsers=原版酿造台+药物实验台）。

## 4. 8.3 武器部件

### 4.1 新建筑（3 台，新建 `MMT_Buildings_WeaponParts.xml`）

| defName | label | 对照 HSK | stuff/造价 | WorkToBuild | 尺寸 | 功耗 | 研究 |
|---|---|---|---|---|---|---|---|
| MMT_WeaponCraftingTable | 武器加工台 | WeaponCraftingWorkTable | Metallic+Woody stuff 140（HSK RuggedMetallic 是 SK stuff 类别，映射原版 Metallic）+costList 照抄 | 4000 | 3×1 | 500W | 原版 Machining |
| MMT_AdvWeaponCraftingTable | 先进武器加工台 | AdvWeaponCraftingWorkTable | stuff 160+costList 照抄 | 6500 | 3×1 | 500W | 原版 Fabrication（HSK Range_C2 无原版对应） |
| MMT_MechWeaponTable | 未来武器工作台 | MechWeaponCraftingWorkTable | stuff 190+costList 照抄 | 11000 | 3×1 | 1800W | 原版 ChargedShot |

- 建筑贴图复制 HSK 对应目录；剔除 AffectedByFacilities（SK 设施）。

### 4.2 新资源（14 个，新建 `MMT_Items_Resource_WeaponParts.xml`，全部归 `MMT_WeaponParts`）

武器部件 MMT_WeaponParts、手枪部件 MMT_PistolComponent、冲锋枪部件 MMT_SMGComponent、霰弹枪部件 MMT_ShotgunComponent、步枪部件 MMT_RifleComponent、狙击枪部件 MMT_SniperComponent、先进步枪部件 MMT_AdvRifleComponent、先进狙击枪部件 MMT_AdvSniperComponent、重型武器部件 MMT_HeavyComponent、火炮武器部件 MMT_CannonComponent、发射器部件 MMT_LauncherComponent、电荷武器部件 MMT_ChargedComponent、光束武器部件 MMT_LaserComponent、离子武器部件 MMT_PlasmaComponent。数值/MV/贴图照抄 HSK 对应 def。

### 4.3 配方（14 条，新建 `MMT_Recipes_WeaponParts.xml`）

| defName | 输入 | 产出 | 工作量 | 技能 | 挂载 | 研究映射 |
|---|---|---|---|---|---|---|
| MMT_MakeWeaponParts | [MMT_MetallicBar]×25+ComponentIndustrial×10+MMT_Mechanism×5 | ×5 | 1500 | 6 | 三台全挂 | 原版 Machining |
| MMT_MakePistolComponent | [MMT_SLDBar 或 MMT_USLDBar]×6+[MMT_HCMBar]×3+ComponentIndustrial×2+MMT_Wire×1+MMT_Mechanism×1 | ×1 | 1200 | 6 | 武器台+先进台 | 原版 Gunsmithing |
| MMT_MakeSMGComponent | [SLDB/USLDB]×10+[HCM]×5+ComponentIndustrial×4+MMT_Wire×6+MMT_Mechanism×2 | ×1 | 1800 | 9 | 同上 | 原版 BlowbackOperation |
| MMT_MakeShotgunComponent | [SLDB/USLDB]×15+[HCM]×8+ComponentIndustrial×4+MMT_Wire×5+MMT_Mechanism×2 | ×1 | 1800 | 7 | 同上 | 原版 GasOperation |
| MMT_MakeRifleComponent | [SLDB/USLDB]×20+[HCM]×10+ComponentIndustrial×6+MMT_Wire×8+MMT_Mechanism×3 | ×1 | 2100 | 8 | 同上 | GasOperation（HSK Rifles_C1；原版导气式即解锁突击步枪） |
| MMT_MakeSniperComponent | MMT_RifleComponent×1+ComponentIndustrial×6+MMT_Glass×12 | ×1 | 2100 | 10 | 同上 | 原版 PrecisionRifling |
| MMT_MakeAdvRifleComponent | MMT_SyntheticFibers×5+MMT_Plastic×15+MMT_Rubber×15+MMT_RifleComponent×1+MMT_AdvMechanism×2 | ×1 | 2100 | 13 | 先进台 | 原版 Fabrication（HSK Rifles_C2） |
| MMT_MakeAdvSniperComponent | MMT_SyntheticFibers×10+MMT_Plastic×20+MMT_Rubber×15+MMT_IntegratedCircuit×4+MMT_SniperComponent×2+MMT_AdvMechanism×3 | ×1 | 2100 | 15 | 先进台 | Fabrication（HSK Sniper_rifles_D1） |
| MMT_MakeHeavyComponent | [SLDB/USLDB]×25+[HCM]×15+ComponentIndustrial×12+MMT_Wire×15+MMT_Mechanism×6 | ×1 | 2800 | 12 | 先进台 | 原版 HeavyTurrets |
| MMT_MakeCannonComponent | [SLDB/USLDB]×20+MMT_HeavyComponent×1+MMT_AdvMechanism×5 | ×1 | 3100 | 14 | 先进台 | 原版 Mortars（HSK Turrets_31 炮塔 I） |
| MMT_MakeLauncherComponent | MMT_IntegratedCircuit×2+MMT_HeavyComponent×1+MMT_AdvMechanism×3+MMT_Glass×12 | ×1 | 3100 | 14 | 先进台 | 自建 MMT_Rocketry1（HSK Rockets_C1） |
| MMT_MakeChargedComponent | [MMT_USLDBar]×10+[MMT_HCMBar]×20+MMT_MagneticMaterial×2+MMT_AdvMechanism×2 | ×1 | 2100 | 14 | 未来台 | 原版 ChargedShot |
| MMT_MakeLaserComponent | [MMT_USLDBar]×20+[MMT_HCMBar]×10+MMT_MagneticMaterial×4+MMT_Microchip×1 | ×1 | 2100 | 15 | 未来台 | ChargedShot |
| MMT_MakePlasmaComponent | [MMT_USLDBar]×20+[MMT_HCMBar]×10+MMT_BiosyntheticMaterial×4+MMT_Microchip×1 | ×1 | 2100 | 15 | 未来台 | ChargedShot |

- HSK 的"[轻合金] 或 [超轻合金]"单过滤器多类别写法照搬到 MMT（categories 含 MMT_SLDBar+MMT_USLDBar）。
- HSK 的 Electronics（集成电路）→ MMT_IntegratedCircuit；Microchips → MMT_Microchip；Glass → MMT_Glass（执行核对 MMT 玻璃 defName）。
- workSpeedStat：HSK 用 Smithing/Machining 系，执行逐条照抄；effect/sound 同前批次惯例。
- 武器部件在 MMT 中定位为可交易成品/储备件（MMT 不改原版武器制造体系），与 HSK 组装武器的用途差异属预期（全局非目标）。

## 5. 新研究（3 个，新建 `MMT_Research_TextilesWeapons.xml`）

| defName | label | baseCost | techLevel | 前置 | 对照 |
|---|---|---|---|---|---|
| MMT_ArtSupplies | 艺术用品 | 400 | Industrial | 无 | HSK Art_A1 |
| MMT_SuperFabric | 超织物工艺 | 2000 | Spacer | AdvancedFabrication（原版引用作前置） | HSK Apparel_D1 |
| MMT_Rocketry1 | 火箭弹 I | 1400 | Industrial | HeavyTurrets（原版） | HSK Rockets_C1（1400/Industrial/前置 HeavyTurrets） |

- 坐标执行时分配（现有最大 (22,1)，避开冲突）。
- INFRA_RESEARCH_TOTAL 26→29。

## 6. MMT_ALIAS 更新（`tools/build_mmt_chains_compare.js`）

新增映射：RawCotton/RawDevilstrand/RawFlax/Bamboo/RedWoodLog/RawCoconut/RawCaoutchouc/WildRose/Hypericum/MintLeaves、Flaxcloth/HempCloth/Velour/Kevlar/ChitinPlating/Chitin/Tallow/PaintingSupplies/Dye、BambooPlank/RedWoodPlank、14 个武器部件（Weapon_Parts→MMT_WeaponParts 等）、3 台建筑（WeaponCraftingWorkTable→MMT_WeaponCraftingTable 等）。Synthread/Cloth/DevilstrandCloth/Hyperweave 同名走兜底。

## 7. 验证（新套件 `textiles-weapons`，路线图指定）

- 顶部常量：CROPS_*/TEXTILES_*/WEAPON_PARTS_*（XML 路径、资源/植物/建筑/配方/研究期望表、贴图清单）；PATCH 文件断言（xpath 存在性文本级校验）。
- `validate_textiles_weapons()`：资源断言（label/MV/stack/类别/texPath/graphicClass）→ 植物断言（产物/产量/天数/sowTags/贴图目录 png 存在）→ 建筑断言 → 配方断言（输入/产出/挂载/研究，原版研究 defName 列入白名单）→ patch 文件断言 → 禁 SK. 字段扫描。
- SUITES 注册 + validate_all 挂接；研究总数 26→29、工作台总数 22→25。
- VANILLA_THINGS 补：Cloth/DevilstrandCloth/Synthread/Hyperweave/SmokeleafLeaves/Campfire/FueledStove/ElectricStove/Brewery/DrugLab/Megascarab/Spelopede/Megaspider；VANILLA_RESEARCH（若无则新增常量）：Machining/Gunsmithing/BlowbackOperation/GasOperation/PrecisionRifling/HeavyTurrets/Mortars/Fabrication/ChargedShot/ComplexClothing/AdvancedFabrication/Firefoam/Pemmican。

## 8. 贴图（HSK 已授权，执行时按 HSK texPath 逐项复制）

- 作物资源 10 + 植物目录 8（多阶段）+ 织物 9 + 武器部件 14 + 建筑 3 目录；植物贴图约 40-60 个文件。

## 9. 非目标与风险

- 不做食物链（椰浆/咖喱/草药茶等 3.3 条目，全局非目标）；椰子等仅作原料落地。
- 不做修理包（B7 已定）、不做炮塔箱、不改原版武器制造。
- patch 棉花/恶魔菇改变原版布获取节奏（决策 4 已确认）；`validate all` 全绿 + 重跑对比文档 + 部署同步照旧。
- dotnet build：本批纯 XML（无 C#），本机无 SDK 沿用既有说明。
