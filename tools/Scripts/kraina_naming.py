#!/usr/bin/env python3
"""
Naming rules for Kraina Możliwości.

Convention:
  Akt 1 Nowi bohaterzy w Warrington — early campaign; scenes as chapters 1.1, 1.2, …
  Akt 2–14 (folders)       — legacy acts 10, 12–21 → 2–12; legacy 22→13, 23→14
  Scene files:             N.M - Opis
  Przygoda Sharu           — ignored
"""

from __future__ import annotations

import re
from typing import Optional

IS_FOLDER = "application/vnd.google-apps.folder"

IGNORE_NAME_PREFIXES = ("Sharu ", "Przygoda Sharu")
IGNORE_FOLDER_NAMES = {"Przygoda Sharu", "Archiwum"}

# Sceny poza archiwum — nie dodawać przy odświeżaniu z Drive
IGNORE_DOCUMENT_NAMES = {
    "1.0 - Dziennik zadań",
    "1.2 - Wstęp, Turniej",
    "DZIENNIK ZADAŃ",
    "00 Dziennik zadań",
    "Wstęp.Turniej",
    "00 Wstęp - Turniej",
}

# current folder title → target folder title (includes partial Drive renames)
AKT1_FOLDER = "Akt 1 Nowi bohaterzy w Warrington"

FOLDER_RENAMES: dict[str, str] = {
    "Akt 01-11 Warrington": AKT1_FOLDER,
    "Akt 1": AKT1_FOLDER,
    "Akt 10 Rozdzielona drużyna": "Akt 2 Rozdzielona drużyna",
    "Akt 12 Dzień 1 turniej bohaterów": "Akt 3 Turniej bohaterów",
    "Akt 12 Turniej bohaterów": "Akt 3 Turniej bohaterów",
    "Akt 13 Nowi wrogowie": "Akt 4 Nowi wrogowie",
    "Akt 14 Nowe zadania": "Akt 5 Nowe zadania",
    "Akt 15 Polowanie na nić": "Akt 6 Polowanie na nić",
    "Akt 16": "Akt 7 Przed jaskinią",
    "Akt 16 Przed jaskinią": "Akt 7 Przed jaskinią",
    "Akt 17 W jaskini handlarzy niewolników": "Akt 8 W jaskini handlarzy niewolników",
    "Akt 18 Odpoczynek po jaskini niewolników": "Akt 9 Odpoczynek po jaskini niewolników",
    "Akt 19 Przygotowanie do wyprawy na przeklęty klasztor": (
        "Akt 10 Przygotowanie do wyprawy na przeklęty klasztor"
    ),
    "Akt 20 Przeklęty Klasztor": "Akt 11 Przeklęty Klasztor",
    "Akt 21 Bestia z Kanałów": "Akt 12 Bestia z Kanałów",
    "Akt 13": "Akt 13 Ponownie rozdzielona drużyna",
    "Akt 14": "Akt 14 Drużyna się\u00a0zbiera",
}

LEGACY_ACT_TO_FOLDER: dict[int, int] = {
    10: 2,
    12: 3,
    13: 4,
    14: 5,
    15: 6,
    16: 7,
    17: 8,
    18: 9,
    19: 10,
    20: 11,
    21: 12,
}

