# myModeTest 基础零件家族（Parts）链 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增 6 种自造零件资源、2 种原版零件（工业/高级零件）的生产配方、3 座装配建筑、3 级研究、14 个配方，完整实现 HSK 基础零件家族生产链。

**Architecture:** 新增 4 个 XML（资源/建筑/研究/配方）+ 19 张 HSK 复制贴图；修改 `tools/validate_mod.py`（新套件 `parts` + `VANILLA_THINGS` 加 `ComponentAdvanced`）。配方全部由建筑 `<recipes>` 列表挂载，不写 `recipeUsers`。

**Tech Stack:** RimWorld 1.7 XML Def、Python 3 静态验证脚本、.NET 10 C# 项目、Git。

**Spec:** `docs/superpowers/specs/2026-10-19-myModeTest-parts-production-design.md`

## Global Constraints

- Mod 显示名 `myModeTest`；自定义 Def 全部 `MMT_` 前缀；中文 label/description，英文 defName。
- 不引用 `SK.` 类型与 HSK 专属 Def/分类；stuff 分类仅可用 `Metallic`（禁 SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic）。
- 配方 workSpeedStat 用 `GeneralLaborSpeed`（禁 `SmithingSpeed`）；effectWorking `Smith`；soundWorking `Recipe_Machining`（对标 `MMT_AssembleElectronicParts_Hand`）。
- HSK 贴图来源固定为本地克隆 `D:\project\rw\Hardcore-SK`，文件复制（含蒙版 `*m.png`）。
- 所有文本文件 UTF-8；git 提交信息结尾带 `Co-Authored-By: Claude Code <noreply@anthropic.com>`。
- 最终必须执行 `PYTHONIOENCODING=utf-8 python tools/validate_mod.py all` 和 `"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`。
- 本次不启动 RimWorld，不声称游戏内加载通过；只提交本地 Git，不 push。

## Review Focus

1. **手动装配台自举死锁**：HSK 原成本（原始零件×10+机构件×5）在本 Mod 无法自举（机构件只能在装配台制作），已修正为 `WoodLog`×40 + `Steel`×25；Task 1 对 costList 逐项断言，防回退。
2. **`ComponentAdvanced` 是原版 Def**：`MMT_MakeComponentAdvanced_*` 的输出不在任何 MMT 资源闭包内，`VANILLA_THINGS` 必须新增 `ComponentAdvanced`（Task 1 Step 2），否则输出闭包检查误报。
3. **原始零件两配方无技能要求**：其余 12 个配方均有 Crafting 门槛，Task 1 对 `skill: None` 的配方言明 `skillRequirements` 必须缺席，防误加。
4. **跨链研究前置**：`MMT_UltraComponents` 前置含高级合金链的 `MMT_AdvancedAlloys`；两级/三级研究前置列表各不相同，Task 1 逐项断言，不统一断言。
5. **挂载集合不统一**：仅 2 个原始零件配方同时挂两座装配台，太空零件只挂电动台，超级零件只挂机器人台；Task 1 用每配方 `benches` 断言挂载集合，防错挂/漏挂。

---

### Task 1: 新增 parts 静态验证套件（RED）

**Files:**
- Modify: `tools/validate_mod.py`

**Interfaces:**
- Consumes: 现有公共函数（`require_files`/`parse_xml`/`def_map`/`child_text`/`li_values`/`assert_equal`/`assert_png`/`read_text`/`fail`）与常量 `METAL_RESOURCE_XML`/`PETROCHEM_RESOURCE_XML`/`ELECTRONICS_RESOURCE_XML`/`ADVANCED_ALLOY_RESOURCE_XML`/`VANILLA_THINGS`。
- Produces: 常量 `PARTS_RESOURCE_XML`/`PARTS_BUILDING_XML`/`PARTS_RECIPE_XML`/`PARTS_RESEARCH_XML`/`PARTS_ITEM_TEXTURES`/`PARTS_BUILDING_TEXTURES`/`PARTS_RESOURCE_EXPECTATIONS`/`PARTS_RESEARCH_EXPECTATIONS`/`PARTS_HAND_BENCH_RECIPES`/`PARTS_ELECTRIC_BENCH_RECIPES`/`PARTS_BUILDING_EXPECTATIONS`/`PARTS_RECIPE_EXPECTATIONS`；函数 `validate_parts()`；套件名 `parts`。

