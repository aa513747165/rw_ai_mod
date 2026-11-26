using System.Linq;
using RimWorld;
using Verse;

namespace MyModeTest
{
    // 召唤建筑（mmt-class-design.md §2.3 Summon 桶的建筑侧）：
    // 魔法防御塔/神经炮塔/萨满三图腾/魔法地雷共用同一个参数化建筑 comp，
    // 限时由原版 CompProperties_Lifespan 到期自毁，无自研 AI、无 per-tick 全图扫描（间隔事件）。

    /// <summary>召唤建筑参数：attack=间隔攻击最近敌人；heal=间隔治疗周围友军；mine=敌人接近即爆。</summary>
    public class CompProperties_MMTSummonTurret : CompProperties
    {
        /// <summary>attack / heal / mine。</summary>
        public string mode = "attack";
        public DamageDef damageDef;
        public int damageAmount = 12;
        public float armorPen;
        /// <summary>攻击/感应半径。</summary>
        public float radius = 18f;
        public int intervalTicks = 90;
        /// <summary>heal 模式：每次脉冲治疗量与目标数。</summary>
        public float healAmount = 6f;
        public int healMaxTargets = 3;
        /// <summary>mine 模式：爆炸半径。</summary>
        public float explosionRadius = 2.9f;
        public string fleckDefName;

        public CompProperties_MMTSummonTurret()
        {
            compClass = typeof(CompMMTSummonTurret);
        }
    }

    public class CompMMTSummonTurret : ThingComp
    {
        private int ticksToNext;

        public new CompProperties_MMTSummonTurret Props => (CompProperties_MMTSummonTurret)props;

        private Building Building => parent as Building;

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned)
            {
                return;
            }
            ticksToNext--;
            if (ticksToNext > 0)
            {
                return;
            }
            ticksToNext = Props.intervalTicks;
            Map map = parent.Map;
            switch (Props.mode)
            {
                case "heal":
                    DoHeal(map);
                    break;
                case "mine":
                    TryDetonate(map);
                    break;
                default:
                    DoAttack(map);
                    break;
            }
        }

        private void DoAttack(Map map)
        {
            Pawn target = GenRadial.RadialDistinctThingsAround(parent.Position, map, Props.radius, true)
                .OfType<Pawn>()
                .Where(p => !p.Dead && !p.Downed && p.Faction != null && p.Faction != parent.Faction && p.HostileTo(parent.Faction))
                .OrderBy(p => p.Position.DistanceToSquared(parent.Position))
                .FirstOrDefault();
            if (target == null)
            {
                return;
            }
            FleckMaker.Static(parent.Position.ToVector3Shifted(), map, FleckDefOf.ShotFlash, 1f);
            if (!Props.fleckDefName.NullOrEmpty())
            {
                FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail(Props.fleckDefName);
                if (fleck != null)
                {
                    FleckMaker.Static(target.Position.ToVector3Shifted(), map, fleck, 1.5f);
                }
            }
            target.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Bullet, Props.damageAmount, Props.armorPen, -1f, parent));
        }

        private void DoHeal(Map map)
        {
            int healed = 0;
            foreach (Pawn p in GenRadial.RadialDistinctThingsAround(parent.Position, map, Props.radius, true).OfType<Pawn>())
            {
                if (healed >= Props.healMaxTargets)
                {
                    break;
                }
                if (p.Dead || p.Faction != parent.Faction || p.health?.hediffSet == null)
                {
                    continue;
                }
                Hediff_Injury worst = p.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                    .Where(h => !h.IsPermanent()).OrderByDescending(h => h.Severity).FirstOrDefault();
                if (worst != null)
                {
                    worst.Severity -= Props.healAmount;
                    healed++;
                    FleckMaker.ThrowMetaIcon(p.Position, map, FleckDefOf.HealingCross);
                }
            }
        }

        private void TryDetonate(Map map)
        {
            bool triggered = GenRadial.RadialDistinctThingsAround(parent.Position, map, Props.radius, true)
                .OfType<Pawn>()
                .Any(p => !p.Dead && p.Faction != null && p.Faction != parent.Faction && p.HostileTo(parent.Faction));
            if (!triggered)
            {
                return;
            }
            IntVec3 cell = parent.Position;
            parent.Destroy(DestroyMode.Vanish);
            GenExplosion.DoExplosion(cell, map, Props.explosionRadius, Props.damageDef ?? DamageDefOf.Bomb,
                null, Props.damageAmount, Props.armorPen);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksToNext, "ticksToNext");
        }
    }
}
