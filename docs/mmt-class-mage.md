# MMT 新职业 · 法师（Mage）技能设计

> 手工维护设计文档。总纲见 `docs/mmt-new-classes-design.md`。
> 三系基调色：火 `(1, 0.45, 0.15)` / 冰 `(0.6, 0.85, 1)` / 奥 `(0.7, 0.4, 1)`；读条色同系色 + alpha 0.5。数值均为设计锚点，待实测平衡。

## 1. 职业定位

远程法系输出：**火系**主打直接伤害（单体→范围→大招阶梯）；**冰系**主打控制（减速/冻结/冰墙封路）兼中等伤害；**奥系**主打辅助与机动（折跃/减速/造物）兼浮游齐射输出。身板最脆，输出上限最高。

## 2. 技能总览（15 个：火 5 / 冰 5 / 奥 5）

| # | 技能 | 系 | 框架 demoType | 类型 | 阶位 | 阶段二消耗档 | 缺口 |
|---|---|---|---|---|---|---|---|
| 1 | 火球术 | 火 | singleShot | 伤害·单体 | I | 小 | —（现有 `MMT_Proj_SkillFireball`） |
| 2 | 寒冰箭 | 冰 | singleShot | 伤害+减速 | I | 小 | ⚠ 投射物命中挂 Hediff（§6-B）+ 新投射物 |
| 3 | 奥术飞弹 | 奥 | funnelShot | 伤害·浮游齐射 | I | 中 | 新 fleck 贴图（可先默认顶测） |
| 4 | 折跃 | 奥 | blink | 位移 | I | 小 | — |
| 5 | 龙息术 | 火 | breathCone | 伤害·锥形 | I | 中 | — |
| 6 | 烈焰风暴 | 火 | groundFire | 伤害·火海 | II | 中 | — |
| 7 | 暴风雪 | 冰 | groundZone | 伤害+减速·区域 | II | 中 | — |
| 8 | 冰墙术 | 冰 | wall | 工事·封路 | II | 中 | ⚠ 墙体 Def 参数（§6-B，可先石墙顶测） |
| 9 | 时空减速 | 奥 | bufffx | 减益·群体 | II | 中 | —（Hediff `MMT_Debuff_Slow` 已有） |
| 10 | 寒冰屏障 | 冰 | bufffx | 自保·冰箱 | II | 中 | ⚠ bufffx 覆盖物（§6-B，可选） |
| 11 | 炎爆术 | 火 | singleShot | 伤害·大火球跨墙 | III | 大 | —（现有 `MMT_Proj_BigFireball`） |
| 12 | 冰霜新星 | 冰 | ⚠ 新原型 | 控制·自身周围 | III | 大 | ⚠ backlog §一.1 新星环 |
| 13 | 陨石术 | 火 | delayedStrike | 伤害·范围大招 | III | 大 | — |
| 14 | 奥术回响 | 奥 | summonOrb | 伤害·浮游雷球 | III | 中 | — |
| 15 | 魔法造物 | 奥 | ⚠ 非 SkillFX | 辅助·生成食物 | III | 中 | ⚠ 走 `ClassAbilityEffects.cs` SpawnEffect 桶 |

## 3. 技能卡

### 火系

#### 3.1 火球术（I 阶，小技）

- **定位**：火系基础填充技。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=25，castSeconds=1.5，projectileDefName=`MMT_Proj_SkillFireball`（现有：Flame 伤害、drawSize 1.6、橙红 `(255,140,40)`），soundDefName=`Shot_IncendiaryLauncher`。
- **特效详情**：火球直径 1.6 格；朝北贴图飞行自旋朝向；大小恒定；命中小爆炸（投射物自带）。
- **音效**：`Shot_IncendiaryLauncher`（原版）。
- **图标**：`UI/RoM/firebolt`。
- **贴图**：复用（投射物用原版 LauncherShot 贴图放大，已有）。

#### 3.2 龙息术（I 阶，中技）

- **定位**：锥形喷吐，近身清群。
- **框架**：`breathCone`。radius=0，targetFriendly=false，range=12，showRangeRing=false，castSeconds=1，breathRange=8，breathHalfAngleDegrees=30，breathDurationSeconds=3，breathDamage=3，breathDamageIntervalTicks=10，breathIgniteChance=0.2，soundDefName=`FireSpew_Resolve`。
- **特效详情**：锥形长 8 格、半角 30°；火焰 mote 从喷口向锥面扩散（框架自带：粒子初速快、尾速衰减，颜色橙红）；持续 3s 期间可缓慢转方向（框架自带跟随）。
- **音效**：`FireSpew_Resolve`（原版）。
- **图标**：`UI/RoM/innerfire`。
- **贴图**：复用（火焰粒子框架内置）。

#### 3.3 烈焰风暴（II 阶，中技）

