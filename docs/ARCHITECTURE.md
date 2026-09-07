# Runtime architecture

The authoritative world is a deterministic square tile grid partitioned into 16×16 chunks. Tiles store material, accumulated damage, flags, and provenance references. Free-position entities query and mutate that grid; no render polygon owns collision state.

Generation is a seed/version/parameter function behind one interface; the current algorithm contract is `badlands-v2`. Heightmap emphasizes broad boundary escarpments and long silhouettes, Cellular emphasizes fractured local outcrops and material irregularity, and Layered combines those systems with the densest embedded ruin provenance. All emit the same `TileWorld`. Routes include switchbacks, short descents, lateral spans, connected talus, and optional longer detours. The validator checks one-tile spawn/body clearance, landing support, route variety, conservative transitions, summit attachment, and diggability using no consumables. Failed candidates are rejected and the next derived seed is visible in debug state.

The game loop uses MonoGame's fixed timestep at 60 Hz. Input is translated into named actions before controllers consume it. `PlayerController` owns the 18×22 px standing body inside a 24×24 px tile, its 18×14 px crouched body, and explicit movement transitions. Terrain collision uses swept one-pixel axis steps, including dash termination at the first collision. Ledge catches validate a reachable clear mantle destination before committing. Active grappling is absent; placed ropes remain a separate scarce entity system. Mutations dirty only affected chunks.

Runtime persistence normally stores generation configuration and mutable run metadata. The editor can export a complete indented JSON snapshot containing tiles, chunks, materials, entities, hazards, relics, history references, and mutations. The permanent Archive is separate from run state and stores discoveries only.

Telemetry writes one self-contained directory per session. Frame samples, event records, screenshots, metadata, and a world snapshot share frame/time identifiers. Full deterministic replay is intentionally out of scope.

Rendering uses generated PNG sheets, nearest-neighbour sampling, whole-pixel camera transforms, a global logical palette with a restrained badlands extension, authored foreground dithering, and a simple screen-space atmospheric dither. No custom effect is required for the slice.
