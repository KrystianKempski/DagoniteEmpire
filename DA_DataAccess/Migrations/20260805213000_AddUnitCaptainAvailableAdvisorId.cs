using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260805213000_AddUnitCaptainAvailableAdvisorId")]
    public partial class AddUnitCaptainAvailableAdvisorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "CaptainAvailableAdvisorId" integer NULL;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_BaronyUnits_CaptainAvailableAdvisorId"
                ON "BaronyUnits" ("CaptainAvailableAdvisorId")
                WHERE "CaptainAvailableAdvisorId" IS NOT NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_BaronyUnits_CaptainAvailableAdvisorId";
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "CaptainAvailableAdvisorId";
                """);
        }
    }
}
