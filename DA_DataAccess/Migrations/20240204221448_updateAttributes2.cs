using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAttributes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkills_Attributes_AtributeId",
                table: "SpecialSkills");

            migrationBuilder.DropIndex(
                name: "IX_SpecialSkills_AtributeId",
                table: "SpecialSkills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SpecialSkills_AtributeId",
                table: "SpecialSkills",
                column: "AtributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkills_Attributes_AtributeId",
                table: "SpecialSkills",
                column: "AtributeId",
                principalTable: "Attributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
