using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class updatedbV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_ApprovedUserUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_Users_CreatedByUserUserId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSchedules_CreatedByUserUserId",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "TrainingSchedules");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TrainingSchedules",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "ApprovedUserUserId",
                table: "Requests",
                newName: "ApproveByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_ApprovedUserUserId",
                table: "Requests",
                newName: "IX_Requests_ApproveByUserId");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 1, 637, DateTimeKind.Utc).AddTicks(1610), new DateTime(2025, 4, 27, 0, 0, 1, 637, DateTimeKind.Local).AddTicks(1607) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3536), "$2a$11$tBJPPC0f3tVEc200l1o9SuvlfuH.snKX46yI5unXsXV2LmyytS7o6", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3536) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3564), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3564) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3543), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3544) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3550), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3551) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3553), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3554) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3557), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3557) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3560), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3561) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3547), "$2a$11$4dAIhexXqTkA9/bemOPKWu898ATd/tukt97N9.JVmiSKyy.SV7JWC", new DateTime(2025, 4, 26, 17, 0, 2, 38, DateTimeKind.Utc).AddTicks(3547) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_CreatedByUserId",
                table: "TrainingSchedules",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_ApproveByUserId",
                table: "Requests",
                column: "ApproveByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_Users_CreatedByUserId",
                table: "TrainingSchedules",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_ApproveByUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSchedules_Users_CreatedByUserId",
                table: "TrainingSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainingSchedules_CreatedByUserId",
                table: "TrainingSchedules");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TrainingSchedules",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "ApproveByUserId",
                table: "Requests",
                newName: "ApprovedUserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Requests_ApproveByUserId",
                table: "Requests",
                newName: "IX_Requests_ApprovedUserUserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserUserId",
                table: "TrainingSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 0, 843, DateTimeKind.Utc).AddTicks(5457), new DateTime(2025, 4, 25, 17, 18, 0, 843, DateTimeKind.Local).AddTicks(5457) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4062), "$2a$11$VkTeR3b6JXz1WcUGj13D2eDt1ltzMepx7kRQfDFlQmraQ6F4Ocfru", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4062) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4086), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4087) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4067), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4067) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4075), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4075) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4078), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4078) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4080), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4081) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4083), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4084) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4070), "$2a$11$B5v5YMCBfbrJq5uHz7adS.i0HXowH/6ZhqKqamLh0zA7XZ9tof1hW", new DateTime(2025, 4, 25, 10, 18, 1, 85, DateTimeKind.Utc).AddTicks(4070) });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSchedules_CreatedByUserUserId",
                table: "TrainingSchedules",
                column: "CreatedByUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_ApprovedUserUserId",
                table: "Requests",
                column: "ApprovedUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSchedules_Users_CreatedByUserUserId",
                table: "TrainingSchedules",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
