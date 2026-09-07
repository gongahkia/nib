# In-game world editor

Press `F1` to enter or leave the editor. Simulation pauses while editing; WASD pans and Shift accelerates the camera. The editor always mutates the same `TileWorld` and `WorldFeature` data used by generation and play.

Press `K` while the editor is open to use the separate live visual selector. Player sprites and environment tiles can be changed independently without regenerating or mutating the edited world.

- `Tab`: cycle inspect, paint, erase, place burrower, place brittle site, place relic, place ruin, move entity, and delete entity tools.
- `[` / `]`: select material. Left click applies the current tool; right click always erases a tile.
- `T`: type an exact signed 64-bit seed, then Enter. `V` cycles generator variant. `F2` regenerates. `F3` chooses a wall-clock random seed.
- `1` / `2` / `3`: load Broad Escarpments, Fractured Needles, or Buried Processional comparison presets. `P` or gamepad right shoulder cycles them. `H` toggles Easy/Hard and restarts the same geometry with the corresponding pressure tuning.
- `-` / `+`: adjust erosion. `,` / `.` adjusts ruin density. `;` / `'` adjusts wind strength. Press `F2` to apply parameter changes to a newly generated world.
- `Ctrl+S`: save the complete current state to `saves/editor-world.json`.
- `Ctrl+L`: load that file through the same JSON world model.
- `Ctrl+E`: export a timestamped complete snapshot under `exports/`.

Placed feature records are visible immediately when purely decorative. Entities and grouped hazards display a reload notice because their runtime controllers are reconstructed from the edited feature list on save/load. This keeps the editor model explicit instead of maintaining a second hidden entity representation.

The JSON snapshot contains generator version/parameters, every chunk and tile including damage/flags/provenance, complete material properties, route anchors, summit/start, weather, features, generated history and relations, dynamic entities, player state, resources, and recent terrain mutation records. JSON is indented and uses enum names rather than numeric codes.
