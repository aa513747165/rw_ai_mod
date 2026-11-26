# MMT 新职业 · 圣骑士（Paladin）技能设计

> 手工维护设计文档。总纲见 `docs/mmt-new-classes-design.md`。
> 职业基调色：金黄 `(1, 0.8, 0.3)`；读条色 `(1, 0.8, 0.3, 0.5)`。数值均为设计锚点，待实测平衡。

## 1. 职业定位

坦克：自身减伤、护盾、嘲讽、掩护队友，兼近战圣光打击。站前排吃伤害，给后排创造输出环境。灵能施法者（耗熵模型同其他职业），但战斗半径小、机动靠冲锋。

## 2. 技能总览（15 个）

| # | 技能 | 框架 demoType | 类型 | 阶位 | 阶段二消耗档 | 缺口 |
|---|---|---|---|---|---|---|
| 1 | 圣盾术 | aura(护盾) | 护盾·自身近圈 | I | 中 | — |
| 2 | 圣光之锤 | strike | 伤害·单体 | I | 小 | ⚠ 落雷皮肤（可接受） |
| 3 | 正义之锤 | knockback | 近战击退 | I | 小 | — |
| 4 | 圣光治愈 | heal | 治疗·自身/单体 | I | 小 | — |
| 5 | 冲锋陷阵 | charge | 位移·突进 | I | 小 | — |
| 6 | 虔诚护盾 | bufffx | 减伤·自身 | II | 小 | — |
| 7 | 嘲讽 | ⚠ 新机制 | 控制·群体 | II | 中 | ⚠ 总纲 §6-C 嘲讽 |
| 8 | 奉献 | groundFire | 伤害·持续区域 | II | 中 | — |
| 9 | 神圣之地 | groundZone | 减伤·区域 | II | 中 | — |
| 10 | 力量祝福 | bufffx | 增益·群体 | II | 中 | — |
| 11 | 保护祝福 | bufffx | 减伤·单体友方 | II | 中 | — |
| 12 | 援护 | blink | 位移·驰援 | II | 小 | ⚠ 落点套盾为小扩展（可选） |
| 13 | 制裁之锤 | knockback | 控制·单体外加踉跄 | III | 中 | ⚠ 击退加 stagger 参数（§6-B） |
| 14 | 复仇之怒 | bufffx | 自我强化·大招 | III | 大 | — |
| 15 | 圣光壁垒 | aura(护盾) | 护盾·团队大招 | III | 大 | — |

## 3. 技能卡

### 3.1 圣盾术（I 阶，中技）

- **定位**：自伤自扛的核心护盾。
- **框架**：`aura`（auraShieldMode=true）。radius=1.5（仅罩自己+贴身者），castSeconds=0.8，auraDurationSeconds=15，auraShieldHitPoints=300，auraTint=`(1, 0.8, 0.3)`。
- **特效详情**：原版范围护盾半球罩，直径 3 格；受击涟漪；生成 0.8→1 弹开；金黄 tint；无旋转。
- **音效**：`MMT_Gong`。
- **图标**：`UI/RoM/Shield`。
- **贴图**：复用（原版护盾投影器视觉）。

### 3.2 圣光之锤（I 阶，小技）

- **定位**：单体远程打击，圣锤砸顶。
- **框架**：`strike`。radius=0，targetFriendly=false，strikeRequireTarget=true，range=15，castSeconds=1，strikeDamage=20，strikeExplosionRadius=1（基本单体），strikeArmorPenetration=0.2。
- **特效详情**：⚠ 框架 strike 视觉为原版落雷（蓝白闪电）——本技能接受"金色雷=圣锤"的近似，配色不可改；如后期做 §6-B 的 strike 皮肤参数，换金色光柱（贴图 `MMT_HolyPillar` 复用牧师资产）。落点爆炸半径 1 格、伤害 30。
- **音效**：`Thunder_OnMap`（原版，框架默认）。
- **图标**：`UI/RoM/earthernhammer`。
- **贴图**：复用（阶段一接受落雷皮肤）。

