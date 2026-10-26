# -*- coding: utf-8 -*-
"""从 HSK(只读参考)生成 MMT 仿生链 Def。

复用 docs/temp/extract_hsk_bionics.py 的解析结果(导入即重建其内存索引),
产出:
  1.7/Defs/ThingDefs_Items/MMT_Items_Bionics.xml        组一/二/七物品
  1.7/Defs/ThingDefs_Items/MMT_Items_BionicsAdv.xml     组三/四/五/六/八物品
  1.7/Defs/ThingDefs_Items/MMT_Items_BionicsCONN.xml    组九~十三 CONN 物品
  1.7/Defs/ThingDefs_Items/MMT_Items_Neuromod.xml       组十四神经调制使用品
  1.7/Defs/HediffDefs/MMT_Hediffs_Bionics.xml           组一~八 Hediff
  1.7/Defs/HediffDefs/MMT_Hediffs_BionicsCONN.xml       组九~十三 Hediff
  1.7/Defs/RecipeDefs/MMT_Recipes_Bionics.xml           全部安装手术
  1.7/Defs/ResearchProjectDefs/MMT_Research_Bionics.xml 研究树(制造13+安装9)
  1.7/Patches/MMT_Patch_VanillaBionics.xml              原版同名部件改造
并复制物品贴图到 1.7/Textures/Things/Item/MMT/MMT_Bionics/。

卫生仿生(Dubs Bad Hygiene,组十五)不移植。CE/EBF 依赖剔除,CONN 自定义
comp 转为 MMT.* 同名实现(Source/MyModeTest/BionicsComps.cs)。
"""
import os, io, contextlib, copy, shutil
import xml.etree.ElementTree as ET
import importlib.util

ROOT = os.path.dirname(os.path.abspath(__file__))
RW = os.path.abspath(os.path.join(ROOT, ".."))
HSK = os.path.join(RW, "Hardcore-SK")

# ---- 导入提取脚本(副作用:重新生成 docs/hsk-bionic-chains.md,幂等无害) ----
_spec = importlib.util.spec_from_file_location(
    "ext", os.path.join(RW, "docs", "temp", "extract_hsk_bionics.py"))
ext = importlib.util.module_from_spec(_spec)
with contextlib.redirect_stdout(io.StringIO()):
    _spec.loader.exec_module(ext)

warn = []

def esc(t):
    return (t or "").replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

# ---- 原版同名部件:不新建 Def,经 MMT_Patch_VanillaBionics.xml 改造 ----
VANILLA_ITEMS = {
    "Denture", "SimpleProstheticArm", "SimpleProstheticLeg",
    "BionicEar", "BionicJaw", "BionicSpine", "BionicEye", "BionicArm",
    "BionicTongue", "BionicLeg", "PowerClaw", "BionicStomach", "BionicHeart",
    "Joywire", "Painstopper",
    "ArchotechEye", "ArchotechArm", "ArchotechLeg",
}

# 1.7 已移除物品 ThingDef 的原版部件(假牙改为手术直接安装,只需药品):
# 不对其生成补丁(补丁会因找不到 ThingDef 节点而失败)
VANILLA_ITEMS_REMOVED_17 = {"Denture"}

def item_ref(dn):
    """物品引用名:材料映射表优先(MAAI_Chip->MMT_MedicalAssistantChip 等),
    原版同名件保留原 defName,其余加 MMT_ 前缀。"""
    if dn in MAT_MAP:
        return MAT_MAP[dn]
    return dn if dn in VANILLA_ITEMS else "MMT_" + dn

# ---- HSK 材料 -> MMT/原版 defName ----
MAT_MAP = {
    # MMT 已有资源
    "Biomatter": "MMT_Biomatter", "SyntheticFibers": "MMT_SyntheticFibers",
    "Microchips": "MMT_Microchip", "Paraffins": "MMT_Paraffins",
    "Mechanism": "MMT_Mechanism", "ComponentMedieval": "MMT_ComponentMedieval",
    "AdvMechanism": "MMT_AdvMechanism", "ComponentSpacer": "MMT_SpacerComponent",
    "ComponentUltra": "MMT_UltraComponent",
    "ComponentAdvanced": "MMT_SpacerComponent",  # 先进零部件:取最接近层级
    "AlphaPoly": "MMT_AlphaPoly", "BetaPoly": "MMT_BetaPoly",
    "MagneticMaterial": "MMT_MagneticMaterial", "BioMicrochips": "MMT_BioMicrochips",
    "BiosyntheticMaterial": "MMT_BiosyntheticMaterial", "Titanium": "MMT_Titanium",
    "CarbonAlloy": "MMT_CarbonAlloy", "Wire": "MMT_Wire", "Glass": "MMT_Glass",
    "ReinforcedGlass": "MMT_ReinforcedGlass", "Plastic": "MMT_Plastic",
    "Rubber": "MMT_Rubber", "RawDevilstrand": "MMT_RawDevilstrand",
    "Electronics": "MMT_IntegratedCircuit", "HerbMedicine": "MMT_HerbMedicine",
    "VG_Ibuprofen": "MMT_Ibuprofen",
    # 本链新增材料
    "ArtificialBone": "MMT_ArtificialBone", "MAAI_Chip": "MMT_MedicalAssistantChip",
    # 原版材料/药品直通
    "ComponentIndustrial": "ComponentIndustrial", "Gold": "Gold", "Silver": "Silver",
    "Hyperweave": "Hyperweave", "Neutroamine": "Neutroamine",
    "Penoxycyline": "Penoxycyline", "MedicineUltratech": "MedicineUltratech",
    "AIPersonaCore": "AIPersonaCore", "Flake": "Flake", "Yayo": "Yayo",
    "GoJuice": "GoJuice", "PsychoidLeaves": "PsychoidLeaves",
    # 天然耳(HSK 器官收割产物)-> 原版仿生耳
    "Ear": "BionicEar",
}

# ---- HSK 研究 -> MMT/原版 ----
RES_MAP = {
    "Prosthetics": "Prosthetics", "Bionics": "Bionics",  # 原版研究直通
    "VitalsMonitor": "VitalsMonitor",
    "Clinical_B1": "MMT_Medicine_B1", "Clinical_C1": "MMT_Medicine_B2",
    "Clinical_D1": "MMT_Medicine_D1", "Clinical_D2": "MMT_Medicine_D1",
    "Clinical_E1": "MedicineProduction",
}
for t in ["B1", "C1", "C2", "D1", "D2", "D3", "D4", "E1", "E2", "E3", "E4", "E5", "E6"]:
    RES_MAP["Prosthesis_making_" + t] = "MMT_Prosthesis_" + t
