using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.Policies;
using BusinessObject.SystemLogs;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BusinessObject.Authentication;
using BusinessObject.Branches;
using BusinessObject.Notifications;
using BusinessObject.Technician;
using BusinessObject.Roles;
using BusinessObject.Policies;
using BusinessObject.SystemLogs;
using BusinessObject.Enums;

namespace DataAccessLayer
{
    public class MyAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public MyAppDbContext(DbContextOptions<MyAppDbContext> options)
            : base(options) { }

        // Manager-related entities
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Color> Colors { get; set; }
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
        public DbSet<JobPart> JobParts { get; set; }
        public DbSet<JobTechnician> JobTechnicians { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<Specifications> Specifications { get; set; }
        public DbSet<SpecificationsData> SpecificationsData { get; set; }
        public DbSet<VehicleLookup> VehicleLookups { get; set; }
        public DbSet<SecurityPolicy> SecurityPolicies { get; set; }
        public DbSet<SecurityPolicyHistory> SecurityPolicyHistories { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationService> QuotationServices { get; set; }
        public DbSet<QuotationServicePart> QuotationServiceParts { get; set; }
        
        public DbSet<RepairOrderService> RepairOrderServices { get; set; }
        public DbSet<RepairOrderServicePart> RepairOrderServiceParts { get; set; }
        public DbSet<ServiceInspection> ServiceInspections { get; set; }
        public DbSet<PartInspection> PartInspections { get; set; }
        public DbSet<ServicePart> ServiceParts { get; set; }

        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<SecurityLogRelation> SecurityLogRelations { get; set; }
        public DbSet<LogCategory> LogCategories { get; set; }
        public DbSet<LogTag> LogTags { get; set; }

        // Notification
        public DbSet<CategoryNotification> CategoryNotifications { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        //Technician
        public DbSet<Technician> Technicians { get; set; }

        public DbSet<PermissionCategory> PermissionCategories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                // Customer approval workflow properties
                entity.Property(e => e.CustomerApprovalNote).HasMaxLength(1000);
                entity.Property(e => e.AssignedByManagerId).HasMaxLength(450); // Standard ASP.NET Identity user ID length
                entity.Property(e => e.RevisionReason).HasMaxLength(500); // Reason for estimate revision

                entity.HasMany(j => j.JobTechnicians)
                      .WithOne(jt => jt.Job)
                      .HasForeignKey(jt => jt.JobId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(j => j.Repairs) // Quan hệ một-nhiều với Repair
                      .WithOne(r => r.Job)
                      .HasForeignKey(r => r.JobId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(j => j.Quotation)
                      .WithOne(q => q.Job)
                      .HasForeignKey<Job>(j => j.QuotationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Quotation configuration
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.QuotationId);
                entity.Property(e => e.CustomerNote).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ChangeRequestDetails).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.HasOne(q => q.Inspection)
                      .WithOne() // One-to-one relationship with Inspection
                      .HasForeignKey<Quotation>(q => q.InspectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.User)
                      .WithMany()
                      .HasForeignKey(q => q.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(q => q.QuotationServices)
                      .WithOne(qs => qs.Quotation)
                      .HasForeignKey(qs => qs.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(q => q.Job)
                      .WithOne(j => j.Quotation)
                      .HasForeignKey<Job>(j => j.QuotationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // QuotationService configuration
            modelBuilder.Entity<QuotationService>(entity =>
            {
                entity.HasKey(e => e.QuotationServiceId);
                entity.Property(e => e.ServicePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CustomerRequestedParts).HasMaxLength(1000);

                entity.HasOne(qs => qs.Quotation)
                      .WithMany(q => q.QuotationServices)
                      .HasForeignKey(qs => qs.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qs => qs.Service)
                      .WithMany()
                      .HasForeignKey(qs => qs.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(qs => qs.QuotationServiceParts)
                      .WithOne(qsp => qsp.QuotationService)
                      .HasForeignKey(qsp => qsp.QuotationServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // QuotationServicePart configuration
            modelBuilder.Entity<QuotationServicePart>(entity =>
            {
                entity.HasKey(e => e.QuotationServicePartId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(qsp => qsp.QuotationService)
                      .WithMany(qs => qs.QuotationServiceParts)
                      .HasForeignKey(qsp => qsp.QuotationServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qsp => qsp.Part)
                      .WithMany()
                      .HasForeignKey(qsp => qsp.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
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
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired();
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

            // Color-Label relationship
            modelBuilder.Entity<Label>()
                .HasOne(l => l.Color)
                .WithMany(c => c.Labels)
                .HasForeignKey(l => l.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // RepairOrderService configuration (Junction table)
            modelBuilder.Entity<RepairOrderService>(entity =>
            {
                entity.HasKey(e => e.RepairOrderServiceId);
                entity.Property(e => e.ServicePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ActualDuration).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(ros => ros.RepairOrder)
                      .WithMany(ro => ro.RepairOrderServices)
                      .HasForeignKey(ros => ros.RepairOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

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
                entity.Property(e => e.Status).HasMaxLength(100);
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
                entity.Property(e => e.Status).HasMaxLength(100);
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

            // PartSpecification configuration
            modelBuilder.Entity<PartSpecification>(entity =>
            {
                entity.HasKey(e => e.SpecId);
                entity.Property(e => e.SpecValue).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(ps => ps.Part)
                      .WithMany(p => p.PartSpecifications)
                      .HasForeignKey(ps => ps.PartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // Many-to-many Branch <-> Service
            modelBuilder.Entity<BranchService>()
                .HasKey(bs => new { bs.BranchId, bs.ServiceId });

            modelBuilder.Entity<BranchService>()
                .HasOne(bs => bs.Branch)
                .WithMany(b => b.BranchServices)
                .HasForeignKey(bs => bs.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // <-- thêm vào

            modelBuilder.Entity<BranchService>()
                .HasOne(bs => bs.Service)
                .WithMany(s => s.BranchServices)
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict); // <-- thêm vào
           
            // ServicePart configuration
            modelBuilder.Entity<ServicePart>(entity =>
            {
                entity.HasKey(e => e.ServicePartId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(sp => sp.Service)
                      .WithMany(s => s.ServiceParts)
                      .HasForeignKey(sp => sp.ServiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sp => sp.Part)
                      .WithMany(p => p.ServiceParts)
                      .HasForeignKey(sp => sp.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // branch 1 -> application User

            modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Branch)
            .WithMany(b => b.Staffs)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.SetNull); // Hoặc Cascade nếu muốn xóa user khi branch bị xóa
        }
    }
}