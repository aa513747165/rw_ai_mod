---
name: rw-texture-design
description: 为本 Mod（MMT，RimWorld 1.7）设计/生成物品与建筑贴图。当需要新建、修改或补全 1.7/Textures 下的贴图、遮罩（_m）、多朝向（_east/_north/_south）或多状态（_a/_b/_c）贴图时使用。涵盖 MMT 命名约定、HSK 风格参考、Python/Pillow 纯代码出图流程与验收清单。
---

# RimWorld / MMT 贴图设计

为 MMT 制作 RimWorld 风格贴图。目标：风格与 HSK / 原版一致（俯视、厚深色描边、柔和手绘填色），文件布局符合本仓库约定，Def 引用一次通过验证。

## 关键事实（已核对仓库现状）

- 贴图根目录：`1.7/Textures/Things/`。所有 PNG，尺寸统一 **128×128**（1 格建筑/物品；多格建筑按格数等比放大，如 3×2 → 384×256）。
- 建筑贴图：`Things/Building/Production/<defName>/` 子目录，内部两种形态：
  - 单图（Graphic_Single）：`<defName>.png` + 遮罩 `<defName>_m.png`（例：`MMT_ElectricGrinder/`）。
  - 多朝向（Graphic_Multi）：`<defName>_east.png` / `_north.png` / `_south.png`，遮罩为 `<defName>_eastm.png` / `_northm.png` / `_southm.png`（**方向字母后直接接 m，无下划线**，例：`MMT_EConcreteMixer/`）。
- 物品贴图：`Things/Item/Resource/` 下，单图为 `<Name>.png`；多状态物品放同名子目录 `<Name>/<Name>_a.png` / `_b.png` / `_c.png`，texPath 指到子目录路径（不含扩展名）。
- Def 侧引用：`graphicData.texPath` 指向贴图路径（无 `.png`），`shaderType` 用 `CutoutComplex` 时游戏按 `<texPath>_m` 约定自动加载遮罩；`drawSize` 与格数一致，单向建筑加 `<drawRotated>false</drawRotated>`。
- 遮罩规则（CutoutComplex，红/绿通道）：红色区域 = 主材质色（stuffColor/thingColor），绿色区域 = 次要色，黑 = 不染、白灰 = 按明度染色。不需要染色的建筑也建议保留全黑 `_m` 以统一约定（仓库现有建筑均带 `_m`）。
- 特殊材质（如 Shield/ 下的护盾）有成对的 `.dds.zstd`，属个别压缩格式，普通物品/建筑一律用 PNG。
- HSK 参考贴图（只读！）：`Hardcore-SK/Mods/` 下各模组的 Textures 目录；本仓库已复制的授权贴图也可作风格基准。

## 工作流程

1. **定位参考**：先找同类已有贴图作基准。优先看本仓库 `1.7/Textures` 中同链贴图（如工作台类看 `MMT_ElectricGrinder`、`MMT_AdvWeaponCraftingTable`），再看 HSK 对应建筑的贴图。用 ReadMediaFile 查看 2-3 张参考图，记录：轮廓粗细、主色板、明暗方向（RimWorld 惯例光源在上方偏左）、地面投影。
2. **确定规格**：确认建筑格数（drawSize）与朝向需求（是否 Graphic_Multi）、是否需遮罩染色、物品是否多状态。据此确定文件名清单。
3. **生成贴图**：用 Python + Pillow 纯代码绘制（不依赖图像生成 API，可复现、可迭代）：
   - 画布透明底，RGBA。
   - 先画 4-6px 深色描边剪影（近黑但带色相，如 `#1a1410`），再向内填色。
   - 填色用 2-3 阶明暗块面（亮部/固有色/暗部），可加少量噪点纹理模拟手绘感。
   - 俯视角度：可见顶面 + 朝南侧面，侧面用暗部色。
   - 多朝向版本：先画 south，再派生 north（去细节正面）/ east（侧面轮廓），不要简单翻转。
   - 遮罩单独生成：需要被游戏染色的区域涂纯红（或红+绿），其余纯黑。
   - 每步用 ReadMediaFile 回看成果，与参考图对比迭代。
4. **落盘**：按上面"关键事实"的路径与命名写入 `1.7/Textures/`。
5. **接线 Def**：在对应 ThingDef 的 `graphicData` 中写好 texPath/shaderType/drawSize；多朝向用 `Graphic_Multi`。
6. **验证**：
   - `PYTHONIOENCODING=utf-8 python tools/validate_mod.py <相关套件>` 或 `python tools/validate_new.py <链名>`（含贴图存在性检查）。
   - 最终用 ReadMediaFile 逐张检查产物：尺寸正确、透明边干净、遮罩与主图对齐。

## 风格速查

- 描边：粗、深色、略不规则（手绘感），不要用纯黑 `#000`。
- 配色：低饱和、偏大地色系；金属偏冷灰蓝，石木偏暖。
- 比例：物品贴图内容约占画布 60-80%，四周留透明边。
- 禁止：照片级渐变、锐利矢量感、文字/字母入图、与 HSK 风格冲突的高饱和霓虹色。

## 验收清单

- [ ] 文件名/目录符合 MMT 约定（含 `_m` 或 `_eastm` 等遮罩）
- [ ] 尺寸 = 128 × 格数，PNG RGBA 透明底
- [ ] texPath 无扩展名且与实际路径一致
- [ ] 遮罩通道符合红/绿染色规则并与主图逐像素对齐
- [ ] 对应 validate 套件通过
- [ ] ReadMediaFile 目检通过，风格贴近参考图
