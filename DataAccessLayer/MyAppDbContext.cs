﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // ApplicationUser configuration
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FirstName).HasMaxLength(50);
                b.Property(u => u.LastName).HasMaxLength(50);

                b.HasMany(u => u.SystemLogs)
                 .WithOne(l => l.User)
                 .HasForeignKey(l => l.UserId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

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
