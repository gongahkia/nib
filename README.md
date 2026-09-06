# Palimpsest Run

Palimpsest Run is a provisional codename for a finite high-speed LÖVE platformer about crossing a city built over older cities while an architecture-sized hand runs after you on its fingers. This repository contains a complete rough vertical slice: procedural art and audio are generated locally, and no extracted assets are used.

## Fedora setup and run

The project targets Fedora 43, LÖVE 11.5, and LuaJIT 2.1. Install missing runtime tools with:

```sh
sudo dnf install love luajit make zip
```

Run the game or package it:

```sh
make run
make package
love build/palimpsest-run.love
```

The game renders into a palette-quantized 128×128 framebuffer, then integer-scales and centres it in the window. Start with `N` for a fresh seed, type digits and press `S` or Enter for the displayed seed, and press `R` on a result screen to retry the identical seed.

## Verification

```sh
make test
make soak
make smoke
```

`make test` runs the headless behavioural suite. `make soak` generates and validates 200 seeds while sampling hand IK. `make smoke` launches the graphical game for three seconds and treats the expected timeout as success; it requires an active graphical session.

## Data editing and persistence

Versioned JSON schemas live in `data/schemas/`; generator and default binding data live in `data/`. Starting a run writes the exact generated level to:

```text
~/.local/share/love/palimpsest-run/hot-level.json
```

Edit that readable JSON while a run is active and press `F5`. A valid file is loaded atomically; invalid data leaves the running level intact and reports a JSON path in the debug overlay. Schema version 1 is strict at the required structural boundary and preserves additional provenance/metrics fields. Future versions use explicit pure migrations; unknown versions fail instead of being rewritten.

Profiles, remapped bindings, lore, run summaries, and all replay recordings use the same save directory. Runs are never automatically pruned. Only the explicit CLI deletion command removes one.

See [docs/TONE.md](docs/TONE.md), [docs/LORE.md](docs/LORE.md), [docs/CONTROLS.md](docs/CONTROLS.md), and [docs/CLI.md](docs/CLI.md).

## Architecture

Headless modules under `game/` separate fixed-step simulation, input, player state transitions, material physics, tool interactions, generation, resource-state reachability, hand IK, scoring, persistence, replay capture, schemas, hot reload, overlays, and A/B obstacle analysis. `main.lua` is the LÖVE adapter; `cli.lua` is the headless adapter. Gameplay constants are centralized in `game/config.lua`.

Generation composes route topology, grammar-selected strata and rhythms, authored structural motifs, continuous height variation, materials, props, alternate branches, and deterministic placements. The validator derives its limits from the implemented movement configuration, tracks dash/grapple resources and refresh transitions, repairs a frontier locally when needed, and records evidence plus quantitative metrics.

## Known limits of this rough slice

- Physical gamepad hardware was unavailable during automated verification; the standard gamepad mapping was exercised through a hardware-independent fake joystick.
- The A/B evaluator performs bounded local fixed-step ballistic perturbations and then validates the complete route/collectible graph. It does not claim that its tolerance metric measures fun.
- The reachability solver models implemented movement transitions and airborne resources, but it is not a full search over every analogue input trajectory.
- Audio is synthesized effects only; there is intentionally no composed soundtrack.
- Art, environmental incidents, and fragment text are deliberately sparse prototype content, while the underlying systems are implemented.
