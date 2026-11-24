#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""MMT 职业系统生成器（mmt-class-design.md §2.4）。

输入：tools/data/mmt_rom_class_mapping.json（唯一人工维护点：每技能一条
{romDef, mmtDef, comp, 参数, 威力档, 中文label来源, 状态}）。

产出（均为生成物，要改就改本脚本/映射表后重跑）：
  - 1.7/Defs/AbilityDefs/MMT_Abilities_RoM_<职业>.xml ×37
  - 1.7/Defs/HediffDefs/MMT_Hediffs_RoMClasses.xml（37 职业×3 阶 + 技能 buff/debuff/光环 Hediff）
  - 1.7/Defs/ThingDefs_Misc/MMT_Projectiles_RoM.xml（投射物桶的 ThingDef）
  - 1.7/Defs/Misc/MMT_GameConditions_RoM.xml（气象法师自定义事件条件）
  - 1.7/Textures/UI/RoM/（技能图标）、1.7/Textures/Things/Projectile/RoM/（投射物贴图）

用法：
  python tools/build_mmt_rom_classes.py          # 全量生成 + 复制贴图
  python tools/build_mmt_rom_classes.py --check  # 核对 RoM 技能全集 = mapped + passive + deferred + ignored
"""
import json
import os
import re
import shutil
import sys
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ROM = r"D:/steamcmd/steamapps/workshop/content/294100/3823345404"
MAPPING_PATH = os.path.join(ROOT, "tools/data/mmt_rom_class_mapping.json")
OUT_ABILITY_DIR = os.path.join(ROOT, "1.7/Defs/AbilityDefs")
OUT_HEDIFFS = os.path.join(ROOT, "1.7/Defs/HediffDefs/MMT_Hediffs_RoMClasses.xml")
OUT_PROJECTILES = os.path.join(ROOT, "1.7/Defs/ThingDefs_Misc/MMT_Projectiles_RoM.xml")
OUT_CONDITIONS = os.path.join(ROOT, "1.7/Defs/Misc/MMT_GameConditions_RoM.xml")
OUT_BUILDINGS = os.path.join(ROOT, "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_RoMSummons.xml")
OUT_ICON_DIR = os.path.join(ROOT, "1.7/Textures/UI/RoM")
OUT_PROJ_TEX_DIR = os.path.join(ROOT, "1.7/Textures/Things/Projectile/RoM")

# 威力档 → 灵能消耗（mmt-class-design.md §2.2）
TIERS = {
    "small": {"entropy": 8, "psyfocus": 0.01},
    "mid": {"entropy": 16, "psyfocus": 0.02},
    "big": {"entropy": 35, "psyfocus": 0.04},
}

# RoM 同名分级技能后缀（--check 归并基名）
VARIANT_SUFFIXES = ["_Mass", "_Projected", "_Global", "DW", "_IV", "_III", "_II", "_I"]

errors = []
warnings = []


def fail(msg):
    errors.append(msg)


def warn(msg):
    warnings.append(msg)


# ---------------- RoM 解析（图标/冷却/汉化标签） ----------------

def rom_base_name(def_name):
    """去掉分级后缀得到技能基名（TM_ShadowBolt_I → TM_ShadowBolt）。"""
    changed = True
    while changed:
        changed = False
        for suf in VARIANT_SUFFIXES:
            if def_name.endswith(suf) and len(def_name) > len(suf):
                def_name = def_name[: -len(suf)]
                changed = True
    return def_name


def parse_rom_abilities():
    """解析 RoM 全部 TMAbilityDef（AbilityDefs/ + CustomDefs/TM_WeatherMage.xml）。"""
    abilities = {}
    files = []
    adir = os.path.join(ROM, "v1.7/Defs/AbilityDefs")
    for fn in sorted(os.listdir(adir)):
        if fn.endswith(".xml"):
            files.append(os.path.join(adir, fn))
    files.append(os.path.join(ROM, "v1.7/Defs/CustomDefs/TM_WeatherMage.xml"))
    for path in files:
        try:
            tree = ET.parse(path)
        except ET.ParseError:
            continue
        for el in tree.getroot():
            if not (el.tag.endswith("TMAbilityDef") or el.tag == "AbilityDef"):
                continue
            def_el = el.find("defName")
            if def_el is None or not def_el.text:
                continue
            entry = {"icon": None, "cooldown": None, "label": None}
            icon = el.find("uiIconPath")
            if icon is None:
                icon = el.find("iconPath")
            if icon is not None and icon.text:
                entry["icon"] = icon.text.strip()
            lab = el.find("label")
            if lab is not None and lab.text:
                entry["label"] = lab.text.strip()
            verb = el.find("MainVerb") if el.find("MainVerb") is not None else el.find("verbProperties")
            if verb is not None:
                cd = verb.find("SecondsToRecharge")
                if cd is not None and cd.text:
                    entry["cooldown"] = cd.text.strip()
            abilities[def_el.text.strip()] = entry
    return abilities


def parse_rom_zh_labels():
    """RoM 自带汉化（共享层 + career 层）的 TMAbilityDef .label 条目。"""
    labels = {}
    for sub in ("Languages/ChineseSimplified/DefInjected/TorannMagic.TMAbilityDef",
                "v1.7/Languages/ChineseSimplified/DefInjected/TorannMagic.TMAbilityDef",
                "v1.7/Languages/ChineseSimplified/DefInjected/AbilityDef"):
        d = os.path.join(ROM, sub)
        if not os.path.isdir(d):
            continue
        for fn in os.listdir(d):
            if not fn.endswith(".xml"):
                continue
            try:
                tree = ET.parse(os.path.join(d, fn))
            except ET.ParseError:
                continue
            for el in tree.getroot():
                if el.tag.endswith(".label") and el.text:
                    labels[el.tag[: -len(".label")]] = el.text.strip()
    return labels


# ---------------- XML 生成 ----------------

def x(tag, value, indent):
    return " " * indent + "<%s>%s</%s>" % (tag, escape(str(value)), tag)


def target_params_xml(target, indent=8):
    """按目标类型产出 targetParams。"""
    pad = " " * indent
    if target == "self":
        body = [x("canTargetSelf", "true", indent + 2), x("canTargetPawns", "false", indent + 2),
                x("canTargetBuildings", "false", indent + 2), x("canTargetLocations", "false", indent + 2),
                x("canTargetAnimals", "false", indent + 2), x("canTargetHumans", "false", indent + 2),
                x("canTargetMechs", "false", indent + 2)]
    elif target == "corpse":
        body = [x("canTargetPawns", "false", indent + 2), x("canTargetBuildings", "false", indent + 2),
                x("canTargetItems", "false", indent + 2), x("canTargetCorpses", "true", indent + 2),
                x("onlyTargetCorpses", "true", indent + 2)]
    elif target == "friendly":
        body = [x("canTargetPawns", "true", indent + 2), x("canTargetSelf", "true", indent + 2),
                x("canTargetAnimals", "true", indent + 2), x("canTargetBuildings", "false", indent + 2),
                x("mapObjectTargetsMustBeAutoAttackable", "false", indent + 2)]
    elif target == "location":
        body = [x("canTargetLocations", "true", indent + 2), x("canTargetPawns", "true", indent + 2),
                x("canTargetAnimals", "true", indent + 2), x("canTargetMechs", "true", indent + 2),
                x("mapObjectTargetsMustBeAutoAttackable", "false", indent + 2)]
    else:  # enemy
        body = [x("canTargetPawns", "true", indent + 2), x("canTargetAnimals", "true", indent + 2),
                x("canTargetMechs", "true", indent + 2), x("canTargetSelf", "false", indent + 2),
                x("canTargetBuildings", "false", indent + 2),
                x("mapObjectTargetsMustBeAutoAttackable", "false", indent + 2)]
    return pad + "<targetParams>\n" + "\n".join(body) + "\n" + pad + "</targetParams>"


def comp_xml(ab):
    """按 comp 桶产出 comps 节点（无桶组件返回空串）。"""
    comp = ab["comp"]
    p = ab.get("params", {})

    def li(props_class, fields=None, comp_class=None):
        lines = ['      <li Class="%s">' % props_class]
        if comp_class:
            lines.append(x("compClass", comp_class, 8))
        for k, v in (fields or {}).items():
            if isinstance(v, list):
                lines.append("        <%s>" % k)
                for item in v:
                    lines.append(x("li", item, 10))
                lines.append("        </%s>" % k)
            else:
                lines.append(x(k, v, 8))
        lines.append("      </li>")
        return "\n".join(lines)

    if comp == "GiveHediff":
        return li("CompProperties_AbilityGiveHediff", {
            "hediffDef": p["hediff"], "psychic": "true", "durationMultiplier": "PsychicSensitivity",
            "applicableToMechs": "false"})
    if comp == "GiveHediffAoE":
        return li("MyModeTest.CompProperties_MMTGiveHediffAoE", {
            "hediffDef": p["hediff"], "radius": p.get("radius", 6),
            "targetFriendly": str(p.get("targetFriendly", True)).lower(),
            "includeSelf": str(p.get("includeSelf", True)).lower()})
    if comp == "LaunchProjectile":
        return li("CompProperties_AbilityLaunchProjectile", {"projectileDef": p["projectile"]})
    if comp == "AoEDamage":
        fields = {"damageDef": p["damageDef"], "damageAmount": p["damage"],
                  "radius": p.get("radius", 3), "armorPen": p.get("armorPen", 0),
                  "pulses": p.get("pulses", 1), "pulseIntervalTicks": p.get("pulseIntervalTicks", 60)}
        for opt in ("randomStrikes", "friendlyFire", "startFireChance", "hitHediffDef", "fleckDefName"):
            if opt in p:
                fields[opt] = str(p[opt]).lower() if isinstance(p[opt], bool) else p[opt]
        return li("MyModeTest.CompProperties_MMTAoEDamage", fields)
    if comp == "Heal":
        fields = {"healAmount": p.get("heal", 10), "maxInjuries": p.get("maxInjuries", 3)}
        for opt in ("radius", "tendBleeding", "cureInfection", "cureScar"):
            if opt in p:
                fields[opt] = str(p[opt]).lower() if isinstance(p[opt], bool) else p[opt]
        return li("MyModeTest.CompProperties_MMTHeal", fields)
    if comp == "Summon":
        if p.get("building"):
            return li("MyModeTest.CompProperties_MMTSummon", {"buildingDef": p["building"]})
        fields = {"pawnKindDef": p["pawnKind"], "count": p.get("count", 1),
                  "durationTicks": p.get("durationTicks", 18000), "maxActive": p.get("maxActive", 6)}
        if p.get("fromCorpse"):
            fields["fromCorpse"] = "true"
        return li("MyModeTest.CompProperties_MMTSummon", fields)
    if comp == "DashStrike":
        fields = {"damageDef": p.get("damageDef", "Cut"), "damageAmount": p.get("damage", 20)}
        for opt in ("radius", "armorPen", "returnToStart"):
            if opt in p:
                fields[opt] = str(p[opt]).lower() if isinstance(p[opt], bool) else p[opt]
        return li("MyModeTest.CompProperties_MMTDashStrike", fields)
    if comp == "MentalState":
        return li("CompProperties_AbilityGiveMentalState", {
            "stateDef": p["mentalState"], "psychic": "true", "durationMultiplier": "PsychicSensitivity"})
    if comp == "MentalStateAoE":
        fields = {"mentalStateDef": p["mentalState"], "radius": p.get("radius", 6),
                  "targetFriendly": str(p.get("targetFriendly", False)).lower()}
        if p.get("animalsOnly"):
            fields["animalsOnly"] = "true"
        return li("MyModeTest.CompProperties_MMTMentalStateAoE", fields)
    if comp == "SpawnEffect":
        fields = {}
        for opt in ("gameConditionDef", "conditionDurationTicks", "weatherDef",
                    "spawnThingDef", "spawnCount", "extinguishRadius", "shortenConditionDef", "shortenFactor"):
            if opt in p:
                fields[opt] = p[opt]
        return li("MyModeTest.CompProperties_MMTSpawnEffect", fields)
    if comp == "MultiShot":
        return li("MyModeTest.CompProperties_MMTMultiShot", {
            "projectileDef": p["projectile"], "shotCount": p.get("shotCount", 5),
            "scatterRadius": p.get("scatterRadius", 1.5)})
    if comp == "ToggleAura":
        return li("MyModeTest.CompProperties_MMTToggleAura", {"auraHediffDef": p["aura"]})
    if comp == "Teleport":
        return li("CompProperties_AbilityTeleport")
    if comp == "Stun":
        return li("CompProperties_AbilityStun")
    if comp == "Smokepop":
        return li("CompProperties_AbilityEffect", comp_class="CompAbilityEffect_Smokepop")
    if comp == "Wallraise":
        return li("CompProperties_AbilityWallraise", {"thingDef": p.get("wallDef", "RaisedRocks")})
    if comp == "ResurrectLite":
        fields = {}
        if p.get("revivedHediff"):
            fields["revivedHediffDef"] = p["revivedHediff"]
        if p.get("casterHediff"):
            fields["casterHediffDef"] = p["casterHediff"]
        return li("MyModeTest.CompProperties_MMTResurrectLite", fields)
    if comp == "SelfEntropy":
        fields = {"entropyChange": p.get("entropyChange", -10)}
        for opt in ("selfDamage", "healAmount", "targetHediffDef"):
            if opt in p:
                fields[opt] = p[opt]
        return li("MyModeTest.CompProperties_MMTSelfEntropy", fields)
    if comp == "FadeEmotions":
        fields = {}
        if p.get("moodHediff"):
            fields["moodHediffDef"] = p["moodHediff"]
        return li("MyModeTest.CompProperties_MMTFadeEmotions", fields)
    if comp == "Pull":
        return li("CompProperties_AbilityEffect", comp_class="MyModeTest.CompAbilityEffect_MMTPull")
    if comp == "GroupTeleport":
        return li("MyModeTest.CompProperties_MMTGroupTeleport", {"radius": p.get("radius", 4)})
    if comp == "Transpose":
        fields = {}
        if p.get("stunHediff"):
            fields["stunHediffDef"] = p["stunHediff"]
        if p.get("selfBuffHediff"):
            fields["selfBuffHediffDef"] = p["selfBuffHediff"]
        return li("MyModeTest.CompProperties_MMTTranspose", fields)
    if comp == "CombatDrugs":
        return li("MyModeTest.CompProperties_MMTCombatDrugs", {"hediffPool": p["hediffPool"]})
    if comp == "CorpseExplosion":
        return li("MyModeTest.CompProperties_MMTCorpseExplosion", {
            "damageDef": p.get("damageDef", "Bomb"), "damageAmount": p.get("damage", 30),
            "radius": p.get("radius", 2.5), "delayTicks": p.get("delayTicks", 90)})
    if comp == "AlterFate":
        return li("MyModeTest.CompProperties_MMTAlterFate", {"fateHediffDef": p["fateHediff"]})
    if comp == "PistolWhip":
        fields = {"damageAmount": p.get("damage", 12), "disarm": str(p.get("disarm", True)).lower()}
        if p.get("stunHediff"):
            fields["stunHediffDef"] = p["stunHediff"]
        return li("MyModeTest.CompProperties_MMTPistolWhip", fields)
    fail("%s: 未知 comp 桶 %s" % (ab["mmtDef"], comp))
    return ""


def ability_xml(ab, rom_abilities):
    rom = rom_abilities.get(ab["romDef"], {})
    tier = TIERS[ab["tier"]]
    cooldown = ab.get("cooldownSeconds")
    if cooldown is None and rom.get("cooldown"):
        try:
            cooldown = float(rom["cooldown"])
        except ValueError:
            cooldown = None
    hostile = ab["target"] in ("enemy", "location", "corpse")
    lines = ['  <AbilityDef ParentName="PsycastBase">',
             x("defName", ab["mmtDef"], 4),
             x("label", ab["label"], 4),
             x("description", ab.get("description", ab["label"]), 4),
             x("level", 1, 4)]
    if ab.get("iconPath"):
        lines.append(x("iconPath", ab["iconPath"], 4))
    lines.append(x("writeCombatLog", "true" if hostile else "false", 4))
    lines.append(x("casterMustBeCapableOfViolence", "true" if hostile else "false", 4))
    lines.append("    <statBases>")
    lines.append(x("Ability_EntropyGain", ab.get("entropy", tier["entropy"]), 6))
    lines.append(x("Ability_PsyfocusCost", ab.get("psyfocus", tier["psyfocus"]), 6))
    lines.append("    </statBases>")
    if cooldown:
        lines.append(x("cooldownTicksRange", int(float(cooldown) * 60), 4))
    lines.append("    <verbProperties>")
    lines.append(x("warmupTime", ab.get("warmupSeconds", 0.5), 6))
    lines.append(x("range", 0 if ab["target"] == "self" else ab.get("range", 24.9), 6))
    if ab["target"] == "self":
        lines.append(x("targetRequired", "false", 6))
    lines.append(target_params_xml(ab["target"], 6))
    lines.append("    </verbProperties>")
    cx = comp_xml(ab)
    extras = [comp_xml(dict(x, mmtDef=ab["mmtDef"])) for x in ab.get("extraComps", [])]
    if cx or any(extras):
        lines.append("    <comps>")
        if cx:
            lines.append(cx)
        for ex in extras:
            if ex:
                lines.append(ex)
        lines.append("    </comps>")
    lines.append("  </AbilityDef>")
    return "\n".join(lines)


# ---------------- Hediff 生成 ----------------

def hediff_xml(def_name, spec):
    """技能 buff/debuff/光环目标 Hediff。"""
    lines = ["  <HediffDef>",
             x("defName", def_name, 4),
             x("label", spec["label"], 4)]
    if spec.get("description"):
        lines.append(x("description", spec["description"], 4))
    lines.append(x("isBad", str(spec.get("isBad", False)).lower(), 4))
    lines.append(x("everCurableByItem", "false", 4))
    if spec.get("hediffClass"):
        lines.append(x("hediffClass", spec["hediffClass"], 4))
    duration = spec.get("durationTicks")
    stage_lines = []
    if spec.get("hidden"):
        stage_lines.append(x("becomeVisible", "false", 8))
    for group in ("statOffsets", "statFactors"):
        vals = spec.get(group)
        if vals:
            stage_lines.append("        <%s>" % group)
            for k, v in vals.items():
                stage_lines.append(x(k, v, 10))
            stage_lines.append("        </%s>" % group)
    if spec.get("capMods"):
        stage_lines.append("        <capMods>")
        for cap, off in spec["capMods"].items():
            stage_lines.append("          <li>")
            stage_lines.append(x("capacity", cap, 12))
            stage_lines.append(x("offset", off, 12))
            stage_lines.append("          </li>")
        stage_lines.append("        </capMods>")
    if spec.get("painOffset") is not None:
        stage_lines.append(x("painOffset", spec["painOffset"], 8))
    if spec.get("totalBleedFactor") is not None:
        stage_lines.append(x("totalBleedFactor", spec["totalBleedFactor"], 8))
    if spec.get("naturalHealingFactor") is not None:
        stage_lines.append(x("naturalHealingFactor", spec["naturalHealingFactor"], 8))
    if spec.get("hungerRateFactor") is not None:
        stage_lines.append(x("hungerRateFactor", spec["hungerRateFactor"], 8))
    lines.append("    <stages>")
    lines.append("      <li>")
    lines.extend(stage_lines)
    lines.append("      </li>")
    lines.append("    </stages>")
    comps = []
    if duration:
        comps.append('      <li Class="HediffCompProperties_Disappears">')
        comps.append(x("disappearsAfterTicks", duration, 8))
        comps.append("      </li>")
    if spec.get("aura"):
        a = spec["aura"]
        comps.append('      <li Class="MyModeTest.HediffCompProperties_MMTAura">')
        if a.get("targetHediff"):
            comps.append(x("targetHediffDef", a["targetHediff"], 8))
        comps.append(x("radius", a.get("radius", 8), 8))
        comps.append(x("targetFriendly", str(a.get("targetFriendly", True)).lower(), 8))
        comps.append(x("entropyPerPulse", a.get("entropyPerPulse", 0.3), 8))
        comps.append("      </li>")
    if spec.get("alterFate"):
        comps.append('      <li Class="MyModeTest.HediffCompProperties_MMTAlterFate">')
        comps.append(x("durationTicks", spec["alterFate"].get("durationTicks", 36000), 8))
        comps.append("      </li>")
    if comps:
        lines.append("    <comps>")
        lines.extend(comps)
        lines.append("    </comps>")
    lines.append("  </HediffDef>")
    return "\n".join(lines)


def class_hediff_xml(class_key, cls, tier, abilities_by_stage):
    """职业阶位 Hediff：I/II/III 各一 def，仅保留阶位被动 stat。
    v2 天赋树：技能授予移交家族树技能节点（tools/build_mmt_talent_trees.py），阶位 Hediff 不再输出 <abilities>。"""
    roman = {1: "I", 2: "II", 3: "III"}[tier]
    def_name = "MMT_Class_%s_%s" % (class_key, roman)
    label = "%s（%s 阶）" % (cls["label"], roman)
    lines = ["  <HediffDef>",
             x("defName", def_name, 4),
             x("label", label, 4),
             x("description", cls.get("description", "职业阶位：" + label), 4),
             x("isBad", "false", 4),
             x("everCurableByItem", "false", 4),
             x("initialSeverity", 1, 4)]
    passive = cls.get("passives", {}).get(str(tier), {})
    stage_lines = [x("becomeVisible", "false", 8)]
    for group in ("statOffsets", "statFactors"):
        vals = passive.get(group)
        if vals:
            stage_lines.append("        <%s>" % group)
            for k, v in vals.items():
                stage_lines.append(x(k, v, 10))
            stage_lines.append("        </%s>" % group)
    lines.append("    <stages>")
    lines.append("      <li>")
    lines.extend(stage_lines)
    lines.append("      </li>")
    lines.append("    </stages>")
    lines.append("  </HediffDef>")
    return "\n".join(lines)


# ---------------- 投射物生成 ----------------

def projectile_xml(ab):
    p = ab["params"]
    proj = p["projectile"]
    spec = p.get("projectileSpec", {})
    tex = spec.get("tex")
    lines = ['  <ThingDef ParentName="BaseBullet">',
             x("defName", proj, 4),
             x("label", ab["label"], 4)]
    if tex:
        lines.append("    <graphicData>")
        lines.append(x("texPath", "Things/Projectile/RoM/" + tex, 6))
        lines.append(x("graphicClass", "Graphic_Single", 6))
        lines.append(x("shaderType", "Transparent", 6))
        lines.append("    </graphicData>")
    lines.append("    <projectile>")
    lines.append(x("flyOverhead", "false", 6))
    lines.append(x("damageDef", spec.get("damageDef", "Bullet"), 6))
    lines.append(x("damageAmountBase", spec.get("damage", 15), 6))
    lines.append(x("speed", spec.get("speed", 50), 6))
    if spec.get("explosionRadius"):
        lines.append(x("explosionRadius", spec["explosionRadius"], 6))
    if spec.get("armorPenetrationBase"):
        lines.append(x("armorPenetrationBase", spec["armorPenetrationBase"], 6))
    lines.append("    </projectile>")
    lines.append("  </ThingDef>")
    return "\n".join(lines)


# ---------------- 召唤建筑生成 ----------------

def building_xml(def_name, spec):
    """召唤建筑（炮塔/图腾/地雷）：CompMMTSummonTurret + 原版 Lifespan 限时。"""
    lines = ['  <ThingDef ParentName="BuildingBase">',
             x("defName", def_name, 4),
             x("label", spec["label"], 4),
             x("description", spec.get("description", spec["label"]), 4),
             "    <graphicData>",
             x("texPath", spec.get("tex", "Things/Building/Production/RoM/MMT_ClassMentorBench"), 6),
             x("graphicClass", "Graphic_Single", 6),
             x("shaderType", "CutoutComplex", 6),
             x("drawSize", "(1.4,1.4)", 6),
             "    </graphicData>",
             x("altitudeLayer", "Building", 4),
             x("passability", "PassThroughOnly", 4),
             x("fillPercent", "0.4", 4),
             x("rotatable", "false", 4),
             x("tickerType", "Normal", 4),
             x("leaveResourcesWhenKilled", "false", 4),
             "    <statBases>",
             x("MaxHitPoints", spec.get("hitPoints", 120), 6),
             x("Flammability", 0, 6),
             "    </statBases>",
             "    <comps>",
             "      <li Class=\"CompProperties_Lifespan\">",
             x("lifespanTicks", spec.get("lifespanTicks", 36000), 8),
             "      </li>",
             "      <li Class=\"MyModeTest.CompProperties_MMTSummonTurret\">",
             x("mode", spec.get("mode", "attack"), 8)]
    for opt in ("damageDef", "damageAmount", "armorPen", "radius", "intervalTicks",
                "healAmount", "healMaxTargets", "explosionRadius", "fleckDefName"):
        if opt in spec:
            lines.append(x(opt, spec[opt], 8))
    lines += ["      </li>", "    </comps>", "  </ThingDef>"]
    return "\n".join(lines)


# ---------------- 事件条件生成（气象法师） ----------------

def conditions_xml(mapping):
    conds = mapping.get("gameConditions", {})
    if not conds:
        return None
    lines = ['<?xml version="1.0" encoding="utf-8"?>', "<Defs>",
             "  <!-- 气象法师自定义事件条件（生成物：build_mmt_rom_classes.py） -->"]
    for def_name, spec in conds.items():
        lines.append("  <GameConditionDef>")
        lines.append(x("defName", def_name, 4))
        lines.append(x("label", spec["label"], 4))
        lines.append(x("description", spec.get("description", spec["label"]), 4))
        lines.append(x("conditionClass", spec.get("conditionClass", "GameCondition"), 4))
        if spec.get("temperatureOffset") is not None:
            lines.append(x("temperatureOffset", spec["temperatureOffset"], 4))
        lines.append(x("endWhenSilenced", "false", 4))
        lines.append(x("preventRain", "false", 4))
        lines.append(x("canBePermanent", "false", 4))
        lines.append("  </GameConditionDef>")
    lines.append("</Defs>")
    return "\n".join(lines)


# ---------------- 贴图复制 ----------------

def copy_texture(src_rel, dst_dir, mapping_note):
    src = os.path.join(ROM, "Textures", src_rel.replace("/", os.sep) + ".png")
    if not os.path.isfile(src):
        warn("贴图缺失：RoM Textures/%s.png（%s）" % (src_rel, mapping_note))
        return None
    os.makedirs(dst_dir, exist_ok=True)
    base = os.path.basename(src_rel)
    dst = os.path.join(dst_dir, base + ".png")
    if not os.path.isfile(dst):
        shutil.copyfile(src, dst)
    return base


# ---------------- 主流程 ----------------

def main():
    check_only = "--check" in sys.argv
    with open(MAPPING_PATH, encoding="utf-8") as f:
        mapping = json.load(f)
    rom_abilities = parse_rom_abilities()
    zh_labels = parse_rom_zh_labels()

    classes = mapping["classes"]
    abilities = mapping["abilities"]
    hediffs = mapping.get("hediffs", {})
    for mmt_def, ab in abilities.items():
        ab["mmtDef"] = mmt_def

    # 技能标签：copied 优先用 RoM 自带汉化（缺失回退映射表自译）
    for mmt_def, ab in abilities.items():
        if ab.get("status", "mapped") != "mapped":
            continue
        if ab["romDef"] not in rom_abilities:
            fail("%s: romDef %s 在 RoM 中不存在" % (mmt_def, ab["romDef"]))
        if ab.get("labelSource") == "copied":
            zh = zh_labels.get(ab["romDef"])
            if zh:
                ab["label"] = zh
            else:
                warn("%s: 标记 copied 但 RoM 汉化无 %s.label，用映射表 label" % (mmt_def, ab["romDef"]))

    # 图标：映射表 icon（RoM Textures 相对路径，不含扩展名）→ 复制并指向 UI/RoM/<base>
    for mmt_def, ab in abilities.items():
        if ab.get("status", "mapped") != "mapped":
            continue
        icon = ab.get("icon") or (rom_abilities.get(ab["romDef"], {}) or {}).get("icon")
        if not icon:
            warn("%s: 无图标" % mmt_def)
            continue
        if icon.startswith("UI/"):
            out_rel = icon[len("UI/"):]
            dst_dir = OUT_ICON_DIR
        else:
            out_rel = icon
            dst_dir = OUT_ICON_DIR
        if check_only:
            if not os.path.isfile(os.path.join(ROM, "Textures", icon.replace("/", os.sep) + ".png")):
                warn("%s: RoM 图标缺失 Textures/%s.png" % (mmt_def, icon))
            ab["iconPath"] = "UI/RoM/" + os.path.basename(out_rel)
        else:
            base = copy_texture(icon, dst_dir, mmt_def)
            if base:
                ab["iconPath"] = "UI/RoM/" + base

    # --check：RoM 技能全集核对
    mapped_bases = set()
    for ab in abilities.values():
        if ab.get("status", "mapped") == "mapped":
            mapped_bases.add(ab["romDef"])
    passive_bases = {e["romDef"] for e in mapping.get("passives", [])}
    deferred_bases = {e["romDef"] for e in mapping.get("deferred", [])}
    ignored_bases = {e["romDef"] for e in mapping.get("ignored", [])}
    for rom_def in rom_abilities:
        base = rom_base_name(rom_def)
        if base in mapped_bases or base in passive_bases or base in deferred_bases or base in ignored_bases:
            continue
        fail("RoM 技能未登记（mapped/passive/deferred/ignored 均无）：%s" % rom_def)
    for e in mapping.get("deferred", []):
        if e["romDef"] not in rom_abilities and rom_base_name(e["romDef"]) not in {rom_base_name(r) for r in rom_abilities}:
            warn("deferred 条目 %s 在 RoM 中不存在（版本漂移？）" % e["romDef"])

    if check_only:
        for w in warnings:
            print("WARN:", w)
        if errors:
            for e in errors:
                print("FAIL:", e)
            print("--check 未通过：%d 项" % len(errors))
            sys.exit(1)
        print("--check 通过：RoM 技能全集 %d 条全部登记" % len(rom_abilities))
        return

    # 生成技能 XML（按职业分文件）
    by_class = {}
    for mmt_def, ab in abilities.items():
        if ab.get("status", "mapped") != "mapped":
            continue
        cls = ab.get("class")
        if cls not in classes:
            fail("%s: class %s 未在 classes 中定义" % (mmt_def, cls))
            continue
        by_class.setdefault(cls, {})[mmt_def] = ab

    os.makedirs(OUT_ABILITY_DIR, exist_ok=True)
    for cls_key, cls in classes.items():
        cls_abs = by_class.get(cls_key, {})
        declared = cls.get("abilities", [])
        ordered = [d for d in declared if d in cls_abs]
        for d in cls_abs:
            if d not in ordered:
                warn("%s: 技能 %s 未在 classes.%s.abilities 声明顺序" % (cls_key, d, cls_key))
                ordered.append(d)
        lines = ['<?xml version="1.0" encoding="utf-8"?>', "<Defs>",
                 "  <!-- 生成物：tools/build_mmt_rom_classes.py + tools/data/mmt_rom_class_mapping.json，",
                 "       不要手改（改动会在重跑时被覆盖）。" + cls.get("label", cls_key) + " 职业技能 -->"]
        for mmt_def in ordered:
            lines.append(ability_xml(cls_abs[mmt_def], rom_abilities))
        lines.append("</Defs>")
        out_path = os.path.join(OUT_ABILITY_DIR, "MMT_Abilities_RoM_%s.xml" % cls_key)
        with open(out_path, "w", encoding="utf-8", newline="\n") as f:
            f.write("\n".join(lines) + "\n")

    # 生成 Hediff XML：职业×3 阶 + 技能 hediffs
    hediff_lines = ['<?xml version="1.0" encoding="utf-8"?>', "<Defs>",
                    "  <!-- 生成物：tools/build_mmt_rom_classes.py，不要手改。",
                    "       37 职业×3 阶（abilities 按阶累积）+ 技能 buff/debuff/光环 Hediff。",
                    "       全部 becomeVisible=false 或限时 Disappears，不进健康页常规列表。 -->"]
    for cls_key, cls in classes.items():
        cls_abs = by_class.get(cls_key, {})
        stages = {}
        for mmt_def, ab in cls_abs.items():
            stages.setdefault(ab["stage"], []).append(mmt_def)
        for tier in (1, 2, 3):
            hediff_lines.append(class_hediff_xml(cls_key, cls, tier, stages))
    emitted = set()
    for def_name, spec in hediffs.items():
        if def_name in emitted:
            continue
        emitted.add(def_name)
        hediff_lines.append(hediff_xml(def_name, spec))
    hediff_lines.append("</Defs>")
    with open(OUT_HEDIFFS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(hediff_lines) + "\n")

    # 引用完整性：技能引用的 hediff 必须存在（生成 hediffs 表或框架手写表或原版）
    known_extra = set(mapping.get("externalHediffs", []))
    for mmt_def, ab in abilities.items():
        if ab.get("status", "mapped") != "mapped":
            continue
        p = ab.get("params", {})
        refs = [p.get("hediff"), p.get("aura"), p.get("hitHediffDef"), p.get("targetHediffDef"),
                p.get("moodHediff"), p.get("stunHediff"), p.get("selfBuffHediff"),
                p.get("revivedHediff"), p.get("casterHediff"), p.get("fateHediff")]
        refs += p.get("hediffPool", [])
        aura_spec = hediffs.get(p.get("aura", ""), {})
        if aura_spec.get("aura", {}).get("targetHediff"):
            refs.append(aura_spec["aura"]["targetHediff"])
        for ref in refs:
            if ref and ref not in hediffs and ref not in known_extra:
                fail("%s: 引用的 Hediff %s 未在 hediffs 表/externalHediffs 登记" % (mmt_def, ref))

    # 生成投射物 XML + 复制投射物贴图
    proj_lines = ['<?xml version="1.0" encoding="utf-8"?>', "<Defs>",
                  "  <!-- 生成物：tools/build_mmt_rom_classes.py，不要手改。投射物桶 ThingDef。 -->"]
    seen_proj = set()
    for mmt_def, ab in abilities.items():
        if ab.get("status", "mapped") != "mapped":
            continue
        if ab["comp"] not in ("LaunchProjectile", "MultiShot"):
            continue
        proj = ab["params"]["projectile"]
        if proj in seen_proj:
            fail("%s: 投射物 %s 重复定义" % (mmt_def, proj))
            continue
        seen_proj.add(proj)
        proj_lines.append(projectile_xml(ab))
        tex = ab["params"].get("projectileSpec", {}).get("tex")
        if tex:
            copy_texture("Spells/" + tex, OUT_PROJ_TEX_DIR, mmt_def)
    proj_lines.append("</Defs>")
    with open(OUT_PROJECTILES, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(proj_lines) + "\n")

    # 召唤建筑 XML
    buildings = mapping.get("summonBuildings", {})
    if buildings:
        blines = ['<?xml version="1.0" encoding="utf-8"?>', "<Defs>",
                  "  <!-- 生成物：tools/build_mmt_rom_classes.py，不要手改。召唤建筑（炮塔/图腾/地雷） -->"]
        for bdef, bspec in buildings.items():
            blines.append(building_xml(bdef, bspec))
        blines.append("</Defs>")
        with open(OUT_BUILDINGS, "w", encoding="utf-8", newline="\n") as f:
            f.write("\n".join(blines) + "\n")

    # 事件条件
    conds = conditions_xml(mapping)
    if conds:
        os.makedirs(os.path.dirname(OUT_CONDITIONS), exist_ok=True)
        with open(OUT_CONDITIONS, "w", encoding="utf-8", newline="\n") as f:
            f.write(conds + "\n")

    for w in warnings:
        print("WARN:", w)
    if errors:
        for e in errors:
            print("FAIL:", e)
        print("生成完成但有 %d 项错误" % len(errors))
        sys.exit(1)
    print("生成完成：%d 职业 / %d 技能 / %d 技能Hediff / %d 投射物" % (
        len(classes), sum(len(v) for v in by_class.values()), len(emitted), len(seen_proj)))


if __name__ == "__main__":
    main()
