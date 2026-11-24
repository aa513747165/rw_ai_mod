# MMT 职业系统设计文档（RoM 全职业复刻）

> 手工维护的设计文档（非脚本生成物）。本文件是 MMT 职业体系的**唯一设计依据**（地位同 `mmt-rpg-design.md`、`mmt-endgame-design.md`）。
> 状态：**v1 已实现**（B1–B5 全部落地：历练/19 Perk/37 职业/生成器/校验套件；§8 spike 项已全部回填）。
> 关系：本文档**取代** `mmt-rpg-design.md` §2.3（原 4 职业草案作废）；历练等级与 Perk 仍以 `mmt-rpg-design.md` §2.1/§2.2 为准，本文档仅引用不重复。

## 1. 总纲

### 1.1 目标

复刻参考 Mod **A RimWorld of Magic (Continued)**（下称 RoM，本地路径 `D:\steamcmd\steamapps\workshop\content\294100\3823345404`，已获授权使用其贴图与音效）的**全部职业**到 MMT：RoM 共 39 个职业（法师系 26 + 斗士系 13），约 200 个去重技能（463 个 TMAbilityDef 含 I–IV 分级与 Dismiss 附属技）。

### 1.2 已确认的用户决策

1. 支柱二全套一次做完：历练等级 + 18 Perk（`mmt-rpg-design.md` §2.1/§2.2）+ 本文档的职业系统；
2. **全部职业所有主动技能统一走原版灵能系统**：AbilityDef 用 Core `PsycastBase` 基类，消耗心灵集中（`Ability_PsyfocusCost`）、积累心灵熵（`Ability_EntropyGain`）、时长受心灵敏感度缩放（comps 的 `psychic=true`）。斗士系不再单设体力资源——RoM 的法力/体力双资源在 MMT 统一映射为心灵熵/心灵集中；
3. 职业凭证本阶段仅 Dev 模式发放（`[DebugAction]`），导师台不做制作配方；副本/Boss 掉落（`mmt-rpg-design.md` 支柱三）实装后凭证改由据点掉落。

### 1.3 硬性规则

1. **不依赖 RoM 的任何 Def/程序集**——RoM 仅作设计参照与贴图/音效资源库；其 Bridge 层（`AbilityDefs/Bridge/`）解析到自身程序集，不可借用；
2. 所有 defName 加 `MMT_` 前缀；label/description 中文写死——RoM 已有汉化条目直接抄（覆盖源：`Languages/ChineseSimplified/DefInjected/` 共享层 14 个 TMAbilityDef 文件 338 条 + `v1.7/Languages/ChineseSimplified/DefInjected/` career 层，技能覆盖约 40%，职业名覆盖 30+14 条；§3 表内以 ★ 逐条标注），其余自译；
3. 贴图/音效复制仅限本 Mod 使用，About.xml 备注来源；RoM 目录只读，不改动；
4. 性能优先：无新增 per-tick 全图扫描；维持型光环用间隔脉冲（见 §6）；
5. 存档兼容：新字段带默认值，comp 缺失静默跳过，关开关不删数据；
6. 生成的 XML 是**生成物**，要改就改生成脚本/映射表后重跑（同仿生链惯例）。

### 1.4 RoM 职业口径换算

RoM 的 39 职业在 MMT 落地为 **36 个可选职业**，差异说明：

| RoM 原样 | MMT 处理 |
|---|---|
| 术士 Warlock / 魅魔 Succubus（共用 DemonKin 技能组） | 合并为 1 个职业「恶魔术士」 |
| 巫妖 Lich（死灵法师变身形态） | 不作独立职业；巫妖形态技能 TM_LichForm 入 deferred（§4） |
| 影行者 ShadowWalker（暗影的 Might 变体，无独立技能） | 并入「暗影」，不作独立职业 |
| 混沌法师 ChaosMage（唯一技能=随机重掷技能池） | 职业整体 deferred（机制即随机池，与固定职业体系冲突） |
| 军医 Field Medic（无独立 trait，血清授予） | 保留为独立职业「战地军医」 |
| 流浪法师 Wanderer / 旅者 Wayfarer（纯被动，无专属技） | 保留为独立职业（定位：通用属性成长 + 通用技能池入口，见 §3.23/§3.37） |

法师系 25 + 斗士系 11 = 36 个可选职业 + 1 个 deferred（混沌法师）。

## 2. 架构设计

### 2.1 职业载体

- 每职业 3 个 HediffDef：`MMT_Class_<名>_I/II/III`（对应 RoM 的多档 trait），`isBad=false`、不显示在健康页常规列表（`MMT_PawnProgress` 同款处理），无自然消退；
- def 级 `<abilities>` 按阶解锁技能：**I 阶 2 个基础技 → II 阶 +2 → III 阶全开**（含大招与 career 追加技）；升阶 = 摘除低阶 Hediff + 添加高阶 Hediff（技能 gizmo 随 Hediff 自动增减，零 C# 绑定代码）；
- 阶位被动：RoM 的 Training/成长型被动技（剑术精通、弓箭训练、灵能强化躯体等）**不做成技能**，转化为各阶 Hediff 的 statOffsets/statFactors（§3 每职业给出数值表）；
- 每人限 1 职业（就职后不可改，v1 不做转职）。

### 2.2 技能模型

- 全部主动技能：AbilityDef ParentName=`PsycastBase`（Core `AbilityDefs.xml`），`level=1`；
- **灵能消耗规则**（替代 RoM 的 manaCost/staminaCost，按威力档统一）：

| 威力档 | 心灵熵 | 心灵集中 | 典型对象 |
|---|---|---|---|
| 小技 | 8 | 1% | 单发投射物、单体 buff/debuff、闪烁 |
| 中技 | 16 | 2% | 连射、AoE、召唤小单位、群体 buff |
| 大招 | 35 | 4% | 大范围 AoE/风暴类、强力召唤、复活级治疗 |

  个别技能在映射表中覆盖；冷却照抄 RoM `SecondsToRecharge`（秒→tick 由生成器换算 `cooldownTicksRange`）；
- **维持型光环/开关技**（RoM 约 20 个：抚慰之风、希望之光、指挥官三光环、Sprint 等）：改为"开启后驻留 Hediff + 每脉冲少量涨熵"，脉冲间隔默认 300 tick（设置可调），到熵上限自动关闭——灵能框架下无独立"蓝耗维持"概念，用熵压代替；
- **同名分级技能合并**：RoM 的 I/II/III/IV 级同名技能合并为 1 个 AbilityDef，效果随施法者职业阶位缩放（comp 读职业 Hediff stage，默认系数 I=0.8 / II=1.0 / III=1.25，映射表可覆盖）；

### 2.3 通用技能效果库（C#，约 12 个参数化 comp）

全部属性走 `CompProperties` XML 字段，生成器填参；伤害/治疗数值以 RoM XML 为基准按 §2.2 缩放。

| Comp（`CompAbilityEffect_MMT_*`） | 覆盖桶 | 参数 |
|---|---|---|
| GiveHediffAoE | 群体 buff/debuff | hediffDef、半径、目标阵营（友/敌）、时长 |
| LaunchProjectile | 投射物伤害 | projectile ThingDef、附加命中 Hediff、爆炸半径 |
| AoEDamage | 区域伤害 | damageDef、半径、护甲穿透、脉冲次数/间隔 |
| Heal | 治疗 | 治疗量、止血、治感染、治疤痕开关 |
| Summon | 召唤 | PawnKindDef、数量、存在时限（限时 Hediff 到期消失，不配 Dismiss 技）、数量上限 |
| DashStrike | 位移+伤害 | 突进距离、路径/落点伤害 |
| MentalState | 心灵控制 | mentalStateDef、半径、时长 |
| SpawnEffect | 生成 | GameCondition（天气）/物品/建筑/地形引用 |
| MultiShot | 连射 | 投射物、连发数、散射角 |
| ToggleAura | 维持光环 | hediffDef、半径、脉冲间隔、每脉冲熵增 |
| ResurrectLite | 复活（简化） | 复用原版复活机械血清逻辑，仅对刚死未腐尸体 |
| 个案单例 | special 中易做项 | 战场急救、尸爆（复用 ModuleSystem 尸爆逻辑）、冲锋缴械等 |

单体 buff/debuff 直接用原版 `CompAbilityEffect_GiveHediff`；位移用原版 `CompAbilityEffect_Teleport`；眩晕用原版 `CompAbilityEffect_Stun`——能纯 XML 的桶不进自定义库。

### 2.4 生成器模式（仿生链先例）

- `tools/build_mmt_rom_classes.py`：解析 RoM `v1.7/Defs/AbilityDefs/Abilities_TMagic_*.xml`（含 Career/Progression 追加文件），按映射表产出：
  - `1.7/Defs/AbilityDefs/MMT_Abilities_RoM_<职业>.xml` ×36（生成物）；
  - `1.7/Defs/HediffDefs/MMT_Hediffs_RoMClasses.xml`（36 职业×3 阶 + 全部技能 buff/debuff Hediff，生成物）；
  - 顺带复制图标/投射物贴图/音效：RoM `Textures/UI` → `1.7/Textures/UI/RoM/`、`Textures/Spells` → `1.7/Textures/Things/Projectile/RoM/`、按需 `Sounds/` → `1.7/Sounds/RoM/`；
  - `--check` 模式核对「RoM 技能全集 = 已映射 + 已登记 deferred」，防漏技能；
- `tools/data/mmt_rom_class_mapping.json`：每技能一条 `{romDef, mmtDef, comp, 参数, 威力档, 中文label来源(copied|translated), 状态: mapped|deferred(原因)}`——唯一人工维护点。

