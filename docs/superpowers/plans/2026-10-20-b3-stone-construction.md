# B3 石料建材 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 补齐混凝土电动线（电动搅拌机+电动混凝土/钢筋混凝土/土堆）与农业辅料（堆肥/肥料/土堆/盐链），全部数值对齐 HSK。

**Architecture:** 纯 XML Def（无 C# 改动）。混凝土电动线追加到既有 Concrete 文件组；农业辅料与盐新建 Agriculture 文件组（资源/配方/建筑/研究各一）；贴图从 HSK 仓库复制（已授权）。验证手段是 `tools/validate_mod.py` 新增 `stone-construction` 套件。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 验证脚本、dotnet build（net10.0，仅回归验证）。

**Spec:** `docs/superpowers/specs/2026-10-20-b3-stone-construction-design.md`

## Global Constraints

- 所有 defName 加 `MMT_` 前缀；label/description 用中文；文件编码 UTF-8（XML 声明必须 `encoding="utf-8"`）。
- 新研究一律 `<tab>MMT_ResearchTab</tab>`；新建筑一律 `designationCategory=MMT_Production`。
- 不得引用任何 `SK.` 类型或 SK 资源/类别（如 StrongMetallic、SK.CompProperties_IdlePower）。
- MMT 自有建筑的配方一律挂在建筑 `<recipes>` 列表，不写 recipeUsers；原版建筑才用 recipeUsers。
- 数值照抄 HSK（见 spec 各表）；MMT 没有的资源按 spec §2 适配结论替换。
- 提交按任务分步，中文 conventional commits，只提交本地，绝不 push。
- 每任务结束跑 `python tools/validate_mod.py all`（现有套件必须保持全绿）+ `dotnet build Source/MyModeTest/MyModeTest.csproj`（0 错误，C# 未改动属回归验证）。
- 控制台乱码时用 `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`。

## Review Focus

1. **贴图路径大小写**：Windows 文件系统大小写不敏感，`assert_png` 能过但游戏内可能丢贴图——复制后逐一核对文件名（尤其 `MMT_EConcreteMixer_eastm.png` 这类带 `m` 后缀的遮罩）。→ Task 2/3 步骤含逐项 `ls` 核对。
2. **原版 defName 拼写**：recipeUsers 引用 `ButcherSpot`/`TableButcher`/`FueledStove`/`ElectricStove`/`Hay` 拼错会在游戏加载时报红，静态套件已断言这些字符串原样值。→ Task 4 套件断言。
3. **研究坐标重叠**：新研究 researchViewX/Y 若与既有研究重叠会导致研究界面节点堆叠——`MMT_ConcreteAdvanced` 用 `(8.00, 0.00)`（y=0 行 7 与 10 之间空位），`MMT_AgricultureBasic` 用 `(0.00, 2.00)`（该坐标无既有研究）。→ Task 4 套件断言坐标值。
4. **堆肥配方材料可选性**：`fixedIngredientFilter` 若漏掉类别/物品，玩家账单里选不到材料——必须含 categories FoodRaw + thingDefs Hay/MMT_Ash。→ Task 4 套件断言。
5. **挖盐点地形限制**：`terrainAffordanceNeeded=SmoothableStone` 意味着只能放在粗糙石面，玩家可能误判"无法放置是 bug"——description 必须说明"放在粗糙石面"。→ Task 4 套件断言。

---

### Task 1: 农业辅料资源 + 研究 + 堆肥/肥料/土堆配方

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Agriculture.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml`
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Agriculture.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml`（MMT_ConcreteMixer 的 recipes 列表加一条）
- Create（贴图复制）: `1.7/Textures/Things/Item/MMT/{Dirt,Compost,Fertilizer,Salt}.png`

**Interfaces:**
- Consumes: `MMT_Sand`（既有资源）、`MMT_Ash`（B2 资源）、`MMT_ConcreteMixer`（既有建筑）、`MMT_ConcreteBasic`（既有研究）、原版 `MeatRaw`/`AnimalProductRaw`/`PlantFoodRaw`/`FoodRaw` 类别、`Hay`、`ButcherSpot`/`TableButcher`/`FueledStove`/`ElectricStove`。
- Produces（后续任务依赖）: `MMT_Compost`、`MMT_Fertilizer`、`MMT_Dirt` 资源 defName；`MMT_AgricultureBasic` 研究 defName；`MMT_Recipes_Agriculture.xml` 文件（Task 2 向其追加 `MMT_MineSalt`）。

- [ ] **Step 1: 复制 4 张贴图**

```bash
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/DirtPile.png" "1.7/Textures/Things/Item/MMT/Dirt.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Compost.png" "1.7/Textures/Things/Item/MMT/Compost.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Fertilizer.png" "1.7/Textures/Things/Item/MMT/Fertilizer.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Salt.png" "1.7/Textures/Things/Item/MMT/Salt.png"
ls -la "1.7/Textures/Things/Item/MMT/" | grep -E "Dirt|Compost|Fertilizer|Salt"
```

预期：4 个文件存在，文件名与 texPath 用名完全一致（`Dirt.png` 不是 `DirtPile.png`）。

