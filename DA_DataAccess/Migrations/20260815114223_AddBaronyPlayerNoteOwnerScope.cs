using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBaronyPlayerNoteOwnerScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaronyPlayerNotes_BaronyId_NoteType",
                table: "BaronyPlayerNotes");

            migrationBuilder.AddColumn<string>(
                name: "OwnerScope",
                table: "BaronyPlayerNotes",
                type: "text",
                nullable: false,
                defaultValue: "player");

            migrationBuilder.CreateIndex(
                name: "IX_BaronyPlayerNotes_BaronyId_OwnerScope_NoteType",
                table: "BaronyPlayerNotes",
                columns: new[] { "BaronyId", "OwnerScope", "NoteType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaronyPlayerNotes_BaronyId_OwnerScope_NoteType",
                table: "BaronyPlayerNotes");

            migrationBuilder.DropColumn(
                name: "OwnerScope",
                table: "BaronyPlayerNotes");

            migrationBuilder.CreateIndex(
                name: "IX_BaronyPlayerNotes_BaronyId_NoteType",
                table: "BaronyPlayerNotes",
                columns: new[] { "BaronyId", "NoteType" });
        }
    }
}
