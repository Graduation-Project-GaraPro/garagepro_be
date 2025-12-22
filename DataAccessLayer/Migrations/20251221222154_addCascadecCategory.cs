using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addCascadecCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInventories_Branches_BranchId",
                table: "PartInventories");

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventories_Branches_BranchId",
                table: "PartInventories",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInventories_Branches_BranchId",
                table: "PartInventories");

            migrationBuilder.AddForeignKey(
                name: "FK_PartInventories_Branches_BranchId",
                table: "PartInventories",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
