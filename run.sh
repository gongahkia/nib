#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$script_dir"

if [[ -x artifacts/linux-x64/Summing ]]; then
    exec artifacts/linux-x64/Summing "$@"
fi

exec tools/dotnet.sh run --project src/Summing/Summing.csproj -- "$@"
