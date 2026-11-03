"""
Builds the Google Play store graphics for Stackville:
  Store/graphics/icon-512.png                  - app icon (512 x 512, 32-bit PNG)
  Store/graphics/feature-graphic-<lang>.png    - feature graphic (1024 x 500, no alpha)
  Store/screenshots/framed/<lang>/NN.png       - framed phone screenshots (1080 x 1920) from your real
                                                 screenshots in Store/screenshots/raw/ (01.png, 02.png, ...)

Run from the repo root:  python3 Tools/StoreAssets/make_store_assets.py
Needs Pillow:            pip install pillow
"""
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
SRC = os.path.join(ROOT, "Store", "graphics", "src")
OUT = os.path.join(ROOT, "Store", "graphics")
FONT_TITLE = os.path.join(ROOT, "Assets", "Resources", "Fonts", "TitleFont.ttf")
FONT_MAIN = os.path.join(ROOT, "Assets", "Resources", "Fonts", "MainFont.ttf")

RAINBOW = ["#FF6F91", "#FF9F43", "#FFC93C", "#2ED3A0", "#3FB6FF", "#8E7CFF"]

TAGLINES = {
    "pl-PL": ("Układaj klocki. Buduj miasto.", "Graj w Arcade i rozwijaj swoją wyspę!"),
    "en-US": ("Stack blocks. Build a town.", "Play Arcade and grow your island!"),
}

# Captions for the framed screenshots, in the order of your raw files (01.png, 02.png, ...).
CAPTIONS = {
    "pl-PL": [
        "Układaj klocki i zbieraj monety",
        "Setki poziomów na mapie",
        "Zbuduj swoje miasto 3D",
        "Las z grzybami i ogród",
        "Wspinaczka po wieży",
        "Bieg 3D bez końca",
        "Zadbaj o swojego pupila",
        "Ubierz swój klocek",
    ],
    "en-US": [
        "Stack blocks, collect coins",
        "Hundreds of levels on the map",
        "Build your own 3D town",
        "A mushroom forest and a garden",
        "Climb the tower",
        "An endless 3D run",
        "Look after your pet",
        "Dress up your block",
    ],
}


def hex2rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def gradient(size, top, bottom):
    w, h = size
    img = Image.new("RGB", size)
    d = ImageDraw.Draw(img)
    t, b = hex2rgb(top), hex2rgb(bottom)
    for y in range(h):
        k = y / max(1, h - 1)
        d.line([(0, y), (w, y)], fill=tuple(int(t[i] + (b[i] - t[i]) * k) for i in range(3)))
    return img


def rounded(img, radius):
    mask = Image.new("L", img.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle([0, 0, img.size[0] - 1, img.size[1] - 1], radius=radius, fill=255)
    out = img.convert("RGBA")
    out.putalpha(mask)
    return out


def card(base, img, box, radius=26, border="#FFFFFF"):
    """Pastes an image as a rounded card with a soft shadow and a thin border."""
    x, y, w, h = box
    shadow = Image.new("RGBA", base.size, (0, 0, 0, 0))
    ImageDraw.Draw(shadow).rounded_rectangle([x + 6, y + 12, x + w + 6, y + h + 12], radius=radius, fill=(0, 0, 0, 120))
    base.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(14)))
    im = img.copy()
    # cover-crop to the box
    s = max(w / im.width, h / im.height)
    im = im.resize((int(im.width * s), int(im.height * s)), Image.LANCZOS)
    left, top = (im.width - w) // 2, (im.height - h) // 2
    im = rounded(im.crop((left, top, left + w, top + h)), radius)
    base.alpha_composite(im, (x, y))
    ImageDraw.Draw(base).rounded_rectangle([x, y, x + w, y + h], radius=radius, outline=hex2rgb(border) + (200,), width=4)


def rainbow_title(draw, xy, text, font):
    x, y = xy
    for i, ch in enumerate(text):
        col = hex2rgb(RAINBOW[i % len(RAINBOW)])
        # dark outline / drop
        draw.text((x + 4, y + 6), ch, font=font, fill=(20, 12, 40))
        draw.text((x, y), ch, font=font, fill=col)
        x += draw.textlength(ch, font=font) - 2


