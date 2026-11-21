# MMT 终局内容包设计方案

> 手工维护的设计文档（非脚本生成物）。记录"纪元奇点"终局玩法全套设计：终局奇观、世界探索、武器强化、后期威胁、Mod 设置。
> 状态：**v1 已实现**（阶段 A-F 全部落盘；测试验收见 `docs/mmt-endgame-test-guide.md`）。各 validate 套件与本文件互相印证。

## 0. 背景与动机

MMT 现有生产链已铺全（108 项研究、电力阶梯 原煤2500W → 聚合物5000W → 核电20000W → 聚能100000W、物质转换实现"资源自由"），但内容几乎全是**产出端**，缺少**目标端**：

- Def 中无任务/事件/世界据点/胜利条件类内容（唯一 WorldObject 是传送台临时落脚点）；
- 8 种资源有产出但零消费（见 `docs/mmt-unused-resources.md`）：土堆、强化玻璃块、金属罐、氧化化合燃料、机器人部件、矿物声纳模块、兽脂、绘画用品；
- 唯一的后期消耗赌博是护盾腰带 1–15 级强化。

本设计用五大块内容把资源流闭合成环：

```
8 种闲置资源 ──→ 奇观四阶段造价 + 武器强化台 + 淬火油 + 扫描仪消耗(声纳模块)
深空扫描(耗声纳模块×1) ──→ 生成探索据点(Site) ──→ 传送台折跃战斗 ──→ 独占资源回流
  MMT_ArchotechShard 远古碎晶 ──→ 奇观核心 / 武器强化 +6~+10
  MMT_AncientDatacore 数据核心(不可交易) ──→ 武器特殊附魔词条
  MMT_AncientAlloy 远古合金 ──→ 奇观框架 / 武器强化 +3~+5
奇观激活 ──→ 三选一永久增益 ──→ 对冲能量风暴 / 财富缩放袭击的后期压力
ModSettings 全局调参(产率/研究成本/传送次数/强化成功率/爆墙/风暴)
```

**现状核实**：AI 爆墙炸弹已实现（`Source/MyModeTest/BreachCharge.cs`，财富门槛 10 万、封顶 100 万、携带概率 5%→50%、进攻概率 2%→20%，均为常量），本设计仅做设置化接入。传送台 `CompTeleportPad` 已支持任意 `MapParent` 折跃目标，新探索据点做成 `Site` + `SitePart` 即可零改动接入传送链路。

---

## 1. Mod 设置面板（最先实现，其余各块均读它）

MMT 目前无任何 ModSettings。新增：

- `Source/MyModeTest/MMTSettings.cs`：`MMTSettings : ModSettings`（字段 + `ExposeData`，新字段带默认值，旧存档安全）。
- `Source/MyModeTest/MyModeTestMod.cs` 改造：`GetSettings<MMTSettings>()`、`DoSettingsWindowContents(Rect)`（单页滚动）、`SettingsCategory() => "MMT 资源生产"`。

| 设置项 | 字段 | 默认 | 范围 | 消费端 |
|---|---|---|---|---|
| 资源产率倍率 | resourceYieldFactor | 1.0 | 0.5–3.0 滑条 | Harmony postfix `GenRecipe.MakeRecipeProducts`，仅 `MMT_` 前缀配方乘 stackCount（OreSift 随机产物同路径自动覆盖） |
| 研究成本倍率 | researchCostFactor | 1.0 | 0.25–2.0 | postfix `ResearchProjectDef.get_CostApparent`（签名待 §7 spike 核实），仅 MMT_ 前缀 |
| 传送次数上限加成 | teleportBonusCharges | 0 | 0–9 整数 | `CompTeleportPad.MaxTeleportCharges` 读取 |
| 强化成功率倍率 | enhanceSuccessFactor | 1.0 | 0.5–2.0 | `CompShieldEnhancement.SuccessRateFor` 与新武器强化共用 |
| 仅限 MMT/原版武器强化 | weaponEnhanceVanillaOnly | false | 勾选 | 勾上后 comp 挂载补丁跳过其它 mod 武器（跨 Mod 兼容总开关，见 §2） |
| 爆墙财富门槛 | breachWealthThreshold | 100000 | 5万–50万 | `BreachChargeUtil` 常量改读设置 |
| 爆墙概率倍率 | breachChanceFactor | 1.0 | 0–3（0=关闭） | `BreachChargeUtil` 两条概率曲线 |
| 能量风暴开关 | energyStormEnabled | true | 勾选 | `IncidentWorker_EnergyStorm.CanFireNow` |
| 风暴频率倍率 | energyStormChanceFactor | 1.0 | 0.5–3 | worker 内掷点修正 |

**事件机制开关（硬性设计规则：任何 Incident / GameCondition / 任务 / 据点生成类机制，落地时必须同步加开关；正面/中性事件默认开，恶意事故类默认关）**：

| 设置项 | 字段 | 默认 | 控制对象 |
|---|---|---|---|
| 爆墙炸弹总开关 | breachChargeEnabled | true | 三条爆墙补丁的入口判定（关=完全禁用，含配发） |
| 能量风暴 | energyStormEnabled | true | §5 能量风暴事件 |
| 电磁脉冲日 | empDayEnabled | true | §10.5 护盾/充能设备离线事件 |
| 陨石雨 | meteorShowerEnabled | true | §10.5 陨石坠落（远古合金第二来源） |
| 精英爆破手 | eliteBreacherEnabled | true | §10.5 财富 50 万+ 专职爆破单位 |
| 黑市中间人 | blackMarketEnabled | true | §11.1 远古合金收购事件 |
| 强化材料赏金任务 | materialQuestsEnabled | true | §11.1 定居点委托任务 |
| 据点生成总开关 | explorationSitesEnabled | true | §3 扫描仪生成据点（含坠毁船等后续据点类型） |
| 奇观超载事故 | wonderOverloadEnabled | **false** | §10.4 蓄电超载爆炸（硬核向，默认关） |
| 奇观周年庆典 | wonderAnniversaryEnabled | true | §10.4 激活纪念日节庆 |

