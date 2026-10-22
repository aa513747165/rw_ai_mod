# HSK 药品/医疗用品生产链参考文档

- 数据源：`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`（本地浅克隆基准 HEAD）。
- 范围：`Mods/Core_SK`、`Mods/Core_SK_CoreModule`（对原版的 Patch 层）、`Mods/Rimefeller_SK`、`Mods/CombatExtended`、`Mods/SeedsPlease`。
- 命名：`defName` 为 XML 精确值；中文名取自 HSK 1.6 中文翻译包，无翻译时保留英文 label。
- 注意：HSK 用 `SK.PatchOperationReplaceExtended` 大量改写原版 def（defName 不变但配方/前置被改），文中标注「patch」的即为改写过的原版 def。

## 1. 总览

四层结构：

1. **原料种植层**：Healroot（草药根）、Aloe（芦荟）、NeutroFlower（中性花）、Smokeleaf（烟叶）、Psychoid（精神叶）、野生草药（野玫瑰/金丝桃/薄荷）。
2. **中间产物层**：MedicineHerbal（草药原料，本身可当 0.30 效力的药）、DriedLeavesSmokeleaf（干烟叶）、Neutroamine（中性胺）、MedicalDrink（抗生素）、Bandagekit（绷带包）。
3. **成品药层**：HerbMedicine（0.60）→ Herbmedicine（0.85）→ MedicineIndustrial（1.00）→ MedicineUltratech（1.30）。
4. **毒品/兴奋剂层**：烟卷、精神茶、薄片、雅戈、清醒丸、果汁剂、青霉素、美沙酮、止痛药、狂暴草等。

核心设计：**Neutroamine 是全链条咽喉**——工业药、闪耀药、清醒丸、果汁剂、青霉素、美沙酮全部需要它，且只有两条成本都不低的获取路径（自然合成 / 石化精炼）。工业医药把石化链（石蜡/塑料/合成纤维）与种植链强行耦合。

## 2. 研究项目

### 2.1 医疗研究（tab=Medicine_SK，`Mods/Core_SK/Defs/ResearchProjectDefs/Research_Medicine.xml`）

| defName | label | baseCost | 前置 |
| --- | --- | --- | --- |
| Medicine_A1 | Medical supplies I | 150 | — |
| Medicine_A2 | Medical supplies II | 200 | Medicine_A1 |
| Medicine_B1 | Medical supplies III | 400 | Medicine_A2 |
| Medicine_B2 | Medical supplies IV | 500 | Medicine_B1 |
| MedicineProduction（原版，patch 改写） | Medical supplies V | 1000 | Medicine_B2 |
| Medicine_D1 | Medical supplies VI | 2000（Spacer） | MedicineProduction |

### 2.2 毒品研究（同文件 + CoreModule patch）

| defName | label | baseCost | 前置 |
| --- | --- | --- | --- |
| Drugs_A1 | Drugs I | 75 | — |
| PsychoidBrewing（原版，patch） | Drugs II | 100 | Drugs_A1 |
| Drugs_B1 | Drugs III | 300 | PsychoidBrewing |
| DrugProduction（原版，patch） | Drugs IV | 1300 | Oil_Industry_C5（石化研究） |
| Drugs_C2 | Penoxycyline Production | 1000 | DrugProduction + Chemistry_C2 |
| Drugs_C3 | Drugs VI | 900 | Chemistry_C2 + Drugs_B1 |
| PenoxycylineProduction（原版，patch） | Drugs V | 1000 | Drugs_C2 |
| WakeUpProduction（原版，patch） | Wake-up Production | 1000 | Drugs_C2 |
| PsychiteRefining（原版，patch） | Psychite Refining | 1200 | Drugs_C3 |
| GoJuiceProduction（原版，patch） | Go-juice Production | 1400 | WakeUpProduction + PsychiteRefining |
| SynchronisityRefining（HSK 新增） | synchronite refining | 1400 | Drugs_C3 |

### 2.3 种植研究（tab=Plants_SK，`Mods/Core_SK/Defs/ResearchProjectDefs/Research_Plants.xml`）

- Drug_plants_A1（100）→ 种 Smokeleaf；Drug_plants_A2（150，前置 A1）→ 种 Psychoid
- Medical_Plants_A1（150）→ 种 Healroot；Medical_Plants_A2（200）→ 种 Aloe
- Flowers_A2（50，前置 Flowers_A1）→ 种 NeutroFlower

## 3. 原料种植层