### 2.5 就职与升阶

- 导师台 `MMT_ClassMentorBench`（2×2，微芯片×6 + 高级合金×40 + 玻璃钢×60，WorkToBuild 6000，800W）；FloatMenu 交互（复用 `ChargingStation.cs` 模式），按法师/斗士两系列出 36 职业；
- 就职：消耗通用凭证 `MMT_ClassToken`×1（Dev 发放）+ 历练 ≥5 级；**无灵能链接者就职时自动觉醒 1 级灵能**（原版 `ChangePsylinkLevel` 系 API，spike 项 §8）；
- 升阶：II 阶要求历练 ≥10 级 + 能量结晶×10；III 阶 ≥15 级 + 能量结晶×20（不重复耗凭证）；
- 品质暴击：RoM 无工匠职业，原 4 职业草案的工匠大师品质暴击改为**独立高阶 Perk**（第 19 个 perk，消耗 perk 点×2，制作成品 10% 概率品质 +1 档，Harmony postfix 品质生成点，spike 项 §8）。

---

## 3. 全职业目录（法师系 24 + 斗士系 13 = 37 个可选，另混沌法师 deferred）

通用约定（每节不再重复）：

- 技能 defName 规则：RoM `TM_<名>` → `MMT_Magic_<名>` / `MMT_Might_<名>`；同名 I–IV 级合并为一个 defName（阶位缩放见 §2.2）；
- 「中文名」列：标 ★ 者抄自 RoM 自带汉化（已逐一核对存在对应 `.label` 条目；覆盖来源 = 共享层 `Languages/ChineseSimplified/DefInjected/`：火/冰/雷/奥术/德鲁伊/圣骑士/召唤师/死灵/牧师/吟游诗人/角斗士/游侠/狙击手/刀锋舞者 + Sustained/通用池部分；career 层 `v1.7/Languages/ChineseSimplified/DefInjected/`：全部 19 个 career 追加技 + 14 个职业名含 指挥官/死亡骑士/武僧/灵能武者/超级士兵/药剂师/无面者/影行者 等），其余为 MMT 自译；
- 「档」= §2.2 威力档（小/中/大），冷却照抄 RoM `SecondsToRecharge`（空=实现时从映射表回填）；
- 「阶」= 解锁阶位（I/II/III）；「Deferred」技能见 §4，不进生成物；
- 阶位被动数值为设计值，stat defName 实现时以 1.7 实际核对（§8）。

### 法师系（24）

#### 3.1 火系魔法师 `MMT_Class_FireMage`

- 原型：RoM trait `InnerFire`；中文名 ★。主题：火焰投射与区域焚烧。
- 阶位被动：ShootingAccuracyPawn ×1.03/×1.06/×1.10，火焰系技能伤害经阶位缩放（§2.2）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Firebolt | 燃烧之箭 ★ | 投射物 → LaunchProjectile | 小 | 4s | I | 命中点燃 |
| 烈焰光环（RoM TM_RayofHope） | 内心之火 ★ | 维持光环 → ToggleAura | 中 | — | I | 友军抗寒/自愈/耐力 |
| MMT_Magic_Fireball | 火球术 ★ | 投射物爆炸 → LaunchProjectile | 中 | 20s | II | 落点爆炸 |
| MMT_Magic_Fireclaw | 焰爪 ★ | 区域伤害+火场 → AoEDamage | 中 | — | II | 横扫一片 |
| MMT_Magic_Firestorm | 火焰风暴 ★ | 区域轰炸 → AoEDamage | 大 | 120s | III | 流星火雨 |

#### 3.2 冰系魔法师 `MMT_Class_IceMage`

- 原型：RoM trait `HeartOfFrost`；中文名 ★。主题：减速冻结与控制。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_FrostRay | 霜冻之寒 ★ | 投射物+减速 → LaunchProjectile | 小 | — | I | 命中冻结减速 |
| MMT_Magic_Icebolt | 冰箭术 ★ | 投射物 → LaunchProjectile | 小 | — | I | 重伤流血 |
| MMT_Magic_Snowball | 冰雪术 ★ | 投射物 AoE → LaunchProjectile | 中 | — | II | 范围伤害+冻结 |
| 抚慰之风（RoM TM_Soothe） | 抚慰之风 ★ | 维持光环 → ToggleAura | 中 | — | II | 减疼痛/疲劳 |
| MMT_Magic_Rainmaker | 唤雨者 ★ | 天气 → SpawnEffect | 中 | 120s | III | 召唤/停止降雨 |
| MMT_Magic_Blizzard | 暴风雪 ★ | 区域伤害 → AoEDamage | 大 | — | III | 掩埋敌人 |

#### 3.3 雷系魔法师 `MMT_Class_LightningMage`

- 原型：RoM trait `StormBorn`；中文名 ★。主题：雷击与眩晕。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_LightningBolt | 电闪雷鸣 ★ | 投射物 → LaunchProjectile | 小 | — | I | 单体雷击 |
| MMT_Magic_AMP | AMP ★ | 自身强化 → GiveHediff | 中 | 600s | I | 躯体大幅强化 |
| MMT_Magic_LightningCloud | 天丛云雷 ★ | 区域伤害+眩晕 → AoEDamage | 中 | — | II | 雷云 |
| MMT_Magic_EyeOfTheStorm | 风暴之眼 ★ | 投射物 → LaunchProjectile | 中 | — | II | 湮灭电球 |
| MMT_Magic_LightningStorm | 闪电风暴 ★ | 区域失控雷暴 → AoEDamage | 大 | 300s | III | — |

#### 3.4 奥术法师 `MMT_Class_Arcanist`

- 原型：RoM trait `Arcanist`；中文名 ★。主题：空间操控。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Blink | 闪烁 ★ | 位移 → Teleport（原版） | 小 | 5–10s | I | 自身瞬移 |
| MMT_Magic_MagicMissile | 魔法飞弹 ★ | 投射物 → LaunchProjectile | 小 | — | I | 几率迷惑 |
| 暗影光环（RoM TM_Shadow） | 暗影随形 ★ | 维持光环 → ToggleAura | 中 | — | II | 强化友军工作/躯体 |
| MMT_Magic_Summon | 召唤 ★ | 拉取目标 → 个案 comp | 中 | — | II | 把远处物品/人拉到身边 |
| MMT_Magic_Teleport | 传送 ★ | 群体传送 → 个案 comp | 大 | 1800s | III | 小范围群体 |
| MMT_Magic_FoldReality | 空间扭曲 ★ | — | — | — | — | Deferred（双向星门跨格转移） |

#### 3.5 德鲁伊 `MMT_Class_Druid`

- 原型：RoM trait `Druid`；中文名 ★。主题：自然恢复与动物。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Poison | 施毒术 ★ | 减益 → GiveHediff | 小 | — | I | 移动越多毒发越快 |
| MMT_Magic_Regenerate | 再生术 ★ | 治疗 buff → GiveHediff | 中 | — | I | 持续回复重伤 |
| MMT_Magic_SootheAnimal | 诱惑动物 ★ | 心灵 → MentalState | 中 | — | II | 安抚或激怒动物 |
| MMT_Magic_CureDisease | 治愈疾病 ★ | 治疗 → Heal | 中 | — | II | 驱除疾病/感染 |
| MMT_Magic_RegrowLimb | 再生肢体 ★ | — | — | — | — | Deferred（跨手术系统） |

#### 3.6 圣骑士 `MMT_Class_Paladin`

- 原型：RoM trait `Paladin`；中文名 ★。主题：防护与治疗。近战向：MeleeHitChance +2/+4/+6。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Heal | 圣光术 ★ | 治疗 → Heal | 小 | 15s | I | 治 3 处伤 |
| MMT_Magic_Shield | 神佑之盾 ★ | 护盾 buff → GiveHediff | 中 | — | I | 吸收伤害能量盾 |
| 圣辉光环（RoM TM_P_RayofHope） | 希望之光 ★ | 维持光环 → ToggleAura | 中 | — | II | 友军增益 |
| MMT_Magic_ValiantCharge | 神圣附体 ★ | 突进+落点伤害 → DashStrike | 中 | — | II | 飞跃震击 |
| MMT_Magic_Overwhelm | 圣光压制 ★ | 近身震慑 → GiveHediffAoE | 中 | — | III | — |
| MMT_Magic_HolyWrath | 神圣之怒 ★ | 天降轰炸 → AoEDamage | 大 | — | III | — |

#### 3.7 召唤师 `MMT_Class_Summoner`

- 原型：RoM trait `Summoner`；中文名 ★。主题：召唤物海。召唤物全部限时存在（§2.3 Summon）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_SummonMinion | 召唤仆从 ★ | 召唤 → Summon | 中 | — | I | 干活仆从 |
| MMT_Magic_SummonPylon | 魔法防御塔 ★ | 召唤建筑 → Summon | 中 | — | I | 定点奥术炮塔 |
| MMT_Magic_SummonExplosive | 魔法地雷 ★ | 召唤建筑 → Summon | 小 | — | II | 法力地雷 |
| MMT_Magic_SummonPoppi | 召唤波皮 ★ | 召唤 → Summon | 中 | — | II | 狂暴波皮群 |
| MMT_Magic_SummonElemental | 元素召唤 ★ | 召唤大型 → Summon | 大 | — | III | 元素生物 |
| MMT_Magic_DismissMinion | 解散 ★ | — | — | — | — | Deferred（限时消失替代，§4） |

#### 3.8 死灵法师 `MMT_Class_Necromancer`

