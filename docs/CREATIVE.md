# Creative and data palettes

These foundation-driven exports make Nib usable outside code editors without
inventing application-specific colors. They are **Generated** until exercised
and captured in their host applications.

## GIMP

Import [`gimp/Nib.gpl`](../gimp/Nib.gpl) through GIMP's palette dialog. It
contains the mode anchor at steps `400` and `600` for each foundation family;
the complete ramps remain available in
[`dist/nib-foundation.json`](../dist/nib-foundation.json).

## Matplotlib

Load [`python-matplotlib/nib.py`](../python-matplotlib/nib.py) as a local module
and call `apply("light")` or `apply("dark")` before constructing a figure. The
module updates figure, axes, text, ticks, and a nine-color cycle. It imports
Matplotlib only when `apply` is called, so the file can be inspected without
the dependency installed.

## R

Source [`r/nib.R`](../r/nib.R) to define the named `nib_light` and `nib_dark`
vectors. Each contains the nine writing-material accent families for use with
base plotting or another visualization system.
