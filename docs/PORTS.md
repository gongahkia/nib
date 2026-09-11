# Port installation guide

These ports are generated from the locked Nib palette. Copy or import the
generated files; do not edit them in place because `make generate` replaces
them.

## Editors

### IntelliJ Platform IDEs

Open **Settings → Editor → Color Scheme**, use the scheme menu's **Import
Scheme** action, and import either `intellij/Nib Light.icls` or
`intellij/Nib Dark.icls`.

### Lite XL

Copy `lite-xl/nib-light.lua` and `lite-xl/nib-dark.lua` into the `colors`
directory beneath Lite XL's user directory. On the standard installation this
is `~/.config/lite-xl/colors`. Load a variant from `init.lua`:

```lua
core.reload_module("colors.nib-dark")
```

The optional official `select_colorscheme` plugin can select installed themes
through the command palette.

## Terminals

### Alacritty

Import one generated TOML file from `alacritty.toml`:

```toml
[general]
import = ["~/.config/alacritty/nib-dark.toml"]
```

### Kitty

Copy the files into Kitty's configuration directory and include one from
`kitty.conf`:

```conf
include nib-dark.conf
```

### WezTerm

Copy both TOML files to `~/.config/wezterm/colors/`, then select one in
`.wezterm.lua`:

```lua
config.color_scheme = "Nib Dark"
```

### iTerm2

Open **Settings → Profiles → Colors → Color Presets… → Import…** and choose an
`.itermcolors` file. The preset name follows the imported filename.

### Windows Terminal

Add the objects in `windows-terminal/*.json` to the `schemes` array in
`settings.json`. A profile can follow the application or OS appearance:

```json
"colorScheme": {
  "light": "Nib Light",
  "dark": "Nib Dark"
}
```

### Warp

Copy `warp-terminal/nib-light.yaml` and `nib-dark.yaml` into Warp's custom
themes directory, then choose the variant from the theme picker. Warp documents
the current per-platform directory under **Settings → Appearance**.

### Black Box

Import either JSON file under `black-box/` through Black Box's terminal-theme
preferences. Both files include the complete ANSI palette and selection and
cursor roles.

### Xresources

Merge a variant into an X resource database:

```sh
xrdb -merge /absolute/path/to/nib/xresources/nib-dark
```

This affects applications that consume X resource terminal colors; it does not
theme unrelated desktop components.

## Shell and command-line tools

### fish

Copy both `.theme` files to `~/.config/fish/themes/`, then run:

```fish
fish_config theme choose "Nib Dark"
```

### fzf

Source one generated shell fragment from Bash or Zsh:

```sh
. /absolute/path/to/nib/fzf/nib-dark.sh
```

The fragment preserves existing `FZF_DEFAULT_OPTS` and appends only color
options.

### tmux

Source one variant from `tmux.conf` and reload the configuration:

```tmux
source-file /absolute/path/to/nib/tmux/nib-dark.conf
```

The file changes tmux UI styles only; it does not replace key bindings or
status content.

### Zellij

Copy `zellij/nib.kdl` to the Zellij themes directory and select a variant in
`config.kdl`:

```kdl
theme "nib-dark"
```

### Pywal

Pywal is archived upstream but still accepts predefined JSON themes:

```sh
wal --theme /absolute/path/to/nib/pywal/nib-dark.json
```

## Web projects

`css/nib.css` exposes semantic `--nib-*` custom properties. It follows the OS
with `prefers-color-scheme` and accepts explicit `data-nib-theme="light"` or
`data-nib-theme="dark"` overrides.

For Tailwind CSS v4, import `tailwind/nib.css`. Its `@theme inline` block makes
the semantic roles available as utilities such as `bg-nib-background`,
`text-nib-foreground`, and `border-nib-border`.
