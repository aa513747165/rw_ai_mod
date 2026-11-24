# -*- coding: utf-8 -*-
# 一次性贴图生成：熔火元素 Boss（MMT_FireElementalLord）与小火元素（MMT_FireElementalLesser）。
# 俯视 3/4、厚深色描边、熔岩裂纹 + 火焰鬃毛，符合 rw-texture-design 规范。
# 输出到 1.7/Textures/Things/Pawn/FireElemental/，每个角色 _south/_east/_north 三张。
import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                   "1.7", "Textures", "Things", "Pawn", "FireElemental")

OUTLINE = (42, 20, 16, 255)        # 深棕黑描边（带色相，不用纯黑）
ROCK_DARK = (46, 34, 30, 255)      # 岩体暗部
ROCK_MID = (74, 56, 48, 255)       # 岩体固有色
ROCK_LIGHT = (104, 80, 66, 255)    # 岩体亮部（光源左上）
LAVA = (226, 88, 34, 255)          # 熔岩橙
LAVA_BRIGHT = (255, 154, 60, 255)  # 熔岩亮橙
CORE = (255, 215, 94, 255)         # 核心亮黄
CORE_WHITE = (255, 243, 208, 255)  # 核心白炽
FLAME_OUTER = (206, 74, 24, 255)   # 火焰外缘
FLAME_MID = (244, 128, 38, 255)    # 火焰中层
FLAME_INNER = (255, 208, 92, 255)  # 火焰内芯


def shade(color, f):
    return (int(color[0] * f), int(color[1] * f), int(color[2] * f), color[3])


def blob_points(cx, cy, rx, ry, n=14, jitter=0.18, seed=0):
    rnd = random.Random(seed)
    pts = []
    for i in range(n):
        a = 2 * math.pi * i / n
        r = 1.0 + rnd.uniform(-jitter, jitter)
        pts.append((cx + rx * r * math.cos(a), cy + ry * r * math.sin(a)))
    return pts


def draw_rock(img, pts, seed=0, light_dir=(-0.6, -0.8)):
    """岩块：描边剪影 + 固有色 + 左上亮部 + 右下暗部 + 熔岩裂纹。"""
    d = ImageDraw.Draw(img)
    cx = sum(p[0] for p in pts) / len(pts)
    cy = sum(p[1] for p in pts) / len(pts)

    def scaled(f):
        return [(cx + (p[0] - cx) * f, cy + (p[1] - cy) * f) for p in pts]

    d.polygon(scaled(1.09), fill=OUTLINE)          # 描边
    d.polygon(scaled(1.0), fill=ROCK_MID)          # 固有色
    # 亮部：朝左上偏移的小一号形
    ox, oy = light_dir[0] * 5, light_dir[1] * 5
    light_pts = [(cx + (p[0] - cx) * 0.72 + ox, cy + (p[1] - cy) * 0.72 + oy) for p in pts]
    d.polygon(light_pts, fill=ROCK_LIGHT)
    # 暗部：朝右下偏移
    dark_pts = [(cx + (p[0] - cx) * 0.85 - ox, cy + (p[1] - cy) * 0.85 - oy) for p in pts]
    d.polygon(dark_pts, fill=ROCK_DARK)
    d.polygon(light_pts, fill=ROCK_MID)
    # 熔岩裂纹：从中心向外随机折线
    rnd = random.Random(seed + 7)
    max_r = max(abs(p[0] - cx) for p in pts)
    for _ in range(rnd.randint(2, 4)):
        a = rnd.uniform(0, 2 * math.pi)
        x, y = cx + rnd.uniform(-0.2, 0.2) * max_r, cy + rnd.uniform(-0.2, 0.2) * max_r
        line = [(x, y)]
        steps = rnd.randint(3, 5)
        for _ in range(steps):
            a += rnd.uniform(-0.9, 0.9)
            x += math.cos(a) * max_r * rnd.uniform(0.18, 0.3)
            y += math.sin(a) * max_r * rnd.uniform(0.18, 0.3)
            line.append((x, y))
        d.line(line, fill=shade(LAVA, 0.55), width=4)
        d.line(line, fill=LAVA, width=2)
        d.line(line, fill=LAVA_BRIGHT, width=1)


