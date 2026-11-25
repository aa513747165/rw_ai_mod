using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    // ========== 技能天降打击框架（第五步：上方落下的单体落雷 + 范围雷暴） ==========
    // 视觉/音效/基础爆炸全部复用原版闪电事件（RimWorld.WeatherEvent_LightningStrike，
    // 经 map.weatherManager.eventHandler.AddEvent 立即触发：闪电实体 + 天空闪光 + 雷声 +
    // 原版火焰爆炸，表现与原版闪电风暴完全一致）；可在其上叠加一层可配置自定义爆炸
    // （MMTStrikeParams.damage > 0 时生效，无二次视觉/音效，纯伤害层）。
    // 调度：MMTStrikeUtil.FireStorm 为统一入口——单次定点直接触发；连发/范围则生成隐形
    // 调度器 Thing_MMTSkyStorm 逐 tick 在目标区域随机落点（优先无屋顶格，雷从天降）。
    // pawn 技能侧出口：CompAbilityEffect_MMTStrike（范围预览圈复用第一步 AoETarget 框架）；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="strike"）。
    // 注：天气事件不进存档，闪电视觉原生存续 ~1 秒内；调度器参数为运行时传入，读档后自毁。

    /// <summary>天降打击参数包（MMTStrikeUtil.FireStorm / FireStrike 的输入）。</summary>
    public class MMTStrikeParams
    {
        /// <summary>落雷次数（1 = 单体落雷；>1 = 雷暴连发）。</summary>
        public int strikeCount = 1;
        /// <summary>雷暴连发间隔 tick。</summary>
        public int strikeIntervalTicks = 12;
        /// <summary>雷暴随机落点半径（格，围绕目标点；0 = 定点落雷）。</summary>
        public float areaRadius;
        /// <summary>首次落雷前延迟 tick。</summary>
        public int initialDelayTicks;
        /// <summary>附加自定义伤害类型（null = Flame）。</summary>
        public DamageDef damageDef;
        /// <summary>附加自定义爆炸伤害（0 = 仅原版落雷爆炸）。</summary>
        public float damage;
        public float armorPenetration;
        /// <summary>附加爆炸半径。</summary>
        public float explosionRadius = 1.9f;
        /// <summary>附加爆炸点燃概率。</summary>
        public float chanceToStartFire;
        public Thing equipment;
    }

    /// <summary>天降打击统一入口：单次落雷直接触发，雷暴生成隐形调度器。</summary>
    public static class MMTStrikeUtil
    {
        public const string StormDefName = "MMT_SkyStorm";

        /// <summary>单次落雷：原版闪电事件立即触发（闪电+闪光+雷声+原版爆炸），可叠加自定义伤害层。</summary>
        public static void FireStrike(Map map, IntVec3 cell, Thing caster, MMTStrikeParams p)
        {
            if (map == null || !cell.IsValid || !cell.InBounds(map))
            {
                return;
            }
            map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, cell));
            if (p != null && p.damage > 0f)
            {
                GenExplosion.DoExplosion(cell, map, p.explosionRadius, p.damageDef ?? DamageDefOf.Flame, caster,
                    damAmount: (int)p.damage, armorPenetration: p.armorPenetration,
                    weapon: p.equipment?.def,
                    chanceToStartFire: p.chanceToStartFire, doVisualEffects: false, doSoundEffects: false);
            }
        }

        /// <summary>统一入口：单次定点直接落雷；连发/范围/延迟则生成隐形调度器 Thing_MMTSkyStorm。</summary>
        public static void FireStorm(Thing caster, IntVec3 center, MMTStrikeParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !center.IsValid || !center.InBounds(map))
            {
                return;
            }
            if (p.strikeCount <= 1 && p.areaRadius <= 0f && p.initialDelayTicks <= 0)
            {
                FireStrike(map, center, caster, p);
                return;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(StormDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + StormDefName + " 定义，无法发动天降打击");
                return;
            }
            Thing_MMTSkyStorm storm = (Thing_MMTSkyStorm)GenSpawn.Spawn(def, center, map);
            storm.Setup(caster, p);
        }
    }

    /// <summary>
    /// 雷暴调度器：隐形 Attachment，按间隔在目标区域随机落点连发落雷，次数用尽自毁。
    /// 参数全为运行时传入不进存档；读档后参数丢失即自毁（雷暴最长数秒，可接受）。
    /// </summary>
    public class Thing_MMTSkyStorm : Thing
    {
        private Thing caster;
        private MMTStrikeParams stormParams;
        private int strikesLeft;
        private int ticksUntilNext;

        public void Setup(Thing caster, MMTStrikeParams p)
        {
            this.caster = caster;
            stormParams = p;
            strikesLeft = Mathf.Max(1, p.strikeCount);
            ticksUntilNext = Mathf.Max(0, p.initialDelayTicks);
        }

        protected override void Tick()
        {
            base.Tick();
            if (stormParams == null || Map == null)
            {
                Destroy();
                return;
            }
            if (--ticksUntilNext > 0)
            {
                return;
            }
            ticksUntilNext = Mathf.Max(1, stormParams.strikeIntervalTicks);
            MMTStrikeUtil.FireStrike(Map, PickCell(), caster, stormParams);
            if (--strikesLeft <= 0)
            {
                Destroy();
            }
        }

        /// <summary>雷暴落点：目标点半径内随机抽格，优先无屋顶格（雷从天降），抽不到则退回首个在界内的格子。</summary>
        private IntVec3 PickCell()
        {
            Map map = Map;
            IntVec3 center = Position;
            float radius = stormParams.areaRadius;
            if (radius <= 0f)
            {
                return center;
            }
            IntVec3 fallback = center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 v = Rand.UnitVector2 * Rand.Range(0f, radius);
                IntVec3 cell = center + new IntVec3(Mathf.RoundToInt(v.x), 0, Mathf.RoundToInt(v.y));
                if (!cell.InBounds(map))
                {
                    continue;
                }
                if (!cell.Roofed(map))
                {
                    return cell;
                }
                fallback = cell;
            }
            return fallback;
        }

        /// <summary>调度器本体不可见，不绘制任何图形。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }
    }

    // ========== pawn 技能出口：天降打击 ==========

    public class CompProperties_MMTStrike : CompProperties_MMTAoETargetBase
    {
        /// <summary>落雷次数（1 = 单体落雷；>1 = 雷暴连发）。</summary>
        public int strikeCount = 1;
        public int strikeIntervalTicks = 12;
        /// <summary>雷暴随机落点半径（0 = 用预览圈半径 radius）。</summary>
        public float areaRadius;
        public int initialDelayTicks;
        /// <summary>附加自定义伤害类型（空 = Flame）。</summary>
        public string damageDefName;
        /// <summary>附加自定义爆炸伤害（0 = 仅原版落雷爆炸）。</summary>
        public float damage;
        public float armorPenetration;
        public float explosionRadius = 1.9f;
        public float chanceToStartFire;

        public CompProperties_MMTStrike()
        {
            compClass = typeof(CompAbilityEffect_MMTStrike);
        }
    }

    /// <summary>天降打击技能：确认落点后降下落雷/雷暴（radius>0 时瞄预览圈走 AoETarget 基类）。</summary>
    public class CompAbilityEffect_MMTStrike : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTStrike Props => (CompProperties_MMTStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTStrikeUtil.FireStorm(caster, target.Cell, new MMTStrikeParams
            {
                strikeCount = Props.strikeCount,
                strikeIntervalTicks = Props.strikeIntervalTicks,
                areaRadius = Props.areaRadius > 0f ? Props.areaRadius : Props.radius,
                initialDelayTicks = Props.initialDelayTicks,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                armorPenetration = Props.armorPenetration,
                explosionRadius = Props.explosionRadius,
                chanceToStartFire = Props.chanceToStartFire,
                equipment = caster.equipment?.Primary
            });
        }
    }
}
