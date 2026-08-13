using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ApplySocialGroupInfluenceAndActiveColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SocialGroupRelations" ADD COLUMN IF NOT EXISTS "InfluencePercent" integer NULL;
                ALTER TABLE "SocialGroupRelations" ADD COLUMN IF NOT EXISTS "IsActive" boolean NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "SocialGroupRelations" DROP COLUMN IF EXISTS "InfluencePercent";
                ALTER TABLE "SocialGroupRelations" DROP COLUMN IF EXISTS "IsActive";
                """);
        }
    }
}
