#!/usr/bin/env bash
set -euo pipefail

if command -v dotnet >/dev/null 2>&1; then
    dotnet_bin="$(command -v dotnet)"
else
    user_home="$(getent passwd "$(id -u)" | cut -d: -f6)"
    dotnet_bin="${XDG_DATA_HOME:-${user_home}/.local/share}/dotnet/dotnet"
fi

if [[ ! -x "$dotnet_bin" ]]; then
    echo "SUMMING requires the .NET 9 SDK. Install dotnet-sdk-9.0, then run this script again." >&2
    exit 1
fi

exec "$dotnet_bin" run --project src/Summing/Summing.csproj -- "$@"
