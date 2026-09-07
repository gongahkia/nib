# SUMMING

Summing is a silent 2D procedural action-platformer about ascending an open-air badlands whose geology contains the remains of incompatible ancient histories. This repository targets a complete 3–5 minute first vertical slice in MonoGame/C#.

## Fedora quick start

MonoGame 3.8.5.1 requires .NET 9. Install `dotnet-sdk-9.0` through DNF, or use Microsoft's non-admin `dotnet-install.sh` workflow. Then:

```sh
make restore
make build
./run.sh
```

The scripts detect the per-user SDK at `~/.local/share/dotnet` as well as a working SDK on `PATH`. The game creates ignored local `archive/`, `saves/`, `exports/`, and `playtests/` directories. Silence is intentional; no audio subsystem or audio assets exist. Linux debug builds are self-contained because the framework-dependent Fedora apphost can fail when the system .NET host is incomplete. Use `make publish-fedora` for a self-contained release.

The current build deliberately has one presentation mode: a sanitized movement-debug renderer. It draws uniform collision tiles, primitive objects, and the exact white 18×22 px player body against a flat field. No runtime sprite sheets, imported tilesets, backgrounds, atmosphere, material styling, animation art, visual-pack selector, or rendering toggle remain. Downloaded art ZIPs are retained only as ignored dormant archives and are never loaded by the game. Future original art direction and the Aseprite MCP workflow are recorded in `docs/art/SPRITE-GENERATION.md`.

The title screen selects Easy/Hard, deterministic seed, and Heightmap/Cellular/Layered generation. Keys `1`–`3` or gamepad right shoulder select labelled comparison presets. `Esc`/Start pauses during play. Death has no checkpoint; Easy repeats the world and Hard advances to another generated history. Reach the summit relay to complete the slice.

Run bounded non-visual verification with:

```sh
make verify
```

`./run.sh --smoke-run` performs a six-second native scripted render/telemetry check, `./run.sh --smoke-periodic` crosses the ten-second periodic screenshot boundary, and `./run.sh --smoke-movement-view` is retained as a compatibility alias for a short sanitized-renderer smoke. Diagnostic smoke runs accept `--variant=Heightmap|Cellular|Layered`, `--seed=<integer>`, and `--difficulty=Easy|Hard`.

## Project map

- `src/Summing`: game bootstrap and runtime systems
- `docs`: tone, world, architecture, controls, playtest evidence, and future art direction
- `third_party/art/source-archives`: ignored dormant ZIPs retained for provenance; never runtime-loaded
- `tools`: local build utilities

See `docs/CONTROLS.md`, `docs/EDITOR.md`, and `docs/PLAYTEST-ARTIFACTS.md`. Press `F1` in game for the debug/editor overlay.
