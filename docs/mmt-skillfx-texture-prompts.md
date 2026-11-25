# MMT 技能特效框架 — AI 出图提示词集（喂图模型用）

> **状态**：§一 技能光柱、§二 火焰柱、§三 光环圆盘、§四 增益粒子、§五 位移特效、§六 范围选择圆、§七 召唤特效 **均已出图完成**（2026-11-25，AI 终稿覆盖占位图，Def/代码零改动）。
> §九 瞄准预览圆环 `MMT_TargetRing` **待出图**（现用 §六 旧图副本占位，出图后直接覆盖同名文件，代码零改动）。

> 与 `mmt-dungeon-texture-prompts.md`、`mmt-fireboss-texture-prompts.md` 同格式。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**。

## 背景

技能特效框架（`SkillBeam.cs` 持续射线）需要一张**可拉伸的光柱贴图**：代码将贴图沿光束方向拉伸成四边形并整体染色（`beamTint`），所以贴图本体必须是**白色/灰度**（染色交给代码），alpha 通道决定光柱的实心与边缘衰减。

**UV 约定（关键）**：贴图的**纵轴（V，图片高度方向）= 光束长度方向**，横轴 = 光束宽度方向。即图片是一根**竖直的光柱**，左右边缘透明，垂直方向上下贯通、两端不需要收口（射线两端由代码裁切，出口另有光晕）。

## 〇、全局风格前缀与负面词

**风格前缀（FX_STYLE，特效贴图专用）**：

```
RimWorld video game VFX sprite, stylized hand-painted energy effect, soft
cel shading with smooth glow falloff, white and light-gray grayscale only
(tint applied by engine), pure black background, clean edges, no scene no
objects, symmetrical around the vertical center axis
```

**负面词（NEGATIVE）**：

```
text, letters, watermark, logo, color, rainbow, photorealistic, photograph,
3D render, scene, ground, character, weapon, particles outside the beam
```

**本包补充约束**：

- 出图为**黑底白图**：光柱最亮处纯白 `#FFFFFF`，向外渐隐到纯黑；落盘前按"亮度 → alpha"转成 RGBA 透明底（黑=全透明，白=不透明），RGB 统一填白；
- 横向（宽度方向）必须**左右对称**，中心轴为最亮核心；
- 纵向（长度方向）可以有能量流动纹理（波纹/ streaks），但**不能断流**——任意横截面的核心 alpha 需保持 ≥ 0.85，否则拉伸后光柱会像断成几节；
- 四边 1-2 像素保持全透明/全黑，避免拉伸时出现接缝亮边。

---

## 一、技能光柱 `MMT_SkillBeam`（1 张，64×256）

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_SkillBeam.png`（texPath `Things/Mote/MMT_SkillBeam`，ThingDef `MMT_SkillBeam` 与 `MMTBeamParams.texPath` 默认值均指向它，覆盖即用）。
- **功能**：持续射线（demoType="beam"，假人「测试·烈焰射线」）的光柱本体；代码按 `beamTint` 染色（演示为橙红烈焰色），宽度呼吸抖动 + 淡入淡出。

```
FX_STYLE + a vertical continuous laser / energy beam column filling the
full image height: an intense pure-white hot core line 4-6 px wide on the
exact horizontal center, wrapped in a soft cylindrical glow that fades
smoothly to black toward both left and right edges, with subtle wavy
energy undulations and faint inner filaments flowing along the length,
brightness uniform from top to bottom, no tapering at the ends
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸保持 **64×256**（宽度必须 2 的幂，高度建议 256）；
2. 中心列（x=32 附近 ±3px）alpha ≥ 0.9 且整列连贯无暗点（断流检查：把图纵向拉到 20 格长看是否出现横断纹）；
3. 左右边缘 2px alpha = 0；上下边缘允许非零（光束贯通）但四角应趋近 0；
4. 灰度：RGB 三通道一致（无彩色倾向），染色效果交给 `beamTint`；
5. 游戏内验证：Dev 模式对假人用「测试·烈焰射线」，光柱应为平滑橙红光柱，无明显横向条纹/接缝。

