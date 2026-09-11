#!/usr/bin/env python3
"""Dependency-free release checks for Nib."""

from __future__ import annotations

import json
import math
import os
import re
import shutil
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
    require(PALETTE["meta"]["slug"] == "nib", "unexpected palette slug")
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
    require(
        sorted(path.name for path in (ROOT / "colors").glob("*.lua")) == ["nib-dark.lua", "nib-light.lua", "nib.lua"],
        "public Neovim entry points must be exactly nib, nib-light, and nib-dark",
    )
    require(
        sorted(path.name for path in (ROOT / "ghostty" / "themes").iterdir()) == ["nib-dark", "nib-light"],
        "public Ghostty variants must be exactly nib-light and nib-dark",
    )
    require((ROOT / "lua" / "nib" / "init.lua").is_file(), "nib Lua namespace is missing")
    first = render_files(PALETTE)
    second = render_files(load_palette(ROOT))
    require(first == second, "generator output is not deterministic")
    for relative, content in first.items():
        require(MARKER in "\n".join(content.splitlines()[:3]), f"generated marker missing: {relative}")
    authored = [
        ROOT / "colors" / "nib.lua",
        ROOT / "lua" / "nib" / "init.lua",
        ROOT / "lua" / "nib" / "highlights.lua",
        ROOT / "lua" / "nib" / "integrations.lua",
        ROOT / "lua" / "lualine" / "themes" / "nib.lua",
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
    highlights = (ROOT / "lua" / "nib" / "highlights.lua").read_text(encoding="utf-8")
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


def canonical_colors(mode: dict[str, Any]) -> set[str]:
    return {color for _, color in iter_colors(mode)}


def validate_elisp_structure(source: str, path: Path) -> None:
    stack: list[str] = []
    pairs = {")": "(", "]": "["}
    in_string = False
    escaped = False
    in_comment = False
    for character in source:
        if in_comment:
            if character == "\n":
                in_comment = False
            continue
        if in_string:
            if escaped:
                escaped = False
            elif character == "\\":
                escaped = True
            elif character == '"':
                in_string = False
            continue
        if character == ";":
            in_comment = True
        elif character == '"':
            in_string = True
        elif character in "([":
            stack.append(character)
        elif character in ")]":
            require(bool(stack) and stack.pop() == pairs[character], f"{path}: unbalanced {character}")
    require(not in_string, f"{path}: unterminated string")
    require(not stack, f"{path}: unclosed delimiter {stack[-1] if stack else ''}")


def verify_emacs() -> None:
    theme_directory = ROOT / "emacs"
    require(
        sorted(path.name for path in theme_directory.glob("*-theme.el")) == ["nib-dark-theme.el", "nib-light-theme.el"],
        "Emacs variants must be exactly nib-light and nib-dark",
    )
    required_faces = {
        "default", "cursor", "region", "mode-line", "line-number-current-line",
        "font-lock-comment-face", "font-lock-function-name-face", "font-lock-keyword-face",
        "font-lock-string-face", "font-lock-type-face", "diff-added", "diff-removed",
        "flymake-error", "flymake-warning", "org-level-1", "markdown-code-face",
    }
    for style in ("light", "dark"):
        path = theme_directory / f"nib-{style}-theme.el"
        source = path.read_text(encoding="utf-8")
        validate_elisp_structure(source, path)
        forms = "\n".join(line for line in source.splitlines() if not line.lstrip().startswith(";")).lstrip()
        require(forms.startswith(f"(deftheme nib-{style}"), f"{path}: deftheme is not the first form")
        require(f"(provide-theme 'nib-{style})" in source, f"{path}: provide-theme is missing")
        require(all(f"'({face} " in source for face in required_faces), f"{path}: required face coverage is incomplete")
        colors = set(re.findall(r"#[0-9A-F]{6}", source))
        require(colors <= canonical_colors(PALETTE["modes"][style]), f"{path}: contains a non-canonical color")
        ansi = [entry["hex"] for entry in PALETTE["modes"][style]["ansi"]]
        require(all(color in source for color in ansi), f"{path}: ANSI colors are incomplete")

    emacs = shutil.which("emacs")
    if emacs is None:
        print("  Emacs runtime: skipped (emacs unavailable; Lisp structure and generated colors passed)")
        return
    result = command([emacs, "--batch", "-Q", "-l", "tests/emacs_spec.el"])
    output = (result.stdout + result.stderr).strip()
    require("Emacs runtime: pass" in output, f"Emacs test did not report success:\n{output}")
    version = command([emacs, "--batch", "-Q", "--eval", "(princ emacs-version)"]).stdout.strip()
    print(f"  Emacs runtime: pass ({version})")


def verify_vscode() -> None:
    extension = ROOT / "vscode"
    require((extension / "LICENSE").read_bytes() == (ROOT / "LICENSE").read_bytes(), "VS Code licence copy drifted")
    package = json.loads((extension / "package.json").read_text(encoding="utf-8"))
    require(package["name"] == "nib-color-theme", "VS Code extension name changed")
    require(package["version"] == PALETTE["meta"]["version"], "VS Code extension version drifted")
    require(package["engines"]["vscode"] == f"^{PALETTE['meta']['minimum_vscode']}", "VS Code baseline drifted")
    contributions = package["contributes"]["themes"]
    require(
        contributions
        == [
            {"label": "nib-light", "uiTheme": "vs", "path": "./themes/nib-light-color-theme.json"},
            {"label": "nib-dark", "uiTheme": "vs-dark", "path": "./themes/nib-dark-color-theme.json"},
        ],
        "VS Code/Cursor theme contributions are not the exact light/dark pair",
    )
    required_colors = {
        "editor.background", "editor.foreground", "editorCursor.foreground",
        "editor.selectionBackground", "editor.findMatchBackground", "editor.lineHighlightBackground",
        "editorError.foreground", "editorWarning.foreground", "editorInfo.foreground", "editorHint.foreground",
        "diffEditor.insertedTextBackground", "diffEditor.removedTextBackground",
        "editorSuggestWidget.background", "statusBar.background", "sideBar.background",
    }
    required_semantic = {
        "type", "class", "function", "method", "keyword", "string", "comment",
        "*.readonly", "*.deprecated", "variable.defaultLibrary", "function.defaultLibrary",
    }
    required_scope_fragments = {
        "comment", "string", "constant.numeric", "entity.name.function", "entity.name.type",
        "keyword", "keyword.control", "keyword.operator", "invalid.deprecated", "markup.heading",
    }
    ansi_names = ("Black", "Red", "Green", "Yellow", "Blue", "Magenta", "Cyan", "White")
    for style in ("light", "dark"):
        path = extension / "themes" / f"nib-{style}-color-theme.json"
        theme = json.loads(path.read_text(encoding="utf-8"))
        require(theme["$schema"] == "vscode://schemas/color-theme", f"{path}: wrong schema")
        require(theme["name"] == f"nib-{style}", f"{path}: wrong theme name")
        require(theme["semanticHighlighting"] is True, f"{path}: semantic highlighting is disabled")
        require(required_colors <= theme["colors"].keys(), f"{path}: workbench/editor coverage is incomplete")
        require(required_semantic <= theme["semanticTokenColors"].keys(), f"{path}: semantic coverage is incomplete")
        scopes = {
            scope
            for rule in theme["tokenColors"]
            for scope in ([rule["scope"]] if isinstance(rule["scope"], str) else rule["scope"])
        }
        require(required_scope_fragments <= scopes, f"{path}: TextMate baseline is incomplete")
        serialized_colors = set(re.findall(r"#[0-9A-Fa-f]{6,8}\b", path.read_text(encoding="utf-8")))
        require(all(HEX_RE.fullmatch(color) for color in serialized_colors), f"{path}: colors must be uppercase six-digit sRGB")
        require(serialized_colors <= canonical_colors(PALETTE["modes"][style]), f"{path}: contains a non-canonical color")
        ansi = PALETTE["modes"][style]["ansi"]
        for index, name in enumerate(ansi_names):
            require(theme["colors"][f"terminal.ansi{name}"] == ansi[index]["hex"], f"{path}: ANSI {index} drifted")
            require(theme["colors"][f"terminal.ansiBright{name}"] == ansi[index + 8]["hex"], f"{path}: ANSI {index + 8} drifted")
        for foreground, background in (
            ("button.foreground", "button.background"),
            ("button.secondaryForeground", "button.secondaryBackground"),
            ("badge.foreground", "badge.background"),
            ("input.foreground", "input.background"),
            ("dropdown.foreground", "dropdown.background"),
            ("list.activeSelectionForeground", "list.activeSelectionBackground"),
            ("list.inactiveSelectionForeground", "list.inactiveSelectionBackground"),
            ("list.focusForeground", "list.focusBackground"),
            ("activityBarBadge.foreground", "activityBarBadge.background"),
            ("editor.foreground", "editor.background"),
            ("editor.selectionForeground", "editor.selectionBackground"),
            ("editor.findMatchForeground", "editor.findMatchBackground"),
            ("editor.findMatchHighlightForeground", "editor.findMatchHighlightBackground"),
            ("editorSuggestWidget.selectedForeground", "editorSuggestWidget.selectedBackground"),
            ("peekViewResult.selectionForeground", "peekViewResult.selectionBackground"),
            ("statusBar.debuggingForeground", "statusBar.debuggingBackground"),
            ("menu.selectionForeground", "menu.selectionBackground"),
        ):
            ratio = contrast(theme["colors"][foreground], theme["colors"][background])
            require(ratio >= 4.5, f"{path}: {foreground}/{background} contrast is {ratio:.2f}:1")
    if shutil.which("code") is None and shutil.which("cursor") is None:
        print("  VS Code/Cursor runtime: skipped (editors unavailable; extension structure passed)")


def verify_zed() -> None:
    extension = ROOT / "zed"
    require((extension / "LICENSE").read_bytes() == (ROOT / "LICENSE").read_bytes(), "Zed licence copy drifted")
    manifest = tomllib.loads((extension / "extension.toml").read_text(encoding="utf-8"))
    require(manifest["id"] == "nib-theme", "Zed extension ID changed")
    require(manifest["name"] == "nib", "Zed extension name changed")
    require(manifest["version"] == PALETTE["meta"]["version"], "Zed extension version drifted")
    require(manifest["schema_version"] == 1, "Zed extension manifest schema changed")
    path = extension / "themes" / "nib.json"
    family = json.loads(path.read_text(encoding="utf-8"))
    require(
        family["$schema"] == f"https://zed.dev/schema/themes/v{PALETTE['meta']['zed_theme_schema']}.json",
        "Zed theme schema drifted",
    )
    require(family["name"] == "nib" and family["author"] == "nib contributors", "Zed family metadata changed")
    require([theme["name"] for theme in family["themes"]] == ["nib-light", "nib-dark"], "Zed variants changed")
    require([theme["appearance"] for theme in family["themes"]] == ["light", "dark"], "Zed appearances changed")
    required_style = {
        "background.appearance", "background", "surface.background", "elevated_surface.background",
        "border", "border.focused", "text", "text.muted", "editor.background", "editor.foreground",
        "editor.active_line.background", "editor.line_number", "editor.active_line_number",
        "search.match_background", "search.active_match_background", "error", "warning", "info", "hint", "success", "players", "syntax",
        "terminal.background", "terminal.foreground",
    }
    required_syntax = {
        "attribute", "boolean", "comment", "constant", "constructor", "function", "keyword",
        "number", "operator", "property", "punctuation", "string", "type", "variable",
        "diff.plus", "diff.minus",
    }
    ansi_names = ("black", "red", "green", "yellow", "blue", "magenta", "cyan", "white")
    for style, theme in zip(("light", "dark"), family["themes"]):
        values = theme["style"]
        require(required_style <= values.keys(), f"Zed {style}: UI/editor coverage is incomplete")
        require(values["background.appearance"] == "opaque", f"Zed {style}: core theme is not opaque")
        require(len(values["players"]) == 8, f"Zed {style}: collaborative cursor palette is incomplete")
        require(values["players"][0]["selection"] == PALETTE["modes"][style]["selection"]["background"], f"Zed {style}: local selection drifted")
        require(required_syntax <= values["syntax"].keys(), f"Zed {style}: syntax coverage is incomplete")
        for highlight, definition in values["syntax"].items():
            require(set(definition) <= {"color", "font_style", "font_weight"}, f"Zed {style}: invalid syntax style {highlight}")
            require("color" in definition, f"Zed {style}: syntax style has no color: {highlight}")
        serialized_colors = set(re.findall(r"#[0-9A-Fa-f]{6,8}\b", json.dumps(theme)))
        require(all(HEX_RE.fullmatch(color) for color in serialized_colors), f"Zed {style}: colors must be uppercase six-digit sRGB")
        require(serialized_colors <= canonical_colors(PALETTE["modes"][style]), f"Zed {style}: contains a non-canonical color")
        ansi = PALETTE["modes"][style]["ansi"]
        for index, name in enumerate(ansi_names):
            require(values[f"terminal.ansi.{name}"] == ansi[index]["hex"], f"Zed {style}: ANSI {index} drifted")
            require(values[f"terminal.ansi.bright_{name}"] == ansi[index + 8]["hex"], f"Zed {style}: ANSI {index + 8} drifted")
    if shutil.which("zed") is None:
        print("  Zed runtime: skipped (zed unavailable; v0.2.0 schema structure passed)")


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
    for style in ("light", "dark"):
        path = ROOT / "ghostty" / "themes" / f"nib-{style}"
        values = parse_ghostty_theme(path)
        require(required <= values.keys(), f"{path}: missing theme keys")
        require(set(values) == required | {"palette"}, f"{path}: contains an unaudited theme key")
        require(not any("shader" in key for key in values), f"{path}: core theme unexpectedly loads a shader")
        indexes = [int(entry.split("=", 1)[0]) for entry in values["palette"]]
        require(indexes == list(range(16)), f"{path}: ANSI palette is incomplete")
    paired = (ROOT / "ghostty" / "examples" / "paired.conf").read_text(encoding="utf-8")
    require("theme = light:nib-light,dark:nib-dark" in paired, "paired theme syntax is missing")
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
    with tempfile.TemporaryDirectory(prefix="nib-ghostty-") as temporary:
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
        for preset in ("daily", "showcase"):
            shader_config = config_home / f"shaders-{preset}.conf"
            source = (ROOT / "ghostty" / "examples" / f"shaders-{preset}.conf").read_text(encoding="utf-8")
            shader_config.write_text(source.replace("/absolute/path/to/nib", str(ROOT)), encoding="utf-8")
            command([ghostty, "+validate-config", f"--config-file={shader_config}"], env=environment)
    version = command([ghostty, "+version"]).stdout.splitlines()[0]
    print(f"  Ghostty runtime: pass ({version})")


def verify_installer() -> None:
    script = [sys.executable, "scripts/install_ghostty.py"]
    with tempfile.TemporaryDirectory(prefix="nib-installer-") as temporary:
        root = Path(temporary)
        copy_destination = root / "copy" / "themes"
        command(script + ["--dest", str(copy_destination)])
        require(not copy_destination.exists(), "installer dry-run created a destination")
        command(script + ["--dest", str(copy_destination), "--apply"])
        for source in (ROOT / "ghostty" / "themes").iterdir():
            require((copy_destination / source.name).read_bytes() == source.read_bytes(), f"installer copy mismatch: {source.name}")
        command(script + ["--dest", str(copy_destination), "--apply"])

        occupied = copy_destination / "nib-light"
        occupied.write_text("user content\n", encoding="utf-8")
        blocked = subprocess.run(script + ["--dest", str(copy_destination), "--apply"], cwd=ROOT, text=True, capture_output=True)
        require(blocked.returncode == 2, "installer did not refuse an occupied destination")
        require(occupied.read_text(encoding="utf-8") == "user content\n", "blocked install changed user content")
        command(script + ["--dest", str(copy_destination), "--apply", "--force"])
        backups = list(copy_destination.glob("nib-light.bak-*"))
        require(len(backups) == 1, "forced install did not make exactly one backup")
        require(backups[0].read_text(encoding="utf-8") == "user content\n", "installer backup lost user content")

        link_destination = root / "link" / "themes"
        command(script + ["--dest", str(link_destination), "--link", "--apply"])
        for source in (ROOT / "ghostty" / "themes").iterdir():
            installed = link_destination / source.name
            require(installed.is_symlink(), f"link install created a non-symlink: {source.name}")
            require(installed.resolve() == source.resolve(), f"link install points to the wrong source: {source.name}")


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


def verify_preview() -> None:
    parser = PreviewParser()
    parser.feed((ROOT / "preview" / "index.html").read_text(encoding="utf-8"))
    required_ids = {"comparison-grid", "sample-select", "control-status"}
    require(required_ids <= parser.ids, "comparison preview is missing required structure or controls")
    require(parser.resources[:2] == ["data:,", "generated/palette.css"], "preview resource order changed unexpectedly")
    for resource in parser.resources:
        if resource.startswith(("data:", "#", "http://", "https://")):
            continue
        target = (ROOT / "preview" / resource.split("#", 1)[0]).resolve()
        require(target.exists(), f"preview resource is missing: {resource}")
    comparison_data = json.loads((ROOT / "palette" / "comparisons.json").read_text(encoding="utf-8"))
    themes = comparison_data.get("themes", [])
    require(
        [theme.get("slug") for theme in themes]
        == [
            "flexoki",
            "solarized",
            "everforest",
            "rose-pine",
            "kanagawa",
            "gruvbox",
            "papercolor",
            "melange",
            "ayu",
            "tokyo-night",
            "catppuccin",
            "modus",
        ],
        "comparison palette set or order changed unexpectedly",
    )
    required_mode_keys = {"variant", "background", "surface", "foreground", "muted", "accents"}
    required_accents = {"red", "orange", "yellow", "green", "cyan", "blue", "purple", "magenta"}
    for theme in themes:
        require(str(theme.get("source", "")).startswith("https://"), f"{theme.get('slug')}: source URL is missing")
        require(set(theme.get("modes", {})) == {"light", "dark"}, f"{theme.get('slug')}: paired modes are required")
        for style, mode in theme["modes"].items():
            require(required_mode_keys <= set(mode), f"{theme['slug']} {style}: required comparison roles are missing")
            require(set(mode["accents"]) == required_accents, f"{theme['slug']} {style}: accent set is incomplete")
            colors = [mode[key] for key in ("background", "surface", "foreground", "muted")]
            colors.extend(mode["accents"].values())
            require(all(HEX_RE.fullmatch(color) for color in colors), f"{theme['slug']} {style}: invalid color value")
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
        "terminal/transcript.txt",
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
    transcript = (ROOT / "fixtures" / "terminal" / "transcript.txt").read_text(encoding="utf-8")
    for evidence in ("$ ls -F", "$ git status --short", "PASS", "WARN", "FAIL", "$ man nib", "https://", "remote host"):
        require(evidence in transcript, f"terminal fixture lacks representative output: {evidence}")


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
        "docs/ACCESSIBILITY.md", "docs/GHOSTTY.md", "docs/NEOVIM.md",
        "docs/PALETTE.md", "docs/RESEARCH.md", "docs/SHADERS.md", "docs/DEVELOPMENT.md",
        "docs/EMACS.md", "docs/VSCODE.md", "docs/ZED.md",
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
    ruff = shutil.which("ruff")
    if ruff:
        command([ruff, "check", "scripts", "tests"])
    else:
        print("  Python lint: skipped (ruff unavailable; byte compilation passed)")
    command(["git", "diff", "--check"])
    tracked = command(["git", "ls-files", "AGENTS.md"]).stdout.strip()
    require(not tracked, "workspace-provided AGENTS.md must remain untracked")


CHECKS: tuple[tuple[str, Callable[[], None]], ...] = (
    ("palette schema and semantic roles", verify_palette),
    ("deterministic generation and source-of-truth policy", verify_generation),
    ("contrast and accent distinguishability", verify_contrast),
    ("colour-vision simulations and redundant state", verify_colour_vision),
    ("ANSI identity, contrast, and normal/bright separation", verify_ansi),
    ("Emacs theme structure and runtime", verify_emacs),
    ("VS Code and Cursor extension structure", verify_vscode),
    ("Zed extension and theme structure", verify_zed),
    ("Ghostty themes and paired configuration", verify_ghostty),
    ("dry-run installer, backups, and symlinks", verify_installer),
    ("static shader structure and compilation", verify_shaders),
    ("comparison preview and reference palettes", verify_preview),
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
