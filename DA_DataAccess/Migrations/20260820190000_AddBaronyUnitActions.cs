using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260820190000_AddBaronyUnitActions")]
    public partial class AddBaronyUnitActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "CaptainIsBaron" boolean NOT NULL DEFAULT FALSE;

                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "CurrentAction" text NOT NULL DEFAULT '';

                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "ActionTrainingJc" integer NOT NULL DEFAULT 0;

                ALTER TABLE "BaronyUnits"
                ADD COLUMN IF NOT EXISTS "ActionDemobilizeTroops" integer NOT NULL DEFAULT 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "ActionDemobilizeTroops";
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "ActionTrainingJc";
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "CurrentAction";
                ALTER TABLE "BaronyUnits" DROP COLUMN IF EXISTS "CaptainIsBaron";
                """);
        }
    }
}
