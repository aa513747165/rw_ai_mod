# MMT 新职业 · 猎人（Hunter）技能设计

> 手工维护设计文档。总纲见 `docs/mmt-new-classes-design.md`。
> 职业基调色：自然绿棕 `(0.55, 0.7, 0.35)`；读条色 `(0.55, 0.7, 0.35, 0.5)`。数值均为设计锚点，待实测平衡。

## 1. 职业定位

远程物理输出：超远距离狙击点杀高价值目标，陷阱封锁走位，召唤野兽协同作战。无甲脆皮，依赖射程与宠物拉扯。

## 2. 技能总览（15 个）

| # | 技能 | 框架 demoType | 类型 | 阶位 | 阶段二消耗档 | 缺口 |
|---|---|---|---|---|---|---|
| 1 | 精准射击 | singleShot | 伤害·单体 | I | 小 | — |
| 2 | 召唤狼 | summonAnimal | 召唤·野兽 | I | 中 | — |
| 3 | 爆炸陷阱 | mine | 陷阱·伤害 | I | 中 | — |
| 4 | 后跃 | jump | 位移·脱战 | I | 小 | — |
| 5 | 多重射击 | funnelShot | 伤害·群体 | I | 中 | — |
| 6 | 震荡射击 | singleShot | 伤害+减速 | II | 小 | ⚠ 投射物命中挂 Hediff（§6-B） |
| 7 | 冰霜陷阱 | mine | 陷阱·控制 | II | 中 | ⚠ mine 触发皮肤（§6-B） |
| 8 | 召唤熊 | summonAnimal | 召唤·坦克野兽 | II | 中 | — |
| 9 | 狙击姿态 | bufffx | 自我强化 | II | 小 | — |
| 10 | 杀戮命令 | bufffx | 召唤物强化 | II | 小 | — |
| 11 | 毒蛇陷阱 | mine | 陷阱·毒伤 | II | 中 | ⚠ mine 触发皮肤（§6-B） |
| 12 | 伪装 | ⚠ 新原型 | 隐身·脱战 | III | 中 | ⚠ backlog §二.9 |
| 13 | 超远狙击 | singleShot | 伤害·超远程点杀 | III | 大 | — |
| 14 | 兽群召唤 | summonAnimal | 召唤·群体 | III | 大 | ⚠ 召唤数量参数（§6-B） |
| 15 | 照明弹 | ⚠ 新原型 | 侦察·开视野 | III | 小 | ⚠ backlog §二.12（低优先，可砍） |

## 3. 技能卡

### 3.1 精准射击（I 阶，小技）

- **定位**：基础射击，稳定单体伤害。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=30，castSeconds=1，projectileDefName=`MMT_Proj_SkillArrow`（新投射物：damageDef=Bullet，伤害 10，弹速 90，贴图复用 `Things/Projectile/Arrows/Arrow_Steel.png`（武器链现有钢箭），drawSize (0.9,0.9)），soundDefName=`MMT_AirWoosh`。
- **特效详情**：箭矢 0.9 格长条（朝北贴图，飞行朝向自动旋转——投射物机制自带）；速度 90 格/s（快，箭速感）；无缩放。
- **音效**：`MMT_AirWoosh`（离弦风声）。
- **图标**：`UI/RoM/arrowstorm`（无单箭图标，用箭束）。
- **贴图**：复用 `Things/Projectile/Arrows/Arrow_Steel.png`。

### 3.2 召唤狼（I 阶，中技）

- **定位**：召唤森林狼伙伴，撕咬牵制。
- **框架**：`summonAnimal`。radius=0，range=10，castSeconds=1，summonAnimalKindDefName=`TimberWolf`（原版），summonAnimalDurationSeconds=25。
- **特效详情**：召唤落点绿色 BuffSpark 粒子 5 点 + MMT_SummonPaw 爪印闪光（≈1.2 格，0.4s 淡出，框架自带召唤特效）。
- **音效**：`MMT_WolfHowl`（狼嚎，RoM 复刻——本技能绝配）。
- **图标**：`UI/RoM/spiritwolf`。
- **贴图**：复用（狼用原版贴图，召唤特效复用 `MMT_SummonPaw`）。

### 3.3 爆炸陷阱（I 阶，中技）

