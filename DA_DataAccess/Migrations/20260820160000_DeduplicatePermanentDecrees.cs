using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <summary>
    /// Concurrent Domain Panel / HUD GetOverview calls could insert permanent work-calendar
    /// decrees twice. Remove extras and lock the catalog names with a unique index.
    /// </summary>
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260820160000_DeduplicatePermanentDecrees")]
    public partial class DeduplicatePermanentDecrees : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Decrees" d
                WHERE d."Name" IN ('Few free days', 'Many free days')
                  AND d."Id" NOT IN (
                    SELECT keep."Id"
                    FROM (
                        SELECT DISTINCT ON ("BaronyId", "Name") "Id"
                        FROM "Decrees"
                        WHERE "Name" IN ('Few free days', 'Many free days')
                        ORDER BY "BaronyId", "Name", "IsActive" DESC, "Id"
                    ) keep
                  );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Decrees_BaronyId_PermanentName"
                ON "Decrees" ("BaronyId", "Name")
                WHERE "Name" IN ('Few free days', 'Many free days');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Decrees_BaronyId_PermanentName";
                """);
        }
    }
}
