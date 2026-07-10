using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBaronyLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Advisors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    OfficeType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    PersonName = table.Column<string>(type: "text", nullable: false),
                    IsBaron = table.Column<bool>(type: "boolean", nullable: false),
                    HasAssistant = table.Column<bool>(type: "boolean", nullable: false),
                    AssistantBonus = table.Column<int>(type: "integer", nullable: false),
                    SkillsJson = table.Column<string>(type: "text", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    FormulaText = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UpkeepGold = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advisors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Baronies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    Season = table.Column<string>(type: "text", nullable: false),
                    TreasuryGold = table.Column<decimal>(type: "numeric", nullable: false),
                    BaronPurseGold = table.Column<decimal>(type: "numeric", nullable: false),
                    FoodInGranaries = table.Column<decimal>(type: "numeric", nullable: false),
                    Unrest = table.Column<int>(type: "integer", nullable: false),
                    BaseParametersJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baronies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaronyBuildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaronyBuildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaronyEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaronyEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaronyProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CostJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    AllocatedJson = table.Column<string>(type: "text", nullable: false),
                    ResultDescription = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TurnsRemaining = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaronyProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuildingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    GoldCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductionCost = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectAdditiveJson = table.Column<string>(type: "text", nullable: false),
                    EffectPercentJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TerrainRequirement = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityModifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    FormulaText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityModifiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Decrees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FormulaText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decrees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fiefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LiegeName = table.Column<string>(type: "text", nullable: false),
                    IsBaronDemesne = table.Column<bool>(type: "boolean", nullable: false),
                    BonusMultiplier = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fiefs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialGroupRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    RelationLevel = table.Column<int>(type: "integer", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    FormulaText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialGroupRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TerrainImprovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    TileId = table.Column<int>(type: "integer", nullable: true),
                    TemplateId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AdditiveJson = table.Column<string>(type: "text", nullable: false),
                    PercentJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FormulaText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainImprovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TerrainTiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaronyId = table.Column<int>(type: "integer", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    BaseType = table.Column<string>(type: "text", nullable: false),
                    FeaturesCsv = table.Column<string>(type: "text", nullable: false),
                    Fertility = table.Column<int>(type: "integer", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: true),
                    FiefId = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainTiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advisors");

            migrationBuilder.DropTable(
                name: "Baronies");

            migrationBuilder.DropTable(
                name: "BaronyBuildings");

            migrationBuilder.DropTable(
                name: "BaronyEvents");

            migrationBuilder.DropTable(
                name: "BaronyProjects");

            migrationBuilder.DropTable(
                name: "BuildingTemplates");

            migrationBuilder.DropTable(
                name: "CommunityModifiers");

            migrationBuilder.DropTable(
                name: "Decrees");

            migrationBuilder.DropTable(
                name: "Fiefs");

            migrationBuilder.DropTable(
                name: "SocialGroupRelations");

            migrationBuilder.DropTable(
                name: "TerrainImprovements");

            migrationBuilder.DropTable(
                name: "TerrainTiles");
        }
    }
}
