# MMT RPG 内容包设计方案

> 手工维护的设计文档（非脚本生成物）。记录 RPG 内容包全套设计：装备随机强化与随机附魔、角色成长与职业、任务链与 Boss 副本。
> 状态：**设计定稿，未实现**。后续实现以本文为唯一依据（地位同 `mmt-endgame-design.md`）。

## 0. 背景与动机

MMT 现有 RPG 相邻基础已相当完整，但互不成体系：

- **词条框架现成**：`ModuleEffectKeys`（`Source/MyModeTest/ModuleSystem.cs`）已有 40+ 武器/防具效果键，全部在伤害补丁中实现结算；`CompWeaponEnhancement`/`CompApparelEnhancement`（IEnchantable）通过 XPath 补丁（`1.7/Patches/MMT_Patch_WeaponEnhancement.xml`、`MMT_Patch_ApparelEnhancement.xml`）挂到**全体武器/防具**（含其它 mod），手工附魔 1–3 槽、100% 成功、材料驱动。
- **装备生成钩子现成**：`PawnWeaponBudgetPatch.cs` 已 patch `PawnWeaponGenerator.TryGenerateWeaponFor`。
- **据点/世界框架现成**：`Exploration.cs`（深空扫描 `MMT_DeepSpaceScanner` → Site + SitePart + `StockpileRewardExtension` 奖励堆，传送台折跃进入，30 天过期）。
- **能力/Hediff 先例现成**：`StateBackup.cs`（Royalty AbilityDef + HediffComp + Harmony 拦截死亡）；`BionicsComps.cs`（狂暴 gizmo 等）。
- **设置/校验/文档惯例现成**：`MMTSettings.cs` 模式、`tools/validate/<链名>.py` 新套件模式。

**缺口**：

- 词条只能手工附魔（确定性），装备获取没有掉落惊喜，袭击者装备与玩家装备是两条不相干的线；
- 小人只有原生技能，无 RPG 式成长维度（等级/perk/职业）；
- 探索据点只有资源堆，缺战斗目标、叙事与终局挑战。

三大支柱互相咬合成环：

```
副本/Boss/任务链 ──掉落──→ 随机强化/附魔装备 + 职业凭证 + 历练经验
角色成长(历练/perk/职业) ──增强──→ 攻略更高档副本的能力
强化台冲级/补附魔(消耗淬火油/能量结晶等自产材料) ──消化──→ 生产链过剩产出
```

**硬性设计规则**（全文适用）：

1. 副本守卫**绝不硬编码 PawnKindDef 名单**，一律走宿主派系 PawnGroupMaker，跨 Mod 敌人自动兼容（详见 §3.1）；
2. 任何事件/据点/任务类机制落地时同步加设置开关（正面/主动触发型默认开，恶意事故类默认关）；
3. 性能优先：无新增 per-tick 全图扫描，钩子只挂在生成/命中瞬间（遵循 `mmt-endgame-design.md` §12）；
4. 存档兼容：新字段带默认值，comp 缺失静默跳过，关开关不删既有数据；
5. 所有 defName 加 `MMT_` 前缀，label/description 中文写死。

---

## 1. 支柱一：装备随机强化 + 随机附魔

> 范围裁减决定（v1）：**不做稀有度档位、不做独立随机词条 comp、不做重铸**。掉落惊喜直接落在两个既有系统上——强化等级（0–15，原赌博线）与附魔词条（模块 Def，原确定线），装备生成瞬间一次掷好。**难度越高，强化等级越高、附魔越多越高级。**

### 1.1 为什么这样最省

- 数值结算零改动：`CompWeaponEnhancement`/`CompApparelEnhancement` 的 `level`/`enchants` 字段与 `GetEffectSum` 聚合原样复用，40+ 词条键的既有消费端（伤害补丁/StatWorker）自动生效；
- 显示零改动：`TransformLabel` 已实现「前缀·装备名 +N」，`CompInspectStringExtra` 已列强化等级与附魔词条；
- 存档零改动：不新增 comp、不新增字段，掷点结果就写在既有存档字段里；
- 挂载零改动：强化 comp 已挂全体武器/防具（跨 Mod），只需在生成钩子处掷点。

### 1.2 随机强化等级（难度分档）

袭击者装备按生成时威胁点（`StorytellerUtility.DefaultThreatPointsNow`）分档掷初始强化等级（直接写 `level`，不耗材料；据点/任务按各自 tier 走同一函数，tier 0–3 对应下表四档）：

