using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newfixeddb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeNotifications_Users_UserId",
                table: "TraineeNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TraineeNotifications",
                table: "TraineeNotifications");

            migrationBuilder.RenameTable(
                name: "TraineeNotifications",
                newName: "TraineeNotification");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeNotifications_UserId",
                table: "TraineeNotification",
                newName: "IX_TraineeNotification_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "CourseChangeRequests",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TraineeNotification",
                table: "TraineeNotification",
                column: "NotificationId");

            migrationBuilder.CreateTable(
                name: "CertificatesTemplate",
                columns: table => new
                {
                    CertificateTemplateId = table.Column<string>(type: "text", nullable: false),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    TemplateFile = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificatesTemplate", x => x.CertificateTemplateId);
                    table.ForeignKey(
                        name: "FK_CertificatesTemplate_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    SubjectName = table.Column<string>(type: "text", nullable: false),
                    ScheduleFile = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectId);
                    table.ForeignKey(
                        name: "FK_Subjects_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraineeProfiles",
                columns: table => new
                {
                    TraineeProfileId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    ExternalCertificate = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeProfiles", x => x.TraineeProfileId);
                    table.ForeignKey(
                        name: "FK_TraineeProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    CertificateId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    CertificateTemplateId = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DigitalSignature = table.Column<string>(type: "text", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevocationReason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificatesTemplate_CertificateTemplateId",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificatesTemplate",
                        principalColumn: "CertificateTemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 21, 6, 16, 47, 121, DateTimeKind.Utc).AddTicks(5530), "$2a$11$meNBKaHAAFhcTUPl4H4nNOpdrXIMRCVOGlfz.dbPIDAi4zs9eiBnW", new DateTime(2025, 2, 21, 6, 16, 47, 121, DateTimeKind.Utc).AddTicks(5527) });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateTemplateId",
                table: "Certificates",
                column: "CertificateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UserId",
                table: "Certificates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificatesTemplate_CreatedBy",
                table: "CertificatesTemplate",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CourseId",
                table: "Subjects",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_UserId",
                table: "TraineeProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeNotification_Users_UserId",
                table: "TraineeNotification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeNotification_Users_UserId",
                table: "TraineeNotification");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "TraineeProfiles");

            migrationBuilder.DropTable(
                name: "CertificatesTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TraineeNotification",
                table: "TraineeNotification");

            migrationBuilder.RenameTable(
                name: "TraineeNotification",
                newName: "TraineeNotifications");

            migrationBuilder.RenameIndex(
                name: "IX_TraineeNotification_UserId",
                table: "TraineeNotifications",
                newName: "IX_TraineeNotifications_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                table: "CourseChangeRequests",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TraineeNotifications",
                table: "TraineeNotifications",
                column: "NotificationId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 20, 16, 0, 32, 959, DateTimeKind.Utc).AddTicks(2795), "$2a$11$jcT9x56EggffsZtByPdFSOsaKlY0AQqg1VleR0reBD/nS4Bj84/k2", new DateTime(2025, 2, 20, 16, 0, 32, 959, DateTimeKind.Utc).AddTicks(2791) });

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeNotifications_Users_UserId",
                table: "TraineeNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