- [ ] **Step 2: 新建资源文件 `MMT_Items_Resource_Agriculture.xml`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 农业辅料：堆肥/肥料/土堆/盐，数值对齐 HSK（Items_Resource_Terraforming.xml / Items_Resource_Extracted.xml） -->
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Compost</defName>
    <label>堆肥料</label>
    <description>食物废料发酵而成的混合物，气味难闻，但是制作肥料的原料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Compost</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Wood_Drop</soundInteract>
    <soundDrop>Wood_Drop</soundDrop>
    <stackLimit>300</stackLimit>
    <statBases>
      <MarketValue>2.4</MarketValue>
      <DeteriorationRate>5</DeteriorationRate>
      <Flammability>0.5</Flammability>
      <Beauty>-14</Beauty>
      <Mass>0.4</Mass>
    </statBases>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
    <tradeability>None</tradeability>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Fertilizer</defName>
    <label>肥料</label>
    <description>增强土壤肥力、加速植物生长的肥料，也是拌制土堆的原料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Fertilizer</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Wood_Drop</soundInteract>
    <soundDrop>Wood_Drop</soundDrop>
    <stackLimit>300</stackLimit>
    <statBases>
      <MarketValue>3.8</MarketValue>
      <DeteriorationRate>5</DeteriorationRate>
      <Flammability>1.5</Flammability>
      <Beauty>-7</Beauty>
      <Mass>0.35</Mass>
    </statBases>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Dirt</defName>
    <label>土堆</label>
    <description>从地里挖出的普通土壤，可用作窑炉的封料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Dirt</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Standard_Drop</soundInteract>
    <soundDrop>Standard_Drop</soundDrop>
    <stackLimit>900</stackLimit>
    <statBases>
      <MarketValue>0.2</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Mass>0.50</Mass>
    </statBases>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Salt</defName>
    <label>盐</label>
    <description>常见的矿物盐，是化工生产中重要的原料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Salt</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <color>(245,245,245)</color>
    </graphicData>
    <stackLimit>600</stackLimit>
    <statBases>
      <MarketValue>1</MarketValue>
      <Mass>0.025</Mass>
    </statBases>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: 新建研究文件 `MMT_Research_Agriculture.xml`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 农业 I：解锁堆肥/肥料/手动土堆，对照 HSK SewageSludgeComposting（150 点，新石器） -->
  <ResearchProjectDef>
    <defName>MMT_AgricultureBasic</defName>
    <label>农业 I</label>
    <description>学习将有机废料堆成堆肥并加工为肥料，为农业生产打下基础。</description>
    <baseCost>150</baseCost>
    <techLevel>Neolithic</techLevel>
    <tab>MMT_ResearchTab</tab>
    <researchViewX>0.00</researchViewX>
    <researchViewY>2.00</researchViewY>
  </ResearchProjectDef>
</Defs>
```

- [ ] **Step 4: 新建配方文件 `MMT_Recipes_Agriculture.xml`（4 条）**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 堆肥：对照 HSK MakeCompost；RottedMeat/RottedMush/Waste 为 HSK 专有，用原版类别+Hay+MMT_Ash 替代 -->
  <RecipeDef>
    <defName>MMT_MakeCompost</defName>
    <label>制作堆肥</label>
    <description>将有机废料堆成堆肥。产出 5。</description>
    <jobString>正在制作堆肥。</jobString>
    <workAmount>625</workAmount>
    <workSpeedStat>DrugCookingSpeed</workSpeedStat>
    <effectWorking>Cook</effectWorking>
    <soundWorking>Recipe_CookMeal</soundWorking>
    <allowMixingIngredients>true</allowMixingIngredients>
    <recipeUsers>
      <li>ButcherSpot</li>
      <li>TableButcher</li>
      <li>FueledStove</li>
      <li>ElectricStove</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>MeatRaw</li>
            <li>AnimalProductRaw</li>
          </categories>
        </filter>
        <count>25</count>
      </li>
      <li>
        <filter>
          <categories>
            <li>PlantFoodRaw</li>
          </categories>
          <thingDefs>
            <li>Hay</li>
            <li>MMT_Ash</li>
          </thingDefs>
        </filter>
        <count>5</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>FoodRaw</li>
      </categories>
      <thingDefs>
        <li>Hay</li>
        <li>MMT_Ash</li>
      </thingDefs>
    </fixedIngredientFilter>
    <defaultIngredientFilter>
      <thingDefs>
        <li>Hay</li>
        <li>MMT_Ash</li>
      </thingDefs>
    </defaultIngredientFilter>
    <products>
      <MMT_Compost>5</MMT_Compost>
    </products>
    <workSkill>Crafting</workSkill>
    <researchPrerequisite>MMT_AgricultureBasic</researchPrerequisite>
  </RecipeDef>

  <!-- 肥料：对照 HSK MakeFertilizer（HSK 中为无建筑挂载的死配方，MMT 挂到食物区建筑） -->
  <RecipeDef>
    <defName>MMT_MakeFertilizer</defName>
    <label>制作肥料</label>
    <description>将堆肥进一步加工为肥料。产出 10。</description>
    <jobString>正在制作肥料。</jobString>
    <workAmount>10000</workAmount>
    <workSpeedStat>DrugCookingSpeed</workSpeedStat>
    <effectWorking>Cook</effectWorking>
    <soundWorking>Recipe_CookMeal</soundWorking>
    <recipeUsers>
      <li>ButcherSpot</li>
      <li>TableButcher</li>
      <li>FueledStove</li>
      <li>ElectricStove</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Compost</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Compost</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Fertilizer>10</MMT_Fertilizer>
    </products>
    <workSkill>Crafting</workSkill>
    <researchPrerequisite>MMT_AgricultureBasic</researchPrerequisite>
  </RecipeDef>

  <!-- 土堆（屠宰点小批量）：对照 HSK make_dirt -->
  <RecipeDef>
    <defName>MMT_MakeDirt</defName>
    <label>拌制土堆</label>
    <description>将沙和肥料拌成土堆。产出 3。</description>
    <jobString>正在拌制土堆。</jobString>
    <workAmount>300</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <recipeUsers>
      <li>ButcherSpot</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>2</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Fertilizer</li>
          </thingDefs>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
        <li>MMT_Fertilizer</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Dirt>3</MMT_Dirt>
    </products>
    <workSkill>Crafting</workSkill>
    <researchPrerequisite>MMT_AgricultureBasic</researchPrerequisite>
  </RecipeDef>

  <!-- 土堆（手动搅拌器大批量）：对照 HSK make_dirt_hand；挂 MMT_ConcreteMixer 的 recipes 列表 -->
  <RecipeDef>
    <defName>MMT_MakeDirt_Hand</defName>
    <label>拌制土堆</label>
    <description>将沙和肥料拌成土堆。产出 30。</description>
    <jobString>正在拌制土堆。</jobString>
    <workAmount>3000</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Fertilizer</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
        <li>MMT_Fertilizer</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Dirt>30</MMT_Dirt>
    </products>
    <workSkill>Crafting</workSkill>
    <researchPrerequisite>MMT_ConcreteBasic</researchPrerequisite>
  </RecipeDef>
</Defs>
```

