using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addEmertech : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TechnicianId",
                table: "RequestEmergencies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestEmergencies_TechnicianId",
                table: "RequestEmergencies",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestEmergencies_AspNetUsers_TechnicianId",
                table: "RequestEmergencies",
                column: "TechnicianId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestEmergencies_AspNetUsers_TechnicianId",
                table: "RequestEmergencies");

            migrationBuilder.DropIndex(
                name: "IX_RequestEmergencies_TechnicianId",
                table: "RequestEmergencies");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "RequestEmergencies");
        }
    }
}
