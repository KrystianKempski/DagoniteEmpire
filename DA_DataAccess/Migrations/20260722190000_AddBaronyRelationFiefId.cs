using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260722190000_AddBaronyRelationFiefId")]
    public partial class AddBaronyRelationFiefId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "BaronyRelations"
                ADD COLUMN IF NOT EXISTS "FiefId" integer NULL;

                CREATE INDEX IF NOT EXISTS "IX_BaronyRelations_FiefId"
                    ON "BaronyRelations" ("FiefId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_BaronyRelations_FiefId";
                ALTER TABLE "BaronyRelations" DROP COLUMN IF EXISTS "FiefId";
                """);
        }
    }
}
