using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OCMS_BOs.Entities;
using OCMS_BOs.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs
{
    public class OCMSDbContext : DbContext
    {
        public OCMSDbContext(DbContextOptions<OCMSDbContext> options)
             : base(options)
        {
        }
        
        // Define DbSet properties for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        // Configure entity relationships and seeding
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API for Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId); // Primary key
                entity.Property(r => r.RoleName)
                      .IsRequired()
                      .HasMaxLength(100); // Optional: Max length for role name
            });

            // Fluent API for User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId); // Primary key
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(u => u.PasswordHash)
                      .IsRequired();
                entity.Property(u => u.CreatedDate)
                      .IsRequired();

                // Configure foreign key relationship
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete
            });
           
            // Seed data for roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "HeadMaster" },
                new Role { RoleId = 3, RoleName = "Training staff" },
                new Role { RoleId = 4, RoleName = "HR" },
                new Role { RoleId = 5, RoleName = "Instructor" },
                new Role { RoleId = 6, RoleName = "Reviewer" },
                new Role { RoleId = 7, RoleName = "Trainee" }
            );

            string adminPassword = PasswordHasher.HashPassword("Admin@123"); // Replace with a secure password
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = "ADM-1", // Ensure it doesn't conflict with your identity framework if auto-generated
                    Username = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = adminPassword,
                    RoleId = 1, // Admin role
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
    public class OCMSDbContextFactory : IDesignTimeDbContextFactory<OCMSDbContext>
    {
        public OCMSDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<OCMSDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new OCMSDbContext(optionsBuilder.Options);
        }
    }

}
