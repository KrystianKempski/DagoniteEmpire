#!/usr/bin/env python3
"""Build wiki/static/wiki-access.json from content + _meta/wiki-parties.json."""

from __future__ import annotations

import ast
import json
import re
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
EMPIRE_ROOT = SCRIPT_DIR.parent.parent
WIKI_ROOT = EMPIRE_ROOT.parent / "dagonite-wiki"
CONTENT = WIKI_ROOT / "content"
PARTIES_FILE = CONTENT / "_meta" / "wiki-parties.json"
QUARTZ_PUBLIC = WIKI_ROOT / "public"
OUTPUT = EMPIRE_ROOT / "DagoniteEmpire" / "wwwroot" / "wiki" / "static" / "wiki-access.json"
PARTIES_OUTPUT = OUTPUT.parent / "wiki-parties.json"
LINKS_OUTPUT = OUTPUT.parent / "wiki-links.json"

CONFIG: dict = {}
PLAYER_NAMES: set[str] = set()
PARTY_BY_CHARACTER: dict[str, list[str]] = {}
PARTY_CHARACTERS: dict[str, list[str]] = {}
MIN_PARTY_SCENE = 3


def load_config() -> None:
    global CONFIG, PLAYER_NAMES, PARTY_BY_CHARACTER, PARTY_CHARACTERS, MIN_PARTY_SCENE
    CONFIG = json.loads(PARTIES_FILE.read_text(encoding="utf-8"))
    MIN_PARTY_SCENE = int(CONFIG.get("partySceneMinPlayers", 3))
    for party_id, party in CONFIG.get("parties", {}).items():
        chars = list(party.get("characters", []))
        PARTY_CHARACTERS[party_id] = chars
        for name in chars:
            PLAYER_NAMES.add(name)
            PARTY_BY_CHARACTER.setdefault(name, []).append(party_id)


def parse_frontmatter(text: str) -> dict:
    if not text.startswith("---"):
        return {}
    end = text.find("\n---", 3)
    if end < 0:
        return {}
    block = text[3:end]
    try:
        import yaml  # type: ignore

        return yaml.safe_load(block) or {}
    except Exception:
        data: dict = {}
        for line in block.splitlines():
            if ":" not in line:
                continue
            key, _, val = line.partition(":")
            val = val.strip().strip('"').strip("'")
            data[key.strip()] = val
        return data


def clean_title(title: str) -> str:
    return title.strip().strip('"').strip("'")


def parse_tags(fm: dict) -> list[str]:
    raw = fm.get("tags")
    if raw is None:
        return []
    if isinstance(raw, list):
        return [str(t).strip() for t in raw]
    if isinstance(raw, str):
        s = raw.strip().strip("[]")
        if not s:
            return []
        try:
            parsed = ast.literal_eval("[" + s + "]" if not s.startswith("[") else s)
            if isinstance(parsed, list):
                return [str(t).strip() for t in parsed]
        except (SyntaxError, ValueError):
            return [t.strip() for t in s.split(",")]
    return []


def parse_players(fm: dict) -> list[str]:
    raw = fm.get("players")
    if raw is None:
        return []
    if isinstance(raw, list):
        return [str(p).strip() for p in raw if str(p).strip()]
    if isinstance(raw, str):
        s = raw.strip()
        if not s:
            return []
        try:
            parsed = ast.literal_eval(s)
            if isinstance(parsed, list):
                return [str(p).strip() for p in parsed]
        except (SyntaxError, ValueError):
            pass
    return []


def slugify_segment(segment: str) -> str:
    return segment.strip().lower().replace(" ", "-")


def content_rel_to_slug(rel: Path) -> str:
    parts = list(rel.with_suffix("").parts)
    if parts and parts[-1].lower() in ("index", "_index"):
        parts = parts[:-1]
        if not parts:
            return "index"
        return "/".join(slugify_segment(p) for p in parts) + "/"
    return "/".join(slugify_segment(p) for p in parts)


def read_data_slug(html_path: Path) -> str | None:
    text = html_path.read_text(encoding="utf-8", errors="ignore")[:8000]
    match = re.search(r'<body[^>]*\sdata-slug="([^"]+)"', text)
    return match.group(1).strip("/") if match else None


