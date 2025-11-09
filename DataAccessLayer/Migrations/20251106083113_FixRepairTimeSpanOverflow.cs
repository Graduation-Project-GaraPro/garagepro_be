using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixRepairTimeSpanOverflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualTime",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Repairs");

            migrationBuilder.AddColumn<long>(
                name: "ActualTimeTicks",
                table: "Repairs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EstimatedTimeTicks",
                table: "Repairs",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualTimeTicks",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeTicks",
                table: "Repairs");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ActualTime",
                table: "Repairs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedTime",
                table: "Repairs",
                type: "time",
                nullable: true);
        }
    }
}
