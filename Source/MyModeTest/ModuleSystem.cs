using System.Collections.Generic;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 可装入建筑槽位的物品识别扩展（挂物品 ThingDef 上）：
    /// slotKey 与建筑侧 CompProperties_ModuleSlots.slotLimits 的 key 匹配才可装，同 key 共享安装上限。
    /// 仿滤网 MeshFilterExtension 模式：新增可安装物品挂本扩展（或子类）即被槽 Comp 自动识别，零 C# 改动。
    /// </summary>
    public class InstallableSlotItemExtension : DefModExtension
    {
        /// <summary>槽位类别键（如 Overclock / Mesh），决定共享上限与互斥。</summary>
        public string slotKey;

        /// <summary>页签/菜单中的效果说明文案（完整中文文案，如"处理速度 +10%/个"）。</summary>
        public string effectReport;
    }

    /// <summary>单条模块效果：key 语义由消费端定义（见 ModuleEffectKeys），valuePerItem 每件累加。</summary>
    public class ModuleEffect
    {
        public string key;
        public float valuePerItem;
    }

    /// <summary>
    /// 增效模块配置（继承槽位识别扩展）：模块的效果列表全部 XML 化。
    /// 新增同组模块变体（如"高级超频"）= 纯 XML；新增效果语义 = 常量 1 行 + 消费端读取 1 行。
    /// </summary>
    public class ModuleExtension : InstallableSlotItemExtension
    {
        public List<ModuleEffect> effects;
    }

    /// <summary>内置效果键常量（消费端与 XML 约定用；新键无需登记，约定即可）。</summary>
    public static class ModuleEffectKeys
    {
        /// <summary>处理速度加成：每件 +0.1 → 间隔 / (1 + 合计)。</summary>
        public const string SpeedFactor = "SpeedFactor";
        /// <summary>生产线加成：每件 +1 条。</summary>
        public const string ExtraLanes = "ExtraLanes";
        /// <summary>产量加成：每件直接加单次产量（如 10）。</summary>
        public const string YieldBonus = "YieldBonus";
    }

    /// <summary>设备侧槽位上限条目：key 对应物品扩展的 slotKey。</summary>
    public class ModuleSlotLimit
    {
        public string key;
        public int max;
    }

    /// <summary>一条已安装槽位物品记录（物品实体在安装时销毁，只留 def 与槽位键）。</summary>
    public class InstalledSlotItem : IExposable
    {
        public string slotKey;
        public ThingDef itemDef;

        public void ExposeData()
        {
            Scribe_Values.Look(ref slotKey, "slotKey");
            Scribe_Defs.Look(ref itemDef, "itemDef");
        }
    }

    /// <summary>增效模块页签的一行展示数据（槽位键 + 代表物品 + 已装/上限 + 效果文案）。</summary>
    public class ModuleSlotRow
    {
        public string slotKey;
        public ThingDef itemDef;
        public int installed;
        public int max;
        public string effectText;
    }
}
