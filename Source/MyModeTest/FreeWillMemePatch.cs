using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 「自由意志」模因（MMT_FreeWill）的职位心情惩罚屏蔽补丁。
    /// 文化含该模因时：
    ///   1. 领袖/传道者空缺不再产生 IdeoRoleEmpty（-5）想法；
    ///   2. 任职者职位服装要求未满足不再产生 IdeoRoleApparelRequirementNotMet（-4）想法；
    ///   3. 右侧"职位未分配"警报（Alert_RolesEmpty）不再报告本文化的空缺职位。
    /// 两个想法分别由 Thought_IdeoRoleEmpty / Thought_IdeoRoleApparelRequirementNotMet 的
    /// CurrentStateInternal 求值，postfix 只在结果为 Active 时按模因翻转为 Inactive，
    /// 保留原版全部判定（奴隶豁免、10 天宽限、非主文化领袖豁免），幂等且可与其他 Mod 叠加。
    /// 职位本身由 C# 强制加入每个文化（classic/canRemoveInUI），无法纯 XML 移除，
    /// 因此采用"职位可留空壳、惩罚屏蔽"方案。
    /// </summary>
    internal static class FreeWillMeme
    {
        private static MemeDef cached;
        private static bool resolved;
        private static bool warnedMissing;

        /// <summary>懒加载 MMT_FreeWill 模因 def；缺失时警告一次并让补丁静默不生效（安全降级）。</summary>
        public static MemeDef Def
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    cached = DefDatabase<MemeDef>.GetNamedSilentFail("MMT_FreeWill");
                    if (cached == null && !warnedMissing)
                    {
                        warnedMissing = true;
                        Log.Warning("[MMT] 未找到模因 MMT_FreeWill，自由意志补丁不生效");
                    }
                }
                return cached;
            }
        }

        public static bool ActiveFor(Pawn pawn)
        {
            return pawn != null && ActiveForIdeo(pawn.Ideo);
        }

        public static bool ActiveForIdeo(Ideo ideo)
        {
            return ideo != null && Def != null && ideo.HasMeme(Def);
        }

        /// <summary>职位心情想法求值后置处理：本文化信徒的激活惩罚翻转为 Inactive。</summary>
        public static void SuppressIfFreeWill(Thought thought, ref ThoughtState result, string tag)
        {
            try
            {
                if (result.Active && ActiveFor(thought.pawn))
                {
                    result = ThoughtState.Inactive;
                }
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 自由意志补丁(" + tag + ")降级: " + e.Message);
            }
        }
    }

    /// <summary>职位空缺想法（-5）：自由意志文化不强制任命领袖/传道者。</summary>
    [HarmonyPatch(typeof(Thought_IdeoRoleEmpty), "CurrentStateInternal")]
    public static class FreeWillPatch_RoleEmpty
    {
        // 目标方法缺失（未来版本签名变动）时整个补丁跳过，只恢复原惩罚
        public static bool Prepare()
        {
            return AccessTools.DeclaredMethod(typeof(Thought_IdeoRoleEmpty), "CurrentStateInternal") != null;
        }

        public static void Postfix(Thought __instance, ref ThoughtState __result)
        {
            FreeWillMeme.SuppressIfFreeWill(__instance, ref __result, "职位空缺");
        }
    }

    /// <summary>职位服装要求未满足想法（-4）：自由意志文化不追究任职者着装。</summary>
    [HarmonyPatch(typeof(Thought_IdeoRoleApparelRequirementNotMet), "CurrentStateInternal")]
    public static class FreeWillPatch_RoleApparel
    {
        public static bool Prepare()
        {
            return AccessTools.DeclaredMethod(typeof(Thought_IdeoRoleApparelRequirementNotMet), "CurrentStateInternal") != null;
        }

        public static void Postfix(Thought __instance, ref ThoughtState __result)
        {
            FreeWillMeme.SuppressIfFreeWill(__instance, ref __result, "职位服装");
        }
    }

    /// <summary>
    /// 职位未分配警报（屏幕右侧 Alert_RolesEmpty）：剔除自由意志文化的空缺职位，
    /// 并按原版逻辑重建目标列表（GetLabel/GetExplanation 也复用这些列表，一并生效）。
    /// 私有字段用 Traverse 反射访问，全部包在 try/catch 里，失败仅恢复警报不崩。
    /// </summary>
    [HarmonyPatch(typeof(Alert_RolesEmpty), "GetTargets")]
    public static class FreeWillPatch_RoleEmptyAlert
    {
        public static bool Prepare()
        {
            return AccessTools.DeclaredMethod(typeof(Alert_RolesEmpty), "GetTargets") != null;
        }

        public static void Postfix(Alert_RolesEmpty __instance)
        {
            try
            {
                if (FreeWillMeme.Def == null)
                {
                    return;
                }
                Traverse tr = Traverse.Create(__instance);
                List<Precept_Role> emptyRoles = tr.Field("emptyRoles").GetValue<List<Precept_Role>>();
                if (emptyRoles == null || emptyRoles.Count == 0)
                {
                    return;
                }
                // 剔除自由意志文化的空缺职位；没剔除则目标列表无需重建
                int removed = emptyRoles.RemoveAll(r => r != null && FreeWillMeme.ActiveForIdeo(r.ideo));
                if (removed == 0)
                {
                    return;
                }
                List<GlobalTargetInfo> targets = tr.Field("targets").GetValue<List<GlobalTargetInfo>>();
                List<string> labels = tr.Field("targetLabels").GetValue<List<string>>();
                if (targets == null || labels == null)
                {
                    return;
                }
                // 复制原版填充逻辑：当前地图自由殖民者、非奴隶、其文化有空缺职位
                targets.Clear();
                labels.Clear();
                Map map = Find.CurrentMap;
                if (map == null)
                {
                    return;
                }
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.IsSlave)
                    {
                        continue;
                    }
                    if (emptyRoles.Any(r => p.Ideo == r.ideo))
                    {
                        targets.Add(p);
                        labels.Add(p.NameFullColored.Resolve());
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 自由意志补丁(职位警报)降级: " + e.Message);
            }
        }
    }
}
