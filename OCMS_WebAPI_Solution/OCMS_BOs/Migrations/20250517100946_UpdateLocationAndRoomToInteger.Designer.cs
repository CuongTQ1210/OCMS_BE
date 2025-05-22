using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OCMS_BOs;

namespace OCMS_BOs.Migrations
{
    [DbContext(typeof(OCMSDbContext))]
    [Migration("20250517100946_UpdateLocationAndRoomToInteger")]
    partial class UpdateLocationAndRoomToInteger
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OCMS_BOs.Entities.TrainingSchedule", b =>
            {
                b.Property<string>("ScheduleID")
                    .HasColumnType("text");

                b.Property<string>("ClassSubjectId")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<TimeOnly>("ClassTime")
                    .HasColumnType("time without time zone");

                b.Property<string>("CreatedByUserId")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<DateTime>("CreatedDate")
                    .HasColumnType("timestamp without time zone");

                b.Property<int[]>("DaysOfWeek")
                    .IsRequired()
                    .HasColumnType("integer[]");

                b.Property<DateTime>("EndDateTime")
                    .HasColumnType("timestamp without time zone");

                b.Property<int>("Location")
                    .HasColumnType("integer");

                b.Property<DateTime>("ModifiedDate")
                    .HasColumnType("timestamp without time zone");

                b.Property<string>("Notes")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<int>("Room")
                    .HasColumnType("integer");

                b.Property<DateTime>("StartDateTime")
                    .HasColumnType("timestamp without time zone");

                b.Property<int>("Status")
                    .HasColumnType("integer");

                b.Property<TimeSpan>("SubjectPeriod")
                    .HasColumnType("interval");

                b.HasKey("ScheduleID");

                b.HasIndex("ClassSubjectId");

                b.HasIndex("CreatedByUserId");

                b.ToTable("TrainingSchedules");
            });
        }
    }
} 