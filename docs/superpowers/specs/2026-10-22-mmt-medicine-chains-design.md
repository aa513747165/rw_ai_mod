# MMT 药品生产链设计

- 日期：2026-10-22
- 状态：待用户审阅
- 参考文档：[docs/hsk-medicine-chains.md](../../hsk-medicine-chains.md)（HSK 药品链提取报告，基准提交 b99b17d2）

## 1. 背景与目标

MMT 目前没有任何药品/毒品生产内容，医药完全依赖原版收获与商人。本设计参考 HSK 的四层药品链（原料种植 → 中间产物 → 成品药 → 毒品/兴奋剂），在 MMT 中实现完整药品生产体系，并与 MMT 已有石化链（`MMT_Plastic`/`MMT_Paraffins`/`MMT_SyntheticFibers`）、农业链（野玫瑰/金丝桃/薄荷）和电子链（`MMT_Microchip`）耦合。

成功标准：
- 游戏内可经种植+加工产出全部效力层级药品（0.30 → 1.30）与主要毒品；
- 中性胺有自然合成与石化精炼两条获取路径；
- `python tools/validate_mod.py medicine` 套件通过；
- 同名改造不删除任何原版 def/配方（遵循 [[替换原版不删除原则]]：只从工作台 recipes 列表移除或改写属性）。

## 2. 已确认的范围决策

| 决策点 | 结论 |
| --- | --- |
| 实现范围 | 医疗链 + 毒品兴奋剂链（全量） |
| 原版已有 def 策略 | 同名改造（保留 defName，改配方/属性，隐藏原版制作配方） |
| 中性胺路径 | 双路径：自然合成（花瓣+金丝桃 @ DrugLab）+ 石化精炼厂（烧化合燃料） |
| 实施方案 | 全量一次实现，按逻辑单元分步 commit |

**明确不做（YAGNI / 依赖缺失）**：抗毒剂×2（依赖 HSK 特有 PoisonGland）、同步粉（依赖 Synchronite）、碘片（Rimatomics）、Luciferium 制作（HSK 本也不可造）、SeedsPlease 种子、UniversalFermenter 发酵（MMT 无此框架，晾晒用长工时配方模拟）。

## 3. 新增物品（`MMT_` 前缀，中文 label 写死）

### 3.1 植物（追加到 `ThingDefs_Plants/MMT_Plants_Cultivated.xml`）

| defName | label | 产物 | 生长期 | 约束 | 研究前置 |
| --- | --- | --- | --- | --- | --- |
| MMT_PlantNeutroFlower | 中性花 | MMT_NeutroPetals ×5 | 6.2 天 | 仅限水培（sowTags 仅 Hydroponic） | MMT_MedicalPlants_A2 |
| MMT_PlantAloe | 芦荟 | MMT_Aloe ×4 | 4.7 天，sow 技能 10 | — | MMT_MedicalPlants_A2 |

### 3.2 资源/中间产物（新文件 `ThingDefs_Resources/MMT_Items_Resource_Medicine.xml`）

| defName | label | 说明 |
| --- | --- | --- |
| MMT_NeutroPetals | 中性花瓣 | 中性胺合成原料 |
| MMT_Aloe | 芦荟 | 基础医药包原料 |
| MMT_DriedLeavesSmokeleaf | 干烟叶 | 烟卷原料 |
| MMT_Bandagekitcrude | 粗制绷带 | 效力 0.30，可止血 |
| MMT_Bandagekit | 绷带包 | 效力 0.40 |
| MMT_MedicalDrink | 抗生素 | 闪耀医药原料 |

### 3.3 成品药/毒品（新文件 `ThingDefs_Items/MMT_Items_Medicine.xml`）

| defName | label | 效力/效果 | 对应 HSK |
| --- | --- | --- | --- |
| MMT_HerbMedicine | 草药包 | 0.60 | HerbMedicine |
| MMT_BasicMedicineKit | 基础医药包 | 0.85 | Herbmedicine（HSK 命名仅靠大小写区分，MMT 改名避免混淆） |
| MMT_HerbalTea | 草药茶 | 治食物中毒 | HerbalTea |
| MMT_ActivatedCarbon | 活性炭丸 | 解毒/食物中毒 | ActivatedCarbon |
| MMT_BerserkHerb | 狂暴草 | 战斗兴奋剂 | Berserk_herb |
| MMT_Methadone | 美沙酮 | 戒断替代 | Methadone |
| MMT_Ibuprofen | 止痛药 | 减痛 | VG_Ibuprofen |

