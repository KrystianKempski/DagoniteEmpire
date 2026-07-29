using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260730120000_AddAudiencePhp")]
    public partial class AddAudiencePhp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "Prestige" integer NOT NULL DEFAULT 0;
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "Honor" integer NOT NULL DEFAULT 0;
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "Fear" integer NOT NULL DEFAULT 0;

                ALTER TABLE "BaronAudienceExchanges"
                ADD COLUMN IF NOT EXISTS "Prestige" integer NOT NULL DEFAULT 0;
                ALTER TABLE "BaronAudienceExchanges"
                ADD COLUMN IF NOT EXISTS "Honor" integer NOT NULL DEFAULT 0;
                ALTER TABLE "BaronAudienceExchanges"
                ADD COLUMN IF NOT EXISTS "Fear" integer NOT NULL DEFAULT 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "Prestige";
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "Honor";
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "Fear";
                ALTER TABLE "BaronAudienceExchanges" DROP COLUMN IF EXISTS "Prestige";
                ALTER TABLE "BaronAudienceExchanges" DROP COLUMN IF EXISTS "Honor";
                ALTER TABLE "BaronAudienceExchanges" DROP COLUMN IF EXISTS "Fear";
                """);
        }
    }
}
