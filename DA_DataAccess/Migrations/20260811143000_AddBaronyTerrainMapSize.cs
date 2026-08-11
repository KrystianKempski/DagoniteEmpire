using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260811143000_AddBaronyTerrainMapSize")]
    public partial class AddBaronyTerrainMapSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "TerrainMapWidth" integer NOT NULL DEFAULT 15;

                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "TerrainMapHeight" integer NOT NULL DEFAULT 15;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "TerrainMapWidth";
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "TerrainMapHeight";
                """);
        }
    }
}
