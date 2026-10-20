# B2 基础冶金 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 补齐 MMT 基础冶金链：铅/铝/斯特合金/金属罐/软粘土/陶瓷/粘土砖/灰烬，含 3 条矿脉、1 座窑炉、9 条配方、灰烬 patch 与验证套件，数值全部对齐 HSK。

**Architecture:** 纯 XML Def（无 C# 改动）。金属系追加到现有 Metal/Electric 三件套文件；陶瓷链新建 Ceramics 三件套文件 + StuffCategoryDef；灰烬通过 Patch 追加到原版火化配方。电配方统一挂 `MMT_ElectricArcFurnace`（MMT 无电熔炼器）。

**Tech Stack:** RimWorld 1.7 XML Def / PatchOperation，`tools/validate_mod.py`（Python 静态验证）。

**Spec:** `docs/superpowers/specs/2026-10-20-b2-basic-metallurgy-design.md`

## Global Constraints

- 所有 defName 加 `MMT_` 前缀；label/description 中文写死在 Def 里，无本地化目录。
- 所有 XML 文件 UTF-8 编码；不得出现 `SK.` 字符串（validate 套件会拦截）。
- 新建筑 `designationCategory=MMT_Production`；本批不新增研究。
- 电配方挂 `MMT_ElectricArcFurnace` 的 `<recipes>` 列表（不写 recipeUsers）；挂原版建筑（Campfire/ElectricCrematorium）才用 `<recipeUsers>`。
- 提交：中文 conventional commits，只提交本地，message 末尾加 `Co-Authored-By: Claude Code <noreply@anthropic.com>`；`git add` 只加本批涉及的 .xml/.py/.png 文件。
- **WIP 风险**：工作区有用户保留的未提交改动，且部分 B2 目标文件（如 `MMT_Recipes_Metal.xml`）已带 staged WIP。每个 commit 前必须 `git status --short` 检查；若 B2 目标文件混入用户 WIP 无法避免（同文件级提交不可拆分），先暂停报告用户，由用户决定先提交 WIP 还是允许混入，不得擅自处理。
- 验证命令统一加 `PYTHONIOENCODING=utf-8`（防中文乱码）。
- HSK 仓库（`Hardcore-SK/`）只读，不得改动。

## Review Focus

1. **斯特/金属罐输入超出 electric 套件资源白名单**：`Steel`（VANILLA）与 `MMT_Silicon`（electronics 文件）不在 `validate_electric` 的输入资源集合内，直接加期望会 fail——Task 1 步骤里已包含对该套件 resource_names 的扩展，执行时不得遗漏。
2. **铜/锡/青铜类别改 `MMT_SLDBar` 的连锁影响**：`validate_metal_resources` 对 bars 硬编码断言 `["ResourcesRaw"]`，改类别后必须同步改期望，否则 metal 套件红。
3. **`MMT_MakeMetalCan` 是唯一的 categories 输入配方**：`validate_electric` 对所有期望配方强制"输入只含 thingDefs"，MakeMetalCan 不能塞进 `ELECTRIC_RECIPE_EXPECTATIONS`，只能进集合断言例外 + basic-metallurgy 套件专项校验。
4. **灰烬 patch 的 products 节点**：原版 `BurnApparel`/`CremateCorpse` 无 `<products>` 节点，直接 PatchOperationAdd 到 `…/products` 会 xpath 失配——必须用 Conditional nomatch 先建节点（Task 3 已给出完整 XML）。
5. **窑炉 recipes 前向引用**：`MMT_BurnThings` 在 Task 3 才创建，Task 2 建窑炉时 recipes 列表只含陶瓷两条，Task 3 再追加，否则 XML 加载报引用错误。

---

### Task 1: 铅/铝/斯特/金属罐资源 + 矿脉 + 冶炼配方

**Files:**
- Modify: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml`（追加 6 个 ThingDef + 改 3 个既有锭类别）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml`（追加 2 矿脉 + 熔炉 recipes 加 1 条）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml:43-60`（EAF recipes 加 4 条）
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml`（追加 1 配方）
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml`（追加 4 配方）
- Modify: `tools/validate_mod.py:523-580,735-795,1877-1903,2390-2483`（同步现有套件期望）
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_Anglesite/MMT_Anglesite_{a,b,c}.png`、`.../MMT_AluminiumOre/MMT_AluminiumOre_{a,b,c}.png`、`1.7/Textures/Things/Item/MMT/MetalCan.png`（复制自 HSK）

**Interfaces:**
- Consumes: 现有 `MMT_HandFurnace`、`MMT_ElectricArcFurnace`、`MMT_Silicon`、`MMT_MetallicBar`（B1 类别）、原版 `Steel`。
- Produces（后续 Task 依赖）: `MMT_Anglesite`、`MMT_AluminiumOre`、`MMT_LeadBar`、`MMT_AluminiumBar`、`MMT_FerrosiliconAlloy`、`MMT_MetalCan`；矿脉 `MMT_MineableAnglesite`、`MMT_MineableAluminium`；配方 `MMT_SmeltLeadBar_Hand`、`MMT_SmeltLeadBar_Electric`、`MMT_SmeltAluminiumBar_Electric`、`MMT_MakeFerrosiliconAlloy`、`MMT_MakeMetalCan`。

- [ ] **Step 0: WIP 检查**

```bash
cd D:/project/rw && git status --short
```

若上述 6 个目标 .xml 文件带用户 staged WIP（`M ` 前缀），暂停并报告用户，等指示后再继续。

- [ ] **Step 1: 复制贴图**

```bash
cd D:/project/rw
mkdir -p 1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_Anglesite 1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_AluminiumOre
for s in a b c; do
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Anglesite/Anglesite_${s}.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_Anglesite/MMT_Anglesite_${s}.png"
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Ores/Bauxite/Bauxite_${s}.png" "1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_AluminiumOre/MMT_AluminiumOre_${s}.png"
done
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/MetalCan.png" "1.7/Textures/Things/Item/MMT/MetalCan.png"
```

- [ ] **Step 2: `MMT_Items_Resource_Metal.xml` 追加 6 个 ThingDef（`</Defs>` 前插入）**