- [ ] **Step 1: 新增常量（放在 `ADVANCED_ALLOYS_RECIPE_EXPECTATIONS` 之后）**

```python
PARTS_RESOURCE_XML = "1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Parts.xml"
PARTS_BUILDING_XML = "1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml"
PARTS_RECIPE_XML = "1.7/Defs/RecipeDefs/MMT_Recipes_Parts.xml"
PARTS_RESEARCH_XML = "1.7/Defs/ResearchProjectDefs/MMT_Research_Parts.xml"

PARTS_ITEM_TEXTURES = [
    f"1.7/Textures/Things/Item/MMT/MMT_Parts/{name}.png"
    for name in ("ComponentMedieval", "Wires", "Mechanism", "AdvMechanism", "AdvancedComponent", "ComponentUltra")
]

PARTS_BUILDING_TEXTURES = [
    *[
        f"1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
    *[
        f"1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench_{side}{mask}.png"
        for side in ("east", "north", "south")
        for mask in ("", "m")
    ],
    "1.7/Textures/Things/Building/Production/MMT_RoboticAssembler/MMT_RoboticAssembler.png",
]

PARTS_RESOURCE_EXPECTATIONS = {
    "MMT_ComponentMedieval": {"label": "原始零件", "value": "6", "mass": "1.5", "stack": "300", "tech": "Neolithic", "flam": "0.2", "tex": "Things/Item/MMT/MMT_Parts/ComponentMedieval"},
    "MMT_Wire": {"label": "导线", "value": "1.5", "mass": "0.2", "stack": "600", "tech": "Industrial", "flam": "0.1", "tex": "Things/Item/MMT/MMT_Parts/Wires"},
    "MMT_Mechanism": {"label": "机构件", "value": "200", "mass": "0.65", "stack": "75", "tech": "Industrial", "flam": "1", "tex": "Things/Item/MMT/MMT_Parts/Mechanism"},
    "MMT_AdvMechanism": {"label": "高级机构件", "value": "400", "mass": "0.7", "stack": "75", "tech": "Spacer", "flam": "1", "tex": "Things/Item/MMT/MMT_Parts/AdvMechanism"},
    "MMT_SpacerComponent": {"label": "太空零件", "value": "150", "mass": "0.5", "stack": "150", "tech": "Spacer", "flam": "0.3", "tex": "Things/Item/MMT/MMT_Parts/AdvancedComponent"},
    "MMT_UltraComponent": {"label": "超级零件", "value": "200", "mass": "0.5", "stack": "150", "tech": "Ultra", "flam": "0.3", "tex": "Things/Item/MMT/MMT_Parts/ComponentUltra"},
}

PARTS_RESEARCH_EXPECTATIONS = {
    "MMT_ComponentCrafting": {"label": "零件加工", "cost": "600", "tech": "Industrial", "prereqs": [], "x": "15.00", "y": "0.00"},
    "MMT_AdvancedComponents": {"label": "高级零件", "cost": "1600", "tech": "Spacer", "prereqs": ["MMT_ComponentCrafting"], "x": "15.00", "y": "1.00"},
    "MMT_UltraComponents": {"label": "超级零件", "cost": "2500", "tech": "Spacer", "prereqs": ["MMT_AdvancedComponents", "MMT_AdvancedAlloys"], "x": "15.00", "y": "2.00"},
}

PARTS_HAND_BENCH_RECIPES = [
    "MMT_MakeComponentMedieval_Wood",
    "MMT_MakeComponentMedieval_Metal",
    "MMT_MakeComponentIndustrial_Hand",
    "MMT_MakeWire_Hand",
    "MMT_MakeMechanism_Hand",
    "MMT_MakeComponentAdvanced_Hand",
    "MMT_MakeAdvMechanism_Hand",
]

PARTS_ELECTRIC_BENCH_RECIPES = [
    "MMT_MakeComponentMedieval_Wood",
    "MMT_MakeComponentMedieval_Metal",
    "MMT_MakeComponentIndustrial_Electric",
    "MMT_MakeWire_Electric",
    "MMT_MakeMechanism_Electric",
    "MMT_MakeComponentAdvanced_Electric",
    "MMT_MakeAdvMechanism_Electric",
    "MMT_MakeSpacerComponent",
]

PARTS_BUILDING_EXPECTATIONS = {
    "MMT_HandAssemblyBench": {
        "label": "装配台", "tex": "Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench", "graphic": "Graphic_Multi",
        "stuff_count": "125", "costs": {"WoodLog": "40", "Steel": "25"},
        "work_build": "1200", "hp": "180", "size": "(3,1)", "speed": "0.9", "power": None,
        "research": ["MMT_ComponentCrafting"], "recipes": PARTS_HAND_BENCH_RECIPES, "construction_skill": "4",
    },
    "MMT_ElectricAssemblyBench": {
        "label": "电动装配台", "tex": "Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench", "graphic": "Graphic_Multi",
        "stuff_count": "220", "costs": {"MMT_SyntheticFibers": "45", "MMT_IntegratedCircuit": "10", "ComponentIndustrial": "15", "MMT_Mechanism": "10"},
        "work_build": "5500", "hp": "210", "size": "(3,1)", "speed": "1.5", "power": "750",
        "research": ["MMT_ComponentCrafting", "MMT_AdvancedComponents"], "recipes": PARTS_ELECTRIC_BENCH_RECIPES, "construction_skill": "8",
    },
    "MMT_RoboticAssembler": {
        "label": "机器人装配台", "tex": "Things/Building/Production/MMT_RoboticAssembler/MMT_RoboticAssembler", "graphic": "Graphic_Single",
        "stuff_count": "120", "costs": {"MMT_SyntheticFibers": "45", "MMT_SpacerComponent": "10", "MMT_AdvMechanism": "10", "MMT_Microchip": "5"},
        "work_build": "9000", "hp": "200", "size": "(1,1)", "speed": None, "power": "5000",
        "research": ["MMT_UltraComponents"], "recipes": ["MMT_MakeUltraComponent"], "construction_skill": "10",
    },
}

PARTS_RECIPE_EXPECTATIONS = {
    "MMT_MakeComponentMedieval_Wood": {"work": "900", "inputs": [("thingDefs", "WoodLog", "20")], "output": "MMT_ComponentMedieval", "output_count": "10", "skill": None, "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench", "MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentMedieval_Metal": {"work": "900", "inputs": [("thingDefs", ("Steel", "MMT_CopperBar"), "10")], "output": "MMT_ComponentMedieval", "output_count": "10", "skill": None, "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench", "MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Hand": {"work": "2000", "inputs": [("thingDefs", ("Steel", "Plasteel"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentIndustrial_Electric": {"work": "2000", "inputs": [("thingDefs", ("Steel", "Plasteel"), "30")], "output": "ComponentIndustrial", "output_count": "10", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeWire_Hand": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "10")], "output": "MMT_Wire", "output_count": "20", "skill": "3", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeWire_Electric": {"work": "800", "inputs": [("thingDefs", "MMT_CopperBar", "20")], "output": "MMT_Wire", "output_count": "40", "skill": "6", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeMechanism_Hand": {"work": "1800", "inputs": [("thingDefs", "Steel", "30"), ("thingDefs", "ComponentIndustrial", "10")], "output": "MMT_Mechanism", "output_count": "5", "skill": "4", "research": "MMT_ComponentCrafting", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeMechanism_Electric": {"work": "1800", "inputs": [("thingDefs", "Steel", "60"), ("thingDefs", "ComponentIndustrial", "20")], "output": "MMT_Mechanism", "output_count": "10", "skill": "7", "research": "MMT_ComponentCrafting", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeComponentAdvanced_Hand": {"work": "2500", "inputs": [("thingDefs", "Plasteel", "30")], "output": "ComponentAdvanced", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeComponentAdvanced_Electric": {"work": "2500", "inputs": [("thingDefs", "Plasteel", "30")], "output": "ComponentAdvanced", "output_count": "10", "skill": "6", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeAdvMechanism_Hand": {"work": "3200", "inputs": [("thingDefs", "Plasteel", "30"), ("thingDefs", "ComponentAdvanced", "10"), ("thingDefs", "MMT_SyntheticFibers", "5")], "output": "MMT_AdvMechanism", "output_count": "5", "skill": "13", "research": "MMT_AdvancedComponents", "benches": ["MMT_HandAssemblyBench"]},
    "MMT_MakeAdvMechanism_Electric": {"work": "3200", "inputs": [("thingDefs", "Plasteel", "60"), ("thingDefs", "ComponentAdvanced", "20"), ("thingDefs", "MMT_SyntheticFibers", "10")], "output": "MMT_AdvMechanism", "output_count": "10", "skill": "16", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeSpacerComponent": {"work": "3500", "inputs": [("thingDefs", "Plasteel", "25"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_SpacerComponent", "output_count": "10", "skill": "9", "research": "MMT_AdvancedComponents", "benches": ["MMT_ElectricAssemblyBench"]},
    "MMT_MakeUltraComponent": {"work": "4500", "inputs": [("thingDefs", ("MMT_Tungsten", "MMT_Titanium"), "15"), ("thingDefs", ("MMT_StelliteAlloy", "MMT_PobediteAlloy"), "10"), ("thingDefs", ("Silver", "Gold"), "5")], "output": "MMT_UltraComponent", "output_count": "10", "skill": "13", "research": "MMT_UltraComponents", "benches": ["MMT_RoboticAssembler"]},
}
```

