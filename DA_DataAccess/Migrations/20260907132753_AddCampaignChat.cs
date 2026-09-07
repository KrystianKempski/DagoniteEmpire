using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.CreateTable(
                name: "CampaignChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    SenderCharacterId = table.Column<int>(type: "integer", nullable: false),
                    RecipientCharacterId = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignChatMessages_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignChatMessages_Characters_RecipientCharacterId",
                        column: x => x.RecipientCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignChatMessages_Characters_SenderCharacterId",
                        column: x => x.SenderCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CampaignChatReads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    PeerCharacterId = table.Column<int>(type: "integer", nullable: true),
                    LastReadDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignChatReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignChatReads_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignChatReads_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatMessages_CampaignId_RecipientCharacterId_Create~",
                table: "CampaignChatMessages",
                columns: new[] { "CampaignId", "RecipientCharacterId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatMessages_CampaignId_SenderCharacterId_CreatedDa~",
                table: "CampaignChatMessages",
                columns: new[] { "CampaignId", "SenderCharacterId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatMessages_RecipientCharacterId",
                table: "CampaignChatMessages",
                column: "RecipientCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatMessages_SenderCharacterId",
                table: "CampaignChatMessages",
                column: "SenderCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatReads_CampaignId_CharacterId_PeerCharacterId",
                table: "CampaignChatReads",
                columns: new[] { "CampaignId", "CharacterId", "PeerCharacterId" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignChatReads_CharacterId",
                table: "CampaignChatReads",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignChatMessages");

            migrationBuilder.DropTable(
                name: "CampaignChatReads");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<string>(type: "text", nullable: false),
                    ToUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_FromUserId",
                table: "ChatMessages",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ToUserId",
                table: "ChatMessages",
                column: "ToUserId");
        }
    }
}
