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
python tools/build_mmt_classes_doc.py       # → docs/mmt-classes.md（职业手册：37 职业技能全表，数值取自映射表 + 生成产物 XML）
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
  - `FireElemental.cs`：小火元素死亡爆燃 worker（`DeathActionWorker_MMTFlameBurst`，爆炸羊式亡语）：死亡瞬间以尸体为中心 `GenExplosion.DoExplosion`（Flame 伤害 + 50% 点燃）；原版爆炸 worker 固定 Bomb 且 XML 不可配参数故自写；召唤物到期走 Destroy 不经死亡流程不引爆；`DangerousInMelee` 提示近战危险。
  - `Reforge.cs`：装备品质重铸（挂在装备强化台右键菜单，WeaponEnhance.cs 侧入口 + DefOf）：`ReforgeUtil`（品质梯一般→良好→极佳→大师、成功率 50%/30%/10%、合金轮替池/远古合金需求解析、菜单文案与结算）+ `JobDriver_ReforgeGear`（搬材料→掷点→CompQuality.SetQuality）；材料池/数量/成功率走强化台 comp 的 XML 配置（reforgeAlloyDefs 15 种合金锭轮替 ×10，极佳→大师远古合金 ×5），失败仅损失材料，仅一般/良好/极佳可重铸、大师封顶。
  - `CastBar.cs`：技能读条框架（第二步）：`MMTCastBar.Draw` 在施法者头顶画圆形读条（满圆按顺时针递减，复用原版 `MeshPool.pies` 扇形网格 + 固定 270° 旋转，颜色/半径/头顶偏移为可调常量，后续引导/蓄力通道可直接调用）；`DrawAt(Vector3, zOffset, fraction, color)` 为世界坐标通用核心（pawn/建筑皆可，假人施法演示在用），材质按颜色缓存；`Patch_MMTCastBar` 以 prefix 替换 `Stance_Warmup.StanceDraw` 中技能（Verb_CastAbility）的原版瞄准扇形（枪械不受影响），读条总时长按 `Verb.WarmupTime × AimingDelayFactor` 计算与建 stance 公式一致；瞬发技能（warmupTime=0）不产生读条 stance 自然无圆。
  - `SkillShooting.cs`：技能射击特效框架（第三步，纯原版素材不依赖 RoM）：`MMTShootUtil` 发射核心（`FireSingle` 单发直射 / `FireFunnels` 浮游齐射——施法者环形均布 N 个浮游单元，fleck 旋转对准目标 + instanceColor 染色，aimTicks 后同一 tick 齐射）；`MapComponent_MMTSkillFX` 延迟任务调度（原版反射自动注册，纯内存队列不进存档）；pawn 技能出口 `CompAbilityEffect_MMTSingleShot`/`CompAbilityEffect_MMTFunnelShot`；假人火球演示（`MMT_Proj_SkillFireball` 原版 LauncherShot 贴图弹 + `MMT_Fleck_FireballCharge` 悬浮火球 fleck，`MMT_Projectiles_SkillDemo.xml`/`MMT_Flecks_SkillFX.xml` 均为手写非生成物）。
  - `SkillBeam.cs`：技能持续射线框架（第四步）：`MMTBeamUtil`（`BeamDirection` 鼠标方向 / `BeamLength` 步进 0.5 格 raycast——出界回退、撞满填墙体停在其格内 / `FireBeam` 统一入口 / `DrawBeamPreview` 瞄准期半透明路径预览，与实射同一 raycast 所见即所得）；`Thing_MMTSkillBeam`（Attachment 类无碰撞 Thing，Def `MMT_SkillBeam`，Draw 自绘贴图拉伸光柱 + 出口光晕，宽度呼吸抖动 + 淡入淡出，按 damageIntervalTicks 对光柱经过格内 pawn/建筑跳伤默认 Burn 可点燃，参数运行时传入不进存档、读档自毁）；pawn 技能出口 `CompAbilityEffect_MMTBeam`（DrawEffectPreview 画路径预览）；第十四步引导占用（`MMTBeamParams.occupyCaster` 默认 true，仅 pawn 施法者生效）：`StartChannel` 挂引导作业 `MMT_ChannelSkill`（`JobDriver_MMTChannel` 原地不动面朝目标 + `ToilCompleteMode.Never`，Def 在 JobDefs/MMT_Jobs.xml）+ `Stance_MMTChannel` 忙碌姿态禁开火（注意 Stance_Busy 在 1.7 为抽象类需具体子类，子类须带无参 ctor 供存档 scribe）；射线每 tick `ChannelBroken` 校验——移动/下令/施放其它技能/被击晕倒地即断线并提示打断，自然结束走 `EndChannel`（结束作业 + 姿态 ticksLeft=1 自然过期），`BreakChannel` 供新技能起手打断旧引导；光柱贴图 `Things/Mote/MMT_SkillBeam`（纵轴=光束长度方向，白图代码染色，Pillow 占位待 AI 终稿，提示词见 `docs/mmt-skillfx-texture-prompts.md`）。
  - `SkillStrike.cs`：技能天降打击框架（第五步）：视觉/音效/基础爆炸全复用原版闪电事件（`RimWorld.WeatherEvent_LightningStrike` 经 `map.weatherManager.eventHandler.AddEvent` 立即触发：闪电实体+天空闪光+雷声+原版火焰爆炸，与原版闪电风暴一致），其上可叠加可配置自定义爆炸伤害层（`MMTStrikeParams.damage`>0，无二次视觉/音效）；`MMTStrikeUtil.FireStorm` 统一入口（单次定点直接触发；连发/范围/延迟则生成隐形调度器 `Thing_MMTSkyStorm`，Def `MMT_SkyStorm`，按间隔在目标半径内随机落点、优先无屋顶格，次数用尽自毁，参数运行时传入读档自毁）；pawn 技能出口 `CompAbilityEffect_MMTStrike`（继承 AoETarget 基类，radius>0 自动带范围预览圈）。
  - `SkillGroundFire.cs`：技能地面火焰框架（第六步：单体烈焰喷发 + 范围烈焰火海）：火焰留在原地持续燃烧不追踪目标；视觉为贴图火焰柱（`Thing_MMTGroundFlame`，Def `MMT_GroundFlame`，DrawAt 自绘双层四边形——底层火身 + 上移摆动火舌，随机朝向/相位 + 呼吸闪烁 + 淡入淡出，白图代码染色，贴图 `Things/Mote/MMT_FlamePillar` 已出 AI 终稿，提示词见 `docs/mmt-skillfx-texture-prompts.md` §二），效果为真实燃烧（按间隔对影响格 pawn/建筑跳 Burn，`FireUtility.TryAttachFire` 点燃小人 / `TryStartFireIn` 引燃格子走原版蔓延）；`MMTGroundFireUtil.FireGround` 统一入口（areaRadius≤0 单柱喷发；>0 半径内按密度抽格铺火海、自中心向外按距离延迟点燃，跳过满填墙格）；pawn 技能出口 `CompAbilityEffect_MMTGroundFire`（继承 AoETarget 基类）；校验套件 `tools/validate/skillfx.py`。
  - `SkillAura.cs`：技能光环框架（第七步：以目标为中心随其移动的持续光环，pawn/建筑均可挂）：盾类复刻原版临时护盾投影器（`Thing_MMTSkillAuraShield : Verse.MechShield`，Def `MMT_SkillAuraShield`，SetTarget 后力场泡跟随目标，`CompProjectileInterceptor` 走原版力场绘制/投射物拦截/命中点/EMP，本类只管 Activate/命中点覆写/时限与跟随失效自毁）；增益/减益类 `Thing_MMTSkillAuraBuff : AttachableThing`（Def `MMT_SkillAuraBuff`，AttachTo 跟随，画贴图光圈——圆盘 `Things/Mote/MMT_AuraDisc` 浮在小人身体中心高度（Pawn 层 +0.1，DrawPos 平滑跟随）呼吸脉冲 + GenDraw 地面描边，敌我配色，按脉冲间隔对范围内 pawn 施加/续期 Hediff，离开经原版 HediffComp_Disappears 消退；效果数值全在 HediffDef XML，如 `MMT_AuraBuff_Swift` MoveSpeed×1.5 / `MMT_AuraDebuff_Slow` ×0.5，见 `MMT_Hediffs_SkillAura.xml`）；`MMTAuraUtil.StartAura` 统一入口；pawn 技能出口 `CompAbilityEffect_MMTAura`（以自己为中心，预览圈走 AoETarget 基类）；1.7 注意：`MeshPool.plane10` 为 1×1 平面（缩放直接用格数），`AttachableThing.InspectStringAddon` 为抽象成员必须覆写。
  - `SkillBuffFX.cs`：增益/减益特效框架（第八步：施法后范围内有效目标头顶短暂浮现增益/减益小图标 + 身上飘少量粒子）：`Thing_MMTBuffIconFlash : AttachableThing`（Def `MMT_BuffIconFlash`，AttachTo 跟随目标，头顶画图标——贴图/染色/时长全走运行时参数、淡入淡出 + 缓缓上浮，按间隔抛身上小光点 fleck——投掷基类 `MMT_Fleck_BuffSpark`（贴图 `Things/Mote/MMT_BuffSpark`）经 `FleckMaker.GetDataStatic` + `instanceColor` 染色 + velocity 上飘）；`MMTBuffFXUtil.Apply/ApplyOne` 统一入口（pawn 经 `ApplyOrRefreshHediff` 施实际 Hediff，建筑只放特效不加 Hediff）；实际效果数值在 `MMT_Hediffs_SkillBuff.xml`（`MMT_Buff_Swift` MoveSpeed×1.5 / `MMT_Debuff_Slow` ×0.5，HediffComp_Disappears 长持续自动消退，区别于光环的秒级短续期 Hediff）；单选（`buffRequireTarget` 锁一名目标）与范围（radius 圈选，含带标记假人建筑）两种，敌我配色自动（增益绿/减益红）；pawn 技能出口 `CompAbilityEffect_MMTBuffFX`；1.7 注意：FleckMaker 在 RimWorld 命名空间，`FleckCreationData.velocity`/`instanceColor` 为可空字段直接赋值。
  - `SkillDisplace.cs`：位移技能框架（第九步：折跃/群体传送/冲锋/跳跃）：`MMTDisplaceUtil` 统一入口（Blink 瞬移=DeSpawn+GenSpawn 换格、MassTeleport 按相对偏移整体传送圈内有效目标、StartCharge/StartJump 走载体），`Thing_MMTDisplaceMover`（Def `MMT_DisplaceMover`：携带被位移目标——pawn 用 `pawn.Drawer.renderer.RenderPawnAt` 实绘、建筑用 Graphic.MatSingle 实绘，冲锋贴地平移+拖尾 fleck `MMT_Fleck_DashTrail`、建筑跳跃抛物线先升后降，到点生成落点，存档瞬间立即落地/异常摧毁原地放回防载体丢失），pawn 跳跃直接复用原版 `PawnFlyer.MakeFlyer`（Core 自带 `PawnFlyer` ThingDef，飞行曲线在 Core XML）；起终点闪光 fleck `MMT_Fleck_BlinkFlash`；pawn 技能出口 `CompAbilityEffect_MMTDisplace`（群体传送在 Apply 后自动 BeginTargeting 进入第二段落点瞄准）；1.7 注意：`Thing.DeSpawn()` 无参（DespawnMode 枚举已移除）、`BeginTargeting` 第三位置参数是 highlightAction 不是 caster。
  - `SkillSummon.cs`：召唤技能框架（第十步：雷球/追踪弹/动物/炮塔四召唤）：`MMTSummonUtil` 统一入口（FireOrb/FireHoming/SummonAnimal/SummonTurret）；`Thing_MMTSummonOrb`（Def `MMT_SummonOrb`，EtherealThingBase + useHitPoints=false = 无敌不可选中，从施法者匀速滑向目标格、按间隔对周围敌方脉冲伤害——敌我判定复用 `TargetsInRadius`，到点消失）、`Thing_MMTSummonHoming`（Def `MMT_SummonHoming`，同无敌，每 tick 朝目标当前位置修正航向，命中半径内造成伤害——有爆炸半径走 GenExplosion 否则直击，超时/目标失效消散；两者参数运行时传入、读档后首 tick 自毁）；动物召唤 = 生成玩家派系驯服动物 + `MMT_SummonAnimal` Hediff（`HediffComp_MMTSummonAnimal` 计时到期 Vanish 消散，运行时写入 ticksLeft），Harmony 补丁 Designator_Slaughter/Designator_ReleaseAnimalToWild.CanDesignateThing 禁止宰杀/放生；炮塔召唤 = 原版逻辑 `Building_TurretGun`（Def `MMT_SummonTurret` + 枪管 `MMT_Gun_SummonTurret` ParentName BaseWeaponTurret、投射物复用原版 Bullet_MiniTurret，不带电力/燃料/操作 comp = 无需电力弹药自动索敌），限时走原版 `CompLifespan`（公开字段 `age` 偏移 = lifespanTicks - 技能时长，实现运行时自定义时长）；pawn 技能出口 `CompAbilityEffect_MMTSkillSummon`（注意：职业系统已有 `CompAbilityEffect_MMTSummon`，命名冲突故加 Skill 中缀）。
  - `SkillDelayedStrike.cs`：延时打击框架（第十一步：预警圈→延迟→陨石）：`MMTDelayedStrikeUtil.FireDelayedStrike` 生成调度器 `Thing_MMTDelayedStrike`（Def `MMT_DelayedStrike`，自绘脉冲预警圈=最终爆炸半径，炼狱魔王火炮式先标记后落弹），延迟结束生成 `Thing_MMTMeteor`（Def `MMT_Meteor`，高空斜向坠落+火焰拖尾+迫近呼啸 MortarRound_PreImpact，落地 GenExplosion 原版爆炸可点燃；读档后立即引爆兜底）；pawn 技能出口 `CompAbilityEffect_MMTDelayedStrike`（预警圈预览复用 AoETarget 基类 radius）。
  - `SkillKnockback.cs`：近战击退框架（第十二步）：`MMTKnockbackUtil.Execute` 先结算钝器伤害（方向=施法者→目标），再把 pawn 沿该方向击飞 N 格（复用原版 `PawnFlyer.MakeFlyer`，落点逐格求可站立格，撞墙提前停并追加 wallSlamDamage，落地按原版飞行速度 12 格/s 估算时刻经 MapComponent_MMTSkillFX 补 StaggerFor 踉跄；建筑免疫击退）；pawn 技能出口 `CompAbilityEffect_MMTKnockback`。
  - `SkillChainLightning.cs`：链式弹跳框架（第十三步：连锁闪电）：`MMTChainLightningUtil.Fire` 建弹跳链（每次在上一个命中点 bounceRange 内找最近的未命中敌人，HostileTo 判定，pawn/建筑均可），`Thing_MMTChainArc`（Def `MMT_ChainArc`）按跳间节奏逐跳结算伤害（damageFalloff 乘算衰减）并自绘抖动折线电弧（光柱贴图细条 5 折点随机抖动、3 tick 换批、宽暗光晕+亮芯双层，Rand.PushState 定种子防闪瞎）+ 节点 ElectricalSpark 火星；pawn 技能出口 `CompAbilityEffect_MMTChainLightning`。
  - `SkillBreath.cs`：锥形喷吐框架（第十五步：龙息）：`MMTBreathUtil.FireBreath` 生成 `Thing_MMTBreathCone`（Def `MMT_BreathCone`，喷口跟随施法者、方向固定，原版 FireGlow 火星沿锥向抛撒 + 光柱贴图三层渐宽火舌，按间隔对锥内 pawn 跳伤**不分敌我**并概率点燃格子），`InCone` 距离+半角判定与 `DrawConePreview` 预览（两边线+末端弧）共用同一几何；pawn 技能出口 `CompAbilityEffect_MMTBreath`。
  - `SkillHeal.cs`：治疗特效框架（第十七步：范围回血）：`MMTHealUtil.Apply/ApplyOne` 按治疗量池从重伤到轻伤依次 Heal 非永久性 Hediff_Injury，特效=原版绿十字飘字 + 复用第八步 `Thing_MMTBuffIconFlash`（绿图标+绿粒子）；pawn 技能出口 `CompAbilityEffect_MMTSkillHeal`（注意：ClassAbilityEffects 已有 `CompAbilityEffect_MMTHeal`，命名冲突故加 Skill 中缀）。
  - `SkillWall.cs`：造墙/地形障碍框架（第十八步：石墙术）：`MMTWallUtil.RaiseWall` 在圈选范围可建格（无建筑/pawn/物品且可站立，`CanRaiseAt`）生成临时石墙 `MMT_MagicStoneWall`（满填 Impassable、挡光挡风托屋顶、不可燃、**无主派系**、不进建筑菜单，限时走 CompLifespan age 偏移到期崩解）；pawn 技能出口 `CompAbilityEffect_MMTWall`。
  - `AbilityTargeting.cs`：技能范围瞄准框架（第一步）：`MMTAoETargetPreview` 敌我判定/绘制核心（绿=友方红=敌方鼠标跟随圆形选择框 `DrawCircle`——贴图 `Things/Mote/MMT_TargetCircle` 细圆环+微填充、代码染色按半径缩放，贴图缺失退回原版方格 DrawRadiusRing； + 目标高亮，`TargetsInRadius` 含 pawn + CompAoETargetable 标记建筑）、`CompAbilityEffect_MMTAoETargetBase` 范围技能基类（经原版 DrawEffectPreview 钩子画圈，GiveHediffAoE/Heal/AoEDamage 已继承接入）、`CompSkillDemo` 演示技能 comp（不进任何建筑/角色 Def，经 `MMTSkillDemoGrant` 运行时注入小人：preview 类纯提示，singleShot/funnelShot/beam/strike/groundFire 类确认目标后进入读条，aura 类免瞄准直接以自身为中心展开，bufffx 类对目标施增益/减益，blink/charge/jump 类把施法者自己位移到落点（落点必须可站立），massTeleport 类第一段圈选后自动进入第二段落点瞄准再整体传送——读条圆画施法者正上方，CompTick 倒计时归零经 MMTShootUtil/MMTBeamUtil/MMTStrikeUtil/MMTGroundFireUtil/MMTAuraUtil/MMTBuffFXUtil/MMTDisplaceUtil 真实发射，beam 类瞄准时画鼠标方向光柱路径预览，锁定目标类（strikeRequireTarget / groundRequireTarget / buffRequireTarget）不许点地板、按 targetFriendly 限定敌我、效果落在目标读条后的当前位置（落雷劈身/烈焰喷发从脚下起火，火焰留原地不追踪），雷暴/火海走范围圈（随机落点/按密度铺格），一次一个读条重复触发直接替换）、沙袋人偶只留敌我靶子（CompAoETargetable；敌方假人 CompForceHostileFaction 强制远古敌对派系 + Designator_Build.Visible Dev 门控，技能演示不挂假人）、`MMTSkillDemoSetDef`（34 条演示唯一数据源 `1.7/Defs/Misc/MMT_SkillDemoSet.xml`，含第十一~十八步：delayedStrike 陨石 / knockback 击退 / chainLightning 连锁闪电 / breathCone 龙息 / 抛物线大火球=singleShot+ignoreLineOfSight 无视视线瞄准 / heal 范围回血 / wall 石墙术；FireDemo 起手先 `MMTBeamUtil.BreakChannel` 打断进行中的引导）+`MMTSkillDemoGrant` Debug 一键赋予（Dev「MMT 技能」菜单 ToolMapForPawns 把整套演示 comp 运行时注入任意小人；1.7 comp 不按存档持久化、读档后需重新赋予，Pawn.DrawAt/Tick 均走 comp PostDraw/CompTick 链已反射确认）。后续待补特效原型规划（新星环/陷阱/硬控/持续地面区域/驱散等 12 项）见手工维护文档 `docs/mmt-skillfx-backlog.md`。