- **定位**：经典陷阱，封路口。
- **框架**：`mine`。radius=3（=爆炸半径预览），targetFriendly=false，range=15，castSeconds=1，mineArmSeconds=1，mineTriggerRadius=2，mineExplosionRadius=3，mineDamage=30，mineDurationSeconds=120。
- **特效详情**：地面装置贴图 MMT_SkillMine（≈0.6 格，武装后红色微光——框架自带）；触发即爆炸（原版爆炸视觉+火焰，半径 3 格）。
- **音效**：布设 `MMT_FastReleaseSD`；爆炸 `MMT_SoftExplosion`（框架内置爆炸音为准）。
- **图标**：`UI/RoM/trap`。
- **贴图**：复用 `Things/Mote/MMT_SkillMine`。

### 3.4 后跃（I 阶，小技）

- **定位**：向后跳跃脱战，拉开射击距离。
- **框架**：`jump`。radius=0，range=8，castSeconds=0.3，jumpTicksPerCell=2，jumpPeakHeight=2。
- **特效详情**：起跳点尘雾闪光（MMT_BlinkFlash 染绿 `(0.55,0.7,0.35)`——若位移 fleck 染色需小扩展，阶段一接受默认色）；抛物线顶点 2 格；速度 2 tick/格。
- **音效**：`MMT_AirWoosh`。
- **图标**：`UI/RoM/grapplinghook`。
- **贴图**：复用。
- **备注**：瞄准方向=起跳方向的反向选择由玩家选落点实现，框架天然支持。

### 3.5 多重射击（I 阶，中技）

- **定位**：浮游齐射，一片箭雨覆盖数名敌人。
- **框架**：`funnelShot`。radius=0，targetFriendly=false，range=25，castSeconds=2，funnelCount=5，ringRadius=1.8，aimTicks=15，projectileDefName=`MMT_Proj_SkillArrow`（同 3.1），funnelFleckDefName=`MMT_Fleck_ArrowCharge`（⚠ 新 fleck：默认是火球充能 `MMT_Fleck_FireballCharge`，箭雨需换成箭簇形态——新贴图 `MMT_ArrowCharge.png`，提示词 §六；阶段一可先用默认火球充能顶测），soundDefName=`MMT_AirWoosh`。
- **特效详情**：5 枚箭簇绕身环（半径 1.8 格）悬浮瞄准 15 tick 后齐发；单箭 0.9 格、弹速 90；悬浮环旋转（框架自带）。
- **音效**：齐发 `MMT_AirWooshLow`。
- **图标**：`UI/RoM/arrowstorm`。
- **贴图**：需新出 `MMT_ArrowCharge.png`（可后置，默认火球充能先顶）。

### 3.6 震荡射击（II 阶，小技）⚠

- **定位**：减速射击，放风筝核心。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=30，castSeconds=1，projectileDefName=`MMT_Proj_SkillConcussive`（伤害 6，弹速 90，复用箭贴图染蓝——投射物 graphicData color `(140,190,255)`，drawSize (0.9,0.9)）。
- **⚠ 小扩展（§6-B）**：投射物命中附加 Hediff——命中挂 `MMT_Debuff_Slow`（已有，移动 ×0.7，6s）。
- **特效详情**：命中时目标身上蓝色 BuffSpark 爆点 3 点；其余同精准射击。
- **音效**：`MMT_AirWoosh`。
- **图标**：`UI/RoM/disablingshot`。
- **贴图**：复用箭贴图（染色）。

### 3.7 冰霜陷阱（II 阶，中技）⚠

- **定位**：冻住踩中的敌人，强控场。
- **框架**：`mine` + ⚠ **触发皮肤扩展（§6-B）**：触发后不爆炸，改为半径 2.5 格内敌人 `StaggerFor(240)`（4s 冻结）+ 罩冰块（复用 `Thing_MMTIceBlock` Attach 机制，zoneIceScale 同款 1.7 格）。
- **参考参数**：range=15，castSeconds=1，mineArmSeconds=1，mineTriggerRadius=2，mineDurationSeconds=120。
- **特效详情**：装置贴图 MMT_SkillMine 染冰蓝 `(0.6, 0.85, 1)`（若染色需参数，列为扩展；否则新贴图 `MMT_SkillMineIce.png` 提示词 §七）；触发时冰蓝爆闪 2 格 + 冰块罩住被冻目标（淡入/融化淡出，复用寒风领域机制）。
- **音效**：触发 `MMT_FastReleaseSD`。
- **图标**：`UI/RoM/encase`。
- **阶段一取舍**：扩展未做前先做"爆炸陷阱·冰皮版"（低伤害 10 + 爆炸视觉），控制版等扩展。

