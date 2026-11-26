using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 驱散/净化框架（第二十三步：移除限时状态） ==========
    // 增益/减益框架（第八步）的逆操作：移除目标身上的 Hediff。
    // 核心约束：只移除"有持续时间"的状态——必须带 HediffComp_Disappears（到时自动消退），
    // 永久性/无时限状态（植入体、成瘾、慢性病、部位缺失等）一律不可驱散；
    // 敌我自动分流：友方目标移除减益（isBad=true），敌方目标移除增益（isBad=false），
    // 一次最多移除 N 个，按 Severity 从高到低优先（先解最严重的）。
    // 视觉：目标头顶白色图标闪光（复用 Thing_MMTBuffIconFlash 图标桶，0.5s 淡出）
    // + 每驱散 1 个状态爆 1 次白色微光点（原版 ThrowMicroSparks）。
    // MMTDispelUtil 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTDispel
    // （radius=0 单体锁定，radius>0 圈选范围驱散，敌我按目标各自判定）；
    // 建筑演示侧出口 demoType="dispel"（单体锁定，牧师 II 阶驱散魔法的框架皮肤，
    // 参数见 docs/mmt-class-priest.md §3.8）。无任何新增贴图/Def，纯代码框架。

    /// <summary>驱散参数包（MMTDispelUtil 的输入）。</summary>
    public class MMTDispelParams
    {
        /// <summary>单目标最多移除的状态数。</summary>
        public int maxRemove = 3;
        /// <summary>头顶图标贴图（空=不画图标）。</summary>
        public string iconTexPath = "UI/RoM/purify";
        /// <summary>图标/粒子染色（默认圣白）。</summary>
        public Color tint = new Color(1f, 1f, 1f);
        /// <summary>图标显示 tick（0.5s ≈ 30）。</summary>
        public int iconDurationTicks = 30;
        /// <summary>图标尺寸（格）。</summary>
        public float iconScale = 0.5f;
        /// <summary>身上粒子 fleck（null=不放粒子）。</summary>
        public FleckDef particleFleckDef;
        public SoundDef sound;
    }

    /// <summary>驱散/净化统一入口。</summary>
    public static class MMTDispelUtil
    {
        /// <summary>该 Hediff 是否可驱散：有持续时间（HediffComp_Disappears）且非永久。</summary>
        public static bool IsDispellable(Hediff h)
        {
            if (h == null || h.IsPermanent())
            {
                return false;
            }
            return h is HediffWithComps hwc && hwc.TryGetComp<HediffComp_Disappears>() != null;
        }

        /// <summary>友方判定（语义同新星环：同派系，或无派系且无原属派系的动物）。</summary>
        private static bool IsFriendlyTo(Pawn target, Thing caster)
        {
            return caster != null && (target.Faction == caster.Faction
                || (target.Faction == null && target.RaceProps.Animal && caster.Faction != null && target.HostFaction == null));
        }

        /// <summary>收集目标身上可驱散状态（友方取减益、敌方取增益），按 Severity 从高到低排序。</summary>
        public static List<Hediff> FindDispellable(Pawn target, Thing caster)
        {
            List<Hediff> found = new List<Hediff>();
            if (target == null || target.Dead || caster == null)
            {
                return found;
            }
            // 敌我分流：友方驱散减益，敌方驱散增益；中立目标不处理
            bool? removeDebuff = IsFriendlyTo(target, caster) ? true
                : target.Faction != null && target.HostileTo(caster) ? false
                : (bool?)null;
            if (removeDebuff == null)
            {
                return found;
            }
            foreach (Hediff h in target.health.hediffSet.hediffs)
            {
                if (h.def.isBad == removeDebuff.Value && IsDispellable(h))
                {
                    found.Add(h);
                }
            }
            found.Sort((a, b) => b.Severity.CompareTo(a.Severity));
            return found;
        }

        /// <summary>驱散单个目标：移除至多 maxRemove 个可驱散状态，返回被移除状态的显示名列表。</summary>
        public static List<string> DispelOne(Thing caster, Pawn target, MMTDispelParams p)
        {
            List<string> removed = new List<string>();
            if (target == null || target.Destroyed || !target.Spawned || target.Dead || p == null)
            {
                return removed;
            }
            List<Hediff> candidates = FindDispellable(target, caster);
            int take = Mathf.Min(Mathf.Max(1, p.maxRemove), candidates.Count);
            for (int i = 0; i < take; i++)
            {
                removed.Add(candidates[i].LabelCap);
                target.health.RemoveHediff(candidates[i]);
            }
            if (removed.Count == 0)
            {
                return removed;
            }
            // 视觉：头顶白色图标闪光（复用增益图标桶，纯特效不加 Hediff）
            if (!p.iconTexPath.NullOrEmpty() || p.particleFleckDef != null)
            {
                MMTBuffFXUtil.ApplyOne(target, new MMTBuffFXParams
                {
                    buffDef = null,
                    iconTexPath = p.iconTexPath,
                    iconTint = p.tint,
                    iconDurationTicks = Mathf.Max(1, p.iconDurationTicks),
                    iconScale = p.iconScale,
                    particleFleckDef = p.particleFleckDef,
                    particleTint = p.tint
                });
            }
            // 每驱散 1 个状态爆 1 次白色微光点
            if (target.MapHeld == Find.CurrentMap)
            {
                for (int i = 0; i < removed.Count; i++)
                {
                    Vector3 pos = target.DrawPos;
                    pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    FleckMaker.ThrowMicroSparks(pos, target.MapHeld);
                }
            }
            if (p.sound != null)
            {
                SoundStarter.PlayOneShot(p.sound, new TargetInfo(target.Position, target.MapHeld));
            }
            return removed;
        }

        /// <summary>范围驱散：对一批 pawn 各自按敌我分流驱散，返回成功驱散的目标数。</summary>
        public static int Apply(Thing caster, IEnumerable<Pawn> targets, MMTDispelParams p)
        {
            int affected = 0;
            foreach (Pawn pawn in targets)
            {
                if (DispelOne(caster, pawn, p).Count > 0)
                {
                    affected++;
                }
            }
            return affected;
        }
    }

    // ========== pawn 技能出口：驱散/净化 ==========

    public class CompProperties_MMTDispel : CompProperties_MMTAoETargetBase
    {
        public int maxRemove = 3;
        public string iconTexPath = "UI/RoM/purify";
        public Color tint = new Color(1f, 1f, 1f);
        public float iconSeconds = 0.5f;
        public float iconScale = 0.5f;
        public string particleFleckDefName = "MMT_Fleck_BuffSpark";
        public string soundDefName;

        public CompProperties_MMTDispel()
        {
            compClass = typeof(CompAbilityEffect_MMTDispel);
        }
    }

    /// <summary>驱散技能：radius=0 单体锁定，radius>0 圈选范围驱散（敌我自动分流）。</summary>
    public class CompAbilityEffect_MMTDispel : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTDispel Props => (CompProperties_MMTDispel)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTDispelParams p = new MMTDispelParams
            {
                maxRemove = Props.maxRemove,
                iconTexPath = Props.iconTexPath,
                tint = Props.tint,
                iconDurationTicks = Mathf.Max(1, Props.iconSeconds.SecondsToTicks()),
                iconScale = Props.iconScale,
                particleFleckDef = Props.particleFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.particleFleckDefName),
                sound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            };
            if (Props.radius > 0f)
            {
                MMTDispelUtil.Apply(caster, AffectedPawns(target), p);
            }
            else if (target.HasThing && target.Thing is Pawn pawn)
            {
                MMTDispelUtil.DispelOne(caster, pawn, p);
            }
        }
    }
}
