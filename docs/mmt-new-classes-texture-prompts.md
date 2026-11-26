# MMT 新职业（第一批 5 职业）— AI 出图提示词集（喂图模型用）

> **状态**：全部待出图（设计阶段，技能卡见 `mmt-class-priest.md` 等各分册）。出图流程照惯例：Pillow 占位图先行落盘 → AI 终稿覆盖同名文件（Def/代码零改动）→ 本文档标注已出图。
> 与 `mmt-skillfx-texture-prompts.md`、`mmt-dungeon-texture-prompts.md` 同格式。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**。

## 〇、全局风格前缀与负面词

**风格前缀（FX_STYLE，特效/投射物贴图专用）**：

```
RimWorld video game VFX sprite, stylized hand-painted energy effect, soft
cel shading with smooth glow falloff, white and light-gray grayscale only
(tint applied by engine), pure black background, clean edges, no scene no
objects, symmetrical around the vertical center axis
```

**风格前缀（ICON_STYLE，图标/物品/建筑贴图专用——要固有色，不靠代码染色）**：

```
RimWorld video game item icon, stylized hand-painted, soft cel shading,
clean readable silhouette, centered single object on pure black
background, no scene no ground
```

**负面词（NEGATIVE）**：

```
text, letters, watermark, logo, photorealistic, photograph, 3D render,
scene, ground, character, weapon held in hands, busy background
```

**通用后处理约定**：

- 标【白图】的条目：出图为黑底白图 → 落盘前按"亮度 → alpha"转 RGBA 透明底（黑=全透明，白=不透明），RGB 统一填白，染色交给代码；
- 标【彩色】的条目：黑底抠透明底，保留固有色；
- 四边留 1-2 像素全透明，避免缩放/拉伸接缝；
- 投射物贴图一律**朝北**（尖端朝上），飞行朝向由代码旋转。

---

## 一、圣光弹 `MMT_HolyBolt`（1 张，64×64）【白图】

- **落盘**：`1.7/Textures/Things/Projectile/MMT_HolyBolt.png`（投射物 `MMT_Proj_HolyBolt`，牧师·惩击）。
- **注意**：投射物 graphicData 里写死 color `(255,215,110)`（金色），贴图本体出白图或淡金均可，drawSize 代码放大到 1.2 格。

```
FX_STYLE + a compact holy light bolt projectile pointing straight up
(north): an elongated radiant light bullet with an intense white core,
tapered glowing tail below and a bright rounded head, subtle holy
shimmer filaments along the body, overall vertical slender shape
```

## 二、新星环 `MMT_NovaRing`（1 张，256×256）【白图】

- **落盘**：`1.7/Textures/Things/Mote/MMT_NovaRing.png`（配 backlog §一.1 新星环原型；牧师·神圣新星染金、法师·冰霜新星染冰蓝）。
- **要求**：完美的几何圆环（AI 画不圆的话改为程序生成，参照 `MMT_TargetRing` 先例——`docs/temp/gen_target_ring.py` 脚本生成即终稿）。

```
FX_STYLE + a perfect thin circular ring viewed from directly above,
evenly glowing band of uniform width all around, bright white rim
fading softly to transparent both inward and outward, centered in the
frame, the ring outer edge at 95% of the image half-width
```

## 三、圣光柱 `MMT_HolyPillar`（1 张，128×256）【白图】

- **落盘**：`1.7/Textures/Things/Mote/MMT_HolyPillar.png`（配 §6-B delayedStrike/strike 皮肤参数；牧师·圣光审判、圣骑士·圣光之锤染金）。
- **UV 约定**：纵轴 = 光柱高度方向（竖直光柱，上下贯通不收口，顶端由代码裁切）。

```
FX_STYLE + a vertical pillar of holy light filling the full image
height: a broad intense white core column with soft glow fading to
black at left and right edges, gentle vertical light streaks and
faint ascending particle motes inside, uniform brightness top to
bottom, no tapering at the ends
```

