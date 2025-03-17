using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class fixExternalCertificateRelasion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalCertificate",
                table: "Candidate");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 10, 44, 8, 162, DateTimeKind.Utc).AddTicks(9436), new DateTime(2025, 3, 16, 17, 44, 8, 162, DateTimeKind.Local).AddTicks(9435) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 16, 10, 44, 8, 287, DateTimeKind.Utc).AddTicks(8053), "$2a$11$0YLDR4NFLfrCK5.hhNE9b.7ko5qal.t3MNB2n4Dz8ujjYxaaBrY3C", new DateTime(2025, 3, 16, 10, 44, 8, 287, DateTimeKind.Utc).AddTicks(8054) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCertificate",
                table: "Candidate",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 14, 18, 43, 46, 246, DateTimeKind.Utc).AddTicks(9115), new DateTime(2025, 3, 15, 1, 43, 46, 246, DateTimeKind.Local).AddTicks(9115) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4468), "$2a$11$0LHlJnSpv3ZqhXKDwgv1peGJnnmFEK686KbT2KQEDsb1xf6zFvOaS", new DateTime(2025, 3, 14, 18, 43, 46, 371, DateTimeKind.Utc).AddTicks(4469) });
        }
    }
}
