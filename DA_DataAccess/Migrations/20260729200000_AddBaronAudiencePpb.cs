using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260729200000_AddBaronAudiencePpb")]
    public partial class AddBaronAudiencePpb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "AdditiveJson" text NOT NULL DEFAULT '[]';

                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "PercentJson" text NOT NULL DEFAULT '[]';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "AdditiveJson";
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "PercentJson";
                """);
        }
    }
}