## 四、光元素 race 贴图 `MMT_HolyGuardian`（4 张，128×128，`_east/_north/_south/_west`）【彩色】

- **落盘**：`1.7/Textures/Things/Pawn/HolyGuardian/`（race `MMT_HolyGuardian` 身体贴图，牧师·守护天使）。
- **要求**：人形浮游光体，无腿脚（下摆是渐散的光雾），金色半透明质感；四朝向剪影差异主要在肩线与头部微侧。

```
ICON_STYLE + a floating humanoid elemental spirit made of golden holy
light: translucent radiant body, no legs (lower body dissolves into
drifting light mist), glowing white-gold core in the chest, two arms
slightly spread, serene blank face, seen from the front (generate
south; then matching east/west/north variants with rotated shoulders
and head)
```

## 五、嘲讽标记 `MMT_TauntMark`（1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Mote/MMT_TauntMark.png`（圣骑士·嘲讽，被嘲讽敌人头顶图标）。

```
ICON_STYLE + a single bold exclamation mark shaped like a stylized
war-banner spike, bright aggressive red-orange with a subtle glow,
thick readable silhouette for a tiny overhead combat indicator
```

## 六、浮游箭簇 `MMT_ArrowCharge`（1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Mote/MMT_ArrowCharge.png`（fleck `MMT_Fleck_ArrowCharge`，猎人·多重射击浮游单元；朝北）。

```
ICON_STYLE + a single sleek steel arrow with dark green fletching,
pointing straight up (north), sharp bodkin arrowhead, subtle motion
sheen along the shaft, hunting gear style
```

## 七、陷阱皮肤 `MMT_SkillMineIce` / `MMT_SkillMinePoison`（各 1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Mote/MMT_SkillMineIce.png`、`MMT_SkillMinePoison.png`（猎人·冰霜/毒蛇陷阱；若框架支持装置染色则不出图，直接用 `MMT_SkillMine` 染——实现时定）。
- **参考**：现有 `MMT_SkillMine.png` 造型，换配色/元素。

```
ICON_STYLE + a small flat ground-concealed trap device seen from
above: round pressure plate with [ice-blue frost crystals growing
over it / sickly green vial of poison embedded in the center],
metal rim, compact readable silhouette
```

## 八、狙击曳光弹 `MMT_SnipeTracer`（1 张，64×32）【白图】

- **落盘**：`1.7/Textures/Things/Projectile/MMT_SnipeTracer.png`（投射物 `MMT_Proj_SkillSnipe`，猎人·超远狙击；朝北，drawSize (1.4, 0.5)）。

```
FX_STYLE + an extremely thin elongated bullet tracer pointing straight
up (north): a needle-thin white-hot core line with a short bright head
and a long fading streak tail, minimal width, high velocity feel
```

## 九、护卫机器人 race 贴图 `MMT_GuardBot`（4 张，128×128，四朝向）【彩色】

- **落盘**：`1.7/Textures/Things/Pawn/GuardBot/`（race `MMT_GuardBot`，工程师·部署护卫机器人）。
- **要求**：小型浮游炮机（非人形），橙色工业涂装，比 pawn 略小（代码 drawSize 0.8）。

```
ICON_STYLE + a small hovering combat robot drone: compact rounded
armored chassis with orange industrial paint and grey metal joints,
single central camera eye glowing cyan, two stubby side weapon pods,
anti-gravity glow ring underneath, no limbs, seen from the front
(generate south; then matching east/west/north variants)
```

## 十、能量屏障墙 `MMT_EnergyBarrierWall`（1 张 + 遮罩，128×128）【彩色】

- **落盘**：`1.7/Textures/Things/Building/MMT_EnergyBarrierWall.png` + `_m.png` 遮罩（建筑 `MMT_EnergyBarrierWall`，工程师·能量屏障；配 §6-B 墙体参数扩展）。链接方式同原版墙（atlases），如做单体版则单图+遮罩。

```
ICON_STYLE + a waist-high energy barrier emitter post with a vertical
translucent blue hexagonal-pattern energy screen projecting upward
from it: sci-fi shield wall, cyan-blue glow, metal base housing with
orange hazard stripes, readable as cover
```

