# Optional ink and paper shaders

Shaders are optional post-processing. Nib's colors, hierarchy, ANSI
semantics, and measured contrast are designed for the shader-free core.

Ghostty post-processing affects the entire rendered terminal surface,
including Neovim borders, selections, signs, diagnostics, and UI chrome. These
stages simulate display-space texture; they do not model physical ink
absorption. Both stages are static, contain no `iTime`/frame input, require no
external texture, and set `custom-shader-animation = false`.

## Presets and order

Each preset is a two-stage chain:

1. `paper-grain-*.glsl` applies low-amplitude procedural grain mostly to pixels
   close to Ghostty's background color.
2. `ink-feather-*.glsl` takes five samples and applies a restrained edge blend
   plus mode-aware density variation.

The daily preset is deliberately close to imperceptible. The showcase preset
is stronger so the idea remains visible in documentation comparisons; it is
not the recommended long-session default. Both use the background luminance
uniform to calibrate strength and density direction for light and dark modes.

Copy the matching lines from `ghostty/examples/shaders-daily.conf` or
`ghostty/examples/shaders-showcase.conf` into the Ghostty configuration. Replace
the placeholder with the checkout's absolute path. Declaration order matters:

```ini
custom-shader = /absolute/path/to/nib/ghostty/shaders/paper-grain-daily.glsl
custom-shader = /absolute/path/to/nib/ghostty/shaders/ink-feather-daily.glsl
custom-shader-animation = false
```

No shader is copied by the theme installer and neither example is included by
another configuration automatically.

## Opt out and recover

The one-line instruction is: **remove or comment every `custom-shader = ...`
line and reload Ghostty.** Leaving `custom-shader-animation = false` is
harmless.

An invalid custom shader can make a Ghostty surface appear black. If that
happens, edit `~/.config/ghostty/config` from another terminal, TTY, or text
editor; remove all `custom-shader` entries; run:

```bash
ghostty +validate-config
```

Then fully restart Ghostty if the affected surface cannot reload. Preserve a
copy of the configuration before any manual rewrite. Configuration validation
does not compile shaders because Ghostty compiles them later on its render
thread.

## Validation and limits

Run `python3 scripts/validate_shaders.py`. It wraps every Shadertoy-style body
with the subset of Ghostty uniforms used here and compiles it as an OpenGL
fragment shader with `glslc`. This verifies GLSL syntax and types. It does not
prove that a particular Ghostty build, GPU, display color pipeline, or driver
will render identically.

The browser showcase screenshots are authoritative only for the shader-free
reference rendering they depict. They are not Ghostty captures and do not prove
GPU shader output. A shader comparison must be labelled as a deterministic
reference simulation unless an actual Ghostty capture identifies the app,
version, operating system, GPU, and preset.