# All known source titles → final title (raw Drive, intermediate pass, local)
DOCUMENT_ALIASES: dict[str, str] = {
    # --- Akt 1 chapters (raw + intermediate → 1.x) ---
    "Wstęp Tomin Basind": "1.1 - Wstęp, Tomin Basind",
    "00 Wstęp - Tomin Basind": "1.1 - Wstęp, Tomin Basind",
    "Wstęp.Turniej": "1.2 - Wstęp, Turniej",
    "00 Wstęp - Turniej": "1.2 - Wstęp, Turniej",
    "DZIENNIK ZADAŃ": "1.0 - Dziennik zadań",
    "00 Dziennik zadań": "1.0 - Dziennik zadań",
    "Akt 1 Masakra Pod Czerwonym Bębnem": "1.3 - Masakra pod Czerwonym Bębnem",
    "Akt 01 - Masakra pod Czerwonym Bębnem": "1.3 - Masakra pod Czerwonym Bębnem",
    "Akt 2 Droga do Warrington": "1.4 - Droga do Warrington",
    "Akt 02 - Droga do Warrington": "1.4 - Droga do Warrington",
    "Akt 3 Uczta w Wesołym Partianinie": "1.5 - Uczta w Wesołym Partianinie",
    "Akt 03 - Uczta w Wesołym Partianinie": "1.5 - Uczta w Wesołym Partianinie",
    "Akt 4 Czarne skrzydła": "1.6 - Czarne skrzydła",
    "Akt 04 - Czarne skrzydła": "1.6 - Czarne skrzydła",
    "Akt 5 Bitwa pod murami": "1.7 - Bitwa pod murami",
    "Akt 05 - Bitwa pod murami": "1.7 - Bitwa pod murami",
    'Akt 6 Narada w "Pijanym Smoku"': "1.8 - Narada w Pijanym Smoku",
    "Akt 06 - Narada w Pijanym Smoku": "1.8 - Narada w Pijanym Smoku",
    "Akt 7 Ponownie w Starym Młynie": "1.9 - Ponownie w Starym Młynie",
    "Akt 07 - Ponownie w Starym Młynie": "1.9 - Ponownie w Starym Młynie",
    "Akt 8 Pułapka na drodze do Warrington": "1.10 - Pułapka na drodze do Warrington",
    "Akt 08 - Pułapka na drodze do Warrington": "1.10 - Pułapka na drodze do Warrington",
    "Bjorn i Glorio": "1.11 - Bjorn i Glorio",
    "Akt 08 - Bjorn i Glorio": "1.11 - Bjorn i Glorio",
    "Akt 9 Poszukiwania w Warrington": "1.12 - Poszukiwania w Warrington",
    "Akt 09 - Poszukiwania w Warrington": "1.12 - Poszukiwania w Warrington",
    "Akt 11 Zwiedzanie Kojca": "1.13 - Zwiedzanie Kojca",
    "Akt 11 - Zwiedzanie Kojca": "1.13 - Zwiedzanie Kojca",
    # --- Akt 2 (legacy 10) ---
    "Akt 10 Poszukiwania w mieście": "2.1 - Poszukiwania w mieście",
    "Akt 10.01 - Poszukiwania w mieście": "2.1 - Poszukiwania w mieście",
    "21 Serenith Bjorn": "2.2 - Bjorn",
    "Akt 10.02 - Bjorn": "2.2 - Bjorn",
    "21 Serenith Udar i Glorio - dalsze przygody": "2.3 - Udar i Glorio",
    "Akt 10.03 - Udar i Glorio": "2.3 - Udar i Glorio",
    "21 Serenith Tomin i Granit - dalsze przygody": "2.4 - Tomin i Granit",
    "Akt 10.04 - Tomin i Granit": "2.4 - Tomin i Granit",
    "21 Serenith Glorio i Bjorn, dalsze przygody": "2.5 - Glorio i Bjorn",
    "Akt 10.05 - Glorio i Bjorn": "2.5 - Glorio i Bjorn",
    "21 Serenith Dalsze przygody Glorio, Bjorn, Granit, Tomin": (
        "2.6 - Glorio, Bjorn, Granit, Tomin"
    ),
    "Akt 10.06 - Glorio, Bjorn, Granit, Tomin": "2.6 - Glorio, Bjorn, Granit, Tomin",
    "21 Serenith Glorio": "2.7 - Glorio",
    "Akt 10.07 - Glorio": "2.7 - Glorio",
    "21 Serenit Glorio i Tomin": "2.8 - Glorio i Tomin",
    "Akt 10.08 - Glorio i Tomin": "2.8 - Glorio i Tomin",
    "21 Serenith Tomin, Granit Bjorn": "2.9 - Tomin, Granit, Bjorn",
    "Akt 10.09 - Tomin, Granit, Bjorn": "2.9 - Tomin, Granit, Bjorn",
    "21 Serenith. Tomin, Glorio, Udar, Granit. ": "2.10 - Tomin, Glorio, Udar, Granit",
    "21 Serenith. Tomin, Glorio, Udar, Granit._": "2.10 - Tomin, Glorio, Udar, Granit",
    "Akt 10.10 - Tomin, Glorio, Udar, Granit": "2.10 - Tomin, Glorio, Udar, Granit",
    "21 Serenith Znowu wszyscy razem": "2.11 - Znowu wszyscy razem",
    "Akt 10.11 - Znowu wszyscy razem": "2.11 - Znowu wszyscy razem",
    # --- Akt 3 (legacy 12) ---
    "Akt 12 Dzień Turnieju": "3.1 - Dzień turnieju",
    "Akt 12.01 - Dzień turnieju": "3.1 - Dzień turnieju",
    "Akt 12.1 Zmagania turniejowe": "3.2 - Zmagania turniejowe",
    "Akt 12.02 - Zmagania turniejowe": "3.2 - Zmagania turniejowe",
    "Akt 12.2.1 Dzień turnieju Tomin": "3.3 - Dzień turnieju, Tomin",
    "Akt 12.03 - Dzień turnieju, Tomin": "3.3 - Dzień turnieju, Tomin",
    "Akt 12.2.2 Dzień turnieju Granit": "3.4 - Dzień turnieju, Granit",
    "Akt 12.04 - Dzień turnieju, Granit": "3.4 - Dzień turnieju, Granit",
    "Akt 12.3 Pijany smok": "3.5 - Pijany Smok",
    "Akt 12.05 - Pijany Smok": "3.5 - Pijany Smok",
    "Akt 12.3.1 Bjorn i Reila": "3.6 - Bjorn i Reila",
    "Akt 12.06 - Bjorn i Reila": "3.6 - Bjorn i Reila",
    "Akt 12.4 Bjorn i Glorio - odwiedziny w zamtuzie": "3.7 - Bjorn i Glorio, odwiedziny w zamku",
    "Akt 12.07 - Bjorn i Glorio, odwiedziny w zamku": "3.7 - Bjorn i Glorio, odwiedziny w zamku",
    "Akt 12.4.1 Udar": "3.8 - Udar",
    "Akt 12.08 - Udar": "3.8 - Udar",
    "Akt 12.4.2 Granit": "3.9 - Granit",
    "Akt 12.09 - Granit": "3.9 - Granit",
    "Akt 12.4.3 TURNIEJ pojedynki wręcz": "3.10 - Turniej, pojedynki wręcz",
    "Akt 12.10 - Turniej, pojedynki wręcz": "3.10 - Turniej, pojedynki wręcz",
    "Akt 12.4.3 Udar i Granit ": "3.11 - Udar i Granit",
    "Akt 12.4.3 Udar i Granit_": "3.11 - Udar i Granit",
    "Akt 12.11 - Udar i Granit": "3.11 - Udar i Granit",
    "Akt 12.5 Udar po raz drugi": "3.12 - Udar po raz drugi",
    "Akt 12.12 - Udar po raz drugi": "3.12 - Udar po raz drugi",
    "Akt 12.6 Pijany Smok": "3.13 - Pijany Smok",
    "Akt 12.13 - Pijany Smok": "3.13 - Pijany Smok",
    "Akt 12.7. Wilcza Brama": "3.14 - Wilcza Brama",
    "Akt 12.14 - Wilcza Brama": "3.14 - Wilcza Brama",
    "Akt 12.8 Pręgi": "3.15 - Pręgi",
    "Akt 12.15 - Pręgi": "3.15 - Pręgi",
    # --- Akt 4–12: generated via regex fallback; key raw rows below ---
    "Alt 13.4 Tomin Rozmowa w cztery oczy ": "4.4alt - Tomin, rozmowa w cztery oczy",
    "Alt 13.4 Tomin Rozmowa w cztery oczy_": "4.4alt - Tomin, rozmowa w cztery oczy",
    "Akt 14 Tomin i Bjorn": "5.1 - Tomin i Bjorn",
    "Akt 14. Zakupy Udara": "5.2 - Zakupy Udara",
    "Akt 14 Tomin, Udar, Granit": "5.3 - Tomin, Udar, Granit",
    "Akt 14.3 Bjorn, noc": "5.4 - Bjorn, noc",
    "Akt 14. Wszyscy port": "5.5 - Wszyscy, port",
    "Akt 15. Wszyscy Początek dnia": "6.1 - Wszyscy, początek dnia",
    "Akt 16 Poranek w Pijanym Smoku": "7.1 - Poranek w Pijanym Smoku",
    "Akt 17 Poranek w Pijanym Smoku": "8.1 - Poranek w Pijanym Smoku",
    "Akt 19 Pijany Smok, wszyscy": "10.1 - Pijany Smok, wszyscy",
    "Akt 20 Nieumarłe opactwo": "11.1 - Nieumarłe opactwo",
    "Akt 21 Dzień zapłaty": "12.1 - Dzień zapłaty",
    "17.8 Wszyscy. Przystań": "8.15 - Wszyscy, Przystań",
    "Sharu W niewoli": "8.16 - Sharu w niewoli",
    "29 Serenith. Godzina przed północą, Karczma Pijany Smok": (
        "10.6 - Godzina przed północą, Pijany Smok"
    ),
    "Akt 19.06 - Godzina przed północą, Pijany Smok": (
        "10.6 - Godzina przed północą, Pijany Smok"
    ),
    "W ciemności Tomin": "11.3 - W ciemności, Tomin",
    "W ciemności Granit": "11.4 - W ciemności, Granit",
    "W ciemności Bjorn": "11.5 - W ciemności, Bjorn",
    "W ciemności Sharu": "11.6 - W ciemności, Sharu",
    "W ciemności Ostatnia bitwa": "11.7 - W ciemności, ostatnia bitwa",
    # --- Akt 13 (legacy Drive act 22) ---
    "Akt 22 Wszyscy, przed Warsztatem lorda Mevira": (
        "13.1 - Wszyscy, przed Warsztatem lorda Mevira"
    ),
    "Akt 22.1 Udar, Plac Targowy": "13.2 - Udar, Plac Targowy",
    "Akt 22.2 Wszyscy prócz Udara, Warsztat lady Emely": (
        "13.3 - Wszyscy prócz Udara, Warsztat lady Emely"
    ),
    "Akt 22.3 Tomin, wizyta w Zamku Margrabiego": (
        "13.4 - Tomin, wizyta w Zamku Margrabiego"
    ),
    "Akt 22.4 Granit": "13.5 - Granit",
    'Akt 22.5 Wyprawa do "Zamtuza"': '13.6 - Wyprawa do "Zamtuza"',
    "Akt 22.5 Wyprawa do _Zamtuza_": '13.6 - Wyprawa do "Zamtuza"',
    "Akt 22.6 Tomin i  Udar po wyjściu z Zamtuza": (
        "13.7 - Tomin i Udar po wyjściu z Zamtuza"
    ),
    "Akt 22.7 Granit sam w Zamtuzie  i na Skundlonej Arenie": (
        "13.8 - Granit sam w Zamtuzie i na Skundlonej Arenie"
    ),
    "Akt 22.8 Skundlona Arena": "13.9 - Skundlona Arena",
    "Spotkanie w Pijanym Smoku i podróż do Kojca": (
        "13.10 - Spotkanie w Pijanym Smoku i podróż do Kojca"
    ),
    # --- Akt 14 (legacy Drive act 23) ---
    "Akt 23 Sir Cedrick o poranku": "14.1 - Sir Cedrick o poranku",
    "Akt 23.2 Poranek Udara i Reili": "14.2 - Poranek Udara i Reili",
    "Akt 23.3 Granit i Tomin, Sir Cedrick w Klasztorze Irori": (
        "14.3 - Granit i Tomin, Sir Cedrick w Klasztorze Irori"
    ),
    "Akt 23.4 Wszyscy w Pijanym Smoku, po południu.": (
        "14.4 - Wszyscy w Pijanym Smoku, po południu"
    ),
    "Akt 23.4 Wszyscy w Pijanym Smoku, po południu_": (
        "14.4 - Wszyscy w Pijanym Smoku, po południu"
    ),
    "Akt23.5 Sir Cedrick w Zakonie": "14.5 - Sir Cedrick w Zakonie",
    "Akt 23.5 Sir Cedrick w Zakonie": "14.5 - Sir Cedrick w Zakonie",
}

