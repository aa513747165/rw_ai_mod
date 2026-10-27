using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 充能护盾背包参数：持续耗电与伤害吸收耗电系数（XML 可配置）。
    /// 电量由同物品上的 CompRechargeable 提供。
    /// </summary>
    public class CompProperties_ShieldPack : CompProperties
    {
        /// <summary>开启期间持续耗电（Wd/天）。</summary>
        public float passiveDrainWdPerDay = 300f;

        /// <summary>每吸收 1 点伤害的耗电（Wd）。</summary>
        public float wdPerDamage = 40f;

        public CompProperties_ShieldPack()
        {
            compClass = typeof(CompShieldPack);
        }
    }

    /// <summary>
    /// 充能护盾背包核心 Comp（挂在腰带层服装上）：
    /// 征召时指令栏出现开关按钮；开启后全额吸收所有伤害（含近战），按伤害量耗电，
    /// 同时持续缓慢耗电；电能耗尽或解除征召自动关闭。EMP 直接清空电量并击穿护盾。
    /// </summary>
    public class CompShieldPack : ThingComp
    {
        /// <summary>护盾是否开启（存档持久化）。</summary>
        private bool active;

        // 电池 comp 惰性缓存（服装在背包/充能台内不 spawn，不能靠 PostSpawnSetup 缓存）
        private CompRechargeable battery;

        private CompProperties_ShieldPack Props
        {
            get { return (CompProperties_ShieldPack)props; }
        }

        /// <summary>穿戴者；未被穿戴时为 null。</summary>
        public Pawn Wearer
        {
            get { return (parent as Apparel)?.Wearer; }
        }

        /// <summary>电池 comp；物品上没有 CompRechargeable 时为 null（护盾不可用）。</summary>
        public CompRechargeable Battery
        {
            get
            {
                if (battery == null && parent is ThingWithComps twc)
                    battery = twc.TryGetComp<CompRechargeable>();
                return battery;
            }
        }

        /// <summary>当前电量（Wd）。</summary>
        public float BatteryCharge
        {
            get { return Battery?.ChargeWd ?? 0f; }
        }

        /// <summary>护盾是否生效中：已开启 + 正被穿戴 + 电量充足。</summary>
        public bool ShieldActive
        {
            get { return active && Wearer != null && BatteryCharge > 0f; }
        }

        /// <summary>面板显示用：当前是否开启。</summary>
        public bool IsActive
        {
            get { return active; }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref active, "active", false);
        }

        /// <summary>开启期间持续耗电；电量耗尽或非征召状态自动关闭；被卸下时静默关闭。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (!active)
                return;
            Pawn wearer = Wearer;
            if (wearer == null)
            {
                // 被卸下/装入充能台：静默关闭，重新穿戴后需再次开启
                active = false;
                return;
            }
            if (!wearer.Spawned)
                return;
            if (!wearer.Drafted)
            {
                Deactivate(wearer, null);
                return;
            }
            Battery?.Drain(Props.passiveDrainWdPerDay / 60000f);
            if (BatteryCharge <= 0f)
                Deactivate(wearer, "电量耗尽");
        }

        /// <summary>
        /// 尝试用护盾吸收这次伤害：电量足够则全额吸收返回 true；
        /// 不足则按比例抵消部分伤害、剩余穿透，同时电量归零自动关闭。EMP 直接清空电量不吸收。
        /// </summary>
        public bool TryAbsorb(Pawn pawn, ref DamageInfo dinfo)
        {
            if (!ShieldActive || Wearer != pawn || Battery == null)
                return false;
            if (dinfo.Def == DamageDefOf.EMP)
            {
                Battery.ClearCharge();
                Deactivate(pawn, "EMP 击穿护盾");
                return false;
            }
            float cost = dinfo.Amount * Props.wdPerDamage;
            if (BatteryCharge >= cost)
            {
                Battery.Drain(cost);
                PlayAbsorbFx(pawn);
                if (BatteryCharge <= 0f)
                    Deactivate(pawn, "电量耗尽");
                return true;
            }
            // 电量不足以全额吸收：按比例抵消部分伤害，剩余穿透
            float absorbedDamage = BatteryCharge / Props.wdPerDamage;
            Battery.ClearCharge();
            dinfo.SetAmount(dinfo.Amount - absorbedDamage);
            Deactivate(pawn, "电量耗尽");
            return false;
        }

        /// <summary>gizmo 开关：开启需电量充足；关闭无限制。</summary>
        public void Toggle(Pawn wearer)
        {
            if (active)
            {
                Deactivate(wearer, null);
                return;
            }
            if (BatteryCharge <= 0f)
                return;
            active = true;
            // 开启特效：复用 Royalty 护盾激活闪光与音效（无 Royalty 时静默跳过）
            if (wearer != null && wearer.Map != null)
            {
                if (FleckDefOf.BroadshieldActivation != null)
                    FleckMaker.Static(wearer.TrueCenter(), wearer.Map, FleckDefOf.BroadshieldActivation);
                if (SoundDefOf.EnergyShield_Reset != null)
                    SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
            }
        }

        /// <summary>解除征召时自动关闭（由 set_Drafted 补丁调用，CompTick 兜底）。</summary>
        public void OnUndrafted(Pawn wearer)
        {
            if (active)
                Deactivate(wearer, null);
        }

        /// <summary>关闭护盾；reason 非空时播报中文原因。</summary>
        private void Deactivate(Pawn wearer, string reason)
        {
            active = false;
            if (reason != null && wearer != null)
                Messages.Message(wearer.LabelShort + " 的护盾背包已自动关闭：" + reason,
                    new LookTargets(wearer), MessageTypeDefOf.NegativeEvent, false);
        }

        /// <summary>吸收特效：复用 Royalty 护盾吸收音效（无 Royalty 时静默跳过）。</summary>
        private void PlayAbsorbFx(Pawn pawn)
        {
            if (pawn.Map != null && SoundDefOf.EnergyShield_AbsorbDamage != null)
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
        }

        /// <summary>征召时的开关按钮（图标用背包自身图标，label 带电量%；电量为 0 时禁用并注明原因）。</summary>
        public Command_Action MakeGizmo(Pawn wearer)
        {
            Command_Action cmd = new Command_Action();
            float ratio = Battery?.ChargeRatio ?? 0f;
            cmd.defaultLabel = (active ? "关闭护盾背包" : "开启护盾背包") + "（电量 " + ratio.ToStringPercent("F0") + "）";
            cmd.defaultDesc = "开启后展开贴身能量护盾，全额吸收所有伤害（含近战）：每吸收 1 点伤害耗电 "
                + Props.wdPerDamage.ToString("F0") + " Wd，开启期间持续耗电 "
                + Props.passiveDrainWdPerDay.ToString("F0") + " Wd/天；电能耗尽或解除征召自动关闭。电量可在充能台回充。";
            cmd.icon = parent.def.uiIcon;
            cmd.action = delegate { Toggle(wearer); };
            if (!active && BatteryCharge <= 0f)
                cmd.Disable("电量耗尽，请在充能台充电");
            return cmd;
        }
    }

    /// <summary>
    /// 护盾背包开关补丁（与 ShieldDeployGizmoPatch 同模式）：
    /// 征召 + 玩家殖民者 + 穿戴充能护盾背包时，指令栏追加开关按钮。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class ShieldPackGizmoPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
        {
            foreach (Gizmo gizmo in values)
                yield return gizmo;
            if (!__instance.Drafted || !__instance.IsColonistPlayerControlled)
                yield break;
            if (__instance.apparel == null)
                yield break;
            foreach (Apparel ap in __instance.apparel.WornApparel)
            {
                CompShieldPack comp = ap.TryGetComp<CompShieldPack>();
                if (comp != null)
                    yield return comp.MakeGizmo(__instance);
            }
        }
    }

    /// <summary>
    /// 伤害吸收补丁：护盾生效中拦截 Pawn.PreApplyDamage，
    /// 全额吸收时置 absorbed=true 并跳过原方法；部分吸收时削减伤害量后放行。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class ShieldPackAbsorbPatch
    {
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (__instance.apparel == null)
                return true;
            List<Apparel> worn = __instance.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                CompShieldPack comp = worn[i].TryGetComp<CompShieldPack>();
                if (comp != null && comp.ShieldActive && comp.TryAbsorb(__instance, ref dinfo))
                {
                    absorbed = true;
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>解除征召时自动关闭身上所有开启的护盾背包。</summary>
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public static class ShieldPackUndraftPatch
    {
        public static void Postfix(Pawn_DraftController __instance, bool value)
        {
            if (value)
                return;
            Pawn pawn = __instance.pawn;
            if (pawn?.apparel == null)
                return;
            foreach (Apparel ap in pawn.apparel.WornApparel)
            {
                CompShieldPack comp = ap.TryGetComp<CompShieldPack>();
                if (comp != null)
                    comp.OnUndrafted(pawn);
            }
        }
    }

    /// <summary>
    /// 充能护盾背包服装类：1.7 渲染重构后服装额外绘制走 Apparel.DrawWornExtras，
    /// 护盾开启期间围绕穿戴者绘制常驻半透明能量泡（复用 Core ShieldBubble 贴图，
    /// 1.7 打包在 Assets.zip 内，运行时可直接引用），尺寸随游戏 tick 缓慢脉动。
    /// </summary>
    public class Apparel_ChargeShieldPack : Apparel
    {
        // 护盾气泡材质惰性缓存（自定义着色参数独占一份缓存材质，不污染原版同款材质）
        private Material bubbleMat;

        public override void DrawWornExtras()
        {
            base.DrawWornExtras();
            CompShieldPack comp = this.TryGetComp<CompShieldPack>();
            if (comp == null || !comp.ShieldActive || Wearer == null || !Wearer.Spawned)
                return;
            if (bubbleMat == null)
                bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent,
                    new Color(0.8f, 0.95f, 1f));
            float scale = 1.35f + 0.05f * Mathf.Sin(Find.TickManager.TicksGame * 0.06f);
            Vector3 drawPos = Wearer.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(scale, 1f, scale)),
                bubbleMat, 0);
        }
    }
}
