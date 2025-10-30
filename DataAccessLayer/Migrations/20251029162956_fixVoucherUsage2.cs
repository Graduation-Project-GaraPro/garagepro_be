using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixVoucherUsage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherUsage_PromotionalCampaigns_CampaignId",
                table: "VoucherUsage");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherUsage_PromotionalCampaigns_CampaignId",
                table: "VoucherUsage",
                column: "CampaignId",
                principalTable: "PromotionalCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherUsage_PromotionalCampaigns_CampaignId",
                table: "VoucherUsage");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherUsage_PromotionalCampaigns_CampaignId",
                table: "VoucherUsage",
                column: "CampaignId",
                principalTable: "PromotionalCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
