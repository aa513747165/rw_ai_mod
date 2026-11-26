using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 纳米净化血清的服用效果：移除服用者的全部化学成瘾 hediff（Hediff_Addiction），
    /// 覆盖薄片/雅戈/清醒丸/酒精/灵能粉乃至原版魔鬼素依赖（LuciferiumAddiction 同为 Hediff_Addiction 子类）。
    /// 耐受 hediff 不移除，防止靠反复注射刷效果；化学需求 Need 由原版按成瘾 hediff 存在性自动清理。
    /// </summary>
    public class IngestionOutcomeDoer_RemoveAddictions : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn?.health?.hediffSet == null)
                return;
            // 流程开始：收集全部成瘾 hediff（原版/本 mod/第三方 mod 的成瘾一律覆盖）
            List<Hediff_Addiction> addictions = pawn.health.hediffSet.hediffs.OfType<Hediff_Addiction>().ToList();
            if (addictions.Count == 0)
            {
                Messages.Message("MMT_NanoPurgeSerumNoAddiction".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            // 节点流转：逐一直接移除成瘾 hediff（RemoveHediff 会同步清掉对应化学需求）
            foreach (Hediff_Addiction addiction in addictions)
                pawn.health.RemoveHediff(addiction);
            // 流程结束：提示清除了几种成瘾
            Messages.Message("MMT_NanoPurgeSerumCured".Translate(pawn.LabelShort, addictions.Count), pawn, MessageTypeDefOf.PositiveEvent, true);
        }
    }
}
