using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 能量风暴 GameCondition（设计方案 §5，持续 1-2 天）：
    /// 1. 太阳/风能输出 ×0（发电补丁判活，见 Wonder.cs 的 Patch_WonderPowerSolar/Wind）；
    /// 2. 每 2500 tick 点名 1-3 个未防护电子设备：触发故障 + MMT 蓄电建筑放掉 25% 电（短路火花）；
    /// 3. 风暴阻尼器（CompStormDampener）半径 12 内建筑免疫点名。
    /// 候选列表：每轮点名时扫一次 allBuildingsColonist（2500 tick 一次，几百项遍历可接受，
    /// 无需注册表——原版/其它 mod 建筑无法挂注册 comp，注册表方案对它们无效）。
    /// </summary>
    public class GameCondition_MMT_EnergyStorm : GameCondition
    {
        /// <summary>点名间隔（2500 tick = 游戏 1 小时）。</summary>
        private const int StrikeInterval = 2500;

        /// <summary>每轮点名数量。</summary>
        private static readonly IntRange StrikeCount = new IntRange(1, 3);

        /// <summary>点名倒计时。</summary>
        private int ticksToStrike = StrikeInterval;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToStrike, "stormTicksToStrike", StrikeInterval);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            ticksToStrike--;
            if (ticksToStrike > 0)
                return;
            ticksToStrike = StrikeInterval;
            DoStrikeRound();
        }

        /// <summary>一轮点名：随机 1-3 个未防护电子设备故障 + 放电。</summary>
        private void DoStrikeRound()
        {
            Map map = SingleMap;
            if (map == null)
                return;
            // 候选：玩家阵营、有故障 comp 或 MMT 蓄电 comp、不在阻尼器保护内
            List<Building> candidates = new List<Building>();
            List<Building> colonistBuildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < colonistBuildings.Count; i++)
            {
                Building b = colonistBuildings[i];
                if (CompStormDampener.IsProtected(b))
                    continue;
                if (b.GetComp<CompBreakdownable>() != null || b.GetComp<CompBuildingBattery>() != null)
                    candidates.Add(b);
            }
            if (candidates.Count == 0)
                return;
            int strikes = Mathf.Min(StrikeCount.RandomInRange, candidates.Count);
            for (int i = 0; i < strikes; i++)
            {
                Building target = candidates.RandomElement();
                candidates.Remove(target);
                StrikeBuilding(map, target);
            }
        }

        /// <summary>点名单个建筑：故障 + 蓄电放电 25% + 短路火花。</summary>
        private void StrikeBuilding(Map map, Building b)
        {
            CompBreakdownable breakdown = b.GetComp<CompBreakdownable>();
            if (breakdown != null && !breakdown.BrokenDown)
                breakdown.DoBreakdown();
            CompBuildingBattery battery = b.GetComp<CompBuildingBattery>();
            if (battery != null)
                battery.TryDrain(battery.StoredChargeWd * 0.25f);
            // 短路火花
            FleckMaker.ThrowLightningGlow(b.TrueCenter(), map, 1.2f);
            MoteMaker.ThrowText(b.DrawPos, map, "能量过载!", Color.yellow, 1.5f);
        }
    }

    /// <summary>
    /// 能量风暴事件 Worker（财富 ≥20 万 + 设置开关才触发，频率乘设置倍率）。
    /// </summary>
    public class IncidentWorker_EnergyStorm : IncidentWorker
    {
        /// <summary>财富门槛 20 万。</summary>
        private const float WealthThreshold = 200000f;

        /// <summary>触发判定（1.7 入口为 CanFireNowSub）：总开关 + 财富门槛。</summary>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            // 总开关（入口第一行廉价短路）
            if (!MMTSettings.EnergyStormEnabled)
                return false;
            Map map = parms.target as Map;
            if (map == null || map.wealthWatcher.WealthTotal < WealthThreshold)
                return false;
            return base.CanFireNowSub(parms);
        }

        /// <summary>频率倍率（设置 0.5-3 直接乘触发权重）。</summary>
        public override float ChanceFactorNow(IIncidentTarget target)
        {
            return base.ChanceFactorNow(target) * MMTSettings.EnergyStormChanceFactor;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            if (map == null)
                return false;
            GameConditionDef condDef = MMT_WonderDefOf.MMT_EnergyStorm;
            // 持续 1-2 天（1.7 GameConditionDef 已无 durationTicks，时长在 MakeCondition 传入）
            int durationTicks = Mathf.RoundToInt(new FloatRange(60000f, 120000f).RandomInRange);
            GameCondition cond = GameConditionMaker.MakeCondition(condDef, durationTicks);
            map.gameConditionManager.RegisterCondition(cond);
            SendStandardLetter("能量风暴", "一场能量风暴正在逼近殖民地：太阳与风能将完全停摆，未受防护的电子设备会被逐一过载击穿。建造风暴阻尼器可保护关键产线。",
                LetterDefOf.ThreatBig, parms, new LookTargets(map.Center, map));
            return true;
        }
    }

    /// <summary>风暴阻尼器参数。</summary>
    public class CompProperties_StormDampener : CompProperties
    {
        /// <summary>保护半径（格）。</summary>
        public float radius = 12f;

        public CompProperties_StormDampener()
        {
            compClass = typeof(CompStormDampener);
        }
    }

    /// <summary>
    /// 风暴阻尼器 Comp：通电开启时保护半径 12 内建筑免于能量风暴点名。
    /// 阻尼器按 Map 注册/注销（PostSpawnSetup/DeSpawn），保护判定只查阻尼器列表（通常 0-3 个）。
    /// </summary>
    public class CompStormDampener : ThingComp
    {
        /// <summary>按图注册的阻尼器（MapRemoved 时随建筑 DeSpawn 注销）。</summary>
        private static readonly Dictionary<Map, List<CompStormDampener>> Registry =
            new Dictionary<Map, List<CompStormDampener>>();

        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        private CompProperties_StormDampener Props
        {
            get { return (CompProperties_StormDampener)props; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
            if (!Registry.TryGetValue(parent.Map, out List<CompStormDampener> list))
                Registry[parent.Map] = list = new List<CompStormDampener>();
            if (!list.Contains(this))
                list.Add(this);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            if (Registry.TryGetValue(map, out List<CompStormDampener> list))
            {
                list.Remove(this);
                if (list.Count == 0)
                    Registry.Remove(map);
            }
            base.PostDeSpawn(map, mode);
        }

        /// <summary>本阻尼器当前是否提供保护（开启 + 通电）。</summary>
        public bool Active
        {
            get
            {
                return (flickComp == null || flickComp.SwitchIsOn)
                    && (powerComp == null || powerComp.PowerOn);
            }
        }

        /// <summary>建筑是否在任一活动阻尼器的保护半径内。</summary>
        public static bool IsProtected(Building b)
        {
            if (b?.Map == null || !Registry.TryGetValue(b.Map, out List<CompStormDampener> list))
                return false;
            for (int i = 0; i < list.Count; i++)
            {
                CompStormDampener d = list[i];
                if (d.Active && d.parent.Position.DistanceTo(b.Position) <= d.Props.radius)
                    return true;
            }
            return false;
        }

        public override string CompInspectStringExtra()
        {
            return Active ? "防护半径: " + Props.radius.ToString("F0") + " 格（生效中）" : "防护半径: " + Props.radius.ToString("F0") + " 格（未生效：需要开启并通电）";
        }
    }
}
