# Core_SK 全武器移植 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 HSK `Core_SK` 的 162 个非抽象武器/投射物 ThingDef、87 条武器配方、16 个可建造炮塔、26 条炮塔箱链与压缩研究树移植为独立的 MMT 1.7 武器体系。

**Architecture:** 使用 `tools/build_mmt_weapons_from_hsk.py` 从本地 HSK/CE XML 确定性生成 MMT XML，人工审查结果只回写到 `tools/data/mmt_weapon_mapping.json`。手持武器使用原版 verb 和 MMT 投射物；装填类炮塔用原版 refuelable 组件按发消耗 MMT 子弹。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 标准库、`tools/validate_mod.py`、.NET 10 构建。

**Spec:** `docs/superpowers/specs/2026-10-21-core-sk-weapons-design.md`

## Global Constraints

- 只读 `Hardcore-SK/`，禁止修改 HSK 仓库或访问远程。
- MMT 新 defName 一律使用 `MMT_` 前缀，不覆盖原版武器。
- 不引入 `CombatExtended.*`、`SK.*`、`RightToolForJob.*` 类型或程序集依赖。
- 手持武器不消耗弹药；只有 HSK 原本需要装填的炮塔/自动武器消耗 MMT 子弹。
- HSK 贴图与音频已获授权；缺失音频只允许使用映射表中的原版 fallback。
- 中文 label/description 优先来自 `Hardcore-SK/1.6/Languages/ChineseSimplified (简体中文)/DefInjected`。
- 每批先加失败验证，再实现，再跑对应套件；提交前至少跑对应套件，最终跑 `validate_mod.py all` 和 `dotnet build`。
- 新增/修改的 `*.java`、`*.xml` 可自动 `git add`；Python/JSON/Markdown 在提交前必须先取得用户确认。
- 只提交本地仓库，绝不 push。

## Review Focus

1. **CE 投射物继承未解析**：例如 `Bullet_762x51mmNATO_FMJ` 应从父类取得 `speed=156`，自身取得 `damageAmountBase=20`；Task 2 用精确断言固定。
2. **炮塔按发耗弹不成立**：`Gun_M2Browning` 对应炮塔应装填 150 发 `MMT_Ammo_50BMG_FMJ` 且每发扣 1；Task 7 先做最小样本，失败即停止扩量。
3. **缺失音频静默丢失**：`Shot_Tavor` 的 `Tavor` 音频缺失，应映射原版 `Shot_AssaultRifle`；`FlameThrower` 的 `Misc/Fire/BurningLoop` 缺失，应映射 Biotech `Shot_MiniFlameblaster`；Task 2 断言。
4. **配方 filter 与 fixedIngredientFilter 不一致**：例如 `BuildHandGun` 的 `Metallic` 输入映射后必须同时修正 fixed filter；Task 4 断言。
5. **重复实现 B8 武器部件配方**：`MakeWeapon_Parts` 等 14 条已有 MMT 版本，生成器必须排除；Task 4 断言无重复。
6. **生成器覆盖人工修正**：所有人工调整必须进入映射 JSON；Task 1 的 `--check` 模式和 Task 9 的重复生成一致性检查固定。
7. **22 个缺失配方产物断链**：17 个来自 HSK patch 替换值、5 个来自 CE Def，必须全部生成 MMT 克隆；Task 1 固定清单，Task 4–6 分别验证。

---

### Task 1: W0 映射表与生成器骨架

**Files:**
- Create: `tools/build_mmt_weapons_from_hsk.py`
- Create: `tools/data/mmt_weapon_mapping.json`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 本地 HSK `D:/project/rw/Hardcore-SK`、CE XML、HSK 1.6 中文翻译。
- Produces:
  - `load_source_index(mapping: dict) -> SourceIndex`
  - `load_mapping(path: Path) -> dict`
  - `mmt_name(source_name: str) -> str`
  - `load_recipe_product_clone(def_name: str) -> ET.Element`
  - `main(argv: list[str] | None = None) -> int`
  - CLI：`--batch core|research|conventional|archaic|futuristic|turrets|npc|all`，`--check` 或 `--write`

- [ ] **Step 1: 写失败验证**

在 `tools/validate_mod.py` 顶部追加：

```python
WEAPON_MAPPING_JSON = "tools/data/mmt_weapon_mapping.json"
WEAPON_GENERATOR_PY = "tools/build_mmt_weapons_from_hsk.py"
WEAPON_BATCH_FILES = {
    "conventional": [
        "Weapons_Pistols.xml", "Weapons_SMGs.xml", "Weapons_Carabines.xml",
        "Weapons_Rifles.xml", "Weapons_Shotguns.xml", "Weapons_SniperRifles.xml",
        "Weapons_SniperRiflesAdvanced.xml", "Weapons_Heavy.xml",
        "Weapons_Launchers.xml", "Weapons_Flamethrower.xml",
    ],
    "archaic": [
        "Weapons_Bows.xml", "Weapons_Crossbows.xml",
        "Weapons_Melee_Neolithic.xml", "Weapons_Melee_Medieval.xml",
        "Weapons_Melee_Midworld.xml", "Weapons_Melee_Hightech.xml",
        "Weapons_Grenades.xml",
    ],
    "futuristic": [
        "Weapons_Archotech.xml", "Weapons_Blasters.xml", "Weapons_Charged.xml",
        "Weapons_Lasers.xml", "Weapons_Plasma.xml",
    ],
    "turrets": [
        "Weapons_Turrets_Heavy.xml", "Weapons_Turrets_Launchers.xml",
        "Weapons_Turrets_Machineguns.xml", "Weapons_Turrets_Sentry.xml",
    ],
    "npc": [
        "Weapons_Mechanoid.xml", "Weapons_Ogre.xml",
        "Weapons_RangedSpecial.xml", "Weapons_Special.xml",
    ],
}
```