for t in ["C1", "C2", "C3", "D1", "D2", "D3", "D4", "E1", "E2"]:
    RES_MAP["Prosthesis_install_" + t] = "MMT_ProsthesisInstall_" + t

# ---- HSK 工作台 -> MMT ----
BENCH_MAP = {
    "TablePrimitiveProsthetic": "MMT_PrimitiveProstheticsTable",
    "TableBasicProsthetic": "MMT_BasicProstheticsTable",
    "TableBionics": "MMT_BionicsTable",
    "TableSynthetics": "MMT_SyntheticsTable",
    "UpgradingStation": "MMT_UpgradingStation",
    "meditable": "MMT_MedicalTable",
    "ElectricSmithy": "MMT_MechWeaponTable",  # 仅飞螳镰刃使用
    "TableElectronics": "MMT_ElectricElectronicsTable",  # 神经调制物品
}

# ---- ThingCategory 映射(HSK 自定义 -> 原版) ----
CAT_MAP = {
    "BodyPartsBionic": "BodyPartsBionic",
    "BodyPartsProsthetic": "BodyPartsProsthetic",
    "BodyPartsArchotech": "BodyPartsArchotech",
    "CyberneticProstheses": "BodyPartsArchotech",
    "Neurotrainers": "Neurotrainers",
    # HSK 自定义类别 -> 原版就近类别
    "BasicProstheses": "BodyPartsProsthetic",
    "SimpleProstheses": "BodyPartsProsthetic",
    "SyntheticOrgans": "BodyPartsBionic",
    "BodyParts": "BodyPartsBionic",
}

# MMT/原版不存在的 stat(C/CONN 专有),从 statOffsets 剔除
STAT_DROP = {
    "CONN_HealthPointAddedToPart", "CONN_ShieldMultiplier", "CONN_ShieldRechargeRateMultiplier",
    "CarryWeight", "ShotSpread", "SightsEfficiency", "SwayFactor",
}

# EMP 反应 hediff(HSK 专有)-> MMT 版本(手维护于 1.7/Defs/HediffDefs/MMT_Hediffs_EMP.xml)
EMP_HEDIFF_MAP = {
    "EMP_Disorientation": "MMT_EMP_Disorientation",
    "EMP_ProtectiveExoskeleton": "MMT_EMP_ProtectiveExoskeleton",
}

# Orassan 种族专用部件(尾巴/爪):MMT 无该种族,人类身体无 OTail/HandClaws 部位,整体跳过
ORASSAN_ONLY = {"OBionicTail", "PlasteelClaws", "CyberneticTail"}

# HSK 中本就无安装配方的材料型物品(不告警)
MATERIALS_ONLY = {
    "SyntheticSkin", "NeuromuscularFramework", "NeurocureFramework",
    "InstinctOptimizingNanobots", "ScytherBlade",
}

# ---- 工作速度 Stat 映射 ----
WORK_SPEED_MAP = {
    "ElectronicCraftingSpeed": "MMT_ElectronicCraftingSpeed",
    "SmithingSpeed": "MMT_SmithingSpeed",
}

# ---- CONN 自定义类 -> MMT 实现 ----
CLASS_MAP = {
    "CONN.HediffGizmoBerserk": "MyModeTest.HediffBerserkGizmo",
    "CONN.HediffCompProperties_ThoughtsRemover": "MyModeTest.HediffCompProperties_ThoughtsRemover",
    "CONN.HediffCompProperties_RecreationGiver": "MyModeTest.HediffCompProperties_RecreationGiver",
    "CONN.CompProperties_LightEffect": "MyModeTest.HediffCompProperties_LightEffect",
    # Core_SK 专有 hediff 类 -> MMT 实现
    "Hediff_AddedPartNoRemoveSubparts": "MyModeTest.Hediff_AddedPartNoRemoveSubparts",
    "SK.Hediff_Senexium": "MyModeTest.Hediff_AgeAccelerator",
}

# 组十四:使用类物品 compClass 映射
NEUROMOD_COMP = {
    "CONN_NeuromodBooster": "MyModeTest.CompUseEffect_LearnSkillRandom",
    "CONN_Brainwash": "MyModeTest.CompUseEffect_TraitReset",
    "CONN_PassionGiver": "MyModeTest.CompUseEffect_PassionGiver",
    "CONN_TraitGiver": "MyModeTest.CompUseEffect_TraitGiver",
    "NeurotrainersClearer": "MyModeTest.CompUseEffect_ThoughtsClearer",
    "CONN_Reconditioner": "MyModeTest.CompUseEffect_Reconditioner",
}

BODYPART_EN = {v: k for k, v in ext.BODYPART_CN.items()}

ALL_ITEMS = [m for g in ext.GROUPS[:-1] for m in g[2]]

def map_mat(dn):
    if dn in MAT_MAP:
        return MAT_MAP[dn]
    if dn in ALL_ITEMS:
        return item_ref(dn)
    warn.append("未知材料映射: " + dn)
    return dn

def map_res(rn):
    if rn is None:
        return None
    if rn in RES_MAP:
        return RES_MAP[rn]
    warn.append("未知研究映射: " + rn)
    return rn

def map_bench(bn):
    if bn in BENCH_MAP:
        return BENCH_MAP[bn]
    warn.append("未知工作台映射: " + bn)
    return bn

def get_benches(item):
    """recipeUsers:Inherit=False 表示不继承父类但子节点仍有效,需直接取 li。"""
    for node in ext.chain(item):
        cur = node.find("recipeMaker/recipeUsers")
        if cur is not None:
            return [li.text.strip() for li in cur.findall("li") if li.text]
    return []

# ---- 完整安装配方索引(不限 Install 前缀,CONN_InstallXxx 也命中) ----
install_by_item = {}
for (tag, dn), el in ext.defs.items():
    if tag != "RecipeDef":
        continue
    if el.findtext("addsHediff") is None and not (dn or "").startswith(("Install", "CONN_Install")):
        continue
    for ing in el.findall("ingredients/li"):
        td = ing.find("filter/thingDefs")
        if td is None:
            continue
        for li in td.findall("li"):
            if li.text and not li.text.strip().startswith("Medicine"):
                install_by_item.setdefault(li.text.strip(), []).append(el)

