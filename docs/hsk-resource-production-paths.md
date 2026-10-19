# HSK 官方核心模块资源生产路径

- 数据源：`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`
- 生成日期：2026-10-19
- 范围：`Core_SK` 与官方 `Core_SK_*` 模块；不包含第三方独立 Mod。
- 统计口径：制造/发酵配方、矿脉与岩壁、深钻井、野外矿点、植物收获、动物屠宰/挤奶/剪毛/产蛋、自动生成器、物质生成、补丁新增 Def。
- 数值来源：XML 静态解析；`ParentName` 继承已按本地 Def 链合并；`MayRequire` 与 Patch 条件保留在“条件”列。
- 相关文档：[HSK 官方核心模块武器与防具](hsk-weapons-armor.md)。

## 1. 汇总

- 生产路径条目：**578**
- 有本地产出的资源：**332**
- 官方模块内识别资源但无本地产出：**87**
- Patch 新增 Def：**118**
- Patch 应用操作：**3763**
- Patch 未模拟操作：**562**

### 1.1 按获取途径统计

| 获取途径 | 条目数 |
| --- | --- |
| 动物产受精蛋 | 9 |
| 动物产未受精蛋 | 9 |
| 动物剪毛 | 12 |
| 动物挤奶 | 18 |
| 发酵/缓处理配方 | 68 |
| 工作台配方 | 302 |
| 摧毁残留 | 50 |
| 植物收获 | 50 |
| 深钻井 | 19 |
| 物质生成 | 1 |
| 矿脉开采 | 29 |
| 野外矿点 | 11 |

### 1.2 按资源大类统计

| 资源大类 | 生产路径条目数 |
| --- | --- |
| 其他资源 | 39 |
| 加工品 | 175 |
| 医疗 | 24 |
| 原材料 | 147 |
| 外部/原版 | 35 |
| 未分类 | 15 |
| 食物 | 143 |

## 2. 资源生产路径总表

### 2.1 原材料

#### Anglesite Ore（`Anglesite`）

- 分类：`ResourcesRaw/Extracted/Metals/HVY`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to lead ore（`ConvertToLead`） | Matter（inorganic matter） ×50 | Anglesite（Anglesite Ore）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 深钻井 | Anglesite Ore（`Anglesite`） | — | Anglesite（Anglesite Ore）×20 | 每份产量=20；每批规模=32~78；常见度=1.2 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Anglesite Deposits（`MineableLead`） | — | Anglesite（Anglesite Ore）×75 | 单格产量=75；散布常见度=0.8；矿团规模=6~13；耐久=6000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Anglesite（Anglesite Ore）×按矿点规则 | 权重=3.5 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### ash（`Ash`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | burn wood into ash (full load)（`MakeAsh_Pit_Full`） | [Wooden] ×100；Dirt（Pile of dirt） ×10 | Ash（ash）×30；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | burn wood into ash (half load)（`MakeAsh_Pit_Half`） | [Wooden] ×50；Dirt（Pile of dirt） ×5 | Ash（ash）×15；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 工作台配方 | Burn apparel（`BurnApparel`） | — | Ash（ash）×2 | 工作量=200 | 研究: Craft_0；建筑: Campfire, ElectricCrematorium, Kiln | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml` |
| 工作台配方 | Burn materials（`BurnThings`） | [Wooden] 或 [Woody] 或 [Textiles] 或 [Leathers] ×20 | Ash（ash）×3 | 工作量=400；速度属性=GeneralLaborSpeed | 研究: Craft_0；建筑: Campfire, ElectricCrematorium, Kiln | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Cremation.xml` |
| 工作台配方 | Cremate corpse（`CremateCorpse`） | — | Ash（ash）×4 | 工作量=600 | 研究: Craft_0；建筑: Campfire, ElectricCrematorium, Kiln | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml` |
| 工作台配方 | Cremate corpse（`CremateCorpseCampfire`） | [Corpses] ×1 | Ash（ash）×4 | 工作量=800；速度属性=GeneralLaborSpeed | 研究: Craft_0 | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Cremation.xml` |

#### bamboo log（`Bamboo`）

- 分类：`ResourcesRaw/Wooden`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_TreeBamboo（`Plant_TreeBamboo`） | — | Bamboo（bamboo log）×10 | 收获量=10；生长天数=4；收获工作量=200；最低肥力=0.15；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Bauxite Ore（`Aluminium`）

- 分类：`ResourcesRaw/Extracted/Metals/HCM`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to bauxite ore（`ConvertToAluminium`） | Matter（inorganic matter） ×50 | Aluminium（Bauxite Ore）×80 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 摧毁残留 | Aluminium Meteorite（`AluminiumBoulder`） | — | Aluminium（Bauxite Ore）×50 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 深钻井 | Bauxite Ore（`Aluminium`） | — | Aluminium（Bauxite Ore）×25 | 每份产量=25；每批规模=40~80；常见度=1.35 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Aluminium Meteorite（`AluminiumBoulder`） | — | Aluminium（Bauxite Ore）×100 | 单格产量=100；耐久=1000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 矿脉开采 | Bauxite Ore Deposits（`MineableAluminium`） | — | Aluminium（Bauxite Ore）×75 | 单格产量=75；散布常见度=0.65；矿团规模=11~19；耐久=5500 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Aluminium（Bauxite Ore）×按矿点规则 | 权重=2 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Carbon（`Carbon`）

- 分类：`ResourcesRaw/Extracted/Hydrocarbons`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create carbon（`MakeCarbon`） | Polymers（Polymers） ×25；[Coal] ×25；Rubber（Rubber） ×15 | Carbon（Carbon）×12 | 工作量=4000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Oil_Industry_C6；技能: Crafting≥15；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### Charcoal（`Charcoal`）

- 分类：`ResourcesRaw/Extracted/Coal`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | burn wood into charcoal (full load)（`MakeCharcoal_Pit_Full`） | [Wooden] ×100；Dirt（Pile of dirt） ×10 | Charcoal（Charcoal）×80；Dirt（Pile of dirt）×6 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | burn wood into charcoal (half load)（`MakeCharcoal_Pit_Half`） | [Wooden] ×50；Dirt（Pile of dirt） ×5 | Charcoal（Charcoal）×40；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 工作台配方 | create charcoal（`MakeCharcoal_Electric`） | [Wooden] ×25 | Charcoal（Charcoal）×20 | 工作量=800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥4；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | create charcoal（`MakeCharcoal_Hand`） | [Wooden] ×25 | Charcoal（Charcoal）×20 | 工作量=1600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；建筑: Campfire, Kiln, TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Coal Ore（`Coal`）

- 分类：`ResourcesRaw/Extracted/Coal`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 深钻井 | Coal Ore（`Coal`） | — | Coal（Coal Ore）×50 | 每份产量=50；每批规模=95~150；常见度=2.0 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| 矿脉开采 | Coal Ore Deposits（`MineableCoal`） | — | Coal（Coal Ore）×75 | 单格产量=75；散布常见度=1.2；矿团规模=11~18；耐久=1500 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |

#### coffee beans（`CoffeeBeans`）

- 分类：`ResourcesRaw/DriedPlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | process coffee beans（`Make_CoffeeBeans`） | RawCoffee（coffee berries） ×5 | CoffeeBeans（coffee beans）×5 | 处理天数=5；槽位=2；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=15~65；理想温度=25~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |
| 发酵/缓处理配方 | process coffee beans（`Make_CoffeeBeansIndustrial_x1`） | RawCoffee（coffee berries） ×5 | CoffeeBeans（coffee beans）×5 | 处理天数=3；槽位=1 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | process coffee beans x10（`Make_CoffeeBeansIndustrial_x10`） | RawCoffee（coffee berries） ×50 | CoffeeBeans（coffee beans）×50 | 处理天数=3；槽位=10 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | process coffee beans x5（`Make_CoffeeBeansIndustrial_x5`） | RawCoffee（coffee berries） ×25 | CoffeeBeans（coffee beans）×25 | 处理天数=3；槽位=5 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | process coffee beans x5（`Make_CoffeeBeans_x5`） | RawCoffee（coffee berries） ×25 | CoffeeBeans（coffee beans）×25 | 处理天数=5；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=15~65；理想温度=25~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |

#### Coldstone（`Coldstone`）

- 分类：`ResourcesRaw/Extracted`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 深钻井 | Coldstone（`Coldstone`） | — | Coldstone（Coldstone）×15 | 每份产量=15；每批规模=15~35；常见度=1 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| 矿脉开采 | Coldstone Deposit（`MineableColdstone`） | — | Coldstone（Coldstone）×50 | 单格产量=50；散布常见度=0.3；矿团规模=4~7；耐久=3200 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |

#### compost（`Compost`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make compost（`MakeCompost`） | RottedMeat（rotted meat） 或 [MeatRaw] 或 [AnimalProductRaw] 或 [Waste] ×25；Hay（Hay） 或 RottedMush（rotted mush） 或 Ash（ash） 或 [PlantFoodRaw] ×5 | Compost（compost）×5 | 工作量=625；速度属性=DrugCookingSpeed；工作技能=Crafting；技能经验系数=0.25；允许混合材料=true | 研究: SewageSludgeComposting；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_FertileFields.xml` |

#### Copper Ore（`Copper`）

- 分类：`ResourcesRaw/Extracted/Metals/HCM`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to copper ore（`ConvertToCopper`） | Matter（inorganic matter） ×50 | Copper（Copper Ore）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 摧毁残留 | Copper Meteorite（`CopperBoulder`） | — | Copper（Copper Ore）×50 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 深钻井 | Copper Ore（`Copper`） | — | Copper（Copper Ore）×40 | 每份产量=40；每批规模=72~145；常见度=1.5 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Copper Meteorite（`CopperBoulder`） | — | Copper（Copper Ore）×100 | 单格产量=100；耐久=1500 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 矿脉开采 | Copper Ore Deposits（`MineableCopper`） | — | Copper（Copper Ore）×75 | 单格产量=75；散布常见度=0.95；矿团规模=15~25；耐久=1800 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Copper（Copper Ore）×按矿点规则 | 权重=3 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### dried smokeleaf leaves（`DriedLeavesSmokeleaf`）

- 分类：`ResourcesRaw/DriedPlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make dried smokeleaf（`Make_Smokeleaf`） | SmokeleafLeaves（SmokeleafLeaves） ×5 | DriedLeavesSmokeleaf（dried smokeleaf leaves）×5 | 处理天数=4；槽位=2；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=0~65；理想温度=30~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |
| 发酵/缓处理配方 | make dried smokeleaf（`Make_SmokeleafIndustrial_x1`） | SmokeleafLeaves（SmokeleafLeaves） ×5 | DriedLeavesSmokeleaf（dried smokeleaf leaves）×5 | 处理天数=2；槽位=1 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried smokeleaf x10（`Make_SmokeleafIndustrial_x10`） | SmokeleafLeaves（SmokeleafLeaves） ×50 | DriedLeavesSmokeleaf（dried smokeleaf leaves）×50 | 处理天数=2；槽位=10 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried smokeleaf x5（`Make_SmokeleafIndustrial_x5`） | SmokeleafLeaves（SmokeleafLeaves） ×25 | DriedLeavesSmokeleaf（dried smokeleaf leaves）×25 | 处理天数=2；槽位=5 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried smokeleaf x5（`Make_Smokeleaf_x5`） | SmokeleafLeaves（SmokeleafLeaves） ×25 | DriedLeavesSmokeleaf（dried smokeleaf leaves）×25 | 处理天数=4；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=0~65；理想温度=30~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |

#### dried tobacco leaves（`DriedLeavesTobacco`）

- 分类：`ResourcesRaw/DriedPlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make dried tobacco（`Make_Tobacco`） | RawTobacco（Tobacco Leaves） ×5 | DriedLeavesTobacco（dried tobacco leaves）×5 | 处理天数=3；槽位=2；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=0~65；理想温度=30~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |
| 发酵/缓处理配方 | make dried tobacco（`Make_TobaccoIndustrial_x1`） | RawTobacco（Tobacco Leaves） ×5 | DriedLeavesTobacco（dried tobacco leaves）×5 | 处理天数=1；槽位=1 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried tobacco x10（`Make_TobaccoIndustrial_x10`） | RawTobacco（Tobacco Leaves） ×50 | DriedLeavesTobacco（dried tobacco leaves）×50 | 处理天数=1；槽位=10 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried tobacco x5（`Make_TobaccoIndustrial_x5`） | RawTobacco（Tobacco Leaves） ×25 | DriedLeavesTobacco（dried tobacco leaves）×25 | 处理天数=1；槽位=5 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried tobacco x5（`Make_Tobacco_x5`） | RawTobacco（Tobacco Leaves） ×25 | DriedLeavesTobacco（dried tobacco leaves）×25 | 处理天数=3；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=0~65；理想温度=30~50 | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |

#### fertilizer（`Fertilizer`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make fertilizer（`Make_Fertilizer`） | Compost（compost） ×1 | Fertilizer（fertilizer）×1 | 处理天数=2.8；低于安全温度速度=0.5；高于安全温度速度=1；安全温度=-1~49；理想温度=10~30 | 建筑: CompostBarrel | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CompostBarrel.xml` |
| 发酵/缓处理配方 | make fertilizer x5（`Make_Fertilizer_x5`） | Compost（compost） ×5 | Fertilizer（fertilizer）×5 | 处理天数=2.8；槽位=5；低于安全温度速度=0.5；高于安全温度速度=1；安全温度=-1~49；理想温度=10~30 | 建筑: CompostBarrel | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CompostBarrel.xml` |
| 工作台配方 | make fertilizer（`MakeFertilizer`） | Compost（compost） ×10 | Fertilizer（fertilizer）×10 | 工作量=10000；速度属性=DrugCookingSpeed；工作技能=Crafting | — | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_FertileFields.xml` |

#### Glowstone（`Glowstone`）

- 分类：`ResourcesRaw/Extracted`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 深钻井 | Glowstone（`Glowstone`） | — | Glowstone（Glowstone）×15 | 每份产量=15；每批规模=15~35；常见度=1 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| 矿脉开采 | Glowstone Deposit（`MineableGlowstone`） | — | Glowstone（Glowstone）×50 | 单格产量=50；散布常见度=0.35；矿团规模=3~6；耐久=3400 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |

#### Gold Ore（`Gold`）

- 分类：`ResourcesRaw/Extracted/Metals/PRS`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to gold ore（`ConvertToGold`） | Matter（inorganic matter） ×60 | Gold（Gold Ore）×20 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 深钻井 | Gold Ore（`Gold`） | — | Gold（Gold Ore）×10 | 每份产量=10；每批规模=1~12；常见度=0.6 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Gold Ore Deposits（`MineableGold`） | — | Gold（Gold Ore）×50 | 单格产量=50；散布常见度=0.15；矿团规模=3~9；耐久=4500 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Gold（Gold Ore）×按矿点规则 | 权重=0.75 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Ilmenite Ore（`Ilmenite`）

- 分类：`ResourcesRaw/Extracted/Metals/RAR`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to ilmenite ore（`ConvertToTitanium`） | Matter（inorganic matter） ×200 | Ilmenite（Ilmenite Ore）×20 | 工作量=15000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 深钻井 | Ilmenite Ore（`Ilmenite`） | — | Ilmenite（Ilmenite Ore）×15 | 每份产量=15；每批规模=25~58；常见度=1.1 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Ilmenite Ore Deposits（`MineableTitanium`） | — | Ilmenite（Ilmenite Ore）×75 | 单格产量=75；散布常见度=0.25；矿团规模=3~9；耐久=13000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Ilmenite（Ilmenite Ore）×按矿点规则 | 权重=0.7 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Iron Ore（`Iron`）

- 分类：`ResourcesRaw/Extracted/Metals/SLD`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to iron ore（`ConvertToSteel`） | Matter（inorganic matter） ×50 | Iron（Iron Ore）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 摧毁残留 | Big meteorite（`BigIronBoulder`） | — | Iron（Iron Ore）×100 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 摧毁残留 | Iron Meteorite（`IronBoulder`） | — | Iron（Iron Ore）×60 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 摧毁残留 | Iron Meteorite（`MineralBoulder`） | — | Iron（Iron Ore）×35 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 深钻井 | Iron Ore（`Iron`） | — | Iron（Iron Ore）×35 | 每份产量=35；每批规模=80~175；常见度=3.2 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Big meteorite（`BigIronBoulder`） | — | Iron（Iron Ore）×240 | 单格产量=240；耐久=6000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 矿脉开采 | Iron Meteorite（`IronBoulder`） | — | Iron（Iron Ore）×120 | 单格产量=120；耐久=2000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 矿脉开采 | Iron Ore Deposits（`MineableSteel`） | — | Iron（Iron Ore）×35 | 单格产量=35；散布常见度=1.1；矿团规模=20~40；耐久=2800 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml` |
| 矿脉开采 | Iron Meteorite（`MineralBoulder`） | — | Iron（Iron Ore）×75 | 单格产量=75；耐久=2000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### Nickel Ore（`Nickel`）

- 分类：`ResourcesRaw/Extracted/Metals/RAR`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to nickel ore（`ConvertToNickel`） | Matter（inorganic matter） ×150 | Nickel（Nickel Ore）×20 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 深钻井 | Nickel Ore（`Nickel`） | — | Nickel（Nickel Ore）×20 | 每份产量=20；每批规模=32~68；常见度=1.2 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Nickel Ore Deposits（`MineableNickel`） | — | Nickel（Nickel Ore）×50 | 单格产量=50；散布常见度=0.4；矿团规模=5~10；耐久=10000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Nickel（Nickel Ore）×按矿点规则 | 权重=1 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Nitre（`Nitre`）

- 分类：`ResourcesRaw/Extracted`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | collect nitre（`CollectNitreGeyser`） | Chemfuel（Chemfuel） ×10 | Nitre（Nitre）×20；Sulfur（Sulfur）×10 | 处理天数=5 | 建筑: SulfurExtractor | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SulfurExtractor.xml` |
| 发酵/缓处理配方 | Extracts nitre from body（`Make_Nitre_Body`） | [Corpses] ×1；Ash（ash） ×1 | Nitre（Nitre）×30 | 处理天数=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=5~35；理想温度=10~30 | 建筑: Saltpeter_pit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SaltpeterPit.xml` |
| 发酵/缓处理配方 | Extracts nitre from fecal（`Make_Nitre_Fecal`） | FecalSludge（FecalSludge） ×350；Ash（ash） ×1 | Nitre（Nitre）×15 | 处理天数=10；每度每小时损坏=0；低于安全温度速度=0.01；高于安全温度速度=0.8；安全温度=5~35；理想温度=10~30 | 建筑: Saltpeter_pit | — | — | `Mods/Core_SK/Patches/Mods_Hygiene/NitreToFecalRecipe.xml` |
| 发酵/缓处理配方 | Extracts nitre from mush（`Make_Nitre_Mush`） | RottedMush（rotted mush） ×70；Ash（ash） ×1 | Nitre（Nitre）×7 | 处理天数=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=5~35；理想温度=10~30 | 建筑: Saltpeter_pit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SaltpeterPit.xml` |
| 深钻井 | Nitre（`Nitre`） | — | Nitre（Nitre）×30 | 每份产量=30；每批规模=42~85；常见度=1.6 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| 矿脉开采 | Nitre Deposits（`MineableNitre`） | — | Nitre（Nitre）×75 | 单格产量=75；散布常见度=1.05；矿团规模=8~16；耐久=1800 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |

#### Paraffin（`Paraffins`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to paraffins（`ConvertToParaffins`） | Matter（inorganic matter） ×50 | Paraffins（Paraffin）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |

#### Peat（`Peat`）

- 分类：`ResourcesRaw/Extracted`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 深钻井 | Peat（`Peat`） | — | Peat（Peat）×50 | 每份产量=50；每批规模=45~95；常见度=0.8 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |

#### Pile of dirt（`Dirt`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | burn wood into ash (full load)（`MakeAsh_Pit_Full`） | [Wooden] ×100；Dirt（Pile of dirt） ×10 | Ash（ash）×30；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | burn wood into ash (half load)（`MakeAsh_Pit_Half`） | [Wooden] ×50；Dirt（Pile of dirt） ×5 | Ash（ash）×15；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | burn wood into charcoal (full load)（`MakeCharcoal_Pit_Full`） | [Wooden] ×100；Dirt（Pile of dirt） ×10 | Charcoal（Charcoal）×80；Dirt（Pile of dirt）×6 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | burn wood into charcoal (half load)（`MakeCharcoal_Pit_Half`） | [Wooden] ×50；Dirt（Pile of dirt） ×5 | Charcoal（Charcoal）×40；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | bake clay bricks (full load)（`MakeClayBricks_Pit_Full`） | SoftClay（Soft Clay） ×100；Dirt（Pile of dirt） ×5 | ClayBrick（clay bricks）×100；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | bake clay bricks (half load)（`MakeClayBricks_Pit_Half`） | SoftClay（Soft Clay） ×50；Dirt（Pile of dirt） ×5 | ClayBrick（clay bricks）×50；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 工作台配方 | Make dirt（`make_dirt`） | SandResource（Sand） ×2；Fertilizer（fertilizer） ×1 | Dirt（Pile of dirt）×3 | 工作量=300；速度属性=SmeltingSpeed | 研究: Craft_0；建筑: ButcherSpot | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | Make dirt（`make_dirt_Electric`） | SandResource（Sand） ×20；Fertilizer（fertilizer） ×10 | Dirt（Pile of dirt）×30 | 工作量=1500；速度属性=SmeltingSpeed | 研究: Concrete_C2；建筑: EConcreteMixer | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | Make dirt（`make_dirt_hand`） | SandResource（Sand） ×20；Fertilizer（fertilizer） ×10 | Dirt（Pile of dirt）×30 | 工作量=3000；速度属性=SmeltingSpeed | 研究: Concrete_C1；建筑: ConcreteMixer | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Plastic（`Plastic`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create plastic（`MakePlastic`） | Polymers（Polymers） ×30 | Plastic（Plastic）×10 | 工作量=800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |
| 工作台配方 | Create plastic with sulphate filler（`MakePlasticSulphate`） | Polymers（Polymers） ×20；Sulphates（Sulphates） ×5 | Plastic（Plastic）×20 | 工作量=2400；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |
| 工作台配方 | Create plastic with sulphate filler bulk（`MakePlasticSulphateBulk`） | Polymers（Polymers） ×60；Sulphates（Sulphates） ×15 | Plastic（Plastic）×60 | 工作量=2400；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### Polymers（`Polymers`）

- 分类：`ResourcesRaw/Extracted/Hydrocarbons`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to polymers（`ConvertToPolymers`） | Matter（inorganic matter） ×50 | Polymers（Polymers）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |

#### redwood（`RedWoodLog`）

- 分类：`ResourcesRaw/Wooden`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | redwood（`PlantTreeRedwood`） | — | RedWoodLog（redwood）×225 | 收获量=225；生长天数=85；播种工作量=1800；收获工作量=1700；最低肥力=0.8；肥力敏感度=0.8 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Trees.xml` |

#### Rubber（`Rubber`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create rubber（`MakeRubber`） | Polymers（Polymers） ×30；Sulphates（Sulphates） ×3 | Rubber（Rubber）×10 | 工作量=800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |
| 工作台配方 | Vulcanize rubber（`VulcanizeRubber`） | RawCaoutchouc（Caoutchouc） ×30；Ash（ash） 或 Sulfur（Sulfur） 或 Sulphates（Sulphates） ×3 | Rubber（Rubber）×10 | 工作量=800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: BiofuelRefining；技能: Crafting≥10；建筑: Kiln, TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### rubble（`CrushedStone`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | crush blocks into rubble（`Crush_Blocks_Rubble`） | [StoneBlocks] ×20 | CrushedStone（rubble）×20 | 处理天数=0.3 | 建筑: RockCrusher | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_RockCrusher.xml` |
| 发酵/缓处理配方 | crush chunks into rubble（`Crush_Chunk_Rubble`） | [StoneChunks] ×1 | CrushedStone（rubble）×35 | 处理天数=0.6 | 建筑: RockCrusher | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_RockCrusher.xml` |
| 工作台配方 | crush blocks into rubble（`MakeCrushedStone_Electric`） | [StoneBlocks] ×40 | CrushedStone（rubble）×40 | 工作量=1200；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 建筑: TableMachining | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | crush chunks into rubble（`MakeCrushedStone_Electric_Chunk`） | [StoneChunks] ×1 | CrushedStone（rubble）×32 | 工作量=1200；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 建筑: TableMachining | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | crush blocks into rubble（`MakeCrushedStone_Hand`） | [StoneBlocks] ×20 | CrushedStone（rubble）×20 | 工作量=1200；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 建筑: TableStonecutter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | crush chunks into rubble（`MakeCrushedStone_Hand_Chunk`） | [StoneChunks] ×1 | CrushedStone（rubble）×32 | 工作量=2400；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 建筑: TableStonecutter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 深钻井 | rubble（`CrushedStone`） | — | CrushedStone（rubble）×30 | 每份产量=30 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Terraforming.xml` |

#### salt（`Salt`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | mine salt（`VG_Minesalt`） | — | Salt（salt）×5 | 工作量=1800；速度属性=MiningSpeed；工作技能=Mining；技能经验系数=0.5 | 建筑: VG_SaltMine | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_saltmine.xml` |
| 深钻井 | salt（`Salt`） | — | Salt（salt）×40 | 每份产量=40；每批规模=60~120；常见度=1.8 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| 矿脉开采 | Compacted salt（`MineableSalt`） | — | Salt（salt）×150 | 单格产量=150；散布常见度=1.2；矿团规模=5~10；耐久=900 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |

#### Sand（`SandResource`）

