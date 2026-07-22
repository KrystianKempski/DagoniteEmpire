using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260722232000_AddBaronLetterReplySeenByBaron")]
    public partial class AddBaronLetterReplySeenByBaron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetters"
                    ADD COLUMN IF NOT EXISTS "ReplySeenByBaron" boolean NOT NULL DEFAULT TRUE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetters" DROP COLUMN IF EXISTS "ReplySeenByBaron";
                """);
        }
    }
}
