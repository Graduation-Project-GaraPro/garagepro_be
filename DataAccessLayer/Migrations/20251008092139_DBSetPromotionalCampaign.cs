using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DBSetPromotionalCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaign_PromotionalCampaignId",
                table: "PromotionalCampaignService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaign",
                table: "PromotionalCampaign");

            migrationBuilder.RenameTable(
                name: "PromotionalCampaign",
                newName: "PromotionalCampaigns");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaigns",
                table: "PromotionalCampaigns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignService",
                column: "PromotionalCampaignId",
                principalTable: "PromotionalCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaigns_PromotionalCampaignId",
                table: "PromotionalCampaignService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionalCampaigns",
                table: "PromotionalCampaigns");

            migrationBuilder.RenameTable(
                name: "PromotionalCampaigns",
                newName: "PromotionalCampaign");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionalCampaign",
                table: "PromotionalCampaign",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionalCampaignService_PromotionalCampaign_PromotionalCampaignId",
                table: "PromotionalCampaignService",
                column: "PromotionalCampaignId",
                principalTable: "PromotionalCampaign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
