# AGENTS.md

面向 AI 编码代理的仓库指南：结构、构建方式与开发约定。

## 项目概述

`myModeTest`（MMT）是 RimWorld 1.7 的独立资源生产 Mod，参考 Hardcore SK（HSK）的生产链，**不依赖 HSK 程序集/Def**。内容为多条资源加工链（玻璃、混凝土、木材、金属、石化、电子、电力、陶瓷、农业、矿粉、合金、零件、煤、硫、贵金属、医药、武器、核能、仿生等），每链 = 资源 ThingDef → 配方 RecipeDef → 建筑 ThingDef（手工+电动，部分自动版）→ 研究 ResearchProjectDef。绝大部分为纯 XML Def；C# 仅承担运行时逻辑（Harmony 补丁、自定义 Comp 等）。

## 关键路径与环境

- 游戏目录：`D:\RimWorld v1.7.4979`（1.7，DLC 全，可用 DLC 依赖）；也可用环境变量 `RIMWORLD_PATH` 或 `-p:RimWorldPath=` 覆盖。
- 开发目录：`D:\project\rw`（仓库根）。
- 只读参考（**不要改动**）：`Hardcore-SK/`（HSK 克隆，分支 `1.7-super-unstable`，资源已获授权；`1.6` 子目录为中文翻译参考）；RoM（`D:\steamcmd\steamapps\workshop\content\294100\3823345404`，职业系统参照+贴图/音效，已授权，不依赖其程序集）；Isekai（`...\294100\3657580708`，天赋树参照+UI 贴图，已授权，不复制代码）。
- 生成 AI 贴图前，先让 AI 详述贴图元素再生成，尽量立体符合风格。
- 技能特效尽量不要压缩贴图，保持贴图宽高

## 目录结构

- `About/About.xml`：元数据，仅支持 1.7，声明 Harmony 运行时依赖（`brrainz.harmony`，loadAfter）。
- `LoadFolders.xml`：只挂 `1.7/`。
- `1.7/Defs/`：全部 XML Def，按 RimWorld 惯例分目录。
- `1.7/Patches/`：XPath PatchDef（改原版矿脉/配方/研究/武器等）。
- `1.7/Textures/`、`1.7/Sounds/`：已授权贴图与音效。
- `1.7/Languages/ChineseSimplified/Keyed/`：少量 Keyed 翻译；label/description 大多中文写死在 Def 里。
- `1.7/Assemblies/`：C# 输出目录（dll/pdb 已 gitignore）。
- `Source/MyModeTest/`：C# 工程；`tools/`：构建与文档脚本；`docs/`：链路文档（多为**生成物**，改 Def 后需重跑脚本）；`docs/temp/`：临时产物，勿在正式文档引用。
- `.superpowers/`：历史任务工作区；`data/`、`后继更新一.txt`：待办记录。

## 构建与测试命令

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj          # 编译 C#（需 .NET 10 SDK）

