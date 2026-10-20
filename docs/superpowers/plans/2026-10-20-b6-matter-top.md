# B6 无机物转化 + 顶级材料实施计划

- 日期：2026-10-20
- 依据：`docs/superpowers/specs/2026-10-20-b6-matter-top-design.md`（已审阅）；HSK 数值已实地核对（Core_SK 1.7-super-unstable + DLCModule/Anomaly）
- 前置：B1–B5 已完成；MMT_RoboticAssembler / MMT_ElectricChemLab / MMT_SuperMaterialCat / MMT_USLDBar / MMT_USLDHBar / MMT_SLDBar / MMT_ElectronicsCat / MMT_Coal 均已存在
- 架构：纯 XML Def + 贴图复制（HSK 已授权）；新建筑用原版 `Building_WorkTable`/`CompProperties_PowerTrader`；无 C# 改动

## 目标

新增 9 资源 + 3 建筑（无机物转化器/量子加工台/粒子对撞台）+ 21 配方 + 3 研究；新增 `matter-top` 验证套件；MMT_ALIAS 更新 + 重跑对比文档；全量验证 + dotnet build + 同步部署 + 分步提交。

## 关键决策（spec 已确认）

1. **生物质来源**：双配方挂 MMT_ElectricChemLab——尸体×1→×20（6000 工作量，贴近 HSK）+ [MeatRaw]×40→×20（4000，量产替代），研究 MMT_MatterConversion1
2. **反物质**：MMT_Matter×50 + Uranium×20 → MMT_Antimatter×2 @ MMT_ParticleCollider，工作量 10000，技能 15（自平衡，已确认）
3. **建筑耗电**：转化器 2500W（=HSK 工作功耗）、量子台 8000W、对撞台 15000W（恒定 PowerTrader，替代 SK.IdlePower）
4. **对撞台尺寸**：7×2 维持 HSK；不做 hopper 进料器（SK 自定义类）
5. **字段剔除**：Bulk/MaxBurningTempCelsius/BurnDurationHours/MeleePenetrationFactor 及 HSK stuff 类别（SolidMetallic 等）一律剔除；`ElectronicCraftingSpeed`（1.7 原版有）与活铁灵能品质因子（Anomaly 原版同款）保留
6. **铀/AI 核心/超织物/活铁**：直接引用原版 def（Uranium/AIPersonaCore/Hyperweave/Bioferrite/BioferriteShaper/BioferriteShaping），Anomaly 相关条目带 `MayRequire="Ludeon.RimWorld.Anomaly"`
7. **金银转化**：映射 MMT 自有矿 MMT_GoldOre/MMT_SilverOre（不引用原版 Gold/Silver）

## Task 1：贴图复制（11 项）

| 源（Hardcore-SK/Mods/Core_SK/Textures/） | 目标（1.7/Textures/） |
|---|---|
| Things/Item/RMatter.png | Things/Item/MMT/MMT_Matter.png |
| Things/Item/Resource/MagneticMaterial.png | Things/Item/MMT/MMT_MagneticMaterial.png |
| Things/Item/Resource/Biomatter.png | Things/Item/MMT/MMT_Biomatter.png |
| Things/Item/Resource/BioMicrochips.png | Things/Item/MMT/MMT_BioMicrochips.png |
| Things/Item/Resource/APoly.png | Things/Item/MMT/MMT_AlphaPoly.png |
| Things/Item/Resource/BPoly.png | Things/Item/MMT/MMT_BetaPoly.png |
| Things/Item/Resource/BiosyntheticMaterial/BiosyntheticMaterial_{a,b,c}.png | Things/Item/MMT/MMT_BiosyntheticMaterial/MMT_BiosyntheticMaterial_{a,b,c}.png |
| Things/Building/Production/HadronCollider/Antimatter.png | Things/Item/MMT/MMT_Antimatter.png |
| Things/Item/IngotsSpecial/Bioferrite/Bioferrite_{A,B,C}.png | Things/Item/MMT/MMT_BioferriteAlloy/MMT_BioferriteAlloy_{a,b,c}.png（改小写） |
| Things/Building/Production/MatterConverter/MC_off_{east,north,south}.png | Things/Building/Production/MMT_MatterConverter/MMT_MatterConverter_{east,north,south}.png |
| Things/Building/Production/MatterFab/MatterFab_{east,north,south}.png | Things/Building/Production/MMT_Matterfab/MMT_Matterfab_{east,north,south}.png |
| Things/Building/Production/HadronCollider/HadronCollider.png | Things/Building/Production/MMT_ParticleCollider/MMT_ParticleCollider.png |

提交：`feat: 添加 B6 无机物转化与顶级材料贴图`

## Task 2：资源 ThingDef（2 个新文件，9 个）

`1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Matter.xml`（6 个）：MMT_Matter（MV10/stack150/发光蓝）、MMT_Antimatter（MV120/stack75/爆炸 comp Bomb r1.5/tradeability None/DeteriorationRate 0.01）、MMT_Biomatter（MV10/stack150/SellPriceFactor0.2/Exotic）、MMT_MagneticMaterial（MV110/stack75）、MMT_BiosyntheticMaterial（MV90/stack75/StackCount）、MMT_BioMicrochips（MV2100/stack25/MMT_ElectronicsCat/Exotic）。

