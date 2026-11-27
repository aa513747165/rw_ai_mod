# MMT 新职业 · 牧师（Priest）技能设计

> 手工维护设计文档。总纲（阶段规划/框架速查/缺口登记/资源约定/新增 Hediff 表）见 `docs/mmt-new-classes-design.md`。
> 职业基调色：圣光金白 `(1, 0.9, 0.55)`；读条色 `(1, 0.9, 0.55, 0.5)`。数值均为设计锚点，待实测平衡。

## 1. 职业定位

治疗辅助为主（全职业唯一的治疗池/护盾/驱散体系），兼具中距圣光输出。灵能施法者，身板脆，依赖站位。

## 2. 技能总览（15 个）

| # | 技能 | 框架 demoType | 类型 | 阶位 | 阶段二消耗档 | 缺口 |
|---|---|---|---|---|---|---|
| 1 | 治疗术 | heal | 治疗·单体 | I | 小 | — |
| 2 | 惩击 | singleShot | 伤害·单体 | I | 小 | 新投射物 |
| 3 | 心灵安抚 | bufffx | 增益·单体 | I | 小 | 新 Hediff（总纲 §8） |
| 4 | 真言术·盾 | aura(护盾) | 护盾·近身范围 | I | 中 | — |
| 5 | 恢复祷言 | groundZone | 治疗·区域 | I | 中 | — |
| 6 | 勇气祝福 | bufffx | 增益·群体 | II | 中 | — |
| 7 | 圣光射线 | beam | 伤害·持续射线 | II | 中 | — |
| 8 | 驱散魔法 | dispel | 驱散·单体 | II | 中 | ✅ 原型已落地（第二十三步） |
| 9 | 治疗之环 | heal | 治疗·群体 | II | 中 | — |
| 10 | 信仰守护 | aura(增益) | 光环·友方 | II | 中 | — |
| 11 | 神圣新星 | nova | 伤害+治疗·自身周围 | III | 大 | ✅ 原型已落地（第二十二步） |
| 12 | 圣光审判 | delayedStrike | 伤害·范围大招 | III | 大 | ✅ 坡道光柱皮肤已落地（演示「测试·圣光审判」） |
| 13 | 神圣赞美诗 | heal | 治疗·全屏级 | III | 大 | — |
| 14 | 守护天使 | summonAnimal | 召唤·光元素 | III | 大 | ⚠ 新 PawnKind 资产（总纲 §6-C） |
| 15 | 复活术 | ⚠ 新机制 | 复活·单体 | III | 大 | ⚠ backlog 无，全新机制 |

## 3. 技能卡

### 3.1 治疗术（I 阶，小技）

- **定位**：基础单体治疗，快速抬血。
- **框架**：`heal`。radius=1.5（小圈，基本只罩住目标），targetFriendly=true，range=25，castSeconds=1.2，healAmount=20。
- **特效详情**：框架内置表现——目标头顶绿十字（HealingCross，尺寸 ≈1 格）+ 绿色治疗粒子上升；颜色：绿十字改金绿 `(0.6, 1, 0.5)`（如需换色则走 ⚠ 小扩展：heal 粒子染色参数，现阶段接受默认绿）；无旋转/缩放动画（fleck 自带淡出）；粒子速度=框架默认（约 0.5 格/秒上升）。
- **音效**：`MMT_Powerup`（RoM 复刻，柔和充能声）。
- **图标**：`UI/RoM/Heal`。
- **贴图**：复用框架内置（无新增）。
- **效果**：治疗池 20 点按伤口分配（框架机制）。

### 3.2 惩击（I 阶，小技）

- **定位**：基础输出，填充技。
- **框架**：`singleShot`。radius=0，targetFriendly=false，range=25，castSeconds=1.5，projectileDefName=`MMT_Proj_HolyBolt`（新投射物：damageDef=Flame 之外无神圣伤害类型，用 `Burn` 伤害 12、弹速 70，金色 `(255, 215, 110)`，drawSize (1.2,1.2)，贴图 `Things/Projectile/MMT_HolyBolt`），soundDefName=`MMT_Launch`。
- **特效详情**：投射物直径 1.2 格（比普通子弹大，小于火球 1.6）；不旋转（朝北贴图代码自旋朝向）；大小恒定；弹速 70 格/s（快于火球，体现"光弹"）。
- **音效**：`MMT_Launch`（RoM 发射声）。
- **图标**：`UI/RoM/rayoflight`。
- **贴图**：**需新出** `MMT_HolyBolt.png`（64×64，金色光弹，提示词见提示词文档 §一）。

