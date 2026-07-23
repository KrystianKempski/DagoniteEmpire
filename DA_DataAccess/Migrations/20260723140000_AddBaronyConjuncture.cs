using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723140000_AddBaronyConjuncture")]
    public partial class AddBaronyConjuncture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies"
                    ADD COLUMN IF NOT EXISTS "ConjunctureDice" integer NOT NULL DEFAULT 7;
                ALTER TABLE "Baronies"
                    ADD COLUMN IF NOT EXISTS "ConjunctureModifier" integer NOT NULL DEFAULT 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "ConjunctureDice";
                ALTER TABLE "Baronies" DROP COLUMN IF EXISTS "ConjunctureModifier";
                """);
        }
    }
}
