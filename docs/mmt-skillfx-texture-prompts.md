# MMT 技能特效框架 — AI 出图提示词集（喂图模型用）

> **状态**：§一 技能光柱、§二 火焰柱、§三 光环圆盘、§四 增益粒子、§五 位移特效、§六 范围选择圆、§七 召唤特效 **均已出图完成**（2026-11-25，AI 终稿覆盖占位图，Def/代码零改动）。
> §九 瞄准预览圆环 `MMT_TargetRing` **已由程序生成终稿**（2026-11-26，`docs/temp/gen_target_ring.py`，2048² 按最大 45 格射程设计；AI 不适合绝对几何对称图形，程序生成即终稿，不再走 AI 出图）。
> §十 技能地雷、§十一 冰块覆盖物 **均已出图完成**（2026-11-26，AI 终稿覆盖占位图，Def/代码零改动）。
> §十二 射线端头（发射头/命中端）**已出图完成**（2026-11-26，用户生成：RGBA 128²、RGB 全白、中心 alpha 255、四角 alpha 0，验收通过，Def/代码零改动）。
> §十三 新星环 `MMT_NovaRing` **已出图完成**（2026-11-26，AI 终稿覆盖占位图，Def/代码零改动）；**2026-11-27 叠加贴地光幕版**（`docs/temp/add_nova_veil.py` 程序合成，环内径向渐亮柔光填充，底图备份 `docs/temp/MMT_NovaRing_base.png`）。

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

## 二、火焰柱 `MMT_FlamePillar`（1 张，128×128）【已出图完成（2026-11-26 用户生成，俯视径向火焰团版）】

> **2026-11-26 绘制改版**：1.7 竖直 quad 不渲染（严禁立起 plane10），火柱绘制从
> "平躺双层四边形"改为「贴地火盘 + 多层水平碟堆叠」（SkillGroundFire.cs `Thing_MMTGroundFlame.DrawAt`，
> 参数 `groundPillarHeight/groundPillarLayers/groundPillarTopScale/groundPillarSway`）。
> 每层贴图都是**俯视可见**的，旧"侧视火舌"图叠出来方向不对——需按下方新提示词重出图，覆盖同名文件即用。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_FlamePillar.png`（texPath `Things/Mote/MMT_FlamePillar`，ThingDef `MMT_GroundFlame` 与 `MMTGroundFireParams.texPath` 默认值均指向它，覆盖即用）。
- **功能**：地面技能（demoType="groundFire"，假人「测试·烈焰喷发」单柱 /「测试·烈焰火海」成片）的火焰本体；代码按 `groundTint` 染色（演示为橙黄烈焰色），贴地火盘 + 7 层水平碟堆叠成垂直火柱（逐层收小、相位呼吸、顶部摆动）+ 淡入淡出。
- **构图**：**俯视正圆形火焰团**：中心高亮实心核（约占图幅 30%），向外辐射翻卷火舌，亮度从核到透明边缘平滑衰减；整体近似圆对称（堆叠层随机旋转后不能看出方向性）。

```
FX_STYLE + a single radial burst of flame viewed from directly above:
an intense pure-white hot core at the exact center of the image, soft
glowing flame tongues curling outward in all directions, smooth
brightness falloff from the core to fully transparent edges, roughly
circular silhouette filling about 70% of the image, no ground no embers
outside the burst
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **128×128**；
2. 中心核 alpha ≥ 0.9（要实心），四边 2px alpha = 0；
3. 灰度：RGB 三通道一致（无彩色倾向），染色交给 `groundTint`；
4. 剪影近似圆形、重心在图幅中心 ±6px 内（堆叠层随机旋转/成片火海不偏向）；
5. 游戏内验证：Dev 模式对假人用「测试·烈焰喷发」应为**贴地面、垂直向上**喷发的火柱（底大顶小、顶部摆动），「测试·烈焰火海」成片自中心向外蔓延、不集体偏向一边。

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

