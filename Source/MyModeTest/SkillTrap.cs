using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    // ========== 触发式陷阱框架（第二十步：爆炸地雷） ==========
    // 在目标格布设一枚地雷：短暂武装时间后开始扫描，敌对 pawn 进入触发半径即
    // 引爆（GenExplosion 真实爆炸），长时间未触发自动失效。与第十一步延时打击
    // （纯时间轴）互补——这是"空间触发"轴，埋伏/区域拒止类技能的基础。
    // 与原版陷阱（踩格触发、建筑）的区别：技能地雷是"半径触发"的轻量特效实体
    // （Attach 类无碰撞 Thing），不可选中、不可拆除。
    // MMTMineUtil.PlaceMine 为统一入口；pawn 技能侧出口 CompAbilityEffect_MMTMine
    // （radius 预览圈 = 爆炸半径，走第一步 AoETarget 基类）；演示侧出口
    // demoType="mine"（数据源 MMT_SkillDemoSet.xml）。
    // 注：参数运行时传入不进存档，读档后首个 tick 自毁（同其它特效实体，可接受）。

    /// <summary>触发式陷阱参数包（MMTMineUtil.PlaceMine 的输入）。</summary>
    public class MMTMineParams
    {
        /// <summary>武装时间 tick（布设后该期间不触发，给施法者撤离窗口）。</summary>
        public int armTicks = 60;
        /// <summary>触发半径（格）：敌对 pawn 进入即引爆。</summary>
        public float triggerRadius = 2f;
        /// <summary>爆炸半径（格）。</summary>
        public float explosionRadius = 3f;
        public float damage = 30f;
        /// <summary>伤害类型（null = Bomb）。</summary>
        public DamageDef damageDef;
        public float armorPenetration;
        /// <summary>存在上限 tick（到期未触发自动失效）。</summary>
        public int durationTicks = 7200;
        /// <summary>触发扫描间隔 tick。</summary>
        public int scanIntervalTicks = 10;
        public SoundDef startSound;
        public Thing equipment;
    }

    /// <summary>触发式陷阱统一入口。</summary>
    public static class MMTMineUtil
    {
        public const string MineDefName = "MMT_SkillMine";

        /// <summary>在 cell 布设一枚地雷，返回地雷实体（格子不可布设返回 null）。</summary>
        public static Thing_MMTMine PlaceMine(Thing caster, IntVec3 cell, MMTMineParams p)
        {
            Map map = caster?.MapHeld;
            if (map == null || p == null || !cell.IsValid || !cell.InBounds(map) || !cell.Standable(map))
            {
                return null;
            }
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(MineDefName);
            if (def == null)
            {
                Log.Error("[MMT] 找不到 " + MineDefName + " 定义，无法布设地雷");
                return null;
            }
            Thing_MMTMine mine = (Thing_MMTMine)GenSpawn.Spawn(def, cell, map);
            mine.Setup(caster, p);
            if (p.startSound != null)
            {
                SoundStarter.PlayOneShot(p.startSound, new TargetInfo(cell, map));
            }
            return mine;
        }
    }

    /// <summary>
    /// 技能地雷实体：Attach 类无碰撞 Thing（不可选中），武装后按扫描间隔检查触发半径内
    /// 敌对 pawn，发现即引爆并自毁；超过存在上限未触发则静默失效。武装完成后周期性
    /// 放一粒红色微光 fleck 作"已激活"提示。参数运行时传入不进存档，读档后首 tick 自毁。
    /// </summary>
    public class Thing_MMTMine : Thing
    {
        private Thing caster;
        private MMTMineParams mineParams;
        private int armTicksLeft;
        private int ticksLeft = -1;
        private int ticksUntilScan;
        private bool loadedFromSave;

        public void Setup(Thing caster, MMTMineParams p)
        {
            this.caster = caster;
            mineParams = p;
            armTicksLeft = Mathf.Max(0, p.armTicks);
            ticksLeft = Mathf.Max(1, p.durationTicks);
            ticksUntilScan = 1;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            loadedFromSave = respawningAfterLoad;
        }

        protected override void Tick()
        {
            base.Tick();
            if (loadedFromSave || mineParams == null || Map == null)
            {
                Destroy();
                return;
            }
            ticksLeft--;
            if (ticksLeft <= 0)
            {
                // 超时未触发：失效自毁（尘雾提示，不爆炸）
                FleckMaker.ThrowDustPuff(Position, Map, 0.8f);
                Destroy();
                return;
            }
            if (armTicksLeft > 0)
            {
                armTicksLeft--;
                return;
            }
            if (--ticksUntilScan <= 0)
            {
                ticksUntilScan = Mathf.Max(1, mineParams.scanIntervalTicks);
                if (AnyHostileInTriggerRadius())
                {
                    Detonate();
                    return;
                }
            }
            // 已武装提示：每 45 tick 放一粒红色微光（仅当前地图）
            if (ticksLeft % 45 == 0 && Map == Find.CurrentMap)
            {
                FleckDef fleck = DefDatabase<FleckDef>.GetNamedSilentFail("MMT_Fleck_BuffSpark");
                if (fleck != null)
                {
                    Vector3 pos = Position.ToVector3Shifted();
                    pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    FleckCreationData data = FleckMaker.GetDataStatic(pos, Map, fleck, 0.5f);
                    data.instanceColor = new Color(1f, 0.25f, 0.2f);
                    Map.flecks.CreateFleck(data);
                }
            }
        }

        /// <summary>触发半径内是否存在敌对 pawn（语义同区域 HostileOnly）。</summary>
        private bool AnyHostileInTriggerRadius()
        {
            foreach (Pawn p in MMTAoETargetPreview.RawPawnsInRadius(Map, Position, mineParams.triggerRadius))
            {
                if (p != caster && (caster == null
                    || (p.Faction != null && p.Faction != caster.Faction && p.HostileTo(caster))))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>引爆：真实爆炸（伤害半径）后自毁。</summary>
        private void Detonate()
        {
            Map map = Map;
            if (map != null)
            {
                GenExplosion.DoExplosion(Position, map, mineParams.explosionRadius,
                    mineParams.damageDef ?? DamageDefOf.Bomb, caster,
                    damAmount: Mathf.RoundToInt(mineParams.damage),
                    armorPenetration: mineParams.armorPenetration,
                    weapon: mineParams.equipment?.def);
            }
            Destroy();
        }
    }

    // ========== pawn 技能出口：触发式爆炸陷阱 ==========

    public class CompProperties_MMTMine : CompProperties_MMTAoETargetBase
    {
        /// <summary>武装时间秒数。</summary>
        public float armSeconds = 1f;
        public float triggerRadius = 2f;
        public float explosionRadius = 3f;
        public float damage = 30f;
        public string damageDefName;
        public float armorPenetration;
        /// <summary>存在上限秒数。</summary>
        public float durationSeconds = 120f;
        public string soundDefName;

        public CompProperties_MMTMine()
        {
            compClass = typeof(CompAbilityEffect_MMTMine);
            // 预览圈画爆炸半径（触发半径在技能描述里说明）
            radius = 3f;
            targetFriendly = false;
        }
    }

    /// <summary>爆炸陷阱：在目标格布设地雷（武装后敌人进入触发半径即引爆）。</summary>
    public class CompAbilityEffect_MMTMine : CompAbilityEffect_MMTAoETargetBase
    {
        public new CompProperties_MMTMine Props => (CompProperties_MMTMine)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            MMTMineUtil.PlaceMine(caster, target.Cell, new MMTMineParams
            {
                armTicks = Mathf.Max(1, Props.armSeconds.SecondsToTicks()),
                triggerRadius = Props.triggerRadius,
                explosionRadius = Props.explosionRadius,
                damage = Props.damage,
                damageDef = Props.damageDefName.NullOrEmpty() ? null : DefDatabase<DamageDef>.GetNamedSilentFail(Props.damageDefName),
                armorPenetration = Props.armorPenetration,
                durationTicks = Mathf.Max(1, Props.durationSeconds.SecondsToTicks()),
                startSound = Props.soundDefName.NullOrEmpty() ? null : DefDatabase<SoundDef>.GetNamedSilentFail(Props.soundDefName),
                equipment = caster.equipment?.Primary
            });
        }
    }
}
