using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebBanHang1.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu người dùng đã đăng nhập nhưng không có vai trò "Admin"
            if (context.User.Identity.IsAuthenticated &&
                !context.User.IsInRole("Admin") &&
                context.Request.Path.StartsWithSegments("/Admin"))
            {
                // Chuyển hướng đến trang AccessDenied
                context.Response.Redirect("/Admin/AccessDenied");
                return;
            }

            // Tiếp tục xử lý các middleware khác
            await _next(context);
        }
    }
}
