using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 装备集成接口族：穿戴/携带物品的 comp 实现这些接口后，由本文件的三个统一 Harmony 补丁
    /// 自动接入"征召指令栏按钮 / 伤害拦截 / 解除征召回调"，新设备接入零新补丁。
    /// （原 ShieldPack.cs / ShieldDeployer.cs 中按具体 comp 类型硬编码的补丁已收敛到此。）
    /// </summary>

    /// <summary>征召状态下向指令栏提供 gizmo 的物品 comp（穿戴或背包携带均可）。</summary>
    public interface IDraftedGizmoProvider
    {
        IEnumerable<Gizmo> GetDraftedGizmos(Pawn pawn);
    }

    /// <summary>穿戴吸收伤害的物品 comp（PreApplyDamage 统一补丁按穿戴顺序询问）。</summary>
    public interface IWornDamageAbsorber
    {
        /// <summary>返回 true 表示全额吸收（absorbed=true，跳过原方法）；false 放行（可已削减 dinfo）。</summary>
        bool TryAbsorbDamage(Pawn pawn, ref DamageInfo dinfo);
    }

    /// <summary>解除征召时需要回调的物品 comp（set_Drafted 统一补丁通知）。</summary>
    public interface IUndraftCallback
    {
        void OnUndrafted(Pawn pawn);
    }

    /// <summary>
    /// 统一指令栏注入补丁：征召 + 玩家殖民者时，遍历穿戴与背包物品的 comp，
    /// 追加所有 IDraftedGizmoProvider 提供的 gizmo（原护盾腰带/部署护盾两个补丁合并于此）。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class MMT_DraftedGizmoPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
        {
            foreach (Gizmo gizmo in values)
                yield return gizmo;
            if (!__instance.Drafted || !__instance.IsColonistPlayerControlled)
                yield break;
            if (__instance.apparel != null)
            {
                foreach (Apparel ap in __instance.apparel.WornApparel)
                {
                    foreach (ThingComp comp in ap.AllComps)
                    {
                        if (comp is IDraftedGizmoProvider provider)
                        {
                            foreach (Gizmo g in provider.GetDraftedGizmos(__instance))
                                yield return g;
                        }
                    }
                }
            }
            if (__instance.inventory != null)
            {
                foreach (Thing t in __instance.inventory.innerContainer)
                {
                    if (!(t is ThingWithComps twc))
                        continue;
                    foreach (ThingComp comp in twc.AllComps)
                    {
                        if (comp is IDraftedGizmoProvider provider)
                        {
                            foreach (Gizmo g in provider.GetDraftedGizmos(__instance))
                                yield return g;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 统一伤害吸收补丁：按穿戴顺序询问所有 IWornDamageAbsorber，
    /// 第一个全额吸收者置 absorbed=true 并跳过原方法；部分吸收者削减伤害量后放行。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class MMT_WornDamageAbsorbPatch
    {
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (__instance.apparel == null)
                return true;
            List<Apparel> worn = __instance.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                List<ThingComp> comps = worn[i].AllComps;
                for (int j = 0; j < comps.Count; j++)
                {
                    if (comps[j] is IWornDamageAbsorber absorber && absorber.TryAbsorbDamage(__instance, ref dinfo))
                    {
                        absorbed = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>统一解除征召补丁：解除征召时回调身上所有 IUndraftCallback。</summary>
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public static class MMT_UndraftNotifyPatch
    {
        public static void Postfix(Pawn_DraftController __instance, bool value)
        {
            if (value)
                return;
            Pawn pawn = __instance.pawn;
            if (pawn?.apparel == null)
                return;
            foreach (Apparel ap in pawn.apparel.WornApparel)
            {
                foreach (ThingComp comp in ap.AllComps)
                {
                    if (comp is IUndraftCallback callback)
                        callback.OnUndrafted(pawn);
                }
            }
        }
    }
}
