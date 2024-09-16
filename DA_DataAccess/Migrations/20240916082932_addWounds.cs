using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addWounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    IsIgnored = table.Column<bool>(type: "boolean", nullable: false),
                    IsTended = table.Column<bool>(type: "boolean", nullable: false),
                    IsMagicHealed = table.Column<bool>(type: "boolean", nullable: false),
                    DateMonth = table.Column<int>(type: "integer", nullable: false),
                    DateDay = table.Column<int>(type: "integer", nullable: false),
                    HealTime = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wounds_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wounds_CharacterId",
                table: "Wounds",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wounds");
        }
    }
}