- 分类：`ResourcesRaw/NaturalResources`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | pulverize chunks into sand（`CrushChunkSand`） | [StoneChunks] ×1 | SandResource（Sand）×80 | 处理天数=0.8 | 建筑: RockCrusher | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_RockCrusher.xml` |
| 发酵/缓处理配方 | crush rubble into sand（`CrushRubbleSand`） | CrushedStone（rubble） ×10 | SandResource（Sand）×25 | 处理天数=0.5 | 建筑: RockCrusher | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_RockCrusher.xml` |
| 工作台配方 | crush rubble into sand（`MakeSand_Electric`） | CrushedStone（rubble） ×20 | SandResource（Sand）×50 | 工作量=600；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 建筑: TableMachining | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | pulverize chunks into sand（`MakeSand_Electric_Chunk`） | [StoneChunks] ×1 | SandResource（Sand）×80 | 工作量=1500；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 建筑: TableMachining | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | crush rubble into sand（`MakeSand_Hand`） | CrushedStone（rubble） ×10 | SandResource（Sand）×25 | 工作量=600；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Stonecutting；建筑: TableStonecutter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | pulverize chunks into sand（`MakeSand_Hand_Chunk`） | [StoneChunks] ×1 | SandResource（Sand）×80 | 工作量=3000；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Stonecutting；建筑: TableStonecutter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 深钻井 | Sand（`SandResource`） | — | SandResource（Sand）×50 | 每份产量=50 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Terraforming.xml` |

#### silicon（`Silicon`）

- 分类：`ResourcesRaw/Extracted`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | extract silicon（`MakeSilicon_Electric`） | SandResource（Sand） ×50 | Silicon（silicon）×5 | 工作量=800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥5；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | extract silicon (x4)（`MakeSilicon_Set`） | SandResource（Sand） ×200 | Silicon（silicon）×20 | 工作量=1800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥5；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Silver Ore（`Silver`）

- 分类：`ResourcesRaw/Extracted/Metals/PRS`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to silver ore（`ConvertToSilver`） | Matter（inorganic matter） ×100 | Silver（Silver Ore）×300 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 摧毁残留 | Silver Meteorite（`SilverBoulder`） | — | Silver（Silver Ore）×50 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 深钻井 | Silver Ore（`Silver`） | — | Silver（Silver Ore）×20 | 每份产量=20；每批规模=20~40；常见度=1 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Silver Ore Deposits（`MineableSilver`） | — | Silver（Silver Ore）×250 | 单格产量=250；散布常见度=0.25；矿团规模=7~15；耐久=3500 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml` |
| 矿脉开采 | Silver Meteorite（`SilverBoulder`） | — | Silver（Silver Ore）×100 | 单格产量=100；耐久=1000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Silver（Silver Ore）×按矿点规则 | 权重=0.95 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Sulfur（`Sulfur`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | collect nitre（`CollectNitreGeyser`） | Chemfuel（Chemfuel） ×10 | Nitre（Nitre）×20；Sulfur（Sulfur）×10 | 处理天数=5 | 建筑: SulfurExtractor | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SulfurExtractor.xml` |
| 发酵/缓处理配方 | collect sulfur（`CollectSulfurGeyser`） | Chemfuel（Chemfuel） ×10 | Sulfur（Sulfur）×50 | 处理天数=3 | 建筑: SulfurExtractor | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SulfurExtractor.xml` |
| 工作台配方 | Obtain sulfur（`MakeSulfur`） | Iron（Iron Ore） ×35 | Sulfur（Sulfur）×10 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 建筑: AmmoBench, TableHeavyAmmunition | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Ammo.xml` |

#### Sulphates（`Sulphates`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to sulphates（`ConvertToSulphates`） | Matter（inorganic matter） ×50 | Sulphates（Sulphates）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |

#### Surfactant Fuel（`Surfactant`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create Surfactants（`MakeSurfactant`） | Salt（salt） ×10；Sulfur（Sulfur） ×15；[Alcohol_Bottles] ×15 | Surfactant（Surfactant Fuel）×50 | 工作量=4000；速度属性=DrugSynthesisSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Firefoam；技能: Crafting≥8；建筑: Brewery, DrugLab, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |
| 工作台配方 | Create Surfactants from coconut oil（`MakeSurfactant_CoconutSubstitute`） | Salt（salt） ×10；Sulfur（Sulfur） ×15；RawCoconut（Coconut） ×25 | Surfactant（Surfactant Fuel）×50 | 工作量=4000；速度属性=DrugSynthesisSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Firefoam；技能: Crafting≥12, Intellectual≥4；建筑: Brewery, DrugLab, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |
| 工作台配方 | Create Polymer Surfactants（`MakeSurfactant_Polymers`） | Compaste（Compound paste） ×15；Polymers（Polymers） ×25 | Surfactant（Surfactant Fuel）×50 | 工作量=4000；速度属性=DrugSynthesisSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Oil_Industry_C5；技能: Crafting≥12, Intellectual≥4；建筑: DrugLab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### Synthetic Fibers（`SyntheticFibers`）

- 分类：`ResourcesRaw/Extracted/Chemical`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create synthetic fibers（`MakeSyntheticFibers`） | Polymers（Polymers） ×25；Sulphates（Sulphates） ×3 | SyntheticFibers（Synthetic Fibers）×5 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Oil_Industry_C6；技能: Crafting≥14；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### Tin Ore（`Tin`）

- 分类：`ResourcesRaw/Extracted/Metals/HCM`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to tin ore（`ConvertToTin`） | Matter（inorganic matter） ×50 | Tin（Tin Ore）×100 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 深钻井 | Tin Ore（`Tin`） | — | Tin（Tin Ore）×40 | 每份产量=40；每批规模=58~115；常见度=1.6 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Tin Ore Deposits（`MineableTin`） | — | Tin（Tin Ore）×75 | 单格产量=75；散布常见度=1.1；矿团规模=9~17；耐久=1300 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Tin（Tin Ore）×按矿点规则 | 权重=2.5 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Uranium Ore（`Uranium`）

- 分类：`ResourcesRaw/Extracted/Metals/RAD`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Convert to uranium ore（`ConvertToUranium`） | Matter（inorganic matter） ×200 | Uranium（Uranium Ore）×50 | 工作量=12000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_E1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |
| 摧毁残留 | ancient reactor hive（`NuclearHive`） | — | Uranium（Uranium Ore）×95 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Technical.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | Uranium（Uranium Ore）×50 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | Uranium（Uranium Ore）×50 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | Uranium Meteorite（`UraniumBoulder`） | — | Uranium（Uranium Ore）×35 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |
| 深钻井 | Uranium Ore（`Uranium`） | — | Uranium（Uranium Ore）×10 | 每份产量=10；每批规模=15~35；常见度=0.6 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml` |
| 矿脉开采 | Uranium Ore Deposit（`MineableUranium`） | — | Uranium（Uranium Ore）×40 | 单格产量=40；散布常见度=0.18；矿团规模=4~9；耐久=4000 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml` |
| 矿脉开采 | Uranium Meteorite（`UraniumBoulder`） | — | Uranium（Uranium Ore）×75 | 单格产量=75；耐久=4000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### Wolframite Ore（`Wolframite`）

- 分类：`ResourcesRaw/Extracted/Metals/RAR`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 深钻井 | Wolframite Ore（`Wolframite`） | — | Wolframite（Wolframite Ore）×10 | 每份产量=10；每批规模=5~25；常见度=0.6 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Ore.xml` |
| 矿脉开采 | Wolframite Ore Deposits（`MineableTungsten`） | — | Wolframite（Wolframite Ore）×50 | 单格产量=50；散布常见度=0.15；矿团规模=3~9；耐久=20000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Rocks.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Wolframite（Wolframite Ore）×按矿点规则 | 权重=0.65 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### wood log（`WoodLog`）

- 分类：`ResourcesRaw/Wooden`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_Timbershroom（`Plant_Timbershroom`） | — | WoodLog（wood log）×60 | 收获量=60；播种工作量=750；收获工作量=300 | — | 播种标签: Fungiponics | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml` |

### 2.3 加工品

#### 120mm Tank cannon crate（`CannonTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 120mm Tank cannon crate（`BuildCannonTurret_Crate`） | Weapon_Parts（weapon parts） ×14；Cannon_Component（cannon weapon component） ×2；[Metallic] ×55；Rubber（Rubber） ×25 | CannonTurret_Crate（120mm Tank cannon crate）×1 | 工作量=17500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Artillery_C2；技能: Crafting≥14；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 130mm Heavy Missile System crate（`RocketLauncher_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 130mm Heavy Missile System crate（`BuildRocketLauncher_Crate`） | Weapon_Parts（weapon parts） ×10；Launcher_Component（launcher weapon component） ×3；[Metallic] ×35 | RocketLauncher_Crate（130mm Heavy Missile System crate）×1 | 工作量=19500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rockets_D2；技能: Crafting≥15；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 130mm Medium Missile System crate（`MediumMissileSystem_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 130mm Medium Missile System crate（`BuildMediumMissileSystem_Crate`） | Weapon_Parts（weapon parts） ×10；Launcher_Component（launcher weapon component） ×2；[Metallic] ×35 | MediumMissileSystem_Crate（130mm Medium Missile System crate）×1 | 工作量=18500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rockets_D1；技能: Crafting≥14；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 14.5mm KPV MG crate（`KPV_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 14.5mm KPV MG crate（`BuildKPV_Crate`） | Weapon_Parts（weapon parts） ×17；Heavy_Component（heavy weapon component） ×1；[Metallic] ×50 | KPV_Crate（14.5mm KPV MG crate）×1 | 工作量=11500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Heavy_turrets_C2；技能: Crafting≥7；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 14.5x114mm Sentry heavy crate（`HeavyTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 145x114mm Sentry heavy crate（`BuildHeavyTurret_Crate`） | Weapon_Parts（weapon parts） ×14；Heavy_Component（heavy weapon component） ×1；[Metallic] ×35；Electronics（Integrated Circuits） ×5 | HeavyTurret_Crate（14.5x114mm Sentry heavy crate）×1 | 工作量=13500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Autoturrets_D2；技能: Crafting≥12；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 155mm Howitzer Cannon Crate（`HowitzerTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 155mm Howitzer Cannon Crate（`BuildHowitzerTurret_Crate`） | Weapon_Parts（weapon parts） ×18；Cannon_Component（cannon weapon component） ×2；[Metallic] ×70；Rubber（Rubber） ×35 | HowitzerTurret_Crate（155mm Howitzer Cannon Crate）×1 | 工作量=20500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Artillery_C2；技能: Crafting≥15；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 20mm Vulcan Cannon Crate（`VulcanCannon_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 20mm Vulcan Cannon crate（`BuildVulcanCannon_Crate`） | Weapon_Parts（weapon parts） ×20；Cannon_Component（cannon weapon component） ×1；[Metallic] ×60；Rubber（Rubber） ×25 | VulcanCannon_Crate（20mm Vulcan Cannon Crate）×1 | 工作量=17500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Heavy_turrets_C3；技能: Crafting≥16；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 20x102mm Oerlikon autocannon crate（`Oerlikonautocannon_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 20x102mm Oerlikon autocannon crate（`BuildOerlikonautocannon_Crate`） | Weapon_Parts（weapon parts） ×15；Cannon_Component（cannon weapon component） ×1；[Metallic] ×45；Rubber（Rubber） ×15 | Oerlikonautocannon_Crate（20x102mm Oerlikon autocannon crate）×1 | 工作量=13500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Heavy_turrets_C3；技能: Crafting≥12；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 30mm AGS-17 crate（`AGSturret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 30mm AGS-17 crate（`BuildAGSturret_Crate`） | Weapon_Parts（weapon parts） ×10；Cannon_Component（cannon weapon component） ×1；[Metallic] ×20；Rubber（Rubber） ×10 | AGSturret_Crate（30mm AGS-17 crate）×1 | 工作量=11500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Grenade_launchers_C2；技能: Crafting≥8；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 30mm AGS-30 crate（`AGCturret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 30mm AGS-30 crate（`BuildAGCturret_Crate`） | Weapon_Parts（weapon parts） ×12；Cannon_Component（cannon weapon component） ×1；[Metallic] ×20；Rubber（Rubber） ×10 | AGCturret_Crate（30mm AGS-30 crate）×1 | 工作量=15500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Grenade_launchers_C2；技能: Crafting≥8；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 30x173mm GAU-8 Avenger crate（`Avenger_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 30x165mm GAU-8 Avenger crate（`BuildAvenger_Crate`） | Weapon_Parts（weapon parts） ×22；Cannon_Component（cannon weapon component） ×1；[Metallic] ×60；Rubber（Rubber） ×35 | Avenger_Crate（30x173mm GAU-8 Avenger crate）×1 | 工作量=18500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Heavy_turrets_C3；技能: Crafting≥16；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 35mm Sentry naval crate（`NavalGunTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 30x165mm Sentry naval crate（`BuildNavalGunTurret_Crate`） | Weapon_Parts（weapon parts） ×18；Cannon_Component（cannon weapon component） ×1；[Metallic] ×35；Electronics（Integrated Circuits） ×5 | NavalGunTurret_Crate（35mm Sentry naval crate）×1 | 工作量=14500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Autoturrets_D3；技能: Crafting≥13；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 5.56x45mm NATO Sentry light crate（`SentryTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 5.56x45mm NATO Sentry light crate（`BuildSentryTurret_Crate`） | Weapon_Parts（weapon parts） ×7；Rifle_Component（rifle component） ×2；[Metallic] ×20；Electronics（Integrated Circuits） ×5 | SentryTurret_Crate（5.56x45mm NATO Sentry light crate）×1 | 工作量=10500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: GunTurrets；技能: Crafting≥11；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 50BMG M2 Browning MG crate（`MGTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 50BMG M2 Browning MG crate（`BuildMGTurret_Crate`） | Weapon_Parts（weapon parts） ×14；Heavy_Component（heavy weapon component） ×1；[Metallic] ×40 | MGTurret_Crate（50BMG M2 Browning MG crate）×1 | 工作量=11500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: SniperTurret；技能: Crafting≥7；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 7.62x51mm M134 Minigun crate（`MinigunTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 7.62x51mm M134 Minigun crate（`BuildMinigunTurret_Crate`） | Weapon_Parts（weapon parts） ×15；Heavy_Component（heavy weapon component） ×1；[Metallic] ×45 | MinigunTurret_Crate（7.62x51mm M134 Minigun crate）×1 | 工作量=10500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Heavy_turrets_C2；技能: Crafting≥8；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 7.62x51mm M240B MG crate（`M240B_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 7.62x51mm M240B MG crate（`BuildM240B_Crate`） | Weapon_Parts（weapon parts） ×10；Heavy_Component（heavy weapon component） ×1；[Metallic] ×35 | M240B_Crate（7.62x51mm M240B MG crate）×1 | 工作量=10500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: SniperTurret；技能: Crafting≥7；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 81mm Mortar Launcher Crate（`Mortar_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 81mm Mortar Launcher crate（`BuildMortar_Crate`） | Weapon_Parts（weapon parts） ×8；Cannon_Component（cannon weapon component） ×1；[Metallic] ×20 | Mortar_Crate（81mm Mortar Launcher Crate）×1 | 工作量=12500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Mortars；技能: Crafting≥9；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### 90mm Flak turret crate（`FlakTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 90mm Flak turret crate（`BuildFlakTurret_Crate`） | Weapon_Parts（weapon parts） ×12；Cannon_Component（cannon weapon component） ×2；[Metallic] ×45；Rubber（Rubber） ×20 | FlakTurret_Crate（90mm Flak turret crate）×1 | 工作量=14500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Mortars；技能: Crafting≥12；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### advanced component（`ComponentAdvanced`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Deconstruct personal shield（`DeconstructPersonalShield`） | Apparel_ShieldBelt（Apparel_ShieldBelt） 或 Apparel_ShieldGuardian（guardian shield） ×1 | ComponentAdvanced（advanced component）×8；Electronics（Integrated Circuits）×1 | 工作量=1000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableRecycling | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_DeconstructPersonalShield.xml` |
| 工作台配方 | Assemble advanced components（`Make_ComponentAdvanced`） | [USLDBar] ×30 | ComponentAdvanced（advanced component）×10 | 工作量=2500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D1；技能: Crafting≥6；建筑: AdvToolBench, FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |

#### advanced mechanism（`AdvMechanism`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble advanced mechanisms（`MakeAdvMechanism_Electric`） | [USLDBar] ×60；ComponentAdvanced（advanced component） ×20；SyntheticFibers（Synthetic Fibers） ×10 | AdvMechanism（advanced mechanism）×10 | 工作量=3200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D1；技能: Crafting≥16；建筑: AdvToolBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 工作台配方 | Assemble advanced mechanisms（`MakeAdvMechanism_Hand`） | [USLDBar] ×30；ComponentAdvanced（advanced component） ×10；SyntheticFibers（Synthetic Fibers） ×5 | AdvMechanism（advanced mechanism）×5 | 工作量=3200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D1；技能: Crafting≥13；建筑: FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |

#### advanced rifle component（`AdvRifle_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Advanced Rifle Component（`MakeAdvRifle_Component`） | SyntheticFibers（Synthetic Fibers） ×5；Plastic（Plastic） ×15；Rubber（Rubber） ×15；Rifle_Component（rifle component） ×1；AdvMechanism（advanced mechanism） ×2 | AdvRifle_Component（advanced rifle component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rifles_C2；技能: Crafting≥13；建筑: AdvWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### advanced sniper component（`AdvSniper_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Advanced Sniper Component（`MakeAdvSniper_Component`） | SyntheticFibers（Synthetic Fibers） ×10；Plastic（Plastic） ×20；Rubber（Rubber） ×15；Electronics（Integrated Circuits） ×4；Sniper_Component（sniper component） ×2；AdvMechanism（advanced mechanism） ×3 | AdvSniper_Component（advanced sniper component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Sniper_rifles_D1；技能: Crafting≥15；建筑: AdvWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### alnico alloy（`AlnicoAlloy`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt alnico alloy（`MakeAlnicoAlloy`） | Steel（steel alloy） ×8；AluminiumBar（aluminium alloy） ×3；Cobalt（cobalt alloy） ×3；NickelBar（nickel alloy） ×6 | AlnicoAlloy（alnico alloy）×20 | 工作量=1650；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥13；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Alpaca wool（`WoolAlpaca`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | Alpaca（`Alpaca`） | — | WoolAlpaca（Alpaca wool）×50 | 间隔天数=10 | — | 组件=CompProperties_Shearable | PatchApplied=Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml` |

#### alpha poly（`AlphaPoly`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Disassemble AI persona core（`DisassembleAIPersonaCore`） | AIPersonaCore（AIPersonaCore） ×1 | BioMicrochips（Bioelectronic Devices）×1；AlphaPoly（alpha poly）×30；CarbonAlloy（carbon fiber alloy）×40 | 工作量=4000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D2；技能: Crafting≥16；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |
| 工作台配方 | Fabricate alpha poly（`ProduceAPoly`） | DepletedUranium（depleted uranium alloy） ×5；CarbonAlloy（carbon fiber alloy） ×5；Antimatter（AntiMatter） ×3 | AlphaPoly（alpha poly）×15 | 工作量=8000；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥19；建筑: Matterfab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_HitechCraft.xml` |

#### aluminium alloy（`AluminiumBar`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt aluminium alloy（`MakeAluminiumBars_Electric`） | Aluminium（Bauxite Ore） ×20 | AluminiumBar（aluminium alloy）×20 | 工作量=1200；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C2；技能: Crafting≥7；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### AntiMatter（`Antimatter`）

- 分类：`Manufactured/SuperMaterialCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 物质生成 | Antimatter（`AntimatterResourceInfo`） | — | Antimatter（AntiMatter）×2 | 最小数量=2；最大数量=2；生产 tick=10000 | — | — | — | `Mods/Core_SK/Defs/MatterResourceDefs/MatterResourceInfo.xml` |

#### AT missile BGM-71 TOW crate（`TOW_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build AT missile BGM-71 TOW crate（`BuildTOW_Crate`） | Weapon_Parts（weapon parts） ×15；Launcher_Component（launcher weapon component） ×1；[Metallic] ×30；Rubber（Rubber） ×15 | TOW_Crate（AT missile BGM-71 TOW crate）×1 | 工作量=16500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rockets_D1；技能: Crafting≥13；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### AT missile FGM-148 Javelin Crate（`LightMissileSystem_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build AT missile FGM-148 Javelin crate（`BuildLightMissileSystem_Crate`） | Weapon_Parts（weapon parts） ×16；Launcher_Component（launcher weapon component） ×1；[Metallic] ×35；Rubber（Rubber） ×15 | LightMissileSystem_Crate（AT missile FGM-148 Javelin Crate）×1 | 工作量=16500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rockets_C2；技能: Crafting≥13；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### AT missile Sentry rocket launcher crate（`SentryRocketLauncher_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build AT missile Sentry rocket launcher crate（`BuildSentryRocketLauncher_Crate`） | Weapon_Parts（weapon parts） ×15；Launcher_Component（launcher weapon component） ×1；[Metallic] ×35；Electronics（Integrated Circuits） ×5 | SentryRocketLauncher_Crate（AT missile Sentry rocket launcher crate）×1 | 工作量=16500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Autoturrets_D3；技能: Crafting≥15；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### Balista crate（`Balista_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build balista crate（`BuildBalista_Crate`） | [Wooden] ×90；[Metallic] ×30 | Balista_Crate（Balista crate）×1 | 工作量=4500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Ballista_B1；技能: Crafting≥3；建筑: CraftingSpot, HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### bamboo plank（`BambooPlank`）

- 分类：`Manufactured/Woody`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | saw bamboo planks（`MakeBambooPlanks_Electric`） | Bamboo（bamboo log） ×20 | BambooPlank（bamboo plank）×30 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥9；建筑: TableSawmillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | saw bamboo planks（`MakeBambooPlanks_Hand`） | Bamboo（bamboo log） ×10 | BambooPlank（bamboo plank）×15 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；技能: Crafting≥6；建筑: TableSawmillHand | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### beta poly（`BetaPoly`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Fabricate beta poly（`ProduceBPoly`） | AlphaPoly（alpha poly） ×3；BiosyntheticMaterial（BioSynthetic Material） ×5 | BetaPoly（beta poly）×5 | 工作量=12000；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥20；建筑: Matterfab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_HitechCraft.xml` |

#### Bioelectronic Devices（`BioMicrochips`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Disassemble AI persona core（`DisassembleAIPersonaCore`） | AIPersonaCore（AIPersonaCore） ×1 | BioMicrochips（Bioelectronic Devices）×1；AlphaPoly（alpha poly）×30；CarbonAlloy（carbon fiber alloy）×40 | 工作量=4000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D2；技能: Crafting≥16；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |
| 工作台配方 | Assemble bioelectronic device（`MakeBioelectronicDevices`） | Biomatter（biomatter） ×60；Hyperweave（Hyperweave） ×15；Microchips（Microchip Integrated Circuits） ×2 | BioMicrochips（Bioelectronic Devices）×1 | 工作量=7000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D3；技能: Crafting≥18；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |

#### bioferrite alloy（`BioferriteAlloy`）

- 分类：`Manufactured/Metallic/SLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt bioferrite alloy（`MakeBioferriteAlloy`） | Bioferrite（Bioferrite） ×10；Iron（Iron Ore） ×10；[Coal] ×5 | BioferriteAlloy（bioferrite alloy）×10 | 工作量=2100；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: BioferriteShaping；技能: Crafting≥9；建筑: BioferriteShaper | — | — | `Mods/Core_SK_DLCModule/Anomaly/Defs/RecipeDefs/Recipes_Alloys_Anomaly.xml` |

#### biomatter（`Biomatter`）

- 分类：`Manufactured/SuperMaterialCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | extract biomatter from corpse（`ExtractBiomatterCorpse`） | [Corpses] ×1 | Biomatter（biomatter）×20 | 处理天数=1.5 | 建筑: TableOrganvat | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_TableOrganvat.xml` |

#### BioSynthetic Material（`BiosyntheticMaterial`）

- 分类：`Manufactured/SuperMaterialCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Synthesize BioSynthetic material（`BuildBiosyntheticMaterial`） | Biomatter（biomatter） ×15；SyntheticFibers（Synthetic Fibers） ×9；Plastic（Plastic） ×30 | BiosyntheticMaterial（BioSynthetic Material）×5 | 工作量=2400；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥18；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_HitechCraft.xml` |

#### bronze alloy（`Bronze`）

- 分类：`Manufactured/Metallic/SLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt bronze alloy（`MakeBronze_Electric`） | Copper（Copper Ore） 或 CopperBar（copper alloy） ×20；TinBar（tin alloy） 或 AluminiumBar（aluminium alloy） ×10 | Bronze（bronze alloy）×30 | 工作量=700；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥5；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt bronze alloy（`MakeBronze_Hand`） | Copper（Copper Ore） 或 CopperBar（copper alloy） ×20；TinBar（tin alloy） 或 AluminiumBar（aluminium alloy） ×10 | Bronze（bronze alloy）×30 | 工作量=1000；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥2；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### cannon weapon component（`Cannon_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Cannon component（`MakeCannon_Component`） | [SLDBar] 或 [USLDBar] ×20；Heavy_Component（heavy weapon component） ×1；AdvMechanism（advanced mechanism） ×5 | Cannon_Component（cannon weapon component）×1 | 工作量=3100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Turrets_31；技能: Crafting≥14；建筑: AdvWeaponCraftingWorkTable, HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### carbon fiber alloy（`CarbonAlloy`）

- 分类：`Manufactured/ConstructionParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Disassemble AI persona core（`DisassembleAIPersonaCore`） | AIPersonaCore（AIPersonaCore） ×1 | BioMicrochips（Bioelectronic Devices）×1；AlphaPoly（alpha poly）×30；CarbonAlloy（carbon fiber alloy）×40 | 工作量=4000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D2；技能: Crafting≥16；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |
| 工作台配方 | Smelt carbon fiber alloy（`MakeCarbonAlloy`） | SyntheticFibers（Synthetic Fibers） ×10；Carbon（Carbon） ×10 | CarbonAlloy（carbon fiber alloy）×10 | 工作量=2800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Oil_Industry_C6；技能: Crafting≥15；建筑: TableChemlab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Catapult crate（`Catapult_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build catapult crate（`BuildCatapult_Crate`） | [Wooden] ×160；[Metallic] ×60 | Catapult_Crate（Catapult crate）×1 | 工作量=10500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Ballista_B1；技能: Crafting≥5；建筑: CraftingSpot, HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### Ceramics（`Ceramics`）

- 分类：`Manufactured/Ceramic`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake ceramics（`MakeCeramics`） | SoftClay（Soft Clay） ×20；GlassBatch（Glass Batch） ×5 | Ceramics（Ceramics）×10 | 工作量=1200；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥8；建筑: ElectricSmelter, Kiln | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### charged weapon component（`Charged_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Charged component（`MakeCharged_Component`） | [USLDBar] ×10；[HCMBar] ×20；MagneticMaterial（Magnetic Material） ×2；AdvMechanism（advanced mechanism） ×2 | Charged_Component（charged weapon component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: ChargedShot；技能: Crafting≥14；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### Chemfuel（`Chemfuel`）

- 分类：`Manufactured/FuelCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | create fuel（`MakeChemfuel`） | [Foods] ×40 | Chemfuel（Chemfuel）×15 | 工作量=2500；速度属性=DrugCookingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 技能: Crafting≥5 | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Chitin plating（`ChitinPlating`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create chitin plating（`MakeChitinPlating`） | Chitin（Chitin） ×50；SyntheticFibers（Synthetic Fibers） ×4；Compaste（Compound paste） ×1 | ChitinPlating（Chitin plating）×15 | 工作量=1080；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: AdvancedFabrication；技能: Crafting≥12；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### chromium alloy（`Chromium`）

- 分类：`Manufactured/Metallic/RARBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt chromium alloy（`ProcessForChromium`） | Iron（Iron Ore） ×25 | Chromium（chromium alloy）×10 | 工作量=1200；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥11；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### clay bricks（`ClayBrick`）

