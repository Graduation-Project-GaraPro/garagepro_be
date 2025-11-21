using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertiesPartInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartSpecifications");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Inspections");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PartInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PartInspections");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Inspections",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartSpecifications",
                columns: table => new
                {
                    SpecId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpecTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartSpecifications", x => x.SpecId);
                    table.ForeignKey(
                        name: "FK_PartSpecifications_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartSpecifications_PartId",
                table: "PartSpecifications",
                column: "PartId");
        }
    }
}
