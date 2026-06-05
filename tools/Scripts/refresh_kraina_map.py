#!/usr/bin/env python3
"""
refresh_kraina_map.py — odśwież graf z Google Drive + metadane z lokalnych .docx,
zachowując wywnioskowane daty (dateInferred) tam, gdzie brak nowego nagłówka.

Usage:
  python3 refresh_kraina_map.py
  python3 refresh_kraina_map.py --skip-drive   # tylko lokalne .docx
"""

from __future__ import annotations

import argparse
import json
import re
import shutil
from datetime import datetime
from pathlib import Path

from docx import Document

from assign_sort_keys import IS_FOLDER, date_to_sortkey
from extract_kraina_metadata import regenerate_md
from infer_kraina_dates import run_inference
from kraina_colors import KRAINA_JSON, KRAINA_ROOT, resolve_map_name
from kraina_header import extract_header_from_docx, parse_header
from kraina_naming import IS_FOLDER as FOLDER_MIME, should_ignore

SCRIPT_DIR = Path(__file__).resolve().parent
MD_FILE = SCRIPT_DIR / "outputKraina" / "adventure_map.md"
AUDIT_MD = SCRIPT_DIR / "outputKraina" / "map_audit.md"
# Archiwum Kraina na Drive (wszystkie foldery Akt …)
DEFAULT_DRIVE_ROOT = "1CCXCnPv5zpVDz1Lt04Wqg2TXs9giALxj"

PRESERVE_KEYS = (
    "players",
    "dateInferred",
    "dateInference",
    "dateOriginal",
    "sortKey",
)

META_OVERWRITE_FROM_DRIVE = ("date", "location")  # tylko gdy nie chronione


def _norm_name(name: str) -> str:
    return re.sub(r"[_\s\"“”]+", " ", (name or "").lower()).strip()


def preserve_meta(old: dict, new: dict) -> dict:
    """Połącz węzeł z Drive z zachowanymi polami z poprzedniej mapy."""
    out = dict(new)
    for key in PRESERVE_KEYS:
        if key in old and old[key] is not None:
            out[key] = old[key]
    if old.get("dateInferred"):
        out["date"] = old.get("date")
        out["location"] = old.get("location") or new.get("location")
    elif old.get("date") and old["date"] not in ("brak info", "—", ""):
        if new.get("date") in ("brak info", None, "—", ""):
            out["date"] = old["date"]
        if new.get("location") in ("brak info", None, "—", ""):
            out["location"] = old.get("location") or new.get("location")
    return out


def fetch_drive_graph(root_folder_id: str) -> dict:
    from googleapiclient.discovery import build

    from gdrive_map_links import build_map, get_credentials

    creds = get_credentials(str(SCRIPT_DIR / "credentials.json"))
    drive_service = build("drive", "v3", credentials=creds)
    docs_service = build("docs", "v1", credentials=creds)
    graph = build_map(root_folder_id, drive_service, docs_service)
    return graph.to_dict()


def merge_drive_into_existing(existing: dict, drive: dict) -> tuple[dict, list[str]]:
    old_by_id = {n["id"]: n for n in existing["nodes"]}
    old_by_name = {_norm_name(n["name"]): n for n in existing["nodes"]}

    merged_nodes: list[dict] = []
    log: list[str] = []

    for node in drive["nodes"]:
        nid = node["id"]
        old = old_by_id.get(nid) or old_by_name.get(_norm_name(node["name"]))
        if old:
            merged = preserve_meta(old, node)
            if old.get("name") != node.get("name"):
                log.append(f"rename: {old['name']} → {node['name']}")
            merged_nodes.append(merged)
        else:
            log.append(f"new on Drive: {node['name']}")
            merged_nodes.append(node)

    drive_ids = {n["id"] for n in drive["nodes"]}
    for nid, old in old_by_id.items():
        if nid not in drive_ids and old.get("mimeType") != FOLDER_MIME:
            if not should_ignore(old["name"], old["mimeType"], old.get("folderChain")):
                log.append(f"orphan (tylko stara mapa): {old['name']}")
                merged_nodes.append(old)

    return {
        "generated": datetime.utcnow().isoformat() + "Z",
        "nodeCount": len(merged_nodes),
        "edgeCount": len(drive["edges"]),
        "nodes": merged_nodes,
        "edges": drive["edges"],
    }, log


