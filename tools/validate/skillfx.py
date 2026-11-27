#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""技能特效框架静态验证：地面火焰（第六步）+ 光环（第七步）+ 增益/减益特效（第八步）+ 位移（第九步）+ 召唤（第十步）+ 余烬爆发（第二十四步）+ 侧视平躺序列帧（第二十六步）Def/贴图/假人演示条目/C# 核心。"""
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
CS = "Source/MyModeTest/SkillFX/SkillGroundFire.cs"
AURA_CS = "Source/MyModeTest/SkillFX/SkillAura.cs"
BUFF_CS = "Source/MyModeTest/SkillFX/SkillBuffFX.cs"
DISPLACE_CS = "Source/MyModeTest/SkillFX/SkillDisplace.cs"
SUMMON_CS = "Source/MyModeTest/SkillFX/SkillSummon.cs"
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
DELAY_CS = "Source/MyModeTest/SkillFX/SkillDelayedStrike.cs"
KNOCK_CS = "Source/MyModeTest/SkillFX/SkillKnockback.cs"
CHAIN_CS = "Source/MyModeTest/SkillFX/SkillChainLightning.cs"
BREATH_CS = "Source/MyModeTest/SkillFX/SkillBreath.cs"
HEAL_CS = "Source/MyModeTest/SkillFX/SkillHeal.cs"
WALL_CS = "Source/MyModeTest/SkillFX/SkillWall.cs"
BEAM_CS = "Source/MyModeTest/SkillFX/SkillBeam.cs"
METEOR_TEX = "1.7/Textures/Things/Mote/MMT_Meteor.png"
BEAM_HEAD_TEX = "1.7/Textures/Things/Mote/MMT_SkillBeam_Head.png"
BEAM_TAIL_TEX = "1.7/Textures/Things/Mote/MMT_SkillBeam_Tail.png"
BIGBALL_TEX = "1.7/Textures/Things/Projectile/MMT_BigFireball.png"
WALL_TEX = "1.7/Textures/Things/Building/MMT_StoneWall.png"
# —— 第 19 步：持续地面区域（毒气池） ——
ZONE_XML = "1.7/Defs/ThingDefs_Misc/MMT_GroundZone.xml"
ZONE_CS = "Source/MyModeTest/SkillFX/SkillGroundZone.cs"
# —— 第 20 步：触发式爆炸陷阱；第 21 步：寒风领域（地面区域冰冻皮肤） ——
MINE_XML = "1.7/Defs/ThingDefs_Misc/MMT_SkillMine.xml"
TRAP_CS = "Source/MyModeTest/SkillFX/SkillTrap.cs"
MINE_TEX = "1.7/Textures/Things/Mote/MMT_SkillMine.png"
ICE_TEX = "1.7/Textures/Things/Mote/MMT_IceBlock.png"
# —— 第 22 步：新星环（360° 扩散冲击波，神圣新星） ——
NOVA_XML = "1.7/Defs/ThingDefs_Misc/MMT_NovaRing.xml"
NOVA_CS = "Source/MyModeTest/SkillFX/SkillNova.cs"
NOVA_TEX = "1.7/Textures/Things/Mote/MMT_NovaRing.png"
# —— 第 23 步：驱散/净化（纯代码框架，复用 BuffIconFlash/微光点，无新增 Def/贴图） ——
DISPEL_CS = "Source/MyModeTest/SkillFX/SkillDispel.cs"
# —— 第 24 步：余烬爆发（爆发 + 短余烬 hybrid，复用 BuffSpark/BlinkFlash/FlamePillar 贴图） ——
EMBER_CS = "Source/MyModeTest/SkillFX/SkillEmberBurst.cs"
# —— 第 25 步：屏幕空间升起序列帧（侧视素材走 DrawGUIOverlay UI 层贴图，绕开竖直 quad 不渲染） ——
RISE_XML = "1.7/Defs/Effects/MMT_Mote_SkillRiseFX.xml"
RISE_CS = "Source/MyModeTest/SkillFX/SkillRiseFX.cs"
RISE_TEX = "1.7/Textures/Things/Mote/MMT_SkillRiseFX_FlamePillar.png"
# —— 第 26 步：侧视平躺序列帧（整帧平铺水平 quad，贴图上边朝北=屏幕上） ——
SIDEFLAT_XML = "1.7/Defs/ThingDefs_Misc/MMT_SkillSideFlat.xml"
SIDEFLAT_CS = "Source/MyModeTest/SkillFX/SkillSideFlat.cs"


