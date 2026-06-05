#!/usr/bin/env python3
"""
infer_kraina_dates.py — uzupełnij brakujące daty z grafu linków między scenami.

Semantyka krawędzi (jak w Google Docs):
  A → B  =  dokument A zawiera link do B (B = poprzednia scena)
  ⇒ B jest chronologicznie wcześniej niż A

Dla sceny bez daty:
  - krawędzie wychodzące N → P  →  N jest po P  →  dolna granica: max(sortKey(P) + 1h)
  - krawędzie przychodzące X → N  →  N jest przed X  →  górna granica: min(sortKey(X) − 1h)

Wynik w adventure_map.json:
  dateInferred, dateInference, date (z prefiksem ≥ / ≤), sortKey
"""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path

from assign_sort_keys import (
    IS_FOLDER,
    date_to_sortkey,
    sortkey_to_human,
)
from kraina_colors import KRAINA_JSON

SCRIPT_DIR = Path(__file__).resolve().parent
HOUR_STEP = 100  # +1h w formacie MMDDHHMM (minuty = 00)


def sortkey_add_hours(sk: int, hours: int) -> int:
    month = sk // 1_000_000
    rest = sk % 1_000_000
    day = rest // 10_000
    hour = (rest % 10_000) // 100
    minute = rest % 100
    hour += hours
    while hour >= 24:
        hour -= 24
        day += 1
    return month * 1_000_000 + day * 10_000 + hour * 100 + minute


def sortkey_sub_hours(sk: int, hours: int) -> int:
    month = sk // 1_000_000
    rest = sk % 1_000_000
    day = rest // 10_000
    hour = (rest % 10_000) // 100
    minute = rest % 100
    hour -= hours
    while hour < 0:
        hour += 24
        day = max(1, day - 1)
    return month * 1_000_000 + day * 10_000 + hour * 100 + minute


def needs_date(node: dict) -> bool:
    d = (node.get("date") or "").strip()
    return d.lower() in ("brak info", "—", "-", "") or node.get("sortKey") is None


def anchor_sortkey(node: dict) -> int | None:
    """Znany moment sceny (explicit lub już wywnioskowany w tej samej serii passów)."""
    sk = node.get("sortKey")
    if sk is not None:
        return sk
    sk = date_to_sortkey(node.get("date", ""))
    return sk


def infer_bounds(
    nid: str,
    nodes: dict[str, dict],
    out_edges: dict[str, list[str]],
    in_edges: dict[str, list[str]],
    *,
    anchors_only: bool,
) -> tuple[int | None, int | None, list[str], list[str]]:
    lower: int | None = None
    upper: int | None = None
    lower_from: list[str] = []
    upper_from: list[str] = []

    for pred_id in out_edges.get(nid, []):
        pred = nodes.get(pred_id, {})
        if anchors_only and pred.get("dateInferred"):
            continue
        sk = anchor_sortkey(pred)
        if sk is None:
            continue
        cand = sortkey_add_hours(sk, 1)
        if lower is None or cand > lower:
            lower = cand
            lower_from = [pred.get("name", pred_id)]
        elif cand == lower:
            lower_from.append(pred.get("name", pred_id))

    for succ_id in in_edges.get(nid, []):
        succ = nodes.get(succ_id, {})
        if anchors_only and succ.get("dateInferred"):
            continue
        sk = anchor_sortkey(succ)
        if sk is None:
            continue
        cand = sortkey_sub_hours(sk, 1)
        if upper is None or cand < upper:
            upper = cand
            upper_from = [succ.get("name", succ_id)]
        elif cand == upper:
            upper_from.append(succ.get("name", succ_id))

    return lower, upper, lower_from, upper_from


def pick_inferred(
    lower: int | None,
    upper: int | None,
    lower_from: list[str],
    upper_from: list[str],
) -> tuple[int | None, str, str]:
    if lower is not None and upper is not None:
        if lower <= upper:
            human = sortkey_to_human(lower)
            refs = ", ".join(lower_from[:3])
            return (
                lower,
                f"≥ {human}",
                f"między {refs} a {', '.join(upper_from[:3])} (+1h / −1h z linków)",
            )
        human = sortkey_to_human(lower)
        return (
            lower,
            f"≥ {human}",
            f"konflikt górnej/dolnej granicy; użyto dolnej po {', '.join(lower_from[:3])}",
        )
    if lower is not None:
        human = sortkey_to_human(lower)
        return (
            lower,
            f"≥ {human}",
            f"po {', '.join(lower_from[:3])} (+1h, link wstecz)",
        )
    if upper is not None:
        human = sortkey_to_human(upper)
        return (
            upper,
            f"≤ {human}",
            f"przed {', '.join(upper_from[:3])} (−1h, link ze sceny późniejszej)",
        )
    return None, "", ""


def run_inference(data: dict, anchors_only: bool) -> int:
    nodes = {n["id"]: n for n in data["nodes"]}
    out_edges: dict[str, list[str]] = {}
    in_edges: dict[str, list[str]] = {}

    for e in data["edges"]:
        fr, to = e["from"], e["to"]
        if nodes.get(fr, {}).get("mimeType") == IS_FOLDER:
            continue
        if nodes.get(to, {}).get("mimeType") == IS_FOLDER:
            continue
        out_edges.setdefault(fr, []).append(to)
        in_edges.setdefault(to, []).append(fr)

    changed = 0
    for nid, node in nodes.items():
        if node.get("mimeType") == IS_FOLDER:
            continue
        if not needs_date(node):
            continue
        if node.get("dateInferred") and anchors_only:
            continue

        lower, upper, lf, uf = infer_bounds(
            nid, nodes, out_edges, in_edges, anchors_only=anchors_only
        )
        sk, date_txt, note = pick_inferred(lower, upper, lf, uf)
        if sk is None:
            continue

        node["dateInferred"] = True
        node["dateInference"] = {
            "kind": "graph_links",
            "lowerBound": sortkey_to_human(lower) if lower else None,
            "upperBound": sortkey_to_human(upper) if upper else None,
            "predecessors": lf[:5],
            "successors": uf[:5],
            "note": note,
        }
        if not node.get("dateOriginal"):
            node["dateOriginal"] = node.get("date") or "brak info"
        node["date"] = date_txt
        node["sortKey"] = sk
        changed += 1

    return changed


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    data = json.loads(KRAINA_JSON.read_text(encoding="utf-8"))
    docs = [n for n in data["nodes"] if n["mimeType"] != IS_FOLDER]
    before = sum(1 for n in docs if needs_date(n))

    c1 = run_inference(data, anchors_only=True)
    c2 = run_inference(data, anchors_only=False)

    after = sum(1 for n in docs if needs_date(n))
    inferred = sum(1 for n in docs if n.get("dateInferred"))

    print(f"Bez daty przed: {before}")
    print(f"Wywnioskowano (pass 1 anchors): {c1}")
    print(f"Wywnioskowano (pass 2 chain):   {c2}")
    print(f"Bez daty po:    {after}")
    print(f"Razem dateInferred: {inferred}")

    if args.dry_run:
        print("(dry-run — nie zapisuję)")
        return

    backup = KRAINA_JSON.with_suffix(".json.bak")
    shutil.copy2(KRAINA_JSON, backup)
    KRAINA_JSON.write_text(
        json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    print(f"Zapisano {KRAINA_JSON} (kopia {backup.name})")


if __name__ == "__main__":
    main()
