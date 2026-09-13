[![](https://img.shields.io/badge/nib_1.0.0-passing-green)](https://github.com/gongahkia/nib/releases/tag/1.0.0) 
[![](https://github.com/gongahkia/nib/actions/workflows/verify.yml/badge.svg)](https://github.com/gongahkia/nib/actions/workflows/release.yml)
                                                                   
# `Nib` 🖋

A fountain-pen colour system for [sustained](#colors) & [accessible](#accessibility) reading, writing, and code.

## Quick installation

On Linux, install the file-based ports into your user directories with one command:

```sh
git clone --depth 1 https://github.com/gongahkia/nib.git && python3 nib/scripts/install.py --apply
```

From an existing checkout, run `make install`. The installer places light and
dark theme files for ports with known user-local paths, including Ghostty,
Neovim, Helix, Vim, Kitty, Konsole, fish, and others. It does not change app
settings or select a variant; use the [port installation guide](docs/PORTS.md)
and the linked app guides below to activate one. Browser themes, Obsidian,
VS Code, and other GUI imports still use their app-specific instructions.

Run `python3 scripts/install.py` to preview the changes, or add `--only ghostty`
(repeat `--only` for multiple apps) to select ports. Existing paths block the
install before any files change; `--force` backs them up before replacing them.
No root privileges or package manager are needed.

## Ports

`Nib` currently supports the below apps *(and frameworks)*.

* [Alacritty](alacritty/)
* [Black Box](black-box/)
* [Chrome & Chromium](chromium/)
* [Discord *(through BetterDiscord or Vencord)*](docs/MESSAGING.md#discord)
* [Dunst](docs/DESKTOP.md#dunst)
* [GNU Emacs](docs/EMACS.md)
* [GIMP palette](docs/CREATIVE.md#gimp)
* [Firefox](docs/FIREFOX.md)
* [fish](fish/)
* [fzf](fzf/)
* [Ghostty](docs/GHOSTTY.md)
* [Helium](docs/HELIUM.md)
* [Helix](docs/HELIX.md)
* [i3](docs/DESKTOP.md#i3)
* [IntelliJ Platform IDEs](intellij/)
* [iTerm2](iterm2/)
* [Kitty](kitty/)
* [Konsole](docs/PORTS.md#konsole)
* [Lite XL](lite-xl/)
* [macOS Terminal](docs/PORTS.md#macos-terminal)
* [Matplotlib](docs/CREATIVE.md#matplotlib)
* [Neovim](docs/NEOVIM.md)
* [Obsidian](docs/OBSIDIAN.md)
* [Pywal](pywal/)
* [R](docs/CREATIVE.md#r)
* [Slack *(through Slick or Slack's native custom colours)*](docs/MESSAGING.md#slack)
* [Sublime Text](docs/SUBLIME.md)
* [Telegram Desktop](docs/MESSAGING.md#telegram-desktop)
* [tmux](tmux/)
* [Vim](docs/VIM.md)
* [Visual Studio Code & Cursor](docs/VSCODE.md)
* [Warp](warp-terminal/)
* [Waybar](docs/DESKTOP.md#waybar)
* [WezTerm](wezterm/)
* [Windows Terminal](windows-terminal/)
* [Xresources-compatible terminals](xresources/)
* [Yazi](docs/PORTS.md#yazi)
* [Zed](docs/ZED.md)
* [Zathura](docs/DESKTOP.md#zathura)
* [Zellij](zellij/)

### Frameworks

* [CSS custom properties](css/nib.css)
* [Tailwind CSS v4](tailwind/nib.css)

## Screenshots

### Alacritty

<div align="center">
    <img src="./asset/reference/alacritty-1.png" alt="Nib dark theme in Alacritty" width="40%">
    <img src="./asset/reference/alacritty-2.png" alt="Nib light theme in Alacritty" width="40%">
</div>

### Black Box

<div align="center">
    <img src="./asset/reference/black-box-1.png" alt="Nib dark theme in Black Box" width="40%">
    <img src="./asset/reference/black-box-2.png" alt="Nib light theme in Black Box" width="40%">
</div>

### Chromium

<div align="center">
    <img src="./asset/reference/chromium-1.png" alt="Nib dark theme in Chromium" width="40%">
    <img src="./asset/reference/chromium-2.png" alt="Nib light theme in Chromium" width="40%">
</div>

### Fish

<div align="center">
    <img src="./asset/reference/fish-1.png" alt="Nib dark theme in Fish" width="40%">
    <img src="./asset/reference/fish-2.png" alt="Nib light theme in Fish" width="40%">
</div>

### Emacs

<div align="center">
    <img src="./asset/reference/emacs-1.png" alt="Nib dark theme in Emacs" width="40%">
    <img src="./asset/reference/emacs-2.png" alt="Nib light theme in Emacs" width="40%">
</div>

### fzf

<div align="center">
    <img src="./asset/reference/fzf-1.png" alt="Nib dark theme in fzf" width="40%">
    <img src="./asset/reference/fzf-2.png" alt="Nib light theme in fzf" width="40%">
</div>

### Ghostty

<div align="center">
    <img src="./asset/reference/ghostty-1.png" alt="Nib dark theme in Ghostty" width="40%">
    <img src="./asset/reference/ghostty-2.png" alt="Nib light theme in Ghostty" width="40%">
</div>

### Helium

<div align="center">
    <img src="./asset/reference/helium-1.png" alt="Nib dark theme in Helium" width="40%">
    <img src="./asset/reference/helium-2.png" alt="Nib light theme in Helium" width="40%">
</div>

### Helix

<div align="center">
    <img src="./asset/reference/helix-1.png" alt="Nib dark theme in Helix" width="40%">
    <img src="./asset/reference/helix-2.png" alt="Nib light theme in Helix" width="40%">
</div>

### Kitty

<div align="center">
    <img src="./asset/reference/kitty-1.png" alt="Nib dark theme in Kitty" width="40%">
    <img src="./asset/reference/kitty-2.png" alt="Nib light theme in Kitty" width="40%">
</div>

### Lite XL

<div align="center">
    <img src="./asset/reference/lite-xl-1.png" alt="Nib dark theme in Lite XL" width="40%">
    <img src="./asset/reference/lite-xl-2.png" alt="Nib light theme in Lite XL" width="40%">
</div>

### Neovim

<div align="center">
    <img src="./asset/reference/neovim-1.png" alt="Nib dark theme in Neovim" width="40%">
    <img src="./asset/reference/neovim-2.png" alt="Nib light theme in Neovim" width="40%">
</div>

### tmux

<div align="center">
    <img src="./asset/reference/tmux-1.png" alt="Nib dark theme in tmux" width="40%">
    <img src="./asset/reference/tmux-2.png" alt="Nib light theme in tmux" width="40%">
</div>

### Vim

<div align="center">
    <img src="./asset/reference/vim-1.png" alt="Nib dark theme in Vim" width="40%">
    <img src="./asset/reference/vim-2.png" alt="Nib light theme in Vim" width="40%">
</div>

### Waybar

<div align="center">
    <img src="./asset/reference/waybar-1.png" alt="Nib dark theme in Waybar" width="40%">
    <img src="./asset/reference/waybar-2.png" alt="Nib light theme in Waybar" width="40%">
</div>

### Zathura

<div align="center">
    <img src="./asset/reference/zathura-1.png" alt="Nib dark theme in Zathura" width="40%">
    <img src="./asset/reference/zathura-2.png" alt="Nib light theme in Zathura" width="40%">
</div>

## Accessibility

> [!NOTE]
> See more at [accessibility notes](docs/ACCESSIBILITY.md), [generated contrast docs](docs/generated/CONTRAST.md), [ANSI docs](docs/generated/ANSI.md) and a [colour-vision report](docs/generated/COLOR_VISION.md).

* Principal text targets 7:1 contrast where practical.
* Meaningful text targets 4.5:1.
* Relevant boundaries target 3:1.
* Diagnostics and diffs do not rely on hue alone.

## Colors

> [!NOTE]
> The tables below show `Nib`'s core interface colours and primary syntax accents. A more detailed & complete semantic ANSI palette is available at [`dist/nib-palette.json`](dist/nib-palette.json).

### Base colors

| Name | Light | Light RGB | Dark | Dark RGB |
| --- | --- | --- | --- | --- |
| background | `#F2F1EC` | `242, 241, 236` | `#181A19` | `24, 26, 25` |
| foreground | `#182A38` | `24, 42, 56` | `#DDDCD2` | `221, 220, 210` |
| elevated surface | `#E7E8E4` | `231, 232, 228` | `#20231F` | `32, 35, 31` |
| floating surface | `#FAFAF7` | `250, 250, 247` | `#272A25` | `39, 42, 37` |
| muted text | `#59656A` | `89, 101, 106` | `#969C97` | `150, 156, 151` |
| focus | `#315D78` | `49, 93, 120` | `#82A6B6` | `130, 166, 182` |

### Dark colors

| Color | Hex | RGB |
| --- | --- | --- |
| blue | `#83A8BB` | `131, 168, 187` |
| moss | `#A3AE72` | `163, 174, 114` |
| teal | `#6FA8A0` | `111, 168, 160` |
| burgundy | `#C17E8B` | `193, 126, 139` |
| rust | `#C28168` | `194, 129, 104` |
| violet | `#AF97B7` | `175, 151, 183` |
| amber | `#C5A667` | `197, 166, 103` |

### Light colors

| Color | Hex | RGB |
| --- | --- | --- |
| blue | `#285D7C` | `40, 93, 124` |
| moss | `#536126` | `83, 97, 38` |
| teal | `#17666A` | `23, 102, 106` |
| burgundy | `#7C3448` | `124, 52, 72` |
| rust | `#8A432C` | `138, 67, 44` |
| violet | `#684873` | `104, 72, 115` |
| amber | `#77560F` | `119, 86, 15` |
