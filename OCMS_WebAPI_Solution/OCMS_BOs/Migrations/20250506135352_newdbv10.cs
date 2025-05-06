using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbv10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TrainingPlanId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SpecialtiesSpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "TrainingPlanId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "Desciption",
                table: "TrainingPlans",
                newName: "SpecialtyId");

            migrationBuilder.AddColumn<string>(
                name: "TrainingPlanPlanId",
                table: "TrainingSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "TrainingPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TrainingPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 244, DateTimeKind.Utc).AddTicks(6996), new DateTime(2025, 5, 6, 20, 53, 51, 244, DateTimeKind.Local).AddTicks(6994) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(91), "$2a$11$E7dtbJRZ2Q7sA.lqUamyQeKc2Ru8p/YKzETpAFwcJLsVwFl4fr4aW", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(91) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(113), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(113) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(96), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(97) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(102), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(103) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(105), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(105) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(108), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(108) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(110), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(111) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(100), "$2a$11$3DtQlMoEhXY1bGb/Dymo/ewulyg0DOvE/a7ivCU85Rrea/SzBeKhS", new DateTime(2025, 5, 6, 13, 53, 51, 608, DateTimeKind.Utc).AddTicks(100) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_TrainingPlanPlanId",
                table: "TrainingSchedules",
                column: "TrainingPlanPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CourseId",
                table: "TrainingPlans",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Courses_CourseId",
                table: "TrainingPlans",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_TrainingPlans_TrainingPlanPlanId",
                table: "TrainingSchedules",
                column: "TrainingPlanPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Courses_CourseId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_TrainingPlans_TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSchedules_TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_CourseId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "TrainingPlanPlanId",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TrainingPlans");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "TrainingPlans",
                newName: "Desciption");

            migrationBuilder.AddColumn<string>(
                name: "SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainingPlanId",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 99, DateTimeKind.Utc).AddTicks(6157), new DateTime(2025, 5, 6, 19, 51, 33, 99, DateTimeKind.Local).AddTicks(6155) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9704), "$2a$11$MVsN0XhrYTBEAERFcgIFXuCMSw62i0sM4GSzXf5h/3jQXGH5ErJ3W", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9704) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9724), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9724) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9709), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9709) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9715), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9715) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9717), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9717) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9719), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9720) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9722), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9722) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9711), "$2a$11$VvwPivmzY96Y1QOunSVbkeCmGJj1f/xJhPAmtUc5iJk5iopgZ4pHu", new DateTime(2025, 5, 6, 12, 51, 33, 316, DateTimeKind.Utc).AddTicks(9712) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtiesSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TrainingPlanId",
                table: "Courses",
                column: "TrainingPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_TrainingPlans_TrainingPlanId",
                table: "Courses",
                column: "TrainingPlanId",
                principalTable: "TrainingPlans",
                principalColumn: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtiesSpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtiesSpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId");
        }
    }
}
