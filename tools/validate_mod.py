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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
        "speed": "GeneralLaborSpeed",
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
    # 校验 PNG 魔数、IHDR 块类型以及宽高均大于 0
    data = (ROOT / path).read_bytes()
    if len(data) <= len(PNG_SIGNATURE) or not data.startswith(PNG_SIGNATURE):
        fail(f"{path}: 不是有效的 PNG 文件")
    # IHDR 块紧随 8 字节魔数：4 字节长度 + 4 字节类型 + 4 字节宽 + 4 字节高
    ihdr_end = len(PNG_SIGNATURE) + 8 + 8
    if len(data) < ihdr_end:
        fail(f"{path}: PNG 缺少完整的 IHDR 块")
    if data[len(PNG_SIGNATURE) + 4 : len(PNG_SIGNATURE) + 8] != b"IHDR":
        fail(f"{path}: PNG 首个块不是 IHDR")
    width = int.from_bytes(data[len(PNG_SIGNATURE) + 8 : len(PNG_SIGNATURE) + 12], "big")
    height = int.from_bytes(data[len(PNG_SIGNATURE) + 12 : len(PNG_SIGNATURE) + 16], "big")
    if width <= 0 or height <= 0:
        fail(f"{path}: PNG 宽高必须大于 0，实际 {width}x{height}")


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
        # label 必填，child_text 会处理缺失/空值失败
        child_text(recipe, "label")
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
