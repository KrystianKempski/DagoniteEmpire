using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateSkills99 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtherBonuses",
                table: "SpecialSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TempBonuses",
                table: "SpecialSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OtherBonuses",
                table: "BaseSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TempBonuses",
                table: "BaseSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OtherBonuses",
                table: "Attributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TempBonuses",
                table: "Attributes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherBonuses",
                table: "SpecialSkills");

            migrationBuilder.DropColumn(
                name: "TempBonuses",
                table: "SpecialSkills");

            migrationBuilder.DropColumn(
                name: "OtherBonuses",
                table: "BaseSkills");

            migrationBuilder.DropColumn(
                name: "TempBonuses",
                table: "BaseSkills");

            migrationBuilder.DropColumn(
                name: "OtherBonuses",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "TempBonuses",
                table: "Attributes");
        }
    }
}
