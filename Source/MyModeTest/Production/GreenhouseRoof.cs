using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>温室玻璃天窗相关 DefOf。</summary>
    [DefOf]
    public static class MMT_GreenhouseDefOf
    {
        /// <summary>透明屋顶（玻璃天窗顶）：透光由 GlowGrid 补丁放行天空光实现。</summary>
        public static RoofDef MMT_TransparentRoof;

        static MMT_GreenhouseDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_GreenhouseDefOf));
        }
    }

    /// <summary>
    /// 玻璃天窗框架：建造完成（非读档 SpawnSetup）时把所在格设为透明屋顶并自毁，
    /// 地图上只留屋顶数据不占格；若建造期间支撑被破坏则改为原地返还建材。
    /// </summary>
    public class Building_TransparentRoofFrame : Building
    {
        // 流程开始：落成即转换为透明屋顶
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
                return; // 正常不会在地图存档中残留框架，读档时不再处理（防御）
            IntVec3 c = Position;
            if (CanSetTransparentRoof(c, map))
            {
                // 节点流转：框架 -> 透明屋顶格
                map.roofGrid.SetRoof(c, MMT_GreenhouseDefOf.MMT_TransparentRoof);
                MoteMaker.PlaceTempRoof(c, map);
            }
            else
            {
                // 建造期间支撑被拆/格子已被其它屋顶覆盖：退还全部建材，避免吞材料
                if (def.costList != null)
                {
                    for (int i = 0; i < def.costList.Count; i++)
                    {
                        Thing refund = ThingMaker.MakeThing(def.costList[i].thingDef);
                        refund.stackCount = def.costList[i].count;
                        GenPlace.TryPlaceThing(refund, c, map, ThingPlaceMode.Near);
                    }
                }
            }
            // 流程结束：框架消失，只留屋顶格（或返还的材料）
            Destroy(DestroyMode.Vanish);
        }

        /// <summary>该格能否铺设透明屋顶：无现存屋顶 + 支撑范围内 + 与支撑连通（同原版建屋顶判定）。</summary>
        public static bool CanSetTransparentRoof(IntVec3 c, Map map)
        {
            return c.InBounds(map)
                && !c.Roofed(map)
                && RoofCollapseUtility.WithinRangeOfRoofHolder(c, map)
                && RoofCollapseUtility.ConnectedToRoofHolder(c, map, assumeRoofAtRoot: true);
        }
    }

    /// <summary>
    /// 玻璃天窗框架放置限制：只允许放在无屋顶且与支撑（墙体/岩层）连通的格子，
    /// 不满足时给出中文原因。
    /// </summary>
    public class PlaceWorker_TransparentRoofFrame : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.Roofed(map))
                return new AcceptanceReport("这里已经有屋顶了");
            if (!RoofCollapseUtility.WithinRangeOfRoofHolder(loc, map))
                return new AcceptanceReport("距离支撑太远：玻璃天窗需要建在墙体或岩层附近");
            if (!RoofCollapseUtility.ConnectedToRoofHolder(loc, map, assumeRoofAtRoot: true))
                return new AcceptanceReport("玻璃天窗需要与支撑（墙体/岩层）连通");
            return AcceptanceReport.WasAccepted;
        }
    }

    /// <summary>
    /// 透光补丁：透明屋顶不遮挡天空光（原版 GroundGlowAt 对任何屋顶一律挡光），
    /// 植物生长光照、光照不足提示等都走 GroundGlowAt，一处补丁全部生效。
    /// </summary>
    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt), typeof(IntVec3), typeof(bool), typeof(bool))]
    public static class Patch_GreenhouseGroundGlow
    {
        public static void Postfix(IntVec3 c, bool ignoreSky, ref float __result, Map ___map)
        {
            if (ignoreSky || ___map == null)
                return;
            float skyGlow = ___map.skyManager.CurSkyGlow;
            if (__result < skyGlow && ___map.roofGrid.RoofAt(c) == MMT_GreenhouseDefOf.MMT_TransparentRoof)
                __result = skyGlow;
        }
    }
}
