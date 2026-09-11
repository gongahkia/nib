# Palette system

`palette/palette.json` is Nib's sole color source. `palette/schema.json` validates its shape, semantic role inventory, color encoding, and 16 ANSI entries. Generated consumers carry a warning and must not be edited directly.

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

The authored format is six-digit uppercase sRGB. No hidden OKLCH interpolation is currently used: regeneration is a direct deterministic translation of the checked-in values. Any future conversion step must be deterministic, documented, gamut-controlled, and tested before becoming part of the generator.

## Changing a color

Change the smallest appropriate semantic role in `palette/palette.json`, run `make generate`, inspect every generated diff, review the preview in both modes, and run `make verify`. Do not solve a contrast failure by lowering its legitimate target. Physical ink swatches remain qualitative references because nib, feed, paper, lighting, capture, and display conditions make exact screen extraction misleading.