### 3.3 心灵安抚（I 阶，小技）

- **定位**：单体情绪/疼痛安抚，辅助。
- **框架**：`bufffx`。radius=0，targetFriendly=true，buffRequireTarget=true，range=15，castSeconds=1，buffHediffDefName=`MMT_Buff_Soothed`，buffIconTexPath=`UI/RoM/soothe`，buffIconTint=`(0.6, 0.9, 1)`，buffIconSeconds=2。
- **特效详情**：目标头顶安抚图标（尺寸 ≈0.5 格，框架默认）淡入淡出 2s；身上粒子=MMT_Fleck_BuffSpark 淡蓝 `(0.6, 0.9, 1)`，粒子随机闪现无固定旋转；缩放=粒子自带 0.3→0 收缩。
- **音效**：`MMT_Pulse`（轻柔脉冲）。
- **图标**：`UI/RoM/soothe`。
- **贴图**：复用（图标即 RoM 图标，粒子复用 `MMT_BuffSpark`）。
- **效果**：挂 `MMT_Buff_Soothed`（疼痛 ×0.5、情绪 +8，30s，见总纲 §8）。

### 3.4 真言术·盾（I 阶，中技）

- **定位**：给身边友方套临时护盾，救急。
- **框架**：`aura`（auraShieldMode=true）。radius=2.5（以自身为中心，罩住贴身友方），targetFriendly=true，castSeconds=1，auraDurationSeconds=10，auraShieldHitPoints=150，auraTint=`(1, 0.9, 0.5)`。
- **特效详情**：原版范围护盾投影器视觉——半透明半球罩半径 2.5 格，受击时盾面涟漪（原版机制）；无旋转；缩放=生成瞬间 0.8→1 弹开（原版默认）；颜色金 tint。
- **音效**：`MMT_Gong`（圣钟鸣响，护盾展开感）。
- **图标**：`UI/RoM/Shield`。
- **贴图**：复用（原版护盾投影器视觉）。
- **效果**：原版临时护盾机制，150 点护盾值，10s 到期消失。

### 3.5 恢复祷言（I 阶，中技）

