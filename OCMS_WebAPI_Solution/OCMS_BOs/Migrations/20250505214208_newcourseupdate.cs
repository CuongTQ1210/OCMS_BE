using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newcourseupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "TrainingPlanId",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 655, DateTimeKind.Utc).AddTicks(4279), new DateTime(2025, 5, 6, 4, 42, 7, 655, DateTimeKind.Local).AddTicks(4277) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8920), "$2a$11$HEVa8fNy6Km157Y2gjs5R.ntTdJJpmG1Kn52qH/DHRC5VLq6TOXW6", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8920) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8942), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8942) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8925), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8926) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8931), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8932) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8934), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8935) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8937), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8937) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8939), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8928), "$2a$11$RvWp9YTPNqnZAfs66WbABum7e9jbhqEi4zwdlhWSPOgnvyABBrK4a", new DateTime(2025, 5, 5, 21, 42, 7, 897, DateTimeKind.Utc).AddTicks(8929) });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "TrainingPlanId",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 179, DateTimeKind.Utc).AddTicks(3152), new DateTime(2025, 5, 6, 3, 51, 20, 179, DateTimeKind.Local).AddTicks(3150) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(243), "$2a$11$rDE0fJ1ZjXiwql9Clvb64OgvyilL7KL2MkN96MOIPwGaZzhXm.h0S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(244) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(268), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(268) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(251), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(251) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(257), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(257) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(259), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(262), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(265), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(265) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(254), "$2a$11$gyKYV.ZUPn0vPAcZYqQuru2di/PysSiNySC6nKiaNzlwdvN2gl29S", new DateTime(2025, 5, 5, 20, 51, 20, 425, DateTimeKind.Utc).AddTicks(254) });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
