# MANA ODYSSEY 玩法机制调研报告

> 调研对象：Workshop mod `3846855664`「MANA ODYSSEY」（JellyCreative.AManaOdyssey，v1.0.1），
> 目录 `D:\SteamLibrary\steamapps\workshop\content\294100\3846855664`（只读参考，勿改动）。
> 支持 1.6/1.7，依赖 Harmony + Royalty，软兼容四 DLC + CombatExtended（`Compat/CombatExtended`）。
> **仅英文，无中文翻译。** 本文档为手工调研文档，非生成物。

---

## 一、核心循环：成为法师 → 成长 → 战斗

### 1. 魔核（Mana Core）与法师诞生

核心类 `Source/AManaOdyssey/Core/CompManaCore.cs`，comp 挂所有 pawn，首次被查询时按 pawn ID 种子 roll 一次（存档稳定）。

- 约 33%（设置 `mageShare`）是法师；其中约 50% 带一个元素亲和，其余为纯法力（raw mana）法师。敏感度 = 高斯(1, 0.22) 截断 0.45–2.2，4% 概率出 1.6–2.1 天才。
- **觉醒途径**：
  - **心灵链接**：每获得一级 psylink，魔核推进一阶；第一级唤醒非法师成为纯法力法师（Patches/ManaLink.xml 给 PsychicAmplifier hediff 加 comp）。
  - **基因**（Biotech）：`MO_Gene_MageBorn` 必定法师；各亲和基因 `MO_Gene_<X>` 使其成为对应亲和法师且不占亲和槽，可遗传（单方 50%、双方 100%），偶尔生出变异亲和后代。
  - 支持后天觉醒 `Awaken()`，未实装常规玩法入口。
- **亲和槽（Attunement）**：Single/Dual(1/40)/Tri(1/200)/Quadra(1/1000)。空槽在 Grimoire 界面以 RNG 填入；1/5 概率 Unattuned——无槽但可读任何魔法书。暗系可通过 Hollowing 剥夺他人亲和。
- **18 种基础亲和**三档：
  - Primary（3 星，权重各 10）：Fire、Water、Earth、Wind
  - Secondary（4 星，各 3）：Wood、Lightning、Cloud、Explosion、Metal、Ice、Blood
  - Primordial（5 星，各 0.3/0.2）：Light、Dark、Star、Time、Death、AntiMagic
- **Divergent 变异亲和**（比母系再高 1 星）：如 Fire 的 FrozenFire/Amaterasu/Ghostfire，Water 的 Acid/HadalWater/HolyWater。无自己的树，借母系树、以变异法术替换对应节点，带独特规则（Amaterasu 黑炎只有 Snuff 能扑灭、Penumbra 静止隐身）。
- **法力敏感度 = 原版 PsychicSensitivity**（StatPart 注入）。敏感度 0 不能施法。

### 2. 魔核精炼（成长轴）

- **18 阶位**（致敬《The Beginning After the End》）：6 Band（黑/红/橙/黄/银/白）× 3 Phase（暗/固/明），stage 0–17。
- **精炼方式 = 原版冥想**：每小时冥想 = 1 小时精炼 × 敏感度 × 魔法特质修正。每天前 N 小时全额，之后 1/4 效率（防刷）。第一阶 9 小时，每阶 ×1.22；每天冥想 4 小时约 5 年到顶。
- **阶位随威胁点数动态封顶**（300 威胁→3 阶，1000→6，3500→12，7000→满），防早期超模；设置中可对殖民者/陌生人分别关闭。
- **法术点来源**：初始 + 每阶给点（跨 Band 更多）+ 冥想随机（MTB）+ 战斗施法计数（第 n 点需 5·n(n+1)/2 次施法，仅战斗计入）。
- **法力池/回复**走自定义 Stat：被动约 8h 回满，冥想约 1.5h 回满。
- **法术消耗**无单独 cost 字段，按阶位表计费；纯法力法术贵 25%。引导类按秒额外扣。冷却写在每法术 `<cast><cooldown>`；另有 CastSpeed/CastRange/SpellPotency/ManaCostFactor/CooldownFactor 等自定义 stat 供装备/被动加成。

### 3. 亲和树（Magic Tree）

