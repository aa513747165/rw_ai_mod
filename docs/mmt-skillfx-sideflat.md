# MMT 侧视平躺序列帧（SideFlat/SideRamp）设计文档

> 状态：两条原型已落地（第二十六步「测试·地心火柱·平躺」单帧版、第二十七步「测试·地心火柱·坡道」
> 多片坡道版），**游戏内视觉效果待验证**。
> 实现：`Source/MyModeTest/SkillFX/SkillSideFlat.cs`、`Source/MyModeTest/SkillFX/SkillSideRamp.cs`；
> 载体 Def：`1.7/Defs/ThingDefs_Misc/MMT_SkillSideFlat.xml`、`MMT_SkillSideRamp.xml`；
> 演示条目：`1.7/Defs/Misc/MMT_SkillDemoSet.xml`；素材规范：本文档 + `docs/mmt-skillfx-texture-prompts.md` 第十五节。

## 一、要解决的问题

1.7/Waylith 下侧视序列帧素材（底部贴地、顶部朝上的"立起来"构图，如火柱、爆发、刀光）**在世界空间没有竖直呈现手段**。经逆向 Waylith/UnityStubs/RimWorld.dll 证实（2026-11-27，产物在 `docs/temp/waylith_rev/`）：

- Play 场景主相机是**正交相机 + 旋转 Euler(90,0,0)**（正上方俯视，`RimWorld.dll` 的 `BuildPlayScene` 创建）。
- DrawMesh 全链路（`MeshDrawSubmission → DrawItemFactory → MeshBatchRenderer`）**无任何逐 draw 视锥/遮挡剔除**（OcclusionQuery/HiZ 仅服务游戏主动注册的盒子），光栅化参数全部正常。
- 因此竖直 quad（plane10 绕 X ±90° 立起）的屏幕投影**退化为零面积线段、光栅化无片元**——不是被剔除，而是数学上不可见；Harmony 补丁无法"恢复"竖直渲染，做竖直效果只有 billboard（正交俯视下面向相机=又是水平，失去意义）或绕行方案。
- 已证伪的路线：竖直 quad 立起（不渲染）；水平切片堆叠按**真实 Y 高度**画（屏幕上塌成一条带）。

## 二、原理：屏幕竖直轴 = 世界 +Z（北）

90° 正上方正交相机下，**地面与屏幕 1:1 无失真映射**：屏幕上 = 世界 +Z（北），屏幕右 = 世界 +X（东）。把侧视素材**整帧平铺在水平 quad（`plane10 + Quaternion.identity`，旋转锁 0°）**上，贴图的"上"边就朝屏幕上方——躺倒的侧视图在屏幕上看起来就是竖立的侧视图。

- 无需任何预拉伸/补偿（正交 + 90°，无透视缩短）。
- 贴图横向 ↔ 世界 X（格数 = `sideFlatWidthCells`），贴图纵向 ↔ 世界 +Z（格数 = `sideFlatHeightCells`），非方帧按宽高比给格数即可。
- 锚点语义：贴图**底边中点**钉在目标格中心（quad 中心沿 +Z 偏 `heightCells/2`）。

### 原版背书：龙卷风就是这么画的

原版 `Tornado`（`Assembly-CSharp` 的 `Tornado` 类，`DrawTornadoPart`）整个漏斗由 180 片**水平 quad** 组成，没有任何竖直 quad：

1. 每片的"高度序号"（1~10）×4 直接映射成**向北 Z 偏移**（底部经 bias 归零、顶部伸出锚点以北 ~36 格），世界 Y 只加 0.036 的随机抖动用于排绘制次序、防 z-fighting；
2. 水平半径按高度平方收缩（宽顶窄尾）形成漏斗轮廓；
3. 每片绕中心公转 + quad 绕 Y 自转（绕 Y 旋转保持 quad 水平，合法），按公转角度做 alpha 衰减伪造前后遮挡；
4. 覆盖 `DrawSize`（45×100）防动态绘制按绘制半径剔除（碎片伸出锚点太远时必需）。

结论：**侧视平躺不是邪道，是原版官方做法**。龙卷风的"高度"全在 Z 轴上，Y 轴不参与——这也是之前水平切片堆叠失败的根因（堆错了轴）。

## 三、三条侧视呈现路线对比

