#!/usr/bin/env python3
"""Dependency-free release checks for Quireveil."""

from __future__ import annotations

import json
import math
import os
import re
import shutil
import struct
import subprocess
import sys
import tempfile
import tomllib
from html.parser import HTMLParser
from pathlib import Path
from typing import Any, Callable

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "scripts"))

from generate import CONTRAST_PAIRS, CVD_PAIRS, MARKER, render_files  # noqa: E402
from theme_utils import (  # noqa: E402
    CVD_MATRICES,
    HEX_RE,
    REQUIRED_MODE_PATHS,
    contrast,
    get_path,
    iter_colors,
    load_palette,
    luminance,
    oklab_distance,
)

PALETTE = load_palette(ROOT)
SCHEMA = json.loads((ROOT / "palette" / "schema.json").read_text(encoding="utf-8"))


class VerificationError(RuntimeError):
    pass


def require(condition: bool, message: str) -> None:
    if not condition:
        raise VerificationError(message)


def command(arguments: list[str], *, env: dict[str, str] | None = None) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(arguments, cwd=ROOT, env=env, text=True, capture_output=True)
    if result.returncode:
        detail = (result.stdout + result.stderr).strip()
        raise VerificationError(f"command failed ({' '.join(arguments)}):\n{detail}")
    return result


def resolve_reference(reference: str) -> dict[str, Any]:
    require(reference.startswith("#/"), f"unsupported schema reference: {reference}")
    value: Any = SCHEMA
    for component in reference[2:].split("/"):
        value = value[component.replace("~1", "/").replace("~0", "~")]
    return value


def validate_schema(value: Any, rule: dict[str, Any], path: str = "$") -> None:
    if "$ref" in rule:
        validate_schema(value, resolve_reference(rule["$ref"]), path)
        return
    if "const" in rule:
        require(value == rule["const"], f"{path}: expected constant {rule['const']!r}")
    expected_type = rule.get("type")
    checks: dict[str, Callable[[Any], bool]] = {
        "object": lambda item: isinstance(item, dict),
        "array": lambda item: isinstance(item, list),
        "string": lambda item: isinstance(item, str),
        "integer": lambda item: isinstance(item, int) and not isinstance(item, bool),
    }
    if expected_type:
        require(checks[expected_type](value), f"{path}: expected {expected_type}")
    if isinstance(value, dict):
        for key in rule.get("required", []):
            require(key in value, f"{path}: missing required property {key}")
        require(len(value) >= rule.get("minProperties", 0), f"{path}: too few properties")
        properties = rule.get("properties", {})
        additional = rule.get("additionalProperties", True)
        for key, child in value.items():
            child_path = f"{path}.{key}"
            if key in properties:
                validate_schema(child, properties[key], child_path)
            elif additional is False:
                raise VerificationError(f"{child_path}: unexpected property")
            elif isinstance(additional, dict):
                validate_schema(child, additional, child_path)
    if isinstance(value, list):
        require(len(value) >= rule.get("minItems", 0), f"{path}: too few items")
        require(len(value) <= rule.get("maxItems", math.inf), f"{path}: too many items")
        if "items" in rule:
            for index, child in enumerate(value):
                validate_schema(child, rule["items"], f"{path}[{index}]")
    if isinstance(value, str) and "pattern" in rule:
        require(re.search(rule["pattern"], value) is not None, f"{path}: pattern mismatch")
    if isinstance(value, int) and not isinstance(value, bool):
        require(value >= rule.get("minimum", -math.inf), f"{path}: below minimum")
        require(value <= rule.get("maximum", math.inf), f"{path}: above maximum")


def verify_palette() -> None:
    validate_schema(PALETTE, SCHEMA)
    require(PALETTE["meta"]["slug"] == "quireveil", "unexpected palette slug")
    require(PALETTE["meta"]["minimum_neovim"] == "0.10.0", "unexpected Neovim baseline")
    require(PALETTE["meta"]["minimum_ghostty"] == "1.3.0", "unexpected Ghostty baseline")
    for style, mode in PALETTE["modes"].items():
        for role in REQUIRED_MODE_PATHS:
            require(HEX_RE.fullmatch(get_path(mode, role)) is not None, f"{style}: invalid or missing {role}")
        for path, color in iter_colors(mode):
            require(HEX_RE.fullmatch(color) is not None, f"{style}.{path}: color must be uppercase six-digit sRGB")
            require(color not in {"#000000", "#FFFFFF"}, f"{style}.{path}: pure black/white is forbidden")
    require(luminance(PALETTE["modes"]["light"]["background"]) > 0.80, "light paper is not a light surface")
    require(luminance(PALETTE["modes"]["dark"]["background"]) < 0.02, "dark paper is not near-black")


