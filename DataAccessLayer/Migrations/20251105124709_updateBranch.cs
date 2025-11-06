using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Branches");

            migrationBuilder.RenameColumn(
                name: "Ward",
                table: "Branches",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Branches",
                newName: "Comune");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Branches",
                newName: "Ward");

            migrationBuilder.RenameColumn(
                name: "Comune",
                table: "Branches",
                newName: "District");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
