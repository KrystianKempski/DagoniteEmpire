#!/usr/bin/env python3
"""
assign_sort_keys.py
====================
Adds a `sortKey` integer to every document node in adventure_map.json,
then regenerates adventure_map.md with nodes sorted chronologically.

Sort key format:  MMDDHHMM  (as integer)
  MM  = month  (06 = Sarenith/June, 07 = Erastus/Erastil/July)
  DD  = day
  HH  = hour   (0-23)
  MM  = minute

Bell-hour convention (as per GM notes):
  "po Nth dzwonie" with afternoon/evening context  → hour = N + 12
  "po Nth dzwonie" with morning context            → hour = N
  "przed Nth dzwonem" subtracts ~30 min

Null sortKey is assigned to nodes whose date cannot be determined.
"""

import json
import re
from collections import defaultdict
from datetime import datetime
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
OUTPUT_DIR = SCRIPT_DIR / "outputKraina"
JSON_FILE = OUTPUT_DIR / "adventure_map.json"
MD_FILE = OUTPUT_DIR / "adventure_map.md"
WIKI_FILE = Path("/home/kkempski/other_repos/Dag1/dagonite-wiki/content/Mapy/Mapa powiązań.md")

# ---------------------------------------------------------------------------
# Ordinal bell-number words (Polish)
# ---------------------------------------------------------------------------
BELL_ORDINAL_PO = {        # "po Nth dzwonie"
    "pierwszym": 1, "drugim": 2, "trzecim": 3, "czwartym": 4,
    "piątym": 5, "szóstym": 6, "siódmym": 7, "ósmym": 8,
    "dziewiątym": 9, "dziesiątym": 10, "jedenastym": 11, "dwunastym": 12,
}
BELL_ORDINAL_PRZED = {     # "przed Nth dzwonem"
    "pierwszym": 1, "drugim": 2, "trzecim": 3, "czwartym": 4,
    "piątym": 5, "szóstym": 6, "siódmym": 7, "ósmym": 8,
    "dziewiątym": 9, "dziesiątym": 10, "jedenastym": 11, "dwunastym": 12,
}
BELL_ORDINAL_KOLO = {      # "koło Nth dzwonu/dzwona"
    "pierwszego": 1, "drugiego": 2, "trzeciego": 3, "czwartego": 4,
    "piątego": 5, "szóstego": 6, "siódmego": 7, "ósmego": 8,
    "dziewiątego": 9, "dziesiątego": 10, "jedenastego": 11, "dwunastego": 12,
}
BELL_ORDINAL_OKOLO = BELL_ORDINAL_KOLO   # "około Nth dzwonu"


def _is_morning(text: str) -> bool:
    return any(w in text for w in ("poranek", "rano", "świt", "przedświt", "porankiem"))


def _bell_to_hour(bell: int, text: str) -> int:
    """Convert bell number to 24h hour. Afternoon/evening bells are PM."""
    if _is_morning(text):
        return bell
    # Bell 1–12, afternoon/evening/night context → PM
    return bell + 12 if bell <= 12 else bell


