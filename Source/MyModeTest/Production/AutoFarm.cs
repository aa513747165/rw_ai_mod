using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 自动种植机（5x5）：无人值守全自动种植与收获。
    /// 整机统一种植一种植物（gizmo 全局切换），25 个虚拟种植格逐 tick 推进生长，
    /// 成熟后自动收获到交互格并原地重播。基础肥力 300%（封闭式水培，不受光照/季节限制），
    /// 燃料舱（CompRefuelable，燃料=肥料）有存货时额外 +100% 肥力（即 400%）。
    /// 绘制时在建筑泥土底图之上按生长度缩放画出每格植物图形。
    /// </summary>
    public class Building_AutoFarm : Building
    {
        /// <summary>基础肥力系数（无肥料加成时）。</summary>
        private const float BaseFertility = 3f;
        /// <summary>燃料舱有肥料时的额外肥力系数。</summary>
        private const float FuelFertilityBonus = 1f;
        /// <summary>植物贴图最长边封顶（格）：盘体外沿约 0.66 格、盘间距约 0.6 格，
        /// 封顶后成熟植株收在盘沿内，不会溢出到相邻培养盘或机外。</summary>
        private const float MaxPlantDrawSize = 0.55f;

        // 各朝向贴图的盘心实测坐标（单位：格，建筑中心为原点，列为西→东、行为南→北）。
        // 三张贴图是独立美术资源、并非严格旋转对称，故逐朝向建表（土壤区域直方图加权中心）；
        // 西向不单独出图，复用东向表做水平镜像（Graphic_Multi 西向即东向贴图左右翻转）。
        private static readonly float[] TrayX_South = { -1.298f, -0.650f, 0.003f, 0.653f, 1.309f };
        private static readonly float[] TrayZ_South = { -1.079f, -0.473f, 0.115f, 0.712f, 1.307f };
        private static readonly float[] TrayX_East  = { -1.375f, -0.750f, -0.125f, 0.502f, 1.130f };
        private static readonly float[] TrayZ_East  = { -1.058f, -0.443f, 0.172f, 0.770f, 1.349f };
        private static readonly float[] TrayX_North = { -1.365f, -0.705f, -0.035f, 0.636f, 1.299f };
        private static readonly float[] TrayZ_North = { -1.141f, -0.576f, -0.007f, 0.571f, 1.137f };

        // 当前种植的植物 def（全局一种，gizmo 切换；null = 未选种）
        private ThingDef selectedPlant;
        // 25 格虚拟种植格的生长进度（0~1），与 cells 平行
        private List<float> growths = new List<float>();
        // 缓存的种植格（PostSpawnSetup 时按占地生成，重放/重建后重取）
        private List<IntVec3> cells = new List<IntVec3>();

        // comps 引用缓存，避免每 tick 线性扫描
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        private CompBreakdownable breakdownComp;
        private CompRefuelable fuelComp;

        /// <summary>全部可播种植物（sowTags 含 "Ground" 或 "Hydroponic"，本机为封闭式水培，
        /// 两者都支持——如辉晶花这类水培限定作物）；延迟扫描一次。</summary>
        private static List<ThingDef> sowablePlants;

        public static List<ThingDef> SowablePlants
        {
            get
            {
                if (sowablePlants == null)
                {
                    sowablePlants = new List<ThingDef>();
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.plant != null && def.plant.sowTags != null
                            && (def.plant.sowTags.Contains("Ground") || def.plant.sowTags.Contains("Hydroponic")))
                            sowablePlants.Add(def);
                    }
                    sowablePlants.SortBy(def => def.label);
                }
                return sowablePlants;
            }
        }

        /// <summary>当前肥力系数：300%，燃料仓有肥料时 400%。</summary>
        public float CurrentFertility
        {
            get { return BaseFertility + (fuelComp != null && fuelComp.Fuel > 0f ? FuelFertilityBonus : 0f); }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            flickComp = GetComp<CompFlickable>();
            breakdownComp = GetComp<CompBreakdownable>();
            fuelComp = GetComp<CompRefuelable>();
            cells.Clear();
            foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(this))
                cells.Add(cell);
            EnsureGrowthSlots();
        }

        /// <summary>保证生长进度数组与格子数一致（新建筑/旧存档缺数据时补零）。
        /// 注意：ExposeData 的 PostLoadInit 早于 SpawnSetup，此时 cells 尚未填充，
        /// 格子数按 def 尺寸兜底，否则读档会把刚加载的进度误判为多余数据清空。</summary>
        private void EnsureGrowthSlots()
        {
            int count = cells.Count > 0 ? cells.Count : def.size.x * def.size.z;
            while (growths.Count < count)
                growths.Add(0f);
            if (growths.Count > count)
                growths.RemoveRange(count, growths.Count - count);
        }

        protected override void Tick()
        {
            base.Tick();
            if (selectedPlant == null || cells.Count == 0)
                return;
            // 断电、手动关闸或故障时生长冻结
            if (powerComp != null && !powerComp.PowerOn)
                return;
            if (flickComp != null && !flickComp.SwitchIsOn)
                return;
            if (breakdownComp != null && breakdownComp.BrokenDown)
                return;

            float growDays = selectedPlant.plant.growDays;
            if (growDays <= 0f)
                return;
            // 对齐原版生长速率公式：满状态每 tick 1/(60000*growDays)，乘肥力系数
            float growthPerTick = CurrentFertility / (GenDate.TicksPerDay * growDays);
            bool harvested = false;
            for (int i = 0; i < cells.Count; i++)
            {
                float g = growths[i] + growthPerTick;
                if (g >= 1f)
                {
                    HarvestCell(i);
                    harvested = true;
                }
                else
                {
                    growths[i] = g;
                }
            }
            // 收获后格子归零重播，贴图需要刷新频率较低，无需手动 dirty
            if (harvested)
                return;
        }

        /// <summary>收获单格：按植物定义产出收获物到交互格，进度归零自动重播。</summary>
        private void HarvestCell(int index)
        {
            growths[index] = 0f;
            ThingDef productDef = selectedPlant.plant.harvestedThingDef;
            int yield = Mathf.RoundToInt(selectedPlant.plant.harvestYield);
            if (productDef == null || yield <= 0)
                return;
            Thing product = ThingMaker.MakeThing(productDef);
            product.stackCount = yield;
            GenPlace.TryPlaceThing(product, InteractionCell, Map, ThingPlaceMode.Near);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (selectedPlant == null)
                return;
            Material mat = selectedPlant.graphic.MatSingleFor(this);
            float baseAlt = AltitudeLayer.Building.AltitudeFor() + 0.05f;
            Vector2 drawSize = selectedPlant.graphic.drawSize;
            Vector3 center = DrawPos;
            for (int i = 0; i < cells.Count; i++)
            {
                float growth = growths[i];
                if (growth <= 0.01f)
                    continue;
                // 原版植物同款缩放曲线：随生长度从幼苗到成株
                float scale = selectedPlant.plant.visualSizeRange.LerpThroughRange(growth);
                // 盘面内缩、盘沿仅约 0.66 格：封顶最长边，防止成熟植株溢出培养盘
                float w = drawSize.x * scale;
                float h = drawSize.y * scale;
                float maxSide = Mathf.Max(w, h);
                if (maxSide > MaxPlantDrawSize)
                {
                    float k = MaxPlantDrawSize / maxSide;
                    w *= k;
                    h *= k;
                }
                // 盘面在贴图内缩且三朝向贴图各自独立：按格子在占地内的列/行
                // 查当前朝向的实测盘心表，直接得到世界坐标（贴图按朝向绘制，无需再旋转）
                Vector3 local = cells[i].ToVector3Shifted() - center;
                int col = Mathf.Clamp(Mathf.RoundToInt(local.x) + 2, 0, 4);
                int row = Mathf.Clamp(Mathf.RoundToInt(local.z) + 2, 0, 4);
                float tx, tz;
                switch (Rotation.AsInt)
                {
                    case 0: tx = TrayX_North[col]; tz = TrayZ_North[row]; break;
                    case 1: tx = TrayX_East[col]; tz = TrayZ_East[row]; break;
                    case 2: tx = TrayX_South[col]; tz = TrayZ_South[row]; break;
                    default: tx = -TrayX_East[4 - col]; tz = TrayZ_East[row]; break; // 西 = 东贴图水平镜像
                }
                Matrix4x4 matrix = Matrix4x4.TRS(
                    new Vector3(center.x + tx, baseAlt, center.z + tz),
                    Quaternion.identity,
                    new Vector3(w, 1f, h));
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
        }

        // 未选种时的默认 gizmo 图标（原版种植区"选择种植植物"同款；null 图标会画成红 X）
        private static Texture2D defaultPlantIcon;

        private static Texture2D DefaultPlantIcon
        {
            get
            {
                if (defaultPlantIcon == null)
                    defaultPlantIcon = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow");
                return defaultPlantIcon;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;
            Command_Action selectPlant = new Command_Action
            {
                defaultLabel = selectedPlant == null ? "选择种植植物" : "种植: " + selectedPlant.label,
                defaultDesc = "选择本机自动种植与收获的植物。支持所有可种植植物，切换后重新播种。",
                icon = selectedPlant != null ? selectedPlant.uiIcon : DefaultPlantIcon,
                action = OpenPlantMenu,
            };
            yield return selectPlant;
        }

        /// <summary>弹出全部可播种植物的选种菜单；切换植物时清空生长进度重新播种。</summary>
        private void OpenPlantMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (ThingDef def in SowablePlants)
            {
                ThingDef plant = def;
                options.Add(new FloatMenuOption(
                    plant.LabelCap,
                    delegate
                    {
                        selectedPlant = plant;
                        for (int i = 0; i < growths.Count; i++)
                            growths[i] = 0f;
                    },
                    shownItemForIcon: plant));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            if (sb.Length > 0)
                sb.AppendLine();
            if (selectedPlant == null)
            {
                sb.Append("未选择种植植物");
            }
            else
            {
                float sum = 0f;
                for (int i = 0; i < growths.Count; i++)
                    sum += growths[i];
                float avg = growths.Count > 0 ? sum / growths.Count : 0f;
                sb.Append("种植: " + selectedPlant.label + "，平均生长 " + avg.ToStringPercent());
            }
            sb.AppendLine();
            sb.Append("肥力: " + (CurrentFertility * 100f).ToString("F0") + "%");
            if (fuelComp != null && fuelComp.Fuel > 0f)
                sb.Append("（肥料增效中）");
            return sb.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref selectedPlant, "selectedPlant");
            Scribe_Collections.Look(ref growths, "growths", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (growths == null)
                    growths = new List<float>();
                EnsureGrowthSlots();
            }
        }
    }
}
