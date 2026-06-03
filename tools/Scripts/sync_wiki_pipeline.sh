#!/usr/bin/env bash
# sync_wiki_pipeline.sh — pełna ścieżka: docx → vault → Quartz content → build → wwwroot/wiki
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BASE="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "📜 1/4 build_obsidian_vault.py"
python3 "$SCRIPT_DIR/build_obsidian_vault.py"

echo ""
echo "📦 2/4 sync_vault_to_quartz.sh"
"$SCRIPT_DIR/sync_vault_to_quartz.sh"

echo ""
echo "🏗️ 3/4 build_wiki_for_empire.sh"
"$SCRIPT_DIR/build_wiki_for_empire.sh"

echo ""
echo "✅ Pipeline wiki zakończony. Zbuduj obraz Docker, jeśli wdrażasz na serwer."
