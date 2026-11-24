using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// Boss 池（mmt-dungeon-design.md §4.7）：Boss 一律从「XML 白名单 ∪ 全库自动扫描」的池子里
    /// 按威胁预算（由财富公式推导）选取，预算允许时一处巢穴可出多个 Boss。
    /// 自动扫描零 mod 感知代码（isFighter + combatPower 门槛，原版/Mod Boss 通吃）；
    /// 后继新增 MMT Boss 只需把 defName 登记进据点/召唤器的 XML 白名单即入池参选，代码零改动。
    /// 池构建结果静态缓存一次（Def 库运行期不变），选取只发生在据点地图生成瞬间。
    /// </summary>
    public static class BossPool
    {
        /// <summary>自动扫描战力门槛：isFighter 且 combatPower ≥ 该值视为 Boss 级（与 Boss 召唤器默认阈值一致）。</summary>
        public const float AutoScanMinCombatPower = 150f;

        /// <summary>低财富兜底容忍度：池内最弱 Boss 不超过预算的该倍数时仍出 1 只（避免巢穴永远无 Boss）。</summary>
        private const float FallbackBudgetTolerance = 1.5f;

        private static List<PawnKindDef> scanCache;

        /// <summary>全库自动扫描（按 AutoScanMinCombatPower 缓存一次；更高门槛从缓存过滤）。</summary>
        public static List<PawnKindDef> AutoScan(float minCombatPower)
        {
            if (scanCache == null)
            {
                scanCache = new List<PawnKindDef>();
                foreach (PawnKindDef kind in DefDatabase<PawnKindDef>.AllDefsListForReading)
                {
                    if (kind?.RaceProps == null || !kind.isFighter)
                        continue;
                    if (kind.combatPower >= AutoScanMinCombatPower)
                        scanCache.Add(kind);
                }
                // 战力降序；同战力按 defName 稳定排序
                scanCache.Sort((a, b) =>
                {
                    int cmp = b.combatPower.CompareTo(a.combatPower);
                    return cmp != 0 ? cmp : string.CompareOrdinal(a.defName, b.defName);
                });
            }
            if (minCombatPower <= AutoScanMinCombatPower)
                return scanCache;
            return scanCache.FindAll(k => k.combatPower >= minCombatPower);
        }

        /// <summary>候选池 = XML 白名单（惰性解析，缺失静默跳过）∪ 自动扫描，按 defName 去重。</summary>
        public static List<PawnKindDef> Candidates(IEnumerable<string> whitelist, float minCombatPower)
        {
            Dictionary<string, PawnKindDef> kinds = new Dictionary<string, PawnKindDef>();
            if (whitelist != null)
            {
                foreach (string name in whitelist)
                {
                    if (name.NullOrEmpty())
                        continue;
                    PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(name);
                    if (kind != null)
                        kinds[kind.defName] = kind;
                }
            }
            foreach (PawnKindDef kind in AutoScan(minCombatPower))
                kinds[kind.defName] = kind;
            List<PawnKindDef> list = new List<PawnKindDef>(kinds.Values);
            list.Sort((a, b) =>
            {
                int cmp = b.combatPower.CompareTo(a.combatPower);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.defName, b.defName);
            });
            return list;
        }

        /// <summary>
        /// 按威胁预算选取 Boss（可多只）：逐只抽选，单只 combatPower ≤ 剩余预算，
        /// 数量上限 countRange（实际只数受预算约束）；权重偏向战力贴近剩余预算者（财富越高越容易出强 Boss），
        /// 已选种类不重复（保证多 Boss 时有变化）。预算抽不出任何 Boss 返回空表（调用方静默退化为大型据点）。
        /// </summary>
        public static List<PawnKindDef> PickBosses(float budget, IntRange countRange, IEnumerable<string> whitelist)
        {
            List<PawnKindDef> picked = new List<PawnKindDef>();
            if (budget <= 0f)
                return picked;
            List<PawnKindDef> pool = Candidates(whitelist, AutoScanMinCombatPower);
            if (pool.Count == 0)
                return picked;
            int maxCount = Mathf.Max(1, countRange.RandomInRange);
            float spent = 0f;
            for (int i = 0; i < maxCount && pool.Count > 0; i++)
            {
                float remaining = budget - spent;
                List<PawnKindDef> eligible = pool.FindAll(k => k.combatPower <= remaining);
                if (eligible.Count == 0 && i == 0)
                {
                    // 低财富兜底：最弱候选在容忍度内仍出 1 只
                    PawnKindDef cheapest = null;
                    foreach (PawnKindDef k in pool)
                    {
                        if (cheapest == null || k.combatPower < cheapest.combatPower)
                            cheapest = k;
                    }
                    if (cheapest != null && cheapest.combatPower <= budget * FallbackBudgetTolerance)
                        eligible.Add(cheapest);
                }
                if (eligible.Count == 0)
                    break;
                float rem = remaining;
                PawnKindDef pick = eligible.RandomElementByWeight(
                    k => 1f / (1f + Mathf.Abs(k.combatPower - rem) / Mathf.Max(rem, 100f)));
                picked.Add(pick);
                pool.Remove(pick);
                spent += pick.combatPower;
            }
            return picked;
        }

        /// <summary>
        /// Boss 派系解析：类人 → 据点宿主派系（随守卫 lord 行动）；机械族/虫族/异常实体挂各自原版派系
        /// （原生对玩家敌对）；动物等返回 null（调用方强制永久狂暴兜底）。全程不自写判敌逻辑。
        /// </summary>
        public static Faction ResolveBossFaction(PawnKindDef kind, Faction siteFaction)
        {
            if (kind?.RaceProps == null)
                return null;
            if (kind.RaceProps.Humanlike)
                return siteFaction;
            if (kind.RaceProps.IsMechanoid)
                return Faction.OfMechanoids;
            if (kind.RaceProps.Insect)
                return Find.FactionManager.FirstFactionOfDef(FactionDefOf.Insect);
            if (ModsConfig.AnomalyActive && kind.RaceProps.IsAnomalyEntity)
                return Faction.OfEntities;
            return null;
        }
    }
}