- **定位**：铺一片恢复区，站桩回血。
- **框架**：`groundZone`。radius=3.5，targetFriendly=true，range=20，castSeconds=2，zoneDurationSeconds=15，zoneIntervalTicks=60，zoneAffectEveryone=false（仅友方——实现时注意 groundZone 现有"仅敌方"是 targetFriendly=false；友方版=zoneAffectEveryone=false + targetFriendly=true，需实现时确认敌我过滤逻辑支持），zoneHediffDefName=`MMT_Buff_HolyRegen`，zoneHediffSeverity=0.02，zoneTint=`(1, 0.9, 0.5)`。
- **特效详情**：区域圆盘贴图 MMT_AuraDisc 金色 `(1, 0.9, 0.5)`，直径 7 格（=radius 3.5）；三层圆盘缓速反向旋转（框架自带，约 3°/tick）；无缩放动画；边缘半透明渐隐。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/Regen`。
- **贴图**：复用 `Things/Mote/MMT_AuraDisc`。
- **效果**：站内友方挂 `MMT_Buff_HolyRegen`（持续缓回，见总纲 §8）。

### 3.6 勇气祝福（II 阶，中技）

- **定位**：群体攻击增益。
- **框架**：`bufffx`。radius=5，targetFriendly=true，range=20，castSeconds=2，buffHediffDefName=`MMT_Buff_Valor`，buffIconTexPath=`UI/RoM/inspire`，buffIconTint=`(1, 0.85, 0.3)`，buffIconSeconds=2。
- **特效详情**：圈内每个友方头顶金色剑盾图标（≈0.5 格）2s；金色 BuffSpark 粒子环绕；粒子无定向运动。
- **音效**：`MMT_Gong`。
- **图标**：`UI/RoM/inspire`。
- **贴图**：复用。
- **效果**：挂 `MMT_Buff_Valor`（命中 +4 级 移速+20%，20s）。

### 3.7 圣光射线（II 阶，中技）

- **定位**：持续输出主技，引导型。
- **框架**：`beam`。radius=0，targetFriendly=false，showRangeRing=false，castSeconds=2，soundDefName=`ChargeLance_Fire`，beamDurationSeconds=4，beamRange=18，beamWidth=0.5，beamTint=`(1, 0.9, 0.5)`，beamMuzzleOffset=0.9，beamHeadTexPath=`Things/Mote/MMT_SkillBeam_Head`，beamTailTexPath=`Things/Mote/MMT_SkillBeam_Tail`，beamHeadSize=1.2，beamTailSize=1.4，beamDamage=5，beamDamageIntervalTicks=10。
- **特效详情**：光柱宽 0.5 格（比烈焰射线 0.7 细，体现凝聚的圣光）；出膛偏移 0.9 格；发射头光球 1.2 格平躺；命中端爆点 1.4 格独立脉冲；整体金色 `(1, 0.9, 0.5)`；光柱宽度呼吸抖动+淡入淡出（框架自带）；贴图不旋转（沿光束方向拉伸）。
- **音效**：`ChargeLance_Fire`（原版）。
- **图标**：`UI/RoM/lightlance`。
- **贴图**：复用（白图代码染金色）。
- **效果**：每 10 tick 5 点 Burn 伤害，持续 4s。

### 3.8 驱散魔法（II 阶，中技）

- **定位**：移除友方减益 / 移除敌方增益。
- **框架**：`dispel` —— ✅ **原型已落地**（第二十三步，`SkillDispel.cs`，演示「测试·驱散魔法」/「测试·驱散魔法·敌」）。**只移除有持续时间的状态**（带 `HediffComp_Disappears` 的限时 Hediff；伤疤、植入体、成瘾等永久/无时限状态不可驱散）；敌我自动分流——友方移除减益、敌方移除增益，按 Severity 高者优先。
- **参考参数**：radius=0、单体锁定、targetFriendly=true、range=20、castSeconds=1、dispelCount=3；敌用版另设条目（targetFriendly=false）。
- **特效详情**：目标头顶爆出白色 `(1,1,1)` 净化图标闪光（尺寸 0.5 格，无旋转，0.5s 淡出），每驱散 1 个状态再爆 1 次白色微光点（原版 `ThrowMicroSparks`）。
- **音效**：`MMT_FastReleaseSD`。
- **图标**：`UI/RoM/purify`。

### 3.9 治疗之环（II 阶，中技）

- **定位**：群体治疗主技。
- **框架**：`heal`。radius=5，targetFriendly=true，range=25，castSeconds=2.5，healAmount=12。
- **特效详情**：圈内每个友方头顶绿十字（≈1 格）+ 治疗粒子；其余同 3.1；范围圈 5 格=瞄准预览圈。
- **音效**：`MMT_Powerup`。
- **图标**：`UI/RoM/healingcircle`。
- **贴图**：复用。
- **效果**：圈内每个友方各得 12 点治疗池。

### 3.10 信仰守护（II 阶，中技）

- **定位**：常驻守护光环，团队减痛加速。
- **框架**：`aura`（增益模式）。radius=4，targetFriendly=true，castSeconds=2，auraDurationSeconds=20，auraBuffDefName=`MMT_AuraBuff_Faith`，auraRefreshIntervalTicks=60，auraTint=`(1, 0.9, 0.5)`。
- **特效详情**：脚下金色光圈（MMT_AuraDisc，直径 8 格）跟随施法者移动，三层缓旋（≈3°/tick）；半透明（alpha ≈0.35，贴图自带）；无缩放。
- **音效**：`MMT_Pulse`。
- **图标**：`UI/RoM/enchantedaura`。
- **贴图**：复用 `Things/Mote/MMT_AuraDisc`。
- **效果**：光环内友方脉冲挂 `MMT_AuraBuff_Faith`（疼痛 ×0.8、移动 +5%）。

### 3.11 神圣新星（III 阶，大招）✅ 框架已落地

- **定位**：以自身为中心的金色冲击波，伤敌+疗友。
- **框架**：✅ `nova`（第二十二步新星环原型，`SkillNova.cs`：`Thing_MMTNovaRing` 半径匀速增大，环带 ±0.5 格判定命中，已命中集合去重）。命中双效已支持：敌方吃 15 点伤害、友方吃 8 点治疗池（`novaDamage`/`novaHealAmount` 参数，纯伤害/纯治疗皮肤把另一项置 0）。
- **参考参数**：radius=6（最大半径）、扩散速度 3 格/s（`novaCellsPerSecond`）、每目标只命中一次（`novaHitOnce`）、castSeconds=2。演示条目「测试·神圣新星」已按本参数落地（`MMT_SkillDemoSet.xml`，含效果设计卡注释）。
- **特效详情**：金色 `(1, 0.85, 0.4)` 光环从脚下扩散至直径 12 格，环带宽度 0.8 格（贴图环带随扩散等比变宽）；不旋转（径向扩散）；缩放=半径 0→6 线性增大；波峰亮度最高，通过后 0.3s 拖尾淡出，到边后整体 0.3s 淡出自毁；贴图 `MMT_NovaRing`（白图圆环，代码染色，已出图完成、提示词见 `docs/mmt-skillfx-texture-prompts.md` §十三）。
- **音效**：`MMT_Gong`（爆发瞬间）。
- **图标**：`UI/RoM/lightburst`。

### 3.12 圣光审判（III 阶，大招）✅ 皮肤已落地

- **框架**：`delayedStrike` + **坡道光柱皮肤**（`meteorUsePillarRamp`，第二十七步坡道框架复用，坠落视觉由坡道光柱承担、计时/爆炸不变；pawn 技能侧走 `CompProperties_MMTDelayedStrike.pillarRamp*` 字段）。radius=2.5，targetFriendly=false，range=25，castSeconds=2.5，meteorDelaySeconds=1.5，meteorFallTicks=20，meteorDamage=45，meteorExplosionRadius=2.5，meteorIgniteChance=0。
- **特效详情**：贴图 `MMT_HolyPillar`（128×256 单帧白图，代码染金 `(1, 0.9, 0.55)`，已出图落地），宽 3.0 格 × 高 4.8 格，8 片坡道带；顶部 alpha 衰减至 0.45（上端延伸出屏幕=来自高空）、顶端微扩 1.1（光源在上）、摆动 0.06（庄重）、脉冲 0.08，坠落 20 tick 内向南压落 0.6 格（负漂移=砸落感）；落点金色爆闪 + 预警圈 2.5 格。演示条目「测试·圣光审判」已按本参数落地（`MMT_SkillDemoSet.xml`，含效果设计卡注释），**游戏内效果待验证**（坡道原型首个实战皮肤，顺带验证坡道本体）。
- **音效**：`MMT_Gong`（光柱降下，迫近音效位）+ 原版爆炸音（落点，框架内置）。
- **图标**：`UI/RoM/sol`。
- **贴图**：✅ `Things/Mote/MMT_HolyPillar`（已出图）。

### 3.13 神圣赞美诗（III 阶，大招）

- **定位**：全团大加，翻盘技。
- **框架**：`heal`。radius=9，targetFriendly=true，range=30，castSeconds=4，healAmount=25。
- **特效详情**：大范围 9 格内全体绿十字+治疗粒子齐发；读条 4s 读条色金 `(1, 0.9, 0.55, 0.5)`；其余同框架默认。
- **音效**：`MMT_Powerup`（起）+ `MMT_Gong`（成）。
- **图标**：`UI/RoM/anthem`。
- **贴图**：复用。

### 3.14 守护天使（III 阶，大招）⚠资产

- **定位**：召唤一只光元素护卫，跟随作战。
- **框架**：`summonAnimal`（框架本身支持，阵营=施法者派系）。castSeconds=2.5，range=10，summonAnimalKindDefName=`MMT_HolyGuardian`（**新 PawnKind+race，见总纲 §6-C**：仿 `MMT_Races_FireElemental` 做光元素 race，人形/浮游光体，近战拍击+小治疗脉冲；**不可**用机械体 PawnKind），summonAnimalDurationSeconds=30。
- **特效详情**：召唤落点金色 BlinkFlash 闪光（≈1.5 格，0.3s 淡出）+ 光元素本体贴图（身高 ≈1 格，半透明金色光体，缓浮上下 ±0.1 格漂动——若漂动需 race comp 支持则列为可选）。
- **音效**：`MMT_Gong`。
- **图标**：`UI/RoM/summon`。
- **贴图**：需新出光元素 race 身体贴图（东南西北向，提示词 §四）。
- **阶段一取舍**：race 资产工作量大，可先用 `MMT_FireElementalLesser` 顶测流程（视觉不符，仅验证机制）。

### 3.15 复活术（III 阶，大招）⚠

- **定位**：复活一名死亡的殖民者（限时尸体未腐）。
- **框架**：⚠ **全新机制**（backlog 无）。落地参考原版复活血清（`ResurrectionUtility`）：目标=友方尸体，读条 6s 后复活，附加复活后遗症 Hediff（参考原版 ResurrectionPsychosis 的虚弱期）；视觉=尸体上浮空 0.5 格 + 金色光柱笼罩（复用 `MMT_SkillBeam_Head` 竖直摆法或 groundFire 柱体无伤害版）+ 完成时 Gong。
- **参考参数**：range=8，castSeconds=6，冷却档=大招；限制：尸体腐坏/头颅被毁不可复活。
- **特效详情**：金色光柱直径 1.5 格罩住尸体，柱高 2 格（水平碟堆叠法，同 groundFire 柱体参数：层数 6、顶端缩放 0.5、摆动 0.2），颜色 `(1, 0.95, 0.7)`；持续整个读条；完成时尸体处爆开金色 BuffSpark 粒子 10 点。
- **音效**：`MMT_Gong` + `MMT_Powerup`。
- **图标**：`UI/RoM/resurrection`。
- **阶段一取舍**：暂缓，机制原型单独排期。

## 4. 天赋树节点草案（阶段二）

```
Start（0 费，就职自动解锁）
├─ 职业印记「圣光亲和」（1 点）：治疗溢出 10% 转护盾（小被动）
├─ I 阶链：治疗术(1) → 惩击(1) → 心灵安抚(1) → 真言术·盾(2) → 恢复祷言(2)
├─ 强化 I「深湛法力」（2 点，需 II 阶）：熵上限 +15
├─ II 阶链：勇气祝福(2) → 圣光射线(2) → 驱散魔法(2) → 治疗之环(3) → 信仰守护(3)
├─ 强化 II「神圣源泉」（3 点，需 III 阶）：熵恢复 +20%
├─ III 阶链：神圣新星(3) → 圣光审判(3) → 神圣赞美诗(4) → 守护天使(4) → 复活术(4)
└─ Keystone「大祭司」（3 点）：全部治疗量 +15%，圣光伤害 +15%
```

## 5. 本职业新增资产清单

| 资产 | 名称 | 说明 |
|---|---|---|
| 投射物+贴图 | `MMT_Proj_HolyBolt` / `MMT_HolyBolt.png` | 惩击用金色光弹（提示词 §一） |
| 贴图 | `MMT_NovaRing.png` | 神圣新星环（已出图完成，提示词 §十三） |
| —— | 驱散魔法无新增资产 | 复用 `Thing_MMTBuffIconFlash` 图标桶 + 原版微光点（`dispel`，第二十三步） |
| 贴图 | `MMT_HolyPillar.png` | ✅ 圣光审判坡道光柱皮肤（已出图落地，单帧白图代码染金） |
| race+PawnKind+贴图 | `MMT_HolyGuardian` | 守护天使光元素（提示词 §四） |
| Hediff | `MMT_Buff_Soothed` / `MMT_Buff_Valor` / `MMT_AuraBuff_Faith` / `MMT_Buff_HolyRegen` | 见总纲 §8 |