JELLY = ["#FF5A6E", "#3FB6FF", "#FFC93C", "#2ED3A0", "#B48CFF", "#FF9F43", "#FF4FA3"]


def jelly_cell(d, x, y, s, color):
    """One glossy jelly block, like the game's Jelly skin."""
    c = hex2rgb(color)
    dark = tuple(int(v * 0.72) for v in c)
    light = tuple(min(255, int(v + (255 - v) * 0.45)) for v in c)
    r = max(3, s // 5)
    d.rounded_rectangle([x + 1, y + 2, x + s - 1, y + s], radius=r, fill=dark)
    d.rounded_rectangle([x + 1, y + 1, x + s - 1, y + s - 3], radius=r, fill=c)
    d.rounded_rectangle([x + s * 0.18, y + s * 0.14, x + s * 0.62, y + s * 0.34], radius=max(2, s // 8), fill=light)


def arcade_board(w, h):
    """A neon Arcade board: a stack with nearly full rows, a falling T piece and a big 3x3 square."""
    cols = 10
    rows = max(20, round(cols * (h - 12) / (w - 12)))  # fill the card's shape
    s = min((w - 12) // cols, (h - 12) // rows)
    w, h = cols * s + 12, rows * s + 12
    img = gradient((w, h), "#1B1040", "#0E0A26").convert("RGBA")
    d = ImageDraw.Draw(img)
    ox, oy = 6, 6
    for gx in range(cols):
        for gy in range(rows):
            cx, cy = ox + gx * s + s // 2, oy + gy * s + s // 2
            d.ellipse([cx - 1, cy - 1, cx + 1, cy + 1], fill=(70, 60, 120))
    # the stack (bottom rows), '.' = empty; letters pick a colour
    stack = [
        "..........",
        "......f...",
        ".....ff..d",
        "g....fc.dd",
        "gg..cccbbd",
        "ag.eeebbcc",
        "....bb....",
        "a..bbcc..e",
        "aa.ddcc.ee",
        "afddggg.ee",
        "affdgbbbca",
        "cfaaagbcca",
        "ccaeeebbdd",
    ]
    for i, line in enumerate(stack):
        gy = rows - len(stack) + i
        for gx, ch in enumerate(line):
            if ch != ".":
                jelly_cell(d, ox + gx * s, oy + gy * s, s, JELLY["abcdefg".index(ch)])
    # falling T piece with its ghost
    fy = rows // 3
    for gx, gy in [(1, fy), (2, fy), (3, fy), (2, fy - 1)]:
        jelly_cell(d, ox + gx * s, oy + gy * s, s, "#B48CFF")
    gyh = rows - 13
    for gx, gy in [(1, gyh), (2, gyh), (3, gyh), (2, gyh - 1)]:
        d.rounded_rectangle([ox + gx * s + 2, oy + gy * s + 2, ox + gx * s + s - 2, oy + gy * s + s - 2], radius=4, outline=hex2rgb("#B48CFF"), width=2)
    # big 3x3 square coming next
    for gx in range(3):
        for gy in range(3):
            jelly_cell(d, ox + (6 + gx) * s, oy + (2 + gy) * s, s, "#FFC93C")
    return img


def icon():
    src = Image.open(os.path.join(SRC, "icon-src.png")).convert("RGBA").resize((512, 512), Image.LANCZOS)
    path = os.path.join(OUT, "icon-512.png")
    src.save(path)
    return path


def feature_graphic(lang):
    W, H = 1024, 500
    base = gradient((W, H), "#14152E", "#3A2461").convert("RGBA")
    # soft glows
    glow = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow)
    gd.ellipse([600, -120, 1100, 380], fill=hex2rgb("#FF6F91") + (70,))
    gd.ellipse([-150, 260, 420, 700], fill=hex2rgb("#3FB6FF") + (60,))
    base.alpha_composite(glow.filter(ImageFilter.GaussianBlur(70)))

    # the Arcade block puzzle (big card) and the 3D town on the right
    city = Image.open(os.path.join(SRC, "city3d.png")).convert("RGBA")
    towers = Image.open(os.path.join(SRC, "towers.png")).convert("RGBA")
    jungle = towers.crop((1080, 0, 1620, 960))
    space = towers.crop((0, 960, 540, 1920))
    card(base, space, (572, 30, 122, 210), 20)
    card(base, jungle, (572, 262, 122, 210), 20)
    card(base, city.crop((60, 160, 480, 820)), (706, 96, 128, 300), 20)
    card(base, arcade_board(332, 912), (846, 22, 166, 456), 22, "#FF4FA3")

    # mascot
    mascot = Image.open(os.path.join(SRC, "icon-src.png")).convert("RGBA").resize((230, 230), Image.LANCZOS)
    mascot = rounded(mascot, 52)
    card(base, mascot, (50, 345, 120, 120), 30)

    d = ImageDraw.Draw(base)
    tsize = 92
    while tsize > 50 and sum(d.textlength(ch, font=ImageFont.truetype(FONT_TITLE, tsize)) - 2 for ch in "STACKVILLE") > 500:
        tsize -= 2
    title = ImageFont.truetype(FONT_TITLE, tsize)
    rainbow_title(d, (44, 96 + (92 - tsize) // 2), "STACKVILLE", title)
    line1, line2 = TAGLINES[lang]
    def fit(text, size, max_w=490):
        while size > 16:
            f = ImageFont.truetype(FONT_MAIN, size)
            if d.textlength(text, font=f) <= max_w:
                return f
            size -= 2
        return ImageFont.truetype(FONT_MAIN, size)
    f1 = fit(line1, 40)
    f2 = fit(line2, 28)
    d.text((48, 212), line1, font=f1, fill=(242, 243, 255))
    d.text((48, 272), line2, font=f2, fill=hex2rgb("#9FA5D8"))

    out = base.convert("RGB")
    path = os.path.join(OUT, f"feature-graphic-{lang}.png")
    out.save(path)
    return path


def frame_screenshots(lang):
    raw_dir = os.path.join(ROOT, "Store", "screenshots", "raw")
    out_dir = os.path.join(ROOT, "Store", "screenshots", "framed", lang)
    if not os.path.isdir(raw_dir):
        return []
    os.makedirs(out_dir, exist_ok=True)
    files = sorted(f for f in os.listdir(raw_dir) if f.lower().endswith(".png"))
    made = []
    W, H = 1080, 1920
    cap_font = ImageFont.truetype(FONT_TITLE, 66)
    for i, name in enumerate(files):
        shot = Image.open(os.path.join(raw_dir, name)).convert("RGBA")
        base = gradient((W, H), "#14152E", "#3A2461").convert("RGBA")
        d = ImageDraw.Draw(base)
        caption = CAPTIONS[lang][i] if i < len(CAPTIONS[lang]) else ""
        # wrap caption into at most two lines
        words, lines, cur = caption.split(), [], ""
        for w in words:
            test = (cur + " " + w).strip()
            if d.textlength(test, font=cap_font) > W - 120 and cur:
                lines.append(cur)
                cur = w
            else:
                cur = test
        if cur:
            lines.append(cur)
        y = 70
        for ln in lines[:2]:
            tw = d.textlength(ln, font=cap_font)
            d.text(((W - tw) / 2 + 3, y + 5), ln, font=cap_font, fill=(10, 8, 30))
            d.text(((W - tw) / 2, y), ln, font=cap_font, fill=(255, 255, 255))
            y += 84
        # the real screenshot, scaled into a rounded "phone" card
        top = 70 + 84 * 2 + 30
        box_h = H - top - 60
        s = min((W - 140) / shot.width, box_h / shot.height)
        sw, sh = int(shot.width * s), int(shot.height * s)
        card(base, shot.resize((sw, sh), Image.LANCZOS), ((W - sw) // 2, top, sw, sh), 44, "#8E7CFF")
        path = os.path.join(out_dir, f"{i + 1:02d}.png")
        base.convert("RGB").save(path)
        made.append(path)
    return made


if __name__ == "__main__":
    print(icon())
    for lang in TAGLINES:
        print(feature_graphic(lang))
        for p in frame_screenshots(lang):
            print(p)
