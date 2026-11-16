using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MyModeTest
{
    /// <summary>
    /// 敌意感应开关 Comp：地图上存在敌对派系的 Lord（袭击/围攻/兽群/虫灾等进攻事件）时自动通电，
    /// 进攻全部结束后自动断电。提供 Gizmo 在 自动/强制通电/强制断电 三档间切换。
    /// </summary>
    public class CompEnemyDetector : ThingComp
    {
        /// <summary>工作模式：自动感应 / 强制通电 / 强制断电。</summary>
        public enum SwitchMode
        {
            Auto,
            ForceOn,
            ForceOff
        }

        private SwitchMode mode = SwitchMode.Auto;
        private CompFlickable flickable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickable = parent.GetComp<CompFlickable>();
            // 注册到本图跟踪器，之后由事件通知驱动刷新，不做轮询
            EnemyRaidTracker.Register(parent.Map, this);
            RefreshPower();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            EnemyRaidTracker.Unregister(map, this);
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mode, "MMT_enemySwitchMode", SwitchMode.Auto);
        }

        /// <summary>按当前模式计算并应用通断状态，仅状态变化时写入（避免反复触发电网重建）。</summary>
        public void RefreshPower()
        {
            if (flickable == null || parent.Map == null)
            {
                return;
            }
            bool on;
            switch (mode)
            {
                case SwitchMode.ForceOn:
                    on = true;
                    break;
                case SwitchMode.ForceOff:
                    on = false;
                    break;
                default:
                    on = EnemyRaidTracker.HasHostileLord(parent.Map);
                    break;
            }
            if (flickable.SwitchIsOn != on)
            {
                flickable.SwitchIsOn = on;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            // 节点切换：自动 -> 强制通电 -> 强制断电 -> 自动
            yield return new Command_Action
            {
                defaultLabel = "感应模式：" + ModeLabel(),
                defaultDesc = "自动：有进攻事件时通电、结束后断电。\n强制通电：忽略感应保持通电。\n强制断电：忽略感应保持断电。",
                icon = parent.def.uiIcon,
                action = delegate
                {
                    mode = (SwitchMode)(((int)mode + 1) % 3);
                    RefreshPower();
                }
            };
        }

        private string ModeLabel()
        {
            switch (mode)
            {
                case SwitchMode.ForceOn: return "强制通电";
                case SwitchMode.ForceOff: return "强制断电";
                default: return "自动";
            }
        }
    }

    /// <summary>感应开关的 CompProperties，仅用于挂上 <see cref="CompEnemyDetector"/>。</summary>
    public class CompProperties_EnemyDetector : CompProperties
    {
        public CompProperties_EnemyDetector()
        {
            compClass = typeof(CompEnemyDetector);
        }
    }

    /// <summary>
    /// 敌对 Lord 跟踪器（事件驱动，零轮询）：由 Harmony 补丁在 Lord 建立/解散、成员进出、
    /// 派系关系翻转时维护每张地图的敌对 Lord 计数，计数在 0 与非 0 间跨越时通知本图所有感应开关刷新。
    /// 已计数的 Lord 记录于集合中，避免解散时因成员已清空而误判为非敌对导致计数泄漏。
    /// </summary>
    public static class EnemyRaidTracker
    {
        private static readonly Dictionary<Map, int> counts = new Dictionary<Map, int>();
        private static readonly HashSet<Lord> countedLords = new HashSet<Lord>();
        private static readonly Dictionary<Map, List<CompEnemyDetector>> switches = new Dictionary<Map, List<CompEnemyDetector>>();

        public static bool HasHostileLord(Map map)
        {
            return map != null && counts.TryGetValue(map, out int count) && count > 0;
        }

        public static void Register(Map map, CompEnemyDetector comp)
        {
            if (map == null)
            {
                return;
            }
            if (!switches.TryGetValue(map, out List<CompEnemyDetector> list))
            {
                list = new List<CompEnemyDetector>();
                switches[map] = list;
            }
            list.Add(comp);
        }

        public static void Unregister(Map map, CompEnemyDetector comp)
        {
            if (map != null && switches.TryGetValue(map, out List<CompEnemyDetector> list))
            {
                list.Remove(comp);
            }
        }

        /// <summary>事件入口：Lord 建立或成员变化后，按当前敌对状态同步其计数身份。</summary>
        public static void SyncLord(Lord lord)
        {
            if (lord?.Map == null)
            {
                return;
            }
            bool hostile = IsHostile(lord);
            bool counted = countedLords.Contains(lord);
            if (hostile == counted)
            {
                return;
            }
            if (hostile)
            {
                countedLords.Add(lord);
                Increment(lord.Map);
            }
            else
            {
                countedLords.Remove(lord);
                Decrement(lord.Map);
            }
        }

        /// <summary>事件入口：Lord 解散（前缀调用，此时 Lord 仍持有地图与成员信息）。</summary>
        public static void OnLordRemoved(Lord lord)
        {
            if (lord != null && countedLords.Remove(lord))
            {
                Decrement(lord.Map);
            }
        }

        /// <summary>事件入口：派系关系翻转，全量重建计数（该事件极罕见，直接重算最稳妥）。</summary>
        public static void ReevaluateAll()
        {
            countedLords.Clear();
            counts.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                List<Lord> lords = map.lordManager.lords;
                for (int j = 0; j < lords.Count; j++)
                {
                    if (IsHostile(lords[j]))
                    {
                        countedLords.Add(lords[j]);
                        counts.TryGetValue(map, out int c);
                        counts[map] = c + 1;
                    }
                }
            }
            // 全量通知，让各开关按最新状态刷新
            foreach (KeyValuePair<Map, List<CompEnemyDetector>> pair in switches)
            {
                Notify(pair.Key);
            }
        }

        /// <summary>
        /// 判定 Lord 是否为敌对进攻事件：任一在场存活成员与玩家敌对即可。
        /// 用成员判定而非仅看派系，可覆盖无派系的猎杀人类兽群（manhunter）等场景。
        /// </summary>
        private static bool IsHostile(Lord lord)
        {
            Faction player = Faction.OfPlayer;
            if (lord == null || player == null)
            {
                return false;
            }
            List<Pawn> pawns = lord.ownedPawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn.Spawned && !pawn.Dead && pawn.HostileTo(player))
                {
                    return true;
                }
            }
            // 成员尚未进场（如袭击 Lord 刚创建、部队正在空投途中）时按派系敌对判定
            return lord.faction != null && lord.faction.HostileTo(player);
        }

        private static void Increment(Map map)
        {
            counts.TryGetValue(map, out int c);
            counts[map] = c + 1;
            // 状态流转：无威胁 -> 有威胁
            if (c == 0)
            {
                Notify(map);
            }
        }

        private static void Decrement(Map map)
        {
            if (map == null || !counts.TryGetValue(map, out int c) || c <= 0)
            {
                return;
            }
            counts[map] = c - 1;
            // 状态流转：有威胁 -> 无威胁
            if (c == 1)
            {
                Notify(map);
            }
        }

        private static void Notify(Map map)
        {
            if (map == null || !switches.TryGetValue(map, out List<CompEnemyDetector> list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].RefreshPower();
            }
        }
    }

    /// <summary>补丁：Lord 建立（袭击/围攻/兽群等进攻事件开始）。</summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.AddLord))]
    public static class Patch_LordManager_AddLord
    {
        public static void Postfix(Lord lord)
        {
            EnemyRaidTracker.SyncLord(lord);
        }
    }

    /// <summary>补丁：Lord 解散（进攻事件结束；前缀执行以保留 Lord 的地图信息）。</summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.RemoveLord))]
    public static class Patch_LordManager_RemoveLord
    {
        public static void Prefix(Lord lord)
        {
            EnemyRaidTracker.OnLordRemoved(lord);
        }
    }

    /// <summary>补丁：成员加入 Lord（覆盖无派系兽群等 AddLord 时成员尚未入场的情况）。</summary>
    [HarmonyPatch(typeof(Lord), nameof(Lord.AddPawn))]
    public static class Patch_Lord_AddPawn
    {
        public static void Postfix(Lord __instance)
        {
            EnemyRaidTracker.SyncLord(__instance);
        }
    }

    /// <summary>补丁：成员离开 Lord（如兽群成员被逐个击杀但 Lord 尚未解散时重新评估）。</summary>
    [HarmonyPatch(typeof(Lord), nameof(Lord.Notify_PawnLost))]
    public static class Patch_Lord_NotifyPawnLost
    {
        public static void Postfix(Lord __instance)
        {
            EnemyRaidTracker.SyncLord(__instance);
        }
    }

    /// <summary>补丁：派系关系翻转（好感度跌破敌对阈值、中途议和等），全量重建计数。</summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.SetRelation))]
    public static class Patch_Faction_SetRelation
    {
        public static void Postfix()
        {
            if (Current.ProgramState == ProgramState.Playing)
            {
                EnemyRaidTracker.ReevaluateAll();
            }
        }
    }
}
