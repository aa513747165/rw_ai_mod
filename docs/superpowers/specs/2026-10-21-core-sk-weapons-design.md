# Core_SK 全武器移植到 MMT 设计

- 日期：2026-10-21
- 状态：待用户审阅
- HSK 基准：本地 `Hardcore-SK`，`1.7-super-unstable`，HEAD `b99b17d`
- 目标版本：RimWorld 1.7
- 范围来源：`Hardcore-SK/Mods/Core_SK`；不包含 HMC、Ratkin 或其他扩展武器包

## 1. 已确认决策

1. **范围**：实现 `Core_SK` 全武器，包括手持武器、炮塔武器、炮塔建筑、机械体/Ogre/Norbal 等 NPC 或特殊武器。
2. **保真策略**：保留 HSK 的伤害、射程、射速、质量、价值、材料与研究节奏；剔除 SK/CE 专属字段和 C# 组件，用原版机制替代。
3. **弹药策略**：CE 弹药数值转换为 MMT 投射物；手持武器不消耗弹药。
4. **装填策略**：HSK 原本需要装填的炮塔/自动武器建筑，用对应 MMT 子弹物品装填并随发射消耗。
5. **炮塔成本**：恢复 HSK 炮塔箱链。本决策覆盖 B8 设计中“炮塔箱链已砍”的旧非目标。
6. **研究策略**：压缩为约 20–30 个 MMT 武器研究节点；可对应原版研究的直接引用原版。
7. **实施方式**：生成器提取/转换初稿，人工分批审查和修正。
8. **资源授权**：HSK 武器贴图与音效资源均已获授权，可纳入 MMT。

## 2. 源数据规模

以下数字由本地 XML 解析实际统计，忽略注释掉的 Def：

- `Defs/ThingDefs_Weapons/*.xml`：33 个文件，167 个顶层 ThingDef，其中 27 个抽象、140 个非抽象（含炮塔枪与内联投射物）。
- `Defs/RecipeDefs_Weapons/*.xml`：17 个文件，103 个 RecipeDef，其中 1 个抽象、102 个非抽象；其中 14 条武器部件配方已由 B8 实现、1 条护盾拆解配方不属于武器范围，本次实际计划处理 87 条。
- `Defs/ResearchProjectDefs/Research_Weapon.xml`：44 个 ResearchProjectDef。
- `Defs/ThingDefs_Items/Items_Crate.xml`：26 个炮塔箱 ThingDef。
- `Defs/RecipeDefs/Recipes_Turrets.xml`：26 条炮塔箱 RecipeDef。
- `Defs/ThingDefs_Buildings/Buildings_Turrets.xml`：18 个 ThingDef，其中 1 个抽象、17 个可建造炮塔。
- `Defs/SoundDefs/World_Oneshots_Weapons.xml`：88 个 SoundDef。

## 3. 非目标

- 不引入 Combat Extended 或 HSK 程序集依赖。
- 不实现 CE 的弹匣 UI、压制、火控模式、后座模拟、护甲穿透公差和复杂弹道模拟。
- 不移植 `HMC weapon pack`、Ratkin 武器或其他非 `Core_SK` 武器包。
- 不重制派系、商人、袭击者装备表；本任务交付武器本体、投射物、制造链、研究和炮塔装填。
- 不覆盖原版武器；即使 HSK 源定义是 DefOverwrite，MMT 也生成带 `MMT_` 前缀的独立武器。
- 不恢复 CE 全弹药物品体系；只生成装填类炮塔所需的 MMT 子弹物品与配方。
- 不移植 `DeconstructPersonalShield` 护盾拆解配方；护盾不属于本次武器范围。

## 4. 批次拆分

### W0 武器基础

- 新增转换工具和映射表。
- 建立武器共享基类、投射物转换模型、炮塔弹药模型。
- 用最小样本验证原版 `CompProperties_Refuelable + consumeFuelPerShot` 是否满足炮塔按发耗弹；若验证失败，停止批量生成并回到设计阶段，不静默改加 C#。

### W1 常规枪械

- 手枪、冲锋枪、卡宾枪、步枪、霰弹枪、狙击枪、重武器、发射器、喷火器。
- 对应武器配方、贴图和音效。

### W2 冷兵器与投掷物

- 近战、弓、弩、手雷及爆炸投射物。
- CE `ToolCE` 转原版 `Tool`。

### W3 未来与远古武器

- 充能、光束、离子、Blaster、Archotech 武器。
- 特殊投射物逐一人工审查。

### W4 炮塔体系

- 炮塔枪、17 个可建造炮塔、26 个炮塔箱、26 条炮塔箱配方。
- 装填类炮塔对应的 MMT 子弹物品与配方。
- 炮塔按发消耗对应子弹。

### W5 NPC/特殊武器

- 机械体、Ogre、Norbal/Special 等非玩家制造武器。
- 只要求 Def 可加载和被引用；不强制增加制造配方。

### W6 收尾

- 更新映射表、别名表与相关文档。
- 执行武器套件、全量静态验证和统一编译。

## 5. 生成器与映射表

新增：