---

## 二、火焰柱 `MMT_FlamePillar`（1 张，128×128）【已出图完成】

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_FlamePillar.png`（texPath `Things/Mote/MMT_FlamePillar`，ThingDef `MMT_GroundFlame` 与 `MMTGroundFireParams.texPath` 默认值均指向它，覆盖即用）。
- **功能**：地面技能（demoType="groundFire"，假人「测试·烈焰喷发」单柱 /「测试·烈焰火海」成片）的火焰本体；代码按 `groundTint` 染色（演示为橙黄烈焰色），双层四边形叠绘（底层火身 + 上移摆动的火舌）+ 呼吸闪烁 + 淡入淡出。
- **构图**：单根**竖直火焰舌**，纵轴（图片高度方向）= 火焰向上方向：底部为宽圆火根（宽约图幅 40%），向上收窄成摆动火尖，顶尖在图幅顶部附近收束；左右对称度不要求（自然火焰允许轻微偏向），但横向重心须在中心轴附近。

```
FX_STYLE + a single vertical flame tongue rising from a wide rounded base
at the bottom of the image: an intense pure-white hot core at the center
of the base, soft glowing body tapering upward into a gently wavering
pointed tip near the top edge, smooth brightness falloff from core to
transparent edges, a few faint inner filaments curling upward, the flame
occupies the lower two-thirds firmly and thins toward the tip, no ground
no embers outside the flame silhouette
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **128×128**；
2. 底部火根中心 alpha ≥ 0.9（火身要实心），火尖与四边 2px alpha = 0；
3. 灰度：RGB 三通道一致（无彩色倾向），染色交给 `groundTint`；
4. 火焰剪影重心在横向中心 ±8px 内（成片火海时不会集体偏向一边）；
5. 游戏内验证：Dev 模式对假人用「测试·烈焰喷发」（单柱高大火柱）与「测试·烈焰火海」（成片自中心向外蔓延），火焰应为橙黄贴图火而非原版火焰闪烁图。

---

## 三、光环圆盘 `MMT_AuraDisc`（1 张，128×128）【AI 符文版在用，低透明度衬底】

> **2026-11-26 两轮迭代结论**：① AI 符文阵版外圈 alpha≈255 硬环，高透明度（0.9）下呈"齿轮硬环"；
> ② 程序柔光版（`docs/temp/gen_aura_soft.py`）预乘暗 RGB 的内部填充在 TransparentPostLight 半透明混合下把地面压暗成泥浆色，弃用。
> **现用方案**：AI 符文版（预乘黑底）+ 代码透明度压到 0.5，符文盘只作柔和衬底。
> 教训：TransparentPostLight 是 alpha 混合而非纯加法——贴图内部填充的暗 RGB 会压暗地面，柔光类填充贴图慎用预乘。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_AuraDisc.png`（texPath `Things/Mote/MMT_AuraDisc`，ThingDef `MMT_SkillAuraBuff` 与 `MMTAuraParams.discTexPath` 默认值均指向它，覆盖即用）。
- **功能**：增益/减益光环（demoType="aura"，假人「测试·迅捷光环/迟缓光环」）铺在目标脚下的地面光圈；代码按敌我配色染色（友方绿/减益红），呼吸脉冲 + 淡入淡出，另有 GenDraw 描边圈。
- **构图**：**正圆形地面符文光圈**俯视图：外圈亮环位于半径约 80% 处，环内可有符文/魔法阵刻线纹理（径向对称），中心区域弱填充，图片四边全透明；亮环是视觉主体。

```
FX_STYLE + a circular magic rune circle seen directly from above: a bright
glowing ring at about 80% of the radius as the main visual, with faint
radial runic glyphs and thin connecting arc lines inside the ring, a very
dim energy fill toward the center, perfectly centered and radially
symmetric, brightness falling to pure black outside the outer ring
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **128×128**；
2. 亮环位置：距中心 78%-85% 半径处为最亮（alpha ≥ 0.85），环闭合无缺口；
3. 四边 2px alpha = 0（圆不顶到图边，缩放后不出硬边）；
4. 灰度：RGB 三通道一致，染色交给代码（友方绿/减益红）；
5. 游戏内验证：Dev 模式假人「测试·迅捷光环」，脚下应为绿色呼吸光圈，范围与技能半径一致。