# ---- XML 片段输出 ----
def ser(el):
    """序列化元素的子节点为 XML 文本(保持原结构)。"""
    return "".join(ET.tostring(c, encoding="unicode") for c in list(el))

# CE ToolCE -> 原版 tool:仅保留白名单字段
TOOL_KEEP = {"label", "capacities", "power", "cooldownTime", "armorPenetration",
             "linkedBodyPartsGroup", "chanceFactor"}

def convert_comp(li):
    """转换一个 hediff comp <li>,返回 XML 文本或 None(剔除)。"""
    cls = li.get("Class", "")
    if cls.startswith("EBF.") or cls.startswith("CombatExtended."):
        return None
    if cls == "HediffCompProperties_GiveAbility":
        warn.append("剔除 GiveAbility comp(能力 Def 未移植)")
        return None
    if cls in CLASS_MAP:
        li = copy.deepcopy(li)
        li.set("Class", CLASS_MAP[cls])
        # LightEffect 的 mote 引用改 MMT
        vm = li.find("visualMote")
        if vm is not None and vm.text:
            vm.text = "MMT_Mote_Flashlight"
        return ser_wrap(li)
    if cls == "HediffCompProperties_ReactOnDamage":
        # EMP 反应 hediff 为 HSK 专有,MMT 版本手维护于 HediffDefs/MMT_Hediffs_EMP.xml
        li = copy.deepcopy(li)
        ch = li.find("createHediff")
        if ch is not None and ch.text:
            ch.text = EMP_HEDIFF_MAP.get(ch.text, "MMT_" + ch.text)
        return ser_wrap(li)
    if cls == "HediffCompProperties_VerbGiver":
        li = copy.deepcopy(li)
        tools = li.find("tools")
        if tools is not None:
            for t in tools.findall("li"):
                t.attrib.pop("Class", None)
                for ch in list(t):
                    if ch.tag not in TOOL_KEEP:
                        t.remove(ch)
        return ser_wrap(li)
    return ser_wrap(li)

def ser_wrap(el):
    return ET.tostring(el, encoding="unicode")

def indent_block(xml_text, ind):
    return "\n".join(ind + line for line in xml_text.splitlines())

def get_cn(dn, kind="label"):
    return ext.cn_of(dn, kind)

# ============ Hediff 生成 ============
def emit_hediff(hdn, item_dn):
    """生成一个 MMT HediffDef,返回 XML 文本;无定义时返回 None。"""
    hd = ext.defs.get(("HediffDef", hdn))
    if hd is None:
        warn.append("未找到 HediffDef: %s (物品 %s)" % (hdn, item_dn))
        return None
    label = get_cn(hdn) or hd.findtext("label") or hdn
    desc = get_cn(hdn, "description") or hd.findtext("description") or ""
    labelnoun = get_cn(hdn, "labelNoun") or hd.findtext("labelNoun")
    hclass = ext.get_scalar(hd, "hediffClass") or "HediffWithComps"
    hclass = CLASS_MAP.get(hclass, hclass)
    color = ext.get_scalar(hd, "defaultLabelColor")

    out = ['  <HediffDef>']
    out.append('    <defName>%s</defName>' % item_ref(hdn))
    out.append('    <label>%s</label>' % esc(label))
    if desc:
        out.append('    <description>%s</description>' % esc(desc))
    if labelnoun:
        out.append('    <labelNoun>%s</labelNoun>' % esc(labelnoun))
    out.append('    <hediffClass>%s</hediffClass>' % hclass)
    if color:
        out.append('    <defaultLabelColor>%s</defaultLabelColor>' % esc(color))
    out.append('    <isBad>false</isBad>')
    # initialSeverity 透传(如年龄加速器 severity=60 决定加速倍率)
    sev = ext.get_scalar(hd, "initialSeverity")
    if sev:
        out.append('    <initialSeverity>%s</initialSeverity>' % esc(sev))

    # addedPartProps(白名单字段)
    ap = None
    for node in ext.chain(hd):
        ap = node.find("addedPartProps")
        if ap is not None:
            break
    if ap is not None:
        keep = ("partEfficiency", "solid", "betterThanNatural")
        seg = ['    <addedPartProps>']
        for k in keep:
            v = ap.findtext(k)
            if v:
                seg.append('      <%s>%s</%s>' % (k, v, k))
        seg.append('    </addedPartProps>')
        if len(seg) > 2:
            out += seg

    # stages(子优先;剔除 CE/CONN 专有 statOffsets/statFactors)
    for node in ext.chain(hd):
        st = node.find("stages")
        if st is not None and len(st):
            st = copy.deepcopy(st)
            for stage in st.findall("li"):
                for tag in ("statOffsets", "statFactors"):
                    so = stage.find(tag)
                    if so is not None:
                        for c in list(so):
                            if c.tag in STAT_DROP:
                                so.remove(c)
                        if not len(so):
                            stage.remove(so)
            out.append('    <stages>')
            out.append(indent_block(ser(st).strip(), "      "))
            out.append('    </stages>')
            break

    # comps(子优先,逐个转换)
    for node in ext.chain(hd):
        comps = node.find("comps")
        if comps is not None and len(comps):
            converted = []
            for li in comps.findall("li"):
                x = convert_comp(li)
                if x:
                    converted.append(x)
            if converted:
                out.append('    <comps>')
                for x in converted:
                    out.append(indent_block(x.strip(), "      "))
                out.append('    </comps>')
            break

    sp = ext.get_scalar(hd, "spawnThingOnRemoved")
    if sp:
        out.append('    <spawnThingOnRemoved>%s</spawnThingOnRemoved>' % item_ref(sp))
    if ext.get_scalar(hd, "priceImpact") == "true":
        out.append('    <priceImpact>true</priceImpact>')
    out.append('  </HediffDef>')
    return "\n".join(out)

