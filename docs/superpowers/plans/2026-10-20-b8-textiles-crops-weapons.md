# B8 织物 + 作物 + 武器部件 实施计划

- 日期：2026-10-20
- spec：`docs/superpowers/specs/2026-10-20-b8-textiles-crops-weapons-design.md`（已审阅）
- 执行顺序：8.1 作物 → 8.2 织物杂项 → 8.3 武器部件 → 验证套件 → 别名/文档 → 全量验证+部署
- 遵守 B1 教训：改用户 WIP 文件前先查 git status；每小组独立提交

## Task 1：8.1 作物

1. 从 HSK 提取 10 个收获资源 def 数值（Items_Resource_Cloth_Raw/Crops/Crops_Fruits/Crops_Luxury/Wood.xml），新建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Crops.xml`。
2. 从 HSK 提取 8 个植物 def（Plants_Cultivated_Manufacture/Plants_Trees/Plants_Wild.xml），新建 `1.7/Defs/ThingDefs_Plants/MMT_Plants_Cultivated.xml`；去掉 sowResearchPrerequisites，保留 sowMinSkill/sowTags。
3. 新建 `1.7/Patches/MMT_Patch_CropHarvest.xml`：PatchOperationReplace 棉花/恶魔菇 harvestedThingDef。
4. 复制贴图：10 资源 png + 8 植物目录（按 HSK texPath）。
5. 提交：`feat: 添加 B8 作物资源与植物定义` + `feat: patch 原版棉花与恶魔菇收获物`（或合并）。

## Task 2：8.2 织物与杂项

1. 新建 `MMT_Items_Resource_Textiles.xml`（9 资源：Flaxcloth/HempCloth/Velour/Kevlar/ChitinPlating/Chitin/Tallow/PaintingSupplies/Dye），从 HSK 提取数值与 stuffProps。
2. `MMT_Items_Resource_Wood.xml` 追加 MMT_BambooPlank/MMT_RedWoodPlank（或放 Textiles 文件，执行定，推荐放 Wood）。
3. 新建 `MMT_Recipes_Textiles.xml`：9 条织物（挂 MMT_Loom）+ 兽脂/绘画×2/染料（recipeUsers 原版炉灶篝火）；MMT_Loom recipes += 9。
4. 竹板/红木板 4 条追加 `MMT_Recipes_Wood.xml`/`MMT_Recipes_ElectricWood.xml`；两台锯木台 recipes += 各 2。
5. 新建 `1.7/Patches/MMT_Patch_InsectChitin.xml`（3 虫 PatchOperationConditional 双分支）。
6. `MMT_Patch_VanillaCategories.xml` 追加 Cloth→MMT_BTextiles。
7. `MMT_Recipes_PetrochemExt.xml` 追加 MMT_MakeSurfactant_CoconutSubstitute。
8. 复制贴图：11 资源 + 提交。

## Task 3：8.3 武器部件

1. 新建 `MMT_Items_Resource_WeaponParts.xml`（14 资源，归 MMT_WeaponParts）。
2. 新建 `MMT_Buildings_WeaponParts.xml`（3 台，对照 HSK 造价/尺寸/功耗，剔除 SK 设施）。
3. 新建 `MMT_Recipes_WeaponParts.xml`（14 条；[SLDB/USLDB] 单过滤器双类别）。
4. 新建 `MMT_Research_TextilesWeapons.xml`（MMT_ArtSupplies/MMT_SuperFabric/MMT_Rocketry1，分配坐标避开冲突）。
5. 复制贴图：14 资源 + 3 建筑目录。提交。

## Task 4：验证套件

1. `tools/validate_mod.py`：顶部常量 CROPS_*/TEXTILES_*/WEAPON_PARTS_* + VANILLA_THINGS/VANILLA_RESEARCH 扩充；新函数 `validate_textiles_weapons()`；SUITES 注册 + validate_all 挂接。
2. 总数断言：INFRA_RESEARCH_TOTAL 26→29；INFRA_WORKTABLE_TOTAL 22→25。
3. 建筑 recipes 列表断言更新（MMT_Loom +9、锯木台各 +2）。
4. 提交 `test: 添加 B8 织物作物武器部件验证套件`。

## Task 5：别名与文档

1. `tools/build_mmt_chains_compare.js` MMT_ALIAS += ~40 条。
2. 重跑 `node tools/build_mmt_chains_compare.js`，核对新条目匹配。
3. 提交 `docs: B8 spec/plan 与对比文档`（spec/plan 文件一并提交）。

## Task 6：收尾

1. `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all` 全绿。
2. 部署同步 Defs+Textures+Patches 到 `D:\RimWorld v1.7.4979\Mods\rw` + diff 校验。
3. 汇总报告（git log、验证结果、进游戏验证建议）。
