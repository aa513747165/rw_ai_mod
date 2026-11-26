using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 暗红余烬爆发框架（第二十四步：爆发 + 短余烬 hybrid） ==========
    // 烈焰火海的暗红改版：读条后目标区域瞬间爆裂——中心暗红爆闪 + 数十颗火星
    // 径向四溅（Fleck 投掷粒子，各自随机方向/速度/寿命/两档红色），爆发瞬间
    // 对半径内 pawn/建筑结算一次即时 Burn 伤害并概率点燃；随后溅落点按密度
    // 留下贴地暗红余烬（复用 SkillGroundFire 的 SpawnFlame，但矮、平、短命——
    // 闷燃余烬而非火柱），继续跳小额持续伤害直至熄灭。
    // 与第六步地面火焰（持续火柱/火海）互补：这是"一次爆发 + 短余烬"形态。
    // 视觉零新贴图：火星复用 MMT_Fleck_Ember（BuffSpark 白光点）+ MMT_Fleck_BlinkFlash
    // 中心闪光，instanceColor 代码染色；残余火走 MMT_FlamePillar 贴图压扁染暗红。
    // 演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="emberBurst"）。
    // 注：参数运行时传入不进存档；残余火读档后自毁（存续仅数秒，可接受）。

    /// <summary>余烬爆发参数包（MMTEmberBurstUtil.FireBurst 的输入）。</summary>
    public class MMTEmberBurstParams
    {
        /// <summary>爆发半径（格）：即时伤害范围 + 余烬溅落范围。</summary>
        public float radius = 5f;
        /// <summary>火星粒子数量。</summary>
        public int emberCount = 48;
        /// <summary>火星初速范围（格/秒，转换为格/tick 写入 velocity）。</summary>
        public float speedMin = 2f;
        public float speedMax = 6f;
        /// <summary>火星染色两档（亮红橙 / 暗红，随机取增加层次）。</summary>
        public Color sparkTint = new Color(1f, 0.25f, 0.08f);
        public Color sparkTintDark = new Color(0.55f, 0.05f, 0.02f);
        /// <summary>爆发瞬间即时伤害（0 = 不结算即时伤害）。</summary>
        public float burstDamage = 8f;
        public float burstArmorPenetration;
        /// <summary>爆发瞬间点燃 pawn 概率（身上挂原版火）。</summary>
        public float burstIgnitePawnChance = 0.25f;
        /// <summary>伤害类型（null = Burn）。</summary>
        public DamageDef damageDef;
        /// <summary>余烬持续秒数（0 = 不留余烬，纯爆发）。</summary>
        public float residDurationSeconds = 2.5f;
        /// <summary>余烬每跳伤害与跳伤间隔 tick。</summary>
        public float residDamage = 2f;
        public int residDamageIntervalTicks = 15;
        /// <summary>余烬溅落密度（每格抽中概率）。</summary>
        public float residDensity = 0.7f;
        /// <summary>余烬染色（默认暗红）。</summary>
        public Color residTint = new Color(0.85f, 0.15f, 0.05f);
        /// <summary>余烬每跳引燃本格概率（生成原版火焰，可蔓延）。</summary>
        public float residIgniteCellChance = 0.02f;
        /// <summary>余烬自中心向外溅落的延迟（tick/格）。</summary>
        public float residSpreadPerCellTicks = 2f;
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>余烬爆发统一入口。</summary>
    public static class MMTEmberBurstUtil
    {
        /// <summary>在 center 引发暗红爆裂：爆闪 + 火星四溅 + 即时伤害 + 溅落短余烬。</summary>
        public static void FireBurst(Thing caster, IntVec3 center, MMTEmberBurstParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || p.radius <= 0f || !center.IsValid || !center.InBounds(map))
            {
                return;
            }
            if (Find.CurrentMap == map)
            {
                SpawnFlash(map, center, p);
                SpawnSparks(map, center, p);
            }
            BurstDamage(caster, map, center, p);
            ScatterEmbers(caster, map, center, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(center, map));
            }
        }

        /// <summary>中心爆闪：静态闪光 fleck 大尺度染暗红（照 SkillNova.Setup 写法）。</summary>
        private static void SpawnFlash(Map map, IntVec3 center, MMTEmberBurstParams p)
        {
            FleckDef flash = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BlinkFlash");
            if (flash == null)
            {
                return;
            }
            FleckCreationData data = FleckMaker.GetDataStatic(center.ToVector3Shifted(), map, flash, 2.5f);
            data.instanceColor = p.sparkTintDark;
            map.flecks.CreateFleck(data);
        }

        /// <summary>火星四溅：投掷 fleck 随机径向速度/寿命/两档红色（照 SkillBuffFX velocity 写法）。</summary>
        private static void SpawnSparks(Map map, IntVec3 center, MMTEmberBurstParams p)
        {
            FleckDef ember = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_Ember");
            if (ember == null)
            {
                Log.Error("[MMT] 找不到 MMT_Fleck_Ember 定义，余烬爆发无火星粒子");
                return;
            }
            Vector3 origin = center.ToVector3Shifted();
            int count = Mathf.Max(1, p.emberCount);
            for (int i = 0; i < count; i++)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(origin, map, ember, Rand.Range(0.15f, 0.45f));
                float angle = Rand.Range(0f, 360f);
                // 格/秒 → 格/tick（Fleck velocity 按 tick 推进，同 SkillNova 的 cellsPerSecond/60 惯例）
                float speed = Rand.Range(p.speedMin, p.speedMax) / 60f;
                data.velocity = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * speed;
                data.airTimeLeft = Rand.Range(0.4f, 1.1f);
                data.rotationRate = Rand.Range(-180f, 180f);
                data.instanceColor = Rand.Bool ? p.sparkTint : p.sparkTintDark;
                map.flecks.CreateFleck(data);
            }
        }

        /// <summary>爆发瞬间即时伤害：半径内 pawn/建筑吃一次伤害，pawn 概率点燃（照 DamagePulse 写法）。</summary>
        private static void BurstDamage(Thing caster, Map map, IntVec3 center, MMTEmberBurstParams p)
        {
            if (p.burstDamage <= 0f)
            {
                return;
            }
            DamageDef damageDef = p.damageDef ?? DamageDefOf.Burn;
            foreach (Thing t in GenRadial.RadialDistinctThingsAround(center, map, p.radius, true))
            {
                if (t == caster || t.Destroyed || (!(t is Pawn) && !(t is Building)))
                {
                    continue;
                }
                t.TakeDamage(new DamageInfo(damageDef, p.burstDamage, p.burstArmorPenetration,
                    -1f, caster, null, p.equipment?.def));
                if (p.burstIgnitePawnChance > 0f && t is Pawn pawn
                    && Rand.Chance(p.burstIgnitePawnChance) && FireUtility.CanEverAttachFire(pawn))
                {
                    FireUtility.TryAttachFire(pawn, 0.2f, caster);
                }
            }
        }

        /// <summary>溅落余烬：半径内按密度抽格，自中心向外延迟生成贴地暗红余烬
        /// （复用 SpawnFlame，压扁/短命/暗红 = 闷燃余烬而非火柱）。</summary>
        private static void ScatterEmbers(Thing caster, Map map, IntVec3 center, MMTEmberBurstParams p)
        {
            if (p.residDurationSeconds <= 0f)
            {
                return;
            }
            // 余烬形态参数：矮（0.25 高 2 层）、平（顶端不收口）、微摆、贴地小盘
            MMTGroundFireParams ember = new MMTGroundFireParams
            {
                tint = p.residTint,
                visualScale = 0.9f,
                pillarHeight = 0.25f,
                pillarLayers = 2,
                pillarTopScale = 1f,
                pillarSway = 0.05f,
                durationTicks = Mathf.Max(1, p.residDurationSeconds.SecondsToTicks()),
                damageDef = p.damageDef,
                damage = p.residDamage,
                damageIntervalTicks = p.residDamageIntervalTicks,
                armorPenetration = 0f,
                ignitePawnChance = 0f,
                igniteCellChance = p.residIgniteCellChance,
                effectRadius = 0f,
                equipment = p.equipment
            };
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, p.radius, true))
            {
                if (!cell.InBounds(map) || !Rand.Chance(p.residDensity))
                {
                    continue;
                }
                Building edifice = cell.GetEdifice(map);
                if (edifice != null && edifice.def.fillPercent >= 1f)
                {
                    continue;
                }
                int delay = Mathf.RoundToInt(center.DistanceTo(cell) * p.residSpreadPerCellTicks) + Rand.RangeInclusive(0, 6);
                MMTGroundFireUtil.SpawnFlame(caster, cell, ember, delay);
            }
        }
    }
}
