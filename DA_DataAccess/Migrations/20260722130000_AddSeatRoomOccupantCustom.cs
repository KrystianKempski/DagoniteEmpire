using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260722130000_AddSeatRoomOccupantCustom")]
    /// <inheritdoc />
    public partial class AddSeatRoomOccupantCustom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SeatRooms"
                ADD COLUMN IF NOT EXISTS "OccupantCustom" text NOT NULL DEFAULT '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SeatRooms"
                DROP COLUMN IF EXISTS "OccupantCustom";
                """);
        }
    }
}