## 九、瞄准预览圆环 `MMT_TargetRing`（1 张，2048×2048）【程序生成终稿】

> **2026-11-26 结论：本贴图不走 AI 出图**。验收要求"完美闭合、径向绝对对称、峰值精确落在指定半径"，
> AI 图必然带偏心/不对称（染色后像"环上有个瘤"），程序生成（`docs/temp/gen_target_ring.py`，Pillow）直接就是终稿。
> 如需调参（环位/线宽/辉光/填充）改脚本重跑即可，Def/代码零改动。
>
> **按最大 45 格射程设计**：代码侧 `MMTAoETargetPreview.MaxRangeRingRadius = 45f`，
> 射程超过 45 格的技能**不再绘制射程环**（贴图被等比拉成粗带只剩劣化观感；范围圈 radius 不受影响，AoE 圈都很小）。
> 自检指标（脚本运行即打印）：峰值半径 94.1%（目标 94.5%）、核心线宽 5px（45 格时 ≈0.22 格）、
> 辉光总宽 21px（≈0.92 格）、360° 闭合最低 alpha 254、环内填充 16/255、环外与四边全 0、RGB 全白。

- **落盘**：`1.7/Textures/Things/Mote/MMT_TargetRing.png`（texPath `Things/Mote/MMT_TargetRing`，范围瞄准框架 `MMTAoETargetPreview.DrawRing` / `RingTexPath` 常量指向它；同步游戏目录时一并复制）。
- **功能**：技能范围瞄准时的鼠标跟随圆形描边（敌我配色：友方绿/敌方红/最大射程白），代码将整张 quad 缩放到 `半径×2` 格，`ShaderDatabase.Transparent` 按贴图 alpha 混合 + 双线性过滤（高分辨率 + alpha 渐变边缘 = 抗锯齿的来源）。与 §六 的区别：§六 仅延时打击警示圈在用（TransparentPostLight），本节专供瞄准预览。
- **生成参数**（脚本常量）：2048² 终稿（4096² 超采样 2× 抗锯齿）、环峰值 94.5% 半径、核心亮线 10px@SS（终稿 5px）、辉光高斯 10px@SS 峰值 0.3、环内微填充 16/255。
- **设计容量换算**：45 格半径时整图缩到 90 格宽，1 格 ≈ 22.8 tex px → 核心 ≈0.22 格细线 + 辉光 ≈0.9 格柔边；小圈（半径几格）经 mipmap 缩小仍保持闭合细环。

**验收清单**（脚本 `verify()` 自动打印，人工复核游戏内观感）：

1. 环带峰值位于 94%~96% 半径，360° 闭合最低 alpha ≥ 230；
2. 核心线宽 ≤ 半径 0.5%、辉光总宽（a≥20）≤ 半径 3%（45 格时分别 ≈0.22/0.9 格）；
3. 环内填充 alpha ≤ 0.08，环外到四边全为 0（四角无亮斑）；
4. 灰度：RGB 三通道一致（全白），染色交给代码；
5. 游戏内验证：Dev 模式假人任意范围技能瞄准——鼠标处圆环平滑无锯齿、闭合无缺口；敌我配色正确；45 格射程的白环仍是细环；射程 >45 的技能不再出现射程环。

---

## 十、技能地雷 `MMT_SkillMine`（1 张，128×128）【已出图完成】

