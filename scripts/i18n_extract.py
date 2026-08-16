#!/usr/bin/env python3
"""Extract an EN->PL string corpus from the localization branch diff and emit a .resx.

The `master` branch holds English strings in place; the `Localisation` branch holds the
Polish equivalents at the same code locations. `git diff master..Localisation` is therefore
a parallel EN<->PL corpus. This script pairs removed (EN) / added (PL) lines, extracts the
literals that actually changed, and writes:

  - DagoniteEmpire/Resources/Localization/SharedResources.pl.resx  (key = English, value = Polish)
  - scripts/i18n_review.tsv                                        (ambiguous cases for manual review)

Keys were deliberately left unchanged during localization, so identical literals (en == pl)
are skipped automatically — only genuine display strings are captured.
"""
from __future__ import annotations

import difflib
import os
import re
import subprocess
import sys
from xml.sax.saxutils import escape

BASE = "master"
HEAD = "Localisation"
REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

RESX_PATH = os.path.join(REPO, "DagoniteEmpire", "Resources", "Localization", "SharedResources.pl.resx")
REVIEW_PATH = os.path.join(REPO, "scripts", "i18n_review.tsv")

EXTS = (".razor", ".cs", ".json", ".cshtml", ".html")
SKIP_DIRS = ("/obj/", "/bin/", "/wwwroot/lib/")

STR_RE = re.compile(r'"((?:[^"\\]|\\.)*)"')
TAG_WRAP_RE = re.compile(r"^<(\w+)(?:\s[^>]*)?>(.*)</\1>$")
# Inner text of an element: the run between '>' and '<' with no markup/code chars.
TEXT_NODE_RE = re.compile(r'>([^<>@{}"]+)<')
# Markup / code that means a line is NOT a plain display string we can lift wholesale.
MARKUP_RE = re.compile(r'[<>@{}="]')
# A Polish/Latin display run: letters (incl. PL diacritics), digits, spaces and common punctuation.
PL = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ"
TEXTY_RE = re.compile(rf"^[\w{PL} ,.\-–—!?:;'’\"()/%+×→≥…&]+$")
HAS_LETTER_RE = re.compile(rf"[A-Za-z{PL}]")


def git(*args: str) -> str:
    return subprocess.check_output(["git", *args], text=True, cwd=REPO)


def is_texty(s: str) -> bool:
    s = s.strip()
    return bool(s) and bool(HAS_LETTER_RE.search(s)) and bool(TEXTY_RE.match(s))


def changed_files() -> list[str]:
    out = git("diff", "--name-only", f"{BASE}..{HEAD}")
    files = []
    for f in out.splitlines():
        if not f.endswith(EXTS):
            continue
        if any(d in f"/{f}" for d in SKIP_DIRS):
            continue
        files.append(f)
    return files


def hunk_pairs(diff: str):
    """Yield (minus_lines, plus_lines) blocks from a -U0 unified diff."""
    minus, plus = [], []
    for line in diff.splitlines():
        if line.startswith(("+++", "---", "diff ", "index ", "@@")):
            if minus or plus:
                yield minus, plus
                minus, plus = [], []
            continue
        if line.startswith("-"):
            minus.append(line[1:])
        elif line.startswith("+"):
            plus.append(line[1:])
        else:
            if minus or plus:
                yield minus, plus
                minus, plus = [], []
    if minus or plus:
        yield minus, plus


