using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 充能台建筑：绘制部分——台内设备飘浮在台座正上方缓慢起伏，充能时叠加微弱脉动光柱。
    /// 充能/装卸逻辑全部在 CompChargingStation 与两个 JobDriver 里。
    /// </summary>
    public class Building_ChargingStation : Building
    {
        /// <summary>光柱贴图（竖直渐变，充能时竖直绘制在台座上方）。</summary>
        private const string BeamTexPath = "Things/Building/Misc/MMT_ChargeBeam";

        // spawn 时缓存（comps 列表终身不变）
        private CompChargingStation chargeComp;
        // 光柱材质惰性缓存（仅本贴图使用，每帧改 color 做脉动是安全的）
        private Material beamMat;
        // 台内物品与其电池 comp 的缓存（持有物变动极少，按引用比对即可）
        private Thing cachedHeld;
        private CompRechargeable cachedBat;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            chargeComp = this.TryGetComp<CompChargingStation>();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Thing held = chargeComp?.HeldItem;
            if (held == null)
                return;
            // 飘浮物品：台座中心正上方，按游戏 tick 缓慢上下浮动；
            // 充满后停止浮动保持静止，一眼即可判断"已充满可取走"
            if (held != cachedHeld)
            {
                cachedHeld = held;
                cachedBat = held is ThingWithComps twc ? twc.TryGetComp<CompRechargeable>() : null;
            }
            bool full = cachedBat != null && cachedBat.IsFull;
            // 用真实时间而非游戏 tick：游戏倍速下 TicksGame 加速会导致浮动鬼畜
            // （0.05/tick × 60 tick/s ≈ 3 rad/s，与 1 倍速观感一致）
            float bob = full ? 0f : Mathf.Sin(Time.realtimeSinceStartup * 3f) * 0.06f;
            Vector3 pos = drawLoc;
            pos.y = AltitudeLayer.Item.AltitudeFor() + 0.05f;
            pos.z += 0.55f + bob;
            held.Graphic.Draw(pos, Rot4.North, held, 0f);
            if (chargeComp.IsCharging)
                DrawBeam(drawLoc);
        }

        /// <summary>
        /// 充能光柱。注意：Waylith 下竖直 quad（plane10 绕 X 轴 ±90° 立起）经对照实验
        /// （同帧并排竖直/水平 4 片）证实完全不渲染，水平 quad 则一切正常——所以这里用
        /// 6 片水平发光碟从台面堆叠到空中，俯视相机下高度差投成纵向，观感即光柱。
        /// 单一共享材质无法逐片改 alpha（渲染在帧末统一取值），故动画做在每片缩放上：
        /// 相位错开的正弦缩放，看起来像能量向上涌动。
        /// </summary>
        private void DrawBeam(Vector3 drawLoc)
        {
            if (beamMat == null)
                beamMat = MaterialPool.MatFrom(BeamTexPath, ShaderDatabase.Transparent, Color.white);
            // 真实时间驱动（理由同上）：0.08/tick→4.8 rad/s，0.07/tick→4.2 rad/s
            float pulse = 0.42f + 0.18f * Mathf.Sin(Time.realtimeSinceStartup * 4.8f);
            beamMat.color = new Color(0.6f, 0.95f, 1f, pulse);
            float baseY = AltitudeLayer.Item.AltitudeFor() + 0.08f;
            float time = Time.realtimeSinceStartup * 4.2f;
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f; // 0=贴台面，1=顶端
                float s = (0.68f - t * 0.28f) * (1f + 0.15f * Mathf.Sin(time + i * 1.1f));
                Vector3 p = new Vector3(drawLoc.x, baseY + i * 0.20f, drawLoc.z);
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(p, Quaternion.identity, new Vector3(s, 1f, s)), beamMat, 0);
            }
        }
    }

    /// <summary>
    /// 充能台参数：充能速率与功耗（XML 可配置）。
    /// </summary>
    public class CompProperties_ChargingStation : CompProperties
    {
        /// <summary>充能速度（Wd/天）。</summary>
        public float chargeRateWdPerDay = 3000f;

        /// <summary>充能中的电网功耗（W）。</summary>
        public float chargePowerW = 1500f;

        /// <summary>待机功耗（W，开启但未充能时）。</summary>
        public float idlePowerW = 25f;

        public CompProperties_ChargingStation()
        {
            compClass = typeof(CompChargingStation);
        }
    }

    /// <summary>
    /// 充能台核心 Comp：内置容器持有一件可充能设备，通电且开启时为其回充电量。
    /// 装卸完全由小人右键交互驱动（不做任何地面扫描）：空台可"装载可充能设备"
    /// （从该小人穿戴/背包中选择），有设备可"取出设备"（确认后派活取回）。
    /// </summary>
    public class CompChargingStation : ThingComp, IThingHolder
    {
        /// <summary>内置容器：持有正在充能的设备（容量 1，持有期间物品不进世界，由建筑 DrawAt 飘浮绘制）。</summary>
        private ThingOwner<Thing> container;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;

        public CompChargingStation()
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

        private CompProperties_ChargingStation Props
        {
            get { return (CompProperties_ChargingStation)props; }
        }

        /// <summary>台内设备；空台为 null。</summary>
        public Thing HeldItem
        {
            get { return container.Count > 0 ? container[0] : null; }
        }

        /// <summary>是否正在充能：有未满设备 + 开关开 + 通电。</summary>
        public bool IsCharging
        {
            get
            {
                CompRechargeable bat = BatteryOf(HeldItem);
                return bat != null && bat.NeedsCharge && SwitchOn && PowerOn;
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
        }

        /// <summary>动态功耗：需求取决于台内设备状态（不读 PowerOn，避免欠压电网下待机/充能来回振荡）。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (powerComp == null)
                return;
            if (!SwitchOn)
            {
                powerComp.PowerOutput = 0f;
                return;
            }
            CompRechargeable bat = BatteryOf(HeldItem);
            powerComp.PowerOutput = bat != null && bat.NeedsCharge ? -Props.chargePowerW : -Props.idlePowerW;
        }

        /// <summary>充能推进（delta = 距上次间隔回调经过的 tick 数，1~15）；仅在从未满变满的一拍播报"充能完成"。
        /// 注意：1.7 的 DoTick 只对 tickerType==Rare 的物体调 TickRare，Normal 建筑永远收不到
        /// CompTickRare（已反汇编 1.7 Thing.DoTick 核实），周期逻辑必须放 CompTickInterval。</summary>
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            CompRechargeable bat = BatteryOf(HeldItem);
            if (bat == null || !bat.NeedsCharge || !SwitchOn || !PowerOn)
                return;
            bat.AddCharge(Props.chargeRateWdPerDay / 60000f * delta);
            if (bat.IsFull)
                Messages.Message("充能完成：" + HeldItem.LabelCap + " 已充满，可从充能台取回",
                    new LookTargets(parent), MessageTypeDefOf.PositiveEvent, false);
        }

        /// <summary>把设备装入充能台；台内已有设备时拒绝。</summary>
        public bool TryLoadDevice(Thing device)
        {
            if (HeldItem != null || device == null)
                return false;
            return container.TryAdd(device);
        }

        /// <summary>取出台内设备（不校验调用者）；空台返回 null。</summary>
        public Thing TakeHeldItem()
        {
            Thing held = HeldItem;
            if (held != null)
                container.Remove(held);
            return held;
        }

        /// <summary>拆除/摧毁时把台内设备弹回地面，避免设备丢失。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            container.TryDropAll(parent.PositionHeld, map, ThingPlaceMode.Near);
            base.PostDeSpawn(map, mode);
        }

        // ---- 右键菜单：小人手动装卸 ----

        /// <summary>右键菜单（仿 CompOrganVat 模式）：空台给"装载可充能设备"，有设备给"取出设备"。</summary>
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                yield return new FloatMenuOption("充能台（设备被禁止或在小人活动区域外）", null);
                yield break;
            }
            if (!selPawn.CanReserve(parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption("充能台（已被其他人预定）", null);
                yield break;
            }
            Thing held = HeldItem;
            if (held == null)
            {
                yield return new FloatMenuOption("装载可充能设备", delegate { StartLoadSelection(selPawn); });
            }
            else
            {
                string name = held.LabelCap;
                // 直接派活不弹确认框：右键选项即时生效是 RimWorld 惯例，也排除对话框回调失效的可能
                yield return new FloatMenuOption("取出设备：" + name, delegate { OrderUnload(selPawn); });
            }
        }

        /// <summary>装载入口：检查小人穿戴与背包中的可充能设备；没有则提示，多件则弹选择框。</summary>
        private void StartLoadSelection(Pawn selPawn)
        {
            List<Thing> devices = FindRechargeableDevices(selPawn);
            if (devices.Count == 0)
            {
                Messages.Message(selPawn.LabelShort + " 身上没有可充能设备（需要穿戴或携带带电池的设备，如充能护盾腰带）",
                    selPawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (devices.Count == 1)
            {
                OrderLoad(selPawn, devices[0]);
                return;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Thing d in devices)
            {
                Thing local = d;
                CompRechargeable bat = BatteryOf(d);
                string pct = bat != null ? bat.ChargeRatio.ToStringPercent("F0") : "?";
                options.Add(new FloatMenuOption(d.LabelCap + "（电量 " + pct + "）",
                    delegate { OrderLoad(selPawn, local); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>收集小人身上（穿戴 + 背包）所有可充能设备。</summary>
        private static List<Thing> FindRechargeableDevices(Pawn pawn)
        {
            List<Thing> list = new List<Thing>();
            if (pawn.apparel != null)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    if (ap.TryGetComp<CompRechargeable>() != null)
                        list.Add(ap);
                }
            }
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (t.TryGetComp<CompRechargeable>() != null)
                        list.Add(t);
                }
            }
            return list;
        }

        /// <summary>派装载作业：targetA=充能台，targetB=设备（设备在小人身上，无需取货步）。</summary>
        private void OrderLoad(Pawn pawn, Thing device)
        {
            Job job = JobMaker.MakeJob(MMT_ChargingDefOf.MMT_LoadChargingStation, parent, device);
            job.count = 1; // 1.7 自定义作业必须显式 count，默认 -1 在搬运链会报 Invalid count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        /// <summary>派取回作业：targetA=充能台。失败时在消息栏与日志给出原因，避免"点了没反应"。</summary>
        private void OrderUnload(Pawn pawn)
        {
            if (HeldItem == null)
            {
                Messages.Message("充能台内没有设备可取", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(pawn.LabelShort + " 无法到达充能台（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_ChargingDefOf.MMT_UnloadChargingStation, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                Messages.Message(pawn.LabelShort + " 前往取出 " + HeldItem.LabelCap, parent,
                    MessageTypeDefOf.TaskCompletion, false);
                Log.Message("[MMT] 取出充能设备作业已派发: " + pawn.LabelShort + " -> " + parent);
            }
            else
            {
                Messages.Message(pawn.LabelShort + " 当前无法接受取出作业（征召/倒地/其他强制作业中）",
                    parent, MessageTypeDefOf.RejectInput, false);
                Log.Warning("[MMT] TryTakeOrderedJob 被拒绝: " + pawn.LabelShort + " -> " + parent);
            }
        }

        private static CompRechargeable BatteryOf(Thing thing)
        {
            return thing is ThingWithComps twc ? twc.TryGetComp<CompRechargeable>() : null;
        }

        public override string CompInspectStringExtra()
        {
            Thing held = HeldItem;
            if (held == null)
                return "状态: 待机（台内没有设备，右键装载可充能设备）";
            CompRechargeable bat = BatteryOf(held);
            if (bat == null)
                return "设备: " + held.LabelCap + "（不可充能）";
            string text = "设备: " + held.LabelCap + "（电量 " + bat.ChargeRatio.ToStringPercent("F0") + "）";
            if (bat.IsFull)
                text += " 已充满，等待取走";
            else if (IsCharging)
                text += " 充能中（" + Props.chargeRateWdPerDay.ToString("F0") + " Wd/天）";
            else
                text += " 暂停充能：需要开启开关并通电";
            return text;
        }
    }

    /// <summary>
    /// 装载充能设备作业（targetA=充能台，targetB=设备）：
    /// 设备本就在小人身上（穿戴/背包），走到充能台后卸下装入。
    /// </summary>
    public class JobDriver_LoadChargingStation : JobDriver
    {
        private CompChargingStation Station
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompChargingStation>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定充能台不预定设备（设备在小人身上，无法被其他小人抢走，但可能被自己卸下/丢弃，装入前再校验）
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            // 台内已有设备（别人抢先装入）则中止
            this.FailOn(() => Station == null || Station.HeldItem != null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.DoAtomic(delegate
            {
                CompChargingStation station = Station;
                Thing device = job.GetTarget(TargetIndex.B).Thing;
                if (station == null || device == null || station.HeldItem != null)
                    return;
                // 只接受从小人身上取出：穿戴中则脱下，背包中则移除；中途掉落/被卸则静默结束
                if (device is Apparel apparel && pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel))
                    pawn.apparel.Remove(apparel);
                else if (pawn.inventory != null && pawn.inventory.innerContainer.Contains(device))
                    pawn.inventory.innerContainer.Remove(device);
                else
                    return;
                station.TryLoadDevice(device);
            });
        }
    }

    /// <summary>
    /// 取出充能设备作业（targetA=充能台）：走到充能台取回设备，优先放入背包，放不下则放脚边。
    /// </summary>
    public class JobDriver_UnloadChargingStation : JobDriver
    {
        private CompChargingStation Station
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompChargingStation>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            // 台内设备已被取走则中止
            this.FailOn(() => Station == null || Station.HeldItem == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.DoAtomic(delegate
            {
                CompChargingStation station = Station;
                if (station == null)
                    return;
                Thing held = station.TakeHeldItem();
                if (held == null)
                    return;
                if (pawn.inventory == null || !pawn.inventory.innerContainer.TryAdd(held))
                    GenPlace.TryPlaceThing(held, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            });
        }
    }

    [DefOf]
    public static class MMT_ChargingDefOf
    {
        /// <summary>充能台（建筑）。</summary>
        public static ThingDef MMT_ChargingStation;
        /// <summary>装载充能设备作业。</summary>
        public static JobDef MMT_LoadChargingStation;
        /// <summary>取出充能设备作业。</summary>
        public static JobDef MMT_UnloadChargingStation;

        static MMT_ChargingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ChargingDefOf));
        }
    }
}