def date_to_sortkey(date_str: str) -> int | None:
    """
    Parse a date string like '2 Erastus, po szóstym dzwonie' and return
    a sortKey integer MMDDHHMM, or None if unparseable.
    """
    if not date_str or date_str.strip().lower() in ("brak info", "—", "-", ""):
        return None

    t = date_str.lower().strip()

    # ---- Month -------------------------------------------------------
    if "sarenith" in t or "serenith" in t:
        month = 6
    elif "erastus" in t or "erastil" in t:
        month = 7
    elif "środek lata" in t or "srodek lata" in t:
        month, day = 6, 15
        hour = 22 if "późny wieczór" in t or "pozny wieczor" in t else 12
        return month * 1000000 + day * 10000 + hour * 100
    elif "późny wieczór" in t and month is None if False else False:
        pass  # handled below
    else:
        # Can't determine month — try to infer from bell/time hints only
        # e.g. "Koło trzeciego dzwonu" without a date
        month = None

    # ---- Day ---------------------------------------------------------
    day_m = re.match(r"^(\d{1,2})\s+(?:sarenith|serenith|erastus|erastil)", t)
    if day_m:
        day = int(day_m.group(1))
    elif "koniec sarenith" in t or "koniec serenith" in t:
        month = 6
        day = 30
    elif month is not None:
        # no day found → fallback to day 1
        day = 1
    else:
        return None   # can't determine either month or day

    # ---- Hour / Minute -----------------------------------------------
    hour = 12
    minute = 0

    # "po Nth dzwonie"
    m = re.search(r"\bpo\s+(\w+)\s+dzwonie\b", t)
    if m:
        bell = BELL_ORDINAL_PO.get(m.group(1))
        if bell:
            hour = _bell_to_hour(bell, t)

    # "przed Nth dzwonem"
    elif (m := re.search(r"\bprzed\s+(\w+)\s+dzwonem\b", t)):
        bell = BELL_ORDINAL_PRZED.get(m.group(1))
        if bell:
            h = _bell_to_hour(bell, t)
            hour = h - 1
            minute = 30

    # "koło/około Nth dzwonu/dzwona"
    elif (m := re.search(r"\b(?:koło|około)\s+(\w+)\s+dzwon[ua]\b", t)):
        bell = BELL_ORDINAL_KOLO.get(m.group(1))
        if bell:
            hour = _bell_to_hour(bell, t)

    # Named times (in priority order)
    elif "przedświt" in t:
        hour, minute = 4, 0
    elif "świt" in t:
        hour, minute = 5, 30
    elif "niedługo po wschodzie słońca" in t or "wkrótce po wschodzie" in t:
        hour = 7
    elif "przed południem" in t:
        hour = 10
    elif "późne popołudnie" in t or "pozne popoludnie" in t:
        hour = 17
    elif "popołudnie" in t or "popoludnie" in t:
        hour = 15
    elif "po południu" in t and "dzwonie" not in t:
        hour = 15
    elif "południe" in t or "poludnie" in t:
        hour = 12
    elif "poranek" in t or "rano" in t:
        hour = 8
    elif "kilka godzin przed zachodem słońca" in t:
        hour = 16
    elif "tuż przed zachodem słońca" in t:
        hour = 19
    elif "tuż po zachodzie słońca" in t:
        hour, minute = 20, 30
    elif "godzinę po zachodzie słońca" in t:
        hour = 21
    elif "wieczorne godziny" in t or "godziny wieczorne" in t:
        hour = 20
    elif "wieczór" in t or "wieczor" in t:
        hour = 19
    elif "wczesna noc" in t:
        hour = 22
    elif "przed północą" in t or "przed polnoca" in t:
        hour = 23
    elif "późny wieczór" in t or "pozny wieczor" in t:
        hour = 22
    elif "noc" in t:
        hour = 23
    # else: keep hour=12 (noon default)

    return month * 1000000 + day * 10000 + hour * 100 + minute


