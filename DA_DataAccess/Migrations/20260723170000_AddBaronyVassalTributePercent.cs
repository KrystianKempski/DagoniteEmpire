using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723170000_AddBaronyVassalTributePercent")]
    public partial class AddBaronyVassalTributePercent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                    ADD COLUMN IF NOT EXISTS "VassalTributePercent" numeric NOT NULL DEFAULT 15;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "VassalTributePercent";
                """);
        }
    }
}
