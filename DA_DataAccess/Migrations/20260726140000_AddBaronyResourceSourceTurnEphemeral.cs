using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260726140000_AddBaronyResourceSourceTurnEphemeral")]
    public partial class AddBaronyResourceSourceTurnEphemeral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyResourceSources"
                ADD COLUMN IF NOT EXISTS "IsTurnEphemeral" boolean NOT NULL DEFAULT false;

                ALTER TABLE "BaronyResourceSources"
                ADD COLUMN IF NOT EXISTS "VisibleOnTurn" integer NULL;

                -- Test-phase cleanup: drop old one-time project grant rows (replaced by turn-ephemeral).
                DELETE FROM "BaronyResourceSources"
                WHERE "Description" ILIKE '%One-time resources from completed project%';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyResourceSources" DROP COLUMN IF EXISTS "VisibleOnTurn";
                ALTER TABLE "BaronyResourceSources" DROP COLUMN IF EXISTS "IsTurnEphemeral";
                """);
        }
    }
}
