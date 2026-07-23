using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723120000_AddBaronLetterMessageSeenByGm")]
    public partial class AddBaronLetterMessageSeenByGm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetterMessages"
                    ADD COLUMN IF NOT EXISTS "SeenByGm" boolean NOT NULL DEFAULT TRUE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetterMessages" DROP COLUMN IF EXISTS "SeenByGm";
                """);
        }
    }
}