def extract_from_pair(en: str, pl: str, add, review, ctx):
    added_any = False
    # Element-text pass: pair >TEXT< segments even amid attributes with @/{ interpolation.
    en_nodes = TEXT_NODE_RE.findall(en)
    pl_nodes = TEXT_NODE_RE.findall(pl)
    if en_nodes and len(en_nodes) == len(pl_nodes):
        for a, b in zip(en_nodes, pl_nodes):
            a, b = a.strip(), b.strip()
            if a and b and a != b and is_texty(a) and is_texty(b):
                add(a, b, ctx)
                added_any = True

    lm = STR_RE.findall(en)
    lp = STR_RE.findall(pl)
    if lm and len(lm) == len(lp):
        emitted = False
        for a, b in zip(lm, lp):
            if a == b:
                continue
            if "{" in a or "{" in b:
                review.append((ctx, a, b, "interpolation/format"))
                continue
            if is_texty(a):
                add(a, b, ctx)
                emitted = True
            else:
                review.append((ctx, a, b, "non-texty literal"))
        if emitted or lm == lp:
            return
        if any(a != b for a, b in zip(lm, lp)):
            return
    if added_any:
        return
    # Fallback: no matched quoted literals (e.g. Razor element text). Lift whole plain-text lines.

    en_s, pl_s = en.strip(), pl.strip()
    if not en_s or not pl_s or en_s == pl_s:
        return
    mm, mp = TAG_WRAP_RE.match(en_s), TAG_WRAP_RE.match(pl_s)
    if mm and mp and mm.group(1) == mp.group(1):
        en_s, pl_s = mm.group(2).strip(), mp.group(2).strip()
    if (en_s and pl_s and en_s != pl_s
            and not MARKUP_RE.search(en_s) and not MARKUP_RE.search(pl_s)
            and is_texty(en_s) and is_texty(pl_s)):
        add(en_s, pl_s, ctx)
    else:
        review.append((ctx, en.strip(), pl.strip(), "unresolved line"))


def main() -> int:
    pairs: dict[str, str] = {}
    conflicts: dict[str, set[str]] = {}
    review: list[tuple[str, str, str, str]] = []

    def add(en: str, pl: str, ctx: str):
        en = en.strip()
        pl = pl.strip()
        if not en or en == pl:
            return
        if en in pairs and pairs[en] != pl:
            conflicts.setdefault(en, {pairs[en]}).add(pl)
            return
        pairs.setdefault(en, pl)

    files = changed_files()
    for f in files:
        diff = git("diff", "-U0", f"{BASE}..{HEAD}", "--", f)
        for minus, plus in hunk_pairs(diff):
            if len(minus) == len(plus) and minus:
                for en, pl in zip(minus, plus):
                    if en != pl:
                        extract_from_pair(en, pl, add, review, f)
            else:
                for en in minus:
                    for m in STR_RE.findall(en):
                        pass  # unmatched block; skip (added/removed whole lines)
                if minus or plus:
                    review.append((f, " | ".join(minus)[:200], " | ".join(plus)[:200], "unbalanced hunk"))

    for en, plset in conflicts.items():
        for pl in plset:
            review.append(("(conflict)", en, pl, "multiple PL for same EN"))

    write_resx(pairs)
    write_review(review)

    print(f"files scanned : {len(files)}")
    print(f"pairs emitted : {len(pairs)}")
    print(f"conflicts     : {len(conflicts)}")
    print(f"review items  : {len(review)}")
    print(f"resx  -> {os.path.relpath(RESX_PATH, REPO)}")
    print(f"review-> {os.path.relpath(REVIEW_PATH, REPO)}")
    return 0


RESX_HEADER = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""


def write_resx(pairs: dict[str, str]):
    os.makedirs(os.path.dirname(RESX_PATH), exist_ok=True)
    with open(RESX_PATH, "w", encoding="utf-8") as fh:
        fh.write(RESX_HEADER)
        for en in sorted(pairs):
            fh.write(f'  <data name="{escape(en)}" xml:space="preserve">\n')
            fh.write(f"    <value>{escape(pairs[en])}</value>\n")
            fh.write("  </data>\n")
        fh.write("</root>\n")


def write_review(review: list[tuple[str, str, str, str]]):
    os.makedirs(os.path.dirname(REVIEW_PATH), exist_ok=True)

    def clean(s: str) -> str:
        return s.replace("\t", " ").replace("\r", " ").replace("\n", " ")

    with open(REVIEW_PATH, "w", encoding="utf-8") as fh:
        fh.write("file\tEN\tPL\treason\n")
        for ctx, en, pl, reason in review:
            fh.write(f"{clean(ctx)}\t{clean(en)}\t{clean(pl)}\t{clean(reason)}\n")


if __name__ == "__main__":
    sys.exit(main())