> 2026-11-26 AI 终稿已覆盖占位图（黄铜军绿俯视地雷：边圈铆钉 + 中央压力盖，四角 alpha=0、盘体 alpha=255，验收通过），Def/代码零改动。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_SkillMine.png`（texPath `Things/Mote/MMT_SkillMine`，ThingDef `MMT_SkillMine` 地雷本体（Cutout 0.8 格）+ gizmo 图标）。
- **功能**：触发式爆炸陷阱（demoType="mine"，「测试·爆炸陷阱」）布设在地面上的地雷本体。**实物贴图（需要真实 RGB 而非白图）**，风格对齐原版陷阱/建筑手绘。
- **构图**：**俯视一枚反步兵地雷**：低矮圆形金属盘（军绿/暗钢灰），外圈略深的埋土边环，盘面中央一颗凸起的压力引信盖，盘缘 6~8 颗铆钉，左上受光的轻微金属高光，底部一点贴地阴影，风格对齐原版花岗岩墙/陷阱的手绘灰阶金属感。

```
RimWorld video game sprite, top-down view of a single anti-personnel
landmine: a low circular metal disc in muted olive-drab and dark steel
gray, slightly darker soil-blended outer rim, a raised pressure-fuze cap
at the center, 6-8 rivets around the rim, subtle metallic highlight from
the upper-left, faint contact shadow underneath, hand-painted stylized
grays, matches vanilla trap and granite wall style, pure black background
```

**后处理与验收清单**：

1. RGBA 128×128，盘体 alpha=255，四边外 alpha=0（Cutout 着色器）；
2. 灰阶军绿 RGB，无彩色偏移；铆钉与压力盖对比清晰；
3. 游戏内验证：「测试·爆炸陷阱」布设后地面上为清晰可辨的小圆盘（0.8 格），不突兀不发光。

---

## 十一、冰块覆盖物 `MMT_IceBlock`（1 张，128×128）【已出图完成】

> 2026-11-26 AI 终稿已覆盖占位图（浅蓝半透明冰晶块：52% 像素 alpha 落在 100~249 半透明区间、无全不透明点、四角 alpha=0，冰下目标隐约可见，验收通过），Def/代码零改动。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_IceBlock.png`（texPath `Things/Mote/MMT_IceBlock`，ThingDef `MMT_IceBlock` 冰冻覆盖物，`Thing_MMTIceBlock.TexPath` 常量亦指向它）。
- **功能**：寒风领域（demoType="groundZone" 冰冻皮肤，「测试·寒风领域」）冻结目标时罩在 pawn 身上的冰块（代码按 alpha 做凝冰淡入/微呼吸/融化淡出，**贴图自带冰蓝色**，代码只调透明度不染 RGB）。
- **构图**：**俯视一块罩住人形的不规则冰晶**：浅蓝半透明冰体（alpha ≈ 0.6~0.8，要能隐约看到冰下目标），多边形外轮廓带亮边，内部几道晶面棱线与细裂纹，左上受光一片柔和高光，四边渐隐。

```
RimWorld video game VFX sprite, top-down view of a jagged ice crystal
block covering a humanoid silhouette: pale blue translucent ice body
(semi-transparent, the figure beneath faintly visible), irregular
polygonal outline with a bright frosted rim, a few crystalline facet
lines and thin cracks inside, a soft white specular highlight in the
upper-left, edges fading out smoothly, pure black background
```

**后处理与验收清单**：

1. RGBA 128×128：冰体 alpha ≈ 150~210（**不要全 255**，冰下目标须隐约可见），四边 2px alpha=0；
2. 冰蓝 RGB（无彩色溢出到纯蓝），裂纹/棱线为更亮的浅蓝白；
3. 游戏内验证：「测试·寒风领域」冻结的敌人全身罩冰、快速凝冰出现、离区后冰块淡出融化。

---

## 十二、射线端头 `MMT_SkillBeam_Head` + `MMT_SkillBeam_Tail`（2 张，128×128）【已出图完成】

> **状态**：已出图（2026-11-26 用户生成，验收：RGBA 128²、RGB 全白、中心 alpha 255、四角 alpha 0、纯灰度无彩色）。
> 风格基准：`1.7/Textures/Things/Projectile/RoM/FrostRay.png`（末端大爆点 + 飞溅碎屑的手绘能量质感）。

> 持续射线三段式（`SkillBeam.cs`，demoType="beam"，「测试·烈焰射线」）：
> **中段**（§一 `MMT_SkillBeam`，已有）+ **发射头**（本节 Head，出膛点光球）+ **命中端**（本节 Tail，终点爆点）。
> 头尾均为代码按 `beamTint` 染色的白/灰度图（TransparentPostLight），以 quad 平铺绘制、随光束方向旋转，
> 尺寸由 XML 配置（演示：Head 1.4 格 / Tail 1.6 格），命中端另有独立脉冲缩放。

