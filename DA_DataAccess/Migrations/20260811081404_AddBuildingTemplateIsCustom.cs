using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingTemplateIsCustom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "BuildingTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "BuildingTemplates");
        }
    }
}
