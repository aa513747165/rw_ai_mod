using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MyModeTest
{
    /// <summary>
    /// 小火元素死亡爆燃（爆炸羊式亡语）：死亡瞬间以尸体为中心放一次火焰爆炸并点燃地面。
    /// 原版 DeathActionWorker_Small/BigExplosion 固定 Bomb 伤害且 XML 不可配参数（DeathActionProperties
    /// 只有 workerClass 字段），火系定制必须自写 worker；火焰爆炸用 DamageDefOf.Flame +
    /// chanceToStartFire（同原版燃烧弹管线）。召唤物到期消失走 Destroy 不经死亡流程，不会引爆。
    /// </summary>
    public class DeathActionWorker_MMTFlameBurst : DeathActionWorker
    {
        // 近战攻击者提示危险（同爆炸鼠/爆炸羊）
        public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            Map map = corpse.MapHeld;
            if (map == null)
            {
                return;
            }
            GenExplosion.DoExplosion(corpse.Position, map, 2.9f, DamageDefOf.Flame, corpse.InnerPawn,
                damAmount: 20, armorPenetration: 0.1f, chanceToStartFire: 0.5f);
        }
    }
}
