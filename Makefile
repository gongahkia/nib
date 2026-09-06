.PHONY: run test smoke cli

run:
	love .

smoke:
	SDL_VIDEODRIVER=dummy SDL_AUDIODRIVER=dummy timeout 3s love .

test:
	luajit tests/run.lua

cli:
	luajit cli.lua help