def draw_flame(img, cx, cy, w, h, seed=0):
    """火焰泪滴：外橙→中橙→内黄，底部在 (cx, cy)，向上燃烧。"""
    d = ImageDraw.Draw(img)
    rnd = random.Random(seed)
    lean = rnd.uniform(-0.25, 0.25)

    def layer(f, color, up=0.0):
        hw, hh = w * f / 2, h * f
        tipx = cx + lean * hh
        tipy = cy - hh - up
        pts = [(cx - hw, cy - up)]
        n = 8
        for i in range(1, n + 1):  # 左缘上行到尖端
            t = i / n
            x = cx - hw * (1 - t ** 1.6) + lean * hh * t
            y = cy - hh * t
            pts.append((x, y))
        pts.append((tipx, tipy - hh * 0.12))
        for i in range(n, 0, -1):  # 右缘下行
            t = i / n
            x = cx + hw * (1 - t ** 1.6) + lean * hh * t
            y = cy - hh * t
            pts.append((x, y))
        pts.append((cx + hw, cy - up))
        d.polygon(pts, fill=color)

    layer(1.18, OUTLINE)
    layer(1.0, FLAME_OUTER)
    layer(0.66, FLAME_MID, up=1)
    layer(0.36, FLAME_INNER, up=2)


def draw_core(img, cx, cy, r):
    """熔岩核心：橙光晕 + 黄核 + 白炽点。"""
    glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow)
    gd.ellipse([cx - r * 1.9, cy - r * 1.9, cx + r * 1.9, cy + r * 1.9], fill=(226, 88, 34, 90))
    glow = glow.filter(ImageFilter.GaussianBlur(6))
    img.alpha_composite(glow)
    d = ImageDraw.Draw(img)
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=LAVA)
    d.ellipse([cx - r * 0.66, cy - r * 0.66, cx + r * 0.66, cy + r * 0.66], fill=CORE)
    d.ellipse([cx - r * 0.3, cy - r * 0.3, cx + r * 0.3, cy + r * 0.3], fill=CORE_WHITE)


def draw_eyes(img, cx, cy, gap, r, angry=True):
    d = ImageDraw.Draw(img)
    for sx in (-1, 1):
        ex = cx + sx * r * 2.2
        d.ellipse([ex - r * 1.6, cy - r * 1.6, ex + r * 1.6, cy + r * 1.6], fill=OUTLINE)
        d.ellipse([ex - r, cy - r, ex + r, cy + r], fill=CORE)
        d.ellipse([ex - r * 0.5, cy - r * 0.5, ex + r * 0.5, cy + r * 0.5], fill=CORE_WHITE)
    if angry:  # 怒眉：两条内低外高的岩脊
        for sx in (-1, 1):
            x0 = cx + sx * r * 1.2
            x1 = cx + sx * r * 3.4
            y0 = cy - r * 3.0
            y1 = cy - r * 1.6
            pts = [(x0, y0), (x1, y1 - r), (x1, y1 + r * 0.8), (x0, y0 + r * 1.2)]
            d.polygon(pts, fill=OUTLINE)