---

## 四、增益/减益粒子 `MMT_BuffSpark`（1 张，64×64）【已出图完成】

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_BuffSpark.png`（texPath `Things/Mote/MMT_BuffSpark`，FleckDef `MMT_Fleck_BuffSpark` 与 ThingDef `MMT_BuffIconFlash` 兜底贴图均指向它，覆盖即用）。
- **功能**：增益/减益技能（demoType="bufffx"，假人「测试·迅捷祝福/迟缓诅咒」等）命中目标后，从目标身上持续飘起的小光点粒子；代码经 `instanceColor` 染色（增益绿/减益红）+ 上飘速度，单张尺寸 0.25~0.45 格。
- **构图**：**径向对称的柔和能量光点**：中心最亮的小核，向外柔和渐隐，可带轻微四芒星辉光（sparkle），不能有任何方向性拖尾（粒子自带运动方向由代码速度决定）。

```
FX_STYLE + a tiny radially symmetric glowing energy mote seen head-on: a
bright compact core with soft halo falloff and a faint four-pointed
sparkle flare, perfectly centered, brightness falling to pure black at
the edges, no directional streaks or trails
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **64×64**；
2. 中心 20% 半径内 alpha ≥ 0.9（光核要实心），四边 2px alpha = 0；
3. 径向对称：上下左右四个方向的亮度剖面基本一致（不偏心不拖尾）；
4. 灰度：RGB 三通道一致，染色交给代码（增益绿/减益红）；
5. 游戏内验证：Dev 模式假人「测试·群体迅捷」，范围内目标身上应飘起柔和绿色小光点（减益为红色）。

---

## 五、位移特效 `MMT_BlinkFlash` + `MMT_DashTrail`（2 张，256×256）【已出图完成】

> 2026-11-25 AI 终稿已覆盖占位图（Def/代码零改动）：爆闪中心 alpha 252、拖尾头部 252/尾端渐隐，
> 均纯灰度 RGBA、四边全透明，验收通过。

> 第九步位移框架（`SkillDisplace.cs`）：折跃/群体传送的起终点爆闪、冲锋的拖尾残影。
> 两张均为黑底白图（alpha=亮度，RGB=全白），代码按 FleckDef 直接播放，无运行时染色。

### 5.1 位移闪光 `MMT_BlinkFlash`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_BlinkFlash.png`（texPath `Things/Mote/MMT_BlinkFlash`，FleckDef `MMT_Fleck_BlinkFlash`，折跃/群体传送起终点、冲锋起跳、建筑跳跃落点均用它）。
- **构图**：**径向对称的传送爆闪**：中心高亮能量核，向外柔和光晕 + 四道主星芒（十字）与四道副星芒（斜角），星芒细长锐利、末端渐隐；整体像一次短促的空间撕裂闪光，不能有任何方向性偏移。

```
FX_STYLE + a radially symmetric teleport flash burst seen head-on: a
brilliant compact energy core, soft surrounding halo, four long sharp
star rays in a cross plus four shorter diagonal rays, ray tips fading
to pure black, perfectly centered, no directional bias, magical
space-tearing sparkle feel
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 中心 15% 半径内 alpha ≥ 0.9（爆闪核心要实心），四边 2px alpha = 0；
3. 径向对称：四道主星芒长度/亮度一致，不偏心；
4. 游戏内验证：Dev 模式假人「测试·折跃」，起点与落点应各爆一次短促闪光。

### 5.2 冲锋拖尾 `MMT_DashTrail`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_DashTrail.png`（texPath `Things/Mote/MMT_DashTrail`，FleckDef `MMT_Fleck_DashTrail`，冲锋位移途中沿途播放）。
- **构图**：**横向速度残影条**：彗星状横向光带，右端为明亮头部（速度方向），向左拉出渐隐尾迹；上下边缘柔和羽化；整体狭长，像一道被拉长的风压/能量残影。条带沿水平中轴，无上下偏移。

