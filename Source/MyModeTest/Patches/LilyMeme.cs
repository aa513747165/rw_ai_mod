using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 「百合花」模因（MMT_Lily）的运行时补丁集合，对应 1.7/Defs/MemeDefs/MMT_Memes_Lily.xml 头注释的三条机制：
    ///   1. 女女可恋爱：百合信徒女性对女性恢复吸引力（右键"求爱"选项、RomanceEligiblePair 性向校验、
    ///      亲热几率的同性归零均解锁），求爱权重不再被原版 0.15 同性惩罚打折
    ///      （恋爱/求婚/结婚原版原生支持，无需婚姻补丁）；
    ///   2. 女女怀孕：双方均为本文化信徒的女女伴侣亲热后，按原版怀孕公式自然怀孕（随机一方受孕，
    ///      受孕 Hediff 的 father 槽存第二位女性——原版不可能产生"双女性父母"，此即百合生育的天然识别标记）；
    ///   3. 后代必女：百合伴侣分娩时（ApplyBirthOutcome 期间）生成的婴儿 FixedGender 强制为 Female。
    /// 全部补丁遵循 MMT 约定：Prepare() 探测目标方法存在性（缺失则警告并跳过）、Postfix 全 try/catch 降级、
    /// def 用 GetNamedSilentFail 懒加载缓存。无 Biotech 时补丁 2/3 静默不生效，不需要 Biotech 硬依赖。
    /// </summary>
    internal static class LilyMeme
    {
        private static MemeDef cached;
        private static bool resolved;
        private static bool warnedMissing;

        /// <summary>懒加载 MMT_Lily 模因 def；缺失时警告一次并让补丁静默不生效（安全降级）。</summary>
        public static MemeDef Def
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    cached = DefDatabase<MemeDef>.GetNamedSilentFail("MMT_Lily");
                    if (cached == null && !warnedMissing)
                    {
                        warnedMissing = true;
                        Log.Warning("[MMT] 未找到模因 MMT_Lily，百合花补丁不生效");
                    }
                }
                return cached;
            }
        }

        public static bool ActiveFor(Pawn pawn)
        {
            return pawn != null && pawn.Ideo != null && Def != null && pawn.Ideo.HasMeme(Def);
        }
    }

    /// <summary>
    /// 补丁 A（女女怀孕）：原版 JobDriver_Lovin 的亲热结束回调 &lt;MakeNewToils&gt;b__12_4 只按性别选出
    /// man/woman 槽，同性伴侣两槽为 null 直接跳过怀孕判定。Postfix 在原版跑完后补一次女女分支：
    /// 双方女性且双方百合信徒 → 从未孕的一方随机选受孕方 → 按原版公式掷签
    /// （PregnancyChance 静态字段 × PregnancyUtility.PregnancyChanceForPartners，性别无关）→
    /// 基因可结合则挂上 PregnantHuman 并 SetParents(null, 另一位女性, genes)。
    /// </summary>
    [HarmonyPatch]
    public static class LilyPatch_LovinPregnancy
    {
        // 原版默认亲热怀孕概率；PregnancyChance 私有静态字段反射失败时的兜底值
        private const float FallbackPregnancyChance = 0.025f;

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(JobDriver_Lovin), "<MakeNewToils>b__12_4");
        }

        public static bool Prepare()
        {
            if (TargetMethod() == null)
            {
                Log.Warning("[MMT] 未找到 JobDriver_Lovin.<MakeNewToils>b__12_4，百合花女女怀孕补丁跳过");
                return false;
            }
            return true;
        }

        public static void Postfix(JobDriver_Lovin __instance)
        {
            try
            {
                if (!ModsConfig.BiotechActive)
                {
                    return;
                }
                Pawn pawn = __instance.pawn;
                Pawn partner = Traverse.Create(__instance).Property("Partner").GetValue<Pawn>();
                if (pawn == null || partner == null)
                {
                    return;
                }
                // 仅双方均为女性的百合信徒伴侣
                if (pawn.gender != Gender.Female || partner.gender != Gender.Female)
                {
                    return;
                }
                if (!LilyMeme.ActiveFor(pawn) || !LilyMeme.ActiveFor(partner))
                {
                    return;
                }
                // 选受孕方：已怀 PregnantHuman 的一方排除；都怀则跳过
                bool pawnPregnant = pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman);
                bool partnerPregnant = partner.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman);
                if (pawnPregnant && partnerPregnant)
                {
                    return;
                }
                Pawn carrier;
                Pawn other;
                if (pawnPregnant)
                {
                    carrier = partner;
                    other = pawn;
                }
                else if (partnerPregnant)
                {
                    carrier = pawn;
                    other = partner;
                }
                else if (Rand.Bool)
                {
                    carrier = pawn;
                    other = partner;
                }
                else
                {
                    carrier = partner;
                    other = pawn;
                }
                // 原版怀孕概率 = PregnancyChance(静态字段) × 双方生育力系数
                float baseChance = FallbackPregnancyChance;
                FieldInfo chanceField = AccessTools.Field(typeof(JobDriver_Lovin), "PregnancyChance");
                if (chanceField != null && chanceField.IsStatic)
                {
                    baseChance = (float)chanceField.GetValue(null);
                }
                if (!Rand.Chance(baseChance * PregnancyUtility.PregnancyChanceForPartners(carrier, other)))
                {
                    return;
                }
                // 基因结合（另一位女性占 father 参数位）；不可结合时仿原版发失败消息
                GeneSet genes = PregnancyUtility.GetInheritedGeneSet(other, carrier, out bool canCombine);
                if (!canCombine)
                {
                    Messages.Message(
                        "MessagePregnancyFailed".Translate(other.Named("FATHER"), carrier.Named("MOTHER")),
                        carrier,
                        MessageTypeDefOf.NegativeEvent);
                    return;
                }
                Hediff_Pregnant hediff = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, carrier);
                // mother 槽留空、father 槽存第二位女性：与原版结构一致，且"双女性父母"成为补丁 B 的识别标记
                hediff.SetParents(null, other, genes);
                carrier.health.AddHediff(hediff);
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 百合花补丁(女女怀孕)降级: " + e.Message);
            }
        }
    }

    /// <summary>
    /// 补丁 B（后代必女）前置：ApplyBirthOutcome 是所有人类分娩（含培养舱）的出口。1.7 签名无 mother 参数，
    /// 母体用 geneticMother（培养舱可能为空）兜底 birtherThing（自然分娩即分娩者本人）。原版父亲必为男性，
    /// father 为女性即百合生育（补丁 A 以 mother 槽留空、father 槽存第二位女性的结构写入），
    /// 置线程静态标记，由 GeneratePawn 前置补丁消费。Finalizer 保证异常路径也会清除标记。
    /// </summary>
    [HarmonyPatch]
    public static class LilyPatch_BirthOutcomeFlag
    {
        [ThreadStatic]
        private static bool lilyBirthActive;

        public static bool IsLilyBirth => lilyBirthActive;

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PregnancyUtility), "ApplyBirthOutcome");
        }

        public static bool Prepare()
        {
            if (TargetMethod() == null)
            {
                Log.Warning("[MMT] 未找到 PregnancyUtility.ApplyBirthOutcome，百合花后代性别补丁跳过");
                return false;
            }
            return true;
        }

        public static void Prefix(Pawn geneticMother, Thing birtherThing, Pawn father)
        {
            try
            {
                // 原版不可能产生女性 father；百合生育的 mother 槽为空，母体用 birtherThing（自然分娩即受孕方本人）兜底
                Pawn mother = geneticMother ?? birtherThing as Pawn;
                lilyBirthActive = father != null
                    && father.gender == Gender.Female
                    && (mother == null || mother.gender == Gender.Female)
                    && (LilyMeme.ActiveFor(mother) || LilyMeme.ActiveFor(father));
            }
            catch (Exception e)
            {
                lilyBirthActive = false;
                Log.Warning("[MMT] 百合花补丁(分娩标记)降级: " + e.Message);
            }
        }

        public static void Finalizer()
        {
            lilyBirthActive = false;
        }
    }

    /// <summary>
    /// 补丁 B（后代必女）消费端：百合分娩期间 PawnGenerator.GeneratePawn 生成的婴儿强制女性。
    /// 只填空的 FixedGender，不覆盖其他来源已指定的性别，幂等且可与同类 Mod 叠加。
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
    public static class LilyPatch_ForceFemaleBaby
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) }) != null;
        }

        public static void Prefix(ref PawnGenerationRequest request)
        {
            try
            {
                if (LilyPatch_BirthOutcomeFlag.IsLilyBirth && request.FixedGender == null)
                {
                    request.FixedGender = Gender.Female;
                }
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 百合花补丁(后代必女)降级: " + e.Message);
            }
        }
    }

    /// <summary>
    /// 补丁 C（求爱去惩罚）：原版 InteractionWorker_RomanceAttempt.RandomSelectionWeight 对同性求爱有两处
    /// ×0.15 惩罚——发起方非 Gay 女性一项、两人同性且非双方皆 Gay 一项。双方均为百合信徒的女性伴侣
    /// 按原版分支反向除回 0.15，恢复与异性求爱相同的权重。仅在 __result &gt; 0 时修正，幂等。
    /// </summary>
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
    public static class LilyPatch_RomanceWeight
    {
        public static bool Prepare()
        {
            return AccessTools.DeclaredMethod(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight") != null;
        }

        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            try
            {
                if (__result <= 0f)
                {
                    return;
                }
                if (initiator == null || recipient == null
                    || initiator.gender != Gender.Female || recipient.gender != Gender.Female)
                {
                    return;
                }
                if (!LilyMeme.ActiveFor(initiator) || !LilyMeme.ActiveFor(recipient))
                {
                    return;
                }
                bool initiatorGay = initiator.story != null && initiator.story.traits.HasTrait(TraitDefOf.Gay);
                bool recipientGay = recipient.story != null && recipient.story.traits.HasTrait(TraitDefOf.Gay);
                // 原版分支一：发起方非 Gay 女性 ×0.15 → 除回
                if (!initiatorGay)
                {
                    __result /= 0.15f;
                }
                // 原版分支二：同性且非双方皆 Gay ×0.15 → 除回
                if (!initiatorGay || !recipientGay)
                {
                    __result /= 0.15f;
                }
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 百合花补丁(求爱去惩罚)降级: " + e.Message);
            }
        }
    }

    /// <summary>
    /// 补丁 D（女女吸引力）：原版 RelationsUtility.AttractedToGender 对非 Gay/Bisexual 的异性恋
    /// 只认异性，同性一律 false——右键"求爱"选项（FloatMenuOptionProvider_Romance → RomanceOption）
    /// 与 RomanceEligiblePair 的双向性向校验都走这里，直女对直女直接隐藏选项，这就是"无法选择同性"的根因。
    /// Postfix：女性百合信徒对女性补 true（无性恋尊重原版仍 false）。RomanceEligiblePair 双向各查一次，
    /// 对方非百合信徒时反向检查照旧失败，天然实现"双方均须为百合信徒"。
    /// </summary>
    [HarmonyPatch(typeof(RelationsUtility), "AttractedToGender")]
    public static class LilyPatch_AttractedToGender
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(RelationsUtility), "AttractedToGender") != null;
        }

        public static void Postfix(Pawn pawn, Gender gender, ref bool __result)
        {
            try
            {
                if (__result || pawn == null)
                {
                    return;
                }
                if (pawn.gender != Gender.Female || gender != Gender.Female)
                {
                    return;
                }
                if (!LilyMeme.ActiveFor(pawn))
                {
                    return;
                }
                // 无性恋对任何性别都无吸引力，原版语义不动
                if (pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                {
                    return;
                }
                __result = true;
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 百合花补丁(女女吸引力)降级: " + e.Message);
            }
        }
    }

    /// <summary>
    /// 补丁 E（女女亲热几率）：原版 Pawn_RelationsTracker.SecondaryLovinChanceFactor 对非 Gay/Bisexual
    /// 的同性组合直接 return 0，导致 RandomSelectionWeight / SuccessChance / 社交页"恋爱可能性"全链路归零
    /// （0.15 惩罚补丁 C 只修非零权重，修不了这里的硬归零）。
    /// Postfix：双方均为女性百合信徒且结果为 0 时，把对方性别临时翻成男性重算一次原版方法——
    /// 无性恋、未成年、血缘等原版限制在重算中依旧生效，只有"同性归零"这一条分支被解除。
    /// 递归调用时对方已是男性，性别前置条件不再满足，不会死循环。
    /// </summary>
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor")]
    public static class LilyPatch_LovinChanceFactor
    {
        // 私有字段 Pawn_RelationsTracker.pawn 的缓存访问器：编译期一次性反射生成委托，
        // 调用开销等同直接字段访问（Traverse.Create 每次调用都新建对象+反射，热路径上禁用）
        private static readonly AccessTools.FieldRef<Pawn_RelationsTracker, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Pawn_RelationsTracker, Pawn>("pawn");

        public static bool Prepare()
        {
            return AccessTools.Method(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor") != null;
        }

        public static void Postfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
        {
            try
            {
                // 按开销从低到高短路：数值/枚举比较 → 字段访问 → HasMeme 列表扫描
                if (__result > 0f || otherPawn == null || otherPawn.gender != Gender.Female)
                {
                    return;
                }
                Pawn pawn = PawnRef(__instance);
                if (pawn == null || pawn.gender != Gender.Female)
                {
                    return;
                }
                if (!LilyMeme.ActiveFor(pawn) || !LilyMeme.ActiveFor(otherPawn))
                {
                    return;
                }
                Gender original = otherPawn.gender;
                try
                {
                    otherPawn.gender = Gender.Male;
                    __result = __instance.SecondaryLovinChanceFactor(otherPawn);
                }
                finally
                {
                    otherPawn.gender = original;
                }
            }
            catch (Exception e)
            {
                Log.Warning("[MMT] 百合花补丁(女女亲热几率)降级: " + e.Message);
            }
        }
    }
}
