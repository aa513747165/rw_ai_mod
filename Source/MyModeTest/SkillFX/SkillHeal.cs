using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 治疗特效框架（第十七步：范围回血） ==========
    // 施法完成后，范围内每个有效友方 pawn：按治疗量池从重伤到轻伤依次回复伤口
    // （Hediff_Injury.Heal，只治非永久性伤口），头顶浮现治疗小图标 + 原版绿十字
    // 飘字 + 身上冒绿色光点粒子（图标/粒子实体直接复用第八步 Thing_MMTBuffIconFlash）。
    // MMTHealUtil.Apply 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTSkillHeal
    // （radius>0 圈选范围友方，radius=0 只治点选目标）；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="heal"）。
    // 素材仅原版绿十字贴图 + MMT 占位光点贴图，不依赖 RoM。

    /// <summary>治疗特效参数包（MMTHealUtil.Apply 的输入）。</summary>
    public class MMTHealParams
    {
        /// <summary>每个目标的治疗量池（按伤口严重度从高到低分配）。</summary>
        public float healAmount = 15f;
        /// <summary>头顶小图标贴图（默认原版绿十字）。</summary>
        public string iconTexPath = "Things/Mote/HealingCross";
        public Color iconTint = new Color(0.4f, 1f, 0.5f);
        /// <summary>图标显示 tick。</summary>
        public int iconDurationTicks = 90;
        /// <summary>身上粒子 fleck（null = 不放粒子）。</summary>
        public FleckDef particleFleckDef;
        public Color particleTint = new Color(0.4f, 1f, 0.5f);
        /// <summary>是否额外抛原版绿十字飘字。</summary>
        public bool throwHealingCross = true;
        public SoundDef startSound;
    }

    /// <summary>治疗统一入口：回血 + 治疗特效。</summary>
    public static class MMTHealUtil
    {
        /// <summary>对一批目标施放治疗（只有 pawn 回血，所有 pawn 放特效），返回实际治疗人数。</summary>
        public static int Apply(Thing caster, IEnumerable<Thing> targets, MMTHealParams p)
        {
            int count = 0;
            Thing first = null;
            foreach (Thing t in targets)
            {
                if (!(t is Pawn pawn) || pawn.Dead || !pawn.Spawned)
                {
                    continue;
                }
                ApplyOne(pawn, p);
                if (first == null)
                {
                    first = pawn;
                }
                count++;
            }
            if (count > 0 && p.startSound != null && first != null && first.MapHeld != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(first.Position, first.MapHeld));
            }
            return count;
        }

        /// <summary>治疗单个 pawn：治疗量池按伤口严重度从高到低分配 + 播放治疗特效。</summary>
        public static void ApplyOne(Pawn pawn, MMTHealParams p)
        {
            if (pawn == null || pawn.Dead || !pawn.Spawned || p == null)
            {
                return;
            }
            // 治疗量池：从重伤到轻伤依次回复（只治非永久性伤口）
            float pool = p.healAmount;
            if (pool > 0f)
            {
                foreach (Hediff_Injury injury in pawn.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>().Where(h => !h.IsPermanent())
                    .OrderByDescending(h => h.Severity).ToList())
                {
                    if (pool <= 0f)
                    {
                        break;
                    }
                    float healed = Mathf.Min(pool, injury.Severity);
                    injury.Heal(healed);
                    pool -= healed;
                }
            }
            // 特效：绿十字飘字 + 头顶图标/身上粒子（复用第八步实体）
            Map map = pawn.Map;
            if (p.throwHealingCross)
            {
                FleckMaker.ThrowMetaIcon(pawn.Position, map, FleckDefOf.HealingCross);
            }
            MMTBuffFXUtil.ApplyOne(pawn, new MMTBuffFXParams
            {
                buffDef = null,
                iconTexPath = p.iconTexPath,
                iconTint = p.iconTint,
                iconDurationTicks = p.iconDurationTicks,
                particleFleckDef = p.particleFleckDef,
                particleTint = p.particleTint,
                startSound = null
            });
        }
    }

    // ========== pawn 技能出口：治疗（单体走目标，范围走预览圈） ==========

    public class CompProperties_MMTSkillHeal : CompProperties_MMTAoETargetBase
    {
        public float healAmount = 15f;
        public string iconTexPath = "Things/Mote/HealingCross";
        public Color iconTint = new Color(0.4f, 1f, 0.5f);
        public float iconSeconds = 1.5f;
        public string particleFleckDefName = "MMT_Fleck_BuffSpark";
        public string soundDefName;

        public CompProperties_MMTSkillHeal()
        {
            compClass = typeof(CompAbilityEffect_MMTSkillHeal);
            targetFriendly = true;
        }
    }

    /// <summary>治疗技能：radius>0 圈选范围友方回血，radius=0 只治点选目标。</summary>
    public class CompAbilityEffect_MMTSkillHeal : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTSkillHeal Props => (CompProperties_MMTSkillHeal)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTHealParams p = new MMTHealParams
            {
                healAmount = Props.healAmount,
                iconTexPath = Props.iconTexPath,
                iconTint = Props.iconTint,
                iconDurationTicks = Mathf.Max(1, Props.iconSeconds.SecondsToTicks()),
                particleFleckDef = Props.particleFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.particleFleckDefName),
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            };
            if (Props.radius > 0f)
            {
                MMTHealUtil.Apply(caster, AffectedPawns(target), p);
            }
            else if (target.Thing is Pawn pawn)
            {
                MMTHealUtil.ApplyOne(pawn, p);
            }
        }
    }
}
