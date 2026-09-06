# Playtest artifacts

Every generated run creates one `playtests/<local timestamp>-<accepted seed>/` directory. Regenerating, loading an editor world, or restarting a run closes the current recorder with `world-replaced` and starts another directory. Normal application exit writes `application-exit`; summit and death record their own outcomes after their event screenshots are captured.

Contents:

- `metadata.json`: schema version, seed, generator and parameters, difficulty, history identity, bindings, fixed-step rate, screenshot cadence, solvability report, and every rejected seed.
- `world-start.json`: complete human-readable starting world and runtime state, sufficient to reconstruct chunks, tiles, provenance, entities, history, tuning context, and route anchors.
- `frames.csv`: a 60 Hz trace of named input actions, aim/move axes, position/velocity, movement and visual state, camera/look-ahead, collision contacts/normals, grounded/wall state, stamina, dash/grapple state and anchor, coyote/buffer use, health/stun, resources, wind, and a 7×5 nearby material/damage window.
- `events.jsonl`: frame-linked events for damage/death/fall, dash, grapple, terrain damage/destruction, tool strikes, bombs, burrower interactions, brittle triggers/collapse, relic collection, and summit completion.
- `screenshots/index.csv` plus PNGs: periodic captures every ten seconds and event/post-event captures twelve frames apart.
- `summary.json`: outcome, duration, event count, screenshot count, and final frame.

Positions and velocities use world pixels; the tile size is recorded in `world-start.json`. Event and screenshot rows share the fixed-update frame number with `frames.csv`. This makes a feedback report such as “the jump near frame 4120 was too strict” traceable to approach velocity, input buffer use, available dash/grapple state, collision normals, nearby mutated materials, the accepted/rejected generation data, and an on-screen image.

Telemetry is diagnostic rather than replay-authoritative. Screenshot encoding occurs only at the stated cadence or meaningful events. Frequent non-destructive tile damage and burrower digging remain event records but do not each force a screenshot, limiting disk and frame-time cost.