- 原型：RoM trait `Necromancer`（进阶 Lich）；中文名 ★。主题：尸体与亡灵。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_RaiseUndead | 死者苏生 ★ | 尸体复活为仆从 → Summon | 中 | — | I | 限时存在 |
| MMT_Magic_DeathMark | 死亡印记 ★ | 减益 → GiveHediff | 小 | — | I | 降攻击欲/命中 |
| MMT_Magic_DeathBolt | 死亡螺旋 ★ | 投射物 → LaunchProjectile | 小 | — | II | 范围剧痛 |
| MMT_Magic_CorpseExplosion | 尸爆术 ★ | 尸体延时爆炸 → 个案（复用 ModuleSystem 尸爆逻辑） | 中 | — | II | — |
| MMT_Magic_FogOfTorment | 窒息之雾 ★ | 区域伤害+亡灵治疗 → AoEDamage | 大 | — | III | 雾中活物受折磨 |
| MMT_Magic_ConsumeCorpse | 攥取死亡 ★ | — | — | — | — | Deferred（吞尸体回蓝，灵能模型下无对应资源） |
| MMT_Magic_LichForm | 巫妖形态 ★ | — | — | — | — | Deferred（永久变身不可逆） |
| MMT_Magic_DismissUndead | 解散不死者 ★ | — | — | — | — | Deferred（限时消失替代） |

#### 3.9 牧师 `MMT_Class_Priest`

- 原型：RoM trait `Priest`；中文名 ★。主题：纯粹治疗。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_AdvancedHeal | 强效治疗 ★ | 治疗 → Heal | 中 | — | I | 大治疗+止血 |
| MMT_Magic_BestowMight | 激励 ★ | 单体强化 → GiveHediff | 中 | — | I | 躯体能力大幅提升 |
| MMT_Magic_HealingCircle | 治愈之环 ★ | 范围治疗 → Heal(AoE) | 中 | — | II | 治疗场 |
| MMT_Magic_Purify | 净化术 ★ | 治疗疤痕 → Heal | 大 | 600s | III | 净化身心疤痕 |
| MMT_Magic_Resurrection | 英雄不朽 ★ | 复活 → ResurrectLite | 大 | 600s | III | 仅刚死未腐，双方虚弱数日 |

#### 3.10 吟游诗人 `MMT_Class_Bard`

- 原型：RoM trait `TM_Bard`（4 档头衔）；中文名 ★。主题：情绪与激励。被动：SocialImpact ×1.1/×1.2/×1.3。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Entertain | 招待 ★ | 心灵（娱乐） → MentalState | 小 | — | I | 娱乐殖民者 |
| 激励光环（RoM TM_Inspire） | 鼓舞 ★ | 维持光环 → ToggleAura | 中 | — | I | 激励周围人 |
| MMT_Magic_Lullaby | 催眠曲 ★ | 范围催眠 → MentalState | 中 | — | II | 对机械无效 |
| MMT_Magic_BattleHymn | 战斗之曲 ★ | 群体强化 → GiveHediffAoE | 大 | — | III | 逐段强化战力 |
| TM_BardTraining | 训练 ★ | — | — | — | — | 转阶位被动，不做技能 |

#### 3.11 恶魔术士 `MMT_Class_DemonKin`

- 原型：RoM trait `Warlock`/`Succubus`（合并，共用 DemonKin 技能组）；中文名自译。主题：暗影与支配。
- 契约系技能（灵魂契约/暗影召唤/暗影步）依赖双 pawn 纽带，全部 deferred（§4）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_ShadowBolt | 暗影弹 | 投射物 → LaunchProjectile | 小 | — | I | 路径触染 |
| MMT_Magic_Repulsion | 斥力冲击 | 击退减益 → GiveHediffAoE | 小 | — | I | 简化：减速+踉跄，不做位移 |
| MMT_Magic_Dominate | 恐惧支配 | 心灵 → MentalState | 中 | — | II | 旁观者亦受吓 |
| MMT_Magic_Attraction | 暗影之井 | 减益 → GiveHediffAoE | 中 | — | II | 简化：范围减速，不做聚拢 |
| MMT_Magic_Scorn | 魔神飞跃 | 突进+落点伤害 → DashStrike | 中 | — | III | 恶魔化砸地 |
| MMT_Magic_PsychicShock | 灵能巨浪 | 范围精神冲击 → MentalState | 大 | 240s | III | — |
| MMT_Magic_SummonDemon | 召唤恶魔 | 召唤 → Summon | 大 | 1200s | III | — |
| MMT_Magic_SoulBond / ShadowCall / ShadowStep | 灵魂契约系 | — | — | — | — | Deferred（双 pawn 契约纽带） |

#### 3.12 地占师 `MMT_Class_Geomancer`

- 原型：RoM trait `Geomancer`；中文名自译。主题：岩石与大地。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Stoneskin | 石肤 | 层数护盾 → ToggleAura(自身) | 中 | — | I | 各层挡一次攻击（简化：限时减伤） |
| MMT_Magic_Encase | 石墙禁锢 | 生成墙 → SpawnEffect | 中 | — | I | 石墙环封锁 |
| MMT_Magic_Sentinel | 石哨兵 | 召唤 → Summon | 中 | — | II | 休眠哨戒遇敌苏醒（简化：常驻限时） |
| MMT_Magic_EarthernHammer | 大地之锤 | 区域伤害 → AoEDamage | 中 | — | II | 引力抛石 |
| MMT_Magic_Meteor | 陨石轰击 | 区域伤害+留矿 → AoEDamage | 大 | 600s | III | 落点留矿渣 |
| TM_DispelStoneskin / EarthSprites / ShatterSentinel | 解除/大地精灵/引爆哨兵 | — | — | — | — | Deferred（配套解除技/地形改造/多单位联动） |

#### 3.13 技法师 `MMT_Class_Technomancer`

- 原型：RoM trait `Technomancer`；中文名自译。主题：科技与机械克制。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_NanoStimulant | 纳米强化 | 自身强化 → GiveHediff | 小 | — | I | — |
| MMT_Magic_TechnoShield | 动能护盾 | 护盾 buff → GiveHediff | 中 | — | I | 偏转投射物 |
| MMT_Magic_Sabotage | 瘫痪破坏 | 对机械减益 → GiveHediff | 中 | — | II | 瘫痪机械/电子 |
| MMT_Magic_TechnoTurret | 神经炮塔 | 召唤建筑 → Summon | 中 | — | II | 限时遥控炮塔 |
| MMT_Magic_Overdrive | 超载 | 敌损友强 → GiveHediffAoE | 中 | — | III | 机械专属 |
| MMT_Magic_OrbitalStrike | 轨道打击 | 区域轰炸 → AoEDamage | 大 | — | III | — |
| TM_TechnoBit / TechnoWeapon | 浮游助手/铸造武器 | — | — | — | — | Deferred（自主随从 AI/造武器） |

#### 3.14 血法师 `MMT_Class_BloodMage`

- 原型：RoM trait `BloodMage`；中文名自译。主题：以血换力。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_BloodGift | 鲜血献祭 | 自伤降熵 → 个案 comp | 小 | — | I | 灵能化改编：自残放血换心灵熵下降 |
| MMT_Magic_BloodForBlood | 鲜血诅咒 | 渗血减益 → GiveHediff | 小 | — | I | DoT |
| MMT_Magic_IgniteBlood | 燃血术 | 区域伤害+火场 → AoEDamage | 中 | — | II | 点燃暴露血液 |
| MMT_Magic_BloodShield | 血能护盾 | 护盾+再生 → GiveHediff | 中 | — | II | — |
| MMT_Magic_Rend | 血爆 | 区域大出血 → AoEDamage | 大 | — | III | — |
| MMT_Magic_BloodMoon | 血月 | 心灵+召唤 → MentalState+Summon | 大 | 300s | III | 狂怒恶魔猎杀敌人 |

#### 3.15 附魔师 `MMT_Class_Enchanter`

- 原型：RoM trait `Enchanter`；中文名自译。主题：增益与造物。
- 注意：RoM 附魔师核心（物品附魔/嬗变/变形）与 MMT 既有附魔系统（ModuleSystem）定位重叠且机制重，大半 deferred；本职业以人身增益技上线，属**薄职业**（后续阶段补强）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_EnchantedBody | 附魔之躯 | 自身强化 → GiveHediff | 中 | — | I | 操纵/攻速 |
| MMT_Magic_EnchantedAura | 魅力光环 | 社交 buff → ToggleAura | 中 | — | II | — |
| TM_Transmutate / EnchanterStone / EnchantWeapon / Polymorph / Shapeshift | 嬗变/魔法造物/武器附魔/变形 | — | — | — | — | Deferred（物品材质转换/永久造物/与 ModuleSystem 冲突/变形） |

#### 3.16 时空法师 `MMT_Class_Chronomancer`

- 原型：RoM trait `Chronomancer`；中文名自译。主题：时间操控（多数机制独特，薄职业）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_AlterFate | 改写命运 | 死亡回溯 → 个案（复用 StateBackup.cs 快照/回溯先例） | 大 | 600s | I | RoM「预言资源」简化为纯冷却 |
| MMT_Magic_AccelerateTime | 时间加速 | 区域老化减益 → GiveHediffAoE | 中 | — | II | 简化：减速+饥饿加速 |
| TM_Prediction / ReverseTime / ChronostaticField / TimeMark / Recall | 预言/逆转/静滞/时标回溯 | — | — | — | — | Deferred（资源积攒/腐化逆转/时间静滞/状态快照） |

#### 3.17 暗影 `MMT_Class_Shadow`

