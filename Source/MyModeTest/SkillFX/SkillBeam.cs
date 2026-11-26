using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 技能持续射线框架（第四步：鼠标指定方向发射持续光柱；第十四步：引导占用） ==========
    // 玩家用鼠标点一个方向，射线从施法者出发沿该方向贯穿至最大射程（撞满填墙体停在墙面），
    // 持续期间按固定间隔对光柱经过的格子逐格结算伤害（默认 Burn，可点燃），光束本体为
    // 贴图拉伸四边形（Thing_MMTSkillBeam.Draw 自绘，宽度呼吸抖动 + 淡入淡出）。
    // 第十四步（引导占用，MMTBeamParams.occupyCaster）：pawn 施法者发射后进入引导状态
    // （JobDriver_MMTChannel 原地引导 + Stance_Busy 禁止开火），期间不能做其它事，
    // 移动/下令/被眩晕倒地等任何打断都会立刻切断射线；建筑施法者不受影响。
    // MMTBeamUtil.FireBeam 为统一入口；射线长度/终点计算 BeamLength 与伤害结算共用同一
    // 步进 raycast，保证"看到的光柱"和"烧到的格子"严格一致。
    // pawn 技能侧出口：CompAbilityEffect_MMTBeam（读条时间由 AbilityDef.warmupTime 决定）；
    // 建筑演示侧出口：AbilityTargeting.cs 的 CompSkillDemo（沙袋人偶 demoType="beam"）。
    // 贴图默认 MMT 占位光柱（Pillow 生成，可 AI 终稿直接覆盖）；音效/伤害类型全走原版。

    /// <summary>持续射线参数包（MMTBeamUtil.FireBeam 的输入）。</summary>
    public class MMTBeamParams
    {
        /// <summary>光柱贴图（纵向光束、白色核心，代码染色；UV 的 V 轴 = 光束长度方向）。</summary>
        public string texPath = "Things/Mote/MMT_SkillBeam";
        public Color tint = new Color(1f, 0.55f, 0.15f);
        /// <summary>光柱宽度（格）。</summary>
        public float width = 0.7f;
        /// <summary>出膛偏移（格）：光柱起点沿发射方向前移到施法者身前，避免与施法者身体重叠。</summary>
        public float muzzleOffset = MMTBeamUtil.MuzzleOffset;
        /// <summary>发射头贴图（出膛点光球/喷口，白图代码染色；空 = 不画）。</summary>
        public string headTexPath = "Things/Mote/MMT_SkillBeam_Head";
        /// <summary>命中端贴图（光柱终点爆点/溅射，白图代码染色；空 = 不画）。</summary>
        public string tailTexPath = "Things/Mote/MMT_SkillBeam_Tail";
        /// <summary>发射头绘制尺寸（格）。</summary>
        public float headSize = 1.4f;
        /// <summary>true = 发射头画成"地面喷发火柱"：多层水平碟从地面堆叠到空中，
        /// 俯视相机下高度差投成垂直柱体（1.7 竖直 quad 不渲染，严禁立起 plane10，
        /// 见 ChargingStation.DrawBeam / Building_Singularity 的对照实验结论）；
        /// false = 旧式平躺光球（随光柱方向旋转）。</summary>
        public bool headVertical;
        /// <summary>火柱高度（世界单位，碟片堆叠总高）。</summary>
        public float headPillarHeight = 1.1f;
        /// <summary>火柱碟片层数（≥2）。</summary>
        public int headPillarLayers = 6;
        /// <summary>火柱顶端缩放系数（相对底部，&lt;1 呈锥形收口）。</summary>
        public float headPillarTopScale = 0.55f;
        /// <summary>命中端绘制尺寸（格）。</summary>
        public float tailSize = 1.6f;
        /// <summary>最大射程（格，撞墙提前截止）。</summary>
        public float range = 20f;
        /// <summary>持续 tick 数。</summary>
        public int durationTicks = 240;
        /// <summary>伤害类型（null = Burn）。</summary>
        public DamageDef damageDef;
        /// <summary>每跳伤害。</summary>
        public float damage = 4f;
        /// <summary>跳伤间隔 tick。</summary>
        public int damageIntervalTicks = 10;
        public float armorPenetration;
        /// <summary>每跳点燃经过格子的概率（0 = 不点燃）。</summary>
        public float igniteChance = 0.1f;
        /// <summary>true = 施法者进入引导状态（第十四步）：持续期间不能做其它事，
        /// 移动/下令/被眩晕等任何打断都会立刻切断射线；建筑施法者自动忽略。</summary>
        public bool occupyCaster = true;
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>持续射线核心：方向/终点计算、发射、瞄准预览、引导状态管理，pawn 技能与建筑演示共用。</summary>
    public static class MMTBeamUtil
    {
        /// <summary>步进 raycast 的采样步长（格），与伤害结算共用。</summary>
        public const float MarchStep = 0.5f;

        /// <summary>默认出膛偏移（格）：光柱起点沿发射方向前移到施法者身前，避免与小人身体重叠。
        /// 实际偏移以 MMTBeamParams.muzzleOffset / 各出口 XML 字段为准，此处仅为兜底默认。</summary>
        public const float MuzzleOffset = 0.9f;

        /// <summary>施法者身前的出膛点（脚下位置沿 dir 前移 offset），实射与瞄准预览统一取这里。</summary>
        public static Vector3 MuzzleOrigin(Thing caster, Vector3 dir, float offset = MuzzleOffset)
        {
            return caster.Position.ToVector3Shifted() + dir * offset;
        }

        /// <summary>引导作业 Def（持续技能占用施法者用，见 JobDefs/MMT_Jobs.xml）。</summary>
        public const string ChannelJobDefName = "MMT_ChannelSkill";

        private static JobDef channelJobDef;

        public static JobDef ChannelJobDef =>
            channelJobDef ?? (channelJobDef = DefDatabase<JobDef>.GetNamedSilentFail(ChannelJobDefName));

        // 预览材质按"贴图+颜色"缓存（预览色固定，不会逐帧改 alpha，可用共享材质）
        private static readonly Dictionary<string, Material> previewMats = new Dictionary<string, Material>();

        /// <summary>施法者 → 目标格的水平方向（单位向量）；目标与施法者重合时返回零向量。</summary>
        public static Vector3 BeamDirection(Thing caster, IntVec3 aimCell)
        {
            Vector3 dir = aimCell.ToVector3Shifted() - caster.Position.ToVector3Shifted();
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
            {
                return Vector3.zero;
            }
            return dir.normalized;
        }

        /// <summary>
        /// 沿方向步进取射线长度：每 0.5 格采样，出界回退半步结束，
        /// 撞上满填建筑（关着的门/墙）则停在其格内（光柱顶到墙面，该格可被烧到）。
        /// </summary>
        public static float BeamLength(Map map, Vector3 origin, Vector3 dir, float maxRange)
        {
            float dist = 0f;
            while (dist < maxRange)
            {
                dist += MarchStep;
                IntVec3 cell = (origin + dir * dist).ToIntVec3();
                if (!cell.InBounds(map))
                {
                    dist -= MarchStep;
                    break;
                }
                Building edifice = cell.GetEdifice(map);
                if (edifice != null && edifice.def.fillPercent >= 1f && !(edifice is Building_Door door && door.Open))
                {
                    break;
                }
            }
            return Mathf.Min(dist, maxRange);
        }

        /// <summary>发射持续射线：生成 Thing_MMTSkillBeam 并播开火音效，返回射线实例（方向无效返回 null）。</summary>
        public static Thing_MMTSkillBeam FireBeam(Thing caster, IntVec3 aimCell, MMTBeamParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null)
            {
                return null;
            }
            Vector3 dir = BeamDirection(caster, aimCell);
            if (dir.sqrMagnitude < 0.01f)
            {
                return null;
            }
            // 起点前移出膛偏移：光柱从施法者身前射出，不再穿过小人身体
            Vector3 origin = MuzzleOrigin(caster, dir, p.muzzleOffset);
            float length = BeamLength(map, origin, dir, p.range);
            if (length < MarchStep)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail("MMT_SkillBeam");
            if (def == null)
            {
                Log.Error("[MMT] 找不到 MMT_SkillBeam 定义，无法发射技能射线");
                return null;
            }
            Thing_MMTSkillBeam beam = (Thing_MMTSkillBeam)GenSpawn.Spawn(def, caster.Position, map);
            beam.Setup(caster, origin, dir, length, p);
            // 引导状态（第十四步）：pawn 施法者被射线占用，移动/下令/受创等打断即断线
            if (p.occupyCaster && caster is Pawn pawn)
            {
                Stance_Busy stance = StartChannel(pawn, aimCell, p.durationTicks);
                if (stance != null)
                {
                    beam.AttachChannel(pawn, stance);
                }
            }
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(caster.Position, map));
            }
            return beam;
        }

        // ========== 引导状态（持续技能占用施法者，射线/龙息等共用） ==========

        /// <summary>
        /// 让 pawn 进入引导状态：挂专用引导作业（原地不动、面朝目标）+ 忙碌姿态（禁止开火）。
        /// 返回忙碌姿态实例（null = 未能进入引导，如缺少 JobDef）。
        /// </summary>
        public static Stance_Busy StartChannel(Pawn pawn, LocalTargetInfo focus, int durationTicks)
        {
            if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.Downed)
            {
                return null;
            }
            if (ChannelJobDef == null)
            {
                Log.Error("[MMT] 找不到 " + ChannelJobDefName + " 定义，无法进入技能引导状态");
                return null;
            }
            Job job = JobMaker.MakeJob(ChannelJobDef, focus);
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            Stance_Busy stance = new Stance_MMTChannel(durationTicks + 30, focus, null) { neverAimWeapon = true };
            pawn.stances.SetStance(stance);
            return stance;
        }

        /// <summary>引导是否已被打断（作业被替换/姿态被替换/倒地死亡等）。</summary>
        public static bool ChannelBroken(Pawn pawn, Stance_Busy stance)
        {
            return pawn == null || pawn.Destroyed || !pawn.Spawned || pawn.Dead || pawn.Downed
                || pawn.CurJob == null || pawn.CurJob.def != ChannelJobDef
                || pawn.stances.curStance != stance;
        }

        /// <summary>引导自然结束：结束引导作业并令忙碌姿态下一 tick 自然过期。</summary>
        public static void EndChannel(Pawn pawn, Stance_Busy stance)
        {
            if (pawn == null || pawn.Destroyed)
            {
                return;
            }
            if (stance != null && pawn.stances.curStance == stance)
            {
                stance.ticksLeft = 1;
            }
            if (pawn.CurJob != null && ChannelJobDef != null && pawn.CurJob.def == ChannelJobDef)
            {
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        }

        /// <summary>外部打断引导（如施法者开始另一个技能）：结束引导作业，被占用的持续实体下 tick 自检断开。</summary>
        public static void BreakChannel(Thing caster)
        {
            if (caster is Pawn pawn && pawn.CurJob != null && ChannelJobDef != null && pawn.CurJob.def == ChannelJobDef)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        /// <summary>瞄准期间画半透明光柱预览（与实射同一 raycast，所见即所得）。
        /// origin 须为已前移的出膛点（用 MuzzleOrigin 取），本方法原样绘制不做偏移。
        /// headTexPath/tailTexPath 非空时在出膛点/终点补画发射头与命中端；
        /// headVertical 时发射头改画地面喷发火柱预览（与实射同一几何）。</summary>
        public static void DrawBeamPreview(Map map, Vector3 origin, Vector3 dir, float range, float width, string texPath, Color color,
            string headTexPath = null, string tailTexPath = null, float headSize = 0f, float tailSize = 0f,
            bool headVertical = false, float headPillarHeight = 1.1f, int headPillarLayers = 6, float headPillarTopScale = 0.55f)
        {
            if (map == null || map != Find.CurrentMap || dir.sqrMagnitude < 0.01f)
            {
                return;
            }
            float length = BeamLength(map, origin, dir, range);
            if (length < MarchStep)
            {
                return;
            }
            float alt = AltitudeLayer.MoteOverhead.AltitudeFor();
            Quaternion rot = Quaternion.AngleAxis(dir.AngleFlat(), Vector3.up);
            Material mat = PreviewMaterial(texPath, color);
            if (mat != null)
            {
                Vector3 center = origin + dir * (length * 0.5f);
                center.y = alt;
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(center, rot, new Vector3(width, 1f, length)), mat, 0);
            }
            if (headVertical && headSize > 0f)
            {
                Material headMat = PreviewMaterial(headTexPath, color);
                if (headMat != null)
                {
                    DrawPillarStack(origin, headMat, headSize, headPillarHeight, headPillarLayers, headPillarTopScale,
                        Time.realtimeSinceStartup);
                }
            }
            else
            {
                DrawPreviewCap(headTexPath, color, origin, rot, alt, headSize);
            }
            DrawPreviewCap(tailTexPath, color, origin + dir * length, rot, alt, tailSize);
        }

        /// <summary>地面喷发火柱：layers 片水平碟从地面堆叠 height 高，尺寸向顶端收至 topScale 倍，
        /// 相位错开的正弦缩放模拟能量上涌（共享材质无法逐片改 alpha，动画只能做在缩放上，
        /// 见 ChargingStation.DrawBeam）。basePos.y 忽略，从近地高度起叠；俯视投影即垂直柱体。</summary>
        public static void DrawPillarStack(Vector3 basePos, Material mat, float baseSize, float height, int layers, float topScale, float time)
        {
            if (mat == null || baseSize <= 0f)
            {
                return;
            }
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : new Vector3(0f, -1f, 0f);
            float baseY = AltitudeLayer.Item.AltitudeFor() + 0.02f;
            int n = Mathf.Max(2, layers);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                float y = baseY + t * height;
                float s = Mathf.Lerp(baseSize, baseSize * topScale, t) * (1f + 0.15f * Mathf.Sin(time * 4.2f + i * 1.1f));
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(PerspectiveCompensate(basePos, y, camForward), Quaternion.identity, new Vector3(s, 1f, s)),
                    mat, 0, null, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off, false);
            }
        }

        /// <summary>透视补偿：抬高 y 的面片向相机方向平移，使俯视投影仍落在原地面格（抄自 Building_Singularity）。</summary>
        public static Vector3 PerspectiveCompensate(Vector3 center, float y, Vector3 camForward)
        {
            if (camForward.y >= -0.0001f)
            {
                return new Vector3(center.x, y, center.z);
            }
            float t = y / camForward.y; // f.y<0 → 沿水平前向反向（朝相机）平移
            return new Vector3(center.x + t * camForward.x, y, center.z + t * camForward.z);
        }

        private static void DrawPreviewCap(string texPath, Color color, Vector3 pos, Quaternion rot, float alt, float size)
        {
            if (texPath.NullOrEmpty() || size <= 0f)
            {
                return;
            }
            Material mat = PreviewMaterial(texPath, color);
            if (mat == null)
            {
                return;
            }
            pos.y = alt;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, rot, new Vector3(size, 1f, size)), mat, 0);
        }

        private static Material PreviewMaterial(string texPath, Color color)
        {
            string key = (texPath ?? "") + "|" + color;
            if (!previewMats.TryGetValue(key, out Material mat) || mat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(texPath, false);
                if (tex == null)
                {
                    return null;
                }
                mat = new Material(ShaderDatabase.TransparentPostLight) { color = color, mainTexture = tex };
                previewMats[key] = mat;
            }
            return mat;
        }
    }

    /// <summary>
    /// 持续射线实体：Attach 类无碰撞 Thing，存活期间逐帧自绘拉伸光柱、按间隔对经过格子跳伤。
    /// 参数全为运行时传入不进存档；读档后参数丢失即自毁（射线最长数秒，可接受）。
    /// </summary>
    public class Thing_MMTSkillBeam : Thing
    {
        private Thing caster;
        private Vector3 origin;
        private Vector3 dir;
        private float length;
        private float angle;
        private MMTBeamParams beamParams;
        private int durationTotal;
        private int ticksLeft;
        private int ticksUntilDamage;
        private Material beamMat;
        private Material headMat;
        private Material tailMat;
        /// <summary>引导状态（第十四步）：占用施法者的射线记录其作业/姿态，打断即断线。</summary>
        private Pawn channelPawn;
        private Stance_Busy channelStance;

        public void Setup(Thing caster, Vector3 origin, Vector3 dir, float length, MMTBeamParams p)
        {
            this.caster = caster;
            this.origin = origin;
            this.dir = dir;
            this.length = length;
            angle = dir.AngleFlat();
            beamParams = p;
            durationTotal = Mathf.Max(1, p.durationTicks);
            ticksLeft = durationTotal;
            ticksUntilDamage = 1;
        }

        /// <summary>挂引导状态：之后每 tick 校验施法者仍在引导，打断即断线。</summary>
        public void AttachChannel(Pawn pawn, Stance_Busy stance)
        {
            channelPawn = pawn;
            channelStance = stance;
        }

        protected override void Tick()
        {
            base.Tick();
            if (beamParams == null || Map == null)
            {
                Destroy();
                return;
            }
            // 引导校验：移动/下令/被眩晕等打断立刻切断射线
            if (channelPawn != null && MMTBeamUtil.ChannelBroken(channelPawn, channelStance))
            {
                if (channelPawn != null && channelPawn.Faction == Faction.OfPlayer)
                {
                    Messages.Message("【MMT】持续射线被打断！", channelPawn, MessageTypeDefOf.NeutralEvent, false);
                }
                Destroy();
                return;
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                if (channelPawn != null)
                {
                    MMTBeamUtil.EndChannel(channelPawn, channelStance);
                }
                Destroy();
                return;
            }
            if (--ticksUntilDamage <= 0)
            {
                ticksUntilDamage = Mathf.Max(1, beamParams.damageIntervalTicks);
                DamagePulse();
            }
        }

        /// <summary>一跳伤害：沿光柱步进收集经过的格子，对格内 pawn/建筑结算伤害并可点燃。</summary>
        private void DamagePulse()
        {
            Map map = Map;
            HashSet<IntVec3> seen = new HashSet<IntVec3>();
            DamageDef damageDef = beamParams.damageDef ?? DamageDefOf.Burn;
            for (float d = MMTBeamUtil.MarchStep; d <= length; d += MMTBeamUtil.MarchStep)
            {
                IntVec3 cell = (origin + dir * d).ToIntVec3();
                if (!cell.InBounds(map) || !seen.Add(cell) || cell == caster?.Position)
                {
                    continue;
                }
                List<Thing> things = cell.GetThingList(map);
                for (int i = things.Count - 1; i >= 0; i--)
                {
                    Thing t = things[i];
                    if (t == this || t == caster || (!(t is Pawn) && !(t is Building)))
                    {
                        continue;
                    }
                    t.TakeDamage(new DamageInfo(damageDef, beamParams.damage, beamParams.armorPenetration,
                        angle, caster, null, beamParams.equipment?.def));
                }
                if (beamParams.igniteChance > 0f && Rand.Chance(beamParams.igniteChance))
                {
                    FireUtility.TryStartFireIn(cell, map, 0.15f, caster);
                }
            }
        }

        private static Material LoadCapMat(string texPath)
        {
            if (texPath.NullOrEmpty())
            {
                return null;
            }
            Texture2D tex = ContentFinder<Texture2D>.Get(texPath, false);
            if (tex == null)
            {
                return null;
            }
            return new Material(ShaderDatabase.TransparentPostLight) { mainTexture = tex };
        }

        /// <summary>自绘光柱三段式：发射头（出膛点光球，或 headVertical 时的地面喷发火柱）+
        /// 中段（拉伸四边形）+ 命中端（终点爆点），共用同一 tint/透明度；
        /// 中段宽度呼吸抖动 + 整体淡入淡出，命中端独立脉冲。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (beamParams == null)
            {
                return;
            }
            if (beamMat == null)
            {
                Texture2D tex = ContentFinder<Texture2D>.Get(beamParams.texPath, false);
                if (tex == null)
                {
                    return;
                }
                beamMat = new Material(ShaderDatabase.TransparentPostLight) { mainTexture = tex };
                headMat = LoadCapMat(beamParams.headTexPath);
                tailMat = LoadCapMat(beamParams.tailTexPath);
            }
            int elapsed = durationTotal - ticksLeft;
            float alpha = Mathf.Clamp01(elapsed / 10f) * Mathf.Clamp01(ticksLeft / 25f);
            float shimmer = 1f + 0.08f * Mathf.Sin(elapsed * 0.55f);
            Color color = beamParams.tint;
            color.a *= alpha;
            float alt = AltitudeLayer.MoteOverhead.AltitudeFor();
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            beamMat.color = color;
            Vector3 center = origin + dir * (length * 0.5f);
            center.y = alt;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(center, rot, new Vector3(beamParams.width * shimmer, 1f, length)), beamMat, 0);
            // 发射头：垂直火柱模式 = 多层水平碟从地面堆叠（贴地、垂直向上喷发）；
            // 否则旧式平躺光球（随中段同步呼吸）
            if (headMat != null && beamParams.headSize > 0f)
            {
                headMat.color = color;
                if (beamParams.headVertical)
                {
                    MMTBeamUtil.DrawPillarStack(origin, headMat, beamParams.headSize * shimmer,
                        beamParams.headPillarHeight, beamParams.headPillarLayers, beamParams.headPillarTopScale,
                        elapsed / 60f);
                }
                else
                {
                    Vector3 muzzle = origin;
                    muzzle.y = alt;
                    float headDraw = beamParams.headSize * shimmer;
                    Graphics.DrawMesh(MeshPool.plane10,
                        Matrix4x4.TRS(muzzle, rot, new Vector3(headDraw, 1f, headDraw)), headMat, 0);
                }
            }
            // 命中端：终点爆点（独立快速脉冲，营造灼烧溅射感）
            if (tailMat != null && beamParams.tailSize > 0f)
            {
                tailMat.color = color;
                Vector3 impact = origin + dir * length;
                impact.y = alt;
                float tailDraw = beamParams.tailSize * (1f + 0.15f * Mathf.Sin(elapsed * 0.8f));
                Graphics.DrawMesh(MeshPool.plane10,
                    Matrix4x4.TRS(impact, rot, new Vector3(tailDraw, 1f, tailDraw)), tailMat, 0);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (beamMat != null)
            {
                Object.Destroy(beamMat);
                beamMat = null;
            }
            if (headMat != null)
            {
                Object.Destroy(headMat);
                headMat = null;
            }
            if (tailMat != null)
            {
                Object.Destroy(tailMat);
                tailMat = null;
            }
            base.Destroy(mode);
        }
    }

    // ========== pawn 技能出口：持续射线 ==========

    public class CompProperties_MMTBeam : CompProperties_AbilityEffect
    {
        public string texPath = "Things/Mote/MMT_SkillBeam";
        public Color tint = new Color(1f, 0.55f, 0.15f);
        public float width = 0.7f;
        /// <summary>出膛偏移（格）：光柱起点前移到施法者身前，避免与小人重叠。</summary>
        public float muzzleOffset = 0.9f;
        /// <summary>发射头/命中端贴图（空 = 不画）与绘制尺寸（格）。</summary>
        public string headTexPath = "Things/Mote/MMT_SkillBeam_Head";
        public string tailTexPath = "Things/Mote/MMT_SkillBeam_Tail";
        public float headSize = 1.4f;
        /// <summary>true = 发射头画成地面喷发火柱（多层水平碟堆叠，贴地、垂直向上）；false = 平躺光球。</summary>
        public bool headVertical;
        /// <summary>火柱高度（世界单位）/碟片层数/顶端缩放系数（相对底部）。</summary>
        public float headPillarHeight = 1.1f;
        public int headPillarLayers = 6;
        public float headPillarTopScale = 0.55f;
        public float tailSize = 1.6f;
        public float range = 20f;
        public int durationTicks = 240;
        public string damageDefName = "Burn";
        public float damage = 4f;
        public int damageIntervalTicks = 10;
        public float armorPenetration;
        public float igniteChance = 0.1f;
        public string soundDefName;

        public CompProperties_MMTBeam()
        {
            compClass = typeof(CompAbilityEffect_MMTBeam);
        }
    }

    /// <summary>技能引导姿态：Stance_Busy 在 1.7 为抽象类，引导状态用本具体子类实例化（自然过期即回移动姿态）。</summary>
    public class Stance_MMTChannel : Stance_Busy
    {
        public Stance_MMTChannel()
        {
        }

        public Stance_MMTChannel(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
        }
    }

    /// <summary>
    /// 技能引导作业（第十四步）：原地不动、面朝目标，永不自然完成——
    /// 由持续实体（射线等）在自然结束时结束本作业；任何移动/下令都会替换本作业从而打断引导。
    /// </summary>
    public class JobDriver_MMTChannel : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = ToilMaker.MakeToil("MMTChannel");
            toil.initAction = delegate { pawn.pather.StopDead(); };
            toil.tickAction = delegate
            {
                if (job.targetA.IsValid)
                {
                    pawn.rotationTracker.FaceTarget(job.targetA);
                }
            };
            toil.handlingFacing = true;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }

    /// <summary>持续射线技能：确认目标方向后从施法者射出持续光柱（读条圆走 CastBar 框架）。</summary>
    public class CompAbilityEffect_MMTBeam : CompAbilityEffect
    {
        public new CompProperties_MMTBeam Props => (CompProperties_MMTBeam)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTBeamUtil.FireBeam(caster, target.Cell, new MMTBeamParams
            {
                texPath = Props.texPath,
                tint = Props.tint,
                width = Props.width,
                muzzleOffset = Props.muzzleOffset,
                headTexPath = Props.headTexPath,
                tailTexPath = Props.tailTexPath,
                headSize = Props.headSize,
                headVertical = Props.headVertical,
                headPillarHeight = Props.headPillarHeight,
                headPillarLayers = Props.headPillarLayers,
                headPillarTopScale = Props.headPillarTopScale,
                tailSize = Props.tailSize,
                range = Props.range,
                durationTicks = Props.durationTicks,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                damageIntervalTicks = Props.damageIntervalTicks,
                armorPenetration = Props.armorPenetration,
                igniteChance = Props.igniteChance,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }

        /// <summary>瞄准期间画光柱路径预览。</summary>
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);
            if (!target.IsValid)
            {
                return;
            }
            Pawn caster = parent.pawn;
            Vector3 dir = MMTBeamUtil.BeamDirection(caster, target.Cell);
            Color preview = Props.tint;
            preview.a = 0.3f;
            MMTBeamUtil.DrawBeamPreview(caster.MapHeld, MMTBeamUtil.MuzzleOrigin(caster, dir, Props.muzzleOffset), dir,
                Props.range, Props.width, Props.texPath, preview,
                Props.headTexPath, Props.tailTexPath, Props.headSize, Props.tailSize,
                Props.headVertical, Props.headPillarHeight, Props.headPillarLayers, Props.headPillarTopScale);
        }
    }
}
