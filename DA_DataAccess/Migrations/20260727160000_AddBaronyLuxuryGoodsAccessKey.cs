using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260727160000_AddBaronyLuxuryGoodsAccessKey")]
    public partial class AddBaronyLuxuryGoodsAccessKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "LuxuryGoodsAccessKey" text NOT NULL DEFAULT 'basic';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "LuxuryGoodsAccessKey";
                """);
        }
    }
}
