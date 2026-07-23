using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260723200000_AddBaronyUnits")]
    public partial class AddBaronyUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "BaronyUnits" (
                    "Id" serial PRIMARY KEY,
                    "BaronyId" integer NOT NULL,
                    "Name" text NOT NULL,
                    "Status" text NOT NULL,
                    "TroopCount" integer NOT NULL DEFAULT 50,
                    "RecruitSelectionKey" text NOT NULL,
                    "TrainingTypeKey" text NOT NULL,
                    "Wage" integer NOT NULL DEFAULT 0,
                    "UpkeepFood" numeric NOT NULL DEFAULT 0.5,
                    "UpkeepDefense" integer NOT NULL DEFAULT 5,
                    "Build" integer NOT NULL DEFAULT 0,
                    "Agility" integer NOT NULL DEFAULT 0,
                    "Will" integer NOT NULL DEFAULT 0,
                    "Perception" integer NOT NULL DEFAULT 0,
                    "AttrPenaltyBuild" integer NOT NULL DEFAULT 0,
                    "AttrPenaltyAgility" integer NOT NULL DEFAULT 0,
                    "AttrOtherBuild" integer NOT NULL DEFAULT 0,
                    "AttrOtherAgility" integer NOT NULL DEFAULT 0,
                    "AttrOtherWill" integer NOT NULL DEFAULT 0,
                    "AttrOtherPerception" integer NOT NULL DEFAULT 0,
                    "SkillsJson" text NOT NULL DEFAULT '{}',
                    "SkillOtherJson" text NOT NULL DEFAULT '{}',
                    "Weapon1Key" text NULL,
                    "Weapon2Key" text NULL,
                    "ArmorKey" text NULL,
                    "ShieldKey" text NULL,
                    "Weapon1Quality" text NOT NULL DEFAULT 'Normal',
                    "Weapon2Quality" text NOT NULL DEFAULT 'Normal',
                    "DefenseSkillKey" text NOT NULL DEFAULT 'shields',
                    "CommanderAttack" integer NOT NULL DEFAULT 0,
                    "CommanderDefense" integer NOT NULL DEFAULT 0,
                    "OtherAttack" integer NOT NULL DEFAULT 0,
                    "OtherDefense" integer NOT NULL DEFAULT 0,
                    "OtherDamage" integer NOT NULL DEFAULT 0,
                    "OtherMove" integer NOT NULL DEFAULT 0,
                    "OtherArmor" integer NOT NULL DEFAULT 0,
                    "OtherHp" integer NOT NULL DEFAULT 0,
                    "RemainingPd" integer NOT NULL DEFAULT 0,
                    "Discipline" integer NOT NULL DEFAULT 1,
                    "MaxBaseSkillAtGraduation" integer NOT NULL DEFAULT 0,
                    "FreeAttributePoints" integer NOT NULL DEFAULT 0,
                    "CurrentHp" integer NOT NULL DEFAULT 0,
                    "CreatedAtUtc" timestamp with time zone NOT NULL DEFAULT NOW(),
                    "UpdatedAtUtc" timestamp with time zone NOT NULL DEFAULT NOW()
                );

                CREATE INDEX IF NOT EXISTS "IX_BaronyUnits_BaronyId" ON "BaronyUnits" ("BaronyId");

                ALTER TABLE "BaronyProjects"
                    ADD COLUMN IF NOT EXISTS "UnitId" integer NULL;

                CREATE INDEX IF NOT EXISTS "IX_BaronyProjects_UnitId" ON "BaronyProjects" ("UnitId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_BaronyProjects_UnitId";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "UnitId";
                DROP TABLE IF EXISTS "BaronyUnits";
                """);
        }
    }
}
