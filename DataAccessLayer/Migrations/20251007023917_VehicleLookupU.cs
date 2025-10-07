using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class VehicleLookupU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_SpecificationCategor_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpecificationCategor",
                table: "SpecificationCategor");

            migrationBuilder.RenameTable(
                name: "SpecificationCategor",
                newName: "SpecificationCategory");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationCategor_DisplayOrder",
                table: "SpecificationCategory",
                newName: "IX_SpecificationCategory_DisplayOrder");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpecificationCategory",
                table: "SpecificationCategory",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_SpecificationCategory_CategoryID",
                table: "SpecificationsData",
                column: "CategoryID",
                principalTable: "SpecificationCategory",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_SpecificationCategory_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpecificationCategory",
                table: "SpecificationCategory");

            migrationBuilder.RenameTable(
                name: "SpecificationCategory",
                newName: "SpecificationCategor");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationCategory_DisplayOrder",
                table: "SpecificationCategor",
                newName: "IX_SpecificationCategor_DisplayOrder");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpecificationCategor",
                table: "SpecificationCategor",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_SpecificationCategor_CategoryID",
                table: "SpecificationsData",
                column: "CategoryID",
                principalTable: "SpecificationCategor",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
