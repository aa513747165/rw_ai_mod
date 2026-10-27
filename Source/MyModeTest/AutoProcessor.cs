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

    /// <summary>
    /// 增效模块槽参数：设备支持的槽位组及安装上限（XML 配置）。
    /// 模块是可制作物品，由殖民者搬运安装进设备长期生效，拆除/摧毁设备时掉落返还。
    /// </summary>
    public class CompProperties_ModuleSlots : CompProperties
    {
        /// <summary>本设备支持的槽位组及上限（key 对应模块物品扩展的 slotKey；缺省为空=不支持任何模块）。</summary>
        public List<ModuleSlotLimit> slotLimits;

        public CompProperties_ModuleSlots()
        {
            compClass = typeof(CompModuleSlots);
        }
    }

    /// <summary>
    /// 增效模块槽 Comp：记录设备内已安装的模块（slotKey + itemDef 列表，scribe），
    /// 提供安装/卸载/掉落返还与效果聚合查询（GetEffectSum）。
    /// 模块种类与效果全部走物品上的 ModuleExtension 配置（见 ModuleSystem.cs），新增模块零 C# 改动。
    /// </summary>
    public class CompModuleSlots : ThingComp
    {
        // 已安装模块列表（物品实体在安装时销毁，卸载/拆除时按 itemDef 再造返还）
        private List<InstalledSlotItem> installed = new List<InstalledSlotItem>();

        // 旧存档迁移用（老格式只存三个计数，字段名沿用；PostLoadInit 并入 installed 后清零，
        // 保存时不再写出→单向迁移，回退旧版 mod 会丢失已装模块）
        private int legacyOverclock;
        private int legacyParallel;
        private int legacyYield;

        // 全部增效模块物品 def（带 ModuleExtension 者），右键菜单/页签用；延迟扫描一次
        private static List<ThingDef> moduleDefs;

        private CompProperties_ModuleSlots Props
        {
            get { return (CompProperties_ModuleSlots)props; }
        }

        /// <summary>该物品是否为增效模块（以是否挂 ModuleExtension 为准，新增模块物品自动识别）。</summary>
        public static bool IsModule(ThingDef def)
        {
            return def != null && def.HasModExtension<ModuleExtension>();
        }

        /// <summary>全部增效模块物品 def（带 ModuleExtension 者）。</summary>
        public static List<ThingDef> AllModuleDefs()
        {
            if (moduleDefs == null)
            {
                moduleDefs = new List<ThingDef>();
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    if (IsModule(def))
                        moduleDefs.Add(def);
            }
            return moduleDefs;
        }

        /// <summary>某槽位组的安装上限（props 未配置该组为 0）。</summary>
        public int MaxCount(string slotKey)
        {
            if (Props.slotLimits != null)
                foreach (ModuleSlotLimit limit in Props.slotLimits)
                    if (limit.key == slotKey)
                        return limit.max;
            return 0;
        }

        /// <summary>某槽位组已安装数量。</summary>
        public int InstalledCount(string slotKey)
        {
            int count = 0;
            for (int i = 0; i < installed.Count; i++)
                if (installed[i].slotKey == slotKey)
                    count++;
            return count;
        }

        /// <summary>统一效果查询：Σ 已安装模块物品 ModuleExtension 中 key 的 valuePerItem。</summary>
        public float GetEffectSum(string effectKey)
        {
            float sum = 0f;
            for (int i = 0; i < installed.Count; i++)
            {
                ModuleExtension ext = installed[i].itemDef?.GetModExtension<ModuleExtension>();
                if (ext?.effects == null)
                    continue;
                for (int j = 0; j < ext.effects.Count; j++)
                    if (ext.effects[j].key == effectKey)
                        sum += ext.effects[j].valuePerItem;
            }
            return sum;
        }

        /// <summary>按物品 def 分组的已装计数（供检查面板/页签通用展示，按安装顺序）。</summary>
        public IEnumerable<KeyValuePair<ThingDef, int>> InstalledCountsByDef()
        {
            Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
            List<ThingDef> order = new List<ThingDef>();
            foreach (InstalledSlotItem item in installed)
            {
                if (item.itemDef == null)
                    continue;
                if (counts.TryGetValue(item.itemDef, out int c))
                    counts[item.itemDef] = c + 1;
                else
                {
                    counts[item.itemDef] = 1;
                    order.Add(item.itemDef);
                }
            }
            foreach (ThingDef def in order)
                yield return new KeyValuePair<ThingDef, int>(def, counts[def]);
        }

        /// <summary>页签展示行：slotLimits 与已装组取并集（旧存档装过但上限为 0 的组仍显示以便卸载）。</summary>
        public IEnumerable<ModuleSlotRow> GetRows()
        {
            List<string> keys = new List<string>();
            if (Props.slotLimits != null)
                foreach (ModuleSlotLimit limit in Props.slotLimits)
                    if (!keys.Contains(limit.key))
                        keys.Add(limit.key);
            foreach (InstalledSlotItem item in installed)
                if (!keys.Contains(item.slotKey))
                    keys.Add(item.slotKey);
            foreach (string key in keys)
            {
                // 代表物品 def：优先已装物品，否则取该组第一个可用模块 def
                ThingDef rep = null;
                foreach (InstalledSlotItem item in installed)
                    if (item.slotKey == key && item.itemDef != null)
                        rep = item.itemDef;
                if (rep == null)
                    foreach (ThingDef def in AllModuleDefs())
                    {
                        ModuleExtension ext = def.GetModExtension<ModuleExtension>();
                        if (ext != null && ext.slotKey == key)
                        {
                            rep = def;
                            break;
                        }
                    }
                if (rep == null)
                    continue;
                yield return new ModuleSlotRow
                {
                    slotKey = key,
                    itemDef = rep,
                    installed = InstalledCount(key),
                    max = MaxCount(key),
                    effectText = rep.GetModExtension<ModuleExtension>()?.effectReport,
                };
            }
        }

        /// <summary>该物品是否可装入（是模块 + 本设备支持该组 + 组内未满）。</summary>
        public bool CanInstallThing(ThingDef def)
        {
            ModuleExtension ext = def?.GetModExtension<ModuleExtension>();
            return ext != null && InstalledCount(ext.slotKey) < MaxCount(ext.slotKey);
        }

        /// <summary>安装一件模块物品：计入槽位并销毁物品；槽满或非模块返回 false 不消耗。</summary>
        public bool InstallThing(Thing module)
        {
            if (!CanInstallThing(module.def))
                return false;
            ModuleExtension ext = module.def.GetModExtension<ModuleExtension>();
            installed.Add(new InstalledSlotItem { slotKey = ext.slotKey, itemDef = module.def });
            module.Destroy();
            return true;
        }

        /// <summary>卸载某组一个模块：掉落到交互格返还物品（供页签按钮）。</summary>
        public void Uninstall(string slotKey)
        {
            for (int i = installed.Count - 1; i >= 0; i--)
            {
                if (installed[i].slotKey != slotKey)
                    continue;
                ThingDef itemDef = installed[i].itemDef;
                installed.RemoveAt(i);
                if (itemDef != null)
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(itemDef), parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
                return;
            }
        }

        /// <summary>拆除包装/被摧毁时返还全部已安装模块（在建筑原位掉落）。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            foreach (InstalledSlotItem item in installed)
                if (item.itemDef != null)
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(item.itemDef), parent.Position, map, ThingPlaceMode.Near);
            installed.Clear();
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref installed, "installed", LookMode.Deep);
            // 旧存档兼容：老格式只存三个计数（字段名沿用，仅加载时读；保存不写出→单向迁移）
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref legacyOverclock, "overclockInstalled", 0);
                Scribe_Values.Look(ref legacyParallel, "parallelInstalled", 0);
                Scribe_Values.Look(ref legacyYield, "yieldInstalled", 0);
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MigrateLegacyCounts();
            }
        }

        /// <summary>旧存档迁移：把三个旧计数按对应模块 def 的扩展补建成 installed 条目。</summary>
        private void MigrateLegacyCounts()
        {
            TryMigrateLegacy(legacyOverclock, MMT_DefOf.MMT_OverclockModule);
            TryMigrateLegacy(legacyParallel, MMT_DefOf.MMT_ParallelModule);
            TryMigrateLegacy(legacyYield, MMT_DefOf.MMT_YieldModule);
            legacyOverclock = legacyParallel = legacyYield = 0;
        }

        private void TryMigrateLegacy(int count, ThingDef def)
        {
            ModuleExtension ext = def?.GetModExtension<ModuleExtension>();
            if (count <= 0 || ext == null)
                return;
            for (int i = 0; i < count; i++)
                installed.Add(new InstalledSlotItem { slotKey = ext.slotKey, itemDef = def });
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
            foreach (ThingDef itemDef in AllModuleDefs())
            {
                ModuleExtension ext = itemDef.GetModExtension<ModuleExtension>();
                // 流程节点：本设备不支持该组（槽位上限 0 且未装过），不显示对应安装项
                if (MaxCount(ext.slotKey) <= 0 && InstalledCount(ext.slotKey) <= 0)
                    continue;
                string label = "安装" + itemDef.label;
                if (!CanInstallThing(itemDef))
                {
                    // 流程节点：槽位已满，禁用
                    yield return new FloatMenuOption(label + "（已达上限 " + MaxCount(ext.slotKey) + "）", null);
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
                if (CompModuleSlots.IsModule(t.def))
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

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        private CompRefuelable fuelComp;
        private CompModuleSlots slotsComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            flickComp = parent.GetComp<CompFlickable>();
            fuelComp = parent.GetComp<CompRefuelable>();
            slotsComp = parent.GetComp<CompModuleSlots>();
        }

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
            CompPowerTrader power = powerComp;
            if (power == null || !power.PowerOn)
                return;
            CompFlickable flick = flickComp;
            if (flick != null && !flick.SwitchIsOn)
                return;
            // 带燃料仓的建筑走燃料模式：原料即燃料，按轮从燃料仓预扣，不再扫进料口
            CompRefuelable fuel = fuelComp;

            // 统计槽位中已安装的增效模块效果：超频提速、并行增加线数
            float speedBonus = slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f;
            int targetCount = 1 + (slotsComp != null ? (int)slotsComp.GetEffectSum(ModuleEffectKeys.ExtraLanes) : 0);
            int interval = CurrentIntervalTicks(speedBonus);

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

        /// <summary>按超频加成合计计算当前一轮的处理间隔（下限 60 tick）。</summary>
        private int CurrentIntervalTicks(float speedBonus)
        {
            float speedFactor = 1f + speedBonus;
            return System.Math.Max(60, (int)(Props.intervalTicks / speedFactor));
        }

        /// <summary>汇总目标线数与当前一轮处理间隔（一次槽位读取，供 ITab 展示）。</summary>
        internal void GetDisplayInfo(out int intervalTicks, out int targetLanes)
        {
            float speedBonus = slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f;
            intervalTicks = CurrentIntervalTicks(speedBonus);
            targetLanes = 1 + (slotsComp != null ? (int)slotsComp.GetEffectSum(ModuleEffectKeys.ExtraLanes) : 0);
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
            int running = 0;
            for (int i = 0; i < lanes.Count; i++)
                if (lanes[i].running)
                    running++;
            string text = "生产线: 运行中 " + running + "/" + lanes.Count;
            // 有已安装模块时追加显示计数（按物品 def 分组，通用展示）
            if (slotsComp != null)
                foreach (KeyValuePair<ThingDef, int> pair in slotsComp.InstalledCountsByDef())
                    text += "\n" + pair.Key.label + " x" + pair.Value;
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

            // 按槽位组动态出行（slotLimits ∪ 已装组；上限 0 但旧存档装过的组仍显示，以便卸载返还）
            foreach (ModuleSlotRow row in slots.GetRows())
                DrawSlotRow(listing, slots, row);
            listing.End();
        }

        /// <summary>一行槽位：标签（已装/上限 + 效果说明）+ 卸载按钮（空槽禁用）。</summary>
        private void DrawSlotRow(Listing_Standard listing, CompModuleSlots slots, ModuleSlotRow row)
        {
            string effect = string.IsNullOrEmpty(row.effectText) ? "" : "（" + row.effectText + "）";
            listing.Label(row.itemDef.label + "：已安装 " + row.installed + "/" + row.max + effect);
            Rect btnRect = listing.GetRect(30f);
            // 卸载：返还一个模块物品到交互格
            if (Widgets.ButtonText(btnRect, "卸载一个", true, true, row.installed > 0))
                slots.Uninstall(row.slotKey);
            listing.Gap(8f);
        }
    }
}