- [ ] **Step 5: MMT_ConcreteMixer 挂载 MMT_MakeDirt_Hand**

修改 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml` 中 `MMT_ConcreteMixer` 的 recipes 列表：

```xml
    <recipes>
      <li>MMT_MixConcrete_Hand</li>
      <li>MMT_MakeDirt_Hand</li>
    </recipes>
```

- [ ] **Step 6: 回归验证**

```bash
python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：validate 全绿（现有套件不受新文件影响）；build 0 错误。

- [ ] **Step 7: Commit**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Agriculture.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml 1.7/Defs/ResearchProjectDefs/MMT_Research_Agriculture.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml "1.7/Textures/Things/Item/MMT/Dirt.png" "1.7/Textures/Things/Item/MMT/Compost.png" "1.7/Textures/Things/Item/MMT/Fertilizer.png" "1.7/Textures/Things/Item/MMT/Salt.png"
git commit -m "feat: 添加农业辅料资源与堆肥肥料配方"
```

---

### Task 2: 盐链（盐矿脉 + 挖盐点 + 挖盐配方）

**Files:**
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Agriculture.xml`
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml`（追加 `MMT_MineSalt`）
- Create（贴图复制）: `1.7/Textures/Things/Building/Production/MMT_SaltMine.png`

**Interfaces:**
- Consumes: `MMT_Salt`（Task 1）、`MMT_Recipes_Agriculture.xml`（Task 1 创建）。
- Produces: `MMT_MineableSalt`、`MMT_SaltMine`、`MMT_MineSalt` defName（Task 4 验证）。

- [ ] **Step 1: 复制挖盐点贴图**

```bash
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/ProductionFood/SaltMine.png" "1.7/Textures/Things/Building/Production/MMT_SaltMine.png"
ls -la "1.7/Textures/Things/Building/Production/MMT_SaltMine.png"
```

- [ ] **Step 2: 新建 `MMT_Buildings_Agriculture.xml`（矿脉 + 挖盐点）**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 盐矿脉：对照 HSK MineableSalt（产量 150 / 散布 1.2 / 矿团 5~10） -->
  <ThingDef ParentName="RockBase">
    <defName>MMT_MineableSalt</defName>
    <label>压实的盐</label>
    <description>经长年压实形成的盐矿层，开采后可获得盐。</description>
    <graphicData>
      <texPath>Things/Building/Linked/RockFlecked_Atlas</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <color>(199,201,197)</color>
      <colorTwo>(240,242,237)</colorTwo>
    </graphicData>
    <statBases>
      <MaxHitPoints>900</MaxHitPoints>
    </statBases>
    <building>
      <isResourceRock>true</isResourceRock>
      <mineableThing>MMT_Salt</mineableThing>
      <mineableYield>150</mineableYield>
      <mineableScatterCommonality>1.2</mineableScatterCommonality>
      <mineableScatterLumpSizeRange>5~10</mineableScatterLumpSizeRange>
      <veinMineable>true</veinMineable>
    </building>
  </ThingDef>

  <!-- 挖盐点：对照 HSK VG_SaltMine（1x1，需粗糙石面，免研究）；原版 Building_WorkTable 即可 -->
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_SaltMine</defName>
    <label>挖盐点</label>
    <description>放置在粗糙的石面上，从中挖掘盐。费时费力。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_SaltMine</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <drawSize>(1,1)</drawSize>
    </graphicData>
    <altitudeLayer>FloorEmplacement</altitudeLayer>
    <statBases>
      <WorkToBuild>2500</WorkToBuild>
    </statBases>
    <scatterableOnMapGen>false</scatterableOnMapGen>
    <useHitPoints>False</useHitPoints>
    <size>(1,1)</size>
    <designationCategory>MMT_Production</designationCategory>
    <useStuffTerrainAffordance>false</useStuffTerrainAffordance>
    <terrainAffordanceNeeded>SmoothableStone</terrainAffordanceNeeded>
    <passability>Standable</passability>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-1)</interactionCellOffset>
    <recipes>
      <li>MMT_MineSalt</li>
    </recipes>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <building>
      <canPlaceOverImpassablePlant>false</canPlaceOverImpassablePlant>
    </building>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: `MMT_Recipes_Agriculture.xml` 追加挖盐配方（`</Defs>` 之前插入）**

```xml
  <!-- 挖盐：对照 HSK VG_Minesalt（无输入，工作量 1800，采矿技能） -->
  <RecipeDef>
    <defName>MMT_MineSalt</defName>
    <label>挖掘盐</label>
    <description>从粗糙的岩石中挖盐。工作量很大。产出 5。</description>
    <jobString>正在挖盐。</jobString>
    <workSpeedStat>MiningSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <workSkillLearnFactor>0.5</workSkillLearnFactor>
    <soundWorking>Recipe_Smith</soundWorking>
    <workAmount>1800</workAmount>
    <products>
      <MMT_Salt>5</MMT_Salt>
    </products>
    <workSkill>Mining</workSkill>
  </RecipeDef>
```

- [ ] **Step 4: 回归验证**

```bash
python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：validate 全绿；build 0 错误。

- [ ] **Step 5: Commit**

```bash
git add 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Agriculture.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml "1.7/Textures/Things/Building/Production/MMT_SaltMine.png"
git commit -m "feat: 添加盐矿脉与挖盐点"
```

