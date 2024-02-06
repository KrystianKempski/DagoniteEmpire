using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "SpecialSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialSkills_CharacterId",
                table: "SpecialSkills",
                column: "CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkills_Characters_CharacterId",
                table: "SpecialSkills",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkills_Characters_CharacterId",
                table: "SpecialSkills");

            migrationBuilder.DropIndex(
                name: "IX_SpecialSkills_CharacterId",
                table: "SpecialSkills");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "SpecialSkills");
        }
    }
}
