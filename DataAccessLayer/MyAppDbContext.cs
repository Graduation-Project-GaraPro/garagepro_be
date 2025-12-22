﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using BusinessObject;
using BusinessObject.AiChat;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Campaigns;
using BusinessObject.Customers;
using BusinessObject.Manager;
using BusinessObject.Notifications;
using BusinessObject.Policies;
using BusinessObject.Roles;
using BusinessObject.SystemLogs;
using BusinessObject.Vehicles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Notifications;
using BusinessObject.InspectionAndRepair;
using BusinessObject.Roles;
using BusinessObject.Branches;
using BusinessObject.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Manager;
using BusinessObject.RequestEmergency;
using BusinessObject.PayOsModels;

namespace DataAccessLayer
{
    public class MyAppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public MyAppDbContext(DbContextOptions<MyAppDbContext> options)
            : base(options) { }

        // Manager-related entities
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<InspectionType> InspectionTypes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PartCategory> PartCategories { get; set; }
        public DbSet<PartInventory> PartInventories { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationService> QuotationServices { get; set; }


        public DbSet<WebhookInbox> WebhookInboxes { get; set; }

        public DbSet<RequestEmergency> RequestEmergencies { get; set; }
        public DbSet<PriceEmergency> PriceEmergencies { get; set; }
        public DbSet<EmergencyMedia> EmergencyMedias { get; set; }
        public DbSet<QuotationServicePart> QuotationServiceParts { get; set; }

        // Junction tables
        //Technician
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<SpecificationCategory> SpecificationCategory { get; set; }
        public DbSet<Specification> Specification { get; set; }

        public DbSet<JobPart> JobParts { get; set; }
        public DbSet<JobTechnician> JobTechnicians { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        //public DbSet<Specifications> Specifications { get; set; }
        public DbSet<SpecificationsData> SpecificationsData { get; set; }
        public DbSet<VehicleLookup> VehicleLookups { get; set; }
        public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<SecurityPolicyHistory> SecurityPolicyHistories { get; set; }

        public DbSet<RepairOrderService> RepairOrderServices { get; set; }
        public DbSet<RepairOrderServicePart> RepairOrderServiceParts { get; set; }
        public DbSet<ServiceInspection> ServiceInspections { get; set; }
        public DbSet<ServicePartCategory> ServicePartCategories { get; set; }
        public DbSet<PartInspection> PartInspections { get; set; }

      

        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<PermissionCategory> PermissionCategories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<BranchService> BranchServices { get; set; }
        public DbSet<OperatingHour> OperatingHours { get; set; }

        //AiChat
        public DbSet<AIChatMessage> AiChatMessages { get; set; }
        public DbSet<AIChatSession> AiChatSessions { get; set; }

        public DbSet<AIDiagnostic_Keyword> AIDiagnostic_Keywords { get; set; }
        public DbSet<AIResponseTemplate> AIResponseTemplates { get; set; }
        //Vehicle
        //public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleColor> VehicleColors { get; set; }
        public DbSet<VehicleModelColor> VehicleModelColors { get; set; }
        //Customer
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<RepairImage> RepairImages { get; set; }
        public DbSet<RequestPart> RequestParts { get; set; }
        public DbSet<RequestService> RequestServices { get; set; }

        public DbSet<PromotionalCampaign> PromotionalCampaigns { get; set; }
        public DbSet<PromotionalCampaignService> PromotionalCampaignServices { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Quotation relationships
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasOne(q => q.Inspection)
                      .WithMany(i => i.Quotations)
                      .HasForeignKey(q => q.InspectionId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Made the relationship optional

                entity.HasOne(q => q.User)
                      .WithMany()
                      .HasForeignKey(q => q.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.Vehicle)
                      .WithMany()
                      .HasForeignKey(q => q.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Add relationship with RepairOrder
                entity.HasOne(q => q.RepairOrder)
                      .WithMany(ro => ro.Quotations)
                      .HasForeignKey(q => q.RepairOrderId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Configure the Status property to use the enum
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

            });

            modelBuilder.Entity<QuotationService>(entity =>
            {
                entity.HasOne(qs => qs.Quotation)
                      .WithMany(q => q.QuotationServices)
                      .HasForeignKey(qs => qs.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qs => qs.Service)
                      .WithMany()
                      .HasForeignKey(qs => qs.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Add the new relationship with QuotationServicePart
                entity.HasMany(qs => qs.QuotationServiceParts)
                      .WithOne(qsp => qsp.QuotationService)
                      .HasForeignKey(qsp => qsp.QuotationServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Thêm khóa ngoại đến PromotionalCampaign
               
                    entity.HasOne(qs => qs.AppliedPromotion)
                        .WithMany(pc => pc.QuotationServices)
                        .HasForeignKey(qs => qs.AppliedPromotionId)
                        .OnDelete(DeleteBehavior.Restrict);
                
            });

            // Add the new QuotationServicePart configuration
            modelBuilder.Entity<QuotationServicePart>(entity =>
            {
                entity.HasOne(qsp => qsp.QuotationService)
                      .WithMany(qs => qs.QuotationServiceParts)
                      .HasForeignKey(qsp => qsp.QuotationServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qsp => qsp.Part)
                      .WithMany()
                      .HasForeignKey(qsp => qsp.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Removed the configuration for QuotationPart and QuotationService relationship
            // as QuotationPart entity was removed

            //chặn casadate
            modelBuilder.Entity<Vehicle>()
              .HasOne(v => v.Brand)
              .WithMany(b => b.Vehicles)
              .HasForeignKey(v => v.BrandId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Color)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Explicitly configure the ApplicationRole properties to ensure they map correctly
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).IsRequired(false);
                entity.Property(e => e.Users).HasDefaultValue(0);
            });

            // Cấu hình bảng RolePermission
            modelBuilder.Entity<RolePermission>(entity =>
            {
                // Khóa chính composite: RoleId + PermissionId
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                // Quan hệ RolePermission -> Role
                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions) // chỉ rõ navigation property
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade); // tùy chọn xóa

                // Quan hệ RolePermission -> Permission
                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ RolePermission -> User (ai gán quyền)
                entity.HasOne(rp => rp.User)
                      .WithMany() // nếu ApplicationUser không có collection RolePermissions
                      .HasForeignKey(rp => rp.GrantedUserId)
                      .OnDelete(DeleteBehavior.Restrict); // không xóa user thì quyền vẫn giữ
            });

            // --- Nếu muốn, bạn có thể cấu hình thêm default value cho CreatedAt/UpdatedAt ---
            modelBuilder.Entity<Permission>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Quan hệ Permission -> Category
            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Permissions)
                .HasForeignKey(p => p.CategoryId);

            // 🔧 Cấu hình tự sinh Guid cho PermissionCategory.Id
            modelBuilder.Entity<PermissionCategory>()
                .Property(pc => pc.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()"); // hoặc NEWID()

            // 🔧 Cấu hình tự sinh Guid cho Permission.Id
            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // ApplicationUser configuration
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FirstName).HasMaxLength(50);
                b.Property(u => u.LastName).HasMaxLength(50);
                b.Property(u => u.Email).HasMaxLength(100).IsRequired();
                b.Property(u => u.PhoneNumber).HasMaxLength(20);
                b.Property(u => u.AvatarUrl).HasMaxLength(200);
                b.Property(u => u.Status).HasMaxLength(50);

                b.HasMany(u => u.SystemLogs)
                 .WithOne(l => l.User)
                 .HasForeignKey(l => l.UserId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(u => u.Notifications) // Quan hệ với Notifications
                 .WithOne(n => n.User)
                 .HasForeignKey(n => n.UserID)
                 .OnDelete(DeleteBehavior.Restrict); // Tránh xóa liên quan

                b.HasOne(u => u.Technician) // Quan hệ một-một với Technician
                 .WithOne(t => t.User)
                 .HasForeignKey<Technician>(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade); // Xóa Technician nếu User bị xóa
            });

            //// CategoryNotification configuration
            //modelBuilder.Entity<CategoryNotification>(entity =>
            //{
            //    entity.HasKey(e => e.CategoryID);
            //    entity.Property(e => e.CategoryName)
            //          .IsRequired()
            //          .HasMaxLength(100);

            //    entity.HasMany(e => e.Notifications)
            //          .WithOne(n => n.CategoryNotification)
            //          .HasForeignKey(n => n.CategoryID)
            //          .OnDelete(DeleteBehavior.Restrict); // Tránh xóa liên quan
            //});

            modelBuilder.Entity<WebhookInbox>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderCode);
                entity.HasIndex(e => new { e.Status, e.Attempts, e.ReceivedAt });

                //  Enum -> string mapping
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });


            // Notifications configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(e => e.Type)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.Target)
                      .HasMaxLength(200);
                entity.Property(e => e.TimeSent)
                      .IsRequired();

                //// Quan hệ với CategoryNotification
                //entity.HasOne(e => e.CategoryNotification)
                //      .WithMany(c => c.Notifications)
                //      .HasForeignKey(e => e.CategoryID)
                //      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với ApplicationUser
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Technician configuration
            modelBuilder.Entity<Technician>(entity =>
            {
                entity.HasKey(e => e.TechnicianId);
                entity.Property(e => e.Quality).HasColumnType("float");
                entity.Property(e => e.Speed).HasColumnType("float");
                entity.Property(e => e.Efficiency).HasColumnType("float");
                entity.Property(e => e.Score).HasColumnType("float");

                entity.HasMany(t => t.Inspections)
                      .WithOne(i => i.Technician)
                      .HasForeignKey(i => i.TechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(t => t.JobTechnicians)
                      .WithOne(jt => jt.Technician)
                      .HasForeignKey(jt => jt.TechnicianId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // JobTechnician configuration
            modelBuilder.Entity<JobTechnician>(entity =>
            {
                entity.HasKey(e => e.JobTechnicianId);

                entity.HasOne(jt => jt.Job)
                      .WithMany(j => j.JobTechnicians)
                      .HasForeignKey(jt => jt.JobId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(jt => jt.Technician)
                      .WithMany(t => t.JobTechnicians)
                      .HasForeignKey(jt => jt.TechnicianId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Job configuration
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.JobName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                // Customer approval workflow properties
                entity.Property(e => e.AssignedByManagerId).HasMaxLength(450); // Standard ASP.NET Identity user ID length
                entity.Property(e => e.RevisionReason).HasMaxLength(500); // Reason for estimate revision

                entity.HasMany(j => j.JobTechnicians)
                      .WithOne(jt => jt.Job)
                      .HasForeignKey(jt => jt.JobId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(j => j.Repair)
                      .WithOne(r => r.Job)
                      .HasForeignKey<Repair>(r => r.JobId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure the relationship with the original job
                entity.HasOne(j => j.OriginalJob)
                      .WithMany()
                      .HasForeignKey(j => j.OriginalJobId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Repair configuration
            modelBuilder.Entity<Repair>(entity =>
            {
                entity.HasKey(e => e.RepairId);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasIndex(e => e.JobId).IsUnique();
                entity.HasOne(r => r.Job)
                      .WithOne(j => j.Repair)
                      .HasForeignKey<Repair>(r => r.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Inspection configuration
            modelBuilder.Entity<Inspection>(entity =>
            {
                entity.HasKey(e => e.InspectionId);
                entity.Property(e => e.CustomerConcern).HasMaxLength(500);
                entity.Property(e => e.Finding).HasMaxLength(500);
                entity.Property(e => e.Note).HasMaxLength(500);              
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.IssueRating)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(i => i.Technician)
                      .WithMany(t => t.Inspections)
                      .HasForeignKey(i => i.TechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // SpecificationCategory configuration
            modelBuilder.Entity<SpecificationCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryID);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.DisplayOrder)
                      .IsRequired();

                // Index để tăng performance khi sort
                entity.HasIndex(e => e.DisplayOrder);

                // Relationship với Specification
                entity.HasMany(e => e.Specifications)
                      .WithOne(s => s.SpecificationCategory)
                      .HasForeignKey(s => s.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa category nếu có specifications
            });

            // Specification configuration
            modelBuilder.Entity<Specification>(entity =>
            {
                entity.HasKey(e => e.SpecificationID);

                entity.Property(e => e.Label)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.DisplayOrder)
                      .IsRequired();

                // Relationship với SpecificationCategory
                entity.HasOne(s => s.SpecificationCategory)
                      .WithMany(c => c.Specifications)
                      .HasForeignKey(s => s.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship với SpecificationsData
                entity.HasMany(s => s.SpecificationsDatas)
                      .WithOne(d => d.Specification)
                      .HasForeignKey(d => d.SpecificationID)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa specification nếu có data

                // Composite Index để tăng performance khi query theo category và sort
                entity.HasIndex(e => new { e.CategoryID, e.DisplayOrder });

                // Index để search theo Label
                entity.HasIndex(e => e.Label);
            });

            // VehicleLookup configuration
            modelBuilder.Entity<VehicleLookup>(entity =>
            {
                entity.HasKey(e => e.LookupID);

                entity.Property(e => e.Automaker)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.NameCar)
                      .IsRequired()
                      .HasMaxLength(100);

                // Relationship với SpecificationsData
                entity.HasMany(e => e.SpecificationsDatas)
                      .WithOne(d => d.VehicleLookup)
                      .HasForeignKey(d => d.LookupID)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa xe thì xóa hết data của xe đó

                // Index để search theo Automaker và NameCar
                entity.HasIndex(e => new { e.Automaker, e.NameCar });
            });

            // SpecificationsData configuration
            modelBuilder.Entity<SpecificationsData>(entity =>
            {
                entity.HasKey(e => e.DataID);

                entity.Property(e => e.Value)
                      .IsRequired()
                      .HasMaxLength(200);

                // Relationship với VehicleLookup
                entity.HasOne(d => d.VehicleLookup)
                      .WithMany(v => v.SpecificationsDatas)
                      .HasForeignKey(d => d.LookupID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship với Specification
                entity.HasOne(d => d.Specification)
                      .WithMany(s => s.SpecificationsDatas)
                      .HasForeignKey(d => d.SpecificationID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Composite Index để tăng performance khi query theo xe và specification
                entity.HasIndex(e => new { e.LookupID, e.SpecificationID })
                      .IsUnique(); // Đảm bảo mỗi xe chỉ có 1 giá trị cho mỗi specification

                // Index để query theo FieldTemplateID (khi muốn xem tất cả xe có specification này)
                entity.HasIndex(e => e.SpecificationID);
            });

            //Log System
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.ToTable("SystemLogs");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                entity.Property(e => e.Level)
                    .HasConversion<string>() // Lưu enum dưới dạng string (vd: "Info", "Error")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Tag)
                    .HasConversion<string>() // nếu bạn có enum LogTagType
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.Source)
               .HasConversion<string>()    // ✅ Lưu dưới dạng nvarchar
               .HasMaxLength(50)
               .IsRequired();

                entity.Property(e => e.Message)
                    .IsRequired();

                entity.Property(e => e.Details)
                    .IsRequired(false);

                entity.Property(e => e.UserId)
                    .HasMaxLength(450) // ASP.NET Identity mặc định là nvarchar(450)
                    .IsRequired(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(255)
                    .IsRequired(false);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45)
                    .IsRequired(false);

                entity.Property(e => e.UserAgent)
                    .IsRequired(false);

                entity.Property(e => e.SessionId)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.RequestId)
                    .HasMaxLength(100)
                    .IsRequired(false);

                // Quan hệ với ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany() // Nếu ApplicationUser chưa có ICollection<SystemLog>
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);



                // Index giúp truy vấn nhanh hơn
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => e.UserId);

            });







            // -------- SecurityPolicies --------
            modelBuilder.Entity<SecurityPolicy>(entity =>
            {
                entity.ToTable("SecurityPolicies");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.MinPasswordLength).IsRequired();
                entity.Property(e => e.RequireSpecialChar).IsRequired();
                entity.Property(e => e.RequireNumber).IsRequired();
                entity.Property(e => e.RequireUppercase).IsRequired();
                entity.Property(e => e.SessionTimeout).IsRequired();
                entity.Property(e => e.MaxLoginAttempts).IsRequired();
                entity.Property(e => e.AccountLockoutTime).IsRequired();

                entity.Property(e => e.PasswordExpiryDays).IsRequired();
                entity.Property(e => e.EnableBruteForceProtection).IsRequired();

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("SYSUTCDATETIME()")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("SYSUTCDATETIME()")
                      .ValueGeneratedNever();

                entity.HasOne(e => e.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // -------- SecurityPolicyHistories --------
            modelBuilder.Entity<SecurityPolicyHistory>(entity =>
            {
                entity.ToTable("SecurityPolicyHistories");

                entity.HasKey(e => e.HistoryId);
                entity.Property(e => e.HistoryId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.ChangeSummary)
                      .HasMaxLength(500);

                entity.Property(e => e.ChangedAt)
                      .HasDefaultValueSql("SYSUTCDATETIME()")
                      .ValueGeneratedOnAdd();

                entity.HasOne(e => e.Policy)
                      .WithMany(p => p.Histories)
                      .HasForeignKey(e => e.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ChangedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure relationships to prevent cascade delete cycles

            // RepairOrder relationships - prevent cascade delete conflicts

            modelBuilder.Entity<RepairRequest>()
                .Ignore(rr => rr.RepairOrder);

            modelBuilder.Entity<RepairRequest>().HasIndex(r => new { r.VehicleID, r.RequestDate })
              .HasDatabaseName("UX_RepairRequests_VehicleRequestDate_Active")
              .IsUnique()
              .HasFilter("[Status] IN (0,1,2)");

            modelBuilder.Entity<RepairOrder>()
             .HasOne(ro => ro.RepairRequest)
             .WithOne()
             .HasForeignKey<RepairOrder>(ro => ro.RepairRequestId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
            .Property(r => r.CarPickupStatus)
            .HasConversion<int>();

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.User)
                .WithMany()
                .HasForeignKey(ro => ro.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.OrderStatus)
                .WithMany(os => os.RepairOrders)
                .HasForeignKey(ro => ro.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Vehicle)
                .WithMany(v => v.RepairOrders)
                .HasForeignKey(ro => ro.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Branch)
                .WithMany(b => b.RepairOrders)
                .HasForeignKey(ro => ro.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the PaidStatus property to use the enum
            modelBuilder.Entity<RepairOrder>()
                .Property(e => e.PaidStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Vehicle-User relationship
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment-User relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentId)
                .ValueGeneratedNever();


            // ServiceCategory self-referencing relationship
            modelBuilder.Entity<ServiceCategory>()
                .HasOne(sc => sc.ParentServiceCategory)
                .WithMany(sc => sc.ChildServiceCategories)
                .HasForeignKey(sc => sc.ParentServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);




            modelBuilder.Entity<FeedBack>()
    .HasOne(f => f.RepairOrder)
    .WithOne(ro => ro.FeedBack)
    .HasForeignKey<FeedBack>(f => f.RepairOrderId)
    .OnDelete(DeleteBehavior.Cascade);


            // Job relationships - prevent cascade delete conflicts
            modelBuilder.Entity<Job>()
                .HasOne(j => j.RepairOrder)
                .WithMany(ro => ro.Jobs)
                .HasForeignKey(j => j.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Service)
                .WithMany(s => s.Jobs)
                .HasForeignKey(j => j.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobPart relationships
            modelBuilder.Entity<JobPart>()
                .HasOne(jp => jp.Job)
                .WithMany(j => j.JobParts)
                .HasForeignKey(jp => jp.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobPart>()
                .HasOne(jp => jp.Part)
                .WithMany(p => p.JobParts)
                .HasForeignKey(jp => jp.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment-RepairOrder relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.RepairOrder)
                .WithMany(ro => ro.Payments)
                .HasForeignKey(p => p.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Service-ServiceCategory relationship
            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceCategory)
                .WithMany(sc => sc.Services)
                .HasForeignKey(s => s.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //// Service-Branch relationship
            //modelBuilder.Entity<Service>()
            //    .HasOne(s => s.Branch)
            //    .WithMany(b => b.Services)
            //    .HasForeignKey(s => s.BranchId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Part-PartCategory relationship
            modelBuilder.Entity<Part>()
                .HasOne(p => p.PartCategory)
                .WithMany(pc => pc.Parts)
                .HasForeignKey(p => p.PartCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Part indexes for better query performance
            modelBuilder.Entity<Part>()
                .HasIndex(p => p.PartCategoryId)
                .HasDatabaseName("IX_Part_PartCategoryId");

            modelBuilder.Entity<Part>()
                .HasIndex(p => p.BranchId)
                .HasDatabaseName("IX_Part_BranchId");

            modelBuilder.Entity<Part>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Part_Name");

            modelBuilder.Entity<Part>()
                .HasIndex(p => new { p.PartCategoryId, p.BranchId })
                .HasDatabaseName("IX_Part_CategoryId_BranchId");

            modelBuilder.Entity<Part>()
                .HasIndex(p => new { p.PartCategoryId, p.Name })
                .HasDatabaseName("IX_Part_CategoryId_Name");

            // ServicePartCategory indexes for better query performance
            modelBuilder.Entity<ServicePartCategory>()
                .HasIndex(spc => spc.ServiceId)
                .HasDatabaseName("IX_ServicePartCategory_ServiceId");

            modelBuilder.Entity<ServicePartCategory>()
                .HasIndex(spc => spc.PartCategoryId)
                .HasDatabaseName("IX_ServicePartCategory_PartCategoryId");

            modelBuilder.Entity<ServicePartCategory>()
                .HasIndex(spc => new { spc.ServiceId, spc.PartCategoryId })
                .HasDatabaseName("UX_ServicePartCategory_ServiceId_PartCategoryId");

            // VehicleModel-PartCategory relationship
            modelBuilder.Entity<PartCategory>()
                .HasOne(pc => pc.VehicleModel)
                .WithMany(vm => vm.PartCategories)
                .HasForeignKey(pc => pc.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // PartCategory unique constraint on (ModelId, CategoryName)
            modelBuilder.Entity<PartCategory>()
                .HasIndex(pc => new { pc.ModelId, pc.CategoryName })
                .IsUnique()
                .HasDatabaseName("UX_PartCategory_ModelId_CategoryName");

            // PartInventory configuration
            modelBuilder.Entity<PartInventory>(entity =>
            {
                entity.HasKey(e => e.PartInventoryId);
                entity.Property(e => e.Stock).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Part-PartInventory relationship
                entity.HasOne(pi => pi.Part)
                      .WithMany(p => p.PartInventories)
                      .HasForeignKey(pi => pi.PartId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Branch-PartInventory relationship
                entity.HasOne(pi => pi.Branch)
                      .WithMany(b => b.PartInventories)
                      .HasForeignKey(pi => pi.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Removed unique constraint to allow multiple inventory records per part-branch combination
                entity.HasIndex(pi => new { pi.PartId, pi.BranchId })
                      .HasDatabaseName("IX_PartInventory_PartId_BranchId");
            });

            // Inspection-RepairOrder relationship
            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.RepairOrder)
                .WithMany(ro => ro.Inspections)
                .HasForeignKey(i => i.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Label-OrderStatus relationship
            modelBuilder.Entity<Label>()
                .HasOne(l => l.OrderStatus)
                .WithMany(os => os.Labels)
                .HasForeignKey(l => l.OrderStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many RepairOrder -> Label relationship (each RO has one label)
            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Label)
                .WithMany(l => l.RepairOrders)
                .HasForeignKey(ro => ro.LabelId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Configure OrderStatus to use identity
            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasKey(e => e.OrderStatusId);
                entity.Property(e => e.OrderStatusId)
                      .ValueGeneratedOnAdd(); // Identity column
                entity.Property(e => e.StatusName)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // RepairOrderService configuration (Junction table)
            modelBuilder.Entity<RepairOrderService>(entity =>
            {
                entity.HasKey(e => e.RepairOrderServiceId);
                entity.Property(e => e.ActualDuration).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(ros => ros.RepairOrder)
                      .WithMany(ro => ro.RepairOrderServices)
                      .HasForeignKey(ros => ros.RepairOrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ros => ros.Service)
                      .WithMany(s => s.RepairOrderServices)
                      .HasForeignKey(ros => ros.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(ros => ros.RepairOrderServiceParts)
                      .WithOne(rosp => rosp.RepairOrderService)
                      .HasForeignKey(rosp => rosp.RepairOrderServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // RepairOrderServicePart configuration (Junction table)
            modelBuilder.Entity<RepairOrderServicePart>(entity =>
            {
                entity.HasKey(e => e.RepairOrderServicePartId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(rosp => rosp.RepairOrderService)
                      .WithMany(ros => ros.RepairOrderServiceParts)
                      .HasForeignKey(rosp => rosp.RepairOrderServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rosp => rosp.Part)
                      .WithMany(p => p.RepairOrderServiceParts)
                      .HasForeignKey(rosp => rosp.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceInspection configuration (Junction table)
            modelBuilder.Entity<ServiceInspection>(entity =>
            {
                entity.HasKey(e => e.ServiceInspectionId);
                entity.Property(s => s.ConditionStatus)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(si => si.Service)
                      .WithMany(s => s.ServiceInspections)
                      .HasForeignKey(si => si.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(si => si.Inspection)
                      .WithMany(i => i.ServiceInspections)
                      .HasForeignKey(si => si.InspectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PartInspection configuration (Junction table)
            modelBuilder.Entity<PartInspection>(entity =>
            {
                entity.HasKey(e => e.PartInspectionId);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(pi => pi.Part)
                      .WithMany(p => p.PartInspections)
                      .HasForeignKey(pi => pi.PartId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pi => pi.Inspection)
                      .WithMany(i => i.PartInspections)
                      .HasForeignKey(pi => pi.InspectionId)
                      .OnDelete(DeleteBehavior.Cascade);


            });

            

            // Many-to-many Branch <-> Service
            modelBuilder.Entity<BranchService>().HasKey(bs => new { bs.BranchId, bs.ServiceId });
            // Many-to-many Branch <-> Service
            modelBuilder.Entity<BranchService>()
             .HasOne(bs => bs.Branch)
             .WithMany(b => b.BranchServices)
             .HasForeignKey(bs => bs.BranchId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BranchService>()
                .HasOne(bs => bs.Service)
                .WithMany(s => s.BranchServices)
                .HasForeignKey(bs => bs.ServiceId)

                .OnDelete(DeleteBehavior.Cascade);




            // ServicePart configuration
            //modelBuilder.Entity<ServicePart>(entity =>
            //{

            //    entity.HasKey(e => new { e.ServiceId, e.PartId });
            //    entity.Property(e => e.CreatedAt).IsRequired();

            //    entity.HasOne(sp => sp.Service)
            //          .WithMany(s => s.ServiceParts)
            //          .HasForeignKey(sp => sp.ServiceId)
            //          .OnDelete(DeleteBehavior.Cascade);

            //    entity.HasOne(sp => sp.Part)
            //          .WithMany(p => p.ServiceParts)
            //          .HasForeignKey(sp => sp.PartId)
            //          .OnDelete(DeleteBehavior.Restrict);
            //});
            // branch 1 -> application User

            modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Branch)
            .WithMany(b => b.Staffs)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull); // Hoặc Cascade nếu muốn xóa user khi branch bị xóa


            // PromotionalCampaignService n-n Branch

            modelBuilder.Entity<PromotionalCampaignService>()
            .HasKey(pcs => new { pcs.PromotionalCampaignId, pcs.ServiceId });

            modelBuilder.Entity<PromotionalCampaignService>()
                .HasOne(pcs => pcs.PromotionalCampaign)
                .WithMany(pc => pc.PromotionalCampaignServices)
                .HasForeignKey(pcs => pcs.PromotionalCampaignId);

            modelBuilder.Entity<PromotionalCampaignService>()
                .HasOne(pcs => pcs.Service)
                .WithMany(s => s.PromotionalCampaignServices)
                .HasForeignKey(pcs => pcs.ServiceId);


            // 🔹 RepairRequest - RequestService (1-n)
            modelBuilder.Entity<RepairRequest>()
                .HasMany(r => r.RequestServices)
                .WithOne(rs => rs.RepairRequest)
                .HasForeignKey(rs => rs.RepairRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // INDEX phục vụ duyệt/quota theo range [winStart, winEnd)
            modelBuilder.Entity<RepairRequest>()
                .HasIndex(r => new { r.BranchId, r.ArrivalWindowStart, r.Status })
                .HasDatabaseName("IX_Request_Branch_Arrival_Status");

            // (tuỳ chọn) Index phụ cho WIP (Arrived/InProgress)
            modelBuilder.Entity<RepairRequest>()
                .HasIndex(r => new { r.BranchId, r.Status })
                .HasDatabaseName("IX_Request_Branch_Status");


            // 🔹 RequestService - RequestPart (1-n)
            modelBuilder.Entity<RequestService>()
                .HasMany(rs => rs.RequestParts)
                .WithOne(rp => rp.RequestService)
                .HasForeignKey(rp => rp.RequestServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 (Nếu có) RepairRequest - RepairImage (1-n)
            modelBuilder.Entity<RepairRequest>()
                .HasMany(r => r.RepairImages)
                .WithOne(img => img.RepairRequest)
                .HasForeignKey(img => img.RepairRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherUsage>(entity =>
            {
                entity.ToTable("VoucherUsage");
                entity.Property(v => v.Id)
                .HasDefaultValueSql("NEWID()");
                entity.HasKey(v => v.Id);

                entity.Property(v => v.UsedAt)
                      .HasColumnType("datetime2");

                entity.HasOne(v => v.Campaign)
                      .WithMany(c => c.VoucherUsages)
                      .HasForeignKey(v => v.CampaignId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Customer)
                      .WithMany()
                      .HasForeignKey(v => v.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.RepairOrder)
                      .WithMany(ro => ro.VoucherUsages)
                      .HasForeignKey(v => v.RepairOrderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<RequestEmergency>()
                .HasOne(r => r.Technician)
                .WithMany()
                .HasForeignKey(r => r.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}