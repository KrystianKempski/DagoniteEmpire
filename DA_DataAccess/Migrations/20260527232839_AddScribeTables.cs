using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddScribeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "ScribeConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: true),
                    CampaignId = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScribeConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScribeMemories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SourcePostId = table.Column<int>(type: "integer", nullable: true),
                    SourceChapterId = table.Column<int>(type: "integer", nullable: true),
                    SourceCampaignId = table.Column<int>(type: "integer", nullable: true),
                    SourceDocumentName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CharacterIdsJson = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsGmOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedByModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScribeMemories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScribeMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversationId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SourceChunkIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GenerationTimeMs = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScribeMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScribeMessages_ScribeConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ScribeConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScribeChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScribeMemoryId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false),
                    CampaignId = table.Column<int>(type: "integer", nullable: true),
                    ChapterId = table.Column<int>(type: "integer", nullable: true),
                    MemoryType = table.Column<int>(type: "integer", nullable: false),
                    CharacterIdsJson = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsGmOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScribeChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScribeChunks_ScribeMemories_ScribeMemoryId",
                        column: x => x.ScribeMemoryId,
                        principalTable: "ScribeMemories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScribeChunks_CampaignId",
                table: "ScribeChunks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeChunks_Embedding",
                table: "ScribeChunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ScribeChunks_IsPublic",
                table: "ScribeChunks",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeChunks_MemoryType",
                table: "ScribeChunks",
                column: "MemoryType");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeChunks_ScribeMemoryId",
                table: "ScribeChunks",
                column: "ScribeMemoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeConversations_CampaignId",
                table: "ScribeConversations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeConversations_UserId",
                table: "ScribeConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeMemories_SourceCampaignId",
                table: "ScribeMemories",
                column: "SourceCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeMemories_Type",
                table: "ScribeMemories",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ScribeMessages_ConversationId",
                table: "ScribeMessages",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScribeChunks");

            migrationBuilder.DropTable(
                name: "ScribeMessages");

            migrationBuilder.DropTable(
                name: "ScribeMemories");

            migrationBuilder.DropTable(
                name: "ScribeConversations");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
