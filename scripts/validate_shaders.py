#!/usr/bin/env python3
"""Compile Ghostty shader bodies with a small compatible GLSL wrapper."""

from __future__ import annotations

import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PREFIX = """#version 330 core
uniform sampler2D iChannel0;
uniform vec3 iResolution;
uniform vec3 iBackgroundColor;
out vec4 qv_output;
"""
SUFFIX = """
void main() {
    mainImage(qv_output, gl_FragCoord.xy);
}
"""


def main() -> int:
    compiler = shutil.which("glslc")
    if compiler is None:
        print("shader compile: skipped (glslc unavailable)")
        return 0

    shaders = sorted((ROOT / "ghostty" / "shaders").glob("*.glsl"))
    if not shaders:
        print("shader compile: failed (no shaders found)", file=sys.stderr)
        return 1

    with tempfile.TemporaryDirectory(prefix="quireveil-shaders-") as temp:
        for shader in shaders:
            wrapped = Path(temp) / shader.name
            wrapped.write_text(PREFIX + shader.read_text(encoding="utf-8") + SUFFIX, encoding="utf-8")
            output = Path(temp) / f"{shader.name}.spv"
            result = subprocess.run(
                [
                    compiler,
                    "--target-env=opengl",
                    "-fshader-stage=frag",
                    "-fauto-map-locations",
                    "-fauto-bind-uniforms",
                    str(wrapped),
                    "-o",
                    str(output),
                ],
                text=True,
                capture_output=True,
            )
            if result.returncode != 0:
                print(f"shader compile: failed ({shader.relative_to(ROOT)})", file=sys.stderr)
                print(result.stdout, end="", file=sys.stderr)
                print(result.stderr, end="", file=sys.stderr)
                return result.returncode
            print(f"shader compile: pass ({shader.relative_to(ROOT)})")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