- 分类：`Manufactured/ConstructionParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | bake clay bricks (full load)（`MakeClayBricks_Pit_Full`） | SoftClay（Soft Clay） ×100；Dirt（Pile of dirt） ×5 | ClayBrick（clay bricks）×100；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=10；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 发酵/缓处理配方 | bake clay bricks (half load)（`MakeClayBricks_Pit_Half`） | SoftClay（Soft Clay） ×50；Dirt（Pile of dirt） ×5 | ClayBrick（clay bricks）×50；Dirt（Pile of dirt）×3 | 处理天数=1；槽位=5；每度每小时损坏=0；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-25~200；理想温度=0~100 | 建筑: CharcoalPit | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_CharcoalPit.xml` |
| 工作台配方 | bake clay bricks（`MakeClayBricks`） | SoftClay（Soft Clay） ×15 | ClayBrick（clay bricks）×15 | 工作量=800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥6；建筑: ElectricSmelter, Kiln | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Cloth（`Cloth`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin cloth（`MakeCloth`） | RawCotton（Raw Cotton） ×20 | Cloth（Cloth）×20 | 工作量=600；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Apparel_B1；技能: Crafting≥2；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### cobalt alloy（`Cobalt`）

- 分类：`Manufactured/Metallic/RARBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt cobalt alloy（`ProcessCobalt`） | Nickel（Nickel Ore） ×30 | Cobalt（cobalt alloy）×15 | 工作量=1300；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥14；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### ComponentIndustrial（`ComponentIndustrial`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble components（`Make_ComponentIndustrial`） | [SLDBar] 或 [USLDBar] ×30 | ComponentIndustrial（ComponentIndustrial）×10 | 工作量=2000；速度属性=SmithingSpeed；技能经验系数=0.8 | 研究: Craft_0；技能: Crafting≥3；建筑: AdvToolBench, FabricationBench | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml` |
| 摧毁残留 | Enemy ship（`EnemyShipPart`） | — | ComponentIndustrial（ComponentIndustrial）×20 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events.xml` |
| 摧毁残留 | ancient reactor hive（`NuclearHive`） | — | ComponentIndustrial（ComponentIndustrial）×35 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Technical.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | ComponentIndustrial（ComponentIndustrial）×4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | ComponentIndustrial（ComponentIndustrial）×4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | ShuttleCrashed（`ShuttleCrashed`） | — | ComponentIndustrial（ComponentIndustrial）×1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | ComponentIndustrial（ComponentIndustrial）×按矿点规则 | 权重=0.7 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### Compound paste（`Compaste`）

- 分类：`Textiles/CTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Craft compound paste（`MakeCompaste`） | Chemfuel（Chemfuel） ×10；Rubber（Rubber） ×5；Paraffins（Paraffin） ×2 | Compaste（Compound paste）×20 | 工作量=3200；速度属性=DrugCookingSpeed；工作技能=Crafting | 研究: AdvancedFabrication；技能: Crafting≥5；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Petrochemistry.xml` |

#### concrete（`ConcreteResource`）

- 分类：`Manufactured/ConstructionParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | mix concrete（`MixConcrete`） | SandResource（Sand） ×45；CrushedStone（rubble） ×30 | ConcreteResource（concrete）×15 | 工作量=1200；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Concrete_C1；技能: Crafting≥3；建筑: ConcreteMixer | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | mix concrete（`MixConcreteElectric`） | SandResource（Sand） ×45；CrushedStone（rubble） ×30 | ConcreteResource（concrete）×15 | 工作量=600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Concrete_C2；技能: Crafting≥6；建筑: EConcreteMixer | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### copper alloy（`CopperBar`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt copper alloy（`MakeCopperBars_Electric`） | Copper（Copper Ore） ×20 | CopperBar（copper alloy）×20 | 工作量=500；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥4；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt copper alloy（`MakeCopperBars_Hand`） | Copper（Copper Ore） ×10 | CopperBar（copper alloy）×10 | 工作量=700；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥1；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### depleted uranium alloy（`DepletedUranium`）

- 分类：`Manufactured/Metallic/RADBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Depleted uranium alloy（`MakeDepletedUraniumAlloy`） | Uranium（Uranium Ore） ×10 | DepletedUranium（depleted uranium alloy）×10 | 工作量=1700；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥11；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Devilstrand cloth（`DevilstrandCloth`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin devilstrand cloth（`MakeDevilstrand`） | RawDevilstrand（Raw Devilstrand） ×15；SyntheticFibers（Synthetic Fibers） ×2 | DevilstrandCloth（Devilstrand cloth）×15 | 工作量=600；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: AdvancedFabrication；技能: Crafting≥13；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### Dromedary hair（`WoolDromedary`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | Dromedary（`Dromedary`） | — | WoolDromedary（Dromedary hair）×80 | 间隔天数=18 | — | 组件=CompProperties_Shearable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |

#### Electronic Components（`ElectronicComponents`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble electronic components（`MakeElectronicComponents`） | [HCMBar] ×35；Glass（Glass Frame） ×10；ComponentIndustrial（ComponentIndustrial） ×10；SilverBar（sterling silver alloy） ×10 | ElectronicComponents（Electronic Components）×5 | 工作量=2500；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥8；建筑: TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |
| 摧毁残留 | Enemy ship（`EnemyShipPart`） | — | ElectronicComponents（Electronic Components）×8 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | ElectronicComponents（Electronic Components）×5 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | ElectronicComponents（Electronic Components）×5 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | ShuttleCrashed（`ShuttleCrashed`） | — | ElectronicComponents（Electronic Components）×3 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### ferrotitanium alloy（`Plasteel`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt ferrotitanium alloy（`MakeFerrotitaniumAlloy`） | Titanium（titanium alloy） ×5；Steel（steel alloy） ×12；CarbonAlloy（carbon fiber alloy） ×3 | Plasteel（ferrotitanium alloy）×15 | 工作量=2400；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥15；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### firewood（`Kindling`）

- 分类：`Manufactured/FuelCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | chop firewood（`MakeKindling_Electric`） | [Wooden] ×20 | Kindling（firewood）×40 | 工作量=500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.25 | 研究: Metals_C1；技能: Crafting≥4；建筑: TableSawmillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | chop firewood（`MakeKindling_Hand`） | [Wooden] ×10 | Kindling（firewood）×20 | 工作量=500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.25 | 研究: Craft_0；建筑: TableSawmillHand | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 植物收获 | Plant_Bush（`Plant_Bush`） | — | Kindling（firewood）×5 | 收获量=5；生长天数=5；收获工作量=130；最低肥力=0.2 | — | 播种标签: Ground | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Bushes.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Bushes.xml` |
| 植物收获 | rhododendron（`Plant_Rhododendron`） | — | Kindling（firewood）×6 | 收获量=6；生长天数=7；收获工作量=150；最低肥力=0.2 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Bushes.xml` |

#### FSX（`FSX`）

- 分类：`Manufactured/ExplosivesCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | Boomalope（`Boomalope`） | — | FSX（FSX）×6 | 间隔天数=4 | — | 组件=SK.CompProperties_ShearableRenameable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |
| 动物剪毛 | Boomrat（`Boomrat`） | — | FSX（FSX）×1 | 间隔天数=2 | — | 组件=SK.CompProperties_ShearableRenameable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |

#### Glass Batch（`GlassBatch`）

- 分类：`Manufactured/GlassCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | create glass batch（`MakeGlassBatch`） | SandResource（Sand） ×75 | GlassBatch（Glass Batch）×25 | 工作量=900；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Glass_B1；技能: Crafting≥3；建筑: GlassworkTable, TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### Glass Frame（`Glass`）

- 分类：`Manufactured/GlassCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | create glass frames（`MakeGlass`） | GlassBatch（Glass Batch） ×20 | Glass（Glass Frame）×10 | 工作量=600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Glass_B1；技能: Crafting≥5；建筑: GlassworkTable, TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### gold alloy（`GoldBar`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt gold alloy（`MakeGoldBars_Electric`） | Gold（Gold Ore） ×12；[HCM] ×3 | GoldBar（gold alloy）×15 | 工作量=500；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥6；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt gold alloy（`MakeGoldBars_Hand`） | Gold（Gold Ore） ×12；[HCM] ×3 | GoldBar（gold alloy）×15 | 工作量=750；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥3；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Gunpowder（`Powder`）

- 分类：`Manufactured/ExplosivesCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make gunpowder（`MakePowder`） | Nitre（Nitre） ×75；[Coal] ×20；Sulfur（Sulfur） ×20 | Powder（Gunpowder）×20 | 工作量=3000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 技能: Crafting≥3；建筑: AmmoBench, TableHeavyAmmunition | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Ammo.xml` |

#### heavy weapon component（`Heavy_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Heavy Component（`MakeHeavy_Component`） | [SLDBar] 或 [USLDBar] ×25；[HCMBar] ×15；ComponentIndustrial（ComponentIndustrial） ×12；Wire（wire） ×15；Mechanism（mechanism） ×6 | Heavy_Component（heavy weapon component）×1 | 工作量=2800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: HeavyTurrets；技能: Crafting≥12；建筑: AdvWeaponCraftingWorkTable, HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### Hemp（`HempCloth`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin hemp（`MakeHempCloth`） | SmokeleafLeaves（SmokeleafLeaves） ×30 | HempCloth（Hemp）×15 | 工作量=1200；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 研究: Apparel_B1；技能: Crafting≥6；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### HexCell（`Hexcell`）

- 分类：`Textiles/CTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Craft hex-cells（`MakeHexcell`） | MagneticMaterial（Magnetic Material） ×2；[HCMBar] ×4；Microchips（Microchip Integrated Circuits） ×1 | Hexcell（HexCell）×10 | 工作量=6000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting | 研究: Droids_D1；技能: Crafting≥14；建筑: TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |

#### Hyperweave（`Hyperweave`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin hyperweave（`MakeHyperweaveCloth`） | Synthread（Synthread） ×10；DevilstrandCloth（Devilstrand cloth） ×10 | Hyperweave（Hyperweave）×10 | 工作量=600；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Apparel_D1；技能: Crafting≥17；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### inorganic matter（`Matter`）

- 分类：`Manufactured/SuperMaterialCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create inorganic matter（`ConvertResourcesRaw`） | [ResourcesRaw] ×300 | Matter（inorganic matter）×150 | 工作量=10000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Super_matter_D1；技能: Crafting≥15；建筑: MatterConverter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Matter.xml` |

#### Integrated Circuits（`Electronics`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Deconstruct personal shield（`DeconstructPersonalShield`） | Apparel_ShieldBelt（Apparel_ShieldBelt） 或 Apparel_ShieldGuardian（guardian shield） ×1 | ComponentAdvanced（advanced component）×8；Electronics（Integrated Circuits）×1 | 工作量=1000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥10；建筑: TableRecycling | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_DeconstructPersonalShield.xml` |
| 工作台配方 | Assemble integrated circuits（`MakeIntegratedCircuits`） | Silicon（silicon） ×25；Plastic（Plastic） ×20；ElectronicComponents（Electronic Components） ×3；GoldBar（gold alloy） ×2 | Electronics（Integrated Circuits）×5 | 工作量=3500；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Electronics_C2；技能: Crafting≥12；建筑: TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |

#### Kevlar（`Kevlar`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create kevlar（`MakeKevlar`） | Plastic（Plastic） ×30；SyntheticFibers（Synthetic Fibers） ×15 | Kevlar（Kevlar）×10 | 工作量=1800；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: AdvancedFabrication；技能: Crafting≥13；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### laser rifle component（`Laser_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Laser component（`MakeLaser_Component`） | [USLDBar] ×20；[HCMBar] ×10；MagneticMaterial（Magnetic Material） ×4；Microchips（Microchip Integrated Circuits） ×1 | Laser_Component（laser rifle component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: ChargedShot；技能: Crafting≥15；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### Laser Turret Crate（`LaserTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build Laser Turret crate（`BuildLaserTurret_Crate`） | Weapon_Parts（weapon parts） ×8；Laser_Component（laser rifle component） ×1；[Metallic] ×30；Electronics（Integrated Circuits） ×5 | LaserTurret_Crate（Laser Turret Crate）×1 | 工作量=17500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Autoturrets_E1；技能: Crafting≥17；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### launcher weapon component（`Launcher_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Launcher component（`MakeLauncher_Component`） | Electronics（Integrated Circuits） ×2；Heavy_Component（heavy weapon component） ×1；AdvMechanism（advanced mechanism） ×3；Glass（Glass Frame） ×12 | Launcher_Component（launcher weapon component）×1 | 工作量=3100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rockets_C1；技能: Crafting≥14；建筑: AdvWeaponCraftingWorkTable, HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### lead alloy（`LeadBar`）

- 分类：`Manufactured/Metallic/HeavyBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt lead alloy（`MakeLeadBars_Electric`） | Anglesite（Anglesite Ore） ×20 | LeadBar（lead alloy）×20 | 工作量=1000；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥6；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt lead alloy（`MakeLeadBars_Hand`） | Anglesite（Anglesite Ore） ×10 | LeadBar（lead alloy）×10 | 工作量=1100；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥2；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Linen cloth（`Flaxcloth`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin linen cloth（`MakeLinen`） | RawFlax（Raw flax） ×20 | Flaxcloth（Linen cloth）×10 | 工作量=600；速度属性=TailoringSpeed；工作技能=Crafting | 技能: Crafting≥4；建筑: LeatherworkerTable, TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### Magnetic Material（`MagneticMaterial`）

- 分类：`Manufactured/SuperMaterialCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Synthesize magnetic material（`BuildMagneticMaterial`） | Matter（inorganic matter） ×30；Sulphates（Sulphates） ×15；[USLDBar] ×10；[SLDBar] ×5 | MagneticMaterial（Magnetic Material）×5 | 工作量=2400；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_D3；技能: Crafting≥18；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_HitechCraft.xml` |

#### mechanism（`Mechanism`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble mechanisms（`MakeSimpleMechanism_Electric`） | [SLDBar] ×60；ComponentIndustrial（ComponentIndustrial） ×20 | Mechanism（mechanism）×10 | 工作量=1800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥7；建筑: AdvToolBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 工作台配方 | Assemble mechanisms（`MakeSimpleMechanism_Hand`） | [SLDBar] ×30；ComponentIndustrial（ComponentIndustrial） ×10 | Mechanism（mechanism）×5 | 工作量=1800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥4；建筑: FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 摧毁残留 | Enemy ship（`EnemyShipPart`） | — | Mechanism（mechanism）×10 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events.xml` |
| 摧毁残留 | ancient reactor hive（`NuclearHive`） | — | Mechanism（mechanism）×20 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Technical.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | Mechanism（mechanism）×14 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | Mechanism（mechanism）×14 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | ShuttleCrashed（`ShuttleCrashed`） | — | Mechanism（mechanism）×2 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### melchior alloy（`CupronickelAlloy`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt melchior alloy（`MakeCupronickelAlloy`） | CopperBar（copper alloy） ×24；NickelBar（nickel alloy） ×6 | CupronickelAlloy（melchior alloy）×30 | 工作量=1600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C2；技能: Crafting≥12；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### mending kit（`MendingKit`）

- 分类：`Manufactured/MiscManufactured`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft mending kits（`MakeMendingKit`） | ComponentMedieval（primitive component） ×1；ComponentIndustrial（ComponentIndustrial） ×1；[Textiles] ×10；[SLDBar] ×10 | MendingKit（mending kit）×5 | 工作量=500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Repair_table_B1；技能: Crafting≥8；建筑: AdvToolBench, FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |

#### metal can（`MetalCan`）

- 分类：`Manufactured/MiscManufactured`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft metal cans（`MakeMetalCan`） | [Metallic] ×8 | MetalCan（metal can）×15 | 工作量=800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥2；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |

#### Microchip Integrated Circuits（`Microchips`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble microchip integrated circuit（`MakeMicrochipIntegratedCircuits`） | Electronics（Integrated Circuits） ×2；CarbonAlloy（carbon fiber alloy） ×5；GoldBar（gold alloy） ×5 | Microchips（Microchip Integrated Circuits）×1 | 工作量=3500；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Electronics_D1；技能: Crafting≥16；建筑: TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |

#### mineral sonar module（`MineralSonarModule`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble mineral sonar module（`MakeMineralSonarModule`） | [Metallic] ×25；ComponentAdvanced（advanced component） ×5；ElectronicComponents（Electronic Components） ×10；Mechanism（mechanism） ×1 | MineralSonarModule（mineral sonar module）×1 | 工作量=4800；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Mining_C1；技能: Crafting≥10；建筑: AdvToolBench, TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Electronics.xml` |

#### Muffalo wool（`WoolMuffalo`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | buffalo（`Buffalo`） | — | WoolMuffalo（Muffalo wool）×110 | 间隔天数=15 | — | 组件=CompProperties_Shearable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Arid.xml` |

#### Napalm（`Napalm`）

- 分类：`Manufactured/FuelCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create napalm（`MakeNapalm`） | Chemfuel（Chemfuel） ×30；Polymers（Polymers） ×15 | Napalm（Napalm）×10 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8；允许混合材料=true | 技能: Crafting≥5；建筑: AmmoBench, TableHeavyAmmunition | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Ammo.xml` |

#### nickel alloy（`NickelBar`）

- 分类：`Manufactured/Metallic/RARBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt nickel alloy（`ProcessNickelOre`） | Nickel（Nickel Ore） ×15 | NickelBar（nickel alloy）×15 | 工作量=1500；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥13；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### nitinol alloy（`NitinolAlloy`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt nitinol alloy（`MakeNitinolAlloy`） | Titanium（titanium alloy） ×4；NickelBar（nickel alloy） ×11 | NitinolAlloy（nitinol alloy）×15 | 工作量=2600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥16；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### oxidized chemfuel（`OxidizedChemfuel`）

- 分类：`Manufactured/FuelCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Create oxidized chemfuel（`MakeOxidizedChemfuel`） | Chemfuel（Chemfuel） ×100；Nitre（Nitre） ×10；Sulphates（Sulphates） ×10；Prometheum（Prometheum） 或 FSX（FSX） ×5 | OxidizedChemfuel（oxidized chemfuel）×75 | 工作量=10000；速度属性=DrugSynthesisSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: BasicGravtech；技能: Crafting≥12；建筑: TableChemlab | — | — | `Mods/Core_SK_DLCModule/Odyssey/Defs/RecipeDefs/Recipes_RocketFuel.xml` |

#### painting supplies（`PaintingSupplies`）

- 分类：`Manufactured/MiscManufactured`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | create painting supplies (herbs)（`CookPaint`） | WildRose（Wild Rose） 或 Hypericum（Hypericum） 或 MintLeaves（Mint Leaves） ×10；[Woody] ×2 | PaintingSupplies（painting supplies）×1 | 工作量=550；速度属性=CookSpeed；工作技能=Crafting；允许混合材料=true | 研究: Art_A1；技能: Crafting≥5；建筑: ElectricStove, FueledStove | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Misc.xml` |
| 工作台配方 | create painting supplies（`CookPaintDye`） | Dye（Dye） ×10；[Woody] ×2 | PaintingSupplies（painting supplies）×1 | 工作量=550；速度属性=CookSpeed；工作技能=Crafting；允许混合材料=true | 研究: Art_A1；技能: Crafting≥5；建筑: ElectricStove, FueledStove | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Misc.xml` |

#### pistol component（`Pistol_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Pistol component（`MakePistol_Component`） | [SLDBar] 或 [USLDBar] ×6；[HCMBar] ×3；ComponentIndustrial（ComponentIndustrial） ×2；Wire（wire） ×1；Mechanism（mechanism） ×1 | Pistol_Component（pistol component）×1 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Gunsmithing；技能: Crafting≥6；建筑: AdvWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### plasma weapon component（`Plasma_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Plasma component（`MakePlasma_Component`） | [USLDBar] ×20；[HCMBar] ×10；BiosyntheticMaterial（BioSynthetic Material） ×4；Microchips（Microchip Integrated Circuits） ×1 | Plasma_Component（plasma weapon component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: ChargedShot；技能: Crafting≥15；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### pobedite alloy（`PobediteAlloy`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt pobedite alloy（`MakePobediteAlloy`） | Tungsten（tungsten alloy） ×3；CarbonAlloy（carbon fiber alloy） ×3；Cobalt（cobalt alloy） ×3 | PobediteAlloy（pobedite alloy）×10 | 工作量=2700；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥18；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### primitive component（`ComponentMedieval`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble primitive components from wood（`Make_ComponentMedieval`） | [Woody] ×20 | ComponentMedieval（primitive component）×10 | 工作量=900；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；建筑: AdvToolBench, CraftingSpot, FabricationBench, TableSawmillElectric, TableSawmillHand | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 工作台配方 | Assemble primitive components from metal（`Make_ComponentMedievalBronze`） | [SLDBar] 或 [HCMBar] 或 [PRSBar] ×10 | ComponentMedieval（primitive component）×10 | 工作量=900；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；建筑: AdvToolBench, CraftingSpot, FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |

#### redwood plank（`RedWoodPlank`）

- 分类：`Manufactured/Woody`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | saw redwood planks（`MakeRedWoodPlanks_Electric`） | RedWoodLog（redwood） ×20 | RedWoodPlank（redwood plank）×30 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥6；建筑: TableSawmillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | saw redwood planks（`MakeRedWoodPlanks_Hand`） | RedWoodLog（redwood） ×10 | RedWoodPlank（redwood plank）×15 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；技能: Crafting≥3；建筑: TableSawmillHand | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### reinforced concrete（`ReinforcedConcrete`）

- 分类：`Manufactured/ConstructionParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | create reinforced concrete（`MakeReinforcedConcrete_Electric`） | [Metallic] ×20；ConcreteResource（concrete） ×20 | ReinforcedConcrete（reinforced concrete）×16 | 工作量=1200；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥7；建筑: AdvToolBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |
| 工作台配方 | create reinforced concrete（`MakeReinforcedConcrete_Hand`） | [Metallic] ×10；ConcreteResource（concrete） ×10 | ReinforcedConcrete（reinforced concrete）×8 | 工作量=1600；速度属性=StonecuttingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Concrete_C1；技能: Crafting≥4；建筑: TableStonecutter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Resources.xml` |

#### Reinforced Glass Frame（`ReinforcedGlass`）

- 分类：`Manufactured/GlassCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | reinforce glass frames（`MakeReinforcedGlass`） | GlassBatch（Glass Batch） ×40；Carbon（Carbon） ×5；SyntheticFibers（Synthetic Fibers） ×5 | ReinforcedGlass（Reinforced Glass Frame）×5 | 工作量=1400；速度属性=SmeltingSpeed；技能经验系数=0.8 | 研究: Glass_C1；技能: Crafting≥8；建筑: GlassworkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### rifle component（`Rifle_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Rifle Component（`MakeRifle_Component`） | [SLDBar] 或 [USLDBar] ×20；[HCMBar] ×10；ComponentIndustrial（ComponentIndustrial） ×6；Wire（wire） ×8；Mechanism（mechanism） ×3 | Rifle_Component（rifle component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Rifles_C1；技能: Crafting≥8；建筑: AdvWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### robot parts（`RobotParts`）

- 分类：`Manufactured/ElectronicsCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | assemble robot parts（`MakeRobotParts`） | [USLDBar] ×60；AdvMechanism（advanced mechanism） ×15 | RobotParts（robot parts）×25 | 工作量=8000；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Droids_D1；技能: Crafting≥15；建筑: TableElectronics | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Misc.xml` |

#### sendust alloy（`FerrosiliconAlloy`）

- 分类：`Manufactured/Metallic/SLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt sendust alloy（`MakeFerrosiliconAlloy`） | Steel（steel alloy） ×15；Silicon（silicon） ×10；AluminiumBar（aluminium alloy） ×5 | FerrosiliconAlloy（sendust alloy）×30 | 工作量=1700；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C2；技能: Crafting≥8；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Sentry blaster crate（`SentryBlasterTurret_Crate`）

- 分类：`Manufactured/WeaponCrate`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Build 6x24mm Sentry blaster crate（`BuildSentryBlasterTurret_Crate`） | Weapon_Parts（weapon parts） ×8；Plasma_Component（plasma weapon component） ×1；[Metallic] ×30；Electronics（Integrated Circuits） ×5 | SentryBlasterTurret_Crate（Sentry blaster crate）×1 | 工作量=17500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Autoturrets_E1；技能: Crafting≥17；建筑: HeavyArmsBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Turrets.xml` |

#### shotgun component（`Shotgun_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Shotgun component（`MakeShotgun_Component`） | [SLDBar] 或 [USLDBar] ×15；[HCMBar] ×8；ComponentIndustrial（ComponentIndustrial） ×4；Wire（wire） ×5；Mechanism（mechanism） ×2 | Shotgun_Component（shotgun component）×1 | 工作量=1800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: GasOperation；技能: Crafting≥7；建筑: AdvWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### SMG component（`SMG_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make SMG component（`MakeSMG_Component`） | [SLDBar] 或 [USLDBar] ×10；[HCMBar] ×5；ComponentIndustrial（ComponentIndustrial） ×4；Wire（wire） ×6；Mechanism（mechanism） ×2 | SMG_Component（SMG component）×1 | 工作量=1800；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: BlowbackOperation；技能: Crafting≥9；建筑: AdvWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### sniper component（`Sniper_Component`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Sniper Component（`MakeSniper_Component`） | Rifle_Component（rifle component） ×1；ComponentIndustrial（ComponentIndustrial） ×6；Glass（Glass Frame） ×12 | Sniper_Component（sniper component）×1 | 工作量=2100；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Sniper_rifles_C1；技能: Crafting≥10；建筑: AdvWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### spacer component（`ComponentSpacer`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble spacer components（`Make_ComponentSpacer`） | [USLDBar] ×25；[PRSBar] ×5 | ComponentSpacer（spacer component）×10 | 工作量=3500；速度属性=SmithingSpeed；技能经验系数=0.8 | 研究: Components_D1；技能: Crafting≥9；建筑: AdvToolBench | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml` |
| 摧毁残留 | DefoliatorShipPart（`DefoliatorShipPart`） | — | ComponentSpacer（spacer component）×4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | Enemy ship（`EnemyShipPart`） | — | ComponentSpacer（spacer component）×5 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events.xml` |
| 摧毁残留 | PsychicDronerShipPart（`PsychicDronerShipPart`） | — | ComponentSpacer（spacer component）×4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### steel alloy（`Steel`）

- 分类：`Manufactured/Metallic/SLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt steel alloy（`MakeSteelBars_Electric`） | Iron（Iron Ore） ×15；[Coal] ×7 | Steel（steel alloy）×20 | 工作量=900；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥6；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt steel alloy（`MakeSteelBars_Hand`） | Iron（Iron Ore） ×15；[Coal] ×13 | Steel（steel alloy）×20 | 工作量=1300；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥3；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 摧毁残留 | Enemy ship（`EnemyShipPart`） | — | Steel（steel alloy）×150 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs/Events.xml` |
| 摧毁残留 | ancient reactor hive（`NuclearHive`） | — | Steel（steel alloy）×155 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Technical.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | Steel（steel alloy）×50 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | Steel（steel alloy）×50 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | ShuttleCrashed（`ShuttleCrashed`） | — | Steel（steel alloy）×7 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 野外矿点 | WorkSite_Mining（`WorkSite_Mining`） | — | Steel（steel alloy）×按矿点规则 | 权重=4 | — | Ideology 工作台/矿点营地产出 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml` |