```
FX_STYLE + a horizontal motion-blur speed streak: a bright rounded
comet head on the right with a long tapering trail fading out to the
left, soft feathered edges above and below, centered on the horizontal
axis, aerodynamic wind-tunnel energy feel, no vertical offset
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 头部（右侧 20% 区域）alpha ≥ 0.9，尾端（左侧 5%）alpha ≈ 0；
3. 上下羽化对称，四边 2px alpha = 0；
4. 游戏内验证：Dev 模式假人「测试·冲锋」，位移路径上应留下一串渐隐残影。

---

## 六、范围选择圆 `MMT_TargetCircle`（1 张，256×256）【已出图完成】

> 2026-11-25 AI 终稿已覆盖占位图（Def/代码零改动）：环峰值半径 0.93、360° 闭合无缺口（峰值 alpha 233~255）、
> 圆内填充 ≤8%、圆外≈0、纯灰度，验收通过。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_TargetCircle.png`（texPath `Things/Mote/MMT_TargetCircle`，第一步范围瞄准框架 `MMTAoETargetPreview.DrawCircle` 统一使用，光环外圈描边同用）。
- **功能**：技能范围瞄准时的鼠标跟随圆形选择框（替换原版方格 DrawRadiusRing），代码按敌我染色（友方绿/敌方红），按技能半径整图缩放。
- **构图**：**完美的细圆环**：距中心约 93% 半径处一道清晰闭合的细亮线（1~2px 核心），两侧带极轻微柔光；圆内区域有 8%~12% 的微弱均匀填充（营造"选中区域"感），圆外完全透明；径向绝对对称。

```
FX_STYLE + a perfectly circular thin targeting reticle ring seen from
directly above: one crisp closed bright hairline circle at 93% radius
with a very subtle soft glow on both sides, the disc interior filled
with an extremely faint uniform luminescence, fully transparent outside
the ring, perfectly centered and radially symmetric
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 环带位于 92%~94% 半径，闭合无缺口，最亮处 alpha ≥ 0.9；
3. 圆内填充 alpha ≤ 0.15（不喧宾夺主），圆外 2px alpha = 0；
4. 径向对称：任意角度的亮度剖面一致；
5. 游戏内验证：Dev 模式假人任意范围技能瞄准时，鼠标处应为平滑圆形选择框（无方格感），敌我配色正确。

---

## 七、召唤特效 `MMT_SummonOrb` + `MMT_HomingBolt` + `MMT_SummonPaw` + `MMT_SummonTurret_Base/Top`（5 张，256×256）【已出图完成】

> 2026-11-25 AI 终稿已覆盖占位图（Def/代码零改动），Pillow 验收通过：雷球核心 alpha 254、60% 半径衰减至 14、边缘 0；
> 追踪弹头部 251/尾端 0、朝上纵向区间 15~207；爪印实心 250；炮塔底座/枪管图形内 alpha 255、灰阶 RGB、图形外 0。

> 第十步召唤技能框架（`SkillSummon.cs`）：雷球/追踪弹为 MoteGlow 加法着色（贴图必须白/灰度，代码 `orbTint`/`homingTint` 染色）；
> 炮塔底座/枪管为建筑 Cutout 贴图（需真实灰阶 RGB，不走染色）；爪印为 gizmo 图标白图。

### 7.1 雷球 `MMT_SummonOrb`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_SummonOrb.png`（texPath `Things/Mote/MMT_SummonOrb`，雷球实体 DrawAt 实绘 + 演示 gizmo 图标）。
- **功能**：召唤雷球的本体图形：从施法者缓慢移向目标点，沿途脉冲伤害，代码叠加呼吸缩放与自转。
- **构图**：**俯视正球**：中心一枚极亮的能量核心，向外平滑径向辉光衰减；核心外环绕 2~3 圈断续的弧形电弧（像球面闪电缠绕），整体径向对称，四边全透明。

