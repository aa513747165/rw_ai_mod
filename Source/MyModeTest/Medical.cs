using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MyModeTest
{
    // =====================================================================
    // 战地医疗装备链（已获「超级医疗科技」mod 授权移植，原作 workshop 3832201245）
    //   - 战地医疗背包：Verb_MMTMedPackLaunch 投掷 Projectile_MMTMedBurst 气雾胶囊；
    //   - 小型治疗炮塔：Building_MMTHealTurret 发射 Projectile_MMTHealBolt 医疗软子弹；
    //   - 共用：Hediff_MMTNaniteHealing（伤口愈合 ×5）与 MedUtil 治疗工具函数。
    // 机制与数值照搬原作，仅改名换前缀；新增的两条 CompRefuelable 补丁见文件末尾。
    // =====================================================================

    /// <summary>战地医疗链通用工具。</summary>
    public static class MedUtil
    {
        /// <summary>不吃纳米医疗的 hediff（按 defName 比对，DLC 缺失时无害）。</summary>
        private static readonly string[] ExcludedHediffDefNames = { "Ghoul", "Shambler" };

        /// <summary>
        /// 可被纳米医疗机器人治疗的对象：一切有机体（IsFlesh：人/动物/实体都行），
        /// 只排除食尸鬼、蹒跚怪。
        /// 注意：不能用 bloodDef 判——机械族的 bloodDef 是 Filth_MachineBits，非空。
        /// </summary>
        public static bool IsMedHealable(Pawn p)
        {
            if (p == null || p.Dead || p.RaceProps == null) return false;
            if (!p.RaceProps.IsFlesh) return false;
            if (HasExcludedHediff(p)) return false;
            return true;
        }

        private static bool HasExcludedHediff(Pawn p)
        {
            var hs = p.health?.hediffSet;
            if (hs == null) return false;
            for (int i = 0; i < hs.hediffs.Count; i++)
                for (int j = 0; j < ExcludedHediffDefNames.Length; j++)
                    if (hs.hediffs[i].def.defName == ExcludedHediffDefNames[j]) return true;
            return false;
        }

        /// <summary>包扎目标所有正在流血的伤口（治疗质量 min~max 随机）。医疗气雾用。</summary>
        public static void TendBleedingWounds(Pawn p, float minQuality = 0.3f, float maxQuality = 0.6f)
        {
            var set = p?.health?.hediffSet;
            if (set == null) return;
            float quality = Rand.Range(minQuality, maxQuality);
            var hediffs = set.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h.Bleeding && h.TendableNow()) h.Tended(quality, 1f, 0);
            }
        }

        /// <summary>
        /// 只包扎**一个正在流血、且还没被治疗过**的伤口（治疗质量 min~max 随机），返回是否真的包了。
        /// 小型治疗炮塔用：一次命中包一处。覆盖 Hediff_Injury 与 Hediff_MissingPart（断肢同样会流血、可包扎）。
        /// 只挑流血最快的那个来包（BleedRate 最高）。骨折/瘀伤这类不流血的伤不包扎。
        /// </summary>
        public static bool TendOneBleedingWound(Pawn p, float minQuality = 0.2f, float maxQuality = 0.5f)
        {
            var set = p?.health?.hediffSet;
            if (set == null) return false;

            Hediff worst = null;
            float worstBleed = 0f;
            var hediffs = set.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (!(h is Hediff_Injury) && !(h is Hediff_MissingPart)) continue;
                if (h is Hediff_Injury && h.Severity <= 0f) continue;
                if (h.BleedRate <= 0f) continue;                    // 只处理正在流血的

                HediffComp_TendDuration tend = h.TryGetComp<HediffComp_TendDuration>();
                if (tend != null && tend.IsTended) continue;         // 已经包扎过的不重复包
                if (!h.TendableNow(ignoreTimer: true)) continue;      // 不可包扎的（老疤痕等）排除

                if (worst == null || h.BleedRate > worstBleed)
                {
                    worst = h;
                    worstBleed = h.BleedRate;
                }
            }
            if (worst == null) return false;

            worst.Tended(Rand.Range(minQuality, maxQuality), 1f, 0);
            return true;
        }
    }

    /// <summary>
    /// 战地医疗背包的投掷技能。
    /// 关键：继承 Verb_LaunchProjectileStatic 而不是 OneUse——OneUse.TryCastShot 会无条件
    /// SelfConsume（销毁装备本体），医疗背包要可装填、不销毁。
    /// 充能消耗由爷爷类 Verb_LaunchProjectile.TryCastShot 内的 UsedOnce 正常完成。
    /// </summary>
    public class Verb_MMTMedPackLaunch : Verb_LaunchProjectileStatic
    {
        /// <summary>优先用 verbProps.commandIcon 指定的贴图，加载失败回退到 base。</summary>
        public override Texture2D UIIcon
        {
            get
            {
                string path = verbProps?.commandIcon;
                if (!path.NullOrEmpty())
                {
                    Texture2D tex = ContentFinder<Texture2D>.Get(path, false);
                    if (tex != null) return tex;
                }
                return base.UIIcon;
            }
        }

        /// <summary>射程圈只画有视线的弧段；落点爆散圈沿用原版（自带墙体遮挡）。</summary>
        public override void DrawHighlight(LocalTargetInfo target)
        {
            Pawn caster = CasterPawn;
            Map map = caster?.Map;
            if (caster != null && map != null)
            {
                IntVec3 pos = caster.Position;
                float range = EffectiveRange;
                GenDraw.DrawRadiusRing(pos, range, Color.white,
                    (IntVec3 c) => GenSight.LineOfSight(pos, c, map));
            }
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                DrawHighlightFieldRadiusAroundTarget(target);
            }
        }

        public override float EffectiveRange
        {
            get
            {
                Thing src = EquipmentSource;
                if (src != null)
                {
                    float r = 0f;
                    try { r = src.GetStatValue(StatDefOf.PackRadius); } catch { }
                    if (r > 0f) return r;
                }
                return verbProps.range;
            }
        }

        /// <summary>
        /// 放宽落点校验：胶囊是飞过头顶的抛射物，落点不需要可站立/无建筑。
        /// 保留充能可用性检查（0 充能时不可发射）。
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!target.IsValid) return false;
            Map map = caster?.Map;
            if (map == null) return false;
            if (target.HasThing)
            {
                if (target.Thing == null || !target.Thing.Spawned || target.Thing.Map != map) return false;
            }
            else if (!target.Cell.InBounds(map) || target.Cell.Fogged(map)) return false;

            Pawn pawn = CasterPawn;
            if (pawn != null && EquipmentSource != null
                && !RimWorld.Utility.ReloadableUtility.CanUseConsideringQueuedJobs(pawn, EquipmentSource, showMessages))
                return false;
            return true;
        }
    }

    /// <summary>
    /// 医疗气雾胶囊：落地后在落点半径内释放纳米医疗机器人气雾：
    ///   1) 范围内所有生物 pawn 的流血伤口立刻全部包扎（治疗质量 30%~60% 随机，每个伤员掷一次）；
    ///   2) 附加 hediff「纳米医疗机器人生效」：持续 1 小时，伤口愈合速度 ×5；重复命中只刷新剩余时间。
    /// 视觉为双层雾结构（浓雾 + 柔雾贴图余雾），深青绿色；不流血污、不进气体网格。
    /// </summary>
    public class Projectile_MMTMedBurst : Projectile
    {
        /// <summary>医疗气雾主色：006B6B 深青绿。</summary>
        private static readonly Color MedMistColor = new Color(0f, 0.42f, 0.42f);

        /// <summary>纳米 hediff 持续 1 小时 = 2500 tick（与 XML 的 disappearsAfterTicks 保持一致）。</summary>
        private const int NaniteTicks = 2500;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 cell = Position;

            Destroy(DestroyMode.Vanish);

            if (map == null || map.Disposed || !cell.IsValid) return;

            float radius = def?.projectile?.explosionRadius > 0f ? def.projectile.explosionRadius : 3f;
            DoMedicalBurst(cell, map, radius);
        }

        public static void DoMedicalBurst(IntVec3 cell, Map map, float radius)
        {
            if (map == null || !cell.IsValid) return;

            SpawnMedMist(cell, map, radius);

            SoundDef snd = DefDatabase<SoundDef>.GetNamedSilentFail("Explosion_FirefoamPopPack")
                        ?? DefDatabase<SoundDef>.GetNamedSilentFail("Explosion_FirefoamPopper");
            if (snd != null) SoundStarter.PlayOneShot(snd, new TargetInfo(cell, map));

            HediffDef naniteDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_NaniteHealing");

            foreach (Thing t in GenRadial.RadialDistinctThingsAround(cell, map, radius, true))
            {
                if (!(t is Pawn p) || p.Dead || !MedUtil.IsMedHealable(p)) continue;

                // 气雾是"散出去"的，被墙挡住就覆盖不到
                if (!GenSight.LineOfSight(cell, p.Position, map)) continue;

                // 立刻包扎所有流血伤口：质量 30%~60% 随机（每个伤员掷一次）
                MedUtil.TendBleedingWounds(p);

                // 附加/刷新纳米医疗机器人 hediff
                if (naniteDef != null)
                {
                    Hediff existing = p.health.hediffSet.GetFirstHediffOfDef(naniteDef);
                    if (existing == null)
                    {
                        p.health.AddHediff(naniteDef);
                    }
                    else
                    {
                        HediffComp_Disappears dis = existing.TryGetComp<HediffComp_Disappears>();
                        if (dis != null) dis.ticksToDisappear = NaniteTicks;
                    }
                }
            }
        }

        /// <summary>
        /// 深绿色医疗气雾：两层结构（DustPuffThick 浓雾 + 柔雾贴图余雾），
        /// 密度按到爆点的距离衰减；视线之外不生成，与实际作用范围一致。
        /// </summary>
        private static void SpawnMedMist(IntVec3 cell, Map map, float radius)
        {
            FleckDef[] variants = FleckCache;
            if (variants == null || variants.Length == 0) return;

            float safeRadius = Mathf.Max(radius, 1f);

            foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, radius, true))
            {
                if (!c.InBounds(map)) continue;
                if (!GenSight.LineOfSight(cell, c, map)) continue;

                float falloff = 1f - Mathf.Clamp01(cell.DistanceTo(c) / safeRadius);   // 1=中心 0=边缘
                int layers = falloff > 0.55f ? 2 : 1;
                for (int i = 0; i < layers; i++)
                {
                    FleckMaker.ThrowDustPuffThick(JitterPos(c), map, Mathf.Lerp(1.6f, 3.4f, falloff), MedMistColor);
                    FleckDef def = variants[Rand.Range(0, variants.Length)];
                    float scale = Mathf.Lerp(1.2f, 3.2f, falloff) * Rand.Range(0.85f, 1.15f);
                    FleckMaker.Static(JitterPos(c), map, def, scale);
                }
            }
        }

        private static FleckDef[] FleckCache
        {
            get
            {
                if (fleckCache != null) return fleckCache;
                var list = new List<FleckDef>(3);
                foreach (string dn in new[] { "MMT_MedMist", "MMT_MedMistMid", "MMT_MedMistShort" })
                {
                    var d = DefDatabase<FleckDef>.GetNamedSilentFail(dn);
                    if (d != null) list.Add(d);
                }
                fleckCache = list.ToArray();
                return fleckCache;
            }
        }

        private static FleckDef[] fleckCache;

        /// <summary>格内随机偏移，避免所有粒子叠在同一个点上而看不出"雾"。</summary>
        private static Vector3 JitterPos(IntVec3 c) =>
            c.ToVector3Shifted() + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.45f, 0.45f));
    }

    /// <summary>
    /// 治疗弹（医疗炮塔的"炮弹"）。
    /// 关键：不调用 base.Impact——原版 Projectile.Impact 会构造 DamageInfo 并 ApplyDamage，
    /// 这里整个跳过，永远零伤害：打中人治疗、打在墙/地上什么都不发生。
    /// 打偏时 intendedTarget 是地面格、HitFlags = NonTargetPawns，流弹会误击弹道上的其他生物：
    ///   - 原定目标（炮塔自己锁的人）→ 必须 IsHealableSide（玩家派系 / 玩家收容 / 盟友与中立）；
    ///   - 被流弹误击的 → 只要 IsMedHealable 放过就治，包括敌人（有机体、非食尸鬼/蹒跚怪/机械族）。
    /// "要不要治疗"只看阵营/血肉，不看伤情——ApplyHeal 无条件施加/刷新纳米医疗 hediff。
    /// </summary>
    public class Projectile_MMTHealBolt : Projectile
    {
        protected override void Impact(Thing hitThing, bool blockedByShield)
        {
            // blockedByShield：目标戴护盾腰包被打中时走这条；护盾挡不住治疗，照样结算。
            Pawn pawn = hitThing as Pawn;
            if (pawn == null) pawn = intendedTarget.Thing as Pawn;

            if (pawn != null)
            {
                bool isIntendedTarget = intendedTarget.Thing == pawn;
                // 原定目标按"友方"筛；流弹误击按"有没有血肉可治"筛（所以误击敌人也会治）
                bool healable = isIntendedTarget
                    ? Building_MMTHealTurret.IsHealableSide(pawn)
                    : MedUtil.IsMedHealable(pawn);

                if (healable) Building_MMTHealTurret.ApplyHeal(pawn);
            }

            Destroy(DestroyMode.Vanish);
        }
    }

    /// <summary>
    /// 纳米医疗机器人生效：括号里显示剩余时间倒计时，如「纳米医疗机器人生效中（0.6时）」。
    /// </summary>
    public class Hediff_MMTNaniteHealing : HediffWithComps
    {
        public override string LabelInBrackets
        {
            get
            {
                HediffComp_Disappears dis = this.TryGetComp<HediffComp_Disappears>();
                if (dis == null) return null;
                float hoursLeft = dis.ticksToDisappear / 2500f;
                return "MMT.HoursLeft".Translate(hoursLeft.ToString("0.#"));
            }
        }
    }

    /// <summary>
    /// 小型治疗炮塔（医疗炮台）。
    ///
    /// 底座/建造/材质照抄原版「小型机枪塔」；武器贴图 MMT_MedTurret_Top 原样使用（不旋转、不缩放），
    /// 朝向由这里自己算：贴图未旋转时炮口朝东，extraRotation 直接用 AngleFlat。
    ///
    /// 为什么不继承 Building_TurretGun：
    ///   - 它的索敌硬编码 HostileTo（只找敌人）；
    ///   - 开火前有 turretBurstWarmupTime 预热 + 瞄准音效，而"瞄准"本身不该有声音；
    ///   - 它在 Tick 里用私有 IsValidTarget 把玩家派系目标判成无效。
    /// 所以只继承 Building，自己处理：待机扫视（照抄原版 TurretTop）、瞄准、命中判定、治疗。
    ///
    /// 治疗：每命中一次 → 包扎一个正在流血的伤口（质量随机 20%~50%）+ 施加/刷新
    /// 「纳米医疗机器人生效」（与医疗背包同款 hediff，伤口愈合 ×5），但只持续 0.5 小时（1250 tick）。
    ///
    /// 弹药「医疗软子弹」：容量 8、开局满、不靠搬运；通电时每 3 小时（7500 tick）+1 发，
    /// 每开一枪 -1 发；空弹时只瞄不打。弹药量借 CompRefuelable 的 gizmo/条显示，
    /// 炮塔身上的燃料覆盖层图标被文件末尾的 CompRefuelable.PostDraw 补丁关掉。
    /// </summary>
    public class Building_MMTHealTurret : Building, IAttackTarget
    {
        /// <summary>射程（格）。</summary>
        public const float Range = 18f;
        /// <summary>射程平方：热路径距离判断用 DistanceToSquared，免开方。</summary>
        private const float RangeSq = Range * Range;
        /// <summary>两发之间的冷却（tick，240 = 4 秒）。</summary>
        private const int CooldownTicks = 240;
        /// <summary>重新索敌间隔（tick）。</summary>
        private const int ScanInterval = 30;

        /// <summary>打偏概率系数：最终命中率 = 1 - (1 - acc) × 0.75（即打偏率在原版基础上 ×1.5）。</summary>
        private const float MissChanceFactor = 0.75f;

        // ===== 原版 TurretTop 待机扫视参数（照抄）=====
        private const float IdleTurnDegreesPerTick = 0.26f;
        private const int IdleTurnDuration = 140;
        private const int IdleTurnIntervalMin = 150;
        private const int IdleTurnIntervalMax = 350;

        // ===== 精度分档（同原版 Verb：0~3 / 3~12 / 12~25 / 25+）=====
        private const float TouchRange = 3f;
        private const float ShortRange = 12f;
        private const float MediumRange = 25f;

        /// <summary>纳米医疗 hediff 的持续时间：0.5 小时。</summary>
        private const int NaniteTicks = 1250;

        private CompPowerTrader power;
        private CompFlickable flick;
        private CompRefuelable ammo;      // 医疗软子弹（CompRefuelable，只用于计数与那个耐久度 gizmo）
        private CompStunnable stun;       // EMP / 眩晕（被瘫痪时必须停火）

        private Pawn currentTarget;
        private int cooldownTicks;
        private float curRotation;
        private int ticksUntilIdleTurn;
        private int idleTurnTicksLeft;
        private bool idleTurnClockwise;
        private int rechargeTicks;        // 通电时累计到 7500 tick 补 1 发
        private bool holdFire;            // 停火 gizmo（原版炮塔同款）

        /// <summary>弹容量。</summary>
        public const int AmmoCapacity = 8;
        /// <summary>通电时每 3 小时（7500 tick）补 1 发。</summary>
        private const int RechargeTicks = 7500;

        /// <summary>
        /// 炮口朝向的贴图补偿。原版 TurretTop.ArtworkRotation 也是 -90：
        /// AngleFlat 以北为 0，而炮塔顶图未旋转时炮口朝东（=90），所以想指向方位 A 需要 θ = A - 90。
        /// </summary>
        private const float TopArtworkRotation = -90f;

        private Graphic topGraphic;
        private ThingDef healGunDef;
        private static ThingDef healBoltDef;

        public bool PoweredOn => (power == null || power.PowerOn) && (flick == null || flick.SwitchIsOn);
        public float Ammo => ammo?.Fuel ?? AmmoCapacity;

        /// <summary>被 EMP / 眩晕时（CompStunnable）：完全不工作——不瞄准、不开火、不补充、贴图也不转。</summary>
        public bool Stunned => stun != null && stun.StunHandler != null && stun.StunHandler.Stunned;

        // ===== 让敌人能主动打它（IAttackTarget） =====
        // 原版 Thing 本身不实现 IAttackTarget，建筑不会进 Map.attackTargetsCache，
        // 敌人的索敌永远看不见它。实现接口后敌人就能选中它；断电时被忽略。
        Thing IAttackTarget.Thing => this;
        LocalTargetInfo IAttackTarget.TargetCurrentlyAimingAt => currentTarget;
        float IAttackTarget.TargetPriorityFactor => 1f;
        bool IAttackTarget.ThreatDisabled(IAttackTargetSearcher disabledFor) => !PoweredOn;

        private ThingDef HealGunDef =>
            healGunDef ?? (healGunDef = DefDatabase<ThingDef>.GetNamedSilentFail("MMT_HealTurret_Gun"));

        private static ThingDef HealBoltDef =>
            healBoltDef ?? (healBoltDef = DefDatabase<ThingDef>.GetNamedSilentFail("MMT_HealBolt"));

        // 静态缓存：NeedsHeal 热路径不再每次做 DefDatabase 字典查询
        private static HediffDef naniteHediffDef;
        private static HediffDef NaniteHediffDef =>
            naniteHediffDef ?? (naniteHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("MMT_NaniteHealing"));

        // ===== 生命周期 =====

        public override void PostMake()
        {
            base.PostMake();
            CacheComps();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CacheComps();
        }

        private void CacheComps()
        {
            power = GetComp<CompPowerTrader>();
            flick = GetComp<CompFlickable>();
            ammo = GetComp<CompRefuelable>();
            stun = GetComp<CompStunnable>();
            if (ammo != null)
            {
                ammo.allowAutoRefuel = false;   // 不靠搬运，通电自动补
                // 这两个字段原版只做 Translate() 不会额外格式化；Translate 对已翻译文本幂等。
                ammo.Props.fuelLabel = "MMT.HealTurretAmmo".Translate();
                ammo.Props.fuelGizmoLabel = "MMT.HealTurretAmmoGizmo".Translate();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicks, "MMT_healTurretCooldown", 0);
            Scribe_Values.Look(ref curRotation, "MMT_healTurretRotation", 0f);
            Scribe_Values.Look(ref ticksUntilIdleTurn, "MMT_healTurretIdleTicks", 0);
            Scribe_Values.Look(ref idleTurnTicksLeft, "MMT_healTurretIdleLeft", 0);
            Scribe_Values.Look(ref idleTurnClockwise, "MMT_healTurretIdleCw", false);
            Scribe_Values.Look(ref rechargeTicks, "MMT_healTurretRecharge", 0);
            Scribe_Values.Look(ref holdFire, "MMT_healTurretHoldFire", false);
            // 目标不存盘：30 tick 内重新索敌；存档里留一个已销毁的引用会报 destroyed reference
        }

        // ===== 被摧毁时的材料返还 =====

        /// <summary>
        /// 被摧毁（KillFinalize）时按造价随机返还一部分材料（30%~60%，按堆叠上限分堆）。
        /// 掉在居住区以外的堆自动标记为禁止，免得小人跑去危险地带搬东西。
        /// </summary>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.KillFinalize && Spawned && Map != null)
            {
                try { SpawnDestroyLeavings(); }
                catch (System.Exception e) { Log.Warning($"[myModeTest] 治疗炮塔返还材料失败：{e.Message}"); }
            }
            base.Destroy(mode);
        }

        private void SpawnDestroyLeavings()
        {
            if (def.costList != null)
            {
                for (int i = 0; i < def.costList.Count; i++)
                {
                    ThingDefCountClass c = def.costList[i];
                    if (c?.thingDef == null) continue;
                    DropLeavingsOf(c.thingDef, RandomLeavingCount(c.count), null);
                }
            }
            if (def.costStuffCount > 0 && Stuff != null)
                DropLeavingsOf(Stuff, RandomLeavingCount(def.costStuffCount), Stuff);
        }

        /// <summary>30%~60% 随机（至少 0、至多不超过原数量）。</summary>
        private static int RandomLeavingCount(int original)
        {
            if (original <= 0) return 0;
            int min = Mathf.FloorToInt(original * 0.3f);
            int max = Mathf.CeilToInt(original * 0.6f);
            return Mathf.Clamp(Rand.RangeInclusive(min, max), 0, original);
        }

        private void DropLeavingsOf(ThingDef thingDef, int count, ThingDef stuff)
        {
            if (count <= 0 || thingDef == null) return;
            int perStack = Mathf.Max(1, thingDef.stackLimit);
            while (count > 0)
            {
                int n = Mathf.Min(count, perStack);
                Thing t = ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? stuff : null);
                if (t == null) return;
                t.stackCount = n;
                if (GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Near))
                {
                    // 掉在居住区外 → 自动禁止
                    if (Map.areaManager?.Home != null && !Map.areaManager.Home[t.PositionHeld])
                        t.SetForbidden(true);
                }
                count -= n;
            }
        }

        // ===== 每 tick：补给 / 扫视 / 瞄准 / 开火 =====

        protected override void Tick()
        {
            base.Tick();
            if (!Spawned || Map == null) return;

            if (cooldownTicks > 0) cooldownTicks--;

            // 没通电：贴图完全不动（也不扫视、不开火、不补充）
            if (!PoweredOn)
            {
                currentTarget = null;
                return;
            }

            // 被 EMP / 眩晕瘫痪：同样完全停摆
            if (Stunned)
            {
                currentTarget = null;
                return;
            }

            RechargeAmmo();

            // 停火 gizmo 打开时：不做索敌、不瞄准，直接随机乱转
            if (holdFire)
            {
                currentTarget = null;
                IdleTurn();
                return;
            }

            // 周期重扫：待机索敌 / 切换更重伤员（原作此处无目标时每 tick 全图扫描，
            // 是本链唯一性能热点，改为只走扫描周期，最坏 30 tick（0.5 秒）捕获新目标，
            // 开火冷却本来就有 240 tick，实际无感）
            if (this.IsHashIntervalTick(ScanInterval)) currentTarget = FindBestTarget();

            // 目标死了 / 走出射程 / 看不见 → 立刻重找一个（低频事件，保留原作响应性）；
            // 无目标时不再每 tick 扫，等下个周期；同时也消除了"扫描周期当 tick 扫两次"的浪费
            if (currentTarget != null && !IsValidAimTarget(currentTarget))
                currentTarget = FindBestTarget();

            if (currentTarget == null)
            {
                IdleTurn();               // 射程内没有需要治疗的单位：慢慢转圈（不发任何声音）
                return;
            }

            // 有目标就一直跟着转；冷却只限制开火，不限制瞄准
            AimAt(currentTarget);

            if (cooldownTicks > 0) return;              // 开火冷却 4 秒
            if (!NeedsHeal(currentTarget)) return;      // 目标已经治好：只瞄不打
            if (Ammo < 1f) return;                      // 没子弹：只瞄不打
            FireAt(currentTarget);
        }

        /// <summary>通电时每 3 小时补 1 发。</summary>
        private void RechargeAmmo()
        {
            if (ammo == null) return;
            if (ammo.Fuel >= ammo.Props.fuelCapacity)
            {
                rechargeTicks = 0;
                return;
            }
            if (++rechargeTicks < RechargeTicks) return;
            rechargeTicks = 0;
            ammo.Refuel(1f);
        }

        /// <summary>待机扫视，参数照抄原版 TurretTop（0.26°/tick、每次 140 tick、间隔 150~350 tick）。</summary>
        private void IdleTurn()
        {
            if (idleTurnTicksLeft > 0)
            {
                idleTurnTicksLeft--;
                curRotation += (idleTurnClockwise ? 1f : -1f) * IdleTurnDegreesPerTick;
                curRotation = Mathf.Repeat(curRotation, 360f);
                return;
            }

            if (ticksUntilIdleTurn > 0)
            {
                ticksUntilIdleTurn--;
                return;
            }

            ticksUntilIdleTurn = Rand.RangeInclusive(IdleTurnIntervalMin, IdleTurnIntervalMax);
            idleTurnTicksLeft = IdleTurnDuration;
            idleTurnClockwise = Rand.Bool;
        }

        /// <summary>直接对准目标（不插值）——射击时贴图必须钉在目标方向上。</summary>
        private void AimAt(Pawn target)
        {
            ticksUntilIdleTurn = 0;
            idleTurnTicksLeft = 0;
            curRotation = (target.DrawPos - DrawPos).AngleFlat();
        }

        private void FireAt(Pawn target)
        {
            ThingDef boltDef = HealBoltDef;
            if (boltDef == null) return;
            cooldownTicks = CooldownTicks;
            if (ammo != null && ammo.Fuel >= 1f) ammo.ConsumeFuel(1f);   // 一发 = 1 发「医疗软子弹」

            bool hit = Rand.Chance(AccuracyAgainst(target));

            LocalTargetInfo destination;
            ProjectileHitFlags flags;
            LocalTargetInfo intended;
            if (hit)
            {
                destination = target;
                intended = target;
                flags = ProjectileHitFlags.IntendedTarget;
            }
            else
            {
                // 打偏：落点取目标附近 5 格内随机格。flags 用 NonTargetPawns：
                // 流弹会沿弹道误击路上任意生物，被误击的可治疗生物会被治疗，
                // 敌人依然被 IsHealableSide 挡掉；原定目标被显式排除，对他依然是打偏。
                IntVec3 missCell = target.Position + GenRadial.RadialPattern[Rand.RangeInclusive(1, 5)];
                if (!missCell.InBounds(Map)) missCell = target.Position;
                destination = new LocalTargetInfo(missCell);
                intended = new LocalTargetInfo(missCell);
                flags = ProjectileHitFlags.NonTargetPawns;
            }

            Projectile projectile = (Projectile)GenSpawn.Spawn(boltDef, Position, Map, WipeMode.Vanish);
            projectile.Launch(this, DrawPos + new Vector3(0f, 0f, 0.25f), destination, intended, flags,
                preventFriendlyFire: false, equipment: null, targetCoverDef: null);

            PlayFireSound();
        }

        /// <summary>
        /// 命中率 = 枪 def 上的原版精度（0.77/0.70/0.45/0.24）× 建筑炮塔精度（0.96），
        /// 再把打偏的概率 ×0.75：最终 = 1 - (1 - acc) × 0.75。
        /// </summary>
        private float AccuracyAgainst(Pawn target)
        {
            float dist = target.Position.DistanceTo(Position);
            StatDef stat = dist <= TouchRange ? StatDefOf.AccuracyTouch
                : dist <= ShortRange ? StatDefOf.AccuracyShort
                : dist <= MediumRange ? StatDefOf.AccuracyMedium
                : StatDefOf.AccuracyLong;

            float acc = StatUtility.GetStatValueFromList(HealGunDef?.statBases, stat, 0.5f);
            acc *= this.GetStatValue(StatDefOf.ShootingAccuracyTurret);
            acc = 1f - (1f - Mathf.Clamp01(acc)) * MissChanceFactor;
            return Mathf.Clamp01(acc);
        }

        /// <summary>开火音效：直接读枪 def 上的 soundCast / soundCastTail，只在真的开火时播。</summary>
        private void PlayFireSound()
        {
            List<VerbProperties> verbs = HealGunDef?.Verbs;
            VerbProperties props = verbs != null && verbs.Count > 0 ? verbs[0] : null;
            if (props == null) return;
            SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, Map), MaintenanceType.None);
            if (props.soundCast != null) SoundStarter.PlayOneShot(props.soundCast, info);
            if (props.soundCastTail != null) SoundStarter.PlayOneShot(props.soundCastTail, info);
        }

        // ===== 索敌 =====

        /// <summary>
        /// 选目标：优先"需要治疗"的（流血/可包扎伤口），取生命百分比最低者，同分取近；
        /// 一个需要治疗的都没有 → 返回 null，调用方进入待机扫视。
        /// </summary>
        private Pawn FindBestTarget()
        {
            IReadOnlyList<Pawn> all = Map.mapPawns.AllPawnsSpawned;
            Pawn best = null;
            float bestHealth = 1f;
            int bestDistSq = int.MaxValue;

            for (int i = 0; i < all.Count; i++)
            {
                Pawn p = all[i];
                if (!IsValidAimTarget(p)) continue;   // 友方/盟友 + 有机体 + 射程内 + 有视线
                if (!NeedsHeal(p)) continue;          // 必须有需要治疗的伤口，否则不算目标

                float health = p.health.summaryHealth.SummaryHealthPercent;
                int distSq = Position.DistanceToSquared(p.Position);
                if (health < bestHealth - 0.0001f
                    || (Mathf.Abs(health - bestHealth) <= 0.0001f && distSq < bestDistSq))
                {
                    best = p;
                    bestHealth = health;
                    bestDistSq = distSq;
                }
            }

            return best;
        }

        /// <summary>能不能继续瞄着它：友方、有机体、射程内、有视线（不要求"还需要治疗"）。</summary>
        private bool IsValidAimTarget(Pawn p)
        {
            if (p == null || p.Dead || !p.Spawned || p.Map != Map) return false;
            if (!IsHealableSide(p) || !MedUtil.IsMedHealable(p)) return false;
            if (p.Position.DistanceToSquared(Position) > RangeSq) return false;   // 免开方
            return GenSight.LineOfSight(Position, p.Position, Map, skipFirstCell: true);
        }

        // ===== 判定 =====

        /// <summary>友方：玩家自己人 / 玩家收容的人（囚犯/客人/奴隶）/ 盟友与中立阵营的生物。</summary>
        public static bool IsHealableSide(Pawn p)
        {
            if (p == null) return false;
            if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) return true;
            return p.Faction != null && p.Faction.AllyOrNeutralTo(Faction.OfPlayer);
        }

        /// <summary>
        /// 需要治疗 = 有机体 + 身上还有还没被治疗过的可包扎伤口，
        /// 并且不是那种"只剩非开放性伤口、且纳米机器人已经在生效"的情况。
        /// </summary>
        public static bool NeedsHeal(Pawn p)
        {
            if (p == null || p.Dead || p.health?.hediffSet == null) return false;
            if (!MedUtil.IsMedHealable(p)) return false;      // 排除机械族/食尸鬼/蹒跚怪
            if (!HasUntreatedInjury(p, out bool hasUntreatedBleeding)) return false;

            // 有未治疗的流血伤口 → 一定开火（要包扎 + 给纳米）。
            if (hasUntreatedBleeding) return true;

            // 只剩非开放性伤口（瘀伤 / 骨折 / 破裂等）：
            // 如果这个人已经在被纳米机器人治疗，就不要再来一发刷新 hediff —— 直接排除。
            return !HasActiveNanite(p);
        }

        /// <summary>身上是否正带着「纳米医疗机器人生效」hediff。</summary>
        public static bool HasActiveNanite(Pawn p)
        {
            HediffDef def = NaniteHediffDef;
            return p?.health?.hediffSet != null && def != null && p.health.hediffSet.GetFirstHediffOfDef(def) != null;
        }

        /// <summary>
        /// 有没有"还没治疗过"的伤：覆盖 Hediff_Injury 和 Hediff_MissingPart（断肢也会流血、也吃包扎）；
        /// 已包扎过（IsTended）的不算；TendableNow(ignoreTimer: true) 判定可包扎，永久疤痕依然排除；
        /// hasBleeding 输出：其中是否有正在流血的（决定要不要包扎）。
        /// </summary>
        public static bool HasUntreatedInjury(Pawn p, out bool hasBleeding)
        {
            hasBleeding = false;
            if (p?.health?.hediffSet == null) return false;
            bool any = false;
            List<Hediff> hediffs = p.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                bool isInjury = h is Hediff_Injury;
                if (!isInjury && !(h is Hediff_MissingPart)) continue;
                if (isInjury && h.Severity <= 0f) continue;

                HediffComp_TendDuration tend = h.TryGetComp<HediffComp_TendDuration>();
                if (tend != null && tend.IsTended) continue;          // 已经包扎过 → 不需要再打
                if (!h.TendableNow(ignoreTimer: true)) continue;       // 不可包扎（疤痕、久远的断肢）排除

                any = true;
                if (h.BleedRate > 0f) hasBleeding = true;
                if (hasBleeding) return true;                          // 已经确定要打，不必再扫
            }
            return any;
        }

        // ===== 治疗结算（弹体命中时调用） =====

        /// <summary>
        /// 命中效果：先看有没有流血伤口——有就包扎一处（质量 20%~50%），没有就算了；
        /// 然后无条件施加/刷新 0.5 小时的「纳米医疗机器人生效」。
        /// 命中效果与"这个生物受没受伤"无关：索敌逻辑（NeedsHeal）只决定炮塔瞄谁，
        /// 打偏的流弹砸到一只毫发无伤的动物/敌人，也照样给它挂上纳米医疗 hediff（只要求有血肉）。
        /// </summary>
        public static void ApplyHeal(Pawn p)
        {
            if (p == null || p.Dead || p.health?.hediffSet == null) return;

            // 只包扎"正在流血且还没包扎过"的伤口；骨折/瘀伤这类不流血的伤不包扎。
            MedUtil.TendOneBleedingWound(p, 0.2f, 0.5f);

            HediffDef naniteDef = NaniteHediffDef;
            if (naniteDef != null)
            {
                Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(naniteDef);
                if (hediff == null)
                {
                    hediff = HediffMaker.MakeHediff(naniteDef, p);
                    p.health.AddHediff(hediff);
                }
                HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappears != null) disappears.ticksToDisappear = NaniteTicks;   // 0.5 小时
            }

            if (p.Spawned && p.Map != null)
                MoteMaker.ThrowText(p.DrawPos + new Vector3(0f, 0f, 0.6f), p.Map,
                    "MMT.HealTurretHitMote".Translate(), new Color(0.35f, 0.95f, 0.55f), 1.2f);
        }

        // ===== 绘制：底座（原版贴图）+ 自己旋转的塔顶 =====

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);          // 底座（原版 TurretMini_Base）
            Graphic top = TopGraphic;
            if (top == null) return;

            // 画法与 TurretTop.DrawTurret 完全一致：
            //   matrix = TRS(位置 + altitude 增量, ToQuat(ArtworkRotation + AngleFlat), 缩放)
            // 抬高 altitude 增量是必须的：底座会印进地图网格，不抬高会被盖住。
            Vector2 size = HealGunDef?.graphicData?.drawSize ?? Vector2.one;
            Matrix4x4 matrix = Matrix4x4.TRS(
                drawLoc + Altitudes.AltIncVect,
                GenMath.ToQuat(curRotation + TopArtworkRotation),
                new Vector3(size.x, 1f, size.y));
            Graphics.DrawMesh(MeshPool.plane10, matrix, top.MatSingleFor(this), 0);
        }

        private Graphic TopGraphic
        {
            get
            {
                if (topGraphic == null)
                {
                    GraphicData gd = HealGunDef?.graphicData;
                    string path = gd?.texPath ?? "Things/Building/Security/MMT_MedTurret_Top";
                    Vector2 size = gd?.drawSize ?? Vector2.one;
                    topGraphic = GraphicDatabase.Get<Graphic_Single>(path, ShaderTypeDefOf.Cutout.Shader, size, Color.white);
                }
                return topGraphic;
            }
        }

        // ===== 属性栏 =====

        /// <summary>
        /// 属性栏：弹药那一行由文件末尾的 CompInspectStringExtra 补丁整行生成；
        /// 这里再追加一行补充倒计时：子弹不满 + 通电时显示「正在自动补充子弹（1.6 小时）」。
        /// </summary>
        public override string GetInspectString()
        {
            string baseStr = base.GetInspectString();
            if (ammo == null || !PoweredOn) return baseStr;
            if (ammo.Fuel >= ammo.Props.fuelCapacity) return baseStr;   // 满弹不显示倒计时

            int left = Mathf.Max(0, RechargeTicks - rechargeTicks);
            string line = "MMT.HealTurretRefilling".Translate(left.ToStringTicksToPeriod());
            return baseStr.NullOrEmpty() ? line : baseStr + "\n" + line;
        }

        // ===== 选中时的射程白框 =====

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(Position, Range);
        }

        // ===== 停火 gizmo（原版炮塔同款） =====

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

            yield return new Command_Toggle
            {
                defaultLabel = "CommandHoldFire".Translate(),
                defaultDesc = "CommandHoldFireDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire"),
                isActive = () => holdFire,
                toggleAction = () => holdFire = !holdFire,
            };
        }
    }

    /// <summary>建造预览：把 18 格治疗范围画出来（原版 PlaceWorker_ShowTurretRadius 需要炮塔 def，这里自备）。</summary>
    public class PlaceWorker_MMTHealTurretRange : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawRadiusRing(center, Building_MMTHealTurret.Range);
        }
    }

    // ===== Harmony 补丁：治疗炮塔的弹药显示 =====

    /// <summary>
    /// 治疗炮塔不画 CompRefuelable 的燃料覆盖层图标
    ///（否则炮塔身上会挂一个"钢铁/燃料"小图标；弹药量只在 gizmo 与属性栏里看）。
    /// </summary>
    [HarmonyPatch(typeof(CompRefuelable), "PostDraw")]
    public static class CompRefuelable_PostDraw_Patch
    {
        private static bool Prefix(CompRefuelable __instance)
        {
            return !(__instance.parent is Building_MMTHealTurret);
        }
    }

    /// <summary>
    /// 治疗炮塔的属性栏弹药行：只显示一行「已组装的纳米医疗子弹：8/8」，全由 Keyed 出中文
    ///（原版这个 comp 的默认格式会把 fuelLabel 拼上英文分隔符，中文环境下容易中英混排）。
    /// </summary>
    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.CompInspectStringExtra))]
    public static class CompRefuelable_InspectString_Patch
    {
        private static void Postfix(CompRefuelable __instance, ref string __result)
        {
            if (__instance.parent is Building_MMTHealTurret)
            {
                __result = "MMT.HealTurretAmmoInspect".Translate(
                    __instance.Fuel.ToString("F0"), __instance.Props.fuelCapacity.ToString("F0"));
            }
        }
    }
}
