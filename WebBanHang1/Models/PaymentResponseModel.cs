namespace WebBanHang1.Models
{
    public class PaymentResponseModel
    {
        public bool Success { get; set; }             // Trạng thái thanh toán
        public string PaymentMethod { get; set; }     // Phương thức thanh toán
        public string OrderDescription { get; set; }  // Mô tả đơn hàng
        public string OrderId { get; set; }           // Mã đơn hàng
        public string PaymentId { get; set; }         // Mã giao dịch VNPay
        public string TransactionId { get; set; }     // Mã giao dịch
        public string Token { get; set; }             // Token xác thực
        public string VnPayResponseCode { get; set; } // Mã phản hồi từ VNPay
    }
}
