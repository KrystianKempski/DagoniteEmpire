using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFiefColorAndSeniorDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Fiefs",
                type: "text",
                nullable: false,
                defaultValue: "#4d7ea8");

            migrationBuilder.AddColumn<int>(
                name: "SeniorDomainId",
                table: "Fiefs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Fiefs");

            migrationBuilder.DropColumn(
                name: "SeniorDomainId",
                table: "Fiefs");
        }
    }
}
