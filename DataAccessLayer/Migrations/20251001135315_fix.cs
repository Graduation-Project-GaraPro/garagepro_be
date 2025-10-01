using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_AspNetUsers_GrantedBy",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_GrantedBy",
                table: "RolePermissions");

            migrationBuilder.AlterColumn<string>(
                name: "GrantedBy",
                table: "RolePermissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GrantedBy",
                table: "RolePermissions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GrantedBy",
                table: "RolePermissions",
                column: "GrantedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_AspNetUsers_GrantedBy",
                table: "RolePermissions",
                column: "GrantedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
