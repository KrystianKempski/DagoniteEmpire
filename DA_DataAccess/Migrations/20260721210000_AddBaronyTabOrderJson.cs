using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260721210000_AddBaronyTabOrderJson")]
    public partial class AddBaronyTabOrderJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUsers"
                ADD COLUMN IF NOT EXISTS "BaronyTabOrderJson" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUsers"
                DROP COLUMN IF EXISTS "BaronyTabOrderJson";
                """);
        }
    }
}
