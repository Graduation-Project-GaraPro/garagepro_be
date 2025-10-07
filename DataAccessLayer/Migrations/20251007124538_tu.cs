using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class tu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModel_VehicleBrand_BrandID",
                table: "VehicleModel");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModelColor_VehicleColor_ColorID",
                table: "VehicleModelColor");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModelColor_VehicleModel_ModelID",
                table: "VehicleModelColor");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleBrand_BrandId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleColor_ColorId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleModel_ModelId",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleModelColor",
                table: "VehicleModelColor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleModel",
                table: "VehicleModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleColor",
                table: "VehicleColor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleBrand",
                table: "VehicleBrand");

            migrationBuilder.RenameTable(
                name: "VehicleModelColor",
                newName: "VehicleModelColors");

            migrationBuilder.RenameTable(
                name: "VehicleModel",
                newName: "VehicleModels");

            migrationBuilder.RenameTable(
                name: "VehicleColor",
                newName: "VehicleColors");

            migrationBuilder.RenameTable(
                name: "VehicleBrand",
                newName: "VehicleBrands");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModelColor_ModelID",
                table: "VehicleModelColors",
                newName: "IX_VehicleModelColors_ModelID");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModelColor_ColorID",
                table: "VehicleModelColors",
                newName: "IX_VehicleModelColors_ColorID");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModel_BrandID",
                table: "VehicleModels",
                newName: "IX_VehicleModels_BrandID");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleModelColors",
                table: "VehicleModelColors",
                column: "VehicleModelColorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleModels",
                table: "VehicleModels",
                column: "ModelID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleColors",
                table: "VehicleColors",
                column: "ColorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleBrands",
                table: "VehicleBrands",
                column: "BrandID");

            migrationBuilder.CreateTable(
                name: "RepairRequests",
                columns: table => new
                {
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairRequests", x => x.RepairRequestID);
                    table.ForeignKey(
                        name: "FK_RepairRequests_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairRequests_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairImages",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairImages", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_RepairImages_RepairRequests_RepairRequestId",
                        column: x => x.RepairRequestId,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestParts",
                columns: table => new
                {
                    RequestPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    number = table.Column<int>(type: "int", nullable: false),
                    totalAmount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestParts", x => x.RequestPartId);
                    table.ForeignKey(
                        name: "FK_RequestParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestParts_RepairRequests_RepairRequestID",
                        column: x => x.RepairRequestID,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestServices",
                columns: table => new
                {
                    RequestServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    numberService = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestServices", x => x.RequestServiceId);
                    table.ForeignKey(
                        name: "FK_RequestServices_RepairRequests_RepairRequestId",
                        column: x => x.RepairRequestId,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairImages_RepairRequestId",
                table: "RepairImages",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_UserID",
                table: "RepairRequests",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_VehicleID",
                table: "RepairRequests",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParts_PartId",
                table: "RequestParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParts_RepairRequestID",
                table: "RequestParts",
                column: "RepairRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_RequestServices_RepairRequestId",
                table: "RequestServices",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestServices_ServiceId",
                table: "RequestServices",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModelColors_VehicleColors_ColorID",
                table: "VehicleModelColors",
                column: "ColorID",
                principalTable: "VehicleColors",
                principalColumn: "ColorID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModelColors_VehicleModels_ModelID",
                table: "VehicleModelColors",
                column: "ModelID",
                principalTable: "VehicleModels",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandID",
                table: "VehicleModels",
                column: "BrandID",
                principalTable: "VehicleBrands",
                principalColumn: "BrandID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleBrands_BrandId",
                table: "Vehicles",
                column: "BrandId",
                principalTable: "VehicleBrands",
                principalColumn: "BrandID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleColors_ColorId",
                table: "Vehicles",
                column: "ColorId",
                principalTable: "VehicleColors",
                principalColumn: "ColorID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleModels_ModelId",
                table: "Vehicles",
                column: "ModelId",
                principalTable: "VehicleModels",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModelColors_VehicleColors_ColorID",
                table: "VehicleModelColors");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModelColors_VehicleModels_ModelID",
                table: "VehicleModelColors");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleModels_VehicleBrands_BrandID",
                table: "VehicleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleBrands_BrandId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleColors_ColorId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleModels_ModelId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "RepairImages");

            migrationBuilder.DropTable(
                name: "RequestParts");

            migrationBuilder.DropTable(
                name: "RequestServices");

            migrationBuilder.DropTable(
                name: "RepairRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleModels",
                table: "VehicleModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleModelColors",
                table: "VehicleModelColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleColors",
                table: "VehicleColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleBrands",
                table: "VehicleBrands");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Vehicles");

            migrationBuilder.RenameTable(
                name: "VehicleModels",
                newName: "VehicleModel");

            migrationBuilder.RenameTable(
                name: "VehicleModelColors",
                newName: "VehicleModelColor");

            migrationBuilder.RenameTable(
                name: "VehicleColors",
                newName: "VehicleColor");

            migrationBuilder.RenameTable(
                name: "VehicleBrands",
                newName: "VehicleBrand");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModels_BrandID",
                table: "VehicleModel",
                newName: "IX_VehicleModel_BrandID");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModelColors_ModelID",
                table: "VehicleModelColor",
                newName: "IX_VehicleModelColor_ModelID");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleModelColors_ColorID",
                table: "VehicleModelColor",
                newName: "IX_VehicleModelColor_ColorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleModel",
                table: "VehicleModel",
                column: "ModelID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleModelColor",
                table: "VehicleModelColor",
                column: "VehicleModelColorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleColor",
                table: "VehicleColor",
                column: "ColorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleBrand",
                table: "VehicleBrand",
                column: "BrandID");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModel_VehicleBrand_BrandID",
                table: "VehicleModel",
                column: "BrandID",
                principalTable: "VehicleBrand",
                principalColumn: "BrandID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModelColor_VehicleColor_ColorID",
                table: "VehicleModelColor",
                column: "ColorID",
                principalTable: "VehicleColor",
                principalColumn: "ColorID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleModelColor_VehicleModel_ModelID",
                table: "VehicleModelColor",
                column: "ModelID",
                principalTable: "VehicleModel",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleBrand_BrandId",
                table: "Vehicles",
                column: "BrandId",
                principalTable: "VehicleBrand",
                principalColumn: "BrandID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleColor_ColorId",
                table: "Vehicles",
                column: "ColorId",
                principalTable: "VehicleColor",
                principalColumn: "ColorID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleModel_ModelId",
                table: "Vehicles",
                column: "ModelId",
                principalTable: "VehicleModel",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