# ============ 安装手术生成 ============
def emit_install(item_dn, rec, item_cn):
    rdn = rec.findtext("defName") or ("Install" + item_dn)
    adds = rec.findtext("addsHediff")
    if not adds:
        guess = rdn.replace("CONN_Install", "").replace("Install", "")
        if ("HediffDef", guess) in ext.defs:
            adds = guess
    if not adds:
        warn.append("安装配方 %s 无法确定 addsHediff" % rdn)
        return None
    label = get_cn(rdn) or ("植入" + item_cn)
    desc = get_cn(rdn, "description") or ("将%s植入体内。" % item_cn)
    job = get_cn(rdn, "jobString") or ("正在植入%s。" % item_cn)

    out = ['  <RecipeDef ParentName="SurgeryInstallBodyPartArtificialBase">']
    out.append('    <defName>MMT_%s</defName>' % rdn.replace("CONN_Install", "InstallCONN_"))
    out.append('    <label>%s</label>' % esc(label))
    out.append('    <description>%s</description>' % esc(desc))
    out.append('    <jobString>%s</jobString>' % esc(job))
    out.append('    <recipeUsers><li>Human</li></recipeUsers>')
    wa = ext.get_scalar(rec, "workAmount")
    if wa:
        out.append('    <workAmount>%s</workAmount>' % wa)
    sf = ext.get_scalar(rec, "surgerySuccessChanceFactor")
    if sf:
        out.append('    <surgerySuccessChanceFactor>%s</surgerySuccessChanceFactor>' % sf)
    df = ext.get_scalar(rec, "deathOnFailedSurgeryChance")
    if df:
        out.append('    <deathOnFailedSurgeryChance>%s</deathOnFailedSurgeryChance>' % df)
    skills = ext.get_pairs(rec, "skillRequirements")
    if skills:
        out.append('    <skillRequirements>')
        for k, v in skills:
            out.append('      <%s>%s</%s>' % (k, v, k))
        out.append('    </skillRequirements>')
    rr = map_res(ext.get_scalar(rec, "researchPrerequisite"))
    if rr:
        out.append('    <researchPrerequisite>%s</researchPrerequisite>' % rr)
    # 药品消耗
    med = "1"
    for ing in rec.findall("ingredients/li"):
        cat = ing.find("filter/categories/li")
        if cat is not None and cat.text == "Medicine":
            med = ing.findtext("count") or "1"
    out.append('    <ingredients>')
    out.append('      <li><filter><categories><li>Medicine</li></categories></filter><count>%s</count></li>' % med)
    out.append('      <li><filter><thingDefs><li>%s</li></thingDefs></filter><count>1</count></li>' % item_ref(item_dn))
    out.append('    </ingredients>')
    out.append('    <fixedIngredientFilter><categories><li>Medicine</li></categories>'
               '<thingDefs><li>%s</li></thingDefs></fixedIngredientFilter>' % item_ref(item_dn))
    parts = [li.text for li in rec.findall("appliedOnFixedBodyParts/li") if li.text]
    if not parts and item_dn in ext.FALLBACK_PART:
        parts = [BODYPART_EN[ext.FALLBACK_PART[item_dn]]]
    if not parts:
        warn.append("安装配方 %s 缺少 appliedOnFixedBodyParts" % rdn)
    else:
        out.append('    <appliedOnFixedBodyParts>')
        for p in parts:
            out.append('      <li>%s</li>' % p)
        out.append('    </appliedOnFixedBodyParts>')
    out.append('    <addsHediff>%s</addsHediff>' % item_ref(adds))
    out.append('  </RecipeDef>')
    return "\n".join(out), adds

# ============ 物品 ThingDef 生成 ============
STAT_KEEP = ("Mass", "MarketValue", "MaxHitPoints", "DeteriorationRate", "Flammability")

def emit_item(dn):
    item = ext.defs.get(("ThingDef", dn))
    if item is None:
        warn.append("未找到 ThingDef: " + dn)
        return None
    label = get_cn(dn) or item.findtext("label") or dn
    desc = get_cn(dn, "description") or item.findtext("description") or ""

    out = ['  <ThingDef ParentName="MMT_BodyPartBase">']
    out.append('    <defName>%s</defName>' % item_ref(dn))
    out.append('    <label>%s</label>' % esc(label))
    if desc:
        out.append('    <description>%s</description>' % esc(desc))

    tech = ext.get_scalar(item, "techLevel")
    if tech:
        out.append('    <techLevel>%s</techLevel>' % tech)
    cats = ext.get_list(item, "thingCategories")
    mapped_cats = []
    for c in cats:
        if c in CAT_MAP:
            mapped_cats.append(CAT_MAP[c])
        else:
            warn.append("未知 ThingCategory: %s (%s)" % (c, dn))
            mapped_cats.append("BodyPartsBionic")
    if mapped_cats:
        out.append('    <thingCategories>')
        for c in dict.fromkeys(mapped_cats):
            out.append('      <li>%s</li>' % c)
        out.append('    </thingCategories>')

    # 贴图:记录来源路径,由主流程复制
    tex = ext.get_scalar(item, "graphicData", "texPath")
    drawsize = ext.get_scalar(item, "graphicData", "drawSize") or "0.80"
    tex_out = tex or ""
    out.append('    <graphicData>')
    out.append('      <texPath>%TEX%</texPath>')
    out.append('      <graphicClass>Graphic_Single</graphicClass>')
    out.append('      <drawSize>%s</drawSize>' % drawsize)
    out.append('    </graphicData>')

    tt = ext.get_list(item, "tradeTags")
    if tt:
        out.append('    <tradeTags>' + "".join('<li>%s</li>' % t for t in tt) + '</tradeTags>')
    th = ext.get_list(item, "techHediffsTags")
    if th:
        out.append('    <techHediffsTags>' + "".join('<li>%s</li>' % t for t in th) + '</techHediffsTags>')
    ts = ext.get_list(item, "thingSetMakerTags")
    if ts:
        out.append('    <thingSetMakerTags>' + "".join('<li>%s</li>' % t for t in ts) + '</thingSetMakerTags>')

    stats = ext.get_pairs(item, "statBases")
    stats = [(k, v) for k, v in stats if k in STAT_KEEP]
    if stats:
        out.append('    <statBases>')
        for k, v in stats:
            out.append('      <%s>%s</%s>' % (k, v, k))
        out.append('    </statBases>')

    cost = ext.get_pairs(item, "costList")
    if cost:
        out.append('    <costList>')
        for k, v in cost:
            out.append('      <%s>%s</%s>' % (map_mat(k), v, map_mat(k)))
        out.append('    </costList>')

    rm = recipe_maker_xml(item, dn, "    ")
    if rm:
        out.append(rm)
    out.append('  </ThingDef>')
    return "\n".join(out), tex

