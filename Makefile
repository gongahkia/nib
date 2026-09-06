DOTNET ?= dotnet
PROJECT := src/Summing/Summing.csproj

.PHONY: restore build run format test verify clean assets

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

verify: build
	$(DOTNET) src/Summing/bin/Debug/net9.0/Summing.dll --verify-generation
	$(DOTNET) src/Summing/bin/Debug/net9.0/Summing.dll --verify-serialization
	$(DOTNET) src/Summing/bin/Debug/net9.0/Summing.dll --verify-systems

assets:
	python3 tools/generate_sprites.py

clean:
	$(DOTNET) clean $(PROJECT)
