using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingTemplateLordshipLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredLordshipLevel",
                table: "BuildingTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredLordshipLevel",
                table: "BuildingTemplates");
        }
    }
}
