using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 折跃降临光柱：在 ShuttleIncoming 基础上给 DrawColor 叠加呼吸脉动（透明度 0.75~1.0）。
    /// 必须用 ageTicks 整数驱动而非常见的 realtimeSinceStartup：淡入淡出会让 get_Graphic 走
    /// GraphicData.GraphicColoredFor → Graphic.GetColoredVersion，后者按颜色精确值缓存子 Graphic，
    /// float 时间每帧一个新颜色会撑爆缓存；整数 tick 驱动的颜色取值有界（≤ 一个周期）。
    /// 顺带的好处：脉动跟随游戏速度、暂停即停。
    /// </summary>
    public class MMT_ShuttleIncomingBreathing : ShuttleIncoming
    {
        // 呼吸周期 ~90 tick（1.5 秒）
        private const float PulsePeriodTicks = 90f;

        public override Color DrawColor
        {
            get
            {
                Color c = base.DrawColor;
                c.a *= 0.75f + 0.25f * Mathf.Sin(ageTicks * (Mathf.PI * 2f / PulsePeriodTicks));
                return c;
            }
        }

        /// <summary>落地瞬间补折跃闪现视觉（PsycastSkip 四件套）；落地音已由
        /// skyfaller 的 impactSound（Psycast_Skip_Exit）播放，这里静音避免叠双份。
        /// 注意必须先捕获 Position/Map 再调 base.Impact()——后者内部 Destroy(this)，
        /// 之后 Map 返回 null、Position 失效，闪光会被 Play 的有效性检查静默跳过。</summary>
        protected override void Impact()
        {
            IntVec3 cell = Position;
            Map map = Map;
            base.Impact();
            TeleportFx.Play(cell, map, withSound: false);
        }
    }

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
