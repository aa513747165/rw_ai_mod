using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 机械集控核心参数（参考已授权的 Gestalt Engine / Workshop 3392334612 重实现，仅原版+Harmony）。
    /// 带宽/控制组数值 = 基础值 + 已插模块效果（模块效果 key：MechBandwidth / MechControlGroup，
    /// 槽位上限由通用 CompProperties_ModuleSlots 配置）。
    /// </summary>
    public class CompProperties_MechControlHub : CompProperties
    {
        /// <summary>基础机械带宽（不插模块时）。</summary>
        public int baseBandwidth = 20;
        /// <summary>基础控制组数量。</summary>
        public int baseControlGroups = 1;
        /// <summary>每个已插模块增加的耗电（W）。</summary>
        public int powerPerModule = 250;

        public CompProperties_MechControlHub()
        {
            compClass = typeof(CompMechControlHub);
        }
    }

    /// <summary>
    /// 机械集控核心 Comp：建筑内藏一个不可见的"虚拟机械师"假人 Pawn（Mechanitor_Basic，从不 spawn），
    /// 通过原版 Overseer 直接关系接管机械体——征召、工作模式、控制组等全部走原版逻辑。
    /// Harmony 补丁（本文件末尾）把假人的带宽/控制组/可控性/指挥范围挂到本建筑状态上。
    /// 假人 -> 建筑的反查走静态字典（读档后由 PostSpawnSetup 重建；开局/读档时清空，见 Patch_MechControlHub_ResetStatics）。
    /// </summary>
    public class CompMechControlHub : ThingComp
    {
        /// <summary>全部已 spawn 的集控核心（跨地图）。</summary>
        public static readonly HashSet<CompMechControlHub> All = new HashSet<CompMechControlHub>();

        /// <summary>假人 Pawn -> 所属核心反查表（补丁命中判定用）。</summary>
        private static readonly Dictionary<Pawn, CompMechControlHub> ByDummy = new Dictionary<Pawn, CompMechControlHub>();

        /// <summary>1.7 中控制组数量变化通知为私有方法，反射调用。</summary>
        private static readonly MethodInfo NotifyControlGroupsMethod =
            AccessTools.Method(typeof(Pawn_MechanitorTracker), "Notify_ControlGroupAmountMayChanged");

        /// <summary>通知虚拟机械师控制组数量可能变化（反射，1.7 该方法是私有的）。</summary>
        private void NotifyControlGroups()
        {
            if (dummyPawn?.mechanitor != null)
            {
                NotifyControlGroupsMethod?.Invoke(dummyPawn.mechanitor, null);
            }
        }

        /// <summary>骇入敌方机械体的冷却（5 天）。</summary>
        private const int HackCooldownTicks = GenDate.TicksPerDay * 5;

        /// <summary>虚拟机械师假人（Scribe_Deep 持久化；机械体的 Overseer 关系指向它）。</summary>
        public Pawn dummyPawn;

        private Effecter connectMechEffecter;
        private Effecter connectProgressBarEffecter;
        private int connectTick = -1;
        private LocalTargetInfo curTarget = LocalTargetInfo.Invalid;
        private int hackCooldownTicks;

        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        private CompModuleSlots slotsComp;
        private int lastBandwidth = -1;
        private int lastControlGroups = -1;

        // 模块数量可视化叠层（0=T0，1~2=T1，3~4=T2，5~6=T3，7=T4），懒加载缓存
        private static Graphic[] tierGraphics;

        private CompProperties_MechControlHub Props
        {
            get { return (CompProperties_MechControlHub)props; }
        }

        /// <summary>核心是否在工作（通电且未手动关闭）。</summary>
        public bool PoweredOn
        {
            get
            {
                if (powerComp == null || !powerComp.PowerOn)
                {
                    return false;
                }
                return flickComp == null || flickComp.SwitchIsOn;
            }
        }

        /// <summary>当前机械带宽：断电/关停为 0，否则基础值 + 带宽模块加成。</summary>
        public int CurrentBandwidth
        {
            get
            {
                if (!PoweredOn)
                {
                    return 0;
                }
                return Props.baseBandwidth + (slotsComp != null ? (int)slotsComp.GetEffectSum("MechBandwidth") : 0);
            }
        }

        /// <summary>当前控制组数量：基础值 + 控制组模块加成。</summary>
        public int CurrentControlGroups
        {
            get { return Props.baseControlGroups + (slotsComp != null ? (int)slotsComp.GetEffectSum("MechControlGroup") : 0); }
        }

        /// <summary>核心可执行连接/骇入（工作中且有带宽）。</summary>
        private bool MechanitorActive
        {
            get { return PoweredOn && CurrentBandwidth > 0; }
        }

        /// <summary>该 Pawn 是否为某台集控核心的虚拟机械师假人（补丁命中判定）。</summary>
        public static bool TryGetHub(Pawn overseer, out CompMechControlHub result)
        {
            if (overseer != null)
            {
                return ByDummy.TryGetValue(overseer, out result);
            }
            result = null;
            return false;
        }

        /// <summary>清空静态注册表（开局/读档时由补丁调用，防止旧引用泄漏）。</summary>
        public static void ResetStatics()
        {
            All.Clear();
            ByDummy.Clear();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            All.Add(this);
            powerComp = parent.GetComp<CompPowerTrader>();
            flickComp = parent.GetComp<CompFlickable>();
            slotsComp = parent.GetComp<CompModuleSlots>();
            if (dummyPawn == null)
            {
                // 生成虚拟机械师：普通人类 Pawn 对象，从不 spawn，阵营跟随建筑
                dummyPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Mechanitor_Basic, Faction.OfAncients);
                dummyPawn.SetFactionDirect(parent.Faction);
                dummyPawn.Name = new NameSingle(parent.LabelCap);
                PawnComponentsUtility.AddComponentsForSpawn(dummyPawn);
                PawnComponentsUtility.AddAndRemoveDynamicComponents(dummyPawn);
                dummyPawn.gender = Gender.None;
                dummyPawn.equipment.DestroyAllEquipment();
            }
            ByDummy[dummyPawn] = this;
            if (dummyPawn.mechanitor != null)
            {
                dummyPawn.mechanitor.Notify_BandwidthChanged();
                NotifyControlGroups();
            }
            lastBandwidth = CurrentBandwidth;
            lastControlGroups = CurrentControlGroups;
            UpdatePowerOutput();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            All.Remove(this);
            if (dummyPawn != null)
            {
                ByDummy.Remove(dummyPawn);
            }
            base.PostDeSpawn(map, mode);
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff")
            {
                dummyPawn?.mechanitor?.Notify_BandwidthChanged();
            }
            else if (signal == CompModuleSlots.SignalModulesChanged)
            {
                // 模块插拔即时同步：带宽/控制组数量（原版按此物化控制组对象，少了就无法分组）/耗电
                RefreshMechanitorState();
            }
        }

        /// <summary>带宽/控制组数量变化时同步原版机械师状态并刷新耗电。</summary>
        private void RefreshMechanitorState()
        {
            lastBandwidth = CurrentBandwidth;
            lastControlGroups = CurrentControlGroups;
            if (dummyPawn?.mechanitor != null)
            {
                dummyPawn.mechanitor.Notify_BandwidthChanged();
                NotifyControlGroups();
            }
            UpdatePowerOutput();
        }

        /// <summary>已插模块总数（带宽 + 控制组）。</summary>
        public int TotalModuleCount()
        {
            int count = 0;
            if (slotsComp != null)
            {
                foreach (KeyValuePair<ThingDef, int> pair in slotsComp.InstalledCountsByDef())
                {
                    count += pair.Value;
                }
            }
            return count;
        }

        /// <summary>按模块数量刷新实际耗电（基础 500W + 每模块 250W）。</summary>
        private void UpdatePowerOutput()
        {
            if (powerComp == null)
            {
                return;
            }
            float baseConsumption = ((CompProperties_Power)powerComp.props).PowerConsumption;
            powerComp.PowerOutput = -(baseConsumption + Props.powerPerModule * TotalModuleCount());
        }

        /// <summary>模块数量叠层：0=T0，1~2=T1，3~4=T2，5~6=T3，7=T4。</summary>
        private int TierIndex()
        {
            int count = TotalModuleCount();
            return count <= 0 ? 0 : (count - 1) / 2 + 1;
        }

        private static Graphic TierGraphic(int tier)
        {
            if (tierGraphics == null)
            {
                tierGraphics = new Graphic[5];
            }
            if (tierGraphics[tier] == null)
            {
                tierGraphics[tier] = GraphicDatabase.Get<Graphic_Single>(
                    "Things/Building/Production/MMT_MechControlHub/MMT_MechControlHub_Tier" + tier,
                    ShaderTypeDefOf.Cutout.Shader, new Vector2(11f, 11f), Color.white);
            }
            return tierGraphics[tier];
        }

        public override void PostDraw()
        {
            base.PostDraw();
            // 略微抬高避免与建筑本体贴图 z-fighting
            TierGraphic(TierIndex()).Draw(parent.DrawPos + new Vector3(0f, 2f * Altitudes.AltInc, 0f), Rot4.North, parent);
        }

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            // 手动开关等无信号路径的低频轮询兜底（1.7 Normal 物体收不到 CompTickRare，
            // 必须用 CompTickInterval；模块插拔走 SignalModulesChanged 信号即时刷新，不靠这里）
            if (!parent.IsHashIntervalTick(250, delta))
            {
                return;
            }
            int bandwidth = CurrentBandwidth;
            if (bandwidth != lastBandwidth)
            {
                lastBandwidth = bandwidth;
                dummyPawn?.mechanitor?.Notify_BandwidthChanged();
            }
            int groups = CurrentControlGroups;
            if (groups != lastControlGroups)
            {
                lastControlGroups = groups;
                NotifyControlGroups();
            }
            UpdatePowerOutput();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (dummyPawn == null)
            {
                return;
            }
            if (dummyPawn.Faction != parent.Faction)
            {
                dummyPawn.SetFaction(parent.Faction);
            }
            // 推进进行中的连接/骇入
            if (curTarget.IsValid && connectTick != -1)
            {
                Pawn mech = curTarget.Pawn;
                if (mech == null || (mech.Faction == dummyPawn.Faction && !CanConnect(mech).Accepted)
                    || (mech.Faction != dummyPawn.Faction && !CanConnectNonColony(mech).Accepted)
                    || !MechanitorActive)
                {
                    ResetConnect();
                }
                else
                {
                    TickConnectEffects(mech);
                    if (Find.TickManager.TicksGame >= connectTick)
                    {
                        FinishConnect();
                    }
                }
            }
            // 频段节点调谐到假人时，定期刷新其缓存的节点带宽
            if (dummyPawn.IsHashIntervalTick(60) && dummyPawn.health != null)
            {
                foreach (Hediff_BandNode hediff in dummyPawn.health.hediffSet.hediffs.OfType<Hediff_BandNode>())
                {
                    hediff.RecacheBandNodes();
                }
            }
        }

        // ======================== 连接 / 骇入 ========================

        /// <summary>机械体的接管耗时（tick），与原版机械师一致。</summary>
        private static int MechControlTime(Pawn mech)
        {
            return Mathf.RoundToInt(mech.GetStatValue(StatDefOf.ControlTakingTime) * 60f);
        }

        private static bool HasEnoughBandwidth(Pawn pawn, Pawn mech)
        {
            float free = pawn.mechanitor.TotalBandwidth - pawn.mechanitor.UsedBandwidth;
            return free >= mech.GetStatValue(StatDefOf.BandwidthCost);
        }

        /// <summary>本殖民地机械体是否可被本核心接管（对齐原版机械师校验）。</summary>
        private AcceptanceReport CanConnect(Pawn mech)
        {
            if (dummyPawn.mechanitor == null || !mech.IsColonyMech || mech.Downed || mech.Dead || mech.IsAttacking())
            {
                return false;
            }
            if (!MechanitorUtility.EverControllable(mech))
            {
                return "CannotControlMechNeverControllable".Translate();
            }
            if (mech.GetOverseer() == dummyPawn)
            {
                return "CannotControlMechAlreadyControlled".Translate(dummyPawn.LabelShort);
            }
            return true;
        }

        /// <summary>非本殖民地机械体是否可被本核心骇入接管。</summary>
        private AcceptanceReport CanConnectNonColony(Pawn mech)
        {
            if (dummyPawn.mechanitor == null || mech.Downed || mech.Dead)
            {
                return false;
            }
            if (!MechanitorUtility.EverControllable(mech))
            {
                return "CannotControlMechNeverControllable".Translate();
            }
            if (mech.GetOverseer() == dummyPawn)
            {
                return "CannotControlMechAlreadyControlled".Translate(dummyPawn.LabelShort);
            }
            return true;
        }

        private void StartConnect(LocalTargetInfo target)
        {
            curTarget = target;
            connectTick = Find.TickManager.TicksGame + MechControlTime(target.Pawn);
            PawnUtility.ForceWait(target.Pawn, MechControlTime(target.Pawn), parent, true);
            if (!HasEnoughBandwidth(dummyPawn, target.Pawn))
            {
                Messages.Message("机械带宽不足，接管后该机械体将处于失控风险中。", target.Pawn, MessageTypeDefOf.CautionInput, false);
            }
        }

        private void StartHack(LocalTargetInfo target)
        {
            curTarget = target;
            // 骇入耗时翻倍
            connectTick = Find.TickManager.TicksGame + MechControlTime(target.Pawn) * 2;
        }

        /// <summary>完成接管：转移 Overseer 关系到虚拟机械师；骇入 additionally 转变阵营并进入冷却。</summary>
        private void FinishConnect()
        {
            LocalTargetInfo target = curTarget;
            ResetConnect();
            Pawn mech = target.Pawn;
            if (mech == null || mech.Dead)
            {
                return;
            }
            if (mech.Faction != dummyPawn.Faction)
            {
                mech.SetFaction(dummyPawn.Faction);
                hackCooldownTicks = Find.TickManager.TicksGame + HackCooldownTicks;
            }
            mech.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, mech);
            dummyPawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
            Messages.Message("已接管 " + mech.LabelCap + "。", mech, MessageTypeDefOf.PositiveEvent, false);
        }

        private void ResetConnect()
        {
            connectProgressBarEffecter?.Cleanup();
            connectProgressBarEffecter = null;
            connectMechEffecter?.Cleanup();
            connectMechEffecter = null;
            if (curTarget.Thing is Pawn pawn && !pawn.Dead)
            {
                pawn.jobs.StopAll();
                pawn.pather.StopDead();
            }
            curTarget = LocalTargetInfo.Invalid;
            connectTick = -1;
        }

        /// <summary>连接中的进度条与链接特效（复用原版 EffecterDefOf.ControlMech）。</summary>
        private void TickConnectEffects(Pawn mech)
        {
            if (connectProgressBarEffecter == null)
            {
                connectProgressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
            }
            connectProgressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
            MoteProgressBar mote = ((SubEffecter_ProgressBar)connectProgressBarEffecter.children[0]).mote;
            float total = mech.Faction != parent.Faction ? MechControlTime(mech) * 2f : MechControlTime(mech);
            mote.progress = 1f - (connectTick - Find.TickManager.TicksGame) / total;
            mote.offsetZ = -0.8f;
            if (connectMechEffecter == null)
            {
                connectMechEffecter = EffecterDefOf.ControlMech.Spawn();
                connectMechEffecter.Trigger(parent, mech);
            }
            connectMechEffecter.EffectTick(parent, mech);
        }

        // ======================== 指令面板 ========================

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (parent.Faction != Faction.OfPlayer || dummyPawn?.mechanitor == null)
            {
                yield break;
            }
            // 转发虚拟机械师的原版指令（控制组分配等；剔除召唤 Boss 组）
            foreach (Gizmo gizmo in dummyPawn.mechanitor.GetGizmos())
            {
                if (gizmo is Command_CallBossgroup)
                {
                    continue;
                }
                yield return gizmo;
            }
            Command_Action connect = new Command_Action
            {
                defaultLabel = "连接机械体",
                defaultDesc = "把一台本殖民地的机械体移交给集控核心统一管控（占用机械带宽）。",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/MMT_ConnectMechanoid"),
                action = delegate
                {
                    Find.Targeter.BeginTargeting(ConnectTargetParameters(false), StartConnect, Highlight, CanTargetConnect);
                }
            };
            Command_Action hack = new Command_Action
            {
                defaultLabel = "骇入机械体",
                defaultDesc = "远程骇入一台非本殖民地的机械体，将其接管到集控核心麾下（耗时更长，每次骇入后需冷却 5 天）。",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/MMT_HackMechanoid"),
                action = delegate
                {
                    Find.Targeter.BeginTargeting(ConnectTargetParameters(true), StartHack, Highlight, CanTargetHack);
                }
            };
            if (curTarget.IsValid)
            {
                connect.Disable("正在接管一台机械体。");
                hack.Disable("正在接管一台机械体。");
            }
            else if (!MechanitorActive)
            {
                connect.Disable("集控核心未通电、已关闭或无可用带宽。");
                hack.Disable("集控核心未通电、已关闭或无可用带宽。");
            }
            if (!hack.Disabled && hackCooldownTicks > Find.TickManager.TicksGame)
            {
                hack.Disable("骇入冷却中：" + (hackCooldownTicks - Find.TickManager.TicksGame).ToStringTicksToPeriod());
            }
            yield return connect;
            yield return hack;
            // 开发者模式：随机生成一台可控机械体并直接接入本核心（测试接管/带宽用）
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: 随机增加一台机械体",
                    defaultDesc = "开发者模式：在建筑附近随机生成一台可控机械体，并直接接入集控核心。",
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/MMT_ConnectMechanoid"),
                    action = DevSpawnRandomMech
                };
            }
        }

        /// <summary>DEV：随机生成一台原版可控机械体（带宽占用 > 0 的机械族，仅限官方内容包），落位建筑旁并接入本核心。</summary>
        private void DevSpawnRandomMech()
        {
            List<PawnKindDef> kinds = DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(k => k.RaceProps != null && k.RaceProps.IsMechanoid && k.race != null
                    && k.modContentPack != null && k.modContentPack.IsOfficialMod
                    && k.race.GetStatValueAbstract(StatDefOf.BandwidthCost) > 0f)
                .ToList();
            if (kinds.Count == 0 || parent.Map == null)
            {
                return;
            }
            Pawn mech = PawnGenerator.GeneratePawn(kinds.RandomElement(), Faction.OfPlayer);
            IntVec3 cell = CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 15);
            GenSpawn.Spawn(mech, cell, parent.Map);
            dummyPawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
            dummyPawn.mechanitor?.Notify_BandwidthChanged();
            Messages.Message("DEV: 已生成并接管 " + mech.LabelCap, mech, MessageTypeDefOf.NeutralEvent, false);
        }

        private TargetingParameters ConnectTargetParameters(bool nonColony)
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetHumans = false,
                canTargetMechs = true,
                canTargetAnimals = false,
                canTargetLocations = false,
                validator = nonColony
                    ? (Predicate<TargetInfo>)(x => CanTargetHack((LocalTargetInfo)x))
                    : (x => CanTargetConnect((LocalTargetInfo)x))
            };
        }

        private bool CanTargetConnect(LocalTargetInfo target)
        {
            Pawn mech = target.Pawn;
            return mech != null && CanConnect(mech).Accepted;
        }

        private bool CanTargetHack(LocalTargetInfo target)
        {
            Pawn mech = target.Pawn;
            return mech != null && mech.Faction != parent.Faction && CanConnectNonColony(mech).Accepted
                && HasEnoughBandwidth(dummyPawn, mech);
        }

        private static void Highlight(LocalTargetInfo target)
        {
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
            }
        }

        // ======================== 检查面板 / 存档 ========================

        public override string CompInspectStringExtra()
        {
            string text = "机械带宽: " + dummyPawn?.mechanitor?.UsedBandwidth + " / " + CurrentBandwidth
                + "\n控制组: " + CurrentControlGroups;
            if (!PoweredOn)
            {
                text += "\n未通电或已关闭：带宽归零";
            }
            return text;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref dummyPawn, "dummyPawn");
            Scribe_TargetInfo.Look(ref curTarget, "MMT_mechControl_curTarget", LocalTargetInfo.Invalid);
            Scribe_Values.Look(ref connectTick, "MMT_mechControl_connectTick", -1);
            Scribe_Values.Look(ref hackCooldownTicks, "MMT_mechControl_hackCooldownTicks");
        }
    }

    /// <summary>机械集控核心的静态扩展：判定 Pawn 是否为某核心的虚拟机械师假人。</summary>
    public static class MechControlHubExt
    {
        public static bool TryGetMechControlHub(this Pawn overseer, out CompMechControlHub hub)
        {
            return CompMechControlHub.TryGetHub(overseer, out hub);
        }
    }

    // ======================== Harmony 补丁：把虚拟机械师挂到建筑状态上 ========================

    /// <summary>
    /// 集控核心带宽/控制组 StatPart（注入 MechBandwidth / MechControlGroups 属性计算，见 Patches/MMT_Patch_MechControlStats.xml）。
    /// 1.7 的 TotalBandwidth/TotalAvailableControlGroups 是单行 getter（约 20 字节 IL），会被 JIT 内联，
    /// Harmony postfix 在调用点被跳过导致带宽回退为植入体默认值 6——故改为注入 GetStatValue 的属性计算
    /// （GetStatValue 体积大不会被内联），带宽 gizmo/机械页签/工作设置等所有读取方统一生效。
    /// </summary>
    public class StatPart_MechControlHub : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && pawn.TryGetMechControlHub(out CompMechControlHub hub))
            {
                if (parentStat == StatDefOf.MechBandwidth)
                {
                    // 假人自带的机械链接植入体(+6)不算数，只保留频段节点等外部带宽加成
                    val = hub.CurrentBandwidth + ExternalBandwidthBonus(pawn);
                }
                else if (parentStat == StatDefOf.MechControlGroups)
                {
                    val = hub.CurrentControlGroups;
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is Pawn pawn && pawn.TryGetMechControlHub(out CompMechControlHub hub))
            {
                if (parentStat == StatDefOf.MechBandwidth)
                {
                    return "机械集控核心: " + hub.CurrentBandwidth;
                }
                if (parentStat == StatDefOf.MechControlGroups)
                {
                    return "机械集控核心: " + hub.CurrentControlGroups;
                }
            }
            return null;
        }

        /// <summary>假人身上除机械链接植入体以外的带宽加成（频段节点调谐等）。</summary>
        private static float ExternalBandwidthBonus(Pawn pawn)
        {
            float bonus = 0f;
            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null)
            {
                return bonus;
            }
            foreach (Hediff hediff in hediffs)
            {
                if (hediff.def == HediffDefOf.MechlinkImplant)
                {
                    continue;
                }
                StatModifier mod = hediff.CurStage?.statOffsets?.FirstOrDefault(x => x.stat == StatDefOf.MechBandwidth);
                if (mod != null)
                {
                    bonus += mod.value;
                }
            }
            return bonus;
        }
    }

    /// <summary>虚拟机械师永远"可控制机械体"（原版 CanControlMechs 含机械链接植入体等校验，假人没有）。</summary>
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.CanControlMechs), MethodType.Getter)]
    public static class Patch_MechControlHub_CanControlMechs
    {
        public static void Postfix(Pawn_MechanitorTracker __instance, ref AcceptanceReport __result)
        {
            if (__instance.Pawn.TryGetMechControlHub(out _))
            {
                __result = true;
            }
        }
    }

    /// <summary>被核心接管的机械体视为始终在指挥范围内（假人不在地图上，原版距离判定恒失败）。</summary>
    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.InMechanitorCommandRange))]
    public static class Patch_MechControlHub_CommandRange
    {
        public static void Postfix(Pawn mech, LocalTargetInfo target, ref bool __result)
        {
            Pawn overseer = mech.GetOverseer();
            if (overseer != null && overseer.TryGetMechControlHub(out _))
            {
                __result = true;
            }
        }
    }

    /// <summary>机械体"选择控制者"gizmo：控制者是核心假人时，改为选中核心建筑。</summary>
    [HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.GetMechGizmos))]
    public static class Patch_MechControlHub_GetMechGizmos
    {
        private static Texture2D overseerIcon;

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn mech)
        {
            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is Command_Action command && command.Disabled
                    && command.defaultLabel == "CommandSelectOverseer".Translate())
                {
                    Pawn overseer = mech.GetOverseer();
                    if (overseer != null && overseer.TryGetMechControlHub(out CompMechControlHub hub))
                    {
                        if (overseerIcon == null)
                        {
                            overseerIcon = ContentFinder<Texture2D>.Get("UI/Icons/MMT_MechControlOverseer");
                        }
                        // 原版 gizmo 处于禁用态（控制者不在地图上），替换为可选中核心建筑的新指令
                        yield return new Command_Action
                        {
                            defaultLabel = "CommandSelectOverseer".Translate(),
                            defaultDesc = "CommandSelectOverseerDesc".Translate(),
                            icon = overseerIcon,
                            action = delegate
                            {
                                Find.Selector.ClearSelection();
                                Find.Selector.Select(hub.parent);
                            }
                        };
                        continue;
                    }
                }
                yield return gizmo;
            }
        }
    }

    /// <summary>带宽 gizmo：恰好选中一台集控核心时也显示（带宽数据来源是核心转发假人的指令面板）。</summary>
    [HarmonyPatch(typeof(MechanitorBandwidthGizmo), nameof(MechanitorBandwidthGizmo.Visible), MethodType.Getter)]
    public static class Patch_MechControlHub_BandwidthGizmoVisible
    {
        public static void Postfix(ref bool __result)
        {
            if (__result)
            {
                return;
            }
            int count = 0;
            List<object> selected = Find.Selector.SelectedObjects;
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] is Building building && building.GetComp<CompMechControlHub>() != null)
                {
                    count++;
                }
            }
            __result = count == 1;
        }
    }

    /// <summary>频段节点调谐：把同地图的集控核心（其虚拟机械师）加入调谐选项，连线画到建筑上。</summary>
    [HarmonyPatch(typeof(CompBandNode), nameof(CompBandNode.CompGetGizmosExtra))]
    public static class Patch_MechControlHub_BandNodeGizmos
    {
        private static Texture2D tuningIcon;

        // 1.7 中调谐耗时为私有属性，反射读取
        private static readonly PropertyInfo TuningTimeTicksProp =
            AccessTools.Property(typeof(CompBandNode), "TuningTimeTicks");
        private static readonly PropertyInfo RetuneTimeTicksProp =
            AccessTools.Property(typeof(CompBandNode), "RetuneTimeTicks");

        private static Texture2D TuningIcon
        {
            get
            {
                if (tuningIcon == null)
                {
                    tuningIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning");
                }
                return tuningIcon;
            }
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompBandNode __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is Command_Action command && command.icon == TuningIcon)
                {
                    yield return CreateTuneCommand(__instance, command);
                }
                else
                {
                    yield return gizmo;
                }
            }
        }

        /// <summary>替换原版调谐按钮：选项列表追加同图的集控核心（假人）；悬停连线到核心建筑。</summary>
        private static Command_Action CreateTuneCommand(CompBandNode bandNode, Command_Action vanilla)
        {
            Command_Action command = new Command_Action
            {
                defaultLabel = bandNode.tunedTo == null
                    ? "BandNodeTuneTo".Translate() + "..."
                    : "BandNodeRetuneTo".Translate() + "...",
                defaultDesc = bandNode.tunedTo == null
                    ? "BandNodeTuningDesc".Translate("PeriodSeconds".Translate(bandNode.Props.tuneSeconds))
                    : "BandNodeRetuningDesc".Translate(bandNode.Props.retuneDays + " " + "Days".Translate()),
                icon = TuningIcon
            };
            if (!HasAvailableMechanitor(bandNode))
            {
                command.Disable(null);
            }
            command.onHover = (Action)Delegate.Combine(command.onHover, (Action)delegate
            {
                Pawn pawn = bandNode.tuningTo ?? bandNode.tunedTo;
                if (pawn != null)
                {
                    GenDraw.DrawLineBetween(bandNode.parent.DrawPos,
                        pawn.TryGetMechControlHub(out CompMechControlHub hub) ? hub.parent.DrawPos : pawn.DrawPos);
                }
            });
            command.action = (Action)Delegate.Combine(command.action, (Action)delegate
            {
                Find.WindowStack.Add(new FloatMenu(BuildMechanitorOptions(bandNode)));
            });
            return command;
        }

        private static bool IsSelectableMechanitor(Pawn pawn, CompBandNode bandNode)
        {
            // 假人没有机械链接植入体，用 CanControlMechs（已补丁）判定；真人走原版 IsMechanitor
            bool mechanitor = pawn.TryGetMechControlHub(out _)
                ? pawn.mechanitor != null && pawn.mechanitor.CanControlMechs
                : MechanitorUtility.IsMechanitor(pawn);
            return mechanitor && pawn != bandNode.tunedTo;
        }

        private static bool HasAvailableMechanitor(CompBandNode bandNode)
        {
            if (bandNode.parent.Map.mapPawns.AllPawnsSpawned.Any(pawn => IsSelectableMechanitor(pawn, bandNode)))
            {
                return true;
            }
            return CompMechControlHub.All.Any(hub =>
                hub.parent.Map == bandNode.parent.Map && IsSelectableMechanitor(hub.dummyPawn, bandNode));
        }

        private static List<FloatMenuOption> BuildMechanitorOptions(CompBandNode bandNode)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Pawn pawn in bandNode.parent.Map.mapPawns.AllPawnsSpawned.Where(pawn => IsSelectableMechanitor(pawn, bandNode)))
            {
                AddOption(options, bandNode, pawn);
            }
            foreach (CompMechControlHub hub in CompMechControlHub.All)
            {
                if (hub.parent.Map == bandNode.parent.Map && IsSelectableMechanitor(hub.dummyPawn, bandNode))
                {
                    AddOption(options, bandNode, hub.dummyPawn);
                }
            }
            return options;
        }

        private static void AddOption(List<FloatMenuOption> options, CompBandNode bandNode, Pawn pawn)
        {
            Pawn local = pawn;
            string label = pawn.Name.ToStringFull;
            label = bandNode.tunedTo != null
                ? label + " (" + ((int)RetuneTimeTicksProp.GetValue(bandNode)).ToStringTicksToPeriod() + ")"
                : label + " (" + bandNode.Props.tuneSeconds + " " + "SecondsLower".Translate() + ")";
            options.Add(new FloatMenuOption(label, delegate
            {
                bandNode.tuningTimeLeft = bandNode.tunedTo == null
                    ? (int)TuningTimeTicksProp.GetValue(bandNode)
                    : (int)RetuneTimeTicksProp.GetValue(bandNode);
                bandNode.tuningTo = local;
            }));
        }
    }

    /// <summary>全灭判定：尚有集控核心运转时不判负（对齐原 mod；核心本身即是"人格"）。</summary>
    [HarmonyPatch(typeof(GameEnder), nameof(GameEnder.CheckOrUpdateGameOver))]
    public static class Patch_MechControlHub_GameEnder
    {
        public static void Postfix(GameEnder __instance)
        {
            if (__instance.gameEnding
                && CompMechControlHub.All.Any(hub => hub.parent != null && hub.parent.Faction == Faction.OfPlayer))
            {
                __instance.gameEnding = false;
            }
        }
    }

    /// <summary>开局/读档前清空静态注册表（防止上一局的假人引用泄漏到新局）。
    /// 必须 Prefix：读档时建筑的 PostSpawnSetup 在 LoadGameFromSaveFileNow 内部执行并注册假人，
    /// 若用 Postfix 会在方法末尾把刚注册好的表清空，导致补丁失配、带宽回退为原版默认值。</summary>
    [HarmonyPatch]
    public static class Patch_MechControlHub_ResetStatics
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Game), "InitNewGame");
            yield return AccessTools.Method(typeof(Game), "LoadGame");
            yield return AccessTools.Method(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow");
        }

        public static void Prefix()
        {
            CompMechControlHub.ResetStatics();
        }
    }
}
