# MMT 终局内容包 — AI 出图提示词集（喂图模型用）

> 本文档是 `mmt-endgame-texture-brief.md` 的细化执行版：把每个条目展开成可直接喂给图像生成 AI 的**英文提示词**（图模型对英文响应更稳），并标注尺寸、落盘路径与派生规则。
> 用法：**完整提示词 = 全局风格前缀 + 条目提示词 + 负面词**。出图后按 brief 第五节落盘（覆盖同名占位、工具生成遮罩、跑 validate）。

## 与 brief 的差异（以本文件为准）

1. **深空扫描仪**：Def 实为 `Graphic_Single` + `rotatable=false`（`MMT_Buildings_Exploration.xml`），**单图 512×512**，不做三朝向（brief 写的三朝向已过时）。
2. **多朝向建筑只出 south 基准图**：east/north 由工具旋转派生（`east = PIL rotate(90, expand=True)`、`north = 180°`，rw-texture-design 技能实测约定），不需要 AI 单独出图。
3. **补全 brief 未列的条目**：武器模块另有 15 张、防具模块另有 6 张已有占位但 brief 未收录（本文件 §4/§5 已含全部 46+24 张，档位取自 Def 描述）；奇点另有 5 张代码叠加层贴图（§1.6）。
4. **模块档位底色统一**：brief §10 括注（中钛蓝/高金/极紫）与 §9（中深蓝/高紫/极金）冲突，统一采用 **§9 方案**：低=铁灰/军绿、中=深蓝、高=紫、极=金/橙金+强发光。

## 〇、全局风格前缀与负面词（每张图都拼上）

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

**实操提示**：模型不支持透明底时，改用纯色背景（亮品红 `#FF00FF` 或纯白）出图后用工具抠掉；遮罩 `_m` 一律由工具生成（默认全黑），不需要 AI 出。

---

## 一、建筑贴图

### 1.1 武器强化台 `MMT_WeaponEnhanceBench`（Graphic_Multi，只出 south，256×256）

- 落盘：`Things/Building/Production/MMT_WeaponEnhanceBench/MMT_WeaponEnhanceBench_south.png`（`_east/_north` 旋转派生）

```
STYLE + a 2x2-tile precision weapon modification workbench, dark gunmetal
steel table, center of the tabletop holds a weapon clamp fixture with a small
levitating force-field cradle suspending the silhouette of a long rifle
horizontally above it, left side of the tabletop has a holographic control
screen glowing dim cyan (#7fd4e8), right side has a small quench-oil trough
and one compact articulated robotic arm, cool blue-gray metal palette
(base #6b7683, shadow #454c55, highlight #9aa5b1) with small cyan energy
accents only, a mix of high-tech workbench and blacksmith forge
```

### 1.2 深空扫描仪 `MMT_DeepSpaceScanner`（Graphic_Single 单图，512×512）

- 落盘：`Things/Building/Production/MMT_DeepSpaceScanner.png`

```
STYLE + a large 3x3-tile military phased-array radar installation, heavy
concrete-and-dark-metal base block with control cabinets, cooling grilles and
coiled cables on its south face, on top a single large flat phased-array panel
tilted slightly upward toward the upper right, panel surface covered in a
regular honeycomb of hexagonal antenna elements, a short feed-horn mast at the
panel center, a ring of dim amber indicator lights (#d9963f) along the panel
rim, palette: gray-white panel (#b8bcc0), dark gray base (#4a4d52), tiny cyan
status lights (#6fc3d8), heavy restrained military radar-station character
```

### 1.3 风暴阻尼器 `MMT_StormDampener`（Graphic_Multi，只出 south，256×256）

- 落盘：`Things/Building/Production/MMT_StormDampener/MMT_StormDampener_south.png`

```
STYLE + a 2x2-tile vertical resonance tower, square dark gray machine base
(#43464c) with small round gauges and cooling fins on its front, a Tesla-coil
style pillar rising from the base wrapped in copper coils (#b07a3f), the top
crowned by a floating ring-shaped resonator with faint blue-white electric
arcs (#cfe8ff) inside the ring, brass-and-dark-metal palette, compact
lightning-rod-meets-induction-coil character, smaller and lighter than a radar
```

### 1.4 纪元奇点四阶段（各 640×640，单图无朝向）

