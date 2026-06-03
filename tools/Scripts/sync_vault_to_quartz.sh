#!/usr/bin/env bash
# sync_vault_to_quartz.sh
# Kopiuje zawartość vaultu Obsidian do folderu content/ Quartza
# i uruchamia podgląd lokalny.

set -e

BASE="$(cd "$(dirname "$0")/.." && pwd)"
VAULT="$BASE/Resources/Wiki"
# Quartz musi być POZA tools/ (które jest w .gitignore głównego repo)
QUARTZ="$(cd "$BASE/../../dagonite-wiki" && pwd)"
CONTENT="$QUARTZ/content"

if [ ! -d "$VAULT" ]; then
    echo "❌ Vault nie istnieje: $VAULT"
    echo "   Najpierw uruchom: python3 build_obsidian_vault.py"
    exit 1
fi

echo "🧹 Czyszczę stare content/..."
rm -rf "$CONTENT"
mkdir -p "$CONTENT"

echo "📦 Kopiuję vault → content/..."
# kopiuj wszystko z vaultu z wyjątkiem .obsidian i plików tymczasowych
rsync -av --exclude='.obsidian' --exclude='.~*' --exclude='.trash' \
    "$VAULT/" "$CONTENT/"

# Quartz wymaga, by strona główna nazywała się index.md
if [ -f "$CONTENT/Home.md" ]; then
    mv "$CONTENT/Home.md" "$CONTENT/index.md"
    echo "  ✓ Home.md → index.md"
fi

# Pliki _Index.md → index.md w każdym podfolderze (Quartz konwencja)
find "$CONTENT" -name "_Index.md" | while read -r f; do
    dir="$(dirname "$f")"
    mv "$f" "$dir/index.md"
done
echo "  ✓ _Index.md → index.md w podfolderach"

echo ""
echo "✅ Sync zakończony."
echo "   Liczba plików: $(find "$CONTENT" -name '*.md' | wc -l)"
echo ""
echo "▶  Następne kroki:"
echo "   ./sync_wiki_pipeline.sh     # pełny build + deploy do DagoniteEmpire"
echo "   # lub: cd $QUARTZ && npx quartz build --serve"
