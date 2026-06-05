#!/usr/bin/env python3
"""
Google Drive Adventure Map Builder
====================================
Traverses a Google Drive folder recursively, extracts all Google Drive links
from Google Docs, and builds a connection map (graph) of file relationships.

Output:
  - adventure_map.json  — machine-readable graph (for agents)
  - adventure_map.md    — Mermaid diagram (for humans)

Usage:
    python gdrive_map_links.py --folder <FOLDER_ID_OR_URL>
    python gdrive_map_links.py --folder <FOLDER_ID_OR_URL> --output ./my_map --depth 5

Setup:
    1. pip install -r requirements_gdrive.txt
    2. Create a Google Cloud project, enable Drive API + Docs API
    3. Create OAuth 2.0 credentials (Desktop App), download as credentials.json
    4. Place credentials.json next to this script (or use --credentials path/to/creds.json)
    5. On first run, a browser window will open for authorization

Notes:
    - Only Google Docs (.gdoc) are scanned for links; other file types are recorded
      as nodes but their content is not parsed.
    - Links that point outside Google Drive are ignored (not added to the graph).
    - Cycles in the link graph are handled — each file is visited only once.
"""

import argparse
import json
import os
import re
import sys
import time
from collections import defaultdict, deque
from datetime import datetime
from pathlib import Path
from typing import Optional
from urllib.parse import urlparse, parse_qs

# ---------------------------------------------------------------------------
# Google API imports — installed via requirements_gdrive.txt
# ---------------------------------------------------------------------------
try:
    from google.auth.transport.requests import Request
    from google.oauth2.credentials import Credentials
    from google_auth_oauthlib.flow import InstalledAppFlow
    from googleapiclient.discovery import build
    from googleapiclient.errors import HttpError
except ImportError:
    print(
        "ERROR: Required Google API libraries are not installed.\n"
        "Run:  pip install -r requirements_gdrive.txt"
    )
    sys.exit(1)

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------
# Must match apply_kraina_renames / sync (drive write) + Docs API read
SCOPES = [
    "https://www.googleapis.com/auth/drive",
    "https://www.googleapis.com/auth/documents.readonly",
]

# Regex patterns to extract Drive file IDs from various URL forms:
#   https://docs.google.com/document/d/<ID>/...
#   https://drive.google.com/file/d/<ID>/...
#   https://drive.google.com/open?id=<ID>
GDRIVE_ID_PATTERNS = [
    re.compile(r"/(?:document|spreadsheets|presentation|file)/d/([a-zA-Z0-9_-]{25,})"),
    re.compile(r"/folders/([a-zA-Z0-9_-]{25,})"),
    re.compile(r"[?&]id=([a-zA-Z0-9_-]{25,})"),
]

TOKEN_FILE = "token.json"

from kraina_header import DATE_RE, parse_header

# ---------------------------------------------------------------------------
# Auth
# ---------------------------------------------------------------------------

def get_credentials(credentials_path: str) -> Credentials:
    """Load or refresh OAuth2 credentials, prompting browser login if needed."""
    creds = None
    token_path = Path(credentials_path).parent / TOKEN_FILE

    if token_path.exists():
        creds = Credentials.from_authorized_user_file(str(token_path), SCOPES)
        if creds and not set(SCOPES).issubset(set(creds.scopes or [])):
            print(
                "Token bez scope documents.readonly — usuwam token.json, "
                "za chwilę otworzy się przeglądarka."
            )
            token_path.unlink()
            creds = None

    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            if not Path(credentials_path).exists():
                print(
                    f"ERROR: credentials.json not found at '{credentials_path}'.\n"
                    "Download it from Google Cloud Console → APIs & Services → Credentials."
                )
                sys.exit(1)
            flow = InstalledAppFlow.from_client_secrets_file(credentials_path, SCOPES)
            creds = flow.run_local_server(port=0)

        with open(token_path, "w") as f:
            f.write(creds.to_json())
        print(f"Token saved to {token_path}")

    return creds


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def extract_file_id_from_url(url: str) -> Optional[str]:
    """Return the Drive file ID embedded in a URL, or None if not a Drive link."""
    for pattern in GDRIVE_ID_PATTERNS:
        match = pattern.search(url)
        if match:
            return match.group(1)
    return None


def folder_id_from_arg(value: str) -> str:
    """Accept either a raw folder ID or a full Drive URL and return the folder ID."""
    file_id = extract_file_id_from_url(value)
    if file_id:
        return file_id
    # Assume raw ID was passed
    return value.strip()


