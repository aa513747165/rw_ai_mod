using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 使用效果参数：使用物品后要生成的建筑 def（部署式护盾投影器）。
    /// </summary>
    public class CompProperties_UseEffectSpawnBuilding : CompProperties_UseEffect
    {
        /// <summary>使用后在使用者脚下生成的建筑 def（临时护盾投影器）。</summary>
        public ThingDef buildingDef;

        public CompProperties_UseEffectSpawnBuilding()
        {
            compClass = typeof(CompUseEffect_SpawnBuilding);
        }
    }

    /// <summary>
    /// 使用效果 Comp：在使用者脚下生成指定建筑（临时护盾投影器）并归属使用者阵营。
    /// 物品销毁由原版 CompUseEffect_DestroySelf 负责，本 Comp 只管生成。
    /// </summary>
    public class CompUseEffect_SpawnBuilding : CompUseEffect
    {
        private CompProperties_UseEffectSpawnBuilding Props
        {
            get { return (CompProperties_UseEffectSpawnBuilding)props; }
        }

        /// <summary>流程：在使用者脚下生成建筑并归属其阵营（护盾只拦外部投射物）。</summary>
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (Props.buildingDef == null || usedBy.Map == null)
                return;
            // 流程开始：生成建筑（护盾投影器可站立，与使用者同格无冲突）
            Thing shield = GenSpawn.Spawn(Props.buildingDef, usedBy.Position, usedBy.Map, Rot4.North);
            shield.SetFaction(usedBy.Faction);
            // 流程结束
        }
    }
}
