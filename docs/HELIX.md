# Helix guide

Nib provides generated `nib-light.toml` and `nib-dark.toml` Helix themes. Both
cover editor UI, syntax, markup, selections, search, diagnostics, and version
control diffs. Diagnostic severity uses labelled gutters plus undercurls; diffs
combine foreground and surface changes.

## Install

Copy both files into the user theme directory:

```sh
mkdir -p ~/.config/helix/themes
cp helix/nib-*.toml ~/.config/helix/themes/
```

On Windows, use `%AppData%\helix\themes`. Then set a theme at the top level of
Helix's `config.toml`:

```toml
theme = "nib-dark"
```

Switch without restarting with `:theme nib-light` or `:theme nib-dark`. Helix
does not combine both appearances in one theme file, so OS-mode automation
belongs in the user's configuration or launcher.

## Remove

Select another theme, then delete `nib-light.toml` and `nib-dark.toml` from the
user theme directory.
