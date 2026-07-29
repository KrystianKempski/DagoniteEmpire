using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260729230000_AddAudienceExchangeSpeakerName")]
    public partial class AddAudienceExchangeSpeakerName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudienceExchanges"
                ADD COLUMN IF NOT EXISTS "SpeakerName" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudienceExchanges" DROP COLUMN IF EXISTS "SpeakerName";
                """);
        }
    }
}
