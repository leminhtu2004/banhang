using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebBanHang1.Data;
using WebBanHang1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebBanHang1.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WebBanHang1.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLiHangContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController>? _logger;

        public AccountController(QuanLiHangContext context, IWebHostEnvironment webHostEnvironment, 
            IAuthService authService, IEmailService emailService, IConfiguration configuration, ILogger<AccountController>? logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authService = authService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "Bạn không có đủ thẩm quyền để truy cập trang này.";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            ViewBag.HasCustomers = _context.KhachHangs.Any();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(KhachHang khachHang)
        {
            ModelState.Remove("VaiTroNavigation");
            khachHang.HieuLuc = false; // Đảm bảo tài khoản chưa kích hoạt

            // Kiểm tra và xử lý MaKh TRƯỚC KHI kiểm tra ModelState
            var hasCustomers = _context.KhachHangs.Any();

            if (!hasCustomers)
            {
                // Nếu chưa có khách hàng nào, yêu cầu nhập MaKh
                if (string.IsNullOrEmpty(khachHang.MaKh))
                {
                    ModelState.AddModelError("MaKh", "Vui lòng nhập mã khách hàng.");
                    return View(khachHang);
                }
            }
            else
            {
                // Nếu đã có khách hàng, tự động tạo MaKh
                var lastCustomer = _context.KhachHangs.OrderByDescending(u => u.MaKh).FirstOrDefault();
                int lastCustomerNumber = 0;

                if (lastCustomer != null)
                {
                    int.TryParse(lastCustomer.MaKh, out lastCustomerNumber);
                }

                // Tăng giá trị lên 1 và định dạng thành chuỗi 3 chữ số
                khachHang.MaKh = (lastCustomerNumber + 1).ToString("D3");
            }

            // Đặt vai trò mặc định là 1 (Khách hàng)
            khachHang.VaiTro = 1;

            // Đặt hiệu lực mặc định là true
            khachHang.HieuLuc = true;

            // Bỏ qua validation cho MaKh vì đã xử lý ở trên
            ModelState.Remove("MaKh");

            // Kiểm tra định dạng email
            try
            {
                var addr = new System.Net.Mail.MailAddress(khachHang.Email);
                if (addr.Address != khachHang.Email)
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ.");
                    return View(khachHang);
                }
            }
            catch
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(khachHang);
            }
                // Kiểm tra xem email đã tồn tại chưa
                if (_context.KhachHangs.Any(u => u.Email == khachHang.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(khachHang);
                }

            if (ModelState.IsValid)
            {
                // Sử dụng AuthService để tạo user
                if (await _authService.CreateUserAsync(khachHang))
                {
                    // Gửi email xác thực (không gửi email chào mừng ở đây)
                    await _authService.SendEmailVerificationAsync(khachHang.Email);
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
                return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.");
                }
            }

            return View(khachHang);
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng AuthService để xác thực
                if (await _authService.ValidateUserAsync(userLogin.Email, userLogin.MatKhau))
                {
                    var existingUser = await _authService.GetUserByEmailAsync(userLogin.Email);
                    if (existingUser != null)
                    {
                        // Tạo claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, existingUser.Email),
                            new Claim(ClaimTypes.NameIdentifier, existingUser.MaKh),
                            new Claim("FullName", existingUser.HoTen),
                            new Claim(ClaimTypes.Role, existingUser.VaiTro == 2 ? "Admin" : "User"),
                            new Claim("MaKH", existingUser.MaKh) // Thêm claim MaKH
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Đăng nhập người dùng
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        // Lưu thông tin vào session
                        HttpContext.Session.SetString("MaKH", existingUser.MaKh);
                        HttpContext.Session.SetString("HoTen", existingUser.HoTen);
                        HttpContext.Session.SetInt32("MaVaiTro", existingUser.VaiTro);

                        // Thêm thông báo thành công
                        TempData["SuccessMessage"] = "Đăng nhập thành công!";

                        // Chuyển hướng dựa trên vai trò
                        if (existingUser.VaiTro == 2) // Nếu là admin
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        return RedirectToAction("Index", "Products");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                }
            }
            return View(userLogin);
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
                return View();
            }
            // Kiểm tra định dạng email
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ.");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View();
            }
            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email không tồn tại trong hệ thống.");
                return View();
            }
            if (await _authService.SendPasswordResetAsync(email))
            {
                TempData["SuccessMessage"] = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("Email", "Có lỗi xảy ra khi gửi email đặt lại mật khẩu.");
            }
            return View();
        }

        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Token không hợp lệ.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            if (await _authService.ResetPasswordAsync(model.Token, model.NewPassword))
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "Token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Login");
            }
        }

        // GET: Account/VerifyEmail
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger?.LogWarning("[VerifyEmail] Token rỗng hoặc null.");
                TempData["ErrorMessage"] = "Link xác thực không hợp lệ.";
                return RedirectToAction("Login");
            }
            _logger?.LogInformation($"[VerifyEmail] Nhận token: {token}");
            var verification = await _context.EmailVerifications
                .Where(e => e.VerificationCode == token && e.LoaiXacThuc == "EMAIL_VERIFICATION")
                .FirstOrDefaultAsync();
            if (verification == null)
            {
                _logger?.LogWarning($"[VerifyEmail] Không tìm thấy token: {token} trong DB.");
                TempData["ErrorMessage"] = "Link xác thực không hợp lệ.";
                return RedirectToAction("Login");
            }
            if (verification.DaSuDung)
            {
                _logger?.LogInformation($"[VerifyEmail] Token đã dùng. Email: {verification.Email}, MaKH: {verification.MaKh}");
                TempData["SuccessMessage"] = "Tài khoản đã được xác thực, bạn có thể đăng nhập.";
                return RedirectToAction("Login");
            }
            if (verification.NgayHetHan <= DateTime.Now)
            {
                _logger?.LogWarning($"[VerifyEmail] Token hết hạn. Email: {verification.Email}, MaKH: {verification.MaKh}, NgayHetHan: {verification.NgayHetHan}");
                TempData["ErrorMessage"] = "Link xác thực đã hết hạn, vui lòng gửi lại email xác thực.";
                return RedirectToAction("ResendVerification", new { email = verification.Email });
            }
            // Xác thực tài khoản
            var user = await _context.KhachHangs.FindAsync(verification.MaKh);
            if (user == null)
            {
                _logger?.LogError($"[VerifyEmail] Không tìm thấy user với MaKH: {verification.MaKh}, Email: {verification.Email}");
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Login");
            }
            // TỰ ĐỘNG LIÊN KẾT GOOGLE nếu có thông tin GoogleId trong session
            string pendingGoogleId = null;
            if (user.HieuLuc)
            {
                TempData["SuccessMessage"] = "Tài khoản đã được xác thực, bạn có thể đăng nhập.";
                user.HieuLuc = true;
                user.EmailVerified = true;
                await _context.SaveChangesAsync();
                // Gửi email chào mừng
                await _authService.SendWelcomeEmailAsync(user.Email, user.HoTen);
                // Gán giá trị, không khai báo lại
                pendingGoogleId = HttpContext.Session.GetString("PendingGoogleId");
                if (!string.IsNullOrEmpty(pendingGoogleId))
                {
                    var pendingGoogleEmail = HttpContext.Session.GetString("PendingGoogleEmail");
                    var pendingGoogleName = HttpContext.Session.GetString("PendingGoogleName");
                    var pendingGooglePicture = HttpContext.Session.GetString("PendingGooglePicture");
                    await _authService.LinkGoogleAccountAsync(user.MaKh, pendingGoogleId, pendingGoogleEmail, pendingGoogleName, pendingGooglePicture);
                    // Xóa thông tin tạm sau khi liên kết
                    HttpContext.Session.Remove("PendingGoogleId");
                    HttpContext.Session.Remove("PendingGoogleEmail");
                    HttpContext.Session.Remove("PendingGoogleName");
                    HttpContext.Session.Remove("PendingGooglePicture");
                }
                // Nếu thiếu thông tin cá nhân thì chuyển hướng đến Edit
                if (string.IsNullOrWhiteSpace(user.HoTen) || user.NgaySinh == default || string.IsNullOrWhiteSpace(user.DiaChi) || string.IsNullOrWhiteSpace(user.DienThoai))
                {
                    // Đăng nhập tạm thời để cho phép cập nhật thông tin
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.MaKh),
                        new Claim("FullName", user.HoTen ?? ""),
                        new Claim(ClaimTypes.Role, user.VaiTro == 2 ? "Admin" : "User"),
                        new Claim("MaKH", user.MaKh)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    HttpContext.Session.SetString("MaKH", user.MaKh);
                    HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                    HttpContext.Session.SetInt32("MaVaiTro", user.VaiTro);
                    TempData["SuccessMessage"] = "Vui lòng cập nhật đầy đủ thông tin cá nhân để hoàn tất đăng ký.";
                    return RedirectToAction("Edit");
                }
                return RedirectToAction("Login");
            }
            user.HieuLuc = true;
            user.EmailVerified = true;
            await _context.SaveChangesAsync();
            // Gửi email chào mừng
            await _authService.SendWelcomeEmailAsync(user.Email, user.HoTen);
            // Gán giá trị, không khai báo lại
            pendingGoogleId = HttpContext.Session.GetString("PendingGoogleId");
            if (!string.IsNullOrEmpty(pendingGoogleId))
            {
                var pendingGoogleEmail = HttpContext.Session.GetString("PendingGoogleEmail");
                var pendingGoogleName = HttpContext.Session.GetString("PendingGoogleName");
                var pendingGooglePicture = HttpContext.Session.GetString("PendingGooglePicture");
                await _authService.LinkGoogleAccountAsync(user.MaKh, pendingGoogleId, pendingGoogleEmail, pendingGoogleName, pendingGooglePicture);
                // Xóa thông tin tạm sau khi liên kết
                HttpContext.Session.Remove("PendingGoogleId");
                HttpContext.Session.Remove("PendingGoogleEmail");
                HttpContext.Session.Remove("PendingGoogleName");
                HttpContext.Session.Remove("PendingGooglePicture");
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản Google để liên kết.";
            }
            return RedirectToAction("Login");
        }

        // GET: Account/ResendVerification
        public IActionResult ResendVerification()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập email.");
                return View();
            }
            // Kiểm tra định dạng email
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ.");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View();
            }
            // Kiểm tra email có tồn tại trong hệ thống không
            var user = await _authService.GetUserByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email không tồn tại trong hệ thống.");
                return View();
            }

            if (await _authService.SendEmailVerificationAsync(email))
            {
                TempData["SuccessMessage"] = "Email xác thực đã được gửi lại. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("Email", "Email không tồn tại hoặc có lỗi xảy ra.");
            }

            return View();
        }

        // GET: Account/GoogleLogin
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account")
            };
            return Challenge(properties, "Google");
        }

        // GET: Account/GoogleCallback
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                _logger?.LogInformation("Bắt đầu xử lý Google callback");
                
                // Lấy thông tin từ Google
                var result = await HttpContext.AuthenticateAsync("Google");
                if (!result.Succeeded)
                {
                    _logger?.LogError("Google authentication failed: {Error}", result.Failure?.Message);
                    TempData["ErrorMessage"] = "Đăng nhập Google thất bại.";
                    return RedirectToAction("Login");
                }

                // Log tất cả claims để debug
                _logger?.LogInformation("Google authentication successful. Claims:");
                foreach (var claim in result.Principal.Claims)
                {
                    _logger?.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                var googleId = result.Principal.FindFirstValue("sub") ?? 
                              result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                var email = result.Principal.FindFirstValue("email") ?? 
                           result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                var name = result.Principal.FindFirstValue("name") ?? 
                          result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                var picture = result.Principal.FindFirstValue("picture");

                _logger?.LogInformation("Extracted Google info - ID: {GoogleId}, Email: {Email}, Name: {Name}, Picture: {Picture}", 
                    googleId, email, name, picture);

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    _logger?.LogError("Missing required Google information - GoogleId: {GoogleId}, Email: {Email}", googleId, email);
                    TempData["ErrorMessage"] = "Không thể lấy thông tin từ Google. Bạn cần cấp quyền truy cập email và thông tin cá nhân cho ứng dụng. Vui lòng thử lại và chọn đúng tài khoản Google, đồng ý cấp quyền khi được hỏi.";
                    return RedirectToAction("Login");
                }

                // 1. Đã liên kết GoogleId -> đăng nhập luôn nếu tài khoản hợp lệ
                var existingUser = await _authService.GetUserByGoogleIdAsync(googleId);
                if (existingUser != null)
                {
                    if (!existingUser.HieuLuc)
                    {
                        TempData["ErrorMessage"] = "Tài khoản của bạn chưa được kích hoạt hoặc đã bị vô hiệu hóa. Vui lòng kiểm tra email để xác thực.";
                        return RedirectToAction("Login");
                    }
                    if (existingUser.LockoutEnd != null && existingUser.LockoutEnd > DateTime.Now)
                    {
                        TempData["ErrorMessage"] = $"Tài khoản của bạn đang bị khóa đến {existingUser.LockoutEnd:dd/MM/yyyy HH:mm}. Vui lòng thử lại sau.";
                        return RedirectToAction("Login");
                    }
                    await _authService.RecordLoginAsync(existingUser.MaKh, "GOOGLE", 
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        HttpContext.Request.Headers["User-Agent"].ToString());

                    // Tạo claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, existingUser.Email),
                        new Claim(ClaimTypes.NameIdentifier, existingUser.MaKh),
                        new Claim("FullName", existingUser.HoTen),
                        new Claim(ClaimTypes.Role, existingUser.VaiTro == 2 ? "Admin" : "User"),
                        new Claim("MaKH", existingUser.MaKh)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    HttpContext.Session.SetString("MaKH", existingUser.MaKh);
                    HttpContext.Session.SetString("HoTen", existingUser.HoTen);
                    HttpContext.Session.SetInt32("MaVaiTro", existingUser.VaiTro);
                    TempData["SuccessMessage"] = "Đăng nhập Google thành công!";
                    if (existingUser.VaiTro == 2) return RedirectToAction("Index", "Admin"); else return RedirectToAction("Index", "Products");
                }

                // 2. Đã có user theo email nhưng chưa liên kết GoogleId
                var userByEmail = await _authService.GetUserByEmailAsync(email);
                if (userByEmail != null)
                {
                    if (!userByEmail.HieuLuc)
                    {
                        // Lưu thông tin Google tạm vào session để liên kết sau khi xác thực
                        HttpContext.Session.SetString("PendingGoogleId", googleId);
                        HttpContext.Session.SetString("PendingGoogleEmail", email);
                        HttpContext.Session.SetString("PendingGoogleName", name ?? "");
                        HttpContext.Session.SetString("PendingGooglePicture", picture ?? "");
                        TempData["ErrorMessage"] = "Tài khoản email này chưa được kích hoạt. Vui lòng kiểm tra email để xác thực trước khi đăng nhập bằng Google.";
                        return RedirectToAction("Login");
                    }
                    // Liên kết GoogleId với tài khoản đã đăng ký
                    await _authService.LinkGoogleAccountAsync(userByEmail.MaKh, googleId, email, name, picture);
                    // Đăng nhập luôn
                    await _authService.RecordLoginAsync(userByEmail.MaKh, "GOOGLE", 
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        HttpContext.Request.Headers["User-Agent"].ToString());
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userByEmail.Email),
                        new Claim(ClaimTypes.NameIdentifier, userByEmail.MaKh),
                        new Claim("FullName", userByEmail.HoTen),
                        new Claim(ClaimTypes.Role, userByEmail.VaiTro == 2 ? "Admin" : "User"),
                        new Claim("MaKH", userByEmail.MaKh)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    HttpContext.Session.SetString("MaKH", userByEmail.MaKh);
                    HttpContext.Session.SetString("HoTen", userByEmail.HoTen);
                    HttpContext.Session.SetInt32("MaVaiTro", userByEmail.VaiTro);
                    TempData["SuccessMessage"] = "Đăng nhập Google thành công!";
                    if (userByEmail.VaiTro == 2) return RedirectToAction("Index", "Admin"); else return RedirectToAction("Index", "Products");
                }

                // 3. Chưa có user -> tạo user mới, gửi email xác thực, chưa cho đăng nhập
                var checkUser = await _authService.GetUserByEmailAsync(email);
                if (checkUser != null)
                {
                    if (!checkUser.HieuLuc)
                    {
                        TempData["ErrorMessage"] = "Tài khoản này chưa được kích hoạt. Vui lòng kiểm tra email để xác thực trước khi đăng nhập bằng Google.";
                        // Lưu thông tin Google vào session để tự động liên kết sau khi xác thực
                        HttpContext.Session.SetString("PendingGoogleId", googleId);
                        HttpContext.Session.SetString("PendingGoogleEmail", email);
                        HttpContext.Session.SetString("PendingGoogleName", name ?? "");
                        HttpContext.Session.SetString("PendingGooglePicture", picture ?? "");
                        return RedirectToAction("Login");
                    }
                    TempData["ErrorMessage"] = "Email này đã được đăng ký. Vui lòng đăng nhập hoặc liên kết Google với tài khoản hiện có.";
                    return RedirectToAction("Login");
                }
                var newUser = new KhachHang
                {
                    HoTen = name ?? "Google User",
                    Email = email,
                    MatKhau = Guid.NewGuid().ToString(),
                    GioiTinh = true,
                    NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)),
                    HieuLuc = true, // Đặt hiệu lực luôn
                    EmailVerified = true, // Đặt xác thực email luôn
                    VaiTro = 1
                };
                if (await _authService.CreateUserAsync(newUser))
                {
                    var createdUser = await _authService.GetUserByEmailAsync(email);
                    if (createdUser != null)
                    {
                        await _authService.LinkGoogleAccountAsync(createdUser.MaKh, googleId, email, name, picture);
                        // Gửi email chào mừng
                        await _authService.SendWelcomeEmailAsync(email, name ?? "Google User");
                        // Đăng nhập luôn
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, createdUser.Email),
                            new Claim(ClaimTypes.NameIdentifier, createdUser.MaKh),
                            new Claim("FullName", createdUser.HoTen),
                            new Claim(ClaimTypes.Role, createdUser.VaiTro == 2 ? "Admin" : "User"),
                            new Claim("MaKH", createdUser.MaKh)
                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        HttpContext.Session.SetString("MaKH", createdUser.MaKh);
                        HttpContext.Session.SetString("HoTen", createdUser.HoTen);
                        HttpContext.Session.SetInt32("MaVaiTro", createdUser.VaiTro);
                        TempData["SuccessMessage"] = "Đăng nhập Google thành công!";
                        if (createdUser.VaiTro == 2) return RedirectToAction("Index", "Admin");
                        else return RedirectToAction("Index", "Products");
                    }
                }
                TempData["ErrorMessage"] = "Không thể tạo tài khoản Google. Vui lòng thử lại.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng nhập Google.";
            }
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Profile()
        {
            var maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                return RedirectToAction("Login");
            }

            var khachHang = _context.KhachHangs.Find(maKH);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // GET: Account/Edit
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                return RedirectToAction("Login");
            }

            var khachHang = await _context.KhachHangs.FindAsync(maKH);

            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(KhachHang khachHang)
        {
            ModelState.Remove("VaiTroNavigation");
            ModelState.Remove("MatKhau");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy MaKH từ session thay vì model để đảm bảo security
                    var maKH = HttpContext.Session.GetString("MaKH");
                    var existingKhachHang = await _context.KhachHangs.FindAsync(maKH);

                    if (existingKhachHang == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin
                    existingKhachHang.HoTen = khachHang.HoTen;
                    existingKhachHang.GioiTinh = khachHang.GioiTinh;
                    existingKhachHang.NgaySinh = khachHang.NgaySinh;
                    existingKhachHang.DiaChi = khachHang.DiaChi;
                    existingKhachHang.DienThoai = khachHang.DienThoai;
                    existingKhachHang.Email = khachHang.Email;

                    await _context.SaveChangesAsync();

                    // Cập nhật session
                    HttpContext.Session.SetString("HoTen", existingKhachHang.HoTen);

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Profile");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachHangExists(khachHang.MaKh))
                    {
                        return NotFound();
                    }
                    else
                    {
                    throw;
                    }
                }
            }
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file ảnh.";
                return RedirectToAction("Profile");
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif).";
                return RedirectToAction("Profile");
            }

            // Kiểm tra kích thước file (tối đa 5MB)
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File ảnh không được vượt quá 5MB.";
                return RedirectToAction("Profile");
            }

            try
            {
                var maKH = HttpContext.Session.GetString("MaKH");
            var khachHang = await _context.KhachHangs.FindAsync(maKH);

            if (khachHang == null)
            {
                return NotFound();
            }

                // Tạo tên file mới
                var fileName = $"avatar_{maKH}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh trong database
                khachHang.Hinh = $"/images/avatars/{fileName}";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật ảnh đại diện.";
            }

            return RedirectToAction("Profile");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Đăng xuất
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UnlinkGoogle(string googleId)
        {
            var maKH = HttpContext.Session.GetString("MaKH");
            if (maKH == null)
            {
                return RedirectToAction("Login");
            }
            if (string.IsNullOrEmpty(googleId))
            {
                TempData["ErrorMessage"] = "Thiếu thông tin GoogleId.";
                return RedirectToAction("Profile");
            }
            var result = await _authService.UnlinkGoogleAccountAsync(maKH, googleId);
            if (result)
            {
                TempData["SuccessMessage"] = "Hủy liên kết Google thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Hủy liên kết Google thất bại hoặc tài khoản Google không tồn tại.";
            }
            return RedirectToAction("Profile");
        }

        [Authorize]
        public IActionResult LinkGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LinkGoogleCallback", "Account")
            };
            return Challenge(properties, "Google");
        }

        [Authorize]
        public async Task<IActionResult> LinkGoogleCallback()
        {
            try
            {
                _logger?.LogInformation("Bắt đầu xử lý LinkGoogle callback");
                
                var result = await HttpContext.AuthenticateAsync("Google");
                if (!result.Succeeded)
                {
                    _logger?.LogError("Google authentication failed in LinkGoogle: {Error}", result.Failure?.Message);
                    TempData["ErrorMessage"] = "Không thể lấy thông tin từ Google.";
                    return RedirectToAction("Profile");
                }

                // Log tất cả claims để debug
                _logger?.LogInformation("LinkGoogle - Google authentication successful. Claims:");
                foreach (var claim in result.Principal.Claims)
                {
                    _logger?.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                var googleId = result.Principal.FindFirstValue("sub") ?? 
                              result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                var email = result.Principal.FindFirstValue("email") ?? 
                           result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                var name = result.Principal.FindFirstValue("name") ?? 
                          result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                var picture = result.Principal.FindFirstValue("picture");

                _logger?.LogInformation("LinkGoogle - Extracted Google info - ID: {GoogleId}, Email: {Email}, Name: {Name}, Picture: {Picture}", 
                    googleId, email, name, picture);

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    _logger?.LogError("LinkGoogle - Missing required Google information - GoogleId: {GoogleId}, Email: {Email}", googleId, email);
                    TempData["ErrorMessage"] = "Không thể lấy thông tin từ Google. Vui lòng đảm bảo bạn đã cấp quyền truy cập email và thông tin cá nhân.";
                    return RedirectToAction("Profile");
                }

            // Lấy MaKH từ session
            var maKH = HttpContext.Session.GetString("MaKH");
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["ErrorMessage"] = "Không xác định được tài khoản hiện tại.";
                return RedirectToAction("Login");
            }

            // Kiểm tra đã liên kết chưa
            var existing = await _authService.GetUserByGoogleIdAsync(googleId);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "Tài khoản Google này đã được liên kết với tài khoản khác.";
                return RedirectToAction("Profile");
            }

            // Liên kết Google với tài khoản hiện tại
            var success = await _authService.LinkGoogleAccountAsync(maKH, googleId, email, name, picture);
            if (success)
            {
                TempData["SuccessMessage"] = "Liên kết tài khoản Google thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Liên kết tài khoản Google thất bại.";
            }
            return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Có lỗi xảy ra khi liên kết Google account");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi liên kết tài khoản Google.";
                return RedirectToAction("Profile");
            }
        }

        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKh == id);
        }
    }
}
