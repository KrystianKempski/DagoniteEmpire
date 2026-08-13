#!/usr/bin/env python3
"""Read-only dump of the Darkhold barony state from the dev PostgreSQL DB.

Reads the connection string from the app's dotnet user-secrets store so no
credentials are hard-coded. Never prints the password.
"""
import json
import os
import re
import sys

import psycopg2
import psycopg2.extras

SECRETS = os.path.expanduser(
    "~/.microsoft/usersecrets/9a3c55b2-09c2-4342-8905-6c957b9faa98/secrets.json"
)


def load_conn_string() -> str:
    with open(SECRETS, "r", encoding="utf-8-sig") as fh:
        data = json.load(fh)
    return data["ConnectionStrings:DefaultConnection"]


def npgsql_to_psycopg2(conn: str) -> dict:
    parts = {}
    for kv in conn.split(";"):
        kv = kv.strip()
        if not kv or "=" not in kv:
            continue
        k, v = kv.split("=", 1)
        parts[k.strip().lower()] = v.strip()
    return {
        "host": parts.get("host", "localhost"),
        "port": int(parts.get("port", "5432")),
        "dbname": parts.get("database"),
        "user": parts.get("username") or parts.get("user id") or parts.get("userid"),
        "password": parts.get("password"),
    }


def main() -> int:
    cfg = npgsql_to_psycopg2(load_conn_string())
    conn = psycopg2.connect(**cfg)
    conn.set_session(readonly=True, autocommit=True)
    cur = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)

    cmd = sys.argv[1] if len(sys.argv) > 1 else "info"

    if cmd == "tables":
        cur.execute(
            """
            SELECT table_name FROM information_schema.tables
            WHERE table_schema='public' ORDER BY table_name
            """
        )
        for r in cur.fetchall():
            print(r["table_name"])
        return 0

    if cmd == "columns":
        table = sys.argv[2]
        cur.execute(
            """
            SELECT column_name, data_type FROM information_schema.columns
            WHERE table_schema='public' AND table_name=%s ORDER BY ordinal_position
            """,
            (table,),
        )
        for r in cur.fetchall():
            print(f"{r['column_name']}\t{r['data_type']}")
        return 0

    if cmd == "baronies":
        cur.execute('SELECT * FROM "Baronies" ORDER BY "Id"')
        for r in cur.fetchall():
            print(r.get("Id"), "|", r.get("Name"), "|", "CharacterId=", r.get("CharacterId"))
        return 0

    if cmd == "query":
        sql = sys.argv[2]
        cur.execute(sql)
        rows = cur.fetchall()
        print(json.dumps(rows, default=str, ensure_ascii=False, indent=2))
        return 0

    if cmd == "export":
        barony_id = int(sys.argv[2]) if len(sys.argv) > 2 else 2
        out_path = sys.argv[3] if len(sys.argv) > 3 else "/tmp/darkhold_export.json"

        def rows(sql, params):
            cur.execute(sql, params)
            return cur.fetchall()

        doc = {}
        doc["Barony"] = rows('SELECT * FROM "Baronies" WHERE "Id"=%s', (barony_id,))
        doc["TerrainMapDomains"] = rows(
            'SELECT * FROM "TerrainMapDomains" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["Fiefs"] = rows(
            'SELECT * FROM "Fiefs" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["TerrainTiles"] = rows(
            'SELECT * FROM "TerrainTiles" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["TerrainImprovements"] = rows(
            'SELECT * FROM "TerrainImprovements" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["BaronySeats"] = rows(
            'SELECT * FROM "BaronySeats" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        seat_ids = tuple(r["Id"] for r in doc["BaronySeats"]) or (0,)
        doc["SeatRooms"] = rows(
            'SELECT * FROM "SeatRooms" WHERE "SeatId" IN %s ORDER BY "Id"', (seat_ids,)
        )
        room_ids = tuple(r["Id"] for r in doc["SeatRooms"]) or (0,)
        doc["SeatRoomTraits"] = rows(
            'SELECT * FROM "SeatRoomTraits" WHERE "RoomId" IN %s ORDER BY "Id"', (room_ids,)
        )
        doc["SeatTiles"] = rows(
            'SELECT * FROM "SeatTiles" WHERE "SeatId" IN %s ORDER BY "Id"', (seat_ids,)
        )
        doc["BaronyRelations"] = rows(
            'SELECT * FROM "BaronyRelations" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        rel_ids = tuple(r["Id"] for r in doc["BaronyRelations"]) or (0,)
        doc["BaronyRelationModifiers"] = rows(
            'SELECT * FROM "BaronyRelationModifiers" WHERE "RelationId" IN %s ORDER BY "Id"', (rel_ids,)
        )
        doc["AvailableAdvisors"] = rows(
            'SELECT * FROM "AvailableAdvisors" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["Advisors"] = rows(
            'SELECT * FROM "Advisors" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["BaronyBuildings"] = rows(
            'SELECT * FROM "BaronyBuildings" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )
        doc["BaronyUnits"] = rows(
            'SELECT * FROM "BaronyUnits" WHERE "BaronyId"=%s ORDER BY "Id"', (barony_id,)
        )

        with open(out_path, "w", encoding="utf-8") as fh:
            json.dump(doc, fh, default=str, ensure_ascii=False, indent=2)
        for k, v in doc.items():
            print(f"{k}: {len(v)}")
        print(f"written: {out_path}")
        return 0

    print("usage: darkhold_dump.py [tables|columns <t>|baronies|query <sql>|export <baronyId> <out>]")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
