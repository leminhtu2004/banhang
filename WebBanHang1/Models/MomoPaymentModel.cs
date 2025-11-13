using System;
using System.Collections.Generic;

namespace WebBanHang1.Models
{
    public class MomoPaymentModel
    {
        public string PartnerCode { get; set; }
        public string RequestId { get; set; }
        public long Amount { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string RedirectUrl { get; set; }
        public string IpnUrl { get; set; }
        public string RequestType { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }
    }

    public class MomoPaymentResponseModel
    {
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public long Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public string Signature { get; set; }
    }
} 