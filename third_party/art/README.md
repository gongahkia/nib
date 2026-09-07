# Dormant third-party art archives

No third-party sprite or tileset is extracted, tracked as runtime content, discovered by the executable, or copied into build output. `source-archives/` retains the ten original ZIP files exactly as received for provenance and possible later human review; it is ignored by Git. `catalog.json` records hashes, source evidence, licensing limits, and the historical audit.

Do not extract an archive into runtime content without a fresh design and license review. Do not feed GandalfHardcore or Merakintsugi assets to image-generation or training systems; their source terms prohibit it. The current plan is to create original flat-colour animation-led art through the workflow in `docs/art/SPRITE-GENERATION.md`, not derive it from these packs.

Ignored archives can be deleted by `git clean -fdx`; back them up separately before running that command.
