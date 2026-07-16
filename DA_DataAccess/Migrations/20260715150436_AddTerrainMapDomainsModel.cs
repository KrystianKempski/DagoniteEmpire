using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTerrainMapDomainsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "TerrainMapDomains" (
                    "Id" serial PRIMARY KEY,
                    "Name" text NOT NULL DEFAULT '',
                    "LordName" text NOT NULL DEFAULT '',
                    "ColorHex" text NOT NULL DEFAULT '#888888',
                    "LinkedBaronyId" integer NULL,
                    "IsPrimary" boolean NOT NULL DEFAULT false,
                    "SortOrder" integer NOT NULL DEFAULT 0
                );

                ALTER TABLE "TerrainTiles" ADD COLUMN IF NOT EXISTS "MapId" integer NOT NULL DEFAULT 1;
                ALTER TABLE "TerrainTiles" ADD COLUMN IF NOT EXISTS "MapDomainId" integer NULL;

                UPDATE "TerrainTiles" SET "MapId" = 1 WHERE "MapId" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TerrainTiles" DROP COLUMN IF EXISTS "MapDomainId";
                ALTER TABLE "TerrainTiles" DROP COLUMN IF EXISTS "MapId";
                DROP TABLE IF EXISTS "TerrainMapDomains";
                """);
        }
    }
}
