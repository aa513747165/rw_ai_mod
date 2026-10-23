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

    /// <summary>
    /// 自动处理 Comp：通电且开机时按间隔扫描相邻进料口（isHopper 存储建筑），
    /// 凑够一轮输入即消耗并产出到出料格，实现无人值守加工。
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
            // 节点流转：缺料 -> 短间隔重试；成功 -> 进入完整处理周期
            ticksUntilNext = TryProcess() ? Props.intervalTicks : 250;
        }

        private bool TryProcess()
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
            // 输入不足一轮则等待
            if (available < Props.inputCount)
                return false;

            // 消耗输入
            int need = Props.inputCount;
            for (int i = 0; i < sources.Count && need > 0; i++)
            {
                int take = System.Math.Min(need, sources[i].stackCount);
                sources[i].SplitOff(take).Destroy();
                need -= take;
            }

            // 产出：固定产物或矿石池随机（筛选）
            ThingDef outDef = Props.outputDef != null ? Props.outputDef : OreSiftPool.RollOre();
            if (outDef == null)
                return true;
            Thing product = ThingMaker.MakeThing(outDef);
            product.stackCount = Props.outputCountRange.RandomInRange;
            GenPlace.TryPlaceThing(product, parent.InteractionCell, map, ThingPlaceMode.Near);
            // 流程结束
            return true;
        }

        public override string CompInspectStringExtra()
        {
            return "下一轮处理: " + ticksUntilNext.ToStringTicksToPeriod();
        }
    }
}