### 3.3 正义之锤（I 阶，小技）

- **定位**：近战击退，打断+拉开距离。
- **框架**：`knockback`。radius=0，targetFriendly=false，range=3，castSeconds=0.5，knockbackDamage=15，knockbackCells=4，knockbackWallSlamDamage=8。
- **特效详情**：框架自带命中闪光（MMT_BuffSpark 金色爆点 ≈1 格）+ 目标击飞位移 4 格（位移耗时按格数线性，框架默认）；撞墙追加 8 点。无旋转/缩放参数。
- **音效**：`MMT_Woosh`（挥击风声）。
- **图标**：`UI/RoM/heavyblow`。
- **贴图**：复用。

### 3.4 圣光治愈（I 阶，小技）

- **定位**：自奶/救队友，牧师之外的次级治疗。
- **框架**：`heal`。radius=1.5，targetFriendly=true，includeSelf=true，range=15，castSeconds=1.5，healAmount=25。
- **特效详情**：绿十字+治疗粒子（框架默认绿，接受）；粒子 ≈0.5 格上升。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/innerhealing`。
- **贴图**：复用。

### 3.5 冲锋陷阵（I 阶，小技）

- **定位**：突进接敌，坦克的入场技。
- **框架**：`charge`。radius=0，range=12，castSeconds=0.3，chargeTicksPerCell=1.5，displaceTrailFleckDefName=`MMT_Fleck_DashTrail`。
- **特效详情**：路径拖尾残影（MMT_DashTrail，每 3 tick 一枚，≈1 格，0.4s 淡出）；冲锋速度 1.5 tick/格（12 格约 0.3s）；起点金色闪光。颜色：拖尾默认色（如需金色则 fleck 贴图白图已支持染色——若现框架 DashTrail 无染色参数，列为可选小扩展，阶段一接受默认）。
- **音效**：`MMT_AirWoosh`。
- **图标**：`UI/RoM/valiantcharge`。
- **贴图**：复用。

### 3.6 虔诚护盾（II 阶，小技）

- **定位**：主动减伤，应对爆发。
- **框架**：`bufffx`。radius=0，targetFriendly=true，includeSelf=true，buffRequireTarget=false（对自身瞬发，实现时以自身为目标），castSeconds=0.5，buffHediffDefName=`MMT_Buff_Devotion`，buffIconTexPath=`UI/RoM/stoneskin`，buffIconTint=`(1, 0.8, 0.3)`，buffIconSeconds=2。
- **特效详情**：头顶金色盾形图标 2s；身上金色 BuffSpark 粒子环绕 2s。
- **音效**：`MMT_Pulse`。
- **图标**：`UI/RoM/stoneskin`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_Devotion`（承伤 ×0.7，12s）。

### 3.7 嘲讽（II 阶，中技）⚠

- **定位**：强制周围敌人攻击自己，保护后排。
- **框架**：⚠ **全新机制**（总纲 §6-C）。落地要点：半径 6 格内敌对 pawn 挂"仇恨锁定"Hediff（8s），配 Harmony patch 在 `AttackTargetFinder` 目标评分中把施法者权重拉满；机械体/无智能单位免疫。视觉：bufffx 式图标（嘲讽标记）+ 施法者脚下金色光环脉冲。
- **参考参数**：radius=6，castSeconds=0.5，冷却档=中技。
- **特效详情**：被嘲讽敌人头顶红色 `(1, 0.25, 0.2)` 感叹号图标（需新贴图 `MMT_TauntMark.png`，0.5 格，提示词 §五）1.5s；施法者周身金色 BuffSpark 爆发 6 点。
- **音效**：`MMT_Gong`。
- **图标**：`UI/RoM/taunt`。
- **阶段一取舍**：机制未做前，可用 `ClassAbilityEffects.cs` 的 `CompAbilityEffect_MMTMentalStateAoE`（狂暴 MentalState）做临时近似（视觉走 bufffx 参数），但正式版必须换真嘲讽。

### 3.8 奉献（II 阶，中技）