- `tools/build_mmt_weapons_from_hsk.py`
- `tools/data/mmt_weapon_mapping.json`

生成器要求：

1. 只读 HSK 与 Combat Extended XML，不修改 HSK。
2. 输出确定性 MMT XML 初稿；同一输入重复生成应得到同一输出。
3. 映射表保存 defName、材料类别、研究、投射物、音效和特殊字段的人工覆盖。
4. 无法安全自动转换的项目必须写入显式清单，不得静默丢弃。
5. 生成结果只是初稿，必须经人工审查后再纳入对应批次。

## 6. 武器 ThingDef 转换规则

### 命名与分类

- HSK 具体武器 defName 统一转换为 `MMT_<原 defName>`。
- HSK 覆盖原版武器时也生成独立 MMT Def，不 patch 原版。
- 武器 `thingCategories` 映射到原版 `WeaponsRanged` 或 `WeaponsMelee`。
- 中文 label/description 优先取 HSK 1.6 中文翻译；缺失时先使用 HSK 英文文本并记入映射表警告，对应批次提交前必须补成中文。

### 保留字段

保留原版可识别的字段：

- `MarketValue`、`Mass`、`MaxHitPoints`、`Flammability`、`DeteriorationRate`、`RangedWeapon_Cooldown`。
- 贴图、绘制尺寸、装备类型、科技等级、武器标签中原版可识别项。
- `CompEquippable`、`CompQuality`、`CompProperties_Art`、`CompProperties_Styleable`、`CompProperties_Biocodable`、`CompProperties_Forbiddable` 等原版组件。

### Verb 转换

- `CombatExtended.VerbPropertiesCE` 转原版 `VerbProperties`。
- 保留 `warmupTime`、`range`、`minRange`、`burstShotCount`、`ticksBetweenBurstShots`、`soundCast`、`soundCastTail`、`muzzleFlashScale`、`forcedMissRadius`、`requireLineOfSight`、`defaultProjectile` 等原版字段。
- 移除 CE 后座、瞄准模式、火控、弹匣和压制字段。
- `verbClass` 按武器类型映射到原版 `Verb_Shoot`、`Verb_LaunchProjectile` 或人工指定类型。

### 近战 Tool 转换

- `CombatExtended.ToolCE` 转原版 `Tool`。
- 保留 capacities、power、cooldownTime、chanceFactor、linkedBodyPartsGroup。
- CE 穿甲字段映射为原版 `armorPenetration`；异常项进入映射表人工覆盖。

### 剔除字段

- 剔除 `CombatExtended.*`、`SK.*`、`RightToolForJob.*` 类型与 ModExtension。
- 剔除 CE 专用 StatDef 字段，如 `SightsEfficiency`、`ShotSpread`、`SwayFactor`、`Bulk`。
- CE 精度字段通过映射表转换为原版 Accuracy 系字段；没有可靠映射时使用类别默认值并记录人工审查项。

## 7. 投射物转换规则

- CE `Bullet_*` 转为 `MMT_Bullet_*`。
- 直接保留 `damageAmountBase`、`speed`、伤害类型、爆炸半径、爆炸伤害衰减等原版可表达字段。
- `armorPenetrationSharp` 初始按 `值 / 100` 转为原版 `armorPenetrationBase`；异常弹药由映射表覆盖。
- 武器默认投射物使用 HSK 默认弹药对应的 MMT 投射物。
- 手持武器不生成弹药消耗组件，也不要求玩家制造手持武器弹药。
- 火焰、等离子、分裂、追踪、热压等特殊行为不能安全转换时，使用映射表指定原版近似行为，并列入人工审查清单。

## 8. 炮塔装填与弹药

- 只为 HSK 原本需要装填的炮塔/自动武器生成 MMT 子弹物品。
- 子弹物品用 `projectileWhenLoaded` 记录对应 MMT 投射物，保证弹药与弹道的映射可读；实际按发扣弹由炮塔的 refuelable 组件执行。
- 炮塔建筑使用原版 `CompProperties_Refuelable`：
  - `fuelFilter` 指向对应 MMT 子弹；
  - `fuelCapacity` 等于 HSK `magazineSize`；
  - `fuelMultiplier` 为 1；
  - `consumeFuelOnlyWhenUsed` 为 true；
  - 武器 verb 设置 `consumeFuelPerShot` 为 1。
- 炮塔弹药配方只覆盖装填类炮塔所需弹药。
- 不移植 CE 自动装填机；如 HSK 炮塔功能依赖自动装填机，MMT 版本以人工补充弹药为准。

## 9. 配方、工作台与研究