# ---------------------------------------------------------------------------
# Drive / Docs helpers
# ---------------------------------------------------------------------------

def list_files_in_folder(drive_service, folder_id: str) -> list[dict]:
    """Return metadata for all files directly inside *folder_id*."""
    results = []
    page_token = None
    query = f"'{folder_id}' in parents and trashed = false"

    while True:
        resp = _api_call_with_retry(
            drive_service.files().list(
                q=query,
                fields="nextPageToken, files(id, name, mimeType, webViewLink)",
                pageToken=page_token,
            )
        )
        results.extend(resp.get("files", []))
        page_token = resp.get("nextPageToken")
        if not page_token:
            break

    return results


def _api_call_with_retry(call, max_retries: int = 3, base_delay: float = 2.0):
    """Execute a Google API call with exponential backoff on transient errors."""
    for attempt in range(max_retries):
        try:
            return call.execute()
        except HttpError as e:
            if e.resp.status in (429, 500, 502, 503, 504) and attempt < max_retries - 1:
                wait = base_delay * (2 ** attempt)
                print(f"  [RETRY] HTTP {e.resp.status}, retrying in {wait:.0f}s...")
                time.sleep(wait)
            else:
                raise
        except (TimeoutError, OSError, ConnectionError) as e:
            if attempt < max_retries - 1:
                wait = base_delay * (2 ** attempt)
                print(f"  [RETRY] Network error ({type(e).__name__}), retrying in {wait:.0f}s...")
                time.sleep(wait)
            else:
                raise
    return None  # unreachable, satisfies type checkers


def extract_doc_data(docs_service, file_id: str) -> dict:
    """
    Return {'urls': [...], 'header': '...'} for a Google Doc.

    'urls' covers both regular hyperlinks (textRun.textStyle.link.url) and
    rich-link smart chips (richLink.richLinkProperties.uri), in paragraphs
    and table cells. 'header' is the concatenated text of the first few
    top-level paragraphs (used to parse date + location).
    """
    try:
        doc = _api_call_with_retry(docs_service.documents().get(documentId=file_id))
    except HttpError as e:
        print(f"  [WARN] Could not read doc {file_id}: {e}")
        return {"urls": [], "header": ""}
    except (TimeoutError, OSError, ConnectionError) as e:
        print(f"  [WARN] Network error reading doc {file_id}: {e}")
        return {"urls": [], "header": ""}
    if doc is None:
        return {"urls": [], "header": ""}

    urls: list[str] = []
    header_paragraphs: list[str] = []
    MAX_HEADER_PARAGRAPHS = 3

    def _walk_content(content_list, is_top_level: bool):
        for block in content_list:
            if "paragraph" in block:
                para_text_parts: list[str] = []
                for elem in block["paragraph"].get("elements", []):
                    tr = elem.get("textRun", {})
                    text = tr.get("content", "")
                    if text:
                        para_text_parts.append(text)
                    link = tr.get("textStyle", {}).get("link", {})
                    url = link.get("url")
                    if url:
                        urls.append(url)
                    rich_link = elem.get("richLink", {})
                    rich_url = rich_link.get("richLinkProperties", {}).get("uri")
                    if rich_url:
                        urls.append(rich_url)

                if is_top_level and len(header_paragraphs) < MAX_HEADER_PARAGRAPHS:
                    para_text = "".join(para_text_parts).strip()
                    if para_text:
                        header_paragraphs.append(para_text)

            if "table" in block:
                for row in block["table"].get("tableRows", []):
                    for cell in row.get("tableCells", []):
                        _walk_content(cell.get("content", []), False)

    _walk_content(doc.get("body", {}).get("content", []), True)
    return {"urls": urls, "header": "\n".join(header_paragraphs)}


def get_file_metadata(drive_service, file_id: str) -> Optional[dict]:
    """Fetch name, mimeType, webViewLink for a single file ID."""
    try:
        return _api_call_with_retry(
            drive_service.files().get(
                fileId=file_id,
                fields="id, name, mimeType, webViewLink",
            )
        )
    except HttpError as e:
        print(f"  [WARN] Could not fetch metadata for {file_id}: {e}")
        return None
    except (TimeoutError, OSError, ConnectionError) as e:
        print(f"  [WARN] Network error fetching {file_id}: {e}")
        return None


