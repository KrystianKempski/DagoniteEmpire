#!/usr/bin/env bash
set -u

APP_DIR="/home/kkempski/other_repos/Dag1/DagoniteEmpire/DagoniteEmpire"
LOG="/tmp/dagonite-app.log"
PID_FILE="/tmp/dagonite-app.pid"

cd "$APP_DIR"

echo $$ > "$PID_FILE"
echo "=== watchdog pid $$ $(date -Iseconds) ===" >> "$LOG"

while true; do
  echo "=== dotnet run start $(date -Iseconds) ===" >> "$LOG"
  dotnet run --launch-profile http >> "$LOG" 2>&1
  code=$?
  echo "=== dotnet run exit $code $(date -Iseconds) ===" >> "$LOG"
  sleep 2
done
