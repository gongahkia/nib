# Third-party art

This directory separates third-party material from Summing's original runtime sprites.

- `cc0/` contains assets whose source pages explicitly place them in the public domain or under CC0. These files may be tracked.
- `local-only/` contains extracted assets whose licenses forbid raw redistribution, do not explicitly grant it, or whose provenance is unresolved. It is intentionally ignored by Git.
- `source-archives/` preserves the seven ZIP files exactly as received. It is intentionally ignored by Git.
- `catalog.json` records archive hashes, source evidence, licensing status, and the current design decision for every pack.

Do not move files from `local-only/` into tracked runtime content without checking the catalog and the current source terms. Do not feed any GandalfHardcore asset to an image-generation or training system; its bundled license explicitly prohibits AI training.

Ignored files are still stored inside the working copy, but `git clean -fdx` would delete them. Back up `source-archives/` separately before running any command that removes ignored files.

The optional `GandalfOverworld` playtest pack loads the GandalfHardcore character and platformer PNGs directly from `local-only/` at runtime. It never copies them into tracked runtime content or publish output. `F4` hot-swaps between it and the complete original Summing fallback; set `SUMMING_ART_ROOT` to the repository root when launching a build from elsewhere. This integration is for visual comparison and does not change the authoritative 24px tiles, materials, collision, generation, or one-tile player body.
