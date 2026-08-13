using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdvisorAssistantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssistantBonus",
                table: "Advisors");

            migrationBuilder.DropColumn(
                name: "AssistantImpactJson",
                table: "Advisors");

            migrationBuilder.DropColumn(
                name: "HasAssistant",
                table: "Advisors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssistantBonus",
                table: "Advisors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AssistantImpactJson",
                table: "Advisors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasAssistant",
                table: "Advisors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