`Defs/MagicTrees/MagicTree_<X>.xml`（18 个），运行时 `Core/MagicTree.cs`，界面 `UI/Window_MagicTree.cs`。

- 每树若干支路，如火系 Sear（近战拳击）/Blaze（投射）/Cinder（区域）/Phoenix（增益召唤）/Hearth（治疗辅助）；纯法力 Force/Motion/Ward/Mind/Weave/Body。
- 节点 = 法术节点（方形）+ 被动节点（圆形 Small/Notable/Keystone 三档，Keystone 带副作用，如"威力 +25% 但消耗 +30%"）。
- **7 个法术阶位**：Novice/Adept/Expert/Saint/King/Emperor/God，每阶需魔核达对应 stage（0/3/6/9/12/15/17）。
- 链接一律向上，从已学节点解锁；学习花法术点（按阶 1/2/3/4/6/8/12；小被动 1、Notable 2、Keystone 3）。每个法术可升 5 级，每级 +20% 尺寸/强度，2–5 级各有升级被动，第 5 级为 mastery（如召唤物永久化）。
- 树数据同时被外部设计工具（VfxForge 的 magic_tree.html）读写，单一数据源。

### 4. 法术获得与施放

- **学法**：树内花法术点；或读 **Grimoire 魔法书**（`MO_Grimoire`，CompGrimoire 持一个法术，封面按亲和着色）——免法术点直接学会/升一级，消耗品。要求：法师、已解锁该亲和树（Unattuned 任意）、魔核阶位达法术阶位。Mana Domain 不能从书学。商人售卖、任务奖励。
- **施放体系是自研的，非原版 Ability/verb**：法术以 `Command_Spell` gizmo 排列在 pawn 下方（带 `Gizmo_ManaBar` 法力条），点击进入瞄准 → JobDef `MO_ManaCast`（`JobDriver_ManaCast`，被打不打断、眩晕打断）。右键法术图标设自动施放（auto-cast，仅征召生效）。
- 法术数据 = `<steps>`（视觉时间轴）+ `<cast>`（实际效果），同一数据源驱动演出与判定。cast 支持 5 种瞄准（Self/Point/Thing/Direction/Line 双点画线）、`<channel>` 引导、`<melee>` 近身打击系（贴身施放，可要求持近战武器）、`<landing>` 位移落点、弹道 `<flights>`（直线/抛物线/穿透/宽度/追踪 homing）、持续 beam。
- 位移用原版 PawnFlyer（`MO_SpellLeap/MO_SpellDash/MO_SpellShove`）。
- 效果体系 `SpellEffect_*`：Strike/Burst/Beam/Cone/Lance/Barrage/Chain/Pattern/Zone/Dome/Wall/Summon/Heal/Knockback/Control（精神控制，带 `MO_Marionette` 精神态）/TearOut（抽亲和）/GiveBack/MechPulse 等。

---

## 二、战斗机制亮点

### 1. 法术对波（Spell Clash）—— `ManaCast/SpellClash.cs`

- 两个敌对飞行法术（带伤害弹道）在一 tick 内路径相交（1 格内）即对撞：弱者伤害 ≥ 强者 × `clashEven` 则**同归于尽**，否则强者继续飞、弱者破碎；对撞点播放 `MO_Clash` 特效（双色、沿飞行方向）。
- **光束对波**：持续 beam 按每秒伤害计入；两道敌对 beam 正面相遇**互相推挤**——推挤点以 8 格/秒 × 强度差向弱方 caster 移动，到达则其 beam 破碎、对方 beam 贯穿命中。推挤点循环播放 `MO_Clash_Beam`。
- 可在设置关闭。

### 2. 领域（Mana Domain）与领域对撞