新增：

```python
def validate_weapons_core() -> None:
    mapping = json.loads(read_text(WEAPON_MAPPING_JSON))
    assert_equal(mapping["thing_prefix"], "MMT_", "武器映射前缀")
    assert_equal(mapping["batches"]["conventional"], WEAPON_BATCH_FILES["conventional"], "常规枪械批次")
    assert_equal(mapping["sound_fallbacks"]["Shot_Tavor"], "Shot_AssaultRifle", "Tavor 缺失音效 fallback")
    assert_equal(mapping["sound_fallbacks"]["FlameThrower"], "Shot_MiniFlameblaster", "火焰音效 fallback")
    assert_equal(mapping["workbenches"]["HeavyArmsBench"], "MMT_AdvWeaponCraftingTable", "重型武器台映射")
    assert_equal(mapping["workbenches"]["TableHeavyAmmunition"], "MMT_AmmoBench", "重弹药台映射")
    assert_equal(len(mapping["recipe_product_clones"]["patch_replace"]), 17, "patch 克隆产物数")
    assert_equal(len(mapping["recipe_product_clones"]["combat_extended_defs"]), 5, "CE 克隆产物数")
    assert_equal(len(mapping["patch_roots"]), 2, "patch 根目录数")
    print("weapons-core: OK")
```

