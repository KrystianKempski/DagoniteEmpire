using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260820170000_AddBaronArtifactPpb")]
    public partial class AddBaronArtifactPpb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronArtifacts"
                ADD COLUMN IF NOT EXISTS "AdditiveJson" text NOT NULL DEFAULT '{}';
                ALTER TABLE "BaronArtifacts"
                ADD COLUMN IF NOT EXISTS "PercentJson" text NOT NULL DEFAULT '{}';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronArtifacts" DROP COLUMN IF EXISTS "AdditiveJson";
                ALTER TABLE "BaronArtifacts" DROP COLUMN IF EXISTS "PercentJson";
                """);
        }
    }
}
