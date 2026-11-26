using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 近战击退框架（第十二步：对目标猛击并击退数格） ==========
    // 近身锁定一名目标：先结算一次钝器伤害（伤害方向 = 施法者→目标），再把目标
    // 沿该方向击飞数格（pawn 复用原版 PawnFlyer 飞行逻辑，与第九步跳跃一致）；
    // 击飞路径被墙/不可站立格挡住则提前落位并追加一次撞墙伤害，落地后短暂踉跄。
    // 建筑不受击退（只结算伤害）。
    // MMTKnockbackUtil.Execute 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTKnockback；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="knockback"）。
    // 素材仅原版飞行实体 + MMT 占位闪光贴图，不依赖 RoM。

    /// <summary>击退参数包（MMTKnockbackUtil.Execute 的输入）。</summary>
    public class MMTKnockbackParams
    {
        /// <summary>猛击伤害类型（null = Blunt）。</summary>
        public DamageDef damageDef;
        public float damage = 12f;
        public float armorPenetration;
        /// <summary>击退格数。</summary>
        public int knockbackCells = 4;
        /// <summary>击飞路径被障碍挡住时的撞墙追加伤害（0 = 不追加）。</summary>
        public float wallSlamDamage = 8f;
        /// <summary>落地踉跄 tick（0 = 不踉跄）。</summary>
        public int staggerTicks = 50;
        /// <summary>起跳闪光 fleck。</summary>
        public FleckDef departFleckDef;
        public SoundDef sound;
        public Thing equipment;
    }

    /// <summary>击退统一入口：猛击 + 击飞 + 撞墙/踉跄结算。</summary>
    public static class MMTKnockbackUtil
    {
        /// <summary>Core 自带原版飞行实体（与第九步跳跃共用）。</summary>
        public const string VanillaFlyerDefName = "PawnFlyer";

        /// <summary>执行击退，返回是否成功击飞（目标非 pawn 时只结算伤害并返回 false）。</summary>
        public static bool Execute(Thing caster, Thing victimThing, MMTKnockbackParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || victimThing == null || victimThing.Destroyed || !victimThing.Spawned || p == null)
            {
                return false;
            }
            Vector3 dir3 = victimThing.Position.ToVector3Shifted() - caster.Position.ToVector3Shifted();
            dir3.y = 0f;
            if (dir3.sqrMagnitude < 0.01f)
            {
                dir3 = Vector3.forward;
            }
            dir3.Normalize();
            float angle = dir3.AngleFlat();
            // 猛击伤害
            victimThing.TakeDamage(new DamageInfo(p.damageDef ?? DamageDefOf.Blunt, p.damage, p.armorPenetration,
                angle, caster, null, p.equipment?.def));
            if (p.sound != null)
            {
                SoundStarter.PlayOneShot(p.sound, new TargetInfo(victimThing.Position, map));
            }
            if (!(victimThing is Pawn victim) || victim.Dead || !victim.Spawned)
            {
                return false;
            }
            // 击飞落点：沿击退方向逐格走，撞墙/不可站立提前停（撞墙追加伤害）
            IntVec3 landing = victim.Position;
            bool slammed = false;
            for (int i = 1; i <= p.knockbackCells; i++)
            {
                IntVec3 cell = (victim.Position.ToVector3Shifted() + dir3 * i).ToIntVec3();
                if (!cell.InBounds(map) || !cell.Standable(map))
                {
                    slammed = cell.InBounds(map) && cell.GetEdifice(map) != null;
                    break;
                }
                landing = cell;
            }
            if (slammed && p.wallSlamDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(p.damageDef ?? DamageDefOf.Blunt, p.wallSlamDamage, p.armorPenetration,
                    angle, caster, null, p.equipment?.def));
            }
            MMTDisplaceUtil.PlayFleck(map, victim.Position, p.departFleckDef);
            if (landing == victim.Position)
            {
                // 无处可退：原地踉跄
                Stagger(victim, p.staggerTicks);
                return false;
            }
            // 击飞：原版 PawnFlyer（同第九步跳跃）
            ThingDef flyerDef = DefDatabase<ThingDef>.GetNamedSilentFail(VanillaFlyerDefName);
            if (flyerDef == null)
            {
                Log.Error("[MMT] 找不到 Core 的 " + VanillaFlyerDefName + "，无法执行击退");
                return false;
            }
            IntVec3 origin = victim.Position;
            PawnFlyer flyer = PawnFlyer.MakeFlyer(flyerDef, victim, landing, null, null, false, null, null, LocalTargetInfo.Invalid);
            if (flyer.FlyingPawn == null && victim.Spawned)
            {
                victim.DeSpawn();
                flyer.GetDirectlyHeldThings().TryAdd(victim);
            }
            GenSpawn.Spawn(flyer, origin, map);
            // 落地踉跄：按原版飞行速度（12 格/s，最低 0.5s）估算落地时刻后补踉跄
            if (p.staggerTicks > 0)
            {
                float dist = (landing - origin).LengthHorizontal;
                int flightTicks = Mathf.Max(30, Mathf.CeilToInt(dist / 12f * 60f)) + 2;
                int staggerTicks = p.staggerTicks;
                Pawn staggerVictim = victim;
                MapComponent_MMTSkillFX.Schedule(map, flightTicks, delegate
                {
                    Stagger(staggerVictim, staggerTicks);
                });
            }
            return true;
        }

        private static void Stagger(Pawn pawn, int ticks)
        {
            if (pawn != null && !pawn.Dead && pawn.Spawned && ticks > 0)
            {
                pawn.stances.StaggerFor(ticks);
            }
        }
    }

    // ========== pawn 技能出口：击退（单体锁定） ==========

    public class CompProperties_MMTKnockback : CompProperties_AbilityEffect
    {
        public string damageDefName;
        public float damage = 12f;
        public float armorPenetration;
        public int knockbackCells = 4;
        public float wallSlamDamage = 8f;
        public int staggerTicks = 50;
        public string departFleckDefName = "MMT_Fleck_BlinkFlash";
        public string soundDefName;

        public CompProperties_MMTKnockback()
        {
            compClass = typeof(CompAbilityEffect_MMTKnockback);
        }
    }

    /// <summary>击退技能：锁定一名目标猛击并击飞（读条时间由 AbilityDef 的 warmupTime 决定）。</summary>
    public class CompAbilityEffect_MMTKnockback : CompAbilityEffect
    {
        public new CompProperties_MMTKnockback Props => (CompProperties_MMTKnockback)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!target.HasThing)
            {
                return;
            }
            Pawn caster = parent.pawn;
            MMTKnockbackUtil.Execute(caster, target.Thing, new MMTKnockbackParams
            {
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                armorPenetration = Props.armorPenetration,
                knockbackCells = Props.knockbackCells,
                wallSlamDamage = Props.wallSlamDamage,
                staggerTicks = Props.staggerTicks,
                departFleckDef = Props.departFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.departFleckDefName),
                sound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
