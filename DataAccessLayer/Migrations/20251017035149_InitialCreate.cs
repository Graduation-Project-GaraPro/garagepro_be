using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiChatSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalMessages = table.Column<int>(type: "int", nullable: false),
                    DiagnosisResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatSessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true),
                    Users = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                });

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
                name: "Colors",
                columns: table => new
                {
                    ColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "LogCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    OrderStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.OrderStatusId);
                });

            migrationBuilder.CreateTable(
                name: "PartCategories",
                columns: table => new
                {
                    LaborCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCategories", x => x.LaborCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "PermissionCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionalCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionalCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.ServiceCategoryId);
                    table.ForeignKey(
                        name: "FK_ServiceCategories_ServiceCategories_ParentServiceCategoryId",
                        column: x => x.ParentServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBrands",
                columns: table => new
                {
                    BrandID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBrands", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "VehicleColors",
                columns: table => new
                {
                    ColorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleColors", x => x.ColorID);
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
                name: "AiChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_AiChatMessages_AiChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiChatSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPasswordChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OperatingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", maxLength: 5, nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatingHours_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                columns: table => new
                {
                    LabelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.LabelId);
                    table.ForeignKey(
                        name: "FK_Labels_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "ColorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Labels_OrderStatuses_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "OrderStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.PartId);
                    table.ForeignKey(
                        name: "FK_Parts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parts_PartCategories_PartCategoryId",
                        column: x => x.PartCategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "LaborCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Deprecated = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_PermissionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PermissionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDuration = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAdvanced = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_Services_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    ModelID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManufacturingYear = table.Column<int>(type: "int", nullable: false),
                    BrandID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModels", x => x.ModelID);
                    table.ForeignKey(
                        name: "FK_VehicleModels_VehicleBrands_BrandID",
                        column: x => x.BrandID,
                        principalTable: "VehicleBrands",
                        principalColumn: "BrandID",
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
                name: "AIDiagnostic_Keywords",
                columns: table => new
                {
                    KeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AIChatMessageMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AIDiagnostic_KeywordKeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIDiagnostic_Keywords", x => x.KeywordId);
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                        column: x => x.AIDiagnostic_KeywordKeywordId,
                        principalTable: "AIDiagnostic_Keywords",
                        principalColumn: "KeywordId");
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AiChatMessages_AIChatMessageMessageId",
                        column: x => x.AIChatMessageMessageId,
                        principalTable: "AiChatMessages",
                        principalColumn: "MessageId");
                    table.ForeignKey(
                        name: "FK_AIDiagnostic_Keywords_AiChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AiChatSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "SecurityPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinPasswordLength = table.Column<int>(type: "int", nullable: false),
                    RequireSpecialChar = table.Column<bool>(type: "bit", nullable: false),
                    RequireNumber = table.Column<bool>(type: "bit", nullable: false),
                    RequireUppercase = table.Column<bool>(type: "bit", nullable: false),
                    SessionTimeout = table.Column<int>(type: "int", nullable: false),
                    MaxLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountLockoutTime = table.Column<int>(type: "int", nullable: false),
                    MfaRequired = table.Column<bool>(type: "bit", nullable: false),
                    PasswordExpiryDays = table.Column<int>(type: "int", nullable: false),
                    EnableBruteForceProtection = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityPolicies_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemLogs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SystemLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SystemLogs_LogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "PartSpecifications",
                columns: table => new
                {
                    SpecId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartSpecifications", x => x.SpecId);
                    table.ForeignKey(
                        name: "FK_PartSpecifications_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrantedUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetUsers_GrantedUserId",
                        column: x => x.GrantedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionalCampaignServices",
                columns: table => new
                {
                    PromotionalCampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionalCampaignServices", x => new { x.PromotionalCampaignId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_PromotionalCampaignServices_PromotionalCampaigns_PromotionalCampaignId",
                        column: x => x.PromotionalCampaignId,
                        principalTable: "PromotionalCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionalCampaignServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceParts",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceParts", x => new { x.ServiceId, x.PartId });
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

            migrationBuilder.CreateTable(
                name: "VehicleModelColors",
                columns: table => new
                {
                    VehicleModelColorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModelColors", x => x.VehicleModelColorID);
                    table.ForeignKey(
                        name: "FK_VehicleModelColors_VehicleColors_ColorID",
                        column: x => x.ColorID,
                        principalTable: "VehicleColors",
                        principalColumn: "ColorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleModelColors_VehicleModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Odometer = table.Column<long>(type: "bigint", nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_Vehicles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "VehicleBrands",
                        principalColumn: "BrandID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleColors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "VehicleColors",
                        principalColumn: "ColorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "AIResponseTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AIDiagnostic_KeywordKeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResponseTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_AIResponseTemplates_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                        column: x => x.AIDiagnostic_KeywordKeywordId,
                        principalTable: "AIDiagnostic_Keywords",
                        principalColumn: "KeywordId");
                    table.ForeignKey(
                        name: "FK_AIResponseTemplates_AiChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "AiChatMessages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityPolicyHistories",
                columns: table => new
                {
                    HistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ChangeSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityPolicyHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_SecurityPolicyHistories_AspNetUsers_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityPolicyHistories_SecurityPolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "SecurityPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogId = table.Column<long>(type: "bigint", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogTags_SystemLogs_LogId",
                        column: x => x.LogId,
                        principalTable: "SystemLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ThreatLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityLogs_SystemLogs_Id",
                        column: x => x.Id,
                        principalTable: "SystemLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairRequests",
                columns: table => new
                {
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
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
                        name: "FK_RepairRequests_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairRequests_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityLogRelations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecurityLogId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedLogId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityLogRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityLogRelations_SecurityLogs_SecurityLogId",
                        column: x => x.SecurityLogId,
                        principalTable: "SecurityLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityLogRelations_SystemLogs_RelatedLogId",
                        column: x => x.RelatedLogId,
                        principalTable: "SystemLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "RequestServices",
                columns: table => new
                {
                    RequestServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RequestParts",
                columns: table => new
                {
                    RequestPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    totalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                        name: "FK_RequestParts_RequestServices_RequestServiceId",
                        column: x => x.RequestServiceId,
                        principalTable: "RequestServices",
                        principalColumn: "RequestServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedBacks",
                columns: table => new
                {
                    FeedBackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedBacks", x => x.FeedBackId);
                    table.ForeignKey(
                        name: "FK_FeedBacks_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeedBacks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrders",
                columns: table => new
                {
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepairOrderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoType = table.Column<int>(type: "int", nullable: false),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimatedRepairTime = table.Column<long>(type: "bigint", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LabelId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RepairRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedBackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedBackId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrders", x => x.RepairOrderId);
                    table.ForeignKey(
                        name: "FK_RepairOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrders_FeedBacks_FeedBackId1",
                        column: x => x.FeedBackId1,
                        principalTable: "FeedBacks",
                        principalColumn: "FeedBackId");
                    table.ForeignKey(
                        name: "FK_RepairOrders_OrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "OrderStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerConcern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Finding = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.InspectionId);
                    table.ForeignKey(
                        name: "FK_Inspections_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "TechnicianId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    SentToCustomerAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerApprovalNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedByManagerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimateExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisionCount = table.Column<int>(type: "int", nullable: false),
                    OriginalJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevisionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_Jobs_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrderRepairRequest",
                columns: table => new
                {
                    RepairOrdersRepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderRepairRequest", x => new { x.RepairOrdersRepairOrderId, x.RepairRequestID });
                    table.ForeignKey(
                        name: "FK_RepairOrderRepairRequest_RepairOrders_RepairOrdersRepairOrderId",
                        column: x => x.RepairOrdersRepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderRepairRequest_RepairRequests_RepairRequestID",
                        column: x => x.RepairRequestID,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairOrderServices",
                columns: table => new
                {
                    RepairOrderServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualDuration = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderServices", x => x.RepairOrderServiceId);
                    table.ForeignKey(
                        name: "FK_RepairOrderServices_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoucherUsage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherUsage_AspNetUsers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherUsage_PromotionalCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionalCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoucherUsage_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartInspections",
                columns: table => new
                {
                    PartInspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartInspections", x => x.PartInspectionId);
                    table.ForeignKey(
                        name: "FK_PartInspections_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "InspectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartInspections_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    QuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentToCustomerAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RepairRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.QuotationId);
                    table.ForeignKey(
                        name: "FK_Quotations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_Quotations_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "InspectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quotations_RepairRequests_RepairRequestID",
                        column: x => x.RepairRequestID,
                        principalTable: "RepairRequests",
                        principalColumn: "RepairRequestID");
                    table.ForeignKey(
                        name: "FK_Quotations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceInspections",
                columns: table => new
                {
                    ServiceInspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceInspections", x => x.ServiceInspectionId);
                    table.ForeignKey(
                        name: "FK_ServiceInspections_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "InspectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceInspections_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobParts",
                columns: table => new
                {
                    JobPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobParts", x => x.JobPartId);
                    table.ForeignKey(
                        name: "FK_JobParts_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "RepairOrderServiceParts",
                columns: table => new
                {
                    RepairOrderServicePartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairOrderServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderServiceParts", x => x.RepairOrderServicePartId);
                    table.ForeignKey(
                        name: "FK_RepairOrderServiceParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrderServiceParts_RepairOrderServices_RepairOrderServiceId",
                        column: x => x.RepairOrderServiceId,
                        principalTable: "RepairOrderServices",
                        principalColumn: "RepairOrderServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationServices",
                columns: table => new
                {
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_QuotationServices_Services_ServiceId1",
                        column: x => x.ServiceId1,
                        principalTable: "Services",
                        principalColumn: "ServiceId");
                });

            migrationBuilder.CreateTable(
                name: "QuotationParts",
                columns: table => new
                {
                    QuotationPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationParts", x => x.QuotationPartId);
                    table.ForeignKey(
                        name: "FK_QuotationParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationParts_Parts_PartId1",
                        column: x => x.PartId1,
                        principalTable: "Parts",
                        principalColumn: "PartId");
                    table.ForeignKey(
                        name: "FK_QuotationParts_QuotationServices_QuotationServiceId",
                        column: x => x.QuotationServiceId,
                        principalTable: "QuotationServices",
                        principalColumn: "QuotationServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessages_SessionId",
                table: "AiChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_AIChatMessageMessageId",
                table: "AIDiagnostic_Keywords",
                column: "AIChatMessageMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_AIDiagnostic_KeywordKeywordId",
                table: "AIDiagnostic_Keywords",
                column: "AIDiagnostic_KeywordKeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_AIDiagnostic_Keywords_SessionId",
                table: "AIDiagnostic_Keywords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResponseTemplates_AIDiagnostic_KeywordKeywordId",
                table: "AIResponseTemplates",
                column: "AIDiagnostic_KeywordKeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResponseTemplates_MessageId",
                table: "AIResponseTemplates",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BranchServices_ServiceId",
                table: "BranchServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_ApplicationUserId",
                table: "FeedBacks",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_RepairOrderId",
                table: "FeedBacks",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedBacks_UserId",
                table: "FeedBacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_RepairOrderId",
                table: "Inspections",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_TechnicianId",
                table: "Inspections",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_JobParts_JobId",
                table: "JobParts",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobParts_PartId",
                table: "JobParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RepairOrderId",
                table: "Jobs",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ServiceId",
                table: "Jobs",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTechnicians_JobId",
                table: "JobTechnicians",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTechnicians_TechnicianId",
                table: "JobTechnicians",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_ColorId",
                table: "Labels",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_OrderStatusId",
                table: "Labels",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LogTags_LogId",
                table: "LogTags",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CategoryID",
                table: "Notifications",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingHours_BranchId",
                table: "OperatingHours",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInspections_InspectionId",
                table: "PartInspections",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PartInspections_PartId",
                table: "PartInspections",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BranchId",
                table: "Parts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartCategoryId",
                table: "Parts",
                column: "PartCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSpecifications_PartId",
                table: "PartSpecifications",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RepairOrderId",
                table: "Payments",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CategoryId",
                table: "Permissions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionalCampaignServices_ServiceId",
                table: "PromotionalCampaignServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_PartId",
                table: "QuotationParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_PartId1",
                table: "QuotationParts",
                column: "PartId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationParts_QuotationServiceId",
                table: "QuotationParts",
                column: "QuotationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_BranchId",
                table: "Quotations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_InspectionId",
                table: "Quotations",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RepairRequestID",
                table: "Quotations",
                column: "RepairRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_UserId",
                table: "Quotations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_VehicleId",
                table: "Quotations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServices_QuotationId",
                table: "QuotationServices",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServices_ServiceId",
                table: "QuotationServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationServices_ServiceId1",
                table: "QuotationServices",
                column: "ServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairImages_RepairRequestId",
                table: "RepairImages",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderRepairRequest_RepairRequestID",
                table: "RepairOrderRepairRequest",
                column: "RepairRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_BranchId",
                table: "RepairOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_FeedBackId1",
                table: "RepairOrders",
                column: "FeedBackId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_StatusId",
                table: "RepairOrders",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_UserId",
                table: "RepairOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_VehicleId",
                table: "RepairOrders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServiceParts_PartId",
                table: "RepairOrderServiceParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServiceParts_RepairOrderServiceId",
                table: "RepairOrderServiceParts",
                column: "RepairOrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServices_RepairOrderId",
                table: "RepairOrderServices",
                column: "RepairOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServices_ServiceId",
                table: "RepairOrderServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_BranchId",
                table: "RepairRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_UserID",
                table: "RepairRequests",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_VehicleID",
                table: "RepairRequests",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_JobId",
                table: "Repairs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParts_PartId",
                table: "RequestParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestParts_RequestServiceId",
                table: "RequestParts",
                column: "RequestServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestServices_RepairRequestId",
                table: "RequestServices",
                column: "RepairRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestServices_ServiceId",
                table: "RequestServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GrantedUserId",
                table: "RolePermissions",
                column: "GrantedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogRelations_RelatedLogId",
                table: "SecurityLogRelations",
                column: "RelatedLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLogRelations_SecurityLogId",
                table: "SecurityLogRelations",
                column: "SecurityLogId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicies_UpdatedBy",
                table: "SecurityPolicies",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicyHistories_ChangedBy",
                table: "SecurityPolicyHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicyHistories_PolicyId",
                table: "SecurityPolicyHistories",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_ParentServiceCategoryId",
                table: "ServiceCategories",
                column: "ParentServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInspections_InspectionId",
                table: "ServiceInspections",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInspections_ServiceId",
                table: "ServiceInspections",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceParts_PartId",
                table: "ServiceParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceCategoryId",
                table: "Services",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Specifications_LookupID",
                table: "Specifications",
                column: "LookupID");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationsData_SpecificationsID",
                table: "SpecificationsData",
                column: "SpecificationsID");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_ApplicationUserId",
                table: "SystemLogs",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CategoryId",
                table: "SystemLogs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_UserId",
                table: "SystemLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_UserId",
                table: "Technicians",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelColors_ColorID",
                table: "VehicleModelColors",
                column: "ColorID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelColors_ModelID",
                table: "VehicleModelColors",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandID",
                table: "VehicleModels",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_BrandId",
                table: "Vehicles",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ColorId",
                table: "Vehicles",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ModelId",
                table: "Vehicles",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsage_CampaignId",
                table: "VoucherUsage",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsage_CustomerId",
                table: "VoucherUsage",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsage_RepairOrderId",
                table: "VoucherUsage",
                column: "RepairOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_RepairOrders_RepairOrderId",
                table: "FeedBacks",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "RepairOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_AspNetUsers_ApplicationUserId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_AspNetUsers_UserId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_AspNetUsers_UserId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_UserId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Branches_BranchId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_RepairOrders_RepairOrderId",
                table: "FeedBacks");

            migrationBuilder.DropTable(
                name: "AIResponseTemplates");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BranchServices");

            migrationBuilder.DropTable(
                name: "JobParts");

            migrationBuilder.DropTable(
                name: "JobTechnicians");

            migrationBuilder.DropTable(
                name: "Labels");

            migrationBuilder.DropTable(
                name: "LogTags");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OperatingHours");

            migrationBuilder.DropTable(
                name: "PartInspections");

            migrationBuilder.DropTable(
                name: "PartSpecifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PromotionalCampaignServices");

            migrationBuilder.DropTable(
                name: "QuotationParts");

            migrationBuilder.DropTable(
                name: "RepairImages");

            migrationBuilder.DropTable(
                name: "RepairOrderRepairRequest");

            migrationBuilder.DropTable(
                name: "RepairOrderServiceParts");

            migrationBuilder.DropTable(
                name: "Repairs");

            migrationBuilder.DropTable(
                name: "RequestParts");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SecurityLogRelations");

            migrationBuilder.DropTable(
                name: "SecurityPolicyHistories");

            migrationBuilder.DropTable(
                name: "ServiceInspections");

            migrationBuilder.DropTable(
                name: "ServiceParts");

            migrationBuilder.DropTable(
                name: "SpecificationsData");

            migrationBuilder.DropTable(
                name: "VehicleModelColors");

            migrationBuilder.DropTable(
                name: "VoucherUsage");

            migrationBuilder.DropTable(
                name: "AIDiagnostic_Keywords");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "CategoryNotifications");

            migrationBuilder.DropTable(
                name: "QuotationServices");

            migrationBuilder.DropTable(
                name: "RepairOrderServices");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "RequestServices");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "SecurityLogs");

            migrationBuilder.DropTable(
                name: "SecurityPolicies");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Specifications");

            migrationBuilder.DropTable(
                name: "PromotionalCampaigns");

            migrationBuilder.DropTable(
                name: "AiChatMessages");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "PermissionCategories");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "PartCategories");

            migrationBuilder.DropTable(
                name: "VehicleLookups");

            migrationBuilder.DropTable(
                name: "AiChatSessions");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "RepairRequests");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "LogCategories");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "RepairOrders");

            migrationBuilder.DropTable(
                name: "FeedBacks");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleColors");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropTable(
                name: "VehicleBrands");
        }
    }
}
