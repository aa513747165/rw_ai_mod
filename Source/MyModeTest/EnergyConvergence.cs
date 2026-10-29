using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>点火仪式状态。</summary>
    public enum EnergyIgnitionState
    {
        /// <summary>未点火：不发电、不耗燃料。</summary>
        NotIgnited,
        /// <summary>点火序列进行中（射线汇聚能量球 → 光柱点火）。</summary>
        Igniting,
        /// <summary>已点火：正常发电。</summary>
        Ignited,
    }

    /// <summary>
    /// 能量聚能发电站点火参数（XML 可配置）：
    /// 点火时长、光柱时长、四角充能装置 defName 与固定偏移、能量球最大尺寸。
    /// </summary>
    public class CompProperties_EnergyIgnition : CompProperties
    {
        /// <summary>能量汇聚阶段时长（tick）。</summary>
        public int ignitionTicks = 600;

        /// <summary>光柱点火阶段时长（tick）。</summary>
        public int beamTicks = 60;

        /// <summary>四角充能装置的 defName。</summary>
        public string chargerDefName = "MMT_IgnitionCharger";

        /// <summary>充能装置相对发电站中心的固定偏移（默认四角对角外一格）。</summary>
        public List<IntVec3> chargerOffsets;

        /// <summary>能量球充满时的绘制尺寸（格）。</summary>
        public float energyBallMaxSize = 2.2f;

        public CompProperties_EnergyIgnition()
        {
            compClass = typeof(CompEnergyIgnition);
        }
    }

    /// <summary>
    /// 聚能发电站点火核心 Comp（MC 式多方块）：
    /// 四角固定偏移处各有通电的点火充能装置时，可通过 gizmo 启动点火仪式；
    /// 仪式完成后才允许发电；燃料耗尽/断电/关开关即熄火，需重新点火。
    /// </summary>
    public class CompEnergyIgnition : ThingComp
    {
        private EnergyIgnitionState state = EnergyIgnitionState.NotIgnited;
        /// <summary>点火序列进度（tick）。</summary>
        private int progressTicks;

        // 结构校验缓存：每 250 tick 重扫一次
        private bool cachedReady;
        private int lastReadyCheckTick = -9999;
        /// <summary>最近一次结构校验未就绪原因（gizmo 提示用）。</summary>
        private string notReadyReason = "";

        // spawn 时缓存的 comp 引用（comps 列表终身不变）
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        private CompRefuelable fuelComp;
        private ThingDef chargerDef;
        /// <summary>上一次飘余烬的真实时间（发电中特效按真实时间走，与倍速无关）。</summary>
        private float lastEmberTime;

        private CompProperties_EnergyIgnition Props
        {
            get { return (CompProperties_EnergyIgnition)props; }
        }

        public EnergyIgnitionState State
        {
            get { return state; }
        }

        /// <summary>已点火（发电输出以此为准）。</summary>
        public bool IsIgnited
        {
            get { return state == EnergyIgnitionState.Ignited; }
        }

        /// <summary>点火汇聚进度 0~1（能量球尺寸/亮度用）。</summary>
        public float ConvergeProgress
        {
            get
            {
                if (state != EnergyIgnitionState.Igniting)
                    return 0f;
                return Mathf.Clamp01((float)progressTicks / Props.ignitionTicks);
            }
        }

        /// <summary>是否处于光柱点火阶段。</summary>
        public bool InBeamPhase
        {
            get { return state == EnergyIgnitionState.Igniting && progressTicks >= Props.ignitionTicks; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            flickComp = parent.GetComp<CompFlickable>();
            fuelComp = parent.GetComp<CompRefuelable>();
            chargerDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.chargerDefName);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref state, "energyIgnitionState", EnergyIgnitionState.NotIgnited);
            Scribe_Values.Look(ref progressTicks, "energyIgnitionProgress", 0);
        }

        /// <summary>四角充能装置就位且通电。</summary>
        public bool IgnitersReady
        {
            get
            {
                // 250 tick 重扫一次，避免每次 gizmo 刷新都扫网格
                if (Find.TickManager.TicksGame - lastReadyCheckTick >= 250)
                {
                    lastReadyCheckTick = Find.TickManager.TicksGame;
                    cachedReady = CheckStructure(out notReadyReason);
                }
                return cachedReady;
            }
        }

        /// <summary>按固定偏移逐格校验充能装置；未就绪时输出中文原因。</summary>
        private bool CheckStructure(out string reason)
        {
            reason = null;
            if (chargerDef == null || Props.chargerOffsets == null || Props.chargerOffsets.Count == 0)
            {
                reason = "配置错误：缺少充能装置定义";
                return false;
            }
            Map map = parent.Map;
            if (map == null)
            {
                reason = "";
                return false;
            }
            for (int i = 0; i < Props.chargerOffsets.Count; i++)
            {
                IntVec3 cell = parent.Position + Props.chargerOffsets[i];
                if (!cell.InBounds(map))
                {
                    reason = "第 " + (i + 1) + " 角超出地图边界";
                    return false;
                }
                Building charger = cell.GetFirstThing<Building>(map);
                if (charger == null || charger.def != chargerDef)
                {
                    reason = "第 " + (i + 1) + " 角缺少点火充能装置";
                    return false;
                }
                CompPowerTrader cp = charger.GetComp<CompPowerTrader>();
                CompFlickable cf = charger.GetComp<CompFlickable>();
                if (cf != null && !cf.SwitchIsOn)
                {
                    reason = "第 " + (i + 1) + " 角的充能装置未开启";
                    return false;
                }
                if (cp != null && !cp.PowerOn)
                {
                    reason = "第 " + (i + 1) + " 角的充能装置未通电";
                    return false;
                }
            }
            return true;
        }

        /// <summary>四角充能装置当前实例（点火序列与绘制用；含缺角则返回 null）。</summary>
        public List<Building> GetChargers()
        {
            if (chargerDef == null || Props.chargerOffsets == null || parent.Map == null)
                return null;
            List<Building> list = new List<Building>(Props.chargerOffsets.Count);
            foreach (IntVec3 off in Props.chargerOffsets)
            {
                Building b = (parent.Position + off).GetFirstThing<Building>(parent.Map);
                if (b == null || b.def != chargerDef)
                    return null;
                list.Add(b);
            }
            return list;
        }

        /// <summary>点火序列推进与熄火监听（每 tick）。</summary>
        public override void CompTick()
        {
            base.CompTick();
            switch (state)
            {
                case EnergyIgnitionState.Igniting:
                    TickIgniting();
                    break;
                case EnergyIgnitionState.Ignited:
                    // 熄火判定放低频率即可
                    if (parent.IsHashIntervalTick(60))
                        CheckFlameout();
                    SpawnBurningEffects();
                    break;
            }
        }

        private void TickIgniting()
        {
            // 序列中任一装置缺失/断电/关闭 → 中止
            List<Building> chargers = GetChargers();
            if (chargers == null || !PowerSatisfied(chargers))
            {
                AbortIgnition();
                return;
            }
            progressTicks++;
            SpawnBurningEffects();
            if (progressTicks >= Props.ignitionTicks + Props.beamTicks)
                CompleteIgnition();
        }

        /// <summary>
        /// 点火中的动态燃烧火球：火焰 fleck 在站体上持续升腾，密度随汇聚进度增加；
        /// 已点火后偶尔飘余烬，表示核心仍在燃烧。
        /// </summary>
        private void SpawnBurningEffects()
        {
            Map map = parent.Map;
            if (map == null)
                return;
            if (state == EnergyIgnitionState.Igniting)
            {
                if (!parent.IsHashIntervalTick(3))
                    return;
                float prog = InBeamPhase ? 1f : ConvergeProgress;
                int bursts = 1 + Mathf.RoundToInt(prog * 2f);
                for (int i = 0; i < bursts; i++)
                {
                    Vector3 pos = parent.TrueCenter() + new Vector3(Rand.Range(-0.8f, 0.8f), 0f, Rand.Range(-0.8f, 0.8f));
                    FleckMaker.Static(pos, map, FleckDefOf.FireGlow, Rand.Range(2.2f, 3.2f) * (0.6f + 0.5f * prog));
                }
                if (Rand.Value < 0.4f)
                    FleckMaker.Static(parent.TrueCenter(), map, FleckDefOf.Smoke, Rand.Range(1.2f, 1.8f));
            }
            else if (state == EnergyIgnitionState.Ignited)
            {
                // 余烬按真实时间间隔飘：hash tick 会随游戏倍速加速，倍速下火星密度暴涨
                float now = Time.realtimeSinceStartup;
                if (now - lastEmberTime >= 0.9f)
                {
                    lastEmberTime = now;
                    FleckMaker.Static(parent.TrueCenter(), map, FleckDefOf.FireGlow, Rand.Range(1.5f, 2.2f));
                }
            }
        }

        /// <summary>所有充能装置通电且开启（本电站自身也需通电——点火要从电网汲取能量）。</summary>
        private bool PowerSatisfied(List<Building> chargers)
        {
            if (powerComp != null && !powerComp.PowerOn)
                return false;
            if (flickComp != null && !flickComp.SwitchIsOn)
                return false;
            foreach (Building c in chargers)
            {
                CompPowerTrader cp = c.GetComp<CompPowerTrader>();
                CompFlickable cf = c.GetComp<CompFlickable>();
                if ((cp != null && !cp.PowerOn) || (cf != null && !cf.SwitchIsOn))
                    return false;
            }
            return true;
        }

        private void CompleteIgnition()
        {
            state = EnergyIgnitionState.Ignited;
            progressTicks = 0;
            Messages.Message("点火成功：" + parent.LabelCap + " 已开始发电", new LookTargets(parent),
                MessageTypeDefOf.PositiveEvent, false);
        }

        private void AbortIgnition()
        {
            state = EnergyIgnitionState.NotIgnited;
            progressTicks = 0;
            Messages.Message("点火中止：" + parent.LabelCap + " 的充能装置断电或缺失", new LookTargets(parent),
                MessageTypeDefOf.NegativeEvent, false);
        }

        /// <summary>熄火：燃料耗尽 / 断电 / 关开关 → 回到未点火。</summary>
        private void CheckFlameout()
        {
            bool flameout = false;
            string why = null;
            if (fuelComp != null && !fuelComp.HasFuel)
            {
                flameout = true;
                why = "能量结晶耗尽";
            }
            else if (powerComp != null && !powerComp.PowerOn)
            {
                flameout = true;
                why = "电网断电";
            }
            else if (flickComp != null && !flickComp.SwitchIsOn)
            {
                flameout = true;
                why = "被关闭";
            }
            if (!flameout)
                return;
            state = EnergyIgnitionState.NotIgnited;
            progressTicks = 0;
            Messages.Message(parent.LabelCap + " 已熄火（" + why + "），需重新点火", new LookTargets(parent),
                MessageTypeDefOf.NegativeEvent, false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (state == EnergyIgnitionState.NotIgnited)
            {
                Command_Action cmd = new Command_Action();
                cmd.defaultLabel = "开始点火";
                cmd.icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MMT_EnergyBall", false);
                if (IgnitersReady)
                {
                    cmd.defaultDesc = "启动点火仪式：四角充能装置将能量汇聚于发电站上方，点燃聚能核心。";
                    cmd.action = StartIgnition;
                }
                else
                {
                    cmd.defaultDesc = "无法点火：" + notReadyReason + "（需要在发电站四角各放置一座点火充能装置并通电）";
                    cmd.Disable();
                }
                yield return cmd;
            }
        }

        private void StartIgnition()
        {
            if (state != EnergyIgnitionState.NotIgnited || !IgnitersReady)
                return;
            state = EnergyIgnitionState.Igniting;
            progressTicks = 0;
            Messages.Message("点火仪式开始：" + parent.LabelCap, new LookTargets(parent),
                MessageTypeDefOf.NeutralEvent, false);
        }

        public override string CompInspectStringExtra()
        {
            switch (state)
            {
                case EnergyIgnitionState.NotIgnited:
                    if (IgnitersReady)
                        return "状态: 未点火（结构就绪，可启动点火仪式）";
                    return "状态: 未点火（" + notReadyReason + "）";
                case EnergyIgnitionState.Igniting:
                    if (InBeamPhase)
                        return "状态: 点火中——能量释放";
                    return "状态: 点火中——能量汇聚 " + ConvergeProgress.ToStringPercent("F0");
                default:
                    return "状态: 已点火";
            }
        }
    }

    /// <summary>
    /// 受点火状态控制的电站输出：未点火时强制零输出。
    /// 用子类接管而非外部强设，避免与 CompPowerPlant 每 tick 的覆写打架。
    /// </summary>
    public class CompPowerPlantIgnitable : CompPowerPlant
    {
        private CompEnergyIgnition ignition;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ignition = parent.GetComp<CompEnergyIgnition>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ignition != null && !ignition.IsIgnited)
                PowerOutput = 0f;
        }
    }

    /// <summary>
    /// 点火状态联动的燃料消耗：未点火时结晶不燃烧（否则建好就白白耗燃料）。
    /// </summary>
    public class CompRefuelableIgnitable : CompRefuelable
    {
        private CompEnergyIgnition ignition;
        private bool lookedUp;

        public override void CompTick()
        {
            if (!lookedUp)
            {
                lookedUp = true;
                ignition = parent.GetComp<CompEnergyIgnition>();
            }
            if (ignition != null && !ignition.IsIgnited)
                return;
            base.CompTick();
        }
    }

    /// <summary>
    /// 点火充能装置放置限制：只允许放在聚能发电站的四个角（对角外一格），
    /// 其余位置拒绝放置/安装；拖动幽灵时高亮目标发电站的四角与连线。
    /// 发电站本体、蓝图、框架都算作有效目标，方便先摆蓝图规划。
    /// </summary>
    public class PlaceWorker_IgnitionCharger : PlaceWorker
    {
        /// <summary>充能装置 defName → 使用它的(发电站 def, 点火参数)，懒加载缓存。</summary>
        private static List<KeyValuePair<ThingDef, CompProperties_EnergyIgnition>> plantTable;

        private static void EnsureTable()
        {
            if (plantTable != null)
                return;
            plantTable = new List<KeyValuePair<ThingDef, CompProperties_EnergyIgnition>>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                CompProperties_EnergyIgnition props = def.GetCompProperties<CompProperties_EnergyIgnition>();
                if (props != null && props.chargerOffsets != null && props.chargerOffsets.Count > 0)
                    plantTable.Add(new KeyValuePair<ThingDef, CompProperties_EnergyIgnition>(def, props));
            }
        }

        /// <summary>loc 若是某聚能发电站（含蓝图/框架）的四角之一，返回该发电站实例，否则 null。</summary>
        public static Thing FindPlantForCell(ThingDef chargerDef, IntVec3 loc, Map map, out CompProperties_EnergyIgnition props)
        {
            props = null;
            if (map == null)
                return null;
            EnsureTable();
            for (int i = 0; i < plantTable.Count; i++)
            {
                ThingDef plantDef = plantTable[i].Key;
                CompProperties_EnergyIgnition p = plantTable[i].Value;
                if (p.chargerDefName != chargerDef.defName)
                    continue;
                for (int j = 0; j < p.chargerOffsets.Count; j++)
                {
                    IntVec3 center = loc - p.chargerOffsets[j];
                    if (!center.InBounds(map))
                        continue;
                    List<Thing> things = center.GetThingList(map);
                    for (int k = 0; k < things.Count; k++)
                    {
                        Thing t = things[k];
                        // 建成本体 / 建造框架 / 蓝图都算
                        if (t.def == plantDef || t.def.entityDefToBuild == plantDef)
                        {
                            props = p;
                            return t;
                        }
                    }
                }
            }
            return null;
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (FindPlantForCell((ThingDef)checkingDef, loc, map, out _) == null)
                return "点火充能装置只能安装在能量聚能发电站的四个角（对角外一格）";
            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing ghostThing = null)
        {
            Map map = Find.CurrentMap;
            Thing plant = FindPlantForCell(def, center, map, out CompProperties_EnergyIgnition props);
            if (plant == null || props == null)
                return;
            // 高亮发电站四角，并画出各角到发电站中心的能量连线预览
            GenDraw.DrawFieldEdges(props.chargerOffsets.Select(o => plant.Position + o).ToList(), Color.cyan);
            Vector3 plantCenter = plant.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            foreach (IntVec3 off in props.chargerOffsets)
            {
                Vector3 corner = (plant.Position + off).ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                GenDraw.DrawLineBetween(corner, plantCenter, SimpleColor.Cyan);
            }
        }
    }

    /// <summary>点火充能装置参数（XML 可配置）。</summary>
    public class CompProperties_IgnitionCharger : CompProperties
    {
        /// <summary>待机功耗（W）。</summary>
        public float idlePowerW = 50f;

        /// <summary>点火序列中的功耗（W）。</summary>
        public float ignitingPowerW = 5000f;

        /// <summary>搜索关联发电站的半径。</summary>
        public float linkRadius = 3.5f;

        public CompProperties_IgnitionCharger()
        {
            compClass = typeof(CompIgnitionCharger);
        }
    }

    /// <summary>
    /// 点火充能装置 Comp：扫描邻近处于点火序列中的发电站，
    /// 有序列则切换高功耗（能量经本装置汇聚），否则待机低功耗。与发电站解耦。
    /// </summary>
    public class CompIgnitionCharger : ThingComp
    {
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        /// <summary>当前关联的点火序列（可空）。</summary>
        private CompEnergyIgnition linkedSequence;

        private CompProperties_IgnitionCharger Props
        {
            get { return (CompProperties_IgnitionCharger)props; }
        }

        public bool IsFiring
        {
            get { return linkedSequence != null; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            flickComp = parent.GetComp<CompFlickable>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (powerComp == null)
                return;
            if (flickComp != null && !flickComp.SwitchIsOn)
            {
                linkedSequence = null;
                powerComp.PowerOutput = 0f;
                return;
            }
            // 30 tick 重扫一次关联序列
            if (parent.IsHashIntervalTick(30))
                RefreshLink();
            powerComp.PowerOutput = linkedSequence != null ? -Props.ignitingPowerW : -Props.idlePowerW;
        }

        /// <summary>扫描半径内处于点火序列且把本装置算作其四角的发电站。</summary>
        private void RefreshLink()
        {
            linkedSequence = null;
            Map map = parent.Map;
            if (map == null)
                return;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(parent.Position, Props.linkRadius, true))
            {
                if (!cell.InBounds(map))
                    continue;
                Building b = cell.GetFirstThing<Building>(map);
                if (b == null)
                    continue;
                CompEnergyIgnition seq = b.GetComp<CompEnergyIgnition>();
                if (seq == null || seq.State != EnergyIgnitionState.Igniting)
                    continue;
                // 只关联确实把本装置当四角的序列
                List<Building> corners = seq.GetChargers();
                if (corners != null && corners.Contains((Building)parent))
                {
                    linkedSequence = seq;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 能量聚能发电站建筑：绘制点火仪式特效。
    /// 注意 Waylith 下竖直 quad 不渲染（ChargingStation 已验证），全部特效用水平 quad：
    /// 射线画在水平面内俯视投影即为连线；光柱用多片水平碟堆叠；能量球为多层水平圆碟错相位缩放。
    /// </summary>
    public class Building_EnergyConvergencePlant : Building
    {
        private const string BeamTexPath = "Things/Building/Misc/MMT_ChargeBeam";
        private const string BallTexPath = "Things/Building/Misc/MMT_EnergyBall";
        /// <summary>点火后（燃烧态）贴图。</summary>
        private const string LitTexPath = "Things/Building/Power/MMT_EnergyConvergencePlant_Lit";

        private CompEnergyIgnition ignition;
        private Material beamMat;
        private Material ballMat;
        private Graphic litGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ignition = this.TryGetComp<CompEnergyIgnition>();
        }

        /// <summary>未点火用普通贴图；已点火切换为熔金燃烧态贴图（遮罩同名 _m 自动加载）。</summary>
        public override Graphic Graphic
        {
            get
            {
                if (ignition != null && ignition.IsIgnited)
                {
                    if (litGraphic == null)
                        litGraphic = GraphicDatabase.Get<Graphic_Single>(LitTexPath, base.Graphic.Shader,
                            def.graphicData.drawSize, DrawColor, DrawColorTwo);
                    return litGraphic;
                }
                return base.Graphic;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (ignition == null)
                return;
            if (ignition.State == EnergyIgnitionState.Ignited)
            {
                DrawBurningGlow(drawLoc);
                return;
            }
            if (ignition.State != EnergyIgnitionState.Igniting)
                return;
            // 全程画四角射线（顶点→发电站上方）；汇聚阶段加能量球，释放阶段加光柱
            DrawCornerBeams(drawLoc);
            if (ignition.InBeamPhase)
                DrawIgnitionBeam(drawLoc);
            else
                DrawConvergence(drawLoc);
        }

        /// <summary>
        /// 发电中：核心上方一层暖色脉动光晕。真实时间驱动，游戏倍速不影响呼吸节奏。
        /// </summary>
        private void DrawBurningGlow(Vector3 drawLoc)
        {
            if (ballMat == null)
                ballMat = MaterialPool.MatFrom(BallTexPath, ShaderDatabase.Transparent, Color.white);
            float time = Time.realtimeSinceStartup;
            float s = 1.8f + 0.2f * Mathf.Sin(time * 2.3f);
            ballMat.color = new Color(1f, 0.72f, 0.38f, 0.10f + 0.07f * Mathf.Sin(time * 3.7f));
            Vector3 p = new Vector3(drawLoc.x, AltitudeLayer.MoteOverhead.AltitudeFor(), drawLoc.z);
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(p, Quaternion.identity, new Vector3(s, 1f, s)), ballMat, 0);
        }

        /// <summary>四角充能装置顶点到发电站上方的能量射线（点火全程持续）。</summary>
        private void DrawCornerBeams(Vector3 drawLoc)
        {
            List<Building> chargers = ignition.GetChargers();
            if (chargers == null)
                return;
            if (beamMat == null)
                beamMat = MaterialPool.MatFrom(BeamTexPath, ShaderDatabase.Transparent, Color.white);
            float progress = ignition.ConvergeProgress;
            // 真实时间驱动脉动：游戏倍速下 TicksGame 加速会导致动画鬼畜
            float time = Time.realtimeSinceStartup;
            float pulse = 0.55f + 0.35f * Mathf.Sin(time * 6f);
            beamMat.color = new Color(0.65f, 0.9f, 1f, pulse);
            float y = AltitudeLayer.MoteOverhead.AltitudeFor();
            foreach (Building charger in chargers)
            {
                Vector3 from = charger.DrawPos;
                from.y = y;
                Vector3 to = new Vector3(drawLoc.x, y, drawLoc.z);
                DrawBeamQuad(from, to, 0.22f + 0.1f * progress);
            }
        }

        /// <summary>能量汇聚：中央逐渐增大的太阳状能量球。</summary>
        private void DrawConvergence(Vector3 drawLoc)
        {
            float progress = ignition.ConvergeProgress;
            float y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float time = Time.realtimeSinceStartup;
            // 能量球：3 片错相位缩放的圆碟，俯视下呈球体感
            if (ballMat == null)
                ballMat = MaterialPool.MatFrom(BallTexPath, ShaderDatabase.Transparent, Color.white);
            float maxSize = ignition == null ? 2f : ((CompProperties_EnergyIgnition)ignition.props).energyBallMaxSize;
            for (int i = 0; i < 3; i++)
            {
                float phase = time * 2.2f + i * 2.1f;
                float s = maxSize * progress * (0.9f + 0.1f * Mathf.Sin(phase)) * (1f - i * 0.18f);
                if (s <= 0.01f)
                    continue;
                ballMat.color = new Color(1f, 1f, 1f, (0.35f + 0.5f * progress) * (1f - i * 0.25f));
                Vector3 p = new Vector3(drawLoc.x, y + 0.05f * i, drawLoc.z);
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(p, Quaternion.identity, new Vector3(s, 1f, s)), ballMat, 0);
            }
        }

        /// <summary>
        /// 从 from 到 to 画一条水平面内的射线（拉伸 quad）。
        /// 注意 MeshPool.plane10 是 1×1 平面（名字是历史遗留，见 MeshPool.cctor 的 NewPlaneMesh(1f)），
        /// 所以长度方向缩放就是格数本身，不要再 /10（否则射线缩成一截几乎不可见的短线）。
        /// </summary>
        private void DrawBeamQuad(Vector3 from, Vector3 to, float width)
        {
            Vector3 mid = (from + to) * 0.5f;
            Vector3 delta = to - from;
            float len = delta.magnitude;
            if (len < 0.01f)
                return;
            float angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(mid, Quaternion.Euler(0f, angle, 0f), new Vector3(width, 1f, len)), beamMat, 0);
        }

        /// <summary>光柱点火：6 片水平发光碟从站体堆叠到空中（俯视投影为光柱），相位错开的正弦缩放模拟能量涌动。</summary>
        private void DrawIgnitionBeam(Vector3 drawLoc)
        {
            if (beamMat == null)
                beamMat = MaterialPool.MatFrom(BeamTexPath, ShaderDatabase.Transparent, Color.white);
            float time = Time.realtimeSinceStartup;
            beamMat.color = new Color(1f, 0.9f, 0.6f, 0.5f + 0.25f * Mathf.Sin(time * 8f));
            float baseY = AltitudeLayer.MoteOverhead.AltitudeFor() - 0.5f;
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float s = (1.6f - t * 0.6f) * (1f + 0.15f * Mathf.Sin(time * 7f + i * 1.1f));
                Vector3 p = new Vector3(drawLoc.x, baseY + i * 0.22f, drawLoc.z);
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(p, Quaternion.identity, new Vector3(s, 1f, s)), beamMat, 0);
            }
        }
    }
}
