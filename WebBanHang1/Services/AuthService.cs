using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Models;
using BCrypt.Net;

namespace WebBanHang1.Services
{
    public class AuthService : IAuthService
    {
        private readonly QuanLiHangContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(QuanLiHangContext context, IEmailService emailService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            // Kiểm tra tài khoản có bị khóa không
            if (await IsAccountLockedAsync(email))
            {
                _logger.LogWarning($"Login attempt for locked account: {email}");
                return false;
            }

            // Kiểm tra mật khẩu
            if (VerifyPassword(password, user.MatKhau))
            {
                await ResetLoginAttemptsAsync(email);
                await RecordLoginAsync(user.MaKh, "LOCAL", success: true);
                return true;
            }

            // Tăng số lần đăng nhập thất bại
            await IncrementLoginAttemptsAsync(email);
            await RecordLoginAsync(user.MaKh, "LOCAL", success: false);
            return false;
        }

        public async Task<KhachHang?> GetUserByEmailAsync(string email)
        {
            return await _context.KhachHangs
                .Include(k => k.VaiTroNavigation)
                .FirstOrDefaultAsync(k => k.Email == email && k.HieuLuc);
        }

        public async Task<KhachHang?> GetUserByGoogleIdAsync(string googleId)
        {
            var googleAuth = await _context.GoogleAuths
                .Include(g => g.MaKhNavigation)
                .ThenInclude(k => k.VaiTroNavigation)
                .FirstOrDefaultAsync(g => g.GoogleId == googleId && g.HieuLuc);

            return googleAuth?.MaKhNavigation;
        }

        public async Task<bool> CreateUserAsync(KhachHang user)
        {
            try
            {
                // Hash mật khẩu
                user.MatKhau = HashPassword(user.MatKhau);
                
                // Tạo mã khách hàng
                user.MaKh = await GenerateMaKHAsync();
                
                // Mặc định chưa xác thực email
                user.EmailVerified = false;
                user.LoginAttempts = 0;

                _context.KhachHangs.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created successfully: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create user: {user.Email}");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(KhachHang user)
        {
            try
            {
                _context.KhachHangs.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update user: {user.Email}");
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            try
            {
                var verification = await _context.EmailVerifications
                    .Include(e => e.MaKhNavigation)
                    .FirstOrDefaultAsync(e => e.Email == email && 
                                             e.VerificationCode == code && 
                                             e.LoaiXacThuc == "EMAIL_VERIFICATION" &&
                                             !e.DaSuDung &&
                                             e.NgayHetHan > DateTime.Now);

                if (verification == null) return false;

                // Cập nhật trạng thái xác thực
                verification.DaSuDung = true;
                verification.MaKhNavigation.EmailVerified = true;
                verification.MaKhNavigation.NgayXacThucEmail = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify email: {email}");
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null) return false;

                var token = GenerateResetToken();
                var expireMinutes = int.Parse(_configuration["Email:VerificationExpireMinutes"]);

                var verification = new EmailVerification
                {
                    MaKh = user.MaKh,
                    Email = email,
                    VerificationCode = token,
                    NgayTao = DateTime.Now,
                    NgayHetHan = DateTime.Now.AddMinutes(expireMinutes),
                    DaSuDung = false,
                    LoaiXacThuc = "EMAIL_VERIFICATION"
                };

                _context.EmailVerifications.Add(verification);
                await _context.SaveChangesAsync();

                return await _emailService.SendEmailVerificationAsync(email, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email verification: {email}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null) return false;

                var token = GenerateResetToken();
                var expireMinutes = int.Parse(_configuration["Email:PasswordResetExpireMinutes"]);

                var reset = new PasswordReset
                {
                    MaKh = user.MaKh,
                    Email = email,
                    ResetToken = token,
                    NgayTao = DateTime.Now,
                    NgayHetHan = DateTime.Now.AddMinutes(expireMinutes),
                    DaSuDung = false
                };

                _context.PasswordResets.Add(reset);
                await _context.SaveChangesAsync();

                return await _emailService.SendPasswordResetAsync(email, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset: {email}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var reset = await _context.PasswordResets
                    .Include(r => r.MaKhNavigation)
                    .FirstOrDefaultAsync(r => r.ResetToken == token && 
                                             !r.DaSuDung &&
                                             r.NgayHetHan > DateTime.Now);

                if (reset == null) return false;

                // Cập nhật mật khẩu
                reset.MaKhNavigation.MatKhau = HashPassword(newPassword);
                reset.DaSuDung = true;

                // Reset login attempts
                reset.MaKhNavigation.LoginAttempts = 0;
                reset.MaKhNavigation.LockoutEnd = null;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reset password for token: {token}");
                return false;
            }
        }

        public async Task<bool> LinkGoogleAccountAsync(string maKH, string googleId, string email, string name, string picture)
        {
            try
            {
                // Kiểm tra nếu GoogleId đã từng tồn tại với HieuLuc=false thì cập nhật lại
                var existing = await _context.GoogleAuths.FirstOrDefaultAsync(g => g.GoogleId == googleId);
                if (existing != null)
                {
                    existing.MaKh = maKH;
                    existing.Email = email;
                    existing.Name = name;
                    existing.Picture = picture;
                    existing.HieuLuc = true;
                    existing.NgayTao = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                // Nếu chưa có thì thêm mới
                var googleAuth = new GoogleAuth
                {
                    MaKh = maKH,
                    GoogleId = googleId,
                    Email = email,
                    Name = name,
                    Picture = picture,
                    NgayTao = DateTime.Now,
                    HieuLuc = true
                };
                _context.GoogleAuths.Add(googleAuth);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to link Google account: {email}");
                return false;
            }
        }

        public async Task<bool> RecordLoginAsync(string maKH, string loginType, string? ipAddress = null, string? userAgent = null, bool success = true)
        {
            try
            {
                var loginHistory = new LoginHistory
                {
                    MaKh = maKH,
                    LoaiDangNhap = loginType,
                    Ipaddress = ipAddress,
                    UserAgent = userAgent,
                    NgayDangNhap = DateTime.Now,
                    ThanhCong = success
                };

                _context.LoginHistories.Add(loginHistory);

                if (success)
                {
                    var user = await _context.KhachHangs.FindAsync(maKH);
                    if (user != null)
                    {
                        user.LastLoginDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to record login: {maKH}");
                return false;
            }
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return user?.LockoutEnd > DateTime.Now;
        }

        public async Task<bool> IncrementLoginAttemptsAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null) return false;

                user.LoginAttempts++;
                var maxAttempts = int.Parse(_configuration["AppSettings:MaxLoginAttempts"]);
                var lockoutMinutes = int.Parse(_configuration["AppSettings:LockoutDurationMinutes"]);

                if (user.LoginAttempts >= maxAttempts)
                {
                    user.LockoutEnd = DateTime.Now.AddMinutes(lockoutMinutes);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to increment login attempts: {email}");
                return false;
            }
        }

        public async Task<bool> ResetLoginAttemptsAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null) return false;

                user.LoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reset login attempts: {email}");
                return false;
            }
        }

        public async Task<bool> LockAccountAsync(string email, int minutes)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null) return false;

