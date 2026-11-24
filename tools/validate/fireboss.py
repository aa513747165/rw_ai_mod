#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""熔火元素领主 Boss 静态验证（由 tools/validate_new.py 自动发现）。

校验点：
- 种族/PawnKindDef/技能/ThinkTree 四件套齐全，关键字段符合设计（纯 CD 技能、
  fleshType Insectoid 派系解析、Flammability 0 免疫点燃、combatPower 档位）；
- 三向贴图存在（Animal 渲染树 bodyGraphicData 按 _south/_east/_north 取图）；
- Boss 白名单已登记进 BossLair 据点与 Boss 召唤器两处。
"""
from __future__ import annotations

import os

from validate_mod import (
    assert_equal,
    child_text,
    def_map,
    fail,
    li_values,
    parse_xml,
    read_text,
    require_files,
)

RACES_XML = "1.7/Defs/ThingDefs_Races/MMT_Races_FireElemental.xml"
KINDS_XML = "1.7/Defs/PawnKindDefs/MMT_PawnKinds_FireElemental.xml"
TREES_XML = "1.7/Defs/ThinkTreeDefs/MMT_ThinkTrees_FireElemental.xml"
ABILITIES_XML = "1.7/Defs/AbilityDefs/MMT_Abilities_FireBoss.xml"
DUNGEONS_XML = "1.7/Defs/WorldObjectDefs/MMT_WorldObjects_RpgDungeons.xml"
SUMMONER_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Dungeon.xml"
TEX_DIR = "1.7/Textures/Things/Pawn/FireElemental"

BOSS_KIND = "MMT_BossFireElementalLord"
LESSER_KIND = "MMT_FireElementalLesser"
ABILITIES = ("MMT_BossFire_SummonLesser", "MMT_BossFire_FireRain", "MMT_BossFire_FlamePillar")


def validate_fireboss() -> None:
    require_files([RACES_XML, KINDS_XML, TREES_XML, ABILITIES_XML, DUNGEONS_XML, SUMMONER_XML])

    # ---- 种族：免疫点燃、虫族血肉（派系解析关键）、自定义 ThinkTree、近战工具 ----
    races = def_map(parse_xml(RACES_XML))
    for name, speed in (("MMT_FireElementalLord", "3.2"), ("MMT_FireElementalLesser", "4.2")):
        if name not in races:
            fail(f"种族 ThingDef 缺少 {name}")
    for name in ("MMT_FireElementalLord", "MMT_FireElementalLesser"):
        race = races[name]
        race_node = race.find("race")
        stats = race.find("statBases")
        if race_node is None or stats is None:
            fail(f"{name} 缺少 race/statBases")
        if child_text(stats, "Flammability") != "0":
            fail(f"{name} 必须免疫点燃（Flammability 0）")
        assert_equal(child_text(race_node, "fleshType"), "Insectoid",
                     f"{name} fleshType（Insectoid=BossPool 派系解析关键，勿改）")
        assert_equal(child_text(race_node, "renderTree"), "Animal", f"{name} 渲染树")
        assert_equal(child_text(race_node, "thinkTreeMain"), "MMT_" + name.split("_", 1)[1],
                     f"{name} 主 ThinkTree")
        assert_equal(child_text(race_node, "thinkTreeConstant"), "MMT_FireElementalConstant",
                     f"{name} constant 树")
        if race.find("tools") is None:
            fail(f"{name} 缺少近战 tools")

    # ---- PawnKindDef：战力档位、isFighter、技能挂载、贴图路径 ----
    kinds = def_map(parse_xml(KINDS_XML))
    if BOSS_KIND not in kinds or LESSER_KIND not in kinds:
        fail(f"PawnKindDef 缺少 {BOSS_KIND} 或 {LESSER_KIND}")
    boss = kinds[BOSS_KIND]
    assert_equal(child_text(boss, "race"), "MMT_FireElementalLord", "Boss 种族引用")
    boss_power = int(child_text(boss, "combatPower") or "0")
    if not 200 <= boss_power <= 1500:
        fail(f"Boss combatPower 应在 200~1500 区间（Boss 巢穴预算 800~5000×0.5），当前 {boss_power}")
    assert_equal(child_text(boss, "isFighter"), "true", "Boss 必须 isFighter（全库扫描入池门槛）")
    boss_abilities = li_values(boss, "abilities")
    for ab in ABILITIES:
        if ab not in boss_abilities:
            fail(f"Boss PawnKindDef 未挂技能 {ab}")
    assert_equal(child_text(kinds[LESSER_KIND], "race"), "MMT_FireElementalLesser", "小元素种族引用")

    # ---- 贴图：每个角色三向齐全 ----
    for stem in ("MMT_FireElementalLord", "MMT_FireElementalLesser"):
        for rot in ("south", "east", "north"):
            path = os.path.join(TEX_DIR, f"{stem}_{rot}.png")
            if not os.path.isfile(path):
                fail(f"缺少贴图 {path}")

    # ---- 技能：纯 CD（无 PsycastBase 父类、无灵能熵耗）、AI 可用、效果桶接线 ----
    abilities = def_map(parse_xml(ABILITIES_XML))
    for ab in ABILITIES:
        if ab not in abilities:
            fail(f"AbilityDef 缺少 {ab}")
        node = abilities[ab]
        if node.get("ParentName") is not None:
            fail(f"{ab} 不应挂 PsycastBase 父类（纯 CD 路线）")
        assert_equal(child_text(node, "aiCanUse"), "true", f"{ab} AI 可用")
        assert_equal(child_text(node, "showPsycastEffects"), "false", f"{ab} 关闭灵能表现")
        if node.find("cooldownTicksRange") is None:
            fail(f"{ab} 缺少 cooldownTicksRange")
        verb = node.find("verbProperties")
        if verb is None or child_text(verb, "soundCast") is None:
            fail(f"{ab} 缺少 verbProperties.soundCast")
        comps = node.find("comps")
        if comps is None or "MyModeTest.CompProperties_MMT" not in "".join(
                c.get("Class") or "" for c in comps):
            fail(f"{ab} 未接 MMT 效果桶")
    summon = abilities["MMT_BossFire_SummonLesser"]
    summon_text = read_text(ABILITIES_XML)
    if f"<pawnKindDef>{LESSER_KIND}</pawnKindDef>" not in summon_text:
        fail("召唤技能 pawnKindDef 必须指向 MMT_FireElementalLesser")

    # ---- ThinkTree：三个技能各挂一个 JobGiver_AIAbilityFight ----
    trees = read_text(TREES_XML)
    for ab in ABILITIES:
        if f"<ability>{ab}</ability>" not in trees:
            fail(f"ThinkTree 缺少 {ab} 的 JobGiver_AIAbilityFight 节点")
    if trees.count('<li Class="JobGiver_AIAbilityFight">') != 3:
        fail("Boss ThinkTree 应恰好 3 个 JobGiver_AIAbilityFight（一技一节点）")

    # ---- 白名单：BossLair 据点 + Boss 召唤器双登记 ----
    for path in (DUNGEONS_XML, SUMMONER_XML):
        if f"<li>{BOSS_KIND}</li>" not in read_text(path):
            fail(f"{path} 的 bossPawnKindDefNames 未登记 {BOSS_KIND}")

    print("fireboss: OK")


SUITES = {"fireboss": validate_fireboss}
