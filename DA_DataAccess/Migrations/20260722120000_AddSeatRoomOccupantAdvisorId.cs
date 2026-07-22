using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using DA_DataAccess.Data;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260722120000_AddSeatRoomOccupantAdvisorId")]
    /// <inheritdoc />
    public partial class AddSeatRoomOccupantAdvisorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SeatRooms"
                ADD COLUMN IF NOT EXISTS "OccupantAdvisorId" integer NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SeatRooms"
                DROP COLUMN IF EXISTS "OccupantAdvisorId";
                """);
        }
    }
}
