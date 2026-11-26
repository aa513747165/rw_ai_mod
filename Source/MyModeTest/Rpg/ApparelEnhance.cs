using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 防具强化参数：等级上限、每级护甲加成、满级奖励、成功率表与材料阶梯（XML 可配置）。
    /// 经 XPath 补丁挂到全体防具（跨 Mod 兼容，见 MMT_Patch_ApparelEnhancement.xml）；
    /// 护盾腰带被补丁排除（走 ShieldEnhancement 线）。
    /// </summary>
    public class CompProperties_ApparelEnhancement : CompProperties
    {
        /// <summary>强化等级上限（默认 15）。</summary>
        public int maxLevel = 15;

        /// <summary>每级护甲加成（复利，0.03 = 每级 ×1.03，满级 ≈×1.56）。</summary>
        public float armorBonusPerLevel = 0.03f;

        /// <summary>满级额外护甲加成（乘法，0.10 = 三防再 ×1.10）。</summary>
        public float finalLevelArmorBonus = 0.10f;

        /// <summary>满级额外隔温加成（乘法，0.20 = 保暖/隔温再 ×1.20）。</summary>
        public float finalLevelInsulationBonus = 0.20f;

        /// <summary>各级成功率表（第 i 项 = 强化到 i+1 级），缺省时用代码内置默认表。</summary>
        public List<float> successRates;

        /// <summary>材料阶梯表（目标等级 ≤ maxLevel 的第一个段位生效）。</summary>
        public List<EnhanceTierMaterials> materialTiers;

        public CompProperties_ApparelEnhancement()
        {
            compClass = typeof(CompApparelEnhancement);
        }
    }

    /// <summary>
    /// 防具强化 Comp：强化等级（赌博线，护甲复利）+ 附魔词条列表（确定线，模块物品安装）。
    /// 消费端：护甲/隔温/pawn 属性走 StatWorker 补丁；减伤/EMP 防护走 IWornDamageAbsorber；
    /// 受击回血/反击特效走 IWornPostDamageHandler。附魔槽位：基础 1，+7 解锁第 2 槽，+13 解锁第 3 槽。
    /// </summary>
    public class CompApparelEnhancement : ThingComp, IEnchantable, IWornDamageAbsorber, IWornPostDamageHandler
    {
        /// <summary>内置默认成功率表（强化到 1~15 级）：与武器同款（1-3 必成，15 级 5%）。</summary>
        private static readonly float[] DefaultRates =
        {
            1f, 1f, 1f,
            0.8f, 0.8f, 0.8f,
            0.6f, 0.6f, 0.6f,
            0.4f, 0.4f, 0.4f,
            0.2f, 0.2f,
            0.05f,
        };

        /// <summary>当前强化等级（0-15，存档持久化）。</summary>
        private int level;

        /// <summary>已安装附魔词条（模块物品 def，存档持久化；效果从 def 的 ModuleExtension 读取）。</summary>
        private List<ThingDef> enchants = new List<ThingDef>();

        /// <summary>危险受击特效开关（荆棘/放电/毒雾；默认开，存档持久化，旧档缺省回退 true）。</summary>
        private bool triggerEffectsEnabled = true;

        private CompProperties_ApparelEnhancement Props
        {
            get { return (CompProperties_ApparelEnhancement)props; }
        }

        /// <summary>设置门控：本防具是否开放强化效果。</summary>
        private bool Eligible
        {
            get { return EnhanceLadder.ApparelEquipEligible(parent.def); }
        }

        /// <summary>当前强化等级。</summary>
        public int Level
        {
            get { return level; }
        }

        /// <summary>下一强化等级。</summary>
        public int NextLevel
        {
            get { return level + 1; }
        }

        /// <summary>是否已达最高强化等级。</summary>
        public bool IsMaxLevel
        {
            get { return level >= Props.maxLevel; }
        }

        /// <summary>已安装附魔词条（只读）。</summary>
        public IReadOnlyList<ThingDef> Enchants
        {
            get { return enchants; }
        }

        /// <summary>附魔槽位数：基础 1，+7 第 2 槽，+13 第 3 槽。</summary>
        public int EnchantSlots
        {
            get { return 1 + (level >= 7 ? 1 : 0) + (level >= 13 ? 1 : 0); }
        }

        /// <summary>IEnchantable：本装备接受的模块槽位类别。</summary>
        public string ModuleSlotKey
        {
            get { return "ApparelEnchant"; }
        }

        /// <summary>危险受击特效是否开启（荆棘/放电/毒雾；gizmo 开关）。</summary>
        public bool TriggerEffectsEnabled
        {
            get { return triggerEffectsEnabled; }
        }

        /// <summary>能否安装某词条：仅同词条不重复（槽满也可装——直接覆盖最早装的词条）。</summary>
        public bool CanAddEnchant(ThingDef moduleDef)
        {
            return moduleDef != null && !enchants.Contains(moduleDef);
        }

        /// <summary>安装词条（模块物品在作业侧已消耗）；槽满时覆盖最早装的词条并返回被覆盖 def（无覆盖返回 null）。</summary>
        public ThingDef AddEnchant(ThingDef moduleDef)
        {
            if (!CanAddEnchant(moduleDef))
                return null;
            ThingDef overwritten = null;
            if (enchants.Count >= EnchantSlots && enchants.Count > 0)
            {
                overwritten = enchants[0];
                enchants.RemoveAt(0);
            }
            enchants.Add(moduleDef);
            return overwritten;
        }

        /// <summary>词条效果合计（key 见 ModuleEffectKeys.Apparel*）。</summary>
        public float GetEffectSum(string key)
        {
            float sum = 0f;
            for (int i = 0; i < enchants.Count; i++)
            {
                ModuleExtension ext = enchants[i].GetModExtension<ModuleExtension>();
                if (ext?.effects == null)
                    continue;
                for (int j = 0; j < ext.effects.Count; j++)
                {
                    if (ext.effects[j].key == key)
                        sum += ext.effects[j].valuePerItem;
                }
            }
            return sum;
        }

        // ---- 聚合属性（StatWorker 补丁消费；均先过设置门控） ----

        /// <summary>护甲总倍率：强化复利 × (1 + 词条护甲加成) × 满级奖励。</summary>
        public float ArmorFactor
        {
            get
            {
                return Mathf.Pow(1f + Props.armorBonusPerLevel, level)
                    * (1f + GetEffectSum(ModuleEffectKeys.ApparelArmorFactor))
                    * (IsMaxLevel ? 1f + Props.finalLevelArmorBonus : 1f);
            }
        }

        /// <summary>利刃护甲倍率（总倍率 × 词条利刃加成）。</summary>
        public float ArmorSharpFactor
        {
            get { return ArmorFactor * (1f + GetEffectSum(ModuleEffectKeys.ApparelArmorSharpFactor)); }
        }

        /// <summary>钝器护甲倍率。</summary>
        public float ArmorBluntFactor
        {
            get { return ArmorFactor * (1f + GetEffectSum(ModuleEffectKeys.ApparelArmorBluntFactor)); }
        }

        /// <summary>热能护甲倍率。</summary>
        public float ArmorHeatFactor
        {
            get { return ArmorFactor * (1f + GetEffectSum(ModuleEffectKeys.ApparelArmorHeatFactor)); }
        }

        /// <summary>保暖倍率（词条加成 × 满级奖励）。</summary>
        public float InsulationColdFactor
        {
            get
            {
                return (1f + GetEffectSum(ModuleEffectKeys.ApparelInsulationColdFactor))
                    * (IsMaxLevel ? 1f + Props.finalLevelInsulationBonus : 1f);
            }
        }

        /// <summary>隔温倍率（词条加成 × 满级奖励）。</summary>
        public float InsulationHeatFactor
        {
            get
            {
                return (1f + GetEffectSum(ModuleEffectKeys.ApparelInsulationHeatFactor))
                    * (IsMaxLevel ? 1f + Props.finalLevelInsulationBonus : 1f);
            }
        }

        /// <summary>强化到下一级的成功率（满级为 0；结果 × 设置倍率）。</summary>
        public float NextLevelSuccessRate
        {
            get { return IsMaxLevel ? 0f : SuccessRateFor(NextLevel); }
        }

        /// <summary>强化到 targetLevel 级的成功率（XML 表缺项/越界时回退内置默认表）。</summary>
        public float SuccessRateFor(int targetLevel)
        {
            if (targetLevel < 1 || targetLevel > Props.maxLevel)
                return 0f;
            float baseRate;
            if (Props.successRates != null && Props.successRates.Count >= targetLevel)
                baseRate = Props.successRates[targetLevel - 1];
            else if (targetLevel <= DefaultRates.Length)
                baseRate = DefaultRates[targetLevel - 1];
            else
                return 0f;
            return MMTSettings.AdjustEnhanceRate(baseRate);
        }

        /// <summary>强化到下一级的材料条目（阶梯表）。</summary>
        public List<EnhanceMaterialOption> NextLevelOptions
        {
            get { return EnhanceLadder.OptionsFor(Props.materialTiers, NextLevel) ?? new List<EnhanceMaterialOption>(); }
        }

        /// <summary>IEnhanceable：强化 comp 所属装备。</summary>
        public Thing ParentThing
        {
            get { return parent; }
        }

        /// <summary>掷一次强化：按下一级成功率判定，成功则等级 +1。材料由作业侧先行消耗。</summary>
        public bool DoEnhance()
        {
            if (IsMaxLevel)
                return false;
            if (Rand.Chance(NextLevelSuccessRate))
            {
                level++;
                return true;
            }
            return false;
        }

        /// <summary>掉落掷点直接写入强化等级（仅 LootRoll 调用：不耗材料、不走成功率）。</summary>
        public void SetLevelForLoot(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 0, Props.maxLevel);
        }

        /// <summary>物品标签：首个带前缀的附魔词条加"前缀·"，再追加强化等级后缀。</summary>
        public override string TransformLabel(string label)
        {
            string prefix = null;
            for (int i = 0; i < enchants.Count; i++)
            {
                string p = enchants[i].GetModExtension<ModuleExtension>()?.labelPrefix;
                if (!p.NullOrEmpty())
                {
                    prefix = p;
                    break;
                }
            }
            string text = prefix != null ? prefix + "·" + label : label;
            return level > 0 ? text + " +" + level : text;
        }

        public override string CompInspectStringExtra()
        {
            if (level <= 0 && enchants.Count == 0)
                return null;
            string text = null;
            if (level > 0)
                text = "强化等级: +" + level + "（护甲 ×" + ArmorFactor.ToString("F2") + "）";
            if (enchants.Count > 0)
            {
                string names = "";
                for (int i = 0; i < enchants.Count; i++)
                {
                    if (i > 0)
                        names += "、";
                    names += enchants[i].label;
                }
                text = (text == null ? "" : text + "\n") + "附魔（" + enchants.Count + "/" + EnchantSlots + "）: " + names;
            }
            return text;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref level, "apparelEnhanceLevel", 0);
            Scribe_Collections.Look(ref enchants, "apparelEnchants", LookMode.Def);
            // 默认值必须写 true：旧存档缺节点时保持"危险特效开启"，与字段初值一致
            Scribe_Values.Look(ref triggerEffectsEnabled, "apparelTriggerFx", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && enchants == null)
                enchants = new List<ThingDef>();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            // 只有装了危险受击词条（荆棘/放电/毒雾）才出开关 gizmo
            if (GetEffectSum(ModuleEffectKeys.ApparelReflectDamage) + GetEffectSum(ModuleEffectKeys.ApparelShockBurst)
                + GetEffectSum(ModuleEffectKeys.ApparelToxicCloud) <= 0f)
                yield break;
            Command_Toggle toggle = new Command_Toggle();
            toggle.defaultLabel = "危险受击特效";
            toggle.defaultDesc = "关闭后此防具的荆棘反伤/放电/毒雾不再触发（防误伤友军与引燃基地）。";
            toggle.isActive = () => triggerEffectsEnabled;
            toggle.toggleAction = delegate { triggerEffectsEnabled = !triggerEffectsEnabled; };
            toggle.icon = parent.def.uiIcon;
            yield return toggle;
        }

        // ---- IWornDamageAbsorber（PreApplyDamage 统一补丁消费）：EMP 防护 + 承伤减免 ----

        public bool TryAbsorbDamage(Pawn pawn, ref DamageInfo dinfo)
        {
            if (!Eligible)
                return false;
            // 法拉第词条：全额吸收 EMP（含眩晕来源伤害归零）
            if (dinfo.Def == DamageDefOf.EMP && GetEffectSum(ModuleEffectKeys.ApparelEmpProtection) > 0f)
                return true;
            // 承伤减免词条（合计上限 50%）
            float reduce = Mathf.Min(0.5f, -GetEffectSum(ModuleEffectKeys.ApparelDamageTakenFactor));
            if (reduce > 0f)
                dinfo.SetAmount(dinfo.Amount * (1f - reduce));
            return false;
        }

        // ---- IWornPostDamageHandler（PostApplyDamage 统一补丁消费）：受击回血 + 反击特效 ----

        public void PostWearerDamageApplied(Pawn pawn, DamageInfo dinfo, float totalDamageDealt)
        {
            if (!Eligible || pawn == null || pawn.Dead)
                return;
            Map map = pawn.Map;
            Thing attacker = dinfo.Instigator;
            // 危险受击特效门控：防具 gizmo 开关 + 设置总开关（荆棘/放电/毒雾）
            bool hazardOn = triggerEffectsEnabled && MMTSettings.ApparelHazardEffects;

            // ---- 复苏：按承伤 × 系数治疗自身伤口（每次上限 2 点） ----
            float healOnHit = GetEffectSum(ModuleEffectKeys.ApparelHealOnHit);
            if (healOnHit > 0f && totalDamageDealt > 0f)
            {
                float budget = Mathf.Min(2f, totalDamageDealt * healOnHit);
                List<Hediff> injuries = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < injuries.Count && budget > 0f; i++)
                {
                    if (injuries[i] is Hediff_Injury injury && injury.Severity > 0f)
                    {
                        float heal = Mathf.Min(budget, injury.Severity);
                        injury.Severity -= heal;
                        budget -= heal;
                    }
                }
            }

            // ---- 荆棘（危险特效）：反弹承伤 × 系数给攻击者 ----
            float reflect = GetEffectSum(ModuleEffectKeys.ApparelReflectDamage);
            if (reflect > 0f && hazardOn && MMTSettings.EnableApparelThorns && totalDamageDealt > 0f
                && attacker != null && attacker != pawn && map != null)
            {
                DamageInfo reflectInfo = new DamageInfo(dinfo.Def, totalDamageDealt * reflect, 0f, -1f, pawn);
                attacker.TakeDamage(reflectInfo);
                MoteMaker.ThrowText(attacker.DrawPos, map, "荆棘", new Color(0.6f, 0.8f, 0.3f), 1f);
            }

            // ---- 放电（危险特效）：受击概率眩晕攻击者 90tick ----
            float shock = GetEffectSum(ModuleEffectKeys.ApparelShockBurst);
            if (shock > 0f && hazardOn && MMTSettings.EnableApparelDischarge && map != null
                && attacker is Pawn attackerPawn && !attackerPawn.Dead && attackerPawn.stances?.stunner != null
                && Rand.Chance(shock))
            {
                attackerPawn.stances.stunner.StunFor(90, pawn, false);
                MoteMaker.ThrowText(attackerPawn.DrawPos, map, "放电", new Color(0.6f, 0.8f, 1f), 1f);
            }

            // ---- 毒雾（危险特效）：受击概率在自身位置放毒气（Biotech ToxGas，无则回退攻击者毒性积累） ----
            float toxicCloud = GetEffectSum(ModuleEffectKeys.ApparelToxicCloud);
            if (toxicCloud > 0f && hazardOn && MMTSettings.EnableApparelToxicCloud && map != null
                && Rand.Chance(toxicCloud))
            {
                if (ModsConfig.BiotechActive)
                {
                    GasUtility.AddGas(pawn.Position, map, GasType.ToxGas, 2.9f);
                }
                else if (attacker is Pawn toxicVictim && !toxicVictim.Dead
                    && toxicVictim.RaceProps != null && toxicVictim.RaceProps.IsFlesh)
                {
                    Hediff buildup = toxicVictim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
                    if (buildup == null)
                    {
                        buildup = toxicVictim.health.AddHediff(HediffDefOf.ToxicBuildup);
                        buildup.Severity = 0.10f;
                    }
                    else
                    {
                        buildup.Severity += 0.10f;
                    }
                }
                MoteMaker.ThrowText(pawn.DrawPos, map, "毒雾", new Color(0.5f, 0.8f, 0.3f), 1f);
            }
        }
    }

    /// <summary>
    /// 防具强化数值统一补丁（ildump 核实 1.7 StatWorker.GetValueUnfinalized(StatRequest, bool) 有方法体，
    /// 且目标 stat 全部用默认 StatWorker 无子类重写）：
    /// 防具自身维度（护甲三防/保暖/隔温，乘系数）与穿戴者维度（移速/负重/闪避/工作速度/痛阈/射击精度/崩溃阈值/心灵敏感/愈合/免疫）。
    /// </summary>
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class Patch_ApparelEnhanceStats
    {
        /// <summary>StatWorker.stat 字段访问（1.7 为保护字段，Harmony FieldRef 免反射开销）。</summary>
        private static readonly AccessTools.FieldRef<StatWorker, StatDef> StatField =
            AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");

        public static void Postfix(StatWorker __instance, StatRequest req, ref float __result)
        {
            StatDef stat = __instance == null ? null : StatField(__instance);
            if (stat == null)
                return;
            // ---- 防具自身维度：护甲三防与保暖/隔温 ----
            if (req.Thing is Apparel apparel)
            {
                if (!EnhanceLadder.ApparelEquipEligible(apparel.def))
                    return;
                CompApparelEnhancement comp = apparel.TryGetComp<CompApparelEnhancement>();
                if (comp == null)
                    return;
                if (stat == StatDefOf.ArmorRating_Sharp)
                    __result *= comp.ArmorSharpFactor;
                else if (stat == StatDefOf.ArmorRating_Blunt)
                    __result *= comp.ArmorBluntFactor;
                else if (stat == StatDefOf.ArmorRating_Heat)
                    __result *= comp.ArmorHeatFactor;
                else if (stat == StatDefOf.Insulation_Cold)
                    __result *= comp.InsulationColdFactor;
                else if (stat == StatDefOf.Insulation_Heat)
                    __result *= comp.InsulationHeatFactor;
                return;
            }
            // ---- 穿戴者维度：移速/工作速度/射击精度/灵敏/愈合/免疫（乘法），负重/闪避/痛阈/崩溃阈值（加法） ----
            Pawn pawn = req.Pawn;
            if (pawn?.apparel == null)
                return;
            bool additive = stat == StatDefOf.CarryingCapacity || stat == StatDefOf.MeleeDodgeChance
                || stat == StatDefOf.PainShockThreshold || stat == StatDefOf.MentalBreakThreshold;
            bool multiplicative = stat == StatDefOf.MoveSpeed || stat == StatDefOf.WorkSpeedGlobal
                || stat == StatDefOf.ShootingAccuracyPawn || stat == StatDefOf.PsychicSensitivity
                || stat == StatDefOf.InjuryHealingFactor || stat == StatDefOf.ImmunityGainSpeedFactor;
            if (!additive && !multiplicative)
                return;
            float sum = 0f;
            foreach (Apparel ap in pawn.apparel.WornApparel)
            {
                CompApparelEnhancement comp = ap.TryGetComp<CompApparelEnhancement>();
                if (comp == null || !EnhanceLadder.ApparelEquipEligible(ap.def))
                    continue;
                if (stat == StatDefOf.MoveSpeed)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelMoveSpeed);
                else if (stat == StatDefOf.CarryingCapacity)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelCarryCapacity);
                else if (stat == StatDefOf.MeleeDodgeChance)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelDodgeChance);
                else if (stat == StatDefOf.WorkSpeedGlobal)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelWorkSpeedFactor);
                else if (stat == StatDefOf.PainShockThreshold)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelPainShockThreshold);
                else if (stat == StatDefOf.ShootingAccuracyPawn)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelShootingAccuracy);
                else if (stat == StatDefOf.MentalBreakThreshold)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelMentalThreshold);
                else if (stat == StatDefOf.PsychicSensitivity)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelPsychicSensitivity);
                else if (stat == StatDefOf.InjuryHealingFactor)
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelHealingFactor);
                else
                    sum += comp.GetEffectSum(ModuleEffectKeys.ApparelImmunitySpeed);
            }
            // 注意不能 sum <= 0 短路：硬撑/灵壁等词条本就是负值
            if (sum == 0f)
                return;
            if (additive)
                __result += sum;
            else
                __result *= 1f + sum;
        }
    }
}
