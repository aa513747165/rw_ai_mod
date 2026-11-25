#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""技能特效框架静态验证：地面火焰（第六步）+ 光环（第七步）+ 增益/减益特效（第八步）+ 位移（第九步）+ 召唤（第十步）Def/贴图/假人演示条目/C# 核心。"""
from pathlib import Path

from validate_mod import assert_equal, child_text, def_map, fail, parse_xml, require_files

DUMMY_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_TrainingDummy.xml"
DEMOSET_XML = "1.7/Defs/Misc/MMT_SkillDemoSet.xml"
FLAME_XML = "1.7/Defs/ThingDefs_Misc/MMT_GroundFlame.xml"
AURA_XML = "1.7/Defs/ThingDefs_Misc/MMT_SkillAura.xml"
AURA_HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_SkillAura.xml"
BUFF_XML = "1.7/Defs/ThingDefs_Misc/MMT_SkillBuffIcon.xml"
BUFF_HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_SkillBuff.xml"
MOVER_XML = "1.7/Defs/ThingDefs_Misc/MMT_DisplaceMover.xml"
SUMMON_XML = "1.7/Defs/ThingDefs_Misc/MMT_SummonEntities.xml"
TURRET_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_SummonTurret.xml"
SUMMON_HEDIFF_XML = "1.7/Defs/HediffDefs/MMT_Hediffs_Summon.xml"
FLECK_XML = "1.7/Defs/Effects/MMT_Flecks_SkillFX.xml"
CS = "Source/MyModeTest/SkillGroundFire.cs"
AURA_CS = "Source/MyModeTest/SkillAura.cs"
BUFF_CS = "Source/MyModeTest/SkillBuffFX.cs"
DISPLACE_CS = "Source/MyModeTest/SkillDisplace.cs"
SUMMON_CS = "Source/MyModeTest/SkillSummon.cs"
TEX = "1.7/Textures/Things/Mote/MMT_FlamePillar.png"
AURA_TEX = "1.7/Textures/Things/Mote/MMT_AuraDisc.png"
BUFF_TEX = "1.7/Textures/Things/Mote/MMT_BuffSpark.png"
BLINK_TEX = "1.7/Textures/Things/Mote/MMT_BlinkFlash.png"
DASH_TEX = "1.7/Textures/Things/Mote/MMT_DashTrail.png"
CIRCLE_TEX = "1.7/Textures/Things/Mote/MMT_TargetCircle.png"
ORB_TEX = "1.7/Textures/Things/Mote/MMT_SummonOrb.png"
HOMING_TEX = "1.7/Textures/Things/Mote/MMT_HomingBolt.png"
PAW_TEX = "1.7/Textures/Things/Mote/MMT_SummonPaw.png"
TURRET_BASE_TEX = "1.7/Textures/Things/Building/MMT_SummonTurret_Base.png"
TURRET_TOP_TEX = "1.7/Textures/Things/Building/MMT_SummonTurret_Top.png"
# —— 第 11~18 步 ——
DELAY_XML = "1.7/Defs/ThingDefs_Misc/MMT_DelayedStrike.xml"
CHAIN_XML = "1.7/Defs/ThingDefs_Misc/MMT_ChainArc.xml"
BREATH_XML = "1.7/Defs/ThingDefs_Misc/MMT_BreathCone.xml"
WALL_XML = "1.7/Defs/ThingDefs_Buildings/MMT_MagicStoneWall.xml"
PROJ_XML = "1.7/Defs/ThingDefs_Misc/MMT_Projectiles_SkillDemo.xml"
JOBS_XML = "1.7/Defs/JobDefs/MMT_Jobs.xml"
DELAY_CS = "Source/MyModeTest/SkillDelayedStrike.cs"
KNOCK_CS = "Source/MyModeTest/SkillKnockback.cs"
CHAIN_CS = "Source/MyModeTest/SkillChainLightning.cs"
BREATH_CS = "Source/MyModeTest/SkillBreath.cs"
HEAL_CS = "Source/MyModeTest/SkillHeal.cs"
WALL_CS = "Source/MyModeTest/SkillWall.cs"
BEAM_CS = "Source/MyModeTest/SkillBeam.cs"
METEOR_TEX = "1.7/Textures/Things/Mote/MMT_Meteor.png"
BIGBALL_TEX = "1.7/Textures/Things/Projectile/MMT_BigFireball.png"
WALL_TEX = "1.7/Textures/Things/Building/MMT_StoneWall.png"