                user.LockoutEnd = DateTime.Now.AddMinutes(minutes);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to lock account: {email}");
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private async Task<string> GenerateMaKHAsync()
        {
            var prefix = "KH";
            var lastUser = await _context.KhachHangs
                .Where(k => k.MaKh.StartsWith(prefix))
                .OrderByDescending(k => k.MaKh)
                .FirstOrDefaultAsync();

            if (lastUser == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastUser.MaKh.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        public async Task<bool> UnlinkGoogleAccountAsync(string maKH, string googleId)
        {
            try
            {
                var googleAuth = await _context.GoogleAuths
                    .FirstOrDefaultAsync(g => g.MaKh == maKH && g.GoogleId == googleId && g.HieuLuc);
                if (googleAuth == null) return false;
                googleAuth.HieuLuc = false;
                googleAuth.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unlink Google account: {maKH} - {googleId}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string name)
        {
            try
            {
                var subject = "Chào mừng bạn đến với WebBanHang";
                var body = $@"<div style='background:#4CAF50;color:white;padding:20px;text-align:center;font-size:24px;font-weight:bold;'>Chào mừng bạn!</div><div style='padding:20px;'>Xin chào {name},<br/><br/>Chào mừng bạn đến với WebBanHang! Tài khoản của bạn đã được tạo thành công.<br/><br/>Bây giờ bạn có thể:<ul><li>Mua sắm các sản phẩm chất lượng</li><li>Theo dõi đơn hàng của mình</li><li>Nhận thông báo về khuyến mãi</li><li>Đánh giá sản phẩm</li></ul>Cảm ơn bạn đã chọn WebBanHang!<br/><br/>Trân trọng,<br/>Đội ngũ WebBanHang</div>";
                return await _emailService.SendEmailAsync(email, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"[SendWelcomeEmailAsync] Lỗi gửi email chào mừng cho {email}");
                return false;
            }
        }
    }
} 