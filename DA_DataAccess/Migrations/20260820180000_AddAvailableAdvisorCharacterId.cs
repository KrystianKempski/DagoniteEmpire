using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260820180000_AddAvailableAdvisorCharacterId")]
    public partial class AddAvailableAdvisorCharacterId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "CharacterId" integer NULL;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_AvailableAdvisors_CharacterId"
                ON "AvailableAdvisors" ("CharacterId")
                WHERE "CharacterId" IS NOT NULL;

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_AvailableAdvisors_BaronyId_CharacterId"
                ON "AvailableAdvisors" ("BaronyId", "CharacterId")
                WHERE "CharacterId" IS NOT NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_AvailableAdvisors_BaronyId_CharacterId";
                DROP INDEX IF EXISTS "IX_AvailableAdvisors_CharacterId";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "CharacterId";
                """);
        }
    }
}