- 每系一个 God 阶法术（`MO_Spell_Fire_ManaDomain` "Mana Domain: Hell"，neta《黑色五叶草》Mana Zone），由代码加在树上 God 法术之上，需最后魔核阶位。
- 效果（`ManaCast/ManaDomain.cs`）：以自身为中心半径 14（随等级成长至 ~43）的领域持续 30 秒跟随法师；域内本系法术**任意位置无视射程/视线施放**、施法时间与冷却 ×0.3、威力 +25%、隐身敌人显形，外加各系专属 2 秒脉冲 "twist"（火系敌人体内阴燃、时间系减速等）。结束后法师法力归零。一天一次（冷却 1000s）。
- **领域对撞**（`ManaCast/DomainClash.cs`，neta《咒术回战》领域战）：两个敌对领域重叠产生边界线，强者（魔核阶位 × 威力 × 等级）以最多 3 格/秒把边界推向弱方，差距 ≤0.6 则僵持；边界抵达某方法师即**击碎其领域**。反魔法领域 "Nowhere" 不推不动——覆盖处其他领域失效，且不可被推回。边界沿双方颜色播放 MO_Clash_Beam，双方地面 Vfx 被裁剪。

### 3. 元素反应（Reactions，neta 原神）—— `ManaCast/Reactions.cs`

- 法术伤害在目标身上留下 8 秒**元素印记** hediff `MO_ElementMark`（记录 nature：Heat/Cold/Wet/Shock/Air/Stone/Life/Light/Dark/Time/Mana）；另一种 nature 的法术再打中带印记者触发反应，同一 pawn 有 2 秒反应 CD。
- 反应表：Melt（热×冷）、Vaporize（热×湿）、Conduct（电×湿）、Overload（电×热）、Superconduct（电×冷，破甲 8 秒）、Freeze（冷×湿）、Swirl（风×任意）、Crystallize（岩×元素，给法师护盾）、Wildfire（生×热）、Bloom（生×湿）、Eclipse（光×暗，致盲 5 秒）、Echo（时间×任意）、Resonance（法力×任意）。
- 自创法术（Spellforge）的链式链接（A>B）在链内直接触发反应且伤害加成内建。

### 4. 敌方法师 AI —— `ManaCast/ManaCastAI.cs`

- NPC 法师首次被需要时按魔核"训练"一次（`TrainAsNpc`）：法术点按 60% 本系 / 40% 纯法力分配到真实树（遵守解锁/费用/等级规则），最多掌握 3 + 阶位/2 个法术（封顶 11），少而精。
- 战斗决策每 1.5 秒思考 + 被敌方施法锁定时立即反应：①治疗重伤友军 → ②接战先用最强 opener，再按阶位加权选攻击，给无 buff 友军补 buff，留蓝防御 → ③无敌人在视野时自 buff/召唤 → ④战后治疗轻伤。避免 AoE 误伤友军；只在有活跃威胁时行动。
- **召唤物**（如水系 BrookFrog→Leviathan、风系 SkyManta 等真实 pawn）由召唤法术生成，`MO_Summoned` hediff 防死亡（被打败即回归元素），跟随护主、可自带法术按间隔施放；mastery 后永久存在，再施放召唤术可送回。
- 征召的我方法师共用同一 AI（仅限 auto-cast 法术）。

### 5. 其他

- 驯服的动物法师按陌生人规则训练但只保留 2–3 个最强法术。
- 法术暴击/反制、未来自己借阶位（LendStages）、死亡魔法用 souls 资源（`Gizmo_Souls`）、暗系 Hollowing 夺取亲和、Anti-Magic 驱散等。

---

## 三、Magic Council（魔法评议会）与 Notoriety（恶名）

- **隐藏派系** `MO_MagicCouncil`（neta 海贼王世界政府）：帝国执法与军事臂，全员法师，跟随帝国参战。三军衔：
  - **Warden**：白制服士兵，stage 4–7，固定 kit（ManaBolt/ForcePush/ManaShield/Dispel/Bind），上限 Expert；
  - **五将军 General**：亲和全树满级含 God 法术（无领域），持 archstaff，combatPower 1500；
  - **三长老 Elder**：满级 + 原始系亲和。
  NPC 法师构型由 `CouncilBuild` modExtension 手工定义。
- **恶名星级**（GTA 式 6 星，`Council/Notoriety.cs`）：常驻信函栏六星显示（LetterDef `MO_CouncilStars`）。每个殖民地法师各持份额；魔核到 stage 6 仅靠存在缓慢涨星（10 天 1 星）；施法、杀评议会/帝国贵族涨星；15 天无新动作掉星。与帝国开战且殖民地有法师时保底 3 星。
- **六级事件**：1 星 Census 人口普查 → 2 星 Fine 罚款（24h 内选择支付或拒绝）→ 3 星 Warrant 通缉抓捕 → 4 星 Kidnap 绑架 → 5 星 TaskForce 特遣队 → 6 星 Edict 讨伐。5 星以下"只抓不杀"（`MO_CouncilSpared` hediff 防一天内死亡），5 星起下死手（可设置调）。

