using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PerBaronyTerrainMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TerrainMapDomains" ADD COLUMN IF NOT EXISTS "BaronyId" integer NULL;

                UPDATE "TerrainMapDomains"
                SET "BaronyId" = "LinkedBaronyId"
                WHERE "BaronyId" IS NULL AND "LinkedBaronyId" IS NOT NULL;

                DELETE FROM "TerrainMapDomains" WHERE "BaronyId" IS NULL;

                ALTER TABLE "TerrainMapDomains" ALTER COLUMN "BaronyId" SET NOT NULL;

                ALTER TABLE "TerrainMapDomains" DROP COLUMN IF EXISTS "LinkedBaronyId";

                DELETE FROM "TerrainTiles" t
                USING "TerrainTiles" t2
                WHERE t."BaronyId" = t2."BaronyId"
                  AND t."X" = t2."X"
                  AND t."Y" = t2."Y"
                  AND t."Id" < t2."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TerrainTiles_BaronyId_X_Y",
                table: "TerrainTiles",
                columns: new[] { "BaronyId", "X", "Y" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TerrainTiles_BaronyId_X_Y",
                table: "TerrainTiles");

            migrationBuilder.Sql("""
                ALTER TABLE "TerrainMapDomains" ADD COLUMN IF NOT EXISTS "LinkedBaronyId" integer NULL;

                UPDATE "TerrainMapDomains"
                SET "LinkedBaronyId" = "BaronyId"
                WHERE "IsPrimary" = true;

                ALTER TABLE "TerrainMapDomains" DROP COLUMN IF EXISTS "BaronyId";
                """);
        }
    }
}
