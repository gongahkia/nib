# Runtime architecture

The authoritative world is a deterministic square tile grid partitioned into 16×16 chunks. Tiles store material, accumulated damage, flags, and provenance references. Free-position entities query and mutate that grid; no render polygon owns collision state.

Generation is a seed/version/parameter function behind one interface. Heightmap, cellular, and layered variants emit the same `TileWorld`. A validator checks the explicit macro route against conservative player-scale transition, clearance, summit attachment, and diggability limits using no consumables. Failed candidates are rejected and the next derived seed is visible in debug state.

The game loop uses MonoGame's fixed timestep at 60 Hz. Input is translated into named actions before controllers consume it. `PlayerController` owns stable body physics while focused ability/state components own dash, grapple, climbing, tools, health, and animation presentation. Terrain collision uses axis-separated tile queries. Mutations dirty only affected chunks.

Runtime persistence normally stores generation configuration and mutable run metadata. The editor can export a complete indented JSON snapshot containing tiles, chunks, materials, entities, hazards, relics, history references, and mutations. The permanent Archive is separate from run state and stores discoveries only.

Telemetry writes one self-contained directory per session. Frame samples, event records, screenshots, metadata, and a world snapshot share frame/time identifiers. Full deterministic replay is intentionally out of scope.

Rendering uses generated PNG sheets, nearest-neighbour sampling, whole-pixel camera transforms, a global logical palette with a restrained badlands extension, authored foreground dithering, and a simple screen-space atmospheric dither. No custom effect is required for the slice.
