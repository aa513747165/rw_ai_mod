using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 延时打击框架（第十一步：预警圈 → 延迟 → 陨石天降） ==========
    // 确认落点后，目标区域先出现持续预警圈（Thing_MMTDelayedStrike 自绘脉冲圆环，
    // 炼狱魔王火炮式"先标记后落弹"节奏），延迟结束后从高空坠下一颗陨石
    // （Thing_MMTMeteor：斜向高速坠落 + 火焰拖尾 + 迫近呼啸），撞击点触发
    // 原版爆炸（画面/音效全走 GenExplosion，可点燃）。
    // MMTDelayedStrikeUtil.FireDelayedStrike 为统一入口；pawn 技能侧出口
    // CompAbilityEffect_MMTDelayedStrike（预警圈预览复用第一步 AoETarget 框架）；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="delayedStrike"）。
    // 素材仅原版音效 + MMT 占位贴图，不依赖 RoM。
    // 注：调度器/陨石参数运行时传入不进存档，读档后首个 tick 自毁（最长数秒，可接受）。

    /// <summary>延时打击参数包（MMTDelayedStrikeUtil.FireDelayedStrike 的输入）。</summary>
    public class MMTDelayedStrikeParams
    {
        /// <summary>预警圈半径（格，= 爆炸覆盖的可视提示半径）。</summary>
        public float warningRadius = 3f;
        /// <summary>预警持续 tick（之后陨石开始坠落）。</summary>
        public int delayTicks = 120;
        /// <summary>预警圈贴图（白图，代码染色；默认复用第一步的目标圈）。</summary>
        public string warningTexPath = "Things/Mote/MMT_TargetCircle";
        public Color warningTint = new Color(1f, 0.35f, 0.2f);
        /// <summary>陨石贴图（白图，代码染色）与染色。</summary>
        public string meteorTexPath = "Things/Mote/MMT_Meteor";
        public Color meteorTint = new Color(1f, 0.7f, 0.35f);
        /// <summary>陨石绘制尺寸（格）。</summary>
        public float meteorDrawSize = 1.6f;
        /// <summary>陨石坠落耗时 tick。</summary>
        public int fallTicks = 25;
        /// <summary>坠落起点相对撞击点的水平偏移（格，斜向砸落）。</summary>
        public float fallDistance = 14f;
        /// <summary>坠落起点的视觉高度（格）。</summary>
        public float fallHeight = 8f;
        /// <summary>撞击伤害类型（null = Bomb）。</summary>
        public DamageDef damageDef;
        public float damage = 40f;
        public float armorPenetration;
        /// <summary>撞击爆炸半径。</summary>
        public float explosionRadius = 3f;
        /// <summary>撞击点燃概率。</summary>
        public float chanceToStartFire = 0.3f;
        /// <summary>陨石开始坠落时的迫近呼啸音效（null = MortarRound_PreImpact）。</summary>
        public SoundDef anticipateSound;
        public Thing equipment;
    }

    /// <summary>延时打击统一入口：生成预警调度器，延迟后坠陨石。</summary>
    public static class MMTDelayedStrikeUtil
    {
        public const string SchedulerDefName = "MMT_DelayedStrike";
        public const string MeteorDefName = "MMT_Meteor";

        public static Thing_MMTDelayedStrike FireDelayedStrike(Thing caster, IntVec3 cell, MMTDelayedStrikeParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !cell.IsValid || !cell.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(SchedulerDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + SchedulerDefName + " 定义，无法发动延时打击");
                return null;
            }
            Thing_MMTDelayedStrike strike = (Thing_MMTDelayedStrike)GenSpawn.Spawn(def, cell, map);
            strike.Setup(caster, p);
            return strike;
        }

        /// <summary>预警圈绘制：复用目标圈贴图材质，按秒脉冲透明度。</summary>
        public static void DrawWarningCircle(IntVec3 center, float radius, Color tint, int ageTicks)
        {
            if (radius <= 0f)
            {
                return;
            }
            Texture2D tex = ContentFinder<Texture2D>.Get("Things/Mote/MMT_TargetCircle", false);
            if (tex == null)
            {
                GenDraw.DrawRadiusRing(center, radius, tint);
                return;
            }
            float pulse = 0.75f + 0.25f * Mathf.Sin(ageTicks * 0.2f);
            Color color = tint;
            color.a *= pulse;
            Material mat = MaterialPool.MatFrom("Things/Mote/MMT_TargetCircle", ShaderDatabase.TransparentPostLight, color);
            Vector3 pos = center.ToVector3Shifted();
            pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            float size = radius * 2f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)), mat, 0);
        }
    }

    /// <summary>
    /// 延时打击调度器：预警期间自绘脉冲预警圈，延迟结束生成陨石后自毁。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTDelayedStrike : Thing
    {
        private Thing caster;
        private MMTDelayedStrikeParams strikeParams;
        private int ticksLeft = -1;
        private int age;
        private bool loadedFromSave;

        public void Setup(Thing caster, MMTDelayedStrikeParams p)
        {
            this.caster = caster;
            strikeParams = p;
            ticksLeft = Mathf.Max(1, p.delayTicks);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || strikeParams == null || Map == null)
            {
                Destroy();
                return;
            }
            age++;
            if (--ticksLeft > 0)
            {
                return;
            }
            SpawnMeteor();
            Destroy();
        }

        /// <summary>延迟结束：从高空斜向坠下陨石。</summary>
        private void SpawnMeteor()
        {
            Map map = Map;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(MMTDelayedStrikeUtil.MeteorDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + MMTDelayedStrikeUtil.MeteorDefName + " 定义，无法召唤陨石");
                return;
            }
            Thing_MMTMeteor meteor = (Thing_MMTMeteor)GenSpawn.Spawn(def, Position, map);
            meteor.Setup(caster, strikeParams);
            SoundDef anticipate = strikeParams.anticipateSound
                ?? DefDatabase<SoundDef>.GetNamedSilentFail("MortarRound_PreImpact");
            if (anticipate != null)
            {
                SoundStarter.PlayOneShot(anticipate, new TargetInfo(Position, map));
            }
        }

        /// <summary>预警圈：随时间脉冲的红色圆环（与最终爆炸半径一致）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (strikeParams == null || Map != Find.CurrentMap)
            {
                return;
            }
            MMTDelayedStrikeUtil.DrawWarningCircle(Position, strikeParams.warningRadius, strikeParams.warningTint, age);
        }
    }

    /// <summary>
    /// 陨石：从高空斜向坠向撞击点（位置插值 + 高度衰减 + 火焰拖尾），
    /// 落地触发原版爆炸后自毁。参数运行时传入不进存档；读档后立即引爆兜底。
    /// </summary>
    public class Thing_MMTMeteor : Thing
    {
        private Thing caster;
        private MMTDelayedStrikeParams meteorParams;
        private Vector3 startGround;
        private float startHeight = 8f;
        private int ticksTotal = 1;
        private int age;
        private Material mat;
        private bool loadedFromSave;

        public void Setup(Thing caster, MMTDelayedStrikeParams p)
        {
            this.caster = caster;
            meteorParams = p;
            ticksTotal = Mathf.Max(1, p.fallTicks);
            startHeight = p.fallHeight;
            // 坠落起点：撞击点西北高空（固定方向斜落，观感和迫击炮一致）
            Vector3 impact = Position.ToVector3Shifted();
            Vector3 offset = new Vector3(-0.45f, 0f, 1f).normalized * p.fallDistance;
            startGround = impact + offset;
            mat = MaterialPool.MatFrom(p.meteorTexPath, ShaderDatabase.MoteGlow, p.meteorTint);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        private Vector3 CurPos()
        {
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            Vector3 pos = Vector3.Lerp(startGround, Position.ToVector3Shifted(), t);
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor() + startHeight * (1f - t);
            return pos;
        }

        protected override void Tick()
        {
            base.Tick();
            if (meteorParams == null || Map == null)
            {
                Destroy();
                return;
            }
            if (loadedFromSave)
            {
                // 读档兜底：参数已丢失，直接引爆避免悬空哑弹
                Impact();
                return;
            }
            age++;
            if (Map == Find.CurrentMap)
            {
                SpawnTrail();
            }
            if (age >= ticksTotal)
            {
                Impact();
            }
        }

        /// <summary>火焰拖尾：沿坠落轨迹抛火星。</summary>
        private void SpawnTrail()
        {
            Map map = Map;
            FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark");
            if (fleck == null)
            {
                return;
            }
            Vector3 pos = CurPos();
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, fleck, Rand.Range(0.5f, 0.9f));
            data.velocity = new Vector3(Rand.Range(-0.4f, 0.4f), 0f, Rand.Range(-0.4f, 0.4f));
            data.instanceColor = new Color(1f, Rand.Range(0.4f, 0.65f), 0.15f);
            map.flecks.CreateFleck(data);
        }

        /// <summary>撞击：原版爆炸（画面/音效全套），可点燃。</summary>
        private void Impact()
        {
            Map map = Map;
            if (map != null)
            {
                GenExplosion.DoExplosion(Position, map, meteorParams.explosionRadius,
                    meteorParams.damageDef ?? DamageDefOf.Bomb, caster,
                    damAmount: Mathf.RoundToInt(meteorParams.damage),
                    armorPenetration: meteorParams.armorPenetration,
                    weapon: meteorParams.equipment?.def,
                    chanceToStartFire: meteorParams.chanceToStartFire);
            }
            Destroy();
        }

        /// <summary>自绘陨石：拖尾焰球，接近地面时逐渐放大。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (mat == null || Map != Find.CurrentMap)
            {
                return;
            }
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            float size = meteorParams.meteorDrawSize * Mathf.Lerp(0.7f, 1.15f, t);
            Vector3 pos = CurPos();
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.AngleAxis(age * 9f, Vector3.up), new Vector3(size, 1f, size)), mat, 0);
        }
    }

    // ========== pawn 技能出口：延时打击 ==========

    public class CompProperties_MMTDelayedStrike : CompProperties_MMTAoETargetBase
    {
        public float warningRadius = 3f;
        public float delaySeconds = 2f;
        public int fallTicks = 25;
        public string damageDefName;
        public float damage = 40f;
        public float armorPenetration;
        public float explosionRadius = 3f;
        public float chanceToStartFire = 0.3f;
        public string meteorTexPath = "Things/Mote/MMT_Meteor";
        public Color meteorTint = new Color(1f, 0.7f, 0.35f);
        public string anticipateSoundDefName;

        public CompProperties_MMTDelayedStrike()
        {
            compClass = typeof(CompAbilityEffect_MMTDelayedStrike);
            radius = 3f;
        }
    }

    /// <summary>延时打击技能：确认落点后先亮预警圈，延迟结束坠下陨石轰击（radius = 预警圈预览）。</summary>
    public class CompAbilityEffect_MMTDelayedStrike : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTDelayedStrike Props => (CompProperties_MMTDelayedStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTDelayedStrikeUtil.FireDelayedStrike(caster, target.Cell, new MMTDelayedStrikeParams
            {
                warningRadius = Props.warningRadius > 0f ? Props.warningRadius : Props.radius,
                delayTicks = Mathf.Max(1, Props.delaySeconds.SecondsToTicks()),
                fallTicks = Props.fallTicks,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                armorPenetration = Props.armorPenetration,
                explosionRadius = Props.explosionRadius,
                chanceToStartFire = Props.chanceToStartFire,
                meteorTexPath = Props.meteorTexPath,
                meteorTint = Props.meteorTint,
                anticipateSound = Props.anticipateSoundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.anticipateSoundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
