namespace WebBanHang1.Models
{
    public class VnPaymentRequestModel
    {
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public string FullName { get; set; }
        public int OrderId { get; set; }
    }
}