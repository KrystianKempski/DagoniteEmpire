using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260722180000_RenameSeatTileSpaceToWater")]
    public partial class RenameSeatTileSpaceToWater : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SeatTiles"
                SET "Kind" = 'Water'
                WHERE "Kind" = 'Space';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SeatTiles"
                SET "Kind" = 'Space'
                WHERE "Kind" = 'Water';
                """);
        }
    }
}
