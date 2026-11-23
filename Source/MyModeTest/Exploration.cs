using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>深空扫描仪参数（XML 可配置）。</summary>
    public class CompProperties_DeepSpaceScanner : CompProperties
    {
        /// <summary>基础扫描时长（游戏天，SpeedFactor 模块加速）。</summary>
        public float scanDays = 1f;

        /// <summary>每次扫描消耗的矿物声纳模块数。</summary>
        public int sonarModulesPerScan = 1;

        public CompProperties_DeepSpaceScanner()
        {
            compClass = typeof(CompDeepSpaceScanner);
        }
    }

    /// <summary>
    /// 深空扫描仪 Comp（设计方案 §3）：gizmo 启动扫描（蓄电满 TryDrain 一次清空 + 消耗声纳模块）
    /// → 扫描中（CompTickInterval 推进，模块 SpeedFactor 加速）→ 完成在世界地图生成据点并发信。
    /// 据点 30 天过期（TimeoutComp）；折跃进入/召回/回收全走传送台现有链路，零改动。
    /// </summary>
    public class CompDeepSpaceScanner : ThingComp
    {
        /// <summary>扫描剩余 tick（存档持久化；≤0 表示空闲）。</summary>
        private int scanTicksLeft = -1;

        private CompBuildingBattery batteryComp;
        private CompModuleSlots slotsComp;
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        private CompProperties_DeepSpaceScanner Props
        {
            get { return (CompProperties_DeepSpaceScanner)props; }
        }

        /// <summary>原版机械集群 SitePart（Royalty 内容，惰性解析；未装 Royalty 时为 null，据点退化为无守卫）。</summary>
        private static SitePartDef mechClusterPart;
        private static bool mechClusterSearched;
        private static SitePartDef MechClusterPart
        {
            get
            {
                if (!mechClusterSearched)
                {
                    mechClusterSearched = true;
                    mechClusterPart = DefDatabase<SitePartDef>.GetNamedSilentFail("MechCluster");
                }
                return mechClusterPart;
            }
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

        /// <summary>启动扫描条件：总开关开 + 未在扫描 + 开关通电 + 蓄电满 + 地图上有声纳模块。</summary>
        private string CanStartScan()
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
            if (CountSonarModules() < Props.sonarModulesPerScan)
                return "地图上缺少矿物声纳模块 ×" + Props.sonarModulesPerScan;
            return null;
        }

        /// <summary>统计地图上未被禁止的声纳模块总数。</summary>
        private int CountSonarModules()
        {
            int total = 0;
            ThingDef sonarDef = MMT_ExplorationDefOf.MMT_MineralSonarModule;
            foreach (Thing t in parent.Map.listerThings.ThingsOfDef(sonarDef))
            {
                if (!t.IsForbidden(Faction.OfPlayer))
                    total += t.stackCount;
            }
            return total;
        }

        /// <summary>消耗地图上的声纳模块（逐堆 SplitOff）。</summary>
        private void ConsumeSonarModules()
        {
            int remaining = Props.sonarModulesPerScan;
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

        /// <summary>启动扫描：清空蓄电 + 消耗模块，进入倒计时。</summary>
        private void StartScan()
        {
            string reason = CanStartScan();
            if (reason != null)
            {
                Messages.Message("无法启动深空扫描：" + reason, new LookTargets(parent), MessageTypeDefOf.RejectInput, false);
                return;
            }
            ConsumeSonarModules();
            batteryComp.TryDrain(batteryComp.StoredChargeWd);
            scanTicksLeft = Mathf.RoundToInt(Props.scanDays * 60000f);
            Messages.Message("深空扫描启动：预计 " + Props.scanDays.ToString("0.#") + " 天后完成（扫描仪蓄电已清空）",
                new LookTargets(parent), MessageTypeDefOf.TaskCompletion, false);
        }

        /// <summary>据点类型（标签 + 奖励 SitePart + 是否带机械守卫 + 威胁点系数）。</summary>
        private class SiteType
        {
            public string label;
            public SitePartDef rewardPart;
            public bool mechGuard;
            public float threatFactor;
        }

        /// <summary>三类据点的加权表（启动时惰性构建，运行期只读）：稀有资源点 45% / 轨道遗迹 35% / 机械据点 20%。</summary>
        private static SiteType[] siteTypes;

        private static SiteType[] SiteTypes
        {
            get
            {
                return siteTypes ?? (siteTypes = new[]
                {
                    new SiteType { label = "稀有资源点", rewardPart = MMT_ExplorationDefOf.MMT_SitePart_RareDeposit, mechGuard = false, threatFactor = 0.5f },
                    new SiteType { label = "轨道遗迹", rewardPart = MMT_ExplorationDefOf.MMT_SitePart_OrbitalRuinCache, mechGuard = true, threatFactor = 1f },
                    new SiteType { label = "机械据点", rewardPart = MMT_ExplorationDefOf.MMT_SitePart_StrongholdCache, mechGuard = true, threatFactor = 1.6f },
                });
            }
        }

        /// <summary>扫描完成：随机据点类型 → 世界地图生成 Site → 发信。</summary>
        private void FinishScan()
        {
            // 据点类型加权随机
            float roll = Rand.Value;
            SiteType type = roll < 0.45f ? SiteTypes[0] : roll < 0.80f ? SiteTypes[1] : SiteTypes[2];
            Site site = TryGenerateSite(type);
            if (site == null)
            {
                // 找不到合适世界格（极端情况）：退还一半蓄电，发提示信
                batteryComp?.TryDrain(-(batteryComp.MaxChargeWd * 0.5f));
                Find.LetterStack.ReceiveLetter("深空扫描无结果", "扫描完成，但未能在附近空域定位到可降落的目标。扫描仪回收了部分能量。",
                    LetterDefOf.NeutralEvent, new LookTargets(parent));
                return;
            }
            Find.LetterStack.ReceiveLetter("深空扫描完成：" + type.label,
                "扫描仪定位到一处" + type.label + "。坐标已同步至世界地图（30 天后信号消失）。\n\n派出折跃小队前往探索，取回里面的远古材料。",
                LetterDefOf.PositiveEvent, new LookTargets(site));
            Messages.Message("深空扫描完成：" + type.label + " 已出现在世界地图", new LookTargets(parent),
                MessageTypeDefOf.PositiveEvent, false);
        }

        /// <summary>在世界地图生成据点（非水非占领格，距家 6-30 格）。</summary>
        private Site TryGenerateSite(SiteType type)
        {
            Map homeMap = parent.Map;
            PlanetTile homeTile = homeMap.Tile;
            if (!homeTile.Valid)
                return null;
            if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, homeTile, 6, 30, true,
                null, 0f, false, TileFinderMode.Random, false, false, null, null))
                return null;
            List<SitePartDef> parts = new List<SitePartDef> { type.rewardPart };
            float threatPoints = Mathf.Clamp(homeMap.wealthWatcher.WealthTotal / 100f, 800f, 5000f) * type.threatFactor;
            if (type.mechGuard && MechClusterPart != null)
                parts.Add(MechClusterPart); // 原版休眠机械守卫（绝不自写判敌逻辑；无 Royalty 时退化为无守卫）
            Site site = SiteMaker.MakeSite(parts, tile, null, true, threatPoints, null);
            if (site == null)
                return null;
            site.customLabel = type.label;
            Find.WorldObjects.Add(site);
            // 30 天过期
            TimeoutComp timeout = site.GetComponent<TimeoutComp>();
            timeout?.StartTimeout(30 * 60000);
            return site;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            Command_Action scan = new Command_Action();
            scan.defaultLabel = IsScanning ? "扫描中…" : "启动深空扫描";
            scan.defaultDesc = "消耗全部蓄电与 1 个矿物声纳模块，扫描 "
                + Props.scanDays.ToString("0.#") + " 天后在世界地图生成一处探索据点（30 天过期）。";
            scan.icon = parent.def.uiIcon;
            string reason = CanStartScan();
            if (reason != null)
            {
                scan.Disable(reason);
            }
            else
            {
                scan.action = StartScan;
            }
            yield return scan;
            // 开发者模式：即刻扫描（跳过蓄电/声纳模块消耗与等待，直接出据点）
            if (Prefs.DevMode)
            {
                Command_Action devScan = new Command_Action();
                devScan.defaultLabel = "DEV: 即刻扫描";
                devScan.defaultDesc = IsScanning
                    ? "开发者模式：立即完成当前扫描并生成据点。"
                    : "开发者模式：跳过蓄电与声纳模块消耗，立即完成一次扫描并生成据点。";
                devScan.icon = parent.def.uiIcon;
                devScan.action = DevInstantScan;
                yield return devScan;
            }
        }

        /// <summary>开发者即刻扫描：扫描中直接结算；空闲时不消耗资源直接走完成流程。</summary>
        private void DevInstantScan()
        {
            if (IsScanning)
                scanTicksLeft = -1;
            FinishScan();
        }

        public override string CompInspectStringExtra()
        {
            if (IsScanning)
            {
                float speed = 1f + (slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f);
                return "扫描中: 剩余 " + (scanTicksLeft / 60000f).ToString("0.0") + " 天"
                    + (speed > 1f ? "（加速 ×" + speed.ToString("0.0") + "）" : "");
            }
            return null;
        }
    }

    /// <summary>据点奖励参数（SitePartDef 的 modExtension）：生成的资源堆条目。</summary>
    public class StockpileRewardExtension : DefModExtension
    {
        /// <summary>奖励物品（def + 数量区间）。</summary>
        public List<ThingDefCountRangeClass> rewards;
    }

    /// <summary>
    /// 探索据点奖励堆 Worker（设计方案 §3）：PostMapGenerate 在据点小地图上生成
    /// 沙袋围起的奖励堆（独占资源 + 随机太空零件）。一个类三个参数化 def 复用。
    /// </summary>
    public class SitePartWorker_MMT_Stockpile : SitePartWorker
    {
        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            StockpileRewardExtension ext = def.GetModExtension<StockpileRewardExtension>();
            if (ext?.rewards == null || ext.rewards.Count == 0)
                return;
            // 奖励堆落点：地图中心附近的可站立格（机械集群等 genstep 也在中心，向外偏一点找空位）
            IntVec3 center = CellFinderLoose.RandomCellWith(
                c => c.Standable(map) && !c.Fogged(map) && c.GetFirstItem(map) == null
                    && c.GetFirstBuilding(map) == null, map, 200);
            if (!center.IsValid)
                center = map.Center;
            // 生成奖励堆（每条目一堆，散布在落点周围 2 格内）
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
