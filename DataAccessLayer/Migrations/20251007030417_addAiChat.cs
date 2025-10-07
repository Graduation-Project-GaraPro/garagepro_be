using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addAiChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiChatSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalMessages = table.Column<int>(type: "int", nullable: false),
                    DiagnosisResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatSessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "AiChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_AiChatMessages_AiChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiChatSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIDiagnostic_Keywords",
                columns: table => new
                {
                    KeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AIChatMessageMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AIDiagnostic_KeywordKeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDiagnostic_Keywords", x => x.KeywordId);
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                        column: x => x.AIDiagnostic_KeywordKeywordId,
                        principalTable: "AIDiagnostic_Keywords",
                        principalColumn: "KeywordId");
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AiChatMessages_AIChatMessageMessageId",
                        column: x => x.AIChatMessageMessageId,
                        principalTable: "AiChatMessages",
                        principalColumn: "MessageId");
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AiChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiChatSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIResponseTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AIDiagnostic_KeywordKeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResponseTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_AIResponseTemplates_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                        column: x => x.AIDiagnostic_KeywordKeywordId,
                        principalTable: "AIDiagnostic_Keywords",
                        principalColumn: "KeywordId");
                    table.ForeignKey(
                        name: "FK_AIResponseTemplates_AiChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "AiChatMessages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessages_SessionId",
                table: "AiChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_AIChatMessageMessageId",
                table: "AIDiagnostic_Keywords",
                column: "AIChatMessageMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                table: "AIDiagnostic_Keywords",
                column: "AIDiagnostic_KeywordKeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_SessionId",
                table: "AIDiagnostic_Keywords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResponseTemplates_AIDiagnostic_KeywordKeywordId",
                table: "AIResponseTemplates",
                column: "AIDiagnostic_KeywordKeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResponseTemplates_MessageId",
                table: "AIResponseTemplates",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIResponseTemplates");

            migrationBuilder.DropTable(
                name: "AIDiagnostic_Keywords");

            migrationBuilder.DropTable(
                name: "AiChatMessages");

            migrationBuilder.DropTable(
                name: "AiChatSessions");
        }
    }
}
