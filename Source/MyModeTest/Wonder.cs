using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>纪元增益选项（激活三选一）：GameConditionDef + 展示文案。</summary>
    public class WonderEraOption
    {
        /// <summary>落地为本地图永久 GameCondition（durationTicks=-1）。</summary>
        public GameConditionDef condition;
        /// <summary>选项名（如"能源纪元"）。</summary>
        public string label;
        /// <summary>选项说明。</summary>
        public string description;
    }

    /// <summary>奇观阶段推进参数：下一阶段建筑 def。</summary>
    public class CompProperties_WonderStage : CompProperties
    {
        /// <summary>下一阶段建筑（gizmo 点击后本建筑消失，原地放下一阶段蓝图）。</summary>
        public ThingDef nextStage;

        public CompProperties_WonderStage()
        {
            compClass = typeof(CompWonderStage);
        }
    }

    /// <summary>
    /// 奇观二~四阶段的 PlaceWorker：从建造菜单隐藏（IsBuildDesignatorVisible=false），
    /// 只能通过上一阶段建筑的 gizmo 推进。注意 designationCategory 必须保留——
    /// 1.7 蓝图/框架 def 只对 BuildableByPlayer（=designationCategory 非空）的建筑生成，
    /// 缺了它 GenConstruct.PlaceBlueprintForBuild 会因 blueprintDef 为 null 抛 NRE。
    /// </summary>
    public class PlaceWorker_WonderHidden : PlaceWorker
    {
        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            return false;
        }
    }

    /// <summary>
    /// 奇观阶段推进 Comp（前 3 阶段通用）：gizmo"开始下一阶段建设" → 本建筑消失 +
    /// 原地放下一阶段蓝图（蓝图造价逐阶段另付，由建造系统正常收料施工）。
    /// </summary>
    public class CompWonderStage : ThingComp
    {
        private CompProperties_WonderStage Props
        {
            get { return (CompProperties_WonderStage)props; }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            if (Props.nextStage == null)
                yield break;
            Command_Action advance = new Command_Action();
            advance.defaultLabel = "开始下一阶段建设";
            advance.defaultDesc = "拆除本阶段结构，开始下一阶段：" + Props.nextStage.LabelCap
                + "。\n\n下一阶段造价：" + CostSummary(Props.nextStage);
            advance.icon = Props.nextStage.uiIcon;
            advance.action = AdvanceStage;
            yield return advance;
            // 开发者模式：跳过蓝图建造，直接落位下一阶段成品
            if (Prefs.DevMode)
            {
                Command_Action devAdvance = new Command_Action();
                devAdvance.defaultLabel = "DEV: 立即进入下一阶段";
                devAdvance.defaultDesc = "开发者模式：跳过材料与建造，直接生成下一阶段建筑：" + Props.nextStage.LabelCap;
                devAdvance.icon = Props.nextStage.uiIcon;
                devAdvance.action = DevAdvanceStage;
                yield return devAdvance;
            }
        }

        /// <summary>开发者直建：本阶段消失，原地生成下一阶段成品建筑。</summary>
        private void DevAdvanceStage()
        {
            Map map = parent.Map;
            IntVec3 pos = parent.Position;
            Rot4 rot = parent.Rotation;
            ThingDef stuff = parent.Stuff;
            parent.Destroy(DestroyMode.Vanish);
            Thing next = ThingMaker.MakeThing(Props.nextStage, stuff);
            next.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(next, pos, map, rot);
            Messages.Message("DEV: 已直接生成 " + Props.nextStage.LabelCap,
                new LookTargets(next), MessageTypeDefOf.NeutralEvent, false);
        }

        /// <summary>造价摘要（菜单文案）。</summary>
        private static string CostSummary(BuildableDef def)
        {
            if (def.CostList == null || def.CostList.Count == 0)
                return "无";
            string text = "";
            foreach (ThingDefCountClass cost in def.CostList)
            {
                if (text.Length > 0)
                    text += "、";
                text += cost.thingDef.label + "×" + cost.count;
            }
            return text;
        }

        private void AdvanceStage()
        {
            Map map = parent.Map;
            IntVec3 pos = parent.Position;
            Rot4 rot = parent.Rotation;
            ThingDef stuff = parent.Stuff;
            // 流程结束：本阶段消失 -> 下一阶段蓝图落位（材料由建造系统收取）
            parent.Destroy(DestroyMode.Vanish);
            GenConstruct.PlaceBlueprintForBuild(Props.nextStage, pos, map, rot, Faction.OfPlayer, stuff, null, null, false);
            Messages.Message("下一阶段建设已开始：" + Props.nextStage.LabelCap,
                new LookTargets(new TargetInfo(pos, map)), MessageTypeDefOf.PositiveEvent, false);
        }
    }

    /// <summary>纪元奇点激活参数。</summary>
    public class CompProperties_WonderActivation : CompProperties
    {
        /// <summary>点火时长（tick，默认 5000 ≈ 2 小时）。</summary>
        public int activationTicks = 5000;

        /// <summary>激活所需蓄电（Wd）。</summary>
        public float requiredChargeWd = 50000f;

        /// <summary>三选一纪元增益。</summary>
        public List<WonderEraOption> eras;

        public CompProperties_WonderActivation()
        {
            compClass = typeof(CompWonderActivation);
        }
    }

    /// <summary>
    /// 纪元奇点激活 Comp（终态建筑）：反物质燃料满 + 蓄电满 → gizmo"启动奇点" →
    /// 点火倒计时（光柱特效由 Building_Singularity 绘制）→ 三选一弹窗 → 本地图永久 GameCondition。
    /// 每张图各建各的奇观（增益不跨图共享）。激活为一次性，完成后不再消耗燃料/电力。
    /// </summary>
    public class CompWonderActivation : ThingComp
    {
        /// <summary>点火剩余 tick（-1 = 未点火）。</summary>
        private int activationTicksLeft = -1;

        /// <summary>已激活（一次性）。</summary>
        private bool activated;

        private CompRefuelable refuelComp;
        private CompBuildingBattery batteryComp;

        private CompProperties_WonderActivation Props
        {
            get { return (CompProperties_WonderActivation)props; }
        }

        /// <summary>点火中（光柱特效开关）。</summary>
        public bool IsActivating
        {
            get { return activationTicksLeft >= 0; }
        }

        /// <summary>点火进度（0-1，特效用）。</summary>
        public float ActivationProgress
        {
            get { return activationTicksLeft < 0 ? 0f : 1f - (float)activationTicksLeft / Props.activationTicks; }
        }

        /// <summary>已激活。</summary>
        public bool Activated
        {
            get { return activated; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelComp = parent.GetComp<CompRefuelable>();
            batteryComp = parent.GetComp<CompBuildingBattery>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref activationTicksLeft, "wonderActivationTicksLeft", -1);
            Scribe_Values.Look(ref activated, "wonderActivated", false);
        }

        /// <summary>点火推进（CompTickInterval：1.7 Normal 建筑收不到 CompTickRare）。</summary>
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            if (!IsActivating)
                return;
            activationTicksLeft -= delta;
            if (activationTicksLeft < 0)
            {
                activationTicksLeft = -1;
                ShowEraChoice();
            }
        }

        /// <summary>启动条件：反物质燃料满 + 蓄电满。</summary>
        private string CanActivate()
        {
            if (activated)
                return "已激活";
            if (IsActivating)
                return "点火中";
            if (refuelComp == null || !refuelComp.IsFull)
                return "反物质燃料未满";
            if (batteryComp == null || batteryComp.StoredChargeWd < Props.requiredChargeWd)
                return "蓄电未满（需 " + Props.requiredChargeWd.ToString("F0") + " Wd）";
            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            if (activated)
                yield break;
            Command_Action activate = new Command_Action();
            activate.defaultLabel = IsActivating ? "点火中…" : "启动奇点";
            activate.defaultDesc = "注入全部反物质燃料与 50000 Wd 蓄电，点燃纪元奇点。点火完成后选择本图永久增益。\n\n警告：反物质极易爆炸，加注与存储过程请远离其它设施。";
            activate.icon = parent.def.uiIcon;
            string reason = CanActivate();
            if (reason != null)
                activate.Disable(reason);
            else
                activate.action = delegate
                {
                    activationTicksLeft = Props.activationTicks;
                    Messages.Message("纪元奇点点火程序启动", new LookTargets(parent), MessageTypeDefOf.PositiveEvent, false);
                };
            yield return activate;
        }

        /// <summary>点火完成：三选一弹窗。</summary>
        private void ShowEraChoice()
        {
            if (Props.eras == null || Props.eras.Count == 0)
                return;
            string text = "纪元奇点已点燃。时空在奇点周围泛起涟漪——选择一个纪元，本张地图将获得对应的永久增益：\n\n";
            foreach (WonderEraOption era in Props.eras)
                text += "【" + era.label + "】" + era.description + "\n\n";
            // 1.7 构造序：(text, buttonAText, buttonAAction, buttonBText, buttonBAction, title)
            Dialog_MessageBox dlg = new Dialog_MessageBox(text,
                Props.eras[0].label, delegate { ApplyEra(Props.eras[0]); },
                Props.eras.Count > 1 ? Props.eras[1].label : null,
                Props.eras.Count > 1 ? (System.Action)(() => ApplyEra(Props.eras[1])) : null,
                "纪元抉择");
            if (Props.eras.Count > 2)
            {
                dlg.buttonCText = Props.eras[2].label;
                dlg.buttonCAction = delegate { ApplyEra(Props.eras[2]); };
                dlg.buttonCClose = true;
            }
            Find.WindowStack.Add(dlg);
        }

        /// <summary>落地纪元增益：本地图永久 GameCondition。</summary>
        private void ApplyEra(WonderEraOption era)
        {
            activated = true;
            GameCondition cond = GameConditionMaker.MakeCondition(era.condition, -1);
            parent.Map.gameConditionManager.RegisterCondition(cond);
            Find.LetterStack.ReceiveLetter(era.label + "降临",
                "纪元奇点已稳定运行。" + era.label + "的力量将永远笼罩这张地图：" + era.description,
                LetterDefOf.PositiveEvent, new LookTargets(parent));
        }

        public override string CompInspectStringExtra()
        {
            if (activated)
                return "状态: 纪元已降临";
            if (IsActivating)
                return "点火中: " + ActivationProgress.ToStringPercent("F0");
            return null;
        }
    }

    /// <summary>
    /// 纪元奇点建筑绘制：底座为建筑本体贴图，上方实时叠画——三圈稳定环互逆旋转、
    /// 中央黑洞缓慢旋转+呼吸缩放、周期性黑洞脉冲扩散环；点火时再加水平发光碟堆叠光柱
    /// （1.7 竖直 quad 不渲染，严禁立起 plane10）。全部用 plane10 水平面片，Y 轴分层。
    /// </summary>
    public class Building_Singularity : Building
    {
        private CompWonderActivation activationComp;
        private Material holeMat;
        private Material pulseMat;
        private Material ringOuterMat;
        private Material ringMidMat;
        private Material ringInnerMat;
        private Material glowMat;

        private const string TexDir = "Things/Building/Production/MMT_Singularity/";
        private const string HoleTexPath = TexDir + "MMT_SingularityHole";
        private const string PulseTexPath = TexDir + "MMT_SingularityPulse";
        private const string RingOuterTexPath = TexDir + "MMT_SingularityRingOuter";
        private const string RingMidTexPath = TexDir + "MMT_SingularityRingMid";
        private const string RingInnerTexPath = TexDir + "MMT_SingularityRingInner";
        private const string GlowTexPath = TexDir + "MMT_SingularityGlow";

        // plane10 缩放（片元 10×10 格）：圆环整体 = 底座宽的 0.68，黑洞 = 0.46
        private const float RingScale = 0.435f;
        private const float HoleScale = 0.294f;

        // 圆环转速（度/秒，负号反向）
        private const float RingOuterSpeed = 5f;
        private const float RingMidSpeed = -9f;
        private const float RingInnerSpeed = 14f;

        // 黑洞：缓慢自转 + 呼吸缩放
        private const float HoleSpinSpeed = 3f;
        private const float HoleBreathe = 0.04f;

        // 脉冲扩散环：从黑洞扩散到外环之外
        private const float PulsePeriod = 2.8f;
        private const float PulsePeriodActivating = 1.3f;
        private const float PulseScaleFrom = 0.18f;
        private const float PulseScaleTo = 0.72f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            activationComp = this.TryGetComp<CompWonderActivation>();
        }

        private static Material LoadMat(ref Material mat, string texPath)
        {
            if (mat == null)
                mat = MaterialPool.MatFrom(texPath, ShaderDatabase.Transparent, Color.white);
            return mat;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            float time = Time.realtimeSinceStartup;
            bool activating = activationComp != null && activationComp.IsActivating;
            bool activated = activationComp != null && activationComp.Activated;
            float baseY = AltitudeLayer.Item.AltitudeFor();

            // 三圈稳定环互逆旋转
            DrawPlane(drawLoc, baseY + 0.10f, RingScale, time * RingOuterSpeed,
                LoadMat(ref ringOuterMat, RingOuterTexPath), Color.white);
            DrawPlane(drawLoc, baseY + 0.11f, RingScale, time * RingMidSpeed,
                LoadMat(ref ringMidMat, RingMidTexPath), Color.white);
            DrawPlane(drawLoc, baseY + 0.12f, RingScale, time * RingInnerSpeed,
                LoadMat(ref ringInnerMat, RingInnerTexPath), Color.white);

            // 中央黑洞：慢速自转 + 呼吸缩放（点火时呼吸加剧）
            float breathe = (activating ? HoleBreathe * 2.5f : HoleBreathe) * Mathf.Sin(time * 2.2f);
            DrawPlane(drawLoc, baseY + 0.14f, HoleScale * (1f + breathe), time * HoleSpinSpeed,
                LoadMat(ref holeMat, HoleTexPath), Color.white);

            // 黑洞脉冲：周期性扩散环，点火时更频繁更亮
            float period = activating ? PulsePeriodActivating : PulsePeriod;
            float t = (time % period) / period;
            float pulseScale = Mathf.Lerp(PulseScaleFrom, PulseScaleTo, t);
            float pulseAlpha = (1f - t) * (activating ? 0.9f : 0.55f);
            DrawPlane(drawLoc, baseY + 0.16f, pulseScale, 0f,
                LoadMat(ref pulseMat, PulseTexPath), new Color(1f, 1f, 1f, pulseAlpha));

            // 点火/常驻辉光：6 片水平发光碟从台面堆叠到空中（俯视投影成光柱）
            if (activating || activated)
            {
                Material glow = LoadMat(ref glowMat, GlowTexPath);
                float strength = activated ? 0.18f : 0.35f + activationComp.ActivationProgress * 0.5f;
                float pulse = strength + 0.15f * Mathf.Sin(time * 4.8f);
                glow.color = new Color(0.7f, 0.5f, 1f, pulse);
                float spin = time * 4.2f;
                for (int i = 0; i < 6; i++)
                {
                    float k = i / 5f;
                    float s = (2.2f - k * 0.9f) * (1f + 0.15f * Mathf.Sin(spin + i * 1.1f));
                    Vector3 p = new Vector3(drawLoc.x, baseY + 0.08f + i * 0.22f, drawLoc.z);
                    Graphics.DrawMesh(MeshPool.plane10,
                        Matrix4x4.TRS(p, Quaternion.identity, new Vector3(s, 1f, s)), glow, 0);
                }
            }
        }

        /// <summary>水平面片叠画：绕 Y 轴旋转（俯视即原地自转）。</summary>
        private static void DrawPlane(Vector3 center, float y, float scale, float angleDeg, Material mat, Color color)
        {
            mat.color = color;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(new Vector3(center.x, y, center.z),
                    Quaternion.AngleAxis(angleDeg, Vector3.up),
                    new Vector3(scale, 1f, scale)), mat, 0);
        }
    }

    /// <summary>能源纪元：全图发电建筑输出 ×1.25（GameCondition 判活）。</summary>
    public static class WonderPowerBoost
    {
        /// <summary>本图能源纪元是否激活。</summary>
        public static bool ActiveOn(Map map)
        {
            return map != null && map.gameConditionManager.ConditionIsActive(MMT_WonderDefOf.MMT_Condition_SingularityPower);
        }
    }

    /// <summary>发电输出补丁：常规电厂（CompPowerPlant 基类实现，覆盖地热/核电/燃料等）。</summary>
    [HarmonyPatch(typeof(CompPowerPlant), "get_DesiredPowerOutput")]
    public static class Patch_WonderPowerBase
    {
        public static void Postfix(CompPowerPlant __instance, ref float __result)
        {
            if (__result > 0f && WonderPowerBoost.ActiveOn(__instance.parent?.Map))
                __result *= 1.25f;
        }
    }

    /// <summary>发电输出补丁：太阳能（覆盖重写）。风暴归零也在这里。</summary>
    [HarmonyPatch(typeof(CompPowerPlantSolar), "get_DesiredPowerOutput")]
    public static class Patch_WonderPowerSolar
    {
        public static void Postfix(CompPowerPlantSolar __instance, ref float __result)
        {
            Map map = __instance.parent?.Map;
            if (map != null && map.gameConditionManager.ConditionIsActive(MMT_WonderDefOf.MMT_EnergyStorm))
                __result = 0f;
            else if (__result > 0f && WonderPowerBoost.ActiveOn(map))
                __result *= 1.25f;
        }
    }

    /// <summary>发电输出补丁：风力（覆盖重写）。风暴归零也在这里。</summary>
    [HarmonyPatch(typeof(CompPowerPlantWind), "get_DesiredPowerOutput")]
    public static class Patch_WonderPowerWind
    {
        public static void Postfix(CompPowerPlantWind __instance, ref float __result)
        {
            Map map = __instance.parent?.Map;
            if (map != null && map.gameConditionManager.ConditionIsActive(MMT_WonderDefOf.MMT_EnergyStorm))
                __result = 0f;
            else if (__result > 0f && WonderPowerBoost.ActiveOn(map))
                __result *= 1.25f;
        }
    }

    /// <summary>丰饶纪元：植物生长速度 ×1.5。</summary>
    [HarmonyPatch(typeof(Plant), "get_GrowthRate")]
    public static class Patch_WonderHarvest
    {
        public static void Postfix(Plant __instance, ref float __result)
        {
            if (__result > 0f && __instance.Map != null
                && __instance.Map.gameConditionManager.ConditionIsActive(MMT_WonderDefOf.MMT_Condition_SingularityHarvest))
                __result *= 1.5f;
        }
    }

    /// <summary>
    /// 启迪纪元 GameCondition：每 2500 tick 给图上自由殖民者刷新启迪 Hediff
    /// （研究 +25%、全局工作速度 +10%；hediff 2600 tick 自然消失，随条件结束而消退）。
    /// </summary>
    public class GameCondition_SingularityMind : GameCondition
    {
        /// <summary>刷新倒计时（条件 tick 每拍调用，间隔 ≥2500 符合性能原则）。</summary>
        private int ticksToRefresh;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToRefresh, "mindTicksToRefresh", 0);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            ticksToRefresh--;
            if (ticksToRefresh > 0)
                return;
            ticksToRefresh = 2500;
            HediffDef insight = MMT_WonderDefOf.MMT_Hediff_SingularityInsight;
            Map map = SingleMap;
            if (insight == null || map == null)
                return;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                Hediff old = pawn.health.hediffSet.GetFirstHediffOfDef(insight);
                if (old != null)
                    pawn.health.RemoveHediff(old);
                pawn.health.AddHediff(insight);
            }
        }
    }

    [DefOf]
    public static class MMT_WonderDefOf
    {
        /// <summary>能源纪元条件。</summary>
        public static GameConditionDef MMT_Condition_SingularityPower;
        /// <summary>丰饶纪元条件。</summary>
        public static GameConditionDef MMT_Condition_SingularityHarvest;
        /// <summary>能量风暴条件（发电补丁共用判活）。</summary>
        public static GameConditionDef MMT_EnergyStorm;
        /// <summary>启迪 hediff。</summary>
        public static HediffDef MMT_Hediff_SingularityInsight;

        static MMT_WonderDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_WonderDefOf));
        }
    }
}
