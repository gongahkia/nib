# Template Free player source

- Creator and rights holder: Ozzbit Games, confirmed by the repository owner on 2026-09-07.
- Source archive: `/home/gongahkia/Downloads/template_free.zip`.
- Imported archive SHA-256: `875b4a615ff1b3f0d72f8464c575eaefdafb7e0a2b75339d0f5a448aea3ce697`.
- Cell layout: horizontal 128×128 px frames.
- Runtime use: `player-sprite.json` declares the 78×48 px crop at `(42,36)`, 0.5× scale, semantic state mapping, authored direction, collision contract, and stable foot anchor.

The bundled public free-version license says personal, non-commercial use with required `Ozzbit Games` credit. For this repository, the user explicitly identified themself as the pack's creator and authorized its use despite that bundled restriction. Preserve the original `LICENSE.txt` and `READ ME.txt` alongside the imported sheets as provenance.

This is the sole player sprite family. Missing Summing-specific traversal poses use documented nearest-state mappings in `PlayerSpriteRenderer`; collision remains the authoritative 18×22 px one-tile body. Horizontal flips use the mathematically mirrored pack anchor rather than a sprite-specific offset, so replacing the active profile does not require movement or collision changes.
