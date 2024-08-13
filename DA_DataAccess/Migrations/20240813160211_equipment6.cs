using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class equipment6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SelectedCharacterId = table.Column<int>(type: "integer", nullable: true),
                    ShowBadge = table.Column<bool>(type: "boolean", nullable: true),
                    BadgeContent = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GameMaster = table.Column<string>(type: "text", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ShortDescr = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    EquipmentType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    fileData = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RelatedAttributeName = table.Column<string>(type: "text", nullable: false),
                    ClassLevel = table.Column<int>(type: "integer", nullable: false),
                    MaxFocusPoints = table.Column<int>(type: "integer", nullable: false),
                    CurrentCofusPoints = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsUniversal = table.Column<bool>(type: "boolean", nullable: false),
                    CasterType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RaceApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Traits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    TraitType = table.Column<string>(type: "text", nullable: false),
                    TraitValue = table.Column<int>(type: "integer", nullable: false),
                    Descr = table.Column<string>(type: "text", nullable: false),
                    SummaryDescr = table.Column<string>(type: "text", nullable: false),
                    TraitApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsUnique = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<string>(type: "text", nullable: false),
                    ToUserId = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Day = table.Column<string>(type: "text", nullable: false),
                    Place = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    CampaignId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DC = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<int>(type: "integer", nullable: true),
                    Range = table.Column<string>(type: "text", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    ActiveProfessionId = table.Column<int>(type: "integer", nullable: true),
                    PassiveProfessionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessionSkills_Professions_ActiveProfessionId",
                        column: x => x.ActiveProfessionId,
                        principalTable: "Professions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProfessionSkills_Professions_PassiveProfessionId",
                        column: x => x.PassiveProfessionId,
                        principalTable: "Professions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpellCircles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    KnownSpells = table.Column<int>(type: "integer", nullable: false),
                    PerDay = table.Column<int>(type: "integer", nullable: false),
                    ProfessionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellCircles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpellCircles_Professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "Professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    NPCName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    NPCType = table.Column<string>(type: "text", nullable: true),
                    AttributePoints = table.Column<int>(type: "integer", nullable: false),
                    CurrentExpPoints = table.Column<int>(type: "integer", nullable: false),
                    UsedExpPoints = table.Column<int>(type: "integer", nullable: false),
                    TraitBalance = table.Column<int>(type: "integer", nullable: false),
                    RaceId = table.Column<int>(type: "integer", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ProfessionId = table.Column<int>(type: "integer", nullable: false),
                    ArmorId = table.Column<int>(type: "integer", nullable: false),
                    WeaponId = table.Column<int>(type: "integer", nullable: false),
                    FaceId = table.Column<int>(type: "integer", nullable: false),
                    ThroatId = table.Column<int>(type: "integer", nullable: false),
                    BodyId = table.Column<int>(type: "integer", nullable: false),
                    HandsId = table.Column<int>(type: "integer", nullable: false),
                    WaistId = table.Column<int>(type: "integer", nullable: false),
                    FeetId = table.Column<int>(type: "integer", nullable: false),
                    HeadId = table.Column<int>(type: "integer", nullable: false),
                    ShouldersId = table.Column<int>(type: "integer", nullable: false),
                    TorsoId = table.Column<int>(type: "integer", nullable: false),
                    ArmsId = table.Column<int>(type: "integer", nullable: false),
                    Ring1Id = table.Column<int>(type: "integer", nullable: false),
                    Ring2Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "Professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeatureType = table.Column<string>(type: "text", nullable: false),
                    FeatureName = table.Column<string>(type: "text", nullable: true),
                    BonusValue = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    TraitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bonuses_Traits_TraitId",
                        column: x => x.TraitId,
                        principalTable: "Traits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentTraitEquipment",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    TraitsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentTraitEquipment", x => new { x.EquipmentId, x.TraitsId });
                    table.ForeignKey(
                        name: "FK_EquipmentTraitEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentTraitEquipment_Traits_TraitsId",
                        column: x => x.TraitsId,
                        principalTable: "Traits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaceTraitRace",
                columns: table => new
                {
                    RacesId = table.Column<int>(type: "integer", nullable: false),
                    TraitsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceTraitRace", x => new { x.RacesId, x.TraitsId });
                    table.ForeignKey(
                        name: "FK_RaceTraitRace_Races_RacesId",
                        column: x => x.RacesId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceTraitRace_Traits_TraitsId",
                        column: x => x.TraitsId,
                        principalTable: "Traits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpellSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InSpellbook = table.Column<bool>(type: "boolean", nullable: false),
                    Prepared = table.Column<int>(type: "integer", nullable: false),
                    SpellId = table.Column<int>(type: "integer", nullable: true),
                    SpellCircleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpellSlots_SpellCircles_SpellCircleId",
                        column: x => x.SpellCircleId,
                        principalTable: "SpellCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FeatureType = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    BaseBonus = table.Column<int>(type: "integer", nullable: false),
                    RaceBonus = table.Column<int>(type: "integer", nullable: false),
                    GearBonus = table.Column<int>(type: "integer", nullable: false),
                    TraitBonus = table.Column<int>(type: "integer", nullable: false),
                    OtherBonuses = table.Column<int>(type: "integer", nullable: false),
                    TempBonuses = table.Column<int>(type: "integer", nullable: false),
                    HealthBonus = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attributes_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedAttribute1 = table.Column<string>(type: "text", nullable: false),
                    RelatedAttribute2 = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FeatureType = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    BaseBonus = table.Column<int>(type: "integer", nullable: false),
                    RaceBonus = table.Column<int>(type: "integer", nullable: false),
                    GearBonus = table.Column<int>(type: "integer", nullable: false),
                    TraitBonus = table.Column<int>(type: "integer", nullable: false),
                    OtherBonuses = table.Column<int>(type: "integer", nullable: false),
                    TempBonuses = table.Column<int>(type: "integer", nullable: false),
                    HealthBonus = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignCharacter",
                columns: table => new
                {
                    CampaignsId = table.Column<int>(type: "integer", nullable: false),
                    CharactersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCharacter", x => new { x.CampaignsId, x.CharactersId });
                    table.ForeignKey(
                        name: "FK_CampaignCharacter_Campaigns_CampaignsId",
                        column: x => x.CampaignsId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCharacter_Characters_CharactersId",
                        column: x => x.CharactersId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChapterCharacter",
                columns: table => new
                {
                    ChaptersId = table.Column<int>(type: "integer", nullable: false),
                    CharactersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterCharacter", x => new { x.ChaptersId, x.CharactersId });
                    table.ForeignKey(
                        name: "FK_ChapterCharacter_Chapters_ChaptersId",
                        column: x => x.ChaptersId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChapterCharacter_Characters_CharactersId",
                        column: x => x.CharactersId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterEquipment",
                columns: table => new
                {
                    CharactersId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterEquipment", x => new { x.CharactersId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_CharacterEquipment_Characters_CharactersId",
                        column: x => x.CharactersId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterTraitAdv",
                columns: table => new
                {
                    CharactersId = table.Column<int>(type: "integer", nullable: false),
                    TraitsAdvId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterTraitAdv", x => new { x.CharactersId, x.TraitsAdvId });
                    table.ForeignKey(
                        name: "FK_CharacterTraitAdv_Characters_CharactersId",
                        column: x => x.CharactersId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterTraitAdv_Traits_TraitsAdvId",
                        column: x => x.TraitsAdvId,
                        principalTable: "Traits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    ChapterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Posts_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelatedAttribute1 = table.Column<string>(type: "text", nullable: false),
                    RelatedAttribute2 = table.Column<string>(type: "text", nullable: false),
                    RelatedBaseSkillName = table.Column<string>(type: "text", nullable: true),
                    ChosenAttribute = table.Column<string>(type: "text", nullable: true),
                    Editable = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FeatureType = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    BaseBonus = table.Column<int>(type: "integer", nullable: false),
                    RaceBonus = table.Column<int>(type: "integer", nullable: false),
                    GearBonus = table.Column<int>(type: "integer", nullable: false),
                    TraitBonus = table.Column<int>(type: "integer", nullable: false),
                    OtherBonuses = table.Column<int>(type: "integer", nullable: false),
                    TempBonuses = table.Column<int>(type: "integer", nullable: false),
                    HealthBonus = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_CharacterId",
                table: "Attributes",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseSkills_CharacterId",
                table: "BaseSkills",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_TraitId",
                table: "Bonuses",
                column: "TraitId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacter_CharactersId",
                table: "CampaignCharacter",
                column: "CharactersId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterCharacter_CharactersId",
                table: "ChapterCharacter",
                column: "CharactersId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CampaignId",
                table: "Chapters",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterEquipment_EquipmentId",
                table: "CharacterEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ProfessionId",
                table: "Characters",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_RaceId",
                table: "Characters",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterTraitAdv_TraitsAdvId",
                table: "CharacterTraitAdv",
                column: "TraitsAdvId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_FromUserId",
                table: "ChatMessages",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ToUserId",
                table: "ChatMessages",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTraitEquipment_TraitsId",
                table: "EquipmentTraitEquipment",
                column: "TraitsId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ChapterId",
                table: "Posts",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CharacterId",
                table: "Posts",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionSkills_ActiveProfessionId",
                table: "ProfessionSkills",
                column: "ActiveProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionSkills_PassiveProfessionId",
                table: "ProfessionSkills",
                column: "PassiveProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceTraitRace_TraitsId",
                table: "RaceTraitRace",
                column: "TraitsId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialSkills_CharacterId",
                table: "SpecialSkills",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellCircles_ProfessionId",
                table: "SpellCircles",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_SpellCircleId",
                table: "SpellSlots",
                column: "SpellCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_SpellId",
                table: "SpellSlots",
                column: "SpellId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "BaseSkills");

            migrationBuilder.DropTable(
                name: "Bonuses");

            migrationBuilder.DropTable(
                name: "CampaignCharacter");

            migrationBuilder.DropTable(
                name: "ChapterCharacter");

            migrationBuilder.DropTable(
                name: "CharacterEquipment");

            migrationBuilder.DropTable(
                name: "CharacterTraitAdv");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "EquipmentTraitEquipment");

            migrationBuilder.DropTable(
                name: "ImageFiles");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "ProfessionSkills");

            migrationBuilder.DropTable(
                name: "RaceTraitRace");

            migrationBuilder.DropTable(
                name: "SpecialSkills");

            migrationBuilder.DropTable(
                name: "SpellSlots");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Traits");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "SpellCircles");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropTable(
                name: "Professions");
        }
    }
}
