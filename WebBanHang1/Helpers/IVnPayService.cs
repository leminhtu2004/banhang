using WebBanHang1.Models;

namespace WebBanHang1.Helpers
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }

}
