using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 武器强化参数：等级上限、每级伤害加成、满级穿甲、成功率表与材料阶梯（XML 可配置）。
    /// 经 XPath 补丁挂到全部带 weaponTags 的武器（跨 Mod 兼容，见 §2）。
    /// </summary>
    public class CompProperties_WeaponEnhancement : CompProperties
    {
        /// <summary>强化等级上限（默认 15）。</summary>
        public int maxLevel = 15;

        /// <summary>每级伤害加成（复利，0.05 = 每级 ×1.05，满级 ≈×2.08）。</summary>
        public float damageBonusPerLevel = 0.05f;

        /// <summary>满级额外护甲穿透（加法，0.10 = +10%）。</summary>
        public float finalLevelArmorPen = 0.10f;

        /// <summary>各级成功率表（第 i 项 = 强化到 i+1 级），缺省时用代码内置默认表。</summary>
        public List<float> successRates;

        /// <summary>材料阶梯表（目标等级 ≤ maxLevel 的第一个段位生效）。</summary>
        public List<EnhanceTierMaterials> materialTiers;

        public CompProperties_WeaponEnhancement()
        {
            compClass = typeof(CompWeaponEnhancement);
        }
    }

    /// <summary>
    /// 武器强化 Comp：强化等级（赌博线，伤害复利）+ 附魔词条列表（确定线，模块物品安装）。
    /// 数值效果由伤害/冷却/命中补丁读取本 comp 的聚合系数；特殊机制词条（值>0 启用）
    /// 在命中结算补丁统一处理。附魔槽位：基础 1，+7 解锁第 2 槽，+13 解锁第 3 槽。
    /// </summary>
    public class CompWeaponEnhancement : ThingComp, IEnhanceable
    {
        /// <summary>内置默认成功率表（强化到 1~15 级）：1-3 必成，4-6 80%，7-9 60%，10-12 40%，13-14 20%，15 级 5%。</summary>
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

        private CompProperties_WeaponEnhancement Props
        {
            get { return (CompProperties_WeaponEnhancement)props; }
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

        /// <summary>能否安装某词条：槽位未满 + 同词条不重复。</summary>
        public bool CanAddEnchant(ThingDef moduleDef)
        {
            return moduleDef != null && enchants.Count < EnchantSlots && !enchants.Contains(moduleDef);
        }

        /// <summary>安装词条（模块物品在作业侧已消耗）。</summary>
        public void AddEnchant(ThingDef moduleDef)
        {
            if (CanAddEnchant(moduleDef))
                enchants.Add(moduleDef);
        }

        /// <summary>词条效果合计（key 见 ModuleEffectKeys.Weapon*）。</summary>
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

        /// <summary>伤害总倍率：强化复利 × (1 + 词条伤害加成)。</summary>
        public float DamageFactor
        {
            get
            {
                return Mathf.Pow(1f + Props.damageBonusPerLevel, level)
                    * (1f + GetEffectSum(ModuleEffectKeys.WeaponDamageFactor));
            }
        }

        /// <summary>护甲穿透加值：词条合计 + 满级奖励。</summary>
        public float ArmorPenAdd
        {
            get
            {
                return GetEffectSum(ModuleEffectKeys.WeaponArmorPen)
                    + (IsMaxLevel ? Props.finalLevelArmorPen : 0f);
            }
        }

        /// <summary>冷却倍率（词条负值减冷却，保底 ×0.2）。</summary>
        public float CooldownFactor
        {
            get { return Mathf.Max(0.2f, 1f + GetEffectSum(ModuleEffectKeys.WeaponCooldownFactor)); }
        }

        /// <summary>瞄准时间倍率（词条负值减瞄准，保底 ×0.2）。</summary>
        public float WarmupFactor
        {
            get { return Mathf.Max(0.2f, 1f + GetEffectSum(ModuleEffectKeys.WeaponWarmupFactor)); }
        }

        /// <summary>命中倍率（词条负值降命中，保底 ×0.1）。</summary>
        public float HitChanceFactor
        {
            get { return Mathf.Max(0.1f, 1f + GetEffectSum(ModuleEffectKeys.WeaponHitChance)); }
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

        /// <summary>物品标签追加强化等级后缀（对任意 mod 武器 label 安全）。</summary>
        public override string TransformLabel(string label)
        {
            return level > 0 ? label + " +" + level : label;
        }

        public override string CompInspectStringExtra()
        {
            if (level <= 0 && enchants.Count == 0)
                return null;
            string text = null;
            if (level > 0)
                text = "强化等级: +" + level + "（伤害 ×" + DamageFactor.ToString("F2") + "）";
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
            Scribe_Values.Look(ref level, "weaponEnhanceLevel", 0);
            Scribe_Collections.Look(ref enchants, "weaponEnchants", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && enchants == null)
                enchants = new List<ThingDef>();
        }
    }

    /// <summary>武器强化台参数（目前仅 compClass 载体，预留 XML 扩展位）。</summary>
    public class CompProperties_WeaponEnhanceBench : CompProperties
    {
        public CompProperties_WeaponEnhanceBench()
        {
            compClass = typeof(CompWeaponEnhanceBench);
        }
    }

    /// <summary>
    /// 武器强化台建筑：绘制台内武器（中心上方飘浮），逻辑在 CompWeaponEnhanceBench。
    /// 继承 Building_WorkTable：附魔模块制作走配方账单，强化/附魔操作走右键作业。
    /// </summary>
    public class Building_WeaponEnhanceBench : Building_WorkTable
    {
        private CompWeaponEnhanceBench benchComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            benchComp = this.TryGetComp<CompWeaponEnhanceBench>();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Thing held = benchComp?.HeldItem;
            if (held == null)
                return;
            // 台内武器：中心正上方缓慢浮动（真实时间驱动，倍速不鬼畜）
            float bob = Mathf.Sin(Time.realtimeSinceStartup * 3f) * 0.06f;
            Vector3 pos = drawLoc;
            pos.y = AltitudeLayer.Item.AltitudeFor() + 0.05f;
            pos.z += 0.55f + bob;
            held.Graphic.Draw(pos, Rot4.North, held, 0f);
        }
    }

    /// <summary>
    /// 武器强化台核心 Comp：内置容器持 1 件武器，右键"装载武器/取出/强化/附魔"。
    /// 结构照搬 CompChargingStation（1.7 ThingComp 不自带 IThingHolder，显式实现）。
    /// </summary>
    public class CompWeaponEnhanceBench : ThingComp, IThingHolder, IEnhanceStation
    {
        /// <summary>内置容器：持有正在强化的武器（容量 1）。</summary>
        private ThingOwner<Thing> container;

        public CompWeaponEnhanceBench()
        {
            container = new ThingOwner<Thing>(this, LookMode.Deep, true);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return container;
        }

        /// <summary>台内武器；空台为 null。</summary>
        public Thing HeldItem
        {
            get { return container.Count > 0 ? container[0] : null; }
        }

        /// <summary>台内武器的强化 comp。</summary>
        public CompWeaponEnhancement Enhancement
        {
            get { return (HeldItem as ThingWithComps)?.TryGetComp<CompWeaponEnhancement>(); }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref container, "weaponEnhanceBenchContainer", this);
        }

        /// <summary>把武器装入强化台；台内已有武器时拒绝。</summary>
        public bool TryLoadWeapon(Thing weapon)
        {
            if (HeldItem != null || weapon == null)
                return false;
            return container.TryAdd(weapon);
        }

        /// <summary>取出台内武器；空台返回 null。</summary>
        public Thing TakeHeldItem()
        {
            Thing held = HeldItem;
            if (held != null)
                container.Remove(held);
            return held;
        }

        /// <summary>拆除/摧毁时把台内武器弹回地面。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            container.TryDropAll(parent.PositionHeld, map, ThingPlaceMode.Near);
            base.PostDeSpawn(map, mode);
        }

        // ---- 右键菜单 ----

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                yield return new FloatMenuOption("武器强化台（被禁止或在小人活动区域外）", null);
                yield break;
            }
            if (!selPawn.CanReserve(parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption("武器强化台（已被其他人预定）", null);
                yield break;
            }
            Thing held = HeldItem;
            if (held == null)
            {
                yield return new FloatMenuOption("装载武器", delegate { StartLoadSelection(selPawn); });
                yield break;
            }
            CompWeaponEnhancement enh = Enhancement;
            string name = held.LabelCap;
            yield return new FloatMenuOption("取出武器：" + name, delegate { OrderUnload(selPawn); });
            if (enh == null)
                yield break;
            // 强化入口（赌博线）
            if (enh.IsMaxLevel)
            {
                yield return new FloatMenuOption("强化武器：" + name + "（已达最高强化等级 +" + enh.Level + "）", null);
            }
            else
            {
                List<EnhanceMaterialOption> options = enh.NextLevelOptions;
                string label = "强化武器：" + name + " → +" + enh.NextLevel
                    + "（成功率 " + enh.NextLevelSuccessRate.ToStringPercent("F0")
                    + "，需 " + EnhanceLadder.Describe(options, enh.NextLevel) + "）";
                List<ResolvedMaterial> resolved = EnhanceLadder.Resolve(options, selPawn.Map, selPawn, enh.NextLevel);
                bool enough = true;
                foreach (ResolvedMaterial mat in resolved)
                {
                    if (EnhanceLadder.CountAvailable(selPawn.Map, selPawn, mat.def) < mat.count)
                    {
                        enough = false;
                        break;
                    }
                }
                if (!enough)
                    yield return new FloatMenuOption(label + "——材料不足", null);
                else
                    yield return new FloatMenuOption(label, delegate { OrderEnhance(selPawn); });
            }
            // 附魔入口（确定线）
            if (enh.Enchants.Count >= enh.EnchantSlots)
            {
                yield return new FloatMenuOption("附魔：" + name + "（槽位已满 " + enh.Enchants.Count + "/" + enh.EnchantSlots + "）", null);
            }
            else
            {
                yield return new FloatMenuOption("附魔武器：" + name + "（槽位 " + enh.Enchants.Count + "/" + enh.EnchantSlots + "）",
                    delegate { StartEnchantSelection(selPawn); });
            }
        }

        /// <summary>装载入口：列出小人身上（主手 + 背包）可强化的武器。</summary>
        private void StartLoadSelection(Pawn selPawn)
        {
            List<Thing> weapons = FindEnhanceableWeapons(selPawn);
            if (weapons.Count == 0)
            {
                Messages.Message(selPawn.LabelShort + " 身上没有可强化的武器（需要装备或携带武器）",
                    selPawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (weapons.Count == 1)
            {
                OrderLoad(selPawn, weapons[0]);
                return;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Thing w in weapons)
            {
                Thing local = w;
                options.Add(new FloatMenuOption(w.LabelCap, delegate { OrderLoad(selPawn, local); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>收集小人身上（主手装备 + 背包）所有可强化武器（有强化 comp 且未被设置排除）。</summary>
        private static List<Thing> FindEnhanceableWeapons(Pawn pawn)
        {
            List<Thing> list = new List<Thing>();
            Thing primary = pawn.equipment?.Primary;
            if (primary != null && primary.TryGetComp<CompWeaponEnhancement>() != null
                && EnhanceLadder.EquipEligible(primary.def))
                list.Add(primary);
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (t.TryGetComp<CompWeaponEnhancement>() != null && EnhanceLadder.EquipEligible(t.def))
                        list.Add(t);
                }
            }
            return list;
        }

        /// <summary>附魔入口：列出地图上与本小人背包中可用的附魔模块（槽位类别 WeaponEnchant）。</summary>
        private void StartEnchantSelection(Pawn selPawn)
        {
            CompWeaponEnhancement enh = Enhancement;
            if (enh == null)
                return;
            List<Thing> modules = FindEnchantModules(selPawn);
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Thing m in modules)
            {
                Thing local = m;
                if (!enh.CanAddEnchant(m.def))
                    continue;
                ModuleExtension ext = m.def.GetModExtension<ModuleExtension>();
                string effect = ext != null ? ext.effectReport : "";
                options.Add(new FloatMenuOption(m.def.LabelCap + "（" + effect + "）",
                    delegate { OrderEnchant(selPawn, local); }));
            }
            if (options.Count == 0)
            {
                Messages.Message("没有可用的附魔模块（在强化台制作附魔模块后放入背包或地图储存区）",
                    selPawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>收集地图（未禁止）与小人背包中的附魔模块物品。</summary>
        private static List<Thing> FindEnchantModules(Pawn pawn)
        {
            List<Thing> list = new List<Thing>();
            if (pawn.Map != null)
            {
                foreach (Thing t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                {
                    if (t.IsForbidden(pawn) || !IsEnchantModule(t.def))
                        continue;
                    list.Add(t);
                }
            }
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (IsEnchantModule(t.def))
                        list.Add(t);
                }
            }
            return list;
        }

        /// <summary>是否附魔模块（ModuleExtension 槽位类别 = WeaponEnchant）。</summary>
        public static bool IsEnchantModule(ThingDef def)
        {
            ModuleExtension ext = def?.GetModExtension<ModuleExtension>();
            return ext != null && ext.slotKey == "WeaponEnchant";
        }

        private void OrderLoad(Pawn pawn, Thing weapon)
        {
            Job job = JobMaker.MakeJob(MMT_WeaponEnhanceDefOf.MMT_LoadEnhanceBench, parent, weapon);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private void OrderUnload(Pawn pawn)
        {
            if (HeldItem == null)
            {
                Messages.Message("强化台内没有武器可取", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(pawn.LabelShort + " 无法到达强化台（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_WeaponEnhanceDefOf.MMT_UnloadEnhanceBench, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private void OrderEnhance(Pawn pawn)
        {
            if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(pawn.LabelShort + " 无法到达强化台（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_WeaponEnhanceDefOf.MMT_EnhanceWeapon, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private void OrderEnchant(Pawn pawn, Thing module)
        {
            if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(pawn.LabelShort + " 无法到达强化台（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_WeaponEnhanceDefOf.MMT_EnchantWeapon, parent, module);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override string CompInspectStringExtra()
        {
            Thing held = HeldItem;
            if (held == null)
                return "状态: 待机（台内没有武器，右键装载武器）";
            return "武器: " + held.LabelCap;
        }
    }

    /// <summary>
    /// 装载武器作业（targetA=强化台，targetB=武器）：武器在小人身上（主手/背包），走到强化台装入。
    /// </summary>
    public class JobDriver_LoadEnhanceBench : JobDriver
    {
        private CompWeaponEnhanceBench Bench
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompWeaponEnhanceBench>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定强化台（武器在小人身上，装入前再校验）
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => Bench == null || Bench.HeldItem != null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.DoAtomic(delegate
            {
                CompWeaponEnhanceBench bench = Bench;
                Thing weapon = job.GetTarget(TargetIndex.B).Thing;
                if (bench == null || weapon == null || bench.HeldItem != null)
                    return;
                // 只接受从小人身上取出：主手装备则卸下，背包中则移除
                if (pawn.equipment != null && pawn.equipment.Primary == weapon)
                    pawn.equipment.Remove((ThingWithComps)weapon);
                else if (pawn.inventory != null && pawn.inventory.innerContainer.Contains(weapon))
                    pawn.inventory.innerContainer.Remove(weapon);
                else
                    return;
                bench.TryLoadWeapon(weapon);
            });
        }
    }

    /// <summary>
    /// 取出武器作业（targetA=强化台）：优先主手装备，其次背包，放不下放脚边。
    /// </summary>
    public class JobDriver_UnloadEnhanceBench : JobDriver
    {
        private CompWeaponEnhanceBench Bench
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompWeaponEnhanceBench>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => Bench == null || Bench.HeldItem == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.DoAtomic(delegate
            {
                CompWeaponEnhanceBench bench = Bench;
                if (bench == null)
                    return;
                Thing held = bench.TakeHeldItem();
                if (held == null)
                    return;
                // 空手优先装备到主手，否则背包/脚边
                if (held is ThingWithComps heldComps && pawn.equipment != null && pawn.equipment.Primary == null
                    && held.def.IsWeapon)
                {
                    pawn.equipment.AddEquipment(heldComps);
                    return;
                }
                if (pawn.inventory == null || !pawn.inventory.innerContainer.TryAdd(held))
                    GenPlace.TryPlaceThing(held, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            });
        }
    }

    /// <summary>
    /// 附魔作业（targetA=强化台，targetB=附魔模块）：模块在地图则先取货，走到强化台安装词条。
    /// 100% 成功（材料成本在模块制作时已付），同词条不可重复、槽位受强化等级限制。
    /// </summary>
    public class JobDriver_EnchantWeapon : JobDriver
    {
        private const int EnchantDurationTicks = 600;

        private CompWeaponEnhanceBench Bench
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompWeaponEnhanceBench>() : null;
            }
        }

        private Thing Module
        {
            get { return job.GetTarget(TargetIndex.B).Thing; }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            // 台内武器被取走/词条不再可装（重复或槽位满）则中止；模块在背包里视为有效
            this.FailOn(() => Bench?.Enhancement == null || Module == null
                || !Bench.Enhancement.CanAddEnchant(Module.def));
            // ---- 流程开始：模块不在背包则先取货 ----
            Toil collectDone = Toils_General.Label();
            yield return Toils_Jump.JumpIf(collectDone,
                () => pawn.inventory.innerContainer.Contains(Module));
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.TakeToInventory(TargetIndex.B, 1);
            yield return collectDone;
            // ---- 走到强化台安装 ----
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            Toil enchant = Toils_General.Wait(EnchantDurationTicks, TargetIndex.A);
            enchant.WithProgressBarToilDelay(TargetIndex.A);
            yield return enchant;
            yield return Toils_General.DoAtomic(FinishEnchant);
            // ---- 流程结束 ----
        }

        private void FinishEnchant()
        {
            CompWeaponEnhancement enh = Bench?.Enhancement;
            Thing module = Module;
            if (enh == null || module == null || !enh.CanAddEnchant(module.def))
                return;
            // 消耗模块物品（背包中移除销毁），词条装入武器
            if (pawn.inventory.innerContainer.Contains(module))
                pawn.inventory.innerContainer.Remove(module);
            ThingDef moduleDef = module.def;
            module.Destroy(DestroyMode.Vanish);
            enh.AddEnchant(moduleDef);
            Thing station = job.GetTarget(TargetIndex.A).Thing;
            MoteMaker.ThrowText(station.DrawPos, Map, "附魔完成", Color.green, 3f);
            Messages.Message("附魔完成：" + enh.parent.LabelCap + " 获得词条「" + moduleDef.label + "」",
                new LookTargets(station), MessageTypeDefOf.PositiveEvent, false);
        }
    }

    // ======================================================================
    // 伤害/命中/冷却挂钩补丁（签名经 ildump 核实：1.7.9819）
    // ======================================================================

    /// <summary>武器强化补丁公共助手。</summary>
    public static class WeaponEnhancePatches
    {
        /// <summary>CE 探测：CE 重写弹道/伤害链路，远程数值补丁可能失效（近战线不受影响）。</summary>
        public static readonly bool CombatExtendedActive =
            ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null;

        /// <summary>Projectile.equipment 字段访问（1.7 为保护字段，Harmony FieldRef 免反射开销）。</summary>
        public static readonly AccessTools.FieldRef<Projectile, Thing> ProjectileEquipment =
            AccessTools.FieldRefAccess<Projectile, Thing>("equipment");

        /// <summary>取装备上生效的强化 comp（含设置排除判定）。</summary>
        public static CompWeaponEnhancement ActiveEnhancement(Thing equipment)
        {
            if (equipment is ThingWithComps twc && EnhanceLadder.EquipEligible(twc.def))
                return twc.TryGetComp<CompWeaponEnhancement>();
            return null;
        }
    }

    /// <summary>远程伤害：Projectile.get_DamageAmount（1.7 为属性）× 强化伤害倍率。</summary>
    [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
    public static class Patch_WeaponDamageRanged
    {
        public static void Postfix(Projectile __instance, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(
                __instance == null ? null : WeaponEnhancePatches.ProjectileEquipment(__instance));
            if (comp != null)
                __result *= comp.DamageFactor;
        }
    }

    /// <summary>远程穿甲：Projectile.get_ArmorPenetration + 强化穿甲加值。</summary>
    [HarmonyPatch(typeof(Projectile), "get_ArmorPenetration")]
    public static class Patch_WeaponArmorPenRanged
    {
        public static void Postfix(Projectile __instance, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(
                __instance == null ? null : WeaponEnhancePatches.ProjectileEquipment(__instance));
            if (comp != null)
                __result += comp.ArmorPenAdd;
        }
    }

    /// <summary>远程命中：ShotReport.HitReportFor 的装备命中因子 × 词条倍率。
    /// 注意 ShotReport 是 struct：__result 必须 ref，factorFromEquipment 用 StructFieldRef 原地改。</summary>
    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public static class Patch_WeaponHitChance
    {
        private static readonly AccessTools.StructFieldRef<ShotReport, float> FactorFromEquipment =
            AccessTools.StructFieldRefAccess<ShotReport, float>("factorFromEquipment");

        public static void Postfix(ref ShotReport __result, Verb verb)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(verb?.EquipmentSource);
            if (comp == null)
                return;
            ref float factor = ref FactorFromEquipment(ref __result);
            factor *= comp.HitChanceFactor;
        }
    }

    /// <summary>远程冷却：VerbProperties.AdjustedCooldownTicks × 词条冷却倍率。</summary>
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldownTicks), typeof(Verb), typeof(Pawn))]
    public static class Patch_WeaponCooldownRanged
    {
        public static void Postfix(Verb ownerVerb, ref int __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(ownerVerb?.EquipmentSource);
            if (comp != null)
                __result = Mathf.Max(1, Mathf.RoundToInt(__result * comp.CooldownFactor));
        }
    }

    /// <summary>瞄准时间：Verb.get_WarmupTime × 词条瞄准倍率。</summary>
    [HarmonyPatch(typeof(Verb), "get_WarmupTime")]
    public static class Patch_WeaponWarmup
    {
        public static void Postfix(Verb __instance, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(__instance?.EquipmentSource);
            if (comp != null)
                __result *= comp.WarmupFactor;
        }
    }

    /// <summary>近战伤害：Tool.AdjustedBaseMeleeDamageAmount（实例重载）× 强化伤害倍率。</summary>
    [HarmonyPatch(typeof(Tool), nameof(Tool.AdjustedBaseMeleeDamageAmount), typeof(Thing), typeof(DamageDef))]
    public static class Patch_WeaponDamageMelee
    {
        public static void Postfix(Thing ownerEquipment, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(ownerEquipment);
            if (comp != null)
                __result *= comp.DamageFactor;
        }
    }

    /// <summary>近战冷却：Tool.AdjustedCooldown（实例重载）× 词条冷却倍率。</summary>
    [HarmonyPatch(typeof(Tool), nameof(Tool.AdjustedCooldown), typeof(Thing))]
    public static class Patch_WeaponCooldownMelee
    {
        public static void Postfix(Thing ownerEquipment, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(ownerEquipment);
            if (comp != null)
                __result = Mathf.Max(0.2f, __result * comp.CooldownFactor);
        }
    }

    /// <summary>远程命中特效结算：暴击/处决/EMP/燃烧/冰霜/破盾/连锁闪电/时空折叠（每次命中一次，非热点）。</summary>
    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Patch_WeaponOnHitRanged
    {
        public static void Postfix(Projectile __instance, Thing hitThing, bool blockedByShield)
        {
            if (hitThing == null || __instance?.Map == null)
                return;
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(WeaponEnhancePatches.ProjectileEquipment(__instance));
            if (comp == null)
                return;
            WeaponOnHitMechanics.Apply(comp, __instance.Launcher, hitThing,
                __instance.DamageAmount, __instance.DamageDef, blockedByShield, false, 0f, __instance.Map);
        }
    }

    /// <summary>近战命中特效结算（含吸血/击退，近战专属词条在这里消费）。</summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "ApplyMeleeDamageToTarget")]
    public static class Patch_WeaponOnHitMelee
    {
        public static void Postfix(Verb_MeleeAttack __instance, LocalTargetInfo target, DamageWorker.DamageResult __result)
        {
            Thing hitThing = target.Thing;
            if (hitThing == null || __instance?.CasterPawn == null)
                return;
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(__instance.EquipmentSource);
            if (comp == null)
                return;
            WeaponOnHitMechanics.Apply(comp, __instance.CasterPawn, hitThing,
                __result.totalDamageDealt, null, false, true, __result.totalDamageDealt, __instance.CasterPawn.Map);
        }
    }

    /// <summary>
    /// 武器命中特殊机制统一结算（远程/近战共用）。
    /// baseDamage 已含强化倍率；meleeDealt 为近战实际造成的伤害（吸血按比例回治疗）。
    /// </summary>
    public static class WeaponOnHitMechanics
    {
        /// <summary>冰霜减速 hediff（惰性解析）。</summary>
        private static HediffDef frostDef;
        private static HediffDef FrostDef
        {
            get { return frostDef ?? (frostDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_Hediff_FrostSlow")); }
        }

        /// <summary>CompShield.energy 字段访问（保护字段，Harmony FieldRef 免反射开销）。</summary>
        private static readonly AccessTools.FieldRef<CompShield, float> ShieldEnergyField =
            AccessTools.FieldRefAccess<CompShield, float>("energy");

        /// <summary>CompShield.Break 方法委托（保护方法）。</summary>
        private static readonly Action<CompShield> ShieldBreak =
            AccessTools.MethodDelegate<Action<CompShield>>(AccessTools.Method(typeof(CompShield), "Break"));

        public static void Apply(CompWeaponEnhancement comp, Thing attacker, Thing hitThing,
            float baseDamage, DamageDef damageDef, bool blockedByShield, bool isMelee, float meleeDealt, Map map)
        {
            Pawn victim = hitThing as Pawn;

            // ---- 破盾：攻击被护盾拦截时，额外消耗护盾能量（不触发其它命中特效） ----
            float shieldBreaker = comp.GetEffectSum(ModuleEffectKeys.WeaponShieldBreaker);
            if (blockedByShield)
            {
                if (shieldBreaker > 0f && victim != null && victim.apparel != null)
                {
                    foreach (Apparel ap in victim.apparel.WornApparel)
                    {
                        CompShield shield = ap.TryGetComp<CompShield>();
                        if (shield != null && shield.ShieldState == ShieldState.Active)
                        {
                            // energy 为保护字段：FieldRef 原地扣减（按护盾自身换能比折算），扣穿则破盾
                            float energy = ShieldEnergyField(shield) - baseDamage * shieldBreaker * shield.Props.energyLossPerDamage;
                            ShieldEnergyField(shield) = energy;
                            if (energy <= 0f)
                                ShieldBreak(shield);
                            break;
                        }
                    }
                }
                return;
            }

            // ---- 暴击：掷词条暴击率，命中追加一次等额伤害（合计 ×2） ----
            float critChance = comp.GetEffectSum(ModuleEffectKeys.WeaponCritChance);
            if (critChance > 0f && baseDamage > 0f && Rand.Chance(critChance))
            {
                hitThing.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage, 0f, -1f, attacker));
                MoteMaker.ThrowText(hitThing.DrawPos, map, "暴击!", Color.yellow, 1.5f);
            }

            // ---- 处决：目标血量 <20% 时追加等额伤害（合计 ×2） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponExecute) > 0f && victim != null && !victim.Dead
                && victim.health.summaryHealth.SummaryHealthPercent < 0.2f && baseDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage, 0f, -1f, attacker));
                MoteMaker.ThrowText(victim.DrawPos, map, "处决!", Color.red, 1.5f);
            }

            // ---- EMP 弹：对机械追加 50% EMP 伤害并短路眩晕 ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponEmp) > 0f && victim != null
                && victim.RaceProps != null && victim.RaceProps.IsMechanoid && baseDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(DamageDefOf.EMP, baseDamage * 0.5f, 0f, -1f, attacker));
                if (!victim.Dead && victim.stances?.stunner != null)
                    victim.stances.stunner.StunFor(120, attacker, false);
            }

            // ---- 燃烧弹：命中点燃 ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponIncendiary) > 0f)
                FireUtility.TryAttachFire(hitThing, 0.25f, attacker);

            // ---- 冰霜：命中附加减速 hediff（-20% 移速，3 秒；重复命中刷新时长） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponFrost) > 0f && victim != null && !victim.Dead && FrostDef != null)
            {
                Hediff old = victim.health.hediffSet.GetFirstHediffOfDef(FrostDef);
                if (old != null)
                    victim.health.RemoveHediff(old);
                victim.health.AddHediff(FrostDef);
            }

            // ---- 吸血（近战专属）：实际伤害的百分比转为攻击者治疗 ----
            float leech = comp.GetEffectSum(ModuleEffectKeys.WeaponLifeLeech);
            if (isMelee && leech > 0f && meleeDealt > 0f && attacker is Pawn attackerPawn)
            {
                float budget = meleeDealt * leech;
                List<Hediff> injuries = attackerPawn.health.hediffSet.hediffs;
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

            // ---- 击退（近战专属）：目标踉跄 ----
            float knockback = comp.GetEffectSum(ModuleEffectKeys.WeaponKnockback);
            if (isMelee && knockback > 0f && victim != null && !victim.Dead && victim.stances?.stagger != null)
                victim.stances.stagger.StaggerFor(Mathf.RoundToInt(60f * knockback), 0.5f);

            // ---- 连锁闪电：命中弹射 2 格内另一敌对目标，50% 伤害 ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponChainLightning) > 0f && baseDamage > 0f && map != null)
            {
                Pawn chainTarget = null;
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(hitThing.Position, 2.9f, true))
                {
                    if (!cell.InBounds(map))
                        continue;
                    Pawn candidate = cell.GetFirstPawn(map);
                    if (candidate == null || candidate == hitThing || candidate.Dead || candidate.Downed)
                        continue;
                    if (attacker != null && !candidate.HostileTo(attacker))
                        continue;
                    chainTarget = candidate;
                    break;
                }
                if (chainTarget != null)
                {
                    chainTarget.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage * 0.5f, 0f, -1f, attacker));
                    MoteMaker.ThrowText(chainTarget.DrawPos, map, "连锁", Color.cyan, 1f);
                }
            }

            // ---- 时空折叠：击杀后攻击者本次冷却归零（取消冷却姿态） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponTimeFold) > 0f && victim != null && victim.Dead
                && attacker is Pawn shooter && shooter.stances != null
                && shooter.stances.curStance is Stance_Cooldown)
            {
                shooter.stances.SetStance(new Stance_Mobile());
                MoteMaker.ThrowText(shooter.DrawPos, map, "时空折叠", Color.white, 1f);
            }
        }
    }

    [DefOf]
    public static class MMT_WeaponEnhanceDefOf
    {
        /// <summary>装载武器作业。</summary>
        public static JobDef MMT_LoadEnhanceBench;
        /// <summary>取出武器作业。</summary>
        public static JobDef MMT_UnloadEnhanceBench;
        /// <summary>强化武器作业。</summary>
        public static JobDef MMT_EnhanceWeapon;
        /// <summary>附魔武器作业。</summary>
        public static JobDef MMT_EnchantWeapon;

        static MMT_WeaponEnhanceDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_WeaponEnhanceDefOf));
        }
    }
}