四张共用同一圆形底座轮廓（直径约占画布 85%），递进感必须明确。公共底座描述（BASE，拼进每张提示词开头）：

```
a giant circular 5x5-tile installation platform seen top-down, ring-shaped
gray-white base occupying 85% of the canvas diameter
```

| 阶段 | 落盘（`Things/Building/Production/<defName>/`） | 提示词（STYLE + BASE + 下行） |
|---|---|---|
| 一 `MMT_WonderFoundation` 奇点地基 | `MMT_WonderFoundation.png` | `reinforced concrete ring platform (#a9a49a), radial rebar grooves cast into the surface, rough unfinished rim edges, a recessed circular empty pit at the center, scattered construction scaffolding planks and embedded pipe stubs around the rim, pure civil-engineering feel, no high-tech elements` |
| 二 `MMT_WonderFrame` 奇点框架 | `MMT_WonderFrame.png` | `energy containment frame rising from the platform: multiple curved ribs of reinforced glass and dark carbon alloy arching inward from the ring base toward the center like an unopened metallic flower bud / dome ribcage, faint dark-cyan energy lines glowing at the rib joints (#3f7d84), center still empty, glass segments slightly translucent` |
| 三 `MMT_WonderCore` 奇点核心 | `MMT_WonderCore.png` | `the rib frame nearly closed, center now holding a dark spherical containment pod locked in place by three nested stabilization rings of matte ancient alloy, faint violet light seeping from hairline seams on the sphere (#8a5fd0), purple-black energy mood intensifying, device nearly complete` |
| 四 `MMT_Singularity` 纪元奇点 | `MMT_Singularity.png` | `completed endgame wonder BASE ONLY: polished alloy ring platform with three stabilization-ring mounts docked and folded down around an empty circular central socket, rune-like engraved grooves along the ring mounts, solemn and restrained — the central black hole and rotating rings are drawn by separate overlay layers, so the center socket must stay EMPTY` |

> 第四阶段注意：黑洞/旋转环/脉冲是代码实时叠画的（`Wonder.cs` Building_Singularity），底图中央留空，不要把黑洞画进底图。

### 1.5 沙袋人偶 `MMT_TrainingDummy`（128×128，drawSize 1.6 可略出格）

- 落盘：`Things/Building/Production/MMT_TrainingDummy/MMT_TrainingDummy.png`

```
STYLE + a 1-tile standing sandbag training dummy, thick raw wooden base post
(#8a6a44), a plump burlap sack torso (#c9b18a) tied with two rope loops
cinching a waist, a round burlap head on top with X-shaped stitched thread
eyes, sturdy endearing slightly cartoonish proportions, low-saturation warm
linen-and-wood palette
```

### 1.6 奇点叠加层（5 张，各 512×512，`Things/Building/Production/MMT_Singularity/`）

代码以水平面片实时旋转/呼吸叠画，**必须严格居中、旋转对称、透明底**；除了 Ring 三张外不带描边。

| 文件 | 用途（Wonder.cs） | 提示词 |
|---|---|---|
| `MMT_SingularityGlow.png` | 6 片发光碟堆叠成光柱（染紫色） | `pure white soft radial glow, brightest solid core at center, gaussian falloff to fully transparent at edges, circular halo like a lens flare, perfectly centered, no shapes, no outline`（黑底白晕亦可，工具转透明） |
| `MMT_SingularityHole.png` | 中央黑洞，慢速自转+呼吸 | `a perfect pure black circle with a razor-thin violet-white rim glow (#c9a8ff) and one bright thin elliptical accretion band across its equator, top-down, perfectly centered and near-symmetric, transparent background` |
| `MMT_SingularityPulse.png` | 周期性扩散脉冲环（缩放+渐隐） | `a single thin clean white ring (donut outline), soft outer edge falloff, transparent inside and outside, perfectly centered, rotationally symmetric, no other content` |
| `MMT_SingularityRingOuter.png` | 外稳定环（5°/s） | `a flat ancient-alloy stabilization ring seen from directly above: one thin circular metal band with engraved rune-like geometric grooves and faint cyan inlay (#6fc3d8), gunmetal (#4e5258), band diameter fills the full canvas, perfectly centered, transparent background` |
| `MMT_SingularityRingMid.png` | 中稳定环（-9°/s） | 同上，but `band diameter 75% of canvas, slightly different rune spacing` |
| `MMT_SingularityRingInner.png` | 内稳定环（14°/s） | 同上，but `band diameter 55% of canvas, denser rune pattern` |

