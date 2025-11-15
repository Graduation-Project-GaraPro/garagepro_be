using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingInspectionStatusValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing inspection status values
            // Since we swapped New (0) and Pending (1), we need to update existing records
            // Use a temporary value to avoid conflicts during the swap
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'TEMP' WHERE Status = 'New'"); // Temporarily change New to TEMP
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'New' WHERE Status = 'Pending'");  // Change Pending to New
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'Pending' WHERE Status = 'TEMP'"); // Change temp value to Pending
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the changes
            // Use a temporary value to avoid conflicts during the swap
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'TEMP' WHERE Status = 'Pending'"); // Temporarily change Pending to TEMP
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'Pending' WHERE Status = 'New'");  // Change New to Pending
            migrationBuilder.Sql("UPDATE Inspections SET Status = 'New' WHERE Status = 'TEMP'"); // Change temp value to New
        }
    }
}