#### stein alloy（`SteelBar`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt stein alloy（`MakeFerronickelAlloy`） | Steel（steel alloy） ×15；NickelBar（nickel alloy） ×5 | SteelBar（stein alloy）×20 | 工作量=1800；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C3；技能: Crafting≥11；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### stellite alloy（`StelliteAlloy`）

- 分类：`Manufactured/Metallic/USLDBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt stellite alloy（`MakeStelliteAlloy`） | Cobalt（cobalt alloy） ×5；Chromium（chromium alloy） ×13；Tungsten（tungsten alloy） ×2 | StelliteAlloy（stellite alloy）×20 | 工作量=2100；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥17；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### sterling silver alloy（`SilverBar`）

- 分类：`Manufactured/Metallic/PRSBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt sterling silver alloy（`MakeSilverBars_Electric`） | Silver（Silver Ore） ×20；[HCM] ×5 | SilverBar（sterling silver alloy）×15 | 工作量=650；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥5；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt sterling silver alloy（`MakeSilverBars_Hand`） | Silver（Silver Ore） ×14；[HCM] ×3 | SilverBar（sterling silver alloy）×10 | 工作量=750；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥2；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### Synthread（`Synthread`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin synthread（`MakeSynthreadCloth`） | RawCotton（Raw Cotton） ×10；SyntheticFibers（Synthetic Fibers） ×3 | Synthread（Synthread）×10 | 工作量=600；速度属性=TailoringSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: AdvancedFabrication；技能: Crafting≥12；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### tallow（`Tallow`）

- 分类：`Manufactured/FuelCat`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | render tallow（`MakeTallow`） | [MeatRaw] ×30 | Tallow（tallow）×15 | 工作量=1200；速度属性=CookSpeed；工作技能=Cooking；技能经验系数=0.8；允许混合材料=true | 研究: Food_00；技能: Cooking≥3；建筑: Campfire, FueledStove | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### tin alloy（`TinBar`）

- 分类：`Manufactured/Metallic/HCMBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt tin alloy（`MakeTinBars_Electric`） | Tin（Tin Ore） ×20 | TinBar（tin alloy）×20 | 工作量=500；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥3；建筑: ElectricSmelter | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |
| 工作台配方 | Smelt tin alloy（`MakeTinBars_Hand`） | Tin（Tin Ore） ×10 | TinBar（tin alloy）×10 | 工作量=600；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_B1；技能: Crafting≥1；建筑: TableFurnace | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### titanium alloy（`Titanium`）

- 分类：`Manufactured/Metallic/RARBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt titanium alloy（`ProcessTitanium`） | Ilmenite（Ilmenite Ore） ×15 | Titanium（titanium alloy）×15 | 工作量=3300；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥16；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### tungsten alloy（`Tungsten`）

- 分类：`Manufactured/Metallic/RARBar`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Smelt tungsten alloy（`ProcessTungsten`） | Wolframite（Wolframite Ore） ×15 | Tungsten（tungsten alloy）×15 | 工作量=3000；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_D1；技能: Crafting≥17；建筑: EAF | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` |

#### ultra component（`ComponentUltra`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Assemble ultra components（`Make_ComponentUltra`） | [RARBar] ×15；[USLDHBar] ×10；[PRSBar] ×5 | ComponentUltra（ultra component）×10 | 工作量=4500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Components_E1；技能: Crafting≥13；建筑: RobAssem | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |

#### Velour（`Velour`）

- 分类：`Textiles/HTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Spin velour（`MakeVelour`） | Compaste（Compound paste） ×5；[BTextiles] ×40 | Velour（Velour）×25 | 工作量=1800；速度属性=TailoringSpeed；工作技能=Crafting | 研究: AdvancedFabrication；技能: Crafting≥12；建筑: TableLoom | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Cloth.xml` |

#### weapon parts（`Weapon_Parts`）

- 分类：`Manufactured/WeaponParts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Weapon parts（`MakeWeapon_Parts`） | [Metallic] ×25；ComponentIndustrial（ComponentIndustrial） ×10；Mechanism（mechanism） ×5 | Weapon_Parts（weapon parts）×5 | 工作量=1500；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Machining；技能: Crafting≥6；建筑: AdvWeaponCraftingWorkTable, MechWeaponCraftingWorkTable, WeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_WeaponComponents.xml` |

#### wire（`Wire`）

- 分类：`Manufactured/Parts`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Craft wires（`MakeWires_Electric`） | [HCMBar] ×20 | Wire（wire）×40 | 工作量=800；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥6；建筑: AdvToolBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 工作台配方 | Craft wires（`MakeWires_Hand`） | [HCMBar] ×10 | Wire（wire）×20 | 工作量=800；速度属性=ElectronicCraftingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；技能: Crafting≥3；建筑: FabricationBench | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Components.xml` |
| 摧毁残留 | ancient reactor hive（`NuclearHive`） | — | Wire（wire）×75 | — | — | 摧毁来源物后获得 | — | `Mods/Core_SK/Defs/ThingDefs_Buildings/Buildings_Technical.xml` |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | Wire（wire）×40 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | Wire（wire）×40 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | ShuttleCrashed（`ShuttleCrashed`） | — | Wire（wire）×18 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### wood plank（`WoodPlank`）

- 分类：`Manufactured/Woody`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | saw wood planks（`MakeWoodPlanks_Electric`） | WoodLog（wood log） ×20 | WoodPlank（wood plank）×30 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Metals_C1；技能: Crafting≥6；建筑: TableSawmillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |
| 工作台配方 | saw wood planks（`MakeWoodPlanks_Hand`） | WoodLog（wood log） ×10 | WoodPlank（wood plank）×15 | 工作量=1200；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: Craft_0；技能: Crafting≥3；建筑: TableSawmillHand | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Production.xml` |

#### WoolSheep（`WoolSheep`）

- 分类：`Textiles/BTextiles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | Sheep（`Sheep`） | — | WoolSheep（WoolSheep）×45 | 间隔天数=10 | — | 组件=CompProperties_Shearable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |

### 2.4 食物

#### aged cheese（`AgedCheese`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make aged cheese（`Make_AgedCheese`） | Milk（Milk） ×10 | AgedCheese（aged cheese）×3 | 处理天数=8；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.05；安全温度=0~15；理想温度=2~8 | 建筑: CheeseBasin | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |
| 发酵/缓处理配方 | make aged cheese x10（`Make_AgedCheese_Vat_x10`） | Milk（Milk） ×100 | AgedCheese（aged cheese）×30 | 处理天数=8；槽位=10 | 建筑: CheeseVat | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |
| 发酵/缓处理配方 | make aged cheese x5（`Make_AgedCheese_x5`） | Milk（Milk） ×50 | AgedCheese（aged cheese）×15 | 处理天数=8；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.05；安全温度=0~15；理想温度=2~8 | 建筑: CheeseBasin | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |

#### Apple cider（`Alcohol_Cider`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make cider（`Make_Cider`） | cidermash（Cider Mash） ×40 | Alcohol_Cider（Apple cider）×20 | 处理天数=4.8；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=7.0~22.0；理想温度=11.0~15.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty cider crate（`EmptyCrate_cider`） | Crate_Cider（crate of apple cider） ×1 | Alcohol_Cider（Apple cider）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Applejack（`Alcohol_Applejack`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty applejack crate（`EmptyCrate_Applejack`） | Crate_Applejack（crate of applejack） ×1 | Alcohol_Applejack（Applejack）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Arak（`Alcohol_Arak`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty arak crate（`EmptyCrate_Arak`） | Crate_Arak（crate of arak） ×1 | Alcohol_Arak（Arak）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Basic Canned Meal（`BasicCannedFood`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook basic canned food（`MakeBasicCannedFood`） | [BasicPlantFoodRaw] ×75；MetalCan（metal can） ×5 | BasicCannedFood（Basic Canned Meal）×75 | 工作量=1800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C7；建筑: Canningstove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### Beer（`Beer`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make beer（`Make_Beer`） | Wort（Wort） ×20 | Beer（Beer）×20 | 处理天数=6；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=-1.0~32.0；理想温度=7.0~32.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |

#### Berry Mash（`berrymash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew berry mash（`Makeberrymash`） | RawBerries（Strawberries） ×35；Yeast（Yeast） ×5 | berrymash（Berry Mash）×10 | 工作量=600；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Brewing；技能: Cooking≥8；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Berry Wine（`Alcohol_BerryWine`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make berry wine（`Make_Berrywine`） | berrymash（Berry Mash） ×25 | Alcohol_BerryWine（Berry Wine）×20 | 处理天数=5.8；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=10.0~40.0；理想温度=18.0~32.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty berry wine crate（`EmptyCrate_strawberrywine`） | Crate_BerryWine（crate of berry wine） ×1 | Alcohol_BerryWine（Berry Wine）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Brandy（`Alcohol_Brandy`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty brandy crate（`EmptyCrate_Brandy`） | Crate_Brandy（crate of brandy） ×1 | Alcohol_Brandy（Brandy）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### bread（`bread`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake bread loaves（`MakeBread`） | Flour（flour） ×10 | bread（bread）×2 | 速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_B1；技能: Cooking≥2；建筑: ElectricOven, TableOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Canned prime meat（`MetalCannedMeat_prime`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook canned prime meat（`MakeMetalCannedMeat_prime`） | Meat_Muffalo（Meat_Muffalo） 或 [AnimalProductRaw] ×75；MetalCan（metal can） ×5 | MetalCannedMeat_prime（Canned prime meat）×75 | 工作量=1800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C7；建筑: Canningstove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### Canned raw meat（`MetalCannedMeat_raw`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook canned raw meat（`MakeMetalCannedMeat_raw`） | [MeatRaw] ×75；MetalCan（metal can） ×5 | MetalCannedMeat_raw（Canned raw meat）×75 | 工作量=1800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C7；建筑: Canningstove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### Caramel（`Caramel`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Caramelize sugar（`MakeCaramel`） | Sugar（sugar） ×9 | Caramel（Caramel）×3 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C4；技能: Cooking≥6；建筑: CandyTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### cheese（`Cheese`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make cheese（`Make_Cheese`） | Milk（Milk） 或 SoyMilk（soy milk） ×10 | Cheese（cheese）×5 | 处理天数=4；低于安全温度速度=0.05；高于安全温度速度=0.5；安全温度=0~30；理想温度=10~25 | 建筑: CheeseBasin | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |
| 发酵/缓处理配方 | make cheese x10（`Make_Cheese_Vat_x10`） | Milk（Milk） 或 SoyMilk（soy milk） ×100 | Cheese（cheese）×50 | 处理天数=4；槽位=10 | 建筑: CheeseVat | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |
| 发酵/缓处理配方 | make cheese x5（`Make_Cheese_x5`） | Milk（Milk） 或 SoyMilk（soy milk） ×50 | Cheese（cheese）×25 | 处理天数=4；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.5；安全温度=0~30；理想温度=10~25 | 建筑: CheeseBasin | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Cheese.xml` |
| 工作台配方 | process cheese（`Makecheese`） | [CookingSupplies] ×0.6 营养值 | Cheese（cheese）×4 | 工作量=850；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B1；技能: Cooking≥4；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### cheese burger（`CheeseBurger`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook cheese burger（`MakeBurgerCheese`） | Meat_Muffalo（Meat_Muffalo） 或 MetalCannedMeat_prime（Canned prime meat） ×0.5 营养值；bread（bread） ×0.6 营养值；AgedCheese（aged cheese） 或 Cheese（cheese） ×0.3 营养值；RawTomatoes（Tomatoes） ×0.3 营养值 | CheeseBurger（cheese burger）×2 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C6；技能: Cooking≥13；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Cheese Sandwich（`GrilledCheese`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Grill cheese sandwich（`MakeCheeseGrilled`） | Cheese（cheese） 或 AgedCheese（aged cheese） ×0.2 营养值；bread（bread） ×0.1 营养值 | GrilledCheese（Cheese Sandwich）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B3；技能: Cooking≥5；建筑: ElectricStove, FueledStove, TableGrill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | Grill cheese sandwich (x4)（`MakeCheeseGrilledBulk`） | Cheese（cheese） 或 AgedCheese（aged cheese） ×1 营养值；bread（bread） ×0.5 营养值 | GrilledCheese（Cheese Sandwich）×8 | 工作量=1900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥7；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Chocolate Milk（`Chocomilk`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | mix chocolate milks（`MakeChocoMilk`） | Chocolate（Chocolate） ×0.3 营养值；[CookingSupplies] ×0.9 营养值 | Chocomilk（Chocolate Milk）×3 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C3；技能: Cooking≥6；建筑: TableCoffee | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |

#### Cider Mash（`cidermash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew cider mash（`Makecidermash`） | Rawapple（Apples） ×35；Yeast（Yeast） ×5 | cidermash（Cider Mash）×10 | 工作量=600；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Brewing；技能: Cooking≥7；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Cocoa Beans（`Rawcocoa`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Cocoa Tree（`Plant_TreeCocoa`） | — | Rawcocoa（Cocoa Beans）×16 | 收获量=16；生长天数=9；肥力敏感度=1.2 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Fruits.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### Coconut Curry（`CoconutCurry`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook coconut curry（`MakeCoconutCurry`） | Tofu（tofu） 或 [MeatRaw] ×0.3 营养值；coconutmilk（coconut milk） ×0.4 营养值；MintLeaves（Mint Leaves） ×0.1 营养值；RawRice（RawRice） ×0.4 营养值 | CoconutCurry（Coconut Curry）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C2；技能: Cooking≥9；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | cook coconut curry (x4)（`MakeCoconutCurryBulk`） | Tofu（tofu） 或 [MeatRaw] ×1.5 营养值；coconutmilk（coconut milk） ×2.5 营养值；RawRice（RawRice） ×2 营养值 | CoconutCurry（Coconut Curry）×8 | 工作量=2000；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥11；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### coconut milk（`coconutmilk`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | split coconuts（`HalfCoconut`） | RawCoconut（Coconut） ×0.5 营养值 | coconutmilk（coconut milk）×5 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Pemmican；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### coffee berries（`RawCoffee`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Coffee Plant（`Plant_Coffee`） | — | RawCoffee（coffee berries）×7 | 收获量=7；生长天数=8；最低肥力=1.25；肥力敏感度=1.3 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Cognac（`Alcohol_Cognac`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty cognac crate（`EmptyCrate_Cognac`） | Crate_Cognac（crate of cognac） ×1 | Alcohol_Cognac（Cognac）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### cookie（`cookie`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake chocolate cookies（`MakeCookies`） | Sugar（sugar） ×0.4 营养值；Flour（flour） ×0.3 营养值 | cookie（cookie）×10 | 工作量=800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_B2；技能: Cooking≥6；建筑: ElectricOven, TableOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |

#### Cup of Coffee（`CupCoffee`）

- 分类：`Foods/Energetics`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | brew cups of coffee（`BrewCoffee`） | RoastedCoffeeBeans（roasted coffee beans） ×15 | CupCoffee（Cup of Coffee）×5 | 工作量=1800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C3；技能: Cooking≥2；建筑: TableCoffee | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |
| 工作台配方 | brew cup of coffee（`BrewCoffeeSingle`） | GroundCoffee（ground coffee） ×5 | CupCoffee（Cup of Coffee）×1 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B2；技能: Cooking≥8；建筑: Campfire, ElectricStove, FueledStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |

#### Cup of Tea（`Cuptea`）

- 分类：`Foods/Energetics`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | brew cups of tea（`BrewTea`） | RawTea（Tea Leaves） ×15；Sugar（sugar） ×3 | Cuptea（Cup of Tea）×3 | 工作量=1200；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_00；技能: Cooking≥2；建筑: Campfire, ElectricStove, FueledStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |
| 工作台配方 | brew bulk cups of tea（`BrewTeaBulk`） | RawTea（Tea Leaves） ×60；Sugar（sugar） ×12 | Cuptea（Cup of Tea）×12 | 工作量=2400；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C3；技能: Cooking≥8；建筑: TableCoffee | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |

#### Dried Fruit（`driedfruit`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make dried fruit（`Make_DriedFruit`） | [FruitFoodRaw] ×0.75 营养值 | driedfruit（Dried Fruit）×5 | 处理天数=2；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=5~80；理想温度=18~50；允许混合材料=true | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |
| 发酵/缓处理配方 | make dried fruit（`Make_DriedFruitIndustrial_x1`） | [FruitFoodRaw] ×0.75 营养值 | driedfruit（Dried Fruit）×5 | 处理天数=1；槽位=1；允许混合材料=true | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried fruit x10（`Make_DriedFruitIndustrial_x10`） | [FruitFoodRaw] ×7.5 营养值 | driedfruit（Dried Fruit）×50 | 处理天数=1；槽位=10；允许混合材料=true | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried fruit x5（`Make_DriedFruitIndustrial_x5`） | [FruitFoodRaw] ×3.75 营养值 | driedfruit（Dried Fruit）×25 | 处理天数=1；槽位=5；允许混合材料=true | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make dried fruit x5（`Make_DriedFruit_x5`） | [FruitFoodRaw] ×3.75 营养值 | driedfruit（Dried Fruit）×25 | 处理天数=2；槽位=5；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=5~80；理想温度=18~50；允许混合材料=true | 建筑: DryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |

#### Extra Canned Meal（`ExtraCannedFood`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook extra canned food（`MakeExtraCannedFood`） | [ExtraPlantFoodRaw] ×75；MetalCan（metal can） ×5 | ExtraCannedFood（Extra Canned Meal）×75 | 工作量=1800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C7；建筑: Canningstove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### Fishing bait（`FishingBait`）

- 分类：`Foods/Animalfood`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make fishing baits（`MakeFishingBait`） | [BasicPlantFoodRaw] 或 [FruitFoodRaw] 或 [MeatRaw] ×2.5 营养值 | FishingBait（Fishing bait）×5 | 工作量=1000；速度属性=CookSpeed；工作技能=Crafting；允许混合材料=true | 研究: Fish_A1；技能: Crafting≥1；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Fishing.xml` |

#### flour（`Flour`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make flour（`Grind_Flour`） | Rawwheat（Wheat） 或 RawCorn（RawCorn） 或 RawRice（RawRice） ×100 | Flour（flour）×50 | 处理天数=0.3 | 研究: Bakery_B2；建筑: SK_Windmill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |
| 工作台配方 | grind flour（`MakeFlour`） | Rawwheat（Wheat） 或 RawCorn（RawCorn） 或 RawRice（RawRice） ×20 | Flour（flour）×10 | 工作量=600；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_B1；技能: Cooking≥2；建筑: GrindStone, MillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Fruit Drink（`FruitDrink`）

- 分类：`Foods/Energetics`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | press fruit drinks（`MakeFruitDrink`） | [FruitFoodRaw] ×0.8 营养值 | FruitDrink（Fruit Drink）×3 | 工作量=1350；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B2；技能: Cooking≥4；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |

#### Fruit pie（`PieBlueberry`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake fruit pies（`MakePieBlueberry`） | Flour（flour） ×0.08 营养值；[FruitFoodRaw] ×0.4 营养值 | PieBlueberry（Fruit pie）×2 | 工作量=1200；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_B2；技能: Cooking≥8；建筑: ElectricOven, TableOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |

#### Fruit Yogurt（`FruitYogurt`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook fruit yogurts（`MakeFruitYogurt`） | [FruitFoodRaw] ×0.4 营养值；[CookingSupplies] ×0.6 营养值 | FruitYogurt（Fruit Yogurt）×3 | 速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B2；技能: Cooking≥8；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |
| 工作台配方 | cook bulk fruit yogurts（`MakeFruitYogurt_Bulk`） | [FruitFoodRaw] ×1.6 营养值；[CookingSupplies] ×2.5 营养值 | FruitYogurt（Fruit Yogurt）×10 | 速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥12；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Drinks.xml` |

#### Gin（`Alcohol_Gin`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty gin crate（`EmptyCrate_Gin`） | Crate_Gin（crate of gin） ×1 | Alcohol_Gin（Gin）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Grain Mash（`grainmash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew grain mash（`Makegrainmash`） | Rawwheat（Wheat） ×25；Yeast（Yeast） ×10 | grainmash（Grain Mash）×10 | 工作量=800；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥5；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### ground coffee（`GroundCoffee`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make ground coffee（`GrindingCoffeeBeans`） | RoastedCoffeeBeans（roasted coffee beans） ×5 | GroundCoffee（ground coffee）×5 | 工作量=300；速度属性=CookSpeed；工作技能=Cooking | 研究: Pemmican；技能: Cooking≥6；建筑: GrindStone, MillElectric, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Hay（`Hay`）

- 分类：`Foods/Animalfood`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_Bulrush（`Plant_Bulrush`） | — | Hay（Hay）×8 | 收获量=8 | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml` |
| 植物收获 | Plant_Grass（`Plant_Grass`） | — | Hay（Hay）×3 | 收获量=3；生长天数=2.3；收获工作量=40；最低肥力=0.07 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml` |
| 植物收获 | Plant_Haygrass（`Plant_Haygrass`） | — | Hay（Hay）×8 | 收获量=8；生长天数=2.5；最低肥力=0.08；肥力敏感度=0.7 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml` |
| 植物收获 | Plant_Reeds（`Plant_Reeds`） | — | Hay（Hay）×3 | 收获量=3 | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml` |
| 植物收获 | Plant_TallGrass（`Plant_TallGrass`） | — | Hay（Hay）×6 | 收获量=6；生长天数=2.8；最低肥力=0.08 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml` |
| 植物收获 | Plant_YellowGrass（`Plant_YellowGrass`） | — | Hay（Hay）×8 | 收获量=8 | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Grass.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Grass.xml` |

#### Hearty Stew（`HeartyStew`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook hearty stew（`CookHeartyStew`） | [MeatRaw] 或 [MeatSubRaw] ×0.5 营养值；RawPotatoes（RawPotatoes） ×0.4 营养值；RawCarrots（Carrots） ×0.3 营养值 | HeartyStew（Hearty Stew）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C1；技能: Cooking≥8；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | cook hearty stew (x4)（`CookHeartyStewBulk`） | [MeatRaw] 或 [MeatSubRaw] ×2.5 营养值；RawPotatoes（RawPotatoes） ×2 营养值；RawCarrots（Carrots） ×1.5 营养值 | HeartyStew（Hearty Stew）×8 | 工作量=1900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥10；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Hypericum（`Hypericum`）

- 分类：`Foods/LuxuryStuffs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | hypericum plant（`Plant_Hypericum`） | — | Hypericum（Hypericum）×4 | 收获量=4；生长天数=6；播种工作量=150；最低肥力=1.3；肥力敏感度=1.25 | — | 播种标签: Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Wild.xml` |

#### Ice Cream（`IceCream`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Produce Ice Cream（`MakeIceCream`） | Milk（Milk） ×12；Sugar（sugar） 或 WildRose（Wild Rose） 或 [FruitFoodRaw] ×5 | IceCream（Ice Cream）×3 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C4；技能: Cooking≥6；建筑: CandyTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Jerky（`MealJerky`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make jerky（`Make_Jerky`） | [MeatRaw] ×10 | MealJerky（Jerky）×10 | 处理天数=2；低于安全温度速度=0.6；高于安全温度速度=1；安全温度=10~55；理想温度=25~40 | 建筑: JerkyRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |
| 发酵/缓处理配方 | make jerky（`Make_JerkyIndustrial_x1`） | [MeatRaw] ×10 | MealJerky（Jerky）×10 | 处理天数=0.5；槽位=1 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make jerky x10（`Make_JerkyIndustrial_x10`） | [MeatRaw] ×100 | MealJerky（Jerky）×100 | 处理天数=0.5；槽位=10 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make jerky x5（`Make_JerkyIndustrial_x5`） | [MeatRaw] ×50 | MealJerky（Jerky）×50 | 处理天数=0.5；槽位=5 | 建筑: ElectricDryingRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying_Industrial.xml` |
| 发酵/缓处理配方 | make jerky x5（`Make_Jerky_x5`） | [MeatRaw] ×50 | MealJerky（Jerky）×50 | 处理天数=2；槽位=5；低于安全温度速度=0.6；高于安全温度速度=1；安全温度=10~55；理想温度=25~40 | 建筑: JerkyRack | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Drying.xml` |