实现约束：开关判定放在各自 `IncidentWorker.CanFireNow` / comp 入口的**第一行**（廉价 bool 短路，符合 §12 性能原则）；关闭时已生成的进行中期事件不强制移除，只阻止新触发。

校验套件：`tools/validate/settings.py`。

---

## 2. 装备深度强化：武器强化台（赌博线 + 确定线）

### 建筑

`MMT_WeaponEnhanceBench` 武器强化台（2×2）：强化玻璃×80、机器人部件×20、碳合金×40、微芯片×4；WorkToBuild 6000；1500W。
文件：`1.7/Defs/ThingDefs_Buildings/MMT_Buildings_WeaponEnhance.xml`。

### C#（`Source/MyModeTest/WeaponEnhance.cs`）

- `CompProperties_WeaponEnhancement` / `CompWeaponEnhancement`：等级 + 附魔词条，label 追加 "+N"。经 XPath PatchOperation 批量挂到全部 `ThingDef[weaponTags]`（已有 comps 的用 `PatchOperationSequence`）。
- `CompProperties_WeaponEnhanceBench` / `CompWeaponEnhanceBench`：IThingHolder 容器持 1 件武器，右键"装载武器/取出/强化/附魔"。结构照搬 `ChargingStation.cs` 的 `CompChargingStation`（注意：1.7 ThingComp 不再自带 IThingHolder，需显式实现）。
- **赌博线（+1 ~ +15）**：照 `CompShieldEnhancement` 模式（护盾线同为 15 级，两线共用同一套材料阶梯与成功率表）。成功率表 15 档（1-3 级 100%、4-6 级 80%、7-9 级 60%、10-12 级 40%、13-14 级 20%、15 级 5%），作业在强化台进行（先消耗材料后掷点）。效果：每级伤害 ×1.05（满级 ≈×2.0），15 级额外护甲穿透 +10%。成功率乘 `enhanceSuccessFactor`。
- **强化材料阶梯（核心设计：与探索/任务深度挂钩）**：

| 等级段 | 材料（每次强化消耗） | 来源 |
|---|---|---|
| +1~+3 | 能量结晶×目标等级 | 常规自产（晶体培养舱） |
| +4~+6 | 能量结晶×目标等级 + 高级合金×2（钨/钛等可生产合金轮替） | 常规自产 |
| +7~+9 | 能量结晶×5 + **远古合金**×3 | 探索据点（稀有资源点/轨道遗迹）+ 任务奖励 |
| +10~+12 | 能量结晶×8 + **远古碎晶**×2 | 探索据点（轨道遗迹/机械据点）+ 任务奖励 |
| +13~+15 | 能量结晶×10 + **远古碎晶**×3 + **远古数据核心**×1 | 机械据点/史诗档据点/任务奖励 |

  即：+6 是自给自足的天花板，+7 起必须走出家门。**护盾强化线（现有 1-15）同步改挂此阶梯**（`CompShieldEnhancement` 需把单一 `materialPerLevel` 扩展为按等级段配材料的 XML 表，旧存档兼容：缺省回退旧行为）。
- **确定线（附魔系统，四档词条 + 多槽组合，100% 成功）**：

  **附魔槽位**：武器基础 1 槽；强化 +7 解锁第 2 槽、+13 解锁第 3 槽（强化与附魔互相成就——冲槽位要探索材料，见材料阶梯）。同词条不可重复，不同档可自由组合（如 低级"精准" + 高级"燃烧弹"）。

  **词条总表**（词条 = `ModuleEffectKeys` 新键；标 ★ 的为特殊机制，需在伤害补丁中追加消费逻辑，不止 `GetEffectSum` 一行）：

  | 档位 | 材料（每次附魔） | 词条 |
  |---|---|---|
  | 低级（基础属性） | 淬火油×2 + 能量结晶×3 | 锋利（伤害+8%）、精准（命中+8%）、轻巧（冷却-8%）、穿透（穿甲+15%） |
  | 中级（复合属性，双词条） | 淬火油×3 + 高级合金×8 | 致命（暴击率+5% ★暴击×2 伤害）、疾风（冷却-12% + 瞄准-20%）、重击（伤害+12% + 击退 ★） |
  | 高级（命中特效） | 淬火油×4 + 远古合金×10 | ★EMP 弹（对机械 ×1.5 + 短路）、★燃烧弹（命中点燃）、★吸血（近战，伤害 5% 转治疗）、★破盾（对能量护盾额外 +50%）、★冰霜（命中减速 -20% 持续 3 秒 hediff） |
  | 极品（独特机制，数据核心） | 淬火油×5 + 远古碎晶×5 + 数据核心×1 | ★处决（目标血量 <20% 时伤害 ×2）、★连锁闪电（命中弹射 2 格内另一目标 50% 伤害）、★时空折叠（击杀后本次冷却归零）、★过载（伤害 +25% 但冷却 +15%） |

  与 +13~+15 强化共用数据核心，形成"冲高强化还是做极品附魔"的取舍。低级/中级词条材料全自产，高级起必须探索/任务（与强化材料阶梯同轨）。对应模块物品按词条一件一个（`MMT_WeaponMod<词条名>`，挂 `ModuleExtension`，slotKey=WeaponEnchant），共 17 件。
- **效果键清单**（`ModuleEffectKeys` 新增，纯数值词条 8 键 + 特殊机制 9 键）：
  - 数值键：`WeaponDamageFactor`（锋利/重击/过载）、`WeaponHitChance`（精准）、`WeaponCooldownFactor`（轻巧/疾风/过载）、`WeaponWarmupFactor`（疾风）、`WeaponCritChance`（致命）、`WeaponArmorPen`（穿透）、`WeaponKnockback`（重击）
  - 机制键（值>0 即启用，逻辑写在伤害补丁）：`WeaponEmp` / `WeaponIncendiary` / `WeaponLifeLeech` / `WeaponShieldBreaker` / `WeaponFrost` / `WeaponExecute` / `WeaponChainLightning` / `WeaponTimeFold`