### 12.1 发射头 `MMT_SkillBeam_Head`

- **落盘**：`1.7/Textures/Things/Mote/MMT_SkillBeam_Head.png`（texPath `Things/Mote/MMT_SkillBeam_Head`）。
- **功能**：画在光柱起点（施法者身前 0.9 格出膛点）的"聚能喷口"光球，与中段同步呼吸。
- **贴图元素详述**：**径向对称的能量光球**——① 中心一枚极亮的实心核心（约占图幅 15~20%，纯白）；② 核心外一层饱满的球形光晕，向外柔和渐隐；③ 可带轻微的十字/四芒星辉光（短而细，末端渐隐）；④ **不要**飞溅碎屑、不要方向性拖尾（它是稳定的聚能出口，不是爆炸）；整体像一颗被压缩的能量球。

```
FX_STYLE + a compact radiant energy muzzle orb seen head-on: an intensely
bright solid core, wrapped in a plump spherical glow halo fading smoothly
outward, with a faint short four-pointed star flare, perfectly centered
and radially symmetric, calm and stable (no explosion, no flying debris,
no directional trail), edges falling to pure black
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **128×128**；
2. 中心 15% 半径内 alpha ≥ 0.95（核心实心），辉光到 70% 半径处衰减至 ≈ 0，四边 2px alpha = 0；
3. 径向对称：四个方向亮度剖面基本一致；灰度无彩色倾向（染色交给 `beamTint`）；
4. 游戏内验证：「测试·烈焰射线」光柱起点为一颗稳定的橙红光球，不盖住施法者（出膛点已前移 0.9 格）。

### 12.2 命中端 `MMT_SkillBeam_Tail`

- **落盘**：`1.7/Textures/Things/Mote/MMT_SkillBeam_Tail.png`（texPath `Things/Mote/MMT_SkillBeam_Tail`）。
- **功能**：画在光柱终点（撞墙/最大射程处）的"灼烧爆点"，独立快速脉冲缩放，营造持续溅射灼烧感。
- **贴图元素详述**：**参考 FrostRay 末端的爆发球**——① 中心一大片不规则的高亮爆斑（约占图幅 30~40%，最亮，边缘不是正圆、带火焰状凹凸）；② 爆斑外围一圈半透明光晕；③ 从爆斑向外飞出 8~15 颗大小不一的能量碎屑/液滴（上、左、右三个方向为主，下方可略少——爆点是光束从上方/侧向注入的落点），碎屑越往外越小越暗；④ 允许自然随机感（不要求严格对称），但整体重心须在图幅中心 ±10% 内；整体像能量持续砸在一点上溅开的熔融爆斑。

```
FX_STYLE + an intense energy impact burst seen from above: a large
irregular white-hot splash core with flame-like ragged edges at the
center, a soft surrounding glow, and 8-15 flying energy droplets and
debris sparks of varying size scattering outward (mostly upward and
sideways), droplets shrinking and dimming with distance, natural random
splash feel like the end burst of a frost/flame ray, roughly centered,
edges falling to pure black
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **128×128**；
2. 爆斑核心 alpha ≥ 0.95，碎屑末端渐隐至 0，四边 2px alpha = 0；
3. 灰度无彩色倾向（染色交给 `beamTint`）；重心偏移 ≤ 图幅 10%（避免脉冲时爆点"绕圈"）；
4. 游戏内验证：「测试·烈焰射线」光柱终点为不断脉动的橙红爆点，撞墙时爆点贴在墙面格。

---

## 十三、新星环 `MMT_NovaRing`（1 张，256×256）【已出图完成，含贴地光幕】