#### Kahlua（`Alcohol_Kahlua`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty kahlua crate（`EmptyCrate_Kahlua`） | Crate_Kahlua（crate of kahlua） ×1 | Alcohol_Kahlua（Kahlua）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Kebab（`Kebab`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make kebab（`MakeKebab`） | [MeatRaw] ×0.7 营养值；RawTomatoes（Tomatoes） ×0.3 营养值；[FungusPlantRaw] ×0.3 营养值 | Kebab（Kebab）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C1；技能: Cooking≥8；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | make kebab (x4)（`MakeKebabBulk`） | [MeatRaw] ×3.5 营养值；RawTomatoes（Tomatoes） ×1.5 营养值；[FungusPlantRaw] ×1.5 营养值 | Kebab（Kebab）×8 | 工作量=1900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥10；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Maki Rolls（`MakiRolls`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make maki rolls（`MakeMakiRolls`） | Meat_ThingDefFishSduiggles（Meat_ThingDefFishSduiggles） ×0.5 营养值；RawAlgae（Algae） ×0.2 营养值；RawRice（RawRice） ×0.5 营养值 | MakiRolls（Maki Rolls）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C2；技能: Cooking≥9；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | make maki rolls (x4)（`MakeMakiRollsBulk`） | Meat_ThingDefFishSduiggles（Meat_ThingDefFishSduiggles） ×2.5 营养值；RawAlgae（Algae） ×1 营养值；RawRice（RawRice） ×2.5 营养值 | MakiRolls（Maki Rolls）×8 | 工作量=1900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥11；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Meat Soup（`MeatSoup`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook meat soup（`CookMeatSoup`） | [MeatRaw] 或 [MeatSubRaw] ×0.65 营养值；[BasicPlantFoodRaw] 或 [FungusPlantRaw] 或 [ExtraPlantFoodRaw] ×0.5 营养值 | MeatSoup（Meat Soup）×3 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B4；技能: Cooking≥4；建筑: ElectricStove, FueledStove, OpenHearth | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | cook meat soup (x4)（`CookMeatSoupBulk`） | [MeatRaw] 或 [MeatSubRaw] ×3.25 营养值；[BasicPlantFoodRaw] 或 [FungusPlantRaw] 或 [ExtraPlantFoodRaw] ×2.5 营养值 | MeatSoup（Meat Soup）×12 | 工作量=1900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥6；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Milk（`Milk`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物挤奶 | Bison（`Bison`） | — | Milk（Milk）×9 | 间隔天数=1 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 动物挤奶 | buffalo（`Buffalo`） | — | Milk（Milk）×14 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Arid.xml` |
| 动物挤奶 | Caribou（`Caribou`） | — | Milk（Milk）×12 | 间隔天数=2 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 动物挤奶 | Cow（`Cow`） | — | Milk（Milk）×16 | 间隔天数=1 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 动物挤奶 | Dromedary（`Dromedary`） | — | Milk（Milk）×11 | 间隔天数=2 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 动物挤奶 | Elk（`Elk`） | — | Milk（Milk）×10 | 间隔天数=1 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 动物挤奶 | Goat（`Goat`） | — | Milk（Milk）×13 | 间隔天数=2 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 动物挤奶 | Horse（`Horse`） | — | Milk（Milk）×10 | 间隔天数=2 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Horse.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Horse.xml` |
| 动物挤奶 | woolly horse（`Horse_Wooly`） | — | Milk（Milk）×10 | 间隔天数=2 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Horse.xml` |
| 动物挤奶 | Ibex（`Ibex`） | — | Milk（Milk）×7 | 间隔天数=1 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 动物挤奶 | Muffalo（`Muffalo`） | — | Milk（Milk）×13 | 间隔天数=1 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 动物挤奶 | okapi（`Okapi`） | — | Milk（Milk）×16 | 间隔天数=2 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Tropical.xml` |
| 动物挤奶 | Sheep（`Sheep`） | — | Milk（Milk）×10 | 间隔天数=3 | — | 组件=CompProperties_Milkable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |

#### Mint Leaves（`MintLeaves`）

- 分类：`Foods/LuxuryStuffs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | mint plant（`Plant_Mint`） | — | MintLeaves（Mint Leaves）×5 | 收获量=5；生长天数=6.5；播种工作量=150；最低肥力=1.35；肥力敏感度=1.3 | — | 播种标签: Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Wild.xml` |

#### Miso Soup（`MisoSoup`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook miso soup（`MakeMisoSoup`） | Tofu（tofu） ×0.5 营养值；[FungusPlantRaw] ×0.4 营养值；Rawonion（Onion） ×0.03 营养值；RawAlgae（Algae） ×0.25 营养值 | MisoSoup（Miso Soup）×2 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C6；技能: Cooking≥14；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Moonshine（`Alcohol_Moonshine`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | empty moonshine crate（`EmptyCrate_Moonshine`） | Crate_Moonshine（crate of moonshine） ×1 | Alcohol_Moonshine（Moonshine）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Mushroom madness（`MushroomMadness`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make mushroom madness（`MakeMushroomMadness`） | RawShimmershroom（shimmershroom） ×0.4 营养值；RawGlowbulb（glowbulb） ×0.4 营养值；Rawmushroom（Raw Champignon） ×0.4 营养值 | MushroomMadness（Mushroom madness）×2 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C2；技能: Cooking≥7；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | make mushroom madness (x4)（`MakeMushroomMadnessBulk`） | RawShimmershroom（shimmershroom） ×2 营养值；RawGlowbulb（glowbulb） ×2 营养值；Rawmushroom（Raw Champignon） ×2 营养值 | MushroomMadness（Mushroom madness）×8 | 工作量=2000；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥9；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Onion（`Rawonion`）

- 分类：`Foods/LuxuryStuffs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | onion plant（`Plant_Onion`） | — | Rawonion（Onion）×4 | 收获量=4；生长天数=4.5；最低肥力=1.2；肥力敏感度=1.35 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Wild.xml` |

#### Panfried Vegetables（`FriedVegetables`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | panfry vegetables（`MakeVegetables`） | [BasicPlantFoodRaw] 或 [FungusPlantRaw] ×0.45 营养值 | FriedVegetables（Panfried Vegetables）×1 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_00；技能: Cooking≥0；建筑: Campfire, ElectricStove, FueledStove, OpenHearth, TableGrill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |
| 工作台配方 | panfry vegetables (x4)（`MakeVegetablesBulk`） | [BasicPlantFoodRaw] 或 [FungusPlantRaw] ×2.25 营养值 | FriedVegetables（Panfried Vegetables）×4 | 工作量=1350；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥0；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |

#### Pelmeni（`Pelmeni`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make pelmeni（`MakePelmeni`） | Meat_Muffalo（Meat_Muffalo） 或 MetalCannedMeat_prime（Canned prime meat） ×0.7 营养值；Flour（flour） ×0.05 营养值 | Pelmeni（Pelmeni）×2 | 工作量=800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_C1；技能: Cooking≥8；建筑: ElectricStove | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | make pelmeni (x4)（`MakePelmeniBulk`） | Meat_Muffalo（Meat_Muffalo） 或 MetalCannedMeat_prime（Canned prime meat） ×3.5 营养值；Flour（flour） ×0.25 营养值 | Pelmeni（Pelmeni）×8 | 工作量=2200；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥10；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Pemmican（`Pemmican`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make_Pemmican（`Make_Pemmican`） | MealJerky（Jerky） ×0.45；driedfruit（Dried Fruit） ×0.15 | Pemmican（Pemmican）×20 | 工作量=300 | 研究: Food_0；建筑: ButcherSpot, TableButcher, TableKitchen | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml` |
| 工作台配方 | Make_PemmicanBulk（`Make_PemmicanBulk`） | MealJerky（Jerky） ×2.25；driedfruit（Dried Fruit） ×0.75 | Pemmican（Pemmican）×80 | 工作量=1200 | 建筑: ButcherSpot, TableButcher, TableKitchen | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml` |

#### Pickled Vegetable（`PickledVeg`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | pickle vegetable（`PickleVegetable`） | RawPotatoes（RawPotatoes） 或 RawCorn（RawCorn） 或 Rawbean（Bean Pod） 或 RawTomatoes（Tomatoes） 或 RawCarrots（Carrots） 或 Rawmushroom（Raw Champignon） 或 RawAlgae（Algae） ×3 营养值；Salt（salt） ×0.02 营养值 | PickledVeg（Pickled Vegetable）×15 | 工作量=1000；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B1；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### pizza（`Pizza`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake pizza（`MakePizza`） | Flour（flour） ×0.1 营养值；Meat_Muffalo（Meat_Muffalo） 或 MetalCannedMeat_prime（Canned prime meat） 或 [FungusPlantRaw] ×0.6 营养值；Cheese（cheese） 或 AgedCheese（aged cheese） ×0.3 营养值；RawTomatoes（Tomatoes） ×0.6 营养值 | Pizza（pizza）×3 | 工作量=1500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_C1；技能: Cooking≥15；建筑: ElectricOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |

#### Potatoes With Cheese（`PotatoesCheese`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Grill potatoes with cheese（`MakePotatoesCheese`） | Cheese（cheese） 或 AgedCheese（aged cheese） ×0.4 营养值；RawPotatoes（RawPotatoes） ×0.4 营养值 | PotatoesCheese（Potatoes With Cheese）×3 | 工作量=450；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B3；技能: Cooking≥2；建筑: ElectricStove, FueledStove, TableGrill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |
| 工作台配方 | Grill potatoes with cheese (x4)（`MakePotatoesCheeseBulk`） | Cheese（cheese） 或 AgedCheese（aged cheese） ×2 营养值；RawPotatoes（RawPotatoes） ×2 营养值 | PotatoesCheese（Potatoes With Cheese）×12 | 工作量=1200；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥4；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Special.xml` |

#### Processed Cat Food（`CatFood`）

- 分类：`Foods/Animalfood`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make cat food（`CookCatFood`） | [MeatRaw] 或 [AnimalProductRaw] ×0.5 营养值；[PlantFoodRaw] ×0.5 营养值 | CatFood（Processed Cat Food）×20 | 工作量=100；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Animal_A2；技能: Cooking≥0；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Livestock.xml` |

#### roasted coffee beans（`RoastedCoffeeBeans`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make roasted coffee beans（`RoastingCoffeeBeans`） | CoffeeBeans（coffee beans） ×5 | RoastedCoffeeBeans（roasted coffee beans）×5 | 工作量=300；速度属性=CookSpeed；工作技能=Cooking | 研究: Pemmican；技能: Cooking≥6；建筑: Campfire, ElectricOven, TableOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Roasted Meat（`RoastedMeat`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook roasted meat（`CookRoastedMeat`） | [MeatRaw] ×0.4 营养值 | RoastedMeat（Roasted Meat）×1 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_00；技能: Cooking≥0；建筑: Campfire, ElectricStove, FueledStove, OpenHearth, TableGrill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |
| 工作台配方 | cook roasted meat (x4)（`CookRoastedMeatBulk`） | [MeatRaw] ×2 营养值 | RoastedMeat（Roasted Meat）×4 | 工作量=1350；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥1；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |

#### Rum（`Alcohol_Rum`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make rum（`Make_Rum`） | rummash（Rum Mash） ×35 | Alcohol_Rum（Rum）×20 | 处理天数=4.5；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=25.0~45.0；理想温度=30.0~38.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty rum crate（`EmptyCrate_rum`） | Crate_Rum（crate of rum） ×1 | Alcohol_Rum（Rum）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Rum Mash（`rummash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew rum mash（`Makerummash`） | Rawsugarcane（Sugarcane） ×25；Yeast（Yeast） ×10 | rummash（Rum Mash）×10 | 工作量=650；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥10；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Sake（`Alcohol_Sake`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make sake（`Make_Sake`） | sakemash（Saké Mash） ×30 | Alcohol_Sake（Sake）×20 | 处理天数=5.25；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=2.0~20.0；理想温度=6.0~15.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty sake crate（`EmptyCrate_saki`） | Crate_Sake（crate of sake） ×1 | Alcohol_Sake（Sake）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Saké Mash（`sakemash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew saké mash（`Makesakemash`） | RawRice（RawRice） ×25；Yeast（Yeast） ×10 | sakemash（Saké Mash）×10 | 工作量=800；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥12；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### salted meat（`SaltedMeat`）

- 分类：`Foods/Preserves`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | salt meat（`SaltMeat`） | [MeatRaw] ×25；Salt（salt） ×5 | SaltedMeat（salted meat）×25 | 工作量=1000；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_0；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Canned.xml` |

#### Scramble（`ScrambledEggs`）

- 分类：`FoodMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook scramble（`CookScrambledEggs`） | Tofu（tofu） 或 [EggsUnfertilized] 或 [EggsFertilized] ×0.5 营养值 | ScrambledEggs（Scramble）×1 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_00；技能: Cooking≥0；建筑: Campfire, ElectricStove, FueledStove, OpenHearth, TableGrill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |
| 工作台配方 | cook scrambled eggs (x4)（`CookScrambledEggsBulk`） | Tofu（tofu） 或 [EggsUnfertilized] 或 [EggsFertilized] ×2.5 营养值 | ScrambledEggs（Scramble）×4 | 工作量=1350；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: PackagedSurvivalMeal；技能: Cooking≥0；建筑: ElectricStove_Pro | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals.xml` |

#### Silage（`Silage`）

- 分类：`Foods/Animalfood`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make silage（`Make_Silage`） | Hay（Hay） 或 RawCorn（RawCorn） 或 RawRice（RawRice） 或 Rawwheat（Wheat） ×150 | Silage（Silage）×50 | 处理天数=20；低于安全温度速度=0.05；高于安全温度速度=0.8；安全温度=-273~1000；理想温度=20~50；允许混合材料=true | 建筑: SilagePile | — | — | `Mods/Core_SK/Defs/RecipeDefs_UF/Recipes_SilagePile.xml` |
| 工作台配方 | make silage（`MakeSilage`） | Rawwheat（Wheat） 或 RawCorn（RawCorn） 或 RawRice（RawRice） ×1 营养值；Hay（Hay） ×3 营养值 | Silage（Silage）×30 | 工作量=300；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Animal_A2；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Livestock.xml` |

#### soy milk（`SoyMilk`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | press soy milk（`Makesoymilk`） | Rawbean（Bean Pod） ×10 | SoyMilk（soy milk）×4 | 工作量=700；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Pemmican；技能: Cooking≥2；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### sugar（`Sugar`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make sugar（`Grind_Sugar`） | Rawsugarcane（Sugarcane） ×20 | Sugar（sugar）×20 | 处理天数=0.3 | 研究: Bakery_B2；建筑: SK_Windmill | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |
| 工作台配方 | grind sugarcane（`MakeSugar`） | Rawsugarcane（Sugarcane） ×5 | Sugar（sugar）×5 | 工作量=500；速度属性=CookSpeed；工作技能=Cooking | 研究: Bakery_B2；技能: Cooking≥2；建筑: GrindStone, MillElectric | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |
| 工作台配方 | melting sugary substances（`MeltSugar`） | Honey（honey） 或 InsectJelly（InsectJelly） ×5 | Sugar（sugar）×5 | 工作量=1000；速度属性=CookSpeed；工作技能=Cooking | 研究: Bakery_B2；技能: Cooking≥1；建筑: ElectricOven, TableOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Sugarcane（`Rawsugarcane`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Sugarcane Plant（`Plant_Sugarcane`） | — | Rawsugarcane（Sugarcane）×9 | 收获量=9；生长天数=5；最低肥力=0.1；肥力敏感度=1.1 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### sweet bun（`SweetBun`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | bake sweet rolls（`MakeSweetBun`） | Sugar（sugar） ×0.4 营养值；Flour（flour） ×0.5 营养值 | SweetBun（sweet bun）×5 | 工作量=800；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_C1；技能: Cooking≥2；建筑: ElectricOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |

#### taffy（`Taffy`）

- 分类：`Foods/SweetMeals`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | cook taffy candies（`MakeTaffy`） | Sugar（sugar） ×0.5 营养值 | Taffy（taffy）×5 | 工作量=900；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Bakery_C1；技能: Cooking≥10；建筑: CandyTable, ElectricOven | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Bakery.xml` |

#### Tea Leaves（`RawTea`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Tea Shrub（`Plant_Tea`） | — | RawTea（Tea Leaves）×10 | 收获量=10；生长天数=7；最低肥力=1.1；肥力敏感度=1.2 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Tequila（`Alcohol_Tequila`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make tequila（`Make_Tequila`） | tequilamash（Tequila Mash） ×40 | Alcohol_Tequila（Tequila）×20 | 处理天数=5.6；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=18.0~45.0；理想温度=20.0~30.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty tequila crate（`EmptyCrate_tequila`） | Crate_Tequila（crate of tequila） ×1 | Alcohol_Tequila（Tequila）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Tequila Mash（`tequilamash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew tequila mash（`Maketequilamash`） | RawAgave（RawAgave） ×35；Yeast（Yeast） ×5 | tequilamash（Tequila Mash）×10 | 工作量=850；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥12；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Tobacco Leaves（`RawTobacco`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Tobacco Plant（`Plant_Tobacco`） | — | RawTobacco（Tobacco Leaves）×7 | 收获量=7；生长天数=6；最低肥力=0.8；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### tofu（`Tofu`）

- 分类：`FoodRaw/MeatSubRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | press tofu（`MakeTofu`） | Rawbean（Bean Pod） ×1.6 营养值 | Tofu（tofu）×5 | 工作量=600；速度属性=CookSpeed；工作技能=Cooking；允许混合材料=true | 研究: Food_B1；技能: Cooking≥6；建筑: TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Ingridients.xml` |

#### Vodka（`Alcohol_Vodka`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make vodka（`Make_Vodka`） | vodkamash（Vodka Mash） ×35 | Alcohol_Vodka（Vodka）×20 | 处理天数=4.8；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=40.0~65.0；理想温度=45.0~55.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty vodka crate（`EmptyCrate_vodka`） | Crate_Vodka（crate of vodka） ×1 | Alcohol_Vodka（Vodka）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Vodka Mash（`vodkamash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew vodka mash（`Makevodkamash`） | RawPotatoes（RawPotatoes） ×35；Yeast（Yeast） ×5 | vodkamash（Vodka Mash）×10 | 工作量=600；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥5；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Wheat（`Rawwheat`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Wheat Grass（`Plantwheat`） | — | Rawwheat（Wheat）×18 | 收获量=18；生长天数=5.2；最低肥力=0.6；肥力敏感度=1.4 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Whiskey（`Alcohol_Whisky`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make whisky（`Make_Whisky`） | grainmash（Grain Mash） ×50 | Alcohol_Whisky（Whiskey）×20 | 处理天数=7.25；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=2.0~35.0；理想温度=3.0~24.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty whiskey crate（`EmptyCrate_Whiskey`） | Crate_Whiskey（crate of whiskey） ×1 | Alcohol_Whisky（Whiskey）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Wild Rose（`WildRose`）

- 分类：`Foods/LuxuryStuffs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | wild rose flowers（`Plant_WildRose`） | — | WildRose（Wild Rose）×6 | 收获量=6；生长天数=8；播种工作量=150；最低肥力=1.2；肥力敏感度=1.3 | — | 播种标签: Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Wild.xml` |

#### Wine（`Alcohol_Wine`）

- 分类：`Foods/Alcohol/Alcohol_Bottles`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 发酵/缓处理配方 | make wine（`Make_Wine`） | winemash（Wine Mash） ×35 | Alcohol_Wine（Wine）×20 | 处理天数=5.8；每度每小时损坏=2.5；低于安全温度速度=0.1；高于安全温度速度=1.0；安全温度=15.0~38.0；理想温度=20.0~30.0 | 建筑: UniversalFermenter | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_UF_Alcohol.xml` |
| 工作台配方 | empty wine crate（`EmptyCrate_Wine`） | Crate_Wine（crate of wine） ×1 | Alcohol_Wine（Wine）×75 | 工作量=50；速度属性=DrugCookingSpeed；工作技能=Cooking | 研究: Brewing；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_AlcoholBarrel.xml` |

#### Wine Mash（`winemash`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Brew wine mash（`Makewinemash`） | Rawgrape（Grapes） ×35；Yeast（Yeast） ×5 | winemash（Wine Mash）×10 | 工作量=850；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Alcohol_B2；技能: Cooking≥14；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

#### Yeast（`Yeast`）

- 分类：`Foods/CookingSupplies`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Extract yeast（`Makeyeast`） | [FungusPlantRaw] ×5 | Yeast（Yeast）×15 | 工作量=800；速度属性=DrugCookingSpeed；工作技能=Cooking；允许混合材料=true | 研究: Brewing；技能: Cooking≥6；建筑: Brewery, ElectricBrewery | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Alcohol.xml` |

### 2.5 医疗

#### Activated carbon（`ActivatedCarbon`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make activated carbon（`MakeActivatedCarbon`） | Charcoal（Charcoal） 或 Coal（Coal Ore） ×5 | ActivatedCarbon（Activated carbon）×9 | 工作量=780；速度属性=DrugSynthesisSpeed；工作技能=Medicine；允许混合材料=true | 研究: DrugProduction；技能: Medicine≥3, Crafting≥5；建筑: DrugLab, Kiln, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Drug.xml` |

#### Aloe Leaves（`aloe`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Aloe Plant（`Plant_Aloe`） | — | aloe（Aloe Leaves）×4 | 收获量=4；生长天数=4.7；最低肥力=0.3；肥力敏感度=1.4 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Medical.xml` |

#### Antibiotics（`MedicalDrink`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make Antibiotics（`BrewMedicalDrink`） | MedicineHerbal（Raw herbal medicine） ×1；RawHops（RawHops） 或 Yeast（Yeast） ×10 | MedicalDrink（Antibiotics）×1 | 工作量=1000；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: PenoxycylineProduction；技能: Medicine≥10；建筑: meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Medicals.xml` |

#### antidote（`AntidoteSafe`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make antidote（`MakeAntidoteSafe`） | Neutroamine（Neutroamine） ×1；PoisonGland（venom gland） ×1 | AntidoteSafe（antidote）×1 | 工作量=900；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: DrugProduction；技能: Medicine≥7；建筑: DrugLab, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Antidote.xml` |

#### Bandage Kit（`Bandagekit`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft bandage kit（`MakeBandageKit`） | [BTextiles] ×10 | Bandagekit（Bandage Kit）×1 | 工作量=250；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_A2；技能: Medicine≥2, Crafting≥2；建筑: ElectricTailoringBench, HandTailoringBench, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |
| 工作台配方 | craft bandage kit bulk（`MakeBandageKitBulk`） | [BTextiles] ×30 | Bandagekit（Bandage Kit）×3 | 工作量=750；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_A2；技能: Medicine≥2, Crafting≥2；建筑: ElectricTailoringBench, HandTailoringBench, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |

#### basic medicine kit（`Herbmedicine`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft basic medicine kit (aloe)（`MakeHerbmedicine`） | aloe（Aloe Leaves） ×5；Bandagekit（Bandage Kit） ×1 | Herbmedicine（basic medicine kit）×1 | 工作量=500；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_B2；技能: Medicine≥4, Crafting≥2；建筑: DrugLab, TablePrimitiveProsthetic | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |
| 工作台配方 | craft basic medicine kit (Mixed Herbs)（`MakeHerbmedicineMix`） | WildRose（Wild Rose） ×1；Hypericum（Hypericum） ×1；MintLeaves（Mint Leaves） 或 aloe（Aloe Leaves） ×1；Bandagekit（Bandage Kit） ×1 | Herbmedicine（basic medicine kit）×1 | 工作量=500；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_B2；技能: Medicine≥4, Crafting≥2；建筑: DrugLab, TablePrimitiveProsthetic | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |

#### Crude Bandage Kit（`Bandagekitcrude`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft crude bandage kit（`MakeBandagekitcrude`） | [BTextiles] ×7 | Bandagekitcrude（Crude Bandage Kit）×1 | 工作量=150；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_A1；技能: Medicine≥1, Crafting≥1；建筑: CraftingSpot, ElectricTailoringBench, HandTailoringBench, LeatherworkerTable, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |
| 工作台配方 | craft crude bandage kit bulk（`MakeBandagekitcrudeBulk`） | [BTextiles] ×21 | Bandagekitcrude（Crude Bandage Kit）×3 | 工作量=450；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_A1；技能: Medicine≥1, Crafting≥1；建筑: CraftingSpot, ElectricTailoringBench, HandTailoringBench, LeatherworkerTable, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |

#### gleamcap stem（`GleamcapStem`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | gleamcap（`Agarilux`） | — | GleamcapStem（gleamcap stem）×4 | 收获量=4；生长天数=9；播种工作量=150；收获工作量=100；最低肥力=0.01；肥力敏感度=2 | — | 播种标签: Fungiponics | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml` |

#### healroot tincture（`HealrootTincture`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | brew healroot tincture（`MakeTinctureHealroot`） | MedicineHerbal（Raw herbal medicine） ×3；Ash（ash） ×3 | HealrootTincture（healroot tincture）×1 | 工作量=400；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: Medicine_A1；技能: Medicine≥3；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Medicals.xml` |

#### Herbal Medicine（`HerbMedicine`）

- 分类：`Medicine`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | craft herbal medicine（`MakeHerbMedicine`） | MedicineHerbal（Raw herbal medicine） ×8；Ash（ash） ×2 | HerbMedicine（Herbal Medicine）×1 | 工作量=500；速度属性=DrugSynthesisSpeed；工作技能=Medicine；技能经验系数=0.8 | 研究: Medicine_B1；技能: Medicine≥5；建筑: ButcherSpot, TableButcher, TableKitchen, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Medicine.xml` |

#### Herbal Tea（`HerbalTea`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make herbal tea（`MakeHerbalTea`） | WildRose（Wild Rose） ×2；Hypericum（Hypericum） ×1；MintLeaves（Mint Leaves） ×2 | HerbalTea（Herbal Tea）×6 | 工作量=780；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: DrugProduction；技能: Medicine≥3, Cooking≥5；建筑: Brewery, DrugLab, ElectricBrewery, meditable | — | — | `Mods/Core_SK/Defs/Drugs/HerbalTea.xml` |

#### mushroom tincture（`MushroomTincture`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | brew mushroom tincture（`MakeTincture`） | GleamcapStem（gleamcap stem） ×2 | MushroomTincture（mushroom tincture）×1 | 工作量=400；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: Medicine_A1；技能: Medicine≥1；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Medicals.xml` |
| 工作台配方 | brew bottles of mushroom tincture（`MakeTinctureBulk`） | GleamcapStem（gleamcap stem） ×10 | MushroomTincture（mushroom tincture）×4 | 工作量=800；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: Medicine_A2；技能: Medicine≥6；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Medicals.xml` |

#### Neutroamine（`Neutroamine`）

- 分类：`Manufactured/MedicineStuff/MedicalMaterials`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make natural neutroamine（`MakeNaturalNeutroamine`） | NeutroPetals（Neutro Flower Petals） ×8；Hypericum（Hypericum） ×2 | Neutroamine（Neutroamine）×1 | 工作量=700；速度属性=DrugSynthesisSpeed；工作技能=Medicine | 研究: Drugs_C2；技能: Medicine≥6, Crafting≥3；建筑: DrugLab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Drug.xml` |

#### Painstopper（`VG_Ibuprofen`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make four Ibuprofen (x4)（`VG_BulkIbuprofen`） | Neutroamine（Neutroamine） ×4 | VG_Ibuprofen（Painstopper）×4 | 工作量=720；速度属性=DrugSynthesisSpeed；工作技能=Medicine | 研究: PenoxycylineProduction；技能: Medicine≥10；建筑: DrugLab | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Drug.xml` |

#### primitive antidote（`AntidoteSimple`）

- 分类：`Manufactured/MedicineStuff/MedicalDrugs`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Make primitive antidote（`MakeAntidoteSimple`） | MedicineHerbal（Raw herbal medicine） ×1；PoisonGland（venom gland） ×1 | AntidoteSimple（primitive antidote）×1 | 工作量=500；速度属性=DrugCookingSpeed；工作技能=Medicine；允许混合材料=true | 研究: Medicine_A2；技能: Medicine≥1；建筑: DrugLab, TableButcher, TableKitchen, meditable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Antidote.xml` |

#### unrefined synchronisity（`Synchronite`）

- 分类：`Manufactured/MedicineStuff/MedicalMaterials`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物挤奶 | arctic desiri（`Animal_Desiri`） | — | Synchronite（unrefined synchronisity）×1 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 动物挤奶 | desert desiri（`Animal_Desiri_Desert`） | — | Synchronite（unrefined synchronisity）×1 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 动物挤奶 | temperate desiri（`Animal_Desiri_TemperateOne`） | — | Synchronite（unrefined synchronisity）×1 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 动物挤奶 | temperate desiri（`Animal_Desiri_TemperateTwo`） | — | Synchronite（unrefined synchronisity）×1 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 动物挤奶 | urban desiri（`Animal_Desiri_Urban`） | — | Synchronite（unrefined synchronisity）×1 | 间隔天数=1 | — | 组件=CompProperties_Milkable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |

### 2.7 其他资源

#### Algae（`RawAlgae`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Algae（`PlantAlgae`） | — | RawAlgae（Algae）×5 | 收获量=5；生长天数=4；最低肥力=0.01；肥力敏感度=0.1 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Water.xml` |

