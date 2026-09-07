# Imported asset audit

Reviewed 2026-09-07. This assessment records provenance and fit. The Template Free player is the sole imported runtime pack after its creator explicitly authorized this repository; every other pack remains archive-only.

## Storage policy

Every unique received ZIP is preserved unchanged under the ignored `third_party/art/source-archives/`. Extracted images were removed when the old visual-pack pipeline was retired, except for the explicitly authorized Template Free player sheets under `assets/sprites/player/template-free/`. Duplicate downloads are identified by SHA-256 rather than stored twice.

The ignored files are part of the local working copy but are not protected by Git. `git clean -fdx` would delete them. `third_party/art/catalog.json` contains the original names, byte sizes, and SHA-256 hashes needed to verify a backup or re-download.

## Decisions

| Pack | Rights evidence | Summing fit | Decision |
| --- | --- | --- | --- |
| GandalfHardcore Character | Bundled license permits commercial/non-commercial game use and modification; prohibits raw redistribution, AI training, NFTs, game-development tools, and print | Modular 80x64 animation cells, visually tall character, and only conventional locomotion/combat states | Archived only. It is not extracted or runtime-selectable and must not be used as a generative input. It remains unsuitable as final canonical player art. |
| GandalfHardcore Platformer | Same bundled restricted license | Polished 32x32 pastoral fantasy set; forest, houses, seasonal backgrounds, water, and town decor dominate | Archived only. It is not extracted or runtime-selectable and remains unsuitable for the final badlands identity. |
| GandalfHardcore HP Bar | Same bundled restricted license | Large ornate continuous bars and helmet motif | Retain locally but reject for the slice. It conflicts with discrete hearts and the restrained interface. |
| Pixel Fantasy Caves | [Creator page](https://szadiart.itch.io/pixel-fantasy-caves) permits personal/commercial use and says credit is optional; no raw redistribution grant is stated | Detailed 16x16 cave terrain, props, and parallax backgrounds | Archived only and rejected as primary terrain because it would make the biome a cave and scales non-integrally to 24px. |
| Adve | [Creator page](https://egordorichev.itch.io/adve) explicitly declares CC0-1.0 | Compact 8x8 tiles, ENDESGA-64 palette, strong interaction readability | Archived only. Its cave-room composition should not be imported wholesale. |
| Monochrome Caves | [Creator page](https://adamatomic.itch.io/mc-caves) explicitly declares the assets public domain | Excellent silhouette economy and modular edge vocabulary, but one-bit presentation | Archived only. Direct one-bit foreground use would weaken material readability. |
| `ruins.zip` | The owner confirmed it was downloaded free from itch.io with permission to use it. It visually matches [NicoPardo's STONE RUINS](https://nicopardo.itch.io/stone-ruins), whose page permits personal/commercial use; the current official listing uses a differently named RAR | Restrained masonry fragments are a strong badlands ruin reference | Archived only. Raw redistribution permission is not established; the exact package revision remains uncertain. |
| Dracula's Manor (`lilspook.zip`) | [Creator page](https://adamatomic.itch.io/dracula) explicitly places the pack in the public domain for personal or commercial use | High-contrast 8×8 gothic architecture with a strong silhouette vocabulary; its neon pink/green palette is not a natural fit for canonical badlands | Archived only. Reject as the canonical biome palette. |
| Merakintsugi sample (`sample(idle&walk).zip`) | [Creator page](https://merakintsugi.itch.io/platformer-character-pack) permits commercial/non-commercial project use and modification, with optional attribution; raw asset redistribution, AI training, and NFTs are prohibited | Excellent animation, but the free archive contains only idle, idle-to-walk, and walk frames and its roughly 53×64 source body is much taller than Summing's one-tile player | Archived only and prohibited as a generative input. Reject as final canonical art because required traversal states are absent. |
| Ozzbit free template (`template_free.zip`) | Bundled public license says personal/non-commercial use with credit. On 2026-09-07 the repository owner identified themself as the pack's creator and explicitly authorized its use here, superseding that restriction for this project. | Clean one-tile template and a useful but incomplete state vocabulary | Imported as the sole runtime player, with original license/readme and authorization record retained. Missing traversal states use nearest supplied poses; collision remains independent. |

## Current boundary

The archives are provenance records, not generative references or drop-in art. Future assets should be original, flat-coloured, animation-led work produced through the documented Aseprite workflow. Summing's 24px terrain, 18×22 collision body, open-air badlands composition, material signaling, and original world identity remain authoritative.
