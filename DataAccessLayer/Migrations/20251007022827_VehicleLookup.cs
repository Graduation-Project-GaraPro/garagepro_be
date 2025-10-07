using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class VehicleLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_Specifications_SpecificationsID",
                table: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "Specifications");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_SpecificationsID",
                table: "SpecificationsData");

            migrationBuilder.RenameColumn(
                name: "SpecificationsID",
                table: "SpecificationsData",
                newName: "LookupID");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryID",
                table: "SpecificationsData",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SpecificationsData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SpecificationCategory",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationCategor", x => x.CategoryID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_CategoryID",
                table: "SpecificationsData",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_DisplayOrder",
                table: "SpecificationsData",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_LookupID_CategoryID",
                table: "SpecificationsData",
                columns: new[] { "LookupID", "CategoryID" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationCategor_DisplayOrder",
                table: "SpecificationCategor",
                column: "DisplayOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_SpecificationCategor_CategoryID",
                table: "SpecificationsData",
                column: "CategoryID",
                principalTable: "SpecificationCategor",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_VehicleLookups_LookupID",
                table: "SpecificationsData",
                column: "LookupID",
                principalTable: "VehicleLookups",
                principalColumn: "LookupID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_SpecificationCategor_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_VehicleLookups_LookupID",
                table: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "SpecificationCategor");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_DisplayOrder",
                table: "SpecificationsData");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_LookupID_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SpecificationsData");

            migrationBuilder.RenameColumn(
                name: "LookupID",
                table: "SpecificationsData",
                newName: "SpecificationsID");

            migrationBuilder.CreateTable(
                name: "Specifications",
                columns: table => new
                {
                    SpecificationsID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LookupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specifications", x => x.SpecificationsID);
                    table.ForeignKey(
                        name: "FK_Specifications_VehicleLookups_LookupID",
                        column: x => x.LookupID,
                        principalTable: "VehicleLookups",
                        principalColumn: "LookupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_SpecificationsID",
                table: "SpecificationsData",
                column: "SpecificationsID");

            migrationBuilder.CreateIndex(
                name: "IX_Specifications_LookupID",
                table: "Specifications",
                column: "LookupID");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_Specifications_SpecificationsID",
                table: "SpecificationsData",
                column: "SpecificationsID",
                principalTable: "Specifications",
                principalColumn: "SpecificationsID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
