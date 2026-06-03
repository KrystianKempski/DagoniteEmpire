#!/usr/bin/env python3
"""
annotate_players.py
- Detects player characters from text colour in each .docx
- Adds "players": [...] to each node in adventure_map.json
- Inserts [CharacterName] tag before each new speaking block in source .docx files
  (tag inserted as a new black-coloured run, originals backed up first)

Usage:
  python3 annotate_players.py           # full run (backup + annotate + update JSON)
  python3 annotate_players.py --dry-run # only print detections, no file changes
"""

import json
import re
import shutil
import sys
from collections import Counter, defaultdict
from pathlib import Path

from docx import Document
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

# ── Paths ────────────────────────────────────────────────────────────────────
ROOT = Path(
    "/home/kkempski/other_repos/Dag1/DagoniteEmpire/Resources/W służbie Bonefire"
)
JSON_PATH = Path(
    "/home/kkempski/other_repos/Dag1/DagoniteEmpire"
    "/Resources/Scripts/output/adventure_map.json"
)
BACKUP_DIR = Path(
    "/home/kkempski/other_repos/Dag1/DagoniteEmpire"
    "/Resources/W służbie Bonefire_backup"
)

# ── Colour → character mapping ───────────────────────────────────────────────
# Hex values must be uppercase, no '#'
COLOR_MAP: dict[str, str | None] = {
    # Werner (pure blue + Google-blue + dark shades in his intro file)
    "0000FF": "Werner",
    "4A86E8": "Werner",
    "1155CC": "Werner",
    "A61C00": "Werner",   # dark rust in Wstęp Werner Greatwing
    "85200C": "Werner",   # dark brown-red in Wstęp Werner Greatwing
    # Lawenda
    "FF0000": "Lawenda",
    # Sariel
    "FF00FF": "Sariel",
    # Dorian (yellow/gold shades + medium-purple in Akt 1)
    "F1C232": "Dorian",
    "FFFF00": "Dorian",
    "BF9000": "Dorian",
    "674EA7": "Dorian",
    "E69138": "Dorian",
    # Tomin
    "38761D": "Tomin",
    "00FF00": "Tomin",
    # Udar (gnom)
    "B45F06": "Udar",
    "FF9900": "Udar",
    # Sharu
    "AB1C63": "Sharu",
    "45818E": "Sharu",
    "3D85C6": "Sharu",
    # Sir Bron / Umbra  (see resolve_bron_umbra)
    "9900FF": "__BRON_OR_UMBRA__",
    # Sir Cedrick (dark reds, mostly WSPÓLNE PRZYGODY)
    "660000": "Sir Cedrick",
    "990000": "Sir Cedrick",
    # Baron Mevir
    "351C75": "Baron Mevir",
    "20124D": "Baron Mevir",
    # Umbra (rogue)
    "A64D79": "Umbra",
}

# Keywords that betray Umbra (nimble rogue) vs Sir Bron (armoured knight)
UMBRA_KEYWORDS = frozenset({"umbra", "złodziej", "skrad", "zwinni", "cień", "ukry"})

TAG_RE = re.compile(r"^\s*\[[\w ]+\]")  # already has [Tag] prefix?


# ── Colour helpers ────────────────────────────────────────────────────────────

def get_run_color(run) -> str:
    """Return uppercase hex colour ('RRGGBB') or 'auto' for black/default."""
    try:
        rpr = run._r.find(qn("w:rPr"))
        if rpr is None:
            return "auto"
        color_el = rpr.find(qn("w:color"))
        if color_el is None:
            return "auto"
        val = color_el.get(qn("w:val"))
        if val is None or val.upper() in ("000000", "AUTO"):
            return "auto"
        return val.upper()
    except Exception:
        return "auto"


def para_dominant_color(para) -> str | None:
    """Return the hex colour that accounts for the most characters, or None."""
    counter: Counter = Counter()
    for run in para.runs:
        if not run.text:
            continue
        c = get_run_color(run)
        if c != "auto":
            counter[c] += len(run.text)
    if not counter:
        return None
    return counter.most_common(1)[0][0]


