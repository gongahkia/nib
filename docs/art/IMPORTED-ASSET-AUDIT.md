# Imported asset audit

Reviewed 2026-09-07. This is an asset and provenance assessment, not a runtime integration. None of these downloads currently replaces Summing's original placeholder art.

## Storage policy

Every received ZIP is preserved unchanged under `third_party/art/source-archives/`. Every pack is also extracted. Public-domain and CC0 assets are tracked under `third_party/art/cc0/`; restricted or unresolved assets live under the ignored `third_party/art/local-only/` directory so a future public source repository does not redistribute raw files contrary to their terms.

The ignored files are part of the local working copy but are not protected by Git. `git clean -fdx` would delete them. `third_party/art/catalog.json` contains the original names, byte sizes, and SHA-256 hashes needed to verify a backup or re-download.

## Decisions

| Pack | Rights evidence | Summing fit | Decision |
| --- | --- | --- | --- |
| GandalfHardcore Character | Bundled license permits commercial/non-commercial game use and modification; prohibits raw redistribution, AI training, NFTs, game-development tools, and print | Modular 80x64 animation cells, visually tall character, and only conventional locomotion/combat states | Retain locally. Explicitly reject as Summing's player; it does not fit the 24x24 visual footprint or traversal state vocabulary. Manual animation reference only. |
| GandalfHardcore Platformer | Same bundled restricted license | Polished 32x32 pastoral fantasy set; forest, houses, seasonal backgrounds, water, and town decor dominate | Retain locally. Reject as the biome's primary terrain/background. Rock piles, grave markers, reeds, the angel statue, furnace, and small machinery silhouettes are selective manual adaptation candidates. |
| GandalfHardcore HP Bar | Same bundled restricted license | Large ornate continuous bars and helmet motif | Retain locally but reject for the slice. It conflicts with discrete hearts and the restrained interface. |
| Pixel Fantasy Caves | [Creator page](https://szadiart.itch.io/pixel-fantasy-caves) permits personal/commercial use and says credit is optional; no raw redistribution grant is stated | Detailed 16x16 cave terrain, props, and parallax backgrounds | Retain locally. Reject as primary terrain because it would make the biome a cave and scales non-integrally to 24px. Rock silhouettes and parallax construction are useful references. |
| Adve | [Creator page](https://egordorichev.itch.io/adve) explicitly declares CC0-1.0 | Compact 8x8 tiles, ENDESGA-64 palette, strong interaction readability | Approved and tracked. It scales exactly 3x to 24px. Use individual ladders, chains, edge motifs, and interaction cues selectively; do not import its cave-room composition wholesale. |
| Monochrome Caves | [Creator page](https://adamatomic.itch.io/mc-caves) explicitly declares the assets public domain | Excellent silhouette economy and modular edge vocabulary, but one-bit presentation | Approved and tracked. Best as structural reference or recolored background/ruin fragments; direct foreground use would erase material readability. |
| `ruins.zip` | The owner confirmed it was downloaded free from itch.io with permission to use it. It visually matches [NicoPardo's STONE RUINS](https://nicopardo.itch.io/stone-ruins), whose page permits personal/commercial use; the current official listing uses a differently named RAR | Restrained masonry fragments are a strong badlands ruin reference | Approved for selective in-game use. Keep raw files local-only because standalone redistribution permission is not established. The exact package revision remains uncertain but does not block the owner-confirmed game use. |

## Recommended order

1. Use Adve and Monochrome Caves as the legally clean reference library. Their greatest value is compact shape language, not a drop-in biome replacement.
2. Use the Stone Ruins fragments as the leading imported source for exposed masonry, broken arches, and partly buried architecture after adapting them to Summing's palette and 24px composition.
3. Consider isolated Pixel Fantasy rock silhouettes and isolated GandalfHardcore ruin/machinery props only after a deliberate palette, scale, and silhouette pass.
4. Do not use the GandalfHardcore character or HP bar for the current player/UI.

No pack should be copied wholesale. Summing's 24px terrain, limited palette, one-tile player, open-air badlands composition, material signaling, and original world identity remain authoritative.
