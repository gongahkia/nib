# Contributing

Keep changes small, semantic, and verifiable. Nib's purpose is a coordinated daily-driver theme, so legibility and conventional terminal meaning outrank novelty.

1. Read [docs/PALETTE.md](docs/PALETTE.md) and the relevant upstream research in [THIRD_PARTY_REFERENCES.md](THIRD_PARTY_REFERENCES.md).
2. Edit `palette/foundation.json` for reusable ramps, `palette/aliases.json` for mode aliases, or `palette/palette.json` only for an explicitly approved semantic-color change. Do not hand-edit generated files.
3. Add or update fixtures when changing syntax, Tree-sitter, LSP, semantic-token, diagnostic, diff, or integration behavior across any supported editor.
4. Run `make generate`, inspect the full generated diff and both modes in `showcase/index.html`, then run `make verify`.
5. Run `git diff --check` and keep unrelated changes out of the commit.

New external visual references need a direct URL, creator/publisher, access date, stated license, AI disclosure where applicable, visual lesson, exact-value/reuse status, and required attribution. Do not add photographs, fonts, logos, brand assets, or palettes with unclear redistribution rights.

Do not lower a legitimate contrast or correctness threshold to make a proposed color pass. Explain intentional exceptions, such as ANSI black serving as a building color rather than prose. Plugin integrations must remain harmless when the plugin is absent and must use currently verified highlight names.

A port is **Verified** only after manual light/dark testing in the target application and checked-in screenshot evidence. Update `support/targets.json` and `docs/VISUAL_ACCEPTANCE.md` together; format or headless-runtime success alone remains **Generated**. Third-party loaders and unstable contracts remain **Experimental**.

Original contributions are accepted under the repository's [MIT License](LICENSE).