| | 屏幕空间（第二十五步 SkillRiseFX） | 平躺单帧（第二十六步 SkillSideFlat） | 多片坡道（第二十七步 SkillSideRamp） |
|---|---|---|---|
| 通道 | `Mote.DrawGUIOverlay` UI 层贴图 | 世界空间水平 quad（单张整帧） | 世界空间 N 片水平 quad 沿 Z 坡道分布 |
| 地图光照 | 不吃 | **吃** | 吃 |
| 遮挡 | 无（永远画在世界物体之上） | 按 AltitudeLayer 整体判定 | 按带独立微抬 Y 排序 + 顶部 alpha 衰减，遮挡感最好 |
| 朝向 | 屏幕固定 | 锁死屏幕北（绕 Y 转即穿帮） | 锁死屏幕北（带内贴图不可绕 Y 转；摆动/收放是平移缩放，不破坏构图） |
| 体积感/运动层次 | 弱 | 弱 | **强**（锥形收放+摆动+脉冲，原版 Tornado 同款思路） |
| 适用 | 单点爆发、UI 感技能 | 中小尺寸单点特效 | 火柱/风暴/光柱等需要扭动体积感的特效 |

## 四、实现架构（SkillSideFlat.cs）

- `Thing_MMTSkillSideFlat`：Attachment 类无碰撞 Thing，`drawerType=RealtimeOnly`，`altitudeLayer=MoteOverhead`。参数运行时注入**不进存档**，读档后自毁（特效最长数秒，可接受）。
- `DrawAt` 每帧：
  1. 帧选择：横向等宽图集，用**共享材质的 `mainTextureScale/Offset`** 切当前帧（材质按 `贴图#帧号` 全局缓存，多实例零分配）；
  2. 淡入淡出：`fadeInSeconds`/`fadeOutSeconds` 线性调 alpha；
  3. 位置：`Position + Z 偏移（heightCells/2 + driftCells×progress）`——底边锚定 + 向北漂移模拟上升；
  4. 绘制：`Graphics.DrawMesh(MeshPool.plane10, TRS(PerspectiveCompensate(pos), Quaternion.identity, (widthCells, 1, heightCells)))`，颜色走 `MaterialPropertyBlock`（共享材质防互覆），`ShadowCastingMode.Off`。
  - 注：1.7/Waylith 的 `MeshPool.plane10` 是 **1×1 平面**（非旧版 10×10），缩放直接写格数。
- 视觉参数（尺寸/帧速/漂移/染色/淡入淡出）**全部从 XML 透传**（`MMTSkillDemoEntry.sideFlat*` 字段），C# 仅兜底默认值。

## 六、坡道变体：多片坡道堆叠（SkillSideRamp.cs，第二十七步）

把侧视帧切成 N 条水平带（U 切帧列、V 切条带，`mainTextureScale=(1/帧数, 1/N)`），每带一片独立水平 quad 沿 +Z 坡道排开。带中心 z = 锚点 + t×heightCells + 漂移，带宽 = widthCells × lerp(1, topScale, t) × 呼吸脉冲，带深 = heightCells/N——**静止时 N 带无缝拼回整帧**（与单帧平躺同图），运动时每带独立：

- **锥形收放**：宽度按高度线性收至 `topScale`（漏斗/火柱轮廓）；
- **横向摆动**：振幅按 t² 增至顶部 `sway` 格，周期 `swayPeriodTicks`，带间相位错开（i×0.83）+ 实例 seed；
- **呼吸脉冲**：宽度 ±`pulseAmp` 起伏（相位与摆动错开），0=关闭；
- **遮挡衰减**：alpha 按高度线性降至 `topAlpha`（龙卷风式伪造前后层次）；
- **绘制次序**：世界 Y 逐带 +0.004 微抬（不承担高度，只排序防 z-fighting）。

参数全走 XML（`sideRamp*` 字段，技能侧 `MMTSkillSideRampUtil.Fire`）：

| 字段 | 含义 | 备注 |
|---|---|---|
| `sideRampSlices` | 切片数 N（≥2） | 材质缓存按 (贴图,帧,带,N) 共享，N≤12 建议 |
| `sideRampSway` / `sideRampSwayPeriodTicks` | 顶部摆动幅度（格）/ 周期（tick） | 底部不摆、按 t² 增大 |
| `sideRampTopScale` | 顶端宽度系数 | <1 锥形收口，1=直筒 |
| `sideRampTopAlpha` | 顶端 alpha 系数 | <1 顶部淡出 |
| `sideRampPulseAmp` | 呼吸脉冲幅度（0~1） | 各带相位错开 |

其余字段（贴图/帧/宽高/漂移/染色/淡入淡出）与 `sideFlat*` 同义。摆动/收放/脉冲是平移与缩放，不破坏侧视构图；但带内贴图仍不可绕 Y 旋转。

## 七、使用指南（XML）

演示条目（`MMT_SkillDemoSet.xml`，`demoType="sideFlat"`）字段：

