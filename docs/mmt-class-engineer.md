# MMT 新职业 · 工程师（Engineer）技能设计

> 手工维护设计文档。总纲见 `docs/mmt-new-classes-design.md`。
> 职业基调色：工业橙 `(1, 0.6, 0.2)`；读条色 `(1, 0.6, 0.2, 0.5)`。数值均为设计锚点，待实测平衡。

## 1. 职业定位

科技版"机械师"：远程火力（榴弹/射线）+ 部署体系（炮塔/机器人/地雷/屏障）打阵地战。本体输出平庸，强度在召唤物与工事；机动差，怕近身（用火箭跳与护盾弥补）。

## 2. 技能总览（15 个）

| # | 技能 | 框架 demoType | 类型 | 阶位 | 阶段二消耗档 | 缺口 |
|---|---|---|---|---|---|---|
| 1 | 召唤机枪炮塔 | summonTurret | 召唤·炮塔 | I | 中 | —（现有 `MMT_SummonTurret`） |
| 2 | 榴弹轰击 | singleShot | 伤害·范围 | I | 小 | — |
| 3 | 部署地雷 | mine | 陷阱·伤害 | I | 小 | — |
| 4 | 战斗改装 | bufffx | 自我强化 | I | 小 | — |
| 5 | 磁暴雷球 | summonOrb | 召唤·EMP | I | 中 | — |
| 6 | 能量护盾发生器 | aura(护盾) | 护盾·范围 | II | 中 | — |
| 7 | 过载射线 | beam | 伤害·持续射线 | II | 中 | ⚠ beam 伤害类型参数（§6-B，可先用 Burn） |
| 8 | 部署护卫机器人 | summonAnimal | 召唤·机器人 | II | 中 | ⚠ 新 race 资产（§6-C，不可用机械体 PawnKind） |
| 9 | 火箭跳跃 | jump | 位移 | II | 小 | — |
| 10 | 能量屏障 | wall | 工事·掩体 | II | 中 | ⚠ 墙体 Def 参数（§6-B） |
| 11 | 维修脉冲 | heal | 治疗·友方 | II | 小 | ⚠ 不修建筑/炮塔（框架限制，接受） |
| 12 | 召唤火箭炮塔 | summonTurret | 召唤·重炮塔 | III | 大 | 新建筑+贴图 |
| 13 | 自爆机器人 | summonHoming | 召唤·自杀袭击 | III | 中 | 新贴图（换皮即可） |
| 14 | 轨道炮击 | strike | 伤害·范围连发 | III | 大 | ⚠ 落雷皮肤（可接受） |
| 15 | 机械超载 | aura(增益) | 光环·团队大招 | III | 大 | — |

## 3. 技能卡

### 3.1 召唤机枪炮塔（I 阶，中技）

- **定位**：核心部署技，临时火力点。
- **框架**：`summonTurret`。radius=0，range=12，castSeconds=2，summonTurretDefName=`MMT_SummonTurret`（现有，机枪型），summonTurretDurationSeconds=40。
- **特效详情**：部署点橙色建造闪光（BuffSpark 染橙，≈1 格）+ 炮塔落地（底座+顶部贴图已有 `Things/Building/MMT_SummonTurret_Base/_Top`）；到期自毁冒烟（框架自带）。
- **音效**：`MMT_FastReleaseSD`（部署）+ 炮塔射击音（炮塔 Def 自带）。
- **图标**：`UI/RoM/TechnoTurret`。
- **贴图**：复用现有炮塔贴图。

### 3.2 榴弹轰击（I 阶，小技）

- **定位**：基础 AoE 输出。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=20，castSeconds=1.2，projectileDefName=`MMT_Proj_SkillGrenade`（新投射物：thingClass=Projectile_Explosive，damageDef=Bomb，伤害 18、爆炸半径 1.5，弹速 45（榴弹慢、有抛物感），贴图复用 `Things/Projectile/ChargedFragGrenade.png`（武器链已有），drawSize (0.8,0.8)），soundDefName=`MMT_RF_ShotMilkorMGL`（榴弹发射器声，MMT 武器音效）。
- **特效详情**：榴弹 0.8 格；弹速 45 格/s（明显慢于子弹，可目视躲弹感）；不旋转；落点小爆炸（半径 1.5）。
- **音效**：`MMT_RF_ShotMilkorMGL` + 爆炸音（投射物自带）。
- **图标**：`UI/RoM/mk203gl`。
- **贴图**：复用 `Things/Projectile/ChargedFragGrenade.png`。

