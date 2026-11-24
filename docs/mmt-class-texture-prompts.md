# MMT 职业系统 — AI 出图提示词集（喂图模型用）

> 与 `mmt-endgame-texture-prompts.md` 同格式：把职业系统（RoM 复刻）实现时**用占位图顶替的贴图**展开成可直接喂给图像生成 AI 的英文提示词。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**。

## 背景：为什么这些图要重出

职业系统落地时，以下贴图是临时占位：

- `Things/Building/Production/RoM/MMT_ClassMentorBench.png`、`MMT_SummonPylon.png`：直接拿 RoM 魔法阵 UI 图顶替，**黑底、RGB 无透明通道、UI 图案而非建筑**，且 6 个召唤建筑共用同一张；
- `Things/Item/MMT/MMT_ClassToken.png`：程序生成的色块占位。

技能图标（`UI/RoM/` 174 张）与投射物（`Things/Projectile/RoM/`）是已授权的 RoM 原图，**不在重出范围**。

## 〇、全局风格前缀与负面词（每张图都拼上，与终局包一致）

**风格前缀（STYLE）**：

```
RimWorld video game sprite, straight top-down view with the south-facing side
slightly visible, light source from upper left, thick dark outline (very dark
warm brown #1a1410, never pure black), soft hand-painted cel shading with 2-3
flat value steps, muted low-saturation earthy palette, subtle paper-grain
texture, single centered object occupying 60-80% of canvas, transparent
background, clean silhouette
```

**负面词（NEGATIVE）**：

```
text, letters, numbers, watermark, logo, photorealistic, photograph, 3D render,
isometric 45-degree view, side view, neon colors, high saturation, smooth
gradients, airbrush, blur, multiple objects, scenery, ground texture, drop
shadow on background
```

**本包补充约束**：

- 召唤物是**魔法/科技造物**，允许比常规建筑更亮一点的能量色（紫/青/绿），但仍忌大面积霓虹；
- 三张图腾是**同一系列**：共用"木雕图腾柱 + 元素符文带"基底，只在顶端造型与配色上区分元素，出图时把另两张的描述一起喂给模型保持系列感；
- 所有图必须 **RGBA 透明底**（占位图是 RGB 黑底，直接覆盖时务必转成透明底）。

---

## 一、职业导师台 `MMT_ClassMentorBench`（2×2，Graphic_Single 单图，256×256）

- **落盘**：`Things/Building/Production/RoM/MMT_ClassMentorBench.png`（手写 Def `MMT_Buildings_ClassMentor.xml`，texPath 不变，直接覆盖）
- **功能**：持职业凭证的小人在此觉醒灵能、就职 37 种职业（法师/斗士双体系）并升阶。
- **现状**：占位为 RoM 黑底魔法阵 UI 图，必须换掉。

```
STYLE + a 2x2-tile class mentor ritual podium, a waist-high dark stone
lectern table on a hardwood base, its top surface carved with a shallow
glowing rune ring (#7a5fc0 faint violet), an open large tome resting on the
lectern, a knight's sword and a mage's staff crossed and mounted on the front
face of the podium symbolizing the fighter-and-mage dual path, two small
candles or a tiny brazier at one corner, palette: dark slate stone (#4a4440),
warm wood (#7a5a38), muted bronze fittings, only the rune ring glows softly,
solemn arcane-but-grounded character
```

---

## 二、召唤建筑（6 张，各 128×128，Graphic_Single，drawSize 1.4 内容可略出格）

- **现状**：6 个 def 共用占位图 `RoM/MMT_SummonPylon.png`（黑底魔法阵）。
- **落盘**：建议按仓库规范落到各自子目录 `Things/Building/Production/<defName>/<defName>.png`，然后改 `tools/data/mmt_rom_class_mapping.json` 里 `summonBuildings` 各条的 `tex` 字段并**重跑 `python tools/build_mmt_rom_classes.py`**（`MMT_Buildings_RoMSummons.xml` 是生成物，不要手改）。想省事也可直接在 `RoM/` 目录按新文件名落 6 张并同步改 `tex`。
- **共同气质**：限时存在的召唤物（30s~60s），比正规建筑轻巧、带"临时造物"的能量感；统一附带一句 `a temporary summoned construct, slight magical glow, light and compact`。
- 遮罩 `_m`：与主图同名生成全黑遮罩（占位图目前连遮罩都没有，一并补齐）。

### 2.1 魔法防御塔 `MMT_Bld_SummonPylon`（法师系，TM_SummonPylon，子弹伤害）