| 字段 | 含义 | 备注 |
|---|---|---|
| `sideFlatTexPath` | 横向等宽序列帧图集 | 侧视构图：底部贴地、顶部朝图上方 |
| `sideFlatFrameCount` / `sideFlatFrameTicks` | 帧数 / 每帧 tick | 图集按帧数等宽切片 |
| `sideFlatLoop` | 循环 or 播完停末帧 | |
| `sideFlatDurationSeconds` | 总时长 | 到期自毁 |
| `sideFlatWidthCells` / `sideFlatHeightCells` | 宽（世界 X）/ 高（世界 +Z 北）格数 | 按贴图宽高比给；锚点=底边中点 |
| `sideFlatDriftCells` | 生命周期内向北漂移格数 | 模拟上升感，0=钉在原地 |
| `sideFlatTint` | 染色 | alpha=0 或未写 = 纯白（帧图自带颜色时保持） |
| `sideFlatFadeInSeconds` / `sideFlatFadeOutSeconds` | 首/尾淡入淡出 | 0=生硬出现/消失 |

pawn 技能侧复用：`MMTSkillSideFlatUtil.Fire(map, cell, MMTSkillSideFlatParams)`。

新条目落地前先在 XML 注释写"效果设计卡"（宽/高格数、是否漂移、锚点、参考物），逐项对照实现。

## 八、素材规范（详见 prompts 文档第十五节）

1. 侧视构图，**底部贴图底边中点锚定**、顶部朝图上方（平躺后=朝北=屏幕上），不做任何旋转；
2. 透明底 RGBA，透明区 RGB 预乘为黑（防白边），additive 友好；
3. 横向等宽图集（或按 `MMT_FlameErupt` 惯例拆单帧文件，需小改 C# 支持贴图列表）；
4. 高度建议 ≤4 格：画在 MoteOverhead 层（高于 pawn），过高的图会把北面数格内的小人/建筑整片盖住；
5. 素材里不要画"朝南的背面"，永远屏幕正视。

## 九、已知限制与对策

| 限制 | 原因 | 对策 |
|---|---|---|
| 朝向锁死屏幕北 | 贴图方向即世界方向，绕 Y 旋转会把图在地面平面内转歪 | 对称/径向构图素材可绕 Y 转（龙卷风即如此）；侧视素材不转 |
| 遮挡按 Altitude 整片判定 | 整帧画在 MoteOverhead（高于 pawn） | 控制高度 ≤4 格；或抄龙卷风按"高度带"做 alpha 衰减；大型特效改多片坡道路线 |
| 伸出锚点过高时被相机剔除 | 原版动态绘制按 DrawSize 做相机外剔除 | 高度/伸出 >5 格时覆盖 `DrawSize`（龙卷风做法） |
| 吃地图光照 | 世界空间绘制（相对屏幕空间版的优势，但在暗区会变暗） | 需要自发光感时换 TransparentPostLight 之外的 shader 或调 tint |

## 十、验证清单

- [ ] 游戏内「测试·地心火柱·平躺」呈现竖立侧视火柱（顶部朝屏幕上），序列连贯、淡入淡出无跳变；
- [ ] 游戏内「测试·地心火柱·坡道」静止帧与平躺版同图（无拼缝/错位），运动时锥形收放、摆动、脉冲、顶部衰减呈现火舌舔动的体积感；
- [ ] 与「测试·地心火柱」（屏幕空间版）同格对照：平躺/坡道版吃光照、遮挡按 Altitude 的差异符合预期；
- [ ] 游戏内「测试·圣光审判」（`delayedStrike` + 坡道光柱皮肤）：预警圈 → 延迟 → 光柱在落点立起（顶部朝屏幕北=朝天、顶端淡出延伸出屏幕、向南压落）→ 到点爆炸；首个坡道实战皮肤，顺带验证坡道本体；
- [ ] 验证通过后更新 AGENTS.md 渲染硬约束条目（把"唯一绕行出路"改为多条），并在本文档标记已验证。

## 十一、已接入的实战皮肤

- **圣光审判**（`delayedStrike` 的坡道光柱皮肤，2026-11-27）：`MMTDelayedStrikeParams.pillarRamp` / `CompProperties_MMTDelayedStrike.pillarRamp*` 字段，演示侧 `meteorUsePillarRamp=true` + 复用 `sideRamp*` 字段；光柱寿命强制对齐 `fallTicks`，坠落计时与撞击爆炸仍由陨石实体负责。演示「测试·圣光审判」，参数见 `docs/mmt-class-priest.md` §3.12。