| 植物 defName | 产物 | 产量/生长期 | 种植研究 | 来源 |
| --- | --- | --- | --- | --- |
| Plant_Healroot（原版，patch） | MedicineHerbal ×8 | 5.8 天，sow 技能 5 | Medical_Plants_A1 | patch `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Medical.xml`；野生 Plant_HealrootWild 产量 2 |
| Plant_Aloe | aloe ×4 | 4.7 天，sow 技能 10 | Medical_Plants_A2 | `Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Medical.xml` |
| Plant_NeutroFlower | NeutroPetals ×5 | 6.2 天，**仅限水培** | Flowers_A2 | 同上 |
| Plant_Smokeleaf（原版，patch） | SmokeleafLeaves ×4 | 10.8 天，sow 技能 6 | Drug_plants_A1 | patch `.../Plants_Cultivated_Manufacture.xml` |
| Plant_Psychoid（原版，patch） | PsychoidLeaves ×10 | 6.6 天，sow 技能 7 | Drug_plants_A2 | 同上 patch |
| Plant_WildRose / Plant_Hypericum / Plant_Mint | WildRose / Hypericum / MintLeaves | 野生可采集 | — | `Core_SK/Defs/ThingDefs_Plants/Plants_Wild.xml` |

- 原料物品：NeutroPetals、aloe 在 `Core_SK/Defs/ThingDefs_Resources/Items_Resource_Crops.xml`；WildRose/Hypericum/MintLeaves 在 `Items_Resource_Crops_Luxury.xml`。
- 价格 patch：PsychoidLeaves 市价 2.1、SmokeleafLeaves 市价 4（`Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs/Items_Resouces_Plants.xml`）。
- SeedsPlease 种子：Seed_Smokeleaf、Seed_Psychoid（Items_Seeds_Core.xml）；SeedAloe、SeedNeutroFlower、SeedWildRose、SeedHypericum、SeedMint（Items_Seeds_CoreSK.xml）。未发现 Healroot 种子。

## 4. 中间产物层

### 4.1 MedicineHerbal（草药原料，原版 def，HSK patch 改属性）

- 来源：Plant_Healroot 收获 ×8。
- 本身即药品：MedicalPotency 0.30 / QualityMax 0.60，30 天腐烂。
- patch：`Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs/ThingDef_Medicines.xml`（归类 PlantMatter）。

### 4.2 DriedLeavesSmokeleaf（干烟叶，HSK 新增）

- 定义：`Core_SK/Defs/Drugs/DriedMatter.xml`。
- 晾晒（UniversalFermenterSK）：SmokeleafLeaves ×5 → ×5，4 天 @ DryingRack（`Recipes_UF_Drying.xml`）；电动版 5/25/50 叶 @ ElectricDryingRack（`Recipes_UF_Drying_Industrial.xml`）。

### 4.3 Neutroamine（中性胺）——两条获取途径

1. **自然合成 `MakeNaturalNeutroamine`**（`Core_SK/Defs/RecipeDefs/Recipes_Drug.xml`）：
   NeutroPetals ×8 + Hypericum ×2 → Neutroamine ×1 ｜ work 700 ｜ Medicine 6 + Crafting 3 ｜ @ DrugLab ｜ 研究 Drugs_C2
2. **石化精炼**：建筑 `NeutroamineRefiner`（`Rimefeller_SK/Defs/ThingDefs_Buildings/BuildingsB_Refining.xml`）——化合燃料 0.6/批 → Neutroamine ×1（缓冲 25），耗电 2500W，研究 Oil_Industry_D1，造价 SyntheticFibers×25 + Glass×25 + Electronics×15 + ComponentSpacer×5 + 160 金属。
3. 商人处购买。
- 物品 patch：堆叠 400、市价 10、归 MedicalMaterials（`Core_SK_CoreModule/Patches/AutoGenPatches/Drugs/Neutroamine.xml`）。

### 4.4 MedicalDrink（抗生素，HSK 新增）

- 定义：`Core_SK/Defs/ThingDefs_Meals/Items_Meals_Medical.xml`。
- `BrewMedicalDrink`（`Recipes_Meals_Medicals.xml`）：MedicineHerbal ×1 + RawHops/Yeast ×10 → ×1 ｜ work 1000 ｜ Medicine 10 ｜ @ meditable ｜ 研究 PenoxycylineProduction。
- 是 MedicineUltratech 的原料之一。

### 4.5 绷带（HSK 新增，`Recipes_Medicine.xml`）

- `MakeBandagekitcrude`：BTextiles ×7 → 粗绷带 ×1（效力 0.3）｜ work 150 ｜ Medicine 1/Crafting 1 ｜ 研究 Medicine_A1 ｜ @ CraftingSpot/LeatherworkerTable/meditable/手工及电动缝纫台（批量版 21→3）
- `MakeBandageKit`：BTextiles ×10 → 绷带包 ×1（效力 0.4）｜ work 250 ｜ Medicine 2/Crafting 2 ｜ 研究 Medicine_A2 ｜ @ meditable/缝纫台（批量版 30→3）

