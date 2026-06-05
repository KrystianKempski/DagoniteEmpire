#!/usr/bin/env python3
"""
extract_kraina_metadata.py — read date/location from local Kraina .docx headers
and update outputKraina/adventure_map.json (+ sortKey + adventure_map.md).

Usage:
  python3 extract_kraina_metadata.py
  python3 extract_kraina_metadata.py --dry-run
"""

from __future__ import annotations

import argparse
import json
import shutil
from datetime import datetime
from pathlib import Path

from docx import Document

from assign_sort_keys import (
    IS_FOLDER,
    build_mermaid,
    date_to_sortkey,
    sortkey_to_human,
)
from kraina_colors import KRAINA_JSON, KRAINA_ROOT, resolve_map_name
from kraina_header import BELL_WORDS, extract_header_from_docx, parse_header
from kraina_naming import IS_FOLDER as FOLDER_MIME

SCRIPT_DIR = Path(__file__).resolve().parent
MD_FILE = SCRIPT_DIR / "outputKraina" / "adventure_map.md"

# Akt 13/14 folder titles (user-renamed locally and on Drive)
AKT13_FOLDER = "Akt 13 Ponownie rozdzielona drużyna"
AKT14_FOLDER = "Akt 14 Drużyna się\u00a0zbiera"

FOLDER_TITLE_FIX: dict[str, str] = {
    "Akt 13": AKT13_FOLDER,
    "Akt 14": AKT14_FOLDER,
}


def iter_local_docx() -> list[Path]:
    files: list[Path] = []
    for p in sorted(KRAINA_ROOT.rglob("*.docx")):
        if p.name.startswith("~$"):
            continue
        files.append(p)
    return files


def stem_from_path(path: Path) -> str:
    return path.stem


def update_folder_fields(node: dict) -> bool:
    changed = False
    old_folder = node.get("folder")
    if old_folder in FOLDER_TITLE_FIX:
        new_name = FOLDER_TITLE_FIX[old_folder]
        node["folder"] = new_name
        chain = list(node.get("folderChain") or [])
        if chain and chain[-1] in FOLDER_TITLE_FIX:
            chain[-1] = new_name
            node["folderChain"] = chain
        changed = True
    if node.get("mimeType") == FOLDER_MIME and node.get("name") in FOLDER_TITLE_FIX:
        node["name"] = FOLDER_TITLE_FIX[node["name"]]
        changed = True
    return changed


def regenerate_md(data: dict) -> str:
    nodes = {n["id"]: n for n in data["nodes"]}
    edges = data["edges"]
    mermaid_str = build_mermaid(nodes, edges)

    doc_nodes = sorted(
        [n for n in data["nodes"] if n["mimeType"] != IS_FOLDER],
        key=lambda n: (n.get("sortKey") is None, n.get("sortKey") or 0, n["name"]),
    )
    table_lines = [
        "| # | Name | Sort key | Date | Location | Players | Folder | URL |",
        "|---|------|----------|------|----------|---------|--------|-----|",
    ]
    for i, node in enumerate(doc_nodes, 1):
        url_md = f"[link]({node['url']})" if node.get("url") else "—"
        date = node.get("date", "—")
        location = node.get("location", "—")
        players = ", ".join(node.get("players", [])) or "—"
        sk_human = sortkey_to_human(node.get("sortKey"))
        table_lines.append(
            f"| {i} | {node['name']} | {sk_human} | {date} | {location} | "
            f"{players} | {node.get('folder', '—')} | {url_md} |"
        )

    generated = datetime.utcnow().isoformat() + "Z"
    node_count = len(doc_nodes)
    edge_count = len(edges)
    return f"""# Adventure Link Map

Generated: {generated}  
Nodes: {node_count}  
Edges: {edge_count}  

{mermaid_str}

## Node List (sorted by date)

{chr(10).join(table_lines)}
"""


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    data = json.loads(KRAINA_JSON.read_text(encoding="utf-8"))
    node_by_name: dict[str, dict] = {}
    doc_nodes = [n for n in data["nodes"] if n["mimeType"] != IS_FOLDER]
    for n in doc_nodes:
        node_by_name[n["name"]] = n

    local_files = iter_local_docx()
    print(f"Local docx: {len(local_files)}")

    updated = 0
    folder_fixed = 0
    unmatched: list[str] = []

    for path in local_files:
        stem = stem_from_path(path)
        map_name = resolve_map_name(stem, set(node_by_name))
        if not map_name:
            unmatched.append(stem)
            continue

        doc = Document(str(path))
        header = extract_header_from_docx(doc.paragraphs)
        date, location = parse_header(stem, header)

        node = node_by_name[map_name]
        old_date, old_loc = node.get("date"), node.get("location")
        new_date = date if date != "brak info" else old_date
        new_loc = location if location != "brak info" else old_loc
        if new_date != old_date or new_loc != old_loc:
            node["date"] = new_date
            node["location"] = new_loc
            updated += 1
            print(f"  {map_name}")
            if new_date != old_date:
                print(f"    date: {old_date!r} -> {new_date!r}")
            if new_loc != old_loc:
                print(f"    loc:  {old_loc!r} -> {new_loc!r}")

    for node in data["nodes"]:
        if update_folder_fields(node):
            folder_fixed += 1

    sk_changed = 0
    for node in data["nodes"]:
        if node["mimeType"] == IS_FOLDER:
            node["sortKey"] = None
            continue
        sk = date_to_sortkey(node.get("date", ""))
        if node.get("sortKey") != sk:
            node["sortKey"] = sk
            sk_changed += 1

    if unmatched:
        print(f"\nUnmatched local stems ({len(unmatched)}):")
        for s in unmatched[:20]:
            print(f"  - {s}")
        if len(unmatched) > 20:
            print(f"  ... +{len(unmatched) - 20} more")

    bad_date = sum(
        1 for n in doc_nodes if n.get("date") in (None, "brak info", "—")
    )
    messy = sum(
        1
        for n in doc_nodes
        if n.get("location")
        and any(w in n["location"].lower() for w in BELL_WORDS[:12])
    )
    print(
        f"\nSummary: metadata rows changed={updated}, folders fixed={folder_fixed}, "
        f"sortKeys={sk_changed}, brak date={bad_date}, bell-ish loc={messy}"
    )

    if args.dry_run:
        print("(dry-run — not writing files)")
        return

    backup = KRAINA_JSON.with_suffix(".json.bak")
    shutil.copy2(KRAINA_JSON, backup)
    KRAINA_JSON.write_text(
        json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    MD_FILE.write_text(regenerate_md(data), encoding="utf-8")
    print(f"Wrote {KRAINA_JSON} (backup {backup.name})")
    print(f"Wrote {MD_FILE}")


if __name__ == "__main__":
    main()
