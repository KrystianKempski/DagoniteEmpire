#!/usr/bin/env python3
"""Build wiki-access.json from markdown tags in dagonite-wiki/content.

Access rule (same in app + explorer / contentIndex filter):

  Postać X widzi stronę, gdy:
    • tag strony zawiera slug bohatera X (np. lawenda), lub
    • tag strony to team-<party-id>, a X należy do tej drużyny (wiki-parties.json).

  Dodatkowo (bez tagów bohaterów):
    • wiki-public     → wszyscy (lore)
    • wiki-logged-in  → każdy zalogowany (index, mapy)

  Brak tagów i poza publicznymi ścieżkami → deny (tylko MG/Admin).

Nadpisania MG: content/_meta/wiki-access-overrides.json (najwyższy priorytet).

Usage:
  python3 build_wiki_access_manifest.py
"""

from __future__ import annotations

import ast
import fnmatch
import json
import re
from dataclasses import dataclass, field
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
EMPIRE_ROOT = SCRIPT_DIR.parent.parent
WIKI_ROOT = EMPIRE_ROOT.parent / "dagonite-wiki"
CONTENT = WIKI_ROOT / "content"
PARTIES_FILE = CONTENT / "_meta" / "wiki-parties.json"
OVERRIDES_FILE = CONTENT / "_meta" / "wiki-access-overrides.json"
QUARTZ_PUBLIC = WIKI_ROOT / "public"
OUTPUT = EMPIRE_ROOT / "DagoniteEmpire" / "wwwroot" / "wiki" / "static" / "wiki-access.json"
PARTIES_OUTPUT = OUTPUT.parent / "wiki-parties.json"
LINKS_OUTPUT = OUTPUT.parent / "wiki-links.json"
AUDIT_FILE = SCRIPT_DIR / "output" / "wiki-access-audit.tsv"

TEAM_TAG_RE = re.compile(r"^team-(.+)$", re.I)

# Not used for access — only metadata / Quartz
STRUCTURAL_TAGS = frozenset({
    "wiki-public", "wiki-logged-in",
    "przygoda", "wspolne", "wątek", "watek", "index", "kronika", "wątki",
    "akt", "akt-1", "akt-2", "akt-3", "akt-4", "akt-5",
    "podsumowanie", "otwarty", "zamknięty", "zamkniety", "zamknięty-tymczasowo",
    "częściowo-zamknięty", "tajemnica", "główny", "glowny", "zaplanowany", "krytyczny",
    "choroba", "artefakt", "zlecenie", "npc", "lore", "mapa", "archiwum", "kampania",
    "organizacja", "zasady", "postać-gracza", "postac-gracza", "budynek", "lokacja",
    "dzielnica", "miasto", "region", "dzicz", "straż", "sojusznik", "antagonista",
    "zakon-gromu", "postacie", "status", "priorytet", "wysoki", "trwająca", "trwajaca",
})

VISIBILITY_BY_TAG = {
    "wiki-public": "anonymous",
    "wiki-logged-in": "authenticated",
}


@dataclass
class Config:
    anonymous_prefixes: list[str] = field(default_factory=list)
    logged_in_prefixes: list[str] = field(default_factory=list)
    party_characters: dict[str, list[str]] = field(default_factory=dict)
    char_by_party: dict[str, list[str]] = field(default_factory=dict)
    char_canon: dict[str, str] = field(default_factory=dict)
    player_names: set[str] = field(default_factory=set)
    campaigns: list[dict] = field(default_factory=list)
    overrides: dict[str, dict] = field(default_factory=dict)


def load_config() -> Config:
    raw = json.loads(PARTIES_FILE.read_text(encoding="utf-8"))
    cfg = Config(
        anonymous_prefixes=list(raw.get("anonymousPublicPrefixes", [])),
        logged_in_prefixes=list(raw.get("loggedInPublicPrefixes", [])),
        campaigns=list(raw.get("campaigns", [])),
        overrides={},
    )
    for party_id, party in raw.get("parties", {}).items():
        chars = list(party.get("characters", []))
        cfg.party_characters[party_id] = chars
        for name in chars:
            cfg.player_names.add(name)
            cfg.char_by_party.setdefault(name, []).append(party_id)
            cfg.char_canon[norm_char(name)] = name
    for canonical, aliases in raw.get("characterAliases", {}).items():
        if canonical in cfg.player_names:
            for alias in aliases:
                cfg.char_canon[norm_char(alias)] = canonical
    if OVERRIDES_FILE.exists():
        cfg.overrides = json.loads(OVERRIDES_FILE.read_text(encoding="utf-8")).get("overrides", {}) or {}
    return cfg


