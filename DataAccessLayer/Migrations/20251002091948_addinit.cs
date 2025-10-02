using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatingHours_Friday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Friday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Friday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Monday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Monday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Monday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Saturday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Saturday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Saturday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Sunday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Sunday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Sunday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Thursday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Thursday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Thursday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Tuesday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Tuesday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Tuesday_OpenTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Wednesday_CloseTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Wednesday_IsOpen",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OperatingHours_Wednesday_OpenTime",
                table: "Branches");

            migrationBuilder.CreateTable(
                name: "OperatingHour",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CloseTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatingHour_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHour_BranchId",
                table: "OperatingHour",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatingHour");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Friday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Friday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Friday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Monday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Monday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Monday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Saturday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Saturday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Saturday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Sunday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Sunday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Sunday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Thursday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Thursday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Thursday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Tuesday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Tuesday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Tuesday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Wednesday_CloseTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OperatingHours_Wednesday_IsOpen",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours_Wednesday_OpenTime",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
