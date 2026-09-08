# Research notes

Research was performed on 2026-09-09 before palette construction. These notes
record consequential technical and design decisions; the source ledger is in
[`THIRD_PARTY_REFERENCES.md`](../THIRD_PARTY_REFERENCES.md).

## Name collision check

The chosen name is **Quireveil**, with the lowercase slug `quireveil`. A quire
is a gathering of folded paper; “veil” suggests the theme's restrained surface
texture. The name is descriptive without borrowing an ink manufacturer's name.

The point-in-time check covered:

- GitHub repository and code search (`gh search repos/code quireveil`)
- the installed Ghostty 1.3.1 theme catalog (`ghostty +list-themes --plain`)
- npm, PyPI, crates.io, and RubyGems registry endpoints
- web searches for Neovim plugins, Ghostty themes, software packages, and exact
  trademark wording

No exact project, plugin, theme, registry package, or relevant exact-word mark
was found. Unrelated search results were substring/OCR matches. This is a basic
collision screen, not a legal clearance opinion, and registry availability can
change after the access date.

## Verified technical behavior

### Ghostty 1.3

- Theme files are configuration files discovered first in
  `$XDG_CONFIG_HOME/ghostty/themes` (normally `~/.config/ghostty/themes`) and
  then in Ghostty's resources `themes` directory.
- A paired theme requires both selectors and accepts
  `theme = light:Quireveil Light,dark:Quireveil Dark`. Whitespace is trimmed;
  selector order is not significant. Paired automatic theme selection is
  documented as available since Ghostty 1.3.0.
- Theme-safe color settings used here are `background`, `foreground`,
  `cursor-color`, `cursor-text`, `selection-background`,
  `selection-foreground`, and repeated `palette = N=#RRGGBB` entries.
- Repeated `custom-shader` entries execute in declaration order. Every later
  shader receives the previous stage through `iChannel0`.
- Custom shaders provide `iChannel0`, `iResolution`, and other Shadertoy-like
  uniforms, plus Ghostty-specific terminal color uniforms. The entry point is
  `void mainImage(out vec4 fragColor, in vec2 fragCoord)`.
- `custom-shader-animation = false` redraws only with terminal updates. The
  shaders therefore avoid `iTime`, `iFrame`, and any temporal state.
- Shader compilation happens on the render thread. Configuration validation
  does not prove shader compilation, and an invalid shader can yield a black
  terminal surface. Loading remains explicitly opt-in.

### Neovim 0.10+

- `vim.api.nvim_set_hl(0, name, value)` replaces the full global highlight
  definition unless `update` is requested. A `link` takes precedence over
  other attributes in the same definition.
- Tree-sitter captures use `@capture` highlight groups and a documented
  fallback hierarchy. Their default extmark priority is 100.
- LSP semantic tokens add type, modifier, and type/modifier groups on top of
  Tree-sitter: `@lsp.type.<type>.<ft>`, `@lsp.mod.<modifier>.<ft>`, and
  `@lsp.typemod.<type>.<modifier>.<ft>`. Modifier and typemod priorities are
  respectively one and two above the semantic-token base priority.
- Because server legends can be inconsistent and may contain off-spec names,
  Quireveil maps standard semantic types conservatively, keeps the Tree-sitter
  baseline complete, and uses modifiers mainly for non-color cues such as
  readonly emphasis and deprecated strikethrough.
- Terminal colors are exposed through `vim.g.terminal_color_0` through `15`.

## Ink and paper translation

Physical ink photographs are not colorimetric references. Line width, feed,
paper absorbency and coating, illumination, camera white balance, scanning,
compression, and display calibration all change appearance. Quireveil borrows
relationships instead: blue-black depth, pooling-like value steps, moss before
teal in non-blue prominence, and a small set of wine, violet, ochre, sepia, and
graphite accents.

Pilot's current catalog documents the requested Iroshizuku families across
deep blue, cerulean, moonlit blue, peacock, forest green, wine, and violet.
LAMY's current ranges document blue-black/Benitoite alongside Amazonite,
Petrol, Peridot, Azurite, Dark Lilac, Topaz, and Sepia. Sailor's blue-black and
Manyo shading language informed density variation; Diamine's Oxford Blue,
Oxblood, Ancient Copper, and Lady Grey and Pelikan Edelstein's Olivine,
Aquamarine, Smoky Quartz, Amethyst, Garnet, and Amber helped calibrate the
non-blue families. No sampled photograph or vendor swatch was copied.

Warm ivory and near-black navy are the two “paper” anchors. Cool cotton white
and neutral cream are calibration references for avoiding yellow cast in the
light mode and chalkiness in the dark mode. Neither normal foreground nor
background uses pure black or pure white.

## Paleto lessons

The nine requested Paleto palettes are CC0 according to their itch.io pages.
The pages also carry an AI-assisted disclosure for graphics, and several newer
pages disclose assisted code and sound as well. They were reviewed as compact
relationship studies only: Deep Sea for a navy/cyan/coral span, Water for teal
depth, Green for moss/bark/birch, Stone for neutral structure, Sky for pale
beam balance, Western / Desert for ochre/leather/cactus, Bloom for controlled
rose/violet/yellow, Dead City for ash/rust/dried red, and Fantasy RPG for
earth/gold/arcane-blue grouping. No downloadable asset or exact Paleto value is
included in this repository.

## Design constraints carried forward

- Opaque surfaces are the baseline; transparency is a user override.
- Principal text targets 7:1 where possible; ordinary meaningful text must
  reach 4.5:1 against its normal background; boundaries and non-text state
  indicators must reach 3:1 where the WCAG contrast model applies.
- Diagnostics and diffs combine hue with undercurls, signs, boldness, or
  background tint. Recommended sign text is `E`, `W`, `I`, and `H`.
- ANSI identities stay conventional and bright variants differ in lightness
  and/or chroma. They are not repainted into a blue monochrome.
- The generator works entirely in six-digit sRGB. No hidden interpolation or
  display-dependent conversion is used, keeping regeneration deterministic.

