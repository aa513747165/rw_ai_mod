# MMT 副本/RPG 系统 — AI 出图提示词集（喂图模型用）

> 与 `mmt-class-texture-prompts.md`、`mmt-endgame-texture-prompts.md` 同格式：把副本据点/RPG 任务链系统实现时**用程序生成色块顶替的贴图**展开成可直接喂给图像生成 AI 的英文提示词。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**（§三设计器图标除外，用自带的 UI 风格）。

## 背景：为什么这些图要重出

副本据点（`MMT_WorldObjects_RpgDungeons.xml`）与 Boss 召唤/蓝图设计器落地时，以下贴图是 Python 程序绘制的临时占位（纯色块+简单几何，1~3KB）：

- `World/WorldObjects/Sites/MMT_SiteTrainingGround.png`、`MMT_SiteClassShrine.png`、`MMT_SiteBossLair.png`：三张世界地图据点图标；
- `Things/Building/Production/MMT_BossSummoner.png`：Boss 召唤器（DEV 建筑）；
- `UI/Designators/MMT_BlueprintSave.png`：建筑师菜单「副本蓝图」保存设计器图标。

公会委托站点图标（`UI/TalentTree/World/Isekai_Expedition`）是已授权的 Isekai 原图，**不在重出范围**；同目录的 `MMT_SiteOrbitalRuin/MechStronghold/RareDeposit` 已按 `mmt-endgame-texture-prompts.md` §五出过图，也不在本包。

## 〇、全局风格前缀与负面词（与终局包一致）

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

- 三张据点图标是**同一系列**，且要与已出图的轨道遗迹/机械据点/富矿脉同世界观感：世界地图上会缩到很小，**剪影辨识度优先、细节从简**；出图时把三张（最好连同终局包 §五的三张描述）一起喂给模型保持系列感；
- 所有图必须 **RGBA 透明底**；模型不支持透明底时用纯色背景（亮品红 `#FF00FF`）出图后抠掉；
- Boss 召唤器是 DEV 测试工具，优先级最低，但气质要走「禁忌召唤装置」，与正规生产建筑区分。

---

## 一、世界地图据点图标（3 张，128×128，`World/WorldObjects/Sites/`）

- **落盘**：覆盖同名占位文件，Def 无需改动（`siteTexture`/`expandingIconTexture` 共用同一张）。
- **来源**：副本据点系统（`mmt-dungeon-design.md`），由深空扫描仪/任务链生成。

### 1.1 试炼场 `MMT_SiteTrainingGround`

- **功能**：远古战士的试炼之地，被武装势力占据；围墙内库房封存强化装备与历练典籍（威胁系数 ×1.2，三档全开放）。

```
STYLE + a small ancient warrior proving ground: a sturdy wooden practice
dummy with two crossed swords planted behind it, a broken low stone wall arc
at its base suggesting a ruined training yard, warm carved wood (#7a5a38) and
weathered gray stone palette, martial and grounded, bold readable silhouette
```

### 1.2 职业圣殿 `MMT_SiteClassShrine`

- **功能**：供奉三十六种职业纹章的远古圣殿，深处祭坛散落职业凭证碎片（威胁系数 ×1.4，II 档起出凭证碎片）。

```
STYLE + a small ancient class shrine: a stepped gray stone altar flanked by
two short carved pillars, a round bronze medallion emblem resting on the
altar glowing faint violet (#8a5fd0), weathered temple masonry with moss in
the seams, sacred solemn mood, bold readable silhouette
```

### 1.3 远古守卫巢穴 `MMT_SiteBossLair`

- **功能**：Boss 巢穴，威胁系数 ×2.0、锁定 III 档，系列中的最高危据点；统领者蛰伏于中央巢位。

```
STYLE + a small menacing boss lair: a dark jagged mound of rock and bone
spikes with a cave-like opening, a single glowing red eye (#c03a30) staring
from the darkness within, faint red embers at the mound base, near-black
palette with one ominous red accent, clearly the most dangerous site of the
set, bold readable silhouette
```

---

## 二、Boss 召唤器 `MMT_BossSummoner`（建筑，128×128，Graphic_Single，drawSize 1.4）

- **落盘**：`Things/Building/Production/MMT_BossSummoner.png`（直接覆盖；Def 无 `shaderType` 走默认 Cutout，**不需要遮罩**）。
- **功能**：DEV 模式专用测试装置，向虚空发出挑衅信号，把指定 Boss 拉到本图立即进攻（`BossSummoner.cs`）。
- **优先级**：低（正常游玩不可见）。

```
STYLE + a small forbidden summoning beacon device: a squat dark gunmetal
pedestal (#454048) with etched warning runes around its rim, a jagged
blood-red crystal shard (#c03a30) levitating above its center, thin dark-red
energy wisps circling the crystal tip, ominous arcane-tech ritual character,
light and compact
```

---

## 三、副本蓝图保存图标 `MMT_BlueprintSave`（UI 设计器图标，64×64，`UI/Designators/`）

- **落盘**：覆盖 `UI/Designators/MMT_BlueprintSave.png`（`DungeonBlueprint.cs` Designator_SaveDungeonBlueprint 按此路径加载）。
- **注意**：这是建筑师菜单的设计器图标，**不要用 STYLE 前缀**——原版设计器图标是扁平单色素描风，不是手绘精灵。用下面自带风格的提示词：

```
flat minimal game UI icon, RimWorld architect menu designator style: a faint
square blueprint grid with a dashed selection rectangle on it and one small
four-pointed star at the corner, thin pale desaturated blue-gray lines
(#c8d4e0) on transparent background, crisp even line weight, no shading, no
gradients, no text, simple geometric glyph readable at 32 pixels
```

---

## 四、出图后处理

1. 裁剪/缩放到条目标注尺寸，**PNG RGBA 透明底**（占位图多为程序生成的实底或小色块，注意不是简单覆盖格式）。
2. 本包**无需遮罩**：据点图标与 UI 图标不走 CutoutComplex；Boss 召唤器为默认 Cutout。
3. 全部直接覆盖同名文件，Def/代码均无需改动。
4. `PYTHONIOENCODING=utf-8 python tools/validate_new.py rpg_quests` + `dungeon_blueprint` + 游戏内目检（世界地图据点图标、DEV 模式建召唤器、建筑师菜单副本蓝图按钮）。

**清单合计**：据点图标 3 + Boss 召唤器 1 + 设计器图标 1 = **5 张**。
