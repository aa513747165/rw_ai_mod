using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 技能射击特效框架（第三步：单发射击 + 浮游齐射） ==========
    // MMTShootUtil 为发射核心：单发 = 施法者直射一枚投射物；
    // 浮游齐射 = 施法者周边环形浮现 N 个浮游单元（fleck，朝向目标），短暂瞄准后同一 tick 齐射。
    // 延迟调度由 MapComponent_MMTSkillFX 承载（原版反射自动注册，无需 XML）。
    // pawn 技能侧出口：CompAbilityEffect_MMTSingleShot / CompAbilityEffect_MMTFunnelShot；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（沙袋人偶火球术）。
    // 素材全部走原版（投射物/贴图/音效均在 XML 里引用原版 defName/texPath）。

    /// <summary>
    /// 技能特效延迟任务调度器：纯内存队列不进存档（读档丢失进行中的特效，可接受）。
    /// MapComponent 子类由原版 Map.ConstructComponents 反射自动注册，每图一份。
    /// </summary>
    public class MapComponent_MMTSkillFX : MapComponent
    {
        private struct ScheduledAction
        {
            public int dueTick;
            public Action action;
        }

        private readonly List<ScheduledAction> queue = new List<ScheduledAction>();

        public MapComponent_MMTSkillFX(Map map) : base(map)
        {
        }

        /// <summary>delayTicks 后在指定地图上执行 action（0 = 下一 tick）。</summary>
        public static void Schedule(Map map, int delayTicks, Action action)
        {
            if (map == null || action == null)
            {
                return;
            }
            map.GetComponent<MapComponent_MMTSkillFX>().queue.Add(new ScheduledAction
            {
                dueTick = Find.TickManager.TicksGame + Mathf.Max(1, delayTicks),
                action = action
            });
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (queue.Count == 0)
            {
                return;
            }
            int now = Find.TickManager.TicksGame;
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                if (queue[i].dueTick > now)
                {
                    continue;
                }
                Action action = queue[i].action;
                queue.RemoveAt(i);
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log.Error("[MMT] 技能特效延迟任务异常：" + ex);
                }
            }
        }
    }

    /// <summary>浮游齐射参数包（MMTShootUtil.FireFunnels 的输入）。</summary>
    public class MMTFunnelParams
    {
        public ThingDef projectileDef;
        /// <summary>浮游单元数量。</summary>
        public int funnelCount = 4;
        /// <summary>环绕半径（格）。</summary>
        public float ringRadius = 1.8f;
        /// <summary>浮游单元出现后多少 tick 齐射（0 ≈ 立即）。</summary>
        public int aimTicks = 15;
        /// <summary>浮游单元的静态 fleck（null = 不画浮游图形直接齐射）。</summary>
        public FleckDef funnelFleckDef;
        /// <summary>浮游图形染色（LauncherShot 等白底贴图靠它上色）。</summary>
        public Color tint = new Color(1f, 0.55f, 0.15f);
        public SoundDef sound;
        public bool preventFriendlyFire = true;
        public Thing equipment;
    }

    /// <summary>射击特效核心：单发与浮游齐射的统一入口，pawn 技能与建筑演示共用。</summary>
    public static class MMTShootUtil
    {
        /// <summary>单发射击：施法者位置直射一枚投射物，发音效。</summary>
        public static void FireSingle(Thing caster, LocalTargetInfo target, ThingDef projectileDef, SoundDef sound = null, bool preventFriendlyFire = true, Thing equipment = null)
        {
            Map map = caster.MapHeld;
            if (map == null || projectileDef == null || !target.IsValid)
            {
                return;
            }
            LaunchFrom(caster, caster.Position, target, projectileDef, preventFriendlyFire, equipment);
            PlaySound(sound, caster.Position, map);
        }

        /// <summary>
        /// 浮游齐射：施法者周边环形均布 N 个浮游单元（fleck 朝向目标），
        /// aimTicks 后所有单元同一 tick 向目标开火；只播一声齐射音效。
        /// </summary>
        public static void FireFunnels(Thing caster, LocalTargetInfo target, MMTFunnelParams p)
        {
            Map map = caster.MapHeld;
            if (map == null || p == null || p.projectileDef == null || !target.IsValid)
            {
                return;
            }
            int count = Mathf.Max(1, p.funnelCount);
            float startAngle = Rand.Range(0f, 360f);
            Vector3 center = caster.Position.ToVector3Shifted();
            List<IntVec3> muzzles = new List<IntVec3>(count);
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + 360f * i / count;
                Vector3 pos = center + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * p.ringRadius;
                IntVec3 cell = pos.ToIntVec3();
                if (!cell.InBounds(map))
                {
                    cell = caster.Position;
                }
                muzzles.Add(cell);
                SpawnFunnelFleck(map, pos, target, p);
            }
            MapComponent_MMTSkillFX.Schedule(map, p.aimTicks, delegate
            {
                if (caster.Destroyed || caster.MapHeld != map)
                {
                    return;
                }
                for (int i = 0; i < muzzles.Count; i++)
                {
                    LaunchFrom(caster, muzzles[i], target, p.projectileDef, p.preventFriendlyFire, p.equipment);
                }
                PlaySound(p.sound, caster.Position, map);
            });
        }

        private static void LaunchFrom(Thing caster, IntVec3 muzzle, LocalTargetInfo target, ThingDef projectileDef, bool preventFriendlyFire, Thing equipment)
        {
            Map map = caster.MapHeld;
            if (!muzzle.InBounds(map))
            {
                muzzle = caster.Position;
            }
            Projectile proj = (Projectile)GenSpawn.Spawn(projectileDef, muzzle, map);
            proj.Launch(caster, target, target, ProjectileHitFlags.IntendedTarget, preventFriendlyFire, equipment);
        }

        /// <summary>浮游单元图形：静态 fleck 旋转对准目标，存活到齐射后淡出。</summary>
        private static void SpawnFunnelFleck(Map map, Vector3 pos, LocalTargetInfo target, MMTFunnelParams p)
        {
            if (p.funnelFleckDef == null)
            {
                return;
            }
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, p.funnelFleckDef, 1f);
            data.rotation = (target.CenterVector3 - pos).AngleFlat();
            data.instanceColor = p.tint;
            data.solidTimeOverride = p.aimTicks / 60f + 0.35f;
            map.flecks.CreateFleck(data);
        }

        private static void PlaySound(SoundDef sound, IntVec3 cell, Map map)
        {
            if (sound != null)
            {
                SoundStarter.PlayOneShot(sound, new TargetInfo(cell, map));
            }
        }
    }

    // ========== pawn 技能出口：单发射击 ==========

    public class CompProperties_MMTSingleShot : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public string soundDefName;
        public bool preventFriendlyFire = true;

        public CompProperties_MMTSingleShot()
        {
            compClass = typeof(CompAbilityEffect_MMTSingleShot);
        }
    }

    /// <summary>单发射击技能：向目标直射一枚投射物（读条时间由 AbilityDef 的 warmupTime 决定，读条圆走 CastBar）。</summary>
    public class CompAbilityEffect_MMTSingleShot : CompAbilityEffect
    {
        public new CompProperties_MMTSingleShot Props => (CompProperties_MMTSingleShot)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTShootUtil.FireSingle(caster, target, Props.projectileDef,
                Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                Props.preventFriendlyFire, caster.equipment?.Primary);
        }
    }

    // ========== pawn 技能出口：浮游齐射 ==========

    public class CompProperties_MMTFunnelShot : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public int funnelCount = 4;
        public float ringRadius = 1.8f;
        public int aimTicks = 15;
        public string funnelFleckDefName;
        public Color tint = new Color(1f, 0.55f, 0.15f);
        public string soundDefName;
        public bool preventFriendlyFire = true;

        public CompProperties_MMTFunnelShot()
        {
            compClass = typeof(CompAbilityEffect_MMTFunnelShot);
        }
    }

    /// <summary>浮游齐射技能：施法者周边浮现 N 个浮游单元，短暂瞄准后同时射向目标。</summary>
    public class CompAbilityEffect_MMTFunnelShot : CompAbilityEffect
    {
        public new CompProperties_MMTFunnelShot Props => (CompProperties_MMTFunnelShot)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTShootUtil.FireFunnels(caster, target, new MMTFunnelParams
            {
                projectileDef = Props.projectileDef,
                funnelCount = Props.funnelCount,
                ringRadius = Props.ringRadius,
                aimTicks = Props.aimTicks,
                funnelFleckDef = Props.funnelFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.funnelFleckDefName),
                tint = Props.tint,
                sound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                preventFriendlyFire = Props.preventFriendlyFire,
                equipment = caster.equipment?.Primary
            });
        }
    }
}
