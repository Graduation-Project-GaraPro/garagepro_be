﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Notifications;
using BusinessObject.Technician;
using BusinessObject.Roles;

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
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PartCategory> PartCategories { get; set; }
        public DbSet<PartSpecification> PartSpecifications { get; set; }
        public DbSet<Job> Jobs { get; set; }
        
        // Junction tables
        public DbSet<RepairOrderService> RepairOrderServices { get; set; }
        public DbSet<RepairOrderServicePart> RepairOrderServiceParts { get; set; }
        public DbSet<ServiceInspection> ServiceInspections { get; set; }
        public DbSet<PartInspection> PartInspections { get; set; }
        public DbSet<JobPart> JobParts { get; set; }
        
        //Logs Admin 
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<SecurityLogRelation> SecurityLogRelations { get; set; }
        public DbSet<LogCategory> LogCategories { get; set; }
        public DbSet<LogTag> LogTags { get; set; }
        public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<SecurityPolicyHistory> SecurityPolicyHistories { get; set; }



        // Notification
        public DbSet<CategoryNotification> CategoryNotifications { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        //Technician
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<JobTechnician> JobTechnicians { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<VehicleLookup> VehicleLookups { get; set; }
        public DbSet<Specifications> Specifications { get; set; } 
        public DbSet<SpecificationsData> SpecificationsData { get; set; }

        public DbSet<PermissionCategory> PermissionCategories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔑 Composite key cho RolePermission
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Quan hệ RolePermission -> Role
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId);

            // Quan hệ RolePermission -> Permission
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

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

            // Fixed GUIDs (hardcoded)
            var userManagementCategoryId = new Guid("11111111-1111-1111-1111-111111111111");
            var bookingManagementCategoryId = new Guid("22222222-2222-2222-2222-222222222222");

            var viewUsersPermissionId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var editUsersPermissionId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var deleteUsersPermissionId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var viewBookingsPermissionId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var manageBookingsPermissionId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

            // Seed Permission Categories
            modelBuilder.Entity<PermissionCategory>().HasData(
                new PermissionCategory
                {
                    Id = userManagementCategoryId,
                    Name = "User Management",
                    Description = "Manage application users"
                },
                new PermissionCategory
                {
                    Id = bookingManagementCategoryId,
                    Name = "Booking Management",
                    Description = "Manage bookings and reservations"
                }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    Id = viewUsersPermissionId,
                    Code = "USER_VIEW",
                    Name = "View Users",
                    Description = "Can view users",
                    CategoryId = userManagementCategoryId,
                    CreatedAt = new DateTime(2025, 01, 01),
                    UpdatedAt = new DateTime(2025, 01, 01),
                    Deprecated = false
                },
                new Permission
                {
                    Id = editUsersPermissionId,
                    Code = "USER_EDIT",
                    Name = "Edit Users",
                    Description = "Can edit users",
                    CategoryId = userManagementCategoryId,
                    CreatedAt = new DateTime(2025, 01, 01),
                    UpdatedAt = new DateTime(2025, 01, 01),
                    Deprecated = false
                },
                new Permission
                {
                    Id = deleteUsersPermissionId,
                    Code = "USER_DELETE",
                    Name = "Delete Users",
                    Description = "Can delete users",
                    CategoryId = userManagementCategoryId,
                    CreatedAt = new DateTime(2025, 01, 01),
                    UpdatedAt = new DateTime(2025, 01, 01),
                    Deprecated = false
                },
                new Permission
                {
                    Id = viewBookingsPermissionId,
                    Code = "BOOKING_VIEW",
                    Name = "View Bookings",
                    Description = "Can view bookings",
                    CategoryId = bookingManagementCategoryId,
                    CreatedAt = new DateTime(2025, 01, 01),
                    UpdatedAt = new DateTime(2025, 01, 01),
                    Deprecated = false
                },
                new Permission
                {
                    Id = manageBookingsPermissionId,
                    Code = "BOOKING_MANAGE",
                    Name = "Manage Bookings",
                    Description = "Can manage bookings",
                    CategoryId = bookingManagementCategoryId,
                    CreatedAt = new DateTime(2025, 01, 01),
                    UpdatedAt = new DateTime(2025, 01, 01),
                    Deprecated = false
                }
            );



            // ApplicationUser configuration
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FirstName).HasMaxLength(50);
                b.Property(u => u.LastName).HasMaxLength(50);
                b.Property(u => u.FullName).HasMaxLength(100).IsRequired();
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

            // CategoryNotification configuration
            modelBuilder.Entity<CategoryNotification>(entity =>
            {
                entity.HasKey(e => e.CategoryID);
                entity.Property(e => e.CategoryName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasMany(e => e.Notifications)
                      .WithOne(n => n.CategoryNotification)
                      .HasForeignKey(n => n.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict); // Tránh xóa liên quan
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

                // Quan hệ với CategoryNotification
                entity.HasOne(e => e.CategoryNotification)
                      .WithMany(c => c.Notifications)
                      .HasForeignKey(e => e.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);

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
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasMany(j => j.JobTechnicians)
                      .WithOne(jt => jt.Job)
                      .HasForeignKey(jt => jt.JobId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(j => j.Repairs) // Quan hệ một-nhiều với Repair
                      .WithOne(r => r.Job)
                      .HasForeignKey(r => r.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Repair configuration
            modelBuilder.Entity<Repair>(entity =>
            {
                entity.HasKey(e => e.RepairId);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.HasOne(r => r.Job)
                      .WithMany(j => j.Repairs)
                      .HasForeignKey(r => r.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Inspection configuration
            modelBuilder.Entity<Inspection>(entity =>
            {
                entity.HasKey(e => e.InspectionId);
                entity.Property(e => e.CustomerConcern).HasMaxLength(500);
                entity.Property(e => e.Finding).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(i => i.Technician)
                      .WithMany(t => t.Inspections)
                      .HasForeignKey(i => i.TechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);
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

                entity.HasMany(e => e.Specifications)
                      .WithOne(s => s.VehicleLookup)
                      .HasForeignKey(s => s.LookupID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Specifications configuration
            modelBuilder.Entity<Specifications>(entity =>
            {
                entity.HasKey(e => e.SpecificationsID);
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasOne(s => s.VehicleLookup)
                      .WithMany(v => v.Specifications)
                      .HasForeignKey(s => s.LookupID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.SpecificationsData)
                      .WithOne(d => d.Specifications)
                      .HasForeignKey(d => d.SpecificationsID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SpecificationsData configuration
            modelBuilder.Entity<SpecificationsData>(entity =>
            {
                entity.HasKey(e => e.DataID);
                entity.Property(e => e.Label)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Value)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasOne(d => d.Specifications)
                      .WithMany(s => s.SpecificationsData)
                      .HasForeignKey(d => d.SpecificationsID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //Log System
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Level)
                     .HasConversion<string>()   // Lưu dưới dạng nvarchar
                      .HasMaxLength(20)          // Giữ max length 20 như trước
                      .IsRequired();

                entity.Property(e => e.Source)
                    .HasMaxLength(255);

                entity.Property(e => e.UserName)
                    .HasMaxLength(255);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45);

                entity.Property(e => e.SessionId)
                    .HasMaxLength(100);

                entity.Property(e => e.RequestId)
                    .HasMaxLength(100);

                // Quan hệ với ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany() // hoặc tạo ICollection<SystemLog> trong ApplicationUser nếu muốn navigation
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ với LogCategory
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.SystemLogs)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với LogTag
                entity.HasMany(e => e.Tags)
                    .WithOne(t => t.SystemLog)
                    .HasForeignKey(t => t.LogId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với SecurityLogRelation (RelatedLog)
                entity.HasMany(e => e.SecurityLogRelations)
                    .WithOne(r => r.RelatedLog)
                    .HasForeignKey(r => r.RelatedLogId)
                    .OnDelete(DeleteBehavior.Restrict); // tránh Multiple Cascade Paths
            });

            // =========================
            // LogCategory
            // =========================
            modelBuilder.Entity<LogCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired(false);
            });

            // =========================
            // LogTag
            // =========================
            modelBuilder.Entity<LogTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Tag)
                    .HasConversion<string>()
                    .HasMaxLength(100);
            });

            // =========================
            // SecurityLog
            // =========================
            modelBuilder.Entity<SecurityLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ThreatLevel)
                    .HasMaxLength(20);

                entity.Property(e => e.Action)
                    .HasMaxLength(50);

                entity.Property(e => e.Resource)
                    .HasMaxLength(255);

                entity.Property(e => e.Outcome)
                    .HasMaxLength(20);

                // Quan hệ 1-1 với SystemLog
                entity.HasOne(e => e.SystemLog)
                    .WithOne()
                    .HasForeignKey<SecurityLog>(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với SecurityLogRelation
                entity.HasMany(e => e.Relations)
                    .WithOne(r => r.SecurityLog)
                    .HasForeignKey(r => r.SecurityLogId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // SecurityLogRelation
            // =========================

            modelBuilder.Entity<SecurityLogRelation>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Quan hệ với SecurityLog
                entity.HasOne(e => e.SecurityLog)
                    .WithMany(s => s.Relations)
                    .HasForeignKey(e => e.SecurityLogId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với RelatedLog (SystemLog)
                entity.HasOne(e => e.RelatedLog)
                    .WithMany(l => l.SecurityLogRelations)
                    .HasForeignKey(e => e.RelatedLogId)
                    .OnDelete(DeleteBehavior.Restrict); // tránh Multiple Cascade Paths
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
                entity.Property(e => e.MfaRequired).IsRequired();
                entity.Property(e => e.PasswordExpiryDays).IsRequired();
                entity.Property(e => e.EnableBruteForceProtection).IsRequired();

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("SYSUTCDATETIME()")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("SYSUTCDATETIME()")
                      .ValueGeneratedOnAddOrUpdate();

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

            // Part-Branch relationship
            modelBuilder.Entity<Part>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.Parts)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceCategory self-referencing relationship
            modelBuilder.Entity<ServiceCategory>()
                .HasOne(sc => sc.ParentServiceCategory)
                .WithMany(sc => sc.ChildServiceCategories)
                .HasForeignKey(sc => sc.ParentServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
