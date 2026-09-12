# Changelog

All notable changes to Nib are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and versions use [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Changed

- Renamed the public theme and Lua namespace to `nib`; the explicit Neovim and Ghostty variants are now exactly `nib-light` and `nib-dark`.
- Refined the canonical palette from a completed twelve-match blind preference audit: light surfaces are cooler and less yellow, while dark surfaces move from navy to chalkboard charcoal and use quieter syntax accents.
- Locked the approved canonical palette by hash and removed the completed browser comparison frontend and its reference-color dataset.

### Added

- A 13-step neutral and ink-family foundation palette with checked light/dark
  aliases, while preserving the existing hash-locked semantic colors.
- A canonical dependency-free showcase for prose, syntax, diagnostics, diffs,
  ANSI output, and copyable foundation swatches, with light/dark Chromium
  reference captures and an explicit native-application acceptance backlog.
- Machine-readable support tiers that separate generated artifacts, native
  visual verification, and experimental third-party integrations.
- Foundation-driven GIMP, Matplotlib, and R exports as generated—not yet
  visually verified—creative and data-visualization ports.

- Generated `nib-light` and `nib-dark` GNU Emacs custom themes with core,
  font-lock, diff, diagnostic, Org, Markdown, completion, Git, and ANSI faces.
- A single generated VS Code color-theme extension for VS Code and Cursor,
  including workbench colors, TextMate scopes, semantic tokens, and ANSI 0–15.
- A generated theme-only Zed extension using theme schema v0.2.0, with paired
  appearances, UI/editor coverage, syntax, diagnostics, version control, and
  terminal colors.
- A permission-free Firefox Manifest V3 static theme with paired automatic
  light/dark definitions and explicit no-data declaration.
- Separate Manifest V3 light and dark themes for Helium and compatible
  Chromium theme loaders.
- Generated light/dark themes for Vim, Helix, and Sublime Text, including
  native UI, syntax, diagnostic, search, selection, and diff roles.
- Generated IntelliJ and Lite XL editor schemes; Alacritty, Black Box, iTerm2,
  Kitty, WezTerm, Windows Terminal, Warp, and Xresources terminal schemes;
  fish, fzf, tmux, Zellij, and Pywal integrations; reusable CSS and Tailwind v4
  tokens; and generic Chrome/Chromium packages.
- Native macOS Terminal and Konsole schemes, complete Yazi themes, a paired
  Obsidian app theme, native Telegram Desktop themes, a variable-only
  BetterDiscord/Vencord theme, and Slick plus native Slack colour imports.
- A Flexoki support audit, consolidated port-installation guide, and a compact
  port-led README patterned after Flexoki's information architecture.
- Offline structural verification, optional Emacs batch loading, packaging and
  installation documentation, and current upstream format provenance for the
  additional editors.

## 0.1.0 - 2026-09-09

### Added

- Original warm-ivory light and near-black-navy dark palettes generated from one schema-validated source.
- Dependency-free Neovim colorscheme with automatic and explicit entry points, built-in UI coverage, Tree-sitter captures, LSP semantic groups, diagnostics, terminal colors, and optional plugin highlight integrations.
- Ghostty light/dark themes, verified paired-mode example, conventional ANSI colors, and a dry-run-first installer with guarded backup behavior.
- Static paper-grain and ink-feather shader chains in daily and showcase strengths, kept opt-in and non-animated.
- Blind A/B preference audit for Nib and twelve shuffled reference palettes, with randomized placement, a single light-or-dark comment requirement, resumable local progress, and JSON export for later synthesis.
- Deterministic contrast, ANSI, and color-vision reports; multi-language fixtures; headless/runtime verification; and GitHub Actions configuration.
- Research and licensing ledger covering technical documentation, fountain inks, and the CC0 AI-assisted Paleto references.