def verify_generation() -> None:
    command([sys.executable, "scripts/generate.py", "--check"])
    first = render_files(PALETTE)
    second = render_files(load_palette(ROOT))
    require(first == second, "generator output is not deterministic")
    for relative, content in first.items():
        require(MARKER in "\n".join(content.splitlines()[:3]), f"generated marker missing: {relative}")
    authored = [
        ROOT / "colors" / "quireveil.lua",
        ROOT / "lua" / "quireveil" / "init.lua",
        ROOT / "lua" / "quireveil" / "highlights.lua",
        ROOT / "lua" / "quireveil" / "integrations.lua",
        ROOT / "lua" / "lualine" / "themes" / "quireveil.lua",
        ROOT / "preview" / "index.html",
        ROOT / "preview" / "style.css",
        ROOT / "preview" / "app.js",
    ]
    literal = re.compile(r"#[0-9A-Fa-f]{6}\b")
    for path in authored:
        require(not literal.search(path.read_text(encoding="utf-8")), f"scattered color literal: {path.relative_to(ROOT)}")


def verify_contrast() -> None:
    for style, mode in PALETTE["modes"].items():
        for foreground_path, background_path, target, label in CONTRAST_PAIRS:
            ratio = contrast(get_path(mode, foreground_path), get_path(mode, background_path))
            require(ratio + 1e-9 >= target, f"{style} {label}: {ratio:.2f}:1 is below {target:.1f}:1")
        accent_pairs = (
            ("blue_ink.primary", "moss.primary"),
            ("moss.primary", "teal.primary"),
            ("teal.primary", "burgundy"),
            ("burgundy", "rust"),
            ("rust", "violet"),
            ("violet", "amber"),
            ("amber", "graphite"),
        )
        for first, second in accent_pairs:
            distance = oklab_distance(get_path(mode, first), get_path(mode, second))
            require(distance >= 0.05, f"{style} accents too similar: {first} / {second} ({distance:.3f})")


def verify_colour_vision() -> None:
    for style, mode in PALETTE["modes"].items():
        for first_path, second_path, label in CVD_PAIRS:
            first = get_path(mode, first_path)
            second = get_path(mode, second_path)
            require(oklab_distance(first, second) >= 0.05, f"{style} {label}: original pair converged")
            for deficiency in CVD_MATRICES:
                distance = oklab_distance(first, second, deficiency)
                require(math.isfinite(distance), f"{style} {label}: invalid {deficiency} result")
                require(distance >= 0.025, f"{style} {label}: {deficiency} distance regressed ({distance:.3f})")
    highlights = (ROOT / "lua" / "quireveil" / "highlights.lua").read_text(encoding="utf-8")
    require(highlights.count("undercurl = true") >= 9, "diagnostics/spelling lack undercurl redundancy")
    require("strikethrough = true" in highlights, "deprecated content lacks strikethrough redundancy")
    require(all(f"DiagnosticSign{name}" in highlights for name in ("Error", "Warn", "Info", "Hint")), "diagnostic signs are incomplete")


def verify_ansi() -> None:
    names = ["black", "red", "green", "yellow", "blue", "magenta", "cyan", "white"]
    expected_names = names + [f"bright {name}" for name in names]
    for style, mode in PALETTE["modes"].items():
        ansi = mode["ansi"]
        require([entry["index"] for entry in ansi] == list(range(16)), f"{style}: ANSI indexes are incomplete")
        require([entry["name"] for entry in ansi] == expected_names, f"{style}: ANSI identities are non-conventional")
        require(len({entry["hex"] for entry in ansi}) == 16, f"{style}: duplicate ANSI colors")
        for entry in ansi[1:]:
            ratio = contrast(entry["hex"], mode["background"])
            require(ratio >= 4.5, f"{style}: ANSI {entry['index']} contrast is {ratio:.2f}:1")
        for index in range(8):
            distance = oklab_distance(ansi[index]["hex"], ansi[index + 8]["hex"])
            require(distance >= 0.025, f"{style}: ANSI {index}/{index + 8} are too similar ({distance:.3f})")


def parse_ghostty_theme(path: Path) -> dict[str, Any]:
    values: dict[str, Any] = {"palette": []}
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        require("=" in line, f"{path}: malformed line: {line}")
        key, value = (part.strip() for part in line.split("=", 1))
        if key == "palette":
            values["palette"].append(value)
        else:
            require(key not in values, f"{path}: duplicate key {key}")
            values[key] = value
    return values


