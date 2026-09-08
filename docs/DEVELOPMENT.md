# Development guide

Development requires Python 3.11 or newer and Neovim 0.10 or newer. The generator and verification harness use only the Python standard library. Node.js adds JavaScript syntax checks, Ghostty adds runtime configuration validation, and `glslc` adds shader compilation; the suite reports optional checks as skipped when those tools are unavailable. Neovim is required because a structural check cannot replace actual colorscheme loading.

## Commands

```sh
make generate        # regenerate every palette-derived file
make check-generated # fail if generated files drift
make shaders         # compile GLSL when glslc is installed
make preview         # serve http://127.0.0.1:8765/preview/
make verify          # complete release suite
```

`make verify` performs schema and role checks, deterministic generation comparison, contrast and OKLab distinguishability tests, color-vision simulation, ANSI checks, Ghostty structure/runtime validation, shader structure/compilation, preview and PNG validation, fixture parsing, Neovim headless tests, local documentation-link checks, Python compilation, and `git diff --check`.

The release-candidate environment was Fedora Linux 43 with Python 3.14.7, Node.js 22.22.2, Neovim 0.11.6, Ghostty 1.3.1, Chromium 151.0.7922.173, `playwright-cli` 0.1.19, and `glslc`/shaderc 2026.1.

## Generated-file policy

Edit `palette/palette.json`, never its consumers. `scripts/generate.py` owns:

- `colors/nib*.lua`
- `lua/nib/palette/*.lua`
- `ghostty/themes/nib-light` and `nib-dark`
- `preview/generated/palette.css` and `palette.js`
- `dist/nib-palette.json`
- `docs/generated/CONTRAST.md`, `ANSI.md`, and `COLOR_VISION.md`

The generator writes stable ordering and a marker into every output. Check mode computes the expected content in memory and fails without modifying files. Review all generated differences because a single canonical change intentionally reaches several applications.

## Runtime tests

The Neovim test runs exactly:

```sh
nvim --headless -u NONE -l tests/nvim_spec.lua
```

It checks automatic and explicit styles, required built-in/Tree-sitter/LSP/plugin groups, terminal colors 0–15, the setup API, integration disabling, transparency, italics, and stale-highlight removal.

When Ghostty is installed, verification copies the themes into a temporary configuration root and runs `ghostty +validate-config` against the paired example. When it is unavailable, structural validation still runs and the suite reports the runtime check as skipped. Manual validation on a Ghostty host is:

```sh
temporary=$(mktemp -d)
mkdir -p "$temporary/ghostty/themes"
cp ghostty/themes/* "$temporary/ghostty/themes/"
XDG_CONFIG_HOME="$temporary" ghostty +validate-config \
  --config-file="$PWD/ghostty/examples/paired.conf"
```

Remove the temporary directory after inspection. Ghostty configuration validation does not compile shader bodies; `scripts/validate_shaders.py` wraps and compiles all four stages with `glslc` when present.

## Preview artifacts

The browser laboratory has no build step beyond palette generation and makes no runtime network request. Screenshot reproduction, environment versions, dimensions, and inspection results are in [ARTIFACTS.md](ARTIFACTS.md). Captures should be reviewed at native size for overflow, clipping, font fallback, and misleading labels before commit.
