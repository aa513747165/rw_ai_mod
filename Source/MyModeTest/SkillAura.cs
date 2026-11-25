using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 技能光环框架（第七步：以目标为中心随其移动的持续光环） ==========
    // 光环挂在目标身上（pawn 或建筑均可），目标移动光环跟随：
    // · 盾类（shieldMode）：复刻原版「临时护盾投影器」（Biotech MechShield，
    //   Def Ethereal_Various.xml）——Thing_MMTSkillAuraShield 继承 Verse.MechShield，
    //   SetTarget 后 DrawPos 自动跟随目标，CompProjectileInterceptor 负责力场泡绘制 +
    //   投射物拦截 + 命中点/EMP 机制，完全走原版逻辑；时限到或目标失效自毁。
    // · 增益/减益类（buffMode）：Thing_MMTSkillAuraBuff（AttachableThing.AttachTo 目标），
    //   画贴图光圈（半径=技能半径，圆盘浮在小人身体中心高度 + GenDraw 地面描边，敌我配色），按间隔
    //   对范围内 pawn 施加/续期 Hediff（增益友方绿 / 减益敌方红，效果数值全在 HediffDef
    //   XML 配，如 MoveSpeed ×1.5 = +50% 移速），离开范围后 Hediff 经原版
    //   HediffComp_Disappears 自动消退。
    // MMTAuraUtil.StartAura 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTAura
    // （以自己为中心，预览圈走第一步 AoETarget 框架）；建筑演示侧出口
    // AbilityTargeting.cs 的 CompSkillDemo（demoType="aura"，免瞄准直接挂自身）。
    // 注：光环参数为运行时传入不进存档，读档后自毁（光环最长数十秒，可接受）。

    /// <summary>光环参数包（MMTAuraUtil.StartAura 的输入）。</summary>
    public class MMTAuraParams
    {
        /// <summary>光环半径（格）。</summary>
        public float radius = 4f;
        /// <summary>持续 tick 数。</summary>
        public int durationTicks = 600;
        /// <summary>光圈染色（盾类不用，护盾泡颜色走 comp XML）。</summary>
        public Color tint = new Color(0.3f, 1f, 0.3f);
        /// <summary>true=作用于友方（增益），false=作用于敌方（减益）。</summary>
        public bool targetFriendly = true;
        public bool includeSelf = true;
        /// <summary>true=原版范围护盾（临时护盾投影器），false=增益/减益光圈。</summary>
        public bool shieldMode;
        /// <summary>盾类：护盾命中点（0=用 comp XML 默认值）。</summary>
        public int shieldHitPoints;
        /// <summary>增益/减益类：脉冲施加给范围内 pawn 的 Hediff（null=纯光圈提示）。</summary>
        public HediffDef buffDef;
        /// <summary>增益/减益脉冲间隔 tick。</summary>
        public int refreshIntervalTicks = 60;
        /// <summary>光圈地面圆盘贴图（白图，代码染色）。</summary>
        public string discTexPath = "Things/Mote/MMT_AuraDisc";
        public SoundDef startSound;
    }

    /// <summary>光环统一入口：盾类 / 增益减益类，挂到目标身上随其移动。</summary>
    public static class MMTAuraUtil
    {
        public const string ShieldDefName = "MMT_SkillAuraShield";
        public const string BuffDefName = "MMT_SkillAuraBuff";

        /// <summary>在 follow 目标身上展开光环（pawn/建筑均可），返回光环实体（失败返回 null）。</summary>
        public static Thing StartAura(Thing caster, Thing follow, MMTAuraParams p)
        {
            Map map = follow?.MapHeld;
            if (map == null || p == null || !follow.Spawned)
            {
                return null;
            }
            string defName = p.shieldMode ? ShieldDefName : BuffDefName;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + defName + " 定义，无法展开光环");
                return null;
            }
            Thing aura;
            if (p.shieldMode)
            {
                Thing_MMTSkillAuraShield shield = (Thing_MMTSkillAuraShield)GenSpawn.Spawn(def, follow.Position, map);
                shield.Setup(follow, p.durationTicks, p.shieldHitPoints);
                aura = shield;
            }
            else
            {
                Thing_MMTSkillAuraBuff buff = (Thing_MMTSkillAuraBuff)GenSpawn.Spawn(def, follow.Position, map);
                buff.Setup(caster, follow, p);
                aura = buff;
            }
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(follow.Position, map));
            }
            return aura;
        }
    }

    /// <summary>
    /// 盾类光环：继承原版临时护盾投影器（MechShield），SetTarget 后力场泡跟随目标移动；
    /// 拦截/命中点/EMP/力场绘制全走原版 CompProjectileInterceptor，本类只管激活、
    /// 命中点覆写与时限/跟随目标失效自毁。
    /// </summary>
    public class Thing_MMTSkillAuraShield : MechShield
    {
        private Thing follow;
        private int ticksLeft = -1;

        public void Setup(Thing followTarget, int durationTicks, int hitPoints)
        {
            follow = followTarget;
            ticksLeft = Mathf.Max(1, durationTicks);
            SetTarget(followTarget);
            CompProjectileInterceptor interceptor = this.GetComp<CompProjectileInterceptor>();
            if (interceptor == null)
            {
                Log.Error("[MMT] MMT_SkillAuraShield 缺少 CompProjectileInterceptor");
                return;
            }
            if (hitPoints > 0)
            {
                interceptor.maxHitPointsOverride = hitPoints;
                interceptor.currentHitPoints = hitPoints;
            }
            interceptor.Activate();
        }

        protected override void Tick()
        {
            base.Tick();
            if (ticksLeft < 0 || follow == null || follow.Destroyed || follow.MapHeld != Map)
            {
                Destroy();
                return;
            }
            if (--ticksLeft <= 0)
            {
                Destroy();
            }
        }
    }

    /// <summary>
    /// 增益/减益类光环：Attach 到目标跟随移动，贴图光圈浮在小人身体中心高度（半径=技能半径），
    /// 按脉冲间隔对范围内 pawn 施加/续期 Hediff；时限到或目标失效自毁。
    /// </summary>
    public class Thing_MMTSkillAuraBuff : AttachableThing
    {
        private Thing caster;
        private Thing follow;
        private MMTAuraParams auraParams;
        private int ticksLeft = -1;
        private int ticksUntilPulse;
        private Material discMat;
        /// <summary>圆盘整体不透明度上限（淡入淡出/呼吸脉冲之前的基准）。</summary>
        private const float DiscBaseOpacity = 0.28f;

        /// <summary>1.7 AttachableThing 抽象成员；光环不可选，仅调试可见。</summary>
        public override string InspectStringAddon => "技能光环（演示实体）";

        public void Setup(Thing caster, Thing followTarget, MMTAuraParams p)
        {
            this.caster = caster;
            follow = followTarget;
            auraParams = p;
            ticksLeft = Mathf.Max(1, p.durationTicks);
            ticksUntilPulse = 1;
            AttachTo(followTarget);
        }

        protected override void Tick()
        {
            base.Tick();
            if (auraParams == null || ticksLeft < 0 || follow == null || follow.Destroyed || follow.MapHeld != Map)
            {
                Destroy();
                return;
            }
            if (--ticksLeft <= 0)
            {
                Destroy();
                return;
            }
            if (auraParams.buffDef != null && --ticksUntilPulse <= 0)
            {
                ticksUntilPulse = Mathf.Max(1, auraParams.refreshIntervalTicks);
                PulseBuff();
            }
        }

        /// <summary>一次增益/减益脉冲：范围内 pawn（按敌我过滤）施加/续期 Hediff。</summary>
        private void PulseBuff()
        {
            Map map = Map;
            foreach (Pawn p in MMTAbilityUtil.PawnsInRadius(follow.Position, map, auraParams.radius, caster, auraParams.targetFriendly, auraParams.includeSelf))
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(p, auraParams.buffDef);
            }
        }

        /// <summary>画光环：贴图圆盘浮在小人身体中心高度（DrawPos 平滑跟随，呼吸脉冲 + 淡入淡出）。
        /// 不再叠瞄准风格的描边圈：贴图盘自带发光边缘，两个圆心/半径来源不同会看出"双环分离"。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (auraParams == null || follow == null || Map != Find.CurrentMap)
            {
                return;
            }
            if (discMat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(auraParams.discTexPath, false);
                if (tex == null)
                {
                    return;
                }
                discMat = new Material(ShaderDatabase.TransparentPostLight) { mainTexture = tex };
            }
            int elapsed = auraParams.durationTicks - ticksLeft;
            float alpha = Mathf.Clamp01(elapsed / 15f) * Mathf.Clamp01(ticksLeft / 30f);
            float pulse = 0.75f + 0.25f * Mathf.Sin(elapsed * 0.09f);
            Color color = auraParams.tint;
            // 贴图内部接近实心，tint alpha 又是 1，不压基准则整片浓绿/浓红盖住地面
            color.a *= alpha * pulse * DiscBaseOpacity;
            discMat.color = color;
            // 高度取小人层（身体中心），略高于 Pawn 层避免与本体闪面；DrawPos 平滑跟随移动
            Vector3 pos = follow.DrawPos;
            pos.y = AltitudeLayer.Pawn.AltitudeFor() + 0.1f;
            float size = auraParams.radius * 2f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)), discMat, 0);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (discMat != null)
            {
                Object.Destroy(discMat);
                discMat = null;
            }
            base.Destroy(mode);
        }
    }

    // ========== pawn 技能出口：光环（以自己为中心） ==========

    public class CompProperties_MMTAura : CompProperties_MMTAoETargetBase
    {
        /// <summary>true=原版范围护盾，false=增益/减益光圈。</summary>
        public bool shieldMode;
        public int shieldHitPoints = 200;
        /// <summary>增益/减益类施加的 Hediff（空=纯光圈提示）。</summary>
        public string buffDefName;
        public int durationTicks = 600;
        public int refreshIntervalTicks = 60;
        /// <summary>光圈染色，alpha=0 时按敌我自动配色。</summary>
        public Color tint = new Color(0f, 0f, 0f, 0f);
        public string discTexPath = "Things/Mote/MMT_AuraDisc";
        public string soundDefName;

        public CompProperties_MMTAura()
        {
            compClass = typeof(CompAbilityEffect_MMTAura);
        }
    }

    /// <summary>光环技能：以自己为中心展开持续光环（radius 即光环半径，预览圈走 AoETarget 基类）。</summary>
    public class CompAbilityEffect_MMTAura : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTAura Props => (CompProperties_MMTAura)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Color tint = Props.tint.a > 0f ? Props.tint : MMTAoETargetPreview.RingColor(Props.targetFriendly);
            MMTAuraUtil.StartAura(caster, caster, new MMTAuraParams
            {
                radius = Props.radius,
                durationTicks = Props.durationTicks,
                tint = tint,
                targetFriendly = Props.targetFriendly,
                includeSelf = Props.includeSelf,
                shieldMode = Props.shieldMode,
                shieldHitPoints = Props.shieldHitPoints,
                buffDef = Props.buffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(Props.buffDefName),
                refreshIntervalTicks = Props.refreshIntervalTicks,
                discTexPath = Props.discTexPath,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            });
        }
    }
}
