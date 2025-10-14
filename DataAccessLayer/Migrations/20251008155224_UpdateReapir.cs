using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReapir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa hai cột bigint cũ
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "ActualTime",
                table: "Repairs");

            // Thêm lại hai cột mới kiểu time
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedTime",
                table: "Repairs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ActualTime",
                table: "Repairs",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa hai cột kiểu time
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "ActualTime",
                table: "Repairs");

            // Thêm lại hai cột bigint nếu rollback
            migrationBuilder.AddColumn<long>(
                name: "EstimatedTime",
                table: "Repairs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ActualTime",
                table: "Repairs",
                type: "bigint",
                nullable: true);
        }
    }
}
