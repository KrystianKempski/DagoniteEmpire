#!/usr/bin/env python3
"""
Parse date + location from Kraina scene filenames and document header lines.
Shared by gdrive_map_links.py and extract_kraina_metadata.py.
"""

from __future__ import annotations

import re

MONTH_NAMES = [
    "Abadius",
    "Calistril", "Kalistril",
    "Pharast", "Faraskt", "Pharasma",
    "Gozran",
    "Desnus",
    "Sarenith", "Serenith",
    "Erastus", "Erastil",
    "Arodus",
    "Rova",
    "Lamashan", "Lamasht",
    "Neth",
    "Kuthona",
]

_MONTH_ALT = "|".join(sorted(MONTH_NAMES, key=len, reverse=True))
DATE_RE = re.compile(rf"\b(\d{{1,2}})?\s*({_MONTH_ALT})\b", re.IGNORECASE)

TIME_HINT_WORDS = re.compile(
    r"\b(rano|poranek|przedpołudnie|popołudnie|po\s*południu|południe|"
    r"wiecz[oó]r\w*|noc\w*|p[oó]łnoc\w*|długo|"
    r"dzwon\w*|godzin\w*|świt\w*|zmierzch\w*|p[oó]źnym|wczesnym|następn\w+|"
    r"tuż|przed|po|koło|około|moonday|wczesny|piąta)\b",
    re.IGNORECASE,
)

TRAILING_TIME_RE = re.compile(
    r"[.,;\s-]+(po|koło|przed|około)\s+\w+\s+(dzwonie|dzwonu|dzwonem|dzwona)\b.*$",
    re.IGNORECASE,
)

MAX_LOCATION_LEN = 80

BELL_WORDS = (
    "pierwszym", "drugim", "trzecim", "czwartym", "piątym", "szóstym", "siódmym",
    "ósmym", "dziewiątym", "dziesiątym", "jedenastym", "dwunastym",
    "pierwszego", "drugiego", "trzeciego", "czwartego", "piątego", "szóstego",
    "siódmego", "ósmego", "dziewiątego", "dziesiątego", "jedenastego", "dwunastego",
)

BELL_ONLY_RE = re.compile(
    rf"^(?:po|przed|koło|około|tuż)?\s*(?:{'|'.join(BELL_WORDS)})\s*"
    r"(?:dzwon\w*)?\s*\.?\s*$",
    re.IGNORECASE,
)

LOCATION_KEYWORDS = re.compile(
    r"\b(karczma|plac|warsztat|dzielnica|pracownia|port|klasztor|siedziba|"
    r"dormitorium|jaskin\w*|kanał\w*|pagórek|smok|partianin|targow\w*|kupieck\w*|"
    r"świątyn\w*|wysypisko|warrington|imperial\w*|mevira|yfla|bęben|mur\w*|"
    r"podwórze|dębow\w*|rzemieślnic\w*|świątynn\w*|oktaw\w*|błotnist\w*|"
    r"handlarz\w*|niewolnik\w*|kanałach|loch\w*|komnat\w*|sala|wieża|ogród)\b",
    re.IGNORECASE,
)

PLACE_PREFIX_RE = re.compile(
    r"^(pod|przed|w|na|u|do|za|od|między|obok|koło)\s+",
    re.IGNORECASE,
)

TAG_RE = re.compile(r"^\s*\[[\w ]+\]")


def _split_segments(text: str) -> list[str]:
    """Split header remainder on commas; break on '. ' before a new place phrase."""
    segments: list[str] = []
    for comma_part in text.split(","):
        chunk = comma_part.strip(" \t\n\r—-“”\"';:")
        if not chunk:
            continue
        if re.search(r"\.\s+(?=[A-ZĄĆĘŁŃÓŚŹŻJ]|jaskin|karczma|plac|pod|przed|w\s)", chunk, re.I):
            for sub in re.split(r"\.\s+", chunk):
                sub = sub.strip()
                if sub:
                    segments.append(sub)
        else:
            segments.append(chunk)
    return segments


def _is_time_segment(seg: str) -> bool:
    s = seg.strip()
    if not s:
        return True
    if BELL_ONLY_RE.match(s):
        return True
    if re.search(r"\b(dzwon\w*|po\s+\w+\s+dzwon)\b", s, re.IGNORECASE):
        return True
    stripped = TIME_HINT_WORDS.sub("", s).strip(" .,;:—-“”\"'")
    if not stripped or len(stripped) <= 2:
        return True
    if stripped.lower() in BELL_WORDS:
        return True
    return False