---

### Task 3: 电动混凝土线（EConcreteMixer + 混凝土 II + 3 条电动配方）

**Files:**
- Modify: `1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml`（追加 `MMT_ConcreteAdvanced`）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml`（追加 `MMT_EConcreteMixer`）
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml`（追加 3 条配方）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml:110-119`（`MMT_ElectricAssemblyBench` recipes 列表加一条）
- Create（贴图复制）: `1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer_{east,north,south}{,m}.png`（6 个）

**Interfaces:**
- Consumes: `MMT_Sand`/`MMT_CrushedStone`/`MMT_Concrete`/`MMT_ReinforcedConcrete`（既有）、`MMT_Fertilizer`/`MMT_Dirt`（Task 1）、`MMT_Rubber`/`MMT_ElectronicParts`/`MMT_Mechanism`（既有资源）、`MMT_MetallicBar`（B1 类别）、`MMT_ConcreteBasic`（既有研究）、`MMT_ElectricAssemblyBench`（既有建筑）。
- Produces: `MMT_ConcreteAdvanced`、`MMT_EConcreteMixer`、`MMT_MixConcrete_Electric`、`MMT_MakeReinforcedConcrete_Electric`、`MMT_MakeDirt_Electric` defName（Task 4 验证）。

- [ ] **Step 1: 复制电动搅拌机贴图（6 个，含遮罩）**

```bash
mkdir -p "1.7/Textures/Things/Building/Production/MMT_EConcreteMixer"
for f in east eastm north northm south southm; do
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/ConcreteMixer/ConcreteMixerElectro_${f}.png" "1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer_${f}.png"
done
ls "1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/"
```

预期：恰好 6 个文件，文件名形如 `MMT_EConcreteMixer_eastm.png`（遮罩 `m` 在朝向后缀后）。

- [ ] **Step 2: `MMT_Research_Concrete.xml` 追加混凝土 II（`</Defs>` 之前插入）**

```xml
  <!-- 混凝土 II：解锁电动混凝土搅拌机与电动配方，对照 HSK Concrete_C2（1000 点，工业） -->
  <ResearchProjectDef>
    <defName>MMT_ConcreteAdvanced</defName>
    <label>混凝土 II</label>
    <description>学习使用电力驱动的混凝土搅拌机，更快地搅拌混凝土并拌制土堆。</description>
    <baseCost>1000</baseCost>
    <techLevel>Industrial</techLevel>
    <tab>MMT_ResearchTab</tab>
    <prerequisites>
      <li>MMT_ConcreteBasic</li>
    </prerequisites>
    <researchViewX>8.00</researchViewX>
    <researchViewY>0.00</researchViewY>
  </ResearchProjectDef>
```

- [ ] **Step 3: `MMT_Buildings_Concrete.xml` 追加电动搅拌机（`</Defs>` 之前插入）**

```xml
  <!-- 电动混凝土搅拌机：对照 HSK EConcreteMixer；SK 的 IdlePower 组件用原版 CompProperties_Power 替代 -->
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_EConcreteMixer</defName>
    <label>电动混凝土搅拌机</label>
    <description>以电力驱动的混凝土搅拌机，由砂和碎石搅拌混凝土，效率更高。需要持续供电。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(2,1)</drawSize>
    </graphicData>
    <passability>PassThroughOnly</passability>
    <pathCost>80</pathCost>
    <fillPercent>0.6</fillPercent>
    <surfaceType>Item</surfaceType>
    <canOverlapZones>false</canOverlapZones>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-1)</interactionCellOffset>
    <useStuffTerrainAffordance>true</useStuffTerrainAffordance>
    <stuffCategories>
      <li>Metallic</li>
    </stuffCategories>
    <costStuffCount>115</costStuffCount>
    <costList>
      <MMT_Rubber>45</MMT_Rubber>
      <ComponentIndustrial>10</ComponentIndustrial>
      <MMT_ElectronicParts>5</MMT_ElectronicParts>
      <MMT_Mechanism>5</MMT_Mechanism>
    </costList>
    <minifiedDef>MinifiedThing</minifiedDef>
    <thingCategories>
      <li>BuildingsProduction</li>
    </thingCategories>
    <statBases>
      <WorkToBuild>5000</WorkToBuild>
      <MaxHitPoints>180</MaxHitPoints>
      <Flammability>0.5</Flammability>
      <Beauty>-40</Beauty>
      <Cleanliness>-6</Cleanliness>
      <Mass>500</Mass>
    </statBases>
    <size>(2,1)</size>
    <designationCategory>MMT_Production</designationCategory>
    <recipes>
      <li>MMT_MixConcrete_Electric</li>
      <li>MMT_MakeDirt_Electric</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <comps>
      <li Class="CompProperties_Power">
        <compClass>CompPowerTrader</compClass>
        <basePowerConsumption>500</basePowerConsumption>
        <shortCircuitInRain>true</shortCircuitInRain>
      </li>
      <li Class="CompProperties_Flickable" />
      <li Class="CompProperties_Breakdownable" />
    </comps>
    <building>
      <uninstallWork>2500</uninstallWork>
    </building>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_ConcreteAdvanced</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>11</constructionSkillPrerequisite>
  </ThingDef>
