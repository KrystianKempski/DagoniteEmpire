using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260726150000_AddBaronyPreviousTurnStock")]
    public partial class AddBaronyPreviousTurnStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "PreviousTurnStockJson" text NOT NULL DEFAULT '{}';

                -- Test-phase: wipe ledger; opening stock will be rebuilt on next Resolve.
                DELETE FROM "BaronyResourceSources";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "PreviousTurnStockJson";
                """);
        }
    }
}
