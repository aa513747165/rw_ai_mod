using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    // 支柱二·历练/Perk 框架（mmt-rpg-design.md §2.1/§2.2 + mmt-class-design.md §5.3）：
    // CompPawnProgress 经 XPath 补丁挂到 Human ThingDef，记录历练 XP/等级/perk 点/已选 perk/职业；
    // XP 来源三钩子：伤害(Pawn.PostApplyDamage)、击杀(Pawn.Kill)、技能学习(SkillRecord.Learn)；
    // 每级被动走 MMT_PawnProgress Hediff 的 20 个 severity stage（becomeVisible=false 隐藏）；
    // 品质暴击为第 19 个 perk（MMT_Perk_Masterwork，2 点），Harmony postfix 品质生成点。

    /// <summary>历练 comp 参数（无字段，挂载标记用）。</summary>
    public class CompProperties_PawnProgress : CompProperties
    {
        public CompProperties_PawnProgress()
        {
            compClass = typeof(CompPawnProgress);
        }
    }

    /// <summary>
    /// 小人历练成长：XP/等级(1-60)/天赋点/已解锁天赋节点/已选 perk(旧)/职业记录。
    /// 存档字段全部带默认值，旧存档小人补挂 comp 后从 1 级开始。
    /// v2 天赋树：每级 +1 天赋点；perkPoints/perks 为旧字段（迁移后停写，见 TalentTreeUtil.Migrate）。
    /// </summary>
    public class CompPawnProgress : ThingComp
    {
        public const int MaxLevel = 60;

        /// <summary>总 XP 之外另记当前级进度，升级曲线 N→N+1 需 100×N^1.5。</summary>
        public float xp;
        public int level = 1;
        /// <summary>旧 perk 点（v2 起不再发放，迁移时 1:1 转入 talentPoints）。</summary>
        public int perkPoints;
        /// <summary>旧 perk 列表（迁移时标记为通用树节点，Hediff 保留）。</summary>
        public List<HediffDef> perks = new List<HediffDef>();
        /// <summary>天赋点（v2：每级 +1，天赋树加点货币）。</summary>
        public int talentPoints;
        /// <summary>已解锁天赋节点（"treeDefName:nodeId"）。</summary>
        public List<string> unlockedTalents = new List<string>();
        /// <summary>已解锁就职资格的职业家族 key（通用树 ClassUnlock 节点写入）。</summary>
        public List<string> unlockedFamilies = new List<string>();
        /// <summary>旧档迁移完成标记。</summary>
        public bool perkMigrated;
        /// <summary>已就职职业的 I 阶 Hediff（null=未就职；每人限 1 职业，v1 不做转职）。</summary>
        public HediffDef classDef;

        /// <summary>unlockedTalents 的哈希查询缓存（运行时不存档；懒构建，Clear 类批量变更后需 InvalidateTalentCache）。</summary>
        private HashSet<string> unlockedSet;

        /// <summary>已解锁节点键的 O(1) 查询集（UI 每帧高频调用路径用）。</summary>
        public HashSet<string> UnlockedSet
        {
            get
            {
                if (unlockedSet == null)
                {
                    unlockedSet = new HashSet<string>(unlockedTalents);
                }
                return unlockedSet;
            }
        }

        /// <summary>unlockedTalents 被绕过 UnlockedSet 批量改写后调用，使哈希缓存失效重建。</summary>
        public void InvalidateTalentCache()
        {
            unlockedSet = null;
        }

        public Pawn Pawn => parent as Pawn;

        /// <summary>查找小人的历练 comp（无 comp 静默返回 null）。</summary>
        public static CompPawnProgress Get(Pawn pawn)
        {
            return pawn?.TryGetComp<CompPawnProgress>();
        }

        /// <summary>升级 XP 曲线查表（100×N^1.5 预计算；AddXp 高频路径避免 Math.Pow）。</summary>
        private static readonly float[] XpTable = BuildXpTable();

        private static float[] BuildXpTable()
        {
            float[] table = new float[MaxLevel + 1];
            for (int i = 1; i <= MaxLevel; i++)
            {
                table[i] = 100f * (float)Math.Pow(i, 1.5);
            }
            return table;
        }

        /// <summary>N 级升到 N+1 级所需 XP。</summary>
        public static float XpForLevel(int n)
        {
            return n >= 1 && n <= MaxLevel ? XpTable[n] : 100f * (float)Math.Pow(n, 1.5);
        }

        /// <summary>当前级进度 0-1（满级返回 1）。</summary>
        public float LevelProgress
        {
            get { return level >= MaxLevel ? 1f : Mathf.Clamp01(xp / XpForLevel(level)); }
        }

        /// <summary>获得 XP（总开关关闭时直接丢弃；既有数据保留）。</summary>
        public void AddXp(float amount, bool showMote = false)
        {
            if (!MMTSettings.PawnProgressEnabled || amount <= 0f || level >= MaxLevel)
            {
                return;
            }
            Pawn pawn = Pawn;
            if (pawn == null || pawn.Dead || pawn.Faction == null || !pawn.Faction.IsPlayer)
            {
                return;
            }
            xp += amount * MMTSettings.XpMultiplier;
            if (showMote && MMTSettings.Instance != null && MMTSettings.Instance.xpGainMotes
                && pawn.MapHeld != null && amount >= 1f)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "+" + amount.ToString("F0") + " 历练", 3f);
            }
            while (level < MaxLevel && xp >= XpForLevel(level))
            {
                xp -= XpForLevel(level);
                level++;
                talentPoints++;
                OnLevelUp(pawn);
            }
        }

        private void OnLevelUp(Pawn pawn)
        {
            // 每级被动：刷新 MMT_PawnProgress Hediff 的 stage（severity=等级，20 档封顶，20 级后成长走天赋树）
            RefreshLevelHediff(pawn);
            Messages.Message(pawn.LabelShort + " 历练提升到 " + level + " 级，获得 1 点天赋点", pawn, MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>确保历练 Hediff 存在并把 severity 同步到当前等级。</summary>
        public void RefreshLevelHediff(Pawn pawn)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MMT_ProgressDefOf.MMT_PawnProgress);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(MMT_ProgressDefOf.MMT_PawnProgress, pawn);
                pawn.health.AddHediff(hediff);
            }
            hediff.Severity = level;
        }

        // —— Perk 选择 ——

        /// <summary>perk 三支分组（defName → 花费点数）；校验套件 tools/validate/pawn_progress.py 同步核对。</summary>
        public static readonly Dictionary<string, (string branch, int cost)> PerkTable = new Dictionary<string, (string, int)>
        {
            // 战斗支
            { "MMT_Perk_Veteran", ("战斗", 1) },
            { "MMT_Perk_Brawler", ("战斗", 1) },
            { "MMT_Perk_Tough", ("战斗", 1) },
            { "MMT_Perk_Fleet", ("战斗", 1) },
            { "MMT_Perk_FastHeal", ("战斗", 1) },
            { "MMT_Perk_BattleSense", ("战斗", 1) },
            // 生产支
            { "MMT_Perk_Deft", ("生产", 1) },
            { "MMT_Perk_Miner", ("生产", 1) },
            { "MMT_Perk_Farmer", ("生产", 1) },
            { "MMT_Perk_Builder", ("生产", 1) },
            { "MMT_Perk_Scholar", ("生产", 1) },
            { "MMT_Perk_Chef", ("生产", 1) },
            // 生存支
            { "MMT_Perk_IronBody", ("生存", 1) },
            { "MMT_Perk_EfficientGut", ("生存", 1) },
            { "MMT_Perk_DeepSleep", ("生存", 1) },
            { "MMT_Perk_MindWall", ("生存", 1) },
            { "MMT_Perk_GritMind", ("生存", 1) },
            { "MMT_Perk_PackMule", ("生存", 1) },
            // 高阶：品质暴击（mmt-class-design.md §5.3，2 点）
            { "MMT_Perk_Masterwork", ("生产", 2) },
        };

        /// <summary>尝试学习 perk：点数足够且未学时加 Hediff 并登记。</summary>
        public bool TryLearnPerk(HediffDef perkDef)
        {
            Pawn pawn = Pawn;
            if (pawn == null || perks.Contains(perkDef))
            {
                return false;
            }
            if (!PerkTable.TryGetValue(perkDef.defName, out (string branch, int cost) info) || perkPoints < info.cost)
            {
                return false;
            }
            perkPoints -= info.cost;
            perks.Add(perkDef);
            pawn.health.AddHediff(perkDef);
            Messages.Message(pawn.LabelShort + " 习得强化：" + perkDef.LabelCap, pawn, MessageTypeDefOf.PositiveEvent);
            return true;
        }

        public bool HasPerk(string defName)
        {
            return perks.Any(p => p.defName == defName);
        }

        /// <summary>perk 效果说明文本（读 HediffDef 首个 stage 的 stat 偏移/系数生成）。</summary>
        public static string DescribePerk(HediffDef def)
        {
            if (!string.IsNullOrEmpty(def.description))
            {
                return def.description;
            }
            HediffStage stage = def.stages?.FirstOrDefault();
            if (stage == null)
            {
                return "";
            }
            List<string> parts = new List<string>();
            if (stage.statOffsets != null)
            {
                foreach (StatModifier m in stage.statOffsets)
                {
                    parts.Add(m.stat.label + " " + (m.value >= 0f ? "+" : "") + m.value.ToString("0.##"));
                }
            }
            if (stage.statFactors != null)
            {
                foreach (StatModifier m in stage.statFactors)
                {
                    parts.Add(m.stat.label + " ×" + m.value.ToString("F2"));
                }
            }
            return string.Join("，", parts);
        }

        /// <summary>有可用天赋点时给选中小人挂天赋树 gizmo（总开关关闭隐藏）。</summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = Pawn;
            if (!MMTSettings.PawnProgressEnabled || pawn == null || pawn.Faction == null || !pawn.Faction.IsPlayer)
            {
                yield break;
            }
            if (talentPoints > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "天赋树（可用 " + talentPoints + " 点）",
                    defaultDesc = "打开天赋树，消耗天赋点解锁节点。",
                    icon = TexCommand.GatherSpotActive,
                    action = delegate { Find.WindowStack.Add(new Window_TalentTree(Pawn)); },
                };
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                // 新生成小人：补发通用树 Start 节点（迁移逻辑对旧档走 PostExposeData）
                TalentTreeUtil.Migrate(this);
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref xp, "xp");
            Scribe_Values.Look(ref level, "level", 1);
            Scribe_Values.Look(ref perkPoints, "perkPoints");
            Scribe_Collections.Look(ref perks, "perks", LookMode.Def);
            Scribe_Values.Look(ref talentPoints, "talentPoints");
            Scribe_Collections.Look(ref unlockedTalents, "unlockedTalents", LookMode.Value);
            Scribe_Collections.Look(ref unlockedFamilies, "unlockedFamilies", LookMode.Value);
            Scribe_Values.Look(ref perkMigrated, "perkMigrated");
            Scribe_Defs.Look(ref classDef, "classDef");
            if (perks == null)
            {
                perks = new List<HediffDef>();
            }
            if (unlockedTalents == null)
            {
                unlockedTalents = new List<string>();
            }
            if (unlockedFamilies == null)
            {
                unlockedFamilies = new List<string>();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                TalentTreeUtil.Migrate(this);
            }
        }
    }

    /// <summary>小人"历练"页签：等级/XP 进度/强化点/已选 perk/职业（mmt-rpg-design.md §2.4）。</summary>
    public class ITab_Pawn_Progress : ITab
    {
        public ITab_Pawn_Progress()
        {
            size = new Vector2(420f, 360f);
            labelKey = "MMT_TabPawnProgress";
        }

        protected override void FillTab()
        {
            Pawn pawn = SelPawn;
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp == null)
            {
                return;
            }
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            listing.Label("历练等级：" + comp.level + " / " + CompPawnProgress.MaxLevel);
            Rect barRect = listing.GetRect(22f);
            Widgets.FillableBar(barRect, comp.LevelProgress);
            string xpText = comp.level >= CompPawnProgress.MaxLevel
                ? "已满级"
                : comp.xp.ToString("F0") + " / " + CompPawnProgress.XpForLevel(comp.level).ToString("F0") + " XP";
            Widgets.Label(barRect, "  " + xpText);
            listing.Gap(4f);

            listing.Label("可用天赋点：" + comp.talentPoints);
            listing.Gap(4f);

            Hediff classHediff = PawnClassUtil.CurrentClassHediff(pawn);
            string classText = classHediff != null ? "职业：" + classHediff.def.LabelCap.ToString() : "职业：未就职（职业导师台）";
            listing.Label(classText);
            listing.Gap(4f);

            listing.Label("已解锁天赋节点：" + comp.unlockedTalents.Count + " 个");
            if (comp.unlockedFamilies.Count > 0)
            {
                listing.Label("已解锁职业家族：" + comp.unlockedFamilies.Count + " 个");
            }
            listing.Gap(8f);
            if (listing.ButtonText("打开天赋树"))
            {
                Find.WindowStack.Add(new Window_TalentTree(pawn));
            }
            listing.End();
        }
    }

    /// <summary>
    /// 历练书摄入效果（mmt-dungeon-design.md §3.2 消耗品）：服用后获得历练 XP。
    /// 无历练 comp（非人类/旧存档）或总开关关闭时静默无效。
    /// </summary>
    public class IngestionOutcomeDoer_GiveProgressXp : IngestionOutcomeDoer
    {
        /// <summary>每本给予的历练 XP。</summary>
        public float xpAmount = 500f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp == null)
                return;
            comp.AddXp(xpAmount * ingestedCount, true);
        }
    }

    /// <summary>Dev 模式快速刷历练（mmt-rpg-design.md 调试便利）。</summary>
    public static class ProgressDebugActions
    {
        [DebugAction("MMT 职业", "历练升 1 级（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LevelUpOnce(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp == null || comp.level >= CompPawnProgress.MaxLevel)
            {
                return;
            }
            // AddXp 会乘全局倍率，除以倍率保证恰好升 1 级（倍率异常时兜底 1）
            float mult = Math.Max(0.01f, MMTSettings.XpMultiplier);
            float need = (CompPawnProgress.XpForLevel(comp.level) - comp.xp) / mult + 1f;
            comp.AddXp(need);
        }

        [DebugAction("MMT 职业", "历练直升满级（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LevelToMax(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp == null)
            {
                return;
            }
            float mult = Math.Max(0.01f, MMTSettings.XpMultiplier);
            float need = 0f;
            for (int n = comp.level; n < CompPawnProgress.MaxLevel; n++)
            {
                need += CompPawnProgress.XpForLevel(n);
            }
            comp.AddXp((need - comp.xp) / mult + 1f);
        }
    }

    [DefOf]
    public static class MMT_ProgressDefOf
    {
        /// <summary>历练等级被动 Hediff（20 档 severity stage）。</summary>
        public static HediffDef MMT_PawnProgress;

        static MMT_ProgressDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ProgressDefOf));
        }
    }

    /// <summary>伤害 XP：每 1 点实际伤害 0.1 XP（instigator 为玩家人类时）。</summary>
    [HarmonyPatch(typeof(Pawn), "PostApplyDamage")]
    public static class Patch_ProgressDamageXp
    {
        public static void Postfix(DamageInfo dinfo)
        {
            if (!MMTSettings.PawnProgressEnabled || dinfo.Amount <= 0f)
            {
                return;
            }
            Pawn instigator = dinfo.Instigator as Pawn;
            CompPawnProgress comp = CompPawnProgress.Get(instigator);
            if (comp != null)
            {
                comp.AddXp(dinfo.Amount * 0.1f);
            }
            // 死亡骑士阶位被动：近战吸血 1%/2%/3%（mmt-class-design.md §3.32，无法纯 stat 表达，随伤害钩子结算）
            if (instigator != null && (dinfo.Def.isRanged == false))
            {
                Hediff dk = PawnClassUtil.CurrentClassHediff(instigator);
                if (dk != null && dk.def.defName.StartsWith("MMT_Class_DeathKnight_"))
                {
                    float factor = dk.def.defName.EndsWith("_III") ? 0.03f : dk.def.defName.EndsWith("_II") ? 0.02f : 0.01f;
                    Hediff_Injury worst = instigator.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Where(h => !h.IsPermanent()).OrderByDescending(h => h.Severity).FirstOrDefault();
                    if (worst != null)
                    {
                        worst.Severity -= dinfo.Amount * factor;
                    }
                }
            }
        }
    }

    /// <summary>击杀 XP：人类 10 / 虫 8 / 机械 15 / Boss 200（instigator 判定）。</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_ProgressKillXp
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (!MMTSettings.PawnProgressEnabled || dinfo == null)
            {
                return;
            }
            Pawn instigator = dinfo.Value.Instigator as Pawn;
            CompPawnProgress comp = CompPawnProgress.Get(instigator);
            if (comp == null || __instance?.RaceProps == null)
            {
                return;
            }
            float xp;
            if (__instance.kindDef != null && __instance.kindDef.defName.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                xp = 200f;
            }
            else if (__instance.RaceProps.IsMechanoid)
            {
                xp = 15f;
            }
            else if (__instance.RaceProps.Humanlike)
            {
                xp = 10f;
            }
            else if (__instance.RaceProps.Insect)
            {
                xp = 8f;
            }
            else
            {
                return;
            }
            comp.AddXp(xp, showMote: true);
        }
    }

    /// <summary>工作学习 XP：技能经验 × 0.2。</summary>
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
    public static class Patch_ProgressSkillXp
    {
        public static void Postfix(float xp, Pawn ___pawn)
        {
            if (!MMTSettings.PawnProgressEnabled || xp <= 0f)
            {
                return;
            }
            CompPawnProgress comp = CompPawnProgress.Get(___pawn);
            if (comp != null)
            {
                comp.AddXp(xp * 0.2f);
            }
        }
    }

    /// <summary>
    /// 品质暴击（mmt-class-design.md §5.3，perk MMT_Perk_Masterwork）：
    /// 制作成品 10% 概率品质 +1 档。1.7 品质生成出口为
    /// QualityUtility.GenerateQualityCreatedByPawn(Pawn, SkillDef, bool consumeInspiration)。
    /// </summary>
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), typeof(Pawn), typeof(SkillDef), typeof(bool))]
    public static class Patch_MasterworkQualityCrit
    {
        public static void Postfix(Pawn pawn, ref QualityCategory __result)
        {
            if (pawn == null || __result >= QualityCategory.Legendary
                || !MMTSettings.ClassQualityCritEnabled)
            {
                return;
            }
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp != null && comp.HasPerk("MMT_Perk_Masterwork") && Rand.Chance(0.1f))
            {
                __result += 1;
                Messages.Message("品质暴击：" + pawn.LabelShort + " 的成品品质提升到了 " + __result.GetLabel(),
                    pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