- 伤害挂钩 Harmony patch ×2（**实现前必须完成 §7 签名 spike**）：
  1. 远程：`Projectile.GetDamageAmount` postfix 取发射装备 comp 乘系数（取不到则 `Verb.TryCastShot` 上下文缓存兜底）；★机制词条在命中补丁（`Projectile.Impact`）统一结算；
  2. 近战：`Tool.AdjustedBaseMeleeDamageAmount` postfix；近战 ★机制在 `Verb_MeleeAttack` 伤害应用后结算。
- **跨 Mod 兼容（硬性要求：强化体系对所有 mod 武器开放）**：
  - comp 挂载补丁的 XPath 打 `Defs/ThingDef[weaponTags]` **全体武器**（不按 `MMT_` 前缀过滤），其它 mod 武器自动获得强化能力；补丁在游戏全部 mod 加载完后执行，天然覆盖后加载 mod 的 Def。
  - 强化/附魔效果判定一律以"装备上是否存在 `CompWeaponEnhancement`"为准，不判 defName 前缀。
  - **黑名单**：设置面板提供"排除其它 mod 武器"总开关（`weaponEnhanceVanillaOnly`，默认关=全兼容）；另在 XML 补丁中对明显不合适的类别（炮塔建筑武器、biocoded 绑定装备类）加 `PatchOperationConditional` 排除。
  - **CE（Combat Extended）风险**：若玩家同时装 CE，其重写了弹道/伤害链路，`GetDamageAmount` postfix 可能不生效——实现时检测 `ModLister` 中 CE 存在则在设置面板标注"CE 环境下远程强化可能失效"，近战线不受影响。MMT 自身不声明 CE 依赖。
  - 附魔槽位/强化标签显示走 `TransformLabel`，对任意 mod 武器 label 安全。
- **护盾强化跨 Mod 兼容**：护盾强化 comp 挂载补丁打全体带 `CompProperties_ShieldBubble` 的 Apparel（含其它 mod 护盾腰带），规则：
  - **MMT 护盾**：保持现有完整 15 级阶梯（电量上限/减耗/全部专有强化维度）；
  - **非 MMT 护盾**：只开放两个通用维度——**护盾值**（每级能量上限 +8%）与**恢复效率**（每级充能/回充速率 +6%），满级 15 级即上限 ×2.2 / 恢复 ×1.9；不开放 MMT 专有维度（避免对未知 mod 护盾的机制做假设，CE/护盾类 mod 各自实现差异大时也只影响这两个数值，风险最小）；
  - 判定按 def 是否 `MMT_` 前缀区分两档行为（此处前缀判定合法：决定的是"能力集"而非"是否生效"）；
  - 设置面板共用 `weaponEnhanceVanillaOnly` 开关（勾上后非 MMT 护盾同样不挂 comp）。
- 两个 JobDriver（Enhance / Enchant）+ JobDef `MMT_EnhanceWeapon` / `MMT_EnchantWeapon`。

### 配套资源/配方

- `MMT_QuenchOil` 淬火油：配方 `MMT_MakeQuenchOil`（兽脂×5 + 氧化化合燃料×2 → 3 份，挂精炼设备）。

### 贴图与校验

- 强化台多朝向 6 张 + 淬火油 1 张 + 武器模块 17 张（走 rw-texture-design 流程，同档位模块共用底色、图标区分词条）。
- 校验套件：`tools/validate/weaponenhance.py`（comp 挂载补丁存在、成功率表长度=15、材料阶梯表引用、17 词条模块与效果键一一对应、贴图、JobDef driverClass 匹配）。

---

## 3. 世界地图探索：深空扫描 + 三类据点

### 建筑

`MMT_DeepSpaceScanner` 深空扫描仪（3×3）：Steel×150、矿物声纳模块×10、太空零件×6、金×20；WorkToBuild 8000；3000W。
comps：`CompBuildingBattery`（30000 Wd / 3000 每天）+ 模块槽（`SpeedFactor` 上限 5，缩短扫描冷却）+ 自定义扫描 comp。
文件：`1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Exploration.xml`。

### C#（`Source/MyModeTest/Exploration.cs`）

- `CompDeepSpaceScanner`：gizmo"启动深空扫描"（蓄电满 `TryDrain` + 消耗声纳模块×1）→ 扫描中（默认 1 天，SpeedFactor 加速，周期逻辑用 `CompTickInterval(delta)`——1.7 Normal 建筑收不到 `CompTickRare`）→ 完成时随机选非水非占领格（半径 30 内）`SiteMaker.MakeSite` 生成据点 → 发信（PositiveEvent，SitePart `expirationDays` 30 天过期）。
- `SitePartWorker_MMT_Stockpile : SitePartWorker`：`PostMapGenerate` 在图上生成护栏围起的奖励堆（独占资源×N + 随机太空零件）。一个类三个参数化 def 复用。
- 据点回收走原版 `Site.ShouldRemoveMapNow` 逻辑，无需自定义。

### 三类据点（`1.7/Defs/WorldObjectDefs/MMT_WorldObjects_Exploration.xml`）

| Site defName | 内容 | 守卫 |
|---|---|---|
| MMT_Site_OrbitalRuin 轨道遗迹 | 奖励堆（远古碎晶为主） | 原版 MechCluster SitePart（休眠机械群，唤醒走原版 CompCanBeDormant，**绝不自写判敌逻辑**） |
| MMT_Site_MechStronghold 机械据点 | 奖励堆（数据核心为主） | 大参数 MechCluster |
| MMT_Site_RareDeposit 稀有资源点 | 富矿散点 + 远古合金 | 轻守卫（少量虫族或无） |

折跃进入 / 选点 / 召回 / 临时站点回收全部走传送台现有分支，零改动。

### 据点奖励档位（喂强化材料阶梯，见 §2）

| 据点 | 主产出 | 强化阶梯对应 |
|---|---|---|
| 稀有资源点 | 远古合金×15-25 | +7~+9 材料 |
| 轨道遗迹 | 远古碎晶×6-10 + 远古合金×5 | +10~+12 材料 |
| 机械据点 | 远古碎晶×8-12 + 远古数据核心×1 | +13~+15 与特殊附魔材料 |

