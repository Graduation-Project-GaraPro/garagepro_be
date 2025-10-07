using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Jobs_JobId",
                table: "Quotations");

            migrationBuilder.RenameColumn(
                name: "JobId",
                table: "Quotations",
                newName: "InspectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_JobId",
                table: "Quotations",
                newName: "IX_Quotations_InspectionId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalQuotationId",
                table: "Quotations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionNumber",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Users",
                table: "AspNetRoles",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "AspNetRoles",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "QuotationServices",
                columns: table => new
                {
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerRequestedParts = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationServices", x => x.QuotationServiceId);
                    table.ForeignKey(
                        name: "FK_QuotationServices_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuotationServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationServiceParts",
                columns: table => new
                {
                    QuotationServicePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationServiceParts", x => x.QuotationServicePartId);
                    table.ForeignKey(
                        name: "FK_QuotationServiceParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationServiceParts_QuotationServices_QuotationServiceId",
                        column: x => x.QuotationServiceId,
                        principalTable: "QuotationServices",
                        principalColumn: "QuotationServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_QuotationId",
                table: "Jobs",
                column: "QuotationId",
                unique: true,
                filter: "[QuotationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServiceParts_PartId",
                table: "QuotationServiceParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServiceParts_QuotationServiceId",
                table: "QuotationServiceParts",
                column: "QuotationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServices_QuotationId",
                table: "QuotationServices",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServices_ServiceId",
                table: "QuotationServices",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Quotations_QuotationId",
                table: "Jobs",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "QuotationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Inspections_InspectionId",
                table: "Quotations",
                column: "InspectionId",
                principalTable: "Inspections",
                principalColumn: "InspectionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Quotations_QuotationId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Inspections_InspectionId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "QuotationServiceParts");

            migrationBuilder.DropTable(
                name: "QuotationServices");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_QuotationId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OriginalQuotationId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.RenameColumn(
                name: "InspectionId",
                table: "Quotations",
                newName: "JobId");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_InspectionId",
                table: "Quotations",
                newName: "IX_Quotations_JobId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Quotations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Users",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDefault",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Jobs_JobId",
                table: "Quotations",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "JobId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
