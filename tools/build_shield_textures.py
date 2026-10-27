#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""基于 Shield/ 源图批量生成 MMT 护盾贴图（HSV 色相偏移重配色）。

只旋转"有颜色"的像素（饱和度/明度高于阈值），保留黑色描边与低饱和金属灰，
alpha 通道不动。产物为 256x256 RGBA PNG，写入 1.7/Textures/Things/Item/MMT/Shield/。

用法：
    python tools/build_shield_textures.py          # 生成全部产物
    python tools/build_shield_textures.py --list   # 只打印计划，不写入
"""
import colorsys
import os
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC_DIR = os.path.join(ROOT, "Shield")
OUT_DIR = os.path.join(ROOT, "1.7", "Textures", "Things", "Item", "MMT", "Shield")

# 参与改色的像素阈值：低于该饱和度视为描边/金属灰，低于该明度视为阴影，均保持不变
SAT_THRESHOLD = 0.25
VAL_THRESHOLD = 0.15

# 目标色相（0-360）
HUE_TECH_BLUE = 205
HUE_JADE = 150
HUE_VIOLET = 275
HUE_PSYCHIC_YELLOW = 60

# (输出文件名, 源文件名, 目标色相, 选项)
# 选项：
#   sat_threshold  参与改色的最低饱和度（默认 SAT_THRESHOLD，低饱和源图可调低）
#   tint           True 时对"低饱和但非暗色"的像素染色（色相=目标色，饱和度补足到 tint_sat，明度不变），
#                  用于灰环/浅色球体等本身没什么颜色的区域
#   tint_sat       染色时补足的饱和度（默认 0.35）
#   sat_boost      改色后饱和度倍率（默认 1.0，>1 更艳）
#   alpha_glow     True 时用于"纯黑+alpha"的光效贴图（颜色信息全在透明度里）：
#                  所有非透明像素直接设为目标色的亮饱和 RGB，alpha 原样保留
JOBS = [
    # 腰带
    ("MMT_ShieldBeltTech.png", "ShieldBeltArcho.png", HUE_TECH_BLUE, {"tint": True}),
    ("MMT_ShieldBeltJade.png", "ShieldBeltPsychic.png", HUE_JADE, {}),
    ("MMT_ShieldBeltViolet.png", "ShieldBeltVoid.png", HUE_VIOLET, {}),
    # 气泡
    ("MMT_ShieldBubbleTech.png", "ShieldBubbleArcho.png", HUE_TECH_BLUE, {}),
    ("MMT_ShieldBubbleJade.png", "ShieldBubbleBlood.png", HUE_JADE, {}),
    ("MMT_ShieldBubbleViolet.png", "ShieldBubbleVoid.png", HUE_VIOLET,
     {"alpha_glow": True}),
    ("MMT_ShieldBubblePsychic.png", "ShieldBubbleArcho.png", HUE_PSYCHIC_YELLOW,
     {"sat_boost": 1.3}),
]


def recolor(src_path, target_hue, sat_threshold=SAT_THRESHOLD, tint=False,
            tint_sat=0.35, sat_boost=1.0, alpha_glow=False):
    """把源图的有色像素旋转到目标色相，返回 RGBA Image。"""
    img = Image.open(src_path).convert("RGBA")
    target = target_hue / 360.0
    px = img.load()
    w, h = img.size
    glow_rgb = None
    if alpha_glow:
        gr, gg, gb = colorsys.hsv_to_rgb(target, 0.6, 1.0)
        glow_rgb = (int(gr * 255), int(gg * 255), int(gb * 255))
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            if glow_rgb is not None:
                px[x, y] = (glow_rgb[0], glow_rgb[1], glow_rgb[2], a)
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


def validate(img, out_path):
    if img.size != (256, 256):
        raise ValueError(f"{out_path}: 尺寸异常 {img.size}")
    if img.mode != "RGBA":
        raise ValueError(f"{out_path}: 模式异常 {img.mode}")
    if img.getchannel("A").getextrema()[1] == 0:
        raise ValueError(f"{out_path}: 全透明，产物无效")


def main():
    list_only = "--list" in sys.argv
    for out_name, src_name, hue, opts in JOBS:
        src_path = os.path.join(SRC_DIR, src_name)
        out_path = os.path.join(OUT_DIR, out_name)
        print(f"{src_name} -> {out_name} (色相 {hue}, {opts})")
        if list_only:
            continue
        if not os.path.isfile(src_path):
            raise FileNotFoundError(f"源图缺失: {src_path}")
        img = recolor(src_path, hue, **opts)
        validate(img, out_path)
        os.makedirs(OUT_DIR, exist_ok=True)
        img.save(out_path, "PNG")
    if not list_only:
        print(f"完成，共 {len(JOBS)} 张 -> {OUT_DIR}")


if __name__ == "__main__":
    main()
