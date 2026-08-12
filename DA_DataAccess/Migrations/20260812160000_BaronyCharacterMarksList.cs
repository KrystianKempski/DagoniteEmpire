using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260812160000_BaronyCharacterMarksList")]
    public partial class BaronyCharacterMarksList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "MarksJson" text NOT NULL DEFAULT '[]';

                UPDATE "BaronyRelations"
                SET "MarksJson" = json_build_array(
                    json_build_object('IconKey', "MarkIconKey", 'ColorKey', "MarkColorKey")
                )::text
                WHERE "MarkIconKey" IS NOT NULL
                  AND "MarkColorKey" IS NOT NULL
                  AND ("MarksJson" IS NULL OR "MarksJson" = '[]');

                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "MarkIconKey";
                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "MarkColorKey";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "MarkIconKey" text NULL;

                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "MarkColorKey" text NULL;

                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "MarksJson";
                """);
        }
    }
}
