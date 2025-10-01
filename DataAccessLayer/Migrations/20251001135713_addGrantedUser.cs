using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addGrantedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GrantedUserId",
                table: "RolePermissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GrantedUserId",
                table: "RolePermissions",
                column: "GrantedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_AspNetUsers_GrantedUserId",
                table: "RolePermissions",
                column: "GrantedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_AspNetUsers_GrantedUserId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_GrantedUserId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "GrantedUserId",
                table: "RolePermissions");
        }
    }
}
