using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260726210000_AddBaronyProjectHideResultFromBaron")]
    public partial class AddBaronyProjectHideResultFromBaron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects"
                ADD COLUMN IF NOT EXISTS "HideResultFromBaron" boolean NOT NULL DEFAULT false;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "HideResultFromBaron";
                """);
        }
    }
}