def recipe_maker_xml(item, dn, ind):
    """由继承链合成 recipeMaker 片段;无可制作信息或源 def 显式 IsNull 时返回 None。"""
    # HSK 源 def 显式 <recipeMaker IsNull="True" Inherit="False"/> 表示该件不可制作（如 EyePatch），
    # 此时继承链上虽有基类的 recipeMaker 信息也不能合成，否则产出"有 recipeMaker 但无 costList"的配置错误
    own_rm = item.find("recipeMaker")
    if own_rm is not None and own_rm.attrib.get("IsNull") == "True":
        return None
    benches = get_benches(item)
    cost = ext.get_pairs(item, "costList")
    if not benches or not cost:
        return None
    lines = [ind + '<recipeMaker>']
    ws = ext.get_scalar(item, "recipeMaker", "workSpeedStat")
    if ws in WORK_SPEED_MAP:
        lines.append(ind + '  <workSpeedStat>%s</workSpeedStat>' % WORK_SPEED_MAP[ws])
    wk = ext.get_scalar(item, "recipeMaker", "workSkill")
    if wk:
        lines.append(ind + '  <workSkill>%s</workSkill>' % wk)
    wa = ext.get_scalar(item, "recipeMaker", "workAmount")
    if wa:
        lines.append(ind + '  <workAmount>%s</workAmount>' % wa)
    lines.append(ind + '  <effectWorking>Smith</effectWorking>')
    lines.append(ind + '  <soundWorking>Recipe_Smith</soundWorking>')
    lines.append(ind + '  <unfinishedThingDef>UnfinishedHealthItemBionic</unfinishedThingDef>')
    skills = ext.get_pairs(item, "recipeMaker", "skillRequirements")
    if skills:
        lines.append(ind + '  <skillRequirements>')
        for k, v in skills:
            lines.append(ind + '    <%s>%s</%s>' % (k, v, k))
        lines.append(ind + '  </skillRequirements>')
    rr = map_res(ext.get_scalar(item, "recipeMaker", "researchPrerequisite"))
    if rr:
        lines.append(ind + '  <researchPrerequisite>%s</researchPrerequisite>' % rr)
    pc = ext.get_scalar(item, "recipeMaker", "productCount")
    if pc and pc != "1":
        lines.append(ind + '  <productCount>%s</productCount>' % pc)
    if benches:
        lines.append(ind + '  <recipeUsers>')
        for b in benches:
            lines.append(ind + '    <li>%s</li>' % map_bench(b))
        lines.append(ind + '  </recipeUsers>')
    lines.append(ind + '</recipeMaker>')
    return "\n".join(lines)

# ============ 组十四:神经调制使用品 ============
def emit_neuromod(dn):
    item = ext.defs.get(("ThingDef", dn))
    if item is None:
        warn.append("未找到神经调制物品: " + dn)
        return None
    label = get_cn(dn) or item.findtext("label") or dn
    desc = get_cn(dn, "description") or item.findtext("description") or ""
    comp_cls = NEUROMOD_COMP[dn]

    out = ['  <ThingDef ParentName="MechSerumBase">']
    out.append('    <defName>%s</defName>' % item_ref(dn))
    out.append('    <label>%s</label>' % esc(label))
    if desc:
        out.append('    <description>%s</description>' % esc(desc))
    tex = ext.get_scalar(item, "graphicData", "texPath")
    out.append('    <graphicData><texPath>%TEX%</texPath><graphicClass>Graphic_Single</graphicClass></graphicData>')
    tech = ext.get_scalar(item, "techLevel")
    if tech:
        out.append('    <techLevel>%s</techLevel>' % tech)
    out.append('    <stackLimit>1</stackLimit>')
    out.append('    <tradeNeverStack>true</tradeNeverStack>')
    out.append('    <drawGUIOverlay>false</drawGUIOverlay>')
    out.append('    <thingCategories><li>Neurotrainers</li></thingCategories>')
    out.append('    <tradeability>Sellable</tradeability>')
    ts = ext.get_list(item, "thingSetMakerTags")
    if ts:
        out.append('    <thingSetMakerTags>' + "".join('<li>%s</li>' % t for t in ts) + '</thingSetMakerTags>')
    tt = ext.get_list(item, "tradeTags")
    if tt:
        out.append('    <tradeTags>' + "".join('<li>%s</li>' % t for t in tt) + '</tradeTags>')
    stats = [(k, v) for k, v in ext.get_pairs(item, "statBases") if k in STAT_KEEP]
    if stats:
        out.append('    <statBases>')
        for k, v in stats:
            out.append('      <%s>%s</%s>' % (k, v, k))
        out.append('    </statBases>')
    cost = ext.get_pairs(item, "costList")
    if cost:
        out.append('    <costList>')
        for k, v in cost:
            out.append('      <%s>%s</%s>' % (map_mat(k), v, map_mat(k)))
        out.append('    </costList>')
    rm = recipe_maker_xml(item, dn, "    ")
    if rm:
        # 神经调制物品不用未完成品(对照 HSK 的 UnfinishedComponent,MMT 省略)
        rm = "\n".join(l for l in rm.splitlines() if "unfinishedThingDef" not in l)
        out.append(rm)
    use_label = "使用" + label
    out.append('    <comps>')
    out.append('      <li Class="CompProperties_Usable"><useJob>UseItem</useJob><useLabel>%s</useLabel></li>' % esc(use_label))
    out.append('      <li Class="CompProperties_UseEffect"><compClass>%s</compClass></li>' % comp_cls)
    out.append('      <li Class="CompProperties_UseEffectDestroySelf" />')
    out.append('    </comps>')
    out.append('  </ThingDef>')
    return "\n".join(out), tex