```xml
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Anglesite</defName>
    <label>铅矾矿</label>
    <description>铅矾矿是铅的矿石形式。铅是一种重金属，比大多数普通材料密度大。铅柔软而有延展性，熔点相对较低。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ore/MMT_Anglesite/MMT_Anglesite</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
      <color>(165,185,70)</color>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>4</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.3</Mass>
    </statBases>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_AluminiumOre</defName>
    <label>铝土矿</label>
    <description>铝土矿是铝的矿石形式。铝的冶炼无法手工完成，需要电力驱动。铝密度低、抗腐蚀，具有顺磁性。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ore/MMT_AluminiumOre/MMT_AluminiumOre</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
      <color>(255,200,200)</color>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>2</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.1</Mass>
    </statBases>
    <stackLimit>600</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_LeadBar</defName>
    <label>铅合金</label>
    <description>由铅矾矿冶炼出的铅锭。密度大的重金属，柔软有延展性，熔点相对较低。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ingot/MMT_Ingot</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
      <color>(129,107,100)</color>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>6</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.4</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>MMT_HeavyBar</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Metallic</li>
      </categories>
      <stuffAdjective>铅制</stuffAdjective>
      <color>(114,92,83)</color>
      <constructEffect>ConstructMetal</constructEffect>
      <statOffsets>
        <Beauty>0</Beauty>
      </statOffsets>
      <statFactors>
        <MaxHitPoints>1.35</MaxHitPoints>
        <Flammability>0.1</Flammability>
        <Beauty>0.7</Beauty>
        <WorkToMake>1.7</WorkToMake>
        <WorkToBuild>1.7</WorkToBuild>
        <MeleeWeapon_CooldownMultiplier>1.5</MeleeWeapon_CooldownMultiplier>
        <MoveSpeed>0.8</MoveSpeed>
        <MarketValue>0.7</MarketValue>
        <BedRestEffectiveness>0.4</BedRestEffectiveness>
        <ImmunityGainSpeedFactor>0.25</ImmunityGainSpeedFactor>
        <WorkTableWorkSpeedFactor>0.8</WorkTableWorkSpeedFactor>
        <DoorOpenSpeed>0.6</DoorOpenSpeed>
        <MeleePenetrationFactor>0.45</MeleePenetrationFactor>
        <Mass>1.2</Mass>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_AluminiumBar</defName>
    <label>铝合金</label>
    <description>由铝土矿电解冶炼出的铝锭。类似铜但不那么贵重，因高导电性广泛用于电气产业。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ingot/MMT_Ingot</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
      <color>(190,205,190)</color>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.1</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>MMT_HCMBar</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Metallic</li>
      </categories>
      <stuffAdjective>铝制</stuffAdjective>
      <color>(175,175,175)</color>
      <constructEffect>ConstructMetal</constructEffect>
      <statOffsets>
        <Beauty>3</Beauty>
      </statOffsets>
      <statFactors>
        <MaxHitPoints>1.4</MaxHitPoints>
        <Flammability>0</Flammability>
        <Beauty>1.1</Beauty>
        <WorkToMake>1.45</WorkToMake>
        <WorkToBuild>1.45</WorkToBuild>
        <DoorOpenSpeed>1.4</DoorOpenSpeed>
        <MeleeWeapon_CooldownMultiplier>0.7</MeleeWeapon_CooldownMultiplier>
        <MoveSpeed>1.05</MoveSpeed>
        <MarketValue>0.7</MarketValue>
        <BedRestEffectiveness>1</BedRestEffectiveness>
        <ImmunityGainSpeedFactor>1.05</ImmunityGainSpeedFactor>
        <WorkTableWorkSpeedFactor>1.2</WorkTableWorkSpeedFactor>
        <MeleePenetrationFactor>0.7</MeleePenetrationFactor>
        <Mass>0.85</Mass>
        <Cleanliness>0.6</Cleanliness>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_FerrosiliconAlloy</defName>
    <label>斯特合金</label>
    <description>铁硅铝三元合金，可用作其他金属的脱氧剂，比标准铸铁或钢更耐磨。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ingot/MMT_Ingot</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
      <color>(131,137,147)</color>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>5</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.3</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>MMT_SLDBar</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Metallic</li>
      </categories>
      <stuffAdjective>斯特制</stuffAdjective>
      <color>(101,107,117)</color>
      <constructEffect>ConstructMetal</constructEffect>
      <statFactors>
        <MaxHitPoints>2.1</MaxHitPoints>
        <Beauty>0.8</Beauty>
        <Flammability>0</Flammability>
        <WorkToMake>1.3</WorkToMake>
        <WorkToBuild>1.3</WorkToBuild>
        <DoorOpenSpeed>0.8</DoorOpenSpeed>
        <MeleeWeapon_CooldownMultiplier>1.1</MeleeWeapon_CooldownMultiplier>
        <MoveSpeed>0.8</MoveSpeed>
        <MarketValue>0.7</MarketValue>
        <BedRestEffectiveness>0.85</BedRestEffectiveness>
        <ImmunityGainSpeedFactor>0.9</ImmunityGainSpeedFactor>
        <WorkTableWorkSpeedFactor>1.1</WorkTableWorkSpeedFactor>
        <MeleePenetrationFactor>1.2</MeleePenetrationFactor>
        <Mass>1.2</Mass>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_MetalCan</defName>
    <label>金属罐</label>
    <description>做罐头食品所需的空金属罐。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MetalCan</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>1.7</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0.2</Flammability>
      <Mass>0.1</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>200</stackLimit>
    <thingCategories>
      <li>Manufactured</li>
    </thingCategories>
  </ThingDef>
```

- [ ] **Step 3: 既有三锭类别改 `MMT_SLDBar`**

在 `MMT_Items_Resource_Metal.xml` 中对 `MMT_CopperBar`、`MMT_TinBar`、`MMT_Bronze` 三个 ThingDef 各执行一处替换（三处相同，逐个用 Edit 并带上各自 defName 上下文唯一定位）：

```xml
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
```
改为：
```xml
    <thingCategories>
      <li>MMT_SLDBar</li>
    </thingCategories>
```

注意：矿石（IronOre/CopperOre/TinOre）的 `ResourcesRaw` 不变，只改三个锭。

- [ ] **Step 4: `MMT_Buildings_Metal.xml` 追加 2 条矿脉（`</Defs>` 前插入）+ 熔炉 recipes 追加**

熔炉 `<recipes>` 列表末尾（`<li>MMT_SmeltGold_Hand</li>` 后）加：`<li>MMT_SmeltLeadBar_Hand</li>`

矿脉 XML：

```xml
  <ThingDef ParentName="RockBase">
    <defName>MMT_MineableAnglesite</defName>
    <label>铅矾矿脉</label>
    <description>露头的铅矾矿矿脉，开采后可获得铅矾矿，用于冶炼铅合金。</description>
    <graphicData>
      <texPath>Things/Building/Linked/RockFlecked_Atlas</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <color>(89,74,68)</color>
      <colorTwo>(134,148,63)</colorTwo>
    </graphicData>
    <statBases>
      <MaxHitPoints>1600</MaxHitPoints>
    </statBases>
    <building>
      <isResourceRock>true</isResourceRock>
      <mineableThing>MMT_Anglesite</mineableThing>
      <mineableYield>75</mineableYield>
      <mineableScatterCommonality>0.8</mineableScatterCommonality>
      <mineableScatterLumpSizeRange>6~13</mineableScatterLumpSizeRange>
      <veinMineable>true</veinMineable>
    </building>
  </ThingDef>

  <ThingDef ParentName="RockBase">
    <defName>MMT_MineableAluminium</defName>
    <label>铝土矿脉</label>
    <description>露头的铝土矿矿脉，开采后可获得铝土矿，铝只能在电力驱动下冶炼。</description>
    <graphicData>
      <texPath>Things/Building/Linked/RockFlecked_Atlas</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <color>(124,124,110)</color>
      <colorTwo>(175,175,165)</colorTwo>
    </graphicData>
    <statBases>
      <MaxHitPoints>1600</MaxHitPoints>
    </statBases>
    <building>
      <isResourceRock>true</isResourceRock>
      <mineableThing>MMT_AluminiumOre</mineableThing>
      <mineableYield>75</mineableYield>
      <mineableScatterCommonality>0.65</mineableScatterCommonality>
      <mineableScatterLumpSizeRange>11~19</mineableScatterLumpSizeRange>
      <veinMineable>true</veinMineable>
    </building>
  </ThingDef>
```

- [ ] **Step 5: `MMT_Recipes_Metal.xml` 追加手炼铅配方（`</Defs>` 前插入）**

```xml
  <RecipeDef>
    <defName>MMT_SmeltLeadBar_Hand</defName>
    <label>提炼铅（10个）</label>
    <description>将铅矾矿冶炼成铅合金。产出 10。</description>
    <jobString>正在冶炼铅合金。</jobString>
    <workAmount>1100</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Anglesite</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Anglesite</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_LeadBar>10</MMT_LeadBar>
    </products>
    <skillRequirements>
      <Crafting>2</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_BasicMetalworking</researchPrerequisite>
  </RecipeDef>
```

- [ ] **Step 6: `MMT_Recipes_Electric.xml` 追加 4 条电配方（`</Defs>` 前插入）**

