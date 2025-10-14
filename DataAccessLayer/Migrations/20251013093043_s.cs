using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class s : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignService");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignService_Services_ServiceId",
                table: "PromotionalCampaignService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaignService",
                table: "PromotionalCampaignService");

            migrationBuilder.RenameTable(
                name: "PromotionalCampaignService",
                newName: "PromotionalCampaignServices");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionalCampaignService_ServiceId",
                table: "PromotionalCampaignServices",
                newName: "IX_PromotionalCampaignServices_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices",
                columns: new[] { "PromotionalCampaignId", "ServiceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignServices_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignServices",
                column: "PromotionalCampaignId",
                principalTable: "PromotionalCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignServices_Services_ServiceId",
                table: "PromotionalCampaignServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignServices_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignServices");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignServices_Services_ServiceId",
                table: "PromotionalCampaignServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaignServices",
                table: "PromotionalCampaignServices");

            migrationBuilder.RenameTable(
                name: "PromotionalCampaignServices",
                newName: "PromotionalCampaignService");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionalCampaignServices_ServiceId",
                table: "PromotionalCampaignService",
                newName: "IX_PromotionalCampaignService_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaignService",
                table: "PromotionalCampaignService",
                columns: new[] { "PromotionalCampaignId", "ServiceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignService",
                column: "PromotionalCampaignId",
                principalTable: "PromotionalCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignService_Services_ServiceId",
                table: "PromotionalCampaignService",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
