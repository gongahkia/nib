#!/usr/bin/env bash
set -euo pipefail

user_home="$(getent passwd "$(id -u)" | cut -d: -f6)"
user_dotnet="${XDG_DATA_HOME:-${user_home}/.local/share}/dotnet/dotnet"
dotnet_bin=""

if [[ -x "$user_dotnet" ]] && "$user_dotnet" --info >/dev/null 2>&1; then
    dotnet_bin="$user_dotnet"
elif command -v dotnet >/dev/null 2>&1 && dotnet --info >/dev/null 2>&1; then
    dotnet_bin="$(command -v dotnet)"
fi

if [[ -z "$dotnet_bin" ]]; then
    echo "SUMMING could not find a working .NET 9 SDK. Install dotnet-sdk-9.0, then try again." >&2
    exit 1
fi

dotnet_root="$(dirname -- "$dotnet_bin")"
exec env DOTNET_ROOT="$dotnet_root" PATH="$dotnet_root:$PATH" "$dotnet_bin" "$@"
