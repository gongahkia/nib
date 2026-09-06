# Agent-facing CLI

All successful commands emit JSON with `outputSchemaVersion: 1`. Errors are JSON on stderr with a nonzero exit code. No command opens a graphical window.

```sh
# generate a deterministic serialized level
./bin/palimpsest generate 424242 /tmp/level.json

# validate schema, exit, and every collectible
./bin/palimpsest validate /tmp/level.json

# inspect provenance, decisions, warnings, and metrics
./bin/palimpsest inspect /tmp/level.json

# inspect movement-resource reachability evidence
./bin/palimpsest reachability /tmp/level.json

# bounded multi-seed audit (count, optional first seed)
./bin/palimpsest audit 200 1

# list all retained recordings
./bin/palimpsest runs

# summarize by run ID or explicit replay path
./bin/palimpsest summarize run-424242-1700000000-1
./bin/palimpsest summarize /tmp/replay.json

# deterministically replay inputs and verify state hashes
./bin/palimpsest replay run-424242-1700000000-1

# create SVG plus structured overlay JSON
./bin/palimpsest overlay run-424242-1700000000-1 /tmp/run.svg

# identify the nearest likely obstacle at tick 1800
./bin/palimpsest problems run-424242-1700000000-1 1800

# evaluate bounded local variants and retain the verified winner
./bin/palimpsest ab run-424242-1700000000-1 1800 /tmp/revision

# explicitly delete exactly one recording
./bin/palimpsest delete-run run-424242-1700000000-1

# enumerate commands as machine-readable JSON
./bin/palimpsest help
```

The A/B command writes `.level.json`, `.report.json`, `.svg`, and `.overlay.json` at the given prefix. Its report includes observed approach velocity/state, timing window, landing margin, failure reason, nine tick-offset perturbation samples, before/after completion rate, the local geometry edit, and whole-level reachability/collectible/generator constraints. If no candidate improves both margin and bounded completion, it fails without retaining a change.
