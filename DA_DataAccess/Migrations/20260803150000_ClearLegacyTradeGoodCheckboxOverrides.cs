using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <summary>
    /// AvailableTradeGoodsJson is now MG override only (availability is derived from
    /// buildings / improvements / treaties). Clear legacy checkbox state.
    /// </summary>
    [Migration("20260803150000_ClearLegacyTradeGoodCheckboxOverrides")]
    public partial class ClearLegacyTradeGoodCheckboxOverrides : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Baronies"
                SET "AvailableTradeGoodsJson" = '[]'
                WHERE "AvailableTradeGoodsJson" IS NULL
                   OR "AvailableTradeGoodsJson" <> '[]';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data clear — no-op.
        }
    }
}
