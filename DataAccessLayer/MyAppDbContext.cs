﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Authentication;
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
        public DbSet<Customer> Customers { get; set; }
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
            });

            // Configure relationships to prevent cascade delete cycles
            
            // RepairOrder relationships - prevent cascade delete conflicts
            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Customer)
                .WithMany(c => c.RepairOrders)
                .HasForeignKey(ro => ro.CustomerId)
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

            // Customer-Branch relationship
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Branch)
                .WithMany(b => b.Customers)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle-Customer relationship
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
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
