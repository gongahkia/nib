# `Nib` 🖋

A fountain-pen colour system for [sustained](#colors) & [accessible](#accessibility) reading, writing, and code.

## Screenshots

![](./asset/reference/fzf-1.png)
![](./asset/reference/fzf-2.png)
![](./asset/reference/vim-1.png)
![](./asset/reference/vim-2.png)
![](./asset/reference/fish-1.png)
![](./asset/reference/fish-2.png)
![](./asset/reference/tmux-1.png)
![](./asset/reference/tmux-2.png)
![](./asset/reference/helium-1.png)
![](./asset/reference/helium-2.png)
![](./asset/reference/neovim-1.png)
![](./asset/reference/neovim-2.png)
![](./asset/reference/waybar-1.png)
![](./asset/reference/waybar-2.png)
![](./asset/reference/ghostty-1.png)
![](./asset/reference/ghostty-2.png)
![](./asset/reference/zathura-1.png)
![](./asset/reference/zathura-2.png)
![](./asset/reference/chromium-1.png)
![](./asset/reference/chromium-2.png)

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

## Accessibility

Principal text targets 7:1 contrast where practical, meaningful text targets
4.5:1, and relevant boundaries target 3:1. Diagnostics and diffs do not rely on
hue alone. See the [accessibility notes](docs/ACCESSIBILITY.md) and generated
[contrast](docs/generated/CONTRAST.md), [ANSI](docs/generated/ANSI.md), and
[colour-vision](docs/generated/COLOR_VISION.md) reports.

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