| 威胁点 | +0 | +1~3 | +4~6 | +7~9 | +10~12 | +13~15 |
|---|---|---|---|---|---|---|
| < 500 | 70% | 25% | 5% | — | — | — |
| 500–2000 | 50% | 33% | 14% | 3% | — | — |
| 2000–5000 | 35% | 33% | 20% | 9% | 3% | — |
| > 5000 | 25% | 28% | 25% | 13% | 7% | 2% |

`lootRollChanceFactor` 倍率整体放大非 +0 档概率。袭击者携带高强化装备会提升敌对强度——视为特性（掉落与威胁对等），由财富/点数缩放自然平衡。掉落装备极少到 +13 以上，强化台冲满级仍是材料消化口（淬火油/能量结晶），原强化玩法不变。

### 1.3 随机附魔（词条池沿用模块档位）

掷完强化等级后掷附魔：直接 `AddEnchant(moduleDef)`（不消耗模块物品；词条数 ≤ 当前槽位 1/+7→2/+13→3，强化等级天然限制低难度掉落的词条数）：

| 威胁点 | 附魔概率与条数 | 开放池 |
|---|---|---|
| < 500 | 15% 概率 1 条 | 低级池 |
| 500–2000 | 30% 概率 1 条 | 低级 + 中级 |
| 2000–5000 | 45% 概率 1~2 条 | 低级 + 中级 + 高级 |
| > 5000 | 60% 概率 1 条~槽满 | 全池（极品权重减半） |

**词条规则**：

- 词条池 = 既有附魔模块 Def（`MMT_WeaponMod*`/`MMT_ApparelMod*`）按 XML 档位注释分低/中/高/极品四组（实现时照抄分组，校验套件防漏），**新增零个效果键**；
- 随机附魔与手工附魔**共用同一 `enchants` 列表与槽位**：互不重复、效果叠加、可被手工附魔覆盖——掉落线与制作线完全合流；
- 类型约束：近战限定词条（吸血 `MMT_WeaponModLifeLeech`）不对远程武器入池，远程限定词条（`MMT_WeaponModRangedLeech`、`MMT_WeaponModMagazine`、`MMT_WeaponModLongRange` 等）不对近战武器入池；防具池与武器池完全分离；
- 危险特效词条（爆炸/引雷/毒气/烟幕/尸爆等，见 ModuleEffectKeys 注释标注"危险特效"者）在池内权重减半，且仍受既有 gizmo/设置门控；
- 数值 = 模块 Def 的 valuePerItem 原值（不缩放，与手工附魔等值）。

### 1.4 掉落钩子（掷点从哪触发）

| 来源 | 钩子 | 难度依据 |
|---|---|---|
| 袭击者装备 | `PawnWeaponGenerator.TryGenerateWeaponFor` postfix（紧跟现有 BudgetPatch）；防具同理 spike `PawnApparelGenerator` 钩子 | 生成时威胁点分档（§1.2/§1.3） |
| 副本/据点奖励 | 自有生成代码（`SitePartWorker_MMT_Stockpile` 扩展、Boss 掉落）直接调 `LootRoll.Roll(gear, tier)` | 据点档位（§3.1） |
| 任务奖励 | QuestScriptDef 奖励生成走同一 `Roll` | 任务节点配置 |

### 1.5 C# 设计（`Source/MyModeTest/LootRoll.cs`，新增）

- 静态类 `LootRoll`：`RollForPoints(ThingWithComps gear, float points)`（袭击者入口）与 `Roll(ThingWithComps gear, int tier)`（据点/任务入口）；内部按 §1.2 表掷强化等级写入 comp，按 §1.3 表掷附魔逐条 `AddEnchant`。
- 池表：静态只读分组（低/中/高/极品 × 武器/防具），近战/远程限定与危险特效清单内嵌；抽取前按 `gear.def.IsRangedWeapon` 过滤限定词条。
- 不新增 comp/字段/挂载补丁；`CompWeaponEnhancement`/`CompApparelEnhancement` 各加一个 `SetLevelForLoot(int)` 写入方法（`level` 现为私有字段，仅 LootRoll 调用）。
- 防具生成钩子（已 spike 回填）：1.7 的 `PawnApparelGenerator.GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)` 是着装生成唯一出口，postfix 后遍历 `pawn.apparel.WornApparel` 逐件掷点（护盾腰带无附魔 comp 自动跳过）；武器钩子为 `PawnWeaponGenerator.TryGenerateWeaponFor` postfix 读 `pawn.equipment.Primary`。两钩子仅对玩家敌对派系生效，威胁点取 `StorytellerUtility.DefaultThreatPointsNow(pawn.Map ?? Find.AnyPlayerHomeMap)`。

