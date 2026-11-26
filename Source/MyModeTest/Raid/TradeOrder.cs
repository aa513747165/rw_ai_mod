using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MyModeTest
{
    // 早期贸易订单：商队到访（LordJob_TradeWithColony）时随机一名成员提供高价收购订单
    // （货物符合殖民地科技水平，报酬 = 货物价值 × 150%-250% 白银）；
    // 接受后商队离图时在殖民地附近生成交易据点，远行队右键据点交付货物；
    // 订单超时或提供人死亡/销毁 → 据点与订单一并消失。
    // 状态自管于 GameComponent_MMTTradeOrders（单订单槽，全字段存档，不走 QuestScriptDef）。

    /// <summary>订单状态机状态。</summary>
    public enum TradeOrderState
    {
        /// <summary>待接取（商队还在图上）。</summary>
        Offered = 0,
        /// <summary>已接取（等商队离图生成据点）。</summary>
        Accepted = 1,
        /// <summary>据点已生成（等交付/到期）。</summary>
        DepotSpawned = 2,
    }

    /// <summary>贸易订单数据（存档在 GameComponent）。</summary>
    public class TradeOrderData : IExposable
    {
        /// <summary>订单提供人（商队随机成员；死亡/销毁即整单取消）。</summary>
        public Pawn giver;
        /// <summary>收购货物。</summary>
        public ThingDef itemDef;
        /// <summary>收购数量。</summary>
        public int itemCount;
        /// <summary>报酬白银（货物价值 × 150%-250%）。</summary>
        public int silverReward;
        public TradeOrderState state;
        /// <summary>据点到期 tick（DepotSpawned 后生效）。</summary>
        public int expireTick = -1;
        /// <summary>订单来源殖民地（生据点选格用）。</summary>
        public Map homeMap;
        /// <summary>交易据点世界对象。</summary>
        public WorldObject_TradeDepot depot;

        public void ExposeData()
        {
            Scribe_References.Look(ref giver, "giver");
            Scribe_Defs.Look(ref itemDef, "itemDef");
            Scribe_Values.Look(ref itemCount, "itemCount");
            Scribe_Values.Look(ref silverReward, "silverReward");
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref expireTick, "expireTick", -1);
            Scribe_References.Look(ref homeMap, "homeMap");
            Scribe_References.Look(ref depot, "depot");
        }
    }

    /// <summary>订单货物生成与远行队货物清点工具（货物池全库扫描一次缓存）。</summary>
    public static class TradeOrderGen
    {
        /// <summary>订单基础价值 = 殖民地财富 × 该系数（钳制在地板/天花板之间）。</summary>
        public const float WealthValueFactor = 0.01f;
        /// <summary>订单基础价值地板（赤贫殖民地保底 200）。</summary>
        public const float ValueFloor = 200f;
        /// <summary>订单基础价值天花板（财富 20 万+ 封顶 2000）。</summary>
        public const float ValueCap = 2000f;

        /// <summary>报酬倍率区间（货物价值的 150%-250%）。</summary>
        public const float RewardFactorMin = 1.5f;
        public const float RewardFactorMax = 2.5f;

        /// <summary>据点等待期限（15 天）。</summary>
        public const int DepotLifetimeTicks = 15 * 60000;

        /// <summary>数量钳制（上限随财富订单规模放大，防止低价值货物堆出离谱堆数）。</summary>
        public const int CountMin = 5;
        public const int CountMax = 500;

        private static List<ThingDef> pool;

        /// <summary>货物池：可交易的非装备/非易腐/非尸体物品（单价 1-60），缓存一次。</summary>
        private static List<ThingDef> Pool
        {
            get
            {
                if (pool == null)
                {
                    pool = new List<ThingDef>();
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.category != ThingCategory.Item)
                            continue;
                        if (def.tradeability == Tradeability.None)
                            continue;
                        // 武器/防具有品质差异，单价不稳，排除
                        if (def.IsWeapon || def.IsApparel)
                            continue;
                        if (def.IsCorpse)
                            continue;
                        // 易腐品交付周期长，排除
                        if (def.HasComp(typeof(CompRottable)))
                            continue;
                        if (def.BaseMarketValue < 1f || def.BaseMarketValue > 60f)
                            continue;
                        if (def.thingCategories.NullOrEmpty())
                            continue;
                        pool.Add(def);
                    }
                }
                return pool;
            }
        }

        /// <summary>按殖民地科技水平生成一张订单（无合格货物返回 null）。</summary>
        public static TradeOrderData Generate(Pawn giver, Map homeMap)
        {
            TechLevel colonyTech = Find.FactionManager.OfPlayer?.def?.techLevel ?? TechLevel.Industrial;
            List<ThingDef> candidates = new List<ThingDef>();
            foreach (ThingDef def in Pool)
            {
                // 科技水平符合殖民地：Undefined 视为原始可产
                if (def.techLevel == TechLevel.Undefined || def.techLevel <= colonyTech)
                    candidates.Add(def);
            }
            if (candidates.Count == 0)
                return null;
            ThingDef item = candidates.RandomElement();
            // 订单规模随殖民地财富缩放：财富 × 1%，钳制 200-2000
            float wealth = homeMap?.wealthWatcher?.WealthTotal ?? 0f;
            float targetValue = Mathf.Clamp(wealth * WealthValueFactor, ValueFloor, ValueCap);
            int count = Mathf.Clamp(Mathf.RoundToInt(targetValue / item.BaseMarketValue), CountMin, CountMax);
            int silver = Mathf.Max(1, Mathf.RoundToInt(count * item.BaseMarketValue * Rand.Range(RewardFactorMin, RewardFactorMax)));
            return new TradeOrderData
            {
                giver = giver,
                itemDef = item,
                itemCount = count,
                silverReward = silver,
                state = TradeOrderState.Offered,
                homeMap = homeMap,
            };
        }

        /// <summary>统计远行队携带的订单货物数量。</summary>
        public static int CountGoods(Caravan caravan, ThingDef itemDef)
        {
            int total = 0;
            List<Thing> items = CaravanInventoryUtility.AllInventoryItems(caravan);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].def == itemDef)
                    total += items[i].stackCount;
            }
            return total;
        }

        /// <summary>从远行队扣除订单货物（调用前须确认数量足够）。</summary>
        public static void TakeGoods(Caravan caravan, ThingDef itemDef, int count)
        {
            int remaining = count;
            List<Thing> items = CaravanInventoryUtility.AllInventoryItems(caravan);
            for (int i = 0; i < items.Count && remaining > 0; i++)
            {
                if (items[i].def != itemDef)
                    continue;
                int take = Mathf.Min(remaining, items[i].stackCount);
                items[i].SplitOff(take).Destroy(DestroyMode.Vanish);
                remaining -= take;
            }
        }

        /// <summary>统计殖民地地图上未被禁用的订单货物数量（家有现货判定）。</summary>
        public static int CountColonyGoods(Map map, ThingDef itemDef)
        {
            int total = 0;
            List<Thing> stacks = map.listerThings.ThingsOfDef(itemDef);
            for (int i = 0; i < stacks.Count; i++)
            {
                if (!stacks[i].IsForbidden(Faction.OfPlayer))
                    total += stacks[i].stackCount;
            }
            return total;
        }

        /// <summary>从殖民地地图扣除订单货物（调用前须确认数量足够）。</summary>
        public static void TakeColonyGoods(Map map, ThingDef itemDef, int count)
        {
            int remaining = count;
            List<Thing> stacks = map.listerThings.ThingsOfDef(itemDef);
            for (int i = 0; i < stacks.Count && remaining > 0; i++)
            {
                if (stacks[i].IsForbidden(Faction.OfPlayer))
                    continue;
                int take = Mathf.Min(remaining, stacks[i].stackCount);
                stacks[i].SplitOff(take).Destroy(DestroyMode.Vanish);
                remaining -= take;
            }
        }
    }

    /// <summary>
    /// 贸易订单状态机（250 tick 节流，与公会委托同节奏）：
    /// 待接取 → 已接取 → 据点等待交付；超时/提供人死亡即取消并销毁据点。
    /// </summary>
    public class GameComponent_MMTTradeOrders : GameComponent
    {
        private const int CheckInterval = 250;

        public static GameComponent_MMTTradeOrders Current
        {
            get { return Verse.Current.Game?.GetComponent<GameComponent_MMTTradeOrders>(); }
        }

        /// <summary>当前活动订单（同时只允许一张）。</summary>
        public TradeOrderData active;

        public GameComponent_MMTTradeOrders(Game game)
        {
        }

        public bool HasActive
        {
            get { return active != null; }
        }

        /// <summary>商队到访出单（Harmony postfix 调用；开关/占槽/科技过滤在此短路）。</summary>
        public void TryOfferOrder(Lord lord, Map map)
        {
            if (!MMTSettings.TradeOrdersEnabled || active != null || map == null)
                return;
            Pawn giver = lord?.ownedPawns?
                .Where(p => p != null && !p.Dead && p.RaceProps != null && p.RaceProps.Humanlike)
                .RandomElementWithFallback();
            if (giver == null)
                return;
            TradeOrderData order = TradeOrderGen.Generate(giver, map);
            if (order == null)
                return;
            active = order;
            float factor = order.silverReward / (order.itemCount * order.itemDef.BaseMarketValue);
            ChoiceLetter_TradeOrder letter = new ChoiceLetter_TradeOrder();
            letter.def = LetterDefOf.PositiveEvent;
            letter.title = "贸易订单";
            letter.Label = "贸易订单：" + order.itemDef.LabelCap + " ×" + order.itemCount;
            letter.Text = "商队成员 " + giver.LabelShortCap + " 想与殖民地做一笔买卖：\n\n"
                + "收购：" + order.itemDef.LabelCap + " ×" + order.itemCount
                + "（基础价值约 " + (order.itemCount * order.itemDef.BaseMarketValue).ToString("F0") + " 白银）\n"
                + "报酬：白银 ×" + order.silverReward + "（约原价 " + factor.ToString("P0") + "）\n\n"
                + "接受后，商队离开殖民地时会在附近驻扎等候（15 天内有效），届时派远行队带上货物前往交付即可。\n"
                + "若殖民地已有现货，可在商队离开前直接交付（选中商人点「交付贸易订单」，或下方按钮）。\n"
                + "注意：若 " + giver.LabelShortCap + " 死亡，订单与等候点将一并取消。";
            letter.lookTargets = new LookTargets(giver);
            Find.LetterStack.ReceiveLetter(letter);
        }

        /// <summary>接取订单（信件按钮回调）。</summary>
        public void AcceptOrder()
        {
            if (active == null || active.state != TradeOrderState.Offered)
                return;
            active.state = TradeOrderState.Accepted;
            Messages.Message("已接取贸易订单：商队离开后会在殖民地附近等候交付。",
                MessageTypeDefOf.PositiveEvent, false);
            // 商队恰好在读信期间离图：立即补生成据点
            if (active.giver != null && !active.giver.Spawned && active.giver.MapHeld == null)
                SpawnDepot(active);
        }

        /// <summary>婉拒订单（信件按钮回调）。</summary>
        public void DeclineOrder()
        {
            if (active == null || active.state != TradeOrderState.Offered)
                return;
            ClearOrder(false);
            Messages.Message("已婉拒贸易订单。", MessageTypeDefOf.NeutralEvent, false);
        }

        /// <summary>家有现货直接交付：商队还在图上时从殖民地库存扣货，白银落在商人旁边（待接取状态视为接受）。</summary>
        public void DeliverFromColony()
        {
            TradeOrderData order = active;
            if (order == null || order.giver == null || !order.giver.Spawned)
                return;
            if (order.state != TradeOrderState.Offered && order.state != TradeOrderState.Accepted)
                return;
            Map map = order.giver.Map;
            if (map == null || !map.IsPlayerHome)
                return;
            if (TradeOrderGen.CountColonyGoods(map, order.itemDef) < order.itemCount)
            {
                Messages.Message("殖民地库存不足，无法直接交付订单。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            TradeOrderGen.TakeColonyGoods(map, order.itemDef, order.itemCount);
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = order.silverReward;
            GenPlace.TryPlaceThing(silver, order.giver.Position, map, ThingPlaceMode.Near);
            Find.LetterStack.ReceiveLetter("贸易订单完成",
                "殖民地直接交付了 " + order.itemDef.LabelCap + " ×" + order.itemCount
                + "，商人当场支付了白银 ×" + order.silverReward + "。",
                LetterDefOf.PositiveEvent, new LookTargets(order.giver));
            ClearOrder(false);
        }

        /// <summary>远行队抵达据点结算（复检订单与货物，防寻路期间状态变化）。</summary>
        public void NotifyDepotArrived(WorldObject_TradeDepot depot, Caravan caravan)
        {
            TradeOrderData order = active;
            if (order == null || order.depot != depot || order.state != TradeOrderState.DepotSpawned)
                return;
            if (TradeOrderGen.CountGoods(caravan, order.itemDef) < order.itemCount)
            {
                Messages.Message("货物不足，无法交付订单。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            TradeOrderGen.TakeGoods(caravan, order.itemDef, order.itemCount);
            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = order.silverReward;
            CaravanInventoryUtility.GiveThing(caravan, silver);
            Find.LetterStack.ReceiveLetter("贸易订单完成",
                "远行队交付了 " + order.itemDef.LabelCap + " ×" + order.itemCount
                + "，商人满意地支付了白银 ×" + order.silverReward + "。",
                LetterDefOf.PositiveEvent, new LookTargets(caravan));
            ClearOrder(true);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (active == null || Find.TickManager.TicksGame % CheckInterval != 0)
                return;
            TickOrder(active);
        }

        /// <summary>订单状态推进（节流调用）。</summary>
        private void TickOrder(TradeOrderData order)
        {
            bool giverGone = order.giver == null || order.giver.Dead || order.giver.Destroyed;
            bool giverLeft = !giverGone && !order.giver.Spawned && order.giver.MapHeld == null;
            switch (order.state)
            {
                case TradeOrderState.Offered:
                    if (giverGone)
                    {
                        CancelOrder("贸易订单取消", "提供订单的商人已死亡，订单作废。");
                    }
                    else if (giverLeft)
                    {
                        // 商队离图仍未接取：静默失效
                        ClearOrder(false);
                    }
                    break;
                case TradeOrderState.Accepted:
                    if (giverGone)
                    {
                        CancelOrder("贸易订单取消", "提供订单的商人已死亡，订单作废。");
                    }
                    else if (giverLeft)
                    {
                        SpawnDepot(order);
                    }
                    break;
                case TradeOrderState.DepotSpawned:
                    if (giverGone)
                    {
                        CancelOrder("贸易订单取消", "提供订单的商人已死亡，等候点已撤离。");
                    }
                    else if (order.depot == null || order.depot.Destroyed || !order.depot.Spawned)
                    {
                        // 据点被外部力量移除（理论上不会自然发生，兜底清槽）
                        ClearOrder(false);
                    }
                    else if (Find.TickManager.TicksGame >= order.expireTick)
                    {
                        CancelOrder("贸易订单过期", "商人等不到货物，已撤离等候点，订单作废。");
                    }
                    break;
            }
        }

        /// <summary>商队离图：在殖民地附近生成交易据点并发信。</summary>
        private void SpawnDepot(TradeOrderData order)
        {
            order.state = TradeOrderState.DepotSpawned;
            PlanetTile homeTile = order.homeMap?.Tile ?? PlanetTile.Invalid;
            WorldObjectDef depotDef = MMT_TradeOrderDefOf.MMT_TradeDepot;
            if (!homeTile.Valid || depotDef == null
                || !TileFinder.TryFindNewSiteTile(out PlanetTile tile, homeTile, 2, 8, true,
                    null, 0f, false, TileFinderMode.Random, false, false, null, null))
            {
                CancelOrder("贸易订单取消", "商队未能在殖民地附近找到合适的驻扎点，订单作废。");
                return;
            }
            WorldObject_TradeDepot depot = (WorldObject_TradeDepot)WorldObjectMaker.MakeWorldObject(depotDef);
            depot.Tile = tile;
            Find.WorldObjects.Add(depot);
            order.depot = depot;
            order.expireTick = Find.TickManager.TicksGame + TradeOrderGen.DepotLifetimeTicks;
            Find.LetterStack.ReceiveLetter("贸易订单：商人已就位",
                "商队在殖民地附近驻扎下来，等候交付 " + order.itemDef.LabelCap + " ×" + order.itemCount + "。\n\n"
                + "派远行队带上货物，在世界地图上右键「贸易商队等候点」选择交付（15 天内有效，逾期撤离）。",
                LetterDefOf.PositiveEvent, new LookTargets(depot));
        }

        /// <summary>取消订单：销毁据点 + 发信 + 清槽。</summary>
        private void CancelOrder(string title, string text)
        {
            Find.LetterStack.ReceiveLetter(title, text, LetterDefOf.NegativeEvent,
                active?.depot != null ? new LookTargets(active.depot) : LookTargets.Invalid);
            ClearOrder(true);
        }

        /// <summary>清订单槽（可选销毁据点）。</summary>
        private void ClearOrder(bool destroyDepot)
        {
            if (destroyDepot && active?.depot != null && active.depot.Spawned && !active.depot.Destroyed)
                active.depot.Destroy();
            active = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref active, "tradeOrderActive");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && active != null
                && (active.itemDef == null || active.giver == null))
            {
                // 存档损坏/物品 def 被移除：丢弃订单防红字
                active = null;
            }
        }
    }

    /// <summary>贸易订单抉择信：接受/拒绝（Choices 打开时重建，委托不进存档）。</summary>
    public class ChoiceLetter_TradeOrder : ChoiceLetter
    {
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                GameComponent_MMTTradeOrders comp = GameComponent_MMTTradeOrders.Current;
                if (comp != null && comp.active != null && comp.active.state == TradeOrderState.Offered)
                {
                    TradeOrderData order = comp.active;
                    if (order.giver != null && order.giver.Spawned && order.giver.Map != null
                        && TradeOrderGen.CountColonyGoods(order.giver.Map, order.itemDef) >= order.itemCount)
                    {
                        yield return new DiaOption("家有现货，立即交付（白银 ×" + order.silverReward + "）")
                        {
                            action = delegate { comp.DeliverFromColony(); },
                            resolveTree = true,
                        };
                    }
                    yield return new DiaOption("接受订单")
                    {
                        action = delegate { comp.AcceptOrder(); },
                        resolveTree = true,
                    };
                    yield return new DiaOption("婉拒")
                    {
                        action = delegate { comp.DeclineOrder(); },
                        resolveTree = true,
                    };
                }
                else
                {
                    yield return new DiaOption("关闭") { resolveTree = true };
                }
            }
        }
    }

    /// <summary>交易据点（纯世界交互，无地图）：远行队右键交付订单货物换白银。</summary>
    public class WorldObject_TradeDepot : WorldObject
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption opt in base.GetFloatMenuOptions(caravan))
                yield return opt;
            GameComponent_MMTTradeOrders comp = GameComponent_MMTTradeOrders.Current;
            TradeOrderData order = comp?.active;
            if (order == null || order.depot != this || order.state != TradeOrderState.DepotSpawned)
                yield break;
            int have = TradeOrderGen.CountGoods(caravan, order.itemDef);
            if (have >= order.itemCount)
            {
                yield return new FloatMenuOption(
                    "交付订单货物（" + order.itemDef.LabelCap + " ×" + order.itemCount
                    + " → 白银 ×" + order.silverReward + "）",
                    delegate { caravan.pather.StartPath(Tile, new CaravanArrivalAction_TradeDepot(this), true); });
            }
            else
            {
                FloatMenuOption lack = new FloatMenuOption(
                    "订单货物不足（需 " + order.itemDef.LabelCap + " ×" + order.itemCount
                    + "，仅携带 ×" + have + "）", null);
                lack.Disabled = true;
                yield return lack;
            }
        }

        public override string GetInspectString()
        {
            GameComponent_MMTTradeOrders comp = GameComponent_MMTTradeOrders.Current;
            TradeOrderData order = comp?.active;
            if (order == null || order.depot != this)
                return base.GetInspectString();
            int leftTicks = order.expireTick - Find.TickManager.TicksGame;
            return "等待交付：" + order.itemDef.LabelCap + " ×" + order.itemCount
                + "（报酬白银 ×" + order.silverReward + "）\n剩余时间："
                + (leftTicks / 60000f).ToString("0.0") + " 天";
        }
    }

    /// <summary>远行队抵达交易据点：交由 GameComponent 验货结算。</summary>
    public class CaravanArrivalAction_TradeDepot : CaravanArrivalAction
    {
        private WorldObject_TradeDepot depot;

        public CaravanArrivalAction_TradeDepot()
        {
        }

        public CaravanArrivalAction_TradeDepot(WorldObject_TradeDepot depot)
        {
            this.depot = depot;
        }

        public override string Label
        {
            get { return "交付贸易订单货物"; }
        }

        public override string ReportString
        {
            get { return "正在交付贸易订单货物"; }
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
        {
            return base.StillValid(caravan, destinationTile)
                && depot != null && depot.Spawned && !depot.Destroyed && depot.Tile == destinationTile;
        }

        public override void Arrived(Caravan caravan)
        {
            GameComponent_MMTTradeOrders.Current?.NotifyDepotArrived(depot, caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref depot, "depot");
        }
    }

    /// <summary>订单提供人 gizmo：商队还在图上且家有现货时可直接交付（待接取/已接取均可）。</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_Pawn_GetGizmos_TradeOrder
    {
        [HarmonyPostfix]
        private static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            GameComponent_MMTTradeOrders comp = GameComponent_MMTTradeOrders.Current;
            TradeOrderData order = comp?.active;
            if (order == null || order.giver != __instance || !__instance.Spawned)
                return;
            if (order.state != TradeOrderState.Offered && order.state != TradeOrderState.Accepted)
                return;
            Map map = __instance.Map;
            if (map == null || !map.IsPlayerHome)
                return;
            __result = __result.Concat(DeliverGizmo(comp, order, map));
        }

        private static IEnumerable<Gizmo> DeliverGizmo(GameComponent_MMTTradeOrders comp, TradeOrderData order, Map map)
        {
            Command_Action cmd = new Command_Action();
            cmd.defaultLabel = "交付贸易订单";
            cmd.defaultDesc = "从殖民地库存交付 " + order.itemDef.LabelCap + " ×" + order.itemCount
                + "，商人当场支付白银 ×" + order.silverReward + "。";
            cmd.icon = order.itemDef.uiIcon;
            int have = TradeOrderGen.CountColonyGoods(map, order.itemDef);
            if (have < order.itemCount)
            {
                cmd.Disable("库存不足：需 " + order.itemDef.label + " ×" + order.itemCount + "（现有 ×" + have + "）");
            }
            else
            {
                cmd.action = delegate { comp.DeliverFromColony(); };
            }
            yield return cmd;
        }
    }

    /// <summary>贸易商队到访钩子：生成贸易 lord 时随机成员提供订单。</summary>
    [HarmonyPatch(typeof(LordMaker), nameof(LordMaker.MakeNewLord))]
    public static class Patch_LordMaker_MakeNewLord
    {
        [HarmonyPostfix]
        private static void Postfix(LordJob lordJob, Map map, Lord __result)
        {
            if (__result == null || !(lordJob is LordJob_TradeWithColony) || map == null || !map.IsPlayerHome)
                return;
            GameComponent_MMTTradeOrders.Current?.TryOfferOrder(__result, map);
        }
    }

    /// <summary>贸易订单 Debug 工具。</summary>
    public static class TradeOrderDebugActions
    {
        [DebugAction("MMT 贸易", "生成贸易订单（需图上有贸易商队）", allowedGameStates = AllowedGameStates.Playing)]
        private static void ForceTradeOrder()
        {
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
                return;
            Lord lord = map.lordManager.lords.FirstOrDefault(l => l.LordJob is LordJob_TradeWithColony);
            if (lord == null)
            {
                Log.Warning("[MMT] 当前地图没有贸易商队，无法生成贸易订单。");
                return;
            }
            GameComponent_MMTTradeOrders.Current?.TryOfferOrder(lord, map);
        }
    }

    [DefOf]
    public static class MMT_TradeOrderDefOf
    {
        /// <summary>交易据点世界对象。</summary>
        public static WorldObjectDef MMT_TradeDepot;

        static MMT_TradeOrderDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_TradeOrderDefOf));
        }
    }
}
