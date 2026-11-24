# MMT 副本/据点生成设计方案

> 手工维护的设计文档（非脚本生成物）。副本/据点生成专项设计：难度分层、敌人类型、物品奖励、地图建筑。
> 状态：**已实现**（v1：三档扫描/派系守卫/分档奖励池/程序化布局/设置与校验；任务链三幕已实现（`mmt-rpg-design.md` §3.3）；
> v2：Boss 改为池化选取——原版/Mod 全库 Boss 按财富预算抽选、可多只，见 §4.7）。
> 本文是 `mmt-rpg-design.md` §3（支柱三）的展开与细化，遵循其 §0 硬性规则；稀有度掷点 `Roll(tier)`、Boss 掉落表、任务链三幕等以 rpg 文档为准，本文不重复定义。

## 0. 跨 Mod 兼容总原则（全文最高优先级）

副本/据点的所有内容必须对其它 mod 开放且安全：

1. **清单一律走 XML**：敌人派系、奖励物品、布局建筑的清单一律定义在 XML（DefModExtension / Def 引用），C# 零硬编码 defName；其它 mod 可用 XPath 补丁向清单追加自己的内容，MMT 代码无需感知其存在。
2. **惰性解析 + 静默回退**：所有跨程序集/可能缺失的 Def 引用用 `GetNamedSilentFail` 式惰性解析，解析失败静默降级（先例：`Exploration.cs` 的 `MechClusterPart`）。
3. **绝不自写判敌/生成逻辑**：敌意、lord、派系装备全走原版机制；守卫经宿主派系 `PawnGroupMaker` 产出，mod 单位随派系自动出现。
4. **性能**：所有生成只发生在据点地图 `PostMapGenerate` 瞬间，无新增 per-tick 全图扫描。
5. **存档兼容**：新字段带默认值，关开关不删既有数据；defName 加 `MMT_` 前缀，label/description 中文写死。

---

## 1. 难度分层（扫描时选档）

### 1.1 三档难度

深空扫描仪 gizmo 从单一按钮改为 FloatMenu 选档，玩家主动选择挑战强度：

| 档位 | label | 声纳模块 | 扫描天数系数 | 档位威胁系数 | 奖励稀有度地板 |
|---|---|---|---|---|---|
| I | 浅层扫描 | ×1 | ×1.0 | ×1.0 | 精良 ~ 稀有 |
| II | 深层扫描 | ×2 | ×1.5 | ×1.6 | 稀有 ~ 史诗 |
| III | 全域扫描 | ×3 | ×2.0 | ×2.5 | 史诗 ~ 传说 |

- 蓄电规则不变（充满后一次清空）；声纳模块数与扫描天数系数写入 `CompProperties_DeepSpaceScanner` 的 XML 字段（如 `tierCosts` 列表），C# 只读表。
- **Boss 巢穴（`MMT_SitePart_BossLair`）低概率进扫描随机池**（权重 4/104 ≈ 3.8%，类型系数 ×2.0，档位系数正常乘算），受 `bossEncountersEnabled` 设置门控（`requiresBossEncounters` 条目字段）；Dev 模式扫描仪 gizmo 仍可直接生成验收。

### 1.2 威胁点公式

```
threatPoints = clamp(基地财富 / 100, 800, 5000) × 据点类型系数 × 档位系数
```

- 据点类型系数沿用 rpg 文档 §3.1：试炼场 ×1.2 / 职业圣殿 ×1.4 / Boss 巢穴 ×2.0（既有三类探索据点 ×0.5/×1/×1.6 不变，视为零档资源点）。
- 财富缩放走原版 `wealthWatcher` 机制：修改财富计算或袭击强度的 mod 自动生效，无需特例适配。
- 选档即玩家对"威胁 ↑ ⇄ 奖励 ↑"的主动权衡，符合 rpg 文档 §0"正面/主动触发型默认开"的规则。

### 1.3 档位与奖励稀有度的对接