### 1.6 设置与校验

- 设置：`lootRollEnabled`（总开关，默认开；关闭时不掷点，既有数据天然保留——数据就在普通强化字段里）、`lootRollChanceFactor`（0–3，默认 1）。
- 校验套件 `tools/validate/loot_roll.py`：池表与 XML 档位注释分组一致、模块 defName 有效、近战/远程限定标注完整。
- 贴图：无新贴图需求。

---

## 2. 支柱二：角色成长 / 职业系统

### 2.1 历练等级

`CompPawnProgress`（经 XPath 补丁挂到 `Human` ThingDef 的 comps，沿用武器 comp 补丁模式）：

- 字段：`xp`、`level`(1–20)、`perkPoints`、已选 perk（List\<HediffDef\>）、职业记录（见 §2.2）。
- **XP 来源**：

| 行为 | XP | 钩子 |
|---|---|---|
| 造成伤害 | 每 1 点伤害 0.1 XP | 复用武器机制伤害补丁的命中上下文（不新增命中补丁） |
| 击杀 | 人类 10 / 虫 8 / 机械 15 / Boss 200 | `Pawn.Kill` postfix 检查 instigator（先例：`StateBackup.cs` 的 Kill 拦截） |
| 工作学习 | 技能经验 × 0.2 | `SkillRecord.Learn` postfix |

- **升级曲线**：N → N+1 级需 `100 × N^1.5` XP（1→2 约 100，10→11 约 3160，19→20 约 8490）。
- **每级被动**：经 HediffDef `MMT_PawnProgress`（不可见，20 个 stage）下发：每级 `WorkSpeedGlobal` +1%（满级 +20%）、`MoveSpeed` +0.5%（满级 +10%）。
- **Perk 点**：每 3 级 +1（3/6/9/12/15/18 级共 6 点）。

### 2.2 Perk 系统（纯 XML 为主）

Perk = HediffDef（单 stage + statOffsets/statFactors），选择 UI：有可用 perk 点时小人出现 gizmo → FloatMenu 三页分支 → 确认加 Hediff（同 perk 不可重复）。共 18 个，分三支：

**战斗支**：

| Perk | defName | 效果 |
|---|---|---|
| 老兵 | MMT_Perk_Veteran | ShootingAccuracyPawn ×1.05 |
| 近战大师 | MMT_Perk_Brawler | MeleeHitChance +5，MeleeDodgeChance +3 |
| 坚韧 | MMT_Perk_Tough | PainShockThreshold +0.08 |
| 神行 | MMT_Perk_Fleet | MoveSpeed +0.15 |
| 快速恢复 | MMT_Perk_FastHeal | InjuryHealingFactor ×1.2 |
| 战场直觉 | MMT_Perk_BattleSense | MeleeDodgeChance +5 |

**生产支**：

| Perk | defName | 效果 |
|---|---|---|
| 巧手 | MMT_Perk_Deft | WorkSpeedGlobal ×1.08 |
| 矿工 | MMT_Perk_Miner | MiningSpeed ×1.15，MiningYield ×1.1 |
| 农夫 | MMT_Perk_Farmer | PlantWorkSpeed ×1.15 |
| 建筑师 | MMT_Perk_Builder | ConstructionSpeed ×1.15 |
| 学者 | MMT_Perk_Scholar | ResearchSpeed ×1.15 |
| 厨师 | MMT_Perk_Chef | CookingSpeed ×1.15 |

**生存支**：

| Perk | defName | 效果 |
|---|---|---|
| 铁人 | MMT_Perk_IronBody | ImmunityGainSpeed ×1.15 |
| 大胃王 | MMT_Perk_EfficientGut | HungerRate ×0.85 |
| 深眠 | MMT_Perk_DeepSleep | RestRateMultiplier ×1.1 |
| 心灵壁垒 | MMT_Perk_MindWall | PsychicSensitivity ×0.85 |
| 抗压 | MMT_Perk_GritMind | MentalBreakThreshold -0.04 |
| 驮兽 | MMT_Perk_PackMule | CarryingCapacity +20 |

（stat defName 以 1.7 实际为准，实现时逐一核对；核对结果回填本表。）

### 2.3 职业系统

