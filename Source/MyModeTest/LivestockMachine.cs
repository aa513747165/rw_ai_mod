using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 畜牧机器建筑：三层绘制——舱内层（底，建筑主 graphic）→ 舱内动物 → 外壳玻璃层（顶）。
    /// 动物为静态贴图（缓存其生命阶段身体 Graphic 手动 DrawMesh），超过舱体容量的动物按 bodySize 等比缩小；
    /// 朝向跟随建筑旋转：南→南、北→北、东→东（头朝舱门/屏幕右）、西→西。
    /// 收容与生产逻辑在 CompLivestockMachine，装卸在两个 JobDriver。
    /// </summary>
    public class Building_LivestockMachine : Building
    {
        /// <summary>外壳玻璃层贴图（Graphic_Multi 三朝向，west 自动镜像 east）。</summary>
        private const string ShellTexPath = "Things/Building/Production/MMT_LivestockMachine/MMT_LivestockMachine_Shell";

        // comps 列表生成后终身不变，spawn 时缓存引用
        private CompLivestockMachine livestockComp;
        // 外壳玻璃层材质惰性缓存（1024 大图只建一次）
        private Graphic shellGraphic;
        // 舱内动物与其身体 Graphic 的缓存（持有物变动极少，按引用比对即可）
        private Pawn cachedAnimal;
        private Graphic cachedBodyGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            livestockComp = this.TryGetComp<CompLivestockMachine>();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Pawn animal = livestockComp?.ContainedAnimal;
            if (animal != null)
                DrawAnimal(drawLoc, animal);
            DrawShell(drawLoc);
        }

        /// <summary>绘制舱内动物：按 bodySize 缩放到舱内容许尺寸（maxBodySizeForFullScale 内不缩放），脚踩舱底略偏下。</summary>
        private void DrawAnimal(Vector3 drawLoc, Pawn animal)
        {
            if (animal != cachedAnimal)
            {
                cachedAnimal = animal;
                PawnKindLifeStage stage = animal.ageTracker.CurKindLifeStage;
                GraphicData gd = stage == null ? null
                    : (animal.gender == Gender.Female && stage.femaleGraphicData != null ? stage.femaleGraphicData : stage.bodyGraphicData);
                cachedBodyGraphic = gd?.Graphic;
            }
            Graphic g = cachedBodyGraphic;
            if (g == null)
                return;
            float bodySize = animal.RaceProps.baseBodySize;
            float maxBody = livestockComp.Props.maxBodySizeForFullScale;
            float s = Mathf.Min(1f, maxBody / Mathf.Max(0.01f, bodySize));
            // 绝对上限：再大也不许超出舱宽（防 bodySize 小但 drawSize 大的贴图穿模）
            s = Mathf.Min(s, 1.5f / Mathf.Max(0.01f, g.drawSize.x));
            Material mat = g.MatAt(FacingFor(Rotation), null);
            if (mat == null)
                return;
            Vector3 pos = drawLoc;
            pos.y = drawLoc.y + 0.02f; // 垫高到舱内层之上、玻璃层之下
            pos.z -= 0.3f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(g.drawSize.x * s, 1f, g.drawSize.y * s)), mat, 0);
        }

        /// <summary>绘制外壳玻璃层（顶层）。手动 DrawMesh 而非 Graphic.Draw：手动加载的 Graphic 无 GraphicData，
        /// DrawWorker 会按 rot.AsQuat 旋转网格，对预烘焙的三朝向贴图是错的；这里固定 identity，由 MatAt 选朝向材质。</summary>
        private void DrawShell(Vector3 drawLoc)
        {
            if (shellGraphic == null)
                shellGraphic = GraphicDatabase.Get<Graphic_Multi>(ShellTexPath, ShaderDatabase.Transparent,
                    def.graphicData.drawSize, Color.white, Color.white);
            Material mat = shellGraphic.MatAt(Rotation, this);
            if (mat == null)
                return;
            Vector3 pos = drawLoc;
            pos.y = drawLoc.y + 0.04f;
            Vector2 size = def.graphicData.drawSize;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size.x, 1f, size.y)), mat, 0);
        }

        /// <summary>动物朝向：与建筑一致（east 建筑 → 动物朝 east，头朝舱门方向）。</summary>
        private static Rot4 FacingFor(Rot4 rot)
        {
            if (rot == Rot4.North)
                return Rot4.North;
            if (rot == Rot4.West)
                return Rot4.West;
            if (rot == Rot4.East)
                return Rot4.East;
            return Rot4.South;
        }
    }

    /// <summary>
    /// 畜牧机器参数（XML 可配置）：舱内容量、饲料换算、无产物动物的产肉循环。
    /// </summary>
    public class CompProperties_LivestockMachine : CompProperties
    {
        /// <summary>舱内全尺寸显示的最大 bodySize：超过该值的动物按比例缩小（2×2 舱默认 2.0，大象等会被缩小）。</summary>
        public float maxBodySizeForFullScale = 2f;

        /// <summary>饲料消耗系数：每日耗料 = 动物原本每日食量 × 本系数（默认 1.0 = 与散养一致）。</summary>
        public float feedConsumptionFactor = 1f;

        /// <summary>单份饲料（1 燃料单位）的营养值：干草/粗饲料/生素食均为 0.05。</summary>
        public float feedNutritionPerUnit = 0.05f;

        /// <summary>无产物动物的产肉间隔（天）。</summary>
        public float meatCycleDays = 1f;

        /// <summary>每轮产肉量 = bodySize × 本系数。</summary>
        public float meatPerBodySize = 5f;

        /// <summary>小人引导动物入舱的工时 tick。</summary>
        public int leadTicks = 600;

        public CompProperties_LivestockMachine()
        {
            compClass = typeof(CompLivestockMachine);
        }
    }

    /// <summary>
    /// 畜牧机器的一条产物源（scribe 整条；动物不变时无需重建）：
    /// 产物 def + 基础数量（蛋为范围）+ 基础间隔（天）+ 当前进度 tick。
    /// 无产物动物用产肉源（isMeatCycle 仅用于检查面板文案）。
    /// </summary>
    public class LivestockProductSource : IExposable
    {
        public ThingDef productDef;
        public IntRange amountRange = new IntRange(1, 1);
        public float intervalDays = 1f;
        public int progressTicks;
        public bool isMeatCycle;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref productDef, "productDef");
            Scribe_Values.Look(ref amountRange, "amountRange");
            Scribe_Values.Look(ref intervalDays, "intervalDays");
            Scribe_Values.Look(ref progressTicks, "progressTicks");
            Scribe_Values.Look(ref isMeatCycle, "isMeatCycle");
        }
    }

    /// <summary>
    /// 畜牧机器核心 Comp：内置容器持有一只本派系动物，通电且开机时按动物自身产物 comps
    /// （挤奶/剪毛/产蛋）的原始间隔与数量周期产出；没有任何产物源的动物走产肉循环。
    /// 饲料走燃料仓：每日消耗 = 动物原本每日食量（FoodFallPerTickAssumingCategory × 60000，
    /// 天然含种族 HungerRate/体型/生命阶段）换算成燃料份数；缺饲料时停产。
    /// 超频模块缩短间隔（/(1+0.1n)），增值模块每次产出按基础量 ×(1+n)。
    /// 动物在舱内不 tick（悬空保存，年龄/需求冻结）。
    /// </summary>
    public class CompLivestockMachine : ThingComp, IThingHolder
    {
        /// <summary>内置容器：持有舱内动物（容量 1，持有期间动物不进世界，由建筑 DrawAt 绘制）。</summary>
        private ThingOwner<Thing> container;

        /// <summary>产物源列表（含产肉兜底源），放入动物时建立。</summary>
        private List<LivestockProductSource> sources = new List<LivestockProductSource>();

        // comps 列表生成后终身不变，spawn 时缓存引用，避免每 tick 线性扫描
        private CompFlickable flickComp;
        private CompPowerTrader powerComp;
        private CompRefuelable fuelComp;
        private CompModuleSlots slotsComp;

        public CompLivestockMachine()
        {
            container = new ThingOwner<Thing>(this, LookMode.Deep, true);
        }

        // ---- IThingHolder：容器以本 comp 为持有者（1.7 ThingComp 不再自带 IThingHolder） ----

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return container;
        }

        public CompProperties_LivestockMachine Props
        {
            get { return (CompProperties_LivestockMachine)props; }
        }

        /// <summary>舱内动物；空舱为 null。</summary>
        public Pawn ContainedAnimal
        {
            get { return container.Count > 0 ? container[0] as Pawn : null; }
        }

        /// <summary>设备是否可用：开启开关且通电（断电时生产与喂食暂停）。</summary>
        private bool CanRun()
        {
            if (flickComp != null && !flickComp.SwitchIsOn)
                return false;
            return powerComp == null || powerComp.PowerOn;
        }

        /// <summary>饲料是否充足（燃料仓有料）。</summary>
        private bool HasFeed
        {
            get { return fuelComp == null || fuelComp.Fuel > 0f; }
        }

        /// <summary>超频模块合计加成（每件 +0.1）。</summary>
        private float SpeedBonus
        {
            get { return slotsComp != null ? slotsComp.GetEffectSum(ModuleEffectKeys.SpeedFactor) : 0f; }
        }

        /// <summary>增值模块件数（每件产出 +1 份基础量）。</summary>
        private int YieldCount
        {
            get { return slotsComp != null ? (int)slotsComp.GetEffectSum(ModuleEffectKeys.YieldBonus) : 0; }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
            fuelComp = parent.GetComp<CompRefuelable>();
            slotsComp = parent.GetComp<CompModuleSlots>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref container, "container", this);
            Scribe_Collections.Look(ref sources, "sources", LookMode.Deep);
            // 旧存档兜底：有动物但源列表缺失时重建
            if (Scribe.mode == LoadSaveMode.PostLoadInit && ContainedAnimal != null && (sources == null || sources.Count == 0))
                sources = BuildSources(ContainedAnimal);
        }

        /// <summary>把动物装入舱内（调用方需先 DeSpawn）；舱内已有动物时拒绝。</summary>
        public bool TryAcceptAnimal(Pawn animal)
        {
            if (animal == null || ContainedAnimal != null)
                return false;
            if (!container.TryAdd(animal))
                return false;
            sources = BuildSources(animal);
            return true;
        }

        /// <summary>放出舱内动物到建筑相邻空格；空舱返回 null。</summary>
        public Pawn ReleaseAnimal()
        {
            Pawn animal = ContainedAnimal;
            if (animal == null)
                return null;
            container.Remove(animal);
            sources = new List<LivestockProductSource>();
            IntVec3 cell = FindReleaseCell();
            GenSpawn.Spawn(animal, cell, parent.Map);
            return animal;
        }

        /// <summary>找建筑相邻第一个可站立空格（2×2 建筑中心不可站，不能直接用交互格）。</summary>
        private IntVec3 FindReleaseCell()
        {
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8WayAndInside(parent))
            {
                if (!cell.InBounds(parent.Map) || !cell.Standable(parent.Map))
                    continue;
                if (cell.GetFirstPawn(parent.Map) == null)
                    return cell;
            }
            return parent.Position;
        }

        /// <summary>拆除/摧毁时把舱内动物放回地面，避免丢失。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            container.TryDropAll(parent.PositionHeld, map, ThingPlaceMode.Near);
            sources = new List<LivestockProductSource>();
            base.PostDeSpawn(map, mode);
        }

        // mod 自定义采集 comp 的基类属性读取（ResourceDef/ResourceAmount/GatherResourcesIntervalDays/Active
        // 在 1.7 为 protected 抽象属性，已反射核实）；缓存 PropertyInfo，仅入舱时调用一次
        private static readonly PropertyInfo GatherResourceDefProp =
            typeof(CompHasGatherableBodyResource).GetProperty("ResourceDef", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo GatherResourceAmountProp =
            typeof(CompHasGatherableBodyResource).GetProperty("ResourceAmount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo GatherIntervalProp =
            typeof(CompHasGatherableBodyResource).GetProperty("GatherResourcesIntervalDays", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo GatherActiveProp =
            typeof(CompHasGatherableBodyResource).GetProperty("Active", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// 扫描动物 comps 建立产物源列表：挤奶（milkFemaleOnly 校验性别）/剪毛/产蛋（eggLayFemaleOnly 校验性别，
        /// 机器产出未受精蛋）走公开 Props；mod 自定义采集 comp（继承 CompHasGatherableBodyResource 基类，
        /// 如 VE/Alpha Animals 的自定义资源）经反射读基类属性自动识别。
        /// 一个产物源都没有则建立产肉循环源（raceProps.meatDef，可能为 null 则不产出）。
        /// </summary>
        private List<LivestockProductSource> BuildSources(Pawn animal)
        {
            List<LivestockProductSource> list = new List<LivestockProductSource>();
            foreach (ThingComp comp in animal.AllComps)
            {
                if (comp is CompMilkable milkable)
                {
                    CompProperties_Milkable p = milkable.Props;
                    if (p.milkDef != null && p.milkAmount > 0 && p.milkIntervalDays > 0
                        && (!p.milkFemaleOnly || animal.gender == Gender.Female))
                    {
                        list.Add(new LivestockProductSource
                        {
                            productDef = p.milkDef,
                            amountRange = new IntRange(p.milkAmount, p.milkAmount),
                            intervalDays = p.milkIntervalDays,
                        });
                    }
                }
                else if (comp is CompShearable shearable)
                {
                    CompProperties_Shearable p = shearable.Props;
                    if (p.woolDef != null && p.woolAmount > 0 && p.shearIntervalDays > 0)
                    {
                        list.Add(new LivestockProductSource
                        {
                            productDef = p.woolDef,
                            amountRange = new IntRange(p.woolAmount, p.woolAmount),
                            intervalDays = p.shearIntervalDays,
                        });
                    }
                }
                else if (comp is CompHasGatherableBodyResource gatherable)
                {
                    // 流程节点：mod 自定义采集 comp（非奶/毛/蛋），反射读基类 protected 属性
                    if (GatherResourceDefProp == null || GatherActiveProp == null)
                        continue;
                    // Active 校验生命阶段（幼崽当前不可采集）；舱内动物不成长，放入幼崽落入产肉循环
                    if (GatherActiveProp.GetValue(gatherable) is bool active && !active)
                        continue;
                    ThingDef res = GatherResourceDefProp.GetValue(gatherable) as ThingDef;
                    int amount = GatherResourceAmountProp?.GetValue(gatherable) as int? ?? 0;
                    int interval = GatherIntervalProp?.GetValue(gatherable) as int? ?? 0;
                    if (res == null || amount <= 0 || interval <= 0)
                        continue;
                    list.Add(new LivestockProductSource
                    {
                        productDef = res,
                        amountRange = new IntRange(amount, amount),
                        intervalDays = interval,
                    });
                }
                else if (comp is CompEggLayer eggLayer)
                {
                    CompProperties_EggLayer p = eggLayer.Props;
                    ThingDef egg = p.eggUnfertilizedDef ?? p.eggFertilizedDef;
                    if (egg != null && p.eggLayIntervalDays > 0f
                        && (!p.eggLayFemaleOnly || animal.gender == Gender.Female))
                    {
                        list.Add(new LivestockProductSource
                        {
                            productDef = egg,
                            amountRange = p.eggCountRange,
                            intervalDays = p.eggLayIntervalDays,
                        });
                    }
                }
            }
            if (list.Count == 0)
            {
                // 流程节点：无产物动物 -> 产肉循环
                ThingDef meat = animal.RaceProps.meatDef;
                if (meat != null)
                {
                    int amount = Mathf.Max(1, Mathf.RoundToInt(animal.RaceProps.baseBodySize * Props.meatPerBodySize));
                    list.Add(new LivestockProductSource
                    {
                        productDef = meat,
                        amountRange = new IntRange(amount, amount),
                        intervalDays = Props.meatCycleDays,
                        isMeatCycle = true,
                    });
                }
            }
            return list;
        }

        /// <summary>生产推进（delta = 距上次间隔回调经过的 tick 数）：先喂料，再推进各产物源计时，到点产出。
        /// 注意：1.7 的 DoTick 只对 tickerType==Rare 的物体调 TickRare，Normal 建筑永远收不到
        /// CompTickRare（已反汇编 1.7 Thing.DoTick 核实），周期逻辑必须放 CompTickInterval。</summary>
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
            Pawn animal = ContainedAnimal;
            if (animal == null || sources.Count == 0 || !CanRun() || !HasFeed)
                return;
            // 喂料：按动物原本的食量消耗（饥饿下降速率 × 经过 tick × 系数 ÷ 单份饲料营养）
            Need_Food food = animal.needs?.food;
            if (food != null && fuelComp != null)
            {
                float nutrition = food.FoodFallPerTickAssumingCategory(HungerCategory.Fed) * delta * Props.feedConsumptionFactor;
                if (nutrition > 0f)
                    fuelComp.ConsumeFuel(nutrition / Props.feedNutritionPerUnit);
            }
            float speedFactor = 1f + SpeedBonus;
            for (int i = 0; i < sources.Count; i++)
            {
                LivestockProductSource source = sources[i];
                source.progressTicks += delta;
                int intervalTicks = Mathf.Max(60, Mathf.RoundToInt(source.intervalDays * 60000f / speedFactor));
                if (source.progressTicks < intervalTicks)
                    continue;
                // 流程结束：本轮产出（超出的进度结转到下一轮，长间隔源不亏进度）
                source.progressTicks -= intervalTicks;
                Produce(source);
            }
        }

        /// <summary>产出一份产物到交互格附近：数量 = 基础量 × (1 + 增值模块数)。</summary>
        private void Produce(LivestockProductSource source)
        {
            Thing product = ThingMaker.MakeThing(source.productDef);
            product.stackCount = source.amountRange.RandomInRange * (1 + YieldCount);
            GenPlace.TryPlaceThing(product, parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
        }

        // ---- 右键菜单：放入/放出动物 ----

        /// <summary>右键菜单（仿 CompChargingStation 模式）：空舱给"放入动物"（列出本派系动物），有动物给"放出动物"。</summary>
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed
                || !selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                yield break;
            if (selPawn.Map != parent.Map)
                yield break;
            if (parent.IsForbidden(selPawn))
            {
                yield return new FloatMenuOption("畜牧机器（设备被禁止或在小人活动区域外）", null);
                yield break;
            }
            Pawn held = ContainedAnimal;
            if (held == null)
            {
                yield return new FloatMenuOption("放入动物", delegate { StartLoadSelection(selPawn); });
            }
            else
            {
                yield return new FloatMenuOption("放出动物：" + held.LabelShort, delegate { OrderUnload(selPawn); });
            }
        }

        /// <summary>放入入口：列出地图上可引导的本派系动物；没有则提示，多只则弹选择框。</summary>
        private void StartLoadSelection(Pawn selPawn)
        {
            List<Pawn> animals = FindLoadableAnimals(selPawn);
            if (animals.Count == 0)
            {
                Messages.Message("附近没有可放入的动物（需要本派系、未倒地、可到达且未被预定的动物）",
                    selPawn, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (animals.Count == 1)
            {
                OrderLoad(selPawn, animals[0]);
                return;
            }
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Pawn animal in animals)
            {
                Pawn local = animal;
                options.Add(new FloatMenuOption(animal.LabelCap + "（" + animal.KindLabel + "）",
                    delegate { OrderLoad(selPawn, local); }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        /// <summary>收集地图上可放入的动物：本派系、动物、未倒地、未禁止、可到达且可预定。</summary>
        private List<Pawn> FindLoadableAnimals(Pawn selPawn)
        {
            List<Pawn> list = new List<Pawn>();
            foreach (Pawn p in parent.Map.mapPawns.AllPawnsSpawned)
            {
                if (p.RaceProps == null || !p.RaceProps.Animal)
                    continue;
                // 流程节点：只能放入自己的动物
                if (p.Faction != Faction.OfPlayer)
                    continue;
                if (p.Downed || p.Dead || p.IsForbidden(selPawn))
                    continue;
                if (!selPawn.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly))
                    continue;
                if (!selPawn.CanReserve(p, 1, -1, null, false))
                    continue;
                list.Add(p);
            }
            list.SortBy(p => p.Position.DistanceToSquared(selPawn.Position));
            return list;
        }

        /// <summary>派放入作业：targetA=动物，targetB=畜牧机器。</summary>
        private void OrderLoad(Pawn pawn, Pawn animal)
        {
            Job job = JobMaker.MakeJob(MMT_LivestockDefOf.MMT_LoadLivestockMachine, animal, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        /// <summary>派放出作业：targetA=畜牧机器。</summary>
        private void OrderUnload(Pawn pawn)
        {
            if (ContainedAnimal == null)
            {
                Messages.Message("畜牧机器内没有动物", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                Messages.Message(pawn.LabelShort + " 无法到达畜牧机器（路径不通）", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            Job job = JobMaker.MakeJob(MMT_LivestockDefOf.MMT_UnloadLivestockMachine, parent);
            job.count = 1; // 1.7 自定义作业必须显式 count
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override string CompInspectStringExtra()
        {
            Pawn animal = ContainedAnimal;
            if (animal == null)
                return "状态: 空舱（右键放入本派系动物）";
            string text = "动物: " + animal.LabelShort + "（" + animal.KindLabel + "）";
            if (fuelComp != null)
                text += "\n饲料: " + fuelComp.Fuel.ToString("F0") + " / " + fuelComp.Props.fuelCapacity.ToString("F0");
            if (!CanRun())
                return text + "\n生产暂停：需要开启开关并通电";
            if (!HasFeed)
                return text + "\n生产暂停：缺饲料（向燃料仓补充干草/粗饲料等）";
            float speedFactor = 1f + SpeedBonus;
            foreach (LivestockProductSource source in sources)
            {
                int intervalTicks = Mathf.Max(60, Mathf.RoundToInt(source.intervalDays * 60000f / speedFactor));
                int remaining = intervalTicks - source.progressTicks;
                text += "\n" + source.productDef.label + ": "
                    + ((float)source.progressTicks / intervalTicks).ToStringPercent()
                    + "（剩余 " + remaining.ToStringTicksToPeriod() + "）";
            }
            if (sources.Count == 0)
                text += "\n该动物没有任何可产出资源";
            return text;
        }
    }

    /// <summary>
    /// 放入动物作业（targetA=动物，targetB=畜牧机器）：
    /// 走到动物旁 → 短暂引导（进度条）→ 动物入舱（DeSpawn 后存入容器）。
    /// </summary>
    public class JobDriver_LoadLivestockMachine : JobDriver
    {
        private CompLivestockMachine Machine
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.B).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompLivestockMachine>() : null;
            }
        }

        private Pawn Animal
        {
            get { return job.GetTarget(TargetIndex.A).Thing as Pawn; }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 只预定动物不预定设备（带燃料仓的设备常被加油作业长期预定；舱位冲突由最终 toil 复检兜底）
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            // 舱内已有动物（别人抢先放入）则中止
            this.FailOn(() => Machine == null || Machine.ContainedAnimal != null);
            // 动物死亡/倒地/被禁则中止
            this.FailOn(() => Animal == null || Animal.Dead || Animal.Downed || Animal.IsForbidden(pawn));
            // 走到动物旁（动物会游走，ClosestTouch 会持续重寻路）
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            // 引导：短工时 + 进度条；期间动物跑出触及范围则中止
            int leadTicks = Machine?.Props.leadTicks ?? 600;
            Toil lead = Toils_General.Wait(leadTicks, TargetIndex.B);
            lead.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            lead.FailOn(() => Machine == null || Machine.ContainedAnimal != null);
            lead.FailOn(() => Animal == null || Animal.Dead || Animal.Downed
                || !Animal.Position.InHorDistOf(pawn.Position, 3f));
            lead.WithProgressBarToilDelay(TargetIndex.B);
            yield return lead;
            // 流程结束：动物入舱
            yield return Toils_General.DoAtomic(delegate
            {
                CompLivestockMachine machine = Machine;
                Pawn animal = Animal;
                if (machine == null || animal == null || animal.Dead || animal.Downed || machine.ContainedAnimal != null)
                    return;
                animal.DeSpawn();
                if (!machine.TryAcceptAnimal(animal))
                    GenSpawn.Spawn(animal, pawn.Position, pawn.Map); // 入舱失败兜底放回，防动物丢失
            });
        }
    }

    /// <summary>
    /// 放出动物作业（targetA=畜牧机器）：走到机器旁放出动物到相邻空格。
    /// </summary>
    public class JobDriver_UnloadLivestockMachine : JobDriver
    {
        private CompLivestockMachine Machine
        {
            get
            {
                Thing t = job.GetTarget(TargetIndex.A).Thing;
                return t is ThingWithComps twc ? twc.TryGetComp<CompLivestockMachine>() : null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 不预定设备（理由同放入作业）
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            // 舱内动物已被放出则中止
            this.FailOn(() => Machine == null || Machine.ContainedAnimal == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.DoAtomic(delegate
            {
                Machine?.ReleaseAnimal();
            });
        }
    }

    [DefOf]
    public static class MMT_LivestockDefOf
    {
        /// <summary>畜牧机器（建筑）。</summary>
        public static ThingDef MMT_LivestockMachine;
        /// <summary>放入动物作业。</summary>
        public static JobDef MMT_LoadLivestockMachine;
        /// <summary>放出动物作业。</summary>
        public static JobDef MMT_UnloadLivestockMachine;

        static MMT_LivestockDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_LivestockDefOf));
        }
    }
}
