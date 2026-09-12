# Palette system

Nib now separates reusable colour decisions from application semantics:

```text
foundation ramps (`palette/foundation.json`)
    → light/dark aliases (`palette/aliases.json`)
        → locked semantic roles (`palette/palette.json`)
            → generated application ports
```

The foundation and alias schemas validate their authored layers. The existing
semantic schema still validates role inventory, color encoding, and all 16 ANSI
entries, while `palette/lock.json` pins the approved semantic file by SHA-256.
Verification resolves every alias and requires it to match the corresponding
locked semantic role. Generated consumers carry a warning and must not be
edited directly.

## Foundation ramps

Neutral, blue-black, moss, teal, burgundy, rust, violet, amber, sepia, and
graphite each provide 13 reusable steps from `50` through `950`. The accent
`400` values are the approved dark-mode anchors and the `600` values are the
approved light-mode anchors. Intermediate and outer steps were deliberately
interpolated in a perceptual colour space, gamut-clipped to sRGB, and reviewed
in the canonical showcase. They extend the system; they do not retroactively
claim that the original palette came from a mathematical ramp.

Specialized selection, diff, border, and state values remain explicit approved
anchors. They should move onto a ramp only after visual review demonstrates
that the replacement preserves their semantic contrast.

## Visual model

Light mode is cool-neutral paper with a faint blue-grey cast, marked with deep blue-black. Dark mode is a separate near-black charcoal leaf with a restrained warm-green cast, marked with soft ivory; it is not a numerical inversion. The surfaces were refined after a personal [blind preference audit](BLIND_AUDIT.md): the light page moved away from yellowed ivory, while the dark page moved away from navy toward a chalkboard character.

The accent order is intentional:

1. Deep blue and blue-black establish structure.
2. Moss and olive are the leading non-blue family and carry types and structural concepts.
3. Teal and blue-green carry strings and related literal content.
4. Burgundy and rust identify control, exceptions, errors, and warm state.
5. Dusty violet marks attributes, macros, and special constructs.
6. Amber, ochre, and sepia support numbers, search, and properties.
7. Graphite quiets operators, punctuation, and secondary structure.

Modes preserve those identities while independently tuning lightness and chroma. The generated [contrast table](generated/CONTRAST.md) is the authoritative readable-value view; the generated [machine export](../dist/nib-palette.json) provides flattened roles for tooling.

## Role families

- `background`, `surface.*`, and `border.*` form the paper and UI layers.
- `foreground.*` separates principal, secondary, comment/muted, and disabled text.
- `blue_ink.*`, `moss.*`, `teal.*`, and the named warm/violet/neutral accents form syntax hierarchy.
- `diagnostic.*`, `selection.*`, `search.*`, `match.*`, `focus`, and `hyperlink` represent interaction and meaning.
- `diff.*` separates full-line surfaces from stronger inline spans.
- `cursor.*` provides a measured cursor/text pair.
- `ansi` contains ordered conventional identities 0–15.

The authored format is six-digit uppercase sRGB. Ramp construction used OKLCH
as a design tool, but regeneration performs no hidden interpolation: it directly
translates reviewed checked-in values. Any future conversion step must be
deterministic, documented, gamut-controlled, and tested before becoming part
of the generator.

## Locked colors

The palette was locked after the completed blind audit and final side-by-side approval. Adding a platform must map its roles to these values without altering them. A future color revision requires an explicit design decision, a deliberate `palette/lock.json` hash update, regeneration, visual review in both modes, and `make verify`. Do not solve a contrast failure by lowering its legitimate target. Physical ink swatches remain qualitative references because nib, feed, paper, lighting, capture, and display conditions make exact screen extraction misleading.
