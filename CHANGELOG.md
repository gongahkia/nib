# Changelog

All notable changes to Nib are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and versions use [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Changed

- Renamed the public theme and Lua namespace to `nib`; the explicit Neovim and Ghostty variants are now exactly `nib-light` and `nib-dark`.
- Refined the canonical palette from a completed twelve-match blind preference audit: light surfaces are cooler and less yellow, while dark surfaces move from navy to chalkboard charcoal and use quieter syntax accents.

### Added

- Generated `nib-light` and `nib-dark` GNU Emacs custom themes with core,
  font-lock, diff, diagnostic, Org, Markdown, completion, Git, and ANSI faces.
- A single generated VS Code color-theme extension for VS Code and Cursor,
  including workbench colors, TextMate scopes, semantic tokens, and ANSI 0–15.
- A generated theme-only Zed extension using theme schema v0.2.0, with paired
  appearances, UI/editor coverage, syntax, diagnostics, version control, and
  terminal colors.
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
