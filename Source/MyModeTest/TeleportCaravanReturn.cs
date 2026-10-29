using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>折跃载体携带物品台账的 CompProperties。</summary>
    public class CompProperties_TeleportCargoLedger : CompProperties
    {
        public CompProperties_TeleportCargoLedger()
        {
            compClass = typeof(CompTeleportCargoLedger);
        }
    }

    /// <summary>
    /// 折跃载体的携带物品台账：出发组成商队时快照全队背包里每个物品及其数量（引用+计数），
    /// 返回时把台账之外的物品（交易购得等途中获得）全部吐出；
    /// 途中获得的同类物品堆叠进原堆的，按计数差只吐超出部分。
    /// </summary>
    public class CompTeleportCargoLedger : ThingComp
    {
        private Dictionary<Thing, int> originalItems = new Dictionary<Thing, int>();

        /// <summary>出发快照：记录商队全队背包物品（不含载体自身）。须在货物分配进背包后调用。</summary>
        public void Snapshot(Caravan caravan)
        {
            originalItems.Clear();
            foreach (Thing t in CaravanInventoryUtility.AllInventoryItems(caravan))
            {
                if (t.def != MMT_TeleportDefOf.MMT_TeleportShuttle)
                    originalItems[t] = t.stackCount;
            }
        }

        /// <summary>把该成员背包里台账之外的物品吐出到指定位置；台账内物品只吐超出出发数量的部分。
        /// 装备/服装不在背包容器内，天然留在身上。返回吐出的堆数。</summary>
        public int DumpNonOriginal(Pawn pawn, IntVec3 cell, Map map)
        {
            int dropped = 0;
            ThingOwner<Thing> inv = pawn.inventory.innerContainer;
            for (int i = inv.Count - 1; i >= 0; i--)
            {
                Thing t = inv[i];
                if (t.def == MMT_TeleportDefOf.MMT_TeleportShuttle)
                    continue;
                if (originalItems.TryGetValue(t, out int startCount))
                {
                    int excess = t.stackCount - startCount;
                    if (excess <= 0)
                        continue; // 原携带（或途中被消耗/卖掉一部分）：留在身上
                    // 后来获得的同类物品堆叠进了原堆：只吐出超出的部分
                    Thing extra = t.SplitOff(excess);
                    GenPlace.TryPlaceThing(extra, cell, map, ThingPlaceMode.Near);
                    dropped++;
                }
                else
                {
                    // 途中获得（交易购得等）：整堆吐出
                    inv.Remove(t);
                    GenPlace.TryPlaceThing(t, cell, map, ThingPlaceMode.Near);
                    dropped++;
                }
            }
            return dropped;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref originalItems, "originalItems", LookMode.Reference, LookMode.Value);
            if (originalItems == null)
                originalItems = new Dictionary<Thing, int>();
        }
    }

    /// <summary>
    /// 商队折跃返回：非敌对立据点分支组成商队时，货物挂在成员背包（可正常交易），
    /// 隐身穿梭机载体随行（挂在背包里，大地图不绘制=隐身、Mass=0）并记录出发物品台账。
    /// 本补丁给携带载体的商队追加"发射：折跃返回"按钮——触发折跃传送台的传送，
    /// 全队传回传送台落点；台账之外的物品（交易购得等）在落点全部吐出，载体随传送消散、商队解散。
    /// 检测对象是"物品"而非商队子类，商队拆分后哪个商队拿到载体哪个就有返回按钮。
    /// </summary>
    [HarmonyPatch(typeof(Caravan), "GetGizmos")]
    public static class CaravanGetGizmosTeleportReturnPatch
    {
        // 载体检测按 60 tick 缓存（参照原版 Caravan.hasShuttleDirty 模式）：
        // GetGizmos 每帧被调多次，避免每帧全量扫背包
        private static int cacheTick = -1;
        private static Caravan cacheCaravan;
        private static Thing cachedShuttle;

        /// <summary>商队携带的折跃载体（无则 null）。结果缓存 60 tick。</summary>
        public static Thing FindTeleportShuttle(Caravan caravan)
        {
            int tick = Find.TickManager.TicksGame;
            if (cacheCaravan == caravan && tick - cacheTick < 60)
                return cachedShuttle;
            cacheCaravan = caravan;
            cacheTick = tick;
            cachedShuttle = null;
            List<Thing> items = CaravanInventoryUtility.AllInventoryItems(caravan);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].def == MMT_TeleportDefOf.MMT_TeleportShuttle)
                {
                    cachedShuttle = items[i];
                    break;
                }
            }
            return cachedShuttle;
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Caravan __instance)
        {
            foreach (Gizmo g in values)
                yield return g;
            Caravan caravan = __instance;
            // 与原版一致：商队命令只在单选时出现
            if (caravan == null || !caravan.IsPlayerControlled
                || Find.WorldSelector.SingleSelectedObject != caravan)
                yield break;
            if (FindTeleportShuttle(caravan) == null)
                yield break;

            Command_Action cmd = new Command_Action();
            cmd.defaultLabel = "发射：折跃返回";
            cmd.defaultDesc = "激活随行的隐形折跃载体，触发折跃传送台的传送：全队传送回传送台落点（消耗 1 次传送次数），载体随传送消散。出发时携带的物品留在身上，途中获得的物品（交易购得等）在落点全部吐出。";
            cmd.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", false);
            List<CompTeleportPad> pads = TeleportPadUtility.FindAvailablePads();
            if (pads.Count == 0)
            {
                cmd.Disable("没有可用的折跃传送台（需已开启且有剩余传送次数）");
            }
            else if (pads.Count == 1)
            {
                CompTeleportPad pad = pads[0];
                cmd.action = delegate { ExecuteReturn(caravan, pad); };
            }
            else
            {
                // 多台可用：弹菜单选目标传送台（列出所在基地与剩余传送次数）
                cmd.action = delegate
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (CompTeleportPad p in pads)
                    {
                        CompTeleportPad local = p;
                        options.Add(new FloatMenuOption(
                            p.parent.Map.Parent.LabelCap + "（传送次数 " + p.TeleportCharges + "/" + p.MaxTeleportCharges + "）",
                            delegate { ExecuteReturn(caravan, local); }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                };
            }
            yield return cmd;
        }

        /// <summary>返回执行：消耗 1 次传送次数，全队连同背包货物传回传送台落点，
        /// 载体销毁、商队解散（背包货物本就分布在成员身上，随人传送）。</summary>
        private static void ExecuteReturn(Caravan caravan, CompTeleportPad pad)
        {
            if (!pad.TryConsumeTeleport(out string reason))
            {
                Messages.Message("折跃返回失败：" + reason, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Map map = pad.parent.Map;
            IntVec3 homeCell = CompTeleportPad.FindFreeCellNear(map, pad.parent.InteractionCell, 12);

            List<Pawn> pawns = new List<Pawn>(caravan.PawnsListForReading);

            // 台账之外的物品（交易购得等途中获得）在落点全部吐出；出发时携带的留在身上
            Thing shuttle = FindTeleportShuttle(caravan);
            CompTeleportCargoLedger ledger = (shuttle as ThingWithComps)?.GetComp<CompTeleportCargoLedger>();
            int dropped = 0;
            foreach (Pawn pawn in pawns)
                dropped += ledger?.DumpNonOriginal(pawn, homeCell, map) ?? 0;

            // 载体随传送能量耗尽消散
            if (shuttle != null)
            {
                Pawn owner = CaravanInventoryUtility.GetOwnerOf(caravan, shuttle);
                owner?.inventory.innerContainer.Remove(shuttle);
                TeleportPadUtility.DumpShuttleCargoAndDestroy(shuttle, homeCell, map);
            }

            foreach (Pawn pawn in pawns)
            {
                IntVec3 cell = CompTeleportPad.FindFreeCellNear(map, homeCell, 8);
                // 从世界pawn直接生成进地图（原版商队进图同款写法），出发时携带的物品随人带走
                GenSpawn.Spawn(pawn, cell, map, Rot4.Random);
                pawn.Notify_Teleported();
            }
            // 商队空员销毁（原版 CaravanEnterMapUtility.Enter 末尾同款写法）
            caravan.RemoveAllPawns();
            if (!caravan.Destroyed)
                caravan.Destroy();

            TeleportFx.Play(homeCell, map);
            Messages.Message("折跃返回完成：" + pawns.Count + " 名人员已随隐形载体传回" + map.Parent.LabelCap
                + (dropped > 0 ? "，" + dropped + " 堆途中获得的物品已在落点吐出" : ""),
                new LookTargets(new GlobalTargetInfo(homeCell, map)), MessageTypeDefOf.PositiveEvent, false);
            CameraJumper.TryJump(homeCell, map);
        }
    }

    /// <summary>折跃传送台公用扫描与载体处理。</summary>
    internal static class TeleportPadUtility
    {
        /// <summary>把折跃载体舱内货物在指定位置全部吐出，然后销毁载体（防止货物随载体消失）。</summary>
        public static void DumpShuttleCargoAndDestroy(Thing shuttle, IntVec3 cell, Map map)
        {
            CompTransporter transporter = (shuttle as ThingWithComps)?.GetComp<CompTransporter>();
            if (transporter != null)
            {
                ThingOwner hold = transporter.innerContainer;
                int guard = hold.Count + 1;
                while (hold.Count > 0 && guard-- > 0)
                    hold.TryDrop(hold[0], cell, map, ThingPlaceMode.Near, out Thing _);
            }
            shuttle.Destroy(DestroyMode.Vanish);
        }

        /// <summary>扫描所有己方基地中可用的传送台（已开启、有剩余传送次数）。</summary>
        public static List<CompTeleportPad> FindAvailablePads()
        {
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
            return pads;
        }
    }
}
