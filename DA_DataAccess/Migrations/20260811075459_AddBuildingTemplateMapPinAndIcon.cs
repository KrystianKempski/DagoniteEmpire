using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingTemplateMapPinAndIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "BuildingTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapPinKind",
                table: "BuildingTemplates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "BuildingTemplates");

            migrationBuilder.DropColumn(
                name: "MapPinKind",
                table: "BuildingTemplates");
        }
    }
}
