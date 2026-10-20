# B3 石料建材 设计 spec

- 日期：2026-10-20
- 状态：已设计，待用户审阅
- 引用：`2026-10-20-b2-b8-batch-roadmap.md` §3（范围）与 §0（全局约定，此处不重复）

## 1. 启动澄清结论（用户已决策）

1. 对比文档生成脚本（`tools/build_mmt_chains_compare.js` 等）已丢失 → **本批跳过对比文档更新**（Def+研究+验证套件照常交付）。
2. 研究布局 → **新建两个研究**：`MMT_AgricultureBasic`（农业 I）、`MMT_ConcreteAdvanced`（混凝土 II）。
3. 肥料配方挂载 → **屠宰点 + 屠宰台 + 厨房类建筑**。
4. 堆肥输入 → **类别替代**，不新增腐肉/腐烂物资源。

## 2. HSK 查证结论（spec 阶段待查项）

- **肥料建筑**：HSK `MakeFertilizer`（堆肥×10→肥料×10，工作量 10000）无任何建筑挂载也无研究，是死配方；HSK 实际路线是堆肥桶发酵（属路线图排除的"发酵/缓处理"非目标）。MMT 自行挂载，按决策 3 执行。
- **厨房案桌（TableKitchen）是 HSK 自建建筑**，原版不存在。MMT 以原版炉灶 `FueledStove` + `ElectricStove` 替代"厨房"挂载位（保留 HSK"食物区可堆肥"的意图）。`ButcherSpot`、`TableButcher`、`Hay` 均为原版 def，可直接引用。
- **堆肥输入适配**：HSK 用了 MMT 没有的 `RottedMeat`/`RottedMush`/`Waste` 类别，按决策 4 替换为原版类别 + `Hay` + `MMT_Ash`。
- **盐资源不带 deep\* 参数**：HSK `Salt` 带深钻井参数，但深钻井产出对齐是路线图非目标，且 MMT 现有资源均无 deep\* 参数 → 不带。ingestible 去掉（MMT 不做食物链）。
- **电动钢筋混凝土 workSpeedStat**：HSK 为 `StonecuttingSpeed`，照抄。

## 3. 资源（新建 `MMT_Items_Resource_Agriculture.xml`，数值照抄 HSK）

| defName | label | MarketValue | stackLimit | thingCategories | 其它 |
|---|---|---|---|---|---|
| `MMT_Compost` | 堆肥料 | 2.4 | 300 | ResourcesRaw | DeteriorationRate 5、Beauty -14、Flammability 0.5、Mass 0.4、tradeability None |
| `MMT_Fertilizer` | 肥料 | 3.8 | 300 | ResourcesRaw | DeteriorationRate 5、Beauty -7、Flammability 1.5、Mass 0.35 |
| `MMT_Dirt` | 土堆 | 0.2 | 900 | ResourcesRaw | DeteriorationRate 0、Mass 0.5 |
| `MMT_Salt` | 盐 | 1 | 600 | ResourcesRaw | Mass 0.025；不带 deep\* / ingestible |

贴图（HSK 已授权复制）：`DirtPile.png`→`Things/Item/MMT/Dirt.png`、`Compost.png`、`Fertilizer.png`、`Salt.png` 同名复制到 `Things/Item/MMT/`。

## 4. 矿脉 + 挖盐点（新建 `MMT_Buildings_Agriculture.xml`）

- `MMT_MineableSalt`（压实的盐）：ParentName `RockBase`；mineableThing `MMT_Salt`、mineableYield 150、scatterCommonality 1.2、矿团 5~10、MaxHitPoints 900、veinMineable；贴图沿用 MMT 矿脉惯例 `RockFlecked_Atlas` + color `(199,201,197)` / colorTwo `(240,242,237)`。
- `MMT_SaltMine`（挖盐点）：`Building_WorkTable`，1×1，Graphic_Single，`terrainAffordanceNeeded=SmoothableStone`，WorkToBuild 2500，useHitPoints false，passability Standable，免研究（对齐 HSK `VG_SaltMine`）。贴图复制 `SaltMine.png` → `Things/Building/Production/MMT_SaltMine.png`。

## 5. 配方（8 条）

### 5.1 追加到既有 `MMT_Recipes_Concrete.xml`（3 条）

| defName | 输入 → 产出 | 工作量 | workSpeedStat | 挂载方式 | 研究 / 技能 |
|---|---|---|---|---|---|
| `MMT_MixConcrete_Electric` | MMT_Sand×45 + MMT_CrushedStone×30 → MMT_Concrete×15 | 600 | SmeltingSpeed | `MMT_EConcreteMixer` 建筑 recipes 列表 | MMT_ConcreteAdvanced / Crafting 6 |
| `MMT_MakeReinforcedConcrete_Electric` | [MMT_MetallicBar]×20 + MMT_Concrete×20 → MMT_ReinforcedConcrete×16 | 1200 | StonecuttingSpeed | `MMT_ElectricAssemblyBench` 建筑 recipes 列表 | 无（对齐 HSK）/ Crafting 7 |
| `MMT_MakeDirt_Electric` | MMT_Sand×20 + MMT_Fertilizer×10 → MMT_Dirt×30 | 1500 | SmeltingSpeed | `MMT_EConcreteMixer` 建筑 recipes 列表 | MMT_ConcreteAdvanced |

