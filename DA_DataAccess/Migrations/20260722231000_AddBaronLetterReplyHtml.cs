using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260722231000_AddBaronLetterReplyHtml")]
    public partial class AddBaronLetterReplyHtml : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetters"
                    ADD COLUMN IF NOT EXISTS "ReplyHtml" text NULL;

                ALTER TABLE "BaronLetters"
                    ADD COLUMN IF NOT EXISTS "ReplyYear" integer NULL;

                ALTER TABLE "BaronLetters"
                    ADD COLUMN IF NOT EXISTS "ReplyMonth" integer NULL;

                ALTER TABLE "BaronLetters"
                    ADD COLUMN IF NOT EXISTS "ReplySeason" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronLetters" DROP COLUMN IF EXISTS "ReplyHtml";
                ALTER TABLE "BaronLetters" DROP COLUMN IF EXISTS "ReplyYear";
                ALTER TABLE "BaronLetters" DROP COLUMN IF EXISTS "ReplyMonth";
                ALTER TABLE "BaronLetters" DROP COLUMN IF EXISTS "ReplySeason";
                """);
        }
    }
}
