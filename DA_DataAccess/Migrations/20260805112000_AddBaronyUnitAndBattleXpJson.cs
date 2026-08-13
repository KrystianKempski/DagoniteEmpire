using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260805112000_AddBaronyUnitAndBattleXpJson")]
    public partial class AddBaronyUnitAndBattleXpJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "LogJson" text NOT NULL DEFAULT '[]';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBattleMaps"
                ADD COLUMN IF NOT EXISTS "TalliesJson" text NOT NULL DEFAULT '[]';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBattleMaps"
                ADD COLUMN IF NOT EXISTS "XpSummaryJson" text NOT NULL DEFAULT 'null';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "LogJson";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBattleMaps" DROP COLUMN IF EXISTS "TalliesJson";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBattleMaps" DROP COLUMN IF EXISTS "XpSummaryJson";
                """);
        }
    }
}