DOCUMENT_FOLDER_MOVES: dict[str, str] = {
    "29 Serenith. Godzina przed północą, Karczma Pijany Smok": (
        "Akt 10 Przygotowanie do wyprawy na przeklęty klasztor"
    ),
    "Akt 19.06 - Godzina przed północą, Pijany Smok": (
        "Akt 10 Przygotowanie do wyprawy na przeklęty klasztor"
    ),
}

_SCENE_DASHED_RE = re.compile(
    r"^Akt (\d+)\.(\d+)(alt)?\s*-\s*(.+)$",
    re.IGNORECASE,
)
_SCENE_PLAIN_RE = re.compile(
    r"^Akt (\d+)\.(\d+)(?:\.(\d+))?\s+(.+)$",
    re.IGNORECASE,
)
_SCENE_ACT_ONLY_RE = re.compile(r"^Akt (\d+)\s+(.+)$", re.IGNORECASE)
_SCENE_ACT_DOT_TITLE_RE = re.compile(r"^Akt (\d+)\.\s+(.+)$", re.IGNORECASE)
# Intermediate pass: "12.03 - Title" inside folder that should be act 3
_INTERMEDIATE_RE = re.compile(r"^(\d+)\.(\d+)(alt)?\s*-\s*(.+)$", re.IGNORECASE)

