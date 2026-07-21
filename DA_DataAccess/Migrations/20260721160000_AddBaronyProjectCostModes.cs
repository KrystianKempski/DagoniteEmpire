using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260721160000_AddBaronyProjectCostModes")]
    public partial class AddBaronyProjectCostModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'CostGoldProductionJson') THEN
                    ALTER TABLE "BaronyProjects" ADD "CostGoldProductionJson" text NOT NULL DEFAULT '{}';
                  END IF;
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'CostMaterialsJson') THEN
                    ALTER TABLE "BaronyProjects" ADD "CostMaterialsJson" text NOT NULL DEFAULT '{}';
                  END IF;
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'AllowedCostModes') THEN
                    ALTER TABLE "BaronyProjects" ADD "AllowedCostModes" text NOT NULL DEFAULT 'Player choice';
                  END IF;
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'SelectedCostMode') THEN
                    ALTER TABLE "BaronyProjects" ADD "SelectedCostMode" text NULL;
                  END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "CostGoldProductionJson";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "CostMaterialsJson";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "AllowedCostModes";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "SelectedCostMode";
                """);
        }
    }
}