- 原型：RoM trait `TM_TheShadow`（含影行者 Might 变体并入）；中文名 ★（影行者）。主题：潜行背刺。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_ShadowWalk | 暗影行走 | 位移 → Teleport（原版） | 小 | — | I | 瞬移到目标影中 |
| MMT_Magic_Nightshade | 夜影淬毒 | 武器淬毒 → ToggleAura(自身) | 中 | — | II | 持续型 |
| MMT_Magic_ShadowStrike | 暗影突袭 | 背刺爆发 → DashStrike | 中 | — | II | 5s 急速+返回原位 |
| MMT_Magic_VeilOfShadows | 暗影斗篷 | 隐身+烟雾 → GiveHediff+SpawnEffect | 大 | — | III | — |

#### 3.18 光法师 `MMT_Class_Brightmage`

- 原型：RoM trait `TM_Brightmage`；中文名自译。主题：光与致盲。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_LightLance | 光矛 | 投射物 → LaunchProjectile | 小 | — | I | — |
| MMT_Magic_LightBurst | 闪光爆发 | 致盲减益 → GiveHediffAoE | 中 | — | I | 敌我不分 |
| MMT_Magic_LightSkip | 光行 | 位移 → Teleport（原版） | 小 | — | II | 单人（Mass/Global 版 deferred） |
| MMT_Magic_Refraction | 光墙折射 | 生成墙 → SpawnEffect | 中 | — | II | 偏移敌方弹道（简化：掩体墙） |
| MMT_Magic_Sunfire | 日焰 | 区域切割 → AoEDamage | 大 | — | III | — |
| MMT_Magic_SpiritOfLight | 光之精灵 | 召唤 → Summon | 大 | — | III | 限时 |
| TM_SoL_CreateLight / Equalize | 造光/均衡 | — | — | — | — | Deferred（召唤物联动） |

#### 3.19 萨满 `MMT_Class_Shaman`

- 原型：RoM trait `TM_Shaman`；中文名自译。主题：图腾与灵狼。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_Hex | 咒印 | 减益印记 → GiveHediff | 小 | — | I | 后续技前提（简化：不做前提检测，三咒技独立可用） |
| MMT_Magic_ChainLightning | 连环闪电 | 投射物 → LaunchProjectile | 中 | — | I | 简化：单链不跳跃 |
| MMT_Magic_Hex_Pain | 咒印·剧痛 | 减益 → GiveHediff | 小 | — | II | — |
| MMT_Magic_Hex_CriticalFail | 咒印·厄运 | 减益 → GiveHediff | 小 | — | II | 极度倒霉 |
| MMT_Magic_Hex_MentalAssault | 咒印·精神冲击 | 心灵 → MentalState | 中 | — | II | — |
| MMT_Magic_SummonTotemLightning / Earth / Healing | 闪电/大地/治疗图腾 | 召唤建筑 → Summon | 中 | — | II | 三图腾各一 defName |
| MMT_Magic_Enrage | 狂怒 | 心灵+强化 → MentalState | 中 | — | III | 情绪转战怒 |
| MMT_Magic_SpiritWolves | 灵狼群 | 召唤 → Summon | 大 | — | III | 灵伤穿甲 |
| MMT_Magic_GuardianSpirit | 守护灵兽 | 召唤 → Summon | 大 | — | III | — |

#### 3.20 傀儡师 `MMT_Class_Golemancer`

- 原型：RoM trait `TM_Golemancer`；中文名自译。主题：烙印网络（薄职业，傀儡制作体系 deferred）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_FitnessBrand | 体能烙印 | 单体强化 → GiveHediff | 小 | — | I | — |
| MMT_Magic_EmotionBrand | 心绪烙印 | 单体强化 → GiveHediff | 小 | — | I | 心情向 |
| MMT_Magic_VitalityBrand | 活力烙印 | 单体强化 → GiveHediff | 小 | — | II | 健康向 |
| MMT_Magic_ProtectionBrand | 护卫烙印 | 单体护盾 → GiveHediff | 中 | — | II | — |
| MMT_Magic_AwarenessBrand | 感知烙印 | 单体强化 → GiveHediff | 小 | — | III | — |
| TM_Golemancy / RuneCarving / SiphonBrand / SigilSurge / SigilDrain / LivingWall / DispelBranding | 傀儡制作/符文/烙印网络管理 | — | — | — | — | Deferred（制作体系/烙印网络反转超载/墙体活化） |

#### 3.21 共情者 `MMT_Class_Empath`

- 原型：RoM trait `TM_Empath`；中文名自译。主题：情绪操控。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_SuppressiveAura | 情绪压制 | 维持光环 → ToggleAura | 中 | — | I | 周围敌人情绪/战力压制 |
| MMT_Magic_FadeEmotions | 抚平心绪 | 消除负面记忆 → 个案 comp | 中 | — | II | 简化：移除目标 1 个负面 thought + 心情 buff |
| MMT_Magic_MindKiller | 心灵杀手 | 范围情绪洪流 → MentalState | 大 | — | III | 冲垮周围人 |
| TM_Empathy / HarvestPassion / IncitePassion / Symbiosis | 共情/热情收授/命运共生 | — | — | — | — | 共情转阶位被动；其余 Deferred（操作 passion 数据/双 pawn 绑定） |

#### 3.22 附身灵 `MMT_Class_Possessor`

- 原型：RoM trait `TM_Possessed`/`TM_Possessor`；中文名自译。主题：灵体与亡魂。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_SpiritDrain | 灵能汲取 | 减益+自身降熵 → 个案 comp | 小 | — | I | 目标虚弱一天，自身熵下降 |
| MMT_Magic_DistortSpirit | 灵体扭曲 | 受伤加深减益 → GiveHediffAoE | 中 | — | I | 受伤 +20% |
| MMT_Magic_SpiritRally | 亡魂集结 | 新死之魂化灵狼 → Summon | 中 | — | II | — |
| MMT_Magic_SummonSpiritAnimalMass | 灵兽齐召 | 召唤 → Summon | 大 | — | III | 每种灵兽各一 |
| MMT_Magic_SpiritStorm | 灵能风暴 | 区域伤害 → AoEDamage | 大 | — | III | 简化：定点风暴，不可引导 |
| TM_SpiritPossession / ControlSpiritStorm | 附身/引导风暴 | — | — | — | — | Deferred（接管 pawn 控制权/引导移动实体） |

#### 3.23 流浪法师 `MMT_Class_Wanderer`

- 原型：RoM trait `TM_Wanderer`；中文名自译。主题：通用成长。无专属技——**III 阶解锁通用技能池（§3.39）中的法师通用技**。
- 阶位被动（通用成长）：WorkSpeedGlobal ×1.05/×1.10/×1.15，MoveSpeed +0.1/+0.2/+0.3，GlobalLearningFactor ×1.1/×1.2/×1.2。

#### 3.24 气象法师 `MMT_Class_WeatherMage`

- 原型：RoM `CustomDefs/TM_WeatherMage.xml`；中文名自译。主题：天气操控（全部 SpawnEffect/GameCondition）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Magic_WeatherClearSkies | 驱散阴云 | 天气 → SpawnEffect | 小 | — | I | 驱散雨雪雷暴 |
| MMT_Magic_WeatherWarmFront / CoolFront | 暖流/寒流 | 天气 → SpawnEffect | 中 | — | I/II | 全图 ±8°C 8h |
| MMT_Magic_WeatherFertileRain | 丰饶之雨 | 天气 → SpawnEffect | 中 | — | II | 8h 雨+植物生长 +20% |
| MMT_Magic_WeatherDisperseFallout | 消散毒尘 | 天气 → SpawnEffect | 中 | — | III | 减半毒尘剩余 |
| MMT_Magic_WeatherAurora | 极光 | 天气 → SpawnEffect | 小 | — | III | 夜晚心情加成 |

#### 3.25 混沌法师（Deferred 职业）

RoM trait `ChaosMage`（中文名 ★）：唯一技能 TM_ChaosTradition = 随机重掷可用法术池，机制与 MMT 固定职业体系冲突，**职业整体 deferred**（§4），占位不入导师台列表。

### 斗士系（13）

斗士系技能同样统一为 Psycast 灵能消耗（§1.2 决策 2），冷却照抄 RoM。

#### 3.26 角斗士 `MMT_Class_Gladiator`

- 原型：RoM trait `Gladiator`；中文名 ★。主题：近战生存与横扫。被动：PainShockThreshold +0.05/+0.10/+0.15，每击固定减伤（RoM TM_Fortitude → 转被动 IncomingDamageFactor ×0.98/×0.96/×0.94）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_Sprint | 冲刺 ★ | 移速开关 → ToggleAura(自身) | 小 | — | I | 移速大增但疲劳累积 |
| MMT_Might_Grapple | 钩爪 ★ | 拉拽+扫倒 → 个案 comp | 小 | — | I | 简化：目标踉跄+定身，不做位移 |
| MMT_Might_Cleave | 顺劈斩 ★ | 近战横扫 → AoEDamage | 中 | — | II | RoM 自动施放改手动 |
| MMT_Might_Whirlwind | 旋风斩 ★ | 近战连斩 → AoEDamage | 中 | — | II | — |
| MMT_Might_GladiatorManeuver | 机动 ★(career) | 自身移速 → GiveHediff | 小 | — | III | +8% 移速 8s |
| MMT_Might_GladiatorSweep | 横扫 ★(career) | 双击 → AoEDamage | 中 | — | III | 65% 武器伤×2 |

#### 3.27 游侠 `MMT_Class_Ranger`

