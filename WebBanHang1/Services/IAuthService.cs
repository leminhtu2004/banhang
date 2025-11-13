using WebBanHang1.Models;

namespace WebBanHang1.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string email, string password);
        Task<KhachHang?> GetUserByEmailAsync(string email);
        Task<KhachHang?> GetUserByGoogleIdAsync(string googleId);
        Task<bool> CreateUserAsync(KhachHang user);
        Task<bool> UpdateUserAsync(KhachHang user);
        Task<bool> VerifyEmailAsync(string email, string code);
        Task<bool> SendEmailVerificationAsync(string email);
        Task<bool> SendPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> LinkGoogleAccountAsync(string maKH, string googleId, string email, string name, string picture);
        Task<bool> RecordLoginAsync(string maKH, string loginType, string? ipAddress = null, string? userAgent = null, bool success = true);
        Task<bool> IsAccountLockedAsync(string email);
        Task<bool> IncrementLoginAttemptsAsync(string email);
        Task<bool> ResetLoginAttemptsAsync(string email);
        Task<bool> LockAccountAsync(string email, int minutes);
        Task<bool> UnlinkGoogleAccountAsync(string maKH, string googleId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateVerificationCode();
        string GenerateResetToken();
        Task<bool> SendWelcomeEmailAsync(string email, string name);
    }
} 