using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class removekeyPartService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts");

            migrationBuilder.DropIndex(
                name: "IX_ServiceParts_ServiceId",
                table: "ServiceParts");

            migrationBuilder.DropColumn(
                name: "ServicePartId",
                table: "ServiceParts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts",
                columns: new[] { "ServiceId", "PartId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts");

            migrationBuilder.AddColumn<Guid>(
                name: "ServicePartId",
                table: "ServiceParts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts",
                column: "ServicePartId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceParts_ServiceId",
                table: "ServiceParts",
                column: "ServiceId");
        }
    }
}
