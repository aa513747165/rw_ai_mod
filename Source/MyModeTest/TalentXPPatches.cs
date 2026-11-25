using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    // 支柱二·全源历练经验（mmt-class-design.md v2，数值参照 Isekai XPPatches 按 MMT 60 级曲线缩到 ~1/4）：
    // 在既有三钩子（伤害/击杀/技能学习）之外补工作/品质/社交/贸易/招募/捕获/灵能系钩子；
    // 全部 postfix、仅玩家派系、走 CompPawnProgress.AddXp（内部统一乘 MMTSettings.XpMultiplier）；
    // 高频来源（灵能熵/集中/冥想）按 pawn 做 2500 tick 间隔冷却，工作按 pawn+JobDef 做 60 tick 冷却。

    /// <summary>XP 冷却与灵能补丁目标的共享工具。</summary>
    public static class TalentXpUtil
    {
        private static readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>();

        /// <summary>冷却检查：key 首次或距上次超过 interval tick 返回 true 并刷新。</summary>
        public static bool PassCooldown(string key, int interval)
        {
            int now = Find.TickManager?.TicksGame ?? 0;
            if (cooldowns.TryGetValue(key, out int last) && now - last < interval)
            {
                return false;
            }
            if (cooldowns.Count > 4000)
            {
                cooldowns.Clear();
            }
            cooldowns[key] = now;
            return true;
        }

        /// <summary>击杀加成：按 combatPower 分档（简化定级，仅供 XP 与任务选怪用）。</summary>
        public static float KillBonusXp(Pawn victim)
        {
            float cp = victim?.kindDef?.combatPower ?? 0f;
            if (cp < 35f) return 2f;
            if (cp < 55f) return 5f;
            if (cp < 80f) return 8f;
            if (cp < 110f) return 15f;
            if (cp < 150f) return 25f;
            if (cp < 220f) return 40f;
            if (cp < 300f) return 65f;
            if (cp < 600f) return 100f;
            return 160f;
        }
    }

    /// <summary>工作完成 XP：按 JobDef 分档（手术走 bill 配方判定），同 pawn 同 JobDef 60 tick 冷却。
    /// Prefix/Postfix 间经 __state 传值（不用静态字段：EndCurrentJob 可能嵌套触发）。</summary>
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
    public static class Patch_TalentJobXp
    {
        /// <summary>工作 XP 表（JobDef defName → XP）。</summary>
        private static readonly Dictionary<string, float> JobXpTable = new Dictionary<string, float>
        {
            { "DoBill", 8f },
            { "Research", 8f },
            { "Mine", 6f },
            { "ConstructFinishFrame", 4f },
            { "Harvest", 4f },
            { "HarvestDesignated", 4f },
            { "Sow", 2f },
            { "CutPlant", 2f },
            { "CutPlantDesignated", 2f },
            { "HaulToCell", 2f },
            { "HaulToContainer", 2f },
            { "Clean", 1f },
            { "Tame", 15f },
            { "Train", 8f },
            { "Slaughter", 5f },
            { "Milk", 3f },
            { "Shear", 3f },
            { "DoctorTendTo", 18f },
            { "DoctorTendToEmergency", 18f },
        };

        public static void Prefix(Pawn_JobTracker __instance, out JobXpState __state)
        {
            Job job = __instance?.curJob;
            __state = new JobXpState
            {
                jobDef = job?.def?.defName,
                wasSurgery = job?.bill?.recipe != null && job.bill.recipe.IsSurgery,
            };
        }

        public static void Postfix(JobCondition condition, Pawn ___pawn, JobXpState __state)
        {
            if (!MMTSettings.PawnProgressEnabled || condition == JobCondition.Ongoing || __state.jobDef == null)
            {
                return;
            }
            float amount = __state.wasSurgery ? 25f : JobXpTable.TryGetValue(__state.jobDef, out float v) ? v : 0f;
            if (amount <= 0f || !TalentXpUtil.PassCooldown(___pawn?.thingIDNumber + ":" + __state.wasSurgery, 60))
            {
                return;
            }
            CompPawnProgress.Get(___pawn)?.AddXp(amount);
        }

        /// <summary>__state 载体（struct 免堆分配）。</summary>
        public struct JobXpState
        {
            public string jobDef;
            public bool wasSurgery;
        }
    }

    /// <summary>品质产出 XP：糟糕 2 → 传奇 75（与品质暴击补丁并存）。
    /// 1.7 方法签名：GenerateQualityCreatedByPawn(Pawn, SkillDef, bool consumeInspiration)。</summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_TalentQualityXp
    {
        public static void Postfix(Pawn pawn, QualityCategory __result)
        {
            if (!MMTSettings.PawnProgressEnabled)
            {
                return;
            }
            float amount;
            switch (__result)
            {
                case QualityCategory.Awful: amount = 2f; break;
                case QualityCategory.Poor: amount = 5f; break;
                case QualityCategory.Normal: amount = 10f; break;
                case QualityCategory.Good: amount = 18f; break;
                case QualityCategory.Excellent: amount = 30f; break;
                case QualityCategory.Masterwork: amount = 50f; break;
                case QualityCategory.Legendary: amount = 75f; break;
                default: return;
            }
            CompPawnProgress.Get(pawn)?.AddXp(amount);
        }
    }

    /// <summary>社交互动成功 XP：2。</summary>
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class Patch_TalentSocialXp
    {
        public static void Postfix(bool __result, Pawn ___pawn)
        {
            if (__result && MMTSettings.PawnProgressEnabled)
            {
                CompPawnProgress.Get(___pawn)?.AddXp(2f);
            }
        }
    }

    /// <summary>贸易达成 XP：谈判者 15。</summary>
    [HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
    public static class Patch_TalentTradeXp
    {
        public static void Postfix(bool __result)
        {
            if (__result && MMTSettings.PawnProgressEnabled)
            {
                CompPawnProgress.Get(TradeSession.playerNegotiator)?.AddXp(15f);
            }
        }
    }

    /// <summary>招募成功 XP：招募者 38（postfix 看目标是否已变殖民者）。</summary>
    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.Interacted))]
    public static class Patch_TalentRecruitXp
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            if (!MMTSettings.PawnProgressEnabled || recipient?.guest == null)
            {
                return;
            }
            if (!recipient.IsPrisoner && recipient.Faction != null && recipient.Faction.IsPlayer)
            {
                CompPawnProgress.Get(initiator)?.AddXp(38f);
            }
        }
    }

    /// <summary>捕获 XP：目标变囚犯时 12 格内殖民者各 6。</summary>
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
    public static class Patch_TalentCaptureXp
    {
        public static void Postfix(Pawn_GuestTracker __instance, GuestStatus guestStatus, Pawn ___pawn)
        {
            if (!MMTSettings.PawnProgressEnabled || guestStatus != GuestStatus.Prisoner || ___pawn?.Map == null)
            {
                return;
            }
            foreach (Pawn colonist in ___pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (colonist.Position.DistanceTo(___pawn.Position) <= 12f)
                {
                    CompPawnProgress.Get(colonist)?.AddXp(6f);
                }
            }
        }
    }

    /// <summary>击杀加成 XP：按目标 combatPower 分档（叠加在既有击杀 XP 上）。</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_TalentKillBonusXp
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (!MMTSettings.PawnProgressEnabled || dinfo == null)
            {
                return;
            }
            CompPawnProgress.Get(dinfo.Value.Instigator as Pawn)?.AddXp(TalentXpUtil.KillBonusXp(__instance));
        }
    }

    /// <summary>灵能释放 XP：基础 5 + 熵耗 ×1，钳 3-50；同 pawn 2500 tick 冷却。</summary>
    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate), typeof(LocalTargetInfo), typeof(LocalTargetInfo))]
    public static class Patch_TalentAbilityXp
    {
        public static void Postfix(Ability __instance)
        {
            if (!MMTSettings.PawnProgressEnabled || __instance?.pawn == null || !TalentXpUtil.PassCooldown(__instance.pawn.thingIDNumber + ":ability", 2500))
            {
                return;
            }
            // 1.5+ 技能消耗走 StatDef（与 MMT 技能 XML 的 Ability_EntropyGain 字段一致）
            float entropy = __instance.def != null ? __instance.def.GetStatValueAbstract(StatDefOf.Ability_EntropyGain) : 0f;
            float amount = Mathf.Clamp(5f + entropy, 3f, 50f);
            CompPawnProgress.Get(__instance.pawn)?.AddXp(amount);
        }
    }

    /// <summary>熵累积 XP：Δ熵 ×0.25 钳 2-25（Royalty 缺席时 Prepare 跳过补丁）。</summary>
    [HarmonyPatch]
    public static class Patch_TalentEntropyXp
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "TryAddEntropy") != null;
        }

        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "TryAddEntropy");
        }

        // 1.7 参数名由 amount 改为 value（Harmony 按名绑定，名字错了补丁注册失败）
        public static void Postfix(Pawn_PsychicEntropyTracker __instance, float value, Pawn ___pawn)
        {
            if (!MMTSettings.PawnProgressEnabled || value <= 0f || !TalentXpUtil.PassCooldown(___pawn?.thingIDNumber + ":entropy", 2500))
            {
                return;
            }
            CompPawnProgress.Get(___pawn)?.AddXp(Mathf.Clamp(value * 0.25f, 2f, 25f));
        }
    }

    /// <summary>集中消耗 XP：消耗 ×15 钳 1-20（Royalty 缺席时跳过）。</summary>
    [HarmonyPatch]
    public static class Patch_TalentPsyfocusSpendXp
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "OffsetPsyfocusDirectly") != null;
        }

        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "OffsetPsyfocusDirectly");
        }

        public static void Postfix(float offset, Pawn ___pawn)
        {
            if (!MMTSettings.PawnProgressEnabled || offset >= 0f || !TalentXpUtil.PassCooldown(___pawn?.thingIDNumber + ":psyfocus", 2500))
            {
                return;
            }
            CompPawnProgress.Get(___pawn)?.AddXp(Mathf.Clamp(-offset * 15f, 1f, 20f));
        }
    }

    /// <summary>冥想 XP：每 2500 tick 4（Royalty 缺席时跳过）。</summary>
    [HarmonyPatch]
    public static class Patch_TalentMeditationXp
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "GainPsyfocus") != null;
        }

        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Pawn_PsychicEntropyTracker), "GainPsyfocus");
        }

        public static void Postfix(Pawn ___pawn)
        {
            if (!MMTSettings.PawnProgressEnabled || !TalentXpUtil.PassCooldown(___pawn?.thingIDNumber + ":meditate", 2500))
            {
                return;
            }
            CompPawnProgress.Get(___pawn)?.AddXp(4f);
        }
    }
}
