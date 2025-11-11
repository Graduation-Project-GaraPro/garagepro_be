using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class MakeRepairRequestIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_RepairRequests_RepairRequestId",
                table: "RepairOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "RepairRequestId",
                table: "RepairOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_RepairRequests_RepairRequestId",
                table: "RepairOrders",
                column: "RepairRequestId",
                principalTable: "RepairRequests",
                principalColumn: "RepairRequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_RepairRequests_RepairRequestId",
                table: "RepairOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "RepairRequestId",
                table: "RepairOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_RepairRequests_RepairRequestId",
                table: "RepairOrders",
                column: "RepairRequestId",
                principalTable: "RepairRequests",
                principalColumn: "RepairRequestID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
