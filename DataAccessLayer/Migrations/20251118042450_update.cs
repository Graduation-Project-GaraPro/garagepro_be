using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrderId",
                table: "RepairRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "RepairOrderId",
                table: "RepairRequests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrderId",
                table: "RepairRequests",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "RepairOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrderId",
                table: "RepairRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "RepairOrderId",
                table: "RepairRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_RepairOrders_RepairOrderId",
                table: "RepairRequests",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "RepairOrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
