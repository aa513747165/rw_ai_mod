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

        public CompProperties_ChunkDrill Props
        {
            get { return (CompProperties_ChunkDrill)props; }
        }

        /// <summary>当前一块岩块的进度 0~1（供进度条与检查面板）。</summary>
        public float Progress
        {
            get { return progress / Props.workTicksPerChunk; }
        }

        /// <summary>手动开关是唯一闸门（无需电力、无需矿脉）。</summary>
        public bool CanDrillNow()
        {
            CompFlickable flick = parent.GetComp<CompFlickable>();
            return flick == null || flick.SwitchIsOn;
        }

        /// <summary>累积一次 tick 的采石工作量，满额产出并保留溢出进度。</summary>
        public void DrillWorkDone(Pawn driller)
        {
            progress += driller.GetStatValue(StatDefOf.DeepDrillingSpeed);
            while (progress >= Props.workTicksPerChunk)
            {
                // 保留溢出，长班不吞进度
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
            thing.stackCount = Props.productCount;
            GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref progress, "progress");
        }

        public override string CompInspectStringExtra()
        {
            return "采石进度: " + Progress.ToStringPercent();
        }
    }

    /// <summary>采矿工作：扫描地图上的手动采石机并派发作业。仿原版 WorkGiver_DeepDrill。</summary>
    public class WorkGiver_ChunkDrill : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get { return PathEndMode.InteractionCell; }
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get { return ThingRequest.ForDef(MMT_DefOf.MMT_ChunkDrill); }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building b = t as Building;
            if (b == null || b.Faction != pawn.Faction)
                return false;
            if (b.IsForbidden(pawn) || b.IsBurning())
                return false;
            CompChunkDrill comp = b.GetComp<CompChunkDrill>();
            if (comp == null || !comp.CanDrillNow())
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
        public static JobDef MMT_OperateChunkDrill;

        static MMT_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_DefOf));
        }
    }
}