def resolve_slug(md: Path, public_dir: Path) -> str:
    rel = md.relative_to(CONTENT)
    guessed = content_rel_to_slug(rel).strip("/")
    if not public_dir.is_dir():
        return guessed

    candidates = [
        public_dir / f"{guessed}.html",
        public_dir / guessed / "index.html",
    ]
    for html_path in candidates:
        if html_path.is_file():
            return read_data_slug(html_path) or guessed

    return guessed


def wikilinks_in(text: str) -> list[str]:
    return [m.group(1).split("|")[0].strip() for m in re.finditer(r"\[\[([^\]]+)\]\]", text)]


def player_names_in_text(text: str) -> set[str]:
    found: set[str] = set()
    for link in wikilinks_in(text):
        base = link.split("/")[-1].strip()
        for player in PLAYER_NAMES:
            if base.lower() == player.lower():
                found.add(player)
    return found


def parties_for_players(players: list[str]) -> set[str]:
    result: set[str] = set()
    for p in players:
        result.update(PARTY_BY_CHARACTER.get(p, []))
    return result


def characters_for_parties(party_ids: set[str]) -> list[str]:
    chars: list[str] = []
    for pid in party_ids:
        chars.extend(PARTY_CHARACTERS.get(pid, []))
    return sorted(set(chars))


def campaign_party_characters() -> list[str]:
    chars: list[str] = []
    for camp in CONFIG.get("campaigns", []):
        for pid in camp.get("partyIds", []):
            chars.extend(PARTY_CHARACTERS.get(pid, []))
    return sorted(set(chars))


def entry(mode: str, characters: list[str] | None = None, reason: str = "") -> dict:
    return {
        "mode": mode,
        "characters": characters or [],
        "reason": reason,
    }


def scene_access(fm: dict, title: str) -> dict:
    title = clean_title(title)
    players = parse_players(fm)
    if "wstęp" in title.lower():
        hero = re.sub(r"(?i)^wstęp\s*", "", title).strip()
        for player in PLAYER_NAMES:
            if hero.lower() == player.lower():
                return entry("characters", [player], "intro-title")
        return entry("authenticated", reason="intro-unknown")

    if not players:
        return entry("authenticated", reason="scene-no-players")

    if len(players) <= 2:
        return entry("characters", sorted(set(players)), "scene-secret-1-2")

    party_ids = parties_for_players(players)
    return entry("characters", characters_for_parties(party_ids), "scene-party-3plus")


def thread_access(fm: dict, title: str, body: str) -> dict:
    title = clean_title(title)
    tags = parse_tags(fm)
    tag_hits = []
    for t in tags:
        tl = str(t).lower().replace("-", "")
        for player in PLAYER_NAMES:
            pl = player.lower().replace("-", "")
            if tl == pl or tl in pl or pl in tl:
                tag_hits.append(player)
                break
    if len(tag_hits) == 1:
        return entry("characters", [tag_hits[0]], "thread-single-hero-tag")

    mentioned = player_names_in_text(body)
    if len(mentioned) == 1:
        return entry("characters", sorted(mentioned), "thread-single-hero-body")

    if len(mentioned) == 0 and any(
        name.lower() in title.lower() for name in PLAYER_NAMES if title.count(name) == 1
    ):
        for name in PLAYER_NAMES:
            if name.lower() in title.lower():
                return entry("characters", [name], "thread-single-hero-title")

    return entry("authenticated", reason="thread-public")


def npc_access(body: str) -> dict:
    mentioned = player_names_in_text(body)
    if len(mentioned) == 1:
        return entry("characters", sorted(mentioned), "npc-single-contact")
    return entry("authenticated", reason="npc-public")


