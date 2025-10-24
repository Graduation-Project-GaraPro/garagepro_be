using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuotationAmountFieldsAndRelatedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationParts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuotationServices");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "QuotationServices");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "QuotationServices");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "QuotationServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuotationServiceParts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "QuotationServiceParts");

            migrationBuilder.DropColumn(
                name: "RecommendationNote",
                table: "QuotationServiceParts");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "QuotationServiceParts");

            // Add the new column without a default value first
            migrationBuilder.AddColumn<Guid>(
                name: "PromotionalCampaignServiceId",
                table: "PromotionalCampaignServices",
                type: "uniqueidentifier",
                nullable: true); // Make it nullable initially

            // Generate unique GUIDs for existing records
            migrationBuilder.Sql(@"
                UPDATE PromotionalCampaignServices 
                SET PromotionalCampaignServiceId = NEWID() 
                WHERE PromotionalCampaignServiceId IS NULL");

            // Make the column non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "PromotionalCampaignServiceId",
                table: "PromotionalCampaignServices",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices",
                column: "PromotionalCampaignServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionalCampaignServices_PromotionalCampaignId",
                table: "PromotionalCampaignServices",
                column: "PromotionalCampaignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices");

            migrationBuilder.DropIndex(
                name: "IX_PromotionalCampaignServices_PromotionalCampaignId",
                table: "PromotionalCampaignServices");

            migrationBuilder.DropColumn(
                name: "PromotionalCampaignServiceId",
                table: "PromotionalCampaignServices");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "QuotationServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "QuotationServices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "QuotationServices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "QuotationServices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "QuotationServiceParts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "QuotationServiceParts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RecommendationNote",
                table: "QuotationServiceParts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "QuotationServiceParts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices",
                columns: new[] { "PromotionalCampaignId", "ServiceId" });

            migrationBuilder.CreateTable(
                name: "QuotationParts",
                columns: table => new
                {
                    QuotationPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationParts", x => x.QuotationPartId);
                    table.ForeignKey(
                        name: "FK_QuotationParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationParts_QuotationServices_QuotationServiceId",
                        column: x => x.QuotationServiceId,
                        principalTable: "QuotationServices",
                        principalColumn: "QuotationServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_PartId",
                table: "QuotationParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_QuotationServiceId",
                table: "QuotationParts",
                column: "QuotationServiceId");
        }
    }
}