```xml
  <RecipeDef>
    <defName>MMT_SmeltLeadBar_Electric</defName>
    <label>提炼铅（20个）</label>
    <description>将铅矾矿批量冶炼成铅合金。产出 20。</description>
    <jobString>正在冶炼铅合金。</jobString>
    <workAmount>1000</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Anglesite</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Anglesite</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_LeadBar>20</MMT_LeadBar>
    </products>
    <skillRequirements>
      <Crafting>6</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ElectricSmelting</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_SmeltAluminiumBar_Electric</defName>
    <label>提炼铝（20个）</label>
    <description>电解铝土矿冶炼成铝合金。产出 20。</description>
    <jobString>正在冶炼铝合金。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_AluminiumOre</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_AluminiumOre</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_AluminiumBar>20</MMT_AluminiumBar>
    </products>
    <skillRequirements>
      <Crafting>7</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ElectricSmelting</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeFerrosiliconAlloy</defName>
    <label>冶炼斯特合金（30个）</label>
    <description>将钢铁、硅和铝合金合炼成斯特合金。产出 30。</description>
    <jobString>正在冶炼斯特合金。</jobString>
    <workAmount>1700</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>Steel</li>
          </thingDefs>
        </filter>
        <count>15</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Silicon</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_AluminiumBar</li>
          </thingDefs>
        </filter>
        <count>5</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>Steel</li>
        <li>MMT_Silicon</li>
        <li>MMT_AluminiumBar</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_FerrosiliconAlloy>30</MMT_FerrosiliconAlloy>
    </products>
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ElectricSmelting</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeMetalCan</defName>
    <label>制作金属罐（15个）</label>
    <description>将金属锭压制成空金属罐。产出 15。</description>
    <jobString>正在制作金属罐。</jobString>
    <workAmount>800</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>MMT_MetallicBar</li>
          </categories>
        </filter>
        <count>8</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>MMT_MetallicBar</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_MetalCan>15</MMT_MetalCan>
    </products>
    <skillRequirements>
      <Crafting>2</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_ElectricSmelting</researchPrerequisite>
  </RecipeDef>
```

- [ ] **Step 7: EAF recipes 追加 4 条**

`MMT_Buildings_Electric.xml` 第 50 行 `<li>MMT_ExtractSilicon_Electric</li>` 之后（高级合金配方之前）插入：

```xml
      <li>MMT_SmeltLeadBar_Electric</li>
      <li>MMT_SmeltAluminiumBar_Electric</li>
      <li>MMT_MakeFerrosiliconAlloy</li>
      <li>MMT_MakeMetalCan</li>
```

- [ ] **Step 8: 同步 `tools/validate_mod.py` 现有套件（关键：不改则 metal/electric 套件全红）**

8a. `METAL_ORE_EXPECTATIONS`（523-527 行）：三个既有条目各加 `"value": "2.0", "stack": "600"` 字段，并追加两条：

```python
METAL_ORE_EXPECTATIONS = {
    "MMT_IronOre": {"label": "铁矿石", "tex": "Things/Item/MMT/MMT_Ore/MMT_IronOre/MMT_IronOre", "value": "2.0", "stack": "600"},
    "MMT_CopperOre": {"label": "铜矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_CopperOre/MMT_CopperOre", "value": "2.0", "stack": "600"},
    "MMT_TinOre": {"label": "锡矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_TinOre/MMT_TinOre", "value": "2.0", "stack": "600"},
    "MMT_Anglesite": {"label": "铅矾矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_Anglesite/MMT_Anglesite", "value": "4", "stack": "300"},
    "MMT_AluminiumOre": {"label": "铝土矿", "tex": "Things/Item/MMT/MMT_Ore/MMT_AluminiumOre/MMT_AluminiumOre", "value": "2", "stack": "600"},
}
```

8b. `METAL_BAR_EXPECTATIONS`（529-533 行）：三个既有条目各加 `"category": "MMT_SLDBar"` 字段，并追加三条：

```python
    "MMT_LeadBar": {"label": "铅合金", "value": "6", "stack": "300", "adjective": "铅制", "hp": "1.35", "flammable": "0.1", "work": "1.7", "category": "MMT_HeavyBar"},
    "MMT_AluminiumBar": {"label": "铝合金", "value": "3", "stack": "300", "adjective": "铝制", "hp": "1.4", "flammable": "0", "work": "1.45", "category": "MMT_HCMBar"},
    "MMT_FerrosiliconAlloy": {"label": "斯特合金", "value": "5", "stack": "300", "adjective": "斯特制", "hp": "2.1", "flammable": "0", "work": "1.3", "category": "MMT_SLDBar"},
```

8c. `validate_metal_resources`（1877-1903 行）三处修改：

```python
    # 1877 行：set 断言加入 MetalCan
    assert_equal(set(defs), set(METAL_ORE_EXPECTATIONS) | set(METAL_BAR_EXPECTATIONS) | {"MMT_MetalCan"}, "金属资源 defName 集合")

    # ore 循环（1882-1883 行）：硬编码改读期望字段
    assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
    assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")

    # bar 循环（1897 行）：类别断言改读期望字段
    assert_equal(li_values(node, "thingCategories"), [expected["category"]], f"{name} thingCategories")
```

8d. `METAL_MINEABLE_EXPECTATIONS`（545-549 行）追加：

```python
    "MMT_MineableAnglesite": {"label": "铅矾矿脉", "yield_thing": "MMT_Anglesite", "hp": "1600"},
    "MMT_MineableAluminium": {"label": "铝土矿脉", "yield_thing": "MMT_AluminiumOre", "hp": "1600"},
```

8e. `METAL_FURNACE_RECIPES`（535-543 行）末尾追加：`"MMT_SmeltLeadBar_Hand",`

8f. `METAL_RECIPE_EXPECTATIONS`（551-580 行）追加：

```python
    "MMT_SmeltLeadBar_Hand": {
        "work": "1100",
        "inputs": [("thingDefs", "MMT_Anglesite", "10")],
        "output": "MMT_LeadBar",
        "output_count": "10",
        "skill": "2",
    },
```

8g. `ELECTRIC_FURNACE_RECIPES`（735-743 行）末尾追加：

```python
    "MMT_SmeltLeadBar_Electric",
    "MMT_SmeltAluminiumBar_Electric",
    "MMT_MakeFerrosiliconAlloy",
    "MMT_MakeMetalCan",
```

8h. `ELECTRIC_RECIPE_EXPECTATIONS`（745-795 行）追加三条（**不含** MakeMetalCan，它是 categories 输入，违反该套件"输入只含 thingDefs"断言）：

```python
    "MMT_SmeltLeadBar_Electric": {
        "work": "1000",
        "inputs": [("thingDefs", "MMT_Anglesite", "20")],
        "output": "MMT_LeadBar",
        "output_count": "20",
        "skill": "6",
    },
    "MMT_SmeltAluminiumBar_Electric": {
        "work": "1200",
        "inputs": [("thingDefs", "MMT_AluminiumOre", "20")],
        "output": "MMT_AluminiumBar",
        "output_count": "20",
        "skill": "7",
    },
    "MMT_MakeFerrosiliconAlloy": {
        "work": "1700",
        "inputs": [("thingDefs", "Steel", "15"), ("thingDefs", "MMT_Silicon", "10"), ("thingDefs", "MMT_AluminiumBar", "5")],
        "output": "MMT_FerrosiliconAlloy",
        "output_count": "30",
        "skill": "8",
    },
```

8i. `validate_electric`（2421 行）：集合断言加 MakeMetalCan 例外：

```python
    assert_equal(set(recipe_defs), set(ELECTRIC_RECIPE_EXPECTATIONS) | {"MMT_MakeMetalCan"}, "电动配方 defName 集合")
```

8j. `validate_electric`（2423-2430 行）：resource_names 并入 electronics（斯特配方输入 `MMT_Silicon`），输入白名单并入 VANILLA_THINGS（斯特配方输入 `Steel`）：

```python
    resource_names = (
        set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(WOOD_RESOURCE_XML)))
        | set(def_map(parse_xml(COAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PRECIOUS_RESOURCE_XML)))
        | set(def_map(parse_xml(RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
    )
    output_names = resource_names | VANILLA_THINGS
```