> 2026-11-26 AI 终稿已覆盖占位图（白图正圆环：RGBA 256²、RGB 全白无彩色、环带外沿 alpha 255、圆心 60% 半径内与四边 alpha=0，验收通过），Def/代码零改动。
> 2026-11-27 光幕版：在 AI 圆环底图上程序合成环内柔光填充（`docs/temp/add_nova_veil.py`，幂等，底图备份 `docs/temp/MMT_NovaRing_base.png`）——
> 环内一层中心 0、向环带内缘渐亮的柔光盘（单层峰值 40/255 ≈ 16%，4 层拖尾叠加有效 ≈ 24%），环扩散时波后留下渐隐光纱。
> 注意：代码同图叠画 4 层（波峰 + 3 层拖尾，SkillNova.cs DrawAt），环内填充会叠加，单层 alpha 务必 ≤ 16%；
> RGB 必须全白（TransparentPostLight 是 alpha 混合，暗 RGB 压暗地面，见 §三教训）。

- **落盘**：覆盖 `1.7/Textures/Things/Mote/MMT_NovaRing.png`（texPath `Things/Mote/MMT_NovaRing`，ThingDef `MMT_NovaRing` 与 `MMTNovaParams.texPath` 默认值均指向它，覆盖即用）。
- **功能**：新星环（demoType="nova"，「测试·神圣新星」，牧师 III 阶神圣新星）的冲击波环本体；代码按 `novaTint` 染色（演示为圣光金 `(1,0.85,0.4)`），quad 从 0 线性放大到直径 12 格（径向扩散、不旋转），波峰环最亮、通过后拖尾淡出。
- **贴图元素详述**：**正圆冲击波环**——① 一整圈粗细均匀的圆环，环带占图幅半径约 13%（内缘 ≈0.84R，外沿贴到 0.99R）；② 外沿最亮（波峰），向内缘柔和渐隐，外缘 1~2px 急收（扩散到最大半径时环外不能有溢出亮边）；③ 环带内可有细密的能量流动纹理（沿切线方向的流光/细波纹），但任意角度的环带不能断流；④ 严格径向对称（代码不旋转，任何不对称都会被等比放大成"偏心波"）；⑤ 圆心区域为**极淡的贴地光幕**：一层从中心 0 向环带内缘渐亮的柔光填充（单层峰值 ≤ 16% alpha），像冲击波扫过后地面残留的渐隐光纱，不能盖过环带本体、不能有中心光点（中心闪光由代码另放）。

```
FX_STYLE + a perfect circular shockwave ring seen from directly above:
a uniform-width glowing annulus band occupying the outer 13% of the
radius, brightest at its crisp outer rim and fading smoothly inward,
with fine tangential energy streaks flowing along the band (continuous,
never broken at any angle), a very faint translucent veil of light
filling the ring interior (brightest just inside the band, dimming to
zero at the center, like a fading afterglow left on the ground),
perfectly radially symmetric, transparent outside the ring and at the
corners, edges falling to pure black
```

**后处理与验收清单**：

1. 黑底白图 → RGBA：alpha = 亮度，RGB = 全白，尺寸 **256×256**；
2. 环带外沿 alpha ≥ 0.95（波峰实心），内缘渐隐，图幅四边 2px alpha = 0，环外无溢出亮边；环内光幕填充单层 alpha ≤ 0.16、中心处 = 0（代码 4 层叠画会放大填充）；
3. 径向对称：任取 8 个方向的环带亮度剖面一致（重心偏移 ≤ 图幅 2%）；灰度无彩色倾向（染色交给 `novaTint`）；
4. 游戏内验证：「测试·神圣新星」金色光环从脚下匀速扩散到直径 12 格，波峰清晰、波后留下一层向中心渐隐的柔光纱，拖尾自然，不偏心、无接缝亮边、地面不被压暗。

---

## 十四、烈焰喷发序列帧 `MMT_FlameErupt_0~4`（5 张，256×256）【占位图已落盘，待 AI 俯视终稿】