# ---------------------------------------------------------------------------
# Manual overrides for nodes that can't be parsed automatically
# (id → sortKey or None)
# ---------------------------------------------------------------------------
MANUAL_OVERRIDES: dict[str, int | None] = {
    # Wstęp Lawenda – "Środek lata" introductory scene, daytime
    "1exSSaq7FKGIlY0BMI7a-UtixQfmMSaOCc6oL60GwVKM": 6151200,
    # Wstęp Sariel – "Środek lata, późny wieczór"
    "1IVzPtCq_2UFn_ewnewuheVOwzURDmuGCAZuvVshtYE4": 6152200,
    # Przekazanie więźnia – "Późny wieczór" → 4 Erastus night (after arrest)
    "1hjW4oFDbErgVEQ6MjWGlDQVY7QOCaFTv2fM0gUZIvLk": 7042200,
    # Rozmowa w 'Ślicznotce z Haldren' – "Koło trzeciego dzwonu", 2 Erastus afternoon
    "1SVjNJ1uRUmIYcylOgZa1_mzv_qZfXCRgAADXI34VC50": 7021500,
    # Zagubiony w Kanałach – 2 Erastus, during predawn canal fight
    "11wa6ocL39XzBzRxKlFGMT179Ff9amZYRW9gbs0LR2wM": 7020430,
    # Rozmowa przed warsztatem – 2 Erastus, before noon negotiations
    "10n2N6fDcXFdiJSjv4pPQYXahbNjeHjH8s9WJdeaxwnk": 7021100,
    # Rozmowa X z Y – unknown date
    "1zB7eaLfkqL1il1MhZkeP3gla2JFH_bMvPYLR7qScfms": None,
    # Warrington_plan.jpg – image, no date
    "1U1Avp3mueF6d3ALMMSC_W6z6_r8k9m04": None,
}


# ---------------------------------------------------------------------------
# Mermaid / MD regeneration (copied from gdrive_map_links.py logic)
# ---------------------------------------------------------------------------
IS_FOLDER = "application/vnd.google-apps.folder"


def mermaid_id(file_id: str) -> str:
    return "n_" + re.sub(r"[^a-zA-Z0-9]", "_", file_id)


def sg_id(chain: tuple) -> str:
    if not chain:
        return "sg_root"
    return "sg_" + re.sub(r"[^a-zA-Z0-9]", "_", "__".join(chain))


def build_mermaid(nodes: dict, edges: list) -> str:
    all_chains: set[tuple] = set()
    folder_nodes: dict[tuple, list[str]] = defaultdict(list)
    for nid, node in nodes.items():
        if node["mimeType"] == IS_FOLDER:
            continue
        chain = tuple(node.get("folderChain", []))
        folder_nodes[chain].append(nid)
        for i in range(len(chain) + 1):
            all_chains.add(chain[:i])

    lines = ["```mermaid", "flowchart TD"]

    def emit_level(chain: tuple, indent: str) -> None:
        # Sort nodes within a folder by sortKey, then by name
        def sort_key_fn(nid: str):
            sk = nodes[nid].get("sortKey")
            return (sk is None, sk or 0, nodes[nid]["name"])

        for nid in sorted(folder_nodes.get(chain, []), key=sort_key_fn):
            node = nodes[nid]
            mid = mermaid_id(nid)
            name = node["name"].replace('"', "'")
            date = node.get("date", "brak info").replace('"', "'")
            if node.get("dateInferred"):
                date = f"~ {date}"
            location = node.get("location", "brak info").replace('"', "'")
            label = f"{name}<br/><i>{date}</i><br/><i>{location}</i>"
            url = node.get("url", "")
            lines.append(f'{indent}{mid}["{label}"]')
            if url:
                lines.append(f'{indent}click {mid} "{url}" _blank')

        sub_chains = sorted(
            c for c in all_chains
            if len(c) == len(chain) + 1 and c[:len(chain)] == chain
        )
        for sub_chain in sub_chains:
            folder_label = sub_chain[-1].replace('"', "'")
            sg = sg_id(sub_chain)
            lines.append(f'{indent}subgraph {sg}["{folder_label}"]')
            emit_level(sub_chain, indent + "  ")
            lines.append(f'{indent}end')

    emit_level((), "  ")
    lines.append("")

    for edge in edges:
        if (nodes.get(edge["from"], {}).get("mimeType") == IS_FOLDER or
                nodes.get(edge["to"], {}).get("mimeType") == IS_FOLDER):
            continue
        src = mermaid_id(edge["from"])
        dst = mermaid_id(edge["to"])
        lines.append(f"  {src} --> {dst}")

    lines.append("```")
    return "\n".join(lines)


