#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""布局工具：按 mapping 重排 MMT 研究的 researchViewX/Y（唯一事实来源，validate_mod 也引用本表）。

设计（2026-10 重建，96 项 MMT 研究 + 7 项补丁注入的原版研究，宽 13 列 × 9 行，填充率 88%）：
- 最长链（12 节点：医药 I-IV → 仿生安装 C2-E2）拉直为 y=2 的 13 节点主干线，定义画布宽度；
- 同一系列保持在同一水平线（Y 相同、X 递增），系列间留 1 列间隔；
- 跨系列前置一律满足 依赖 X > 前置 X（箭头只向右/右下，无回退线），含补丁注入的原版节点（VANILLA_LAYOUT）；
- 无法同排放置的系列放上下相邻行。
用法：python tools/apply_research_layout.py
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RESEARCH_GLOB = os.path.join(ROOT, "1.7/Defs/ResearchProjectDefs/MMT_Research_*.xml")

# defName -> (x, y)，宽 13 列（x 0-12）× 9 行（y 0-8）
LAYOUT = {
    # y=0 基础采集：木工 | 艺术 | 攻城 | 医药 V（主干线 D3/D4 的交叉前置）| 农业 | 草药种植 | 超级织物
    "MMT_Woodworking": (0, 0),
    "MMT_CharcoalBurning": (1, 0),
    "MMT_ElectricWoodworking": (2, 0),
    "MMT_ArtSupplies": (4, 0),
    "MMT_SiegeWeapons": (6, 0),
    "MMT_Medicine_D1": (7, 0),
    "MMT_AgricultureBasic": (8, 0),
    "MMT_MedicalPlants_A1": (9, 0),
    "MMT_MedicalPlants_A2": (10, 0),
    "MMT_SuperFabric": (12, 0),
    # y=1 毒品链（与原版节点混排成一条直线）：毒植 | 毒品 I | [原版 精神茶酿造] | 毒品 III | [原版 毒品生产]
    # | 毒品 V/VI | [原版 精神叶精炼/清醒丸/青霉素/活力水]（原版坐标由 MMT_Patch_VanillaDrugs.xml 注入，见 VANILLA_LAYOUT）
    "MMT_DrugPlants_A1": (0, 1),
    "MMT_DrugPlants_A2": (1, 1),
    "MMT_Drugs_A1": (2, 1),
    "MMT_Drugs_B1": (4, 1),
    "MMT_Drugs_C2": (6, 1),
    "MMT_Drugs_C3": (7, 1),
    # y=2 主干线（13 节点直线）：医药 I-IV → 仿生安装 C1-E2（全图最长链）
    "MMT_Medicine_A1": (0, 2),
    "MMT_Medicine_A2": (1, 2),
    "MMT_Medicine_B1": (2, 2),
    "MMT_Medicine_B2": (3, 2),
    "MMT_ProsthesisInstall_C1": (4, 2),
    "MMT_ProsthesisInstall_C2": (5, 2),
    "MMT_ProsthesisInstall_C3": (6, 2),
    "MMT_ProsthesisInstall_D1": (7, 2),
    "MMT_ProsthesisInstall_D2": (8, 2),
    "MMT_ProsthesisInstall_D3": (9, 2),
    "MMT_ProsthesisInstall_D4": (10, 2),
    "MMT_ProsthesisInstall_E1": (11, 2),
    "MMT_ProsthesisInstall_E2": (12, 2),
    # y=3 仿生制造：E4-E6（E1-E3 同列左侧起点）| B1 紧靠前置 医药 III 下方；D2 在 D1 右、安装 C3 右下
    "MMT_Prosthesis_E4": (0, 3),
    "MMT_Prosthesis_E5": (1, 3),
    "MMT_Prosthesis_E6": (2, 3),
    "MMT_Prosthesis_B1": (3, 3),
    "MMT_Prosthesis_C1": (4, 3),
    "MMT_Prosthesis_C2": (5, 3),
    "MMT_Prosthesis_D3": (6, 3),
    "MMT_Prosthesis_D4": (7, 3),
    "MMT_Prosthesis_D1": (8, 3),
    "MMT_Prosthesis_D2": (9, 3),
    "MMT_Prosthesis_E1": (10, 3),
    "MMT_Prosthesis_E2": (11, 3),
    "MMT_Prosthesis_E3": (12, 3),
    # y=4 工业原料：石化 | [原版 医药生产 x4] | 金属 | 混凝土 | 手榴弹
    "MMT_BasicPetrochemistry": (0, 4),
    "MMT_ElectricPetrochemistry": (1, 4),
    "MMT_AdvancedPetrochemistry": (2, 4),
    "MMT_OxidizedFuel": (3, 4),
    "MMT_BasicMetalworking": (5, 4),
    "MMT_ElectricSmelting": (6, 4),
    "MMT_AdvancedMetalRefining": (7, 4),
    "MMT_AdvancedAlloys": (8, 4),
    "MMT_ConcreteBasic": (9, 4),
    "MMT_ConcreteAdvanced": (10, 4),
    "MMT_GrenadesBasic": (11, 4),
    "MMT_GrenadesAdvanced": (12, 4),
    # y=5 加工：玻璃（强化玻璃在前置 基础石化 右下）| 矿料 | 零件（超级零件在 高级合金 右下）| 精密筛选
    "MMT_Glassworking": (0, 5),
    "MMT_ReinforcedGlassTech": (1, 5),
    "MMT_OreSifting": (3, 5),
    "MMT_AutoOreProcessing": (4, 5),
    "MMT_AutoBoost": (5, 5),
    "MMT_ComponentCrafting": (7, 5),
    "MMT_AdvancedComponents": (8, 5),
    "MMT_UltraComponents": (9, 5),
    "MMT_PrecisionSifting": (10, 5),
    # y=6 高科技：电子 | 机器人 | 展开护盾 | 物质转换 I-III → 传送（终点收在右上角）；充能技术
    "MMT_BasicElectronics": (0, 6),
    "MMT_IntegratedCircuitTech": (1, 6),
    "MMT_ElectricElectronics": (2, 6),
    "MMT_PersonaCoreCrafting": (3, 6),
    "MMT_Robotics1": (5, 6),
    "MMT_DeployableShieldTech": (7, 6),
    "MMT_MatterConversion1": (8, 6),
    "MMT_RechargeableTech": (9, 6),
    "MMT_MatterConversion2": (10, 6),
    "MMT_MatterConversion3": (11, 6),
    "MMT_Teleportation": (12, 6),
    # y=7 武器与杂项高科技：远古 | 枪械 | 核能 | 研究设施 | 近战 IV | 激光 | 灵能护盾（充能右下）
    "MMT_ArchotechWeapons": (0, 7),
    "MMT_GunsAdvanced": (2, 7),
    "MMT_RiflesAdvanced": (3, 7),
    "MMT_RiflesMaster": (4, 7),
    "MMT_NuclearPower": (5, 7),
    "MMT_LabTerminalTech": (6, 7),
    "MMT_MultiAnalyzerTech": (7, 7),
    "MMT_LabStationTech": (8, 7),
    "MMT_MeleeAdvanced": (9, 7),
    "MMT_DirectedEnergyWeapons1": (10, 7),
    "MMT_DirectedEnergyWeapons2": (11, 7),
    "MMT_PsychicShieldTech": (12, 7),
    # y=8 重武器：狙击 | 榴弹炮/火炮/自动炮塔 | 火箭弹 | 重机枪塔/重型炮塔
    "MMT_SnipersAdvanced": (0, 8),
    "MMT_GrenadeLaunchers": (2, 8),
    "MMT_Artillery2": (3, 8),
    "MMT_Autoturrets3": (4, 8),
    "MMT_Rocketry1": (6, 8),
    "MMT_Rocketry2": (7, 8),
    "MMT_Rocketry3": (8, 8),
    "MMT_HeavyMachineGunTurret": (10, 8),
    "MMT_HeavyTurrets2": (11, 8),
    "MMT_HeavyTurrets3": (12, 8),
}

# 经 Patches 注入 MMT 标签页的原版研究坐标（唯一事实来源；补丁文件里的 researchViewX/Y 必须与这里一致，
# tools/validate/research.py 会同时校验 MMT 与原版节点互不重叠）
VANILLA_LAYOUT = {
    # y=1 毒品链（MMT_Patch_VanillaDrugs.xml）：精神茶酿造 → 毒品生产 → 精神叶精炼/清醒丸/青霉素 → 活力水
    "PsychoidBrewing": (3, 1),
    "DrugProduction": (5, 1),
    "PsychiteRefining": (8, 1),
    "WakeUpProduction": (9, 1),
    "PenoxycylineProduction": (10, 1),
    "GoJuiceProduction": (11, 1),
    # y=4 工业原料行（MMT_Patch_VanillaMedicine.xml）：医药生产（前置 MMT_Medicine_B2，x3）
    "MedicineProduction": (4, 4),
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
