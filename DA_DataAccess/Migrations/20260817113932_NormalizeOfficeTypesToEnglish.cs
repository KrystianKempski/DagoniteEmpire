using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeOfficeTypesToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Chancellor'    WHERE ""OfficeType"" = 'Kanclerz';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Guard Captain' WHERE ""OfficeType"" = 'Kapitan Straży';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Steward'       WHERE ""OfficeType"" = 'Ekonom';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Custom'        WHERE ""OfficeType"" = 'Inny';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Kanclerz'      WHERE ""OfficeType"" = 'Chancellor';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Kapitan Straży' WHERE ""OfficeType"" = 'Guard Captain';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Ekonom'        WHERE ""OfficeType"" = 'Steward';
                UPDATE ""Advisors"" SET ""OfficeType"" = 'Inny'          WHERE ""OfficeType"" = 'Custom';
            ");
        }
    }
}
