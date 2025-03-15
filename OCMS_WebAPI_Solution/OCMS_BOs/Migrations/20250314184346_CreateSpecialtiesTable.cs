using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class CreateSpecialtiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    SpecialtyId = table.Column<string>(type: "text", nullable: false),
                    SpecialtyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ParentSpecialtyId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.SpecialtyId);
                    table.ForeignKey(
                        name: "FK_Specialties_Specialties_ParentSpecialtyId",
                        column: x => x.ParentSpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "SpecialtyId");
                    table.ForeignKey(
                        name: "FK_Specialties_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Specialties_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.RenameColumn(
                name: "Specialty",
                table: "Departments",
                newName: "SpecialtyId");

            migrationBuilder.AddColumn<string>(
                name: "SpecialtyId",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialtyId",
                table: "TrainingPlans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialtyId",
                table: "Candidate",
                type: "text",
                nullable: false,
                defaultValue: "");            

            migrationBuilder.InsertData(
                table: "Specialties",
                columns: new[] { "SpecialtyId", "CreatedAt", "CreatedByUserId", "Description", "ParentSpecialtyId", "SpecialtyName", "Status", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { "SPEC-001", new DateTime(2025, 3, 14, 18, 43, 46, 246, DateTimeKind.Utc).AddTicks(9115), "ADM-1", "Admin Specialty Description", null, "Admin Specialty", 1, new DateTime(2025, 3, 15, 1, 43, 46, 246, DateTimeKind.Local).AddTicks(9115), null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "SpecialtyId", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4468), "$2a$11$0LHlJnSpv3ZqhXKDwgv1peGJnnmFEK686KbT2KQEDsb1xf6zFvOaS", "SPEC-001", new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4469) });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpecialtyId",
                table: "Users",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SpecialtyId",
                table: "Departments",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_SpecialtyId",
                table: "Candidate",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_CreatedByUserId",
                table: "Specialties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_ParentSpecialtyId",
                table: "Specialties",
                column: "ParentSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_UpdatedByUserId",
                table: "Specialties",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidate_Specialties_SpecialtyId",
                table: "Candidate",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Specialties_SpecialtyId",
                table: "Departments",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Specialties_SpecialtyId",
                table: "Users",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidate_Specialties_SpecialtyId",
                table: "Candidate");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Specialties_SpecialtyId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlans_Specialties_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Specialties_SpecialtyId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Users_SpecialtyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlans_SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Departments_SpecialtyId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Candidate_SpecialtyId",
                table: "Candidate");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Candidate");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "Departments",
                newName: "Specialty");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 11, 11, 49, 11, 639, DateTimeKind.Utc).AddTicks(3115), "$2a$11$FIyXTZCqUSFN66a3fQ19NuDBm3GxkSsWqE3L1Qpm3PQBq.EWW/NlK", new DateTime(2025, 3, 11, 11, 49, 11, 639, DateTimeKind.Utc).AddTicks(3116) });
        }
    }
}