除探索外，**任务线**为第二获取通道（见 §11.1"强化材料赏金任务"），保证不喜欢战斗的玩家也能冲高强化，但单位时间产出低于探索。

### 独占资源（`MMT_Items_Resource_Exploration.xml`）

| defName | label | MarketValue | 备注 |
|---|---|---|---|
| MMT_ArchotechShard | 远古碎晶 | 90 | 奇观核心×30 / 武器+6~10 材料 |
| MMT_AncientDatacore | 远古数据核心 | 150 | tradeability None（不可交易，强制闭环）；武器特殊词条 |
| MMT_AncientAlloy | 远古合金 | 35 | 金属 stuff；奇观框架×80 / 武器+3~5 材料 |

### 贴图与校验

- 扫描仪多朝向 6 张 + 独占资源 3 张 + 据点世界图标 3 张。
- 校验套件：`tools/validate/exploration.py`（Site/SitePart/worker 类名匹配、资源引用、扫描仪 comps、贴图）。

---

## 4. 终局奇观：纪元奇点（四阶段）

新文件：`1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Wonder.xml`。5×5，露天（挂原版 `PlaceWorker_NotUnderRoof`）。

| 阶段 defName | label | 造价 | WorkToBuild |
|---|---|---|---|
| MMT_WonderFoundation | 奇点地基 | 土堆×2000、混凝土×400、Steel×300 | 15000 |
| MMT_WonderFrame | 奇点框架 | 强化玻璃×300、金属罐×200、碳合金×150、金×50 | 20000 |
| MMT_WonderCore | 奇点核心 | 反物质×10、α聚合物×100、机器人部件×60、远古合金×80、太空零件×20 | 30000 |
| MMT_Singularity | 纪元奇点（终态） | 氧化化合燃料×400、远古碎晶×30、绘画用品×50、铀×100 | 20000 |

### C#（`Source/MyModeTest/Wonder.cs`）

- `CompWonderStage`（前 3 阶段通用）：gizmo"开始下一阶段建设" → despawn 自身 + `GenConstruct.PlaceBlueprintForBuild` 放下一阶段蓝图（造价逐阶段另付）。
- `CompWonderActivation`（终态）：激活条件 = 反物质燃料满（`CompProperties_Refuelable`，容量 5，激活后不再消耗；**反物质自带 CompExplosive，搬运/存储爆炸风险是特性，描述中警告玩家**）+ 蓄电 50000 Wd（`CompBuildingBattery`，chargeRate 5000/天）→ gizmo"启动奇点" → 点火进度动画（特效结构仿能量聚能点火；光柱用**水平碟片堆叠**，1.7 竖直 quad 不渲染）→ 三选一弹窗（`Dialog_MessageBox` 三按钮）→ 生成对应永久 GameCondition 落到**本地图**（`durationTicks=-1`，GameConditionManager 自动持久化；跨图不共享——每张图各建各的奇观）。
- 三选一永久增益：

| 选项 | GameConditionDef | 机制 |
|---|---|---|
| 能源纪元 | MMT_Condition_SingularityPower | Harmony postfix：发电建筑输出 ×1.25 |
| 启迪纪元 | MMT_Condition_SingularityMind | 条件每 2500 tick 给图上殖民者刷新永久 Hediff `MMT_Hediff_SingularityInsight`（研究速度+25%、全局工作速度+10%） |
| 丰饶纪元 | MMT_Condition_SingularityHarvest | Harmony postfix：`Plant.GrowthRate` ×1.5 |

### 贴图与校验

- 四阶段各 6 张（多朝向+遮罩，5×5 → 640×640）+ 激活光效叠加层 `MMT_SingularityGlow.png`（DrawAt 叠画）。
- 校验套件：`tools/validate/wonder.py`（4 阶段 def、造价资源引用、贴图、终态 comps 三件套、GameCondition/Hediff def）。

---

## 5. 后期威胁：能量风暴 + 爆墙设置化

### 能量风暴

- C#：`Source/MyModeTest/Storm.cs`；Def：`1.7/Defs/GameConditionDefs/MMT_GameConditions.xml`。
- `MMT_EnergyStorm` GameCondition（持续 1–2 天）：
  1. 太阳/风能输出 ×0（条件在图时 postfix 对应 CompPowerPlant）；
  2. 每 2500 tick 随机点名 1–3 个未防护电子设备触发 `CompBreakdownable` 故障 + 蓄电建筑 `TryDrain` 部分电量（短路火花）；
  3. 防护联动：`MMT_StormDampener` 风暴阻尼器（2×2，强化玻璃×40 + 金×15，800W）半径 12 内建筑免疫点名；MMT 护盾系建筑白名单免疫。
- `MMT_Incident_EnergyStorm`（baseChance 2，minRefireDays 20）：worker `CanFireNow` 检查财富 ≥20 万 + `energyStormEnabled`，频率乘 `energyStormChanceFactor`。
- **v1 不做自定义天气/天空特效**，仅机制 + 事件信。

### 爆墙炸弹设置化

`BreachChargeUtil` 的 4 个常量（财富门槛/封顶/携带曲线/进攻曲线端点）改读 `MMTSettings`，逻辑零改动，回归 `breachcharge` 套件。

### 贴图与校验

- 阻尼器多朝向 6 张。
- 校验套件：`tools/validate/storm.py`（GameCondition/Incident def、阻尼器 comp、补丁类存在性）。

---

## 6. 研究树新增（5 项）

新文件 `1.7/Defs/ResearchProjectDefs/MMT_Research_Endgame.xml`。坐标追加进 `tools/apply_research_layout.py` 的 LAYOUT 表（唯一事实来源，禁止手改 XML 坐标），改后重跑 `apply_research_layout.py` + `build_research_mindmap.py`：

