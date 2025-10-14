using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs",
                column: "JobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs",
                column: "JobId");
        }
    }
}