#### Apples（`Rawapple`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Apple Tree（`Plant_TreeApple`） | — | Rawapple（Apples）×32 | 收获量=32；生长天数=18.6；播种工作量=650；收获工作量=800；最低肥力=0.6；肥力敏感度=0.5 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### Banana（`Rawbanana`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Banana Tree（`Plant_TreeBanana`） | — | Rawbanana（Banana）×30 | 收获量=30；生长天数=14；播种工作量=650；收获工作量=800；最低肥力=0.15；肥力敏感度=0.95 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### Bean Pod（`Rawbean`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Bean Stalk（`Plantbean`） | — | Rawbean（Bean Pod）×20 | 收获量=20；生长天数=9.2；最低肥力=1；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### Behir Egg (Fertilized)（`EggBehirFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | Behir（`Behir`） | — | EggBehirFertilized（Behir Egg (Fertilized)）×1 | 间隔天数=25；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Behir Egg (Unfertilized)（`EggBehirUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | Behir（`Behir`） | — | EggBehirUnfertilized（Behir Egg (Unfertilized)）×1 | 间隔天数=25；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Caoutchouc（`RawCaoutchouc`）

- 分类：`PlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Hevea Tree（`Plant_TreeHevea`） | — | RawCaoutchouc（Caoutchouc）×20 | 收获量=20；生长天数=6.5；播种工作量=750；收获工作量=700；最低肥力=0.45；肥力敏感度=0.6 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Carrots（`RawCarrots`）

- 分类：`PlantFoodRaw/ExtraPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Carrot Plant（`PlantCarrot`） | — | RawCarrots（Carrots）×9 | 收获量=9；生长天数=5.4；最低肥力=0.9；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### Coconut（`RawCoconut`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_TreePalm（`Plant_TreePalm`） | — | RawCoconut（Coconut）×22 | 收获量=22；生长天数=12；收获工作量=550；最低肥力=0.03；肥力敏感度=0.25 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Trees.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Trees.xml` |

#### Fenix Egg (Fertilized)（`EggFenixFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | Feenix（`Feenix`） | — | EggFenixFertilized（Fenix Egg (Fertilized)）×1 | 间隔天数=24；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Fenix Egg (Unfertilized)（`EggFenixUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | Feenix（`Feenix`） | — | EggFenixUnfertilized（Fenix Egg (Unfertilized)）×1 | 间隔天数=24；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### giant leaf（`RawGiantLeaf`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | giant leaf（`Plant_GiantLeaf`） | — | RawGiantLeaf（giant leaf）×5 | 收获量=5；生长天数=6；播种工作量=150；收获工作量=200；最低肥力=0.3；肥力敏感度=0.9 | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Special.xml` |

#### glowbulb（`RawGlowbulb`）

- 分类：`PlantFoodRaw/FungusPlantRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | glowbulb（`Glowstool`） | — | RawGlowbulb（glowbulb）×5 | 收获量=5；生长天数=12；播种工作量=150；收获工作量=50；最低肥力=0.01；肥力敏感度=2 | — | 播种标签: Fungiponics | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml` |

#### Grapes（`Rawgrape`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | grape vine（`Plant_TreeGrape`） | — | Rawgrape（Grapes）×16 | 收获量=16；生长天数=11；播种工作量=150；收获工作量=150；最低肥力=1.2；肥力敏感度=0.7 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### Green wyvern Egg (Fertilized)（`EggGreenWyvernFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | green wyvern（`GreenWyvern`） | — | EggGreenWyvernFertilized（Green wyvern Egg (Fertilized)）×1 | 间隔天数=23；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Green wyvern Egg (Unfertilized)（`EggGreenWyvernUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | green wyvern（`GreenWyvern`） | — | EggGreenWyvernUnfertilized（Green wyvern Egg (Unfertilized)）×1 | 间隔天数=23；数量范围=1 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Jade（`Jade`）

- 分类：`li=PRS`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Jade（`MineableJade`） | — | Jade（Jade）×75 | 单格产量=75；散布常见度=0.25；矿团规模=7~12；耐久=6000 | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml` |

#### Lacosdile Egg (Fertilized)（`EggLacosdileFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | lacosdile（`Lacosdile`） | — | EggLacosdileFertilized（Lacosdile Egg (Fertilized)）×5~7 | 间隔天数=15；数量范围=5~7 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Tropical.xml` |

#### Lacosdile Egg (Unfertilized)（`EggLacosdileUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | lacosdile（`Lacosdile`） | — | EggLacosdileUnfertilized（Lacosdile Egg (Unfertilized)）×5~7 | 间隔天数=15；数量范围=5~7 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Tropical.xml` |

#### Neutro Flower Petals（`NeutroPetals`）

- 分类：`PlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Neutro Flower（`Plant_NeutroFlower`） | — | NeutroPetals（Neutro Flower Petals）×5 | 收获量=5；生长天数=6.2；肥力敏感度=1 | — | 播种标签: Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Medical.xml` |

#### Nightling egg (Fertilized)（`EggNightlingFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | nightling（`Nightling`） | — | EggNightlingFertilized（Nightling egg (Fertilized)）×1~2 | 间隔天数=10；数量范围=1~2 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_WildCanines.xml` |

#### Nightling egg (Unfertilized)（`EggNightlingUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | nightling（`Nightling`） | — | EggNightlingUnfertilized（Nightling egg (Unfertilized)）×1~2 | 间隔天数=10；数量范围=1~2 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_WildCanines.xml` |

#### Orange（`Raworange`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Orange Tree（`Plant_TreeOrange`） | — | Raworange（Orange）×40 | 收获量=40；生长天数=13；播种工作量=650；收获工作量=800；最低肥力=0.6；肥力敏感度=0.8 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### Peach（`Rawpeach`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | peach tree（`Plant_TreePeach`） | — | Rawpeach（Peach）×35 | 收获量=35；生长天数=18；播种工作量=650；收获工作量=800；最低肥力=0.6；肥力敏感度=0.85 | — | 播种标签: Ground | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

#### ptarmigan egg (Fertilized)（`EggPtarmiganFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | Ptarmigan（`Ptarmigan`） | — | EggPtarmiganFertilized（ptarmigan egg (Fertilized)）×1~5 | 间隔天数=5；数量范围=1~5 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Ptarmigan.xml` |

#### ptarmigan egg (Unfertilized)（`EggPtarmiganUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | Ptarmigan（`Ptarmigan`） | — | EggPtarmiganUnfertilized（ptarmigan egg (Unfertilized)）×1~5 | 间隔天数=5；数量范围=1~5 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Ptarmigan.xml` |

#### Raptor Egg (Fertilized)（`EggFRaptorFertilized`）

- 分类：`EggsFertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | feathered raptor（`FRaptor`） | — | EggFRaptorFertilized（Raptor Egg (Fertilized)）×1~2 | 间隔天数=14；数量范围=1~2 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Raptor Egg (Unfertilized)（`EggFRaptorUnfertilized`）

- 分类：`EggsUnfertilized`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | feathered raptor（`FRaptor`） | — | EggFRaptorUnfertilized（Raptor Egg (Unfertilized)）×1~2 | 间隔天数=14；数量范围=1~2 | — | 组件=CompProperties_EggLayer | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |

#### Raw Champignon（`Rawmushroom`）

- 分类：`PlantFoodRaw/FungusPlantRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Champignon Mushroom（`Plant_Mushroom`） | — | Rawmushroom（Raw Champignon）×12 | 收获量=12；生长天数=5.1；最低肥力=0.9；肥力敏感度=0.7 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### Raw Cotton（`RawCotton`）

- 分类：`PlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Cotton Bush（`Plant_Cotton`） | — | RawCotton（Raw Cotton）×8 | 收获量=8；生长天数=5.6；最低肥力=0.5；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Raw Devilstrand（`RawDevilstrand`）

- 分类：`PlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Devilstrand Mushroom（`Plant_Devilstrand`） | — | RawDevilstrand（Raw Devilstrand）×8 | 收获量=8；生长天数=15；最低肥力=1.1；肥力敏感度=0.4 | — | 播种标签: Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Raw flax（`RawFlax`）

- 分类：`PlantMatter`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Flax flower（`Plant_Flax`） | — | RawFlax（Raw flax）×9 | 收获量=9；生长天数=8.1；肥力敏感度=0.4 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### RawCorn（`RawCorn`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Corn Plant（`Plant_Corn`） | — | RawCorn（RawCorn）×30 | 收获量=30；生长天数=9；最低肥力=0.4；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### RawPotatoes（`RawPotatoes`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Potato Plant（`Plant_Potato`） | — | RawPotatoes（RawPotatoes）×13 | 收获量=13；生长天数=5；最低肥力=0.5；肥力敏感度=0.4 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### RawRice（`RawRice`）

- 分类：`PlantFoodRaw/BasicPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Rice Plant（`Plant_Rice`） | — | RawRice（RawRice）×16 | 收获量=16；生长天数=4.6；最低肥力=0.5；肥力敏感度=1 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### shimmershroom（`RawShimmershroom`）

- 分类：`PlantFoodRaw/FungusPlantRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | shimmershroom（`Bryolux`） | — | RawShimmershroom（shimmershroom）×5 | 收获量=5；生长天数=18；播种工作量=150；收获工作量=50；最低肥力=0.01；肥力敏感度=2 | — | 播种标签: Fungiponics | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml` |

#### soylent green（`soylentgreen`）

- 分类：`MeatRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | obtain soylent green（`Make_SoylentGreen`） | [CorpsesHumanlike] ×1 | soylentgreen（soylent green）×45 | 工作量=1400；速度属性=CookSpeed；工作技能=Crafting；技能经验系数=0.8 | 研究: NutrientPaste；技能: Crafting≥12；建筑: soylenttable | — | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Extracts.xml` |

#### Tomatoes（`RawTomatoes`）

- 分类：`PlantFoodRaw/ExtraPlantFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Tomato Vine（`PlantTomato`） | — | RawTomatoes（Tomatoes）×10 | 收获量=10；生长天数=3.7；最低肥力=1.3；肥力敏感度=1.2 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml` |

#### Watermelon（`RawWatermelon`）

- 分类：`PlantFoodRaw/FruitFoodRaw`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | watermelon plant（`Plant_Watermelon`） | — | RawWatermelon（Watermelon）×5 | 收获量=5；生长天数=6.2；收获工作量=160；最低肥力=0.5；肥力敏感度=0.6 | — | 播种标签: Ground, Hydroponic | — | `Mods/Core_SK/Defs/ThingDefs_Plants/Plants_Cultivated_Fruits.xml` |

### 2.8 未分类

#### Bioferrite（`Bioferrite`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_TreeHarbinger（`Plant_TreeHarbinger`） | — | Bioferrite（Bioferrite）×按规则 | — | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK_DLCModule/Anomaly/Patches/ThingDefs_Plants/Plants_Special_Anomaly.xml; PatchOperationConditional | `Mods/Core_SK_DLCModule/Anomaly/Patches/ThingDefs_Plants/Plants_Special_Anomaly.xml` |

#### Chitin（`Chitin`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物剪毛 | Crashbug（`Crashbug`） | — | Chitin（Chitin）×7 | 间隔天数=5 | — | 组件=SK.CompProperties_ShearableRenameable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 动物剪毛 | Megascarab（`Megascarab`） | — | Chitin（Chitin）×30 | 间隔天数=5 | — | 组件=SK.CompProperties_ShearableRenameable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml` |
| 动物剪毛 | Megaspider（`Megaspider`） | — | Chitin（Chitin）×60 | 间隔天数=5 | — | 组件=SK.CompProperties_ShearableRenameable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml` |
| 动物剪毛 | Scorpion（`Scorpion`） | — | Chitin（Chitin）×125 | 间隔天数=8 | — | 组件=SK.CompProperties_ShearableRenameable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Insects.xml` |
| 动物剪毛 | Spelopede（`Spelopede`） | — | Chitin（Chitin）×45 | 间隔天数=5 | — | 组件=SK.CompProperties_ShearableRenameable | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml` |
| 动物剪毛 | terramorph（`Terramorph`） | — | Chitin（Chitin）×80 | 间隔天数=5 | — | 组件=SK.CompProperties_ShearableRenameable | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Cold.xml` |

#### Granite Chunk（`ChunkGranite`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Granite Meteorite（`GraniteBoulder`） | — | ChunkGranite（Granite Chunk）×1 | 单格产量=1；耐久=2000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### Limestone Chunk（`ChunkLimestone`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Limestone Meteorite（`LimestoneBoulder`） | — | ChunkLimestone（Limestone Chunk）×1 | 单格产量=1；耐久=1000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### Marble Chunk（`ChunkMarble`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Marble Meteorite（`MarbleBoulder`） | — | ChunkMarble（Marble Chunk）×1 | 单格产量=1；耐久=1500 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### PsychoidLeaves（`PsychoidLeaves`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_Psychoid（`Plant_Psychoid`） | — | PsychoidLeaves（PsychoidLeaves）×10 | 收获量=10；生长天数=6.6；最低肥力=0.50；肥力敏感度=0.4 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### RawHops（`RawHops`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Hops Plant（`Plant_Hops`） | — | RawHops（RawHops）×8 | 收获量=8；生长天数=5.4；最低肥力=0.3；肥力敏感度=0.6 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

#### Sandstone Chunk（`ChunkSandstone`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Sandstone Meteorite（`SandstoneBoulder`） | — | ChunkSandstone（Sandstone Chunk）×1 | 单格产量=1；耐久=1000 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### Slate Chunk（`ChunkSlate`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 矿脉开采 | Slate Meteorite（`SlateBoulder`） | — | ChunkSlate（Slate Chunk）×1 | 单格产量=1；耐久=1500 | — | — | — | `Mods/Core_SK/Defs/ThingDefs/Events_Meteors.xml` |

#### SmokeleafLeaves（`SmokeleafLeaves`）

- 分类：`未分类资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 植物收获 | Plant_Smokeleaf（`Plant_Smokeleaf`） | — | SmokeleafLeaves（SmokeleafLeaves）×4 | 收获量=4；生长天数=10.8；收获工作量=200；最低肥力=0.2；肥力敏感度=1.5 | — | 播种标签: Ground, Hydroponic | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml` |

### 2.9 外部/原版

#### AIPersonaCore（`AIPersonaCore`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 摧毁残留 | DefoliatorShipPart（`DefoliatorShipPart`） | — | AIPersonaCore（AIPersonaCore）×1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | PsychicDronerShipPart（`PsychicDronerShipPart`） | — | AIPersonaCore（AIPersonaCore）×1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### Ammo_12Gauge_Buck（`Ammo_12Gauge_Buck`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | label=make 12 gauge (Buck) shell x40（`MakeAmmo_12Gauge_Buck`） | 未指定过滤器的材料 | Ammo_12Gauge_Buck（Ammo_12Gauge_Buck）×Ammo_12Gauge_Buck=40 | 工作量=workAmount=2500；速度属性=workSpeedStat=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥3 | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml` |

#### Ammo_30x29mmGrenade_HEDP（`Ammo_30x29mmGrenade_HEDP`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make 30x29mm HEDP grenades x20（`MakeAmmo_30x29mmGrenade_HEDP`） | [Metallic] ×40；Powder（Gunpowder） 或 FSX（FSX） ×8；[Chemical] ×5；ComponentIndustrial（ComponentIndustrial） ×4 | Ammo_30x29mmGrenade_HEDP（Ammo_30x29mmGrenade_HEDP）×20 | — | 研究: Range_C2 | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml` |

#### Ammo_30x29mmGrenade_Smoke（`Ammo_30x29mmGrenade_Smoke`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | MakeAmmo_30x29mmGrenade_Smoke（`MakeAmmo_30x29mmGrenade_Smoke`） | [Metallic] ×42；Plastic（Plastic） ×15；ComponentIndustrial（ComponentIndustrial） ×6 | Ammo_30x29mmGrenade_Smoke（Ammo_30x29mmGrenade_Smoke）×Ammo_30x29mmGrenade_Smoke=20 | — | 研究: Range_C2；技能: Crafting≥Crafting=12 | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml` |

#### Ammo_66mmThermalBolt_Incendiary（`Ammo_66mmThermalBolt_Incendiary`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make 66mm Thermal Bolt x20（`MakeAmmo_66mmThermalBolt_Incendiary`） | [Metallic] ×40；Powder（Gunpowder） 或 FSX（FSX） ×10；Napalm（Napalm） ×15 | Ammo_66mmThermalBolt_Incendiary（Ammo_66mmThermalBolt_Incendiary）×20 | — | — | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/66mmThermalBolt.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/66mmThermalBolt.xml` |

#### EggCobraFertilized（`EggCobraFertilized`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | Cobra（`Cobra`） | — | EggCobraFertilized（EggCobraFertilized）×1~3 | 间隔天数=8.5；数量范围=1~3 | — | 组件=CompProperties_EggLayer | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |

#### EggCobraUnfertilized（`EggCobraUnfertilized`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | Cobra（`Cobra`） | — | EggCobraUnfertilized（EggCobraUnfertilized）×1~3 | 间隔天数=8.5；数量范围=1~3 | — | 组件=CompProperties_EggLayer | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |

#### EggIguanaFertilized（`EggIguanaFertilized`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产受精蛋 | Iguana（`Iguana`） | — | EggIguanaFertilized（EggIguanaFertilized）×1~3 | 间隔天数=5.661；数量范围=1~3 | — | 组件=CompProperties_EggLayer | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |

#### EggIguanaUnfertilized（`EggIguanaUnfertilized`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 动物产未受精蛋 | Iguana（`Iguana`） | — | EggIguanaUnfertilized（EggIguanaUnfertilized）×1~3 | 间隔天数=5.661；数量范围=1~3 | — | 组件=CompProperties_EggLayer | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |

#### killedLeavings（`killedLeavings`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 摧毁残留 | Defoliator（`Defoliator`） | — | killedLeavings（killedLeavings）×Plasteel=50, DepletedUranium=35, CarbonAlloy=15, ComponentAdvanced=8, Electronics=4, ChunkSlagSteel=8 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | MechAssembler（`MechAssembler`） | — | killedLeavings（killedLeavings）×ChunkSlagSteel=4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | MechCapsule（`MechCapsule`） | — | killedLeavings（killedLeavings）×ChunkSlagSteel=3 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | ShieldGeneratorBullets（`ShieldGeneratorBullets`） | — | killedLeavings（killedLeavings）×Plasteel=10, DepletedUranium=10, CarbonAlloy=5, ComponentAdvanced=3, Electronics=1, ChunkSlagSteel=4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | ShieldGeneratorMortar（`ShieldGeneratorMortar`） | — | killedLeavings（killedLeavings）×Plasteel=10, DepletedUranium=10, CarbonAlloy=5, ComponentAdvanced=3, Electronics=1, ChunkSlagSteel=4 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | Spacedrone（`Spacedrone`） | — | killedLeavings（killedLeavings）×Plasteel=50, ChunkSlagSteel=4, ComponentAdvanced=3, Electronics=1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_DLCModule/Ideology/Patches/ThingDefs_Buildings/Buildings_Exotic.xml` |
| 摧毁残留 | Turret_AutoChargeBlaster（`Turret_AutoChargeBlaster`） | — | killedLeavings（killedLeavings）×Plasteel=35, DepletedUranium=10, CarbonAlloy=10, ComponentAdvanced=4, ChunkSlagSteel=3 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | Turret_AutoInferno（`Turret_AutoInferno`） | — | killedLeavings（killedLeavings）×Plasteel=40, DepletedUranium=20, CarbonAlloy=10, ComponentAdvanced=6, ChunkSlagSteel=3 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | Turret_AutoMiniTurret（`Turret_AutoMiniTurret`） | — | killedLeavings（killedLeavings）×ChunkSlagSteel=2 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |
| 摧毁残留 | Turret_AutoMortar（`Turret_AutoMortar`） | — | killedLeavings（killedLeavings）×Plasteel=40, DepletedUranium=20, CarbonAlloy=10, ComponentAdvanced=6, ChunkSlagSteel=3 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml | `Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml` |

#### Meat_Human（`Meat_Human`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | mince limbs（`MinceLimbs`） | [BodyPartsNatural] ×1 | Meat_Human（Meat_Human）×16 | 工作量=250；速度属性=ButcheryFleshSpeed；工作技能=Cooking | 研究: Food_0；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Cannibals.xml` |
| 工作台配方 | mince organs（`MinceOrgans`） | [BodyPartsNatural] ×1 | Meat_Human（Meat_Human）×8 | 工作量=150；速度属性=ButcheryFleshSpeed；工作技能=Cooking | 研究: Food_0；建筑: TableButcher, TableKitchen | — | — | `Mods/Core_SK/Defs/RecipeDefs_Meals/Recipes_Meals_Cannibals.xml` |

#### Meat_ThingDefFishSduiggles（`Meat_ThingDefFishSduiggles`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | process small fish（`ProcessSmallFish`） | [Fish] ×2 营养值 | Meat_ThingDefFishSduiggles（Meat_ThingDefFishSduiggles）×30 | 工作量=400；速度属性=CookSpeed；工作技能=Cooking；技能经验系数=0.8；允许混合材料=true | 研究: Fish_A1；技能: Cooking≥2；建筑: ButcherSpot, TableButcher, TableKitchen | — | — | `Mods/Core_SK_DLCModule/Odyssey/Defs/RecipeDefs/Recipes_Fish.xml` |

#### Pila（`Pila`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make pila x5（`MakeAmmo_Pilum`） | [Woody] ×10；[Metallic] ×5 | Pila（Pila）×5 | 工作量=2400；速度属性=GeneralLaborSpeed；工作技能=Crafting | 技能: Crafting≥3；建筑: CraftingSpot, ElectricSmithy, FueledSmithy | — | — | `Mods/Core_SK/Defs/CombatExtended/Ammo/Neolithic/Pillum.xml` |

#### Shell_Deadlife（`Shell_Deadlife`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make a 81mm deadlife shell x1（`MakeShell_Deadlife`） | [SLDBar] 或 [USLDBar] ×15；[HeavyBar] ×5；Powder（Gunpowder） 或 FSX（FSX） ×6；BioferriteAlloy（bioferrite alloy） ×25 | Shell_Deadlife（Shell_Deadlife）×Shell_Deadlife=2 | 工作量=1000；速度属性=workSpeedStat=SmithingSpeed | 研究: Range_C2, DeadlifeDust | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml; PatchApplied=Mods/Core_SK_DLCModule/Anomaly/Patches/CombatExtended/Ammo/SK_AnomalyAmmoPatch.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml` |

#### Shell_HighExplosive（`Shell_HighExplosive`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make 81mm HE mortar shells x1（`MakeShell_HighExplosive`） | [SLDBar] 或 [USLDBar] ×15；[HeavyBar] ×5；Powder（Gunpowder） 或 FSX（FSX） ×6；ComponentIndustrial（ComponentIndustrial） ×2 | Shell_HighExplosive（Shell_HighExplosive）×Shell_HighExplosive=1 | 工作量=1000；速度属性=workSpeedStat=SmithingSpeed | — | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml` |

#### Shell_HighExplosive_HFuzed（`Shell_HighExplosive_HFuzed`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make 81mm airburst mortar shells x1（`MakeShell_HighExplosive_HFuzed`） | [SLDBar] 或 [USLDBar] ×15；[HeavyBar] ×5；Powder（Gunpowder） 或 FSX（FSX） ×6；ComponentIndustrial（ComponentIndustrial） ×2；ElectronicComponents（Electronic Components） ×2 | Shell_HighExplosive_HFuzed（Shell_HighExplosive_HFuzed）×Shell_HighExplosive_HFuzed=1 | 工作量=1000；速度属性=workSpeedStat=SmithingSpeed | — | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml` |

#### Shell_Incendiary（`Shell_Incendiary`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make 81mm incendiary mortar shells x1（`MakeShell_Incendiary`） | [SLDBar] 或 [USLDBar] ×15；[HeavyBar] ×5；Powder（Gunpowder） 或 FSX（FSX） ×6；Prometheum（Prometheum） ×2 | Shell_Incendiary（Shell_Incendiary）×Shell_Incendiary=1 | 工作量=1000；速度属性=workSpeedStat=SmithingSpeed | — | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml` |

#### Shell_Toxic（`Shell_Toxic`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | make a 81mm tox mortar shell x1（`MakeShell_Toxic`） | [SLDBar] 或 [USLDBar] ×12；[HeavyBar] ×4；[Chemical] ×15；Powder（Gunpowder） 或 FSX（FSX） ×3；ComponentIndustrial（ComponentIndustrial） ×2 | Shell_Toxic（Shell_Toxic）×Shell_Toxic=1 | 工作量=1000；速度属性=workSpeedStat=SmithingSpeed | 研究: Range_C2, ToxGas | — | PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml` |

#### ShuttleEngine（`ShuttleEngine`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 摧毁残留 | PassengerShuttle（`PassengerShuttle`） | — | ShuttleEngine（ShuttleEngine）×1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml` |
| 摧毁残留 | shuttle（`Shuttle`） | — | ShuttleEngine（ShuttleEngine）×1 | — | — | 摧毁来源物后获得 | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml` |

#### Techprint_Blaster_weapons_Archotech（`Techprint_Blaster_weapons_Archotech`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Reverse Engineer Archotech Blaster（`ReverseEngineer_ArchotechBlaster`） | Gun_ArchotechBlaster_Prototype（Archotech Replica А-2 «Blaster»） ×1 | Techprint_Blaster_weapons_Archotech（Techprint_Blaster_weapons_Archotech）×1 | 工作量=50000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥15, Intellectual≥15, Artistic≥5；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_Weapons_Archotech.xml` |

#### Techprint_Bow_Archotech（`Techprint_Bow_Archotech`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Reverse Engineer Archotech Bow（`ReverseEngineer_ArchotechBow`） | Bow_Archotech_Prototype（Archotech Replica А-1 «Bow»） ×1 | Techprint_Bow_Archotech（Techprint_Bow_Archotech）×1 | 工作量=50000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥15, Intellectual≥15, Artistic≥5；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_Weapons_Archotech.xml` |

#### Techprint_Melee_Archotech（`Techprint_Melee_Archotech`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Reverse Engineer Archotech Sword（`ReverseEngineer_ArchotechSword`） | MeleeWeapon_Archotech1H_Prototype（Archotech Replica B-2 «Shortsword»） 或 MeleeWeapon_Archotech2H_Prototype（Archotech Replica B-1 «Broadsword»） ×1 | Techprint_Melee_Archotech（Techprint_Melee_Archotech）×1 | 工作量=50000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥15, Intellectual≥15, Artistic≥5；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_Weapons_Archotech.xml` |

#### Techprint_SMG_Archotech（`Techprint_SMG_Archotech`）

- 分类：`外部/原版资源`