# ---------------------------------------------------------------------------
# Graph builder
# ---------------------------------------------------------------------------

class LinkGraph:
    """Directed graph: node = Drive file, edge = link found in source doc."""

    def __init__(self):
        self.nodes: dict[str, dict] = {}   # file_id -> {id, name, mimeType, url, folder}
        self.edges: list[dict] = []         # [{from, to, from_name, to_name}]
        self._edge_set: set[tuple] = set()

    def add_node(self, file_id: str, name: str, mime: str, web_url: str, folder_chain: tuple,
                 date: str = "brak info", location: str = "brak info"):
        if file_id not in self.nodes:
            self.nodes[file_id] = {
                "id": file_id,
                "name": name,
                "mimeType": mime,
                "url": web_url,
                "folder": folder_chain[-1] if folder_chain else "<root>",
                "folderChain": list(folder_chain),
                "date": date,
                "location": location,
            }

    def set_node_meta(self, file_id: str, date: str, location: str) -> None:
        node = self.nodes.get(file_id)
        if node is not None:
            node["date"] = date
            node["location"] = location

    def add_edge(self, from_id: str, to_id: str):
        if (from_id, to_id) not in self._edge_set and from_id != to_id:
            from_name = self.nodes.get(from_id, {}).get("name", from_id)
            to_name = self.nodes.get(to_id, {}).get("name", to_id)
            self.edges.append({
                "from": from_id,
                "to": to_id,
                "fromName": from_name,
                "toName": to_name,
            })
            self._edge_set.add((from_id, to_id))

    def to_dict(self) -> dict:
        return {
            "generated": datetime.utcnow().isoformat() + "Z",
            "nodeCount": len(self.nodes),
            "edgeCount": len(self.edges),
            "nodes": list(self.nodes.values()),
            "edges": self.edges,
        }

    @staticmethod
    def _mermaid_id(file_id: str) -> str:
        """
        Convert a Drive file ID to a valid Mermaid node identifier.
        Mermaid requires IDs to start with a letter and contain only
        alphanumerics and underscores.
        """
        return "n_" + re.sub(r"[^a-zA-Z0-9]", "_", file_id)

    def to_mermaid(self) -> str:
        """Produce a Mermaid flowchart diagram with folders as nested subgraphs."""
        IS_FOLDER = "application/vnd.google-apps.folder"

        # Group file nodes by their folder chain and collect all chain prefixes
        all_chains: set[tuple] = set()
        folder_nodes: dict[tuple, list[str]] = defaultdict(list)
        for nid, node in self.nodes.items():
            if node["mimeType"] == IS_FOLDER:
                continue  # folders become subgraphs, not boxes
            chain = tuple(node.get("folderChain", []))
            folder_nodes[chain].append(nid)
            for i in range(len(chain) + 1):
                all_chains.add(chain[:i])

        lines = ["```mermaid", "flowchart TD"]

        def _sg_id(chain: tuple) -> str:
            if not chain:
                return "sg_root"
            return "sg_" + re.sub(r"[^a-zA-Z0-9]", "_", "__".join(chain))

        def _emit_level(chain: tuple, indent: str) -> None:
            # File nodes directly inside this folder
            for nid in sorted(folder_nodes.get(chain, []),
                               key=lambda x: self.nodes[x]["name"]):
                node = self.nodes[nid]
                mid = self._mermaid_id(nid)
                name = node["name"].replace('"', "'")
                date = node.get("date", "brak info").replace('"', "'")
                location = node.get("location", "brak info").replace('"', "'")
                # Multi-line label: name on top, then date / location below
                label = f"{name}<br/><i>{date}</i><br/><i>{location}</i>"
                url = node.get("url", "")
                lines.append(f'{indent}{mid}["{label}"]')
                if url:
                    lines.append(f'{indent}click {mid} "{url}" _blank')
            # Sub-folders as nested subgraphs
            sub_chains = sorted(
                c for c in all_chains
                if len(c) == len(chain) + 1 and c[:len(chain)] == chain
            )
            for sub_chain in sub_chains:
                folder_label = sub_chain[-1].replace('"', "'")
                sg = _sg_id(sub_chain)
                lines.append(f'{indent}subgraph {sg}["{folder_label}"]')
                _emit_level(sub_chain, indent + "  ")
                lines.append(f'{indent}end')

        _emit_level((), "  ")
        lines.append("")

        # Edges — only between non-folder nodes
        for edge in self.edges:
            if (self.nodes.get(edge["from"], {}).get("mimeType") == IS_FOLDER or
                    self.nodes.get(edge["to"], {}).get("mimeType") == IS_FOLDER):
                continue
            src = self._mermaid_id(edge["from"])
            dst = self._mermaid_id(edge["to"])
            lines.append(f"  {src} --> {dst}")

        lines.append("```")
        return "\n".join(lines)


