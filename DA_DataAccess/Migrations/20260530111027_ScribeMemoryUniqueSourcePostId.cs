using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ScribeMemoryUniqueSourcePostId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScribeMemories_SourcePostId",
                table: "ScribeMemories",
                column: "SourcePostId",
                unique: true,
                filter: "\"SourcePostId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScribeMemories_SourcePostId",
                table: "ScribeMemories");
        }
    }
}