| defName | label | 点数 | 前置 | (x, y) |
|---|---|---|---|---|
| MMT_WeaponEnhancement | 武器强化 | 4000 | MMT_AdvancedAlloys(8,4) + MMT_RechargeableTech(9,6) | (11,5) |
| MMT_WeaponEnchantment | 武器附魔 | 6000 | MMT_WeaponEnhancement(11,5) | (12,5) |
| MMT_StormShielding | 风暴防护 | 3000 | MMT_DeployableShieldTech(7,6) | (13,5) |
| MMT_DeepSpaceScan | 深空扫描 | 8000 | MMT_Teleportation(12,6) | (13,7) |
| MMT_SingularityConstruction | 奇点工程 | 20000 | MMT_MatterConversion3(11,6) + MMT_EnergyConvergence(10,9) | (12,9) |

全部满足"依赖 X > 前置 X"，坐标与现有 108 项无冲突。新内容无 HSK 对应物，`tools/data/mmt_hsk_alias.json` 不动。

---

## 7. 实现顺序（6 阶段）

1. **阶段 A · 设置面板**：`MMTSettings` + Mod 入口改造 + 爆墙设置化 → `settings.py` + `breachcharge` 回归。独立可测，后续各块都读设置。
2. **阶段 B · 签名 spike**：用 `tools/temp/ildump` 核实 1.7 四个方法签名（Harmony 参数名须逐字一致；`BreachCharge.cs`、`OreSiftPool.cs` 的 patch 写法是已验证参照）：
   - `Projectile.GetDamageAmount`（能否取到发射装备，决定远程强化写法）
   - `Tool.AdjustedBaseMeleeDamageAmount`
   - `ResearchProjectDef.CostApparent`
   - `GenRecipe.MakeRecipeProducts`
3. **阶段 C · 武器强化**：建筑/comp/JobDriver/伤害 patch/淬火油/模块键全链 → `weaponenhance.py` + 贴图。
4. **阶段 D · 世界探索**：独占资源 def → 扫描仪 → Site/SitePart/worker → `exploration.py` + 贴图。手动测试重点：折跃 MechCluster 据点（休眠唤醒）、召回、取走奖励后站点回收。
5. **阶段 E · 奇观 + 风暴**：奇观四阶段+三增益（依赖 D 的独占资源定价）；能量风暴+阻尼器（可与奇观并行）→ `wonder.py` + `storm.py` + 贴图。
6. **阶段 F · 研究树 + 收尾**：研究 XML + LAYOUT 落盘 + 重跑两脚本 → `validate_mod.py research` 全绿 → 全量 validate → 同步 `D:\SteamLibrary\steamapps\common\RimWorld\Mods\rw` → 游戏内手动测试清单。

每阶段末尾：`dotnet build` + 对应新套件 + `validate_mod.py base research breachcharge` 回归。提交按块分中文 conventional commits。

### 关键复用点

| 复用对象 | 用途 |
|---|---|
| `Source/MyModeTest/ChargingStation.cs` | 强化台容器/右键装卸/JobDriver 的复制模板 |
| `Source/MyModeTest/BuildingBattery.cs` | 扫描仪/奇观蓄电一行 comps（同建筑不得重复挂载） |
| `Source/MyModeTest/ModuleSystem.cs` | 武器词条 = 新效果键常量 + 消费端 `GetEffectSum(key)` |
| `Source/MyModeTest/TeleportPad.cs` | 探索折跃链路（零改动，Site 即 MapParent） |
| `Source/MyModeTest/BreachCharge.cs` | 仅常量改读设置 |
| `tools/temp/ildump` | 阶段 B 签名核实 |
| `tools/validate_mod.py` helper | 新套件 `from validate_mod import fail, require_files, parse_xml, ...` |

---

## 8. 风险与 1.7 坑（设计已规避，实现时复查）

- **武器伤害挂钩（最高风险）**：1.7 `Projectile.GetDamageAmount` 可能拿不到装备引用 → 阶段 B 先行；兜底 `Verb.TryCastShot` 上下文缓存方案。
- **tick 分发**：Normal tickerType 建筑收不到 `CompTickRare`，扫描进度/风暴点名/启迪 hediff 刷新一律 `CompTickInterval(delta)`。
- **竖直 quad 不渲染**：奇点光柱用水平碟片堆叠，禁止 plane10 立起。
- **ThingComp 不自带 IThingHolder**：强化台容器显式实现。
- **Harmony 参数名逐字一致**：单个补丁失败会中断 PatchAll 致后续补丁全失效。
- **休眠 Lord**：机械据点用原版 MechCluster SitePart，不自写生成/判敌；奖励判定以"奖励堆被取走/站点计时"为准，不依赖 pawn 扫描。
- **批量挂 comp 的 PatchOperation**：含已有 comps 的 ThingDef 用 `PatchOperationSequence`；加载警告 + 游戏内 inspect 验证。
- **GameCondition 永久化**：`durationTicks=-1` 由 GameConditionManager 持久化，无需自写 WorldComponent；增益绑定建造所在图。
- **研究布局**：坐标一律改 LAYOUT 表再跑脚本，禁止手改 XML（会被覆盖且 validate 校验一致性）。

---

## 9. 验证方式

**自动校验（每阶段后）**：
- `PYTHONIOENCODING=utf-8 python tools/validate_new.py settings weaponenhance exploration storm wonder`
- `python tools/validate_mod.py base research breachcharge`
- 阶段 F：`apply_research_layout.py` + `build_research_mindmap.py` 后 `validate_mod.py research` 必须全绿。

**游戏内手动测试清单**：
1. dev 刷资源 → 造强化台，装 MMT 步枪强化 +3，验证伤害上浮与标签 "+N"；淬火油附魔词条生效。
2. 扫描仪消耗 1 声纳模块 → 1 天后收据点信 → 传送台折跃进入（休眠机械正常唤醒、战斗、拾取独占资源）→ 召回 → 站点自动回收。
3. dev god mode 依次建四阶段奇观：每阶段 gizmo 换蓝图；终态加注反物质 + 蓄电 50000 Wd → 激活弹窗三选一 → 对应增益生效（发电数值/hediff/植物生长 inspect）。
4. 财富 20 万+ dev 触发 `MMT_Incident_EnergyStorm`：太阳能归零、未防护建筑故障、阻尼器半径内免疫。
5. 财富 10 万+ 袭击：爆墙炸弹携带者出现，概率随设置滑条变化。
6. 设置面板 8 项改动即时生效、读档保持。
7. 旧存档加载：无红字，传送台蓄电/次数、护盾强化等级无缝接管。

