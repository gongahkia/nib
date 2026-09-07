# Playtest artifacts

Every generated run creates one `playtests/<local timestamp>-<accepted seed>/` directory. Regenerating, loading an editor world, or restarting a run closes the current recorder with `world-replaced` and starts another directory. Normal application exit writes `application-exit`; summit and death record their own outcomes after their event screenshots are captured.

Contents:

- `metadata.json`: schema version, seed, generator and parameters, difficulty, the complete starting player/environment visual selection, fixed world-camera zoom, history identity, bindings, fixed-step rate, screenshot cadence, manual bookmark binding, solvability report, and every rejected seed.
- `world-start.json`: complete human-readable starting world and runtime state, sufficient to reconstruct chunks, tiles, provenance, entities, history, tuning context, and route anchors.
- `frames.csv`: a 60 Hz trace of named input actions, aim/move axes, position/velocity, current collision-body dimensions, movement and visual state, camera/look-ahead/shake, collision contacts/normals, grounded/wall state, stamina, dash state, coyote/buffer use, health/stun, resources, wind, a 7×5 nearby material/damage window, nearest validated route-anchor index, and altitude band.
- `events.jsonl`: frame-linked events for damage/death/fall, successful dash starts, terrain damage/destruction, directional tool strikes, successful or rejected rope placement, bombs, burrower interactions, brittle triggers/collapse, relic collection, visual-pack changes, manual playtest bookmarks, and summit completion.
- `screenshots/index.csv` plus PNGs: periodic captures every ten seconds and event/post-event captures twelve frames apart.
- `summary.json`: outcome, duration, event count, screenshot count, and final frame.

Positions and velocities use world pixels; the tile size is recorded in `world-start.json`. Event and screenshot rows share the fixed-update frame number with `frames.csv`. This makes a feedback report such as “the jump near frame 4120 was too strict” traceable to approach velocity, input buffer use, available dash state, collision normals, nearby mutated materials, camera shake, the accepted/rejected generation data, and an on-screen image.

Telemetry schema 5 is diagnostic rather than replay-authoritative. Press `F8` or gamepad right shoulder at a location worth discussing to capture an immediate and twelve-frame-later screenshot around a `playtest-bookmark` event. Every applied visual-selector change records the changed row and the complete resulting player/environment selection with the same immediate/post-event evidence, making exact art comparisons attributable. Screenshot encoding otherwise occurs only at the stated cadence or meaningful events. Frequent non-destructive tile damage and burrower digging remain event records but do not each force a screenshot, limiting disk and frame-time cost.
