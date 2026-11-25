using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 造墙/地形障碍框架（第十八步：石墙术） ==========
    // 在选定范围内可建造的格子（无建筑/无 pawn/无物品、可站立）召唤临时石墙：
    // 满填墙体（挡路挡视线挡子弹、可托屋顶、不可燃），无主派系（不属玩家也不属敌人），
    // 限时走原版 CompLifespan（age 偏移实现运行时自定义时长），到期崩解消失。
    // MMTWallUtil.RaiseWall 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTWall
    // （radius 预览圈复用第一步 AoETarget 框架）；建筑演示侧出口：
    // AbilityTargeting.cs 的 CompSkillDemo（demoType="wall"）。
    // 素材仅原版尘雾 fleck/落石音效 + MMT 占位石墙贴图，不依赖 RoM。

    /// <summary>造墙参数包（MMTWallUtil.RaiseWall 的输入）。</summary>
    public class MMTWallParams
    {
        /// <summary>墙体建筑 Def（null 时由调用方解析 MMT_MagicStoneWall）。</summary>
        public ThingDef wallDef;
        /// <summary>墙体存续 tick（0 = 用墙体 Def 自带 lifespan）。</summary>
        public int durationTicks;
        public SoundDef startSound;
    }

    /// <summary>造墙统一入口。</summary>
    public static class MMTWallUtil
    {
        public const string WallDefName = "MMT_MagicStoneWall";

        /// <summary>判定格子是否可召唤墙体（无建筑/pawn/物品且可站立）。</summary>
        public static bool CanRaiseAt(IntVec3 cell, Map map)
        {
            return cell.InBounds(map) && cell.Standable(map)
                && cell.GetEdifice(map) == null && cell.GetFirstPawn(map) == null
                && cell.GetFirstItem(map) == null;
        }

        /// <summary>在 center 半径内全部可建格召唤石墙，返回成功召唤数。</summary>
        public static int RaiseWall(Thing caster, IntVec3 center, float radius, MMTWallParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !center.IsValid || !center.InBounds(map))
            {
                return 0;
            }
            ThingDef def = p.wallDef ?? DefDatabase<ThingDef>.GetNamedSilentFail(WallDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + WallDefName + " 定义，无法施放石墙术");
                return 0;
            }
            int count = 0;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!CanRaiseAt(cell, map))
                {
                    continue;
                }
                Building wall = (Building)GenSpawn.Spawn(def, cell, map);
                // 限时：原版 CompLifespan 按 age >= lifespanTicks 到期，偏移 age 实现运行时自定义时长
                if (p.durationTicks > 0)
                {
                    CompLifespan life = wall.GetComp<CompLifespan>();
                    if (life != null)
                    {
                        life.age = Mathf.Max(0, life.Props.lifespanTicks - p.durationTicks);
                    }
                }
                FleckMaker.ThrowDustPuff(cell, map, 1.2f);
                count++;
            }
            if (count > 0 && p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(center, map));
            }
            return count;
        }
    }

    // ========== pawn 技能出口：造墙（radius 圈选范围） ==========

    public class CompProperties_MMTWall : CompProperties_MMTAoETargetBase
    {
        public string wallDefName = "MMT_MagicStoneWall";
        /// <summary>墙体存续秒数（0 = 用墙体 Def 自带 lifespan）。</summary>
        public float durationSeconds = 30f;
        public string soundDefName = "ChunkRock_Drop";

        public CompProperties_MMTWall()
        {
            compClass = typeof(CompAbilityEffect_MMTWall);
            radius = 2.5f;
        }
    }

    /// <summary>石墙术：在圈选范围内召唤一片临时石墙（到期崩解）。</summary>
    public class CompAbilityEffect_MMTWall : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTWall Props => (CompProperties_MMTWall)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTWallUtil.RaiseWall(caster, target.Cell, Props.radius, new MMTWallParams
            {
                wallDef = Props.wallDefName.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(Props.wallDefName),
                durationTicks = Props.durationSeconds > 0f ? Props.durationSeconds.SecondsToTicks() : 0,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            });
        }
    }
}
