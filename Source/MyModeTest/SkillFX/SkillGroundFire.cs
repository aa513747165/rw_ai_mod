using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 技能地面火焰框架（第六步：单体烈焰喷发 + 范围烈焰火海） ==========
    // 地面技能：火焰从地面升起，留在原地持续燃烧（不跟随目标移动）。
    // 视觉：贴图火焰柱（Thing_MMTGroundFlame.DrawAt 自绘"贴地火盘 + 多层水平碟堆叠"——
    //   1.7 竖直 quad 不渲染，垂直火柱靠水平碟堆叠投影而成，随机朝向/相位 + 呼吸闪烁 +
    //   淡入淡出），不走原版 Fire 的 Graphic_Flicker，贴图可整体染色（groundTint）。
    // 效果：真实燃烧——按间隔对站上来的 pawn/建筑结算 Burn 伤害，概率点燃小人
    //   （FireUtility.TryAttachFire 身上挂原版火）与引燃格子（TryStartFireIn，
    //   引燃后走原版火焰蔓延），烧的是真火，视觉是我们的贴图。
    // 形态：MMTGroundFireUtil.FireGround 统一入口——areaRadius<=0 在目标点喷一根
    //   火柱（烈焰喷发）；areaRadius>0 半径内按密度抽格铺火海，自中心向外按距离
    //   延迟依次点燃（蔓延感），每柱独立存续。
    // pawn 技能侧出口：CompAbilityEffect_MMTGroundFire（范围预览圈走第一步 AoETarget 框架）；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（demoType="groundFire"）。
    // 注：火焰柱参数为运行时传入不进存档，读档后自毁（火焰最长数秒，可接受）。

    /// <summary>地面火焰参数包（MMTGroundFireUtil.FireGround / SpawnFlame 的输入）。</summary>
    public class MMTGroundFireParams
    {
        /// <summary>火焰贴图（白图，代码染色；纵轴=火焰向上方向）。</summary>
        public string texPath = "Things/Mote/MMT_FlamePillar";
        public Color tint = new Color(1f, 0.62f, 0.2f);
        /// <summary>序列帧贴图列表（非空时走序列帧模式：火焰按帧序列循环播放——
        /// 起燃→喷发峰值→旋卷→余烬消散，帧图本身已带颜色，tint 应传白色；
        /// 每柱帧相位按 seed 错开，火海成片时不同步）。</summary>
        public List<string> frameTexPaths;
        /// <summary>序列帧每帧停留 tick 数（frameLoop=true 时生效）。</summary>
        public int frameTicks = 7;
        /// <summary>true=帧序列循环播放到火焰熄灭；false=帧序列按单柱存活期摊平只播一遍
        /// （起燃→峰值→余烬随火焰寿命自然演进，帧相位不再按 seed 错开），停在末帧直到熄灭。</summary>
        public bool frameLoop = true;
        /// <summary>true=完全跳过自绘（伤害/点燃逻辑不变），用于视觉改由外部特效承担
        /// （如烈焰喷发叠加 sideRamp 坡道火柱后隐藏旧的俯视序列帧）。</summary>
        public bool hideVisual;
        /// <summary>单柱贴图尺寸（格，贴地火盘直径；堆叠层以此为基准收小）。</summary>
        public float visualScale = 1.7f;
        /// <summary>火柱高度（世界单位，水平碟堆叠总高——1.7 竖直 quad 不渲染，
        /// 严禁立起 plane10，垂直火柱只能靠水平碟堆叠，见 ChargingStation.DrawBeam 对照实验）。</summary>
        public float pillarHeight = 1.4f;
        /// <summary>火柱堆叠层数（≥2）。</summary>
        public int pillarLayers = 7;
        /// <summary>火柱顶端缩放系数（相对火盘，&lt;1 呈锥形收口）。</summary>
        public float pillarTopScale = 0.4f;
        /// <summary>火柱顶端横向摆动幅度（世界单位，底层不摆、越高摆越大）。</summary>
        public float pillarSway = 0.35f;
        /// <summary>单柱持续 tick 数。</summary>
        public int durationTicks = 360;
        /// <summary>伤害类型（null = Burn）。</summary>
        public DamageDef damageDef;
        /// <summary>每跳伤害。</summary>
        public float damage = 3f;
        /// <summary>跳伤间隔 tick。</summary>
        public int damageIntervalTicks = 15;
        public float armorPenetration;
        /// <summary>每跳点燃烧到的小人的概率（身上挂原版火）。</summary>
        public float ignitePawnChance = 0.2f;
        /// <summary>每跳引燃本格的概率（生成原版火焰，可蔓延）。</summary>
        public float igniteCellChance = 0.04f;
        /// <summary>单柱伤害半径（格，0 = 仅本格）。</summary>
        public float effectRadius;
        /// <summary>火海半径（格，0 = 单柱喷发）。</summary>
        public float areaRadius;
        /// <summary>火海覆盖率（每格抽中概率）。</summary>
        public float density = 0.9f;
        /// <summary>火海自中心向外铺开的延迟（tick/格）。</summary>
        public float spreadPerCellTicks = 2f;
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>地面火焰统一入口：单柱喷发 / 范围火海。</summary>
    public static class MMTGroundFireUtil
    {
        public const string FlameDefName = "MMT_GroundFlame";

        /// <summary>在指定格生成一根火焰柱（igniteDelayTicks &gt; 0 时先隐形待命，用于火海铺开）。</summary>
        public static Thing_MMTGroundFlame SpawnFlame(Thing caster, IntVec3 cell, MMTGroundFireParams p, int igniteDelayTicks = 0)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !cell.IsValid || !cell.InBounds(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(FlameDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + FlameDefName + " 定义，无法发动地面火焰");
                return null;
            }
            Thing_MMTGroundFlame flame = (Thing_MMTGroundFlame)GenSpawn.Spawn(def, cell, map);
            flame.Setup(caster, p, igniteDelayTicks);
            return flame;
        }

        /// <summary>统一入口：areaRadius&lt;=0 单柱喷发；&gt;0 半径内按密度铺火海，自中心向外延迟点燃。</summary>
        public static void FireGround(Thing caster, IntVec3 center, MMTGroundFireParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !center.IsValid || !center.InBounds(map))
            {
                return;
            }
            if (p.areaRadius <= 0f)
            {
                SpawnFlame(caster, center, p);
            }
            else
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, p.areaRadius, true))
                {
                    if (!cell.InBounds(map) || !Rand.Chance(p.density))
                    {
                        continue;
                    }
                    Building edifice = cell.GetEdifice(map);
                    if (edifice != null && edifice.def.fillPercent >= 1f)
                    {
                        continue;
                    }
                    int delay = Mathf.RoundToInt(center.DistanceTo(cell) * p.spreadPerCellTicks) + Rand.RangeInclusive(0, 6);
                    SpawnFlame(caster, cell, p, delay);
                }
            }
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(center, map));
            }
        }
    }

    /// <summary>
    /// 火焰柱实体：Attach 类无碰撞 Thing，存活期间自绘贴图火焰（双层四边形闪烁），
    /// 按间隔对影响格内的 pawn/建筑跳 Burn 伤害并可点燃。参数全为运行时传入不进存档；
    /// 读档后参数丢失即自毁（火焰最长数秒，可接受）。
    /// 性能：火焰材质按贴图全局共享（火海 70 柱不再各自 new Material）；
    /// 逐碟染色走 MaterialPropertyBlock 传入，不改共享材质的 color（同帧互覆坑）。
    /// </summary>
    public class Thing_MMTGroundFlame : Thing
    {
        private Thing caster;
        private MMTGroundFireParams flameParams;
        /// <summary>铺开延迟：&gt;0 时隐形待命、无伤害（火海自中心向外蔓延用）。</summary>
        private int igniteDelayTicks;
        private int durationTotal;
        private int ticksLeft;
        private int ticksUntilDamage;
        /// <summary>每柱随机动画相位/朝向，火海成片时避免齐刷刷同步闪烁。</summary>
        private float seed;
        private float baseAngle;

        // 火焰材质按贴图全局共享（同一火海全部柱子一个材质实例）
        private static readonly Dictionary<string, Material> sharedMats = new Dictionary<string, Material>();

        private static Material SharedMat(string texPath)
        {
            if (!sharedMats.TryGetValue(texPath, out Material mat) || mat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(texPath, false);
                if (tex == null)
                {
                    return null;
                }
                mat = new Material(ShaderDatabase.TransparentPostLight) { mainTexture = tex };
                sharedMats[texPath] = mat;
            }
            return mat;
        }

        public void Setup(Thing caster, MMTGroundFireParams p, int delayTicks)
        {
            this.caster = caster;
            flameParams = p;
            igniteDelayTicks = Mathf.Max(0, delayTicks);
            durationTotal = Mathf.Max(1, p.durationTicks);
            ticksLeft = durationTotal;
            ticksUntilDamage = 1;
            seed = Rand.Value * 20f;
            baseAngle = Rand.Range(0f, 360f);
        }

        protected override void Tick()
        {
            base.Tick();
            if (flameParams == null || Map == null)
            {
                Destroy();
                return;
            }
            if (igniteDelayTicks > 0)
            {
                igniteDelayTicks--;
                return;
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Destroy();
                return;
            }
            if (--ticksUntilDamage <= 0)
            {
                ticksUntilDamage = Mathf.Max(1, flameParams.damageIntervalTicks);
                DamagePulse();
            }
        }

        /// <summary>一跳伤害：本格（或 effectRadius 范围）内的 pawn/建筑吃 Burn，概率点燃。</summary>
        private void DamagePulse()
        {
            Map map = Map;
            DamageDef damageDef = flameParams.damageDef ?? DamageDefOf.Burn;
            foreach (Thing t in AffectedThings(map))
            {
                t.TakeDamage(new DamageInfo(damageDef, flameParams.damage, flameParams.armorPenetration,
                    0f, caster, null, flameParams.equipment?.def));
                if (flameParams.ignitePawnChance > 0f && t is Pawn pawn
                    && Rand.Chance(flameParams.ignitePawnChance) && FireUtility.CanEverAttachFire(pawn))
                {
                    FireUtility.TryAttachFire(pawn, 0.2f, caster);
                }
            }
            if (flameParams.igniteCellChance > 0f && Rand.Chance(flameParams.igniteCellChance))
            {
                FireUtility.TryStartFireIn(Position, map, 0.12f, caster);
            }
        }

        private IEnumerable<Thing> AffectedThings(Map map)
        {
            if (flameParams.effectRadius > 0f)
            {
                foreach (Thing t in GenRadial.RadialDistinctThingsAround(Position, map, flameParams.effectRadius, true))
                {
                    if (IsBurnableTarget(t))
                    {
                        yield return t;
                    }
                }
            }
            else
            {
                List<Thing> things = Position.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    if (IsBurnableTarget(things[i]))
                    {
                        yield return things[i];
                    }
                }
            }
        }

        private bool IsBurnableTarget(Thing t)
        {
            return t != this && t != caster && !t.Destroyed && (t is Pawn || t is Building);
        }

        /// <summary>自绘火焰：贴地火盘 + 多层水平碟堆叠成垂直火柱（贴地面、垂直向上喷发）。
        /// 1.7 竖直 quad 不渲染、严禁立起 plane10（ChargingStation.DrawBeam 对照实验），
        /// 故火柱 = 水平碟自地面逐层升高收小，俯视相机下高度差投成纵向；
        /// 抬高面片必须做透视补偿并关实时阴影（Building_Singularity.DrawPlane 封过的两个坑）。
        /// 逐层相位错开呼吸 + 顶部横向摆动 + 整体淡入淡出。
        /// frameTexPaths 非空时改走序列帧模式（见 DrawFrameSequence）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (flameParams == null || igniteDelayTicks > 0 || flameParams.hideVisual)
            {
                return;
            }
            int elapsed = durationTotal - ticksLeft;
            float alpha = Mathf.Clamp01(elapsed / 8f) * Mathf.Clamp01(ticksLeft / 30f);
            float flicker = 1f + 0.12f * Mathf.Sin(elapsed * 0.65f + seed) + 0.06f * Mathf.Sin(elapsed * 1.73f + seed * 1.7f);
            Color color = flameParams.tint;
            color.a *= alpha;
            float baseY = AltitudeLayer.Item.AltitudeFor() + 0.02f;
            float scale = flameParams.visualScale;
            Vector3 basePos = Position.ToVector3Shifted();
            Quaternion rot = Quaternion.AngleAxis(baseAngle, Vector3.up);
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : new Vector3(0f, -1f, 0f);
            if (flameParams.frameTexPaths != null && flameParams.frameTexPaths.Count > 0)
            {
                DrawFrameSequence(elapsed, basePos, baseY, scale, flicker, color, rot, camForward);
                return;
            }
            Material flameMat = SharedMat(flameParams.texPath);
            if (flameMat == null)
            {
                return;
            }
            // 贴地火盘（喷发口，平躺、比柱身略大）
            DrawDisc(basePos, baseY, scale * 1.05f * flicker, 1f, rot, flameMat, camForward, color);
            // 堆叠火柱：自火盘之上逐层升高收小，越高摆动越大
            int layers = Mathf.Max(2, flameParams.pillarLayers);
            for (int i = 0; i < layers; i++)
            {
                float t = (i + 1) / (float)layers;
                float y = baseY + 0.05f + t * flameParams.pillarHeight;
                float s = Mathf.Lerp(scale * 0.85f, scale * flameParams.pillarTopScale, t)
                    * (1f + 0.15f * Mathf.Sin(elapsed * 0.9f + seed + i * 1.1f));
                Vector3 p = basePos;
                p.x += flameParams.pillarSway * t * Mathf.Sin(elapsed * 0.5f + seed * 2.3f);
                DrawDisc(p, y, s, 1f, rot, flameMat, camForward, color);
            }
        }

        /// <summary>序列帧模式：帧图必须是**俯视**（正上方视角）的径向喷发贴图，
        /// 与原版火焰同思路——整帧平铺在目标格地面上（plane10 + Quaternion.identity）。
        /// frameLoop=true 按 frameTicks 循环播放、帧相位按 seed 错开（火海成片不同步）；
        /// frameLoop=false 把帧序列摊满单柱存活期只播一遍（起燃→峰值→余烬随寿命演进）。
        /// 侧视素材在 1.7/Waylith 下没有可靠的竖直呈现手段：竖直 quad 完全不渲染，
        /// 水平切片堆叠按真实高度画也在屏幕上塌成一条带（2026-11-27 四轮实验定论，
        /// 疑似 Waylith 对抬高面片的高度处理与预期不符）——故本模式只接受俯视帧图。
        /// 沿 DrawDisc 走 PerspectiveCompensate 贴地即可（贴地高度补偿近似恒等）。</summary>
        private void DrawFrameSequence(int elapsed, Vector3 basePos, float baseY, float scale, float flicker, Color color, Quaternion rot, Vector3 camForward)
        {
            List<string> frames = flameParams.frameTexPaths;
            int idx;
            if (flameParams.frameLoop)
            {
                int ft = Mathf.Max(1, flameParams.frameTicks);
                idx = ((elapsed / ft) + Mathf.RoundToInt(seed * 3.7f)) % frames.Count;
            }
            else
            {
                // 播一遍：帧序列摊满整个存活期（每帧 durationTotal/count tick），停在末帧直到熄灭
                idx = Mathf.Min((int)(elapsed * (long)frames.Count / Mathf.Max(1, durationTotal)), frames.Count - 1);
            }
            Material frameMat = SharedMat(frames[idx]);
            if (frameMat == null)
            {
                return;
            }
            DrawDisc(basePos, baseY + 0.05f, scale * flicker, 1f, rot, frameMat, camForward, color);
        }

        // 颜色走 MaterialPropertyBlock 逐次传入：共享材质下同帧多次 Graphics.DrawMesh
        // 直接改 mat.color 会互相覆盖（整屏色雾坑，见 AuraDisc/GroundZone 的处理）
        private static readonly MaterialPropertyBlock flameMpb = new MaterialPropertyBlock();

        /// <summary>画一片水平火焰碟：透视补偿 + 关阴影（抬高面片不投影、不出大黑影）。</summary>
        private static void DrawDisc(Vector3 center, float y, float size, float stretchZ, Quaternion rot, Material mat, Vector3 camForward, Color color)
        {
            flameMpb.SetColor(ShaderPropertyIDs.Color, color);
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(MMTBeamUtil.PerspectiveCompensate(center, y, camForward), rot, new Vector3(size, 1f, size * stretchZ)),
                mat, 0, null, 0, flameMpb, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        }
    }

    // ========== pawn 技能出口：地面火焰 ==========

    public class CompProperties_MMTGroundFire : CompProperties_MMTAoETargetBase
    {
        public string texPath = "Things/Mote/MMT_FlamePillar";
        public Color tint = new Color(1f, 0.62f, 0.2f);
        /// <summary>序列帧贴图列表（非空时走序列帧模式，帧图已带颜色、tint 传白色）。</summary>
        public List<string> frameTexPaths;
        /// <summary>序列帧每帧停留 tick 数（frameLoop=true 时生效）。</summary>
        public int frameTicks = 7;
        /// <summary>true=帧序列循环播放到火焰熄灭；false=帧序列按单柱存活期摊平只播一遍
        /// （起燃→峰值→余烬随火焰寿命自然演进，帧相位不再按 seed 错开），停在末帧直到熄灭。</summary>
        public bool frameLoop = true;
        /// <summary>true=完全跳过自绘（伤害/点燃逻辑不变），用于视觉改由外部特效承担
        /// （如烈焰喷发叠加 sideRamp 坡道火柱后隐藏旧的俯视序列帧）。</summary>
        public bool hideVisual;
        public float visualScale = 1.7f;
        /// <summary>火柱堆叠参数：总高（世界单位）/层数/顶端缩放系数（相对火盘）/顶端摆动幅度。</summary>
        public float pillarHeight = 1.4f;
        public int pillarLayers = 7;
        public float pillarTopScale = 0.4f;
        public float pillarSway = 0.35f;
        public int durationTicks = 360;
        public string damageDefName = "Burn";
        public float damage = 3f;
        public int damageIntervalTicks = 15;
        public float armorPenetration;
        public float ignitePawnChance = 0.2f;
        public float igniteCellChance = 0.04f;
        /// <summary>单柱伤害半径（0 = 仅本格）。</summary>
        public float effectRadius;
        /// <summary>火海半径（0 = 单柱喷发；缺省用预览圈半径 radius）。</summary>
        public float areaRadius;
        public float density = 0.9f;
        public float spreadPerCellTicks = 2f;
        public string soundDefName;

        public CompProperties_MMTGroundFire()
        {
            compClass = typeof(CompAbilityEffect_MMTGroundFire);
        }
    }

    /// <summary>地面火焰技能：确认目标后喷发火柱/铺开火海（radius&gt;0 时瞄准预览圈走 AoETarget 基类）。</summary>
    public class CompAbilityEffect_MMTGroundFire : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTGroundFire Props => (CompProperties_MMTGroundFire)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTGroundFireUtil.FireGround(caster, target.Cell, new MMTGroundFireParams
            {
                texPath = Props.texPath,
                tint = Props.tint,
                frameTexPaths = Props.frameTexPaths,
                frameTicks = Props.frameTicks,
                frameLoop = Props.frameLoop,
                hideVisual = Props.hideVisual,
                visualScale = Props.visualScale,
                pillarHeight = Props.pillarHeight,
                pillarLayers = Props.pillarLayers,
                pillarTopScale = Props.pillarTopScale,
                pillarSway = Props.pillarSway,
                durationTicks = Props.durationTicks,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                damageIntervalTicks = Props.damageIntervalTicks,
                armorPenetration = Props.armorPenetration,
                ignitePawnChance = Props.ignitePawnChance,
                igniteCellChance = Props.igniteCellChance,
                effectRadius = Props.effectRadius,
                areaRadius = Props.areaRadius > 0f ? Props.areaRadius : Props.radius,
                density = Props.density,
                spreadPerCellTicks = Props.spreadPerCellTicks,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
