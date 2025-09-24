using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class Customer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogTag_SystemLog_LogId",
                table: "LogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLog_SystemLog_Id",
                table: "SecurityLog");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLogRelation_SecurityLog_SecurityLogId",
                table: "SecurityLogRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLogRelation_SystemLog_RelatedLogId",
                table: "SecurityLogRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLog_AspNetUsers_ApplicationUserId",
                table: "SystemLog");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLog_AspNetUsers_UserId",
                table: "SystemLog");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLog_LogCategory_CategoryId",
                table: "SystemLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemLog",
                table: "SystemLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecurityLogRelation",
                table: "SecurityLogRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecurityLog",
                table: "SecurityLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogTag",
                table: "LogTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogCategory",
                table: "LogCategory");

            migrationBuilder.RenameTable(
                name: "SystemLog",
                newName: "SystemLogs");

            migrationBuilder.RenameTable(
                name: "SecurityLogRelation",
                newName: "SecurityLogRelations");

            migrationBuilder.RenameTable(
                name: "SecurityLog",
                newName: "SecurityLogs");

            migrationBuilder.RenameTable(
                name: "LogTag",
                newName: "LogTags");

            migrationBuilder.RenameTable(
                name: "LogCategory",
                newName: "LogCategories");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLog_UserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLog_CategoryId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLog_ApplicationUserId",
                table: "SystemLogs",
                newName: "IX_SystemLogs_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityLogRelation_SecurityLogId",
                table: "SecurityLogRelations",
                newName: "IX_SecurityLogRelations_SecurityLogId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityLogRelation_RelatedLogId",
                table: "SecurityLogRelations",
                newName: "IX_SecurityLogRelations_RelatedLogId");

            migrationBuilder.RenameIndex(
                name: "IX_LogTag_LogId",
                table: "LogTags",
                newName: "IX_LogTags_LogId");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Vehicles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemLogs",
                table: "SystemLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecurityLogRelations",
                table: "SecurityLogRelations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecurityLogs",
                table: "SecurityLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogTags",
                table: "LogTags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogCategories",
                table: "LogCategories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CategoryNotifications",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNotifications", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Star = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedback_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairRequest",
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
                    table.PrimaryKey("PK_RepairRequest", x => x.RepairRequestID);
                    table.ForeignKey(
                        name: "FK_RepairRequest_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairRequest_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Repairs",
                columns: table => new
                {
                    RepairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualTime = table.Column<long>(type: "bigint", nullable: true),
                    EstimatedTime = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repairs", x => x.RepairId);
                    table.ForeignKey(
                        name: "FK_Repairs_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quality = table.Column<double>(type: "float", nullable: false),
                    Speed = table.Column<double>(type: "float", nullable: false),
                    Efficiency = table.Column<double>(type: "float", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.TechnicianId);
                    table.ForeignKey(
                        name: "FK_Technicians_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleLookups",
                columns: table => new
                {
                    LookupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Automaker = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameCar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLookups", x => x.LookupID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_CategoryNotifications_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "CategoryNotifications",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepairImage",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairImage", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_RepairImage_RepairRequest_RepairRequestId",
                        column: x => x.RepairRequestId,
                        principalTable: "RepairRequest",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestPart",
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
                    table.PrimaryKey("PK_RequestPart", x => x.RequestPartId);
                    table.ForeignKey(
                        name: "FK_RequestPart_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestPart_RepairRequest_RepairRequestID",
                        column: x => x.RepairRequestID,
                        principalTable: "RepairRequest",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestService",
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
                    table.PrimaryKey("PK_RequestService", x => x.RequestServiceId);
                    table.ForeignKey(
                        name: "FK_RequestService_RepairRequest_RepairRequestId",
                        column: x => x.RepairRequestId,
                        principalTable: "RepairRequest",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestService_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobTechnicians",
                columns: table => new
                {
                    JobTechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTechnicians", x => x.JobTechnicianId);
                    table.ForeignKey(
                        name: "FK_JobTechnicians_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTechnicians_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "TechnicianId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specifications",
                columns: table => new
                {
                    SpecificationsID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LookupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specifications", x => x.SpecificationsID);
                    table.ForeignKey(
                        name: "FK_Specifications_VehicleLookups_LookupID",
                        column: x => x.LookupID,
                        principalTable: "VehicleLookups",
                        principalColumn: "LookupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationsData",
                columns: table => new
                {
                    DataID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SpecificationsID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationsData", x => x.DataID);
                    table.ForeignKey(
                        name: "FK_SpecificationsData_Specifications_SpecificationsID",
                        column: x => x.SpecificationsID,
                        principalTable: "Specifications",
                        principalColumn: "SpecificationsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ApplicationUserId",
                table: "Vehicles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_TechnicianId",
                table: "Inspections",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_RepairOrderId",
                table: "Feedback",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserId",
                table: "Feedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTechnicians_JobId",
                table: "JobTechnicians",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTechnicians_TechnicianId",
                table: "JobTechnicians",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CategoryID",
                table: "Notifications",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairImage_RepairRequestId",
                table: "RepairImage",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequest_UserID",
                table: "RepairRequest",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequest_VehicleID",
                table: "RepairRequest",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestPart_PartId",
                table: "RequestPart",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestPart_RepairRequestID",
                table: "RequestPart",
                column: "RepairRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_RequestService_RepairRequestId",
                table: "RequestService",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestService_ServiceId",
                table: "RequestService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Specifications_LookupID",
                table: "Specifications",
                column: "LookupID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_SpecificationsID",
                table: "SpecificationsData",
                column: "SpecificationsID");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_UserId",
                table: "Technicians",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inspections_Technicians_TechnicianId",
                table: "Inspections",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "TechnicianId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LogTags_SystemLogs_LogId",
                table: "LogTags",
                column: "LogId",
                principalTable: "SystemLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLogRelations_SecurityLogs_SecurityLogId",
                table: "SecurityLogRelations",
                column: "SecurityLogId",
                principalTable: "SecurityLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLogRelations_SystemLogs_RelatedLogId",
                table: "SecurityLogRelations",
                column: "RelatedLogId",
                principalTable: "SystemLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLogs_SystemLogs_Id",
                table: "SecurityLogs",
                column: "Id",
                principalTable: "SystemLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_AspNetUsers_ApplicationUserId",
                table: "SystemLogs",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_AspNetUsers_UserId",
                table: "SystemLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_LogCategories_CategoryId",
                table: "SystemLogs",
                column: "CategoryId",
                principalTable: "LogCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_ApplicationUserId",
                table: "Vehicles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inspections_Technicians_TechnicianId",
                table: "Inspections");

            migrationBuilder.DropForeignKey(
                name: "FK_LogTags_SystemLogs_LogId",
                table: "LogTags");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLogRelations_SecurityLogs_SecurityLogId",
                table: "SecurityLogRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLogRelations_SystemLogs_RelatedLogId",
                table: "SecurityLogRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityLogs_SystemLogs_Id",
                table: "SecurityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_AspNetUsers_ApplicationUserId",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_AspNetUsers_UserId",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_LogCategories_CategoryId",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_ApplicationUserId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "JobTechnicians");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RepairImage");

            migrationBuilder.DropTable(
                name: "Repairs");

            migrationBuilder.DropTable(
                name: "RequestPart");

            migrationBuilder.DropTable(
                name: "RequestService");

            migrationBuilder.DropTable(
                name: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.DropTable(
                name: "CategoryNotifications");

            migrationBuilder.DropTable(
                name: "RepairRequest");

            migrationBuilder.DropTable(
                name: "Specifications");

            migrationBuilder.DropTable(
                name: "VehicleLookups");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_ApplicationUserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Inspections_TechnicianId",
                table: "Inspections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemLogs",
                table: "SystemLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecurityLogs",
                table: "SecurityLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SecurityLogRelations",
                table: "SecurityLogRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogTags",
                table: "LogTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogCategories",
                table: "LogCategories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "SystemLogs",
                newName: "SystemLog");

            migrationBuilder.RenameTable(
                name: "SecurityLogs",
                newName: "SecurityLog");

            migrationBuilder.RenameTable(
                name: "SecurityLogRelations",
                newName: "SecurityLogRelation");

            migrationBuilder.RenameTable(
                name: "LogTags",
                newName: "LogTag");

            migrationBuilder.RenameTable(
                name: "LogCategories",
                newName: "LogCategory");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_UserId",
                table: "SystemLog",
                newName: "IX_SystemLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_CategoryId",
                table: "SystemLog",
                newName: "IX_SystemLog_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SystemLogs_ApplicationUserId",
                table: "SystemLog",
                newName: "IX_SystemLog_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityLogRelations_SecurityLogId",
                table: "SecurityLogRelation",
                newName: "IX_SecurityLogRelation_SecurityLogId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityLogRelations_RelatedLogId",
                table: "SecurityLogRelation",
                newName: "IX_SecurityLogRelation_RelatedLogId");

            migrationBuilder.RenameIndex(
                name: "IX_LogTags_LogId",
                table: "LogTag",
                newName: "IX_LogTag_LogId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemLog",
                table: "SystemLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecurityLog",
                table: "SecurityLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SecurityLogRelation",
                table: "SecurityLogRelation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogTag",
                table: "LogTag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogCategory",
                table: "LogCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LogTag_SystemLog_LogId",
                table: "LogTag",
                column: "LogId",
                principalTable: "SystemLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLog_SystemLog_Id",
                table: "SecurityLog",
                column: "Id",
                principalTable: "SystemLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLogRelation_SecurityLog_SecurityLogId",
                table: "SecurityLogRelation",
                column: "SecurityLogId",
                principalTable: "SecurityLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityLogRelation_SystemLog_RelatedLogId",
                table: "SecurityLogRelation",
                column: "RelatedLogId",
                principalTable: "SystemLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLog_AspNetUsers_ApplicationUserId",
                table: "SystemLog",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLog_AspNetUsers_UserId",
                table: "SystemLog",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLog_LogCategory_CategoryId",
                table: "SystemLog",
                column: "CategoryId",
                principalTable: "LogCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
