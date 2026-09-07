# Controls

Bindings are editable from the in-game binding panel and persisted as readable JSON.

On the title screen, D-pad/left stick selects difficulty and generator, A starts, B opens bindings, Y changes the climber palette, and left-stick click randomizes the seed. `K` or gamepad left shoulder cycles the available visual art packs. Keys `1`–`3`, `P`, or gamepad right shoulder select/cycle the three labelled development seed presets. In the bindings panel, Up/Down selects an action, Left/Right selects keyboard or gamepad, A begins rebinding, and Start returns.

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / crouch / aim | WASD or arrows | left stick / D-pad; right stick aims |
| Jump / mantle | Space | A |
| Dash | Left Shift | X |
| Wall cling / climb | Left Ctrl | left shoulder |
| Dig | left mouse or F | Y |
| Slide | C | B |
| Place rope | R | left-stick click |
| Bomb | Q | right-stick click |
| Aim dash direction | mouse / movement direction | right stick |
| Debug/editor | F1 | Back |
| Pause | Escape | Start |
| Playtest bookmark | F8 | right shoulder |
| Hot-swap visual art pack | K | left shoulder on title screen |

The art-pack switch is a presentation-only debug control: it changes the terrain and player sheets but not terrain tiles, materials, collision, generation, movement, or serialized worlds. The bookmark records a frame-linked event and immediate/post-event screenshots; it does not alter play. Editor controls appear in the overlay because their meaning depends on the selected tool. Active grappling does not exist; placed ropes are the sole rope traversal resource.