def boss_south(size=256, seed=11, facing="south"):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    u = size / 256.0  # 单位缩放
    cx = size / 2
    # 背后的火焰鬃毛（先画，压在肩后）
    for i, (fx, fy, fw, fh) in enumerate([
            (cx - 62 * u, 118 * u, 40 * u, 58 * u),
            (cx - 22 * u, 96 * u, 44 * u, 66 * u),
            (cx + 20 * u, 94 * u, 46 * u, 70 * u),
            (cx + 60 * u, 116 * u, 40 * u, 56 * u),
            (cx + 2 * u, 78 * u, 40 * u, 58 * u)]):
        draw_flame(img, fx, fy, fw, fh, seed=seed + i)
    # 腿（朝南，画面下方）
    draw_rock(img, blob_points(cx - 34 * u, 196 * u, 26 * u, 34 * u, seed=seed + 1), seed + 1)
    draw_rock(img, blob_points(cx + 34 * u, 196 * u, 26 * u, 34 * u, seed=seed + 2), seed + 2)
    # 手臂 + 拳头
    draw_rock(img, blob_points(cx - 78 * u, 150 * u, 24 * u, 40 * u, seed=seed + 3), seed + 3)
    draw_rock(img, blob_points(cx + 78 * u, 150 * u, 24 * u, 40 * u, seed=seed + 4), seed + 4)
    draw_rock(img, blob_points(cx - 84 * u, 190 * u, 28 * u, 26 * u, seed=seed + 5), seed + 5)
    draw_rock(img, blob_points(cx + 84 * u, 190 * u, 28 * u, 26 * u, seed=seed + 6), seed + 6)
    # 肩甲
    draw_rock(img, blob_points(cx - 56 * u, 118 * u, 34 * u, 26 * u, seed=seed + 7), seed + 7)
    draw_rock(img, blob_points(cx + 56 * u, 118 * u, 34 * u, 26 * u, seed=seed + 8), seed + 8)
    # 躯干（上宽下窄）
    torso = [(cx - 52 * u, 104 * u), (cx + 52 * u, 104 * u),
             (cx + 58 * u, 150 * u), (cx + 36 * u, 190 * u),
             (cx - 36 * u, 190 * u), (cx - 58 * u, 150 * u)]
    draw_rock(img, torso, seed + 9)
    # 胸口熔岩核心（north 背面不画）
    if facing != "north":
        draw_core(img, cx, 142 * u, 15 * u)
    # 头 + 岩冠尖刺
    draw_rock(img, blob_points(cx, 86 * u, 30 * u, 24 * u, seed=seed + 10), seed + 10)
    d = ImageDraw.Draw(img)
    for sx, hh in ((-1.6, 16), (-0.7, 22), (0.2, 26), (1.1, 20), (1.8, 14)):
        bx = cx + sx * 12 * u
        by = 70 * u
        d.polygon([(bx - 7 * u, by + 8 * u), (bx, by - hh * u), (bx + 7 * u, by + 8 * u)],
                  fill=OUTLINE)
        d.polygon([(bx - 4 * u, by + 6 * u), (bx, by - hh * u + 5 * u), (bx + 4 * u, by + 6 * u)],
                  fill=ROCK_MID)
    if facing == "south":
        draw_eyes(img, cx, 88 * u, gap=10 * u, r=4.5 * u)
    else:
        draw_core(img, cx, 150 * u, 9 * u)  # 背面：背部小熔核
    return img


def lesser_south(size=128, seed=31, facing="south"):
    """小火元素：悬浮裂纹岩核 + 周身火焰 + 双眼。"""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    u = size / 128.0
    cx = size / 2
    # 环绕火焰
    for i, (fx, fy, fw, fh) in enumerate([
            (cx - 26 * u, 78 * u, 22 * u, 30 * u),
            (cx - 8 * u, 62 * u, 24 * u, 36 * u),
            (cx + 12 * u, 64 * u, 22 * u, 34 * u),
            (cx + 28 * u, 80 * u, 20 * u, 28 * u)]):
        draw_flame(img, fx, fy, fw, fh, seed=seed + i)
    # 主岩核
    draw_rock(img, blob_points(cx, 84 * u, 30 * u, 28 * u, n=12, jitter=0.22, seed=seed + 5), seed + 5)
    # 小碎岩（悬浮感）
    draw_rock(img, blob_points(cx - 34 * u, 100 * u, 9 * u, 8 * u, n=8, seed=seed + 6), seed + 6)
    draw_rock(img, blob_points(cx + 36 * u, 104 * u, 8 * u, 7 * u, n=8, seed=seed + 7), seed + 7)
    if facing == "south":
        draw_eyes(img, cx, 82 * u, gap=6 * u, r=3 * u, angry=False)
    else:
        draw_core(img, cx, 88 * u, 6 * u)
    return img


def derive(img, facing):
    """由 south 派生其他朝向：east=水平镜像（对称生物可接受），north=去正面细节由调用方重画。"""
    if facing == "east":
        return img.transpose(Image.FLIP_LEFT_RIGHT)
    return img


def save(img, name):
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, name)
    img.save(path)
    print("written:", path, img.size)


def main():
    random.seed(20261124)
    boss_s = boss_south(facing="south")
    boss_n = boss_south(facing="north", seed=41)
    save(boss_s, "MMT_FireElementalLord_south.png")
    save(derive(boss_s, "east"), "MMT_FireElementalLord_east.png")
    save(boss_n, "MMT_FireElementalLord_north.png")

    les_s = lesser_south(facing="south")
    les_n = lesser_south(facing="north", seed=61)
    save(les_s, "MMT_FireElementalLesser_south.png")
    save(derive(les_s, "east"), "MMT_FireElementalLesser_east.png")
    save(les_n, "MMT_FireElementalLesser_north.png")


if __name__ == "__main__":
    main()
