# MMT 新职业体系设计 · 总纲（第一批 5 职业）

> 手工维护的设计文档（非生成物）。本文档是**新职业体系**（牧师/圣骑士/猎人/工程师/法师）的唯一总纲。
> 各职业技能详情见分册：`mmt-class-priest.md`、`mmt-class-paladin.md`、`mmt-class-hunter.md`、`mmt-class-engineer.md`、`mmt-class-mage.md`；新贴图 AI 提示词见 `mmt-new-classes-texture-prompts.md`。
> 状态：**v0 设计阶段（仅文档，未实现）**。

## 1. 背景与目标

以 MMT 技能特效框架（SkillFX，第 1~21 步共 18 种 demoType，见 §5）为基础，设计 5 个类魔兽世界职业，每职业 15 个技能。RoM 复刻职业体系（`mmt-class-design.md`，36 职业）后续将**整体移除**——其问题太多；**历练等级（PawnProgress）与天赋树系统保留**，职业载体换为本文档的新职业。

与 RoM 职业体系的关系：

| 保留 | 移除/替换 |
|---|---|
| 历练等级 1-60 与全源 XP（`PawnProgress.cs`、`TalentXPPatches.cs`） | RoM 36 职业本体（Hediff/技能/生成器 `build_mmt_rom_classes.py`） |
| 天赋树框架（`TalentTreeDef.cs`/`TalentTreeWindow.cs`）与通用天赋树 | 10 棵职业家族树 → 改为 5 棵单职业树（§4） |
| 灵能熵/心灵集中消耗模型 | 职业凭证/导师台的就职交互可沿用，载体换名 |

## 2. 五职业定位总表

| 职业 | defName | 定位 | 主色（tint 基调） | 战斗半径 |
|---|---|---|---|---|
| 牧师 Priest | `MMT_Class_Priest` | 治疗辅助为主，圣光输出为辅 | 金白 (1, 0.9, 0.55) | 中距 15~25 格 |
| 圣骑士 Paladin | `MMT_Class_Paladin` | 坦克：自身减伤/护盾/掩护队友 | 金黄 (1, 0.8, 0.3) | 近战~中距 0~15 |
| 猎人 Hunter | `MMT_Class_Hunter` | 远程物理：超远狙击/陷阱/召唤野兽 | 自然绿棕 (0.55, 0.7, 0.35) | 远距 20~40 格 |
| 工程师 Engineer | `MMT_Class_Engineer` | 远程输出 + 召唤炮塔/机器人 | 工业橙 (1, 0.6, 0.2) | 中远距 15~30 |
| 法师 Mage | `MMT_Class_Mage` | 远程法系：冰控场/火主伤/奥辅助 | 火橙 (1,0.45,0.15) / 冰蓝 (0.6,0.85,1) / 奥紫 (0.7,0.4,1) | 中远距 15~30 |

设计原则：技能效果优先从 §5 的 18 种框架原型映射；映射不了的显式标注 **⚠ 需新框架原型/新机制**（汇总见 §6），不允许在文档里含糊带过。

## 3. 实施阶段

### 阶段一：技能先行（框架演示模式落地）

照现有演示技能（`1.7/Defs/Misc/MMT_SkillDemoSet.xml`）的模式落地，**不接职业/灵能/历练**：

1. 每职业一个技能集 Def：`MMTSkillDemoSetDef`（defName `MMT_SkillSet_<职业>`），15 个条目全部走 `MMTSkillDemoEntry` 参数字段；
2. **Dev 模式按职业一键赋予**：Debug 菜单新增 5 个动作「MMT 技能 → 赋予<职业>全套技能（选中小人）」，实现同 `MMTSkillDemoGrant`——运行时给选中小人注入 comp；另保留 1 个「移除全部职业技能」清理动作；
3. 新资产先行落地：新 Hediff（§8）、新投射物/新墙/新炮塔 Def、占位贴图（Pillow 占位 → AI 终稿覆盖，提示词见提示词文档）；
4. ⚠ 标记的非框架技能：阶段一可先做「框架内近似版」或直接留到 backlog 原型落地后补，各分册逐技能注明取舍；
5. 校验：`tools/validate/` 新增 `newclasses.py` 套件（校验 5 个技能集 XML、贴图存在性、引用 Def 存在性）。

