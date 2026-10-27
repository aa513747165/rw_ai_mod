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

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompTransporter transporterComp;
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;

        private CompProperties_TeleportPad Props
        {
            get { return (CompProperties_TeleportPad)props; }
        }

        private CompTransporter Transporter
        {
            get { return transporterComp; }
        }

        private CompPowerTrader Power
        {
            get { return powerComp; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            transporterComp = parent.GetComp<CompTransporter>();
            powerComp = parent.GetComp<CompPowerTrader>();
            flickComp = parent.GetComp<CompFlickable>();
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
            CompFlickable flick = flickComp;
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
            CompFlickable flick = flickComp;
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
            launch.defaultDesc = "将舱内的人员和物品瞬间传送到世界地图上任意地块：敌对据点=进攻空降；非敌对立据点=远程交易或进攻；空白地块=生成荒野临时地图（人员全部撤离后自动销毁，遗留物品随图消失）；己方基地=精确选落点。消耗内部蓄电并进入冷却（远程交易不消耗）。";
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

        /// <summary>流程开始：跳世界地图并进入选目标模式（任意非水域地块可选）。</summary>
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

        /// <summary>目标格校验：拒绝水域与当前地图所在格；其余（据点/任务地点/空白地块）均可。</summary>
        private bool IsValidWorldTarget(GlobalTargetInfo target)
        {
            if (Find.WorldGrid[target.Tile].WaterCovered)
                return false;
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            return mp == null || !mp.HasMap || mp.Map != parent.Map;
        }

        /// <summary>目标器悬停文案：按分支提示（己方/交易或进攻/进攻/荒野/拒绝原因）。</summary>
        private TaggedString WorldTargetLabel(GlobalTargetInfo target)
        {
            if (Find.WorldGrid[target.Tile].WaterCovered)
                return "无法传送：目标为水域";
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            if (mp == null)
                return "荒野空降：" + Find.WorldGrid[target.Tile].PrimaryBiome.LabelCap + "（生成临时地图）";
            if (mp.HasMap && mp.Map == parent.Map)
                return "无法传送：目标就是当前地图";
            if (mp.HasMap && mp.Map.IsPlayerHome)
                return "传送至：" + mp.LabelCap;
            if (mp.Faction != null && !mp.Faction.HostileTo(Faction.OfPlayer) && mp is Settlement)
                return "交易或进攻：" + mp.LabelCap;
            return "进攻：" + mp.LabelCap;
        }

        /// <summary>节点流转：按目标类型分派——己方精确选点 / 非敌对交易或进攻 / 敌对进攻 / 荒野临时地图。</summary>
        private bool OnWorldTargetChosen(GlobalTargetInfo target)
        {
            if (!IsValidWorldTarget(target))
            {
                Messages.Message("无法传送：目标为水域或当前地图", MessageTypeDefOf.RejectInput, false);
                return false;
            }
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            if (mp != null && mp.HasMap && mp.Map.IsPlayerHome)
            {
                // 己方地图：跳图后精确选落点
                Map homeMap = mp.Map;
                CameraJumper.TryJump(homeMap.Center, homeMap);
                Find.Targeter.BeginTargeting(TargetingParameters.ForCell(),
                    delegate(LocalTargetInfo cell) { ExecuteTeleport(homeMap, cell.Cell); });
                return true;
            }
            if (mp is Settlement settlement && mp.Faction != null && !mp.Faction.HostileTo(Faction.OfPlayer))
            {
                // 非敌对立据点：远程交易（不传送不生成地图）或进攻（生成地图空降）
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                options.Add(new FloatMenuOption("交易（远程开启交易窗口，不传送）",
                    delegate { OpenTradeWith(settlement); }));
                options.Add(new FloatMenuOption("进攻：在地图边缘空降",
                    delegate { AttackMapParent(mp, target.Tile, false); }));
                options.Add(new FloatMenuOption("进攻：在地图中心附近空降",
                    delegate { AttackMapParent(mp, target.Tile, true); }));
                Find.WindowStack.Add(new FloatMenu(options));
                return true;
            }
            if (mp != null)
            {
                // 敌对据点 / 无派系任务地点：进攻（地图按需生成）
                ShowAttackOptions(mp, target.Tile);
                return true;
            }
            // 空白地块：确认后生成荒野临时地图
            List<FloatMenuOption> wild = new List<FloatMenuOption>();
            wild.Add(new FloatMenuOption("空降荒野（生成临时地图，人员撤离后自动销毁）",
                delegate { TeleportToWilderness(target.Tile); }));
            Find.WindowStack.Add(new FloatMenu(wild));
            return true;
        }

        /// <summary>敌对/无派系目标：弹空降方式菜单（边缘 / 中心附近），与旧行为一致。</summary>
        private void ShowAttackOptions(MapParent mp, PlanetTile tile)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            options.Add(new FloatMenuOption("在地图边缘空降", delegate { AttackMapParent(mp, tile, false); }));
            options.Add(new FloatMenuOption("在地图中心附近空降", delegate { AttackMapParent(mp, tile, true); }));
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>进攻执行：按需生成目标地图（同步卡顿 1-3 秒，与原版空投一致），再选点空降。</summary>
        private void AttackMapParent(MapParent mp, PlanetTile tile, bool nearCenter)
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, mp.def);
            IntVec3 spot;
            if (nearCenter)
                spot = DropCellFinder.TryFindDropSpotNear(map.Center, map, out IntVec3 found, false, true) ? found : map.Center;
            else
                spot = DropCellFinder.RandomDropSpot(map);
            ExecuteTeleport(map, spot);
        }

        /// <summary>远程交易：等同原版通讯台与定居点交易，谈判者取本图社交最高的健康殖民者；不耗蓄电不进冷却。</summary>
        private void OpenTradeWith(Settlement settlement)
        {
            Pawn negotiator = parent.Map.mapPawns.FreeColonistsSpawned
                .Where(p => !p.Downed && !p.InMentalState
                    && p.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                .OrderByDescending(p => p.GetStatValue(StatDefOf.NegotiationAbility))
                .FirstOrDefault();
            if (negotiator == null)
            {
                Messages.Message("交易失败：没有可通话的健康殖民者作为谈判人员", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Find.WindowStack.Add(new Dialog_Trade(negotiator, settlement));
        }

        /// <summary>荒野空降：创建临时落脚点世界对象 → 按该地块 biome 生成荒野地图 → 边缘空降。</summary>
        private void TeleportToWilderness(PlanetTile tile)
        {
            MMT_TeleportTempSite site = (MMT_TeleportTempSite)WorldObjectMaker.MakeWorldObject(MMT_TeleportDefOf.MMT_TeleportTempSite);
            site.Tile = tile;
            Find.WorldObjects.Add(site);
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, MMT_TeleportDefOf.MMT_TeleportTempSite);
            ExecuteTeleport(map, DropCellFinder.RandomDropSpot(map));
        }

        /// <summary>去程执行：抽电、进冷却，把舱内所有人和物瞬间移到目标地图落点；返回成功落图的己方人员。</summary>
        private List<Pawn> ExecuteTeleport(Map targetMap, IntVec3 nearCell)
        {
            List<Pawn> arrived = new List<Pawn>();
            if (!CanTeleportNow(out string reason))
            {
                Messages.Message("传送失败：" + reason, parent, MessageTypeDefOf.RejectInput, false);
                return arrived;
            }
            if (!TryDrainCharge(Props.energyCostWd))
            {
                Messages.Message("传送失败：蓄电不足", parent, MessageTypeDefOf.RejectInput, false);
                return arrived;
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
                    if (pawn.Faction == Faction.OfPlayer)
                        arrived.Add(pawn);
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
            return arrived;
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
            // 若从荒野临时落脚点召回，立即触发一次清理（人走图销）
            if (fromMap.Parent is MMT_TeleportTempSite tempSite)
                tempSite.CheckRemoveMapNow();
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

    /// <summary>
    /// 持续缓慢自转的建筑贴图（星云式效果）：墙钟时间驱动约 45 秒一圈，暂停/断电也转；
    /// drawRotated=false 下与建筑摆放朝向无关。性能开销可忽略（每帧一次四元数叠加，无额外 DrawCall）。
    /// </summary>
    public class MMT_Graphic_Rotating : Graphic_Single
    {
        /// <summary>旋转速度（度/秒）。</summary>
        private const float DegreesPerSecond = 8f;

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            float angle = Time.realtimeSinceStartup * DegreesPerSecond % 360f;
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation + angle);
        }
    }

    [DefOf]
    public static class MMT_TeleportDefOf
    {
        public static ThingDef MMT_TeleportPad;
        /// <summary>荒野临时落脚点世界对象。</summary>
        public static WorldObjectDef MMT_TeleportTempSite;

        static MMT_TeleportDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_TeleportDefOf));
        }
    }
}