> 2026-11-27 转向：原侧视素材（爱给网 234 序列帧，110×128）在 1.7/Waylith 下无可靠竖直呈现手段
> （竖直 quad 不渲染；水平切片堆叠按真实高度画仍在屏幕上塌成一条带），四版实验后弃用侧视方案。
> 现按**俯视径向爆燃**重新出图——与原版火焰同思路，整帧平铺地面即正确。
> 当前为 Pillow 程序占位图（径向火舌 + 白黄高温芯），AI 终稿回来后**覆盖同名文件即可，Def 零改动**。
> 同日晚些补充：侧视素材另有出路——**屏幕空间 UI 层贴图**（Mote.DrawGUIOverlay 通道，
> 见 `Source/MyModeTest/SkillFX/SkillRiseFX.cs` 与演示条目「测试·地心火柱」，
> 素材已拷至 `1.7/Textures/Things/Mote/MMT_SkillRiseFX_FlamePillar.png`）；
> 代价是画在世界物体之上（无遮挡关系）、不吃光照，适合单点爆发特效，不适合火海密度。

- **落盘**：`1.7/Textures/Things/Mote/MMT_FlameErupt/MMT_FlameErupt_0.png` … `_4.png`（texPath `Things/Mote/MMT_FlameErupt/MMT_FlameErupt_<i>`，`MMTGroundFireParams.frameTexPaths` / `CompProperties_MMTGroundFire.frameTexPaths` / 演示 `groundFrameTexPaths` 指向它们）。
- **功能**：烈焰喷发（`MMT_BossFire_FlamePillar` 熔岩火柱 / 假人「测试·烈焰喷发」）的火焰本体。`frameTexPaths` 非空时 `Thing_MMTGroundFlame` 走序列帧模式：5 帧（0 起燃 → 1/2 喷发峰值 → 3 旋卷 → 4 余烬消散）按 `frameTicks`（7 tick/帧）循环播放，帧相位按 seed 错开（火海成片不同步）。
- **绘制**：整帧平铺目标格地面（plane10 + Quaternion.identity，沿 `DrawDisc` 走 PerspectiveCompensate，贴地高度补偿近似恒等），颜色走 MaterialPropertyBlock 逐次传入，不改共享材质 color。**帧图必须是俯视构图**，侧视帧平铺会"躺倒"。

### AI 出图提示词（终稿用，每帧一张或出 5 帧序列图后等宽切片）

全局前缀：

```text
game VFX sprite, top-down view (viewed from directly above), radial flame eruption
on transparent background, vivid orange-red fire with white-hot core, hand-painted
fantasy game style, symmetrical radial composition centered in frame, no ground,
no smoke plume upward, PNG with alpha channel
```

逐帧差异（negative 通用：`side view, vertical flame, smoke column, ground, text, watermark`）：

1. `_0` 起燃：`small ignition burst, short flame tongues, dim embers gathering`
2. `_1` 喷发：`expanding radial fire burst, medium flame tongues spreading outward`
3. `_2` 峰值：`full eruption peak, large radial flame star, brightest white-hot core`
4. `_3` 旋卷：`swirling flame tongues curling clockwise, slightly dimmer`
5. `_4` 余烬：`dying embers, sparse fading sparks, low intensity`

**验收清单**：

1. 5 帧均为 256×256（或同尺寸正方形）RGBA、**透明底**、构图居中、径向对称；
2. alpha 通道充足：峰值帧 alpha max ≥ 230，透明区 RGB 预乘为黑（防白边）；
3. 游戏内验证：「测试·烈焰喷发」为连贯的径向爆燃循环（约 0.58s/循环），无帧间跳动错位、无集体同步闪烁（火海场景）；
4. AI 终稿直接覆盖 `MMT_FlameErupt_<i>.png` 同名文件即可，帧数可变（改 XML 列表即可）。


## 十五、侧视平躺序列帧 `MMT_SkillSideFlat` 用素材（原型复用第二十五步图集，已出图）