```
FX_STYLE + a glowing ball lightning orb seen from directly above: an
intensely bright compact energy core with smooth radial glow falloff,
wrapped by 2-3 broken arcing lightning rings hugging the sphere
surface, perfectly centered and radially symmetric, edges fading to
pure black
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 核心（中心 25% 半径）alpha ≥ 0.95，辉光到 60% 半径处衰减至 ≈ 0；
3. 电弧亮度不得超过核心，四边 2px alpha = 0；
4. 游戏内验证：Dev 模式假人「测试·雷球」，应为蓝色（代码染色）发光球体滑向目标点。

### 7.2 追踪弹 `MMT_HomingBolt`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_HomingBolt.png`（texPath `Things/Mote/MMT_HomingBolt`，追踪弹实体 DrawAt 实绘，代码按航向旋转 + gizmo 图标）。
- **功能**：追踪弹本体图形。**方向约定：头部朝图片正上方（北）**，代码 `drawAngle` 旋转对准目标。
- **构图**：**朝上的梭形能量弹**：顶部一枚明亮弹头光点，向下延伸出收窄的弹体与渐隐尾迹（彗星状倒置），左右对称，四边全透明。

```
FX_STYLE + a sleek energy bolt pointing straight up: one brilliant
rounded warhead at the top with a narrow tapering body and a fading
comet trail extending downward, bilaterally symmetric around the
vertical center axis, aerodynamic missile feel, edges fading to pure
black
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 弹头（上方 20% 区域）alpha ≥ 0.9，尾端（下方 5%）alpha ≈ 0；
3. 左右羽化对称，四边 2px alpha = 0；
4. 游戏内验证：Dev 模式假人「测试·追踪弹」，弹体应始终头朝飞行方向。

### 7.3 召唤爪印 `MMT_SummonPaw`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_SummonPaw.png`（texPath `Things/Mote/MMT_SummonPaw`，动物召唤演示 gizmo 图标）。
- **构图**：**简洁的动物爪印剪影**：下方一枚大掌垫、上方四枚小趾垫，扁平图标风，实心白色，边缘干净。

