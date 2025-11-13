using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang1.Data;

namespace WebBanHang1.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly QuanLiHangContext _context;

        public MenuLoaiViewComponent(QuanLiHangContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loaiList = await _context.Loais
                .Select(l => new
                {
                    l.MaLoai,
                    l.TenLoai,
                    ProductCount = _context.HangHoas.Count(h => h.MaLoai == l.MaLoai)
                })
                .ToListAsync();

            return View(loaiList);
        }
    }
}