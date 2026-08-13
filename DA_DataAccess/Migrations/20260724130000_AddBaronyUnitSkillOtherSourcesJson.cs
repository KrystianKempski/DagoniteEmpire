using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260724130000_AddBaronyUnitSkillOtherSourcesJson")]
    public partial class AddBaronyUnitSkillOtherSourcesJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "SkillOtherSourcesJson" text NOT NULL DEFAULT '{}';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "SkillOtherSourcesJson";
                """);
        }
    }
}
