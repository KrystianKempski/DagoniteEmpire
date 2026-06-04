#!/usr/bin/env bash
# build_wiki_for_empire.sh — Quartz build + deploy do wwwroot/wiki (przed docker build).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
EMPIRE_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
WIKI_ROOT="$(cd "$EMPIRE_ROOT/../dagonite-wiki" && pwd)"

if [ ! -f "$WIKI_ROOT/package.json" ]; then
    echo "❌ Nie znaleziono dagonite-wiki: $WIKI_ROOT"
    exit 1
fi

echo "📚 Buduję Quartz w $WIKI_ROOT ..."
cd "$WIKI_ROOT"
if [ ! -d node_modules ]; then
    npm ci
fi
if [ ! -d .quartz/plugins ]; then
    npx quartz plugin install
fi
npx quartz build

echo ""
"$SCRIPT_DIR/deploy_wiki_to_wwwroot.sh"
