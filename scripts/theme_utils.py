"""Shared deterministic color and palette helpers for Nib."""

from __future__ import annotations

import json
import math
import re
from pathlib import Path
from typing import Any, Iterator

HEX_RE = re.compile(r"^#[0-9A-F]{6}$")

REQUIRED_MODE_PATHS = (
    "background",
    "surface.elevated",
    "surface.floating",
    "surface.subtle",
    "border.default",
    "border.subtle",
    "border.focus",
    "foreground.primary",
    "foreground.secondary",
    "foreground.muted",
    "foreground.disabled",
    "blue_ink.deep",
    "blue_ink.primary",
    "blue_ink.bright",
    "moss.primary",
    "moss.muted",
    "teal.primary",
    "teal.muted",
    "burgundy",
    "rust",
    "violet",
    "amber",
    "sepia",
    "graphite",
    "diagnostic.error",
    "diagnostic.warning",
    "diagnostic.information",
    "diagnostic.hint",
    "diagnostic.success",
    "selection.background",
    "selection.foreground",
    "search.background",
    "search.foreground",
    "search.current_background",
    "search.current_foreground",
    "current_line",
    "match.background",
    "match.foreground",
    "focus",
    "hyperlink",
    "diff.add",
    "diff.add_text",
    "diff.change",
    "diff.change_text",
    "diff.delete",
    "diff.delete_text",
    "diff.foreground",
    "cursor.background",
    "cursor.foreground",
)

CVD_MATRICES = {
    "protanopia": (
        (0.152286, 1.052583, -0.204868),
        (0.114503, 0.786281, 0.099216),
        (-0.003882, -0.048116, 1.051998),
    ),
    "deuteranopia": (
        (0.367322, 0.860646, -0.227968),
        (0.280085, 0.672501, 0.047413),
        (-0.011820, 0.042940, 0.968881),
    ),
    "tritanopia": (
        (1.255528, -0.076749, -0.178779),
        (-0.078411, 0.930809, 0.147602),
        (0.004733, 0.691367, 0.303900),
    ),
}


def load_palette(root: Path) -> dict[str, Any]:
    return json.loads((root / "palette" / "palette.json").read_text(encoding="utf-8"))


def load_foundation(root: Path) -> dict[str, Any]:
    return json.loads((root / "palette" / "foundation.json").read_text(encoding="utf-8"))


def load_aliases(root: Path) -> dict[str, Any]:
    return json.loads((root / "palette" / "aliases.json").read_text(encoding="utf-8"))


def resolve_reference(value: dict[str, Any], reference: str) -> Any:
    if not reference.startswith("$"):
        raise ValueError(f"invalid palette reference: {reference}")
    return get_path(value, reference[1:])


def get_path(value: dict[str, Any], dotted: str) -> Any:
    current: Any = value
    for component in dotted.split("."):
        current = current[component]
    return current


def iter_colors(value: Any, prefix: str = "") -> Iterator[tuple[str, str]]:
    if isinstance(value, str) and value.startswith("#"):
        yield prefix, value
    elif isinstance(value, dict):
        for key, child in value.items():
            child_prefix = f"{prefix}.{key}" if prefix else key
            yield from iter_colors(child, child_prefix)
    elif isinstance(value, list):
        for index, child in enumerate(value):
            child_prefix = f"{prefix}.{index}" if prefix else str(index)
            yield from iter_colors(child, child_prefix)


def rgb(hex_color: str) -> tuple[float, float, float]:
    return tuple(int(hex_color[index : index + 2], 16) / 255 for index in (1, 3, 5))  # type: ignore[return-value]


def linear_channel(channel: float) -> float:
    if channel <= 0.04045:
        return channel / 12.92
    return ((channel + 0.055) / 1.055) ** 2.4


def srgb_channel(channel: float) -> float:
    if channel <= 0.0031308:
        return 12.92 * channel
    return 1.055 * channel ** (1 / 2.4) - 0.055


def luminance(hex_color: str) -> float:
    red, green, blue = (linear_channel(value) for value in rgb(hex_color))
    return 0.2126 * red + 0.7152 * green + 0.0722 * blue


def contrast(first: str, second: str) -> float:
    high, low = sorted((luminance(first), luminance(second)), reverse=True)
    return (high + 0.05) / (low + 0.05)


def simulated_rgb(hex_color: str, deficiency: str) -> tuple[float, float, float]:
    source = tuple(linear_channel(value) for value in rgb(hex_color))
    matrix = CVD_MATRICES[deficiency]
    linear = tuple(
        min(1.0, max(0.0, sum(row[index] * source[index] for index in range(3))))
        for row in matrix
    )
    return tuple(srgb_channel(value) for value in linear)  # type: ignore[return-value]


def oklab(color: tuple[float, float, float]) -> tuple[float, float, float]:
    red, green, blue = (linear_channel(value) for value in color)
    l_value = 0.4122214708 * red + 0.5363325363 * green + 0.0514459929 * blue
    m_value = 0.2119034982 * red + 0.6806995451 * green + 0.1073969566 * blue
    s_value = 0.0883024619 * red + 0.2817188376 * green + 0.6299787005 * blue
    l_root = math.copysign(abs(l_value) ** (1 / 3), l_value)
    m_root = math.copysign(abs(m_value) ** (1 / 3), m_value)
    s_root = math.copysign(abs(s_value) ** (1 / 3), s_value)
    return (
        0.2104542553 * l_root + 0.7936177850 * m_root - 0.0040720468 * s_root,
        1.9779984951 * l_root - 2.4285922050 * m_root + 0.4505937099 * s_root,
        0.0259040371 * l_root + 0.7827717662 * m_root - 0.8086757660 * s_root,
    )


def oklab_distance(first: str, second: str, deficiency: str | None = None) -> float:
    first_rgb = simulated_rgb(first, deficiency) if deficiency else rgb(first)
    second_rgb = simulated_rgb(second, deficiency) if deficiency else rgb(second)
    first_lab = oklab(first_rgb)
    second_lab = oklab(second_rgb)
    return math.sqrt(sum((a - b) ** 2 for a, b in zip(first_lab, second_lab)))


def ansi_colors(mode: dict[str, Any]) -> list[str]:
    return [entry["hex"] for entry in mode["ansi"]]
