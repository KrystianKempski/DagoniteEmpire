#!/usr/bin/env python3
"""
Upload local Kraina Możliwości .docx files to matching Google Docs on Drive.

Matches files by stem ↔ adventure_map.json node name (same IDs as renames).
Requires OAuth with Drive write scope (shared with apply_kraina_renames.py).

Usage:
    python3 sync_kraina_to_gdrive.py --dry-run
    python3 sync_kraina_to_gdrive.py
    python3 sync_kraina_to_gdrive.py --only "8.6 - Udar"
"""

from __future__ import annotations

import argparse
import json
import sys
import time
from pathlib import Path

from apply_kraina_renames import (
    MAP_JSON,
    RESOURCES_DIR,
    get_drive_service,
)
from kraina_colors import resolve_map_name
from kraina_naming import IS_FOLDER, should_ignore

DOCX_MIME = (
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
)
GOOGLE_DOC = "application/vnd.google-apps.document"


def build_name_to_id(data: dict) -> dict[str, str]:
    """Map document title → Drive file id."""
    out: dict[str, str] = {}
    for node in data["nodes"]:
        if node["mimeType"] == IS_FOLDER:
            continue
        if should_ignore(node["name"], node["mimeType"], node.get("folderChain")):
            continue
        if node["mimeType"] != GOOGLE_DOC:
            continue
        out[node["name"]] = node["id"]
    return out


def find_local_docx(only: str | None) -> list[Path]:
    paths = sorted(RESOURCES_DIR.rglob("*.docx"))
    if only:
        needle = only.lower().strip()
        paths = [
            p
            for p in paths
            if needle in p.stem.lower() or needle in str(p).lower()
        ]
    return paths


def upload_docx(service, file_id: str, local_path: Path, dry_run: bool) -> None:
    from googleapiclient.http import MediaFileUpload

    if dry_run:
        return
    media = MediaFileUpload(str(local_path), mimetype=DOCX_MIME, resumable=True)
    service.files().update(
        fileId=file_id,
        media_body=media,
        supportsAllDrives=True,
    ).execute()


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Sync local Kraina .docx content to Google Docs on Drive"
    )
    parser.add_argument("--dry-run", action="store_true", help="Preview only")
    parser.add_argument(
        "--only",
        metavar="NAME",
        help="Substring filter on file name (e.g. '8.6 - Udar')",
    )
    parser.add_argument(
        "--sleep",
        type=float,
        default=0.25,
        help="Seconds between uploads (rate limit)",
    )
    args = parser.parse_args()

    if not RESOURCES_DIR.is_dir():
        sys.exit(f"Brak folderu: {RESOURCES_DIR}")
    if not MAP_JSON.is_file():
        sys.exit(f"Brak mapy: {MAP_JSON}")

    data = json.loads(MAP_JSON.read_text(encoding="utf-8"))
    name_to_id = build_name_to_id(data)
    node_names = set(name_to_id)
    local_files = find_local_docx(args.only)

    if not local_files:
        sys.exit("Brak plików .docx do synchronizacji.")

    service, HttpError = get_drive_service()

    ok, skip, err = 0, 0, 0
    prefix = "[DRY RUN] " if args.dry_run else ""
    print(f"{prefix}Synchronizacja {len(local_files)} plików → Google Drive\n")

    for path in local_files:
        stem = path.stem
        map_name = resolve_map_name(stem, node_names)
        file_id = name_to_id.get(map_name) if map_name else None
        if not file_id:
            print(f"  SKIP (brak w mapie): {stem}")
            skip += 1
            continue
        try:
            label = stem if map_name == stem else f"{stem} → {map_name}"
            print(f"  UPLOAD: {label}")
            upload_docx(service, file_id, path, args.dry_run)
            ok += 1
            if not args.dry_run:
                time.sleep(args.sleep)
        except HttpError as e:
            print(f"    ERROR: {e}")
            err += 1

    print(f"\nDone: {ok} uploaded, {skip} skipped, {err} errors")
    if err:
        sys.exit(1)


if __name__ == "__main__":
    main()
