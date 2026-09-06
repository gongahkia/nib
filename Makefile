DOTNET ?= ./tools/dotnet.sh
PROJECT := src/Summing/Summing.csproj

.PHONY: restore build run publish-fedora format test verify clean assets

restore:
	$(DOTNET) restore $(PROJECT)

build:
	$(DOTNET) build $(PROJECT) --no-restore

run:
	$(DOTNET) run --project $(PROJECT)

publish-fedora:
	$(DOTNET) publish $(PROJECT) -c Release -r linux-x64 --self-contained true -o artifacts/linux-x64

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
