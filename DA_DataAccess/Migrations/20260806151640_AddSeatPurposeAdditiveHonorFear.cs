using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatPurposeAdditiveHonorFear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditiveHonor",
                table: "SeatPurposeTemplates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdditiveFear",
                table: "SeatPurposeTemplates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditiveHonor",
                table: "SeatPurposeTemplates");

            migrationBuilder.DropColumn(
                name: "AdditiveFear",
                table: "SeatPurposeTemplates");
        }
    }
}
