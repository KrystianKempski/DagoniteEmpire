using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseSkill_Characters_CharacterID",
                table: "BaseSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterID",
                table: "SpecialSkill");

            migrationBuilder.RenameColumn(
                name: "CharacterID",
                table: "SpecialSkill",
                newName: "CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_SpecialSkill_CharacterID",
                table: "SpecialSkill",
                newName: "IX_SpecialSkill_CharacterId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Characters",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CharacterID",
                table: "BaseSkill",
                newName: "CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_BaseSkill_CharacterID",
                table: "BaseSkill",
                newName: "IX_BaseSkill_CharacterId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SpecialSkill",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BaseSkill",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Attributes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseSkill_Characters_CharacterId",
                table: "BaseSkill");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterId",
                table: "SpecialSkill");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "CharacterId",
                table: "SpecialSkill",
                newName: "CharacterID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecialSkill_CharacterId",
                table: "SpecialSkill",
                newName: "IX_SpecialSkill_CharacterID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Characters",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "CharacterId",
                table: "BaseSkill",
                newName: "CharacterID");

            migrationBuilder.RenameIndex(
                name: "IX_BaseSkill_CharacterId",
                table: "BaseSkill",
                newName: "IX_BaseSkill_CharacterID");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SpecialSkill",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BaseSkill",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Attributes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseSkill_Characters_CharacterID",
                table: "BaseSkill",
                column: "CharacterID",
                principalTable: "Characters",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialSkill_Characters_CharacterID",
                table: "SpecialSkill",
                column: "CharacterID",
                principalTable: "Characters",
                principalColumn: "ID");
        }
    }
}