### 3.4 物品类别

`ThingCategoryDefs/MMT_ThingCategories.xml` 追加：`MMT_MedicalSupplies`（医疗材料，父类 Medicine 或 Resources）——绷带/花瓣/芦荟/干烟叶/抗生素归入。

## 4. 建筑（新文件 `ThingDefs_Buildings/MMT_Buildings_Medicine.xml`，手工+电动成对）

| defName | label | 耗电 | 研究前置 | 用途 |
| --- | --- | --- | --- | --- |
| MMT_MedicalTable | 手工制药台 | 0 | MMT_Medicine_A2 | 绷带、草药包、基础医药包 |
| MMT_ElectricMedicalTable | 电动制药台 | 500W | MedicineProduction（原版同名 patch 后） | 工业医药、闪耀医药、抗生素 |
| MMT_DryingRack | 晾晒架 | 0 | MMT_Drugs_A1 | 烟叶→干烟叶（长工时） |
| MMT_ElectricDryingRack | 电动晾晒架 | 400W | MMT_Drugs_B1 | 同上（批量/更快） |
| MMT_NeutroamineRefiner | 中性胺精炼厂 | 2500W | MMT_Drugs_C2 | 化合燃料→Neutroamine |

- 原版 `DrugLab` 保留不动（挂毒品配方）；仅 patch 其研究前置（见 §7）。
- 造价对齐 HSK 风格：玻璃+橡胶+零部件+stuff，电动版加电子件；建筑贴图从 HSK 授权贴图复制（含 `_east/_north/_south` + `_m` 遮罩）。

## 5. 配方（新文件 `RecipeDefs/MMT_Recipes_Medicine.xml`、`MMT_Recipes_Drugs.xml`）

### 5.1 中间产物

| 配方 defName | 输入 → 输出 | work | 技能 | 工作台 | 研究 |
| --- | --- | --- | --- | --- | --- |
| MMT_MakeNeutroamine | MMT_NeutroPetals×8 + MMT_Hypericum×2 → Neutroamine×1 | 700 | Medicine 6 + Crafting 3 | DrugLab | MMT_Drugs_C2 |
| （精炼厂配方） | Chemfuel 0.6/批 → Neutroamine×1（缓冲 25） | — | — | MMT_NeutroamineRefiner | MMT_Drugs_C2 |
| MMT_DrySmokeleaf / _x5 | SmokeleafLeaves×5/25 → MMT_DriedLeavesSmokeleaf×5/25 | 长工时（模拟 4 天晾晒） | Cooking | 晾晒架×2 | MMT_Drugs_A1 / B1 |
| MMT_MakeBandagekitcrude | 纺织品类×7 → ×1（批量 21→3） | 150 | Medicine 1 | 手工制药台 | MMT_Medicine_A1 |
| MMT_MakeBandagekit | 纺织品类×10 → ×1（批量 30→3） | 250 | Medicine 2 | 制药台×2 | MMT_Medicine_A2 |
| MMT_BrewMedicalDrink | MedicineHerbal×1 + 啤酒花/酵母×10 → ×1 | 1000 | Medicine 10 | 电动制药台 | PenoxycylineProduction |

### 5.2 成品药

| 配方 | 输入 → 输出 | work | 技能 | 工作台 | 研究 |
| --- | --- | --- | --- | --- | --- |
| MMT_MakeHerbMedicine | MedicineHerbal×8 + Ash×2 → MMT_HerbMedicine×1 | 500 | Medicine 5 | 制药台×2 | MMT_Medicine_B1 |
| MMT_MakeBasicMedicineKit | MMT_Aloe×5 + MMT_Bandagekit×1 → ×1；混合方：MMT_WildRose+MMT_Hypericum+(MMT_MintLeaves/MMT_Aloe)+绷带 | 500 | Medicine 4 | 制药台×2 | MMT_Medicine_B2 |
| MMT_MakeMedicineIndustrial | Neutroamine×2 + MedicineHerbal×6 + MMT_Paraffins×10 + MMT_SyntheticFibers×2 + MMT_Plastic×10 → MedicineIndustrial×1 | 800 | Medicine 10 | 电动制药台 | MedicineProduction |
| MMT_MakeMedicineUltratech | MMT_Microchip×1 + MMT_MedicalDrink×2 + Neutroamine×2 + MMT_Paraffins×3 + MMT_Plastic×20 + Synthread×5 → MedicineUltratech×1 | 1400 | Medicine 15 | 电动制药台 | MMT_Medicine_D1 |

