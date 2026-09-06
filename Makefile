.PHONY: run test smoke soak package cli

run:
	love .

smoke:
	@sh -c 'timeout 3s love .; code=$$?; test $$code -eq 0 -o $$code -eq 124'

test:
	luajit tests/run.lua

soak:
	luajit scripts/soak.lua 200

package:
	mkdir -p build
	zip -9 -r build/palimpsest-run.love . -x '.git/*' 'build/*' 'artifacts/*' '.local/*' 'AGENTS.md'

cli:
	luajit cli.lua help
