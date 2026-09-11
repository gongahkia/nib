# Accessibility and comfort report

Nib is designed for long, opaque coding sessions and tests a defined set of color relationships. These results are scoped engineering checks, not blanket accessibility certification and not a guarantee for every plugin, font, display, transparency composition, shader, or user condition.

## Contrast targets

- Principal foreground against the primary background targets at least 7:1.
- Ordinary meaningful code, prose, comments, diagnostics, hyperlinks, selected text, search text, diff text, and ANSI text slots 1–15 require at least 4.5:1 against their intended surface.
- Secondary UI boundaries and focus indicators require at least 3:1 against the primary background.

The exact generated ratios are in [CONTRAST.md](generated/CONTRAST.md). Principal text measures 13.01:1 in light mode and 12.70:1 in dark mode. Comment text measures 5.31:1 and 6.25:1 respectively. The narrowest tested boundary ratios are 3.51:1 and 3.14:1.

ANSI index 0 is intentionally a dark building/background color and is not treated as ordinary text in dark mode. Other ANSI slots pass the ordinary-text threshold against their mode background. Normal and bright partners are separately checked for perceptual distance; operational red, green, yellow, blue, magenta, and cyan identities remain distinct.

## Color-vision evaluation

The generator applies deterministic full-severity protanopia, deuteranopia, and tritanopia matrices in linear sRGB, then measures selected semantic pairs in OKLab. The [color-vision report](generated/COLOR_VISION.md) records the results. Distance is used as a regression signal, not a perceptual guarantee or medical model.

Some red/green relationships converge under simulation, especially error/success and diff add/delete. Nib therefore does not rely on hue alone:

- diagnostics use `E`, `W`, `I`, and `H` gutter signs, severity-specific undercurls, and bold signs;
- deprecated content uses strikethrough;
- diff states combine `+`, `~`, and `-` markers with distinct full-line and inline surface tints;
- selection, search, current search, and matching text use different backgrounds and measured text contrast;
- focus and current-line state use restrained boundary or surface changes.

The colorscheme defines the relevant highlight groups. Applications and user configuration remain responsible for showing textual sign glyphs and preserving status labels.

## Comfort constraints

Normal text and backgrounds avoid pure black and pure white. Essential text is never assigned the disabled ramp. Large bright-blue surfaces, neon saturation, full-line red diagnostics, and pervasive bold/italic styling are avoided. Comments are secondary but remain above the ordinary-text threshold.

The editor themes are font-neutral. The browser comparison preview prefers `JetBrainsMono Nerd Font Mono`, then JetBrains Mono and the platform monospace. No font is bundled or required.

Measured results apply to the opaque core themes. Transparency mixes unknown colors beneath the theme, and Ghostty shaders alter the entire rendered surface. Recheck the resulting composition if either option is enabled.
