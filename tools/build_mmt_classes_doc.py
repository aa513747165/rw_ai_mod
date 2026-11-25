#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成 docs/mmt-classes.md：全职业技能技术详情手册。

数据源：
- tools/data/mmt_rom_class_mapping.json（职业/技能/状态唯一人工维护点）
- tools/data/mmt_talent_tree_mapping.json（家族中文名）
- 1.7/Defs/AbilityDefs/MMT_Abilities_RoM_*.xml（生成产物，取最终冷却/施法/射程/消耗）

用法：python tools/build_mmt_classes_doc.py
"""

import json
import os
import re
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CLASS_MAP = os.path.join(ROOT, "tools", "data", "mmt_rom_class_mapping.json")
TREE_MAP = os.path.join(ROOT, "tools", "data", "mmt_talent_tree_mapping.json")
ABILITY_DIR = os.path.join(ROOT, "1.7", "Defs", "AbilityDefs")
OUT = os.path.join(ROOT, "docs", "mmt-classes.md")

TIERS = {
    "small": {"entropy": 8, "psyfocus": 0.01, "label": "小"},
    "mid": {"entropy": 16, "psyfocus": 0.02, "label": "中"},
    "big": {"entropy": 35, "psyfocus": 0.04, "label": "大"},
}

TARGET_LABELS = {
    "self": "自身",
    "friendly": "友方",
    "enemy": "敌方",
    "location": "地点",
    "corpse": "尸体",
}

BRANCH_LABELS = {"magic": "法师组", "might": "斗士组"}

COMP_LABELS = {
    "GiveHediff": "施加状态",
    "GiveHediffAoE": "范围施加状态",
    "LaunchProjectile": "投射物",
    "AoEDamage": "范围伤害",
    "Heal": "治疗",
    "Summon": "召唤",
    "DashStrike": "突进打击",
    "MentalState": "精神影响",
    "MentalStateAoE": "范围精神影响",
    "SpawnEffect": "环境效果",
    "MultiShot": "多重射击",
    "ToggleAura": "光环开关",
    "Teleport": "传送",
    "Stun": "眩晕",
    "Smokepop": "烟雾弹",
    "Wallraise": "升起石墙",
    "ResurrectLite": "复活",
    "SelfEntropy": "自伤降熵",
    "FadeEmotions": "抚平心绪",
    "Pull": "拉取",
    "GroupTeleport": "群体传送",
    "Transpose": "移形换位",
    "CombatDrugs": "战斗药剂",
    "CorpseExplosion": "尸爆",
    "AlterFate": "改写命运",
    "PistolWhip": "枪托猛击",
}


def num(v):
    return "%g" % round(v, 2)


def ticks_to_sec(ticks):
    return num(ticks / 60.0)


def parse_generated_stats():
    """从生成产物 XML 读每个技能的最终数值（冷却/施法/射程/熵/灵能）。"""
    stats = {}
    for fn in os.listdir(ABILITY_DIR):
        if not (fn.startswith("MMT_Abilities_RoM_") and fn.endswith(".xml")):
            continue
        tree = ET.parse(os.path.join(ABILITY_DIR, fn))
        for el in tree.getroot():
            if el.tag != "AbilityDef":
                continue
            dn = el.findtext("defName")
            if not dn:
                continue
            entry = {}
            sb = el.find("statBases")
            if sb is not None:
                for tag, key in (("Ability_EntropyGain", "entropy"),
                                 ("Ability_PsyfocusCost", "psyfocus")):
                    v = sb.findtext(tag)
                    if v:
                        entry[key] = float(v)
            cd = el.findtext("cooldownTicksRange")
            if cd:
                m = re.match(r"\(?\s*(\d+)", cd)
                entry["cooldownTicks"] = int(m.group(1)) if m else None
            vp = el.find("verbProperties")
            if vp is not None:
                w = vp.findtext("warmupTime")
                r = vp.findtext("range")
                if w:
                    entry["warmup"] = float(w)
                if r:
                    entry["range"] = float(r)
            stats[dn] = entry
    return stats


def hediff_label(def_name, hediffs, external):
    if def_name in hediffs:
        return "%s（%s）" % (hediffs[def_name]["label"], def_name)
    if def_name in external:
        return "原版 %s" % def_name
    return def_name


def fmt_duration(ticks):
    if not ticks:
        return ""
    sec = ticks / 60.0
    if sec >= 3600:
        return "%s 小时" % num(sec / 3600.0)
    if sec >= 60:
        return "%s 分" % num(sec / 60.0)
    return "%s 秒" % num(sec)


def fmt_params(ab, hediffs, external, buildings):
    """按 comp 桶汇总关键参数（默认值与 build_mmt_rom_classes.py 的 comp_xml 对齐）。"""
    comp = ab["comp"]
    p = ab.get("params", {})
    parts = []
    if comp in ("LaunchProjectile", "MultiShot"):
        spec = p.get("projectileSpec", {})
        dmg = []
        if "damage" in spec:
            dmg.append("伤害 %s（%s）" % (spec["damage"], spec.get("damageDef", "Bullet")))
        if "speed" in spec:
            dmg.append("弹速 %s" % spec["speed"])
        if spec.get("explosionRadius"):
            dmg.append("爆炸半径 %s" % spec["explosionRadius"])
        if comp == "MultiShot":
            dmg.insert(0, "%s 发、散布 %s" % (p.get("shotCount", 5), p.get("scatterRadius", 1.5)))
        parts.append("，".join(dmg))
    elif comp == "AoEDamage":
        s = "%s 伤害（%s），半径 %s" % (p.get("damage"), p.get("damageDef"), p.get("radius", 3))
        if p.get("pulses", 1) > 1:
            s += "，%s 次脉冲/间隔 %s 秒" % (p["pulses"], ticks_to_sec(p.get("pulseIntervalTicks", 60)))
        if p.get("armorPen"):
            s += "，穿甲 %s" % p["armorPen"]
        if p.get("startFireChance"):
            s += "，%s%% 点燃" % int(p["startFireChance"] * 100)
        if p.get("hitHediffDef"):
            s += "，命中附加 %s" % hediff_label(p["hitHediffDef"], hediffs, external)
        parts.append(s)
    elif comp in ("GiveHediff", "GiveHediffAoE"):
        s = hediff_label(p["hediff"], hediffs, external)
        hd = hediffs.get(p["hediff"], {})
        if hd.get("durationTicks"):
            s += "，持续 " + fmt_duration(hd["durationTicks"])
        if comp == "GiveHediffAoE":
            s += "，半径 %s" % p.get("radius", 6)
            if not p.get("targetFriendly", True):
                s += "（对敌）"
        parts.append(s)
    elif comp == "Heal":
        s = "治疗 %s，最多 %s 处伤" % (p.get("heal", 10), p.get("maxInjuries", 3))
        if p.get("radius"):
            s += "，半径 %s" % p["radius"]
        for key, label in (("tendBleeding", "止血"), ("cureInfection", "治感染"), ("cureScar", "祛疤")):
            if p.get(key):
                s += "，" + label
        parts.append(s)
    elif comp == "Summon":
        if p.get("building"):
            b = buildings.get(p["building"], {})
            parts.append("召唤建筑 %s（%s）" % (b.get("label", p["building"]), p["building"]))
        else:
            s = "%s ×%s" % (p["pawnKind"], p.get("count", 1))
            s += "，持续 " + fmt_duration(p.get("durationTicks", 18000))
            if p.get("maxActive", 6) != 6:
                s += "，场上限 %s" % p["maxActive"]
            if p.get("fromCorpse"):
                s += "，从尸体唤起"
            parts.append(s)
    elif comp == "DashStrike":
        s = "%s 伤害（%s）" % (p.get("damage", 20), p.get("damageDef", "Cut"))
        if p.get("radius"):
            s += "，路径半径 %s" % p["radius"]
        if p.get("returnToStart"):
            s += "，返回原位"
        parts.append(s)
    elif comp in ("MentalState", "MentalStateAoE"):
        s = "精神状态 %s" % p["mentalState"]
        if comp == "MentalStateAoE":
            s += "，半径 %s" % p.get("radius", 6)
            if p.get("animalsOnly"):
                s += "（仅动物）"
        parts.append(s)
    elif comp == "SpawnEffect":
        if p.get("gameConditionDef"):
            s = "游戏条件 %s" % p["gameConditionDef"]
            if p.get("conditionDurationTicks"):
                s += "，持续 " + fmt_duration(p["conditionDurationTicks"])
            parts.append(s)
        if p.get("weatherDef"):
            parts.append("天气 %s" % p["weatherDef"])
        if p.get("spawnThingDef"):
            parts.append("生成 %s ×%s" % (p["spawnThingDef"], p.get("spawnCount", 1)))
        if p.get("extinguishRadius"):
            parts.append("扑灭半径 %s 的火焰" % p["extinguishRadius"])
    elif comp == "ToggleAura":
        aura = p["aura"]
        hd = hediffs.get(aura, {})
        s = "光环 %s（%s）" % (hd.get("label", aura), aura)
        a = hd.get("aura", {})
        if a:
            s += "，半径 %s，每脉冲熵 %s" % (a.get("radius", 8), a.get("entropyPerPulse", 0.3))
            if a.get("targetHediff"):
                s += "，作用于 %s" % hediff_label(a["targetHediff"], hediffs, external)
        parts.append(s)
    elif comp == "Wallraise":
        parts.append("墙体 %s" % p.get("wallDef", "RaisedRocks"))
    elif comp == "ResurrectLite":
        if p.get("revivedHediff"):
            parts.append("复活并附加 %s" % hediff_label(p["revivedHediff"], hediffs, external))
        if p.get("casterHediff"):
            parts.append("施法者承受 %s" % hediff_label(p["casterHediff"], hediffs, external))
    elif comp == "SelfEntropy":
        s = "熵变化 %s" % p.get("entropyChange", -10)
        if p.get("selfDamage"):
            s += "，自伤 %s" % p["selfDamage"]
        if p.get("healAmount"):
            s += "，自疗 %s" % p["healAmount"]
        parts.append(s)
    elif comp == "FadeEmotions":
        if p.get("moodHediff"):
            parts.append("附加心情 %s" % hediff_label(p["moodHediff"], hediffs, external))
    elif comp == "GroupTeleport":
        parts.append("携带半径 %s 内友军" % p.get("radius", 4))
    elif comp == "Transpose":
        if p.get("stunHediff"):
            parts.append("目标眩晕（%s）" % hediff_label(p["stunHediff"], hediffs, external))
        if p.get("selfBuffHediff"):
            parts.append("自身增益（%s）" % hediff_label(p["selfBuffHediff"], hediffs, external))
    elif comp == "CombatDrugs":
        names = [hediff_label(h, hediffs, external) for h in p.get("hediffPool", [])]
        parts.append("随机药剂：" + "、".join(names))
    elif comp == "CorpseExplosion":
        parts.append("%s 伤害（%s），半径 %s，延迟 %s 秒" % (
            p.get("damage", 30), p.get("damageDef", "Bomb"),
            p.get("radius", 2.5), ticks_to_sec(p.get("delayTicks", 90))))
    elif comp == "AlterFate":
        parts.append("命运状态 %s" % hediff_label(p["fateHediff"], hediffs, external))
    elif comp == "PistolWhip":
        s = "%s 伤害" % p.get("damage", 12)
        if p.get("disarm", True):
            s += "，缴械"
        if p.get("stunHediff"):
            s += "，眩晕（%s）" % hediff_label(p["stunHediff"], hediffs, external)
        parts.append(s)
    for ex in ab.get("extraComps", []):
        ex_label = COMP_LABELS.get(ex["comp"], ex["comp"])
        ex_params = fmt_params(dict(ex, params=ex.get("params", {})), hediffs, external, buildings)
        parts.append("附加「%s」：%s" % (ex_label, ex_params) if ex_params
                     else "附加「%s」" % ex_label)
    return "；".join(x for x in parts if x)


def fmt_stat_value(group, value):
    if group == "statFactors":
        return "×%s" % value
    return "%+g" % value


def main():
    class_map = json.load(open(CLASS_MAP, encoding="utf-8"))
    tree_map = json.load(open(TREE_MAP, encoding="utf-8"))
    classes = class_map["classes"]
    abilities = class_map["abilities"]
    hediffs = class_map.get("hediffs", {})
    external = set(class_map.get("externalHediffs", []))
    buildings = class_map.get("summonBuildings", {})
    conditions = class_map.get("gameConditions", {})
    gen_stats = parse_generated_stats()

    fam_labels = {k: v.get("label", k) for k, v in tree_map.get("families", {}).items()}

    # 家族 → 职业 排序（按映射表出现顺序）
    fam_order = []
    fam_classes = {}
    for key, cls in classes.items():
        fam = cls.get("family", "?")
        if fam not in fam_classes:
            fam_classes[fam] = []
            fam_order.append(fam)
        fam_classes[fam].append(key)

    out = []
    w = out.append
    w("# MMT 职业系统技术详情手册")
    w("")
    w("> 本文档由 `tools/build_mmt_classes_doc.py` 自动生成，数据源为 "
      "`tools/data/mmt_rom_class_mapping.json` 与生成产物 `1.7/Defs/AbilityDefs/MMT_Abilities_RoM_*.xml`。"
      "请勿手工编辑；要改内容请改映射表后重跑脚本。")
    w("")
    w("## 系统总览")
    w("")
    w("- 共 **%d 个职业**，分属 **%d 个家族**，按导师台菜单分为法师组（magic）与斗士组（fighter）。"
      % (len(classes), len(fam_classes)))
    w("- 原型为 A RimWorld of Magic（RoM）职业技能，已获授权复刻；技能音效/特效为 RoM 更名复制。")
    w("- 技能基类为 `PsycastBase`，消耗灵能（psyfocus）并积累熵（entropy），分三档：")
    w("")
    w("| 档位 | 熵增加 | 灵能消耗 |")
    w("| --- | --- | --- |")
    for tkey in ("small", "mid", "big"):
        t = TIERS[tkey]
        w("| %s（%s） | %s | %s |" % (tkey, t["label"], t["entropy"], t["psyfocus"]))
    w("")
    w("- 默认施法时间 0.5 秒；非自身目标默认射程 24.9 格；冷却标注「—」表示无冷却。")
    w("- 解锁阶位（stage）1/2/3 对应职业 I/II/III 阶：就职需历练 5 级 + 职业凭证，"
      "II 阶需历练 20 级 + 能量结晶，III 阶需历练 35 级 + 能量结晶（导师台操作）。")
    w("- v2 起技能由职业家族天赋树的技能节点授予（requireTier = 技能阶位），"
      "阶位 Hediff 仅保留被动属性。")
    w("")
    w("## 家族与职业一览")
    w("")
    w("| 家族 | 职业 |")
    w("| --- | --- |")
    for fam in fam_order:
        names = "、".join("%s（%s，%s）" % (classes[c]["label"], c,
                          BRANCH_LABELS.get(classes[c]["branch"], classes[c]["branch"]))
                          for c in fam_classes[fam])
        w("| %s（%s） | %s |" % (fam_labels.get(fam, fam), fam, names))
    w("")

    for fam in fam_order:
        w("## 家族：%s（%s）" % (fam_labels.get(fam, fam), fam))
        w("")
        for ckey in fam_classes[fam]:
            cls = classes[ckey]
            w("### %s（%s）" % (cls["label"], ckey))
            w("")
            w("- 分支：%s　家族：%s" % (BRANCH_LABELS.get(cls["branch"], cls["branch"]),
                                        fam_labels.get(fam, fam)))
            w("- 说明：%s" % cls.get("description", ""))
            # 阶位被动
            passives = cls.get("passives", {})
            if passives:
                w("- 阶位被动：")
                for tier in ("1", "2", "3"):
                    p = passives.get(tier)
                    if not p:
                        continue
                    roman = {"1": "I", "2": "II", "3": "III"}[tier]
                    items = []
                    for group in ("statOffsets", "statFactors"):
                        for stat, val in p.get(group, {}).items():
                            items.append("%s %s" % (stat, fmt_stat_value(group, val)))
                    w("  - %s 阶：%s" % (roman, "，".join(items) if items else "无"))
            w("")
            w("| 技能 | 阶位 | 档位 | 熵 | 灵能 | 冷却 | 施法 | 射程 | 目标 | 效果 | 关键参数 |")
            w("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
            for ab_def in cls["abilities"]:
                ab = abilities.get(ab_def)
                if not ab:
                    continue
                gs = gen_stats.get(ab_def, {})
                tier = TIERS[ab["tier"]]
                entropy = gs.get("entropy", tier["entropy"])
                psyfocus = gs.get("psyfocus", tier["psyfocus"])
                cd = gs.get("cooldownTicks")
                cd_s = (ticks_to_sec(cd) + " 秒") if cd else "—"
                warmup = "%g 秒" % gs.get("warmup", 0.5)
                rng = "—" if ab["target"] == "self" else "%g" % gs.get("range", 24.9)
                comp_label = COMP_LABELS.get(ab["comp"], ab["comp"])
                params = fmt_params(ab, hediffs, external, buildings) or "—"
                desc = ab.get("description", "")
                w("| %s<br>`%s` | %s | %s | %g | %g | %s | %s | %s | %s | %s | %s |" % (
                    ab["label"], ab_def, ab["stage"], tier["label"],
                    entropy, psyfocus, cd_s, warmup, rng,
                    TARGET_LABELS.get(ab["target"], ab["target"]),
                    comp_label, params))
                if desc:
                    w("| ↳ %s |  |  |  |  |  |  |  |  |  |  |" % desc.replace("|", "\\|"))
            w("")

    if buildings:
        w("## 附录 A：召唤建筑")
        w("")
        w("| defName | 名称 | 模式 | 伤害 | 半径 | 间隔 | 存在时间 | 说明 |")
        w("| --- | --- | --- | --- | --- | --- | --- | --- |")
        mode_labels = {"attack": "攻击", "heal": "治疗", "mine": "地雷"}
        for dn, b in buildings.items():
            w("| `%s` | %s | %s | %s | %s | %s 秒 | %s | %s |" % (
                dn, b["label"], mode_labels.get(b.get("mode"), b.get("mode")),
                ("%s（%s）" % (b.get("damageAmount"), b.get("damageDef"))) if b.get("damageAmount") else "—",
                b.get("radius", "—"),
                ticks_to_sec(b.get("intervalTicks", 0)) if b.get("intervalTicks") else "—",
                fmt_duration(b.get("lifespanTicks")) if b.get("lifespanTicks") else "—",
                b.get("description", "")))
        w("")

    if conditions:
        w("## 附录 B：气象/游戏条件")
        w("")
        w("| defName | 名称 | 效果 | 说明 |")
        w("| --- | --- | --- | --- |")
        for dn, c in conditions.items():
            if "temperatureOffset" in c:
                effect = "气温 %+g℃" % c["temperatureOffset"]
            elif c.get("conditionClass"):
                effect = "作物加速生长（%s）" % c["conditionClass"]
            else:
                effect = "—"
            w("| `%s` | %s | %s | %s |" % (dn, c["label"], effect, c.get("description", "")))
        w("")

    with open(OUT, "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("已生成 %s（%d 行）" % (OUT, len(out)))


if __name__ == "__main__":
    main()
