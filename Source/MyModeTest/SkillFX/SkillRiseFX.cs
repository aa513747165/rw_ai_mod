using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 屏幕空间升起序列帧特效（第二十五步：侧视素材绕开竖直 quad 不渲染的路线） ==========
    // 1.7/Waylith 世界空间只渲染水平 quad，侧视"由地面往上"素材无竖直呈现手段
    // （立起 plane10 不渲染、水平切片堆叠塌扁，见 AGENTS.md 渲染硬约束）。
    // 本类改走 UI 层：复用 MoteText 同款 DrawGUIOverlay 屏幕空间通道，每帧把世界坐标
    // 投影成屏幕坐标，用 GUI.DrawTextureWithTexCoords 贴序列帧子矩形，上升感由屏幕空间逐帧
    // 上移实现。已知代价：画在所有世界物体之上（无遮挡关系）、不吃地图光照、迷雾手动判。

    /// <summary>屏幕空间升起特效参数包（全部由 demo XML 条目透传，此处仅为兜底默认值）。</summary>
    public class MMTSkillRiseFXParams
    {
        /// <summary>序列帧图集贴图（横向等宽排布，帧图自带颜色时 tint 用纯白）。</summary>
        public string texPath = "Things/Mote/MMT_SkillRiseFX_FlamePillar";
        /// <summary>图集帧数（等宽切片）。</summary>
        public int frameCount = 5;
        /// <summary>每帧停留 tick 数。</summary>
        public int frameTicks = 7;
        /// <summary>true=帧序列循环播放到时长结束；false=播完一轮停在末帧。</summary>
        public bool loop = true;
        /// <summary>特效总时长秒数（到期 mote 自动销毁）。</summary>
        public float durationSeconds = 2.2f;
        /// <summary>绘制宽/高（格，按相机投影逐帧换算成屏幕像素；贴图底边中点锚在目标格中心）。</summary>
        public float widthCells = 2.2f;
        public float heightCells = 2.6f;
        /// <summary>生命周期内整体上升距离（格，0=钉在原地）。</summary>
        public float riseCells = 0.6f;
        /// <summary>染色（帧图自带颜色时保持纯白；alpha=0 视为未设置按纯白处理）。</summary>
        public Color tint = Color.white;
        /// <summary>首/尾淡入淡出秒数（0=生硬出现/消失）。</summary>
        public float fadeInSeconds = 0.15f;
        public float fadeOutSeconds = 0.4f;
        public SoundDef startSound;
    }

    /// <summary>
    /// 屏幕空间序列帧 mote：世界空间故意不画任何东西，全部走 DrawGUIOverlay。
    /// 寿命借原版 mote 逻辑（solidTimeOverride），到期自动销毁，无需自管。
    /// </summary>
    public class MoteMMTSkillRiseFX : Mote
    {
        private MMTSkillRiseFXParams fx;
        private Texture2D sheet;

        /// <summary>生成后立刻注入参数（exactPosition 已在外层赋值）。</summary>
        public void Setup(MMTSkillRiseFXParams p)
        {
            fx = p;
            solidTimeOverride = p.durationSeconds;
            if (!p.texPath.NullOrEmpty())
            {
                sheet = ContentFinder<Texture2D>.Get(p.texPath, false);
            }
            if (sheet == null)
            {
                Log.Error("[MMT] 找不到升起特效贴图 " + p.texPath + "，本次特效不可见");
            }
        }

        /// <summary>世界空间留空：竖直 quad 在 1.7/Waylith 不渲染，本特效只活在 UI 层。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        public override void DrawGUIOverlay()
        {
            if (fx == null || sheet == null || Map == null || Map != Find.CurrentMap)
            {
                return;
            }
            if (Position.Fogged(Map))
            {
                return;
            }
            float age = AgeSecsPausable;
            float duration = Mathf.Max(0.05f, fx.durationSeconds);
            float progress = Mathf.Clamp01(age / duration);

            // 世界→屏幕投影：像素/格用投影目标点与其东侧 1 格点的屏幕间距实测，透视/缩放都自适应
            Camera cam = Find.Camera;
            Vector3 world = DrawPos;
            Vector3 sp0 = cam.WorldToScreenPoint(world);
            if (sp0.z <= 0f)
            {
                return;
            }
            Vector3 sp1 = cam.WorldToScreenPoint(world + Vector3.right);
            float pxPerCell = Mathf.Abs(sp1.x - sp0.x) / Prefs.UIScale;
            if (pxPerCell <= 0.01f)
            {
                return;
            }
            float x = sp0.x / Prefs.UIScale;
            float y = UI.screenHeight - sp0.y / Prefs.UIScale;
            float w = fx.widthCells * pxPerCell;
            float h = fx.heightCells * pxPerCell;
            // 底边中点锚在格中心，向上长；上升感 = 屏幕空间逐帧上移
            Rect rect = new Rect(x - w / 2f, y - h - fx.riseCells * pxPerCell * progress, w, h);

            int elapsedTicks = Mathf.FloorToInt(age * 60f);
            int frame = fx.loop
                ? elapsedTicks / Mathf.Max(1, fx.frameTicks) % fx.frameCount
                : Mathf.Min(elapsedTicks / Mathf.Max(1, fx.frameTicks), fx.frameCount - 1);
            Rect uv = new Rect(frame / (float)fx.frameCount, 0f, 1f / fx.frameCount, 1f);

            Color color = fx.tint.a <= 0f ? Color.white : fx.tint;
            if (fx.fadeInSeconds > 0f && age < fx.fadeInSeconds)
            {
                color.a *= age / fx.fadeInSeconds;
            }
            float remain = duration - age;
            if (fx.fadeOutSeconds > 0f && remain < fx.fadeOutSeconds)
            {
                color.a *= remain / fx.fadeOutSeconds;
            }
            GUI.color = color;
            GUI.DrawTextureWithTexCoords(rect, sheet, uv);
            GUI.color = Color.white;
        }
    }

    public static class MMTSkillRiseFXUtil
    {
        public const string MoteDefName = "MMT_Mote_SkillRiseFX";

        /// <summary>在目标格放一段屏幕空间升起序列帧特效（纯视觉，不造成伤害）。</summary>
        public static MoteMMTSkillRiseFX Fire(Map map, IntVec3 cell, MMTSkillRiseFXParams p)
        {
            if (map == null || p == null || !cell.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(MoteDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + MoteDefName + " 定义，升起特效未生成");
                return null;
            }
            MoteMMTSkillRiseFX mote = (MoteMMTSkillRiseFX)ThingMaker.MakeThing(def);
            mote.exactPosition = cell.ToVector3Shifted();
            mote.Setup(p);
            GenSpawn.Spawn(mote, cell, map);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(cell, map));
            }
            return mote;
        }
    }
}
