using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 链式弹跳框架（第十三步：连锁闪电） ==========
    // 锁定首目标后，闪电依次跳向最近的其他敌方目标（pawn/建筑，每次在上一个命中点
    // 的弹跳半径内找最近的未命中敌人），每跳伤害按衰减系数递减；跳间有几 tick 间隔
    // 形成逐段蔓延的观感。视觉为自绘抖动折线电弧（光柱贴图细条 + 随机折点，亮蓝白）
    // + 每个节点原版 ElectricalSpark 火星 + 首跳雷声。
    // MMTChainLightningUtil.Fire 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTChainLightning；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="chainLightning"）。
    // 素材仅原版火星 fleck/雷声 + MMT 占位光柱贴图，不依赖 RoM。
    // 注：电弧参数运行时传入不进存档，读档后首个 tick 自毁（仅存活零点几秒）。

    /// <summary>连锁闪电参数包（MMTChainLightningUtil.Fire 的输入）。</summary>
    public class MMTChainParams
    {
        /// <summary>额外弹跳次数（0 = 只打首目标）。</summary>
        public int bounceCount = 4;
        /// <summary>每次弹跳的最大距离（格）。</summary>
        public float bounceRange = 5f;
        /// <summary>首跳伤害。</summary>
        public float damage = 15f;
        /// <summary>每跳伤害衰减系数（乘算）。</summary>
        public float damageFalloff = 0.75f;
        public float armorPenetration;
        /// <summary>伤害类型（null = Burn）。</summary>
        public DamageDef damageDef;
        /// <summary>电弧贴图（默认复用光柱贴图）与染色。</summary>
        public string arcTexPath = "Things/Mote/MMT_SkillBeam";
        public Color arcTint = new Color(0.6f, 0.8f, 1f);
        /// <summary>电弧宽度（格）。</summary>
        public float arcWidth = 0.16f;
        /// <summary>每跳间隔 tick（视觉蔓延节奏）。</summary>
        public int hopIntervalTicks = 2;
        /// <summary>最后一跳后电弧驻留 tick。</summary>
        public int arcLingerTicks = 10;
        /// <summary>节点火星 fleck（null = 原版 ElectricalSpark）。</summary>
        public FleckDef nodeFleckDef;
        /// <summary>首跳音效（null = 无）。</summary>
        public SoundDef sound;
        public Thing equipment;
    }

    /// <summary>连锁闪电统一入口：计算弹跳链并生成电弧实体逐跳结算。</summary>
    public static class MMTChainLightningUtil
    {
        public const string ArcDefName = "MMT_ChainArc";

        public static Thing_MMTChainArc Fire(Thing caster, Thing firstTarget, MMTChainParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || firstTarget == null || firstTarget.Destroyed || !firstTarget.Spawned)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(ArcDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + ArcDefName + " 定义，无法施放连锁闪电");
                return null;
            }
            List<Thing> chain = BuildChain(caster, firstTarget, p);
            Thing_MMTChainArc arc = (Thing_MMTChainArc)GenSpawn.Spawn(def, firstTarget.Position, map);
            arc.Setup(caster, chain, p);
            return arc;
        }

        /// <summary>弹跳链：首目标 + 每次在上一个命中点 bounceRange 内找最近的未命中敌人。</summary>
        private static List<Thing> BuildChain(Thing caster, Thing firstTarget, MMTChainParams p)
        {
            Map map = caster.MapHeld;
            List<Thing> chain = new List<Thing> { firstTarget };
            HashSet<Thing> hit = new HashSet<Thing> { firstTarget, caster };
            Thing last = firstTarget;
            for (int i = 0; i < p.bounceCount; i++)
            {
                Thing next = null;
                float best = float.MaxValue;
                foreach (Thing t in GenRadial.RadialDistinctThingsAround(last.Position, map, p.bounceRange, true))
                {
                    if (hit.Contains(t) || t.Destroyed || (!(t is Pawn) && !(t is Building)) || !t.HostileTo(caster))
                    {
                        continue;
                    }
                    float dist = (t.Position - last.Position).LengthHorizontalSquared;
                    if (dist < best)
                    {
                        best = dist;
                        next = t;
                    }
                }
                if (next == null)
                {
                    break;
                }
                chain.Add(next);
                hit.Add(next);
                last = next;
            }
            return chain;
        }
    }

    /// <summary>
    /// 连锁闪电电弧：按跳间节奏逐跳结算伤害（衰减）并点亮对应线段，
    /// 自绘抖动折线电弧（每段 5 折点随机抖动，每 3 tick 换一批折点）。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTChainArc : Thing
    {
        private const int JitterSubdivisions = 5;

        private Thing caster;
        private List<Thing> chain;
        private MMTChainParams arcParams;
        private int age = -1;
        private int currentHop;
        private Material mat;
        private bool loadedFromSave;

        public void Setup(Thing caster, List<Thing> chain, MMTChainParams p)
        {
            this.caster = caster;
            this.chain = chain;
            arcParams = p;
            age = 0;
            mat = MaterialPool.MatFrom(p.arcTexPath, ShaderDatabase.MoteGlow, p.arcTint);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || arcParams == null || chain == null || Map == null)
            {
                Destroy();
                return;
            }
            age++;
            int hopDue = currentHop * Mathf.Max(1, arcParams.hopIntervalTicks);
            if (currentHop < chain.Count && age >= hopDue)
            {
                DoHop(chain[currentHop], currentHop);
                currentHop++;
            }
            int lastHopTick = (chain.Count - 1) * Mathf.Max(1, arcParams.hopIntervalTicks);
            if (currentHop >= chain.Count && age >= lastHopTick + arcParams.arcLingerTicks)
            {
                Destroy();
            }
        }

        /// <summary>一跳：结算伤害（衰减）+ 节点火星 + 首跳音效。</summary>
        private void DoHop(Thing target, int hopIndex)
        {
            Map map = Map;
            if (target != null && !target.Destroyed && target.Spawned)
            {
                float damage = arcParams.damage * Mathf.Pow(arcParams.damageFalloff, hopIndex);
                target.TakeDamage(new DamageInfo(arcParams.damageDef ?? DamageDefOf.Burn, damage,
                    arcParams.armorPenetration, -1f, caster, null, arcParams.equipment?.def));
            }
            FleckDef fleck = arcParams.nodeFleckDef ?? DefDatabase<FleckDef>.GetNamedSilentFail("ElectricalSpark");
            if (fleck != null && map == Find.CurrentMap && target != null)
            {
                Vector3 pos = target.Position.ToVector3Shifted();
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                FleckCreationData data = FleckMaker.GetDataStatic(pos, map, fleck, Rand.Range(0.9f, 1.3f));
                data.instanceColor = arcParams.arcTint;
                map.flecks.CreateFleck(data);
            }
            if (hopIndex == 0 && arcParams.sound != null)
            {
                SoundStarter.PlayOneShot(arcParams.sound, new TargetInfo(Position, map));
            }
        }

        /// <summary>自绘电弧：施法者 → 首目标 → 各弹跳点的抖动折线（只画已点亮的段）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (mat == null || chain == null || Map != Find.CurrentMap || caster == null)
            {
                return;
            }
            Vector3 prev = caster.Position.ToVector3Shifted();
            prev.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            int fadeStart = (chain.Count - 1) * Mathf.Max(1, arcParams.hopIntervalTicks);
            float alpha = currentHop >= chain.Count
                ? Mathf.Clamp01((fadeStart + arcParams.arcLingerTicks - age) / (float)Mathf.Max(1, arcParams.arcLingerTicks))
                : 1f;
            for (int i = 0; i < chain.Count && i < currentHop; i++)
            {
                Thing node = chain[i];
                Vector3 next = (node != null && node.Spawned ? node.Position : chain[i].Position).ToVector3Shifted();
                next.y = prev.y;
                DrawSegment(prev, next, i, alpha);
                prev = next;
            }
        }

        /// <summary>一段闪电：折成 5 段随机抖动细条，外加一层宽暗光晕。</summary>
        private void DrawSegment(Vector3 from, Vector3 to, int segmentIndex, float alpha)
        {
            // 折点抖动按 (段号, 时间桶) 定种子，每 3 tick 换一批，闪烁但不乱跳
            Rand.PushState(Find.TickManager.TicksGame / 3 * 31 + segmentIndex * 97 + Position.GetHashCode());
            try
            {
                Vector3 prev = from;
                for (int s = 1; s <= JitterSubdivisions; s++)
                {
                    float t = s / (float)JitterSubdivisions;
                    Vector3 point = Vector3.Lerp(from, to, t);
                    if (s < JitterSubdivisions)
                    {
                        float jitter = 0.22f * (1f - Mathf.Abs(t - 0.5f) * 2f);
                        point += new Vector3(Rand.Range(-jitter, jitter), 0f, Rand.Range(-jitter, jitter));
                    }
                    DrawStrip(prev, point, arcParams.arcWidth * 2.6f, alpha * 0.25f);
                    DrawStrip(prev, point, arcParams.arcWidth, alpha);
                    prev = point;
                }
            }
            finally
            {
                Rand.PopState();
            }
        }

        private void DrawStrip(Vector3 from, Vector3 to, float width, float alpha)
        {
            Vector3 delta = to - from;
            float length = delta.magnitude;
            if (length < 0.01f)
            {
                return;
            }
            Color color = arcParams.arcTint;
            color.a = alpha;
            mat.color = color;
            Vector3 center = (from + to) * 0.5f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(center, Quaternion.AngleAxis(delta.AngleFlat(), Vector3.up), new Vector3(width, 1f, length)), mat, 0);
        }
    }

    // ========== pawn 技能出口：连锁闪电（单体锁定，自动弹跳） ==========

    public class CompProperties_MMTChainLightning : CompProperties_AbilityEffect
    {
        public int bounceCount = 4;
        public float bounceRange = 5f;
        public float damage = 15f;
        public float damageFalloff = 0.75f;
        public float armorPenetration;
        public string damageDefName;
        public string arcTexPath = "Things/Mote/MMT_SkillBeam";
        public Color arcTint = new Color(0.6f, 0.8f, 1f);
        public string soundDefName;

        public CompProperties_MMTChainLightning()
        {
            compClass = typeof(CompAbilityEffect_MMTChainLightning);
        }
    }

    /// <summary>连锁闪电技能：锁定首目标，闪电自动在敌方目标间弹跳（读条走 CastBar 框架）。</summary>
    public class CompAbilityEffect_MMTChainLightning : CompAbilityEffect
    {
        public new CompProperties_MMTChainLightning Props => (CompProperties_MMTChainLightning)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!target.HasThing)
            {
                return;
            }
            Pawn caster = parent.pawn;
            MMTChainLightningUtil.Fire(caster, target.Thing, new MMTChainParams
            {
                bounceCount = Props.bounceCount,
                bounceRange = Props.bounceRange,
                damage = Props.damage,
                damageFalloff = Props.damageFalloff,
                armorPenetration = Props.armorPenetration,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                arcTexPath = Props.arcTexPath,
                arcTint = Props.arcTint,
                sound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
