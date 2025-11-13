using WebBanHang1.Models;

namespace WebBanHang1.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailVerificationAsync(string email, string verificationCode);
        Task<bool> SendPasswordResetAsync(string email, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string email, string name);
        string GenerateEmailVerificationTemplate(string name, string verificationCode);
        string GeneratePasswordResetTemplate(string name, string resetToken);
        string GenerateWelcomeEmailTemplate(string name);
    }
} 