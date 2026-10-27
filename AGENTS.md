# AGENTS.md

本文件面向 AI 编码代理，介绍本仓库的结构、构建方式与开发约定。阅读前假设你对本项目一无所知。

## 项目概述

`myModeTest`（简称 MMT）是 RimWorld 1.7 的独立资源生产 Mod，参考 Hardcore SK（HSK）的资源生产链实现，但**不依赖 HSK 的程序集或 Def**。核心内容是若干条完整的资源加工链：玻璃、混凝土、木材、金属、石化、电子、电力（电力木/电力石化/电力电子）、陶瓷、农业、矿粉（磨矿/筛矿）、高级合金、零件、煤、硫、贵金属、医药、武器（古代/常规/未来/炮塔/NPC）、核能与物质、仿生/义肢（含 CyberneticOrganism_SK 的 CONN 系列，不含 Dubs 卫生仿生）等。每条链通常包含：资源 ThingDef → 配方 RecipeDef → 建筑 ThingDef（手工 + 电动，部分含自动版）→ 研究 ResearchProjectDef。

绝大部分内容以纯 XML Def 实现，C# 仅承担少量无法 XML 表达的运行时逻辑（Harmony 补丁、自定义 Building Comp、矿石随机池等）。

## 关键路径与环境

- 游戏目录：`D:\RimWorld v1.7.4979`（RimWorld 1.7，DLC 全，可使用 DLC 依赖）
- 开发目录：`D:\project\rw`（即本仓库根）
- HSK 参考克隆：`Hardcore-SK/`（分支 `1.7-super-unstable`），**只读参考，不要改动**
- 中文翻译参考：`Hardcore-SK/1.6`
- 所有 HSK 资源已获授权使用

## 目录结构

- `About/About.xml`：Mod 元数据；supportedVersions 仅 1.7；声明 Harmony 运行时依赖（`brrainz.harmony`，loadAfter 置于其后）。
- `LoadFolders.xml`：只挂载 `1.7/` 目录。
- `1.7/Defs/`：全部 XML Def，按 RimWorld 惯例分目录（`ThingDefs_Resources`、`RecipeDefs`、`ThingDefs_Buildings`、`ResearchProjectDefs`、`ThingDefs_Weapons`、`HediffDefs`、`JobDefs`、`WorkGiverDefs`、`SoundDefs`、`StatDefs`、`StuffCategoryDefs`、`ThingCategoryDefs`、`ThingDefs_Items`、`ThingDefs_Plants`）。
- `1.7/Patches/`：XPath PatchDef，用于修改原版（Vanilla）矿脉、配方、研究、武器、药品、发电等。
- `1.7/Textures/`：已获授权复制的 HSK 静态贴图（`Things/Item`、`Things/Building/Production` 等）。
- `1.7/Sounds/`：武器与炮塔音效（.ogg 及按武器名组织的子目录）。
- `1.7/Languages/ChineseSimplified/Keyed/`：少量 Keyed 翻译（如检查面板标签）；绝大多数 label/description 直接以中文写死在 Def 里，无完整本地化目录。
- `1.7/Assemblies/`：C# 编译输出目录（`.dll/.pdb/.deps.json` 已被 .gitignore 忽略）。
- `Source/MyModeTest/`：C# 工程（见下）。
- `tools/`：构建与文档生成脚本（Python + Node.js）。
- `docs/`：从 HSK 提取/对比生成的链路文档（多为**生成物**，改 Def 后需重跑对应脚本）。其中 `hsk-bionic-chains.md` 由一次性脚本 `docs/temp/extract_hsk_bionics.py` 生成（HSK 全部仿生配件：材料/制作/安装/作用）。
- `docs/temp/`：一次性提取脚本与中间 JSON，属临时产物，**不要在正式文档中引用**。
- `.superpowers/`：历史任务工作区（如 `sdd/2026-10-21-core-sk-weapons`）。
- `data/`、根目录 `后继更新一.txt`：待办/需求记录。

## 构建与测试命令

```bash
# 编译 C#（需要 .NET 10 SDK；目标框架 net10.0）
dotnet build Source/MyModeTest/MyModeTest.csproj
# 指定其他游戏目录
dotnet build Source/MyModeTest/MyModeTest.csproj -p:RimWorldPath="D:\Games\RimWorld"
# 也可用环境变量 RIMWORLD_PATH 提供游戏目录

# 静态验证（本项目的主要"测试"手段）
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all   # 旧链全量；Windows 中文控制台建议加 PYTHONIOENCODING=utf-8
python tools/validate_mod.py parts                        # 旧链单套件（套件名见 tools/validate_mod.py 底部 SUITES）
python tools/validate_new.py all                          # 新链全量（tools/validate/ 包，自动发现）
python tools/validate_new.py <链名>                       # 新链单套件

# 重新生成对比/链路文档（改 Def 后需重跑；这些 md 是生成物）
node tools/build_mmt_chains_compare.js      # → docs/mmt-vs-hsk-resource-chains.md
node tools/build_resource_chains.js         # → docs/hsk-resource-production-chains.md
python tools/build_research_mindmap.py      # → docs/mmt-research-mindmap.md（研究思维导图，含坐标/前置/研究点/解锁内容）
python tools/build_mmt_weapon_chains.py     # → docs/mmt-weapon-chains.md
```

其他工具脚本：

