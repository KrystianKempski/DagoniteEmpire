using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260818224500_AddBaronAudiencePetitionerIcon")]
    public partial class AddBaronAudiencePetitionerIcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences"
                ADD COLUMN IF NOT EXISTS "PetitionerIcon" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronAudiences" DROP COLUMN IF EXISTS "PetitionerIcon";
                """);
        }
    }
}
