#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""RoM 职业系统静态验证（mmt-class-design.md §5.6）。

校验点：
- 生成器 --check 等价核对：RoM 技能全集 = mapped + passives + deferred + ignored；
- 生成物引用完整：hediffDef/damageDef/pawnKindDef/mentalStateDef/weatherDef/
  gameConditionDef/spawnThingDef/wallDef/stat/capacity/投射物/图标贴图全部存在；
- PawnClass.cs 的 37 职业表与映射表 classes 一致；
- 职业 Hediff 三阶 defName 与 abilities 累积正确（I⊂II⊂III）。
"""
from __future__ import annotations

import json
import os
import re
import subprocess
import sys

from validate_mod import ROOT, fail, require_files

GAME = r"D:/RimWorld v1.7.4979/Data"
MAPPING = os.path.join(ROOT, "tools/data/mmt_rom_class_mapping.json")
PAWNCLASS_CS = os.path.join(ROOT, "Source/MyModeTest/PawnClass.cs")
OUT_ABILITY_DIR = os.path.join(ROOT, "1.7/Defs/AbilityDefs")
OUT_HEDIFFS = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_RoMClasses.xml")
OUT_PROJECTILES = os.path.join(ROOT, "1.7/Defs/ThingDefs_Misc/MMT_Projectiles_RoM.xml")
OUT_CONDITIONS = os.path.join(ROOT, "1.7/Defs/Misc/MMT_GameConditions_RoM.xml")
OUT_BUILDINGS = os.path.join(ROOT, "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_RoMSummons.xml")
FRAMEWORK_XML = [
    "1.7/Defs/HediffDefs/MMT_Hediffs_ClassSystem.xml",
    "1.7/Defs/ThingDefs_Misc/MMT_Misc_ClassSystem.xml",
    "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_ClassMentor.xml",
    "1.7/Defs/ThingDefs_Items/MMT_Items_ClassToken.xml",
]

_vanilla_cache: dict[str, set[str]] = {}


def _game_defnames() -> dict[str, set[str]]:
    """扫游戏 Core+DLC Defs：{根标签: {defName}}。"""
    if _vanilla_cache:
        return _vanilla_cache
    pattern = re.compile(r"<([A-Za-z0-9_.]+)(?:\s[^>]*)?>\s*<defName>([^<]+)</defName>")
    for dlc in ("Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"):
        base = os.path.join(GAME, dlc, "Defs")
        for dirpath, _dirs, files in os.walk(base):
            for fn in files:
                if not fn.endswith(".xml"):
                    continue
                try:
                    with open(os.path.join(dirpath, fn), encoding="utf-8", errors="ignore") as f:
                        text = f.read()
                except OSError:
                    continue
                for tag, def_name in pattern.findall(text):
                    _vanilla_cache.setdefault(tag, set()).add(def_name)
    return _vanilla_cache


def _xml_defnames(path: str, tag: str) -> set[str]:
    if not os.path.isfile(path):
        return set()
    with open(path, encoding="utf-8") as f:
        text = f.read()
    return set(re.findall(r"<%s[^>]*>\s*<defName>([^<]+)</defName>" % tag, text))


def _check_generator() -> None:
    result = subprocess.run(
        [sys.executable, os.path.join(ROOT, "tools/build_mmt_rom_classes.py"), "--check"],
        capture_output=True, text=True, encoding="utf-8", errors="replace")
    out = (result.stdout or "") + (result.stderr or "")
    if result.returncode != 0 or "FAIL" in out:
        fail("生成器 --check 未通过：\n" + out.strip()[-2000:])


def validate_rom_classes() -> None:
    require_files([os.path.relpath(p, ROOT) for p in [MAPPING, PAWNCLASS_CS, OUT_HEDIFFS,
                   OUT_PROJECTILES, OUT_CONDITIONS, OUT_BUILDINGS]])
    with open(MAPPING, encoding="utf-8") as f:
        mapping = json.load(f)
    game = _game_defnames()

    _check_generator()

    # PawnClass.cs 职业表 = 映射表 classes
    with open(PAWNCLASS_CS, encoding="utf-8") as f:
        cs = f.read()
    cs_names = set(re.findall(r'baseName = "([A-Za-z]+)"', cs))
    map_names = set(mapping["classes"].keys())
    if cs_names != map_names:
        fail("PawnClass.cs 职业表与映射表不一致：仅C# %s / 仅映射 %s"
             % (sorted(cs_names - map_names), sorted(map_names - cs_names)))

    # 生成物 def 集合
    gen_hediffs = _xml_defnames(OUT_HEDIFFS, "HediffDef") | _xml_defnames(FRAMEWORK_XML[0], "HediffDef")
    gen_projectiles = _xml_defnames(OUT_PROJECTILES, "ThingDef")
    gen_buildings = _xml_defnames(OUT_BUILDINGS, "ThingDef")
    gen_conditions = _xml_defnames(OUT_CONDITIONS, "GameConditionDef")
    vanilla_hediffs = game.get("HediffDef", set())
    vanilla_things = game.get("ThingDef", set())
    mmt_thing_text = ""
    for d in ("ThingDefs_Resources", "ThingDefs_Items", "ThingDefs_Buildings", "ThingDefs_Misc", "ThingDefs_Weapons"):
        dd = os.path.join(ROOT, "1.7/Defs", d)
        for fn in os.listdir(dd):
            if fn.endswith(".xml"):
                with open(os.path.join(dd, fn), encoding="utf-8") as f:
                    mmt_thing_text += f.read()

    known_hediffs = gen_hediffs | vanilla_hediffs | set(mapping.get("externalHediffs", []))
    known_stats = game.get("StatDef", set())
    known_caps = game.get("PawnCapacityDef", set())

    abilities = mapping["abilities"]
    buildings = mapping.get("summonBuildings", {})
    problems = []

    def need(cond, msg):
        if not cond:
            problems.append(msg)

    for mmt_def, ab in abilities.items():
        p = ab.get("params", {})
        comps = [dict(comp=ab["comp"], params=p)] + ab.get("extraComps", [])
        for c in comps:
            cp = c.get("params", {})
            comp = c["comp"]
            for key in ("hediff", "aura", "hitHediffDef", "targetHediffDef", "moodHediff",
                        "stunHediff", "selfBuffHediff", "revivedHediff", "casterHediff", "fateHediff"):
                if cp.get(key):
                    need(cp[key] in known_hediffs, "%s: Hediff %s 不存在" % (mmt_def, cp[key]))
            for hd in cp.get("hediffPool", []):
                need(hd in known_hediffs, "%s: hediffPool %s 不存在" % (mmt_def, hd))
            if cp.get("damageDef"):
                need(cp["damageDef"] in game.get("DamageDef", set()),
                     "%s: DamageDef %s 不存在" % (mmt_def, cp["damageDef"]))
            if cp.get("pawnKind"):
                need(cp["pawnKind"] in game.get("PawnKindDef", set()),
                     "%s: PawnKindDef %s 不存在" % (mmt_def, cp["pawnKind"]))
            if cp.get("building"):
                need(cp["building"] in buildings, "%s: 召唤建筑 %s 未在 summonBuildings 定义" % (mmt_def, cp["building"]))
            if cp.get("mentalState"):
                need(cp["mentalState"] in game.get("MentalStateDef", set()),
                     "%s: MentalStateDef %s 不存在" % (mmt_def, cp["mentalState"]))
            if cp.get("weatherDef"):
                need(cp["weatherDef"] in game.get("WeatherDef", set()),
                     "%s: WeatherDef %s 不存在" % (mmt_def, cp["weatherDef"]))
            if cp.get("gameConditionDef"):
                need(cp["gameConditionDef"] in game.get("GameConditionDef", set()) | set(mapping.get("gameConditions", {})),
                     "%s: GameConditionDef %s 不存在" % (mmt_def, cp["gameConditionDef"]))
            if cp.get("shortenConditionDef"):
                need(cp["shortenConditionDef"] in game.get("GameConditionDef", set()),
                     "%s: shortenConditionDef %s 不存在" % (mmt_def, cp["shortenConditionDef"]))
            for key in ("spawnThingDef", "wallDef"):
                if cp.get(key):
                    need(cp[key] in vanilla_things or cp[key] in mmt_thing_text or cp[key] in gen_buildings,
                         "%s: ThingDef %s 不存在" % (mmt_def, cp[key]))
            if cp.get("projectile"):
                need(cp["projectile"] in gen_projectiles or cp["projectile"] in vanilla_things,
                     "%s: 投射物 %s 未生成" % (mmt_def, cp["projectile"]))

    # 光环目标 hediff 闭环
    hediffs = mapping["hediffs"]
    for def_name, spec in hediffs.items():
        if spec.get("aura", {}).get("targetHediff"):
            need(spec["aura"]["targetHediff"] in hediffs,
                 "光环 %s 的目标 Hediff %s 未定义" % (def_name, spec["aura"]["targetHediff"]))
        for group in ("statOffsets", "statFactors"):
            for stat in spec.get(group, {}):
                need(stat in known_stats, "Hediff %s: StatDef %s 不存在" % (def_name, stat))
        for cap in spec.get("capMods", {}):
            need(cap in known_caps, "Hediff %s: PawnCapacityDef %s 不存在" % (def_name, cap))

    # 职业阶位被动 stat 有效 + abilities 引用映射内技能
    for cls_key, cls in mapping["classes"].items():
        for tier, passive in cls.get("passives", {}).items():
            for group in ("statOffsets", "statFactors"):
                for stat in passive.get(group, {}):
                    need(stat in known_stats, "职业 %s 阶位被动: StatDef %s 不存在" % (cls_key, stat))
        for a in cls.get("abilities", []):
            need(a in abilities, "职业 %s 声明的技能 %s 不在映射 abilities 中" % (cls_key, a))

    # 职业 Hediff 三阶生成（v2：阶位 Hediff 不再携带 abilities，技能授予移交天赋树，见 validate/talent_tree.py）
    with open(OUT_HEDIFFS, encoding="utf-8") as f:
        hediffs_xml = f.read()
    for cls_key in mapping["classes"]:
        for roman in ("I", "II", "III"):
            dn = "MMT_Class_%s_%s" % (cls_key, roman)
            need(dn in gen_hediffs, "职业 Hediff %s 未生成" % dn)
            m = re.search(r"<defName>%s</defName>(.*?)<stages>" % dn, hediffs_xml, re.S)
            need(m is not None and "<abilities>" not in m.group(1),
                 "职业 Hediff %s 仍携带 abilities（v2 应移交天赋树）" % dn)

    # 图标/投射物贴图存在
    for fn in os.listdir(OUT_ABILITY_DIR):
        if not fn.startswith("MMT_Abilities_RoM_"):
            continue
        with open(os.path.join(OUT_ABILITY_DIR, fn), encoding="utf-8") as f:
            text = f.read()
        for icon in re.findall(r"<iconPath>([^<]+)</iconPath>", text):
            rel = icon.replace("UI/RoM/", "1.7/Textures/UI/RoM/") + ".png"
            need(os.path.isfile(os.path.join(ROOT, rel)), "%s: 图标缺失 %s" % (fn, icon))
    if os.path.isfile(OUT_PROJECTILES):
        with open(OUT_PROJECTILES, encoding="utf-8") as f:
            ptext = f.read()
        for tex in re.findall(r"<texPath>([^<]+)</texPath>", ptext):
            need(os.path.isfile(os.path.join(ROOT, "1.7/Textures", tex + ".png")),
                 "投射物贴图缺失 %s" % tex)

    # 召唤建筑伤害引用
    for bdef, bspec in buildings.items():
        if bspec.get("damageDef"):
            need(bspec["damageDef"] in game.get("DamageDef", set()),
                 "召唤建筑 %s: DamageDef %s 不存在" % (bdef, bspec["damageDef"]))
        need(os.path.isfile(os.path.join(ROOT, "1.7/Textures", bspec.get("tex", "") + ".png")),
             "召唤建筑 %s: 贴图缺失 %s" % (bdef, bspec.get("tex")))

    for msg in problems:
        fail(msg)


SUITES = {"rom_classes": validate_rom_classes}