注：HSK 的闪耀药配方含 BiosyntheticMaterial（HSK 特有），MMT 无此资源，实现时从 MMT 现有高级材料中择一替代（候选：MMT_SyntheticFibers 增量 或 电子链高级件），在实现阶段定稿并记录。

### 5.3 毒品/兴奋剂（同名改造沿用原版配方者见 §7，此处为新增）

| 配方 | 输入 → 输出 | 工作台 | 研究 |
| --- | --- | --- | --- |
| MMT_MakeHerbalTea | MMT_WildRose×2 + MMT_Hypericum×1 + MMT_MintLeaves×2 → MMT_HerbalTea×6 | 炉灶/电动炉灶/制药台 | DrugProduction |
| MMT_MakeActivatedCarbon | Charcoal/Coal×5 → MMT_ActivatedCarbon×9 | 窑炉/制药台 | DrugProduction |
| MMT_MakeBerserkHerb | MedicineHerbal×3 + PsychoidLeaves×3 → MMT_BerserkHerb×1 | CraftingSpot/DrugLab | PsychoidBrewing |
| MMT_MakeMethadone | Neutroamine×1 → MMT_Methadone×1 | DrugLab | PenoxycylineProduction |
| MMT_MakeIbuprofen | Neutroamine×1 → MMT_Ibuprofen×1（批量 4→4） | DrugLab | MMT_Drugs_C2 |

## 6. 研究（新文件 `ResearchProjectDefs/MMT_Research_Medicine.xml`，tab=MMT_ResearchTab）

```
医疗线：MMT_Medicine_A1(150) → A2(200) → B1(400) → B2(500)
        → MedicineProduction(原版, patch 前置=B2, 1000) → MMT_Medicine_D1(2000)
毒品线：MMT_Drugs_A1(75) → PsychoidBrewing(原版, patch 前置=A1)
        → MMT_Drugs_B1(300) → DrugProduction(原版, patch 前置=B1, 1300)
        → MMT_Drugs_C2(1000) → PenoxycylineProduction / WakeUpProduction(原版, patch 前置=C2)
        → MMT_Drugs_C3(900)  → PsychiteRefining(原版, patch 前置=C3)
        → GoJuiceProduction(原版, patch 前置=WakeUpProduction+PsychiteRefining)
种植线：MMT_DrugPlants_A1(100, 烟叶) → MMT_DrugPlants_A2(150, 精神叶)
        MMT_MedicalPlants_A1(150, Healroot) → MMT_MedicalPlants_A2(200, 芦荟+中性花)
```

- DrugProduction 另加 MMT 石化链终端研究为第二前置（对齐 HSK「DrugProduction 需石化研究」的耦合设计，实现时取 `MMT_Research_Petrochem.xml` 的终端研究名）。

## 7. 同名改造 Patch（新文件 `Patches/MMT_Patch_VanillaMedicine.xml`、`MMT_Patch_VanillaDrugs.xml`、`MMT_Patch_VanillaDrugPlants.xml`）

遵循现有 patch 风格（`PatchOperationSequence`+Replace，详细中文注释；**不删除任何原版 def/RecipeDef**，只从工作台 recipes 列表移除条目或改写属性）。

