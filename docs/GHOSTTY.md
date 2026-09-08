# Ghostty guide

Quireveil ships two opaque, shader-free core themes. Ghostty 1.3.0 is the
minimum supported version because that release introduced paired theme
selection. The generated theme files use only audited color settings.

## Install on Linux

Preview the default copy operation first:

```bash
python3 scripts/install_ghostty.py
```

Then install into `$XDG_CONFIG_HOME/ghostty/themes` or, when that variable is
unset, `~/.config/ghostty/themes`:

```bash
python3 scripts/install_ghostty.py --apply
```

To keep a checkout and the installed themes in sync, request absolute
symlinks:

```bash
python3 scripts/install_ghostty.py --link --apply
```

## Install on macOS

Ghostty uses the same user theme location on macOS:

```bash
mkdir -p "$HOME/.config/ghostty/themes"
cp "ghostty/themes/Quireveil Light" "ghostty/themes/Quireveil Dark" \
  "$HOME/.config/ghostty/themes/"
```

The helper above is platform-neutral and can be used instead. Ghostty's
built-in resources directory inside the application bundle is not an install
target; application updates may replace it.

## Select a theme

Use automatic OS appearance selection in `~/.config/ghostty/config`:

```ini
theme = light:Quireveil Light,dark:Quireveil Dark
window-theme = system
background-opacity = 1
```

Both `light:` and `dark:` are required by the current paired syntax. To pin a
mode, use `theme = Quireveil Light` or `theme = Quireveil Dark`. Reload the
configuration from Ghostty after changing it.

The core install has no shader, animation, transparency, font, or shell
dependency. The canonical preview font is `AtkynsonMono Nerd Font Mono`,
Medium, 15 pt when available; it is neither bundled nor required.

## Existing files and removal

The installer is a dry-run unless `--apply` is present. It refuses to replace
any path unless `--force` is also present. Forced replacement first renames the
existing path to `Quireveil Light.bak-YYYYMMDD-HHMMSS` (likewise for dark),
adding a numeric suffix if needed.

To uninstall, remove only the two installed files or symlinks and delete or
replace the `theme` line in your Ghostty config:

```bash
rm "$HOME/.config/ghostty/themes/Quireveil Light" \
  "$HOME/.config/ghostty/themes/Quireveil Dark"
```

Review any `.bak-*` file before restoring or removing it.

## tmux and SSH

The 16 ANSI slots preserve red, green, yellow, blue, magenta, and cyan
identities, so remote programs and multiplexers retain their operational
meaning. Keep Ghostty's default `TERM` unless a remote host has the matching
terminfo entry. If `xterm-ghostty` is missing remotely, install Ghostty's
terminfo there or temporarily use a compatible fallback for that connection.

Truecolor output across tmux also depends on tmux and its terminal-overrides,
but the base ANSI palette remains usable when truecolor is unavailable. Theme
files cannot force a remote application to honor ANSI conventions; programs
that emit fixed RGB values bypass the theme's 16-color mapping.

## Shader opt-in

Shader setup and black-screen recovery are documented in
[`SHADERS.md`](SHADERS.md). The one-line opt-out is to remove or comment every
`custom-shader = ...` entry. No correctness or intended contrast depends on
post-processing.