def verify_ghostty() -> None:
    required = {"background", "foreground", "cursor-color", "cursor-text", "selection-background", "selection-foreground"}
    for style in ("Light", "Dark"):
        path = ROOT / "ghostty" / "themes" / f"Quireveil {style}"
        values = parse_ghostty_theme(path)
        require(required <= values.keys(), f"{path}: missing theme keys")
        require(not any("shader" in key for key in values), f"{path}: core theme unexpectedly loads a shader")
        indexes = [int(entry.split("=", 1)[0]) for entry in values["palette"]]
        require(indexes == list(range(16)), f"{path}: ANSI palette is incomplete")
    paired = (ROOT / "ghostty" / "examples" / "paired.conf").read_text(encoding="utf-8")
    require("theme = light:Quireveil Light,dark:Quireveil Dark" in paired, "paired theme syntax is missing")
    require("window-theme = system" in paired, "paired example does not follow OS appearance")
    for preset in ("daily", "showcase"):
        example = (ROOT / "ghostty" / "examples" / f"shaders-{preset}.conf").read_text(encoding="utf-8")
        paper = example.find(f"paper-grain-{preset}.glsl")
        ink = example.find(f"ink-feather-{preset}.glsl")
        require(0 <= paper < ink, f"{preset}: shader stage order is wrong")
        require("custom-shader-animation = false" in example, f"{preset}: animation is not disabled")

    ghostty = shutil.which("ghostty")
    if ghostty is None:
        print("  Ghostty runtime: skipped (ghostty unavailable; structural checks passed)")
        return
    with tempfile.TemporaryDirectory(prefix="quireveil-ghostty-") as temporary:
        config_home = Path(temporary)
        theme_dir = config_home / "ghostty" / "themes"
        theme_dir.mkdir(parents=True)
        for source in (ROOT / "ghostty" / "themes").iterdir():
            shutil.copy2(source, theme_dir / source.name)
        config = config_home / "paired.conf"
        shutil.copy2(ROOT / "ghostty" / "examples" / "paired.conf", config)
        environment = os.environ.copy()
        environment["XDG_CONFIG_HOME"] = str(config_home)
        command([ghostty, "+validate-config", f"--config-file={config}"], env=environment)
    version = command([ghostty, "+version"]).stdout.splitlines()[0]
    print(f"  Ghostty runtime: pass ({version})")


def verify_shaders() -> None:
    shaders = sorted((ROOT / "ghostty" / "shaders").glob("*.glsl"))
    require(len(shaders) == 4, "expected four shader stages")
    for path in shaders:
        source = path.read_text(encoding="utf-8")
        require("void mainImage(out vec4 fragColor, in vec2 fragCoord)" in source, f"{path}: wrong mainImage contract")
        require("iChannel0" in source and "iResolution" in source, f"{path}: required Ghostty uniforms are missing")
        require("iTime" not in source and "iFrame" not in source, f"{path}: shader contains temporal input")
        require(source.count("texture(") <= 5, f"{path}: sampling budget exceeded")
    result = command([sys.executable, "scripts/validate_shaders.py"])
    print("  " + result.stdout.strip().replace("\n", "\n  "))


class PreviewParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.ids: set[str] = set()
        self.resources: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        attributes = dict(attrs)
        if attributes.get("id"):
            self.ids.add(attributes["id"] or "")
        for key in ("href", "src"):
            if attributes.get(key):
                self.resources.append(attributes[key] or "")


def png_size(path: Path) -> tuple[int, int]:
    data = path.read_bytes()[:24]
    require(data[:8] == b"\x89PNG\r\n\x1a\n", f"{path}: not a PNG")
    return struct.unpack(">II", data[16:24])


def verify_preview() -> None:
    parser = PreviewParser()
    parser.feed((ROOT / "preview" / "index.html").read_text(encoding="utf-8"))
    required_ids = {"mode-grid", "sample-select", "light-overview", "dark-overview", "syntax-detail", "ansi-comparison", "shader-comparison", "contrast-body"}
    require(required_ids <= parser.ids, "preview laboratory is missing required sections or controls")
    require(parser.resources[:2] == ["data:,", "generated/palette.css"], "preview resource order changed unexpectedly")
    for resource in parser.resources:
        if resource.startswith(("data:", "#", "http://", "https://")):
            continue
        target = (ROOT / "preview" / resource.split("#", 1)[0]).resolve()
        require(target.exists(), f"preview resource is missing: {resource}")
    expected = {
        "light-overview-render.png": (742, 833),
        "dark-overview-render.png": (742, 833),
        "ansi-comparison-render.png": (1504, 638),
        "syntax-detail-render.png": (1504, 686),
        "shader-comparison-render.png": (1504, 406),
    }
    for name, dimensions in expected.items():
        path = ROOT / "output" / "playwright" / "preview" / name
        require(path.exists(), f"preview artifact is missing: {name}")
        require(png_size(path) == dimensions, f"preview artifact dimensions changed: {name}")
    require("Reference simulation — not a Ghostty capture" in (ROOT / "preview" / "index.html").read_text(encoding="utf-8"), "shader render is not truthfully labelled")
    node = shutil.which("node")
    if node:
        command([node, "--check", "preview/app.js"])
    else:
        print("  JavaScript syntax: skipped (node unavailable)")


