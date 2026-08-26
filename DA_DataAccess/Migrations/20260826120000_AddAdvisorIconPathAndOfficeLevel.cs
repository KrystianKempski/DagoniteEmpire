using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260826120000_AddAdvisorIconPathAndOfficeLevel")]
    public partial class AddAdvisorIconPathAndOfficeLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Advisors"
                ADD COLUMN IF NOT EXISTS "IconPath" text NULL;
                ALTER TABLE "Advisors"
                ADD COLUMN IF NOT EXISTS "OfficeLevel" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Advisors" DROP COLUMN IF EXISTS "IconPath";
                ALTER TABLE "Advisors" DROP COLUMN IF EXISTS "OfficeLevel";
                """);
        }
    }
}
