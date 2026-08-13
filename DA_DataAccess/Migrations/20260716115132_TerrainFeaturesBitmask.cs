using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TerrainFeaturesBitmask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FeaturesMask",
                table: "TerrainTiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Convert legacy CSV feature names into bit flags:
            // Forest=1, Coast=2, River=4, Swamp=8, Wasteland=16
            migrationBuilder.Sql("""
                UPDATE "TerrainTiles"
                SET "FeaturesMask" =
                    CASE WHEN "FeaturesCsv" ~ '(^|,)\\s*(Las|Forest)\\s*(,|$)' THEN 1 ELSE 0 END
                  + CASE WHEN "FeaturesCsv" ~ '(^|,)\\s*(Wybrzeże|Coast)\\s*(,|$)' THEN 2 ELSE 0 END
                  + CASE WHEN "FeaturesCsv" ~ '(^|,)\\s*(Rzeka|River)\\s*(,|$)' THEN 4 ELSE 0 END
                  + CASE WHEN "FeaturesCsv" ~ '(^|,)\\s*(Bagna|Swamp)\\s*(,|$)' THEN 8 ELSE 0 END
                  + CASE WHEN "FeaturesCsv" ~ '(^|,)\\s*(Pustkowie|Wasteland)\\s*(,|$)' THEN 16 ELSE 0 END
                WHERE "FeaturesCsv" IS NOT NULL AND "FeaturesCsv" <> '';
                """);

            migrationBuilder.DropColumn(
                name: "FeaturesCsv",
                table: "TerrainTiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturesCsv",
                table: "TerrainTiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "TerrainTiles"
                SET "FeaturesCsv" = TRIM(BOTH ',' FROM CONCAT_WS(',',
                    CASE WHEN ("FeaturesMask" & 1) <> 0 THEN 'Las' END,
                    CASE WHEN ("FeaturesMask" & 2) <> 0 THEN 'Wybrzeże' END,
                    CASE WHEN ("FeaturesMask" & 4) <> 0 THEN 'Rzeka' END,
                    CASE WHEN ("FeaturesMask" & 8) <> 0 THEN 'Bagna' END,
                    CASE WHEN ("FeaturesMask" & 16) <> 0 THEN 'Pustkowie' END
                ));
                """);

            migrationBuilder.DropColumn(
                name: "FeaturesMask",
                table: "TerrainTiles");
        }
    }
}
