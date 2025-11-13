using WebBanHang1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebBanHang1.Helpers
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VnPayService> _logger;
        private readonly ILogger<VnPayLibrary> _vnPayLibraryLogger;

        public VnPayService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<VnPayService> logger, ILogger<VnPayLibrary> vnPayLibraryLogger)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _vnPayLibraryLogger = vnPayLibraryLogger;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"] ?? "SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary(_httpContextAccessor, _vnPayLibraryLogger, _configuration);

            var requestData = new SortedDictionary<string, string>
            {
                { "vnp_Version", _configuration["Vnpay:Version"] ?? throw new ArgumentNullException("Vnpay:Version") },
                { "vnp_Command", _configuration["Vnpay:Command"] ?? throw new ArgumentNullException("Vnpay:Command") },
                { "vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? throw new ArgumentNullException("Vnpay:TmnCode") },
                { "vnp_Amount", ((int)model.Amount * 100).ToString() }, // Nhân 100 để chuyển sang số nguyên
                { "vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? throw new ArgumentNullException("Vnpay:CurrCode") },
                { "vnp_IpAddr", pay.GetIpAddress() },
                { "vnp_Locale", _configuration["Vnpay:Locale"] ?? throw new ArgumentNullException("Vnpay:Locale") },
                { "vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}" },
                { "vnp_OrderType", model.OrderType },
                { "vnp_ReturnUrl", _configuration["Vnpay:ReturnUrl"] ?? throw new ArgumentNullException("Vnpay:ReturnUrl") },
                { "vnp_TxnRef", tick }
            };

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"] ?? throw new ArgumentNullException("Vnpay:BaseUrl"), (decimal)model.Amount, $"{model.Name} {model.OrderDescription} {model.Amount}", tick);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary(_httpContextAccessor, _vnPayLibraryLogger, _configuration);
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"] ?? throw new ArgumentNullException("Vnpay:HashSecret"));
            return response;
        }
    }
}