PYTHONIOENCODING=utf-8 python tools/validate_mod.py all   # 旧链全量校验（主要"测试"手段）
python tools/validate_mod.py <套件>                       # 旧链单套件（名单见文件底部 SUITES）
python tools/validate_new.py all                          # 新链全量（tools/validate/ 包自动发现）
python tools/validate_new.py <链名>                       # 新链单套件
```

文档重生成（改 Def 后按需重跑，产物见各脚本注释）：`build_mmt_chains_compare.js`、`build_resource_chains.js`、`build_research_mindmap.py`、`build_mmt_weapon_chains.py`、`build_mmt_buildings_doc.py`、`build_mmt_weapons_doc.py`、`build_mmt_items_doc.py`、`build_mmt_classes_doc.py`（后三者共用 `tools/docgen_common.py`）。

生成器脚本（产出的 XML/md 均为**生成物**，要改就改映射表/脚本重跑）：
- `build_mmt_weapons_from_hsk.py`：HSK/CE → MMT 武器 Def；人工覆盖在 `tools/data/mmt_weapon_mapping.json`；`--write`/`--check`。
- `build_mmt_bionics_from_hsk.py`：HSK → 仿生链全套 Def+贴图。
- `build_mmt_rom_classes.py`：RoM 职业系统（37 职业技能/阶位 Hediff/投射物/音效/特效）；唯一人工维护点 `tools/data/mmt_rom_class_mapping.json`；`--check` 核对全集。
- `build_mmt_talent_trees.py`：天赋树（通用树+10 家族树）；人工维护点 `tools/data/mmt_talent_tree_mapping.json`；`--check` 幂等校验。
- `apply_research_layout.py`：一次性研究坐标重排工具。

## 测试策略

无单元测试框架；`tools/validate_mod.py`（旧链专用，**已冻结**）逐字段校验 XML/贴图/数值。**新链校验一律写入 `tools/validate/<链名>.py`**（每链一个小文件），用 `from validate_mod import fail, require_files, parse_xml, ...` 复用 helper，末尾注册 `SUITES = {"<链名>": validate_<链名>}`，由 `validate_new.py` 聚合；仅当涉及旧链交叉校验才可在旧文件追加。

## C# 工程

单工程 `Source/MyModeTest/MyModeTest.csproj`（net10.0，Nullable/ImplicitUsings disable），输出直落 `1.7/Assemblies/`。经 `RimWorldPath` 引用游戏 `Assembly-CSharp.dll`、`UnityStubs.dll`、`Waylith.dll`（均不复制）。Harmony 用 NuGet `Lib.Harmony 2.3.6`（仅编译期）。**C# 注释与日志用中文。**

源文件一览（详细设计见对应 docs/ 文档）：

- 基础：`MyModeTestMod.cs`（入口+PatchAll）、`OreSiftPool.cs`（筛矿随机池）、`AutoProcessor.cs`（自动处理机）、`ChunkDrill.cs`（手动采石机）、`BionicsComps.cs`（仿生 CONN 效果）、`Rechargeable.cs`+`ChargingStation.cs`（可充能 Comp+充能台）、`ShieldPack.cs`（充能护盾腰带）、`AutoFarm.cs`（5x5 自动种植机）、`FireElemental.cs`（火元素死亡爆燃）。
- RPG/袭击：`LootRoll.cs`（装备强化/附魔掷点）、`MechStandby.cs`（机械体哨戒待机）、`BreachCharge.cs`（袭击者爆墙炸弹）、`EliteBreacher.cs`（精英爆破手）、`TradeOrder.cs`（商队订单）、`Reforge.cs`（装备品质重铸）。
- 历练/职业/天赋：`PawnProgress.cs`（XP/等级/天赋点 Comp）、`TalentXPPatches.cs`（全源 XP 钩子）、`TalentTreeDef.cs`+`TalentTreeWindow.cs`（天赋树数据+窗口）、`PawnClass.cs`（37 职业就职/升阶）、`ClassAbilityEffects.cs`（职业技能效果库）、`SummonTurret.cs`（召唤建筑 comp）。
- 任务/据点：`GuildQuest.cs`（公会狩猎委托）、`Exploration.cs`（深空扫描仪+据点）、`RpgQuestChain.cs`（远古军械库任务链）、`Dungeon.cs`（副本程序化生成）、`BossPool.cs`（Boss 池）、`BossSummoner.cs`（Dev Boss 召唤器）、`DungeonBlueprint.cs`（玩家副本蓝图）。
- 技能特效框架（SkillFX 系列，演示数据源 `1.7/Defs/Misc/MMT_SkillDemoSet.xml`，待补规划见 `docs/mmt-skillfx-backlog.md`）：`AbilityTargeting.cs`（第一步，范围瞄准+演示注入+圈内每 tick 缓存扫描）、`CastBar.cs`（第二步，读条）、`SkillShooting.cs`（单发/浮游齐射）、`SkillBeam.cs`（持续射线+引导）、`SkillStrike.cs`（天降打击）、`SkillGroundFire.cs`（地面火焰）、`SkillAura.cs`（跟随光环）、`SkillBuffFX.cs`（增益/减益图标特效）、`SkillDisplace.cs`（折跃/传送/冲锋/跳跃）、`SkillSummon.cs`（雷球/追踪弹/动物/炮塔召唤）、`SkillDelayedStrike.cs`（延时陨石）、`SkillKnockback.cs`（近战击退）、`SkillChainLightning.cs`（连锁闪电）、`SkillBreath.cs`（锥形喷吐）、`SkillHeal.cs`（范围回血）、`SkillWall.cs`（石墙术）、`SkillGroundZone.cs`（持续地面区域，含冰冻定身硬控）、`SkillTrap.cs`（触发式爆炸陷阱）、`SkillNova.cs`（新星环 360° 扩散冲击波，敌伤友疗双效）、`SkillDispel.cs`（驱散/净化，仅移除带时限的状态，友方减益/敌方增益）。

## 代码与命名约定

- defName 一律 `MMT_` 前缀；Def 文件名 `MMT_<类别>_<链名>.xml`；label/description 中文写死。
- 贴图：多状态物品 `名称_a/_b/_c.png` 放同名子目录，texPath 指向子目录；建筑贴图带朝向（`_east/_north/_south`），遮罩为方向字母+`m`（如 `..._eastm.png`），单图建筑为 `<名>.png`+`<名>_m.png`。设计细则见 `.claude/skills/rw-texture-design/SKILL.md`。
- 贴图工作流：先 Pillow 占位图落盘让 Def/校验通过，同批写 `docs/mmt-<主题>-texture-prompts.md` AI 提示词文档；终稿直接覆盖同名占位图（零改动），并在提示词文档标注已出图。
- MMT↔HSK defName 映射在 `tools/data/mmt_hsk_alias.json`，新增对应物时同步更新。
- SkillFX 视觉参数（尺寸/旋转/缩放/速度/颜色）一律从 Def XML 读取（CompProperties / MMTSkillDemoEntry 加字段透传），C# 只保留兜底默认值，禁止在 C# 里硬编码这些量；新技能落地前先在 XML 条目注释写"效果设计卡"（尺寸格数、是否旋转、缩放区间、参考物），实现逐项对照，数值锚点如陨石直径 ≈ 爆炸半径 × 1.2~1.5，如果用户没有说明应主动询问用户并提供参考。
- 提交信息用中文 conventional commits（如 `feat: 添加零件链研究与配方`）。
- 整体代码审查只在用户主动要求时执行。

## 安全与注意事项

- `Hardcore-SK/`、RoM、Isekai 参考均为只读，不要改动。
- `docs/` 多数文档为生成物，手改会被覆盖；要改就改源 Def 或生成脚本。
- 不要读取/提交凭据密钥类文件；仓库无依赖安装步骤（脚本只用标准库）。
- Windows 环境：shell 为 Git Bash，路径用正斜杠；Python 中文乱码时加 `PYTHONIOENCODING=utf-8`。
