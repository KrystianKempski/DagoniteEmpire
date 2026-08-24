using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260824120000_AddAvailableAdvisorDuty")]
    public partial class AddAvailableAdvisorDuty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "DutyKind" text NOT NULL DEFAULT 'None';

                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "DutyCustomName" text NULL;

                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "DutyOfficeType" text NULL;

                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "DutyUnitId" integer NULL;

                ALTER TABLE "AvailableAdvisors"
                ADD COLUMN IF NOT EXISTS "SalaryGold" numeric NOT NULL DEFAULT 3;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyKind";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyCustomName";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyOfficeType";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyUnitId";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "SalaryGold";
                """);
        }
    }
}