# Target act number for each folder (after folder rename)
FOLDER_ACT_NUMBER: dict[str, int] = {
    AKT1_FOLDER: 1,
    "Akt 1": 1,
    "Akt 2 Rozdzielona drużyna": 2,
    "Akt 3 Turniej bohaterów": 3,
    "Akt 4 Nowi wrogowie": 4,
    "Akt 5 Nowe zadania": 5,
    "Akt 6 Polowanie na nić": 6,
    "Akt 7 Przed jaskinią": 7,
    "Akt 8 W jaskini handlarzy niewolników": 8,
    "Akt 9 Odpoczynek po jaskini niewolników": 9,
    "Akt 10 Przygotowanie do wyprawy na przeklęty klasztor": 10,
    "Akt 11 Przeklęty Klasztor": 11,
    "Akt 12 Bestia z Kanałów": 12,
    "Akt 13": 13,
    "Akt 13 Ponownie rozdzielona drużyna": 13,
    "Akt 14": 14,
    "Akt 14 Drużyna się zbiera": 14,
    "Akt 14 Drużyna się\u00a0zbiera": 14,
}

# Map legacy multi-part scene ids (e.g. 12.2.1) → new scene index
_MULTI_SCENE_INDEX: dict[tuple[int, str], int] = {
    (12, "2.1"): 3,
    (12, "2.2"): 4,
    (12, "3.1"): 6,
    (12, "4.1"): 8,
    (12, "4.2"): 9,
    (12, "4.3"): 10,
    (17, "2.1"): 3,
    (17, "2.2"): 4,
    (17, "2.3"): 5,
    (17, "2.4"): 6,
    (17, "3.1"): 8,
    (17, "3.2"): 9,
    (17, "5.1"): 12,
}


