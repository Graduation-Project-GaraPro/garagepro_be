using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addSeedingValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "FeedBacks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RepairOrderRepairRequest",
                columns: table => new
                {
                    RepairOrdersRepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderRepairRequest", x => new { x.RepairOrdersRepairOrderId, x.RepairRequestID });
                    table.ForeignKey(
                        name: "FK_RepairOrderRepairRequest_RepairOrders_RepairOrdersRepairOrderId",
                        column: x => x.RepairOrdersRepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderRepairRequest_RepairRequests_RepairRequestID",
                        column: x => x.RepairRequestID,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_ApplicationUserId",
                table: "FeedBacks",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderRepairRequest_RepairRequestID",
                table: "RepairOrderRepairRequest",
                column: "RepairRequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_AspNetUsers_ApplicationUserId",
                table: "FeedBacks",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_AspNetUsers_ApplicationUserId",
                table: "FeedBacks");

            migrationBuilder.DropTable(
                name: "RepairOrderRepairRequest");

            migrationBuilder.DropIndex(
                name: "IX_FeedBacks_ApplicationUserId",
                table: "FeedBacks");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "FeedBacks");
        }
    }
}