### 3.8 召唤熊（II 阶，中技）

- **定位**：召唤灰熊，前排肉盾。
- **框架**：`summonAnimal`。range=10，castSeconds=1.5，summonAnimalKindDefName=`GrizzlyBear`（原版），summonAnimalDurationSeconds=30。
- **特效详情**：同 3.2，爪印闪光更大（≈1.5 格）。
- **音效**：`MMT_WolfHowl`（无熊吼素材，阶段一复用；如需熊吼标"需新增音效"）。
- **图标**：`UI/RoM/summonspiritanimals`。
- **贴图**：复用。

### 3.9 狙击姿态（II 阶，小技）

- **定位**：架枪强化，射程命中提升但钉在原地。
- **框架**：`bufffx`。自身（includeSelf=true、buffRequireTarget=false），castSeconds=0.8，buffHediffDefName=`MMT_Buff_SniperStance`，buffIconTexPath=`UI/RoM/stayalert`，buffIconTint=`(0.55, 0.7, 0.35)`，buffIconSeconds=2。
- **特效详情**：头顶绿色准星图标 2s；身上绿粒子。
- **音效**：`MMT_Pulse`。
- **图标**：`UI/RoM/stayalert`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_SniperStance`（射程 +20%、命中 +6 级、移动 ×0.5；开关式——再点一次解除，阶段二做切换逻辑，阶段一为固定 30s）。

### 3.10 杀戮命令（II 阶，小技）

- **定位**：指挥宠物猛攻。
- **框架**：`bufffx`。radius=8（大圈罩住自家召唤物），targetFriendly=true，range=25，castSeconds=0.5，buffHediffDefName=`MMT_Buff_KillCommand`，buffIconTexPath=`UI/RoM/enrage`，buffIconTint=`(0.9, 0.4, 0.2)`，buffIconSeconds=2。
- **特效详情**：圈内友方（主要是召唤兽）头顶红色獠牙图标 2s + 红粒子。
- **音效**：`MMT_WolfHowl`。
- **图标**：`UI/RoM/enrage`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_KillCommand`（攻击冷却 ×0.7，10s）。

### 3.11 毒蛇陷阱（II 阶，中技）⚠

- **定位**：毒伤陷阱，踩中持续掉血。
- **框架**：`mine` + ⚠ **触发皮肤扩展（§6-B，同 3.7 一次落地）**：触发后半径 2 格内敌人累积原版 `ToxicBuildup`（每跳 0.08，共 5 跳/间隔 30 tick）。
- **特效详情**：装置染毒绿 `(0.5, 0.85, 0.3)`；触发时绿色毒雾爆闪 2 格（复用毒气池 Tint 粒子）。
- **音效**：触发 `MMT_FastReleaseSD`。
- **图标**：`UI/RoM/poison`。
- **贴图**：同 3.7（染色或 `MMT_SkillMinePoison.png`）。

### 3.12 伪装（III 阶，中技）⚠

- **定位**：隐身脱离战斗，重新站位。
- **框架**：⚠ **需新框架原型**（backlog §二.9 隐身：hediff 携带透明度覆写 + `AttackTargetFinder` 排除 + 攻击/受伤解除钩子）。
- **参考参数**：持续 10s，移动速度 ×0.8，主动攻击或受击解除；castSeconds=0.5。
- **特效详情**：施法者透明度降到 30%（渲染 patch）；脚下淡绿烟霭粒子（BuffSpark 染绿灰，每 20 tick 1 点）。
- **音效**：`MMT_AirWooshLow`。
- **图标**：`UI/RoM/invisibility`。
- **阶段一取舍**：暂缓；临时替代=后跃+烟雾（`smokecloud` 图标技，走 bufffx 挂命中率 Hediff）——文档保留隐身设计，替代技不单列。

### 3.13 超远狙击（III 阶，大招）

