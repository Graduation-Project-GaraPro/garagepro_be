using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_VehicleID",
                table: "RepairRequests");

            migrationBuilder.CreateIndex(
                name: "UX_RepairRequests_VehicleRequestDate_Active",
                table: "RepairRequests",
                columns: new[] { "VehicleID", "RequestDate" },
                unique: true,
                filter: "[Status] IN (0,1,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RepairRequests_VehicleRequestDate_Active",
                table: "RepairRequests");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_VehicleID",
                table: "RepairRequests",
                column: "VehicleID");
        }
    }
}
