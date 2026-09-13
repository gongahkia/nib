#!/usr/bin/env python3
"""Install Nib's file-based themes into user-local application directories."""

from __future__ import annotations

import argparse
import os
import shutil
import sys
import tempfile
from pathlib import Path

from install_ghostty import backup_path, same_file

ROOT = Path(__file__).resolve().parents[1]
PORTS = (
    ("alacritty", "alacritty/nib-*.toml", "config", "alacritty"),
    ("dunst", "dunst/nib-*.conf", "config", "dunst"),
    ("emacs", "emacs/nib-*-theme.el", "home", ".emacs.d/themes"),
    ("fish", "fish/nib-*.theme", "config", "fish/themes"),
    ("fzf", "fzf/nib-*.sh", "config", "fzf"),
    ("ghostty", "ghostty/themes/nib-*", "config", "ghostty/themes"),
    ("helix", "helix/nib-*.toml", "config", "helix/themes"),
    ("i3", "i3/nib-*.conf", "config", "i3"),
    ("kitty", "kitty/nib-*.conf", "config", "kitty"),
    ("konsole", "konsole/Nib *.colorscheme", "data", "konsole"),
    ("lite-xl", "lite-xl/nib-*.lua", "config", "lite-xl/colors"),
    ("tmux", "tmux/nib-*.conf", "config", "tmux"),
    ("vim", "vim/colors/nib*.vim", "home", ".vim/colors"),
    ("waybar", "waybar/nib-*.css", "config", "waybar"),
    ("wezterm", "wezterm/Nib *.toml", "config", "wezterm/colors"),
    ("yazi", "yazi/nib-*.toml", "config", "yazi"),
    ("zathura", "zathura/nib-*", "config", "zathura"),
    ("zellij", "zellij/nib.kdl", "config", "zellij/themes"),
)
PORT_NAMES = sorted({name for name, *_ in PORTS} | {"neovim"})


def xdg_home(variable: str, fallback: Path) -> Path:
    value = os.environ.get(variable)
    path = Path(value).expanduser() if value else fallback
    if not path.is_absolute():
        raise ValueError(f"{variable} must be an absolute path: {path}")
    return path


def plan(selected: set[str]) -> list[tuple[str, Path, Path]]:
    home = Path.home()
    roots = {
        "home": home,
        "config": xdg_home("XDG_CONFIG_HOME", home / ".config"),
        "data": xdg_home("XDG_DATA_HOME", home / ".local/share"),
    }
    files: list[tuple[str, Path, Path]] = []
    for name, pattern, root, directory in PORTS:
        if name not in selected:
            continue
        sources = sorted(ROOT.glob(pattern))
        if not sources or any(not source.is_file() for source in sources):
            raise ValueError(f"missing source files for {name}: {pattern}")
        files.extend((name, source, roots[root] / directory / source.name) for source in sources)

    if "neovim" in selected:
        package = roots["data"] / "nvim/site/pack/themes/start/nib"
        for directory, pattern in (("colors", "*.lua"), ("lua", "**/*.lua")):
            source_root = ROOT / directory
            sources = sorted(source_root.glob(pattern))
            if not sources or any(not source.is_file() for source in sources):
                raise ValueError(f"missing Neovim runtime files: {directory}/{pattern}")
            files.extend(
                ("neovim", source, package / directory / source.relative_to(source_root))
                for source in sources
            )

    destinations = [destination for _, _, destination in files]
    if len(destinations) != len(set(destinations)):
        raise ValueError("two Nib source files share an installation destination")
    return sorted(files, key=lambda item: (item[0], str(item[2])))


def install_file(source: Path, destination: Path, force: bool) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.NamedTemporaryFile(prefix=f".{destination.name}.", dir=destination.parent, delete=False) as staged:
        staged_path = Path(staged.name)
    backup = None
    try:
        shutil.copy2(source, staged_path)
        if destination.exists() or destination.is_symlink():
            if not force:
                raise ValueError(f"destination appeared during install: {destination}")
            backup = backup_path(destination)
            destination.rename(backup)
            print(f"backed up: {destination} -> {backup}")
        staged_path.replace(destination)
    except (OSError, ValueError):
        staged_path.unlink(missing_ok=True)
        if backup is not None and not destination.exists() and not destination.is_symlink():
            backup.rename(destination)
        raise


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="perform the displayed changes")
    parser.add_argument("--force", action="store_true", help="back up and replace conflicting Nib paths")
    parser.add_argument("--only", action="append", choices=PORT_NAMES, help="install only this port; repeat as needed")
    parser.add_argument("--list", action="store_true", help="list ports handled by this installer")
    args = parser.parse_args()
    if args.list:
        print("\n".join(PORT_NAMES))
        return 0

    selected = set(args.only or PORT_NAMES)
    try:
        files = plan(selected)
        print(f"mode: {'apply' if args.apply else 'dry-run'}")
        print(f"ports: {', '.join(sorted(selected))}")
        changes: list[tuple[Path, Path]] = []
        blocked = False
        for name, source, destination in files:
            if same_file(source, destination, link=False) or same_file(source, destination, link=True):
                print(f"unchanged [{name}]: {destination}")
                continue
            if name == "neovim" and any(
                parent.name == "nib" and parent.parent.name == "start" and parent.is_symlink()
                for parent in destination.parents
            ):
                print(f"blocked (Neovim package is a symlink) [{name}]: {destination}")
                blocked = True
                continue
            invalid_parent = next(
                (parent for parent in destination.parents if (parent.exists() or parent.is_symlink()) and not parent.is_dir()),
                None,
            )
            if invalid_parent is not None:
                print(f"blocked (parent is not a directory) [{name}]: {invalid_parent}")
                blocked = True
                continue
            if destination.is_dir():
                print(f"blocked (directory) [{name}]: {destination}")
                blocked = True
                continue
            if destination.exists() or destination.is_symlink():
                if not args.force:
                    print(f"blocked (exists; use --force to back up) [{name}]: {destination}")
                    blocked = True
                    continue
                print(f"backup then copy [{name}]: {source} -> {destination}")
            else:
                print(f"copy [{name}]: {source} -> {destination}")
            changes.append((source, destination))

        if not args.apply:
            print("dry-run only; add --apply to make these changes")
            return 1 if blocked else 0
        if blocked:
            print("nothing changed because at least one destination is blocked")
            return 2
        for source, destination in changes:
            install_file(source, destination, args.force)
        print(f"installed {len(changes)} file(s); select Nib in each application's settings")
        return 0
    except (OSError, ValueError) as error:
        print(f"install failed: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