### 阶段二：接入角色系统

1. 消耗：全技能接灵能熵/心灵集中，沿用 RoM 职业三档（小技 熵8/集中1%、中技 熵16/集中2%、大招 熵35/集中4%），各分册技能卡已预标档位；
2. 职业载体：每职业 3 个 HediffDef `MMT_Class_<名>_I/II/III`（保留 I/II/III 阶位升阶），阶位被动转 statOffsets/statFactors，做法同现有体系；
3. 天赋树重构：见 §4；
4. 移除 RoM 36 职业：生成器/Def/校验套件/文档一并下线，另立任务执行。

## 4. 天赋树重构（去家族树）

现状（`mmt-talent-tree.md`）：通用树 5 支末端 10 个「道」之 Keystone → 解锁 10 个职业家族 → 10 棵家族树（37 职业混编）。

**新方案**：

```
通用天赋树（保留，节点内容可微调）
   ↓ 支路末端：5 个新职业解锁 Keystone（各 3 点）
   战斗支 → 圣骑士之道 / 猎人之道
   灵能支 → 牧师之道 / 法师之道
   生产支 → 工程师之道
   ↓ 解锁对应职业就职资格（写 unlockedClasses，替代 unlockedFamilies）
就职（职业凭证，来源不变：公会委托/Dev；历练≥5、升 II 阶≥20 / III 阶≥35 不变）
   ↓
显示该职业自己的天赋树（每职业一棵，约 20~30 节点）：
   Start（0 费）→ 职业印记（1 点小被动）→ 技能节点链（按分册技能卡顺序，
   节点 requireTier 门控：I 阶技能直接可点，II 阶技能需 II 阶，III 阶大招需 III 阶）
   → 强化节点 ×2（熵恢复/熵上限 + 职业向小加成）→ 职业 Keystone（3 点，终极被动）
```

- 家族树概念整体废弃，`TalentTreeDef` 增加按职业过滤/显示的逻辑（只显示已就职职业的树）；
- 各分册 §天赋树节点草案 给出本职业树的节点清单（技能节点+被动节点+阶位门控），树坐标/布局留待阶段二实现时排。

## 5. SkillFX 框架速查（18 种 demoType）

参数字段名全部对齐 `Source/MyModeTest/AbilityTargeting.cs` 的 `MMTSkillDemoEntry`（行内注释即文档）。通用字段：`label / iconPath / radius（预览圈半径）/ targetFriendly / includeSelf / range（射程，默认 24.9）/ showRangeRing / castSeconds / soundDefName / castBarColor / ignoreLineOfSight`。

