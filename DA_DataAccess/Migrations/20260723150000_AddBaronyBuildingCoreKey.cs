using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723150000_AddBaronyBuildingCoreKey")]
    public partial class AddBaronyBuildingCoreKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBuildings"
                    ADD COLUMN IF NOT EXISTS "CoreKey" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyBuildings" DROP COLUMN IF EXISTS "CoreKey";
                """);
        }
    }
}
