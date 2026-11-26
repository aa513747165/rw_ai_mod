using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    // 生命备份链运行时逻辑:
    // 植入物(HediffComp_StateBackup)在安装时自动备份宿主完整健康状态快照,
    // 通过 Royalty 技能(CompAbilityEffect_SaveBackup)可随时刷新备份点;
    // Harmony 补丁拦截 Pawn.Kill,有备份时取消死亡、销毁植入物并把健康状态回档到备份点。
    // 备份一次性消耗,触发后需重新手术植入才能再次获得能力。

    /// <summary>
    /// 单条 Hediff 快照:只记录 def/身体部位/严重度三项。
    /// 部位用"部位定义 + 同类部位序号"存储,避免跨存档的 BodyPartRecord 引用问题。
    /// 治疗质量、感染进度等附加状态不保留(完整恢复实现成本过高,属可接受简化)。
    /// </summary>
    public class HediffSnapshot : IExposable
    {
        public HediffDef def;
        public BodyPartDef partDef;
        public int partIndex = -1;
        public float severity = -1f;

        /// <summary>从当前 Hediff 生成快照。</summary>
        public static HediffSnapshot From(Hediff hediff)
        {
            HediffSnapshot snap = new HediffSnapshot { def = hediff.def, severity = hediff.Severity };
            if (hediff.Part != null)
            {
                snap.partDef = hediff.Part.def;
                snap.partIndex = hediff.pawn.RaceProps.body.GetPartsWithDef(hediff.Part.def).IndexOf(hediff.Part);
            }
            return snap;
        }

        /// <summary>在目标小人身上按快照重建 Hediff 并返回(部位序号失效时退化为无部位)。</summary>
        public Hediff RebuildOn(Pawn pawn)
        {
            BodyPartRecord part = null;
            if (partDef != null && partIndex >= 0)
            {
                List<BodyPartRecord> parts = pawn.RaceProps.body.GetPartsWithDef(partDef);
                if (partIndex < parts.Count)
                {
                    part = parts[partIndex];
                }
            }
            Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
            if (severity >= 0f)
            {
                hediff.Severity = severity;
            }
            return hediff;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref partDef, "partDef");
            Scribe_Values.Look(ref partIndex, "partIndex", -1);
            Scribe_Values.Look(ref severity, "severity", -1f);
        }
    }

    /// <summary>生命备份植入物参数。</summary>
    public class HediffCompProperties_StateBackup : HediffCompProperties
    {
        public HediffCompProperties_StateBackup()
        {
            compClass = typeof(HediffComp_StateBackup);
        }
    }

    /// <summary>
    /// 生命备份植入物:安装时自动备份宿主全部 Hediff 快照(排除植入物自身),
    /// 死亡拦截命中时由 Patch_PawnKill 调用 Restore() 回档健康状态。
    /// </summary>
    public class HediffComp_StateBackup : HediffComp
    {
        private List<HediffSnapshot> backupHediffs = new List<HediffSnapshot>();
        private bool hasBackup;
        private int backupTick = -1;

        public bool HasBackup => hasBackup;

        /// <summary>查找小人身上的备份 comp(没有则为 null)。</summary>
        public static HediffComp_StateBackup FindOnPawn(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return null;
            }
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is HediffWithComps withComps)
                {
                    HediffComp_StateBackup comp = withComps.TryGetComp<HediffComp_StateBackup>();
                    if (comp != null)
                    {
                        return comp;
                    }
                }
            }
            return null;
        }

        /// <summary>手术植入完成:自动备份当前状态。</summary>
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            Backup();
        }

        /// <summary>备份(刷新)当前健康状态快照。</summary>
        public void Backup()
        {
            backupHediffs.Clear();
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs)
            {
                // 排除植入物自身:复活时植入物已销毁,不能随快照被加回
                if (hediff.def == parent.def)
                {
                    continue;
                }
                backupHediffs.Add(HediffSnapshot.From(hediff));
            }
            hasBackup = true;
            backupTick = Find.TickManager?.TicksGame ?? 0;
            if (Pawn.Faction != null && Pawn.Faction.IsPlayer)
            {
                Messages.Message(Pawn.LabelShort + "的生命状态已备份(共 " + backupHediffs.Count + " 项体征记录)。",
                    Pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        /// <summary>
        /// 回档健康状态到备份点:清空当前全部 Hediff(缺失肢体随 Hediff_MissingPart 移除而恢复),
        /// 再按快照重建。调用前植入物本身应已被移除。备份随之消耗。
        /// </summary>
        public void Restore()
        {
            // 流程开始:清空当前健康状态
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs.ToList())
            {
                Pawn.health.RemoveHediff(hediff);
            }
            // 节点流转:按快照重建备份时点的健康状态
            foreach (HediffSnapshot snap in backupHediffs)
            {
                Pawn.health.AddHediff(snap.RebuildOn(Pawn));
            }
            // 流程结束:解除强制倒地,健康状态交由下一 tick 重新计算
            Pawn.health.forceDowned = false;
            hasBackup = false;
            backupHediffs.Clear();
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref hasBackup, "hasBackup");
            Scribe_Values.Look(ref backupTick, "backupTick", -1);
            Scribe_Collections.Look(ref backupHediffs, "backupHediffs", LookMode.Deep);
            if (backupHediffs == null)
            {
                backupHediffs = new List<HediffSnapshot>();
            }
        }
    }

    /// <summary>技能"状态备份"效果:刷新施法者的健康状态快照。</summary>
    public class CompAbilityEffect_SaveBackup : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            HediffComp_StateBackup comp = HediffComp_StateBackup.FindOnPawn(parent.pawn);
            if (comp != null)
            {
                comp.Backup();
            }
        }
    }

    /// <summary>
    /// Harmony 补丁:拦截 Pawn.Kill。有可用备份时取消死亡,
    /// 销毁植入物并把宿主健康状态回档到备份点(处决/手术失败等一切致死路径均生效)。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_PawnKill
    {
        public static bool Prefix(Pawn __instance)
        {
            Pawn pawn = __instance;
            if (pawn.Dead || pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
            {
                return true;
            }
            HediffComp_StateBackup comp = HediffComp_StateBackup.FindOnPawn(pawn);
            if (comp == null || !comp.HasBackup)
            {
                return true;
            }
            // 流程开始:先销毁植入物(不回档植入物自身),再恢复备份
            pawn.health.RemoveHediff(comp.parent);
            comp.Restore();
            // 流程结束:死亡被取消,发出提示
            Messages.Message(pawn.LabelShort + "遭遇致命伤害,生命备份核心已烧毁:身体回溯到了备份时点的状态!",
                pawn, MessageTypeDefOf.PositiveEvent);
            if (pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                Find.LetterStack.ReceiveLetter("生命备份触发",
                    pawn.LabelShort + " 在致命伤害下被生命备份核心救回,健康状态已回溯到备份时点。核心已烧毁,如需再次获得保护请重新手术植入。",
                    LetterDefOf.PositiveEvent, new LookTargets(pawn));
            }
            return false;
        }
    }
}
