#!/usr/bin/env bash
# deploy_wiki_to_wwwroot.sh
# Copies the built Quartz site (dagonite-wiki/public) into DagoniteEmpire/wwwroot/wiki
# and rewrites the base path from GitHub Pages (/dagonite-wiki) to in-app hosting (/wiki).

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
EMPIRE_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
WIKI_ROOT="$(cd "$EMPIRE_ROOT/../dagonite-wiki" && pwd)"
QUARTZ_PUBLIC="$WIKI_ROOT/public"
TARGET="$EMPIRE_ROOT/DagoniteEmpire/wwwroot/wiki"
BASE_PATH="${WIKI_BASE_PATH:-/wiki}"

if [ ! -d "$QUARTZ_PUBLIC" ] || [ ! -f "$QUARTZ_PUBLIC/index.html" ]; then
    echo "❌ Brak buildu Quartza: $QUARTZ_PUBLIC"
    echo "   cd $WIKI_ROOT && npx quartz build"
    exit 1
fi

echo "🧹 Czyszczę $TARGET ..."
rm -rf "$TARGET"
mkdir -p "$TARGET"

echo "📦 Kopiuję public → wwwroot/wiki ..."
rsync -a --delete "$QUARTZ_PUBLIC/" "$TARGET/"

echo "🔧 Przepisuję base path → $BASE_PATH ..."
export TARGET BASE_PATH
python3 <<'PY'
import os
import re
from pathlib import Path

target = Path(os.environ["TARGET"])
base = os.environ["BASE_PATH"].rstrip("/") or "/wiki"

# Quartz slugs with commas (e.g. "barana,-cz.-2") — without %2C the server/SPA often returns 404.
# Only internal wiki links (data-slug or relative href), not external URLs (Google Fonts, etc.).
_COMMA_ATTR_RE = re.compile(
    r'(?P<attr>(?:data-slug)=["\'])(?P<url>[^"\']*?,[^"\']*?)(?=["\'])'
    r'|(?P<href>href=["\'])(?P<hurl>(?:\.\./|\./|/wiki/)[^"\']*?,[^"\']*?)(?=["\'])',
    re.IGNORECASE,
)


def encode_commas_in_wiki_urls(text: str) -> str:
    def repl(m: re.Match) -> str:
        if m.group("url"):
            return f'{m.group("attr")}{m.group("url").replace(",", "%2C")}'
        return f'{m.group("href")}{m.group("hurl").replace(",", "%2C")}'

    return _COMMA_ATTR_RE.sub(repl, text)


replacements = [
    ("/dagonite-wiki", base),
    ('data-basepath="/dagonite-wiki"', f'data-basepath="{base}"'),
    ("krystiankempski.github.io/dagonite-wiki", "dagonite-empire.drik.it/wiki"),
    ("krystiankempski.github.io/wiki/", "dagonite-empire.drik.it/wiki/"),
    ("https://krystiankempski.github.io/wiki", "https://dagonite-empire.drik.it/wiki"),
]

extensions = {".html", ".js", ".json", ".xml", ".css", ".webp"}
count = 0
for path in target.rglob("*"):
    if not path.is_file() or path.suffix.lower() not in extensions:
        continue
    text = path.read_text(encoding="utf-8", errors="ignore")
    original = text
    for old, new in replacements:
        text = text.replace(old, new)
    text = encode_commas_in_wiki_urls(text)
    if text != original:
        path.write_text(text, encoding="utf-8")
        count += 1

print(f"  ✓ zaktualizowano {count} plików")
PY

echo "🔐 Buduję manifest dostępu..."
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
python3 "$SCRIPT_DIR/build_wiki_access_manifest.py"

echo ""
echo "✅ Wiki wdrożone do: $TARGET"
echo "   Otwórz w aplikacji: /wiki"
