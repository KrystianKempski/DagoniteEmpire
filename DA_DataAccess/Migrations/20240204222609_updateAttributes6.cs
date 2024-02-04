using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAttributes6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkills_Characters_CharacterId",
                table: "SpecialSkills");

            migrationBuilder.RenameColumn(
                name: "CharacterId",
                table: "SpecialSkills",
                newName: "BaseSkillId");

            migrationBuilder.RenameIndex(
                name: "IX_SpecialSkills_CharacterId",
                table: "SpecialSkills",
                newName: "IX_SpecialSkills_BaseSkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkills_BaseSkills_BaseSkillId",
                table: "SpecialSkills",
                column: "BaseSkillId",
                principalTable: "BaseSkills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkills_BaseSkills_BaseSkillId",
                table: "SpecialSkills");

            migrationBuilder.RenameColumn(
                name: "BaseSkillId",
                table: "SpecialSkills",
                newName: "CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_SpecialSkills_BaseSkillId",
                table: "SpecialSkills",
                newName: "IX_SpecialSkills_CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkills_Characters_CharacterId",
                table: "SpecialSkills",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
