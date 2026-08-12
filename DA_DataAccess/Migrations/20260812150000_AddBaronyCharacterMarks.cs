using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260812150000_AddBaronyCharacterMarks")]
    public partial class AddBaronyCharacterMarks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "MarkIconKey" text NULL;

                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "MarkColorKey" text NULL;

                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "KnownLordMarksJson" text NOT NULL DEFAULT '{}';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "MarkIconKey";
                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "MarkColorKey";
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "KnownLordMarksJson";
                """);
        }
    }
}
