using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>哨戒待机模式的 def 引用（MMT_Sentinel 工作模式，见 Defs/Misc/MMT_MechWorkMode_Sentinel.xml）。</summary>
    [DefOf]
    public static class MMT_MechStandbyDefOf
    {
        public static MechWorkModeDef MMT_Sentinel;

        static MMT_MechStandbyDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_MechStandbyDefOf));
        }
    }

    /// <summary>
    /// 哨戒待机 JobGiver（挂在 Mechanoid ThinkTree 的 MMT_Sentinel 工作模式分支末尾，见 MMT_Patch_MechStandby.xml）：
    /// 机械体闲下来时发原版 SelfShutdown 作业（躺地关机、不再跑任何 JobGiver 扫描、电量冻结并缓慢自充）。
    /// 优先走玩家在指令面板标定的待机点，未标定则复用原版自关机选址（RCellFinder）。
    /// 征召时或附近有活动敌人（65 格内，与战斗节点索敌半径一致）时不发睡眠作业，保持清醒应对；
    /// 远处的潜伏型威胁（如地图生成的血肉兽突袭 Lord，65 格内才交战）不挡睡眠，
    /// 真正的进攻由 <see cref="MechStandbyManager"/> 在新敌对 Lord 出现时事件驱动唤醒。
    /// 征召/受击/切换工作模式走原版作业中断逻辑（set_Drafted 结束可中断作业、SetWorkModeForPawn 触发 CheckForJobOverride）。
    /// </summary>
    public class JobGiver_MechStandby : ThinkNode_JobGiver
    {
        /// <summary>索敌半径，与 MMT_Patch_MechStandby.xml 里 JobGiver_AIFightEnemies 的 targetAcquireRadius 一致。</summary>
        private const float HostileCheckRadius = 65f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            Map map = pawn.Map;
            if (map == null || pawn.Drafted)
            {
                return null;
            }
            // 附近有活动敌人时不睡（保持清醒让位给战斗节点）；远处潜伏敌人不管，避免
            // 地图生成的血肉兽之类"永不进攻"的敌对 Lord 把睡眠门永久堵死
            if (AnyHostileNearby(pawn, map))
            {
                return null;
            }
            MapComponent_MechStandby comp = map.GetComponent<MapComponent_MechStandby>();
            if (comp != null && comp.TryGetStandbySpot(pawn, out IntVec3 spot)
                && spot.IsValid && spot.Standable(map) && !spot.IsForbidden(pawn)
                && pawn.CanReserveAndReach(spot, PathEndMode.OnCell, Danger.Deadly))
            {
                return MakeSleepJob(spot);
            }
            // 未标定待机点：复用原版自关机选址逻辑
            if (RCellFinder.TryFindNearbyMechSelfShutdownSpot(pawn.Position, pawn, map, out IntVec3 result, allowForbidden: true))
            {
                return MakeSleepJob(result);
            }
            return null;
        }

        /// <summary>与原版 JobGiver_SelfShutdown 一致：forceSleep 使机械体到位后立即入睡。</summary>
        private static Job MakeSleepJob(IntVec3 cell)
        {
            Job job = JobMaker.MakeJob(JobDefOf.SelfShutdown, cell);
            job.forceSleep = true;
            return job;
        }

        /// <summary>指定半径内是否有清醒的、敌对玩家的在场单位（休眠威胁不算，与 EnemyRaidTracker.IsAwake 同口径）。</summary>
        private static bool AnyHostileNearby(Pawn pawn, Map map)
        {
            Faction player = Faction.OfPlayer;
            if (player == null)
            {
                return false;
            }
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn other = pawns[i];
                if (other == pawn || other.Dead || other.Downed || !other.HostileTo(player))
                {
                    continue;
                }
                CompCanBeDormant dormant = other.TryGetComp<CompCanBeDormant>();
                if (dormant != null && !dormant.Awake)
                {
                    continue;
                }
                if (other.Position.InHorDistOf(pawn.Position, HostileCheckRadius))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 哨戒待机唤醒管理器（事件驱动，零轮询）：由 EnemyRaidTracker 在新敌对 Lord 出现
    /// （含"无威胁 -&gt; 有威胁"翻转与已有潜伏威胁时又来真袭击）时回调，
    /// 唤醒本图所有哨戒模式下正在睡眠的机械体（结束 SelfShutdown 作业，下一 tick 由 ThinkTree 分配战斗/工作作业）。
    /// </summary>
    public static class MechStandbyManager
    {
        /// <summary>敌意状态翻转回调（挂在 EnemyRaidTracker.Notify 末尾）。只在变为有敌意时执行唤醒。</summary>
        public static void OnHostileStateChanged(Map map, bool hasHostiles)
        {
            if (map == null || !hasHostiles)
            {
                return;
            }
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (IsStandbySleeping(pawns[i]))
                {
                    pawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }

        /// <summary>哨戒模式下正在睡眠的殖民地机械体（原版 SelfShutdown 作业 + MMT_Sentinel 工作模式）。</summary>
        public static bool IsStandbySleeping(Pawn pawn)
        {
            return pawn != null && pawn.Spawned && pawn.IsColonyMech
                && pawn.CurJobDef == JobDefOf.SelfShutdown
                && pawn.GetMechWorkMode() == MMT_MechStandbyDefOf.MMT_Sentinel;
        }

        /// <summary>单体唤醒（指令面板"立即唤醒"gizmo 用）。</summary>
        public static void Wake(Pawn pawn)
        {
            if (IsStandbySleeping(pawn))
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }

    /// <summary>
    /// 哨戒待机的地图级数据：每台机械体的标定待机点（pawn -&gt; 格子），Scribe 持久化。
    /// 附带低频巡检，清理死亡/离场/不再属于殖民地等失效条目，防止字典泄漏。
    /// </summary>
    public class MapComponent_MechStandby : MapComponent
    {
        /// <summary>巡检间隔（tick），取质数避免与其他周期任务撞车。</summary>
        private const int SweepInterval = 2507;

        private Dictionary<Pawn, IntVec3> standbySpots = new Dictionary<Pawn, IntVec3>();
        private List<Pawn> scribeKeys;
        private List<IntVec3> scribeValues;

        public MapComponent_MechStandby(Map map) : base(map)
        {
        }

        public bool TryGetStandbySpot(Pawn pawn, out IntVec3 spot)
        {
            return standbySpots.TryGetValue(pawn, out spot);
        }

        public void SetStandbySpot(Pawn pawn, IntVec3 cell)
        {
            standbySpots[pawn] = cell;
        }

        public void ClearStandbySpot(Pawn pawn)
        {
            standbySpots.Remove(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref standbySpots, "MMT_mechStandbySpots", LookMode.Reference, LookMode.Value, ref scribeKeys, ref scribeValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && standbySpots == null)
            {
                standbySpots = new Dictionary<Pawn, IntVec3>();
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % SweepInterval != 0)
            {
                return;
            }
            List<Pawn> toRemove = null;
            foreach (KeyValuePair<Pawn, IntVec3> pair in standbySpots)
            {
                Pawn pawn = pair.Key;
                if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.Map != map || !pawn.IsColonyMech)
                {
                    if (toRemove == null)
                    {
                        toRemove = new List<Pawn>();
                    }
                    toRemove.Add(pawn);
                }
            }
            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                {
                    standbySpots.Remove(toRemove[i]);
                }
            }
        }
    }

    /// <summary>
    /// 哨戒待机指令面板注入：给玩家控制的机械体追加"设定待机点 / 清除待机点 / 立即唤醒"gizmo。
    /// "设定待机点"对当前选中的所有玩家机械体批量生效。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_MechStandbyGizmos
    {
        private static Texture2D standbyIcon;

        /// <summary>待机图标（MMT 自绘，1.7/Textures/UI/Icons/MMT_WorkModeSentinel.png）。</summary>
        private static Texture2D StandbyIcon
        {
            get
            {
                if (standbyIcon == null)
                {
                    standbyIcon = ContentFinder<Texture2D>.Get("UI/Icons/MMT_WorkModeSentinel");
                }
                return standbyIcon;
            }
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
        {
            foreach (Gizmo gizmo in values)
            {
                yield return gizmo;
            }
            Pawn mech = __instance;
            if (!mech.IsColonyMechPlayerControlled || !mech.Spawned || mech.Map == null)
            {
                yield break;
            }
            MapComponent_MechStandby comp = mech.Map.GetComponent<MapComponent_MechStandby>();
            if (comp == null)
            {
                yield break;
            }
            yield return new Command_Target
            {
                defaultLabel = "设定待机点",
                defaultDesc = "哨戒待机模式下，闲置的机械体会走到该格子关机睡眠（不再游走浪费性能），有敌人进攻时自动唤醒。\n未设定待机点时在附近自选位置睡眠。\n对当前选中的所有机械体批量生效。",
                icon = StandbyIcon,
                targetingParams = new TargetingParameters
                {
                    canTargetLocations = true,
                    canTargetBuildings = false,
                    canTargetPawns = false
                },
                action = delegate(LocalTargetInfo target)
                {
                    if (!target.IsValid)
                    {
                        return;
                    }
                    int count = 0;
                    foreach (Pawn p in SelectedPlayerMechs(mech))
                    {
                        if (p.Spawned && p.Map == mech.Map)
                        {
                            comp.SetStandbySpot(p, target.Cell);
                            count++;
                        }
                    }
                    Messages.Message("已为 " + count + " 台机械体设定待机点。", MessageTypeDefOf.TaskCompletion, false);
                }
            };
            if (comp.TryGetStandbySpot(mech, out _))
            {
                yield return new Command_Action
                {
                    defaultLabel = "清除待机点",
                    defaultDesc = "清除该机械体的待机点，恢复为在附近自选位置睡眠。",
                    icon = StandbyIcon,
                    action = delegate
                    {
                        comp.ClearStandbySpot(mech);
                    }
                };
            }
            if (MechStandbyManager.IsStandbySleeping(mech))
            {
                yield return new Command_Action
                {
                    defaultLabel = "立即唤醒",
                    defaultDesc = "立即唤醒这台处于哨戒待机睡眠中的机械体。",
                    icon = StandbyIcon,
                    action = delegate
                    {
                        MechStandbyManager.Wake(mech);
                    }
                };
            }
        }

        /// <summary>当前选中的所有玩家机械体；一个都没选中时退回 gizmo 所属的机械体自身。</summary>
        private static IEnumerable<Pawn> SelectedPlayerMechs(Pawn self)
        {
            bool any = false;
            List<object> selected = Find.Selector.SelectedObjects;
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] is Pawn p && p.IsColonyMechPlayerControlled)
                {
                    any = true;
                    yield return p;
                }
            }
            if (!any)
            {
                yield return self;
            }
        }
    }
}
