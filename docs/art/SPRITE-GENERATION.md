# Future sprite direction and Aseprite workflow

The current build contains one runtime sprite family: Ozzbit Games' Template Free player, explicitly authorized by its creator for this repository. Terrain remains sanitized uniform collision geometry with primitive object marks. The player sheets use 128×128 px cells, a fixed cropped presentation window, and a stable foot pivot over the authoritative 18×22 px collision body. States absent from the free pack deliberately use the nearest supplied pose.

All other downloaded packs are inactive and their original ZIP files remain ignored provenance archives under `third_party/art/source-archives/`. Do not extract them into runtime content or use restricted packs as generative inputs.

## Locked future direction

The Template Free player is a practical movement-test skin, not a reversal of the longer-term direction. Future Summing-specific sprites should be original and extremely flat-coloured. Use Nidhogg only as high-level vocabulary for bold single-colour silhouettes and immediate figure/background separation—never copy its characters, poses, palette, stages, or assets. Identity must come primarily from silhouette changes, timing, spacing, anticipation, follow-through, and detailed state-specific animation frames rather than internal texture or shading.

- Give the player one stable, high-contrast base colour that stays distinct from every biome background and collision surface.
- Keep each character frame mostly one opaque colour. A second colour is reserved for an essential readability mark, not routine shading.
- Make motion readable at 1× through pose and negative space. Do not rely on glow, outline noise, gradients, or palette cycling to explain a state.
- Preserve the one-tile 18×22 px collision footprint. Art may extend a few pixels outside it, but feet/pivot and contact points must remain stable.
- Author explicit frames for idle, run, takeoff, ascent, apex, fall, landing, crouch/crawl, slide, ledge hang, mantle, wall cling, wall jump, dash, rope grip/climb, rope jump, digging in four directions, bomb use, hurt, stun, and death.
- Keep terrain broad and flat enough that collision edges remain dominant. Ruins and geology may gain sparse silhouette-breaking detail only after movement readability is proven.

## Aseprite MCP later

The intended development-only authoring path is [`diivi/aseprite-mcp`](https://github.com/diivi/aseprite-mcp). Its documented toolset supports layers, frames, tags, palette inspection, onion-skin rendering, frame comparison, and sprite-sheet export. Aseprite's official CLI can export sheets and JSON metadata for deterministic integration. Neither MCP nor Aseprite belongs in the shipped game or runtime dependency graph.

When art work resumes:

1. Install a licensed Aseprite build and a temporary, pinned checkout of `diivi/aseprite-mcp` outside the repository.
2. Create the player in an editable `.aseprite` source with one layer group per semantic part and one animation tag per movement state.
3. Establish a deliberately tiny palette before drawing. Validate the player base colour against every intended background value.
4. Block the full silhouette sequence first, inspect 8× previews and onion skins, and use frame differences to catch accidental jitter.
5. Export transparent PNG sheets plus JSON metadata through Aseprite; keep editable sources and a machine-readable manifest when runtime art is deliberately reintroduced.
6. Integrate only after 1× movement-state readability and stable pivots have been manually approved.

## Future generation brief

> Create an original pixel-art animation sheet for Summing's anonymous one-tile climber. Use Nidhogg only as high-level vocabulary for extremely flat single-colour silhouettes, strong contrast, and animation-led readability; do not reproduce any existing character, pose sequence, palette, stage, or asset. The figure must read inside an 18×22 px collision footprint on a 24×24 px frame grid. Use one dominant opaque body colour and at most one sparse functional accent. Communicate identity and mechanics through precise poses, timing, anticipation, follow-through, and changing negative space. Include explicitly tagged sequences for idle, run, takeoff, ascent, apex, fall, landing, crouch, crawl, slide, ledge hang, mantle, wall cling, wall jump, directional dash, rope grip/climb, rope jump, four-direction digging, bomb use, hurt, stun, and death. Keep the foot pivot stable and make every state distinguishable at native 1× scale. Transparent runtime background; if producing a source contact sheet for extraction, use a plain pure-white background. No text, logos, watermark, gradients, soft filtering, copied symbols, or extra mechanics.

## References

- `diivi/aseprite-mcp`: <https://github.com/diivi/aseprite-mcp>
- Aseprite CLI export documentation: <https://www.aseprite.org/docs/cli/>
- Messhof's official Nidhogg page: <https://www.messhof.com/nidhogg>