def _is_location_segment(seg: str) -> bool:
    s = seg.strip()
    if not s or len(s) > MAX_LOCATION_LEN:
        return False
    if _is_time_segment(s):
        return False
    if LOCATION_KEYWORDS.search(s):
        return True
    if PLACE_PREFIX_RE.match(s):
        return True
    cleaned = TRAILING_TIME_RE.sub("", s).strip(" .,;:—-“”\"'")
    if cleaned and len(cleaned) <= MAX_LOCATION_LEN and not _is_time_segment(cleaned):
        if len(cleaned) >= 6 and DATE_RE.search(cleaned) is None:
            return True
    return False


def _clean_location(seg: str) -> str:
    cleaned = TRAILING_TIME_RE.sub("", seg.strip())
    cleaned = TIME_HINT_WORDS.sub("", cleaned).strip(" .,;:—-“”\"'")
    cleaned = re.sub(r"\s+", " ", cleaned)
    return cleaned[:MAX_LOCATION_LEN].strip()


def _parse_single_line(text: str) -> tuple[str, str]:
    if not text or not text.strip():
        return ("brak info", "brak info")

    text = text.strip()
    date_str = "brak info"
    m = DATE_RE.search(text)
    if m:
        day = m.group(1)
        month = m.group(2)
        date_str = f"{day} {month}" if day else month
        remainder = (text[: m.start()] + text[m.end() :]).strip(" ,.;:—-")
    else:
        remainder = text

    segments = _split_segments(remainder) if remainder else []
    time_parts: list[str] = []
    location = "brak info"

    for seg in segments:
        if _is_location_segment(seg):
            loc = _clean_location(seg)
            if loc:
                location = loc
                break
        elif _is_time_segment(seg):
            time_parts.append(seg.strip())
        elif (
            location == "brak info"
            and len(seg) <= MAX_LOCATION_LEN
            and (LOCATION_KEYWORDS.search(seg) or PLACE_PREFIX_RE.match(seg) or len(seg) >= 12)
        ):
            loc = _clean_location(seg)
            if loc and not _is_time_segment(loc):
                location = loc
                break

    if time_parts:
        time_blob = ", ".join(time_parts)
        date_str = f"{date_str}, {time_blob}" if date_str != "brak info" else time_blob

    return (date_str, location)


def parse_header(filename: str, header_text: str) -> tuple[str, str]:
    """
    Extract (date, location) from document header and filename.
    Header wins when present; filename fills gaps.
    """
    first_line = (header_text or "").split("\n")[0] if header_text else ""
    head_date, head_loc = _parse_single_line(first_line)

    # Filename often encodes location in title after " - "
    name_part = filename or ""
    if " - " in name_part:
        name_part = name_part.split(" - ", 1)[1]
    name_date, name_loc = _parse_single_line(name_part)

    date = head_date if head_date != "brak info" else name_date
    location = head_loc if head_loc != "brak info" else name_loc

    if location == "brak info" and " - " in (filename or ""):
        title = filename.split(" - ", 1)[1]
        if LOCATION_KEYWORDS.search(title) or PLACE_PREFIX_RE.search(title):
            _, title_loc = _parse_single_line(title)
            if title_loc != "brak info":
                location = title_loc
        elif "," in title:
            after_comma = title.split(",", 1)[-1].strip()
            if LOCATION_KEYWORDS.search(after_comma):
                location = after_comma[:MAX_LOCATION_LEN]

    return (date, location)


def extract_header_from_docx(paragraphs) -> str:
    """Return best header line from first paragraphs of a docx."""
    for para in paragraphs[:8]:
        t = (para.text if hasattr(para, "text") else str(para)).strip()
        if not t:
            continue
        if "docs.google.com" in t or t.startswith("http"):
            continue
        if t.startswith("(Poprzedni") or "Poprzedni plik" in t[:50]:
            continue
        if TAG_RE.match(t) and not DATE_RE.search(t):
            continue
        if re.match(r"^DZIENNIK\s+ZADAŃ", t, re.IGNORECASE):
            return ""
        if DATE_RE.search(t) or LOCATION_KEYWORDS.search(t) or PLACE_PREFIX_RE.search(t):
            if len(t) < 350:
                return t
    return ""
