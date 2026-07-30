using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260730160000_AddBaronyBattleMap")]
    public partial class AddBaronyBattleMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "BaronyBattleMaps" (
                    "Id" serial NOT NULL,
                    "BaronyId" integer NOT NULL,
                    "IsActive" boolean NOT NULL DEFAULT false,
                    "Phase" text NOT NULL DEFAULT 'setup',
                    "Width" integer NOT NULL DEFAULT 20,
                    "Height" integer NOT NULL DEFAULT 20,
                    "CellsJson" text NOT NULL DEFAULT '[]',
                    "TokensJson" text NOT NULL DEFAULT '[]',
                    "TurnStateJson" text NOT NULL DEFAULT '{}',
                    "LogJson" text NOT NULL DEFAULT '[]',
                    CONSTRAINT "PK_BaronyBattleMaps" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_BaronyBattleMaps_BaronyId"
                    ON "BaronyBattleMaps" ("BaronyId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "BaronyBattleMaps";""");
        }
    }
}
