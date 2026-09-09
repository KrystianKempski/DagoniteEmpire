using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260909190000_AddBaronyProjectAllocatedAtTurnStart")]
    public partial class AddBaronyProjectAllocatedAtTurnStart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects"
                    ADD COLUMN IF NOT EXISTS "AllocatedAtTurnStartJson" text NOT NULL DEFAULT '{}';

                -- Existing open projects: prior funding is already inside PreviousTurnStock after
                -- Resolve, so treat current Allocated as the turn-start baseline (zero this-turn cost).
                UPDATE "BaronyProjects"
                SET "AllocatedAtTurnStartJson" = "AllocatedJson"
                WHERE "Status" IS DISTINCT FROM 'Completed'
                  AND "Status" IS DISTINCT FROM 'Cancelled';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "AllocatedAtTurnStartJson";
                """);
        }
    }
}