- **定位**：火海覆盖，阵地压制。
- **框架**：`groundFire`（火海模式）。radius=5，targetFriendly=false，range=25，castSeconds=3，soundDefName=`Explosion_Flame`，groundDurationSeconds=5，groundVisualScale=1.6，groundDamage=2，groundDamageIntervalTicks=15，groundDensity=0.85，groundIgnitePawnChance=0.15，groundIgniteCellChance=0.03，groundTint=`(1, 0.55, 0.15)`。
- **特效详情**：火盘 1.6 格/簇、覆盖率 0.85 铺满 5 格半径；火柱堆叠参数默认（高 1.4、7 层、顶端 0.4、摆动 0.35）；橙红染色。
- **音效**：`Explosion_Flame`（原版）。
- **图标**：`UI/RoM/firestorm`。
- **贴图**：复用 `Things/Mote/MMT_FlamePillar`。

#### 3.4 炎爆术（III 阶，大招）

- **定位**：高空抛物线大火球，跨墙轰击。
- **框架**：`singleShot`。radius=0，targetFriendly=false，ignoreLineOfSight=true，range=30，castSeconds=3，projectileDefName=`MMT_Proj_BigFireball`（现有：Flame 30 伤害、爆炸，drawSize 1.8），soundDefName=`Shot_IncendiaryLauncher`。
- **特效详情**：大火球 1.8 格高空抛物线（flyOverhead，跨墙）；落点爆炸+火焰。
- **音效**：`Shot_IncendiaryLauncher` + 爆炸音（投射物自带）。
- **图标**：`UI/RoM/fireball4`。
- **贴图**：复用 `Things/Projectile/MMT_BigFireball.png`。

#### 3.5 陨石术（III 阶，大招）

- **定位**：读条+预警+天降陨石，火系终极。
- **框架**：`delayedStrike`。radius=3，targetFriendly=false，range=25，castSeconds=2，meteorDelaySeconds=2，meteorFallTicks=25，meteorDamage=40，meteorExplosionRadius=3，meteorIgniteChance=0.3，meteorDrawSize=4，meteorSpinDegPerTick=0，meteorGrowFrom=1，meteorGrowTo=1。
- **特效详情**：预警圈 3 格（2s）→ 陨石直径 4 格（=爆炸半径 3 × 1.33）坠落 25 tick；不旋转；大小恒定；落点爆炸+30% 点燃。
- **音效**：`MMT_FireWooshSD`（坠落）+ 爆炸音。
- **图标**：`UI/RoM/meteorite`。
- **贴图**：复用 `Things/Mote/MMT_Meteor`。

### 冰系

#### 3.6 寒冰箭（I 阶，小技）⚠

- **定位**：冰系基础技，伤害+减速。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=25，castSeconds=1.5，projectileDefName=`MMT_Proj_SkillIcebolt`（**新投射物**：damageDef=Frost，伤害 10，弹速 65，贴图 `Things/Projectile/MMT_Icebolt.png` 新出——冰蓝锥形冰晶，提示词 §十三，drawSize (1.1,1.1)，color `(170,215,255)`），soundDefName=`MMT_FastReleaseSD`。
- **⚠ 小扩展（§6-B）**：投射物命中附加 Hediff——命中挂 `MMT_Debuff_Chill`（移动 ×0.6，5s）。
- **特效详情**：冰晶 1.1 格细长锥形；弹速 65 格/s；飞行尾部冰晶碎屑粒子（投射物 fleck——若需专属粒子列为可选，阶段一用默认）；命中蓝色冰晶爆点。
- **音效**：`MMT_FastReleaseSD`。
- **图标**：`UI/RoM/Icebolt`。
- **贴图**：需新出 `MMT_Icebolt.png`。

#### 3.7 暴风雪（II 阶，中技）

- **定位**：区域持续冰伤+减速，冰系控场主技。
- **框架**：`groundZone`。radius=5，targetFriendly=false，range=25，castSeconds=2.5，zoneDurationSeconds=10，zoneIntervalTicks=20，zoneAffectEveryone=false（仅敌方），zoneDamage=3，zoneDamageDefName=`Frost`，zoneHediffDefName=`MMT_Debuff_Chill`，zoneHediffSeverity=0.05，zoneTint=`(0.6, 0.85, 1)`。
- **特效详情**：冰蓝 AuraDisc 圆盘直径 10 格三层缓旋；**区域上方冰晶/雪花粒子下落**——若 groundZone 无顶部粒子则需小扩展（列为可选，阶段一仅圆盘+命中冰晶爆点）。
- **音效**：`MMT_AirWooshLow`（寒风）。
- **图标**：`UI/RoM/blizzard`。
- **贴图**：复用 `Things/Mote/MMT_AuraDisc`。
- **效果**：每 20 tick 3 点 Frost + 累积 `MMT_Debuff_Chill`。