---

## 10. 扩展玩法与设备候选（排期池）

> 以下为各块可追加的候选内容，均**未排期**。每条标注：机制一句话 / 资源咬合 / 复用点 / 实现成本（低=纯 XML 或几十行 C#，中=单 comp 级，高=新系统级）。实现任意条目时从本节移入对应正文章节。

### 10.1 设置面板扩展

| 候选 | 机制 | 成本 |
|---|---|---|
| 预设档位按钮 | 面板顶部"轻松/标准/硬核"三按钮一键填充全部滑条 | 低 |
| 据点生成与掉落 | 扫描冷却倍率、据点奖励数量倍率两个滑条（读 CompDeepSpaceScanner / SitePartWorker） | 低 |
| 风暴强度 | 每轮点名数量 1-5 滑条、点名间隔倍率 | 低 |
| 奇观激活难度 | 激活蓄电量倍率（默认 50000 Wd，0.5-2 倍） | 低 |

### 10.2 装备强化扩展

| 候选 | 机制 | 资源咬合 | 成本 |
|---|---|---|---|
| 防具强化线 | 同一强化台第二容器槽，护甲/衣物 +1~+5 级（每级护甲+4%），材料碳合金+α聚合物 | 消耗 α聚合物、碳合金 | 中（复用赌博线框架） |
| 强化转移装置 | `MMT_EnhanceTransferer`：把旧武器强化等级转移到同类型新武器，消耗数据核心×1 | 数据核心新出口 | 中 |
| 充能设备强化 | 护盾腰带之外的充能设备（手电、充能武器）接入充能台强化线，共用 SuccessRateFor | 能量结晶 | 低 |
| 更多附魔词条 | 命中率+8%、射程+2、近战吸血 5%、EMP 特效弹（对机械 ×1.5） | ModuleEffectKeys 新键 | 低（每键=常量+消费端一行） |
| 满级命名 | 强化 +10 武器可右键自定义命名（Dialog_Rename 模式） | 纯体验 | 低 |

### 10.3 世界探索扩展

| 候选 | 机制 | 资源咬合 | 成本 |
|---|---|---|---|
| 坠毁运输船据点 | 第四类 Site：无守卫，舱内 1-2 名幸存者可救援（招募或释放换好感），限时 15 天 | 人力来源 | 中 |
| 远古仓储库 | 第五类 Site：大量普通资源（钢铁/零部件/化合燃料堆），虫族守卫 | 中前期资源反哺 | 低（复用 Stockpile worker） |
| 据点品质分级 | 扫描产出 普通/稀有/史诗 三档，档次影响奖励数量与守卫强度；史诗档必出数据核心 | 声纳模块消耗不变，期望值分层 | 中 |
| 深空信标 | `MMT_DeepSpaceBeacon`：被动建筑，每 5-8 天自动生成一个普通档据点（不耗声纳模块），与扫描仪互补 | 声纳模块压力缓解 | 低 |
| 波次防守据点 | 可重复挑战的机械竞技场：抵达后按波次唤醒机械，撑过 N 波开奖励舱；波次随完成次数递增 | 后期战斗目标端 | 高 |
| 双向传送信标 | `MMT_TeleportBeacon`：可携带建筑，在据点部署后成为传送台固定折跃目标（免扫描往返） | 传送台链路扩展 | 中 |

### 10.4 奇观扩展

| 候选 | 机制 | 资源咬合 | 成本 |
|---|---|---|---|
| 奇观升级 Lv2/Lv3 | 激活后可用 远古碎晶×20/50 升级，增益幅度 ×1.5/×2，外观加环 | 碎晶持续消耗 | 中 |
| 新增增益选项 | 贸易纪元（售价+20%）、坚韧纪元（疼痛×0.8、倒地阈值+10%）——三选一改五选一 | 无 | 低（各一个 postfix/hediff） |
| 奇点能源 | 奇观激活后解锁研究"奇点能源"：`MMT_SingularityReactor`（200000W，无需燃料），电力阶梯第五级 | 奇观成为电力链顶端 | 低（建筑+研究） |
| 激活周年庆典 | 每年激活纪念日全图节庆事件（心情+6 一天），纯氛围 | 无 | 低 |
| 超载事故（可选） | 设置开关：蓄电超载时小概率爆炸事故（反物质设定契合），硬核向 | 风险叙事 | 中 |

### 10.5 威胁/事件扩展

| 候选 | 机制 | 资源咬合 | 成本 |
|---|---|---|---|
| 气象雷达 | `MMT_WeatherRadar`：能量风暴提前 6 小时预警信（给玩家开防护的时间窗） | 与阻尼器形成"预警-防护"链 | 中 |
| 法拉第罩（消耗品） | 一次性物品，给单建筑 24h 风暴免疫；配方：金属罐×2+金×1 | 金属罐新出口 | 低 |
| 闪电收集器 | `MMT_LightningCollector`：仅风暴期间发电（8000W），风险与收益并存 | 风暴从纯负面变双刃剑 | 低 |
| 电磁脉冲日 | 新 GameCondition：护盾腰带/充能设备全部离线 12-24h，克制护盾流 | 反制玩家最强装备 | 低 |
| 陨石雨 | 新 Incident：小规模陨石坠落，坑内可挖远古合金（据点之外的第二获取源，量小） | 合金双通道 | 中 |
| 精英爆破手 | 财富 50 万+袭击中出现专职爆破单位（多枚爆墙炸弹+直奔墙体），爆墙系统进阶 | 威胁曲线延伸 | 中 |

### 10.6 全局/研究扩展

