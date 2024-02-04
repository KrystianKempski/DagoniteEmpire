using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAttributes7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkills_BaseSkills_BaseSkillId",
                table: "SpecialSkills");

            migrationBuilder.DropIndex(
                name: "IX_SpecialSkills_BaseSkillId",
                table: "SpecialSkills");

            migrationBuilder.DropColumn(
                name: "BaseSkillId",
                table: "SpecialSkills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseSkillId",
                table: "SpecialSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialSkills_BaseSkillId",
                table: "SpecialSkills",
                column: "BaseSkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkills_BaseSkills_BaseSkillId",
                table: "SpecialSkills",
                column: "BaseSkillId",
                principalTable: "BaseSkills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
