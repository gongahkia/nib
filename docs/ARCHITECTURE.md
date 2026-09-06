# Architecture decisions

Palimpsest Run is a dependency-light Lua 5.1/LuaJIT project targeting LÖVE 11.5. Simulation modules do not depend on `love.*`; the graphical entry point adapts them to rendering, input, audio, and persistence.

- Fixed simulation step: 1/60 second, integer ticks, project-owned 32-bit PRNG.
- World units: internal pixels. Rendering is a strict 128×128 canvas scaled by an integer.
- Colour: one shared 16-entry RGB palette; alpha changes coverage only.
- Data: canonical versioned JSON under `data/`; generated levels and replays share explicit schemas.
- Movement choice: wall traversal includes wall slide and wall jump; slide is chosen over roll; dive is chosen over ground-pound; grapple is chosen over tether.
- Terrain: tile-backed collision with explicit material properties. Generation composes motifs, grammar, continuous profiles, and deterministic placement, then validates and repairs.
- Determinism: gameplay uses no global random source, wall clock, or frame delta. Replays store per-tick inputs and state hashes.
- Tooling: `bin/palimpsest` invokes headless Lua modules through LuaJIT; LÖVE is graphical only when requested.

Module boundaries are `fixed_step`/`session` (simulation), `render` and rig renderers (graphics), `input`, `animation`, `world`/`materials`/`tool`, `generator`, `reachability`, `hand`, `scoring`, `persistence`/`storage`, `replay`, `hot_reload`, and `schema`/`serialization`/`overlay`/`obstacle` (agent tooling). Tables are owned by a session; global mutable randomness is absent.

Schema version 1 is current. Future readers reject unknown versions with a JSON path. Migrations are pure transforms registered by source and destination version; raw files are never silently rewritten.