# ---------------------------------------------------------------------------
# Main traversal
# ---------------------------------------------------------------------------

def build_map(
    root_folder_id: str,
    drive_service,
    docs_service,
    max_depth: int = 10,
) -> LinkGraph:
    """
    BFS over Drive folders + link-following.

    Strategy:
      1. Enumerate all files in the folder tree (BFS by folder).
      2. For every Google Doc, extract internal Drive links.
      3. For every linked file ID not yet in the graph, fetch its metadata
         and also scan it for further links (if it's a Doc).
    """
    graph = LinkGraph()
    visited_files: set[str] = set()   # file IDs whose links have been extracted
    visited_folders: set[str] = set()

    # --- Phase 1: collect all files in the folder tree ---
    # Queue: (folder_id, folder_name, files_chain, depth)
    # files_chain = tuple of folder names from root down to this folder
    #   e.g. files in "Akt 1" have chain ("Akt 1",)
    #        files in "Akt 1" > "Sub" have chain ("Akt 1", "Sub")
    folder_queue = deque([(root_folder_id, "<root>", (), 0)])

    print(f"\n[1/2] Scanning folder tree (root: {root_folder_id})...")

    while folder_queue:
        folder_id, folder_name, files_chain, depth = folder_queue.popleft()
        if folder_id in visited_folders or depth > max_depth:
            continue
        visited_folders.add(folder_id)

        files = list_files_in_folder(drive_service, folder_id)
        print(f"  Folder '{folder_name}': {len(files)} items")

        for f in files:
            mime = f.get("mimeType", "")
            fid = f["id"]
            graph.add_node(fid, f["name"], mime, f.get("webViewLink", ""), files_chain)

            if mime == "application/vnd.google-apps.folder":
                folder_queue.append((fid, f["name"], files_chain + (f["name"],), depth + 1))

    # --- Phase 2: scan Docs for links ---
    print(f"\n[2/2] Extracting links from Google Docs...")

    # Start with all known nodes; grow the set as we discover linked files
    file_queue = deque(list(graph.nodes.keys()))

    while file_queue:
        file_id = file_queue.popleft()
        if file_id in visited_files:
            continue
        visited_files.add(file_id)

        node = graph.nodes.get(file_id, {})
        mime = node.get("mimeType", "")

        if mime != "application/vnd.google-apps.document":
            continue  # Only Docs contain extractable links

        name = node.get("name", file_id)
        print(f"  Scanning: {name}")

        doc_data = extract_doc_data(docs_service, file_id)
        date, location = parse_header(name, doc_data["header"])
        graph.set_node_meta(file_id, date, location)

        for url in doc_data["urls"]:
            linked_id = extract_file_id_from_url(url)
            if not linked_id:
                continue  # Not a Drive link — skip

            # Only follow links to files within the scanned folder tree
            if linked_id not in graph.nodes:
                continue  # Outside root folder — skip

            graph.add_edge(file_id, linked_id)

    return graph


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def debug_doc(docs_service, doc_id: str) -> None:
    """
    Dump the raw structure of every paragraph element in a document.
    Use this to discover what fields Google actually returns for smart chips,
    rich links, and other special element types.
    """
    print(f"\nFetching document: {doc_id}")
    try:
        doc = _api_call_with_retry(docs_service.documents().get(documentId=doc_id))
    except Exception as e:
        print(f"ERROR: {e}")
        return

    print(f"Title: {doc.get('title', '?')}\n")

    seen_element_keys: set[str] = set()

    def _walk(content_list, depth=0):
        indent = "  " * depth
        for i, block in enumerate(content_list):
            if "paragraph" in block:
                for j, elem in enumerate(block["paragraph"].get("elements", [])):
                    keys = set(elem.keys()) - {"startIndex", "endIndex"}
                    seen_element_keys.update(keys)
                    # Print elements that have anything beyond a plain text run
                    for key in keys:
                        if key == "textRun":
                            link = elem["textRun"].get("textStyle", {}).get("link", {})
                            url = link.get("url") or link.get("headingId") or link.get("bookmarkId")
                            if url:
                                text = elem["textRun"].get("content", "").strip()
                                print(f"{indent}[textRun link] text={repr(text[:60])} url={url}")
                        else:
                            print(f"{indent}[{key}] raw = {json.dumps(elem[key], ensure_ascii=False)[:300]}")
            if "table" in block:
                for row in block["table"].get("tableRows", []):
                    for cell in row.get("tableCells", []):
                        _walk(cell.get("content", []), depth + 1)

    _walk(doc.get("body", {}).get("content", []))
    print(f"\nAll element key types seen: {sorted(seen_element_keys)}")