档位决定装备箱 `Roll(tier)` 的稀有度地板（rpg 文档 §1.1/§1.3）：I 档保底精良、II 档保底稀有、III 档保底史诗，传说仅在 III 档与 Boss 掉落中出现。

---

## 2. 敌人类型（兼容 mod）

### 2.1 派系选取（守卫宿主）

1. 从 `Find.FactionManager` 枚举满足以下条件的派系：对玩家敌对 + 拥有可用 `PawnGroupMaker`（存在 Commonality > 0 的战斗组）；
2. 经设置项 `dungeonFactionBlacklist`（defName 逗号列表，默认空）过滤；
3. 按派系出现权重随机选定据点宿主派系。

其它 mod 注册的敌对派系天然进入枚举，mod 单位经其自带 PawnGroupMaker 自动出现，MMT 不做任何 mod 感知代码。

### 2.2 威胁预算拆分

| 用途 | 预算占比 | 生成方式 |
|---|---|---|
| 守卫小队 | ~70% | 宿主派系 `PawnGroupMaker` 按威胁点产出（mod 单位自动在列） |
| 防御工事 | ~30% | §4 布局生成的炮塔/陷阱/掩体（挂宿主派系，敌意走原版） |

Boss 巢穴护卫经 `PawnGroupMakerParms` 传宿主派系抽组；Boss 本体走 §4.7 池化选取（原版/Mod 全库 + XML 白名单，按财富预算，可多只），同样不做 mod 感知代码。

### 2.3 回退链（全程静默，不报错不红字）

```
黑名单过滤后的敌对派系枚举
  → 为空或 PawnGroupMaker 生成失败：原版 MechCluster（先例：Exploration.cs）
    → MechClusterPart 缺失（无 Royalty）：无守卫纯奖励点
```

机械族/虫族等无标准 PawnGroupMaker 的派系走其原版生成器，不做特例硬编码。

### 2.4 与稀有度系统的联动

守卫装备经原版 `PawnWeaponGenerator.TryGenerateWeaponFor` 生成，rpg 文档 §1.3 的稀有度掷点 postfix 自动生效——**mod 派系的 mod 武器同样掷出随机词条**（装备 comp 经 XPath 补丁挂全体武器/防具）。击杀守卫的掉落因此成为副本的第二条奖励线（见 §3.4），跨 mod 一致性闭环。

---

## 3. 物品奖励

### 3.1 档位奖励池（XML 驱动）

奖励从现有静态资源堆升级为**按档位组织的奖励池**：池子全部在 XML 定义（扩展 `StockpileRewardExtension`，每条目加 `minTier` 字段表示出现的最低档位），C# 只按据点档位选池 + 掷数量，不持有任何物品清单。其它 mod 可 XPath 往奖励池追加自己的物品条目。

### 3.2 各档奖励构成（定稿）

| 档位 | 装备箱 | MMT 自产材料 | 消耗品 | 凭证类 |
|---|---|---|---|---|
| I | 精良~稀有随机词条装备 ×1 | 远古合金 ×10~15 | 历练书（+500 XP）×1 | — |
| II | 稀有~史诗随机词条装备 ×1~2 | 远古合金 ×8~12 + 远古碎晶 ×4~6 | 历练书 ×1~2 | 职业凭证碎片 ×1~2（职业圣殿必出） |
| III | 史诗~传说随机词条装备 ×2 | 远古碎晶 ×6~10 + 远古合金 ×5~8 | 历练书 ×2 | 职业凭证碎片 ×2~3（职业圣殿必出） |

- 装备箱武器从玩家科技等级可用的 weaponTags 池抽（沿用 `PawnWeaponGenerator` 技术等级过滤思路），防具同理；稀有度按 §1.3 地板掷点。
- Boss 巢穴高档奖励由档位奖励池承担（III 档装备箱 ×2 + 远古碎晶/凭证碎片，见各 SitePartDef XML）；任务链第三幕的空投奖励（传说装备/凭证/数据核心）为任务奖励，与巢穴战利品叠加（rpg 文档 §3.3）。
- 职业凭证碎片合成配方（3 碎片 → 1 凭证，工作台配方）以 rpg 文档 §3.1 为准。

