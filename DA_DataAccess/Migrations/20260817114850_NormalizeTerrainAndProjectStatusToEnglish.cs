using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTerrainAndProjectStatusToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Water'     WHERE ""BaseType"" = 'Woda';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Plains'    WHERE ""BaseType"" = 'Równiny';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Hills'     WHERE ""BaseType"" = 'Wzgórza';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Mountains' WHERE ""BaseType"" = 'Góry';

                UPDATE ""BaronyProjects"" SET ""Status"" = 'Draft'               WHERE ""Status"" = 'Szkic';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Resource allocation'  WHERE ""Status"" = 'Alokacja zasobów';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'In progress'          WHERE ""Status"" = 'W trakcie';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Completed'            WHERE ""Status"" = 'Zakończony';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Cancelled'            WHERE ""Status"" = 'Anulowany';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Woda'    WHERE ""BaseType"" = 'Water';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Równiny' WHERE ""BaseType"" = 'Plains';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Wzgórza' WHERE ""BaseType"" = 'Hills';
                UPDATE ""TerrainTiles"" SET ""BaseType"" = 'Góry'    WHERE ""BaseType"" = 'Mountains';

                UPDATE ""BaronyProjects"" SET ""Status"" = 'Szkic'            WHERE ""Status"" = 'Draft';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Alokacja zasobów' WHERE ""Status"" = 'Resource allocation';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'W trakcie'        WHERE ""Status"" = 'In progress';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Zakończony'       WHERE ""Status"" = 'Completed';
                UPDATE ""BaronyProjects"" SET ""Status"" = 'Anulowany'        WHERE ""Status"" = 'Cancelled';
            ");
        }
    }
}