- [ ] **Step 2: 修改既有常量**

`VANILLA_THINGS` 改为：

```python
VANILLA_THINGS = {"ComponentIndustrial", "ComponentAdvanced", "WoodLog", "Steel", "Silver", "Gold", "Plasteel"}
```

- [ ] **Step 3: 实现 `validate_parts()`（放在 `validate_advanced_alloys()` 之后）**

```python
def validate_parts() -> None:
    require_files([
        PARTS_RESOURCE_XML,
        PARTS_BUILDING_XML,
        PARTS_RECIPE_XML,
        PARTS_RESEARCH_XML,
        METAL_RESOURCE_XML,
        PETROCHEM_RESOURCE_XML,
        ELECTRONICS_RESOURCE_XML,
        ADVANCED_ALLOY_RESOURCE_XML,
        *PARTS_ITEM_TEXTURES,
        *PARTS_BUILDING_TEXTURES,
    ])

    research_defs = def_map(parse_xml(PARTS_RESEARCH_XML))
    assert_equal(set(research_defs), set(PARTS_RESEARCH_EXPECTATIONS), "零件研究 defName 集合")
    for name, expected in PARTS_RESEARCH_EXPECTATIONS.items():
        node = research_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "baseCost"), expected["cost"], f"{name} baseCost")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(li_values(node, "prerequisites"), expected["prereqs"], f"{name} 前置")
        assert_equal(child_text(node, "researchViewX"), expected["x"], f"{name} X 坐标")
        assert_equal(child_text(node, "researchViewY"), expected["y"], f"{name} Y 坐标")

    resource_defs = def_map(parse_xml(PARTS_RESOURCE_XML))
    assert_equal(set(resource_defs), set(PARTS_RESOURCE_EXPECTATIONS), "零件资源 defName 集合")
    for name, expected in PARTS_RESOURCE_EXPECTATIONS.items():
        node = resource_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "./statBases/MarketValue"), expected["value"], f"{name} MarketValue")
        assert_equal(child_text(node, "./statBases/Mass"), expected["mass"], f"{name} Mass")
        assert_equal(child_text(node, "./statBases/Flammability"), expected["flam"], f"{name} Flammability")
        assert_equal(child_text(node, "stackLimit"), expected["stack"], f"{name} stackLimit")
        assert_equal(child_text(node, "techLevel"), expected["tech"], f"{name} techLevel")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), "Graphic_Single", f"{name} graphicClass")
        assert_equal(li_values(node, "thingCategories"), ["ResourcesRaw"], f"{name} thingCategories")
        if node.find("stuffProps") is not None:
            fail(f"{name} 不应定义 stuffProps")

    building_defs = def_map(parse_xml(PARTS_BUILDING_XML))
    assert_equal(set(building_defs), set(PARTS_BUILDING_EXPECTATIONS), "零件建筑 defName 集合")
    for name, expected in PARTS_BUILDING_EXPECTATIONS.items():
        node = building_defs[name]
        assert_equal(child_text(node, "label"), expected["label"], f"{name} label")
        assert_equal(child_text(node, "thingClass"), "Building_WorkTable", f"{name} thingClass")
        assert_equal(child_text(node, "./graphicData/texPath"), expected["tex"], f"{name} texPath")
        assert_equal(child_text(node, "./graphicData/graphicClass"), expected["graphic"], f"{name} graphicClass")
        assert_equal(li_values(node, "stuffCategories"), ["Metallic"], f"{name} 材料分类")
        assert_equal(child_text(node, "costStuffCount"), expected["stuff_count"], f"{name} 材料数量")
        cost_list = {child.tag: (child.text or "").strip() for child in node.findall("./costList/*")}
        assert_equal(cost_list, expected["costs"], f"{name} 成本列表")
        assert_equal(child_text(node, "./statBases/WorkToBuild"), expected["work_build"], f"{name} 建造工作量")
        assert_equal(child_text(node, "./statBases/MaxHitPoints"), expected["hp"], f"{name} 耐久")
        assert_equal(child_text(node, "size"), expected["size"], f"{name} 尺寸")
        speed = node.findtext("./statBases/WorkTableWorkSpeedFactor")
        speed = speed.strip() if speed else None
        assert_equal(speed, expected["speed"], f"{name} 工作台速度因子")
        assert_equal(li_values(node, "recipes"), expected["recipes"], f"{name} 配方列表")
        assert_equal(li_values(node, "researchPrerequisites"), expected["research"], f"{name} 研究前置")
        assert_equal(child_text(node, "constructionSkillPrerequisite"), expected["construction_skill"], f"{name} 建造技能")
        comp_classes = [li.attrib.get("Class") for li in node.findall("./comps/li")]
        if expected["power"] is None:
            if "CompProperties_Power" in comp_classes:
                fail(f"{name} 不应包含电力组件")
        else:
            if "CompProperties_Power" not in comp_classes:
                fail(f"{name} 缺少电力组件")
            power_comp = next(li for li in node.findall("./comps/li") if li.attrib.get("Class") == "CompProperties_Power")
            assert_equal(child_text(power_comp, "basePowerConsumption"), expected["power"], f"{name} 功耗")

    recipe_defs = def_map(parse_xml(PARTS_RECIPE_XML))
    assert_equal(set(recipe_defs), set(PARTS_RECIPE_EXPECTATIONS), "零件配方 defName 集合")

    resource_names = (
        set(resource_defs)
        | set(def_map(parse_xml(METAL_RESOURCE_XML)))
        | set(def_map(parse_xml(PETROCHEM_RESOURCE_XML)))
        | set(def_map(parse_xml(ELECTRONICS_RESOURCE_XML)))
        | set(def_map(parse_xml(ADVANCED_ALLOY_RESOURCE_XML)))
    )
    output_names = set(resource_defs) | VANILLA_THINGS
    building_recipes = {name: li_values(node, "recipes") for name, node in building_defs.items()}

    for name, expected in PARTS_RECIPE_EXPECTATIONS.items():
        recipe = recipe_defs[name]
        child_text(recipe, "label")
        assert_equal(child_text(recipe, "workAmount"), expected["work"], f"{name} workAmount")
        assert_equal(child_text(recipe, "workSpeedStat"), "GeneralLaborSpeed", f"{name} workSpeedStat")
        assert_equal(child_text(recipe, "effectWorking"), "Smith", f"{name} effectWorking")
        assert_equal(child_text(recipe, "soundWorking"), "Recipe_Machining", f"{name} soundWorking")
        assert_equal(child_text(recipe, "workSkill"), "Crafting", f"{name} workSkill")
        assert_equal(child_text(recipe, "workSkillLearnFactor"), "0.8", f"{name} workSkillLearnFactor")
        if expected["skill"] is None:
            if recipe.find("skillRequirements") is not None:
                fail(f"{name} 不应有技能要求")
        else:
            assert_equal(child_text(recipe, "./skillRequirements/Crafting"), expected["skill"], f"{name} Crafting requirement")

        actual_inputs = []
        for ingredient in recipe.findall("./ingredients/li"):
            filter_node = ingredient.find("filter")
            if filter_node is None:
                fail(f"{name} 存在缺少 filter 的输入")
            thing_defs = li_values(filter_node, "thingDefs")
            if not thing_defs or li_values(filter_node, "categories"):
                fail(f"{name} 每个输入 filter 必须只包含 thingDefs 引用")
            actual_inputs.append(("thingDefs", tuple(thing_defs), child_text(ingredient, "count")))
        normalized = [
            (kind, (value,) if isinstance(value, str) else tuple(value), count)
            for kind, value, count in expected["inputs"]
        ]
        assert_equal(actual_inputs, normalized, f"{name} 输入列表")

        fixed = sorted(li_values(recipe, "./fixedIngredientFilter/thingDefs"))
        expected_fixed = sorted({item for _, value, _ in normalized for item in value})
        assert_equal(fixed, expected_fixed, f"{name} 固定输入列表")

        products = [node for node in recipe.findall("./products/*") if isinstance(node.tag, str)]
        if len(products) != 1:
            fail(f"{name} 必须且只能有一个输出")
        assert_equal(products[0].tag, expected["output"], f"{name} 输出")
        assert_equal((products[0].text or "").strip(), expected["output_count"], f"{name} 输出数量")

        if li_values(recipe, "recipeUsers"):
            fail(f"{name} 应由建筑 recipes 列表挂载，不应写 recipeUsers")
        actual_benches = sorted(bench for bench, recipes in building_recipes.items() if name in recipes)
        assert_equal(actual_benches, sorted(expected["benches"]), f"{name} 挂载工作台")

        research = recipe.findtext("researchPrerequisite")
        research = research.strip() if research else None
        assert_equal(research, expected["research"], f"{name} researchPrerequisite")

        for _, value, _ in normalized:
            for item in value:
                if item not in resource_names | VANILLA_THINGS:
                    fail(f"{name} 使用未知资源: {item}")
        if expected["output"] not in output_names:
            fail(f"{name} 输出不是零件资源: {expected['output']}")

    for path in PARTS_ITEM_TEXTURES + PARTS_BUILDING_TEXTURES:
        assert_png(path)

    for path in (PARTS_RESOURCE_XML, PARTS_BUILDING_XML, PARTS_RECIPE_XML, PARTS_RESEARCH_XML):
        if "SK." in read_text(path):
            fail(f"{path} 不得引用 SK 类型")
    for path in (PARTS_RESOURCE_XML, PARTS_BUILDING_XML):
        text = read_text(path)
        for forbidden in ("SolidMetallic", "StrongMetallic", "RuggedMetallic", "HeavyMetallic"):
            if forbidden in text:
                fail(f"{path} 不得使用 RimWorld 1.7 不存在的 {forbidden} 分类")
    if "SmithingSpeed" in read_text(PARTS_RECIPE_XML):
        fail("零件配方不得使用 RimWorld 1.7 不存在的 SmithingSpeed")
    print("parts: OK")
```

