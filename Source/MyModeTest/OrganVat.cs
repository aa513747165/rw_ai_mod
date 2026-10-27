using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 器官培养舱的一条培养配方（XML 可配置）：
    /// 产物 + 原料（生物质数量或一具人类尸体）+ 是否需要"义肢制造 V"研究。
    /// </summary>
    public class OrganVatProduct
    {
        /// <summary>产物 def（原版器官或 MMT_Biomatter）。</summary>
        public ThingDef productDef;
        /// <summary>产物数量（提取生物质时为 20）。</summary>
        public int productCount = 1;
        /// <summary>原料物品 def（生物质）；useCorpse 时为空。</summary>
        public ThingDef ingredientDef;
        /// <summary>原料数量。</summary>
        public int ingredientCount = 35;
        /// <summary>true = 原料为一具人类尸体（提取生物质）。</summary>
        public bool useCorpse;
        /// <summary>true = 需要研究 MMT_Prosthesis_D2（义肢制造 V）才能培养。</summary>
        public bool needsOrganResearch;
        /// <summary>右键菜单显示的动作名（如"培养心脏"/"提取生物质"）。</summary>
        public string label;
    }

    /// <summary>
    /// 器官培养舱参数：培养时长与启动工时（XML 可配置）。
    /// </summary>
    public class CompProperties_OrganVat : CompProperties
    {
        /// <summary>无人值守培养时长 tick（90000 = 1.5 游戏天）。</summary>
        public int formingTicks = 90000;
        /// <summary>殖民者启动培养的工时 tick。</summary>
        public int initiateTicks = 1800;
        /// <summary>可培养的产物列表。</summary>
        public List<OrganVatProduct> products;

        public CompProperties_OrganVat()
        {
            compClass = typeof(CompOrganVat);
        }
    }

    /// <summary>
    /// 器官培养舱核心 Comp（仿原版机械培养器体验，但产物是普通物品）：
    /// 殖民者搬原料短工时启动后，设备通电无人值守培养 formingTicks，完成自动产出到交互格；
    /// 断电/关闭时培养暂停。右键菜单选择培养内容。
    /// </summary>
    public class CompOrganVat : ThingComp
    {
        /// <summary>培养中的产物 def（scribe，null = 空闲）。</summary>
        private ThingDef formingProduct;
        /// <summary>培养中的产物数量（scribe）。</summary>
        private int formingCount;
        /// <summary>培养剩余 tick（scribe）。</summary>
        private int formingTicksLeft;

        /// <summary>菜单点击后待启动的培养项（不存档：小人走一半读档回来只需重新下单）。</summary>
        private OrganVatProduct pending;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        public CompProperties_OrganVat Props
        {
            get { return (CompProperties_OrganVat)props; }
        }

        /// <summary>是否正在培养中。</summary>
        public bool IsForming
        {
            get { return formingProduct != null; }
        }

        /// <summary>菜单点击时记录的待启动培养项（Job 完成时消费）。</summary>
        public OrganVatProduct Pending
        {
            get { return pending; }
            set { pending = value; }
        }

        /// <summary>培养进度 0~1（供检查面板）。</summary>
        public float Progress
        {
            get { return IsForming ? 1f - (float)formingTicksLeft / Props.formingTicks : 0f; }
        }

        /// <summary>设备是否可用：开启开关且通电（断电时培养暂停）。</summary>
        public bool CanRun()
        {
            CompFlickable flick = flickComp;
            if (flick != null && !flick.SwitchIsOn)
                return false;
            CompPowerTrader power = powerComp;
            return power == null || power.PowerOn;
        }

        /// <summary>培养推进：仅通电且开启时计时，归零产出到交互格。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (!IsForming || !CanRun())
                return;
            formingTicksLeft--;
            if (formingTicksLeft <= 0)
                FinishForming();
        }

        /// <summary>启动一次培养（原料已在 Job 中消耗）。</summary>
        public void StartForming(OrganVatProduct product)
        {
            formingProduct = product.productDef;
            formingCount = product.productCount;
            formingTicksLeft = Props.formingTicks;
            pending = null;
        }

        /// <summary>流程结束：产出一批产物到交互格并播报。</summary>
        private void FinishForming()
        {
            Thing thing = ThingMaker.MakeThing(formingProduct);
            thing.stackCount = formingCount;
            GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
            Messages.Message("培养完成：" + formingProduct.label + " x" + formingCount + " 已产出到培养舱旁",
                new LookTargets(parent), MessageTypeDefOf.PositiveEvent, false);
            formingProduct = null;
            formingCount = 0;
            formingTicksLeft = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref formingProduct, "formingProduct");
            Scribe_Values.Look(ref formingCount, "formingCount");
            Scribe_Values.Look(ref formingTicksLeft, "formingTicksLeft");
        }

        // ---- 右键菜单：选择培养内容 ----

        /// <summary>右键菜单：殖民者搬原料启动培养（忙碌/研究不足/缺料时禁用并注明原因）。</summary>
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                yield return new FloatMenuOption("启动培养（设备被禁止或在小人活动区域外）", null);
                yield break;
            }
            if (IsForming)
            {
                // 流程节点：培养舱一次只能培养一项
                yield return new FloatMenuOption("启动培养（培养中：" + formingProduct.label
                    + "，剩余 " + formingTicksLeft.ToStringTicksToPeriod() + "）", null);
                yield break;
            }
            foreach (OrganVatProduct product in Props.products)
            {
                string label = product.label + "（" + IngredientDesc(product)
                    + "，培养 " + (Props.formingTicks / 60000f).ToString("F1") + " 天）";
                // 流程节点：研究门槛（器官培养需义肢制造 V）
                if (product.needsOrganResearch && !MMT_DefOf.MMT_Prosthesis_D2.IsFinished)
                {
                    yield return new FloatMenuOption(label + "（需研究：义肢制造 V）", null);
                    continue;
                }
                Thing ingredient = FindIngredient(selPawn, product);
                if (ingredient == null)
                {
                    yield return new FloatMenuOption(label + "（缺少原料：" + IngredientDesc(product) + "）", null);
                    continue;
                }
                OrganVatProduct selected = product;
                Thing target = ingredient;
                yield return new FloatMenuOption(label, delegate
                {
                    pending = selected;
                    Job job = JobMaker.MakeJob(MMT_DefOf.MMT_InitiateOrganCulture, target, parent);
                    job.count = selected.useCorpse ? 1 : selected.ingredientCount;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            }
        }

        /// <summary>原料描述文案：尸体 1 具或生物质数量。</summary>
        private static string IngredientDesc(OrganVatProduct product)
        {
            return product.useCorpse ? "人类尸体 x1" : product.ingredientDef.label + " x" + product.ingredientCount;
        }

        /// <summary>找一件可达原料：生物质需单堆数量足够，尸体需人类尸体（均排除禁用/已预定）。</summary>
        private Thing FindIngredient(Pawn selPawn, OrganVatProduct product)
        {
            if (product.useCorpse)
            {
                return GenClosest.ClosestThingReachable(selPawn.Position, parent.Map,
                    ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch,
                    TraverseParms.For(selPawn), 9999f,
                    t => t is Corpse c && c.InnerPawn != null && c.InnerPawn.RaceProps.Humanlike
                        && !t.IsForbidden(selPawn) && selPawn.CanReserve(t, 1, -1, null, false));
            }
            return GenClosest.ClosestThingReachable(selPawn.Position, parent.Map,
                ThingRequest.ForDef(product.ingredientDef), PathEndMode.ClosestTouch,
                TraverseParms.For(selPawn), 9999f,
                t => t.stackCount >= product.ingredientCount
                    && !t.IsForbidden(selPawn) && selPawn.CanReserve(t, 1, -1, null, false));
        }

        public override string CompInspectStringExtra()
        {
            if (!IsForming)
                return null;
            string text = "培养中: " + formingProduct.label + " x" + formingCount
                + "（" + Progress.ToStringPercent() + "，剩余 " + formingTicksLeft.ToStringTicksToPeriod() + "）";
            if (!CanRun())
                text += "\n培养暂停：需要开启开关并通电";
            return text;
        }
    }

    /// <summary>
    /// 启动培养作业（仿安装增效模块）：取原料 → 搬到培养舱 → 短工时启动 → 消耗原料并转入无人值守培养。
    /// targetA=原料（生物质堆或尸体），targetB=培养舱。
    /// </summary>
    public class JobDriver_OrganVatInitiate : JobDriver
    {
        private CompOrganVat Vat
        {
            get { return ((ThingWithComps)job.GetTarget(TargetIndex.B).Thing).GetComp<CompOrganVat>(); }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定原料不预定设备（与增效模块安装同理，避免与加油等作业冲突）
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            this.FailOn(() => Vat.IsForming || Vat.Pending == null);
            // 取货：走到原料并拾取所需数量入背包
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
            // 搬运到培养舱交互格
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            // 启动：短工时 + 进度条；期间设备被抢占则中止
            Toil wait = Toils_General.Wait(Vat.Props.initiateTicks, TargetIndex.B);
            wait.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            wait.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            wait.FailOn(() => Vat.IsForming || Vat.Pending == null);
            wait.WithProgressBarToilDelay(TargetIndex.B);
            yield return wait;
            // 流程结束：消耗背包原料并启动培养
            yield return Toils_General.DoAtomic(delegate
            {
                OrganVatProduct product = Vat.Pending;
                if (product == null || Vat.IsForming)
                    return;
                if (!ConsumeIngredient(product))
                    return;
                Vat.StartForming(product);
            });
        }

        /// <summary>消耗背包中的原料（尸体整具销毁；生物质按数量扣除），不足则不消耗并返回 false。</summary>
        private bool ConsumeIngredient(OrganVatProduct product)
        {
            foreach (Thing t in pawn.inventory.innerContainer)
            {
                if (product.useCorpse)
                {
                    if (t is Corpse c && c.InnerPawn != null && c.InnerPawn.RaceProps.Humanlike)
                    {
                        t.Destroy();
                        return true;
                    }
                }
                else if (t.def == product.ingredientDef && t.stackCount >= product.ingredientCount)
                {
                    t.stackCount -= product.ingredientCount;
                    if (t.stackCount <= 0)
                        t.Destroy();
                    return true;
                }
            }
            return false;
        }
    }
}
