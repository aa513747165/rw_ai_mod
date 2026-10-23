using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 自动处理机参数（营养膏机模式）：定时从相邻进料口消耗输入并产出。
    /// outputDef 为空时产物走 OreSiftPool 矿石池随机（筛选用）。
    /// </summary>
    public class CompProperties_AutoProcessor : CompProperties
    {
        /// <summary>每轮处理间隔（tick），默认 2500 ≈ 1 游戏小时。</summary>
        public int intervalTicks = 2500;
        /// <summary>每轮消耗的输入数量。</summary>
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
    }

    /// <summary>
    /// 自动处理 Comp：通电且开机时按间隔扫描相邻进料口（isHopper 存储建筑），
    /// 凑够一轮输入即消耗并产出到出料格，实现无人值守加工。
    /// 相邻的超频模块缩短处理间隔，并行模块使每轮多耗料多产出。
    /// </summary>
    public class CompAutoProcessor : ThingComp
    {
        // 建成后延迟启动，避免落成瞬间吞料
        private int ticksUntilNext = 600;

        private CompProperties_AutoProcessor Props
        {
            get { return (CompProperties_AutoProcessor)props; }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilNext, "ticksUntilNext", 600);
        }

        public override void CompTick()
        {
            // 断电或手动关闭时不工作
            CompPowerTrader power = parent.GetComp<CompPowerTrader>();
            if (power == null || !power.PowerOn)
                return;
            CompFlickable flick = parent.GetComp<CompFlickable>();
            if (flick != null && !flick.SwitchIsOn)
                return;
            if (--ticksUntilNext > 0)
                return;
            // 统计相邻通电的增效模块：超频提速、并行增产
            int overclock, parallel;
            CountModules(out overclock, out parallel);
            // 节点流转：缺料 -> 短间隔重试；成功 -> 进入完整处理周期（超频模块按比例缩短）
            if (TryProcess(parallel))
            {
                float speedFactor = 1f + 0.1f * overclock;
                ticksUntilNext = System.Math.Max(60, (int)(Props.intervalTicks / speedFactor));
            }
            else
            {
                ticksUntilNext = 250;
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

        private bool TryProcess(int parallel)
        {
            Map map = parent.Map;
            // 流程开始：汇总相邻进料口中可接受的输入物品
            int available = 0;
            List<Thing> sources = new List<Thing>();
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
            // 并行模块等比放大：轮数 = min(1 + 并行数, 料够跑的轮数)，一轮都跑不起则等待
            int rounds = System.Math.Min(1 + parallel, available / Props.inputCount);
            if (rounds <= 0)
                return false;

            // 消耗输入（rounds 轮共 rounds * inputCount）
            int need = rounds * Props.inputCount;
            for (int i = 0; i < sources.Count && need > 0; i++)
            {
                int take = System.Math.Min(need, sources[i].stackCount);
                sources[i].SplitOff(take).Destroy();
                need -= take;
            }

            // 产出：每轮独立产出，固定产物或矿石池随机（筛选）
            for (int r = 0; r < rounds; r++)
            {
                ThingDef outDef = Props.outputDef != null ? Props.outputDef : OreSiftPool.RollOre();
                if (outDef == null)
                    continue;
                Thing product = ThingMaker.MakeThing(outDef);
                product.stackCount = Props.outputCountRange.RandomInRange;
                GenPlace.TryPlaceThing(product, parent.InteractionCell, map, ThingPlaceMode.Near);
            }
            // 流程结束
            return true;
        }

        public override string CompInspectStringExtra()
        {
            int overclock, parallel;
            CountModules(out overclock, out parallel);
            string text = "下一轮处理: " + ticksUntilNext.ToStringTicksToPeriod();
            // 有模块时追加显示模块计数
            if (overclock > 0)
                text += "\n超频模块 x" + overclock;
            if (parallel > 0)
                text += "\n并行模块 x" + parallel;
            return text;
        }
    }
}
