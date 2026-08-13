using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260811154000_AddBaronAudienceKind")]
    public partial class AddBaronAudienceKind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "Kind" text NOT NULL DEFAULT 'Audience';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "Kind";
                """);
        }
    }
}
