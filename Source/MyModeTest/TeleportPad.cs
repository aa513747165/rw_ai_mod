using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 折跃传送台参数：单次传送耗电量与冷却时长（XML 可配置）。
    /// </summary>
    public class CompProperties_TeleportPad : CompProperties
    {
        /// <summary>单次传送（去程/回程各算一次）消耗的内部蓄电（Wd）。</summary>
        public float energyCostWd = 15000f;

        /// <summary>传送后冷却 tick（30000 = 12 游戏小时）。</summary>
        public int cooldownTicks = 30000;

        /// <summary>开启并通电时的蓄电速度（Wd/天；默认 3000 ≈ 待机功耗，5 天充满）。</summary>
        public float chargeRateWdPerDay = 3000f;

        public CompProperties_TeleportPad()
        {
            compClass = typeof(CompTeleportPad);
        }
    }

    /// <summary>
    /// 折跃传送台核心 Comp：去程（装载复用原版 CompTransporter，世界地图选目标后瞬间空降目标地图）
    /// 与回程（供召回 Designator 调用，把框选的己方人员和物品传回本建筑）。
    /// 装载/取消装载 gizmo 由原版 CompTransporter 自带（1.7 已确认），本 Comp 只提供"启动传送"。
    /// </summary>
    public class CompTeleportPad : ThingComp
    {
        /// <summary>冷却结束的游戏 tick（存档持久化，默认 0 兼容旧存档）。</summary>
        private int cooldownUntilTick;

        /// <summary>内部蓄电（Wd）：开启并通电时自动累积，传送时消耗（存档持久化）。</summary>
        private float storedChargeWd;

        private CompProperties_TeleportPad Props
        {
            get { return (CompProperties_TeleportPad)props; }
        }

        private CompTransporter Transporter
        {
            get { return parent.GetComp<CompTransporter>(); }
        }

        private CompPowerTrader Power
        {
            get { return parent.GetComp<CompPowerTrader>(); }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownUntilTick, "cooldownUntilTick");
            Scribe_Values.Look(ref storedChargeWd, "storedChargeWd");
        }

        /// <summary>开启并通电时按 chargeRateWdPerDay 自动蓄电，充满 energyCostWd 即止。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (storedChargeWd >= Props.energyCostWd)
                return;
            if (!IsCharging())
                return;
            storedChargeWd = Mathf.Min(Props.energyCostWd, storedChargeWd + Props.chargeRateWdPerDay / 60000f);
        }

        /// <summary>当前是否正在蓄电：开关打开且电网能供应待机功耗。</summary>
        public bool IsCharging()
        {
            CompFlickable flick = parent.GetComp<CompFlickable>();
            if (flick != null && !flick.SwitchIsOn)
                return false;
            CompPowerTrader power = Power;
            return power != null && power.PowerOn;
        }

        public bool OnCooldown
        {
            get { return Find.TickManager.TicksGame < cooldownUntilTick; }
        }

        /// <summary>传送台当前内部蓄电（Wd）。</summary>
        public float StoredCharge()
        {
            return storedChargeWd;
        }

        /// <summary>能否执行一次传送；不可用时给出中文原因（供 gizmo 禁用提示与召回校验）。
        /// 只要求内部蓄电充足：蓄满后即使电网暂时断电也可发射。</summary>
        public bool CanTeleportNow(out string reason)
        {
            if (!parent.Spawned)
            {
                reason = "传送台不存在";
                return false;
            }
            CompFlickable flick = parent.GetComp<CompFlickable>();
            if (flick != null && !flick.SwitchIsOn)
            {
                reason = "传送台未开启";
                return false;
            }
            if (OnCooldown)
            {
                reason = "传送冷却中（剩余 " + (cooldownUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod() + "）";
                return false;
            }
            if (storedChargeWd < Props.energyCostWd)
            {
                reason = "蓄电不足（" + storedChargeWd.ToString("F0") + "/" + Props.energyCostWd.ToString("F0")
                    + " Wd；保持开启并通电即可自动蓄电）";
                return false;
            }
            reason = null;
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            // 启动传送：舱内为空、装载未完成、电力/冷却不满足时禁用并提示原因
            Command_Action launch = new Command_Action();
            launch.defaultLabel = "启动传送";
            launch.defaultDesc = "将舱内的人员和物品瞬间传送到世界地图上指定的已有地图目标（敌对据点/任务地点/己方基地）。消耗内部蓄电并进入冷却。";
            launch.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", false);
            launch.action = StartChoosingDestination;
            if (!CanTeleportNow(out string reason))
                launch.Disable(reason);
            else if (Transporter == null || Transporter.innerContainer.Count == 0)
                launch.Disable("舱内为空，请先装载人员和物品");
            else if (Transporter.LoadingInProgressOrReadyToLaunch && Transporter.AnyInGroupHasAnythingLeftToLoad)
                // 1.7 的 LoadingInProgressOrReadyToLaunch 在装载组存在期间恒为 true（含"已装满待发"），
                // 用它拦截会导致装载完成后永远无法发射；只在仍有物资待搬入时禁用
                launch.Disable("装载未完成，还有物资在搬运途中");
            yield return launch;

            // 开发者模式：一键充满蓄电并清除冷却（测试传送链路用）
            if (Prefs.DevMode)
            {
                Command_Action devCharge = new Command_Action();
                devCharge.defaultLabel = "DEV: 立即充满蓄电";
                devCharge.defaultDesc = "开发者模式：立即充满内部蓄电并清除传送冷却。";
                devCharge.icon = ContentFinder<Texture2D>.Get("Things/Building/Power/Battery", false);
                devCharge.action = delegate
                {
                    storedChargeWd = Props.energyCostWd;
                    cooldownUntilTick = 0;
                };
                yield return devCharge;
            }
        }

        // ---- 去程：世界地图选目标 ----

        /// <summary>流程开始：跳世界地图并进入选目标模式（仅限已有地图的格子）。</summary>
        private void StartChoosingDestination()
        {
            CameraJumper.TryShowWorld();
            Find.WorldTargeter.BeginTargeting(
                OnWorldTargetChosen,
                canTargetTiles: true,
                mouseAttachment: TexCommand.Attack,
                closeWorldTabWhenFinished: true,
                extraLabelGetter: WorldTargetLabel,
                canSelectTarget: IsValidWorldTarget);
        }

        /// <summary>目标格校验：必须存在已生成地图的 MapParent（据点/任务地点/己方基地），且不是本图。</summary>
        private bool IsValidWorldTarget(GlobalTargetInfo target)
        {
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            return mp != null && mp.HasMap && mp.Map != parent.Map;
        }

        private TaggedString WorldTargetLabel(GlobalTargetInfo target)
        {
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            if (mp == null || !mp.HasMap)
                return "无法传送：该位置没有已存在的地图";
            if (mp.Map == parent.Map)
                return "无法传送：目标就是当前地图";
            return "传送至：" + mp.LabelCap;
        }

        private bool OnWorldTargetChosen(GlobalTargetInfo target)
        {
            if (!IsValidWorldTarget(target))
            {
                Messages.Message("无法传送：该位置没有已存在的地图", MessageTypeDefOf.RejectInput, false);
                return false;
            }
            Map map = Find.WorldObjects.MapParentAt(target.Tile).Map;
            if (map.IsPlayerHome)
            {
                // 己方地图：跳图后精确选落点
                CameraJumper.TryJump(map.Center, map);
                Find.Targeter.BeginTargeting(TargetingParameters.ForCell(),
                    delegate(LocalTargetInfo cell) { ExecuteTeleport(map, cell.Cell); });
                return true;
            }
            // 节点流转：敌对/Site 地图 -> 选择空降方式（边缘 / 中心附近）
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            options.Add(new FloatMenuOption("在地图边缘空降", delegate
            {
                ExecuteTeleport(map, DropCellFinder.RandomDropSpot(map));
            }));
            options.Add(new FloatMenuOption("在地图中心附近空降", delegate
            {
                IntVec3 spot = DropCellFinder.TryFindDropSpotNear(map.Center, map, out IntVec3 found, false, true) ? found : map.Center;
                ExecuteTeleport(map, spot);
            }));
            Find.WindowStack.Add(new FloatMenu(options));
            return true;
        }

        /// <summary>去程执行：抽电、进冷却，把舱内所有人和物瞬间移到目标地图落点。</summary>
        private void ExecuteTeleport(Map targetMap, IntVec3 nearCell)
        {
            if (!CanTeleportNow(out string reason))
            {
                Messages.Message("传送失败：" + reason, parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!TryDrainCharge(Props.energyCostWd))
            {
                Messages.Message("传送失败：蓄电不足", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            cooldownUntilTick = Find.TickManager.TicksGame + Props.cooldownTicks;

            // 本端特效
            TeleportFx.Play(parent.Position, parent.Map);

            IntVec3 landing = FindFreeCellNear(targetMap, nearCell, 12);
            int pawns = 0, things = 0;
            List<Thing> cargo = new List<Thing>(Transporter.innerContainer);
            foreach (Thing t in cargo)
            {
                Transporter.innerContainer.Remove(t);
                if (t is Pawn pawn)
                {
                    // 人员落点散开避免叠格；落图后驱散迷雾并重置状态
                    IntVec3 cell = FindFreeCellNear(targetMap, landing, 8);
                    GenSpawn.Spawn(pawn, cell, targetMap, Rot4.Random);
                    if (pawn.Faction == Faction.OfPlayer && cell.Fogged(targetMap))
                        FloodFillerFog.FloodUnfog(cell, targetMap);
                    pawn.Notify_Teleported();
                    pawns++;
                }
                else
                {
                    GenPlace.TryPlaceThing(t, landing, targetMap, ThingPlaceMode.Near);
                    things++;
                }
            }
            TeleportFx.Play(landing, targetMap);
            // 传送完毕后解散装载组（groupID 复位、残余装载lord移除），
            // 否则传送台一直停留在"待发"状态，无法再开始新一轮装载
            Transporter.CancelLoad(parent.Map);
            Messages.Message("传送完成：" + pawns + " 名人员、" + things + " 件物品已抵达目标地图",
                new LookTargets(new GlobalTargetInfo(landing, targetMap)), MessageTypeDefOf.PositiveEvent, false);
            CameraJumper.TryJump(landing, targetMap);
            // 流程结束
        }

        // ---- 回程：由 Designator_RecallTeleport 调用 ----

        /// <summary>回程执行：把 fromMap 上框选的己方人员/物品传回本建筑落点（整框算一次传送）。</summary>
        public void ExecuteRecall(Map fromMap, List<Thing> targets)
        {
            if (!parent.Spawned)
            {
                Messages.Message("召回失败：传送台已被摧毁", MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!CanTeleportNow(out string reason))
            {
                Messages.Message("召回失败：" + reason, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (targets == null || targets.Count == 0)
            {
                Messages.Message("召回失败：没有可召回的目标", MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!TryDrainCharge(Props.energyCostWd))
            {
                Messages.Message("召回失败：蓄电不足", MessageTypeDefOf.RejectInput, false);
                return;
            }
            cooldownUntilTick = Find.TickManager.TicksGame + Props.cooldownTicks;

            IntVec3 homeCell = FindFreeCellNear(parent.Map, parent.InteractionCell, 12);
            TeleportFx.Play(targets[0].Position, fromMap);

            int pawns = 0, things = 0;
            foreach (Thing t in targets)
            {
                if (!t.Spawned)
                    continue;
                if (t is Pawn pawn)
                {
                    // teleporting 标记抑制离图副作用；重置状态防止卡在旧图 job
                    pawn.teleporting = true;
                    pawn.DeSpawn(DestroyMode.Vanish);
                    IntVec3 cell = FindFreeCellNear(parent.Map, homeCell, 8);
                    GenSpawn.Spawn(pawn, cell, parent.Map, Rot4.Random);
                    pawn.teleporting = false;
                    pawn.Notify_Teleported();
                    pawns++;
                }
                else
                {
                    t.DeSpawn(DestroyMode.Vanish);
                    GenPlace.TryPlaceThing(t, homeCell, parent.Map, ThingPlaceMode.Near);
                    things++;
                }
            }
            TeleportFx.Play(homeCell, parent.Map);
            Messages.Message("召回完成：" + pawns + " 名人员、" + things + " 件物品已传回" + parent.Map.Parent.LabelCap,
                new LookTargets(new GlobalTargetInfo(homeCell, parent.Map)), MessageTypeDefOf.PositiveEvent, false);
            // 流程结束
        }

        // ---- 电力 ----

        /// <summary>消耗内部蓄电；蓄电不足时不扣任何电并返回 false。</summary>
        private bool TryDrainCharge(float amount)
        {
            if (storedChargeWd < amount)
                return false;
            storedChargeWd -= amount;
            return true;
        }

        // ---- 工具 ----

        /// <summary>在中心附近找可站立格，递增半径兜底；找不到退到地图中心。</summary>
        private static IntVec3 FindFreeCellNear(Map map, IntVec3 center, int maxRadius)
        {
            if (!center.IsValid || !center.InBounds(map))
                center = map.Center;
            if (center.Standable(map))
                return center;
            foreach (IntVec3 c in GenRadial.RadialCellsAround(center, maxRadius, true))
                if (c.InBounds(map) && c.Standable(map))
                    return c;
            return map.Center;
        }

        public override string CompInspectStringExtra()
        {
            string text = "蓄电: " + storedChargeWd.ToString("F0") + "/" + Props.energyCostWd.ToString("F0") + " Wd";
            if (storedChargeWd >= Props.energyCostWd)
                text += "（已满）";
            else if (IsCharging())
                text += "（充电中，" + Props.chargeRateWdPerDay.ToString("F0") + " Wd/天）";
            else
                text += "（未充电：需要开启开关并通电）";
            if (OnCooldown)
                text += "\n传送冷却: 剩余 " + (cooldownUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod();
            if (Transporter != null)
                text += "\n舱内质量: " + Transporter.MassUsage.ToString("F0") + "/" + Transporter.MassCapacity.ToString("F0") + " kg";
            return text;
        }
    }

    /// <summary>
    /// 召回传送 Designator：挂在 Architect 菜单（specialDesignatorClasses，零 Harmony patch），
    /// 在任意己方所在地图拖框选范围，把框内己方人员和物品传回一台可用的母基地传送台。
    /// </summary>
    public class Designator_RecallTeleport : Designator_Cells
    {
        public Designator_RecallTeleport()
        {
            defaultLabel = "召回传送";
            defaultDesc = "框选一个范围，将范围内所有己方人员和物品传送回母基地的折跃传送台（消耗一次传送电量与冷却）。";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/MMT_RecallTeleport", false);
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Claim;
        }

        /// <summary>1.7 拖拽形状由 DrawStyleCategory 决定：实心矩形框选。</summary>
        public override DrawStyleCategoryDef DrawStyleCategory
        {
            get { return DefDatabase<DrawStyleCategoryDef>.GetNamed("FilledRectangle"); }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            // 禁迷雾格，防止盲捞
            return c.InBounds(Map) && !c.Fogged(Map);
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            // 1. 扫描所有己方基地中可用的传送台
            List<CompTeleportPad> pads = new List<CompTeleportPad>();
            foreach (Map m in Find.Maps)
            {
                if (!m.IsPlayerHome)
                    continue;
                foreach (Thing t in m.listerThings.ThingsOfDef(MMT_TeleportDefOf.MMT_TeleportPad))
                {
                    CompTeleportPad comp = ((ThingWithComps)t).GetComp<CompTeleportPad>();
                    if (comp != null && comp.parent.Spawned && comp.CanTeleportNow(out _))
                        pads.Add(comp);
                }
            }
            if (pads.Count == 0)
            {
                Messages.Message("没有可用的折跃传送台（需已开启、蓄电充足且不在冷却中）", MessageTypeDefOf.RejectInput, false);
                return;
            }

            // 2. 收集框内己方人员（含倒地/动物，排除尸体与囚犯）与可搬运物品
            HashSet<Thing> targets = new HashSet<Thing>();
            foreach (IntVec3 c in cells)
            {
                if (!CanDesignateCell(c).Accepted)
                    continue;
                List<Thing> things = Map.thingGrid.ThingsListAtFast(c);
                for (int i = 0; i < things.Count; i++)
                {
                    Thing t = things[i];
                    if (t is Pawn p)
                    {
                        if (p.Faction == Faction.OfPlayer && !p.Dead && !p.IsPrisoner)
                            targets.Add(p);
                    }
                    else if (t.def.category == ThingCategory.Item && !(t is Corpse) && t.def.EverHaulable)
                    {
                        targets.Add(t);
                    }
                }
            }
            if (targets.Count == 0)
            {
                Messages.Message("框选范围内没有可召回的己方人员或物品", MessageTypeDefOf.RejectInput, false);
                return;
            }

            // 3. 节点选择：单台直接召回，多台弹菜单选（列出所在基地与状态）
            if (pads.Count == 1)
            {
                pads[0].ExecuteRecall(Map, targets.ToList());
            }
            else
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (CompTeleportPad pad in pads)
                {
                    CompTeleportPad local = pad;
                    options.Add(new FloatMenuOption(
                        pad.parent.Map.Parent.LabelCap + "（蓄电 " + pad.StoredCharge().ToString("F0") + " Wd）",
                        delegate { local.ExecuteRecall(Map, targets.ToList()); }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            FinalizeDesignationSucceeded();
        }
    }

    /// <summary>传送特效：复用 Royalty 折跃灵能的闪现贴图与音效（无 Royalty 时静默跳过）。</summary>
    internal static class TeleportFx
    {
        public static void Play(IntVec3 cell, Map map)
        {
            if (map == null || !cell.IsValid || !cell.InBounds(map))
                return;
            if (FleckDefOf.PsycastSkipFlashEntry != null)
                FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 2f);
            if (FleckDefOf.PsycastSkipInnerExit != null)
                FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipInnerExit, 2f);
            if (SoundDefOf.Psycast_Skip_Exit != null)
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(cell, map));
        }
    }

    [DefOf]
    public static class MMT_TeleportDefOf
    {
        public static ThingDef MMT_TeleportPad;

        static MMT_TeleportDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_TeleportDefOf));
        }
    }
}
