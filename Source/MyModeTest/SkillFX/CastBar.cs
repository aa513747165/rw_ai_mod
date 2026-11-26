using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 技能读条框架：小人施法读条期间在其头顶画圆形读条。
    /// 圆一开始为满圆，随读条进行按顺时针方向递减（空缺从 12 点方向顺时针扩大）。
    /// 瞬发技能（warmupTime = 0）不产生读条 stance，自然无读条圆；
    /// 后续技能特效可直接调用 <see cref="Draw"/> 复用（如自定义引导/蓄力通道）。
    /// </summary>
    public static class MMTCastBar
    {
        /// <summary>读条圆颜色（法力蓝，半透明）。</summary>
        public static Color BarColor = new Color(0.45f, 0.75f, 1f, 0.5f);

        /// <summary>读条圆半径缩放（原版扇形网格半径 0.55，乘此系数）。</summary>
        public const float PieScale = 0.55f;

        /// <summary>头顶偏移（世界北向格数）。</summary>
        public const float HeadOffset = 0.8f;

        // 扇形网格旋转恒为 270° 时，张角 k 恰好覆盖 [360-k, 360]（缺口从正北顺时针扩大），见补丁注释。
        private const float PieRotation = 270f;

        // 按颜色缓存材质（默认法力蓝 + 各技能自定义配色）
        private static readonly System.Collections.Generic.Dictionary<Color, Material> barMaterials = new System.Collections.Generic.Dictionary<Color, Material>();

        private static Material GetMaterial(Color color)
        {
            if (!barMaterials.TryGetValue(color, out Material mat) || mat == null)
            {
                mat = SolidColorMaterials.SimpleSolidColorMaterial(color);
                barMaterials[color] = mat;
            }
            return mat;
        }

        /// <summary>
        /// 在指定小人头顶画读条圆。
        /// fractionRemaining = 剩余读条比例（1=刚开始满圆，0=读条结束）。
        /// </summary>
        public static void Draw(Pawn pawn, float fractionRemaining)
        {
            if (pawn == null || !pawn.Spawned)
            {
                return;
            }
            DrawAt(pawn.DrawPos, HeadOffset, fractionRemaining);
        }

        /// <summary>默认配色（<see cref="BarColor"/>）版本，供建筑/任意位置调用（如假人施法演示）。</summary>
        public static void DrawAt(Vector3 basePos, float zOffset, float fractionRemaining)
        {
            DrawAt(basePos, zOffset, fractionRemaining, BarColor);
        }

        /// <summary>
        /// 在世界坐标正上方画读条圆（pawn/建筑通用）。
        /// fractionRemaining = 剩余读条比例（1=刚开始满圆，0=读条结束）。
        /// </summary>
        public static void DrawAt(Vector3 basePos, float zOffset, float fractionRemaining, Color color)
        {
            if (fractionRemaining <= 0f)
            {
                return;
            }
            int degrees = Mathf.Clamp(Mathf.RoundToInt(360f * fractionRemaining), 0, 360);
            if (degrees <= 0)
            {
                return;
            }
            Vector3 center = basePos;
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            center.z += zOffset;
            Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.AngleAxis(PieRotation, Vector3.up), new Vector3(PieScale, 1f, PieScale));
            Graphics.DrawMesh(MeshPool.pies[degrees], matrix, GetMaterial(color), 0);
        }

        /// <summary>颜色变更后丢弃缓存材质（供后续按技能配色时调用）。</summary>
        public static void ClearCache()
        {
            barMaterials.Clear();
        }
    }

    /// <summary>
    /// 替换技能施法（Verb_CastAbility）的原版瞄准扇形为头顶读条圆。
    /// 枪械等其他动词的原版瞄准扇形不受影响。
    /// </summary>
    [HarmonyPatch(typeof(Stance_Warmup), nameof(Stance_Warmup.StanceDraw))]
    public static class Patch_MMTCastBar
    {
        private static bool Prefix(Stance_Warmup __instance)
        {
            if (!(__instance.verb is Verb_CastAbility verb) || __instance.ticksLeft <= 0)
            {
                return true;
            }
            Pawn pawn = __instance.stanceTracker?.pawn;
            if (pawn == null || !pawn.Spawned)
            {
                return true;
            }
            // 与 Verb.TryStartCastOn 的建 stance 公式一致，保证比例准确
            float totalTicks = (verb.WarmupTime * pawn.GetStatValue(StatDefOf.AimingDelayFactor)).SecondsToTicks();
            if (totalTicks <= 0f)
            {
                return true;
            }
            MMTCastBar.Draw(pawn, __instance.ticksLeft / totalTicks);
            return false;
        }
    }
}
