using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260826140000_AddBaronyBaronFocusPpbJson")]
    public partial class AddBaronyBaronFocusPpbJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                ADD COLUMN IF NOT EXISTS "BaronFocusPpbJson" text NOT NULL DEFAULT '["Food","Economy","Production","Loyalty","Stability"]';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "BaronFocusPpbJson";
                """);
        }
    }
}
