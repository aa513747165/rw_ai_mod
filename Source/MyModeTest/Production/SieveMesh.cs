using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MyModeTest
{
    /// <summary>
    /// 滤网过滤配置（挂在滤网物品 ThingDef 上的 modExtension）：白名单矿石 + 沙子概率 + 档位。
    /// 全部走 XML 配置，改过滤名单/概率/档位无需重编译；其它 mod 可用 XPath Patch 增删矿石。
    /// tier 用于"未分类矿"（不在任何滤网白名单的矿石，含第三方 mod 矿脉）归窗：
    /// 0=粗（木滤网）、1=中（铁滤网）、2=细（精密滤网）。
    /// 继承 InstallableSlotItemExtension（slotKey=Mesh）：与增效模块同属可装入槽位的物品体系。
    /// </summary>
    public class MeshFilterExtension : InstallableSlotItemExtension
    {
        /// <summary>可筛出的矿石白名单（矿石 drop 的 ThingDef）。</summary>
        public List<ThingDef> ores;
        /// <summary>每轮只筛出沙子的概率（0~1）。</summary>
        public float sandChance;
        /// <summary>档位：未分类矿按矿脉权重归窗后，只落入对应档位的滤网。</summary>
        public int tier;
    }

    public class CompProperties_SieveMesh : CompProperties
    {
        public CompProperties_SieveMesh()
        {
            compClass = typeof(CompSieveMesh);
        }
    }

    /// <summary>
    /// 筛子滤网槽 Comp：记录已安装滤网 def（安装即销毁物品，卸载/拆除再造返还，无耐久）。
    /// 装上后筛分只在滤网白名单内抽取并按概率掺沙子；无滤网维持全池随机（现状）。
    /// </summary>
    public class CompSieveMesh : CompItemSlotBase
    {
        // 已安装滤网 def（物品实体在安装时销毁，卸载/拆除时再造返还）
        private ThingDef installedMesh;

        // 全部滤网物品 def（带 MeshFilterExtension 者），右键菜单用；延迟扫描一次
        private static List<ThingDef> meshDefs;

        public override JobDef InstallJobDef
        {
            get { return MMT_DefOf.MMT_InstallSieveMesh; }
        }

        protected override string ForbiddenLabel
        {
            get { return "安装滤网"; }
        }

        public bool HasMesh
        {
            get { return installedMesh != null; }
        }

        public ThingDef MeshDef
        {
            get { return installedMesh; }
        }

        /// <summary>该物品是否为滤网（以是否挂过滤配置为准，新增滤网物品自动识别）。</summary>
        public static bool IsMesh(ThingDef def)
        {
            return def != null && def.HasModExtension<MeshFilterExtension>();
        }

        /// <summary>地图上全部滤网物品 def。</summary>
        public static List<ThingDef> AllMeshDefs()
        {
            if (meshDefs == null)
            {
                meshDefs = new List<ThingDef>();
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    if (IsMesh(def))
                        meshDefs.Add(def);
            }
            return meshDefs;
        }

        /// <summary>该物品是否为可装入的滤网（槽位空且是滤网）。</summary>
        public override bool CanInstallThing(ThingDef def)
        {
            return installedMesh == null && IsMesh(def);
        }

        /// <summary>安装一件滤网物品：记录 def 并销毁物品；已有滤网或非滤网返回 false 不消耗。</summary>
        public override bool InstallThing(Thing mesh)
        {
            if (!CanInstallThing(mesh.def))
                return false;
            installedMesh = mesh.def;
            mesh.Destroy();
            return true;
        }

        /// <summary>卸载滤网：掉落到交互格返还物品（供页签按钮）。</summary>
        public void Uninstall()
        {
            if (installedMesh == null)
                return;
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(installedMesh), parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
            installedMesh = null;
        }

        /// <summary>拆除包装/被摧毁时返还已安装滤网（在建筑原位掉落）。</summary>
        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            if (installedMesh != null)
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(installedMesh), parent.Position, map, ThingPlaceMode.Near);
            installedMesh = null;
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref installedMesh, "installedMesh");
        }

        public override string CompInspectStringExtra()
        {
            if (installedMesh == null)
                return "未安装滤网：全池随机筛选";
            MeshFilterExtension ext = installedMesh.GetModExtension<MeshFilterExtension>();
            string text = "滤网：" + installedMesh.label;
            if (ext != null && ext.sandChance > 0f)
                text += "（掺沙 " + ext.sandChance.ToStringPercent() + "）";
            return text;
        }

        /// <summary>安装候选：槽位被占用时给一个禁用项引导卸载；否则逐个滤网 def 给安装项。</summary>
        protected override IEnumerable<SlotOffer> GetOffers(Pawn selPawn)
        {
            if (installedMesh != null)
            {
                // 流程节点：槽位已占用，引导到页签卸载
                yield return new SlotOffer
                {
                    label = "已安装" + installedMesh.label + "（更换请先在滤网页签卸载）",
                    disabled = true,
                };
                yield break;
            }
            foreach (ThingDef meshDef in AllMeshDefs())
            {
                yield return new SlotOffer
                {
                    label = "安装" + meshDef.label,
                    itemDef = meshDef,
                };
            }
        }
    }

    /// <summary>
    /// 筛子"滤网"页签：显示已装滤网、掺沙概率与可筛矿石白名单，提供卸载按钮；
    /// 安装走右键菜单由殖民者搬运。
    /// </summary>
    public class ITab_SieveMesh : ITab
    {
        public ITab_SieveMesh()
        {
            size = new Vector2(420f, 260f);
            // 页签标题走 Keyed 翻译（1.7/Languages/ChineseSimplified/Keyed/MMT_InspectorTabs.xml）
            labelKey = "MMT_TabSieveMesh";
        }

        protected override void FillTab()
        {
            ThingWithComps building = SelThing as ThingWithComps;
            CompSieveMesh slot = building != null ? building.GetComp<CompSieveMesh>() : null;
            if (slot == null)
                return;

            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            // 安装引导：右键设备由殖民者搬运滤网物品装入
            listing.Label("安装方式：选中殖民者右键本设备，选择安装滤网。");
            listing.Gap(6f);

            if (!slot.HasMesh)
            {
                listing.Label("未安装滤网：筛出矿石按全部矿脉权重随机。");
                listing.End();
                return;
            }

            ThingDef meshDef = slot.MeshDef;
            MeshFilterExtension ext = meshDef.GetModExtension<MeshFilterExtension>();
            listing.Label("已安装：" + meshDef.label);
            if (ext != null)
            {
                listing.Label("掺沙概率：" + ext.sandChance.ToStringPercent());
                // 白名单矿石列表，方便玩家查看过滤范围
                if (ext.ores != null && ext.ores.Count > 0)
                {
                    StringBuilder sb = new StringBuilder("可筛出：");
                    for (int i = 0; i < ext.ores.Count; i++)
                    {
                        if (i > 0)
                            sb.Append("、");
                        sb.Append(ext.ores[i].label);
                    }
                    listing.Label(sb.ToString());
                }
            }
            Rect btnRect = listing.GetRect(30f);
            if (Widgets.ButtonText(btnRect, "卸载滤网", true, true, true))
                slot.Uninstall();
            listing.End();
        }
    }
}
