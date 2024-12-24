using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addGold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDay",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "CurrentMonth",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "CurrentYear",
                table: "Characters",
                newName: "DateNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "Gold",
                table: "Characters",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "WealthRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    DateNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WealthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WealthRecords_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WealthRecords_CharacterId",
                table: "WealthRecords",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WealthRecords");

            migrationBuilder.DropColumn(
                name: "Gold",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "DateNumber",
                table: "Characters",
                newName: "CurrentYear");

            migrationBuilder.AddColumn<int>(
                name: "CurrentDay",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentMonth",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
