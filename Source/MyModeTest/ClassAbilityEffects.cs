using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    // 职业技能通用效果库（mmt-class-design.md §2.3）：
    // 全部主动技能走原版灵能框架（AbilityDef ParentName=PsycastBase），消耗/时长由 XML statBases 下发；
    // 本文件提供参数化 CompAbilityEffect_MMT_* 桶，生成器（tools/build_mmt_rom_classes.py）按映射表填参。
    // 单体 buff/debuff、位移、眩晕、精神控制单体等能用原版 comp 的桶不在此重复实现。

    // ========== 施法特效（RoM 战斗特效复刻：施法者/目标点 fleck） ==========

    public class CompProperties_MMTFleckFX : CompProperties_AbilityEffect
    {
        /// <summary>施法者身上的 fleck（生成器按职业主题填 MMT_Fleck_*）。</summary>
        public string casterFleckDefName;
        /// <summary>目标点的 fleck；self 技能生成器只填施法者。</summary>
        public string targetFleckDefName;
        public float scale = 2f;

        public CompProperties_MMTFleckFX()
        {
            compClass = typeof(CompAbilityEffect_MMTFleckFX);
        }
    }

    /// <summary>纯视觉桶：施放瞬间在施法者/目标点放主题 fleck，无玩法效果。</summary>
    public class CompAbilityEffect_MMTFleckFX : CompAbilityEffect
    {
        public new CompProperties_MMTFleckFX Props => (CompProperties_MMTFleckFX)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            SpawnFleck(Props.casterFleckDefName, caster.Position, map);
            if (target.IsValid && target.Cell != caster.Position)
            {
                SpawnFleck(Props.targetFleckDefName, target.Cell, map);
            }
        }

        private void SpawnFleck(string defName, IntVec3 cell, Map map)
        {
            if (defName.NullOrEmpty() || !cell.InBounds(map))
            {
                return;
            }
            FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail(defName);
            if (fleck != null)
            {
                FleckMaker.Static(cell.ToVector3Shifted(), map, fleck, Props.scale);
            }
        }
    }

    // ========== 群体 buff/debuff ==========

    public class CompProperties_MMTGiveHediffAoE : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public float radius = 5f;
        /// <summary>true=作用于友军（含自己派系），false=作用于敌对。</summary>
        public bool targetFriendly = true;
        public bool includeSelf = true;

        public CompProperties_MMTGiveHediffAoE()
        {
            compClass = typeof(CompAbilityEffect_MMTGiveHediffAoE);
        }
    }

    /// <summary>对目标点半径内的友/敌 pawn 施加（或刷新）Hediff。</summary>
    public class CompAbilityEffect_MMTGiveHediffAoE : CompAbilityEffect
    {
        public new CompProperties_MMTGiveHediffAoE Props => (CompProperties_MMTGiveHediffAoE)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            foreach (Pawn p in MMTAbilityUtil.PawnsInRadius(target.Cell, map, Props.radius, caster, Props.targetFriendly, Props.includeSelf))
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(p, Props.hediffDef);
            }
        }
    }

    // ========== 区域伤害（含多脉冲风暴类） ==========

    public class CompProperties_MMTAoEDamage : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public int damageAmount = 20;
        public float armorPen = 0f;
        public float radius = 3f;
        /// <summary>脉冲次数（1=一次性）。</summary>
        public int pulses = 1;
        public int pulseIntervalTicks = 60;
        /// <summary>true=每次脉冲在半径内随机选点（风暴类）。</summary>
        public bool randomStrikes;
        public bool friendlyFire;
        public float startFireChance;
        /// <summary>命中时给存活 pawn 附加的 Hediff（如冻结减速）。</summary>
        public HediffDef hitHediffDef;
        public string fleckDefName;

        public CompProperties_MMTAoEDamage()
        {
            compClass = typeof(CompAbilityEffect_MMTAoEDamage);
        }
    }

    public class CompAbilityEffect_MMTAoEDamage : CompAbilityEffect
    {
        public new CompProperties_MMTAoEDamage Props => (CompProperties_MMTAoEDamage)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.MapHeld;
            if (map == null)
            {
                return;
            }
            if (Props.pulses <= 1)
            {
                MMTAoEPulseZone.Pulse(target.Cell, map, parent.pawn, Props);
            }
            else
            {
                MMTAoEPulseZone.Spawn(target.Cell, map, parent.pawn, Props);
            }
        }
    }

    /// <summary>
    /// 多脉冲区域伤害的载体：临时 Thing，按间隔脉冲伤害，脉冲耗尽后自毁。
    /// 一次性技能不会生成它（直接 Pulse 静态调用）。
    /// </summary>
    public class MMTAoEPulseZone : Thing
    {
        public Pawn caster;
        public DamageDef damageDef;
        public int damageAmount;
        public float armorPen;
        public float radius;
        public int pulsesLeft;
        public int interval = 60;
        public bool randomStrikes;
        public bool friendlyFire;
        public float startFireChance;
        public HediffDef hitHediffDef;
        public string fleckDefName;
        private int nextPulseTick;

        public static void Spawn(IntVec3 center, Map map, Pawn caster, CompProperties_MMTAoEDamage props)
        {
            MMTAoEPulseZone zone = (MMTAoEPulseZone)ThingMaker.MakeThing(MMT_ClassDefOf.MMT_AoEPulseZone);
            zone.caster = caster;
            zone.damageDef = props.damageDef;
            zone.damageAmount = props.damageAmount;
            zone.armorPen = props.armorPen;
            zone.radius = props.radius;
            zone.pulsesLeft = props.pulses;
            zone.interval = props.pulseIntervalTicks;
            zone.randomStrikes = props.randomStrikes;
            zone.friendlyFire = props.friendlyFire;
            zone.startFireChance = props.startFireChance;
            zone.hitHediffDef = props.hitHediffDef;
            zone.fleckDefName = props.fleckDefName;
            GenSpawn.Spawn(zone, center, map);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            nextPulseTick = Find.TickManager.TicksGame;
        }

        protected override void Tick()
        {
            base.Tick();
            if (Map == null)
            {
                return;
            }
            if (Find.TickManager.TicksGame >= nextPulseTick)
            {
                IntVec3 cell = randomStrikes
                    ? GenRadial.RadialCellsAround(Position, radius, true).Where(c => c.InBounds(Map)).RandomElementWithFallback(Position)
                    : Position;
                PulseNow(cell);
                pulsesLeft--;
                nextPulseTick = Find.TickManager.TicksGame + interval;
                if (pulsesLeft <= 0)
                {
                    Destroy(DestroyMode.Vanish);
                }
            }
        }

        private void PulseNow(IntVec3 cell)
        {
            Pulse(cell, Map, caster, this);
        }

        /// <summary>单次脉冲：半径内 pawn 承伤（默认不伤施法者派系），可选点燃/附加 Hediff/飘点。</summary>
        public static void Pulse(IntVec3 cell, Map map, Pawn caster, CompProperties_MMTAoEDamage props)
        {
            PulseWorker(cell, map, caster, props.damageDef, props.damageAmount, props.armorPen, props.radius,
                props.friendlyFire, props.startFireChance, props.hitHediffDef, props.fleckDefName);
        }

        private static void Pulse(IntVec3 cell, Map map, Pawn caster, MMTAoEPulseZone z)
        {
            PulseWorker(cell, map, caster, z.damageDef, z.damageAmount, z.armorPen, z.radius,
                z.friendlyFire, z.startFireChance, z.hitHediffDef, z.fleckDefName);
        }

        private static void PulseWorker(IntVec3 cell, Map map, Pawn caster, DamageDef damageDef, int damageAmount,
            float armorPen, float radius, bool friendlyFire, float startFireChance, HediffDef hitHediffDef, string fleckDefName)
        {
            if (!fleckDefName.NullOrEmpty())
            {
                FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail(fleckDefName);
                if (fleck != null)
                {
                    FleckMaker.Static(cell.ToVector3Shifted(), map, fleck, 2f);
                }
            }
            foreach (Pawn p in GenRadial.RadialDistinctThingsAround(cell, map, radius, true).OfType<Pawn>().ToList())
            {
                if (!friendlyFire && caster != null && p.Faction == caster.Faction)
                {
                    continue;
                }
                p.TakeDamage(new DamageInfo(damageDef, damageAmount, armorPen, -1f, caster));
                if (hitHediffDef != null && !p.Dead)
                {
                    MMTAbilityUtil.ApplyOrRefreshHediff(p, hitHediffDef);
                }
            }
            if (startFireChance > 0f)
            {
                foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, radius, true))
                {
                    if (c.InBounds(map) && Rand.Chance(startFireChance))
                    {
                        FireUtility.TryStartFireIn(c, map, Rand.Range(0.2f, 0.6f), caster);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref caster, "caster");
            Scribe_Defs.Look(ref damageDef, "damageDef");
            Scribe_Defs.Look(ref hitHediffDef, "hitHediffDef");
            Scribe_Values.Look(ref damageAmount, "damageAmount");
            Scribe_Values.Look(ref armorPen, "armorPen");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref pulsesLeft, "pulsesLeft");
            Scribe_Values.Look(ref interval, "interval", 60);
            Scribe_Values.Look(ref randomStrikes, "randomStrikes");
            Scribe_Values.Look(ref friendlyFire, "friendlyFire");
            Scribe_Values.Look(ref startFireChance, "startFireChance");
            Scribe_Values.Look(ref fleckDefName, "fleckDefName");
            Scribe_Values.Look(ref nextPulseTick, "nextPulseTick");
        }
    }

    // ========== 治疗 ==========

    public class CompProperties_MMTHeal : CompProperties_AbilityEffect
    {
        /// <summary>每处伤治疗量。</summary>
        public float healAmount = 10f;
        public int maxInjuries = 3;
        /// <summary>0=单体目标，&gt;0=以目标点为中心的范围治疗。</summary>
        public float radius;
        public bool tendBleeding;
        public bool cureInfection;
        public bool cureScar;

        public CompProperties_MMTHeal()
        {
            compClass = typeof(CompAbilityEffect_MMTHeal);
        }
    }

    public class CompAbilityEffect_MMTHeal : CompAbilityEffect
    {
        public new CompProperties_MMTHeal Props => (CompProperties_MMTHeal)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            IEnumerable<Pawn> targets = Props.radius > 0f
                ? MMTAbilityUtil.PawnsInRadius(target.Cell, map, Props.radius, caster, true, true)
                : new[] { target.Pawn ?? caster };
            foreach (Pawn p in targets)
            {
                HealPawn(p);
            }
        }

        private void HealPawn(Pawn p)
        {
            if (p?.health?.hediffSet == null)
            {
                return;
            }
            int healed = 0;
            foreach (Hediff hediff in p.health.hediffSet.hediffs.ToList())
            {
                if (healed >= Props.maxInjuries)
                {
                    break;
                }
                if (Props.cureInfection && hediff.def.makesSickThought || hediff.def == HediffDefOf.WoundInfection)
                {
                    if (Props.cureInfection)
                    {
                        p.health.RemoveHediff(hediff);
                        healed++;
                        continue;
                    }
                }
                if (hediff is Hediff_Injury injury && !injury.IsPermanent())
                {
                    injury.Severity -= Props.healAmount;
                    if (Props.tendBleeding && injury.Bleeding && injury.Severity > 0f)
                    {
                        injury.Tended(0.9f, 0.9f, 0);
                    }
                    healed++;
                }
                else if (Props.cureScar && hediff is Hediff_Injury scar && scar.IsPermanent())
                {
                    scar.Severity -= Props.healAmount;
                    healed++;
                }
            }
            if (healed > 0 && p.MapHeld != null)
            {
                FleckMaker.ThrowMetaIcon(p.Position, p.MapHeld, FleckDefOf.HealingCross);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Pawn != null || Props.radius > 0f;
        }
    }

    // ========== 召唤（限时存在） ==========

    public class CompProperties_MMTSummon : CompProperties_AbilityEffect
    {
        public PawnKindDef pawnKindDef;
        public int count = 1;
        /// <summary>存在时限（tick），到期自动消失。</summary>
        public int durationTicks = 18000;
        /// <summary>同一施法者同时存在的该技能召唤物上限。</summary>
        public int maxActive = 6;
        /// <summary>true=以尸体为目标，把尸体点化为召唤物（死者苏生）。</summary>
        public bool fromCorpse;
        /// <summary>召唤建筑（魔法防御塔/图腾/地雷；设置后忽略 pawnKindDef）。</summary>
        public ThingDef buildingDef;

        public CompProperties_MMTSummon()
        {
            compClass = typeof(CompAbilityEffect_MMTSummon);
        }
    }

    public class CompAbilityEffect_MMTSummon : CompAbilityEffect
    {
        public new CompProperties_MMTSummon Props => (CompProperties_MMTSummon)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            if (Props.buildingDef != null)
            {
                if (target.Cell.Standable(map))
                {
                    Thing building = ThingMaker.MakeThing(Props.buildingDef);
                    building.SetFaction(caster.Faction);
                    GenSpawn.Spawn(building, target.Cell, map);
                    FleckMaker.Static(target.Cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
                }
                return;
            }
            if (Props.pawnKindDef == null)
            {
                return;
            }
            int active = MMTAbilityUtil.CountSummons(caster, map, Props.pawnKindDef);
            IntVec3 center = Props.fromCorpse && target.Thing is Corpse corpse ? corpse.Position : target.Cell;
            for (int i = 0; i < Props.count && active < Props.maxActive; i++, active++)
            {
                if (!CellFinder.TryFindRandomCellNear(center, map, 3,
                        c => c.Standable(map) && !c.Fogged(map), out IntVec3 cell))
                {
                    cell = center;
                }
                Pawn summon = PawnGenerator.GeneratePawn(Props.pawnKindDef, caster.Faction);
                GenSpawn.Spawn(summon, cell, map);
                MMTAbilityUtil.MarkSummon(summon, caster, Props.durationTicks);
                FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
            }
            if (Props.fromCorpse && target.Thing is Corpse c)
            {
                c.Destroy(DestroyMode.Vanish);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            if (!Props.fromCorpse)
            {
                return true;
            }
            return target.Thing is Corpse;
        }
    }

    /// <summary>召唤物限时 Hediff 参数。</summary>
    public class HediffCompProperties_MMTSummonTimer : HediffCompProperties
    {
        public HediffCompProperties_MMTSummonTimer()
        {
            compClass = typeof(HediffComp_MMTSummonTimer);
        }
    }

    /// <summary>召唤物限时存在：到期消失（不配 Dismiss 技，mmt-class-design.md §2.3）。</summary>
    public class HediffComp_MMTSummonTimer : HediffComp
    {
        public Pawn master;
        public int ticksLeft = 18000;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            ticksLeft -= delta;
            if (ticksLeft <= 0 && Pawn.Spawned && !Pawn.Dead)
            {
                FleckMaker.Static(Pawn.Position.ToVector3Shifted(), Pawn.Map, FleckDefOf.PsycastSkipInnerExit, 1f);
                Pawn.Destroy(DestroyMode.Vanish);
            }
        }

        public override string CompLabelInBracketsExtra => (ticksLeft / 60) + "s";

        public override void CompExposeData()
        {
            Scribe_References.Look(ref master, "master");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", 18000);
        }
    }

    // ========== 位移+伤害（突进/背刺） ==========

    public class CompProperties_MMTDashStrike : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public int damageAmount = 20;
        public float armorPen = 0f;
        /// <summary>落点伤害半径（0=仅目标本身）。</summary>
        public float radius;
        /// <summary>true=造成伤害后返回原位（暗影突袭）。</summary>
        public bool returnToStart;

        public CompProperties_MMTDashStrike()
        {
            compClass = typeof(CompAbilityEffect_MMTDashStrike);
        }
    }

    public class CompAbilityEffect_MMTDashStrike : CompAbilityEffect
    {
        public new CompProperties_MMTDashStrike Props => (CompProperties_MMTDashStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            IntVec3 origin = caster.Position;
            IntVec3 land = target.Cell;
            if (target.Pawn != null)
            {
                land = GenAdjFast.AdjacentCells8Way(target.Cell).Where(c => c.Standable(map))
                    .OrderBy(c => c.DistanceToSquared(origin)).FirstOrFallback(target.Cell);
            }
            if (!land.Standable(map))
            {
                land = CellFinder.TryFindRandomCellNear(target.Cell, map, 2, c => c.Standable(map), out IntVec3 c2) ? c2 : target.Cell;
            }
            FleckMaker.Static(origin.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
            caster.Position = land;
            FleckMaker.Static(land.ToVector3Shifted(), map, FleckDefOf.PsycastSkipInnerExit, 1f);
            foreach (Pawn p in GenRadial.RadialDistinctThingsAround(land, map, Mathf.Max(Props.radius, 0.9f), true).OfType<Pawn>().ToList())
            {
                if (p == caster || p.Faction == caster.Faction)
                {
                    continue;
                }
                if (Props.radius > 0f || p == target.Pawn || p.Position == land || p.Position.AdjacentTo8WayOrInside(land))
                {
                    p.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount, Props.armorPen, -1f, caster));
                }
            }
            if (Props.returnToStart && origin.IsValid && origin.Standable(map))
            {
                caster.Position = origin;
            }
        }
    }

    // ========== 心灵控制（范围） ==========

    public class CompProperties_MMTMentalStateAoE : CompProperties_AbilityEffect
    {
        public MentalStateDef mentalStateDef;
        public float radius = 5f;
        public bool targetFriendly;
        /// <summary>true=仅作用于动物（诱惑动物）。</summary>
        public bool animalsOnly;

        public CompProperties_MMTMentalStateAoE()
        {
            compClass = typeof(CompAbilityEffect_MMTMentalStateAoE);
        }
    }

    public class CompAbilityEffect_MMTMentalStateAoE : CompAbilityEffect
    {
        public new CompProperties_MMTMentalStateAoE Props => (CompProperties_MMTMentalStateAoE)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null || Props.mentalStateDef == null)
            {
                return;
            }
            foreach (Pawn p in GenRadial.RadialDistinctThingsAround(target.Cell, map, Props.radius, true).OfType<Pawn>().ToList())
            {
                if (p == caster || p.Dead || p.mindState == null)
                {
                    continue;
                }
                if (Props.animalsOnly && !p.RaceProps.Animal)
                {
                    continue;
                }
                if (!Props.animalsOnly)
                {
                    bool friendly = p.Faction == caster.Faction;
                    if (friendly != Props.targetFriendly)
                    {
                        continue;
                    }
                }
                p.mindState.mentalStateHandler.TryStartMentalState(Props.mentalStateDef, forced: true);
            }
        }
    }

    // ========== 生成（天气/事件条件/物品/灭火） ==========

    public class CompProperties_MMTSpawnEffect : CompProperties_AbilityEffect
    {
        /// <summary>生成 GameCondition（如丰饶雨、寒流）。</summary>
        public GameConditionDef gameConditionDef;
        public int conditionDurationTicks = 28800;
        /// <summary>切换天气（驱散阴云→Fair，唤雨→Rain）。</summary>
        public WeatherDef weatherDef;
        /// <summary>生成物品（医疗补给、医疗凝胶等）。</summary>
        public ThingDef spawnThingDef;
        public int spawnCount = 1;
        /// <summary>半径灭火（灭火罐/灭火）。</summary>
        public float extinguishRadius;
        /// <summary>缩短地图上既有 GameCondition 剩余时长（消散毒尘：ToxicFallout ×0.5）。</summary>
        public GameConditionDef shortenConditionDef;
        public float shortenFactor = 0.5f;

        public CompProperties_MMTSpawnEffect()
        {
            compClass = typeof(CompAbilityEffect_MMTSpawnEffect);
        }
    }

    public class CompAbilityEffect_MMTSpawnEffect : CompAbilityEffect
    {
        public new CompProperties_MMTSpawnEffect Props => (CompProperties_MMTSpawnEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.MapHeld;
            if (map == null)
            {
                return;
            }
            if (Props.weatherDef != null)
            {
                map.weatherManager.TransitionTo(Props.weatherDef);
            }
            if (Props.gameConditionDef != null)
            {
                GameCondition cond = GameConditionMaker.MakeCondition(Props.gameConditionDef, Props.conditionDurationTicks);
                map.gameConditionManager.RegisterCondition(cond);
            }
            if (Props.spawnThingDef != null)
            {
                for (int i = 0; i < Props.spawnCount; i++)
                {
                    Thing thing = ThingMaker.MakeThing(Props.spawnThingDef);
                    GenPlace.TryPlaceThing(thing, target.Cell, map, ThingPlaceMode.Near);
                }
            }
            if (Props.shortenConditionDef != null)
            {
                GameCondition active = map.gameConditionManager.GetActiveCondition(Props.shortenConditionDef);
                if (active != null)
                {
                    active.TicksLeft = (int)(active.TicksLeft * Props.shortenFactor);
                    Messages.Message("「" + parent.def.LabelCap + "」使 " + Props.shortenConditionDef.LabelCap + " 的剩余时间减半", parent.pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
            if (Props.extinguishRadius > 0f)
            {
                foreach (IntVec3 c in GenRadial.RadialCellsAround(target.Cell, Props.extinguishRadius, true))
                {
                    Fire fire = c.GetFirstThing<Fire>(map);
                    if (fire != null)
                    {
                        fire.Destroy();
                    }
                }
                FleckMaker.ThrowSmoke(target.Cell.ToVector3Shifted(), map, 2f);
            }
        }
    }

    // ========== 连射/散射 ==========

    public class CompProperties_MMTMultiShot : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public int shotCount = 5;
        /// <summary>散射半径（格）：每发在目标周边随机偏移。</summary>
        public float scatterRadius = 1.5f;

        public CompProperties_MMTMultiShot()
        {
            compClass = typeof(CompAbilityEffect_MMTMultiShot);
        }
    }

    public class CompAbilityEffect_MMTMultiShot : CompAbilityEffect
    {
        public new CompProperties_MMTMultiShot Props => (CompProperties_MMTMultiShot)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null || Props.projectileDef == null)
            {
                return;
            }
            for (int i = 0; i < Props.shotCount; i++)
            {
                IntVec3 aim = target.Cell;
                if (Props.scatterRadius > 0f)
                {
                    aim += new IntVec3(Mathf.RoundToInt(Rand.Range(-Props.scatterRadius, Props.scatterRadius)), 0,
                        Mathf.RoundToInt(Rand.Range(-Props.scatterRadius, Props.scatterRadius)));
                    aim = aim.ClampInsideMap(map);
                }
                Projectile proj = (Projectile)GenSpawn.Spawn(Props.projectileDef, caster.Position, map);
                proj.Launch(caster, target, new LocalTargetInfo(aim), ProjectileHitFlags.All, false, caster.equipment?.Primary);
            }
        }
    }

    // ========== 维持光环/开关技（mmt-class-design.md §2.2 熵压模型） ==========

    public class CompProperties_MMTToggleAura : CompProperties_AbilityEffect
    {
        /// <summary>驻留在施法者身上的光环 Hediff（携带 HediffComp_MMTAura 做脉冲）。</summary>
        public HediffDef auraHediffDef;

        public CompProperties_MMTToggleAura()
        {
            compClass = typeof(CompAbilityEffect_MMTToggleAura);
        }
    }

    /// <summary>开关型光环：开启给施法者驻留光环 Hediff，再次施放关闭。</summary>
    public class CompAbilityEffect_MMTToggleAura : CompAbilityEffect
    {
        public new CompProperties_MMTToggleAura Props => (CompProperties_MMTToggleAura)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Hediff existing = caster.health.hediffSet.GetFirstHediffOfDef(Props.auraHediffDef);
            if (existing != null)
            {
                caster.health.RemoveHediff(existing);
                Messages.Message(caster.LabelShort + " 关闭了「" + parent.def.LabelCap + "」", caster, MessageTypeDefOf.NeutralEvent);
            }
            else
            {
                caster.health.AddHediff(Props.auraHediffDef);
                Messages.Message(caster.LabelShort + " 开启了「" + parent.def.LabelCap + "」（维持会缓慢积累心灵熵，再次施放关闭）",
                    caster, MessageTypeDefOf.PositiveEvent);
            }
        }
    }

    /// <summary>光环 Hediff 参数（挂在光环 HediffDef 上）。</summary>
    public class HediffCompProperties_MMTAura : HediffCompProperties
    {
        /// <summary>脉冲施加给周围 pawn 的 Hediff（null=仅自身开关增益，无对外施加）。</summary>
        public HediffDef targetHediffDef;
        public float radius = 8f;
        public bool targetFriendly = true;
        /// <summary>每次脉冲给施法者积累的心灵熵（0=不耗）。</summary>
        public float entropyPerPulse = 0.3f;

        public HediffCompProperties_MMTAura()
        {
            compClass = typeof(HediffComp_MMTAura);
        }
    }

    /// <summary>
    /// 维持光环脉冲（全套系统唯一持续 tick 开销，mmt-class-design.md §6）：
    /// 按设置间隔（classAuraInterval，默认 300 tick）扫描半径内 pawn 施加/续期目标 Hediff，
    /// 每脉冲给施法者涨少量熵；到熵上限/倒地/死亡自动关闭。
    /// </summary>
    public class HediffComp_MMTAura : HediffComp
    {
        private int ticksToNextPulse;

        public new HediffCompProperties_MMTAura Props => (HediffCompProperties_MMTAura)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            ticksToNextPulse -= delta;
            if (ticksToNextPulse > 0)
            {
                return;
            }
            ticksToNextPulse = MMTSettings.ClassAuraInterval;
            Pawn pawn = Pawn;
            if (pawn.Map == null || pawn.Downed || pawn.Dead)
            {
                pawn.health.RemoveHediff(parent);
                return;
            }
            // 熵压：灵能框架下无独立"蓝耗维持"，用熵积累代替；逼近上限自动关闭
            if (Props.entropyPerPulse > 0f && pawn.psychicEntropy != null)
            {
                pawn.psychicEntropy.TryAddEntropy(Props.entropyPerPulse, pawn, true, true);
                if (pawn.psychicEntropy.EntropyValue >= pawn.psychicEntropy.MaxEntropy - 0.5f)
                {
                    pawn.health.RemoveHediff(parent);
                    Messages.Message(pawn.LabelShort + " 心灵熵逼近上限，光环「" + parent.def.LabelCap + "」自动关闭",
                        pawn, MessageTypeDefOf.NeutralEvent);
                    return;
                }
            }
            if (Props.targetHediffDef != null)
            {
                foreach (Pawn p in MMTAbilityUtil.PawnsInRadius(pawn.Position, pawn.Map, Props.radius, pawn, Props.targetFriendly, true))
                {
                    MMTAbilityUtil.ApplyOrRefreshHediff(p, Props.targetHediffDef);
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToNextPulse, "ticksToNextPulse");
        }
    }

    // ========== 复活（简化，仅刚死未腐） ==========

    public class CompProperties_MMTResurrectLite : CompProperties_AbilityEffect
    {
        /// <summary>复活后被复活者的虚弱 Hediff（默认原版复活后遗症）。</summary>
        public HediffDef revivedHediffDef;
        /// <summary>施法者的虚弱 Hediff（双方虚弱数日）。</summary>
        public HediffDef casterHediffDef;

        public CompProperties_MMTResurrectLite()
        {
            compClass = typeof(CompAbilityEffect_MMTResurrectLite);
        }
    }

    public class CompAbilityEffect_MMTResurrectLite : CompAbilityEffect
    {
        public new CompProperties_MMTResurrectLite Props => (CompProperties_MMTResurrectLite)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!(target.Thing is Corpse corpse))
            {
                return;
            }
            Pawn pawn = corpse.InnerPawn;
            ResurrectionUtility.TryResurrectWithSideEffects(pawn);
            HediffDef sickness = Props.revivedHediffDef ?? DefDatabase<HediffDef>.GetNamedSilentFail("ResurrectionSickness");
            if (sickness != null)
            {
                pawn.health.AddHediff(sickness);
            }
            if (Props.casterHediffDef != null)
            {
                parent.pawn.health.AddHediff(Props.casterHediffDef);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            if (!(target.Thing is Corpse corpse))
            {
                return false;
            }
            // 仅刚死未腐（Rotting 已腐不可复活）
            RotStage? stage = corpse.GetRotStage();
            return stage == null || stage == RotStage.Fresh;
        }
    }

    // ========== 个案：鲜血献祭（自伤降熵） / 冥想 / 灵能汲取 ==========

    public class CompProperties_MMTSelfEntropy : CompProperties_AbilityEffect
    {
        /// <summary>自伤量（0=不受伤）。</summary>
        public float selfDamage;
        /// <summary>熵变化（负=下降）。</summary>
        public float entropyChange = -10f;
        /// <summary>附带小治疗（冥想）。</summary>
        public float healAmount;
        /// <summary>给目标的虚弱 Hediff（灵能汲取）。</summary>
        public HediffDef targetHediffDef;

        public CompProperties_MMTSelfEntropy()
        {
            compClass = typeof(CompAbilityEffect_MMTSelfEntropy);
        }
    }

    /// <summary>鲜血献祭/冥想/灵能汲取共用：自身熵下降 + 可选自伤/治疗/目标虚弱。</summary>
    public class CompAbilityEffect_MMTSelfEntropy : CompAbilityEffect
    {
        public new CompProperties_MMTSelfEntropy Props => (CompProperties_MMTSelfEntropy)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            if (Props.selfDamage > 0f)
            {
                caster.TakeDamage(new DamageInfo(DamageDefOf.Cut, Props.selfDamage));
            }
            if (Props.healAmount > 0f)
            {
                Hediff_Injury worst = caster.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                    .Where(h => !h.IsPermanent()).OrderByDescending(h => h.Severity).FirstOrDefault();
                if (worst != null)
                {
                    worst.Severity -= Props.healAmount;
                }
            }
            if (caster.psychicEntropy != null && Props.entropyChange != 0f)
            {
                caster.psychicEntropy.TryAddEntropy(Props.entropyChange, caster, false, true);
            }
            if (Props.targetHediffDef != null && target.Pawn != null && target.Pawn != caster)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(target.Pawn, Props.targetHediffDef);
            }
        }
    }

    // ========== 个案：抚平心绪（移除 1 个负面记忆） ==========

    public class CompProperties_MMTFadeEmotions : CompProperties_AbilityEffect
    {
        /// <summary>安抚后的心情 buff Hediff。</summary>
        public HediffDef moodHediffDef;

        public CompProperties_MMTFadeEmotions()
        {
            compClass = typeof(CompAbilityEffect_MMTFadeEmotions);
        }
    }

    public class CompAbilityEffect_MMTFadeEmotions : CompAbilityEffect
    {
        public new CompProperties_MMTFadeEmotions Props => (CompProperties_MMTFadeEmotions)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn?.needs?.mood?.thoughts?.memories == null)
            {
                return;
            }
            Thought_Memory worst = pawn.needs.mood.thoughts.memories.Memories
                .Where(t => t.MoodOffset() < 0f).OrderBy(t => t.MoodOffset()).FirstOrDefault();
            if (worst != null)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemory(worst);
            }
            if (Props.moodHediffDef != null)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(pawn, Props.moodHediffDef);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Pawn != null && target.Pawn.needs?.mood != null;
        }
    }

    // ========== 个案：拉取目标（奥术"召唤"） ==========

    public class CompAbilityEffect_MMTPull : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            Thing thing = target.Thing;
            if (map == null || thing == null || thing == caster)
            {
                return;
            }
            if (!CellFinder.TryFindRandomCellNear(caster.Position, map, 2, c => c.Standable(map), out IntVec3 cell))
            {
                return;
            }
            FleckMaker.Static(thing.Position.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
            thing.DeSpawn();
            GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Direct);
            FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipInnerExit, 1f);
            if (thing is Pawn p)
            {
                p.jobs?.StopAll();
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Thing != null && target.Thing != parent.pawn;
        }
    }

    // ========== 个案：群体传送 ==========

    public class CompProperties_MMTGroupTeleport : CompProperties_AbilityEffect
    {
        /// <summary>施法者周围带走友军的半径。</summary>
        public float radius = 4f;

        public CompProperties_MMTGroupTeleport()
        {
            compClass = typeof(CompAbilityEffect_MMTGroupTeleport);
        }
    }

    public class CompAbilityEffect_MMTGroupTeleport : CompAbilityEffect
    {
        public new CompProperties_MMTGroupTeleport Props => (CompProperties_MMTGroupTeleport)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null || !target.Cell.Standable(map))
            {
                return;
            }
            List<Pawn> group = MMTAbilityUtil.PawnsInRadius(caster.Position, map, Props.radius, caster, true, true).ToList();
            foreach (Pawn p in group)
            {
                IntVec3 cell = p == caster ? target.Cell
                    : CellFinder.TryFindRandomCellNear(target.Cell, map, 4, c => c.Standable(map), out IntVec3 c2) ? c2 : target.Cell;
                FleckMaker.Static(p.Position.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
                p.Position = cell;
                p.jobs?.StopAll();
                FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.PsycastSkipInnerExit, 1f);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            Map map = parent.pawn.MapHeld;
            return map != null && target.Cell.IsValid && target.Cell.InBounds(map) && target.Cell.Standable(map);
        }
    }

    // ========== 个案：移形换位 ==========

    public class CompProperties_MMTTranspose : CompProperties_AbilityEffect
    {
        /// <summary>双方眩晕 Hediff。</summary>
        public HediffDef stunHediffDef;
        /// <summary>自身获得的反弹 Hediff。</summary>
        public HediffDef selfBuffHediffDef;

        public CompProperties_MMTTranspose()
        {
            compClass = typeof(CompAbilityEffect_MMTTranspose);
        }
    }

    public class CompAbilityEffect_MMTTranspose : CompAbilityEffect
    {
        public new CompProperties_MMTTranspose Props => (CompProperties_MMTTranspose)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Pawn other = target.Pawn;
            Map map = caster.MapHeld;
            if (other == null || map == null)
            {
                return;
            }
            IntVec3 a = caster.Position;
            IntVec3 b = other.Position;
            caster.Position = b;
            other.Position = a;
            caster.jobs?.StopAll();
            other.jobs?.StopAll();
            FleckMaker.Static(a.ToVector3Shifted(), map, FleckDefOf.PsycastSkipFlashEntry, 1f);
            FleckMaker.Static(b.ToVector3Shifted(), map, FleckDefOf.PsycastSkipInnerExit, 1f);
            if (Props.stunHediffDef != null)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(caster, Props.stunHediffDef);
                MMTAbilityUtil.ApplyOrRefreshHediff(other, Props.stunHediffDef);
            }
            if (Props.selfBuffHediffDef != null)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(caster, Props.selfBuffHediffDef);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Pawn != null && target.Pawn != parent.pawn;
        }
    }

    // ========== 个案：战斗药剂（随机正面药剂） ==========

    public class CompProperties_MMTCombatDrugs : CompProperties_AbilityEffect
    {
        public List<HediffDef> hediffPool = new List<HediffDef>();

        public CompProperties_MMTCombatDrugs()
        {
            compClass = typeof(CompAbilityEffect_MMTCombatDrugs);
        }
    }

    public class CompAbilityEffect_MMTCombatDrugs : CompAbilityEffect
    {
        public new CompProperties_MMTCombatDrugs Props => (CompProperties_MMTCombatDrugs)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn ?? parent.pawn;
            if (Props.hediffPool.Count == 0)
            {
                return;
            }
            MMTAbilityUtil.ApplyOrRefreshHediff(pawn, Props.hediffPool.RandomElement());
        }
    }

    // ========== 个案：尸爆术（尸体延时爆炸） ==========

    public class CompProperties_MMTCorpseExplosion : CompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public int damageAmount = 30;
        public float radius = 2.5f;
        public int delayTicks = 90;

        public CompProperties_MMTCorpseExplosion()
        {
            compClass = typeof(CompAbilityEffect_MMTCorpseExplosion);
        }
    }

    public class CompAbilityEffect_MMTCorpseExplosion : CompAbilityEffect
    {
        public new CompProperties_MMTCorpseExplosion Props => (CompProperties_MMTCorpseExplosion)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!(target.Thing is Corpse corpse))
            {
                return;
            }
            Map map = corpse.MapHeld;
            IntVec3 cell = corpse.Position;
            corpse.Destroy(DestroyMode.Vanish);
            MMTAoEPulseZone.Spawn(cell, map, parent.pawn, new CompProperties_MMTAoEDamage
            {
                damageDef = Props.damageDef,
                damageAmount = Props.damageAmount,
                radius = Props.radius,
                pulses = 1,
                pulseIntervalTicks = Props.delayTicks,
                friendlyFire = false,
            });
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Thing is Corpse;
        }
    }

    // ========== 个案：改写命运（死亡回溯，复用 StateBackup 快照先例） ==========

    public class CompProperties_MMTAlterFate : CompProperties_AbilityEffect
    {
        /// <summary>驻留的命运快照 Hediff（携带 HediffComp_MMTAlterFate）。</summary>
        public HediffDef fateHediffDef;

        public CompProperties_MMTAlterFate()
        {
            compClass = typeof(CompAbilityEffect_MMTAlterFate);
        }
    }

    public class CompAbilityEffect_MMTAlterFate : CompAbilityEffect
    {
        public new CompProperties_MMTAlterFate Props => (CompProperties_MMTAlterFate)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Hediff existing = caster.health.hediffSet.GetFirstHediffOfDef(Props.fateHediffDef);
            if (existing != null)
            {
                caster.health.RemoveHediff(existing);
            }
            Hediff hediff = caster.health.AddHediff(Props.fateHediffDef);
            if (hediff is HediffWithComps hwc)
            {
                hwc.TryGetComp<HediffComp_MMTAlterFate>()?.Snapshot();
            }
        }
    }

    public class HediffCompProperties_MMTAlterFate : HediffCompProperties
    {
        public int durationTicks = 36000;

        public HediffCompProperties_MMTAlterFate()
        {
            compClass = typeof(HediffComp_MMTAlterFate);
        }
    }

    /// <summary>改写命运：施法时快照健康状态，限时内致死则回溯（一次性消耗）。</summary>
    public class HediffComp_MMTAlterFate : HediffComp
    {
        public List<HediffSnapshot> snapshot = new List<HediffSnapshot>();
        public int ticksLeft;

        public new HediffCompProperties_MMTAlterFate Props => (HediffCompProperties_MMTAlterFate)props;

        public bool Ready => snapshot.Count > 0;

        public override void CompPostMake()
        {
            ticksLeft = Props.durationTicks;
        }

        public void Snapshot()
        {
            snapshot.Clear();
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs)
            {
                if (hediff.def == parent.def)
                {
                    continue;
                }
                snapshot.Add(HediffSnapshot.From(hediff));
            }
            ticksLeft = Props.durationTicks;
        }

        public void Restore()
        {
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs.ToList())
            {
                Pawn.health.RemoveHediff(hediff);
            }
            foreach (HediffSnapshot snap in snapshot)
            {
                Pawn.health.AddHediff(snap.RebuildOn(Pawn));
            }
            Pawn.health.forceDowned = false;
            snapshot.Clear();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (ticksLeft > 0)
            {
                ticksLeft -= delta;
                if (ticksLeft <= 0)
                {
                    Pawn.health.RemoveHediff(parent);
                }
            }
        }

        public override string CompLabelInBracketsExtra => (ticksLeft / 60) + "s";

        public override void CompExposeData()
        {
            Scribe_Collections.Look(ref snapshot, "snapshot", LookMode.Deep);
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
            if (snapshot == null)
            {
                snapshot = new List<HediffSnapshot>();
            }
        }
    }

    /// <summary>改写命运：致死拦截（优先级高于生命备份补丁，先消费命运快照）。</summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    [HarmonyPriority(Priority.High)]
    public static class Patch_AlterFateKill
    {
        public static bool Prefix(Pawn __instance)
        {
            Pawn pawn = __instance;
            if (pawn.Dead || pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
            {
                return true;
            }
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is HediffWithComps hwc)
                {
                    HediffComp_MMTAlterFate comp = hwc.TryGetComp<HediffComp_MMTAlterFate>();
                    if (comp != null && comp.Ready)
                    {
                        pawn.health.RemoveHediff(hediff);
                        comp.Restore();
                        Messages.Message(pawn.LabelShort + " 的命运被改写：致死伤害被回溯抵消！", pawn, MessageTypeDefOf.PositiveEvent);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // ========== 个案：枪托猛击（击晕/缴械） ==========

    public class CompProperties_MMTPistolWhip : CompProperties_AbilityEffect
    {
        public int damageAmount = 12;
        public HediffDef stunHediffDef;
        public bool disarm = true;

        public CompProperties_MMTPistolWhip()
        {
            compClass = typeof(CompAbilityEffect_MMTPistolWhip);
        }
    }

    public class CompAbilityEffect_MMTPistolWhip : CompAbilityEffect
    {
        public new CompProperties_MMTPistolWhip Props => (CompProperties_MMTPistolWhip)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return;
            }
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Props.damageAmount, 0f, -1f, parent.pawn));
            if (Props.stunHediffDef != null)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(pawn, Props.stunHediffDef);
            }
            if (Props.disarm && pawn.equipment?.Primary != null)
            {
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out ThingWithComps _, pawn.Position);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages)
        {
            return target.Pawn != null && target.Pawn != parent.pawn;
        }
    }

    /// <summary>丰饶之雨：生效期间周期性催熟种植区作物（气象法师；事件限时，开销有界）。</summary>
    public class GameCondition_MMTFertility : GameCondition
    {
        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksGame % 2500 != 0)
            {
                return;
            }
            foreach (Map map in AffectedMaps)
            {
                foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant))
                {
                    if (thing is Plant plant && plant.Position.GetZone(map) is Zone_Growing)
                    {
                        plant.Growth += 0.02f;
                    }
                }
            }
        }
    }

    // ========== 共用工具 ==========

    public static class MMTAbilityUtil
    {
        /// <summary>半径内按阵营筛选 pawn（targetFriendly=true 取同派系，false 取敌对）。</summary>
        public static IEnumerable<Pawn> PawnsInRadius(IntVec3 center, Map map, float radius, Pawn caster, bool targetFriendly, bool includeSelf)
        {
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(center, map, radius, true))
            {
                if (!(thing is Pawn p) || p.Dead || (!includeSelf && p == caster))
                {
                    continue;
                }
                if (targetFriendly)
                {
                    if (p.Faction == caster.Faction || (p.Faction == null && p.RaceProps.Animal && caster.Faction != null && p.HostFaction == null))
                    {
                        yield return p;
                    }
                }
                else if (p.Faction != null && p.Faction != caster.Faction && p.HostileTo(caster))
                {
                    yield return p;
                }
            }
        }

        /// <summary>施加 Hediff；已存在则续期（重置 Disappears 计时）并恢复初始严重度。</summary>
        public static Hediff ApplyOrRefreshHediff(Pawn pawn, HediffDef def)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (hediff == null)
            {
                hediff = pawn.health.AddHediff(def);
            }
            else
            {
                hediff.Severity = def.initialSeverity > 0f ? def.initialSeverity : hediff.Severity;
                if (hediff is HediffWithComps hwc)
                {
                    HediffComp_Disappears dis = hwc.TryGetComp<HediffComp_Disappears>();
                    if (dis != null)
                    {
                        dis.ticksToDisappear = dis.disappearsAfterTicks;
                    }
                }
            }
            return hediff;
        }

        /// <summary>给召唤物打限时标记（到期消失）。</summary>
        public static void MarkSummon(Pawn summon, Pawn master, int durationTicks)
        {
            Hediff hediff = summon.health.AddHediff(MMT_ClassDefOf.MMT_SummonTimer);
            if (hediff is HediffWithComps hwc)
            {
                HediffComp_MMTSummonTimer comp = hwc.TryGetComp<HediffComp_MMTSummonTimer>();
                if (comp != null)
                {
                    comp.master = master;
                    comp.ticksLeft = durationTicks;
                }
            }
        }

        /// <summary>统计施法者当前存活的某类召唤物数量（上限控制）。</summary>
        public static int CountSummons(Pawn caster, Map map, PawnKindDef kind)
        {
            int count = 0;
            foreach (Pawn p in map.mapPawns.AllPawnsSpawned)
            {
                if (p.kindDef != kind)
                {
                    continue;
                }
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(MMT_ClassDefOf.MMT_SummonTimer);
                if (hediff is HediffWithComps hwc && hwc.TryGetComp<HediffComp_MMTSummonTimer>() is HediffComp_MMTSummonTimer comp
                    && comp.master == caster)
                {
                    count++;
                }
            }
            return count;
        }
    }

    [DefOf]
    public static class MMT_ClassDefOf
    {
        /// <summary>多脉冲区域伤害的临时载体 Thing。</summary>
        public static ThingDef MMT_AoEPulseZone;
        /// <summary>召唤物限时存在 Hediff。</summary>
        public static HediffDef MMT_SummonTimer;

        static MMT_ClassDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ClassDefOf));
        }
    }
}