| 目标 def | 改动 |
| --- | --- |
| MedicineHerbal | 属性 patch：效力 0.30/上限 0.60、30 天腐烂、归类 PlantMatter 系 |
| MedicineIndustrial | 从 DrugLab recipes 移除原版 `Make_Medicine`；制作由 `MMT_MakeMedicineIndustrial` 接管 |
| MedicineUltratech | 原版本不可制作，无需隐藏；仅新增 MMT 配方 |
| Neutroamine | 堆叠 400、市价 10 |
| SmokeleafJoint | 配方 patch：原料 SmokeleafLeaves×4 → MMT_DriedLeavesSmokeleaf×4、技能 Cooking、研究 MMT_Drugs_A1 |
| PsychiteTea | 不动（挂 PsychoidBrewing，随其前置 patch 自然融入 MMT 线） |
| Flake / Yayo | 配方不动；研究经 PsychiteRefining/C3 patch 融入（Flake 原版配方研究 PsychiteRefining） |
| WakeUp / GoJuice / Penoxycyline | 配方不动；研究前置 patch |
| Plant_Healroot | 产量 8、5.8 天、种植研究 MMT_MedicalPlants_A1 |
| Plant_Smokeleaf | 产量 4、种植研究 MMT_DrugPlants_A1 |
| Plant_Psychoid | 产量 10、种植研究 MMT_DrugPlants_A2 |
| DrugLab | 研究前置 → DrugProduction（patch 后已含 MMT 前置链）；造价补橡胶/玻璃 |
| 原版研究（MedicineProduction/DrugProduction/PsychoidBrewing/PenoxycylineProduction/WakeUpProduction/PsychiteRefining/GoJuiceProduction） | prerequisites patch 到 MMT 研究线（见 §6） |

## 8. 贴图

- 新增物品/建筑贴图从 `Hardcore-SK/Mods/Core_SK/Textures/` 复制（项目已获授权）：中性花、芦荟、花瓣、干烟叶、绷带×2、草药包、基础医药包、抗生素、草药茶、活性炭、狂暴草、美沙酮、止痛药、晾晒架×2、制药台×2、精炼厂。实现时逐一在 HSK 贴图目录定位对应文件。
- 多状态物品按 MMT 约定放同名子目录 `_a/_b/_c`；建筑带朝向与 `_m` 遮罩。
- 同名改造的原版成品（Medicine 系列、毒品）**不改 texPath**，保持原版外观（同零件 patch 先例）。

## 9. 验证与工具

1. `tools/validate_mod.py` 新增 `validate_medicine()` 并注册套件 `medicine`，校验：
   - 所有新 defName 带 `MMT_` 前缀（同名 patch 目标除外）；
   - 配方输入/输出引用的 ThingDef 均存在；研究前置链无悬空引用；
   - 药品效力阶梯精确为 0.30/0.60/0.85/1.00/1.30；
   - 中性胺双路径存在（配方 + 精炼厂）；
   - 新物品/建筑 texPath 对应 png 文件存在；
   - 更新文件顶部路径常量。
2. `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 增加药品映射（MedicineHerbal→MedicineHerbal、MMT_HerbMedicine→HerbMedicine、Neutroamine→Neutroamine 等），重跑生成 `docs/mmt-vs-hsk-resource-chains.md`。
3. 全量跑 `python tools/validate_mod.py all`（中文乱码时 `PYTHONIOENCODING=utf-8`）。
4. 编译验证：本任务纯 XML，无 C# 改动，无需 `dotnet build`；以游戏加载无红字为最终验证（无法自动化时在交付说明中注明需用户进游戏确认）。

## 10. 提交切分（中文 conventional commits，仅本地 commit）

1. `feat: 添加药品链资源与药用植物`（§3 物品 + 植物 + 贴图）
2. `feat: 添加制药台/晾晒架/中性胺精炼厂建筑`（§4）
3. `feat: 添加药品与毒品配方及研究线`（§5、§6）
4. `feat: 同名改造原版药品/毒品/毒株植物`（§7 patch）
5. `test: 添加 medicine 校验套件并更新对比文档`（§9）

每步完成后可编译（XML 加载层面）即 commit；`.java/.xml` 自动 `git add`，`.md/.png` 等其它文件先询问确认。

## 11. 风险与开放点

- **闪耀药替代原料**：BiosyntheticMaterial 的 MMT 替代在实现 §5.2 时定稿（候选已列）。
- **晾晒模拟**：无发酵框架，用长 workAmount 配方近似；若用户后续引入发酵框架可迁移。
- **DrugLab 造价 patch**：对齐 HSK（橡胶+玻璃+零部件）可能与其它依赖原版 DrugLab 造价的 mod 轻微冲突，属可接受范围。
- **游戏内实测**：数值（work、产量）先对齐 HSK，实测平衡性留待用户试玩反馈。
