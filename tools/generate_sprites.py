#!/usr/bin/env python3
"""Generate deterministic original placeholder sheets for SUMMING."""

from __future__ import annotations

import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "assets" / "sprites" / "runtime"
SOURCE = ROOT / "assets" / "sprites" / "source"

PALETTE = {
    "void": "#0a0d14", "ink": "#151823", "stone": "#403a48", "stone_light": "#66545a",
    "oxide": "#9f574a", "blood": "#b84943", "bone": "#cdb98a", "gold": "#d8b866",
    "cyan": "#8fcac4", "salt": "#b9d3bc", "white": "#ffffff", "transparent": "#00000000",
}

PLAYER_LOCOMOTION = ["idle_0", "idle_1", "run_0", "run_1", "run_2", "run_3", "takeoff", "ascend", "apex", "fall", "land"]
PLAYER_TRAVERSAL = ["crouch", "crawl", "slide", "ledge", "mantle", "wall_cling", "wall_jump", "dash", "rope", "dig_0", "dig_1", "bomb", "hurt", "death"]


def color(name: str) -> str:
    return PALETTE[name]


def make_sheet(name: str, cell: tuple[int, int], frames: list[str], drawer) -> dict:
    RUNTIME.mkdir(parents=True, exist_ok=True)
    SOURCE.mkdir(parents=True, exist_ok=True)
    width, height = cell[0] * len(frames), cell[1]
    runtime = Image.new("RGBA", (width, height), color("transparent"))
    source = Image.new("RGBA", (width, height), color("white"))
    for index, frame in enumerate(frames):
        for image in (runtime, source):
            draw = ImageDraw.Draw(image)
            drawer(draw, index * cell[0], 0, frame)
    runtime.save(RUNTIME / f"{name}.png", optimize=True)
    source.save(SOURCE / f"{name}-white.png", optimize=True)
    return {"file": f"runtime/{name}.png", "source": f"source/{name}-white.png", "cell": list(cell), "frames": frames}


def player(draw: ImageDraw.ImageDraw, ox: int, oy: int, state: str) -> None:
    low = state in {"crouch", "crawl", "slide", "death"}
    dash = state == "dash"
    wall = state in {"wall_cling", "ledge", "mantle"}
    hurt = state == "hurt"
    if low:
        draw.rectangle((ox + 2, oy + 14, ox + 10, oy + 20), fill=color("oxide"))
        draw.rectangle((ox + 6, oy + 12, ox + 21, oy + 21), fill=color("stone"))
        draw.rectangle((ox + 10, oy + 6, ox + 19, oy + 14), fill=color("bone"))
        draw.rectangle((ox + 14, oy + 9, ox + 21, oy + 11), fill=color("ink"))
        draw.rectangle((ox + 4, oy + 21, ox + 22, oy + 23), fill=color("ink" if state == "death" else "stone_light"))
    else:
        x = ox + (1 if dash else 3)
        draw.rectangle((x - (2 if dash else 1), oy + 10, x + 6, oy + 17), fill=color("oxide"))
        draw.rectangle((x + 4, oy + 9, x + (19 if dash else 15), oy + 19), fill=color("stone"))
        draw.rectangle((x + 7, oy + 1, x + 15, oy + 10), fill=color("bone"))
        draw.rectangle((x + 11, oy + 4, x + 17, oy + 6), fill=color("ink"))
        if state.startswith("run") or state in {"wall_jump", "takeoff", "ascend", "fall"}:
            phase = int(state[-1]) if state.startswith("run_") else 1
            draw.line((x + 7, oy + 18, x + 4 + (phase % 2) * 8, oy + 23), fill=color("ink"), width=3)
            draw.line((x + 13, oy + 18, x + 16 - (phase % 2) * 8, oy + 23), fill=color("stone_light"), width=3)
        else:
            draw.rectangle((x + 6, oy + 18, x + 9, oy + 23), fill=color("ink"))
            draw.rectangle((x + 12, oy + 18, x + 15, oy + 23), fill=color("stone_light"))
    if wall:
        draw.rectangle((ox + 18, oy + 9, ox + 22, oy + 13), fill=color("gold"))
    if state in {"dig_0", "dig_1"}:
        draw.line((ox + 16, oy + 13, ox + 22, oy + (3 if state == "dig_0" else 20)), fill=color("bone"), width=2)
    if hurt:
        draw.rectangle((ox + 4, oy + 9, ox + 6, oy + 18), fill=color("blood"))