| demoType | 用途 | 关键参数 |
|---|---|---|
| `singleShot` | 单发投射物 | projectileDefName（伤害/速度在投射物 Def） |
| `funnelShot` | 浮游齐射（环绕后齐发） | funnelCount、ringRadius、aimTicks、funnelFleckDefName、projectileDefName |
| `beam` | 持续射线（三段式：发射头+光柱+命中端） | beamDurationSeconds、beamRange、beamWidth、beamTint、beamMuzzleOffset、beamHeadTexPath/beamTailTexPath、beamHeadSize/beamTailSize、beamDamage、beamDamageIntervalTicks、beamIgniteChance（伤害类型固定 Burn） |
| `strike` | 天降打击（落雷视觉，单体或连发） | strikeCount、strikeIntervalTicks、strikeAreaRadius、strikeInitialDelayTicks、strikeDamage、strikeArmorPenetration、strikeExplosionRadius、strikeIgniteChance、strikeRequireTarget |
| `groundFire` | 地面火焰（单柱/火海） | groundDurationSeconds、groundVisualScale、火柱堆叠参数（groundPillarHeight/Layers/TopScale/Sway）、groundDamage、groundDamageIntervalTicks、groundIgnitePawnChance、groundIgniteCellChance、groundAreaRadius、groundDensity、groundTexPath、groundTint |
| `aura` | 自身光环（免瞄准） | auraShieldMode（true=范围护盾，auraShieldHitPoints）、auraDurationSeconds、auraBuffDefName、auraRefreshIntervalTicks、auraTint |
| `bufffx` | 增益/减益（头顶图标+身上粒子） | buffHediffDefName、buffIconTexPath、buffIconTint、buffIconSeconds、buffParticleFleckDefName、buffRequireTarget |
| `blink` / `massTeleport` | 折跃 / 群体传送（两段瞄准） | range |
| `charge` / `jump` | 冲锋 / 跳跃 | chargeTicksPerCell、jumpTicksPerCell、jumpPeakHeight、拖尾 fleck |
| `summonOrb` | 漂浮雷球（脉冲伤害） | summonOrbTexPath/summonOrbTint、summonOrbTicksPerCell、summonDamage、summonDamageDefName、summonPulseRadius、summonPulseIntervalTicks |
| `summonHoming` | 追踪弹 | summonHomingTexPath/summonHomingTint、summonHomingTicksPerCell、summonDamage、summonHomingExplosionRadius、summonHomingDurationSeconds |
| `summonAnimal` | 召唤生物 | summonAnimalKindDefName、summonAnimalDurationSeconds（阵营=施法者派系） |
| `summonTurret` | 召唤临时炮塔 | summonTurretDefName、summonTurretDurationSeconds |
| `delayedStrike` | 延时打击（预警圈→坠落→爆炸） | meteorDelaySeconds、meteorFallTicks、meteorDamage、meteorExplosionRadius、meteorIgniteChance、meteorDrawSize、meteorSpinDegPerTick、meteorGrowFrom/meteorGrowTo（坠落物贴图固定 MMT_Meteor） |
| `knockback` | 近战击退 | knockbackDamage、knockbackCells、knockbackWallSlamDamage |
| `chainLightning` | 连锁闪电 | chainBounceCount、chainBounceRange、chainDamage、chainDamageFalloff |
| `breathCone` | 锥形吐息 | breathRange、breathHalfAngleDegrees、breathDurationSeconds、breathDamage、breathDamageIntervalTicks、breathIgniteChance |
| `heal` | 范围治疗（治疗量池按目标分配，绿十字+治疗粒子） | healAmount |
| `wall` | 造墙（圈选范围铺临时墙） | wallDurationSeconds（墙体建筑固定 MMT_StoneWall） |
| `groundZone` | 持续地面区域（毒/冰冻硬控等皮肤） | zoneDurationSeconds、zoneIntervalTicks、zoneAffectEveryone、zoneDamage、zoneDamageDefName、zoneHediffDefName、zoneHediffSeverity、zoneStaggerTicks（冰冻定身）、zoneIceScale、zoneTexPath、zoneTint |
| `mine` | 触发式陷阱 | mineArmSeconds、mineTriggerRadius、mineExplosionRadius、mineDamage、mineDurationSeconds |

## 6. 框架缺口与扩展项登记

以下效果现有框架**做不到**，技能卡中以 ⚠ 标注。按建议处理方式分三档：

### A. backlog 已有规划的新原型（落地后技能直接可用）

| 缺口 | backlog 出处 | 本批使用方 |
|---|---|---|
| 新星环（360° 扩散冲击波，命中环带上目标） | §一.1 | 牧师·神圣新星、法师·冰霜新星/魔爆术 |
| 驱散/净化（移除友方减益/敌方增益） | §一.5 | 牧师·驱散魔法/群体驱散 |
| 隐身（半透明+不可选为目标） | §二.9 | 猎人·伪装 |
| 照明/开视野 | §二.12 | 猎人·照明弹（可选，低优先） |
| 复活 | 无，全新机制（参考原版复活血清 `ResurrectorMechSerum` 流程） | 牧师·复活术 |

### B. 小扩展（现有原型加参数，工作量小）

| 扩展项 | 说明 | 使用方 |
|---|---|---|
| 投射物命中附加 Hediff | `singleShot`/`funnelShot` 投射物命中后挂指定 Hediff（减速/中毒/点燃已有对应伤害类型，但"减速"类需此出口） | 猎人·震荡射击、法师·寒冰箭 |
| `mine` 触发皮肤 | 触发后不爆炸，改为挂 Hediff/定身（backlog §一.2 已列为"后续可选扩展"） | 猎人·冰霜陷阱/毒蛇陷阱 |
| `summonAnimal` 数量参数 | 一次召 N 只 | 猎人·兽群召唤 |
| `strike`/`delayedStrike` 皮肤参数 | 落雷视觉换冰柱/圣光柱（加 texPath/tint 字段） | 法师·暴风雪(备选)、牧师·圣光审判 |
| `bufffx` 覆盖物 | 给目标罩全身贴图（复用 `Thing_MMTIceBlock` Attach 机制） | 法师·寒冰屏障 |
| `wall` 墙体 Def 参数 | 墙体建筑可配（石墙→冰墙/能量屏障） | 法师·冰墙术、工程师·能量屏障 |
| `beam` 伤害类型参数 | 现在固定 Burn | 工程师·过载射线（想用电击） |

