#!/usr/bin/env python3
"""
Eksport mapy Kraina do podglądu w Obsidian (jak Bonefyre).

Tworzy vault:
  tools/Resources/KrainaMapa/
    Mapy/Mapa powiązań.md   — diagram Mermaid + tabela
    README.md               — jak otworzyć w Obsidian
    mapa.html               — podgląd w przeglądarce (mermaid.js)

Źródło: outputKraina/adventure_map.json + assign_sort_keys.build_mermaid
"""

from __future__ import annotations

import json
import re
from datetime import datetime
from pathlib import Path

from assign_sort_keys import (
    IS_FOLDER,
    build_mermaid,
    sortkey_to_human,
)

SCRIPT_DIR = Path(__file__).resolve().parent
JSON_FILE = SCRIPT_DIR / "outputKraina" / "adventure_map.json"
VAULT_DIR = SCRIPT_DIR.parent / "Resources" / "KrainaMapa"
MAP_MD = VAULT_DIR / "Mapy" / "Mapa powiązań.md"
HTML_FILE = VAULT_DIR / "mapa.html"
README = VAULT_DIR / "README.md"


def build_table(nodes_list: list[dict]) -> str:
    doc_nodes = sorted(
        [n for n in nodes_list if n["mimeType"] != IS_FOLDER],
        key=lambda n: (n.get("sortKey") is None, n.get("sortKey") or 0, n["name"]),
    )
    lines = [
        "| # | Name | Sort key | Date | Inferred | Location | Folder |",
        "|---|------|----------|------|----------|----------|--------|",
    ]
    for i, node in enumerate(doc_nodes, 1):
        inf = "tak" if node.get("dateInferred") else "—"
        date = node.get("date", "—")
        loc = (node.get("location") or "—")[:60]
        lines.append(
            f"| {i} | {node['name']} | {sortkey_to_human(node.get('sortKey'))} | "
            f"{date} | {inf} | {loc} | {node.get('folder', '—')} |"
        )
    return "\n".join(lines)


def mermaid_to_html(mermaid_body: str) -> str:
    """Wyciąga blok mermaid z markdown i owija w HTML z CDN."""
    return f"""<!DOCTYPE html>
<html lang="pl">
<head>
  <meta charset="utf-8"/>
  <title>Kraina Możliwości — mapa scen</title>
  <script type="module">
    import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
    mermaid.initialize({{ startOnLoad: true, maxTextSize: 200000, flowchart: {{ htmlLabels: true }} }});
  </script>
  <style>
    body {{ font-family: system-ui, sans-serif; margin: 1rem; background: #1e1e1e; color: #ddd; }}
    .mermaid {{ background: #fff; padding: 1rem; border-radius: 8px; overflow: auto; }}
    p.note {{ max-width: 60rem; line-height: 1.5; }}
  </style>
</head>
<body>
  <h1>Mapa powiązań — Kraina Możliwości</h1>
  <p class="note">Wygenerowano: {datetime.utcnow().isoformat()}Z.
  Obsidian: otwórz folder <code>KrainaMapa</code> jako vault i plik
  <code>Mapy/Mapa powiązań.md</code> (tryb podglądu).</p>
  <pre class="mermaid">
{mermaid_body}
  </pre>
</body>
</html>
"""


def main() -> None:
    data = json.loads(JSON_FILE.read_text(encoding="utf-8"))
    nodes = {n["id"]: n for n in data["nodes"]}
    edges = data["edges"]
    mermaid_str = build_mermaid(nodes, edges)
    mermaid_inner = re.search(r"```mermaid\n(.*?)```", mermaid_str, re.DOTALL)
    mermaid_body = mermaid_inner.group(1).strip() if mermaid_inner else ""

    generated = datetime.utcnow().isoformat() + "Z"
    node_count = sum(1 for n in data["nodes"] if n["mimeType"] != IS_FOLDER)
    md = f"""---
title: "Mapa powiązań — Kraina Możliwości"
tags: [mapa, kraina]
---

# Mapa powiązań scen

Wygenerowano: {generated}  
Scen: {node_count} · Krawędzi: {len(edges)}

> **Podgląd:** w Cursor/VS Code diagram Mermaid często się nie renderuje.
> Otwórz ten vault w **Obsidian** albo plik `mapa.html` w przeglądarce.

{mermaid_str}

## Lista scen (chronologicznie)

{build_table(data["nodes"])}
"""

    VAULT_DIR.mkdir(parents=True, exist_ok=True)
    MAP_MD.parent.mkdir(parents=True, exist_ok=True)
    MAP_MD.write_text(md, encoding="utf-8")
    HTML_FILE.write_text(mermaid_to_html(mermaid_body), encoding="utf-8")
    README.write_text(
        """# KrainaMapa — vault Obsidian

## Mapa z folderami i linkami

1. Zainstaluj [Obsidian](https://obsidian.md/).
2. **Open folder as vault** → wybierz ten katalog (`KrainaMapa`).
3. Otwórz **Mapy → Mapa powiązań.md** i włącz podgląd (Ctrl+E).

Diagram pokazuje **subgraphy = akty** (foldery) oraz strzałki = linki między Google Docs.

## Przeglądarka

Otwórz `mapa.html` — renderuje ten sam diagram (wymaga internetu do mermaid.js).

## Aktualizacja

```bash
cd DagoniteEmpire/tools/Scripts
python3 infer_kraina_dates.py      # opcjonalnie: daty z grafu
python3 assign_sort_keys.py        # sortKey + adventure_map.md
python3 export_kraina_obsidian_map.py
```
""",
        encoding="utf-8",
    )
    print(f"Vault: {VAULT_DIR}")
    print(f"  {MAP_MD}")
    print(f"  {HTML_FILE}")
    print(f"  {README}")


if __name__ == "__main__":
    main()
