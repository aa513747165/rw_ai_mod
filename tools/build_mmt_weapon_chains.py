#!/usr/bin/env python3
"""生成 MMT 武器链式文档 docs/mmt-weapon-chains.md。

链式格式：配料(数量) -> 产物（关键战斗属性）。只读 1.7/Defs 生成结果，不依赖 HSK。
"""
from __future__ import annotations

import glob
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFS = ROOT / "1.7/Defs"
OUT = ROOT / "docs/mmt-weapon-chains.md"

# 原版/常用配料与类别中文名（MMT 自带 label 的优先用 label）
VANILLA_LABELS = {
    "Steel": "钢", "Chemfuel": "化合燃料", "ComponentIndustrial": "零部件",
    "Uranium": "铀", "WoodLog": "原木", "Plasteel": "玻璃钢",
    "Gunsmithing": "枪械制造", "BlowbackOperation": "反冲作用", "GasOperation": "导气式",
    "PrecisionRifling": "精密膛线", "MultibarrelWeapons": "多管武器", "HeavyTurrets": "重型炮塔",
    "Mortars": "迫击炮", "SniperTurret": "狙击炮塔", "Machining": "机械加工",
    "LongBlades": "长刃", "Greatbow": "巨弓", "Smithing": "锻造", "ChargedShot": "充能射击",
    "Fabrication": "制造", "RecurveBow": "反曲弓", "GunTurrets": "机枪塔",
    "Crafting": "制作", "Intellectual": "智识", "Artistic": "艺术", "Shooting": "射击",
    "Construction": "建造", "Melee": "近战",
}
CATEGORY_LABELS = {
    "Metallic": "金属", "Leathers": "皮革", "Woody": "木质",
    "MMT_SLDBar": "低质钢坯", "MMT_USLDBar": "优质钢坯", "MMT_HCMBar": "重粗钢坯",
    "MMT_BTextiles": "布料", "Stony": "石质", "Leathery": "皮革",
}


def load_labels() -> dict[str, str]:
    """MMT 全部 Def 的 defName -> 中文 label。"""
    labels: dict[str, str] = {}
    for path in sorted(DEFS.glob("**/*.xml")):
        try:
            root = ET.parse(path).getroot()
        except ET.ParseError:
            continue
        for node in root.iter():
            name = node.findtext("defName")
            label = node.findtext("label")
            if name and label:
                labels[name] = label
    return labels


def parse(path: Path) -> ET.Element:
    return ET.parse(path).getroot()


def label_of(name: str, labels: dict[str, str]) -> str:
    return labels.get(name) or VANILLA_LABELS.get(name) or name


def fmt_ingredients(recipe: ET.Element, labels: dict[str, str]) -> str:
    """配料列表 -> '钢(30) + [金属](10)' 形式。"""
    parts: list[str] = []
    for li in recipe.findall("./ingredients/li"):
        count = li.findtext("count") or "?"
        names: list[str] = []
        for td in li.findall("./filter/thingDefs/li"):
            if td.text:
                names.append(label_of(td.text.strip(), labels))
        for cat in li.findall("./filter/categories/li"):
            if cat.text:
                names.append(f"[{CATEGORY_LABELS.get(cat.text.strip(), cat.text.strip())}]")
        parts.append(f"{'/'.join(names) or '?'}({count})")
    return " + ".join(parts)


def fmt_cost(cost_node: ET.Element | None, labels: dict[str, str]) -> str:
    if cost_node is None:
        return ""
    return " + ".join(f"{label_of(item.tag, labels)}({item.text})" for item in cost_node)


def projectile_stats(projectiles: dict[str, ET.Element], name: str | None) -> list[str]:
    """投射物 -> ['伤害20', '穿透0.07', '弹速156']"""
    if not name or name not in projectiles:
        return []
    proj = projectiles[name].find("projectile")
    if proj is None:
        return []
    stats: list[str] = []
    damage = proj.findtext("damageAmountBase")
    if damage:
        stats.append(f"伤害{damage}")
    ap = proj.findtext("armorPenetrationBase")
    if ap and ap != "0":
        stats.append(f"穿透{ap}")
    speed = proj.findtext("speed")
    if speed:
        stats.append(f"弹速{speed}")
    radius = proj.findtext("explosionRadius")
    if radius:
        stats.append(f"爆炸半径{radius}")
    return stats


def ranged_stats(weapon: ET.Element, projectiles: dict[str, ET.Element]) -> str:
    verb = weapon.find("./verbs/li")
    stats: list[str] = []
    if verb is not None:
        stats += projectile_stats(projectiles, verb.findtext("defaultProjectile"))
        rng = verb.findtext("range")
        if rng:
            stats.append(f"射程{rng}")
        burst = verb.findtext("burstShotCount")
        ticks = verb.findtext("ticksBetweenBurstShots")
        if burst and burst != "1":
            stats.append(f"连发{burst}×{ticks or '?'}tick")
        warmup = verb.findtext("warmupTime")
        if warmup:
            stats.append(f"预热{warmup}s")
    cooldown = weapon.findtext("statBases/RangedWeapon_Cooldown")
    if cooldown:
        stats.append(f"冷却{cooldown}s")
    return " ".join(stats)


