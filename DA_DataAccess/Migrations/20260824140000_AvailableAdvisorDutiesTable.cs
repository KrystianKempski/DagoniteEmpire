using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260824140000_AvailableAdvisorDutiesTable")]
    public partial class AvailableAdvisorDutiesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "AvailableAdvisorDuties" (
                    "Id" serial PRIMARY KEY,
                    "AvailableAdvisorId" integer NOT NULL,
                    "DutyKind" text NOT NULL DEFAULT 'None',
                    "DutyCustomName" text NULL,
                    "DutyOfficeType" text NULL,
                    "DutyUnitId" integer NULL,
                    "SalaryGold" numeric NOT NULL DEFAULT 3,
                    "SortOrder" integer NOT NULL DEFAULT 0,
                    CONSTRAINT "FK_AvailableAdvisorDuties_AvailableAdvisors_AvailableAdvisorId"
                        FOREIGN KEY ("AvailableAdvisorId") REFERENCES "AvailableAdvisors" ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_AvailableAdvisorDuties_AvailableAdvisorId"
                    ON "AvailableAdvisorDuties" ("AvailableAdvisorId");

                INSERT INTO "AvailableAdvisorDuties"
                    ("AvailableAdvisorId", "DutyKind", "DutyCustomName", "DutyOfficeType", "DutyUnitId", "SalaryGold", "SortOrder")
                SELECT "Id",
                       COALESCE(NULLIF("DutyKind", ''), 'None'),
                       "DutyCustomName",
                       "DutyOfficeType",
                       "DutyUnitId",
                       COALESCE("SalaryGold", 3),
                       0
                FROM "AvailableAdvisors"
                WHERE COALESCE("DutyKind", 'None') <> 'None'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AvailableAdvisorDuties" d
                      WHERE d."AvailableAdvisorId" = "AvailableAdvisors"."Id"
                  );

                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyKind";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyCustomName";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyOfficeType";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "DutyUnitId";
                ALTER TABLE "AvailableAdvisors" DROP COLUMN IF EXISTS "SalaryGold";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

                UPDATE "AvailableAdvisors" a
                SET "DutyKind" = d."DutyKind",
                    "DutyCustomName" = d."DutyCustomName",
                    "DutyOfficeType" = d."DutyOfficeType",
                    "DutyUnitId" = d."DutyUnitId",
                    "SalaryGold" = d."SalaryGold"
                FROM (
                    SELECT DISTINCT ON ("AvailableAdvisorId") *
                    FROM "AvailableAdvisorDuties"
                    ORDER BY "AvailableAdvisorId", "SortOrder", "Id"
                ) d
                WHERE a."Id" = d."AvailableAdvisorId";

                DROP TABLE IF EXISTS "AvailableAdvisorDuties";
                """);
        }
    }
}
