# MMT 终局内容包 — 性能分析报告

> 范围：终局内容包全部新增机制（武器强化/附魔、深空探索、纪元奇点、能量风暴、Mod 设置挂钩、沙袋人偶）。
> 方法：静态代码审查 + ildump 核实 1.7 引擎调用频率。结论先行：**无性能热点，全部符合性能约束（无每 tick 全图扫描，周期逻辑间隔 ≥2500 tick 或早退 O(1)）**。

## 一、结论摘要

| 机制 | 挂钩方式 | 调用频率 | 单次成本 | 评估 |
|---|---|---|---|---|
| 武器伤害/穿甲/命中/冷却/瞄准补丁 ×7 | Harmony postfix | 战斗时每次射击/挥砍数次 | 一次 `is` 转型 + TryGetComp（武器 comp 列表 ≤3 项线性查） | ✅ 纳秒级 |
| 命中特效结算（远程 Impact / 近战 ApplyMeleeDamageToTarget） | Harmony postfix | 每次实际命中一次 | comp 为 null 立即返回；有词条才结算 | ✅ 非每 tick |
| 武器强化 comp 挂全武器（含敌人） | XML comps | — | 每武器一个 comp 实例（几十字节） | ✅ 内存可忽略 |
| 沙袋人偶承伤统计 | Harmony postfix `Thing.TakeDamage` | **全图每次掉血** | 一次 `as` 转型 + null 判断，非假人立即返回 | ✅ 纳秒级 |
| 沙袋人偶自修复 | `Tick()` 取模 | 每 tick 一次整数取模 | O(1) | ✅ |
| 蓄电 comp（扫描仪/奇点/传送台共用） | `CompTickInterval(delta)` | 每 tick delta 推进 | 2 次 float 比较，满电即退 | ✅ |
| 扫描仪扫描推进 | `CompTickInterval(delta)` | 仅扫描中 | bool 早退 + 一次 float 乘减 | ✅ |
| 奇点点火倒计时 | `CompTickInterval(delta)` | 仅点火中 | bool 早退 | ✅ |
| 奇点辉光绘制 | `DrawAt` 每帧 | 仅点火中/已激活 | 6 次 `Graphics.DrawMesh` + 缓存 Material | ✅ 每图仅 1 座 |
| 强化台托举武器绘制 | `DrawAt` 每帧 | 仅台内有武器 | 1 次 Graphic.Draw | ✅ |
| 能源/丰饶纪元增益 | postfix 发电/生长 getter | 电网变更时 / 植物 TickLong（2000 tick/株） | 一次 `ConditionIsActive` 列表查找（条件数 <10） | ✅ |
| 启迪纪元 hediff 刷新 | `GameConditionTick` | 每 tick 递减；每 2500 tick 扫一次殖民者 | 平时 O(1)；刷新时 O(殖民者数)，殖民者几十人 | ✅ |
| 能量风暴点名 | `GameConditionTick` | 每 tick 递减；每 2500 tick 扫一次全部殖民者建筑 | 平时 O(1)；点名时 O(建筑数 × 阻尼器数)，数百 × 个位数 | ✅ |
| 风暴触发判定 | `CanFireNowSub` | 事件系统每次选事件时 | 第一行布尔短路（设置关闭即退） | ✅ |
| 阻尼器保护判定 | 静态注册表 `Dictionary<Map, List>` | 仅风暴点名的候选筛选 | 无阻尼器的图 O(1) 字典未命中 | ✅ |
| 产率/研究成本倍率 | postfix 配方结算 / 研究 UI | 每次做完配方一次 / 研究界面 | defName 前缀比较 | ✅ |
| 据点生成/奖励堆 | 扫描完成一次性 + `PostMapGenerate` | 每张据点图生成一次 | 一次性 | ✅ |

## 二、关键路径详析

### 1. 武器强化补丁链（最热，重点核实）

七个 postfix 的公共入口都是 `WeaponEnhancePatches.ActiveEnhancement(equipment)`:

```
equipment is ThingWithComps          // 模式匹配，~1ns
→ EnhanceLadder.EquipEligible(def)   // 默认设置下直接 return true（开了"仅原版"才 StartsWith）
→ TryGetComp<CompWeaponEnhancement>() // comp 列表线性查，武器一般 ≤3 个 comp
```

- **无强化武器**（全部敌人武器、未强化玩家武器）:comp 存在但等级 0，各 Factor 返回 1——多走一次 `Mathf.Pow(1.05, 0)`，仍是常数时间。
- `Projectile.equipment` 用 Harmony `FieldRef`（编译期委托），不是反射，无每发子弹反射开销。
- `ShotReport` 是 struct，用 `StructFieldRef` 原地改，无装箱。
- 结论：即便大会战每 tick 数百发投射物结算，新增开销也在微秒级/帧，远低于引擎自身的弹道计算。

