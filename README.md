# SUMMING

Summing is a silent 2D procedural action-platformer about ascending an open-air badlands whose geology contains the remains of incompatible ancient histories. This repository targets a complete 3–5 minute first vertical slice in MonoGame/C#.

## Fedora quick start

MonoGame 3.8.5.1 requires .NET 9. Install `dotnet-sdk-9.0` through DNF, or use Microsoft's non-admin `dotnet-install.sh` workflow. Then:

```sh
make restore
make build
./run.sh
```

The project scripts detect the documented per-user install at `~/.local/share/dotnet` as well as a working SDK on `PATH`. The game creates local `archive/`, `saves/`, `exports/`, and `playtests/` directories in the working directory. They are deliberately ignored by Git. Silence is intentional; no audio subsystem or audio assets exist in this slice.

Fedora's framework-dependent apphost cannot start when the system .NET host is incomplete. Debug builds on Linux are therefore self-contained, while the project scripts also verify the SDK host and prefer a working per-user install. To produce a clean runtime-independent release, use `make publish-fedora`, then launch `artifacts/linux-x64/Summing` directly.

The title screen selects Easy/Hard, the deterministic seed, Heightmap/Cellular/Layered generation, climber palette, visuals, and in-game bindings. `K` opens the live visual selector from the title or a run; gamepad left shoulder also opens it on the title. Player and environment art are independent. When the audited local files are present, the player list provides original, Gandalf male/female, and the limited Merakintsugi sample; Gandalf also exposes exact modular body/clothing/hair sheets. The environment library exposes Summing Original, Gandalf Overworld, Adve, Monochrome Caves, Pixel Fantasy Caves, Stone Ruins, and Dracula's Manor. Imported terrain adapters use authored edge/interior roles instead of random atlas cells; prop-only sheets are restricted to compatible terrain. Choices save to `saves/visual-selection.json` and never change terrain, collision, or generation. Summing's original art remains the portable fallback. During a run, `M` toggles a sanitized movement-test view with no backdrop or material styling, a stable camera, uniform collision tiles, and an exact white player body. Keys `1`–`3` or gamepad right shoulder select labelled comparison presets; these are development conveniences rather than authored worlds. `Esc`/Start pauses during play. Death has no checkpoint; Easy repeats the world and Hard advances to another generated history. Reach the summit relay to complete the slice.

Run all bounded non-visual verification with:

```sh
make verify
```

This checks three-variant determinism and traversability, material diversity, one-tile collision/state edge cases, complete JSON round-trip, hardness, terrain mutation, brittle collapse, and Archive persistence. `./run.sh --smoke-run` additionally runs a six-second native scripted render/telemetry check that cycles every installed player/environment pack, both Gandalf tile sheets, and the selectable Gandalf/Adve bands; `./run.sh --smoke-periodic` extends it past the ten-second periodic screenshot boundary. `./run.sh --smoke-visual-selector` captures the selector at `artifacts/visual-selector-smoke.png`; `./run.sh --smoke-movement-view` runs a two-second native check of the sanitized renderer. Diagnostic smoke runs accept `--variant=Heightmap|Cellular|Layered`, `--seed=<integer>`, `--difficulty=Easy|Hard`, and `--art-pack=SummingOriginal|GandalfOverworld` for repeatable startup presets.

## Project map

- `src/Summing`: game bootstrap and runtime systems
- `docs`: tone, world, architecture, controls, and authoring references
- `assets/sprites`: reproducible source and runtime placeholder art
- `tools`: local asset-generation utilities

See `docs/CONTROLS.md` for controls, `docs/EDITOR.md` for the integrated editor, and `docs/PLAYTEST-ARTIFACTS.md` for evidence schemas. Press `F1` in game for the debug/editor overlay.