- **定位**：脚下圣焰地带，灼烧来犯之敌（经典 tank 站桩技）。
- **框架**：`groundFire`（火海模式）。radius=4，targetFriendly=false，castSeconds=1.5，groundDurationSeconds=8，groundAreaRadius=4，groundDensity=0.85，groundVisualScale=1.4，groundDamage=2，groundDamageIntervalTicks=20，groundIgnitePawnChance=0，groundIgniteCellChance=0（圣焰不引燃环境——点燃概率清零），groundTint=`(1, 0.85, 0.4)`，groundTexPath=`Things/Mote/MMT_FlamePillar`。
- **特效详情**：贴地火盘 1.4 格/簇，金白色 `(1, 0.85, 0.4)`（白图代码染色）；覆盖率 0.85 铺满 4 格半径；火盘无堆叠火柱（groundPillarHeight 调 0.6 压低，圣焰贴地）；不旋转。
- **音效**：`MMT_FireBurn`。
- **图标**：`UI/RoM/sunfire`。
- **贴图**：复用 `Things/Mote/MMT_FlamePillar`（染色即可）。
- **注意**：火焰引燃概率清零，避免圣焰烧自家地板。

### 3.9 神圣之地（II 阶，中技）

- **定位**：给团队铺减伤区，配合奉献前后铺。
- **框架**：`groundZone`。radius=3.5，targetFriendly=true，range=15，castSeconds=1.5，zoneDurationSeconds=15，zoneIntervalTicks=60，zoneAffectEveryone=false（仅友方），zoneHediffDefName=`MMT_Buff_Consecrated`，zoneHediffSeverity=0.02，zoneTint=`(1, 0.85, 0.4)`。
- **特效详情**：金色 AuraDisc 圆盘直径 6 格，三层缓旋（≈3°/tick），半透明渐隐边缘。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/magelight`。
- **贴图**：复用 `Things/Mote/MMT_AuraDisc`。
- **效果**：站内友方挂 `MMT_Buff_Consecrated`（承伤 ×0.75）。

### 3.10 力量祝福（II 阶，中技）

- **定位**：群体攻击增益（与牧师勇气祝福同位，坦克版偏近战）。
- **框架**：`bufffx`。radius=4，targetFriendly=true，range=15，castSeconds=1.5，buffHediffDefName=`MMT_Buff_Might`，buffIconTexPath=`UI/RoM/bestowmight`，buffIconTint=`(1, 0.75, 0.25)`，buffIconSeconds=2。
- **特效详情**：圈内友方头顶金色拳形图标（≈0.5 格）2s + 金色粒子。
- **音效**：`MMT_Gong`。
- **图标**：`UI/RoM/bestowmight`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_Might`（近战/射击伤害 ×1.15，20s）。

### 3.11 保护祝福（II 阶，中技）

- **定位**：点名保护濒危队友。
- **框架**：`bufffx`。radius=0，targetFriendly=true，buffRequireTarget=true，range=20，castSeconds=1，buffHediffDefName=`MMT_Buff_Protection`，buffIconTexPath=`UI/RoM/shieldother`，buffIconTint=`(1, 0.85, 0.35)`，buffIconSeconds=2.5。
- **特效详情**：目标头顶金盾图标 2.5s；身上持续金粒子上浮（时长=Hediff 8s 内每 30 tick 一批）。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/shieldother`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_Protection`（承伤 ×0.5，8s）。

### 3.12 援护（II 阶，小技）

- **定位**：折跃到被围队友身边救场。
- **框架**：`blink`。radius=0，targetFriendly=true（瞄友方，落点取目标邻格），range=25，castSeconds=0.5。
- **特效详情**：起点+落点金色 BlinkFlash 闪光（≈1.2 格，0.3s 淡出）；位移瞬时。
- **⚠ 可选小扩展**：落点后给半径 2 内友方套 50 点小护盾（需 blink 落地回调挂 aura 逻辑；阶段一不做，纯位移）。
- **音效**：`MMT_LightZip`。
- **图标**：`UI/RoM/lightskip`。
- **贴图**：复用。