def norm_char(value: str) -> str:
    return re.sub(r"[\s\-_.]+", "", value.lower())


def resolve_character(value: str, cfg: Config) -> str | None:
    return cfg.char_canon.get(norm_char(value))


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
            if ":" in line:
                k, _, v = line.partition(":")
                data[k.strip()] = v.strip().strip('"').strip("'")
        return data


def parse_tags(fm: dict) -> list[str]:
    raw = fm.get("tags")
    if raw is None:
        return []
    if isinstance(raw, list):
        return [str(t).strip() for t in raw if str(t).strip()]
    s = str(raw).strip().strip("[]")
    if not s:
        return []
    try:
        parsed = ast.literal_eval(s if s.startswith("[") else f"[{s}]")
        if isinstance(parsed, list):
            return [str(t).strip() for t in parsed]
    except (SyntaxError, ValueError):
        pass
    return [p.strip() for p in s.split(",") if p.strip()]


@dataclass
class AccessRule:
    mode: str
    characters: list[str] = field(default_factory=list)
    parties: list[str] = field(default_factory=list)
    reason: str = ""
    source_file: str = ""

    def to_json(self) -> dict:
        return {
            "mode": self.mode,
            "characters": self.characters,
            "parties": self.parties,
            "reason": self.reason,
            "source": self.source_file,
        }


def rule_from_tags(tags: list[str], cfg: Config) -> AccessRule | None:
    """Map frontmatter tags → manifest entry. Core rule for heroes + parties."""
    for tag in tags:
        tl = tag.strip().lower()
        vis = VISIBILITY_BY_TAG.get(tl)
        if vis:
            return AccessRule(mode=vis, reason=f"tag-{tl}")

    characters: list[str] = []
    parties: list[str] = []
    for tag in tags:
        tl = tag.strip().lower()
        if not tl or tl in STRUCTURAL_TAGS:
            continue
        m = TEAM_TAG_RE.match(tl)
        if m:
            pid = m.group(1).lower()
            if pid in cfg.party_characters:
                parties.append(pid)
            continue
        canonical = resolve_character(tl, cfg) or resolve_character(tl.replace("-", " "), cfg)
        if canonical:
            characters.append(canonical)

    characters = sorted(set(characters))
    parties = sorted(set(parties))
    if not characters and not parties:
        return None

    # App treats Characters and Party the same: user in characters[] OR in parties[].
    return AccessRule(
        mode="characters",
        characters=characters,
        parties=parties,
        reason="tags",
    )


def rule_from_path_prefix(slug: str, cfg: Config) -> AccessRule | None:
    for prefix in cfg.anonymous_prefixes:
        p = slugify(prefix)
        if slug == p or slug.startswith(p + "/"):
            return AccessRule(mode="anonymous", reason="path-public")
    for prefix in cfg.logged_in_prefixes:
        p = slugify(prefix)
        if slug == p or slug.startswith(p + "/"):
            return AccessRule(mode="authenticated", reason="path-logged-in")
    return None


def rule_from_override(slug: str, cfg: Config) -> AccessRule | None:
    bare = slug.rstrip("/")
    for key in (slug, bare, bare + "/"):
        if key in cfg.overrides:
            return _override_to_rule(cfg.overrides[key])
    for pattern, data in cfg.overrides.items():
        if fnmatch.fnmatch(bare, pattern.rstrip("/")) or fnmatch.fnmatch(bare + "/", pattern):
            return _override_to_rule(data)
    return None