---

## 二、探索资源与淬火油（各 128×128，`Things/Item/MMT/`）

| 文件 | 提示词（STYLE + 下行） |
|---|---|
| `MMT_AncientAlloy.png` 远古合金 | `a single trapezoidal ingot of ancient-civilization alloy, matte silver-gray with a faint teal-gold sheen (#9fa8a8 with #7fa8a0 tint), minimalist ancient geometric glyphs (one ring and straight lines) cast into the top face, worn aged edges, colder and more mysterious than a plain steel ingot` |
| `MMT_ArchotechShard.png` 远古碎晶 | `an irregular polyhedral crystal shard of archotech origin, deep purple-black crystal (#2a1f3d) with fine golden-white energy veins glowing from inside like lightning sealed in amber, small bright highlights against an overall dark mass` |
| `MMT_AncientDatacore.png` 远古数据核心 | `an intact palm-sized ancient data core: a rounded octahedron device with matte black-and-gold casing (#2b2620 / #c9a24a), one band of fine circular grooves around its equator, a single softly glowing cyan-white light dot (#bfe8f0) set in the center, pristine, artifact-like, jewel quality` |
| `MMT_QuenchOil.png` 淬火油 | `a small open-top iron bucket of quenching oil, dark iron banded pail (#3d3833), the open mouth reveals a dark amber oil surface (#8a5a20) with one small specular highlight, a simple flame motif stamped on the bucket side (no text), dark brown and amber palette` |

---

## 三、武器附魔模块（46 张，128×128，`Things/Item/MMT/WeaponMod/`）

**共用基底（MODBASE）**——每张提示词 = `STYLE + MODBASE + 档位底色 + 差异化图案`：

```
a small rectangular firearm modification chip the shape of a stubby pistol
magazine, brushed metal casing, gold contact pins along one short edge, one
tiny glowing status LED, a large embossed emblem on its flat face:
```

**档位底色（BASE COLOR）**：

- 低级：`casing painted iron-gray / olive drab (#6e6e66, #5c6248), dim emblem`
- 中级：`casing painted deep navy blue (#2e4a7a), moderately glowing emblem`
- 高级：`casing painted deep violet (#6b3fa0), brightly glowing emblem`
- 极品：`casing painted gold / orange-gold (#c9922e), intensely glowing emblem with a soft halo`