| 候选 | 机制 | 资源咬合 | 成本 |
|---|---|---|---|
| 研究加速装置 | `MMT_DatacoreBurner`：消耗数据核心×1 立即完成当前研究 25% 进度（每个核心限用一次的研究外挂） | 数据核心第二出口 | 中 |
| 壁画系列 | 绘画用品消费端：画架配方 → 3 档 Beauty 建筑（速写/油画/巨作），后期财富与美观来源 | 绘画用品闭环 | 低 |
| 兽脂蜡烛/肥皂 | 兽脂轻量消费：蜡烛（早期照明+美观）、肥皂（卫生向心情小加成） | 兽脂闭环（淬火油之外的轻出口） | 低 |
| 自动化扩展 | 见 §11.3（已按"不与现有自动设备职能重叠"规则重整：屠宰做成畜牧机器扩展、深层采矿做成自动采石机升级，不新增重复设备） | — | — |
| 探索者传承 | 完成 N 次据点探索解锁殖民者永久 hediff"探索者"（移动+5%、射击+1） | 重复游玩激励 | 中 |

---

## 11. 三大领域分阶段建议（交易 / 装备 / 自动化）

> 现状：MMT **交易内容为零**（无商人/订单/轨道交易任何 Def）；**自动化**已有自动处理机、自动农场、畜牧机器、自动采石机、晶体培养舱（均 AutoProcessor 模式）；**装备**已有古代/常规/未来武器链、护盾腰带、充能设备、模块体系。以下按游戏阶段给候选，均未排期。

### 11.1 交易（从 0 到 1）

| 阶段 | 候选 | 机制 | 咬合与成本 |
|---|---|---|---|
| 前期 | 大宗订单事件 | QuestScriptDef：定居点高价收购 MMT 中间品（玻璃×200、混凝土×500 等），时限送货到世界格或等商队上门 | 让过剩产能有出口；中 |
| 前期 | 打包售货箱 | 把散装资源（矿粉/玻璃/建材）打包成"货箱"（堆叠 1、价值浓缩），缓解卖货运力 | 复用 HSK 炮塔装箱模式；低 |
| 中期 | 贸易站台 | `MMT_TradePost`：提高商队到访频率 + 售价 +5%（美观/舒适度类被动建筑） | 消耗玻璃钢/金；低 |
| 中期 | 期货合约台 | 锁定某资源 30 天收购价（价格高于市价 15% 但必须到期足量交付，违约扣好感） | 产量规划玩法；中 |
| 后期 | 自动售货站 | `MMT_AutoSeller`：资源装进货舱，商队/轨道商路过时按市价自动结算扣货（挂机经济） | 电力+机器人部件；中 |
| 后期 | 强化材料赏金任务 | QuestScriptDef：定居点发布委托（送货/剿匪/护卫），奖励 远古合金×5~8 / 远古碎晶×2~3，稀有档给数据核心×1；财富 15 万+开始刷 | +7 以上强化材料的任务通道（不打仗玩家的替代路径）；中 |
| 后期 | 折跃贸易舱 | 传送台新分支：把售货舱折跃到友好定居点即时成交（**注意 1.7 地图小人 Dialog_Trade(settlement) 必 NRE，须走商队/世界交互路径，不可直接开交易窗口**） | 传送次数新出口；中 |
| 后期 | 黑市中间人 | 稀有事件：中间人收购远古合金（数据核心仍不可交易，保住闭环），售价 ×2 但 -5 好感 | 独占资源有限出口；低 |
| 终局 | 星际交易所 | 奇观激活后解锁：周期性大订单（反物质/α聚合物换 远古碎晶），探索之外的碎晶第三通道 | 与"贸易纪元"增益叠加；中 |

### 11.2 装备（从多到深）

| 阶段 | 候选 | 机制 | 咬合与成本 |
|---|---|---|---|
| 前期 | 工具三件套 | 矿镐（采矿+10%）、手锯（伐木+10%）、泥刀（建造+10%）：装备后给 stat offset 的"武器"，铁/木造价 | 填补 MMT 无工具类空白；低 |
| 前期 | 简易护甲链 | 皮甲→铆接铁甲（stuff 体系，皮革/钢铁），填补原版与 MMT 未来甲之间的空档 | 皮革/钢铁；低 |
| 中期 | 模块化工装腰带 | `MMT_ToolBelt`：Apparel 挂模块槽（复用 ModuleSystem），可插建造/采矿/种植/搬运模块（各 +8%） | 模块体系自然延伸；中 |
| 中期 | 背包系列 | 帆布包/登山包：携带容量 +40/+80（`CarryCapacity`? 无此 stat——用携带重量 hediff 或 PawnCapacity 偏移实现，需验证 1.7 可行性） | 布料/皮革；中 |
| 中期 | 防具强化线 | 见 §10.2：强化台第二容器，护甲 +1~+5 | 碳合金/α聚合物；中 |
| 后期 | 品质重铸台 | `MMT_ReforgeBench`：消耗合金把武器/护甲品质重铸一档（一般→良好→极佳，极佳→大师要远古合金） | 合金持续消耗；中 |
| 后期 | 套装系统 | 穿齐同系列 3 件（头/身/腰）触发套装 hediff（如"未来作战套装"：射击+1、移动+5%）；检测走 apparel 扫描 tick | 引导玩家集齐全链装备；中 |
| 终局 | 远古装备 | 据点独占掉落（不可制造）：远古步枪/远古战甲，基础强于未来系，只能经强化台继续升级 | 探索奖励闭环；中 |
| 终局 | 奇点共鸣 | 奇观激活后，全图 +8 以上强化武器获得共鸣 glow +5% 伤害（奇观与强化线的联动彩蛋） | 终局整合；低 |

### 11.3 自动化（从单机到产线）

> **自动化设计规则（避免功能重复）**：现有无人值守设备已覆盖 种植/收获（自动种植机）、奶/毛/蛋（畜牧机器）、矿石研磨/筛分（自动研磨机/自动筛子+进料口）、采石（自动采石机）、结晶培养（晶体培养舱）。**新候选不得与上述职能重叠；同类需求一律做成现有设备的模块/升级档位，不新增设备。**