### 5.2 新建 `MMT_Recipes_Agriculture.xml`（5 条）

| defName | 输入 → 产出 | 工作量 | 挂载 | 研究 / 技能 |
|---|---|---|---|---|
| `MMT_MakeCompost` | [MeatRaw 或 AnimalProductRaw]×25 + [PlantFoodRaw 或 Hay 或 MMT_Ash]×5 → MMT_Compost×5 | 625 | recipeUsers: ButcherSpot / TableButcher / FueledStove / ElectricStove | MMT_AgricultureBasic |
| `MMT_MakeFertilizer` | MMT_Compost×10 → MMT_Fertilizer×10 | 10000 | 同上台 | MMT_AgricultureBasic |
| `MMT_MakeDirt` | MMT_Sand×2 + MMT_Fertilizer×1 → MMT_Dirt×3 | 300 | recipeUsers: ButcherSpot | MMT_AgricultureBasic |
| `MMT_MakeDirt_Hand` | MMT_Sand×20 + MMT_Fertilizer×10 → MMT_Dirt×30 | 3000 | `MMT_ConcreteMixer` 建筑 recipes 列表 | MMT_ConcreteBasic（既有） |
| `MMT_MineSalt` | — → MMT_Salt×5 | 1800 | `MMT_SaltMine` 建筑 recipes 列表 | 无；workSkill Mining |

堆肥配方细节照 HSK：workSpeedStat `DrugCookingSpeed`、effectWorking `Cook`、soundWorking `Recipe_CookMeal`、allowMixingIngredients、fixedIngredientFilter（categories FoodRaw + thingDefs Hay/MMT_Ash）。土堆系 workSpeedStat `SmeltingSpeed`、effectWorking `Smelt`。

## 6. 建筑（追加到既有 `MMT_Buildings_Concrete.xml`，1 个）

`MMT_EConcreteMixer`（电动混凝土搅拌机），对照 HSK `EConcreteMixer`：

- 2×1、`Building_WorkTable`、PassThroughOnly、hasInteractionCell、interactionCellOffset (0,0,-1)
- stuffCategories Metallic（MMT 手动搅拌器同款；HSK 用 StrongMetallic 属 SK 类别，不可用）、costStuffCount 115
- costList：MMT_Rubber×45 + ComponentIndustrial×10 + MMT_ElectronicParts×5 + MMT_Mechanism×5（HSK 的 Rubber/ComponentIndustrial/ElectronicComponents/Mechanism 一一对应 MMT 已有资源）
- statBases：WorkToBuild 5000、MaxHitPoints 180、Flammability 0.5、Beauty -40、Cleanliness -6、Mass 500
- comps：原版 `CompProperties_Power`（basePowerConsumption 500、shortCircuitInRain）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`（不用 SK.CompProperties_IdlePower）
- researchPrerequisites：MMT_ConcreteAdvanced；constructionSkillPrerequisite 11
- recipes：MMT_MixConcrete_Electric、MMT_MakeDirt_Electric
- 贴图：HSK `ConcreteMixerElectro_{east,north,south}[m].png` → `Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer_*`

## 7. 研究（2 个新）

| defName | label | baseCost | techLevel | 前置 | 文件 |
|---|---|---|---|---|---|
| `MMT_AgricultureBasic` | 农业 I | 150 | Neolithic | 无 | 新建 `MMT_Research_Agriculture.xml` |
| `MMT_ConcreteAdvanced` | 混凝土 II | 1000 | Industrial | MMT_ConcreteBasic | 追加 `MMT_Research_Concrete.xml` |

均 `<tab>MMT_ResearchTab</tab>`；researchViewX/Y 按 MMT 现有布局选不冲突坐标。

## 8. 验证套件 `stone-construction`

`tools/validate_mod.py` 新增 `validate_stone_construction()` 并注册到 `SUITES` + `validate_all`：

- 4 资源 defName/label/市价/堆叠/类别/腐烂率
- 盐矿脉产出/散布/矿团；挖盐点地形需求/尺寸
- 8 配方的输入输出数值、工作量、挂载关系（建筑 recipes 列表或 recipeUsers）、研究前置
- EConcreteMixer 电力组件/耗电/研究/配方列表
- 2 研究的 cost/前置/tab
- 贴图文件存在且为合法 PNG；新文件不得出现 `SK.` 引用

## 9. 提交分步（本地，中文 conventional commits）

1. `feat: 添加农业辅料资源与堆肥肥料配方`（资源 + 农业配方 + 农业研究 + 物品贴图）
2. `feat: 添加盐矿脉与挖盐点`（矿脉 + 挖盐点 + 挖盐配方 + 贴图）
3. `feat: 添加电动混凝土搅拌机与电动配方`（建筑 + 混凝土配方追加 + 混凝土 II 研究 + 贴图；含 MMT_ElectricAssemblyBench 挂钢筋混凝土）
4. `test: 添加 stone-construction 验证套件`

## 10. 明确不做

- 对比文档 / MMT_ALIAS 更新（脚本丢失，本批跳过）
- 堆肥桶发酵路线（路线图非目标）
- 盐的食物用途（腌肉等 3.3 食物链）、ingestible、深钻井参数
- HSK 的 RottedMeat/RottedMush/Waste 机制