```

- [ ] **Step 4: `MMT_Recipes_Concrete.xml` 追加 3 条电动配方（`</Defs>` 之前插入）**

```xml
  <!-- 电动混凝土：对照 HSK MixConcreteElectric（工作量减半，技能 6） -->
  <RecipeDef>
    <defName>MMT_MixConcrete_Electric</defName>
    <label>搅拌混凝土</label>
    <description>将砂和碎石搅拌成混凝土。产出 15。</description>
    <jobString>正在搅拌混凝土。</jobString>
    <workAmount>600</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>45</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_CrushedStone</li>
          </thingDefs>
        </filter>
        <count>30</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
        <li>MMT_CrushedStone</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Concrete>15</MMT_Concrete>
    </products>
    <skillRequirements>
      <Crafting>6</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ConcreteAdvanced</researchPrerequisite>
  </RecipeDef>

  <!-- 电动钢筋混凝土：对照 HSK MakeReinforcedConcrete_Electric；类别输入用 MMT_MetallicBar；挂电动装配台 -->
  <RecipeDef>
    <defName>MMT_MakeReinforcedConcrete_Electric</defName>
    <label>制作钢筋混凝土</label>
    <description>在混凝土中加入金属材料，制作钢筋混凝土。产出 16。</description>
    <jobString>正在制作钢筋混凝土。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>MMT_MetallicBar</li>
          </categories>
        </filter>
        <count>20</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Concrete</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>MMT_MetallicBar</li>
      </categories>
      <thingDefs>
        <li>MMT_Concrete</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_ReinforcedConcrete>16</MMT_ReinforcedConcrete>
    </products>
    <skillRequirements>
      <Crafting>7</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <!-- 电动土堆：对照 HSK make_dirt_Electric -->
  <RecipeDef>
    <defName>MMT_MakeDirt_Electric</defName>
    <label>拌制土堆</label>
    <description>将沙和肥料拌成土堆。产出 30。</description>
    <jobString>正在拌制土堆。</jobString>
    <workAmount>1500</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Fertilizer</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
        <li>MMT_Fertilizer</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Dirt>30</MMT_Dirt>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ConcreteAdvanced</researchPrerequisite>
  </RecipeDef>
```

- [ ] **Step 5: 电动装配台挂载电动钢筋混凝土**

修改 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml` 中 `MMT_ElectricAssemblyBench` 的 recipes 列表，末尾追加一行：

```xml
    <recipes>
      <li>MMT_MakeComponentMedieval_Wood</li>
      <li>MMT_MakeComponentMedieval_Metal</li>
      <li>MMT_MakeComponentIndustrial_Electric</li>
      <li>MMT_MakeWire_Electric</li>
      <li>MMT_MakeMechanism_Electric</li>
      <li>MMT_MakeComponentSpacer_Electric</li>
      <li>MMT_MakeAdvMechanism_Electric</li>
      <li>MMT_MakeSpacerComponent</li>
      <li>MMT_MakeReinforcedConcrete_Electric</li>
    </recipes>
```

- [ ] **Step 6: 回归验证**

```bash
python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：validate 全绿；build 0 错误。

- [ ] **Step 7: Commit**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Concrete.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Concrete.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Concrete.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml 1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/
git commit -m "feat: 添加电动混凝土搅拌机与电动配方"
```

---

### Task 4: `stone-construction` 验证套件

**Files:**
- Modify: `tools/validate_mod.py`（顶部常量区、套件函数、`validate_all`、`SUITES`）

**Interfaces:**
- Consumes: Task 1–3 的全部 defName/文件/贴图；既有 helper（`require_files`/`def_map`/`parse_xml`/`child_text`/`li_values`/`assert_equal`/`fail`/`assert_png`/`read_text`）。
- Produces: 套件名 `stone-construction`（注册进 `SUITES` 与 `validate_all`）。

- [ ] **Step 1: 扩展登记表与路径常量**

`tools/validate_mod.py` 顶部常量区（`ASH_PATCH_XML` 之后）追加：

```python
AGRICULTURE_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Agriculture.xml"
AGRICULTURE_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Agriculture.xml"
AGRICULTURE_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Agriculture.xml"
AGRICULTURE_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Agriculture.xml"

AGRICULTURE_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/Dirt.png",
    "1.7/Textures/Things/Item/MMT/Compost.png",
    "1.7/Textures/Things/Item/MMT/Fertilizer.png",
    "1.7/Textures/Things/Item/MMT/Salt.png",
]

AGRICULTURE_BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_SaltMine.png",
    *[f"1.7/Textures/Things/Building/Production/MMT_EConcreteMixer/MMT_EConcreteMixer_{side}{mask}.png" for side in ("east", "north", "south") for mask in ("", "m")],
]
```

登记表扩展（约 396–399 行）：

```python
VANILLA_CATEGORIES = {"StoneBlocks", "StoneChunks", "Metallic", "MeatRaw", "AnimalProductRaw", "PlantFoodRaw", "FoodRaw"}
VANILLA_WORKTABLES = {"TableStonecutter", "TableMachining", "ButcherSpot", "TableButcher", "FueledStove", "ElectricStove"}
VANILLA_RESEARCH = {"Stonecutting", "Smithing", "BiofuelRefining", "MicroelectronicsBasics"}
VANILLA_THINGS = {"ComponentIndustrial", "ComponentSpacer", "WoodLog", "Steel", "Silver", "Gold", "Plasteel", "Hay"}
```

- [ ] **Step 2: 新增套件函数（放在 `validate_basic_metallurgy` 之后；`CONCRETE_*`/`PARTS_BUILDING_XML` 常量已存在，直接引用）**