> **本节已由专项设计文档取代**：职业体系扩展为复刻 RoM 全职业（37 个可选职业），见 `docs/mmt-class-design.md`。本节原 4 职业（狂战士/神射手/战地医师/工匠大师）草案作废，仅保留两条仍有效的设计决定：
>
> - 职业载体 = HediffDef 三阶（I/II/III）+ 主动技能走原版 Ability 框架；每人限 1 职业，v1 不做转职；
> - 解锁建筑仍为职业导师台 `MMT_ClassMentorBench`（造价/升阶条件以 `mmt-class-design.md` §5.1/§2.5 为准）；原"品质暴击"改为独立高阶 Perk（`mmt-class-design.md` §5.3）。
>
> <details><summary>原草案存档（作废，仅供溯源）</summary>
>
> 4 职业 v1。每职业 = HediffDef 三阶（I/II/III，stage statOffsets 递增）+ 主动技能（Royalty AbilityDef + CompAbilityEffect，先例 `StateBackup.cs`）。职业与 perk 独立叠加，构成 build。

| 职业 | defName | 阶位效果（I/II/III） | 主动技能 |
|---|---|---|---|
| 狂战士 | MMT_Class_Berserker | MeleeHitChance +4/+8/+12，MeleeDodgeChance +3/+6/+9，PainShockThreshold +0.05/+0.10/+0.15 | **狂暴**（20 秒：移速×1.3、近战命中+10，结束后 10 秒疲劳；先例：BionicsComps 狂暴 gizmo 改 Ability） |
| 神射手 | MMT_Class_Marksman | ShootingAccuracyPawn ×1.06/×1.12/×1.18，AimingDelayFactor ×0.97/×0.94/×0.90 | **狙击姿态**（15 秒：命中×1.2、射程+3，移速×0.5；切换式） |
| 战地医师 | MMT_Class_FieldMedic | MedicalTendSpeed ×1.15/×1.3/×1.5，MedicalTendQuality +0.1/+0.2/+0.3 | **战场急救**（18 格内友军瞬时稳定伤势：止血 + 小治疗，冷却 1 天） |
| 工匠大师 | MMT_Class_MasterCrafter | WorkSpeedGlobal ×1.06/×1.12/×1.18，ConstructionSpeed ×1.1/×1.2/×1.3 | **赶工**（4 小时：WorkSpeedGlobal ×1.25，冷却 2 天）；另：III 阶解锁**品质暴击**（制作成品 10% 概率品质 +1 档，需 1 条 Harmony postfix 于品质生成点） |

**解锁**：新建筑 **职业导师台 `MMT_ClassMentorBench`**（2×2，造价：微芯片×6 + 高级合金（钨/钛轮替）×40 + 玻璃钢×60，WorkToBuild 6000，800W）。右键交互 + JobDriver（复用 `ChargingStation.cs` 交互模式），条件：

- 消耗对应 **职业凭证 `MMT_ClassToken<职业>`** ×1（副本/Boss 掉落，见 §3）；
- 历练等级要求：I 阶 ≥5 级、II 阶 ≥10 级、III 阶 ≥15 级（升阶不重复消耗凭证，消耗能量结晶×10/×20）。

每人限 1 个职业（就职后不可改，v1 不做转职洗点）。
>
> </details>

### 2.4 UI 与设置

- 新 InspectTab `ITab_Pawn_Progress`（挂 Human）：历练等级/XP 进度条、perk 列表、职业与阶位；Keyed 文案入 `1.7/Languages/ChineseSimplified/Keyed/MMT_Rpg.xml`（沿用 `MMT_InspectorTabs.xml` 模式）。
- 设置：`pawnProgressEnabled`（总开关，默认开；关闭后不再获得 XP、gizmo/页签隐藏，既有 Hediff 保留）。

### 2.5 校验与贴图

- `tools/validate/pawn_progress.py`：perk/职业 HediffDef 字段完整、stat 引用有效、AbilityDef 引用有效、导师台造价/凭证 defName 有效、20 个历练 stage 数值连续。
- 贴图：职业导师台 1 座（`<名>.png` + `<名>_m.png`）、4 个职业技能图标、4 个职业凭证图标（多状态 `_a/_b/_c` 不需要，单图即可）。AI 生成前按 `.claude/skills/rw-texture-design/SKILL.md` 先写贴图元素描述。

---

## 3. 支柱三：任务链 / Boss / 副本

### 3.1 副本据点（跨 Mod 敌人兼容为核心约束）

新 SitePartDef ×3（`1.7/Defs/WorldObjectDefs/MMT_WorldObjects_RpgDungeons.xml`），worker 复用/扩展 `SitePartWorker_MMT_Stockpile` 奖励堆模式：

