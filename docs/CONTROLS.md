# Controls

Bindings are editable from the in-game binding panel and persisted as readable JSON.

On the title screen, D-pad/left stick selects difficulty and generator, A starts, B opens bindings, Y changes the climber palette, and left-stick click randomizes the seed. `K` or gamepad left shoulder opens the visual selector. Keys `1`–`3`, `P`, or gamepad right shoulder select/cycle the three labelled development seed presets. In the bindings panel, Up/Down selects an action, Left/Right selects keyboard or gamepad, A begins rebinding, and Start returns.

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / crouch / aim | WASD or arrows | left stick / D-pad; right stick aims |
| Jump / mantle | Space | A |
| Dash | Left Shift | X |
| Wall cling / climb | Left Ctrl | left shoulder |
| Dig | left mouse or F | Y |
| Slide | C | B |
| Place rope | R | left-stick click |
| Climb or descend a rope | W / S while touching it | left stick / D-pad up or down |
| Bomb | Q | right-stick click |
| Aim dash direction | mouse / movement direction | right stick |
| Debug/editor | F1 | Back |
| Pause | Escape | Start |
| Playtest bookmark | F8 | right shoulder |
| Open visual selector | K | left shoulder on title screen |
| Toggle movement-test view | M | — |

Ropes can be secured only while grounded at an exposed ledge in the direction the player faces; an invalid attempt does not consume inventory. Hold Up or Down near a rope to attach and climb without the wall-grab button. Hold a direction while digging to strike the immediately adjacent tile left, right, above, or below; a neutral swing follows the player's facing direction. Tool recovery is 0.16 seconds, and all sixteen solid materials occupy distinct one-hit or two-hit hardness tiers. A bomb's inner blast remains damaging, but catching its outer edge applies a controllable, damage-free launch suitable for rocket jumps. `M` toggles a rendering-only movement-test view: collision and simulation remain authoritative, while backgrounds, atmosphere, material identities, imported tiles, ruin dressing, particles, and camera shake are suppressed. It draws uniform collision tiles, an exact white player collision body, primitive ropes/bombs, and a minimal red burrower marker. In the visual selector, Up/Down chooses a row and Left/Right changes it live; `K`, Enter, Escape, B, or Start closes it. Player pack, exact modular player sheets, world pack, tileset, and tileset band are independent presentation-only choices. Six environment packs are currently discoverable when every local upload is present. The uploaded archives contain one animated character family, exposed as original plus Gandalf male/female configurations; the remaining uploads are environment or UI art. The bookmark records a frame-linked event and immediate/post-event screenshots; it does not alter play. Editor controls appear in the overlay because their meaning depends on the selected tool. Active grappling does not exist; placed ropes are the sole rope traversal resource.
