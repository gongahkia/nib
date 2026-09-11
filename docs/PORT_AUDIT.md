# Flexoki support audit

This audit compares the applications listed by Flexoki and the formats present
in its repository with Nib's first-party generated outputs. It was performed on
2026-09-11 against Flexoki's `main` branch. “Supported” means this repository
contains an applicable artifact; it does not imply publication in an app store
or upstream theme gallery.

## Summary

Nib now matches Flexoki's principal editor coverage and portable terminal and
command-line formats, including Obsidian, macOS Terminal, Konsole, and Yazi.
Nib additionally provides first-party Helium support, a paired Firefox package,
palette locking, accessibility regressions, and deterministic verification.
Flexoki remains broader in full desktop themes and proprietary writing and
creative applications.

## Editors and browsers

| Target | Flexoki | Nib | Notes |
| --- | --- | --- | --- |
| Neovim | Yes | Yes | Nib includes a configurable Lua runtime and integrations. |
| Vim | Yes | Yes | Automatic plus explicit light/dark entry points. |
| Helix | Yes | Yes | UI, syntax, diagnostics, and diffs. |
| Emacs | Community port | Yes | Generated custom themes. |
| VS Code | Yes | Yes | Also consumed by Cursor. |
| Zed | Yes | Yes | Theme-only extension. |
| Sublime Text | Yes | Yes | Editor color scheme; app chrome remains separate. |
| IntelliJ Platform | Yes | Yes | Importable `.icls` schemes. |
| Lite XL | Yes | Yes | Generated Lua color files. |
| Xcode | Yes | Not yet | `.xccolortheme` has no stable public Apple schema. |
| Chrome/Chromium | Yes | Yes | Separate Manifest V3 light/dark themes. |
| Firefox | Community port | Yes | One paired light/dark static theme. |
| Helium | No dedicated port | Yes | Uses Chromium's theme format. |
| Obsidian | Related upstream theme | Yes | Paired app theme with manifest and current semantic CSS variables. |

## Terminals and command-line tools

| Target | Flexoki | Nib | Notes |
| --- | --- | --- | --- |
| Ghostty | Built in | Yes | Paired mode and optional static shaders. |
| Alacritty | Yes | Yes | Modern TOML format. |
| Black Box | Yes | Yes | Importable JSON terminal palette. |
| iTerm2 | Yes | Yes | Importable `.itermcolors` presets. |
| Kitty | Yes | Yes | UI roles and ANSI 0–15. |
| WezTerm | Yes | Yes | Discoverable TOML scheme files. |
| Windows Terminal | Yes | Yes | Importable scheme objects. |
| Warp | Yes | Yes | Current custom-theme YAML. |
| Xresources | Yes | Yes | Generic X terminal resource palette. |
| macOS Terminal | Yes | Yes | Importable native profiles with archived NSColor values and ANSI 0–15. |
| Konsole | Yes | Yes | Native light/dark `.colorscheme` files with regular, faint, and intense ANSI roles. |
| fish | Yes | Yes | Complete current theme-variable coverage. |
| fzf | Documented | Yes | Sourceable 24-bit color fragments. |
| tmux | Yes | Yes | UI styles without key-binding or content changes. |
| Zellij | Yes | Yes | Both modes in one KDL theme file. |
| Pywal | Yes | Yes | Supported for compatibility; upstream is archived. |
| Tealdeer | Yes | Not yet | Low-impact niche port; safe future addition. |
| Yazi | Community port | Yes | Full current `theme.toml` mappings for both modes. |

## Desktop, frameworks, and other applications

| Group | Flexoki coverage | Nib status |
| --- | --- | --- |
| Web tokens | CSS, Tailwind, VitePress, Starlight | CSS custom properties and Tailwind v4 are supported. |
| Linux desktop | GTK, KDE, Qt, i3, Waybar, Dunst, imv, Zathura | Deferred: these need environment-specific component roles and visual runtime testing. |
| Messaging | Discord, Slack, Telegram, Substack | Discord supports BetterDiscord/Vencord, Slack supports Slick plus native custom colours, and Telegram Desktop has native themes. The third-party loaders remain explicitly opt-in. |
| Writing apps | Drafts, Ulysses, Typora, Standard Notes | Not yet; each needs a separately tested proprietary format or maintained CSS integration. |
| Creative palettes | Affinity, GIMP, Clip Studio Paint, Figma | Not yet; these are palette interchange formats rather than code/UI themes. |
| Site tooling | VitePress and framework-specific ports | Nib's CSS variables are usable now; packaged integrations remain future work. |

## Remaining priorities

The largest remaining gap is full Linux desktop coverage across GTK, KDE/Qt,
Waybar, and notification applications. Xcode and proprietary writing-app
formats remain narrower candidates. Desktop-environment themes should be
separate packages with screenshots and runtime testing; any additional web
patchers should retain a reproducible validation path and an unambiguous
third-party warning.