def verify_fixtures() -> None:
    expected = {
        "lua/ledger.lua", "python/ledger.py", "go/ledger.go", "rust/ledger.rs",
        "typescript/ledger.ts", "javascript/ledger.js", "shell/ledger.sh", "json/ledger.json",
        "yaml/ledger.yaml", "toml/ledger.toml", "markdown/ledger.md", "diff/ledger.diff",
    }
    actual = {str(path.relative_to(ROOT / "fixtures")) for path in (ROOT / "fixtures").glob("*/*") if path.is_file()}
    require(actual == expected, f"fixture set mismatch: expected {sorted(expected)}, found {sorted(actual)}")
    json.loads((ROOT / "fixtures" / "json" / "ledger.json").read_text(encoding="utf-8"))
    tomllib.loads((ROOT / "fixtures" / "toml" / "ledger.toml").read_text(encoding="utf-8"))
    compile((ROOT / "fixtures" / "python" / "ledger.py").read_text(encoding="utf-8"), "ledger.py", "exec")
    command(["sh", "-n", "fixtures/shell/ledger.sh"])
    node = shutil.which("node")
    if node:
        command([node, "--check", "fixtures/javascript/ledger.js"])
    gofmt = shutil.which("gofmt")
    if gofmt:
        result = command([gofmt, "-d", "fixtures/go/ledger.go"])
        require(not result.stdout, "Go fixture is not gofmt-clean")


def verify_neovim() -> None:
    neovim = shutil.which("nvim")
    if neovim is None:
        raise VerificationError("nvim is required for the Neovim runtime checks")
    result = command([neovim, "--headless", "-u", "NONE", "-l", "tests/nvim_spec.lua"])
    output = (result.stdout + result.stderr).strip()
    require("Neovim runtime: pass" in output, f"Neovim test did not report success:\n{output}")
    print("  " + output)


def verify_documentation() -> None:
    required = [
        "README.md", "CHANGELOG.md", "CONTRIBUTING.md", "LICENSE", "THIRD_PARTY_REFERENCES.md",
        "docs/ACCESSIBILITY.md", "docs/ARTIFACTS.md", "docs/GHOSTTY.md", "docs/NEOVIM.md",
        "docs/PALETTE.md", "docs/RESEARCH.md", "docs/SHADERS.md", "docs/DEVELOPMENT.md",
    ]
    for relative in required:
        require((ROOT / relative).is_file(), f"required documentation is missing: {relative}")
    markdown_link = re.compile(r"(?<!!)\[[^\]]+\]\(([^)]+)\)")
    for path in [ROOT / item for item in required if item.endswith(".md")]:
        for target in markdown_link.findall(path.read_text(encoding="utf-8")):
            target = target.strip().split(maxsplit=1)[0].strip("<>")
            if target.startswith(("#", "http://", "https://", "mailto:")):
                continue
            resolved = (path.parent / target.split("#", 1)[0]).resolve()
            require(resolved.exists(), f"broken local link in {path.relative_to(ROOT)}: {target}")


def verify_repository_hygiene() -> None:
    command([sys.executable, "-m", "compileall", "-q", "scripts", "tests"])
    command(["git", "diff", "--check"])
    tracked = command(["git", "ls-files", "AGENTS.md"]).stdout.strip()
    require(not tracked, "workspace-provided AGENTS.md must remain untracked")


CHECKS: tuple[tuple[str, Callable[[], None]], ...] = (
    ("palette schema and semantic roles", verify_palette),
    ("deterministic generation and source-of-truth policy", verify_generation),
    ("contrast and accent distinguishability", verify_contrast),
    ("colour-vision simulations and redundant state", verify_colour_vision),
    ("ANSI identity, contrast, and normal/bright separation", verify_ansi),
    ("Ghostty themes and paired configuration", verify_ghostty),
    ("static shader structure and compilation", verify_shaders),
    ("preview laboratory and committed renders", verify_preview),
    ("language and terminal fixtures", verify_fixtures),
    ("Neovim runtime, switching, and setup API", verify_neovim),
    ("documentation presence and local links", verify_documentation),
    ("repository formatting and hygiene", verify_repository_hygiene),
)


def main() -> int:
    failures: list[str] = []
    for label, check in CHECKS:
        try:
            check()
        except Exception as error:  # report every independent check in one run
            failures.append(f"{label}: {error}")
            print(f"FAIL {label}\n  {error}")
        else:
            print(f"PASS {label}")
    if failures:
        print(f"\n{len(failures)} verification check(s) failed", file=sys.stderr)
        return 1
    print(f"\nPASS all {len(CHECKS)} verification checks")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