---

## 四、资源 / 建筑 / 打造链

### 1. 法力工艺（研究 `MO_ManaBotany` 600 点中世纪）

链：**Manabloom 魔力花**（8 天作物）→ **Mana Crucible 法力坩埚**结晶成 **mana crystal 法力水晶** → 法师在坩埚**调和（attune）成自己亲和的水晶**（18 个配方，专用 WorkGiver 只允许对应亲和法师做；四元素 45 银、次级 70 银、原始系更贵）→ 水晶用于打造法杖。法力药剂 `MO_ManaDraught` 也在坩埚做。

### 2. 法杖（约 50 种）

分阶：Hedge（乡野，木材+1 水晶，Crafting Spot 可做）→ 进阶 → Named（具名，需对应亲和水晶）→ Arch（将官级）→ 顶级（WorldBranch 等）。给装备者 CastSpeed/CastRange 等加成；NPC 法师出生按魔核阶位自动配发相应法杖 + 法师服 + 药水 + 魔法书（`PostSpawnSetup` 一套）。

### 3. 药水（研究 `MO_ManaAlchemy` 1000 点）

5 种草药（heartmoss/windreed/ironfern/emberroot/starlily）+ manabloom 在坩埚酿造；6 类 × 3 尺寸：Mana（回蓝）/Healing/Swiftness/Stoneskin/Vigor/Arcana，强度 = 尺寸 × 品质，酿造者 Intellectual 决定品质。

### 4. 法术锻造（Spellforge）

不是建筑，是从技能树窗口打开的 UI（`UI/Window_Spellforge.cs`）：法师自创法术（1–3 个链接 link，每链选亲和 + 10 种形态：Bolt/Burst/Beam/Cone/Wall/Zone/Rain/Strike/Dash/Aura + 外观借用现有法术 + focus/dial/rider 参数），选阶位、速度、三段式命名；"铭刻"后写入法术书（`GameComponent_Spellforge`，加载时由 `SpellComposer` 动态生成 `MO_Forged_<id>` SpellDef），花对应阶位法术点学会。链内 A>B 触发元素反应。

### 5. 装备 / 神器

- **法师装备**（研究线 ManaWeaving→ManaForging→ArcaneResonance）：多套主题套装分别强化施速/冷却/射程/蓝池/回复；评议会制服可被帝国低级贵族头衔承认。
- **神器 Artefacts**（约 60 件武器 + 饰品）：`CompArtefact`——**非法师也能用神器施放固定法术**，按神器自身等级/威力/伤害施放，消耗充能不耗蓝；充能缓慢自回或用法力水晶装填（专属 `MO_ReloadArtefact` job）。神话级每次施放索取代价（伤口/heddiff/眩晕/寿命年数）。特色：`MO_Artefact_BlackStandard` 黑旗自带 Shadow Keep 储存最多 N 名亡灵士兵，随武器易主传递。异域商人售卖，NPC 偶尔持神器。

### 6. 基因（Biotech）

`MO_Gene_MageBorn` + 每亲和一个基因（互斥 tag），携带者必为法师、亲和可跨槽叠加、可遗传。

---

## 五、Patches（对原版的改动）

| Patch | 内容 |
|---|---|
| ManaLink.xml | PsychicAmplifier(psylink) hediff 加 comp，心灵链接推动魔核 |
| ManaSensitivity.xml | PsychicSensitivity 加 StatPart（法师天赋） |
| MechMagic.xml | Biotech MechBandwidth/MechControlGroups 加 StatPart（金属系机控师法师增益） |
| MagicCouncil.xml | 评议会入帝国盟友列表；制服计入皇家头衔服装许可 |
| MagicTraits.xml | 所有场景加 ScenPart（开局法师特质）；特质体系镜像 |
| MageValue.xml | MarketValue 加 StatPart——法师按阶位/最强法术/亲和星级更值钱 |
| Grimoires.xml | MarketValue 加 StatPart（书值其法术）；商人库存/任务奖励加魔法书 |
| ManaCrystalTraders / PotionTraders / Artefacts.xml | 水晶/药水/神器进商人库存 |
| BurningPawns.xml | 改 BurningResponse 思考树 |
| HideBody.xml | RenderSkipFlag 扩展（法术隐形） |
| StormRod.xml | 雷暴闪电打击 eventClass 替换（Storm Rod 引雷） |
| Variants.xml | 变异亲和法术补丁 |

