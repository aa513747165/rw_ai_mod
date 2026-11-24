using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // 支柱二·天赋树（mmt-class-design.md v2，仿 Isekai PassiveTreeDef）：
    // MMT_TalentTreeDef 由生成器 tools/build_mmt_talent_trees.py 产出（生成物，勿手改）；
    // 一棵通用树（含旧 19 perk + 家族解锁 Keystone）+ 每职业家族一棵树（技能/强化/被动节点）。
    // 数据与效果分离：comp 只存 "treeDef:nodeId" 字符串；被动走 Hediff、技能走 pawn.abilities，零聚合缓存。

    /// <summary>节点类型：Start=树入口（0 费）；ClassUnlock=通用树家族解锁 Keystone；Skill/SkillUpgrade=职业技能/强化。</summary>
    public enum TalentNodeType
    {
        Start,
        Minor,
        Notable,
        Keystone,
        ClassUnlock,
        Skill,
        SkillUpgrade,
    }

    public enum TalentTreeKind
    {
        General,
        Family,
    }

    /// <summary>天赋节点（非 Def，挂在 MMT_TalentTreeDef.nodes 里）。</summary>
    public class TalentNodeRecord
    {
        public string nodeId;
        public string label;
        public string description;
        public TalentNodeType nodeType = TalentNodeType.Minor;
        /// <summary>图标贴图路径（相对 Textures，无扩展名）；空则由 UI 按类型画默认形状。</summary>
        public string icon;
        public float x;
        public float y;
        public int cost = 1;
        /// <summary>被动载体：解锁即 AddHediff（旧 perk Hediff 复用此字段节点化）。</summary>
        public HediffDef hediff;
        /// <summary>技能节点授予的技能（直接 pawn.abilities.GainAbility）。</summary>
        public List<AbilityDef> abilities;
        /// <summary>Skill/SkillUpgrade：限定职业基底名；ClassUnlock 节点不用此字段。</summary>
        public string classBaseName;
        /// <summary>ClassUnlock：点出后解锁的职业家族 key（写入 comp.unlockedFamilies）。</summary>
        public string familyKey;
        /// <summary>要求职业阶位（0=不限，2/3=需已升 II/III 阶）。</summary>
        public int requireTier;
        /// <summary>true=所有相邻节点都解锁后才可点（汇聚节点）。</summary>
        public bool requireAllConnected;

        /// <summary>所属树（ResolveReferences 回填，运行时缓存）。</summary>
        [Unsaved] public MMT_TalentTreeDef tree;
        /// <summary>预拼接的 "treeDefName:nodeId" 查询键（ResolveReferences 回填，避免运行时反复拼字符串）。</summary>
        [Unsaved] public string key;
    }

    /// <summary>天赋树 Def：节点 + "A~B" 无向连线字符串，ResolveReferences 建邻接缓存。</summary>
    public class MMT_TalentTreeDef : Def
    {
        public TalentTreeKind treeKind = TalentTreeKind.General;
        /// <summary>家族树标识（通用树为空）。</summary>
        public string familyKey;
        /// <summary>家族树成员职业基底名（生成器从映射表 classes.family 归纳）。</summary>
        public List<string> memberClasses = new List<string>();
        public List<TalentNodeRecord> nodes = new List<TalentNodeRecord>();
        /// <summary>无向边，"NodeA~NodeB" 格式。</summary>
        public List<string> connections = new List<string>();

        private Dictionary<string, TalentNodeRecord> nodeMap;
        private Dictionary<string, List<TalentNodeRecord>> adjacency;
        /// <summary>去重连线对缓存（ResolveReferences 构建一次，UI 每帧直接遍历，零分配）。</summary>
        private List<(TalentNodeRecord a, TalentNodeRecord b)> connectionPairsCache;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            nodeMap = new Dictionary<string, TalentNodeRecord>();
            adjacency = new Dictionary<string, List<TalentNodeRecord>>();
            foreach (TalentNodeRecord n in nodes)
            {
                n.tree = this;
                n.key = defName + ":" + n.nodeId;
                if (!string.IsNullOrEmpty(n.nodeId) && !nodeMap.ContainsKey(n.nodeId))
                {
                    nodeMap.Add(n.nodeId, n);
                }
            }
            HashSet<string> seenPairs = new HashSet<string>();
            connectionPairsCache = new List<(TalentNodeRecord, TalentNodeRecord)>();
            foreach (string conn in connections)
            {
                int sep = conn.IndexOf('~');
                if (sep <= 0 || sep >= conn.Length - 1)
                {
                    continue;
                }
                string idA = conn.Substring(0, sep);
                string idB = conn.Substring(sep + 1);
                TalentNodeRecord a = Node(idA);
                TalentNodeRecord b = Node(idB);
                if (a == null || b == null || a == b)
                {
                    continue;
                }
                if (!adjacency.TryGetValue(a.nodeId, out List<TalentNodeRecord> la))
                {
                    la = adjacency[a.nodeId] = new List<TalentNodeRecord>();
                }
                if (!la.Contains(b))
                {
                    la.Add(b);
                }
                if (!adjacency.TryGetValue(b.nodeId, out List<TalentNodeRecord> lb))
                {
                    lb = adjacency[b.nodeId] = new List<TalentNodeRecord>();
                }
                if (!lb.Contains(a))
                {
                    lb.Add(a);
                }
                if (seenPairs.Add(string.CompareOrdinal(idA, idB) < 0 ? idA + "~" + idB : idB + "~" + idA))
                {
                    connectionPairsCache.Add((a, b));
                }
            }
        }

        public TalentNodeRecord Node(string nodeId)
        {
            return nodeId != null && nodeMap != null && nodeMap.TryGetValue(nodeId, out TalentNodeRecord n) ? n : null;
        }

        private static readonly List<TalentNodeRecord> EmptyNeighbors = new List<TalentNodeRecord>();

        public IReadOnlyList<TalentNodeRecord> Neighbors(TalentNodeRecord node)
        {
            if (node != null && adjacency != null && adjacency.TryGetValue(node.nodeId, out List<TalentNodeRecord> list))
            {
                return list;
            }
            return EmptyNeighbors;
        }

        /// <summary>去重后的连线对（ResolveReferences 预构建，UI 每帧零分配遍历）。</summary>
        public IReadOnlyList<(TalentNodeRecord a, TalentNodeRecord b)> ConnectionPairs()
        {
            return connectionPairsCache ?? (IReadOnlyList<(TalentNodeRecord, TalentNodeRecord)>)new List<(TalentNodeRecord, TalentNodeRecord)>();
        }

        public TalentNodeRecord StartNode
        {
            get { return nodes.FirstOrDefault(n => n.nodeType == TalentNodeType.Start); }
        }
    }

    /// <summary>节点状态（UI 着色用）。</summary>
    public enum TalentNodeState
    {
        Locked,
        Available,
        Unlocked,
    }

    /// <summary>
    /// 天赋树运行时：解锁门禁/效果施加/链式解锁/就职联动/旧档迁移。
    /// 状态全部存在 CompPawnProgress（talentPoints/unlockedTalents/unlockedFamilies）。
    /// </summary>
    public static class TalentTreeUtil
    {
        public static string KeyOf(MMT_TalentTreeDef tree, TalentNodeRecord node)
        {
            return tree.defName + ":" + node.nodeId;
        }

        public static IEnumerable<MMT_TalentTreeDef> AllTrees
        {
            get { return DefDatabase<MMT_TalentTreeDef>.AllDefs; }
        }

        public static MMT_TalentTreeDef GeneralTree
        {
            get { return AllTrees.FirstOrDefault(t => t.treeKind == TalentTreeKind.General); }
        }

        public static MMT_TalentTreeDef FamilyTreeOf(string familyKey)
        {
            return familyKey == null ? null : AllTrees.FirstOrDefault(t => t.treeKind == TalentTreeKind.Family && t.familyKey == familyKey);
        }

        /// <summary>职业基底名 → 所属家族 key（查家族树 memberClasses，无匹配返回 null）。</summary>
        public static string FamilyOfClass(string baseName)
        {
            if (baseName == null)
            {
                return null;
            }
            foreach (MMT_TalentTreeDef tree in AllTrees)
            {
                if (tree.treeKind == TalentTreeKind.Family && tree.memberClasses.Contains(baseName))
                {
                    return tree.familyKey;
                }
            }
            return null;
        }

        /// <summary>小人可见的家族树（仅自己职业那棵；未就职为 null）。</summary>
        public static MMT_TalentTreeDef FamilyTreeFor(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp?.classDef == null)
            {
                return null;
            }
            return FamilyTreeOf(FamilyOfClass(PawnClassUtil.BaseNameOf(comp.classDef)));
        }

        public static bool IsUnlocked(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord node)
        {
            if (comp == null || node == null)
            {
                return false;
            }
            // node.key 为 ResolveReferences 预拼接键（零分配）；异常缺失时回退现拼
            return comp.UnlockedSet.Contains(node.key ?? KeyOf(tree, node));
        }

        /// <summary>该家族是否已解锁就职资格（通用树 ClassUnlock 节点写入）。</summary>
        public static bool FamilyUnlocked(CompPawnProgress comp, string familyKey)
        {
            return comp != null && familyKey != null && comp.unlockedFamilies.Contains(familyKey);
        }

        public static TalentNodeState StateOf(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord node)
        {
            if (IsUnlocked(comp, tree, node))
            {
                return TalentNodeState.Unlocked;
            }
            return CanUnlock(comp, tree, node, out _) ? TalentNodeState.Available : TalentNodeState.Locked;
        }

        /// <summary>解锁门禁（不含点数检查以外的副作用）。failReason 供 UI/消息显示。</summary>
        public static bool CanUnlock(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord node, out string failReason)
        {
            failReason = null;
            Pawn pawn = comp?.Pawn;
            if (comp == null || pawn == null || tree == null || node == null)
            {
                failReason = "数据缺失";
                return false;
            }
            if (IsUnlocked(comp, tree, node))
            {
                failReason = "已解锁";
                return false;
            }
            if (tree.treeKind == TalentTreeKind.Family)
            {
                // 家族树仅本职业小人可点
                string myFamily = comp.classDef != null ? FamilyOfClass(PawnClassUtil.BaseNameOf(comp.classDef)) : null;
                if (myFamily != tree.familyKey)
                {
                    failReason = "非本职业天赋树";
                    return false;
                }
            }
            if ((node.nodeType == TalentNodeType.Skill || node.nodeType == TalentNodeType.SkillUpgrade)
                && !string.IsNullOrEmpty(node.classBaseName))
            {
                string myClass = comp.classDef != null ? PawnClassUtil.BaseNameOf(comp.classDef) : null;
                if (myClass != node.classBaseName)
                {
                    failReason = "限「" + (PawnClassUtil.FindByBaseName(node.classBaseName)?.label ?? node.classBaseName) + "」";
                    return false;
                }
            }
            if (node.requireTier > 0 && PawnClassUtil.CurrentTier(pawn) < node.requireTier)
            {
                failReason = "需要职业 " + (node.requireTier == 2 ? "II" : "III") + " 阶";
                return false;
            }
            if (node.nodeType != TalentNodeType.Start)
            {
                IReadOnlyList<TalentNodeRecord> neighbors = tree.Neighbors(node);
                // 手写循环：UI 每帧调用路径避免 LINQ 闭包分配
                int unlockedCount = 0;
                foreach (TalentNodeRecord neighbor in neighbors)
                {
                    if (IsUnlocked(comp, tree, neighbor))
                    {
                        unlockedCount++;
                    }
                }
                if (node.requireAllConnected)
                {
                    if (neighbors.Count == 0 || unlockedCount < neighbors.Count)
                    {
                        failReason = "需要相邻节点全部解锁";
                        return false;
                    }
                }
                else if (unlockedCount == 0)
                {
                    failReason = "需要相邻节点已解锁";
                    return false;
                }
            }
            if (comp.talentPoints < node.cost)
            {
                failReason = "天赋点不足（需 " + node.cost + " 点）";
                return false;
            }
            return true;
        }

        /// <summary>解锁节点：扣点、登记、施加效果（Hediff/技能/家族解锁）。free=true 不扣点（迁移/就职联动）。</summary>
        public static bool Unlock(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord node, bool free = false)
        {
            Pawn pawn = comp?.Pawn;
            if (pawn == null || tree == null || node == null || IsUnlocked(comp, tree, node))
            {
                return false;
            }
            if (!free && !CanUnlock(comp, tree, node, out _))
            {
                return false;
            }
            if (!free)
            {
                comp.talentPoints -= node.cost;
            }
            string key = node.key ?? KeyOf(tree, node);
            comp.unlockedTalents.Add(key);
            comp.UnlockedSet.Add(key);
            if (node.hediff != null && pawn.health.hediffSet.GetFirstHediffOfDef(node.hediff) == null)
            {
                pawn.health.AddHediff(node.hediff);
            }
            if (node.abilities != null && pawn.abilities != null)
            {
                foreach (AbilityDef ability in node.abilities)
                {
                    if (ability != null && pawn.abilities.GetAbility(ability) == null)
                    {
                        pawn.abilities.GainAbility(ability);
                    }
                }
            }
            if (node.nodeType == TalentNodeType.ClassUnlock && !string.IsNullOrEmpty(node.familyKey)
                && !comp.unlockedFamilies.Contains(node.familyKey))
            {
                comp.unlockedFamilies.Add(node.familyKey);
                MMT_TalentTreeDef familyTree = FamilyTreeOf(node.familyKey);
                Messages.Message(pawn.LabelShort + " 领悟了「" + (familyTree?.LabelCap.ToString() ?? node.familyKey)
                    + "」！现在可以在职业导师台就职该家族职业。", pawn, MessageTypeDefOf.PositiveEvent);
            }
            return true;
        }

        /// <summary>就职联动：自动免费解锁本家族树 Start 节点。</summary>
        public static void OnClassAttuned(Pawn pawn, string baseName)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            MMT_TalentTreeDef tree = FamilyTreeOf(FamilyOfClass(baseName));
            TalentNodeRecord start = tree?.StartNode;
            if (comp != null && tree != null && start != null)
            {
                Unlock(comp, tree, start, free: true);
            }
        }

        /// <summary>Dijkstra 最短路：从任一已解锁节点到目标节点，返回沿途未解锁节点（含目标，按顺序）。</summary>
        public static List<TalentNodeRecord> FindUnlockChain(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord target)
        {
            if (comp == null || tree == null || target == null || IsUnlocked(comp, tree, target))
            {
                return null;
            }
            // 距离 = 进入节点的 cost 之和；起点为全部已解锁节点（距离 0）
            Dictionary<string, int> dist = new Dictionary<string, int>();
            Dictionary<string, string> prev = new Dictionary<string, string>();
            List<TalentNodeRecord> frontier = new List<TalentNodeRecord>();
            foreach (TalentNodeRecord n in tree.nodes)
            {
                if (IsUnlocked(comp, tree, n))
                {
                    dist[n.nodeId] = 0;
                    frontier.Add(n);
                }
            }
            if (frontier.Count == 0)
            {
                return null;
            }
            HashSet<string> settled = new HashSet<string>();
            while (frontier.Count > 0)
            {
                TalentNodeRecord current = frontier[0];
                foreach (TalentNodeRecord n in frontier)
                {
                    if (dist[n.nodeId] < dist[current.nodeId])
                    {
                        current = n;
                    }
                }
                frontier.Remove(current);
                if (!settled.Add(current.nodeId))
                {
                    continue;
                }
                if (current == target)
                {
                    break;
                }
                foreach (TalentNodeRecord next in tree.Neighbors(current))
                {
                    int nd = dist[current.nodeId] + (IsUnlocked(comp, tree, next) ? 0 : next.cost);
                    if (!dist.TryGetValue(next.nodeId, out int old) || nd < old)
                    {
                        dist[next.nodeId] = nd;
                        prev[next.nodeId] = current.nodeId;
                        if (!frontier.Contains(next))
                        {
                            frontier.Add(next);
                        }
                    }
                }
            }
            if (!prev.ContainsKey(target.nodeId) && !IsUnlocked(comp, tree, target))
            {
                return null;
            }
            List<TalentNodeRecord> chain = new List<TalentNodeRecord>();
            string cursor = target.nodeId;
            while (cursor != null && !IsUnlocked(comp, tree, tree.Node(cursor)))
            {
                TalentNodeRecord n = tree.Node(cursor);
                if (n == null)
                {
                    return null;
                }
                chain.Insert(0, n);
                prev.TryGetValue(cursor, out cursor);
            }
            return chain;
        }

        /// <summary>双击加点：可直接点走单点解锁；深层节点链式一次点完（每个节点仍走完整门禁）。</summary>
        public static void TryUnlockWithChain(CompPawnProgress comp, MMT_TalentTreeDef tree, TalentNodeRecord target)
        {
            Pawn pawn = comp?.Pawn;
            if (pawn == null)
            {
                return;
            }
            if (CanUnlock(comp, tree, target, out string reason))
            {
                if (Unlock(comp, tree, target))
                {
                    SoundDefOf.Lesson_Activated?.PlayOneShotOnCamera();
                }
                return;
            }
            if (reason == "已解锁")
            {
                return;
            }
            List<TalentNodeRecord> chain = FindUnlockChain(comp, tree, target);
            if (chain == null || chain.Count == 0)
            {
                Messages.Message("无法解锁「" + target.label + "」：" + reason, MessageTypeDefOf.RejectInput, false);
                return;
            }
            int total = chain.Sum(n => n.cost);
            if (comp.talentPoints < total)
            {
                Messages.Message("链式解锁「" + target.label + "」需要 " + total + " 天赋点（当前 " + comp.talentPoints + " 点）",
                    MessageTypeDefOf.RejectInput, false);
                return;
            }
            foreach (TalentNodeRecord node in chain)
            {
                if (!CanUnlock(comp, tree, node, out string stepReason))
                {
                    Messages.Message("链式解锁中断于「" + node.label + "」：" + stepReason, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                Unlock(comp, tree, node);
            }
            SoundDefOf.Lesson_Activated?.PlayOneShotOnCamera();
            Messages.Message(pawn.LabelShort + " 链式解锁了 " + chain.Count + " 个天赋节点（终点：" + target.label + "）",
                pawn, MessageTypeDefOf.PositiveEvent);
        }

        /// <summary>全退（Debug 洗点）：移除全部节点效果并返还点数；随后重发通用树 Start 与本家族 Start。</summary>
        public static void RefundAll(CompPawnProgress comp)
        {
            Pawn pawn = comp?.Pawn;
            if (pawn == null)
            {
                return;
            }
            int refund = 0;
            foreach (string key in comp.unlockedTalents.ToList())
            {
                int sep = key.IndexOf(':');
                if (sep <= 0)
                {
                    continue;
                }
                MMT_TalentTreeDef tree = DefDatabase<MMT_TalentTreeDef>.GetNamedSilentFail(key.Substring(0, sep));
                TalentNodeRecord node = tree?.Node(key.Substring(sep + 1));
                if (node == null)
                {
                    continue;
                }
                refund += node.cost;
                if (node.hediff != null)
                {
                    Hediff h = pawn.health.hediffSet.GetFirstHediffOfDef(node.hediff);
                    if (h != null)
                    {
                        pawn.health.RemoveHediff(h);
                    }
                }
                if (node.abilities != null && pawn.abilities != null)
                {
                    foreach (AbilityDef ability in node.abilities)
                    {
                        Ability a = pawn.abilities.GetAbility(ability);
                        if (a != null)
                        {
                            pawn.abilities.RemoveAbility(ability);
                        }
                    }
                }
            }
            comp.talentPoints += refund;
            comp.unlockedTalents.Clear();
            comp.unlockedFamilies.Clear();
            comp.InvalidateTalentCache();
            EnsureStartNodes(comp);
        }

        /// <summary>确保通用树 Start 与（已就职时）本家族树 Start 已免费解锁。</summary>
        public static void EnsureStartNodes(CompPawnProgress comp)
        {
            MMT_TalentTreeDef general = GeneralTree;
            if (general?.StartNode != null)
            {
                Unlock(comp, general, general.StartNode, free: true);
            }
            if (comp?.classDef != null)
            {
                OnClassAttuned(comp.Pawn, PawnClassUtil.BaseNameOf(comp.classDef));
            }
        }

        /// <summary>
        /// 旧档迁移（幂等）：旧 perk → 通用树节点标记；剩余 perk 点 1:1 转天赋点；
        /// 已就职小人按当前阶位补发全部基础技能节点（阶 Hediff 去 abilities 后保技能不丢）。
        /// </summary>
        public static void Migrate(CompPawnProgress comp)
        {
            Pawn pawn = comp?.Pawn;
            if (pawn == null || comp.perkMigrated)
            {
                return;
            }
            comp.perkMigrated = true;
            // 旧 perk → 节点标记（Hediff 已在身上，只补登记）
            if (comp.perks.Count > 0)
            {
                MMT_TalentTreeDef general = GeneralTree;
                if (general != null)
                {
                    foreach (TalentNodeRecord node in general.nodes)
                    {
                        if (node.hediff != null && comp.perks.Contains(node.hediff))
                        {
                            string key = KeyOf(general, node);
                            if (!comp.unlockedTalents.Contains(key))
                            {
                                comp.unlockedTalents.Add(key);
                            }
                        }
                    }
                    comp.InvalidateTalentCache();
                }
            }
            // 剩余 perk 点 1:1 转天赋点
            if (comp.perkPoints > 0)
            {
                comp.talentPoints += comp.perkPoints;
                comp.perkPoints = 0;
            }
            // 已就职：补发家族 Start + 当前阶位应得的全部基础技能节点
            if (comp.classDef != null)
            {
                string baseName = PawnClassUtil.BaseNameOf(comp.classDef);
                int tier = PawnClassUtil.CurrentTier(pawn);
                MMT_TalentTreeDef family = FamilyTreeOf(FamilyOfClass(baseName));
                if (family != null)
                {
                    foreach (TalentNodeRecord node in family.nodes)
                    {
                        if (node.nodeType == TalentNodeType.Start
                            || (node.nodeType == TalentNodeType.Skill
                                && (string.IsNullOrEmpty(node.classBaseName) || node.classBaseName == baseName)
                                && node.requireTier <= tier))
                        {
                            Unlock(comp, family, node, free: true);
                        }
                    }
                }
            }
            EnsureStartNodes(comp);
        }
    }

    /// <summary>天赋树 Debug 工具（加点/洗点/解锁全家族）。</summary>
    public static class TalentTreeDebugActions
    {
        [DebugAction("MMT 职业", "天赋点 +10（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveTalentPoints(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp != null)
            {
                comp.talentPoints += 10;
            }
        }

        [DebugAction("MMT 职业", "天赋全退（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RespecTalents(Pawn pawn)
        {
            TalentTreeUtil.RefundAll(CompPawnProgress.Get(pawn));
        }

        [DebugAction("MMT 职业", "解锁全部职业家族（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void UnlockAllFamilies(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp == null)
            {
                return;
            }
            foreach (MMT_TalentTreeDef tree in TalentTreeUtil.AllTrees)
            {
                if (tree.treeKind == TalentTreeKind.Family && !comp.unlockedFamilies.Contains(tree.familyKey))
                {
                    comp.unlockedFamilies.Add(tree.familyKey);
                }
            }
        }
    }
}
