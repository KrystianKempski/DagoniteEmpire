using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723130000_AddBaronyProjectTileConstruction")]
    public partial class AddBaronyProjectTileConstruction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects"
                    ADD COLUMN IF NOT EXISTS "TileId" integer NULL;
                ALTER TABLE "BaronyProjects"
                    ADD COLUMN IF NOT EXISTS "BuildingTemplateId" integer NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "TileId";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "BuildingTemplateId";
                """);
        }
    }
}
