# Procedural history model

`WorldHistoryGenerator` is a deterministic grammar/simulation boundary keyed only by world seed. It emits a world-history identity, epoch name, three to five extinct cultures, a complete pairwise relationship graph, and a time-ordered event record. Cultures have generated names, beliefs, technological practices, territories, symbols, and fates. Relations preserve both a disposition and an inherited cause of grievance.

The model is static after generation in this slice. Nothing simulates living politics. The badlands generator assigns culture IDs to ruins, tile traces, and optional relics. Relic collection resolves those IDs and events into permanent Archive entries. The generated prose is encountered only by detour; primary presentation remains material, markings, structures, and overwritten provenance.

`archive/archive.json` persists across death and across replacement of the current world. It stores knowledge fragments only and has no link to movement, health, resources, generation, or other power progression. Invalid hand-edited Archive JSON is preserved beside the original with an `.invalid-<timestamp>` suffix before a clean document is started.

There is no model call, network access, or wall-clock input in world-history generation. Collection time is metadata and does not affect generated content.
