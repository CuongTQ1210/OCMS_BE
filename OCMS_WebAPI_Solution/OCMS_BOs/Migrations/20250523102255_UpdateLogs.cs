using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSubjects_Subjects_SubjectId",
                table: "ClassSubjects");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "ClassSubjects",
                newName: "SubjectSpecialtyId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSubjects_SubjectId",
                table: "ClassSubjects",
                newName: "IX_ClassSubjects_SubjectSpecialtyId");

            migrationBuilder.AlterColumn<int>(
                name: "Room",
                table: "TrainingSchedules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Location",
                table: "TrainingSchedules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "AuditLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LoginLog",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SessionExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLog", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_LoginLog_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 51, 781, DateTimeKind.Utc).AddTicks(4575), new DateTime(2025, 5, 23, 17, 22, 51, 781, DateTimeKind.Local).AddTicks(4572) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2036), "$2a$11$tdmMZxn9DjG6L4rDksUz5eWY7TZDT8CNwS6uXK0fz9rQ7XkE9xrem", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2037) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2117), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2118) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2046), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2047) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2060), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2061) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2067), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2068) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2075), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2075) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2109), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2111) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2053), "$2a$11$IHV5eWxC4xE2ESo3EAuS/exwN6.PHe8duc6N2KRCIIndh5y/A56we", new DateTime(2025, 5, 23, 10, 22, 52, 463, DateTimeKind.Utc).AddTicks(2054) });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SessionId",
                table: "AuditLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginLog_UserId",
                table: "LoginLog",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_LoginLog_SessionId",
                table: "AuditLogs",
                column: "SessionId",
                principalTable: "LoginLog",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSubjects_SubjectSpecialty_SubjectSpecialtyId",
                table: "ClassSubjects",
                column: "SubjectSpecialtyId",
                principalTable: "SubjectSpecialty",
                principalColumn: "SubjectSpecialtyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_LoginLog_SessionId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSubjects_SubjectSpecialty_SubjectSpecialtyId",
                table: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "LoginLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_SessionId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "SubjectSpecialtyId",
                table: "ClassSubjects",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSubjects_SubjectSpecialtyId",
                table: "ClassSubjects",
                newName: "IX_ClassSubjects_SubjectId");

            migrationBuilder.AlterColumn<string>(
                name: "Room",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "TrainingSchedules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 43, 967, DateTimeKind.Utc).AddTicks(5228), new DateTime(2025, 5, 17, 17, 9, 43, 967, DateTimeKind.Local).AddTicks(5228) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3790), "$2a$11$uLNJyFekCqopEk.Rqr.JrOeseq39HI7dBix.XWsAP5VALdBhVtkKO", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3791) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3813), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3813) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3794), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3795) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3802), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3802) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3805), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3805) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3808), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3808) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3810), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3811) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3797), "$2a$11$1LRxkdMsmJEdAkXMTCuLKO5cgNCNT4jVGq0w9oE6DHs.8HFPzeUrq", new DateTime(2025, 5, 17, 10, 9, 44, 215, DateTimeKind.Utc).AddTicks(3798) });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSubjects_Subjects_SubjectId",
                table: "ClassSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
