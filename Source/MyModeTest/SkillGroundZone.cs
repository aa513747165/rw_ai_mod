using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 持续地面区域框架（第十九步：通用版 + 毒气池演示） ==========
    // 在地面上维持一片圆形区域，站在里面的 pawn 按脉冲间隔持续吃效果
    // （伤害和/或 Hediff 严重度累积），区域到时自毁。这是"站里面持续吃效果"的
    // 通用底层：毒云（ToxicBuildup 累积）/ 减速地带 / 腐蚀酸池等都只是参数皮肤。
    // 与第六步地面火焰的区别：火焰是"每格一个实体"（真实燃烧可蔓延），
    // 区域是"单实体管整片圆"（N 格只 1 个 Thing、3 个 quad，实体数量不随面积放大）。
    // 视觉：三层贴图圆盘（共享材质、慢速反向旋转 + 呼吸透明度）模拟气雾翻卷，
    // 另有少量上升雾滴 fleck（节流）。贴图默认复用光环圆盘（白图，代码染色）。
    // MMTGroundZoneUtil.FireZone 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTGroundZone
    // （预览圈走第一步 AoETarget 基类）；建筑演示侧出口 demoType="groundZone"。
    // 注：区域参数运行时传入不进存档，读档后首个 tick 自毁（区域最长数十秒，可接受）。

    /// <summary>区域效果的敌我作用模式。</summary>
    public enum MMTZoneAffectMode
    {
        /// <summary>不分敌我（毒气等真实区域效果）。</summary>
        Everyone,
        /// <summary>仅敌对。</summary>
        HostileOnly,
        /// <summary>仅友方（含无派系野生动物，语义同瞄准框架）。</summary>
        FriendlyOnly
    }

    /// <summary>持续地面区域参数包（MMTGroundZoneUtil.FireZone 的输入）。</summary>
    public class MMTGroundZoneParams
    {
        /// <summary>区域半径（格）。</summary>
        public float radius = 4f;
        /// <summary>持续 tick 数。</summary>
        public int durationTicks = 1200;
        /// <summary>效果脉冲间隔 tick。</summary>
        public int intervalTicks = 30;
        /// <summary>敌我作用模式。</summary>
        public MMTZoneAffectMode affectMode = MMTZoneAffectMode.Everyone;
        /// <summary>每跳直接伤害（0 = 不造成伤害，仅 Hediff）。</summary>
        public float damage;
        /// <summary>伤害类型（null = ToxGas）。</summary>
        public DamageDef damageDef;
        public float armorPenetration;
        /// <summary>每跳累积的 Hediff（null = 不挂 Hediff；毒气池用原版 ToxicBuildup，离区后自然衰减）。</summary>
        public HediffDef hediffDef;
        /// <summary>每跳 Hediff 严重度增量。</summary>
        public float hediffSeverityPerPulse = 0.04f;
        /// <summary>圆盘贴图（白图，代码染色；默认复用光环圆盘）。</summary>
        public string texPath = "Things/Mote/MMT_AuraDisc";
        /// <summary>区域染色（默认毒气黄绿）。</summary>
        public Color tint = new Color(0.55f, 0.85f, 0.3f);
        /// <summary>雾滴 fleck（null = 不放雾滴）。</summary>
        public FleckDef moteFleckDef;
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>持续地面区域统一入口。</summary>
    public static class MMTGroundZoneUtil
    {
        public const string ZoneDefName = "MMT_GroundZone";

        /// <summary>以 center 为圆心展开持续地面区域，返回区域实体（参数无效返回 null）。</summary>
        public static Thing_MMTGroundZone FireZone(Thing caster, IntVec3 center, MMTGroundZoneParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || p.radius <= 0f || !center.IsValid || !center.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(ZoneDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + ZoneDefName + " 定义，无法展开持续地面区域");
                return null;
            }
            Thing_MMTGroundZone zone = (Thing_MMTGroundZone)GenSpawn.Spawn(def, center, map);
            zone.Setup(caster, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(center, map));
            }
            return zone;
        }
    }

    /// <summary>
    /// 持续地面区域实体：单实体管整片圆（面积不放大实体数），按脉冲间隔对圆内
    /// pawn 结算伤害/累积 Hediff；自绘三层反向旋转圆盘 + 节流雾滴 fleck。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTGroundZone : Thing
    {
        private Thing caster;
        private MMTGroundZoneParams zoneParams;
        private int durationTotal;
        private int ticksLeft = -1;
        private int ticksUntilPulse;
        private bool loadedFromSave;

        public void Setup(Thing caster, MMTGroundZoneParams p)
        {
            this.caster = caster;
            zoneParams = p;
            durationTotal = Mathf.Max(1, p.durationTicks);
            ticksLeft = durationTotal;
            ticksUntilPulse = 1;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || zoneParams == null || Map == null)
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
            if (--ticksUntilPulse <= 0)
            {
                ticksUntilPulse = Mathf.Max(1, zoneParams.intervalTicks);
                EffectPulse();
            }
            // 雾滴节流：每 8 tick 放一粒上升雾滴（仅当前地图）
            if (zoneParams.moteFleckDef != null && ticksLeft % 8 == 0 && Map == Find.CurrentMap)
            {
                SpawnMote();
            }
        }

        /// <summary>一次效果脉冲：圆内 pawn（按敌我模式过滤）结算伤害 + 累积 Hediff。</summary>
        private void EffectPulse()
        {
            Map map = Map;
            DamageDef damageDef = zoneParams.damageDef ?? DefDatabase<DamageDef>.GetNamedSilentFail("ToxGas");
            foreach (Pawn p in MMTAoETargetPreview.RawPawnsInRadius(map, Position, zoneParams.radius))
            {
                if (!Affects(p))
                {
                    continue;
                }
                if (zoneParams.damage > 0f && damageDef != null)
                {
                    p.TakeDamage(new DamageInfo(damageDef, zoneParams.damage, zoneParams.armorPenetration,
                        -1f, caster, null, zoneParams.equipment?.def));
                }
                if (zoneParams.hediffDef != null && zoneParams.hediffSeverityPerPulse > 0f)
                {
                    Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(zoneParams.hediffDef);
                    if (hediff == null)
                    {
                        hediff = HediffMaker.MakeHediff(zoneParams.hediffDef, p);
                        p.health.AddHediff(hediff);
                    }
                    hediff.Severity += zoneParams.hediffSeverityPerPulse;
                }
            }
        }

        private bool Affects(Pawn p)
        {
            switch (zoneParams.affectMode)
            {
                case MMTZoneAffectMode.Everyone:
                    return p != caster;
                case MMTZoneAffectMode.HostileOnly:
                    return caster == null || (p.Faction != null && p.Faction != caster.Faction && p.HostileTo(caster));
                default:
                    return p.Faction == caster?.Faction || (p.Faction == null && p.RaceProps.Animal && p.HostFaction == null);
            }
        }

        /// <summary>一粒上升雾滴：圆内随机位置，缓慢上飘。</summary>
        private void SpawnMote()
        {
            Map map = Map;
            Vector3 pos = Position.ToVector3Shifted() + new Vector3(Rand.Range(-zoneParams.radius * 0.8f, zoneParams.radius * 0.8f), 0f, Rand.Range(-zoneParams.radius * 0.8f, zoneParams.radius * 0.8f));
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, zoneParams.moteFleckDef, Rand.Range(0.7f, 1.2f));
            data.instanceColor = zoneParams.tint;
            data.velocity = new Vector3(0f, 0f, Rand.Range(0.15f, 0.3f));
            map.flecks.CreateFleck(data);
        }

        /// <summary>自绘气雾：三层嵌套程序圆盘 + 呼吸透明度（外淡内浓模拟气雾团）。
        /// 弃用贴图圆盘：MMT_AuraDisc 透明区 RGB 纯白，加法系着色器下整 quad 发光、alpha 失效；
        /// 程序网格 + ShaderDatabase.Transparent 的 alpha 确定生效。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (zoneParams == null || Map != Find.CurrentMap)
            {
                return;
            }
            int elapsed = durationTotal - ticksLeft;
            float fade = Mathf.Clamp01(elapsed / 20f) * Mathf.Clamp01(ticksLeft / 40f);
            Vector3 pos = Position.ToVector3Shifted();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            // 三层嵌套：外层大而淡、内层小而浓，错相位呼吸模拟气雾翻卷
            for (int layer = 0; layer < 3; layer++)
            {
                float r = zoneParams.radius * (1f - layer * 0.18f);
                float layerBreath = 0.8f + 0.2f * Mathf.Sin(elapsed * 0.07f + layer * 1.9f);
                Color color = zoneParams.tint;
                color.a = fade * layerBreath * (0.16f - layer * 0.035f);
                MMTAoETargetPreview.DrawDisc(pos, r, color);
            }
        }
    }

    // ========== pawn 技能出口：持续地面区域 ==========

    public class CompProperties_MMTGroundZone : CompProperties_MMTAoETargetBase
    {
        public int durationTicks = 1200;
        public int intervalTicks = 30;
        public MMTZoneAffectMode affectMode = MMTZoneAffectMode.Everyone;
        public float damage;
        public string damageDefName;
        public float armorPenetration;
        public string hediffDefName;
        public float hediffSeverityPerPulse = 0.04f;
        public string texPath = "Things/Mote/MMT_AuraDisc";
        public Color tint = new Color(0.55f, 0.85f, 0.3f);
        public string moteFleckDefName;
        public string soundDefName;

        public CompProperties_MMTGroundZone()
        {
            compClass = typeof(CompAbilityEffect_MMTGroundZone);
        }
    }

    /// <summary>持续地面区域技能：确认目标点后展开区域（radius 即区域半径，预览圈走 AoETarget 基类）。</summary>
    public class CompAbilityEffect_MMTGroundZone : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTGroundZone Props => (CompProperties_MMTGroundZone)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTGroundZoneUtil.FireZone(caster, target.Cell, new MMTGroundZoneParams
            {
                radius = Props.radius,
                durationTicks = Props.durationTicks,
                intervalTicks = Props.intervalTicks,
                affectMode = Props.affectMode,
                damage = Props.damage,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                armorPenetration = Props.armorPenetration,
                hediffDef = Props.hediffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(Props.hediffDefName),
                hediffSeverityPerPulse = Props.hediffSeverityPerPulse,
                texPath = Props.texPath,
                tint = Props.tint,
                moteFleckDef = Props.moteFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.moteFleckDefName),
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
