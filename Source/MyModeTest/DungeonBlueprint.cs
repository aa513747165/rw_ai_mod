using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    /// <summary>蓝图单格条目：建筑 defName/材质/相对偏移（选区左下角为原点）/朝向。</summary>
    public class DungeonBlueprintEntry : IExposable
    {
        public string defName;
        public string stuffDefName;
        public IntVec3 offset;
        public int rot;

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref stuffDefName, "stuffDefName");
            Scribe_Values.Look(ref offset, "offset");
            Scribe_Values.Look(ref rot, "rot", 0);
        }
    }

    /// <summary>
    /// 副本蓝图（跨存档全局，存 MMTSettings）：框选保存的玩家布局。
    /// enabledSiteParts 为副本类型开关（SitePartDef defName）——只有勾选的据点类型才能随机选用本蓝图。
    /// </summary>
    public class DungeonBlueprint : IExposable
    {
        /// <summary>蓝图名（唯一键）。</summary>
        public string name = "";

        /// <summary>选区尺寸（格）。</summary>
        public int width;

        public int height;

        /// <summary>建筑条目。</summary>
        public List<DungeonBlueprintEntry> entries = new List<DungeonBlueprintEntry>();

        /// <summary>副本类型开关：启用本蓝图的据点 SitePartDef defName 列表（空 = 不被任何据点选用）。</summary>
        public List<string> enabledSiteParts = new List<string>();

        /// <summary>档位开关：启用本蓝图的扫描档位（1-3；默认全档）。</summary>
        public List<int> enabledTiers = new List<int> { 1, 2, 3 };

        // —— 运行时缓存（不存档） ——
        private Texture2D thumbnail;

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name", "");
            Scribe_Values.Look(ref width, "width", 0);
            Scribe_Values.Look(ref height, "height", 0);
            Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
            Scribe_Collections.Look(ref enabledSiteParts, "enabledSiteParts", LookMode.Value);
            Scribe_Collections.Look(ref enabledTiers, "enabledTiers", LookMode.Value);
            if (entries == null)
                entries = new List<DungeonBlueprintEntry>();
            if (enabledSiteParts == null)
                enabledSiteParts = new List<string>();
            if (enabledTiers == null || enabledTiers.Count == 0)
                enabledTiers = new List<int> { 1, 2, 3 }; // 旧存档蓝图默认全档
        }

        /// <summary>本蓝图是否启用某据点类型。</summary>
        public bool EnabledFor(string sitePartDefName)
        {
            return sitePartDefName != null && enabledSiteParts.Contains(sitePartDefName);
        }

        public void SetEnabled(string sitePartDefName, bool on)
        {
            if (sitePartDefName.NullOrEmpty())
                return;
            if (on && !enabledSiteParts.Contains(sitePartDefName))
                enabledSiteParts.Add(sitePartDefName);
            else if (!on)
                enabledSiteParts.Remove(sitePartDefName);
        }

        /// <summary>本蓝图是否启用某档位（1-3）。</summary>
        public bool EnabledAtTier(int tier)
        {
            return enabledTiers.Contains(tier);
        }

        public void SetTierEnabled(int tier, bool on)
        {
            if (on && !enabledTiers.Contains(tier))
                enabledTiers.Add(tier);
            else if (!on)
                enabledTiers.Remove(tier);
        }

        /// <summary>缩小截图：按条目类别着色逐格绘制（炮塔红/门黄/满格墙灰/用电橙/其它蓝灰），缓存一次。</summary>
        public Texture2D GetThumbnail()
        {
            if (thumbnail != null)
                return thumbnail;
            int w = Mathf.Max(1, width);
            int h = Mathf.Max(1, height);
            int px = Mathf.Max(1, 96 / Mathf.Max(w, h));
            Texture2D tex = new Texture2D(w * px, h * px, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color32[] pixels = new Color32[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(20, 24, 32, 160);
            for (int i = 0; i < entries.Count; i++)
            {
                DungeonBlueprintEntry e = entries[i];
                if (e == null || e.offset.x < 0 || e.offset.x >= w || e.offset.z < 0 || e.offset.z >= h)
                    continue;
                Color32 color = EntryColor(e);
                for (int dx = 0; dx < px; dx++)
                {
                    for (int dz = 0; dz < px; dz++)
                    {
                        int x = e.offset.x * px + dx;
                        int y = e.offset.z * px + dz;
                        if (x < tex.width && y < tex.height)
                            pixels[y * tex.width + x] = color;
                    }
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
            thumbnail = tex;
            return thumbnail;
        }

        private static Color32 EntryColor(DungeonBlueprintEntry e)
        {
            ThingDef d = e.defName.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(e.defName);
            if (d == null)
                return new Color32(128, 128, 128, 255);
            if (d.building != null && d.building.turretGunDef != null)
                return new Color32(220, 80, 60, 255); // 炮塔：红
            if (typeof(Building_Door).IsAssignableFrom(d.thingClass))
                return new Color32(230, 200, 80, 255); // 门：黄
            if (d.Fillage == FillCategory.Full)
                return new Color32(200, 200, 205, 255); // 墙/满格：浅灰
            if (d.HasComp(typeof(CompPowerTrader)))
                return new Color32(240, 150, 60, 255); // 用电设备：橙
            return new Color32(120, 150, 200, 255); // 其它：蓝灰
        }
    }

    /// <summary>蓝图存取（MMTSettings 全局，跨存档）。</summary>
    public static class DungeonBlueprintStore
    {
        public static List<DungeonBlueprint> All
        {
            get
            {
                if (MMTSettings.Instance != null)
                    return MMTSettings.Instance.dungeonBlueprints;
                return new List<DungeonBlueprint>();
            }
        }

        public static DungeonBlueprint Find(string name)
        {
            if (name.NullOrEmpty())
                return null;
            List<DungeonBlueprint> all = All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] != null && all[i].name == name)
                    return all[i];
            }
            return null;
        }

        /// <summary>新增或同名覆盖，并立即落盘。</summary>
        public static void AddOrReplace(DungeonBlueprint bp)
        {
            if (bp == null || MMTSettings.Instance == null)
                return;
            Remove(bp.name, false);
            MMTSettings.Instance.dungeonBlueprints.Add(bp);
            InvalidatePickCache();
            MMTSettings.Instance.Write();
        }

        public static void Remove(string name, bool save = true)
        {
            if (MMTSettings.Instance == null)
                return;
            MMTSettings.Instance.dungeonBlueprints.RemoveAll(b => b == null || b.name == name);
            InvalidatePickCache();
            if (save)
                MMTSettings.Instance.Write();
        }

        // 可选蓝图缓存：蓝图集合只在设置面板/保存时变化，生成侧每次据点最多算一次
        private static readonly Dictionary<string, List<DungeonBlueprint>> pickCache =
            new Dictionary<string, List<DungeonBlueprint>>();

        private static int pickCacheCount = -1;

        /// <summary>使蓝图选用缓存失效（增删蓝图/切换类型开关后调用）。</summary>
        public static void InvalidatePickCache()
        {
            pickCacheCount = -1;
            pickCache.Clear();
        }

        // 副本类据点类型缓存（带 DungeonLayoutExtension 的 SitePartDef；Def 库加载后不变）
        private static List<SitePartDef> dungeonSiteParts;

        /// <summary>全部支持蓝图的据点类型（设置面板类型开关列表用）。</summary>
        public static List<SitePartDef> DungeonSiteParts
        {
            get
            {
                if (dungeonSiteParts == null)
                {
                    dungeonSiteParts = new List<SitePartDef>();
                    foreach (SitePartDef d in DefDatabase<SitePartDef>.AllDefsListForReading)
                    {
                        if (d.GetModExtension<DungeonLayoutExtension>() != null)
                            dungeonSiteParts.Add(d);
                    }
                }
                return dungeonSiteParts;
            }
        }

        /// <summary>为据点类型 + 档位随机抽一张已启用的蓝图（无可用返回 null，调用方走程序化布局兜底）。</summary>
        public static DungeonBlueprint PickFor(SitePartDef sitePart, int tier)
        {
            if (sitePart == null)
                return null;
            List<DungeonBlueprint> all = All;
            if (all.Count == 0)
                return null;
            // 蓝图总数变化时重建按 类型+档位 的候选缓存
            if (pickCacheCount != all.Count)
            {
                pickCacheCount = all.Count;
                pickCache.Clear();
            }
            string key = sitePart.defName + "#" + tier;
            List<DungeonBlueprint> candidates;
            if (!pickCache.TryGetValue(key, out candidates))
            {
                candidates = new List<DungeonBlueprint>();
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] != null && all[i].entries.Count > 0
                        && all[i].EnabledFor(sitePart.defName) && all[i].EnabledAtTier(tier))
                        candidates.Add(all[i]);
                }
                pickCache[key] = candidates;
            }
            return candidates.Count > 0 ? candidates.RandomElement() : null;
        }
    }

    /// <summary>
    /// 蓝图保存设计器（建筑师菜单"副本蓝图"分类，DesignationCategoryDef.specialDesignatorClasses 挂载）：
    /// 框选矩形 → 捕捉矩形内全部已建成建筑（排除 Frame/未建成蓝图）→ 命名存入全局设置。
    /// </summary>
    public class Designator_SaveDungeonBlueprint : Designator
    {
        /// <summary>单边尺寸上限（防误框全图）。</summary>
        public const int MaxEdge = 40;

        public Designator_SaveDungeonBlueprint()
        {
            defaultLabel = "保存为副本蓝图";
            defaultDesc = "框选一片区域，把其中的墙/门/炮塔/建筑保存为副本蓝图。\n\n保存后在 Mod 设置【副本蓝图】中勾选启用的据点类型（试炼场/职业圣殿/Boss 巢穴等），该类型据点生成时会随机选用已启用的蓝图布局，敌人仍按威胁点动态生成。";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/MMT_BlueprintSave");
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(Map);
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            // 框选矩形归一化
            IntVec3 min = new IntVec3(int.MaxValue, 0, int.MaxValue);
            IntVec3 max = new IntVec3(int.MinValue, 0, int.MinValue);
            int cellCount = 0;
            foreach (IntVec3 c in cells)
            {
                if (!c.InBounds(Map))
                    continue;
                cellCount++;
                if (c.x < min.x) min.x = c.x;
                if (c.z < min.z) min.z = c.z;
                if (c.x > max.x) max.x = c.x;
                if (c.z > max.z) max.z = c.z;
            }
            if (cellCount == 0)
                return;
            int w = max.x - min.x + 1;
            int h = max.z - min.z + 1;
            if (w > MaxEdge || h > MaxEdge)
            {
                Messages.Message("蓝图选区过大（单边最多 " + MaxEdge + " 格）", MessageTypeDefOf.RejectInput, false);
                return;
            }
            // 捕捉已建成建筑（多格建筑只记其 Position 一次）
            DungeonBlueprint bp = new DungeonBlueprint { width = w, height = h };
            HashSet<Thing> seen = new HashSet<Thing>();
            for (int x = min.x; x <= max.x; x++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    List<Thing> things = Map.thingGrid.ThingsListAtFast(new IntVec3(x, 0, z));
                    for (int i = 0; i < things.Count; i++)
                    {
                        Thing t = things[i];
                        if (!(t is Building) || t is Frame || t is Blueprint || t.Position.x != x || t.Position.z != z)
                            continue;
                        if (!seen.Add(t))
                            continue;
                        bp.entries.Add(new DungeonBlueprintEntry
                        {
                            defName = t.def.defName,
                            stuffDefName = t.Stuff != null ? t.Stuff.defName : null,
                            offset = new IntVec3(t.Position.x - min.x, 0, t.Position.z - min.z),
                            rot = t.Rotation.AsInt,
                        });
                    }
                }
            }
            if (bp.entries.Count == 0)
            {
                Messages.Message("选区内没有可保存的建筑（未建成的框架/蓝图不计）", MessageTypeDefOf.RejectInput, false);
                return;
            }
            Find.WindowStack.Add(new Dialog_NameDungeonBlueprint(bp));
        }
    }

    /// <summary>蓝图命名弹窗：重名时确认覆盖。</summary>
    public class Dialog_NameDungeonBlueprint : Window
    {
        private readonly DungeonBlueprint blueprint;
        private string name = "";

        public Dialog_NameDungeonBlueprint(DungeonBlueprint blueprint)
        {
            this.blueprint = blueprint;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(420f, 160f); }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.Label("蓝图名称（" + blueprint.width + "×" + blueprint.height + "，"
                + blueprint.entries.Count + " 座建筑）：");
            name = Widgets.TextField(list.GetRect(28f), name);
            list.Gap();
            Rect row = list.GetRect(30f);
            if (Widgets.ButtonText(row.LeftHalf(), "保存") && !name.NullOrEmpty())
            {
                blueprint.name = name.Trim();
                if (blueprint.name.Length == 0)
                    return;
                if (DungeonBlueprintStore.Find(blueprint.name) != null)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "已存在同名蓝图「" + blueprint.name + "」，覆盖？", DoSave));
                }
                else
                {
                    DoSave();
                }
            }
            if (Widgets.ButtonText(row.RightHalf(), "取消"))
                Close();
            list.End();
        }

        private void DoSave()
        {
            DungeonBlueprintStore.AddOrReplace(blueprint);
            Messages.Message("副本蓝图「" + blueprint.name + "」已保存（" + blueprint.entries.Count
                + " 座建筑）。请到 Mod 设置【副本蓝图】勾选启用的据点类型。",
                MessageTypeDefOf.PositiveEvent, false);
            Close();
        }
    }
}
