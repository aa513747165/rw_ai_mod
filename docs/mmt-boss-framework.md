# MMT Boss 框架（魔物派系 + 全类型技能链路）

> 手工维护的设计/模板文档（非脚本生成物）。定义 MMT 自产 Boss 与敌对召唤物的统一做法：
> 隐藏魔物派系、四型 Boss 配方、技能链路、DEV 测试工具。
> 副本巢位接入与派系路由的语境见 `mmt-dungeon-design.md` §4.7；火元素为首个完整实例。

## 1. MMT_Monster 魔物派系

`1.7/Defs/FactionDefs/MMT_FactionDefs_Monster.xml` 定义隐藏派系 `MMT_Monster`（label 魔物）：

- **`permanentEnemy=true`**（Core 机械族同款）：只对玩家永久敌对，与其它 NPC 派系（副本守卫宿主、
  虫巢、机械族、实体）天然中立——Boss 在巢位不与守卫互殴，也不入守卫 lord。
- `hidden=true`、`displayInFactionSelection=false`、`humanlikeFaction=false`、`techLevel=Animal`、
  `autoFlee=false`、`canUseAvoidGrid=false`。
- 永不主动袭击：`raidCommonalityFromPointsCurve` 单点 `(0, 0)`；`pawnGroupMakers` 空列表
  （Inherit="False"）。
- `requiredCountAtGameStart=1`：世界生成必有 1 个，保证 `FirstFactionOfDef` 恒定可取。
- 玩家召唤物不受影响：召唤归属走 `caster.Faction`（见 §4），与该派系无关。

## 2. 单位归派系：XML 标记路由

- `Source/MyModeTest/MonsterFaction.cs`：
  - `DefModExtension_MMTMonsterFaction`：空标记类，PawnKindDef 挂
    `<modExtensions><li Class="MyModeTest.DefModExtension_MMTMonsterFaction" /></modExtensions>`
    即声明"我是魔物"。其它 mod 可 XPath 复用。
  - `MMTMonsterFaction.Get`：`FirstFactionOfDef("MMT_Monster")` 惰性缓存，失败返回 null。
- `BossPool.ResolveBossFaction`（BossPool.cs）**第一优先分支**：带标记 → 魔物派系；
  取不到静默回退原路由（类人→宿主派系 / 机械→机械族 / 虫→虫巢 / 实体→实体 / 其余→null 兜底狂暴）。
  原版/Mod Boss（无标记）行为完全不变。

## 3. 四型 Boss 统一配方（全部 = Pawn，不做建筑路径）

| Boss 型 | 种族做法 | 派系 | 思维树 |
|---|---|---|---|
| 元素/野兽 | Animal 风格种族 + `renderTree=Animal`（火元素已验证） | 魔物派系（modExtension） | 自定义 thinkTreeMain |
| 类人 | 类人身体种族（Ghoul 已证明类人可自定义思维树+技能） | 魔物派系（modExtension） | 自定义 thinkTreeMain |
| 机械 | 机械种族（`IsMechanoid`） | 魔物派系（modExtension） | 自定义 thinkTreeMain |
| 建筑类 | **移速 0 的 Pawn 种族**（`MoveSpeed=0`，大 drawSize；当"不会移动的生物"处理） | 魔物派系（modExtension） | 自定义 thinkTreeMain（去掉游走/追击，只留技能+近战反应） |

落地一个 Boss 的清单（照火元素四件套抄）：

1. `ThingDefs_Races`：种族（body/renderTree/thinkTreeMain/tools/statBases）。
2. `PawnKindDefs`：combatPower/isFighter/abilities/lifeStages 贴图 + **modExtensions 魔物标记**。
3. `AbilityDefs`：技能（§4 配方）。
4. `ThinkTreeDefs`：thinkRoot 优先级 = 脱战/倒地/lord 子树 → 每技能一个
   `JobGiver_AIAbilityFight`（半径 65/72）→ 近战反应 → `JobGiver_AIFightEnemies` → 游走。
   节点按序尝试、CD 中跳过，技能排前面的优先放。

建筑类 Boss 特记：`MoveSpeed=0` 即可（原版野生 Healroot 式站桩 Pawn 无需特殊处理）；
ThinkTree 去掉 `JobGiver_WanderAnywhere` 与追击节点，只留 `JobGiver_AIAbilityFight` +
`JobGiver_ReactToCloseMeleeThreat`；drawSize 做大、贴图可带朝向。

