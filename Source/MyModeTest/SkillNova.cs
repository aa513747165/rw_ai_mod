using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 新星环框架（第二十二步：360° 扩散冲击波） ==========
    // 以施法点为中心，一个圆环向外匀速扩散，命中扩散路径上的 pawn：
    // 每 tick 判定当前环带（|距离-当前半径| ≤ 带宽/2）内的目标，默认每目标只命中一次
    // （已命中集合去重），可参数化为环带内持续判定。命中双效：友方吃治疗池、敌方吃伤害
    // （神圣新星就是这套参数皮肤；纯伤害/纯治疗新星把另一项置 0 即可）。
    // 与第五步落雷（圆域一次性）和第十五步龙息（锥形）互补：这是"全向扩散环"形态。
    // 视觉：自绘贴图圆环（白图代码染色），半径 0→最大线性插值（不旋转），
    // 波峰最亮 + 通过后短拖尾淡出（0.3s 尾迹），到达最大半径后整体淡出自毁。
    // MMTNovaUtil.FireNova 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTNova；
    // 建筑演示侧出口 demoType="nova"（以自身为中心，免瞄准）。
    // 注：参数运行时传入不进存档，读档后首个 tick 自毁（存活仅数秒）。

    /// <summary>新星环参数包（MMTNovaUtil.FireNova 的输入）。</summary>
    public class MMTNovaParams
    {
        /// <summary>最大扩散半径（格）。</summary>
        public float maxRadius = 6f;
        /// <summary>扩散速度（格/秒，匀速）。</summary>
        public float cellsPerSecond = 3f;
        /// <summary>命中环带宽度（格，判定 |距离-当前半径| ≤ 带宽/2）。</summary>
        public float bandWidth = 1f;
        /// <summary>伤害类型（null = Burn 圣光灼烧）。</summary>
        public DamageDef damageDef;
        /// <summary>敌方命中伤害（0 = 不伤敌）。</summary>
        public float damage = 15f;
        public float armorPenetration;
        /// <summary>友方命中治疗池（0 = 不治友）。</summary>
        public float healAmount = 8f;
        /// <summary>true = 每目标只命中一次（去重）；false = 环带内持续判定。</summary>
        public bool hitOnce = true;
        /// <summary>到达最大半径后的整体淡出 tick（0.3s ≈ 18）。</summary>
        public int fadeOutTicks = 18;
        /// <summary>环贴图（白图圆环，代码染色）。</summary>
        public string texPath = "Things/Mote/MMT_NovaRing";
        /// <summary>环染色（默认圣光金）。</summary>
        public Color tint = new Color(1f, 0.85f, 0.4f);
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>新星环统一入口。</summary>
    public static class MMTNovaUtil
    {
        public const string NovaDefName = "MMT_NovaRing";

        /// <summary>以 center 为圆心爆发新星环，返回环实体（参数无效返回 null）。</summary>
        public static Thing_MMTNovaRing FireNova(Thing caster, IntVec3 center, MMTNovaParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || p.maxRadius <= 0f || p.cellsPerSecond <= 0f || !center.IsValid)
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(NovaDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + NovaDefName + " 定义，无法施放新星环");
                return null;
            }
            Thing_MMTNovaRing nova = (Thing_MMTNovaRing)GenSpawn.Spawn(def, center, map);
            nova.Setup(caster, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(center, map));
            }
            return nova;
        }
    }

    /// <summary>
    /// 新星环实体：圆心固定在施放点（不跟随施法者），半径匀速增大；
    /// 每 tick 判定当前环带内 pawn（敌伤友疗，默认命中一次去重）；
    /// 自绘波峰环 + 短拖尾，到达最大半径后淡出自毁。
    /// 参数运行时传入不进存档；读档后首个 tick 自毁。
    /// </summary>
    public class Thing_MMTNovaRing : Thing
    {
        private Thing caster;
        private Faction casterFaction;
        private MMTNovaParams novaParams;
        private float radius;
        private int fadeTicksLeft = -1;
        private bool loadedFromSave;
        private readonly HashSet<Pawn> hitPawns = new HashSet<Pawn>();

        // 全部新星环共享同贴图材质；同帧多枚环的颜色/透明度必须逐次 MaterialPropertyBlock 传
        // （共享材质颜色是引用语义，见 Thing_MMTGroundZone 注释）
        private static readonly Dictionary<string, Material> sharedMats = new Dictionary<string, Material>();
        private static MaterialPropertyBlock matProps;

        public void Setup(Thing caster, MMTNovaParams p)
        {
            this.caster = caster;
            casterFaction = caster?.Faction;
            novaParams = p;
            // 爆发瞬间中心闪光（复用折跃闪光桶，跟随环染色）
            FleckDef flash = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BlinkFlash");
            if (flash != null && caster?.Map == Find.CurrentMap)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(Position.ToVector3Shifted(), caster.Map, flash, 1.6f);
                data.instanceColor = p.tint;
                caster.Map.flecks.CreateFleck(data);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || novaParams == null || Map == null)
            {
                Destroy();
                return;
            }
            if (fadeTicksLeft >= 0)
            {
                // 已到最大半径：整体淡出
                if (--fadeTicksLeft <= 0)
                {
                    Destroy();
                }
                return;
            }
            radius += novaParams.cellsPerSecond / 60f;
            if (radius >= novaParams.maxRadius)
            {
                radius = novaParams.maxRadius;
                fadeTicksLeft = Mathf.Max(1, novaParams.fadeOutTicks);
            }
            HitBand();
        }

        /// <summary>当前环带（|距离-当前半径| ≤ 带宽/2）内的 pawn 结算：友方治疗、敌方伤害。</summary>
        private void HitBand()
        {
            Map map = Map;
            float halfBand = Mathf.Max(0.05f, novaParams.bandWidth * 0.5f);
            DamageDef damageDef = novaParams.damageDef ?? DamageDefOf.Burn;
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Dead || (novaParams.hitOnce && hitPawns.Contains(p)))
                {
                    continue;
                }
                Vector3 offset = p.Position.ToVector3Shifted() - Position.ToVector3Shifted();
                offset.y = 0f;
                if (Mathf.Abs(offset.magnitude - radius) > halfBand)
                {
                    continue;
                }
                hitPawns.Add(p);
                if (IsFriendly(p))
                {
                    if (novaParams.healAmount > 0f)
                    {
                        MMTHealUtil.ApplyOne(p, new MMTHealParams
                        {
                            healAmount = novaParams.healAmount,
                            iconTint = novaParams.tint,
                            particleTint = novaParams.tint,
                            particleFleckDef = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark")
                        });
                    }
                }
                else if (novaParams.damage > 0f && p.Faction != null && p.Faction != casterFaction
                    && caster != null && p.HostileTo(caster))
                {
                    p.TakeDamage(new DamageInfo(damageDef, novaParams.damage, novaParams.armorPenetration,
                        -1f, caster.Destroyed ? null : caster, null, novaParams.equipment?.def));
                }
            }
        }

        /// <summary>友方判定（语义同瞄准框架：同派系，或无派系且无原属派系的动物）。</summary>
        private bool IsFriendly(Pawn p)
        {
            return p.Faction == casterFaction
                || (p.Faction == null && p.RaceProps.Animal && casterFaction != null && p.HostFaction == null);
        }

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

        /// <summary>自绘新星环：波峰环最亮 + 通过后 0.3s 尾迹淡出（3 层拖尾递减），尾段整体淡出。
        /// 不旋转（径向扩散），半径匀速线性增大（缩放=半径 0→最大）。</summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (novaParams == null || Map != Find.CurrentMap)
            {
                return;
            }
            Material mat = SharedMat(novaParams.texPath);
            if (mat == null)
            {
                return;
            }
            if (matProps == null)
            {
                matProps = new MaterialPropertyBlock();
            }
            float master = 1f;
            if (fadeTicksLeft >= 0)
            {
                master = Mathf.Clamp01(fadeTicksLeft / (float)Mathf.Max(1, novaParams.fadeOutTicks));
            }
            Vector3 pos = Position.ToVector3Shifted();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            // 拖尾总长 = 扩散速度 × 0.3s（波通过后 0.3s 内淡出的视觉表达），均分 3 层递减
            float trailLen = novaParams.cellsPerSecond * 0.3f;
            for (int layer = 0; layer <= 3; layer++)
            {
                float r = radius - trailLen * layer / 3f;
                if (r < 0.05f)
                {
                    continue;
                }
                float alpha = layer == 0 ? 0.85f : 0.45f / layer;
                DrawRingQuad(mat, pos, r, novaParams.tint, alpha * master);
            }
        }

        private static void DrawRingQuad(Material mat, Vector3 pos, float r, Color tint, float alpha)
        {
            Color color = tint;
            color.a = Mathf.Clamp01(alpha);
            mat.color = color;
            matProps.SetColor("_Color", color);
            float size = r * 2f;
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)), mat, 0, null, 0, matProps);
        }
    }

    // ========== pawn 技能出口：新星环 ==========

    public class CompProperties_MMTNova : CompProperties_AbilityEffect
    {
        public float maxRadius = 6f;
        public float cellsPerSecond = 3f;
        public float bandWidth = 1f;
        public string damageDefName;
        public float damage = 15f;
        public float armorPenetration;
        public float healAmount = 8f;
        public bool hitOnce = true;
        public int fadeOutTicks = 18;
        public string texPath = "Things/Mote/MMT_NovaRing";
        public Color tint = new Color(1f, 0.85f, 0.4f);
        public string soundDefName;

        public CompProperties_MMTNova()
        {
            compClass = typeof(CompAbilityEffect_MMTNova);
        }
    }

    /// <summary>新星环技能：以自身为中心爆发扩散冲击波（友疗敌伤）。</summary>
    public class CompAbilityEffect_MMTNova : CompAbilityEffect
    {
        public new CompProperties_MMTNova Props => (CompProperties_MMTNova)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTNovaUtil.FireNova(caster, caster.Position, new MMTNovaParams
            {
                maxRadius = Props.maxRadius,
                cellsPerSecond = Props.cellsPerSecond,
                bandWidth = Props.bandWidth,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                damage = Props.damage,
                armorPenetration = Props.armorPenetration,
                healAmount = Props.healAmount,
                hitOnce = Props.hitOnce,
                fadeOutTicks = Props.fadeOutTicks,
                texPath = Props.texPath,
                tint = Props.tint,
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
