using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 锥形喷吐框架（第十五步：龙息） ==========
    // 玩家用鼠标点一个方向，施法者朝该方向喷出锥形吐息（扇形：射程 × 半角），
    // 持续期间喷口跟随施法者移动、方向固定；按间隔对锥内所有 pawn 结算伤害
    // （默认 Flame，不分敌我，真实吐息），并按概率点燃锥内格子。
    // 视觉 = 密集火星抛射（原版 FireGlow fleck 沿锥向抛撒）+ 光柱贴图叠两层
    // 渐宽火舌（明暗两层，抖动闪烁）。瞄准时画锥形预览（两条边界线 + 末端弧）。
    // MMTBreathUtil.FireBreath 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTBreath；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="breathCone"）。
    // 素材仅原版火星 fleck + MMT 占位光柱贴图，不依赖 RoM。
    // 注：吐息参数运行时传入不进存档，读档后首个 tick 自毁（仅存活数秒）。

    /// <summary>锥形喷吐参数包（MMTBreathUtil.FireBreath 的输入）。</summary>
    public class MMTBreathParams
    {
        /// <summary>射程（格）。</summary>
        public float range = 8f;
        /// <summary>锥形半角（度）。</summary>
        public float halfAngleDegrees = 30f;
        /// <summary>持续 tick。</summary>
        public int durationTicks = 180;
        /// <summary>伤害类型（null = Flame）。</summary>
        public DamageDef damageDef;
        public float damage = 3f;
        /// <summary>跳伤间隔 tick。</summary>
        public int damageIntervalTicks = 10;
        public float armorPenetration;
        /// <summary>每跳点燃锥内随机格子的概率。</summary>
        public float igniteChance = 0.2f;
        /// <summary>火星 fleck（null = 原版 FireGlow）。</summary>
        public FleckDef sparkFleckDef;
        /// <summary>火星染色。</summary>
        public Color sparkTint = new Color(1f, 0.6f, 0.2f);
        /// <summary>火舌贴图（默认复用光柱贴图）与染色。</summary>
        public string flameTexPath = "Things/Mote/MMT_SkillBeam";
        public Color flameTint = new Color(1f, 0.55f, 0.15f);
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>锥形喷吐统一入口 + 锥形几何计算（预览与实喷共用同一套判定）。</summary>
    public static class MMTBreathUtil
    {
        public const string BreathDefName = "MMT_BreathCone";

        public static Thing_MMTBreathCone FireBreath(Thing caster, IntVec3 aimCell, MMTBreathParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null)
            {
                return null;
            }
            Vector3 dir = MMTBeamUtil.BeamDirection(caster, aimCell);
            if (dir.sqrMagnitude < 0.01f)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(BreathDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + BreathDefName + " 定义，无法施放吐息");
                return null;
            }
            Thing_MMTBreathCone cone = (Thing_MMTBreathCone)GenSpawn.Spawn(def, caster.Position, map);
            cone.Setup(caster, dir, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(caster.Position, map));
            }
            return cone;
        }

        /// <summary>pawn 是否在锥形内（以 origin 为顶点、dir 为中心轴、range/halfAngle 为界）。</summary>
        public static bool InCone(Vector3 origin, Vector3 dir, Vector3 targetPos, float range, float halfAngleDegrees)
        {
            Vector3 to = targetPos - origin;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 0.01f || dist > range)
            {
                return false;
            }
            float angle = Vector3.Angle(dir, to);
            return angle <= halfAngleDegrees;
        }

        /// <summary>瞄准期间画锥形预览：两条边界线 + 末端弧线（与实喷同一几何）。
        /// origin 传施法者脚下位置即可，内部与实喷一样前移 MuzzleOffset。 </summary>
        public static void DrawConePreview(Map map, Vector3 origin, Vector3 dir, float range, float halfAngleDegrees, string texPath, Color color)
        {
            if (map == null || map != Find.CurrentMap || dir.sqrMagnitude < 0.01f)
            {
                return;
            }
            origin += dir * MMTBeamUtil.MuzzleOffset;
            float baseAngle = dir.AngleFlat();
            MMTBeamUtil.DrawBeamPreview(map, origin,
                Quaternion.AngleAxis(baseAngle - halfAngleDegrees, Vector3.up) * Vector3.forward, range, 0.12f, texPath, color);
            MMTBeamUtil.DrawBeamPreview(map, origin,
                Quaternion.AngleAxis(baseAngle + halfAngleDegrees, Vector3.up) * Vector3.forward, range, 0.12f, texPath, color);
            // 末端弧：若干短弦近似
            Vector3 prev = origin + Quaternion.AngleAxis(baseAngle - halfAngleDegrees, Vector3.up) * Vector3.forward * range;
            for (int i = 1; i <= 8; i++)
            {
                float a = baseAngle - halfAngleDegrees + halfAngleDegrees * 2f * i / 8f;
                Vector3 next = origin + Quaternion.AngleAxis(a, Vector3.up) * Vector3.forward * range;
                Vector3 mid = (prev + next) * 0.5f;
                MMTBeamUtil.DrawBeamPreview(map, prev, (next - prev).normalized, (next - prev).magnitude, 0.1f, texPath, color);
                prev = next;
            }
        }
    }

    /// <summary>
    /// 锥形吐息实体：喷口跟随施法者、方向固定；抛火星 + 自绘两层火舌，
    /// 按间隔对锥内 pawn 跳伤并点燃格子；到时或施法者失效自毁。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTBreathCone : Thing
    {
        private Thing caster;
        private Vector3 dir;
        private MMTBreathParams breathParams;
        private int ticksLeft = -1;
        private int ticksUntilDamage;
        private Material flameMat;
        private bool loadedFromSave;

        public void Setup(Thing caster, Vector3 dir, MMTBreathParams p)
        {
            this.caster = caster;
            this.dir = dir;
            breathParams = p;
            ticksLeft = Mathf.Max(1, p.durationTicks);
            ticksUntilDamage = 1;
            flameMat = MaterialPool.MatFrom(p.flameTexPath, ShaderDatabase.MoteGlow, p.flameTint);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || breathParams == null || Map == null || caster == null || caster.Destroyed || !caster.Spawned)
            {
                Destroy();
                return;
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Destroy();
                return;
            }
            if (--ticksUntilDamage <= 0)
            {
                ticksUntilDamage = Mathf.Max(1, breathParams.damageIntervalTicks);
                DamagePulse();
            }
            if (Map == Find.CurrentMap)
            {
                SpraySparks();
            }
        }

        /// <summary>一跳：锥内所有 pawn 结算伤害（不分敌我）+ 概率点燃锥内格子。</summary>
        private void DamagePulse()
        {
            Map map = Map;
            Vector3 origin = MMTBeamUtil.MuzzleOrigin(caster, dir);
            DamageDef damageDef = breathParams.damageDef ?? DamageDefOf.Flame;
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p == caster || p.Dead
                    || !MMTBreathUtil.InCone(origin, dir, p.Position.ToVector3Shifted(), breathParams.range, breathParams.halfAngleDegrees))
                {
                    continue;
                }
                p.TakeDamage(new DamageInfo(damageDef, breathParams.damage, breathParams.armorPenetration,
                    dir.AngleFlat(), caster, null, breathParams.equipment?.def));
            }
            if (breathParams.igniteChance > 0f && Rand.Chance(breathParams.igniteChance))
            {
                float dist = Rand.Range(1.5f, breathParams.range);
                float angle = dir.AngleFlat() + Rand.Range(-breathParams.halfAngleDegrees, breathParams.halfAngleDegrees);
                IntVec3 cell = (origin + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * dist).ToIntVec3();
                if (cell.InBounds(map) && cell != caster.Position)
                {
                    FireUtility.TryStartFireIn(cell, map, 0.2f, caster);
                }
            }
        }

        /// <summary>火星抛射：每 tick 沿锥向抛撒原版火星，形成连续吐息流。</summary>
        private void SpraySparks()
        {
            Map map = Map;
            FleckDef fleck = breathParams.sparkFleckDef ?? DefDatabase<FleckDef>.GetNamedSilentFail("FireGlow");
            if (fleck == null)
            {
                return;
            }
            Vector3 origin = MMTBeamUtil.MuzzleOrigin(caster, dir);
            origin.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float baseAngle = dir.AngleFlat();
            for (int i = 0; i < 2; i++)
            {
                float angle = baseAngle + Rand.Range(-breathParams.halfAngleDegrees, breathParams.halfAngleDegrees);
                Vector3 vel = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward
                    * Rand.Range(breathParams.range * 0.45f, breathParams.range * 0.75f);
                FleckCreationData data = FleckMaker.GetDataStatic(origin, map, fleck, Rand.Range(0.5f, 0.95f));
                data.velocity = vel;
                data.instanceColor = breathParams.sparkTint;
                map.flecks.CreateFleck(data);
            }
        }

        /// <summary>自绘火舌：两层渐宽拉伸贴图（外暗内亮），随时间抖动闪烁。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (flameMat == null || breathParams == null || caster == null || Map != Find.CurrentMap)
            {
                return;
            }
            float elapsed = Mathf.Max(1, breathParams.durationTicks) - ticksLeft;
            float alpha = Mathf.Clamp01(elapsed / 8f) * Mathf.Clamp01(ticksLeft / 15f);
            float flicker = 0.85f + 0.15f * Mathf.Sin(elapsed * 0.9f) + 0.05f * Mathf.Sin(elapsed * 2.3f);
            Vector3 origin = MMTBeamUtil.MuzzleOrigin(caster, dir);
            origin.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float angle = dir.AngleFlat();
            // 三层渐宽段：近窄远宽近似锥形
            for (int seg = 0; seg < 3; seg++)
            {
                float t0 = seg / 3f;
                float t1 = (seg + 1) / 3f;
                float width = Mathf.Lerp(0.7f, breathParams.range * Mathf.Tan(breathParams.halfAngleDegrees * Mathf.Deg2Rad) * 2f, (t0 + t1) * 0.5f);
                float segStart = breathParams.range * t0;
                float segLen = breathParams.range * (t1 - t0);
                Vector3 center = origin + dir * (segStart + segLen * 0.5f);
                DrawFlameStrip(center, angle, width * 1.25f, segLen, alpha * 0.35f * flicker);
                DrawFlameStrip(center, angle, width * 0.6f, segLen, alpha * 0.8f * flicker);
            }
        }

        private void DrawFlameStrip(Vector3 center, float angle, float width, float length, float alpha)
        {
            Color color = breathParams.flameTint;
            color.a = Mathf.Clamp01(alpha);
            flameMat.color = color;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(center, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(width, 1f, length)), flameMat, 0);
        }
    }

    // ========== pawn 技能出口：锥形喷吐 ==========

    public class CompProperties_MMTBreath : CompProperties_AbilityEffect
    {
        public float range = 8f;
        public float halfAngleDegrees = 30f;
        public float durationSeconds = 3f;
        public string damageDefName;
        public float damage = 3f;
        public int damageIntervalTicks = 10;
        public float armorPenetration;
        public float igniteChance = 0.2f;
        public string flameTexPath = "Things/Mote/MMT_SkillBeam";
        public Color flameTint = new Color(1f, 0.55f, 0.15f);
        public Color sparkTint = new Color(1f, 0.6f, 0.2f);
        public string soundDefName;

        public CompProperties_MMTBreath()
        {
            compClass = typeof(CompAbilityEffect_MMTBreath);
        }
    }

    /// <summary>锥形喷吐技能：确认方向后喷出持续吐息（瞄准时画锥形预览）。</summary>
    public class CompAbilityEffect_MMTBreath : CompAbilityEffect
    {
        public new CompProperties_MMTBreath Props => (CompProperties_MMTBreath)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTBreathUtil.FireBreath(caster, target.Cell, new MMTBreathParams
            {
                range = Props.range,
                halfAngleDegrees = Props.halfAngleDegrees,
                durationTicks = Mathf.Max(1, Props.durationSeconds.SecondsToTicks()),
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                damageIntervalTicks = Props.damageIntervalTicks,
                armorPenetration = Props.armorPenetration,
                igniteChance = Props.igniteChance,
                flameTexPath = Props.flameTexPath,
                flameTint = Props.flameTint,
                sparkTint = Props.sparkTint,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }

        /// <summary>瞄准期间画锥形预览（两条边界线 + 末端弧）。</summary>
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            if (!target.IsValid)
            {
                return;
            }
            Pawn caster = parent.pawn;
            Vector3 dir = MMTBeamUtil.BeamDirection(caster, target.Cell);
            Color preview = Props.flameTint;
            preview.a = 0.3f;
            MMTBreathUtil.DrawConePreview(caster.MapHeld, caster.Position.ToVector3Shifted(), dir,
                Props.range, Props.halfAngleDegrees, Props.flameTexPath, preview);
        }
    }
}
