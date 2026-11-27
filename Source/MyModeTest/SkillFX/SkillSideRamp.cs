using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 多片坡道堆叠序列帧特效（第二十七步：龙卷风式侧视呈现，原版 Tornado 同款思路） ==========
    // 把侧视序列帧切成 N 条水平带，每带一片独立的水平 quad（plane10 + Quaternion.identity），
    // 沿世界 +Z（屏幕向北）坡道依次排开——龙卷风的"高度"就堆在 Z 轴上（世界 Y 只加微小抖动
    // 排绘制次序），这正是之前"水平切片按真实 Y 高度堆叠塌扁"失败的对照修正版。
    // 静止时 N 带无缝拼回整帧（与第二十六步单帧平躺一致），但每带独立：
    //   锥形收放（底宽顶窄）、横向摆动（越高摆越大、相位错开）、呼吸脉冲、顶部透明衰减，
    //   于是整帧有了体积感与运动层次（火舌舔动、漏斗扭动），这是单帧平躺做不到的。
    // 朝向仍锁死屏幕北（带内贴图不可绕 Y 旋转；摆动/收放是平移与缩放，不破坏侧视构图）。

    /// <summary>多片坡道堆叠参数包（全部由 demo XML 条目透传，此处仅为兜底默认值）。</summary>
    public class MMTSkillSideRampParams
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
        /// <summary>整体宽/高（格；宽=世界 X，高=世界 +Z 北；贴图底边中点锚在目标格中心）。</summary>
        public float widthCells = 2.2f;
        public float heightCells = 2.6f;
        /// <summary>生命周期内整体向北（屏幕上）漂移距离（格，模拟上升感，0=钉在原地）。</summary>
        public float driftCells = 0.6f;
        /// <summary>坡道切片数（≥2；每带深度=heightCells/slices，静止时无缝拼回整帧）。</summary>
        public int slices = 8;
        /// <summary>横向摆动最大幅度（格，底部不摆、随高度平方增大至顶部）。</summary>
        public float sway = 0.35f;
        /// <summary>摆动周期 tick（每带相位按序错开）。</summary>
        public float swayPeriodTicks = 40f;
        /// <summary>顶端宽度缩放系数（&lt;1 呈锥形收口，1=直筒）。</summary>
        public float topScale = 0.75f;
        /// <summary>顶端 alpha 系数（&lt;1 顶部淡出，伪造龙卷法式遮挡衰减）。</summary>
        public float topAlpha = 0.85f;
        /// <summary>呼吸脉冲幅度（0~1，各带相位错开的宽度起伏，0=关闭）。</summary>
        public float pulseAmp = 0.06f;
        /// <summary>染色（帧图自带颜色时保持纯白；alpha=0 视为未设置按纯白处理）。</summary>
        public Color tint = Color.white;
        /// <summary>首/尾淡入淡出秒数（0=生硬出现/消失）。</summary>
        public float fadeInSeconds = 0.15f;
        public float fadeOutSeconds = 0.4f;
        public SoundDef startSound;
    }

    /// <summary>
    /// 多片坡道堆叠实体：Attach 类无碰撞 Thing，存活期间在 MoteOverhead 高度自绘
    /// N 片水平带（旋转锁 Quaternion.identity，沿 +Z 坡道排开、锥形收放 + 摆动 + 脉冲）。
    /// 参数全为运行时传入不进存档；读档后参数丢失即自毁（特效最长数秒，可接受）。
    /// 带材质按 (贴图, 帧, 带) 全局共享：mainTextureScale/Offset 同时切图集列与横向条带。
    /// </summary>
    public class Thing_MMTSkillSideRamp : Thing
    {
        private MMTSkillSideRampParams fx;
        private int durationTotal;
        private int ticksLeft;
        /// <summary>每实例随机相位种子，多个特效并存时摆动/脉冲不同步。</summary>
        private float seed;

        // 带材质按 (贴图路径, 帧号, 带号, 带数) 全局共享
        private static readonly Dictionary<string, Material> sharedMats = new Dictionary<string, Material>();

        private static Material SharedBandMat(string texPath, int frame, int frameCount, int band, int slices)
        {
            string key = texPath + "#" + frame + "#b" + band + "of" + slices;
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
                    // U 切帧列、V 切横向条带（V=0 为贴图底边=地面，band 0 贴地）
                    mainTextureScale = new Vector2(1f / Mathf.Max(1, frameCount), 1f / Mathf.Max(2, slices)),
                    mainTextureOffset = new Vector2(
                        frame / (float)Mathf.Max(1, frameCount),
                        band / (float)Mathf.Max(2, slices))
                };
                sharedMats[key] = mat;
            }
            return mat;
        }

        public void Setup(MMTSkillSideRampParams p)
        {
            fx = p;
            durationTotal = Mathf.Max(1, p.durationSeconds.SecondsToTicks());
            ticksLeft = durationTotal;
            seed = Rand.Value * 20f;
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

        /// <summary>自绘 N 片坡道带：每带独立平移/缩放/透明度，静止时无缝拼回整帧。
        /// 世界 Y 逐带 +0.004 微抬（排绘制次序、防 z-fighting，不承担高度——高度全在 Z 轴，
        /// 原版 Tornado 同款）；抬高面片走 PerspectiveCompensate 并关实时阴影。</summary>
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
            int n = Mathf.Max(2, fx.slices);
            float age = elapsed / 60f;
            float duration = durationTotal / 60f;
            float progress = Mathf.Clamp01(age / Mathf.Max(0.05f, duration));
            Color baseColor = fx.tint.a <= 0f ? Color.white : fx.tint;
            if (fx.fadeInSeconds > 0f && age < fx.fadeInSeconds)
            {
                baseColor.a *= age / fx.fadeInSeconds;
            }
            float remain = duration - age;
            if (fx.fadeOutSeconds > 0f && remain < fx.fadeOutSeconds)
            {
                baseColor.a *= Mathf.Clamp01(remain / fx.fadeOutSeconds);
            }
            Vector3 basePos = Position.ToVector3Shifted();
            float baseAlt = AltitudeLayer.MoteOverhead.AltitudeFor();
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : new Vector3(0f, -1f, 0f);
            // 摆动相位（弧度）：周期读 XML，脉冲相位与摆动相位错开避免机械感
            float swayPhase = elapsed / Mathf.Max(1f, fx.swayPeriodTicks) * Mathf.PI * 2f;
            float bandDepth = fx.heightCells / n;
            for (int i = 0; i < n; i++)
            {
                Material mat = SharedBandMat(fx.texPath, frame, count, i, n);
                if (mat == null)
                {
                    continue;
                }
                float t = (i + 0.5f) / n;
                float pulse = 1f + fx.pulseAmp * Mathf.Sin(swayPhase * 2.7f + seed * 1.7f + i * 1.31f);
                float width = fx.widthCells * Mathf.Lerp(1f, fx.topScale, t) * pulse;
                // 锚点：贴图底边中点钉在目标格中心，带中心沿 +Z 排在 t×height + 漂移
                float z = basePos.z + t * fx.heightCells + fx.driftCells * progress;
                float x = basePos.x + fx.sway * t * t * Mathf.Sin(swayPhase + seed + i * 0.83f);
                float y = baseAlt + i * 0.004f;
                Color color = baseColor;
                color.a *= Mathf.Lerp(1f, fx.topAlpha, t);
                Vector3 drawPos = MMTBeamUtil.PerspectiveCompensate(new Vector3(x, y, z), y, camForward);
                mpb.SetColor(ShaderPropertyIDs.Color, color);
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(width, 1f, bandDepth)),
                    mat, 0, null, 0, mpb, UnityEngine.Rendering.ShadowCastingMode.Off, false);
            }
        }
    }

    public static class MMTSkillSideRampUtil
    {
        public const string SideRampDefName = "MMT_SkillSideRamp";

        /// <summary>在目标格放一段多片坡道堆叠序列帧特效（纯视觉，不造成伤害）。</summary>
        public static Thing_MMTSkillSideRamp Fire(Map map, IntVec3 cell, MMTSkillSideRampParams p)
        {
            if (map == null || p == null || !cell.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(SideRampDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + SideRampDefName + " 定义，坡道特效未生成");
                return null;
            }
            Thing_MMTSkillSideRamp fx = (Thing_MMTSkillSideRamp)GenSpawn.Spawn(def, cell, map);
            fx.Setup(p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(cell, map));
            }
            return fx;
        }
    }
}