- **定位**：标志性大招——贯穿全屏的一枪。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=45（=框架瞄准环上限 MaxRangeRingRadius），castSeconds=3，projectileDefName=`MMT_Proj_SkillSnipe`（新投射物：damageDef=Bullet，伤害 45、护甲穿透 0.35，弹速 120，贴图 `Things/Projectile/MMT_SnipeTracer.png` 新出——细长曳光弹，提示词 §八，drawSize (1.4,0.5)，color `(255,240,200)`），soundDefName=`MMT_ShotM82`（巴雷特枪声，MMT 武器音效）。
- **特效详情**：曳光弹细长 1.4×0.5 格，弹速 120 格/s（几乎瞬达的视觉）；读条 3s（读条色绿棕）；命中爆点（原版弹着尘土）。
- **音效**：`MMT_ShotM82`。
- **图标**：`UI/RoM/headshot`。
- **贴图**：需新出 `MMT_SnipeTracer.png`。

### 3.14 兽群召唤（III 阶，大招）⚠

- **定位**：一次召一群狼淹没敌人。
- **框架**：`summonAnimal` + ⚠ **数量参数扩展（§6-B）**：一次 3 只 `TimberWolf`，落点围绕目标格散开。扩展未做前=连点 3 次召唤狼（阶段一接受）。
- **参考参数**：range=10，castSeconds=2，summonAnimalDurationSeconds=25。
- **特效详情**：三连爪印闪光（每只一枚，间隔 10 tick）+ 绿粒子。
- **音效**：`MMT_WolfHowl` ×1（齐嚎）。
- **图标**：`UI/RoM/poppihorde`。
- **贴图**：复用。

### 3.15 照明弹（III 阶，小技）⚠ 低优先

- **定位**：侦察向——照亮战争迷雾区域。
- **框架**：⚠ **需新框架原型**（backlog §二.12：投射物落地生成限时照明建筑 + FogGrid 强制除雾定时恢复）。
- **参考参数**：range=45，castSeconds=1，持续 15s，照明半径 8 格。
- **特效详情**：投射物抛物线（复用大火球轨迹样式）+ 落点暖黄光球（GlowGrid 光源，直径 16 格渐暗）。
- **音效**：`MMT_ShotMilkorMGL`（榴弹发射器声）。
- **图标**：`UI/RoM/magelight`。
- **阶段一取舍**：**建议砍掉或最后做**——战斗价值低，与深空扫描玩法联动时再做。砍后由「冰霜陷阱」升档补位或职业保持 14 技能。

## 4. 天赋树节点草案（阶段二）

```
Start（0 费，就职自动解锁）
├─ 职业印记「荒野猎手」（1 点）：射击精度 +2 级（小被动）
├─ I 阶链：精准射击(1) → 召唤狼(2) → 爆炸陷阱(2) → 后跃(1) → 多重射击(2)
├─ 强化 I「鹰眼」（2 点，需 II 阶）：射程 +10%、熵上限 +15
├─ II 阶链：震荡射击(1) → 冰霜陷阱(2) → 召唤熊(2) → 狙击姿态(2) → 杀戮命令(2) → 毒蛇陷阱(2)
├─ 强化 II「兽王血脉」（3 点，需 III 阶）：召唤物存在时间 +50%
├─ III 阶链：伪装(3) → 超远狙击(3) → 兽群召唤(4) → 照明弹(1)
└─ Keystone「神射手」（3 点）：射击伤害 +15%，陷阱触发半径 +0.5
```

## 5. 本职业新增资产清单

| 资产 | 名称 | 说明 |
|---|---|---|
| 投射物 | `MMT_Proj_SkillArrow` / `MMT_Proj_SkillConcussive` | 箭矢（优先复用 Arrows 贴图）与蓝染震荡箭 |
| 投射物+贴图 | `MMT_Proj_SkillSnipe` / `MMT_SnipeTracer.png` | 超远狙击曳光弹（提示词 §八） |
| fleck 贴图 | `MMT_ArrowCharge.png` | 多重射击浮游箭簇（提示词 §六，可先默认顶测） |
| 贴图 | `MMT_SkillMineIce.png` / `MMT_SkillMinePoison.png` | 冰霜/毒蛇陷阱皮肤（提示词 §七；若框架支持装置染色则不出图） |
| Hediff | `MMT_Buff_KillCommand` / `MMT_Buff_SniperStance` | 见总纲 §8 |
