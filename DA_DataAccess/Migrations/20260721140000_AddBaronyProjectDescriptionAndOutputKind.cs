using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260721140000_AddBaronyProjectDescriptionAndOutputKind")]
    public partial class AddBaronyProjectDescriptionAndOutputKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'Description') THEN
                    ALTER TABLE "BaronyProjects" ADD "Description" text NOT NULL DEFAULT '';
                  END IF;
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'OutputKind') THEN
                    ALTER TABLE "BaronyProjects" ADD "OutputKind" text NOT NULL DEFAULT 'Decree / Technology';
                  END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "Description";
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "OutputKind";
                """);
        }
    }
}
