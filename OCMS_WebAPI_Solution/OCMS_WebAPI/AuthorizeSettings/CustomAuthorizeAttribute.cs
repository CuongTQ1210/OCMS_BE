using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Utils;
using System.Security.Claims;

namespace OCMS_WebAPI.AuthorizeSettings
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomAuthorizeAttribute : TypeFilterAttribute
    {
        public CustomAuthorizeAttribute() : base(typeof(CustomAuthorizeFilter))
        {
            // Không cần quyền cụ thể, chỉ cần đăng nhập
            Arguments = new object[] { new string[0], true };
        }

        public CustomAuthorizeAttribute(params string[] roles) : base(typeof(CustomAuthorizeFilter))
        {
            // Yêu cầu các quyền cụ thể
            Arguments = new object[] { roles, false };
        }

        // Constructor đặc biệt cho phép bất kỳ người dùng đã đăng nhập nào
        public CustomAuthorizeAttribute(bool anyAuthenticated) : base(typeof(CustomAuthorizeFilter))
        {
            Arguments = new object[] { new string[0], anyAuthenticated };
        }

        private class CustomAuthorizeFilter : IAuthorizationFilter
        {
            private readonly string[] _roles;
            private readonly bool _anyAuthenticated;

            public CustomAuthorizeFilter(string[] roles, bool anyAuthenticated)
            {
                _roles = roles;
                _anyAuthenticated = anyAuthenticated;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                // Kiểm tra người dùng đã đăng nhập chưa
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Bạn chưa đăng nhập vào hệ thống." });
                    return;
                }

                // Kiểm tra người dùng có ID không
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Không tìm thấy thông tin người dùng." });
                    return;
                }

                // Nếu chỉ yêu cầu xác thực (bất kỳ role nào cũng được), hoặc không có role nào được chỉ định
                if (_anyAuthenticated || _roles == null || _roles.Length == 0)
                {
                    return;
                }

                // Kiểm tra người dùng có quyền truy cập không
                var userRoles = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                // Kiểm tra xem người dùng có bất kỳ role nào trong danh sách được yêu cầu không
                bool hasRequiredRole = _roles.Any(requiredRole => userRoles.Contains(requiredRole));

                if (!hasRequiredRole)
                {
                    context.Result = new ForbiddenObjectResult(new { message = "Bạn không có quyền truy cập chức năng này." });
                    return;
                }
            }
        }
    }

    // Tạo lớp kết quả Forbidden (403) tùy chỉnh
    public class ForbiddenObjectResult : ObjectResult
    {
        public ForbiddenObjectResult(object value) : base(value)
        {
            StatusCode = 403;
        }
    }
}