- [ ] **Step 4: 注册套件**

`validate_all()` 在 `validate_advanced_alloys()` 之后追加 `validate_parts()`；`SUITES` 在 `"advanced-alloys"` 之后追加 `"parts": validate_parts`。

- [ ] **Step 5: 运行验证器确认 RED**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py parts`
预期：失败，提示缺少 4 个 XML 文件。

- [ ] **Step 6: 提交**

```bash
git add tools/validate_mod.py
git commit -m "test: 添加零件链静态验证"
```

---

### Task 2: 零件资源与物品贴图（GREEN 第一部分）

**Files:**
- Create: `1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Parts.xml`
- Create: `1.7/Textures/Things/Item/MMT/MMT_Parts/` 新增 6 张

**Interfaces:**
- Produces: 后续任务引用的资源 defName（`MMT_ComponentMedieval`/`MMT_Wire`/`MMT_Mechanism`/`MMT_AdvMechanism`/`MMT_SpacerComponent`/`MMT_UltraComponent`）。

- [ ] **Step 1: 复制 6 张物品贴图**

```bash
mkdir -p "1.7/Textures/Things/Item/MMT/MMT_Parts"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/ComponentMedieval.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/ComponentMedieval.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/Wires.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/Wires.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/Mechanism.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/Mechanism.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/AdvMechanism.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/AdvMechanism.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/AdvancedComponent.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/AdvancedComponent.png"
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Item/Components/ComponentUltra.png" "1.7/Textures/Things/Item/MMT/MMT_Parts/ComponentUltra.png"
```

- [ ] **Step 2: 编写资源 XML**

`MMT_Items_Resource_Parts.xml` 含 6 个 ThingDef，结构参照 `MMT_Items_Resource_Electronics.xml` 的 `MMT_ElectronicParts`（`ResourceBase` + `Graphic_Single` + `tickerType Rare` + DeteriorationRate 0 + `ResourcesRaw`，不写 stuffProps/useHitPoints）。要点：

- defName/label/MarketValue/Mass/Flammability/stackLimit/techLevel/texPath 以 Task 1 `PARTS_RESOURCE_EXPECTATIONS` 为准；
- 音效：原始零件 `Wood_Drop`/`Wood_Drop`，其余 5 种 `Metal_Drop`/`Metal_Drop`（照抄 HSK）；
- description 示例（原始零件）：`各种简易的木石零件，可用于制作原始工具和搭建装配台。`；（机构件）：`齿轮状的简单机构件，是组装机械设备的基础部件。`

- [ ] **Step 3: 提交**

```bash
git add 1.7/Defs/ThingDefs_Resources/MMT_Items_Resource_Parts.xml 1.7/Textures/Things/Item/MMT/MMT_Parts/
git commit -m "feat: 添加基础零件资源与贴图"
```

---

### Task 3: 三座装配建筑与建筑贴图（GREEN 第二部分）

**Files:**
- Create: `1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml`
- Create: `1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/`、`MMT_ElectricAssemblyBench/`、`MMT_RoboticAssembler/` 共 13 张

**Interfaces:**
- Consumes: Task 2 的 `MMT_Mechanism`/`MMT_SpacerComponent`/`MMT_AdvMechanism`；既有 `MMT_SyntheticFibers`/`MMT_IntegratedCircuit`/`MMT_Microchip`；原版 `WoodLog`/`Steel`/`ComponentIndustrial`。
- Produces: 建筑 defName（`MMT_HandAssemblyBench`/`MMT_ElectricAssemblyBench`/`MMT_RoboticAssembler`）及其 `<recipes>` 挂载列表（Task 4 配方 defName 已按 Task 1 常量固定）。

- [ ] **Step 1: 复制 13 张建筑贴图**

```bash
mkdir -p "1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench" "1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench" "1.7/Textures/Things/Building/Production/MMT_RoboticAssembler"
for side in east north south; do
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/FabricationBenchHand/FabricationBenchHand_${side}.png" "1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench_${side}.png"
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/FabricationBenchHand/FabricationBenchHand_${side}m.png" "1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/MMT_HandAssemblyBench_${side}m.png"
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/FabricationBench/FabricationBench_${side}.png" "1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench_${side}.png"
  cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/FabricationBench/FabricationBench_${side}m.png" "1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench/MMT_ElectricAssemblyBench_${side}m.png"