| defName | 中文/档位 | 差异化图案（接在 MODBASE 后） |
|---|---|---|
| `MMT_WeaponModSharp` | 锋利/低 | `a single blade edge trailing high-frequency vibration wave lines` |
| `MMT_WeaponModAccurate` | 精准/低 | `a crosshair with concentric circles` |
| `MMT_WeaponModSwift` | 轻巧/低 | `a feather with three speed lines` |
| `MMT_WeaponModPierce` | 穿透/低 | `a pointed armor-piercing bullet tip` |
| `MMT_WeaponModDeadly` | 致命/中 | `a skull merged into a bullseye weak-point marker` |
| `MMT_WeaponModGale` | 疾风/中 | `a cyclone swirl vortex` |
| `MMT_WeaponModHeavy` | 重击/中 | `a hammer head resting on an anvil block` |
| `MMT_WeaponModRapid` | 速射/中 | `three bullets side by side in a burst row` |
| `MMT_WeaponModBlitz` | 极速/中 | `three swept-back parallel speed lines` |
| `MMT_WeaponModEmp` | EMP弹/高 | `a blue electric arc bursting outward, electromagnetic pulse rings` |
| `MMT_WeaponModIncendiary` | 燃烧弹/高 | `an orange-red flame` |
| `MMT_WeaponModLifeLeech` | 吸血/高 | `a dark red cross with a blood drop being drawn into circuit traces` |
| `MMT_WeaponModShieldBreaker` | 破盾/高 | `a hexagonal shield cracking apart` |
| `MMT_WeaponModFrost` | 冰霜/高 | `an ice-blue snowflake crystal` |
| `MMT_WeaponModExplode` | 爆裂/高 | `an explosion starburst` |
| `MMT_WeaponModGas` | 毒气/高 | `three billowing toxic gas clouds` |
| `MMT_WeaponModSuppress` | 压制/高 | `a heavy arrow pressing straight down` |
| `MMT_WeaponModDisarm` | 缴械/高 | `a sword snapped in two pieces` |
| `MMT_WeaponModToxic` | 剧毒/高 | `a single hanging drop of venom` |
| `MMT_WeaponModInsectBane` | 虫敌/高 | `a beetle silhouette` |
| `MMT_WeaponModMending` | 急救/高 | `a medical cross` |
| `MMT_WeaponModGuardian` | 守护/高 | `a shield split by one vertical line` |
| `MMT_WeaponModInsight` | 悟战/高 | `an eye with clear pupil and iris` |
| `MMT_WeaponModLongRange` | 长程/高 | `a rifle scope reticle with range tick marks` |
| `MMT_WeaponModDeadeye` | 神射/高 | `a stylized eye centered behind a crosshair` |
| `MMT_WeaponModMagazine` | 扩容/高 | `two stacked extended ammo magazines` |
| `MMT_WeaponModBlinding` | 致盲/高 | `a radiant white-yellow flash starburst` |
| `MMT_WeaponModCorrode` | 蚀甲/高 | `acid drops eating pitted holes through a metal plate` |
| `MMT_WeaponModBleed` | 放血/高 | `a serrated blade edge with one dripping blood line` |
| `MMT_WeaponModSmoke` | 烟幕/高 | `billowing gray smoke puffs` |
| `MMT_WeaponModSlaughter` | 屠夫/高 | `a butcher cleaver` |
| `MMT_WeaponModDemolition` | 破城/高 | `a breaching charge cracking a wall slab` |
| `MMT_WeaponModExecute` | 处决/极 | `a falling executioner blade above a horizontal line, subtle and stylized` |
| `MMT_WeaponModChainLightning` | 连锁闪电/极 | `a lightning bolt splitting into two forks` |
| `MMT_WeaponModTimeFold` | 时空折叠/极 | `an hourglass wrapped in circular time-warp rings` |
| `MMT_WeaponModOverload` | 过载/极 | `an overheated red-glowing energy core with a warning triangle (no text)` |
| `MMT_WeaponModGambler` | 赌徒诅咒/极 | `a single die with visible pips` |
| `MMT_WeaponModRiot` | 镇暴/极 | `a riot shield with one horizontal bar` |
| `MMT_WeaponModStormCall` | 引雷/极 | `one straight lightning bolt striking down` |
| `MMT_WeaponModSureHit` | 必中/极 | `a bullseye of perfect concentric circles` |
| `MMT_WeaponModVampire` | 嗜血/极 | `a fang with small bat wings over a blood drop` |
| `MMT_WeaponModDisruptor` | 磁暴/极 | `concentric electromagnetic waves broken by a cracked magnet` |
| `MMT_WeaponModBrutal` | 暴虐/极 | `three jagged savage slash marks` |
| `MMT_WeaponModTerror` | 威慑/极 | `a subtle screaming-face silhouette trailing fear wave lines` |
| `MMT_WeaponModRangedLeech` | 汲取/极 | `a bullet trailing a siphon tendril curving back` |
| `MMT_WeaponModCorpseBurst` | 尸爆/极 | `a small skull bursting outward into green-tinged fragments` |

---

## 四、防具附魔模块（24 张，128×128，`Things/Item/MMT/ApparelMod/`）

**共用基底**同 §3 的 MODBASE，仅把 `firearm modification chip the shape of a stubby pistol magazine` 换成 `flexible armor-weave effect chip the shape of a flat rounded cartridge`（表示缝进防具纤维的效应芯片）。档位底色同 §3（低=铁灰/军绿、中=深蓝、高=紫、极=金）。

