# SUMMING

Summing is a silent 2D procedural action-platformer about ascending an open-air badlands whose geology contains the remains of incompatible ancient histories. This repository targets a complete 3–5 minute first vertical slice in MonoGame/C#.

## Fedora quick start

MonoGame 3.8.5.1 requires .NET 9. Install `dotnet-sdk-9.0` through DNF, or use Microsoft's non-admin `dotnet-install.sh` workflow. Then:

```sh
dotnet restore src/Summing/Summing.csproj
make build
./run.sh
```

`run.sh` also detects the documented per-user install at `~/.local/share/dotnet` when `dotnet` is not on `PATH`. The game creates local `archive/`, `saves/`, `exports/`, and `playtests/` directories in the working directory. They are deliberately ignored by Git. Silence is intentional; no audio subsystem or audio assets exist in this slice.

The title screen selects Easy/Hard, the deterministic seed, Heightmap/Cellular/Layered generation, climber palette, and in-game bindings. `Esc`/Start pauses during play. Death has no checkpoint; Easy repeats the world and Hard advances to another generated history. Reach the summit relay to complete the slice.

Run all bounded non-visual verification with:

```sh
make verify
```

This checks three-variant determinism and traversability, complete JSON round-trip, scripted movement state transitions, hardness, terrain mutation, brittle collapse, and Archive persistence. `dotnet run --project src/Summing/Summing.csproj -- --smoke-run` additionally runs a four-second native scripted render/telemetry check.

## Project map

- `src/Summing`: game bootstrap and runtime systems
- `docs`: tone, world, architecture, controls, and authoring references
- `assets/sprites`: reproducible source and runtime placeholder art
- `tools`: local asset-generation utilities

See `docs/CONTROLS.md` for controls, `docs/EDITOR.md` for the integrated editor, and `docs/PLAYTEST-ARTIFACTS.md` for evidence schemas. Press `F1` in game for the debug/editor overlay.