# HSK 研究原文描述多为占位,按解锁内容补写中文描述
RES_DESC = {
    "Prosthesis_making_B1": "学习用木石与简单零件制作最基础的义肢。解锁原始假肢工作台，以及眼罩、钩手、义齿的制作。",
    "Prosthesis_making_C1": "掌握工业级义肢组装技术。解锁生命维持系统的制作。",
    "Prosthesis_making_C2": "改进义肢生产线。解锁假肢工作台，可制作玻璃眼等进阶简易义肢。",
    "Prosthesis_making_D1": "结合生命监测技术，为人造器官制造打下基础。",
    "Prosthesis_making_D2": "掌握人造器官与神经刺激的制造。解锁肌肉刺激器等植入物。",
    "Prosthesis_making_D3": "建立合成人造组织工艺。解锁人造器官组装台、人造骨头与七种功能肋骨。",
    "Prosthesis_making_D4": "掌握高端合成材料植入。解锁合成皮肤、神经肌肉结构、飞螳镰刃等部件。",
    "Prosthesis_making_E1": "神经机械技术入门。解锁神经机械体工作台与防护性外骨骼。",
    "Prosthesis_making_E2": "神经机械技术进阶。解锁暗影行者、外骨骼装甲、皮肤装甲板等 CONN 高级仿生。",
    "Prosthesis_making_E3": "神经机械战斗化改造。解锁能量连接、外刃、动力臂、能量拳等战斗仿生。",
    "Prosthesis_making_E4": "闪耀世界脑机接口技术。解锁战术角膜植入体与全部辅助AI核心。",
    "Prosthesis_making_E5": "顶级神经机械工艺。解锁机械眼、变焦眼、黑珍珠、悬浮脚等 CONN 顶级仿生。",
    "Prosthesis_making_E6": "神经机械技术巅峰。解锁最终级仿生部件。",
    "Prosthesis_install_C1": "训练医生安装基础义肢。解锁生命维持系统(血泵/呼吸器等)的安装手术。",
    "Prosthesis_install_C2": "提高义肢手术水平。解锁简易假手、假足、假腿、假臂与人造鼻、人造骨盆的安装。",
    "Prosthesis_install_C3": "进一步规范义肢手术流程。",
    "Prosthesis_install_D1": "掌握人造器官移植。解锁功能肋骨与合成器官的安装手术。",
    "Prosthesis_install_D2": "提高人造器官手术成功率。",
    "Prosthesis_install_D3": "掌握仿生体移植。解锁仿生手、仿生足、仿生耳等 Bionics 级部件的安装。",
    "Prosthesis_install_D4": "掌握脊椎与神经级仿生安装。",
    "Prosthesis_install_E1": "掌握脑机接口手术。解锁辅助AI核心与防护性外骨骼的安装。",
    "Prosthesis_install_E2": "神经机械手术巅峰。解锁外骨骼、战术角膜植入体等顶级部件的安装。",
}

# ============ 研究树 ============
def emit_research():
    """解析 HSK Research_Medicine.xml,输出 MMT 研究树。"""
    path = os.path.join(HSK, "Mods", "Core_SK", "Defs", "ResearchProjectDefs", "Research_Medicine.xml")
    root = ET.parse(path).getroot()
    hsk_res = {el.findtext("defName"): el for el in root if el.tag == "ResearchProjectDef"}
    making = ["B1", "C1", "C2", "D1", "D2", "D3", "D4", "E1", "E2", "E3", "E4", "E5", "E6"]
    install = ["C1", "C2", "C3", "D1", "D2", "D3", "D4", "E1", "E2"]
    out = ['<?xml version="1.0" encoding="utf-8"?>', '<Defs>',
           '  <!-- 仿生/义肢研究树:对照 HSK Core_SK Research_Medicine.xml 义肢制造/义肢安装双线,',
           '       制造线锚定原版 Bionics,安装线锚定原版 Prosthetics;HSK 临床医学前置映射到',
           '       MMT_Medicine 链(Clinical_B1→MMT_Medicine_B1 等,VitalsMonitor/Clinical_E1 用原版)。',
           '       制造线 Y=6.5,安装线 Y=7.5(避开已有整数行)。 -->', '']
    def emit_one(hsk_dn, x, y):
        el = hsk_res.get(hsk_dn)
        if el is None:
            warn.append("未找到研究: " + hsk_dn)
            return
        mmt_dn = RES_MAP[hsk_dn]
        label = get_cn(hsk_dn) or el.findtext("label") or hsk_dn
        desc = RES_DESC.get(hsk_dn) or get_cn(hsk_dn, "description") or el.findtext("description") or ""
        if desc == "暂无介绍。":
            desc = ""
        out.append('  <!-- %s -->' % esc(label))
        out.append('  <ResearchProjectDef>')
        out.append('    <defName>%s</defName>' % mmt_dn)
        out.append('    <label>%s</label>' % esc(label))
        if desc:
            out.append('    <description>%s</description>' % esc(desc))
        out.append('    <baseCost>%s</baseCost>' % el.findtext("baseCost"))
        out.append('    <techLevel>%s</techLevel>' % el.findtext("techLevel"))
        out.append('    <tab>MMT_ResearchTab</tab>')
        pre = [li.text for li in el.findall("prerequisites/li")]
        pre = [map_res(p) for p in pre]
        if pre:
            out.append('    <prerequisites>')
            for p in dict.fromkeys(pre):
                out.append('      <li>%s</li>' % p)
            out.append('    </prerequisites>')
        out.append('    <researchViewX>%.2f</researchViewX>' % x)
        out.append('    <researchViewY>%.2f</researchViewY>' % y)
        out.append('  </ResearchProjectDef>')
        out.append('')
    for i, t in enumerate(making):
        emit_one("Prosthesis_making_" + t, float(i), 6.5)
    for i, t in enumerate(install):
        emit_one("Prosthesis_install_" + t, float(i), 7.5)
    out.append('</Defs>')
    return "\n".join(out) + "\n"

# ============ 原版同名部件补丁 ============
def emit_patch():
    out = ['<?xml version="1.0" encoding="utf-8"?>', '<Patch>',
           '  <!-- 原版同名仿生/义肢部件改造(对照 HSK AutoGenPatches/BodyParts):',
           '       替换 costList 为 MMT 材料链,追加 recipeMaker 改到 MMT 工作台与研究。',
           '       安装手术沿用原版(Human 自带 InstallXxx),数值保留原版。 -->', '']
    n = 0
    for dn in sorted(VANILLA_ITEMS - VANILLA_ITEMS_REMOVED_17):
        item = ext.defs.get(("ThingDef", dn))
        if item is None:
            warn.append("原版部件无 HSK 定义,跳过: " + dn)
            continue
        cost = ext.get_pairs(item, "costList")
        benches = get_benches(item)
        if not cost and not benches:
            warn.append("原版部件无制作信息,保持原版: " + dn)
            continue
        cn = get_cn(dn) or dn
        out.append('  <!-- %s(%s) -->' % (cn, dn))
        out.append('  <Operation Class="PatchOperationConditional">')
        out.append('    <xpath>Defs/ThingDef[defName="%s"]/costList</xpath>' % dn)
        out.append('    <match Class="PatchOperationRemove">')
        out.append('      <xpath>Defs/ThingDef[defName="%s"]/costList</xpath>' % dn)
        out.append('    </match>')
        out.append('  </Operation>')
        val = []
        if cost:
            val.append('    <costList>')
            for k, v in cost:
                val.append('      <%s>%s</%s>' % (map_mat(k), v, map_mat(k)))
            val.append('    </costList>')
        rm = recipe_maker_xml(item, dn, "    ")
        if rm:
            val.append(rm)
        out.append('  <Operation Class="PatchOperationAdd">')
        out.append('    <xpath>Defs/ThingDef[defName="%s"]</xpath>' % dn)
        out.append('    <value>')
        out += val
        out.append('    </value>')
        out.append('  </Operation>')
        out.append('')
        n += 1
    out.append('</Patch>')
    return "\n".join(out) + "\n", n

