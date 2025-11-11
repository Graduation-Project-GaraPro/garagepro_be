using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobGenerationTrackingFromQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Quotations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OriginalJobId",
                table: "Jobs",
                column: "OriginalJobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_OriginalJobId",
                table: "Jobs",
                column: "OriginalJobId",
                principalTable: "Jobs",
                principalColumn: "JobId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_OriginalJobId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_OriginalJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Quotations");
        }
    }
}
