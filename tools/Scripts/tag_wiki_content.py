#!/usr/bin/env python3
"""Add wiki access tags to all markdown under dagonite-wiki/content.

Convention (see content/_meta/wiki-parties.json):
  wiki-public       — lore for everyone (anonymous)
  wiki-logged-in    — wiki navigation (any logged-in user)
  lawenda, sir-bron — single hero who may read the page
  team-bonefyre     — whole party (2+ heroes from that party in scene)

Run: python3 tag_wiki_content.py [--dry-run]
Then: python3 build_wiki_access_manifest.py
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(SCRIPT_DIR))

import build_wiki_access_manifest as m  # noqa: E402

CONTENT = m.CONTENT
BOTH_TEAMS = ["team-bonefyre", "team-pijany-smok"]
CAMPAIGN_ROOT = "W służbie Bonefyre"

_CFG: m.Config | None = None


def _cfg() -> m.Config:
    global _CFG
    if _CFG is None:
        _CFG = m.load_config()
    return _CFG


def _resolve(name: str) -> str | None:
    return m.resolve_character(name, _cfg())


def char_to_tag(name: str) -> str:
    return name.strip().lower().replace(" ", "-")


def access_tags_for_players(mentioned: set[str]) -> list[str]:
    if not mentioned:
        return []

    party_to_chars: dict[str, set[str]] = {}
    cfg = _cfg()
    for player in mentioned:
        canonical = m.resolve_character(player, cfg) or player
        for pid in cfg.char_by_party.get(canonical, []):
            party_to_chars.setdefault(pid, set()).add(canonical)

    tags: list[str] = []
    for pid, chars in sorted(party_to_chars.items()):
        if len(chars) == 1:
            tags.append(char_to_tag(next(iter(chars))))
        else:
            tags.append(f"team-{pid}")
    return sorted(set(tags))


def players_mentioned(body: str) -> set[str]:
    cfg = _cfg()
    found = m.player_names_in_text(body, cfg)
    for name in sorted(cfg.player_names, key=len, reverse=True):
        pattern = re.escape(name).replace(r"\ ", r"[\s\-]+")
        if re.search(pattern, body, re.IGNORECASE):
            found.add(name)
    return found


def resolve_wstęp_hero(title: str) -> str | None:
    hero = re.sub(r"(?i)^wstęp\s*", "", title).strip()
    for candidate in (hero, *hero.split()):
        hit = _resolve(candidate)
        if hit:
            return hit
    if "dorea" in hero.lower():
        return _resolve("Dorian")
    if "greatwing" in hero.lower():
        return _resolve("Werner")
    return None


def tags_for_archiwum(fm: dict, title: str, body: str) -> list[str]:
    title = m.clean_title(title or "")
    if "wstęp" in title.lower():
        canonical = resolve_wstęp_hero(title)
        if canonical:
            return [char_to_tag(canonical)]

    players = m.canonicalize_players(m.parse_players(fm), _cfg())
    if players:
        return access_tags_for_players(set(players))

    # Title/body fallback for scenes without players: (e.g. group summaries)
    mentioned = players_mentioned(body)
    if mentioned:
        return access_tags_for_players(mentioned)

    return list(BOTH_TEAMS)


def tags_for_player_sheet(stem: str) -> list[str]:
    canonical = _resolve(stem)
    if not canonical:
        return list(BOTH_TEAMS)
    tags = [char_to_tag(canonical)]
    for pid in _cfg().char_by_party.get(canonical, []):
        tags.append(f"team-{pid}")
    return sorted(set(tags))


def tags_for_path(rel: str, md: Path, fm: dict, body: str) -> list[str]:
    title = str(fm.get("title") or md.stem)

    if rel.startswith("Świat i zasady"):
        return ["wiki-public"]
    if rel.startswith("Mapy/"):
        return ["wiki-logged-in"]
    if rel == "index.md":
        return ["wiki-logged-in"]

    if rel.startswith(f"{CAMPAIGN_ROOT}/Archiwum sesji"):
        return tags_for_archiwum(fm, title, body)

    if rel.startswith(f"{CAMPAIGN_ROOT}/Postacie/NPC"):
        if md.name == "index.md":
            return list(BOTH_TEAMS)
        mentioned = players_mentioned(body)
        auth = access_tags_for_players(mentioned)
        return auth or ["team-bonefyre"]

    if rel.startswith(f"{CAMPAIGN_ROOT}/Postacie/"):
        if md.name == "index.md":
            return list(BOTH_TEAMS)
        return tags_for_player_sheet(md.stem)

    if rel.startswith(f"{CAMPAIGN_ROOT}/Lokacje"):
        if md.name == "index.md":
            return list(BOTH_TEAMS)
        mentioned = players_mentioned(body)
        auth = access_tags_for_players(mentioned)
        return auth or ["team-bonefyre"]

    if rel.startswith(f"{CAMPAIGN_ROOT}/Kronika"):
        if md.name == "index.md" or rel.endswith("Kronika/index.md") or rel.endswith("Wątki/index.md"):
            return list(BOTH_TEAMS)
        existing_auth = [t for t in m.parse_tags(fm) if _is_access_tag(t)]
        if existing_auth:
            return existing_auth
        mentioned = players_mentioned(body)
        return access_tags_for_players(mentioned) or list(BOTH_TEAMS)

    if rel.startswith(CAMPAIGN_ROOT):
        if md.name == "index.md":
            return list(BOTH_TEAMS)
        mentioned = players_mentioned(body)
        return access_tags_for_players(mentioned) or list(BOTH_TEAMS)

    return ["wiki-logged-in"]


def _is_access_tag(tag: str) -> bool:
    tl = tag.strip().lower()
    if tl in ("wiki-public", "wiki-logged-in"):
        return True
    if m.TEAM_TAG_RE.match(tl):
        return True
    cfg = _cfg()
    return m.resolve_character(tl, cfg) is not None or tl in cfg.party_characters


def merge_tags(existing: list[str], auth: list[str]) -> list[str]:
    kept = [t for t in existing if not _is_access_tag(t)]
    return sorted(set(kept + auth), key=lambda x: (_is_access_tag(x), x))


def parse_frontmatter_block(text: str) -> tuple[dict[str, str], str]:
    if not text.startswith("---"):
        return {}, text
    end = text.find("\n---", 3)
    if end < 0:
        return {}, text
    block = text[3:end]
    rest = text[end + 4 :]
    fields: dict[str, str] = {}
    for line in block.splitlines():
        if ":" not in line:
            continue
        key, _, val = line.partition(":")
        fields[key.strip()] = val.strip()
    return fields, rest


def format_tags(tags: list[str]) -> str:
    return "tags: [" + ", ".join(tags) + "]"


def ensure_frontmatter(text: str, fields: dict[str, str]) -> str:
    _, body = parse_frontmatter_block(text)
    lines = ["---"]
    for key in ("title", "tags"):
        if key in fields:
            lines.append(f"{key}: {fields[key]}" if key != "tags" else format_tags(
                [t.strip() for t in fields["tags"].strip("[]").split(",") if t.strip()]
            ))
    for key, val in fields.items():
        if key not in ("title", "tags"):
            lines.append(f"{key}: {val}")
    lines.append("---")
    return "\n".join(lines) + "\n" + body.lstrip("\n")


def update_file(md: Path, auth_tags: list[str], dry_run: bool) -> list[str]:
    text = md.read_text(encoding="utf-8")
    fields, body = parse_frontmatter_block(text)

    if not fields and md.name == "index.md" and str(md.relative_to(CONTENT)) == "index.md":
        fields = {"title": '"DagoniteEmpire — Wiki"'}
        text = ensure_frontmatter(text, fields)
        fields, body = parse_frontmatter_block(text)

    if "tags" in fields:
        existing = m.parse_tags({"tags": fields["tags"]})
        merged = merge_tags(existing, auth_tags)
        new_line = format_tags(merged)
        if fields["tags"] in text:
            new_text = re.sub(
                r"^tags:\s*\[.*\]\s*$",
                new_line,
                text,
                count=1,
                flags=re.MULTILINE,
            )
        else:
            new_text = text
    else:
        merged = merge_tags([], auth_tags)
        if text.startswith("---"):
            end = text.find("\n---", 3)
            if end >= 0:
                block = text[3:end]
                new_text = text[: end + 4] + "\n" + format_tags(merged) + text[end + 4 :]
            else:
                new_text = text
        else:
            new_text = "---\n" + format_tags(merged) + "\n---\n\n" + text

    if new_text != text and not dry_run:
        md.write_text(new_text, encoding="utf-8")
    return merged


def main() -> None:
    dry = "--dry-run" in sys.argv
    _cfg()
    updated = 0
    for md in sorted(CONTENT.rglob("*.md")):
        if "_meta" in md.parts:
            continue
        rel = md.relative_to(CONTENT).as_posix()
        text = md.read_text(encoding="utf-8")
        fm = m.parse_frontmatter(text)
        _, body = parse_frontmatter_block(text)
        auth = tags_for_path(rel, md, fm, body)
        before = text
        merged = update_file(md, auth, dry_run=dry)
        after = md.read_text(encoding="utf-8") if not dry else before
        changed = before != after
        if changed:
            updated += 1
        print(f"{'~' if changed else ' '} {rel}: {merged}")

    print(f"\n{'Would update' if dry else 'Updated'} {updated} file(s).")
    if not dry:
        print("Run: python3 build_wiki_access_manifest.py")


if __name__ == "__main__":
    main()