| 阶段 | 候选 | 机制 | 咬合与成本 |
|---|---|---|---|
| 前期 | 手动档补全核对 | 逐链核对"手动→电动→自动"过渡档是否齐全，缺的手工版补齐 | 平滑曲线；低 |
| 中期 | 自动烹饪机 | AutoProcessor 模式：输入生食材，定时出简单餐/精致餐（现有设备无烹饪职能） | 电力+食材；中 |
| 中期 | 自动制药机 | AutoProcessor 模式：草药→医药/药剂（现有为电动制药台，无自动版，职能不重叠） | 医药链；低 |
| 中期 | 畜牧机器·屠宰扩展 | **不新增设备**：给现有畜牧机器加"屠宰模式"开关（动物老死后自动屠宰出肉/皮），补上畜牧机器未覆盖的最后一环 | 复用 CompLivestockMachine；低 |
| 中期 | 集中控制台 | `MMT_AutomationConsole`：一个 ITab 列出全图 AutoProcessor 系建筑的开关/进度/模块状态（免跑路巡检；管理工具非生产设备） | 机器人部件；中 |
| 中期 | 堆垛无人机坞 | 小型飞行无人机：自动把指定区域产物搬到储存区（现有设备只管生产不管搬运，职能不重叠） | 机器人部件；高 |
| 后期 | 炮塔自动装填机 | 相邻储存区的炮弹自动补给到 MMT 炮塔（检测弹药 comp 缺口，定时消耗储存） | 弹药链闭环；中 |
| 后期 | 自动采石机·深层升级 | **不新增设备**：给现有自动采石机加升级模块/档位（消耗高级合金+机器人部件升级为"深层模式"，直采地下矿脉，产能翻倍） | 复用 MMT_AutoChunkDrill；中 |
| 后期 | 流水线联动 | 相邻 AutoProcessor 建筑自动把产物推送进下游建筑输入仓（"传送带 lite"：不新增物流实体，只在 tick 里做容器间搬移） | 全链自动化质变；高 |
| 终局 | 奇点供能自动化 | 奇观"能源纪元"激活后，全图自动设备功耗 -25%（与增益联动的第二条自动化福利） | 终局整合；低 |
| 终局 | 风暴博弈 | 能量风暴中未防护自动设备停摆（§5 点名逻辑自然覆盖），阻尼器保护产线成为后期布局课题 | 威胁×自动化交叉；随风暴附带 |

---

## 12. 性能设计原则（全设计通用，性能优先）

> 本设计所有 C# 实现必须遵守以下原则；新增候选条目（§10/§11）实现时同样适用。

### 12.1 Tick 与周期逻辑

- **禁止每 tick 轮询**：所有周期逻辑用 `CompTickInterval(delta)` + 自定义间隔（参考 `BuildingBattery` 的 250 tick 节流惯例），间隔内只累加计数器，不做任何查询。
- **扫描类逻辑下限**：全图 pawn/建筑扫描（风暴点名、启迪 hediff 刷新、套装检测）间隔 ≥2500 tick（游戏 1 小时），且只扫缓存列表不扫 `listerThings` 全表——建筑 comp 在 `PostSpawnSetup/DeSpawn` 时注册/注销到 Map 级静态缓存（`Dictionary<Map, List<...>>`，MapRemoved 时清理）。
- **World 级逻辑**（扫描仪进度、据点过期）挂 `WorldComponent.Tick` 的长间隔分支，不进 Map tick。

### 12.2 Harmony 补丁热点

- **允许打的方法**：`GetDamageAmount`/`AdjustedBaseMeleeDamageAmount`（每次攻击一次，可接受）、发电/生长 postfix（纯乘法）。
- **禁止打的方法**：任何 per-tick 或 per-frame 方法（`Tick`、`Draw*`、pawn 移动/寻路、`GetGizmos` 不加锁分配）。
- **过滤要廉价**：postfix 首行过滤用 `def.defName` 缓存的 `HashSet<Def>`（预建 `MMT_` 前缀集合），**禁止**在热点里做字符串 StartsWith/Contains（每次调用都分配）。
- 补丁结果可缓存则缓存（如装备 comp 引用在 postfix 内 `TryGetComp` 结果写入 `ConditionalWeakTable` 或挂到投射物的扩展字典，避免重复反射式查找）。

### 12.3 内存与分配

- 热点路径零 LINQ、零装箱；gizmo/ITab 字符串用缓存字段，不在 `get` 里拼接。
- `GetGizmos`/`GetInspectString` 允许分配（低频），但内部循环不得嵌套查询。
- 随机池/权重表（仿 `OreSiftPool`）启动时一次构建，运行期只读。

### 12.4 渲染

- 奇观 glow、充能光柱等叠加层用单个 `Graphic` 复用 + 矩阵变换，禁止每帧 `GetGraphic`；动画状态用 tick 计数取模，不开定时器。
- 实时绘制建筑一律 `drawerType RealtimeOnly`（1.7 BuildingBase 默认 MapMeshOnly 会烘焙进静态网格，动画不生效且重建网格很贵——已踩过的坑）。

### 12.5 各条目的性能注记（对 §10/§11 的修正）

| 条目 | 性能修正 |
|---|---|
| 套装系统（§11.2） | apparel 扫描改 2500 tick 一次且只扫殖民者列表，不逐 tick |
| 流水线联动（§11.3） | 容器间搬移挂 250 tick 间隔 + 每次最多搬 N 组，避免大图卡帧 |
| 堆垛无人机坞（§11.3） | 高成本标注不变：若实现，寻址用 `StoreUtility` 缓存槽位，禁止每 tick `ClosestThingReachable` |
| 集中控制台（§11.3） | ITab 打开时才收集状态，后台不跑任何轮询 |
| 风暴点名（§5） | 候选列表由建筑 comp 注册/注销维护，点名时从列表随机取，不扫图 |
| 精英爆破手/黑市事件 | 走原版 Incident 管线（自带频率控制），不加自定义 ticker |
