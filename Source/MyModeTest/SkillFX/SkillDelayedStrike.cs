using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 延时打击框架（第十一步：预警圈 → 延迟 → 陨石天降） ==========
    // 确认落点后，目标区域先出现持续预警圈（Thing_MMTDelayedStrike 自绘脉冲圆环，
    // 炼狱魔王火炮式"先标记后落弹"节奏），延迟结束后从高空坠下一颗陨石
    // （Thing_MMTMeteor：斜向高速坠落 + 视觉高度北偏 + 落点阴影 + 火焰拖尾 + 迫近呼啸），撞击点触发
    // 原版爆炸（画面/音效全走 GenExplosion，可点燃）。
    // 皮肤分支：配了 pillarRamp（MMTSkillSideRampParams）则坠落物换成多片坡道光柱
    // （第二十七步坡道框架：侧视光柱切 N 带沿 +Z 坡道，顶部朝屏幕北=朝天，
    // 负漂移=向南压落）——牧师·圣光审判走这条（docs/mmt-class-priest.md §3.12）；
    // 坡道实体只承担视觉，坠落计时与撞击爆炸仍由陨石实体负责。
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
        /// <summary>陨石自旋速度（度/tick，0 = 坠落不旋转）。</summary>
        public float meteorSpinDegPerTick;
        /// <summary>坠落途中缩放区间：从 growFrom 线性放大到 growTo（1→1 = 恒定大小）。</summary>
        public float meteorGrowFrom = 0.7f;
        public float meteorGrowTo = 1.15f;
        /// <summary>陨石坠落耗时 tick。</summary>
        public int fallTicks = 25;
        /// <summary>坠落起点相对撞击点的水平偏移（格，斜向砸落）。</summary>
        public float fallDistance = 14f;
        /// <summary>坠落起点的视觉高度（格）。正交俯视下世界 Y 不可见，实现为向屏幕北（+Z）的
        /// 偏移量：陨石从目标点以北 fallHeight 格"高空"压落，配合落点阴影读出天降感。</summary>
        public float fallHeight = 8f;
        /// <summary>落点阴影最大 alpha（0=不画阴影；阴影随陨石逼近加深放大，是俯视下"有东西砸下来"的关键提示）。</summary>
        public float meteorShadowAlpha = 0.35f;
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
        /// <summary>坡道光柱皮肤（null = 默认陨石视觉）。配了则在坠落起点生成多片坡道光柱
        /// （durationSeconds 会被强制对齐 fallTicks），陨石实体只保留坠落计时与撞击爆炸，
        /// 不再自绘也不喷火焰拖尾。</summary>
        public MMTSkillSideRampParams pillarRamp;
        /// <summary>撞击帧序列皮肤（null = 无）。配了则坠落物视觉同样隐去（不再自绘/拖尾，
        /// 可与 pillarRamp 叠加：天上光柱 + 落地爆发），撞击瞬间在落点平铺一段侧视平躺序列帧
        /// （第二十六步 sideFlat 路线，如圣光审判II 的金色爆发冲击环），时长/尺寸由参数自带，
        /// 不对齐 fallTicks。爆炸伤害/音效仍走 GenExplosion。</summary>
        public MMTSkillSideFlatParams impactSideFlat;
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
    /// 坡道光柱皮肤模式（pillarMode）：坠落视觉由坡道光柱实体承担（在撞击点生成，
    /// 顶部朝屏幕北=朝天，负漂移向南压落），本实体只保留坠落计时与撞击爆炸。
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
        private bool pillarMode;
        private bool impactFxMode;
        private bool loadedFromSave;

        public void Setup(Thing caster, MMTDelayedStrikeParams p)
        {
            this.caster = caster;
            meteorParams = p;
            ticksTotal = Mathf.Max(1, p.fallTicks);
            // 撞击帧序列皮肤：坠落物视觉隐去（同 pillarMode），落地爆发由 Impact 时生成
            impactFxMode = p.impactSideFlat != null;
            if (p.pillarRamp != null)
            {
                // 坡道光柱皮肤：光柱寿命强制对齐坠落耗时（落地即爆，爆炸闪光盖住收尾淡出）
                pillarMode = true;
                p.pillarRamp.durationSeconds = ticksTotal / 60f;
                MMTSkillSideRampUtil.Fire(Map, Position, p.pillarRamp);
                return;
            }
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

        /// <summary>地面轨迹位置（不含视觉高度）：坠落起点（西北）→ 撞击点线性插值。</summary>
        private Vector3 GroundPos()
        {
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            Vector3 pos = Vector3.Lerp(startGround, Position.ToVector3Shifted(), t);
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            return pos;
        }

        /// <summary>视觉位置：地面轨迹 + 向屏幕北（+Z）偏移剩余高度——1.7 正交俯视下
        /// 世界 Y 不改变屏幕位置（渲染硬约束），"高度"只能画成向北偏移，
        /// 陨石随坠落从目标以北高空压向落点（同坡道方案"高度堆在 Z 轴"的思路）。</summary>
        private Vector3 VisualPos()
        {
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            Vector3 pos = GroundPos();
            pos.z += startHeight * (1f - t);
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
            if (Map == Find.CurrentMap && !pillarMode && !impactFxMode)
            {
                SpawnTrail();
            }
            if (age >= ticksTotal)
            {
                Impact();
            }
        }

        /// <summary>火焰拖尾：沿视觉位置（含高度偏移）抛火星，使尾迹跟着"天上那颗"走。</summary>
        private void SpawnTrail()
        {
            Map map = Map;
            FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark");
            if (fleck == null)
            {
                return;
            }
            Vector3 pos = VisualPos();
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, fleck, Rand.Range(0.5f, 0.9f));
            data.velocity = new Vector3(Rand.Range(-0.4f, 0.4f), 0f, Rand.Range(-0.4f, 0.4f));
            data.instanceColor = new Color(1f, Rand.Range(0.4f, 0.65f), 0.15f);
            map.flecks.CreateFleck(data);
        }

        /// <summary>撞击：原版爆炸（画面/音效全套），可点燃；配了撞击帧序列皮肤则
        /// 同 tick 在落点平铺落地爆发序列帧（视觉盖在爆炸闪光上，闪光作高光打底）。</summary>
        private void Impact()
        {
            Map map = Map;
            if (map != null)
            {
                if (meteorParams.impactSideFlat != null)
                {
                    MMTSkillSideFlatUtil.Fire(map, Position, meteorParams.impactSideFlat);
                }
                GenExplosion.DoExplosion(Position, map, meteorParams.explosionRadius,
                    meteorParams.damageDef ?? DamageDefOf.Bomb, caster,
                    damAmount: Mathf.RoundToInt(meteorParams.damage),
                    armorPenetration: meteorParams.armorPenetration,
                    weapon: meteorParams.equipment?.def,
                    chanceToStartFire: meteorParams.chanceToStartFire);
            }
            Destroy();
        }

        /// <summary>自绘陨石：先在落点画随逼近加深放大的阴影，再把陨石画在视觉位置
        /// （北偏高度），尺寸/缩放/自旋全部由参数控制（0 自旋 = 坠落不旋转）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (pillarMode || impactFxMode || mat == null || Map != Find.CurrentMap)
            {
                return;
            }
            float t = Mathf.Clamp01(age / (float)ticksTotal);
            float size = meteorParams.meteorDrawSize * Mathf.Lerp(meteorParams.meteorGrowFrom, meteorParams.meteorGrowTo, t);
            if (meteorParams.meteorShadowAlpha > 0f)
            {
                float shadowAlpha = Mathf.Lerp(0.08f, meteorParams.meteorShadowAlpha, t);
                float shadowRadius = size * 0.45f * Mathf.Lerp(0.5f, 1f, t);
                MMTAoETargetPreview.DrawDiscAtAltitude(Position.ToVector3Shifted(), shadowRadius,
                    new Color(0f, 0f, 0f, shadowAlpha), AltitudeLayer.MoteLow.AltitudeFor());
            }
            Vector3 pos = VisualPos();
            Quaternion rot = meteorParams.meteorSpinDegPerTick != 0f
                ? Quaternion.AngleAxis(age * meteorParams.meteorSpinDegPerTick, Vector3.up)
                : Quaternion.identity;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, rot, new Vector3(size, 1f, size)), mat, 0);
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
        /// <summary>陨石绘制尺寸（格，建议 ≈ 爆炸半径 × 1.2~1.5）。</summary>
        public float meteorDrawSize = 4f;
        /// <summary>陨石自旋速度（度/tick，0 = 坠落不旋转）。</summary>
        public float meteorSpinDegPerTick;
        /// <summary>坠落缩放区间（1→1 = 恒定大小）。</summary>
        public float meteorGrowFrom = 1f;
        public float meteorGrowTo = 1f;
        public string anticipateSoundDefName;

        // —— 坡道光柱皮肤（配了 pillarRampTexPath 即替代陨石视觉，其余 meteor* 视觉字段失效） ——
        /// <summary>光柱序列帧图集贴图（横向等宽排布，侧视构图：底部贴地、顶部朝北=朝天）。</summary>
        public string pillarRampTexPath;
        /// <summary>图集帧数（等宽切片，单帧贴图填 1）。</summary>
        public int pillarRampFrameCount = 1;
        /// <summary>每帧停留 tick 数。</summary>
        public int pillarRampFrameTicks = 7;
        /// <summary>坡道切片数（≥2，建议 ≤12）。</summary>
        public int pillarRampSlices = 8;
        /// <summary>光柱宽/高（格；宽=世界 X，高=世界 +Z 北=屏幕上；底边中点锚在落点）。</summary>
        public float pillarRampWidthCells = 3f;
        public float pillarRampHeightCells = 4.8f;
        /// <summary>坠落期间整体漂移（格，负值=向南压落，模拟从天而降的砸落感）。</summary>
        public float pillarRampDriftCells;
        /// <summary>横向摆动最大幅度（格，圣光类给小值保持庄重）。</summary>
        public float pillarRampSway = 0.06f;
        /// <summary>摆动周期 tick。</summary>
        public float pillarRampSwayPeriodTicks = 50f;
        /// <summary>顶端宽度缩放系数（&lt;1 收口，1=直筒，&gt;1 顶端微扩=光源在上）。</summary>
        public float pillarRampTopScale = 1f;
        /// <summary>顶端 alpha 系数（&lt;1 顶部淡出=光柱上端延伸出屏幕）。</summary>
        public float pillarRampTopAlpha = 0.5f;
        /// <summary>呼吸脉冲幅度（0~1，0=关闭）。</summary>
        public float pillarRampPulseAmp = 0.08f;
        /// <summary>染色（白图光柱代码染色用；alpha=0 视为纯白）。</summary>
        public Color pillarRampTint = Color.white;
        /// <summary>首/尾淡入淡出秒数（尾部淡出会被落地爆炸闪光盖住，给小值即可）。</summary>
        public float pillarRampFadeInSeconds = 0.08f;
        public float pillarRampFadeOutSeconds = 0.15f;

        // —— 撞击帧序列皮肤（配了 impactBurstTexPath 即隐去坠落物视觉，落地瞬间在落点平铺爆发序列帧；可与 pillarRamp 叠加） ——
        /// <summary>落地爆发序列帧图集贴图（横向等宽排布，侧视构图：底部贴地、顶部朝北）。</summary>
        public string impactBurstTexPath;
        /// <summary>图集帧数（等宽切片）。</summary>
        public int impactBurstFrameCount = 1;
        /// <summary>每帧停留 tick 数。</summary>
        public int impactBurstFrameTicks = 3;
        /// <summary>特效总时长秒数（≤0 = 自动按 帧数×每帧tick 播完一轮）。</summary>
        public float impactBurstDurationSeconds;
        /// <summary>绘制宽/高（格，正方形帧两者取同值；建议 ≈ 爆炸半径 × 2 ÷ 环带占帧宽比例）。</summary>
        public float impactBurstWidthCells = 5f;
        public float impactBurstHeightCells = 5f;
        /// <summary>锚点沿世界 Z 平移（格，负值=向南压：帧图爆发中心在底边上方 f 处时取 -f × 高，把环心拉回撞击点）。</summary>
        public float impactBurstOffsetZCells;
        /// <summary>染色（alpha=0 视为纯白，帧图自带颜色时保持默认）。</summary>
        public Color impactBurstTint = new Color(0f, 0f, 0f, 0f);
        /// <summary>首/尾淡入淡出秒数。</summary>
        public float impactBurstFadeInSeconds;
        public float impactBurstFadeOutSeconds = 0.1f;

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
            MMTDelayedStrikeParams p = new MMTDelayedStrikeParams
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
                meteorDrawSize = Props.meteorDrawSize,
                meteorSpinDegPerTick = Props.meteorSpinDegPerTick,
                meteorGrowFrom = Props.meteorGrowFrom,
                meteorGrowTo = Props.meteorGrowTo,
                anticipateSound = Props.anticipateSoundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.anticipateSoundDefName),
                equipment = caster.equipment?.Primary
            };
            if (!Props.pillarRampTexPath.NullOrEmpty())
            {
                p.pillarRamp = new MMTSkillSideRampParams
                {
                    texPath = Props.pillarRampTexPath,
                    frameCount = Props.pillarRampFrameCount,
                    frameTicks = Props.pillarRampFrameTicks,
                    widthCells = Props.pillarRampWidthCells,
                    heightCells = Props.pillarRampHeightCells,
                    driftCells = Props.pillarRampDriftCells,
                    slices = Props.pillarRampSlices,
                    sway = Props.pillarRampSway,
                    swayPeriodTicks = Props.pillarRampSwayPeriodTicks,
                    topScale = Props.pillarRampTopScale,
                    topAlpha = Props.pillarRampTopAlpha,
                    pulseAmp = Props.pillarRampPulseAmp,
                    tint = Props.pillarRampTint,
                    fadeInSeconds = Props.pillarRampFadeInSeconds,
                    fadeOutSeconds = Props.pillarRampFadeOutSeconds
                };
            }
            if (!Props.impactBurstTexPath.NullOrEmpty())
            {
                int burstFrames = Mathf.Max(1, Props.impactBurstFrameCount);
                int burstFrameTicks = Mathf.Max(1, Props.impactBurstFrameTicks);
                p.impactSideFlat = new MMTSkillSideFlatParams
                {
                    texPath = Props.impactBurstTexPath,
                    frameCount = burstFrames,
                    frameTicks = burstFrameTicks,
                    loop = false,
                    durationSeconds = Props.impactBurstDurationSeconds > 0f
                        ? Props.impactBurstDurationSeconds
                        : burstFrames * burstFrameTicks / 60f,
                    widthCells = Props.impactBurstWidthCells,
                    heightCells = Props.impactBurstHeightCells,
                    offsetZCells = Props.impactBurstOffsetZCells,
                    tint = Props.impactBurstTint,
                    fadeInSeconds = Props.impactBurstFadeInSeconds,
                    fadeOutSeconds = Props.impactBurstFadeOutSeconds
                };
            }
            MMTDelayedStrikeUtil.FireDelayedStrike(caster, target.Cell, p);
        }
    }
}