| 途径 | 来源 | 输入材料 | 产出 | 关键数值 | 前置 | 备注 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | Reverse Engineer Archotech SMG（`ReverseEngineer_ArchotechSMG`） | Gun_ArchotechSMG_Prototype（Archotech Replica А-3 «SMG»） ×1 | Techprint_SMG_Archotech（Techprint_SMG_Archotech）×1 | 工作量=50000；速度属性=SmithingSpeed；工作技能=Crafting；技能经验系数=0.8 | 技能: Crafting≥15, Intellectual≥15, Artistic≥5；建筑: MechWeaponCraftingWorkTable | — | — | `Mods/Core_SK/Defs/RecipeDefs_Weapons/Recipes_Weapons_Archotech.xml` |

## 3. 动态产出与 XML 无法固定换算的路径

| 类型 | 来源 | 输入材料 | 动态产出 | 关键数值 | 前置 | 条件 | 源文件 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 工作台配方 | disassemble prosthesis（`ScrapProsthesis`） | [BasicProstheses] 或 [SimpleProstheses] 或 [OrganicOptimizer] 或 [SyntheticOrgans] 或 [BodyPartsBionic] 或 [BodyPartsUltra] 或 [CyberneticProstheses] ×1 | Smelted | 工作量=400；速度属性=GeneralLaborSpeed；工作技能=Crafting | Prosthesis_making_B1；TableBasicProsthetic；TableBionics；TablePrimitiveProsthetic；TableRecycling；TableSynthetics；UpgradingStation | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Disassemble.xml` |
| 工作台配方 | disassemble firearms（`DissasembleFirearms`） | [WeaponsRanged] ×1 | Smelted | 工作量=750；速度属性=SmeltingSpeed；工作技能=Crafting；技能经验系数=0.8 | Machining；Crafting≥6；TableRecycling | — | `Mods/Core_SK/Defs/RecipeDefs/Recipes_Disassemble.xml` |
| 工作台配方 | ExtractMetalFromSlag（`ExtractMetalFromSlag`） | — | Smelted | 工作量=1200；工作技能=Crafting；技能经验系数=0.8 | Metals_C1；Crafting≥4；EAF；ElectricSmelter | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Extracts.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Extracts.xml` |
| 屠宰肉类继承 | saberfish（`ThingDefSaberfish`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | redfin tuna（`ThingDefRedfinTuna`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | snackeral（`ThingDefFishMashgon`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | cuda（`ThingDefCuda`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | ice flounder（`ThingDefIceFlounder`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | eel（`ThingDefFishSeasnake`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | alligar（`ThingDefFishTailteeth`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | breadmouth bass（`ThingDefFishBlueblade`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | killer catfish（`ThingDefFishKillerCatfish`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | death sturgeon（`ThingDefDeathSturgeon`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | sterling perch（`ThingDefFishSterlingPerch`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | pinstripe bass（`ThingDefFishPinstripeBass`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | — | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| 屠宰肉类继承 | buffalo（`Buffalo`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Arid.xml` |
| 屠宰肉类继承 | panda bear（`PandaBear`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bears.xml` |
| 屠宰肉类继承 | White tiger（`WhiteTiger`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_BigCats.xml` |
| 屠宰肉类继承 | snow leopard（`SnowLeopard`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_BigCats.xml` |
| 屠宰肉类继承 | Jaguar（`Jaguar`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_BigCats.xml` |
| 屠宰肉类继承 | Mosquito（`Mosquito`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | Crashbug（`Crashbug`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | Fly（`Fly`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | Flea（`Flea`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | cockroach（`Cockroach`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | Cropeater（`CropEaterInsect`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Bugs.xml` |
| 屠宰肉类继承 | snork（`Snork`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Cold.xml` |
| 屠宰肉类继承 | terramorph（`Terramorph`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Cold.xml` |
| 屠宰肉类继承 | arctic desiri（`Animal_Desiri`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 屠宰肉类继承 | temperate desiri（`Animal_Desiri_TemperateOne`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 屠宰肉类继承 | temperate desiri（`Animal_Desiri_TemperateTwo`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 屠宰肉类继承 | urban desiri（`Animal_Desiri_Urban`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 屠宰肉类继承 | desert desiri（`Animal_Desiri_Desert`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Desiri.xml` |
| 屠宰肉类继承 | shepherd（`GermanShepherd`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_DogGroup.xml` |
| 屠宰肉类继承 | Feenix（`Feenix`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | green wyvern（`GreenWyvern`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | kirin（`Kirin`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | Behir（`Behir`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | feathered raptor（`FRaptor`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | barghest（`Barghest`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Exotic.xml` |
| 屠宰肉类继承 | mammoth（`Mammoth`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Giants.xml` |
| 屠宰肉类继承 | megabadger（`Megabadger`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Giants.xml` |
| 屠宰肉类继承 | woolly rhino（`Rhino_Wooly`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Giants.xml` |
| 屠宰肉类继承 | woolly horse（`Horse_Wooly`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Horse.xml` |
| 屠宰肉类继承 | Spotted Hyena（`SpottedHyena`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Hyena.xml` |
| 屠宰肉类继承 | Scorpion（`Scorpion`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Insects.xml` |
| 屠宰肉类继承 | fisher（`Fishercat`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Martens.xml` |
| 屠宰肉类继承 | ermine（`Ermine`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Martens.xml` |
| 屠宰肉类继承 | meerkat（`Meerkat`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Meerkat.xml` |
| 屠宰肉类继承 | Ptarmigan（`Ptarmigan`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Ptarmigan.xml` |
| 屠宰肉类继承 | chipmunk（`Chipmunk`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | sabretooth cat（`SmilodonPop`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_SmilodonPop.xml` |
| 屠宰肉类继承 | lacosdile（`Lacosdile`） | — | 肉类继承自 Muffalo | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Tropical.xml` |
| 屠宰肉类继承 | okapi（`Okapi`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Tropical.xml` |
| 屠宰肉类继承 | nightling（`Nightling`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_WildCanines.xml` |
| 屠宰肉类继承 | black wolf（`Rimwolf`） | — | 肉类继承自 Elephant | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Animal_Wolfs.xml` |
| 屠宰肉类继承 | antis（`Antis`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Insectoid.xml` |
| 屠宰肉类继承 | venetis（`Venetis`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Insectoid.xml` |
| 屠宰肉类继承 | gnawler（`Gnawler`） | — | 肉类继承自 Megaspider | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Insectoid.xml` |
| 屠宰肉类继承 | Mutant（`Mutant_Ogre`） | — | 肉类继承自 Human | — | — | — | `Mods/Core_SK/Defs/ThingDefs_Races/Races_Mutant.xml` |
| 屠宰肉类继承 | Megasloth（`Megasloth`） | — | 肉类继承自 Elephant | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Giant.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Giants.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Giant.xml` |
| 屠宰肉类继承 | Thrumbo（`Thrumbo`） | — | 肉类继承自 Muffalo | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Giant.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Giants.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Giant.xml` |
| 屠宰肉类继承 | Husky（`Husky`） | — | 肉类继承自 Elephant | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Pet.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_DogGroup.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Pet.xml` |
| 屠宰肉类继承 | LabradorRetriever（`LabradorRetriever`） | — | 肉类继承自 Elephant | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Pet.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_DogGroup.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_Pet.xml` |
| 屠宰肉类继承 | Warg（`Warg`） | — | 肉类继承自 Elephant | — | — | PatchOperationConditional; PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_WildCanines.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_WildCanines.xml | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Races/Races_Animal_WildCanines.xml` |
| 屠宰肉类继承 | GuineaPig（`GuineaPig`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml` |
| 屠宰肉类继承 | Alpaca（`Alpaca`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml` |
| 屠宰肉类继承 | Fox_Fennec（`Fox_Fennec`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml` |
| 屠宰肉类继承 | Fox_Red（`Fox_Red`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml` |
| 屠宰肉类继承 | Fox_Arctic（`Fox_Arctic`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Foxes.xml` |
| 屠宰肉类继承 | Hare（`Hare`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Hares.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Hares.xml` |
| 屠宰肉类继承 | snow hare（`Snowhare`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Hares.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Hares.xml` |
| 屠宰肉类继承 | Wolf_Timber（`Wolf_Timber`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Wolfs.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Wolfs.xml` |
| 屠宰肉类继承 | Wolf_Arctic（`Wolf_Arctic`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Wolfs.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Races/Races_Animal_Wolfs.xml` |
| 屠宰肉类继承 | Gazelle（`Gazelle`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 屠宰肉类继承 | Iguana（`Iguana`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 屠宰肉类继承 | Dromedary（`Dromedary`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 屠宰肉类继承 | Sheep（`Sheep`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml` |
| 屠宰肉类继承 | Lynx（`Lynx`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_BigCats.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_BigCats.xml` |
| 屠宰肉类继承 | Ostrich（`Ostrich`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml` |
| 屠宰肉类继承 | Emu（`Emu`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml` |
| 屠宰肉类继承 | Cassowary（`Cassowary`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Birds.xml` |
| 屠宰肉类继承 | common cat（`Cat`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_CatGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_CatGroup.xml` |
| 屠宰肉类继承 | Chicken（`Chicken`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml` |
| 屠宰肉类继承 | Duck（`Duck`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml` |
| 屠宰肉类继承 | Turkey（`Turkey`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml` |
| 屠宰肉类继承 | Goose（`Goose`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_ChickenGroup.xml` |
| 屠宰肉类继承 | YorkshireTerrier（`YorkshireTerrier`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_DogGroup.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_DogGroup.xml` |
| 屠宰肉类继承 | Bison（`Bison`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Cow（`Cow`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Donkey（`Donkey`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Goat（`Goat`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Pig（`Pig`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Yak（`Yak`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml` |
| 屠宰肉类继承 | Rhinoceros（`Rhinoceros`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Giants.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Giants.xml` |
| 屠宰肉类继承 | Horse（`Horse`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Horse.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Horse.xml` |
| 屠宰肉类继承 | Alphabeaver（`Alphabeaver`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Boomrat（`Boomrat`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Capybara（`Capybara`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Chinchilla（`Chinchilla`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Raccoon（`Raccoon`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Rat（`Rat`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Squirrel（`Squirrel`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml` |
| 屠宰肉类继承 | Caribou（`Caribou`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | Deer（`Deer`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | Elk（`Elk`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | Ibex（`Ibex`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | Tortoise（`Tortoise`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | WildBoar（`WildBoar`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml` |
| 屠宰肉类继承 | Boomalope（`Boomalope`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |
| 屠宰肉类继承 | Cobra（`Cobra`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |
| 屠宰肉类继承 | Monkey（`Monkey`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml` |
| 屠宰肉类继承 | WasteRat（`WasteRat`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Biotech/Patches/ThingDefs_Races/Races_Animal_Pollution.xml | `Mods/Core_SK_DLCModule/Biotech/Patches/ThingDefs_Races/Races_Animal_Pollution.xml` |
| 屠宰肉类继承 | Toxalope（`Toxalope`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Biotech/Patches/ThingDefs_Races/Races_Animal_Pollution.xml | `Mods/Core_SK_DLCModule/Biotech/Patches/ThingDefs_Races/Races_Animal_Pollution.xml` |
| 屠宰肉类继承 | Tiger（`Tiger`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animal_BigCats.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animal_BigCats.xml` |
| 屠宰肉类继承 | Wolf_Great（`Wolf_Great`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animal_Birds.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animal_Birds.xml` |
| 屠宰肉类继承 | BogHound（`BogHound`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | MonitorLizard（`MonitorLizard`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | PrairieDog（`PrairieDog`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Porcupine（`Porcupine`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Mastodon（`Mastodon`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Cat_Scimitar（`Cat_Scimitar`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Wolverine（`Wolverine`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Moose（`Moose`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | AlphaThrumbo（`AlphaThrumbo`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Muskox（`Muskox`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Mink（`Mink`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Gorilla（`Gorilla`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Badger（`Badger`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Panda（`Panda`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Hippo（`Hippo`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Megavole（`Megavole`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Armadillo（`Armadillo`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Alligator（`Alligator`） | — | 肉类继承自 Muffalo | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Bullfrog（`Bullfrog`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | ColossusToad（`ColossusToad`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | LavaSnail（`LavaSnail`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals.xml` |
| 屠宰肉类继承 | Walrus（`Walrus`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | SeaLion（`SeaLion`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | Seal（`Seal`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | HermitCrab（`HermitCrab`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | StoneCrab（`StoneCrab`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | SeaTurtle（`SeaTurtle`） | — | 肉类继承自 ThingDefFishSduiggles | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | Otter（`Otter`） | — | 肉类继承自 Elephant | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Coastal.xml` |
| 屠宰肉类继承 | Larva（`Larva`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml` |
| 屠宰肉类继承 | HiveQueen（`HiveQueen`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml` |
| 屠宰肉类继承 | Locust（`Locust`） | — | 肉类继承自 Megaspider | — | — | PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml | `Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Races/Races_Animals_Insect.xml` |

## 4. C# 驱动、XML 未给出产量的建筑

| 建筑 | Def | ThingClass | 源文件 |
| --- | --- | --- | --- |
| Aquaculture basin | `AquacultureBasin` | `SK.Building_AquacultureBasin` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Building_AquacultureBasin.xml` |
| fishing spot | `HSK_FishingSpotSpawner` | `SK.Building_FishingPierSpawner` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Building_FishingPier.xml` |
| fishing spot | `HSK_FishingSpot` | `SK.Building_FishingPier` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Building_FishingPier.xml` |

### 4.1 需要运行时代码才能得到精确数值的系统

- `Core_SK_FertileFields`：地形改造矩阵、堆肥/地形转化等部分规则由 `FertileFields.dll` 运行时生成；`Core_SK` 中已有的 FertileFields RecipeDef/建筑 XML 已纳入本表。
- `Core_SK_MendAndRecycle`：修理与回收配方由 `Mending.dll` 按物品制造成本动态生成，XML 中没有固定返还比例；本文不估算该比例。
- `AquacultureBasin` / `FishingPier`：部分渔业产出由 C# 驱动；能从 RecipeDef 提取的渔业配方已纳入，其余仅标记建筑。
- `RareMinerResourceDefs`：当前官方模块仅存在 `.bak` 文件，未被游戏加载，因此不生成生产条目。

## 5. 官方模块内识别但无本地生产路径的资源

| 资源 | Def | 大类 | 分类 | 源文件 |
| --- | --- | --- | --- | --- |
| Berserk herb | `Berserk_herb` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/Drugs/Drugs.xml` |
| crate of applejack | `Crate_Applejack` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of arak | `Crate_Arak` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of berry wine | `Crate_BerryWine` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of brandy | `Crate_Brandy` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of apple cider | `Crate_Cider` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of cognac | `Crate_Cognac` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of gin | `Crate_Gin` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of kahlua | `Crate_Kahlua` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of moonshine | `Crate_Moonshine` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of rum | `Crate_Rum` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of sake | `Crate_Sake` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of tequila | `Crate_Tequila` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of vodka | `Crate_Vodka` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of whiskey | `Crate_Whiskey` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| crate of wine | `Crate_Wine` | 食物 | `Foods/Alcohol/Alcohol_Crates` | `Mods/Core_SK/Defs/Drugs/AlcoholCrates.xml` |
| Deployable Tent | `DeployableTent` | 加工品 | `Manufactured/Tents` | `Mods/Core_SK/Defs/ThingDefs_Items/Items_CampingStuff.xml` |
| Deployable Tent (Big) | `DeployableTentBig` | 加工品 | `Manufactured/Tents` | `Mods/Core_SK/Defs/ThingDefs_Items/Items_CampingStuff.xml` |
| Deployable Tent (Medium) | `DeployableTentMedium` | 加工品 | `Manufactured/Tents` | `Mods/Core_SK/Defs/ThingDefs_Items/Items_CampingStuff.xml` |
| honey | `Honey` | 其他资源 | `AnimalProductRaw` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Honey.xml` |
| Kibble | `Kibble` | 食物 | `Foods/Animalfood` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs/Meals_Special.xml` |
| barghest fur pelt | `Leather_BarghestFur` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| bird leather | `Leather_Bird` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| blue fur pelt | `Leather_Bluefur` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| boomanimal leather | `Leather_Boomanimal` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| chinchilla fur pelt | `Leather_Chinchilla` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Dog | `Leather_Dog` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Elephant | `Leather_Elephant` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| feathered raptor leather | `Leather_FeatheredRaptorLeather` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| feenix dragonskin scales | `Leather_FireDragonskin` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| fox fur pelt | `Leather_Fox` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Heavy | `Leather_Heavy` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Human | `Leather_Human` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| kirin fur pelt | `Leather_KirinHide` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| light leather | `Leather_Light` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| lizard leather | `Leather_Lizard` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| nightling fur pelt | `Leather_Nightling` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| big cat fur pelt | `Leather_Panthera` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| patch leather | `Leather_Patch` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| pig leather | `Leather_Pig` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Plain | `Leather_Plain` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| Leather_Rhinoceros | `Leather_Rhinoceros` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| magic fur | `Leather_Thrumbo` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| wolf fur pelt | `Leather_Wolf` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff_Leather.xml` |
| MA-AI chip | `MAAI_Chip` | 加工品 | `Manufactured/ElectronicsCat` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Electronics.xml` |
| methadone | `Methadone` | 医疗 | `Manufactured/MedicineStuff/MedicalDrugs` | `Mods/Core_SK/Defs/Drugs/Methadone.xml` |
| Uranium Ore Deposit | `MineableUranium` | 未分类 | `未分类资源` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml` |
| Mote | `Mote_Bubble` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Bubble.xml` |
| Mote_FishBluebladeEast | `Mote_FishBluebladeEast` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote_FishBluebladeWest | `Mote_FishBluebladeWest` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote_FishMashgonEast | `Mote_FishMashgonEast` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote_FishMashgonWest | `Mote_FishMashgonWest` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote_FishTailteethEast | `Mote_FishTailteethEast` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote_FishTailteethWest | `Mote_FishTailteethWest` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_Fish.xml` |
| Mote | `Mote_FishingRodEast` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_FishingRod.xml` |
| Mote | `Mote_FishingRodNorth` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_FishingRod.xml` |
| Mote | `Mote_FishingRodSouth` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_FishingRod.xml` |
| Mote | `Mote_FishingRodWest` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Mote_FishingRod.xml` |
| Oyster | `Oyster` | 食物 | `FoodMeals` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Item_Oyster.xml` |
| Pearl | `Pearl` | 其他资源 | `ItemsMisc` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Item_Pearl.xml` |
| Penoxycyline | `Penoxycyline` | 医疗 | `Manufactured/MedicineStuff/MedicalDrugs` | `Mods/Core_SK_CoreModule/Patches/Mods_ResearchTree/ResearchPrerequisites/Core.xml` |
| drill pipe | `Pipe` | 加工品 | `Manufactured/Parts` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Parts.xml` |
| venom gland | `PoisonGland` | 医疗 | `Manufactured/MedicineStuff/MedicalMaterials` | `Mods/Core_SK/Defs/ThingDefs_Meals/Items_CurePoison.xml` |
| Prometheum | `Prometheum` | 加工品 | `Manufactured/ExplosivesCat` | `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Resources/Items_Resource_Fuel.xml` |
| rotted meat | `RottedMeat` | 食物 | `Foods/Spoilage` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Terraforming.xml` |
| rotted mush | `RottedMush` | 食物 | `Foods/Spoilage` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Terraforming.xml` |
| Soft Clay | `SoftClay` | 原材料 | `ResourcesRaw/NaturalResources` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Extracted.xml` |
| synchronisity powder | `Synchronisity` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/Drugs/Synchronisity.xml` |
| Synthetic Ammonia | `SyntheticAmmonia` | 原材料 | `ResourcesRaw/Extracted/Chemical` | `Mods/Core_SK/Defs/ThingDefs_Resources/Items_Resource_Chemical.xml` |
| cuda | `ThingDefCuda` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| death sturgeon | `ThingDefDeathSturgeon` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| breadmouth bass | `ThingDefFishBlueblade` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| killer catfish | `ThingDefFishKillerCatfish` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| snackeral | `ThingDefFishMashgon` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| pinstripe bass | `ThingDefFishPinstripeBass` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| hypnosquid | `ThingDefFishSduiggles` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| eel | `ThingDefFishSeasnake` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| sterling perch | `ThingDefFishSterlingPerch` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| alligar | `ThingDefFishTailteeth` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| ice flounder | `ThingDefIceFlounder` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| redfin tuna | `ThingDefRedfinTuna` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| saberfish | `ThingDefSaberfish` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/ThingDefs_FishIndustry/Races_Animal_Fish.xml` |
| cigar | `TobaccoCigar` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/Drugs/TobaccoCigar.xml` |
| chocolate cigar | `TobaccoCigarChocolate` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/Drugs/TobaccoCigarChocolate.xml` |
| cigarette | `TobaccoCigarette` | 未分类 | `未分类资源` | `Mods/Core_SK/Defs/Drugs/TobaccoCigarette.xml` |
| Bison wool | `WoolBison` | 加工品 | `Textiles/BTextiles` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff.xml` |
| Megasloth wool | `WoolMegasloth` | 加工品 | `Textiles/BTextiles` | `Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Stuff.xml` |

## 6. 提取规则与限制

- XML 注释、`.bak`、`.disable`、`Languages/` 不参与提取。
- 替代输入以 `A 或 B ×数量` 表示；`[类别]` 表示 ThingCategory 输入。
- `ingredientValueGetterClass=IngredientValueGetter_Nutrition` 的配方，材料数量按营养值而非件数解释，表中已标注。
- 屠宰的肉类和皮革主要由 RimWorld 运行时根据动物体型、肉类和皮革规则计算；本文只列出 XML 明确的额外屠宰产物，并把 `useMeatFrom` 放入动态产出表。
- `specialProducts=Butchery`、拆解/熔炼返还等路径由 C# 按输入物成本动态计算，XML 没有固定产出表，统一放在第 3 节。
- PatchOperation 已应用生产文档所需的受限子集：Replace/ReplaceExtended、Add/Insert、Remove、AttributeSet；复杂条件、整 Def 删除和带谓词的深层 XPath 保留在诊断中。Patch 中新增的完整 Def 已纳入，条件列保留 `MayRequire` / `FindMod`。
- 原版游戏定义的父类（如 `PlantBase`、`BuildingBase`）不在 HSK 仓库内；若数值只存在于原版父类，本静态提取可能为空。

## 7. 解析诊断

- Def 记录数：7375
- Patch 应用操作数：3763
- Patch 未模拟操作数：562
- 重复 defName 覆盖数：0
- XML 解析错误数：0

### 7.1 未模拟的 Patch 操作

