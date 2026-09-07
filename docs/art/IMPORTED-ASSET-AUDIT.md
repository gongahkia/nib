# Imported asset audit

Reviewed 2026-09-07. This assessment records both provenance and the bounded optional runtime integration. Summing's original placeholder art remains the portable default/fallback; the local GandalfHardcore character/platformer pair can now be selected as a presentation-only playtest pack.

## Storage policy

Every received ZIP is preserved unchanged under `third_party/art/source-archives/`. Every pack is also extracted. Public-domain and CC0 assets are tracked under `third_party/art/cc0/`; restricted or unresolved assets live under the ignored `third_party/art/local-only/` directory so a future public source repository does not redistribute raw files contrary to their terms.

The ignored files are part of the local working copy but are not protected by Git. `git clean -fdx` would delete them. `third_party/art/catalog.json` contains the original names, byte sizes, and SHA-256 hashes needed to verify a backup or re-download.

## Decisions

| Pack | Rights evidence | Summing fit | Decision |
| --- | --- | --- | --- |
| GandalfHardcore Character | Bundled license permits commercial/non-commercial game use and modification; prohibits raw redistribution, AI training, NFTs, game-development tools, and print | Modular 80x64 animation cells, visually tall character, and only conventional locomotion/combat states | Integrated in the optional local visual selector. Male/female pack and exact body, legs, torso, feet, and hair sheets are independently selectable. Half-scale rendering keeps the visible figure near one tile tall; missing traversal states use documented nearest-pose mappings. It remains unsuitable as final canonical player art. |
| GandalfHardcore Platformer | Same bundled restricted license | Polished 32x32 pastoral fantasy set; forest, houses, seasonal backgrounds, water, and town decor dominate | Both floor sheets and their green/autumn/snow bands are integrated in the optional local visual selector, independently of the player. Edges are reduced to 24px and layered over authoritative material colours. It remains unsuitable as the final badlands identity. |
| GandalfHardcore HP Bar | Same bundled restricted license | Large ornate continuous bars and helmet motif | Retain locally but reject for the slice. It conflicts with discrete hearts and the restrained interface. |
| Pixel Fantasy Caves | [Creator page](https://szadiart.itch.io/pixel-fantasy-caves) permits personal/commercial use and says credit is optional; no raw redistribution grant is stated | Detailed 16x16 cave terrain, props, and parallax backgrounds | Available in the local visual selector through a 16px-to-24px comparison adapter. Reject as primary terrain because it would make the biome a cave and scales non-integrally to 24px. Rock silhouettes and parallax construction remain useful references. |
| Adve | [Creator page](https://egordorichev.itch.io/adve) explicitly declares CC0-1.0 | Compact 8x8 tiles, ENDESGA-64 palette, strong interaction readability | Available in the visual selector with oxide, verdigris, and salt atlas regions. It scales exactly 3x to 24px. Do not import its cave-room composition wholesale. |
| Monochrome Caves | [Creator page](https://adamatomic.itch.io/mc-caves) explicitly declares the assets public domain | Excellent silhouette economy and modular edge vocabulary, but one-bit presentation | Available in the visual selector through a material-tinted, translucent 8px-to-24px comparison adapter. It remains a reference rather than a primary foreground because direct one-bit use weakens material readability. |
| `ruins.zip` | The owner confirmed it was downloaded free from itch.io with permission to use it. It visually matches [NicoPardo's STONE RUINS](https://nicopardo.itch.io/stone-ruins), whose page permits personal/commercial use; the current official listing uses a differently named RAR | Restrained masonry fragments are a strong badlands ruin reference | Available in the local visual selector through a material-tinted 8px-to-24px comparison adapter. Keep raw files local-only because standalone redistribution permission is not established. The exact package revision remains uncertain but does not block the owner-confirmed game use. |

## Recommended order

1. Use Adve and Monochrome Caves as the legally clean reference library. Their greatest value is compact shape language, not a drop-in biome replacement.
2. Use the Stone Ruins fragments as the leading imported source for exposed masonry, broken arches, and partly buried architecture after adapting them to Summing's palette and 24px composition.
3. Use the GandalfHardcore paired pack only for A/B playtesting. Do not treat its conventional fantasy character, grass edge, or incomplete traversal vocabulary as a final art decision.
4. Consider isolated Pixel Fantasy rock silhouettes and isolated GandalfHardcore ruin/machinery props only after a deliberate palette, scale, and silhouette pass. Do not use the GandalfHardcore HP bar.

No pack should be copied wholesale. Summing's 24px terrain, limited palette, 18×22 collision body, open-air badlands composition, material signaling, and original world identity remain authoritative. The visual-pack chooser changes no gameplay or world data.
