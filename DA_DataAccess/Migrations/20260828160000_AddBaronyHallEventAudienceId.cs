using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260828160000_AddBaronyHallEventAudienceId")]
    public partial class AddBaronyHallEventAudienceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyHallEvents"
                    ADD COLUMN IF NOT EXISTS "AudienceId" integer NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyHallEvents"
                    DROP COLUMN IF EXISTS "AudienceId";
                """);
        }
    }
}
