using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260805194000_AddCourtCharacterSheet")]
    public partial class AddCourtCharacterSheet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy pool people used raw PPB vectors — wipe and start from court sheets.
            migrationBuilder.Sql("""
                UPDATE "Advisors"
                SET "AvailableAdvisorId" = NULL
                WHERE "AvailableAdvisorId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM "AvailableAdvisors";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "SheetJson" text NOT NULL DEFAULT '{}';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "SheetJson";
                """);
        }
    }
}