def should_ignore(name: str, mime_type: str, folder_chain: list[str] | None = None) -> bool:
    if name in IGNORE_FOLDER_NAMES or name in IGNORE_DOCUMENT_NAMES:
        return True
    if name.startswith("Sharu ") and folder_chain and any(
        p.startswith("Akt ") for p in folder_chain
    ):
        # Sceny w aktach (np. „Sharu W niewoli” w Akt 8) — nie boczna Przygoda Sharu
        pass
    elif any(name.startswith(p) for p in IGNORE_NAME_PREFIXES):
        return True
    if folder_chain and any(p in IGNORE_FOLDER_NAMES for p in folder_chain):
        return True
    if mime_type != IS_FOLDER and folder_chain and folder_chain[0] == "Przygoda Sharu":
        return True
    return False


def _lookup_alias(name: str) -> Optional[str]:
    for key in (name, name.strip(), name.rstrip("._ ")):
        if key in DOCUMENT_ALIASES:
            new = DOCUMENT_ALIASES[key]
            return new if new != key else None
    return None


def _renumber_scene(act: int, sub: str, alt: str, title: str) -> Optional[str]:
    new_act = LEGACY_ACT_TO_FOLDER.get(act)
    if new_act is None:
        return None
    sub_key = sub
    if "." in sub:
        idx = _MULTI_SCENE_INDEX.get((act, sub))
        if idx is not None:
            sub_int = idx
        else:
            parts = sub.split(".")
            sub_int = int(parts[0]) * 10 + int(parts[1]) if len(parts) >= 2 else int(parts[0])
    else:
        sub_int = int(sub)
    suffix = alt or ""
    return f"{new_act}.{sub_int}{suffix} - {title.strip()}"