> 2026-11-27 第三条出路：侧视素材**整帧平铺水平 quad**（plane10 + Quaternion.identity）。
> 逆向 Waylith 确认 1.7 主相机为 90° 正上方正交相机、DrawMesh 链路无任何逐 draw 剔除
> （竖直 quad 是投影退化为零面积线段，非被剔除），地面与屏幕 1:1 无失真映射，
> 故贴图"上"边朝世界 +Z（北）即屏幕上——侧视构图平躺即呈现竖立侧视效果，无需预拉伸。
> 实现见 `Source/MyModeTest/SkillFX/SkillSideFlat.cs` 与演示条目「测试·地心火柱·平躺」。
> 原型直接复用第二十五步图集 `MMT_SkillRiseFX_FlamePillar.png`（550×128，5 帧侧视火焰，
> 已是终稿），无需占位图；本节留给后续专用侧视平躺素材的出图规范。

- **构图硬约束**（与屏幕空间版不同，平躺版贴图方向即世界方向）：
  1. 侧视构图：底部贴地（贴图底边 = 地面/锚点）、顶部朝上——平躺后顶部朝世界北 = 屏幕上，**出图时"上"就是图的上边，不做任何旋转**；
  2. 帧图底边中点对齐：特效锚点是贴图**底边中点**（钉在目标格中心），主体须底边居中，左右对称最佳；
  3. 透明底 RGBA，alpha 充足；additive 友好——亮色主体配纯黑透明区（黑底可程序转 alpha）；
  4. 朝向锁死屏幕北：素材里不要画"朝南看的背面"，永远是屏幕正视；
  5. 高度建议 ≤ 4 格：遮挡按地面 AltitudeLayer 判定（画在 MoteOverhead 层，高于 pawn），过高的图会把北面数格内的小人整片盖住。
- **绘制参数对应**：宽（世界 X）↔ 贴图横向、高（世界 +Z 北）↔ 贴图纵向，XML `sideFlatWidthCells` / `sideFlatHeightCells` 分别控制，非方帧（如 110×128）按宽高比给格数即可，引擎端不二次拉伸。
- **序列帧排布**：横向等宽图集（同第二十五步），帧数写 XML `sideFlatFrameCount`，C# 用 `mainTextureOffset` 等宽切片；也可按 `MMT_FlameErupt` 惯例拆单帧文件（需改 C# 为逐帧贴图列表，目前仅支持图集）。

### AI 出图提示词（终稿用，每帧一张或出序列图后等宽切片）

全局前缀：

```text
game VFX sprite, SIDE view vertical flame pillar eruption on transparent background,
vivid orange-red fire with white-hot core, hand-painted fantasy game style,
base of the flame anchored at the bottom edge center of the frame, flame rises
toward the top edge, no ground plane, no perspective tilt, PNG with alpha channel
```

negative 通用：`top-down view, radial composition, ground, text, watermark, cropped base`

逐帧差异可参考第十四节的起燃→峰值→旋卷→余烬节奏（侧视版：底部始终贴底边，顶部火焰高度逐帧起伏）。

**验收清单**：

1. 图集 RGBA、透明底，单帧等宽（如 110×128 × N），底边中点锚定、顶部留白 ≤ 5%；
2. 透明区 RGB 预乘为黑（防白边），峰值帧 alpha max ≥ 230；
3. 游戏内验证：「测试·地心火柱·平躺」目标格上呈现竖立侧视火柱（顶部朝屏幕上），
   序列连贯、淡入淡出无跳变；与「测试·地心火柱」（屏幕空间版）同格对照，确认
   吃地图光照与 Altitude 遮挡差异符合预期；
4. 终稿覆盖同名文件即生效，Def 零改动（宽高比变化时同步调 XML 宽/高格数）。

---
## 十六、火海 5×5 地面贴图 `MMT_FlameSea5x5`（5 帧，1024×1024）【已出图完成，已接入「测试·烈焰火海」】