### 3.3 难度收益对等

奖励堆总市场价值 ≈ `threatPoints × 价值系数`（初始定稿 ×1.2，实现期可只调常量）。档位越高威胁越大、奖励价值越大，防止"低难度刷高级奖励"或"高难度无收益"。

### 3.4 第二条奖励线：守卫掉落

击杀守卫按袭击者规则正常掉装备（§2.4 联动稀有度掷点），工事被拆/占领后可回收材料。奖励设计不把全部价值压在奖励堆上，鼓励清图而非偷鸡。

---

## 4. 地图建筑（自定义 worker 程序化布局）

### 4.1 `SitePartWorker_MMT_Dungeon`

新增 worker（替换/包裹现有 `SitePartWorker_MMT_Stockpile` 模式），在 `PostMapGenerate` 程序化生成布局：

```
┌─────────────────────────┐
│  外墙（围合，1~2 处入口）  │
│  ┌─────┐   ┌────────┐  │
│  │奖励库房│   │守卫哨位 ×N│  │
│  │(奖励堆)│   │(沙袋/炮塔)│  │
│  └─────┘   └────────┘  │
│        内部道路/照明      │
└─────────────────────────┘
```

布局要素：

- **外墙**：围合据点核心区，留 1~2 处入口（门口配守卫哨位）；
- **奖励库房**：独立小房间（墙+门），内部生成 §3 奖励堆；
- **守卫哨位**：沙袋/掩体 + 可选炮塔，数量与位置按威胁预算（§2.2 工事部分）；
- **照明/杂项**：火把或落地灯点缀，提升据点可读性；
- **Boss 巢穴**：同一 worker + 更大型布局参数 + 中央 Boss 巢位（开阔区，Boss 初始休眠/巡逻由 lord 机制决定）。

### 4.2 布局参数全部 XML 化

布局参数写在各 SitePartDef 的 `DungeonLayoutExtension`（DefModExtension）里：

| 参数 | 说明 |
|---|---|
| `wallDef` / `wallStuffDef` | 墙体与材质（默认原版 `Wall` + 石材） |
| `doorDef` | 门（默认原版 `Door`） |
| `coverDefs` | 掩体列表（默认 `Sandbags`，可多项加权） |
| `turretDefs` | 炮塔白名单（兜底/人工追加用，默认空；优先级低于 §4.4 派系自适应扫描） |
| `lightDefs` | 照明列表（默认 `TorchLamp`） |
| `wallRadiusRange` / `vaultSizeRange` / `postCountRange` | 布局尺寸区间 |

所有 def 引用惰性解析、缺失静默跳过（该条目不入生成，不报错）。默认清单**只用原版 + MMT def，不预置任何第三方 mod 引用**；其它 mod 可 XPath 往 `coverDefs`/`turretDefs`/`lightDefs` 追加自己的建筑。宿主为 mod 派系时，墙体材质与炮塔由 §4.4 派系自适应机制优先推导，本表仅兜底。

### 4.3 派系与敌意

防御建筑（炮塔/门）生成时 `SetFaction` 为据点宿主派系，敌意与目标选取全走原版 lord/攻击机制；墙体/沙袋中立无阵营（沿用现有沙袋模式）。

### 4.4 派系自适应材料与炮塔（mod 派系宿主）

当宿主派系来自其它 mod 时，据点建筑的画风应跟着派系走，且全程零 mod 感知代码：

**墙体/掩体材质：从守卫装备采样推导**

守卫生成时（§2.2 小队部分）顺手统计其武器/服装的 StuffDef 出现频次，得票最高的 stuff 用作外墙与掩体材质。mod 派系的小人装备 mod 材料制品，据点墙自然用 mod 材料砌——代码只统计生成实例的 StuffDef，不读任何 mod 标识。采不到 stuff（机械族等无 stuff 装备的派系）回退 XML `wallStuffDef` 默认材质。