# ============ 贴图复制 ============
TEX_DEST = os.path.join(RW, "1.7", "Textures", "Things", "Item", "MMT", "MMT_Bionics")
TEX_SRC_ROOTS = [
    os.path.join(HSK, "Mods", "Core_SK", "Textures"),
    os.path.join(HSK, "Mods", "CyberneticOrganism_SK", "Textures"),
]
TEX_OUT_PATH = "Things/Item/MMT/MMT_Bionics"

def copy_tex(tex, dn):
    """复制贴图,返回 MMT texPath;缺图回退 BasicProsthesis。
    HSK BodyPart 贴图常在 HSK/ 或 Core/ 子目录,texPath 不含子目录时逐个探测。"""
    src = None
    base = None
    if tex:
        cands = [tex]
        d, b = tex.rsplit("/", 1)
        base = b
        for sub in ("HSK", "Core"):
            cands.append(d + "/" + sub + "/" + b)
        for r in TEX_SRC_ROOTS:
            for c in cands:
                p = os.path.join(r, c + ".png")
                if os.path.isfile(p):
                    src = p
                    break
            if src:
                break
        # HSK 未启用贴图库兜底(Unused/Bodyparts/<basename>.png)
        if src is None and base:
            p = os.path.join(TEX_SRC_ROOTS[0], "..", "Textures", "Unused", "Bodyparts", base + ".png")
            p = os.path.normpath(p)
            if os.path.isfile(p):
                src = p
    if src is None:
        if tex:
            warn.append("缺贴图 %s (%s),回退 BasicProsthesis" % (tex, dn))
        base = "BasicProsthesis"
        src = os.path.join(TEX_SRC_ROOTS[0], "Things", "Item", "BodyPart", "BasicProsthesis.png")
        if not os.path.isfile(src):
            warn.append("回退贴图也缺失: " + src)
            return "Things/Item/MMT/MMT_Bionics/BasicProsthesis"
    os.makedirs(TEX_DEST, exist_ok=True)
    shutil.copyfile(src, os.path.join(TEX_DEST, base + ".png"))
    return TEX_OUT_PATH + "/" + base

# 人造骨头制作配方(对照 HSK Make_ArtificialBone/Bulk,材料已映射 MMT)
MATERIAL_RECIPES = '''  <!-- 材料配方:人造骨头(HSK BodyParts_Bones.xml Make_ArtificialBone/Bulk) -->
  <RecipeDef>
    <defName>MMT_MakeArtificialBone</defName>
    <label>制造人造骨骼</label>
    <description>制造人造骨骼。</description>
    <jobString>正在制造骨骼。</jobString>
    <workAmount>200</workAmount>
    <workSpeedStat>MMT_SmithingSpeed</workSpeedStat>
    <workSkill>Crafting</workSkill>
    <effectWorking>Smith</effectWorking>
    <soundWorking>Recipe_Machining</soundWorking>
    <ingredients>
      <li><filter><thingDefs><li>MMT_Biomatter</li></thingDefs></filter><count>5</count></li>
      <li><filter><thingDefs><li>MMT_SpacerComponent</li></thingDefs></filter><count>4</count></li>
      <li><filter><thingDefs><li>MMT_Paraffins</li></thingDefs></filter><count>2</count></li>
      <li><filter><thingDefs><li>MMT_Plastic</li></thingDefs></filter><count>10</count></li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Biomatter</li>
        <li>MMT_SpacerComponent</li>
        <li>MMT_Paraffins</li>
        <li>MMT_Plastic</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_ArtificialBone>1</MMT_ArtificialBone>
    </products>
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <researchPrerequisite>MMT_Prosthesis_D3</researchPrerequisite>
  </RecipeDef>

  <RecipeDef>
    <defName>MMT_MakeArtificialBoneBulk</defName>
    <label>制造人造骨骼（x4）</label>
    <description>制造人造骨骼 x4。</description>
    <jobString>正在制造骨骼。</jobString>
    <workAmount>720</workAmount>
    <workSpeedStat>MMT_SmithingSpeed</workSpeedStat>
    <workSkill>Crafting</workSkill>
    <effectWorking>Smith</effectWorking>
    <soundWorking>Recipe_Machining</soundWorking>
    <ingredients>
      <li><filter><thingDefs><li>MMT_Biomatter</li></thingDefs></filter><count>20</count></li>
      <li><filter><thingDefs><li>MMT_SpacerComponent</li></thingDefs></filter><count>16</count></li>
      <li><filter><thingDefs><li>MMT_Paraffins</li></thingDefs></filter><count>8</count></li>
      <li><filter><thingDefs><li>MMT_Plastic</li></thingDefs></filter><count>40</count></li>
    </ingredients>
    <fixedIngredientFilter>
      <thingDefs>
        <li>MMT_Biomatter</li>
        <li>MMT_SpacerComponent</li>
        <li>MMT_Paraffins</li>
        <li>MMT_Plastic</li>
      </thingDefs>
    </fixedIngredientFilter>
    <products>
      <MMT_ArtificialBone>4</MMT_ArtificialBone>
    </products>
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <researchPrerequisite>MMT_Prosthesis_D3</researchPrerequisite>
  </RecipeDef>'''

# 手电照明 mote(对照 CONN_Mote_MoteFlashLight,thingClass 换原版 MoteAttached)
MOTE_DEF = '''  <!-- 手电光圈 mote:由 MyModeTest.HediffComp_LightEffect 在植入者身上维持 -->
  <ThingDef ParentName="MoteBase">
    <defName>MMT_Mote_Flashlight</defName>
    <thingClass>MoteAttached</thingClass>
    <altitudeLayer>MoteOverhead</altitudeLayer>
    <mote />
    <graphicData>
      <texPath>Things/Flashlight/SpotLight_Mote</texPath>
      <shaderType>MoteGlow</shaderType>
    </graphicData>
  </ThingDef>
'''

# ============ 主流程 ============
HEADER = '<?xml version="1.0" encoding="utf-8"?>\n<Defs>\n'
FOOTER = '</Defs>\n'