### 3.3 部署地雷（I 阶，小技）

- **定位**：工程版地雷（与猎人爆炸陷阱同框架，参数更"工事"）。
- **框架**：`mine`。radius=3，targetFriendly=false，range=12，castSeconds=1，mineArmSeconds=1，mineTriggerRadius=2，mineExplosionRadius=3，mineDamage=35，mineDurationSeconds=180。
- **特效详情**：装置 MMT_SkillMine（≈0.6 格，武装后红微光）；触发爆炸半径 3 格。
- **音效**：布设 `MMT_FastReleaseSD`。
- **图标**：`UI/RoM/trap`。
- **贴图**：复用。

### 3.4 战斗改装（I 阶，小技）

- **定位**：自我超频，射速提升。
- **框架**：`bufffx`。自身，castSeconds=0.8，buffHediffDefName=`MMT_Buff_Overclock`，buffIconTexPath=`UI/RoM/overdrive`，buffIconTint=`(1, 0.6, 0.2)`，buffIconSeconds=2。
- **特效详情**：头顶橙色齿轮图标（overdrive 图标）2s + 橙色电弧粒子（BuffSpark 染橙）。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/overdrive`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_Overclock`（射击冷却 ×0.75，15s）。

### 3.5 磁暴雷球（I 阶，中技）

- **定位**：EMP 雷球，克制机械体。
- **框架**：`summonOrb`。radius=0，targetFriendly=false，range=15，castSeconds=1.5，summonOrbTexPath=`Things/Mote/MMT_SummonOrb`，summonOrbTint=`(0.55, 0.85, 1)`，summonOrbTicksPerCell=15，summonDamage=10，summonDamageDefName=`EMP`，summonPulseRadius=2.5，summonPulseIntervalTicks=20。
- **特效详情**：电蓝 `(0.55, 0.85, 1)` 雷球（直径 ≈1 格，白图染色），漂移速度 15 tick/格；每 20 tick 一圈 EMP 电弧脉冲（半径 2.5，框架自带脉冲闪光）；不旋转。
- **音效**：`MMT_Vibration`（电流嗡鸣）。
- **图标**：`UI/RoM/psiblast`。
- **贴图**：复用 `Things/Mote/MMT_SummonOrb`（染色）。
- **效果**：EMP 伤害——对机械体眩晕、对护盾放电（原版 EMP 机制）。

### 3.6 能量护盾发生器（II 阶，中技）

- **定位**：原地展开护盾罩，保护炮塔阵地。
- **框架**：`aura`（auraShieldMode=true）。radius=3，castSeconds=2，auraDurationSeconds=15，auraShieldHitPoints=250，auraTint=`(0.5, 0.8, 1)`。
- **特效详情**：原版护盾半球直径 6 格，科技蓝 tint；受击涟漪；生成弹开。
- **音效**：`MMT_VibrationLow`。
- **图标**：`UI/RoM/technoshield`。
- **贴图**：复用。

### 3.7 过载射线（II 阶，中技）

- **定位**：引导型主炮，持续烧穿一条线。
- **框架**：`beam`。radius=0，targetFriendly=false，showRangeRing=false，castSeconds=2，soundDefName=`MMT_LaserBeam`（MMT 武器音效），beamDurationSeconds=3，beamRange=20，beamWidth=0.8，beamTint=`(1, 0.55, 0.2)`，beamMuzzleOffset=0.9，beamHeadTexPath=`Things/Mote/MMT_SkillBeam_Head`，beamTailTexPath=`Things/Mote/MMT_SkillBeam_Tail`，beamHeadSize=1.5，beamTailSize=1.8，beamDamage=6，beamDamageIntervalTicks=10。
- **特效详情**：光柱宽 0.8 格（比牧师圣光 0.5 粗，重工业感）；橙色 `(1, 0.55, 0.2)`；发射头光球 1.5 格；命中端爆点 1.8 格脉冲；宽度呼吸抖动（框架自带）。
- **⚠ 小扩展（§6-B）**：beam 伤害类型固定 Burn，想过载"电击"需加 damageDef 参数；阶段一接受 Burn（过载=烧蚀说得通）。
- **音效**：`MMT_LaserBeam`。
- **图标**：`UI/RoM/refraction`。
- **贴图**：复用（染色）。

### 3.8 部署护卫机器人（II 阶，中技）⚠资产