## 十一、火箭炮塔 `MMT_SummonTurretRocket_Base/_Top`（2 张，128×128）【彩色】

- **落盘**：`1.7/Textures/Things/Building/MMT_SummonTurretRocket_Base.png` + `_Top.png`（建筑 `MMT_SummonTurretRocket`，工程师·召唤火箭炮塔；顶部朝北，比例比机枪版 `MMT_SummonTurret` 粗壮）。

```
ICON_STYLE + a heavy rocket artillery turret, top-down three-quarter
view matching RimWorld building style: [base: sturdy square bolted
platform with ammo feed hoppers / top: twin rocket launcher pods with
visible missile tips, dark grey armor with orange markings, pointing
north], chunkier than a machine-gun turret
```

## 十二、自爆机器人 `MMT_HomingBot`（1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Mote/MMT_HomingBot.png`（工程师·自爆机器人追踪弹皮肤；朝北，≈0.9 格）。

```
ICON_STYLE + a tiny four-legged spider-like demolition robot scuttling
forward (north): squat body packed with a visible orange-glowing
explosive core, small blinking red warning light, crude welded plates,
top-down three-quarter view
```

## 十三、寒冰箭 `MMT_Icebolt`（1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Projectile/MMT_Icebolt.png`（投射物 `MMT_Proj_SkillIcebolt`，法师·寒冰箭；朝北，drawSize 1.1）。

```
ICON_STYLE + a sharp elongated ice crystal bolt pointing straight up
(north): translucent pale-blue jagged icicle with a bright white
frozen core, tiny frost shard fragments trailing behind the tip,
cold mist wisp at the tail
```

## 十四、冰墙 `MMT_IceWall`（1 张 + 遮罩，128×128）【彩色】

- **落盘**：`1.7/Textures/Things/Building/MMT_IceWall.png` + `_m.png`（建筑 `MMT_IceWall`，法师·冰墙术；配 §6-B 墙体参数扩展）。

```
ICON_STYLE + a wall segment made of solid glacial ice: translucent
pale-blue crystalline blocks with white frost veins and a cold
inner glow, slightly uneven natural ice surface, matching RimWorld
wall footprint
```

## 十五、奥术飞弹 `MMT_ArcaneMissile`（1 张，64×64）【白图】

- **落盘**：`1.7/Textures/Things/Projectile/MMT_ArcaneMissile.png`（投射物 `MMT_Proj_ArcaneMissile`，法师·奥术飞弹；朝北，graphicData color `(190,120,255)` 染紫）。

```
FX_STYLE + a sleek spindle-shaped arcane missile pointing straight up
(north): elongated glowing energy dart with a bright white core,
swirling spiral filaments wrapping the body, small sparkle motes
trailing the tail
```

## 十六、奥术浮游单元 `MMT_ArcaneCharge`（1 张，64×64）【白图】

- **落盘**：`1.7/Textures/Things/Mote/MMT_ArcaneCharge.png`（fleck `MMT_Fleck_ArcaneCharge`，法师·奥术飞弹浮游充能单元；可先用默认火球充能 `MMT_Fleck_FireballCharge` 顶测）。

```
FX_STYLE + a small hovering orb of raw arcane energy: bright white
spherical core wrapped in two thin rotating rune rings, soft radial
glow, compact and centered
```

## 十七、魔法馒头 `MMT_ManaBun`（1 张，64×64）【彩色】

- **落盘**：`1.7/Textures/Things/Item/MMT_ManaBun.png`（物品 `MMT_ManaBun`，法师·魔法造物）。

```
ICON_STYLE + a single steamed bun made of glowing violet arcane
energy: soft doughy swirl shape, translucent magical glow with tiny
floating sparkle motes above it, appetizing but clearly magical
```

---

## 附：出图后标注格式

出图完成后在对应 §节标题后追加"✅ 已出图（日期）"，并确认落盘路径与 Def 引用一致（零改动原则）。
