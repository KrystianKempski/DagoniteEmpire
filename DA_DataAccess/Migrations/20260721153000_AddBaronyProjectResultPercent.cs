using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260721153000_AddBaronyProjectResultPercent")]
    public partial class AddBaronyProjectResultPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF NOT EXISTS (
                      SELECT 1 FROM information_schema.columns
                      WHERE table_name = 'BaronyProjects' AND column_name = 'ResultPercentJson') THEN
                    ALTER TABLE "BaronyProjects" ADD "ResultPercentJson" text NOT NULL DEFAULT '{}';
                  END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyProjects" DROP COLUMN IF EXISTS "ResultPercentJson";
                """);
        }
    }
}