- 原型：RoM trait `Ranger`（4 档）；中文名 ★。主题：弓箭与陷阱。被动（训练技转化）：ShootingAccuracyPawn ×1.03/×1.06/×1.10。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_PoisonTrap | 毒素陷阱 ★ | 部署陷阱 → SpawnEffect | 小 | — | I | 限量 |
| MMT_Might_ArrowStorm | 箭雨风暴 ★ | 连射 → MultiShot | 中 | — | I | 5–8 连发 |
| MMT_Might_AnimalFriend | 动物伙伴 ★ | 召唤+羁绊 → Summon | 中 | — | II | 简化：限时召唤野生动物，不做永久羁绊 |
| MMT_Might_RangerSafeTrail | 安全小径 ★(career) | 路径加速 → GiveHediffAoE | 小 | — | II | 简化：范围友军加速，不标记路径 |
| MMT_Might_RangerBreakthrough | 协同突猎 ★(career) | — | — | — | — | Deferred（人宠协同 AI 编排） |
| TM_RangerTraining / BowTraining | 训练 ★ | — | — | — | — | 转阶位被动 |

#### 3.28 狙击手 `MMT_Class_Sniper`

- 原型：RoM trait `TM_Sniper`（4 档）；中文名 ★。主题：致命射击。被动：AimingDelayFactor ×0.97/×0.94/×0.90。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_SniperAimedShot | 瞄准射击 ★(career) | 强化射击 → LaunchProjectile | 小 | 30s | I | 110–140% 武器伤 |
| MMT_Might_DisablingShot | 残废射击 ★ | 麻痹弹 → LaunchProjectile+GiveHediff | 小 | — | I | 电击麻痹 |
| MMT_Might_Headshot | 爆头一击 ★ | 致命射击 → LaunchProjectile | 中 | — | II | 高伤要害 |
| MMT_Might_AntiArmor | 破甲 ★ | 破甲弹 → LaunchProjectile+GiveHediff | 中 | — | II | 撕甲 |
| MMT_Might_ShadowSlayer | 暗影杀手 ★ | 隐身 → GiveHediff | 中 | — | III | 长时隐形力场 |
| MMT_Might_SniperArmorFracture | 破甲射击 ★(career) | 团队破甲窗口 → GiveHediffAoE | 中 | — | III | 6s 破甲缺口 |
| MMT_Might_SniperSingleOpening | 一线战机 ★(career) | 引导 3 连狙 → MultiShot | 大 | — | III | 5s 瞄准后 120%×3 |

#### 3.29 刀锋舞者 `MMT_Class_Bladedancer`

- 原型：RoM trait `Bladedancer`（4 档）；中文名 ★（剑舞者/刀锋舞者）。主题：位移连斩。被动：MeleeHitChance +2/+4/+6，MoveSpeed +0.1/+0.2/+0.3。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_BladeSpin | 剑刃回旋 ★ | 周身斩 → AoEDamage | 小 | — | I | — |
| MMT_Might_PhaseStrike | 次元之刃 ★ | 相位背刺 → DashStrike | 中 | — | I | 闪现背刺 |
| MMT_Might_SeismicSlash | 地裂斩 ★ | 直线裂斩 → AoEDamage | 中 | — | II | — |
| MMT_Might_BladePassage | 穿刃步 ★(career) | 直线突进斩 → DashStrike | 中 | — | III | — |
| MMT_Might_BladeSevenSteps | 七步式 ★(career) | — | — | — | — | Deferred（12 格路径 7 连斩，需路径编排） |
| TM_BladeFocus / BladeArt | 剑术精通 ★ | — | — | — | — | 转阶位被动 |

#### 3.30 无面者 `MMT_Class_Faceless`

- 原型：RoM trait `Faceless`；中文名 ★。主题：欺诈与反制。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_Disguise | 认知遮蔽 | 近似隐身 → GiveHediff | 中 | — | I | — |
| MMT_Might_Reversal | 反转 | 反弹护盾 → GiveHediff | 中 | — | II | 短时免疫近战/投射物 |
| MMT_Might_Transpose | 移形换位 | 与目标换位 → 个案 comp | 中 | — | II | 双方眩晕+自获反弹 |
| MMT_Might_Possess | 精神操控 | 心灵 → MentalState | 大 | — | III | 简化：目标狂暴错乱，不接管控制 |
| TM_Mimic / FacelessEcho | 模仿/借来的招式 ★(career) | — | — | — | — | Deferred（复制存储他人技能） |

#### 3.31 灵能武者 `MMT_Class_Psionic`

- 原型：RoM trait `TM_Psionic`；中文名 ★。主题：灵能近战。与灵能系统天然契合（RoM 本就灵能主题）。被动：PsychicSensitivity ×1.05/×1.1/×1.15。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_PsionicBlast | 灵能爆弹 | 连射 → MultiShot | 小 | — | I | 最多 5 连 |
| MMT_Might_PsionicBarrier | 灵能屏障 | 护盾 → GiveHediff | 中 | — | I | 自身/投射给他人 |
| MMT_Might_PsionicDash | 灵能冲刺 | 冲刺+冲击波 → DashStrike | 中 | — | II | — |
| MMT_Might_PsionicKineticCrucible | 动能熔炉 ★(career) | 引导脉冲+群体护盾 → AoEDamage+GiveHediffAoE | 大 | — | III | 4 脉冲+友军 60 点屏障 |
| MMT_Might_PsionicStorm | 灵能风暴 | 区域伤害 → AoEDamage | 大 | — | III | 自耗巨大 |
| TM_PsionicAugmentation | 灵能强化 | — | — | — | — | 转阶位被动 |

#### 3.32 死亡骑士 `MMT_Class_DeathKnight`

- 原型：RoM trait `DeathKnight`；中文名 ★。主题：恐惧与吸血。被动：近战吸血（RoM TM_LifeSteal → 转被动，复用 ModuleSystem 吸血键 `Lifesteal` 逻辑）1%/2%/3%。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_Spite | 怨毒冲击 | 投射物 → LaunchProjectile | 小 | 2–5s | I | RoM 耗"憎恨"→灵能化 |
| MMT_Might_WaveOfFear | 恐惧之波 | 恐惧+削弱 → MentalState | 中 | — | II | 暗防/移速/攻速削减 |
| MMT_Might_GraveBlade | 墓地灵剑 | 灵剑突起 → AoEDamage | 中 | — | II | — |
| MMT_Might_Shroud | 黑暗斗篷 | 自身光环 → ToggleAura | 中 | — | III | RoM"憎恨积攒"简化为减伤+近战强化光环 |
| TM_LifeSteal | 吸血 | — | — | — | — | 转阶位被动 |

#### 3.33 武僧 `MMT_Class_Monk`

- 原型：RoM trait `TM_Monk`；中文名 ★。主题：空手格斗（RoM"气"资源 → 灵能化）。被动：MeleeDodgeChance +3/+6/+9，IncomingDamageFactor ×0.97/×0.94/×0.90（MindOverBody 转化）。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_ChiBurst | 气爆 | 范围打断 → AoEDamage | 小 | — | I | — |
| MMT_Might_Meditate | 冥想 | 自愈+降熵 → 个案 comp | 小 | — | I | 灵能化：净化身心+熵下降 |
| MMT_Might_DragonStrike | 龙翔踢 | 飞跃踢击 → DashStrike | 中 | — | II | — |
| MMT_Might_MonkSilentMeridian | 寂静经脉 ★(career) | 站桩脉冲 → AoEDamage | 中 | — | III | 5s 后 3 脉冲 |
| TM_TigerStrike / ThunderStrike / MonkPalmSeal / Chi / MindOverBody | 近战连招系 | — | — | — | — | Deferred（近战多段连招需自定义 Verb）；Chi/MindOverBody 转阶位被动 |

#### 3.34 指挥官 `MMT_Class_Commander`

- 原型：RoM trait `TM_Commander`；中文名 ★。主题：群体光环（**光环大户，性能注意 §6**）。被动：SocialImpact、NegotiationAbility 小幅。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_ProvisionerAura | 后勤光环 | 维持光环 → ToggleAura | 中 | — | I | 休息/饥饿改善 |
| MMT_Might_TaskMasterAura | 监工光环 | 维持光环 → ToggleAura | 中 | — | I | 工作速度+/社交- |
| MMT_Might_CommanderAura | 统御光环 | 维持光环 → ToggleAura | 中 | — | II | 心情+/产能- |
| MMT_Might_MoveOut | 全员推进 | 群体移速 → GiveHediffAoE | 中 | — | II | 免踉跄眩晕 |
| MMT_Might_StayAlert | 保持警戒 | 群体回复 → GiveHediffAoE | 中 | — | II | 回满休息+视野 |
| MMT_Might_HoldTheLine | 坚守阵地 | 群体减伤 → GiveHediffAoE | 大 | — | III | 减伤/忍痛/射速 |
| MMT_Might_CommanderWorkCrew | 工作队 ★(career) | 3 友军工速 → GiveHediffAoE | 小 | — | III | +10~20% |
| MMT_Might_CommanderRally | 集合点 ★(career) | 范围防护 → GiveHediffAoE | 中 | — | III | 简化：直接范围 buff，不设集合点实体 |
| MMT_Might_CommanderOperationEvac | 全员归队 ★(career) | 引导群体加速+防护 → GiveHediffAoE | 大 | — | III | — |
| MMT_Might_CommanderOperationAid | 紧急工作 ★(career) | — | — | — | — | Deferred（按工作类型筛选目标+引导维持） |

#### 3.35 超级士兵 `MMT_Class_SuperSoldier`