def tile(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    pairs = {
        "loess": ("oxide", "stone_light"), "sandstone": ("oxide", "blood"), "basalt": ("stone", "stone_light"),
        "fossil": ("stone_light", "bone"), "alloy": ("stone", "cyan"), "brittle": ("stone_light", "oxide"),
        "salt_glass": ("cyan", "salt"), "ruin_mark": ("stone", "gold"),
    }
    base, accent = pairs[frame]
    draw.rectangle((ox, oy, ox + 23, oy + 23), fill=color(base))
    draw.rectangle((ox, oy, ox + 23, oy + 2), fill=color(accent))
    for px, py in ((4, 8), (13, 5), (8, 17), (19, 14)):
        if (px + py + ox // 24) % 2 == 0:
            draw.rectangle((ox + px, oy + py, ox + px + 1, oy + py + 1), fill=color(accent))
    if frame == "brittle":
        draw.line((ox + 11, oy + 3, ox + 8, oy + 12, ox + 14, oy + 22), fill=color("ink"), width=1)
    if frame == "ruin_mark":
        draw.rectangle((ox + 8, oy + 6, ox + 15, oy + 17), outline=color("gold"), width=2)


def burrower(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    phase = int(frame[-1])
    draw.ellipse((ox + 3, oy + 7 + phase % 2, ox + 28, oy + 21), fill=color("stone"), outline=color("oxide"), width=2)
    draw.polygon([(ox + 24, oy + 10), (ox + 31, oy + 14), (ox + 24, oy + 18)], fill=color("bone"))
    for x in range(7, 24, 5):
        draw.line((ox + x, oy + 19, ox + x - 3, oy + 23), fill=color("ink"), width=2)
    draw.point((ox + 23, oy + 11), fill=color("gold"))


def hazard(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    tile(draw, ox, oy, "brittle")
    phase = int(frame[-1])
    for index in range(phase + 1):
        draw.line((ox + 5 + index * 4, oy + 2, ox + 8 + index * 3, oy + 21), fill=color("ink"), width=1)


def tool(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    if frame == "bomb":
        draw.ellipse((ox + 6, oy + 7, ox + 18, oy + 19), fill=color("ink"), outline=color("stone_light"))
        draw.line((ox + 13, oy + 7, ox + 17, oy + 3), fill=color("gold"), width=2)
    elif frame == "rope":
        draw.line((ox + 12, oy + 2, ox + 12, oy + 22), fill=color("bone"), width=2)
        for y in range(5, 22, 5): draw.line((ox + 9, oy + y, ox + 15, oy + y), fill=color("gold"), width=1)
    else:
        draw.line((ox + 5, oy + 19, ox + 18, oy + 5), fill=color("bone"), width=3)
        draw.line((ox + 13, oy + 3, ox + 21, oy + 10), fill=color("cyan"), width=3)


def relic(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    index = int(frame[-1])
    if index % 3 == 0:
        draw.ellipse((ox + 5, oy + 5, ox + 18, oy + 18), outline=color("gold"), width=3)
        draw.rectangle((ox + 10, oy + 9, ox + 13, oy + 22), fill=color("stone"))
    elif index % 3 == 1:
        draw.polygon([(ox + 12, oy + 3), (ox + 20, oy + 20), (ox + 4, oy + 20)], fill=color("stone"), outline=color("cyan"))
        draw.rectangle((ox + 11, oy + 8, ox + 13, oy + 18), fill=color("gold"))
    else:
        draw.rectangle((ox + 5, oy + 4, ox + 19, oy + 21), fill=color("stone"), outline=color("bone"), width=2)
        draw.line((ox + 8, oy + 9, ox + 16, oy + 16), fill=color("oxide"), width=2)


def ruin(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    index = int(frame[-1])
    draw.rectangle((ox + 6, oy + 12, ox + 41, oy + 63), fill=color("stone"))
    draw.rectangle((ox + 10 + index * 2, oy + 20, ox + 35, oy + 63), fill=color("ink"))
    draw.rectangle((ox + 5, oy + 11, ox + 42, oy + 15), fill=color("stone_light"))
    draw.line((ox + 14, oy + 18, ox + 14, oy + 57), fill=color("gold"), width=2)
    if index % 2 == 0:
        draw.ellipse((ox + 20, oy + 26, ox + 31, oy + 37), outline=color("cyan"), width=2)


def ui(draw: ImageDraw.ImageDraw, ox: int, oy: int, frame: str) -> None:
    if frame == "heart":
        draw.polygon([(ox + 4, oy + 8), (ox + 8, oy + 4), (ox + 12, oy + 8), (ox + 16, oy + 4), (ox + 20, oy + 8), (ox + 12, oy + 21)], fill=color("blood"))
    elif frame == "dash":
        draw.polygon([(ox + 3, oy + 12), (ox + 16, oy + 4), (ox + 12, oy + 10), (ox + 22, oy + 10), (ox + 8, oy + 20), (ox + 12, oy + 13)], fill=color("cyan"))
    elif frame == "archive":
        draw.rectangle((ox + 5, oy + 4, ox + 19, oy + 21), outline=color("gold"), width=2)
        draw.line((ox + 8, oy + 9, ox + 16, oy + 9), fill=color("bone"), width=1)
    else:
        tool(draw, ox, oy, frame)


def main() -> None:
    sheets = {
        "player_locomotion": make_sheet("player_locomotion", (24, 24), PLAYER_LOCOMOTION, player),
        "player_traversal": make_sheet("player_traversal", (24, 24), PLAYER_TRAVERSAL, player),
        "badlands_terrain": make_sheet("badlands_terrain", (24, 24), ["loess", "sandstone", "basalt", "fossil", "alloy", "brittle", "salt_glass", "ruin_mark"], tile),
        "burrower": make_sheet("burrower", (32, 24), ["burrow_0", "burrow_1", "burrow_2", "burrow_3"], burrower),
        "brittle_hazard": make_sheet("brittle_hazard", (24, 24), ["crack_0", "crack_1", "crack_2", "crack_3"], hazard),
        "tools": make_sheet("tools", (24, 24), ["pick", "bomb", "rope"], tool),
        "relics": make_sheet("relics", (24, 24), [f"relic_{i}" for i in range(6)], relic),
        "ruins": make_sheet("ruins", (48, 64), [f"ruin_{i}" for i in range(4)], ruin),
        "ui": make_sheet("ui", (24, 24), ["heart", "dash", "bomb", "rope", "archive"], ui),
        "player_aseprite_locomotion": {
            "file": "runtime/player_aseprite_locomotion.png",
            "source": "aseprite/ashen_pilgrim.aseprite",
            "cell": [24, 24],
            "frames": PLAYER_LOCOMOTION,
            "generation": "Aseprite MCP using tools/aseprite/generate_ashen_pilgrim.lua",
        },
        "player_aseprite_traversal": {
            "file": "runtime/player_aseprite_traversal.png",
            "source": "aseprite/ashen_pilgrim.aseprite",
            "cell": [24, 24],
            "frames": PLAYER_TRAVERSAL,
            "generation": "Aseprite MCP using tools/aseprite/generate_ashen_pilgrim.lua",
        },
    }
    manifest = {
        "version": 2,
        "license": "Project-owned original placeholders; license not specified",
        "generation": "python3 tools/generate_sprites.py (Pillow; deterministic; no external inputs)",
        "palette": PALETTE,
        "sheets": sheets,
        "source_concept": "source/summing-concept-white.png",
        "source_prompt_document": "docs/art/SPRITE-GENERATION.md",
    }
    (ROOT / "assets" / "sprites" / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")


if __name__ == "__main__":
    main()