同时在文件顶部 `import json`，并在 `SUITES` 注册 `"weapons-core": validate_weapons_core`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-core
```

预期：失败，提示 `tools/data/mmt_weapon_mapping.json` 不存在。

- [ ] **Step 3: 创建映射 JSON**

创建 `tools/data/mmt_weapon_mapping.json`，核心内容必须包含：

```json
{
  "thing_prefix": "MMT_",
  "source_root": "Hardcore-SK/Mods/Core_SK",
  "combat_extended_root": "Hardcore-SK/Mods/CombatExtended",
  "translation_root": "Hardcore-SK/1.6/Languages/ChineseSimplified (简体中文)/DefInjected",
  "patch_roots": [
    "Hardcore-SK/Mods/Core_SK_CoreModule/Patches",
    "Hardcore-SK/Mods/Core_SK_DLCModule"
  ],
  "batches": {
    "conventional": ["Weapons_Pistols.xml", "Weapons_SMGs.xml", "Weapons_Carabines.xml", "Weapons_Rifles.xml", "Weapons_Shotguns.xml", "Weapons_SniperRifles.xml", "Weapons_SniperRiflesAdvanced.xml", "Weapons_Heavy.xml", "Weapons_Launchers.xml", "Weapons_Flamethrower.xml"],
    "archaic": ["Weapons_Bows.xml", "Weapons_Crossbows.xml", "Weapons_Melee_Neolithic.xml", "Weapons_Melee_Medieval.xml", "Weapons_Melee_Midworld.xml", "Weapons_Melee_Hightech.xml", "Weapons_Grenades.xml"],
    "futuristic": ["Weapons_Archotech.xml", "Weapons_Blasters.xml", "Weapons_Charged.xml", "Weapons_Lasers.xml", "Weapons_Plasma.xml"],
    "turrets": ["Weapons_Turrets_Heavy.xml", "Weapons_Turrets_Launchers.xml", "Weapons_Turrets_Machineguns.xml", "Weapons_Turrets_Sentry.xml"],
    "npc": ["Weapons_Mechanoid.xml", "Weapons_Ogre.xml", "Weapons_RangedSpecial.xml", "Weapons_Special.xml"]
  },
  "excluded_recipes": [
    "DeconstructPersonalShield",
    "MakeWeapon_Parts", "MakePistol_Component", "MakeSMG_Component", "MakeShotgun_Component",
    "MakeRifle_Component", "MakeAdvRifle_Component", "MakeSniper_Component", "MakeAdvSniper_Component",
    "MakeHeavy_Component", "MakeCannon_Component", "MakeLauncher_Component", "MakeCharged_Component",
    "MakeLaser_Component", "MakePlasma_Component"
  ],
  "recipe_product_clones": {
    "patch_replace": [
      "Gun_BoltActionRifle", "Gun_ChargeRifle", "Weapon_GrenadeMolotov", "Weapon_GrenadeFrag",
      "Weapon_GrenadeTox", "Weapon_GrenadeEMP", "Gun_LMG", "Gun_Minigun",
      "Gun_IncendiaryLauncher", "Gun_Revolver", "Gun_Autopistol", "Gun_AssaultRifle",
      "Gun_PumpShotgun", "Gun_ChainShotgun", "Gun_MachinePistol", "Gun_HeavySMG", "Gun_SniperRifle"
    ],
    "combat_extended_defs": [
      "CE_Weapon_GrenadeStickBomb", "CE_Weapon_GrenadeFirefoam", "CE_Weapon_GrenadeFlashbang",
      "CE_Weapon_GrenadeSmoke", "CE_FlareGun"
    ]
  },
  "workbenches": {
    "WeaponCraftingWorkTable": "MMT_WeaponCraftingTable",
    "AdvWeaponCraftingWorkTable": "MMT_AdvWeaponCraftingTable",
    "MechWeaponCraftingWorkTable": "MMT_MechWeaponTable",
    "HeavyArmsBench": "MMT_AdvWeaponCraftingTable",
    "TableHeavyAmmunition": "MMT_AmmoBench"
  },
  "ingredient_defs": {
    "Weapon_Parts": "MMT_WeaponParts",
    "Pistol_Component": "MMT_PistolComponent",
    "SMG_Component": "MMT_SMGComponent",
    "Shotgun_Component": "MMT_ShotgunComponent",
    "Rifle_Component": "MMT_RifleComponent",
    "AdvRifle_Component": "MMT_AdvRifleComponent",
    "Sniper_Component": "MMT_SniperComponent",
    "AdvSniper_Component": "MMT_AdvSniperComponent",
    "Heavy_Component": "MMT_HeavyComponent",
    "Cannon_Component": "MMT_CannonComponent",
    "Launcher_Component": "MMT_LauncherComponent",
    "Charged_Component": "MMT_ChargedComponent",
    "Laser_Component": "MMT_LaserComponent",
    "Plasma_Component": "MMT_PlasmaComponent",
    "Plastic": "MMT_Plastic",
    "Mechanism": "MMT_Mechanism",
    "ElectronicComponents": "MMT_ElectronicParts",
    "Electronics": "MMT_IntegratedCircuit",
    "Microchips": "MMT_Microchip"
  },
  "ingredient_categories": {
    "SLDBar": "MMT_SLDBar",
    "USLDBar": "MMT_USLDBar",
    "HCMBar": "MMT_HCMBar",
    "Metallic": "Metallic",
    "Leathers": "Leathers",
    "Woody": "Woody"
  },
  "sound_fallbacks": {
    "Shot_Tavor": "Shot_AssaultRifle",
    "FlameThrower": "Shot_MiniFlameblaster"
  },
  "research": {
    "Melee_C1": "LongBlades",
    "Melee_C2": "MMT_MeleeAdvanced",
    "Melee_D1": "MMT_MeleeAdvanced",
    "Melee_E1": "MMT_MeleeUltra",
    "Melee_Archotech": "MMT_ArchotechWeapons",
    "Bow_A1": "MMT_BowsBasic",
    "Bow_B2": "Greatbow",
    "Bow_C1": "Machining",
    "Bow_Archotech": "MMT_ArchotechWeapons",
    "Pistols_C2": "Gunsmithing",
    "SMG_C2": "BlowbackOperation",
    "SMG_Archotech": "MMT_ArchotechWeapons",
    "Rifles_C1": "GasOperation",
    "Sniper_rifles_C1": "PrecisionRifling",
    "Grenade_C1": "MMT_GrenadesBasic",
    "Grenade_C2": "MMT_GrenadesAdvanced",
    "IED_C1": "MMT_GrenadesAdvanced",
    "Range_C2": "MMT_GunsAdvanced",
    "Shotgun_C2": "MMT_GunsAdvanced",
    "Rifles_C2": "MMT_RiflesAdvanced",
    "Rifles_C3": "MMT_RiflesMaster",
    "Heavy_C3": "MultibarrelWeapons",
    "Grenade_launchers_C1": "MMT_GrenadeLaunchers",
    "Grenade_launchers_C2": "MMT_GrenadeLaunchers",
    "Rockets_C1": "MMT_Rocketry1",
    "Sniper_rifles_D1": "MMT_SnipersAdvanced",
    "Blaster_weapons_Archotech": "MMT_ArchotechWeapons",
    "Laser_weapons_D": "MMT_DirectedEnergyWeapons1",
    "Plasma_weapons_D": "MMT_DirectedEnergyWeapons1",
    "Charge_weapons_D": "MMT_DirectedEnergyWeapons1",
    "Laser_weapons_E": "MMT_DirectedEnergyWeapons2",
    "Plasma_weapons_E": "MMT_DirectedEnergyWeapons2",
    "Charge_weapons_E": "MMT_DirectedEnergyWeapons2",
    "Ballista_B1": "MMT_SiegeWeapons",
    "Turrets_31": "MMT_Turrets1",
    "Heavy_turrets_C2": "MMT_HeavyTurrets2",
    "Heavy_turrets_C3": "MMT_HeavyTurrets3",
    "Artillery_C2": "MMT_Artillery2",
    "Autoturrets_D2": "MMT_Autoturrets2",
    "Autoturrets_D3": "MMT_Autoturrets3",
    "Autoturrets_E1": "MMT_Autoturrets4",
    "Rockets_C2": "MMT_Rocketry2",
    "Rockets_D1": "MMT_Rocketry3",
    "Rockets_D2": "MMT_Rocketry3"
  }
}
```

- [ ] **Step 4: 实现生成器骨架**

实现以下骨架，后续任务只扩展转换函数：

```python
#!/usr/bin/env python3
"""从本地 HSK/CE XML 生成 MMT 武器 Def；所有人工覆盖写入 mapping JSON。"""
from __future__ import annotations

import argparse
import json
import shutil
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MAPPING_PATH = ROOT / "tools/data/mmt_weapon_mapping.json"

@dataclass(frozen=True)
class SourceIndex:
    thing_defs: dict[str, ET.Element]
    recipe_defs: dict[str, ET.Element]
    research_defs: dict[str, ET.Element]
    sound_defs: dict[str, ET.Element]
    strings_zh: dict[str, str]


def load_mapping(path: Path = MAPPING_PATH) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def mmt_name(source_name: str) -> str:
    return source_name if source_name.startswith("MMT_") else f"MMT_{source_name}"


def collect_defs(root: Path, pattern: str, tag: str) -> dict[str, ET.Element]:
    result: dict[str, ET.Element] = {}
    for path in sorted(root.glob(pattern)):
        for node in ET.parse(path).getroot().iter(tag):
            name = node.findtext("defName") or node.get("Name")
            if name:
                result[name] = node
    return result


def load_chinese_strings(root: Path) -> dict[str, str]:
    strings: dict[str, str] = {}
    for path in sorted(root.rglob("*.xml")):
        for node in ET.parse(path).getroot():
            if isinstance(node.tag, str) and node.text and node.text.strip():
                strings[node.tag] = node.text.strip()
    return strings