def sortkey_to_human(sk: int | None) -> str:
    """Format sortKey as 'MM-DD HH:MM' for display."""
    if sk is None:
        return "—"
    month = sk // 1000000
    rest = sk % 1000000
    day = rest // 10000
    rest2 = rest % 10000
    hour = rest2 // 100
    minute = rest2 % 100
    month_name = {6: "Sarenith", 7: "Erastus"}.get(month, f"M{month}")
    return f"{day} {month_name} {hour:02d}:{minute:02d}"


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    data = json.loads(JSON_FILE.read_text(encoding="utf-8"))
    nodes_list = data["nodes"]
    edges = data["edges"]

    # Build dict for fast lookup
    nodes = {n["id"]: n for n in nodes_list}

    # Assign sortKey
    changed = 0
    for node in nodes_list:
        nid = node["id"]
        if nid in MANUAL_OVERRIDES:
            node["sortKey"] = MANUAL_OVERRIDES[nid]
            changed += 1
            continue
        if node["mimeType"] == IS_FOLDER:
            node["sortKey"] = None
            continue
        if node.get("dateInferred") and node.get("sortKey") is not None:
            sk = node["sortKey"]
        else:
            sk = date_to_sortkey(node.get("date", ""))
        node["sortKey"] = sk
        if sk is not None:
            changed += 1

    # Sort nodes_list: folders first (by name), then docs by sortKey
    def global_sort(n):
        is_folder = n["mimeType"] == IS_FOLDER
        sk = n.get("sortKey")
        return (not is_folder, sk is None, sk or 0, n["name"])

    nodes_list_sorted = sorted(nodes_list, key=global_sort)
    data["nodes"] = nodes_list_sorted

    # Write updated JSON
    JSON_FILE.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Updated JSON: {JSON_FILE}  ({changed} sortKeys assigned)")

    # Rebuild nodes dict (sorted)
    nodes = {n["id"]: n for n in nodes_list_sorted}

    # Build sortKey → human for table
    mermaid_str = build_mermaid(nodes, edges)

    # Node table (sorted by sortKey)
    doc_nodes_sorted = sorted(
        [n for n in nodes_list_sorted if n["mimeType"] != IS_FOLDER],
        key=lambda n: (n.get("sortKey") is None, n.get("sortKey") or 0, n["name"])
    )

    table_lines = [
        "| # | Name | Sort key | Date | Wywn. | Location | Players | Folder | URL |",
        "|---|------|----------|------|-------|----------|---------|--------|-----|",
    ]
    for i, node in enumerate(doc_nodes_sorted, 1):
        mime_short = node["mimeType"].split(".")[-1] if node["mimeType"] else "—"
        url_md = f"[link]({node['url']})" if node.get("url") else "—"
        date = node.get("date", "—")
        inferred = "tak" if node.get("dateInferred") else "—"
        location = node.get("location", "—")
        players = ", ".join(node.get("players", [])) or "—"
        sk_human = sortkey_to_human(node.get("sortKey"))
        table_lines.append(
            f"| {i} | {node['name']} | {sk_human} | {date} | {inferred} | {location} | "
            f"{players} | {node['folder']} | {url_md} |"
        )
    table_str = "\n".join(table_lines)

    generated = datetime.utcnow().isoformat() + "Z"
    node_count = sum(1 for n in nodes_list_sorted if n["mimeType"] != IS_FOLDER)
    edge_count = len(edges)

    md_body = f"""# Adventure Link Map

Generated: {generated}  
Nodes: {node_count}  
Edges: {edge_count}  

{mermaid_str}

## Node List (sorted by date)

{table_str}
"""

    MD_FILE.write_text(md_body, encoding="utf-8")
    print(f"Updated MD:   {MD_FILE}")

    wiki_content = f"""---
title: "Mapa powiązań między scenami"
tags: [mapa]
---

{md_body}"""

    WIKI_FILE.write_text(wiki_content, encoding="utf-8")
    print(f"Updated Wiki: {WIKI_FILE}")


if __name__ == "__main__":
    main()
