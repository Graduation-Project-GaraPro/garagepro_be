using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateRepairo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairOrderRepairRequest");

            migrationBuilder.AddColumn<Guid>(
                name: "RepairOrdersRepairOrderId",
                table: "RepairRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_RepairOrdersRepairOrderId",
                table: "RepairRequests",
                column: "RepairOrdersRepairOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrdersRepairOrderId",
                table: "RepairRequests",
                column: "RepairOrdersRepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "RepairOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrdersRepairOrderId",
                table: "RepairRequests");

            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_RepairOrdersRepairOrderId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "RepairOrdersRepairOrderId",
                table: "RepairRequests");

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
                name: "IX_RepairOrderRepairRequest_RepairRequestID",
                table: "RepairOrderRepairRequest",
                column: "RepairRequestID");
        }
    }
}
