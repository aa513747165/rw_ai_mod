# MMT × MANA ODYSSEY 机制融合方案（克隆 vs 补丁决策）

> 前置调研：`docs/mmt-mana-odyssey-research.md`。
> 本文档回答：Mana Odyssey（下称 MO）的哪些机制融入 MMT 的职业系统（`Progression/PawnClass.cs`，37 职业 10 家族）与历练系统（`Progression/PawnProgress.cs`，60 级 XP + 天赋点），以及每个机制采用**克隆（重实现）**还是**补丁**路线。

---

## 一、路线决策：为什么"机制重实现"而不是克隆代码或依赖补丁

### 三条候选路线

| 路线 | 做法 | 结论 |
|---|---|---|
| A. 依赖补丁 | MMT 的 About 声明依赖 MO，用 XPath patch 改 MO 的 Def、Harmony patch 挂 MO 的类，把 MO 系统桥接进 MMT | **不推荐** |
| B. 克隆代码 | 把 MO 的 C# 源码/Def/贴图整批搬入 MMT（改名 MMT_ 前缀） | **不推荐** |
| C. 机制重实现 | 借鉴 MO 的机制设计，用 MMT 现有支柱（历练/天赋树/职业/SkillFX/原生 Ability）原生实现 | **推荐主路线** |

### 否决 A（依赖补丁）的理由

1. **破坏 MMT 独立性**：MMT 是独立 mod（About.xml 仅依赖 Harmony + 全 DLC）。依赖 MO 后 MO 每次更新都可能崩桥接补丁，且强迫玩家订阅两个 mod。
2. **体系不兼容**：MO 的法术是**自研 SpellDef 体系**（非原版 Ability），自带 SpellCaster/SpellRun/JobDriver/gizmo/AI 全链路；MMT 的技能全部是**原生 AbilityDef + SkillFX Comp**。桥接等于在 MMT 里同时维护两套施法框架，对波/反应这类机制还要穿透对方黑盒取运行时状态，Harmony 反向补丁极其脆弱。
3. XML 补丁只能改 Def 数值，**触达不到 MO 的机制核心**（对波、反应、领域、AI 全在 C# 里）——补丁路线能拿到的只有贴图和数值表，恰恰是价值最低的部分。

### 否决 B（克隆代码）的理由

1. **授权风险**：HSK 资源已获授权，但 MO 作者 Jelly Creative 未声明任何开源许可证，代码与美术资源（Vfx 素材/贴图/UI）默认受版权保护，**整批克隆不可行**。机制思想本身不受版权保护，重写实现无风险。
2. **克隆过来即成孤岛**：MO 的 Vfx 求值器、SpellDef steps 体系与 MMT 的 SkillFX 28 步框架功能重复，两套并行只会增加维护成本。
3. MO 的施法运行时（自研 JobDriver/Command_Spell/法力条）搬进来会与 MMT 的原生 Ability 路线冲突（弃用谁都是大返工）。

### C 路线的核心映射：MO 机制在 MMT 里已有 70% 等价物

| MO 概念 | MMT 等价物 | 状态 |
|---|---|---|
| 魔核（Mana Core）comp | `CompPawnProgress`（历练）+ 职业 Hediff | ✅ 已有，不重做 |
| 18 种亲和 | 10 职业家族：elemental（火/冰/雷/地占/气象≈Fire/Ice/Lightning/Earth/Wind+Cloud）、arcane（奥术/附魔/时空/技法/流浪≈Star/Time/AntiMagic/Explosion/Metal）、necro（死灵/血/恶魔/附身≈Death/Blood/Dark）、holy≈Light、nature≈Wood、Wanderer/Arcanist≈纯法力 | ✅ 已覆盖，亲和即家族 |
| 魔核 18 阶位（6 Band × 3 Phase） | 职业 III 阶（I/II/III，历练 5/20/35 解锁） | ✅ 已有（简化版） |
| 冥想精炼成长 | 历练 XP（`TalentXPPatches` 全源钩子） | 🔧 加冥想 XP 钩子即可（见 P0） |
| 法术点 | 天赋点（每级 +1） | ✅ 已有 |
| 亲和树（支路/7 阶/mastery 5 级） | `MMT_TalentTreeDef` 家族树（生成器产出） | ✅ 已有 |
| 法术施放（Command_Spell + MO_ManaCast job） | 原生 AbilityDef + SkillFX（28 步特效框架） | ✅ 已有 |
| 法力池/回复 Stat | 原生 Ability 冷却/充能 + MMT Stat | ✅ 近似已有 |
| 法术对波 / 元素反应 / 领域 / 敌法师 AI / 评议会 | — | ❌ 缺失，**融合重点** |
| 魔法书 / 神器（非法师施法）/ 召唤物防死 | — | ❌ 缺失，低成本可补 |

