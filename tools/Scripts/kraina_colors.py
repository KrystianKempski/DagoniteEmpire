#!/usr/bin/env python3
"""
Colour → hero mapping for Kraina Możliwości (.docx).

MG / narrator: black (auto / 000000) — not a player.
NPC dialogue may use other colours; only HERO_COLOR_MAP counts as players.
"""

from __future__ import annotations

from collections import Counter
from pathlib import Path

from docx.oxml.ns import qn

SCRIPT_DIR = Path(__file__).resolve().parent
KRAINA_ROOT = SCRIPT_DIR.parent / "Resources" / "Kraina Możliwości"
KRAINA_JSON = SCRIPT_DIR / "outputKraina" / "adventure_map.json"
KRAINA_BACKUP = SCRIPT_DIR.parent / "Resources" / "Kraina Możliwości_backup"

# Active & archived PCs (font colour in Google Docs / Word)
HERO_COLOR_MAP: dict[str, str] = {
    "B45F06": "Udar",
    "FF9900": "Udar",
    "38761D": "Tomin",
    "274E13": "Tomin",
    "00FF00": "Tomin",
    "0000FF": "Granit",
    "4A86E8": "Granit",
    "FF0000": "Glorio",
    "C27BA0": "Bjorn",
    "A64D79": "Bjorn",
    "980000": "Sharu",
    "990000": "Sharu",
    "CC0000": "Sharu",
    "5B0F00": "Sharu",
    "6FA8DC": "Sharu",
    "660000": "Sir Cedrick",
}

# Triaxianka = Sharu (ten sam kolor #660000 co Sir Cedrick w późniejszych aktach)
TRIAXIANKA_KEYWORDS: frozenset[str] = frozenset(
    {"triaxianka", "dertu terh", "dawhar", "musheee mojha"}
)

# Coloured NPC / formatting — do not add to `players`
NPC_OR_IGNORE_COLORS: frozenset[str] = frozenset(
    {
        "674EA7",  # Baron Mevir
        "351C75",
        "20124D",
        "1155CC",
        "0000EE",
        "E69138",
        "783F04",
        "434343",
        "85200C",
        "A61C00",
        "666666",
        "999999",
        "D5A6BD",
        "741B47",
        "E06666",
        "B7B7B7",  # narracja / myśli (np. Granit w Zamtzie) — nie osobny PC
        "6AA84F",  # pojedyncze wstawki NPC
        "1C4587",  # maskowany NPC (Akt 1.3)
        "434343",  # szary — MG / opis
    }
)


def get_run_color(run) -> str:
    """Uppercase RRGGBB or 'auto' for black / default."""
    try:
        rgb = run.font.color.rgb
        if rgb is not None:
            hx = str(rgb).upper()
            if hx in ("000000",):
                return "auto"
            return hx
    except Exception:
        pass
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
    counter: Counter[str] = Counter()
    for run in para.runs:
        if not run.text:
            continue
        c = get_run_color(run)
        if c != "auto":
            counter[c] += len(run.text)
    if not counter:
        return None
    return counter.most_common(1)[0][0]


