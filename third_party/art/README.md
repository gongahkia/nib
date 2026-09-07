# Third-party art

This directory separates third-party material from Summing's original runtime sprites.

- `cc0/` contains assets whose source pages explicitly place them in the public domain or under CC0. These files may be tracked.
- `local-only/` contains extracted assets whose licenses forbid raw redistribution, do not explicitly grant it, or whose provenance is unresolved. It is intentionally ignored by Git.
- `source-archives/` preserves the ten unique ZIP files exactly as received. It is intentionally ignored by Git.
- `catalog.json` records archive hashes, source evidence, licensing status, and the current design decision for every pack.

Do not move files from `local-only/` into tracked runtime content without checking the catalog and the current source terms. Do not feed any GandalfHardcore asset to an image-generation or training system; its bundled license explicitly prohibits AI training.

Ignored files are still stored inside the working copy, but `git clean -fdx` would delete them. Back up `source-archives/` separately before running any command that removes ignored files.

The optional visual selector discovers all runtime-compatible art in this directory. It never copies local-only PNGs into tracked runtime content or publish output. `K` opens independent player and environment choices, including exact Gandalf modular character layers and pack-specific terrain options. The environment list includes Summing Original, Gandalf Overworld, Adve, Monochrome Caves, Pixel Fantasy Caves, Stone Ruins, and the public-domain Dracula's Manor when their source files are present. The player list includes the original climber, Gandalf's modular male/female sheets, and Merakintsugi's local-only idle/walk sample. The limited Merakintsugi sample and Gandalf's incomplete traversal vocabulary make both comparison packs rather than final player art. The HP bar and Ozzbit free template remain intentionally excluded. Set `SUMMING_ART_ROOT` to the repository root when launching a build from elsewhere. This integration is for visual comparison and does not change the authoritative 24px tiles, materials, collision, generation, or one-tile player body.