**炮塔：全库扫描 + 同包加权 + 预算过滤**

`FactionDef` 不携带炮塔清单，改为运行时推导：

1. 扫描 `DefDatabase<ThingDef>`，筛 `building.turretGunDef != null` 且 `techLevel ≤ 宿主派系 techLevel` 的全部炮塔（mod 炮塔标了正确 techLevel 即自动入池）；
2. **同包优先加权**：`ThingDef.modContentPack == factionDef.modContentPack` 者权重 ×3（同 mod 派系与同 mod 炮塔大概率同一美术体系）；
3. 按造价 ≤ 工事预算（§2.2 威胁点 30%）过滤后加权随机选取。

扫描结果静态缓存一次（Def 库运行期不变），无每据点重复扫描开销。

**守卫**：已由 PawnGroupMaker 保证 mod 派系出 mod 单位，无需任何附加逻辑。

**回退与优先级**（全程静默）：

```
炮塔：扫描+预算过滤命中 → XML turretDefs 白名单 → 沙袋纯人力哨位
材质：守卫采样命中 → XML wallStuffDef 默认石材
```

XML `turretDefs`/`wallStuffDef` 从"唯一来源"降级为**兜底与人工修正口**：玩家/整合包作者可对个别不平衡的 mod 炮塔用黑名单（复用 `dungeonFactionBlacklist` 同风格的 defName 列表，新增 `dungeonTurretBlacklist`，默认空）排除，或往白名单追加。总体原则：**派系自适应优先，XML 清单兜底**。

### 4.5 性能

全部生成逻辑只在据点地图 `PostMapGenerate` 瞬间执行一次；无 Comp、无 per-tick 扫描，符合 rpg 文档 §5 性能规则。

### 4.6 玩家蓝图布局（已实现）

玩家可自建据点布局替代程序化生成：

- **捕捉**：建筑师菜单「副本蓝图」分类（`MMT_Cat_DungeonBlueprint`，`specialDesignatorClasses` 零补丁挂载）→「保存为副本蓝图」框选矩形 → 捕捉矩形内全部已建成建筑（排除 Frame/未建成蓝图，单边上限 40 格，多格建筑只记 Position 一次），记录 defName/材质/朝向/相对偏移（选区左下角为原点）→ 命名弹窗保存（重名确认覆盖）。
- **存储**：跨存档全局，存 `MMTSettings.dungeonBlueprints`（Scribe Deep 序列化）；恢复默认设置不清除。
- **副本类型开关**：每张蓝图带 `enabledSiteParts`（SitePartDef defName 列表）——**只有勾选的据点类型才能随机选用该蓝图**；另带 `enabledTiers` 档位开关（I/II/III，默认全档），生成时按"类型 + 档位"双重过滤（`PickFor(sitePart, tier)`），旧存档蓝图自动补全档。管理在 Mod 设置【副本蓝图】（缩略图 + 类型开关 + 档位开关 + 删除）。缩略图按条目类别着色逐格绘制（炮塔红/门黄/满格墙灰/用电橙/其它蓝灰），运行时生成并缓存。
- **生成接入**：`SitePartWorker_MMT_Dungeon.PostMapGenerate` 先查 `DungeonBlueprintStore.PickFor(sitePart, tier)`（该类型该档启用蓝图随机抽一；无可用 → 程序化布局兜底，完全静默）。蓝图路径：外接矩形定中心 → 逐条 `TrySpawnBuilding`（炮塔挂宿主派系、其余中立、材质缺失用 GenStuff 默认）→ 奖励散布据点内部空格（无独立库房）→ 哨位取炮塔/门格。守卫（PawnGroupMaker 动态生成）/lord/Boss/威胁公式/分档奖励全部不变；蓝图含需电建筑仍走 `GenStep_Power`。
- 蓝图中的炮塔不过 `dungeonTurretBlacklist`（玩家显式设计），也不计工事预算。

### 4.7 Boss 池化选取（v2，已实现）