def main():
    parser = argparse.ArgumentParser(
        description="Build a link-map of a Google Drive adventure folder."
    )
    parser.add_argument(
        "--folder", required=False,
        help="Root Google Drive folder ID or full URL",
    )
    parser.add_argument(
        "--debug-doc", dest="debug_doc", default=None,
        metavar="DOC_ID_OR_URL",
        help="Dump raw element structure of a single doc (for diagnosing missing links)",
    )
    parser.add_argument(
        "--output", default=".",
        help="Directory to write output files (default: current directory)",
    )
    parser.add_argument(
        "--credentials", default="credentials.json",
        help="Path to OAuth2 credentials.json (default: credentials.json)",
    )
    parser.add_argument(
        "--depth", type=int, default=10,
        help="Maximum folder recursion depth (default: 10)",
    )
    parser.add_argument(
        "--prefix", default="adventure_map",
        help="Output filename prefix (default: adventure_map)",
    )
    args = parser.parse_args()

    if not args.folder and not args.debug_doc:
        parser.error("Provide --folder or --debug-doc")

    print("Authenticating with Google...")
    creds = get_credentials(args.credentials)
    drive_service = build("drive", "v3", credentials=creds)
    docs_service = build("docs", "v1", credentials=creds)

    # --- Debug mode: inspect a single document and exit ---
    if args.debug_doc:
        doc_id = folder_id_from_arg(args.debug_doc)  # reuse ID extractor
        debug_doc(docs_service, doc_id)
        return

    folder_id = folder_id_from_arg(args.folder)
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)

    graph = build_map(folder_id, drive_service, docs_service, max_depth=args.depth)

    # --- Write JSON ---
    json_path = output_dir / f"{args.prefix}.json"
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(graph.to_dict(), f, ensure_ascii=False, indent=2)
    print(f"\nJSON map written to: {json_path}")

    # --- Write Mermaid Markdown ---
    md_path = output_dir / f"{args.prefix}.md"
    with open(md_path, "w", encoding="utf-8") as f:
        f.write(f"# Adventure Link Map\n\n")
        f.write(f"Generated: {datetime.utcnow().isoformat()}Z  \n")
        f.write(f"Nodes: {len(graph.nodes)}  \n")
        f.write(f"Edges: {len(graph.edges)}  \n\n")
        f.write(graph.to_mermaid())
        f.write("\n\n## Node List\n\n")
        f.write("| Name | Date | Location | Type | Folder | URL |\n")
        f.write("|------|------|----------|------|--------|-----|\n")
        for node in graph.nodes.values():
            mime_short = node["mimeType"].split(".")[-1] if node["mimeType"] else "—"
            url_md = f"[link]({node['url']})" if node.get("url") else "—"
            date = node.get("date", "—")
            location = node.get("location", "—")
            f.write(f"| {node['name']} | {date} | {location} | {mime_short} | {node['folder']} | {url_md} |\n")
    print(f"Mermaid map written to: {md_path}")

    # --- Summary ---
    print(f"\nDone! {len(graph.nodes)} nodes, {len(graph.edges)} edges found.")

    # Show top connected nodes
    in_degree: dict[str, int] = defaultdict(int)
    out_degree: dict[str, int] = defaultdict(int)
    for edge in graph.edges:
        out_degree[edge["from"]] += 1
        in_degree[edge["to"]] += 1

    print("\nTop 10 most-linked files (by incoming links):")
    top = sorted(in_degree.items(), key=lambda x: x[1], reverse=True)[:10]
    for fid, count in top:
        name = graph.nodes.get(fid, {}).get("name", fid)
        print(f"  [{count:3d}] {name}")


if __name__ == "__main__":
    main()
