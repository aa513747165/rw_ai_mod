using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 折跃传送专用卸货作业：原版 ShipJob_Unload 每 60 tick 才吐一件（空投节奏），
    /// 传送要求"落地即全部吐出"，这里在首个 Tick 内循环原版公开的 UnloadThingFromShuttle
    /// 一次吐空后立刻 End——队列中的 ShipJob_FlyAway 随即接管（离去光柱升空、隐形穿梭机销毁）。
    /// </summary>
    public class MMT_ShipJob_UnloadInstant : ShipJob
    {
        // 首个 Tick 内卸空并手动 End，不靠 ShouldEnd 驱动
        protected override bool ShouldEnd
        {
            get { return false; }
        }

        public override bool ShowGizmos
        {
            get { return false; }
        }

        public override bool TryStart()
        {
            // 与原版 Unload 一致：穿梭机已落地（降临天坠物触地生成）才能开始卸货
            if (!transportShip.ShipExistsAndIsSpawned)
                return false;
            return base.TryStart();
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            Thing shipThing = transportShip.shipThing;
            if (shipThing == null || !shipThing.Spawned)
            {
                End();
                return;
            }
            Map map = shipThing.Map;
            IntVec3 dropCell = shipThing.InteractionCell;
            // 原版吐出校验拒绝迷雾格，而传送落点常在未探索区：先驱散落点迷雾保证能吐出
            if (dropCell.IsValid && dropCell.InBounds(map) && dropCell.Fogged(map))
                FloodFillerFog.FloodUnfog(dropCell, map);
            ThingOwner container = transportShip.TransporterComp.innerContainer;
            int guard = container.Count + 1;
            while (container.Count > 0 && guard-- > 0)
            {
                Thing thing = container[0];
                int before = container.Count;
                ShipJob_Unload.UnloadThingFromShuttle(transportShip, thing, null, true);
                if (container.Count == before)
                {
                    // 吐出失败（极端情况：交互格周边被堵死/全迷雾）时无校验强制放置兜底，防死循环丢货
                    container.TryDrop(thing, dropCell, map, ThingPlaceMode.Near, out Thing _);
                }
            }
            transportShip.TransporterComp.TryRemoveLord(map);
            End();
        }
    }
}