def apply_local_metadata(data: dict) -> tuple[int, list[str]]:
    node_by_name = {
        n["name"]: n
        for n in data["nodes"]
        if n["mimeType"] != IS_FOLDER
    }
    unmatched: list[str] = []
    updated = 0

    for path in sorted(KRAINA_ROOT.rglob("*.docx")):
        if path.name.startswith("~$"):
            continue
        stem = path.stem
        map_name = resolve_map_name(stem, set(node_by_name))
        if not map_name:
            unmatched.append(f"{stem} ({path.parent.name})")
            continue

        doc = Document(str(path))
        header = extract_header_from_docx(doc.paragraphs)
        date, location = parse_header(stem, header)
        node = node_by_name[map_name]
        old_date, old_loc = node.get("date"), node.get("location")
        explicit_date = date not in ("brak info", "—", "", None)
        explicit_loc = location not in ("brak info", "—", "", None)

        new_date = old_date
        new_loc = old_loc

        if explicit_date:
            if node.get("dateInferred") and not node.get("dateOriginal"):
                node["dateOriginal"] = old_date or "brak info"
            if node.get("dateInferred"):
                node.pop("dateInferred", None)
                node.pop("dateInference", None)
            new_date = date
        elif not node.get("dateInferred"):
            if date != "brak info":
                new_date = date

        if explicit_loc:
            new_loc = location
        elif not node.get("dateInferred"):
            if location != "brak info":
                new_loc = location

        changed = new_date != old_date or new_loc != old_loc
        if explicit_date or explicit_loc or changed:
            node["date"] = new_date
            node["location"] = new_loc
            if explicit_date:
                sk = date_to_sortkey(new_date)
                if sk is not None:
                    node["sortKey"] = sk
            if changed or explicit_date or explicit_loc:
                updated += 1

    return updated, unmatched


def build_audit(data: dict, unmatched_local: list[str]) -> str:
    docs = [n for n in data["nodes"] if n["mimeType"] != IS_FOLDER]
    rows: list[tuple[str, str, str, str]] = []

    for n in sorted(docs, key=lambda x: (x.get("sortKey") is None, x.get("sortKey") or 0, x["name"])):
        issues = []
        date = n.get("date") or "brak info"
        loc = n.get("location") or "brak info"

        if date in ("brak info", "—", ""):
            issues.append("brak daty")
        elif n.get("dateInferred"):
            issues.append("data wywnioskowana")
        if loc in ("brak info", "—", ""):
            issues.append("brak lokacji")
        elif len(loc) < 8 and loc not in ("Warrington",):
            issues.append(f"lokacja podejrzana: {loc!r}")
        if n.get("dateInference", {}).get("note", "").startswith("konflikt"):
            issues.append("konflikt granic dat")
        if not n.get("url"):
            issues.append("brak URL Drive")

        if issues:
            rows.append((n["name"], date[:50], loc[:40], "; ".join(issues)))

    lines = [
        "# Audyt mapy Kraina",
        "",
        f"Wygenerowano: {datetime.utcnow().isoformat()}Z",
        "",
        "## Pliki lokalne bez dopasowania w mapie",
        "",
    ]
    if unmatched_local:
        for u in unmatched_local:
            lines.append(f"- {u}")
    else:
        lines.append("- (brak)")

    lines.extend(
        [
            "",
            "## Sceny z uwagami",
            "",
            "| Scena | Data | Lokacja | Problem |",
            "|-------|------|---------|---------|",
        ]
    )
    for name, date, loc, prob in rows:
        lines.append(f"| {name} | {date} | {loc} | {prob} |")

    if not rows:
        lines.append("| — | — | — | Brak zgłoszonych nieścisłości |")

    return "\n".join(lines) + "\n"


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--skip-drive", action="store_true")
    ap.add_argument("--folder", default=DEFAULT_DRIVE_ROOT)
    args = ap.parse_args()

    existing = json.loads(KRAINA_JSON.read_text(encoding="utf-8"))
    shutil.copy2(KRAINA_JSON, KRAINA_JSON.with_suffix(".json.bak"))

    if args.skip_drive:
        data = existing
        merge_log = []
    else:
        print(f"Pobieram graf z Drive ({args.folder})...")
        drive = fetch_drive_graph(args.folder)
        data, merge_log = merge_drive_into_existing(existing, drive)
        for line in merge_log:
            print(f"  {line}")

    print("Ekstrakcja z lokalnych .docx...")
    upd, unmatched = apply_local_metadata(data)
    print(f"  zaktualizowano metadane: {upd}")

    c1 = run_inference(data, anchors_only=True)
    c2 = run_inference(data, anchors_only=False)
    print(f"  infer pass1={c1} pass2={c2}")

    for node in data["nodes"]:
        if node["mimeType"] == IS_FOLDER:
            node["sortKey"] = None
            continue
        if node.get("dateInferred") and node.get("sortKey") is not None:
            continue
        sk = date_to_sortkey(node.get("date", ""))
        node["sortKey"] = sk

    KRAINA_JSON.write_text(
        json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    MD_FILE.write_text(regenerate_md(data), encoding="utf-8")
    AUDIT_MD.write_text(build_audit(data, unmatched), encoding="utf-8")
    print(f"Zapisano {KRAINA_JSON}, {MD_FILE}, {AUDIT_MD}")


if __name__ == "__main__":
    main()
