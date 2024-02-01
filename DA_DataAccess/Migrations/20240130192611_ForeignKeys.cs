using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseSkill_Characters_CharacterId",
                table: "BaseSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterId",
                table: "SpecialSkill");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterId",
                table: "SpecialSkill",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CharacterId",
                table: "BaseSkill",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "Attributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_CharacterId",
                table: "Attributes",
                column: "CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attributes_Characters_CharacterId",
                table: "Attributes",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseSkill_Characters_CharacterId",
                table: "BaseSkill",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterId",
                table: "SpecialSkill",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attributes_Characters_CharacterId",
                table: "Attributes");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseSkill_Characters_CharacterId",
                table: "BaseSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterId",
                table: "SpecialSkill");

            migrationBuilder.DropIndex(
                name: "IX_Attributes_CharacterId",
                table: "Attributes");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "Attributes");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterId",
                table: "SpecialSkill",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CharacterId",
                table: "BaseSkill",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseSkill_Characters_CharacterId",
                table: "BaseSkill",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterId",
                table: "SpecialSkill",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");
        }
    }
}