#### 3.8 冰墙术（II 阶，中技）⚠

- **定位**：一排冰墙封路挡枪线。
- **框架**：`wall` + ⚠ **墙体 Def 参数（§6-B，与工程师能量屏障同一扩展）**：新建筑 `MMT_IceWall`（耐久 120，25s 自毁，蓝色半透明）。
- **参考参数**：radius=2.5，range=15，castSeconds=1.5，wallDurationSeconds=25。
- **特效详情**：冰墙拔地而起瞬间冰晶爆点（ChunkRock_Drop 音效配合）；墙体半透明冰蓝 `(0.6, 0.85, 1, 0.6)`（提示词 §十四）。
- **音效**：`ChunkRock_Drop`（原版，阶段一）；扩展后配 `MMT_FastReleaseSD`。
- **图标**：`UI/RoM/encase`。
- **贴图**：需新出 `MMT_IceWall`（配扩展）。
- **阶段一取舍**：扩展未做前先放石墙顶测。

#### 3.9 寒冰屏障（II 阶，中技）⚠

- **定位**：冰箱——短时近乎无敌但不能动（保命）。
- **框架**：`bufffx`。自身，castSeconds=0.5，buffHediffDefName=`MMT_Buff_IceBarrier`（承伤 ×0.2、移动 0，6s），buffIconTexPath=`UI/RoM/encase`，buffIconTint=`(0.6, 0.85, 1)`，buffIconSeconds=2.5。
- **⚠ 小扩展（§6-B，可选）**：bufffx 覆盖物——持续期罩 `Thing_MMTIceBlock` 冰块（尺寸 1.7 格，淡入/融化淡出，复用寒风领域 Attach 机制）；扩展未做则只有图标+粒子。
- **特效详情**：头顶冰晶图标 2.5s；身上冰蓝粒子；冰块罩（扩展后）1.7 格罩全身，生成时随机朝向不旋转。
- **音效**：`MMT_VibrationLow`。
- **图标**：`UI/RoM/encase`。
- **贴图**：复用 `Things/Mote/MMT_IceBlock`（扩展后）。

#### 3.10 冰霜新星（III 阶，大招）⚠

- **定位**：以自身为中心的冰环爆发，冻结周围所有敌人——近身解围神技。
- **框架**：⚠ **需新框架原型**（backlog §一.1 新星环，与牧师神圣新星/法师魔爆共用一次落地）。命中效果：敌方 8 点 Frost 伤害 + `StaggerFor(180)`（3s 冻结）+ 罩冰块（复用 zoneIceScale 机制）。
- **参考参数**：扩散速度 3.5 格/s、最大半径 5、每目标一次、castSeconds=1。
- **特效详情**：冰蓝 `(0.6, 0.85, 1)` 环扩散至直径 10 格，环带 0.8 格；贴图 `MMT_NovaRing`（与牧师共用，代码染色区分）；被冻目标罩冰块 1.7 格。
- **音效**：`MMT_FastReleaseSD` + `MMT_AirWooshLow`。
- **图标**：`UI/RoM/Snowball`。
- **阶段一取舍**：暂缓，等新星环原型。

### 奥系

#### 3.11 奥术飞弹（I 阶，中技）

- **定位**：浮游齐射，奥系输出主技。
- **框架**：`funnelShot`。radius=0，targetFriendly=false，range=25，castSeconds=2，funnelCount=4，ringRadius=1.6，aimTicks=15，projectileDefName=`MMT_Proj_ArcaneMissile`（**新投射物**：damageDef=Blunt（奥术冲击用钝击代）伤害 8、弹速 60、贴图 `Things/Projectile/MMT_ArcaneMissile.png` 新出——紫色梭形奥术弹，提示词 §十五，drawSize (1.0,1.0)，color `(190,120,255)`），funnelFleckDefName=`MMT_Fleck_ArcaneCharge`（**新 fleck 贴图** `MMT_ArcaneCharge.png` 紫色奥术浮游单元，提示词 §十六；可先默认火球充能顶测），soundDefName=`MMT_LightZip`。
- **特效详情**：4 枚紫色奥术单元绕身环（半径 1.6 格）悬浮瞄准 15 tick 后连续齐发；单弹 1.0 格梭形；弹速 60。
- **音效**：`MMT_LightZip`。
- **图标**：`UI/RoM/arcanebolt`。
- **贴图**：需新出 `MMT_ArcaneMissile.png` + `MMT_ArcaneCharge.png`。

#### 3.12 折跃（I 阶，小技）