## 4. 技能链路（Boss 与召唤物通用配方）

```
AbilityDef（纯 CD）→ PawnKindDef.abilities（1.7 原生授予）→ ThinkTree JobGiver_AIAbilityFight（AI 施放）
```

- **AbilityDef**：不挂 PsycastBase 父类；`showPsycastEffects=false`；`aiCanUse=true` +
  `ai_IsOffensive=true`；节奏全靠 `cooldownTicksRange`（毫秒 tick，600=10 秒）。
  效果 comps 一律走 **SkillFX 技能特效框架** pawn 侧桶（参数全部 XML 透传，视觉/数值不进 C#）：
  - 范围天降打击：`CompProperties_MMTStrike`（SkillStrike.cs，strikeCount/strikeIntervalTicks/
    areaRadius/damage/chanceToStartFire；原版闪电视觉+自定义爆炸伤害层）——Boss「流星火雨」即实例
  - 地面火柱/火海：`CompProperties_MMTGroundFire`（SkillGroundFire.cs，单柱 effectRadius/
    火海 areaRadius、duration/damage/armorPenetration/点燃概率、火柱视觉参数全 XML）——
    Boss「熔岩火柱」即实例
  - 生物召唤：`CompProperties_MMTSkillSummon`（mode=Animal，animalKindDefName/count/
    durationSeconds/vanishFleckDefName；**动物/元素等非人形 PawnKind 均可**，生成派系 =
    `caster.Faction`——魔物派系 Boss 召的小怪天然魔物派系，玩家技能召唤归玩家）——
    Boss「召唤火元素」即实例；限时到期走 `MMT_SummonAnimal` Hediff 计时消散
  - 单发/浮游投射物：`CompProperties_MMTSingleShot` / `CompProperties_MMTFunnelShot`
    （SkillShooting.cs）——小火元素 `MMT_SummonFire_FlameBolt` 即实例
  - 其余桶（射线 MMTBeam/吐息 MMTBreath/新星 MMTNova/连锁闪电 MMTChainLightning/
    击退 MMTKnockback/位移 MMTDisplace 等）同法可用，按 Boss 设计挑选
- **PawnKindDef.abilities**：`<abilities><li>技能 defName</li></abilities>`（Anomaly 实体同款）。
- **ThinkTree**：每个技能一个 `JobGiver_AIAbilityFight`（`<ability>` 指 defName，
  `targetAcquireRadius`/`targetKeepRadius` 65/72），置于近战反应节点之前。
- **Boss 当玩家召唤物用**：同一 PawnKindDef 敌对生成（魔物派系）与归属玩家（caster.Faction）
  都成立；技能由同一 ThinkTree 驱动。类人（ToolUser）归玩家后的自动施放表现列入实机验证项，
  备选方案为 Animal 智力变体或自写通用 JobGiver。
- 限时召唤物到期移除走 `MMT_SummonAnimal` Hediff（SkillFX 生物召唤）或
  `MMT_SummonTimer`（旧机制，仍在役）。

## 5. DEV 测试工具

- **Boss 召唤器** `MMT_BossSummoner`（Dev 模式建造菜单）：gizmo 列 Boss 池 → 选格生成，
  派系走 ResolveBossFaction（魔物派系 Boss 挂 `LordJob_AssaultColony` 主动进攻）。
- **操控放技能**（`DevBossControl`，BossSummoner.cs）：Debug 菜单
  「MMT Boss → 操控放技能（选中有技能的单位）」对地图上任一带技能单位使用：
  FloatMenu 列全部技能（含剩余冷却秒数）→ 按 `ability.verb.targetParams` 进原版瞄准 →
  `QueueCastingJob` 排入施法 job；无目标参数的技能直接 `Activate`。
  不打断派系/lord，施放完由原 ThinkTree 接管。Boss、召唤物、玩家小人通用。

## 6. 校验与实例索引

- 校验套件：`tools/validate/monster_faction.py`（派系字段/标记挂载/召唤物技能链路/C# 接线）；
  火元素四件套细节另由 `tools/validate/fireboss.py` 覆盖。
- 首个完整实例（元素型）：熔火元素领主 + 小火元素
  （`MMT_Races_FireElemental.xml` / `MMT_PawnKinds_FireElemental.xml` /
  `MMT_Abilities_FireBoss.xml` / `MMT_ThinkTrees_FireElemental.xml`）。
