using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 沙袋人偶（武器伤害测试假人）：
    /// - 超高耐久（XML 50000）+ 每 60 tick 自修复，持续测试不用修理
    /// - 重写 TakeDamage 统计实际承伤：近 10 秒滚动窗口 DPS / 最近一击 / 累计伤害
    /// - gizmo"清零统计"；娱乐由 XML joyKind 提供（小人站旁边练习）
    /// 征召小人用"攻击"指令点自己的假人即可测试武器伤害。
    /// </summary>
    public class Building_TrainingDummy : Building
    {
        /// <summary>DPS 统计滚动窗口长度（tick，10 秒）。</summary>
        private const int WindowTicks = 600;
        /// <summary>自修复间隔（tick）。</summary>
        private const int HealInterval = 60;
        /// <summary>每间隔恢复耐久（约 500/秒）。</summary>
        private const int HealAmount = 500;

        /// <summary>滚动窗口承伤记录（tick, 实际掉血）。</summary>
        private readonly Queue<KeyValuePair<int, float>> window = new Queue<KeyValuePair<int, float>>();
        private float windowSum;
        /// <summary>累计承伤（存档持久化，读档后滚动窗口清零）。</summary>
        private float totalDamage;
        private float lastHit = -1f;
        private int lastHitTick = -99999;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalDamage, "mmtDummyTotalDamage", 0f);
        }

        /// <summary>记录一次实际承伤（由 Patch_ThingTakeDamage postfix 调用，lost 为实际掉血）。</summary>
        public void RecordDamage(float lost)
        {
            if (lost <= 0f)
                return;
            int now = Find.TickManager.TicksGame;
            window.Enqueue(new KeyValuePair<int, float>(now, lost));
            windowSum += lost;
            totalDamage += lost;
            lastHit = lost;
            lastHitTick = now;
            Trim(now);
        }

        /// <summary>承伤统计：1.7 Thing.TakeDamage 非虚方法不能重写，用 Harmony postfix 过滤假人类型。</summary>
        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        public static class Patch_ThingTakeDamage
        {
            public static void Postfix(Thing __instance, DamageWorker.DamageResult __result)
            {
                Building_TrainingDummy dummy = __instance as Building_TrainingDummy;
                if (dummy != null && __result != null)
                    dummy.RecordDamage(__result.totalDamageDealt);
            }
        }

        protected override void Tick()
        {
            base.Tick();
            // 高频自修复：50000 耐久上限基本打不爆，这里保证长时间测试后自动回满
            if (HitPoints < MaxHitPoints && Find.TickManager.TicksGame % HealInterval == 0)
                HitPoints = Mathf.Min(MaxHitPoints, HitPoints + HealAmount);
        }

        private void Trim(int now)
        {
            while (window.Count > 0 && now - window.Peek().Key > WindowTicks)
                windowSum -= window.Dequeue().Value;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            int now = Find.TickManager.TicksGame;
            Trim(now);
            sb.AppendLine();
            sb.Append("近10秒秒伤: " + (windowSum / (WindowTicks / 60f)).ToString("F1"));
            if (lastHit >= 0f && now - lastHitTick < 2500)
                sb.Append("  |  最近一击: " + lastHit.ToString("F0"));
            sb.AppendLine();
            sb.Append("累计承伤: " + totalDamage.ToString("F0"));
            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;
            Command_Action reset = new Command_Action();
            reset.defaultLabel = "清零承伤统计";
            reset.defaultDesc = "清空累计承伤与滚动窗口统计，重新开始测量。";
            reset.icon = parent.def.uiIcon;
            reset.action = delegate
            {
                window.Clear();
                windowSum = 0f;
                totalDamage = 0f;
                lastHit = -1f;
            };
            yield return reset;
        }
    }
}