def _override_to_rule(data: dict) -> AccessRule:
    vis = str(data.get("visibility", "deny")).lower()
    chars = data.get("characters") or []
    parties = data.get("parties") or []
    if isinstance(chars, str):
        chars = [c.strip() for c in chars.split(",") if c.strip()]
    if isinstance(parties, str):
        parties = [p.strip() for p in parties.split(",") if p.strip()]
    mode = {
        "public": "anonymous",
        "anonymous": "anonymous",
        "authenticated": "authenticated",
        "logged-in": "authenticated",
        "party": "party",
        "characters": "characters",
        "deny": "deny",
        "gm-only": "deny",
        "gm": "deny",
    }.get(vis, "deny")
    return AccessRule(
        mode=mode,
        characters=list(chars),
        parties=list(parties),
        reason="override",
    )


def classify_page(slug: str, tags: list[str], cfg: Config, rel: str) -> tuple[AccessRule, str]:
    override = rule_from_override(slug, cfg)
    if override:
        return override, "override"

    tagged = rule_from_tags(tags, cfg)
    if tagged:
        return tagged, "tags"

    by_path = rule_from_path_prefix(slug, cfg)
    if by_path:
        return by_path, "path"

    return AccessRule(mode="deny", reason="missing-tags"), "deny"


def slugify(segment: str) -> str:
    return segment.strip().lower().replace(" ", "-")


def content_rel_to_slug(rel: Path) -> str:
    parts = list(rel.with_suffix("").parts)
    if parts and parts[-1].lower() in ("index", "_index"):
        parts = parts[:-1]
        return "/".join(slugify(p) for p in parts) + "/" if parts else "index/"
    return "/".join(slugify(p) for p in parts)


def read_quartz_slug(md: Path) -> str | None:
    rel = md.relative_to(CONTENT)
    guessed = content_rel_to_slug(rel).strip("/")
    if not QUARTZ_PUBLIC.is_dir():
        return guessed or None
    for candidate in (
        QUARTZ_PUBLIC / f"{guessed}.html",
        QUARTZ_PUBLIC / guessed / "index.html",
    ):
        if candidate.is_file():
            text = candidate.read_text(encoding="utf-8", errors="ignore")[:8000]
            m = re.search(r'<body[^>]*\sdata-slug="([^"]+)"', text)
            if m:
                return m.group(1).strip("/")
    return guessed or None


def build_links(cfg: Config, slug_by_rel: dict[str, str]) -> dict:
    characters: dict[str, str] = {}
    campaigns: dict[str, str] = {}
    chapters: dict[str, str] = {}

    for md in sorted(CONTENT.rglob("*.md")):
        if "_meta" in md.parts:
            continue
        rel = md.relative_to(CONTENT).as_posix()
        slug = slug_by_rel.get(rel) or content_rel_to_slug(md.relative_to(CONTENT)).strip("/")

        if rel.startswith("W służbie Bonefyre/Postacie/") and "/NPC/" not in rel:
            c = resolve_character(md.stem, cfg)
            if c:
                characters[c] = slug

        for camp in cfg.campaigns:
            folder = camp.get("contentFolder", "")
            title = camp.get("title", "")
            cid = camp.get("id", "").strip("/")
            if rel == folder or rel == f"{folder}/index.md":
                if title:
                    campaigns[title] = cid
                for alias in camp.get("aliases", []) or []:
                    campaigns[alias] = cid

        if "Archiwum sesji" in rel and md.stem.lower() not in ("index", "_index"):
            chapters[md.stem] = slug
            key = re.sub(r"[^a-z0-9ąćęłńóśźż]+", "", md.stem.lower())
            chapters[key] = slug

    return {
        "version": 1,
        "characters": characters,
        "campaigns": campaigns,
        "chapters": {k: v for k, v in chapters.items() if not k.isdigit()},
        "allPlayerNames": sorted(cfg.player_names),
    }


def write_audit(pages: list[tuple[str, AccessRule, str]]) -> None:
    AUDIT_FILE.parent.mkdir(parents=True, exist_ok=True)
    lines = ["slug\tmode\tsource\treason\tcharacters\tparties\tsource_file"]
    for slug, rule, src in pages:
        lines.append("\t".join([
            slug,
            rule.mode,
            src,
            rule.reason,
            ",".join(rule.characters),
            ",".join(rule.parties),
            rule.source_file,
        ]))
    AUDIT_FILE.write_text("\n".join(lines) + "\n", encoding="utf-8")


