using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class moreuserdatafortesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 9, 44, 27, 816, DateTimeKind.Utc).AddTicks(5119), new DateTime(2025, 3, 16, 16, 44, 27, 816, DateTimeKind.Local).AddTicks(5117) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2214), "$2a$11$GLLAf2yFYGsgvPzNtyXON.OZPzVILroSZ/x9GeCo.mGFX0.QLTViy", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2214) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "CreatedAt", "DateOfBirth", "DepartmentId", "Email", "FullName", "Gender", "LastLogin", "PasswordHash", "PhoneNumber", "RoleId", "SpecialtyId", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { "AOC-1", "505 AOC Street", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2308), new DateTime(1975, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "aocmanager@gmail.com", "AOC Manager User", "Male", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "6677889900", 8, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2309), "AOCManager" },
                    { "HM-1", "456 Headmaster Street", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2221), new DateTime(1980, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "headmaster@gmail.com", "Head Master User", "Male", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "0987654321", 2, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2221), "HeadMaster" },
                    { "HR-1", "101 HR Street", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2285), new DateTime(1985, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "hrmanager@gmail.com", "HR Manager", "Male", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "2233445566", 4, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2286), "HRManager" },
                    { "INST-1", "202 Instructor Avenue", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2288), new DateTime(1990, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "instructor@gmail.com", "Instructor User", "Female", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "3344556677", 5, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2289), "Instructor" },
                    { "REV-1", "303 Reviewer Blvd", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2291), new DateTime(1993, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "reviewer@gmail.com", "Reviewer User", "Male", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "4455667788", 6, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2291), "Reviewer" },
                    { "TR-1", "404 Trainee Lane", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2294), new DateTime(2002, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "trainee@gmail.com", "Trainee User", "Female", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "5566778899", 7, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2294), "Trainee" },
                    { "TS-1", "789 Training Staff Lane", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2224), new DateTime(1992, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "trainingstaff@gmail.com", "Training Staff User", "Female", null, "$2a$11$9dSkgzY43hUdzm745eyYkeDxhQap0SCpFLOELl811rP/P6zOo3H/S", "1122334455", 3, "SPEC-001", new DateTime(2025, 3, 16, 9, 44, 28, 64, DateTimeKind.Utc).AddTicks(2224), "TrainingStaff" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 15, 10, 24, 32, 831, DateTimeKind.Utc).AddTicks(674), new DateTime(2025, 3, 15, 17, 24, 32, 831, DateTimeKind.Local).AddTicks(671) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 15, 10, 24, 33, 28, DateTimeKind.Utc).AddTicks(9476), "$2a$11$xVxE2k8CxoOCp1L7yWS7IeeoF/exGGFhuknwpXnwZGL6BXiE4CSH.", new DateTime(2025, 3, 15, 10, 24, 33, 28, DateTimeKind.Utc).AddTicks(9476) });
        }
    }
}