```python
        # 2480 行：
        if item not in resource_names | VANILLA_THINGS:
            fail(f"{name} 使用未知资源: {item}")
```

- [ ] **Step 9: 运行验证**

```bash
cd D:/project/rw
PYTHONIOENCODING=utf-8 python tools/validate_mod.py metal
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all-xml
```

预期：三个套件全 OK。若失败，按输出修正后重跑。

- [ ] **Step 10: Commit**

```bash
cd D:/project/rw
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Metal.xml \
        1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Metal.xml \
        1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml \
        1.7/Defs/RecipeDefs/MMT_Recipes_Metal.xml \
        1.7/Defs/RecipeDefs/MMT_Recipes_Electric.xml \
        tools/validate_mod.py \
        1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_Anglesite \
        1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_AluminiumOre \
        1.7/Textures/Things/Item/MMT/MetalCan.png
git status --short   # 确认暂存区只有本批内容；若混入用户 WIP 先报告
git commit -m "feat: 添加铅铝斯特金属罐资源与冶炼配方

Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 陶瓷链（MMT_Bricks + 软粘土/粘土砖/陶瓷 + 粘土矿脉 + 窑炉 + 2 配方）

**Files:**
- Create: `1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml`
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml`
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml`（EAF recipes 再加 2 条）
- Modify: `tools/validate_mod.py:735`（`ELECTRIC_FURNACE_RECIPES` 加 2 条）
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ceramics/{SoftClay,ClayBrick,Ceramics}.png`、`1.7/Textures/Things/Building/Production/MMT_Kiln/MMT_Kiln_{east,north,south}{,m}.png`

**Interfaces:**
- Consumes: Task 1 的 `MMT_ElectricArcFurnace` recipes 列表位置；既有 `MMT_GlassBatch`（玻璃生料）；`MMT_BasicMetalworking` 研究。
- Produces: `MMT_Bricks`（stuff 类别）；`MMT_SoftClay`、`MMT_ClayBrick`、`MMT_Ceramics`；`MMT_MineableSoftClay`；`MMT_Kiln`（Task 3 向其 recipes 追加 `MMT_BurnThings`）；`MMT_MakeCeramics`、`MMT_MakeClayBricks`。

- [ ] **Step 1: 复制贴图**

```bash
cd D:/project/rw
mkdir -p 1.7/Textures/Things/Item/MMT/MMT_Ceramics 1.7/Textures/Things/Building/Production/MMT_Kiln
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/SoftClay.png"   1.7/Textures/Things/Item/MMT/MMT_Ceramics/SoftClay.png
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/ClayBricks.png" 1.7/Textures/Things/Item/MMT/MMT_Ceramics/ClayBrick.png
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Ceramics.png"   1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ceramics.png
for side in east north south; do
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_${side}.png"  "1.7/Textures/Things/Building/Production/MMT_Kiln/MMT_Kiln_${side}.png"
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/Kiln/Kiln_${side}m.png" "1.7/Textures/Things/Building/Production/MMT_Kiln/MMT_Kiln_${side}m.png"
done
```

- [ ] **Step 2: 新建 `1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <!-- 砖块建材类别：对应 HSK Bricks，供粘土砖与窑炉建造材质使用 -->
  <StuffCategoryDef>
    <defName>MMT_Bricks</defName>
    <label>砖块</label>
  </StuffCategoryDef>
</Defs>
```

- [ ] **Step 3: 新建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml`（本 Task 只含 3 个资源，灰烬在 Task 3 追加）**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_SoftClay</defName>
    <label>软粘土</label>
    <description>从粘土矿脉开采的未加工粘土，可以在窑炉中烧制成粘土砖或陶瓷。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ceramics/SoftClay</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Stone_Drop</soundInteract>
    <soundDrop>Stone_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>1</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.15</Mass>
    </statBases>
    <stackLimit>600</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_ClayBrick</defName>
    <label>粘土砖</label>
    <description>在窑炉中烧制而成的坚硬粘土砖，可以用作建筑材料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ceramics/ClayBrick</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Stone_Drop</soundInteract>
    <soundDrop>Stone_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.5</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>Manufactured</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>MMT_Bricks</li>
      </categories>
      <stuffAdjective>粘土砖</stuffAdjective>
      <color>(153,87,61)</color>
      <statOffsets>
        <WorkToMake>70</WorkToMake>
        <WorkToBuild>70</WorkToBuild>
        <Beauty>1.5</Beauty>
      </statOffsets>
      <statFactors>
        <MaxHitPoints>1.4</MaxHitPoints>
        <Beauty>1.35</Beauty>
        <Flammability>0</Flammability>
        <WorkToMake>1.3</WorkToMake>
        <WorkToBuild>1.3</WorkToBuild>
        <WorkTableWorkSpeedFactor>1.0</WorkTableWorkSpeedFactor>
        <Mass>1.3</Mass>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Ceramics</defName>
    <label>陶瓷</label>
    <description>用高温窑炉制造的釉陶瓷，表面光滑美观但易碎。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ceramics/Ceramics</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <soundInteract>Stone_Drop</soundInteract>
    <soundDrop>Stone_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>12</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.4</Mass>
    </statBases>
    <useHitPoints>true</useHitPoints>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>Manufactured</li>
    </thingCategories>
    <stuffProps>
      <categories>
        <li>Stony</li>
      </categories>
      <stuffAdjective>陶瓷</stuffAdjective>
      <color>(255,255,255)</color>
      <statOffsets>
        <WorkToMake>120</WorkToMake>
        <WorkToBuild>110</WorkToBuild>
        <Beauty>3</Beauty>
      </statOffsets>
      <statFactors>
        <MaxHitPoints>0.6</MaxHitPoints>
        <Beauty>1.7</Beauty>
        <Flammability>0</Flammability>
        <WorkToMake>1.3</WorkToMake>
        <WorkToBuild>1.3</WorkToBuild>
        <WorkTableWorkSpeedFactor>1.0</WorkTableWorkSpeedFactor>
        <MeleePenetrationFactor>0.9</MeleePenetrationFactor>
        <Mass>0.8</Mass>
        <Cleanliness>0.8</Cleanliness>
      </statFactors>
    </stuffProps>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
  </ThingDef>
</Defs>
```

