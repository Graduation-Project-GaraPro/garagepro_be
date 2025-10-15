using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatedQuotationStructureUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationParts_Quotations_QuotationId",
                table: "QuotationParts");

            migrationBuilder.DropIndex(
                name: "IX_QuotationParts_QuotationId",
                table: "QuotationParts");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "QuotationParts");

            migrationBuilder.AddColumn<Guid>(
                name: "QuotationServiceId",
                table: "QuotationParts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuotationServiceParts",
                columns: table => new
                {
                    QuotationServicePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationServiceParts", x => x.QuotationServicePartId);
                    table.ForeignKey(
                        name: "FK_QuotationServiceParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationServiceParts_QuotationServices_QuotationServiceId",
                        column: x => x.QuotationServiceId,
                        principalTable: "QuotationServices",
                        principalColumn: "QuotationServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_QuotationServiceId",
                table: "QuotationParts",
                column: "QuotationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServiceParts_PartId",
                table: "QuotationServiceParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServiceParts_QuotationServiceId",
                table: "QuotationServiceParts",
                column: "QuotationServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationParts_QuotationServices_QuotationServiceId",
                table: "QuotationParts",
                column: "QuotationServiceId",
                principalTable: "QuotationServices",
                principalColumn: "QuotationServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationParts_QuotationServices_QuotationServiceId",
                table: "QuotationParts");

            migrationBuilder.DropTable(
                name: "QuotationServiceParts");

            migrationBuilder.DropIndex(
                name: "IX_QuotationParts_QuotationServiceId",
                table: "QuotationParts");

            migrationBuilder.DropColumn(
                name: "QuotationServiceId",
                table: "QuotationParts");

            migrationBuilder.AddColumn<Guid>(
                name: "QuotationId",
                table: "QuotationParts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_QuotationId",
                table: "QuotationParts",
                column: "QuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationParts_Quotations_QuotationId",
                table: "QuotationParts",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "QuotationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
