#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$SCRIPT_DIR/DagoniteEmpire"
RUN_SCRIPT="$SCRIPT_DIR/run-dev-server.sh"
LOG="/tmp/dagonite-app.log"
PID_FILE="/tmp/dagonite-app.pid"
PORT=5093
LAUNCH_PROFILE="http"
APP_URL="http://localhost:${PORT}"
STARTUP_TIMEOUT_SEC=120

log() {
  printf '[%s] %s\n' "$(date '+%H:%M:%S')" "$*"
}

stop_watchdog() {
  if [[ ! -f "$PID_FILE" ]]; then
    return 0
  fi

  local pid
  pid="$(cat "$PID_FILE" 2>/dev/null || true)"
  if [[ -z "$pid" ]]; then
    rm -f "$PID_FILE"
    return 0
  fi

  if kill -0 "$pid" 2>/dev/null; then
    log "Stopping watchdog (pid $pid)..."
    pkill -P "$pid" 2>/dev/null || true
    kill "$pid" 2>/dev/null || true

    for _ in $(seq 1 10); do
      kill -0 "$pid" 2>/dev/null || break
      sleep 0.5
    done

    if kill -0 "$pid" 2>/dev/null; then
      kill -9 "$pid" 2>/dev/null || true
    fi
  fi

  rm -f "$PID_FILE"
}

stop_dotnet_processes() {
  log "Stopping Dagonite Empire dotnet processes..."

  pkill -f "$RUN_SCRIPT" 2>/dev/null || true
  pkill -f "dotnet run --launch-profile ${LAUNCH_PROFILE}" 2>/dev/null || true
  pkill -f "DagoniteEmpire.dll" 2>/dev/null || true

  if [[ -d "$APP_DIR" ]]; then
    while read -r pid; do
      [[ -n "$pid" ]] || continue
      kill "$pid" 2>/dev/null || true
    done < <(pgrep -f "$APP_DIR" 2>/dev/null || true)
  fi

  sleep 1
}

free_port() {
  if command -v ss >/dev/null 2>&1 && ss -tln | grep -q ":${PORT} "; then
    log "Freeing port ${PORT}..."
  elif command -v lsof >/dev/null 2>&1 && lsof -ti ":${PORT}" >/dev/null 2>&1; then
    log "Freeing port ${PORT}..."
  else
    return 0
  fi

  if command -v fuser >/dev/null 2>&1; then
    fuser -k "${PORT}/tcp" 2>/dev/null || true
  elif command -v lsof >/dev/null 2>&1; then
    local pids
    pids="$(lsof -ti ":${PORT}" 2>/dev/null || true)"
    if [[ -n "$pids" ]]; then
      kill $pids 2>/dev/null || true
      sleep 1
      kill -9 $pids 2>/dev/null || true
    fi
  fi
}

wait_for_port_free() {
  for _ in $(seq 1 20); do
    if command -v ss >/dev/null 2>&1; then
      ss -tln | grep -q ":${PORT} " || return 0
    elif command -v lsof >/dev/null 2>&1; then
      lsof -ti ":${PORT}" >/dev/null 2>&1 || return 0
    else
      return 0
    fi
    sleep 0.5
  done

  log "Warning: port ${PORT} may still be in use."
}

start_watchdog() {
  if [[ ! -x "$RUN_SCRIPT" ]]; then
    chmod +x "$RUN_SCRIPT"
  fi

  log "Starting watchdog..."
  nohup bash "$RUN_SCRIPT" >/dev/null 2>&1 &
  disown 2>/dev/null || true
}

wait_for_app() {
  log "Waiting for app at ${APP_URL} (max ${STARTUP_TIMEOUT_SEC}s)..."

  local elapsed=0
  while (( elapsed < STARTUP_TIMEOUT_SEC )); do
    if curl -s -o /dev/null --max-time 2 "${APP_URL}/" 2>/dev/null; then
      log "App is up: ${APP_URL}"
      log "Log file: ${LOG}"
      return 0
    fi

    sleep 2
    elapsed=$((elapsed + 2))
  done

  log "App did not become ready in time."
  log "Check log: tail -50 ${LOG}"
  return 1
}

main() {
  if [[ ! -d "$APP_DIR" ]]; then
    echo "App directory not found: $APP_DIR" >&2
    exit 1
  fi

  if [[ ! -f "$RUN_SCRIPT" ]]; then
    echo "Run script not found: $RUN_SCRIPT" >&2
    exit 1
  fi

  log "Restarting Dagonite Empire dev server..."

  stop_watchdog
  stop_dotnet_processes
  free_port
  wait_for_port_free

  echo "=== restart $(date -Iseconds) ===" >> "$LOG"
  start_watchdog
  wait_for_app
}

main "$@"