# Local filename stem → adventure_map.json node name (Drive titles)
LOCAL_STEM_ALIASES: dict[str, str] = {
    "12.3 - Udar w łapach inkwizycji": "12.3 - Udar w łapach Inkwizycji",
    "12.5 - Obóz goblinów, Granit": "12.5 - Obóz Goblinów - Granit",
    "12.6 - Obóz goblinów, Udar i Sharu": "12.5 - Obóz Goblinów - Udar i Sharu",
    "12.7 - Przyłączenie do wyprawy na Bestię z Kanałów": (
        "12.6 - Przyłączenie się do wyprawy na Bestię z Kanałów"
    ),
    "12.8 - Niespodziewany gość": "12.7 - Niespodziewany Gość",
    "Akt 22.5 Wyprawa do _Zamtuza_": '13.6 - Wyprawa do "Zamtuza"',
    "Akt 23.4 Wszyscy w Pijanym Smoku, po południu_": "14.4 - Wszyscy w Pijanym Smoku, po południu",
    "8.11 - Wszyscy prócz Udara": "8.5 - Wszyscy prócz Udara",
    "8.16 - Sharu w niewoli": "Sharu W niewoli",
    "10.5 - Sharu i Udar, rozmowa w kuchni": "10.5 - Sharu i Udar, Rozmowa w kuchni",
    "11.2 - Walka w dormitorium": "11.1 - Walka w Dormitorium",
    "4.10 - Śledztwo U Wesołego Partianina, Udar, Bjorn, Granit": (
        '4.71 - Śledztwo "U Wesołego Partianina" Udar, Bjorn, Granit'
    ),
    "4.11 - Śledztwo U Wesołego Partianina, Tomin i Glorio": (
        '4.72 - Śledztwo "U Wesołego Partianina" Tomin, Glorio'
    ),
    "4.12 - Śledztwo U Wesołego Partianina, Tomin i Granit": (
        '4.73 - Śledztwo "U Wesołego Partianina" Tomin, Granit'
    ),
    "4.13 - Śledztwo U Wesołego Partianina, Bjorn": (
        '4.74 - Śledztwo "U Wesołego Partianina"  Bjorn'
    ),
    "4.14 - Walka w Pijanym Smoku": '4.8 - Walka w "Pijanym Smoku"',
    "4.2 - Pijany Smok": "4.1 - Pijany Smok",
    "4.3 - Plac targowy, wszyscy": "4.2 - Plac targowy, wszyscy",
    "4.5 - Plac targowy, Udar i Tomin": "4.3 - Plac targowy, Udar, Tomin",
    "4.6 - Pijany Smok, Granit, Glorio, Bjorn": "4.4 - Pijany Smok, Granit, Glorio Bjorn",
    "4.7 - Pijany Smok, wszyscy, wieczór": "4.5 - Pijany Smok, Wszyscy, wieczór",
    "4.8 - Rozmowa Udara z bratem Kwarcem": "4.6 - Rozmowa Udara z bratem Kwarcem",
    "4.9 - U Wesołego Partianina": "4.6 - U Wesołego Partianina",
    "6.2 - Poranek, Tomin i Granit": "6.1 - Poranek, Tomin i Granit",
    "6.5 - Wszyscy, Pijany Smok, po południu": "6.5 - Wszyscy, Pijany Smok, Po południu",
    "7.2 - Bjorn u mistrza Yfla": "7.2 - Bjorn idzie do pracowni mistrza Yfla",
    "8.10 - Tomin, Granit, Udar, Reila": "8.4 - Tomin, Granit Udar, Reila",
    "8.13 - Bjorn i Granit": "8.6 - Bjorn i Granit",
    "8.14 - Drużyna razem": "8.7 - Drużyna razem",
    "8.2 - Bjorn, Udar, Reila": "8.2 - Bjorn Udar Reila",
    "8.4 - Udar i Reila": "8.4 - Udar Reila",
    "8.5 - Bjorn, jaskinia handlarzy niewolników": "8.5 - Bjorn, jaskinie handlarzy niewolników",
    "8.7 - Granit i Tomin": "8.3 - Granit, Tomin",
    "4.8 - Walka w _Pijanym Smoku_": '4.8 - Walka w "Pijanym Smoku"',
    "13.6 - Wyprawa do _Zamtuza_": '13.6 - Wyprawa do "Zamtuza"',
}


def stem_map_variants(stem: str) -> list[str]:
    """Local .docx stem → possible adventure_map.json node names."""
    if stem in LOCAL_STEM_ALIASES:
        return [stem, LOCAL_STEM_ALIASES[stem]]
    cleaned = stem.rstrip("._ ")
    variants = [stem, cleaned]
    quoted = cleaned.replace("_", '"')
    if quoted not in variants:
        variants.append(quoted)
    if stem.endswith("_"):
        dot_end = stem[:-1] + "."
        if dot_end not in variants:
            variants.append(dot_end)
    # Local underscore ↔ Drive curly quotes
    if "_" in cleaned:
        alt = cleaned.replace("_", '"')
        if alt not in variants:
            variants.append(alt)
    return variants


def resolve_map_name(stem: str, node_names: set[str]) -> str | None:
    for variant in stem_map_variants(stem):
        if variant in node_names:
            return variant
    return None


def resolve_hero(color: str, para_text: str = "") -> str | None:
    if color in NPC_OR_IGNORE_COLORS:
        return None
    if color == "660000":
        lower = para_text.lower()
        if any(kw in lower for kw in TRIAXIANKA_KEYWORDS):
            return "Sharu"
    return HERO_COLOR_MAP.get(color)


def detect_heroes_in_paragraphs(paragraphs) -> set[str]:
    """Scan paragraphs; return set of hero names present (by colour)."""
    found: set[str] = set()
    for para in paragraphs:
        dom = para_dominant_color(para)
        if dom is None:
            continue
        hero = resolve_hero(dom, para.text)
        if hero:
            found.add(hero)
    return found