- `tools/build_mmt_weapons_from_hsk.py`：从本地 HSK/CE XML 批量生成 MMT 武器 Def，人工覆盖写入 `tools/data/mmt_weapon_mapping.json`，支持 `--write` 落盘与 `--check` 比对。
- `tools/build_mmt_bionics_from_hsk.py`：从 HSK 生成全部仿生链 Def（物品/Hediff/安装手术/研究树/原版部件补丁）并复制贴图；复用 `docs/temp/extract_hsk_bionics.py` 的解析索引，材料/研究/工作台经内置映射表转 MMT defName。仿生链 XML 均为**生成物**，要改就改脚本重跑。
- `tools/apply_research_layout.py`：一次性布局工具，按内置 mapping 重排研究的 `researchViewX/Y`。

## 测试策略

没有单元测试框架；`tools/validate_mod.py`（约 7100 行，**旧链专用，已冻结不再新增**）就是测试套件，逐字段校验 XML、贴图存在性、数值是否满足设计约束。套件按链组织（如 `glass` 链对应 `resources`/`building`/`recipes` 三个套件，另有 `concrete`、`wood`、`metal`、`weapons-*`、`orepowder`、`power`、`science` 等数十个，完整名单见文件底部 `SUITES` 字典）。

**新增资源链的校验一律写入 `tools/validate/<链名>.py`（每链一个小文件），不要改 `validate_mod.py`。** 链模块内通过 `from validate_mod import fail, require_files, parse_xml, ...` 复用旧文件的 helper，末尾注册 `SUITES = {"<链名>": validate_<链名>}`，由 `tools/validate_new.py` 自动发现聚合。若新链确实涉及旧链数据的交叉校验，才允许在旧文件中追加。

## C# 工程

- 单一工程 `Source/MyModeTest/MyModeTest.csproj`（`net10.0`，LangVersion latest，Nullable/ImplicitUsings 均 disable），解决方案文件为根目录 `MyModeTest.sln`。
- 编译输出直接落到 `1.7/Assemblies/`（csproj 的 `OutputPath`）。
- 通过 `RimWorldPath` 引用游戏目录下的 `Assembly-CSharp.dll`、`UnityStubs.dll` 与 `Waylith.dll`（1.7 的 Unity 抽象层，Material 等类型在此；均 `<Private>false</Private>` 不复制）；`ValidateRimWorldPath` Target 会在引用缺失时给出中文报错。
- Harmony 为 NuGet 包 `Lib.Harmony 2.3.6`，`ExcludeAssets="runtime"` 仅编译期引用；运行时依赖 Harmony Mod（已在 About.xml 声明）。
- 源文件：
  - `MyModeTestMod.cs`：Mod 入口，构造时打日志并 `Harmony.PatchAll()`。
  - `OreSiftPool.cs`：筛选配方的随机矿石池（`[StaticConstructorOnStartup]`，扫描全部 `isResourceRock` 矿脉按权重入池，含黑名单）。
  - `AutoProcessor.cs`：自动处理机 Comp（营养膏机模式，定时从相邻进料口消耗输入并产出；outputDef 为空时走矿石池随机）。
  - `ChunkDrill.cs`：手动采石机 Comp（产粗矿石，含工作量与采矿经验参数）。
  - `BionicsComps.cs`：仿生链 CONN 特殊效果重实现（狂战士芯片狂暴 gizmo、精神扭曲思想清除、缪斯娱乐供给、手电照明 mote、六种神经调制使用品效果），对照 CONN.dll 行为简化实现。
  - `Rechargeable.cs`：通用可充能 Comp（电量存档 + 充/放电接口），充能台与充能设备共用的电量载体。
  - `ChargingStation.cs`：充能台（内置容器持一件设备，通电回充；右键小人手动装卸：空台从穿戴/背包选设备装入，有设备确认后取回；DrawAt 画飘浮设备与充能光柱；装卸各一个 JobDriver）。
  - `ShieldPack.cs`：充能护盾背包（征召时开关 gizmo、开启全额吸收伤害按量耗电、耗尽/解除征召自动关、EMP 击穿；`Apparel_ChargeShieldPack` 重写 `DrawWornExtras` 画常驻护盾气泡；Harmony 补丁 Pawn.GetGizmos / Pawn.PreApplyDamage / set_Drafted）。
- C# 注释与日志均用中文。

## 代码与命名约定

- 所有 defName 加 `MMT_` 前缀（如 `MMT_Sand`、`MMT_GlassworksTable`）。
- Def 文件按 `MMT_<类别>_<链名>.xml` 命名（如 `MMT_Recipes_Parts.xml`、`MMT_Buildings_ElectricWood.xml`、`MMT_Research_Science.xml`）。
- label/description 用中文，直接写死在 Def 中。
- 贴图约定：多状态物品贴图按 `名称_a/_b/_c.png` 放在同名子目录（如 `MMT_Ore/MMT_CopperOre/MMT_CopperOre_a.png`），texPath 指向子目录路径；建筑贴图带朝向与遮罩（`_east/_north/_south` + `m` 后缀，如 `MMT_EConcreteMixer/MMT_EConcreteMixer_east_m.png`）。
- MMT 与 HSK 的 defName 映射维护在 `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 表中——新增与 HSK 对应的资源时同步更新该表。
- 提交信息使用中文 conventional commits（如 `feat: 添加零件链研究与配方`）。
- 整体代码审查只在用户主动要求时才执行。

## 安全与注意事项

- `Hardcore-SK/` 是只读参考仓库，不要改动其中任何文件。
- `docs/` 下多数文档为脚本生成物，手工编辑会在下次重跑时被覆盖；要改就改源 Def 或生成脚本。
- 不要读取或提交任何凭据/密钥类文件；本仓库无依赖安装步骤（Python/Node 脚本只用标准库）。
- Windows 环境：shell 为 Git Bash，路径用正斜杠；Python 中文输出乱码时使用 `PYTHONIOENCODING=utf-8`。