- **定位**：召唤一台跟随作战的护卫机器人。
- **框架**：`summonAnimal`（阵营=施法者派系，框架支持）。castSeconds=2，range=10，summonAnimalKindDefName=`MMT_GuardBot`（**新 race+PawnKind，见总纲 §6-C**：仿 `MMT_Races_FireElemental` 做普通 race——**禁止用原版机械体 PawnKind**（无机械师 overseeing 会失效）；造型小型浮游炮机，近战电击+小护盾），summonAnimalDurationSeconds=40。
- **特效详情**：部署点橙色闪光 + 机器人本体（身高 ≈0.8 格，悬浮）。
- **音效**：`MMT_Vibration`。
- **图标**：`UI/RoM/minion_south`。
- **贴图**：需新出机器人 race 身体贴图（提示词 §九）。
- **阶段一取舍**：可先用 `Husky` 顶测流程（视觉不符，仅验证机制）。

### 3.9 火箭跳跃（II 阶，小技）

- **定位**：火箭推进跳走，位移+入场/脱战两用。
- **框架**：`jump`。range=12，castSeconds=0.5，jumpTicksPerCell=2.5，jumpPeakHeight=3.5。
- **特效详情**：起跳点橙色爆炸闪光（BuffSpark 染橙 1.5 格）+ 尾焰拖尾（DashTrail，每 3 tick 一枚）；顶点 3.5 格（比普通跳跃 3 略高，火箭感）。
- **音效**：`MMT_ShotRPG`（火箭发射声）。
- **图标**：`UI/RoM/phased`。
- **贴图**：复用。

### 3.10 能量屏障（II 阶，中技）⚠

- **定位**：一排临时能量墙，挡子弹封走位。
- **框架**：`wall` + ⚠ **墙体 Def 参数（§6-B）**：现框架墙体固定 `MMT_StoneWall`；扩展后本技能用新建筑 `MMT_EnergyBarrierWall`（半透明蓝色能量墙，耐久 150，30s 自毁，可射击穿透友方——穿透属性若做不了就做成普通实体墙）。
- **参考参数**：radius=2.5，range=15，castSeconds=1.5，wallDurationSeconds=30。
- **特效详情**：墙体落地橙色建造闪光；能量墙本体半透明蓝 `(0.5, 0.8, 1, 0.5)`，高 1 格贴图；到期闪烁 3 次消失（需墙 comp 支持，列为扩展附带项）。
- **音效**：`ChunkRock_Drop`（原版，落墙声）——扩展后用 `MMT_VibrationLow`。
- **图标**：`UI/RoM/defense_pylon`。
- **贴图**：需新出 `MMT_EnergyBarrierWall`（提示词 §十）。
- **阶段一取舍**：扩展未做前先放石墙（视觉上违和但机制一致）。

### 3.11 维修脉冲（II 阶，小技）

- **定位**：纳米修复波，治疗周围友方（炮塔的"修理"概念套在人身上）。
- **框架**：`heal`。radius=3，targetFriendly=true，range=15，castSeconds=1.5，healAmount=10。
- **特效详情**：绿十字+治疗粒子（框架默认；若可染色改科技青 `(0.4, 0.9, 0.8)`——同牧师 §3.1 的 heal 粒子染色扩展项，阶段一接受默认绿）。
- **⚠ 框架限制**：heal 只修 pawn，不修建筑/召唤炮塔——接受（炮塔本就限时）。
- **音效**：`MMT_HealTurret_ClickSoft`（MMT 医疗音效）。
- **图标**：`UI/RoM/nano_stim`。
- **贴图**：复用。

### 3.12 召唤火箭炮塔（III 阶，大招）

- **定位**：重型部署，范围轰炸火力点。
- **框架**：`summonTurret`。range=12，castSeconds=3，summonTurretDefName=`MMT_SummonTurretRocket`（**新建筑**：火箭炮，射速慢、单发爆炸半径 2、伤害 25，射程 30），summonTurretDurationSeconds=45。
- **特效详情**：部署闪光更大（2 格橙色）；炮塔贴图比机枪版粗壮（需新出 `MMT_SummonTurretRocket_Base/_Top.png`，提示词 §十一）；发射时炮口火光+后坐（炮塔建筑自带 verb 视觉）。
- **音效**：部署 `MMT_FastReleaseSD`；发射 `MMT_130mmRocket`。
- **图标**：`UI/RoM/defense_pylon`。
- **贴图**：需新出火箭炮塔 base/top 两张。

### 3.13 自爆机器人（III 阶，中技）

