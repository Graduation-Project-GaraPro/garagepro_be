using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addfee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DistanceToGarageKm",
                table: "RequestEmergencies",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "RequestEmergencies",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceToGarageKm",
                table: "RequestEmergencies");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "RequestEmergencies");
        }
    }
}
