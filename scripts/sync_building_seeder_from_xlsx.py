#!/usr/bin/env python3
"""Merge Buildings sheet from Katalog_towarow_handlowych.xlsx into BuildingTemplateSeeder.cs."""

from __future__ import annotations

import re
from pathlib import Path

import openpyxl

ROOT = Path(__file__).resolve().parents[1]
XLSX = ROOT / "docs/barony/Katalog_towarow_handlowych.xlsx"
SEEDER = ROOT / "DagoniteEmpire/Service/BuildingTemplateSeeder.cs"

PPB_COLS = [
    ("Food", "food"),
    ("Economy", "economy"),
    ("Production", "production"),
    ("Loyalty", "loyalty"),
    ("Stability", "stability"),
    ("Law", "law"),
    ("Corruption", "corruption"),
    ("Science", "science"),
    ("Magic", "magic"),
    ("Culture", "culture"),
    ("Intelligence", "intelligence"),
    ("Defense", "defense"),
    ("Treasury", "treasury"),
]

# Seeder names superseded by Excel canonical names
REMOVE_NAMES = {
    "Small Brewery",  # -> Brewery
    "Herbalist",  # -> Herbalist / Alchemist's Workshop
}

TERRAIN_BY_NAME = {
    "Quarry - Granite": "Quarry",
    "Quarry - common stone": "Quarry",
    "Quarry - Obsidian": "Obsidian",
    "Quarry - Tarnit": "Quarry",
    "Clay pit": "Clay",
    "Mine - precious gems (luxury)": "Quarry",
    "Mine - soft metals": "Mine",
    "Mine - Silver": "Mine",
    "Mine - Salt": "Salt",
    "Mine - Sulfur": "Sulfur",
    "Mine - Gold": "Mine",
    "Mine - Iron": "Mine",
    "Mine - Dagoferryt": "Mine",
    "Sawmill - Ironwood": "Forest",
    "Sawmill - Elven alder": "Forest",
    "Sawmill - Shipbuilding wood": "Forest",
}


def pn(v) -> float | None:
    if v is None or v == "":
        return None
    if isinstance(v, (int, float)):
        return float(v)
    return float(str(v).strip().replace(",", "."))


def esc(s: str) -> str:
    return s.replace("\\", "\\\\").replace('"', '\\"')


def load_excel_buildings() -> list[dict]:
    wb = openpyxl.load_workbook(XLSX, data_only=True)
    ws = wb["Buildings"]
    hdr = {ws.cell(1, c).value: c for c in range(1, 40) if ws.cell(1, c).value}
    rows = []
    for r in range(2, ws.max_row + 1):
        name = ws.cell(r, hdr["Name (trade goods)"]).value
        if not name:
            continue
        row = {"name": str(name).strip()}
        for h, c in hdr.items():
            row[h] = ws.cell(r, c).value
        rows.append(row)
    return rows


def fx_args(row: dict) -> list[str]:
    parts = []
    for col, arg in PPB_COLS:
        v = pn(row.get(col))
        if v is None:
            continue
        if v == int(v):
            parts.append(f"{arg}: {int(v)}")
        else:
            parts.append(f"{arg}: {v}m")
    return parts


def generate_add(row: dict) -> str:
    name = row["name"]
    lordship = int(row.get("Lordship") or 1)
    kind = row.get("Kind") or "Building"
    kind_cs = "B" if kind == "Building" else "I"
    prod = int(pn(row.get("Production cost")) or 0)
    gold = int(pn(row.get("Gold cost")) or 0)
    notes = row.get("Notes")
    desc = str(notes).strip() if notes else f"{name}."
    # Keep multi-line notes (Produces …) as C# \n escapes
    desc_cs = esc(desc).replace("\n", "\\n")
    fx = fx_args(row)
    terrain = TERRAIN_BY_NAME.get(name)
    lines = []
    if fx:
        fx_s = ", ".join(fx)
        if terrain:
            lines.append(
                f'            Add("{esc(name)}", {lordship}, {kind_cs}, {prod}, {gold},'
            )
            lines.append(f'                "{desc_cs}",')
            lines.append(f"                Fx({fx_s}),")
            lines.append(f'                terrainRequirement: "{terrain}");')
        else:
            lines.append(
                f'            Add("{esc(name)}", {lordship}, {kind_cs}, {prod}, {gold},'
            )
            lines.append(f'                "{desc_cs}",')
            lines.append(f"                Fx({fx_s}));")
    else:
        lines.append(f'            Add("{esc(name)}", {lordship}, {kind_cs}, {prod}, {gold},')
        lines.append(f'                "{desc_cs}");')
    return "\n".join(lines)


def split_add_blocks(text: str) -> tuple[str, list[tuple[str, str]], str]:
    """Return (prefix, [(name, block)], suffix) for CreateAll body."""
    m = re.search(r"(static PpbVector Fx\([\s\S]*?\n            \})\n\n            const string B", text)
    if not m:
        raise RuntimeError("Could not find Fx helper")
    prefix_end = text.find("            Add(", m.end())
    prefix = text[:prefix_end]
    rest = text[prefix_end:]
    suffix_m = re.search(r"\n            return list;\n        \}\n    \}\n\}", rest)
    if not suffix_m:
        raise RuntimeError("Could not find return list")
    body = rest[: suffix_m.start()]
    suffix = rest[suffix_m.start() :]

    blocks: list[tuple[str, str]] = []
    pos = 0
    while pos < len(body):
        m = re.match(r'\s*Add\("((?:[^"\\]|\\.)*)"', body[pos:])
        if not m:
            pos += 1
            continue
        name = m.group(1).replace('\\"', '"')
        start = pos
        # find end of this Add(...) including optional terrainRequirement
        depth = 0
        i = pos + m.start()
        while i < len(body):
            if body[i : i + 3] == "Add":
                pass
            if body[i] == "(":
                depth += 1
            elif body[i] == ")":
                depth -= 1
                if depth == 0 and i + 1 < len(body) and body[i + 1] == ";":
                    end = i + 2
                    block = body[start:end].strip()
                    blocks.append((name, block))
                    pos = end
                    break
            i += 1
        else:
            break
    return prefix, blocks, suffix


def main() -> None:
    excel_rows = load_excel_buildings()
    excel_by_name = {r["name"]: r for r in excel_rows}
    excel_names = set(excel_by_name)

    text = SEEDER.read_text(encoding="utf-8")
    prefix, blocks, suffix = split_add_blocks(text)

    kept: list[str] = []
    replaced = set()
    for name, block in blocks:
        if name in REMOVE_NAMES:
            continue
        if name in excel_by_name:
            kept.append(generate_add(excel_by_name[name]))
            replaced.add(name)
        else:
            kept.append(block)

    for name, row in excel_by_name.items():
        if name not in replaced:
            kept.append(generate_add(row))

    # Insert trade-goods block comment before newly added tail (optional)
    new_body = "\n\n".join(kept) + "\n"
    out = prefix + new_body + suffix
    SEEDER.write_text(out, encoding="utf-8")
    print(f"Updated {SEEDER}")
    print(f"  Excel buildings: {len(excel_rows)}")
    print(f"  Replaced in seeder: {len(replaced)}")
    print(f"  Added new: {len(excel_rows) - len(replaced)}")
    print(f"  Removed legacy: {', '.join(sorted(REMOVE_NAMES))}")


if __name__ == "__main__":
    main()