- **定位**：放出小型自爆机器人追踪敌人自爆。
- **框架**：`summonHoming`（追踪弹框架换皮）。radius=0，targetFriendly=false，range=30，castSeconds=1.5，summonHomingTexPath=`Things/Mote/MMT_HomingBot`（**新贴图**：小型四足/轮式自爆机器人顶视图，朝北，提示词 §十二），summonHomingTint=`(1, 0.7, 0.3)`，summonHomingTicksPerCell=6（6 tick/格，比追踪弹 5 略慢——地面爬行感），summonDamage=30，summonHomingExplosionRadius=2，summonHomingDurationSeconds=10。
- **特效详情**：机器人尺寸 ≈0.9 格，橙色涂装；爬行速度 6 tick/格；命中/超时爆炸（半径 2，30 伤害）；追踪转向（框架自带）。
- **音效**：放出 `MMT_Vibration`；爆炸框架内置。
- **图标**：`UI/RoM/sabotage`。
- **贴图**：需新出 `MMT_HomingBot.png`。

### 3.14 轨道炮击（III 阶，大招）

- **定位**：呼叫轨道火力覆盖一片区域。
- **框架**：`strike`（连发模式）。radius=4，targetFriendly=false，range=30，castSeconds=3，strikeCount=6，strikeIntervalTicks=15，strikeAreaRadius=4，strikeInitialDelayTicks=30，strikeDamage=18，strikeExplosionRadius=1.9，strikeArmorPenetration=0.3。
- **特效详情**：⚠ 视觉为原版落雷（蓝白）——包装为"轨道电离炮"说得通（电离光束从天而降），接受；如做 §6-B strike 皮肤参数可换红热轨道柱。6 连发、间隔 15 tick、首发延迟 30 tick（预警感）。
- **音效**：`MMT_RailgunShot`（每发）。
- **图标**：`UI/RoM/orbitalstrike`。
- **贴图**：复用。

### 3.15 机械超载（III 阶，大招）

- **定位**：团队光环大招，全场友方+召唤物超载运转。
- **框架**：`aura`（增益模式）。radius=5，targetFriendly=true，castSeconds=2.5，auraDurationSeconds=15，auraBuffDefName=`MMT_AuraBuff_MechOverdrive`，auraRefreshIntervalTicks=60，auraTint=`(1, 0.6, 0.2)`。
- **特效详情**：脚下橙色 AuraDisc 光圈（直径 10 格）跟随，三层缓旋；光环内友方身上橙色电弧粒子（aura 脉冲自带挂 Hediff 的闪光）。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/commander`。
- **贴图**：复用。
- **效果**：脉冲挂 `MMT_AuraBuff_MechOverdrive`（射速/工作速度 +10%）。

## 4. 天赋树节点草案（阶段二）

```
Start（0 费，就职自动解锁）
├─ 职业印记「战场工坊」（1 点）：建造速度 +10%（小被动）
├─ I 阶链：召唤机枪炮塔(2) → 榴弹轰击(1) → 部署地雷(1) → 战斗改装(1) → 磁暴雷球(2)
├─ 强化 I「改良图纸」（2 点，需 II 阶）：召唤炮塔存在时间 +30%
├─ II 阶链：能量护盾发生器(2) → 过载射线(2) → 部署护卫机器人(2) → 火箭跳跃(1) → 能量屏障(2) → 维修脉冲(1)
├─ 强化 II「核动力核心」（3 点，需 III 阶）：熵上限 +20、熵恢复 +15%
├─ III 阶链：召唤火箭炮塔(4) → 自爆机器人(3) → 轨道炮击(4) → 机械超载(4)
└─ Keystone「总工程师」（3 点）：同时存在的召唤炮塔上限 +1（需配套机制，否则改为召唤物伤害 +20%）
```

## 5. 本职业新增资产清单

| 资产 | 名称 | 说明 |
|---|---|---|
| 投射物 | `MMT_Proj_SkillGrenade` | 榴弹（复用 ChargedFragGrenade.png） |
| 建筑+贴图 | `MMT_SummonTurretRocket`（base/top 两张） | 火箭炮塔（提示词 §十一） |
| 建筑+贴图 | `MMT_EnergyBarrierWall` | 能量屏障墙（配 §6-B 墙体参数，提示词 §十） |
| race+PawnKind+贴图 | `MMT_GuardBot` | 护卫机器人（提示词 §九；普通 race，非机械体） |
| 贴图 | `MMT_HomingBot.png` | 自爆机器人（提示词 §十二） |
| Hediff | `MMT_Buff_Overclock` / `MMT_AuraBuff_MechOverdrive` | 见总纲 §8 |
