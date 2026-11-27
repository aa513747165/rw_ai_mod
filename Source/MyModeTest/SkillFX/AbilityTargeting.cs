using LudeonTK;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MyModeTest
{
    // ========== 技能范围瞄准框架（第一步：圆形选择框 + 敌我区分） ==========
    // pawn 技能侧：CompAbilityEffect_MMTAoETargetBase 经原版 CompAbilityEffect.DrawEffectPreview 钩子
    //   在瞄准期间画出以鼠标格为圆心的范围圈，并按 targetFriendly 区分敌我配色与高亮；
    // 建筑侧：CompSkillDemo 用 BeginTargeting 的 highlightAction 复用同一绘制核心（见 MMTAoETargetPreview），
    //   后续技能特效统一基于本文件扩展。

    /// <summary>敌我配色：绿=友方/治疗，红=敌方/输出。</summary>
    public static class MMTAoETargetPreview
    {
        // 单通道亮度压到 0.6 档：满亮度（1.0）纯色细环会触发 RimIris 泛光（bloom ×3 + mip 金字塔 ×12），
        // 被大范围模糊核晕成整屏绿/红色雾——此前只降 alpha 无效就是因为泛光种子是 alpha=1 的描边环本身。
        public static readonly Color FriendlyRingColor = new Color(0.22f, 0.62f, 0.22f);
        public static readonly Color HostileRingColor = new Color(0.62f, 0.22f, 0.22f);
        /// <summary>最大射程环配色：中性白、低透明度（只是距离参考，不应抢范围圈的视觉焦点）。</summary>
        public static readonly Color MaxRangeRingColor = new Color(1f, 1f, 1f, 0.3f);

        /// <summary>射程环显示上限（格）：超出该距离的技能不再画射程环。
        /// 圆环贴图（RingTexPath，2048²）按 ≤45 格设计，再大的圈贴图被等比拉伸成粗带、只剩劣化观感。</summary>
        public const float MaxRangeRingRadius = 45f;

        /// <summary>圆形选择框贴图（白图，代码按敌我染色）。仅 SkillDelayedStrike 地面警示圈仍在用。</summary>
        public const string CircleTexPath = "Things/Mote/MMT_TargetCircle";

        /// <summary>瞄准预览圆环描边贴图（白环近外缘 + 淡填充，代码按敌我染色；双线性过滤天然抗锯齿，
        /// 要换高分辨率版本直接覆盖该文件即可）。注意：贴图环宽随半径等比缩放，大圈的环会等比偏粗。</summary>
        public const string RingTexPath = "Things/Mote/MMT_TargetRing";

        /// <summary>环内微填充相对环色的 alpha 倍率（双面渲染 alpha 会叠加一次，实际观感约为两倍）。</summary>
        private const float FillAlphaFactor = 0.08f;
        // 关键坑：同帧多次 Graphics.DrawMesh 共用同一材质时，颜色是引用语义——全部按最后一次
        // 设置的 color 渲染（白射程盘 + 红范围盘同帧 → 全部变成满 alpha 红盘，整屏染色）。
        // 必须逐次走 MaterialPropertyBlock；盘/环再分两个材质实例兜底。
        private static Material discMat;
        private static Material ringMat;
        private static MaterialPropertyBlock matProps;
        private static readonly Dictionary<int, Mesh> fillMeshCache = new Dictionary<int, Mesh>();

        public static Color RingColor(bool friendly)
        {
            return friendly ? FriendlyRingColor : HostileRingColor;
        }

        /// <summary>画平滑圆形选择框（贴图圆环描边 + 程序网格微填充）；比原版方格环平滑。</summary>
        public static void DrawCircle(IntVec3 center, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }
            Vector3 pos = center.ToVector3Shifted();
            pos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            Color fillColor = color;
            fillColor.a *= FillAlphaFactor;
            DrawDisc(pos, radius, fillColor);
            DrawRing(pos, radius, color);
        }

        /// <summary>画实心圆盘（程序网格 + ShaderDatabase.Transparent，alpha 确定生效；
        /// 持续特效别用贴图圆盘——白底贴图在加法着色器下整个 quad 发光，alpha 无效整屏染色）。</summary>
        public static void DrawDisc(Vector3 pos, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }
            EnsureMats();
            discMat.color = color;
            matProps.SetColor("_Color", color);
            Graphics.DrawMesh(GetDiscMesh(radius), pos, Quaternion.identity, discMat, 0, null, 0, matProps);
        }

        /// <summary>画圆环描边（贴图 quad 缩放到直径，双线性过滤平滑无锯齿；ShaderDatabase.Transparent
        /// 按贴图 alpha 混合，黑角与淡填充区都自然衰减；语义同 DrawCircle 的环部，供持续特效复用）。</summary>
        public static void DrawRing(Vector3 pos, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }
            EnsureMats();
            ringMat.color = color;
            matProps.SetColor("_Color", color);
            float size = radius * 2f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)), ringMat, 0, null, 0, matProps);
        }

        private static void EnsureMats()
        {
            if (discMat == null)
            {
                discMat = new Material(ShaderDatabase.Transparent) { mainTexture = BaseContent.WhiteTex };
            }
            if (ringMat == null)
            {
                Texture2D ringTex = ContentFinder<Texture2D>.Get(RingTexPath, false);
                // 贴图缺失时退化为白图（实心色环，至少不崩）
                ringMat = new Material(ShaderDatabase.Transparent) { mainTexture = ringTex != null ? ringTex : BaseContent.WhiteTex };
            }
            if (matProps == null)
            {
                matProps = new MaterialPropertyBlock();
            }
        }

        /// <summary>实心圆盘网格（环内微填充用，三角扇）。</summary>
        private static Mesh GetDiscMesh(float radius)
        {
            int key = Mathf.RoundToInt(radius * 20f);
            if (fillMeshCache.TryGetValue(key, out Mesh cached))
            {
                return cached;
            }
            float r = key / 20f;
            int segments = Mathf.Clamp(Mathf.RoundToInt(r * 10f), 32, 180);
            Vector3[] verts = new Vector3[segments + 1];
            // 双面索引，与圆环同理
            int[] tris = new int[segments * 6];
            verts[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float ang = i * Mathf.PI * 2f / segments;
                verts[i + 1] = new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
                int j = (i + 1) % segments;
                int t = i * 6;
                tris[t] = 0; tris[t + 1] = j + 1; tris[t + 2] = i + 1;
                tris[t + 3] = 0; tris[t + 4] = i + 1; tris[t + 5] = j + 1;
            }
            Mesh mesh = new Mesh { vertices = verts, triangles = tris };
            fillMeshCache[key] = mesh;
            return mesh;
        }

        // —— 每 tick 原始扫描缓存 ——
        // 瞄准预览每帧会调 2~4 次圈内判定（高亮 + 命中数提示 + 基类预览），原先每次各自
        // 做 1~2 次径向遍历；pawn 位置只随 tick 变化，故按 (tick, map, center, radius) 缓存
        // 一次合并扫描（pawn 与带标记建筑同一趟收集），帧内/帧间同 tick 全部复用。
        private static readonly List<Pawn> scanPawns = new List<Pawn>();
        private static readonly List<Thing> scanBuildings = new List<Thing>();
        private static int scanTick = -1;
        private static Map scanMap;
        private static IntVec3 scanCenter;
        private static float scanRadius = -1f;

        private static void EnsureScan(Map map, IntVec3 center, float radius)
        {
            int tick = Find.TickManager.TicksGame;
            if (scanTick == tick && scanMap == map && scanCenter == center && scanRadius == radius)
            {
                return;
            }
            scanTick = tick;
            scanMap = map;
            scanCenter = center;
            scanRadius = radius;
            scanPawns.Clear();
            scanBuildings.Clear();
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(center, map, radius, true))
            {
                if (thing.Destroyed)
                {
                    continue;
                }
                if (thing is Pawn p)
                {
                    if (!p.Dead)
                    {
                        scanPawns.Add(p);
                    }
                }
                else if (thing.Faction != null && thing.TryGetComp<CompAoETargetable>() != null)
                {
                    scanBuildings.Add(thing);
                }
            }
        }

        /// <summary>原始扫描的 pawn 全集（不过滤敌我）；内部复用列表，勿缓存勿修改，仅供自定义筛选（如 friendlyFire 语义）。</summary>
        public static IReadOnlyList<Pawn> RawPawnsInRadius(Map map, IntVec3 center, float radius)
        {
            EnsureScan(map, center, radius);
            return scanPawns;
        }

        /// <summary>圈内 pawn 敌我判定（语义与 MMTAbilityUtil.PawnsInRadius 一致，走每 tick 缓存扫描）。</summary>
        public static IEnumerable<Pawn> PawnsInRadiusCached(Thing casterCtx, IntVec3 center, Map map, float radius, bool friendly, bool includeSelf)
        {
            EnsureScan(map, center, radius);
            foreach (Pawn p in scanPawns)
            {
                if (!includeSelf && p == casterCtx)
                {
                    continue;
                }
                if (friendly)
                {
                    if (p.Faction == casterCtx.Faction || (p.Faction == null && p.RaceProps.Animal && casterCtx.Faction != null && p.HostFaction == null))
                    {
                        yield return p;
                    }
                }
                else if (p.Faction != null && p.Faction != casterCtx.Faction && p.HostileTo(casterCtx))
                {
                    yield return p;
                }
            }
        }

        /// <summary>
        /// 圈内敌我判定：pawn 判定语义同 MMTAbilityUtil.PawnsInRadius，
        /// 建筑/物品须带 CompAoETargetable 标记才参与判定（按各自派系分敌我）。
        /// </summary>
        public static IEnumerable<Thing> TargetsInRadius(Thing casterCtx, IntVec3 center, Map map, float radius, bool friendly, bool includeSelf)
        {
            foreach (Pawn p in PawnsInRadiusCached(casterCtx, center, map, radius, friendly, includeSelf))
            {
                yield return p;
            }
            EnsureScan(map, center, radius);
            foreach (Thing thing in scanBuildings)
            {
                if (!includeSelf && thing == casterCtx)
                {
                    continue;
                }
                if (friendly)
                {
                    if (thing.Faction == casterCtx.Faction)
                    {
                        yield return thing;
                    }
                }
                else if (thing.HostileTo(casterCtx))
                {
                    yield return thing;
                }
            }
        }

        /// <summary>画范围圈并高亮圈内受影响目标（目标集合与施法结算走同一入口）。</summary>
        public static void Draw(Thing casterCtx, IntVec3 center, Map map, float radius, bool friendly, bool includeSelf)
        {
            if (map == null || map != Find.CurrentMap || radius <= 0f)
            {
                return;
            }
            DrawCircle(center, radius, RingColor(friendly));
            foreach (Thing t in TargetsInRadius(casterCtx, center, map, radius, friendly, includeSelf))
            {
                GenDraw.DrawTargetHighlightWithLayer(t.Position.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
            }
        }

        /// <summary>鼠标旁命中数提示。</summary>
        public static string AffectedLabel(Thing casterCtx, IntVec3 center, Map map, float radius, bool friendly, bool includeSelf)
        {
            int count = TargetsInRadius(casterCtx, center, map, radius, friendly, includeSelf).Count();
            return "将影响 " + count + (friendly ? " 名友方" : " 名敌方");
        }
    }

    // ========== 可圈选标记 comp（建筑/物品参与技能范围敌我判定） ==========

    public class CompProperties_MMTAoETargetable : CompProperties
    {
        public CompProperties_MMTAoETargetable()
        {
            compClass = typeof(CompAoETargetable);
        }
    }

    /// <summary>空标记：此 Thing 参与技能范围敌我判定（如沙袋人偶）；pawn 天生参与无需标记。</summary>
    public class CompAoETargetable : ThingComp
    {
    }

    // ========== pawn 技能框架基类 ==========

    public class CompProperties_MMTAoETargetBase : CompProperties_AbilityEffect
    {
        /// <summary>范围半径；0=单体技能不画圈。</summary>
        public float radius;
        /// <summary>true=作用于友军（绿圈），false=作用于敌对（红圈）。</summary>
        public bool targetFriendly = true;
        public bool includeSelf = true;

        public CompProperties_MMTAoETargetBase()
        {
            compClass = typeof(CompAbilityEffect_MMTAoETargetBase);
        }
    }

    /// <summary>
    /// 范围技能基类：瞄准时画鼠标跟随范围圈 + 高亮受影响 pawn + 鼠标旁命中数提示。
    /// 后续范围技能 comp 继承本类，只写 Apply 即可自动获得瞄准预览。
    /// </summary>
    public class CompAbilityEffect_MMTAoETargetBase : CompAbilityEffect
    {
        public new CompProperties_MMTAoETargetBase Props => (CompProperties_MMTAoETargetBase)props;

        public virtual Color RingColor => MMTAoETargetPreview.RingColor(Props.targetFriendly);

        /// <summary>默认只圈选 pawn（治疗/伤害不应选中建筑）；需要选建筑的技能 override 换 TargetsInRadius。</summary>
        public virtual IEnumerable<Pawn> AffectedPawns(LocalTargetInfo target)
        {
            Pawn caster = parent.pawn;
            Map map = caster.MapHeld;
            if (map == null || Props.radius <= 0f)
            {
                yield break;
            }
            foreach (Pawn p in MMTAoETargetPreview.PawnsInRadiusCached(caster, target.Cell, map, Props.radius, Props.targetFriendly, Props.includeSelf))
            {
                yield return p;
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
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
            MMTAoETargetPreview.DrawCircle(target.Cell, Props.radius, RingColor);
            foreach (Pawn p in AffectedPawns(target))
            {
                GenDraw.DrawTargetHighlightWithLayer(p.Position.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
            }
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.radius <= 0f || !target.IsValid)
            {
                return null;
            }
            int count = AffectedPawns(target).Count();
            return "将影响 " + count + (Props.targetFriendly ? " 名友方" : " 名敌方");
        }
    }

    // ========== 建筑演示技能 comp（沙袋人偶，仅 Dev 模式） ==========

    /// <summary>演示技能条目：后续技能演示在 XML 加条目即可。</summary>
    public class MMTSkillDemoEntry
    {
        public string label;
        public string iconPath;
        public float radius = 5f;
        /// <summary>true=圈选友方（绿），false=圈选敌方（红）。</summary>
        public bool targetFriendly;
        public bool includeSelf = true;
        public float range = 24.9f;
        /// <summary>false = 瞄准时不画技能距离白环（以自身为起点、只需选方向的技能，如龙息，白环无参考意义）。</summary>
        public bool showRangeRing = true;

        // —— 射击类演示（第三步） ——
        /// <summary>演示类型：""/"preview"=范围圈纯提示；"singleShot"=单发射击；"funnelShot"=浮游齐射；"beam"=持续射线；"strike"=天降打击；"groundFire"=地面火焰；"aura"=自身光环（免瞄准）；"bufffx"=增益/减益特效；"blink"=折跃；"massTeleport"=群体传送（两段瞄准）；"charge"=冲锋；"jump"=跳跃；"summonOrb"=雷球；"summonHoming"=追踪弹；"summonAnimal"=动物召唤；"summonTurret"=炮塔召唤；"delayedStrike"=延时打击（陨石）；"knockback"=近战击退；"chainLightning"=连锁闪电；"breathCone"=锥形吐息（龙息）；"heal"=治疗；"wall"=造墙（石墙术）；"groundZone"=持续地面区域（毒气池/寒风领域）；"mine"=触发式爆炸陷阱（地雷）；"nova"=新星环（360° 扩散冲击波，自身中心免瞄准）；"dispel"=驱散/净化（单体锁定，移除限时状态）；"riseFX"=屏幕空间升起序列帧（侧视素材走 UI 层贴图，纯视觉演示）；"sideFlat"=侧视平躺序列帧（整帧平铺水平 quad，贴图上边朝北=屏幕上，纯视觉演示）。</summary>
        public string demoType;
        /// <summary>true = 瞄准时无视视线（抛物线类技能可跨墙选点）。</summary>
        public bool ignoreLineOfSight;
        /// <summary>确认目标后的读条秒数（0=瞬发无读条），读条圆画在建筑正上方。</summary>
        public float castSeconds;
        public string projectileDefName;
        public int funnelCount = 5;
        public float ringRadius = 1.8f;
        public int aimTicks = 15;
        public string soundDefName;
        /// <summary>浮游单元图形（默认悬浮火球）。</summary>
        public string funnelFleckDefName = "MMT_Fleck_FireballCharge";
        /// <summary>读条圆配色，alpha=0 时用默认法力蓝。</summary>
        public Color castBarColor = new Color(0f, 0f, 0f, 0f);

        // —— 持续射线演示（第四步） ——
        /// <summary>射线持续秒数。</summary>
        public float beamDurationSeconds = 4f;
        /// <summary>射线最大射程（格，撞墙提前截止）。</summary>
        public float beamRange = 20f;
        /// <summary>光柱宽度（格）。</summary>
        public float beamWidth = 0.35f;
        /// <summary>光柱贴图（默认 MMT 占位光柱）。</summary>
        public string beamTexPath = "Things/Mote/MMT_SkillBeam";
        /// <summary>光柱染色。</summary>
        public Color beamTint = new Color(1f, 0.55f, 0.15f);
        /// <summary>每跳伤害（默认 Burn）。</summary>
        /// <summary>射线出膛偏移（格，避免光柱起点与施法者重叠）。</summary>
        public float beamMuzzleOffset = 0.9f;
        /// <summary>射线发射头/命中端贴图（空 = 不画）与绘制尺寸（格）。</summary>
        public string beamHeadTexPath = "Things/Mote/MMT_SkillBeam_Head";
        public string beamTailTexPath = "Things/Mote/MMT_SkillBeam_Tail";
        public float beamHeadSize = 1.4f;
        /// <summary>true = 发射头画成地面喷发火柱（多层水平碟堆叠，贴地、垂直向上）；false = 平躺光球。</summary>
        public bool beamHeadVertical;
        /// <summary>火柱高度（世界单位）/碟片层数/顶端缩放系数（相对底部）。</summary>
        public float beamHeadPillarHeight = 1.1f;
        public int beamHeadPillarLayers = 6;
        public float beamHeadPillarTopScale = 0.55f;
        public float beamTailSize = 1.6f;
        public float beamDamage = 4f;
        /// <summary>跳伤间隔 tick。</summary>
        public int beamDamageIntervalTicks = 10;
        /// <summary>每跳点燃经过格子的概率。</summary>
        public float beamIgniteChance = 0.1f;

        // —— 天降打击演示（第五步） ——
        /// <summary>落雷次数（1 = 单体落雷；>1 = 雷暴连发）。</summary>
        public int strikeCount = 1;
        /// <summary>雷暴连发间隔 tick。</summary>
        public int strikeIntervalTicks = 12;
        /// <summary>雷暴随机落点半径（0 = 雷暴时用预览圈半径 radius）。</summary>
        public float strikeAreaRadius;
        /// <summary>首次落雷前延迟 tick。</summary>
        public int strikeInitialDelayTicks;
        /// <summary>附加自定义爆炸伤害（0 = 仅原版落雷爆炸）。</summary>
        public float strikeDamage;
        public float strikeArmorPenetration;
        public float strikeExplosionRadius = 1.9f;
        public float strikeIgniteChance;
        /// <summary>true=落雷必须点选目标（pawn/建筑），雷击落在目标身上而非鼠标格；敌我限定走 targetFriendly。</summary>
        public bool strikeRequireTarget;

        // —— 地面火焰演示（第六步） ——
        /// <summary>火焰贴图（白图，代码染色）。</summary>
        public string groundTexPath = "Things/Mote/MMT_FlamePillar";
        public Color groundTint = new Color(1f, 0.62f, 0.2f);
        /// <summary>序列帧贴图列表（非空时火焰柱走序列帧模式，帧图已带颜色、groundTint 传白色）。</summary>
        public List<string> groundFrameTexPaths;
        /// <summary>序列帧每帧停留 tick 数。</summary>
        public int groundFrameTicks = 7;
        /// <summary>单柱贴图尺寸（格，贴地火盘直径）。</summary>
        public float groundVisualScale = 1.7f;
        /// <summary>火柱堆叠参数：总高（世界单位）/层数/顶端缩放系数（相对火盘）/顶端摆动幅度。</summary>
        public float groundPillarHeight = 1.4f;
        public int groundPillarLayers = 7;
        public float groundPillarTopScale = 0.4f;
        public float groundPillarSway = 0.35f;
        /// <summary>单柱持续秒数。</summary>
        public float groundDurationSeconds = 6f;
        public float groundDamage = 3f;
        public int groundDamageIntervalTicks = 15;
        public float groundArmorPenetration;
        /// <summary>每跳点燃小人的概率。</summary>
        public float groundIgnitePawnChance = 0.2f;
        /// <summary>每跳引燃格子的概率（原版火焰，可蔓延）。</summary>
        public float groundIgniteCellChance = 0.04f;
        /// <summary>单柱伤害半径（0=仅本格）。</summary>
        public float groundEffectRadius;
        /// <summary>火海半径（0=单柱喷发；缺省用预览圈半径 radius）。</summary>
        public float groundAreaRadius;
        /// <summary>火海覆盖率。</summary>
        public float groundDensity = 0.9f;
        /// <summary>火海铺开延迟（tick/格）。</summary>
        public float groundSpreadPerCellTicks = 2f;
        /// <summary>true=必须点选目标（pawn/建筑），火柱从目标脚下喷起；敌我限定走 targetFriendly。</summary>
        public bool groundRequireTarget;

        // —— 余烬爆发演示（第二十四步：爆发 + 短余烬 hybrid，半径用 radius） ——
        /// <summary>火星粒子数量。</summary>
        public int emberCount = 48;
        /// <summary>火星初速范围（格/秒）。</summary>
        public float emberSparkSpeedMin = 2f;
        public float emberSparkSpeedMax = 6f;
        /// <summary>火星染色两档（亮红橙 / 暗红）。</summary>
        public Color emberSparkTint = new Color(1f, 0.25f, 0.08f);
        public Color emberSparkTintDark = new Color(0.55f, 0.05f, 0.02f);
        /// <summary>爆发瞬间即时伤害与穿甲。</summary>
        public float emberBurstDamage = 8f;
        public float emberBurstArmorPenetration;
        /// <summary>爆发瞬间点燃 pawn 概率。</summary>
        public float emberBurstIgnitePawnChance = 0.25f;
        /// <summary>余烬持续秒数（0=纯爆发不留余烬）。</summary>
        public float emberResidDurationSeconds = 2.5f;
        /// <summary>余烬每跳伤害与跳伤间隔 tick。</summary>
        public float emberResidDamage = 2f;
        public int emberResidDamageIntervalTicks = 15;
        /// <summary>余烬溅落密度（每格抽中概率）。</summary>
        public float emberResidDensity = 0.7f;
        /// <summary>余烬染色（默认暗红）。</summary>
        public Color emberResidTint = new Color(0.85f, 0.15f, 0.05f);
        /// <summary>余烬每跳引燃格子概率（原版火焰，可蔓延）。</summary>
        public float emberResidIgniteCellChance = 0.02f;

        // —— 光环演示（第七步） ——
        /// <summary>true=原版范围护盾（临时护盾投影器），false=增益/减益光圈；半径用 radius。</summary>
        public bool auraShieldMode;
        /// <summary>光环持续秒数。</summary>
        public float auraDurationSeconds = 12f;
        /// <summary>盾类：护盾命中点。</summary>
        public int auraShieldHitPoints = 200;
        /// <summary>增益/减益类：脉冲施加的 Hediff（空=纯光圈提示）。</summary>
        public string auraBuffDefName;
        /// <summary>增益/减益脉冲间隔 tick。</summary>
        public int auraRefreshIntervalTicks = 60;
        /// <summary>光圈贴图（白图，代码染色）。</summary>
        public string auraTexPath = "Things/Mote/MMT_AuraDisc";
        /// <summary>光圈染色，alpha=0 时按敌我自动配色。</summary>
        public Color auraTint = new Color(0f, 0f, 0f, 0f);

        // —— 增益/减益特效演示（第八步） ——
        /// <summary>施加的增益/减益 Hediff（空=纯特效提示）。</summary>
        public string buffHediffDefName;
        /// <summary>目标头顶小图标贴图。</summary>
        public string buffIconTexPath = "UI/Icons/MoveSpeedBonus";
        /// <summary>图标染色，alpha=0 时按敌我自动配色。</summary>
        public Color buffIconTint = new Color(0f, 0f, 0f, 0f);
        /// <summary>图标显示秒数。</summary>
        public float buffIconSeconds = 1.5f;
        /// <summary>身上粒子 fleck。</summary>
        public string buffParticleFleckDefName = "MMT_Fleck_BuffSpark";
        /// <summary>粒子染色，alpha=0 时跟随图标染色。</summary>
        public Color buffParticleTint = new Color(0f, 0f, 0f, 0f);
        /// <summary>true=必须点选目标（单体增益/减益）；false + radius=范围圈选。</summary>
        public bool buffRequireTarget;

        // —— 位移演示（第九步） ——
        /// <summary>起点闪光 fleck（折跃/传送/冲锋/跳跃起跳时播放）。</summary>
        public string displaceDepartFleckDefName = "MMT_Fleck_BlinkFlash";
        /// <summary>落点闪光 fleck。</summary>
        public string displaceArriveFleckDefName = "MMT_Fleck_BlinkFlash";
        /// <summary>冲锋拖尾 fleck。</summary>
        public string displaceTrailFleckDefName = "MMT_Fleck_DashTrail";
        /// <summary>拖尾粒子间隔 tick。</summary>
        public int displaceTrailIntervalTicks = 3;
        /// <summary>冲锋：每格耗时 tick（越小越快）。</summary>
        public float chargeTicksPerCell = 2f;
        /// <summary>冲锋最短耗时 tick。</summary>
        public int chargeMinTicks = 6;
        /// <summary>跳跃（建筑）：每格耗时 tick。</summary>
        public float jumpTicksPerCell = 3f;
        /// <summary>跳跃（建筑）最短耗时 tick。</summary>
        public int jumpMinTicks = 25;
        /// <summary>跳跃（建筑）抛物线顶点高度（格）。</summary>
        public float jumpPeakHeight = 3f;

        // —— 召唤演示（第十步） ——
        /// <summary>雷球贴图（白图，代码染色）与染色。</summary>
        public string summonOrbTexPath = "Things/Mote/MMT_SummonOrb";
        public Color summonOrbTint = new Color(0.55f, 0.75f, 1f);
        /// <summary>雷球：每格耗时 tick（越大越慢）。</summary>
        public float summonOrbTicksPerCell = 15f;
        /// <summary>追踪弹贴图（朝北）与染色。</summary>
        public string summonHomingTexPath = "Things/Mote/MMT_HomingBolt";
        public Color summonHomingTint = new Color(1f, 0.75f, 0.35f);
        /// <summary>追踪弹：每格耗时 tick（越小越快）。</summary>
        public float summonHomingTicksPerCell = 5f;
        /// <summary>脉冲/命中伤害类型（空=Flame）。</summary>
        public string summonDamageDefName;
        public float summonDamage = 8f;
        public float summonArmorPenetration;
        /// <summary>雷球：脉冲伤害半径。</summary>
        public float summonPulseRadius = 2.5f;
        public int summonPulseIntervalTicks = 20;
        /// <summary>追踪弹：续航秒数（超时消失）。</summary>
        public float summonHomingDurationSeconds = 8f;
        /// <summary>追踪弹：命中爆炸半径（0=单体直击）。</summary>
        public float summonHomingExplosionRadius;
        /// <summary>动物召唤：动物种类与持续秒数。</summary>
        public string summonAnimalKindDefName = "Husky";
        public float summonAnimalDurationSeconds = 20f;
        /// <summary>炮塔召唤：炮塔建筑与持续秒数。</summary>
        public string summonTurretDefName = "MMT_SummonTurret";
        public float summonTurretDurationSeconds = 40f;

        // —— 延时打击演示（第十一步：陨石） ——
        /// <summary>预警圈持续秒数（之后陨石坠落；预警圈半径用 radius）。</summary>
        public float meteorDelaySeconds = 2f;
        /// <summary>陨石坠落耗时 tick。</summary>
        public int meteorFallTicks = 25;
        public float meteorDamage = 40f;
        public float meteorArmorPenetration;
        /// <summary>陨石爆炸半径。</summary>
        public float meteorExplosionRadius = 3f;
        public float meteorIgniteChance = 0.3f;
        /// <summary>陨石绘制尺寸（格，建议 ≈ 爆炸半径 × 1.2~1.5）。</summary>
        public float meteorDrawSize = 4f;
        /// <summary>陨石自旋速度（度/tick，0 = 坠落不旋转）。</summary>
        public float meteorSpinDegPerTick;
        /// <summary>坠落缩放区间（1→1 = 恒定大小）。</summary>
        public float meteorGrowFrom = 1f;
        public float meteorGrowTo = 1f;

        // —— 近战击退演示（第十二步） ——
        public float knockbackDamage = 12f;
        /// <summary>击退格数。</summary>
        public int knockbackCells = 4;
        /// <summary>撞墙追加伤害。</summary>
        public float knockbackWallSlamDamage = 8f;

        // —— 连锁闪电演示（第十三步） ——
        /// <summary>额外弹跳次数。</summary>
        public int chainBounceCount = 4;
        /// <summary>每次弹跳最大距离（格）。</summary>
        public float chainBounceRange = 5f;
        public float chainDamage = 15f;
        /// <summary>每跳伤害衰减系数。</summary>
        public float chainDamageFalloff = 0.75f;

        // —— 锥形吐息演示（第十五步：龙息） ——
        /// <summary>吐息射程（格，range 字段为瞄准距离上限）。</summary>
        public float breathRange = 8f;
        /// <summary>锥形半角（度）。</summary>
        public float breathHalfAngleDegrees = 30f;
        public float breathDurationSeconds = 3f;
        public float breathDamage = 3f;
        public int breathDamageIntervalTicks = 10;
        public float breathIgniteChance = 0.2f;

        // —— 治疗演示（第十七步） ——
        /// <summary>每个目标的治疗量池。</summary>
        public float healAmount = 15f;

        // —— 持续地面区域演示（第十九步：毒气池） ——
        /// <summary>区域持续秒数（半径用 radius）。</summary>
        public float zoneDurationSeconds = 20f;
        /// <summary>效果脉冲间隔 tick。</summary>
        public int zoneIntervalTicks = 30;
        /// <summary>true=不分敌我（真实毒气），false=仅敌方。</summary>
        public bool zoneAffectEveryone = true;
        /// <summary>每跳直接伤害（0=仅 Hediff 累积）。</summary>
        public float zoneDamage;
        public string zoneDamageDefName;
        /// <summary>每跳累积的 Hediff（默认原版 ToxicBuildup 毒素积累；寒风领域等纯定身皮肤显式置空）。</summary>
        public string zoneHediffDefName = "ToxicBuildup";
        public float zoneHediffSeverity = 0.04f;
        /// <summary>每跳踉跄定身 tick（0=不定身；寒风领域冰冻硬控用，略大于脉冲间隔保持连续冻结）。</summary>
        public int zoneStaggerTicks;
        /// <summary>冰冻皮肤：冰块覆盖物贴图尺寸（格，略大于 pawn 身位即罩住全身）。</summary>
        public float zoneIceScale = 1.7f;
        /// <summary>区域圆盘贴图与染色。</summary>
        public string zoneTexPath = "Things/Mote/MMT_AuraDisc";
        public Color zoneTint = new Color(0.55f, 0.85f, 0.3f);

        // —— 触发式陷阱演示（第二十步：爆炸地雷） ——
        /// <summary>布设后的武装时间秒数（期间不触发）。</summary>
        public float mineArmSeconds = 1f;
        /// <summary>触发半径（格，敌人进入即引爆）。</summary>
        public float mineTriggerRadius = 2f;
        /// <summary>爆炸半径（格；预览圈用 radius，演示中两者设为一致）。</summary>
        public float mineExplosionRadius = 3f;
        public float mineDamage = 30f;
        /// <summary>存在上限秒数（超时未触发自动失效）。</summary>
        public float mineDurationSeconds = 120f;

        // —— 新星环演示（第二十二步：神圣新星） ——
        /// <summary>扩散速度（格/秒，匀速；最大半径用 radius）。</summary>
        public float novaCellsPerSecond = 3f;
        /// <summary>命中环带宽度（格，判定 |距离-当前半径| ≤ 带宽/2）。</summary>
        public float novaBandWidth = 1f;
        /// <summary>敌方命中伤害（0=不伤敌）。</summary>
        public float novaDamage = 15f;
        public string novaDamageDefName;
        public float novaArmorPenetration;
        /// <summary>友方命中治疗池（0=不治友）。</summary>
        public float novaHealAmount = 8f;
        /// <summary>true=每目标只命中一次；false=环带内持续判定。</summary>
        public bool novaHitOnce = true;
        /// <summary>到达最大半径后的整体淡出 tick。</summary>
        public int novaFadeOutTicks = 18;
        /// <summary>环贴图（白图圆环，代码染色）与染色。</summary>
        public string novaTexPath = "Things/Mote/MMT_NovaRing";
        public Color novaTint = new Color(1f, 0.85f, 0.4f);

        // —— 驱散/净化演示（第二十三步：驱散魔法） ——
        /// <summary>单目标最多移除的限时状态数（友方移除减益、敌方移除增益，Severity 高者优先）。</summary>
        public int dispelCount = 3;
        /// <summary>驱散闪光图标贴图与显示秒数。</summary>
        public string dispelIconTexPath = "UI/RoM/purify";
        public float dispelIconSeconds = 0.5f;

        // —— 造墙演示（第十八步：石墙术） ——
        /// <summary>石墙存续秒数（墙半径用 radius）。</summary>
        public float wallDurationSeconds = 30f;

        // —— 屏幕空间升起特效演示（第二十五步：侧视序列帧火柱，绕开竖直 quad 不渲染） ——
        /// <summary>序列帧图集贴图（横向等宽排布）。</summary>
        public string riseTexPath = "Things/Mote/MMT_SkillRiseFX_FlamePillar";
        public int riseFrameCount = 5;
        /// <summary>每帧停留 tick 数。</summary>
        public int riseFrameTicks = 7;
        /// <summary>true=帧序列循环播放到时长结束；false=播完一轮停在末帧。</summary>
        public bool riseLoop = true;
        public float riseDurationSeconds = 2.2f;
        /// <summary>绘制宽/高（格，贴图底边中点锚在目标格中心）。</summary>
        public float riseWidthCells = 2.2f;
        public float riseHeightCells = 2.6f;
        /// <summary>生命周期内整体上升距离（格，0=钉在原地）。</summary>
        public float riseCells = 0.6f;
        /// <summary>染色，alpha=0 时纯白（帧图自带颜色）。</summary>
        public Color riseTint = new Color(0f, 0f, 0f, 0f);
        /// <summary>首/尾淡入淡出秒数。</summary>
        public float riseFadeInSeconds = 0.15f;
        public float riseFadeOutSeconds = 0.4f;

        // —— 侧视平躺序列帧演示（第二十六步：整帧平铺水平 quad，贴图上边朝北=屏幕上） ——
        /// <summary>序列帧图集贴图（横向等宽排布，侧视构图：底部贴地、顶部朝北）。</summary>
        public string sideFlatTexPath = "Things/Mote/MMT_SkillRiseFX_FlamePillar";
        public int sideFlatFrameCount = 5;
        /// <summary>每帧停留 tick 数。</summary>
        public int sideFlatFrameTicks = 7;
        /// <summary>true=帧序列循环播放到时长结束；false=播完一轮停在末帧。</summary>
        public bool sideFlatLoop = true;
        public float sideFlatDurationSeconds = 2.2f;
        /// <summary>绘制宽/高（格；宽=世界 X，高=世界 +Z 北；贴图底边中点锚在目标格中心）。</summary>
        public float sideFlatWidthCells = 2.2f;
        public float sideFlatHeightCells = 2.6f;
        /// <summary>生命周期内整体向北（屏幕上）漂移距离（格，0=钉在原地）。</summary>
        public float sideFlatDriftCells = 0.6f;
        /// <summary>染色，alpha=0 时纯白（帧图自带颜色）。</summary>
        public Color sideFlatTint = new Color(0f, 0f, 0f, 0f);
        /// <summary>首/尾淡入淡出秒数。</summary>
        public float sideFlatFadeInSeconds = 0.15f;
        public float sideFlatFadeOutSeconds = 0.4f;
    }

    public class CompProperties_MMTSkillDemo : CompProperties
    {
        public List<MMTSkillDemoEntry> demos = new List<MMTSkillDemoEntry>();

        public CompProperties_MMTSkillDemo()
        {
            compClass = typeof(CompSkillDemo);
        }
    }

    /// <summary>
    /// 演示技能 comp：每个条目一个 gizmo（Dev 模式 + 玩家派系限定），点击后进入瞄准
    /// （鼠标跟随范围圈 + 敌我高亮，判定走 TargetsInRadius，带标记的假人建筑也会被圈选）。
    /// 不进任何建筑/角色 Def，经 MMTSkillDemoGrant 运行时注入小人（数据源 MMTSkillDemoSetDef）。
    /// preview 类确认后只弹消息提示；射击类（singleShot/funnelShot）确认目标后进入读条
    /// （读条圆画在施法者正上方），读条结束经 MMTShootUtil 真实发射。
    /// </summary>
    public class CompSkillDemo : ThingComp
    {
        /// <summary>假人读条圆的高度偏移（drawSize 1.6，略高于顶部）。</summary>
        private const float CastBarZOffset = 1.0f;

        // —— 施法状态（一次只允许一个读条，重复触发直接替换） ——
        private MMTSkillDemoEntry castingDemo;
        private LocalTargetInfo castingTarget;
        private int castTicksLeft;
        private int castTicksTotal;
        /// <summary>pawn 读条期间的引导姿态（占用施法者原地不动；建筑不用）。</summary>
        private Stance_Busy channelStance;
        /// <summary>群体传送：第一段圈选已确认、等待第二段落点瞄准的条目。</summary>
        private MMTSkillDemoEntry pendingTeleportDemo;

        public CompProperties_MMTSkillDemo Props => (CompProperties_MMTSkillDemo)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (!Prefs.DevMode || parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            foreach (MMTSkillDemoEntry demo in Props.demos)
            {
                MMTSkillDemoEntry captured = demo;
                yield return new Command_Action
                {
                    defaultLabel = demo.label,
                    defaultDesc = Describe(captured),
                    icon = ContentFinder<Texture2D>.Get(demo.iconPath, false) ?? BaseContent.BadTex,
                    action = delegate
                    {
                        // 光环/新星环类以自身为中心，免瞄准直接进入读条
                        if (captured.demoType == "aura" || captured.demoType == "nova")
                        {
                            FireDemo(captured, new LocalTargetInfo(parent));
                        }
                        else
                        {
                            StartTargeting(captured);
                        }
                    }
                };
            }
        }

        private static string Describe(MMTSkillDemoEntry demo)
        {
            if (demo.demoType == "singleShot" || demo.demoType == "funnelShot")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + (demo.demoType == "singleShot" ? "向目标直射一枚投射物。" : "周边浮游单元齐射目标。");
            }
            if (demo.demoType == "beam")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "沿鼠标指定方向发射持续 " + demo.beamDurationSeconds + "s 的射线，灼烧光柱经过的目标（撞墙截止）。";
            }
            if (demo.demoType == "strike")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                if (demo.strikeCount > 1)
                {
                    return "技能框架演示：" + cast + "在目标区域降下 " + demo.strikeCount + " 道连环落雷（原版闪电风暴表现）。";
                }
                return "技能框架演示：" + cast + (demo.strikeRequireTarget
                    ? "锁定一名" + (demo.targetFriendly ? "友方" : "敌方") + "目标，降下落雷轰击它（原版闪电表现）。"
                    : "在目标点降下一道落雷（原版闪电表现）。");
            }
            if (demo.demoType == "groundFire")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                if (demo.groundAreaRadius > 0f || demo.radius > 0f)
                {
                    return "技能框架演示：" + cast + "在目标区域铺开持续 " + demo.groundDurationSeconds
                        + "s 的烈焰火海（贴图火焰，自中心向外蔓延，灼烧并点燃踏入者）。";
                }
                return "技能框架演示：" + cast + (demo.groundRequireTarget
                    ? "锁定一名" + (demo.targetFriendly ? "友方" : "敌方") + "目标，从其脚下喷起持续 " + demo.groundDurationSeconds + "s 的烈焰火柱（贴图火焰，可点燃）。"
                    : "在目标点喷起持续 " + demo.groundDurationSeconds + "s 的烈焰火柱（贴图火焰，可点燃）。");
            }
            if (demo.demoType == "emberBurst")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标区域（半径 " + demo.radius + "）引发暗红爆裂，火星四溅造成即时伤害并点燃敌人，"
                    + (demo.emberResidDurationSeconds > 0f
                        ? "溅落处留下持续约 " + demo.emberResidDurationSeconds + "s 的暗红余烬继续灼烧。"
                        : "不留余烬。");
            }
            if (demo.demoType == "aura")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                if (demo.auraShieldMode)
                {
                    return "技能框架演示：" + cast + "以自身为中心展开持续 " + demo.auraDurationSeconds + "s 的原版范围护盾（半径 "
                        + demo.radius + "，随目标移动，拦截投射物，命中点 " + demo.auraShieldHitPoints + "）。";
                }
                return "技能框架演示：" + cast + "以自身为中心展开持续 " + demo.auraDurationSeconds + "s 的光环（半径 " + demo.radius
                    + "，随目标移动），" + (demo.targetFriendly ? "增益范围内友方" : "减益范围内敌方") + "。";
            }
            if (demo.demoType == "bufffx")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                string side = demo.targetFriendly ? "增益友方" : "减益敌方";
                return "技能框架演示：" + cast + (demo.buffRequireTarget || demo.radius <= 0f
                    ? "锁定一名" + (demo.targetFriendly ? "友方" : "敌方") + "目标" + side + "（头顶图标 + 身上粒子 + 实际 Hediff）。"
                    : "对范围内所有" + (demo.targetFriendly ? "友方" : "敌方") + "目标" + side + "（头顶图标 + 身上粒子 + 实际 Hediff）。");
            }
            if (demo.demoType == "blink")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "自身折跃到技能范围内的指定位置（瞬移，起终点闪光）。";
            }
            if (demo.demoType == "massTeleport")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "先圈选一个范围，再指定落点，将范围内"
                    + (demo.targetFriendly ? "友方" : "敌方") + "目标整体传送过去（按相对偏移落位）。";
            }
            if (demo.demoType == "charge")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "以快速移动冲锋到指定位置（贴地平移 + 拖尾残影）。";
            }
            if (demo.demoType == "jump")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "跳跃到指定位置（先升后降，pawn 复用原版飞行逻辑）。";
            }
            if (demo.demoType == "summonOrb")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "从自身位置生成雷球（无敌不可选中），缓慢移向目标点，沿途灼烧周围敌方，到达后消失。";
            }
            if (demo.demoType == "summonHoming")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "发射追踪弹锁定一个" + (demo.targetFriendly ? "友方" : "敌方")
                    + " pawn/建筑（弹体无敌不可选中），持续 " + demo.summonHomingDurationSeconds + "s 追踪，命中造成伤害并消失，超时自动消散。";
            }
            if (demo.demoType == "summonAnimal")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标位置召唤驯服的" + demo.summonAnimalKindDefName
                    + "（持续 " + demo.summonAnimalDurationSeconds + "s，不可宰杀/放生，到期消失）。";
            }
            if (demo.demoType == "summonTurret")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标位置召唤自动炮塔（原版炮塔逻辑，无需电力弹药，持续 "
                    + demo.summonTurretDurationSeconds + "s 后消失）。";
            }
            if (demo.demoType == "delayedStrike")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标区域亮出预警圈（半径 " + demo.radius + "），" + demo.meteorDelaySeconds
                    + "s 后坠下陨石轰击（爆炸半径 " + demo.meteorExplosionRadius + "，可点燃）。";
            }
            if (demo.demoType == "knockback")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "猛击近身一名" + (demo.targetFriendly ? "友方" : "敌方")
                    + "目标，将其击退 " + demo.knockbackCells + " 格（撞墙追加伤害，落地踉跄）。";
            }
            if (demo.demoType == "chainLightning")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "向锁定目标施放连锁闪电，自动向 " + demo.chainBounceRange
                    + " 格内最近的敌人弹跳至多 " + demo.chainBounceCount + " 次（伤害逐跳衰减）。";
            }
            if (demo.demoType == "breathCone")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "朝鼠标方向喷出持续 " + demo.breathDurationSeconds + "s 的锥形龙息（射程 "
                    + demo.breathRange + "，半角 " + demo.breathHalfAngleDegrees + "°，灼烧点燃不分敌我）。";
            }
            if (demo.demoType == "heal")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "治疗范围内所有友方目标（每人最多 " + demo.healAmount
                    + " 点，从重伤到轻伤分配，绿十字 + 治疗粒子）。";
            }
            if (demo.demoType == "wall")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在圈选范围（半径 " + demo.radius + "）内召唤一片临时石墙（挡路挡视线，"
                    + demo.wallDurationSeconds + "s 后崩解）。";
            }
            if (demo.demoType == "groundZone")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                if (demo.zoneStaggerTicks > 0)
                {
                    return "技能框架演示：" + cast + "在目标点展开持续 " + demo.zoneDurationSeconds + "s 的寒风领域（半径 " + demo.radius
                        + "，" + (demo.zoneAffectEveryone ? "不分敌我" : "仅敌方") + "，站内被冰冻定身，离开即恢复行动）。";
                }
                return "技能框架演示：" + cast + "在目标点展开持续 " + demo.zoneDurationSeconds + "s 的毒气池（半径 " + demo.radius
                    + "，" + (demo.zoneAffectEveryone ? "不分敌我" : "仅敌方") + "，站内持续积累毒素，离区后自然衰减）。";
            }
            if (demo.demoType == "mine")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标格布设一枚爆炸地雷（" + demo.mineArmSeconds + "s 后武装，敌人进入 "
                    + demo.mineTriggerRadius + " 格触发，爆炸半径 " + demo.mineExplosionRadius + "，" + demo.mineDurationSeconds + "s 未触发自动失效）。";
            }
            if (demo.demoType == "nova")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "以自身为中心爆发新星环（半径 0→" + demo.radius + " 匀速扩散，速度 "
                    + demo.novaCellsPerSecond + " 格/s），环带扫过敌人造成 " + demo.novaDamage + " 点伤害、扫过友方治疗 "
                    + demo.novaHealAmount + " 点" + (demo.novaHitOnce ? "（每目标只命中一次）" : "（环带内持续判定）") + "。";
            }
            if (demo.demoType == "dispel")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "驱散锁定" + (demo.targetFriendly ? "友方" : "敌方") + "目标身上至多 " + demo.dispelCount
                    + " 个限时" + (demo.targetFriendly ? "减益" : "增益") + "（Severity 高者优先，永久/无时限状态不可驱散；白色图标闪光）。";
            }
            if (demo.demoType == "riseFX")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标点从地面升起一段侧视序列帧火柱（屏幕空间贴图路线，持续 "
                    + demo.riseDurationSeconds + "s，纯视觉无伤害）。";
            }
            if (demo.demoType == "sideFlat")
            {
                string cast = demo.castSeconds > 0f ? "读条 " + demo.castSeconds + "s 后" : "瞬发，";
                return "技能框架演示：" + cast + "在目标点平铺一段侧视序列帧火柱（平躺水平 quad 路线，贴图上边朝北=屏幕上，持续 "
                    + demo.sideFlatDurationSeconds + "s，纯视觉无伤害）。";
            }
            return "技能框架演示：范围圈敌我预览，确认后仅提示影响名单，无实际效果。";
        }

        /// <summary>由 demo 条目构建位移参数包（音效沿用 soundDefName）。</summary>
        private static MMTDisplaceParams BuildDisplaceParams(MMTSkillDemoEntry demo, SoundDef sound)
        {
            return new MMTDisplaceParams
            {
                departFleckDef = demo.displaceDepartFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(demo.displaceDepartFleckDefName),
                arriveFleckDef = demo.displaceArriveFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(demo.displaceArriveFleckDefName),
                trailFleckDef = demo.displaceTrailFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(demo.displaceTrailFleckDefName),
                trailIntervalTicks = demo.displaceTrailIntervalTicks,
                chargeTicksPerCell = demo.chargeTicksPerCell,
                chargeMinTicks = demo.chargeMinTicks,
                jumpTicksPerCell = demo.jumpTicksPerCell,
                jumpMinTicks = demo.jumpMinTicks,
                jumpPeakHeight = demo.jumpPeakHeight,
                landingSound = sound
            };
        }

        /// <summary>由 demo 条目构建召唤参数包（音效沿用 soundDefName）。</summary>
        private static MMTSummonParams BuildSummonParams(MMTSkillDemoEntry demo, SoundDef sound)
        {
            return new MMTSummonParams
            {
                orbTexPath = demo.summonOrbTexPath,
                orbTint = demo.summonOrbTint,
                orbTicksPerCell = demo.summonOrbTicksPerCell,
                homingTexPath = demo.summonHomingTexPath,
                homingTint = demo.summonHomingTint,
                homingTicksPerCell = demo.summonHomingTicksPerCell,
                damageDef = demo.summonDamageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(demo.summonDamageDefName),
                damage = demo.summonDamage,
                armorPen = demo.summonArmorPenetration,
                pulseRadius = demo.summonPulseRadius,
                pulseIntervalTicks = demo.summonPulseIntervalTicks,
                hitRadius = 0.7f,
                trailFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark"),
                vanishFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BlinkFlash"),
                startSound = sound
            };
        }

        /// <summary>群体传送第二段：指定落点（画圈提示被圈选范围，确认后整体传送）。</summary>
        private void StartTeleportDestTargeting(MMTSkillDemoEntry demo, IntVec3 fromCenter)
        {
            TargetingParameters parms = new TargetingParameters
            {
                canTargetLocations = true,
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetSelf = false
            };
            Find.Targeter.BeginTargeting(parms,
                delegate(LocalTargetInfo t)
                {
                    pendingTeleportDemo = null;
                    if (parent.Destroyed || parent.Map == null)
                    {
                        return;
                    }
                    SoundDef sound = demo.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(demo.soundDefName);
                    int n = MMTDisplaceUtil.MassTeleport(parent, fromCenter, demo.radius, demo.targetFriendly, demo.includeSelf,
                        t.Cell, parent.Map, BuildDisplaceParams(demo, sound));
                    Messages.Message("【" + demo.label + "】已传送 " + n + " 个目标！",
                        new LookTargets(t.Cell, parent.Map), MessageTypeDefOf.NeutralEvent, false);
                },
                highlightAction: delegate(LocalTargetInfo t)
                {
                    // 落点最大距离环（以自身为中心；超出贴图设计上限或条目关闭时不画）
                    if (demo.showRangeRing && demo.range > 0f && demo.range <= MMTAoETargetPreview.MaxRangeRingRadius)
                    {
                        MMTAoETargetPreview.DrawCircle(parent.Position, demo.range, MMTAoETargetPreview.MaxRangeRingColor);
                    }
                    MMTAoETargetPreview.DrawCircle(fromCenter, demo.radius, MMTAoETargetPreview.RingColor(demo.targetFriendly));
                    // 超出技能距离：不画落点选择框
                    if (t.IsValid && t.Cell.DistanceTo(parent.Position) <= demo.range)
                    {
                        GenDraw.DrawTargetHighlightWithLayer(t.Cell.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
                    }
                },
                targetValidator: delegate(LocalTargetInfo t)
                {
                    return t.IsValid && t.Cell.InBounds(parent.Map) && t.Cell.DistanceTo(parent.Position) <= demo.range;
                });
        }

        private void StartTargeting(MMTSkillDemoEntry demo)
        {
            // 取消遗留的群体传送第二段状态（新瞄准覆盖了它）
            pendingTeleportDemo = null;
            TargetingParameters parms = new TargetingParameters
            {
                // 锁定目标类技能（单体落雷/烈焰喷发/单体增益减益/追踪弹/击退/连锁闪电/驱散）不允许点地板，必须点在 pawn/建筑上
                canTargetLocations = !demo.strikeRequireTarget && !demo.groundRequireTarget && !demo.buffRequireTarget
                    && demo.demoType != "summonHoming" && demo.demoType != "knockback" && demo.demoType != "chainLightning"
                    && demo.demoType != "dispel",
                canTargetPawns = true,
                canTargetBuildings = demo.radius <= 0f && demo.demoType != "knockback" && demo.demoType != "dispel",
                canTargetItems = false,
                canTargetSelf = false
            };
            Find.Targeter.BeginTargeting(parms,
                delegate(LocalTargetInfo t) { FireDemo(demo, t); },
                highlightAction: delegate(LocalTargetInfo t)
                {
                    // 自定义瞄准没有 verb 作 targetingSource，原版最大射程环不会画，这里手画（中性白；
                    // 超出贴图设计上限 MaxRangeRingRadius 的远程技能、或条目关闭时不画）
                    if (demo.showRangeRing && demo.range > 0f && demo.range <= MMTAoETargetPreview.MaxRangeRingRadius)
                    {
                        MMTAoETargetPreview.DrawCircle(parent.Position, demo.range, MMTAoETargetPreview.MaxRangeRingColor);
                    }
                    // 超出技能距离：不画任何选择框/落点标记/路径预览（射程白环仍画，作为距离参考）
                    if (!t.IsValid || t.Cell.DistanceTo(parent.Position) > demo.range)
                    {
                        return;
                    }
                    if (demo.demoType == "beam")
                    {
                        // 射线演示：画鼠标方向的光柱路径预览（与实射同一 raycast）
                        Vector3 dir = MMTBeamUtil.BeamDirection(parent, t.Cell);
                        Color preview = demo.beamTint;
                        preview.a = 0.3f;
                        MMTBeamUtil.DrawBeamPreview(parent.Map, MMTBeamUtil.MuzzleOrigin(parent, dir, demo.beamMuzzleOffset), dir,
                            demo.beamRange, demo.beamWidth, demo.beamTexPath, preview,
                            demo.beamHeadTexPath, demo.beamTailTexPath, demo.beamHeadSize, demo.beamTailSize,
                            demo.beamHeadVertical, demo.beamHeadPillarHeight, demo.beamHeadPillarLayers, demo.beamHeadPillarTopScale);
                    }
                    else if (demo.demoType == "breathCone")
                    {
                        // 龙息演示：画鼠标方向的锥形预览（两条边界线 + 末端弧，与实喷同一几何）
                        Vector3 dir = MMTBeamUtil.BeamDirection(parent, t.Cell);
                        Color preview = new Color(1f, 0.55f, 0.15f, 0.3f);
                        MMTBreathUtil.DrawConePreview(parent.Map, parent.Position.ToVector3Shifted(), dir,
                            demo.breathRange, demo.breathHalfAngleDegrees, "Things/Mote/MMT_SkillBeam", preview);
                    }
                    else if (demo.demoType == "wall")
                    {
                        // 石墙术：只画墙范围圈（不高亮圈内目标，墙落位与目标无关）
                        MMTAoETargetPreview.DrawCircle(t.Cell, demo.radius, MMTAoETargetPreview.RingColor(demo.targetFriendly));
                    }
                    else
                    {
                        // 单体落雷/烈焰喷发/单体增益减益/折跃/冲锋/跳跃/召唤/击退/连锁闪电/驱散类：鼠标格画落点标记；雷暴/火海/范围增益/群体传送/陨石/治疗走下方范围圈
                        if ((demo.demoType == "strike" || demo.demoType == "groundFire" || demo.demoType == "bufffx"
                            || demo.demoType == "blink" || demo.demoType == "charge" || demo.demoType == "jump"
                            || demo.demoType == "summonOrb" || demo.demoType == "summonHoming"
                            || demo.demoType == "summonAnimal" || demo.demoType == "summonTurret"
                            || demo.demoType == "knockback" || demo.demoType == "chainLightning"
                            || demo.demoType == "dispel" || demo.demoType == "riseFX" || demo.demoType == "sideFlat") && demo.radius <= 0f)
                        {
                            GenDraw.DrawTargetHighlightWithLayer(t.Cell.ToVector3Shifted(), AltitudeLayer.MetaOverlays);
                        }
                        if (demo.radius > 0f)
                        {
                            MMTAoETargetPreview.Draw(parent, t.Cell, parent.Map, demo.radius, demo.targetFriendly, demo.includeSelf);
                        }
                    }
                },
                targetValidator: delegate(LocalTargetInfo t)
                {
                    if (!t.IsValid || !t.Cell.InBounds(parent.Map)
                        || t.Cell.DistanceTo(parent.Position) > demo.range
                        || (!demo.ignoreLineOfSight && !GenSight.LineOfSight(parent.Position, t.Cell, parent.Map, true)))
                    {
                        return false;
                    }
                    if (demo.demoType == "knockback")
                    {
                        // 近战击退：必须锁定 pawn，且按 targetFriendly 限定敌我
                        if (!t.HasThing || !(t.Thing is Pawn) || t.Thing == parent)
                        {
                            return false;
                        }
                        return demo.targetFriendly ? t.Thing.Faction == parent.Faction : t.Thing.HostileTo(parent);
                    }
                    if (demo.demoType == "chainLightning")
                    {
                        // 连锁闪电：必须锁定 pawn/建筑，且按 targetFriendly 限定敌我
                        if (!t.HasThing || t.Thing == parent || (!(t.Thing is Pawn) && !(t.Thing is Building)))
                        {
                            return false;
                        }
                        return demo.targetFriendly ? t.Thing.Faction == parent.Faction : t.Thing.HostileTo(parent);
                    }
                    if (demo.demoType == "blink" || demo.demoType == "charge" || demo.demoType == "jump")
                    {
                        // 位移落点必须可站立
                        return t.Cell.Standable(parent.Map);
                    }
                    if (demo.demoType == "summonHoming")
                    {
                        // 追踪弹：必须锁定 pawn/建筑（敌对人偶是建筑），且按 targetFriendly 限定敌我
                        if (!t.HasThing || t.Thing == parent || (!(t.Thing is Pawn) && !(t.Thing is Building)))
                        {
                            return false;
                        }
                        return demo.targetFriendly ? t.Thing.Faction == parent.Faction : t.Thing.HostileTo(parent);
                    }
                    if (demo.demoType == "summonAnimal")
                    {
                        // 动物召唤：落点可站立即可（不可站立时会自动就近找格）
                        return t.Cell.Standable(parent.Map);
                    }
                    if (demo.demoType == "summonTurret")
                    {
                        // 炮塔召唤：落点可站立且无既有建筑
                        return t.Cell.Standable(parent.Map) && t.Cell.GetEdifice(parent.Map) == null;
                    }
                    if (demo.demoType == "dispel")
                    {
                        // 驱散/净化：必须锁定 pawn（Hediff 只长在 pawn 身上），且按 targetFriendly 限定敌我
                        if (!t.HasThing || !(t.Thing is Pawn) || t.Thing == parent)
                        {
                            return false;
                        }
                        return demo.targetFriendly ? t.Thing.Faction == parent.Faction : t.Thing.HostileTo(parent);
                    }
                    if ((demo.demoType == "strike" && demo.strikeRequireTarget)
                        || (demo.demoType == "groundFire" && demo.groundRequireTarget)
                        || (demo.demoType == "bufffx" && demo.buffRequireTarget))
                    {
                        // 锁定目标：必须是 pawn/建筑，且按 targetFriendly 限定敌我（输出类只许打敌方）
                        if (!t.HasThing || t.Thing == parent || (!(t.Thing is Pawn) && !(t.Thing is Building)))
                        {
                            return false;
                        }
                        return demo.targetFriendly ? t.Thing.Faction == parent.Faction : t.Thing.HostileTo(parent);
                    }
                    return true;
                });
        }

        /// <summary>确认落点：preview 类只提示影响名单；射击/射线类进入读条（0 秒则立即发射）。</summary>
        private void FireDemo(MMTSkillDemoEntry demo, LocalTargetInfo target)
        {
            // 新技能起手即打断进行中的持续引导（第十四步：引导中做其它事会断线）
            MMTBeamUtil.BreakChannel(parent);
            if (demo.demoType == "singleShot" || demo.demoType == "funnelShot" || demo.demoType == "beam" || demo.demoType == "strike" || demo.demoType == "groundFire" || demo.demoType == "emberBurst" || demo.demoType == "aura" || demo.demoType == "bufffx"
                || demo.demoType == "blink" || demo.demoType == "massTeleport" || demo.demoType == "charge" || demo.demoType == "jump"
                || demo.demoType == "summonOrb" || demo.demoType == "summonHoming" || demo.demoType == "summonAnimal" || demo.demoType == "summonTurret"
                || demo.demoType == "delayedStrike" || demo.demoType == "knockback" || demo.demoType == "chainLightning"
                || demo.demoType == "breathCone" || demo.demoType == "heal" || demo.demoType == "wall" || demo.demoType == "groundZone"
                || demo.demoType == "mine" || demo.demoType == "nova" || demo.demoType == "dispel" || demo.demoType == "riseFX" || demo.demoType == "sideFlat")
            {
                castingDemo = demo;
                castingTarget = target;
                castTicksTotal = Mathf.Max(1, demo.castSeconds.SecondsToTicks());
                castTicksLeft = castTicksTotal;
                // pawn 读条期间挂引导作业原地定身（占用施法者，移动/下令/倒地即打断施法；建筑不需要）
                if (castTicksLeft > 1 && parent is Pawn pawn)
                {
                    channelStance = MMTBeamUtil.StartChannel(pawn, target.IsValid ? target : new LocalTargetInfo(parent), castTicksLeft);
                }
                if (demo.castSeconds <= 0f)
                {
                    ExecuteShot();
                }
                return;
            }
            List<Thing> targets = MMTAoETargetPreview.TargetsInRadius(parent, target.Cell, parent.Map, demo.radius, demo.targetFriendly, demo.includeSelf).ToList();
            string side = demo.targetFriendly ? "友方" : "敌方";
            string names = targets.Count == 0 ? "（无）" : string.Join("、", targets.Select(t => t.LabelShortCap));
            Messages.Message("【" + demo.label + "】将影响 " + targets.Count + " 名" + side + "：" + names,
                new LookTargets(targets), MessageTypeDefOf.NeutralEvent, false);
            FleckDef fleck = demo.targetFriendly ? FleckDefOf.HealingCross : FleckDefOf.ExplosionFlash;
            foreach (Thing t in targets)
            {
                FleckMaker.ThrowMetaIcon(t.Position, parent.Map, fleck);
            }
        }

        /// <summary>读条倒计时（假人 tickerType=Normal，comp 逐 tick 走）。</summary>
        public override void CompTick()
        {
            base.CompTick();
            if (castingDemo == null)
            {
                return;
            }
            // 引导被打断（移动/被下令/倒地等）：取消读条
            if (channelStance != null && MMTBeamUtil.ChannelBroken(parent as Pawn, channelStance))
            {
                channelStance = null;
                castingDemo = null;
                Messages.Message("【MMT】施法被打断！", parent, MessageTypeDefOf.RejectInput, false);
                return;
            }
            castTicksLeft--;
            if (castTicksLeft <= 0)
            {
                ExecuteShot();
            }
        }

        /// <summary>读条圆高度：建筑用顶部偏移，小人用头顶偏移（Dev 赋予小人时适用）。</summary>
        private float BarZOffset => parent is Pawn ? 0.75f : CastBarZOffset;

        /// <summary>读条期间在建筑/小人正上方画读条圆（满圆顺时针递减）。</summary>
        public override void PostDraw()
        {
            base.PostDraw();
            if (castingDemo == null || castTicksTotal <= 1)
            {
                return;
            }
            float fraction = Mathf.Clamp01(castTicksLeft / (float)castTicksTotal);
            if (castingDemo.castBarColor.a > 0f)
            {
                MMTCastBar.DrawAt(parent.DrawPos, BarZOffset, fraction, castingDemo.castBarColor);
            }
            else
            {
                MMTCastBar.DrawAt(parent.DrawPos, BarZOffset, fraction);
            }
        }

        /// <summary>读条结束：经 MMTShootUtil / MMTBeamUtil / MMTStrikeUtil / MMTGroundFireUtil / MMTAuraUtil / MMTBuffFXUtil / MMTDisplaceUtil / MMTSummonUtil / MMTDelayedStrikeUtil / MMTKnockbackUtil / MMTChainLightningUtil / MMTBreathUtil / MMTHealUtil / MMTWallUtil / MMTGroundZoneUtil / MMTMineUtil / MMTNovaUtil / MMTDispelUtil 真实执行。</summary>
        private void ExecuteShot()
        {
            MMTSkillDemoEntry demo = castingDemo;
            LocalTargetInfo target = castingTarget;
            castingDemo = null;
            // 读条结束：结束引导姿态，让小人恢复正常
            if (channelStance != null && parent is Pawn pawn)
            {
                MMTBeamUtil.EndChannel(pawn, channelStance);
                channelStance = null;
            }
            if (demo == null || parent.Destroyed || parent.Map == null)
            {
                return;
            }
            SoundDef sound = demo.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(demo.soundDefName);
            if (demo.demoType == "beam")
            {
                Thing_MMTSkillBeam beam = MMTBeamUtil.FireBeam(parent, target.Cell, new MMTBeamParams
                {
                    texPath = demo.beamTexPath,
                    tint = demo.beamTint,
                    width = demo.beamWidth,
                    muzzleOffset = demo.beamMuzzleOffset,
                    headTexPath = demo.beamHeadTexPath,
                    tailTexPath = demo.beamTailTexPath,
                    headSize = demo.beamHeadSize,
                    headVertical = demo.beamHeadVertical,
                    headPillarHeight = demo.beamHeadPillarHeight,
                    headPillarLayers = demo.beamHeadPillarLayers,
                    headPillarTopScale = demo.beamHeadPillarTopScale,
                    tailSize = demo.beamTailSize,
                    range = demo.beamRange,
                    durationTicks = Mathf.Max(1, demo.beamDurationSeconds.SecondsToTicks()),
                    damage = demo.beamDamage,
                    damageIntervalTicks = demo.beamDamageIntervalTicks,
                    igniteChance = demo.beamIgniteChance,
                    startSound = sound
                });
                Messages.Message(beam != null ? "【" + demo.label + "】射线展开！" : "【" + demo.label + "】方向无效，射线未能展开",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "strike")
            {
                // 锁定目标类落雷：雷击落在目标身上（目标在读条期间移动则劈其当前位置）；否则劈落点格
                Thing victim = demo.strikeRequireTarget && target.HasThing && target.Thing.Spawned && !target.Thing.Destroyed
                    ? target.Thing
                    : null;
                IntVec3 strikeCell = victim?.Position ?? target.Cell;
                MMTStrikeUtil.FireStorm(parent, strikeCell, new MMTStrikeParams
                {
                    strikeCount = demo.strikeCount,
                    strikeIntervalTicks = demo.strikeIntervalTicks,
                    areaRadius = demo.strikeAreaRadius > 0f ? demo.strikeAreaRadius : (demo.strikeCount > 1 ? demo.radius : 0f),
                    initialDelayTicks = demo.strikeInitialDelayTicks,
                    damage = demo.strikeDamage,
                    armorPenetration = demo.strikeArmorPenetration,
                    explosionRadius = demo.strikeExplosionRadius,
                    chanceToStartFire = demo.strikeIgniteChance
                });
                Messages.Message("【" + demo.label + "】" + (demo.strikeCount > 1 ? "雷暴降临！"
                        : victim != null ? "落雷击中 " + victim.LabelShortCap + "！" : "落雷！"),
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "groundFire")
            {
                // 锁定目标类喷发：火柱从目标脚下喷起（读条期间目标移动则烧其当前位置）；火焰留在原地不追踪
                Thing victim = demo.groundRequireTarget && target.HasThing && target.Thing.Spawned && !target.Thing.Destroyed
                    ? target.Thing
                    : null;
                IntVec3 fireCell = victim?.Position ?? target.Cell;
                bool isSea = demo.groundAreaRadius > 0f || demo.radius > 0f;
                MMTGroundFireUtil.FireGround(parent, fireCell, new MMTGroundFireParams
                {
                    texPath = demo.groundTexPath,
                    tint = demo.groundTint,
                    frameTexPaths = demo.groundFrameTexPaths,
                    frameTicks = demo.groundFrameTicks,
                    visualScale = demo.groundVisualScale,
                    pillarHeight = demo.groundPillarHeight,
                    pillarLayers = demo.groundPillarLayers,
                    pillarTopScale = demo.groundPillarTopScale,
                    pillarSway = demo.groundPillarSway,
                    durationTicks = Mathf.Max(1, demo.groundDurationSeconds.SecondsToTicks()),
                    damage = demo.groundDamage,
                    damageIntervalTicks = demo.groundDamageIntervalTicks,
                    armorPenetration = demo.groundArmorPenetration,
                    ignitePawnChance = demo.groundIgnitePawnChance,
                    igniteCellChance = demo.groundIgniteCellChance,
                    effectRadius = demo.groundEffectRadius,
                    areaRadius = isSea ? (demo.groundAreaRadius > 0f ? demo.groundAreaRadius : demo.radius) : 0f,
                    density = demo.groundDensity,
                    spreadPerCellTicks = demo.groundSpreadPerCellTicks,
                    startSound = sound
                });
                Messages.Message("【" + demo.label + "】" + (isSea ? "烈焰火海蔓延！"
                        : victim != null ? "烈焰吞没 " + victim.LabelShortCap + "！" : "烈焰喷发！"),
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "emberBurst")
            {
                // 余烬爆发：暗红爆裂 + 火星四溅 + 即时伤害 + 溅落短余烬（第二十四步）
                MMTEmberBurstUtil.FireBurst(parent, target.Cell, new MMTEmberBurstParams
                {
                    radius = demo.radius,
                    emberCount = demo.emberCount,
                    speedMin = demo.emberSparkSpeedMin,
                    speedMax = demo.emberSparkSpeedMax,
                    sparkTint = demo.emberSparkTint,
                    sparkTintDark = demo.emberSparkTintDark,
                    burstDamage = demo.emberBurstDamage,
                    burstArmorPenetration = demo.emberBurstArmorPenetration,
                    burstIgnitePawnChance = demo.emberBurstIgnitePawnChance,
                    residDurationSeconds = demo.emberResidDurationSeconds,
                    residDamage = demo.emberResidDamage,
                    residDamageIntervalTicks = demo.emberResidDamageIntervalTicks,
                    residDensity = demo.emberResidDensity,
                    residTint = demo.emberResidTint,
                    residIgniteCellChance = demo.emberResidIgniteCellChance,
                    startSound = sound,
                    equipment = (parent as Pawn)?.equipment?.Primary
                });
                Messages.Message("【" + demo.label + "】暗红爆裂，余烬四溅！",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "aura")
            {
                Color tint = demo.auraTint.a > 0f ? demo.auraTint : MMTAoETargetPreview.RingColor(demo.targetFriendly);
                MMTAuraUtil.StartAura(parent, parent, new MMTAuraParams
                {
                    radius = demo.radius,
                    durationTicks = Mathf.Max(1, demo.auraDurationSeconds.SecondsToTicks()),
                    tint = tint,
                    targetFriendly = demo.targetFriendly,
                    includeSelf = demo.includeSelf,
                    shieldMode = demo.auraShieldMode,
                    shieldHitPoints = demo.auraShieldHitPoints,
                    buffDef = demo.auraBuffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(demo.auraBuffDefName),
                    refreshIntervalTicks = demo.auraRefreshIntervalTicks,
                    discTexPath = demo.auraTexPath,
                    startSound = sound
                });
                Messages.Message("【" + demo.label + "】" + (demo.auraShieldMode ? "范围护盾展开！" : "光环展开！"),
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "bufffx")
            {
                // 增益/减益：单体锁定目标走 ApplyOne；范围圈选走 TargetsInRadius（含带标记的假人建筑，建筑只放特效不加 Hediff）
                Color iconTint = demo.buffIconTint.a > 0f ? demo.buffIconTint : MMTAoETargetPreview.RingColor(demo.targetFriendly);
                MMTBuffFXParams bp = new MMTBuffFXParams
                {
                    buffDef = demo.buffHediffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(demo.buffHediffDefName),
                    iconTexPath = demo.buffIconTexPath,
                    iconTint = iconTint,
                    iconDurationTicks = Mathf.Max(1, demo.buffIconSeconds.SecondsToTicks()),
                    particleFleckDef = demo.buffParticleFleckDefName.NullOrEmpty() ? null : DefDatabase<FleckDef>.GetNamedSilentFail(demo.buffParticleFleckDefName),
                    particleTint = demo.buffParticleTint.a > 0f ? demo.buffParticleTint : iconTint,
                    startSound = sound
                };
                int affected;
                if (demo.buffRequireTarget || demo.radius <= 0f)
                {
                    affected = 0;
                    if (target.HasThing && !target.Thing.Destroyed && target.Thing.Spawned)
                    {
                        MMTBuffFXUtil.ApplyOne(target.Thing, bp);
                        affected = 1;
                    }
                }
                else
                {
                    affected = MMTBuffFXUtil.Apply(parent,
                        MMTAoETargetPreview.TargetsInRadius(parent, target.Cell, parent.Map, demo.radius, demo.targetFriendly, demo.includeSelf), bp);
                }
                Messages.Message("【" + demo.label + "】" + (affected > 0
                        ? "已" + (demo.targetFriendly ? "增益 " : "减益 ") + affected + " 个目标！"
                        : "没有有效目标！"),
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "blink" || demo.demoType == "charge" || demo.demoType == "jump")
            {
                // 位移：落点必须可站立；折跃瞬移，冲锋贴地平移，跳跃先升后降
                if (!MMTDisplaceUtil.CanLand(target.Cell, parent.Map))
                {
                    Messages.Message("【" + demo.label + "】落点不可站立，位移取消", parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTDisplaceParams p = BuildDisplaceParams(demo, sound);
                if (demo.demoType == "blink")
                {
                    MMTDisplaceUtil.Blink(parent, target.Cell, p);
                    Messages.Message("【" + demo.label + "】折跃！", parent, MessageTypeDefOf.NeutralEvent, false);
                }
                else if (demo.demoType == "charge")
                {
                    MMTDisplaceUtil.StartCharge(parent, target.Cell, p);
                    Messages.Message("【" + demo.label + "】冲锋！", parent, MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    MMTDisplaceUtil.StartJump(parent, target.Cell, p);
                    Messages.Message("【" + demo.label + "】跳跃！", parent, MessageTypeDefOf.NeutralEvent, false);
                }
                return;
            }
            if (demo.demoType == "massTeleport")
            {
                // 群体传送：第一段圈选已确认，进入第二段落点瞄准
                pendingTeleportDemo = demo;
                StartTeleportDestTargeting(demo, target.Cell);
                Messages.Message("【" + demo.label + "】请指定传送落点", parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "summonOrb")
            {
                // 雷球：从自身位置缓慢移向目标点，沿途脉冲伤害周围敌方
                MMTSummonParams sp = BuildSummonParams(demo, sound);
                Thing_MMTSummonOrb orb = MMTSummonUtil.FireOrb(parent, target.Cell, sp);
                Messages.Message(orb != null ? "【" + demo.label + "】雷球出击！" : "【" + demo.label + "】召唤失败",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "summonHoming")
            {
                // 追踪弹：锁定目标持续追踪，命中或超时消失
                if (!target.HasThing || target.Thing.Destroyed || !target.Thing.Spawned)
                {
                    Messages.Message("【" + demo.label + "】目标已失效", parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTSummonParams sp = BuildSummonParams(demo, sound);
                sp.durationTicks = Mathf.Max(1, demo.summonHomingDurationSeconds.SecondsToTicks());
                sp.explosionRadius = demo.summonHomingExplosionRadius;
                MMTSummonUtil.FireHoming(parent, target.Thing, sp);
                Messages.Message("【" + demo.label + "】追踪弹发射！", parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "summonAnimal")
            {
                // 动物召唤：目标位置生成驯服动物，到期消失（不可宰杀/放生）
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(demo.summonAnimalKindDefName);
                if (kind == null)
                {
                    Messages.Message("【" + demo.label + "】找不到动物种类 " + demo.summonAnimalKindDefName,
                        parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTSummonParams sp = BuildSummonParams(demo, sound);
                sp.animalKind = kind;
                sp.durationTicks = Mathf.Max(1, demo.summonAnimalDurationSeconds.SecondsToTicks());
                Pawn summoned = MMTSummonUtil.SummonAnimal(parent, target.Cell, sp);
                Messages.Message(summoned != null ? "【" + demo.label + "】召唤了 " + summoned.LabelShortCap + "！"
                        : "【" + demo.label + "】目标位置无法召唤",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "summonTurret")
            {
                // 炮塔召唤：目标位置生成原版逻辑的自动炮塔，CompLifespan 到期自毁
                ThingDef turretDef = DefDatabase<ThingDef>.GetNamedSilentFail(demo.summonTurretDefName);
                if (turretDef == null)
                {
                    Messages.Message("【" + demo.label + "】找不到炮塔 " + demo.summonTurretDefName,
                        parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTSummonParams sp = BuildSummonParams(demo, sound);
                sp.turretDef = turretDef;
                sp.durationTicks = Mathf.Max(1, demo.summonTurretDurationSeconds.SecondsToTicks());
                Building turret = MMTSummonUtil.SummonTurret(parent, target.Cell, sp);
                Messages.Message(turret != null ? "【" + demo.label + "】炮塔展开！"
                        : "【" + demo.label + "】目标位置无法部署炮塔",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "delayedStrike")
            {
                // 延时打击：目标区域亮预警圈，延迟结束坠下陨石
                MMTDelayedStrikeUtil.FireDelayedStrike(parent, target.Cell, new MMTDelayedStrikeParams
                {
                    warningRadius = demo.radius > 0f ? demo.radius : 3f,
                    delayTicks = Mathf.Max(1, demo.meteorDelaySeconds.SecondsToTicks()),
                    fallTicks = demo.meteorFallTicks,
                    damage = demo.meteorDamage,
                    armorPenetration = demo.meteorArmorPenetration,
                    explosionRadius = demo.meteorExplosionRadius,
                    chanceToStartFire = demo.meteorIgniteChance,
                    meteorDrawSize = demo.meteorDrawSize,
                    meteorSpinDegPerTick = demo.meteorSpinDegPerTick,
                    meteorGrowFrom = demo.meteorGrowFrom,
                    meteorGrowTo = demo.meteorGrowTo
                });
                Messages.Message("【" + demo.label + "】陨石即将坠落！", parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "knockback")
            {
                // 近战击退：猛击并击飞目标（撞墙追加伤害）
                if (!target.HasThing || target.Thing.Destroyed || !target.Thing.Spawned)
                {
                    Messages.Message("【" + demo.label + "】目标已失效", parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTKnockbackUtil.Execute(parent, target.Thing, new MMTKnockbackParams
                {
                    damage = demo.knockbackDamage,
                    knockbackCells = demo.knockbackCells,
                    wallSlamDamage = demo.knockbackWallSlamDamage,
                    departFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BlinkFlash"),
                    sound = sound
                });
                Messages.Message("【" + demo.label + "】击退！", parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "chainLightning")
            {
                // 连锁闪电：锁定首目标，自动在敌方目标间弹跳
                if (!target.HasThing || target.Thing.Destroyed || !target.Thing.Spawned)
                {
                    Messages.Message("【" + demo.label + "】目标已失效", parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                MMTChainLightningUtil.Fire(parent, target.Thing, new MMTChainParams
                {
                    bounceCount = demo.chainBounceCount,
                    bounceRange = demo.chainBounceRange,
                    damage = demo.chainDamage,
                    damageFalloff = demo.chainDamageFalloff,
                    sound = sound
                });
                Messages.Message("【" + demo.label + "】闪电链放出！", parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "breathCone")
            {
                // 锥形吐息：朝鼠标方向喷出持续龙息（喷口跟随施法者，不分敌我）
                Thing_MMTBreathCone cone = MMTBreathUtil.FireBreath(parent, target.Cell, new MMTBreathParams
                {
                    range = demo.breathRange,
                    halfAngleDegrees = demo.breathHalfAngleDegrees,
                    durationTicks = Mathf.Max(1, demo.breathDurationSeconds.SecondsToTicks()),
                    damage = demo.breathDamage,
                    damageIntervalTicks = demo.breathDamageIntervalTicks,
                    igniteChance = demo.breathIgniteChance,
                    startSound = sound
                });
                Messages.Message(cone != null ? "【" + demo.label + "】吐息喷出！" : "【" + demo.label + "】方向无效，吐息失败",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "heal")
            {
                // 治疗：范围内友方回血（绿十字 + 治疗图标/粒子）
                int healed = MMTHealUtil.Apply(parent,
                    MMTAoETargetPreview.TargetsInRadius(parent, target.Cell, parent.Map, demo.radius, demo.targetFriendly, demo.includeSelf),
                    new MMTHealParams
                    {
                        healAmount = demo.healAmount,
                        particleFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark"),
                        startSound = sound
                    });
                Messages.Message("【" + demo.label + "】" + (healed > 0 ? "治疗了 " + healed + " 名目标！" : "没有可治疗的目标！"),
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "wall")
            {
                // 石墙术：圈选范围内召唤临时石墙
                int walls = MMTWallUtil.RaiseWall(parent, target.Cell, demo.radius, new MMTWallParams
                {
                    durationTicks = Mathf.Max(1, demo.wallDurationSeconds.SecondsToTicks()),
                    startSound = sound
                });
                Messages.Message(walls > 0 ? "【" + demo.label + "】召唤了 " + walls + " 段石墙！"
                        : "【" + demo.label + "】该区域内没有可召唤石墙的位置！",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "groundZone")
            {
                // 持续地面区域：目标点展开毒气池（站内累积毒素）/ 寒风领域（站内冰冻定身）
                Thing_MMTGroundZone zone = MMTGroundZoneUtil.FireZone(parent, target.Cell, new MMTGroundZoneParams
                {
                    radius = demo.radius,
                    durationTicks = Mathf.Max(1, demo.zoneDurationSeconds.SecondsToTicks()),
                    intervalTicks = demo.zoneIntervalTicks,
                    affectMode = demo.zoneAffectEveryone ? MMTZoneAffectMode.Everyone : MMTZoneAffectMode.HostileOnly,
                    damage = demo.zoneDamage,
                    damageDef = demo.zoneDamageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(demo.zoneDamageDefName),
                    hediffDef = demo.zoneHediffDefName.NullOrEmpty() ? null : DefDatabase<HediffDef>.GetNamedSilentFail(demo.zoneHediffDefName),
                    hediffSeverityPerPulse = demo.zoneHediffSeverity,
                    staggerTicksPerPulse = demo.zoneStaggerTicks,
                    iceScale = demo.zoneIceScale,
                    texPath = demo.zoneTexPath,
                    tint = demo.zoneTint,
                    moteFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark"),
                    startSound = sound
                });
                string zoneName = demo.zoneStaggerTicks > 0 ? "寒风领域展开！" : "毒气池蔓延！";
                Messages.Message(zone != null ? "【" + demo.label + "】" + zoneName : "【" + demo.label + "】区域展开失败",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "mine")
            {
                // 触发式爆炸陷阱：目标格布设地雷（武装后敌人进入触发半径引爆，超时失效）
                Thing_MMTMine mine = MMTMineUtil.PlaceMine(parent, target.Cell, new MMTMineParams
                {
                    armTicks = Mathf.Max(1, demo.mineArmSeconds.SecondsToTicks()),
                    triggerRadius = demo.mineTriggerRadius,
                    explosionRadius = demo.mineExplosionRadius,
                    damage = demo.mineDamage,
                    durationTicks = Mathf.Max(1, demo.mineDurationSeconds.SecondsToTicks()),
                    startSound = sound,
                    equipment = (parent as Pawn)?.equipment?.Primary
                });
                Messages.Message(mine != null ? "【" + demo.label + "】地雷已布设！" : "【" + demo.label + "】该位置无法布设地雷",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "nova")
            {
                // 新星环：以自身当前位置为圆心爆发扩散冲击波（环带扫过：敌伤友疗）
                Thing_MMTNovaRing nova = MMTNovaUtil.FireNova(parent, parent.Position, new MMTNovaParams
                {
                    maxRadius = demo.radius,
                    cellsPerSecond = demo.novaCellsPerSecond,
                    bandWidth = demo.novaBandWidth,
                    damageDef = demo.novaDamageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(demo.novaDamageDefName),
                    damage = demo.novaDamage,
                    armorPenetration = demo.novaArmorPenetration,
                    healAmount = demo.novaHealAmount,
                    hitOnce = demo.novaHitOnce,
                    fadeOutTicks = demo.novaFadeOutTicks,
                    texPath = demo.novaTexPath,
                    tint = demo.novaTint,
                    startSound = sound,
                    equipment = (parent as Pawn)?.equipment?.Primary
                });
                Messages.Message(nova != null ? "【" + demo.label + "】新星爆发！" : "【" + demo.label + "】新星未能爆发",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "dispel")
            {
                // 驱散/净化：移除锁定 pawn 身上至多 N 个限时状态（友方减益/敌方增益，Severity 高者优先）
                if (!target.HasThing || !(target.Thing is Pawn dispelTarget) || dispelTarget.Destroyed || !dispelTarget.Spawned)
                {
                    Messages.Message("【" + demo.label + "】目标已失效", parent, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                List<string> removed = MMTDispelUtil.DispelOne(parent, dispelTarget, new MMTDispelParams
                {
                    maxRemove = demo.dispelCount,
                    iconTexPath = demo.dispelIconTexPath,
                    iconDurationTicks = Mathf.Max(1, demo.dispelIconSeconds.SecondsToTicks()),
                    particleFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark"),
                    sound = sound
                });
                Messages.Message(removed.Count > 0
                        ? "【" + demo.label + "】驱散了 " + dispelTarget.LabelShortCap + " 的 " + removed.Count + " 个状态：" + string.Join("、", removed)
                        : "【" + demo.label + "】" + dispelTarget.LabelShortCap + " 身上没有可驱散的限时状态！",
                    parent, MessageTypeDefOf.NeutralEvent, false);
                return;
            }
            if (demo.demoType == "riseFX")
            {
                // 屏幕空间升起序列帧：纯视觉演示（侧视素材走 UI 层贴图，绕开竖直 quad 不渲染）
                MoteMMTSkillRiseFX fxMote = MMTSkillRiseFXUtil.Fire(parent.Map, target.Cell, new MMTSkillRiseFXParams
                {
                    texPath = demo.riseTexPath,
                    frameCount = demo.riseFrameCount,
                    frameTicks = demo.riseFrameTicks,
                    loop = demo.riseLoop,
                    durationSeconds = demo.riseDurationSeconds,
                    widthCells = demo.riseWidthCells,
                    heightCells = demo.riseHeightCells,
                    riseCells = demo.riseCells,
                    tint = demo.riseTint,
                    fadeInSeconds = demo.riseFadeInSeconds,
                    fadeOutSeconds = demo.riseFadeOutSeconds,
                    startSound = sound
                });
                Messages.Message(fxMote != null ? "【" + demo.label + "】地心火柱喷发！" : "【" + demo.label + "】特效生成失败",
                    parent, fxMote != null ? MessageTypeDefOf.NeutralEvent : MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (demo.demoType == "sideFlat")
            {
                // 侧视平躺序列帧：纯视觉演示（整帧平铺水平 quad，贴图上边朝北=屏幕上）
                Thing_MMTSkillSideFlat flatFx = MMTSkillSideFlatUtil.Fire(parent.Map, target.Cell, new MMTSkillSideFlatParams
                {
                    texPath = demo.sideFlatTexPath,
                    frameCount = demo.sideFlatFrameCount,
                    frameTicks = demo.sideFlatFrameTicks,
                    loop = demo.sideFlatLoop,
                    durationSeconds = demo.sideFlatDurationSeconds,
                    widthCells = demo.sideFlatWidthCells,
                    heightCells = demo.sideFlatHeightCells,
                    driftCells = demo.sideFlatDriftCells,
                    tint = demo.sideFlatTint,
                    fadeInSeconds = demo.sideFlatFadeInSeconds,
                    fadeOutSeconds = demo.sideFlatFadeOutSeconds,
                    startSound = sound
                });
                Messages.Message(flatFx != null ? "【" + demo.label + "】地心火柱喷发！" : "【" + demo.label + "】特效生成失败",
                    parent, flatFx != null ? MessageTypeDefOf.NeutralEvent : MessageTypeDefOf.RejectInput, false);
                return;
            }
            ThingDef projectile = DefDatabase<ThingDef>.GetNamedSilentFail(demo.projectileDefName);
            if (projectile == null)
            {
                Log.Warning("[MMT] 技能演示「" + demo.label + "」找不到投射物 " + demo.projectileDefName);
                return;
            }
            if (demo.demoType == "funnelShot")
            {
                MMTShootUtil.FireFunnels(parent, target, new MMTFunnelParams
                {
                    projectileDef = projectile,
                    funnelCount = demo.funnelCount,
                    ringRadius = demo.ringRadius,
                    aimTicks = demo.aimTicks,
                    funnelFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail(demo.funnelFleckDefName),
                    sound = sound
                });
            }
            else
            {
                MMTShootUtil.FireSingle(parent, target, projectile, sound);
            }
            Messages.Message("【" + demo.label + "】发射！", parent, MessageTypeDefOf.NeutralEvent, false);
        }
    }

    // ========== 敌方沙袋人偶 ==========

    public class CompProperties_MMTForceHostileFaction : CompProperties
    {
        public CompProperties_MMTForceHostileFaction()
        {
            compClass = typeof(CompForceHostileFaction);
        }
    }

    /// <summary>敌方假人：生成后强制归属远古敌对派系（恒敌对、无外交副作用），充当敌方靶子。</summary>
    public class CompForceHostileFaction : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent.Faction != Faction.OfAncientsHostile)
            {
                parent.SetFaction(Faction.OfAncientsHostile);
            }
        }
    }

    /// <summary>敌方沙袋人偶仅 Dev 模式可见可建（同 BossSummoner 的 Dev 门控模式）。</summary>
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.Visible), MethodType.Getter)]
    public static class Patch_MMTTrainingDummyHostileDevOnly
    {
        public static void Postfix(Designator_Build __instance, ref bool __result)
        {
            if (__result && !Prefs.DevMode
                && __instance.PlacingDef is ThingDef td && td.defName == "MMT_TrainingDummyHostile")
            {
                __result = false;
            }
        }
    }

    // ========== Dev 一键赋予小人全部演示技能 ==========

    /// <summary>技能框架演示技能集 Def：全部演示条目的唯一数据源（1.7/Defs/Misc/MMT_SkillDemoSet.xml）。</summary>
    public class MMTSkillDemoSetDef : Def
    {
        public List<MMTSkillDemoEntry> demos = new List<MMTSkillDemoEntry>();
    }

    /// <summary>
    /// 把演示技能集（CompSkillDemo）运行时注入到任意小人身上，
    /// 以 pawn 身份实测全部十步框架（位移/光环等以自身为载体的技能在小人上更直观）。
    /// 演示条目来自 MMTSkillDemoSetDef（单一维护点，沙袋人偶只留敌我靶子、不再携带技能）。
    /// 注意：comp 为运行时注入，原版只按 Def 重建 comp，故存档读档后需重新一键赋予。
    /// </summary>
    public static class MMTSkillDemoGrant
    {
        private static CompProperties_MMTSkillDemo sharedProps;

        /// <summary>演示技能数据源：MMT_SkillDemoSet Def（运行时包一层 comp props）。</summary>
        public static CompProperties_MMTSkillDemo SharedProps =>
            sharedProps ?? (sharedProps = new CompProperties_MMTSkillDemo
            {
                demos = DefDatabase<MMTSkillDemoSetDef>.GetNamed("MMT_SkillDemoSet").demos
            });

        public static CompSkillDemo Get(Pawn pawn)
        {
            return pawn?.AllComps?.FirstOrDefault(c => c is CompSkillDemo) as CompSkillDemo;
        }

        public static bool Grant(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || Get(pawn) != null)
            {
                return false;
            }
            CompSkillDemo comp = new CompSkillDemo { parent = pawn };
            comp.Initialize(SharedProps);
            pawn.AllComps.Add(comp);
            return true;
        }

        public static bool Revoke(Pawn pawn)
        {
            CompSkillDemo comp = Get(pawn);
            if (comp == null)
            {
                return false;
            }
            pawn.AllComps.Remove(comp);
            return true;
        }
    }

    /// <summary>演示技能赋予/移除 Debug 工具（gizmo 本身仍要求玩家派系 + Dev 模式才显示）。</summary>
    public static class MMTSkillDemoDebugActions
    {
        [DebugAction("MMT 技能", "赋予全部演示技能（选中小人）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GrantDemos(Pawn pawn)
        {
            if (MMTSkillDemoGrant.Grant(pawn))
            {
                Messages.Message("【MMT】已赋予 " + pawn.LabelShortCap + " 全部演示技能（" + MMTSkillDemoGrant.SharedProps.demos.Count + " 个），选中小人查看 gizmo", pawn, MessageTypeDefOf.NeutralEvent, false);
            }
            else if (pawn != null)
            {
                Messages.Message("【MMT】" + pawn.LabelShortCap + " 已有演示技能", pawn, MessageTypeDefOf.RejectInput, false);
            }
        }

        [DebugAction("MMT 技能", "移除演示技能（选中小人）", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RevokeDemos(Pawn pawn)
        {
            if (MMTSkillDemoGrant.Revoke(pawn))
            {
                Messages.Message("【MMT】已移除 " + pawn.LabelShortCap + " 的演示技能", pawn, MessageTypeDefOf.NeutralEvent, false);
            }
        }
    }
}