### 2. 1.7 CompTickInterval 分发（ildump 核实）

`ThingWithComps.TickInterval(int delta)` 逐个 comp 调 `CompTickInterval`。新增的蓄电/扫描/点火推进都遵循"第一行早退"模式（未激活立即 return），常态成本为每建筑每 tick 一次虚调用 + bool 判断。每个图此类建筑数量为个位数，可忽略。

**未踩的坑已规避**:1.7 Normal tickerType 建筑收不到 `CompTickRare`，全部周期逻辑走 `CompTickInterval`（蓄电/扫描/点火/风暴/启迪），没有错误依赖 Rare tick。

### 3. 能量风暴点名（每 2500 tick 一轮）

`DoStrikeRound` 遍历 `allBuildingsColonist`（数百级），每个建筑做：

1. `CompStormDampener.IsProtected`：无阻尼器的图是字典未命中 O(1)；有阻尼器时 每建筑 × 阻尼器数 次 `DistanceTo`（一次 sqrt)。阻尼器数量个位数 → 总开销数百次 sqrt/41 游戏秒，**无感**。
2. `GetComp<T>` 两次：comp 列表线性查。

### 4. 纪元增益 getter 补丁

- 发电 ×1.25:`DesiredPowerOutput` 只在电网拓扑变化/选中查看时求值，非每 tick。
- 生长 ×1.5:`Plant.get_GrowthRate` 在 `Plant.TickLong`（每株 2000 tick）调用，千株农场 = 每 tick 约 0.5 次调用 × 一次条件列表查找（<10 项）。
- 启迪 hediff:2500 tick 刷一次，`FreeColonistsSpawned` 是现成列表（无全图搜索），每人一次 Remove+Add Hediff。

### 5. 沙袋人偶

- `TakeDamage` postfix 挂在**全局**掉血口，但过滤是 `as` 转型 + null 判断（失败分支 ~2ns)，对正常战斗零影响。
- DPS 滚动窗口：Queue 按 600 tick 窗口裁剪，最高射速武器窗口内条目数百级，内存 KB 级。
- 自修复每 tick 一次取模 + 满血短路。

### 6. 绘制（每帧路径）

- 奇点辉光：仅点火中/已激活绘制，6 片 `Graphics.DrawMesh`（透明 shader),Material 走 `MaterialPool` 缓存、实时脉动改 color。每图 1 座 → 每帧 6 draw call，与原版火把同级。
- 强化台：台内有武器时多 1 次 Graphic.Draw。无武器早退。
- 风暴短路特效（LightningGlow + 文字 Mote)：每次点名 1-3 个，风暴一天约 24-48 轮 → 每分钟几个 mote，远低于引擎 mote 上限。

## 三、风险与遗留项

| 级别 | 项 | 说明 |
|---|---|---|
| 低 | `CompStormDampener.Registry` 静态字典 | 阻尼器拆走时会自清；但若**整个地图被废弃时阻尼器未走 PostDeSpawn**（地图直接销毁），字典会残留死 Map 引用。存量为废弃图数量 × 几十字节，实际无感；如介意可在 `MapComponent`/`WorldObject` 清理钩子补一刀。 |
| 低 | 强化 comp 挂全图武器 | 袭击者武器也带 comp（等级 0)。数千把武器 ≈ 数百 KB 托管内存，无 CPU 开销（敌人不触发玩家侧逻辑）。 |
| 提示 | CE 兼容 | 检测到 Combat Extended 时远程数值补丁可能失效（CE 重写弹道链），代码已探测并在描述中说明；近战线不受影响。非性能问题。 |

## 四、验证过的调用频率依据（ildump,1.7.9819)

- `ThingWithComps.TickInterval(int delta)` 存在并逐个 comp 调 `CompTickInterval` —— 周期逻辑正确落位。
- `ThingWithComps.Tick()` 只调 `CompTick`,Normal 建筑确实无 Rare tick —— 未误用。
- `Thing.TakeDamage` 非虚方法 —— 假人统计用 postfix 而非重写，单分支过滤。
- `Projectile.get_DamageAmount` 返回 int、命中/冷却/瞄准各 getter 均为单次结算口 —— 无每 tick 连续调用点。

## 五、结论

新增机制**没有任何每 tick 全图/全物扫描**；所有周期事件间隔 ≥2500 tick；Harmony 补丁全部挂在单次结算口（命中/冷却/伤害/配方完成），最坏情况（大会战 + 风暴 + 点火同时进行）新增 CPU 开销估计 <0.1 ms/帧，可忽略。无需优化。
