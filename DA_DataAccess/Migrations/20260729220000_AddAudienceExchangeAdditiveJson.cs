using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260729220000_AddAudienceExchangeAdditiveJson")]
    public partial class AddAudienceExchangeAdditiveJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudienceExchanges"
                ADD COLUMN IF NOT EXISTS "AdditiveJson" text NOT NULL DEFAULT '[]';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudienceExchanges" DROP COLUMN IF EXISTS "AdditiveJson";
                """);
        }
    }
}
