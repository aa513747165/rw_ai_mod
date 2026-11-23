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
    /// 派系关系翻转时维护每张地图的敌对 Lord 计数，计数在 0 与非 0 间跨越时通知本图所有感应开关刷新，
    /// 并回调哨戒待机唤醒管理器（MechStandbyManager.OnHostileStateChanged）唤醒睡眠中的哨戒机械体。
    /// 已计数的 Lord 记录于集合中，避免解散时因成员已清空而误判为非敌对导致计数泄漏。
    /// </summary>
    public static class EnemyRaidTracker
    {
        private static readonly Dictionary<Map, int> counts = new Dictionary<Map, int>();
        private static readonly HashSet<Lord> countedLords = new HashSet<Lord>();
        private static readonly Dictionary<Map, List<CompEnemyDetector>> switches = new Dictionary<Map, List<CompEnemyDetector>>();
        /// <summary>已做过全图重扫的地图（惰性初始化，每图一次）。</summary>
        private static readonly HashSet<Map> scannedMaps = new HashSet<Map>();

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
            EnsureMapScanned(map);
        }

        /// <summary>
        /// 惰性重扫本图 Lord 计数：静态计数不入档，且地图生成瞬间休眠单位尚未进入休眠 job
        /// 可能被误计，注册开关时以当前真实状态重扫一次最稳妥。
        /// </summary>
        private static void EnsureMapScanned(Map map)
        {
            if (!scannedMaps.Add(map))
            {
                return;
            }
            // 先清掉本图可能存在的陈旧计数，再以 IsHostile 重算
            countedLords.RemoveWhere(l => l.Map == map);
            counts.Remove(map);
            int count = 0;
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                if (IsHostile(lords[i]))
                {
                    countedLords.Add(lords[i]);
                    count++;
                }
            }
            if (count > 0)
            {
                counts[map] = count;
            }
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
                // 新敌对 Lord 出现即唤醒本图哨戒睡眠的机械体：不能只在计数 0->1 翻转时唤醒
                //（Increment 内的 Notify），否则地图上已存在潜伏型威胁（如地图生成的血肉兽
                // 突袭 Lord）占用计数后，真正的袭击不会触发翻转，睡眠的机械体永远不会被唤醒。
                MechStandbyManager.OnHostileStateChanged(lord.Map, true);
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
            scannedMaps.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                scannedMaps.Add(map);
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
        /// 判定 Lord 是否为敌对进攻事件：任一在场存活的敌对成员且未处于休眠（远古遗迹/休眠
        /// 机械集群的 CompCanBeDormant 未唤醒不算进攻，惊醒时经 Notify_DormancyWakeup 补丁重新计入）。
        /// 用成员判定而非仅看派系，可覆盖无派系的猎杀人类兽群（manhunter）等场景。
        /// 全部成员未进场（袭击刚创建、空投途中）时才退回派系敌对判定；只要有成员在场，
        /// 即使全部阵亡或全部休眠，都按在场状态判定，避免休眠威胁/尸体残留导致误判常开。
        /// </summary>
        private static bool IsHostile(Lord lord)
        {
            Faction player = Faction.OfPlayer;
            if (lord == null || player == null)
            {
                return false;
            }
            bool anySpawned = false;
            List<Pawn> pawns = lord.ownedPawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!pawn.Spawned)
                {
                    continue;
                }
                anySpawned = true;
                if (!pawn.Dead && pawn.HostileTo(player) && IsAwake(pawn))
                {
                    return true;
                }
            }
            // 成员尚未进场（如袭击 Lord 刚创建、部队正在空投途中）时按派系敌对判定
            return !anySpawned && lord.faction != null && lord.faction.HostileTo(player);
        }

        /// <summary>休眠威胁（远古遗迹/休眠机械集群）唤醒前不计为进攻。</summary>
        private static bool IsAwake(Pawn pawn)
        {
            CompCanBeDormant dormant = pawn.TryGetComp<CompCanBeDormant>();
            return dormant == null || dormant.Awake;
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
            if (map == null)
            {
                return;
            }
            // 哨戒待机（MechStandby.cs）：敌意状态翻转时唤醒本图哨戒睡眠的机械体。
            // 必须在感应开关列表判断之前调用：本图没建感应开关时机械体唤醒也不能被跳过。
            MechStandbyManager.OnHostileStateChanged(map, HasHostileLord(map));
            if (!switches.TryGetValue(map, out List<CompEnemyDetector> list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].RefreshPower();
            }
        }
    }

    /// <summary>补丁：休眠威胁惊醒（远古遗迹/休眠机械集群惊醒时立即重估该 Lord）。</summary>
    [HarmonyPatch(typeof(Lord), nameof(Lord.Notify_DormancyWakeup))]
    public static class Patch_Lord_NotifyDormancyWakeup
    {
        public static void Postfix(Lord __instance)
        {
            EnemyRaidTracker.SyncLord(__instance);
        }
    }

    /// <summary>补丁：开局/读档完成后全量重建计数（跟踪器为静态内存数据，不入档）。</summary>
    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Patch_Game_FinalizeInit
    {
        public static void Postfix()
        {
            EnemyRaidTracker.ReevaluateAll();
        }
    }

    /// <summary>补丁：Lord 建立（袭击/围攻/兽群等进攻事件开始）。</summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.AddLord))]
    public static class Patch_LordManager_AddLord
    {
        // 1.7 该参数名为 newLord（Harmony 按参数名绑定，写错会在 PatchAll 时抛异常）
        public static void Postfix(Lord newLord)
        {
            EnemyRaidTracker.SyncLord(newLord);
        }
    }

    /// <summary>补丁：Lord 解散（进攻事件结束；前缀执行以保留 Lord 的地图信息）。</summary>
    [HarmonyPatch(typeof(LordManager), nameof(LordManager.RemoveLord))]
    public static class Patch_LordManager_RemoveLord
    {
        // 1.7 该参数名为 oldLord（Harmony 按参数名绑定）
        public static void Prefix(Lord oldLord)
        {
            EnemyRaidTracker.OnLordRemoved(oldLord);
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

    /// <summary>
    /// 补丁：派系关系翻转（好感度跌破敌对阈值、中途议和等），全量重建计数。
    /// 注：1.7 中好感度变化与直接改关系都不经过 Faction.SetRelation，统一收口于
    /// Notify_RelationKindChanged（CheckKindThresholds / SetRelationDirect 均调它），故补丁挂此方法。
    /// </summary>
    [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_RelationKindChanged))]
    public static class Patch_Faction_NotifyRelationKindChanged
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
