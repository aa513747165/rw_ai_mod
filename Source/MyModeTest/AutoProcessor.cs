using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

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
        Parallel,
        Yield
    }

    /// <summary>
    /// 增效模块槽参数：自动设备内置的模块插槽上限配置。
    /// 模块是可制作物品，由殖民者搬运安装进设备长期生效，拆除/摧毁设备时掉落返还。
    /// </summary>
    public class CompProperties_ModuleSlots : CompProperties
    {
        /// <summary>超频模块安装上限（每个 +10% 处理速度）。</summary>
        public int maxOverclock = 5;
        /// <summary>并行模块安装上限（每个 +1 条生产线）。</summary>
        public int maxParallel = 3;
        /// <summary>矿产增值模块安装上限（每个产量 +10；默认 0，仅自动采石机显式开启）。</summary>
        public int maxYield = 0;

        public CompProperties_ModuleSlots()
        {
            compClass = typeof(CompModuleSlots);
        }
    }

    /// <summary>
    /// 增效模块槽 Comp：记录设备内已安装的超频/并行模块数量（scribe），
    /// 提供安装/卸载/掉落返还；右键菜单由殖民者搬运模块物品执行安装（见 JobDriver_InstallModule）。
    /// </summary>
    public class CompModuleSlots : ThingComp
    {
        // 已安装数量（槽位计数，物品实体在安装时销毁）
        private int overclockInstalled;
        private int parallelInstalled;
        private int yieldInstalled;

        private CompProperties_ModuleSlots Props
        {
            get { return (CompProperties_ModuleSlots)props; }
        }

        /// <summary>已安装超频模块数（供各自动机读速度加成）。</summary>
        public int OverclockCount
        {
            get { return overclockInstalled; }
        }

        /// <summary>已安装并行模块数（供自动处理机算生产线数）。</summary>
        public int ParallelCount
        {
            get { return parallelInstalled; }
        }

        /// <summary>已安装矿产增值模块数（供自动采石机算产量加成）。</summary>
        public int YieldCount
        {
            get { return yieldInstalled; }
        }

        /// <summary>模块物品 def → 槽位类型；非模块物品返回 null。</summary>
        public static ModuleType? TypeOf(ThingDef def)
        {
            if (def == MMT_DefOf.MMT_OverclockModule)
                return ModuleType.Overclock;
            if (def == MMT_DefOf.MMT_ParallelModule)
                return ModuleType.Parallel;
            if (def == MMT_DefOf.MMT_YieldModule)
                return ModuleType.Yield;
            return null;
        }

        /// <summary>槽位类型 → 模块物品 def。</summary>
        public static ThingDef ItemDef(ModuleType type)
        {
            switch (type)
            {
                case ModuleType.Parallel: return MMT_DefOf.MMT_ParallelModule;
                case ModuleType.Yield: return MMT_DefOf.MMT_YieldModule;
                default: return MMT_DefOf.MMT_OverclockModule;
            }
        }

        public int InstalledCount(ModuleType type)
        {
            switch (type)
            {
                case ModuleType.Parallel: return parallelInstalled;
                case ModuleType.Yield: return yieldInstalled;
                default: return overclockInstalled;
            }
        }

        public int MaxCount(ModuleType type)
        {
            switch (type)
            {
                case ModuleType.Parallel: return Props.maxParallel;
                case ModuleType.Yield: return Props.maxYield;
                default: return Props.maxOverclock;
            }
        }

        public bool CanInstall(ModuleType type)
        {
            return InstalledCount(type) < MaxCount(type);
        }

        /// <summary>该物品是否为可装入且槽位未满的模块。</summary>
        public bool CanInstallThing(ThingDef def)
        {
            ModuleType? type = TypeOf(def);
            return type != null && CanInstall(type.Value);
        }

        /// <summary>安装一件模块物品：计入槽位并销毁物品；槽满或非模块返回 false 不消耗。</summary>
        public bool InstallThing(Thing module)
        {
            if (!CanInstallThing(module.def))
                return false;
            ModuleType? type = TypeOf(module.def);
            if (type == ModuleType.Overclock)
                overclockInstalled++;
            else if (type == ModuleType.Parallel)
                parallelInstalled++;
            else
                yieldInstalled++;
            module.Destroy();
            return true;
        }

        /// <summary>卸载一个模块：掉落到交互格返还物品（供页签按钮）。</summary>
        public void Uninstall(ModuleType type)
        {
            if (InstalledCount(type) <= 0)
                return;
            if (type == ModuleType.Overclock)
                overclockInstalled--;
            else if (type == ModuleType.Parallel)
                parallelInstalled--;
            else
                yieldInstalled--;
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ItemDef(type)), parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        /// <summary>拆除包装/被摧毁时返还全部已安装模块（在建筑原位掉落）。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            for (int i = 0; i < overclockInstalled; i++)
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(MMT_DefOf.MMT_OverclockModule), parent.Position, map, ThingPlaceMode.Near);
            for (int i = 0; i < parallelInstalled; i++)
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(MMT_DefOf.MMT_ParallelModule), parent.Position, map, ThingPlaceMode.Near);
            for (int i = 0; i < yieldInstalled; i++)
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(MMT_DefOf.MMT_YieldModule), parent.Position, map, ThingPlaceMode.Near);
            overclockInstalled = 0;
            parallelInstalled = 0;
            yieldInstalled = 0;
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref overclockInstalled, "overclockInstalled");
            Scribe_Values.Look(ref parallelInstalled, "parallelInstalled");
            Scribe_Values.Look(ref yieldInstalled, "yieldInstalled");
        }

        /// <summary>右键菜单：殖民者搬运模块物品安装（槽满或无可用物品时禁用并注明原因）。</summary>
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                // IsForbidden 含"建筑格在小人活动区域外"：此时原版语义是该建筑对小人不可交互。
                // 不能静默不给选项，否则玩家完全看不出为何装不了（研磨机/筛子被活动区挡住时曾表现为"无法安装"）
                yield return new FloatMenuOption("安装增效模块（设备被禁止或在小人活动区域外）", null);
                yield break;
            }
            foreach (ModuleType type in new[] { ModuleType.Overclock, ModuleType.Parallel, ModuleType.Yield })
            {
                // 流程节点：本设备不支持该类模块（槽位上限 0 且未装过），不显示对应安装项
                if (MaxCount(type) <= 0 && InstalledCount(type) <= 0)
                    continue;
                ThingDef itemDef = ItemDef(type);
                string label = "安装" + itemDef.label;
                if (!CanInstall(type))
                {
                    // 流程节点：槽位已满，禁用
                    yield return new FloatMenuOption(label + "（已达上限 " + MaxCount(type) + "）", null);
                    continue;
                }
                Thing item = FindModuleItem(selPawn, itemDef);
                if (item == null)
                {
                    // 区分"地图上根本没有"与"有但不可达/被禁止/已被预定"，方便玩家排查
                    string why = parent.Map.listerThings.ThingsOfDef(itemDef).Count == 0
                        ? "（没有" + itemDef.label + "，需先在电动装配台制作）"
                        : "（" + itemDef.label + "不可达、被禁止或已被其他工作预定）";
                    yield return new FloatMenuOption(label + why, null);
                    continue;
                }
                Thing target = item;
                yield return new FloatMenuOption(label, delegate
                {
                    Job job = JobMaker.MakeJob(MMT_DefOf.MMT_InstallProcessorModule, target, parent);
                    // 1.7 的 Toils_Haul.TakeToInventory 会校验 curJob.count > 0，
                    // MakeJob 默认 count = -1 会在拾取瞬间报 "Invalid count: -1" 红字
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }

        /// <summary>找一件可达、未禁用、可预定的模块物品（不可达时视为没有）。
        /// 搜索根必须用小人位置而非建筑位置：研磨机/筛子本体 Impassable，中心格不属于任何通行区域，
        /// 以建筑位置为根的 ClosestThingReachable 会把所有模块判为"不可达"（采石机 PassThroughOnly 才有区域，故之前只有它能装）。</summary>
        private Thing FindModuleItem(Pawn selPawn, ThingDef itemDef)
        {
            return GenClosest.ClosestThingReachable(selPawn.Position, parent.Map, ThingRequest.ForDef(itemDef),
                PathEndMode.ClosestTouch, TraverseParms.For(selPawn), 9999f,
                t => !t.IsForbidden(selPawn) && selPawn.CanReserve(t, 1, -1, null, false));
        }
    }

    /// <summary>
    /// 安装增效模块作业（仿加燃料）：取一件模块物品 → 搬到设备旁 → 短暂装配 → 装入槽位。
    /// targetA=模块物品，targetB=设备。
    /// </summary>
    public class JobDriver_InstallModule : JobDriver
    {
        /// <summary>装配耗时（tick）。</summary>
        private const int InstallTicks = 300;

        private CompModuleSlots Slots
        {
            get { return ((ThingWithComps)job.GetTarget(TargetIndex.B).Thing).GetComp<CompModuleSlots>(); }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定模块物品，不预定设备本身：带燃料仓的设备（研磨机/筛子）常被
            // 加油作业长期预定，若这里也预定设备会导致安装作业根本无法开始。
            // 并发安装的槽位冲突由装配 toil 的 CanInstallThing 复检兜底（槽满不吞物品）。
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            // 取货：走到模块物品并拾取 1 件入背包
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.TakeToInventory(TargetIndex.A, 1);
            // 搬运到设备旁
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            // 装配：短暂等待 + 进度条；期间槽位被抢先装满则中止（不吞物品，留给最终 toil 前校验）
            Toil wait = Toils_General.Wait(InstallTicks, TargetIndex.B);
            wait.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            wait.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            wait.FailOn(() => !Slots.CanInstallThing(CarriedModuleDef()));
            wait.WithProgressBarToilDelay(TargetIndex.B);
            yield return wait;
            // 流程结束：消耗背包中的模块物品并计入设备槽位
            yield return Toils_General.DoAtomic(delegate
            {
                Thing module = CarriedModule();
                if (module != null)
                    Slots.InstallThing(module);
            });
        }

        private ThingDef CarriedModuleDef()
        {
            Thing module = CarriedModule();
            return module != null ? module.def : null;
        }

        /// <summary>背包中第一件模块物品。</summary>
        private Thing CarriedModule()
        {
            foreach (Thing t in pawn.inventory.innerContainer)
            {
                if (CompModuleSlots.TypeOf(t.def) != null)
                    return t;
            }
            return null;
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
    /// 槽位中每个已安装的并行模块 +1 条）。每条线空闲时预扣一轮输入并启动倒计时，
    /// 跑满后产出到出料格并自动开始下一轮，实现无人值守加工。
    /// 输入模式二选一：建筑带 CompRefuelable 时走燃料模式（原料即燃料，按轮从燃料仓扣）；
    /// 否则从相邻进料口（isHopper 存储建筑）取料。两种模式都需要通电。
    /// 已安装的超频模块缩短所有线的处理间隔；并行模块被卸载后，超额的运行中线跑完当前轮再退役。
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

            // 统计槽位中已安装的增效模块：超频提速、并行增加线数
            int overclock, parallel;
            GetModuleCounts(out overclock, out parallel);
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

        /// <summary>读取设备槽位中已安装的超频/并行模块数量（无槽位 Comp 按 0 计）。</summary>
        private void GetModuleCounts(out int overclock, out int parallel)
        {
            CompModuleSlots slots = parent.GetComp<CompModuleSlots>();
            overclock = slots != null ? slots.OverclockCount : 0;
            parallel = slots != null ? slots.ParallelCount : 0;
        }

        /// <summary>按超频数计算当前一轮的处理间隔（下限 60 tick）。</summary>
        private int CurrentIntervalTicks(int overclock)
        {
            float speedFactor = 1f + 0.1f * overclock;
            return System.Math.Max(60, (int)(Props.intervalTicks / speedFactor));
        }

        /// <summary>汇总目标线数与当前一轮处理间隔（一次槽位读取，供 ITab 展示）。</summary>
        internal void GetDisplayInfo(out int intervalTicks, out int targetLanes)
        {
            int overclock, parallel;
            GetModuleCounts(out overclock, out parallel);
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

        /// <summary>产出一份产物：固定产物或矿石池随机（筛选；装滤网时走滤网白名单窗口，可能掺沙），放到出料格。</summary>
        private void ProduceOnce()
        {
            ThingDef outDef = Props.outputDef;
            if (outDef == null)
            {
                CompSieveMesh meshSlot = parent.GetComp<CompSieveMesh>();
                outDef = meshSlot != null && meshSlot.HasMesh ? OreSiftPool.RollOre(meshSlot.MeshDef) : OreSiftPool.RollOre();
            }
            if (outDef == null)
                return;
            Thing product = ThingMaker.MakeThing(outDef);
            product.stackCount = Props.outputCountRange.RandomInRange;
            GenPlace.TryPlaceThing(product, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        public override string CompInspectStringExtra()
        {
            int overclock, parallel;
            GetModuleCounts(out overclock, out parallel);
            int running = 0;
            for (int i = 0; i < lanes.Count; i++)
                if (lanes[i].running)
                    running++;
            string text = "生产线: 运行中 " + running + "/" + lanes.Count;
            // 有已安装模块时追加显示计数
            if (overclock > 0)
                text += "\n超频模块 x" + overclock;
            if (parallel > 0)
                text += "\n并行模块 x" + parallel;
            return text;
        }
    }

    /// <summary>
    /// 自动处理机"处理进度"页签：顶部显示处理周期，
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

            // 逐线进度条：超额线（并行模块已卸载）标注退役待完成
            for (int i = 0; i < comp.Lanes.Count; i++)
            {
                ProcessorLane lane = comp.Lanes[i];
                Rect barRect = listing.GetRect(26f);
                Widgets.FillableBar(barRect, lane.Progress);
                string status;
                if (i >= target && lane.running)
                    // 退役线：并行模块已卸载，跑完本轮即移除
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

    /// <summary>
    /// 增效模块页签：显示超频/并行/矿产增值模块已装数量、上限与当前加成，
    /// 每类一个"卸载"按钮（掉落到交互格返还物品）；安装走右键菜单由殖民者搬运。
    /// </summary>
    public class ITab_ModuleSlots : ITab
    {
        public ITab_ModuleSlots()
        {
            size = new Vector2(420f, 300f);
            // 页签标题走 Keyed 翻译（1.7/Languages/ChineseSimplified/Keyed/MMT_InspectorTabs.xml）
            labelKey = "MMT_TabModuleSlots";
        }

        protected override void FillTab()
        {
            ThingWithComps building = SelThing as ThingWithComps;
            CompModuleSlots slots = building != null ? building.GetComp<CompModuleSlots>() : null;
            if (slots == null)
                return;

            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            // 安装引导：右键设备由殖民者搬运模块物品装入
            listing.Label("安装方式：选中殖民者右键本设备，选择安装对应模块。");
            listing.Gap(6f);

            // 上限为 0 的模块类型本设备不支持，整行隐藏；
            // 旧存档已装过的（如采石机上的并行模块）仍显示，以便卸载返还
            if (slots.MaxCount(ModuleType.Overclock) > 0 || slots.InstalledCount(ModuleType.Overclock) > 0)
                DrawSlotRow(listing, slots, ModuleType.Overclock,
                    "处理速度 +" + slots.OverclockCount * 10 + "%");
            if (slots.MaxCount(ModuleType.Parallel) > 0 || slots.InstalledCount(ModuleType.Parallel) > 0)
                DrawSlotRow(listing, slots, ModuleType.Parallel,
                    "生产线 " + (1 + slots.ParallelCount) + " 条");
            if (slots.MaxCount(ModuleType.Yield) > 0 || slots.InstalledCount(ModuleType.Yield) > 0)
                DrawSlotRow(listing, slots, ModuleType.Yield,
                    "产量 +" + slots.YieldCount * 10);
            listing.End();
        }

        /// <summary>一行槽位：标签（已装/上限 + 加成效果）+ 卸载按钮（空槽禁用）。</summary>
        private void DrawSlotRow(Listing_Standard listing, CompModuleSlots slots, ModuleType type, string effect)
        {
            ThingDef itemDef = CompModuleSlots.ItemDef(type);
            listing.Label(itemDef.label + "：已安装 " + slots.InstalledCount(type) + "/" + slots.MaxCount(type)
                + "（" + effect + "）");
            Rect btnRect = listing.GetRect(30f);
            // 卸载：返还一个模块物品到交互格
            if (Widgets.ButtonText(btnRect, "卸载一个", true, true, slots.InstalledCount(type) > 0))
                slots.Uninstall(type);
            listing.Gap(8f);
        }
    }
}
