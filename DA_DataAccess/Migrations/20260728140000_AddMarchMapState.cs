using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(DA_DataAccess.Data.ApplicationDbContext))]
    [Migration("20260728140000_AddMarchMapState")]
    public partial class AddMarchMapState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "MarchMapStates" (
                    "Id" integer NOT NULL,
                    "PayloadJson" text NOT NULL DEFAULT '{}',
                    CONSTRAINT "PK_MarchMapStates" PRIMARY KEY ("Id")
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "MarchMapStates";""");
        }
    }
}
