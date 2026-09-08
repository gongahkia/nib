#!/usr/bin/env python3
"""Safely plan or install Nib Ghostty theme files."""

from __future__ import annotations

import argparse
import os
import shutil
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCES = tuple(sorted((ROOT / "ghostty" / "themes").iterdir()))


def default_destination() -> Path:
    config_home = os.environ.get("XDG_CONFIG_HOME")
    root = Path(config_home).expanduser() if config_home else Path.home() / ".config"
    return root / "ghostty" / "themes"


def parser() -> argparse.ArgumentParser:
    result = argparse.ArgumentParser(description=__doc__)
    result.add_argument("--dest", type=Path, default=default_destination(), help="theme directory")
    result.add_argument("--apply", action="store_true", help="perform the displayed changes")
    result.add_argument("--link", action="store_true", help="create absolute symlinks instead of copies")
    result.add_argument("--force", action="store_true", help="back up and replace existing paths")
    return result


def backup_path(destination: Path) -> Path:
    stamp = datetime.now().astimezone().strftime("%Y%m%d-%H%M%S")
    candidate = destination.with_name(f"{destination.name}.bak-{stamp}")
    sequence = 1
    while candidate.exists() or candidate.is_symlink():
        candidate = destination.with_name(f"{destination.name}.bak-{stamp}-{sequence}")
        sequence += 1
    return candidate


def same_file(source: Path, destination: Path, link: bool) -> bool:
    if link and destination.is_symlink():
        return destination.resolve() == source.resolve()
    if not link and destination.is_file() and not destination.is_symlink():
        return source.read_bytes() == destination.read_bytes()
    return False


def main() -> int:
    args = parser().parse_args()
    destination_root = args.dest.expanduser().resolve()
    action = "link" if args.link else "copy"
    print(f"mode: {'apply' if args.apply else 'dry-run'}")
    print(f"destination: {destination_root}")

    plans: list[tuple[Path, Path]] = []
    blocked = False
    for source in SOURCES:
        destination = destination_root / source.name
        if same_file(source, destination, args.link):
            print(f"unchanged: {destination}")
            continue
        if destination.exists() or destination.is_symlink():
            if not args.force:
                print(f"blocked (exists; use --force to back up): {destination}")
                blocked = True
                continue
            print(f"backup then {action}: {destination}")
        else:
            print(f"{action}: {source} -> {destination}")
        plans.append((source, destination))

    if not args.apply:
        print("dry-run only; add --apply to make these changes")
        return 1 if blocked else 0
    if blocked:
        print("nothing changed because at least one destination is blocked")
        return 2

    destination_root.mkdir(parents=True, exist_ok=True)
    for source, destination in plans:
        if destination.exists() or destination.is_symlink():
            backup = backup_path(destination)
            destination.rename(backup)
            print(f"backed up: {destination} -> {backup}")
        if args.link:
            destination.symlink_to(source.resolve())
        else:
            shutil.copy2(source, destination)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
