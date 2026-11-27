using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 造墙/地形障碍框架（第十八步：石墙术） ==========
    // 以施法者为锥顶、指向目标点的四分之一圆环（90° 扇形）范围内，仅在锥形
    // 末端外弧（约 1 格厚的环带）召唤一段弧形临时石墙：
    // 满填墙体（挡路挡视线挡子弹、可托屋顶、不可燃），无主派系（不属玩家也不
    // 属敌人），限时走原版 CompLifespan（age 偏移实现运行时自定义时长），到期崩解消失。
    // 墙体图形走原版 CornerFiller 链接（linkFlags Wall+Rock），相邻石墙之间、
    // 与原版墙/打磨岩壁视觉上连成一体。
    // MMTWallUtil.RaiseWall 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTWall
    // （锥形预览复用龙息的 MMTBreathUtil.DrawConePreview）；建筑演示侧出口：
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
        /// <summary>默认锥形半角（度）：45° 时全角 90° = 整圆四分之一。</summary>
        public const float DefaultHalfAngleDegrees = 45f;
        /// <summary>瞄画锥形预览用的贴图（与龙息预览同款细线光带）。</summary>
        public const string ConePreviewTexPath = "Things/Mote/MMT_SkillBeam";

        /// <summary>判定格子是否可召唤墙体（无建筑/pawn/物品且可站立）。</summary>
        public static bool CanRaiseAt(IntVec3 cell, Map map)
        {
            return cell.InBounds(map) && cell.Standable(map)
                && cell.GetEdifice(map) == null && cell.GetFirstPawn(map) == null
                && cell.GetFirstItem(map) == null;
        }

        /// <summary>
        /// 锥形扇区末端外弧的墙格枚举：以 origin 为锥顶、origin→targetCell 为中心轴，
        /// 半角 halfAngleDegrees，半径 range；只保留距锥顶 [range-1, range] 的 1 格厚外弧。
        /// targetCell 与锥顶重合时无朝向，空集。
        /// </summary>
        public static IEnumerable<IntVec3> WallCellsInCone(IntVec3 origin, IntVec3 targetCell, float range, float halfAngleDegrees)
        {
            if (range <= 0f)
            {
                yield break;
            }
            float dx = targetCell.x - origin.x;
            float dz = targetCell.z - origin.z;
            float dirLen = Mathf.Sqrt(dx * dx + dz * dz);
            if (dirLen < 0.001f)
            {
                yield break;
            }
            float cosLimit = Mathf.Cos(halfAngleDegrees * Mathf.Deg2Rad);
            float bandInner = Mathf.Max(0f, range - 1f);
            int ext = Mathf.CeilToInt(range);
            for (int ox = -ext; ox <= ext; ox++)
            {
                for (int oz = -ext; oz <= ext; oz++)
                {
                    float dist = Mathf.Sqrt(ox * ox + oz * oz);
                    // 只留末端外弧：距锥顶 range-1 ~ range 的环带
                    if (dist > range + 0.01f || dist < bandInner - 0.01f)
                    {
                        continue;
                    }
                    if (dist > 0.001f)
                    {
                        float dot = (ox * dx + oz * dz) / (dist * dirLen);
                        if (dot < cosLimit)
                        {
                            continue;
                        }
                    }
                    yield return new IntVec3(origin.x + ox, 0, origin.z + oz);
                }
            }
        }

        /// <summary>沿锥形末端外弧召唤石墙，返回成功召唤数。</summary>
        public static int RaiseWall(Thing caster, IntVec3 targetCell, float range, float halfAngleDegrees, MMTWallParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !targetCell.IsValid || !targetCell.InBounds(map))
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
            foreach (IntVec3 cell in WallCellsInCone(caster.Position, targetCell, range, halfAngleDegrees))
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
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(caster.Position, map));
            }
            return count;
        }

        /// <summary>锥形末端外弧中当前可建墙的格数（预览提示用）。</summary>
        public static int CountValidCells(Map map, IntVec3 origin, IntVec3 targetCell, float range, float halfAngleDegrees)
        {
            int n = 0;
            foreach (IntVec3 cell in WallCellsInCone(origin, targetCell, range, halfAngleDegrees))
            {
                if (CanRaiseAt(cell, map))
                {
                    n++;
                }
            }
            return n;
        }

        /// <summary>画石墙术瞄准预览：施法者为锥顶的扇形边界线+末端弧，并高亮末端外弧可建墙格。</summary>
        public static void DrawWallPreview(Thing caster, Map map, IntVec3 targetCell, float range, float halfAngleDegrees, Color color)
        {
            if (caster == null || map == null || map != Find.CurrentMap)
            {
                return;
            }
            Vector3 dir = MMTBeamUtil.BeamDirection(caster, targetCell);
            if (dir.sqrMagnitude > 0.01f)
            {
                MMTBreathUtil.DrawConePreview(map, caster.Position.ToVector3Shifted(), dir,
                    range, halfAngleDegrees, ConePreviewTexPath, color);
            }
            foreach (IntVec3 cell in WallCellsInCone(caster.Position, targetCell, range, halfAngleDegrees))
            {
                if (CanRaiseAt(cell, map))
                {
                    GenDraw.DrawTargetHighlightWithLayer(cell.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
                }
            }
        }
    }

    // ========== pawn 技能出口：造墙（锥形末端外弧） ==========

    public class CompProperties_MMTWall : CompProperties_MMTAoETargetBase
    {
        public string wallDefName = "MMT_MagicStoneWall";
        /// <summary>墙体存续秒数（0 = 用墙体 Def 自带 lifespan）。</summary>
        public float durationSeconds = 30f;
        /// <summary>锥形半角（度）：45° 时全角 90° = 整圆四分之一；锥形半径用 radius。</summary>
        public float coneHalfAngleDegrees = MMTWallUtil.DefaultHalfAngleDegrees;
        public string soundDefName = "ChunkRock_Drop";

        public CompProperties_MMTWall()
        {
            compClass = typeof(CompAbilityEffect_MMTWall);
            radius = 2.5f;
        }
    }

    /// <summary>石墙术：以施法者为锥顶，在指向目标点的扇形末端召唤一段弧形临时石墙（到期崩解）。</summary>
    public class CompAbilityEffect_MMTWall : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTWall Props => (CompProperties_MMTWall)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTWallUtil.RaiseWall(caster, target.Cell, Props.radius, Props.coneHalfAngleDegrees, new MMTWallParams
            {
                wallDef = Props.wallDefName.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(Props.wallDefName),
                durationTicks = Props.durationSeconds > 0f ? Props.durationSeconds.SecondsToTicks() : 0,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName)
            });
        }

        /// <summary>锥形预览：不画范围圈不高亮 pawn，画扇形 + 末端可建墙格。</summary>
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if (Props.radius <= 0f || !target.IsValid)
            {
                return;
            }
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return;
            }
            Color color = RingColor;
            color.a = 0.5f;
            MMTWallUtil.DrawWallPreview(caster, map, target.Cell, Props.radius, Props.coneHalfAngleDegrees, color);
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.radius <= 0f || !target.IsValid)
            {
                return null;
            }
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null)
            {
                return null;
            }
            int n = MMTWallUtil.CountValidCells(map, caster.Position, target.Cell, Props.radius, Props.coneHalfAngleDegrees);
            return "将召唤 " + n + " 段石墙";
        }
    }
}
