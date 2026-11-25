using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 增益/减益特效框架（第八步：目标头顶图标 + 身上粒子） ==========
    // 施法完成后，技能范围内每个有效目标：
    // · 头顶短暂浮现增益/减益小图标（Thing_MMTBuffIconFlash，AttachTo 目标跟随，
    //   淡入淡出 + 缓缓上浮，图标贴图/染色/时长全走运行时参数，增益绿 / 减益红）；
    // · 身上持续冒少量小光点粒子（FleckDef 投掷基类 + instanceColor 染色 + 上飘速度）。
    // 同时经 MMTAbilityUtil.ApplyOrRefreshHediff 施加实际增益/减益 Hediff（数值全在
    // HediffDef XML 配，如 MoveSpeed ×1.5）；建筑等非 pawn 目标只放特效不加 Hediff。
    // 技能分单选（buffRequireTarget 锁一名目标）与范围（radius 圈选）两种，敌我限定
    // 沿用第一步框架。MMTBuffFXUtil.Apply 为统一入口；pawn 技能侧出口
    // CompAbilityEffect_MMTBuffFX；建筑演示侧出口 AbilityTargeting.cs 的 CompSkillDemo
    // （demoType="bufffx"）。素材仅原版 UI 图标 + MMT 占位光点贴图，不依赖 RoM。
    // 注：特效实体参数为运行时传入不进存档，读档后自毁（图标仅存活一两秒，可接受）。

    /// <summary>增益/减益特效参数包（MMTBuffFXUtil 的输入）。</summary>
    public class MMTBuffFXParams
    {
        /// <summary>实际施加的增益/减益 Hediff（null=纯特效提示）。</summary>
        public HediffDef buffDef;
        /// <summary>头顶小图标贴图（空=不画图标）。</summary>
        public string iconTexPath = "UI/Icons/MoveSpeedBonus";
        /// <summary>图标染色（增益绿 / 减益红）。</summary>
        public Color iconTint = new Color(0.3f, 1f, 0.3f);
        /// <summary>图标显示 tick 数。</summary>
        public int iconDurationTicks = 90;
        /// <summary>图标尺寸（格）。</summary>
        public float iconScale = 0.45f;
        /// <summary>身上粒子 fleck（null=不放粒子）。</summary>
        public FleckDef particleFleckDef;
        public Color particleTint = new Color(0.3f, 1f, 0.3f);
        /// <summary>粒子间隔 tick。</summary>
        public int particleIntervalTicks = 8;
        public SoundDef startSound;
    }

    /// <summary>增益/减益统一入口：施 Hediff + 头顶图标 + 身上粒子。</summary>
    public static class MMTBuffFXUtil
    {
        public const string IconFlashDefName = "MMT_BuffIconFlash";

        /// <summary>对一批目标施加增益/减益（pawn 加 Hediff，所有目标放特效），返回命中数。</summary>
        public static int Apply(Thing caster, IEnumerable<Thing> targets, MMTBuffFXParams p)
        {
            int count = 0;
            Thing first = null;
            foreach (Thing t in targets)
            {
                ApplyOne(t, p);
                if (first == null)
                {
                    first = t;
                }
                count++;
            }
            if (count > 0 && p.startSound != null && first != null && first.MapHeld != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(first.Position, first.MapHeld));
            }
            return count;
        }

        /// <summary>对单个目标施加：pawn 施/续 Hediff；头顶图标 + 粒子特效（pawn/建筑均可）。</summary>
        public static void ApplyOne(Thing target, MMTBuffFXParams p)
        {
            if (target == null || target.Destroyed || !target.Spawned || p == null)
            {
                return;
            }
            if (target is Pawn pawn && p.buffDef != null && !pawn.Dead)
            {
                MMTAbilityUtil.ApplyOrRefreshHediff(pawn, p.buffDef);
            }
            if (p.iconTexPath.NullOrEmpty() && p.particleFleckDef == null)
            {
                return;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(IconFlashDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + IconFlashDefName + " 定义，无法播放增益特效");
                return;
            }
            Thing_MMTBuffIconFlash flash = (Thing_MMTBuffIconFlash)GenSpawn.Spawn(def, target.Position, target.Map);
            flash.Setup(target, p);
        }
    }

    /// <summary>
    /// 增益/减益头顶图标 + 粒子实体：Attach 到目标跟随移动，头顶画小图标
    /// （淡入淡出 + 缓缓上浮），按间隔向身上抛小光点粒子；时限到或目标失效自毁。
    /// </summary>
    public class Thing_MMTBuffIconFlash : AttachableThing
    {
        /// <summary>图标距目标中心的初始高度（格）。</summary>
        private const float IconBaseHeight = 0.9f;
        /// <summary>图标存续期间上浮距离（格）。</summary>
        private const float IconRise = 0.3f;

        private Thing follow;
        private MMTBuffFXParams fx;
        private int ticksTotal = -1;
        private int ticksLeft = -1;
        private int particleTimer;
        private Material iconMat;

        /// <summary>1.7 AttachableThing 抽象成员；特效不可选，仅调试可见。</summary>
        public override string InspectStringAddon => "技能增益特效（演示实体）";

        public void Setup(Thing followTarget, MMTBuffFXParams p)
        {
            follow = followTarget;
            fx = p;
            ticksTotal = ticksLeft = Mathf.Max(1, p.iconDurationTicks);
            particleTimer = 1;
            // 建筑等无 CompAttachBase 的目标不能 Attach（AttachTo 报红字且 parent 留空，
            // 之后 AttachableThing.Tick 每 tick NRE）；跟随/绘制本就只读 follow.DrawPos，
            // 仅对可挂载目标（pawn 等）走 Attach。
            if (followTarget.TryGetComp<CompAttachBase>() != null)
            {
                AttachTo(followTarget);
            }
        }

        protected override void Tick()
        {
            // 未 Attach（目标是建筑）时 parent 为空，AttachableThing.Tick 会 NRE，跳过基类
            if (parent != null)
            {
                base.Tick();
            }
            if (fx == null || ticksLeft < 0 || follow == null || follow.Destroyed || follow.MapHeld != Map)
            {
                Destroy();
                return;
            }
            if (--ticksLeft <= 0)
            {
                Destroy();
                return;
            }
            if (fx.particleFleckDef != null && --particleTimer <= 0)
            {
                particleTimer = Mathf.Max(1, fx.particleIntervalTicks);
                SpawnSpark();
            }
        }

        /// <summary>身上小光点：投掷基类 fleck，随机落点 + 上飘速度，instanceColor 染色。</summary>
        private void SpawnSpark()
        {
            Map map = Map;
            if (map == null || map != Find.CurrentMap)
            {
                return;
            }
            Vector3 pos = follow.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            pos += new Vector3(Rand.Range(-0.35f, 0.35f), 0f, Rand.Range(-0.2f, 0.45f));
            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, fx.particleFleckDef, Rand.Range(0.25f, 0.45f));
            data.velocity = new Vector3(Rand.Range(-0.12f, 0.12f), 0f, Rand.Range(0.3f, 0.55f));
            data.rotationRate = Rand.Range(-40f, 40f);
            data.instanceColor = fx.particleTint;
            map.flecks.CreateFleck(data);
        }

        /// <summary>头顶小图标：淡入淡出 + 缓缓上浮（DrawPos 平滑跟随目标移动）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (fx == null || follow == null || Map != Find.CurrentMap || fx.iconTexPath.NullOrEmpty())
            {
                return;
            }
            if (iconMat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(fx.iconTexPath, false);
                if (tex == null)
                {
                    return;
                }
                iconMat = new Material(ShaderDatabase.TransparentPostLight) { mainTexture = tex };
            }
            int elapsed = ticksTotal - ticksLeft;
            float alpha = Mathf.Clamp01(elapsed / 6f) * Mathf.Clamp01(ticksLeft / 18f);
            Color color = fx.iconTint;
            color.a *= alpha;
            iconMat.color = color;
            Vector3 pos = follow.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            pos.z += IconBaseHeight + IconRise * (elapsed / (float)ticksTotal);
            float size = fx.iconScale * 2f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)), iconMat, 0);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (iconMat != null)
            {
                Object.Destroy(iconMat);
                iconMat = null;
            }
            base.Destroy(mode);
        }
    }

    // ========== pawn 技能出口：增益/减益（单体走目标，范围走预览圈） ==========

    public class CompProperties_MMTBuffFX : CompProperties_MMTAoETargetBase
    {
        /// <summary>施加的 Hediff（空=纯特效提示）。</summary>
        public string buffDefName;
        public string iconTexPath = "UI/Icons/MoveSpeedBonus";
        /// <summary>图标染色，alpha=0 时按敌我自动配色。</summary>
        public Color iconTint = new Color(0f, 0f, 0f, 0f);
        public float iconSeconds = 1.5f;
        public string particleFleckDefName = "MMT_Fleck_BuffSpark";
        public Color particleTint = new Color(0f, 0f, 0f, 0f);
        public int particleIntervalTicks = 8;
        public string soundDefName;

        public CompProperties_MMTBuffFX()
        {
            compClass = typeof(CompAbilityEffect_MMTBuffFX);
        }
    }

    /// <summary>增益/减益技能：radius>0 圈选范围目标，radius=0 只作用于点选目标。</summary>
    public class CompAbilityEffect_MMTBuffFX : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTBuffFX Props => (CompProperties_MMTBuffFX)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Color tint = Props.iconTint.a > 0f ? Props.iconTint : MMTAoETargetPreview.RingColor(Props.targetFriendly);
            MMTBuffFXParams p = new MMTBuffFXParams
            {
                buffDef = Props.buffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(Props.buffDefName),
                iconTexPath = Props.iconTexPath,
                iconTint = tint,
                iconDurationTicks = Mathf.Max(1, Props.iconSeconds.SecondsToTicks()),
                particleFleckDef = Props.particleFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(Props.particleFleckDefName),
                particleTint = Props.particleTint.a > 0f ? Props.particleTint : tint,
                particleIntervalTicks = Props.particleIntervalTicks,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            };
            if (Props.radius > 0f)
            {
                MMTBuffFXUtil.Apply(caster, AffectedPawns(target), p);
            }
            else if (target.HasThing)
            {
                MMTBuffFXUtil.ApplyOne(target.Thing, p);
            }
        }
    }
}
