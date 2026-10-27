using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 研究窗口纵向滚动补丁。
    /// 1.7 原版 MainTabWindow_Research.DrawRightRect 的滚动视图 viewRect 高度写死为
    /// 可见高度（outRect.height - 16），与内容高度无关；窗口总高度又封顶 screenHeight - 35。
    /// 结果：右侧面板只有横向滚动，researchViewY 较大的研究（MMT 研究页 Y 用到 16）底部永远不可达。
    /// 本转译器把 viewRect 高度改为 max(原高度, 内容高度 + 20)，
    /// Widgets.BeginScrollView（内部走 GUI.BeginScrollView 3 参重载）会自动出现纵向滚动条。
    /// 匹配失败时只打警告、不改 IL，保证其他版本/其他研究界面 Mod 下安全降级。
    /// </summary>
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawRightRect")]
    public static class ResearchScrollPatch
    {
        private delegate Vector2 ViewSizeDelegate(MainTabWindow_Research window, ResearchTabDef tab);

        private static readonly ViewSizeDelegate ViewSizeFunc =
            AccessTools.MethodDelegate<ViewSizeDelegate>(
                AccessTools.Method(typeof(MainTabWindow_Research), "ViewSize"));

        private static readonly Func<MainTabWindow_Research, ResearchTabDef> CurTabGetter =
            AccessTools.MethodDelegate<Func<MainTabWindow_Research, ResearchTabDef>>(
                AccessTools.PropertyGetter(typeof(MainTabWindow_Research), "CurTab"));

        // 内容高度按页签缓存：研究项目坐标是 Def 静态数据，整个会话不变，每个页签只算一次
        private static readonly Dictionary<ResearchTabDef, Vector2> viewSizeCache =
            new Dictionary<ResearchTabDef, Vector2>();

        /// <summary>注入 helper：viewRect 高度取原版高度与内容实际高度的较大值（内容高度按页签缓存）。</summary>
        public static float EnsureContentHeight(float height, MainTabWindow_Research window)
        {
            try
            {
                ResearchTabDef tab = CurTabGetter(window);
                if (!viewSizeCache.TryGetValue(tab, out Vector2 size))
                {
                    size = ViewSizeFunc(window, tab);
                    viewSizeCache[tab] = size;
                }
                return Mathf.Max(height, size.y + 20f);
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 计算研究页内容高度失败，退回原版高度: " + e.Message);
                return height;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            ConstructorInfo rectCtor = AccessTools.Constructor(typeof(Rect),
                new[] { typeof(float), typeof(float), typeof(float), typeof(float) });
            MethodInfo helper = AccessTools.Method(typeof(ResearchScrollPatch), nameof(EnsureContentHeight));
            bool patched = false;
            for (int i = 2; i < list.Count; i++)
            {
                // 目标序列（viewRect 构造的高度实参）：ldc.r4 16 ; sub ; call Rect..ctor(float, float, float, float)
                if (list[i].opcode == OpCodes.Call && Equals(list[i].operand, rectCtor)
                    && list[i - 1].opcode == OpCodes.Sub
                    && list[i - 2].opcode == OpCodes.Ldc_R4
                    && list[i - 2].operand is float f && f == 16f)
                {
                    // 此刻栈顶为原版高度 height；注入 ldarg.0 + call EnsureContentHeight(height, this)
                    list.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    list.Insert(i + 1, new CodeInstruction(OpCodes.Call, helper));
                    patched = true;
                    break;
                }
            }
            if (!patched)
                Log.Warning("[MMT] 研究窗口纵向滚动补丁未匹配到目标 IL（游戏版本或界面 Mod 可能已变更），研究页底部可能显示不全");
            return list;
        }
    }
}