def print_report(pages: list[tuple[str, AccessRule, str]]) -> None:
    by_mode: dict[str, int] = {}
    by_src: dict[str, int] = {}
    denied: list[str] = []

    for slug, rule, src in pages:
        by_mode[rule.mode] = by_mode.get(rule.mode, 0) + 1
        by_src[src] = by_src.get(src, 0) + 1
        if rule.mode == "deny":
            denied.append(slug)

    print("\n── Wiki access (tag rule) ───────────────────────────")
    print(f"Pages: {len(pages)}")
    print("Visibility: " + ", ".join(f"{k}={v}" for k, v in sorted(by_mode.items())))
    print("Source:     " + ", ".join(f"{k}={v}" for k, v in sorted(by_src.items())))
    print(f"Audit: {AUDIT_FILE}")
    if denied:
        print(f"\n! {len(denied)} bez tagów (deny):")
        for s in denied[:30]:
            print(f"    {s}")
        if len(denied) > 30:
            print(f"    ... +{len(denied) - 30}")
    else:
        print("\nOK — wszystkie strony mają tagi lub ścieżkę publiczną.")
    print("────────────────────────────────────────────────────\n")


def main() -> None:
    if not PARTIES_FILE.exists():
        raise SystemExit(f"Missing {PARTIES_FILE}")

    cfg = load_config()
    slug_by_rel: dict[str, str] = {}
    all_rules: dict[str, AccessRule] = {}
    report_rows: list[tuple[str, AccessRule, str]] = []

    for md in sorted(CONTENT.rglob("*.md")):
        if "_meta" in md.parts:
            continue
        rel = md.relative_to(CONTENT).as_posix()
        slug = read_quartz_slug(md) or "index"
        slug_by_rel[rel] = slug

        fm = parse_frontmatter(md.read_text(encoding="utf-8"))
        rule, src = classify_page(slug, parse_tags(fm), cfg, rel)
        rule.source_file = rel

        all_rules[slug] = rule
        if not slug.endswith("/"):
            all_rules[slug + "/"] = rule
        if not slug.endswith("/index"):
            report_rows.append((slug, rule, src))

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    manifest = {
        "version": 4,
        "generatedFrom": "content tags (hero | team-*) + wiki-public | wiki-logged-in + overrides",
        "accessRule": (
            "Character X sees page if X is in page tags OR page has team-<party> and X is in that party."
        ),
        "slugs": {k: v.to_json() for k, v in all_rules.items()},
    }
    OUTPUT.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    PARTIES_OUTPUT.write_text(PARTIES_FILE.read_text(encoding="utf-8"), encoding="utf-8")
    LINKS_OUTPUT.write_text(
        json.dumps(build_links(cfg, slug_by_rel), ensure_ascii=False, indent=2),
        encoding="utf-8",
    )

    print(f"Wrote {len(all_rules)} slug rules → {OUTPUT}")
    print_report(report_rows)


# ── Helpers for tag_wiki_content.py ────────────────────────────────────────────

def parse_players(fm: dict) -> list[str]:
    raw = fm.get("players")
    if not raw:
        return []
    if isinstance(raw, list):
        return [str(p).strip() for p in raw if str(p).strip()]
    try:
        parsed = ast.literal_eval(str(raw))
        if isinstance(parsed, list):
            return [str(p).strip() for p in parsed]
    except (SyntaxError, ValueError):
        pass
    return []


def canonicalize_players(names: list[str], cfg: Config) -> list[str]:
    out: list[str] = []
    for n in names:
        out.append(resolve_character(n, cfg) or n)
    return sorted(set(out))


def clean_title(title: str) -> str:
    return title.strip().strip('"').strip("'")


def player_names_in_text(body: str, cfg: Config) -> set[str]:
    found: set[str] = set()
    for link in re.findall(r"\[\[([^\]]+)\]\]", body):
        base = link.split("|")[0].split("/")[-1].strip()
        c = resolve_character(base, cfg)
        if c:
            found.add(c)
    return found


if __name__ == "__main__":
    main()