- [ ] **Step 4: 新建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml`（粘土矿脉 + 窑炉）**

注意：窑炉 recipes 本 Task 只含 2 条陶瓷配方，`MMT_BurnThings` 在 Task 3 追加（Review Focus #5）。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="RockBase">
    <defName>MMT_MineableSoftClay</defName>
    <label>粘土矿脉</label>
    <description>露头的软粘土矿脉，开采后可获得软粘土，用于烧制粘土砖和陶瓷。</description>
    <graphicData>
      <texPath>Things/Building/Linked/RockFlecked_Atlas</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <color>(121,96,76)</color>
      <colorTwo>(168,134,105)</colorTwo>
    </graphicData>
    <statBases>
      <MaxHitPoints>1400</MaxHitPoints>
    </statBases>
    <building>
      <isResourceRock>true</isResourceRock>
      <mineableThing>MMT_SoftClay</mineableThing>
      <mineableYield>75</mineableYield>
      <mineableScatterCommonality>0.7</mineableScatterCommonality>
      <mineableScatterLumpSizeRange>8~15</mineableScatterLumpSizeRange>
      <veinMineable>true</veinMineable>
    </building>
  </ThingDef>

  <!-- 窑炉：对标 HSK Kiln，SK 燃料组件用原版 CompProperties_Refuelable 替代 -->
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_Kiln</defName>
    <label>窑炉</label>
    <description>烧柴煤的土窑，用于烧制粘土砖、陶瓷，也可焚烧杂物获取灰烬。可用石砖或粘土砖建造。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_Kiln/MMT_Kiln</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(3,3)</drawSize>
    </graphicData>
    <uiIconPath>Things/Building/Production/MMT_Kiln/MMT_Kiln_north</uiIconPath>
    <passability>PassThroughOnly</passability>
    <pathCost>90</pathCost>
    <fillPercent>0.9</fillPercent>
    <canOverlapZones>false</canOverlapZones>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-2)</interactionCellOffset>
    <useStuffTerrainAffordance>true</useStuffTerrainAffordance>
    <stuffCategories>
      <li>Stony</li>
      <li>MMT_Bricks</li>
    </stuffCategories>
    <costStuffCount>180</costStuffCount>
    <minifiedDef>MinifiedThing</minifiedDef>
    <thingCategories>
      <li>BuildingsProduction</li>
    </thingCategories>
    <statBases>
      <WorkToBuild>3000</WorkToBuild>
      <MaxHitPoints>270</MaxHitPoints>
      <Flammability>0</Flammability>
      <Cleanliness>-8</Cleanliness>
      <Mass>500</Mass>
    </statBases>
    <size>(3,3)</size>
    <designationCategory>MMT_Production</designationCategory>
    <recipes>
      <li>MMT_MakeCeramics</li>
      <li>MMT_MakeClayBricks</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <building>
      <uninstallWork>1850</uninstallWork>
    </building>
    <comps>
      <li Class="CompProperties_Refuelable">
        <fuelConsumptionRate>10.0</fuelConsumptionRate>
        <fuelCapacity>30.0</fuelCapacity>
        <fuelFilter>
          <thingDefs>
            <li>MMT_Coal</li>
            <li>MMT_Charcoal</li>
            <li>MMT_Kindling</li>
            <li>WoodLog</li>
          </thingDefs>
        </fuelFilter>
        <showAllowAutoRefuelToggle>true</showAllowAutoRefuelToggle>
      </li>
      <li Class="CompProperties_Glower">
        <glowRadius>3</glowRadius>
        <glowColor>(217,112,33,0)</glowColor>
      </li>
      <li Class="CompProperties_Flickable" />
    </comps>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_BasicMetalworking</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>4</constructionSkillPrerequisite>
  </ThingDef>
</Defs>
```

- [ ] **Step 5: 新建 `1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml`（本 Task 只含 2 条）**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <RecipeDef>
    <defName>MMT_MakeCeramics</defName>
    <label>烧制陶瓷（10个）</label>
    <description>将软粘土和玻璃生料烧制成釉陶瓷。产出 10。</description>
    <jobString>正在烧制陶瓷。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_SoftClay</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_GlassBatch</li>
          </thingDefs>
        </filter>
        <count>5</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_SoftClay</li>
        <li>MMT_GlassBatch</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Ceramics>10</MMT_Ceramics>
    </products>
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_BasicMetalworking</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeClayBricks</defName>
    <label>烧制粘土砖（15个）</label>
    <description>将软粘土烧制成粘土砖。产出 15。</description>
    <jobString>正在烧制粘土砖。</jobString>
    <workAmount>800</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_SoftClay</li>
          </thingDefs>
        </filter>
        <count>15</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_SoftClay</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_ClayBrick>15</MMT_ClayBrick>
    </products>
    <skillRequirements>
      <Crafting>6</Crafting>
    </skillRequirements>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_BasicMetalworking</researchPrerequisite>
  </RecipeDef>
</Defs>
```

- [ ] **Step 6: EAF recipes 追加 2 条 + 同步 validate**

`MMT_Buildings_Electric.xml` 在 Task 1 加的 `<li>MMT_MakeMetalCan</li>` 后追加：

```xml
      <li>MMT_MakeCeramics</li>
      <li>MMT_MakeClayBricks</li>
```

`tools/validate_mod.py` 的 `ELECTRIC_FURNACE_RECIPES` 末尾（Task 1 加的 4 条之后）追加：

```python
    "MMT_MakeCeramics",
    "MMT_MakeClayBricks",
```

- [ ] **Step 7: 运行验证**

```bash
cd D:/project/rw
PYTHONIOENCODING=utf-8 python tools/validate_mod.py electric
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all-xml
```

预期：全 OK。

- [ ] **Step 8: Commit**

```bash
cd D:/project/rw
git add 1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml \
        1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml \
        1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml \
        1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml \
        1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Electric.xml \
        tools/validate_mod.py \
        1.7/Textures/Things/Item/MMT/MMT_Ceramics \
        1.7/Textures/Things/Building/Production/MMT_Kiln
git status --short   # 确认无 WIP 混入
git commit -m "feat: 添加窑炉与陶瓷粘土砖软粘土链

Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 灰烬资源 + 燃烧配方 + 原版火化 patch

**Files:**
- Modify: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml`（追加 `MMT_Ash`）
- Modify: `1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml`（追加 2 配方）
- Modify: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml`（窑炉 recipes 加 `MMT_BurnThings`）
- Create: `1.7/Patches/MMT_Patch_AshRecipes.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash/MMT_Ash_{a,b,c}.png`

**Interfaces:**
- Consumes: Task 2 的 `MMT_Kiln`、`MMT_Recipes_Ceramics.xml`；原版 `Campfire`/`ElectricCrematorium`/`BurnApparel`/`CremateCorpse`。
- Produces: `MMT_Ash`（B3 堆肥/B5 硝石的输入）；`MMT_BurnThings`、`MMT_CremateCorpseCampfire`；灰烬 patch（Task 4 验证其 xpath 目标）。

- [ ] **Step 1: 复制灰烬贴图**

```bash
cd D:/project/rw
mkdir -p 1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash
for s in a b c; do
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Resource/Ash/Ash_${s}.png" "1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash/MMT_Ash_${s}.png"
done
```

- [ ] **Step 2: `MMT_Items_Resource_Ceramics.xml` 追加灰烬（`</Defs>` 前插入）**

```xml
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Ash</defName>
    <label>灰烬</label>
    <description>在火化炉或篝火中焚烧尸体或其他材料得到的灰烬，可用作农业和化学原料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/MMT_Ceramics/Ash/MMT_Ash</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
    </graphicData>
    <soundInteract>Metal_Drop</soundInteract>
    <soundDrop>Metal_Drop</soundDrop>
    <tickerType>Rare</tickerType>
    <statBases>
      <MarketValue>0.3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
      <Mass>0.05</Mass>
      <Beauty>-5</Beauty>
    </statBases>
    <stackLimit>600</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
    <tradeability>None</tradeability>
  </ThingDef>
```

- [ ] **Step 3: `MMT_Recipes_Ceramics.xml` 追加 2 条燃烧配方（`</Defs>` 前插入）**

```xml
  <RecipeDef>
    <defName>MMT_BurnThings</defName>
    <label>焚烧材料</label>
    <description>焚烧木材、织物和皮革获取灰烬。产出 3。</description>
    <jobString>正在焚烧材料。</jobString>
    <workAmount>400</workAmount>
    <workSpeedStat>GeneralLaborSpeed</workSpeedStat>
    <requiredGiverWorkType>Hauling</requiredGiverWorkType>
    <effectWorking>Cremate</effectWorking>
    <soundWorking>Recipe_Cremate</soundWorking>
    <recipeUsers>
      <li>Campfire</li>
      <li>ElectricCrematorium</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>Textiles</li>
            <li>Leathers</li>
          </categories>
          <thingDefs>
            <li>WoodLog</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>Textiles</li>
        <li>Leathers</li>
      </categories>
      <thingDefs>
        <li>WoodLog</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Ash>3</MMT_Ash>
    </products>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_CremateCorpseCampfire</defName>
    <label>火化尸体</label>
    <description>在篝火上火化尸体，获得灰烬。产出 4。</description>
    <jobString>正在火化。</jobString>
    <workAmount>800</workAmount>
    <workSpeedStat>GeneralLaborSpeed</workSpeedStat>
    <effectWorking>Cremate</effectWorking>
    <soundWorking>Recipe_Cremate</soundWorking>
    <autoStripCorpses>false</autoStripCorpses>
    <recipeUsers>
      <li>Campfire</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>Corpses</li>
          </categories>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>Corpses</li>
      </categories>
      <disallowedCategories>
        <li>CorpsesMechanoid</li>
      </disallowedCategories>
    </fixedIngredientFilter>
    <defaultIngredientFilter>
      <thingDefs>
        <li>Corpse_Human</li>
      </thingDefs>
      <specialFiltersToAllow>
        <li>AllowCorpsesColonist</li>
        <li MayRequire="Ludeon.RimWorld.Ideology">AllowCorpsesSlave</li>
        <li>AllowCorpsesStranger</li>
      </specialFiltersToAllow>
    </defaultIngredientFilter>
    <products>
      <MMT_Ash>4</MMT_Ash>
    </products>
  </RecipeDef>
```

