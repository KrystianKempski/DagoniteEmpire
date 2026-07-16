#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -d "$ROOT_DIR/DA_DataAccess" && -d "$ROOT_DIR/DagoniteEmpire" ]]; then
  PROJECT_PATH="DA_DataAccess"
  STARTUP_PATH="DagoniteEmpire"
elif [[ -d "$ROOT_DIR/DagoniteEmpire/DA_DataAccess" && -d "$ROOT_DIR/DagoniteEmpire/DagoniteEmpire" ]]; then
  PROJECT_PATH="DagoniteEmpire/DA_DataAccess"
  STARTUP_PATH="DagoniteEmpire/DagoniteEmpire"
else
  echo "Could not locate DA_DataAccess and DagoniteEmpire projects from: $ROOT_DIR"
  exit 1
fi

ACTION="${1:-update}"
shift || true
EXTRA_ARGS=("$@")

usage() {
  echo "Usage:"
  echo "  ./run-migrations.sh [update|list|script] [extra dotnet-ef args]"
  echo
  echo "Examples:"
  echo "  ./run-migrations.sh"
  echo "  ./run-migrations.sh list"
  echo "  ./run-migrations.sh script --idempotent"
}

if [[ "$ACTION" == "-h" || "$ACTION" == "--help" ]]; then
  usage
  exit 0
fi

case "$ACTION" in
  update|list|script)
    ;;
  *)
    echo "Unsupported action: $ACTION"
    usage
    exit 1
    ;;
esac

cd "$ROOT_DIR"

COMMAND_GROUP="database"
COMMAND_NAME="update"
case "$ACTION" in
  update)
    COMMAND_GROUP="database"
    COMMAND_NAME="update"
    ;;
  list)
    COMMAND_GROUP="migrations"
    COMMAND_NAME="list"
    ;;
  script)
    COMMAND_GROUP="migrations"
    COMMAND_NAME="script"
    ;;
esac

COMMON_ARGS=(
  "$COMMAND_GROUP"
  "$COMMAND_NAME"
  "--project" "$PROJECT_PATH"
  "--startup-project" "$STARTUP_PATH"
  "${EXTRA_ARGS[@]}"
)

run_local_tool() {
  dotnet tool restore >/dev/null || return 1
  dotnet tool run dotnet-ef "${COMMON_ARGS[@]}"
}

run_global_tool() {
  "$HOME/.dotnet/tools/dotnet-ef" "${COMMON_ARGS[@]}"
}

run_dll_tool() {
  local dll_path
  dll_path="$(ls "$HOME/.nuget/packages/dotnet-ef/"*/tools/net8.0/any/dotnet-ef.dll 2>/dev/null | sort -V | tail -1 || true)"
  if [[ -z "$dll_path" ]]; then
    return 1
  fi
  dotnet "$dll_path" "${COMMON_ARGS[@]}"
}

echo "Running EF migration command: $ACTION"

if run_local_tool; then
  exit 0
fi

echo "Local dotnet tool failed, trying global ~/.dotnet/tools/dotnet-ef..."
if [[ -x "$HOME/.dotnet/tools/dotnet-ef" ]] && run_global_tool; then
  exit 0
fi

echo "Global tool failed, trying dotnet-ef.dll fallback..."
if run_dll_tool; then
  exit 0
fi

echo "All migration command paths failed."
echo "Try installing tool globally:"
echo "  dotnet tool install --global dotnet-ef"
exit 1
