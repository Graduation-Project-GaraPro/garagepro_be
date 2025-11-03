using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specification_SpecificationCategory_TemplateID",
                table: "Specification");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_Specification_FieldTemplateID",
                table: "SpecificationsData");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartInspections");

            migrationBuilder.RenameColumn(
                name: "FieldTemplateID",
                table: "SpecificationsData",
                newName: "SpecificationID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_LookupID_FieldTemplateID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_LookupID_SpecificationID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_FieldTemplateID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_SpecificationID");

            migrationBuilder.RenameColumn(
                name: "TemplateID",
                table: "Specification",
                newName: "CategoryID");

            migrationBuilder.RenameIndex(
                name: "IX_Specification_TemplateID_DisplayOrder",
                table: "Specification",
                newName: "IX_Specification_CategoryID_DisplayOrder");

            migrationBuilder.AddColumn<Guid>(
                name: "PartCategoryId",
                table: "PartInspections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ServicePartCategory",
                columns: table => new
                {
                    ServicePartCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePartCategory", x => x.ServicePartCategoryId);
                    table.ForeignKey(
                        name: "FK_ServicePartCategory_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "LaborCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePartCategory_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartInspections_PartCategoryId",
                table: "PartInspections",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePartCategory_PartCategoryId",
                table: "ServicePartCategory",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePartCategory_ServiceId",
                table: "ServicePartCategory",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartInspections_PartCategories_PartCategoryId",
                table: "PartInspections",
                column: "PartCategoryId",
                principalTable: "PartCategories",
                principalColumn: "LaborCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Specification_SpecificationCategory_CategoryID",
                table: "Specification",
                column: "CategoryID",
                principalTable: "SpecificationCategory",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_Specification_SpecificationID",
                table: "SpecificationsData",
                column: "SpecificationID",
                principalTable: "Specification",
                principalColumn: "SpecificationID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartInspections_PartCategories_PartCategoryId",
                table: "PartInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_Specification_SpecificationCategory_CategoryID",
                table: "Specification");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_Specification_SpecificationID",
                table: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "ServicePartCategory");

            migrationBuilder.DropIndex(
                name: "IX_PartInspections_PartCategoryId",
                table: "PartInspections");

            migrationBuilder.DropColumn(
                name: "PartCategoryId",
                table: "PartInspections");

            migrationBuilder.RenameColumn(
                name: "SpecificationID",
                table: "SpecificationsData",
                newName: "FieldTemplateID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_SpecificationID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_FieldTemplateID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_LookupID_SpecificationID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_LookupID_FieldTemplateID");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "Specification",
                newName: "TemplateID");

            migrationBuilder.RenameIndex(
                name: "IX_Specification_CategoryID_DisplayOrder",
                table: "Specification",
                newName: "IX_Specification_TemplateID_DisplayOrder");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PartInspections",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Specification_SpecificationCategory_TemplateID",
                table: "Specification",
                column: "TemplateID",
                principalTable: "SpecificationCategory",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_Specification_FieldTemplateID",
                table: "SpecificationsData",
                column: "FieldTemplateID",
                principalTable: "Specification",
                principalColumn: "SpecificationID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
