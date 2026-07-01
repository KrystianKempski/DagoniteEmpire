#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$SCRIPT_DIR/DagoniteEmpire"
RUN_SCRIPT="$SCRIPT_DIR/run-dev-server.sh"
LOG="/tmp/dagonite-app.log"
PID_FILE="/tmp/dagonite-app.pid"
DOTNET_PID_FILE="/tmp/dagonite-dotnet.pid"
PORT=5093
LAUNCH_PROFILE="http"
APP_URL="http://localhost:${PORT}"
STARTUP_TIMEOUT_SEC=180
POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-postgres-pgvector}"

log() {
  printf '[%s] %s\n' "$(date '+%H:%M:%S')" "$*"
}

ensure_postgres() {
  if ! command -v docker >/dev/null 2>&1; then
    log "Warning: docker not found; skipping PostgreSQL check."
    return 0
  fi

  if ! docker ps --format '{{.Names}}' | grep -qx "$POSTGRES_CONTAINER"; then
    log "Starting PostgreSQL container ($POSTGRES_CONTAINER)..."
    if ! docker start "$POSTGRES_CONTAINER" >/dev/null 2>&1; then
      log "ERROR: Could not start container $POSTGRES_CONTAINER"
      exit 1
    fi
  fi

  local i
  for i in $(seq 1 30); do
    if docker exec "$POSTGRES_CONTAINER" pg_isready -U dagonite -d DagoniteEmpire -q 2>/dev/null; then
      return 0
    fi
    sleep 1
  done

  log "ERROR: PostgreSQL is not ready."
  exit 1
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

  if [[ -f "$DOTNET_PID_FILE" ]]; then
    local dotnet_pid
    dotnet_pid="$(cat "$DOTNET_PID_FILE" 2>/dev/null || true)"
    if [[ -n "$dotnet_pid" ]] && kill -0 "$dotnet_pid" 2>/dev/null; then
      kill "$dotnet_pid" 2>/dev/null || true
      sleep 1
      kill -9 "$dotnet_pid" 2>/dev/null || true
    fi
    rm -f "$DOTNET_PID_FILE"
  fi

  pkill -f "$RUN_SCRIPT" 2>/dev/null || true

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
  if command -v setsid >/dev/null 2>&1; then
    setsid nohup bash "$RUN_SCRIPT" >>"$LOG" 2>&1 &
  else
    nohup bash "$RUN_SCRIPT" >>"$LOG" 2>&1 &
  fi
  disown 2>/dev/null || true

  for _ in $(seq 1 20); do
    if [[ -f "$PID_FILE" ]] && kill -0 "$(cat "$PID_FILE")" 2>/dev/null; then
      log "Watchdog running (pid $(cat "$PID_FILE"))"
      return 0
    fi
    sleep 0.25
  done

  log "Warning: watchdog pid file not ready."
}

wait_for_app() {
  log "Waiting for app at ${APP_URL} (max ${STARTUP_TIMEOUT_SEC}s)..."

  local elapsed=0
  while (( elapsed < STARTUP_TIMEOUT_SEC )); do
    if curl --noproxy '*' -s -o /dev/null --max-time 2 "${APP_URL}/" 2>/dev/null; then
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

  ensure_postgres
  stop_watchdog
  stop_dotnet_processes
  free_port
  wait_for_port_free

  log "Building app..."
  (cd "$APP_DIR" && dotnet build --verbosity quiet) || {
    log "ERROR: dotnet build failed."
    exit 1
  }

  echo "=== restart $(date -Iseconds) ===" >> "$LOG"
  start_watchdog
  wait_for_app
}

main "$@"
