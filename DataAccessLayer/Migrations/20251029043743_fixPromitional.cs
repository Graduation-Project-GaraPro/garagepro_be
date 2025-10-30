using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixPromitional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices",
                columns: new[] { "PromotionalCampaignId", "ServiceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices");

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionalCampaignServiceId",
                table: "PromotionalCampaignServices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices",
                column: "PromotionalCampaignServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionalCampaignServices_PromotionalCampaignId",
                table: "PromotionalCampaignServices",
                column: "PromotionalCampaignId");
        }
    }
}
