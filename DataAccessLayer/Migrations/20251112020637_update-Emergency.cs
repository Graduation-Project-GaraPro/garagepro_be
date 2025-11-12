using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateEmergency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RepairRequestId",
                table: "RequestEmergencies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmergencyRequestId",
                table: "RepairRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestEmergencies_RepairRequestId",
                table: "RequestEmergencies",
                column: "RepairRequestId",
                unique: true,
                filter: "[RepairRequestId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestEmergencies_RepairRequests_RepairRequestId",
                table: "RequestEmergencies",
                column: "RepairRequestId",
                principalTable: "RepairRequests",
                principalColumn: "RepairRequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestEmergencies_RepairRequests_RepairRequestId",
                table: "RequestEmergencies");

            migrationBuilder.DropIndex(
                name: "IX_RequestEmergencies_RepairRequestId",
                table: "RequestEmergencies");

            migrationBuilder.DropColumn(
                name: "RepairRequestId",
                table: "RequestEmergencies");

            migrationBuilder.DropColumn(
                name: "EmergencyRequestId",
                table: "RepairRequests");
        }
    }
}
