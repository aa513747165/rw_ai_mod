# B7 电子零部件实施计划

- 日期：2026-10-20
- 依据：`docs/superpowers/specs/2026-10-20-b7-electronics-parts-design.md`（已审阅：拆解护盾做、修理包跳过、ComponentAdvanced 不自建）
- 架构：纯 XML Def + 贴图复制；无 C# 改动

## 目标

既有零部件/电子件类别补齐（MMT_Parts/MMT_ElectronicsCat）+ 3 新资源（动力电池/机器人部件/矿物声纳模块）+ 4 配方 + 1 研究（机器人工程 I）；扩展现有验证套件；全量验证 + 同步部署 + 分步提交。

## Task 1：贴图（3 张）

HSK `Things/Item/HexCell.png`、`Things/Item/Resource/RobotParts.png`、`Things/Item/Resource/MineralSonarModule.png` → `1.7/Textures/Things/Item/MMT/MMT_Hexcell.png / MMT_RobotParts.png / MMT_MineralSonarModule.png`。

提交：`feat: 添加 B7 电子零部件贴图`

## Task 2：类别补齐

- `MMT_Items_Resource_Parts.xml`：6 个资源 thingCategories `ResourcesRaw` → `MMT_Parts`
- `MMT_Items_Resource_Electronics.xml`：MMT_ElectronicParts/MMT_IntegratedCircuit/MMT_Microchip → `MMT_ElectronicsCat`（MMT_Silicon 保持 ResourcesRaw）

提交：`refactor: 零部件与电子件归入 MMT_Parts 与 MMT_ElectronicsCat 类别`

## Task 3：新资源（`MMT_Items_Resource_Robotics.xml`）

MMT_Hexcell（MV150/stack50/HP10/Det0.02/Flam5/爆炸 comp Flame r1.4/useHitPoints/Spacer）、MMT_RobotParts（MV70/stack75/Flam1/Exotic）、MMT_MineralSonarModule（MV300/stack25/Flam0.1/Mass6），均归 MMT_ElectronicsCat。

提交：`feat: 添加动力电池、机器人部件与矿物声纳模块资源`

## Task 4：配方 + 建筑挂载（`MMT_Recipes_Robotics.xml`）

4 条按 spec §4 表；`MMT_ElectricElectronicsTable` recipes += Hexcell/RobotParts/MineralSonarModule；`MMT_ElectricAssemblyBench` recipes += MineralSonarModule；拆解护盾写 recipeUsers=MMT_RoboticAssembler。

提交：`feat: 添加动力电池、机器人部件、声纳模块与拆解护盾配方`

## Task 5：研究

`MMT_Research_Electronics.xml` 追加 MMT_Robotics1（2000/Spacer/前置 MMT_BasicElectronics/(22,1)）。

提交：`feat: 添加机器人工程研究`

## Task 6：验证

- validate_parts：PARTS_RESOURCE_EXPECTATIONS 加 category=MMT_Parts 断言
- validate_electronics：类别改 MMT_ElectronicsCat + 3 资源/4 配方/1 研究断言；建筑 recipes 列表断言更新；VANILLA_THINGS += Apparel_ShieldBelt
- INFRA_RESEARCH_TOTAL 25→26
- `validate_mod.py all` 全绿

提交：`test: 扩展电子零部件验证套件覆盖 B7`

## Task 7：收尾

MMT_ALIAS 无需改（Hexcell/RobotParts/MineralSonarModule 同名兜底）；重跑对比文档；同步部署目录 diff 校验。

提交：`docs: 重跑 MMT 与 HSK 资源链对比文档`

## Review Focus

- 类别改动后 `MMT_ConvertResourcesRaw` 不再吃零部件（与 HSK 一致，B6 套件不受影响——其断言的是类别名而非成员）
- 建筑 recipes 列表断言同步更新，避免旧套件 FAIL
- 爆炸 comp 为原版 CompProperties_Explosive
