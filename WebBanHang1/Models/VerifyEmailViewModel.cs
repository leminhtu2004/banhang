using System.ComponentModel.DataAnnotations;

namespace WebBanHang1.Models
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mã xác thực không được để trống.")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mã xác thực.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác thực phải có 6 chữ số.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã xác thực chỉ chứa chữ số.")]
        public string VerificationCode { get; set; } = null!;
    }
} 