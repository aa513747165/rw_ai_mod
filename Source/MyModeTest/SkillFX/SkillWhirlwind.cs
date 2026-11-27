using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 近战旋风斩（第二十八步：俯视旋涡序列帧整帧平铺地面） ==========
    // 素材为 DNF 蓝色旋风卷 7 帧（近俯视水平椭圆构图，非侧视竖直素材），
    // 已按 alpha 包围盒居中归一化拼成横向图集（tools 一次性脚本处理，
    // 源 F:\...\legrollbluedodge.img\0~6.png）。渲染路线同第二十六步
    // SkillSideFlat（水平 quad + 材质 UV 偏移切帧），但允许绕竖直轴自转
    // （俯视旋涡构图旋转不穿帮，侧视素材才必须锁 0°）。
    // 行为：一次性挥斩——帧序列播一轮（约 1s），峰值帧（whirlHitFraction）
    // 对施法者周围 whirlDamageRadius 内敌对 pawn 结算一次切割伤害。

    /// <summary>旋风斩参数包（全部由 demo XML 条目透传，此处仅为兜底默认值）。</summary>
    public class MMTSkillWhirlwindParams
    {
        /// <summary>序列帧图集贴图（横向等宽排布，帧图自带颜色时 tint 用纯白）。</summary>
        public string texPath = "Things/Mote/MMT_SkillWhirlwind";
        /// <summary>图集帧数（等宽切片）。</summary>
        public int frameCount = 7;
        /// <summary>每帧停留 tick 数。</summary>
        public int frameTicks = 9;
        /// <summary>特效总时长秒数（到期自毁；缺省与 帧数×每帧tick 一致，播一轮）。</summary>
        public float durationSeconds = 1.05f;
        /// <summary>绘制宽/高（格；按图集单帧宽高比 252:191，宽 3.2 → 高 ≈2.43）。</summary>
        public float widthCells = 3.2f;
        public float heightCells = 2.43f;
        /// <summary>生命周期内绕竖直轴总自转角度（度，0=不转；俯视旋涡构图旋转不穿帮）。</summary>
        public float spinDegrees = 120f;
        /// <summary>伤害半径（格，以施法者为中心）。</summary>
        public float damageRadius = 1.5f;
        /// <summary>单次挥斩伤害（默认 Cut）。</summary>
        public float damage = 18f;
        public DamageDef damageDef;
        public float armorPenetration;
        /// <summary>伤害结算时机（时长进度 0~1，峰值帧附近）。</summary>
        public float hitFraction = 0.45f;
        /// <summary>染色（帧图自带颜色时保持纯白；alpha=0 视为未设置按纯白处理）。</summary>
        public Color tint = Color.white;
        /// <summary>首/尾淡入淡出秒数（0=生硬出现/消失；帧序列末两帧自带消散，fadeOut 给小值即可）。</summary>
        public float fadeInSeconds = 0.05f;
        public float fadeOutSeconds = 0.15f;
        public SoundDef startSound;
        /// <summary>施法者主手武器（伤害溯源用）。</summary>
        public ThingWithComps equipment;
    }

    /// <summary>
    /// 旋风斩特效实体：Attach 类无碰撞 Thing，存活期间在 MoteOverhead 高度
    /// 自绘水平 quad（帧图平铺 + 绕 Y 自转），跟随施法者位置，
    /// 进度到 hitFraction 时对周围敌对 pawn 结算一次伤害。
    /// 参数全为运行时传入不进存档；读档后参数丢失即自毁（特效最长约 1s，可接受）。
    /// 帧材质按 (贴图, 帧) 全局共享：每帧一个 Material，用 mainTextureOffset 切图集子矩形。
    /// </summary>
    public class Thing_MMTSkillWhirlwind : Thing
    {
        private Thing caster;
        private Faction casterFaction;
        private MMTSkillWhirlwindParams fx;
        private int durationTotal;
        private int ticksLeft;
        private bool hitDone;

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

        public void Setup(Thing caster, MMTSkillWhirlwindParams p)
        {
            this.caster = caster;
            casterFaction = caster?.Faction;
            fx = p;
            durationTotal = Mathf.Max(1, p.durationSeconds.SecondsToTicks());
            ticksLeft = durationTotal;
        }

        /// <summary>特效中心：施法者存活时跟随其绘制位置，否则钉在生成格。</summary>
        private Vector3 Center()
        {
            if (caster is Pawn cp && cp.Spawned && !cp.Dead)
            {
                return cp.DrawPos;
            }
            if (caster != null && caster.Spawned)
            {
                return caster.Position.ToVector3Shifted();
            }
            return Position.ToVector3Shifted();
        }

        protected override void Tick()
        {
            base.Tick();
            if (fx == null || Map == null)
            {
                Destroy();
                return;
            }
            int elapsed = durationTotal - ticksLeft;
            if (!hitDone && elapsed >= durationTotal * Mathf.Clamp01(fx.hitFraction))
            {
                hitDone = true;
                HitAround();
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Destroy();
            }
        }

        /// <summary>峰值帧一次性结算：伤害半径内敌对 pawn 吃一次切割（语义同新星环的敌方分支）。</summary>
        private void HitAround()
        {
            Map map = Map;
            if (fx.damage <= 0f || map == null)
            {
                return;
            }
            DamageDef damageDef = fx.damageDef ?? DamageDefOf.Cut;
            Vector3 center = Center();
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Dead || p == caster)
                {
                    continue;
                }
                Vector3 offset = p.Position.ToVector3Shifted() - center;
                offset.y = 0f;
                if (offset.magnitude > fx.damageRadius)
                {
                    continue;
                }
                if (p.Faction != null && p.Faction != casterFaction
                    && caster != null && p.HostileTo(caster))
                {
                    p.TakeDamage(new DamageInfo(damageDef, fx.damage, fx.armorPenetration,
                        -1f, caster.Destroyed ? null : caster, null, fx.equipment?.def));
                }
            }
        }

        // 颜色走 MaterialPropertyBlock 逐次传入：共享材质下同帧多次 DrawMesh 直接改
        // mat.color 会互相覆盖（同 SkillGroundFire 的处理）
        private static readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        /// <summary>自绘旋风序列帧：整帧平铺水平 quad 钉在施法者脚下，绕竖直轴匀速自转，
        /// 不循环播一轮，首尾淡入淡出。抬高面片照抄 DrawDisc 惯例（关实时阴影 +
        /// 沿相机前向 PerspectiveCompensate）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (fx == null || Map != Find.CurrentMap)
            {
                return;
            }
            int elapsed = durationTotal - ticksLeft;
            int ft = Mathf.Max(1, fx.frameTicks);
            int count = Mathf.Max(1, fx.frameCount);
            int frame = Mathf.Min(elapsed / ft, count - 1);
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
            float y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : new Vector3(0f, -1f, 0f);
            Vector3 drawPos = MMTBeamUtil.PerspectiveCompensate(Center(), y, camForward);
            Quaternion rot = Quaternion.AngleAxis(fx.spinDegrees * progress, Vector3.up);
            mpb.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(drawPos, rot, new Vector3(fx.widthCells, 1f, fx.heightCells)),
                mat, 0, null, 0, mpb, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        }
    }

    public static class MMTSkillWhirlwindUtil
    {
        public const string WhirlwindDefName = "MMT_SkillWhirlwind";

        /// <summary>在施法者脚下放一段旋风斩特效（峰值帧结算一次范围伤害）。</summary>
        public static Thing_MMTSkillWhirlwind Fire(Thing caster, MMTSkillWhirlwindParams p)
        {
            Map map = caster?.Map;
            if (map == null || p == null)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(WhirlwindDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + WhirlwindDefName + " 定义，旋风斩特效未生成");
                return null;
            }
            Thing_MMTSkillWhirlwind fx = (Thing_MMTSkillWhirlwind)GenSpawn.Spawn(def, caster.Position, map);
            fx.Setup(caster, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(caster.Position, map));
            }
            return fx;
        }
    }
}
