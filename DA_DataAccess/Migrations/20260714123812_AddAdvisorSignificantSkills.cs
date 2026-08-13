using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisorSignificantSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignificantSkillsJson",
                table: "Advisors",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("""
                UPDATE "Advisors"
                SET "SignificantSkillsJson" = '["Loyalty","Stability","Culture"]'
                WHERE "OfficeType" = 'Kanclerz' AND ("SignificantSkillsJson" = '[]' OR "SignificantSkillsJson" = '');

                UPDATE "Advisors"
                SET "SignificantSkillsJson" = '["Law","Corruption","Defense"]'
                WHERE "OfficeType" = 'Kapitan Straży' AND ("SignificantSkillsJson" = '[]' OR "SignificantSkillsJson" = '');

                UPDATE "Advisors"
                SET "SignificantSkillsJson" = '["Food","Production","Economy"]'
                WHERE "OfficeType" = 'Ekonom' AND ("SignificantSkillsJson" = '[]' OR "SignificantSkillsJson" = '');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignificantSkillsJson",
                table: "Advisors");
        }
    }
}
