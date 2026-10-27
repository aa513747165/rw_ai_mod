using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 手动采石机参数：钻出一份产物所需的基础工作量与产物配置（粗矿石）。
    /// </summary>
    public class CompProperties_ChunkDrill : CompProperties
    {
        /// <summary>DeepDrillingSpeed=1 时钻出一份产物所需 tick（10000 ≈ 4 游戏小时）。</summary>
        public int workTicksPerChunk = 10000;
        /// <summary>每 tick 采矿技能经验（对齐原版深钻 0.065）。</summary>
        public float skillXpPerTick = 0.065f;
        /// <summary>产出的物品 def（粗矿石）。</summary>
        public ThingDef productDef;
        /// <summary>每次产出的数量。</summary>
        public int productCount = 25;
        /// <summary>自动版：true 时无需殖民者操作，Comp 自行按固定速率累积进度（需供电）。</summary>
        public bool autoDrill;
        /// <summary>自动版每 tick 累积的工作量（对齐 DeepDrillingSpeed=1 的殖民者）。</summary>
        public float autoWorkPerTick = 1f;

        public CompProperties_ChunkDrill()
        {
            compClass = typeof(CompChunkDrill);
        }
    }

    /// <summary>
    /// 手动采石 Comp：不 tick。进度只在 JobDriver 的 tickAction 里按 pawn 的
    /// DeepDrillingSpeed 累积，满额在交互格产出一份粗矿石。进度存 Comp（可换班/读档续钻）。
    /// </summary>
    public class CompChunkDrill : ThingComp
    {
        /// <summary>已累积工作量（scribe）。</summary>
        private float progress;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;
        private CompModuleSlots slotsComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
            slotsComp = parent.GetComp<CompModuleSlots>();
        }

        public CompProperties_ChunkDrill Props
        {
            get { return (CompProperties_ChunkDrill)props; }
        }

        /// <summary>当前一块岩块的进度 0~1（供进度条与检查面板）。</summary>
        public float Progress
        {
            get { return progress / Props.workTicksPerChunk; }
        }

        /// <summary>手动开关 + 供电（电动/自动版）双闸门（无需矿脉）。</summary>
        public bool CanDrillNow()
        {
            CompFlickable flick = flickComp;
            if (flick != null && !flick.SwitchIsOn)
                return false;
            CompPowerTrader power = powerComp;
            return power == null || power.PowerOn;
        }

        /// <summary>累积一次 tick 的采石工作量，满额产出并保留溢出进度。</summary>
        public void DrillWorkDone(Pawn driller)
        {
            AddProgress(driller.GetStatValue(StatDefOf.DeepDrillingSpeed));
        }

        /// <summary>自动版自运转：无需殖民者，通电且开启时按固定速率累积进度；超频模块按效果合计提速。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (!Props.autoDrill || !CanDrillNow())
                return;
            float speedBonus = slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f;
            AddProgress(Props.autoWorkPerTick * (1f + speedBonus));
        }

        /// <summary>进度累积与满额产出（保留溢出，长班不吞进度）。</summary>
        private void AddProgress(float amount)
        {
            progress += amount;
            while (progress >= Props.workTicksPerChunk)
            {
                progress -= Props.workTicksPerChunk;
                TryProduceChunk();
            }
        }

        private void TryProduceChunk()
        {
            ThingDef def = Props.productDef;
            if (def == null)
                return;
            Thing thing = ThingMaker.MakeThing(def);
            // 矿产增值模块：每件 valuePerItem 即单次产量加成（仅装了槽位 Comp 的自动采石机生效）
            int yieldBonus = slotsComp != null ? (int)slotsComp.GetEffectSum(ModuleEffectKeys.YieldBonus) : 0;
            thing.stackCount = Props.productCount + yieldBonus;
            GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref progress, "progress");
        }

        public override string CompInspectStringExtra()
        {
            string text = "采石进度: " + Progress.ToStringPercent();
            // 自动版显示已安装的模块计数（手动/电动版由殖民者操作，不受模块影响）
            if (Props.autoDrill && slotsComp != null)
                foreach (KeyValuePair<ThingDef, int> pair in slotsComp.InstalledCountsByDef())
                    text += "\n" + pair.Key.label + " x" + pair.Value;
            return text;
        }
    }

    /// <summary>采矿工作：扫描地图上的手动/电动采石机并派发作业（自动版自运转不派工）。仿原版 WorkGiver_DeepDrill。</summary>
    public class WorkGiver_ChunkDrill : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get { return PathEndMode.InteractionCell; }
        }

        /// <summary>有人操作的采石机列表（手动 + 电动），自动版不在其中。</summary>
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Building b in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(MMT_DefOf.MMT_ChunkDrill))
                yield return b;
            foreach (Building b in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(MMT_DefOf.MMT_ElectricChunkDrill))
                yield return b;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building b = t as Building;
            if (b == null || b.Faction != pawn.Faction)
                return false;
            if (b.IsForbidden(pawn) || b.IsBurning())
                return false;
            CompChunkDrill comp = b.GetComp<CompChunkDrill>();
            if (comp == null || comp.Props.autoDrill || !comp.CanDrillNow())
                return false;
            if (pawn.Map.designationManager.DesignationOn(b, DesignationDefOf.Uninstall) != null)
                return false;
            return pawn.CanReserve(b, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // 必须带 expiryInterval + checkOverrideOnExpiry（对齐原版深钻 1500）：
            // 作业 toil 是 Never 完成模式，主思考树只在作业过期时才会重跑，
            // 否则小人饿了/困了也不会被打断，一直钻到倒地。
            return JobMaker.MakeJob(MMT_DefOf.MMT_OperateChunkDrill, t, 1500, checkOverrideOnExpiry: true);
        }
    }

    /// <summary>作业：走到交互格后无限作业 toil，按 tick 累积采石进度并练采矿技能。</summary>
    public class JobDriver_OperateChunkDrill : JobDriver
    {
        private CompChunkDrill Comp
        {
            get { return ((Building)job.GetTarget(TargetIndex.A).Thing).GetComp<CompChunkDrill>(); }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
            this.FailOn(() => !Comp.CanDrillNow());
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = ToilMaker.MakeToil("OperateChunkDrill");
            work.tickAction = delegate
            {
                Pawn actor = work.actor;
                Comp.DrillWorkDone(actor);
                if (actor.skills != null)
                    actor.skills.Learn(SkillDefOf.Mining, Comp.Props.skillXpPerTick);
            };
            // 与原版深钻一致：需求/打断才停，进度存 Comp 不丢
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithProgressBar(TargetIndex.A, () => Comp.Progress);
            work.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.activeSkill = () => SkillDefOf.Mining;
            yield return work;
        }
    }

    [DefOf]
    public static class MMT_DefOf
    {
        public static ThingDef MMT_ChunkDrill;
        public static ThingDef MMT_ElectricChunkDrill;
        public static JobDef MMT_OperateChunkDrill;
        /// <summary>增效模块物品（仅旧存档迁移用：老格式槽位只存计数，加载时按这些 def 补建成条目）。</summary>
        public static ThingDef MMT_OverclockModule;
        public static ThingDef MMT_ParallelModule;
        public static ThingDef MMT_YieldModule;
        /// <summary>安装增效模块作业。</summary>
        public static JobDef MMT_InstallProcessorModule;
        /// <summary>启动器官培养作业。</summary>
        public static JobDef MMT_InitiateOrganCulture;
        /// <summary>义肢制造 V（器官培养配方研究门槛）。</summary>
        public static ResearchProjectDef MMT_Prosthesis_D2;
        /// <summary>沙子：滤网掺沙产物（玻璃链原料）。</summary>
        public static ThingDef MMT_Sand;
        /// <summary>安装滤网作业。</summary>
        public static JobDef MMT_InstallSieveMesh;

        static MMT_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_DefOf));
        }
    }
}