def load_source_index(mapping: dict) -> SourceIndex:
    source = ROOT / mapping["source_root"]
    ce = ROOT / mapping["combat_extended_root"]
    translations = ROOT / mapping["translation_root"]
    return SourceIndex(
        thing_defs=collect_defs(source, "Defs/**/*.xml", "ThingDef") | collect_defs(ce, "Defs/**/*.xml", "ThingDef"),
        recipe_defs=collect_defs(source, "Defs/**/*.xml", "RecipeDef") | collect_defs(ce, "Defs/**/*.xml", "RecipeDef"),
        research_defs=collect_defs(source, "Defs/ResearchProjectDefs/*.xml", "ResearchProjectDef"),
        sound_defs=collect_defs(source, "Defs/SoundDefs/*.xml", "SoundDef"),
        strings_zh=load_chinese_strings(translations),
    )


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--batch", required=True, choices=["core", "research", "conventional", "archaic", "futuristic", "turrets", "npc", "all"])
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args(argv)
    if args.check == args.write:
        parser.error("必须且只能指定 --check 或 --write")
    mapping = load_mapping()
    load_source_index(mapping)
    return 0
```

- [ ] **Step 5: 运行验证确认通过**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-core
```

预期：输出 `weapons-core: OK`。

- [ ] **Step 6: 提交**

Python/JSON 属非自动 add 类型，先取得用户确认，然后：

```bash
git add tools/build_mmt_weapons_from_hsk.py tools/data/mmt_weapon_mapping.json tools/validate_mod.py
git commit -m "feat: 添加武器移植映射表与生成器骨架"
```

---

