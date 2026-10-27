using System.Linq;
using RimWorld.Planet;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 折跃荒野临时落脚点：传送到空白世界地块时生成的无派系 MapParent。
    /// 地图上不再有存活己方人员时自动销毁地图并移除本世界对象（不占常驻世界对象）；
    /// 遗留物品随地图一并销毁。另由 CompTeleportPad.ExecuteRecall 在召回后立即触发一次清理。
    /// </summary>
    public class MMT_TeleportTempSite : MapParent
    {
        /// <summary>自动清理判定：地图未生成（异常残留）或图上已无存活己方人员 → 销毁。</summary>
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = true;
            return !HasMap || !Map.mapPawns.AllPawnsSpawned.Any(
                p => p.Faction != null && p.Faction.IsPlayer && !p.Dead);
        }

        /// <summary>
        /// 定时自检（双保险）：若基类未自动调用 CheckRemoveMapNow，这里按 250 tick 间隔手动触发。
        /// </summary>
        protected override void Tick()
        {
            base.Tick();
            if ((ID + Find.TickManager.TicksGame) % 250 == 0)
                CheckRemoveMapNow();
        }
    }
}
