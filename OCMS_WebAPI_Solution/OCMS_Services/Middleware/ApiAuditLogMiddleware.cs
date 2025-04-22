using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OCMS_BOs;
using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
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
            // Chỉ áp dụng cho các API requests
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // Lưu lại request body
            var originalBodyStream = context.Response.Body;

            // Cho phép đọc request body nhiều lần
            context.Request.EnableBuffering();

            // Đọc request body
            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset vị trí để đọc lại sau này
                }
            }

            // Chuẩn bị để capture response
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
                // Sau khi request được xử lý, đọc response
                responseBody.Position = 0;
                string responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Position = 0;

                // Copy response lại cho client
                await responseBody.CopyToAsync(originalBodyStream);

                // Lấy thông tin người dùng
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Chỉ ghi log nếu người dùng đã đăng nhập
                if (!string.IsNullOrEmpty(userId))
                {
                    // Loại bỏ thông tin nhạy cảm từ request và response
                    var sanitizedRequestBody = SanitizeContent(requestBody);
                    var sanitizedResponseContent = SanitizeContent(responseContent);

                    // Xác định tên action từ route data
                    var controllerName = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                    var actionName = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";

                    // Tạo action name theo định dạng thống nhất
                    var action = $"{context.Request.Method}_{controllerName}_{actionName}".ToLower();

                    // Tạo chi tiết hành động
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

                    // Tạo mới bản ghi AuditLog
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = action,
                        ActionDetails = actionDetails,
                        Timestamp = startTime
                    };

                    // Lưu vào database
                    await dbContext.AuditLogs.AddAsync(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private string SanitizeContent(string content)
        {
            // Đây là nơi bạn có thể thêm logic để lọc thông tin nhạy cảm như passwords, tokens, etc.
            // Ví dụ đơn giản:
            if (string.IsNullOrEmpty(content))
                return content;

            try
            {
                // Xử lý nếu content là JSON
                var jsonDoc = JsonDocument.Parse(content);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);

                SanitizeJsonElement(jsonDoc.RootElement, writer);

                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                // Nếu không phải JSON, trả về nguyên bản
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

                        // Kiểm tra các field nhạy cảm
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
    }

    // Extension method để dễ dàng đăng ký middleware
    public static class ApiAuditLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiAuditLogMiddleware>();
        }
    }
}