- 原型：RoM trait `TM_SuperSoldier`；中文名 ★。主题：现代武器战术。被动（三专精转化）：ShootingAccuracyPawn ×1.03/×1.06/×1.10，AimingDelayFactor ×0.98/×0.96/×0.94。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_FirstAid | 自我急救 | 治疗自身 → Heal | 小 | — | I | — |
| MMT_Might_Buckshot | 散射鹿弹 | 散射 → MultiShot | 小 | — | I | — |
| MMT_Might_PistolWhip | 枪托猛击 | 击晕/缴械 → 个案（复用 ModuleSystem 缴械键逻辑） | 小 | — | I | — |
| MMT_Might_SuppressingFire | 压制火力 | 弹幕 → MultiShot | 中 | — | II | — |
| MMT_Might_Mk203GL | 枪挂榴弹 | 榴弹 → LaunchProjectile | 中 | — | II | — |
| MMT_Might_BreachingCharge | 爆破炸药 | 对建筑特化 → LaunchProjectile | 中 | — | II | 对建筑 30 倍 |
| MMT_Might_SuperSoldierFieldDressing | 战地包扎 ★(career) | 耗药自疗 → Heal | 小 | — | III | 耗 1 药品 |
| MMT_Might_60mmMortar | 便携迫击炮 | 迫击 → LaunchProjectile | 大 | — | III | — |
| TM_CQC | 近战连招 | — | — | — | — | Deferred（近战连招） |
| TM_PistolSpec/RifleSpec/ShotgunSpec | 武器专精 | — | — | — | — | 转阶位被动 |

#### 3.36 药剂师 `MMT_Class_Apothecary`

- 原型：RoM trait `TM_Apothecary`（耗"草药"→灵能化）；中文名 ★。主题：药剂与治疗。被动：MedicalTendSpeed ×1.1/×1.2/×1.3。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_SoothingBalm | 舒缓药膏 | 治疗 → Heal | 小 | — | I | 止血抗感染止痛 |
| MMT_Might_PoisonFlask | 毒气瓶 | 投射物+毒 → LaunchProjectile+GiveHediff | 小 | — | I | — |
| MMT_Might_ApothecaryAntitoxin | 解毒剂 ★(career) | 治疗 → Heal | 小 | — | II | 减食物中毒 |
| MMT_Might_Elixir | 万能灵药 | 治疗+强化 → Heal+GiveHediff | 中 | — | II | — |
| MMT_Might_HerbalClarity | 清明药剂 | 灵能回复 buff → GiveHediff | 中 | — | II | 灵能化：加速心灵集中恢复 |
| MMT_Might_FireSuppressionFlask | 灭火罐 | 范围灭火 → SpawnEffect | 小 | — | III | — |
| MMT_Might_ApothecaryGoldenHour | 黄金急救 ★(career) | 引导群体治疗 → Heal(AoE) | 大 | — | III | 8 格内 4 名友军 |
| MMT_Might_HerbalBliss | 极乐药 | 强化（成瘾风险） → GiveHediff | 中 | — | III | 遮蔽疼痛 |
| TM_Herbalist | 草药辨识 | — | — | — | — | 转阶位被动（采集速度） |

#### 3.37 旅者 `MMT_Class_Wayfarer`

- 原型：RoM trait `TM_Wayfarer`；中文名自译。主题：斗士版通用成长。无专属技——**III 阶解锁通用技能池（§3.39）中的斗士通用技**。
- 阶位被动：MoveSpeed +0.1/+0.2/+0.3，CarryingCapacity +10/+20/+30，ImmunityGainSpeed ×1.05/×1.1/×1.15。

#### 3.38 战地军医 `MMT_Class_FieldMedic`

- 原型：RoM 血清授予职业（无 trait，无汉化）；中文名自译。主题：战场救护。被动：MedicalTendSpeed ×1.1/×1.25/×1.4，MedicalTendQuality +0.05/+0.1/+0.15。

| 技能 defName | 中文名 | 桶 → comp | 档 | 冷却 | 阶 | 备注 |
|---|---|---|---|---|---|---|
| MMT_Might_Medigel | 医疗凝胶 | 缓释治疗 → Heal | 小 | — | I | — |
| MMT_Might_Emergency | 紧急动员 | 自身强化 → GiveHediff | 中 | — | II | 移速/医护速度/品质 |
| MMT_Might_CombatDrugs | 战斗药剂 | 随机正面药剂 → 个案 comp | 中 | — | II | 投掷给友军 |
| MMT_Might_MedicalSupply | 医疗补给 | 生成药品 → SpawnEffect | 中 | 长 | III | 从虚空补给改长冷却 |

#### 3.39 通用技能池（Standalone 精选）

RoM 跨职业通用技（45 个 Standalone + 7 个 Sustained）经筛选后作为**流浪法师/旅者的 III 阶技能**，不开放给其它职业（保持职业特色）：

| 技能 defName | 中文名 | 桶 → comp | 档 | 职业 | 备注 |
|---|---|---|---|---|---|
| MMT_Magic_MageLight | 法师之光 | 生成光源 → SpawnEffect | 小 | 流浪法师 | — |
| MMT_Magic_SmokeCloud | 烟幕 ★ | 烟雾 → Smokepop（原版） | 小 | 流浪法师 | — |
| MMT_Magic_CauterizeWound | 烧灼止血 ★ | 治疗 → Heal | 小 | 流浪法师 | — |
| MMT_Magic_ArcaneBolt | 奥术箭 | 投射物 → LaunchProjectile | 小 | 流浪法师 | — |
| MMT_Magic_Extinguish | 灭火 ★ | 灭火 → SpawnEffect | 小 | 流浪法师 | — |
| MMT_Magic_Invisibility | 隐身 | 隐身 → GiveHediff | 中 | 流浪法师 | — |
| MMT_Might_Taunt | 嘲讽 | 心灵 → MentalState | 小 | 旅者 | 激怒目标冲向自己 |
| MMT_Might_ShieldOther | 援护之盾 | 给他人套盾 → GiveHediff | 中 | 旅者 | — |
| MMT_Might_ThrowingKnife | 飞刀 | 投射物 → LaunchProjectile | 小 | 旅者 | — |
| MMT_Might_HeavyBlow | 强力打击 ★ | 下次攻击强化 → GiveHediff | 小 | 旅者 | RoM Sustained 改一次性 |
| MMT_Might_ThickSkin | 石化皮肤 ★ | 减伤开关 → ToggleAura | 中 | 旅者 | — |
| MMT_Might_InnerHealing | 内在治疗 ★ | 自愈开关 → ToggleAura | 中 | 旅者 | — |

通用池其余（热源/冷源/发电节点/日光灯塔/沃土/法力转移/装备维修/次元衣柜/传授经验/军团复制等）全部 deferred 或不适合移植（与 MMT 生产链定位重叠），见 §4。

---

## 4. Deferred 清单（本阶段不实现，映射表登记 `deferred`）

原则：机制独特、无法用 §2.3 通用库表达，或与 MMT 既有系统冲突者延后；后续阶段按需单个复活。映射表 `--check` 保证 RoM 技能全集 = 已映射 + 本清单。

**机制独特类**（实现需专项设计）：

| RoM 技能 | 职业 | 延后原因 |
|---|---|---|
| TM_FoldReality | 奥术法师 | 双向星门跨格转移实体 |
| TM_RegrowLimb | 德鲁伊 | 生成手术材料+解锁手术，跨医疗系统 |
| TM_ConsumeCorpse(+Mass) | 死灵法师 | 吞尸体回蓝，灵能模型无对应资源 |
| TM_LichForm | 死灵法师 | 永久变身（改 race/不死化），不可逆 |
| TM_SoulBond / ShadowCall / ShadowStep | 恶魔术士 | 双 pawn 契约纽带+互相传送 |
| TM_DispelStoneskin / EarthSprites / ShatterSentinel | 地占师 | 配套解除技/地形改造/多召唤物联动引爆 |
| TM_TechnoBit / TechnoWeapon | 技法师 | 自主随从 AI/法力铸造武器 |
| TM_Transmutate / EnchanterStone / EnchantWeapon / Polymorph / Shapeshift | 附魔师 | 材质转换/永久造物/与 ModuleSystem 附魔冲突/变形 |
| TM_Prediction / ReverseTime / ChronostaticField / TimeMark / Recall | 时空法师 | 资源积攒/腐化逆转/时间静滞/状态快照回溯 |
| TM_SoL_CreateLight / Equalize | 光法师 | 召唤物联动 |
| TM_Mimic / FacelessEcho | 无面者 | 复制存储他人技能 |
| TM_TigerStrike / ThunderStrike / MonkPalmSeal / CQC / BladeSevenSteps | 武僧/超级士兵/刀锋舞者 | 近战多段连招/路径斩，需自定义 Verb |
| TM_HarvestPassion / IncitePassion / Symbiosis | 共情者 | 操作 passion 数据/双 pawn 命运绑定 |
| TM_SpiritPossession / ControlSpiritStorm | 附身灵 | 接管 pawn 控制权/引导移动风暴实体 |
| TM_Golemancy / RuneCarving / SiphonBrand / SigilSurge / SigilDrain / LivingWall | 傀儡师 | 傀儡制作体系/烙印网络管理/墙体活化 |
| TM_CommanderOperationAid | 指挥官 | 按工作类型筛选目标+引导维持 |
| TM_RangerBreakthrough | 游侠 | 人宠协同 AI 编排 |
| TM_ChaosTradition | 混沌法师 | 职业整体 deferred（随机技能池机制） |

