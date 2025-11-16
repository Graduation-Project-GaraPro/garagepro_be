using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ServicePartCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePartCategory_PartCategories_PartCategoryId",
                table: "ServicePartCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePartCategory_Services_ServiceId",
                table: "ServicePartCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicePartCategory",
                table: "ServicePartCategory");

            migrationBuilder.RenameTable(
                name: "ServicePartCategory",
                newName: "ServicePartCategories");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePartCategory_ServiceId",
                table: "ServicePartCategories",
                newName: "IX_ServicePartCategories_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePartCategory_PartCategoryId",
                table: "ServicePartCategories",
                newName: "IX_ServicePartCategories_PartCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicePartCategories",
                table: "ServicePartCategories",
                column: "ServicePartCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePartCategories_PartCategories_PartCategoryId",
                table: "ServicePartCategories",
                column: "PartCategoryId",
                principalTable: "PartCategories",
                principalColumn: "LaborCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePartCategories_Services_ServiceId",
                table: "ServicePartCategories",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePartCategories_PartCategories_PartCategoryId",
                table: "ServicePartCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePartCategories_Services_ServiceId",
                table: "ServicePartCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicePartCategories",
                table: "ServicePartCategories");

            migrationBuilder.RenameTable(
                name: "ServicePartCategories",
                newName: "ServicePartCategory");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePartCategories_ServiceId",
                table: "ServicePartCategory",
                newName: "IX_ServicePartCategory_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePartCategories_PartCategoryId",
                table: "ServicePartCategory",
                newName: "IX_ServicePartCategory_PartCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicePartCategory",
                table: "ServicePartCategory",
                column: "ServicePartCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePartCategory_PartCategories_PartCategoryId",
                table: "ServicePartCategory",
                column: "PartCategoryId",
                principalTable: "PartCategories",
                principalColumn: "LaborCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePartCategory_Services_ServiceId",
                table: "ServicePartCategory",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
