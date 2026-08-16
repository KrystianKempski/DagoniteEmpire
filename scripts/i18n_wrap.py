#!/usr/bin/env python3
"""Semi-automatic L["..."] wrapper for Razor files.

Only wraps literals whose exact text is a KEY in SharedResources.pl.resx — i.e.
exactly the display strings the master↔Localisation diff changed. Because the
resx keys are display prose, code-ish attribute values (Class/Style/Icon/Property)
never match, which keeps the pass safe.

Two contexts are handled:
  1. Element text nodes:  >Save</...>            -> >@L["Save"]</...>
  2. Whitelisted display attributes: Title="Save" -> Title="@(L["Save"])"

Skipped (left for manual review): interpolation (@/{), nested quotes, already
wrapped (@L[), and anything not present as a resx key.

Usage: python3 scripts/i18n_wrap.py <file.razor> [<file.razor> ...]
"""
import re
import sys
import xml.etree.ElementTree as ET

RESX = "DagoniteEmpire/Resources/Localization/SharedResources.pl.resx"

# Attributes whose value is user-facing display text.
ATTR_WHITELIST = {
    "Title", "Label", "Text", "Placeholder", "HelperText", "AdornmentText",
    "ButtonText", "CancelText", "OkText", "NoText", "YesText", "SubmitLabel",
    "CancelLabel", "alt", "aria-label",
}


def load_keys() -> set[str]:
    keys = set()
    for data in ET.parse(RESX).getroot().findall("data"):
        name = data.get("name")
        if name:
            keys.add(name)
    return keys


def wrappable(key: str) -> bool:
    return key and '"' not in key and "\\" not in key and "{" not in key


def wrap_file(path: str, keys: set[str]) -> tuple[int, int]:
    with open(path, encoding="utf-8") as fh:
        src = fh.read()
    text_n = attr_n = 0

    # 1) Element text nodes: >TEXT<
    def text_sub(m: re.Match) -> str:
        nonlocal text_n
        lead, body, trail = m.group(1), m.group(2), m.group(3)
        key = body.strip()
        if key in keys and wrappable(key) and "@L[" not in body:
            text_n += 1
            return f'>{lead}@L["{key}"]{trail}<'
        return m.group(0)

    src = re.sub(r'>(\s*)([^<>@{}"]+?)(\s*)<', text_sub, src)

    # 2) Whitelisted display attributes: Attr="TEXT"
    attr_alt = "|".join(re.escape(a) for a in ATTR_WHITELIST)

    def attr_sub(m: re.Match) -> str:
        nonlocal attr_n
        name, val = m.group(1), m.group(2)
        if val in keys and wrappable(val):
            attr_n += 1
            return f'{name}="@(L["{val}"])"'
        return m.group(0)

    src = re.sub(rf'\b({attr_alt})="([^"@{{]+)"', attr_sub, src)

    if text_n or attr_n:
        with open(path, "w", encoding="utf-8") as fh:
            fh.write(src)
    return text_n, attr_n


def main(argv: list[str]) -> int:
    if not argv:
        print("usage: i18n_wrap.py <file.razor> ...", file=sys.stderr)
        return 2
    keys = load_keys()
    tot_t = tot_a = 0
    for path in argv:
        t, a = wrap_file(path, keys)
        tot_t += t
        tot_a += a
        if t or a:
            print(f"{path}: text={t} attr={a}")
    print(f"TOTAL: text={tot_t} attr={tot_a} across {len(argv)} files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
