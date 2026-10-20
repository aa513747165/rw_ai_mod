# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

1.游戏目录: D:\RimWorld v1.7.4979
2.开发mod文件目录: H:\rw
3.游戏存档目录: C:\Users\Administrator\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios
4.游戏DLC是全的可以使用依赖
5.HSK目录: H:\rw\Hardcore-SK
6.整体代码审查只在用户主动要求才执行
## 项目概述

`myModeTest`（简称 MMT）是 RimWorld 1.7 的独立资源生产 Mod，参考 Hardcore SK（HSK）的资源生产链实现，但**不依赖 HSK 的程序集或 Def**。核心是几条完整的资源加工链：玻璃、混凝土、木材、金属、石化、电子、电力（电力木/电力石化/电力电子）、高级合金、零件、煤、硫、贵金属等。每条链通常包含：资源 ThingDef → 配方 RecipeDef → 建筑 ThingDef（手工 + 电动）→ 研究 ResearchProjectDef。

## 常用命令

```bash
# 编译 C#（需要 .NET 10 SDK；net10.0）
dotnet build Source/MyModeTest/MyModeTest.csproj
# 指定其他游戏目录
dotnet build Source/MyModeTest/MyModeTest.csproj -p:RimWorldPath="D:\Games\RimWorld"

# 静态验证（主要"测试"手段，校验 XML/贴图/字段数值是否满足设计约束）
python tools/validate_mod.py all            # 全量
python tools/validate_mod.py parts          # 单套件；套件名见 SUITES（glass 链用 resources/building/recipes 等）
# 中文控制台乱码时：
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all

# 重新生成对比文档（改 Def 后需重跑，文档是生成物）
node tools/build_mmt_chains_compare.js      # → docs/mmt-vs-hsk-resource-chains.md
node tools/build_resource_chains.js         # → docs/hsk-resource-production-chains.md
```

没有单元测试框架；`tools/validate_mod.py` 就是测试套件，新增一条资源链时必须同步添加对应的 `validate_*` 函数和套件注册，并更新文件顶部的路径常量。

## 架构与约定

- **加载结构**：`LoadFolders.xml` 只挂载 `1.7/`；`About/About.xml` 声明 supportedVersions 1.7。C# 编译输出直接落到 `1.7/Assemblies/`（csproj 的 OutputPath）。
- **C# 引用**：csproj 通过 `RimWorldPath`（可用环境变量 `RIMWORLD_PATH` 覆盖）引用游戏目录下的 `Assembly-CSharp.dll` 和 `UnityStubs.dll`，`<Private>false</Private>` 不复制。目前 C# 仅有一个 Mod 入口类 `MyModeTestMod`，绝大部分逻辑是纯 XML Def。
- **命名约定**：所有 defName 加 `MMT_` 前缀（如 `MMT_Sand`、`MMT_GlassworksTable`）；Def 文件按 `MMT_<类别>_<链名>.xml` 命名（如 `MMT_Recipes_Parts.xml`、`MMT_Buildings_ElectricWood.xml`）。label/description 用中文，无本地化目录（直接写死在 Def 里）。
- **贴图**：`1.7/Textures/` 下为已获授权复制的 HSK 静态贴图。多状态物品贴图按 `名称_a/_b/_c.png` 放在同名子目录（如 `MMT_Ore/MMT_CopperOre/MMT_CopperOre_a.png`）；建筑贴图带朝向与遮罩（`_east/_north/_south` + `m` 后缀）。物品 png 若在子目录里，texPath 指向子目录路径。
- **HSK 参考**：`Hardcore-SK/` 是 HSK 仓库克隆（分支 `1.7-super-unstable`），只作参考，不要改动；`docs/` 内有从 HSK 提取的链路文档。MMT 与 HSK 的 defName 映射维护在 `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 表里——新增与 HSK 对应的资源时同步更新该表。
- **docs/temp/**：一次性提取脚本和中间 JSON，属临时产物，不要在正式文档里引用它。
- 提交信息使用中文 conventional commits（如 `feat: 添加零件链研究与配方`）。