Boss 不再绑定单一 MMT 独占单位，改为从**原版 + Mod 全库 Boss 池按财富预算选取**，预算允许时一处巢穴可出**多个 Boss**：

- **候选池**（`BossPool.cs`）：XML 白名单（各 SitePartDef `DungeonLayoutExtension.bossPawnKindDefNames`，惰性解析、缺失静默跳过，其它 mod 可 XPath 追加）∪ 全库自动扫描（`isFighter` 且 combatPower ≥ 150，原版/Mod Boss 通吃，零 mod 感知代码，缓存只扫一次）。Boss 召唤器（§7）与本池共用候选源。
- **预算**：Boss 预算 = 据点威胁点 × `bossBudgetFraction`（巢穴定稿 0.5；威胁点已由财富公式 §1.2 推导，即"根据财富选 Boss"）。`bossBudgetFraction > 0` 即视为有 Boss 巢位（库房偏移、中央留开阔巢区）。
- **选取**（`BossPool.PickBosses`）：逐只抽选，单只 combatPower ≤ 剩余预算；数量上限 `bossCountRange`（巢穴定稿 1~3，实际只数受预算约束）；权重偏向战力贴近剩余预算者（财富越高越容易出强 Boss），已选种类不重复。低财富兜底：池内最弱 Boss 不超预算 ×1.5 仍出 1 只；再抽不出则静默退化为大型守卫据点。
- **派系**（`BossPool.ResolveBossFaction`）：类人 → 据点宿主派系（入守卫 lord，装备稀有度掷点由 LootRoll 生成钩子自动结算）；机械族/虫族/异常实体 → 各自原版派系（原生敌对）；动物等 → 无派系 + 强制永久狂暴。全程不自写判敌逻辑。
- **后继新增 Boss**（含 MMT 自建的远古守卫者 `MMT_BossAncientGuardian`）：定义 PawnKindDef 后把 defName 登记进巢穴白名单即入池按预算参选，代码零改动——新 Boss 一律走本池化设计，不再单写挂载逻辑。


---

## 5. 设置与校验

### 5.1 设置（并入 `MMTSettings.cs`，与 rpg 文档 §4 同表）

| 设置项 | 字段 | 默认 | 说明 |
|---|---|---|---|
| 副本派系黑名单 | dungeonFactionBlacklist | "" | defName 逗号分隔（rpg 文档已有） |
| 副本炮塔黑名单 | dungeonTurretBlacklist | "" | defName 逗号分隔，排除个别不平衡的 mod 炮塔（§4.4） |
| 档位成本倍率 | dungeonTierCostFactor | 1.0 | 0.5–3，整体缩放三档声纳模块/扫描天数成本 |

其余开关（`rpgQuestsEnabled`、`bossEncountersEnabled`、`ExplorationSitesEnabled`）沿用既有定义，不重复新增。

### 5.2 校验（`tools/validate/rpg_quests.py` 追加）

- 三档成本表字段完整（模块数 > 0、系数递增）；
- 奖励池条目 defName 有效、`minTier` ∈ {1,2,3}；
- `DungeonLayoutExtension` 引用的建筑 defName 在原版 + MMT 白名单内；
- 黑名单设置读取正确（rpg 文档 §3.4 已有）。

### 5.3 贴图

沿用 rpg 文档 §3.4：3 个据点世界图图标（`World/WorldObjects/Sites/MMT_Site*` 现有目录模式）+ Boss 贴图（无合适复用项再 AI 生成，先按 `.claude/skills/rw-texture-design/SKILL.md` 写贴图元素描述）。布局建筑全部复用原版/MMT 现有贴图，无新增需求。

---

## 6. 待定项（实现时回填）

- ~~是否在 `mmt-rpg-design.md` §3 头部加一行指向本文档~~（v1 回填：不加，保持默认）；
- ~~奖励池条目若数量区间/权重表达不足，升级为 ThingSetMakerDef~~（v1 回填：已加 `maxTier` 字段，
  各档构成按"叠加语义 + maxTier 封顶"表达（如装备箱 1@全档 + 0~1@II + 1@III → I×1 / II 1~2 / III×2），
  暂不升级 ThingSetMakerDef）；
