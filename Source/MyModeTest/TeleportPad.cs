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
    /// 折跃传送台参数（当前无 XML 可配置项，占位以便后续扩展）。
    /// 次数制：内部蓄电（CompBuildingBattery）每充满一次转化为 1 次传送次数并重新蓄电；
    /// 次数上限 = 1 + 次元传送模块数（最多 10 个），次数为 0 时进入等待蓄电的 CD。
    /// </summary>
    public class CompProperties_TeleportPad : CompProperties
    {
        public CompProperties_TeleportPad()
        {
            compClass = typeof(CompTeleportPad);
        }
    }

    /// <summary>
    /// 折跃传送台核心 Comp：去程（装载复用原版 CompTransporter；进入地图走原版 TransportShip
    /// 穿梭机管线——光柱落地全部吐出后穿梭机消散，非敌对立据点则组成商队到世界地图该格）
    /// 与回程（供召回 Designator 调用，把框选的己方人员和物品传回本建筑）。
    /// 装载/取消装载 gizmo 由原版 CompTransporter 自带（1.7 已确认），本 Comp 只提供"启动传送"。
    /// 传送消耗"次数"而非直接耗电：蓄电充满一次转化为 1 次（上限 1 + 次元传送模块数）；
    /// 充能效率模块提升蓄电速度。
    /// </summary>
    public class CompTeleportPad : ThingComp
    {
        /// <summary>当前可用传送次数（存档持久化；旧存档无此字段默认 0，蓄电满后自动转化补回）。</summary>
        private int teleportCharges;

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompTransporter transporterComp;
        private CompFlickable flickComp;
        // 蓄电由建筑蓄电 comp 承担（scribe 字段名沿用旧实现的 storedChargeWd，旧存档无缝接管）
        private CompBuildingBattery batteryComp;
        // 增效模块槽（可空）：次元传送模块提升次数上限，充能效率模块提升蓄电速度
        private CompModuleSlots slotsComp;

        private CompProperties_TeleportPad Props
        {
            get { return (CompProperties_TeleportPad)props; }
        }

        private CompTransporter Transporter
        {
            get { return transporterComp; }
        }

        private CompBuildingBattery Battery
        {
            get { return batteryComp; }
        }

        /// <summary>当前可用传送次数。</summary>
        public int TeleportCharges
        {
            get { return teleportCharges; }
        }

        /// <summary>传送次数上限：1 + 次元传送模块数（模块槽上限在 XML 配置，最多 10）。</summary>
        public int MaxTeleportCharges
        {
            get { return 1 + (slotsComp != null ? (int)slotsComp.GetEffectSum(ModuleEffectKeys.ExtraTeleportCharges) : 0); }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            transporterComp = parent.GetComp<CompTransporter>();
            flickComp = parent.GetComp<CompFlickable>();
            batteryComp = parent.GetComp<CompBuildingBattery>();
            slotsComp = parent.GetComp<CompModuleSlots>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref teleportCharges, "teleportCharges");
            // 旧存档迁移：老格式只存蓄电，次数字段缺省为 0；若蓄电已满直接补 1 次，免等一轮
            // （PostLoadInit 时缓存字段尚未赋值，直接取 comp）
            if (Scribe.mode == LoadSaveMode.PostLoadInit && teleportCharges <= 0
                && parent.GetComp<CompBuildingBattery>() != null && parent.GetComp<CompBuildingBattery>().IsFull)
                teleportCharges = 1;
        }

        /// <summary>蓄电转化：充满且次数未达上限时，清空蓄电转化为 1 次传送并重新蓄电。</summary>
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            CompBuildingBattery battery = batteryComp;
            if (battery == null || !battery.IsFull)
                return;
            int max = MaxTeleportCharges;
            if (teleportCharges >= max)
                return; // 次数已满：蓄电保持满格待命
            if (battery.TryDrain(battery.MaxChargeWd))
            {
                teleportCharges++;
                Messages.Message(parent.LabelCap + " 充能完成：传送次数 " + teleportCharges + "/" + max,
                    new LookTargets(parent), MessageTypeDefOf.PositiveEvent, false);
            }
        }

        /// <summary>能否执行一次传送；不可用时给出中文原因（供 gizmo 禁用提示与召回校验）。
        /// 只要求有剩余传送次数：次数由蓄电转化而来，转化后即使电网断电也可发射。</summary>
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
            if (teleportCharges <= 0)
            {
                CompBuildingBattery battery = batteryComp;
                string progress = battery != null
                    ? "（当前蓄电 " + battery.StoredChargeWd.ToString("F0") + "/" + battery.MaxChargeWd.ToString("F0") + " Wd）"
                    : "";
                reason = "传送次数耗尽" + progress + "：保持开启并通电，蓄电充满后恢复 1 次";
                return false;
            }
            reason = null;
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            // 启动传送：舱内为空、装载未完成、次数不足时禁用并提示原因
            Command_Action launch = new Command_Action();
            launch.defaultLabel = "启动传送";
            launch.defaultDesc = "将舱内的人员和物品传送到世界地图上任意地块：敌对据点/任务地点=折跃进攻（跳图后自选落点，光柱落地后全部吐出）；非敌对定居点=组成商队到该格（货物挂成员背包、可正常交易；隐身穿梭机随行，随时可发射折跃返回——返回时除出发时携带的物品外全部吐出，再耗一次传送）；空白地块=生成荒野临时地图（人员全部撤离后自动销毁，遗留物品随图消失）；己方基地=精确选落点。每次传送消耗 1 次传送次数；蓄电充满一次转化为 1 次（上限 1+次元传送模块数）。";
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

            // 开发者模式：一键补满传送次数并充满蓄电（测试传送链路用）
            if (Prefs.DevMode)
            {
                Command_Action devCharge = new Command_Action();
                devCharge.defaultLabel = "DEV: 补满传送次数";
                devCharge.defaultDesc = "开发者模式：传送次数直接补到上限并充满内部蓄电。";
                devCharge.icon = ContentFinder<Texture2D>.Get("Things/Building/Power/Battery", false);
                devCharge.action = delegate
                {
                    teleportCharges = MaxTeleportCharges;
                    Battery?.FillFull();
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
            // 鼠标悬停在行星边缘/无地块处时 Tile 无效，直接索 WorldGrid 会越界抛异常
            if (!target.Tile.Valid)
                return false;
            if (Find.WorldGrid[target.Tile].WaterCovered)
                return false;
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            return mp == null || !mp.HasMap || mp.Map != parent.Map;
        }

        /// <summary>目标器悬停文案：按分支提示（己方/商队交易或进攻/进攻/荒野/拒绝原因）。</summary>
        private TaggedString WorldTargetLabel(GlobalTargetInfo target)
        {
            // 悬停无效地块（行星边缘/无地块处）时给静态提示，避免 WorldGrid 越界刷红字
            if (!target.Tile.Valid)
                return "选择目标地块";
            if (Find.WorldGrid[target.Tile].WaterCovered)
                return "无法传送：目标为水域";
            MapParent mp = Find.WorldObjects.MapParentAt(target.Tile);
            if (mp == null)
                return "荒野折跃：" + Find.WorldGrid[target.Tile].PrimaryBiome.LabelCap + "（生成临时地图）";
            if (mp.HasMap && mp.Map == parent.Map)
                return "无法传送：目标就是当前地图";
            if (mp.HasMap && mp.Map.IsPlayerHome)
                return "传送至：" + mp.LabelCap;
            if (mp is Settlement && mp.Faction != null && !mp.Faction.HostileTo(Faction.OfPlayer))
                return "传送至：" + mp.LabelCap + "（组成商队，可交易/进攻/折跃返回）";
            return "进攻：" + mp.LabelCap;
        }

        /// <summary>节点流转：按目标类型分派——己方精确选点 / 非敌对组成商队 / 敌对进攻 / 荒野临时地图。</summary>
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
                BeginTeleportCellTargeting(homeMap, false);
                return true;
            }
            if (mp is Settlement && mp.Faction != null && !mp.Faction.HostileTo(Faction.OfPlayer))
            {
                // 非敌对立据点：不进图，传送到世界地图该格组成商队（与原版穿梭机一致），
                // 之后由商队在世界上交易或进攻；确认菜单顺便充当防误触（消耗 1 次传送次数）
                List<FloatMenuOption> visit = new List<FloatMenuOption>();
                visit.Add(new FloatMenuOption("传送至 " + mp.LabelCap + " 并组成商队（可交易/进攻；发射折跃返回时非原携带物品全部吐出，消耗一次传送）",
                    delegate { TeleportAsCaravan(mp, target.Tile); }));
                Find.WindowStack.Add(new FloatMenu(visit));
                return true;
            }
            if (mp != null)
            {
                // 敌对据点 / 无派系任务地点：进攻（地图按需生成），直接进入选点
                AttackMapParentPickCell(mp, target.Tile);
                return true;
            }
            // 空白地块：生成荒野临时地图，直接进入选点
            TeleportToWilderness(target.Tile);
            return true;
        }

        /// <summary>落点选择：仅允许点可立足格（Standable），保证光柱精确落在鼠标点中的格子——
        /// 不再因落点被挡而由 FindFreeCellNear 偏移到附近空格；非法格鼠标红标无法点击。</summary>
        private void BeginTeleportCellTargeting(Map map, bool freshlyGenerated)
        {
            TargetingParameters parms = TargetingParameters.ForCell();
            parms.validator = (TargetInfo targ) => targ.Cell.InBounds(map) && targ.Cell.Standable(map);
            Find.Targeter.BeginTargeting(parms,
                delegate(LocalTargetInfo cell) { ExecuteTeleportShuttle(map, cell.Cell, freshlyGenerated); });
        }

        /// <summary>进攻执行：地图未生成时走原版"正在生成地图"进度界面
        /// （LongEventHandler.QueueLongEvent，与商队进攻定居点一致，替代同步卡顿），
        /// 生成完成后跳图进入选点；地图已存在则直接选点。
        /// 鼠标指定落点后才消耗传送；右键取消选点则不消耗（与己方基地选点同一交互）。</summary>
        private void AttackMapParentPickCell(MapParent mp, PlanetTile tile)
        {
            if (!mp.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, mp.def);
                    CameraJumper.TryJump(map.Center, map);
                    BeginTeleportCellTargeting(map, true);
                }, "GeneratingMapForNewEncounter", false, null);
                return;
            }
            Map existing = mp.Map;
            CameraJumper.TryJump(existing.Center, existing);
            BeginTeleportCellTargeting(existing, false);
        }

        /// <summary>荒野折跃：创建临时落脚点世界对象 → 按该地块 biome 生成荒野地图 →
        /// 跳图后鼠标自选落点。全部放进"正在生成地图"进度界面内执行（生成耗时 1-3 秒，不再卡顿）；
        /// 取消选点时临时站点无人员，MMT_TeleportTempSite 自检会销图回收。</summary>
        private void TeleportToWilderness(PlanetTile tile)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                MMT_TeleportTempSite site = (MMT_TeleportTempSite)WorldObjectMaker.MakeWorldObject(MMT_TeleportDefOf.MMT_TeleportTempSite);
                site.Tile = tile;
                Find.WorldObjects.Add(site);
                Map map = GetOrGenerateMapUtility.GetOrGenerateMap(tile, Find.World.info.initialMapSize, MMT_TeleportDefOf.MMT_TeleportTempSite);
                // 取消选点时临时站点无人员，MMT_TeleportTempSite 自检会销图回收
                CameraJumper.TryJump(map.Center, map);
                BeginTeleportCellTargeting(map, true);
            }, "GeneratingMapForNewEncounter", false, null);
        }

        /// <summary>去程执行（进入地图）：扣一次传送次数，舱内人和物转入隐形穿梭机，
        /// 走原版 TransportShip 管线——光柱落地 → 全部吐出 → 光柱离去穿梭机销毁。
        /// freshlyGenerated 标记目标地图是否本次新生成（敌对定居点信件/通告用）。</summary>
        private void ExecuteTeleportShuttle(Map targetMap, IntVec3 nearCell, bool freshlyGenerated)
        {
            if (!CanTeleportNow(out string reason))
            {
                Messages.Message("传送失败：" + reason, parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!TryConsumeCharge())
            {
                Messages.Message("传送失败：传送次数不足", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }

            // 本端特效
            TeleportFx.Play(parent.Position, parent.Map);

            // 落点选择器已限制只能点可立足格，此处基本原样返回点击格；
            // FindFreeCellNear 仅作兜底（极端情况：选点后落点瞬间被占）
            IntVec3 landing = FindFreeCellNear(targetMap, nearCell, 12);
            int pawns = 0, things = 0;
            bool hasPlayerPawn = false;
            List<Thing> cargo = new List<Thing>(Transporter.innerContainer);
            foreach (Thing t in cargo)
            {
                Transporter.innerContainer.Remove(t);
                if (t is Pawn pawn)
                {
                    pawns++;
                    if (pawn.Faction == Faction.OfPlayer)
                        hasPlayerPawn = true;
                }
                else
                {
                    things++;
                }
            }
            // 传送完毕后解散装载组（groupID 复位、残余装载lord移除），
            // 否则传送台一直停留在"待发"状态，无法再开始新一轮装载
            Transporter.CancelLoad(parent.Map);

            // 底层穿梭机：建船并装入全部载荷（船注册进 TransportShipManager，后续作业自动推进）
            TransportShip ship = TransportShipMaker.MakeTransportShip(MMT_TeleportDefOf.MMT_Ship_TeleportShuttle, cargo);

            // 敌对定居点：复制原版 TransportersArrivalAction_TransportShip.Arrived 的关系惩罚与信件
            MapParent mp = targetMap.Parent;
            LookTargets lookTargets = new LookTargets(landing, targetMap);
            if (mp is Settlement settlement && settlement.Faction != Faction.OfPlayer)
            {
                TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
                TaggedString letterText = "LetterShuttleLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
                SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
                if (freshlyGenerated)
                {
                    Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(targetMap.mapPawns.AllPawns, ref letterLabel, ref letterText,
                        "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), true);
                }
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTargets, settlement.Faction);
            }

            // 抵达（生成降临光柱天坠物）→ 落地一次性全吐 → 离去光柱升空并销毁穿梭机（无目的地）
            ship.ArriveAt(landing, mp);
            ship.AddJob(ShipJobMaker.MakeShipJob(MMT_TeleportDefOf.MMT_ShipJob_TeleportUnload));
            ship.AddJob(ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway));

            if (hasPlayerPawn && landing.Fogged(targetMap))
                FloodFillerFog.FloodUnfog(landing, targetMap);
            Messages.Message("传送完成：" + pawns + " 名人员、" + things + " 件物品正随折跃光柱抵达目标地图",
                lookTargets, MessageTypeDefOf.PositiveEvent, false);
            CameraJumper.TryJump(landing, targetMap);
            // 流程结束
        }

        /// <summary>去程执行（非敌对立据点）：不进图，舱内人员在世界地图该格组成商队、
        /// 物品入商队背包（参照原版 TransportersArrivalAction_FormCaravan），之后可交易或进攻。</summary>
        private void TeleportAsCaravan(MapParent mp, PlanetTile tile)
        {
            if (!CanTeleportNow(out string reason))
            {
                Messages.Message("传送失败：" + reason, parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            List<Thing> cargo = new List<Thing>(Transporter.innerContainer);
            List<Pawn> pawns = new List<Pawn>();
            List<Thing> items = new List<Thing>();
            foreach (Thing t in cargo)
            {
                if (t is Pawn pawn)
                    pawns.Add(pawn);
                else
                    items.Add(t);
            }
            if (pawns.Count == 0)
            {
                Messages.Message("无法传送：组成商队至少需要一名人员", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!TryConsumeCharge())
            {
                Messages.Message("传送失败：传送次数不足", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            TeleportFx.Play(parent.Position, parent.Map);
            foreach (Thing t in cargo)
                Transporter.innerContainer.Remove(t);
            Transporter.CancelLoad(parent.Map);

            if (!GenWorldClosest.TryFindClosestPassableTile(tile, out PlanetTile foundTile))
                foundTile = tile;
            Caravan caravan = CaravanMaker.MakeCaravan(pawns, Faction.OfPlayer, foundTile, true);
            // 货物挂在成员背包（原版分配逻辑），保证世界地图交易可用
            foreach (Thing item in items)
                CaravanInventoryUtility.GiveThing(caravan, item);
            // 隐身穿梭机载体：挂在成员背包里随行（大地图不绘制=隐身、Mass=0），
            // 并快照出发时携带物品台账——携带载体的商队获得"发射：折跃返回"按钮
            // （TeleportCaravanReturn），返回时台账之外的物品（交易购得等）全部吐出
            Thing shuttle = ThingMaker.MakeThing(MMT_TeleportDefOf.MMT_TeleportShuttle);
            ((ThingWithComps)shuttle).GetComp<CompTeleportCargoLedger>()?.Snapshot(caravan);
            CaravanInventoryUtility.GiveThing(caravan, shuttle);
            Messages.Message("传送完成：" + pawns.Count + " 名人员、" + items.Count + " 件物品已抵达" + mp.LabelCap
                + "（隐身穿梭机随行：可交易/进攻，或随时发射折跃返回——返回时途中获得的物品全部吐出）",
                new LookTargets(caravan), MessageTypeDefOf.PositiveEvent, false);
            CameraJumper.TryJump(new GlobalTargetInfo(caravan));
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
            if (!TryConsumeCharge())
            {
                Messages.Message("召回失败：传送次数不足", MessageTypeDefOf.RejectInput, false);
                return;
            }

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
                    // 召回顺带回收背包里的折跃载体：舱内货物就地吐出后再销毁，防止货物随载体消失
                    Thing carrier = pawn.inventory?.innerContainer?.FirstOrDefault(t => t.def == MMT_TeleportDefOf.MMT_TeleportShuttle);
                    if (carrier != null)
                    {
                        pawn.inventory.innerContainer.Remove(carrier);
                        TeleportPadUtility.DumpShuttleCargoAndDestroy(carrier, cell, parent.Map);
                    }
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

        // ---- 传送次数 ----

        /// <summary>消耗一次传送次数；次数不足时不扣并返回 false。</summary>
        private bool TryConsumeCharge()
        {
            if (teleportCharges <= 0)
                return false;
            teleportCharges--;
            return true;
        }

        /// <summary>消耗一次传送：校验可用性 → 扣次数，任一失败不扣。
        /// 供商队折跃返回（TeleportCaravanReturn）等外部流程调用。</summary>
        internal bool TryConsumeTeleport(out string reason)
        {
            if (!CanTeleportNow(out reason))
                return false;
            if (!TryConsumeCharge())
            {
                reason = "传送次数不足";
                return false;
            }
            return true;
        }

        // ---- 工具 ----

        /// <summary>在中心附近找可站立格，递增半径兜底；找不到退到地图中心。</summary>
        internal static IntVec3 FindFreeCellNear(Map map, IntVec3 center, int maxRadius)
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
            // 蓄电行由 CompBuildingBattery 自报，这里只列传送次数与舱内质量
            int max = MaxTeleportCharges;
            string text = "传送次数: " + teleportCharges + "/" + max;
            if (teleportCharges < max)
                text += "（蓄电充满可再恢复 1 次）";
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
            defaultDesc = "框选一个范围，将范围内所有己方人员和物品传送回母基地的折跃传送台（消耗一次传送次数）。";
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
            List<CompTeleportPad> pads = TeleportPadUtility.FindAvailablePads();
            if (pads.Count == 0)
            {
                Messages.Message("没有可用的折跃传送台（需已开启且有剩余传送次数）", MessageTypeDefOf.RejectInput, false);
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
                        pad.parent.Map.Parent.LabelCap + "（传送次数 " + pad.TeleportCharges + "/" + pad.MaxTeleportCharges + "）",
                        delegate { local.ExecuteRecall(Map, targets.ToList()); }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            FinalizeDesignationSucceeded();
        }
    }

    /// <summary>传送特效：对齐原版 Skip_ExitNoDelay 效果器的构成（大闪光 + 巨型白闪 + 内圈次元洞 +
    /// 外圈扭曲环），但去掉原版的 11~16 tick 延迟全部立即触发——降临光柱本身已充当"预告"。
    /// 内圈/外环用 MMT 拉长版（原版全程仅 0.4 秒，3 倍速下约 0.13 秒肉眼不可见，曾被反馈
    /// "折跃效果没有显示"）；大闪光用原版 PsycastSkipFlashExit（淡出 1.3 秒本来就够长）。
    /// withSound=false 时只放视觉闪现（用于降临光柱落地——落地音已由 skyfaller impactSound 播放）。</summary>
    internal static class TeleportFx
    {
        public static void Play(IntVec3 cell, Map map, bool withSound = true)
        {
            if (map == null || !cell.IsValid || !cell.InBounds(map))
                return;
            Vector3 center = cell.ToVector3Shifted();
            if (MMT_TeleportDefOf.PsycastSkipFlashExit != null)
                FleckMaker.Static(center, map, MMT_TeleportDefOf.PsycastSkipFlashExit, 2f);
            if (MMT_TeleportDefOf.PlainFlash != null)
                FleckMaker.Static(center, map, MMT_TeleportDefOf.PlainFlash, 7f);
            if (MMT_TeleportDefOf.MMT_TeleportSkipInnerLong != null)
                FleckMaker.Static(center, map, MMT_TeleportDefOf.MMT_TeleportSkipInnerLong, 2f);
            if (MMT_TeleportDefOf.MMT_TeleportSkipOuterRingLong != null)
                FleckMaker.Static(center, map, MMT_TeleportDefOf.MMT_TeleportSkipOuterRingLong, 2f);
            if (withSound && SoundDefOf.Psycast_Skip_Exit != null)
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
        /// <summary>隐形折跃载体（ThingDef）：去程穿梭机本体，商队分支也作为随行物品挂进背包。</summary>
        public static ThingDef MMT_TeleportShuttle;
        /// <summary>折跃传送船（穿梭机逻辑载体）。</summary>
        public static TransportShipDef MMT_Ship_TeleportShuttle;
        /// <summary>落地一次性全吐的卸货作业。</summary>
        public static ShipJobDef MMT_ShipJob_TeleportUnload;
        /// <summary>折跃闪光内圈次元洞（拉长版 ~1.3 秒，见 Defs/Effects/MMT_Flecks_Teleport.xml）。</summary>
        public static FleckDef MMT_TeleportSkipInnerLong;
        /// <summary>折跃闪光外圈扭曲环（拉长版 ~1.3 秒）。</summary>
        public static FleckDef MMT_TeleportSkipOuterRingLong;
        /// <summary>原版折跃落地大闪光（Core，淡出 1.3 秒；FleckDefOf 无此字段故在此声明）。</summary>
        public static FleckDef PsycastSkipFlashExit;
        /// <summary>原版巨型白闪（Core；FleckDefOf 无此字段故在此声明）。</summary>
        public static FleckDef PlainFlash;

        static MMT_TeleportDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_TeleportDefOf));
        }
    }
}