**Dismiss 解除技**（约 10 个，多职业）：召唤物全部改为限时 Hediff 到期自动消失，解除技失去存在意义，统一 deferred。

**通用池未入选部分**：TM_Heater/Cooler/PowerNode/Sunlight（与 MMT 生产链/电力定位重叠）、TM_TransferMana/SiphonMana（pawn 间法力转移，灵能模型无对应）、TM_ChargeBattery/SpellMending/MagicWardrobe/TeachMagic/TeachMight/Legion（充电池/修装备/次元衣柜/传授经验/技能复制）、TM_Explosion（彩蛋大爆炸）、TM_Artifact_Conviction（强行招募）、TM_Weapon_WintersFury（神器武器技）、天气改造类中与 §3.24 重复者。

## 5. 配套设计

### 5.1 建筑与物品

- `MMT_ClassMentorBench` 职业导师台：2×2，造价 微芯片×6 + 高级合金×40 + 玻璃钢×60，WorkToBuild 6000，功耗 800W；FloatMenu 交互（复用 `ChargingStation.cs` 模式），法师/斗士两系分组列出 37 职业；贴图优先从 RoM `Textures/Equipment`/`Other` 找候选，无合适按 `.claude/skills/rw-texture-design/SKILL.md` 写描述后 AI 生成（`<名>.png`+`<名>_m.png`）；
- `MMT_ClassToken` 职业凭证：单种通用凭证（37 职业若各发凭证不可控），Dev 模式 `[DebugAction]` 发放；图标用 RoM `Textures/UI` 符文/图腾类候选。

### 5.2 灵能觉醒

就职时检查灵能链接：无则调原版 API 觉醒 1 级（`ChangePsylinkLevel` 系，§8 spike）；觉醒是就职的一部分（"职业启蒙"叙事），升阶不再提升灵能等级——玩家想压更高熵上限可自行走原版灵能内容（apothecary 清明药剂等提供协同）。

### 5.3 品质暴击

原 4 职业草案的工匠大师 III 阶品质暴击改为**第 19 个独立 Perk** `MMT_Perk_Masterwork`（生存/生产支）：消耗 perk 点×2，制作成品 10% 概率品质 +1 档；Harmony postfix `QualityUtility.GenerateQualityCreatedByPawn`（§8 spike）。设置开关 `classQualityCritEnabled`（默认开）。

### 5.4 贴图/音效/特效复制规则

- 技能图标：RoM `Textures/UI/<语义名>.png` → `1.7/Textures/UI/RoM/`（生成器按映射表顺带复制，约 174 张）；
- 投射物贴图：RoM `Textures/Spells/` → `1.7/Textures/Things/Projectile/RoM/`（投射物技能按需）；
- 音效：生成器照抄 RoM 技能的 `soundCast`/`soundCastTail`；TM_* 音效更名 `MMT_*` 生成 `MMT_Sounds_RoM.xml`（RoM SoundDef 复制件），wav 复制到 `1.7/Sounds/RoM/`；原版音效名原样引用；
- 战斗特效：RoM MoteDefs 复刻为 FleckDef（`MMT_Flecks_RoM.xml`，贴图 `1.7/Textures/Things/Fleck/RoM/`）；每个技能经 `CompAbilityEffect_MMTFleckFX` 在施法者/目标点放主题 fleck（职业主题表 `CLASS_FLECK` + 桶级覆盖 + 映射表 `fx` 字段单技能覆盖），AoE 桶的 `fleckDefName` 默认填主题 fleck；
- About.xml 备注资源来源（RoM 已授权）。

### 5.5 设置项（`MMTSettings.cs` 追加）

| 设置项 | 字段 | 默认 | 说明 |
|---|---|---|---|
| 历练/职业总开关 | pawnProgressEnabled | true | 关闭不再获得 XP、gizmo/页签隐藏、既有 Hediff 保留 |
| XP 飘字 | xpGainMotes | **false** | 性能项（§6） |
| 光环脉冲间隔 | classAuraInterval | 300 tick | 性能项（§6），调大降载 |
| 品质暴击 | classQualityCritEnabled | true | MMT_Perk_Masterwork 生效开关 |

### 5.6 校验套件

- `tools/validate/pawn_progress.py`：历练 20 stage 连续、perk/职业 Hediff stat 引用有效、导师台造价与凭证 defName 有效；
- `tools/validate/rom_classes.py`：生成 Def 引用有效（AbilityDef→comp/HediffDef/图标文件存在）、`build_mmt_rom_classes.py --check` 等价核对（RoM 技能全集 = mapped + deferred）、deferred 清单与本文档 §4 一致。

## 6. 性能预算（取舍标注）

| 项 | 开销 | 结论 |
|---|---|---|
| 伤害/击杀/技能 XP 三钩子 | 每次命中/击杀/技能经验一次加法，无分配 | 廉价，常开 |
| 历练/perk/职业 stat | Hediff stage 原生 statOffsets，吃原版 stat 缓存，仅阶位变化时重算 | 廉价，常开 |
| 普通 Psycast 施放（投射物/AoE/治疗/召唤） | 原版框架，一次性事件 | 廉价，常开 |
| **维持型光环/开关技**（约 20 个：抚慰之风/内心之火/指挥官三光环/Sprint/淬毒/石肤等） | **全套系统唯一持续 tick 开销**：每个激活光环按间隔对半径内 pawn 扫描并施加/续期 Hediff；N 个光环 × 半径扫描 | **间隔脉冲实现（默认每 300 tick 一次），`classAuraInterval` 可调大降载；熵压天然限制同时激活数量**；标注为性能注意项，低配机器建议少开光环 |
| **XP/升级飘字 fleck** | 命中密集时 fleck 分配+视觉刷屏 | **默认关**（`xpGainMotes`） |
| 召唤物 | 限时 Hediff 到期消失 + 数量上限参数兜底，无自研 AI | 廉价 |
| AoE 脉冲技（陨石/雷暴/轨道打击） | 一次性天坠物/有界脉冲 | 廉价 |
| 品质暴击 postfix | 每件成品一次掷点 | 廉价（留开关） |

承诺：无 per-tick 全图扫描（光环为间隔事件）；熵上限即资源阀门，无需自研法力系统。

## 7. 实施阶段建议（供后续开工参考）

| 阶段 | 内容 | 主要产出 |
|---|---|---|
| B1 | 框架：PawnProgress + 历练/Perk XML + ITab + XP 钩子 | `PawnProgress.cs`、`MMT_Hediffs_PawnProgress.xml`、ITab |
| B2 | 技能效果库 + 导师台 + 灵能觉醒 + Debug 发凭证 | `ClassAbilityEffects.cs`、`PawnClass.cs`、导师台/凭证 XML |
| B3 | 生成器 + 3 个代表职业端到端（火法=投射物/AoE、牧师=治疗/buff、角斗士=斗士近战） | `build_mmt_rom_classes.py`、映射表初版 |
| B4 | 映射表铺完全部 37 职业 + deferred 定稿 + 贴图音效复制 | 全量生成物 |
| B5 | 收尾：Keyed、设置面板、校验套件、文档回填、AGENTS.md | `validate/pawn_progress.py`、`rom_classes.py` |

每阶段验证：`dotnet build Source/MyModeTest/MyModeTest.csproj` + `PYTHONIOENCODING=utf-8 python tools/validate_new.py <套件>` + `python tools/validate_mod.py all` 回归。

游戏内手测路径：Dev 发凭证 → 建导师台 → 37 职业分组 FloatMenu → 就职（自动觉醒灵能）→ 技能 gizmo 出现、耗心灵集中/涨熵 → 升 II/III 阶解锁全技能 → 升级选 perk（含品质暴击 perk）→ ITab 正确；抽查各桶代表技能（火球/圣光治疗/召唤仆从/闪烁/恐惧支配/箭雨/指挥官光环的脉冲间隔表现）。

## 8. 待定项（实现时 spike 回填）——已全部回填（v1）

- ~~灵能觉醒 API~~ → 1.7 为 `RimWorld.PawnUtility.ChangePsylinkLevel(Pawn pawn, int levelOffset, bool sendLetter)`，就职时 `offset=1, sendLetter=false`（已实现）；
- ~~品质生成 patch 点~~ → `QualityUtility.GenerateQualityCreatedByPawn(Pawn, SkillDef)` 存在且已 postfix（`Patch_MasterworkQualityCrit`）；
- ~~心灵控制原版先例~~ → 单体走原版 `CompProperties_AbilityGiveMentalState`（纯 XML），范围用自研 `CompAbilityEffect_MMTMentalStateAoE`；
- ~~stat defName 核对~~ → 1.7 变更：`CookingSpeed` 更名 `CookSpeed`；**1.7 已删除 HungerRate 属性** → 饥饿类效果改用 HediffStage `hungerRateFactor`；其余（ShootingAccuracyPawn/AimingDelayFactor/SocialImpact/NegotiationAbility/MedicalTendQuality/ComfyTemperatureMin/PsychicEntropyRecoveryRate）均核实存在；
- ~~RoM 冷却/伤害回填~~ → 生成器自动读 RoM `SecondsToRecharge` 回填冷却（映射表可覆盖），伤害按 §2.2 威力档设计值；
- ~~召唤物选型~~ → 已落定（映射表 `summonBuildings`/`pawnKind`）：仆从=Colonist、波皮=Boomrat、元素/血月恶魔=Chimera、恶魔=Devourer、亡灵=Ghoul、石哨兵=Megaspider、守护灵兽=Thrumbo、灵狼=Wolf_Timber/Wolf_Arctic、动物伙伴=Bear_Grizzly、光之精灵=Fingerspike。