### 3.13 制裁之锤（III 阶，中技）⚠

- **定位**：单体硬控——重锤打懵（眩晕）。
- **框架**：`knockback` 为基础。radius=0，targetFriendly=false，range=3，castSeconds=0.8，knockbackDamage=10，knockbackCells=1（原地砸懵而非击飞），knockbackWallSlamDamage=0。
- **⚠ 小扩展（总纲 §6-B）**：knockback 增加 `staggerTicks` 参数——命中后 `StaggerFor(180)`（3s 眩晕）；框架已有 StaggerFor 用法（击退落地踉跄同 API），工作量小。
- **特效详情**：命中金色爆点 1 格 + 目标头顶金星旋转粒子（复用 BuffSpark 染金，2s）。
- **音效**：`MMT_Woosh` + `MMT_Gong`。
- **图标**：`UI/RoM/overwhelm`。
- **阶段一取舍**：扩展未做前先按 1 格击退版落地，控制感靠击退打断。

### 3.14 复仇之怒（III 阶，大招）

- **定位**：自我爆发强化，残局收割/强开。
- **框架**：`bufffx`。radius=0，targetFriendly=true，includeSelf=true，castSeconds=1，buffHediffDefName=`MMT_Buff_AvengingWrath`，buffIconTexPath=`UI/RoM/overdrive`，buffIconTint=`(1, 0.7, 0.2)`，buffIconSeconds=3。
- **特效详情**：施法瞬间金色冲击闪光 2 格 + 头顶烈焰金图标 3s；持续期身上金粒子不断（每 30 tick 一批，15s）。
- **音效**：`MMT_Powerup` + `MMT_Gong`。
- **图标**：`UI/RoM/overdrive`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_AvengingWrath`（全伤害 ×1.3、移动 +10%，15s）。

### 3.15 圣光壁垒（III 阶，大招）

- **定位**：团队大护盾，扛集火/扛 Boss 大招。
- **框架**：`aura`（auraShieldMode=true）。radius=5，castSeconds=3，auraDurationSeconds=20，auraShieldHitPoints=400，auraTint=`(1, 0.85, 0.35)`。
- **特效详情**：原版护盾半球直径 10 格，金黄 tint；读条 3s（读条色金黄）；生成弹开 0.8→1；受击涟漪。
- **音效**：`MMT_Gong`（展开）+ 原版护盾受击音（机制自带）。
- **图标**：`UI/RoM/psionicbarrier`。
- **贴图**：复用。

## 4. 天赋树节点草案（阶段二）

```
Start（0 费，就职自动解锁）
├─ 职业印记「圣光铸躯」（1 点）：承伤 ×0.95（小被动）
├─ I 阶链：圣盾术(2) → 圣光之锤(1) → 正义之锤(1) → 圣光治愈(1) → 冲锋陷阵(1)
├─ 强化 I「坚韧壁垒」（2 点，需 II 阶）：熵上限 +15、承伤 ×0.95
├─ II 阶链：虔诚护盾(2) → 嘲讽(2) → 奉献(2) → 神圣之地(2) → 力量祝福(2) → 保护祝福(2) → 援护(1)
├─ 强化 II「不灭信仰」（3 点，需 III 阶）：熵恢复 +20%
├─ III 阶链：制裁之锤(3) → 复仇之怒(3) → 圣光壁垒(4)
└─ Keystone「白银之手」（3 点）：护盾值 +25%，光环半径 +1
```

## 5. 本职业新增资产清单

| 资产 | 名称 | 说明 |
|---|---|---|
| 贴图 | `MMT_TauntMark.png` | 嘲讽头顶感叹号（提示词 §五） |
| Hediff | `MMT_Buff_Devotion` / `MMT_Buff_Consecrated` / `MMT_Buff_Might` / `MMT_Buff_Protection` / `MMT_Buff_AvengingWrath` + 嘲讽用 `MMT_Debuff_Taunted` | 见总纲 §8 |
| 机制 | 嘲讽（AttackTargetFinder 权重 patch） | 总纲 §6-C |
