using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 强化材料条目：defs 中任选其一满足（轮替设计，如钨/钛），数量 = count + countPerLevel×目标等级。
    /// </summary>
    public class EnhanceMaterialOption
    {
        /// <summary>可选材料（任一即可，如钨/钛轮替）。</summary>
        public List<ThingDef> defs;
        /// <summary>固定数量。</summary>
        public int count;
        /// <summary>随目标等级线性增长的数量系数。</summary>
        public int countPerLevel;

        /// <summary>目标等级下的需求量。</summary>
        public int CountFor(int targetLevel)
        {
            return count + countPerLevel * targetLevel;
        }

        /// <summary>菜单展示名（多选材料用 "/" 连接）。</summary>
        public string DefsLabel()
        {
            if (defs == null || defs.Count == 0)
                return "?";
            if (defs.Count == 1)
                return defs[0].LabelCap;
            string text = defs[0].label;
            for (int i = 1; i < defs.Count; i++)
                text += "/" + defs[i].label;
            return text;
        }
    }

    /// <summary>
    /// 强化材料阶梯段位：目标等级 ≤ maxLevel 时适用本段材料表（取第一个满足的段位）。
    /// </summary>
    public class EnhanceTierMaterials
    {
        /// <summary>适用上限等级（含）。</summary>
        public int maxLevel;
        /// <summary>本段材料条目。</summary>
        public List<EnhanceMaterialOption> options;
    }

    /// <summary>可强化装备接口：护盾强化与武器强化共用（供通用强化作业驱动调用）。</summary>
    public interface IEnhanceable
    {
        /// <summary>当前强化等级。</summary>
        int Level { get; }
        /// <summary>下一强化等级。</summary>
        int NextLevel { get; }
        /// <summary>是否已满级。</summary>
        bool IsMaxLevel { get; }
        /// <summary>强化到下一级的成功率（已含设置倍率）。</summary>
        float NextLevelSuccessRate { get; }
        /// <summary>强化到下一级的材料条目（阶梯或旧版单材料回退）。</summary>
        List<EnhanceMaterialOption> NextLevelOptions { get; }
        /// <summary>掷一次强化判定（材料由作业侧先行消耗）。</summary>
        bool DoEnhance();
        /// <summary>强化 comp 所属装备。</summary>
        Thing ParentThing { get; }
    }

    /// <summary>强化工位接口：充能台与武器强化台共用（持有待强化装备）。</summary>
    public interface IEnhanceStation
    {
        /// <summary>台内装备；空台为 null。</summary>
        Thing HeldItem { get; }
    }

    /// <summary>
    /// 可附魔装备接口：武器与防具强化 comp 共用（附魔槽/词条安装/效果合计），
    /// 强化台与附魔作业经本接口对两类装备透明。
    /// </summary>
    public interface IEnchantable : IEnhanceable
    {
        /// <summary>已安装附魔词条（模块物品 def，只读）。</summary>
        IReadOnlyList<ThingDef> Enchants { get; }
        /// <summary>附魔槽位数。</summary>
        int EnchantSlots { get; }
        /// <summary>本装备接受的模块槽位类别（WeaponEnchant / ApparelEnchant）。</summary>
        string ModuleSlotKey { get; }
        /// <summary>能否安装某词条模块。</summary>
        bool CanAddEnchant(ThingDef moduleDef);
        /// <summary>安装词条；槽满覆盖最早装的并返回被覆盖 def（无覆盖返回 null）。</summary>
        ThingDef AddEnchant(ThingDef moduleDef);
        /// <summary>词条效果合计。</summary>
        float GetEffectSum(string key);
    }

    /// <summary>已解析的具体材料需求（轮替条目落定到一个 def + 数量）。</summary>
    public struct ResolvedMaterial
    {
        public ThingDef def;
        public int count;

        public ResolvedMaterial(ThingDef def, int count)
        {
            this.def = def;
            this.count = count;
        }
    }

    /// <summary>
    /// 强化材料阶梯共享助手：轮替条目解析（选地图上存量最多的可选 def）、
    /// 可用量统计、菜单文案、背包精确消耗。菜单与作业两侧共用同一解析结果。
    /// </summary>
    public static class EnhanceLadder
    {
        /// <summary>按目标等级查阶梯段：取第一个 maxLevel ≥ targetLevel 的段位；无段位返回 null。</summary>
        public static List<EnhanceMaterialOption> OptionsFor(List<EnhanceTierMaterials> tiers, int targetLevel)
        {
            if (tiers == null || tiers.Count == 0)
                return null;
            for (int i = 0; i < tiers.Count; i++)
            {
                if (targetLevel <= tiers[i].maxLevel)
                    return tiers[i].options;
            }
            return tiers[tiers.Count - 1].options;
        }

        /// <summary>把材料条目解析成具体需求：轮替条目选地图上（含小人背包）存量最多的 def。</summary>
        public static List<ResolvedMaterial> Resolve(List<EnhanceMaterialOption> options, Map map, Pawn pawn, int targetLevel)
        {
            List<ResolvedMaterial> result = new List<ResolvedMaterial>();
            if (options == null)
                return result;
            foreach (EnhanceMaterialOption opt in options)
            {
                if (opt.defs == null || opt.defs.Count == 0)
                    continue;
                ThingDef best = opt.defs[0];
                int bestCount = -1;
                foreach (ThingDef def in opt.defs)
                {
                    int available = CountAvailable(map, pawn, def);
                    if (available > bestCount)
                    {
                        bestCount = available;
                        best = def;
                    }
                }
                result.Add(new ResolvedMaterial(best, opt.CountFor(targetLevel)));
            }
            return result;
        }

        /// <summary>地图上（未禁止）+ 小人背包内的材料总数。</summary>
        public static int CountAvailable(Map map, Pawn pawn, ThingDef def)
        {
            int total = 0;
            if (map != null)
            {
                foreach (Thing t in map.listerThings.ThingsOfDef(def))
                {
                    if (!t.IsForbidden(pawn))
                        total += t.stackCount;
                }
            }
            if (pawn?.inventory != null)
                total += pawn.inventory.innerContainer.TotalStackCountOfDef(def);
            return total;
        }

        /// <summary>菜单文案："能量结晶 ×5 + 钨/钛 ×2"。</summary>
        public static string Describe(List<EnhanceMaterialOption> options, int targetLevel)
        {
            if (options == null || options.Count == 0)
                return "";
            string text = "";
            foreach (EnhanceMaterialOption opt in options)
            {
                if (text.Length > 0)
                    text += " + ";
                text += opt.DefsLabel() + " ×" + opt.CountFor(targetLevel);
            }
            return text;
        }

        /// <summary>背包中是否已带齐全部解析需求。</summary>
        public static bool CarriedEnough(Pawn pawn, List<ResolvedMaterial> resolved)
        {
            foreach (ResolvedMaterial mat in resolved)
            {
                if (pawn.inventory.innerContainer.TotalStackCountOfDef(mat.def) < mat.count)
                    return false;
            }
            return true;
        }

        /// <summary>精确消耗背包中的解析需求（逐堆 SplitOff 销毁）；不足返回 false（不消耗）。</summary>
        public static bool ConsumeFromInventory(Pawn pawn, List<ResolvedMaterial> resolved)
        {
            foreach (ResolvedMaterial mat in resolved)
            {
                if (pawn.inventory.innerContainer.TotalStackCountOfDef(mat.def) < mat.count)
                    return false;
            }
            foreach (ResolvedMaterial mat in resolved)
            {
                int remaining = mat.count;
                foreach (Thing t in pawn.inventory.innerContainer.Where(x => x.def == mat.def).ToList())
                {
                    if (remaining <= 0)
                        break;
                    int num = Mathf.Min(remaining, t.stackCount);
                    t.SplitOff(num).Destroy(DestroyMode.Vanish);
                    remaining -= num;
                }
            }
            return true;
        }

        /// <summary>从装备上取强化接口（护盾/武器/防具强化 comp 均实现 IEnhanceable）。</summary>
        public static IEnhanceable Of(Thing thing)
        {
            if (thing is ThingWithComps twc)
            {
                CompShieldEnhancement shield = twc.TryGetComp<CompShieldEnhancement>();
                if (shield != null)
                    return shield;
                CompApparelEnhancement apparel = twc.TryGetComp<CompApparelEnhancement>();
                if (apparel != null)
                    return apparel;
                return twc.TryGetComp<CompWeaponEnhancement>();
            }
            return null;
        }

        /// <summary>
        /// 装备是否开放强化（设置"仅限 MMT/原版"勾选时，其它 mod 装备一律不生效）：
        /// comp 挂载是全体的（XML 补丁无运行时开关），效果与工位装载走本判定。
        /// </summary>
        public static bool EquipEligible(ThingDef def)
        {
            if (def == null)
                return false;
            if (!MMTSettings.WeaponEnhanceVanillaOnly)
                return true;
            return IsVanillaOrMmt(def);
        }

        /// <summary>防具是否开放强化（设置"仅限 MMT/原版防具"勾选时，其它 mod 防具一律不生效）。</summary>
        public static bool ApparelEquipEligible(ThingDef def)
        {
            if (def == null)
                return false;
            if (!MMTSettings.ApparelEnhanceVanillaOnly)
                return true;
            return IsVanillaOrMmt(def);
        }

        /// <summary>MMT_ 前缀或原版（Core mod）装备判定。</summary>
        private static bool IsVanillaOrMmt(ThingDef def)
        {
            return def.defName != null && def.defName.StartsWith("MMT_")
                || def.modContentPack != null && def.modContentPack.IsCoreMod;
        }
    }

    /// <summary>
    /// 通用强化作业（护盾充能台与武器强化台共用，targetA=工位）：
    /// 按材料阶梯逐种收集（轮替条目在派活时已解析定死），走到工位消耗材料并掷成功率。
    /// 存档安全：作业不持久化解析结果，中途读档后重建 toil 时按当前状态重新解析。
    /// </summary>
    public class JobDriver_EnhanceGeneric : JobDriver
    {
        /// <summary>强化作业时长（tick，1 倍速约 10 秒）。</summary>
        private const int EnhanceDurationTicks = 600;

        private IEnhanceStation Station
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompChargingStation>() as IEnhanceStation
                    ?? twc.TryGetComp<CompWeaponEnhanceBench>() : null;
            }
        }

        private IEnhanceable Enhancement
        {
            get { return EnhanceLadder.Of(Station?.HeldItem); }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => Enhancement == null || Enhancement.IsMaxLevel);

            // ---- 流程开始：逐种材料收集循环（每种材料一个独立循环，收齐一种再收下一种） ----
            IEnhanceable enh = Enhancement;
            if (enh == null)
                yield break;
            List<ResolvedMaterial> resolved = EnhanceLadder.Resolve(enh.NextLevelOptions, pawn.Map, pawn, enh.NextLevel);
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
                        Messages.Message("强化中止：地图上找不到可用的 " + cur.def.label,
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
            // ---- 材料收齐：走到工位执行强化 ----
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            Toil enhance = Toils_General.Wait(EnhanceDurationTicks, TargetIndex.A);
            enhance.WithProgressBarToilDelay(TargetIndex.A);
            yield return enhance;
            yield return Toils_General.DoAtomic(FinishEnhance);
            // ---- 流程结束 ----
        }

        /// <summary>强化结算：重新解析需求并精确消耗后掷成功率；材料不足（作业期间被挪用）则不掷骰直接中止。</summary>
        private void FinishEnhance()
        {
            IEnhanceable enh = Enhancement;
            if (enh == null || enh.IsMaxLevel)
                return;
            int targetLevel = enh.NextLevel;
            List<ResolvedMaterial> resolved = EnhanceLadder.Resolve(enh.NextLevelOptions, pawn.Map, pawn, targetLevel);
            if (!EnhanceLadder.ConsumeFromInventory(pawn, resolved))
            {
                Messages.Message("强化中止：材料在作业期间被挪用", pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Thing station = job.GetTarget(TargetIndex.A).Thing;
            if (enh.DoEnhance())
            {
                // 流程结束：强化成功 -> 等级 +1（特效 + 消息）
                MoteMaker.ThrowText(station.DrawPos, Map, "强化成功！ +" + enh.Level, Color.cyan, 3f);
                if (SoundDefOf.EnergyShield_Reset != null)
                    SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(station.Position, Map));
                Messages.Message("强化成功：" + enh.ParentThing.LabelCap + " 提升到 +" + enh.Level + " 级",
                    new LookTargets(station), MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                // 流程结束：强化失败 -> 仅损失材料
                MoteMaker.ThrowText(station.DrawPos, Map, "强化失败", Color.red, 3f);
                Messages.Message("强化失败：" + enh.ParentThing.LabelCap + " 未能提升到 +" + targetLevel
                    + " 级（材料已消耗）", new LookTargets(station), MessageTypeDefOf.NegativeEvent, false);
            }
        }
    }
}