def _fix_intermediate_numbering(name: str, folder: str | None = None) -> Optional[str]:
    """Map wrong intermediate act numbers (e.g. 12.1 → 3.1 in turniej folder)."""
    m = _INTERMEDIATE_RE.match(name.strip())
    if not m:
        return None
    act, sub, alt, title = int(m.group(1)), m.group(2), m.group(3) or "", m.group(4)
    folder_key = rename_folder(folder) if folder and folder != "<root>" else None
    expected = FOLDER_ACT_NUMBER.get(folder_key or "")
    if expected is not None and act == expected:
        return None
    if act in LEGACY_ACT_TO_FOLDER:
        new = _renumber_scene(act, sub, alt, title)
        return new if new and new != name else None
    return None


def rename_document(old: str, folder: str | None = None) -> Optional[str]:
    if should_ignore(old, "application/vnd.google-apps.document"):
        return None

    alias = _lookup_alias(old)
    if alias:
        return alias

    fixed = _fix_intermediate_numbering(old, folder)
    if fixed:
        return fixed

    stripped = old.strip()
    m = _SCENE_DASHED_RE.match(stripped)
    if m:
        new = _renumber_scene(
            int(m.group(1)), m.group(2), m.group(3) or "", m.group(4)
        )
        if new and new != old:
            return new

    m = _SCENE_PLAIN_RE.match(stripped)
    if m:
        sub = m.group(2)
        if m.group(3):
            sub = f"{sub}.{m.group(3)}"
        new = _renumber_scene(int(m.group(1)), sub, "", m.group(4))
        if new and new != old:
            return new

    m = _SCENE_ACT_DOT_TITLE_RE.match(stripped)
    if m:
        new = _renumber_scene(int(m.group(1)), "1", "", m.group(2))
        if new and new != old:
            return new

    m = _SCENE_ACT_ONLY_RE.match(stripped)
    if m:
        new = _renumber_scene(int(m.group(1)), "1", "", m.group(2))
        if new and new != old:
            return new

    return None


def rename_folder(old: str) -> str:
    seen: set[str] = set()
    current = old
    while current in FOLDER_RENAMES and current not in seen:
        seen.add(current)
        current = FOLDER_RENAMES[current]
    return current


def apply_folder_chain(chain: list[str]) -> list[str]:
    return [rename_folder(f) for f in chain]


def normalize_local_filename(stem: str, folder: str | None = None) -> Optional[str]:
    cleaned = stem.rstrip("._ ")
    for candidate in (stem, cleaned, stem.replace("_", '"'), cleaned.replace("_", '"')):
        new = rename_document(candidate, folder)
        if new:
            return new
    return None
