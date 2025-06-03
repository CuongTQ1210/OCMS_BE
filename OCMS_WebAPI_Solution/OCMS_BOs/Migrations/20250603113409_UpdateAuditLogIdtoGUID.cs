using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCMS_BOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLogIdtoGUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sử dụng raw SQL để xử lý việc chuyển đổi từ int sang UUID
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    BEGIN
                        ALTER TABLE ""AuditLogs"" ALTER COLUMN ""LogId"" DROP IDENTITY;
                    EXCEPTION
                        WHEN others THEN
                            -- Nếu không có IDENTITY, không làm gì cả
                            NULL;   
                    END;
                END $$;                
                ALTER TABLE ""AuditLogs"" ALTER COLUMN ""LogId"" TYPE uuid USING gen_random_uuid();
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""AuditLogs"" ALTER COLUMN ""LogId"" TYPE uuid USING gen_random_uuid();
            ");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 166, DateTimeKind.Utc).AddTicks(8506), new DateTime(2025, 6, 3, 18, 34, 9, 166, DateTimeKind.Local).AddTicks(8505) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4244), "$2a$11$M0VbP4ylo9.ZbFsRuFFoh./ObUaiy7CNeAwc1KuHI2tzmRdQ5zvEa", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4245) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4286), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4286) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4250), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4250) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4257), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4257) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4259), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4260) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4262), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4265), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4266) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4253), "$2a$11$jNIBxBkSLuaPaRFQm5J.kOZaKfu/i1OEKrb2qhNBWrjKkX4JAWaTG", new DateTime(2025, 6, 3, 11, 34, 9, 420, DateTimeKind.Utc).AddTicks(4253) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback cũng cần raw SQL vì không thể chuyển UUID về int tự động
            migrationBuilder.Sql(@"
                -- Chuyển về int với IDENTITY, nhưng sẽ mất dữ liệu UUID hiện tại
                ALTER TABLE ""AuditLogs"" ALTER COLUMN ""LogId"" TYPE integer USING 1;
                ALTER TABLE ""AuditLogs"" ALTER COLUMN ""LogId"" ADD GENERATED BY DEFAULT AS IDENTITY;
            ");

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "SpecialtyId",
                keyValue: "SPEC-001",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 421, DateTimeKind.Utc).AddTicks(1810), new DateTime(2025, 5, 26, 8, 18, 50, 421, DateTimeKind.Local).AddTicks(1808) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "ADM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5549), "$2a$11$GFMBWumalt82dUJXDdlSculVFWHMoE1tGKHOSyDrH41wSOSNYjcDq", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5550) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "AOC-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5587), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5588) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HM-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5556), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5557) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "HR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5562), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5563) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "INST-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5565), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5566) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "REV-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5568), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5568) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TR-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5571), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5571) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "TS-1",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 5, 26, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5559), "$2a$11$4hXwaBMOHNZvkoRe9RvU1e1JubiHBEEvkjLc2C6Qk74uTkMO0NoPO", new DateTime(2025, 6, 1, 1, 18, 50, 793, DateTimeKind.Utc).AddTicks(5560) });
        }
    }
}