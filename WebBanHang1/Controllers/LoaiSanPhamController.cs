using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang1.Data;
using WebBanHang1.Models;
using System.Threading.Tasks;

namespace WebBanHang1.Controllers
{
    public class LoaiSanPhamController : Controller
    {
        private readonly QuanLiHangContext _context;

        public LoaiSanPhamController(QuanLiHangContext context)
        {
            _context = context;
        }

        // GET: LoaiSanPham
        public async Task<IActionResult> Index()
        {
            var loaiSanPhams = await _context.Loais.ToListAsync();
            return View(loaiSanPhams);
        }

        // GET: LoaiSanPham/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiSanPham/Create
        // POST: LoaiSanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Loai loaiSanPham)
        {
            if (ModelState.IsValid)
            {
                _context.Loais.Add(loaiSanPham);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công
                TempData["SuccessMessage"] = "Thêm mới loại sản phẩm thành công!";

                // Chuyển hướng đến Admin/Index
                return RedirectToAction("Index", "Admin");
            }
            return View(loaiSanPham);
        }

        // GET: LoaiSanPham/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var loaiSanPham = await _context.Loais.FindAsync(id);
            if (loaiSanPham == null)
            {
                return NotFound();
            }
            return View(loaiSanPham);
        }

        // POST: LoaiSanPham/Edit/5
        // POST: LoaiSanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Loai loaiSanPham)
        {
            if (id != loaiSanPham.MaLoai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiSanPham);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật loại sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiSanPhamExists(loaiSanPham.MaLoai))
                    {
                        return NotFound();
                    }
                    throw;
                }
                // Admin/Index
                return RedirectToAction("Index", "Admin");
            }
            return View(loaiSanPham);
        }
        // GET: LoaiSanPham/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var loaiSanPham = await _context.Loais.FindAsync(id);
            if (loaiSanPham == null)
            {
                return NotFound();
            }
            return View(loaiSanPham);
        }

        // POST: LoaiSanPham/Delete/5
        // POST: LoaiSanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiSanPham = await _context.Loais.FindAsync(id);
            if (loaiSanPham != null)
            {
                _context.Loais.Remove(loaiSanPham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa loại sản phẩm thành công!";
            }
            // Chuyển hướng đến Admin/Index
            return RedirectToAction("Index", "Admin");
        }

        private bool LoaiSanPhamExists(int id)
        {
            return _context.Loais.Any(e => e.MaLoai == id);
        }
    }
}