---

## 六、源码结构（`Source/AManaOdyssey/`）

- **Core/**：CompManaCore、MagicTree/MagicTreeDef、CoreStage、AffinityBook、ManaLink、MagicGenes、MagicStats、MagicTrait。
- **Spells/**：SpellDef（steps 视觉 + cast 逻辑）、SpellCaster（执行器）。
- **ManaCast/**：施法运行时——SpellRun/SpellCast/SpellProjectile、SpellClash、DomainClash、Reactions、ManaDomain、ManaCastAI、Summons、DeathMagic、AntiMagic、MechMagic、SpellEffects（几十种）、LegendaryMages、CastingSpeech。
- **Forge/**：Spellforge/SpellComposer/GameComponent_Spellforge。
- **Grimoires/**：CompGrimoire。**Council/**：MagicCouncil/Notoriety/CouncilEvents。**Crafting/**：ManaCrafting/Potions/StaffArming/MageDress。
- **Artefacts/**：CompArtefact/来源。**Variants/**：AffinityVariantDef + 自定义 Patch 操作。
- **UI/**：ITab_ManaCore、Window_MagicTree、Window_Grimoire、Window_Spellforge、Window_CouncilLetter、Window_ShadowKeep、Gizmo_ManaBar/Gizmo_Souls、自研 BitmapFont。

---

## 七、Vfx 特效引擎（对 MMT 最有参考价值）

`Source/AManaOdyssey/Vfx/` + `Defs/Vfx/`：

- **数据驱动的"After Effects 式"时间轴特效**：每个特效是 `VfxDef`（XML：duration/loop 区间/镜头震动/地面层 + layers 图层列表）。Def 几乎不含逻辑，layer XML 原样交给求值核心（`VfxComp.cs`、`VfxTrack.cs`、`VfxEvaluator.cs`、`VfxEase.cs`）解析。
- **双引擎同源**：C# 求值核心与外部设计工具 VfxForge 的 JavaScript 核心"逐行相同"（`VfxJs.cs` 抹平 JS 与 .NET 差异），游戏内播放与网页编辑器预览完全一致，可 scrub 预览。
- **确定性求值**：`VfxInstance` 不存动画状态，只存 (def, seed, pos, startTick)，每帧从时间纯函数算出画面——帧率无关、暂停/变速安全；支持 attach 跟随、站立朝向（施法阵立在身前）、双色重染（对波按双方法术颜色着色）、地面裁剪（领域对撞边界）、相机震动、音效 cue、暂停中播放（施法圈）。
- **法术即分镜**：SpellDef 的 `<steps>` 是声明式分镜（锚点 Caster/Target/Sky、飞行/曲线/散射/流式发射/跟随/站立/前伸/扩散、after/delay/mark 时刻编排）；`SpellCaster` 把视觉步骤的"时刻"绑定到游戏效果（`<effects><li on="bolt">`）——**视觉与判定严格同步**，这是弹道对波、命中判定、特效表现三者一致的关键。
- 弹道用自研 SpellProjectile（每 tick 记录起止线段）即可支持对撞检测；Beam 用"每秒伤害"抽象纳入同一对抗体系。

---

## 八、总体评价

- 系统设计高度自洽：**敌我同规则**（NPC 点数花进真实树）、**成长与威胁联动**（阶位动态封顶）、**视觉/判定同源**（steps 时刻绑定效果）是三大设计支柱。
- 施放体系完全绕开原版 Ability/verb 自研，代价是要自己维护 JobDriver/gizmo/AI 全链路，换来对波/反应/领域这类需要逐 tick 弹道控制的机制自由。
- 授权状态：作者 Jelly Creative，未见开源许可证声明——**代码与美术资源不能直接克隆**，机制思想可参考重写（详见 `mmt-mana-fusion-plan.md`）。
