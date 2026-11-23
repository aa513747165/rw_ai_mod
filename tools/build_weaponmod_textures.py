#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""基于现有附魔模块贴图批量生成新模块贴图（HSV 色相偏移重配色）。

只旋转"有颜色"的像素（饱和度/明度高于阈值），保留黑色描边与低饱和金属灰，
alpha 通道不动。产物尺寸与源图一致（RGBA PNG），分别写入
1.7/Textures/Things/Item/MMT/WeaponMod/ 与 .../ApparelMod/。

用法：
    python tools/build_weaponmod_textures.py          # 生成全部产物
    python tools/build_weaponmod_textures.py --list   # 只打印计划，不写入
"""
import colorsys
import os
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WEAPON_DIR = os.path.join(ROOT, "1.7", "Textures", "Things", "Item", "MMT", "WeaponMod")
APPAREL_DIR = os.path.join(ROOT, "1.7", "Textures", "Things", "Item", "MMT", "ApparelMod")

# 参与改色的像素阈值：低于该饱和度视为描边/金属灰，低于该明度视为阴影，均保持不变
SAT_THRESHOLD = 0.25
VAL_THRESHOLD = 0.15

# (目标目录, 输出文件名, 源文件名, 目标色相 0-360, 选项)
# 选项：sat_threshold / tint(低饱和区染色) / tint_sat / sat_boost，语义同 build_shield_textures.py
JOBS = [
    # ---- 武器模块（附魔3 扩展批次） ----
    (WEAPON_DIR, "MMT_WeaponModLongRange.png", "MMT_WeaponModPierce.png", 120, {}),
    (WEAPON_DIR, "MMT_WeaponModDeadeye.png", "MMT_WeaponModAccurate.png", 280, {}),
    (WEAPON_DIR, "MMT_WeaponModMagazine.png", "MMT_WeaponModSwift.png", 30, {}),
    (WEAPON_DIR, "MMT_WeaponModBlinding.png", "MMT_WeaponModIncendiary.png", 50, {"sat_boost": 1.2}),
    (WEAPON_DIR, "MMT_WeaponModCorrode.png", "MMT_WeaponModToxic.png", 100, {}),
    (WEAPON_DIR, "MMT_WeaponModBleed.png", "MMT_WeaponModLifeLeech.png", 0, {}),
    (WEAPON_DIR, "MMT_WeaponModSmoke.png", "MMT_WeaponModGas.png", 210, {"tint": True, "tint_sat": 0.12}),
    (WEAPON_DIR, "MMT_WeaponModSlaughter.png", "MMT_WeaponModHeavy.png", 350, {}),
    (WEAPON_DIR, "MMT_WeaponModDemolition.png", "MMT_WeaponModExplode.png", 25, {}),
    (WEAPON_DIR, "MMT_WeaponModVampire.png", "MMT_WeaponModLifeLeech.png", 320, {}),
    (WEAPON_DIR, "MMT_WeaponModDisruptor.png", "MMT_WeaponModEmp.png", 260, {}),
    (WEAPON_DIR, "MMT_WeaponModBrutal.png", "MMT_WeaponModDeadly.png", 10, {"sat_boost": 1.2}),
    (WEAPON_DIR, "MMT_WeaponModTerror.png", "MMT_WeaponModStormCall.png", 285, {}),
    (WEAPON_DIR, "MMT_WeaponModRangedLeech.png", "MMT_WeaponModLifeLeech.png", 170, {}),
    (WEAPON_DIR, "MMT_WeaponModCorpseBurst.png", "MMT_WeaponModExplode.png", 90, {}),
    # ---- 防具模块（附魔3 扩展批次） ----
    (APPAREL_DIR, "MMT_ApparelModNumbing.png", "MMT_ApparelModRecovery.png", 190, {}),
    (APPAREL_DIR, "MMT_ApparelModSteady.png", "MMT_ApparelModDiligent.png", 35, {}),
    (APPAREL_DIR, "MMT_ApparelModGrit.png", "MMT_ApparelModBulwark.png", 20, {}),
    (APPAREL_DIR, "MMT_ApparelModImmunity.png", "MMT_ApparelModRecovery.png", 130, {}),
    (APPAREL_DIR, "MMT_ApparelModMindWard.png", "MMT_ApparelModEmpShield.png", 275, {}),
    (APPAREL_DIR, "MMT_ApparelModRegen.png", "MMT_ApparelModRecovery.png", 150, {}),
]


def recolor(src_path, target_hue, sat_threshold=SAT_THRESHOLD, tint=False,
            tint_sat=0.35, sat_boost=1.0):
    """把源图的有色像素旋转到目标色相，返回 RGBA Image。"""
    img = Image.open(src_path).convert("RGBA")
    target = target_hue / 360.0
    px = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            hh, ss, vv = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)
            if vv < VAL_THRESHOLD:
                continue
            if ss >= sat_threshold:
                ss = min(1.0, ss * sat_boost)
            elif tint:
                ss = max(ss, tint_sat)
            else:
                continue
            nr, ng, nb = colorsys.hsv_to_rgb(target, ss, vv)
            px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    return img


def validate(img, out_path, src_size):
    if img.size != src_size:
        raise ValueError(f"{out_path}: 尺寸异常 {img.size}，应与源图 {src_size} 一致")
    if img.mode != "RGBA":
        raise ValueError(f"{out_path}: 模式异常 {img.mode}")
    if img.getchannel("A").getextrema()[1] == 0:
        raise ValueError(f"{out_path}: 全透明，产物无效")


def main():
    list_only = "--list" in sys.argv
    for out_dir, out_name, src_name, hue, opts in JOBS:
        src_path = os.path.join(out_dir, src_name)
        out_path = os.path.join(out_dir, out_name)
        print(f"{src_name} -> {out_name} (色相 {hue}, {opts})")
        if list_only:
            continue
        if not os.path.isfile(src_path):
            raise FileNotFoundError(f"源图缺失: {src_path}")
        src_size = Image.open(src_path).size
        img = recolor(src_path, hue, **opts)
        validate(img, out_path, src_size)
        img.save(out_path, "PNG")
    if not list_only:
        print(f"完成，共 {len(JOBS)} 张")


if __name__ == "__main__":
    main()
