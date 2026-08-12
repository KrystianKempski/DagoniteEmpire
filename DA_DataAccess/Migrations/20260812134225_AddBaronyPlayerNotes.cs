using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBaronyPlayerNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaronyPlayerNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    NoteType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    BodyHtml = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    DueTurn = table.Column<int>(type: "integer", nullable: true),
                    CreatedTurn = table.Column<int>(type: "integer", nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaronyPlayerNotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaronyPlayerNotes_BaronyId",
                table: "BaronyPlayerNotes",
                column: "BaronyId");

            migrationBuilder.CreateIndex(
                name: "IX_BaronyPlayerNotes_BaronyId_NoteType",
                table: "BaronyPlayerNotes",
                columns: new[] { "BaronyId", "NoteType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaronyPlayerNotes");
        }
    }
}
