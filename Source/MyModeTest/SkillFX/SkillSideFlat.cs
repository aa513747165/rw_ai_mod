using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 侧视平躺序列帧特效（第二十六步：侧视素材第三条路线） ==========
    // 侧视序列帧整帧平铺在水平 quad（plane10 + Quaternion.identity）：
    // 1.7 主相机为 90° 正上方正交相机（BuildPlayScene 创建，逆向 Waylith 确认
    // 无任何逐 draw 剔除——竖直 quad 是投影退化为零面积线段，非被剔除），
    // 地面与屏幕 1:1 无失真映射，贴图"上"边朝世界 +Z（北）即屏幕上，
    // 故侧视素材平躺即呈现竖立侧视效果，无需任何预拉伸/补偿。
    // 与第二十五步屏幕空间版（SkillRiseFX）对照：本版画在世界空间，
    // 吃地图光照、遮挡按地面 AltitudeLayer 判定；代价是朝向锁死屏幕北
    // （绕 Y 旋转即穿帮），且按 Altitude 高于 pawn/建筑的部分会整片盖住它们。

    /// <summary>侧视平躺特效参数包（全部由 demo XML 条目透传，此处仅为兜底默认值）。</summary>
    public class MMTSkillSideFlatParams
    {
        /// <summary>序列帧图集贴图（横向等宽排布，侧视构图：底部贴地、顶部朝北，帧图自带颜色时 tint 用纯白）。</summary>
        public string texPath = "Things/Mote/MMT_SkillRiseFX_FlamePillar";
        /// <summary>图集帧数（等宽切片）。</summary>
        public int frameCount = 5;
        /// <summary>每帧停留 tick 数。</summary>
        public int frameTicks = 7;
        /// <summary>true=帧序列循环播放到时长结束；false=播完一轮停在末帧。</summary>
        public bool loop = true;
        /// <summary>特效总时长秒数（到期自毁）。</summary>
        public float durationSeconds = 2.2f;
        /// <summary>绘制宽/高（格；宽=世界 X，高=世界 +Z 北；贴图底边中点锚在目标格中心，可再由 offsetZCells 平移）。</summary>
        public float widthCells = 2.2f;
        public float heightCells = 2.6f;
        /// <summary>锚点沿世界 Z 的额外平移（格，负值=整体向南/屏幕下压）：帧图爆发中心不在底边时
        /// 用它把视觉中心拉回目标格（如金爆冲击环中心在帧底边上方 0.37 × 高 → offsetZ = -0.37 × 高）。</summary>
        public float offsetZCells;
        /// <summary>生命周期内整体向北（屏幕上）漂移距离（格，模拟上升感，0=钉在原地）。</summary>
        public float driftCells = 0.6f;
        /// <summary>染色（帧图自带颜色时保持纯白；alpha=0 视为未设置按纯白处理）。</summary>
        public Color tint = Color.white;
        /// <summary>首/尾淡入淡出秒数（0=生硬出现/消失）。</summary>
        public float fadeInSeconds = 0.15f;
        public float fadeOutSeconds = 0.4f;
        public SoundDef startSound;
    }

    /// <summary>
    /// 侧视平躺序列帧实体：Attach 类无碰撞 Thing，存活期间在 MoteOverhead 高度
    /// 自绘水平 quad（旋转锁 Quaternion.identity，帧图"上"边朝北=屏幕上）。
    /// 参数全为运行时传入不进存档；读档后参数丢失即自毁（特效最长数秒，可接受）。
    /// 帧材质按 (贴图, 帧) 全局共享：每帧一个 Material，用 mainTextureOffset 切图集子矩形。
    /// </summary>
    public class Thing_MMTSkillSideFlat : Thing
    {
        private MMTSkillSideFlatParams fx;
        private int durationTotal;
        private int ticksLeft;

        // 帧材质按 (贴图路径, 帧号) 全局共享
        private static readonly Dictionary<string, Material> sharedMats = new Dictionary<string, Material>();

        private static Material SharedFrameMat(string texPath, int frame, int frameCount)
        {
            string key = texPath + "#" + frame;
            if (!sharedMats.TryGetValue(key, out Material mat) || mat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(texPath, false);
                if (tex == null)
                {
                    return null;
                }
                mat = new Material(ShaderDatabase.TransparentPostLight)
                {
                    mainTexture = tex,
                    mainTextureScale = new Vector2(1f / Mathf.Max(1, frameCount), 1f),
                    mainTextureOffset = new Vector2(frame / (float)Mathf.Max(1, frameCount), 0f)
                };
                sharedMats[key] = mat;
            }
            return mat;
        }

        public void Setup(MMTSkillSideFlatParams p)
        {
            fx = p;
            durationTotal = Mathf.Max(1, p.durationSeconds.SecondsToTicks());
            ticksLeft = durationTotal;
        }

        protected override void Tick()
        {
            base.Tick();
            if (fx == null || Map == null)
            {
                Destroy();
                return;
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Destroy();
            }
        }

        // 颜色走 MaterialPropertyBlock 逐次传入：共享材质下同帧多次 DrawMesh 直接改
        // mat.color 会互相覆盖（同 SkillGroundFire 的处理）
        private static readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        /// <summary>自绘平躺序列帧：整帧平铺水平 quad，旋转锁 0°（贴图上边朝北=屏幕上），
        /// 底边中点锚在格中心（quad 中心沿 +Z 偏 height/2），整体向北漂移模拟上升，
        /// 首尾淡入淡出。正交俯视相机下 MoteOverhead 离地不高，PerspectiveCompensate 近似恒等，
        /// 照抄 DrawDisc 惯例（抬高面片关实时阴影）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (fx == null || Map != Find.CurrentMap)
            {
                return;
            }
            int elapsed = durationTotal - ticksLeft;
            int ft = Mathf.Max(1, fx.frameTicks);
            int count = Mathf.Max(1, fx.frameCount);
            int frame = fx.loop ? (elapsed / ft) % count : Mathf.Min(elapsed / ft, count - 1);
            Material mat = SharedFrameMat(fx.texPath, frame, count);
            if (mat == null)
            {
                return;
            }
            float age = elapsed / 60f;
            float duration = durationTotal / 60f;
            float progress = Mathf.Clamp01(age / Mathf.Max(0.05f, duration));
            Color color = fx.tint.a <= 0f ? Color.white : fx.tint;
            if (fx.fadeInSeconds > 0f && age < fx.fadeInSeconds)
            {
                color.a *= age / fx.fadeInSeconds;
            }
            float remain = duration - age;
            if (fx.fadeOutSeconds > 0f && remain < fx.fadeOutSeconds)
            {
                color.a *= Mathf.Clamp01(remain / fx.fadeOutSeconds);
            }
            // 锚点：贴图底边中点钉在目标格中心 → quad 中心沿 +Z（北）偏 height/2，再叠加锚点平移与漂移
            Vector3 pos = Position.ToVector3Shifted();
            pos.z += fx.heightCells * 0.5f + fx.offsetZCells + fx.driftCells * progress;
            float y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : new Vector3(0f, -1f, 0f);
            Vector3 drawPos = MMTBeamUtil.PerspectiveCompensate(pos, y, camForward);
            mpb.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(fx.widthCells, 1f, fx.heightCells)),
                mat, 0, null, 0, mpb, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        }
    }

    public static class MMTSkillSideFlatUtil
    {
        public const string SideFlatDefName = "MMT_SkillSideFlat";

        /// <summary>在目标格放一段侧视平躺序列帧特效（纯视觉，不造成伤害）。</summary>
        public static Thing_MMTSkillSideFlat Fire(Map map, IntVec3 cell, MMTSkillSideFlatParams p)
        {
            if (map == null || p == null || !cell.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(SideFlatDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + SideFlatDefName + " 定义，平躺特效未生成");
                return null;
            }
            Thing_MMTSkillSideFlat fx = (Thing_MMTSkillSideFlat)GenSpawn.Spawn(def, cell, map);
            fx.Setup(p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(cell, map));
            }
            return fx;
        }
    }
}