def validate_skillfx() -> None:
    require_files([DUMMY_XML, DEMOSET_XML, FLAME_XML, CS, TEX, AURA_XML, AURA_HEDIFF_XML, AURA_CS, AURA_TEX,
                   BUFF_XML, BUFF_HEDIFF_XML, FLECK_XML, BUFF_CS, BUFF_TEX,
                   MOVER_XML, DISPLACE_CS, BLINK_TEX, DASH_TEX, CIRCLE_TEX,
                   SUMMON_XML, TURRET_XML, SUMMON_HEDIFF_XML, SUMMON_CS,
                   ORB_TEX, HOMING_TEX, PAW_TEX, TURRET_BASE_TEX, TURRET_TOP_TEX,
                   DELAY_XML, CHAIN_XML, BREATH_XML, WALL_XML, PROJ_XML, JOBS_XML,
                   DELAY_CS, KNOCK_CS, CHAIN_CS, BREATH_CS, HEAL_CS, WALL_CS, BEAM_CS,
                   METEOR_TEX, BIGBALL_TEX, WALL_TEX,
                   ZONE_XML, ZONE_CS, MINE_XML, TRAP_CS, MINE_TEX, ICE_TEX,
                   BEAM_HEAD_TEX, BEAM_TAIL_TEX, NOVA_XML, NOVA_CS, NOVA_TEX, DISPEL_CS, EMBER_CS,
                   RISE_XML, RISE_CS, RISE_TEX, SIDEFLAT_XML, SIDEFLAT_CS])

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

    for token in ("MMT_Fleck_BlinkFlash", "MMT_BlinkFlash", "MMT_Fleck_DashTrail", "MMT_DashTrail",
                  "MMT_Fleck_Ember"):
        if token not in flecks:
            fail(f"MMT_Flecks_SkillFX.xml 缺少: {token}")

    displace_cs = Path(DISPLACE_CS).read_text(encoding="utf-8")
    for token in ("MMTDisplaceUtil", "Thing_MMTDisplaceMover", "CompAbilityEffect_MMTDisplace",
                  "PawnFlyer", "MakeFlyer", "MassTeleport", "Blink", "StartCharge", "StartJump"):
        if token not in displace_cs:
            fail(f"SkillDisplace.cs 缺少: {token}")

    # —— 圆形选择框（第一步改圆）：瞄准框架统一走 DrawCircle 贴图圆 ——
    targeting_cs = Path("Source/MyModeTest/SkillFX/AbilityTargeting.cs").read_text(encoding="utf-8")
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

    # —— 演示技能集（39 条，Dev 一键赋予小人的唯一数据源） ——
    demoset = Path(DEMOSET_XML).read_text(encoding="utf-8")
    for token in ("MMTSkillDemoSetDef", "MMT_SkillDemoSet",
                  "测试·友方回响", "测试·敌方回响", "测试·单发火球术", "测试·多方位火球术",
                  "测试·烈焰射线", "测试·落雷", "测试·雷暴",
                  "beamMuzzleOffset", "beamHeadTexPath", "beamTailTexPath", "beamHeadSize", "beamTailSize",
                  "测试·烈焰喷发", "测试·烈焰火海", "groundFire", "groundRequireTarget",
                  "groundPillarHeight", "groundPillarLayers", "groundPillarTopScale", "groundPillarSway",
                  "groundFrameTexPaths", "groundFrameTicks", "MMT_FlameErupt",
                  "emberBurst", "emberCount", "emberBurstDamage", "emberResidDurationSeconds",
                  "emberResidDamage", "emberResidDensity", "emberSparkSpeedMin", "emberSparkSpeedMax",
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
                  "MMT_Proj_BigFireball",
                  "测试·毒气池", "groundZone", "zoneDurationSeconds", "zoneIntervalTicks",
                  "zoneAffectEveryone", "zoneHediffDefName", "zoneHediffSeverity", "ToxicBuildup",
                  "测试·爆炸陷阱", "mine", "mineArmSeconds", "mineTriggerRadius",
                  "mineExplosionRadius", "mineDamage", "mineDurationSeconds",
                  "测试·寒风领域", "zoneStaggerTicks", "zoneIceScale",
                  "测试·神圣新星", "nova", "novaCellsPerSecond", "novaBandWidth",
                  "novaDamage", "novaHealAmount", "novaHitOnce", "novaFadeOutTicks",
                  "novaTexPath", "novaTint", "MMT_NovaRing", "MMT_Gong",
                  "测试·驱散魔法", "测试·驱散魔法·敌", "dispel", "dispelCount",
                  "dispelIconTexPath", "dispelIconSeconds", "MMT_FastReleaseSD", "UI/RoM/purify",
                  "测试·地心火柱", "riseFX", "riseTexPath", "riseFrameCount", "riseFrameTicks",
                  "riseLoop", "riseDurationSeconds", "riseWidthCells", "riseHeightCells",
                  "riseCells", "riseFadeInSeconds", "riseFadeOutSeconds", "MMT_SkillRiseFX_FlamePillar",
                  "测试·地心火柱·平躺", "sideFlat", "sideFlatTexPath", "sideFlatFrameCount",
                  "sideFlatFrameTicks", "sideFlatLoop", "sideFlatDurationSeconds",
                  "sideFlatWidthCells", "sideFlatHeightCells", "sideFlatDriftCells",
                  "sideFlatFadeInSeconds", "sideFlatFadeOutSeconds"):
        if token not in demoset:
            fail(f"演示技能集缺少: {token}")

    # —— 屏幕空间升起序列帧（第二十五步） ——
    rise_defs = def_map(parse_xml(RISE_XML))
    rise = rise_defs.get("MMT_Mote_SkillRiseFX")
    if rise is None:
        fail("缺少 MMT_Mote_SkillRiseFX 定义")
        return
    assert_equal(child_text(rise, "thingClass"), "MyModeTest.MoteMMTSkillRiseFX", "升起特效 mote thingClass")
    assert_equal(child_text(rise, "category"), "Mote", "升起特效 mote category")
    assert_equal(child_text(rise, "drawGUIOverlay"), "true", "升起特效 mote drawGUIOverlay")

    rise_cs = Path(RISE_CS).read_text(encoding="utf-8")
    for token in ("MoteMMTSkillRiseFX", "MMTSkillRiseFXUtil", "DrawGUIOverlay",
                  "DrawTextureWithTexCoords", "WorldToScreenPoint", "solidTimeOverride",
                  "MMT_Mote_SkillRiseFX"):
        if token not in rise_cs:
            fail(f"SkillRiseFX.cs 缺少: {token}")

    # —— 侧视平躺序列帧（第二十六步） ——
    flat_defs = def_map(parse_xml(SIDEFLAT_XML))
    flat = flat_defs.get("MMT_SkillSideFlat")
    if flat is None:
        fail("缺少 MMT_SkillSideFlat 定义")
        return
    assert_equal(child_text(flat, "thingClass"), "MyModeTest.Thing_MMTSkillSideFlat", "平躺特效 thingClass")
    assert_equal(child_text(flat, "category"), "Attachment", "平躺特效 category")
    assert_equal(child_text(flat, "drawerType"), "RealtimeOnly", "平躺特效 drawerType")

    flat_cs = Path(SIDEFLAT_CS).read_text(encoding="utf-8")
    for token in ("MMTSkillSideFlatParams", "Thing_MMTSkillSideFlat", "MMTSkillSideFlatUtil",
                  "mainTextureScale", "mainTextureOffset", "Quaternion.identity",
                  "PerspectiveCompensate", "ShadowCastingMode.Off", "MoteOverhead",
                  "MMT_SkillSideFlat", "MaterialPropertyBlock"):
        if token not in flat_cs:
            fail(f"SkillSideFlat.cs 缺少: {token}")

    cs = Path(CS).read_text(encoding="utf-8")
    for token in ("MMTGroundFireUtil", "Thing_MMTGroundFlame", "CompAbilityEffect_MMTGroundFire",
                  "TryAttachFire", "TryStartFireIn", "FireGround",
                  "pillarHeight", "pillarLayers", "pillarTopScale", "pillarSway",
                  "frameTexPaths", "frameTicks", "DrawFrameSequence", "MaterialPropertyBlock",
                  "DrawDisc", "PerspectiveCompensate", "ShadowCastingMode.Off"):
        if token not in cs:
            fail(f"SkillGroundFire.cs 缺少: {token}")

    # —— 烈焰喷发序列帧贴图（5 帧：起燃→峰值→旋卷→余烬） ——
    require_files([f"1.7/Textures/Things/Mote/MMT_FlameErupt/MMT_FlameErupt_{i}.png" for i in range(5)])
    boss_xml = Path("1.7/Defs/AbilityDefs/MMT_Abilities_FireBoss.xml").read_text(encoding="utf-8")
    for token in ("MMT_BossFire_FlamePillar", "frameTexPaths", "MMT_FlameErupt"):
        if token not in boss_xml:
            fail(f"MMT_Abilities_FireBoss.xml 缺少: {token}")

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

    # —— 持续射线引导占用（第十四步）+ 三段式端头（发射头/命中端，贴图已出图并入 require_files） ——
    beam_cs = Path(BEAM_CS).read_text(encoding="utf-8")
    for token in ("occupyCaster", "StartChannel", "ChannelBroken", "EndChannel", "BreakChannel",
                  "JobDriver_MMTChannel", "Stance_MMTChannel", "MMT_ChannelSkill",
                  "AttachChannel", "ToilCompleteMode.Never",
                  "muzzleOffset", "headTexPath", "tailTexPath", "LoadCapMat", "DrawPreviewCap",
                  "headVertical", "headPillarHeight", "headPillarLayers", "headPillarTopScale",
                  "DrawPillarStack"):
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

    # —— 持续地面区域·毒气池（第十九步） ——
    zone_defs = def_map(parse_xml(ZONE_XML))
    zone = zone_defs.get("MMT_GroundZone")
    if zone is None:
        fail("缺少 MMT_GroundZone 定义")
        return
    assert_equal(child_text(zone, "thingClass"), "MyModeTest.Thing_MMTGroundZone", "地面区域 thingClass")
    zone_cs = Path(ZONE_CS).read_text(encoding="utf-8")
    for token in ("MMTGroundZoneUtil", "Thing_MMTGroundZone", "CompAbilityEffect_MMTGroundZone",
                  "MMTZoneAffectMode", "FireZone", "EffectPulse", "hediffSeverityPerPulse",
                  "sharedMats", "RawPawnsInRadius"):
        if token not in zone_cs:
            fail(f"SkillGroundZone.cs 缺少: {token}")

    # —— 寒风领域：地面区域冰冻硬控皮肤（第二十一步） ——
    for token in ("staggerTicksPerPulse", "StaggerFor", "Thing_MMTIceBlock", "ApplyIceBlock",
                  "iceBlocks", "AttachableThing", "iceScale"):
        if token not in zone_cs:
            fail(f"SkillGroundZone.cs 缺少冰冻定身: {token}")
    ice = zone_defs.get("MMT_IceBlock")
    if ice is None:
        fail("缺少 MMT_IceBlock 定义")
        return
    assert_equal(child_text(ice, "thingClass"), "MyModeTest.Thing_MMTIceBlock", "冰块覆盖物 thingClass")
    assert_equal(child_text(ice, "graphicData/texPath"), "Things/Mote/MMT_IceBlock", "冰块覆盖物 texPath")

    # —— 触发式爆炸陷阱（第二十步） ——
    mine_defs = def_map(parse_xml(MINE_XML))
    mine = mine_defs.get("MMT_SkillMine")
    if mine is None:
        fail("缺少 MMT_SkillMine 定义")
        return
    assert_equal(child_text(mine, "thingClass"), "MyModeTest.Thing_MMTMine", "地雷 thingClass")
    assert_equal(child_text(mine, "category"), "Attachment", "地雷 category")
    assert_equal(child_text(mine, "selectable"), "false", "地雷不可选中")
    assert_equal(child_text(mine, "graphicData/texPath"), "Things/Mote/MMT_SkillMine", "地雷 texPath")
    trap_cs = Path(TRAP_CS).read_text(encoding="utf-8")
    for token in ("MMTMineUtil", "Thing_MMTMine", "CompAbilityEffect_MMTMine",
                  "PlaceMine", "Detonate", "DoExplosion", "triggerRadius", "armTicks",
                  "RawPawnsInRadius"):
        if token not in trap_cs:
            fail(f"SkillTrap.cs 缺少: {token}")

    # —— 新星环·神圣新星（第二十二步） ——
    nova_defs = def_map(parse_xml(NOVA_XML))
    nova = nova_defs.get("MMT_NovaRing")
    if nova is None:
        fail("缺少 MMT_NovaRing 定义")
        return
    assert_equal(child_text(nova, "thingClass"), "MyModeTest.Thing_MMTNovaRing", "新星环 thingClass")
    assert_equal(child_text(nova, "category"), "Attachment", "新星环 category")
    assert_equal(child_text(nova, "graphicData/texPath"), "Things/Mote/MMT_NovaRing", "新星环 texPath")
    nova_cs = Path(NOVA_CS).read_text(encoding="utf-8")
    for token in ("MMTNovaUtil", "Thing_MMTNovaRing", "CompAbilityEffect_MMTNova",
                  "FireNova", "HitBand", "hitPawns", "bandWidth", "cellsPerSecond",
                  "hitOnce", "fadeOutTicks", "MMTHealUtil", "sharedMats"):
        if token not in nova_cs:
            fail(f"SkillNova.cs 缺少: {token}")

    # —— 驱散/净化·驱散魔法（第二十三步） ——
    dispel_cs = Path(DISPEL_CS).read_text(encoding="utf-8")
    for token in ("MMTDispelUtil", "MMTDispelParams", "CompAbilityEffect_MMTDispel",
                  "DispelOne", "FindDispellable", "IsDispellable", "HediffComp_Disappears",
                  "IsPermanent", "RemoveHediff", "maxRemove", "MMTBuffFXUtil", "ThrowMicroSparks"):
        if token not in dispel_cs:
            fail(f"SkillDispel.cs 缺少: {token}")

    # —— 余烬爆发·烈焰火海改版（第二十四步） ——
    ember_cs = Path(EMBER_CS).read_text(encoding="utf-8")
    for token in ("MMTEmberBurstUtil", "MMTEmberBurstParams", "FireBurst",
                  "SpawnFlash", "SpawnSparks", "BurstDamage", "ScatterEmbers",
                  "MMT_Fleck_Ember", "MMT_Fleck_BlinkFlash", "SpawnFlame",
                  "airTimeLeft", "instanceColor", "velocity", "SecondsToTicks"):
        if token not in ember_cs:
            fail(f"SkillEmberBurst.cs 缺少: {token}")

    # —— 新演示接入 AbilityTargeting ——
    for token in ("delayedStrike", "knockback", "chainLightning", "breathCone", "heal", "wall",
                  "ignoreLineOfSight", "BreakChannel", "MMTDelayedStrikeUtil", "MMTKnockbackUtil",
                  "MMTChainLightningUtil", "MMTBreathUtil", "MMTHealUtil", "MMTWallUtil",
                  "groundZone", "MMTGroundZoneUtil", "mine", "MMTMineUtil", "zoneStaggerTicks",
                  "寒风领域", "nova", "MMTNovaUtil", "神圣新星", "novaCellsPerSecond",
                  "novaHitOnce", "novaTexPath", "novaTint",
                  "dispel", "MMTDispelUtil", "dispelCount", "dispelIconTexPath",
                  "emberBurst", "MMTEmberBurstUtil", "emberCount", "emberResidDurationSeconds",
                  "sideFlat", "MMTSkillSideFlatUtil", "sideFlatTexPath", "sideFlatDriftCells"):
        if token not in targeting_cs:
            fail(f"AbilityTargeting.cs 缺少新演示接入: {token}")

    # —— 性能：瞄准扫描每 tick 缓存 + 合并双扫描；火海共享材质 ——
    for token in ("EnsureScan", "RawPawnsInRadius", "PawnsInRadiusCached", "scanPawns", "scanBuildings"):
        if token not in targeting_cs:
            fail(f"AbilityTargeting.cs 缺少扫描缓存: {token}")
    fire_cs = Path(CS).read_text(encoding="utf-8")
    for token in ("sharedMats", "SharedMat"):
        if token not in fire_cs:
            fail(f"SkillGroundFire.cs 缺少共享材质优化: {token}")
    if "private Material flameMat" in fire_cs or "Object.Destroy(flameMat)" in fire_cs:
        fail("SkillGroundFire.cs 火焰柱不应再持有每柱独立材质实例")

    print("skillfx: OK")


SUITES = {"skillfx": validate_skillfx}