```
STYLE + a temporary summoned construct, slight magical glow, light and
compact: an arcane pylon turret — a short rune-carved stone base with a
levitating violet crystal obelisk (#8a5fd0) hovering above it, thin arcane
energy wisps orbiting the crystal tip where projectiles are fired, purple
arcane palette with pale lavender highlights, clearly a magical artillery
pylon
```

### 2.2 魔法地雷 `MMT_Bld_ManaMine`（法师系，TM_SummonExplosive，近身爆炸）

```
STYLE + a temporary summoned construct, slight magical glow, light and
compact: an arcane mana mine — a low flat rune-inscribed metal disc
half-sunken into the ground, a pulsing violet-white energy core (#b48ae8) set
in its center like a trigger eye, thin warning rune ring etched around the
rim, unobtrusive landmine silhouette, dark gunmetal (#454048) with violet
glow
```

### 2.3 神经炮塔 `MMT_Bld_TechnoTurret`（技师系，TM_TechnoTurret，子弹伤害）

```
STYLE + a temporary summoned construct, light and compact: a deployable
techno gun turret — a squat tripod gun platform with a single stubby barrel
aimed upward-south, a small neural-link antenna dish on the back with a dim
cyan status light (#6fc3d8), brushed gunmetal and dark steel palette
(#5a6068 / #3a3e44), clean spacer-tech look, the ONLY non-magical entry of
the set
```

### 2.4 闪电图腾 `MMT_Bld_TotemLightning`（萨满系，TM_SummonTotemLightning，EMP 电击）

```
STYLE + a temporary summoned construct, slight magical glow: a shaman totem
series — carved wooden totem pole on a small stone footing, this one topped
with a copper coil and a crackling blue-white electric arc (#cfe8ff) between
two prongs, lightning bolt glyph carved into the shaft glowing faint blue,
warm carved wood (#7a5a38) with copper and electric-blue accents
```

### 2.5 大地图腾 `MMT_Bld_TotemEarth`（萨满系，TM_SummonTotemEarth，钝器石块）

```
STYLE + a temporary summoned construct, slight magical glow: a shaman totem
series — carved wooden totem pole on a small stone footing, this one wrapped
with rough stacked stones and moss at its base, a cluster of small rocks
levitating around the totem head ready to be hurled, mountain glyph carved
into the shaft, earthy brown-and-ochre palette (#8a6a44 / #a08a5a), heavy
grounded feel
```

### 2.6 治疗图腾 `MMT_Bld_TotemHealing`（萨满系，TM_SummonTotemHealing，范围治疗）

```
STYLE + a temporary summoned construct, slight magical glow: a shaman totem
series — carved wooden totem pole on a small stone footing, this one crowned
with woven leaves and a soft green glowing life-rune (#7fbf6a), faint green
motes drifting upward around the shaft, warm carved wood with gentle
leaf-green accents, calm restorative mood
```

---

## 三、职业凭证 `MMT_ClassToken`（物品，128×128）

- **落盘**：`Things/Item/MMT/MMT_ClassToken.png`（直接覆盖程序生成的色块占位）
- **功能**：就职/升阶的消耗凭证，Dev 发放；对应"斗士 + 法师"双职业体系。

```
STYLE + a class credential medallion: a round palm-sized metal badge, split
relief emblem on its face — a knight's sword on the left half and a mage's
staff on the right half crossed at the center, symbolizing the dual
fighter-and-mage class system, aged bronze with dark gold trim (#a8783a /
#6a5230), faint violet-and-steel two-tone sheen, a short tattered ribbon
loop at the top edge, heavy engraved coin-like feel
```

---

## 四、出图后处理

1. 裁剪/缩放到条目标注尺寸，**PNG RGBA 透明底**（占位图是 RGB 黑底，注意不是简单覆盖格式）。
2. 同步生成全黑遮罩：导师台 `MMT_ClassMentorBench_m.png`、各召唤物 `<defName>_m.png`（CutoutComplex 约定）。
3. 召唤物改 `texPath`：编辑 `tools/data/mmt_rom_class_mapping.json` 的 `summonBuildings.*.tex` → 重跑 `python tools/build_mmt_rom_classes.py` → `python tools/build_mmt_rom_classes.py --check` 确认无漂移。
4. `PYTHONIOENCODING=utf-8 python tools/validate_new.py rom_classes` + 游戏内目检。

**清单合计**：导师台 1 + 召唤建筑 6 + 职业凭证 1 = **8 张**（遮罩由工具生成）。
