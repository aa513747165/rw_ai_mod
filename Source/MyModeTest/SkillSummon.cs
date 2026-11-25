using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 召唤技能框架（第十步） ==========
    // 一、雷球 SummonOrb：以施法者为起点生成雷球（EtherealThingBase + useHitPoints=false，
    //    无敌不可选中），缓慢移向目标格，沿途按间隔对当前位置周围造成脉冲伤害
    //    （敌我判定沿用第一步框架 TargetsInRadius），到达目标格消失；
    // 二、追踪弹 SummonHoming：同样无敌不可选中，持续时间内每 tick 朝选中目标的
    //    当前位置修正航向，进入命中半径即造成伤害并消失，续航超时也消失，
    //    目标提前死亡/消失则直接消散；
    // 三、动物召唤 SummonAnimal：临时生成驯服动物（玩家派系，与原版动物一致），
    //    挂 MMT_SummonAnimal Hediff 计时到期消失；Harmony 补丁禁止宰杀/放生；
    // 四、炮塔召唤 SummonTurret：生成与原版自动机枪塔同逻辑的 Building_TurretGun
    //    （不带电力/燃料/操作 comp，自动索敌射击），限时走原版 CompLifespan
    //    （公开字段 age 偏移实现运行时自定义时长），到期自毁。
    // MMTSummonUtil 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTSummon；
    // 建筑演示侧出口 AbilityTargeting.cs 的 CompSkillDemo
    // （demoType="summonOrb"/"summonHoming"/"summonAnimal"/"summonTurret"）。
    // 素材仅原版 + MMT 占位贴图，不依赖 RoM。
    // 注：雷球/追踪弹参数运行时传入不进存档，读档后首个 tick 自毁（防无参数空转）。

    /// <summary>召唤类型。</summary>
    public enum MMTSummonMode
    {
        Orb,
        Homing,
        Animal,
        Turret
    }

    /// <summary>召唤参数包（MMTSummonUtil 的输入）。</summary>
    public class MMTSummonParams
    {
        /// <summary>雷球贴图（白图，代码染色）与染色。</summary>
        public string orbTexPath = "Things/Mote/MMT_SummonOrb";
        public Color orbTint = new Color(0.55f, 0.75f, 1f);
        /// <summary>雷球绘制尺寸（格）。</summary>
        public float orbSize = 1.2f;
        /// <summary>雷球：每格耗时 tick（越大越慢）。</summary>
        public float orbTicksPerCell = 15f;
        /// <summary>追踪弹贴图（朝北/上）与染色。</summary>
        public string homingTexPath = "Things/Mote/MMT_HomingBolt";
        public Color homingTint = new Color(1f, 0.75f, 0.35f);
        /// <summary>追踪弹：每格耗时 tick（越小越快）。</summary>
        public float homingTicksPerCell = 5f;
        /// <summary>脉冲/命中伤害（空伤害类型由调用方给默认）。</summary>
        public DamageDef damageDef;
        public float damage = 8f;
        public float armorPen;
        /// <summary>雷球：脉冲伤害半径。</summary>
        public float pulseRadius = 2.5f;
        public int pulseIntervalTicks = 20;
        /// <summary>追踪弹：续航 tick（超时消失）。</summary>
        public int durationTicks = 480;
        /// <summary>追踪弹：命中半径（格）。</summary>
        public float hitRadius = 0.7f;
        /// <summary>追踪弹：命中爆炸半径（0=单体直击）。</summary>
        public float explosionRadius;
        /// <summary>动物召唤：动物种类。</summary>
        public PawnKindDef animalKind;
        /// <summary>炮塔召唤：炮塔建筑 Def。</summary>
        public ThingDef turretDef;
        /// <summary>移动拖尾 fleck。</summary>
        public FleckDef trailFleckDef;
        /// <summary>到达/消散 fleck。</summary>
        public FleckDef vanishFleckDef;
        public SoundDef startSound;
    }

    /// <summary>召唤统一入口：雷球 / 追踪弹 / 动物 / 炮塔。</summary>
    public static class MMTSummonUtil
    {
        public const string OrbDefName = "MMT_SummonOrb";
        public const string HomingDefName = "MMT_SummonHoming";
        public const string SummonAnimalHediffDefName = "MMT_SummonAnimal";

        private static HediffDef summonAnimalHediff;

        public static HediffDef SummonAnimalHediff
        {
            get
            {
                if (summonAnimalHediff == null)
                {
                    summonAnimalHediff = DefDatabase<HediffDef>.GetNamedSilentFail(SummonAnimalHediffDefName);
                }
                return summonAnimalHediff;
            }
        }

        /// <summary>生成雷球：从施法者位置缓慢移向目标格，沿途脉冲伤害，到点消失。</summary>
        public static Thing_MMTSummonOrb FireOrb(Thing caster, IntVec3 dest, MMTSummonParams p)
        {
            Map map = caster?.Map;
            if (caster == null || !caster.Spawned || map == null || !dest.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(OrbDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + OrbDefName + " 定义，无法召唤雷球");
                return null;
            }
            Thing_MMTSummonOrb orb = (Thing_MMTSummonOrb)GenSpawn.Spawn(def, caster.Position, map);
            orb.Setup(caster, dest, p);
            PlaySound(p, caster.Position, map);
            return orb;
        }

        /// <summary>生成追踪弹：持续追踪目标，命中或超时消失。</summary>
        public static Thing_MMTSummonHoming FireHoming(Thing caster, Thing target, MMTSummonParams p)
        {
            Map map = caster?.Map;
            if (caster == null || !caster.Spawned || map == null || target == null || target.Destroyed || !target.Spawned)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(HomingDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + HomingDefName + " 定义，无法召唤追踪弹");
                return null;
            }
            Thing_MMTSummonHoming homing = (Thing_MMTSummonHoming)GenSpawn.Spawn(def, caster.Position, map);
            homing.Setup(caster, target, p);
            PlaySound(p, caster.Position, map);
            return homing;
        }

        /// <summary>动物召唤：在目标格附近生成驯服动物（挂召唤时限 Hediff，到期消失）。</summary>
        public static Pawn SummonAnimal(Thing caster, IntVec3 cell, MMTSummonParams p)
        {
            Map map = caster?.Map;
            if (caster == null || !caster.Spawned || map == null)
            {
                return null;
            }
            if (p?.animalKind == null || p.animalKind.RaceProps?.Animal != true)
            {
                Log.Error("[MMT] 动物召唤缺少有效动物种类（" + (p?.animalKind?.defName ?? "空") + "）");
                return null;
            }
            if (SummonAnimalHediff == null)
            {
                Log.Error("[MMT] 找不到 " + SummonAnimalHediffDefName + " 定义，无法动物召唤");
                return null;
            }
            if (!cell.Standable(map)
                && !CellFinder.TryFindRandomCellNear(cell, map, 6, c => c.Standable(map), out cell))
            {
                return null;
            }
            Pawn pawn = PawnGenerator.GeneratePawn(p.animalKind, caster.Faction ?? Faction.OfPlayer);
            GenSpawn.Spawn(pawn, cell, map);
            Hediff hediff = HediffMaker.MakeHediff(SummonAnimalHediff, pawn);
            HediffComp_MMTSummonAnimal comp = hediff.TryGetComp<HediffComp_MMTSummonAnimal>();
            if (comp != null)
            {
                comp.ticksLeft = Mathf.Max(1, p.durationTicks);
                comp.vanishFleck = p.vanishFleckDef;
            }
            pawn.health.AddHediff(hediff);
            PlayFleck(map, cell, p?.vanishFleckDef);
            PlaySound(p, cell, map);
            return pawn;
        }

        /// <summary>炮塔召唤：生成原版逻辑的自动炮塔（无电力/弹药），CompLifespan 到期自毁。</summary>
        public static Building SummonTurret(Thing caster, IntVec3 cell, MMTSummonParams p)
        {
            Map map = caster?.Map;
            if (caster == null || !caster.Spawned || map == null || p?.turretDef == null)
            {
                return null;
            }
            if (!cell.Standable(map) || cell.GetEdifice(map) != null)
            {
                return null;
            }
            Building turret = (Building)GenSpawn.Spawn(p.turretDef, cell, map);
            turret.SetFaction(caster.Faction ?? Faction.OfPlayer);
            // 运行时自定义时长：原版 CompLifespan 按 age >= lifespanTicks 到期，偏移 age 即可
            CompLifespan life = turret.GetComp<CompLifespan>();
            if (life != null && p.durationTicks > 0)
            {
                life.age = Mathf.Max(0, life.Props.lifespanTicks - p.durationTicks);
            }
            PlayFleck(map, cell, p.vanishFleckDef);
            PlaySound(p, cell, map);
            return turret;
        }

        public static void PlayFleck(Map map, IntVec3 cell, FleckDef def, float scale = 1f)
        {
            if (map == null || map != Find.CurrentMap || def == null)
            {
                return;
            }
            Vector3 pos = cell.ToVector3Shifted();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            map.flecks.CreateFleck(FleckMaker.GetDataStatic(pos, map, def, scale));
        }

        private static void PlaySound(MMTSummonParams p, IntVec3 cell, Map map)
        {
            if (p?.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(cell, map));
            }
        }
    }

    /// <summary>
    /// 雷球：无敌不可选中的移动伤害源。从施法者匀速滑向目标格，沿途按间隔对
    /// 周围敌人（TargetsInRadius 敌我判定）造成脉冲伤害，到达后消失。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTSummonOrb : Thing
    {
        private Thing owner;
        private Vector3 startVec;
        private Vector3 destVec;
        private int age;
        private int ticksTotal = 1;
        private float pulseRadius = 2.5f;
        private int pulseIntervalTicks = 20;
        private DamageDef damageDef;
        private float damage = 8f;
        private float armorPen;
        private float orbSize = 1.2f;
        private FleckDef trailFleck;
        private FleckDef vanishFleck;
        private Material mat;
        private bool loadedFromSave;

        public void Setup(Thing caster, IntVec3 dest, MMTSummonParams p)
        {
            owner = caster;
            startVec = caster.Position.ToVector3Shifted();
            destVec = dest.ToVector3Shifted();
            float dist = (dest - caster.Position).LengthHorizontal;
            ticksTotal = Mathf.Max(1, Mathf.CeilToInt(dist * (p?.orbTicksPerCell ?? 15f)));
            pulseRadius = p?.pulseRadius ?? 2.5f;
            pulseIntervalTicks = Mathf.Max(1, p?.pulseIntervalTicks ?? 20);
            damageDef = p?.damageDef ?? DamageDefOf.Flame;
            damage = p?.damage ?? 8f;
            armorPen = p?.armorPen ?? 0f;
            orbSize = p?.orbSize ?? 1.2f;
            trailFleck = p?.trailFleckDef;
            vanishFleck = p?.vanishFleckDef;
            string texPath = p?.orbTexPath ?? "Things/Mote/MMT_SummonOrb";
            mat = MaterialPool.MatFrom(texPath, ShaderDatabase.MoteGlow, p?.orbTint ?? Color.white);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        private Vector3 CurPos()
        {
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            return Vector3.Lerp(startVec, destVec, t);
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave)
            {
                Destroy();
                return;
            }
            age++;
            Map map = Map;
            if (trailFleck != null && map == Find.CurrentMap && age % 3 == 0)
            {
                Vector3 pos = CurPos();
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                FleckCreationData data = FleckMaker.GetDataStatic(pos, map, trailFleck, Rand.Range(0.3f, 0.5f));
                data.instanceColor = mat != null ? mat.color : Color.white;
                map.flecks.CreateFleck(data);
            }
            if (age % pulseIntervalTicks == 0)
            {
                PulseDamage();
            }
            if (age >= ticksTotal)
            {
                PulseDamage();
                MMTSummonUtil.PlayFleck(Map, CurPos().ToIntVec3(), vanishFleck, 1.2f);
                Destroy();
            }
        }

        /// <summary>脉冲：对当前位置周围的敌人造成伤害（沿用第一步框架敌我判定）。</summary>
        private void PulseDamage()
        {
            Map map = Map;
            if (map == null || owner == null || owner.Destroyed || damageDef == null)
            {
                return;
            }
            IntVec3 center = CurPos().ToIntVec3();
            foreach (Thing t in MMTAoETargetPreview.TargetsInRadius(owner, center, map, pulseRadius, false, false).ToList())
            {
                t.TakeDamage(new DamageInfo(damageDef, damage, armorPen, -1f, owner));
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (mat == null || Map != Find.CurrentMap)
            {
                return;
            }
            Vector3 pos = CurPos();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float pulse = 1f + 0.12f * Mathf.Sin(age * 0.25f);
            float size = orbSize * pulse;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.AngleAxis(age * 2f, Vector3.up), new Vector3(size, 1f, size)), mat, 0);
        }
    }

    /// <summary>
    /// 追踪弹：无敌不可选中的追踪实体。每 tick 朝目标当前位置修正航向，
    /// 进入命中半径即造成伤害（可选爆炸）并消失；续航超时或目标失效则消散。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTSummonHoming : Thing
    {
        private Thing owner;
        private Thing target;
        private Vector3 curPos;
        private int age;
        private int durationTicks = 480;
        private float ticksPerCell = 5f;
        private float hitRadius = 0.7f;
        private float explosionRadius;
        private DamageDef damageDef;
        private float damage = 8f;
        private float armorPen;
        private FleckDef trailFleck;
        private FleckDef vanishFleck;
        private Material mat;
        private float drawAngle;
        private bool loadedFromSave;

        public void Setup(Thing caster, Thing targetThing, MMTSummonParams p)
        {
            owner = caster;
            target = targetThing;
            curPos = caster.Position.ToVector3Shifted();
            ticksPerCell = Mathf.Max(0.5f, p?.homingTicksPerCell ?? 5f);
            durationTicks = Mathf.Max(1, p?.durationTicks ?? 480);
            hitRadius = p?.hitRadius ?? 0.7f;
            explosionRadius = p?.explosionRadius ?? 0f;
            damageDef = p?.damageDef ?? DamageDefOf.Flame;
            damage = p?.damage ?? 8f;
            armorPen = p?.armorPen ?? 0f;
            trailFleck = p?.trailFleckDef;
            vanishFleck = p?.vanishFleckDef;
            string texPath = p?.homingTexPath ?? "Things/Mote/MMT_HomingBolt";
            mat = MaterialPool.MatFrom(texPath, ShaderDatabase.MoteGlow, p?.homingTint ?? Color.white);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave)
            {
                Destroy();
                return;
            }
            age++;
            // 目标失效：直接消散
            if (target == null || target.Destroyed || !target.Spawned || target is Pawn tp && tp.Dead)
            {
                Vanish();
                return;
            }
            if (age >= durationTicks)
            {
                Vanish();
                return;
            }
            Vector3 targetPos = target.DrawPos;
            targetPos.y = curPos.y;
            Vector3 to = targetPos - curPos;
            float step = 1f / ticksPerCell;
            if (to.magnitude <= Mathf.Max(step, hitRadius))
            {
                Impact();
                return;
            }
            curPos += to.normalized * step;
            drawAngle = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
            Map map = Map;
            if (trailFleck != null && map == Find.CurrentMap && age % 2 == 0)
            {
                Vector3 pos = curPos;
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                FleckCreationData data = FleckMaker.GetDataStatic(pos, map, trailFleck, Rand.Range(0.25f, 0.4f));
                data.instanceColor = mat != null ? mat.color : Color.white;
                map.flecks.CreateFleck(data);
            }
        }

        /// <summary>命中：有爆炸半径走爆炸（含目标），否则单体直击。</summary>
        private void Impact()
        {
            Map map = Map;
            IntVec3 cell = target.Position;
            if (map != null)
            {
                if (explosionRadius > 0f)
                {
                    GenExplosion.DoExplosion(cell, map, explosionRadius, damageDef ?? DamageDefOf.Bomb,
                        owner, Mathf.RoundToInt(damage), armorPen);
                }
                else if (damageDef != null)
                {
                    target.TakeDamage(new DamageInfo(damageDef, damage, armorPen, -1f, owner));
                }
            }
            MMTSummonUtil.PlayFleck(map, cell, vanishFleck, 1.2f);
            Destroy();
        }

        private void Vanish()
        {
            MMTSummonUtil.PlayFleck(Map, curPos.ToIntVec3(), vanishFleck);
            Destroy();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (mat == null || Map != Find.CurrentMap)
            {
                return;
            }
            Vector3 pos = curPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.AngleAxis(drawAngle, Vector3.up), new Vector3(1f, 1f, 1f)), mat, 0);
        }
    }

    // ========== 动物召唤：召唤时限 Hediff（到期消失） ==========

    public class HediffCompProperties_MMTSummonAnimal : HediffCompProperties
    {
        public HediffCompProperties_MMTSummonAnimal()
        {
            compClass = typeof(HediffComp_MMTSummonAnimal);
        }
    }

    /// <summary>召唤动物计时器：到期原地消散（Vanish 不触发死亡效果）；不可宰杀/放生见设计器补丁。</summary>
    public class HediffComp_MMTSummonAnimal : HediffComp
    {
        public int ticksLeft = 1200;
        /// <summary>消散特效（运行时传入不进存档）。</summary>
        public FleckDef vanishFleck;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            ticksLeft -= delta;
            if (ticksLeft > 0 || !Pawn.Spawned || Pawn.Dead)
            {
                return;
            }
            MMTSummonUtil.PlayFleck(Pawn.Map, Pawn.Position, vanishFleck, 1.5f);
            Pawn.Destroy(DestroyMode.Vanish);
        }

        public override string CompLabelInBracketsExtra => (ticksLeft / 60) + "s";

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", 1200);
        }
    }

    /// <summary>召唤动物不可宰杀。</summary>
    [HarmonyPatch(typeof(Designator_Slaughter), nameof(Designator_Slaughter.CanDesignateThing))]
    public static class Patch_MMTSummonNoSlaughter
    {
        public static void Postfix(Thing t, ref AcceptanceReport __result)
        {
            if (__result.Accepted && IsSummoned(t))
            {
                __result = false;
            }
        }

        public static bool IsSummoned(Thing t)
        {
            return t is Pawn p && p.health?.hediffSet != null
                && MMTSummonUtil.SummonAnimalHediff != null
                && p.health.hediffSet.HasHediff(MMTSummonUtil.SummonAnimalHediff);
        }
    }

    /// <summary>召唤动物不可放生。</summary>
    [HarmonyPatch(typeof(Designator_ReleaseAnimalToWild), nameof(Designator_ReleaseAnimalToWild.CanDesignateThing))]
    public static class Patch_MMTSummonNoRelease
    {
        public static void Postfix(Thing t, ref AcceptanceReport __result)
        {
            if (__result.Accepted && Patch_MMTSummonNoSlaughter.IsSummoned(t))
            {
                __result = false;
            }
        }
    }

    // ========== pawn 技能出口：召唤 ==========

    public class CompProperties_MMTSkillSummon : CompProperties_AbilityEffect
    {
        public MMTSummonMode mode = MMTSummonMode.Orb;
        public string orbTexPath = "Things/Mote/MMT_SummonOrb";
        public Color orbTint = new Color(0.55f, 0.75f, 1f);
        public float orbSize = 1.2f;
        public float orbTicksPerCell = 15f;
        public string homingTexPath = "Things/Mote/MMT_HomingBolt";
        public Color homingTint = new Color(1f, 0.75f, 0.35f);
        public float homingTicksPerCell = 5f;
        public string damageDefName;
        public float damage = 8f;
        public float armorPen;
        public float pulseRadius = 2.5f;
        public int pulseIntervalTicks = 20;
        /// <summary>追踪弹续航 / 动物与炮塔持续秒数。</summary>
        public float durationSeconds = 8f;
        public float hitRadius = 0.7f;
        public float explosionRadius;
        public string animalKindDefName;
        public string turretDefName;
        public string trailFleckDefName;
        public string vanishFleckDefName;
        public string soundDefName;

        public CompProperties_MMTSkillSummon()
        {
            compClass = typeof(CompAbilityEffect_MMTSkillSummon);
        }
    }

    /// <summary>召唤技能：雷球打落点；追踪弹锁目标；动物/炮塔在目标格生成。</summary>
    public class CompAbilityEffect_MMTSkillSummon : CompAbilityEffect
    {
        public new CompProperties_MMTSkillSummon Props => (CompProperties_MMTSkillSummon)props;

        public MMTSummonParams BuildParams()
        {
            return new MMTSummonParams
            {
                orbTexPath = Props.orbTexPath,
                orbTint = Props.orbTint,
                orbSize = Props.orbSize,
                orbTicksPerCell = Props.orbTicksPerCell,
                homingTexPath = Props.homingTexPath,
                homingTint = Props.homingTint,
                homingTicksPerCell = Props.homingTicksPerCell,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                armorPen = Props.armorPen,
                pulseRadius = Props.pulseRadius,
                pulseIntervalTicks = Props.pulseIntervalTicks,
                durationTicks = Mathf.Max(1, Props.durationSeconds.SecondsToTicks()),
                hitRadius = Props.hitRadius,
                explosionRadius = Props.explosionRadius,
                animalKind = Props.animalKindDefName.NullOrEmpty() ? null : DefDatabase<PawnKindDef>.GetNamedSilentFail(Props.animalKindDefName),
                turretDef = Props.turretDefName.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(Props.turretDefName),
                trailFleckDef = Props.trailFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.trailFleckDefName),
                vanishFleckDef = Props.vanishFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.vanishFleckDefName),
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            };
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTSummonParams p = BuildParams();
            switch (Props.mode)
            {
                case MMTSummonMode.Orb:
                    MMTSummonUtil.FireOrb(caster, target.Cell, p);
                    break;
                case MMTSummonMode.Homing:
                    if (target.Thing != null)
                    {
                        MMTSummonUtil.FireHoming(caster, target.Thing, p);
                    }
                    break;
                case MMTSummonMode.Animal:
                    MMTSummonUtil.SummonAnimal(caster, target.Cell, p);
                    break;
                case MMTSummonMode.Turret:
                    MMTSummonUtil.SummonTurret(caster, target.Cell, p);
                    break;
            }
        }
    }
}