- `Mods/Core_SK/Patches/AutoGenPatches/PawnKindDefs_Humanlikes/PawnKinds_Mercenary.xml: Defs/PawnKindDef[defName="Mercenary_Sniper" or defName="Mercenary_Sniper_Acidifier"]/weaponMoney`
- `Mods/Core_SK/Patches/AutoGenPatches/ThingDefs/MeleeWeapons.xml: Defs/ThingDef[defName="MeleeWeapon_Ikwa" or defName="MeleeWeapon_Spear" or defName="MeleeWeapon_Mace" or defName="MeleeWeapon_Club" or defName="MeleeWeapon_BreachAxe" ]`
- `Mods/Core_SK/Patches/AutoGenPatches/ThingDefs/MeleeWeapons.xml: Defs/ThingDef[defName="MeleeWeapon_Knife" ]`
- `Mods/Core_SK/Patches/AutoGenPatches/ThingDefs/MeleeWeapons.xml: Defs/ThingDef[defName="MeleeWeapon_Gladius" or defName="MeleeWeapon_LongSword" ]`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Human.xml: Defs/BodyPartDef[defName="Brain" or defName="oBrain"]/permanentInjuryChanceFactor`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def="Head"]/parts/li[def="Nose"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[customLabel = "left arm"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[customLabel = "right arm"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[customLabel="left shoulder"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[customLabel="right shoulder"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Ribcage"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Pelvis"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Stomach"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Heart"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Lung"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Kidney"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Liver"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Neck"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def="Head"]/parts/li[def="Skull"]/parts/li[def = "Brain"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def="Head"]/parts/li[def = "Eye"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def="Head"]/parts/li[def = "Ear"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def="Head"]/parts/li[def = "Nose"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Shoulder"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Clavicle"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Arm"]/parts/li[def="Humerus"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Arm"]/parts/li[def="Radius"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Leg"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Leg"]/parts/li[def="Femur"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Leg"]/parts/li[def="Tibia"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Leg"]/parts/li[def="Foot"]/coverage`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Neck"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def = "Head"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def = "Head"]/parts/li[def="Jaw"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def = "Head"]/parts/li[def="Nose"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Neck"]/parts/li[def = "Head"]/parts/li[def="Ear"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Shoulder"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Arm"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Arm"]/parts/li[def = "Hand"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Shoulder"]/parts/li[def = "Arm"]/parts/li[def = "Hand"]/parts/li[def = "Finger"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def = "Leg"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Leg"]/parts/li[def = "Foot"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Orassan.xml: Defs/BodyDef[defName = "Orassan"]/corePart/parts/li[def="Leg"]/parts/li[def = "Foot"]/parts/li[def = "Toe"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Terminator.xml: Defs/BodyDef[defName = "HumanoidTerminator"]/corePart/parts/li[def="TerminatorNeck"]/parts/li[def="Head"]/parts/li[def="TerminatorNose"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Terminator.xml: Defs/BodyDef[defName = "HumanoidTerminator"]/corePart/parts/li[def="TerminatorShoulder"]/parts/li[customLabel = "left arm"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Terminator.xml: Defs/BodyDef[defName = "HumanoidTerminator"]/corePart/parts/li[def="TerminatorShoulder"]/parts/li[customLabel = "right arm"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Terminator.xml: Defs/BodyDef[defName = "HumanoidTerminator"]/corePart/parts/li[customLabel="left shoulder"]/groups`
- `Mods/Core_SK/Patches/Changes_Bodies/Bodies_Humanlike/Terminator.xml: Defs/BodyDef[defName = "HumanoidTerminator"]/corePart/parts/li[customLabel="right shoulder"]/groups`
- `Mods/Core_SK/Patches/Core/CompStunnable.xml: Defs/ThingDef/comps[not(*[@Class="CompProperties_Stunnable"]) and (*[@Class="CompProperties_Power"])]`
- `Mods/Core_SK/Patches/Core/Skills.xml: Defs/SkillDef[defName="Construction" or
			defName="Mining" or
			defName="Cooking" or
			defName="Animals" or
			defName="Crafting" or
			defName="Artistic" or
			defName="Medicine" or
			defName="Intellectual"]`
- `Mods/Core_SK/Patches/Core/Skills.xml: Defs/SkillDef[defName="Shooting" or defName="Melee"]`
- `Mods/Core_SK/Patches/Core/ThingDefs_Misc/Items_Resource_Categories_Core.xml: Defs/ThingDef[
            defName="AIPersonaCore" or
            defName="TechprofSubpersonaCore"
        ]/thingCategories`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/12x64mmCharged.xml: Defs/RecipeDef[defName="MakeAmmo_12x64mmCharged" and @ParentName="ChargeAmmoRecipeBase" and @MayRequire="Ludeon.RimWorld.Biotech"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/15x65mmDiffusingCharged.xml: Defs/ThingDef[defName="Ammo_15x65mmDiffusingCharged" or defName="Bullet_15x65mmDiffusingCharged"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/30x64mmFuelCell.xml: `
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoBenchesRecipes.xml: Defs/RecipeDef[@Name="GrenadeRecipeBase" or @ParentName="GrenadeRecipeBase"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoBenchesRecipes.xml: Defs/ThingDef[@Name="30x29mmGrenadeBase" or
                                    @Name="40x46mmGrenadeBase" or
                                    @Name="155mmHowitzerShellBase" or
                                    @Name="120mmCannonShellBase" or
                                    @Name="120mmMortarShellBase" or
                                    @Name="90mmCannonShellBase" or
                                    @Name="81mmMortarShellBaseCraftableBase" or
                                    @Name="BaseRPG7Grenade" or
                                    @Name="SPG9RocketBase"
                ]/tradeTags/li[text()="CE_AutoEnableCrafting_TableMachining"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoBenchesRecipes.xml: Defs/ThingDef[defName="Ammo_RPG7Grenade_HEAT" or
                                    defName="Ammo_RPG7Grenade_Thermobaric" or
                                    defName="Ammo_RPG7Grenade_Frag"
                                    ]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Advanced.xml: Defs/CombatExtended.AmmoCategoryDef[defName="ForsakenArrow"]|
Defs/CombatExtended.AmmoCategoryDef[defName="Plasma"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Advanced.xml: whole-def remove Defs/CombatExtended.AmmoCategoryDef[defName="RailgunSabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Advanced.xml: whole-def remove Defs/CombatExtended.AmmoCategoryDef[defName="NanobotsMT"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="VenomArrow"]/labelShort|
Defs/CombatExtended.AmmoCategoryDef[defName="VenomArrow"]/advanced`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="Kunai"]|
Defs/CombatExtended.AmmoCategoryDef[defName="SteelArrow"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="PlasteelArrow"]|
Defs/CombatExtended.AmmoCategoryDef[defName="StoneSlingBullet"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="SteelSlingBullet"]|
Defs/CombatExtended.AmmoCategoryDef[defName="PlasteelSlingBullet"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="StoneCrossbowBolt"]|
Defs/CombatExtended.AmmoCategoryDef[defName="SteelCrossbowBolt"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: Defs/CombatExtended.AmmoCategoryDef[defName="PlasteelCrossbowBolt"]|
Defs/CombatExtended.AmmoCategoryDef[defName="VenomCrossbowBolt"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/AmmoCategoryDefs/AmmoCategories_Neolithic.xml: whole-def remove Defs/CombatExtended.AmmoCategoryDef[defName="FlameCrossbowBolt"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/HighCaliber/14.5x114mmSoviet.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_145x114mm"]/ammoTypes/Ammo_145x114mm_Sabot|
Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_145x114mm"]/ammoTypes/Ammo_145x114mm_AP`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/HighCaliber/14.5x114mmSoviet.xml: Defs/ThingDef[defName="Ammo_145x114mm_AP" or defName="Ammo_145x114mm_Sabot" or defName="Bullet_145x114mm_AP" or defName="Bullet_145x114mm_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/HighCaliber/14.5x114mmSoviet.xml: Defs/RecipeDef[defName="MakeAmmo_145x114mm_AP" or defName="MakeAmmo_145x114mm_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/HighCaliber/50BMG.xml: Defs/ThingDef[defName="Ammo_50BMG_AP" or defName="Bullet_50BMG_AP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/HighCaliber/50BMG.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_50BMG_AP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Neolithic/Arrows.xml: Defs/RecipeDef[defName="MakeAmmo_Arrow_Venom"]/fixedIngredientFilter/thingDefs/li[text()="WoodLog"]|
Defs/RecipeDef[defName="MakeAmmo_Arrow_Venom"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Neolithic/Arrows.xml: Defs/RecipeDef[defName="MakeAmmo_Arrow_Flame"]/fixedIngredientFilter/thingDefs/li[text()="WoodLog"]|
Defs/RecipeDef[defName="MakeAmmo_Arrow_Flame"]/fixedIngredientFilter/thingDefs/li[text()="Prometheum"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Neolithic/Arrows.xml: Defs/ThingDef[defName="Ammo_Arrow_Plasteel"]|
Defs/ThingDef[defName="Projectile_Arrow_Plasteel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Neolithic/Arrows.xml: whole-def remove Defs/ThingDef[defName="Projectile_StreamlinedArrow_Plasteel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Neolithic/Arrows.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_Arrow_Plasteel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Pistols/45ACP.xml: whole-def remove Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_45ACP410Bore_SB"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Pistols/9x19mmPara.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_9x19mmParaSD"]|
Defs/ThingDef[@Name="Base9x19mmParaSDBullet" or defName="Bullet_9x19mmParaSD_FMJ" or defName="Bullet_9x19mmParaSD_AP" or defName="Bullet_9x19mmParaSD_HP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Projectiles_Fragments.xml: whole-def remove Defs/ThingDef[defName="Fragment_Bomblet"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/303British.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_303British" or defName="AmmoSet_303British_SB"]/ammoTypes/*[self::Ammo_303British_Incendiary or self::Ammo_303British_HE or self::Ammo_303British_Sabot]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/303British.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_303British" or defName="AmmoSet_303British_SB"]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/303British.xml: Defs/ThingDef[defName="Ammo_303British_Incendiary" or defName="Ammo_303British_HE" or defName="Ammo_303British_Sabot" or defName="Bullet_303British_Incendiary" or defName="Bullet_303British_HE" or defName="Bullet_303British_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/303British.xml: Defs/RecipeDef[defName="MakeAmmo_303British_Incendiary" or defName="MakeAmmo_303British_HE" or defName="MakeAmmo_303British_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/556x45mmNATO.xml: Defs/ThingDef[defName="Ammo_556x45mmNATO_Incendiary" or defName="Ammo_556x45mmNATO_HE" or defName="Ammo_556x45mmNATO_Sabot" or defName="Bullet_556x45mmNATO_Incendiary" or defName="Bullet_556x45mmNATO_HE" or defName="Bullet_556x45mmNATO_Sabot" or defName="Bullet_556x45mmNATO_HE_SB"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/556x45mmNATO.xml: Defs/RecipeDef[defName="MakeAmmo_556x45mmNATO_Incendiary" or defName="MakeAmmo_556x45mmNATO_HE" or defName="MakeAmmo_556x45mmNATO_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/5x50mmCaseless.xml: Defs/CombatExtended.AmmoSetDef[	defName="AmmoSet_5x50mmCaseless" or 
												defName="AmmoSet_5x50mmCaselessToxic" or
												defName="AmmoSet_5x50mmCaseless_LV" or
												defName="AmmoSet_5x50mmCaseless_HV"]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x39mmSoviet.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_762x39mmSoviet" or defName="AmmoSet_762x39mmSoviet_SB"]
				/ammoTypes/*[name()="Ammo_762x39mmSoviet_Incendiary" or 
								name()="Ammo_762x39mmSoviet_Sabot" or
								name()="Ammo_762x39mmSoviet_HE"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x39mmSoviet.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_762x39mmSoviet" or defName="AmmoSet_762x39mmSoviet_SB"]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x39mmSoviet.xml: Defs/ThingDef[defName="Ammo_762x39mmSoviet_Incendiary" or defName="Ammo_762x39mmSoviet_HE" or defName="Ammo_762x39mmSoviet_Sabot" or defName="Bullet_762x39mmSoviet_Incendiary" or defName="Bullet_762x39mmSoviet_HE" or defName="Bullet_762x39mmSoviet_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x39mmSoviet.xml: Defs/RecipeDef[defName="MakeAmmo_762x39mmSoviet_Incendiary" or defName="MakeAmmo_762x39mmSoviet_HE" or defName="MakeAmmo_762x39mmSoviet_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x51mmNATO.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_762x51mmNATO" or defName="AmmoSet_762x51mmNATO_SB"]/ammoTypes/*[self::Ammo_762x51mmNATO_Sabot or self::Ammo_762x51mmNATO_Incendiary or self::Ammo_762x51mmNATO_HE]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x51mmNATO.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_762x51mmNATO" or defName="AmmoSet_762x51mmNATO_SB"]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x51mmNATO.xml: Defs/ThingDef[defName="Ammo_762x51mmNATO_Incendiary" or defName="Ammo_762x51mmNATO_HE" or defName="Ammo_762x51mmNATO_Sabot" or defName="Bullet_762x51mmNATO_Incendiary" or defName="Bullet_762x51mmNATO_HE" or defName="Bullet_762x51mmNATO_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rifle/762x51mmNATO.xml: Defs/RecipeDef[defName="MakeAmmo_762x51mmNATO_Incendiary" or defName="MakeAmmo_762x51mmNATO_HE" or defName="MakeAmmo_762x51mmNATO_Sabot"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/RPG7.xml: Defs/ThingDef[defName="Ammo_RPG7Grenade_Frag"]/stackLimit|
Defs/ThingDef[defName="Ammo_RPG7Grenade_Frag"]/detonateProjectile`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/RPG7.xml: Defs/RecipeDef[defName="MakeAmmo_RPG7Grenade_HEAT"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_RPG7Grenade_HEAT"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/RPG7.xml: Defs/RecipeDef[defName="MakeAmmo_RPG7Grenade_Thermobaric"]/fixedIngredientFilter/thingDefs/li[text()="Prometheum"]|
Defs/RecipeDef[defName="MakeAmmo_RPG7Grenade_Thermobaric"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_SPG9Rocket"]/ammoTypes/Ammo_SPG9Rocket_EMP|
Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_SPG9Rocket"]/ammoTypes/Ammo_SPG9Rocket_Thermobaric`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_HEAT"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_HEAT"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_Frag"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_Frag"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: Defs/ThingDef[defName="Ammo_SPG9Rocket_Thermobaric"]|
Defs/ThingDef[defName="Ammo_SPG9Rocket_EMP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: Defs/ThingDef[defName="Bullet_SPG9Rocket_Thermobaric"]|
Defs/ThingDef[defName="Bullet_SPG9Rocket_EMP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_Thermobaric"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Rocket/SPG9.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_SPG9Rocket_EMP"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_155mmHowitzerShell" or defName="AmmoSet_155mmHowitzerShell_directfire"]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_155mmHowitzerShell" or defName="AmmoSet_155mmHowitzerShell_directfire"]
				/ammoTypes/*[name()="Ammo_155mmHowitzerShell_Smoke" or 
							name()="Ammo_155mmHowitzerShell_Incendiary"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: Defs/RecipeDef[defName="MakeAmmo_155mmHowitzerShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_155mmHowitzerShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: Defs/ThingDef[defName="Ammo_155mmHowitzerShell_Incendiary" or 
								defName="Ammo_155mmHowitzerShell_Smoke" or
								defName="Bullet_155mmHowitzerShell_Incendiary" or
								defName="Bullet_155mmHowitzerShell_Smoke" or
								defName="Bullet_155mmHowitzerShell_Incendiary_directfire" or
								defName="Bullet_155mmHowitzerShell_Smoke_directfire"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_155mmHowitzerShell_Incendiary"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/155mmHowitzer.xml: whole-def remove Defs/RecipeDef[defName="MakeAmmo_155mmHowitzerShell_Smoke"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/ThingDef[defName="Shell_HighExplosive" and @ParentName="81mmMortarShellBaseCraftableBase"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/ThingDef[defName="Shell_Incendiary" and @ParentName="81mmMortarShellBaseCraftableBase"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/RecipeDef[defName="MakeShell_Incendiary"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeShell_Incendiary"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/RecipeDef[defName="MakeShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/RecipeDef[defName="MakeShell_Smoke"]/fixedIngredientFilter/thingDefs/li[text()="Prometheum"]|
Defs/RecipeDef[defName="MakeShell_Smoke"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: Defs/RecipeDef[defName="MakeShell_Toxic"]/fixedIngredientFilter/thingDefs/li[text()="Prometheum"]|
Defs/RecipeDef[defName="MakeShell_Toxic"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml: `
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/90mmCannon.xml: Defs/RecipeDef[defName="MakeAmmo_90mmCannonShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_90mmCannonShell_EMP"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/90mmCannon.xml: Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_90mmCannonShell"]/ammoTypes/*[name()="Ammo_90mmCannonShell_AP" or 
																							name()="Ammo_90mmCannonShell_APCR"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/90mmCannon.xml: Defs/ThingDef[defName="Bullet_90mmCannonShell_AP" or
								defName="Bullet_90mmCannonShell_APCR" or
								defName="Ammo_90mmCannonShell_AP" or
								defName="Ammo_90mmCannonShell_APCR"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/90mmCannon.xml: Defs/RecipeDef[defName="MakeAmmo_90mmCannonShell_AP" or
								defName="MakeAmmo_90mmCannonShell_APCR"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/90mmCannon.xml: `
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml: Defs/CombatExtended.AmmoSetDef[contains(defName,"AmmoSet_12Gauge")]/similarTo`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml: whole-def remove Defs/CombatExtended.AmmoSetDef[defName="AmmoSet_12Gauge_SB"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml: Defs/RecipeDef[defName="MakeAmmo_12Gauge_ElectroSlug"]/fixedIngredientFilter/thingDefs/li[text()="ComponentIndustrial"]|
Defs/RecipeDef[defName="MakeAmmo_12Gauge_ElectroSlug"]/fixedIngredientFilter/thingDefs/li[text()="Steel"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Effects/Flecks.xml: whole-def remove Defs/ThingDef[defName="Mote_MeltaFlash"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Effects/Flecks.xml: Defs/FleckDef[defName="Mote_BolterCasing" or
				defName="Mote_HeavyBolterCasing"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/EyeCover.xml: Defs/ThingDef[
            defName="CE_Apparel_GasMask" 
            or defName="CE_Apparel_ImprovGasMask" 
            or defName="CE_Apparel_SimpleNightVision" 
            or defName="CE_Apparel_AdvancedNightVision"
        ]/apparel/layers`
- `Mods/Core_SK/Patches/Mods_CombatExtended/EyeCover.xml: Defs/ThingDef[
            defName="Apparel_Psymask" 
            or defName="Apparello_Gas" 
            or defName="Apparello_ViperGas" 
            or defName="Apparello_Cyninja" 
            or defName="Apparel_TacticalMask"
            or defName="WeldingHelmet"
            or defName="Apparel_Respirator"
        ]/apparel/layers`
- `Mods/Core_SK/Patches/Mods_CombatExtended/GunPatcherDefs/GunPatcherDefs.xml: Defs/CombatExtended.GunPatcherPresetDef/CaliberRanges/li[
			AmmoSet="AmmoSet_545x39mmSoviet" or
			AmmoSet="AmmoSet_762x54mmR" or
			AmmoSet="AmmoSet_46x30mm" or
			AmmoSet="AmmoSet_FN57x28mm" or
			AmmoSet="AmmoSet_38Special" or 
			AmmoSet="AmmoSet_44Magnum"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/GunPatcherDefs/GunPatcherDefs.xml: Defs/CombatExtended.GunPatcherPresetDef/specialGuns/li[caliber="AmmoSet_FN57x28mm"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/GunPatcherDefs/GunPatcherDefs.xml: whole-def remove Defs/CombatExtended.GunPatcherPresetDef[defName="Revolver"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Misc/ShellingResponseDefs_Advanced.xml: Defs/CombatExtended.ShellingResponseDef/projectiles/li[projectile="Bullet_80x256mmFuel_Incendiary"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Misc/ShellingResponseDefs_Advanced.xml: Defs/CombatExtended.ShellingResponseDef/projectiles/li[projectile="Bullet_105mmHowitzerShell_HE"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/Misc/ShellingResponseDefs_Advanced.xml: Defs/CombatExtended.ShellingResponseDef/projectiles/li[projectile="Bullet_105mmHowitzerShell_Incendiary"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Apparel/Apparel_Headgear.xml: Defs/ThingDef[	defName="CE_Apparel_PlateHelmet" or
								defName="CE_Apparel_GasMask" or
								defName="CE_Apparel_ImprovGasMask"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Apparel/Apparel_Headgear.xml: Defs/ThingDef[	defName="CE_Apparel_SimpleNightVision" or
								defName="CE_Apparel_AdvancedNightVision"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Apparel/Apparel_Shields.xml: whole-def remove Defs/ThingDef[defName="CE_Apparel_MeleeShield"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Items/Items_Unfinished.xml: whole-def remove Defs/ThingDef[defName="UnfinishedSteeledTechArmor"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Apparel_Carrying.xml: Defs/ThingDef[	defName="CE_Apparel_TacVest" or
								defName="CE_Apparel_Backpack" or
								defName="CE_Apparel_KidBackpack"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Apparel_Carrying.xml: Defs/ThingDef[	defName="CE_Apparel_RadioPack" or
								defName="CE_Apparel_ExoFrame"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Apparel_Various.xml: whole-def remove Defs/ThingDef[defName="CE_Apparel_CompositeVest"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Ranged.xml: Defs/ThingDef[	defName="CE_GrenadeLauncher" or
								defName="CE_DisposableRocketLauncher"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Ranged.xml: Defs/RecipeDef[	defName="MakeDisposableRocketLauncher" or
								defName="MakeDisposableRocketLauncher_x1"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Ranged.xml: Defs/ThingDef[defName="CE_ThermalBoltProjector" or
								defName="CE_MechanoidMinigun"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Turrets.xml: Defs/ThingDef[	defName="Gun_AutocannonTurret" or
								defName="Gun_MediumTurret"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Turrets.xml: Defs/ThingDef[	defName="Turret_Medium" or
								defName="Turret_Heavy"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Turrets.xml: Defs/ThingDef[@Name="BaseTurretGun" and @ParentName="BaseGun"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Misc/Weapons_Turrets.xml: Defs/ThingDef[	defName="Gun_KPV" or
								defName="Gun_M240B" or
								defName="Gun_FlakTurret" or
								defName="Gun_AGSThirty"
								]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Weapons/Weapons_Grenades.xml: Defs/ThingDef[defName="CE_Proj_GrenadeFirefoam" or
								defName="CE_Proj_GrenadeSmoke" or
								defName="CE_Proj_GrenadeStickBomb" or
								defName="CE_Proj_GrenadeFlashbang"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/ThingDefs_Weapons/Weapons_Grenades.xml: Defs/ThingDef[defName="CE_Proj_GrenadeConcussion" or
								defName="CE_Weapon_GrenadeConcussion"]`
- `Mods/Core_SK/Patches/Mods_CombatExtended/TraderKindDefs.xml: Defs/TraderKindDef/stockGenerators/li[@Class="StockGenerator_MarketValue" and tradeTag="WeaponRanged"]`
- `Mods/Core_SK/Patches/Mods_Hygiene/BuildingsWaterRequirement.xml: Defs/ThingDef[defName='HydroponicsBasin' or defName='NewHydroponicsBasin' or defName='ClutterAlloyHydroponicsBasinVS']`
- `Mods/Core_SK/Patches/Mods_Hygiene/BuildingsWaterRequirement.xml: Defs/ThingDef[defName='HydroponicsBasin']/comps`
- `Mods/Core_SK/Patches/Mods_Hygiene/BuildingsWaterRequirement.xml: Defs/ThingDef[defName='FertilizerPump']/comps`
- `Mods/Core_SK/Patches/Mods_Hygiene/MoveBionicImplants.xml: Defs/ThingDef[defName="HygieneEnhancer" or defName="BionicBladder"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ADogSaid.xml: Defs/ResearchProjectDef[
                defName="SimpleAnimalProsthetics" or
                defName="AnimalBionics" or
                defName="AnimalHeal"
                ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/CSL.xml: Defs/ThingDef[
                defName="BirthControl" or
                defName="FertilityBooster" or
                defName="PregnancyTest"
                ]/recipeMaker/researchPrerequisite`
- `Mods/Core_SK/Patches/Mods_ResearchTree/Industrial_Rollers.xml: whole-def remove Defs/ResearchProjectDef[defName="ConveyorsResearch"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/Industrial_Rollers.xml: Defs/ThingDef[
                    defName="MovingRailWall" or
                    defName="MovingRailPusher" or
                    defName="MovingRailPuller" or
                    defName="MovingRailSplitter" or
                    defName="MovingRailGreenPuller" or
                    defName="MovingRailUndergroundInput" or
                    defName="MovingRailUndergroundOutput"
                    ]/researchPrerequisites`
- `Mods/Core_SK/Patches/Mods_ResearchTree/MineralsFrozen_SK.xml: Defs/ResearchProjectDef[
                    defName="FrozenConstructionI" or
                    defName="FrozenConstructionII" or
                    defName="FrozenConstructionIII" or
                    defName="FrozenConstructionVI" or
                    defName="FrozenConstructionV"
                    ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/ThingDef[
                    defName="ChJDroidPrinter" or 
                    defName="ChjDroidHibernationPad"
                    ]/researchPrerequisites`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/ThingDef[
                    defName="AndroidRepairParts" or
                    defName="ChjAndroid_ChemfuelGenerator" or
                    defName="ChJAndroidThermalBandages"
                    ]/recipeMaker/researchPrerequisite`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_DroneCore" or
                    defName="Upgrade_LoverCore"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_VanometricCell" or
                    defName="Upgrade_MechaniteHive" or
                    defName="Upgrade_BlackBox" or
                    defName="Upgrade_PsychicAttunement"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_FightingModule" or
                    defName="Upgrade_MedicalModule" or
                    defName="Upgrade_ConstructionModule" or
                    defName="Upgrade_CrafterModule" or
                    defName="Upgrade_AgriculturalModule" or
                    defName="Upgrade_ScienceModule" or
                    defName="Upgrade_DiplomacyModule"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_HyperweaveSkin" or
                    defName="Upgrade_PlasteelSkin" or
                    defName="Upgrade_DragonScales"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_TechprofAIModule" or
                    defName="Upgrade_ArchotechAIModule"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Androids.xml: Defs/Androids.AndroidUpgradeDef[
                    defName="Upgrade_HulkPhysique" or
                    defName="Upgrade_AgilePhysique" or
                    defName="Upgrade_SuperPhysique"
                    ]/requiredResearch`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/BackupPower.xml: whole-def remove Defs/ResearchProjectDef[defName="BackupPower"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchPrerequisites/Statue_of_Colonist.xml: Defs/ThingDef[
                    defName="TMB_StatueOfColonistKLarge" or 
                    defName="TMB_StatueOfColonistKGrand"
                    ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/Androids.xml: Defs/ResearchProjectDef[
      defName="AndroidRepairKit" or
      defName="ChJAndroidPortableChemfuelGenerator" or
      defName="ChJAndroidDroidBatteries" or
      defName="ChJDroidPrinter" or
      defName="ChJBattleDroid" or
      defName="ChJAndroidPrinter" or
      defName="ChJAndroidUpgrade_Hub" or
      defName="ChJAndroidUpgrade_Physique" or
      defName="ChJAndroidUpgrade_Skin" or
      defName="ChJAndroidUpgrade_Utility" or
      defName="ChJAndroidUpgrade_Proficency" or
      defName="ChJAndroidUpgrade_Archotech"
      ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/Androids.xml: whole-def remove Defs/ResearchProjectDef[defName="AndroidRepairKit"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/CyberneticOrganism.xml: Defs/ResearchProjectDef[
      defName="CONN_NeuralNetwork" or
      defName="CONN_CyberneticOrganism"
      ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/FencesAndFloors.xml: whole-def remove Defs/ResearchProjectDef[defName="FutureFlooring"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/RT_Fuse_SK.xml: Defs/ResearchProjectDef[
      defName="ResearchProject_RTBasicFuses" or
      defName="ResearchProject_RTCircuitBreakers"
      ]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/ResearchRemove/RT_SolarFlareShield_SK.xml: whole-def remove Defs/ResearchProjectDef[defName="ResearchProject_RTMagneticShield"]`
- `Mods/Core_SK/Patches/Mods_ResearchTree/RTSKModExtension.xml: Defs/ResearchProjectDef[(tab="Animal_SK" or tab="Art_SK" or tab="Buildings_SK" or tab="Apparel_SK" or tab="Craft_SK" or tab="Food_SK" or tab="Medicine_SK" or tab="Plants_SK" or tab="Floors_SK" or tab="Terraforming_SK" or tab="Weapon_SK") and not(modExtensions/li[@Class="ResearchTreeSK.ResearchTreeSKModExtension"])
        ]`
- `Mods/Core_SK/Patches/Mods_Rimfeller/Buildings.xml: Defs/ThingDef[defName="OilStorage" or defName="FuelStorage" or defName="CrudeCracker"]/statBases/MaxHitPoints`
- `Mods/Core_SK/Patches/Mods_Rimfeller/Buildings.xml: Defs/ThingDef[@Name="RimefellerPipeBase" or defName="pipelineValve"]/statBases/MaxHitPoints`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[@Name='sbzFridgeBase' or defName='sbz_CorpseFridge']/researchPrerequisites`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[@Name='sbzFridgeBase']/building`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_Fridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_SingleChestFridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_SingleWallFridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_LargeFridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_DoubleChestFridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_DoubleWallFridge']`
- `Mods/Core_SK/Patches/Mods_sbz_Fridge/Fridges.xml: Defs/ThingDef[defName='sbz_CorpseFridge']`
- `Mods/Core_SK/Patches/SK_Patches_AnimalTrader_AllHumanFaction.xml: Defs/FactionDef[@ParentName = "FactionBase" and defName != "Empire"]/caravanTraderKinds`
- `Mods/Core_SK/Patches/SK_Patches_Bodies_AddNose.xml: Defs/BodyDef/corePart/parts/li[contains(def,'Neck')]/parts/li[def="Head"]/parts/li[def="Nose"]/groups`
- `Mods/Core_SK/Patches/SK_Patches_Bodies_AddNose.xml: Defs/BodyDef/corePart/parts/li[contains(def,'Neck')]/parts/li[def="Head"]/parts/li[def="Nose" and not(groups)]`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffGiverSetDef[defName = "Human"]/hediffGivers`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffGiverSetDef[defName = "Human"]/hediffGivers`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffGiverSetDef[defName = "Human"]/hediffGivers`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffGiverSetDef[defName = "Human"]/hediffGivers`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffGiverSetDef[defName = "Human"]/hediffGivers`
- `Mods/Core_SK/Patches/SK_Patches_CommunicableDiseasesChanges.xml: Defs/HediffDef[defName="Flu" or 
			defName="Plague" or 
			defName="CommonCold" or 
			defName="Measles" or 
			defName="Coronavirus"]`
- ……其余 362 条省略
