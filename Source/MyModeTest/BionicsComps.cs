using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // 仿生链(CONN/CyberneticOrganism_SK 移植)自定义运行时逻辑:
    // 狂战士芯片手动狂暴、精神扭曲芯片思想清除、缪斯芯片娱乐供给、手电照明 mote,
    // 以及六种神经调制使用品的效果。数值与行为对照 CONN 原版实现简化重实现。

    /// <summary>
    /// 狂战士芯片:在健康面板提供"激活狂暴"按钮,让植入者立即进入狂暴状态。
    /// 对照 CONN.HediffGizmoBerserk。
    /// </summary>
    public class HediffBerserkGizmo : HediffWithComps
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            // 仅玩家殖民者、未倒地、未处于狂暴精神状态下可用
            if (pawn != null && pawn.IsColonistPlayerControlled && !pawn.Downed
                && pawn.mindState?.mentalStateHandler?.CurStateDef != MentalStateDefOf.Berserk)
            {
                yield return new Command_Action
                {
                    defaultLabel = "激活狂暴",
                    defaultDesc = "释放狂战士芯片的肾上腺素冲击,立即进入狂暴状态,不顾一切地攻击附近的敌人。",
                    icon = TexCommand.AttackMelee,
                    action = delegate
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(
                            MentalStateDefOf.Berserk, "狂战士芯片激活", true);
                        SoundDefOf.Pawn_Melee_Punch_HitPawn.PlayOneShot(new TargetInfo(pawn));
                    }
                };
            }
        }
    }

    /// <summary>精神扭曲芯片参数:要周期性清除的思想列表。对照 CONN.HediffCompProperties_ThoughtsRemover。</summary>
    public class HediffCompProperties_ThoughtsRemover : HediffCompProperties
    {
        public List<string> thoughtsToClear = new List<string>();

        public HediffCompProperties_ThoughtsRemover()
        {
            compClass = typeof(HediffComp_ThoughtsRemover);
        }
    }

    /// <summary>精神扭曲芯片:每隔一段时间清除列表中的记忆思想(负罪感类)。</summary>
    public class HediffComp_ThoughtsRemover : HediffComp
    {
        private const int Interval = 600; // 约每小时游戏时间检查一次

        private HediffCompProperties_ThoughtsRemover Props => (HediffCompProperties_ThoughtsRemover)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(Interval) || Pawn.needs?.mood == null)
            {
                return;
            }
            List<Thought_Memory> found = Pawn.needs.mood.thoughts.memories.Memories
                .OfType<Thought_Memory>()
                .Where(m => Props.thoughtsToClear.Contains(m.def.defName))
                .ToList();
            foreach (Thought_Memory memory in found)
            {
                Pawn.needs.mood.thoughts.memories.RemoveMemory(memory);
            }
        }
    }

    /// <summary>缪斯芯片参数:每次给予的娱乐量与间隔。对照 CONN.HediffCompProperties_RecreationGiver。</summary>
    public class HediffCompProperties_RecreationGiver : HediffCompProperties
    {
        public float recreationToGive = 0.1f;
        public int eachNumberOfTicks = 7500;

        public HediffCompProperties_RecreationGiver()
        {
            compClass = typeof(HediffComp_RecreationGiver);
        }
    }

    /// <summary>缪斯芯片:周期性为植入者补充娱乐值。</summary>
    public class HediffComp_RecreationGiver : HediffComp
    {
        private HediffCompProperties_RecreationGiver Props => (HediffCompProperties_RecreationGiver)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(Props.eachNumberOfTicks))
            {
                return;
            }
            Need_Joy joy = Pawn.needs?.joy;
            if (joy != null)
            {
                joy.CurLevel += Props.recreationToGive;
            }
        }
    }

    /// <summary>手电植入物参数:附着光圈 mote 与尺寸。对照 CONN.CompProperties_LightEffect。</summary>
    public class HediffCompProperties_LightEffect : HediffCompProperties
    {
        public string visualMote;
        public float size = 1f;

        public HediffCompProperties_LightEffect()
        {
            compClass = typeof(HediffComp_LightEffect);
        }
    }

    /// <summary>手电植入物:在植入者身上维持一个附着光圈 mote。</summary>
    public class HediffComp_LightEffect : HediffComp
    {
        private Mote mote;

        private HediffCompProperties_LightEffect Props => (HediffCompProperties_LightEffect)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(120) || !Pawn.Spawned)
            {
                return;
            }
            if (mote == null || mote.Destroyed)
            {
                ThingDef moteDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.visualMote);
                if (moteDef != null)
                {
                    mote = MoteMaker.MakeAttachedOverlay(Pawn, moteDef, Vector3.zero, Props.size, 600f);
                }
            }
        }

        // mote 不持久化,读档后由 CompPostTick 自动重建
    }

    /// <summary>神经调节增强器:全技能小幅提升。对照 CONN.CompUseEffect_LearnSkill。</summary>
    public class CompUseEffect_LearnSkillRandom : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.skills == null)
            {
                return;
            }
            foreach (SkillRecord skill in usedBy.skills.skills)
            {
                skill.Learn(1200f, true);
            }
            Messages.Message("神经调节增强了" + usedBy.LabelShort + "的脑神经,所有技能小幅提升。",
                usedBy, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>洗脑:移除非出身背景赋予的性格特质。对照 CONN.CompUseEffect_TraitReset。</summary>
    public class CompUseEffect_TraitReset : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.story?.traits == null)
            {
                return;
            }
            // 出身背景强制特质集合
            HashSet<TraitDef> forced = new HashSet<TraitDef>();
            foreach (BackstoryTrait bt in BackstoryTraits(usedBy))
            {
                if (bt.def != null)
                {
                    forced.Add(bt.def);
                }
            }
            List<Trait> remove = usedBy.story.traits.allTraits
                .Where(t => !forced.Contains(t.def))
                .ToList();
            foreach (Trait trait in remove)
            {
                usedBy.story.traits.RemoveTrait(trait);
            }
            Messages.Message("洗脑完成:" + usedBy.LabelShort + "忘记了与自身经历无关的性格。",
                usedBy, MessageTypeDefOf.PositiveEvent);
        }

        private static IEnumerable<BackstoryTrait> BackstoryTraits(Pawn pawn)
        {
            if (pawn.story.Childhood?.forcedTraits != null)
            {
                foreach (BackstoryTrait bt in pawn.story.Childhood.forcedTraits)
                {
                    yield return bt;
                }
            }
            if (pawn.story.Adulthood?.forcedTraits != null)
            {
                foreach (BackstoryTrait bt in pawn.story.Adulthood.forcedTraits)
                {
                    yield return bt;
                }
            }
        }
    }

    /// <summary>激情赋予器:随机为一项技能赋予/提升激情。对照 CONN.CompUseEffect_PassionGiver。</summary>
    public class CompUseEffect_PassionGiver : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.skills == null)
            {
                return;
            }
            List<SkillRecord> candidates = usedBy.skills.skills
                .Where(s => s.passion != Passion.Major)
                .ToList();
            if (!candidates.Any())
            {
                return;
            }
            SkillRecord skill = candidates.RandomElement();
            skill.passion = skill.passion == Passion.Minor ? Passion.Major : Passion.Minor;
            Messages.Message("纳米机器人重塑了" + usedBy.LabelShort + "的脑结构:"
                + skill.def.label + "激情提升为" + skill.passion.GetLabel() + "。",
                usedBy, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>特性赋予器:随机灌输一项新性格特质。对照 CONN.CompUseEffect_TraitGiver。</summary>
    public class CompUseEffect_TraitGiver : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.story?.traits == null)
            {
                return;
            }
            List<TraitDef> candidates = DefDatabase<TraitDef>.AllDefs
                .Where(def => def.degreeDatas != null && def.degreeDatas.Count > 0
                    && !usedBy.story.traits.HasTrait(def)
                    && (def.conflictingTraits == null
                        || !def.conflictingTraits.Any(other => usedBy.story.traits.HasTrait(other))))
                .ToList();
            if (!candidates.Any())
            {
                return;
            }
            TraitDef chosen = candidates.RandomElement();
            TraitDegreeData degree = chosen.degreeDatas.RandomElement();
            usedBy.story.traits.GainTrait(new Trait(chosen, degree.degree, true));
            Messages.Message("纳米机器人为" + usedBy.LabelShort + "灌输了新特质:" + degree.label + "。",
                usedBy, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>想法清除器:抹除全部短期(非永久)记忆。对照 CONN NeurotrainersClearer。</summary>
    public class CompUseEffect_ThoughtsClearer : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.needs?.mood == null)
            {
                return;
            }
            List<Thought_Memory> remove = usedBy.needs.mood.thoughts.memories.Memories
                .OfType<Thought_Memory>()
                .Where(m => !m.permanent)
                .ToList();
            foreach (Thought_Memory memory in remove)
            {
                usedBy.needs.mood.thoughts.memories.RemoveMemory(memory);
            }
            Messages.Message(usedBy.LabelShort + "的短期记忆已被抹除。",
                usedBy, MessageTypeDefOf.NeutralEvent);
        }
    }

    /// <summary>重塑器:替换出身背景并抹除全部记忆,相当于获得一段新人生。
    /// 对照 CONN.CompUseEffect_Reconditioner。</summary>
    public class CompUseEffect_Reconditioner : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy.story == null)
            {
                return;
            }
            // 随机替换童年/成年背景
            List<BackstoryDef> childhoods = DefDatabase<BackstoryDef>.AllDefs
                .Where(b => b.slot == BackstorySlot.Childhood).ToList();
            List<BackstoryDef> adulthoods = DefDatabase<BackstoryDef>.AllDefs
                .Where(b => b.slot == BackstorySlot.Adulthood).ToList();
            if (childhoods.Any())
            {
                usedBy.story.Childhood = childhoods.RandomElement();
            }
            if (adulthoods.Any())
            {
                usedBy.story.Adulthood = adulthoods.RandomElement();
            }
            // 抹除全部记忆
            if (usedBy.needs?.mood != null)
            {
                List<Thought_Memory> all = usedBy.needs.mood.thoughts.memories.Memories
                    .OfType<Thought_Memory>().ToList();
                foreach (Thought_Memory memory in all)
                {
                    usedBy.needs.mood.thoughts.memories.RemoveMemory(memory);
                }
            }
            // 特质重建:仅保留新出身背景强制特质,并补齐缺失的强制特质
            RebuildTraitsFromBackstories(usedBy);
            Messages.Message("重塑完成:" + usedBy.LabelShort + "获得了一段全新的人生。",
                usedBy, MessageTypeDefOf.PositiveEvent);
        }

        private static void RebuildTraitsFromBackstories(Pawn pawn)
        {
            HashSet<string> forced = new HashSet<string>();
            foreach (BackstoryDef backstory in new[] { pawn.story.Childhood, pawn.story.Adulthood })
            {
                if (backstory?.forcedTraits == null)
                {
                    continue;
                }
                foreach (BackstoryTrait bt in backstory.forcedTraits)
                {
                    if (bt.def == null)
                    {
                        continue;
                    }
                    forced.Add(bt.def.defName);
                    if (!pawn.story.traits.HasTrait(bt.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(bt.def, bt.degree, true));
                    }
                }
            }
            List<Trait> remove = pawn.story.traits.allTraits
                .Where(t => !forced.Contains(t.def.defName))
                .ToList();
            foreach (Trait trait in remove)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
        }
    }

    /// <summary>
    /// 不移除子部件的仿生部件(对照 HSK Hediff_AddedPartNoRemoveSubparts):
    /// 安装时仅恢复本体部件,不把子部件标记为缺失,用于脖颈等需要保留头部的部位。
    /// </summary>
    public class Hediff_AddedPartNoRemoveSubparts : Hediff_Implant
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            pawn.health.RestorePart(Part, this, checkStateChange: false);
        }
    }

    /// <summary>
    /// 年龄加速器(对照 HSK SK.Hediff_Senexium):按 severity 倍率加速生物学年龄,
    /// severity 60 即 60 倍老化。每 tick 在正常推进之外追加 severity-1 个生物年龄 tick。
    /// </summary>
    public class Hediff_AgeAccelerator : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn?.ageTracker != null && !pawn.Dead && Severity > 1f)
            {
                pawn.ageTracker.AgeBiologicalTicks += (long)(Severity - 1f);
            }
        }
    }
}
