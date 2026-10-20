# B2–B8 批次路线图：3.1 原材料 + 3.2 加工品落地规划

- 日期：2026-10-20
- 状态：总规划（每批实施前仍单独走 brainstorming → spec → 计划 → 执行）
- 前置：B1 基础设施已完成（提交 a9ccd5c..5d124e6）：研究标签页 `MMT_ResearchTab`、建筑分类 `MMT_Production`、类别树 17 项、原版类别 patch 7 条、`infrastructure` 验证套件
- 范围来源：`docs/hsk-resource-production-chains.md` 3.1（46 链）与 3.2（135 链，其中 25 条炮塔箱 `*_Crate` 已按决策 2 砍掉，实际 110 链）
- MMT 现状来源：`docs/mmt-vs-hsk-resource-chains.md`（已有 11 矿脉 + 72 配方）

## 0. 全局约定（每批都适用）

1. 新研究一律 `<tab>MMT_ResearchTab</tab>`；新建筑一律 `designationCategory=MMT_Production`。
2. 新资源按 B1 类别树归类；配方中类别输入（[煤类]/[金属锭类] 等）用 `filter.categories` 引用 MMT 类别。
3. 建筑类层级：普通台用原版 `Building_WorkTable`；动画台（转化器/装配器/电弧炉类）如需动画再评估，功能不依赖 SK 类。
4. 挂到原版建筑的配方（篝火/炉灶/屠宰台/酿造台/药物实验台/纺织机除外——纺织机 MMT 自建）直接写 `recipeUsers` 引用原版 defName，DLC 建筑直接引用（决策 1）。
5. 每批交付 = Def + 研究 + `tools/validate_mod.py` 新套件 + `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 更新 + 重跑对比文档 + `validate all` 全绿 + `dotnet build` 0 错误。
6. 中文 label 沿用 HSK 1.6 中文翻译包；defName 加 `MMT_` 前缀。
7. 提交按逻辑分步，中文 conventional commits，只提交本地。

## 1. 批次总览与依赖

| 批次 | 主题 | 覆盖 HSK 条目（3.1/3.2） | MMT 已有基础 | 依赖 |
|---|---|---|---|---|
| B2 | 基础冶金 | 木炭/铜/锡/钢/青铜/金/银/铅/铝/硅/斯特合金/金属罐/陶瓷/粘土砖/灰烬/柴火 | 熔炉×2、炭窑×2、铜锡钢青铜金银硅木炭柴火已实现 | B1 |
| B3 | 石料建材 | 碎石/沙/混凝土×2/钢筋混凝土×2/土堆/堆肥/肥料/盐/（玻璃三件套/木板已完） | 碎石、沙、手动混凝土、玻璃链已实现 | B1 |
| B4 | 高级合金（EAF） | 镍/钴/铬/钨/钛/贫铀/斯坦/梅尔基奥/亚力可/镍钛诺/钴硬质/司太立/铁钛 | 9 条合金配方 + EAF 已实现 | B2（铝合金） |
| B5 | 石化聚合物 | 聚合物/塑料/橡胶/人造纤维/碳聚物/碳纤维锭/复合粘剂/表面活性剂/石蜡/硫酸盐/化合燃料/凝固汽油/火药/硫磺/氧化化合燃料 | 油苗、聚合物、塑料、橡胶、人造纤维、碳聚物已实现 | B1 |
| B6 | 无机物转化 + 顶级材料 | 无机物×15 条转化/α·β聚合物/生物电子元件/生物合成材料/磁性材料/反物质/生物铁合金 | 自动化装配器已有 | B4、B5 |
| B7 | 电子零部件 | 零部件 5 级/机械组件×2/电线/电子元件/集成电路/微芯片/动力电池/机器人部件/矿物声纳模块 | 12 条零部件/电子配方 + 4 台已实现 | B4、B5（B6 磁性材料） |
| B8 | 织物 + 作物 + 武器部件 | 布/魔菇布/亚麻布/大麻布/合成纤维/超织物/丝绒/凯夫拉/甲壳板/兽脂/绘画用品/竹板/红木板 + 14 种武器部件 | 锯木台×2、装配台×2 | B5、B7 |

执行顺序建议：B2 → B3 → B5（三者互不依赖，可灵活）→ B4（需 B2 铝）→ B6（需 B4+B5）→ B7（需 B4+B5）→ B8（需 B5+B7）。

## 2. B2 基础冶金

**目标**：补齐电熔炼器/熔炉/窑炉体系的基础金属与烧制品，全部对齐 HSK 数值。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 木炭、铜、锡、钢、青铜、纯金、纯银、硅（电熔）、柴火 | 已实现且数值一致 | 无（仅类别补齐，见下） |
| 铅合金 LeadBar | 缺矿缺锭 | 新增铅矾矿 Anglesite 资源 + 铅矿脉 + 手/电冶炼配方；铅锭归 `MMT_HeavyBar` |
| 铝合金 AluminiumBar | 缺矿缺锭 | 新增铝土矿 Aluminium 资源 + 铝土矿脉 + 电冶炼配方；铝锭归 `MMT_SLDBar`（B4 亚力可/斯特的前置） |
| 斯特合金 FerrosiliconAlloy | 无 | 钢+硅+铝合金 @ 电熔炼器（依赖本批铝锭） |
| 金属罐 MetalCan | 无 | [金属锭类]×8 @ 电熔炼器 |
| 陶瓷 Ceramics、粘土砖 ClayBrick | 无 | 软粘土 SoftClay 新资源 + 两条窑炉配方；**SoftClay 来源 spec 阶段查 HSK**（挖掘点/矿脉） |
| 灰烬 Ash | 无 | 燃烧系 4 配方，挂原版篝火/电力火化炉 + MMT 窑炉 |

- 类别补齐：既有锭类（铜/锡/青铜→`MMT_SLDBar`；铅→`MMT_HeavyBar`；金/银沿用原版 def 已被 B1 patch）挂入 B1 类别树。
- 研究：铅/铝冶炼并入 `MMT_ElectricSmelting` 或新增一个研究，spec 定。
- 验证套件：`basic-metallurgy`。

## 3. B3 石料建材

**目标**：补齐混凝土电动线与农业辅料（土堆/堆肥/肥料/盐）。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 碎石、沙（4+4 配方） | 已实现 | 无 |
| 混凝土、钢筋混凝土（手动） | 已实现 | 无 |
| 混凝土（电动）、土堆（电动） | 缺建筑 | 新增电动混凝土搅拌机 `MMT_EConcreteMixer` + `MixConcreteElectric` + 研究混凝土 II |
| 钢筋混凝土（电动） | 缺配方 | 挂已有电动装配台 |
| 土堆 Dirt（手动×2） | 无 | 沙+肥料 @ 屠宰点/手动搅拌器；土堆新资源 |
| 堆肥 Compost、肥料 Fertilizer | 无 | 堆肥挂原版屠宰点/屠宰台；肥料配方建筑 spec 阶段查 HSK（原文未标注建筑） |
| 盐 Salt | 无 | 盐资源 + 盐矿脉（对照 MineableSalt）+ 挖盐点建筑（对照 VG_SaltMine，原版 `Building_WorkTable` 即可） |

- 玻璃三件套、木板已在前期完成，不在本批。
- 验证套件：`stone-construction`。

## 4. B4 高级合金（EAF）

**目标**：补齐 EAF 剩余合金，既有 9 条不变。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 镍/钴/铬/钨/钛/司太立/镍钛诺/钴硬质/铁钛 | 已实现且数值一致 | 仅类别补齐（见下） |
| 贫铀合金 DepletedUranium | 无 | 原版铀矿 Uranium×10 @ EAF；归 `MMT_RADBar`（原版铀矿脉直接可用，无需新建） |
| 斯坦合金 SteelBar | 无 | 钢+镍合金 @ EAF |
| 梅尔基奥 CupronickelAlloy | 无 | 铜+镍合金 @ EAF |
| 亚力可 AlnicoAlloy | 无 | 钢+铝+钴+镍 @ EAF（**依赖 B2 铝合金**） |
| 硅 EAF 批量配方 MakeSilicon_Set | MMT 已有电熔版 | 可选补充，spec 定 |

- 类别补齐（spec 时逐一对照 HSK ThingDef 归属）：镍/钴/铬等→`MMT_SLDBar`；钨→`MMT_HeavyBar`；钛/斯坦→`MMT_USLDBar`；钴硬质/司太立→`MMT_USLDHBar`；贫铀→`MMT_RADBar`。
- 验证套件：`eaf-alloys`（可并入现有合金套件扩展）。

## 5. B5 石化聚合物

**目标**：补齐石化下游与化学/弹药小组（决策 2 落点：MMT 自建弹药台）。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 聚合物、塑料、橡胶、人造纤维、碳聚物 | 已实现 | 无 |
| 碳纤维锭 CarbonAlloy | 无 | 人造纤维+碳聚物 @ 石化台；归 `MMT_USLDHBar`；**同时把 `MMT_MakePobedite`/`MMT_MakeFerrotitanium` 的碳材料输入对齐为碳纤维锭**（对比文档差异项，实施前先核对用户 WIP 是否已改） |
| 复合粘剂 Compaste | 无 | 化合燃料+橡胶+石蜡 @ 纺织机（纺织机 B8 才建——本批先建配方挂 B8 建筑？**否：纺织机提前到本批建，B8 直接使用**；spec 定归属） |
| 表面活性剂 Surfactant | 无 | 酒精/复合粘剂两条配方挂原版酿造台/药物实验台；椰子替代配方等 B8 椰子作物后补 |
| 石蜡 Paraffins | 无 | 来源 spec 阶段查 HSK（3.1 仅转化器配方；若 HSK 无生产链，MMT 自建石化配方或依赖 B6 转化器） |
| 硫酸盐 Sulphates | 无 | 同上；MMT 现有塑料/橡胶配方是否已用硫酸盐需核对 WIP |
| 化合燃料 Chemfuel | 无 | **推荐直接用原版生物燃料精炼厂 + 原版 Chemfuel**（HSK 食物直炼配方是否复制，spec 定） |
| 火药 Powder、凝固汽油 Napalm、硫磺 MakeSulfur | 无 | 新增 `MMT_AmmoBench` 弹药工作台（决策 2：替代 AmmoBench/TableHeavyAmmunition 挂载点）；硝石 Nitre 新资源 + 硝石矿脉 |
| 氧化化合燃料 OxidizedChemfuel | 无 | 决策 2：去掉 Prometheum/FSX 输入，替代催化剂方案 spec 定（候选：增量硝石+硫酸盐，或聚合物） |

- 研究：对照石油化学 IV/V/VI 新增 MMT 研究 1–2 个。
- 验证套件：`petrochem-ext`。

## 6. B6 无机物转化 + 顶级材料

**目标**：物质转换体系 + 量子/装配顶级材料 + 反物质（决策 5 落点）+ 生物铁合金（决策 1 落点）。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 无机物 Matter + ConvertResourcesRaw | 无 | 新增无机物转化器 `MMT_MatterConverter`（对照 SK.Building_AnimatedWorktable，可用原版类）+ Matter 资源 + [原材料]×300→Matter×150 |
| 14 条 ConvertTo*（铅矾/铝土/铜/金/钛铁/铁/镍/石蜡/聚合物/银/硫酸盐/锡/铀） | 无 | 全部照搬，挂转化器；研究：物质转换 I/II 两个 |
| 反物质 Antimatter | HSK 也无生产配方 | **自建粒子对撞台 `MMT_ParticleCollider` + 反物质配方**（输入/耗电/工作量无 HSK 参照，spec 自平衡：建议铀+巨量电力+长工作量） |
| α聚合物 AlphaPoly | 无 | 量子加工台 `MMT_Matterfab` + 贫铀+碳纤维锭+反物质配方；归 `MMT_SuperMaterialCat` |
| β聚合物 BetaPoly | 无 | α+生物合成材料 @ Matterfab；归 `MMT_SuperMaterialCat` |
| 生物电子元件 BioMicrochips | 无 | 拆解伪人格核心 + 合成配方 @ 已有装配器；生物质 Biomatter 来源 spec 阶段查 HSK |
| 生物合成材料 BiosyntheticMaterial | 无 | 生物质+人造纤维+塑料 @ 装配器（依赖 B5） |
| 磁性材料 MagneticMaterial | 无 | 无机物+硫酸盐+[超轻合金]+[轻合金] @ 装配器（依赖 B4/B5） |
| 生物铁合金 BioferriteAlloy | 无 | 活铁+铁+[煤类] @ **原版活铁塑造台**（Anomaly，直接引用，前置研究引用原版 BioferriteShaping） |

- 验证套件：`matter-top`。

## 7. B7 电子零部件

**目标**：补齐零部件体系缺口；既有 12 条配方不变。

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 原始/普通/高级/超级零部件、机械组件、先进机械组件、电线、电子元件、集成电路、微芯片 | 已实现 | 仅类别补齐（零部件→`MMT_Parts`，电子件→`MMT_ElectronicsCat`） |
| 先进零部件 ComponentAdvanced | **路线待定** | 原版无此 def（B1 已踩坑）。选项：①MMT 自建 `MMT_ComponentAdvanced`（归 `MMT_Parts`）并恢复 `[超轻合金]×30→×10` 配方；②统一改用 ComponentSpacer。**与用户 WIP 协调**（其正在移除 `MMT_Recipes_Parts.xml:326,361` 的引用）——批次启动时第一个澄清问题 |
| 拆解护盾 DeconstructPersonalShield | 无 | HSK 挂回收站（SK 建筑）；MMT 无回收站，可挂装配器或跳过，spec 定 |
| 动力电池 Hexcell | 无 | 磁性材料+[高碳金属]+微芯片 @ 电子台（**依赖 B6 磁性材料**） |
| 机器人部件 RobotParts | 无 | [超轻合金]+先进机械组件 @ 电子台（依赖 B4） |
| 矿物声纳模块 MineralSonarModule | 无 | [金属锭]+先进零部件+电子元件+机械组件 @ 装配台/电子台 |
| 修理包 MendingKit | 无 | 依赖 HSK 修理台体系（Repair_table_B1），MMT 无修理链；**建议跳过或单独决策**，spec 定 |

- 验证套件：`electronics-parts`（扩展现有）。

## 8. B8 织物 + 作物 + 武器部件

**目标**：织物链 + 植物决策（决策 4）落地 + 武器部件体系。三小组，批内再分 Task。

### 8.1 作物（决策 4 落点）

- **Patch 原版**：`Plant_Cotton` 收获物改原棉 RawCotton；`Plant_Devilstrand` 改生恶魔菇 RawDevilstrand。⚠️ 风险：原版棉花/恶魔菇直接产布/魔菇布，patch 后布来源变为纺织工序——决策已确认，spec 需评估对原版衣物链的平衡影响。
- **MMT 自建作物×8**：亚麻（生亚麻）、竹（竹节）、红木（红木）、椰子树（椰子）、橡胶树（天然橡胶）、野玫瑰、金丝桃、薄荷。新建 `ThingDefs_Plants/` 目录。
- 两个新资源目录条目均挂原版合适类别（PlantFoodRaw/ResourcesRaw 等，spec 逐一定）。

### 8.2 织物与杂项

| HSK 条目 | 动作 |
|---|---|
| 纺织机 TableLoom | MMT 自建 `MMT_Loom`（B5 复合粘剂也可能提前需要，归属 spec 定） |
| 布 Cloth、魔菇布 DevilstrandCloth | 原棉→布；生恶魔菇+人造纤维→魔菇布（沿用原版成品 def，只加配方） |
| 亚麻布 Flaxcloth、大麻布 HempCloth、合成纤维 Synthread、丝绒 Velour、凯夫拉 Kevlar | MMT 新资源 + 纺织配方；布/亚麻/大麻归 `MMT_BTextiles`（丝绒的 [基础织物] 输入来源） |
| 超织物 Hyperweave | 原版 def 加配方（合成纤维+魔菇布） |
| 甲壳板 ChitinPlating | 甲壳 Chitin 来源 spec 定（HSK 靠虫类剪毛组件 `SK.CompProperties_ShearableRenameable`，MMT 不可用；候选：patch 原版虫类 butcherProducts 掉甲壳） |
| 兽脂 Tallow | [生肉] @ 原版篝火/燃料炉灶 |
| 绘画用品 PaintingSupplies | 野玫瑰/金丝桃/薄荷+[木本植物] @ 原版炉灶；染料 Dye 来源 spec 阶段查 HSK |
| 竹板、红木木板 | 挂已有手工/电动锯木台 |

### 8.3 武器部件（14 种，炮塔箱已砍）

- 自建三台：武器加工台、先进武器加工台、未来武器工作台（对照 WeaponCraftingWorkTable/AdvWeaponCraftingWorkTable/MechWeaponCraftingWorkTable）。
- 基础件：武器部件 Weapon_Parts、手枪/步枪/霰弹枪/冲锋枪/狙击枪部件。
- 进件：先进步枪/先进狙击部件、重型/火炮/发射器部件。
- 能量件：电荷/光束/离子部件（磁性材料/生物合成材料——依赖 B6）。
- 全部归 `MMT_WeaponParts`；研究对照枪械/手枪 I/步枪 I-II/霰弹 I/冲锋 I/狙击 I·III/重型武器 I/炮塔 I/火箭弹 I/定向能武器建 MMT 研究树（约 8–10 个，可合并层级，spec 定）。

- 验证套件：`textiles-weapons`。

## 9. 全局非目标（B2–B8 均不做）

- 采集侧改动：矿脉散布参数对齐、深钻井产出表、野外矿点、摧毁残留 patch、动物挤奶/剪毛/产蛋（3.1 配方外的 2.x 章节）。
- 食物链（3.3，86 条）与发酵/缓处理（68 条）。
- 炮塔组件箱链（25 条，决策 2 已砍）及任何 CE 物品（Prometheum/FSX 等）。
- 修理链（除非 B7 决策做修理包）。

## 10. 每批启动固定动作

1. brainstorming：核对本文件范围表 + 解答该批"spec 阶段待查/待定"项（文中已标注：SoftClay 来源、肥料建筑、石蜡/硫酸盐来源、化合燃料路线、生物质来源、先进零部件路线、甲壳来源、修理包去留、氧化化合燃料催化剂方案）。
2. 写该批 spec（引用本文件，不重复全局约定）→ 用户审阅 → 写计划 → 执行。
3. 执行遵守 B1 教训：涉及用户 WIP 文件先备份、`checkout HEAD` 后重放、纯净提交。
