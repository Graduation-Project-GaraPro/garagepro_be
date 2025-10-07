using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixRepairOrderColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make Note column nullable
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "RepairOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            // Make ArchivedByUserId column nullable
            migrationBuilder.AlterColumn<string>(
                name: "ArchivedByUserId",
                table: "RepairOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Note column to non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "RepairOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            // Revert ArchivedByUserId column to non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "ArchivedByUserId",
                table: "RepairOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}