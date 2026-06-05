#!/usr/bin/env python3
"""
Apply standardized Kraina Możliwości names to:
  - local .docx under tools/Resources/Kraina Możliwości
  - Google Drive files/folders (via adventure_map.json IDs)
  - adventure_map.json + adventure_map.md metadata

Usage:
    python apply_kraina_renames.py --dry-run
    python apply_kraina_renames.py --local
    python apply_kraina_renames.py --gdrive
    python apply_kraina_renames.py --local --gdrive --update-map

Requires credentials.json (OAuth). First --gdrive run may open a browser
and request Drive *modify* scope (broader than gdrive_map_links readonly).
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import time
from datetime import datetime
from pathlib import Path

from kraina_naming import (
    DOCUMENT_FOLDER_MOVES,
    FOLDER_RENAMES,
    IGNORE_FOLDER_NAMES,
    IS_FOLDER,
    apply_folder_chain,
    normalize_local_filename,
    rename_document,
    rename_folder,
    should_ignore,
)

SCRIPT_DIR = Path(__file__).resolve().parent
RESOURCES_DIR = SCRIPT_DIR.parent / "Resources" / "Kraina Możliwości"
MAP_JSON = SCRIPT_DIR / "outputKraina" / "adventure_map.json"
MAP_MD = SCRIPT_DIR / "outputKraina" / "adventure_map.md"
MANIFEST_PATH = SCRIPT_DIR / "outputKraina" / "rename_manifest.json"
CREDENTIALS_PATH = SCRIPT_DIR / "credentials.json"
TOKEN_PATH = SCRIPT_DIR / "token.json"

GDRIVE_SCOPES = ["https://www.googleapis.com/auth/drive"]


def build_manifest_from_map(data: dict, live_names: dict[str, str] | None = None) -> list[dict]:
    """Build rename operations from adventure_map.json nodes."""
    ops: list[dict] = []
    for node in data["nodes"]:
        old = live_names.get(node["id"], node["name"]) if live_names else node["name"]
        mime = node["mimeType"]
        if should_ignore(old, mime, node.get("folderChain")):
            continue
        if mime == IS_FOLDER:
            new = rename_folder(old)
            if new == old:
                continue
            ops.append({
                "id": node["id"],
                "kind": "folder",
                "oldName": old,
                "newName": new,
                "mimeType": mime,
                "folder": node.get("folder"),
            })
        else:
            new = rename_document(old, node.get("folder"))
            if not new:
                continue
            target_folder = DOCUMENT_FOLDER_MOVES.get(old)
            ops.append({
                "id": node["id"],
                "kind": "document",
                "oldName": old,
                "newName": new,
                "mimeType": mime,
                "folder": node.get("folder"),
                "targetFolder": target_folder,
                "url": node.get("url"),
            })
    return ops


def fetch_live_drive_names(service, nodes: list[dict]) -> dict[str, str]:
    """Read current file names from Google Drive (source of truth)."""
    live: dict[str, str] = {}
    for node in nodes:
        if should_ignore(node["name"], node["mimeType"], node.get("folderChain")):
            continue
        try:
            meta = service.files().get(fileId=node["id"], fields="name").execute()
            live[node["id"]] = meta["name"]
        except Exception as e:
            print(f"  WARN: nie odczytano {node['id']}: {e}")
    return live


def build_local_ops() -> list[dict]:
    """Scan Resources and build rename/move operations for .docx files."""
    ops: list[dict] = []
    if not RESOURCES_DIR.exists():
        print(f"WARN: Resources dir not found: {RESOURCES_DIR}")
        return ops

    for path in sorted(RESOURCES_DIR.rglob("*.docx")):
        if any(part in IGNORE_FOLDER_NAMES for part in path.parts):
            continue
        stem = path.stem
        new_stem = normalize_local_filename(stem, path.parent.name)
        if not new_stem:
            continue
        new_path = path.with_name(new_stem + path.suffix)
        if new_path == path:
            continue
        ops.append({
            "kind": "local_file",
            "oldPath": str(path),
            "newPath": str(new_path),
            "oldName": stem,
            "newName": new_stem,
        })

    # Folder renames (deepest first)
    folders = sorted(
        [p for p in RESOURCES_DIR.rglob("*") if p.is_dir()],
        key=lambda p: len(p.parts),
        reverse=True,
    )
    for folder in folders:
        rel = folder.relative_to(RESOURCES_DIR)
        if not rel.parts:
            continue
        if folder.name in IGNORE_FOLDER_NAMES:
            continue
        old_name = folder.name
        new_name = rename_folder(old_name)
        if new_name == old_name:
            continue
        new_folder = folder.parent / new_name
        ops.append({
            "kind": "local_folder",
            "oldPath": str(folder),
            "newPath": str(new_folder),
            "oldName": old_name,
            "newName": new_name,
        })

    # Move orphan scene into Akt 10 folder (after folder renames)
    orphan_key = "Akt 19.06 - Godzina przed północą, Pijany Smok"
    new_title = rename_document(orphan_key)
    target_dir_name = DOCUMENT_FOLDER_MOVES.get(orphan_key)
    if new_title and target_dir_name:
        target_dir = RESOURCES_DIR / rename_folder(target_dir_name)
        for candidate in (
            RESOURCES_DIR / f"{orphan_key}.docx",
            RESOURCES_DIR / "Akt 19 Przygotowanie do wyprawy na przeklęty klasztor"
            / f"{orphan_key}.docx",
        ):
            if candidate.exists():
                new_path = target_dir / f"{new_title}.docx"
                if candidate.resolve() != new_path.resolve():
                    ops.append({
                        "kind": "local_move",
                        "oldPath": str(candidate),
                        "newPath": str(new_path),
                        "oldName": candidate.name,
                        "newName": new_path.name,
                    })
                break

    return ops


def _folder_rename_order(name: str) -> int:
    """Rename higher legacy act numbers first to avoid path collisions."""
    m = re.search(r"Akt (\d+)", name)
    return -(int(m.group(1)) if m else 0)


def apply_local(ops: list[dict], dry_run: bool) -> tuple[int, int]:
    ok, skip = 0, 0
    # Files first (paths still valid), then folders, then cross-folder moves
    order = {"local_file": 0, "local_folder": 1, "local_move": 2}
    def sort_key(o):
        if o["kind"] == "local_folder":
            return (order[o["kind"]], _folder_rename_order(o["oldName"]))
        return (order.get(o["kind"], 9), 0)

    for op in sorted(ops, key=sort_key):
        old_p = Path(op["oldPath"])
        new_p = Path(op["newPath"])
        if not old_p.exists():
            print(f"  SKIP (missing): {old_p}")
            skip += 1
            continue
        if new_p.exists() and new_p != old_p:
            print(f"  SKIP (target exists): {new_p}")
            skip += 1
            continue
        action = "MOVE" if op["kind"] == "local_move" else "RENAME"
        print(f"  {action}: {old_p.name} -> {new_p}")
        if not dry_run:
            new_p.parent.mkdir(parents=True, exist_ok=True)
            old_p.rename(new_p)
        ok += 1
    return ok, skip


def get_drive_service():
    try:
        from google.auth.transport.requests import Request
        from google.oauth2.credentials import Credentials
        from google_auth_oauthlib.flow import InstalledAppFlow
        from googleapiclient.discovery import build
        from googleapiclient.errors import HttpError
    except ImportError:
        print("ERROR: pip install -r requirements_gdrive.txt")
        sys.exit(1)

    creds = None
    if TOKEN_PATH.exists():
        creds = Credentials.from_authorized_user_file(str(TOKEN_PATH), GDRIVE_SCOPES)
        if creds and not set(GDRIVE_SCOPES).issubset(set(creds.scopes or [])):
            print(
                "Token ma zbyt wąskie scope (np. tylko odczyt z gdrive_map_links). "
                "Usuwam token.json — za chwilę otworzy się przeglądarka."
            )
            TOKEN_PATH.unlink()
            creds = None

    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(
                str(CREDENTIALS_PATH), GDRIVE_SCOPES
            )
            creds = flow.run_local_server(port=0)
        TOKEN_PATH.write_text(creds.to_json(), encoding="utf-8")
        print(f"Zapisano token z uprawnieniem zapisu: {TOKEN_PATH}")

    return build("drive", "v3", credentials=creds), HttpError


def gdrive_rename(service, file_id: str, new_name: str, dry_run: bool) -> None:
    if dry_run:
        return
    service.files().update(fileId=file_id, body={"name": new_name}).execute()


def gdrive_move_to_folder(
    service, file_id: str, target_folder_name: str, folder_nodes: dict, dry_run: bool
) -> bool:
    """Move file into folder matched by old or new folder title."""
    target_id = None
    names_to_try = {target_folder_name, rename_folder(target_folder_name)}
    for node in folder_nodes.values():
        if node["mimeType"] == IS_FOLDER and node["name"] in names_to_try:
            target_id = node["id"]
            break
    if not target_id:
        print(f"    WARN: folder not found for move: {target_folder_name}")
        return False
    if dry_run:
        return True
    meta = service.files().get(fileId=file_id, fields="parents").execute()
    prev_parents = ",".join(meta.get("parents", []))
    service.files().update(
        fileId=file_id,
        addParents=target_id,
        removeParents=prev_parents or None,
        fields="id, parents",
    ).execute()
    return True


def apply_gdrive(
    gdrive_ops: list[dict],
    data: dict,
    dry_run: bool,
    service=None,
) -> tuple[int, int]:
    HttpError = None
    if service is None:
        service, HttpError = get_drive_service()
    else:
        from googleapiclient.errors import HttpError
    folder_nodes = {n["id"]: n for n in data["nodes"] if n["mimeType"] == IS_FOLDER}
    ok, err = 0, 0

    folder_ops = [o for o in gdrive_ops if o["kind"] == "folder"]
    doc_ops = [o for o in gdrive_ops if o["kind"] == "document"]

    for op in sorted(folder_ops, key=lambda o: _folder_rename_order(o["oldName"])):
        try:
            print(f"  GDRIVE folder: {op['oldName']} -> {op['newName']}")
            gdrive_rename(service, op["id"], op["newName"], dry_run)
            # Keep folder_nodes in sync for subsequent moves
            if op["id"] in folder_nodes:
                folder_nodes[op["id"]]["name"] = op["newName"]
            ok += 1
            time.sleep(0.2)
        except HttpError as e:
            print(f"    ERROR: {e}")
            err += 1

    for op in sorted(doc_ops, key=lambda o: o["oldName"]):
        try:
            print(f"  GDRIVE doc: {op['oldName']} -> {op['newName']}")
            gdrive_rename(service, op["id"], op["newName"], dry_run)
            if op.get("targetFolder"):
                print(f"    MOVE -> {op['targetFolder']}")
                gdrive_move_to_folder(
                    service, op["id"], op["targetFolder"], folder_nodes, dry_run
                )
            ok += 1
            time.sleep(0.15)
        except HttpError as e:
            print(f"    ERROR: {e}")
            err += 1
    return ok, err


def update_map_json(data: dict, dry_run: bool) -> None:
    """Refresh node names / folder paths in adventure_map.json."""
    folder_id_to_new: dict[str, str] = {}
    for node in data["nodes"]:
        if node["mimeType"] == IS_FOLDER:
            new = rename_folder(node["name"])
            if new != node["name"]:
                folder_id_to_new[node["id"]] = new
                node["name"] = new

    for node in data["nodes"]:
        if node["mimeType"] == IS_FOLDER:
            continue
        if should_ignore(node["name"], node["mimeType"], node.get("folderChain")):
            continue
        if node["name"] in DOCUMENT_FOLDER_MOVES:
            tf = DOCUMENT_FOLDER_MOVES[node["name"]]
            node["folder"] = tf
            node["folderChain"] = [tf]
        new = rename_document(node["name"], node.get("folder"))
        if new:
            node["name"] = new
        chain = node.get("folderChain", [])
        if chain:
            new_chain = apply_folder_chain(chain)
            node["folderChain"] = new_chain
            node["folder"] = new_chain[-1] if new_chain else "<root>"
        elif node.get("folder") and node["folder"] != "<root>":
            node["folder"] = rename_folder(node["folder"])

    # Fix edges names
    id_to_name = {n["id"]: n["name"] for n in data["nodes"]}
    for edge in data["edges"]:
        edge["fromName"] = id_to_name.get(edge["from"], edge.get("fromName"))
        edge["toName"] = id_to_name.get(edge["to"], edge.get("toName"))

    if dry_run:
        return
    MAP_JSON.write_text(
        json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Updated {MAP_JSON}")


def main():
    parser = argparse.ArgumentParser(description="Apply Kraina Możliwości renames")
    parser.add_argument("--dry-run", action="store_true", help="Preview only")
    parser.add_argument("--local", action="store_true", help="Rename local Resources")
    parser.add_argument("--gdrive", action="store_true", help="Rename on Google Drive")
    parser.add_argument(
        "--update-map",
        action="store_true",
        help="Update adventure_map.json names (no MD regen)",
    )
    parser.add_argument(
        "--write-manifest",
        action="store_true",
        help="Write outputKraina/rename_manifest.json",
    )
    args = parser.parse_args()

    if not args.local and not args.gdrive and not args.update_map and not args.write_manifest:
        args.dry_run = True
        args.local = True
        args.gdrive = True
        args.update_map = True
        args.write_manifest = True
        print("No flags given — running full --dry-run preview\n")

    data = json.loads(MAP_JSON.read_text(encoding="utf-8"))
    gdrive_ops: list[dict] = []
    local_ops = build_local_ops()

    if args.gdrive or args.dry_run:
        service, _ = get_drive_service()
        print("Pobieram aktualne nazwy z Google Drive…")
        live_names = fetch_live_drive_names(service, data["nodes"])
        gdrive_ops = build_manifest_from_map(data, live_names)

    manifest = {
        "generated": datetime.utcnow().isoformat() + "Z",
        "folderRenames": FOLDER_RENAMES,
        "documentRenamesCount": len(gdrive_ops),
        "gdriveOperations": gdrive_ops,
        "localOperations": local_ops,
    }

    if args.write_manifest or args.dry_run:
        MANIFEST_PATH.write_text(
            json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        print(f"Manifest: {MANIFEST_PATH}")
        print(f"  GDrive ops: {len(gdrive_ops)}")
        print(f"  Local ops:  {len(local_ops)}")

    if args.local:
        print("\n=== Local Resources ===")
        ok, skip = apply_local(local_ops, args.dry_run)
        print(f"Done: {ok} applied, {skip} skipped")

    if args.gdrive:
        print("\n=== Google Drive ===")
        if args.dry_run:
            for op in gdrive_ops[:5]:
                print(f"  would: {op['oldName']} -> {op['newName']}")
            if len(gdrive_ops) > 5:
                print(f"  ... and {len(gdrive_ops) - 5} more")
        else:
            service, HttpError = get_drive_service()
            ok, err = apply_gdrive(gdrive_ops, data, dry_run=False, service=service)
            print(f"Done: {ok} ok, {err} errors")
            if err == 0:
                update_map_json(data, dry_run=False)

    if args.update_map:
        print("\n=== adventure_map.json ===")
        update_map_json(data, dry_run=args.dry_run)
        if args.dry_run:
            renamed_docs = sum(
                1 for n in data["nodes"]
                if n["mimeType"] != IS_FOLDER and rename_document(n["name"])
            )
            print(f"  Would update names for {renamed_docs} documents")


if __name__ == "__main__":
    main()