def melee_stats(weapon: ET.Element) -> str:
    parts: list[str] = []
    for tool in weapon.findall("./tools/li"):
        caps = "/".join(li.text or "" for li in tool.findall("./capacities/li"))
        entry = f"{tool.findtext('label') or '?'}[{caps} 威力{tool.findtext('power')}"
        ap = tool.findtext("armorPenetration")
        if ap:
            entry += f" 穿透{ap}"
        cd = tool.findtext("cooldownTime")
        if cd:
            entry += f" 冷却{cd}s"
        parts.append(entry + "]")
    return " ".join(parts)


def weapon_stats(weapon: ET.Element, projectiles: dict[str, ET.Element]) -> str:
    if weapon.find("./tools/li") is not None and weapon.find("./verbs/li") is None:
        return melee_stats(weapon)
    return ranged_stats(weapon, projectiles)


def recipe_suffix(recipe: ET.Element, labels: dict[str, str]) -> str:
    suffix: list[str] = []
    users = [li.text for li in recipe.findall("./recipeUsers/li") if li.text]
    if users:
        suffix.append("@" + "/".join(label_of(u, labels) for u in users))
    research = recipe.findtext("researchPrerequisite")
    if research:
        suffix.append(f"研究:{label_of(research, labels)}")
    skill = recipe.find("./skillRequirements")
    if skill is not None and list(skill):
        reqs = ",".join(f"{label_of(s.tag, labels)}{s.text}" for s in skill)
        suffix.append(f"技能:{reqs}")
    return " ｜ ".join(f"（{s}）" for s in suffix)


def maker_suffix(weapon: ET.Element, labels: dict[str, str]) -> str:
    """recipeMaker 武器的工作台/研究/技能后缀。"""
    maker = weapon.find("recipeMaker")
    if maker is None:
        return ""
    suffix: list[str] = []
    users = [li.text for li in maker.findall("./recipeUsers/li") if li.text]
    if users:
        suffix.append("@" + "/".join(label_of(u, labels) for u in users))
    research = maker.findtext("researchPrerequisite")
    if research:
        suffix.append(f"研究:{label_of(research, labels)}")
    skill = maker.find("./skillRequirements")
    if skill is not None and list(skill):
        reqs = ",".join(f"{label_of(s.tag, labels)}{s.text}" for s in skill)
        suffix.append(f"技能:{reqs}")
    return " ｜ ".join(f"（{s}）" for s in suffix)


def maker_ingredients(weapon: ET.Element, labels: dict[str, str]) -> str:
    """recipeMaker 武器的材料成本 -> '[金属](75) + 中世纪零件(7)'。"""
    parts: list[str] = []
    stuff_count = weapon.findtext("costStuffCount")
    stuffs = [li.text for li in weapon.findall("./stuffCategories/li") if li.text]
    if stuff_count and stuffs:
        cats = "/".join(f"[{CATEGORY_LABELS.get(s, s)}]" for s in stuffs)
        parts.append(f"{cats}({stuff_count})")
    cost = fmt_cost(weapon.find("costList"), labels)
    if cost:
        parts.append(cost)
    return " + ".join(parts)


