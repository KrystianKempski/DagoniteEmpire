using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeWoundDayMonthYearToDateNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDay",
                table: "Wounds");

            migrationBuilder.DropColumn(
                name: "DateMonth",
                table: "Wounds");

            migrationBuilder.RenameColumn(
                name: "DateYear",
                table: "Wounds",
                newName: "DateNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateNumber",
                table: "Wounds",
                newName: "DateYear");

            migrationBuilder.AddColumn<int>(
                name: "DateDay",
                table: "Wounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DateMonth",
                table: "Wounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