```
FX_STYLE + a clean flat animal paw print icon: one large rounded pad
at the bottom and four small toe pads arcing above it, solid silhouette,
crisp smooth edges, minimalist game UI icon style
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 实心区域 alpha ≥ 0.9，四边 2px alpha = 0；
3. 游戏内验证：Dev 模式假人 gizmo 栏「测试·动物召唤」图标清晰可辨。

### 7.4 召唤炮塔底座 `MMT_SummonTurret_Base`

- **落盘**：覆盖 `1.7/Textures/Things/Building/MMT_SummonTurret_Base.png`（texPath `Things/Building/MMT_SummonTurret_Base`，炮塔建筑本体）。
- **功能**：召唤炮塔的圆形底座（建筑 Cutout 贴图，**需要真实灰阶 RGB 而非白图**）。
- **构图**：**俯视圆形金属底座**：正圆金属盘，中心略亮、外圈一圈深色描边，表面带轻微铆钉/分块细节，风格对齐原版迷你机枪塔底座。

```
RimWorld video game building sprite, top-down circular metal turret
base platform, slightly domed center panel with subtle rivet and panel
line details, darker outer rim, hand-painted stylized metal grays,
matches vanilla mini-turret base style, pure black background
```

**后处理与验收清单**：

1. RGBA 256×256，圆外 alpha = 0，圆内 alpha = 255（实体建筑不透明）；
2. 灰阶 RGB（无彩色偏移），外圈描边明显；
3. 游戏内验证：Dev 模式假人「测试·炮塔召唤」，底座贴地、尺寸约 1 格。

### 7.5 召唤炮塔枪管 `MMT_SummonTurret_Top`

- **落盘**：覆盖 `1.7/Textures/Things/Building/MMT_SummonTurret_Top.png`（texPath `Things/Building/MMT_SummonTurret_Top`，炮塔 TurretTop 旋转层 + gizmo 图标）。
- **功能**：炮塔旋转枪管层，代码按目标方向旋转。**方向约定：炮管朝图片正上方（北）**。
- **构图**：**俯视朝上的炮管**：中央圆形转台 + 一根向上伸出的炮管，炮口略粗，金属灰，对齐原版 TurretMini_Top 构图。

```
RimWorld video game turret top sprite, top-down gun barrel pointing
straight up: a circular pivot mount at the center with a single cannon
barrel extending upward, slightly thicker muzzle tip, hand-painted
stylized metal grays, matches vanilla turret top style, pure black
background
```

**后处理与验收清单**：

1. RGBA 256×256，图形外 alpha = 0，图形内 alpha = 255；
2. 炮管严格垂直朝上（旋转基准），左右对称；
3. 游戏内验证：炮塔索敌时枪管应平滑转向目标方向。

---

## 八、第 11~18 步新增特效

> **状态**：§8.1 陨石、§8.2 大火球投射物、§8.3 魔法石墙 **均已出图完成**（2026-11-25，AI 终稿覆盖占位图，Def/代码零改动）。
> 占位图由 `docs/temp/gen_skillfx2_placeholders.py` 生成（陨石/火球为黑底白图 → alpha=亮度、RGB 填白；石墙为实体灰砖），保留作返工基准。

### 8.1 陨石 `MMT_Meteor`

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_Meteor.png`（texPath `Things/Mote/MMT_Meteor`，`Thing_MMTMeteor` 自绘 + 「测试·陨石术」gizmo 图标）。
- **功能**：延时打击的坠落陨石本体，代码按 `meteorTint` 染色（演示为暖橙）并旋转；绘制尺寸约 1.6 格。
- **构图**：**黑底白图**（亮度=alpha，RGB 白）：一颗不规则炽烈岩石核（高亮，带暗灰碎块凹凸），左上方拖一条逐渐变小变暗的火焰尾迹（陨石朝向右下坠落）。

```
FX_STYLE + a single blazing meteorite falling toward the bottom-right:
an irregular white-hot rocky core with darker gray craggy chunks on its
surface, wrapped in a bright glow, trailing a tapering stream of flame
and embers toward the upper-left, pure black background
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha=亮度、RGB 全白，128×128；
2. 岩核为最亮处（alpha=255），尾迹渐隐到 0；四边 2px alpha=0；
3. 灰度无彩色偏移（染色交给代码）；
4. 游戏内验证：「测试·陨石术」预警圈结束后陨石斜向坠落、尾焰朝上。

### 8.2 大火球投射物 `MMT_BigFireball`

- **落盘**：覆盖 `1.7/Textures/Things/Projectile/MMT_BigFireball.png`（texPath `Things/Projectile/MMT_BigFireball`，`MMT_Proj_BigFireball` 投射物 + gizmo 图标）。
- **功能**：抛物线大火球的飞行弹体，原版投射物渲染（按飞行方向旋转，TransparentPostLight + XML 橙色染色）。**方向约定：飞行方向朝图片正上方（北），尾焰向下**。
- **构图**：**黑底白图**：一颗饱满的炽热火球（纯白高亮核心 + 外层光晕），下方拖一条收窄的火焰尾迹。

```
FX_STYLE + a big blazing fireball projectile flying straight up: a plump
white-hot spherical core with a soft outer glow, trailing a tapering
flame tail pointing downward, a few tiny embers around the tail, pure
black background
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha=亮度、RGB 全白，64×64；
2. 核心最亮，尾迹向下渐隐；四边 1-2px alpha=0（四角不得有可见亮斑）；
3. 游戏内验证：「测试·大火球」发射后弹体朝飞行方向旋转、尾焰朝后。

### 8.3 魔法石墙 `MMT_StoneWall`