```python
def validate_stone_construction() -> None:
    """B3 石料建材：农业辅料资源/盐链/电动混凝土线的专项校验。"""
    require_files([
        AGRICULTURE_RESOURCE_XML,
        AGRICULTURE_RECIPE_XML,
        AGRICULTURE_RESEARCH_XML,
        AGRICULTURE_BUILDING_XML,
        *AGRICULTURE_ITEM_TEXTURES,
        *AGRICULTURE_BUILDING_TEXTURES,
    ])

    # 农业辅料资源
    resource_defs = def_map(parse_xml(AGRICULTURE_RESOURCE_XML))
    assert_equal(set(resource_defs), {"MMT_Compost", "MMT_Fertilizer", "MMT_Dirt", "MMT_Salt"}, "农业资源 defName 集合")

    compost = resource_defs["MMT_Compost"]
    assert_equal(child_text(compost, "label"), "堆肥料", "堆肥 label")
    assert_equal(child_text(compost, "./statBases/MarketValue"), "2.4", "堆肥 MarketValue")
    assert_equal(child_text(compost, "stackLimit"), "300", "堆肥 stackLimit")
    assert_equal(child_text(compost, "./statBases/DeteriorationRate"), "5", "堆肥 DeteriorationRate")
    assert_equal(child_text(compost, "tradeability"), "None", "堆肥 tradeability")
    assert_equal(li_values(compost, "thingCategories"), ["ResourcesRaw"], "堆肥 thingCategories")

    fertilizer = resource_defs["MMT_Fertilizer"]
    assert_equal(child_text(fertilizer, "label"), "肥料", "肥料 label")
    assert_equal(child_text(fertilizer, "./statBases/MarketValue"), "3.8", "肥料 MarketValue")
    assert_equal(child_text(fertilizer, "stackLimit"), "300", "肥料 stackLimit")
    assert_equal(child_text(fertilizer, "./statBases/DeteriorationRate"), "5", "肥料 DeteriorationRate")
    assert_equal(li_values(fertilizer, "thingCategories"), ["ResourcesRaw"], "肥料 thingCategories")

    dirt = resource_defs["MMT_Dirt"]
    assert_equal(child_text(dirt, "label"), "土堆", "土堆 label")
    assert_equal(child_text(dirt, "./statBases/MarketValue"), "0.2", "土堆 MarketValue")
    assert_equal(child_text(dirt, "stackLimit"), "900", "土堆 stackLimit")
    assert_equal(child_text(dirt, "./statBases/DeteriorationRate"), "0", "土堆 DeteriorationRate")
    assert_equal(li_values(dirt, "thingCategories"), ["ResourcesRaw"], "土堆 thingCategories")

    salt = resource_defs["MMT_Salt"]
    assert_equal(child_text(salt, "label"), "盐", "盐 label")
    assert_equal(child_text(salt, "./statBases/MarketValue"), "1", "盐 MarketValue")
    assert_equal(child_text(salt, "stackLimit"), "600", "盐 stackLimit")
    assert_equal(li_values(salt, "thingCategories"), ["ResourcesRaw"], "盐 thingCategories")
    if salt.find("./statBases/Nutrition") is not None or salt.find("ingestible") is not None:
        fail("盐不应带食物属性（MMT 不做食物链）")
    if salt.find("deepCountPerPortion") is not None:
        fail("盐不应带深钻井参数（属非目标）")

    # 盐矿脉 + 挖盐点
    building_defs = def_map(parse_xml(AGRICULTURE_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableSalt", "MMT_SaltMine"}, "农业建筑 defName 集合")

    vein = building_defs["MMT_MineableSalt"]
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_Salt", "盐矿脉产出")
    assert_equal(child_text(vein, "./building/mineableYield"), "150", "盐矿脉产量")
    assert_equal(child_text(vein, "./building/mineableScatterCommonality"), "1.2", "盐矿脉散布")
    assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), "5~10", "盐矿脉矿团")

    mine = building_defs["MMT_SaltMine"]
    assert_equal(child_text(mine, "thingClass"), "Building_WorkTable", "挖盐点 thingClass")
    assert_equal(child_text(mine, "size"), "(1,1)", "挖盐点尺寸")
    assert_equal(child_text(mine, "terrainAffordanceNeeded"), "SmoothableStone", "挖盐点地形需求")
    assert_equal(child_text(mine, "designationCategory"), "MMT_Production", "挖盐点建造分类")
    assert_equal(li_values(mine, "recipes"), ["MMT_MineSalt"], "挖盐点配方列表")

    # 农业配方（堆肥/肥料/土堆×2/挖盐）
    recipe_defs = def_map(parse_xml(AGRICULTURE_RECIPE_XML))
    assert_equal(set(recipe_defs), {"MMT_MakeCompost", "MMT_MakeFertilizer", "MMT_MakeDirt", "MMT_MakeDirt_Hand", "MMT_MineSalt"}, "农业配方 defName 集合")

    make_compost = recipe_defs["MMT_MakeCompost"]
    assert_equal(child_text(make_compost, "workAmount"), "625", "堆肥 workAmount")
    assert_equal(sorted(li_values(make_compost, "recipeUsers")), ["ButcherSpot", "ElectricStove", "FueledStove", "TableButcher"], "堆肥挂载建筑")
    assert_equal(sorted(li_values(make_compost, "./ingredients/li[1]/filter/categories")), ["AnimalProductRaw", "MeatRaw"], "堆肥输入1类别")
    assert_equal(child_text(make_compost, "./ingredients/li[1]/count"), "25", "堆肥输入1数量")
    assert_equal(li_values(make_compost, "./ingredients/li[2]/filter/categories"), ["PlantFoodRaw"], "堆肥输入2类别")
    assert_equal(sorted(li_values(make_compost, "./ingredients/li[2]/filter/thingDefs")), ["Hay", "MMT_Ash"], "堆肥输入2物品")
    assert_equal(child_text(make_compost, "./ingredients/li[2]/count"), "5", "堆肥输入2数量")
    assert_equal(li_values(make_compost, "./fixedIngredientFilter/categories"), ["FoodRaw"], "堆肥固定过滤类别")
    assert_equal(sorted(li_values(make_compost, "./fixedIngredientFilter/thingDefs")), ["Hay", "MMT_Ash"], "堆肥固定过滤物品")
    compost_products = [(node.tag, (node.text or "").strip()) for node in make_compost.findall("./products/*")]
    assert_equal(compost_products, [("MMT_Compost", "5")], "堆肥产出")
    assert_equal(child_text(make_compost, "researchPrerequisite"), "MMT_AgricultureBasic", "堆肥研究")

    make_fertilizer = recipe_defs["MMT_MakeFertilizer"]
    assert_equal(child_text(make_fertilizer, "workAmount"), "10000", "肥料 workAmount")
    assert_equal(sorted(li_values(make_fertilizer, "recipeUsers")), ["ButcherSpot", "ElectricStove", "FueledStove", "TableButcher"], "肥料挂载建筑")
    assert_equal(li_values(make_fertilizer, "./ingredients/li/filter/thingDefs"), ["MMT_Compost"], "肥料输入")
    assert_equal(child_text(make_fertilizer, "./ingredients/li/count"), "10", "肥料输入数量")
    fertilizer_products = [(node.tag, (node.text or "").strip()) for node in make_fertilizer.findall("./products/*")]
    assert_equal(fertilizer_products, [("MMT_Fertilizer", "10")], "肥料产出")
    assert_equal(child_text(make_fertilizer, "researchPrerequisite"), "MMT_AgricultureBasic", "肥料研究")

    make_dirt = recipe_defs["MMT_MakeDirt"]
    assert_equal(child_text(make_dirt, "workAmount"), "300", "土堆 workAmount")
    assert_equal(li_values(make_dirt, "recipeUsers"), ["ButcherSpot"], "土堆挂载建筑")
    dirt_products = [(node.tag, (node.text or "").strip()) for node in make_dirt.findall("./products/*")]
    assert_equal(dirt_products, [("MMT_Dirt", "3")], "土堆产出")
    assert_equal(child_text(make_dirt, "researchPrerequisite"), "MMT_AgricultureBasic", "土堆研究")

    make_dirt_hand = recipe_defs["MMT_MakeDirt_Hand"]
    assert_equal(child_text(make_dirt_hand, "workAmount"), "3000", "手动土堆 workAmount")
    assert_equal(li_values(make_dirt_hand, "recipeUsers"), [], "手动土堆不写 recipeUsers（建筑 recipes 列表挂载）")
    hand_dirt_products = [(node.tag, (node.text or "").strip()) for node in make_dirt_hand.findall("./products/*")]
    assert_equal(hand_dirt_products, [("MMT_Dirt", "30")], "手动土堆产出")
    assert_equal(child_text(make_dirt_hand, "researchPrerequisite"), "MMT_ConcreteBasic", "手动土堆研究")

    mine_salt = recipe_defs["MMT_MineSalt"]
    assert_equal(child_text(mine_salt, "workAmount"), "1800", "挖盐 workAmount")
    assert_equal(child_text(mine_salt, "workSkill"), "Mining", "挖盐 workSkill")
    salt_products = [(node.tag, (node.text or "").strip()) for node in mine_salt.findall("./products/*")]
    assert_equal(salt_products, [("MMT_Salt", "5")], "挖盐产出")

    # 手动搅拌器挂载手动土堆
    concrete_building_defs = def_map(parse_xml(CONCRETE_BUILDING_XML))
    mixer_recipes = li_values(concrete_building_defs["MMT_ConcreteMixer"], "recipes")
    if "MMT_MakeDirt_Hand" not in mixer_recipes:
        fail("MMT_MakeDirt_Hand 未挂载到手动混凝土搅拌机")

    # 电动搅拌机
    e_mixer = concrete_building_defs["MMT_EConcreteMixer"]
    assert_equal(child_text(e_mixer, "label"), "电动混凝土搅拌机", "电动搅拌机 label")
    assert_equal(child_text(e_mixer, "thingClass"), "Building_WorkTable", "电动搅拌机 thingClass")
    assert_equal(child_text(e_mixer, "size"), "(2,1)", "电动搅拌机尺寸")
    assert_equal(child_text(e_mixer, "costStuffCount"), "115", "电动搅拌机材料数")
    assert_equal(child_text(e_mixer, "./statBases/WorkToBuild"), "5000", "电动搅拌机 WorkToBuild")
    assert_equal(child_text(e_mixer, "designationCategory"), "MMT_Production", "电动搅拌机建造分类")
    assert_equal(child_text(e_mixer, "./comps/li[@Class='CompProperties_Power']/basePowerConsumption"), "500", "电动搅拌机耗电")
    assert_equal(li_values(e_mixer, "researchPrerequisites"), ["MMT_ConcreteAdvanced"], "电动搅拌机研究")
    assert_equal(child_text(e_mixer, "constructionSkillPrerequisite"), "11", "电动搅拌机建造技能")
    e_mixer_costs = [(node.tag, (node.text or "").strip()) for node in e_mixer.findall("./costList/*")]
    assert_equal(e_mixer_costs, [("MMT_Rubber", "45"), ("ComponentIndustrial", "10"), ("MMT_ElectronicParts", "5"), ("MMT_Mechanism", "5")], "电动搅拌机造价")
    assert_equal(sorted(li_values(e_mixer, "recipes")), ["MMT_MakeDirt_Electric", "MMT_MixConcrete_Electric"], "电动搅拌机配方列表")

    # 混凝土电动配方
    concrete_recipe_defs = def_map(parse_xml(CONCRETE_RECIPE_XML))
    for name in ("MMT_MixConcrete_Electric", "MMT_MakeReinforcedConcrete_Electric", "MMT_MakeDirt_Electric"):
        if name not in concrete_recipe_defs:
            fail(f"{name} 不存在于混凝土配方文件")

    mix_electric = concrete_recipe_defs["MMT_MixConcrete_Electric"]
    assert_equal(child_text(mix_electric, "workAmount"), "600", "电动混凝土 workAmount")
    assert_equal(child_text(mix_electric, "./skillRequirements/Crafting"), "6", "电动混凝土技能")
    assert_equal(child_text(mix_electric, "researchPrerequisite"), "MMT_ConcreteAdvanced", "电动混凝土研究")
    mix_products = [(node.tag, (node.text or "").strip()) for node in mix_electric.findall("./products/*")]
    assert_equal(mix_products, [("MMT_Concrete", "15")], "电动混凝土产出")

    reinforced_electric = concrete_recipe_defs["MMT_MakeReinforcedConcrete_Electric"]
    assert_equal(child_text(reinforced_electric, "workAmount"), "1200", "电动钢筋混凝土 workAmount")
    assert_equal(child_text(reinforced_electric, "./skillRequirements/Crafting"), "7", "电动钢筋混凝土技能")
    assert_equal(li_values(reinforced_electric, "./ingredients/li[1]/filter/categories"), ["MMT_MetallicBar"], "电动钢筋混凝土输入类别")
    assert_equal(child_text(reinforced_electric, "./ingredients/li[1]/count"), "20", "电动钢筋混凝土输入数量")
    reinforced_products = [(node.tag, (node.text or "").strip()) for node in reinforced_electric.findall("./products/*")]
    assert_equal(reinforced_products, [("MMT_ReinforcedConcrete", "16")], "电动钢筋混凝土产出")
    if reinforced_electric.find("researchPrerequisite") is not None:
        fail("电动钢筋混凝土不应设研究前置（对齐 HSK，靠装配台建筑前置）")

    dirt_electric = concrete_recipe_defs["MMT_MakeDirt_Electric"]
    assert_equal(child_text(dirt_electric, "workAmount"), "1500", "电动土堆 workAmount")
    e_dirt_products = [(node.tag, (node.text or "").strip()) for node in dirt_electric.findall("./products/*")]
    assert_equal(e_dirt_products, [("MMT_Dirt", "30")], "电动土堆产出")
    assert_equal(child_text(dirt_electric, "researchPrerequisite"), "MMT_ConcreteAdvanced", "电动土堆研究")

    # 电动装配台挂载电动钢筋混凝土
    parts_building_defs = def_map(parse_xml(PARTS_BUILDING_XML))
    bench_recipes = li_values(parts_building_defs["MMT_ElectricAssemblyBench"], "recipes")
    if "MMT_MakeReinforcedConcrete_Electric" not in bench_recipes:
        fail("MMT_MakeReinforcedConcrete_Electric 未挂载到电动装配台")

    # 研究
    agriculture_research = def_map(parse_xml(AGRICULTURE_RESEARCH_XML))["MMT_AgricultureBasic"]
    assert_equal(child_text(agriculture_research, "baseCost"), "150", "农业 I baseCost")
    assert_equal(child_text(agriculture_research, "techLevel"), "Neolithic", "农业 I techLevel")
    assert_equal(child_text(agriculture_research, "tab"), "MMT_ResearchTab", "农业 I tab")
    assert_equal(child_text(agriculture_research, "researchViewX"), "0.00", "农业 I 坐标X")
    assert_equal(child_text(agriculture_research, "researchViewY"), "2.00", "农业 I 坐标Y")

    concrete_research = def_map(parse_xml(CONCRETE_RESEARCH_XML))["MMT_ConcreteAdvanced"]
    assert_equal(child_text(concrete_research, "baseCost"), "1000", "混凝土 II baseCost")
    assert_equal(child_text(concrete_research, "techLevel"), "Industrial", "混凝土 II techLevel")
    assert_equal(child_text(concrete_research, "tab"), "MMT_ResearchTab", "混凝土 II tab")
    assert_equal(li_values(concrete_research, "prerequisites"), ["MMT_ConcreteBasic"], "混凝土 II 前置")
    assert_equal(child_text(concrete_research, "researchViewX"), "8.00", "混凝土 II 坐标X")
    assert_equal(child_text(concrete_research, "researchViewY"), "0.00", "混凝土 II 坐标Y")

    for path in (*AGRICULTURE_ITEM_TEXTURES, *AGRICULTURE_BUILDING_TEXTURES):
        assert_png(path)

    for path in (AGRICULTURE_RESOURCE_XML, AGRICULTURE_RECIPE_XML, AGRICULTURE_RESEARCH_XML, AGRICULTURE_BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("stone-construction: OK")
```

