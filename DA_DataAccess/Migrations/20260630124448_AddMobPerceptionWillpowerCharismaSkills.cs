using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMobPerceptionWillpowerCharismaSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CharismaSkillValue",
                table: "Mobs",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "PerceptionSkillValue",
                table: "Mobs",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "WillpowerSkillValue",
                table: "Mobs",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharismaSkillValue",
                table: "Mobs");

            migrationBuilder.DropColumn(
                name: "PerceptionSkillValue",
                table: "Mobs");

            migrationBuilder.DropColumn(
                name: "WillpowerSkillValue",
                table: "Mobs");
        }
    }
}
