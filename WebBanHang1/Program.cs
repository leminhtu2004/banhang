using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Google.Apis.Auth.AspNetCore3;
using WebBanHang1.Services; // Thêm namespace cho dịch vụ
using WebBanHang1.Middlewares; // Thêm namespace cho middleware
using Microsoft.AspNetCore.Authentication.Google;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình chuỗi kết nối
var getConnectionStr = builder.Configuration.GetConnectionString("ConnectString");
builder.Services.AddDbContext<QuanLiHangContext>(options =>
    options.UseSqlServer(getConnectionStr));

// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews(); // Nếu bạn dùng MVC
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddHttpClient();

// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(365);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký VnPayLibrary
builder.Services.AddScoped<VnPayLibrary>();

// Đăng ký các service mới
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Đăng ký SignalR
builder.Services.AddSignalR();

// Thêm dịch vụ Authorization và Authentication
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                var response = await context.Backchannel.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                var userInfo = await response.Content.ReadAsStringAsync();
                var user = JsonDocument.Parse(userInfo).RootElement;

                context.Identity?.AddClaim(new System.Security.Claims.Claim("sub", user.GetProperty("id").GetString() ?? ""));
                context.Identity?.AddClaim(new System.Security.Claims.Claim("email", user.GetProperty("email").GetString() ?? ""));
                context.Identity?.AddClaim(new System.Security.Claims.Claim("name", user.GetProperty("name").GetString() ?? ""));
                context.Identity?.AddClaim(new System.Security.Claims.Claim("picture", user.GetProperty("picture").GetString() ?? ""));
            }
        };
});

// Register Promotion Service
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddHostedService<PromotionBackgroundService>();

// Register Notification Cleanup Service
builder.Services.AddHostedService<NotificationCleanupService>();

var app = builder.Build(); // Gọi Build() sau khi hoàn tất cấu hình dịch vụ

// Cấu hình middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseRouting();

app.UseSession(); // Sử dụng session
app.UseAuthentication(); // Sử dụng Authentication
app.UseAuthorization(); // Sử dụng Authorization

// Cấu hình các endpoint
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

// Cấu hình SignalR Hub
app.MapHub<WebBanHang1.Hubs.NotificationHub>("/notificationHub");

// Chạy ứng dụng
app.Run();