- [ ] **Step 3: 注册套件**

`validate_all()` 中 `validate_basic_metallurgy()` 之后追加一行 `validate_stone_construction()`；`SUITES` 字典 `"basic-metallurgy"` 之后追加：

```python
    "stone-construction": validate_stone_construction,
```

- [ ] **Step 4: 运行新套件与全量验证**

```bash
python tools/validate_mod.py stone-construction
python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：`stone-construction: OK`；`all` 全绿；build 0 错误。若有断言失败，修正 Def 后重跑（不得改断言迁就错误实现，除非断言本身写错）。

- [ ] **Step 5: Commit**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加 stone-construction 验证套件"
```

---

## Self-Review 记录

- **Spec 覆盖**：spec §3 资源（Task 1 Step 2）✓、§4 矿脉+挖盐点（Task 2）✓、§5.1 三条电动配方（Task 3 Step 4）✓、§5.2 五条农业配方（Task 1 Step 4 + Task 2 Step 3）✓、§6 EConcreteMixer（Task 3 Step 3）✓、§7 两个研究（Task 1 Step 3 + Task 3 Step 2）✓、§8 验证套件（Task 4）✓、§9 提交分步（各 Task 末尾）✓、§10 非目标（盐食物属性/深钻井参数由 Task 4 反向断言）✓。
- **占位符扫描**：全部步骤含完整 XML/Python/bash 内容，无 TBD/TODO/"类似 Task N"。
- **类型/命名一致性**：全计划 defName 与 spec §1–§7 一致；`MMT_ElectronicParts`/`MMT_Mechanism` 已在既有资源文件中核实存在；`PARTS_BUILDING_XML`/`CONCRETE_*` 常量在 validate_mod.py 已存在。
- **Review Focus 落地**：5 条均有归属步骤（Task 2/3 贴图 ls 核对、Task 4 断言原版字符串/坐标/fixedIngredientFilter/地形需求）。