- 奖励价值系数（§3.3，初始 ×1.2）与工事预算拆分（§2.2，初始 70%/30%）经游戏内实测后回填调整值
  （v1 实现定稿：拆分 70%/30% 写入 `DungeonSiteGen.GuardBudgetFraction/FortBudgetFraction` 常量，
  守卫预算封顶 10000 威胁点——超出后多数派系 PawnGroupMaker 无法抽组；
  奖励价值 ×1.2 未做代码强制校验，待游戏内实测）；
- ~~Boss 巢位形态（开阔区 vs 半掩体）~~（v1 回填：开阔巢位——有 Boss 时库房偏移、中央留空；
  v2 回填：Boss 本体改 §4.7 池化选取，不再依赖单一自建 Boss；远古守卫者作为后续白名单登记项保留）。

## 7. v1 实现回填（实现期决策记录）

- **档位传递**：扫描档位写入 `SitePartParams.randomValue`（vanilla 未使用字段，存档安全），
  worker 读取时非法值按 I 档处理（旧存档据点无档位信息，不会误发高档奖励）；
- **据点池权重**（实现期选定，可调）：稀有资源点 28 / 轨道遗迹 22 / 机械据点 12 / 试炼场 22 / 职业圣殿 16 / Boss 巢穴 4（低概率 ≈3.8%，受 bossEncountersEnabled 门控）；
- **品质地板映射**（"精良~稀有"等 RPG 语 → 原版品质档）：I Good~Excellent / II Excellent~Masterwork / III Masterwork~Legendary，
  与 lootTier 1/2/3 一起写在扫描仪 comp 的 `scanTiers` XML 里；
- **炮塔供电**：布局完成后调用原版 `GenStep_Power`（与原版任务据点同机制，自动接发电机+电池+电缆）；
- **守卫 lord**：`LordJob_SitePawns`（原版任务据点守卫 lord：驻守 + 受击/超时后转反攻）；
- **历练书**：服用式使用品（右键"研读"，`IngestionOutcomeDoer_GiveProgressXp`，+500 XP）；
- **凭证碎片配方**：`MMT_CraftClassToken`（3 碎片 → 1 凭证）挂装备强化台（MMT_WeaponEnhanceBench）；
- **贴图**：3 个据点世界图标 + 历练书/凭证碎片物品图标已生成（PIL 代码绘制）；
  Boss 用池内原版/Mod 单位原生贴图，无新增需求（后续自建 Boss 再按 `.claude/skills/rw-texture-design/SKILL.md` 写贴图元素描述）；
- **校验**：`tools/validate/rpg_quests.py`（档位表/据点池/奖励池/布局白名单/Boss 池化字段/配方/设置接线）；
- **Boss 池化**（v2）：Boss 生成由"单 defName 挂载点"改为 §4.7 池化选取
  （`bossBudgetFraction`/`bossCountRange`/`bossPawnKindDefNames` 三字段替换旧 `bossPawnKindDefName`）；
- **Boss 召唤器**（dev 测试工具）：`MMT_BossSummoner`（`MMT_Buildings_Dungeon.xml`），
  仅 Dev 模式在建造菜单可见（`Designator_Build.Visible` postfix 强制隐藏）；
  gizmo 菜单候选复用 Boss 池（§4.7：XML 白名单 ∪ 全库自动扫描，原版与 Mod Boss 通吃，
  菜单标注战力与来源包，候选缓存只扫一次），目标选格生成；
  派系走 BossPool.ResolveBossFaction（类人走副本宿主派系 + `LordJob_AssaultColony` 立即进攻，
  机械/虫/异常实体挂各自派系，动物无派系强制永久狂暴兜底）；
  白名单默认预留 `MMT_BossAncientGuardian`（与巢穴白名单同名，自建 Boss 一处定义两处生效）。
