using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fixTraits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SummaryDescr",
                table: "Traits",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryDescr",
                table: "Traits");
        }
    }
}
