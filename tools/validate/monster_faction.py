#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""MMT 魔物隐藏派系静态验证（由 tools/validate_new.py 自动发现）。

校验点：
- MMT_Monster 派系 Def 关键字段（permanentEnemy、hidden、永不袭击、世界生成必有 1 个）；
- 火元素领主 PawnKindDef 挂 DefModExtension_MMTMonsterFaction（派系路由标记）；
- 小火元素召唤物技能 MMT_SummonFire_FlameBolt 链路（AbilityDef → abilities → ThinkTree）；
- C# 侧 MonsterFaction.cs / BossPool.cs / BossSummoner.cs 接线（modExtension 路由、
  DEV 操控放技能 DebugAction）。
"""
from __future__ import annotations

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

FACTION_XML = "1.7/Defs/FactionDefs/MMT_FactionDefs_Monster.xml"
KINDS_XML = "1.7/Defs/PawnKindDefs/MMT_PawnKinds_FireElemental.xml"
ABILITIES_XML = "1.7/Defs/AbilityDefs/MMT_Abilities_FireBoss.xml"
TREES_XML = "1.7/Defs/ThinkTreeDefs/MMT_ThinkTrees_FireElemental.xml"
PROJECTILES_XML = "1.7/Defs/ThingDefs_Misc/MMT_Projectiles_SkillDemo.xml"
MONSTER_CS = "Source/MyModeTest/MonsterFaction.cs"
BOSSPOOL_CS = "Source/MyModeTest/BossPool.cs"
SUMMONER_CS = "Source/MyModeTest/BossSummoner.cs"

FACTION = "MMT_Monster"
EXT_CLASS = "MyModeTest.DefModExtension_MMTMonsterFaction"
BOSS_KIND = "MMT_BossFireElementalLord"
LESSER_KIND = "MMT_FireElementalLesser"
FLAME_BOLT = "MMT_SummonFire_FlameBolt"


def validate_monster_faction() -> None:
    require_files([FACTION_XML, KINDS_XML, ABILITIES_XML, TREES_XML,
                   PROJECTILES_XML, MONSTER_CS, BOSSPOOL_CS, SUMMONER_CS])

    # ---- 派系 Def：隐藏、只对玩家永久敌对、永不主动袭击、世界生成必有 ----
    factions = def_map(parse_xml(FACTION_XML))
    if FACTION not in factions:
        fail(f"FactionDef 缺少 {FACTION}")
    fac = factions[FACTION]
    for tag, expected in (
        ("hidden", "true"),
        ("displayInFactionSelection", "false"),
        ("humanlikeFaction", "false"),
        ("permanentEnemy", "true"),
        ("hostileToFactionlessHumanlikes", "true"),
        ("autoFlee", "false"),
        ("canUseAvoidGrid", "false"),
        ("techLevel", "Animal"),
        ("requiredCountAtGameStart", "1"),
        ("maxConfigurableAtWorldCreation", "1"),
    ):
        assert_equal(child_text(fac, tag), expected, f"派系字段 {tag}")
    curve = fac.find("raidCommonalityFromPointsCurve")
    points = [li.text.strip() for li in (curve.find("points") if curve is not None else [])]
    if points != ["(0, 0)"]:
        fail(f"派系必须永不主动袭击：raidCommonalityFromPointsCurve 应为单点 (0, 0)，当前 {points}")
    pgm = fac.find("pawnGroupMakers")
    if pgm is None or pgm.get("Inherit") != "False" or len(pgm):
        fail("派系 pawnGroupMakers 应为空列表且 Inherit=False")

    # ---- PawnKindDef：火元素领主挂魔物派系标记；小火元素挂召唤物技能 ----
    kinds = def_map(parse_xml(KINDS_XML))
    boss = kinds.get(BOSS_KIND)
    if boss is None:
        fail(f"PawnKindDef 缺少 {BOSS_KIND}")
    exts = boss.find("modExtensions")
    if exts is None or EXT_CLASS not in "".join(li.get("Class") or "" for li in exts):
        fail(f"{BOSS_KIND} 未挂 {EXT_CLASS}（魔物派系路由标记）")
    lesser = kinds.get(LESSER_KIND)
    if lesser is None:
        fail(f"PawnKindDef 缺少 {LESSER_KIND}")
    if FLAME_BOLT not in li_values(lesser, "abilities"):
        fail(f"{LESSER_KIND} abilities 未挂 {FLAME_BOLT}")

    # ---- 召唤物技能：纯 CD、AI 可用、MMTSingleShot 投射物桶接线 ----
    abilities = def_map(parse_xml(ABILITIES_XML))
    bolt = abilities.get(FLAME_BOLT)
    if bolt is None:
        fail(f"AbilityDef 缺少 {FLAME_BOLT}")
    if bolt.get("ParentName") is not None:
        fail(f"{FLAME_BOLT} 不应挂父类（纯 CD 路线）")
    assert_equal(child_text(bolt, "aiCanUse"), "true", f"{FLAME_BOLT} AI 可用")
    assert_equal(child_text(bolt, "ai_IsOffensive"), "true", f"{FLAME_BOLT} 进攻型")
    assert_equal(child_text(bolt, "showPsycastEffects"), "false", f"{FLAME_BOLT} 关闭灵能表现")
    if bolt.find("cooldownTicksRange") is None:
        fail(f"{FLAME_BOLT} 缺少 cooldownTicksRange")
    comps = bolt.find("comps")
    comp_classes = "" if comps is None else "".join(c.get("Class") or "" for c in comps)
    if "MyModeTest.CompProperties_MMTSingleShot" not in comp_classes:
        fail(f"{FLAME_BOLT} 未接 MMTSingleShot 投射物桶")
    if f"<projectileDef>MMT_Proj_SkillFireball</projectileDef>" not in read_text(ABILITIES_XML):
        fail(f"{FLAME_BOLT} 投射物应为 MMT_Proj_SkillFireball")
    if "MMT_Proj_SkillFireball" not in read_text(PROJECTILES_XML):
        fail(f"投射物 MMT_Proj_SkillFireball 未在 {PROJECTILES_XML} 定义")

    # ---- ThinkTree：小火元素主树带火焰弹节点 ----
    trees = read_text(TREES_XML)
    if f"<ability>{FLAME_BOLT}</ability>" not in trees:
        fail("小火元素 ThinkTree 缺少火焰弹的 JobGiver_AIAbilityFight 节点")

    # ---- C#：modExtension 标记类、派系惰性解析、BossPool 第一优先路由、DEV 操控工具 ----
    monster_cs = read_text(MONSTER_CS)
    for needle in ("DefModExtension_MMTMonsterFaction", "FirstFactionOfDef", FACTION):
        if needle not in monster_cs:
            fail(f"MonsterFaction.cs 缺少关键实现: {needle}")
    if "HasModExtension<DefModExtension_MMTMonsterFaction>" not in read_text(BOSSPOOL_CS):
        fail("BossPool.ResolveBossFaction 缺少魔物派系 modExtension 路由分支")
    summoner_cs = read_text(SUMMONER_CS)
    for needle in ("DevBossControl", "QueueCastingJob", "DebugAction"):
        if needle not in summoner_cs:
            fail(f"BossSummoner.cs 缺少 DEV 操控放技能实现: {needle}")

    print("monster_faction: OK")


SUITES = {"monster_faction": validate_monster_faction}