**实现偏差记录（v1，供溯源）**：

1. 1.7 的 `HediffStage` **没有** abilities 字段（abilities 在 HediffDef 级、整 Hediff 生效）——§2.1 的"升阶=摘除低阶+添加高阶"原样成立，各阶 HediffDef 的 `<abilities>` 累积列出（I⊂II⊂III，校验套件核对）；
2. 伤害 XP 钩子：§2.1（rpg-design）原方案"复用武器机制伤害补丁上下文"不可行（该补丁在无强化 comp 时短路），实现改为 `Pawn.PostApplyDamage` postfix 读 instigator，全伤害源覆盖且同样廉价；
3. 召唤建筑（魔法防御塔/神经炮塔/三图腾/法力地雷）落地为 `SummonTurret.cs` 参数化建筑 comp（attack/heal/mine 三模式）+ 原版 `CompProperties_Lifespan` 限时，生成器 `summonBuildings` 段产出；
4. 职业导师台造价：MMT 无"玻璃钢"与通用"高级合金"物品，取最近似高级链产物——`MMT_PobediteAlloy`（钨钴合金）×40 + `MMT_ReinforcedGlass`（强化玻璃块）×60；
5. 贴图：技能图标/投射物贴图已从 RoM 复制（174 图标 + 20 投射物）；**导师台与召唤建筑暂用 RoM 防御塔贴图占位**（`Things/Building/Production/RoM/`，待 AI 生成专属贴图）；音效与战斗特效 v2.1 已复刻（17 个 SoundDef + 33 个 FleckDef，照抄 RoM soundCast 与主题 Mote，见 §5.4）；
6. 就职/升阶材料改为"随身携带"制（右键菜单明示需求），作业为走到导师台短仪式；
7. 混沌法师未入导师台（37 职业 = 法师 24 + 斗士 13）。

## 9. v2：历练 × 职业深度绑定（天赋树 / 全源经验 / 公会委托，2026-11）

参照 Isekai RPG Leveling（workshop 3657580708，贴图已获授权直接复制；代码仅作实现参考）重构成长体系。v1 的"perk FloatMenu + 阶位 Hediff 带技能"被天赋树取代。

### 9.1 体系总览

- **历练上限 20→60 级**，曲线仍为 `100×N^1.5`；每级 +1 **天赋点**（取代每 3 级 1 perk 点）。20 档历练 Hediff 不变（20 级后成长全走天赋树）。
- **通用天赋树**（`MMT_TalentTree_General`，59 节点）：战斗/灵能/生产/生存/社交五支；19 个旧 perk 原 Hediff 节点化并入；五支末端挂 10 个**家族解锁 Keystone**（ClassUnlock，cost 3）。
- **职业家族树**（10 棵，`MMT_TalentTree_F_<key>`）：37 职业分 10 家族（元素/奥秘/死灵/圣光/自然/心灵/剑刃/先锋/神射/战辅）；Start 就职免费解锁；主干家族被动 + 每职业一簇技能链（requireTier=技能 stage）+ 2 个强化节点（II/III 阶门控）+ 家族 Keystone。
- **就职流程不变**：点出家族解锁 → 导师台 + 凭证就职（菜单只列已解锁家族）；升阶门槛随 60 级重排为 **5/20/35 级**。
- **技能授予改道**：阶位 Hediff 不再携带 `<abilities>`（生成器已改），技能由家族树技能节点直接 `GainAbility`；旧档迁移自动补发当前阶位应得技能节点。
- **凭证来源**：A 级以上公会委托奖励（另保留 Dev 发放）。

### 9.2 数据与生成管线

- 新 Def：`MyModeTest.MMT_TalentTreeDef`（nodes + `"A~B"` 无向连线，`ResolveReferences` 建邻接表）；节点效果二选一：`hediff`（被动，解锁 AddHediff）/ `abilities`（技能，直接 GainAbility）；`classBaseName`（技能节点限定职业）、`familyKey`（ClassUnlock）、`requireTier`、`requireAllConnected`。
- 生成器 `tools/build_mmt_talent_trees.py` + 映射表 `tools/data/mmt_talent_tree_mapping.json`（唯一人工维护点：通用树布局、家族主题、perk 映射）；rom 映射表 classes 段新增 `family` 字段。
- 产出（生成物）：`MMT_Talents_General.xml`、`MMT_Talents_Families.xml`、`MMT_Hediffs_Talents.xml`（201 个被动 Hediff）；并复制 Isekai 授权贴图 41 张到 `1.7/Textures/UI/TalentTree/`（节点形状 8 + 图标 27 + UI 底图 3 + 世界图标 3）。
- 校验套件 `tools/validate/talent_tree.py`：图连通/唯一 Start/cost 规则/家族覆盖/技能全覆盖 requireTier=stage/stat 白名单/贴图存在/C# 接线。

### 9.3 全源经验（TalentXPPatches.cs）

在既有三钩子（伤害/击杀/技能学习）外新增：工作完成按 JobDef 分档（手术 25/治疗 18/制作研究烹饪 8/采矿 6/建造收获 4/搬运 2/清洁 1，60 tick 冷却）、品质产出（糟糕 2→传奇 75）、社交 2、贸易 15、招募 38、捕获（12 格内殖民者各 6）、击杀 combatPower 分档加成（2→160）、灵能释放（5+熵耗，钳 3-50）、熵累积/集中消耗/冥想（2500 tick 冷却）。所有来源经 `AddXp` 统一乘 `MMTSettings.xpMultiplier`（默认 1.0，0.1-5）。

### 9.4 公会狩猎委托（GuildQuest.cs）

- `GameComponent_GuildQuests` 定时器：间隔（默认 1 天，设置 0.5-5）生成委托；段位 F-SSS 按殖民者历练水平 + 财富加权。
- F-D 本地图边缘刷敌对生物（动物永久猎杀/虫挂虫族派系）；B-A 世界站点群猎（4/6 只，2-8 格，`SiteMaker.MakeSite` + `MMT_SitePart_GuildHunt`，15 天过期）；S-SSS 站点单 Boss（combatPower ≥220）。
- **据点联动（委托×副本）**：世界委托优先挂载扫描仪已发现未清剿的据点（`GuildQuestUtil.TryFindLinkedSite`：`MMT_SitePart_*` 且非狩猎场 + 有敌对派系宿主/机械集群守卫 + 未被其它委托占用，取距家最近者）；清剿该据点即完成（战利品+委托报酬双份收益）。无合格据点时按 `guildQuestDungeonChance`（默认 35%）直建新副本（`DungeonSiteGen.GenerateRandomSite`：扫描仪同池同公式、仅抽有守卫条目、段位映射档位 B→I/A→II/S+→III，`generatedDungeon` 标记区分信件措辞；出无守卫据点则销毁回退），再回退自建狩猎场。完成检测走 `sawHostiles`（图上见过敌对威胁后全清→完成；站点消失时按 sawHostiles 区分清剿移除/超时判负），空投在站点已移除时回退殖民地。
- 期限：本地 7 天 / 世界 14 天（`QuestPart_Delay` 超时信号→失败）；完成检测由 GameComponent 250 tick 节流驱动（1.7 `QuestPart` 无 tick 虚方法；`QuestEndOutcome` 替代旧 `EndResult`）。
- 奖励：战斗图殖民者均分 XP（60×2^rank）+ 白银（50×1.8^rank）空投；B+ 附能量结晶 2-6；A+ 附职业凭证×1。

### 9.5 存档兼容与迁移（TalentTreeUtil.Migrate，幂等）

- 旧 perk Hediff/Def 全保留；读档时按"perk→节点"映射补登记通用树节点（不扣点），剩余 perk 点 1:1 转天赋点。
- 已就职小人：按当前阶位免费补发家族树 Start + 全部应得技能节点（阶 Hediff 去 abilities 后技能不丢）。
- 新字段：`talentPoints`/`unlockedTalents`（"tree:node" 字符串）/`unlockedFamilies`/`perkMigrated`，全部带默认值。

### 9.6 UI（TalentTreeWindow.cs，本 Mod 首个自定义 Window）

左侧树页签（通用+本族）/中间画布（右键平移、滚轮朝鼠标缩放 0.35-2.0）/右侧详情（效果文本、解锁/链式解锁按钮）/底部点数。连线 `Widgets.DrawLine` + Cohen–Sutherland 画布裁剪（GL 不吃 GUI 裁剪）；节点形状/图标用 Isekai 授权贴图，技能节点用 RoM 技能图标；双击链式解锁（Dijkstra 最短路，逐节点过门禁）。洗点 v1 仅 DebugAction。

### 9.7 已知边界（v2）

- 家族技能强化节点以主题被动为主（熵/集中/恢复），真升级变体（强化版技能）未做；
- 洗点 UI、精英词缀、称号决斗、法力核心掉落未做（Isekai 特性，非本系统目标）；
- 公会委托为自动接受制（无拒绝按钮），不做 QuestPart_Choice 报价流程；
- 游戏内手测未执行（见 §9.8 路径）。

### 9.8 v2 手测路径

Dev"天赋点+10"→ ITab/ gizmo 开天赋树 → 加点（含双击链式）→ 点出家族解锁 Keystone → 导师台就职（菜单过滤生效）→ 家族树 Start 自动解锁 → 技能节点出 gizmo → 放技能涨经验 → Dev"生成公会狩猎委托"→ 本地猎杀领奖（含凭证）→ 升 II 阶（历练 20 级+结晶×10）→ 高阶技能节点可点；旧档验证 perk/技能迁移。
