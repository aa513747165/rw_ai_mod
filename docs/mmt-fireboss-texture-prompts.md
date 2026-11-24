# MMT 熔火元素 Boss — AI 出图提示词集（喂图模型用）

> 与 `mmt-dungeon-texture-prompts.md`、`mmt-class-texture-prompts.md`、`mmt-endgame-texture-prompts.md` 同格式：把熔火元素领主 Boss 落地时**用程序（Pillow）生成顶替的贴图**展开成可直接喂给图像生成 AI 的英文提示词。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**。

## 背景：为什么这些图要重出

熔火元素领主 Boss（`MMT_BossFireElementalLord`，魔兽火元素风格，三技能：召唤小火元素/流星火雨/熔岩火柱）落地时，以下 6 张贴图是 `tools/make_fire_elemental_textures.py` 用 Pillow 纯代码绘制的临时占位（几何块面拼合，10KB 级）：

- `Things/Pawn/FireElemental/MMT_FireElementalLord_south/_east/_north.png`：Boss 本体三向（256×256，drawSize 2.4）；
- `Things/Pawn/FireElemental/MMT_FireElementalLesser_south/_east/_north.png`：召唤物小火元素三向（128×128，drawSize 1.2）。

技能图标（`UI/RoM/summon_demon`、`UI/RoM/firestorm`、`UI/RoM/sunfire`）与施法特效（`Things/Fleck/RoM/FlameMote`）均为已授权的 RoM 原图，**不在重出范围**。

## 〇、全局风格前缀与负面词

**风格前缀（PAWN_STYLE，pawn 精灵专用，与物品/建筑的 STYLE 略有差异——分朝向绘制）**：

```
RimWorld video game pawn sprite, stylized 3/4 top-down view, light source
from upper left, thick dark outline (very dark warm brown #1a1410, never
pure black), soft hand-painted cel shading with 2-3 flat value steps, muted
palette with one saturated accent, subtle paper-grain texture, single
creature centered occupying 70-85% of canvas, transparent background, clean
bold silhouette readable at small size
```

**负面词（NEGATIVE）**：

```
text, letters, numbers, watermark, logo, photorealistic, photograph, 3D
render, isometric, full side profile, multiple creatures, scenery, ground
texture, drop shadow on background, human face, skin, clothing
```

**本包补充约束**：

- 每个角色 **south/east/north 三张为同一角色**，必须一起喂图（或多轮会话内连续出图）保持岩块分布/火焰形态/配色一致；east 为朝右侧视，north 为背面；
- 两个角色是**同族大小号**：小火元素 = 领主的简化幼体（同材质、同火焰、去掉复杂四肢），喂图时最好同批出，保证一眼看出"父子"关系；
- 所有图 **RGBA 透明底**；模型不支持透明底时用亮品红 `#FF00FF` 底出图后抠掉；
- pawn 贴图**不需要遮罩**（无 `_m`），也无需染色通道；
- 核心色板（两角色共用）：岩体炭黑 `#241d1a` / 岩体固有色 `#4a3830`、熔岩橙 `#e25822` / 亮橙 `#ff9a3c`、核心亮黄 `#ffd75e` / 白炽 `#fff3d0`。

---

## 一、熔火元素领主 `MMT_FireElementalLord`（3 张，256×256）

- **落盘**：覆盖 `Things/Pawn/FireElemental/MMT_FireElementalLord_south/_east/_north.png`（Graphic_Multi 按后缀取图，Def 无需改动）。
- **功能**：副本 Boss 巢穴统领者（combatPower 500），魔兽火元素风格——黑曜岩躯 + 熔岩裂纹 + 火焰鬃毛。

**共同主体描述（三段提示词共用，替换朝向句即可）**：

```
PAWN_STYLE + a towering fire elemental lord: a humanoid torso built from
stacked obsidian volcanic rock plates (#241d1a to #4a3830) with glowing
molten lava (#e25822) seeping through the cracks like veins, two massive
chunky rock arms ending in boulder fists, a crown of jagged basalt spikes
on the head, and a mane of living flame (outer #ce4a18, inner #ffd05c)
erupting from its shoulders and back
```

### 1.1 `_south`（正面，唯一露脸朝向）

```
<共同主体> + front view facing the viewer, two small furious white-hot
glowing eyes (#fff3d0) under heavy rock brow ridges, a bright molten core
orb (#ffd75e with white center) embedded in the chest like a heart,
flames rising highest behind the shoulders
```

### 1.2 `_east`（朝右侧视）

```
<共同主体> + side view facing right, one arm slightly forward, chest core
visible as a glowing edge-on seam, the flame mane trailing toward the
back, same rock plate layout as the front view
```

### 1.3 `_north`（背面）

```
<共同主体> + back view, no eyes, a smaller molten core ember glowing
between the shoulder blades, the flame mane largest and most prominent
from this angle, rock plates reading as a solid armored shell
```

---

## 二、小火元素 `MMT_FireElementalLesser`（3 张，128×128）

- **落盘**：覆盖 `Things/Pawn/FireElemental/MMT_FireElementalLesser_south/_east/_north.png`。
- **功能**：Boss 技能①的限时召唤物（combatPower 70，存在 60 秒）；**幼体感**是关键——简单、圆、萌中带凶。

**共同主体描述**：

```
PAWN_STYLE + a small juvenile fire elemental: a single floating cracked
obsidian rock core (#241d1a, lava #e25822 glowing through the cracks)
ringed by a wreath of small lively flames, two tiny pebble-like rock
fragments orbiting beside it, no legs, cute but menacing
```

### 2.1 `_south`（正面）

```
<共同主体> + front view facing the viewer, two small round glowing
white-hot eyes (#fff3d0) on the rock core, flames slightly taller above
the head like a fiery hair tuft
```

### 2.2 `_east`（朝右侧视）

```
<共同主体> + side view facing right, one orbiting rock fragment visible
ahead of the core, flame wreath trailing slightly backward
```

### 2.3 `_north`（背面）

```
<共同主体> + back view, no eyes, a small ember glow at the core's rear
crack, flame wreath fully visible around the silhouette
```

---

## 三、出图后处理

1. 裁剪/缩放到条目标注尺寸（Boss 256×256，小元素 128×128），**PNG RGBA 透明底**。
2. **无需遮罩**：pawn 贴图不走 CutoutComplex 染色通道。
3. 文件名必须保持 `<texPath 末段>_south/_east/_north.png` 后缀约定（Graphic_Multi 按朝向取图），直接覆盖同名占位文件，Def/代码零改动。
4. 两张 east 可先出 south 再让模型按同一角色转侧视，**不要简单水平翻转**（火焰飘向与明暗方向会错）。
5. 验收：`PYTHONIOENCODING=utf-8 python tools/validate_new.py fireboss`（含贴图存在性检查）+ 游戏内 DEV 模式用 Boss 召唤器生成实测目检。

**清单合计**：Boss 3 + 小火元素 3 = **6 张**。
