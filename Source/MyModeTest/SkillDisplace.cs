using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 位移技能框架（第九步） ==========
    // 一、折跃 Blink：瞬间移动到技能范围内指定格（起/终点各放一次闪光 fleck）；
    // 二、群体传送 MassTeleport：先圈选一个范围，再指定落点，范围内有效目标
    //     （敌我判定沿用第一步框架 TargetsInRadius）按相对偏移整体传送到落点；
    // 三、冲锋 Charge：快速移动冲到指定位置（Thing_MMTDisplaceMover 平移 + 拖尾粒子）；
    // 四、跳跃 Jump：先升后降到指定位置——pawn 直接走原版飞行逻辑
    //     （PawnFlyer.MakeFlyer + Core 自带 PawnFlyer ThingDef，flightSpeed/heightFactor
    //     /progressCurve 全在 Core XML），建筑等非 pawn 走 Mover 抛物线升高/降落复刻同一观感。
    // MMTDisplaceUtil 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTDisplace
    // （群体传送在 Apply 后自动进入第二段落点瞄准）；建筑演示侧出口
    // AbilityTargeting.cs 的 CompSkillDemo（demoType="blink"/"massTeleport"/"charge"/"jump"）。
    // 素材仅原版贴图 + MMT 占位闪光/拖尾贴图，不依赖 RoM。
    // 注：Mover 参数为运行时传入不进存档，存档瞬间立即落地（防止载体随存档丢失）。

    /// <summary>位移类型。Blink/MassTeleport 瞬移；Charge/Jump 有移动过程。</summary>
    public enum MMTDisplaceMode
    {
        Blink,
        MassTeleport,
        Charge,
        Jump
    }

    /// <summary>位移特效/速度参数包（MMTDisplaceUtil 的输入）。</summary>
    public class MMTDisplaceParams
    {
        /// <summary>起点闪光 fleck（折跃/传送/冲锋/跳跃起跳时播放）。</summary>
        public FleckDef departFleckDef;
        /// <summary>落点闪光 fleck。</summary>
        public FleckDef arriveFleckDef;
        /// <summary>冲锋拖尾 fleck。</summary>
        public FleckDef trailFleckDef;
        /// <summary>拖尾粒子间隔 tick。</summary>
        public int trailIntervalTicks = 3;
        /// <summary>冲锋：每格耗时 tick（越小越快）。</summary>
        public float chargeTicksPerCell = 2f;
        /// <summary>冲锋最短耗时 tick。</summary>
        public int chargeMinTicks = 6;
        /// <summary>跳跃（建筑 Mover）：每格耗时 tick；pawn 走原版 PawnFlyer 的 flightSpeed。</summary>
        public float jumpTicksPerCell = 3f;
        /// <summary>跳跃（建筑 Mover）最短耗时 tick。</summary>
        public int jumpMinTicks = 25;
        /// <summary>跳跃（建筑 Mover）抛物线顶点高度（格）。</summary>
        public float jumpPeakHeight = 3f;
        public SoundDef landingSound;
    }

    /// <summary>位移统一入口：折跃 / 群体传送 / 冲锋 / 跳跃。</summary>
    public static class MMTDisplaceUtil
    {
        public const string MoverDefName = "MMT_DisplaceMover";
        /// <summary>Core 自带原版飞行实体（跳跃 pawn 专用）。</summary>
        public const string VanillaFlyerDefName = "PawnFlyer";

        /// <summary>落点可站立判定（瞬移/移动的落点都必须满足）。</summary>
        public static bool CanLand(IntVec3 cell, Map map)
        {
            return map != null && cell.InBounds(map) && cell.Standable(map);
        }

        /// <summary>在指定格放一次位移闪光（当前地图才播放）。</summary>
        public static void PlayFleck(Map map, IntVec3 cell, FleckDef def, float scale = 1f)
        {
            if (map == null || map != Find.CurrentMap || def == null)
            {
                return;
            }
            Vector3 pos = cell.ToVector3Shifted();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, def, scale);
            data.rotation = Rand.Range(0f, 360f);
            map.flecks.CreateFleck(data);
        }

        /// <summary>折跃：瞬间移动到指定格（起/终点闪光 + 落地音效），返回是否成功。</summary>
        public static bool Blink(Thing thing, IntVec3 dest, MMTDisplaceParams p)
        {
            Map map = thing?.Map;
            if (thing == null || thing.Destroyed || !thing.Spawned || !CanLand(dest, map))
            {
                return false;
            }
            if (p != null)
            {
                PlayFleck(map, thing.Position, p.departFleckDef);
            }
            thing.DeSpawn();
            GenSpawn.Spawn(thing, dest, map, thing.Rotation);
            if (p != null)
            {
                PlayFleck(map, dest, p.arriveFleckDef);
                if (p.landingSound != null)
                {
                    SoundStarter.PlayOneShot(p.landingSound, new TargetInfo(dest, map));
                }
            }
            return true;
        }

        /// <summary>群体传送：范围内有效目标按相对偏移整体传送到落点，返回传送数。</summary>
        public static int MassTeleport(Thing casterCtx, IntVec3 fromCenter, float radius, bool friendly, bool includeSelf, IntVec3 destCenter, Map map, MMTDisplaceParams p)
        {
            if (map == null || !destCenter.InBounds(map))
            {
                return 0;
            }
            int count = 0;
            foreach (Thing t in MMTAoETargetPreview.TargetsInRadius(casterCtx, fromCenter, map, radius, friendly, includeSelf).ToList())
            {
                IntVec3 dest = destCenter + (t.Position - fromCenter);
                if (!CanLand(dest, map)
                    && !CellFinder.TryFindRandomCellNear(dest, map, 4, c => CanLand(c, map), out dest))
                {
                    continue;
                }
                if (Blink(t, dest, p))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>冲锋：快速平移到指定位置（Mover 携带目标滑动 + 拖尾粒子）。</summary>
        public static Thing_MMTDisplaceMover StartCharge(Thing thing, IntVec3 dest, MMTDisplaceParams p)
        {
            return StartMover(thing, dest, MMTDisplaceMode.Charge, p);
        }

        /// <summary>
        /// 跳跃：先升后降到指定位置。pawn 直接复用原版 PawnFlyer（Core 自带飞行曲线），
        /// 建筑等非 pawn 走 Mover 抛物线复刻同一观感。
        /// </summary>
        public static void StartJump(Thing thing, IntVec3 dest, MMTDisplaceParams p)
        {
            Map map = thing?.Map;
            if (thing == null || thing.Destroyed || !thing.Spawned || !CanLand(dest, map))
            {
                return;
            }
            if (thing is Pawn pawn)
            {
                ThingDef flyerDef = DefDatabase<ThingDef>.GetNamedSilentFail(VanillaFlyerDefName);
                if (flyerDef == null)
                {
                    Log.Error("[MMT] 找不到 Core 的 " + VanillaFlyerDefName + "，无法执行跳跃");
                    return;
                }
                IntVec3 origin = pawn.Position;
                if (p != null)
                {
                    PlayFleck(map, origin, p.departFleckDef);
                }
                PawnFlyer flyer = PawnFlyer.MakeFlyer(flyerDef, pawn, dest, null, p?.landingSound, false, null, null, LocalTargetInfo.Invalid);
                // 防御：MakeFlyer 未把 pawn 收进容器时手动收编，避免 pawn 滞留原地
                if (flyer.FlyingPawn == null && pawn.Spawned)
                {
                    pawn.DeSpawn();
                    flyer.GetDirectlyHeldThings().TryAdd(pawn);
                }
                GenSpawn.Spawn(flyer, origin, map);
                return;
            }
            StartMover(thing, dest, MMTDisplaceMode.Jump, p);
        }

        /// <summary>生成位移 Mover：捕获目标图形信息后退场目标，由 Mover 携带飞行。</summary>
        private static Thing_MMTDisplaceMover StartMover(Thing thing, IntVec3 dest, MMTDisplaceMode mode, MMTDisplaceParams p)
        {
            Map map = thing?.Map;
            if (thing == null || thing.Destroyed || !thing.Spawned || !CanLand(dest, map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(MoverDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + MoverDefName + " 定义，无法执行位移");
                return null;
            }
            IntVec3 origin = thing.Position;
            if (p != null)
            {
                PlayFleck(map, origin, p.departFleckDef);
            }
            Thing_MMTDisplaceMover mover = (Thing_MMTDisplaceMover)GenSpawn.Spawn(def, origin, map);
            mover.Setup(thing, origin, dest, mode, p);
            return mover;
        }
    }

    /// <summary>
    /// 位移载体：携带被位移目标（pawn 用 renderer 实绘，建筑用材质实绘）沿直线平移
    /// （冲锋，贴地 + 拖尾粒子）或抛物线飞行（建筑跳跃，先升后降），到点把目标生成
    /// 到落点。参数运行时传入不进存档；存档瞬间立即落地；载体被异常摧毁时原地放回目标。
    /// </summary>
    public class Thing_MMTDisplaceMover : Thing
    {
        private Thing payload;
        private Pawn payloadPawn;
        private Vector3 startVec;
        private IntVec3 destCell;
        private MMTDisplaceMode mode;
        private int ticksTotal = 1;
        private int ticksPassed;
        private float peakHeight = 3f;
        private FleckDef trailFleck;
        private int trailInterval = 3;
        private FleckDef arriveFleck;
        private SoundDef landingSound;
        private Material payloadMat;
        private Vector2 drawSize = Vector2.one;

        /// <summary>装载目标：捕获图形信息后将目标退场（由本载体代绘）。</summary>
        public void Setup(Thing payloadThing, IntVec3 origin, IntVec3 dest, MMTDisplaceMode m, MMTDisplaceParams p)
        {
            payload = payloadThing;
            payloadPawn = payloadThing as Pawn;
            startVec = origin.ToVector3Shifted();
            destCell = dest;
            mode = m;
            peakHeight = p?.jumpPeakHeight ?? 3f;
            trailFleck = p?.trailFleckDef;
            trailInterval = Mathf.Max(1, p?.trailIntervalTicks ?? 3);
            arriveFleck = p?.arriveFleckDef;
            landingSound = p?.landingSound;
            float dist = (dest - origin).LengthHorizontal;
            ticksTotal = mode == MMTDisplaceMode.Charge
                ? Mathf.Max(p?.chargeMinTicks ?? 6, Mathf.CeilToInt(dist * (p?.chargeTicksPerCell ?? 2f)))
                : Mathf.Max(p?.jumpMinTicks ?? 25, Mathf.CeilToInt(dist * (p?.jumpTicksPerCell ?? 3f)));
            if (payloadPawn == null && payload.Graphic != null)
            {
                payloadMat = payload.Graphic.MatSingle;
                drawSize = payload.Graphic.drawSize;
                if (drawSize.x <= 0f || drawSize.y <= 0f)
                {
                    drawSize = Vector2.one;
                }
            }
            payload.DeSpawn();
        }

        /// <summary>当前地面位置（不含跳跃高度）。</summary>
        private Vector3 GroundPos()
        {
            float t = Mathf.Clamp01(ticksPassed / (float)ticksTotal);
            Vector3 pos = Vector3.Lerp(startVec, destCell.ToVector3Shifted(), t);
            pos.y = AltitudeLayer.Skyfaller.AltitudeFor();
            return pos;
        }

        protected override void Tick()
        {
            base.Tick();
            if (payload == null || payload.Destroyed)
            {
                Destroy();
                return;
            }
            ticksPassed++;
            if (mode == MMTDisplaceMode.Charge && trailFleck != null && ticksPassed % trailInterval == 0)
            {
                Map map = Map;
                if (map == Find.CurrentMap)
                {
                    Vector3 pos = GroundPos();
                    pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    FleckCreationData data = FleckMaker.GetDataStatic(pos, map, trailFleck, Rand.Range(0.45f, 0.7f));
                    data.velocity = new Vector3(Rand.Range(-0.05f, 0.05f), 0f, Rand.Range(0.1f, 0.25f));
                    data.rotationRate = Rand.Range(-30f, 30f);
                    map.flecks.CreateFleck(data);
                }
            }
            if (ticksPassed >= ticksTotal)
            {
                Land();
            }
        }

        /// <summary>落地：目标生成到落点 + 落点闪光/音效，载体自毁。</summary>
        private void Land()
        {
            Map map = Map;
            Thing t = payload;
            payload = null;
            payloadPawn = null;
            if (t != null && !t.Destroyed && map != null)
            {
                GenSpawn.Spawn(t, destCell, map, t.Rotation);
                MMTDisplaceUtil.PlayFleck(map, destCell, arriveFleck);
                if (landingSound != null)
                {
                    SoundStarter.PlayOneShot(landingSound, new TargetInfo(destCell, map));
                }
            }
            Destroy();
        }

        /// <summary>载体被异常摧毁：把目标原地放回，防止目标随载体消失。</summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (payload != null && !payload.Destroyed && Map != null)
            {
                Thing t = payload;
                payload = null;
                payloadPawn = null;
                GenSpawn.Spawn(t, Position, Map, t.Rotation);
            }
            base.Destroy(mode);
        }

        /// <summary>位移途中存档：立即落地，防止退场中的目标不进存档而丢失。</summary>
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && payload != null && !payload.Destroyed && Map != null)
            {
                Thing t = payload;
                payload = null;
                payloadPawn = null;
                GenSpawn.Spawn(t, destCell, Map, t.Rotation);
            }
            base.ExposeData();
        }

        /// <summary>绘制被位移目标：pawn 走 renderer 实绘，建筑走材质实绘；跳跃叠加抛物线高度。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (payload == null || Map != Find.CurrentMap)
            {
                return;
            }
            Vector3 pos = GroundPos();
            if (mode == MMTDisplaceMode.Jump)
            {
                float t = Mathf.Clamp01(ticksPassed / (float)ticksTotal);
                pos.y += peakHeight * 4f * t * (1f - t);
            }
            if (payloadPawn != null)
            {
                payloadPawn.Drawer.renderer.RenderPawnAt(pos, null, false);
                return;
            }
            if (payloadMat != null)
            {
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(drawSize.x, 1f, drawSize.y)), payloadMat, 0);
            }
        }
    }

    // ========== pawn 技能出口：位移（折跃/冲锋/跳跃走目标格；群体传送两段瞄准） ==========

    public class CompProperties_MMTDisplace : CompProperties_AbilityEffect
    {
        /// <summary>位移类型（Blink/MassTeleport/Charge/Jump）。</summary>
        public MMTDisplaceMode mode = MMTDisplaceMode.Blink;
        /// <summary>MassTeleport 圈选半径与敌我限定（沿用第一步框架）。</summary>
        public float radius = 5f;
        public bool targetFriendly = true;
        public bool includeSelf = true;
        /// <summary>MassTeleport 第二段落点射程。</summary>
        public float destRange = 24.9f;
        public float chargeTicksPerCell = 2f;
        public int chargeMinTicks = 6;
        public float jumpTicksPerCell = 3f;
        public int jumpMinTicks = 25;
        public float jumpPeakHeight = 3f;
        public string departFleckDefName;
        public string arriveFleckDefName;
        public string trailFleckDefName;
        public string landingSoundDefName;

        public CompProperties_MMTDisplace()
        {
            compClass = typeof(CompAbilityEffect_MMTDisplace);
        }
    }

    /// <summary>
    /// 位移技能：折跃/冲锋/跳跃直接以目标格为落点；群体传送在 Apply（第一段圈选确认）
    /// 后自动进入第二段落点瞄准，再把范围内有效目标整体传送过去。
    /// </summary>
    public class CompAbilityEffect_MMTDisplace : CompAbilityEffect
    {
        public new CompProperties_MMTDisplace Props => (CompProperties_MMTDisplace)props;

        private MMTDisplaceParams BuildParams()
        {
            return new MMTDisplaceParams
            {
                departFleckDef = Props.departFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.departFleckDefName),
                arriveFleckDef = Props.arriveFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.arriveFleckDefName),
                trailFleckDef = Props.trailFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.trailFleckDefName),
                chargeTicksPerCell = Props.chargeTicksPerCell,
                chargeMinTicks = Props.chargeMinTicks,
                jumpTicksPerCell = Props.jumpTicksPerCell,
                jumpMinTicks = Props.jumpMinTicks,
                jumpPeakHeight = Props.jumpPeakHeight,
                landingSound = Props.landingSoundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.landingSoundDefName)
            };
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTDisplaceParams p = BuildParams();
            switch (Props.mode)
            {
                case MMTDisplaceMode.Blink:
                    MMTDisplaceUtil.Blink(caster, target.Cell, p);
                    break;
                case MMTDisplaceMode.Charge:
                    MMTDisplaceUtil.StartCharge(caster, target.Cell, p);
                    break;
                case MMTDisplaceMode.Jump:
                    MMTDisplaceUtil.StartJump(caster, target.Cell, p);
                    break;
                case MMTDisplaceMode.MassTeleport:
                    StartDestTargeting(caster, target.Cell, p);
                    break;
            }
        }

        /// <summary>群体传送第二段：指定落点（画圈提示被圈选范围，确认后整体传送）。</summary>
        private void StartDestTargeting(Pawn caster, IntVec3 fromCenter, MMTDisplaceParams p)
        {
            Map map = caster.Map;
            if (map == null)
            {
                return;
            }
            TargetingParameters parms = new TargetingParameters
            {
                canTargetLocations = true,
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetSelf = false
            };
            Find.Targeter.BeginTargeting(parms,
                delegate(LocalTargetInfo t)
                {
                    int n = MMTDisplaceUtil.MassTeleport(caster, fromCenter, Props.radius, Props.targetFriendly, Props.includeSelf, t.Cell, caster.Map, p);
                    Messages.Message("已传送 " + n + " 个目标", new LookTargets(t.Cell, caster.Map), MessageTypeDefOf.NeutralEvent, false);
                },
                highlightAction: delegate(LocalTargetInfo t)
                {
                    MMTAoETargetPreview.DrawCircle(fromCenter, Props.radius, MMTAoETargetPreview.RingColor(Props.targetFriendly));
                    if (t.IsValid)
                    {
                        GenDraw.DrawTargetHighlightWithLayer(t.Cell.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
                    }
                },
                targetValidator: delegate(LocalTargetInfo t)
                {
                    return t.IsValid && t.Cell.InBounds(map) && t.Cell.DistanceTo(caster.Position) <= Props.destRange;
                });
        }
    }
}
