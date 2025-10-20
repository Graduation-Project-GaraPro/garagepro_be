using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixSystemLog3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_LogCategories_CategoryId",
                table: "SystemLogs");

            migrationBuilder.DropTable(
                name: "LogCategories");

            migrationBuilder.DropIndex(
                name: "IX_SystemLogs_CategoryId",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "SystemLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "SystemLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LogCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CategoryId",
                table: "SystemLogs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LogCategories_Name",
                table: "LogCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_LogCategories_CategoryId",
                table: "SystemLogs",
                column: "CategoryId",
                principalTable: "LogCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
