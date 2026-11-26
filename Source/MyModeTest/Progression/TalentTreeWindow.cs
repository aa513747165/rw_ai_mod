using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // 支柱二·天赋树窗口（mmt-class-design.md v2）：本 Mod 第一个自定义 Window。
    // 布局：左侧树页签 / 中间画布（平移缩放）/ 右侧节点详情 / 底部天赋点；
    // 连线用 Widgets.DrawLine + Cohen–Sutherland 画布裁剪（GL 不吃 GUI.BeginGroup 裁剪，须手工裁）；
    // 节点形状/图标贴图复制自已授权的 Isekai（生成器落盘 1.7/Textures/UI/TalentTree/，缺失时色块兜底）。

    /// <summary>天赋树贴图（ContentFinder 空安全加载，缺图返回 null 走色块兜底）。</summary>
    public static class TalentTextures
    {
        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (!cache.TryGetValue(path, out Texture2D tex))
            {
                tex = ContentFinder<Texture2D>.Get(path, false);
                cache[path] = tex;
            }
            return tex;
        }

        /// <summary>节点形状框：按类型取 Isekai 授权贴图（Node{Minor|Notable|Keystone|Start}[Active]）。</summary>
        public static Texture2D NodeShape(TalentNodeType type, bool active)
        {
            string name;
            switch (type)
            {
                case TalentNodeType.Start:
                    name = "NodeStart";
                    break;
                case TalentNodeType.Notable:
                case TalentNodeType.Skill:
                    name = "NodeNotable";
                    break;
                case TalentNodeType.Keystone:
                case TalentNodeType.ClassUnlock:
                    name = "NodeKeystone";
                    break;
                default:
                    name = "NodeMinor";
                    break;
            }
            return Get("UI/TalentTree/" + name + (active ? "Active" : ""));
        }
    }

    /// <summary>天赋树窗口：左侧页签 + 中间画布 + 右侧详情 + 底部点数。</summary>
    public class Window_TalentTree : Window
    {
        private const float GridScale = 70f;
        private const float TabWidth = 150f;
        private const float DetailWidth = 280f;
        private const float FooterHeight = 36f;

        private readonly Pawn pawn;
        private readonly CompPawnProgress comp;
        private MMT_TalentTreeDef currentTree;
        private TalentNodeRecord selected;
        private Vector2 pan;
        private float zoom = 1f;
        private bool dragging;
        private Vector2 detailScroll;
        private readonly Dictionary<MMT_TalentTreeDef, Vector2> panByTree = new Dictionary<MMT_TalentTreeDef, Vector2>();
        private readonly Dictionary<MMT_TalentTreeDef, float> zoomByTree = new Dictionary<MMT_TalentTreeDef, float>();
        /// <summary>详情面板链式解锁的 Dijkstra 缓存：仅选中节点/天赋点变化时重算（否则每帧一次最短路）。</summary>
        private TalentNodeRecord chainCachedFor;
        private int chainCachedPoints = -1;
        private List<TalentNodeRecord> chainCache;

        public Window_TalentTree(Pawn pawn)
        {
            this.pawn = pawn;
            comp = CompPawnProgress.Get(pawn);
            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            currentTree = TalentTreeUtil.GeneralTree ?? TalentTreeUtil.AllTrees.FirstOrDefault();
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.85f); }
        }

        private List<MMT_TalentTreeDef> VisibleTrees()
        {
            List<MMT_TalentTreeDef> list = new List<MMT_TalentTreeDef>();
            MMT_TalentTreeDef general = TalentTreeUtil.GeneralTree;
            if (general != null)
            {
                list.Add(general);
            }
            MMT_TalentTreeDef family = TalentTreeUtil.FamilyTreeFor(pawn);
            if (family != null)
            {
                list.Add(family);
            }
            return list;
        }

        private Vector2 GridToLocal(TalentNodeRecord node, Rect canvas)
        {
            return new Vector2(canvas.width / 2f, canvas.height / 2f)
                + new Vector2(node.x, -node.y) * (GridScale * zoom) + pan;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (comp == null || currentTree == null)
            {
                Widgets.Label(inRect, "天赋树数据缺失（生成器未运行？）");
                return;
            }
            if (pawn == null || pawn.Dead || !pawn.Spawned)
            {
                Close();
                return;
            }

            Rect tabRect = new Rect(inRect.x, inRect.y, TabWidth, inRect.height - FooterHeight);
            Rect detailRect = new Rect(inRect.xMax - DetailWidth, inRect.y, DetailWidth, inRect.height - FooterHeight);
            Rect canvasRect = new Rect(tabRect.xMax + 4f, inRect.y, detailRect.x - tabRect.xMax - 8f, inRect.height - FooterHeight);
            Rect footerRect = new Rect(inRect.x, inRect.yMax - FooterHeight, inRect.width, FooterHeight);

            DrawTabs(tabRect);
            DrawCanvas(canvasRect);
            DrawDetail(detailRect);
            DrawFooter(footerRect);
        }

        // —— 左侧树页签 ——

        private void DrawTabs(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect.ContractedBy(8f));
            foreach (MMT_TalentTreeDef tree in VisibleTrees())
            {
                bool cur = tree == currentTree;
                if (listing.RadioButton(tree.LabelCap + (tree.treeKind == TalentTreeKind.General ? "（通用）" : ""), cur))
                {
                    if (currentTree != null)
                    {
                        panByTree[currentTree] = pan;
                        zoomByTree[currentTree] = zoom;
                    }
                    currentTree = tree;
                    selected = null;
                    pan = panByTree.TryGetValue(tree, out Vector2 p) ? p : Vector2.zero;
                    zoom = zoomByTree.TryGetValue(tree, out float z) ? z : 1f;
                }
            }
            listing.Gap(12f);
            int unlocked = 0;
            foreach (TalentNodeRecord n in currentTree.nodes)
            {
                if (TalentTreeUtil.IsUnlocked(comp, currentTree, n))
                {
                    unlocked++;
                }
            }
            listing.Label("本树进度：" + unlocked + " / " + currentTree.nodes.Count);
            listing.End();
        }

        // —— 中间画布 ——

        private void DrawCanvas(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.08f, 0.1f, 0.9f));
            Widgets.DrawBox(rect);
            HandlePanZoom(rect);
            // 连线：屏幕坐标 = 画布原点 + 局部坐标，Cohen–Sutherland 裁进画布
            foreach ((TalentNodeRecord a, TalentNodeRecord b) in currentTree.ConnectionPairs())
            {
                Vector2 pa = rect.position + GridToLocal(a, rect);
                Vector2 pb = rect.position + GridToLocal(b, rect);
                if (!ClipSegment(rect, ref pa, ref pb))
                {
                    continue;
                }
                bool ua = TalentTreeUtil.IsUnlocked(comp, currentTree, a);
                bool ub = TalentTreeUtil.IsUnlocked(comp, currentTree, b);
                Color color;
                if (ua && ub)
                {
                    color = new Color(1f, 0.85f, 0.3f);
                }
                else if (ua || ub)
                {
                    color = new Color(0.55f, 0.55f, 0.6f);
                }
                else
                {
                    color = new Color(0.28f, 0.28f, 0.32f);
                }
                Widgets.DrawLine(pa, pb, color, (ua && ub ? 2.5f : 1.5f) * zoom);
            }
            // 节点：GUI.BeginGroup 内绘制，天然裁剪
            GUI.BeginGroup(rect);
            foreach (TalentNodeRecord node in currentTree.nodes)
            {
                Vector2 local = GridToLocal(node, rect);
                float size = NodeSize(node.nodeType) * zoom;
                Rect nodeRect = new Rect(local.x - size / 2f, local.y - size / 2f, size, size);
                if (nodeRect.xMax < 0f || nodeRect.yMax < 0f || nodeRect.x > rect.width || nodeRect.y > rect.height)
                {
                    continue;
                }
                DrawNode(node, nodeRect, rect);
            }
            GUI.EndGroup();
        }

        private static float NodeSize(TalentNodeType type)
        {
            switch (type)
            {
                case TalentNodeType.Start:
                case TalentNodeType.Keystone:
                case TalentNodeType.ClassUnlock:
                    return 56f;
                case TalentNodeType.Notable:
                case TalentNodeType.Skill:
                    return 44f;
                default:
                    return 32f;
            }
        }

        private void DrawNode(TalentNodeRecord node, Rect nodeRect, Rect canvas)
        {
            TalentNodeState state = TalentTreeUtil.StateOf(comp, currentTree, node);
            bool unlocked = state == TalentNodeState.Unlocked;
            Texture2D shape = TalentTextures.NodeShape(node.nodeType, unlocked);
            Color tint;
            if (unlocked)
            {
                tint = Color.white;
            }
            else if (state == TalentNodeState.Available)
            {
                tint = new Color(1f, 0.9f, 0.5f);
            }
            else
            {
                tint = new Color(0.45f, 0.45f, 0.5f);
            }
            GUI.color = tint;
            if (shape != null)
            {
                GUI.DrawTexture(nodeRect, shape);
            }
            else
            {
                // 缺图兜底：色块
                GUI.DrawTexture(nodeRect, BaseContent.WhiteTex);
            }
            GUI.color = Color.white;
            // 图标
            Texture2D icon = TalentTextures.Get(node.icon);
            if (icon != null)
            {
                float iconSize = nodeRect.width * 0.62f;
                Rect iconRect = new Rect(nodeRect.center.x - iconSize / 2f, nodeRect.center.y - iconSize / 2f, iconSize, iconSize);
                GUI.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                GUI.DrawTexture(iconRect, icon);
                GUI.color = Color.white;
            }
            if (node == selected)
            {
                Widgets.DrawBox(nodeRect.ExpandedBy(3f), 2);
            }
            // 交互：单击选中，双击加点
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && nodeRect.Contains(Event.current.mousePosition))
            {
                selected = node;
                if (Event.current.clickCount == 2 && !unlocked)
                {
                    TalentTreeUtil.TryUnlockWithChain(comp, currentTree, node);
                }
                Event.current.Use();
            }
            if (nodeRect.Contains(Event.current.mousePosition))
            {
                string costText = node.cost > 0 ? "（" + node.cost + " 点）" : "";
                TooltipHandler.TipRegion(nodeRect, node.label + costText + "\n" + NodeStateText(state));
            }
        }

        private static string NodeStateText(TalentNodeState state)
        {
            return state == TalentNodeState.Unlocked ? "已解锁" : state == TalentNodeState.Available ? "可解锁（双击加点）" : "未解锁";
        }

        private void HandlePanZoom(Rect rect)
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
            {
                float old = zoom;
                zoom = Mathf.Clamp(zoom * (e.delta.y < 0f ? 1.12f : 0.89f), 0.35f, 2f);
                // 朝鼠标点缩放：保持鼠标下的网格点不动
                Vector2 mouse = e.mousePosition - rect.position - new Vector2(rect.width / 2f, rect.height / 2f);
                pan = mouse - (mouse - pan) * (zoom / old);
                e.Use();
            }
            if (e.type == EventType.MouseDown && (e.button == 1 || e.button == 2) && rect.Contains(e.mousePosition))
            {
                dragging = true;
                e.Use();
            }
            if (e.type == EventType.MouseDrag && dragging && (e.button == 1 || e.button == 2))
            {
                pan += e.delta;
                e.Use();
            }
            if (e.type == EventType.MouseUp && dragging && (e.button == 1 || e.button == 2))
            {
                dragging = false;
                e.Use();
            }
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                // 点空白处取消选中
                Vector2 local = e.mousePosition - rect.position;
                bool hitNode = currentTree.nodes.Any(n =>
                {
                    Vector2 c = GridToLocal(n, rect);
                    float r = NodeSize(n.nodeType) * zoom / 2f;
                    return Mathf.Abs(local.x - c.x) <= r && Mathf.Abs(local.y - c.y) <= r;
                });
                if (!hitNode)
                {
                    selected = null;
                }
            }
        }

        /// <summary>Cohen–Sutherland 线段裁剪到画布矩形（GL 画线不吃 GUI 裁剪，须手工裁）。</summary>
        private static bool ClipSegment(Rect rect, ref Vector2 a, ref Vector2 b)
        {
            const int L = 1, R = 2, B = 4, T = 8;
            int Code(Vector2 p)
            {
                int c = 0;
                if (p.x < rect.xMin) c |= L;
                else if (p.x > rect.xMax) c |= R;
                if (p.y < rect.yMin) c |= T;
                else if (p.y > rect.yMax) c |= B;
                return c;
            }
            int ca = Code(a), cb = Code(b);
            while (true)
            {
                if ((ca | cb) == 0)
                {
                    return true;
                }
                if ((ca & cb) != 0)
                {
                    return false;
                }
                int outCode = ca != 0 ? ca : cb;
                Vector2 p;
                if ((outCode & T) != 0)
                {
                    p = new Vector2(a.x + (b.x - a.x) * (rect.yMin - a.y) / (b.y - a.y), rect.yMin);
                }
                else if ((outCode & B) != 0)
                {
                    p = new Vector2(a.x + (b.x - a.x) * (rect.yMax - a.y) / (b.y - a.y), rect.yMax);
                }
                else if ((outCode & R) != 0)
                {
                    p = new Vector2(rect.xMax, a.y + (b.y - a.y) * (rect.xMax - a.x) / (b.x - a.x));
                }
                else
                {
                    p = new Vector2(rect.xMin, a.y + (b.y - a.y) * (rect.xMin - a.x) / (b.x - a.x));
                }
                if (outCode == ca)
                {
                    a = p;
                    ca = Code(a);
                }
                else
                {
                    b = p;
                    cb = Code(b);
                }
            }
        }

        // —— 右侧详情 ——

        private void DrawDetail(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(10f);
            if (selected == null)
            {
                Widgets.Label(inner, "「" + currentTree.LabelCap + "」\n\n" + currentTree.description
                    + "\n\n单击节点查看详情，双击解锁（深层节点自动沿最短路径链式解锁）。");
                return;
            }
            TalentNodeRecord node = selected;
            float contentHeight = 560f;
            Rect viewRect = new Rect(0f, 0f, inner.width - 16f, contentHeight);
            Widgets.BeginScrollView(inner, ref detailScroll, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            Text.Font = GameFont.Medium;
            listing.Label(node.label);
            Text.Font = GameFont.Small;
            listing.Label(NodeTypeText(node) + (node.cost > 0 ? " · 消耗 " + node.cost + " 天赋点" : " · 免费"));
            listing.Gap(6f);
            if (!string.IsNullOrEmpty(node.description))
            {
                listing.Label(node.description);
                listing.Gap(6f);
            }
            if (node.hediff != null)
            {
                listing.Label("效果：" + CompPawnProgress.DescribePerk(node.hediff));
            }
            if (node.abilities != null)
            {
                foreach (AbilityDef ability in node.abilities)
                {
                    if (ability != null)
                    {
                        listing.Label("获得技能：" + ability.LabelCap);
                    }
                }
            }
            if (node.nodeType == TalentNodeType.ClassUnlock && !string.IsNullOrEmpty(node.familyKey))
            {
                MMT_TalentTreeDef familyTree = TalentTreeUtil.FamilyTreeOf(node.familyKey);
                listing.Label("解锁就职资格：" + (familyTree?.LabelCap.ToString() ?? node.familyKey));
            }
            if (node.requireTier > 0)
            {
                listing.Label("需要职业阶位：" + (node.requireTier == 2 ? "II" : "III") + " 阶");
            }
            if (!string.IsNullOrEmpty(node.classBaseName))
            {
                listing.Label("限定职业：" + (PawnClassUtil.FindByBaseName(node.classBaseName)?.label ?? node.classBaseName));
            }
            listing.Gap(12f);

            TalentNodeState state = TalentTreeUtil.StateOf(comp, currentTree, node);
            if (state == TalentNodeState.Unlocked)
            {
                listing.Label("（已解锁）");
            }
            else
            {
                bool can = TalentTreeUtil.CanUnlock(comp, currentTree, node, out string reason);
                if (can)
                {
                    if (listing.ButtonText("解锁（" + node.cost + " 点）"))
                    {
                        TalentTreeUtil.Unlock(comp, currentTree, node);
                        SoundDefOf.Lesson_Activated?.PlayOneShotOnCamera();
                    }
                }
                else
                {
                    // Dijkstra 结果缓存：选中节点与天赋点不变时沿用（避免详情面板每帧一次最短路）
                    if (chainCachedFor != node || chainCachedPoints != comp.talentPoints)
                    {
                        chainCachedFor = node;
                        chainCachedPoints = comp.talentPoints;
                        chainCache = TalentTreeUtil.FindUnlockChain(comp, currentTree, node);
                    }
                    List<TalentNodeRecord> chain = chainCache;
                    int chainCost = 0;
                    if (chain != null)
                    {
                        foreach (TalentNodeRecord n in chain)
                        {
                            chainCost += n.cost;
                        }
                    }
                    if (chain != null && chain.Count > 1 && comp.talentPoints >= chainCost)
                    {
                        if (listing.ButtonText("链式解锁（共 " + chainCost + " 点 / " + chain.Count + " 节点）"))
                        {
                            TalentTreeUtil.TryUnlockWithChain(comp, currentTree, node);
                        }
                    }
                    else
                    {
                        listing.Label("（" + reason + "）");
                    }
                }
            }
            listing.End();
            Widgets.EndScrollView();
        }

        private static string NodeTypeText(TalentNodeRecord node)
        {
            switch (node.nodeType)
            {
                case TalentNodeType.Start: return "起点";
                case TalentNodeType.Notable: return "重要节点";
                case TalentNodeType.Keystone: return "基石";
                case TalentNodeType.ClassUnlock: return "职业解锁";
                case TalentNodeType.Skill: return "技能";
                case TalentNodeType.SkillUpgrade: return "技能强化";
                default: return "天赋节点";
            }
        }

        // —— 底部点数 ——

        private void DrawFooter(Rect rect)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
            Rect left = new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, rect.height - 8f);
            string text = "剩余天赋点：" + comp.talentPoints
                + "    历练等级：" + comp.level + " / " + CompPawnProgress.MaxLevel
                + "    右键拖拽平移 · 滚轮缩放 · 双击节点解锁";
            Widgets.Label(left, text);
        }
    }
}
