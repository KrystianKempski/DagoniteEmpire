#!/usr/bin/env python3
"""Compute Quartz frontmatter tags for Kraina Możliwości scenes."""

from __future__ import annotations

import json
import re
from pathlib import Path

from docx import Document
from kraina_colors import KRAINA_JSON, KRAINA_ROOT, detect_heroes_in_paragraphs, resolve_map_name

SCRIPT_DIR = Path(__file__).resolve().parent
ALL_HEROES = frozenset(
    {"Udar", "Tomin", "Granit", "Sharu", "Glorio", "Bjorn", "Sir Cedrick"}
)
SLUG = {
    "Udar": "udar",
    "Tomin": "tomin",
    "Granit": "granit",
    "Sharu": "sharu",
    "Glorio": "glorio",
    "Bjorn": "bjorn",
    "Sir Cedrick": "sir-cedrick",
}


def act_number_from_path(path: Path) -> int | None:
    m = re.search(r"Akt\s+(\d+)", path.parts[-2] if len(path.parts) > 1 else path.name)
    return int(m.group(1)) if m else None


def load_map_players() -> dict[str, list[str]]:
    data = json.loads(KRAINA_JSON.read_text(encoding="utf-8"))
    out: dict[str, list[str]] = {}
    names = {n["name"] for n in data["nodes"] if n.get("mimeType") == "application/vnd.google-apps.document"}
    for n in data["nodes"]:
        if n.get("mimeType") != "application/vnd.google-apps.document":
            continue
        pl = n.get("players")
        if pl:
            out[n["name"]] = list(pl)
    return out


def players_for_docx(docx: Path, map_players: dict[str, list[str]]) -> list[str]:
    stem = docx.stem
    names = set(map_players.keys())
    key = resolve_map_name(stem, names)
    if key and map_players.get(key):
        return map_players[key]
    doc = Document(docx)
    found = detect_heroes_in_paragraphs(doc.paragraphs)
    return sorted(found)


def tags_for_scene(players: list[str], act: int | None) -> list[str]:
    base = ["archiwum", "przygoda", "kampania"]
    if act is not None:
        base.append(f"akt-{act}")
    # Sir Cedrick nie w drużynie przed Akt 12
    effective = [p for p in players if p in ALL_HEROES]
    if act is not None and act < 12:
        effective = [p for p in effective if p != "Sir Cedrick"]
    if len(effective) >= 3:
        base.append("team-pijany-smok")
    else:
        for p in effective:
            base.append(SLUG[p])
    return base


def main() -> None:
    map_players = load_map_players()
    for act_dir in sorted(KRAINA_ROOT.glob("Akt *")):
        act = act_number_from_path(act_dir)
        for docx in sorted(act_dir.glob("*.docx")):
            pl = players_for_docx(docx, map_players)
            tags = tags_for_scene(pl, act)
            print(f"{docx.stem}\t{','.join(pl)}\t{','.join(tags)}")


if __name__ == "__main__":
    main()
