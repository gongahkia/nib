# SUMMING

Summing is a silent 2D procedural action-platformer about ascending an open-air badlands whose geology contains the remains of incompatible ancient histories. This repository targets a complete 3–5 minute first vertical slice in MonoGame/C#.

## Fedora quick start

MonoGame 3.8.5.1 requires .NET 9. Install `dotnet-sdk-9.0` through DNF, or use Microsoft's non-admin `dotnet-install.sh` workflow. Then:

```sh
dotnet restore src/Summing/Summing.csproj
make build
make run
```

The game creates local `archive/`, `saves/`, and `playtests/` directories beside the executable working directory. They are deliberately ignored by Git. No audio subsystem or assets are included in this slice.

## Project map

- `src/Summing`: game bootstrap and runtime systems
- `docs`: tone, world, architecture, controls, and authoring references
- `assets/sprites`: reproducible source and runtime placeholder art
- `tools`: local asset-generation utilities

See `docs/CONTROLS.md` for controls. Press `F1` in game for the debug/editor overlay.
