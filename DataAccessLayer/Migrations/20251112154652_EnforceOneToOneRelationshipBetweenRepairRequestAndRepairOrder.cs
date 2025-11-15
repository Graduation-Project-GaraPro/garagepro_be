using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOneToOneRelationshipBetweenRepairRequestAndRepairOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_RepairRequestId",
                table: "RepairOrders");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_RepairRequestId",
                table: "RepairOrders",
                column: "RepairRequestId",
                unique: true,
                filter: "[RepairRequestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_RepairRequestId",
                table: "RepairOrders");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_RepairRequestId",
                table: "RepairOrders",
                column: "RepairRequestId");
        }
    }
}
