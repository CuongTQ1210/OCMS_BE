using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCMS_Services.Middleware
{
    public class ApiAuditLogMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiAuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, OCMSDbContext dbContext)
        {
            // Chỉ áp dụng middleware cho các yêu cầu API
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // Lưu trữ body của request gốc
            var originalBodyStream = context.Response.Body;

            // Cho phép đọc lại body của request
            context.Request.EnableBuffering();

            // Đọc nội dung request
            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Đặt lại vị trí để đọc lại sau
                }
            }

            // Chuẩn bị để ghi lại response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Ghi lại thời gian bắt đầu
            var startTime = DateTime.UtcNow;

            try
            {
                // Chuyển request đến middleware tiếp theo
                await _next(context);
            }
            finally
            {
                // Đọc nội dung response
                responseBody.Position = 0;
                string responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Position = 0;

                // Gửi response về client
                await responseBody.CopyToAsync(originalBodyStream);

                // Lấy thông tin người dùng từ claims
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //Lấy thông tin session 
                var sessionId = context.User.FindFirst("jti")?.Value;

                // Chỉ ghi log nếu người dùng đã đăng nhập
                if (!string.IsNullOrEmpty(userId))
                {
                    // Lấy Content-Type của request và response
                    string requestContentType = context.Request.ContentType;
                    string responseContentType = context.Response.ContentType;

                    // Làm sạch dữ liệu nhạy cảm
                    var sanitizedRequestBody = SanitizeContent(requestBody, requestContentType);
                    var sanitizedResponseContent = SanitizeContent(responseContent, responseContentType);

                    // Xác định tên hành động từ route
                    var controllerName = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                    var actionName = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";
                    var action = $"{context.Request.Method}_{controllerName}_{actionName}".ToLower();

                    // Tạo chi tiết hành động dưới dạng JSON
                    var actionDetails = JsonSerializer.Serialize(new
                    {
                        Path = context.Request.Path.Value,
                        Method = context.Request.Method,
                        Query = context.Request.QueryString.ToString(),
                        RequestBody = sanitizedRequestBody,
                        ResponseBody = sanitizedResponseContent,
                        StatusCode = context.Response.StatusCode,
                        Duration = (DateTime.Now - startTime).TotalMilliseconds,
                        IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.Request.Headers["User-Agent"].ToString()
                    });

                    // Tạo bản ghi AuditLog
                    var auditLog = new AuditLog
                    {
                        LogId = await GenerateSequentialLogId(dbContext),
                        UserId = userId,
                        SessionId = sessionId,
                        Action = action,
                        ActionDetails = actionDetails,
                        Timestamp = startTime
                    };

                    // Lưu vào cơ sở dữ liệu
                    await dbContext.AuditLogs.AddAsync(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private string SanitizeContent(string content, string contentType)
        {
            // Nếu nội dung hoặc Content-Type rỗng, hoặc không phải JSON, trả về nguyên bản
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(contentType) ||
                !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                return content;
            }

            try
            {
                // Phân tích JSON và làm sạch dữ liệu nhạy cảm
                var jsonDoc = JsonDocument.Parse(content);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                SanitizeJsonElement(jsonDoc.RootElement, writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                // Nếu JSON không hợp lệ, trả về nội dung gốc
                return content;
            }
        }

        private void SanitizeJsonElement(JsonElement element, Utf8JsonWriter writer)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        var propertyName = property.Name.ToLowerInvariant();
                        // Làm sạch các trường nhạy cảm
                        if (propertyName.Contains("password") ||
                            propertyName.Contains("token") ||
                            propertyName.Contains("secret") ||
                            propertyName.Contains("key") ||
                            propertyName.Contains("credential"))
                        {
                            writer.WriteString(property.Name, "[REDACTED]");
                        }
                        else
                        {
                            writer.WritePropertyName(property.Name);
                            SanitizeJsonElement(property.Value, writer);
                        }
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        SanitizeJsonElement(item, writer);
                    }
                    writer.WriteEndArray();
                    break;
                case JsonValueKind.String:
                    writer.WriteStringValue(element.GetString());
                    break;
                case JsonValueKind.Number:
                    writer.WriteNumberValue(element.GetDecimal());
                    break;
                case JsonValueKind.True:
                    writer.WriteBooleanValue(true);
                    break;
                case JsonValueKind.False:
                    writer.WriteBooleanValue(false);
                    break;
                case JsonValueKind.Null:
                    writer.WriteNullValue();
                    break;
            }
        }

        private async Task<int> GenerateSequentialLogId(OCMSDbContext dbContext)
        {
            // Create an execution strategy specific to this operation
            var strategy = dbContext.Database.CreateExecutionStrategy();

            // Use the strategy to execute our transaction logic
            return await strategy.ExecuteAsync(async () =>
            {
                // Start a transaction within the execution strategy
                using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // Get the maximum log ID from the database
                    var maxLogId = await dbContext.AuditLogs
                        .MaxAsync(l => (int?)l.LogId) ?? 0;

                    // Increment the value by 1
                    int newLogId = maxLogId + 1;

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return newLogId;
                }
                catch
                {
                    // If anything goes wrong, roll back the transaction
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }

    // Phương thức mở rộng để đăng ký middleware
    public static class ApiAuditLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiAuditLogMiddleware>();
        }
    }
}