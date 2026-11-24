using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 品质重铸共享助手（装备强化台右键"重铸"入口与作业驱动共用）：
    /// 一般→良好 50% / 良好→极佳 30% 耗合金轮替池 ×10；极佳→大师 10% 耗远古合金 ×5。
    /// 失败仅损失材料，装备品质不变；仅一般/良好/极佳可重铸，大师封顶。
    /// 材料池/数量/成功率全部走 CompProperties_WeaponEnhanceBench 的 XML 配置（缺省回退内置默认）。
    /// </summary>
    public static class ReforgeUtil
    {
        /// <summary>默认合金轮替池（XML reforgeAlloyDefs 缺省时的回退；与强化台建筑 Def 保持一致）。</summary>
        private static readonly string[] DefaultAlloyDefNames =
        {
            "MMT_LeadBar", "MMT_AluminiumBar", "MMT_FerrosiliconAlloy",
            "MMT_NickelBar", "MMT_Cobalt", "MMT_Chromium",
            "MMT_Tungsten", "MMT_Titanium", "MMT_SteelBar", "MMT_DepletedUranium",
            "MMT_StelliteAlloy", "MMT_NitinolAlloy", "MMT_PobediteAlloy",
            "MMT_CupronickelAlloy", "MMT_AlnicoAlloy",
        };

        /// <summary>惰性解析的默认合金池（缺 def 静默跳过）。</summary>
        private static List<ThingDef> defaultAlloyDefs;

        /// <summary>合金轮替池（XML 优先，缺省回退内置默认池）。</summary>
        public static List<ThingDef> AlloyPool(CompProperties_WeaponEnhanceBench props)
        {
            if (props != null && props.reforgeAlloyDefs != null && props.reforgeAlloyDefs.Count > 0)
                return props.reforgeAlloyDefs;
            if (defaultAlloyDefs == null)
            {
                defaultAlloyDefs = new List<ThingDef>();
                foreach (string name in DefaultAlloyDefNames)
                {
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                    if (def != null)
                        defaultAlloyDefs.Add(def);
                }
            }
            return defaultAlloyDefs;
        }

        /// <summary>极佳→大师 的消耗材料（XML 优先，缺省回退 MMT_AncientAlloy）。</summary>
        public static ThingDef AncientAlloy(CompProperties_WeaponEnhanceBench props)
        {
            if (props != null && props.reforgeAncientAlloy != null)
                return props.reforgeAncientAlloy;
            return DefDatabase<ThingDef>.GetNamedSilentFail("MMT_AncientAlloy");
        }

        /// <summary>品质中文名（右键菜单/消息文案用）。</summary>
        public static string QualityLabel(QualityCategory q)
        {
            switch (q)
            {
                case QualityCategory.Awful: return "极差";
                case QualityCategory.Poor: return "较差";
                case QualityCategory.Normal: return "一般";
                case QualityCategory.Good: return "良好";
                case QualityCategory.Excellent: return "极佳";
                case QualityCategory.Masterwork: return "大师";
                default: return "传奇";
            }
        }

        /// <summary>当前品质是否可重铸（仅一般/良好/极佳，大师封顶、过低品质不收）。</summary>
        public static bool Reforgeable(QualityCategory q)
        {
            return q == QualityCategory.Normal || q == QualityCategory.Good || q == QualityCategory.Excellent;
        }

        /// <summary>重铸目标品质（升一档）。</summary>
        public static QualityCategory NextQuality(QualityCategory q)
        {
            switch (q)
            {
                case QualityCategory.Normal: return QualityCategory.Good;
                case QualityCategory.Good: return QualityCategory.Excellent;
                case QualityCategory.Excellent: return QualityCategory.Masterwork;
                default: return q;
            }
        }

        /// <summary>当前品质重铸升档的成功率。</summary>
        public static float SuccessRate(CompProperties_WeaponEnhanceBench props, QualityCategory q)
        {
            switch (q)
            {
                case QualityCategory.Normal: return props.reforgeRateToGood;
                case QualityCategory.Good: return props.reforgeRateToExcellent;
                case QualityCategory.Excellent: return props.reforgeRateToMasterwork;
                default: return 0f;
            }
        }

        /// <summary>当前品质重铸所需的材料条目（合金轮替池 或 远古合金）。</summary>
        public static List<EnhanceMaterialOption> MaterialOptions(CompProperties_WeaponEnhanceBench props, QualityCategory q)
        {
            EnhanceMaterialOption opt = new EnhanceMaterialOption();
            if (q == QualityCategory.Excellent)
            {
                opt.defs = new List<ThingDef> { AncientAlloy(props) };
                opt.count = props.reforgeAncientAlloyCount;
            }
            else
            {
                opt.defs = AlloyPool(props);
                opt.count = props.reforgeAlloyCount;
            }
            return new List<EnhanceMaterialOption> { opt };
        }

        /// <summary>材料需求文案（"合金 ×10" / "远古合金 ×5"）。</summary>
        public static string DescribeMaterials(CompProperties_WeaponEnhanceBench props, QualityCategory q)
        {
            if (q == QualityCategory.Excellent)
                return "远古合金 ×" + props.reforgeAncientAlloyCount;
            return "合金 ×" + props.reforgeAlloyCount;
        }

        /// <summary>台内装备当前能否重铸（有品质 comp 且品质在可重铸区间）。</summary>
        public static bool CanReforge(CompWeaponEnhanceBench bench)
        {
            Thing held = bench?.HeldItem;
            if (held == null)
                return false;
            CompQuality quality = held.TryGetComp<CompQuality>();
            return quality != null && Reforgeable(quality.Quality);
        }

        /// <summary>按台内装备当前品质解析重铸材料需求（轮替条目选地图上存量最多的合金）。</summary>
        public static List<ResolvedMaterial> ResolveMaterials(CompWeaponEnhanceBench bench, Pawn pawn)
        {
            CompQuality quality = bench.HeldItem.TryGetComp<CompQuality>();
            return EnhanceLadder.Resolve(MaterialOptions(bench.BenchProps, quality.Quality), pawn.Map, pawn, 0);
        }

        /// <summary>右键菜单项文案与可用性（材料不足/品质不在区间时禁用）。</summary>
        public static bool BuildMenuOption(CompProperties_WeaponEnhanceBench props, Thing held, CompQuality quality, Pawn pawn, out string label)
        {
            string name = held.LabelCap;
            QualityCategory from = quality.Quality;
            if (!Reforgeable(from))
            {
                label = from < QualityCategory.Normal
                    ? "重铸装备：" + name + "（品质「" + QualityLabel(from) + "」过低，需一般及以上）"
                    : "重铸装备：" + name + "（已达最高可重铸品质「大师」）";
                return false;
            }
            QualityCategory to = NextQuality(from);
            label = "重铸装备：" + name + "（" + QualityLabel(from) + " → " + QualityLabel(to)
                + "，成功率 " + SuccessRate(props, from).ToStringPercent("F0")
                + "，需 " + DescribeMaterials(props, from) + "）";
            List<ResolvedMaterial> resolved = EnhanceLadder.Resolve(MaterialOptions(props, from), pawn.Map, pawn, 0);
            foreach (ResolvedMaterial mat in resolved)
            {
                if (EnhanceLadder.CountAvailable(pawn.Map, pawn, mat.def) < mat.count)
                {
                    label += "——材料不足";
                    return false;
                }
            }
            return true;
        }

        /// <summary>重铸结算（材料已由作业侧消耗）：掷成功率，成功品质升一档，失败仅损失材料。</summary>
        public static void DoReforge(CompWeaponEnhanceBench bench, Pawn pawn, Map map)
        {
            Thing held = bench?.HeldItem;
            CompQuality quality = held?.TryGetComp<CompQuality>();
            if (quality == null || !Reforgeable(quality.Quality))
                return;
            QualityCategory from = quality.Quality;
            QualityCategory to = NextQuality(from);
            Thing station = bench.parent;
            if (Rand.Chance(SuccessRate(bench.BenchProps, from)))
            {
                quality.SetQuality(to, ArtGenerationContext.Outsider);
                MoteMaker.ThrowText(station.DrawPos, map, "重铸成功！", Color.cyan, 3f);
                if (SoundDefOf.EnergyShield_Reset != null)
                    SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(station.Position, map));
                Messages.Message("重铸成功：" + held.LabelCap + " 品质提升为「" + QualityLabel(to) + "」",
                    new LookTargets(station), MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                MoteMaker.ThrowText(station.DrawPos, map, "重铸失败", Color.red, 3f);
                Messages.Message("重铸失败：" + held.LabelCap + " 品质未提升（材料已消耗）",
                    new LookTargets(station), MessageTypeDefOf.NegativeEvent, false);
            }
        }
    }

    /// <summary>
    /// 重铸装备作业（targetA=强化台）：按解析结果搬合金/远古合金到台边，消耗材料并掷成功率重铸台内装备品质。
    /// 存档安全：作业不持久化解析结果，中途读档后重建 toil 时按当前状态重新解析（同 JobDriver_EnhanceGeneric）。
    /// </summary>
    public class JobDriver_ReforgeGear : JobDriver
    {
        /// <summary>重铸作业时长（tick，与强化作业一致约 10 秒）。</summary>
        private const int ReforgeDurationTicks = 600;

        private CompWeaponEnhanceBench Bench
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompWeaponEnhanceBench>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => !ReforgeUtil.CanReforge(Bench));

            // ---- 流程开始：材料收集循环（重铸只耗一种材料，轮替条目在派活时已解析定死） ----
            CompWeaponEnhanceBench bench = Bench;
            if (bench == null)
                yield break;
            List<ResolvedMaterial> resolved = ReforgeUtil.ResolveMaterials(bench, pawn);
            foreach (ResolvedMaterial mat in resolved)
            {
                ResolvedMaterial cur = mat;
                Toil collectLoop = Toils_General.Label();
                Toil collectDone = Toils_General.Label();
                yield return collectLoop;
                yield return Toils_Jump.JumpIf(collectDone,
                    () => pawn.inventory.innerContainer.TotalStackCountOfDef(cur.def) >= cur.count);
                // 找最近可用材料堆设为 targetB；找不到则中止（消息提示）
                Toil findStack = new Toil();
                findStack.initAction = delegate
                {
                    Thing stack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                        ThingRequest.ForDef(cur.def), PathEndMode.ClosestTouch, TraverseParms.For(pawn),
                        9999f, t => !t.IsForbidden(pawn) && pawn.CanReserve(t, 1, -1, null, false));
                    if (stack == null)
                    {
                        Messages.Message("重铸中止：地图上找不到可用的 " + cur.def.label,
                            pawn, MessageTypeDefOf.RejectInput, false);
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    job.SetTarget(TargetIndex.B, stack);
                    pawn.Reserve(stack, job, 1, -1, null, true);
                };
                findStack.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return findStack;
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                    .FailOnDespawnedNullOrForbidden(TargetIndex.B);
                yield return Toils_Haul.TakeToInventory(TargetIndex.B, new Func<int>(delegate
                {
                    Thing stack = job.GetTarget(TargetIndex.B).Thing;
                    if (stack == null)
                        return 0;
                    int need = cur.count - pawn.inventory.innerContainer.TotalStackCountOfDef(cur.def);
                    return Mathf.Min(need, stack.stackCount);
                }));
                yield return Toils_Jump.Jump(collectLoop);
                yield return collectDone;
            }
            // ---- 材料收齐：走到强化台执行重铸 ----
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            Toil reforge = Toils_General.Wait(ReforgeDurationTicks, TargetIndex.A);
            reforge.WithProgressBarToilDelay(TargetIndex.A);
            yield return reforge;
            yield return Toils_General.DoAtomic(FinishReforge);
            // ---- 流程结束 ----
        }

        /// <summary>重铸结算：重新解析需求并精确消耗后掷成功率；材料不足（作业期间被挪用）则不掷骰直接中止。</summary>
        private void FinishReforge()
        {
            CompWeaponEnhanceBench bench = Bench;
            if (!ReforgeUtil.CanReforge(bench))
                return;
            List<ResolvedMaterial> resolved = ReforgeUtil.ResolveMaterials(bench, pawn);
            if (!EnhanceLadder.ConsumeFromInventory(pawn, resolved))
            {
                Messages.Message("重铸中止：材料在作业期间被挪用", pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            ReforgeUtil.DoReforge(bench, pawn, Map);
        }
    }
}
