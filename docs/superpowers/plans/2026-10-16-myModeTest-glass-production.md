# myModeTest 玻璃资源生产链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 `D:\project\rw` 创建 RimWorld 1.7 Mod `myModeTest`，实现独立的“石块/岩块 → 碎石 → 砂 → 玻璃配合料 → 玻璃”生产链。

**Architecture:** 使用标准 RimWorld `About`、版本化 `1.7` 目录和纯 XML Def；碎石加工复用原版工作台，玻璃加工使用新增的 `MMT_GlassworksTable`。C# 工程仅提供最小 Mod 入口，不复制 HSK 类型、程序集或动画组件。

**Tech Stack:** RimWorld 1.7 XML Def、SDK 风格 .NET 10 C# 项目、Python 3 静态验证脚本、Git。

**Spec:** `docs/superpowers/specs/2026-10-16-myModeTest-glass-production-design.md`

## Global Constraints

- Mod 显示名必须为 `myModeTest`。
- `packageId` 必须为 `YourName.myModeTest`。
- C# 命名空间和程序集名必须为 `MyModeTest`。
- 自定义 Def 必须使用 `MMT_` 前缀。
- `About.xml` 只能声明支持 RimWorld `1.7`。
- 所有自定义 label、description、jobString 使用中文；`defName` 使用英文。
- 不依赖 HSK；XML 和 C# 中不得引用 `SK.` 类型或 HSK 专属 Def。
- HSK 贴图来源必须固定为提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。
- C# 项目必须使用 `net10.0`，并引用 `D:\RimWorld v1.7.4979` 根目录下的 `Assembly-CSharp.dll` 与 `UnityStubs.dll`。
- 安装 .NET 10 SDK 后必须执行 `dotnet build`，编译失败必须先修正；不得在未成功时声称编译通过。
- 本次不得启动 RimWorld，不得声称游戏内加载通过。
- 所有文本文件使用 UTF-8。
- 只提交到本地 Git 仓库，不执行 push。

## Review Focus

1. 原版 ID `TableStonecutter`、`TableMachining`、`Stonecutting`、`StoneBlocks`、`StoneChunks` 若在 1.7 改名，会导致配方或研究不可用；Task 4 的 `python tools/validate_mod.py recipes` 固定检查这些精确引用。
2. 误引用 HSK `SK.` 类型会破坏独立运行目标；Task 3 和 Task 4 的验证套件扫描相关 XML，要求不得包含 `SK.`。
3. `Graphic_Multi` 与 `Graphic_StackCount` 的贴图后缀或路径不一致会在游戏中丢图；Task 2 和 Task 3 的 PNG 检查固定所有实际文件路径。
4. 1.7 已将程序集从旧版 `Data\Managed` 移到游戏根目录，错误的引用路径会导致编译失败；Task 1 的 `base` 套件和 `dotnet build` 共同固定 `Assembly-CSharp.dll`、`UnityStubs.dll` 根目录引用。
5. HSK 分支漂移可能下载到不同贴图；Task 2 和 Task 3 的所有下载 URL 固定使用提交 `0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。

---

### Task 1: 基础框架与静态验证工具

**Files:**
- Create: `About/About.xml`
- Create: `LoadFolders.xml`
- Create: `.gitignore`
- Create: `README.md`
- Create: `MyModeTest.sln`
- Create: `Source/MyModeTest/MyModeTest.csproj`
- Create: `Source/MyModeTest/MyModeTestMod.cs`
- Create: `tools/validate_mod.py`
- Create: `1.7/Assemblies/`

**Interfaces:**
- Consumes: 已确认设计文档中的名称、目录、构建约束。
- Produces: `python tools/validate_mod.py {base|resources|building|recipes|all}` 验证入口；后续任务使用其中的 `resources`、`building`、`recipes` 和 `all` 套件。

- [ ] **Step 1: 编写静态验证工具**

创建 `tools/validate_mod.py`，内容如下：

```python
#!/usr/bin/env python3
"""myModeTest Mod 静态验证工具。"""
from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"

BASE_FILES = [
    "About/About.xml",
    "LoadFolders.xml",
    ".gitignore",
    "README.md",
    "MyModeTest.sln",
    "Source/MyModeTest/MyModeTest.csproj",
    "Source/MyModeTest/MyModeTestMod.cs",
]

RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml"
RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml"
RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml"
BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml"

ITEM_TEXTURES = [
    "1.7/Textures/Things/Item/MMT/CrushedStone.png",
    "1.7/Textures/Things/Item/MMT/Sand.png",
    "1.7/Textures/Things/Item/MMT/GlassBatch.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_a.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_b.png",
    "1.7/Textures/Things/Item/MMT/Glass/Glass_c.png",
]

BUILDING_TEXTURES = [
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_east.png",
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_north.png",
    "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_south.png",
]

RESOURCE_EXPECTATIONS = {
    "MMT_CrushedStone": {
        "label": "碎石",
        "value": "0.3",
        "stack": "900",
        "tex": "Things/Item/MMT/CrushedStone",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
    },
    "MMT_Sand": {
        "label": "砂",
        "value": "0.3",
        "stack": "900",
        "tex": "Things/Item/MMT/Sand",
        "graphic": "Graphic_Single",
        "tech": "Neolithic",
    },
    "MMT_GlassBatch": {
        "label": "玻璃配合料",
        "value": "1.2",
        "stack": "300",
        "tex": "Things/Item/MMT/GlassBatch",
        "graphic": "Graphic_Single",
        "tech": "Industrial",
    },
    "MMT_Glass": {
        "label": "玻璃",
        "value": "5.0",
        "stack": "150",
        "tex": "Things/Item/MMT/Glass",
        "graphic": "Graphic_StackCount",
        "tech": "Industrial",
    },
}