`MMT_Items_Resource_SuperAlloys.xml`（3 个）：MMT_AlphaPoly（MV45/USLDBar）、MMT_BetaPoly（MV65/USLDBar+USLDHBar）、MMT_BioferriteAlloy（MV9/stack600/useHitPoints/smeltable/SLDBar，灵能品质因子保留，stuff categories 仅 Metallic + Bioferrite(MayRequire Anomaly)）。α/β stuffProps：categories 仅 Metallic，statFactors 照 HSK 删 MeleePenetrationFactor；StuffPower_* 照搬。

提交：`feat: 添加无机物、反物质与顶级材料资源定义`

## Task 3：建筑（`MMT_Buildings_Matter.xml`，3 台）

BuildingBase + Building_WorkTable + designationCategory=MMT_Production + PowerTrader/Flickable/Breakdownable + Glower + HeatPusher（按 spec §3 表）；recipes 列表：转化器挂 14 条 MMT_Convert*、量子台挂 MMT_ProduceAPoly/BPoly、对撞台挂 MMT_MakeAntimatter；研究前置与建造技能按 spec。

提交：`feat: 添加无机物转化器、量子加工台与粒子对撞台建筑`

## Task 4：配方（2 个新文件 + 1 处既有建筑扩展）

- `MMT_Recipes_Matter.xml`：14 条转化配方（spec §4.1 表，全部技能 15；ConvertResourcesRaw 研究 I，其余研究 II；workSpeedStat=ElectronicCraftingSpeed）
- `MMT_Recipes_TopMaterials.xml`：装配器 4 条（§4.2）+ 量子台 2 条（§4.3，配方无 researchPrerequisite）+ 对撞台 1 条（§4.4）+ 生物质 2 条（§4.6，研究 I）+ 生物铁合金 1 条（§4.5，整条 MayRequire Anomaly，recipeUsers=BioferriteShaper，研究 BioferriteShaping）
- 既有扩展：MMT_ElectricChemLab（在 MMT_Buildings_ElectricPetrochem.xml）recipes += MMT_ExtractBiomatterCorpse / MMT_ExtractBiomatterMeat

提交：`feat: 添加物质转换与顶级材料配方`

## Task 5：研究（`MMT_Research_Matter.xml`，3 个）

MMT_MatterConversion1（2500/Spacer/前置 MMT_BasicElectronics+MMT_ComponentCrafting，坐标 (19,1)）、MMT_MatterConversion2（2400/Ultra/前置 MMT_UltraComponents+MMT_MatterConversion1，坐标 (20,2)）、MMT_MatterConversion3（2500/Ultra/前置 MMT_MatterConversion2，坐标 (21,2)）。坐标已 grep 避开既有占用（现有最大 (18,2)）。

提交：`feat: 添加物质转换研究树`

## Task 6：验证套件（`tools/validate_mod.py`）

新增 `validate_matter_top` + 注册 `matter-top` 套件进 SUITES 与 all：9 资源（字段+贴图存在性）、3 建筑（造价/尺寸/耗电/recipes 列表）、21 配方（输入产出/技能/研究引用）、3 研究（前置引用存在）；resource_names 并入新文件；VANILLA 白名单 += Uranium/AIPersonaCore/Hyperweave/Bioferrite/BioferriteShaper/BioferriteShaping。

提交：`test: 添加 matter-top 验证套件`

## Task 7：文档、部署与收尾

- `tools/build_mmt_chains_compare.js` MMT_ALIAS += Matter/Antimatter/Biomatter/MagneticMaterial/BiosyntheticMaterial/BioMicrochips/AlphaPoly/BetaPoly/BioferriteAlloy/MatterConverter/Matterfab/HadronCollider 映射
- `node tools/build_mmt_chains_compare.js` 重跑对比文档
- `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all` 全绿 + `dotnet build` 0 错误
- `cp -r` 同步 `D:\RimWorld v1.7.4979\Mods\rw`（1.7/Defs、1.7/Textures 增量），diff 校验
- 提交：`docs: 重跑 MMT 与 HSK 资源链对比文档`（含 ALIAS 改动）

## Review Focus

- 剔除字段是否漏网（Bulk/MaxBurningTempCelsius/HSK stuff 类别）——validate all-xml 会兜底红字
- Anomaly MayRequire 覆盖：生物铁合金配方整条 + MMT_BioferriteAlloy 的 Bioferrite stuff 类别 li
- 研究坐标不与既有冲突
- texPath 指向目录语义（StackCount：MMT_BiosyntheticMaterial、MMT_BioferriteAlloy 指文件夹）

## 不做（范围外）

SK 动画/IdlePower/hopper、器官实验室、反物质反应堆、B7 电子零部件（Hexcell 等）、矿物声纳模块。
