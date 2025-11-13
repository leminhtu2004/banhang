using System.Collections.Generic;

namespace WebBanHang1.Models
{
    public class AdminViewModel
    {
        public List<HangHoa> Products { get; set; }
        public List<Loai> LoaiSanPhams { get; set; }
        public List<HoaDon> Orders { get; set; }
        public decimal TotalRevenue { get; set; } // Add this property

    }
}