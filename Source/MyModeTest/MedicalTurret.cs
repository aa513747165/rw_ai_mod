using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 医疗注射炮台参数：射程、冷却与每种药品的库存上限（XML 可配置）。
    /// </summary>
    public class CompProperties_MedicalTurret : CompProperties
    {
        /// <summary>射程（格）。</summary>
        public float range = 25f;

        /// <summary>两次发射的最小间隔（tick），默认 180 = 3 秒。</summary>
        public int cooldownTicks = 180;

        /// <summary>每种药品的库存上限（剂），防止单一药物占满药仓。</summary>
        public int maxPerKind = 5;

        public CompProperties_MedicalTurret()
        {
            compClass = typeof(CompMedicalTurret);
        }
    }

    /// <summary>
    /// 医疗注射炮台核心 Comp：内置药仓（允许混装，每种药品独立上限），
    /// 每种库存药品生成一个独立的"发射"gizmo，瞄准友方小人后发射注射弹。
    /// 平时无任何 tick 逻辑，待机零开销；只有玩家下令才动作。
    /// </summary>
    public class CompMedicalTurret : ThingComp, IThingHolder
    {
        /// <summary>内置药仓：持有实际药品（保留种类与数量，支持混装）。</summary>
        private ThingOwner<Thing> container;

        /// <summary>下次允许发射的游戏 tick（冷却用绝对值比较，无需每 tick 递减）。</summary>
        private int nextFireTick;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        /// <summary>合格弹药 def 缓存（全图定义扫描一次，含第三方 mod 药物）。</summary>
        private static List<ThingDef> validAmmoDefs;

        /// <summary>发射音效（惰性解析）。</summary>
        private static SoundDef fireSound;

        public CompMedicalTurret()
        {
            container = new ThingOwner<Thing>(this, LookMode.Deep, true);
        }

        // ---- IThingHolder：容器以本 comp 为持有者（1.7 ThingComp 不再自带 IThingHolder） ----

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return container;
        }

        private CompProperties_MedicalTurret Props
        {
            get { return (CompProperties_MedicalTurret)props; }
        }

        /// <summary>合格弹药 def 全集：医药类或带服用效果的药物（自动识别，无需白名单）。</summary>
        public static List<ThingDef> ValidAmmoDefs
        {
            get
            {
                if (validAmmoDefs == null)
                {
                    validAmmoDefs = new List<ThingDef>();
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                    {
                        if (MedicalTurretUtility.IsValidAmmo(def))
                            validAmmoDefs.Add(def);
                    }
                }
                return validAmmoDefs;
            }
        }

        private static SoundDef FireSound
        {
            get
            {
                if (fireSound == null)
                    fireSound = SoundDef.Named("MMT_BlowgunFiring"); // 吹箭发射音，气质契合注射镖
                return fireSound;
            }
        }

        private bool SwitchOn
        {
            get { return flickComp == null || flickComp.SwitchIsOn; }
        }

        private bool PowerOn
        {
            get { return powerComp == null || powerComp.PowerOn; }
        }

        /// <summary>距冷却结束的剩余 tick（&lt;=0 表示可发射）。</summary>
        public int TicksUntilReady
        {
            get { return nextFireTick - Find.TickManager.TicksGame; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref container, "container", this);
            Scribe_Values.Look(ref nextFireTick, "nextFireTick", 0);
        }

        /// <summary>拆除/摧毁时弹出全部药品，不吞物品。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            container.TryDropAll(parent.PositionHeld, map, ThingPlaceMode.Near);
            base.PostDeSpawn(map, mode);
        }

        // ---- 药仓存取 ----

        /// <summary>药仓中某药品的总剂数。</summary>
        public int CountOf(ThingDef def)
        {
            int count = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if (container[i].def == def)
                    count += container[i].stackCount;
            }
            return count;
        }

        /// <summary>某药品的剩余可装额度。</summary>
        public int RoomFor(ThingDef def)
        {
            return Props.maxPerKind - CountOf(def);
        }

        /// <summary>药仓中该药品的堆叠（无则 null）。</summary>
        private Thing FindStack(ThingDef def)
        {
            for (int i = 0; i < container.Count; i++)
            {
                if (container[i].def == def)
                    return container[i];
            }
            return null;
        }

        /// <summary>把药品装入药仓（按种类额度），返回实际装入数量；装不下的部分仍留在传入堆叠上由调用者处理。</summary>
        public int TryLoadDrug(Thing drug)
        {
            int take = Math.Min(RoomFor(drug.def), drug.stackCount);
            if (take <= 0)
                return 0;
            Thing toAdd = take == drug.stackCount ? drug : drug.SplitOff(take);
            if (!container.TryAdd(toAdd))
                GenPlace.TryPlaceThing(toAdd, parent.Position, parent.Map, ThingPlaceMode.Near);
            return take;
        }

        /// <summary>弹出药仓中某种药品的全部库存到交互格。</summary>
        public void EjectDrugs(ThingDef def)
        {
            for (int i = container.Count - 1; i >= 0; i--)
            {
                if (container[i].def == def)
                {
                    Thing stack = container[i];
                    container.Remove(stack);
                    GenPlace.TryPlaceThing(stack, ((Building)parent).InteractionCell, parent.Map, ThingPlaceMode.Near);
                }
            }
        }

        // ---- 发射 ----

        /// <summary>当前能否用指定药品发射；不能时给出中文原因（gizmo 灰显/消息共用）。</summary>
        public bool CanFireNow(ThingDef drugDef, out string reason)
        {
            if (!SwitchOn)
            {
                reason = "设备已关闭";
                return false;
            }
            if (!PowerOn)
            {
                reason = "未通电";
                return false;
            }
            if (TicksUntilReady > 0)
            {
                reason = "冷却中（" + (TicksUntilReady / 60f).ToString("F0") + " 秒）";
                return false;
            }
            if (CountOf(drugDef) <= 0)
            {
                reason = "药仓中没有" + drugDef.label;
                return false;
            }
            reason = null;
            return true;
        }

        /// <summary>目标是否可打：友方小人、存活、同地图、射程内且视线畅通。</summary>
        private bool IsValidTarget(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.Map != parent.Map)
                return false;
            if ((pawn.Position - parent.Position).LengthHorizontal > Props.range)
                return false;
            return GenSight.LineOfSight(parent.Position, pawn.Position, parent.Map, true);
        }

        /// <summary>每种库存药品一个独立发射 gizmo（点哪个用哪种，无"当前药品"状态）。</summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            for (int i = 0; i < container.Count; i++)
            {
                ThingDef drugDef = container[i].def;
                if (!seen.Add(drugDef))
                    continue;
                Command_Action cmd = new Command_Action();
                cmd.defaultLabel = "发射：" + drugDef.LabelCap + " ×" + CountOf(drugDef);
                cmd.defaultDesc = "向射程内的一名友方小人发射一剂" + drugDef.label + "。\n\n" + drugDef.description;
                cmd.icon = drugDef.uiIcon;
                string reason;
                if (!CanFireNow(drugDef, out reason))
                    cmd.Disable(reason);
                ThingDef captured = drugDef;
                cmd.action = delegate { StartTargeting(captured); };
                yield return cmd;
            }
        }

        /// <summary>进入瞄准模式：只可选射程内、视线畅通的友方小人，瞄准时画射程圈。</summary>
        private void StartTargeting(ThingDef drugDef)
        {
            TargetingParameters parms = new TargetingParameters();
            parms.canTargetPawns = true;
            parms.canTargetAnimals = false;
            parms.canTargetBuildings = false;
            parms.canTargetItems = false;
            parms.canTargetLocations = false;
            parms.canTargetSelf = false;
            parms.onlyTargetColonistsOrPrisonersOrSlaves = true;
            parms.validator = delegate(TargetInfo t) { return IsValidTarget(t.Thing as Pawn); };
            Find.Targeter.BeginTargeting(parms,
                delegate(LocalTargetInfo t) { FireAt(t.Thing as Pawn, drugDef); },
                highlightAction: delegate(LocalTargetInfo t) { GenDraw.DrawRadiusRing(parent.Position, Props.range); },
                targetValidator: delegate(LocalTargetInfo t) { return IsValidTarget(t.Thing as Pawn); });
        }

        /// <summary>确认目标后开火：全量复检（瞄准期间可能断电/关开关/库存被打空）再消耗药品发射。</summary>
        private void FireAt(Pawn target, ThingDef drugDef)
        {
            string reason;
            if (!CanFireNow(drugDef, out reason))
            {
                Messages.Message("无法发射：" + reason, new LookTargets(parent), MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!IsValidTarget(target))
            {
                Messages.Message("无法发射：目标不在射程内或视线被遮挡", new LookTargets(parent), MessageTypeDefOf.RejectInput, false);
                return;
            }
            // 医药目标没有需要治疗的伤势时拒绝发射（不消耗药品）
            if (drugDef.IsMedicine && !target.health.HasHediffsNeedingTend())
            {
                Messages.Message(target.LabelShort + " 没有需要治疗的伤势", new LookTargets(target), MessageTypeDefOf.RejectInput, false);
                return;
            }
            Thing stack = FindStack(drugDef);
            if (stack == null)
                return;
            // 流程开始：取 1 剂 -> 生成注射弹 -> 发射
            Thing dose = container.Take(stack, 1);
            if (dose == null)
                return;
            Projectile_MedicalSyringe proj = (Projectile_MedicalSyringe)GenSpawn.Spawn(
                MMT_MedicalTurretDefOf.MMT_Bullet_MedicalSyringe, parent.Position, parent.Map);
            proj.SetPayload(dose, target);
            proj.Launch(parent, parent.TrueCenter(), target, target, ProjectileHitFlags.None);
            nextFireTick = Find.TickManager.TicksGame + Props.cooldownTicks;
            if (FireSound != null)
                FireSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            // 流程结束
        }

        // ---- 右键菜单：手动装填/取出 ----

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                yield return new FloatMenuOption("医疗注射炮台（被禁止或在小人活动区域外）", null);
                yield break;
            }
            // 装载：列出地图上可取得且药仓有额度的药品
            foreach (ThingDef def in ValidAmmoDefs)
            {
                if (RoomFor(def) <= 0)
                    continue;
                Thing drug = FindLoadableDrug(selPawn, def);
                if (drug == null)
                    continue;
                ThingDef captured = def;
                yield return new FloatMenuOption(
                    "装载药品：" + def.label + "（" + CountOf(def) + "/" + Props.maxPerKind + "）",
                    delegate { OrderLoad(selPawn, captured); });
            }
            // 取出：每种库存药品一项
            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            for (int i = 0; i < container.Count; i++)
            {
                ThingDef def = container[i].def;
                if (!seen.Add(def))
                    continue;
                ThingDef captured = def;
                yield return new FloatMenuOption("取出药品：" + def.label + " ×" + CountOf(def),
                    delegate { OrderUnload(selPawn, captured); });
            }
        }

        /// <summary>找一件储物区内可达、未禁用、可预定的药品。搜索根必须用小人位置而非建筑位置：
        /// 以建筑为根的搜索会把物品判为"不可达"（项目已有踩坑记录）。</summary>
        private Thing FindLoadableDrug(Pawn selPawn, ThingDef drugDef)
        {
            return FindNearestUsableDrug(parent.Map, selPawn.Position, selPawn, drugDef);
        }

        /// <summary>线性扫描储物区（储物点/货架）内指定药品的全部堆叠，取距 root 最近且未禁用/可预定/可达的一件。
        /// 性能：替代 ClosestThingReachable 的全图区域 BFS，储物区内药品堆叠通常很少，
        /// 线性扫描 + 区域可达性查询便宜得多（右键菜单与搬运扫描都是高频路径）。
        /// 只搜储物区：散落在野外的药品不纳入装填来源，行为可控且进一步缩小扫描范围。</summary>
        internal static Thing FindNearestUsableDrug(Map map, IntVec3 root, Pawn pawn, ThingDef drugDef)
        {
            List<Thing> stacks = map.listerThings.ThingsOfDef(drugDef);
            if (stacks.Count == 0)
                return null;
            TraverseParms traverse = TraverseParms.For(pawn);
            Thing best = null;
            int bestDistSq = int.MaxValue;
            for (int i = 0; i < stacks.Count; i++)
            {
                Thing d = stacks[i];
                // 只搜储物区：不在储物点/货架上的堆叠跳过
                if (!d.IsInValidStorage())
                    continue;
                if (d.IsForbidden(pawn) || !pawn.CanReserve(d, 1, -1, null, false))
                    continue;
                int distSq = (d.Position - root).LengthHorizontalSquared;
                if (distSq >= bestDistSq)
                    continue;
                // 区域级可达性查询，比 ClosestThingReachable 的 BFS 便宜
                if (!map.reachability.CanReach(root, d, PathEndMode.ClosestTouch, traverse))
                    continue;
                best = d;
                bestDistSq = distSq;
            }
            return best;
        }

        /// <summary>派装填作业：targetA=药品，targetB=炮台；count=该药品剩余可装额度。</summary>
        private void OrderLoad(Pawn pawn, ThingDef drugDef)
        {
            Thing drug = FindLoadableDrug(pawn, drugDef);
            if (drug == null)
            {
                Messages.Message("没有可取得的" + drugDef.label + "（不在储物区、不可达、被禁止或已被预定）",
                    parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_MedicalTurretDefOf.MMT_LoadMedicalTurret, drug, parent);
            // 1.7 自定义作业必须显式 count，默认 -1 在搬运链会报 Invalid count
            job.count = Math.Max(1, Math.Min(RoomFor(drugDef), drug.stackCount));
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        /// <summary>派取出作业：targetA=炮台，targetB=药仓中该药品的堆叠（仅作种类标识，执行时按 def 重新解析）。</summary>
        private void OrderUnload(Pawn pawn, ThingDef drugDef)
        {
            Thing stack = FindStack(drugDef);
            if (stack == null)
                return;
            Job job = JobMaker.MakeJob(MMT_MedicalTurretDefOf.MMT_UnloadMedicalTurret, parent, stack);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (container.Count == 0)
            {
                sb.Append("药仓：空（右键装载药物）");
            }
            else
            {
                sb.Append("药仓：");
                HashSet<ThingDef> seen = new HashSet<ThingDef>();
                bool first = true;
                for (int i = 0; i < container.Count; i++)
                {
                    ThingDef def = container[i].def;
                    if (!seen.Add(def))
                        continue;
                    if (!first)
                        sb.Append("，");
                    sb.Append(def.label).Append(" ×").Append(CountOf(def)).Append("/").Append(Props.maxPerKind);
                    first = false;
                }
            }
            if (!SwitchOn)
                sb.Append("\n状态：已关闭");
            else if (!PowerOn)
                sb.Append("\n状态：未通电");
            else if (TicksUntilReady > 0)
                sb.Append("\n状态：冷却中（" + (TicksUntilReady / 60f).ToString("F0") + " 秒）");
            return sb.ToString();
        }
    }

    /// <summary>
    /// 注射弹投射物：继承 Projectile（非 Bullet，天然零伤害），重写 ImpactSomething 绕过原版
    /// 卧倒脱靶/随机拦截逻辑实现必中。携带一剂药品，命中后对目标生效，落空则掉落在地可回收。
    /// </summary>
    public class Projectile_MedicalSyringe : Projectile, IThingHolder
    {
        /// <summary>载荷：一剂药品。</summary>
        private ThingOwner<Thing> payloadHolder;

        /// <summary>锁定的目标小人（发射时登记，命中时复检存活）。</summary>
        private Pawn targetPawn;

        public Projectile_MedicalSyringe()
        {
            payloadHolder = new ThingOwner<Thing>(this, LookMode.Deep, true);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return payloadHolder;
        }

        /// <summary>登记载荷与目标（发射前由炮台调用）。</summary>
        public void SetPayload(Thing dose, Pawn target)
        {
            targetPawn = target;
            payloadHolder.TryAdd(dose);
        }

        private Thing TakePayload()
        {
            if (payloadHolder.Count == 0)
                return null;
            Thing dose = payloadHolder[0];
            payloadHolder.Remove(dose);
            return dose;
        }

        /// <summary>必中命中：目标仍有效则施药，已死/离场则载荷掉落落点格可回收。</summary>
        protected override void ImpactSomething()
        {
            Thing dose = TakePayload();
            Pawn target = targetPawn;
            if (dose != null && target != null && !target.Dead && target.Spawned && target.Map == Map)
            {
                MedicalTurretUtility.ApplyDose(target, dose);
            }
            else if (dose != null)
            {
                GenPlace.TryPlaceThing(dose, Position, Map, ThingPlaceMode.Near);
                Messages.Message("注射弹落空，药物掉落在地", new LookTargets(Position, Map),
                    MessageTypeDefOf.NeutralEvent, false);
            }
            Destroy();
        }

        /// <summary>被护盾/障碍拦截时（原版私有拦截逻辑走这里）：载荷掉落拦截点，不触发警报。</summary>
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Thing dose = TakePayload();
            if (dose != null && Position.IsValid)
                GenPlace.TryPlaceThing(dose, Position, Map, ThingPlaceMode.Near);
            Destroy();
        }

        /// <summary>存档时弹在飞行中：载荷深序列化 + 目标引用，读档后可继续飞行。</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref payloadHolder, "payloadHolder", this);
            Scribe_References.Look(ref targetPawn, "targetPawn");
        }
    }

    /// <summary>医疗注射炮台工具：弹药判定与命中生效逻辑（注射/治疗分流）。</summary>
    public static class MedicalTurretUtility
    {
        /// <summary>合格弹药：医药类（IsMedicine）或带服用效果的药物（IsDrug 且有 outcomeDoers）。</summary>
        public static bool IsValidAmmo(ThingDef def)
        {
            if (def == null)
                return false;
            if (def.IsMedicine)
                return true;
            return def.IsDrug && def.ingestible != null
                && def.ingestible.outcomeDoers != null && def.ingestible.outcomeDoers.Count > 0;
        }

        /// <summary>命中生效：医药类执行远程包扎，药物类等同目标自行服用（省略食物中毒/joy/想法等无关逻辑）。</summary>
        public static void ApplyDose(Pawn target, Thing dose)
        {
            if (dose.def.IsMedicine)
            {
                ApplyTend(target, dose);
                return;
            }
            // 注射模式：手动跑原版 Ingested 的医疗子集
            string drugLabel = dose.LabelCap;
            SoundDef ingestSound = dose.def.ingestible != null ? dose.def.ingestible.ingestSound : null;
            // CompDrug 钩子：PrePostIngested 处理成瘾，PostIngested 处理过量/记录
            ThingWithComps twc = dose as ThingWithComps;
            if (twc != null)
            {
                for (int i = 0; i < twc.AllComps.Count; i++)
                    twc.AllComps[i].PrePostIngested(target);
            }
            for (int i = 0; i < dose.def.ingestible.outcomeDoers.Count; i++)
                dose.def.ingestible.outcomeDoers[i].DoIngestionOutcome(target, dose, 1);
            if (twc != null)
            {
                for (int i = 0; i < twc.AllComps.Count; i++)
                    twc.AllComps[i].PostIngested(target);
            }
            if (ingestSound != null)
                ingestSound.PlayOneShot(new TargetInfo(target.Position, target.Map));
            Messages.Message(target.LabelShort + " 被注射了" + drugLabel,
                new LookTargets(target), MessageTypeDefOf.PositiveEvent, false);
            dose.Destroy();
        }

        /// <summary>医药模式：对目标执行一次远程包扎（无医生质量 = 0.75 × 医药效力）。</summary>
        private static void ApplyTend(Pawn target, Thing dose)
        {
            string drugLabel = dose.LabelCap;
            Medicine med = dose as Medicine;
            if (med != null)
            {
                // 1.7 的 DoTend 对 null 医生安全；无伤势时提前返回不消耗药品
                TendUtility.DoTend(null, target, med);
                if (!med.Destroyed)
                    med.Destroy(); // 未消耗时兜底销毁，与"每次发射消耗 1 剂"一致
            }
            else
            {
                // 兜底：非 Medicine 类的第三方医药，手动按品质包扎
                float quality = TendUtility.CalculateBaseTendQuality(null, target, dose.def);
                float maxQuality = dose.def.GetStatValueAbstract(StatDefOf.MedicalQualityMax);
                List<Hediff> hediffs = new List<Hediff>();
                TendUtility.GetOptimalHediffsToTendWithSingleTreatment(target, true, hediffs);
                for (int i = 0; i < hediffs.Count; i++)
                    hediffs[i].Tended(quality, maxQuality, i);
                dose.Destroy();
            }
            Messages.Message(target.LabelShort + " 接受了远程治疗（" + drugLabel + "）",
                new LookTargets(target), MessageTypeDefOf.PositiveEvent, false);
        }
    }

    /// <summary>装填药品作业（targetA=药品，targetB=炮台）：取药 -> 搬到炮台交互格 -> 装入药仓。</summary>
    public class JobDriver_LoadMedicalTurret : JobDriver
    {
        private CompMedicalTurret Comp
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.B).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompMedicalTurret>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定药品不预定炮台（参考 SlotInstall：设备常被其他作业长期预定）
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            // 取货：走到药品并拾取 job.count 剂入背包
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
            // 搬运到炮台交互格
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            // 流程结束：装入药仓，超额部分放脚边（并发装填由额度复检兜底，不吞物品）
            yield return Toils_General.DoAtomic(delegate
            {
                CompMedicalTurret comp = Comp;
                if (comp == null)
                    return;
                Thing target = job.GetTarget(TargetIndex.A).Thing;
                ThingDef wanted = target != null ? target.def : null;
                if (wanted == null)
                    return;
                Thing carried = null;
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (t.def == wanted)
                    {
                        carried = t;
                        break;
                    }
                }
                if (carried == null)
                    return;
                pawn.inventory.innerContainer.Remove(carried);
                comp.TryLoadDrug(carried);
                // 部分装入时剩余药品放回脚边；全部装入时 carried 已进药仓（ParentHolder 非空）不处理
                if (!carried.Destroyed && carried.ParentHolder == null && carried.stackCount > 0)
                    GenPlace.TryPlaceThing(carried, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            });
        }
    }

    /// <summary>取出药品作业（targetA=炮台，targetB=药仓中的堆叠仅作种类标识）：走到交互格弹出该种药品。</summary>
    public class JobDriver_UnloadMedicalTurret : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            // 流程结束：该种药品全部弹出到交互格（由其他搬运任务收走）
            yield return Toils_General.DoAtomic(delegate
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                CompMedicalTurret comp = t is ThingWithComps twc ? twc.TryGetComp<CompMedicalTurret>() : null;
                if (comp == null)
                    return;
                Thing marker = job.GetTarget(TargetIndex.B).Thing;
                if (marker == null)
                    return;
                comp.EjectDrugs(marker.def);
            });
        }
    }

    /// <summary>自动装填：扫描殖民地内医疗注射炮台，为未达上限的药品派搬运装填作业（搬运工作类型）。</summary>
    public class WorkGiver_LoadMedicalTurret : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get { return PathEndMode.InteractionCell; }
        }

        /// <summary>殖民地内全部医疗注射炮台。</summary>
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Building b in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(MMT_MedicalTurretDefOf.MMT_MedicalTurret))
                yield return b;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building b = t as Building;
            if (b == null || b.Faction != pawn.Faction)
                return false;
            if (b.IsForbidden(pawn) || b.IsBurning())
                return false;
            if (b.GetComp<CompMedicalTurret>() == null)
                return false;
            return pawn.CanReserve(b, 1, -1, null, forced);
        }

        /// <summary>为药仓未达上限的药品找最近的可用堆叠派活；找不到返回 null（与原版加油作业同一模式）。
        /// 性能：不逐 def 做全图 BFS，改为逐 def 线性扫描实际堆叠，全局取距离最近的一件。</summary>
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompMedicalTurret comp = ((ThingWithComps)t).GetComp<CompMedicalTurret>();
            Thing bestDrug = null;
            int bestDistSq = int.MaxValue;
            foreach (ThingDef def in CompMedicalTurret.ValidAmmoDefs)
            {
                if (comp.RoomFor(def) <= 0)
                    continue;
                Thing drug = CompMedicalTurret.FindNearestUsableDrug(t.Map, pawn.Position, pawn, def);
                if (drug == null)
                    continue;
                int distSq = (drug.Position - pawn.Position).LengthHorizontalSquared;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestDrug = drug;
                }
            }
            if (bestDrug == null)
                return null;
            Job job = JobMaker.MakeJob(MMT_MedicalTurretDefOf.MMT_LoadMedicalTurret, bestDrug, t);
            job.count = Math.Max(1, Math.Min(comp.RoomFor(bestDrug.def), bestDrug.stackCount));
            return job;
        }
    }

    /// <summary>
    /// 按 <c>specialDisplayRadius</c> 画射程圈的放置助手。
    /// 原版 <c>PlaceWorker_ShowTurretRadius</c> 强依赖 <c>building.turretGunDef</c>，
    /// 非枪械炮塔（如医疗注射炮台）挂载会 NRE 刷屏，故自定义本类替代。
    /// </summary>
    public class PlaceWorker_ShowSpecialRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            if (def.specialDisplayRadius > 0f)
            {
                GenDraw.DrawRadiusRing(center, def.specialDisplayRadius);
            }
        }
    }

    [DefOf]
    public static class MMT_MedicalTurretDefOf
    {
        /// <summary>医疗注射炮台（建筑）。</summary>
        public static ThingDef MMT_MedicalTurret;
        /// <summary>注射弹（投射物）。</summary>
        public static ThingDef MMT_Bullet_MedicalSyringe;
        /// <summary>装填药品作业。</summary>
        public static JobDef MMT_LoadMedicalTurret;
        /// <summary>取出药品作业。</summary>
        public static JobDef MMT_UnloadMedicalTurret;

        static MMT_MedicalTurretDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_MedicalTurretDefOf));
        }
    }
}
