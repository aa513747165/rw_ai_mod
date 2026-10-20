# B5 石化聚合物实施计划

- 日期：2026-10-20
- 依据：`docs/superpowers/specs/2026-10-20-b2-b8-batch-roadmap.md` 第 5 节；HSK 数值已实地核对（Core_SK 1.7-super-unstable）
- 前置：B1–B4 已完成；HSK 克隆无简中翻译，中文 label 按路线图命名自撰
- 架构：纯 XML Def + 贴图复制（HSK 已授权）；新建筑用原版 `Building_WorkTable`；无 C# 改动

## 目标

新增 9 资源 + 2 建筑（纺织机/弹药工作台）+ 1 矿脉（硝石）+ 9 配方 + 2 研究；对齐 2 条既有合金配方的碳输入；新增 `petrochem-ext` 验证套件；全量验证 + 重跑对比文档。

## 关键决策（路线图"spec 定"项的落点）

1. **石蜡/硫酸盐来源**：HSK 仅转化器（B6）产出，等不及——MMT 自建石化配方：石蜡=原油×20→×10；硫酸盐=硫磺×10→×10（无 HSK 参照，自平衡，挂手/电石化台）
2. **化合燃料**：直接用原版 `Chemfuel` + 原版生物燃料精炼厂，MMT 零改动（HSK 食物直炼配方是孤儿/依赖 SK 发酵桶类，不复制）
3. **氧化化合燃料催化剂**：去 Prometheum/FSX → 增量方案 `Chemfuel×100 + 硝石×15 + 硫酸盐×15 + 聚合物×5 → ×75`
4. **纺织机**：本批提前建 `MMT_Loom`（B8 直接用），研究前置用原版 `ComplexClothing`，不占新研究名额
5. **弹药工作台**：自建 `MMT_AmmoBench`（对照 HSK TableHeavyAmmunition），研究前置用原版 `Machining`
6. **碳输入对齐**：`MMT_MakePobedite`/`MMT_MakeFerrotitanium` 的 `MMT_Carbon×3` → `MMT_CarbonAlloy×3`（对齐 HSK 的 CarbonAlloy 输入）
7. **研究新增 2 个**：`MMT_AdvancedPetrochemistry`（高级石化，1500，前置 MMT_ElectricPetrochemistry）、`MMT_OxidizedFuel`（氧化燃料，2500 Spacer，前置前者）
8. **类别**：硝石/石蜡/硫酸盐/表面活性剂/复合粘剂/火药 → `ResourcesRaw`（与既有石化资源一致）；凝固汽油/氧化化合燃料 → `MMT_FuelCat`；碳纤维锭 → `MMT_USLDHBar`（路线图指定）
9. **剔除 SK/CE 内容**：表面活性剂在 HSK 是 CE 弹药 Def（FirefoamFuelBase）——MMT 简化为普通资源；爆炸 comp、SK.IdlePower、SmithingSpeed（1.7 不存在，用 SmeltingSpeed）均不引入
10. **MMT_ALIAS 不改**：新资源后缀与 HSK 同名，兜底匹配

## 新增资源（`MMT_Items_Resource_PetrochemExt.xml` 新文件，9 个）

贴图复制 HSK → `1.7/Textures/Things/Item/MMT/MMT_PetroExt/*.png`（Graphic_Single 单图，同 MMT_Petro 模式）：

| defName | label | MV | stack | 类别 | HSK 贴图源 |
|---|---|---|---|---|---|
| MMT_Paraffins | 石蜡 | 14 | 150 | ResourcesRaw | Paraffinwax.png |
| MMT_Sulphates | 硫酸盐 | 7 | 300 | ResourcesRaw | Sulphate.png |
| MMT_Surfactant | 表面活性剂 | 0.4 | 150 | ResourcesRaw | SurfactantFuel.png |
| MMT_Compaste | 复合粘剂 | 5 | 300 | ResourcesRaw | CompoundPaste.png |
| MMT_Powder | 火药 | 25 | 150 | ResourcesRaw | Powder.png |
| MMT_Napalm | 凝固汽油 | 5 | 300 | MMT_FuelCat | NapalmCannister.png |
| MMT_Nitre | 硝石 | 3 | 300 | ResourcesRaw | Nitre.png |
| MMT_OxidizedChemfuel | 氧化化合燃料 | 9 | 375 | MMT_FuelCat | OxFuel.png（DLCModule/Odyssey） |
| MMT_CarbonAlloy | 碳纤维锭 | 40 | 75 | MMT_USLDHBar | 复用 MMT_Ingot + color (130,130,130)，带 stuffProps（仅 Metallic；statFactors 七键参照 HSK：MaxHP 3.6 等，实施时核 HSK 全文） |

