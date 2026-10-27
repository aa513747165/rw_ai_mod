using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// MMT 特殊护盾腰带基类参数：在 vanilla 护盾参数上追加气泡贴图路径（XML 可配置）。
    /// 不直接用于 XML，由具体护盾的参数子类继承。
    /// </summary>
    public class CompProperties_ShieldExBase : CompProperties_Shield
    {
        /// <summary>护盾气泡贴图路径（Textures/ 下，不带扩展名）。</summary>
        public string bubbleTexPath;
    }

    /// <summary>
    /// MMT 特殊护盾腰带抽象基类（参考 More Shield Belts 的四种护盾提炼公共逻辑）：
    /// 与原版护盾腰带一样全额拦截伤害，但“资源驱动”完全交给子类（灵能熵/血液/虚空等），
    /// 本类负责公共部分：气泡绘制、吸收/破盾特效、征召开关 gizmo、破盾/重启流转。
    /// 状态流转：激活（ticksToReset=-1）--受击耗尽资源/手动关闭--> 破盾（ticksToReset=1）
    /// --满足 CanAutoReset 且开关开启--> 重启回激活。不调用基类 CompTick（原版能量回复不适用）。
    /// </summary>
    public abstract class CompShieldExBase : CompShield
    {
        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        /// <summary>护盾开关状态（存档持久化）。</summary>
        protected bool shouldactive = true;

        // 气泡材质惰性缓存（自定义贴图独占一份，不污染原版材质池同名项）
        private Material bubbleMat;

        private CompProperties_ShieldExBase PropsEx
        {
            get { return (CompProperties_ShieldExBase)props; }
        }

        /// <summary>气泡材质；未配置贴图时回退原版护盾气泡。</summary>
        protected Material BubbleMat
        {
            get
            {
                if (bubbleMat == null)
                {
                    string path = PropsEx.bubbleTexPath.NullOrEmpty() ? "Other/ShieldBubble" : PropsEx.bubbleTexPath;
                    bubbleMat = MaterialPool.MatFrom(path, ShaderDatabase.Transparent);
                }
                return bubbleMat;
            }
        }

        // ---- 子类钩子：资源驱动差异 ----

        /// <summary>当前资源余量比例（0-1），决定气泡绘制尺寸。</summary>
        protected abstract float ResourceFraction { get; }

        /// <summary>开关按钮文字（如“灵能护盾”）。</summary>
        protected abstract string ToggleLabel { get; }

        /// <summary>开关按钮说明（解释该护盾的资源机制）。</summary>
        protected abstract string ToggleDesc { get; }

        /// <summary>开启门槛：返回 null 表示可开启，否则返回禁用原因（中文）。</summary>
        protected abstract string GetActivateDisabledReason(Pawn pawn);

        /// <summary>破盾后自动重启条件（每 tick 检查）。</summary>
        protected abstract bool CanAutoReset();

        /// <summary>结算吸收这次伤害的资源代价；返回 false 表示资源耗尽、护盾击破。</summary>
        protected abstract bool ConsumeResource(DamageInfo dinfo);

        /// <summary>护盾进入激活状态时调用（子类可挂增益，如环境保护）。</summary>
        protected virtual void OnShieldUp()
        {
        }

        /// <summary>护盾离开激活状态时调用（破盾/关闭/卸下）。</summary>
        protected virtual void OnShieldDown()
        {
        }

        // ---- 公共流转 ----

        /// <summary>破盾后且开关开启时，资源恢复到阈值自动重启。</summary>
        public override void CompTick()
        {
            if (PawnOwner != null && shouldactive && ShieldState == ShieldState.Resetting && CanAutoReset())
                ResetShield();
        }

        /// <summary>拦截全部伤害（含近战）；资源够则吸收，不够则破盾。EMP 不特判，按普通伤害结算。</summary>
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (ShieldState != ShieldState.Active || PawnOwner == null)
                return;
            DamageDef def = dinfo.Def;
            // 治疗/处决/手术切口不拦截（处决必须放行，否则无法处决穿戴者）
            if (def.consideredHelpful || def.execution || def == DamageDefOf.SurgicalCut)
                return;
            absorbed = true;
            if (ConsumeResource(dinfo))
                AbsorbedDamageFx(dinfo);
            else
                BreakShield();
        }

        /// <summary>征召时的护盾开关（不调用基类，屏蔽原版能量条 gizmo）。</summary>
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            Pawn owner = PawnOwner;
            if (owner == null || !owner.Drafted)
                yield break;
            Command_Toggle toggle = new Command_Toggle();
            toggle.defaultLabel = ToggleLabel + "开关";
            toggle.defaultDesc = ToggleDesc;
            toggle.alsoClickIfOtherInGroupClicked = true;
            toggle.isActive = () => shouldactive;
            toggle.toggleAction = delegate { ToggleShield(); };
            toggle.icon = ContentFinder<Texture2D>.Get(PropsEx.bubbleTexPath.NullOrEmpty() ? "Other/ShieldBubble" : PropsEx.bubbleTexPath);
            if (!shouldactive)
            {
                string reason = GetActivateDisabledReason(owner);
                if (reason != null)
                    toggle.Disable(reason);
            }
            yield return toggle;
        }

        /// <summary>屏蔽原版护盾状态条（本类护盾无“能量”概念）。</summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }

        /// <summary>开关切换：关闭立即破盾；开启需满足子类门槛，资源达标则立即重启。</summary>
        private void ToggleShield()
        {
            Pawn owner = PawnOwner;
            if (owner == null)
                return;
            if (shouldactive)
            {
                shouldactive = false;
                // 节点流转：激活 -> 破盾（手动关闭）
                if (ShieldState == ShieldState.Active)
                    SwitchBreak();
                return;
            }
            if (GetActivateDisabledReason(owner) != null)
                return;
            shouldactive = true;
            // 节点流转：破盾 -> 激活（资源达标立即重启）
            if (ShieldState == ShieldState.Resetting && CanAutoReset())
                ResetShield();
        }

        /// <summary>穿戴时校验开启门槛（如灵能护盾要求启灵等级），不满足则保持关闭。</summary>
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (GetActivateDisabledReason(pawn) != null)
            {
                shouldactive = false;
                ticksToReset = 1;
            }
            else if (ShieldState == ShieldState.Active)
            {
                OnShieldUp();
            }
        }

        /// <summary>卸下时摘下护盾增益。</summary>
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            OnShieldDown();
        }

        /// <summary>绘制气泡：尺寸随资源余量缩放，受击后 8 tick 内向受击反方向抖动。</summary>
        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (IsApparel)
                DrawBubble();
        }

        private void DrawBubble()
        {
            if (ShieldState != ShieldState.Active || !ShouldDisplay)
                return;
            float size = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, Mathf.Clamp01(ResourceFraction));
            Vector3 drawPos = PawnOwner.Drawer.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            int ticksSinceHit = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (ticksSinceHit < 8)
            {
                float jitter = (8 - ticksSinceHit) / 8f * 0.05f;
                drawPos += impactAngleVect * jitter;
                size -= jitter;
            }
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up), new Vector3(size, 1f, size));
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }

        /// <summary>吸收特效：音效 + 受击方向闪光与尘雾，并保持气泡短暂可见。</summary>
        private void AbsorbedDamageFx(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = PawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float flashScale = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, PawnOwner.Map, FleckDefOf.ExplosionFlash, flashScale);
            int dustCount = (int)flashScale;
            for (int i = 0; i < dustCount; i++)
                FleckMaker.ThrowDustPuff(loc, PawnOwner.Map, Rand.Range(0.8f, 1.2f));
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            KeepDisplaying();
        }

        /// <summary>破盾：播放护盾碎裂特效后进入破盾状态。</summary>
        private void BreakShield()
        {
            if (parent.Spawned)
            {
                float scale = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, Mathf.Clamp01(ResourceFraction));
                EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
                FleckMaker.Static(PawnOwner.TrueCenter(), PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(
                        PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f),
                        PawnOwner.Map, Rand.Range(0.8f, 1.2f));
                }
            }
            SwitchBreak();
        }

        /// <summary>进入破盾状态（ticksToReset=1 使 ShieldState 恒为 Resetting）。</summary>
        private void SwitchBreak()
        {
            ticksToReset = 1;
            OnShieldDown();
        }

        /// <summary>重启护盾：音效 + 电弧闪光，回到激活状态。</summary>
        private void ResetShield()
        {
            if (PawnOwner.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
                FleckMaker.ThrowLightningGlow(PawnOwner.TrueCenter(), PawnOwner.Map, 3f);
            }
            ticksToReset = -1;
            OnShieldUp();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref shouldactive, "shouldactive", true);
        }
    }

    /// <summary>灵能护盾腰带参数。</summary>
    public class CompProperties_ShieldPsychic : CompProperties_ShieldExBase
    {
        public CompProperties_ShieldPsychic()
        {
            compClass = typeof(CompShieldPsychic);
        }
    }

    /// <summary>
    /// 灵能护盾腰带（参考 More Shield Belts 的 CompShieldPsychic 搬运）：
    /// 不耗电，改烧穿戴者灵能——每吸收 1 点伤害增加等量精神熵（熵上限由 EnergyShieldEnergyMax=1
    /// 换算 ShieldFactor=1），并按“熵增量/熵上限”概率扣除等量精神集中；
    /// 精神集中归零或精神熵超载即破盾；熵低于 50% 且精神集中不低于 50% 时自动重启。
    /// 仅启灵等级 &gt; 0 的穿戴者可开启；启灵 &gt;= 5 级时护盾激活期间附加环境保护（MMT_ShieldEnvironmentProtect）。
    /// </summary>
    public class CompShieldPsychic : CompShieldExBase
    {
        /// <summary>启灵达到该等级时护盾附带环境保护。</summary>
        private const int EnvProtectPsylinkLevel = 5;

        // 环境保护 Hediff 惰性查找（DefDatabase 查表，避免 [DefOf] 在缺 Def 时炸档）
        private HediffDef envProtectDef;

        private HediffDef EnvProtectDef
        {
            get
            {
                if (envProtectDef == null)
                    envProtectDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_ShieldEnvironmentProtect");
                return envProtectDef;
            }
        }

        private Pawn_PsychicEntropyTracker Tracker
        {
            get { return PawnOwner?.psychicEntropy; }
        }

        /// <summary>伤害 -> 精神熵换算系数：1 / 护盾能量上限（XML 中 EnergyShieldEnergyMax=1 即 1:1）。</summary>
        private float ShieldFactor
        {
            get { return 1f / parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax); }
        }

        /// <summary>气泡尺寸随精神集中缩放（4 倍放大，25% 集中即满尺寸）。</summary>
        protected override float ResourceFraction
        {
            get { return (Tracker?.CurrentPsyfocus ?? 0f) * 4f; }
        }

        protected override string ToggleLabel
        {
            get { return "灵能护盾"; }
        }

        protected override string ToggleDesc
        {
            get
            {
                return "以穿戴者的灵能驱动护盾，全额吸收所有伤害（含近战）：每吸收 1 点伤害增加等量精神熵，"
                    + "并按熵增量占上限的比例概率扣除精神集中；精神集中归零或精神熵超载时护盾击破，"
                    + "熵低于 50% 且精神集中不低于 50% 时自动重启。仅启灵者可开启；启灵 5 级以上附加环境保护。";
            }
        }

        /// <summary>仅启灵等级 &gt; 0 的穿戴者可开启。</summary>
        protected override string GetActivateDisabledReason(Pawn pawn)
        {
            if (pawn == null || pawn.psychicEntropy == null || pawn.GetPsylinkLevel() <= 0)
                return "需要启灵等级";
            return null;
        }

        /// <summary>自动重启条件：精神熵低于 50% 上限且精神集中不低于 50%。</summary>
        protected override bool CanAutoReset()
        {
            Pawn_PsychicEntropyTracker tracker = Tracker;
            if (tracker == null || tracker.MaxEntropy <= 0f)
                return false;
            return tracker.EntropyValue / tracker.MaxEntropy < 0.5f && tracker.CurrentPsyfocus >= 0.5f;
        }

        /// <summary>
        /// 伤害转灵能消耗：全额转精神熵，按熵增量/上限概率扣等量精神集中；
        /// 精神集中耗尽或熵超载（受熵上限约束者必破，不受约束者仅当本次扣了集中才破）返回 false 破盾。
        /// </summary>
        protected override bool ConsumeResource(DamageInfo dinfo)
        {
            Pawn_PsychicEntropyTracker tracker = Tracker;
            if (tracker == null || tracker.MaxEntropy <= 0f)
                return false;
            float entropyCost = dinfo.Amount * ShieldFactor;
            float psyfocusCost = entropyCost / tracker.MaxEntropy;
            bool drainFocus = Rand.Chance(psyfocusCost);
            tracker.TryAddEntropy(entropyCost, null, true, true);
            if (drainFocus)
            {
                tracker.OffsetPsyfocusDirectly(0f - psyfocusCost);
                // 精神集中耗尽 -> 破盾
                if (tracker.CurrentPsyfocus <= 0f)
                    return false;
            }
            // 熵超载 -> 破盾
            if (tracker.EntropyValue / tracker.MaxEntropy >= 1f && (tracker.limitEntropyAmount || drainFocus))
                return false;
            return true;
        }

        /// <summary>护盾激活时：启灵 5 级以上附加环境保护。</summary>
        protected override void OnShieldUp()
        {
            Pawn owner = PawnOwner;
            if (owner != null && owner.GetPsylinkLevel() >= EnvProtectPsylinkLevel && EnvProtectDef != null)
                owner.health.AddHediff(EnvProtectDef);
        }

        /// <summary>护盾失效/卸下时：摘除环境保护。</summary>
        protected override void OnShieldDown()
        {
            Pawn owner = PawnOwner;
            if (owner == null || EnvProtectDef == null)
                return;
            Hediff hediff = owner.health.hediffSet.GetFirstHediffOfDef(EnvProtectDef);
            if (hediff != null)
                owner.health.RemoveHediff(hediff);
        }
    }
}