## 5. 成品药层

| 药品 defName | 效力 | 配方（输入 → 输出） | work / 技能 | 研究 | 工作台 |
| --- | --- | --- | --- | --- | --- |
| HerbMedicine（草药包，HSK 新增） | 0.60 | MedicineHerbal ×8 + Ash ×2 → ×1 | 500 / Medicine 5 | Medicine_B1 | meditable |
| Herbmedicine（基础医药包，HSK 新增，注意仅大小写差异） | 0.85 | aloe ×5 + Bandagekit ×1 → ×1；或 WildRose+Hypericum+(Mint/aloe)+绷带 混合方 | 500 / Medicine 4, Crafting 2 | Medicine_B2 | DrugLab / TablePrimitiveProsthetic |
| MedicineIndustrial（原版 def，原版配方被 patch 删除） | 1.00 | Neutroamine ×2 + MedicineHerbal ×6 + Paraffins ×10 + SyntheticFibers ×2 + Plastic ×10 → ×1（`makemedicine`） | 800 / Medicine 10 | MedicineProduction | meditable |
| MedicineUltratech（原版 def，HSK 自定义配方 `makeadvmedicine`） | 1.30 | Microchips ×1 + MedicalDrink ×2 + BiosyntheticMaterial ×2 + Paraffins ×3 + Neutroamine ×2 + Synthread ×5 + Plastic ×20 → ×1 | 1400 / Medicine 15 | Medicine_D1 | meditable |

### 5.1 关键建筑

- **`meditable` 医疗台**（`Core_SK/Defs/ThingDefs_Buildings/Buildings_Production_Medicine.xml`）：HSK 核心制药台。研究前置 **DrugProduction**；造价 60 Glass + 45 Rubber + 10 ComponentIndustrial + 5 Electronics + 125 stuff；耗电 500W；建筑技能 8。内嵌 MakeHerbMedicine、makemedicine、makeadvmedicine。
- **`DrugLab` 药物实验台**（原版，patch `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Production.xml`）：2×1，造价 45 Rubber + 25 Glass + 8 ComponentIndustrial + 5 Mechanism + 150 stuff，研究前置改 **Chemistry_C2**，建筑技能 12。**HSK 没有电动版药物实验台**，meditable 即高级制药台。

## 6. 毒品/兴奋剂链

### 6.1 烟叶链

Plant_Smokeleaf（Drug_plants_A1）→ SmokeleafLeaves ×4 → 晾晒 4 天 → DriedLeavesSmokeleaf → **SmokeleafJoint**（原版 def，patch `Core_SK_CoreModule/Patches/AutoGenPatches/Drugs/Smokeleaf.xml`）：原料改 DriedLeavesSmokeleaf ×4，workToMake 450，技能改 **Cooking**，@ CraftingSpot + DrugLab，研究 **Drugs_A1**。

### 6.2 精神叶链

Plant_Psychoid（Drug_plants_A2）→ PsychoidLeaves ×10 →

- **PsychiteTea**（精神茶）：原料沿用原版 PsychoidLeaves ×4 @ 灶台/篝火；研究被 patch 提前到 **Food_00**（`Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals_Drinks.xml`），摄入加 Rest+0.1、耐受 0.03。
- **Flake**（薄片）：原版配方 ×4 @ DrugLab；研究 patch 为 **Drugs_C3**（`Core_SK_CoreModule/Patches/Mods_ResearchTree/ResearchPrerequisites/Core.xml`）。
- **Yayo**（雅戈）：原版配方 ×8 @ DrugLab；研究 PsychiteRefining（前置 Drugs_C3）。
- **Berserk_herb**（狂暴草，HSK 新增）：MedicineHerbal ×3 + PsychoidLeaves ×3 @ CraftingSpot + DrugLab；研究 PsychoidBrewing。减痛 0.72、意识/移动 +0.15，高剂量有心脏病风险（`Core_SK/Defs/Drugs/Drugs.xml`）。

### 6.3 合成兴奋剂 / 防疫药（都吃 Neutroamine，@ DrugLab）

| 药品 | 原料 | 研究 |
| --- | --- | --- |
| WakeUp 清醒丸 | Neutroamine ×2 | WakeUpProduction（←Drugs_C2） |
| GoJuice 果汁剂 | Neutroamine ×2 + Yayo ×1 | GoJuiceProduction |
| Penoxycyline 青霉素 | Neutroamine ×2 | Drugs_C2（patch）；免疫清单加 Animal_Plague（`SK_Patches_PenoxycylineChange.xml`） |
| Methadone 美沙酮（HSK 新增） | Neutroamine ×1 | PenoxycylineProduction |
| VG_Ibuprofen 止痛药（HSK 新增） | Neutroamine ×1（批量 4→4，work 720，Medicine 10） | Drugs_C2 / PenoxycylineProduction |
| Synchronisity 同步粉（HSK 新增） | Synchronite ×8 | SynchronisityRefining |
| Luciferium 路西法合剂 | **不可制作**，仅商人出售（7~25 组） | — |