结论：MO 的**成长框架**（魔核/亲和/树/法术点）MMT 已用历练+职业+天赋树等价实现，不需要任何移植；真正值得融合的是**战斗层机制**（对波、元素反应、领域）和**若干低成本玩法点**（魔法书、神器、召唤物防死）。

---

## 二、融合清单（按优先级分阶段）

### P0：低成本高价值（机制重实现，纯 C# 小件 + XML）

1. **冥想给历练 XP**（借鉴 MO 冥想精炼）
   - 在 `Progression/TalentXPPatches.cs` 增加钩子：冥想 job（`JobGiver_Meditation`/meditate toil）按 tick 给 `CompPawnProgress.AddXp`，灵能链接等级提供倍率。
   - 效果：冥想成为法师系职业的另类成长轴，对应 MO"冥想精炼魔核"。
   - 工作量：~50 行 + 校验套件。

2. **技能书（Grimoire 等价物）**
   - 新 ThingDef `MMT_SkillTome`：Comp 存一个 AbilityDef + 家族限定；右键"研读"→ 已就职对应家族职业的小人 `GainAbility`（已会则转化为天赋点 +1）。
   - 进商队/轨道商人库存 + 任务奖励（XPath patch，同 MO 的 Patches/Grimoires.xml 思路）。
   - 校验：书名→AbilityDef 引用完整性。

3. **召唤物防死回归**（借鉴 `MO_Summoned`）
   - `SkillFX/SkillSummon.cs` 召唤的 pawn 挂 `MMT_Summoned` hediff（`CompDisappears` 思路已有先例）：被打倒时不死、倒地即消散回归（播放消散特效），mastery 型节点（天赋树 Keystone）可改为永久存在。
   - 工作量：小。

4. **法器装备（Artefact 等价物）**
   - MMT 已有装备强化体系（`Rpg/`），新增思路：装备 modExtension 挂 AbilityDef，穿戴即授予（脱下移除），**无职业小人也能用**——对应 MO"神器让非法师施法"。消耗走装备充能（`Production/Rechargeable.cs` 已有充能 Comp 可复用）。
   - 神话级可加"施放代价"（heddiff/伤口）。

### P1：战斗层核心机制（融合重点，全部走 SkillFX 扩展 + 新套件校验）

5. **元素印记与反应表**（借鉴 `Reactions.cs`，neta 原神）
   - 新 Hediff `MMT_ElementMark`（8 秒，存 nature 枚举：热/冷/湿/电/风/岩/生/光/暗/时/法）。
   - SkillFX 各 effect（SkillShooting/SkillBeam/SkillGroundFire/SkillNova/SkillChainLightning 等）命中时按技能的 nature 挂印记；命中带异种印记的目标触发反应：
     Melt 热×冷 / Vaporize 热×湿 / Conduct 电×湿 / Overload 电×热 / Superconduct 电×冷（破甲）/ Freeze 冷×湿 / Swirl 风×任意 / Crystallize 岩×元素（护盾）/ Wildfire 生×热 / Bloom 生×湿 / Eclipse 光×暗（致盲）/ Echo 时间×任意 / Resonance 法力×任意。
   - nature 标签走 **Def modExtension**（XML 声明，C# 零硬编码，符合 SkillFX 约定）；反应表放 `1.7/Defs/Misc/MMT_ElementReactions.xml` 数据驱动。
   - 价值：**跨职业联动**——火法师 + 冰法师组队触发 Melt，让 10 家族的组队深度落地。
   - 工作量：中（印记 comp + 反应分发器 + 各 SkillFX effect 挂印记点）。

