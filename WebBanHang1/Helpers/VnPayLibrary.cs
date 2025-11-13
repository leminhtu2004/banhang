using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebBanHang1.Models;

namespace WebBanHang1.Helpers
{
    public class VnPayLibrary
    {
        private readonly string vnp_TmnCode;
        private readonly string vnp_HashSecret;
        private readonly string vnp_Url;
        private readonly string vnp_Command;
        private readonly string vnp_CurrCode;
        private readonly string vnp_Version;
        private readonly string vnp_Locale;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VnPayLibrary> _logger;

        public VnPayLibrary(IHttpContextAccessor httpContextAccessor, ILogger<VnPayLibrary> logger, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            vnp_TmnCode = configuration["Vnpay:TmnCode"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:TmnCode is not configured");
            vnp_HashSecret = configuration["Vnpay:HashSecret"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:HashSecret is not configured");
            vnp_Url = configuration["Vnpay:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:BaseUrl is not configured");
            vnp_Command = configuration["Vnpay:Command"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:Command is not configured");
            vnp_CurrCode = configuration["Vnpay:CurrCode"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:CurrCode is not configured");
            vnp_Version = configuration["Vnpay:Version"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:Version is not configured");
            vnp_Locale = configuration["Vnpay:Locale"] ?? throw new ArgumentNullException(nameof(configuration), "Vnpay:Locale is not configured");
        }

        public string CreateRequestUrl(string returnUrl, decimal amount, string orderInfo, string txnRef)
        {
            try
            {
                var vnp = new SortedDictionary<string, string>
                {
                    { "vnp_Version", vnp_Version },
                    { "vnp_Command", vnp_Command },
                    { "vnp_TmnCode", vnp_TmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() }, // Convert thành VND
                    { "vnp_CurrCode", vnp_CurrCode },
                    { "vnp_Locale", vnp_Locale },
                    { "vnp_OrderInfo", orderInfo },
                    { "vnp_OrderType", "billpayment" },
                    { "vnp_ReturnUrl", returnUrl },
                    { "vnp_TxnRef", txnRef },
                    { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                    { "vnp_IpAddr", GetIpAddress() } // Lấy địa chỉ IP của người dùng
                };

                var queryString = CreateQueryString(vnp);
                _logger.LogInformation($"Query string before hash: {queryString}");

                var hashData = GetHashData(queryString); // Sử dụng phương thức GetHashData
                _logger.LogInformation($"Generated hash: {hashData}");

                vnp.Add("vnp_SecureHash", hashData);
                var finalQueryString = CreateQueryString(vnp);
                var paymentUrl = $"{vnp_Url}?{finalQueryString}";

                _logger.LogInformation($"Final payment URL: {paymentUrl}");
                return paymentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VNPay URL: {ex.Message}");
                throw;
            }
        }

        private string CreateQueryString(SortedDictionary<string, string> vnp)
        {
            var queryString = new StringBuilder();
            foreach (var kvp in vnp.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                if (queryString.Length > 0)
                {
                    queryString.Append('&');
                }
                queryString.Append($"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}");
            }
            return queryString.ToString();
        }

        private string GetHashData(string queryString)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)); // Sử dụng SHA512
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }

        public string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "0.0.0.0";

            var ip = context.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrEmpty(ip) ? "0.0.0.0" : ip;
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collections, string hashSecret)
        {
            var response = new PaymentResponseModel
            {
                Success = collections["vnp_ResponseCode"] == "00",
                PaymentMethod = "VNPay",
                OrderDescription = collections["vnp_OrderInfo"].FirstOrDefault() ?? string.Empty,
                OrderId = collections["vnp_TxnRef"].FirstOrDefault() ?? string.Empty,
                PaymentId = collections["vnp_TransactionNo"].FirstOrDefault() ?? string.Empty,
                TransactionId = collections["vnp_TransactionNo"].FirstOrDefault() ?? string.Empty,
                Token = collections["vnp_SecureHash"].FirstOrDefault() ?? string.Empty,
                VnPayResponseCode = collections["vnp_ResponseCode"].FirstOrDefault() ?? string.Empty
            };
            return response;
        }
    }
}
