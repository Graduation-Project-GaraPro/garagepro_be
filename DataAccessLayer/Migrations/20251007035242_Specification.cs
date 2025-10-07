using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Specification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_SpecificationCategory_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_DisplayOrder",
                table: "SpecificationsData");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_LookupID_CategoryID",
                table: "SpecificationsData");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SpecificationsData");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "SpecificationsData");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "SpecificationsData",
                newName: "FieldTemplateID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_CategoryID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_FieldTemplateID");

            migrationBuilder.CreateTable(
                name: "Specification",
                columns: table => new
                {
                    SpecificationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specification", x => x.SpecificationID);
                    table.ForeignKey(
                        name: "FK_Specification_SpecificationCategory_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "SpecificationCategory",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLookups_Automaker_NameCar",
                table: "VehicleLookups",
                columns: new[] { "Automaker", "NameCar" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_LookupID_FieldTemplateID",
                table: "SpecificationsData",
                columns: new[] { "LookupID", "FieldTemplateID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specification_Label",
                table: "Specification",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_Specification_TemplateID_DisplayOrder",
                table: "Specification",
                columns: new[] { "TemplateID", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_Specification_FieldTemplateID",
                table: "SpecificationsData",
                column: "FieldTemplateID",
                principalTable: "Specification",
                principalColumn: "SpecificationID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecificationsData_Specification_FieldTemplateID",
                table: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "Specification");

            migrationBuilder.DropIndex(
                name: "IX_VehicleLookups_Automaker_NameCar",
                table: "VehicleLookups");

            migrationBuilder.DropIndex(
                name: "IX_SpecificationsData_LookupID_FieldTemplateID",
                table: "SpecificationsData");

            migrationBuilder.RenameColumn(
                name: "FieldTemplateID",
                table: "SpecificationsData",
                newName: "CategoryID");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificationsData_FieldTemplateID",
                table: "SpecificationsData",
                newName: "IX_SpecificationsData_CategoryID");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SpecificationsData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "SpecificationsData",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_DisplayOrder",
                table: "SpecificationsData",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_LookupID_CategoryID",
                table: "SpecificationsData",
                columns: new[] { "LookupID", "CategoryID" });

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificationsData_SpecificationCategory_CategoryID",
                table: "SpecificationsData",
                column: "CategoryID",
                principalTable: "SpecificationCategory",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
