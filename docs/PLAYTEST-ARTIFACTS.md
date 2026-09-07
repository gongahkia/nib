# Playtest artifacts

Every generated run creates one `playtests/<local timestamp>-<accepted seed>/` directory. Regenerating, loading an editor world, or restarting closes the current recorder and starts another directory.

- `metadata.json`: schema version 8, seed, generator and parameters, difficulty, fixed camera zoom, sanitized render mode, history identity, bindings, screenshot cadence, solvability report, and rejected seeds.
- `world-start.json`: complete human-readable starting world and runtime state.
- `frames.csv`: 60 Hz input, aim/move axes, player body/position/velocity/state, camera/look-ahead/shake, collision contacts, stamina, dash, coyote/buffer use, health, resources, explicit rope-grip state and anchor, wind, nearby 7×5 material/damage window, route anchor, and altitude band.
- `events.jsonl`: frame-linked damage/death/fall, dash, terrain interaction, rope placement/grab/jump/detachment, bomb/rocket-jump, burrower, brittle, relic, bookmark, and summit events.
- `screenshots/index.csv` plus PNGs: periodic captures every ten seconds and meaningful immediate/post-event captures twelve frames apart.
- `summary.json`: outcome, duration, event count, screenshot count, and final frame.

Positions and velocities use world pixels. Event, screenshot, and frame rows share fixed-update frame numbers. A rope-jump screenshot can therefore be correlated with its source anchor, input direction, launch velocity, later target-rope grip, collision context, and nearby terrain. Press `F8` or gamepad right shoulder to bookmark a location without changing play.

Telemetry is diagnostic rather than replay-authoritative. The game always captures the sanitized view, but camera shake strength and offset remain recorded even though shake is suppressed visually. Frequent non-destructive tile damage and burrower digging are event records without per-event screenshots to control disk and frame-time cost.
