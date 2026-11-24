# AGENTS.md

本文件面向 AI 编码代理，介绍本仓库的结构、构建方式与开发约定。阅读前假设你对本项目一无所知。

## 项目概述

`myModeTest`（简称 MMT）是 RimWorld 1.7 的独立资源生产 Mod，参考 Hardcore SK（HSK）的资源生产链实现，但**不依赖 HSK 的程序集或 Def**。核心内容是若干条完整的资源加工链：玻璃、混凝土、木材、金属、石化、电子、电力（电力木/电力石化/电力电子）、陶瓷、农业、矿粉（磨矿/筛矿）、高级合金、零件、煤、硫、贵金属、医药、武器（古代/常规/未来/炮塔/NPC）、核能与物质、仿生/义肢（含 CyberneticOrganism_SK 的 CONN 系列，不含 Dubs 卫生仿生）等。每条链通常包含：资源 ThingDef → 配方 RecipeDef → 建筑 ThingDef（手工 + 电动，部分含自动版）→ 研究 ResearchProjectDef。

绝大部分内容以纯 XML Def 实现，C# 仅承担少量无法 XML 表达的运行时逻辑（Harmony 补丁、自定义 Building Comp、矿石随机池等）。

## 关键路径与环境

- 游戏目录：`D:\RimWorld v1.7.4979`（RimWorld 1.7，DLC 全，可使用 DLC 依赖）
- 开发目录：`D:\project\rw`（即本仓库根）
- HSK 参考克隆：`Hardcore-SK/`（分支 `1.7-super-unstable`），**只读参考，不要改动**
- RoM 参考：`D:\steamcmd\steamapps\workshop\content\294100\3823345404`（A RimWorld of Magic，职业系统设计参照与贴图/音效资源库，已获授权；**只读，不改动；不依赖其程序集/Def**）
- Isekai 参考：`D:\steamcmd\steamapps\workshop\content\294100\3657580708`（Isekai RPG Leveling，天赋树/任务系统设计参照与天赋树 UI 贴图来源，贴图已获授权直接复制；**只读，不改动；不复制其代码**）
- 中文翻译参考：`Hardcore-SK/1.6`
- 所有 HSK 资源已获授权使用
- 如果需要生成AI贴图先让AI详情描述贴图所需元素再生成尽量立体符合风格

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
python tools/build_mmt_buildings_doc.py     # → docs/mmt-buildings.md（建筑手册：造价/功耗/配方 + HSK 未实现生产建筑）
python tools/build_mmt_weapons_doc.py       # → docs/mmt-weapons.md（武器手册：伤害/射速/冷却/DPS + HSK 未实现武器）
python tools/build_mmt_items_doc.py         # → docs/mmt-items.md（非武器物品手册 + HSK 未实现资源/材料）
```

后三个手册脚本共用 `tools/docgen_common.py`（Def 索引、ParentName 继承链合并、HSK 中文名读取、造价/配方格式化）。

其他工具脚本：

- `tools/build_mmt_weapons_from_hsk.py`：从本地 HSK/CE XML 批量生成 MMT 武器 Def，人工覆盖写入 `tools/data/mmt_weapon_mapping.json`，支持 `--write` 落盘与 `--check` 比对。
- `tools/build_mmt_bionics_from_hsk.py`：从 HSK 生成全部仿生链 Def（物品/Hediff/安装手术/研究树/原版部件补丁）并复制贴图；复用 `docs/temp/extract_hsk_bionics.py` 的解析索引，材料/研究/工作台经内置映射表转 MMT defName。仿生链 XML 均为**生成物**，要改就改脚本重跑。
- `tools/build_mmt_rom_classes.py`：RoM 职业系统生成器（`docs/mmt-class-design.md`）：按 `tools/data/mmt_rom_class_mapping.json`（唯一人工维护点，classes 段含 `family` 家族分组字段）产出 37 职业技能 AbilityDef（`MMT_Abilities_RoM_<职业>.xml`）、职业×3 阶 + 技能 buff/光环 Hediff（`MMT_Hediffs_RoMClasses.xml`；**v2 起阶位 Hediff 不再携带 abilities，技能授予移交天赋树**）、投射物、召唤建筑、气象事件条件，并复制 RoM 图标/投射物贴图；v2.1 起复刻战斗表现：`MMT_Sounds_RoM.xml`（RoM soundCast 照抄，TM_* 音效更名 MMT_* + wav 复制到 `1.7/Sounds/RoM/`）与 `MMT_Flecks_RoM.xml`（RoM Mote 复刻为 FleckDef + 贴图复制到 `1.7/Textures/Things/Fleck/RoM/`，按职业主题表 CLASS_FLECK 经 `CompAbilityEffect_MMTFleckFX` 接施法者/目标点特效）；`--check` 核对 RoM 技能全集 = mapped + passives + deferred + ignored。产出 XML 均为**生成物**，要改就改映射表/脚本重跑。
- `tools/build_mmt_talent_trees.py`：天赋树生成器（`docs/mmt-class-design.md` §9）：按 `tools/data/mmt_talent_tree_mapping.json`（唯一人工维护点）产出通用树 + 10 棵职业家族树 TalentDef（`1.7/Defs/TalentDefs/`）与节点被动 Hediff（`MMT_Hediffs_Talents.xml`），并复制 Isekai 授权贴图到 `1.7/Textures/UI/TalentTree/`；`--check` 幂等校验。产出均为**生成物**。
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
  - `LootRoll.cs`：装备随机强化/附魔掷点（RPG 支柱一，`docs/mmt-rpg-design.md` §1）：按威胁点四档掷初始强化等级与附魔词条，直接写入既有强化 comp 的 level/enchants（零新增存档字段）；词条池照抄模块 XML 档位注释分组（`tools/validate/loot_roll.py` 同步校验）；袭击者武器/防具生成钩子（`TryGenerateWeaponFor`/`GenerateStartingApparelFor` postfix，仅敌对派系）。
  - `Rechargeable.cs`：通用可充能 Comp（电量存档 + 充/放电接口），充能台与充能设备共用的电量载体。
  - `ChargingStation.cs`：充能台（内置容器持一件设备，通电回充；右键小人手动装卸：空台从穿戴/背包选设备装入，有设备确认后取回；DrawAt 画飘浮设备与充能光柱；装卸各一个 JobDriver）。
  - `ShieldPack.cs`：充能护盾腰带（征召时开关 gizmo、开启全额吸收伤害按量耗电、耗尽/解除征召自动关、EMP 击穿；`Apparel_ChargeShieldPack` 重写 `DrawWornExtras` 画常驻护盾气泡，用 MMT 专属贴图 `Things/Item/MMT/Shield/MMT_ShieldBubbleTech`；Harmony 补丁 Pawn.GetGizmos / Pawn.PreApplyDamage / set_Drafted）。
  - `MechStandby.cs`：哨戒待机模式（Biotech 机械体新工作模式 `MMT_Sentinel`：`JobGiver_MechStandby` 由 ThinkTree patch 挂入 Mechanoid 树，闲置时复用原版 SelfShutdown 作业到待机点关机睡眠省性能；`MechStandbyManager` 由 EnemyRaidTracker 事件驱动在敌意出现时唤醒；`MapComponent_MechStandby` 持久化每台机械体的标定待机点；GetGizmos 补丁提供"设定/清除待机点、立即唤醒"gizmo）。
  - `BreachCharge.cs`：爆墙炸弹系统（财富门槛配发：敌对袭击者按财富概率额外携带爆墙炸弹——背包物品 + 携带者 Hediff 授予投掷动词；三条补丁：GeneratePawn 配发 / JobGiver_AIFightEnemy 主动炸建筑 / FindVerbToUseForBreaching 破墙优先用弹；自定义 JobDriver_ThrowBreachCharge 投掷，炸弹为消耗品、耗尽摘 Hediff）。
  - `EliteBreacher.cs`：精英爆破手（`docs/mmt-endgame-design.md` §5）：财富 50 万+ 袭击注入专职爆破单位（`MMT_EliteBreacher`，名额 1→3 随财富爬升、按 combatPower 从袭击点数预扣；挂 1.7 原版袭击生成出口 `PawnGroupMakerUtility.GeneratePawns(PawnGroupMakerParms, bool)`，raidStrategy 非空即袭击）；精英在 BreachCharge 主动炸建筑补丁中走 100% 主动 + 墙体优先分支；开关 `eliteBreacherEnabled`。
  - `AutoFarm.cs`：自动种植机（5x5 无人值守水培，`Building_AutoFarm`：gizmo 全局选种（sowTags 含 "Ground" 的全部植物），25 格虚拟种植格逐 tick 生长（基础肥力 300%，燃料舱有肥料时 400%），成熟自动收获到交互格并重播；`DrawAt` 按 `visualSizeRange` 缩放逐格绘制植物图形）。
  - `PawnProgress.cs`：历练框架（RPG 支柱二，`docs/mmt-class-design.md` §9）：`CompPawnProgress`（XP/等级 1-60/天赋点/已解锁天赋节点/职业记录，XPath 挂 Human）、每级 +1 天赋点、XP 三钩子（PostApplyDamage 伤害 0.1×/Kill 击杀分档/SkillRecord.Learn 0.2×，全源钩子见 TalentXPPatches.cs）、20 档历练 Hediff（20 级后成长走天赋树）、旧 19 perk（含品质暴击 MMT_Perk_Masterwork + QualityUtility postfix，v2 已并入通用天赋树）、ITab_Pawn_Progress（含天赋树入口）、死亡骑士近战吸血。
  - `TalentTreeDef.cs`：天赋树数据与运行时（`docs/mmt-class-design.md` §9）：`MMT_TalentTreeDef`（nodes + "A~B" 连线建邻接表）、`TalentTreeUtil`（解锁门禁 CanUnlock/效果施加 Unlock/Dijkstra 链式解锁/就职联动 OnClassAttuned/旧档迁移 Migrate）、Debug 加点/洗点/解锁全家族。
  - `TalentTreeWindow.cs`：天赋树窗口（本 Mod 首个自定义 Window）：左侧树页签/中间画布（右键平移、滚轮缩放 0.35-2.0）/右侧详情/底部点数；连线 Widgets.DrawLine + Cohen–Sutherland 裁剪；双击链式解锁；节点形状/图标用 Isekai 授权贴图（UI/TalentTree/）。
  - `TalentXPPatches.cs`：全源历练经验钩子（工作完成分档/品质产出/社交/贸易/招募/捕获/击杀 combatPower 分档加成/灵能释放/熵累积/集中消耗/冥想；高频源按 pawn 冷却节流；数值为 Isekai ~1/4，经 AddXp 统一乘 xpMultiplier）。
  - `PawnClass.cs`：职业就职/升阶（`docs/mmt-class-design.md` §2.5/§9）：37 职业表、导师台 CompClassMentor（FloatMenu 法师/斗士两组，**v2 按已解锁职业家族过滤**，就职耗凭证、升阶耗能量结晶，门槛历练 5/20/35 级）、JobDriver_MMTClassAttune、无灵能链接者就职自动觉醒（PawnUtility.ChangePsylinkLevel）、就职联动家族树 Start 解锁、Dev 发凭证 DebugAction。
  - `ClassAbilityEffects.cs`：职业技能通用效果库（约 12 个参数化 CompAbilityEffect_MMT_* 桶：GiveHediffAoE/AoEDamage(含脉冲区)/Heal/Summon(含限时)/DashStrike/MentalStateAoE/SpawnEffect(天气/条件/物品/灭火)/MultiShot/ToggleAura(熵压光环)/ResurrectLite + 个案：自伤降熵/抚平心绪/拉取/群体传送/移形换位/战斗药剂/尸爆/改写命运(死亡回溯)/枪托猛击 + 丰饶之雨 GameCondition + MMTFleckFX 施法特效桶）。
  - `SummonTurret.cs`：召唤建筑 comp（炮塔/图腾/地雷三模式 attack/heal/mine，原版 Lifespan 限时）。
  - `GuildQuest.cs`：公会狩猎委托（`docs/mmt-class-design.md` §9.4）：`GameComponent_GuildQuests` 定时器按间隔生成委托（F-SSS 段位按历练+财富加权）、F-D 本地图刷怪/B-A 世界站点群猎/S-SSS 站点 Boss（`SitePartWorker_MMT_GuildHunt` 地图生成时刷怪，站点记录存 GameComponent）、完成检测由 GameComponent 250 tick 驱动、奖励 XP 均分 + 白银空投（B+ 附能量结晶、A+ 附职业凭证）、QuestPart_Delay 超时判负、站点世界图标为 Isekai 授权贴图。世界委托据点联动三级目标来源：既有未清剿据点（`TryFindLinkedSite`，清剿即完成，双份收益）→ 按 `guildQuestDungeonChance` 直建新副本（`DungeonSiteGen.GenerateRandomSite`，与扫描仪同池同公式、仅抽有守卫条目）→ 自建狩猎场；联动完成判定走 `sawHostiles`（见过敌对威胁后全清/站点消失分成败）。
  - `Exploration.cs`：深空扫描仪与探索据点（三档扫描 FloatMenu 选档，档位表/据点池全在扫描仪 comp props XML——`scanTiers`/`sitePool`；威胁 = clamp(财富/100, 800, 5000) × 类型系数 × 档位系数；`StockpileRewardExtension` 含静态 rewards + 分档 tieredRewards/gearRewards；档位经 SitePartParams.randomValue 传给 worker；Boss 巢穴低概率在池，requiresBossEncounters 门控；扫描完成钩子触发任务链）。
  - `RpgQuestChain.cs`：任务链「远古军械库」（`docs/mmt-rpg-design.md` §3.3）：QuestScriptDef 壳（autoAccept、rootSelectionWeight=0）+ `QuestPart_MMTAncientArmory` 阶段机（试炼场标注 → 碎片×3 收集计数 → 生成 Boss 巢穴 → 清剿判定复用 sawHostiles 模式 → 空投传说装备/职业凭证/远古数据核心）；`GameComponent_RpgQuestChain` 250 tick 节流驱动（1.7 QuestPart 无 tick），触发 = 扫描完成 20% / 财富 15 万+ 每日 10%，双开关门控 + 30 天冷却可重复。
  - `Dungeon.cs`：副本/据点生成（`docs/mmt-dungeon-design.md`）：`SitePartWorker_MMT_Dungeon` 程序化布局（外墙围合+入口/奖励库房/守卫哨位掩体炮塔/照明/Boss 巢位）、宿主派系选取（原版 `TryGetRandomFactionForCombatPawnGroup` + 派系黑名单）、守卫经 PawnGroupMaker 产出挂 LordJob_SitePawns、炮塔全库扫描+同包加权+预算过滤（缓存一次）、墙体材质从守卫装备采样、炮塔供电走原版 GenStep_Power、装备箱生成（DungeonGearGen，品质地板+LootRoll 掷点）；Boss 池化生成（TrySpawnBoss：预算=威胁点×`bossBudgetFraction`，经 BossPool 按战力贴合预算抽选 `bossCountRange` 上限只数，类人挂宿主派系入守卫 lord、机械/虫/实体挂各自派系、动物永久狂暴，抽不出静默退化为大型据点）。
  - `BossPool.cs`：Boss 池（`docs/mmt-dungeon-design.md` §4.7）：候选 = XML 白名单（各 SitePartDef `bossPawnKindDefNames`，缺失静默跳过）∪ 全库自动扫描（isFighter 且 combatPower ≥ `AutoScanMinCombatPower` 150，原版/Mod 通吃，缓存一次）；`PickBosses` 按预算逐只抽选（单只 ≤ 剩余预算、权重偏向战力贴近预算、种类不重复，低财富最弱候选 ×1.5 容忍兜底）；`ResolveBossFaction` 派系解析（类人→宿主派系/机械→机械族/虫→虫族/异常实体→实体派系）；后继新增 MMT Boss 只需 XML 白名单登记。Boss 召唤器与本池共用候选源。
  - `BossSummoner.cs`：Boss 召唤器（dev 模式专用测试工具）：CompBossSummoner gizmo 列出全部 Boss 级单位（候选复用 BossPool：XML 白名单 ∪ 全库自动扫描，原版/Mod 通吃，菜单标注战力与来源包，候选缓存只扫一次）→ 目标选格 → 敌对生成（派系走 BossPool.ResolveBossFaction，有派系挂 LordJob_AssaultColony 立即进攻，无派系永久狂暴兜底）；`Designator_Build.Visible` postfix 使建筑仅 Dev 模式可建。
  - `DungeonBlueprint.cs`：玩家副本蓝图（`docs/mmt-dungeon-design.md` §4.6）：建筑师菜单「副本蓝图」设计器框选捕捉已建成建筑（排除 Frame/未建成蓝图，单边 ≤40）→ 命名存 MMTSettings 全局列表（跨存档）；每张蓝图带据点类型开关（enabledSiteParts，只有勾选的 SitePartDef 类型才能随机选用）+ 档位开关（enabledTiers I/II/III，默认全档，生成时类型+档位双重过滤），管理区在设置面板（缩略图按类别着色逐格绘制 + 开关 + 删除）；`SitePartWorker_MMT_Dungeon` 生成时 `DungeonBlueprintStore.PickFor(sitePart, tier)` 抽图，有则按偏移盖蓝图（炮塔挂派系/含需电走 GenStep_Power/奖励散布内部），无则程序化布局兜底。
  - `TradeOrder.cs`：贸易商队订单（早期贸易）：`Patch_LordMaker_MakeNewLord` 钩贸易商队到访（LordJob_TradeWithColony）随机成员出单（货物池按殖民地科技水平过滤、排除装备/易腐品；订单基础价值 = 殖民地财富 × 1%，钳 200-2000；报酬 = 货物价值 × 150%-250% 白银），`ChoiceLetter_TradeOrder` 接受/拒绝；`GameComponent_MMTTradeOrders` 单订单槽状态机（待接取→已接取→据点等待，250 tick 节流）——接取后商队离图在殖民地 2-8 格生成 `WorldObject_TradeDepot`（纯世界交互无地图，远行队右键交付，`CaravanArrivalAction_TradeDepot` 抵达结算扣货付银）；家有现货可在商队离图前直接交付（信件第三按钮 + 提供人 gizmo，`DeliverFromColony` 从殖民地库存扣货、白银落在商人旁）；超时 15 天或提供人死亡/销毁则据点与订单一并消失；开关 `tradeOrdersEnabled`。
- C# 注释与日志均用中文。

## 代码与命名约定

- 所有 defName 加 `MMT_` 前缀（如 `MMT_Sand`、`MMT_GlassworksTable`）。
- Def 文件按 `MMT_<类别>_<链名>.xml` 命名（如 `MMT_Recipes_Parts.xml`、`MMT_Buildings_ElectricWood.xml`、`MMT_Research_Science.xml`）。
- label/description 用中文，直接写死在 Def 中。
- 贴图约定：多状态物品贴图按 `名称_a/_b/_c.png` 放在同名子目录（如 `MMT_Ore/MMT_CopperOre/MMT_CopperOre_a.png`），texPath 指向子目录路径；建筑贴图带朝向与遮罩（`_east/_north/_south`，遮罩为方向字母后直接接 `m`，如 `MMT_EConcreteMixer/MMT_EConcreteMixer_eastm.png`；单图建筑为 `<名>.png` + `<名>_m.png`）。贴图设计细则见 `.claude/skills/rw-texture-design/SKILL.md`。
- MMT 与 HSK 的 defName 映射维护在 `tools/data/mmt_hsk_alias.json`——新增与 HSK 对应的资源/建筑/武器时同步更新该表（`build_mmt_chains_compare.js` 与三个手册脚本共用）。
- 提交信息使用中文 conventional commits（如 `feat: 添加零件链研究与配方`）。
- 整体代码审查只在用户主动要求时才执行。

## 安全与注意事项

- `Hardcore-SK/` 是只读参考仓库，不要改动其中任何文件。
- `docs/` 下多数文档为脚本生成物，手工编辑会在下次重跑时被覆盖；要改就改源 Def 或生成脚本。
- 不要读取或提交任何凭据/密钥类文件；本仓库无依赖安装步骤（Python/Node 脚本只用标准库）。
- Windows 环境：shell 为 Git Bash，路径用正斜杠；Python 中文输出乱码时使用 `PYTHONIOENCODING=utf-8`。