- [ ] **Step 4: 窑炉 recipes 追加 `MMT_BurnThings`**

`MMT_Buildings_Ceramics.xml` 窑炉的 `<recipes>` 中 `<li>MMT_MakeClayBricks</li>` 后加：`<li>MMT_BurnThings</li>`

- [ ] **Step 5: 新建 `1.7/Patches/MMT_Patch_AshRecipes.xml`**

原版两条配方无 `<products>` 节点：先用 Conditional 的 nomatch 建节点，再插入产出（Review Focus #4）。只追加产出，不改原版其他字段。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 灰烬产出：对齐 HSK 对 BurnApparel(+2)/CremateCorpse(+4) 的 patch，只加 products 不动其他字段 -->
  <Operation Class="PatchOperationSequence">
    <operations>
      <li Class="PatchOperationConditional">
        <xpath>Defs/RecipeDef[defName="BurnApparel"]/products</xpath>
        <nomatch Class="PatchOperationAdd">
          <xpath>Defs/RecipeDef[defName="BurnApparel"]</xpath>
          <value><products /></value>
        </nomatch>
      </li>
      <li Class="PatchOperationAdd">
        <xpath>Defs/RecipeDef[defName="BurnApparel"]/products</xpath>
        <value><MMT_Ash>2</MMT_Ash></value>
      </li>
    </operations>
  </Operation>

  <Operation Class="PatchOperationSequence">
    <operations>
      <li Class="PatchOperationConditional">
        <xpath>Defs/RecipeDef[defName="CremateCorpse"]/products</xpath>
        <nomatch Class="PatchOperationAdd">
          <xpath>Defs/RecipeDef[defName="CremateCorpse"]</xpath>
          <value><products /></value>
        </nomatch>
      </li>
      <li Class="PatchOperationAdd">
        <xpath>Defs/RecipeDef[defName="CremateCorpse"]/products</xpath>
        <value><MMT_Ash>4</MMT_Ash></value>
      </li>
    </operations>
  </Operation>
</Patch>
```

- [ ] **Step 6: 运行验证**

```bash
cd D:/project/rw && PYTHONIOENCODING=utf-8 python tools/validate_mod.py all-xml
```

预期：OK。

- [ ] **Step 7: Commit**

```bash
cd D:/project/rw
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml \
        1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml \
        1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml \
        1.7/Patches/MMT_Patch_AshRecipes.xml \
        1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash
git status --short   # 确认无 WIP 混入
git commit -m "feat: 添加灰烬资源与燃烧配方 patch

Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: basic-metallurgy 验证套件 + 对比文档更新 + 全量验证

**Files:**
- Modify: `tools/validate_mod.py`（路径常量 + `validate_basic_metallurgy()` + SUITES/validate_all 注册）
- Modify: `docs/mmt-vs-hsk-resource-chains.md`（手工追加 B2 条目，该文件未跟踪入 git）

**Interfaces:**
- Consumes: Task 1–3 全部产物；validate_mod.py 现有辅助函数 `parse_xml`/`def_map`/`child_text`/`li_values`/`assert_equal`/`require_files`/`assert_png`/`read_text`/`fail`。
- Produces: `basic-metallurgy` 套件（B4 亚力可/斯特前置的回归基准）。

- [ ] **Step 1: `tools/validate_mod.py` 顶部路径常量追加（放在 65 行 `ELECTRIC_ELECTRONICS_RECIPE_XML` 之后）**

```python
CERAMICS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Ceramics.xml"
CERAMICS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Ceramics.xml"
CERAMICS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Ceramics.xml"
STUFFCATEGORY_XML = "1.7/Defs/StuffCategoryDefs/MMT_StuffCategories.xml"
ASH_PATCH_XML = "1.7/Patches/MMT_Patch_AshRecipes.xml"

CERAMICS_ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/SoftClay.png",
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/ClayBrick.png",
    "1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ceramics.png",
    "1.7/Textures/Things/Item/MMT/MetalCan.png",
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ceramics/Ash/MMT_Ash_{s}.png" for s in ("a", "b", "c")],
    *[f"1.7/Textures/Things/Item/MMT/MMT_Ore/MMT_{ore}/MMT_{ore}_{s}.png" for ore in ("Anglesite", "AluminiumOre") for s in ("a", "b", "c")],
]

CERAMICS_BUILDING_TEXTURES = [
    f"1.7/Textures/Things/Building/Production/MMT_Kiln/MMT_Kiln_{side}{mask}.png"
    for side in ("east", "north", "south")
    for mask in ("", "m")
]
```

- [ ] **Step 2: 新增 `validate_basic_metallurgy()`（放在 `validate_all` 之前）**