| 据点 | defName | 威胁系数 | 守卫 | 奖励 |
|---|---|---|---|---|
| 试炼场 | MMT_SitePart_TrainingGround | ×1.2 | 宿主派系小队 | 随机强化/附魔装备箱（tier 1）×1–2 + 历练书（使用品 +500 XP）×1 |
| 职业圣殿 | MMT_SitePart_ClassShrine | ×1.4 | 宿主派系小队 | 职业凭证碎片×1–2（3 碎片合成 1 凭证，配方在工作台）+ 装备箱（tier 2） |
| 远古守卫巢穴 | MMT_SitePart_BossLair | ×2.0 | **Boss + 宿主派系护卫** | 见 §3.2 掉落表 |

**跨 Mod 敌人兼容（硬性规则）**：

1. **派系选取**：从 `Find.FactionManager` 枚举对玩家敌对且拥有可用 `PawnGroupMaker`（存在 Commonality > 0 的战斗组）的派系，按出现权重随机——其它 mod 的敌对派系天然在列，代码无需感知其存在；
2. **守卫生成**：`SiteMaker.MakeSite` 传入选定派系 + 威胁点，守卫由该派系自带 PawnGroupMaker 产出（mod 单位随派系自动出现）；Boss 巢穴护卫经 `PawnGroupMakerParms` 按宿主派系抽组；
3. **防御性回退**：派系枚举为空或生成失败时，回退到现有机械守卫路径（`MechCluster` SitePart，已有先例）；机械族/虫族等无标准 PawnGroupMaker 的派系走其原版生成器，不做特例硬编码；
4. **黑名单**：设置项 `dungeonFactionBlacklist`（defName 逗号列表，默认空），玩家可排除个别不平衡的 mod 派系；
5. 绝不自写判敌逻辑：守卫/Boss 均挂据点宿主派系，敌意与 lord 全走原版机制。

**接入扫描仪**：`CompDeepSpaceScanner.SiteTypes` 加权表追加试炼场/职业圣殿（低档副本进随机池，与既有三类据点并列）；**Boss 巢穴不进随机池**，仅由任务链（§3.3）生成。据点沿用 30 天过期 TimeoutComp。

**装备箱**：生成时直接调 `LootRoll.Roll(gear, tier)` 产出随机强化/附魔的武器/防具堆（tier：试炼场=1、职业圣殿=2、Boss=3，映射 §1.2 威胁点档），武器从玩家科技水平可用的 weaponTags 池抽（沿用 `PawnWeaponGenerator` 的技术等级过滤思路）。

### 3.2 Boss：远古守卫者

- PawnKindDef `MMT_BossAncientGuardian`（机械体，基于大型机械体 race，healthScale 大幅提高）：武器为 MMT 未来武器 + 固定满强化 +15、附魔满槽（生成时 `Roll(tier 3)` 后补足：极品池保底 1 条）。
- **阶段机制**：HediffComp severity 由 C# 按 HP 推进（血量 <50% 进 P2），数值全在 HediffDef stage XML：

| 阶段 | 触发 | 效果 |
|---|---|---|
| P1 | 默认 | 标准属性 |
| P2 | 血量 <50% | MoveSpeed ×1.3，AimingDelayFactor ×0.8，ShootingAccuracyPawn ×1.1 |

- **掉落**（必掉）：高档随机强化/附魔装备（tier 3）×1 + 随机职业凭证×1 + 远古数据核心×2；护卫（宿主派系抽取，见 §3.1）按袭击者规则正常掷点掉装备。
- Boss 本体为 MMT 独占单位（据点标题内容，可硬编码）；**护卫不硬编码**，跨 Mod 兼容规则同 §3.1。

### 3.3 任务链「远古军械库」（QuestScriptDef，纯 XML 优先）

`1.7/Defs/QuestScriptDefs/MMT_Quests_RpgChain.xml`，三幕：

1. **信号截获**：深空扫描完成或财富 ≥15 万时概率触发（Incident 入口，开关 `rpgQuestsEnabled`）；任务内容：解密信号（研究式延时）→ 世界图标注一处试炼场；
2. **试炼收集**：清理副本收集职业凭证碎片×3（试炼场/职业圣殿均可重复刷）；WorldComponent `MMT_RpgQuestTracker` 记数，凑齐后开启第三幕；
3. **守卫巢穴**：生成 BossLair 据点 → 击杀 Boss → 奖励：传说装备 + 职业凭证 + 远古数据核心 → 结局信。

