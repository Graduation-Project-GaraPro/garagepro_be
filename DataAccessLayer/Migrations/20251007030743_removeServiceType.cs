using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class removeServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "ServiceCategories");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OpenTime",
                table: "OperatingHours",
                type: "time",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "CloseTime",
                table: "OperatingHours",
                type: "time",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldMaxLength: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceTypeId",
                table: "ServiceCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OpenTime",
                table: "OperatingHours",
                type: "time",
                maxLength: 5,
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "CloseTime",
                table: "OperatingHours",
                type: "time",
                maxLength: 5,
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldMaxLength: 5,
                oldNullable: true);
        }
    }
}
