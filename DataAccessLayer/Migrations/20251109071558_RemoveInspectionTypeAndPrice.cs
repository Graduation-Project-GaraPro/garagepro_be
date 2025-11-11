using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInspectionTypeAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InspectionPrice",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "InspectionType",
                table: "Inspections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InspectionPrice",
                table: "Inspections",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InspectionType",
                table: "Inspections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