- **定位**：瞬移机动，法师保命核心。
- **框架**：`blink`。radius=0，range=12，castSeconds=0.5。
- **特效详情**：起点+落点紫色闪光（MMT_BlinkFlash 染紫 `(0.7,0.4,1)`——若位移 fleck 染色需小扩展，阶段一默认色）；位移瞬时。
- **音效**：`MMT_LightZip`。
- **图标**：`UI/RoM/blink`。
- **贴图**：复用 `Things/Mote/MMT_BlinkFlash`。

#### 3.13 时空减速（II 阶，中技）

- **定位**：群体减速软控。
- **框架**：`bufffx`。radius=5，targetFriendly=false，range=20，castSeconds=2，buffHediffDefName=`MMT_Debuff_Slow`（已有），buffIconTexPath=`UI/RoM/acceleratetime`，buffIconTint=`(0.7, 0.4, 1)`，buffIconSeconds=2。
- **特效详情**：圈内敌方头顶紫色沙漏图标（≈0.5 格）2s + 紫色粒子。
- **音效**：`MMT_Vibration`。
- **图标**：`UI/RoM/acceleratetime`。
- **贴图**：复用。

#### 3.14 奥术回响（III 阶，中技）

- **定位**：放出一颗奥术浮游球，自主漂向敌人脉冲放电。
- **框架**：`summonOrb`。radius=0，targetFriendly=false，range=15，castSeconds=1.5，summonOrbTexPath=`Things/Mote/MMT_SummonOrb`，summonOrbTint=`(0.7, 0.4, 1)`，summonOrbTicksPerCell=12，summonDamage=9，summonDamageDefName=（空=Flame；若想纯奥术可填 `Blunt`），summonPulseRadius=2.5，summonPulseIntervalTicks=18。
- **特效详情**：紫色雷球（≈1 格，白图染色），漂移 12 tick/格（比雷球 15 略快）；每 18 tick 紫色脉冲环（半径 2.5）。
- **音效**：`MMT_Vibration`。
- **图标**：`UI/RoM/eyeofthestorm`。
- **贴图**：复用 `Things/Mote/MMT_SummonOrb`（染色）。

#### 3.15 魔法造物（III 阶，中技）⚠

- **定位**：奥系特色辅助——凭空变出魔法食物（解决队伍断粮）。
- **框架**：⚠ **不走 SkillFX**——用 `ClassAbilityEffects.cs` 的 `CompAbilityEffect_MMTSpawnEffect`（生成物桶）：在目标格生成 `MMT_ManaBun`（**新物品**：魔法馒头，营养 0.6，24h 后消失（消失用 comp 或做成会腐烂 1 天），吃掉无心情加成）。这不是特效原型，是机制 comp，本职业唯一非框架技能。
- **参考参数**：radius=0，targetFriendly=true，range=8，castSeconds=2；一次生成 5 个。
- **特效详情**：目标格紫色奥术粒子聚合（BuffSpark 染紫 8 点，0.5s）→ 物品落地；物品贴图 `MMT_ManaBun.png`（新出，发光紫色面团，提示词 §十七）。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/provisioner`。
- **贴图**：需新出 `MMT_ManaBun.png`。

## 4. 天赋树节点草案（阶段二）

```
Start（0 费，就职自动解锁）
├─ 职业印记「奥术心智」（1 点）：灵能敏感度 +10%（小被动）
├─ I 阶链：火球术(1) → 寒冰箭(1) → 奥术飞弹(2) → 折跃(1) → 龙息术(2)
├─ 强化 I「元素亲和」（2 点，需 II 阶）：火/冰伤害 +10%、熵上限 +15
├─ II 阶链：烈焰风暴(2) → 暴风雪(2) → 冰墙术(2) → 时空减速(2) → 寒冰屏障(2)
├─ 强化 II「法力洪流」（3 点，需 III 阶）：熵恢复 +25%
├─ III 阶链：炎爆术(3) → 冰霜新星(3) → 陨石术(4) → 奥术回响(3) → 魔法造物(2)
└─ Keystone「大法师」（3 点）：全部技能读条 -15%，大招伤害 +15%
```

## 5. 本职业新增资产清单

| 资产 | 名称 | 说明 |
|---|---|---|
| 投射物+贴图 | `MMT_Proj_SkillIcebolt` / `MMT_Icebolt.png` | 寒冰箭（提示词 §十三） |
| 投射物+贴图 | `MMT_Proj_ArcaneMissile` / `MMT_ArcaneMissile.png` | 奥术飞弹（提示词 §十五） |
| fleck 贴图 | `MMT_ArcaneCharge.png` | 奥术浮游单元（提示词 §十六，可先默认顶测） |
| 建筑+贴图 | `MMT_IceWall` | 冰墙（配 §6-B 墙体参数，提示词 §十四） |
| 物品+贴图 | `MMT_ManaBun` | 魔法食物（提示词 §十七） |
| Hediff | `MMT_Debuff_Chill` / `MMT_Buff_IceBarrier` | 见总纲 §8 |
