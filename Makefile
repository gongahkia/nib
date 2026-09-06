DOTNET ?= dotnet
PROJECT := src/Summing/Summing.csproj

.PHONY: restore build run format test clean assets

restore:
	$(DOTNET) restore $(PROJECT)

build:
	$(DOTNET) build $(PROJECT) --no-restore

run:
	$(DOTNET) run --project $(PROJECT)

format:
	$(DOTNET) format $(PROJECT) --no-restore

test:
	$(DOTNET) test Summing.slnx --no-restore

assets:
	python3 tools/generate_sprites.py

clean:
	$(DOTNET) clean $(PROJECT)