```python
def validate_basic_metallurgy() -> None:
    """B2 基础冶金：陶瓷链资源/窑炉/燃烧配方/灰烬 patch 的专项校验。

    金属侧资源（铅/铝/斯特）由 metal/electric 套件覆盖（Task 1 已同步期望），
    本套件聚焦新建文件与例外项（MetalCan、MakeMetalCan 的 categories 输入、灰烬 patch）。
    """
    require_files([
        CERAMICS_RESOURCE_XML,
        CERAMICS_RECIPE_XML,
        CERAMICS_BUILDING_XML,
        STUFFCATEGORY_XML,
        ASH_PATCH_XML,
        METAL_RESOURCE_XML,
        ELECTRIC_BUILDING_XML,
        ELECTRIC_RECIPE_XML,
        *CERAMICS_ITEM_TEXTURES,
        *CERAMICS_BUILDING_TEXTURES,
    ])

    # StuffCategoryDef
    stuffcat_defs = def_map(parse_xml(STUFFCATEGORY_XML))
    assert_equal(set(stuffcat_defs), {"MMT_Bricks"}, "stuff 类别 defName 集合")
    assert_equal(child_text(stuffcat_defs["MMT_Bricks"], "label"), "砖块", "MMT_Bricks label")

    # 陶瓷链资源
    resource_defs = def_map(parse_xml(CERAMICS_RESOURCE_XML))
    assert_equal(set(resource_defs), {"MMT_SoftClay", "MMT_ClayBrick", "MMT_Ceramics", "MMT_Ash"}, "陶瓷资源 defName 集合")

    clay = resource_defs["MMT_SoftClay"]
    assert_equal(child_text(clay, "label"), "软粘土", "软粘土 label")
    assert_equal(child_text(clay, "./statBases/MarketValue"), "1", "软粘土 MarketValue")
    assert_equal(child_text(clay, "stackLimit"), "600", "软粘土 stackLimit")
    assert_equal(li_values(clay, "thingCategories"), ["ResourcesRaw"], "软粘土 thingCategories")

    brick = resource_defs["MMT_ClayBrick"]
    assert_equal(child_text(brick, "label"), "粘土砖", "粘土砖 label")
    assert_equal(child_text(brick, "./statBases/MarketValue"), "3", "粘土砖 MarketValue")
    assert_equal(li_values(brick, "thingCategories"), ["Manufactured"], "粘土砖 thingCategories")
    assert_equal(li_values(brick, "./stuffProps/categories"), ["MMT_Bricks"], "粘土砖 stuff categories")

    ceramics = resource_defs["MMT_Ceramics"]
    assert_equal(child_text(ceramics, "label"), "陶瓷", "陶瓷 label")
    assert_equal(child_text(ceramics, "./statBases/MarketValue"), "12", "陶瓷 MarketValue")
    assert_equal(li_values(ceramics, "thingCategories"), ["Manufactured"], "陶瓷 thingCategories")
    assert_equal(li_values(ceramics, "./stuffProps/categories"), ["Stony"], "陶瓷 stuff categories")

    ash = resource_defs["MMT_Ash"]
    assert_equal(child_text(ash, "label"), "灰烬", "灰烬 label")
    assert_equal(child_text(ash, "./statBases/MarketValue"), "0.3", "灰烬 MarketValue")
    assert_equal(child_text(ash, "./statBases/Beauty"), "-5", "灰烬 Beauty")
    assert_equal(child_text(ash, "tradeability"), "None", "灰烬 tradeability")
    assert_equal(li_values(ash, "thingCategories"), ["ResourcesRaw"], "灰烬 thingCategories")

    # 金属罐（例外项：非 stuff、Manufactured，不在 METAL_BAR_EXPECTATIONS 结构内）
    metal_defs = def_map(parse_xml(METAL_RESOURCE_XML))
    can = metal_defs["MMT_MetalCan"]
    assert_equal(child_text(can, "label"), "金属罐", "金属罐 label")
    assert_equal(child_text(can, "./statBases/MarketValue"), "1.7", "金属罐 MarketValue")
    assert_equal(child_text(can, "stackLimit"), "200", "金属罐 stackLimit")
    assert_equal(li_values(can, "thingCategories"), ["Manufactured"], "金属罐 thingCategories")
    if can.find("stuffProps") is not None:
        fail("金属罐不应定义 stuffProps")

    # 窑炉与粘土矿脉
    building_defs = def_map(parse_xml(CERAMICS_BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_MineableSoftClay", "MMT_Kiln"}, "陶瓷建筑 defName 集合")

    vein = building_defs["MMT_MineableSoftClay"]
    assert_equal(child_text(vein, "./building/mineableThing"), "MMT_SoftClay", "粘土矿脉产出")
    assert_equal(child_text(vein, "./building/mineableYield"), "75", "粘土矿脉产量")
    assert_equal(child_text(vein, "./building/mineableScatterCommonality"), "0.7", "粘土矿脉散布")
    assert_equal(child_text(vein, "./building/mineableScatterLumpSizeRange"), "8~15", "粘土矿脉矿团")

    kiln = building_defs["MMT_Kiln"]
    assert_equal(child_text(kiln, "label"), "窑炉", "窑炉 label")
    assert_equal(child_text(kiln, "thingClass"), "Building_WorkTable", "窑炉 thingClass")
    assert_equal(li_values(kiln, "stuffCategories"), ["Stony", "MMT_Bricks"], "窑炉材料分类")
    assert_equal(child_text(kiln, "costStuffCount"), "180", "窑炉材料数量")
    assert_equal(child_text(kiln, "designationCategory"), "MMT_Production", "窑炉建造分类")
    assert_equal(li_values(kiln, "recipes"), ["MMT_MakeCeramics", "MMT_MakeClayBricks", "MMT_BurnThings"], "窑炉配方")
    assert_equal(li_values(kiln, "researchPrerequisites"), ["MMT_BasicMetalworking"], "窑炉研究前置")
    kiln_comps = [li.attrib.get("Class") for li in kiln.findall("./comps/li")]
    for expected in ("CompProperties_Refuelable", "CompProperties_Glower", "CompProperties_Flickable"):
        if expected not in kiln_comps:
            fail(f"窑炉缺少组件: {expected}")
    fuel = kiln.find("./comps/li[@Class='CompProperties_Refuelable']")
    assert_equal(sorted(li_values(fuel, "./fuelFilter/thingDefs")), ["MMT_Charcoal", "MMT_Coal", "MMT_Kindling", "WoodLog"], "窑炉燃料列表")

    # 陶瓷系配方 + 燃烧配方
    recipe_defs = def_map(parse_xml(CERAMICS_RECIPE_XML))
    assert_equal(set(recipe_defs), {"MMT_MakeCeramics", "MMT_MakeClayBricks", "MMT_BurnThings", "MMT_CremateCorpseCampfire"}, "陶瓷配方 defName 集合")

    make_ceramics = recipe_defs["MMT_MakeCeramics"]
    assert_equal(child_text(make_ceramics, "workAmount"), "1200", "陶瓷 workAmount")
    assert_equal(child_text(make_ceramics, "./skillRequirements/Crafting"), "8", "陶瓷技能")
    assert_equal(child_text(make_ceramics, "researchPrerequisite"), "MMT_BasicMetalworking", "陶瓷研究")

    make_bricks = recipe_defs["MMT_MakeClayBricks"]
    assert_equal(child_text(make_bricks, "workAmount"), "800", "粘土砖 workAmount")
    assert_equal(child_text(make_bricks, "./skillRequirements/Crafting"), "6", "粘土砖技能")
    assert_equal(child_text(make_bricks, "researchPrerequisite"), "MMT_BasicMetalworking", "粘土砖研究")

    burn = recipe_defs["MMT_BurnThings"]
    assert_equal(child_text(burn, "workAmount"), "400", "焚烧 workAmount")
    assert_equal(sorted(li_values(burn, "recipeUsers")), ["Campfire", "ElectricCrematorium"], "焚烧挂载建筑")
    assert_equal(sorted(li_values(burn, "./ingredients/li/filter/categories")), ["Leathers", "Textiles"], "焚烧输入类别")
    assert_equal(li_values(burn, "./ingredients/li/filter/thingDefs"), ["WoodLog"], "焚烧输入物品")
    assert_equal(child_text(burn, "./ingredients/li/count"), "20", "焚烧输入数量")
    ash_products = [(node.tag, (node.text or "").strip()) for node in burn.findall("./products/*")]
    assert_equal(ash_products, [("MMT_Ash", "3")], "焚烧产出")

    cremate = recipe_defs["MMT_CremateCorpseCampfire"]
    assert_equal(child_text(cremate, "workAmount"), "800", "篝火火化 workAmount")
    assert_equal(li_values(cremate, "recipeUsers"), ["Campfire"], "篝火火化挂载")
    cremate_products = [(node.tag, (node.text or "").strip()) for node in cremate.findall("./products/*")]
    assert_equal(cremate_products, [("MMT_Ash", "4")], "篝火火化产出")

    # 陶瓷两条配方同时挂 EAF（HSK 挂 ElectricSmelter+Kiln，MMT 电配方统一挂 EAF）
    eaf = def_map(parse_xml(ELECTRIC_BUILDING_XML))["MMT_ElectricArcFurnace"]
    eaf_recipes = li_values(eaf, "recipes")
    for name in ("MMT_MakeCeramics", "MMT_MakeClayBricks"):
        if name not in eaf_recipes:
            fail(f"{name} 未挂载到电弧炉")

    # MakeMetalCan：唯一的 categories 输入配方（不塞 ELECTRIC_RECIPE_EXPECTATIONS 的原因）
    can_recipe = def_map(parse_xml(ELECTRIC_RECIPE_XML))["MMT_MakeMetalCan"]
    assert_equal(child_text(can_recipe, "workAmount"), "800", "金属罐 workAmount")
    assert_equal(li_values(can_recipe, "./ingredients/li/filter/categories"), ["MMT_MetallicBar"], "金属罐输入类别")
    assert_equal(child_text(can_recipe, "./ingredients/li/count"), "8", "金属罐输入数量")
    can_products = [(node.tag, (node.text or "").strip()) for node in can_recipe.findall("./products/*")]
    assert_equal(can_products, [("MMT_MetalCan", "15")], "金属罐产出")
    assert_equal(child_text(can_recipe, "researchPrerequisite"), "MMT_ElectricSmelting", "金属罐研究")

    # 灰烬 patch：xpath 目标与产出数值
    patch_text = read_text(ASH_PATCH_XML)
    for needle in (
        'Defs/RecipeDef[defName="BurnApparel"]',
        'Defs/RecipeDef[defName="CremateCorpse"]',
        "<MMT_Ash>2</MMT_Ash>",
        "<MMT_Ash>4</MMT_Ash>",
        "PatchOperationConditional",
    ):
        if needle not in patch_text:
            fail(f"灰烬 patch 缺少: {needle}")

    for path in (*CERAMICS_ITEM_TEXTURES, *CERAMICS_BUILDING_TEXTURES):
        assert_png(path)

    for path in (CERAMICS_RESOURCE_XML, CERAMICS_RECIPE_XML, CERAMICS_BUILDING_XML, STUFFCATEGORY_XML, ASH_PATCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("basic-metallurgy: OK")
```

