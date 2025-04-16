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
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseResult> CourseResults { get; set; }
        public DbSet<ExternalCertificate> ExternalCertificates { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Decision> Decisions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }
        public DbSet<InstructorAssignment> InstructorAssignments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<TraineeAssign> TraineeAssignments { get; set; }
        public DbSet<Specialties> Specialties { get; set; }
        public DbSet<TrainingPlan> TrainingPlans { get; set; }
        public DbSet<TrainingSchedule> TrainingSchedules { get; set; }


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
                      .HasDefaultValue(AccountStatus.Active); // Default status is active

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
                entity.HasOne(u => u.Specialty)
                     .WithMany()
                     .HasForeignKey(u => u.SpecialtyId)
                     .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(u => u.Department)
                      .WithMany() // If Department has a `List<User>` navigation property, use `.WithMany(d => d.Users)`
                      .HasForeignKey(u => u.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Specialties>(entity =>
            {
                entity.HasKey(s => s.SpecialtyId);
                entity.Property(s => s.SpecialtyName)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<TrainingPlan>(entity =>
            {
                entity.HasOne(tp => tp.Specialty)
                      .WithMany()
                      .HasForeignKey(tp => tp.SpecialtyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasOne(d => d.Specialty)
                      .WithMany()
                      .HasForeignKey(d => d.SpecialtyId)
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
            modelBuilder.Entity<Department>(entity =>
{
    entity.HasKey(d => d.DepartmentId);

    entity.Property(d => d.DepartmentName)
          .IsRequired();

    entity.HasOne(d => d.Specialty)
          .WithMany()
          .HasForeignKey(d => d.SpecialtyId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(d => d.Manager)
          .WithMany() // If you want to add a collection of managed departments in User, change this to WithMany(u => u.ManagedDepartments)
          .HasForeignKey(d => d.ManagerUserId)
          .OnDelete(DeleteBehavior.Restrict);
});
            modelBuilder.Entity<User>()
    .Property(u => u.Status)
    .HasDefaultValue(AccountStatus.Active) // Ensure this matches your intended default
    .HasConversion<int>() // Store enum as an integer
    .Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            modelBuilder.Entity<Grade>()
                    .HasOne(g => g.TraineeAssign)
                    .WithMany()
                    .HasForeignKey(g => g.TraineeAssignID)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.GradedByInstructor)
                .WithMany()
                .HasForeignKey(g => g.GradedByInstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            // Seed data for roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "HeadMaster" },
                new Role { RoleId = 3, RoleName = "Training staff" },
                new Role { RoleId = 4, RoleName = "HR" },
                new Role { RoleId = 5, RoleName = "Instructor" },
                new Role { RoleId = 6, RoleName = "Reviewer" },
                new Role { RoleId = 7, RoleName = "Trainee" },
                new Role { RoleId = 8, RoleName = "AOC Manager" }
            );
            modelBuilder.Entity<Specialties>().HasData(
    new Specialties
    {
        SpecialtyId = "SPEC-001",
        SpecialtyName = "Admin Specialty",
        Description = "Admin Specialty Description",
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = "ADM-1"
    }
);

            // Seed Test Users for Each Role
            string adminPassword = PasswordHasher.HashPassword("Admin@123");
            string userPassword = PasswordHasher.HashPassword("User@123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = "ADM-1",
                    Username = "Admin",
                    FullName = "Admin User",
                    Gender = "Other",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Address = "123 Admin Street",
                    PhoneNumber = "1234567890",
                    Email = "admin@gmail.com",
                    PasswordHash = adminPassword,
                    RoleId = 1, // Admin
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "HM-1",
                    Username = "HeadMaster",
                    FullName = "Head Master User",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1980, 5, 20),
                    Address = "456 Headmaster Street",
                    PhoneNumber = "0987654321",
                    Email = "headmaster@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 2, // HeadMaster
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "TS-1",
                    Username = "TrainingStaff",
                    FullName = "Training Staff User",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1992, 7, 10),
                    Address = "789 Training Staff Lane",
                    PhoneNumber = "1122334455",
                    Email = "trainingstaff@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 3, // Training Staff
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "HR-1",
                    Username = "HRManager",
                    FullName = "HR Manager",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Address = "101 HR Street",
                    PhoneNumber = "2233445566",
                    Email = "hrmanager@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 4, // HR
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "INST-1",
                    Username = "Instructor",
                    FullName = "Instructor User",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1990, 9, 25),
                    Address = "202 Instructor Avenue",
                    PhoneNumber = "3344556677",
                    Email = "instructor@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 5, // Instructor
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "REV-1",
                    Username = "Reviewer",
                    FullName = "Reviewer User",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1993, 12, 5),
                    Address = "303 Reviewer Blvd",
                    PhoneNumber = "4455667788",
                    Email = "reviewer@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 6, // Reviewer
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "TR-1",
                    Username = "Trainee",
                    FullName = "Trainee User",
                    Gender = "Female",
                    DateOfBirth = new DateTime(2002, 8, 18),
                    Address = "404 Trainee Lane",
                    PhoneNumber = "5566778899",
                    Email = "trainee@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 7, // Trainee
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                },
                new User
                {
                    UserId = "AOC-1",
                    Username = "AOCManager",
                    FullName = "AOC Manager User",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1975, 11, 30),
                    Address = "505 AOC Street",
                    PhoneNumber = "6677889900",
                    Email = "aocmanager@gmail.com",
                    PasswordHash = userPassword,
                    RoleId = 8, // AOC Manager
                    SpecialtyId = "SPEC-001",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = AccountStatus.Active,
                    IsDeleted = false
                }
            );

        }
    }

    public class OCMSDbContextFactory : IDesignTimeDbContextFactory<OCMSDbContext>
    {
        public OCMSDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../OCMS_WebAPI"); // Adjust if needed

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath) // Explicitly set WebAPI path
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<OCMSDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new OCMSDbContext(optionsBuilder.Options);
        }
    }
}