- **落盘**：覆盖 `1.7/Textures/Things/Building/MMT_StoneWall.png`（texPath `Things/Building/MMT_StoneWall`，`MMT_MagicStoneWall` 建筑本体 + gizmo 图标）。
- **功能**：石墙术召唤的临时石墙（建筑 Cutout 贴图，**需要真实灰阶 RGB 而非白图**）。
- **构图**：**俯视一格石墙**：错缝排列的灰色石块（2~3 排），砖缝深色，石块带轻微高光/暗边与石材噪点，外圈深色描边，风格对齐原版花岗岩墙。

```
RimWorld video game building sprite, top-down single-cell stone wall
panel: staggered gray stone blocks in 2-3 rows, dark mortar gaps,
subtle top-edge highlights and bottom-edge shadows on each block, fine
stone grain noise, dark outline around the panel, hand-painted stylized
granite grays, matches vanilla granite wall style, pure black background
```

**后处理与验收清单**：

1. RGBA 128×128，墙体内 alpha=255，四边外 alpha=0；
2. 灰阶 RGB（无彩色偏移），砖缝与描边对比清晰；
3. 游戏内验证：「测试·石墙术」召唤出的墙占满一格、观感与原版墙一致。

---

## 九、瞄准预览圆环 `MMT_TargetRing`（1 张，1024×1024）【待出图】

> 现为 §六 `MMT_TargetCircle` 的副本占位（2026-11-26 起瞄准预览圆环从程序网格改为贴图方案，
> 解决程序网格硬边锯齿）。AI 终稿直接覆盖同名文件即可，代码零改动。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_TargetRing.png`（texPath `Things/Mote/MMT_TargetRing`，范围瞄准框架 `MMTAoETargetPreview.DrawRing` / `RingTexPath` 常量指向它；同步游戏目录时一并复制）。
- **功能**：技能范围瞄准时的鼠标跟随圆形描边（敌我配色：友方绿/敌方红/最大射程白），代码将整张 quad 缩放到 `半径×2` 格，`ShaderDatabase.Transparent` 按贴图 alpha 混合 + 双线性过滤（高分辨率 + alpha 渐变边缘 = 抗锯齿的来源）。与 §六 的区别：§六 仅延时打击警示圈在用（TransparentPostLight），本节专供瞄准预览。
- **构图**：**完美的闭合细圆环**，亮环峰值位于**半径 94%~96%** 处（贴近画布外缘，给缩放留出血）；环核心细亮线 + 两侧柔和辉光衰减；圆内可有 ≤8% 的极微弱均匀填充（代码另有程序填充盘，贴图填充只是点缀，宁可没有）；圆外（环到图边之间）必须纯黑全透明；径向绝对对称、360° 闭合无缺口无接缝。

```
FX_STYLE + a perfectly circular thin targeting reticle ring seen from
directly above: one crisp closed bright hairline circle at 95% radius
as the only main visual, with a smooth soft glow falling off on both
sides of the hairline, an extremely faint uniform luminescence inside
the disc, pure black and fully transparent everywhere outside the ring,
perfectly centered, radially symmetric, seamless all around
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **1024×1024**（最低 512×512；2 的幂，mipmap 保证小圈缩小时依旧平滑）；
2. 环带峰值位于 94%~96% 半径，360° 闭合无缺口，最亮处 alpha ≥ 0.9；
3. **环总宽度（含辉光）≤ 半径的 3~4%，亮线核心 ≤ 半径的 1.5%**——贴图环宽随绘制半径等比缩放（半径 25 格的最大射程圈也会用到它），环做太粗大圈会变成粗带；宁可偏细，双线性过滤会保它平滑可见；
4. 圆内填充 alpha ≤ 0.08（与代码填充盘叠加后不喧宾夺主），圆外到四边 2px 全为 0（四角不得有任何亮斑/杂点）；
5. 径向对称：任意角度的亮度剖面一致，无偏心、无局部增亮（否则染色后像"环上有个瘤"）；
6. 灰度：RGB 三通道一致（无彩色倾向），染色交给代码；
7. 游戏内验证：Dev 模式假人任意范围技能瞄准——鼠标处圆环平滑无锯齿、闭合无缺口；敌我配色正确；最大射程圈（白环）宽度可接受。