- [ ] **Step 3: 注册套件**

`SUITES` 字典（3428-3464 行）在 `"all"` 之前加一行：

```python
    "basic-metallurgy": validate_basic_metallurgy,
```

`validate_all()`（3405-3425 行）在 `validate_all_xml()` 之前加一行：

```python
    validate_basic_metallurgy()
```

- [ ] **Step 4: 运行新套件**

```bash
cd D:/project/rw && PYTHONIOENCODING=utf-8 python tools/validate_mod.py basic-metallurgy
```

预期：输出 `basic-metallurgy: OK`。失败则按输出修正重跑。

- [ ] **Step 5: 手工更新对比文档 `docs/mmt-vs-hsk-resource-chains.md`（未跟踪文件，不入 commit）**

5a. §1 汇总表数字更新：MMT 矿脉 11 → **14**；MMT 配方 72 → **81**；"与 HSK 数值一致" 34 → **42**（新增 8 条一致：铅手/铅电/铝电/斯特/金属罐/陶瓷/粘土砖/BurnThings，建筑差异不计数值差异——沿用文档现有口径，矿脉耐久差异不算配方差异）；"与 HSK 有数值差异" 38 → **40**（篝火火化为 MMT 自建无 HSK 同口径对照 + 粘土矿脉无 HSK 矿脉对照，计入差异/新增说明列）。

5b. §2 矿脉对比表追加 3 行（插在硫磺矿脉行后）：

```markdown
| 铅矾矿脉(MMT_MineableAnglesite) | 铅矾矿 | 75 | 6~13 / 0.8 / 1600 | Anglesite Deposits(`MineableLead`) | 75 | 6~13 / 0.8 / 6000 | 产量/矿团/散布一致，耐久 1600 vs HSK 6000 |
| 铝土矿脉(MMT_MineableAluminium) | 铝土矿 | 75 | 11~19 / 0.65 / 1600 | Bauxite Ore Deposits(`MineableAluminium`) | 75 | 11~19 / 0.65 / 5500 | 产量/矿团/散布一致，耐久 1600 vs HSK 5500 |
| 粘土矿脉(MMT_MineableSoftClay) | 软粘土 | 75 | 8~15 / 0.7 / 1400 | — | — | — | MMT 新增（HSK 靠地形收集，无矿脉） |
```

5c. §3 金属家族小节追加 5 条配方条目（格式沿用文档现有"**MMT**/**HSK**/**对比**"三段式），数值对照：

- `MMT_SmeltLeadBar_Hand` ↔ `MakeLeadBars_Hand`（铅矾×10→铅×10，1100，技能2）：数值一致
- `MMT_SmeltLeadBar_Electric` ↔ `MakeLeadBars_Electric`（×20→×20，1000，技能6）：数值一致（建筑 MMT 电弧炉 vs HSK 电熔炼器）
- `MMT_SmeltAluminiumBar_Electric` ↔ `MakeAluminiumBars_Electric`（×20→×20，1200，技能7）：数值一致（同上建筑差异）
- `MMT_MakeFerrosiliconAlloy` ↔ `MakeFerrosiliconAlloy`（钢15+硅10+铝5→30，1700，技能8）：数值一致（建筑 MMT 电弧炉 vs HSK 电熔炼器）
- `MMT_MakeMetalCan` ↔ `MakeMetalCan`（[金属锭类]×8→15，800，技能2）：数值一致（同上建筑差异）

5d. §3 新增"陶瓷（Ceramics）"家族小节，含 4 条：

- `MMT_MakeCeramics` ↔ `MakeCeramics`（软粘土20+玻璃生料5→10，1200，技能8）：数值一致（建筑 窑炉+电弧炉 vs 窑炉+电熔炼器）
- `MMT_MakeClayBricks` ↔ `MakeClayBricks`（软粘土15→15，800，技能6）：数值一致（同上）
- `MMT_BurnThings` ↔ `BurnThings`（×20→灰3，400）：数值一致（输入类别 MMT 为 Textiles/Leathers/WoodLog vs HSK Wooden/Woody/Textiles/Leathers，类别口径差异注明）
- `MMT_CremateCorpseCampfire` ↔ `CremateCorpseCampfire`（尸体→灰4，800）：数值一致

5e. 文档末尾（或灰烬小节内）加一段 patch 说明：

```markdown
#### 灰烬 patch（`MMT_Patch_AshRecipes.xml`）

- 原版 `BurnApparel` 追加产出 灰烬(MMT_Ash)×2 ｜ 对齐 HSK 同名 patch（HSK 另将工作量改为 200，MMT 保留原版 60）
- 原版 `CremateCorpse` 追加产出 灰烬(MMT_Ash)×4 ｜ 对齐 HSK 同名 patch（HSK 另将工作量改为 600，MMT 保留原版 180）
```

- [ ] **Step 6: 全量验证**

```bash
cd D:/project/rw
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：`all` 全部套件 OK；dotnet build 0 错误（本批无 C# 改动，仅确认程序集仍可编译）。

- [ ] **Step 7: Commit**

```bash
cd D:/project/rw
git add tools/validate_mod.py
git status --short   # 确认无 WIP 混入
git commit -m "test: 添加 basic-metallurgy 验证套件

Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

（`docs/mmt-vs-hsk-resource-chains.md` 未跟踪，按现有惯例不入库。）

---

## Self-Review 记录

- **Spec 覆盖**：spec §2.1/2.2(资源)→Task 1 Step 2、Task 2 Step 3、Task 3 Step 2；§3(矿脉)→Task 1 Step 4、Task 2 Step 4；§4(窑炉)→Task 2 Step 4；§5(配方)→Task 1 Step 5/6、Task 2 Step 5、Task 3 Step 3；§6.1(stuff 类别)→Task 2 Step 2；§6.2(类别补齐)→Task 1 Step 3；§6.3(灰烬 patch)→Task 3 Step 5；§6.4(篝火火化)→Task 3 Step 3；§7.2(套件)→Task 4；§7.3(对比文档)→Task 4 Step 5（按用户决策改为手工更新，不重建脚本）；§8(提交计划)→Task 1-4 各自 commit。无缺口。
- **Placeholder 扫描**：无 TBD/TODO；所有 XML/Python/命令均为完整可复制内容。
- **类型一致**：`MMT_MakeMetalCan`/`MMT_MakeCeramics` 等 defName 在 Task 1-4 间拼写一致；`ELECTRIC_FURNACE_RECIPES` 追加顺序（Task 1 加 4 条、Task 2 加 2 条）与 EAF recipes 追加顺序一致；窑炉 recipes 最终形态 `[MakeCeramics, MakeClayBricks, BurnThings]` 与 Task 4 套件断言一致。
- **Review Focus**：5 条均已落到对应 Task 的具体步骤（#1→T1S8j，#2→T1S8b/8c，#3→T1S8h/8i+T4S2，#4→T3S5，#5→T2S4+T3S4）。