节点尽量用原版 QuestNode/QuestPart 组合（站点生成、奖励、信件均 XML）；预计仅需 1 个自定义 QuestNode/WorldComponent（碎片计数 → 开启第三幕）。任务链整体可重复（完成后进入冷却，约 30 天后可再次触发）。

### 3.4 校验

- `tools/validate/rpg_quests.py`：QuestScriptDef 节点引用有效、SitePartDef workerClass 存在、Boss PawnKindDef 装备引用有效、掉落表/凭证碎片配方 defName 有效、黑名单设置读取正确。
- 贴图：3 个据点世界图图标（`World/WorldObjects/Sites/MMT_Site*` 现有目录模式）、Boss 贴图（若无合适复用项再 AI 生成）。

---

## 4. 设置汇总（`MMTSettings.cs` 追加）

| 设置项 | 字段 | 默认 | 说明 |
|---|---|---|---|
| 随机强化/附魔总开关 | lootRollEnabled | true | 关闭不掷点，数据保留 |
| 掷点概率倍率 | lootRollChanceFactor | 1.0 | 0–3，放大非 +0 档与附魔概率 |
| 历练/职业总开关 | pawnProgressEnabled | true | 关闭不再获得 XP，Hediff 保留 |
| 任务链开关 | rpgQuestsEnabled | true | 入口 Incident 第一行短路 |
| Boss 巢穴开关 | bossEncountersEnabled | true | 玩家主动触发型，默认开 |
| 副本派系黑名单 | dungeonFactionBlacklist | "" | defName 逗号分隔 |

---

## 5. 性能与存档兼容

- 随机强化/附魔掷点只在装备生成瞬间，结果写入既有 comp 字段（零新增存档字段）；XP 钩子复用既有伤害补丁上下文与 `SkillRecord.Learn`（高频但单条廉价加法，无分配）；无新增 per-tick 全图扫描。
- 历练/职业/perk 数值全走 Hediff stage 原生 statOffsets/statFactors，享受原版 stat 缓存，不写自定义 StatWorker。
- 存档：全部新字段带默认值；旧存档小人无 comp 时静默跳过；关开关不删数据。
- 已知风险：CE 环境下远程伤害补丁可能不生效（与终局包 §2 相同结论，设置面板标注；本包不声明 CE 依赖）。

## 6. 实现阶段建议与验证（供后续开工参考）

| 阶段 | 内容 | 主要产出 |
|---|---|---|
| A | 随机强化+随机附魔 | `LootRoll.cs`、武器/防具生成钩子、`validate/loot_roll.py` |
| B | 历练/perk/职业 | 按 `mmt-class-design.md` §7 的 B1–B5 分步：历练/Perk 框架 → 技能效果库+职业导师台 → RoM 全职业生成器铺量 → 收尾 |
| C | 副本/Boss/任务链 | SitePart×3、`BossEncounter.cs`、Boss PawnKindDef、QuestScriptDef 链、`validate/rpg_quests.py`、据点/Boss 贴图 |
| D | 整合 | `MMTSettings` 面板分区、Keyed 翻译、AGENTS.md Source 清单更新、全量校验回归 |

每阶段验证：`dotnet build Source/MyModeTest/MyModeTest.csproj` + `PYTHONIOENCODING=utf-8 python tools/validate_new.py <套件>` + `python tools/validate_mod.py all` 回归。

游戏内手测路径：

1. Dev mode 生成袭击者 → 检查装备强化等级/附魔的 label 与 inspect；多档威胁点各抽一批验证分布；
2. 击杀/工作攒 XP → 升级选 perk → 导师台就职 → 技能 gizmo 可用；
3. 任务链三幕走完 → Boss 击杀 → 掉落传说装备与凭证；
4. 加装任意含敌对派系的 mod → 副本守卫出现该 mod 单位（跨 Mod 兼容验收）。

## 7. 待定项（实现时回填）

- ~~`PawnApparelGenerator` 在 1.7 的防具生成钩子签名~~（已回填 §1.5：`GenerateStartingApparelFor(Pawn, PawnGenerationRequest)`）；
- perk/职业 stat defName 以 1.7 实际为准逐一核对（§2.2/§2.3 表）；
- 品质暴击的品质生成 patch 点（`QualityUtility.GenerateQualityCreatedByPawn` 候选）；
- Boss race 基底选型（大型机械体 vs 自定义 body，实现时评估贴图成本）。