- HSK 武器配方中的 `Weapon_Parts/Pistol_Component/SMG_Component/...` 映射到现有 MMT 武器部件。
- HSK 材料类别映射到 MMT 类别或原版类别。
- 现有 `MMT_WeaponCraftingTable`、`MMT_AdvWeaponCraftingTable`、`MMT_MechWeaponTable` 继续作为主要武器工作台。
- HSK `HeavyArmsBench` 配方并入 `MMT_AdvWeaponCraftingTable`，不新增重型武器机床。
- HSK `TableHeavyAmmunition` 配方挂载到现有 `MMT_AmmoBench`。
- 如 HSK 配方必须由其他工作台承载，优先映射到现有 MMT 台；确无对应时再新增 MMT 工作台。
- HSK 显式 RecipeDef 转 MMT 显式 RecipeDef；HSK 通过武器 `recipeMaker` 自动生成的近战/弓弩配方保留该机制，并将工作台、技能、研究前缀映射到 MMT 或原版等价项。
- 研究树压缩为约 20–30 个 MMT 节点，保留 HSK 科技层级和前置关系。
- 可直接对应的原版研究（如 Gunsmithing、GasOperation、PrecisionRifling、Fabrication、ChargedShot、HeavyTurrets、Mortars）不重复创建。

## 10. 音效与贴图

- HSK 武器贴图复制到 `1.7/Textures/` 对应目录。
- HSK 枪声、尾音与相关音频复制到 `1.7/Sounds/` 对应目录。
- 新增 `1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml`，SoundDef 名称使用 `MMT_` 前缀。
- 武器中的 HSK soundCast/soundCastTail 引用映射到对应 MMT SoundDef。
- 所有贴图和音频路径必须经静态验证确认存在。

## 11. 文件结构

```text
1.7/Defs/ThingDefs_Weapons/
  MMT_Weapons_Base.xml
  MMT_Weapons_Conventional.xml
  MMT_Weapons_Archaic.xml
  MMT_Weapons_Futuristic.xml
  MMT_Weapons_TurretGuns.xml
  MMT_Weapons_NPC.xml
  MMT_Projectiles.xml

1.7/Defs/ThingDefs_Items/
  MMT_Items_TurretAmmo.xml
  MMT_Items_TurretCrates.xml

1.7/Defs/ThingDefs_Buildings/
  MMT_Buildings_Turrets.xml

1.7/Defs/RecipeDefs/
  MMT_Recipes_Weapons_Conventional.xml
  MMT_Recipes_Weapons_Archaic.xml
  MMT_Recipes_Weapons_Futuristic.xml
  MMT_Recipes_TurretAmmo.xml
  MMT_Recipes_TurretCrates.xml

1.7/Defs/ResearchProjectDefs/
  MMT_Research_Weapons.xml

1.7/Defs/SoundDefs/
  MMT_Sounds_Weapons.xml

tools/
  build_mmt_weapons_from_hsk.py
  data/mmt_weapon_mapping.json
```

## 12. 静态验证

`tools/validate_mod.py` 新增 `weapons` 套件并接入 `all`：

1. 所有新增 XML 可解析，defName 不重复。
2. MMT 新增武器、投射物、炮塔、弹药、炮塔箱全部使用 `MMT_` 前缀。
3. 新增 Def 不得残留 `CombatExtended.`、`SK.`、`RightToolForJob.` 类型引用。
4. 所有 `texPath/uiIconPath/clipPath` 指向的本地文件必须存在。
5. 所有 `defaultProjectile/projectileWhenLoaded` 必须指向 MMT 或白名单原版 Def。
6. 所有配方产物、输入、recipeUsers、研究前置必须可解析。
7. 炮塔装填规则逐项断言子弹物品、容量、燃料倍率、`consumeFuelOnlyWhenUsed`、`consumeFuelPerShot`。
8. 数量断言覆盖非抽象武器/内联投射物、配方、炮塔、炮塔箱、装填弹药、压缩研究节点和 SoundDef。
9. `weapons` 套件通过后执行 `validate_mod.py all`。
10. 最终执行 `dotnet build Source/MyModeTest/MyModeTest.csproj`；即使本任务预计不新增 C#，也必须以编译结果作为统一验证。

## 13. 批次验收标准

- W0：生成器可重复运行；映射表可读；最小样本武器、投射物、炮塔耗弹模型通过静态验证。
- W1：常规枪械可装备、可制造，默认投射物均存在。
- W2：近战、弓弩、手雷的 verb/tool/projectile 转换完成，无 CE 残留。
- W3：能量与远古武器转换完成，特殊投射物人工审查完毕。
- W4：炮塔可建造，炮塔箱链完整，装填对应子弹并按发射消耗。
- W5：NPC/特殊武器 Def 可加载，不要求新增制造配方。
- W6：`validate_mod.py weapons`、`validate_mod.py all`、`dotnet build` 全部通过，映射和文档同步更新。

## 14. 风险与处理

- **CE 精度体系无法完全表达**：用映射表转换为原版 Accuracy 系字段，异常项人工覆盖。
- **特殊投射物行为复杂**：不能安全转换的逐项进入人工清单，不做静默降级。
- **炮塔耗弹机制未经验证**：W0 先做最小样本；若原版 refuelable 不满足按发扣弹，停止并重新设计，不直接扩大实现。
- **炮塔箱链恢复扩大范围**：已获用户确认，按 26 个箱和 26 条配方实现。
- **生成器误转换**：映射表和数量断言约束输出；每批人工审查后才提交。
- **研究坐标冲突**：新增 `MMT_Research_Weapons.xml` 时统一分配坐标，验证套件检查重复坐标。
