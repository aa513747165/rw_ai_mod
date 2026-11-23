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
    public class CompWeaponEnhancement : ThingComp, IEnchantable
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

        /// <summary>危险命中特效开关（爆炸/引雷/毒气/燃烧；默认开，存档持久化，旧档缺省回退 true）。</summary>
        private bool triggerEffectsEnabled = true;

        /// <summary>本 tick 的必中掷骰缓存 tick（UI 命中预览与实际射击共享同一结果，不持久化）。</summary>
        private int lastSureHitTick = -1;
        private bool lastSureHitResult;

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

        /// <summary>IEnchantable：本装备接受的模块槽位类别。</summary>
        public string ModuleSlotKey
        {
            get { return "WeaponEnchant"; }
        }

        /// <summary>危险命中特效是否开启（爆炸/引雷/毒气/燃烧；gizmo 开关）。</summary>
        public bool TriggerEffectsEnabled
        {
            get { return triggerEffectsEnabled; }
        }

        /// <summary>本 tick 的必中掷骰（同一 tick 内 UI 预览与实际射击共享同一结果）。</summary>
        public bool RollGuaranteedHit(float chance)
        {
            if (Find.TickManager == null)
                return false;
            int tick = Find.TickManager.TicksGame;
            if (tick != lastSureHitTick)
            {
                lastSureHitTick = tick;
                lastSureHitResult = Rand.Chance(chance);
            }
            return lastSureHitResult;
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

        /// <summary>射程加值（词条合计加法，格；仅远程 verb 有意义）。</summary>
        public float RangeBonus
        {
            get { return GetEffectSum(ModuleEffectKeys.WeaponRangeBonus); }
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

        /// <summary>物品标签：首个带前缀的附魔词条加"前缀·"，再追加强化等级后缀（多词条只取第一个，防交易/堆叠界面超长）。</summary>
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
            // 默认值必须写 true：旧存档缺节点时保持"危险特效开启"，与字段初值一致
            Scribe_Values.Look(ref triggerEffectsEnabled, "weaponTriggerFx", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && enchants == null)
                enchants = new List<ThingDef>();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;
            // 只有装了危险词条（爆炸/引雷/毒气/燃烧/烟幕/尸爆）才出开关 gizmo
            if (GetEffectSum(ModuleEffectKeys.WeaponExplode) + GetEffectSum(ModuleEffectKeys.WeaponStormCall)
                + GetEffectSum(ModuleEffectKeys.WeaponGas) + GetEffectSum(ModuleEffectKeys.WeaponIncendiary)
                + GetEffectSum(ModuleEffectKeys.WeaponSmoke) + GetEffectSum(ModuleEffectKeys.WeaponCorpseBurst) <= 0f)
                yield break;
            Command_Toggle toggle = new Command_Toggle();
            toggle.defaultLabel = "危险命中特效";
            toggle.defaultDesc = "关闭后此武器的爆炸/引雷/毒气/燃烧/烟幕/尸爆不再触发（防误伤与引燃基地）。";
            toggle.isActive = () => triggerEffectsEnabled;
            toggle.toggleAction = delegate { triggerEffectsEnabled = !triggerEffectsEnabled; };
            toggle.icon = parent.def.uiIcon;
            yield return toggle;
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

        /// <summary>台内装备的强化 comp（武器或防具，经 IEnchantable 对两类透明）。</summary>
        public IEnchantable Enhancement
        {
            get
            {
                ThingWithComps held = HeldItem as ThingWithComps;
                if (held == null)
                    return null;
                IEnchantable enh = held.TryGetComp<CompWeaponEnhancement>();
                if (enh != null)
                    return enh;
                return held.TryGetComp<CompApparelEnhancement>();
            }
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
                yield return new FloatMenuOption("装载装备", delegate { StartLoadSelection(selPawn); });
                yield break;
            }
            IEnchantable enh = Enhancement;
            string name = held.LabelCap;
            yield return new FloatMenuOption("取出：" + name, delegate { OrderUnload(selPawn); });
            if (enh == null)
                yield break;
            // 强化入口（赌博线）
            if (enh.IsMaxLevel)
            {
                yield return new FloatMenuOption("强化装备：" + name + "（已达最高强化等级 +" + enh.Level + "）", null);
            }
            else
            {
                List<EnhanceMaterialOption> options = enh.NextLevelOptions;
                string label = "强化装备：" + name + " → +" + enh.NextLevel
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
            // 附魔入口（确定线；槽满也可附魔——新词条直接覆盖最早装的词条）
            if (enh.Enchants.Count >= enh.EnchantSlots && enh.Enchants.Count > 0)
            {
                yield return new FloatMenuOption("附魔装备：" + name + "（槽位已满，新词条将覆盖「" + enh.Enchants[0].label + "」）",
                    delegate { StartEnchantSelection(selPawn); });
            }
            else
            {
                yield return new FloatMenuOption("附魔装备：" + name + "（槽位 " + enh.Enchants.Count + "/" + enh.EnchantSlots + "）",
                    delegate { StartEnchantSelection(selPawn); });
            }
        }

        /// <summary>装载入口：列出小人身上（主手 + 穿戴 + 背包）可强化的装备。</summary>
        private void StartLoadSelection(Pawn selPawn)
        {
            List<Thing> items = FindEnhanceableItems(selPawn);
            if (items.Count == 0)
            {
                Messages.Message(selPawn.LabelShort + " 身上没有可强化的装备（需要装备武器/穿戴防具或随身携带）",
                    selPawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (items.Count == 1)
            {
                OrderLoad(selPawn, items[0]);
                return;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Thing w in items)
            {
                Thing local = w;
                options.Add(new FloatMenuOption(w.LabelCap, delegate { OrderLoad(selPawn, local); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>收集小人身上（主手武器 + 穿戴防具 + 背包）所有可强化装备（有强化 comp 且未被设置排除）。</summary>
        private static List<Thing> FindEnhanceableItems(Pawn pawn)
        {
            List<Thing> list = new List<Thing>();
            Thing primary = pawn.equipment?.Primary;
            if (primary != null && primary.TryGetComp<CompWeaponEnhancement>() != null
                && EnhanceLadder.EquipEligible(primary.def))
                list.Add(primary);
            // 穿戴防具（照搬充能台遍历 WornApparel 的先例）
            if (pawn.apparel != null)
            {
                foreach (Apparel ap in pawn.apparel.WornApparel)
                {
                    if (ap.TryGetComp<CompApparelEnhancement>() != null && EnhanceLadder.ApparelEquipEligible(ap.def))
                        list.Add(ap);
                }
            }
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (t.TryGetComp<CompWeaponEnhancement>() != null && EnhanceLadder.EquipEligible(t.def))
                        list.Add(t);
                    else if (t.TryGetComp<CompApparelEnhancement>() != null && EnhanceLadder.ApparelEquipEligible(t.def))
                        list.Add(t);
                }
            }
            return list;
        }

        /// <summary>附魔入口：列出地图上与本小人背包中可用的附魔模块（按台内装备的槽位类别过滤）。</summary>
        private void StartEnchantSelection(Pawn selPawn)
        {
            IEnchantable enh = Enhancement;
            if (enh == null)
                return;
            List<Thing> modules = FindEnchantModules(selPawn, enh.ModuleSlotKey);
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

        /// <summary>收集地图（未禁止）与小人背包中指定槽位类别的附魔模块物品。</summary>
        private static List<Thing> FindEnchantModules(Pawn pawn, string slotKey)
        {
            List<Thing> list = new List<Thing>();
            if (pawn.Map != null)
            {
                foreach (Thing t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                {
                    if (t.IsForbidden(pawn) || !IsEnchantModule(t.def, slotKey))
                        continue;
                    list.Add(t);
                }
            }
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (IsEnchantModule(t.def, slotKey))
                        list.Add(t);
                }
            }
            return list;
        }

        /// <summary>是否指定槽位类别的附魔模块（ModuleExtension.slotKey 匹配；武器 WeaponEnchant / 防具 ApparelEnchant）。</summary>
        public static bool IsEnchantModule(ThingDef def, string slotKey)
        {
            ModuleExtension ext = def?.GetModExtension<ModuleExtension>();
            return ext != null && ext.slotKey == slotKey;
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
                Messages.Message("强化台内没有装备可取", parent, MessageTypeDefOf.RejectInput, false);
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
                return "状态: 待机（台内没有装备，右键装载武器或防具）";
            return "装备: " + held.LabelCap;
        }
    }

    /// <summary>
    /// 装载装备作业（targetA=强化台，targetB=装备）：装备在小人身上（主手/穿戴/背包），走到强化台装入。
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
            // 只预定强化台（装备在小人身上，装入前再校验）
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
                Thing item = job.GetTarget(TargetIndex.B).Thing;
                if (bench == null || item == null || bench.HeldItem != null)
                    return;
                // 只接受从小人身上取出：主手装备则卸下，穿戴中防具则脱下，背包中则移除
                if (pawn.equipment != null && pawn.equipment.Primary == item)
                    pawn.equipment.Remove((ThingWithComps)item);
                else if (item is Apparel ap && pawn.apparel != null && pawn.apparel.WornApparel.Contains(ap))
                    pawn.apparel.Remove(ap);
                else if (pawn.inventory != null && pawn.inventory.innerContainer.Contains(item))
                    pawn.inventory.innerContainer.Remove(item);
                else
                    return;
                bench.TryLoadWeapon(item);
            });
        }
    }

    /// <summary>
    /// 取出装备作业（targetA=强化台）：武器空手优先装备到主手，防具优先穿回身上，其次背包，放不下放脚边。
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
                // 防具优先穿回身上（层位冲突穿不上则落回背包/脚边兜底）
                if (held is Apparel apparel && pawn.apparel != null
                    && pawn.apparel.CanWearWithoutDroppingAnything(apparel.def))
                {
                    pawn.apparel.Wear(apparel);
                    return;
                }
                // 武器空手优先装备到主手
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
            IEnchantable enh = Bench?.Enhancement;
            Thing module = Module;
            if (enh == null || module == null || !enh.CanAddEnchant(module.def))
                return;
            // 消耗模块物品（背包中移除销毁），词条装入装备（槽满时覆盖最早装的词条）
            if (pawn.inventory.innerContainer.Contains(module))
                pawn.inventory.innerContainer.Remove(module);
            ThingDef moduleDef = module.def;
            module.Destroy(DestroyMode.Vanish);
            ThingDef overwritten = enh.AddEnchant(moduleDef);
            Thing station = job.GetTarget(TargetIndex.A).Thing;
            MoteMaker.ThrowText(station.DrawPos, Map, "附魔完成", Color.green, 3f);
            string msg = "附魔完成：" + enh.ParentThing.LabelCap + " 获得词条「" + moduleDef.label + "」";
            if (overwritten != null)
                msg += "（覆盖了旧词条「" + overwritten.label + "」）";
            Messages.Message(msg, new LookTargets(station), MessageTypeDefOf.PositiveEvent, false);
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

    /// <summary>远程伤害：Projectile.get_DamageAmount（1.7 为 int 属性）× 强化伤害倍率（四舍五入取整）。</summary>
    [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
    public static class Patch_WeaponDamageRanged
    {
        public static void Postfix(Projectile __instance, ref int __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(
                __instance == null ? null : WeaponEnhancePatches.ProjectileEquipment(__instance));
            if (comp != null)
                __result = UnityEngine.Mathf.RoundToInt(__result * comp.DamageFactor);
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
            // ---- 必中词条：每发掷一次（按 tick 缓存，UI 预览与实射一致），命中因子拉满即必中 ----
            float sureHit = comp.GetEffectSum(ModuleEffectKeys.WeaponGuaranteedHit);
            if (sureHit > 0f && MMTSettings.EnableWeaponGuaranteedHit && comp.RollGuaranteedHit(sureHit))
                factor = 999f;
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

    /// <summary>射程：VerbProperties.AdjustedRange + 词条射程加值（长程词条；EffectiveRange 全链路经此取值）。</summary>
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedRange), typeof(Verb), typeof(Thing))]
    public static class Patch_WeaponRange
    {
        public static void Postfix(Verb ownerVerb, ref float __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(ownerVerb?.EquipmentSource);
            if (comp != null)
                __result += comp.RangeBonus;
        }
    }

    /// <summary>连发弹数：Verb.get_BurstShotCount + 词条加成（扩容词条；保底 1 发）。</summary>
    [HarmonyPatch(typeof(Verb), "get_BurstShotCount")]
    public static class Patch_WeaponBurstCount
    {
        public static void Postfix(Verb __instance, ref int __result)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(__instance?.EquipmentSource);
            if (comp == null)
                return;
            int bonus = Mathf.RoundToInt(comp.GetEffectSum(ModuleEffectKeys.WeaponBurstShots));
            if (bonus > 0)
                __result = Mathf.Max(1, __result + bonus);
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

    /// <summary>守护词条：持此武器者承伤减免（ildump 核实 1.7 PreApplyDamage(Verse.Pawn) 为虚方法，prefix 改伤害量）。</summary>
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class Patch_WeaponGuardian
    {
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            CompWeaponEnhancement comp = WeaponEnhancePatches.ActiveEnhancement(__instance?.equipment?.Primary);
            if (comp == null)
                return;
            float guard = comp.GetEffectSum(ModuleEffectKeys.WeaponGuardian);
            if (guard > 0f)
                dinfo.SetAmount(dinfo.Amount * (1f - Mathf.Min(0.5f, guard)));
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

    /// <summary>近战命中特效结算（含吸血/击退，近战专属词条在这里消费）。
    /// 1.7 基类 Verb_MeleeAttack.ApplyMeleeDamageToTarget 是抽象方法（无方法体，补丁会炸 PatchAll），
    /// 必须打具体实现子类 Verb_MeleeAttackDamage。</summary>
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
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

        /// <summary>致盲 hediff（惰性解析）。</summary>
        private static HediffDef blindDef;
        private static HediffDef BlindDef
        {
            get { return blindDef ?? (blindDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_Hediff_Blinded")); }
        }

        /// <summary>蚀甲 hediff（惰性解析）。</summary>
        private static HediffDef armorShredDef;
        private static HediffDef ArmorShredDef
        {
            get { return armorShredDef ?? (armorShredDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_Hediff_ArmorShred")); }
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

            // ---- 危险特效门控：武器 gizmo 开关 + 设置总开关（爆炸/引雷/毒气/燃烧） ----
            bool hazardOn = comp.TriggerEffectsEnabled && MMTSettings.WeaponHazardEffects;

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

            // ---- 暴击：掷词条暴击率，命中追加 1+暴虐值 次等额伤害（默认 ×2，暴虐词条 ×3） ----
            float critChance = comp.GetEffectSum(ModuleEffectKeys.WeaponCritChance);
            if (critChance > 0f && baseDamage > 0f && Rand.Chance(critChance))
            {
                int critExtra = 1 + (int)comp.GetEffectSum(ModuleEffectKeys.WeaponCritDamage);
                for (int i = 0; i < critExtra; i++)
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

            // ---- EMP 弹：对机械按词条倍率追加 EMP 伤害并短路眩晕 ----
            float empFactor = comp.GetEffectSum(ModuleEffectKeys.WeaponEmp);
            if (empFactor > 0f && victim != null
                && victim.RaceProps != null && victim.RaceProps.IsMechanoid && baseDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(DamageDefOf.EMP, baseDamage * empFactor, 0f, -1f, attacker));
                if (!victim.Dead && victim.stances?.stunner != null)
                    victim.stances.stunner.StunFor(120, attacker, false);
            }

            // ---- 燃烧弹（危险特效）：命中点燃 ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponIncendiary) > 0f && hazardOn)
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

            // ---- 爆裂（危险特效）：命中概率触发小爆炸（半径 1.9，伤害 ×0.35） ----
            float explode = comp.GetEffectSum(ModuleEffectKeys.WeaponExplode);
            if (explode > 0f && hazardOn && MMTSettings.EnableWeaponExplode && baseDamage > 0f
                && map != null && Rand.Chance(explode))
            {
                GenExplosion.DoExplosion(hitThing.Position, map, 1.9f, DamageDefOf.Bomb, attacker,
                    Mathf.RoundToInt(baseDamage * 0.35f), -1f);
            }

            // ---- 引雷（危险特效）：命中概率在命中点召唤落雷 ----
            float stormCall = comp.GetEffectSum(ModuleEffectKeys.WeaponStormCall);
            if (stormCall > 0f && hazardOn && MMTSettings.EnableWeaponStormCall && map != null && Rand.Chance(stormCall))
            {
                map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, hitThing.Position));
                MoteMaker.ThrowText(hitThing.DrawPos, map, "引雷", new Color(0.6f, 0.8f, 1f), 1f);
            }

            // ---- 毒气（危险特效）：命中概率落点放毒气（Biotech ToxGas；无 Biotech 回退毒性积累） ----
            float gas = comp.GetEffectSum(ModuleEffectKeys.WeaponGas);
            if (gas > 0f && hazardOn && MMTSettings.EnableWeaponGas && map != null && Rand.Chance(gas))
            {
                if (ModsConfig.BiotechActive)
                    GasUtility.AddGas(hitThing.Position, map, GasType.ToxGas, 2.9f);
                else if (victim != null && victim.RaceProps != null && victim.RaceProps.IsFlesh)
                    ApplyToxic(victim, 0.10f);
                MoteMaker.ThrowText(hitThing.DrawPos, map, "毒气", new Color(0.5f, 0.8f, 0.3f), 1f);
            }

            // ---- 压制：命中概率眩晕（通用控制，与 EMP 短路同款调用） ----
            float suppression = comp.GetEffectSum(ModuleEffectKeys.WeaponSuppression);
            if (suppression > 0f && MMTSettings.EnableWeaponSuppression && victim != null && !victim.Dead
                && victim.stances?.stunner != null && Rand.Chance(suppression))
            {
                victim.stances.stunner.StunFor(90, attacker, false);
                MoteMaker.ThrowText(victim.DrawPos, map, "压制", Color.gray, 1f);
            }

            // ---- 缴械：命中概率打掉目标主手武器（倒地/死亡跳过） ----
            float disarm = comp.GetEffectSum(ModuleEffectKeys.WeaponDisarm);
            if (disarm > 0f && MMTSettings.EnableWeaponDisarm && victim != null && !victim.Dead && !victim.Downed
                && victim.equipment?.Primary != null && Rand.Chance(disarm))
            {
                ThingWithComps dropped;
                if (victim.equipment.TryDropEquipment(victim.equipment.Primary, out dropped, victim.Position, false))
                    MoteMaker.ThrowText(victim.DrawPos, map, "缴械!", Color.white, 1.5f);
            }

            // ---- 剧毒：每次命中叠加毒性积累（仅血肉） ----
            float toxic = comp.GetEffectSum(ModuleEffectKeys.WeaponToxic);
            if (toxic > 0f && MMTSettings.EnableWeaponToxic && victim != null && !victim.Dead
                && victim.RaceProps != null && victim.RaceProps.IsFlesh)
            {
                ApplyToxic(victim, toxic);
            }

            // ---- 虫敌：对昆虫追加伤害（镜像 EMP 对机械） ----
            float insectBane = comp.GetEffectSum(ModuleEffectKeys.WeaponInsectBane);
            if (insectBane > 0f && MMTSettings.EnableWeaponInsectBane && victim != null && !victim.Dead
                && victim.RaceProps?.FleshType == FleshTypeDefOf.Insectoid && baseDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage * insectBane, 0f, -1f, attacker));
                MoteMaker.ThrowText(victim.DrawPos, map, "虫敌", new Color(0.8f, 0.5f, 0.2f), 1f);
            }

            // ---- 急救：命中同派系友军时按伤害 × 系数治疗其伤口（净治疗为正） ----
            float mending = comp.GetEffectSum(ModuleEffectKeys.WeaponMending);
            if (mending > 0f && MMTSettings.EnableWeaponMending && victim != null && !victim.Dead
                && attacker is Pawn medic && victim.Faction != null && victim.Faction == medic.Faction && baseDamage > 0f)
            {
                float healBudget = baseDamage * mending;
                List<Hediff> wounds = victim.health.hediffSet.hediffs;
                for (int i = 0; i < wounds.Count && healBudget > 0f; i++)
                {
                    if (wounds[i] is Hediff_Injury injury && injury.Severity > 0f)
                    {
                        float heal = Mathf.Min(healBudget, injury.Severity);
                        injury.Severity -= heal;
                        healBudget -= heal;
                    }
                }
                MoteMaker.ThrowText(victim.DrawPos, map, "急救", Color.green, 1f);
            }

            // ---- 时空折叠：击杀后攻击者本次冷却归零（取消冷却姿态） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponTimeFold) > 0f && victim != null && victim.Dead
                && attacker is Pawn shooter && shooter.stances != null
                && shooter.stances.curStance is Stance_Cooldown)
            {
                shooter.stances.SetStance(new Stance_Mobile());
                MoteMaker.ThrowText(shooter.DrawPos, map, "时空折叠", Color.white, 1f);
            }

            // ---- 致盲：命中附加致盲 hediff（视力 -30%，5 秒；重复命中刷新时长） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponBlinding) > 0f && victim != null && !victim.Dead && BlindDef != null)
            {
                Hediff oldBlind = victim.health.hediffSet.GetFirstHediffOfDef(BlindDef);
                if (oldBlind != null)
                    victim.health.RemoveHediff(oldBlind);
                victim.health.AddHediff(BlindDef);
            }

            // ---- 蚀甲：命中附加蚀甲 hediff（利刃/钝器护甲 -25%，5 秒；重复命中刷新时长） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponArmorShred) > 0f && victim != null && !victim.Dead && ArmorShredDef != null)
            {
                Hediff oldShred = victim.health.hediffSet.GetFirstHediffOfDef(ArmorShredDef);
                if (oldShred != null)
                    victim.health.RemoveHediff(oldShred);
                victim.health.AddHediff(ArmorShredDef);
            }

            // ---- 放血：命中血肉目标叠加流血积累（仅血肉） ----
            float bleed = comp.GetEffectSum(ModuleEffectKeys.WeaponBleed);
            if (bleed > 0f && victim != null && !victim.Dead
                && victim.RaceProps != null && victim.RaceProps.IsFlesh)
            {
                Hediff bloodLoss = victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (bloodLoss == null)
                {
                    bloodLoss = victim.health.AddHediff(HediffDefOf.BloodLoss);
                    bloodLoss.Severity = 0.03f * bleed;
                }
                else
                {
                    bloodLoss.Severity += 0.03f * bleed;
                }
            }

            // ---- 威慑：命中概率使目标恐慌逃跑（跳过倒地/死亡/机械） ----
            float terror = comp.GetEffectSum(ModuleEffectKeys.WeaponTerror);
            if (terror > 0f && MMTSettings.EnableWeaponTerror && victim != null && !victim.Dead && !victim.Downed
                && victim.RaceProps != null && !victim.RaceProps.IsMechanoid && victim.mindState != null
                && Rand.Chance(terror))
            {
                victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
                MoteMaker.ThrowText(victim.DrawPos, map, "威慑", new Color(0.8f, 0.3f, 0.8f), 1f);
            }

            // ---- 烟幕（危险特效）：命中概率在落点放烟雾 ----
            float smoke = comp.GetEffectSum(ModuleEffectKeys.WeaponSmoke);
            if (smoke > 0f && hazardOn && MMTSettings.EnableWeaponSmoke && map != null && Rand.Chance(smoke))
            {
                GasUtility.AddGas(hitThing.Position, map, GasType.BlindSmoke, 2.9f);
                MoteMaker.ThrowText(hitThing.DrawPos, map, "烟幕", Color.gray, 1f);
            }

            // ---- 屠夫：对血肉目标追加伤害（镜像虫敌/EMP） ----
            float fleshBane = comp.GetEffectSum(ModuleEffectKeys.WeaponFleshBane);
            if (fleshBane > 0f && MMTSettings.EnableWeaponFleshBane && victim != null && !victim.Dead
                && victim.RaceProps != null && victim.RaceProps.IsFlesh && baseDamage > 0f)
            {
                victim.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage * fleshBane, 0f, -1f, attacker));
                MoteMaker.ThrowText(victim.DrawPos, map, "屠夫", new Color(0.9f, 0.2f, 0.2f), 1f);
            }

            // ---- 破城：命中建筑/墙体追加伤害（目标非 Pawn 时生效） ----
            float demolition = comp.GetEffectSum(ModuleEffectKeys.WeaponDemolition);
            if (demolition > 0f && MMTSettings.EnableWeaponDemolition && victim == null && baseDamage > 0f
                && hitThing.def.category == ThingCategory.Building)
            {
                hitThing.TakeDamage(new DamageInfo(damageDef ?? DamageDefOf.Cut, baseDamage * demolition, 0f, -1f, attacker));
                MoteMaker.ThrowText(hitThing.DrawPos, map, "破城", new Color(0.8f, 0.6f, 0.2f), 1f);
            }

            // ---- 汲取（远程吸血）：远程命中按伤害 × 系数治疗攻击者 ----
            float rangedLeech = comp.GetEffectSum(ModuleEffectKeys.WeaponRangedLeech);
            if (!isMelee && rangedLeech > 0f && MMTSettings.EnableWeaponRangedLeech && baseDamage > 0f
                && attacker is Pawn leechPawn)
            {
                float leechBudget = baseDamage * rangedLeech;
                List<Hediff> leechInjuries = leechPawn.health.hediffSet.hediffs;
                for (int i = 0; i < leechInjuries.Count && leechBudget > 0f; i++)
                {
                    if (leechInjuries[i] is Hediff_Injury injury && injury.Severity > 0f)
                    {
                        float heal = Mathf.Min(leechBudget, injury.Severity);
                        injury.Severity -= heal;
                        leechBudget -= heal;
                    }
                }
            }

            // ---- 尸爆（危险特效）：击杀时尸体处触发小爆炸 ----
            float corpseBurst = comp.GetEffectSum(ModuleEffectKeys.WeaponCorpseBurst);
            if (corpseBurst > 0f && hazardOn && MMTSettings.EnableWeaponCorpseBurst && victim != null && victim.Dead
                && baseDamage > 0f && map != null)
            {
                GenExplosion.DoExplosion(victim.Position, map, 1.5f, DamageDefOf.Bomb, attacker,
                    Mathf.RoundToInt(baseDamage * 0.5f), -1f);
                MoteMaker.ThrowText(victim.DrawPos, map, "尸爆", new Color(0.7f, 0.3f, 0.3f), 1f);
            }

            // ---- 悟战：有效命中敌对目标给攻击者武器技能经验（段尾结算） ----
            if (comp.GetEffectSum(ModuleEffectKeys.WeaponSkillExp) > 0f && MMTSettings.EnableWeaponSkillExp
                && attacker is Pawn learner && learner.skills != null && victim != null
                && victim != attacker && victim.HostileTo(attacker))
            {
                learner.skills.Learn(isMelee ? SkillDefOf.Melee : SkillDefOf.Shooting, 15f, false);
            }
        }

        /// <summary>给血肉 pawn 叠加毒性积累（无则新建；毒气回退与剧毒词条共用）。</summary>
        private static void ApplyToxic(Pawn pawn, float amount)
        {
            Hediff toxicHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            if (toxicHediff == null)
            {
                toxicHediff = pawn.health.AddHediff(HediffDefOf.ToxicBuildup);
                toxicHediff.Severity = amount;
            }
            else
            {
                toxicHediff.Severity += amount;
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