> 2026-11-27：为成片火海准备的**整图覆盖 5×5 格**帧序列方案。与 §十四 互补：§十四 是每格一簇火焰的循环动画，本节是一张铺满 5×5 区域的整图火海序列。渲染约束不变（1.7 竖直 quad 不渲染）：**俯视径向构图、整帧平铺地面**。
> **已出图完成**：AI 逐帧出 5 帧（起燃→蔓延→峰值→旋卷→余烬，亮度曲线 3.4→21.7→65.3→17.6→0.8），Pillow 预乘黑后落盘（透明区 RGB=0 防白边，alpha max 250~255、四边 alpha=0、重心居中 ±2%，验收通过），并接入「测试·烈焰火海」（`groundSingleFlame` 整图单团模式，radius 2.5、visualScale 5、7 tick/帧，条目见 `1.7/Defs/Misc/MMT_SkillDemoSet.xml`）。

- **落盘**：`1.7/Textures/Things/Mote/MMT_FlameSea5x5/MMT_FlameSea5x5_0.png` … `_4.png`（texPath `Things/Mote/MMT_FlameSea5x5/MMT_FlameSea5x5_<i>`，演示条目 `groundFrameTexPaths` 指向它们，返工覆盖同名文件即用，Def 零改动）。
- **功能**：「测试·烈焰火海」（demoType="groundFire" + groundSingleFlame）的一体化火海本体：中心一根大火焰按 `visualScale`（5 格）放大覆盖 5×5 区域，5 帧序列循环播放；帧图为白/灰度图，代码按 `groundTint` 染橙红 `(1,0.55,0.15)`；伤害半径缺省 = 预览圈 radius（2.5 格）。
- **接入机制**：`MMTSkillDemoEntry.groundSingleFlame`（AbilityTargeting.cs）——true 时不逐格铺火海（areaRadius=0），只在目标点生成一根大火焰，`effectRadius` 缺省取 radius。

### AI 出图提示词（返工用，每帧一张）

全局前缀：

```text
game VFX sprite, top-down view (viewed from directly above), a large sea of
flames covering the entire frame, radial composition centered in frame:
brightest white-hot fire core at the exact center, dense orange-red flame
tongues spreading outward in all directions, flames gradually thinning and
fading to fully transparent at all four edges, vivid orange-red fire with
yellow-white highlights, hand-painted fantasy game style, symmetrical radial
composition, no ground, no smoke plume, no vertical flame column,
PNG with alpha channel
```

负面词：

```text
side view, vertical flame, smoke column, ground, terrain, text, watermark,
hard edges, abrupt cutoff at borders, perspective tilt, frame grid,
sprite sheet layout
```

逐帧差异（第 1 帧文生图定稿后，其余帧 img2img、重绘幅度 0.3~0.45、同种子，仅替换以下描述拼在全局前缀后）：

1. `_0` 起燃：`small ignition stage, dim embers gathering at the center, low flame intensity, sparse short flame tongues`
2. `_1` 蔓延：`flames spreading outward from the center, medium intensity, flame tongues reaching halfway to the edges`
3. `_2` 峰值：`full eruption peak, brightest white-hot core, dense flame tongues filling the frame`
4. `_3` 旋卷：`swirling flame tongues curling slowly, slightly dimmer, flames beginning to settle`
5. `_4` 余烬：`dying embers, sparse fading sparks, low intensity, flames mostly gone`

**验收清单**：

1. 5 帧同尺寸正方形 RGBA（1024²）、透明底，四边 2px alpha = 0；
2. 透明区 RGB 预乘为黑（防白边）；峰值帧（`_2`）alpha max ≥ 230；亮度整体按 `_0`<`_1`<`_2`>`_3`>`_4` 单调起伏；
3. 重心全部在图幅中心 ±5% 内，逐帧无整体位移（循环首尾 `_4` → `_0` 不跳变）；
4. 相邻帧差分检查：变化应集中在火舌细节而非整体构图（漂移过大 → 降 img2img 重绘幅度重出）；
5. 游戏内验证：「测试·烈焰火海」5×5 区域火海循环播放流畅、无帧间跳动错位、无硬边白边。
