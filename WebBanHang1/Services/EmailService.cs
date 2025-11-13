using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using WebBanHang1.Models;

namespace WebBanHang1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["Email:FromEmail"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpHost"],
                    int.Parse(_configuration["Email:SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:SmtpUsername"],
                    _configuration["Email:SmtpPassword"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string verificationToken)
        {
            var subject = "Xác thực email - WebBanHang";
            var body = GenerateEmailVerificationTemplate("Người dùng", verificationToken);
            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
        {
            var subject = "Đặt lại mật khẩu - WebBanHang";
            var body = GeneratePasswordResetTemplate("Người dùng", resetToken);
            return await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Chào mừng bạn đến với WebBanHang";
            var body = GenerateWelcomeEmailTemplate(name);
            return await SendEmailAsync(email, subject, body, true);
        }

        public string GenerateEmailVerificationTemplate(string name, string verificationToken)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:44328";
            var verifyUrl = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}";
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-size: 18px; }}
                        .footer {{ text-align: center; padding: 20px; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Xác thực Email</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào {name},</p>
                            <p>Cảm ơn bạn đã đăng ký tài khoản tại WebBanHang. Để hoàn tất quá trình đăng ký, vui lòng nhấn vào nút bên dưới để xác thực tài khoản:</p>
                            <a href='{verifyUrl}' class='button'>Kích hoạt tài khoản</a>
                            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>Trân trọng,<br>Đội ngũ WebBanHang</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public string GeneratePasswordResetTemplate(string name, string resetToken)
        {
            var resetUrl = $"{_configuration["AppSettings:BaseUrl"]}/Account/ResetPassword?token={resetToken}";
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Đặt lại mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào {name},</p>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Vui lòng nhấp vào nút bên dưới để đặt lại mật khẩu:</p>
                            <a href='{resetUrl}' class='button'>Đặt lại mật khẩu</a>
                            <p>Hoặc copy link sau vào trình duyệt:</p>
                            <p>{resetUrl}</p>
                            <p>Link này có hiệu lực trong {_configuration["Email:PasswordResetExpireMinutes"]} phút.</p>
                            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>Trân trọng,<br>Đội ngũ WebBanHang</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public string GenerateWelcomeEmailTemplate(string name)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f8f9fa; }}
                        .footer {{ text-align: center; padding: 20px; color: #6c757d; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Chào mừng bạn!</h1>
                        </div>
                        <div class='content'>
                            <p>Xin chào {name},</p>
                            <p>Chào mừng bạn đến với WebBanHang! Tài khoản của bạn đã được tạo thành công.</p>
                            <p>Bây giờ bạn có thể:</p>
                            <ul>
                                <li>Mua sắm các sản phẩm chất lượng</li>
                                <li>Theo dõi đơn hàng của mình</li>
                                <li>Nhận thông báo về khuyến mãi</li>
                                <li>Đánh giá sản phẩm</li>
                            </ul>
                            <p>Cảm ơn bạn đã chọn WebBanHang!</p>
                        </div>
                        <div class='footer'>
                            <p>Trân trọng,<br>Đội ngũ WebBanHang</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
} 