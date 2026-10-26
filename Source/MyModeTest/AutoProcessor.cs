using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 自动处理机参数（营养膏机模式）：定时从相邻进料口消耗输入并产出。
    /// outputDef 为空时产物走 OreSiftPool 矿石池随机（筛选用）。
    /// </summary>
    public class CompProperties_AutoProcessor : CompProperties
    {
        /// <summary>每条生产线一轮的处理间隔（tick），默认 2500 ≈ 1 游戏小时。</summary>
        public int intervalTicks = 2500;
        /// <summary>每轮消耗的输入数量（启动时预扣）。</summary>
        public int inputCount = 1;
        /// <summary>固定产物；null 表示按矿脉权重随机产矿石。</summary>
        public ThingDef outputDef;
        /// <summary>产物数量范围。</summary>
        public IntRange outputCountRange = new IntRange(1, 1);
        /// <summary>接受的输入物品（按 def）。</summary>
        public List<ThingDef> inputDefs;
        /// <summary>接受的输入物品（按类别，如 StoneChunks）。</summary>
        public List<ThingCategoryDef> inputCategories;

        public CompProperties_AutoProcessor()
        {
            compClass = typeof(CompAutoProcessor);
        }

        public bool Accepts(ThingDef def)
        {
            if (inputDefs != null && inputDefs.Contains(def))
                return true;
            if (inputCategories != null && def.thingCategories != null)
            {
                foreach (ThingCategoryDef cat in inputCategories)
                    if (def.thingCategories.Contains(cat))
                        return true;
            }
            return false;
        }
    }

    /// <summary>增效模块类型：超频加速 / 并行多产。</summary>
    public enum ModuleType
    {
        Overclock,
        Parallel
    }

    /// <summary>
    /// 增效模块参数：贴靠在自动处理机旁的 1x1 模块建筑，
    /// 通电后被相邻 CompAutoProcessor 计数生效。
    /// </summary>
    public class CompProperties_ProcessorModule : CompProperties
    {
        /// <summary>模块类型：超频 / 并行。</summary>
        public ModuleType moduleType;

        public CompProperties_ProcessorModule()
        {
            compClass = typeof(CompProcessorModule);
        }
    }

    /// <summary>增效模块标记 Comp：无自身逻辑，仅供相邻自动处理机扫描计数。</summary>
    public class CompProcessorModule : ThingComp
    {
        public CompProperties_ProcessorModule Props
        {
            get { return (CompProperties_ProcessorModule)props; }
        }

        /// <summary>统计建筑相邻 8 格内通电的指定类型增效模块数量（断电模块不计），供各自动机组共用。</summary>
        public static int CountAdjacentPowered(Thing parent, ModuleType type)
        {
            int count = 0;
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8WayAndInside(parent))
            {
                List<Thing> things = cell.GetThingList(parent.Map);
                for (int i = 0; i < things.Count; i++)
                {
                    Building b = things[i] as Building;
                    if (b == null)
                        continue;
                    CompProcessorModule module = b.GetComp<CompProcessorModule>();
                    if (module == null || module.Props.moduleType != type)
                        continue;
                    // 断电模块不生效
                    CompPowerTrader power = b.GetComp<CompPowerTrader>();
                    if (power != null && !power.PowerOn)
                        continue;
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// 单条生产线状态：空闲时等待原料，启动时预扣料并倒计时，跑满产出后回到空闲。
    /// </summary>
    public class ProcessorLane : IExposable
    {
        /// <summary>是否正在处理一轮。</summary>
        public bool running;
        /// <summary>本轮剩余 tick。</summary>
        public int ticksRemaining;
        /// <summary>本轮总 tick（启动时按超频系数计算）。</summary>
        public int ticksTotal = 1;

        /// <summary>进度 0~1，空闲为 0。</summary>
        public float Progress
        {
            get { return running ? 1f - (float)ticksRemaining / System.Math.Max(1, ticksTotal) : 0f; }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref running, "running");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
            Scribe_Values.Look(ref ticksTotal, "ticksTotal", 1);
        }
    }

    /// <summary>
    /// 自动处理 Comp：通电且开机时维护多条独立生产线（默认 1 条，
    /// 每个相邻通电的并行模块 +1 条）。每条线空闲时预扣一轮输入并启动倒计时，
    /// 跑满后产出到出料格并自动开始下一轮，实现无人值守加工。
    /// 输入模式二选一：建筑带 CompRefuelable 时走燃料模式（原料即燃料，按轮从燃料仓扣）；
    /// 否则从相邻进料口（isHopper 存储建筑）取料。两种模式都需要通电。
    /// 相邻的超频模块缩短所有线的处理间隔；并行模块被拆后，超额的运行中线跑完当前轮再退役。
    /// </summary>
    public class CompAutoProcessor : ThingComp
    {
        // 生产线列表：lanes.Count 可能大于目标线数（超额线跑完退役）
        private List<ProcessorLane> lanes = new List<ProcessorLane> { new ProcessorLane() };
        // 缺料冷却：料不够时间隔 250 tick 再扫料口，避免空闲线每 tick 空扫
        private int inputRetryCooldown;

        private CompProperties_AutoProcessor Props
        {
            get { return (CompProperties_AutoProcessor)props; }
        }

        /// <summary>生产线列表（供 ITab 只读展示）。</summary>
        internal List<ProcessorLane> Lanes
        {
            get { return lanes; }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref lanes, "lanes", LookMode.Deep);
            Scribe_Values.Look(ref inputRetryCooldown, "inputRetryCooldown");
            // 旧存档无 lanes 节点时兜底为 1 条空闲线
            if (Scribe.mode == LoadSaveMode.PostLoadInit && (lanes == null || lanes.Count == 0))
                lanes = new List<ProcessorLane> { new ProcessorLane() };
        }

        public override void CompTick()
        {
            // 断电或手动关闭时所有线冻结（燃料模式同样需要电力驱动机组）
            CompPowerTrader power = parent.GetComp<CompPowerTrader>();
            if (power == null || !power.PowerOn)
                return;
            CompFlickable flick = parent.GetComp<CompFlickable>();
            if (flick != null && !flick.SwitchIsOn)
                return;
            // 带燃料仓的建筑走燃料模式：原料即燃料，按轮从燃料仓预扣，不再扫进料口
            CompRefuelable fuel = parent.GetComp<CompRefuelable>();

            // 统计相邻通电的增效模块：超频提速、并行增加线数
            int overclock, parallel;
            CountModules(out overclock, out parallel);
            int targetCount = 1 + parallel;
            int interval = CurrentIntervalTicks(overclock);

            // 线数调整：不足补空闲线；超额的空闲线立即移除，运行中的超额线保留跑完（已预扣料不吞）
            while (lanes.Count < targetCount)
                lanes.Add(new ProcessorLane());
            for (int i = lanes.Count - 1; i >= targetCount; i--)
                if (!lanes[i].running)
                    lanes.RemoveAt(i);

            // 先推进运行中的线：倒计时到点产出并回到空闲
            bool hasIdle = false;
            for (int i = 0; i < lanes.Count; i++)
            {
                ProcessorLane lane = lanes[i];
                if (lane.running)
                {
                    if (--lane.ticksRemaining <= 0)
                    {
                        // 流程结束：本轮产出，回到空闲等待下一轮
                        ProduceOnce();
                        lane.running = false;
                    }
                }
                else if (i < targetCount)
                {
                    hasIdle = true;
                }
            }

            // 再启动空闲线：料口每 tick 至多扫一次，扫描结果在各线间共享
            if (!hasIdle)
                return;
            if (inputRetryCooldown > 0)
            {
                inputRetryCooldown--;
                return;
            }
            if (fuel != null)
            {
                // 燃料模式：燃料即原料，够一整轮才启动（对齐预扣语义，防燃料残渣空轮）
                int fuelStartable = (int)(fuel.Fuel / Props.inputCount);
                if (fuelStartable <= 0)
                {
                    // 缺料：250 tick 后再试
                    inputRetryCooldown = 250;
                    return;
                }
                for (int i = 0; i < lanes.Count && fuelStartable > 0; i++)
                {
                    ProcessorLane lane = lanes[i];
                    if (lane.running || i >= targetCount)
                        continue;
                    // 预扣一轮输入并启动本轮倒计时
                    fuel.ConsumeFuel(Props.inputCount);
                    lane.running = true;
                    lane.ticksTotal = interval;
                    lane.ticksRemaining = interval;
                    fuelStartable--;
                }
                // 仍有空闲线未启动说明料已分完，进入冷却
                for (int i = 0; i < lanes.Count; i++)
                {
                    if (!lanes[i].running && i < targetCount)
                    {
                        inputRetryCooldown = 250;
                        break;
                    }
                }
                return;
            }
            // 进料口模式：汇总相邻进料口中可接受的输入物品
            List<Thing> sources = new List<Thing>();
            int available;
            ScanHoppers(sources, out available);
            int startable = available / Props.inputCount;
            if (startable <= 0)
            {
                // 缺料：250 tick 后再试
                inputRetryCooldown = 250;
                return;
            }
            for (int i = 0; i < lanes.Count && startable > 0; i++)
            {
                ProcessorLane lane = lanes[i];
                if (lane.running || i >= targetCount)
                    continue;
                // 预扣一轮输入并启动本轮倒计时
                ConsumeFrom(sources, Props.inputCount);
                lane.running = true;
                lane.ticksTotal = interval;
                lane.ticksRemaining = interval;
                startable--;
            }
            // 仍有空闲线未启动说明料已分完，进入冷却
            for (int i = 0; i < lanes.Count; i++)
            {
                if (!lanes[i].running && i < targetCount)
                {
                    inputRetryCooldown = 250;
                    break;
                }
            }
        }

        /// <summary>扫描相邻 8 格建筑，统计通电的超频/并行模块数量（断电模块不计）。</summary>
        private void CountModules(out int overclock, out int parallel)
        {
            overclock = 0;
            parallel = 0;
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8WayAndInside(parent))
            {
                List<Thing> things = cell.GetThingList(parent.Map);
                for (int i = 0; i < things.Count; i++)
                {
                    Building b = things[i] as Building;
                    if (b == null)
                        continue;
                    CompProcessorModule module = b.GetComp<CompProcessorModule>();
                    if (module == null)
                        continue;
                    // 断电模块不生效
                    CompPowerTrader power = b.GetComp<CompPowerTrader>();
                    if (power != null && !power.PowerOn)
                        continue;
                    if (module.Props.moduleType == ModuleType.Overclock)
                        overclock++;
                    else
                        parallel++;
                }
            }
        }

        /// <summary>按超频数计算当前一轮的处理间隔（下限 60 tick）。</summary>
        private int CurrentIntervalTicks(int overclock)
        {
            float speedFactor = 1f + 0.1f * overclock;
            return System.Math.Max(60, (int)(Props.intervalTicks / speedFactor));
        }

        /// <summary>汇总目标线数与当前一轮处理间隔（一次模块扫描，供 ITab 展示）。</summary>
        internal void GetDisplayInfo(out int intervalTicks, out int targetLanes)
        {
            int overclock, parallel;
            CountModules(out overclock, out parallel);
            intervalTicks = CurrentIntervalTicks(overclock);
            targetLanes = 1 + parallel;
        }

        /// <summary>扫描相邻进料口，汇总可接受的输入物品与总数量（每 tick 至多调用一次）。</summary>
        private void ScanHoppers(List<Thing> sources, out int available)
        {
            available = 0;
            Map map = parent.Map;
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8WayAndInside(parent))
            {
                List<Thing> things = cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    Building b = things[i] as Building;
                    if (b == null || b.def.building == null || !b.def.building.isHopper)
                        continue;
                    ISlotGroupParent sp = b as ISlotGroupParent;
                    SlotGroup group = sp != null ? sp.GetSlotGroup() : null;
                    if (group == null)
                        continue;
                    foreach (Thing held in group.HeldThings)
                    {
                        if (Props.Accepts(held.def))
                        {
                            sources.Add(held);
                            available += held.stackCount;
                        }
                    }
                }
            }
        }

        /// <summary>从已汇总的输入物品中依次扣除 count 个（预扣一轮用料）。</summary>
        private static void ConsumeFrom(List<Thing> sources, int count)
        {
            for (int i = 0; i < sources.Count && count > 0; i++)
            {
                int take = System.Math.Min(count, sources[i].stackCount);
                sources[i].SplitOff(take).Destroy();
                count -= take;
            }
        }

        /// <summary>产出一份产物：固定产物或矿石池随机（筛选），放到出料格。</summary>
        private void ProduceOnce()
        {
            ThingDef outDef = Props.outputDef != null ? Props.outputDef : OreSiftPool.RollOre();
            if (outDef == null)
                return;
            Thing product = ThingMaker.MakeThing(outDef);
            product.stackCount = Props.outputCountRange.RandomInRange;
            GenPlace.TryPlaceThing(product, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        public override string CompInspectStringExtra()
        {
            int overclock, parallel;
            CountModules(out overclock, out parallel);
            int running = 0;
            for (int i = 0; i < lanes.Count; i++)
                if (lanes[i].running)
                    running++;
            string text = "生产线: 运行中 " + running + "/" + lanes.Count;
            // 有模块时追加显示模块计数
            if (overclock > 0)
                text += "\n超频模块 x" + overclock;
            if (parallel > 0)
                text += "\n并行模块 x" + parallel;
            return text;
        }
    }

    /// <summary>
    /// 自动处理机"处理进度"页签：顶部显示处理周期与模块计数，
    /// 下方每条生产线一条进度条（运行中剩余时间 / 等待原料 / 退役待完成）。
    /// </summary>
    public class ITab_AutoProcessor : ITab
    {
        public ITab_AutoProcessor()
        {
            size = new Vector2(420f, 300f);
            // 页签标题走 Keyed 翻译（1.7/Languages/ChineseSimplified/Keyed/MMT_InspectorTabs.xml）
            labelKey = "MMT_TabAutoProcessor";
        }

        protected override void FillTab()
        {
            ThingWithComps building = SelThing as ThingWithComps;
            CompAutoProcessor comp = building != null ? building.GetComp<CompAutoProcessor>() : null;
            if (comp == null)
                return;

            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            // 顶部：当前处理周期（含超频效果）；一次模块扫描同时取目标线数
            int intervalNow, target;
            comp.GetDisplayInfo(out intervalNow, out target);
            listing.Label("处理周期: " + intervalNow.ToStringTicksToPeriod());
            listing.Gap(6f);

            // 逐线进度条：超额线（并行模块已拆）标注退役待完成
            for (int i = 0; i < comp.Lanes.Count; i++)
            {
                ProcessorLane lane = comp.Lanes[i];
                Rect barRect = listing.GetRect(26f);
                Widgets.FillableBar(barRect, lane.Progress);
                string status;
                if (i >= target && lane.running)
                    // 退役线：并行模块已拆，跑完本轮即移除
                    status = "第 " + (i + 1) + " 线：退役（跑完本轮移除），剩余 " + lane.ticksRemaining.ToStringTicksToPeriod();
                else if (lane.running)
                    status = "第 " + (i + 1) + " 线：运行中，剩余 " + lane.ticksRemaining.ToStringTicksToPeriod();
                else
                    status = "第 " + (i + 1) + " 线：等待原料";
                Widgets.Label(barRect, status);
                listing.Gap(4f);
            }
            listing.End();
        }
    }
}
