# Nib

Nib is an inky colour scheme for reading, writing, and code. Its light mode
uses deep blue-black fountain-pen ink on cool-neutral paper; its dark mode uses
soft ivory ink on near-black chalkboard charcoal.

The palette was selected through a twelve-match blind preference audit and is
now [locked by hash](palette/lock.json). Learn more in the
[palette notes](docs/PALETTE.md) and [audit synthesis](docs/BLIND_AUDIT.md).

## Syntax highlighting

Nib keeps prose calm and gives syntax a restrained hierarchy: blue ink for
structure and callable names, moss for types, teal for strings, and quieter
burgundy, rust, violet, amber, sepia, and graphite accents. Light and dark
ports share the same semantic roles rather than being unrelated palettes.

## Ports

Nib is available for the following apps and tools. Every bundled port is
generated from [`palette/palette.json`](palette/palette.json).

### Editors

- [Neovim](docs/NEOVIM.md)
- [Vim](docs/VIM.md)
- [Helix](docs/HELIX.md)
- [GNU Emacs](docs/EMACS.md)
- [VS Code and Cursor](docs/VSCODE.md)
- [Zed](docs/ZED.md)
- [Sublime Text](docs/SUBLIME.md)
- [Obsidian](docs/OBSIDIAN.md)
- [IntelliJ Platform IDEs](intellij/)
- [Lite XL](lite-xl/)

### Terminals

- [Ghostty](docs/GHOSTTY.md)
- [macOS Terminal](docs/PORTS.md#macos-terminal)
- [Konsole](docs/PORTS.md#konsole)
- [Alacritty](alacritty/)
- [Black Box](black-box/)
- [iTerm2](iterm2/)
- [Kitty](kitty/)
- [Warp](warp-terminal/)
- [WezTerm](wezterm/)
- [Windows Terminal](windows-terminal/)
- [Xresources-compatible terminals](xresources/)

### Browsers

- [Firefox](docs/FIREFOX.md)
- [Chrome and Chromium](chromium/)
- [Helium](docs/HELIUM.md)

### Shell and command-line tools

- [fish](fish/)
- [fzf](fzf/)
- [tmux](tmux/)
- [Zellij](zellij/)
- [Yazi](docs/PORTS.md#yazi)
- [Pywal](pywal/)

### Messaging

- [Discord through BetterDiscord or Vencord](docs/MESSAGING.md#discord)
- [Slack through Slick or Slack's native custom colours](docs/MESSAGING.md#slack)
- [Telegram Desktop](docs/MESSAGING.md#telegram-desktop)

### Frameworks

- [CSS custom properties](css/nib.css)
- [Tailwind CSS v4](tailwind/nib.css)

See the [port installation guide](docs/PORTS.md) for the new portable formats.
The [Flexoki comparison audit](docs/PORT_AUDIT.md) records supported, missing,
and intentionally deferred targets.

## Colours

### Core

| Role | Light | Dark |
| --- | --- | --- |
| Background | `#F2F1EC` | `#181A19` |
| Foreground | `#182A38` | `#DDDCD2` |
| Elevated surface | `#E7E8E4` | `#20231F` |
| Floating surface | `#FAFAF7` | `#272A25` |
| Muted text | `#59656A` | `#969C97` |
| Focus | `#315D78` | `#82A6B6` |

### Accents

| Ink | Light | Dark |
| --- | --- | --- |
| Blue | `#285D7C` | `#83A8BB` |
| Moss | `#536126` | `#A3AE72` |
| Teal | `#17666A` | `#6FA8A0` |
| Burgundy | `#7C3448` | `#C17E8B` |
| Rust | `#8A432C` | `#C28168` |
| Violet | `#684873` | `#AF97B7` |
| Amber | `#77560F` | `#C5A667` |

The complete semantic and ANSI palette is available as
[`dist/nib-palette.json`](dist/nib-palette.json).

## Accessibility

Principal text targets 7:1 contrast where practical, meaningful text targets
4.5:1, and relevant boundaries target 3:1. Diagnostics and diffs do not rely
on hue alone. See the [accessibility notes](docs/ACCESSIBILITY.md) and generated
[contrast](docs/generated/CONTRAST.md), [ANSI](docs/generated/ANSI.md), and
[colour-vision](docs/generated/COLOR_VISION.md) reports.

## Contributing

Nib is MIT licensed. Ports and fixes are welcome; please read
[`CONTRIBUTING.md`](CONTRIBUTING.md). New ports should consume the canonical
palette rather than introduce parallel colour values.

## Development

```sh
make generate
make verify
```

The release suite checks the palette lock, deterministic output, contrast,
syntax and UI coverage, package structure, documentation, and available local
runtimes. Development details are in [`docs/DEVELOPMENT.md`](docs/DEVELOPMENT.md),
and source provenance is recorded in
[`THIRD_PARTY_REFERENCES.md`](THIRD_PARTY_REFERENCES.md).
