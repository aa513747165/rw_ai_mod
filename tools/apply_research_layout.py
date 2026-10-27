#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""一次性布局工具：按 mapping 重排 MMT 研究的 researchViewX/Y。

原则：同一前置链放在同一水平线（Y 相同、X 递增）；
分支/交叉前置放在正上方或正下方紧靠（X 相同、Y 相邻）。
用法：python tools/apply_research_layout.py
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RESEARCH_GLOB = os.path.join(ROOT, "1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml")

# defName -> (x, y)
LAYOUT = {
    # y=0 木工
    "MMT_Woodworking": (0, 0),
    "MMT_CharcoalBurning": (1, 0),
    "MMT_ElectricWoodworking": (2, 0),
    "MMT_ArtSupplies": (4, 0),
    # y=1 种植（攻城武器紧靠其前置 木工加工 正下方）
    "MMT_SiegeWeapons": (0, 1),
    "MMT_AgricultureBasic": (2, 1),
    "MMT_MedicalPlants_A1": (3, 1),
    "MMT_MedicalPlants_A2": (4, 1),
    "MMT_DrugPlants_A1": (6, 1),
    "MMT_DrugPlants_A2": (7, 1),
    # y=2 医疗
    "MMT_Medicine_A1": (0, 2),
    "MMT_Medicine_A2": (1, 2),
    "MMT_Medicine_B1": (2, 2),
    "MMT_Medicine_B2": (3, 2),
    "MMT_Medicine_D1": (4, 2),
    # y=3 毒品
    "MMT_Drugs_A1": (0, 3),
    "MMT_Drugs_B1": (1, 3),
    "MMT_Drugs_C2": (2, 3),
    "MMT_Drugs_C3": (3, 3),
    # y=4 金属冶炼
    "MMT_BasicMetalworking": (0, 4),
    "MMT_ElectricSmelting": (1, 4),
    "MMT_AdvancedMetalRefining": (2, 4),
    "MMT_AdvancedAlloys": (3, 4),
    # y=5 石化
    "MMT_BasicPetrochemistry": (0, 5),
    "MMT_ElectricPetrochemistry": (1, 5),
    "MMT_AdvancedPetrochemistry": (2, 5),
    "MMT_OxidizedFuel": (3, 5),
    # y=6 玻璃/混凝土（强化玻璃紧靠交叉前置 基础石化 下方）
    "MMT_Glassworking": (0, 6),
    "MMT_ReinforcedGlassTech": (1, 6),
    "MMT_ConcreteBasic": (3, 6),
    "MMT_ConcreteAdvanced": (4, 6),
    # y=7 矿料/手榴弹（手榴弹 I 紧靠其前置 混凝土技术 正下方）
    "MMT_OreSifting": (0, 7),
    "MMT_AutoOreProcessing": (1, 7),
    "MMT_AutoBoost": (2, 7),
    "MMT_GrenadesBasic": (3, 7),
    "MMT_GrenadesAdvanced": (4, 7),
    # y=8 零件/物质转换
    "MMT_ComponentCrafting": (0, 8),
    "MMT_AdvancedComponents": (1, 8),
    "MMT_UltraComponents": (2, 8),
    "MMT_MatterConversion2": (3, 8),
    "MMT_MatterConversion3": (4, 8),
    # y=9 物质转换 I 紧靠 物质转换 II 正下方；研究设施链
    "MMT_MatterConversion1": (3, 9),
    "MMT_LabTerminalTech": (5, 9),
    "MMT_MultiAnalyzerTech": (6, 9),
    "MMT_LabStationTech": (7, 9),
    # y=10 电子
    "MMT_BasicElectronics": (0, 10),
    "MMT_IntegratedCircuitTech": (1, 10),
    "MMT_ElectricElectronics": (2, 10),
    "MMT_PersonaCoreCrafting": (3, 10),
    "MMT_SuperFabric": (5, 10),
    # y=11 电子下方支链（机器人/近战 IV/核能 各自紧靠其前置正下方）
    "MMT_Robotics1": (0, 11),
    "MMT_MeleeAdvanced": (1, 11),
    "MMT_NuclearPower": (2, 11),
    "MMT_DeployableShieldTech": (3, 11),
    "MMT_RechargeableTech": (4, 11),
    # y=12 枪械
    "MMT_GunsAdvanced": (0, 12),
    "MMT_RiflesAdvanced": (1, 12),
    "MMT_RiflesMaster": (2, 12),
    "MMT_SnipersAdvanced": (4, 12),
    # y=13 激光武器
    "MMT_DirectedEnergyWeapons1": (0, 13),
    "MMT_DirectedEnergyWeapons2": (1, 13),
    # y=14 近战终局（紧靠 近战 IV 正下方）
    "MMT_ArchotechWeapons": (1, 14),
    # y=15 炮塔/火炮
    "MMT_GrenadeLaunchers": (0, 15),
    "MMT_Artillery2": (1, 15),
    "MMT_Autoturrets3": (2, 15),
    "MMT_HeavyMachineGunTurret": (4, 15),
    "MMT_HeavyTurrets2": (5, 15),
    "MMT_HeavyTurrets3": (6, 15),
    # y=16 火箭弹
    "MMT_Rocketry1": (0, 16),
    "MMT_Rocketry2": (1, 16),
    "MMT_Rocketry3": (2, 16),
}

BLOCK_RE = re.compile(r"<ResearchProjectDef>.*?</ResearchProjectDef>", re.S)
NAME_RE = re.compile(r"<defName>([^<]+)</defName>")
X_RE = re.compile(r"<researchViewX>[^<]*</researchViewX>")
Y_RE = re.compile(r"<researchViewY>[^<]*</researchViewY>")


def fmt(v):
    return "{:.2f}".format(v)


def main():
    seen = set()
    changed_files = 0
    for path in sorted(glob.glob(RESEARCH_GLOB)):
        with open(path, "r", encoding="utf-8-sig") as f:
            text = f.read()

        def fix_block(m):
            block = m.group(0)
            name_m = NAME_RE.search(block)
            if not name_m or name_m.group(1) not in LAYOUT:
                return block
            name = name_m.group(1)
            x, y = LAYOUT[name]
            seen.add(name)
            block = X_RE.sub("<researchViewX>{}</researchViewX>".format(fmt(x)), block)
            block = Y_RE.sub("<researchViewY>{}</researchViewY>".format(fmt(y)), block)
            return block

        new_text = BLOCK_RE.sub(fix_block, text)
        if new_text != text:
            with open(path, "w", encoding="utf-8") as f:
                f.write(new_text)
            changed_files += 1
            print("已更新: {}".format(os.path.relpath(path, ROOT)))

    missing = set(LAYOUT) - seen
    if missing:
        print("警告：以下 defName 未在 XML 中找到: {}".format(sorted(missing)), file=sys.stderr)
        return 1
    print("完成：{} 项研究坐标已更新，涉及 {} 个文件".format(len(seen), changed_files))
    return 0


if __name__ == "__main__":
    sys.exit(main())
