using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameOfRepairService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numberService",
                table: "RequestServices");

            migrationBuilder.DropColumn(
                name: "number",
                table: "RequestParts");

            migrationBuilder.RenameColumn(
                name: "EstimatedAmount",
                table: "RequestServices",
                newName: "ServiceFee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceFee",
                table: "RequestServices",
                newName: "EstimatedAmount");

            migrationBuilder.AddColumn<int>(
                name: "numberService",
                table: "RequestServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "RequestParts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
