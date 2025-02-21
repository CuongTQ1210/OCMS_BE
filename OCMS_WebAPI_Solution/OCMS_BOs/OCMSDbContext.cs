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
        public DbSet<TraineeNotification> Trainees { get; set; }
        public DbSet<TraineeProfile> TraineeProfiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CertificateTemplate> CertificatesTemplate { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseChangeRequest> CourseChangeRequests { get; set; }
        public DbSet<ExternalCertificate> ExternalCertificates { get; set; }
        public DbSet<BackupLog> BackupLogs { get; set; }
        public DbSet<ApprovalLog> ApprovalLogs { get; set; }
        public DbSet<TraineeNotification> TraineeNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API for Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleName)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // Fluent API for User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                      .IsRequired();

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasIndex(u => u.Email)
                      .IsUnique(); // Ensure unique emails

                entity.Property(u => u.Status)
                      .IsRequired()
                      .HasDefaultValue("active"); // Default status is active

                entity.Property(u => u.IsDeleted)
                      .IsRequired()
                      .HasDefaultValue(false); // Default is not deleted

                entity.Property(u => u.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Set default creation time

                entity.Property(u => u.UpdatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Set default update time

                entity.Property(u => u.LastLogin)
                      .IsRequired(false); // Nullable field

                entity.HasOne(u => u.Role) // Define the relationship
                        .WithMany() // Since Role does not have a Users collection
                        .HasForeignKey(u => u.RoleId) // Explicitly define the foreign key
                        .OnDelete(DeleteBehavior.Restrict);

            });


            // Fluent API for CourseParticipants
            modelBuilder.Entity<CourseParticipant>(entity =>
            {
                entity.HasKey(cp => cp.ParticipantId);
                entity.Property(cp => cp.Role)
                      .IsRequired();
                entity.Property(cp => cp.Status)
                      .IsRequired();
                entity.Property(cp => cp.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(cp => cp.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Grade>()
                    .HasOne(g => g.Trainee)
                    .WithMany()
                    .HasForeignKey(g => g.TraineeId)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Instructor)
                .WithMany()
                .HasForeignKey(g => g.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Seed Admin User
            string adminPassword = PasswordHasher.HashPassword("Admin@123");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = "ADM-1",
                    Username = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = adminPassword,
                    RoleId = 1, // Admin role
                    CreatedAt = DateTime.UtcNow,
                    Status = "active"
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