| defName | 中文/档位 | 差异化图案 |
|---|---|---|
| `MMT_ApparelModFortify` | 强固/低 | `three stacked laminated armor plates` |
| `MMT_ApparelModBladeWard` | 御刃/低 | `a shield blocking a slashing blade` |
| `MMT_ApparelModBulwark` | 壁垒/低 | `crenellated fortress wall teeth` |
| `MMT_ApparelModWarmth` | 保暖/低 | `a small sun with radiating heat waves` |
| `MMT_ApparelModCooling` | 隔温/低 | `an ice-blue hexagonal snowflake` |
| `MMT_ApparelModHeatWard` | 御热/中 | `a flame cluster blocked by one horizontal shield line` |
| `MMT_ApparelModAbsorb` | 吸收/中 | `an inward-spiraling vortex` |
| `MMT_ApparelModEvasion` | 灵巧/中 | `a zigzag Z-shaped dodge trajectory` |
| `MMT_ApparelModSwift` | 疾行/中 | `running legs with speed lines` |
| `MMT_ApparelModPorter` | 负重/中 | `a ring-handled weight` |
| `MMT_ApparelModNumbing` | 镇痛/中 | `a capsule pill trailing a soothing wave` |
| `MMT_ApparelModSteady` | 稳手/中 | `a gyroscope in balance` |
| `MMT_ApparelModGrit` | 硬撑/中 | `a clenched fist over a steady heartbeat line` |
| `MMT_ApparelModImmunity` | 免疫/中 | `a shield with antibody Y-shapes` |
| `MMT_ApparelModRecovery` | 复苏/高 | `a medical cross in regenerative green tones` |
| `MMT_ApparelModEmpShield` | 法拉第/高 | `a hexagonal Faraday-cage mesh` |
| `MMT_ApparelModDiligent` | 勤勉/高 | `a single gear` |
| `MMT_ApparelModThorns` | 荆棘/高 | `a row of upward spikes` |
| `MMT_ApparelModDischarge` | 放电/高 | `a bursting electric spark star` |
| `MMT_ApparelModMindWard` | 灵壁/高 | `a brain silhouette under a protective dome` |
| `MMT_ApparelModRegen` | 再生/高 | `a sprouting leaf over a medical cross` |
| `MMT_ApparelModAegis` | 神佑/极 | `a shield with a halo hovering above it` |
| `MMT_ApparelModFeather` | 轻羽/极 | `a single feather` |
| `MMT_ApparelModToxicCloud` | 毒雾/极 | `a hanging drop of venom over a small toxic cloud` |

---

## 五、世界地图据点图标（3 张，128×128，`World/WorldObjects/Sites/`）

世界图标在游戏里会缩到很小，**剪影辨识度优先**，细节从简：

| 文件 | 提示词（STYLE + 下行） |
|---|---|
| `MMT_SiteOrbitalRuin.png` 轨道遗迹 | `a small crashed fragment of a ring-shaped space station: a broken ring segment hull tilted and half-buried, torn cross-section decks visible at the break, scorched burn marks, gray-white and charred-black palette, bold readable silhouette` |
| `MMT_SiteMechStronghold.png` 机械据点 | `a small mechanoid stronghold: a dark angular polygonal bunker with one turret and antenna masts on top, tiny defense-tower silhouettes at its sides, cold gray metal with dim red indicator lights (#a03a30), menacing, bold readable silhouette` |
| `MMT_SiteRareDeposit.png` | `a small exposed rich ore vein: clusters of metallic crystals with a teal-gold sheen (#7fa8a0) jutting from a cracked rock fissure, scattered rubble stones around, bold readable silhouette` |

---

## 六、出图后处理（同 brief 第五节）

1. 裁剪/缩放到条目标注尺寸，PNG RGBA 透明底。
2. 多朝向建筑：south 出图后用 PIL 派生 `east = rotate(90, expand=True)`、`north = rotate(180)`，再统一生成全黑遮罩（`_southm/_eastm/_northm`、`_m`）。
3. 覆盖同名占位文件，Def 无需改动。
4. `PYTHONIOENCODING=utf-8 python tools/validate_new.py all` + 游戏内目检。

**清单合计（AI 需出主图）**：建筑 12 张（强化台 1 + 扫描仪 1 + 阻尼器 1 + 奇观 4 + 人偶 1 + 叠加层 6 中 Glow/Hole/Pulse/Ring×3 计 6）＋ 资源物品 4 张 ＋ 武器模块 46 张 ＋ 防具模块 24 张 ＋ 世界图标 3 张 = **89 张**（多朝向与遮罩由工具派生）。