6. **法术对波（弹道级）**（借鉴 `SpellClash.cs`）
   - 新建运行时注册表：SkillFX 投射物（SkillShooting/SkillStrike/SkillChainLightning/SkillSummon 追踪弹）每 tick 登记**起止线段**（与 MO 同思路），逐 tick 检测敌对线段相交（1 格内）→ 弱者 ≥ 强者 × 系数则同归于尽，否则强者贯穿、弱者破碎；对撞点播放双色特效（`SkillSideFlat` 平躺序列帧正适用）。
   - **Beam 对波推挤**：`SkillBeam.cs` 持续射线按每秒伤害纳入同一体系；敌对 beam 正面相遇时推挤点按强度差向弱方 caster 移动，到达即击碎其 beam、对方贯穿。
   - 设置开关（MMTSettings 加项）。
   - 工作量：中偏大（注册表 + 相交检测 + beam 推挤状态机）；是 MMT 技能演出从"单方面表演"升级到"对抗博弈"的关键一步。

7. **领域（Mana Domain 等价物）**（借鉴 `ManaDomain.cs`）
   - 基于 `SkillAura`（跟随光环）+ `SkillGroundZone`（持续地面区域）扩展新 effect `SkillDomain`：跟随施法者的领域（半径随等级成长），域内本家族技能冷却/威力强化（StatPart 注入，规避 JIT 内联坑），附各家族专属 twist 脉冲（火=敌人体内阴燃、时间=减速等）；结束后清空充能、长冷却。
   - 投放位置：各家族树 Keystone 节点 + 职业 III 阶限定（对应 MO 的 God 阶 + 最终魔核阶位）。
   - **领域对撞**（借鉴 `DomainClash.cs`）：两敌对领域重叠产生边界线，强者按（职业阶位 × 等级）推边界（上限 3 格/秒），边界到法师即击碎其领域；反魔法领域对应 Wanderer/Arcanist 家族做"静默领域"（覆盖处其他领域失效）。
   - 工作量：大；建议拆两期（先做领域本体，对撞二期）。

### P2：敌我同规则

8. **NPC 职业训练**（借鉴 `TrainAsNpc`，敌法师 AI）
   - MMT 袭击系统（`Raid/`）生成敌对法师 pawn 时：按职业家族树**自动点天赋**（点数 60% 本系 / 40% 通用，遵守解锁规则），掌握 3 + 阶位/2 个技能（封顶 ~11），少而精。
   - 施法决策可复用原生 Ability 的 AI（原版 NPC 已会用 Ability），只需保证 NPC 拿到技能；深度决策（治疗→opener→补 buff→留蓝）一期不做，用原生 AI 兜底。
   - 价值：袭击出现"会点天赋树的真实法师"，敌我同规则。

### P3：内容层（大工作量，v2 之后）

9. **隐修会（Council 等价物）**：隐藏法师派系 + GTA 式恶名星级信函 + 六级事件链（普查→罚款→通缉→绑架→特遣队→讨伐）。可挂在现有 Quests 框架（公会委托/据点已有先例）。
   - 5 星以下"只抓不杀"用 hediff 保底——与 MMT 的 Boss/俘虏玩法契合。
   - 优先级低，等 P1/P2 落地后视反馈再开。

### 明确不做

- **Spellforge 自创法术**：UI 工作量巨大，与 MMT"Def 数据驱动 + 生成器"的维护模式冲突。
- **MO 的 Vfx 时间轴引擎**：MMT SkillFX 已走"XML 参数化 + C# 求值"路线（同一设计哲学），无需移植；若未来取得 MO 作者授权，可评估克隆其双引擎同源（C#/JS）编辑器思路，当前不做。
- **魔核/亲和槽/变异亲和**：与 MMT 职业家族模型重复，不引入第二套成长框架。

---

## 三、落地约束（遵循仓库约定）

1. defName 一律 `MMT_` 前缀；反应表/印记/领域参数全部 XML 数据驱动（SkillFX 视觉参数禁止 C# 硬编码）。
2. 每阶段配 `tools/validate/<名>.py` 新套件（如 `element_reactions.py`、`spell_clash.py`），由 `validate_new.py` 聚合；涉及旧链交叉校验才动旧文件。
3. 机制参考 MO 时**只抄设计不抄代码**；注释里标注出处（如"对波推挤参考 Mana Odyssey SpellClash.cs 思路"）。
4. 若涉及 MO 美术风格参考（对波双色特效等），贴图按 `.claude/skills/rw-texture-design/SKILL.md` 自制，不使用 MO 素材。
5. 提交信息中文 conventional commits，按阶段分 commit。

## 四、推荐启动顺序

P0（约 1 个迭代：冥想 XP + 技能书 + 召唤防死 + 法器）→ P1-5 元素反应（联动价值最高）→ P1-6 对波 → P1-7 领域 → P2 NPC 训练 → P3 视情况。
