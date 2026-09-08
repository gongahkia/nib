#!/usr/bin/env sh
set -eu

entry_summary() {
  title=${1:?title is required}
  state=${2:-open}
  case "$state" in
    settled) mark='x' ;;
    open) mark='.' ;;
    *) printf 'unknown state: %s\n' "$state" >&2; return 2 ;;
  esac
  printf '[%s] %s\n' "$mark" "$title"
}

entry_summary "Test the quiet palette" "${LEDGER_STATE:-open}"