统一：ResourceBase、Metal_Drop 音效（化合物类可用 Generic_Drop 若有先例，实施时仿石化文件）、tickerType Rare、DeteriorationRate 0（MMT 惯例，HSK 硫酸盐 0.05 不抄）、Flammability 按 HSK（石蜡3/硫酸盐2/活性剂3/粘剂1.5/火药14/凝固汽油8/硝石5/氧化燃料12/碳纤维0）、Mass 按 HSK。

## 矿脉与建筑（`MMT_Buildings_PetrochemExt.xml` 新文件）

- `MMT_MineableNitre` 硝石矿脉：RockBase 父类（仿既有矿脉），yield 75，HP 1800，commonality 1.05，lump 8~16，color (124,124,110)/(175,175,165)，veinMineable
- `MMT_Loom` 纺织机：Building_WorkTable，size (2,2)，300W（Power+Flickable+Breakdownable），造价 stuff 125（Metallic+Woody）+ MMT_ComponentMedieval×10 + MMT_ElectronicParts×3 + MMT_Mechanism×6，WorkToBuild 3850，HP 170，研究 `ComplexClothing`（原版），建造技能 7，recipes=[MMT_MakeCompaste]，贴图复制 HSK Loom 6 张（含 m 掩码）
- `MMT_AmmoBench` 弹药工作台：size (3,1)，500W，造价 stuff 210（Metallic）+ ComponentIndustrial×10 + MMT_ElectronicParts×5 + MMT_Mechanism×10，WorkToBuild 6500，HP 300，研究 `Machining`（原版），建造技能 10，recipes=[MMT_MakeSulfur, MMT_MakePowder, MMT_MakeNapalm]，贴图复制 HSK TableHeavyAmmunition 6 张
- 均 designationCategory=MMT_Production

## 配方（`MMT_Recipes_PetrochemExt.xml` 新文件，9 条）

| defName | 输入 | 输出 | work | 技能 | 挂载 | 研究 |
|---|---|---|---|---|---|---|
| MMT_MakeParaffins | MMT_CrudeOil×20 | 石蜡×10 | 1000 | Crafting 6 | 手+电石化台 recipes 列表 | MMT_AdvancedPetrochemistry |
| MMT_MakeSulphates | MMT_Sulfur×10 | 硫酸盐×10 | 800 | Crafting 6 | 手+电石化台 | MMT_AdvancedPetrochemistry |
| MMT_MakeCarbonAlloy | MMT_SyntheticFibers×10 + MMT_Carbon×10 | 碳纤维锭×10 | 2800 | Crafting 15 | 电石化台 | MMT_AdvancedPetrochemistry |
| MMT_MakeCompaste | Chemfuel×10 + MMT_Rubber×5 + MMT_Paraffins×2 | 复合粘剂×20 | 3200 | Crafting 5 | MMT_Loom | MMT_AdvancedPetrochemistry |
| MMT_MakeSurfactant | MMT_Salt×10 + MMT_Sulfur×15 + [Alcohol 类别]×15 | 表面活性剂×50 | 4000 | Crafting 8 | recipeUsers: DrugLab, Brewery（原版） | MMT_AdvancedPetrochemistry |
| MMT_MakeSurfactant_Compaste | MMT_Compaste×15 + MMT_Polymers×25 | 表面活性剂×50 | 4000 | Crafting 12 + Intellectual 4 | recipeUsers: DrugLab | MMT_AdvancedPetrochemistry |
| MMT_MakeSulfur | MMT_IronOre×35 | MMT_Sulfur×10 | 1200 | 无 | MMT_AmmoBench | 无（建筑门槛） |
| MMT_MakePowder | MMT_Nitre×75 + [MMT_Coal 类别]×20 + MMT_Sulfur×20 | 火药×20 | 3000 | Crafting 3 | MMT_AmmoBench | 无 |
| MMT_MakeNapalm | Chemfuel×30 + MMT_Polymers×15 | 凝固汽油×10 | 1200 | Crafting 5 | MMT_AmmoBench | 无 |
| MMT_MakeOxidizedChemfuel | Chemfuel×100 + MMT_Nitre×15 + MMT_Sulphates×15 + MMT_Polymers×5 | 氧化化合燃料×75 | 10000 | Crafting 12 | 电石化台 | MMT_OxidizedFuel |

