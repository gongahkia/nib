# Controls

Bindings are editable from the in-game binding panel and persisted as readable JSON. On the title screen, D-pad/left stick selects difficulty and generator, A starts, B opens bindings, and left-stick click randomizes the seed. Keys `1`–`3`, `P`, or gamepad right shoulder select/cycle the three development seed presets.

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move / crouch | WASD or arrows | left stick / D-pad |
| Jump / mantle / rope jump | Space | A |
| Dash | Left Shift | X |
| Wall cling / climb | Left Ctrl | left shoulder |
| Break terrain / punch | left mouse or F | Y |
| Slide | C | B |
| Throw / place rope | R | left-stick click |
| Climb, hold, or descend rope | W / neutral / S while touching it | left stick / D-pad up, neutral, or down |
| Bomb | Q | right-stick click |
| Aim rope throw / dash | mouse or movement direction | right stick |
| Debug/editor | F1 | Back |
| Pause | Escape | Start |
| Playtest bookmark | F8 | right shoulder |

Aim upward with W, a diagonal movement direction, the mouse, or the right stick, then press Rope to catch the exposed underside of solid terrain. The anchor must be within six tiles of the player's feet and have a clear throw path; a close exposed ledge in the facing direction remains a valid local anchor. Invalid attempts do not consume inventory, and destroying an anchor tile detaches its rope.

Hold Up or Down near a rope to grab and climb it. Releasing vertical input holds the current grip. While gripping, press Jump with Left or Right to launch in that direction. Intersecting a different rope during the 0.85-second transfer window catches it automatically; Up or Down remains the normal way to catch a rope outside a transfer. The released source rope has a brief 0.18-second regrab guard. Dash, stun, a strong blast impulse, reaching grounded terrain, or losing the anchor releases the grip.

Hold a direction while punching to strike the immediately adjacent tile left, right, above, or below; a neutral strike follows the player's facing direction. Recovery is 0.16 seconds, and solid materials use one-hit or two-hit hardness tiers. The Template Free combo animation and its white motion streak are the complete strike visual; there is no separate pickaxe. A bomb's inner blast remains damaging, while its outer edge applies a controllable damage-free launch. Active grappling does not exist; placed ropes are the sole rope traversal resource.

The only renderer is the movement-debug view: uniform collision tiles, the Template Free animated player, primitive ropes/bombs, a minimal red burrower marker, no background, no material identities, no environment sprites, no particles, and no visible camera shake. Simulation, telemetry, and terrain-impact shake state remain active.