done
cp "Hardcore-SK/Mods/Core_SK/Textures/Things/Building/Production/RoboticAssembler/AssemblerBench.png" "1.7/Textures/Things/Building/Production/MMT_RoboticAssembler/MMT_RoboticAssembler.png"
```

- [ ] **Step 2: 编写建筑 XML**

`MMT_Buildings_Parts.xml` 含 3 个 ThingDef，结构参照 `MMT_Buildings_Electric.xml` 的 `MMT_ElectricArcFurnace`（`BuildingBase` + `Building_WorkTable` + `CutoutComplex` + stuff `Metallic` + `designationCategory Production` + `building/uninstallWork` + `inspectorTabs ITab_Bills` + `placeWorkers PlaceWorker_PreventInteractionSpotOverlap` + `researchPrerequisites` + `constructionSkillPrerequisite`）。要点：

- 三座建筑的 label/texPath/graphicClass/costStuffCount/costList/WorkToBuild/MaxHitPoints/size/速度因子/功耗/研究前置/建造技能/recipes 列表以 Task 1 `PARTS_BUILDING_EXPECTATIONS` 为准（注意手动台 costList 是 `WoodLog`×40 + `Steel`×25，**不是**零件）；
- `drawSize`：两座 3×1 台用 `(3,1)`，机器人台用 `(1,1)`；`interactionCellOffset` 三台均 `(0,0,-1)`；
- `passability PassThroughOnly`、`pathCost 90`、`fillPercent 0.7`、`canOverlapZones false`、`hasInteractionCell true`、`useStuffTerrainAffordance true`、`minifiedDef MinifiedThing`、`tickerType Normal`；Flammability 1.0；Cleanliness：手动 -8、电动 -2、机器人 -4；Mass：两座台 500、机器人台 100；
- `uninstallWork` 按 0.5×WorkToBuild：手动 600、电动 2750、机器人 4500；
- 手动台：不写 `<comps>`；
- 电动台 comps：`CompProperties_Power`（`CompPowerTrader`，basePowerConsumption 750，`shortCircuitInRain true`）+ `CompProperties_Flickable` + `CompProperties_Breakdownable`；
- 机器人台 comps：同上但 basePowerConsumption 5000，另加 `CompProperties_Glower`（glowRadius 2，glowColor `(255,0,0,0)`）；
- description 示例（电动装配台）：`以电力驱动的装配台，可以更快地组装零件、导线与机构件。需要持续供电。`

- [ ] **Step 3: 提交**

```bash
git add 1.7/Defs/ThingDefs_Buildings/MMT_Buildings_Parts.xml 1.7/Textures/Things/Building/Production/MMT_HandAssemblyBench/ 1.7/Textures/Things/Building/Production/MMT_ElectricAssemblyBench/ 1.7/Textures/Things/Building/Production/MMT_RoboticAssembler/
git commit -m "feat: 添加零件装配台建筑"
```

---

### Task 4: 三级研究与 14 个配方（GREEN 第三部分）

**Files:**
- Create: `1.7/Defs/ResearchProjectDefs/MMT_Research_Parts.xml`
- Create: `1.7/Defs/RecipeDefs/MMT_Recipes_Parts.xml`

**Interfaces:**
- Consumes: Task 2 资源 defName、Task 3 建筑 defName 与挂载列表、既有 `MMT_CopperBar`/`MMT_SyntheticFibers`/`MMT_Tungsten`/`MMT_Titanium`/`MMT_StelliteAlloy`/`MMT_PobediteAlloy`、原版 `WoodLog`/`Steel`/`Plasteel`/`Silver`/`Gold`/`ComponentIndustrial`/`ComponentAdvanced`、既有研究 `MMT_AdvancedAlloys`。
- Produces: 研究 defName（`MMT_ComponentCrafting`/`MMT_AdvancedComponents`/`MMT_UltraComponents`）。

- [ ] **Step 1: 编写研究 XML**

`MMT_Research_Parts.xml` 含 3 个 ResearchProjectDef，结构参照 `MMT_Research_AdvancedAlloys.xml`。字段以 Task 1 `PARTS_RESEARCH_EXPECTATIONS` 为准；`MMT_ComponentCrafting` 无前置（不写 `<prerequisites>` 元素）。description 示例：零件加工 `学习使用木材与金属组装基础零件、导线与机构件，并搭建装配台。`；高级零件 `掌握以玻璃钢加工高级零件、高级机构件与太空零件的精密装配工艺。`；超级零件 `以稀有合金与贵金属制造最精密的超级零件，只有机器人装配台能完成。`

- [ ] **Step 2: 编写配方 XML**

`MMT_Recipes_Parts.xml` 含 14 个 RecipeDef，结构参照 `MMT_Recipes_Electronics.xml` 的 `MMT_AssembleElectronicParts_Hand`（统一 `GeneralLaborSpeed`/`Smith`/`Recipe_Machining`、workSkill Crafting、learn 0.8、不写 recipeUsers、写 researchPrerequisite）。全部数值以 Task 1 `PARTS_RECIPE_EXPECTATIONS` 为准。要点：

- 2 个原始零件配方不写 `skillRequirements`；
- label：组装原始零件（木材）/组装原始零件（金属）/组装工业零件/制作导线/组装机构件/组装高级零件/组装高级机构件/组装太空零件/组装超级零件（手动/电动同名牌配方共用同一 label，与现有 `_Hand`/`_Electric` 配方惯例一致）；
- description 需注明产出数量（如 `用木材组装原始零件。产出 10。`、`将钢或玻璃钢组装成工业零件。产出 10。`）。

- [ ] **Step 3: 运行套件确认 GREEN**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py parts`
预期：`parts: OK`。

- [ ] **Step 4: 提交**

```bash
git add 1.7/Defs/ResearchProjectDefs/MMT_Research_Parts.xml 1.7/Defs/RecipeDefs/MMT_Recipes_Parts.xml
git commit -m "feat: 添加零件链研究与配方"
```

---

### Task 5: 全量验证与收尾

- [ ] **Step 1: 全量静态验证**

运行：`PYTHONIOENCODING=utf-8 python tools/validate_mod.py all`
预期：全部通过（all-xml 计数从 50 增至 54）。

- [ ] **Step 2: C# 编译验证**

运行：`"/c/Program Files/dotnet/dotnet.exe" build Source/MyModeTest/MyModeTest.csproj --no-restore`
预期：0 错误，`1.7/Assemblies/MyModeTest.dll` 存在。

- [ ] **Step 3: 收尾确认**

`git status` 干净；最终报告明确说明只通过静态检查与 C# 编译，未进行游戏内验证。
