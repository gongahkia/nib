# Blind preference audit

On 11 September 2026, Nib was compared with twelve established themes in a personal, single-participant blind audit. Every matchup used the same TypeScript sample and normalized syntax roles. Opponent order and A/B placement were randomized, identities stayed hidden until export, and light and dark modes were chosen separately.

This is evidence about the author's own long-session preferences, not a population study or a claim that one theme is objectively better.

## Results

Nib won 9 of 12 light-mode choices and 6 of 12 dark-mode choices.

| Opponent | Light choice | Dark choice |
| --- | --- | --- |
| Modus | Nib | Nib |
| Tokyo Night | Tokyo Night | Nib |
| Mélange | Mélange | Mélange |
| Flexoki | Flexoki | Nib |
| Ayu | Nib | Ayu |
| PaperColor | Nib | PaperColor |
| Kanagawa | Nib | Kanagawa |
| Rosé Pine | Nib | Nib |
| Solarized | Nib | Nib |
| Catppuccin | Nib | Nib |
| Everforest | Nib | Everforest |
| Gruvbox | Nib | Gruvbox |

Mélange was the only opponent preferred in both modes. Nib was preferred in both modes over Modus, Rosé Pine, Solarized, and Catppuccin.

## Preference signals

- Nib's dark, restrained light-mode syntax was repeatedly preferred over brighter alternatives.
- The original light background felt too yellow and too close to Gruvbox. Cooler undertones and the backgrounds of Tokyo Night, Flexoki, and Rosé Pine were appealing, provided syntax stayed subdued.
- The original dark background felt too navy and too reminiscent of Tokyo Night. Neutral or faintly warm-green charcoal surfaces described as calm or chalkboard-like were repeatedly preferred.
- Mélange's balance of a non-navy surface and low-brightness syntax was the strongest complete reference.
- Bright reds, high-chroma syntax, and strongly blue or purple dark surfaces caused the most consistent rejection.
- Gruvbox's character remained appealing, but both its syntax and background needed lower visual intensity for long sessions.

## Palette response

The light revision changes the paper and surface ramp from yellowed ivory to cool-neutral grey-white while preserving Nib's successful syntax hierarchy. The dark revision replaces blue-black navy with near-black chalkboard charcoal and reduces the chroma and luminance of its accents. Syntax roles, fountain-ink family order, contrast targets, and non-color state cues remain intact.

The canonical values remain in [`palette/palette.json`](../palette/palette.json). Generated [contrast](generated/CONTRAST.md) and [color-vision](generated/COLOR_VISION.md) reports record the engineering checks applied after the revision.