def resolve_character(color: str, para_text: str) -> str | None:
    """Map colour to a character name. Returns None for GM or ignored colours."""
    entry = COLOR_MAP.get(color)
    if entry is None:
        return None
    if entry == "__BRON_OR_UMBRA__":
        lower = para_text.lower()
        if any(kw in lower for kw in UMBRA_KEYWORDS):
            return "Umbra"
        return "Sir Bron"
    return entry


# ── .docx annotation ──────────────────────────────────────────────────────────

def make_tag_run(tag: str) -> "lxml.etree._Element":
    """Create a new black <w:r> element containing '[tag] '."""
    r = OxmlElement("w:r")
    # Minimal rPr – no colour set means inherit (black)
    rpr = OxmlElement("w:rPr")
    r.insert(0, rpr)
    t = OxmlElement("w:t")
    t.text = f"[{tag}] "
    t.set("{http://www.w3.org/XML/1998/namespace}space", "preserve")
    r.append(t)
    return r


def insert_tag_before_first_run(para, tag: str) -> None:
    """Insert [tag] as a new black run before the first run in para."""
    runs = para.runs
    if not runs:
        return
    first_r = runs[0]._r
    para._p.insert(list(para._p).index(first_r), make_tag_run(tag))


def annotate_document(docx_path: Path, dry_run: bool) -> set[str]:
    """
    Scan all paragraphs. When a player character starts a new speaking block
    (character changed, or resumed after GM text), insert [Name] tag.

    Returns the set of character names found in this file.
    """
    doc = Document(str(docx_path))
    prev_char: str | None = None
    characters_found: set[str] = set()
    modified = False

    for para in doc.paragraphs:
        dom_color = para_dominant_color(para)

        if dom_color is None:
            # GM / black paragraph – reset speaker context
            prev_char = None
            continue

        char = resolve_character(dom_color, para.text)

        if char is None:
            # Ignored colour – don't reset context, just skip
            continue

        characters_found.add(char)

        # Only tag at speaker transitions (including resuming after GM text)
        if char != prev_char:
            if not dry_run:
                if not TAG_RE.match(para.text):
                    insert_tag_before_first_run(para, char)
                    modified = True

        prev_char = char

    if modified:
        doc.save(str(docx_path))

    return characters_found


# ── JSON update ───────────────────────────────────────────────────────────────

def update_json(results: dict[str, set[str]]) -> None:
    with open(JSON_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)

    node_map = {node["name"]: node for node in data.get("nodes", [])}

    updated, unmatched = 0, []
    for stem, chars in results.items():
        if stem in node_map:
            node_map[stem]["players"] = sorted(chars)
            updated += 1
        else:
            unmatched.append(stem)

    with open(JSON_PATH, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"\nJSON updated: {updated} nodes")
    if unmatched:
        print(f"Unmatched (no JSON node): {unmatched}")


# ── Main ──────────────────────────────────────────────────────────────────────

def main() -> None:
    dry_run = "--dry-run" in sys.argv

    if not dry_run:
        if not BACKUP_DIR.exists():
            print(f"Creating backup at {BACKUP_DIR} …")
            shutil.copytree(str(ROOT), str(BACKUP_DIR))
            print("Backup done.\n")
        else:
            print(f"Backup already exists at {BACKUP_DIR} – skipping.\n")

    all_docx = sorted(ROOT.rglob("*.docx"))
    print(f"{'[DRY RUN] ' if dry_run else ''}Processing {len(all_docx)} files …\n")

    results: dict[str, set[str]] = {}
    for docx_path in all_docx:
        stem = docx_path.stem
        chars = annotate_document(docx_path, dry_run=dry_run)
        results[stem] = chars
        label = ", ".join(sorted(chars)) if chars else "(only GM)"
        print(f"  {stem}: {label}")

    if not dry_run:
        update_json(results)
    else:
        print("\n[DRY RUN] No files were modified.")


if __name__ == "__main__":
    main()
