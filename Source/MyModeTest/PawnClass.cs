using LudeonTK;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    // 支柱二·职业就职与升阶（mmt-class-design.md §2.5/§5.1/§5.2）：
    // 导师台 MMT_ClassMentorBench 右键 FloatMenu（复用 ChargingStation 模式），按法师/斗士两系列出职业；
    // 就职消耗职业凭证×1 + 历练≥5 级，无灵能链接者自动觉醒 1 级（PawnUtility.ChangePsylinkLevel，spike 已核实）；
    // 升阶 II 需历练≥10 + 能量结晶×10，III 需历练≥15 + 能量结晶×20；每人限 1 职业，v1 不做转职。

    /// <summary>职业条目（defName 基底 MMT_Class_<名>，Hediff 三阶 _I/_II/_III）。</summary>
    public class MMTClassInfo
    {
        public string baseName;
        public string label;
        /// <summary>true=法师系，false=斗士系。</summary>
        public bool isMagic;

        public string HediffDefName(int tier)
        {
            return "MMT_Class_" + baseName + (tier == 1 ? "_I" : tier == 2 ? "_II" : "_III");
        }
    }

    public static class PawnClassUtil
    {
        /// <summary>37 个可选职业（混沌法师 deferred，不在列表；校验套件 rom_classes.py 同步核对）。</summary>
        public static readonly List<MMTClassInfo> Classes = new List<MMTClassInfo>
        {
            // 法师系（24）
            new MMTClassInfo { baseName = "FireMage", label = "火系魔法师", isMagic = true },
            new MMTClassInfo { baseName = "IceMage", label = "冰系魔法师", isMagic = true },
            new MMTClassInfo { baseName = "LightningMage", label = "雷系魔法师", isMagic = true },
            new MMTClassInfo { baseName = "Arcanist", label = "奥术法师", isMagic = true },
            new MMTClassInfo { baseName = "Druid", label = "德鲁伊", isMagic = true },
            new MMTClassInfo { baseName = "Paladin", label = "圣骑士", isMagic = true },
            new MMTClassInfo { baseName = "Summoner", label = "召唤师", isMagic = true },
            new MMTClassInfo { baseName = "Necromancer", label = "死灵法师", isMagic = true },
            new MMTClassInfo { baseName = "Priest", label = "牧师", isMagic = true },
            new MMTClassInfo { baseName = "Bard", label = "吟游诗人", isMagic = true },
            new MMTClassInfo { baseName = "DemonKin", label = "恶魔术士", isMagic = true },
            new MMTClassInfo { baseName = "Geomancer", label = "地占师", isMagic = true },
            new MMTClassInfo { baseName = "Technomancer", label = "技法师", isMagic = true },
            new MMTClassInfo { baseName = "BloodMage", label = "血法师", isMagic = true },
            new MMTClassInfo { baseName = "Enchanter", label = "附魔师", isMagic = true },
            new MMTClassInfo { baseName = "Chronomancer", label = "时空法师", isMagic = true },
            new MMTClassInfo { baseName = "Shadow", label = "暗影", isMagic = true },
            new MMTClassInfo { baseName = "Brightmage", label = "光法师", isMagic = true },
            new MMTClassInfo { baseName = "Shaman", label = "萨满", isMagic = true },
            new MMTClassInfo { baseName = "Golemancer", label = "傀儡师", isMagic = true },
            new MMTClassInfo { baseName = "Empath", label = "共情者", isMagic = true },
            new MMTClassInfo { baseName = "Possessor", label = "附身灵", isMagic = true },
            new MMTClassInfo { baseName = "Wanderer", label = "流浪法师", isMagic = true },
            new MMTClassInfo { baseName = "WeatherMage", label = "气象法师", isMagic = true },
            // 斗士系（13）
            new MMTClassInfo { baseName = "Gladiator", label = "角斗士", isMagic = false },
            new MMTClassInfo { baseName = "Ranger", label = "游侠", isMagic = false },
            new MMTClassInfo { baseName = "Sniper", label = "狙击手", isMagic = false },
            new MMTClassInfo { baseName = "Bladedancer", label = "刀锋舞者", isMagic = false },
            new MMTClassInfo { baseName = "Faceless", label = "无面者", isMagic = false },
            new MMTClassInfo { baseName = "Psionic", label = "灵能武者", isMagic = false },
            new MMTClassInfo { baseName = "DeathKnight", label = "死亡骑士", isMagic = false },
            new MMTClassInfo { baseName = "Monk", label = "武僧", isMagic = false },
            new MMTClassInfo { baseName = "Commander", label = "指挥官", isMagic = false },
            new MMTClassInfo { baseName = "SuperSoldier", label = "超级士兵", isMagic = false },
            new MMTClassInfo { baseName = "Apothecary", label = "药剂师", isMagic = false },
            new MMTClassInfo { baseName = "Wayfarer", label = "旅者", isMagic = false },
            new MMTClassInfo { baseName = "FieldMedic", label = "战地军医", isMagic = false },
        };

        /// <summary>升阶历练要求：I 阶就职≥5，II≥20，III≥35（v2 随 60 级上限重排）。</summary>
        public static int LevelRequirement(int tier)
        {
            return tier == 1 ? 5 : tier == 2 ? 20 : 35;
        }

        /// <summary>升阶能量结晶消耗（就职不耗结晶，耗凭证）。</summary>
        public static int CrystalCost(int tier)
        {
            return tier == 2 ? 10 : tier == 3 ? 20 : 0;
        }

        public static MMTClassInfo FindByBaseName(string baseName)
        {
            return Classes.FirstOrDefault(c => c.baseName == baseName);
        }

        /// <summary>小人身上的职业 Hediff（任意阶；未就职返回 null）。</summary>
        public static Hediff CurrentClassHediff(Pawn pawn)
        {
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (comp?.classDef == null)
            {
                return null;
            }
            MMTClassInfo info = FindByBaseName(BaseNameOf(comp.classDef));
            if (info == null)
            {
                return pawn.health.hediffSet.GetFirstHediffOfDef(comp.classDef);
            }
            for (int tier = 3; tier >= 1; tier--)
            {
                HediffDef def = DefDatabase<HediffDef>.GetNamedSilentFail(info.HediffDefName(tier));
                if (def != null)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
                    if (hediff != null)
                    {
                        return hediff;
                    }
                }
            }
            return null;
        }

        /// <summary>从职业 Hediff defName 反推职业基底名（MMT_Class_FireMage_II → FireMage）。</summary>
        public static string BaseNameOf(HediffDef def)
        {
            string n = def.defName;
            if (n.EndsWith("_III"))
            {
                return n.Substring("MMT_Class_".Length, n.Length - "MMT_Class_".Length - 4);
            }
            if (n.EndsWith("_II") || n.EndsWith("_I"))
            {
                return n.Substring("MMT_Class_".Length, n.Length - "MMT_Class_".Length - 2);
            }
            return n;
        }

        /// <summary>小人当前职业阶位（0=未就职）。</summary>
        public static int CurrentTier(Pawn pawn)
        {
            Hediff hediff = CurrentClassHediff(pawn);
            if (hediff == null)
            {
                return 0;
            }
            string n = hediff.def.defName;
            return n.EndsWith("_III") ? 3 : n.EndsWith("_II") ? 2 : 1;
        }

        /// <summary>背包内某物品数量。</summary>
        public static int CountInInventory(Pawn pawn, ThingDef def)
        {
            int count = 0;
            if (pawn.inventory != null)
            {
                foreach (Thing t in pawn.inventory.innerContainer)
                {
                    if (t.def == def)
                    {
                        count += t.stackCount;
                    }
                }
            }
            return count;
        }

        /// <summary>从背包消耗某物品（不足不扣）。</summary>
        public static bool ConsumeFromInventory(Pawn pawn, ThingDef def, int count)
        {
            if (CountInInventory(pawn, def) < count)
            {
                return false;
            }
            int left = count;
            foreach (Thing t in pawn.inventory.innerContainer.ToList())
            {
                if (t.def != def)
                {
                    continue;
                }
                int take = UnityEngine.Mathf.Min(left, t.stackCount);
                t.SplitOff(take).Destroy();
                left -= take;
                if (left <= 0)
                {
                    return true;
                }
            }
            return true;
        }
    }

    /// <summary>职业导师台 comp：右键 FloatMenu 就职/升阶。</summary>
    public class CompProperties_ClassMentor : CompProperties
    {
        public CompProperties_ClassMentor()
        {
            compClass = typeof(CompClassMentor);
        }
    }

    public class CompClassMentor : ThingComp
    {
        /// <summary>待执行的就职/升阶请求（pawn → 职业基底名:目标阶），作业到达后消费；不存档（中断即放弃）。</summary>
        private readonly Dictionary<Pawn, string> pending = new Dictionary<Pawn, string>();

        private Building Bench => parent as Building;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!MMTSettings.PawnProgressEnabled)
            {
                yield break;
            }
            if (selPawn?.RaceProps == null || !selPawn.RaceProps.Humanlike || selPawn.Faction == null || !selPawn.Faction.IsPlayer)
            {
                yield break;
            }
            CompPowerTrader power = parent.GetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn)
            {
                yield return new FloatMenuOption("职业导师台（未通电）", null);
                yield break;
            }
            CompPawnProgress comp = CompPawnProgress.Get(selPawn);
            if (comp == null)
            {
                yield break;
            }
            int tier = PawnClassUtil.CurrentTier(selPawn);
            if (tier == 0)
            {
                // 就职分支：法师系/斗士系两组
                if (comp.level < PawnClassUtil.LevelRequirement(1))
                {
                    yield return new FloatMenuOption("就职（需要历练 " + PawnClassUtil.LevelRequirement(1) + " 级，当前 " + comp.level + " 级）", null);
                    yield break;
                }
                int tokens = PawnClassUtil.CountInInventory(selPawn, MMT_ClassMentorDefOf.MMT_ClassToken);
                if (tokens < 1)
                {
                    yield return new FloatMenuOption("就职（需要随身携带 职业凭证×1）", null);
                    yield break;
                }
                yield return new FloatMenuOption("就职·法师系…", delegate { OpenClassMenu(selPawn, true); });
                yield return new FloatMenuOption("就职·斗士系…", delegate { OpenClassMenu(selPawn, false); });
            }
            else if (tier < 3)
            {
                int next = tier + 1;
                Hediff current = PawnClassUtil.CurrentClassHediff(selPawn);
                string classLabel = current?.def.LabelCap.ToString() ?? "职业";
                if (comp.level < PawnClassUtil.LevelRequirement(next))
                {
                    yield return new FloatMenuOption(classLabel + " 升阶（需要历练 " + PawnClassUtil.LevelRequirement(next) + " 级，当前 " + comp.level + " 级）", null);
                }
                else
                {
                    int crystals = PawnClassUtil.CountInInventory(selPawn, MMT_ClassMentorDefOf.MMT_EnergyCrystal);
                    int need = PawnClassUtil.CrystalCost(next);
                    if (crystals < need)
                    {
                        yield return new FloatMenuOption(classLabel + " 升阶（需要随身携带 能量结晶×" + need + "）", null);
                    }
                    else
                    {
                        string baseName = PawnClassUtil.BaseNameOf(current.def);
                        yield return new FloatMenuOption(classLabel + " → " + (next == 2 ? "II" : "III") + " 阶（消耗 能量结晶×" + need + "）",
                            delegate { OrderAttune(selPawn, baseName, next); });
                    }
                }
            }
            else
            {
                yield return new FloatMenuOption("已达最高阶（" + (PawnClassUtil.CurrentClassHediff(selPawn)?.def.LabelCap.ToString() ?? "") + "）", null);
            }
        }

        private void OpenClassMenu(Pawn selPawn, bool magic)
        {
            CompPawnProgress comp = CompPawnProgress.Get(selPawn);
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (MMTClassInfo info in PawnClassUtil.Classes.Where(c => c.isMagic == magic))
            {
                // v2 天赋树：仅列出已解锁家族的职业（天赋树 Def 缺失时 family 为 null，放行兼容）
                string family = TalentTreeUtil.FamilyOfClass(info.baseName);
                if (family != null && !TalentTreeUtil.FamilyUnlocked(comp, family))
                {
                    MMT_TalentTreeDef familyTree = TalentTreeUtil.FamilyTreeOf(family);
                    options.Add(new FloatMenuOption(info.label + "（未解锁："
                        + (familyTree?.LabelCap.ToString() ?? family) + "）", null));
                    continue;
                }
                options.Add(new FloatMenuOption(info.label, delegate { OrderAttune(selPawn, info.baseName, 1); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>派就职/升阶作业：targetA=导师台；请求登记在 pending，作业到达后消费材料并执行。</summary>
        private void OrderAttune(Pawn selPawn, string baseName, int tier)
        {
            if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(selPawn.LabelShort + " 无法到达职业导师台（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            pending[selPawn] = baseName + ":" + tier;
            Job job = JobMaker.MakeJob(MMT_ClassMentorDefOf.MMT_AttuneClass, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            if (!selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                pending.Remove(selPawn);
                Messages.Message(selPawn.LabelShort + " 当前无法接受作业（征召/倒地/其他强制作业中）", parent, MessageTypeDefOf.RejectInput, false);
            }
        }

        /// <summary>作业到达：校验并消耗材料，执行就职/升阶。</summary>
        public void CompleteAttune(Pawn pawn)
        {
            if (!pending.TryGetValue(pawn, out string request))
            {
                return;
            }
            pending.Remove(pawn);
            string baseName = request.Substring(0, request.IndexOf(':'));
            int tier = int.Parse(request.Substring(request.IndexOf(':') + 1));
            MMTClassInfo info = PawnClassUtil.FindByBaseName(baseName);
            CompPawnProgress comp = CompPawnProgress.Get(pawn);
            if (info == null || comp == null)
            {
                return;
            }
            HediffDef newDef = DefDatabase<HediffDef>.GetNamedSilentFail(info.HediffDefName(tier));
            if (newDef == null)
            {
                Log.Warning("[MMT] 缺少职业 Hediff: " + info.HediffDefName(tier) + "（生成器未运行？）");
                return;
            }
            if (comp.level < PawnClassUtil.LevelRequirement(tier))
            {
                Messages.Message("历练等级不足，就职/升阶取消", pawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            // 材料校验与消耗
            if (tier == 1)
            {
                if (PawnClassUtil.CurrentTier(pawn) != 0)
                {
                    return;
                }
                if (!PawnClassUtil.ConsumeFromInventory(pawn, MMT_ClassMentorDefOf.MMT_ClassToken, 1))
                {
                    Messages.Message("缺少 职业凭证×1，就职取消", pawn, MessageTypeDefOf.RejectInput, false);
                    return;
                }
            }
            else
            {
                if (PawnClassUtil.CurrentTier(pawn) != tier - 1)
                {
                    return;
                }
                if (!PawnClassUtil.ConsumeFromInventory(pawn, MMT_ClassMentorDefOf.MMT_EnergyCrystal, PawnClassUtil.CrystalCost(tier)))
                {
                    Messages.Message("缺少 能量结晶×" + PawnClassUtil.CrystalCost(tier) + "，升阶取消", pawn, MessageTypeDefOf.RejectInput, false);
                    return;
                }
            }
            // 摘除旧阶 Hediff（技能 gizmo 随 Hediff 自动增减，零绑定代码）
            Hediff old = PawnClassUtil.CurrentClassHediff(pawn);
            if (old != null)
            {
                pawn.health.RemoveHediff(old);
            }
            pawn.health.AddHediff(newDef);
            comp.classDef = DefDatabase<HediffDef>.GetNamedSilentFail(info.HediffDefName(1)) ?? newDef;
            // v2 天赋树：就职联动，免费解锁本家族树 Start 节点
            TalentTreeUtil.OnClassAttuned(pawn, baseName);
            // 职业启蒙：无灵能链接者自动觉醒 1 级（mmt-class-design.md §5.2，spike 已核实 API）
            if (tier == 1 && !pawn.HasPsylink)
            {
                PawnUtility.ChangePsylinkLevel(pawn, 1, false);
                Messages.Message(pawn.LabelShort + " 在职业启蒙中觉醒了灵能链接！", pawn, MessageTypeDefOf.PositiveEvent);
            }
            Messages.Message(pawn.LabelShort + (tier == 1 ? " 就职为「" : " 晋升为「") + newDef.LabelCap + "」",
                pawn, MessageTypeDefOf.PositiveEvent);
        }
    }

    /// <summary>就职/升阶作业：走到导师台 → 短暂共鸣 → 执行。</summary>
    public class JobDriver_MMTClassAttune : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            Toil wait = Toils_General.Wait(180, TargetIndex.A);
            wait.WithProgressBarToilDelay(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return wait;
            Toil finish = new Toil();
            finish.initAction = delegate
            {
                Building bench = job.targetA.Thing as Building;
                bench?.GetComp<CompClassMentor>()?.CompleteAttune(pawn);
            };
            finish.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finish;
        }
    }

    /// <summary>Dev 模式发放职业凭证（mmt-class-design.md §1.2 决策 3；据点掉落实装后改掉落）。</summary>
    public static class MMTClassDebugActions
    {
        [DebugAction("MMT 职业", "发放职业凭证×1（选中者）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveClassToken(Pawn pawn)
        {
            if (pawn?.inventory == null)
            {
                return;
            }
            Thing token = ThingMaker.MakeThing(MMT_ClassMentorDefOf.MMT_ClassToken);
            if (!pawn.inventory.innerContainer.TryAdd(token))
            {
                GenPlace.TryPlaceThing(token, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        }
    }

    [DefOf]
    public static class MMT_ClassMentorDefOf
    {
        /// <summary>职业凭证。</summary>
        public static ThingDef MMT_ClassToken;
        /// <summary>能量结晶（升阶材料）。</summary>
        public static ThingDef MMT_EnergyCrystal;
        /// <summary>就职/升阶作业。</summary>
        public static JobDef MMT_AttuneClass;

        static MMT_ClassMentorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ClassMentorDefOf));
        }
    }
}
