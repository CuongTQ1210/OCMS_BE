using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class adjustassignTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestStatus",
                table: "TraineeAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 44, 818, DateTimeKind.Utc).AddTicks(380), new DateTime(2025, 3, 29, 19, 45, 44, 818, DateTimeKind.Local).AddTicks(378) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3129), "$2a$11$NWKrYk5kJQdL8Fm6EcIY8.bxHjHXvw1nPQ6nGbK/z2LZKVQdQ8Fhu", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3131) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3155), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3156) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3136), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3137) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3144), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3144) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3147), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3147) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3150), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3150) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3152), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3153) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3140), "$2a$11$VtOmofIfis/bq/GdqLq0J.43q0UlAtskF4CMuEf9d1w.FZFm0pzWe", new DateTime(2025, 3, 29, 12, 45, 45, 192, DateTimeKind.Utc).AddTicks(3140) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "TraineeAssignments");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 260, DateTimeKind.Utc).AddTicks(6131), new DateTime(2025, 3, 22, 22, 5, 57, 260, DateTimeKind.Local).AddTicks(6129) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3022), "$2a$11$ezUZulbPMeaHZWV3/I2v4OaG7KI7Be27gVY4Zrb8EpcwTHMXJ4Rxm", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3023) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3169), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3169) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3030), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3031) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3039), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3039) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3144), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3145) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3148), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3148) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3165), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3165) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3035), "$2a$11$8KCXkxpg4d58M4FX7hVkVONUpSmq1hmT999j02HVE4TE/7Qapjq5i", new DateTime(2025, 3, 22, 15, 5, 57, 531, DateTimeKind.Utc).AddTicks(3035) });
        }
    }
}
