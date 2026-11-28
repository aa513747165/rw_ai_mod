using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    // Mana Odyssey 兼容补丁（软兼容，反射挂点、无编译期依赖，未安装 MO 时静默跳过）：
    // MO 的攻击法术 <cast> 默认 <targets> 为空 = 只允许 Pawn 目标（SpellCast.Accepts），
    // 且瞄准参数 canTargetBuildings=false（ManaAim.ParamsFor），而 MMT 沙袋人偶是 Building，
    // 导致 MO 法术无法点选假人、无法测试法术 DPS。本补丁做两处放开（仅对伤害型法术生效）：
    // 1. SpellCast.Accepts(Thing) postfix：额外接受 MMT 假人（两种 def）；
    // 2. ManaAim.ParamsFor(SpellCast, SpellAim) postfix：伤害型法术瞄准参数开启 canTargetBuildings，
    //    使瞄准 UI 能选中建筑格（候选仍由 Accepts 过滤，仅假人通过，其他建筑不受影响）。
    // 结算侧 MO 自身已兼容非 Pawn 目标：SpellEffect_Strike.Hit 走 Thing.TakeDamage，
    // Pawn 专有分支（stagger/stun/反应/吸血）均有 is Pawn 守卫；假人掉血由 MMT 的
    // Patch_ThingTakeDamage postfix 统计 DPS（近10秒秒伤/最近一击/累计），无需额外改动。
    // Point 瞄准的 Burst 类法术走原版 GenExplosion.DoExplosion，本就含建筑，不受影响。
    public static class ManaOdysseyCompat
    {
        private static bool patched;

        /// <summary>伤害字段反射缓存：效果类型 → 其 public float damage 字段（无则存 null）。</summary>
        private static readonly Dictionary<Type, FieldInfo> damageFields = new Dictionary<Type, FieldInfo>();

        /// <summary>检测到 Mana Odyssey 时应用兼容补丁；任何失败只记日志，不影响 MMT 其他补丁。</summary>
        public static void TryPatch(Harmony harmony)
        {
            if (patched || harmony == null)
            {
                return;
            }
            Type spellCast = GenTypes.GetTypeInAnyAssembly("ManaOdyssey.SpellCast");
            Type manaAim = GenTypes.GetTypeInAnyAssembly("ManaOdyssey.ManaAim");
            if (spellCast == null || manaAim == null)
            {
                // 未安装 Mana Odyssey，静默跳过
                return;
            }
            try
            {
                MethodInfo accepts = AccessTools.Method(spellCast, "Accepts");
                MethodInfo paramsFor = AccessTools.Method(manaAim, "ParamsFor");
                if (accepts == null || paramsFor == null)
                {
                    Log.Warning("[myModeTest] Mana Odyssey 兼容：未找到 Accepts/ParamsFor 方法（MO 版本变动？），跳过。");
                    return;
                }
                harmony.Patch(accepts, postfix: new HarmonyMethod(typeof(ManaOdysseyCompat), nameof(AcceptsPostfix)));
                harmony.Patch(paramsFor, postfix: new HarmonyMethod(typeof(ManaOdysseyCompat), nameof(ParamsForPostfix)));
                patched = true;
                Log.Message("[myModeTest] Mana Odyssey 兼容补丁已应用：MO 伤害法术可对沙袋人偶施放。");
            }
            catch (Exception e)
            {
                Log.Warning("[myModeTest] Mana Odyssey 兼容补丁失败：" + e);
            }
        }

        /// <summary>SpellCast.Accepts(Thing) postfix：伤害型法术额外接受 MMT 假人。</summary>
        private static void AcceptsPostfix(object __instance, Thing t, ref bool __result)
        {
            if (__result || t == null)
            {
                return;
            }
            if (t is Building_TrainingDummy && IsDamaging(__instance))
            {
                __result = true;
            }
        }

        /// <summary>ManaAim.ParamsFor(SpellCast, SpellAim) postfix：伤害型 Thing 瞄准开启建筑可选。</summary>
        private static void ParamsForPostfix(object[] __args, TargetingParameters __result)
        {
            if (__result == null || __args == null || __args.Length < 2)
            {
                return;
            }
            // SpellAim.Thing 才有点选目标的需求（Point/Line 走地面不受影响）
            if (__args[1] != null && __args[1].ToString() == "Thing" && IsDamaging(__args[0]))
            {
                __result.canTargetBuildings = true;
            }
        }

        /// <summary>该 cast 是否伤害型：effects 列表中任一效果实例的 public float damage 字段 > 0。</summary>
        private static bool IsDamaging(object cast)
        {
            if (cast == null)
            {
                return false;
            }
            try
            {
                FieldInfo effectsField = AccessTools.Field(cast.GetType(), "effects");
                IEnumerable effects = effectsField?.GetValue(cast) as IEnumerable;
                if (effects == null)
                {
                    return false;
                }
                foreach (object fx in effects)
                {
                    if (fx == null)
                    {
                        continue;
                    }
                    Type ft = fx.GetType();
                    if (!damageFields.TryGetValue(ft, out FieldInfo f))
                    {
                        f = AccessTools.Field(ft, "damage");
                        damageFields[ft] = f;
                    }
                    if (f != null && f.FieldType == typeof(float) && (float)f.GetValue(fx) > 0f)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // 反射异常按非伤害处理，宁缺勿滥
            }
            return false;
        }
    }
}