### Task 2: W0 共享基类、投射物、音效与资源复制

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Base.xml`
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml`
- Create: `1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 1 的 `SourceIndex`、`load_mapping()`、`mmt_name()`。
- Produces:
  - `resolve_inherited_text(index: SourceIndex, def_name: str, path: str) -> str | None`
  - `convert_projectile(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element`
  - `convert_sound(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element | None`
  - `copy_referenced_assets(index: SourceIndex, mapping: dict) -> list[Path]`

- [ ] **Step 1: 写失败验证**

新增 `validate_weapons_projectiles()`：

```python
def validate_weapons_projectiles() -> None:
    projectiles = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml"))
    bullet = projectiles["MMT_Bullet_762x51mmNATO_FMJ"]
    assert_equal(child_text(bullet, "projectile/damageDef"), "Bullet", "7.62 伤害类型")
    assert_equal(child_text(bullet, "projectile/damageAmountBase"), "20", "7.62 伤害")
    assert_equal(child_text(bullet, "projectile/armorPenetrationBase"), "0.07", "7.62 穿甲")
    assert_equal(child_text(bullet, "projectile/speed"), "156", "7.62 速度")

    fifty = projectiles["MMT_Bullet_50BMG_FMJ"]
    assert_equal(child_text(fifty, "projectile/damageAmountBase"), "42", ".50 伤害")
    assert_equal(child_text(fifty, "projectile/armorPenetrationBase"), "0.14", ".50 穿甲")

    sounds = def_map(parse_xml("1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml"))
    if "MMT_Shot_Tavor" in sounds or "MMT_FlameThrower" in sounds:
        fail("缺失音频不得生成 MMT SoundDef，必须走原版 fallback")
    print("weapons-projectiles: OK")
```

注册 `"weapons-projectiles": validate_weapons_projectiles`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-projectiles
```

预期：失败，提示 `MMT_Projectiles.xml` 不存在。

- [ ] **Step 3: 实现投射物转换**

实现继承解析和转换：

```python
def resolve_inherited_text(index: SourceIndex, def_name: str, path: str) -> str | None:
    seen: set[str] = set()
    current = index.thing_defs.get(def_name)
    while current is not None and def_name not in seen:
        seen.add(def_name)
        found = current.find(path)
        if found is not None and found.text:
            return found.text
        parent = current.get("ParentName")
        if not parent:
            return None
        def_name = parent
        current = index.thing_defs.get(parent)
    return None


def convert_projectile(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element:
    out = ET.Element("ThingDef", {"ParentName": "BaseBullet"})
    ET.SubElement(out, "defName").text = mmt_name(source_name)
    projectile = ET.SubElement(out, "projectile")
    damage = resolve_inherited_text(index, source_name, "projectile/damageDef") or "Bullet"
    amount = resolve_inherited_text(index, source_name, "projectile/damageAmountBase")
    speed = resolve_inherited_text(index, source_name, "projectile/speed") or "70"
    sharp = resolve_inherited_text(index, source_name, "projectile/armorPenetrationSharp") or "0"
    ET.SubElement(projectile, "damageDef").text = damage
    ET.SubElement(projectile, "damageAmountBase").text = amount or "1"
    ET.SubElement(projectile, "armorPenetrationBase").text = f"{float(sharp) / 100:.2f}".rstrip("0").rstrip(".")
    ET.SubElement(projectile, "speed").text = speed
    return out
```

特殊爆炸/火焰字段不能只写基础实现；必须继续映射 `explosionRadius`、`explosionDamageType`、`preExplosionSpawnThingDef`、`postExplosionSpawnThingDef`、`stunChance` 等原版字段，无法识别的 CE 字段写入映射表 `manual_review`。

- [ ] **Step 4: 实现音效转换与资源复制**

- `convert_sound()`：SoundDef 名称加 `MMT_` 前缀；`clipPath` 存在 `.ogg/.wav/.mp3` 时复制到 `1.7/Sounds/` 同路径；`Shot_Tavor` 和 `FlameThrower` 不生成 MMT SoundDef，武器引用由映射表替换。
- `copy_referenced_assets()`：只复制被生成 Def 引用的贴图/音频，不整目录复制。
- 生成 `MMT_Weapons_Base.xml`，包含 `MMT_BaseWeapon`、`MMT_BaseGun`、`MMT_BaseMeleeWeapon`、`MMT_BaseTurretGun` 四个原版组件基类。

- [ ] **Step 5: 生成并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch core --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-projectiles
```

预期：输出 `weapons-projectiles: OK`。

- [ ] **Step 6: 提交**

XML 可直接 add；Python 修改先按项目规则取得用户确认：

```bash
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Base.xml 1.7/Defs/ThingDefs_Weapons/MMT_Projectiles.xml 1.7/Defs/SoundDefs/MMT_Sounds_Weapons.xml
git add tools/build_mmt_weapons_from_hsk.py tools/validate_mod.py
git commit -m "feat: 添加武器共享基类投射物与音效转换"
```

---

### Task 3: W0 压缩研究树

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 1 mapping 的 `research` 表。
- Produces: `build_research_defs(index: SourceIndex, mapping: dict) -> list[ET.Element]`；24 个新 MMT 研究节点，另复用现有 `MMT_Rocketry1`。

- [ ] **Step 1: 写失败验证**

新增 `validate_weapons_research()`，断言恰好 24 个新节点且无重复坐标：

```python
WEAPON_RESEARCH_TOTAL = 24

def validate_weapons_research() -> None:
    research = def_map(parse_xml("1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml"))
    assert_equal(len(research), WEAPON_RESEARCH_TOTAL, "武器研究节点数")
    coords = [(child_text(r, "researchViewX"), child_text(r, "researchViewY")) for r in research.values()]
    assert_equal(len(coords), len(set(coords)), "武器研究坐标重复")
    assert_equal(child_text(research["MMT_Rocketry2"], "baseCost"), "1300", "火箭 II 成本")
    assert_equal(li_values(research["MMT_Rocketry2"], "prerequisites"), ["MMT_Rocketry1", "SniperTurret", "MMT_GrenadeLaunchers", "Mortars"], "火箭 II 前置")
    print("weapons-research: OK")
```

注册 `"weapons-research": validate_weapons_research`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-research
```

预期：文件不存在。

- [ ] **Step 3: 生成研究 XML**

24 个新节点固定为：

```text
MMT_MeleeAdvanced, MMT_MeleeUltra,
MMT_BowsBasic,
MMT_GrenadesBasic, MMT_GrenadesAdvanced,
MMT_GunsAdvanced, MMT_RiflesAdvanced, MMT_RiflesMaster,
MMT_GrenadeLaunchers, MMT_SnipersAdvanced,
MMT_DirectedEnergyWeapons1, MMT_DirectedEnergyWeapons2,
MMT_ArchotechWeapons,
MMT_SiegeWeapons, MMT_Turrets1,
MMT_HeavyTurrets2, MMT_HeavyTurrets3,
MMT_Artillery2, MMT_Autoturrets2, MMT_Autoturrets3, MMT_Autoturrets4,
MMT_Rocketry2, MMT_Rocketry3
```

合并多个 HSK 节点时：`baseCost` 取最大值，`techLevel` 取最高层级，前置取映射后并集；非武器前置映射为：

```python
NON_WEAPON_RESEARCH = {
    "Metals_C1": "MMT_BasicMetalworking",
    "Electronics_C2": "MMT_IntegratedCircuitTech",
    "Components_D1": "MMT_AdvancedComponents",
    "Components_D3": "MMT_UltraComponents",
    "Components_E1": "MMT_UltraComponents",
    "Stones_C1": "MMT_ConcreteBasic",
    "Craft_B1": "MMT_Woodworking",
    "Mechanoids": "MMT_Robotics1",
}
```

- [ ] **Step 4: 生成并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch research --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-research
```

预期：`weapons-research: OK`。

- [ ] **Step 5: 提交**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Weapons.xml
git add tools/build_mmt_weapons_from_hsk.py tools/validate_mod.py
git commit -m "feat: 添加压缩武器研究树"
```

---

### Task 4: W1 常规枪械与 58 条配方

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 投射物、Task 3 研究映射。
- Produces:
  - `convert_weapon(source_name: str, batch: str, index: SourceIndex, mapping: dict) -> ET.Element`
  - `convert_recipe(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element`
  - 57 个常规武器 Def、58 条常规配方。

- [ ] **Step 1: 写失败验证**

新增 `validate_weapons_conventional()`：

```python
def validate_weapons_conventional() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml"))
    assert_equal(len(weapons), 57, "常规武器数")
    assert_equal(len(recipes), 58, "常规武器配方数")

    deringer = weapons["MMT_RF_Gun_Deringer"]
    assert_equal(child_text(deringer, "label"), "M1917史密斯威森左轮", "Deringer 中文名")
    assert_equal(child_text(deringer, "verbs/li/defaultProjectile"), "MMT_Bullet_45ACP_FMJ", "Deringer 投射物")
    assert_equal(child_text(deringer, "verbs/li/range"), "31", "Deringer 射程")
    assert_equal(child_text(deringer, "statBases/RangedWeapon_Cooldown"), "0.31", "Deringer 冷却")

    build = recipes["MMT_BuildHandGun"]
    assert "MMT_Gun_Revolver" in weapons
    assert_equal(child_text(build, "products/MMT_Gun_Revolver"), "1", "左轮产品")
    if "<Gun_Revolver>" in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml"):
        fail("常规武器配方残留未加 MMT_ 前缀的 Gun_Revolver")
    assert "MMT_PistolComponent" in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml")
    assert "Weapon_Parts" not in read_text("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml")
    print("weapons-conventional: OK")
```

注册 `"weapons-conventional": validate_weapons_conventional`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-conventional
```

预期：输出文件不存在。

- [ ] **Step 3: 实现武器与配方转换**

`convert_weapon()` 必须执行：

```python
CE_REMOVE_STATS = {"SightsEfficiency", "ShotSpread", "SwayFactor", "Bulk"}
CE_REMOVE_COMPS = {
    "CombatExtended.CompProperties_AmmoUser",
    "CombatExtended.CompProperties_FireModes",
    "CombatExtended.CompProperties_Charges",
    "CombatExtended.CompProperties_Fragments",
}
```

- `defaultProjectile` 加 `MMT_` 前缀。
- `soundCast/soundCastTail` 若在 HSK 音效索引中则加 `MMT_` 前缀；`Shot_Tavor/FlameThrower` 用原版 fallback。
- 配方输入先映射 `ingredient_defs`，再映射 `ingredient_categories`；`fixedIngredientFilter` 使用同一映射结果。
- `researchPrerequisite` 必须经 `mapping["research"]` 转换。
- 配方 defName 加 `MMT_` 前缀，产品武器 defName 加 `MMT_` 前缀。
- 22 个补充克隆按批次分配：常规批次接收 12 个枪械 patch 克隆与 `CE_FlareGun`；archaic 批次接收 8 个手雷克隆；futuristic 批次接收 `Gun_ChargeRifle`。
- patch 克隆只读取 `SK.PatchOperationReplaceExtended` 的 `<value>`；CE 克隆直接读取 CE 武器 ThingDef；两者都继续走 `convert_weapon()`。

- [ ] **Step 4: 生成并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch conventional --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-conventional
```

预期：`weapons-conventional: OK`。

- [ ] **Step 5: 提交**

```bash
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Conventional.xml
git add tools/build_mmt_weapons_from_hsk.py tools/validate_mod.py
git commit -m "feat: 移植 Core_SK 常规枪械与配方"
```

---

### Task 5: W2 冷兵器、弓弩与手雷

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 `convert_projectile()`、Task 3 研究映射、Task 4 `convert_weapon()`。
- Produces: 47 个 archaic ThingDef（21 个 Core_SK 激活近战、2 弓、4 弩、12 手雷/投射物、8 个手雷补充克隆）与 13 条手雷配方。

- [ ] **Step 1: 写失败验证**

```python
def validate_weapons_archaic() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml"))
    assert_equal(len(weapons), 47, "冷兵器与手雷 Def 数")
    assert_equal(len(recipes), 13, "手雷配方数")
    greatsword = weapons["MMT_MeleeWeapon_GreatSword"]
    assert_equal(child_text(greatsword, "tools/li/power"), "7", "巨剑首个工具威力")
    assert "MMT_Weapon_GrenadeFrag" in weapons
    assert "MMT_CE_Weapon_GrenadeSmoke" in weapons
    assert "CombatExtended.ToolCE" not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml")
    print("weapons-archaic: OK")
```

注册 `"weapons-archaic": validate_weapons_archaic`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-archaic
```

预期：文件不存在。

- [ ] **Step 3: 实现 ToolCE 与手雷转换**

- `ToolCE` 输出无 `Class` 的原版 `<li>` 工具节点。
- 弓弩继续用原版 verb，投射物为 MMT 箭/弩箭。
- 手雷 weapon 与内联投射物都加 `MMT_` 前缀；爆炸半径、伤害和预热时间保留。
- 手雷配方挂 `MMT_AmmoBench` 或源配方指定的映射台。

- [ ] **Step 4: 生成并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch archaic --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-archaic
```

预期：`weapons-archaic: OK`。

- [ ] **Step 5: 提交**

```bash
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Archaic.xml
git add tools/build_mmt_weapons_from_hsk.py tools/validate_mod.py
git commit -m "feat: 移植 Core_SK 冷兵器弓弩与手雷"
```

---

### Task 6: W3 未来与远古武器

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 特殊投射物转换、Task 3 研究树。
- Produces: 28 个未来/远古武器 Def（27 个 Core_SK 激活 Def + `Gun_ChargeRifle` 补充克隆）与 16 条配方。

- [ ] **Step 1: 写失败验证**

```python
def validate_weapons_futuristic() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml"))
    recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml"))
    assert_equal(len(weapons), 28, "未来武器数")
    assert_equal(len(recipes), 16, "未来武器配方数")
    laser = weapons["MMT_Gun_LaserRifle"]
    assert_equal(child_text(laser, "verbs/li/defaultProjectile"), "MMT_Bullet_Laserbeam", "激光步枪投射物")
    assert "MMT_Gun_ChargeRifle" in weapons
    assert "SK.Graphic_SingleNoColor" not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml")
    print("weapons-futuristic: OK")
```

注册 `"weapons-futuristic": validate_weapons_futuristic`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-futuristic
```

预期：文件不存在。

- [ ] **Step 3: 实现特殊投射物人工覆盖**

在映射 JSON 添加 `projectile_overrides`，至少显式覆盖：

```json
{
  "Bullet_Laserbeam": {"damageDef": "Burn", "damageAmountBase": 18, "speed": 200},
  "Bullet_Plasma": {"damageDef": "Burn", "damageAmountBase": 24, "speed": 120},
  "Bullet_6x24mmCharged": {"damageDef": "Bullet", "damageAmountBase": 18, "speed": 180}
}
```

如果 HSK 源值与上述默认值不同，以 HSK 源值为准更新 JSON；不允许在生成 XML 中手工改数值。

- [ ] **Step 4: 生成并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch futuristic --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-futuristic
```

预期：`weapons-futuristic: OK`。

- [ ] **Step 5: 提交**

```bash
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Weapons_Futuristic.xml
git add tools/build_mmt_weapons_from_hsk.py tools/data/mmt_weapon_mapping.json tools/validate_mod.py
git commit -m "feat: 移植 Core_SK 未来与远古武器"
```

---

### Task 7: W4 炮塔、炮塔箱与装填弹药

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml`
- Create: `1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml`
- Create: `1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_TurretAmmo.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 投射物、Task 3 研究树、Task 4 配方转换。
- Produces:
  - `convert_turret_building(source_name: str, index: SourceIndex, mapping: dict) -> ET.Element`
  - `build_turret_ammo_item(ammo_set: str, index: SourceIndex, mapping: dict) -> ET.Element`
  - 16 个炮塔枪、16 个可建造炮塔、26 个炮塔箱、26 条炮塔箱配方、11 种炮塔弹药。

- [ ] **Step 1: 写失败验证**

```python
TURRET_AMMO_PROJECTILES = {
    "MMT_Ammo_BalistaBolt": "MMT_Projectile_BalistaBolt",
    "MMT_Ammo_StoneBall": "MMT_Stone_Ball",
    "MMT_Ammo_30x29mmGrenade_HE": "MMT_Bullet_30x29mmGrenade_HE",
    "MMT_Ammo_120mmCannonShell_HEAT": "MMT_Bullet_120mmCannonShell_HEAT",
    "MMT_Ammo_155mmHowitzerShell_HE": "MMT_Bullet_155mmHowitzerShell_HE",
    "MMT_Ammo_SPG9Rocket_Frag": "MMT_Bullet_SPG9Rocket_Frag",
    "MMT_Ammo_130mmRocketMissile": "MMT_Bullet_130mmRocketMissile",
    "MMT_Ammo_50BMG_FMJ": "MMT_Bullet_50BMG_FMJ",
    "MMT_Ammo_762x51mmNATO_FMJ": "MMT_Bullet_762x51mmNATO_FMJ",
    "MMT_Ammo_30x173mm_FMJ": "MMT_Bullet_30x173mm_FMJ",
    "MMT_Ammo_20x102mm_FMJ": "MMT_Bullet_20x102mm_FMJ",
}

def validate_weapons_turrets() -> None:
    guns = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml"))
    buildings = def_map(parse_xml("1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml"))
    ammo = def_map(parse_xml("1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml"))
    crates = def_map(parse_xml("1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml"))
    crate_recipes = def_map(parse_xml("1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml"))
    assert_equal(len(guns), 16, "炮塔枪数")
    concrete_buildings = [b for b in buildings.values() if (b.get("Abstract") or "").lower() != "true"]
    assert_equal(len(buildings), 17, "炮塔建筑文件 Def 数（含 MMT_TurretBuildingBase）")
    assert_equal(len(concrete_buildings), 16, "可建造炮塔数")
    assert_equal(len(ammo), 11, "炮塔弹药数")
    assert_equal(len(crates), 26, "炮塔箱数")
    assert_equal(len(crate_recipes), 26, "炮塔箱配方数")
    for ammo_name, projectile_name in TURRET_AMMO_PROJECTILES.items():
        assert_equal(child_text(ammo[ammo_name], "projectileWhenLoaded"), projectile_name, f"{ammo_name} 投射物")

    m2 = buildings["MMT_Turret_M2Browning"]
    assert_equal(child_text(m2, "building/turretGunDef"), "MMT_Gun_M2Browning", "M2 炮塔枪")
    refuelable = m2.find("./comps/li[@Class='CompProperties_Refuelable']")
    if refuelable is None:
        fail("M2 炮塔缺少 refuelable")
    assert_equal(child_text(refuelable, "fuelFilter/thingDefs/li"), "MMT_Ammo_50BMG_FMJ", "M2 弹药")
    assert_equal(child_text(refuelable, "fuelCapacity"), "150", "M2 弹容量")
    assert_equal(child_text(guns["MMT_Gun_M2Browning"], "verbs/li/consumeFuelPerShot"), "1", "M2 单发耗弹")
    print("weapons-turrets: OK")
```

注册 `"weapons-turrets": validate_weapons_turrets`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-turrets
```

预期：文件不存在。

- [ ] **Step 3: 实现炮塔和弹药转换**

- 11 种弹药来自 16 个激活炮塔枪的唯一 `ammoSet`：Balista、Catapult、30x29mmGrenade、120mmCannonShell、155mmHowitzerShell、SPG9Rocket、130mmRocketMissile、50BMG、762x51mmNATO、30x173mm、20x102mm。
- `fuelCapacity` 取对应炮塔枪的 CE `magazineSize`；同一弹药被多个炮塔使用时取最大值（例如 20x102mm 为 200）。
- 炮塔建筑移除 `SK.CompProperties_IdlePower`，改 vanilla `CompProperties_Power`。
- HSK `TurretMannedBase/TurretBase/TurretMortarBase` 统一映射为本文件内的 `MMT_TurretBuildingBase`；该基类以原版 `BuildingBase + Building_TurretGun` 为模型，并按源炮塔是否可人工操作保留 `CompProperties_Mannable`。
- HSK `MinifiedTurret_*` 不移植，所有炮塔 `minifiedDef` 统一为原版 `MinifiedThing`。
- 炮塔箱物品、26 条箱配方、建筑 costList 全部恢复并加 `MMT_` 前缀。
- 弹药配方材料来自 CE 对应默认弹药配方；工作台映射 `MMT_AmmoBench`。

- [ ] **Step 4: 先生成 M2 最小样本并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch turrets --write --only Turret_M2Browning
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-turrets
```

预期：静态断言通过。随后启动游戏进行一次手动 smoke check：建造 M2 炮塔、装填 150 发、射击后弹药减少。若失败，停止本任务，回到 spec 修改耗弹机制，不继续批量生成。

- [ ] **Step 5: 生成完整炮塔批次并验证**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch turrets --write
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-turrets
```

预期：`weapons-turrets: OK`。

- [ ] **Step 6: 提交**

```bash
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Turrets.xml 1.7/Defs/ThingDefs_Items/MMT_Items_TurretAmmo.xml 1.7/Defs/ThingDefs_Items/MMT_Items_TurretCrates.xml 1.7/Defs/RecipeDefs/MMT_Recipes_TurretAmmo.xml 1.7/Defs/RecipeDefs/MMT_Recipes_TurretCrates.xml
git add tools/build_mmt_weapons_from_hsk.py tools/data/mmt_weapon_mapping.json tools/validate_mod.py
git commit -m "feat: 移植 Core_SK 炮塔与弹药装填链"
```

---

### Task 8: W5 NPC/特殊武器

**Files:**
- Create: `1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml`
- Modify: `tools/build_mmt_weapons_from_hsk.py`
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: Task 2 投射物与 Task 4 武器转换。
- Produces: 14 个 NPC/特殊武器 ThingDef（机械体 5、Ogre 3、RangedSpecial 1、Special 5）。

- [ ] **Step 1: 写失败验证**

```python
def validate_weapons_npc() -> None:
    weapons = def_map(parse_xml("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml"))
    assert_equal(len(weapons), 14, "NPC/特殊武器数")
    assert "MMT_Gun_mechlaser" in weapons
    assert "MMT_Gun_SawGun" in weapons
    assert "CombatExtended." not in read_text("1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml")
    print("weapons-npc: OK")
```

注册 `"weapons-npc": validate_weapons_npc`。

- [ ] **Step 2: 运行验证确认失败**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-npc
```

预期：文件不存在。

- [ ] **Step 3: 生成 NPC 批次**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch npc --write
```

要求：NPC 武器不生成配方；`tradeability=None` 和隐藏字段保留；投射物仍转换为 MMT。

- [ ] **Step 4: 验证并提交**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons-npc
git add 1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml
git add tools/build_mmt_weapons_from_hsk.py tools/validate_mod.py
git commit -m "feat: 移植 Core_SK NPC 与特殊武器"
```

---

### Task 9: W6 聚合验证、别名与收尾

**Files:**
- Modify: `tools/validate_mod.py`
- Modify: `tools/build_mmt_chains_compare.js`
- Modify: `docs/mmt-vs-hsk-resource-chains.md`（仅当重跑后有实际差异）

**Interfaces:**
- Consumes: Task 1–8 全部输出。
- Produces: `validate_weapons()` 聚合套件、`SUITES["weapons"]`、`validate_all()` 挂接、炮塔箱/弹药别名。

- [ ] **Step 1: 写聚合验证**

```python
def validate_weapons() -> None:
    validate_weapons_core()
    validate_weapons_projectiles()
    validate_weapons_research()
    validate_weapons_conventional()
    validate_weapons_archaic()
    validate_weapons_futuristic()
    validate_weapons_turrets()
    validate_weapons_npc()
    weapon_files = [
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Conventional.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Archaic.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Futuristic.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_TurretGuns.xml",
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_NPC.xml",
    ]
    total_weapons = sum(len(def_map(parse_xml(p))) for p in weapon_files)
    assert_equal(total_weapons, 162, "武器/投射物总目标数")
    weapon_text = "\n".join(read_text(p) for p in [
        "1.7/Defs/ThingDefs_Weapons/MMT_Weapons_Base.xml",
        *weapon_files,
    ])
    for forbidden in ("CombatExtended.", "SK.", "RightToolForJob."):
        if forbidden in weapon_text:
            fail(f"武器 Def 残留禁用引用: {forbidden}")
    print("weapons: OK")
```

注册：

```python
SUITES["weapons"] = validate_weapons
```

并在 `validate_all()` 中 `validate_textiles_weapons()` 后调用 `validate_weapons()`。

- [ ] **Step 2: 运行聚合验证**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py weapons
```

预期：`weapons: OK`。

- [ ] **Step 3: 更新别名与文档**

- `tools/build_mmt_chains_compare.js` 的 `MMT_ALIAS` 添加 26 个炮塔箱和 11 个炮塔弹药映射。
- 运行：

```bash
node tools/build_mmt_chains_compare.js
```

- 如果对比文档没有变化，不提交空改动。

- [ ] **Step 4: 检查生成器可重复性**

```bash
PYTHONIOENCODING=utf-8 python tools/build_mmt_weapons_from_hsk.py --batch all --check
```

预期：退出码 0，输出 `generated files are up to date`。

- [ ] **Step 5: 全量验证与编译**

```bash
PYTHONIOENCODING=utf-8 python tools/validate_mod.py all
dotnet build Source/MyModeTest/MyModeTest.csproj
```

预期：静态验证全部 OK，`dotnet build` 0 错误。

- [ ] **Step 6: 最终提交**

```bash
git add tools/validate_mod.py
git add 1.7/Defs
git commit -m "test: 添加 Core_SK 武器全量验证套件"
```

如需提交 JS/Markdown，先按项目规则取得用户确认，再单独提交：

```bash
git add tools/build_mmt_chains_compare.js docs/mmt-vs-hsk-resource-chains.md
git commit -m "docs: 更新武器资源链别名与对比文档"
```
