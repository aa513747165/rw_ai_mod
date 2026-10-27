using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 可安装槽位接收者：统一安装作业只依赖本接口操作槽位（安装校验 + 执行安装）。
    /// 实现者：CompModuleSlots（计数槽）、CompSieveMesh（单槽）。
    /// </summary>
    public interface IInstallSlotReceiver
    {
        bool CanInstallThing(ThingDef def);
        bool InstallThing(Thing item);
    }

    /// <summary>安装菜单候选：一个菜单项（禁用项灰显不派活）。</summary>
    public class SlotOffer
    {
        /// <summary>菜单项完整文案（禁用时自行拼接原因，如"安装超频模块（已达上限 5）"）。</summary>
        public string label;
        /// <summary>待装物品 def（启用项必填）。</summary>
        public ThingDef itemDef;
        /// <summary>true=禁用项（灰显，不派活）。</summary>
        public bool disabled;
        /// <summary>地图上没有该物品时的追加提示（如"，需先在电动装配台制作"）。</summary>
        public string missingHint;
    }

    /// <summary>
    /// 可安装物品槽位 Comp 基类：收敛"右键安装菜单 + 可达物品查找"的公共逻辑
    /// （原 CompModuleSlots 与 CompSieveMesh 各有一份几乎逐行相同的实现）。
    /// 子类负责：安装作业 JobDef、候选列表 GetOffers、安装/卸载/拆除返还与 scribe。
    /// </summary>
    public abstract class CompItemSlotBase : ThingComp, IInstallSlotReceiver
    {
        /// <summary>安装作业 JobDef（统一 JobDriver_InstallIntoSlot 的空壳子类按此区分子类槽位）。</summary>
        public abstract JobDef InstallJobDef { get; }

        /// <summary>被禁止/活动区外时的总提示前缀（如"安装增效模块"）。</summary>
        protected abstract string ForbiddenLabel { get; }

        public abstract bool CanInstallThing(ThingDef def);

        public abstract bool InstallThing(Thing item);

        /// <summary>安装候选（含禁用原因项）。</summary>
        protected abstract IEnumerable<SlotOffer> GetOffers(Pawn selPawn);

        /// <summary>右键菜单：殖民者搬运物品安装（候选禁用/无可用物品时灰显并注明原因）。</summary>
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
                yield return new FloatMenuOption(ForbiddenLabel + "（设备被禁止或在小人活动区域外）", null);
                yield break;
            }
            foreach (SlotOffer offer in GetOffers(selPawn))
            {
                if (offer.disabled || offer.itemDef == null)
                {
                    yield return new FloatMenuOption(offer.label, null);
                    continue;
                }
                Thing item = FindSlotItem(selPawn, offer.itemDef);
                if (item == null)
                {
                    // 区分"地图上根本没有"与"有但不可达/被禁止/已被预定"，方便玩家排查
                    string why = parent.Map.listerThings.ThingsOfDef(offer.itemDef).Count == 0
                        ? "（没有" + offer.itemDef.label + offer.missingHint + "）"
                        : "（" + offer.itemDef.label + "不可达、被禁止或已被其他工作预定）";
                    yield return new FloatMenuOption(offer.label + why, null);
                    continue;
                }
                Thing target = item;
                string label = offer.label;
                yield return new FloatMenuOption(label, delegate
                {
                    Job job = JobMaker.MakeJob(InstallJobDef, target, parent);
                    // 1.7 的 Toils_Haul.TakeToInventory 会校验 curJob.count > 0，
                    // MakeJob 默认 count = -1 会在拾取瞬间报 "Invalid count: -1" 红字
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }

        /// <summary>找一件可达、未禁用、可预定的待装物品（不可达时视为没有）。
        /// 搜索根必须用小人位置而非建筑位置：研磨机/筛子本体 Impassable，中心格不属于任何通行区域，
        /// 以建筑位置为根的 ClosestThingReachable 会把所有物品判为"不可达"。</summary>
        protected Thing FindSlotItem(Pawn selPawn, ThingDef itemDef)
        {
            return GenClosest.ClosestThingReachable(selPawn.Position, parent.Map, ThingRequest.ForDef(itemDef),
                PathEndMode.ClosestTouch, TraverseParms.For(selPawn), 9999f,
                t => !t.IsForbidden(selPawn) && selPawn.CanReserve(t, 1, -1, null, false));
        }
    }

    /// <summary>
    /// 统一安装作业（原 JobDriver_InstallModule / JobDriver_InstallSieveMesh 合并）：
    /// 取一件物品 → 搬到设备旁 → 短暂装配 → 装入槽位。targetA=物品，targetB=设备。
    /// 槽位由子类指定（一台建筑挂多个槽 Comp 时按 JobDef 对应的子类区分）。
    /// </summary>
    public abstract class JobDriver_InstallIntoSlot : JobDriver
    {
        /// <summary>装配耗时（tick）。</summary>
        private const int InstallTicks = 300;

        /// <summary>目标槽位（子类按各自 comp 类型取）。</summary>
        protected abstract IInstallSlotReceiver Slot { get; }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定物品，不预定设备本身：带燃料仓的设备（研磨机/筛子）常被
            // 加油作业长期预定，若这里也预定设备会导致安装作业根本无法开始。
            // 并发安装的槽位冲突由装配 toil 的 CanInstallThing 复检兜底（槽满不吞物品）。
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            // 取货：走到物品并拾取 1 件入背包
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
            wait.FailOn(() => !Slot.CanInstallThing(TargetItemDef()));
            wait.WithProgressBarToilDelay(TargetIndex.B);
            yield return wait;
            // 流程结束：消耗背包中的物品并装入设备槽位
            yield return Toils_General.DoAtomic(delegate
            {
                Thing item = CarriedItem();
                if (item != null)
                    Slot.InstallThing(item);
            });
        }

        /// <summary>待装物品 def（以 targetA 为准；多模块种类共存时避免装错同类他种）。</summary>
        private ThingDef TargetItemDef()
        {
            Thing target = job.GetTarget(TargetIndex.A).Thing;
            return target?.def;
        }

        /// <summary>背包中目标 def 的物品。</summary>
        private Thing CarriedItem()
        {
            ThingDef wanted = TargetItemDef();
            if (wanted == null)
                return null;
            foreach (Thing t in pawn.inventory.innerContainer)
            {
                if (t.def == wanted)
                    return t;
            }
            return null;
        }
    }

    /// <summary>安装增效模块作业（统一安装的模块槽空壳；JobDef driverClass 沿用本类名，存档/job 兼容）。</summary>
    public class JobDriver_InstallModule : JobDriver_InstallIntoSlot
    {
        protected override IInstallSlotReceiver Slot
        {
            get { return ((ThingWithComps)job.GetTarget(TargetIndex.B).Thing).GetComp<CompModuleSlots>(); }
        }
    }

    /// <summary>安装滤网作业（统一安装的滤网槽空壳；JobDef driverClass 沿用本类名，存档/job 兼容）。</summary>
    public class JobDriver_InstallSieveMesh : JobDriver_InstallIntoSlot
    {
        protected override IInstallSlotReceiver Slot
        {
            get { return ((ThingWithComps)job.GetTarget(TargetIndex.B).Thing).GetComp<CompSieveMesh>(); }
        }
    }
}