BASE_DEF = '''  <!-- 仿生部件物品基类:对照 HSK SK_BodyPartBase(剔除 CE 的 Bulk) -->
  <ThingDef Name="MMT_BodyPartBase" Abstract="True">
    <thingClass>ThingWithComps</thingClass>
    <category>Item</category>
    <drawerType>MapMeshOnly</drawerType>
    <useHitPoints>true</useHitPoints>
    <selectable>true</selectable>
    <altitudeLayer>Item</altitudeLayer>
    <tickerType>Never</tickerType>
    <alwaysHaulable>true</alwaysHaulable>
    <isTechHediff>true</isTechHediff>
    <pathCost>10</pathCost>
    <allowedArchonexusCount>1</allowedArchonexusCount>
    <statBases>
      <MaxHitPoints>50</MaxHitPoints>
      <Flammability>0.7</Flammability>
      <Beauty>-4</Beauty>
      <Mass>1.5</Mass>
      <DeteriorationRate>2.0</DeteriorationRate>
    </statBases>
    <tradeTags>
      <li>TechHediff</li>
    </tradeTags>
    <comps>
      <li Class="CompProperties_Forbiddable" />
    </comps>
  </ThingDef>
'''

def write(path, text):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    print("写入 %s" % os.path.relpath(path, RW))

def main():
    item_buckets = {0: [], 1: [], 2: [], 3: []}
    hed_buckets = {0: [], 1: []}
    recipes = []
    hed_done = set()
    n_items = n_heds = n_recs = 0

    for gi, group in enumerate(ext.GROUPS[:-1]):
        for dn in group[2]:
            if dn in VANILLA_ITEMS:
                continue
            if dn in ORASSAN_ONLY:
                continue
            item_cn = get_cn(dn) or dn
            if gi == 13:
                r = emit_neuromod(dn)
                if r:
                    xml, tex = r
                    item_buckets[3].append(xml.replace("%TEX%", copy_tex(tex, dn)))
                    n_items += 1
                continue
            r = emit_item(dn)
            if r is None:
                continue
            xml, tex = r
            xml = xml.replace("%TEX%", copy_tex(tex, dn))
            bucket = 0 if gi in (0, 1, 6) else (1 if gi in (2, 3, 4, 5, 7) else 2)
            item_buckets[bucket].append("  <!-- %s -->\n%s" % (esc(item_cn), xml))
            n_items += 1
            hb = 0 if gi <= 7 else 1
            recs = install_by_item.get(dn, [])
            if recs:
                for rec in recs:
                    ri = emit_install(dn, rec, item_cn)
                    if ri is None:
                        continue
                    rxml, adds = ri
                    recipes.append(rxml)
                    n_recs += 1
                    if adds not in hed_done:
                        hed_done.add(adds)
                        hx = emit_hediff(adds, dn)
                        if hx:
                            hed_buckets[hb].append("  <!-- %s -->\n%s" % (esc(item_cn), hx))
                            n_heds += 1
            elif ("HediffDef", dn) in ext.defs:
                if dn not in hed_done:
                    hed_done.add(dn)
                    hx = emit_hediff(dn, dn)
                    if hx:
                        hed_buckets[hb].append("  <!-- %s -->\n%s" % (esc(item_cn), hx))
                        n_heds += 1
            else:
                if dn not in MATERIALS_ONLY:
                    warn.append("物品无安装配方也无同名 Hediff(纯材料?): " + dn)

    D = os.path.join(RW, "1.7", "Defs")
    write(os.path.join(D, "ThingDefs_Items", "MMT_Items_Bionics.xml"),
          HEADER + "  <!-- 仿生链物品:简易义肢/仿生/生命维持(组一/二/七,HSK 独有件;\n"
          "       原版同名件见 Patches/MMT_Patch_VanillaBionics.xml) -->\n"
          + BASE_DEF + "\n" + "\n\n".join(item_buckets[0]) + "\n" + FOOTER)
    write(os.path.join(D, "ThingDefs_Items", "MMT_Items_BionicsAdv.xml"),
          HEADER + "  <!-- 仿生链物品:功能肋骨/神经机械 GT/外骨骼/辅助 AI/特殊植入(组三~六、八) -->\n"
          + "\n\n".join(item_buckets[1]) + "\n" + FOOTER)
    write(os.path.join(D, "ThingDefs_Items", "MMT_Items_BionicsCONN.xml"),
          HEADER + "  <!-- 仿生链物品:CONN 战斗/精神/生产仿生(组九~十三,CyberneticOrganism_SK) -->\n"
          + "\n\n".join(item_buckets[2]) + "\n" + FOOTER)
    write(os.path.join(D, "ThingDefs_Items", "MMT_Items_Neuromod.xml"),
          HEADER + "  <!-- 神经调制使用品(组十四,非植入;特殊效果由 MMT C# comp 实现) -->\n"
          + "\n\n".join(item_buckets[3]) + "\n" + FOOTER)
    write(os.path.join(D, "HediffDefs", "MMT_Hediffs_Bionics.xml"),
          HEADER + "  <!-- 仿生 Hediff(组一~八):partEfficiency/capMods/statOffsets 数值对照 HSK -->\n"
          + "\n\n".join(hed_buckets[0]) + "\n" + FOOTER)
    write(os.path.join(D, "HediffDefs", "MMT_Hediffs_BionicsCONN.xml"),
          HEADER + "  <!-- CONN 仿生 Hediff(组九~十三);CE/EBF comp 已剔除,CONN 自定义 comp 转 MMT 实现 -->\n"
          + MOTE_DEF + "\n" + "\n\n".join(hed_buckets[1]) + "\n" + FOOTER)
    write(os.path.join(D, "RecipeDefs", "MMT_Recipes_Bionics.xml"),
          HEADER + "  <!-- 仿生安装手术:继承原版 SurgeryInstallBodyPartArtificialBase,recipeUsers 直接挂 Human -->\n"
          + MATERIAL_RECIPES + "\n\n" + "\n\n".join(recipes) + "\n" + FOOTER)
    write(os.path.join(D, "ResearchProjectDefs", "MMT_Research_Bionics.xml"), emit_research())
    pxml, n_patch = emit_patch()
    write(os.path.join(RW, "1.7", "Patches", "MMT_Patch_VanillaBionics.xml"), pxml)

    print("物品 %d 件,Hediff %d 条,安装手术 %d 条,原版补丁 %d 件" % (n_items, n_heds, n_recs, n_patch))
    if warn:
        print("警告 %d 条:" % len(warn))
        for w in sorted(set(warn)):
            print(" -", w)

if __name__ == "__main__":
    main()