### C. 新机制（不是特效原型，是机制）

| 机制 | 说明 | 使用方 |
|---|---|---|
| 嘲讽 | 强制范围内敌人把目标切到施法者（改 `AttackTargetFinder` 权重或给敌人挂"仇恨锁定"Hediff+JobGiver） | 圣骑士·嘲讽 |
| 召唤物 PawnKind 资产 | 牧师·守护天使（光元素）、工程师·机器人——框架支持（`summonAnimal` 阵营=施法者派系），但需要新 race+PawnKind+贴图；**注意：原版机械体 PawnKind 直接召唤会因无机械师 overseeing 而失效，机器人必须做成仿 `MMT_Races_FireElemental` 的普通 race**，不可直接用 Mech_* | 牧师、工程师 |

## 7. 资源约定

### 音效（全部已有 Def，直接引用 defName）

- RoM 复刻（`1.7/Defs/SoundDefs/MMT_Sounds_RoM.xml`）：`MMT_AirWoosh`、`MMT_AirWooshLow`、`MMT_DemonDeath`、`MMT_FastReleaseSD`、`MMT_FireBurn`、`MMT_FireWooshSD`、`MMT_Gong`、`MMT_Launch`、`MMT_LightZip`、`MMT_Powerup`、`MMT_Pulse`、`MMT_SoftExplosion`、`MMT_Thunder_OffMap`、`MMT_Vibration`、`MMT_VibrationLow`、`MMT_WolfHowl`、`MMT_Woosh`；
- 武器音效（`MMT_Sounds_Weapons.xml`，狙击/榴弹/激光等）：`MMT_ShotM82`、`MMT_SRSFire`、`MMT_RF_ShotHecateII`、`MMT_RF_ShotMilkorMGL`、`MMT_ShotRPG`、`MMT_RailgunShot`、`MMT_ShotLaser`、`MMT_LaserBeam`、`MMT_BlowgunFiring`、`MMT_Crossbow_S_hold` 等；
- 原版（演示集已用，确认存在）：`Shot_IncendiaryLauncher`、`ChargeLance_Fire`、`FireSpew_Resolve`、`Explosion_Flame`、`Thunder_OnMap`、`ChunkRock_Drop`。

### 图标

优先复用 `1.7/Textures/UI/RoM/`（174 张，路径写法 `UI/RoM/<名>`）。常用对应：治疗=Heal/advancedheal/healingcircle、护盾=Shield/technoshield/psionicbarrier、圣光=rayoflight/lightlance/lightburst/sol、驱散=purify、复活=resurrection、狙击=headshot、陷阱=trap、召唤动物=spiritwolf/summonspiritanimals/spiritcall、炮塔=TechnoTurret/defense_pylon、火=firebolt/fireball4/firestorm/meteorite、冰=Icebolt/blizzard/encase/Snowball、奥=arcanebolt/blink/teleport/acceleratetime、隐身=invisibility、嘲讽=taunt。完整清单见各分册技能卡。

### 贴图

- 复用现有 Mote 贴图（`1.7/Textures/Things/Mote/`）：`MMT_SkillBeam(+_Head/_Tail)`（白图代码染色）、`MMT_AuraDisc`、`MMT_FlamePillar`、`MMT_Meteor`、`MMT_IceBlock`、`MMT_SummonOrb`、`MMT_HomingBolt`、`MMT_BlinkFlash`、`MMT_DashTrail`、`MMT_BuffSpark`、`MMT_SkillMine`、`MMT_TargetCircle`、`MMT_TargetRing`；
- 新贴图：占位图先行（Pillow），AI 终稿覆盖同名文件，提示词集中在 `mmt-new-classes-texture-prompts.md`；
- 技能图标如需新绘，规格 64×64，黑底白图或直接彩色图标（UI 用途可不染色）。