（表为 10 条：含 MakeSulfur）workSpeedStat：合金用 SmeltingSpeed、Compaste 用 DrugCookingSpeed、氧化燃料用 DrugSynthesisSpeed、其余默认 SmeltingSpeed；椰子版活性剂配方等 B8。

**既有配方改动**：`MMT_Recipes_AdvancedAlloys.xml` 的 Pobedite/Ferrotitanium 碳输入 `MMT_Carbon×3` → `MMT_CarbonAlloy×3`（filter+fixedIngredientFilter 同步）。

**既有建筑 recipes 列表扩展**：`MMT_HandChemTable` += MakeParaffins/MakeSulphates；`MMT_ElectricChemLab` += MakeParaffins/MakeSulphates/MakeCarbonAlloy/MakeOxidizedChemfuel。

## 研究（`MMT_Research_PetrochemExt.xml` 新文件）

- `MMT_AdvancedPetrochemistry` 高级石化工艺：1500，Industrial，前置 MMT_ElectricPetrochemistry，tab MMT_ResearchTab
- `MMT_OxidizedFuel` 氧化燃料：2500，Spacer，前置 MMT_AdvancedPetrochemistry
- 坐标实施时避开既有占用（已有 (13,0)(14,1)(12,2)(13,3)(15,0)(15,1)(15,2) 等），建议 (16.00, 1.00) / (17.00, 2.00)，先 grep 确认

## 验证（`tools/validate_mod.py`）

- 新增 `validate_petrochem_ext` + 注册 `petrochem-ext` 套件并进 `validate_all`：资源 9（含贴图断言）、矿脉、建筑 2（recipes 列表相等断言）、配方 10（含研究前置键、未知资源校验——resource_names 需并入 PetrochemExt/Petrochem/Sulfur/Agriculture(Salt)/AdvancedAlloys(CarbonAlloy 产出)/Metal(IronOre)/Glass? 不含；VANILLA_THINGS 已含 Chemfuel? —— 不含，需加 `"Chemfuel"`）、研究 2
- 改 `PETROCHEM_TABLE_RECIPES`（+2）与 `ELECTRIC_PETROCHEM_LAB_RECIPES`（+4）列表断言
- 改 `ADVANCED_ALLOYS_RECIPE_EXPECTATIONS` 两条配方的碳输入
- 禁用断言沿用：SK. 引用、SK 专有 stat

## MMT_ALIAS 与文档

不改别名表；收尾 `node tools/build_mmt_chains_compare.js` 重跑对比文档。

## Task 划分

1. **Task 0 文档**：B5 spec + 计划落盘（plans/specs），提交 `docs:`
2. **Task 1 资源+贴图**：9 资源 + 复制 8 张物品贴图 + 碳纤维锭复用锭贴图；validate 加资源断言；跑套件；提交 `feat:`
3. **Task 2 矿脉+建筑**：硝石矿脉 + 纺织机 + 弹药台 + 建筑贴图 12 张；validate 加断言；提交 `feat:`
4. **Task 3 配方+研究**：10 配方 + 2 研究 + 既有建筑 recipes 扩展 + Pobedite/Ferrotitanium 碳输入对齐 + validate 全部同步；跑 petrochem-ext/petrochem/electric-petrochem/advanced-alloys；提交 `feat:`（或拆配方/对齐两提交）
5. **Task 4 收尾**：`validate all` 全绿 + 重跑对比文档 + 提交 `docs:`

## Review Focus

1. `VANILLA_THINGS` 加 `Chemfuel`；`[Alcohol]` 类别输入需 vanilla 类别名核实（Alcohol）
2. 既有三套件的建筑 recipes 列表相等断言必须与 XML 同 commit 扩展
3. SK 专有物禁入：SmithingSpeed（用 SmeltingSpeed）、SK.IdlePower、CE AmmoDef、爆炸 comp 简化
4. 贴图复制仅限已授权 HSK 静态贴图，目标路径进 MMT_PetroExt/ 新子目录
5. WIP 纪律：CLAUDE.md 与两个 tools 脚本不纳入提交；每 commit 前 `git status --short`
6. 研究坐标不与既有研究重叠

## 不做（范围外）

- 不改既有 5+5 石化配方数值；不做 HSK 食物直炼化合燃料；不做椰子版活性剂（B8）；不做 VulcanizeRubber（HSK 可选输入版，MMT 橡胶配方已存在）；不碰深钻井产出表（全局非目标）；`dotnet build` 本机无 SDK，沿用 B4 结论（纯 XML 无 C# 改动）
