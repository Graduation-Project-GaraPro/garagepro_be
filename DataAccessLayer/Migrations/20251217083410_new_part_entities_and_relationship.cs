using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class new_part_entities_and_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_VehicleModels_ModelID",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_ModelID",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ModelID",
                table: "Parts");

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                table: "Parts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "PartCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndAt",
                table: "JobParts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                table: "JobParts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyStartAt",
                table: "JobParts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartInventories",
                columns: table => new
                {
                    PartInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInventories", x => x.PartInventoryId);
                    table.ForeignKey(
                        name: "FK_PartInventories_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartInventories_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_PartCategory_ModelId_CategoryName",
                table: "PartCategories",
                columns: new[] { "ModelId", "CategoryName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartInventories_BranchId",
                table: "PartInventories",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "UX_PartInventory_PartId_BranchId",
                table: "PartInventories",
                columns: new[] { "PartId", "BranchId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartCategories_VehicleModels_ModelId",
                table: "PartCategories",
                column: "ModelId",
                principalTable: "VehicleModels",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartCategories_VehicleModels_ModelId",
                table: "PartCategories");

            migrationBuilder.DropTable(
                name: "PartInventories");

            migrationBuilder.DropIndex(
                name: "UX_PartCategory_ModelId_CategoryName",
                table: "PartCategories");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "PartCategories");

            migrationBuilder.DropColumn(
                name: "WarrantyEndAt",
                table: "JobParts");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                table: "JobParts");

            migrationBuilder.DropColumn(
                name: "WarrantyStartAt",
                table: "JobParts");

            migrationBuilder.AddColumn<Guid>(
                name: "ModelID",
                table: "Parts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ModelID",
                table: "Parts",
                column: "ModelID");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_VehicleModels_ModelID",
                table: "Parts",
                column: "ModelID",
                principalTable: "VehicleModels",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