def validate_skillfx() -> None:
    require_files([DUMMY_XML, DEMOSET_XML, FLAME_XML, CS, TEX, AURA_XML, AURA_HEDIFF_XML, AURA_CS, AURA_TEX,
                   BUFF_XML, BUFF_HEDIFF_XML, FLECK_XML, BUFF_CS, BUFF_TEX,
                   MOVER_XML, DISPLACE_CS, BLINK_TEX, DASH_TEX, CIRCLE_TEX,
                   SUMMON_XML, TURRET_XML, SUMMON_HEDIFF_XML, SUMMON_CS,
                   ORB_TEX, HOMING_TEX, PAW_TEX, TURRET_BASE_TEX, TURRET_TOP_TEX,
                   DELAY_XML, CHAIN_XML, BREATH_XML, WALL_XML, PROJ_XML, JOBS_XML,
                   DELAY_CS, KNOCK_CS, CHAIN_CS, BREATH_CS, HEAL_CS, WALL_CS, BEAM_CS,
                   METEOR_TEX, BIGBALL_TEX, WALL_TEX])

    defs = def_map(parse_xml(FLAME_XML))
    node = defs.get("MMT_GroundFlame")
    if node is None:
        fail("缺少 MMT_GroundFlame 定义")
        return
    assert_equal(child_text(node, "thingClass"), "MyModeTest.Thing_MMTGroundFlame", "火焰柱 thingClass")
    assert_equal(child_text(node, "category"), "Attachment", "火焰柱 category")
    assert_equal(child_text(node, "drawerType"), "RealtimeOnly", "火焰柱 drawerType")
    assert_equal(child_text(node, "graphicData/texPath"), "Things/Mote/MMT_FlamePillar", "火焰柱 texPath")

    # —— 光环（第七步） ——
    aura_defs = def_map(parse_xml(AURA_XML))
    shield = aura_defs.get("MMT_SkillAuraShield")
    if shield is None:
        fail("缺少 MMT_SkillAuraShield 定义")
        return
    assert_equal(child_text(shield, "thingClass"), "MyModeTest.Thing_MMTSkillAuraShield", "盾光环 thingClass")
    if "CompProperties_ProjectileInterceptor" not in Path(AURA_XML).read_text(encoding="utf-8"):
        fail("盾光环缺少原版 CompProjectileInterceptor（临时护盾投影器机制）")
    buff = aura_defs.get("MMT_SkillAuraBuff")
    if buff is None:
        fail("缺少 MMT_SkillAuraBuff 定义")
        return
    assert_equal(child_text(buff, "thingClass"), "MyModeTest.Thing_MMTSkillAuraBuff", "增益光环 thingClass")
    assert_equal(child_text(buff, "category"), "Attachment", "增益光环 category")

    aura_hediffs = Path(AURA_HEDIFF_XML).read_text(encoding="utf-8")
    for token in ("MMT_AuraBuff_Swift", "MMT_AuraDebuff_Slow", "HediffCompProperties_Disappears",
                  "<MoveSpeed>1.5</MoveSpeed>", "<MoveSpeed>0.5</MoveSpeed>"):
        if token not in aura_hediffs:
            fail(f"光环 Hediff 缺少: {token}")

    # —— 增益/减益特效（第八步） ——
    buff_defs = def_map(parse_xml(BUFF_XML))
    icon = buff_defs.get("MMT_BuffIconFlash")
    if icon is None:
        fail("缺少 MMT_BuffIconFlash 定义")
        return
    assert_equal(child_text(icon, "thingClass"), "MyModeTest.Thing_MMTBuffIconFlash", "增益图标 thingClass")
    assert_equal(child_text(icon, "category"), "Attachment", "增益图标 category")

    buff_hediffs = Path(BUFF_HEDIFF_XML).read_text(encoding="utf-8")
    for token in ("MMT_Buff_Swift", "MMT_Debuff_Slow", "HediffCompProperties_Disappears",
                  "<MoveSpeed>1.5</MoveSpeed>", "<MoveSpeed>0.5</MoveSpeed>"):
        if token not in buff_hediffs:
            fail(f"增益/减益 Hediff 缺少: {token}")

    flecks = Path(FLECK_XML).read_text(encoding="utf-8")
    if "MMT_Fleck_BuffSpark" not in flecks or "MMT_BuffSpark" not in flecks:
        fail("MMT_Flecks_SkillFX.xml 缺少 MMT_Fleck_BuffSpark 粒子 fleck")

    # —— 位移（第九步） ——
    mover_defs = def_map(parse_xml(MOVER_XML))
    mover = mover_defs.get("MMT_DisplaceMover")
    if mover is None:
        fail("缺少 MMT_DisplaceMover 定义")
        return
    assert_equal(child_text(mover, "thingClass"), "MyModeTest.Thing_MMTDisplaceMover", "位移载体 thingClass")
    assert_equal(child_text(mover, "drawerType"), "RealtimeOnly", "位移载体 drawerType")
    assert_equal(child_text(mover, "altitudeLayer"), "Skyfaller", "位移载体 altitudeLayer")

    for token in ("MMT_Fleck_BlinkFlash", "MMT_BlinkFlash", "MMT_Fleck_DashTrail", "MMT_DashTrail"):
        if token not in flecks:
            fail(f"MMT_Flecks_SkillFX.xml 缺少: {token}")

    displace_cs = Path(DISPLACE_CS).read_text(encoding="utf-8")
    for token in ("MMTDisplaceUtil", "Thing_MMTDisplaceMover", "CompAbilityEffect_MMTDisplace",
                  "PawnFlyer", "MakeFlyer", "MassTeleport", "Blink", "StartCharge", "StartJump"):
        if token not in displace_cs:
            fail(f"SkillDisplace.cs 缺少: {token}")

    # —— 圆形选择框（第一步改圆）：瞄准框架统一走 DrawCircle 贴图圆 ——
    targeting_cs = Path("Source/MyModeTest/AbilityTargeting.cs").read_text(encoding="utf-8")
    for token in ("DrawCircle", "MMT_TargetCircle", "MMTSkillDemoSetDef", "MMTSkillDemoGrant",
                  "MMT_SkillDemoSet"):
        if token not in targeting_cs:
            fail(f"AbilityTargeting.cs 缺少: {token}")

    buff_cs = Path(BUFF_CS).read_text(encoding="utf-8")
    for token in ("MMTBuffFXUtil", "Thing_MMTBuffIconFlash", "CompAbilityEffect_MMTBuffFX",
                  "ApplyOrRefreshHediff", "instanceColor", "AttachTo"):
        if token not in buff_cs:
            fail(f"SkillBuffFX.cs 缺少: {token}")

    # —— 召唤（第十步） ——
    summon_defs = def_map(parse_xml(SUMMON_XML))
    orb = summon_defs.get("MMT_SummonOrb")
    if orb is None:
        fail("缺少 MMT_SummonOrb 定义")
        return
    assert_equal(child_text(orb, "thingClass"), "MyModeTest.Thing_MMTSummonOrb", "雷球 thingClass")
    assert_equal(child_text(orb, "useHitPoints"), "false", "雷球无敌不可选中")
    homing = summon_defs.get("MMT_SummonHoming")
    if homing is None:
        fail("缺少 MMT_SummonHoming 定义")
        return
    assert_equal(child_text(homing, "thingClass"), "MyModeTest.Thing_MMTSummonHoming", "追踪弹 thingClass")
    assert_equal(child_text(homing, "useHitPoints"), "false", "追踪弹无敌不可选中")

    turret_defs = def_map(parse_xml(TURRET_XML))
    turret = turret_defs.get("MMT_SummonTurret")
    if turret is None:
        fail("缺少 MMT_SummonTurret 定义")
        return
    assert_equal(child_text(turret, "thingClass"), "Building_TurretGun", "召唤炮塔走原版炮塔逻辑")
    turret_xml = Path(TURRET_XML).read_text(encoding="utf-8")
    for token in ("CompProperties_Lifespan", "MMT_Gun_SummonTurret", "turretGunDef",
                  "BaseWeaponTurret", "Bullet_MiniTurret"):
        if token not in turret_xml:
            fail(f"召唤炮塔 XML 缺少: {token}")
    if "CompProperties_Power" in turret_xml or "CompProperties_Refuelable" in turret_xml:
        fail("召唤炮塔不应带电力/燃料 comp（设计要求无需电力弹药）")

    summon_hediff = Path(SUMMON_HEDIFF_XML).read_text(encoding="utf-8")
    for token in ("MMT_SummonAnimal", "HediffCompProperties_MMTSummonAnimal"):
        if token not in summon_hediff:
            fail(f"召唤 Hediff 缺少: {token}")

    summon_cs = Path(SUMMON_CS).read_text(encoding="utf-8")
    for token in ("MMTSummonUtil", "Thing_MMTSummonOrb", "Thing_MMTSummonHoming",
                  "HediffComp_MMTSummonAnimal", "CompAbilityEffect_MMTSkillSummon",
                  "FireOrb", "FireHoming", "SummonAnimal", "SummonTurret",
                  "CompLifespan", "Designator_Slaughter", "Designator_ReleaseAnimalToWild"):
        if token not in summon_cs:
            fail(f"SkillSummon.cs 缺少: {token}")

    # —— 假人：只留敌我靶子，不再携带技能演示（演示条目迁入 MMT_SkillDemoSet） ——
    dummy = Path(DUMMY_XML).read_text(encoding="utf-8")
    if "CompProperties_MMTSkillDemo" in dummy:
        fail("假人不应再携带技能演示 comp（演示条目已迁入 MMT_SkillDemoSet.xml）")
    for token in ("CompProperties_MMTAoETargetable", "CompProperties_MMTForceHostileFaction",
                  "MMT_TrainingDummyHostile"):
        if token not in dummy:
            fail(f"假人靶子配置缺少: {token}")

    # —— 演示技能集（34 条，Dev 一键赋予小人的唯一数据源） ——
    demoset = Path(DEMOSET_XML).read_text(encoding="utf-8")
    for token in ("MMTSkillDemoSetDef", "MMT_SkillDemoSet",
                  "测试·友方回响", "测试·敌方回响", "测试·单发火球术", "测试·多方位火球术",
                  "测试·烈焰射线", "测试·落雷", "测试·雷暴",
                  "测试·烈焰喷发", "测试·烈焰火海", "groundFire", "groundRequireTarget",
                  "测试·守护光环", "测试·迅捷光环", "测试·迟缓光环", "auraShieldMode", "auraBuffDefName",
                  "测试·迅捷祝福", "测试·群体迅捷", "测试·迟缓诅咒", "测试·群体迟缓",
                  "bufffx", "buffRequireTarget", "buffHediffDefName", "MMT_Buff_Swift", "MMT_Debuff_Slow",
                  "测试·折跃", "测试·群体传送", "测试·冲锋", "测试·跳跃",
                  "blink", "massTeleport", "charge", "jump",
                  "测试·雷球", "测试·追踪弹", "测试·动物召唤", "测试·炮塔召唤",
                  "summonOrb", "summonHoming", "summonAnimal", "summonTurret",
                  "测试·陨石术", "测试·猛击击退", "测试·连锁闪电", "测试·龙息",
                  "测试·大火球", "测试·治愈之光", "测试·石墙术",
                  "delayedStrike", "knockback", "chainLightning", "breathCone", "heal", "wall",
                  "meteorDelaySeconds", "meteorFallTicks", "knockbackCells", "chainBounceCount",
                  "breathRange", "healAmount", "wallDurationSeconds", "ignoreLineOfSight",
                  "MMT_Proj_BigFireball"):
        if token not in demoset:
            fail(f"演示技能集缺少: {token}")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("MMTGroundFireUtil", "Thing_MMTGroundFlame", "CompAbilityEffect_MMTGroundFire",
                  "TryAttachFire", "TryStartFireIn", "FireGround"):
        if token not in cs:
            fail(f"SkillGroundFire.cs 缺少: {token}")

    aura_cs = Path(AURA_CS).read_text(encoding="utf-8")
    for token in ("MMTAuraUtil", "Thing_MMTSkillAuraShield", "Thing_MMTSkillAuraBuff",
                  "CompAbilityEffect_MMTAura", "MechShield", "CompProjectileInterceptor", "AttachTo"):
        if token not in aura_cs:
            fail(f"SkillAura.cs 缺少: {token}")

    # —— 延时打击·陨石（第十一步） ——
    delay_defs = def_map(parse_xml(DELAY_XML))
    sched = delay_defs.get("MMT_DelayedStrike")
    if sched is None:
        fail("缺少 MMT_DelayedStrike 定义")
        return
    assert_equal(child_text(sched, "thingClass"), "MyModeTest.Thing_MMTDelayedStrike", "延时打击调度器 thingClass")
    meteor = delay_defs.get("MMT_Meteor")
    if meteor is None:
        fail("缺少 MMT_Meteor 定义")
        return
    assert_equal(child_text(meteor, "thingClass"), "MyModeTest.Thing_MMTMeteor", "陨石 thingClass")
    assert_equal(child_text(meteor, "graphicData/texPath"), "Things/Mote/MMT_Meteor", "陨石 texPath")

    delay_cs = Path(DELAY_CS).read_text(encoding="utf-8")
    for token in ("MMTDelayedStrikeUtil", "Thing_MMTDelayedStrike", "Thing_MMTMeteor",
                  "CompAbilityEffect_MMTDelayedStrike", "DoExplosion", "MortarRound_PreImpact"):
        if token not in delay_cs:
            fail(f"SkillDelayedStrike.cs 缺少: {token}")

    # —— 近战击退（第十二步） ——
    knock_cs = Path(KNOCK_CS).read_text(encoding="utf-8")
    for token in ("MMTKnockbackUtil", "CompAbilityEffect_MMTKnockback", "PawnFlyer",
                  "MakeFlyer", "StaggerFor", "wallSlamDamage"):
        if token not in knock_cs:
            fail(f"SkillKnockback.cs 缺少: {token}")

    # —— 连锁闪电（第十三步） ——
    chain_defs = def_map(parse_xml(CHAIN_XML))
    arc = chain_defs.get("MMT_ChainArc")
    if arc is None:
        fail("缺少 MMT_ChainArc 定义")
        return
    assert_equal(child_text(arc, "thingClass"), "MyModeTest.Thing_MMTChainArc", "连锁闪电电弧 thingClass")
    chain_cs = Path(CHAIN_CS).read_text(encoding="utf-8")
    for token in ("MMTChainLightningUtil", "Thing_MMTChainArc", "CompAbilityEffect_MMTChainLightning",
                  "bounceCount", "damageFalloff", "ElectricalSpark", "BuildChain"):
        if token not in chain_cs:
            fail(f"SkillChainLightning.cs 缺少: {token}")

    # —— 持续射线引导占用（第十四步） ——
    beam_cs = Path(BEAM_CS).read_text(encoding="utf-8")
    for token in ("occupyCaster", "StartChannel", "ChannelBroken", "EndChannel", "BreakChannel",
                  "JobDriver_MMTChannel", "Stance_MMTChannel", "MMT_ChannelSkill",
                  "AttachChannel", "ToilCompleteMode.Never"):
        if token not in beam_cs:
            fail(f"SkillBeam.cs 缺少: {token}")
    jobs_xml = Path(JOBS_XML).read_text(encoding="utf-8")
    for token in ("MMT_ChannelSkill", "JobDriver_MMTChannel"):
        if token not in jobs_xml:
            fail(f"MMT_Jobs.xml 缺少: {token}")

    # —— 锥形吐息·龙息（第十五步） ——
    breath_defs = def_map(parse_xml(BREATH_XML))
    cone = breath_defs.get("MMT_BreathCone")
    if cone is None:
        fail("缺少 MMT_BreathCone 定义")
        return
    assert_equal(child_text(cone, "thingClass"), "MyModeTest.Thing_MMTBreathCone", "龙息 thingClass")
    breath_cs = Path(BREATH_CS).read_text(encoding="utf-8")
    for token in ("MMTBreathUtil", "Thing_MMTBreathCone", "CompAbilityEffect_MMTBreath",
                  "InCone", "halfAngleDegrees", "DrawConePreview", "FireGlow"):
        if token not in breath_cs:
            fail(f"SkillBreath.cs 缺少: {token}")

    # —— 抛物线大火球（第十六步） ——
    proj_xml = Path(PROJ_XML).read_text(encoding="utf-8")
    for token in ("MMT_Proj_BigFireball", "<flyOverhead>true</flyOverhead>", "arcHeightFactor",
                  "MortarRound_PreImpact", "Projectile_Explosive", "MMT_BigFireball"):
        if token not in proj_xml:
            fail(f"大火球投射物 XML 缺少: {token}")

    # —— 范围回血（第十七步） ——
    heal_cs = Path(HEAL_CS).read_text(encoding="utf-8")
    for token in ("MMTHealUtil", "CompAbilityEffect_MMTSkillHeal", "Hediff_Injury",
                  "healAmount", "HealingCross", "MMTBuffFXUtil"):
        if token not in heal_cs:
            fail(f"SkillHeal.cs 缺少: {token}")

    # —— 石墙术（第十八步） ——
    wall_defs = def_map(parse_xml(WALL_XML))
    wall_def = wall_defs.get("MMT_MagicStoneWall")
    if wall_def is None:
        fail("缺少 MMT_MagicStoneWall 定义")
        return
    assert_equal(child_text(wall_def, "fillPercent"), "1", "石墙满填")
    assert_equal(child_text(wall_def, "passability"), "Impassable", "石墙不可通行")
    wall_xml = Path(WALL_XML).read_text(encoding="utf-8")
    for token in ("CompProperties_Lifespan", "Things/Building/MMT_StoneWall", "MaxHitPoints"):
        if token not in wall_xml:
            fail(f"石墙 XML 缺少: {token}")
    if "<designationCategory>" in wall_xml:
        fail("石墙不应出现在建筑菜单（designationCategory）")
    wall_cs = Path(WALL_CS).read_text(encoding="utf-8")
    for token in ("MMTWallUtil", "CompAbilityEffect_MMTWall", "RaiseWall", "CanRaiseAt",
                  "CompLifespan", "ThrowDustPuff"):
        if token not in wall_cs:
            fail(f"SkillWall.cs 缺少: {token}")

    # —— 新演示接入 AbilityTargeting ——
    for token in ("delayedStrike", "knockback", "chainLightning", "breathCone", "heal", "wall",
                  "ignoreLineOfSight", "BreakChannel", "MMTDelayedStrikeUtil", "MMTKnockbackUtil",
                  "MMTChainLightningUtil", "MMTBreathUtil", "MMTHealUtil", "MMTWallUtil"):
        if token not in targeting_cs:
            fail(f"AbilityTargeting.cs 缺少新演示接入: {token}")

    print("skillfx: OK")


SUITES = {"skillfx": validate_skillfx}
