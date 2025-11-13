using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang1.Models
{
    public class Promotion
    {
        [Key]
        [StringLength(50)]
        public string MaGiamGia { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal GiaTriGiam { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        [StringLength(50)]
        public string LoaiGiamGia { get; set; }

        [StringLength(200)]
        public string MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaTriToiThieu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaTriToiDa { get; set; }

        public int? SoLuongSuDung { get; set; }

        public int? SoLuongDaSuDung { get; set; }

        public bool HieuLuc { get; set; } = true;

        // Navigation properties
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
} 