def classify(md: Path, fm: dict, body: str, slug: str) -> dict:
    rel = md.relative_to(CONTENT).as_posix()
    title = clean_title(str(fm.get("title") or md.stem))

    for prefix in CONFIG.get("anonymousPublicPrefixes", []):
        pslug = slugify_segment(prefix)
        if slug == pslug or slug.startswith(pslug + "/"):
            return entry("anonymous", reason="world-lore")

    for prefix in CONFIG.get("loggedInPublicPrefixes", []):
        pslug = slugify_segment(prefix)
        if slug == pslug or slug.startswith(pslug + "/"):
            return entry("authenticated", reason="public-logged-in")

    if "Archiwum sesji" in rel:
        return scene_access(fm, title)

    if "Kronika/Wątki" in rel:
        return thread_access(fm, title, body)

    if "Kronika" in rel:
        return entry("authenticated", reason="chronicle")

    if rel.startswith("W służbie Bonefyre/Postacie/NPC/"):
        return npc_access(body)

    if rel.startswith("W służbie Bonefyre/Postacie/"):
        name = md.stem
        for player in PLAYER_NAMES:
            if name.lower().replace("-", " ") == player.lower().replace("-", " "):
                return entry("characters", [player], "player-sheet")
        # Baron Mevir.md etc.
        if name in PLAYER_NAMES:
            return entry("characters", [name], "player-sheet")
        return entry("authenticated", reason="player-page")

    for camp in CONFIG.get("campaigns", []):
        folder = camp.get("contentFolder", "")
        if rel == folder or rel.startswith(folder + "/"):
            return entry("characters", campaign_party_characters(), "campaign-" + camp.get("id", ""))

    return entry("authenticated", reason="default")


def normalize_match_key(value: str) -> str:
    value = value.lower()
    return re.sub(r"[^a-z0-9ąćęłńóśźż]+", "", value)


def build_links(slugs: dict[str, dict]) -> dict:
    characters: dict[str, str] = {}
    campaigns: dict[str, str] = {}
    chapters: dict[str, str] = {}

    for md in sorted(CONTENT.rglob("*.md")):
        if "_meta" in md.parts:
            continue
        rel = md.relative_to(CONTENT).as_posix()
        slug = next((s for s, r in slugs.items() if r.get("source") == rel and not s.endswith("/")), None)
        if not slug:
            slug = content_rel_to_slug(md.relative_to(CONTENT)).strip("/")

        if rel.startswith("W służbie Bonefyre/Postacie/") and "/NPC/" not in rel:
            name = md.stem
            for player in PLAYER_NAMES:
                if name.lower().replace("-", " ") == player.lower().replace("-", " "):
                    characters[player] = slug
                    break
            else:
                if name in PLAYER_NAMES:
                    characters[name] = slug

        for camp in CONFIG.get("campaigns", []):
            folder = camp.get("contentFolder", "")
            title = camp.get("title", "")
            camp_id = camp.get("id", "").strip("/")
            if rel == folder or rel == f"{folder}/index.md":
                if title:
                    campaigns[title] = camp_id
                for alias in camp.get("aliases", []) or []:
                    campaigns[alias] = camp_id

        if "Archiwum sesji" in rel and md.stem.lower() not in ("index", "_index"):
            chapters[md.stem] = slug
            chapters[normalize_match_key(md.stem)] = slug

    for camp in CONFIG.get("campaigns", []):
        title = camp.get("title", "")
        camp_id = camp.get("id", "").strip("/")
        if title and title not in campaigns:
            campaigns[title] = camp_id

    return {
        "version": 1,
        "characters": characters,
        "campaigns": campaigns,
        "chapters": {k: v for k, v in chapters.items() if not k.isdigit()},
        "allPlayerNames": sorted(PLAYER_NAMES),
    }


def main() -> None:
    load_config()
    if not PARTIES_FILE.exists():
        raise SystemExit(f"Missing {PARTIES_FILE}")

    slugs: dict[str, dict] = {}

    for md in sorted(CONTENT.rglob("*.md")):
        if "_meta" in md.parts:
            continue
        body = md.read_text(encoding="utf-8")
        fm = parse_frontmatter(body)
        slug = resolve_slug(md, QUARTZ_PUBLIC)
        if not slug:
            slug = "index"
        rule = classify(md, fm, body, slug)
        rule["source"] = md.relative_to(CONTENT).as_posix()
        slugs[slug] = rule
        if not slug.endswith("/"):
            slugs[slug + "/"] = rule

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    manifest = {
        "version": 2,
        "generatedFrom": "content/_meta/wiki-parties.json",
        "slugs": slugs,
    }
    OUTPUT.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    PARTIES_OUTPUT.write_text(PARTIES_FILE.read_text(encoding="utf-8"), encoding="utf-8")
    links = build_links(slugs)
    LINKS_OUTPUT.write_text(json.dumps(links, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Wrote {len(slugs)} slug rules -> {OUTPUT}")
    print(f"Wrote wiki links ({len(links['characters'])} chars) -> {LINKS_OUTPUT}")


if __name__ == "__main__":
    main()
