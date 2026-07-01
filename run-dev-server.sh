#!/usr/bin/env bash
set -u

APP_DIR="/home/kkempski/other_repos/Dag1/DagoniteEmpire/DagoniteEmpire"
LOG="/tmp/dagonite-app.log"
PID_FILE="/tmp/dagonite-app.pid"
DOTNET_PID_FILE="/tmp/dagonite-dotnet.pid"
LAUNCH_PROFILE="http"

cd "$APP_DIR"

log_line() {
  echo "=== $1 $(date -Iseconds) ===" >> "$LOG"
}

on_exit() {
  local code=$?
  log_line "watchdog exit code $code (pid $$)"
  rm -f "$DOTNET_PID_FILE"
}

trap on_exit EXIT
trap 'log_line "watchdog received SIGTERM (pid $$)"; exit 0' TERM
trap 'log_line "watchdog received SIGHUP (pid $$)"' HUP

echo $$ > "$PID_FILE"
log_line "watchdog pid $$"

build_once() {
  if [[ ! -f "$APP_DIR/bin/Debug/net9.0/DagoniteEmpire.dll" ]]; then
    log_line "dotnet build"
    dotnet build --verbosity quiet >> "$LOG" 2>&1 || return 1
  fi
}

while true; do
  build_once || {
    log_line "dotnet build failed"
    sleep 5
    continue
  }

  log_line "dotnet run start"
  dotnet run --launch-profile "$LAUNCH_PROFILE" --no-build >> "$LOG" 2>&1 &
  dotnet_pid=$!
  echo "$dotnet_pid" > "$DOTNET_PID_FILE"

  wait "$dotnet_pid"
  code=$?
  rm -f "$DOTNET_PID_FILE"
  log_line "dotnet run exit $code"

  sleep 2
done
