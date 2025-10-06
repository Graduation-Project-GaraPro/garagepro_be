using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class BranchAndPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServiceParts_Parts_PartId1",
                table: "RepairOrderServiceParts");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServices_Services_ServiceId1",
                table: "RepairOrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceInspections_Services_ServiceId1",
                table: "ServiceInspections");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInspections_ServiceId1",
                table: "ServiceInspections");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderServices_ServiceId1",
                table: "RepairOrderServices");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderServiceParts_PartId1",
                table: "RepairOrderServiceParts");

            migrationBuilder.DropColumn(
                name: "ServiceId1",
                table: "ServiceInspections");

            migrationBuilder.DropColumn(
                name: "ServiceId1",
                table: "RepairOrderServices");

            migrationBuilder.DropColumn(
                name: "PartId1",
                table: "RepairOrderServiceParts");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Branches");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Branches",
                newName: "Street");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Parts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedByManagerId",
                table: "Jobs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerApprovalNote",
                table: "Jobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerResponseAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimateExpiresAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalJobId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionCount",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RevisionReason",
                table: "Jobs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentToCustomerAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Branches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Users",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "BranchServices",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchServices", x => new { x.BranchId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_BranchServices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "ServiceParts",
                columns: table => new
                {
                    ServicePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceParts", x => x.ServicePartId);
                    table.ForeignKey(
                        name: "FK_ServiceParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceParts_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_BranchId",
                table: "Services",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchServices_ServiceId",
                table: "BranchServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHour_BranchId",
                table: "OperatingHour",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceParts_PartId",
                table: "ServiceParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceParts_ServiceId",
                table: "ServiceParts",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Branches_BranchId",
                table: "Services",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Branches_BranchId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "BranchServices");

            migrationBuilder.DropTable(
                name: "OperatingHour");

            migrationBuilder.DropTable(
                name: "ServiceParts");

            migrationBuilder.DropIndex(
                name: "IX_Services_BranchId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AssignedByManagerId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CustomerApprovalNote",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CustomerResponseAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "EstimateExpiresAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "OriginalJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RevisionCount",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RevisionReason",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SentToCustomerAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Branches",
                newName: "Address");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId1",
                table: "ServiceInspections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId1",
                table: "RepairOrderServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartId1",
                table: "RepairOrderServiceParts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                table: "RepairOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "BranchId",
                table: "Parts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Users",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInspections_ServiceId1",
                table: "ServiceInspections",
                column: "ServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServices_ServiceId1",
                table: "RepairOrderServices",
                column: "ServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServiceParts_PartId1",
                table: "RepairOrderServiceParts",
                column: "PartId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServiceParts_Parts_PartId1",
                table: "RepairOrderServiceParts",
                column: "PartId1",
                principalTable: "Parts",
                principalColumn: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServices_Services_ServiceId1",
                table: "RepairOrderServices",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceInspections_Services_ServiceId1",
                table: "ServiceInspections",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "ServiceId");
        }
    }
}
