using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairOrderQuotationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RepairOrderId",
                table: "Quotations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RepairOrderId",
                table: "Quotations",
                column: "RepairOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_RepairOrders_RepairOrderId",
                table: "Quotations",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "RepairOrderId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_RepairOrders_RepairOrderId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_RepairOrderId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RepairOrderId",
                table: "Quotations");
        }
    }
}
