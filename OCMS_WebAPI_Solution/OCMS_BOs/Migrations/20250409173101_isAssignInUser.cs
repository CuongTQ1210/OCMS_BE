using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class isAssignInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAssign",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 53, DateTimeKind.Utc).AddTicks(520), new DateTime(2025, 4, 10, 0, 31, 1, 53, DateTimeKind.Local).AddTicks(517) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1907), false, "$2a$11$2UzEqQy98E..PqCvIIf1QuImaGNLTsPPFxLNkBQZp2U9sRax8UkC.", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1909) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1929), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1929) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1913), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1914) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1919), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1920) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1922), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1922) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1924), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1924) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1927), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1927) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "IsAssign", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1917), false, "$2a$11$Evyl/DUU6qjnDmbglyLIkuNzkqmfYcYTB2zAvGu4/WzeyRaESe782", new DateTime(2025, 4, 9, 17, 31, 1, 386, DateTimeKind.Utc).AddTicks(1917) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAssign",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 47, 902, DateTimeKind.Utc).AddTicks(6440), new DateTime(2025, 4, 9, 16, 27, 47, 902, DateTimeKind.Local).AddTicks(6438) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1664), "$2a$11$DgmbZsslMnVS8e8KbFX5A.7EkCFiWDKQpP6xgBQopV5QwPY7tfP2e", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1665) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1687), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1688) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1670), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1671) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1676), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1677) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1679), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1682), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1682) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1685), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1685) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1674), "$2a$11$kQhGvVjiYNdLYClynw/ac.dW4d1LJyyQucwXTyMAl6HpgLZu0OZKm", new DateTime(2025, 4, 9, 9, 27, 48, 144, DateTimeKind.Utc).AddTicks(1674) });
        }
    }
}
