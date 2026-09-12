# Linux desktop fragments

Nib provides generated light and dark fragments for i3, Waybar, Dunst, and
Zathura. They use only reviewed foundation aliases and do not alter layout,
fonts, key bindings, opacity, or notification behavior.

These ports are **Generated**, not **Verified**. Their syntax and color sources
are checked by `make verify`, but native screenshots and host-specific parsing
remain pending in the [visual acceptance matrix](VISUAL_ACCEPTANCE.md).

## i3

Choose [`i3/nib-light.conf`](../i3/nib-light.conf) or
[`i3/nib-dark.conf`](../i3/nib-dark.conf), then include its absolute path from
your i3 configuration. The fragment defines namespaced variables and client
colors only. Reload i3 and inspect focused, inactive, unfocused, urgent, and
placeholder windows.

## Waybar

Import [`waybar/nib-light.css`](../waybar/nib-light.css) or
[`waybar/nib-dark.css`](../waybar/nib-dark.css) near the start of your Waybar
stylesheet. The fragment colors the bar, workspaces, modes, network failures,
and battery warnings without changing sizing or typography.

## Dunst

Merge [`dunst/nib-light.conf`](../dunst/nib-light.conf) or
[`dunst/nib-dark.conf`](../dunst/nib-dark.conf) into the matching Dunst
configuration. The fragment changes global separators and the three urgency
classes only. Restart Dunst and send low, normal, and critical test
notifications before treating the result as locally verified.

## Zathura

Include or merge [`zathura/nib-light`](../zathura/nib-light) or
[`zathura/nib-dark`](../zathura/nib-dark) in `zathurarc`. The fragment covers
interface, completion, highlight, notification, and document recoloring roles.
Inspect both ordinary and image-heavy PDFs before leaving recoloring enabled.