def main() -> int:
    labels = load_labels()
    projectiles = {
        node.findtext("defName"): node
        for node in parse(DEFS / "ThingDefs_Weapons/MMT_Projectiles.xml").findall("ThingDef")
    }
    weapons: dict[str, ET.Element] = {}
    for path in sorted(DEFS.glob("ThingDefs_Weapons/MMT_Weapons_*.xml")):
        for node in parse(path).findall("ThingDef"):
            if node.findtext("defName"):
                weapons[node.findtext("defName")] = node
    buildings = {
        node.findtext("defName"): node
        for node in parse(DEFS / "ThingDefs_Buildings/MMT_Buildings_Turrets.xml").findall("ThingDef")
        if node.findtext("defName")
    }
    ammo_items = {
        node.findtext("defName"): node
        for node in parse(DEFS / "ThingDefs_Items/MMT_Items_TurretAmmo.xml").findall("ThingDef")
    }
    crates = {
        node.findtext("defName"): node
        for node in parse(DEFS / "ThingDefs_Items/MMT_Items_TurretCrates.xml").findall("ThingDef")
    }
    recipes: dict[str, ET.Element] = {}
    for path in sorted(DEFS.glob("RecipeDefs/MMT_Recipes_*.xml")):
        for node in parse(path).findall("RecipeDef"):
            if node.findtext("defName"):
                recipes[node.findtext("defName")] = node

    def recipes_producing(pred) -> list[tuple[ET.Element, str]]:
        result = []
        for recipe in recipes.values():
            for product in recipe.findall("./products/*"):
                if pred(product.tag):
                    result.append((recipe, product.tag))
        return sorted(result, key=lambda pair: label_of(pair[1], labels))

    lines: list[str] = []
    add = lines.append
    add("# MMT 武器链式文档")
    add("")
    add("由 `tools/build_mmt_weapon_chains.py` 生成；格式：配料(数量) -> 产物（战斗属性）。（改 Def 后需重跑）")
    add("")

    # ===== 1-3. 手持武器（显式配方 + recipeMaker 自动配方）=====
    sections = [
        ("常规枪械", "ThingDefs_Weapons/MMT_Weapons_Conventional.xml", "RecipeDefs/MMT_Recipes_Weapons_Conventional.xml"),
        ("冷兵器弓弩与手雷", "ThingDefs_Weapons/MMT_Weapons_Archaic.xml", "RecipeDefs/MMT_Recipes_Weapons_Archaic.xml"),
        ("未来与远古武器", "ThingDefs_Weapons/MMT_Weapons_Futuristic.xml", "RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml"),
    ]
    for title, weapon_rel, recipe_rel in sections:
        add(f"## {title}")
        add("")
        batch = {n.findtext("defName") for n in parse(DEFS / weapon_rel).findall("ThingDef")}
        batch_recipes = {r.findtext("defName") for r in parse(DEFS / recipe_rel).findall("RecipeDef")}
        made: set[str] = set()
        for recipe, product in recipes_producing(lambda tag: tag in batch):
            if recipe.findtext("defName") not in batch_recipes:
                continue
            made.add(product)
            weapon = weapons[product]
            add(f"- {fmt_ingredients(recipe, labels)} -> {label_of(product, labels)}（{weapon_stats(weapon, projectiles)}）{recipe_suffix(recipe, labels)}")
        for name in sorted(batch - made, key=lambda n: label_of(n, labels)):
            weapon = weapons[name]
            if weapon.find("recipeMaker") is not None:
                add(f"- {maker_ingredients(weapon, labels)} -> {label_of(name, labels)}（{weapon_stats(weapon, projectiles)}）{maker_suffix(weapon, labels)}")
        add("")

    # ===== 4. 无制造配方武器 =====
    add("## 无制造配方武器（NPC/特殊/缴获）")
    add("")
    craftable = set()
    for _, product in recipes_producing(lambda tag: tag in weapons):
        craftable.add(product)
    for name, weapon in weapons.items():
        if weapon.find("recipeMaker") is not None:
            craftable.add(name)
    for path in sorted(DEFS.glob("ThingDefs_Weapons/MMT_Weapons_NPC.xml")):
        for node in parse(path).findall("ThingDef"):
            name = node.findtext("defName")
            if name and name not in craftable:
                add(f"- {label_of(name, labels)}（{weapon_stats(node, projectiles)}）")
    add("")

    # ===== 5. 炮塔链 =====
    add("## 炮塔")
    add("")
    add("### 炮塔箱配方")
    add("")
    for recipe, product in recipes_producing(lambda tag: tag in crates):
        add(f"- {fmt_ingredients(recipe, labels)} -> {label_of(product, labels)}{recipe_suffix(recipe, labels)}")
    add("")
    add("### 炮塔建造（箱 + 材料 -> 炮塔）")
    add("")
    for name, building in sorted(buildings.items(), key=lambda pair: label_of(pair[0], labels)):
        gun_name = building.findtext("building/turretGunDef")
        gun = weapons.get(gun_name)
        stats = weapon_stats(gun, projectiles) if gun is not None else ""
        refuel = building.find("./comps/li[@Class='CompProperties_Refuelable']")
        if refuel is not None:
            fuel = refuel.findtext("fuelFilter/thingDefs/li")
            stats += f" ｜ 弹药:{label_of(fuel, labels)}×{refuel.findtext('fuelCapacity')}"
        stuff_count = building.findtext("costStuffCount")
        stuffs = "/".join(f"[{CATEGORY_LABELS.get(s.text, s.text)}]" for s in building.findall("./stuffCategories/li") if s.text)
        inputs = []
        if stuff_count and stuffs:
            inputs.append(f"{stuffs}({stuff_count})")
        cost = fmt_cost(building.find("costList"), labels)
        if cost:
            inputs.append(cost)
        research = building.findtext("./researchPrerequisites/li")
        suffix = f"（研究:{label_of(research, labels)}）" if research else ""
        add(f"- {' + '.join(inputs)} -> {label_of(name, labels)}（{stats.strip()}）{suffix}")
    add("")

    # ===== 6. 炮塔弹药 =====
    add("## 炮塔弹药")
    add("")
    for recipe, product in recipes_producing(lambda tag: tag in ammo_items):
        ammo = ammo_items[product]
        stats = projectile_stats(projectiles, ammo.findtext("projectileWhenLoaded"))
        add(f"- {fmt_ingredients(recipe, labels)} -> {label_of(product, labels)}（{' '.join(stats)}）{recipe_suffix(recipe, labels)}")
    add("")

    OUT.write_text("\n".join(lines), encoding="utf-8", newline="\n")
    print(f"wrote {OUT.relative_to(ROOT)} ({len(lines)} lines)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