### 6.4 HSK 新增其他药品

| 物品 | 原料 | 工作台 | 研究 |
| --- | --- | --- | --- |
| HerbalTea 草药茶（治食物中毒） | WildRose ×2 + Hypericum ×1 + MintLeaves ×2 → ×6 ｜ work 780 | Brewery/ElectricBrewery/meditable/DrugLab | DrugProduction |
| ActivatedCarbon 活性炭丸 | Charcoal/Coal ×5 → ×9 ｜ work 780 ｜ Medicine 3/Crafting 5 | Kiln/meditable/DrugLab | DrugProduction |
| AntidoteSimple 原始抗毒剂 | MedicineHerbal ×1 + PoisonGland ×1 | TableButcher/TableKitchen/meditable/DrugLab | Medicine_A2 |
| AntidoteSafe 抗毒剂 | Neutroamine ×1 + PoisonGland ×1 | meditable/DrugLab | DrugProduction |
| MushroomTincture 酊剂 | GleamcapStem ×2 | ButcherSpot/TableButcher/TableKitchen | Medicine_A1 |
| HealrootTincture 草药酊剂 | MedicineHerbal ×3 + Ash ×3 | 同上 | Medicine_A2 |
| 碘片（Rimatomics_SK） | Neutroamine ×3（抗辐射） | — | — |

## 7. 关键 Patch 清单

| Patch 文件 | 内容 |
| --- | --- |
| `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs/ThingDef_Medicines.xml` | 改写三类 Medicine 属性；**删除 MedicineIndustrial 原版配方** |
| `Core_SK_CoreModule/Patches/AutoGenPatches/Drugs/Neutroamine.xml` | 堆叠 400、市价 10、归 MedicalMaterials |
| `Core_SK_CoreModule/Patches/AutoGenPatches/Drugs/Smokeleaf.xml` | 烟卷改干烟叶原料、Cooking 技能、研究 Drugs_A1 |
| `Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` | 精神茶研究提前到 Food_00 |
| `Core_SK_CoreModule/Patches/AutoGenPatches/ResearchProjectDefs/Research_Medicine.xml` | 10 个原版医疗/毒品研究改名、改前置、移入 Medicine_SK 页 |
| `Core_SK_CoreModule/Patches/Mods_ResearchTree/ResearchPrerequisites/Core.xml` | 青霉素配方 → Drugs_C2；Flake → Drugs_C3 |
| `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Production.xml` | DrugLab 改造价/尺寸，研究 → Chemistry_C2 |
| `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Medical.xml` | Healroot 产量 8 / 5.8 天，需 Medical_Plants_A1 |
| `Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` | Smokeleaf 产 4 叶、Psychoid 产 10 叶及研究前置 |
| `CombatExtended/Patches/Core/Drugs/Drugs.xml` | CE 给毒品类加 Bulk；GoJuice/Yayo 高加 Suppressability -0.5 |

## 8. 完整链条路径速查

- **工业医药链**：Oil_Industry_C5 → DrugProduction → 建 meditable；Flowers_A2 → 水培 NeutroFlower → NeutroPetals + 野生 Hypericum →（Drugs_C2, DrugLab）Neutroamine；Medical_Plants_A1 → Healroot → MedicineHerbal；石化线产 Paraffins/Plastic/SyntheticFibers →（MedicineProduction, meditable, Medicine 10）**MedicineIndustrial**。
- **闪耀医药链**：上述 + PenoxycylineProduction → MedicalDrink；+ Microchips/BiosyntheticMaterial/Synthread →（Medicine_D1, meditable, Medicine 15）**MedicineUltratech**。
- **草药链**：Healroot → MedicineHerbal（0.30 直接用）→ +Ash（Medicine_B1, meditable）HerbMedicine 0.60；或 Aloe/混合草药 + Bandagekit（Medicine_B2, DrugLab）Herbmedicine 0.85。
- **中性胺两条路**：① 自然：NeutroPetals×8 + Hypericum×2 → DrugLab（Drugs_C2）；② 石化：NeutroamineRefiner 烧化合燃料（Oil_Industry_D1）。
- **烟叶链**：Drug_plants_A1 → 种植 → 晾晒 4 天 → DriedLeavesSmokeleaf →（Drugs_A1, Cooking）SmokeleafJoint。
- **精神叶链**：Drug_plants_A2 → PsychoidLeaves → 精神茶（Food_00, 灶台）/ Flake（Drugs_C3）/ Yayo（PsychiteRefining）→ +Neutroamine → GoJuice。
