using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class newdbV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Users_ImportByUserID",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_DigitalSignatures_DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Users_CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalCertificates_Users_VerifyByUserId",
                table: "ExternalCertificates");

            migrationBuilder.DropTable(
                name: "CourseParticipants");

            migrationBuilder.DropTable(
                name: "CourseResults");

            migrationBuilder.DropTable(
                name: "DigitalSignatures");

            migrationBuilder.DropIndex(
                name: "IX_ExternalCertificates_VerifyByUserId",
                table: "ExternalCertificates");

            migrationBuilder.DropIndex(
                name: "IX_Decisions_DigitalSignatureId",
                table: "Decisions");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_ImportByUserID",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "DigitalSignatureId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "CreateByUserUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "DigitalSignatureId",
                table: "Certificates");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedCourseId",
                table: "Courses",
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
                name: "IX_Courses_RelatedCourseId",
                table: "Courses",
                column: "RelatedCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_CreatedByUserId",
                table: "CertificateTemplates",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Users_CreatedByUserId",
                table: "CertificateTemplates",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Courses_RelatedCourseId",
                table: "Courses",
                column: "RelatedCourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_Users_CreatedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Courses_RelatedCourseId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_RelatedCourseId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_CreatedByUserId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RelatedCourseId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "DigitalSignatureId",
                table: "Decisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreateByUserUserId",
                table: "CertificateTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DigitalSignatureId",
                table: "Certificates",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseParticipants",
                columns: table => new
                {
                    ParticipantId = table.Column<string>(type: "text", nullable: false),
                    CourseId = table.Column<string>(type: "text", nullable: false),
                    GradeId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseParticipants", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_CourseParticipants_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseParticipants_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "GradeId");
                    table.ForeignKey(
                        name: "FK_CourseParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseResults",
                columns: table => new
                {
                    ResultID = table.Column<string>(type: "text", nullable: false),
                    ApprovedByUserUserId = table.Column<string>(type: "text", nullable: true),
                    CourseID = table.Column<string>(type: "text", nullable: false),
                    SubmittedByUserUserId = table.Column<string>(type: "text", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    AverageScore = table.Column<double>(type: "double precision", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FailCount = table.Column<int>(type: "integer", nullable: false),
                    PassCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SubmittedBy = table.Column<string>(type: "text", nullable: false),
                    TotalTrainees = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseResults", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_CourseResults_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseResults_Users_ApprovedByUserUserId",
                        column: x => x.ApprovedByUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CourseResults_Users_SubmittedByUserUserId",
                        column: x => x.SubmittedByUserUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DigitalSignatures",
                columns: table => new
                {
                    SignatureID = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CertificateChain = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalSignatures", x => x.SignatureID);
                    table.ForeignKey(
                        name: "FK_DigitalSignatures_Users_UserId",
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
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 324, DateTimeKind.Utc).AddTicks(7979), new DateTime(2025, 4, 17, 3, 4, 45, 324, DateTimeKind.Local).AddTicks(7977) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(993), "$2a$11$dFDSSItCyxL9dZohK7lHzuSIWaXQ6oVqCDZlEwACaulyzQ0KPzgSi", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(993) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1017), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1017) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1000), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1000) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1006), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1006) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1009), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1009) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1011), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1012) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1014), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1014) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1003), "$2a$11$npAeEcyD.zzUy5TRaWBFUudPyaWBaOSBhLwI3uz/0n1YYCCCjzSNe", new DateTime(2025, 4, 16, 20, 4, 45, 886, DateTimeKind.Utc).AddTicks(1003) });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_VerifyByUserId",
                table: "ExternalCertificates",
                column: "VerifyByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_DigitalSignatureId",
                table: "Decisions",
                column: "DigitalSignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_CreateByUserUserId",
                table: "CertificateTemplates",
                column: "CreateByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_DigitalSignatureId",
                table: "Certificates",
                column: "DigitalSignatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_ImportByUserID",
                table: "Candidates",
                column: "ImportByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipants_CourseId",
                table: "CourseParticipants",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipants_GradeId",
                table: "CourseParticipants",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipants_UserId",
                table: "CourseParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_ApprovedByUserUserId",
                table: "CourseResults",
                column: "ApprovedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseID",
                table: "CourseResults",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_SubmittedByUserUserId",
                table: "CourseResults",
                column: "SubmittedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalSignatures_UserId",
                table: "DigitalSignatures",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Users_ImportByUserID",
                table: "Candidates",
                column: "ImportByUserID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_DigitalSignatures_DigitalSignatureId",
                table: "Certificates",
                column: "DigitalSignatureId",
                principalTable: "DigitalSignatures",
                principalColumn: "SignatureID");

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_Users_CreateByUserUserId",
                table: "CertificateTemplates",
                column: "CreateByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_DigitalSignatures_DigitalSignatureId",
                table: "Decisions",
                column: "DigitalSignatureId",
                principalTable: "DigitalSignatures",
                principalColumn: "SignatureID");

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalCertificates_Users_VerifyByUserId",
                table: "ExternalCertificates",
                column: "VerifyByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