### 命名

- 技能集：`MMT_SkillSet_<Class>`；技能 label 中文，不带"测试·"前缀（与演示集区分）；
- 新 Hediff：`MMT_Buff_<名>`（增益）/`MMT_Debuff_<名>`（减益）/`MMT_AuraBuff_<名>`（光环脉冲用）；
- 新投射物：`MMT_Proj_<名>`；新建筑：`MMT_Summon<名>`；新墙：`MMT_<名>Wall`；
- 职业 Hediff：`MMT_Class_<Class>_I/II/III`。

## 8. 新增 Hediff 总表（跨职业汇总，阶段一落地）

| defName | 用途 | 效果（设计值，待实测平衡） | 使用方 |
|---|---|---|---|
| `MMT_Buff_HolyRegen` | 持续恢复 | 每 300 tick 恢复 2 点伤（heal 池机制外的缓回） | 牧师·恢复祷言（groundZone 挂） |
| `MMT_Buff_Soothed` | 安抚 | 疼痛 ×0.5、情绪 +8，30s | 牧师·心灵安抚 |
| `MMT_Buff_Valor` | 勇气 | 射击/近战命中 +4 级，20s | 牧师·勇气祝福 |
| `MMT_AuraBuff_Faith` | 信仰光环 | 疼痛 ×0.8、移动 +5%，脉冲维持 | 牧师·信仰守护 |
| `MMT_Buff_Devotion` | 虔诚 | 承伤 ×0.7，12s | 圣骑士·虔诚护盾 |
| `MMT_Buff_Consecrated` | 圣土 | 承伤 ×0.75，站内维持 | 圣骑士·神圣之地（groundZone 挂） |
| `MMT_Buff_Protection` | 保护 | 承伤 ×0.5，8s | 圣骑士·保护祝福 |
| `MMT_Buff_Might` | 力量 | 近战/射击伤害 ×1.15，20s | 圣骑士·力量祝福 |
| `MMT_Buff_AvengingWrath` | 复仇之怒 | 全伤害 ×1.3、移动 +10%，15s | 圣骑士·复仇之怒 |
| `MMT_Buff_KillCommand` | 杀戮命令 | 召唤物攻击冷却 ×0.7，10s | 猎人·杀戮命令 |
| `MMT_Buff_SniperStance` | 狙击姿态 | 射程 +20%、命中 +6 级、移动 ×0.5，开关式 | 猎人·狙击姿态 |
| `MMT_Debuff_Slow`（已有） | 迟缓 | 移动 ×0.7 | 法师·时空减速等 |
| `MMT_Debuff_Chill` | 寒冷 | 移动 ×0.6、操作 −10%，站内/命中维持 | 法师·暴风雪/寒冰箭 |
| `MMT_Buff_Overclock` | 超频 | 射击冷却 ×0.75，15s | 工程师·战斗改装 |
| `MMT_AuraBuff_MechOverdrive` | 机械超载 | 友方射速/工作速度 +10%，脉冲维持 | 工程师·机械超载 |
| `MMT_Buff_IceBarrier` | 寒冰屏障 | 承伤 ×0.2、移动 0（自我定身），6s | 法师·寒冰屏障 |

> 全部 Hediff 为 `isBad` 按增益/减益区分，显示在健康页；数值为设计锚点，阶段一实测后调整。

## 9. 数值基准

以演示集与 RoM 技能档为锚：单发投射物直伤 10~30（大火球 30）、天降打击 10~40（陨石 40）、跳伤类每跳 2~6、治疗池每目标 10~25、护盾 150~400 点、光环 12~20s、召唤物 20~40s、陷阱 120s。所有数值标「待实测平衡」，阶段一手动测试（文档另立 `mmt-new-classes-test-guide.md`，阶段一实现时补）。

## 10. 验证

- 阶段一落地时：`tools/validate/` 新增 `newclasses.py` 套件 + `dotnet build` + 手动测试指南；
- 本次纯文档交付：技能卡 demoType/字段名已对照 `MMTSkillDemoEntry`，贴图/音效/图标路径已对照实际文件，非框架技能均有 ⚠ 标记。