- C# 注释与日志均用中文。

## 代码与命名约定

- 所有 defName 加 `MMT_` 前缀（如 `MMT_Sand`、`MMT_GlassworksTable`）。
- Def 文件按 `MMT_<类别>_<链名>.xml` 命名（如 `MMT_Recipes_Parts.xml`、`MMT_Buildings_ElectricWood.xml`、`MMT_Research_Science.xml`）。
- label/description 用中文，直接写死在 Def 中。
- 贴图约定：多状态物品贴图按 `名称_a/_b/_c.png` 放在同名子目录（如 `MMT_Ore/MMT_CopperOre/MMT_CopperOre_a.png`），texPath 指向子目录路径；建筑贴图带朝向与遮罩（`_east/_north/_south`，遮罩为方向字母后直接接 `m`，如 `MMT_EConcreteMixer/MMT_EConcreteMixer_eastm.png`；单图建筑为 `<名>.png` + `<名>_m.png`）。贴图设计细则见 `.claude/skills/rw-texture-design/SKILL.md`。
- 贴图工作流（占位图先行）：新增贴图时先用 Pillow 程序占位图落盘，让 Def 接线与 validate 套件先行通过、游戏内可用；同批在 `docs/mmt-<主题>-texture-prompts.md` 写好 AI 出图提示词文档（全局风格前缀 + 每条目英文提示词 + 负面词 + 尺寸/遮罩/后处理验收清单，格式参照 `docs/mmt-dungeon-texture-prompts.md`、`docs/mmt-fireboss-texture-prompts.md`）；AI 终稿回来后直接覆盖同名占位图（Def/代码零改动），并在提示词文档顶部标注已出图状态。
- MMT 与 HSK 的 defName 映射维护在 `tools/data/mmt_hsk_alias.json`——新增与 HSK 对应的资源/建筑/武器时同步更新该表（`build_mmt_chains_compare.js` 与三个手册脚本共用）。
- 提交信息使用中文 conventional commits（如 `feat: 添加零件链研究与配方`）。
- 整体代码审查只在用户主动要求时才执行。

## 安全与注意事项

- `Hardcore-SK/` 是只读参考仓库，不要改动其中任何文件。
- `docs/` 下多数文档为脚本生成物，手工编辑会在下次重跑时被覆盖；要改就改源 Def 或生成脚本。
- 不要读取或提交任何凭据/密钥类文件；本仓库无依赖安装步骤（Python/Node 脚本只用标准库）。
- Windows 环境：shell 为 Git Bash，路径用正斜杠；Python 中文输出乱码时使用 `PYTHONIOENCODING=utf-8`。
