using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBaronyEventTurnRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StartTurn",
                table: "BaronyEvents",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "EndTurn",
                table: "BaronyEvents",
                type: "integer",
                nullable: true);

            // Preserve prior TurnNumber / IsActive: active → ongoing from that turn; inactive → ended on that turn.
            migrationBuilder.Sql("""
                UPDATE "BaronyEvents"
                SET "StartTurn" = "TurnNumber",
                    "EndTurn" = CASE WHEN "IsActive" THEN NULL ELSE "TurnNumber" END;
                """);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BaronyEvents");

            migrationBuilder.DropColumn(
                name: "TurnNumber",
                table: "BaronyEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurnNumber",
                table: "BaronyEvents",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BaronyEvents",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE "BaronyEvents"
                SET "TurnNumber" = "StartTurn",
                    "IsActive" = ("EndTurn" IS NULL OR "EndTurn" >= "StartTurn");
                """);

            migrationBuilder.DropColumn(
                name: "EndTurn",
                table: "BaronyEvents");

            migrationBuilder.DropColumn(
                name: "StartTurn",
                table: "BaronyEvents");
        }
    }
}