BUILDING_RECIPES = ["MMT_MakeGlassBatch", "MMT_MakeGlass"]

RECIPE_EXPECTATIONS = {
    "MMT_MakeCrushedStone_Hand": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneBlocks",
        "input_count": "20",
        "output": "MMT_CrushedStone",
        "output_count": "20",
        "users": ["TableStonecutter"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Electric": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneBlocks",
        "input_count": "40",
        "output": "MMT_CrushedStone",
        "output_count": "40",
        "users": ["TableMachining"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Hand_Chunk": {
        "work": "2400",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_CrushedStone",
        "output_count": "32",
        "users": ["TableStonecutter"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeCrushedStone_Electric_Chunk": {
        "work": "1200",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_CrushedStone",
        "output_count": "32",
        "users": ["TableMachining"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Hand": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_CrushedStone",
        "input_count": "10",
        "output": "MMT_Sand",
        "output_count": "25",
        "users": ["TableStonecutter"],
        "research": "Stonecutting",
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Electric": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_CrushedStone",
        "input_count": "20",
        "output": "MMT_Sand",
        "output_count": "50",
        "users": ["TableMachining"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Hand_Chunk": {
        "work": "3000",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_Sand",
        "output_count": "80",
        "users": ["TableStonecutter"],
        "research": "Stonecutting",
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeSand_Electric_Chunk": {
        "work": "1500",
        "input_kind": "categories",
        "input": "StoneChunks",
        "input_count": "1",
        "output": "MMT_Sand",
        "output_count": "80",
        "users": ["TableMachining"],
        "research": None,
        "speed": "StonecuttingSpeed",
        "effect": "CutStone",
        "sound": "Recipe_MakeStoneBlocks",
    },
    "MMT_MakeGlassBatch": {
        "work": "900",
        "input_kind": "thingDefs",
        "input": "MMT_Sand",
        "input_count": "75",
        "output": "MMT_GlassBatch",
        "output_count": "25",
        "users": [],
        "research": "MMT_Glassworking",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
    "MMT_MakeGlass": {
        "work": "600",
        "input_kind": "thingDefs",
        "input": "MMT_GlassBatch",
        "input_count": "20",
        "output": "MMT_Glass",
        "output_count": "10",
        "users": [],
        "research": "MMT_Glassworking",
        "speed": "SmeltingSpeed",
        "effect": "Smelt",
        "sound": "Recipe_Smelt",
    },
}

VANILLA_CATEGORIES = {"StoneBlocks", "StoneChunks", "Metallic"}
VANILLA_WORKTABLES = {"TableStonecutter", "TableMachining"}
VANILLA_RESEARCH = {"Stonecutting"}
VANILLA_THINGS = {"ComponentIndustrial"}


def fail(message: str) -> None:
    raise AssertionError(message)


def require_files(paths: list[str]) -> None:
    missing = [path for path in paths if not (ROOT / path).is_file()]
    if missing:
        fail("缺少文件: " + ", ".join(missing))


def require_dirs(paths: list[str]) -> None:
    missing = [path for path in paths if not (ROOT / path).is_dir()]
    if missing:
        fail("缺少目录: " + ", ".join(missing))


def read_text(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def parse_xml(path: str) -> ET.Element:
    data = (ROOT / path).read_bytes()
    declaration = data[:200].decode("ascii", "ignore").lower()
    if 'encoding="utf-8"' not in declaration and "encoding='utf-8'" not in declaration:
        fail(f"{path}: XML 声明必须指定 UTF-8")
    try:
        return ET.fromstring(data)
    except ET.ParseError as exc:
        fail(f"{path}: XML 解析失败: {exc}")


def child_text(node: ET.Element, path: str) -> str:
    value = node.findtext(path)
    if value is None or not value.strip():
        fail(f"节点缺少必填值: {path}")
    return value.strip()


def li_values(node: ET.Element, path: str) -> list[str]:
    container = node.find(path)
    if container is None:
        return []
    return [li.text.strip() for li in container.findall("li") if li.text and li.text.strip()]


def def_map(root: ET.Element) -> dict[str, ET.Element]:
    result: dict[str, ET.Element] = {}
    for node in root:
        if not isinstance(node.tag, str):
            continue
        name = node.findtext("defName")
        if not name:
            continue
        if name in result:
            fail(f"重复 defName: {name}")
        result[name] = node
    return result


def assert_equal(actual: object, expected: object, label: str) -> None:
    if actual != expected:
        fail(f"{label}: 期望 {expected!r}，实际 {actual!r}")


def assert_png(path: str) -> None:
    data = (ROOT / path).read_bytes()
    if len(data) <= len(PNG_SIGNATURE) or not data.startswith(PNG_SIGNATURE):
        fail(f"{path}: 不是有效的 PNG 文件")


def validate_base() -> None:
    require_dirs(["1.7/Assemblies"])
    require_files(BASE_FILES)

    about = parse_xml("About/About.xml")
    assert_equal(child_text(about, "name"), "myModeTest", "About name")
    assert_equal(child_text(about, "author"), "YourName", "About author")
    assert_equal(child_text(about, "packageId"), "YourName.myModeTest", "About packageId")
    assert_equal(li_values(about, "supportedVersions"), ["1.7"], "支持版本")

    load_folders = parse_xml("LoadFolders.xml")
    assert_equal(li_values(load_folders, "v1.7"), ["1.7"], "LoadFolders v1.7")

    csproj = parse_xml("Source/MyModeTest/MyModeTest.csproj")
    assert_equal(child_text(csproj, "./PropertyGroup/TargetFramework"), "net10.0", "TargetFramework")
    assert_equal(child_text(csproj, "./PropertyGroup/AssemblyName"), "MyModeTest", "AssemblyName")
    assert_equal(child_text(csproj, "./PropertyGroup/RootNamespace"), "MyModeTest", "RootNamespace")
    output_path = child_text(csproj, "./PropertyGroup/OutputPath")
    assert_equal(output_path, "$(MSBuildThisFileDirectory)..\\..\\1.7\\Assemblies\\", "OutputPath")

    for include, assembly_file in (("Assembly-CSharp", "Assembly-CSharp.dll"), ("UnityStubs", "UnityStubs.dll")):
        reference = csproj.find(f"./ItemGroup/Reference[@Include='{include}']")
        if reference is None:
            fail(f"csproj 缺少 {include} 引用")
        assert_equal(child_text(reference, "HintPath"), f"$(RimWorldPath)\\{assembly_file}", f"{include} HintPath")
        assert_equal(child_text(reference, "Private"), "false", f"{include} Private")

    target = csproj.find("./Target[@Name='ValidateRimWorldPath']")
    if target is None:
        fail("csproj 缺少 ValidateRimWorldPath 目标")
    errors = target.findall("Error")
    conditions = [error.attrib.get("Condition", "") for error in errors]
    texts = [error.attrib.get("Text", "") for error in errors]
    if not any("Assembly-CSharp.dll" in condition for condition in conditions):
        fail("ValidateRimWorldPath 未检查 Assembly-CSharp.dll")
    if not any("UnityStubs.dll" in condition for condition in conditions):
        fail("ValidateRimWorldPath 未检查 UnityStubs.dll")
    if not any("RimWorldPath" in text for text in texts):
        fail("ValidateRimWorldPath 错误文本未提示 RimWorldPath")

    mod_source = read_text("Source/MyModeTest/MyModeTestMod.cs")
    for expected in ("namespace MyModeTest", "public sealed class MyModeTestMod : Mod", "MyModeTestMod(ModContentPack content)", "Log.Message"):
        if expected not in mod_source:
            fail(f"MyModeTestMod.cs 缺少: {expected}")

    solution = read_text("MyModeTest.sln")
    if "Source\\MyModeTest\\MyModeTest.csproj" not in solution:
        fail("解决方案未包含 MyModeTest.csproj")

    readme = read_text("README.md")
    if "RimWorldPath" not in readme or "dotnet build" not in readme:
        fail("README 缺少 RimWorldPath 编译说明")

    print("base: OK")


def validate_resources() -> None:
    require_files([RESOURCE_XML, *ITEM_TEXTURES])
    root = parse_xml(RESOURCE_XML)
    defs = def_map(root)
    assert_equal(set(defs), set(RESOURCE_EXPECTATIONS), "资源 defName 集合")

    for name, expected in RESOURCE_EXPECTATIONS.items():
        node = defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")

    for path in ITEM_TEXTURES:
        assert_png(path)

    if "SK." in read_text(RESOURCE_XML):
        fail("资源 XML 不得引用 SK 类型")
    print("resources: OK")


def validate_building() -> None:
    require_files([RESEARCH_XML, BUILDING_XML, *BUILDING_TEXTURES])

    research_defs = def_map(parse_xml(RESEARCH_XML))
    assert_equal(set(research_defs), {"MMT_Glassworking"}, "研究 defName 集合")
    research = research_defs["MMT_Glassworking"]
    assert_equal(child_text(research, "label"), "玻璃制造", "研究 label")
    assert_equal(child_text(research, "baseCost"), "300", "研究 baseCost")
    assert_equal(child_text(research, "techLevel"), "Medieval", "研究 techLevel")
    assert_equal(li_values(research, "prerequisites"), ["Stonecutting"], "研究前置")

    building_defs = def_map(parse_xml(BUILDING_XML))
    assert_equal(set(building_defs), {"MMT_GlassworksTable"}, "建筑 defName 集合")
    building = building_defs["MMT_GlassworksTable"]
    assert_equal(child_text(building, "label"), "玻璃工坊", "建筑 label")
    assert_equal(child_text(building, "thingClass"), "Building_WorkTable", "建筑 thingClass")
    assert_equal(child_text(building, "./graphicData/texPath"), "Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable", "建筑 texPath")
    assert_equal(child_text(building, "./graphicData/graphicClass"), "Graphic_Multi", "建筑 graphicClass")
    assert_equal(child_text(building, "costStuffCount"), "130", "建筑材料数量")
    assert_equal(li_values(building, "stuffCategories"), ["Metallic"], "建筑材料分类")
    assert_equal(child_text(building, "./costList/ComponentIndustrial"), "10", "工业零部件成本")
    assert_equal(child_text(building, "./statBases/WorkToBuild"), "1600", "建造工作量")
    assert_equal(child_text(building, "./statBases/MaxHitPoints"), "180", "最大耐久")
    assert_equal(child_text(building, "size"), "(3,1)", "建筑尺寸")
    assert_equal(li_values(building, "recipes"), BUILDING_RECIPES, "建筑配方")
    assert_equal(li_values(building, "researchPrerequisites"), ["MMT_Glassworking"], "建筑研究前置")
    assert_equal(child_text(building, "constructionSkillPrerequisite"), "9", "建造技能")

    comp_classes = [li.attrib.get("Class") for li in building.findall("./comps/li")]
    for expected in (
        "CompProperties_Power",
        "CompProperties_Flickable",
        "CompProperties_Breakdownable",
        "CompProperties_Glower",
        "CompProperties_HeatPusher",
    ):
        if expected not in comp_classes:
            fail(f"建筑缺少组件: {expected}")

    power = building.find("./comps/li[@Class='CompProperties_Power']")
    if power is None:
        fail("建筑缺少电力组件")
    assert_equal(child_text(power, "compClass"), "CompPowerTrader", "电力 compClass")
    assert_equal(child_text(power, "basePowerConsumption"), "400", "电力消耗")

    for path in BUILDING_TEXTURES:
        assert_png(path)

    for path in (RESEARCH_XML, BUILDING_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    print("building: OK")


def validate_recipes() -> None:
    require_files([RECIPE_XML, RESOURCE_XML, RESEARCH_XML, BUILDING_XML])
    recipe_defs = def_map(parse_xml(RECIPE_XML))
    assert_equal(set(recipe_defs), set(RECIPE_EXPECTATIONS), "配方 defName 集合")

    resource_names = set(def_map(parse_xml(RESOURCE_XML)))
    building_defs = def_map(parse_xml(BUILDING_XML))
    building_names = set(building_defs)
    research_names = set(def_map(parse_xml(RESEARCH_XML)))
    glassworks_recipes = li_values(building_defs["MMT_GlassworksTable"], "recipes")

    for name, expected in RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        if not child_text(recipe, "label"):
            fail(f"{name} 缺少 label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), expected["speed"], f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), expected["effect"], f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), expected["sound"], f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")

        ingredient = recipe.find("./ingredients/li")
        if ingredient is None:
            fail(f"{name} 缺少输入")
        assert_equal(child_text(ingredient, "count"), expected["input_count"], f"{name} 输入数量")
        filter_node = ingredient.find("filter")
        if filter_node is None:
            fail(f"{name} 缺少输入 filter")
        input_values = li_values(filter_node, expected["input_kind"])
        assert_equal(input_values, [expected["input"]], f"{name} 输入引用")

        fixed_filter = recipe.find("fixedIngredientFilter")
        if fixed_filter is None:
            fail(f"{name} 缺少 fixedIngredientFilter")
        fixed_values = li_values(fixed_filter, expected["input_kind"])
        assert_equal(fixed_values, [expected["input"]], f"{name} 固定输入引用")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        users = li_values(recipe, "recipeUsers")
        assert_equal(users, expected["users"], f"{name} recipeUsers")
        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        if expected["input_kind"] == "categories":
            if expected["input"] not in VANILLA_CATEGORIES:
                fail(f"{name} 使用未登记分类: {expected['input']}")
        elif expected["input"] not in resource_names | VANILLA_THINGS:
            fail(f"{name} 使用未知资源: {expected['input']}")

        if expected["output"] not in resource_names:
            fail(f"{name} 输出不是已定义资源: {expected['output']}")

        for user in users:
            if user not in VANILLA_WORKTABLES | building_names:
                fail(f"{name} 使用未知工作台: {user}")

        if research and research not in VANILLA_RESEARCH | research_names:
            fail(f"{name} 使用未知研究: {research}")

        if name in BUILDING_RECIPES and users:
            fail(f"{name} 应由 MMT_GlassworksTable 的 recipes 列表挂载，不应写 recipeUsers")
        if name in BUILDING_RECIPES and name not in glassworks_recipes:
            fail(f"{name} 未挂载到玻璃工坊")

    if "SK." in read_text(RECIPE_XML):
        fail("配方 XML 不得引用 SK 类型")
    print("recipes: OK")


def validate_all_xml() -> None:
    paths = [Path("About/About.xml"), Path("LoadFolders.xml")]
    defs_dir = ROOT / "1.7"
    if defs_dir.exists():
        paths.extend(path.relative_to(ROOT) for path in defs_dir.rglob("*.xml"))
    for path in paths:
        parse_xml(path.as_posix())
    print(f"all-xml: OK ({len(paths)} files)")


def validate_all() -> None:
    validate_base()
    validate_resources()
    validate_building()
    validate_recipes()
    validate_all_xml()


SUITES = {
    "base": validate_base,
    "resources": validate_resources,
    "building": validate_building,
    "recipes": validate_recipes,
    "all": validate_all,
}


def main() -> int:
    parser = argparse.ArgumentParser(description="验证 myModeTest Mod 的静态文件。")
    parser.add_argument("suite", choices=sorted(SUITES))
    args = parser.parse_args()
    try:
        SUITES[args.suite]()
    except (AssertionError, OSError) as exc:
        print(f"[FAIL] {args.suite}: {exc}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
```

- [ ] **Step 2: 运行 base 验证，确认它因缺少基础文件而失败**

Run:

```bash
python tools/validate_mod.py base
```

Expected: 退出码为 `1`，输出包含 `[FAIL] base: 缺少文件:`，并列出 `About/About.xml`。

- [ ] **Step 3: 创建目录结构**

Run:

```bash
mkdir -p "About" "1.7/Assemblies" "1.7/Defs/ThingDefs_Resources" "1.7/Defs/RecipeDefs" "1.7/Defs/ThingDefs_Buildings" "1.7/Defs/ResearchProjectDefs" "1.7/Textures/Things/Item/MMT" "1.7/Textures/Things/Building/Production/MMT_GlassworksTable" "Source/MyModeTest"
```

- [ ] **Step 4: 创建 Mod 元数据和工程文件**

创建 `About/About.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name>myModeTest</name>
  <author>YourName</author>
  <packageId>YourName.myModeTest</packageId>
  <supportedVersions>
    <li>1.7</li>
  </supportedVersions>
  <description>参考 HSK 资源生产流程实现的独立玻璃生产链。</description>
</ModMetaData>
```

创建 `LoadFolders.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <v1.7>
    <li>1.7</li>
  </v1.7>
</loadFolders>
```

创建 `.gitignore`：

```gitignore
.vs/
.idea/
.vscode/
[Bb]in/
[Oo]bj/
*.user
*.suo
docs/temp/
1.7/Assemblies/*.dll
1.7/Assemblies/*.pdb
Thumbs.db
```

创建 `README.md`：

```markdown
# myModeTest

`myModeTest` 是面向 RimWorld 1.7 的独立资源生产 Mod。首版参考 Hardcore SK 的玻璃资源链，实现“石块/岩块 → 碎石 → 砂 → 玻璃配合料 → 玻璃”。

## 目录

- `About/`：Mod 元数据。
- `1.7/Defs/`：资源、配方、建筑和研究定义。
- `1.7/Textures/`：已获授权复制的 HSK 静态贴图。
- `1.7/Assemblies/`：后续 C# 编译输出目录。
- `Source/MyModeTest/`：C# 工程源码。

## 编译

RimWorld 1.7 使用 `net10.0`，编译前需要安装 .NET 10 SDK。工程默认通过 `RimWorldPath` 使用 `D:\RimWorld v1.7.4979`，并引用该目录下的 `Assembly-CSharp.dll` 与 `UnityStubs.dll`。

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj
```

如需使用其他游戏目录，可以执行：

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj -p:RimWorldPath="D:\Games\RimWorld"
```

也可以通过环境变量 `RIMWORLD_PATH` 提供游戏目录。

## 静态验证

```bash
python tools/validate_mod.py all
```

## 参考来源

贴图和流程参考 Hardcore SK `1.7-super-unstable` 分支，固定提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`。本 Mod 不依赖 HSK 程序集或 HSK Def。
```

创建 `MyModeTest.sln`：

```text
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MyModeTest", "Source\MyModeTest\MyModeTest.csproj", "{7D47B790-5F6D-4D32-9B5A-2F064E30F001}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{7D47B790-5F6D-4D32-9B5A-2F064E30F001}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{7D47B790-5F6D-4D32-9B5A-2F064E30F001}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{7D47B790-5F6D-4D32-9B5A-2F064E30F001}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{7D47B790-5F6D-4D32-9B5A-2F064E30F001}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
```

创建 `Source/MyModeTest/MyModeTest.csproj`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>MyModeTest</AssemblyName>
    <RootNamespace>MyModeTest</RootNamespace>
    <LangVersion>latest</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <OutputPath>$(MSBuildThisFileDirectory)..\..\1.7\Assemblies\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <RimWorldPath Condition="'$(RimWorldPath)' == '' and '$(RIMWORLD_PATH)' != ''">$(RIMWORLD_PATH)</RimWorldPath>
    <RimWorldPath Condition="'$(RimWorldPath)' == ''">D:\RimWorld v1.7.4979</RimWorldPath>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(RimWorldPath)\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityStubs">
      <HintPath>$(RimWorldPath)\UnityStubs.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <Target Name="ValidateRimWorldPath" BeforeTargets="ResolveReferences">
    <Error Condition="!Exists('$(RimWorldPath)\Assembly-CSharp.dll')"
           Text="未找到 Assembly-CSharp.dll。请通过 -p:RimWorldPath=游戏目录 或 RIMWORLD_PATH 指定 RimWorld 1.7 根目录。" />
    <Error Condition="!Exists('$(RimWorldPath)\UnityStubs.dll')"
           Text="未找到 UnityStubs.dll。请通过 -p:RimWorldPath=游戏目录 或 RIMWORLD_PATH 指定 RimWorld 1.7 根目录。" />
  </Target>
</Project>
```

创建 `Source/MyModeTest/MyModeTestMod.cs`：

```csharp
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// myModeTest 的 Mod 入口，确认程序集被 RimWorld 加载。
    /// </summary>
    public sealed class MyModeTestMod : Mod
    {
        public MyModeTestMod(ModContentPack content) : base(content)
        {
            // 流程开始：Mod 构造函数由游戏调用，记录加载日志。
            Log.Message("[myModeTest] Mod loaded.");
        }
    }
}
```

- [ ] **Step 5: 运行 base 验证**

Run:

```bash
python tools/validate_mod.py base
```

Expected: 退出码为 `0`，输出 `base: OK`。

- [ ] **Step 6: 执行 C# 编译验证**

前置条件：.NET 10 SDK 已安装，且 `dotnet` 可在当前 shell 中执行。

Run:

```bash
dotnet build Source/MyModeTest/MyModeTest.csproj
test -f "1.7/Assemblies/MyModeTest.dll"
```

Expected: `dotnet build` 退出码为 `0`，`test -f` 退出码为 `0`，确认生成 `1.7/Assemblies/MyModeTest.dll`。若编译失败，必须先修正工程，不得继续后续任务。

- [ ] **Step 7: 提交基础框架**

```bash
git add About/About.xml LoadFolders.xml .gitignore README.md MyModeTest.sln Source/MyModeTest/MyModeTest.csproj Source/MyModeTest/MyModeTestMod.cs tools/validate_mod.py
git commit -m "feat: 初始化 myModeTest 基础框架" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 2: 玻璃链资源定义与物品贴图

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml`
- Create: `1.7/Textures/Things/Item/MMT/CrushedStone.png`
- Create: `1.7/Textures/Things/Item/MMT/Sand.png`
- Create: `1.7/Textures/Things/Item/MMT/GlassBatch.png`
- Create: `1.7/Textures/Things/Item/MMT/Glass/Glass_a.png`
- Create: `1.7/Textures/Things/Item/MMT/Glass/Glass_b.png`
- Create: `1.7/Textures/Things/Item/MMT/Glass/Glass_c.png`

**Interfaces:**
- Consumes: Task 1 的 `resources` 验证套件。
- Produces: `MMT_CrushedStone`、`MMT_Sand`、`MMT_GlassBatch`、`MMT_Glass`；Task 4 的配方引用这些 defName。

- [ ] **Step 1: 运行 resources 验证，确认失败**

Run:

```bash
python tools/validate_mod.py resources
```

Expected: 退出码为 `1`，输出包含 `[FAIL] resources: 缺少文件:`，并列出 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml`。

- [ ] **Step 2: 创建资源 Def**

创建 `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="ResourceBase">
    <defName>MMT_CrushedStone</defName>
    <label>碎石</label>
    <description>由石块或岩块粉碎得到的细小石料，可进一步加工成砂。</description>
    <graphicData>
      <texPath>Things/Item/MMT/CrushedStone</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Neolithic</techLevel>
    <statBases>
      <MarketValue>0.3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
    </statBases>
    <stackLimit>900</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Sand</defName>
    <label>砂</label>
    <description>经过粉碎和筛选的砂料，是制作玻璃配合料的基础原料。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Sand</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Neolithic</techLevel>
    <statBases>
      <MarketValue>0.3</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
    </statBases>
    <stackLimit>900</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_GlassBatch</defName>
    <label>玻璃配合料</label>
    <description>由砂配制而成的玻璃原料，需要在玻璃工坊中进一步加工。</description>
    <graphicData>
      <texPath>Things/Item/MMT/GlassBatch</texPath>
      <graphicClass>Graphic_Single</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Industrial</techLevel>
    <statBases>
      <MarketValue>1.2</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0.05</Flammability>
    </statBases>
    <stackLimit>300</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>

  <ThingDef ParentName="ResourceBase">
    <defName>MMT_Glass</defName>
    <label>玻璃</label>
    <description>由玻璃配合料加工得到的透明材料，可作为后续生产链的基础资源。</description>
    <graphicData>
      <texPath>Things/Item/MMT/Glass</texPath>
      <graphicClass>Graphic_StackCount</graphicClass>
    </graphicData>
    <tickerType>Rare</tickerType>
    <techLevel>Industrial</techLevel>
    <statBases>
      <MarketValue>5.0</MarketValue>
      <DeteriorationRate>0</DeteriorationRate>
      <Flammability>0</Flammability>
    </statBases>
    <stackLimit>150</stackLimit>
    <thingCategories>
      <li>ResourcesRaw</li>
    </thingCategories>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: 下载已授权的物品贴图**

Run:

```bash
mkdir -p "1.7/Textures/Things/Item/MMT/Glass"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/CrushedStone.png" -o "1.7/Textures/Things/Item/MMT/CrushedStone.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/Sand.png" -o "1.7/Textures/Things/Item/MMT/Sand.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Resource/GlassBatch.png" -o "1.7/Textures/Things/Item/MMT/GlassBatch.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Glass/Glass_a.png" -o "1.7/Textures/Things/Item/MMT/Glass/Glass_a.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Glass/Glass_b.png" -o "1.7/Textures/Things/Item/MMT/Glass/Glass_b.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Item/Glass/Glass_c.png" -o "1.7/Textures/Things/Item/MMT/Glass/Glass_c.png"
```

- [ ] **Step 4: 运行 resources 验证**

Run:

```bash
python tools/validate_mod.py resources
```

Expected: 退出码为 `0`，输出 `resources: OK`。

- [ ] **Step 5: 提交资源定义和贴图**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Glass.xml 1.7/Textures/Things/Item/MMT
git commit -m "feat: 添加玻璃链资源定义" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 3: 玻璃制造研究与玻璃工坊

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml`
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_east.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_north.png`
- Create: `1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_south.png`

**Interfaces:**
- Consumes: Task 1 的 `building` 验证套件。
- Produces: `MMT_Glassworking`、`MMT_GlassworksTable`、`BUILDING_RECIPES = [MMT_MakeGlassBatch, MMT_MakeGlass]`；Task 4 的两个玻璃配方依赖这些名称。

- [ ] **Step 1: 运行 building 验证，确认失败**

Run:

```bash
python tools/validate_mod.py building
```

Expected: 退出码为 `1`，输出包含 `[FAIL] building: 缺少文件:`，并列出 `1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml`。

- [ ] **Step 2: 创建研究 Def**

创建 `1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ResearchProjectDef>
    <defName>MMT_Glassworking</defName>
    <label>玻璃制造</label>
    <description>研究如何将砂配制成玻璃配合料，并进一步加工成玻璃。</description>
    <baseCost>300</baseCost>
    <techLevel>Medieval</techLevel>
    <prerequisites>
      <li>Stonecutting</li>
    </prerequisites>
    <researchViewX>4.00</researchViewX>
    <researchViewY>0.00</researchViewY>
  </ResearchProjectDef>
</Defs>
```

- [ ] **Step 3: 创建玻璃工坊建筑 Def**

创建 `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="BuildingBase">
    <defName>MMT_GlassworksTable</defName>
    <label>玻璃工坊</label>
    <description>配有玻璃加工工具的电力工作台，用于制作玻璃配合料和玻璃。</description>
    <thingClass>Building_WorkTable</thingClass>
    <graphicData>
      <texPath>Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable</texPath>
      <graphicClass>Graphic_Multi</graphicClass>
      <shaderType>CutoutComplex</shaderType>
      <drawSize>(3,3)</drawSize>
    </graphicData>
    <passability>PassThroughOnly</passability>
    <pathCost>80</pathCost>
    <fillPercent>0.6</fillPercent>
    <surfaceType>Item</surfaceType>
    <canOverlapZones>false</canOverlapZones>
    <hasInteractionCell>true</hasInteractionCell>
    <interactionCellOffset>(0,0,-1)</interactionCellOffset>
    <useStuffTerrainAffordance>true</useStuffTerrainAffordance>
    <stuffCategories>
      <li>Metallic</li>
    </stuffCategories>
    <costStuffCount>130</costStuffCount>
    <costList>
      <ComponentIndustrial>10</ComponentIndustrial>
    </costList>
    <minifiedDef>MinifiedThing</minifiedDef>
    <statBases>
      <WorkToBuild>1600</WorkToBuild>
      <MaxHitPoints>180</MaxHitPoints>
      <Flammability>1.0</Flammability>
      <Cleanliness>-6</Cleanliness>
      <Mass>500</Mass>
    </statBases>
    <size>(3,1)</size>
    <designationCategory>Production</designationCategory>
    <recipes>
      <li>MMT_MakeGlassBatch</li>
      <li>MMT_MakeGlass</li>
    </recipes>
    <tickerType>Normal</tickerType>
    <comps>
      <li Class="CompProperties_Power">
        <compClass>CompPowerTrader</compClass>
        <basePowerConsumption>400</basePowerConsumption>
        <shortCircuitInRain>true</shortCircuitInRain>
      </li>
      <li Class="CompProperties_Flickable" />
      <li Class="CompProperties_Breakdownable" />
      <li Class="CompProperties_Glower">
        <glowRadius>2</glowRadius>
        <glowColor>(217,112,33,0)</glowColor>
      </li>
      <li Class="CompProperties_HeatPusher">
        <compClass>CompHeatPusherPowered</compClass>
        <heatPerSecond>4</heatPerSecond>
        <heatPushMaxTemperature>45</heatPushMaxTemperature>
      </li>
    </comps>
    <building>
      <uninstallWork>1450</uninstallWork>
      <heatPerTickWhileWorking>0.9</heatPerTickWhileWorking>
    </building>
    <inspectorTabs>
      <li>ITab_Bills</li>
    </inspectorTabs>
    <placeWorkers>
      <li>PlaceWorker_PreventInteractionSpotOverlap</li>
    </placeWorkers>
    <researchPrerequisites>
      <li>MMT_Glassworking</li>
    </researchPrerequisites>
    <constructionSkillPrerequisite>9</constructionSkillPrerequisite>
  </ThingDef>
</Defs>
```

- [ ] **Step 4: 下载已授权的玻璃工坊静态贴图**

Run:

```bash
mkdir -p "1.7/Textures/Things/Building/Production/MMT_GlassworksTable"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_off_east.png" -o "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_east.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_off_north.png" -o "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_north.png"
curl -fsSL "https://raw.githubusercontent.com/HardcoreSK/Hardcore-SK/0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac/Mods/Core_SK/Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_south.png" -o "1.7/Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_south.png"
```

- [ ] **Step 5: 运行 building 验证**

Run:

```bash
python tools/validate_mod.py building
```

Expected: 退出码为 `0`，输出 `building: OK`。

- [ ] **Step 6: 提交研究和建筑**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Glass.xml 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Glassworks.xml 1.7/Textures/Things/Building/Production/MMT_GlassworksTable
git commit -m "feat: 添加玻璃工坊与研究" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

---

### Task 4: 玻璃生产配方与完整静态验证

**Files:**
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml`

**Interfaces:**
- Consumes: Task 2 的 `MMT_CrushedStone`、`MMT_Sand`、`MMT_GlassBatch`、`MMT_Glass`；Task 3 的 `MMT_Glassworking`、`MMT_GlassworksTable`。
- Produces: 完整的 10 个配方 defName，均与 `tools/validate_mod.py` 中 `RECIPE_EXPECTATIONS` 的键一致。

- [ ] **Step 1: 运行 recipes 验证，确认失败**

Run:

```bash
python tools/validate_mod.py recipes
```

Expected: 退出码为 `1`，输出包含 `[FAIL] recipes: 缺少文件:`，并列出 `1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml`。

- [ ] **Step 2: 创建配方 Def**

创建 `1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <RecipeDef>
    <defName>MMT_MakeCrushedStone_Hand</defName>
    <label>粉碎石块为碎石</label>
    <description>将石块粉碎为碎石。产出 20。</description>
    <jobString>正在粉碎石块。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableStonecutter</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneBlocks</li>
          </categories>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneBlocks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_CrushedStone>20</MMT_CrushedStone>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeCrushedStone_Electric</defName>
    <label>批量粉碎石块为碎石</label>
    <description>使用机械加工台批量粉碎石块。产出 40。</description>
    <jobString>正在批量粉碎石块。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableMachining</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneBlocks</li>
          </categories>
        </filter>
        <count>40</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneBlocks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_CrushedStone>40</MMT_CrushedStone>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeCrushedStone_Hand_Chunk</defName>
    <label>粉碎岩块为碎石</label>
    <description>将一整块岩块粉碎为碎石。产出 32。</description>
    <jobString>正在粉碎岩块。</jobString>
    <workAmount>2400</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableStonecutter</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneChunks</li>
          </categories>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneChunks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_CrushedStone>32</MMT_CrushedStone>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeCrushedStone_Electric_Chunk</defName>
    <label>快速粉碎岩块为碎石</label>
    <description>使用机械加工台快速粉碎岩块。产出 32。</description>
    <jobString>正在快速粉碎岩块。</jobString>
    <workAmount>1200</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableMachining</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneChunks</li>
          </categories>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneChunks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_CrushedStone>32</MMT_CrushedStone>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeSand_Hand</defName>
    <label>研磨碎石制砂</label>
    <description>将碎石研磨成砂。产出 25。</description>
    <jobString>正在研磨碎石制砂。</jobString>
    <workAmount>600</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableStonecutter</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_CrushedStone</li>
          </thingDefs>
        </filter>
        <count>10</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_CrushedStone</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Sand>25</MMT_Sand>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>Stonecutting</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeSand_Electric</defName>
    <label>批量研磨碎石制砂</label>
    <description>使用机械加工台批量研磨碎石。产出 50。</description>
    <jobString>正在批量研磨碎石。</jobString>
    <workAmount>600</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableMachining</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_CrushedStone</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_CrushedStone</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Sand>50</MMT_Sand>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeSand_Hand_Chunk</defName>
    <label>粉碎岩块制砂</label>
    <description>将一整块岩块直接粉碎成砂。产出 80。</description>
    <jobString>正在粉碎岩块制砂。</jobString>
    <workAmount>3000</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableStonecutter</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneChunks</li>
          </categories>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneChunks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_Sand>80</MMT_Sand>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>Stonecutting</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeSand_Electric_Chunk</defName>
    <label>快速粉碎岩块制砂</label>
    <description>使用机械加工台将岩块直接粉碎成砂。产出 80。</description>
    <jobString>正在快速粉碎岩块制砂。</jobString>
    <workAmount>1500</workAmount>
    <workSpeedStat>StonecuttingSpeed</workSpeedStat>
    <effectWorking>CutStone</effectWorking>
    <soundWorking>Recipe_MakeStoneBlocks</soundWorking>
    <recipeUsers>
      <li>TableMachining</li>
    </recipeUsers>
    <ingredients>
      <li>
        <filter>
          <categories>
            <li>StoneChunks</li>
          </categories>
        </filter>
        <count>1</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <categories>
        <li>StoneChunks</li>
      </categories>
    </fixedIngredientFilter>
    <products>
      <MMT_Sand>80</MMT_Sand>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeGlassBatch</defName>
    <label>制作玻璃配合料</label>
    <description>将砂配制成玻璃配合料。产出 25。</description>
    <jobString>正在制作玻璃配合料。</jobString>
    <workAmount>900</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_Sand</li>
          </thingDefs>
        </filter>
        <count>75</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Sand</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_GlassBatch>25</MMT_GlassBatch>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_Glassworking</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeGlass</defName>
    <label>加工玻璃</label>
    <description>将玻璃配合料加工成玻璃。产出 10。</description>
    <jobString>正在加工玻璃。</jobString>
    <workAmount>600</workAmount>
    <workSpeedStat>SmeltingSpeed</workSpeedStat>
    <effectWorking>Smelt</effectWorking>
    <soundWorking>Recipe_Smelt</soundWorking>
    <ingredients>
      <li>
        <filter>
          <thingDefs>
            <li>MMT_GlassBatch</li>
          </thingDefs>
        </filter>
        <count>20</count>
      </li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_GlassBatch</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_Glass>10</MMT_Glass>
    </products>
    <workSkill>Crafting</workSkill>
    <workSkillLearnFactor>0.8</workSkillLearnFactor>
    <researchPrerequisite>MMT_Glassworking</researchPrerequisite>
  </RecipeDef>
</Defs>
```

- [ ] **Step 3: 运行 recipes 验证**

Run:

```bash
python tools/validate_mod.py recipes
```

Expected: 退出码为 `0`，输出 `recipes: OK`。

- [ ] **Step 4: 运行完整静态验证**

Run:

```bash
python tools/validate_mod.py all
```

Expected: 退出码为 `0`，依次输出：

```text
base: OK
resources: OK
building: OK
recipes: OK
all-xml: OK (6 files)
```

- [ ] **Step 5: 检查 Git 工作区并提交配方**

Run:

```bash
git status --short
```

Expected: 只显示 `1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml` 为新增文件。

```bash
git add 1.7/Defs/RecipeDefs/MMT_Recipes_Glass.xml
git commit -m "feat: 添加玻璃生产配方" -m "Co-Authored-By: Claude Code <noreply@anthropic.com>"
```

- [ ] **Step 6: 验证最终 Git 状态**

Run:

```bash
git status --short
git log --oneline --max-count=5
```

Expected: `git status --short` 无输出；日志中依次包含：

```text
feat: 添加玻璃生产配方
feat: 添加玻璃工坊与研究
feat: 添加玻璃链资源定义
feat: 初始化 myModeTest 基础框架
docs: 添加玻璃资源生产链设计
```
