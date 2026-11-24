using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>扫描档位（mmt-dungeon-design.md §1.1）：XML 只读表，C# 不持有数值。</summary>
    public class ScanTier
    {
        /// <summary>档位编号（1-3，写入据点 parms.randomValue 带给 worker）。</summary>
        public int tier = 1;

        /// <summary>档位名称（如 浅层扫描）。</summary>
        public string label = "扫描";

        /// <summary>声纳模块消耗（× dungeonTierCostFactor 设置倍率）。</summary>
        public int sonarModules = 1;

        /// <summary>扫描天数系数（× 基础 scanDays × dungeonTierCostFactor）。</summary>
        public float scanDaysFactor = 1f;

        /// <summary>档位威胁系数（§1.2 威胁公式）。</summary>
        public float threatFactor = 1f;

        /// <summary>装备箱 LootRoll 档位（§1.3 稀有度地板）。</summary>
        public int lootTier = 1;

        /// <summary>装备品质地板/天花板（精良~稀有 ↔ Good~Excellent 等映射）。</summary>
        public QualityCategory gearQualityMin = QualityCategory.Good;

        public QualityCategory gearQualityMax = QualityCategory.Excellent;
    }

    /// <summary>扫描据点池条目（mmt-dungeon-design.md §0：清单一律 XML；其它 mod 可 XPath 追加自己的据点）。</summary>
    public class ScanSiteEntry
    {
        /// <summary>据点 SitePartDef。</summary>
        public SitePartDef sitePart;

        /// <summary>据点显示名。</summary>
        public string label = "据点";

        /// <summary>加权随机权重。</summary>
        public float weight = 1f;

        /// <summary>据点类型威胁系数（§1.2）。</summary>
        public float threatFactor = 1f;

        /// <summary>副本类据点：守卫经宿主派系 PawnGroupMaker 产出（Dungeon worker 程序化布局）。</summary>
        public bool factionGuards;

        /// <summary>兼容旧三类资源点：守卫用原版机械集群 SitePart（无 Royalty 时静默退化）。</summary>
        public bool mechGuardFallback;

        /// <summary>需要 Boss 巢穴设置开关（bossEncountersEnabled）开启才可入池（Boss 巢穴低概率扫描结果）。</summary>
        public bool requiresBossEncounters;
    }

    /// <summary>深空扫描仪参数（XML 可配置）。</summary>
    public class CompProperties_DeepSpaceScanner : CompProperties
    {
        /// <summary>基础扫描时长（游戏天，SpeedFactor 模块加速）。</summary>
        public float scanDays = 1f;

        /// <summary>每次扫描消耗的矿物声纳模块数（scanTiers 为空时的旧版回退值）。</summary>
        public int sonarModulesPerScan = 1;

        /// <summary>三档扫描成本/威胁/奖励表（§1.1；C# 只读）。</summary>
        public List<ScanTier> scanTiers;

        /// <summary>据点加权池（§0 清单 XML 化；Boss 巢穴低概率在池，受 requiresBossEncounters 开关门控）。</summary>
        public List<ScanSiteEntry> sitePool;

        public CompProperties_DeepSpaceScanner()
        {
            compClass = typeof(CompDeepSpaceScanner);
        }
    }

    /// <summary>
    /// 深空扫描仪 Comp（设计方案 §3 + mmt-dungeon-design.md §1）：
    /// gizmo 改 FloatMenu 三档选档（浅层/深层/全域，模块与天数成本 × dungeonTierCostFactor），
    /// 蓄电满 TryDrain 一次清空 + 消耗声纳模块 → 扫描中（CompTickInterval 推进，模块 SpeedFactor 加速）
    /// → 完成按 XML 据点池加权随机生成据点并发信。据点 30 天过期（TimeoutComp）；折跃进入/召回/回收全走传送台现有链路。
    /// </summary>
    public class CompDeepSpaceScanner : ThingComp
    {
        /// <summary>扫描剩余 tick（存档持久化；≤0 表示空闲）。</summary>
        private int scanTicksLeft = -1;

        /// <summary>本次扫描选定档位（存档持久化，FinishScan 结算用）。</summary>
        private int scanTier = 1;

        private CompBuildingBattery batteryComp;
        private CompModuleSlots slotsComp;
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        private CompProperties_DeepSpaceScanner Props
        {
            get { return (CompProperties_DeepSpaceScanner)props; }
        }

        // ---- 档位表/据点池（XML 优先，缺失回退内置默认） ----

        private static ThingDef scannerDef;
        private static bool scannerDefSearched;

        private static ThingDef ScannerDef
        {
            get
            {
                if (!scannerDefSearched)
                {
                    scannerDefSearched = true;
                    scannerDef = DefDatabase<ThingDef>.GetNamedSilentFail("MMT_DeepSpaceScanner");
                }
                return scannerDef;
            }
        }

        private static CompProperties_DeepSpaceScanner ScannerProps
        {
            get { return ScannerDef?.GetCompProperties<CompProperties_DeepSpaceScanner>(); }
        }

        /// <summary>档位配置（副本 worker 反向读取品质地板/LootRoll 档；缺 XML 时按 §1.1 内置三档）。</summary>
        public static ScanTier GetTierConfig(int tier)
        {
            List<ScanTier> tiers = ScannerProps?.scanTiers;
            if (tiers != null && tiers.Count > 0)
            {
                for (int i = 0; i < tiers.Count; i++)
                {
                    if (tiers[i] != null && tiers[i].tier == tier)
                        return tiers[i];
                }
                // 档位超出表：钳到表内最近档
                ScanTier best = null;
                for (int i = 0; i < tiers.Count; i++)
                {
                    if (tiers[i] != null && (best == null || Mathf.Abs(tiers[i].tier - tier) < Mathf.Abs(best.tier - tier)))
                        best = tiers[i];
                }
                if (best != null)
                    return best;
            }
            return FallbackTier(tier);
        }

        /// <summary>内置回退档位（§1.1 定稿数值）。</summary>
        private static ScanTier FallbackTier(int tier)
        {
            switch (Mathf.Clamp(tier, 1, 3))
            {
                case 1:
                    return new ScanTier { tier = 1, label = "浅层扫描", sonarModules = 1, scanDaysFactor = 1f, threatFactor = 1f, lootTier = 1, gearQualityMin = QualityCategory.Good, gearQualityMax = QualityCategory.Excellent };
                case 2:
                    return new ScanTier { tier = 2, label = "深层扫描", sonarModules = 2, scanDaysFactor = 1.5f, threatFactor = 1.6f, lootTier = 2, gearQualityMin = QualityCategory.Excellent, gearQualityMax = QualityCategory.Masterwork };
                default:
                    return new ScanTier { tier = 3, label = "全域扫描", sonarModules = 3, scanDaysFactor = 2f, threatFactor = 2.5f, lootTier = 3, gearQualityMin = QualityCategory.Masterwork, gearQualityMax = QualityCategory.Legendary };
            }
        }

        /// <summary>全部可选档位（XML 为空时给内置三档）。</summary>
        private List<ScanTier> Tiers
        {
            get
            {
                List<ScanTier> tiers = Props.scanTiers;
                if (tiers != null && tiers.Count > 0)
                    return tiers;
                return new List<ScanTier> { FallbackTier(1), FallbackTier(2), FallbackTier(3) };
            }
        }

        /// <summary>据点池（XML 为空时给旧版三类，行为与改版前一致）。</summary>
        private List<ScanSiteEntry> SitePool
        {
            get { return AllSitePool; }
        }

        /// <summary>据点池静态入口（公会委托直建新副本等非扫描仪路径复用）。</summary>
        public static List<ScanSiteEntry> AllSitePool
        {
            get
            {
                List<ScanSiteEntry> pool = ScannerProps?.sitePool;
                if (pool != null && pool.Count > 0)
                    return pool;
                return new List<ScanSiteEntry>
                {
                    new ScanSiteEntry { label = "稀有资源点", sitePart = MMT_ExplorationDefOf.MMT_SitePart_RareDeposit, weight = 45f, threatFactor = 0.5f },
                    new ScanSiteEntry { label = "轨道遗迹", sitePart = MMT_ExplorationDefOf.MMT_SitePart_OrbitalRuinCache, weight = 35f, threatFactor = 1f, mechGuardFallback = true },
                    new ScanSiteEntry { label = "机械据点", sitePart = MMT_ExplorationDefOf.MMT_SitePart_StrongholdCache, weight = 20f, threatFactor = 1.6f, mechGuardFallback = true },
                };
            }
        }

        /// <summary>档位实际声纳模块消耗（× 设置倍率，至少 1）。</summary>
        public static int EffectiveModules(ScanTier tier)
        {
            return Mathf.Max(1, Mathf.RoundToInt(tier.sonarModules * MMTSettings.DungeonTierCostFactor));
        }

        /// <summary>档位实际扫描天数（基础天数 × 档位系数 × 设置倍率）。</summary>
        private float EffectiveDays(ScanTier tier)
        {
            return Props.scanDays * tier.scanDaysFactor * MMTSettings.DungeonTierCostFactor;
        }

        /// <summary>是否扫描中。</summary>
        public bool IsScanning
        {
            get { return scanTicksLeft >= 0; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            batteryComp = parent.GetComp<CompBuildingBattery>();
            slotsComp = parent.GetComp<CompModuleSlots>();
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref scanTicksLeft, "deepSpaceScanTicksLeft", -1);
            Scribe_Values.Look(ref scanTier, "deepSpaceScanTier", 1);
        }

        /// <summary>扫描推进（1.7 Normal 建筑收不到 CompTickRare，必须 CompTickInterval）。</summary>
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (!IsScanning)
                return;
            // 节点流转：扫描中 -> 完成/继续（SpeedFactor 模块加速：每件 +0.1 → 进度 × (1+合计)）
            float speed = 1f + (slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f);
            scanTicksLeft -= Mathf.RoundToInt(delta * speed);
            if (scanTicksLeft < 0)
            {
                scanTicksLeft = -1;
                FinishScan();
            }
        }

        /// <summary>启动扫描的全局条件（与档位无关）：总开关/扫描中/通电/蓄电满。</summary>
        private string GlobalBlockReason()
        {
            if (!MMTSettings.ExplorationSitesEnabled)
                return "据点生成已在 Mod 设置中关闭";
            if (IsScanning)
                return "正在扫描中";
            if (flickComp != null && !flickComp.SwitchIsOn)
                return "设备已关闭";
            if (powerComp != null && !powerComp.PowerOn)
                return "未通电";
            if (batteryComp == null || !batteryComp.IsFull)
                return "蓄电未满（需充满 " + (batteryComp != null ? batteryComp.MaxChargeWd.ToString("F0") : "?") + " Wd）";
            return null;
        }

        /// <summary>档位级条件：声纳模块数（gizmo/菜单用缓存值，启动时强制刷新）。</summary>
        private string TierBlockReason(ScanTier tier, bool forceRefresh = false)
        {
            int need = EffectiveModules(tier);
            if (CountSonarModules(forceRefresh) < need)
                return "地图上缺少矿物声纳模块 ×" + need;
            return null;
        }

        // 模块计数缓存（gizmo 高频重绘；60 tick 内复用，启动扫描强制刷新）
        private int cachedSonarCount = -1;
        private int cachedSonarCountTick = -1;

        /// <summary>统计地图上未被禁止的声纳模块总数。</summary>
        private int CountSonarModules(bool forceRefresh = false)
        {
            int now = Find.TickManager.TicksGame;
            if (!forceRefresh && cachedSonarCount >= 0 && now - cachedSonarCountTick < 60)
                return cachedSonarCount;
            int total = 0;
            ThingDef sonarDef = MMT_ExplorationDefOf.MMT_MineralSonarModule;
            foreach (Thing t in parent.Map.listerThings.ThingsOfDef(sonarDef))
            {
                if (!t.IsForbidden(Faction.OfPlayer))
                    total += t.stackCount;
            }
            cachedSonarCount = total;
            cachedSonarCountTick = now;
            return total;
        }

        /// <summary>消耗地图上的声纳模块（逐堆 SplitOff）。</summary>
        private void ConsumeSonarModules(int count)
        {
            int remaining = count;
            foreach (Thing t in parent.Map.listerThings.ThingsOfDef(MMT_ExplorationDefOf.MMT_MineralSonarModule))
            {
                if (remaining <= 0)
                    break;
                if (t.IsForbidden(Faction.OfPlayer))
                    continue;
                int num = Mathf.Min(remaining, t.stackCount);
                t.SplitOff(num).Destroy(DestroyMode.Vanish);
                remaining -= num;
            }
        }

        /// <summary>启动扫描：清空蓄电 + 按档位消耗模块，进入倒计时。</summary>
        private void StartScan(ScanTier tier)
        {
            string reason = GlobalBlockReason() ?? TierBlockReason(tier, true);
            if (reason != null)
            {
                Messages.Message("无法启动深空扫描：" + reason, new LookTargets(parent), MessageTypeDefOf.RejectInput, false);
                return;
            }
            ConsumeSonarModules(EffectiveModules(tier));
            batteryComp.TryDrain(batteryComp.StoredChargeWd);
            float days = EffectiveDays(tier);
            scanTicksLeft = Mathf.RoundToInt(days * 60000f);
            scanTier = tier.tier;
            Messages.Message("深空扫描启动（" + tier.label + "）：预计 " + days.ToString("0.#") + " 天后完成（扫描仪蓄电已清空）",
                new LookTargets(parent), MessageTypeDefOf.TaskCompletion, false);
        }

        /// <summary>打开档位选择菜单（§1.1：玩家主动选择挑战强度）。</summary>
        private void OpenTierMenu(bool devInstant)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (ScanTier tier in Tiers)
            {
                ScanTier t = tier;
                int modules = EffectiveModules(t);
                float days = EffectiveDays(t);
                string text = t.label + "（声纳模块×" + modules + "，" + days.ToString("0.#") + " 天"
                    + "，威胁×" + t.threatFactor.ToString("0.#") + "）";
                string reason = devInstant ? null : TierBlockReason(t);
                FloatMenuOption opt = new FloatMenuOption(reason != null ? reason + "——" + t.label : text,
                    delegate { StartScan(t); });
                if (reason != null)
                    opt.Disabled = true;
                options.Add(opt);
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>扫描完成：XML 据点池加权随机 → 世界地图生成 Site（威胁 × 档位系数）→ 发信。</summary>
        private void FinishScan()
        {
            ScanTier tierCfg = GetTierConfig(scanTier);
            ScanSiteEntry entry = PickSiteEntry();
            Site site = entry == null ? null : TryGenerateSite(entry, tierCfg);
            if (site == null)
            {
                // 找不到合适世界格（极端情况）：退还一半蓄电，发提示信
                batteryComp?.TryDrain(-(batteryComp.MaxChargeWd * 0.5f));
                Find.LetterStack.ReceiveLetter("深空扫描无结果", "扫描完成，但未能在附近空域定位到可降落的目标。扫描仪回收了部分能量。",
                    LetterDefOf.NeutralEvent, new LookTargets(parent));
                return;
            }
            Find.LetterStack.ReceiveLetter("深空扫描完成：" + entry.label,
                "扫描仪（" + tierCfg.label + "）定位到一处" + entry.label + "。坐标已同步至世界地图（30 天后信号消失）。\n\n派出折跃小队前往探索，取回里面的远古材料。",
                LetterDefOf.PositiveEvent, new LookTargets(site));
            Messages.Message("深空扫描完成：" + entry.label + " 已出现在世界地图", new LookTargets(parent),
                MessageTypeDefOf.PositiveEvent, false);
            // 任务链「远古军械库」触发钩子（20% 概率；内部有开关/财富/冷却门控）
            GameComponent_RpgQuestChain.Current?.NotifyScanCompleted(parent.Map);
        }

        /// <summary>据点池加权随机（权重 ≤0 或开关未满足的条目跳过）。</summary>
        private ScanSiteEntry PickSiteEntry()
        {
            return PickRandomSiteEntry(false);
        }

        /// <summary>
        /// 据点池加权随机（静态入口，公会委托直建新副本复用）：
        /// requireGuards 时仅保留有守卫能力的条目（派系守卫或机械集群回退），
        /// 保证生成的据点确有可讨伐目标（委托完成判定依赖敌对威胁）。
        /// </summary>
        public static ScanSiteEntry PickRandomSiteEntry(bool requireGuards)
        {
            List<ScanSiteEntry> pool = AllSitePool;
            float total = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                ScanSiteEntry e = pool[i];
                if (!EntryAvailable(e))
                    continue;
                if (requireGuards && !e.factionGuards && !e.mechGuardFallback)
                    continue;
                total += e.weight;
            }
            if (total <= 0f)
                return null;
            float roll = Rand.Value * total;
            for (int i = 0; i < pool.Count; i++)
            {
                ScanSiteEntry e = pool[i];
                if (!EntryAvailable(e))
                    continue;
                if (requireGuards && !e.factionGuards && !e.mechGuardFallback)
                    continue;
                roll -= e.weight;
                if (roll <= 0f)
                    return e;
            }
            return null;
        }

        /// <summary>条目是否可入池：权重 > 0、sitePart 有效、设置开关满足（Boss 巢穴受 bossEncountersEnabled 门控）。</summary>
        private static bool EntryAvailable(ScanSiteEntry e)
        {
            if (e == null || e.sitePart == null || e.weight <= 0f)
                return false;
            if (e.requiresBossEncounters && !MMTSettings.BossEncountersEnabled)
                return false;
            return true;
        }

        /// <summary>生成据点：副本类走派系守卫路径，其余走旧版机械集群路径（§1.2 威胁 = 基础 × 类型系数 × 档位系数）。</summary>
        private Site TryGenerateSite(ScanSiteEntry entry, ScanTier tierCfg)
        {
            Map homeMap = parent.Map;
            float threat = DungeonSiteGen.BaseThreatPoints(homeMap) * entry.threatFactor * tierCfg.threatFactor;
            if (entry.factionGuards)
            {
                return DungeonSiteGen.GenerateDungeonSite(homeMap, entry.sitePart, tierCfg.tier,
                    entry.threatFactor, tierCfg.threatFactor, entry.label);
            }
            return DungeonSiteGen.GenerateStockpileSite(homeMap, entry.sitePart, tierCfg.tier,
                threat, entry.mechGuardFallback, entry.label);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            Command_Action scan = new Command_Action();
            scan.defaultLabel = IsScanning ? "扫描中…" : "启动深空扫描";
            scan.defaultDesc = "消耗全部蓄电与矿物声纳模块，扫描一段时间后在世界地图生成一处探索据点（30 天过期）。\n\n三档可选：浅层/深层/全域——档位越高，成本与威胁越大，奖励越好。";
            scan.icon = parent.def.uiIcon;
            string reason = GlobalBlockReason();
            if (reason != null)
            {
                scan.Disable(reason);
            }
            else
            {
                scan.action = delegate { OpenTierMenu(false); };
            }
            yield return scan;
            // 开发者模式：即刻扫描（跳过蓄电/声纳模块消耗与等待，直接出据点）
            if (Prefs.DevMode)
            {
                Command_Action devScan = new Command_Action();
                devScan.defaultLabel = "DEV: 即刻扫描";
                devScan.defaultDesc = IsScanning
                    ? "开发者模式：立即完成当前扫描并生成据点。"
                    : "开发者模式：跳过蓄电与声纳模块消耗，选择档位立即生成据点。";
                devScan.icon = parent.def.uiIcon;
                devScan.action = delegate
                {
                    if (IsScanning)
                    {
                        scanTicksLeft = -1;
                        FinishScan();
                    }
                    else
                    {
                        OpenDevTierMenu();
                    }
                };
                yield return devScan;
                // 开发者模式：直接生成 Boss 巢穴（锁定 III 档系数；任务链实装前的验收入口）
                Command_Action devBoss = new Command_Action();
                devBoss.defaultLabel = "DEV: 生成 Boss 巢穴";
                devBoss.defaultDesc = "开发者模式：在附近世界格生成一座远古守卫巢穴（III 档威胁系数 ×2.5、据点系数 ×2.0，30 天过期）。";
                devBoss.icon = parent.def.uiIcon;
                if (!MMTSettings.BossEncountersEnabled)
                {
                    devBoss.Disable("Boss 巢穴已在 Mod 设置中关闭");
                }
                else
                {
                    devBoss.action = DevSpawnBossLair;
                }
                yield return devBoss;
            }
        }

        /// <summary>DEV 档位菜单：不消耗资源直接完成一次扫描。</summary>
        private void OpenDevTierMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (ScanTier tier in Tiers)
            {
                ScanTier t = tier;
                options.Add(new FloatMenuOption(t.label + "（DEV 免费）", delegate
                {
                    scanTier = t.tier;
                    FinishScan();
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>DEV：生成 Boss 巢穴据点（III 档；rpg 文档 §3.1 据点系数 ×2.0）。</summary>
        private void DevSpawnBossLair()
        {
            SitePartDef bossLair = DefDatabase<SitePartDef>.GetNamedSilentFail("MMT_SitePart_BossLair");
            if (bossLair == null)
            {
                Messages.Message("缺少 MMT_SitePart_BossLair 定义", MessageTypeDefOf.RejectInput, false);
                return;
            }
            ScanTier tier3 = GetTierConfig(3);
            Site site = DungeonSiteGen.GenerateDungeonSite(parent.Map, bossLair, 3, 2.0f, tier3.threatFactor, "远古守卫巢穴");
            Messages.Message(site != null
                    ? "DEV: Boss 巢穴已生成（30 天过期）"
                    : "DEV: Boss 巢穴生成失败（无可用世界格）",
                site != null ? new LookTargets(site) : new LookTargets(parent),
                site != null ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput, false);
        }

        public override string CompInspectStringExtra()
        {
            if (IsScanning)
            {
                float speed = 1f + (slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f);
                return "扫描中（" + GetTierConfig(scanTier).label + "）: 剩余 " + (scanTicksLeft / 60000f).ToString("0.0") + " 天"
                    + (speed > 1f ? "（加速 ×" + speed.ToString("0.0") + "）" : "");
            }
            return null;
        }
    }

    /// <summary>
    /// 据点奖励参数（SitePartDef 的 modExtension）：
    /// rewards 为静态资源堆（全档位）；tieredRewards/gearRewards 为副本分档奖励池
    /// （mmt-dungeon-design.md §3.1，minTier/maxTier 门控，其它 mod 可 XPath 追加）。
    /// </summary>
    public class StockpileRewardExtension : DefModExtension
    {
        /// <summary>奖励物品（def + 数量区间；全档位出现，旧版行为）。</summary>
        public List<ThingDefCountRangeClass> rewards;

        /// <summary>分档奖励条目（minTier/maxTier 门控）。</summary>
        public List<DungeonRewardEntry> tieredRewards;

        /// <summary>装备箱条目（随机强化/附魔装备，品质地板随档位）。</summary>
        public List<DungeonGearEntry> gearRewards;
    }

    /// <summary>
    /// 探索据点奖励堆 Worker（设计方案 §3）：PostMapGenerate 在据点小地图上生成
    /// 沙袋围起的奖励堆（独占资源 + 随机太空零件）。一个类三个参数化 def 复用。
    /// 改版后兼容分档奖励：tieredRewards/gearRewards 按据点档位门控（§3.1）。
    /// </summary>
    public class SitePartWorker_MMT_Stockpile : SitePartWorker
    {
        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            StockpileRewardExtension ext = def.GetModExtension<StockpileRewardExtension>();
            if (ext == null)
                return;
            // 奖励堆落点：地图中心附近的可站立格（机械集群等 genstep 也在中心，向外偏一点找空位）
            IntVec3 center = CellFinderLoose.RandomCellWith(
                c => c.Standable(map) && !c.Fogged(map) && c.GetFirstItem(map) == null
                    && c.GetFirstBuilding(map) == null, map, 200);
            if (!center.IsValid)
                center = map.Center;
            // 生成奖励堆（每条目一堆，散布在落点周围 2 格内）
            if (ext.rewards != null)
            {
                foreach (ThingDefCountRangeClass reward in ext.rewards)
                {
                    int count = reward.countRange.RandomInRange;
                    if (count <= 0)
                        continue;
                    Thing stack = ThingMaker.MakeThing(reward.thingDef);
                    stack.stackCount = count;
                    IntVec3 cell = CellFinderLoose.RandomCellWith(
                        c => c.Standable(map) && c.GetFirstItem(map) == null && c.GetFirstBuilding(map) == null
                            && (c - center).LengthHorizontal <= 3f, map, 50);
                    GenSpawn.Spawn(stack, cell.IsValid ? cell : center, map);
                }
            }
            // 分档奖励池（§3.1；档位经 parms.randomValue 传入，非法值按 I 档处理）
            DungeonRewardGen.SpawnTiered(map, ext, DungeonRewardGen.TierOf(map, def), c =>
                CellFinderLoose.RandomCellWith(
                    x => x.Standable(map) && x.GetFirstItem(map) == null && x.GetFirstBuilding(map) == null
                        && (x - c).LengthHorizontal <= 3f, map, 50), center);
            // 沙袋围圈（纯视觉标记，中立无阵营）；1.7 沙袋 madeFromStuff，必须显式给默认材料否则红字刷屏
            ThingDef sandbags = ThingDefOf.Sandbags;
            if (sandbags != null)
            {
                ThingDef sandbagStuff = GenStuff.DefaultStuffFor(sandbags);
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, 2.9f, false))
                {
                    if (!cell.InBounds(map) || !cell.Standable(map) || cell.GetFirstBuilding(map) != null)
                        continue;
                    GenSpawn.Spawn(ThingMaker.MakeThing(sandbags, sandbagStuff), cell, map, Rot4.North);
                }
            }
        }
    }

    [DefOf]
    public static class MMT_ExplorationDefOf
    {
        /// <summary>矿物声纳模块（扫描消耗品）。</summary>
        public static ThingDef MMT_MineralSonarModule;
        /// <summary>稀有资源点奖励堆 SitePart。</summary>
        public static SitePartDef MMT_SitePart_RareDeposit;
        /// <summary>轨道遗迹奖励堆 SitePart。</summary>
        public static SitePartDef MMT_SitePart_OrbitalRuinCache;
        /// <summary>机械据点奖励堆 SitePart。</summary>
        public static SitePartDef MMT_SitePart_StrongholdCache;

        static MMT_ExplorationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ExplorationDefOf));
        }
    }
}
