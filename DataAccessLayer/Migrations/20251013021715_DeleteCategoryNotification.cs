using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCategoryNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CategoryNotifications_CategoryID",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "CategoryNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CategoryID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryID",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CategoryNotifications",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNotifications", x => x.CategoryID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CategoryID",
                table: "Notifications",
                column: "CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CategoryNotifications_CategoryID",
                table: "Notifications",
                column: "CategoryID",
                principalTable: "CategoryNotifications",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
